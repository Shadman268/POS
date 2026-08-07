using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ShopCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}
