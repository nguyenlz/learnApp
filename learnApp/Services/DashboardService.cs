using learnApp.Enums;
using learnApp.Models;
using learnApp.ViewModels.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace learnApp.Services
{
    public class DashboardService
    {
        private readonly VlxdContext _context;

        public DashboardService(VlxdContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardData()
        {
            var vm = new DashboardViewModel();

            // =========================
            // DOANH THU
            // =========================

            vm.TotalSales = await _context.Orders
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

            vm.TotalRevenue = await _context.CustomerPayments
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            // =========================
            // CÔNG NỢ KHÁCH
            // =========================

            vm.TotalCustomerDebt = await _context.Orders
                .SumAsync(x => (decimal?)(x.TotalAmount - x.PaidAmount)) ?? 0;

            // =========================
            // CÔNG NỢ NCC
            // =========================

            vm.TotalSupplierDebt = await _context.StockImports
                .SumAsync(x => (decimal?)x.DebtAmount) ?? 0;

            // =========================
            // TỒN KHO
            // =========================

            vm.InventoryValue = await _context.Products
                .SumAsync(x =>
                    (x.StockQuantity ?? 0) *
                    (x.Price ?? 0));

            // =========================
            // TIỀN MẶT
            // =========================

            var customerPayments = await _context.CustomerPayments
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            var supplierPayments = await _context.SupplierPayments
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            vm.CashOnHand = customerPayments - supplierPayments;

            // =========================
            // LỢI NHUẬN
            // =========================

            var revenue = await _context.CustomerPayments
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            var cost = await _context.StockImportDetails
                .SumAsync(x =>
                    (x.Quantity ?? 0) *
                    (x.ImportPrice ?? 0));

            vm.TotalProfit = revenue - cost;

            // =========================
            // TOP SẢN PHẨM
            // =========================

            vm.TopProducts = await _context.OrderDetails
                .GroupBy(x => x.Product.ProductName)
                .Select(g => new TopProductItem
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            // =========================
            // KHÁCH NỢ
            // =========================

            vm.CustomerDebts = await _context.Orders
                .Where(x => x.PaymentStatus != PaymentStatus.Paid)
                .Select(x => new CustomerDebtItem
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.CustomerName,
                    SiteName = x.Site != null ? x.Site.Name : null,
                    DebtAmount = x.TotalAmount - x.PaidAmount
                })
                .OrderByDescending(x => x.DebtAmount)
                .Take(10)
                .ToListAsync();

            // =========================
            // NCC NỢ
            // =========================

            vm.SupplierDebts = await _context.StockImports
                .Where(x => x.DebtAmount > 0)
                .Select(x => new SupplierDebtItem
                {
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier.SupplierName,
                    DebtAmount = x.DebtAmount
                })
                .OrderByDescending(x => x.DebtAmount)
                .Take(10)
                .ToListAsync();

            // =========================
            // CHART 7 NGÀY
            // =========================

            var today = DateTime.Today;

            var chartData = await _context.Orders
                .Where(x => x.OrderDate >= today.AddDays(-6))
                .GroupBy(x => x.OrderDate!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            vm.RevenueChart = chartData
                .Select(x => new RevenueChartItem
                {
                    Label = x.Date.ToString("dd/MM"),
                    Revenue = x.Revenue,
                    Profit = x.Revenue * 0.2m
                })
                .ToList();

            return vm;
        }
    }
}