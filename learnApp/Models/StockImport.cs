using System;
using System.Collections.Generic;

namespace learnApp.Models;

public partial class StockImport
{
    public int ImportId { get; set; }

    public int SupplierId { get; set; }

    public DateTime? ImportDate { get; set; }

    public int? EmployeeId { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<StockImportDetail> StockImportDetails { get; set; } = new List<StockImportDetail>();

    public virtual Supplier Supplier { get; set; } = null!;
}
