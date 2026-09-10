using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

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

        public PharmacyInventoryMode InventoryMode { get; set; } = PharmacyInventoryMode.BatchExpiry;

        /// <summary>
        /// When true, adding a catalog medicine without a tenant price requires the cashier to enter a price.
        /// The entered price is saved on TenantMedicine and used for the receipt.
        /// </summary>
        public bool PromptPriceWhenUnset { get; set; } = true;

        /// <summary>
        /// When false, stock is not tracked or deducted for sales (catalog-style).
        /// </summary>
        public bool MaintainStock { get; set; } = true;

        [StringLength(500)]
        public string? ReceiptHeader { get; set; }

        [StringLength(500)]
        public string? ReceiptFooter { get; set; }

        /// <summary>
        /// When true, cashiers can apply a discount on individual receipt lines.
        /// </summary>
        public bool ShowLineDiscount { get; set; }

        /// <summary>
        /// When true, VAT is shown and applied in the POS journal summary.
        /// </summary>
        public bool ShowVat { get; set; }

        /// <summary>
        /// VAT percentage applied when ShowVat is enabled (e.g. 5 for 5%).
        /// </summary>
        public decimal VatPercent { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<TenantMedicine> TenantMedicines { get; set; } = new List<TenantMedicine>();
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
