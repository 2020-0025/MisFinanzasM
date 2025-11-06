using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserAsync(string userId)
        {
            return await _context.Notifications
                .Include(n => n.Category)
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderBy(n => n.DueDate)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotificationsByUserAsync(string userId)
        {
            return await _context.Notifications
                .Include(n => n.Category)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task GenerateNotificationsForFixedExpensesAsync()
        {
            var today = DateTime.Now.Date;
            var daysToNotify = 3; // Notificar 3 días antes

            // Obtener todas las categorías con gastos fijos activos
            var fixedExpenseCategories = await _context.Categories
                .Where(c => c.IsFixedExpense && c.DayOfMonth.HasValue)
                .ToListAsync();

            foreach (var category in fixedExpenseCategories)
            {
                // Calcular la fecha de vencimiento del mes actual
                var currentMonth = today.Month;
                var currentYear = today.Year;
                var dayOfMonth = category.DayOfMonth!.Value;

                // Ajustar si el día es mayor que los días del mes
                var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                if (dayOfMonth > daysInMonth)
                    dayOfMonth = daysInMonth;

                var dueDate = new DateTime(currentYear, currentMonth, dayOfMonth);

                // Si la fecha ya pasó este mes, calcular para el próximo mes
                if (dueDate < today)
                {
                    if (currentMonth == 12)
                    {
                        currentMonth = 1;
                        currentYear++;
                    }
                    else
                    {
                        currentMonth++;
                    }

                    daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                    if (category.DayOfMonth!.Value > daysInMonth)
                        dayOfMonth = daysInMonth;
                    else
                        dayOfMonth = category.DayOfMonth!.Value;

                    dueDate = new DateTime(currentYear, currentMonth, dayOfMonth);
                }

                // Calcular fecha de notificación (3 días antes)
                var notificationDate = dueDate.AddDays(-daysToNotify);

                // Solo generar si estamos en la fecha de notificación o después (pero antes del vencimiento)
                if (today >= notificationDate && today <= dueDate)
                {
                    // Verificar si ya existe una notificación para este mes
                    var existingNotification = await _context.Notifications
                        .FirstOrDefaultAsync(n =>
                            n.CategoryId == category.CategoryId &&
                            n.UserId == category.UserId &&
                            n.DueDate.Month == dueDate.Month &&
                            n.DueDate.Year == dueDate.Year);

                    if (existingNotification == null)
                    {
                        // Crear nueva notificación
                        var notification = new Notification
                        {
                            CategoryId = category.CategoryId,
                            UserId = category.UserId,
                            NotificationDate = today,
                            DueDate = dueDate,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Notifications.Add(notification);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CleanOldNotificationsAsync(int daysOld = 60)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);

            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

                if (notification == null)
                    return false;

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting notification: {ex.Message}");
                return false;
            }
        }
    }
}