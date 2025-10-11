using Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class ReceiptItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Receipt")]
        public int ReceiptId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // Navigation properties with foreign key configurations
        public Receipt Receipt { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
