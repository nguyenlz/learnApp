using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace learnApp.Models;

public partial class OrderDetail
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    [ValidateNever]
    public virtual Order Order { get; set; } = null!;
    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
