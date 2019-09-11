using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for FloorTab.xaml
    /// </summary>
    public partial class FloorTab : UserControl
    {
        public FloorTab()
        {
            InitializeComponent();            
        }

        //private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    var regex = new Regex(@"[^a-zA-Z0-9\s]");
        //    if ((e. < Key.A) || (e.Key > Key.Z))
        //        e.Handled = true;
        //}

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsLetterOrDigit);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrWhiteSpace((sender as TextBox).Text) && e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
