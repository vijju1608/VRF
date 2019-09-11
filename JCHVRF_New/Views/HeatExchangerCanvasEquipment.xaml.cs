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
using JCHVRF.Model;
using JCHVRF_New.ViewModels;
using JCHVRF_New.Common.Helpers;
using Prism.Events;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for HeatExchangerCanvasEquipment.xaml
    /// </summary>
    public partial class HeatExchangerCanvasEquipment : UserControl
    {
        // private IEventAggregator _eventAggregator;

        public HeatExchangerCanvasEquipment()
        {
            InitializeComponent();

        }

        internal void SetHvacSystem(SystemBase newSystem)
        {
            var viewModel = (HeatExchangerCanvasEquipmentViewModel)DataContext;
            viewModel.UpdateEquipProp(newSystem);
        }

        private void btn_airflow_Click_1(object sender, RoutedEventArgs e)
        {
           
            HeatExchangerCanvasEquipmentViewModel.flgTabChanged = false;
            cmb_airflow.IsDropDownOpen = true;
        }

        private void btn_area_Click(object sender, RoutedEventArgs e)
        {
            cmb_area.IsDropDownOpen = true;
        }

        private void btn_esp_Click(object sender, RoutedEventArgs e)
        {

            cmb_esp.IsDropDownOpen = true;
        }

      

        private void cmb_airflow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HeatExchangerCanvasEquipmentViewModel.flgTabChanged = false;
        }
    }


}