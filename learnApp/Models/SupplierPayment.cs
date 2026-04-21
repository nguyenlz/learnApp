using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace learnApp.Models
{
    public class SupplierPayment
    {
        public int SupplierPaymentId { get; set; }
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        [ValidateNever]
        public Supplier Supplier{ get; set; }
    }
}
