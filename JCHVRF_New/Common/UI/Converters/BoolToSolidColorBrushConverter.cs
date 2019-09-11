using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace JCHVRF_New.Common.UI.Converters
{
    public class BoolToSolidColorBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Double param = System.Convert.ToDouble(parameter);
            if (value is bool)
            {
                if(param == 0)
                    return (bool)value ? Brushes.Green : Brushes.Red;
                else
                    return (bool)value ? Brushes.Black : Brushes.DarkGray;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
