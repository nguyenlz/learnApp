using learnApp.Models;

namespace learnApp.ViewModels
{
    public class SupplierDebtSummaryViewModel
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public List<StockImport> StockImports { get; set; } = new();
        public List<SupplierPayment> SupplierPayments { get; set; } = new();
    }
}
