using JCHVRF_New.Common.Contracts;
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
using System.Windows.Shapes;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for AddIndoorUnitView.xaml
    /// </summary>
    public partial class AddIndoorUnitView : UserControl
    {
        public AddIndoorUnitView()
        {
            InitializeComponent();
        }
        //public void RequestClose()
        //{
        //    this.Close();
        //}

        private void AutoSelect_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var dCtx = this.DataContext as AddIndoorUnitViewModel;
            if (dCtx != null)
                dCtx.AutoSelectTrigger();
        }
    }
}
