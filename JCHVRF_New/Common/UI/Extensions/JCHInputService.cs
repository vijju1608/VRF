using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JCHVRF_New.Common.UI.Extensions
{
    public class JCHInputService: DependencyObject
    {
        public enum Mask
        {
            None,
            Alphabetic,
            AlphaNumeric,
            Numeric,
            Decimal
        }

        public static Mask GetInputMask(DependencyObject obj)
        {
            return (Mask)obj.GetValue(InputMaskProperty);
        }

        public static void SetInputMask(DependencyObject obj, Mask value)
        {
            obj.SetValue(InputMaskProperty, value);
        }

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsFocused.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(JCHInputService), new PropertyMetadata(false,OnIsFocusedChanged));

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d as UIElement != null && GetIsFocused(d))
            {
                (d as UIElement).Focus();
            }
        }

        // Using a DependencyProperty as the backing store for InputMask.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputMaskProperty =
            DependencyProperty.RegisterAttached("InputMask", typeof(Mask), typeof(JCHUIService), new PropertyMetadata(Mask.None, OnInputMaskChanged));

        private static void OnInputMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox)
            {
                var tb = d as TextBox;
                tb.TextChanged += (o, q) => {
                    Mask mask = GetInputMask(d);
                    if (mask == Mask.Decimal && string.IsNullOrEmpty(tb.Text))
                    {
                        tb.Text = "0";
                    }
                };
                tb.PreviewTextInput += (obj, args) =>
                {
                    Mask mask = GetInputMask(d);
                    if (mask == Mask.Decimal)
                    {
                        double val = 0;
                        args.Handled = !Double.TryParse((obj as TextBox).Text+args.Text, out val);
                    }
                    else
                    { 
                        string regex = string.Empty;
                        switch (mask)
                        {
                            case Mask.Alphabetic:
                                regex = "[^A-Za-z]+";
                                break;
                            case Mask.AlphaNumeric:
                                regex = "[^A-Za-z0-9]+";
                                break;
                            case Mask.Numeric:
                                regex = "[^0-9]+";
                                break;
                        }
                        args.Handled = Regex.IsMatch(args.Text, regex);
                    }
                };
                DataObject.AddPastingHandler(tb, (obj, args) =>
                {
                    bool isText = args.SourceDataObject.GetDataPresent(DataFormats.Text, true);
                    if (!isText) return;

                    var newText = args.SourceDataObject.GetData(DataFormats.Text) as string;
                    Mask mask = GetInputMask(d);
                    string regex = string.Empty;
                    switch (mask)
                    {
                        case Mask.Alphabetic:
                            regex = "[^A-Za-z]+";
                            break;
                        case Mask.AlphaNumeric:
                            regex = "[^A-Za-z0-9]+";
                            break;
                        case Mask.Numeric:
                            regex = "[^0-9]+";
                            break;
                    }

                    if (Regex.IsMatch(newText.Trim(), regex, RegexOptions.IgnoreCase))
                    {
                        args.CancelCommand();
                    }
                });
            }
        }
    }
}
