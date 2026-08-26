namespace Backend.Models.Enums
{
    /// <summary>
    /// Tier A: global catalog only, no stock.
    /// Tier B: optional stock per medicine.
    /// Tier C: stock with batch and expiry.
    /// </summary>
    public enum PharmacyInventoryMode
    {
        CatalogOnly = 1,
        StockTracked = 2,
        BatchExpiry = 3
    }
}
