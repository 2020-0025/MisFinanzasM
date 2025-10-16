using Microsoft.AspNetCore.Identity;

namespace MisFinanzas.Domain.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<ExpenseIncome> Transactions { get; set; } = new List<ExpenseIncome>();
        public virtual ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    }

}
