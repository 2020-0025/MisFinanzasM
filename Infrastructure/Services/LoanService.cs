using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public LoanService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ========== CONSULTAS BÁSICAS ==========

        public async Task<List<Loan>> GetAllByUserAsync(string userId)
        {
            return await _context.Loans
                .Include(l => l.Category)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<List<Loan>> GetActiveByUserAsync(string userId)
        {
            return await _context.Loans
                .Include(l => l.Category)
                .Where(l => l.UserId == userId && l.IsActive)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<Loan?> GetByIdAsync(int loanId, string userId)
        {
            return await _context.Loans
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);
        }

        // ========== CRUD ==========

        public async Task<(bool Success, string? Error, Loan? Loan)> CreateAsync(Loan loan, string userId, bool createReminder = false)
        {
            try
            {
                // Validar que no exista préstamo con el mismo título
                if (await ExistsLoanWithTitleAsync(loan.Title, userId))
                {
                    return (false, "Ya existe un préstamo con ese título.", null);
                }

                // Validar datos
                if (loan.PrincipalAmount <= 0)
                    return (false, "El monto del préstamo debe ser mayor a cero.", null);

                if (loan.InstallmentAmount <= 0)
                    return (false, "La cuota mensual debe ser mayor a cero.", null);

                if (loan.NumberOfInstallments < 1)
                    return (false, "El número de cuotas debe ser al menos 1.", null);

                if (loan.DueDay < 1 || loan.DueDay > 31)
                    return (false, "El día de pago debe estar entre 1 y 31.", null);

                // 1. Crear categoría automáticamente para este préstamo
                var category = new Category
                {
                    UserId = userId,
                    Title = $"{loan.Icon} {loan.Title}",
                    Icon = loan.Icon,
                    Type = TransactionType.Expense,
                    IsFixedExpense = createReminder, // Si se activa recordatorio, es gasto fijo
                    DayOfMonth = createReminder ? loan.DueDay : null,
                    EstimatedAmount = createReminder ? loan.InstallmentAmount : null
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // 2. Asignar categoría al préstamo
                loan.UserId = userId;
                loan.CategoryId = category.CategoryId;
                loan.IsActive = true;
                loan.InstallmentsPaid = 0;

                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                // 3. Las notificaciones se generarán automáticamente por el background service
                // No generamos notificación inmediata para evitar conflictos de DbContext
                if (createReminder)
                {
                    Console.WriteLine($"✅ Recordatorio configurado para préstamo {loan.LoanId}. El background service generará las notificaciones.");
                }

                return (true, null, loan);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear préstamo: {ex.Message}");
                return (false, "Error al crear el préstamo.", null);
            }
        }

        public async Task<bool> UpdateAsync(int loanId, Loan updatedLoan, string userId)
        {
            try
            {
                var loan = await _context.Loans
                    .Include(l => l.Category)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)
                    return false;

                // Validar que no exista otro préstamo con el mismo título
                if (await ExistsLoanWithTitleAsync(updatedLoan.Title, userId, loanId))
                    return false;

                // Actualizar datos del préstamo
                loan.Title = updatedLoan.Title;
                loan.Description = updatedLoan.Description;
                loan.PrincipalAmount = updatedLoan.PrincipalAmount;
                loan.InstallmentAmount = updatedLoan.InstallmentAmount;
                loan.NumberOfInstallments = updatedLoan.NumberOfInstallments;
                loan.DueDay = updatedLoan.DueDay;
                loan.StartDate = updatedLoan.StartDate;
                loan.Icon = updatedLoan.Icon;

                // Actualizar categoría asociada
                if (loan.Category != null)
                {
                    loan.Category.Title = $"{updatedLoan.Icon} {updatedLoan.Title}";
                    loan.Category.Icon = updatedLoan.Icon;
                    loan.Category.EstimatedAmount = updatedLoan.InstallmentAmount;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar préstamo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int loanId, string userId, bool deleteHistory = false)
        {
            try
            {
                var loan = await _context.Loans
                    .Include(l => l.Category)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)
                    return false;

                if (deleteHistory)
                {
                    // Opción A: Eliminación completa
                    // Eliminar todos los ExpenseIncomes relacionados a esta categoría
                    var relatedExpenses = await _context.ExpensesIncomes
                        .Where(e => e.CategoryId == loan.CategoryId && e.UserId == userId)
                        .ToListAsync();

                    _context.ExpensesIncomes.RemoveRange(relatedExpenses);

                    // Eliminar la categoría
                    if (loan.Category != null)
                    {
                        _context.Categories.Remove(loan.Category);
                    }

                    // Eliminar el préstamo
                    _context.Loans.Remove(loan);
                }
                else
                {
                    // Opción B: Marcar como inactivo (preservar historial)
                    loan.IsActive = false;

                    // Marcar categoría como inactiva (si Category tiene IsActive)
                    // Por ahora solo desactivamos el préstamo
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al eliminar préstamo: {ex.Message}");
                return false;
            }
        }

        // ========== OPERACIONES ESPECÍFICAS DE PRÉSTAMOS ==========

        public async Task<bool> RegisterPaymentAsync(int loanId, string userId)
        {
            try
            {
                var loan = await GetByIdAsync(loanId, userId);

                if (loan == null || !loan.IsActive)
                    return false;

                // Validar que no haya pagado todas las cuotas
                if (loan.InstallmentsPaid >= loan.NumberOfInstallments)
                    return false;

                // 1. Incrementar cuotas pagadas
                loan.InstallmentsPaid++;

                // 2. Crear ExpenseIncome (registro del pago)
                var payment = new ExpenseIncome
                {
                    UserId = userId,
                    CategoryId = loan.CategoryId,
                    Type = TransactionType.Expense,
                    Amount = loan.InstallmentAmount,
                    Description = $"Cuota {loan.InstallmentsPaid}/{loan.NumberOfInstallments} - {loan.Title}",
                    Date = DateTime.Now
                };

                _context.ExpensesIncomes.Add(payment);

                // 3. Si completó todas las cuotas, marcar como completado
                if (loan.InstallmentsPaid >= loan.NumberOfInstallments)
                {
                    loan.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al registrar pago: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAsCompletedAsync(int loanId, string userId)
        {
            try
            {
                var loan = await GetByIdAsync(loanId, userId);

                if (loan == null)
                    return false;

                loan.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al marcar como completado: {ex.Message}");
                return false;
            }
        }

        // ========== VALIDACIONES ==========

        public async Task<bool> ExistsLoanWithTitleAsync(string title, string userId, int? excludeLoanId = null)
        {
            var query = _context.Loans.Where(l => l.UserId == userId && l.Title == title);

            if (excludeLoanId.HasValue)
            {
                query = query.Where(l => l.LoanId != excludeLoanId.Value);
            }

            return await query.AnyAsync();
        }

        // ========== RESÚMENES Y ESTADÍSTICAS ==========

        public async Task<decimal> GetTotalBorrowedAsync(string userId)
        {
            var activeLoans = await GetActiveByUserAsync(userId);
            return activeLoans.Sum(l => l.PrincipalAmount);
        }

        public async Task<decimal> GetTotalToPayAsync(string userId)
        {
            var activeLoans = await GetActiveByUserAsync(userId);
            return activeLoans.Sum(l => l.TotalToPay);
        }

        public async Task<decimal> GetTotalPaidAsync(string userId)
        {
            var activeLoans = await GetActiveByUserAsync(userId);
            return activeLoans.Sum(l => l.TotalPaid);
        }

        public async Task<decimal> GetTotalRemainingAsync(string userId)
        {
            var activeLoans = await GetActiveByUserAsync(userId);
            return activeLoans.Sum(l => (l.TotalToPay - l.TotalPaid));
        }

        public async Task<decimal> GetMonthlyPaymentsTotalAsync(string userId)
        {
            var activeLoans = await GetActiveByUserAsync(userId);
            return activeLoans.Sum(l => l.InstallmentAmount);
        }

        public async Task<decimal> GetAverageInterestRateAsync(string userId)
        {
            var activeLoans = await GetActiveByUserAsync(userId);

            if (!activeLoans.Any())
                return 0;

            return activeLoans.Average(l => l.ApproximateInterestRate);
        }

        // ========== PARA DASHBOARD ==========

        public async Task<List<Loan>> GetLoansWithUpcomingPaymentsAsync(string userId, int daysAhead = 7)
        {
            var activeLoans = await GetActiveByUserAsync(userId);
            var today = DateTime.Now;
            var futureDate = today.AddDays(daysAhead);

            return activeLoans
                .Where(l => l.NextPaymentDate >= today && l.NextPaymentDate <= futureDate)
                .OrderBy(l => l.NextPaymentDate)
                .ToList();
        }
    }
}