using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace learnApp.Models
{
    public class Site
    {
        public int SiteId { get; set; }
        [Display(Name = "Tên công trình")]
        public string Name { get; set; }

        public int CustomerId { get; set; }
        [ValidateNever]
        public Customer Customer { get; set; }
        [ValidateNever]
        public ICollection<Order> Orders { get; set; }
    }
}
