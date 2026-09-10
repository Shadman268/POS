using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
    }
}
