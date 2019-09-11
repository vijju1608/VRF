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
    public class ListContainsDateConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is IList<DateTime> && values[1] is DateTime)
            {
                IEnumerable<DateTime> list = values[0] as IEnumerable<DateTime>;
                return list.Any(a=>a.Date == ((DateTime)values[1]).Date);
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
