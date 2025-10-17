using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllByUserAsync(string userId);
        Task<List<CategoryDto>> GetByUserAndTypeAsync(string userId, TransactionType type);
        Task<CategoryDto?> GetByIdAsync(int id, string userId);
        Task<CategoryDto> CreateAsync(CategoryDto dto, string userId);
        Task<bool> UpdateAsync(int id, CategoryDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}