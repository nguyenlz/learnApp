using System.ComponentModel.DataAnnotations;

public enum OrderStatus
{
    [Display(Name = "Chờ xác nhận")]
    Pending = 0,

    [Display(Name = "Đã xác nhận")]
    Confirmed = 1,

    [Display(Name = "Đang giao")]
    Delivering = 2,

    [Display(Name = "Hoàn thành")]
    Completed = 3,

    [Display(Name = "Đã hủy")]
    Cancelled = 4
}