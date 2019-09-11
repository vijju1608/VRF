using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace JCHVRF_New.Common.UI.Converters
{
    public class DoubleGreaterThanParameterConverter : MarkupExtension, IValueConverter
    {
        #region Methods

        /// <summary>
        /// The Converter
        /// </summary>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="targetType">The targetType<see cref="Type"/></param>
        /// <param name="parameter">The parameter<see cref="object"/></param>
        /// <param name="culture">The culture<see cref="CultureInfo"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null && parameter != null && value is double)
        {
            double param = System.Convert.ToDouble(parameter);
            return (double)value > param ;
        }
        return Binding.DoNothing;
    }

    /// <summary>
    /// The ConvertBack
    /// </summary>
    /// <param name="value">The value<see cref="object"/></param>
    /// <param name="targetType">The targetType<see cref="Type"/></param>
    /// <param name="parameter">The parameter<see cref="object"/></param>
    /// <param name="culture">The culture<see cref="CultureInfo"/></param>
    /// <returns>The <see cref="object"/></returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    /// <summary>
    /// The ProvideValue
    /// </summary>
    /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/></param>
    /// <returns>The <see cref="object"/></returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    #endregion
}
}
