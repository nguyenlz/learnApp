namespace learnApp.ViewModels.Dashboard
{
    public class SupplierDebtItem
    {
        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = "";

        public decimal DebtAmount { get; set; }
    }
}