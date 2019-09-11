using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using JCBase.Utility;
using JCHVRF.Model;
using System.Collections.Generic;
using Prism.Events;
using System.Data;
using System.Windows.Media;
using System.IO;
using JCBase.UI;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Constants;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Contracts;
using Prism.Regions;
using System.Linq;

//ACC - RAG 
//Used in System -> Property menu for Total Heat Exchanger

namespace JCHVRF_New.ViewModels
{
    class TotalHeatExchDetailsInfoViewModel : ViewModelBase
    {
        //string unitNameSelected = null;
        private IEventAggregator _eventAggregator;
        private Project _project;
        JCHVRF.BLL.IndoorBLL _bll;
        Project thisProject;
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string utArea = SystemSetting.UserSetting.unitsSetting.settingAREA;

        private RoomIndoor proj;

        public DelegateCommand NumericOutdoorCDBCommand { get; set; }
        public DelegateCommand NumericOutdoorCWBCommand { get; set; }
        public DelegateCommand NumericOutdoorHDBCommand { get; set; }
        public DelegateCommand NumericRHommand { get; set; }
        public DelegateCommand AddFloorCommand { get; set; }
        public DelegateCommand AddEditRoomCommand { get; set; }
        public DelegateCommand ChangeTempCommand { get; set; }
        public DelegateCommand ValidateUnitNameCommand { get; set; }

        private SystemBase _hvacSystem;

        public SystemBase HvacSystem
        {
            get { return _hvacSystem; }
            set
            {
                SetValue(ref _hvacSystem, value);
                Initialize();
            }
        }

        public TotalHeatExchDetailsInfoViewModel(IEventAggregator EventAggregator, IModalWindowService winService, JCHVRF_New.Model.LightProject thisProj)
        {
            try
            {
                NumericOutdoorCDBCommand = new DelegateCommand(NumericOutdoorCDB_LostFocus);
                NumericOutdoorCWBCommand = new DelegateCommand(NumericOutdoorCWBCommand_LostFocus);
                NumericOutdoorHDBCommand = new DelegateCommand(NumericOutdoorHDBCommand_LostFocus);
                NumericRHommand = new DelegateCommand(NumericRHommand_LostFocus);
                ValidateUnitNameCommand = new DelegateCommand(ValidateUnitNameOnLostFocus);
                _eventAggregator = EventAggregator;
                _winService = winService;
                _bll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                ChangeTempCommand = new DelegateCommand(btnChangeTempUClicked);
                _eventAggregator.GetEvent<RoomListSaveSubscriber>().Subscribe(GetRoomList);
                AddFloorCommand = new DelegateCommand(OnAddFloorClicked);
                AddEditRoomCommand = new DelegateCommand(OnAddEditRoomClicked);
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Subscribe(OpenGetFloorList);
                _eventAggregator.GetEvent<CleanupSystemWizard>().Subscribe(OnCleanup);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private bool IsSaveButtonEnabled()
        {
            if ((ValidateUnitName() == false) || (ValidateCoolWetBulb() == false) || (ValidateHDDBT() == false) || (ValidateOdb() == false) || (ValidateOutdoorHDRH() == false))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void NumericOutdoorCDB_LostFocus()
        {
            if (ValidateOdb() == false || IsSaveButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(false);

            }
            else
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(true);

            }
        }

        private void NumericOutdoorCWBCommand_LostFocus()
        {
            if (ValidateCoolWetBulb() == false || IsSaveButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(true);

            }
        }

        private void NumericOutdoorHDBCommand_LostFocus()
        {
            if (ValidateHDDBT() == false || IsSaveButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(false);

            }
            else
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(true);

            }
        }

        private void NumericRHommand_LostFocus()
        {
            if (ValidateOutdoorHDRH() == false || IsSaveButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(false);

            }
            else
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(true);

            }
        }

        private void ValidateUnitNameOnLostFocus()
        {
            if (ValidateUnitName() == false || IsSaveButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<SaveButtonEnability>().Publish(true);
            }
        }

