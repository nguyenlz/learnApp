using learnApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace learnApp.ViewModels
{
    public class PaymentCreateViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        [ValidateNever]
        public Order Order { get; set; }
    }
}
