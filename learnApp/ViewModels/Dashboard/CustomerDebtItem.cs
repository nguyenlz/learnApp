namespace learnApp.ViewModels.Dashboard
{
    public class CustomerDebtItem
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = "";

        public string? SiteName { get; set; }

        public decimal DebtAmount { get; set; }
    }
}