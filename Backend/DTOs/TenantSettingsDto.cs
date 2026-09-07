using Backend.Models.Enums;

namespace Backend.DTOs
{
    public class TenantSettingsDto
    {
        public PharmacyInventoryMode InventoryMode { get; set; }
        public bool PromptPriceWhenUnset { get; set; }
        public bool MaintainStock { get; set; }
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
        public bool ShowLineDiscount { get; set; }
        public bool ShowVat { get; set; }
        public decimal VatPercent { get; set; }
        public string ShopCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTenantSettingsDto
    {
        public string? Name { get; set; }
        public bool? MaintainStock { get; set; }
        public bool? PromptPriceWhenUnset { get; set; }
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
        public bool? ShowLineDiscount { get; set; }
        public bool? ShowVat { get; set; }
        public decimal? VatPercent { get; set; }
    }
}
