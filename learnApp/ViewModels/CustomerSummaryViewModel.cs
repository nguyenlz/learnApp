using learnApp.Models;

namespace learnApp.ViewModels
{
    public class CustomerSummaryViewModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public List<Order> Orders { get; set; } = new();
        public List<CustomerPayment> CustomerPayments { get; set; } = new();
    }
}
