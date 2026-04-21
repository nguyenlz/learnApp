using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace learnApp.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }
    [DisplayName("Tên nhà cung cấp")]
    public string SupplierName { get; set; } = null!;

    [DisplayName("Số điện thoại")]
    public string? Phone { get; set; }
    [DisplayName("Địa chỉ")]
    public string? Address { get; set; }

    [DisplayName("Ngày tạo")]
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<StockImport> StockImports { get; set; } = new List<StockImport>();
}
