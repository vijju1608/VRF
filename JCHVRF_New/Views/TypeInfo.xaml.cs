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
    public partial class TypeInfo : UserControl
    {

        public bool IsSelected
        {

            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);
                //                TotalHeatExUnitInfoViewModel model = (TotalHeatExUnitInfoViewModel) DataContext;
                //                model.IsSelected = value;
            }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TypeInfo),
                new PropertyMetadata(false, new PropertyChangedCallback(IsSelectedChanged)));


        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TypeInfo unitInfo = (TypeInfo)d;
            TypeInfoViewModel unitInfoViewModel = (TypeInfoViewModel)unitInfo.DataContext;
            unitInfoViewModel.IsSelected = unitInfo.IsSelected;
        }


        public TypeInfo()
        {
            InitializeComponent();
        }
    }
}
