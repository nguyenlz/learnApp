using learnApp.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace learnApp.Models;

public partial class Customer
{
    public int CustomerId { get; set; }
    [DisplayName("Tên khách hàng")]
    public string CustomerName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }
    [DisplayName("Loại khách hàng")]
    public CustomerType Type { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<CustomerPayment> CustomerPayments { get; set; } = new List<CustomerPayment>();
}
