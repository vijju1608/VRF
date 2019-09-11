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

namespace JCHVRF_New.Common.Controls
{
    /// <summary>
    /// Interaction logic for JCHUpDown.xaml
    /// </summary>
    public partial class JCHUpDown : UserControl
    {
        public JCHUpDown()
        {
            InitializeComponent();
        }

        public Decimal? Value
        {
            get { return (Decimal?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Decimal?), typeof(JCHUpDown));
        
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(JCHUpDown), new PropertyMetadata(""));



        public string UnitText
        {
            get { return (string)GetValue(UnitTextProperty); }
            set { SetValue(UnitTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitTextProperty =
            DependencyProperty.Register("UnitText", typeof(string), typeof(JCHUpDown), new PropertyMetadata(""));



        public string ErrorText
        {
            get { return (string)GetValue(ErrorTextProperty); }
            set { SetValue(ErrorTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorTextProperty =
            DependencyProperty.Register("ErrorText", typeof(string), typeof(JCHUpDown), new PropertyMetadata("", OnErrorTextChanged));

        private static void OnErrorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           (d as JCHUpDown).txtError.Visibility = (e.NewValue != null && e.NewValue.ToString().Length > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public event RoutedEventHandler UpDownLostFocus;

        private void DecimalUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            UpDownLostFocus?.Invoke(sender, e);
        }
    }
}
