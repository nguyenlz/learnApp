using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace learnApp.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }
    [DisplayName("Tên nhân viên")]
    public string EmployeeName { get; set; } = null!;
    [DisplayName("Số điện thoại")]
    public string? Phone { get; set; }

    [DisplayName("Chức vụ")]
    public string? Position { get; set; }
    
    [DisplayName("Ngày tuyển dụng")]
    public DateOnly? HireDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<StockImport> StockImports { get; set; } = new List<StockImport>();
}
