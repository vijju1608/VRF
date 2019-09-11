using JCBase.Utility;
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
    /// Interaction logic for InputPipeLengthPopup.xaml
    /// </summary>
    public partial class InputPipeLengthPopup : Window , IClosable
    {
        string ut_length = string.Empty;
        public InputPipeLengthPopup(double Length, double Elbow, double OilTrapQty,JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            SizeChanged += (o, e) =>
            {
                var r = SystemParameters.WorkArea;
                Left = r.Right - 100 - ActualWidth;
                Top = r.Bottom - 450 - ActualHeight;
            };
            ut_length = JCHVRF.Model.SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            InitializeComponent();
            InitilationPipingLength(CurrentSystem);
            ((InputPipeLengthPopupViewModel)this.DataContext).PipeLength = Length.ToString();
            ((InputPipeLengthPopupViewModel)this.DataContext).ElbowQty = Elbow.ToString("n2");//Unit.ConvertToControl(Elbow, UnitType.LENGTH_M, ut_length).ToString("n2");
            ((InputPipeLengthPopupViewModel)this.DataContext).OilTrapQty = OilTrapQty.ToString("n2"); //Unit.ConvertToControl(OilTrapQty, UnitType.LENGTH_M, ut_length).ToString("n2");

        }

        private void InitilationPipingLength(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            string regionCode = JCHVRF.Model.Project.CurrentProject.RegionCode;
            string subRegionCode = JCHVRF.Model.Project.CurrentProject.SubRegionCode;
            string series = "";
            if (CurrentSystem != null && CurrentSystem.OutdoorItem != null)
            {
                series = CurrentSystem.OutdoorItem.Series;
            }
            if (regionCode == "EU_W" ||
                regionCode == "EU_S" ||
                regionCode == "EU_E" ||
                ((subRegionCode == "LA_MMA" ||
                    subRegionCode == "LA_PERU" ||
                    subRegionCode == "LA_SC" ||
                    subRegionCode == "LA_BV") &&
                     !series.Contains("Residential")))
            {
                this.lblOilTrap.Visibility = Visibility.Collapsed;
                this.OilTrapQuantity.Visibility = Visibility.Collapsed;
            }
        }

        public void RequestClose()
        {
            this.Close();
        }

         private void FindNext_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((InputPipeLengthPopupViewModel)this.DataContext).OnNextClickInputPipeLength();
            }
        }

    }
}
