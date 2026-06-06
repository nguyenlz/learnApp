using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace learnApp.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }

    public int ProductId { get; set; }
    [Display(Name = "Số lượng")]
    public decimal Quantity { get; set; }
    [Display(Name = "Đơn giá")]
    public decimal UnitPrice { get; set; }
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    [Display(Name = "Đai?")]
    public bool IsSteelProcessing { get; set; }
    [ValidateNever]
    public virtual Order Order { get; set; } = null!;
    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
