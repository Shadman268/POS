using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    /// <summary>
    /// Global medicine catalog (~20k rows). Platform-owned, no TenantId.
    /// Uploaded once via CSV import.
    /// </summary>
    public class Medicine
    {
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? GenericName { get; set; }

        [StringLength(100)]
        public string? Strength { get; set; }

        [StringLength(100)]
        public string? DosageForm { get; set; }

        [StringLength(100)]
        public string Category { get; set; } = "Medicine";

        [StringLength(100)]
        public string Brand { get; set; } = "General";

        [StringLength(50)]
        public string Unit { get; set; } = "Tablet";

        [StringLength(50)]
        public string? Barcode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<TenantMedicine> TenantMedicines { get; set; } = new List<TenantMedicine>();
    }
}
