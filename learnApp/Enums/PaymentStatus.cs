using System.ComponentModel.DataAnnotations;

namespace learnApp.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Chưa thanh toán")]
        Unpaid = 0,
        [Display(Name = "Thanh toán 1 phần")]
        Partial = 1,
        [Display(Name = "Đã thanh toán")]
        Paid = 2
    }
}
