using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace learnApp.Models;

public partial class Category
{
    public int CategoryId { get; set; }
    [DisplayName("Tên danh mục")]
    public string CategoryName { get; set; } = null!;
    [DisplayName("Mô tả")]
    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
