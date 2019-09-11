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
using JCHVRF_New.ViewModels;
using JCHVRF.Model;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for TotalHeatExchDetailsInfo.xaml
    /// </summary>
    public partial class TotalHeatExchDetailsInfo : UserControl
    {
        public TotalHeatExchDetailsInfo()
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
            DependencyProperty.Register("HvacSystem", typeof(SystemBase), typeof(TotalHeatExchDetailsInfo),
                new PropertyMetadata(null, new PropertyChangedCallback(IsSelectedChanged)));



        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TotalHeatExchDetailsInfo systemTab = (TotalHeatExchDetailsInfo)d;
            TotalHeatExchDetailsInfoViewModel viewModel = (TotalHeatExchDetailsInfoViewModel)systemTab.DataContext;


            viewModel.HvacSystem = systemTab.HvacSystem;

        }
    }
}
