using System.ComponentModel.DataAnnotations;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.DTOs
{
    public class ExpenseIncomeDto
    {

        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryTitle { get; set; }
        public string? CategoryIcon { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public TransactionType Type { get; set; }
        public string TypeDisplay => Type == TransactionType.Income ? "Ingreso" : "Gasto";

        //PROPIEDAD CALCULADA (Computed Property)
        public string FormattedAmount => Amount.ToString("C2");  // RD$1,234.56
    }
}
