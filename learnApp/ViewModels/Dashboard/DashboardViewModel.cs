namespace learnApp.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCustomerDebt { get; set; }
        public decimal TotalSupplierDebt { get; set; }
        public decimal CashOnHand { get; set; }
        public decimal InventoryValue { get; set; }

        // Chart
        public List<RevenueChartItem> RevenueChart { get; set; } = new();

        // Top sản phẩm
        public List<TopProductItem> TopProducts { get; set; } = new();

        // Công nợ
        public List<CustomerDebtItem> CustomerDebts { get; set; } = new();

        public List<SupplierDebtItem> SupplierDebts { get; set; } = new();
    }
}

