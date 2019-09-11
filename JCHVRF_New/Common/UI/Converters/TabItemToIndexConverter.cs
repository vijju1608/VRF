/****************************** File Header ******************************\
File Name:	TabItemToIndexConverter.cs
Date Created:	2/5/2019
Description:	Gets the Index of a TabItem by finding its ItemContainer.
\*************************************************************************/

namespace JCHVRF_New.Common.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class TabItemToIndexConverter : MarkupExtension,IValueConverter
    {
        #region Methods

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
            TabItem tabItem = value as TabItem;
            return ItemsControl.ItemsControlFromItemContainer(tabItem)
                .ItemContainerGenerator.Items.IndexOf(tabItem);
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
            return DependencyProperty.UnsetValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #endregion
    }
}
