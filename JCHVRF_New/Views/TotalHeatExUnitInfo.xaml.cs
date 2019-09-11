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

//
// Created by Ramya - 28/3/2019
// This User control includes Room, Floor and other factor informations related to Heat Exchanger unit.
//
namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for TotalHeatExUnitInfo.xaml
    /// </summary>
    public partial class TotalHeatExUnitInfo : UserControl
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
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TotalHeatExUnitInfo),
                new PropertyMetadata(false, new PropertyChangedCallback(IsSelectedChanged)));


        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TotalHeatExUnitInfo unitInfo = (TotalHeatExUnitInfo)d;
            TotalHeatExUnitInfoViewModel unitInfoViewModel = (TotalHeatExUnitInfoViewModel)unitInfo.DataContext;
            unitInfoViewModel.IsSelected = unitInfo.IsSelected;
        }


        public TotalHeatExUnitInfo()
        {
            InitializeComponent();
        }


    }
}
