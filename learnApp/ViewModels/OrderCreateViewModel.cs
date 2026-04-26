using learnApp.Models;

namespace learnApp.ViewModels
{
    public class OrderCreateViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
