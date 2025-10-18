using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.DTOs
{
    public class FinancialGoalDto
    {
        public int GoalId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; } = "🎯";
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public GoalStatus Status { get; set; }
        public DateTime? CompletedAt { get; set; }  // ⭐ Agregar si no existe

        // Propiedades calculadas
        public decimal RemainingAmount => TargetAmount - CurrentAmount;
        public decimal ProgressPercentage => TargetAmount > 0
            ? (CurrentAmount / TargetAmount) * 100
            : 0;
        public int DaysRemaining => (TargetDate - DateTime.Now).Days;
        public bool IsOverdue => DateTime.Now > TargetDate && Status == GoalStatus.InProgress;

        // ⭐ Agregar esta propiedad
        public string StatusDisplay => Status switch
        {
            GoalStatus.InProgress => "🟢 En Progreso",
            GoalStatus.Completed => "✅ Completada",
            GoalStatus.Cancelled => "❌ Cancelada",
            _ => "?"
        };
    }
}