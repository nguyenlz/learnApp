using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace learnApp.Models;

public partial class Product
{
    public int ProductId { get; set; }
    [DisplayName("Tên sản phẩm")]
    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public int SupplierId { get; set; }
    [DisplayName("Đơn vị tính")]
    public string? Unit { get; set; }
    [DisplayName("Giá bán")]
    public decimal? Price { get; set; }
    [DisplayName("Số lượng tồn kho")]
    public int? StockQuantity { get; set; }
    [ValidateNever]
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<StockImportDetail> StockImportDetails { get; set; } = new List<StockImportDetail>();
    [ValidateNever]
    public virtual Supplier Supplier { get; set; } = null!;
}
