/****************************** File Header ******************************\
File Name:	SettingsViewModel.cs
Date Created:	2/1/2019
Description:	View Model For Settings Panel.
\*************************************************************************/

namespace JCHVRF_New.ViewModels
{
    using JCBase.Utility;
    using JCHVRF.DAL.NextGen;
    using JCHVRF.Model;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using JCHVRF_New.Utility;
    using Prism.Commands;
    using Prism.Events;
    using System;
    using System.Windows;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Registr;
    using System.Xml;
    using System.Reflection;
    using JCHVRF_New.LanguageData;
    using System.ComponentModel;
    using JCHVRF.BLL;
    using System.Windows.Controls;
    using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
    using JCHVRF_New.Common.Contracts;
    using System.Collections.ObjectModel;
    using JCHVRF.Model.NextGen;

    public class SettingsViewModel : Common.Helpers.ViewModelBase
    {
        #region Fields

        private IEventAggregator _eventAggregator;

        private NamePrefixModel _namePrefixData;

        private List<Language> _activelanguages;

        private AirflowUnit _selectedAirflowUnit;

        private AreaUnit _selectedAreaUnit;

        private CapacityUnit _selectedCapacityUnit;

        private DimensionsUnit _selectedDimensionsUnit;

        private LengthUnit _selectedLengthUnit;

        private LoadIndexUnit _selectedLoadIndexUnit;

        private int _selectedTabIndex;

        private TemperatureUnit _selectedTemperatureUnit;

        private WaterFlowRateUnit _selectedWaterFlowRateUnit;

        private ESP _eSPUnit;
        private WeightUnit _selectedWeightUnit;



        public DelegateCommand InfoCircleMouseDown { get; set; }
        public DelegateCommand RegionChangeCommmand { get; set; }
        private UILanguageDefn _languageMapping;
        public DelegateCommand SubRegionChangeCommmand { get; set; }
        private SettingsModel _allSettings;
        public UILanguageDefn CurrentLanguage
        {
            get { return _languageMapping; }
        }
        SettingsModel _settings;

        NamePrefixDAL objNamePrefixDAL = new NamePrefixDAL();

        bool result;
        private string permittedRegionCode = string.Empty;
        private bool enableFlag = true;

        #endregion

        #region Constructors

