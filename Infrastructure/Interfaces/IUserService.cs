using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IUserService
    {
        // Para Admin: ver todos los usuarios con toda la información
        Task<List<UserDetailDto>> GetAllUsersAsync();
        Task<UserDetailDto?> GetUserByIdAsync(string id);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ToggleUserStatusAsync(string id);

        // Estadísticas
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetInactiveUsersCountAsync();
    }
}