using Backend.Models.Enums;

namespace Backend.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int TenantMedicineId { get; set; }

        public int? MedicineBatchId { get; set; }

        public StockMovementType MovementType { get; set; }

        public int QuantityDelta { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string ReferenceType { get; set; } = "Receipt";

        public int? ReferenceId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public TenantMedicine TenantMedicine { get; set; } = null!;
        public MedicineBatch? MedicineBatch { get; set; }
    }
}
