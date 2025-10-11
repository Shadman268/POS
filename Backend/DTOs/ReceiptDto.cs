using Backend.Models;

namespace Backend.DTOs
{
    public class ReceiptDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string DiscountUnit { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public decimal CashReceived { get; set; }
        public decimal ChangeAmount { get; set; }
        public List<ReceiptItemDto> Items { get; set; } = new List<ReceiptItemDto>();
    }
}
