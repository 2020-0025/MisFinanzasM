using System.ComponentModel.DataAnnotations;

namespace MisFinanzas.Domain.DTOs
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal AssignedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        [Required] // ⭐ SIEMPRE requerido
        public int CategoryId { get; set; }

        public string CategoryTitle { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = "📁";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Propiedades calculadas
        public decimal AvailableAmount { get; set; }
        public decimal UsedPercentage { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsNearLimit { get; set; }

        // Display
        public string MonthYearDisplay => $"{GetMonthName(Month)} {Year}";

        public string StatusDisplay => IsOverBudget ? "🔴 Excedido" :
                                       IsNearLimit ? "🟡 Cerca del límite" :
                                       "🟢 En control";

        private static string GetMonthName(int month) => month switch
        {
            1 => "Enero",
            2 => "Febrero",
            3 => "Marzo",
            4 => "Abril",
            5 => "Mayo",
            6 => "Junio",
            7 => "Julio",
            8 => "Agosto",
            9 => "Septiembre",
            10 => "Octubre",
            11 => "Noviembre",
            12 => "Diciembre",
            _ => "Desconocido"
        };
    }
}