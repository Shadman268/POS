using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.Models
{
    public class TenantCatalogOption
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public Tenant Tenant { get; set; } = null!;

        public CatalogOptionType OptionType { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
