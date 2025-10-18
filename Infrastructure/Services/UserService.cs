using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<UserDetailDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(u => new UserDetailDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                Password = u.PasswordHash!, // ⚠️ Texto plano en BD
                FullName = u.FullName,
                UserRole = u.UserRole,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin
            }).ToList();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            return new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Password = user.PasswordHash!,
                FullName = user.FullName,
                UserRole = user.UserRole,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.UserRole == "Admin")
                return false; // No eliminar admin

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.UserRole == "Admin")
                return false; // No desactivar admin

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }

        public async Task<int> GetInactiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => !u.IsActive);
        }
    }
}