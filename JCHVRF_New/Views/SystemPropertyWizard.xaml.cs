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
using System.Windows.Shapes;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for System.xaml
    /// </summary>
    public partial class SystemPropertyWizard : Window
    {
        public SystemPropertyWizard()
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
            DependencyProperty.Register("HvacSystem", typeof(SystemBase), typeof(SystemPropertyWizard),
                new PropertyMetadata(null, new PropertyChangedCallback(IsSelectedChanged)));



        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemPropertyWizard systemTab = (SystemPropertyWizard)d;
            SystemPropertyWizardViewModel viewModel = (SystemPropertyWizardViewModel)systemTab.DataContext;
            viewModel.SelectedHvacSystem = systemTab.HvacSystem;
            systemTab.propertyInfo.HvacSystem = systemTab.HvacSystem;
            if (systemTab.HvacSystem.HvacSystemType.Equals("2"))
            {
                systemTab.heatExchangerDetailsInfo.HvacSystem = systemTab.HvacSystem;
            }
            
        }
    }
}
