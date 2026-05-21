using learnApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace learnApp.ViewModels
{
    public class PaymentCreateViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? SiteId { get; set; }
        public string? SiteName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Note { get; set; } = string.Empty;
        [ValidateNever]
        public Customer Customer { get; set; }
        [ValidateNever]
        public Site Site { get; set; }
    }
}
