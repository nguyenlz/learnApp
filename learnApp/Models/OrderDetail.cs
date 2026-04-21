using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace learnApp.Models;

public partial class OrderDetail
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }
    [Display(Name = "Số lượng")]
    public decimal Quantity { get; set; }
    [Display(Name = "Đơn giá")]
    public decimal UnitPrice { get; set; }

    [ValidateNever]
    public virtual Order Order { get; set; } = null!;
    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
