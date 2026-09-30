using System;
using System.Globalization;
using System.Windows.Data;

namespace TeachFlex.Converters
{
    public class NavigationSelectionConverter :
        IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (values.Length < 2)
            {
                return false;
            }

            string buttonKey =
                values[0]?.ToString()
                ?? string.Empty;

            string currentPageKey =
                values[1]?.ToString()
                ?? string.Empty;

            return buttonKey.Equals(
                currentPageKey,
                StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}