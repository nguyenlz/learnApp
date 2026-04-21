using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using learnApp.Enums;

namespace learnApp.Models;

public partial class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    [Display(Name = "Ngày đặt hàng")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime? OrderDate { get; set; }
    [Display(Name = "Tổng tiền")]
    public decimal TotalAmount { get; set; }
    [Display(Name = "Trạng thái đơn hàng")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    [Display(Name = "Trạng thái thanh toán")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    // (optional) công trình
    public int? SiteId { get; set; }
    [ValidateNever]
    public virtual Customer Customer { get; set; } = null!;
    [ValidateNever]
    public virtual Employee Employee { get; set; } = null!;
    [ValidateNever]
    public virtual Site Site { get; set; }
    [ValidateNever]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    [ValidateNever]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public void UpdatePaymentStatus()
    {
        var paid = Payments.Sum(p => p.Amount);

        if (paid == 0)
            PaymentStatus = PaymentStatus.Unpaid;
        else if (paid < TotalAmount)
            PaymentStatus = PaymentStatus.Partial;
        else
            PaymentStatus = PaymentStatus.Paid;
    }
}
