namespace Backend.Models
{
    /// <summary>
    /// Aggregate stock quantity for tier B/C when IsStockTracked is true.
    /// </summary>
    public class MedicineStock
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int TenantMedicineId { get; set; }

        public int QuantityOnHand { get; set; }

        public int? ReorderLevel { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
        public TenantMedicine TenantMedicine { get; set; } = null!;
    }
}
