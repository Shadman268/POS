using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    /// <summary>
    /// Tenant-specific overlay for a global medicine: price, tracking flag.
    /// Created on first price set or when tenant explicitly tracks a medicine.
    /// </summary>
    public class TenantMedicine
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int MedicineId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        public bool IsStockTracked { get; set; }

        [StringLength(50)]
        public string? LocalSku { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
        public Medicine Medicine { get; set; } = null!;
        public MedicineStock? Stock { get; set; }
        public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
    }
}
