using learnApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace learnApp.ViewModels
{
    public class SupplierPaymentCreateViewModel
    {
        public int SupplierId { get; set; }
        [Display(Name = "Số tiền thanh toán")]
        public decimal Amount { get; set; }
        [ValidateNever]
        public Supplier Supplier { get; set; }
    }
}