        #region label outdoor value
        private string _lbloutdoorCoolingDB;
        public string lbloutdoorCoolingDB
        {
            get
            {
                return _lbloutdoorCoolingDB;
            }
            set
            {
                this.SetValue(ref _lbloutdoorCoolingDB, value);
            }
        }
        private string _lbloutdoorHeatingDB;
        public string lbloutdoorHeatingDB
        {
            get
            {
                return _lbloutdoorHeatingDB;
            }
            set
            {
                this.SetValue(ref _lbloutdoorHeatingDB, value);
            }
        }//= 7.0m;

        private string _lbloutdoorHeatingRH;
        public string lbloutdoorHeatingRH
        {
            get
            {
                return _lbloutdoorHeatingRH;
            }
            set
            {
                this.SetValue(ref _lbloutdoorHeatingRH, value);
            }
        }

        private string _lblindoorCoolingWB;
        public string lblindoorCoolingWB
        {
            get
            {
                return _lblindoorCoolingWB;
            }
            set
            {
                this.SetValue(ref _lblindoorCoolingWB, value);
            }
        }

        private string _Errormsg;
        public string DCErrorMessage
        {
            get { return _Errormsg; }
            set
            {
                this.SetValue(ref _Errormsg, value);
                RaisePropertyChanged("IsError");
            }
        }
        public bool IsError
        {
            get { return !string.IsNullOrEmpty(DCErrorMessage); }
        }

        public string ErrorMessage { get; private set; }

        #endregion label outdoor value

        //Outdoor Cooling Dry Bulb
        private bool ValidateOdb()
        {
            double nOdb = Convert.ToDouble(outdoorCoolingDB); //Convert.ToDouble(outdoorCoolingDB);

            if ((nOdb >= Unit.ConvertToControl(10.0, UnitType.TEMPERATURE, CurrentTempUnit)) && (nOdb <= Unit.ConvertToControl(43.0, UnitType.TEMPERATURE, CurrentTempUnit)))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorCoolingDB = string.Empty;
                return true;

            }
            else
            {
                lbloutdoorCoolingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(43, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[16,30]";
                DCErrorMessage = Langauge.Current.GetMessage("ALERT_TEMPERATURE_VALUE"); 
                return false;
            }

        }

        //Outdoor Cooling Wet Bulb
        private bool ValidateCoolWetBulb()
        {
            double nCWBVal = Convert.ToDouble(outdoorCoolingWB); //Convert.ToDouble(indoorCoolingWB);

            if ((nCWBVal >= Unit.ConvertToControl(-20.0, UnitType.TEMPERATURE, CurrentTempUnit)) && (nCWBVal <= Unit.ConvertToControl(15.0, UnitType.TEMPERATURE, CurrentTempUnit)))
            {
                DCErrorMessage = string.Empty;
                lblindoorCoolingWB = string.Empty;

                return true;

            }
            else
            {
                lblindoorCoolingWB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-20, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(15, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[-20, 15]";
                DCErrorMessage = Langauge.Current.GetMessage("ALERT_TEMPERATURE_VALUE"); 

                return false;
            }


        }

        //Outdoor Heating Dry Bulb
        private bool ValidateHDDBT()
        {
            double nOHDDBT = Convert.ToDouble(outdoorHeatingDB);

            if ((nOHDDBT >= Unit.ConvertToControl(-18.0, UnitType.TEMPERATURE, CurrentTempUnit)) && (nOHDDBT <= Unit.ConvertToControl(33.0, UnitType.TEMPERATURE, CurrentTempUnit)))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorHeatingDB = string.Empty;

                return true;

            }
            else
            {
                lbloutdoorHeatingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-18, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(33, UnitType.TEMPERATURE, CurrentTempUnit));
                DCErrorMessage = Langauge.Current.GetMessage("ALERT_TEMPERATURE_VALUE");
                return false;
            }
        }

        //Outdoor RH
        private bool ValidateOutdoorHDRH()
        {
            double nOdb = Convert.ToDouble(outdoorHeatingRH);
            if ((nOdb >= 13.0) && (nOdb <= 100.0))
            {
                lbloutdoorHeatingRH = string.Empty;
                DCErrorMessage = string.Empty;
                return true;
            }
            else
            {
                lbloutdoorHeatingRH = string.Format("Range[{0}, {1}]", 13, 100);//"Range[13, 100]";
                DCErrorMessage = Langauge.Current.GetMessage("ALERT_HUMIDITY_VALUE");
                return false;
            }
        }

