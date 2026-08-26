using Backend.Data;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private const decimal EstimatedProfitMargin = 0.26m;
        private const decimal EstimatedPurchaseRatio = 0.54m;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(int tenantId, string period = "today")
        {
            var (periodStart, periodEnd) = GetPeriodRange(period);
            var (prevStart, prevEnd) = GetPreviousPeriodRange(periodStart, periodEnd);

            var periodReceipts = await _context.Receipts
                .Where(r => r.TenantId == tenantId && r.CreatedAt >= periodStart && r.CreatedAt < periodEnd)
                .Include(r => r.Items)
                .ToListAsync();

            var previousReceipts = await _context.Receipts
                .Where(r => r.TenantId == tenantId && r.CreatedAt >= prevStart && r.CreatedAt < prevEnd)
                .ToListAsync();

            var totalSales = SumReceiptTotals(periodReceipts);
            var previousSales = SumReceiptTotals(previousReceipts);
            var netProfit = totalSales * EstimatedProfitMargin;
            var previousProfit = previousSales * EstimatedProfitMargin;
            var totalPurchase = totalSales * EstimatedPurchaseRatio;
            var previousPurchase = previousSales * EstimatedPurchaseRatio;

            var dueReceipts = periodReceipts
                .Where(r => GetFinalAmount(r) > r.CashReceived)
                .ToList();

            var invoiceDue = dueReceipts.Sum(r => GetFinalAmount(r) - r.CashReceived);

            var salesChart = await BuildSalesChartAsync(tenantId);
            var expiringProducts = await GetExpiringProductsAsync(tenantId);
            var recentTransactions = await GetRecentTransactionsAsync(tenantId);

            return new DashboardSummaryDto
            {
                TotalSales = totalSales,
                NetProfit = netProfit,
                InvoiceDue = invoiceDue,
                PendingInvoiceCount = dueReceipts.Count,
                TotalPurchase = totalPurchase,
                SalesTrendPercent = CalculateTrend(totalSales, previousSales),
                ProfitTrendPercent = CalculateTrend(netProfit, previousProfit),
                PurchaseTrendPercent = CalculateTrend(totalPurchase, previousPurchase),
                SalesChart = salesChart,
                ExpiringProducts = expiringProducts,
                RecentTransactions = recentTransactions
            };
        }

        private async Task<List<SalesChartPointDto>> BuildSalesChartAsync(int tenantId)
        {
            var startDate = DateTime.Today.AddDays(-13);
            var endDate = DateTime.Today.AddDays(1);

            var receipts = await _context.Receipts
                .Where(r => r.TenantId == tenantId && r.CreatedAt >= startDate && r.CreatedAt < endDate)
                .ToListAsync();

            var chart = new List<SalesChartPointDto>();
            for (var day = startDate; day < DateTime.Today.AddDays(1); day = day.AddDays(1))
            {
                var nextDay = day.AddDays(1);
                var dayTotal = receipts
                    .Where(r => r.CreatedAt >= day && r.CreatedAt < nextDay)
                    .Sum(GetFinalAmount);

                chart.Add(new SalesChartPointDto
                {
                    Label = day.ToString("MMM d"),
                    Amount = dayTotal
                });
            }

            return chart;
        }

        private async Task<List<ExpiringProductDto>> GetExpiringProductsAsync(int tenantId)
        {
            var today = DateTime.Today;
            var cutoff = today.AddDays(30);

            var batches = await _context.MedicineBatches
                .AsNoTracking()
                .Include(b => b.TenantMedicine)
                    .ThenInclude(tm => tm.Medicine)
                .Where(b => b.TenantId == tenantId && b.IsActive && b.QuantityOnHand > 0 && b.ExpiryDate <= cutoff)
                .OrderBy(b => b.ExpiryDate)
                .Take(8)
                .ToListAsync();

            return batches.Select(b => new ExpiringProductDto
            {
                ProductName = b.TenantMedicine.Medicine.Name,
                BatchNumber = b.BatchNumber,
                DaysUntilExpiry = Math.Max(0, (b.ExpiryDate.Date - today).Days)
            }).ToList();
        }

        private async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int tenantId)
        {
            var receipts = await _context.Receipts
                .Where(r => r.TenantId == tenantId)
                .Include(r => r.Items)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            return receipts.Select(r => new RecentTransactionDto
            {
                InvoiceNumber = $"INV-{r.Id:D4}",
                CustomerName = r.CustomerName,
                ItemCount = r.Items.Count,
                Amount = GetFinalAmount(r),
                Status = ResolveStatus(r),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        private static string ResolveStatus(Models.Receipt receipt)
        {
            var finalAmount = GetFinalAmount(receipt);
            if (receipt.CashReceived >= finalAmount)
            {
                return "Paid";
            }

            if (receipt.CashReceived > 0)
            {
                return "Due";
            }

            return "Due";
        }

        private static decimal GetFinalAmount(Models.Receipt receipt)
        {
            return receipt.PriceAfterDiscount > 0 ? receipt.PriceAfterDiscount : receipt.Total;
        }

        private static decimal SumReceiptTotals(IEnumerable<Models.Receipt> receipts)
        {
            return receipts.Sum(GetFinalAmount);
        }

        private static decimal CalculateTrend(decimal current, decimal previous)
        {
            if (previous <= 0)
            {
                return current > 0 ? 100 : 0;
            }

            return Math.Round((current - previous) / previous * 100, 1);
        }

        private static (DateTime Start, DateTime End) GetPeriodRange(string period)
        {
            var today = DateTime.Today;
            return period.ToLowerInvariant() switch
            {
                "week" => (today.AddDays(-(int)today.DayOfWeek), today.AddDays(1)),
                "month" => (new DateTime(today.Year, today.Month, 1), today.AddDays(1)),
                "year" => (new DateTime(today.Year, 1, 1), today.AddDays(1)),
                _ => (today, today.AddDays(1))
            };
        }

        private static (DateTime Start, DateTime End) GetPreviousPeriodRange(DateTime start, DateTime end)
        {
            var span = end - start;
            return (start - span, start);
        }
    }
}
