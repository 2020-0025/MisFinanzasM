﻿using Microsoft.EntityFrameworkCore;
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
            var items = await _context.ExpensesIncomes
                .Where(ei => ei.UserId == userId && ei.Type == TransactionType.Income)
                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        public async Task<decimal> GetTotalGastosByUserAsync(string userId)
        {
            var items = await _context.ExpensesIncomes
                .Where(ei => ei.UserId == userId && ei.Type == TransactionType.Expense)
                .ToListAsync();

            return items.Sum(ei => ei.Amount);
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

            var items = await _context.ExpensesIncomes
                .Where(ei => ei.UserId == userId &&
                            ei.Type == TransactionType.Income &&
                            ei.Date >= primerDiaMes &&
                            ei.Date <= ultimoDiaMes)
                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        public async Task<decimal> GetGastosMesActualAsync(string userId)
        {
            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            var items = await _context.ExpensesIncomes
                .Where(ei => ei.UserId == userId &&
                            ei.Type == TransactionType.Expense &&
                            ei.Date >= primerDiaMes &&
                            ei.Date <= ultimoDiaMes)
                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        // ========== MÉTODOS PARA DASHBOARD ==========

        public async Task<Dictionary<DateTime, decimal>> GetDailyIncomesAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var items = await _context.ExpensesIncomes
                .Where(e => e.UserId == userId &&
                            e.Type == TransactionType.Income &&
                            e.Date >= startDate &&
                            e.Date <= endDate)
                .ToListAsync();

            return items
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyExpensesAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var items = await _context.ExpensesIncomes
                .Where(e => e.UserId == userId &&
                            e.Type == TransactionType.Expense &&
                            e.Date >= startDate &&
                            e.Date <= endDate)
                .ToListAsync();

            return items
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<List<ExpenseIncomeDto>> GetRecentTransactionsAsync(string userId, int count)
        {
            return await _context.ExpensesIncomes
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .Take(count)
                .Select(e => new ExpenseIncomeDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    CategoryId = e.CategoryId,
                    CategoryTitle = e.Category.Title,
                    CategoryIcon = e.Category.Icon,
                    Amount = e.Amount,
                    Date = e.Date,
                    Description = e.Description,
                    Type = e.Type
                })
                .ToListAsync();
        }

        public async Task<(decimal Ingresos, decimal Gastos)> GetTotalsByMonthAsync(string userId, int month, int year)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var items = await _context.ExpensesIncomes
                .Where(e => e.UserId == userId &&
                            e.Date >= firstDay &&
                            e.Date <= lastDay)
                .ToListAsync();

            var ingresos = items.Where(e => e.Type == TransactionType.Income).Sum(e => e.Amount);
            var gastos = items.Where(e => e.Type == TransactionType.Expense).Sum(e => e.Amount);

            return (ingresos, gastos);
        }
    }
}
    

