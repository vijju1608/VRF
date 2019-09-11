using JCBase.UI;
using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.Const;
using JCHVRF.Model;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using JCHVRF_New.Utility;
using JCHVRF_New.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using langauge = JCHVRF_New.LanguageData.LanguageViewModel;
namespace JCHVRF_New.ViewModels
{
    public class AddIndoorUnitViewModel : ViewModelBase
    {

        private IEventAggregator _eventAggregator;

        #region Fields
        private double outdoorCoolingDB = 0;
        private double outdoorHeatingWB = 0;
        private double outdoorCoolingIW = 0;
        private double outdoorHeatingIW = 0;
        DataTable IDuModelTypeList = new DataTable();
        string _factory = "";
        string _type = "";
        //string _productType = "";
        string SelectionForUpdateIndoor;
        string ODUProductTypeUniversal = "Universal IDU";
        List<string> _typeList = new List<string>();
        List<Indoor> IDUIndoorList;
        string ODUSeries;// = "Commercial VRF HP, FSXNK"; // "Commercial VRF HP, FSXNK",  Residential VRF HP, FS(V/Y)N1Q/FSNMQ
        string ODUProductType;// = "Comm. Tier 2, HP"; // "Comm. Tier 2, HP"  Res. Tier 1, HP
        IndoorBLL bll;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        string navigateToFolder = "..\\..\\Image\\TypeImageProjectCreation";
        string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        JCHVRF.Model.NextGen.SystemVRF system;
        DataRow datarow;
        #endregion


        #region View_Model_Property

        private List<RoomIndoor> _selectedRoomIndoors;

        public List<RoomIndoor> ListOfIndoorsToAdd
        {
            get { return _selectedRoomIndoors; }
            set { _selectedRoomIndoors = value; }
        }
        private bool _enableManualselectionIduModel;
        public bool EnableManualselectionIduModel { get { return _enableManualselectionIduModel; } set { this.SetValue(ref _enableManualselectionIduModel, value); } }

        private FreshAirArea _currentFreshAirArea;

        public FreshAirArea CurrentFreshAirArea
        {
            get { return _currentFreshAirArea; }
            private set
            {
                SetValue<FreshAirArea>(ref _currentFreshAirArea, value);
            }
        }



        private JCHVRF.Model.Project _project;

        private string _autoSelectNotification;

        public string AutoSelectionMessage
        {
            get { return _autoSelectNotification; }
            private set
            {
                SetValue<string>(ref _autoSelectNotification, value);
            }
        }

        private RoomIndoor _selectedIndoor;

        public RoomIndoor SelectedIndoor
        {
            get { return _selectedIndoor; }
            set
            {
                SetValue<RoomIndoor>(ref _selectedIndoor, value);
            }
        }

        private bool _isIndoorUnitEditable;

        public bool IsIndoorUnitEditable
        {
            get { return _isIndoorUnitEditable; }
            set
            {
                SetValue(ref _isIndoorUnitEditable, value);
            }
        }

