using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Receipt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        public string DiscountUnit { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAfterDiscount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CashReceived { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChangeAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property with inverse property configuration
        [InverseProperty("Receipt")]
        public List<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
    }
}
