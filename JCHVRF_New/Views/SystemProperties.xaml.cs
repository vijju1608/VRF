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
namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for SystemProperties.xaml
    /// </summary>
    public partial class SystemProperties : UserControl
    {
        public SystemProperties(SystemPropertiesViewmodel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
