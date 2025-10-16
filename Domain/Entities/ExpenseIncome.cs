using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.Entities
{
    public class ExpenseIncome
    {
        [Key]
        public int Id { get; set; }

        // Relación con usuario
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Relación con categoría
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Información del gasto/ingreso
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public TransactionType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
