
using System.Globalization;
using System.Windows.Data;

namespace NC.SalaryCalculator.ValueConverters
{
    /// <summary>
    /// TextBox绑定了float值，但当没有输入的时候错误提示：未能转换值""
    /// </summary>
    public class FloatValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !float.TryParse(value.ToString(), out _))
            {
                return 0.00f;
            }
            return value;
        }
    }
}
