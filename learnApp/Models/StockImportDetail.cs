using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace learnApp.Models;

public partial class StockImportDetail
{
    public int ImportDetailId { get; set; }   // khóa chính mới
    public int ImportId { get; set; }

    public int ProductId { get; set; }
    [DisplayName("Số lượng")]
    public decimal? Quantity { get; set; }
    [DisplayName("Giá nhập")]
    public decimal? ImportPrice { get; set; }
    [ValidateNever]
    public virtual StockImport Import { get; set; } = null!;
    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
