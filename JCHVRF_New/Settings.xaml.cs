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
using System.Windows.Shapes;

namespace JCHVRF_New
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Window_Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.WindowState = WindowState.Maximized;
        }

        private void Location_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Location obj = new Location();
            MainFrame.Content = obj.Content;
            //MainDesignerPage obj = new MainDesignerPage();
            //MainFrame.Content = obj.Content;

        }
    }
}
