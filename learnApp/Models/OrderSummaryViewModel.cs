namespace learnApp.Models
{
    public class OrderSummaryViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<Order> Orders { get; set; } = new();

        public decimal TotalAmount { get; set; }
    }
}
