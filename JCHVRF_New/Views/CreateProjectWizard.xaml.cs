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

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for CreateProjectWizard.xaml
    /// </summary>
    public partial class CreateProjectWizard : UserControl
    {
        public CreateProjectWizard()
        {
            InitializeComponent();
        }

        private void MainTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           TabControl ctrl = (TabControl) sender;

        }

    }
}
