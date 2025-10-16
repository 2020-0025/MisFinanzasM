using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.Entities
{
    public class Budget
    {
        //[Key]
        public int Id { get; set; }

        // Relación con usuario
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Información del presupuesto
        public string Name { get; set; } = string.Empty;
        public decimal AssignedAmount { get; set; } // Monto asignado
        public decimal SpentAmount { get; set; } = 0; // Monto gastado

        // Periodo
        public int Month { get; set; } // 1-12
        public int Year { get; set; }

        // Relación opcional con categoría
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Metadatos
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Propiedades calculadas (no se mapean a la BD)
        public decimal AvailableAmount => AssignedAmount - SpentAmount;
        public decimal UsedPercentage => AssignedAmount > 0
            ? (SpentAmount / AssignedAmount) * 100
            : 0;
        public bool IsOverBudget => SpentAmount > AssignedAmount;
    }
}