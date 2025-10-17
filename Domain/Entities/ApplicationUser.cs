using Microsoft.AspNetCore.Identity;

namespace MisFinanzas.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Informaci�n personal
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // Sistema de roles simplificado (adem�s de Identity Roles)
        public string UserRole { get; set; } = "User"; // "Admin" o "User"

        // Navegaci�n a entidades de negocio
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<ExpenseIncome> ExpensesIncomes { get; set; } = new List<ExpenseIncome>();
        public ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}