using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class ExpenseIncomeService : IExpenseIncomeService
    {
        private readonly ApplicationDbContext _context;

        public ExpenseIncomeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExpenseIncomeDto>> GetAllByUserAsync(string userId)
        {
            return await _context.ExpensesIncomes
                .Include(gi => gi.Category)
                .Where(gi => gi.UserId == userId)
                .OrderByDescending(gi => gi.Date)
                .Select(gi => new ExpenseIncomeDto
                {
                    Id = gi.Id,
                    UserId = gi.UserId,
                    CategoryId = gi.CategoryId,
                    CategoryTitle = gi.Category.Title,
                    CategoryIcon = gi.Category.Icon,
                    Amount = gi.Amount,
                    Date = gi.Date,
                    Description = gi.Description,
                    Type = gi.Type
                })
                .ToListAsync();
        }

        public async Task<List<ExpenseIncomeDto>> GetByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.ExpensesIncomes
                .Include(gi => gi.Category)
                .Where(gi => gi.UserId == userId && gi.Date >= startDate && gi.Date <= endDate)
                .OrderByDescending(gi => gi.Date)
                .Select(gi => new ExpenseIncomeDto
                {
                    Id = gi.Id,
                    UserId = gi.UserId,
                    CategoryId = gi.CategoryId,
                    CategoryTitle = gi.Category.Title,
                    CategoryIcon = gi.Category.Icon,
                    Amount = gi.Amount,
                    Date = gi.Date,
                    Description = gi.Description,
                    Type = gi.Type
                })
                .ToListAsync();
        }

        public async Task<List<ExpenseIncomeDto>> GetByUserAndTypeAsync(string userId, TransactionType type)
        {
            return await _context.ExpensesIncomes
                .Include(gi => gi.Category)
                .Where(gi => gi.UserId == userId && gi.Type == type)
                .OrderByDescending(gi => gi.Date)
                .Select(gi => new ExpenseIncomeDto
                {
                    Id = gi.Id,
                    UserId = gi.UserId,
                    CategoryId = gi.CategoryId,
                    CategoryTitle = gi.Category.Title,
                    CategoryIcon = gi.Category.Icon,
                    Amount = gi.Amount,
                    Date = gi.Date,
                    Description = gi.Description,
                    Type = gi.Type
                })
                .ToListAsync();
        }

        public async Task<ExpenseIncomeDto?> GetByIdAsync(int id, string userId)
        {
            var gastoIngreso = await _context.ExpensesIncomes
                .Include(gi => gi.Category)
                .FirstOrDefaultAsync(gi => gi.Id == id && gi.UserId == userId);

            if (gastoIngreso == null)
                return null;

            return new ExpenseIncomeDto
            {
                Id = gastoIngreso.Id,
                UserId = gastoIngreso.UserId,
                CategoryId = gastoIngreso.CategoryId,
                CategoryTitle = gastoIngreso.Category.Title,
                CategoryIcon = gastoIngreso.Category.Icon,
                Amount = gastoIngreso.Amount,
                Date = gastoIngreso.Date,
                Description = gastoIngreso.Description,
                Type = gastoIngreso.Type
            };
        }

        public async Task<ExpenseIncomeDto> CreateAsync(ExpenseIncomeDto dto, string userId)
        {
            var ExpenseIncome = new ExpenseIncome
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExpensesIncomes.Add(ExpenseIncome);
            await _context.SaveChangesAsync();

            dto.Id = ExpenseIncome.Id;
            dto.UserId = userId;

            return dto;
        }

        public async Task<bool> UpdateAsync(int id, ExpenseIncomeDto dto, string userId)
        {
            var gastoIngreso = await _context.ExpensesIncomes
                .FirstOrDefaultAsync(gi => gi.Id == id && gi.UserId == userId);

            if (gastoIngreso == null)
                return false;

            gastoIngreso.CategoryId = dto.CategoryId;
            gastoIngreso.Amount = dto.Amount;
            gastoIngreso.Date = dto.Date;
            gastoIngreso.Description = dto.Description;
            gastoIngreso.Type = dto.Type;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var gastoIngreso = await _context.ExpensesIncomes
                .FirstOrDefaultAsync(gi => gi.Id == id && gi.UserId == userId);

            if (gastoIngreso == null)
                return false;

            _context.ExpensesIncomes.Remove(gastoIngreso);
            await _context.SaveChangesAsync();
            return true;
        }

        // Cálculos
        public async Task<decimal> GetTotalIngresosByUserAsync(string userId)
        {
            return await _context.ExpensesIncomes
                .Where(gi => gi.UserId == userId && gi.Type == TransactionType.Income)
                .SumAsync(gi => gi.Amount);
        }

        public async Task<decimal> GetTotalGastosByUserAsync(string userId)
        {
            return await _context.ExpensesIncomes
                .Where(gi => gi.UserId == userId && gi.Type == TransactionType.Expense)
                .SumAsync(gi => gi.Amount);
        }

        public async Task<decimal> GetBalanceByUserAsync(string userId)
        {
            var ingresos = await GetTotalIngresosByUserAsync(userId);
            var gastos = await GetTotalGastosByUserAsync(userId);
            return ingresos - gastos;
        }

        public async Task<decimal> GetIngresosMesActualAsync(string userId)
        {
            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            return await _context.ExpensesIncomes
                .Where(gi => gi.UserId == userId &&
                            gi.Type == TransactionType.Income &&
                            gi.Date >= primerDiaMes &&
                            gi.Date <= ultimoDiaMes)
                .SumAsync(gi => gi.Amount);
        }

        public async Task<decimal> GetGastosMesActualAsync(string userId)
        {
            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            return await _context.ExpensesIncomes
                .Where(gi => gi.UserId == userId &&
                            gi.Type == TransactionType.Expense &&
                            gi.Date >= primerDiaMes &&
                            gi.Date <= ultimoDiaMes)
                .SumAsync(gi => gi.Amount);
        }
    }
}
