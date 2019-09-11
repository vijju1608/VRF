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
using JCHVRF.Entity;
using JCHVRF.Model;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for IDUEquipmentProperties.xaml
    /// </summary>
    public partial class IDUEquipmentProperties : UserControl
    {
        public event EventHandler<Object> UpdateSelectedEquipmentProperty;

        protected virtual void OnSelectEquipmentProperty(Object data)
        {
            if (UpdateSelectedEquipmentProperty != null) UpdateSelectedEquipmentProperty(this, data);
        }

        private IDUEquipmentPropertiesViewModel _iduEquipmentPropertiesViewModel;
        public IDUEquipmentProperties(IDUEquipmentPropertiesViewModel vm)
        {
            InitializeComponent();
            _iduEquipmentPropertiesViewModel = vm;
            this.DataContext = vm;
        }

        private void onAddAccessories_Click(object sender, RoutedEventArgs e)
        {
            AddAccessoriesTemplate addAccessories = new Views.AddAccessoriesTemplate();
            addAccessories.DataContext = _iduEquipmentPropertiesViewModel.AddAccessoryViewModel;
            //addAccessories.ShowDialog();
        }

        private void txtBindPropertyChange_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            var obj = (TextBox)sender;
            if (!String.IsNullOrEmpty(obj.Text.Trim()))
                OnSelectEquipmentProperty("Equipment");
        }

        private void TextBlock_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {           
            var obj = (TextBlock)sender;
            if (!String.IsNullOrEmpty(obj.Text.Trim()))
            {
                OnSelectEquipmentProperty("True");
            }
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            string Type = "None";
            var obj = (System.Windows.Controls.ComboBox)sender;
            if (obj.Name == "cmbType")
            {
                OnSelectEquipmentProperty("Type");
            }
            else if (obj.Name == "cmbModel")
            {
                OnSelectEquipmentProperty("Model");
            }
            else if (obj.Name == "cmdFanSpeed")
            {
                OnSelectEquipmentProperty("FanSpeed");
            }
            else if (obj.Name == "cmdFloor")
            {
                OnSelectEquipmentProperty("Floor");
            }
            else if (obj.Name == "cmdFloor")
            {
                OnSelectEquipmentProperty("Floor");
            }
            else
                OnSelectEquipmentProperty(Type);
        }
    }
}
