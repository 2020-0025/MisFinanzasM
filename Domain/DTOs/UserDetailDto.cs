namespace MisFinanzas.Domain.DTOs
{
    public class UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; //Texto plano (solo para Admin)
        public string? FullName { get; set; }
        public string UserRole { get; set; } = "User";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        // Información adicional
        public string StatusDisplay => IsActive ? "Activo" : "Inactivo";
        public string RoleDisplay => UserRole == "Admin" ? "👨‍💼 Administrador" : "👤 Usuario";
        public string LastLoginDisplay => LastLogin.HasValue
            ? LastLogin.Value.ToString("dd/MM/yyyy HH:mm")
            : "Nunca";
    }
}