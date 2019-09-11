using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Lassalle.WPF.Flow;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace CustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:JCHVRF_New.Views"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:JCHVRF_New.Views;assembly=JCHVRF_New.Views"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class CustomControl1 : AddFlow
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
           
        }

        public CustomControl1() : base()
        {
        }


        public AddFlow AddFlow { get; set; }


        public List<Item> AddFlowItems
        {

            get { return (List<Item>)GetValue(AddFlowItemsProperty); }
            set
            {
                SetValue(AddFlowItemsProperty, value);
                //                TotalHeatExUnitInfoViewModel model = (TotalHeatExUnitInfoViewModel) DataContext;
                //                model.IsSelected = value;
            }
        }

       
        public  static readonly DependencyProperty AddFlowProperty = DependencyProperty.Register("AddFlow", typeof(AddFlow), typeof(CustomControl1),
            new PropertyMetadata(null, new PropertyChangedCallback(AddFlowChanged)));

        private static void AddFlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty AddFlowItemsProperty =
            DependencyProperty.Register("AddFlowItems", typeof(List<Item>), typeof(CustomControl1),
                new PropertyMetadata(null, new PropertyChangedCallback(AddFlowItemsChanged)));

        private static void AddFlowItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //TODO.
           // Lassalle.WPF.Flow.AddFlow addFlow;
        }

    }
}
