namespace MisFinanzas.Domain.DTOs
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal AssignedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal AvailableAmount { get; set; }
        public decimal UsedPercentage { get; set; }
        public bool IsOverBudget { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryTitle { get; set; }
        public string? CategoryIcon { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Helper para mostrar el periodo
        public string PeriodDisplay => $"{GetMonthName(Month)} {Year}";

        private string GetMonthName(int month)
        {
            var months = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo",
                               "Junio", "Julio", "Agosto", "Septiembre",
                               "Octubre", "Noviembre", "Diciembre" };
            return months[month];
        }
    }
}