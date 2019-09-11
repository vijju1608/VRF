/****************************** File Header ******************************\
File Name:	IntComparerToBooleanConverter.cs
Date Created:	2/8/2019
Description:	Matches Int Value with parameter and returns true or false based on comparison.
\*************************************************************************/

namespace JCHVRF_New.Common.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class IntComparerToBooleanConverter : MarkupExtension, IValueConverter
    {
        #region Methods
        private bool _isInverted;

        public bool IsInverted
        {
            get { return _isInverted; }
            set { _isInverted = value; }
        }

        /// <summary>
        /// The Convert
        /// </summary>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="targetType">The targetType<see cref="Type"/></param>
        /// <param name="parameter">The parameter<see cref="object"/></param>
        /// <param name="culture">The culture<see cref="CultureInfo"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value!=null && parameter !=null && value is int)
            {
                bool output = System.Convert.ToInt32(parameter) == (int)value;
                return IsInverted ? !output : output;
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
