using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace learnApp.Models;

public partial class StockImport
{
    public int ImportId { get; set; }

    public int SupplierId { get; set; }
    [Display(Name = "Ngày nhập hàng")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? ImportDate { get; set; }
    [Display(Name = "Tổng tiền")]
    public decimal TotalAmount { get; set; } = 0;
    [DisplayName("Số tiền đã trả")]
    public decimal PaidAmount { get; set; } = 0;
    [DisplayName("Số tiền còn nợ")]
    public decimal DebtAmount { get; set; } = 0;

    public int? EmployeeId { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<StockImportDetail> StockImportDetails { get; set; } = new List<StockImportDetail>();

    public virtual Supplier Supplier { get; set; } = null!;
}
