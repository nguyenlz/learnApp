namespace learnApp.Models
{
    public class OrderCreateViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
