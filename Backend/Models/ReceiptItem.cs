using Backend.Models.Enums;
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

        public SaleLineType LineType { get; set; } = SaleLineType.Medicine;

        public int? MedicineId { get; set; }

        public int? TenantMedicineId { get; set; }

        public int? MedicineBatchId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? GenericName { get; set; }

        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineDiscount { get; set; }

        public Receipt Receipt { get; set; } = null!;
        public Medicine? Medicine { get; set; }
        public TenantMedicine? TenantMedicine { get; set; }
        public MedicineBatch? MedicineBatch { get; set; }
    }
}
