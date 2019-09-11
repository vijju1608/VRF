using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.ViewModels;

namespace JCHVRF_New.Common.UI.Converters
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    //TODO we need to get rid off selectedTabIndex logic.Instead we should have properties setting visibilities based on booleans.
    public class CreateBtnVisibilty: MarkupExtension, IMultiValueConverter
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
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int systemId = System.Convert.ToInt32(WorkFlowContext.Systemid);
            var selectecTabIndex = (int)values[0];

            switch (systemId)
            {
                //VRF system type
                case 1:
                    if (selectecTabIndex == 5)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                    break;

                //Heat exchanger system type
                case 2:
                    if (selectecTabIndex == 6)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                    break;

                //Central Controller system type
                case 5:
                    return Visibility.Visible;
                    break;

                default:
                    return Visibility.Collapsed;
                    break;
            }
        }

            /// <summary>
            /// The ConvertBack
            /// </summary>
            /// <param name="value">The value<see cref="object"/></param>
            /// <param name="targetType">The targetType<see cref="Type"/></param>
            /// <param name="parameter">The parameter<see cref="object"/></param>
            /// <param name="culture">The culture<see cref="CultureInfo"/></param>
            /// <returns>The <see cref="object"/></returns>
            public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
            {
                return new object[] { Binding.DoNothing, Binding.DoNothing };
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
