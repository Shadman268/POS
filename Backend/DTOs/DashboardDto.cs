namespace Backend.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalSales { get; set; }
        public decimal NetProfit { get; set; }
        public decimal InvoiceDue { get; set; }
        public int PendingInvoiceCount { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal SalesTrendPercent { get; set; }
        public decimal ProfitTrendPercent { get; set; }
        public decimal PurchaseTrendPercent { get; set; }
        public List<SalesChartPointDto> SalesChart { get; set; } = new();
        public List<ExpiringProductDto> ExpiringProducts { get; set; } = new();
        public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class SalesChartPointDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ExpiringProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public int DaysUntilExpiry { get; set; }
    }

    public class RecentTransactionDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
