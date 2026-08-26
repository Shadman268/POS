using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Batch-level inventory for tier C pharmacies.
    /// </summary>
    public class MedicineBatch
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int TenantMedicineId { get; set; }

        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public int QuantityOnHand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public Tenant Tenant { get; set; } = null!;
        public TenantMedicine TenantMedicine { get; set; } = null!;
    }
}