        //Unit Name
        private bool ValidateUnitName()
        {
            string uName = UnitName;
            if (string.IsNullOrEmpty(uName))
            {
                DCErrorMessage = Langauge.Current.GetMessage("ALERT_HEATEXCHANGER_UNITNAME");
                return false;
            }
            else if (uName.HasSpecialCharacters())
            {
                DCErrorMessage = Langauge.Current.GetMessage("ERROR_SPECIAL_CHARACTERS");
                return false;
            }
            else
            {
                DCErrorMessage = string.Empty;
                return true;
            }
        }

        private void OnCleanup()
        {
            try

            {
                _eventAggregator.GetEvent<RoomListSaveSubscriber>().Unsubscribe(GetRoomList);
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Unsubscribe(OpenGetFloorList);
                _eventAggregator.GetEvent<BeforeHESave>().Unsubscribe(OnBeforeSave);
                _eventAggregator.GetEvent<Cleanup>().Unsubscribe(OnCleanup);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void Initialize()
        {
            _eventAggregator.GetEvent<BeforeHESave>().Subscribe(OnBeforeSave);
            GetHeatExchangerSystemProperties();
            GetRoomList();
            BindFloor();
            BindFanSpeed();
            BindSeries();
            UpdateUnitName();
        }

        private SeriesModel _selectedSeries;
        public SeriesModel SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                this.SetValue(ref _selectedSeries, value);

                BindPowerMode();
            }
        }

        private PowerModel _selectedPower;
        public PowerModel SelectedPower
        {
            get { return _selectedPower; }
            set
            {
                this.SetValue(ref _selectedPower, value);
            }
        }

        private double _eSPVal;
        public double ESPVal
        {
            get { return _eSPVal; }
            set { this.SetValue(ref _eSPVal, value); }
        }

        private double _area;
        public double Area
        {
            get { return _area; }
            set { this.SetValue(ref _area, value); }
        }

        private string _unitName;
        public string UnitName
        {
            get { return _unitName; }
            set
            {
                this.SetValue(ref _unitName, value);
            }
        }

        private double _freshAir;
        public double FreshAir
        {
            get
            {
                return _freshAir;
            }
            set
            {
                this.SetValue(ref _freshAir, value);
            }
        }

        private int _noOfPeople;
        public int NoOfPeople
        {
            get
            {
                return _noOfPeople;
            }
            set
            {
                this.SetValue(ref _noOfPeople, value);
            }
        }

        private FanSpeed _sfanspeed;
        public FanSpeed SFanSpeed
        {
            get { return _sfanspeed; }
            set
            {
                this.SetValue(ref _sfanspeed, value);
            }
        }

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

        private string _currentFreshAirUnit;
        public string CurrentFreshAirUnit
        {
            get { return SystemSetting.UserSetting.unitsSetting.settingAIRFLOW; }
            set
            {
                SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = value;
                this.SetValue(ref _currentFreshAirUnit, value);
            }
        }

        private string _currentESPUnit;
        public string CurrentESPUnit
        {
            get { return SystemSetting.UserSetting.unitsSetting.settingPressure; }
            set
            {
                SystemSetting.UserSetting.unitsSetting.settingPressure = value;
                this.SetValue(ref _currentESPUnit, value);
            }
        }

        private string _currentAreaUnit;
        public string CurrentAreaUnit
        {
            get { return SystemSetting.UserSetting.unitsSetting.settingAREA; }
            set
            {
                SystemSetting.UserSetting.unitsSetting.settingAREA = value;
                this.SetValue(ref _currentAreaUnit, value);
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

        private double _outdoorCoolingDB;
        public double outdoorCoolingDB
        {
            get
            {
                return _outdoorCoolingDB;
            }
            set
            {
                this.SetValue(ref _outdoorCoolingDB, value);
            }
        }


        private double _outdoorCoolingWB;
        public double outdoorCoolingWB
        {
            get
            {
                return _outdoorCoolingWB;
            }
            set
            {
                this.SetValue(ref _outdoorCoolingWB, value);
            }
        }

        private double _outdoorHeatingDB;
        public double outdoorHeatingDB
        {
            get
            {
                return _outdoorHeatingDB;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingDB, value);
            }
        }

        private double _outdoorHeatingRH;
        public double outdoorHeatingRH
        {
            get
            {
                return _outdoorHeatingRH;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingRH, value);
            }
        }

