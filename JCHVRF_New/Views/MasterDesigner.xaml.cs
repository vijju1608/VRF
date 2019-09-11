using JCHVRF.Model;
using System;
using System.Windows;
using System.Data;
using JCHVRF.DALFactory;
using System.Windows.Controls;
using JCHVRF_New.ViewModels;
using System.ComponentModel;
using System.Linq;
using JCHVRF.BLL;
using System.Collections.Generic;
using NextGenModel = JCHVRF.Model.NextGen;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Events;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Utility;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for MasterDesigner.xaml
    /// </summary>
    public partial class MasterDesigner : UserControl, INotifyPropertyChanged
    {
        ucDesignerCanvas designerCanvas = null;
        string utTemperature;
        double dbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB;
        double wbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
        double dbHeat = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
        double rhCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH;
        string namePrefix = string.Empty;
        private IEventAggregator _eventAggregator;


        //public static bool blnIsCallfromDelete = false;

        //#region NodeProperty
        //static readonly DependencyProperty NodeProperty =
        //    DependencyProperty.RegisterAttached("NodeProperty", typeof(object), typeof(object),
        //        new FrameworkPropertyMetadata(false));
        //static object GetNodeProperty(Node node)
        //{
        //    if (node == null)
        //        return false;
        //    return (object)node.GetValue(NodeProperty);
        //}
        //static void SetNodeProperty(Node node, object value)
        //{
        //    if (node != null)
        //        node.SetValue(NodeProperty, value);
        //}
        //#endregion
        //As of now we are using custom type within the scope. However Need to move it to NodeItem class to achieve seperation of concerns.
        //public class ImageData
        //{
        //    public string imagePath { get; set; }
        //    public string imageName { get; set; }
        //    public string equipmentType { get; set; }
        //    public int NodeNo { get; set; }   // Added to uniquely identify the node
        //    public string UniqName { get; set; }
        //}
#pragma warning disable CS0169 // The field 'MasterDesigner._dao' is never used
        IDataAccessObject _dao;
#pragma warning restore CS0169 // The field 'MasterDesigner._dao' is never used
#pragma warning disable CS0169 // The field 'MasterDesigner._region' is never used
        string _region;
#pragma warning restore CS0169 // The field 'MasterDesigner._region' is never used
#pragma warning disable CS0169 // The field 'MasterDesigner._brandCode' is never used
        string _brandCode;
#pragma warning restore CS0169 // The field 'MasterDesigner._brandCode' is never used
        DataTable dtFillImageType;
#pragma warning disable CS0169 // The field 'MasterDesigner.dtFullPath' is never used
        DataTable dtFullPath;
        NextGenModel.ImageData SelectedNode;

        public enum equipmentTypeButtonClick
        {
            All = 0,
            IDU = 1,
            ODU = 2,
            Pipe = 3,
            CC = 4

        }

        public equipmentTypeButtonClick enumEQSrch;
        public JCHVRF.Model.Project projectLegacy { get; set; }

        public IDUEquipmentPropertiesViewModel IDUEquipmentPropertiesViewModel
        {
            get { return iduEquipmentPropertiesViewModel; }
            set
            {
                iduEquipmentPropertiesViewModel = value;
                OnPropertyChanged("IDUEquipmentPropertiesViewModel");
            }
        }

        private IDUEquipmentPropertiesViewModel iduEquipmentPropertiesViewModel;

        private ODUEquipmentPropertiesViewModel oduEquipmentPropertiesViewModel;

        public ODUEquipmentPropertiesViewModel OduEquipmentPropertiesViewModel
        {
            get { return oduEquipmentPropertiesViewModel; }

            set
            {


                oduEquipmentPropertiesViewModel = value;
                OnPropertyChanged("ODUEquipmentPropertiesViewModel");
            }
        }


        public MasterDesigner(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            this.Loaded += Main_Loaded;
            this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            //eventAggregator.GetEvent<SetIduPropertiesOnCanvas>().Subscribe(OnSetIduPropertiesOnCanvas);
        }

        protected void UpdateSelectedOutdoorValues(object sender, Object ControlName)
        {
            if (this.SelectedNode != null)
            {
                if (ControlName != null)
                {
                    if (this.projectLegacy.CanvasODUList != null && this.projectLegacy.CanvasODUList.Count > 0)
                    {
                        var itemIndex =
                            this.projectLegacy.CanvasODUList.FindIndex(x => x.NO == this.SelectedNode.NodeNo);
                        if (itemIndex >= 0)
                        {
                            var item = this.projectLegacy.CanvasODUList.ElementAt(itemIndex);
                            this.projectLegacy.CanvasODUList.RemoveAt(itemIndex);
                            if ((string) ControlName == "Type")
                            {
                                item.SelOutdoorType = OduEquipmentPropertiesViewModel.SelectedType;
                                if (item.OutdoorItem != null)
                                    item.OutdoorItem.Type = OduEquipmentPropertiesViewModel.SelectedType;
                            }
                            else if ((string) ControlName == "Power")
                            {
                                item.Power = OduEquipmentPropertiesViewModel.SelectedPower;
                            }
                            else if ((string) ControlName == "Outdoor")
                            {
                                if (item.OutdoorItem != null)
                                {
                                    item.OutdoorItem.ModelFull = OduEquipmentPropertiesViewModel.SelectedOutdoor;
                                    //item.OutdoorItem.FullModuleName = OduEquipmentPropertiesViewModel.SelectedOutdoor;
                                }
                                else
                                {
                                    Outdoor OTD = new Outdoor();
                                    OTD.Series = item.Series;
                                    OTD.ModelFull = OduEquipmentPropertiesViewModel.SelectedOutdoor;
                                    //OTD.FullModuleName = OduEquipmentPropertiesViewModel.SelectedOutdoor;
                                    item.OutdoorItem = OTD;
                                }
                            }
                            else if ((string) ControlName == "Equipment")
                            {
                                item.Name = OduEquipmentPropertiesViewModel.OduName;
                            }
                            else if ((string) ControlName == "AutoManual")
                            {
                                item.IsAuto = OduEquipmentPropertiesViewModel.IsAuto;
                                if (OduEquipmentPropertiesViewModel.IsAuto == true)
                                {
                                    //item.DBCooling = SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB;
                                    //item.DBHeating = SystemSetting.UserSetting.defaultSetting.outdoorHeatingDB;
                                    //item.WBHeating = SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB;
                                    //item.RHHeating = SystemSetting.UserSetting.defaultSetting.outdoorHeatingRH;
                                    //item.PipeEquivalentLength = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                                    //item.PipeEquivalentLengthbuff = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                                    //item.FirstPipeLength = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
                                    //item.FirstPipeLengthbuff = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
                                    //item.HeightDiff = SystemSetting.UserSetting.pipingSetting.pipingHighDifference;
                                    //item.PipingLengthFactor = SystemSetting.UserSetting.pipingSetting.pipingCorrectionFactor;
                                    //item.PipingPositionType = SystemSetting.UserSetting.pipingSetting.pipingPositionType;
                                    //item.IWCooling = SystemSetting.UserSetting.defaultSetting.outdoorCoolingIW;
                                    //item.IWHeating = SystemSetting.UserSetting.defaultSetting.outdoorHeatingIW;
                                }
                            }
                            else if ((string) ControlName == "IndoreFreshAir")
                            {
                                if (OduEquipmentPropertiesViewModel.IsBothIndoreFreshAir == true)
                                {
                                    item.SysType = SystemType.CompositeMode;
                                }
                                else item.SysType = SystemType.OnlyIndoor;
                            }
                            else if ((string) ControlName == "MaxRatio")
                            {
                                if (OduEquipmentPropertiesViewModel.SelectedMaxRatio > 0)
                                {
                                    //if (item.SysType == SystemType.OnlyFreshAir)
                                    //    return;
                                    //if (OduEquipmentPropertiesViewModel.SelectedMaxRatio == Convert.ToInt32(item.MaxRatio * 100))
                                    //    return;
                                    //item.MaxRatio = Convert.ToDouble(OduEquipmentPropertiesViewModel.SelectedMaxRatio) / 100;

                                    item.MaxRatio =
                                        Math.Round(
                                            (Convert.ToDouble(OduEquipmentPropertiesViewModel.SelectedMaxRatio) / 100),
                                            2);
                                }
                            }

                            this.projectLegacy.CanvasODUList.Insert(itemIndex, item);
                        }
                    }
                }
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //To DO, below line will ultimately be removed, when changing to MVVM
            projectLegacy = (DataContext as MasterDesignerViewModel)?.Project;

            utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            //Clear Error log control
            _eventAggregator.GetEvent<ErrorLogVMClearAll>().Publish();
            if (this.projectLegacy.CanvasODUList == null || this.projectLegacy.CanvasODUList.Count == 0)
            {
                this.projectLegacy.CanvasODUList = new List<NextGenModel.SystemVRF>();
                if (this.projectLegacy.SystemListNextGen != null && this.projectLegacy.SystemListNextGen.Count > 0)
                {
                    this.projectLegacy.CanvasODUList.Add(this.projectLegacy.SystemListNextGen.FirstOrDefault());
                }
            }
            //IDUEquipmentPropertiesViewModel = new IDUEquipmentPropertiesViewModel(projectLegacy);
            //brdMain.Child = new IDUEquipmentProperties(IDUEquipmentPropertiesViewModel);
            //ToDo: This needs to get called on Selection of Equipment
            //ImageData data = new ImageData {
            //     equipmentType="Indoor"
            //};

            // SelectEquipmentOnCanvas("",data);

            //_eventAggregator.GetEvent<ToolBarFloatEnableSubscriber>().Subscribe(OnToolBarOptionChanged);
        }

        private string filePath = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void lnkBtnAutoPipingIcon_Click(object sender, RoutedEventArgs e)
        {
            var MDViewModel = (MasterDesignerViewModel)this.DataContext;
            var SysIndex = MDViewModel.SelectedTabIndex;
            if (SysIndex >= 0)
            {
                var Sys = (JCHVRF.Model.NextGen.SystemVRF) MDViewModel.Systems[SysIndex];
                if (Sys != null)
                {
                    Sys.IsPipingVertical = false;
                    Sys.IsPipingOK = false;
                    Sys.PipingLayoutType = JCHVRF.Model.PipingLayoutTypes.Normal;
                    Sys.IsManualPiping=false;
                    _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemTabSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<RefreshSystems>().Publish();
                    UndoRedoSetup.SetInstanceNull();
                }

            }
        }
        private void lnkverticalPiping_Click(object sender, RoutedEventArgs e)
        {
            var MDViewModel = (MasterDesignerViewModel)this.DataContext;
            var SysIndex = MDViewModel.SelectedTabIndex;
            if (SysIndex >= 0)
            {
                var Sys = (JCHVRF.Model.NextGen.SystemVRF) MDViewModel.Systems[SysIndex];
                if (Sys != null)
                {
                    Sys.IsPipingVertical = true;
                    _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemTabSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(Sys);
                }

                ;

            }
        }

        private void lnkhorizontalPiping_Click(object sender, RoutedEventArgs e)
        {
            var MDViewModel = (MasterDesignerViewModel)this.DataContext;
            var SysIndex = MDViewModel.SelectedTabIndex;
            if (SysIndex >= 0)
            {
                var Sys = (JCHVRF.Model.NextGen.SystemVRF) MDViewModel.Systems[SysIndex];
                if (Sys != null)
                {
                    Sys.IsPipingVertical = false;
                    _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemTabSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(Sys);
                }

                ;

            }
        }

        private void FindZero_Click(object sender, RoutedEventArgs e)
        {
            var MDViewModel = (MasterDesignerViewModel)this.DataContext;
            var SysIndex = MDViewModel.SelectedTabIndex;
            if (SysIndex >= 0)
            {
                var Sys = (JCHVRF.Model.NextGen.SystemVRF) MDViewModel.Systems[SysIndex];
                if (Sys != null)
                {
                    _eventAggregator.GetEvent<FindLengthZeroBtnSubscriber>().Publish(Sys);


                }

            }
        }

        //get path from files

        #region

        public string GetReportTemplate()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\NewReport.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        public string GetReportTemplateRegionWise()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\VRF_Report.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        public string WriteReportTemplateRegionWise()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\VRF_Report.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        #endregion

        #region

        public string WriteReport()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\Report.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        #endregion





        private NextGenBLL.PipingBLL GetPipingBLLInstance()
        {
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            bool isInch = CommonBLL.IsDimension_inch();
            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            return new NextGenBLL.PipingBLL(this.projectLegacy, utilPiping, null, isInch, ut_weight, ut_length,
                ut_power);
        }
        // end added for Side bar collapse Setting Panel 12-15-2018

        private void lnkBtnValidate_Click(object sender, RoutedEventArgs e)
        {
            var evm = new ErrorLogViewModel();//to clear the previous errors, line added by anup

            var MDViewModel = (MasterDesignerViewModel)this.DataContext;
            var SysIndex = MDViewModel.SelectedTabIndex;
            if (SysIndex >= 0)
            {
                SystemBase system = MDViewModel.Systems[SysIndex];
                if (system != null)
                {
                    if (system.HvacSystemType.Equals("1"))
                    {
                        var sys = (JCHVRF.Model.NextGen.SystemVRF)system;
                        if (sys != null && sys.IsAuto == true && sys.IsManualPiping == true && sys.SystemStatus == SystemStatus.INVALID)
                        {
                            JCHMessageBox.Show(language.Current.GetMessage("ALERT_CHANGES_CANVAS"));
                        }
                        else
                        {
                            _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Publish(sys);
                            _eventAggregator.GetEvent<SystemExportSubscriber>().Publish(sys);
                            if(!sys.IsPipingOK)
                            {
                                _eventAggregator.GetEvent<DisplayPipingLength>().Publish();
                            }
                        }
                        //UtilTrace.SaveHistoryTraces();//uncommented by vijay
                    }
                    else if (system.HvacSystemType.Equals("6"))
                    {
                        bool hasControllers = false;
                        var proj = Project.CurrentProject;
                        var hasSystem = proj.ControlGroupList.Find(c => c.ControlSystemID == system.Id);

                        if (((ControlSystem) system).IsAutoWiringPerformed)
                        {
                            if (hasSystem != null)
                            {
                                foreach (Controller ctrl in proj.ControllerList)
                                {
                                    if (ctrl.ControlGroupID == hasSystem.Id)
                                    {
                                        hasControllers = true;
                                    }
                                }

                                if (system.SystemStatus.Equals(SystemStatus.INVALID))
                                {
                                    hasSystem.IsValidGrp = false;
                                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_CONTROL_SYSTEM_ERROR"),
                                        MessageType.Error);
                                }
                                else if (!hasControllers)
                                {
                                    hasSystem.IsValidGrp = false;
                                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_CANNOT_VALIDATE_SYSTEM"),
                                        MessageType.Warning);

                                    system.SystemStatus = SystemStatus.INVALID;
                                }
                                else
                                {
                                    proj.ControlSystemList.Find(sys => sys.Id == system.Id).SystemStatus = SystemStatus.VALID;
                                    // to change the status icon in opened tab 
                                    hasSystem.IsValidGrp = true;
                                    system.SystemStatus = SystemStatus.VALID;
                                }
                            }
                        }
                        else
                        {
                            JCHMessageBox.Show("System Cannot be validated unless Auto-Control Wiring is Performed");
                        }
                        UtilTrace.SaveHistoryTraces();
                    }
                    //_eventAggregator.GetEvent<RefreshSystems>().Publish();
                }
            }
        }

        private void lnkBtnAutoWiringIcon_Click(object sender, RoutedEventArgs e)
        {
            var MDViewModel = (MasterDesignerViewModel)this.DataContext;
            var SysIndex = MDViewModel.SelectedTabIndex;
            if (SysIndex >= 0)
            {
                var currentControlSystem = (ControlSystem) MDViewModel.Systems[SysIndex];
                _eventAggregator.GetEvent<AutoControlWiringSubcriber>().Publish(currentControlSystem);
            }
        }
    }
}
