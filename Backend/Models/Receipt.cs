using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.Enums;

namespace Backend.Models
{
    public class Receipt
    {
        [Key]
        public int Id { get; set; }

        public ReceiptType ReceiptType { get; set; } = ReceiptType.Sale;

        public int? OriginalReceiptId { get; set; }

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

        public int TenantId { get; set; }

        public int? BranchId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Tenant Tenant { get; set; } = null!;

        public Receipt? OriginalReceipt { get; set; }

        // Navigation property with inverse property configuration
        [InverseProperty("Receipt")]
        public List<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
    }
}
