using JCHVRF_New.ViewModels;
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
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for IndoorUnitInfoView.xaml
    /// </summary>
    public partial class IndoorUnitInfoView : UserControl
    {
        public IndoorUnitInfoView()
        {
            InitializeComponent();
            //_winService = winService;
        }

        private IModalWindowService _winService;
        private void btnAddUnit_Click(object sender, RoutedEventArgs e)
        {

            var viewCtxt = DataContext as IndoorUnitInfoViewModel;
            if (viewCtxt != null)
                viewCtxt.SubscribeAddAll();
            _winService.ShowView(ViewKeys.AddIndoorUnitView,"Add Indoor Unit View");

           
        }
    }
}
