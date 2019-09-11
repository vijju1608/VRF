using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace JCHVRF_New.Common.UI.Converters
{
    public class DivideByParameterConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                parameter = 1;

            if (value is double)
            {
                return (double)value / System.Convert.ToDouble(parameter);
            }
            else if (value is int)
            {
                return (int)value / System.Convert.ToInt32(parameter);
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                parameter = 1;

            if (value is double)
            {
                return (double)value * System.Convert.ToDouble(parameter);
            }
            else if (value is int)
            {
                return (int)value * System.Convert.ToInt32(parameter);
            }
            else
                return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
