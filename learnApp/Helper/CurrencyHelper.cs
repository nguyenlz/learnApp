using System.Globalization;

namespace learnApp.Helper
{
    public static class CurrencyHelper
    {
        private static readonly CultureInfo ViCulture = new CultureInfo("vi-VN");

        public static string ToVnd(decimal amount)
        {
            return string.Format(ViCulture, "{0:C}", amount);
        }

        public static string ToVnd(this decimal? amount)
        {
            return amount.HasValue
                ? string.Format(ViCulture, "{0:#,0} ₫", amount.Value)
                : "0 ₫";
        }
    }
}
