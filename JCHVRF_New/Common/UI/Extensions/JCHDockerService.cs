using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JCHVRF_New.Common.UI.Extensions
{
    public class JCHDockerService : DependencyObject
    {
        public static double GetFloatingHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(FloatingHeightProperty);
        }

        public static void SetFloatingHeight(DependencyObject obj, double value)
        {
            obj.SetValue(FloatingHeightProperty, value);
        }

        // Using a DependencyProperty as the backing store for FloatingHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloatingHeightProperty =
            DependencyProperty.RegisterAttached("FloatingHeight", typeof(double), typeof(JCHDockerService), new PropertyMetadata(Double.NaN));



        public static double GetFloatingLeft(DependencyObject obj)
        {
            return (double)obj.GetValue(FloatingLeftProperty);
        }

        public static void SetFloatingLeft(DependencyObject obj, double value)
        {
            obj.SetValue(FloatingLeftProperty, value);
        }

        // Using a DependencyProperty as the backing store for FloatingLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloatingLeftProperty =
            DependencyProperty.RegisterAttached("FloatingLeft", typeof(double), typeof(JCHDockerService), new PropertyMetadata(Double.NaN));



        public static double GetFloatingTop(DependencyObject obj)
        {
            return (double)obj.GetValue(FloatingTopProperty);
        }

        public static void SetFloatingTop(DependencyObject obj, double value)
        {
            obj.SetValue(FloatingTopProperty, value);
        }

        // Using a DependencyProperty as the backing store for FloatingTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloatingTopProperty =
            DependencyProperty.RegisterAttached("FloatingTop", typeof(double), typeof(JCHDockerService), new PropertyMetadata(Double.NaN));




        public static double GetFloatingWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(FloatingWidthProperty);
        }

        public static void SetFloatingWidth(DependencyObject obj, double value)
        {
            obj.SetValue(FloatingWidthProperty, value);
        }

        // Using a DependencyProperty as the backing store for FloatingWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FloatingWidthProperty =
            DependencyProperty.RegisterAttached("FloatingWidth", typeof(double), typeof(JCHDockerService), new PropertyMetadata(Double.NaN));




        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(TitleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(JCHDockerService), new PropertyMetadata(string.Empty));

    }
}