        public ObservableCollection<Notification> Notifications
        {
            get
            {
                return this._globalProperties.Notifications;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">The eventAggregator<see cref="IEventAggregator"/></param>       
        public SettingsViewModel(IEventAggregator eventAggregator, IGlobalProperties globalProperties)
        {
            _eventAggregator = eventAggregator;
            _globalProperties = globalProperties;
            _eventAggregator.GetEvent<PubSubEvent<ISideBarItem>>().Subscribe(OnSettingsOpenedFromSideDrawer);
            _eventAggregator.GetEvent<GlobalPropertiesUpdated>().Subscribe(RaisePropertyChanged, ThreadOption.BackgroundThread, false, a => { return (a == nameof(Notifications)); });

            SaveClickCommand = new DelegateCommand(OnSaveClicked);
            _namePrefixData = new NamePrefixModel();
            RegionChangeCommmand = new DelegateCommand(OnRegionChangeCommmand);
            SubRegionChangeCommmand = new DelegateCommand(OnSubRegionChangeCommmand);

            //GetAllDefaultNamePrefix(); when we need to fetch first load with default values from the utility class, currently this is being loaded from XML.

            InfoCircleMouseDown = new DelegateCommand(OnInfoCircleMouseDown);

            LoadData();
            loadLanguageRadioButtons();
            BindRegion();

            SelectedRegionCode = SystemSetting.UserSetting.locationSetting.region;

            EnableFlag = (Registration.IsSuperUser()) ? true : false;

            BindSubRegion(SelectedRegionCode);
            SelectedSubRegionCode = SystemSetting.UserSetting.locationSetting.subRegion;
            _settings = new SettingsModel(this);
            _allSettings = _settings;
          
            //UpdateLanguageData();
        }      
        //public bool UpdateLanguageData()
        //{
        //    string languageCode = string.Empty;

        //    ActiveLanguages.ForEach((d) =>
        //    {
        //        if (d.IsSelected)
        //            languageCode = d.LanguageCode;
        //    });
        //    string files = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').ToString();
        //    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        //    string navigateToFolder = "..\\..\\LanguageData\\Lang" + languageCode + ".xml";
        //    string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
        //    XmlDocument languageData = new XmlDocument();
        //    languageData.Load(sourceDir);
        //    _languageMapping = new UILanguageDefn();
        //    _languageMapping.LoadLanguageData(languageData.DocumentElement);
        //    CommonViewModel.Current.LanguageDefn = CurrentLanguage;
            


        //    return true;
        //}       
        private void OnSaveClicked()
        {
            //if (SelectedTabIndex == 0)
            //{           
            //    UpdateLanguageData();
            //}
            //else
            //{
                if (CheckData())
                {
                    
                    SetData();
                    SystemSetting.Serialize();
                    RefreshDashBoard(); 
                    JCHMessageBox.Show(Langauge.Current.GetMessage("SAVE_SUCCESSFULLY"));//"Save Successfully"
                                         // Will any message popup after save, or we should navigate to dashboard ?
                                         // RefreshLanguage();
            }
            //}
            //}
        }

        private void RefreshDashBoard()
        {
            _eventAggregator.GetEvent<RefreshDashboard>().Publish();
        }
        private void SetData()
        {
            switch (SelectedCapacityUnit)
            {
                case CapacityUnit.kw:
                    SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_kw;
                    break;
                case CapacityUnit.ton:
                    SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_ton;
                    break;
                case CapacityUnit.btu:
                    SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_btu;
                    break;
            }

            switch (SelectedAirflowUnit)
            {
                case AirflowUnit.ls:
                    SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_ls;
                    break;
                case AirflowUnit.m3h: //It means m3/min in legacy
                    SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_m3h;
                    break;
                case AirflowUnit.m3hr:// This is not in new application, it meant m3/h in legacy
                    SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_m3hr;
                    break;
                case AirflowUnit.cfm:
                    SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_cfm;
                    break;
            }

            switch (SelectedTemperatureUnit)
            {
                case TemperatureUnit.F:
                    SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = Unit.ut_Temperature_f;
                    break;
                case TemperatureUnit.C:
                    SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = Unit.ut_Temperature_c;
                    break;
            }

            switch (SelectedLengthUnit)
            {
                case LengthUnit.m:
                    SystemSetting.UserSetting.unitsSetting.settingLENGTH = Unit.ut_Size_m;
                    break;
                case LengthUnit.ft:
                    SystemSetting.UserSetting.unitsSetting.settingLENGTH = Unit.ut_Size_ft;
                    break;
            }

            switch (SelectedDimensionsUnit)
            {
                case DimensionsUnit.mm:
                    SystemSetting.UserSetting.unitsSetting.settingDimension = Unit.ut_Dimension_mm;
                    break;
                case DimensionsUnit.inch:
                    SystemSetting.UserSetting.unitsSetting.settingDimension = Unit.ut_Dimension_inch;
                    break;
            }

            // This was in legacy app for Unit Dimensions and above one for Piping Dimensions
            //if (rbDemensionsmmUnit.Checked == true)
            //{
            //    SystemSetting.UserSetting.unitsSetting.settingDimensionUnit = Unit.ut_Dimension_mm;
            //}
            //else
            //{
            //    SystemSetting.UserSetting.unitsSetting.settingDimensionUnit = Unit.ut_Dimension_inch;
            //}

            switch (SelectedWeightUnit)
            {
                case WeightUnit.kg:
                    SystemSetting.UserSetting.unitsSetting.settingWEIGHT = Unit.ut_Weight_kg;
                    break;
                case WeightUnit.lbs:
                    SystemSetting.UserSetting.unitsSetting.settingWEIGHT = Unit.ut_Weight_lbs;
                    break;
            }

            switch (SelectedAreaUnit)
            {
                case AreaUnit.m2:
                    SystemSetting.UserSetting.unitsSetting.settingAREA = Unit.ut_Area_m2;
                    break;
                case AreaUnit.ft2:
                    SystemSetting.UserSetting.unitsSetting.settingAREA = Unit.ut_Area_ft2;
                    break;
            }

            switch (SelectedLoadIndexUnit)
            {
                case LoadIndexUnit.Wm2:
                    SystemSetting.UserSetting.unitsSetting.settingLOADINDEX = Unit.ut_LoadIndex_w;
                    break;
                case LoadIndexUnit.MBH:
                    SystemSetting.UserSetting.unitsSetting.settingLOADINDEX = Unit.ut_LoadIndex_MBH;
                    break;
            }

            switch (SelectedWaterFlowRateUnit)
            {
                case WaterFlowRateUnit.m3h:
                    SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate = Unit.ut_WaterFlowRate_m3h;
                    break;
                case WaterFlowRateUnit.lmin:
                    SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate = Unit.ut_WaterFlowRate_lmin;
                    break;
            }

            switch (ESPUnit)
            {
                case ESP.Pa:
                    SystemSetting.UserSetting.unitsSetting.settingESP = Unit.ut_Pressure;
                    break;
                case ESP.InWG:
                    SystemSetting.UserSetting.unitsSetting.settingESP = Unit.ut_Pressure_inWG;
                    break;
            }
            SystemSetting.UserSetting.unitsSetting.settingHeight = Unit.ut_Size_m;

            // NamePrefixes
            SystemSetting.UserSetting.defaultSetting.BuildingName = _namePrefixData.BuildingName;
            SystemSetting.UserSetting.defaultSetting.FloorName = _namePrefixData.FloorName;
            SystemSetting.UserSetting.defaultSetting.RoomName = _namePrefixData.RoomName;
            //SystemSetting.UserSetting.defaultSetting.freshAirAreaName = _namePrefixData; //unlike legacy, new project does not have this field 
            SystemSetting.UserSetting.defaultSetting.FreshAirAreaName = _namePrefixData.SystemName; //Legacy has the field FreshAirArea whereas new version US-3.10 has SystemName.
            SystemSetting.UserSetting.defaultSetting.IndoorName = _namePrefixData.IndoorUnitsName;
            SystemSetting.UserSetting.defaultSetting.OutdoorName = _namePrefixData.OutdoorUnitName;
            SystemSetting.UserSetting.defaultSetting.ControllerName = _namePrefixData.Controllers;
            SystemSetting.UserSetting.defaultSetting.ExchangerName = _namePrefixData.TotalHeatExchangers;
            SystemSetting.UserSetting.locationSetting.region = SelectedRegionCode;
            SystemSetting.UserSetting.locationSetting.subRegion = SelectedSubRegionCode;
            SystemSetting.UserSetting.defaultSetting.Language = SystemSetting.SelectedLanguage;
            SystemSetting.UserSetting.defaultSetting.LanguageCode = SystemSetting.SelectedLanguageCode;

            LanguageViewModel.Current.UpdateLanguageData();
            //thisProject.EnableAltitudeCorrectionFactor = jccbAltitude.Checked;

            //string temperatureUnit = Unit.ut_Temperature_c;
            //if (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE != Unit.ut_Temperature_c)
            //{
            //    temperatureUnit = Unit.ut_Temperature_f;
            //}

            //SystemSetting.UserSetting.defaultSetting.indoorCoolingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorCoolingDB.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);
            //SystemSetting.UserSetting.defaultSetting.indoorCoolingWB = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorCoolingWB.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);
            //SystemSetting.UserSetting.defaultSetting.indoorHeatingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorHeatingDB.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);
            //SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorCoolingDB.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);
            //SystemSetting.UserSetting.defaultSetting.outdoorHeatingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingDB.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);
            //SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingWB.Text),
            //                                                                                UnitType.TEMPERATURE,
            //SystemSetting.UserSetting.defaultSetting.outdoorCoolingIW = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorCoolingIW.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);
            //SystemSetting.UserSetting.defaultSetting.outdoorHeatingIW = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingIW.Text),
            //                                                                                UnitType.TEMPERATURE,
            //                                                                                temperatureUnit);

            //SystemSetting.UserSetting.defaultSetting.indoorCoolingRH = ConvertStringToDouble(jctbIndoorCoolingRH.Text);
            //SystemSetting.UserSetting.defaultSetting.outdoorHeatingRH = ConvertStringToDouble(jctbOutdoorHeatingRH.Text);
            //SystemSetting.UserSetting.defaultSetting.roomHeight = ConvertStringToDouble(jctbRoomHeight.Text);

            //SystemSetting.UserSetting.advancedSetting.indoorCooling = trackBarInC.Value;
            //SystemSetting.UserSetting.advancedSetting.indoorHeating = trackBarInH.Value;
            //SystemSetting.UserSetting.advancedSetting.outdoorCooling = trackBarOutC.Value;
            //SystemSetting.UserSetting.advancedSetting.outdoorHeating = trackBarOutH.Value;


            //if (System.IO.Directory.Exists(lbDXFPath.Text))
            //{
            //    SystemSetting.UserSetting.fileSetting.DXFFiles = DXFPath;
            //}
            //else
            //{
            //    SystemSetting.UserSetting.fileSetting.DXFFiles = "";
            //}

            //if (System.IO.Directory.Exists(lbReportPath.Text))
            //{
            //    SystemSetting.UserSetting.fileSetting.reportFiles = reportPath;
            //}
            //else
            //{
            //    SystemSetting.UserSetting.fileSetting.reportFiles = "";
            //}
            //string strLanguage = LangType.ENGLISH;
            //string strSwitchLanguage = "tbtnLanguage_en";
            //if (jcCoboxLanguage.Text == "中文(简体)")
            //{
            //    strLanguage = LangType.CHINESE;
            //    strSwitchLanguage = "tbtnLanguage_zh";
            //}
            //else if (jcCoboxLanguage.Text == "中文(繁體)")
            //{
            //    strLanguage = LangType.CHINESE_TRADITIONAL;
            //    strSwitchLanguage = "tbtnLanguage_zht";
            //}
            //else if (jcCoboxLanguage.Text == "Français")
            //{
            //    strLanguage = LangType.FRENCH;
            //    strSwitchLanguage = "tbtnLanguage_fr";
            //}
            //else if (jcCoboxLanguage.Text == "España")
            //{
            //    strLanguage = LangType.SPANISH;
            //    strSwitchLanguage = "tbtnLanguage_es";
            //}
            //else if (jcCoboxLanguage.Text == "Türk")
            //{
            //    strLanguage = LangType.TURKISH;
            //    strSwitchLanguage = "tbtnLanguage_tr";
            //}
            //else if (jcCoboxLanguage.Text == "Deutsch")
            //{
            //    strLanguage = LangType.GERMANY;
            //    strSwitchLanguage = "tbtnLanguage_de";
            //}
            //else if (jcCoboxLanguage.Text == "Italiano")
            //{
            //    strLanguage = LangType.ITALIAN;
            //    strSwitchLanguage = "tbtnLanguage_it";
            //}
            //else if (jcCoboxLanguage.Text == "Brazilian Portuguese")
            //{
            //    strLanguage = LangType.BRAZILIAN_PORTUGUESS;
            //    strSwitchLanguage = "tbtnLanguage_pt_BR";
            //}
            //else
            //{
            //    strLanguage = LangType.ENGLISH;
            //    strSwitchLanguage = "tbtnLanguage_en";
            //}
            //CurrLanguage = strSwitchLanguage;
            //SystemSetting.UserSetting.fileSetting.settingLanguage = strLanguage;


            int doNotRemovethis = 0;

        }

        #endregion

        #region Location

        JCHVRF.BLL.RegionBLL objRegionBll = new JCHVRF.BLL.RegionBLL();
        private List<ComboBox> _listRegion;
        public List<ComboBox> ListRegion
        {
            get
            {
                return _listRegion;
            }
            set
            {
                this.SetValue(ref _listRegion, value);
            }
        }

        private string _selectedRegion;

        public string SelectedRegionCode
        {
            get { return _selectedRegion; }
            set { this.SetValue(ref _selectedRegion, value); }
        }


        private string _selectedSubRegion;

        public string SelectedSubRegionCode
        {
            get { return _selectedSubRegion; }
            set { this.SetValue(ref _selectedSubRegion, value); }
        }

        private string _selectedLanguage;

        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.SetValue(ref _selectedLanguage, value); }
        }

