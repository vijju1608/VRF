/****************************** File Header ******************************\
File Name:	TabItemToIsLastTabConverter.cs
Date Created:	29/8/2019
Description:	Gets the Index of a TabItem and checks whether it is last Visible tab.
               Has been made MultiValueConverter So that Selected Index can be passed as a Second value
               So that converter refreshes values on tab change as well.               
\*************************************************************************/

namespace JCHVRF_New.Common.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Linq;

    public class TabItemToIsLastTabConverter : MarkupExtension, IMultiValueConverter
    {
        #region Methods
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TabItem tabItem = values[0] as TabItem;
            int indexOfLastVisible = 0;
            foreach (TabItem item in ItemsControl.ItemsControlFromItemContainer(tabItem).Items)
            {
                if (item.Visibility == Visibility.Visible)
                    indexOfLastVisible = ItemsControl.ItemsControlFromItemContainer(tabItem)
                .ItemContainerGenerator.Items.IndexOf(item);
            };
            int index = ItemsControl.ItemsControlFromItemContainer(tabItem)
                .ItemContainerGenerator.Items.IndexOf(tabItem);
            return indexOfLastVisible == index;
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #endregion
    }
}
