using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace learnApp.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }
    [ValidateNever]
    public virtual Customer Customer { get; set; } = null!;
    [ValidateNever]
    public virtual Employee Employee { get; set; } = null!;
    [ValidateNever]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
