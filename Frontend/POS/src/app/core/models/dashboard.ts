export type DashboardPeriod = 'today' | 'week' | 'month' | 'year';

export interface DashboardSummary {
  totalSales: number;
  netProfit: number;
  invoiceDue: number;
  pendingInvoiceCount: number;
  totalPurchase: number;
  salesTrendPercent: number;
  profitTrendPercent: number;
  purchaseTrendPercent: number;
  salesChart: SalesChartPoint[];
  expiringProducts: ExpiringProduct[];
  recentTransactions: RecentTransaction[];
}

export interface SalesChartPoint {
  label: string;
  amount: number;
}

export interface ExpiringProduct {
  productName: string;
  batchNumber: string;
  daysUntilExpiry: number;
}

export interface RecentTransaction {
  invoiceNumber: string;
  customerName: string;
  itemCount: number;
  amount: number;
  status: 'Paid' | 'Due' | 'Refunded';
  createdAt: string;
}
