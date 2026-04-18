using System.ComponentModel.DataAnnotations;

namespace learnApp.Enums
{
    public enum CustomerType
    {
        [Display(Name = "Bán lẻ")]
        Retail = 0,
        [Display(Name = "Nhà thầu")]
        Contractor = 1
    }
}
