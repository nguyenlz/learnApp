namespace learnApp.Models
{
    public class Site
    {
        public int SiteId { get; set; }
        public string Name { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
