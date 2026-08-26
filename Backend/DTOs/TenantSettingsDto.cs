using Backend.Models.Enums;

namespace Backend.DTOs
{
    public class TenantSettingsDto
    {
        public PharmacyInventoryMode InventoryMode { get; set; }
        public bool PromptPriceWhenUnset { get; set; }
        public string ShopCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTenantSettingsDto
    {
        public PharmacyInventoryMode InventoryMode { get; set; }
        public bool PromptPriceWhenUnset { get; set; }
    }
}
