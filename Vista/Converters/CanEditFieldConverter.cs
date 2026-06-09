using System.Globalization;
using System.Windows.Data;
using WPF_Test.Models;
using WPF_Test.Vista.UserControls;

namespace WPF_Test.Vista.Converters
{
    public class CanEditFieldConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = DynamicDataGrid
            // values[1] = OrdenRow
            // values[2] = PropertyName (parameter)

            if (values.Length < 2 || values[0] is not DynamicDataGrid grid || values[1] is not OrdenRow row)
                return true;

            string propertyName = parameter?.ToString() ?? "";
            return grid.CanEditField(row, propertyName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
