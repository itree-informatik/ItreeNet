using System.Globalization;

namespace ItreeNet.Services
{
    public static class Culture
    {
        public static void SetCulture()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("de-CH");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("de-CH");

            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.NumberGroupSeparator = "’";
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.NumberGroupSeparator = "’";
            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.NumberDecimalDigits = 1;
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.NumberDecimalDigits = 1;
            // percents
            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.PercentDecimalDigits = 1;
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.PercentDecimalDigits = 1;
            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.PercentNegativePattern = 1;
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.PercentPositivePattern = 1;
            // decimal
            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.CurrencyDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencyGroupSeparator = "’";
            CultureInfo.DefaultThreadCurrentUICulture.NumberFormat.CurrencyGroupSeparator = "’";
        }
    }
}