        public bool EnableFlag
        {
            get { return enableFlag; }
            set { this.SetValue(ref enableFlag, value); }
        }


        public void BindRegion()
        {
            ListRegion = new List<ComboBox>();
            DataTable dtRegion = objRegionBll.GetParentRegionTable();
            var EnumerableData = dtRegion.AsEnumerable().Select(r => new ComboBox
            {
                DisplayName = r.Field<string>("Region"),
                Value = r.Field<string>("Code"),
            });
            ListRegion = EnumerableData.ToList();
            //if(SelectedRegionCode==string.Empty)
            SelectedRegionCode = ListRegion.FirstOrDefault().Value;
        }

        private List<ComboBox> _listSubRegion;
        private IGlobalProperties _globalProperties;

        public List<ComboBox> ListSubRegion
        {
            get
            {
                return _listSubRegion;
            }
            set
            {
                this.SetValue(ref _listSubRegion, value);
            }
        }

        public void BindSubRegion(string SeletedRegionCode)
        {
            ListSubRegion = new List<ComboBox>();
            DataTable dtSubRegion = objRegionBll.GetSubRegionList(SeletedRegionCode);
            var EnumerableData = dtSubRegion.AsEnumerable().Select(r => new ComboBox
            {
                DisplayName = r.Field<string>("Region"),
                Value = r.Field<string>("Code"),
            });
            ListSubRegion = EnumerableData.ToList();
            SelectedSubRegionCode = ListSubRegion.FirstOrDefault().Value;
        }

