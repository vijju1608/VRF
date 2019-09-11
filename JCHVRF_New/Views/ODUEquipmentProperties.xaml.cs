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
    /// Interaction logic for ODUEquipmentProperties.xaml
    /// </summary>
    public partial class ODUEquipmentProperties : UserControl
    {
        public event EventHandler<Object> UpdateSelectedOutdoorProperty;

        private ODUEquipmentPropertiesViewModel _oduEquipmentPropertiesViewModel;
        public ODUEquipmentProperties(ODUEquipmentPropertiesViewModel vm)
        {
            InitializeComponent();
            _oduEquipmentPropertiesViewModel = vm;
            this.DataContext = vm;
        }

        protected virtual void OnSelectEquipmentProperty(Object data)
        {
            if (UpdateSelectedOutdoorProperty != null) UpdateSelectedOutdoorProperty(this, data);
        }
        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
#pragma warning disable CS0219 // The variable 'Type' is assigned but its value is never used
            string Type = "None";
#pragma warning restore CS0219 // The variable 'Type' is assigned but its value is never used
            var obj = (System.Windows.Controls.ComboBox)sender;
            if (obj.Name == "cmbType")
            {
                OnSelectEquipmentProperty("Type");
            }
            else if (obj.Name == "cmbPower")
            {
                OnSelectEquipmentProperty("Power");
            }
            else if (obj.Name == "cmbOutdoor")
            {
                OnSelectEquipmentProperty("Outdoor");
            }
            else if (obj.Name == "cmbMaxRatio")
            {
                OnSelectEquipmentProperty("MaxRatio");
            }            
               
        }

        private void txtBindPropertyChange_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            var obj = (TextBox)sender;
            if (!String.IsNullOrEmpty(obj.Text.Trim()))
                OnSelectEquipmentProperty("Equipment");
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var obj = (CheckBox)sender;
            if (obj.Name == "chkAutoManual")
            {
                if(obj.IsChecked==true)               
                    cmbOutdoor.IsEnabled = false;
                else
                    cmbOutdoor.IsEnabled = true;

                OnSelectEquipmentProperty("AutoManual");
            }
            if (obj.Name == "chkIndoreFreshAir")
                OnSelectEquipmentProperty("IndoreFreshAir");
        }
    }
}
