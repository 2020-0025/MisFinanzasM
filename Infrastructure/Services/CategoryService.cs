using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllByUserAsync(string userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Title)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    UserId = c.UserId,
                    Title = c.Title,
                    Icon = c.Icon,
                    Type = c.Type
                })
                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetByUserAndTypeAsync(string userId, TransactionType type)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId && c.Type == type)
                .OrderBy(c => c.Title)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    UserId = c.UserId,
                    Title = c.Title,
                    Icon = c.Icon,
                    Type = c.Type
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, string userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)
                return null;

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                UserId = category.UserId,
                Title = category.Title,
                Icon = category.Icon,
                Type = category.Type
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto dto, string userId)
        {
            var category = new Category
            {
                UserId = userId,
                Title = dto.Title,
                Icon = dto.Icon,
                Type = dto.Type
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            dto.CategoryId = category.CategoryId;
            dto.UserId = userId;

            return dto;
        }

        public async Task<bool> UpdateAsync(int id, CategoryDto dto, string userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)
                return false;

            category.Title = dto.Title;
            category.Icon = dto.Icon;
            category.Type = dto.Type;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetRelatedTransactionsCountAsync(int categoryId, string userId)
        {
            return await _context.ExpensesIncomes
                .CountAsync(ei => ei.CategoryId == categoryId && ei.UserId == userId);
        }
    }
}