using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace JCHVRF_New.Common.UI.Converters
{
    public class EmptyStringConverter : MarkupExtension , IValueConverter
    {
        public object Convert(object value, Type targetType,
                          object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? value : value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            object objcheck ="";
            objcheck=string.IsNullOrEmpty(value as string) ? 0 : value;
            if (CharCheck(value.ToString()))
            {
                objcheck=string.IsNullOrEmpty(value as string) ? 0 : value;
                objcheck = 0;
            }
            return objcheck;
            // throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        private bool CharCheck(string value)
        {

            bool IsValid =  Regex.IsMatch(value, @"[^0-9.]+");           
            return IsValid;
            //Regex regex = new Regex("[^0-9]+");
            //e.Handled = regex.IsMatch(value);
        }
    }
}
