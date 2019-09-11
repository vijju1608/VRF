using JCBase.UI;
using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SystemVRF = JCHVRF.Model.NextGen.SystemVRF;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class ODUPropertiesViewModel : ViewModelBase
    {
        #region Local_Property
        JCHVRF.Model.Project CurrentProject;
        private IEventAggregator _eventAggregator;
        JCHVRF.BLL.OutdoorBLL bll;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        string navigateToFolder = "..\\..\\Image\\TypeImageProjectCreation";
        private SystemVRF CurrentSystem;
        public ImageData OduImageData { get; set; }

        #endregion
        #region label Outdoor value
        public static string SettingFile = AppDomain.CurrentDomain.BaseDirectory + @"\Settings.config.xml";

        private string _currentTempUnit;
        public string CurrentTempUnit
        {
            get { return SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE; }
            set
            {
                SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = value;
                string display = value == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c;
                this.SetValue(ref _currentTempUnit, value);
                DisplayCurrentTempUnit = display;
            }
        }
        private string _displayCurrentTempUnit;
        public string DisplayCurrentTempUnit
        {
            get { return CurrentTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c; ; }
            set
            {
                this.SetValue(ref _displayCurrentTempUnit, value);
            }
        }
        private string _lblOutdoorCoolingDB;

        public string lblOutdoorCoolingDB
        {
            get { return _lblOutdoorCoolingDB; }
            set { this.SetValue(ref _lblOutdoorCoolingDB, value); }
        }

        private string _lblOutdoorheatingDB;
        public string lblOutdoorheatingDB
        {
            get
            {
                return _lblOutdoorheatingDB;
            }
            set
            {

                this.SetValue(ref _lblOutdoorheatingDB, value);
            }
        }
        private string _lblOutDoorHeatingWB;
        public string lblOutDoorHeatingWB
        {
            get
            {
                return _lblOutDoorHeatingWB;
            }
            set
            {
                this.SetValue(ref _lblOutDoorHeatingWB, value);
            }
        }
        //private string _lblOutdoorHeatingRH;
        //public string lblOutdoorHeatingRH
        //{
        //    get
        //    {
        //        return _lblOutdoorHeatingRH;
        //    }
        //    set
        //    {
        //        this.SetValue(ref _lblOutdoorHeatingRH, value);
        //    }
        //}
        #endregion label value
        #region ViewModel Property
        #region ViwModel Control Properties
        private string _outdoorName;
        public string OutdoorName
        {
            get { return _outdoorName; }
            set { this.SetValue(ref _outdoorName, value); }
        }

        private string _modelFull;
        public string ModelFull
        {
            get { return _modelFull; }
            set
            {
                string temp_modelFull = _modelFull;
                this.SetValue(ref _modelFull, value);
                if (!string.IsNullOrEmpty(temp_modelFull) && temp_modelFull != _modelFull)
                { /*Utility.UtilTrace.SaveHistoryTraces();*/ }
            }
        }



        private string _selectedModel;
        public string SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                string tempSelectedModel = _selectedModel;
                this.SetValue(ref _selectedModel, value);
                if (SelectedModel != null)
                {
                    GetModalspacification();
                }
                if (!string.IsNullOrEmpty(tempSelectedModel) && tempSelectedModel != _modelFull)
                {
                    Utility.UtilTrace.SaveHistoryTraces();
                }
            }
        }
        private ObservableCollection<SeriesModel> listOduModel;
        public ObservableCollection<SeriesModel> ListOduModel
        {
            get { return listOduModel; }
            set
            {
                this.SetValue(ref listOduModel, value);
            }
        }
        private ObservableCollection<ComboBox> listMaxRatio;
        public ObservableCollection<ComboBox> ListMaxRatio
        {
            get { return listMaxRatio; }
            set { this.SetValue(ref listMaxRatio, value); }
        }

        private string _selectedMaxRatio;
        public string SelectedMaxRatio
        {
            get { return _selectedMaxRatio; }
            set
            {
                this.SetValue(ref _selectedMaxRatio, value);
                if (!String.IsNullOrEmpty(SelectedMaxRatio))
                    BindSelectedRatio();
            }
        }
        private bool _oduManualSelection;
        public bool OduManualSelection
        {
            get { return _oduManualSelection; }
            set { this.SetValue(ref _oduManualSelection, value); }
        }

        private ObservableCollection<ComboBox> _listOfSeries;
        public ObservableCollection<ComboBox> ListOfSeries
        {
            get { return _listOfSeries; }
            set { this.SetValue(ref _listOfSeries, value); }

        }

        private string _selectedSeries;
        public string SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                string tempSelectedSeries = _selectedSeries;
                this.SetValue(ref _selectedSeries, value);
                if (SelectedSeries != null)
                {
                    BindSelectedSeries();
                    GetType(SelectedSeries);
                }
                if (!string.IsNullOrEmpty(tempSelectedSeries))
                {
                    //Utility.UtilTrace.SaveHistoryTraces();
                }
            }
        }
        private string _OduImagePath = string.Empty;
        public string OduImagePath
        {
            get { return _OduImagePath; }
            set { this.SetValue(ref _OduImagePath, value); }
        }
        private double _coolingRated;
        public double CoolingRated
        {
            get { return _coolingRated; }
            set { this.SetValue(ref _coolingRated, value); }
        }
        private double _coolingCorrected;
        public double CoolingCorrected
        {
            get { return _coolingCorrected; }
            set { this.SetValue(ref _coolingCorrected, value); }
        }

        private double _heatingCorrected;
        public double HeatingCorrected
        {
            get { return _heatingCorrected; }
            set { this.SetValue(ref _heatingCorrected, value); }
        }
        private double _heatingRated;
        public double HeatingRated
        {
            get { return _heatingRated; }
            set { this.SetValue(ref _heatingRated, value); }
        }

        private string _electricalSpecification;
        public string ElectricalSpecification
        {
            get { return _electricalSpecification; }
            set { this.SetValue(ref _electricalSpecification, value); }
        }
        private double _maxNumberIDUconnections;
        public double MaxNumberIDUconnections
        {
            get { return _maxNumberIDUconnections; }
            set
            {
                this.SetValue(ref _maxNumberIDUconnections, value);
            }
        }
        private string _actualRatio;
        public string ActualRatio
        {
            get { return _actualRatio; }
            set { this.SetValue(ref _actualRatio, value); }
        }

        private string selectedPower;
        public string SelectedPower
        {
            get { return selectedPower; }
            set { selectedPower = value; }
        }
        private ObservableCollection<ComboBox> listType;
        public ObservableCollection<ComboBox> ListType
        {
            get { return listType; }
            set
            {

                this.SetValue(ref listType, value);
            }
        }
        private string _selectedType;
        public string SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                this.SetValue(ref _selectedType, value);
                if (SelectedType != null)
                {
                    if (ListType != null && ListType.Count > 0)
                        GetPowerValue(ListType.FirstOrDefault().DisplayName);
                }
                if (SelectedSeries != null && SelectedType != null && selectedPower != null)
                {
                    GetModelType(SelectedSeries, SelectedType, selectedPower);
                }
                if (OduImageData != null)
                    PublishImageData();
            }
        }
        #endregion Control Properties
        #region Design condition Properties
        private double _coolingDryBulb;
        public double CoolingDryBulb
        {
            get { return _coolingDryBulb; }
            set { this.SetValue(ref _coolingDryBulb, value); }
        }
        private double _HeatingWetBulb;
        public double HeatingWetBulb
        {
            get { return _HeatingWetBulb; }
            set { this.SetValue(ref _HeatingWetBulb, value); }
        }
        private double _HeatingRelativeHumidity;
        public double HeatingRelativeHumidity
        {
            get { return _HeatingRelativeHumidity; }
            set { this.SetValue(ref _HeatingRelativeHumidity, value); }

        }
        private double _heatingDryBulb;
        public double HeatingDryBulb
        {
            get { return _heatingDryBulb; }
            set { this.SetValue(ref _heatingDryBulb, value); }
        }
        public string CapacityMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingPOWER;
            }
        }
        #endregion Design condition Properties
        #region Change Temperature formate

        private bool _enableManualSelection;
        public bool EnableManualSelection
        {
            get { return _enableManualSelection; }
            set { this.SetValue(ref _enableManualSelection, value); }
        }
        private bool _enableManualselectionOduModel;
        public bool EnableManualselectionOduModel { get { return _enableManualselectionOduModel; } set { this.SetValue(ref _enableManualselectionOduModel, value); } }
        private string _TemperatureTypeOCDB;
        public string TemperatureTypeOCDB
        {
            get { return _TemperatureTypeOCDB; }
            set { this.SetValue(ref _TemperatureTypeOCDB, value); }
        }
        private string _TemperatureTypeOCIW;
        public string TemperatureTypeOCIW
        {
            get { return _TemperatureTypeOCIW; }
            set { this.SetValue(ref _TemperatureTypeOCIW, value); }
        }
        private string _TemperatureTypeOHIW;
        public string TemperatureTypeOHIW
        {
            get { return _TemperatureTypeOHIW; }
            set { this.SetValue(ref _TemperatureTypeOHIW, value); }
        }
        private string _TemperatureTypeOCWB;
        public string TemperatureTypeOCWB
        {
            get { return _TemperatureTypeOCWB; }
            set { this.SetValue(ref _TemperatureTypeOCWB, value); }
        }
        private string _TemperatureTypeOHDB;
        public string TemperatureTypeOHDB
        {
            get { return _TemperatureTypeOHDB; }
            set { this.SetValue(ref _TemperatureTypeOHDB, value); }
        }
        private string _TemperatureTypeICDB;
        public string TemperatureTypeICDB
        {
            get { return _TemperatureTypeICDB; }
            set { this.SetValue(ref _TemperatureTypeICDB, value); }
        }
        private string _TemperatureTypeICWB;
        public string TemperatureTypeICWB
        {
            get { return _TemperatureTypeICWB; }
            set { this.SetValue(ref _TemperatureTypeICWB, value); }
        }
        private string _TemperatureTypeIHDB;
        public string TemperatureTypeIHDB
        {
            get { return _TemperatureTypeIHDB; }
            set
            {
                this.SetValue(ref _TemperatureTypeIHDB, value);
            }
        }
        private string _selectedProductSeries;

        public string SelectedProductSeries
        {
            get { return _selectedProductSeries; }

            set {
                string oldSeries = _selectedProductSeries; //BUG # 4695 : Added temp variable to hold previous value of odu series.
                this.SetValue<string>(ref _selectedProductSeries, value);

                this.SelectedSeries = GetSeriesByProductSeriesAndProductCategory(_selectedProductSeries, CurrentSystem.ProductCategory);

                //Start Bug#4695 : Show Advisory to show the uncommited changes. Set the IsODUDirty flag to true.
                if (oldSeries != null && !oldSeries.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS"));
                    CurrentSystem.IsODUDirty = true;
                }
                //End Bug#4695
            }
        }

        #endregion
        #region Delegate Commands
        public DelegateCommand NumericHeatDryBulbCommand { get; set; }
        public DelegateCommand NumericHeatWetBulbCommand { get; set; }
        public DelegateCommand NumericInternalRHCommand { get; set; }
        public DelegateCommand ManualSelectionCheckedCommand { get; set; }
        public DelegateCommand ManualSelectionUnCheckedCommand { get; set; }
        public DelegateCommand SelectionChangedSelectedODUSeries { get; set; }
        public DelegateCommand ChangeTempCommand { get; set; }
        public DelegateCommand LostFocusCoolingDryBulbCommand { get; set; }



        #endregion Delegate Commands
        #endregion ViewModel Property
        #region Constructor 
        public ODUPropertiesViewModel(IEventAggregator eventAggregator)
        {
            OduList = null;
            _eventAggregator = eventAggregator;
            CurrentProject = JCHVRF.Model.Project.GetProjectInstance;

            NumericHeatDryBulbCommand = new DelegateCommand(NumericHeatingDryBulb_LostFocus);
            NumericHeatWetBulbCommand = new DelegateCommand(NumericHeatWetBulb_LostFocus);
            NumericInternalRHCommand = new DelegateCommand(NumericInternalRH_LostFocus);
            LostFocusCoolingDryBulbCommand = new DelegateCommand(NumericCoolDryBulb_LostFocus);

            ManualSelectionCheckedCommand = new DelegateCommand(ManualSelectionCheckedEvent);
            ManualSelectionUnCheckedCommand = new DelegateCommand(ManualSelectionUnCheckedEvent);
            ChangeTempCommand = new DelegateCommand(btnChangeTempClicked);

            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                bll = new OutdoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode, JCHVRF.Model.Project.CurrentProject.RegionCode);
            }
            else
            {
                bll = new JCHVRF.BLL.OutdoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
            }
        }
        private void Initializations()
        {
            OduList = null;
            if (CurrentSystem != null)
            {
                if (CurrentSystem.IsAuto)
                {
                    this.EnableManualSelection = false;
                    this.EnableManualselectionOduModel = false;
                }
                else
                {
                    this.EnableManualSelection = true;
                    this.EnableManualselectionOduModel = true;
                }
            }
            OutdoorName = SystemSetting.UserSetting.defaultSetting.OutdoorName + CurrentSystem.Name;
            GetSeries();
            BindRatio();
            CoolingDryBulb = Convert.ToDouble(CurrentSystem.DBCooling);
            HeatingDryBulb = Convert.ToDouble(CurrentSystem.DBHeating);
            HeatingWetBulb = Convert.ToDouble(CurrentSystem.WBHeating);
            HeatingRelativeHumidity = Convert.ToDouble(CurrentSystem.RHHeating);
            //CoolingDryBulb = Convert.ToDouble(CurrentProject.DesignCondition.outdoorCoolingDB);
            //HeatingDryBulb = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingDB);
            //HeatingWetBulb = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingWB);
            //HeatingRelativeHumidity = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingRH);

            TemperatureTypeOCDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;//updated as part of default measurement units
            TemperatureTypeOCIW = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeOHIW = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeOCWB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeOHDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeICDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeICWB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeIHDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;

        }
        #endregion Constructor 

        public override void OnNavigatedTo(NavigationContext context)
        {
            OduImageData = context.Parameters.GetValue<ImageData>("OduImageData");
            CurrentSystem = context.Parameters.GetValue<SystemVRF>("CurrentSystem");
            //OutdoorName = SystemSetting.UserSetting.defaultSetting.OutdoorName + _currentSystem.Name;
            Initializations();
            ////if (OduImageData != null)
            ////    this.SelectedSeries = OduImageData.imageName;

        }
        private void ManualSelectionCheckedEvent()
        {
            EnableManualselectionOduModel = true;
            EnableManualSelection = true;
            CurrentSystem.IsAuto = false;
        }
        private void ManualSelectionUnCheckedEvent()
        {
            EnableManualselectionOduModel = false;
            EnableManualSelection = false;
            CurrentSystem.IsAuto = true;
        }
        private void BindOutDoorImageUI(string selectedModel)
        {
            if (ListOduModel != null)
            {
                if (ListOduModel.Count > 0)
                {
                    var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                    if (ListOduModel.Where(x => x.DisplayName.Equals(selectedModel)).FirstOrDefault() != null)
                        this.OduImagePath = ListOduModel.Where(x => x.DisplayName.Equals(selectedModel)).FirstOrDefault().OduImagePath;
                }
            }
        }
        private void GetType(string series)
        {
            string ProductType = string.Empty;
            ListType = new ObservableCollection<ComboBox>();
            //DataTable dtSeries = bll.GetOutdoorTypeBySeries(series);
            DataTable dtSeries = bll.GetOutdooTypeBySeriesAndCategory(series, SelectedProductSeries, CurrentSystem.ProductCategory);
            foreach (DataRow typeRow in dtSeries.Rows)
            {
                ListType.Add(new ComboBox { DisplayName = typeRow.ItemArray[0].ToString(), Value = typeRow.ItemArray[1].ToString() });
            }
            if (ListType.Count > 0)
            {
                this.SelectedType = ListType[0].DisplayName.ToString();
                ProductType = ListType[0].Value.ToString();
            }

            if (CurrentSystem != null)
            {
                CurrentSystem.Series = series;
                CurrentSystem.ProductType = ProductType;
                CurrentSystem.SelOutdoorType = SelectedType;
                if (!String.IsNullOrEmpty(OduImagePath))
                    CurrentSystem.DisplayImageName = OduImagePath;
            }
        }
        private void BindSelectedRatio()
        {

            CurrentSystem.MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100);
        }
        private void BindSelectedSeries()
        {

            CurrentSystem.Series = this.SelectedSeries;
            if (CurrentSystem.OutdoorItem != null)
                CurrentSystem.OutdoorItem.Series = this.SelectedSeries;
        }
        private void BindRatio()
        {
            ListMaxRatio = new ObservableCollection<ComboBox>();
            double max = 1.3;
            double min = 0.5;
            double defaultMaxRatio = 1;
            defaultMaxRatio = 0;
            ListMaxRatio.Clear();
            if (SelectedSeries != null)
            {
                if (SelectedSeries.Contains("FSNP") || SelectedSeries.Contains("FSXNP"))
                {
                    max = 1.5;
                }
                else if (JCHVRF.Model.Project.CurrentProject.SubRegionCode == "TW" && SelectedSeries.StartsWith("IVX,"))
                {
                    min = 1;
                }
            }
            for (int i = (int)(max * 100); i >= min * 100; i -= 10)
            {
                ListMaxRatio.Add(new ComboBox { DisplayName = Convert.ToString(i), Value = Convert.ToString(i) });
            }
            if (CurrentProject.SystemListNextGen != null)
            {

                if (CurrentSystem.MaxRatio == 0)
                    SelectedMaxRatio = Convert.ToString((double)(Math.Round(max - defaultMaxRatio, 2) * 100));
                else
                    SelectedMaxRatio = Convert.ToString(CurrentSystem.MaxRatio * 100);



                //    if (CurrentProject.SystemListNextGen.FirstOrDefault(sys => sys.IsActiveSystem == true) != null && CurrentProject.SystemListNextGen.FirstOrDefault(sys => sys.IsActiveSystem == true).OutdoorItem != null)
                //{
                //    if (CurrentProject.SystemListNextGen.FirstOrDefault(sys => sys.IsActiveSystem == true).OutdoorItem.MaxRatio == 0)
                //    {
                //        SelectedMaxRatio = Convert.ToString((int)(Math.Round(max - defaultMaxRatio, 2) * 100));
                //    }
                //    else
                //    {
                //        SelectedMaxRatio = Convert.ToString(CurrentProject.SystemListNextGen.FirstOrDefault(sys => sys.IsActiveSystem == true).OutdoorItem.MaxRatio);
                //    }
                //}
            }


        }
        private bool IsValidODUName(string OutdoorName)
        {
            bool isValid = false;
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (!string.IsNullOrWhiteSpace(OutdoorName))
            {
                if (r.IsMatch(OutdoorName))
                {
                    isValid = true;
                }
            }
            return isValid;

        }
        private void GetPowerValue(string SelectedType)
        {
            var dtPower = bll.GetOutdoorPowerListBySeriesAndType(SelectedSeries, SelectedType);
            if (dtPower.Rows.Count > 0)
            {
                foreach (DataRow rowView in dtPower.Rows)
                {
                    SelectedPower = rowView["Value"].ToString();
                    break;
                }

                if (CurrentSystem != null)
                { CurrentSystem.Power = SelectedPower; }
            }
        }
        
        private void GetSeries()
        {
            ListOfSeries = new ObservableCollection<ComboBox>();
            //if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            //{
            //    dtSeries = pbll.GetOduSeries(JCHVRF.Model.Project.CurrentProject.BrandCode, JCHVRF.Model.Project.CurrentProject.SubRegionCode);
            //}
            //else
            //{
            //    dtSeries = pbll.GetProductTypeData(JCHVRF.Model.Project.CurrentProject.BrandCode, JCHVRF.Model.Project.CurrentProject.SubRegionCode);
            //}

            //foreach (DataRow rowView in dtSeries.Rows)
            //{
            //    ListOfSeries.Add(new ComboBox { DisplayName = Convert.ToString(rowView["ODU_Series"]), Value = Convert.ToString(rowView["ODU_Series"]) });

            //}
            if (CurrentSystem != null)
            {
                DataTable dtSeries = bll.GetOutdoorListStd(CurrentSystem.ProductCategory);
                
                ListOfSeries = new ObservableCollection<ComboBox>();
                var distinctSeries = dtSeries.AsEnumerable()
                            .Select(row => new
                            {
                                DisplayName = row.Field<string>("ProductSeries"),
                                Value = row.Field<string>("ProductSeries")
                            })
                            .Distinct();

                foreach (var rowView in distinctSeries)
                {
                    ListOfSeries.Add(new ComboBox { DisplayName = rowView.DisplayName, Value = rowView.Value });

                }

                if (CurrentSystem != null && CurrentSystem.Series != null)
                {
                    this.SelectedSeries = CurrentSystem.Series;
                    if (CurrentSystem.OutdoorItem != null && CurrentSystem.OutdoorItem.Model != null && CurrentSystem.Series != null)
                        this.SelectedProductSeries = GetProductSeriesByOdu(CurrentSystem.OutdoorItem.Model, CurrentSystem.Series);
                    else
                        this.SelectedProductSeries = CurrentSystem.ProductSeries;
                }
                else
                {
                    if (ListOfSeries != null && ListOfSeries.Count > 0)
                        this.SelectedSeries = GetSeriesByProductSeriesAndProductCategory(ListOfSeries[0].Value.ToString(), CurrentSystem.ProductCategory);
                }

            }

        }

        private string GetProductSeriesByOdu(string model, string series)
        {
            DataTable dtModuleType = GetOdu();

            var rows = dtModuleType.Select($"Model = '{model}' and Series = '{series}'");
            if (rows.Count() > 0)
                return rows[0]["ProductSeries"].ToString();
            else
                return CurrentSystem.ProductSeries;
        }

        private string GetSeriesByProductSeriesAndProductCategory(string productSeries, string productCategory) {

            var result = string.Empty;
            if(productSeries!=null)
            CurrentSystem.ProductSeries = productSeries;
            DataTable dtSeries = bll.GetOutdoorListStd(CurrentSystem.ProductCategory);

            var rows = 
                dtSeries.Select($"ProductSeries = '{productSeries}' and ProductCategory = '{productCategory}'");

            if (rows.Count() > 0)
                result = rows[0]["Series"].ToString();

            if (String.IsNullOrEmpty(result))
            {
                int projectId = -1;
                Int32.TryParse(CurrentSystem.Id, out projectId);
                Logger.LogProjectError(projectId, new Exception($"Couldn't resolve series for - product category: {productCategory}, product series: {productSeries}"), false);
            }
                
            return result;
            
        }

        private DataTable GetOdu()
        {
            if (OduList == null)
            {
                OduList = bll.GetOutdoorListStd();
            }
            return OduList;
        }

        private void GetModelType(string Series, string SelectedType, string selectedPower)
        {
            DataTable dtModuleType = bll.GetOutdoorListStd();
            DataTable dtFilter = new DataTable();
            DataRow[] rows;
            if ("EU_E".Equals(JCHVRF.Model.Project.CurrentProject.SubRegionCode) && "H".Equals(JCHVRF.Model.Project.CurrentProject.BrandCode))
                rows = dtModuleType.Select("SubString(ModelFull,11,1)='" + selectedPower + "' and UnitType='" + SelectedType + "' and Series='" + Series + "'" + " and TypeImage <> '' and ModelFull <> 'JVOL060VVEM0AQ'", "Model asc");
            else
                rows = dtModuleType.Select("SubString(ModelFull,11,1)='" + selectedPower + "' and UnitType='" + SelectedType + "' and Series='" + Series + "'" + " and TypeImage <> ''", "Model asc");
            if (rows.Length > 0)
            {
                dtModuleType = rows.CopyToDataTable();
                var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                ListOduModel = new ObservableCollection<SeriesModel>();
                foreach (DataRow producttypeRow in dtModuleType.Rows)
                {
                    ListOduModel.Add(new SeriesModel()
                    {
                        DisplayName = JCHVRF.Model.Project.CurrentProject.BrandCode == "H" ? Convert.ToString(producttypeRow["Model_Hitachi"]) : Convert.ToString(producttypeRow["Model_York"]),
                        SelectedValues = JCHVRF.Model.Project.CurrentProject.BrandCode == "H" ? Convert.ToString(producttypeRow["Model_Hitachi"]) : Convert.ToString(producttypeRow["Model_York"]),
                        OduImagePath = sourceDir + "\\" + Convert.ToString(producttypeRow["TypeImage"]),
                        FullModelName = Convert.ToString(producttypeRow["ModelFull"])
                    });
                }


                if (CurrentSystem != null)
                {
                    if (CurrentSystem.OutdoorItem != null)
                    {
                        string ModelNameTemp;
                        if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                            ModelNameTemp = CurrentSystem.OutdoorItem?.Model_Hitachi;
                        else
                            ModelNameTemp = CurrentSystem.OutdoorItem?.Model_York;
                        if (ListOduModel.Any(mm => mm.SelectedValues == ModelNameTemp))
                        {
                            this.SelectedModel = ModelNameTemp;
                            this.ModelFull = CurrentSystem.OutdoorItem?.ModelFull;
                        }
                        else
                        {
                            this.SelectedModel = ListOduModel[0].DisplayName.ToString();
                            this.ModelFull = ListOduModel[0].FullModelName.ToString();
                        }
                    }
                    else
                    {
                        this.SelectedModel = ListOduModel[0].DisplayName.ToString();
                        this.ModelFull = ListOduModel[0].FullModelName.ToString();
                    }
                }


                if (this.SelectedModel != null)
                {
                    BindOutDoorImageUI(this.SelectedModel);
                }
            }
        }
        private void GetModalspacification()
        {
            if (CurrentSystem != null)
            {

                if (EnableManualSelection == false)
                {
                    //CurrentSystem.IsAuto = true;
                    if (CurrentSystem.OutdoorItem != null)
                    {
                        CoolingRated = CurrentSystem.OutdoorItem.CoolingCapacity;
                        HeatingRated = CurrentSystem.OutdoorItem.HeatingCapacity;
                        CoolingCorrected = CurrentSystem.CoolingCapacity;
                        HeatingCorrected = CurrentSystem.HeatingCapacity;
                        MaxNumberIDUconnections = CurrentSystem.OutdoorItem.MaxIU;
                        ActualRatio = ((CurrentSystem.Ratio * 100).ToString("n0") + "%");
                        if (ListOfSeries != null && ListOfSeries.Count > 0 && ListOfSeries[0].DisplayName != null)
                        {
                            var startIndex = (SelectedProductSeries.IndexOf("~") - 3)<=0?SelectedProductSeries.IndexOf("V/") - 3 :SelectedProductSeries.IndexOf("~") - 3;
                            ElectricalSpecification = SelectedProductSeries.Substring(startIndex, (SelectedProductSeries.IndexOf("Hz") + 2) - startIndex);
                        }
                    }
                }

                else
                {
                    //CurrentSystem.IsAuto = false;       
                    if (ListOduModel.FirstOrDefault(mm => mm.SelectedValues == SelectedModel) != null)
                        ModelFull = ListOduModel.FirstOrDefault(mm => mm.SelectedValues == SelectedModel).FullModelName;
                    BindOutDoorImageUI(SelectedModel);
                    CurrentSystem.OutdoorItem = bll.GetOutdoorItemBySeries(ModelFull, SelectedSeries);
                    if (CurrentSystem.OutdoorItem != null)
                    {
                        CoolingRated = CurrentSystem.OutdoorItem.CoolingCapacity;
                        HeatingRated = CurrentSystem.OutdoorItem.HeatingCapacity;
                        CoolingCorrected = CurrentSystem.CoolingCapacity;
                        HeatingCorrected = CurrentSystem.HeatingCapacity;
                        MaxNumberIDUconnections = CurrentSystem.OutdoorItem.MaxIU;
                        ActualRatio = ((CurrentSystem.Ratio * 100).ToString("n0") + "%");
                        if (ListOfSeries != null && ListOfSeries[0].DisplayName != null)
                        {
                            var startIndex = (SelectedProductSeries.IndexOf("~") - 3) <= 0 ? SelectedProductSeries.IndexOf("V/") - 3 : SelectedProductSeries.IndexOf("~") - 3;
                            ElectricalSpecification = SelectedProductSeries.Substring(startIndex, (SelectedProductSeries.IndexOf("Hz") + 2) - startIndex);
                        }
                    }
                }
            }
        }
        //private void ReselectOutdoor()
        //{
        //    SelectOutdoorResult result;
        //    if (EnableManualSelection==false)
        //    {
        //        //室外机选型统一改用新逻辑 Global.DoSelectOutdoorODUFirst 20161112 by Yunxiao Lin
        //       // result = Global.DoSelectOutdoorODUFirst(CurrentSystem, CurrentProject.RoomIndoorList, CurrentProject, out ERRList, out MSGList);
        //    }
        //    else
        //    {
        //        OutdoorBLL bll = new OutdoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
        //        string model = Convert.ToString(SelectedModel);
        //        string productType =SelectedType;
        //        CurrentSystem.OutdoorItem = bll.GetOutdoorItemBySeries(model, productType);
        //        //result = Global.DoSelectOutdoorManual(CurrentSystem, CurrentProject.RoomIndoorList, CurrentProject, out ERRList);
        //    }

        //}

        //private void SelectMaxRatio()
        //{
        //    //ReselectOutdoor();
        //    if (EnableManualSelection == true && CurrentSystem.OutdoorItem!=null)
        //    {
        //        SelectedSeries = CurrentSystem.OutdoorItem.Series;
        //        SelectedModel = CurrentSystem.OutdoorItem.ModelFull;
        //    }
        //}
        private void PublishImageData()
        {
            if (OduImageData != null)
            {
                OduImageData.imageName = this.SelectedSeries;
                OduImageData.imagePath = File.Exists(OduImagePath) ? OduImagePath : OduImagePath.Replace("TypeImages", "TypeImageProjectCreation");
                if (File.Exists(OduImageData.imagePath))
                {
                    _eventAggregator.GetEvent<SetOduPropertiesOnCanvas>().Publish(OduImageData);
                }
            }
        }
        #region Design Condition validation
        void btnChangeTempClicked()
        {
            try
            {
                CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = CurrentTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c;
                RaisePropertyChanged("CoolingDryBulb");
                RaisePropertyChanged("HeatingWetBulb");
                RaisePropertyChanged("HeatingRelativeHumidity");
                RaisePropertyChanged("HeatingDryBulb");
                TemperatureTypeOCDB = TemperatureTypeOCWB = TemperatureTypeOHDB = TemperatureTypeICDB = TemperatureTypeICWB = TemperatureTypeIHDB = TemperatureTypeOCIW = TemperatureTypeOHIW = CurrentTempUnit;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {

            }
        }
        protected void UpdateSelectedEquipmentValues(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (this.CurrentSystem != null)
                {
                    switch (propertyName)
                    {
                        case "CoolingDB":
                            this.CurrentSystem.DBCooling = Convert.ToDouble(_coolingDryBulb);
                            break;
                        case "HeatingDB":
                            this.CurrentSystem.DBHeating = Convert.ToDouble(_heatingDryBulb);
                            break;
                        case "HeatingWB":
                            this.CurrentSystem.WBHeating = Convert.ToDouble(_HeatingWetBulb);
                            break;
                        case "HeatingRH":
                            this.CurrentSystem.RHHeating = Convert.ToDouble(_HeatingRelativeHumidity);
                            break;
                    }
                }
            }
        }
        private void NumericCoolDryBulb_LostFocus()
        {
            if (ValidateOdb() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd("DB");
                UpdateSelectedEquipmentValues("CoolingDB");
            }
        }

        public void NumericHeatingDryBulb_LostFocus()
        {
            if (ValidateHeatingDryBulb() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd("DB");
                UpdateSelectedEquipmentValues("HeatingDB");
            }
        }
        private void NumericHeatWetBulb_LostFocus()
        {
            if (ValidateHeatWetBulb() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd("WB");
                UpdateSelectedEquipmentValues("HeatingWB");
            }

        }
        private void NumericInternalRH_LostFocus()
        {

            if (ValidateRH() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd(UnitTemperature.RH.ToString()); //issue fix 1647
                                                                       // DoCalculateByOptionInd(NumericInternalRH.Value.ToString());
                UpdateSelectedEquipmentValues("HeatingRH");
            }
        }

        private bool ValidateOdb()
        {
            double nOdb = Math.Round(Convert.ToDouble(CoolingDryBulb)); //Convert.ToDouble(outdoorCoolingDB);

            if ((nOdb >= 10) && (nOdb <= 43))
            {
                lblOutdoorCoolingDB = string.Empty;
                return true;

            }
            else
            {
                lblOutdoorCoolingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(43, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[10, 43]";
                return false;
            }

        }
        private bool ValidateRH()
        {
            double nRHVal = Convert.ToDouble(HeatingRelativeHumidity);

            if ((nRHVal >= 13) && (nRHVal <= 100))
            {
                return true;

            }
            else
            {
                return false;
            }

        }
        private bool ValidateHeatWetBulb()
        {
            double nCWBVal = Math.Round((Convert.ToDouble(HeatingWetBulb))); //Convert.ToDouble(indoorCoolingWB);

            if ((nCWBVal >= -20) && (nCWBVal <= 15))
            {

                lblOutDoorHeatingWB = string.Empty;
                if (Convert.ToDecimal(HeatingWetBulb) > Convert.ToDecimal(HeatingDryBulb))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                    return false;
                }

                return true;

            }
            else
            {
                lblOutDoorHeatingWB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-20, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(15, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[14, 24]";

                if (Convert.ToDecimal(HeatingWetBulb) > Convert.ToDecimal(HeatingDryBulb))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));

                }
                return false;
            }
        }
        private bool ValidateHeatingDryBulb()
        {

            double nCDBVal = Math.Round(Convert.ToDouble(HeatingDryBulb));
            if ((nCDBVal >= -18) && (nCDBVal <= 33))
            {

                lblOutdoorheatingDB = string.Empty;
                return true;

            }
            else
            {
                lblOutdoorheatingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-18, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(33, UnitType.TEMPERATURE, CurrentTempUnit));

                if (Convert.ToDecimal(HeatingDryBulb) < Convert.ToDecimal(HeatingWetBulb) && !(HeatingDryBulb == 0))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));

                }

                return false;
            }
        }
        public void DoCalculateByOptionInd(string Opt)
        {
            double dbcool = Convert.ToDouble(HeatingDryBulb.ToString());
            double wbcool = Convert.ToDouble(HeatingWetBulb.ToString());
            double rhcool = Convert.ToDouble(HeatingRelativeHumidity);
            FormulaCalculate fc = new FormulaCalculate();
            decimal pressure = fc.GetPressure(Convert.ToDecimal(0));
            if (Opt == UnitTemperature.WB.ToString())
            {
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));

                if (this.HeatingRelativeHumidity.ToString() != (rh * 100).ToString("n0"))
                {
                    this.HeatingRelativeHumidity = Convert.ToDouble((rh * 100).ToString("n0"));
                }
            }
            else if (Opt == UnitTemperature.DB.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (CoolingDryBulb.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {

                        HeatingWetBulb = Convert.ToDouble(wb.ToString("n1"));

                    }
                }

            }
            else if (Opt == UnitTemperature.RH.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (this.CoolingDryBulb.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {
                        this.HeatingWetBulb = (Double)wb;
                    }

                }
            }
        }
        #endregion Design Condition validation

        private DataTable OduList = null;

        private const string SEPARATOR = "####";
    }
}
