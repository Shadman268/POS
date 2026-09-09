namespace Backend.DTOs
{
    public class ReceiptItemDto
    {
        public int Id { get; set; }

        /// <summary>
        /// POS item id: TenantMedicine.Id or CatalogIdOffset + MedicineId.
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public decimal LineDiscount { get; set; }
        public int? MedicineBatchId { get; set; }
        public int SoldQuantity { get; set; }
        public int AlreadyReturnedQuantity { get; set; }
        public int ReturnableQuantity { get; set; }
    }
}
