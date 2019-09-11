using JCHVRF.Model;
using JCHVRF_New.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for TypeInfo.xaml
    /// </summary>
    public partial class PropertyInfo : UserControl
    {
        public PropertyInfo()
        {
            InitializeComponent();
        }


        public SystemBase HvacSystem
        {

            get { return (SystemBase)GetValue(SystemProperty); }
            set
            {
                SetValue(SystemProperty, value);
                //                TotalHeatExUnitInfoViewModel model = (TotalHeatExUnitInfoViewModel) DataContext;
                //                model.IsSelected = value;
            }
        }

        public static readonly DependencyProperty SystemProperty =
            DependencyProperty.Register("HvacSystem", typeof(SystemBase), typeof(PropertyInfo),
                new PropertyMetadata(null, new PropertyChangedCallback(IsSelectedChanged)));

       

        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyInfo systemTab = (PropertyInfo)d;
            PropertyInfoViewModel viewModel = (PropertyInfoViewModel)systemTab.DataContext;


            viewModel.HvacSystem = systemTab.HvacSystem;
            
        }
    }
}