        private Room _SelectedRoom;
        public Room SelectedRoom
        {
            get { return _SelectedRoom; }
            set
            {
                this.SetValue(ref _SelectedRoom, value);
                UpdateUnitName();
                if (IsSaveButtonEnabled())
                {
                    _eventAggregator.GetEvent<SaveButtonEnability>().Publish(true);
                }
                else
                {
                    _eventAggregator.GetEvent<SaveButtonEnability>().Publish(false);
                }
            }
        }

        private Floor _sIndex;
        public Floor SelectedFloor
        {
            get { return _sIndex; }
            set
            {
                this.SetValue(ref _sIndex, value);
            }
        }

        private List<string> _unitNameList;

        public List<string> UnitNameList
        {
            get { return _unitNameList; }
            set
            {
                this.SetValue(ref _unitNameList, value);
            }
        }



        private string _defaultRoom;
        public string DefaultRoom
        {
            get { return _defaultRoom; }
            set
            {

                this.SetValue(ref _defaultRoom, value);

            }
        }

        private string _defaultFloor;
        public string DefaultFloor
        {
            get { return _defaultFloor; }
            set
            {

                this.SetValue(ref _defaultFloor, value);

            }
        }

        private string _defaultFanSpeed;
        public string DefaultFanSpeed
        {
            get { return _defaultFanSpeed; }
            set
            {

                this.SetValue(ref _defaultFanSpeed, value);

            }
        }

        private string _defaultPower;
        public string DefaultPower
        {
            get { return _defaultPower; }
            set
            {

                this.SetValue(ref _defaultPower, value);

            }
        }

        private string _defaultSeries;
        public string DefaultSeries
        {
            get { return _defaultSeries; }
            set
            {

                this.SetValue(ref _defaultSeries, value);

            }
        }

        //ACC - RAG START
        // Unit Name visibility on Room name selection
        private bool _isHENameEditable;
        public bool IsHENameEditable
        {
            get { return _isHENameEditable; }
            set
            {
                SetValue(ref _isHENameEditable, value);
            }
        }

        //Unit Name text color
        private Brush _unitNameColor;
        public Brush UnitNameColor
        {
            get { return _unitNameColor; }
            set
            {
                SetValue(ref _unitNameColor, value);
            }
        }
        //ACC - RAG END


        private ObservableCollection<Room> _listRoom;
        public ObservableCollection<Room> RoomName
        {
            get
            {
                return _listRoom;
            }
            set
            {
                this.SetValue(ref _listRoom, value);
            }
        }

        private ObservableCollection<Floor> _floor;

        public ObservableCollection<Floor> FloorList
        {
            get { return _floor; }
            set { this.SetValue(ref _floor, value); }
        }

        private ObservableCollection<FanSpeed> _fanSpeeds;

        public ObservableCollection<FanSpeed> FanSpeeds
        {
            get { return _fanSpeeds; }
            set { this.SetValue(ref _fanSpeeds, value); }
        }

        private ObservableCollection<SeriesModel> listSeries;

        public ObservableCollection<SeriesModel> ListSeries
        {
            get { return listSeries; }
            set
            {
                this.SetValue(ref listSeries, value);
                // BindPowerMode();
            }

        }

        private ObservableCollection<PowerModel> _power;
        private IModalWindowService _winService;

        public ObservableCollection<PowerModel> Power
        {
            get { return _power; }
            set
            {
                this.SetValue(ref _power, value);
            }

        }