        public string LengthUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            }
        }

        public string TempMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            }
        }

        private string _designConditionTempMasureUnite;
        public string DesignConditionTempMasureUnit
        {
            get { return _designConditionTempMasureUnite; }
            set { this.SetValue(ref _designConditionTempMasureUnite, value); }
        }


        private string _RelativeHumidityMasureUnit;
        public string RelativeHumidityMasureUnit
        {
            get { return _RelativeHumidityMasureUnit; }
            set { this.SetValue(ref _RelativeHumidityMasureUnit, value); }
        }

        public string CapacityMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingPOWER;
            }
        }

        public string AreaMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingAREA;
            }
        }

        public string AirFlowMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            }
        }

        public string PressureMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingESP;
            }
        }

        private string _ChangeToFandC;
        public string ChangeToFandC
        {
            get { return _ChangeToFandC; }
            set { this.SetValue(ref _ChangeToFandC, value); }
        }

        private string _IndoorUnitName;
        [Required(ErrorMessage = "Please enter Indoor Unit Name")]
        public string IndoorUnitName
        {
            get { return _IndoorUnitName; }
            set { this.SetValue(ref _IndoorUnitName, value); }
        }



        private string _IndoorError;
        public string IndoorError
        {
            get { return _IndoorError == null ? string.Empty : _IndoorError; }
            set { this.SetValue(ref _IndoorError, value); }
        }


        private ObservableCollection<Room> _ListRoom;
        public ObservableCollection<Room> ListRoom
        {
            get
            {
                return _ListRoom;
            }
            set
            {
                this.SetValue(ref _ListRoom, value);
            }
        }

        private Room _SelectedRoom;
        public Room SelectedRoom
        {
            get { return _SelectedRoom; }
            set
            {

                this.SetValue(ref _SelectedRoom, value);
                CurrentFreshAirArea = _project.FreshAirAreaList.FirstOrDefault(i => i.Id == _SelectedRoom.FreshAirAreaId);
                BindCapacityRequirements();
                DoAutoSelect();
                BindInternalDesignConditions();
            }
        }


        private ObservableCollection<JCHVRF.Model.Floor> _ListFloor;
        public ObservableCollection<JCHVRF.Model.Floor> ListFloor
        {
            get
            {
                return _ListFloor;
            }
            set
            {
                this.SetValue(ref _ListFloor, value);
            }
        }

        private string _SelectedFloor;
        public string SelectedFloor
        {
            get { return _SelectedFloor; }
            set { this.SetValue(ref _SelectedFloor, value); }
        }

        private string _IduImagePath;
        public string IduImagePath
        {
            get { return _IduImagePath; }
            set { this.SetValue(ref _IduImagePath, value); }
        }

        public List<string> IduPosition
        {
            get
            {
                return Enum.GetNames(typeof(PipingPositionType)).ToList();
            }
        }


        private string _SelectedIduPosition;
        public string SelectedIduPosition
        {
            get
            {

                if (_SelectedIduPosition == "SameLevel")
                {

                    this.NumericCoolWetBulbVisibility = false;
                    HeightDifference = 0.0;
                }
                else
                {
                    this.NumericCoolWetBulbVisibility = true;
                }
                return _SelectedIduPosition;

            }
            set
            {

                this.SetValue(ref _SelectedIduPosition, value);

            }
        }
        private bool numericCoolWetBulbVisibility;
        public bool NumericCoolWetBulbVisibility
        {
            get
            {
                return numericCoolWetBulbVisibility;
            }
            set
            {
                this.SetValue(ref numericCoolWetBulbVisibility, value);
            }

        }


        private double? _HeightDifference;
        public double? HeightDifference
        {
            get
            {
                return _HeightDifference;
            }
            set
            {
                this.SetValue(ref _HeightDifference, value);
            }
        }

        private string _HeightDifferenceError;
        public string HeightDifferenceError
        {
            get
            {
                return _HeightDifferenceError == null ? string.Empty : _HeightDifferenceError;
            }
            set
            {
                this.SetValue(ref _HeightDifferenceError, value);
            }
        }



        public List<string> FanSpeed
        {
            get
            {
                return Enum.GetNames(typeof(FanSpeed)).ToList(); ;
            }

        }


        private string _SelectedFanSpeed;
        public string SelectedFanSpeed
        {
            get { return _SelectedFanSpeed; }
            set
            {
                this.SetValue(ref _SelectedFanSpeed, value);
                BatchCalculateEstValue(); // fix for 3967 : trigger calculation with change in fan speed
                BatchCalculateAirFlow();


            }
        }


        private bool _ManualSelection;
        public bool ManualSelection
        {
            get { return _ManualSelection; }
            set
            {
                this.SetValue(ref _ManualSelection, value);
                SetCapacityRequirementVisibility();
                AutoSelectionMessage = null;
                DoAutoSelect();
                if (ManualSelection)
                    EnableManualselectionIduModel = true;
                else
                    EnableManualselectionIduModel = false;
            }
        }

        private Visibility _CapacityRequirementsVisibility;
        public Visibility CapacityRequirementsVisibility
        {
            get { return _CapacityRequirementsVisibility; }
            set
            {
                this.SetValue(ref _CapacityRequirementsVisibility, value);

            }
        }


        private Visibility _ESPVisibility;
        public Visibility ESPVisibility
        {
            get { return _ESPVisibility; }
            set
            {
                this.SetValue(ref _ESPVisibility, value);

            }
        }


        private Visibility _freshAirVisibility;
        public Visibility FreshAirVisibility
        {
            get { return _freshAirVisibility; }
            set
            {
                this.SetValue(ref _freshAirVisibility, value);

            }
        }

        private TemperatureUnit _ChangeToUnit;
        public TemperatureUnit ChangeToUnit
        {
            get { return _ChangeToUnit; }
            set { this.SetValue(ref _ChangeToUnit, value); }
        }



        private bool _UseRoomTemperature;
        public bool UseRoomTemperature
        {
            get { return _UseRoomTemperature; }
            set
            {
                this.SetValue(ref _UseRoomTemperature, value);
                BindInternalDesignConditions();
            }
        }

        private List<ComboBox> _ListUnitType;
        public List<ComboBox> ListUnitType
        {
            get
            {
                return _ListUnitType;
            }
            set
            {
                this.SetValue(ref _ListUnitType, value);
            }
        }

        public void GetRoomList()
        {
            CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            ListRoom = new ObservableCollection<Room>(JCHVRF.Model.Project.GetProjectInstance.RoomList);
        }

        private string _SelectedUnitType;
        public string SelectedUnitType
        {
            get { return _SelectedUnitType; }
            set
            {
                this.SetValue(ref _SelectedUnitType, value);
                if (_SelectedUnitType != null)
                {
                    BindIDuModelType();
                    BindESPVisibility();
                    DoAutoSelect();

                }

            }
        }



        private string _UnitTypeError;
        public string UnitTypeError
        {
            get { return _UnitTypeError == null ? string.Empty : _UnitTypeError; }
            set
            {
                this.SetValue(ref _UnitTypeError, value);
            }
        }

        private ObservableCollection<ComboBox> _ListModel;
        public ObservableCollection<ComboBox> ListModel
        {
            get
            {
                return _ListModel;
            }
            set
            {
                this.SetValue(ref _ListModel, value);
            }
        }


        private string _ListModelError;
        public string ListModelError
        {
            get { return _ListModelError == null ? string.Empty : _ListModelError; }
            set
            {
                this.SetValue(ref _ListModelError, value);
            }
        }


        private string _SelectedModel;
        public string SelectedModel
        {
            get { return _SelectedModel; }
            set
            {
                this.SetValue(ref _SelectedModel, value);
                if (_SelectedModel != null)
                {
                    BindIndoorImageToUI();
                    BatchCalculateEstValue();
                    BatchCalculateAirFlow();
                }
            }
        }

        #region Internal_Design_Conditions_Property
        private double? _CoolingDryBulb;
        public double? CoolingDryBulb
        {
            get
            {
                return _CoolingDryBulb;
            }
            set
            {
                this.SetValue(ref _CoolingDryBulb, value);
                if (CoolingDryBulb != null)
                {
                     //DoCalculateByOption(UnitTemperature.DB.ToString());
                    BatchCalculateEstValue();
                }
            }
        }

        private string _CoolingDryBulbError;
        public string CoolingDryBulbError
        {
            get { return _CoolingDryBulbError == null ? string.Empty : _CoolingDryBulbError; }
            set
            {
                this.SetValue(ref _CoolingDryBulbError, value);
            }
        }


        private double? _CoolingWetBulb;
        public double? CoolingWetBulb
        {
            get
            {
                return _CoolingWetBulb;
            }
            set
            {
                this.SetValue(ref _CoolingWetBulb, value);
                if (CoolingWetBulb != null)
                {
                   // DoCalculateByOption(UnitTemperature.WB.ToString());
                    BatchCalculateEstValue();
                }
            }
        }

        private string _CoolingWetBulbError;
        public string CoolingWetBulbError
        {
            get { return _CoolingWetBulbError == null ? string.Empty : _CoolingWetBulbError; }
            set
            {
                this.SetValue(ref _CoolingWetBulbError, value);
            }
        }

        private double? _HeatingDryBulb;
        public double? HeatingDryBulb
        {
            get
            {
                return _HeatingDryBulb;
            }
            set
            {
                this.SetValue(ref _HeatingDryBulb, value);
                if (HeatingDryBulb != null)
                {
                    BatchCalculateEstValue();
                }
            }
        }

        private string _HeatingDryBulbError;
        public string HeatingDryBulbError
        {
            get { return _HeatingDryBulbError == null ? string.Empty : _HeatingDryBulbError; }
            set
            {
                this.SetValue(ref _HeatingDryBulbError, value);
            }
        }

        private double? _RelativeHumidity;
        public double? RelativeHumidity
        {
            get
            {
                return _RelativeHumidity;
            }
            set
            {
                this.SetValue(ref _RelativeHumidity, value);
            }
        }

        private string _RelativeHumidityError;
        public string RelativeHumidityError
        {
            get { return _RelativeHumidityError == null ? string.Empty : _RelativeHumidityError; }
            set
            {
                this.SetValue(ref _RelativeHumidityError, value);
            }
        }

        #endregion

        #region Capacity_Requirements_Property
        private double? _CR_TotalCapacity;
        public double? CR_TotalCapacity
        {
            get
            {
                return _CR_TotalCapacity;
            }
            set
            {
                this.SetValue(ref _CR_TotalCapacity, value);
                //DoAutoSelect();
            }
        }


        private string _CR_TotalCapacityError;
        public string CR_TotalCapacityError
        {
            get { return _CR_TotalCapacityError == null ? string.Empty : _CR_TotalCapacityError; }
            set
            {
                this.SetValue(ref _CR_TotalCapacityError, value);
            }
        }


        private double? _CR_SensibleCapacity;
        public double? CR_SensibleCapacity
        {
            get
            {
                return _CR_SensibleCapacity;
            }
            set
            {
                this.SetValue(ref _CR_SensibleCapacity, value);
                //DoAutoSelect();
            }
        }


        private string _CR_SensibleCapacityError;
        public string CR_SensibleCapacityError
        {
            get { return _CR_SensibleCapacityError == null ? string.Empty : _CR_SensibleCapacityError; }
            set
            {
                this.SetValue(ref _CR_SensibleCapacityError, value);
            }
        }


        private double? _CR_HeatingCapacity;
        public double? CR_HeatingCapacity
        {
            get
            {
                return _CR_HeatingCapacity;
            }
            set
            {
                this.SetValue(ref _CR_HeatingCapacity, value);
            }
        }

        private string _CR_HeatingCapacityError;
        public string CR_HeatingCapacityError
        {
            get { return _CR_HeatingCapacityError == null ? string.Empty : _CR_HeatingCapacityError; }
            set
            {
                this.SetValue(ref _CR_HeatingCapacityError, value);
            }
        }

        private double? _CR_FreshAir;
        public double? CR_FreshAir
        {
            get
            {
                return _CR_FreshAir;
            }
            set
            {
                this.SetValue(ref _CR_FreshAir, value);
            }
        }

        private string _CR_FreshAirError;
        public string CR_FreshAirError
        {
            get { return _CR_FreshAirError == null ? string.Empty : _CR_FreshAirError; }
            set
            {
                this.SetValue(ref _CR_FreshAirError, value);
            }
        }


        private double? _CR_AirFlow;
        public double? CR_AirFlow
        {
            get
            {
                return _CR_AirFlow;
            }
            set
            {
                this.SetValue(ref _CR_AirFlow, value);
            }
        }

        private string _CR_AirFlowError;
        public string CR_AirFlowError
        {
            get { return _CR_AirFlowError == null ? string.Empty : _CR_AirFlowError; }
            set
            {
                this.SetValue(ref _CR_AirFlowError, value);
            }
        }

        private double? _CR_ESP;
        public double? CR_ESP
        {
            get
            {
                return _CR_ESP;
            }
            set
            {
                this.SetValue(ref _CR_ESP, value);
            }
        }


        private string _CR_ESPError;
        public string CR_ESPError
        {
            get { return _CR_ESPError == null ? string.Empty : _CR_ESPError; }
            set
            {
                this.SetValue(ref _CR_ESPError, value);
            }
        }
        #endregion



        #region Selected_Capacity_Property
        private string _SR_TotalCapacity;
        public string SR_TotalCapacity
        {
            get
            {
                return _SR_TotalCapacity;
            }
            set
            {
                this.SetValue(ref _SR_TotalCapacity, value);
            }
        }

        private string _SR_SensibleCapacity;
        public string SR_SensibleCapacity
        {
            get
            {
                return _SR_SensibleCapacity;
            }
            set
            {
                this.SetValue(ref _SR_SensibleCapacity, value);
            }
        }

        private string _SR_HeatingCapacity;
        public string SR_HeatingCapacity
        {
            get
            {
                return _SR_HeatingCapacity;
            }
            set
            {
                this.SetValue(ref _SR_HeatingCapacity, value);
            }
        }


        private string _SR_FreshAir;
        public string SR_FreshAir
        {
            get
            {
                return _SR_FreshAir;
            }
            set
            {
                this.SetValue(ref _SR_FreshAir, value);
            }
        }


        private string _SR_AirFlow;
        public string SR_AirFlow
        {
            get
            {
                return _SR_AirFlow;
            }
            set
            {
                this.SetValue(ref _SR_AirFlow, value);
            }
        }


        private string _SR_ESP;
        public string SR_ESP
        {
            get
            {
                return _SR_ESP;
            }
            set
            {
                this.SetValue(ref _SR_ESP, value);
            }
        }

        #endregion



        private ObservableCollection<RoomIndoor> _ListRoomIndoor;
        public ObservableCollection<RoomIndoor> ListRoomIndoor
        {
            get
            {
                return _ListRoomIndoor;
            }
            set
            {
                this.SetValue(ref _ListRoomIndoor, value);
            }
        }

        #endregion

        #region DelegateCommand
        // public DelegateCommand OnAutoSeletTrigger{ get; set; }
        public DelegateCommand OpenAddRoomWindowCommand { get; set; }
        public DelegateCommand OpenAddFloorWindowCommand { get; set; }
        public DelegateCommand AddAllCommand { get; set; }

        public DelegateCommand ChangeDesignTempCommand { get; set; }
        public DelegateCommand LostFocusCoolingDryBulb { get; set; }
        public DelegateCommand LostFocusCoolingWetBulb { get; set; }
        public DelegateCommand LostFocusRelativeHumidity { get; set; }
        JCHVRF.Model.Project CurrentProject;

        public DelegateCommand AddToListCommand { get; set; }

        public DelegateCommand ResetCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        public DelegateCommand<RoomIndoor> RemoveSelectedIndoorCommand { get; set; }

        public DelegateCommand<RoomIndoor> UpdateSelectedIndoorCommand { get; set; }

        #endregion

        #region Ctor
        public AddIndoorUnitViewModel(IEventAggregator EventAggregator, IModalWindowService windowService)
        {
            try
            {

                this.IsIndoorUnitEditable = true;
                this.ESPVisibility = Visibility.Visible;
                ListRoomIndoor = new ObservableCollection<RoomIndoor>();
                AutoSelectionMessage = null;
                _eventAggregator = EventAggregator;

                _winService = windowService;
                BindDefaultFanSpeed();
                this.IndoorUnitName = SystemSetting.UserSetting.defaultSetting.IndoorName + GetIndoorCount();
                this.DesignConditionTempMasureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                this.RelativeHumidityMasureUnit = "%";
                this.ManualSelection = false;
                this.UseRoomTemperature = false;
                this.HeightDifference = 0;
                this.CR_TotalCapacity = 0;
                this.CR_SensibleCapacity = 0;
                this.CR_HeatingCapacity = 0;
                this.CR_AirFlow = 0;
                this.CR_FreshAir = 0;
                this.CR_ESP = 0;
                BindOutdoorProperty();
                BindFloorList();
                GetRoomList();
                this.CapacityRequirementsVisibility = Visibility.Visible;
                this.FreshAirVisibility = Visibility.Visible;
                if (this.DesignConditionTempMasureUnit == Unit.ut_Temperature_c)
                {
                    this.ChangeToFandC = "Change to °F";
                }
                else
                {
                    this.ChangeToFandC = "Change to °C";
                }
                OpenAddRoomWindowCommand = new DelegateCommand(OpenAddRoomWindowOnClick);
                OpenAddFloorWindowCommand = new DelegateCommand(OpenAddFloorWindowOnClick);
                //            OnAutoSeletTrigger = new DelegateCommand<object>(AutoSelectTrigger);
                AddAllCommand = new DelegateCommand(AddAllIndoorUnitOnClick);
                AddToListCommand = new DelegateCommand(AddIndoorToListOnClick);
                ChangeDesignTempCommand = new DelegateCommand(ChangeDesignTempOnClick);
                ResetCommand = new DelegateCommand(ResetCommandOnClick);
                RemoveSelectedIndoorCommand = new DelegateCommand<RoomIndoor>(RemoveSelectedIndoorOnClick);
                UpdateSelectedIndoorCommand = new DelegateCommand<RoomIndoor>(UpdateSelectedIndoorOnClick);
                LostFocusCoolingWetBulb = new DelegateCommand(LostFocusCoolingWetBulbEvent);
                LostFocusCoolingDryBulb = new DelegateCommand(LostFocusCoolingDryBulbEvent);
                LostFocusRelativeHumidity = new DelegateCommand(LostFocusRelativeHumidityEvent);
                CancelCommand = new DelegateCommand(CancelCommandOnClick);
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Subscribe(OpenGetFloorList);
                _eventAggregator.GetEvent<RoomListSaveSubscriber>().Subscribe(OpenGetRoomList);
                _project = JCHVRF.Model.Project.CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
                bll = new IndoorBLL(JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, JCHVRF.Model.Project.GetProjectInstance.BrandCode);
                _project.ProductType = ODUProductType;

                BindIDUPosition();
                BindIDuUnitType();
                BindIndoorImageToUI();
                BindInternalDesignConditions();
                BatchCalculateEstValue();
                BatchCalculateAirFlow();
                SetCapacityRequirementVisibility();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        //private void ManualSelectionCheckedEvent()
        //{
        //    EnableManualselectionIduModel = true;
        //}
        //private void ManualSelectionUnCheckedEvent()
        //{
        //    EnableManualselectionIduModel = false;
        //}
        public void AutoSelectTrigger()
        {
            DoAutoSelect();
        }

        #endregion
        private void OpenGetFloorList()
        {
            BindFloorList();
        }
        private void OpenGetRoomList()
        {
            GetRoomList();
        }

        #region PropertyChangeEvent       
        private void SetCapacityRequirementVisibility()
        {
            if (this.ManualSelection == true)
            {
                this.CapacityRequirementsVisibility = Visibility.Collapsed;
            }
            else
            {
                this.CapacityRequirementsVisibility = Visibility.Visible;
            }
        }
        #endregion

        #region BindingData

        private void BindESPVisibility()
        {
            if (this.SelectedUnitType != null)
            {
                if (this.SelectedUnitType.ToLower().Contains("ducted"))
                    this.ESPVisibility = Visibility.Visible;
                else
                    this.ESPVisibility = Visibility.Collapsed;

                if ((this.SelectedUnitType.ToLower()).Contains("fresh air"))
                    this.FreshAirVisibility = Visibility.Visible;
                else
                    this.FreshAirVisibility = Visibility.Collapsed;
            }

        }
        private void LostFocusCoolingDryBulbEvent()
        {
            try
            {
                DoCalculateByOptionInd("DB");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void LostFocusRelativeHumidityEvent()
        {
            try
            {
                DoCalculateByOptionInd(UnitTemperature.RH.ToString());
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void LostFocusCoolingWetBulbEvent()
        {
            try
            {
                DoCalculateByOptionInd("WB");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void DoCalculateByOption(string Opt)
        {
            try
            {
                if (this.CoolingDryBulb != null && this.CoolingWetBulb != null && this.RelativeHumidity != null)
                {
                    double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    double rhcool = Convert.ToDouble(this.RelativeHumidity);

                    FormulaCalculate fc = new FormulaCalculate();
                    decimal pressure = fc.GetPressure(Convert.ToDecimal(JCHVRF.Model.Project.GetProjectInstance.Altitude));
                    if (Opt == UnitTemperature.WB.ToString())
                    {
                        double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                        this.RelativeHumidity = (rh * 100);
                    }
                    else if (Opt == UnitTemperature.DB.ToString())
                    {
                        double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                        if (rhcool != 0)
                        {
                            this.CoolingWetBulb = wb;
                        }
                    }
                    else if (Opt == UnitTemperature.RH.ToString())
                    {
                        double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                        if (rhcool != 0)
                        {
                            this.CoolingWetBulb = wb;
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
        public void DoCalculateByOptionInd(string Opt)
        {
            double dbcool = Convert.ToDouble(CoolingDryBulb.ToString());
            double wbcool = Convert.ToDouble(CoolingWetBulb.ToString());
            double rhcool = Convert.ToDouble(RelativeHumidity);
            FormulaCalculate fc = new FormulaCalculate();
            decimal pressure = fc.GetPressure(Convert.ToDecimal(0));
            if (Opt == UnitTemperature.WB.ToString())
            {
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));

                if (this.RelativeHumidity.ToString() != (rh * 100).ToString("n0"))
                {
                    this.RelativeHumidity = Convert.ToDouble((rh * 100).ToString("n0"));
                }
            }
            else if (Opt == UnitTemperature.DB.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (CoolingDryBulb.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {

                        CoolingWetBulb = Convert.ToDouble(wb.ToString("n1"));

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
                        this.CoolingWetBulb = (Double)wb;
                    }

                }
            }
        }
        private void BindCapacityRequirements()
        {
            if (this.SelectedRoom != null)
            {
                this.CR_TotalCapacity = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.RqCapacityCool), UnitType.POWER, utPower);
                this.CR_SensibleCapacity = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.SensibleHeat), UnitType.POWER, utPower);
                this.CR_HeatingCapacity = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.RqCapacityHeat), UnitType.POWER, utPower);
                this.CR_AirFlow = Unit.ConvertToControl(SelectedRoom.AirFlow, UnitType.AIRFLOW, utAirflow);
                this.CR_FreshAir = Unit.ConvertToControl(SelectedRoom.FreshAir, UnitType.AIRFLOW, utAirflow);
                this.CR_ESP = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.StaticPressure), UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(SelectedRoom.StaticPressure);
                this.IndoorUnitName = this.SelectedRoom.Name;
                this.IsIndoorUnitEditable = false;
            }
        }

        private void BindInternalDesignConditions()
        {
            if (this.UseRoomTemperature == false)
            {
                var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
                this.CoolingDryBulb = Unit.ConvertToControl(Convert.ToDouble(InternalDesignConditions.indoorCoolingDB), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                this.CoolingWetBulb = Unit.ConvertToControl(Convert.ToDouble(InternalDesignConditions.indoorCoolingWB), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                this.HeatingDryBulb = Unit.ConvertToControl(Convert.ToDouble(InternalDesignConditions.indoorCoolingHDB), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                this.RelativeHumidity = Convert.ToDouble(InternalDesignConditions.indoorCoolingRH);
            }
            else
            {
                if (SelectedRoom != null)
                {
                    this.CoolingDryBulb = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    this.CoolingWetBulb = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    this.HeatingDryBulb = Unit.ConvertToControl(Convert.ToDouble(SelectedRoom.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    this.RelativeHumidity = Convert.ToDouble(SelectedRoom.CoolingRelativeHumidity);
                }
            }
        }

        private void BindFloorList()
        {
            this.ListFloor = new ObservableCollection<JCHVRF.Model.Floor>(JCHVRF.Model.Project.GetProjectInstance.FloorList);
            if (this.ListFloor != null && this.ListFloor.Count > 0 && this.SelectedFloor == null)
            {
                this.SelectedFloor = this.ListFloor.FirstOrDefault().Id;
            }
        }

        private void BindDefaultFanSpeed()
        {
            this.SelectedFanSpeed = JCHVRF_New.Model.FanSpeed.Max.ToString();
        }

        private void BindIDUPosition()
        {
            this.SelectedIduPosition = PipingPositionType.SameLevel.ToString();

        }

        private void BindIDuUnitType()
        {
            ListUnitType = new List<ComboBox>();
            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                //var IndoorTypeList1 = bll.GetIndoorDisplayName();
                var IndoorTypeList = bll.GetIndoorDisplayNameForODUSeries(ODUSeries);
                foreach (DataRow rowView in IndoorTypeList.Rows)
                {
                    if (Convert.ToString(rowView["Display_Name"]) != "Total Heat Exchanger (KPI-E4E)")
                        ListUnitType.Add(new ComboBox { DisplayName = Convert.ToString(rowView["Display_Name"]), Value = Convert.ToString(rowView["Display_Name"]) });
                }
                if (ListUnitType.Count > 0)
                    this.SelectedUnitType = ListUnitType.FirstOrDefault().DisplayName;
            }
            else
            {
                if (JCHVRF.Model.Project.GetProjectInstance == null || string.IsNullOrEmpty(JCHVRF.Model.Project.GetProjectInstance.SubRegionCode) || string.IsNullOrEmpty(ODUProductType))
                    return;
                string colName = "UnitType";
                DataTable dt = bll.GetIndoorFacCodeList(ODUProductType);
                foreach (DataRow dr in dt.Rows)
                {
                    if (Convert.ToInt32(dr["FactoryCount"].ToString()) > 1)
                    {
                        switch (dr["FactoryCode"].ToString())
                        {
                            case "G":
                                dr[colName] += "-GZF";
                                break;
                            case "E":
                                dr[colName] += "-HAPE";
                                break;
                            case "Q":
                                dr[colName] += "-HAPQ";
                                break;
                            case "B":
                                dr[colName] += "-HAPB";
                                break;
                            case "I":
                                dr[colName] += "-HHLI";
                                break;
                            case "M":
                                dr[colName] += "-HAPM";
                                break;
                            case "S":
                                dr[colName] += "-SMZ";
                                break;
                        }
                    }
                }
                var dv = new DataView(dt);
                if (ODUProductType == "Comm. Tier 2, HP")
                {

                    if (ODUSeries == "Commercial VRF HP, FSN6Q" || ODUSeries == "Commercial VRF HP, JVOH-Q")
                    {
                        dv.RowFilter = "UnitType not in ('High Static Ducted-HAPE','Medium Static Ducted-HAPE','Low Static Ducted-HAPE','High Static Ducted-SMZ','Medium Static Ducted-SMZ','Four Way Cassette-SMZ')";
                    }
                    else
                    {
                        dv.RowFilter = "UnitType <>'Four Way Cassette-HAPQ'";
                    }
                }
                foreach (DataRowView rowView in dv)
                {
                    if (!CommonBLL.StringConversion(rowView["UnitType"]).Contains("Exchanger"))
                        ListUnitType.Add(new ComboBox { DisplayName = Convert.ToString(rowView.Row["UnitType"]), Value = Convert.ToString(rowView.Row["UnitType"]) });
                }
                if (ListUnitType.Count > 0)
                    this.SelectedUnitType = ListUnitType.FirstOrDefault().DisplayName;
            }

        }  
        
        private void UpdateUnitType()
        {
            int i = this._SelectedUnitType.IndexOf("-");
            if (i > 0)
            {
                _factory = this._SelectedUnitType.Substring(i + 1, this._SelectedUnitType.Length - i - 1);
                _type = this._SelectedUnitType.Substring(0, i);
            }
            else
            {
                _factory = "";
                _type = this._SelectedUnitType;
            }
        }
        private void UpdateUnitTypeUniversal()
        {
            _factory = bll.GetFCodeByDisUnitType(this._SelectedUnitType, out _typeList);
        }
        private void BindIDuModelType()
        {
            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                UpdateUnitTypeUniversal();
                IDuModelTypeList = bll.GetUniversalIndoorListStd(this._SelectedUnitType, _factory, _typeList);
                IDuModelTypeList.DefaultView.Sort = "CoolCapacity";
                //await Task.Run(() =>
                BindCompatibleIndoorUniversal();
                //);
            }
            else
            {
                UpdateUnitType();
                IDuModelTypeList = bll.GetIndoorListStd(_type, ODUProductType, _factory);
                IDuModelTypeList.DefaultView.Sort = "CoolCapacity";
                //await Task.Run(() =>
                BindCompatibleIndoorSimple();
                //);
            }
        }

        private void BindCompatibleIndoorSimple()
        {
            try
            {
                IDUIndoorList = new List<Indoor>();
                List<ComboBox> ListModelTemp = new List<ComboBox>();
                ListModel = new ObservableCollection<ComboBox>();
                foreach (DataRow drIduModel in IDuModelTypeList.Rows)
                {
                    var Indoor = bll.GetItem(drIduModel["ModelFull"].ToString(), _type, ODUProductType, ODUSeries);

                    //double est_cool = Convert.ToDouble(drIduModel["CoolCapacity"].ToString());
                    //Indoor.CoolingCapacity = Unit.ConvertToControl(est_cool, UnitType.POWER, utPower);
                    IDUIndoorList.Add(Indoor);
                    if (JCHVRF.Model.Project.GetProjectInstance.BrandCode == "Y")
                        ListModelTemp.Add(new ComboBox { DisplayName = Convert.ToString(drIduModel["Model_York"]), Value = Convert.ToString(drIduModel["ModelFull"]) });
                    else
                        ListModelTemp.Add(new ComboBox { DisplayName = Convert.ToString(drIduModel["Model_Hitachi"]), Value = Convert.ToString(drIduModel["ModelFull"]) });

                }
                if (ListModelTemp != null && ListModelTemp.Count > 0)
                {
                    ListModel = new ObservableCollection<ComboBox>(ListModelTemp);
                    //  this.SelectedModel = ListModel.FirstOrDefault().Value;
                    DoAutoSelect();
                    if (!String.IsNullOrEmpty(SelectionForUpdateIndoor))
                        this.SelectedModel = SelectionForUpdateIndoor;
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void BindCompatibleIndoorUniversal()
        {
            try
            {
                ListModel = new ObservableCollection<ComboBox>();
                List<ComboBox> ListModelTemp = new List<ComboBox>();
                // ODUSeries = "Commercial VRF HP, FSXNSE";
                IDUIndoorList = new List<Indoor>();
                MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
                DataTable typeDt = productTypeBll.GetIduTypeBySeries(JCHVRF.Model.Project.GetProjectInstance.BrandCode, JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, ODUSeries);
                foreach (DataRow drIduModel in IDuModelTypeList.Rows)
                {
                    var Indoor = bll.GetItem(drIduModel["ModelFull"].ToString(), _type, ODUProductTypeUniversal, ODUSeries);

                    if (typeDt != null)
                        foreach (DataRow dr in typeDt.Rows)
                        {
                            if (Indoor.Type == dr["IDU_UnitType"].ToString())
                            {
                                var modelHitachi = dr["IDU_Model_Hitachi"].ToString();
                                if (string.IsNullOrEmpty(modelHitachi) || modelHitachi.Contains(";" + Convert.ToString(drIduModel["Model_Hitachi"]) + ";"))
                                {
                                    IDUIndoorList.Add(Indoor);
                                    if (JCHVRF.Model.Project.GetProjectInstance.BrandCode == "Y")
                                        ListModelTemp.Add(new ComboBox { DisplayName = Convert.ToString(drIduModel["Model_York"]), Value = Convert.ToString(drIduModel["ModelFull"]) });
                                    else
                                        ListModelTemp.Add(new ComboBox { DisplayName = Convert.ToString(drIduModel["Model_Hitachi"]), Value = Convert.ToString(drIduModel["ModelFull"]) });
                                }
                                break;
                            }
                        }
                }
                if (ListModelTemp != null && ListModelTemp.Count > 0)
                {
                    ListModel = new ObservableCollection<ComboBox>(ListModelTemp);
                    //this.SelectedModel = ListModel.FirstOrDefault().Value;
                    DoAutoSelect();
                    if (!String.IsNullOrEmpty(SelectionForUpdateIndoor))
                        this.SelectedModel = SelectionForUpdateIndoor;
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private void BindIndoorImageToUI()
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            if (SelectedModel != null)
            {
                DataRow dr = IDuModelTypeList.AsEnumerable().FirstOrDefault(r => r.Field<string>("ModelFull") == SelectedModel);
                if (dr != null)
                {
                    this.IduImagePath = sourceDir + "\\" + Convert.ToString(dr["TypeImage"]);
                }
            }
        }

        private bool ValidateEstCapacity(double est, string partLoadTableName, IndoorType flag)
        {
            bool res = false;
            if (est == 0)
            {
                //if (flag != IndoorType.Exchanger)
                //    JCHMessageBox.ShowWarning(Msg.DB_NOTABLE_CAP + "[" + partLoadTableName + "]\nRegion:" + JCHVRF.Model.Project.GetProjectInstance.SubRegionCode + ";ProductType:" + _productType + "");
            }
            else if (est == -1)
                JCHMessageBox.ShowWarning(Msg.WARNING_DATA_EXCEED);
            else
                res = true;
            return res;
        }

        void BindOutdoorProperty()
        {
            var CurrentSys = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.NewSystem;
            if (CurrentSys == null)
                CurrentSys = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.CurrentSystem;
            if (CurrentSys != null)
            {
                outdoorCoolingDB = CurrentSys.DBCooling;
                outdoorHeatingWB = CurrentSys.WBHeating;
                outdoorCoolingIW = CurrentSys.IWCooling;
                outdoorHeatingIW = CurrentSys.IWHeating;
                ODUSeries = CurrentSys.Series;
                ODUProductType = CurrentSys.ProductType;
            }

        }

        private Indoor BatchCalculateEstValueForIndoor(Indoor idu)
        {
            double wb_c, db_h;
            Indoor inItem = idu.DeepClone<Indoor>();
            var FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
            var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
            wb_c = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            db_h = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            //if (inItem != null)
            //{
                //datarow = IDuModelTypeList.AsEnumerable().FirstOrDefault(r => r.Field<string>("ModelFull") == this.SelectedModel);
                //if (datarow != null)
                //{
                    //Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
                    if (inItem != null)
                    {
                        double est_cool = 0;
                        double est_heat = 0;
                        double est_sh = 0;
                        string type = inItem.Type;
                        if (type.Contains("YDCF") || (type.Contains("Fresh Air") && JCHVRF.Model.Project.CurrentProject.SubRegionCode != "LA_BR") || type.Contains("Ventilation"))
                        {
                            est_cool = inItem.CoolingCapacity;
                            est_heat = inItem.HeatingCapacity;
                            est_sh = inItem.SensibleHeat;
                        }
                        else if (type.Contains("Hydro Free") || type == "DX-Interface")
                        {
                            est_cool = inItem.CoolingCapacity;
                            est_heat = inItem.HeatingCapacity;
                            est_sh = 0d;
                        }
                        else
                        {

                            double db_c = inItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;
                            double wb_h = inItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;


                            est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
                            if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName, inItem.Flag))
                                return inItem;

                            double shf = inItem.GetSHF(FanSpeed);
                            est_sh = est_cool * shf;

                            if (!inItem.ProductType.Contains(", CO"))
                            {

                                est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
                                if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName, inItem.Flag))
                                    return inItem;
                            }
                        }

                inItem.CoolingCapacity = est_cool;
                inItem.HeatingCapacity = est_heat;
                inItem.SensibleHeat = est_sh;
            }
            //}
            // }
            return inItem;
        }

        private void BatchCalculateEstValue()
        {
            double wb_c, db_h;
            var FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
            var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
            wb_c = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            db_h = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            if (this.SelectedModel != null)
            {
                datarow = IDuModelTypeList.AsEnumerable().FirstOrDefault(r => r.Field<string>("ModelFull") == this.SelectedModel);
                if (datarow != null)
                {
                    Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
                    if (inItem != null)
                    {
                        double est_cool = 0;
                        double est_heat = 0;
                        double est_sh = 0;
                        string type = inItem.Type;
                        if (type.Contains("YDCF") || (type.Contains("Fresh Air") && JCHVRF.Model.Project.CurrentProject.SubRegionCode != "LA_BR") || type.Contains("Ventilation"))
                        {
                            est_cool = inItem.CoolingCapacity;
                            est_heat = inItem.HeatingCapacity;
                            est_sh = inItem.SensibleHeat;
                        }
                        else if (type.Contains("Hydro Free") || type == "DX-Interface")
                        {
                            est_cool = inItem.CoolingCapacity;
                            est_heat = inItem.HeatingCapacity;
                            est_sh = 0d;
                        }
                        else
                        {

                            double db_c = inItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;
                            double wb_h = inItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;


                            est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
                            if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName, inItem.Flag))
                                return;

                            double shf = inItem.GetSHF(FanSpeed);
                            est_sh = est_cool * shf;

                            if (!inItem.ProductType.Contains(", CO"))
                            {

                                est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
                                if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName, inItem.Flag))
                                    return;
                            }
                        }

                        this.SR_TotalCapacity = Unit.ConvertToControl(est_cool, UnitType.POWER, utPower).ToString("n1");
                        this.SR_HeatingCapacity = Unit.ConvertToControl(est_heat, UnitType.POWER, utPower).ToString("n1");
                        this.SR_SensibleCapacity = Unit.ConvertToControl(est_sh, UnitType.POWER, utPower).ToString("n1");
                    }
                }
            }

        }

        private void BatchCalculateAirFlow()
        {
            Indoor inItem;
            var FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
            if (this.SelectedModel != null)
            {
                datarow = IDuModelTypeList.AsEnumerable().FirstOrDefault(r => r.Field<string>("ModelFull") == this.SelectedModel);
                if (datarow != null)
                {
                    inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
                    if (inItem != null)
                    {
                        double fa = 0;
                        double airflow = 0;
                        double staticPressure = 0;
                        string type = inItem.Type;
                        if (IndoorBLL.IsFreshAirUnit(type))
                        {
                            fa = inItem.AirFlow;
                        }
                        else if (!type.Contains("Hydro Free") && type != "DX-Interface")
                        {
                            airflow = inItem.GetAirFlow(FanSpeed);
                            if (type.Contains("Ducted") || type.Contains("Total Heat Exchanger"))
                            {
                                staticPressure = inItem.GetStaticPressure();
                            }
                        }

                        this.SR_AirFlow = Unit.ConvertToControl(airflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                        this.SR_ESP = Unit.ConvertToControl(staticPressure, UnitType.STATICPRESSURE, PressureMasureUnit).ToString("n2");//staticPressure.ToString("n0");
                        this.SR_FreshAir = Unit.ConvertToControl(fa, UnitType.AIRFLOW, utAirflow).ToString("n0");

                    }
                }
            }

        }

        private bool IsValidate()
        {
            float DBCoolJCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit).ToString("n1"));
            float DBCoolJCMaxValue = float.Parse(Unit.ConvertToControl(30, UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit).ToString("n1"));
            float WBCoolJCMinValue = float.Parse(Unit.ConvertToControl(14, UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit).ToString("n1"));
            float WBCoolJCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit).ToString("n1"));
            float DBHeatJCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit).ToString("n1"));
            float DBHeatJCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit).ToString("n1"));
            float RHJCMinValue = float.Parse("13");
            float RHJCMaxValue = float.Parse("100");
            bool result = true;
            if (string.IsNullOrWhiteSpace(this.IndoorUnitName))
            {
                this.IndoorError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else if (this.IndoorUnitName.HasSpecialCharacters())
            {
                this.IndoorError = "Special characters not allowed";
                result = false;
            }
            else if (this.IndoorUnitName.Length > 30)
            {
                this.IndoorError = "Name exceeding 30 characters";
                result = false;
            }
            else this.IndoorError = string.Empty;

            if (this.HeightDifference == null)
            {
                this.HeightDifferenceError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.HeightDifferenceError = string.Empty;


            if (String.IsNullOrEmpty(this.SelectedUnitType))
            {
                this.UnitTypeError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.UnitTypeError = string.Empty;

            if (String.IsNullOrEmpty(this.SelectedModel))
            {
                this.ListModelError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.ListModelError = string.Empty;


            if (this.CR_TotalCapacity == null)
            {
                this.CR_TotalCapacityError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.CR_TotalCapacityError = string.Empty;

            if (this.CR_SensibleCapacity == null)
            {
                this.CR_SensibleCapacityError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.CR_SensibleCapacityError = string.Empty;

            if (this.CR_HeatingCapacity == null)
            {
                this.CR_HeatingCapacityError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.CR_HeatingCapacityError = string.Empty;

            if (this.CR_AirFlow == null)
            {
                this.CR_AirFlowError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.CR_AirFlowError = string.Empty;

            if (this.CR_FreshAir == null)
            {
                this.CR_FreshAirError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.CR_FreshAirError = string.Empty;

            if (this.CR_ESP == null)
            {
                this.CR_ESPError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else this.CR_ESPError = string.Empty;


            if (this.CoolingDryBulb == null)
            {
                this.CoolingDryBulbError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else if (this.CoolingDryBulb < DBCoolJCMinValue || this.CoolingDryBulb > DBCoolJCMaxValue)
            {
                this.CoolingDryBulbError = langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(DBCoolJCMinValue) + "," + Convert.ToString(DBCoolJCMaxValue) + "] *";
                result = false;
            }
            else this.CoolingDryBulbError = string.Empty;


            if (this.CoolingWetBulb == null)
            {
                this.CoolingWetBulbError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else if (this.CoolingWetBulb < WBCoolJCMinValue || this.CoolingWetBulb > WBCoolJCMaxValue)
            {
                this.CoolingWetBulbError = langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(WBCoolJCMinValue) + "," + Convert.ToString(WBCoolJCMaxValue) + "] *"; //"Range [14,24] *";
                result = false;
            }
            else this.CoolingWetBulbError = string.Empty;


            if (this.HeatingDryBulb == null)
            {
                this.HeatingDryBulbError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else if (this.HeatingDryBulb < DBHeatJCMinValue || this.HeatingDryBulb > DBHeatJCMaxValue)

            {
                AutoSelectionMessage = langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(DBHeatJCMinValue) + ", " + Convert.ToString(DBHeatJCMaxValue) + "] * ";//"Range[16, 24] * ";";
                this.HeatingDryBulbError = langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(DBHeatJCMinValue) + ", " + Convert.ToString(DBHeatJCMaxValue) + "] * ";//"Range[16,24] *";
                result = false;
            }
            else this.HeatingDryBulbError = string.Empty;


            if (this.RelativeHumidity == null)
            {
                this.RelativeHumidityError = langauge.Current.GetMessage("REQUIRED");
                result = false;
            }
            else if (this.RelativeHumidity < RHJCMinValue || this.RelativeHumidity > RHJCMaxValue)
            {
                this.RelativeHumidityError = langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(RHJCMinValue) + "," + Convert.ToString(RHJCMaxValue) + "] *"; // "Range[13,100] *";
                result = false;
            }
            else this.RelativeHumidityError = string.Empty;

            if (this.CR_TotalCapacity < this.CR_SensibleCapacity)
            {
                JCHMessageBox.Show(langauge.Current.GetMessage("ERROR_IDU_VALIDATION"));
                result = false;
            }
            if (ListRoomIndoor.Any(ind => ind.IndoorName == this.IndoorUnitName))
            {
                JCHMessageBox.Show(langauge.Current.GetMessage("ALERT_IDU_NAME_EXISTS"));
                result = false;
            }

            return result;
        }

        #endregion

        #region ViewModelMethods
        private int GetIndoorCount()
        {
            int InNodeCount = 0;
            if (JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList != null)
                foreach (RoomIndoor ri in JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList)
                {
                    InNodeCount = InNodeCount > ri.IndoorNO ? InNodeCount : ri.IndoorNO;
                }
            if (ListRoomIndoor != null)
                foreach (RoomIndoor ri in ListRoomIndoor)
                {
                    InNodeCount = InNodeCount > ri.IndoorNO ? InNodeCount : ri.IndoorNO;
                }
            return InNodeCount + 1;
        }

        #endregion

        #region DelegateCommandEvents       

        private void ChangeDesignTempOnClick()
        {
            try
            {
                string currentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                if (this.DesignConditionTempMasureUnit == Unit.ut_Temperature_c)
                {
                    this.CoolingDryBulb = Unit.ConvertToControl(this.CoolingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                    this.CoolingWetBulb = Unit.ConvertToControl(this.CoolingWetBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                    this.HeatingDryBulb = Unit.ConvertToControl(this.HeatingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                    this.DesignConditionTempMasureUnit = Unit.ut_Temperature_f;
                    this.ChangeToFandC = "Change to °C";
                }
                else
                {
                    this.CoolingDryBulb = Unit.ConvertToSource(this.CoolingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                    this.CoolingWetBulb = Unit.ConvertToSource(this.CoolingWetBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                    this.HeatingDryBulb = Unit.ConvertToSource(this.HeatingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                    this.DesignConditionTempMasureUnit = Unit.ut_Temperature_c;
                    this.ChangeToFandC = "Change to °F";
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void OpenAddRoomWindowOnClick()
        {
            NavigationParameters param = new NavigationParameters();
            param.Add("ShowSaveCancel", true);
            _winService.ShowView(ViewKeys.AddEditRoom, langauge.Current.GetMessage("ADDEDITROOMS"), param, true, 850, 550);
        }
        private IModalWindowService _winService;
        private void CancelCommandOnClick()
        {
            _winService.Close(ViewKeys.AddIndoorUnitView);
        }

        private void ResetCommandOnClick()
        {
            Refresh();
            // BindInternalDesignConditions();
        }
        private void OpenAddFloorWindowOnClick()
        {
            try
            {
                NavigationParameters param = new NavigationParameters();
                param.Add("EnableSaveButtons", true);
                _winService.ShowView(ViewKeys.FloorTab, langauge.Current.GetMessage("ADD_OR_EDIT_FLOOR"), param);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        private void AddIndoorToListOnClick()
        {
            try
            {
                if (IsValidate())
                {
                    string SystemId;
                    var CurrentSys = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.NewSystem;
                    if (CurrentSys != null)
                        SystemId = CurrentSys.Id;

                    RoomIndoor RoomObj = new RoomIndoor();
                    RoomObj.IndoorName = IndoorUnitName;
                    RoomObj.IndoorNO = GetIndoorCount();

                    if (this.ManualSelection == false)
                    {
                        RoomObj.RqCoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.CR_TotalCapacity), UnitType.POWER, utPower);
                        RoomObj.RqSensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.CR_SensibleCapacity), UnitType.POWER, utPower);
                        RoomObj.RqHeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.CR_HeatingCapacity), UnitType.POWER, utPower);
                        RoomObj.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(this.CR_AirFlow), UnitType.AIRFLOW, utAirflow);
                        RoomObj.RqFreshAir = Unit.ConvertToSource(Convert.ToDouble(this.CR_FreshAir), UnitType.AIRFLOW, utAirflow);
                        RoomObj.RqStaticPressure = Unit.ConvertToSource(Convert.ToDouble(this.CR_ESP), UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(this.CR_ESP);
                    }
                    else
                    {
                        RoomObj.RqCoolingCapacity = 0;
                        RoomObj.RqSensibleHeat = 0;
                        RoomObj.RqHeatingCapacity = 0;
                        RoomObj.RqAirflow = 0;
                        RoomObj.RqFreshAir = 0;
                        RoomObj.RqStaticPressure = 0;
                    }
                    RoomObj.DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    RoomObj.WBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    RoomObj.DBHeating = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    RoomObj.IsAuto = !this.ManualSelection;
                    RoomObj.HeightDiff = Convert.ToDouble(this.HeightDifference);
                    RoomObj.FanSpeedLevel = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
                    RoomObj.SelectedRoom = this.SelectedRoom;
                    if (this.SelectedRoom != null)
                    {
                        RoomObj.RoomID = this.SelectedRoom.Id;
                        RoomObj.RoomName = this.SelectedRoom.Name;
                    }
                    RoomObj.SelectedFloor = JCHVRF.Model.Project.GetProjectInstance.FloorList.FirstOrDefault(i => i.Id == this.SelectedFloor);
                    RoomObj.PositionType = this.SelectedIduPosition;
                    RoomObj.RHCooling = Convert.ToDouble(this.RelativeHumidity);
                    Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
                    if (inItem != null)
                    {
                        inItem.Series = ODUSeries;
                        //inItem.CoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.POWER, utPower);
                        //inItem.SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.POWER, utPower);
                        //inItem.HeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.POWER, utPower);
                        //inItem.AirFlow = Unit.ConvertToSource(Convert.ToDouble(this.SR_AirFlow), UnitType.AIRFLOW, utAirflow);
                        if (JCHVRF.Model.Project.GetProjectInstance.BrandCode == "Y")
                        {
                            if (inItem.Model_York == "-")
                                inItem.Model_York = inItem.ModelFull;
                        }

                        RoomObj.CoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.POWER, utPower);
                        RoomObj.HeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.POWER, utPower);
                        RoomObj.SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.POWER, utPower);
                        RoomObj.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(this.SR_AirFlow), UnitType.AIRFLOW, utAirflow);

                        RoomObj.IndoorItem = inItem;
                        RoomObj.SelectedUnitType = this.SelectedUnitType;

                        if (WorkFlowContext.NewSystem != null)
                            RoomObj.SetToSystemVRF(WorkFlowContext.NewSystem.Id);
                        else if (WorkFlowContext.CurrentSystem != null)
                            RoomObj.SetToSystemVRF(WorkFlowContext.CurrentSystem.Id);
                        RoomObj.DisplayImagePath = this.IduImagePath;
                        RoomObj.DisplayImageName = this.SelectedUnitType;
                        ListRoomIndoor.Add(RoomObj);
                    }
                    


                    if (SelectedRoom != null)
                        this.IndoorUnitName = SelectedRoom.Name;
                    else
                        this.IndoorUnitName = SystemSetting.UserSetting.defaultSetting.IndoorName + GetIndoorCount();
                    //Refresh();
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void Refresh()
        {
            this.IndoorUnitName = SystemSetting.UserSetting.defaultSetting.IndoorName + GetIndoorCount();
            this.SelectedRoom = null;
            this.IsIndoorUnitEditable = true;
            //this.SelectedFloor = null;
            this.CR_TotalCapacity = 0;
            this.CR_SensibleCapacity = 0;
            this.CR_HeatingCapacity = 0;
            this.CR_AirFlow = 0;
            this.CR_FreshAir = 0;
            this.CR_ESP = 0;
            this.UseRoomTemperature = false;
            SelectionForUpdateIndoor = string.Empty;
        }
        private void RemoveSelectedIndoorOnClick(RoomIndoor Item)
        {
            if (ListRoomIndoor != null)
            {
                ListRoomIndoor.Remove(Item);
                this.IndoorUnitName = SystemSetting.UserSetting.defaultSetting.IndoorName + GetIndoorCount();
            }

        }

        private void UpdateSelectedIndoorOnClick(RoomIndoor Item)
        {
            try
            {
                this.IsIndoorUnitEditable = true;
                this.IndoorUnitName = Item.IndoorName;
                if (Item.SelectedRoom != null)
                    this.SelectedRoom = Item.SelectedRoom;
                else
                    this.SelectedRoom = null;
                if (Item.SelectedFloor != null)
                    this.SelectedFloor = Item.SelectedFloor.Id;
                else
                    this.SelectedFloor = null;
                this.ManualSelection = !Item.IsAuto;
                if (Item.IndoorItem != null)
                {
                    this.SelectedUnitType = Item.SelectedUnitType;
                    SelectionForUpdateIndoor = Item.IndoorItem.ModelFull;
                }
                this.CR_TotalCapacity = Unit.ConvertToControl(Convert.ToDouble(Item.RqCoolingCapacity), UnitType.POWER, utPower);
                this.CR_SensibleCapacity = Unit.ConvertToControl(Convert.ToDouble(Item.RqSensibleHeat), UnitType.POWER, utPower);
                this.CR_HeatingCapacity = Unit.ConvertToControl(Convert.ToDouble(Item.RqHeatingCapacity), UnitType.POWER, utPower);
                this.CR_AirFlow = Unit.ConvertToControl(Convert.ToDouble(Item.RqAirflow), UnitType.AIRFLOW, utAirflow);
                this.CR_FreshAir = Unit.ConvertToControl(Convert.ToDouble(Item.RqFreshAir), UnitType.AIRFLOW, utAirflow);
                this.CR_ESP = Unit.ConvertToControl(Convert.ToDouble(Item.RqStaticPressure), UnitType.STATICPRESSURE, PressureMasureUnit);//Item.RqStaticPressure;

                this.CoolingDryBulb = Unit.ConvertToControl(Convert.ToDouble(Item.DBCooling), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                this.CoolingWetBulb = Unit.ConvertToControl(Convert.ToDouble(Item.WBCooling), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                this.HeatingDryBulb = Unit.ConvertToControl(Convert.ToDouble(Item.DBHeating), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                this.HeightDifference = Item.HeightDiff;
                this.SelectedIduPosition = Item.PositionType;
                this.RelativeHumidity = Item.RHCooling;
                this.SelectedFanSpeed = (string)Enum.GetName(typeof(JCHVRF_New.Model.FanSpeed), Item.FanSpeedLevel);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }



        private void AddAllIndoorUnitOnClick()
        {

            try
            {
                AutoSelectionMessage = null;
                var CurrentSys = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.NewSystem;
                if (CurrentSys != null)
                {
                    //AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                    //AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSys, new List<RoomIndoor>(ListRoomIndoor));
                    AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                    List<RoomIndoor> AddListRoomIndoor = new List<RoomIndoor>();
                    if (_project.RoomIndoorList.Count > 0 && _project.RoomIndoorList != null)
                    {
                        AddListRoomIndoor.AddRange(ListRoomIndoor);
                        AddListRoomIndoor.AddRange(_project.RoomIndoorList.Where(ri => ri.SystemID == (((JCHVRF.Model.SystemBase)CurrentSys).Id)).ToList());
                    }
                    else
                    {
                        AddListRoomIndoor.AddRange(ListRoomIndoor);
                    }

                    SetCompositeMode(CurrentSys);
                    AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSys, new List<RoomIndoor>(AddListRoomIndoor));

                    #region Apply condition for Fresh Air
                    if (ListRoomIndoor.Where(Item => Item.DisplayImageName == "Fresh Air") != null && ListRoomIndoor.Where(Item => Item.DisplayImageName == "Fresh Air").ToList().Count > 0)
                    {
                        ListRoomIndoor.Where(Item => Item.DisplayImageName == "Fresh Air").ToList().ForEach((item) =>
                        {
                            if (item.IndoorItem != null)
                                item.IndoorItem.Flag = IndoorType.FreshAir;
                        });
                    }
                    #endregion Apply condition for Fresh air

                    if (result.SelectionResult == SelectOutdoorResult.OK)
                    {

                        if (ListRoomIndoor != null && ListRoomIndoor.Count > 0)
                        {
                            string ListMaxMinidiff = "";
                            bool MaxMincheck = false;
                            List<string> ERRList;
                            foreach (var roomindoor in ListRoomIndoor)
                            {
                                

                                if (roomindoor.PositionType == PipingPositionType.Upper.ToString() && roomindoor.HeightDiff > CurrentSys.MaxOutdoorAboveHeight)
                                {
                                    double len = Unit.ConvertToControl(CurrentSys.MaxOutdoorAboveHeight, UnitType.LENGTH_M, ut_length);
                                    ListMaxMinidiff += " " + roomindoor.IndoorName + "- " + len.ToString("n0") + ut_length;
                                    MaxMincheck = true;
                                }
                                else if (roomindoor.PositionType == PipingPositionType.Lower.ToString() && roomindoor.HeightDiff > CurrentSys.MaxOutdoorBelowHeight)
                                {
                                    double len = Unit.ConvertToControl(CurrentSys.MaxOutdoorBelowHeight, UnitType.LENGTH_M, ut_length);
                                    ListMaxMinidiff += " " + roomindoor.IndoorName + "- " + len.ToString("n0") + ut_length;
                                    MaxMincheck = true;
                                }
                                //ERRList = new List<string>();
                                //double maxValue = CalculateHighDiff(ListRoomIndoor);
                                //if (maxValue > CurrentSys.MaxDiffIndoorHeight)
                                //{
                                //    double len = Unit.ConvertToControl(CurrentSys.MaxDiffIndoorHeight, UnitType.LENGTH_M, ut_length);
                                //    //this.jcLabMsg.Text = Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length);
                                //    //ERRList.Add(Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length));
                                //    ListMaxMinidiff += Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length);
                                //}
                            }

                            if (MaxMincheck == true)
                            {
                                JCMsg.ShowErrorOK(Msg.Piping_HeightDiffH(ListMaxMinidiff));
                                return;
                            }
                            else
                            {
                                _eventAggregator.GetEvent<PubSubEvent<ObservableCollection<RoomIndoor>>>().Publish(ListRoomIndoor);
                                JCHMessageBox.Show(langauge.Current.GetMessage("SAVED_SUCCESSFULLY"), MessageType.Success);
                                //JCHMessageBox.Show(Langauge.Current.GetMessage("VIEW_SUMMARY"), MessageType.Success);
                                _winService.Close(ViewKeys.AddIndoorUnitView);
                            }

                        }
                        else
                            JCHMessageBox.Show(langauge.Current.GetMessage("ALERT_IDU_ADD"), MessageType.Error);
                    }

                    else
                    {
                        if (result.ERRList.Count > 0)
                        {
                            string AlertMessage = "";
                            if (result.MSGList.Count > 0)
                            {
                                AlertMessage = GetErrorMassage(result.MSGList);
                            }                            
                            JCHMessageBox.Show(GetErrorMassage(result.ERRList) + AlertMessage, MessageType.Error);
                        }
                        //if (result.MSGList.Count > 0)
                        //    //This laguage transalation not implement due to dynamic data comming as per logic language translation not implmeneted.   
                        //    JCHMessageBox.Show("The capacity requirements are mismatching.Please add/remove IDUs to make it compatible." + GetErrorMassage(result.MSGList), MessageType.Error);
                        else if (result.ERRList.Count > 0)
                            //This laguage transalation not implement due to dynamic data comming as per logic language translation not implmeneted.   
                            JCHMessageBox.Show("The capacity requirements are mismatching.Please add/remove IDUs to make it compatible" + GetErrorMassage(result.ERRList), MessageType.Error);
                            // JCHMessageBox.Show(langauge.Current.GetMessage("OUTD_RATIO_Composite") +result.ERRList[1], MessageType.Error);
                            
                            //JCHMessageBox.Show(langauge.Current.GetMessage("ERROR_IDU_CAPACITY_MISMATCH_FRESHAIR"));
                        else
                            AutoSelectionMessage = langauge.Current.GetMessage("ERROR_IDU_CAPACITY_MISMATCH");
                        // JCHMessageBox.Show("Error : Capacity Not Matched");

                    }
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private string GetErrorMassage(List<string> StringData)
        {
            string ErrorReturn = string.Empty;
            if (StringData != null && StringData.Count > 0)
            {
                StringData.Distinct().ToList().ForEach((item) =>
                {
                    ErrorReturn += item + "\n";
                });

            }
            return ErrorReturn;

        }
        private double CalculateHighDiff(ObservableCollection<RoomIndoor> listRISelected)
        {
            List<double> diffList = new List<double>(); //高度差集合
            double maxValue = 0; //最大高度差
            double minValue = 0; //最小高度差
            double indDiff = 0; //室内机与室内机直接的高度差
            if (listRISelected.Count > 0)
            {
                foreach (RoomIndoor ri in listRISelected)
                {
                    double val = Convert.ToDouble(ri.HeightDiff);
                    if (ri.PositionType == PipingPositionType.Lower.ToString())
                    {
                        double m = 0 - val;
                        diffList.Add(m);
                    }
                    else
                    {
                        diffList.Add(val);
                    }
                }
                maxValue = Convert.ToDouble(diffList.Max().ToString("n1"));
                minValue = Convert.ToDouble(diffList.Min().ToString("n1"));
                if (maxValue > minValue)
                {
                    indDiff = maxValue - minValue;
                }
            }
            return indDiff;
        }

        #endregion

        #region AutoSeletionMethods

        private void DoAutoSelect()
        {
            //if (_isLoading || _isAutoSelecting || IDUIndoorList == null) return;
            if (IDUIndoorList == null || IDUIndoorList.Count == 0) return;

            if (!ManualSelection)
            {
                var isUniversal = ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance);
                // is universal 
                if (isUniversal)
                {
                    if (SelectedRoom != null)
                    {
                        var tmpRoom = ModulatedCurrentRoomParameters();
                        DoAutoSelectUniversalWithRoom(IDUIndoorList, tmpRoom, CurrentFreshAirArea, _SelectedUnitType);
                    }
                    else
                        DoAutoSelectUniversalWithoutRoom(IDUIndoorList, CR_TotalCapacity.GetValueOrDefault(), CR_HeatingCapacity.GetValueOrDefault(), CR_SensibleCapacity.GetValueOrDefault(), CR_AirFlow.GetValueOrDefault(), CR_ESP.GetValueOrDefault(), CR_FreshAir.GetValueOrDefault());

                }
                else
                {
                    if (SelectedRoom != null)
                    {
                        var tmpRoom = ModulatedCurrentRoomParameters();
                        DoAutoSelectWithRoom(IDUIndoorList, tmpRoom, CurrentFreshAirArea, ODUSeries);
                    }

                    else
                        DoAutoSelectWithoutRoom(_SelectedUnitType, ODUSeries, IDUIndoorList, CR_TotalCapacity.GetValueOrDefault(), CR_HeatingCapacity.GetValueOrDefault(), CR_SensibleCapacity.GetValueOrDefault(), CR_AirFlow.GetValueOrDefault(), CR_ESP.GetValueOrDefault(), CR_FreshAir.GetValueOrDefault());
                }

            }

        }

        private Room ModulatedCurrentRoomParameters()
        {
            var tmpRoom = SelectedRoom.DeepClone();
            tmpRoom.RqCapacityCool = CR_TotalCapacity.GetValueOrDefault();
            tmpRoom.RqCapacityHeat = CR_HeatingCapacity.GetValueOrDefault();
            tmpRoom.FreshAir = CR_AirFlow.GetValueOrDefault();
            tmpRoom.SensibleHeat = CR_SensibleCapacity.GetValueOrDefault();
            tmpRoom.StaticPressure = CR_ESP.GetValueOrDefault();
            return tmpRoom;
        }

        /// <summary>
        /// Do auto select in case of univeral idu without room specification
        /// </summary>
        /// <param name="type"></param>
        /// <param name="productType"></param>
        /// <param name="srcIduList"></param>
        /// <param name="inputRqCapC"></param>
        /// <param name="intputRqCapH"></param>
        /// <param name="inputSensiCapC"></param>
        /// <param name="inputRqAirFlow"></param>
        /// <param name="inputRoomStaticPressure"></param>
        /// <param name="inputFA"></param>
        /// <param name="selectedIndoor"></param>
        private void DoAutoSelectUniversalWithoutRoom(List<Indoor> srcIduList, double? inputRqCapC, double? intputRqCapH,
            double? inputSensiCapC, double? inputRqAirFlow,
            double? inputRoomStaticPressure, double? inputFA, string productType = "Universal IDU")
        {

            var rq_cool = Unit.ConvertToSource(Convert.ToDouble(inputRqCapC), UnitType.POWER, CapacityMasureUnit);
            var rq_heat = Unit.ConvertToSource(Convert.ToDouble(intputRqCapH), UnitType.POWER, CapacityMasureUnit);
            var rq_sensiable = Unit.ConvertToSource(Convert.ToDouble(inputSensiCapC), UnitType.POWER, CapacityMasureUnit);
            var rq_airflow = Unit.ConvertToSource(Convert.ToDouble(inputRqAirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
            var rq_StaticPressure = Unit.ConvertToSource(Convert.ToDouble(inputRoomStaticPressure), UnitType.STATICPRESSURE, PressureMasureUnit); //Convert.ToDouble(inputRoomStaticPressure);
            var rq_fa = Unit.ConvertToSource(Convert.ToDouble(inputFA), UnitType.AIRFLOW, AirFlowMasureUnit);


            var found = false;
            foreach (var indoor in srcIduList)
            {
                Indoor r = BatchCalculateEstValueForIndoor(indoor);
                bool isPass = false;
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(r.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(r.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);

                isPass = (rq_cool <= std_cool) && (rq_heat <= std_heat);

                if (!isPass || !AutoCompareUniversalWithoutRoom(r, rq_fa, rq_cool, rq_sensiable, rq_airflow, rq_StaticPressure, rq_heat))
                    continue;

                SelectedModel = r.ModelFull;
                found = true;

                break;
            }

            if (!found)
            {
                AutoSelectionMessage = Msg.IND_NOTMATCH;
                SelectedModel = null;
            }
            else
                AutoSelectionMessage = null;

        }

        /// <summary>
        ///  Do autoselect in case of universal idu with room specification
        /// </summary>
        /// <param name="srcIduList"></param>
        /// <param name="room"></param>
        /// <param name="freshAirArea"></param>
        /// <param name="curSelectType"></param>
        private void DoAutoSelectUniversalWithRoom(List<Indoor> srcIduList, Room room, FreshAirArea freshAirArea,
            string curSelectType)
        {
            AutoSelectionMessage = null;

#pragma warning disable CS0219 // The variable 'isOK' is assigned but its value is never used
            bool isOK = false;
#pragma warning restore CS0219 // The variable 'isOK' is assigned but its value is never used
            var found = false;
            foreach (var indoor in srcIduList)
            {
                Indoor stdRow = BatchCalculateEstValueForIndoor(indoor);
                bool isFreshAir = IndoorBLL.IsFreshAirUnit(stdRow.Type);
                bool isPass = true;

                // unit conversion              
                double std_cool = Unit.ConvertToControl(Convert.ToDouble(stdRow.CoolingCapacity), JCBase.Utility.UnitType.POWER, CapacityMasureUnit);
                double std_heat = Unit.ConvertToControl(Convert.ToDouble(stdRow.HeatingCapacity), JCBase.Utility.UnitType.POWER, CapacityMasureUnit);

                if (room != null)
                {
                    if (_project.IsCoolingModeEffective && std_cool < room.RqCapacityCool)
                        isPass = false;

                    if (curSelectType == "Hydro Free-High temp.") isPass = true;

                    if (_project.IsHeatingModeEffective && std_heat < room.RqCapacityHeat)
                        isPass = false;

                    if (!isPass || !AutoCompareUniversalWithRoom(isFreshAir, stdRow, room) && curSelectType != "Hydro Free-High temp.")
                        continue;
                    else
                        isOK = true;
                }
                else if (freshAirArea != null)
                {
                    if (!isPass || !AutoCompareFreshAirArea(stdRow, freshAirArea))
                        continue;
                    else
                        isOK = true;
                }

                SelectedModel = stdRow.ModelFull;
                found = true;
                break;
            }

            if (!found)
            {
                AutoSelectionMessage = Msg.IND_NOTMATCH;
                SelectedModel = null;
            }
            else
                AutoSelectionMessage = null;

        }

        /// <summary>
        /// Do auto select in case of non universal idu without room
        /// </summary>
        /// <param name="type"></param>
        /// <param name="oduSeries"></param>
        /// <param name="srcIduList"></param>
        /// <param name="inputRqCapC"></param>
        /// <param name="intputRqCapH"></param>
        /// <param name="inputSensiCapC"></param>
        /// <param name="inputRqAirFlow"></param>
        /// <param name="inputRoomStaticPressure"></param>
        /// <param name="inputFA"></param>
        private void DoAutoSelectWithoutRoom(string type, string oduSeries, List<Indoor> srcIduList, double? inputRqCapC,
            double? intputRqCapH, double? inputSensiCapC, double? inputRqAirFlow, double? inputRoomStaticPressure, double? inputFA)
        {
            if (string.IsNullOrEmpty(type))
                return;

            // do input conversion
            var rq_cool = Unit.ConvertToSource(Convert.ToDouble(inputRqCapC), UnitType.POWER, CapacityMasureUnit);
            var rq_heat = Unit.ConvertToSource(Convert.ToDouble(intputRqCapH), UnitType.POWER, CapacityMasureUnit);
            var rq_sensiable = Unit.ConvertToSource(Convert.ToDouble(inputSensiCapC), UnitType.POWER, CapacityMasureUnit);
            var rq_airflow = Unit.ConvertToSource(Convert.ToDouble(inputRqAirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
            
            var rq_StaticPressure = Unit.ConvertToSource(Convert.ToDouble(inputRoomStaticPressure), UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(inputRoomStaticPressure);
            var rq_fa = Unit.ConvertToSource(Convert.ToDouble(inputFA), UnitType.AIRFLOW, AirFlowMasureUnit);

            var found = false;

            foreach (var indoor in srcIduList)
            {
                Indoor r = BatchCalculateEstValueForIndoor(indoor);
                bool pass = true;
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(r.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(r.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);

                pass = (rq_cool <= std_cool) && (rq_heat <= std_heat);

                if (!pass || !AutoCompareWithoutRoom(type, oduSeries, r, rq_fa, rq_cool, rq_sensiable, rq_airflow, rq_StaticPressure, rq_heat))
                    continue;

                SelectedModel = r.ModelFull;
                found = true;
                break;
            }

            if (!found)
            {
                AutoSelectionMessage = Msg.IND_NOTMATCH;
                SelectedModel = null;
            }
            else
                AutoSelectionMessage = null;

            return;

        }

        /// <summary>
        /// Do auto select as per room / fresh air area specific requirements and non univeral IDUs
        /// </summary>
        /// <param name="srcIduList"></param>
        /// <param name="room"></param>
        /// <param name="freshAirArea"></param>
        /// <param name="oduSeries"></param>
        private void DoAutoSelectWithRoom(List<Indoor> srcIduList, Room room, FreshAirArea freshAirArea, string oduSeries)
        {
            var isOK = false;

            var found = false;
            if (srcIduList == null || srcIduList.Count == 0)
                return;

            foreach (var indoor in srcIduList)
            {
                Indoor idu = BatchCalculateEstValueForIndoor(indoor);
                var isFreshAir = IndoorBLL.IsFreshAirUnit(idu.Type);
                var pass = true;
                double stdCool = 0.0;
                double stdHeat = 0.0;

                double wb_c, db_h;
                var FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
                var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
                wb_c = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                db_h = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);


                if (CapacityMasureUnit == "BTU/h")
                {
                    double est_cool = 0;
                    double est_heat = 0;
                    double est_sh = 0;
                    string type = idu.Type;
                    if (type.Contains("YDCF") || (type.Contains("Fresh Air") && JCHVRF.Model.Project.CurrentProject.SubRegionCode != "LA_BR") || type.Contains("Ventilation"))
                    {
                        est_cool = idu.CoolingCapacity;
                        est_heat = idu.HeatingCapacity;
                        est_sh = idu.SensibleHeat;
                    }
                    else if (type.Contains("Hydro Free") || type == "DX-Interface")
                    {
                        est_cool = idu.CoolingCapacity;
                        est_heat = idu.HeatingCapacity;
                        est_sh = 0d;
                    }
                    else
                    {
                        double db_c = idu.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;
                        double wb_h = idu.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;

                        est_cool = bll.CalIndoorEstCapacity(idu, db_c, wb_c, false);
                        if (!ValidateEstCapacity(est_cool, idu.PartLoadTableName, idu.Flag))
                            return;

                        double shf = idu.GetSHF(FanSpeed);
                        est_sh = est_cool * shf;

                        if (!idu.ProductType.Contains(", CO"))
                        {

                            est_heat = bll.CalIndoorEstCapacity(idu, wb_h, db_h, true);
                            if (!ValidateEstCapacity(est_heat, idu.PartLoadTableName, idu.Flag))
                                return;
                        }
                    }
                    var coolingCapacity = Unit.ConvertToControl(est_cool, UnitType.POWER, utPower).ToString("n1");
                    var HeatingCapacity = Unit.ConvertToControl(est_cool, UnitType.POWER, utPower).ToString("n1");
                    stdCool = Unit.ConvertToSource(Convert.ToDouble(coolingCapacity), UnitType.POWER, utPower);
                    stdHeat = Unit.ConvertToSource(Convert.ToDouble(HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
                }
                else
                {
                    stdCool = Unit.ConvertToControl(Convert.ToDouble(idu.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
                    stdHeat = Unit.ConvertToControl(Convert.ToDouble(idu.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
                }

                if (room != null)
                {
                    if (CapacityMasureUnit == "BTU/h")
                    {
                        double est_cool = Convert.ToDouble(room.RqCapacityCool.ToString());
                        var roomRqCapacityCool = Unit.ConvertToSource(est_cool, UnitType.POWER, utPower);
                        if (_project.IsCoolingModeEffective && stdCool < roomRqCapacityCool)
                        {
                            pass = false;
                        }
                    }
                    else
                    {
                        if (_project.IsCoolingModeEffective && stdCool < room.RqCapacityCool)
                        {
                            pass = false;
                        }
                    }
                    if (CapacityMasureUnit == "BTU/h")
                    {
                        double est_heat = Convert.ToDouble(room.RqCapacityHeat.ToString());
                        var roomRqCapacityCool = Unit.ConvertToSource(est_heat, UnitType.POWER, utPower);
                        if (_project.IsHeatingModeEffective && !oduSeries.Contains(", CO") && stdHeat < roomRqCapacityCool)
                            pass = false;
                    }
                    else
                    {
                        if (_project.IsHeatingModeEffective && !oduSeries.Contains(", CO") && stdHeat < room.RqCapacityHeat)
                            pass = false;
                    }
                    if (!pass || !AutoCompare(isFreshAir, idu, room, oduSeries))
                        continue;
                    else
                        isOK = true;
                }
                else if (freshAirArea != null)
                {
                    if (!pass || !AutoCompareFreshAirArea(idu, freshAirArea))
                        continue;
                    else
                        isOK = true;
                }

                if (isOK)
                {
                    SelectedModel = idu.ModelFull;
                    found = true;
                }

                break;
            }

            if (!found)
            {
                AutoSelectionMessage = Msg.IND_NOTMATCH;
                SelectedModel = null;
            }
            else
                AutoSelectionMessage = null;


            return;
        }

        /// <summary>
        /// Compare requirements and return true if finds a match
        /// </summary>
        /// <param name="isFreshAir"></param>
        /// <param name="stdRow"></param>
        /// <param name="room"></param>
        /// <param name="productType"></param>
        /// <returns></returns>
        private bool AutoCompare(bool isFreshAir, Indoor stdRow, Room room, string productType)
        {
            if (stdRow == null) return false;
            bool pass = true;
            double est_cool = 0.0;
            double est_heat = 0.0;
            double est_sh = 0.0;
            double airflow = 0.0;
            double staticPressure = 0.0;
            if (CapacityMasureUnit == "BTU/h")
            {
                double wb_c, db_h;
                var FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
                var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
                wb_c = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                db_h = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);

                string type = stdRow.Type;
                if (type.Contains("YDCF") || (type.Contains("Fresh Air") && JCHVRF.Model.Project.CurrentProject.SubRegionCode != "LA_BR") || type.Contains("Ventilation"))
                {
                    est_cool = stdRow.CoolingCapacity;
                    est_heat = stdRow.HeatingCapacity;
                    est_sh = stdRow.SensibleHeat;
                }
                else if (type.Contains("Hydro Free") || type == "DX-Interface")
                {
                    est_cool = stdRow.CoolingCapacity;
                    est_heat = stdRow.HeatingCapacity;
                    est_sh = 0d;
                }
                else
                {
                    double db_c = stdRow.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;
                    double wb_h = stdRow.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;
                    est_cool = bll.CalIndoorEstCapacity(stdRow, db_c, wb_c, false);
                    double shf = stdRow.GetSHF(FanSpeed);
                    est_sh = est_cool * shf;

                    if (!stdRow.ProductType.Contains(", CO"))
                    {
                        est_heat = bll.CalIndoorEstCapacity(stdRow, wb_h, db_h, true);
                    }
                }
                var coolingCapacity = Unit.ConvertToControl(est_cool, UnitType.POWER, utPower).ToString("n1");
                var heating = Unit.ConvertToControl(est_heat, UnitType.POWER, utPower).ToString("n1");
                est_cool = Unit.ConvertToSource(Convert.ToDouble(coolingCapacity), UnitType.POWER, CapacityMasureUnit);
                est_heat = Unit.ConvertToSource(Convert.ToDouble(heating), UnitType.POWER, CapacityMasureUnit);
                est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
                 airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit), 0);
                airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
                staticPressure = Convert.ToDouble(stdRow.GetStaticPressure());
            }
            else
            {

                //est_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
                est_cool = Convert.ToDouble(stdRow.CoolingCapacity);
               // est_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
                est_heat = Convert.ToDouble(stdRow.CoolingCapacity);
                //est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
                est_sh = Convert.ToDouble(stdRow.SensibleHeat);
                airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit), 0);
                airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
                //airflow = Convert.ToDouble(stdRow.AirFlow);
                staticPressure = Unit.ConvertToSource(Convert.ToDouble(stdRow.GetStaticPressure()), UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(stdRow.GetStaticPressure());
            }
            if (isFreshAir)
            {
                airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit), 0);
                airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
                //airflow = Convert.ToDouble(stdRow.AirFlow);
                // Compare estimated capacity to current demand
                if (airflow < room.FreshAir)
                    pass = false;
            }
            else
            {
                if (_project.IsCoolingModeEffective)
                {
                    if (CapacityMasureUnit == "BTU/h")
                    {
                        double est_cools = Convert.ToDouble(room.RqCapacityCool.ToString());
                        var roomRqCapacityCool = Unit.ConvertToSource(est_cools, UnitType.POWER, utPower);
                        if (est_cool < roomRqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
                            pass = false;
                    }
                    else
                    {
                        if (est_cool < room.RqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
                            pass = false;
                    }
                }

                if (_project.IsHeatingModeEffective && !productType.Contains(", CO"))
                {
                    if (CapacityMasureUnit == "BTU/h")
                    {
                        double est_heats = Convert.ToDouble(room.RqCapacityHeat.ToString());
                        var roomRqCapacityCool = Unit.ConvertToSource(est_heats, UnitType.POWER, utPower);
                        if (est_heat < roomRqCapacityCool)
                            pass = false;
                    }
                    else
                    {
                        if (est_heat < room.RqCapacityHeat)
                            pass = false;
                    }
                }
            }
            return pass;
        }

        /// <summary>
        /// Compare for fresh air requirement and return true in case finds a match
        /// </summary>
        /// <param name="stdRow"></param>
        /// <param name="area"></param>
        /// <param name="utAirflow"></param>
        /// <returns></returns>
        private bool AutoCompareFreshAirArea(Indoor stdRow, FreshAirArea area)
        {
            bool pass = true;
            //DoCalculateEstValue(stdRow);            
            double airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit), 0);
            airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
            //double airflow = Convert.ToDouble(stdRow.AirFlow);

            if (airflow < area.FreshAir)
                pass = false;

            return pass;
        }

        private bool AutoCompareWithoutRoom(string type, string productType, Indoor r,
            double rq_fa, double rq_cool, double rq_sensiable, double rq_airflow, double rq_StaticPressure, double rq_heat)
        {
            bool pass = true;

            bool isFreshAir = type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation");


            //double est_cool = Unit.ConvertToSource(Convert.ToDouble(r.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_cool = Convert.ToDouble(r.CoolingCapacity);
            //double est_heat = Unit.ConvertToSource(Convert.ToDouble(r.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Convert.ToDouble(r.HeatingCapacity);
            //double est_sh = Unit.ConvertToSource(Convert.ToDouble(r.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Convert.ToDouble(r.SensibleHeat);
            double airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit),0);
            airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
            //double airflow = Convert.ToDouble(r.AirFlow);
            double staticPressure = Unit.ConvertToSource(Convert.ToDouble(r.GetStaticPressure()), UnitType.STATICPRESSURE, PressureMasureUnit);//r.GetStaticPressure();

            if (isFreshAir)
            {
                //airflow = Unit.ConvertToSource(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
                if (airflow < rq_fa)
                    pass = false;
            }
            else
            {
                if (_project.IsCoolingModeEffective)
                {
                    if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow || staticPressure < rq_StaticPressure)
                        pass = false;                  

                }

                if (_project.IsHeatingModeEffective && !productType.Contains(", CO"))
                {
                    if (est_heat < rq_heat)
                        pass = false;
                }
            }

            return pass;
        }

        /// <summary>
        /// Compare compatible indoor units univerally
        /// </summary>
        /// <param name="isFreshAir"></param>
        /// <param name="stdRow"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        private bool AutoCompareUniversalWithRoom(bool isFreshAir, Indoor stdRow, Room room)
        {
            bool pass = true;

            //double est_cool = Unit.ConvertToControl(Convert.ToDouble(stdRow.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_cool = Convert.ToDouble(stdRow.CoolingCapacity);
            //double est_heat = Unit.ConvertToControl(Convert.ToDouble(stdRow.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Convert.ToDouble(stdRow.HeatingCapacity);
           // double est_sh = Unit.ConvertToControl(Convert.ToDouble(stdRow.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Convert.ToDouble(stdRow.SensibleHeat);
            double airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit), 0);
            airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
            //double airflow = Convert.ToDouble(stdRow.AirFlow);
            double staticPressure = Unit.ConvertToSource(Convert.ToDouble(stdRow.GetStaticPressure()), UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(stdRow.GetStaticPressure());

            if (isFreshAir)
            {
                //airflow = Unit.ConvertToControl(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);

                if (airflow < room.FreshAir)
                    pass = false;
            }
            else
            {
                if (_project.IsCoolingModeEffective)
                {
                    if (est_cool < room.RqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
                        pass = false;
                }

                if (_project.IsHeatingModeEffective)
                {
                    if (est_heat < room.RqCapacityHeat)
                        pass = false;
                }
            }
            return pass;
        }

        private bool AutoCompareUniversalWithoutRoom(Indoor r,
            double rq_fa, double rq_cool, double rq_sensiable, double rq_airflow, double rq_StaticPressure, double rq_heat,
            string productType = "Universal IDU")
        {
            bool pass = true;

            //var modelfull = r.ModelFull;
            //var inItem = r.IndoorItem as Indoor;
            //if (inItem == null) return false;

            var type = r.Type;

            bool isFreshAir = type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation");
            //double est_cool = Unit.ConvertToSource(Convert.ToDouble(r.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_cool = Convert.ToDouble(r.CoolingCapacity);
            //double est_heat = Unit.ConvertToSource(Convert.ToDouble(r.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Convert.ToDouble(r.HeatingCapacity);
            //double est_sh = Unit.ConvertToSource(Convert.ToDouble(r.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Convert.ToDouble(r.SensibleHeat);
            double airflow = Math.Round(Unit.ConvertToControl(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit), 0);
            airflow = Unit.ConvertToSource(Convert.ToDouble(airflow), UnitType.AIRFLOW, AirFlowMasureUnit);
            //double airflow = Convert.ToDouble(r.AirFlow);
            double staticPressure = Unit.ConvertToSource(Convert.ToDouble(r.GetStaticPressure()), UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(r.GetStaticPressure());

            if (isFreshAir)
            {
                //airflow = Unit.ConvertToSource(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
                if (airflow < rq_fa)
                    pass = false;
            }
            else
            {
                if (_project.IsCoolingModeEffective)
                {
                    if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow || staticPressure < rq_StaticPressure)
                        pass = false;
                }
                if (_project.IsHeatingModeEffective && !productType.Contains(", CO"))
                {
                    if (est_heat < rq_heat)
                        pass = false;
                }
            }

            return pass;
        }

        #endregion

        //private void AddSelectedIndoor()
        //{
        //    //has selected row
        //    //find if the selected indoor has matching producttype(ODU type)
        //    //add selected indoor to the selected list 'addToSelectedRow'
        //    //do calculateselectedsumcapacity(optional)
        //    //DoValidateCapacity (optional in case of autoselection)
        //}

        private void SetCompositeMode(JCHVRF.Model.NextGen.SystemVRF CurrentSys)
        {
            if (CurrentSys != null)
            {
                if (ListRoomIndoor != null && ListRoomIndoor.Count > 0)
                {
                    if (ListRoomIndoor.Count == 1 || ListRoomIndoor.Count(mm => mm.IndoorItem.Flag == IndoorType.FreshAir)>=1)
                    {
                        if (ListRoomIndoor.Count(mm => mm.IndoorItem.Flag == IndoorType.FreshAir) >= 1)
                        {
                            CurrentSys.SysType = SystemType.OnlyIndoor;
                        }
                    }
                    else if (ListRoomIndoor.Count(mm => mm.IndoorItem.Flag == IndoorType.FreshAir) >= 1)
                    {
                        CurrentSys.SysType = SystemType.CompositeMode;
                    }
                    else
                    {
                        CurrentSys.SysType = SystemType.OnlyIndoor;
                    }
                }

            }
        }

    }
}
