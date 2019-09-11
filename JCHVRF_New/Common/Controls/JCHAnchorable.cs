using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using JCHVRF_New.Common.UI.Extensions;

namespace JCHVRF_New.Common.Controls
{
    /// <summary>
    /// Class Created Extending Anchorable as Anchorable doesn't support Binding for other properties except Title.
    /// To Create a MVVM friendly way for toggling Hide/Show and Dock/Float behaviour using a simple Handle.
    /// </summary>
    public class JCHAnchorable : LayoutAnchorable
    {
        public bool IsPaneVisible
        {
            get { return (bool)GetValue(IsPaneVisibleProperty); }
            set { SetValue(IsPaneVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPaneVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPaneVisibleProperty =
            DependencyProperty.Register("IsPaneVisible", typeof(bool), typeof(JCHAnchorable), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsPaneVisibleChanged));

        private static void OnIsPaneVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);
            if (!ctrl._valueInternallySetToUpdateHandle)
            {
                if (ctrl.IsPaneVisible)
                {
                    ctrl.Show();
                    if (ctrl.IsFloating)
                    {
                        ctrl.Dock();
                    }
                }
                else
                {
                    ctrl.Hide();
                }
            }
        }

        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            _valueInternallySetToUpdateHandle = true;
            switch (propertyName)
            {
                case "IsVisible":
                    IsPaneVisible = IsVisible;
                    break;
                case "FloatingLeft":
                    FloaterLeft = FloatingLeft;
                    break;
                case "FloatingTop":
                    FloaterTop = FloatingTop;
                    break;
                case "IsSelected":
                    IsPaneSelected = IsSelected;
                    break;
                    //case "FloatingHeight":
                    //    FloaterHeight = FloatingHeight;
                    //    break;
                    //case "FloatingWidth":
                    //    FloaterWidth = FloatingWidth;
                    //break;
            }
            _valueInternallySetToUpdateHandle = false;

        }

        protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
            base.OnParentChanged(oldValue, newValue);
                Task.Run(async delegate
                {
                    await Task.Delay(100);
                    Dispatcher.Invoke((Action)(() =>
                    {
                        _valueInternallySetToUpdateHandle = true;
                        IsPaneFloating = IsFloating;
                        IsPaneFloatingMinimized = IsFloating;
                        _valueInternallySetToUpdateHandle = false;
                        if (!IsFloating)
                        {
                            FloatingHeight = FloaterHeight;
                            FloatingWidth = FloaterWidth;
                        }
                    }));
                });
            this.IsMaximized = false;
        }

        bool _valueInternallySetToUpdateHandle;


        public double FloaterHeight
        {
            get { return (double)GetValue(FloaterHeightProperty); }
            set { SetValue(FloaterHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FloaterHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloaterHeightProperty =
            DependencyProperty.Register("FloaterHeight", typeof(double), typeof(JCHAnchorable), new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFloaterHeightChanged));


        private static void OnFloaterHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);
            if (!ctrl._valueInternallySetToUpdateHandle)
                ctrl.FloatingHeight = ctrl.FloaterHeight;
        }

        public double FloaterWidth
        {
            get { return (double)GetValue(FloaterWidthProperty); }
            set { SetValue(FloaterWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FloaterWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloaterWidthProperty =
            DependencyProperty.Register("FloaterWidth", typeof(double), typeof(JCHAnchorable), new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFloaterWidthChanged));

        private static void OnFloaterWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);
            if (!ctrl._valueInternallySetToUpdateHandle)
                ctrl.FloatingWidth = ctrl.FloaterWidth;
        }

        public double FloaterLeft
        {
            get { return (double)GetValue(FloaterLeftProperty); }
            set { SetValue(FloaterLeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FloaterLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloaterLeftProperty =
            DependencyProperty.Register("FloaterLeft", typeof(double), typeof(JCHAnchorable), new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFloaterLeftChanged));

        private static void OnFloaterLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);
            if (!ctrl._valueInternallySetToUpdateHandle)
                ctrl.FloatingLeft = ctrl.FloaterLeft;
        }

        public double FloaterTop
        {
            get { return (double)GetValue(FloaterTopProperty); }
            set { SetValue(FloaterTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FloaterTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloaterTopProperty =
            DependencyProperty.Register("FloaterTop", typeof(double), typeof(JCHAnchorable), new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFloaterTopChanged));

        private static void OnFloaterTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);
            if (!ctrl._valueInternallySetToUpdateHandle)
                ctrl.FloatingTop = ctrl.FloaterTop;
        }

        public bool IsPaneFloating
        {
            get { return (bool)GetValue(IsPaneFloatingProperty); }
            set { SetValue(IsPaneFloatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPaneFloating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPaneFloatingProperty =
            DependencyProperty.Register("IsPaneFloating", typeof(bool), typeof(JCHAnchorable), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsPaneFloatingChanged));

        private static void OnIsPaneFloatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);

            if (!ctrl._valueInternallySetToUpdateHandle)
            {
                if (ctrl.IsPaneFloating && !ctrl.IsFloating)
                {
                    ctrl.Float();
                }
                else if (!ctrl.IsPaneFloating && ctrl.IsFloating)
                {
                    ctrl.Dock();
                }
            }
        }

        public bool IsPaneFloatingMinimized
        {
            get { return (bool)GetValue(IsPaneFloatingMinimizedProperty); }
            set { SetValue(IsPaneFloatingMinimizedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPaneFloatingMinimizedMinimized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPaneFloatingMinimizedProperty =
            DependencyProperty.Register("IsPaneFloatingMinimized", typeof(bool), typeof(JCHAnchorable), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsPaneFloatingMinimizedChanged));

        private static void OnIsPaneFloatingMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);

            if (!ctrl._valueInternallySetToUpdateHandle)
            {
                if (ctrl.IsPaneFloatingMinimized && !ctrl.IsFloating)
                {
                    ctrl.FloatingHeight = 10;
                    ctrl.FloatingWidth = 150;
                    ctrl.Float();
                }
                else if (!ctrl.IsPaneFloatingMinimized && ctrl.IsFloating)
                {
                    ctrl.Dock();
                }
            }
        }

        public bool IsPaneSelected
        {
            get { return (bool)GetValue(IsPaneSelectedProperty); }
            set { SetValue(IsPaneSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPaneSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPaneSelectedProperty =
            DependencyProperty.Register("IsPaneSelected", typeof(bool), typeof(JCHAnchorable), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsPaneSelectedChanged));

        private static void OnIsPaneSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (d as JCHAnchorable);
            if (!ctrl._valueInternallySetToUpdateHandle)
                ctrl.IsSelected = ctrl.IsPaneSelected;
        }
    }
}