        private void BindSeries()
        {
            try
            {
                //var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                ListSeries = new ObservableCollection<SeriesModel>();
                DataTable dtSeries = _bll.GetExchangerTypeList();

                if (dtSeries != null)
                {
                    foreach (DataRow dtSeriesTypeRow in dtSeries.Rows)
                    {

                        var seriesModel = new SeriesModel()
                        {
                            DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                            SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                        };

                        ListSeries.Add(seriesModel);
                        
                        if (proj != null && proj.IndoorItem != null)
                        {
                            if (proj.IndoorItem.Series.Equals(seriesModel.SelectedValues))
                            {
                                SelectedSeries = seriesModel;
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void BindPowerMode()
        {
            Power = new ObservableCollection<PowerModel>();
            string _series = SelectedSeries.SelectedValues;
            _project = JCHVRF.Model.Project.GetProjectInstance;
            if (!string.IsNullOrWhiteSpace(_series))
            {
                thisProject = _project;

                DataTable dtPower = Global.InitPowerList(thisProject, _series);
                foreach (DataRow dtSeriesTypeRow in dtPower.Rows)
                {

                    var powerModel = new PowerModel()
                    {
                        DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                        SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                        Code = dtSeriesTypeRow.ItemArray[1].ToString()

                    };
                    Power.Add(powerModel);

                    if (proj != null && powerModel.SelectedValues.Equals(proj.Power))
                    {
                        SelectedPower = powerModel;
                    }
                }
                if (Power != null && Power.Count > 0 && SelectedPower == null)
                {
                    SelectedPower = Power[0];
                }
            }
        }

        private void BindFanSpeed()
        {
            FanSpeeds = new ObservableCollection<FanSpeed>() { FanSpeed.High, FanSpeed.Medium, FanSpeed.Low };
        }

        private void OnAddFloorClicked()
        {
            try
            {
                NavigationParameters param = new NavigationParameters();
                param.Add("EnableSaveButtons", true);
                _winService.ShowView(ViewKeys.FloorTab, Langauge.Current.GetMessage("ADD_OR_EDIT_FLOOR"), param);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OpenGetFloorList()
        {
            BindFloor();
        }

        private void OnAddEditRoomClicked()
        {
            try
            {
                NavigationParameters param = new NavigationParameters();
                param.Add("ShowSaveCancel", true);
                _winService.ShowView(ViewKeys.AddEditRoom, Langauge.Current.GetMessage("ADDEDITROOMS"), param, true, 850, 550);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        void btnChangeTempUClicked()
        {

            try
            {
                CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = CurrentTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c;

                if (CurrentTempUnit == Unit.ut_Temperature_f)
                {
                    outdoorCoolingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(outdoorCoolingDB), UnitType.TEMPERATURE, CurrentTempUnit));
                    outdoorCoolingWB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(outdoorCoolingWB), UnitType.TEMPERATURE, CurrentTempUnit));
                    outdoorHeatingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(outdoorHeatingDB), UnitType.TEMPERATURE, CurrentTempUnit));

                    // TemperatureTypeOCDB = Celsius;
                }
                else
                {
                    outdoorCoolingDB = Convert.ToDouble(Unit.ConvertToSource(Convert.ToDouble(outdoorCoolingDB), UnitType.TEMPERATURE, Unit.ut_Temperature_f));
                    outdoorCoolingWB = Convert.ToDouble(Unit.ConvertToSource(Convert.ToDouble(outdoorCoolingWB), UnitType.TEMPERATURE, Unit.ut_Temperature_f));
                    outdoorHeatingDB = Convert.ToDouble(Unit.ConvertToSource(Convert.ToDouble(outdoorHeatingDB), UnitType.TEMPERATURE, Unit.ut_Temperature_f));
                    //  TemperatureTypeOCDB = Fahrenheit;
                }
                ValidateOnTempChange();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void ValidateOnTempChange()
        {
            ValidateHDDBT();
            ValidateOdb();
            ValidateOutdoorHDRH();
            ValidateCoolWetBulb();

        }

        private void BindFloor()
        {
            FloorList = new ObservableCollection<Floor>(Project.CurrentProject.FloorList);
            if (proj != null && proj.SelectedFloor!=null)
            {
                SelectedFloor = FloorList.FirstOrDefault(mm => mm.Id.Equals(proj.SelectedFloor.Id));
                if (SelectedFloor == null)
                {
                    SelectedFloor = FloorList[0];
                }
            }
        }

        public void GetHeatExchangerSystemProperties()
        {
            try
            {
                proj = Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(HvacSystem.Id));
                if (proj == null)
                {
                    return;
                }
                this.DefaultFanSpeed = (string)Enum.GetName(typeof(JCHVRF_New.Model.FanSpeed), proj.FanSpeedLevel);
                this.NoOfPeople = proj.NumberOfPeople;
                this.outdoorCoolingDB = proj.DBCooling;
                this.outdoorCoolingWB = proj.WBCooling;
                this.outdoorHeatingDB = proj.DBHeating;
                this.outdoorHeatingRH = proj.RHCooling;
                this.FreshAir = proj.RqFreshAir;
                this.Area = proj.Area;
                this.ESPVal = proj.RqStaticPressure;
                if (proj.RoomName != null)
                {
                    this.DefaultRoom = proj.RoomName;
                }
                if (proj.SelectedFloor != null)
                {
                    this.DefaultFloor = proj.SelectedFloor.Name;
                }
                if (proj.IndoorItem != null)
                {
                    this.DefaultSeries = proj.IndoorItem.Series;
                }
                this.DefaultPower = proj.Power;
                this.UnitName = proj.IndoorName;


                this.SFanSpeed = (FanSpeed)proj.FanSpeedLevel;

                foreach (var room in Project.CurrentProject.RoomList)
                {
                    if (room.Id.Equals(proj.RoomID))
                    {
                        SelectedRoom = room;
                        break;
                    }
                }

                if (proj.SelectedFloor != null)
                {
                    foreach (var floor in Project.CurrentProject.FloorList)
                    {
                        if (floor.Name.Equals(proj.SelectedFloor.Name))
                        {
                            SelectedFloor = floor;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        private void UpdateUnitName()
        {
            try
            {
                if (SelectedRoom != null)
                {
                    if (!string.IsNullOrWhiteSpace(SelectedRoom.Name))
                    {
                        //ACC - RAG
                        //Read-only Unit name
                        this.UnitName = SelectedRoom.Name;
                        this.IsHENameEditable = true;
                        this.UnitNameColor = new SolidColorBrush(Colors.Gray);
                        SetEditedPropertyValue();

                    }
                    else
                    {
                        this.IsHENameEditable = false;
                        this.UnitNameColor = new SolidColorBrush(Colors.Black);
                    }
                }

                else
                {
                    this.IsHENameEditable = false;
                    this.UnitNameColor = new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        //added by SA
        //To bind properties based on selected room 
        private void SetEditedPropertyValue()
        {
            FreshAir = SelectedRoom.FreshAir;
            ESPVal = SelectedRoom.StaticPressure;
            Area = SelectedRoom.Area;
            NoOfPeople = SelectedRoom.PeopleNumber;
        }

        private void GetRoomList()
        {
            try
            {
                if (JCHVRF.Model.Project.GetProjectInstance.FloorList.Count > 0)
                {
                    _project = JCHVRF.Model.Project.GetProjectInstance;
                    RoomName = new ObservableCollection<Room>(JCHVRF.Model.Project.GetProjectInstance.RoomList);
                }
                if (SelectedRoom != null)
                {
                    SetEditedPropertyValue();
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        public void CreateHeatExchanger(Project proj)
        {
            try
            {
                string newRoomName = "";
                string newRoomId = "";
                double newRoomFreshAir = 0;

                Indoor item = new Indoor();
                item.Series = SelectedSeries.SelectedValues;
                item.DisplayName = SelectedSeries.SelectedValues;
                if (_SelectedRoom != null)
                {
                    newRoomName = _SelectedRoom.Name;
                    newRoomId = _SelectedRoom.Id;
                    newRoomFreshAir = _SelectedRoom.FreshAir;
                    SelectedRoom.FreshAir = FreshAir;
                    SelectedRoom.StaticPressure = ESPVal;
                    SelectedRoom.Area = Area;
                    SelectedRoom.PeopleNumber = NoOfPeople;
                }
                else
                {
                    newRoomName = "";
                    newRoomId = null;
                    newRoomFreshAir = this.FreshAir;
                }

                proj.ExchangerList.Add(new RoomIndoor
                {
                    IndoorName = UnitName,
                    IndoorItem = item,
                    Power = SelectedPower.SelectedValues,
                    FanSpeedLevel = (int)SFanSpeed,
                    RqFreshAir = newRoomFreshAir,
                    Area = Area,
                    RqStaticPressure = ESPVal,
                    NumberOfPeople = NoOfPeople,
                    DBCooling = Convert.ToDouble(Project.CurrentProject.DesignCondition.indoorCoolingDB),
                    WBCooling = Convert.ToDouble(Project.CurrentProject.DesignCondition.indoorCoolingWB),
                    DBHeating = Convert.ToDouble(Project.CurrentProject.DesignCondition.indoorCoolingHDB),
                    RHCooling = Convert.ToDouble(Project.CurrentProject.DesignCondition.indoorCoolingRH),
                    RoomName = newRoomName,
                    RoomID = newRoomId,
                    SelectedFloor = SelectedFloor,
                    SystemID = HvacSystem.Id
                });
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnBeforeSave()
        {
            try
            {
                if (proj != null)
                {
                    UpdateHeatExchanger();
                }
                else
                {
                    CreateHeatExchanger(Project.CurrentProject);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private void UpdateHeatExchanger()
        {
            
            try
            {
                string newRoomName = "";
                string newRoomId = "";
                Indoor inItem = null;

                if (SelectedRoom != null)
                {
                    newRoomName = SelectedRoom.Name;
                    newRoomId = SelectedRoom.Id;
                    // Updating capacity rq values for room
                    SelectedRoom.FreshAir = FreshAir;
                    SelectedRoom.StaticPressure = ESPVal;
                    SelectedRoom.Area = Area;
                    SelectedRoom.PeopleNumber = NoOfPeople;
                }
                else
                {
                    newRoomName = "";
                    newRoomId = null;
                }
                if (SelectedSeries != null)
                {
                    if (proj.IndoorItem != null)
                    {
                        proj.IndoorItem.Series = SelectedSeries.SelectedValues;
                    }
                    else
                    {

                    }
                    if (SelectedPower != null)
                    {
                        DataTable dt = _bll.GetExchnagerListStd(SelectedSeries.SelectedValues, "", SelectedPower.Code);
                        dt.DefaultView.Sort = "AirFlow";
                        if (dt.Rows.Count > 0)
                        {
                            DataTable sortedDT = dt.DefaultView.ToTable();
                            DataRow dr = dt.Rows[0];
                            //DataRow dr = GetAutoModel(sortedDT.Rows);
                            string ProductType = string.Empty;
                            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
                                { ProductType = "Universal IDU"; } else { ProductType = SelectedSeries.SelectedValues; }

                            inItem = _bll.GetItem(dr["ModelFull"].ToString(), SelectedSeries.SelectedValues, ProductType, SelectedSeries.SelectedValues);
                            inItem.Series = SelectedSeries.SelectedValues;
                            //Required field for Report
                            inItem.DisplayName = SelectedSeries.SelectedValues;
                        }
                        proj.IndoorItem = inItem;
                    }
                }

                if (SelectedPower != null)
                {
                    proj.Power = SelectedPower.SelectedValues;
                }
                proj.RoomName = newRoomName;
                proj.RoomID = newRoomId;
                proj.FanSpeedLevel = (int)SFanSpeed;
                proj.IndoorName = UnitName;
                proj.SelectedFloor = SelectedFloor;
                proj.RqFreshAir = FreshAir;
                proj.Area = Area;
                proj.RqStaticPressure = ESPVal;
                proj.NumberOfPeople = NoOfPeople;
                proj.DBCooling = outdoorCoolingDB;
                proj.WBCooling = outdoorCoolingWB;
                proj.DBHeating = outdoorHeatingDB;
                proj.RHCooling = outdoorHeatingRH;

                _eventAggregator.GetEvent<SendHEDetails>().Publish(false);

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        public DataRow GetAutoModel(DataRowCollection rows)
        {
            double next_near = new double();
            int sel_row = 0;

            string airFlowFormat = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            Double FreshAirInfo = new double();

            if (airFlowFormat == Unit.ut_Airflow_cfm)
            {
                FreshAirInfo = FreshAir * 0.02832;
            }
            else if (airFlowFormat == Unit.ut_Airflow_ls)
            {
                FreshAirInfo = FreshAir * 0.06;
            }
            else
            {
                FreshAirInfo = FreshAir;
            }

            if (FreshAirInfo != 0.0)
            {
                next_near = Convert.ToDouble(rows[0]["AirFlow"]);
                for (int i = 0; i < rows.Count; i++)
                {
                    double for_commpare = Convert.ToDouble(rows[i]["AirFlow"]);

                    if (FreshAirInfo <= for_commpare)
                    {

                        sel_row = i;
                        break;
                    }
                    sel_row = i;
                }
            }
            return rows[sel_row];
        }

    }
}