        private void OnRegionChangeCommmand()
        {
            BindSubRegion(SelectedRegionCode);
        }
        private void OnSubRegionChangeCommmand()
        {
            //SetRegionSubRegionCode();
        }

        private void OnInfoCircleMouseDown() { JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_SELECT_REGION")); }//Select the Region & Sub- Region for which you want to create a project


        #endregion
        #region Properties

        /// <summary>
        /// Gets or sets the NamePrefixData
        /// </summary>
        public NamePrefixModel NamePrefixData
        {
            get { return _namePrefixData; }
            set { this.SetValue(ref _namePrefixData, value); }
        }

        /// <summary>
        /// Gets or sets the list of active languages
        /// </summary>
        public List<Language> ActiveLanguages
        {
            get
            {
                return _activelanguages;
            }
            set { this.SetValue(ref _activelanguages, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedAirflowUnit
        /// </summary>
        public AirflowUnit SelectedAirflowUnit
        {
            get { return _selectedAirflowUnit; }
            set { this.SetValue(ref _selectedAirflowUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedAreaUnit
        /// </summary>
        public AreaUnit SelectedAreaUnit
        {
            get { return this._selectedAreaUnit; }
            set { this.SetValue(ref _selectedAreaUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedCapacityUnit
        /// </summary>
        public CapacityUnit SelectedCapacityUnit
        {
            get { return _selectedCapacityUnit; }
            set { this.SetValue(ref _selectedCapacityUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedDimensionsUnit
        /// </summary>
        public DimensionsUnit SelectedDimensionsUnit
        {
            get { return this._selectedDimensionsUnit; }
            set { this.SetValue(ref _selectedDimensionsUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedLengthUnit
        /// </summary>
        public LengthUnit SelectedLengthUnit
        {
            get { return this._selectedLengthUnit; }
            set { this.SetValue(ref _selectedLengthUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedLoadIndexUnit
        /// </summary>
        public LoadIndexUnit SelectedLoadIndexUnit
        {
            get { return this._selectedLoadIndexUnit; }
            set { this.SetValue(ref _selectedLoadIndexUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedTabIndex
        /// </summary>
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { this.SetValue(ref _selectedTabIndex, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedTemperatureUnit
        /// </summary>
        public TemperatureUnit SelectedTemperatureUnit
        {
            get { return this._selectedTemperatureUnit; }
            set { this.SetValue(ref _selectedTemperatureUnit, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedWaterFlowRateUnit
        /// </summary>
        public WaterFlowRateUnit SelectedWaterFlowRateUnit
        {
            get { return this._selectedWaterFlowRateUnit; }
            set { this.SetValue(ref _selectedWaterFlowRateUnit, value); }
        }

        public ESP ESPUnit
        {
            get { return this._eSPUnit; }
            set { this.SetValue(ref _eSPUnit, value); }
        }
        /// <summary>
        /// Gets or sets the SelectedWeightUnit
        /// </summary>
        public WeightUnit SelectedWeightUnit
        {
            get { return this._selectedWeightUnit; }
            set { this.SetValue(ref _selectedWeightUnit, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The GetAllDefaultNamePrefix
        /// </summary>
        /// <returns>The <see cref="NamePrefixModel"/></returns>
        public NamePrefixModel GetAllDefaultNamePrefix()
        {
            try
            {
                _namePrefixData = new JCHVRF_New.Model.NamePrefixModel();
                //DataTable defaultnameprefixlist = objNamePrefixDAL.getNamePrefixData(); For now we do not have a DB table to return anything

                //if (defaultnameprefixlist.Rows.Count == 0 || defaultnameprefixlist == null)
                //{
                _namePrefixData = NamePrefixDefaultValueUtility.GetAllDefaultNamePrefixValues();
                return _namePrefixData;
                // }
                //else
                //{
                //    _namePrefixData.BuildingName = defaultnameprefixlist.Columns[0].ToString();
                //    _namePrefixData.FloorName = defaultnameprefixlist.Columns[1].ToString();
                //    _namePrefixData.RoomName = defaultnameprefixlist.Columns[2].ToString();
                //    _namePrefixData.IndoorUnitsName = defaultnameprefixlist.Columns[3].ToString();
                //    _namePrefixData.OutdoorUnitName = defaultnameprefixlist.Columns[4].ToString();
                //    _namePrefixData.SystemName = defaultnameprefixlist.Columns[5].ToString();
                //    _namePrefixData.Controllers = defaultnameprefixlist.Columns[6].ToString();
                //    _namePrefixData.TotalHeatExchangers = defaultnameprefixlist.Columns[7].ToString();
                //    return _namePrefixData;
                //}
            }

            catch (Exception e)

            {
                JCHMessageBox.Show("");
                return null;
            }
        }

        /// <summary>
        /// The SaveNamePrefix
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool SaveNamePrefix()
        {
            try
            {
                _namePrefixData = new NamePrefixModel();
                result = objNamePrefixDAL.InsertNamePrefixData(_namePrefixData.BuildingName, _namePrefixData.FloorName, _namePrefixData.RoomName,
                                                                _namePrefixData.IndoorUnitsName, _namePrefixData.OutdoorUnitName, _namePrefixData.SystemName,
                                                                _namePrefixData.Controllers, _namePrefixData.TotalHeatExchangers);
                return result;
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
                JCHMessageBox.Show("");
                return false;
            }
        }

        /// <summary>
        /// The UpdateNamePrefix
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        //public bool UpdateNamePrefix()
        //{
        //    try
        //    {
        //        List<string> nameprefixlist = new List<string>();
        //        nameprefixlist.Add(_namePrefixData.BuildingName);
        //        nameprefixlist.Add(_namePrefixData.FloorName);
        //        nameprefixlist.Add(_namePrefixData.RoomName);
        //        nameprefixlist.Add(_namePrefixData.IndoorUnitsName);
        //        nameprefixlist.Add(_namePrefixData.OutdoorUnitName);
        //        nameprefixlist.Add(_namePrefixData.SystemName);
        //        nameprefixlist.Add(_namePrefixData.Controllers);
        //        nameprefixlist.Add(_namePrefixData.TotalHeatExchangers);

        //        result = objNamePrefixDAL.updateNamePrefixData("shweta", "India", nameprefixlist);
        //        return result;
        //    }

        //    catch (Exception e)

        //    {
        //        JCHMessageBox.Show("");
        //        return false;
        //    }
        //}//Commented by Alok coz 0 reference

        /// <summary>
        /// The LoadData
        /// </summary>
        private void LoadData()
        {
            //_switchingTemperatureUnit = false;
            //_loading = true;

            switch (SystemSetting.UserSetting.unitsSetting.settingPOWER)
            {
                case Unit.ut_Capacity_kw:
                    SelectedCapacityUnit = CapacityUnit.kw;
                    break;
                case Unit.ut_Capacity_btu:
                    SelectedCapacityUnit = CapacityUnit.btu;
                    break;
                case Unit.ut_Capacity_ton:
                    SelectedCapacityUnit = CapacityUnit.ton;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingAIRFLOW)
            {
                case Unit.ut_Airflow_ls:
                    SelectedAirflowUnit = AirflowUnit.ls;
                    break;
                case Unit.ut_Airflow_m3h:
                    SelectedAirflowUnit = AirflowUnit.m3h;
                    break;
                case Unit.ut_Airflow_m3hr:
                    SelectedAirflowUnit = AirflowUnit.m3hr;
                    break;
                case Unit.ut_Airflow_cfm:
                    SelectedAirflowUnit = AirflowUnit.cfm;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE)
            {
                case Unit.ut_Temperature_c:
                    SelectedTemperatureUnit = TemperatureUnit.C;
                    break;
                case Unit.ut_Temperature_f:
                    SelectedTemperatureUnit = TemperatureUnit.F;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingLENGTH)
            {
                case Unit.ut_Size_m:
                    SelectedLengthUnit = LengthUnit.m;
                    break;
                case Unit.ut_Size_ft:
                    SelectedLengthUnit = LengthUnit.ft;
                    break;
            }
            //Earlier settingDimension was for Piping Dimension and settingDimensionUnit for Unit Dimension
            //Now only 1 Dimension Settings, So settingDimensionUnit tag in settings file will become irrelevant.
            switch (SystemSetting.UserSetting.unitsSetting.settingDimension)
            {
                case Unit.ut_Dimension_mm:
                    SelectedDimensionsUnit = DimensionsUnit.mm;
                    break;
                case Unit.ut_Dimension_inch:
                    SelectedDimensionsUnit = DimensionsUnit.inch;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingWEIGHT)
            {
                case Unit.ut_Weight_kg:
                    SelectedWeightUnit = WeightUnit.kg;
                    break;
                case Unit.ut_Weight_lbs:
                    SelectedWeightUnit = WeightUnit.lbs;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingAREA)
            {
                case Unit.ut_Area_m2:
                    SelectedAreaUnit = AreaUnit.m2;
                    break;
                case Unit.ut_Area_ft2:
                    SelectedAreaUnit = AreaUnit.ft2;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate)
            {
                case Unit.ut_WaterFlowRate_m3h:
                    SelectedWaterFlowRateUnit = WaterFlowRateUnit.m3h;
                    break;
                case Unit.ut_WaterFlowRate_lmin:
                    SelectedWaterFlowRateUnit = WaterFlowRateUnit.lmin;
                    break;
            }

            switch (SystemSetting.UserSetting.unitsSetting.settingLOADINDEX)
            {
                case Unit.ut_LoadIndex_w:
                    SelectedLoadIndexUnit = LoadIndexUnit.Wm2;
                    break;
                case Unit.ut_LoadIndex_MBH:
                    SelectedLoadIndexUnit = LoadIndexUnit.MBH;
                    break;
            }
            switch (SystemSetting.UserSetting.unitsSetting.settingESP)
            {
                case Unit.ut_Pressure:
                    ESPUnit = ESP.Pa;
                    break;
                case Unit.ut_Pressure_inWG:
                    ESPUnit = ESP.InWG;
                    break;
            }
            //NamePrefixSettings

            _namePrefixData.BuildingName = SystemSetting.UserSetting.defaultSetting.BuildingName;
            _namePrefixData.FloorName = SystemSetting.UserSetting.defaultSetting.FloorName;
            _namePrefixData.RoomName = SystemSetting.UserSetting.defaultSetting.RoomName;
            _namePrefixData.SystemName = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName;
            _namePrefixData.IndoorUnitsName = SystemSetting.UserSetting.defaultSetting.IndoorName;
            _namePrefixData.OutdoorUnitName = SystemSetting.UserSetting.defaultSetting.OutdoorName;
            _namePrefixData.Controllers = SystemSetting.UserSetting.defaultSetting.ControllerName;
            _namePrefixData.TotalHeatExchangers = SystemSetting.UserSetting.defaultSetting.ExchangerName;
            SelectedRegionCode = SystemSetting.UserSetting.locationSetting.region;
            SelectedSubRegionCode = SystemSetting.UserSetting.locationSetting.subRegion;
            SelectedLanguage = SystemSetting.UserSetting.defaultSetting.Language;

            // SelectedRegionCode = SystemSetting.UserSetting.locationSetting.region;
            //SelectedSubRegionCode = SystemSetting.UserSetting.locationSetting.subRegion;

            //string temperatureUnit = Unit.ut_Temperature_c;
            //if (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE != Unit.ut_Temperature_c)
            //{
            //    temperatureUnit = Unit.ut_Temperature_f;
            //}

            //jclblIndoorCoolingDB.Text = temperatureUnit;
            //jclblIndoorCoolingWB.Text = temperatureUnit;
            //jclblIndoorHeatingDB.Text = temperatureUnit;
            //jclblOutdoorCoolingDB.Text = temperatureUnit;
            //jclblOutdoorHeatingDB.Text = temperatureUnit;
            //jclblOutdoorHeatingWB.Text = temperatureUnit;

            //jctbIndoorCoolingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.indoorCoolingDB,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            //jctbIndoorCoolingWB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.indoorCoolingWB,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            //jctbIndoorHeatingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.indoorHeatingDB,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            //jctbOutdoorCoolingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            //jctbOutdoorHeatingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.outdoorHeatingDB,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            //jctbOutdoorHeatingWB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");

            //jclblOutdoorCoolingIW.Text = temperatureUnit;
            //jclblOutdoorHeatingIW.Text = temperatureUnit;
            //jctbOutdoorCoolingIW.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.outdoorCoolingIW,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            //jctbOutdoorHeatingIW.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.outdoorHeatingIW,
            //                                                 UnitType.TEMPERATURE, temperatureUnit).ToString("n1");

            //calRH((decimal)SystemSetting.UserSetting.defaultSetting.indoorCoolingDB, (decimal)SystemSetting.UserSetting.defaultSetting.indoorCoolingWB, true);
            //calRH((decimal)SystemSetting.UserSetting.defaultSetting.outdoorHeatingDB, (decimal)SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB, false);

            //jctbRoomHeight.Text = SystemSetting.UserSetting.defaultSetting.roomHeight.ToString("n1");

            //trackBarInC.Value = SystemSetting.UserSetting.advancedSetting.indoorCooling;
            //trackBarInH.Value = SystemSetting.UserSetting.advancedSetting.indoorHeating;
            //trackBarOutC.Value = SystemSetting.UserSetting.advancedSetting.outdoorCooling;
            //trackBarOutH.Value = SystemSetting.UserSetting.advancedSetting.outdoorHeating;

            //if (!System.IO.Directory.Exists(SystemSetting.UserSetting.fileSetting.DXFFiles))
            //{
            //    SystemSetting.UserSetting.fileSetting.DXFFiles = "C:\\GlobalVRF";
            //    lbDXFPath.Text = Msg.GetResourceString("Msg_DXFPath");
            //}
            //lbDXFPath.Text = SystemSetting.UserSetting.fileSetting.DXFFiles;
            //DXFPath = SystemSetting.UserSetting.fileSetting.DXFFiles;

            //if (!System.IO.Directory.Exists(SystemSetting.UserSetting.fileSetting.reportFiles))
            //{
            //    SystemSetting.UserSetting.fileSetting.reportFiles = "C:\\GlobalVRF";
            //    lbReportPath.Text = Msg.GetResourceString("Msg_ReportPath");
            //}
            //lbReportPath.Text = SystemSetting.UserSetting.fileSetting.reportFiles;
            //reportPath = SystemSetting.UserSetting.fileSetting.reportFiles;

            //this.jccbAltitude.Checked = thisProject.EnableAltitudeCorrectionFactor;

            //_loading = false;
            //_switchingTemperatureUnit = false;


            int doNotRemovethis = 0;

        }

        public DelegateCommand SaveClickCommand { get; set; }

        /// <summary>
        /// The OnSettingsOpenedFromSideDrawer
        /// </summary>
        /// <param name="item">The item<see cref="ISideBarItem"/></param>
        private void OnSettingsOpenedFromSideDrawer(ISideBarItem item)
        {
            switch (item.HeaderKey)
            {
                case "GENERAL":
                    SelectedTabIndex = 0;
                    return;
                case "MY_ACCOUNT":
                    SelectedTabIndex = 1;
                    return;
                case "LOCATION":
                    SelectedTabIndex = 2;
                    return;
                case "PAGE_VIEW":
                    SelectedTabIndex = 3;
                    return;
                case "USERS":
                    SelectedTabIndex = 4;
                    return;
                case "REPORT_LAYOUT":
                    SelectedTabIndex = 5;
                    return;
                case "NOTIFICATIONS":
                    SelectedTabIndex = 6;
                    return;
                case "MEASUREMENT_UNIT":
                    SelectedTabIndex = 7;
                    return;
                case "NAME_PREFIXES":
                    SelectedTabIndex = 8;
                    return;
            }
            SelectedTabIndex = 0;
        }

        /// <summary>
        /// This will validate all tabs data in settings.
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            bool isValid = true;

            //TO Do Later, :D No validations for Measurement Unit Settings.

            //int byteLimit = 15;
            //if (jctbBuildingName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_BuildName")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbBuildingName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_BuildName"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbFloorName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_FloorName")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbFloorName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_FloorName"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbRoomName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_RoomName")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbRoomName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_RoomName"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbFreshAirAreaName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_FreshAirAreaName")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbFreshAirAreaName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_FreshAirAreaName"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbIndoorName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorUnitsName")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbIndoorName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_IndoorUnitsName"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbOutdoorName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorUnitsName")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbOutdoorName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_OutdoorUnitsName"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbControlName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_Controllers")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbControlName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_Controllers"), byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}


            //if (jctbExchangerName.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(this.jcLabel16.Text));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (getBytes(jctbExchangerName.Text) > byteLimit)
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_BYTES_LIMITATION(this.jcLabel16.Text, byteLimit));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //double outData = 0;
            //if (jctbIndoorCoolingDB.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorCoolingDB")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbIndoorCoolingDB.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorCoolingDB")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbIndoorCoolingWB.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorCoolingWB")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbIndoorCoolingWB.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorCoolingWB")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbIndoorCoolingRH.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorCoolingRH")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbIndoorCoolingRH.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorCoolingRH")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbIndoorHeatingDB.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorHeatingDB")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbIndoorHeatingDB.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorHeatingDB")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbOutdoorCoolingDB.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorCoolingDB")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbOutdoorCoolingDB.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorCoolingDB")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbOutdoorHeatingDB.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorHeatingDB")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbOutdoorHeatingDB.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorHeatingDB")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbOutdoorHeatingRH.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorHeatingRH")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbOutdoorHeatingRH.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorHeatingRH")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}
            ////add water source inlet water tempertaure set 20160615 by Yunxiao Lin
            //if (jctbOutdoorCoolingIW.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorCoolingIW")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbOutdoorCoolingIW.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorCoolingIW")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbOutdoorHeatingIW.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorHeatingIW")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbOutdoorHeatingIW.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorHeatingIW")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (jctbRoomHeight.Text == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_RoomHeight")));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}
            //else
            //{
            //    if (!double.TryParse(jctbRoomHeight.Text, out outData))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_RoomHeight")));
            //        tcSetting.SelectedIndex = 1;
            //        return false;
            //    }
            //}

            //if (!JCValidateGroup(pnlIndoorWorkingCondition))
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_PAYATTENTION);
            //    return false;
            //}
            //if (!JCValidateGroup(pnlOutdoorWorkingCondition))
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_PAYATTENTION);
            //    return false;
            //}
            //if (Convert.ToDecimal(jctbIndoorCoolingDB.Text) < Convert.ToDecimal(jctbIndoorCoolingWB.Text))
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}

            //if (Convert.ToDecimal(jctbOutdoorHeatingDB.Text) < Convert.ToDecimal(jctbOutdoorHeatingWB.Text))
            //{
            //    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
            //    tcSetting.SelectedIndex = 1;
            //    return false;
            //}

            //if (DXFPath == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.FILE_WARN_SELECTPATH("DXF"));
            //    tcSetting.SelectedIndex = 3;
            //    return false;
            //}
            //else
            //{
            //    if (!System.IO.Directory.Exists(DXFPath))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.FILE_WARN_NOTEXIST1(Msg.GetResourceString("Msg_DXFFilesDirectionary")));
            //        tcSetting.SelectedIndex = 3;
            //        return false;
            //    }
            //}

            //if (reportPath == "")
            //{
            //    JCHMessageBox.ShowWarning(Msg.FILE_WARN_SELECTPATH(Msg.GetResourceString("Msg_Report")));
            //    tcSetting.SelectedIndex = 3;
            //    return false;
            //}
            //else
            //{
            //    if (!System.IO.Directory.Exists(reportPath))
            //    {
            //        JCHMessageBox.ShowWarning(Msg.FILE_WARN_NOTEXIST1(Msg.GetResourceString("Msg_ReportFilesDirectionary")));
            //        tcSetting.SelectedIndex = 3;
            //        return false;
            //    }
            //}

            return isValid;
        }

        public void loadLanguageRadioButtons()
        {
            RegionBLL regionbll = new RegionBLL();
            ActiveLanguages = new List<Language>();
            bool IsSelected = false;
            DataTable dtsql = regionbll.GetActiveLanguage();
            for (int i = 0; i < dtsql.Rows.Count; i++)
            {
                string lang = dtsql.Rows[i]["LanguageDisplayName"].ToString();
                string langcode = dtsql.Rows[i]["LangCode"].ToString();
                if (lang != SystemSetting.UserSetting.defaultSetting.Language)
                    IsSelected = false;
                else
                {
                    IsSelected = true;
                    SystemSetting.UserSetting.defaultSetting.LanguageCode = langcode;
                }

                ActiveLanguages.Add(new Language
                {
                    IsSelected = IsSelected,
                    LanguageName = lang,
                    LanguageCode = langcode
                });
            }
        }
        #endregion
    }
}
