using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace learnApp.Models
{
    public class CustomerPayment
    {
        public int CustomerPaymentId { get; set; }
        public int CustomerId { get; set; }
        public int? SiteId { get; set; }
        [DisplayName("Đã thanh toán")]
        public decimal Amount { get; set; }
        [DisplayName("Ngày thanh toán")]
        public DateTime PaymentDate { get; set; }        
        [DisplayName("Ghi chú")]
        public string? Note { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; }
        [ValidateNever]
        public Site Site { get; set; }
    }
}