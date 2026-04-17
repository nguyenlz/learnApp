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
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

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

}
