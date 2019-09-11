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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using JCHVRF.BLL;
using JCBase.UI;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Constants;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Contracts;
using Prism.Regions;

namespace JCHVRF_New.ViewModels
{
    /// <summary>
    //  ACC-SKM-1638 
    /// 
    /// </summary>
    // ACC-SKM-1638 START
    class TotalHeatExUnitInfoViewModel : ViewModelBase
    {
        #region Fields
        JCHVRF.BLL.OutdoorBLL bll;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        string navigateToFolder = "..\\..\\Image\\TypeImageProjectCreation";
        string navigateToFolderWithNewImage = "..\\..\\Image\\TypeImages";
        Project thisProject;
        private IEventAggregator _eventAggregator;
        private Project _project;
        DataTable dtModel = new DataTable();
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string utArea = SystemSetting.UserSetting.unitsSetting.settingAREA;
        public static bool flgValidateUnit = false;

        public DelegateCommand NumericOutdoorCDBCommand { get; set; }
        public DelegateCommand NumericOutdoorCWBCommand { get; set; }
        public DelegateCommand NumericOutdoorHDBCommand { get; set; }
        public DelegateCommand NumericRHommand { get; set; }
        public DelegateCommand ValidateUnitNameCommand { get; set; }

        #endregion Fields         

        public TotalHeatExUnitInfoViewModel(IEventAggregator EventAggregator,IModalWindowService winService, JCHVRF_New.Model.LightProject thisProj)
        {

            try
            {
                NumericOutdoorCDBCommand = new DelegateCommand(NumericOutdoorCDB_LostFocus);
                NumericOutdoorCWBCommand = new DelegateCommand(NumericOutdoorCWBCommand_LostFocus);
                NumericOutdoorHDBCommand = new DelegateCommand(NumericOutdoorHDBCommand_LostFocus);
                NumericRHommand = new DelegateCommand(NumericRHommand_LostFocus);
                //CreateClickCommand = new DelegateCommand(CreateHENextClick);
                ChangeTempCommand = new DelegateCommand(btnChangeTempUClicked);
                AddFloorCommand = new DelegateCommand(OnAddFloorClicked);
                AddEditRoomCommand = new DelegateCommand(OnAddEditRoomClicked);
                ValidateUnitNameCommand = new DelegateCommand(ValidateUnitNameOnLostFocus);
                _eventAggregator = EventAggregator;
                _winService = winService;
                _eventAggregator.GetEvent<RoomListSaveSubscriber>().Subscribe(GetRoomList);
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Subscribe(OpenGetFloorList);
                GetRoomList();
                BindFloor();
                //BindDefaultFanSpeed();
                _eventAggregator.GetEvent<TheuInfoVisibility>().Subscribe(OnTypeSelected);
                _eventAggregator.GetEvent<BeforeCreate>().Subscribe(OnBeforeCreate);
                _eventAggregator.GetEvent<Cleanup>().Subscribe(OnCleanup);
                _indoorBll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                WorkFlowContext.FloorNames = null;
                bll = new JCHVRF.BLL.OutdoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                if (JCHVRF.Model.Project.GetProjectInstance.DesignCondition != null)
                {
                    BindInternalDesignConditions();
                }
                WorkFlowContext.FloorNames = new List<string>();

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        private bool IsCreateButtonEnabled()
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

            if (ValidateOdb() == false || IsCreateButtonEnabled() == false)
            {

                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(true);
            }

        }

        private void NumericOutdoorCWBCommand_LostFocus()
        {

            if (ValidateCoolWetBulb() == false || IsCreateButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(true);
            }
        }

        private void NumericOutdoorHDBCommand_LostFocus()
        {

            if (ValidateHDDBT() == false || IsCreateButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(true);
            }
        }

        private void NumericRHommand_LostFocus()
        {

            if (ValidateOutdoorHDRH() == false || IsCreateButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(true);
            }
        }

        private void ValidateUnitNameOnLostFocus()
        {
            if (ValidateUnitName() == false || IsCreateButtonEnabled() == false)
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(false);
            }
            else
            {
                _eventAggregator.GetEvent<CreateButtonEnability>().Publish(true);
            }
        }

        private void OnCleanup()
        {
            _eventAggregator.GetEvent<RoomListSaveSubscriber>().Unsubscribe(GetRoomList);
            _eventAggregator.GetEvent<FloorListSaveSubscriber>().Unsubscribe(OpenGetFloorList);
            _eventAggregator.GetEvent<TheuInfoVisibility>().Unsubscribe(OnTypeSelected);
            _eventAggregator.GetEvent<BeforeCreate>().Unsubscribe(OnBeforeCreate);
            _eventAggregator.GetEvent<Cleanup>().Unsubscribe(OnCleanup);
        }

        private void BindCapacityRequirements()
        {
            if (this.SelectedRoom != null)
            {

                this.FreshAir = Unit.ConvertToControl(SelectedRoom.FreshAir, UnitType.AIRFLOW, utAirflow);
                this.ESPVal = SelectedRoom.StaticPressure;
                this.Area = Unit.ConvertToControl(SelectedRoom.Area, UnitType.AREA, utArea);
            }
        }

        //UPDATING FRESH AIR VALUE FROM UI TO PROPERTY USING SETTER
        private void BindFreshAir()
        {
            if (JCHVRF.Model.Project.GetProjectInstance.RoomList.Count > 0)
            {
                JCHVRF.Model.Project.GetProjectInstance.RoomList[0].FreshAir = this.FreshAir;
                JCHVRF.Model.Project.GetProjectInstance.RoomList[0].Area = this.Area;
            }
        }

        private void OnTypeSelected(Visibility visibility)
        {

        }

        //BINDING ROOM COMBOBOX WITH CURRENT PROJECT FLOOR AND ROOM
        private void GetRoomList()
        {
            if (JCHVRF.Model.Project.GetProjectInstance.RoomList != null && JCHVRF.Model.Project.GetProjectInstance.RoomList.Count > 0)
            {
                //CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
                _project = JCHVRF.Model.Project.GetProjectInstance;
                RoomName = new ObservableCollection<Room>(JCHVRF.Model.Project.GetProjectInstance.RoomList);
            }
            if (SelectedRoom != null)
            {
                SetEditedPropertyValue();
            }
        }

        //getting series from database
        private void GetSeriesTHeatEx()
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            ListSeries = new ObservableCollection<SeriesModel>();

            DataTable dtSeries = bll.GetOutdoorListStdSeries(JCHVRF.Model.Project.GetProjectInstance.SubRegionCode);
            foreach (DataRow dtSeriesTypeRow in dtSeries.Rows)
            {
                if (ListSeries.Count == 0)
                {
                    ListSeries.Add(new SeriesModel()
                    {
                        DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                        SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                    });
                }
                else
                {
                    if (ListSeries.Any(MM => MM.DisplayName == dtSeriesTypeRow.ItemArray[0].ToString()))
                    {
                        continue;
                    }
                    else
                    {
                        ListSeries.Add(new SeriesModel()
                        {
                            DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                            SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),

                        });
                    }

                }

            }

        }
        //GETTING POWER FROM DATABASE USING SELECTED SERIES AND BINDING WITH COMBOBOX
        private void BindPowerMode()
        {
            Power = new ObservableCollection<PowerModel>();
            string _series = UserSelSeries;
            if (!string.IsNullOrWhiteSpace(_series))
            {
                thisProject = _project;

                DataTable dtPower = Global.InitPowerList(thisProject, _series);
                foreach (DataRow dtSeriesTypeRow in dtPower.Rows)
                {
                    if (Power.Count == 0)
                    {
                        Power.Add(new PowerModel()
                        {
                            DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                            SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                            Code = dtSeriesTypeRow.ItemArray[1].ToString()

                        });
                    }
                    else
                    {
                        if (Power.Any(MM => MM.DisplayName == dtSeriesTypeRow.ItemArray[0].ToString()))
                        {
                            continue;
                        }
                        else
                        {
                            Power.Add(new PowerModel()
                            {
                                DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                                SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                                Code = dtSeriesTypeRow.ItemArray[1].ToString()

                            });
                        }

                    }

                }
                if (Power != null && Power.Count > 0)
                {
                    SelectedPower = Power[0];
                }
            }
        }

        //GETTING SERIES FROM DATABASE AND BINDING WITH COMBOBOX
        private void BindSeries()
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            SelectedSeries = new SeriesModel();
            _project = Project.GetProjectInstance;
            ListSeries = new ObservableCollection<SeriesModel>();
            _indoorBll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
            DataTable dtSeries = _indoorBll.GetExchangerTypeList();
            int excount = _project.ExchangerList.Count;

            if (dtSeries != null)
            {
                foreach (DataRow dtSeriesTypeRow in dtSeries.Rows)
                {
                    if (ListSeries.Count == 0)
                    {
                        ListSeries.Add(new SeriesModel()
                        {
                            DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                            SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                        });
                    }
                    else
                    {
                        if (ListSeries.Any(MM => MM.DisplayName == dtSeriesTypeRow.ItemArray[0].ToString()))
                        {
                            continue;
                        }
                        else
                        {
                            ListSeries.Add(new SeriesModel()
                            {
                                DisplayName = dtSeriesTypeRow.ItemArray[0].ToString(),
                                SelectedValues = dtSeriesTypeRow.ItemArray[0].ToString(),
                            });
                        }

                    }

                }

                //Binding of default series
                if(ListSeries.Count > 0 && excount>0)
                {
                        SelectedSeries = ListSeries.First(mm => mm.SelectedValues.Equals(_project.ExchangerList[excount - 1].IndoorItem.Series));
                }
                else if(ListSeries.Count > 0)
                {
                    SelectedSeries = ListSeries[0];
                }
            }
        }

        // ACC-SKM-1638 END
        void Init()
        {
            this.DesignConditionTempMasureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            _project = Project.GetProjectInstance;
            BindFanSpeed();
            BindFloor();
            BindSeries();
            outdoorCoolingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(_project.DesignCondition.outdoorCoolingDB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            outdoorCoolingWB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(_project.DesignCondition.outdoorHeatingWB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            outdoorHeatingRH = Convert.ToDouble((_project.DesignCondition.outdoorHeatingRH));
            outdoorHeatingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(_project.DesignCondition.outdoorHeatingDB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            NumericOutdoorCDB_LostFocus();
            NumericOutdoorHDBCommand_LostFocus();
            NumericOutdoorCWBCommand_LostFocus();
            NumericRHommand_LostFocus();
            Initialisationvalues();
            ValidateUnitNameOnLostFocus();
        }
        // ACC-SKM-1638 START

        //Unit name according to room or default unit name
        void Initialisationvalues()
        {
            bUnit = true;
            if (SelectedRoom != null)
            {
                if (string.IsNullOrWhiteSpace(SelectedRoom.Name.ToString()))
                {
                    this.DesignConditionTempMasureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                    UnitName = SystemSetting.UserSetting.defaultSetting.ExchangerName + (JCHVRF.Model.Project.CurrentProject.HeatExchangerSystems.Count + 1);
                    if (string.IsNullOrEmpty(UnitName))
                        UnitName = "Exc Unit 01";
                    this.IsHENameEditable = false;
                    this.UnitNameColor = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    UnitName = SelectedRoom.Name.ToString();
                    bUnit = false;
                    //ACC - RAG
                    //Read-only Unit name
                    this.IsHENameEditable = true;
                    this.UnitNameColor = new SolidColorBrush(Colors.Gray);
                    SetEditedPropertyValue();
                }

            }
            else
            {
                this.DesignConditionTempMasureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                UnitName = SystemSetting.UserSetting.defaultSetting.ExchangerName + " " + (JCHVRF.Model.Project.CurrentProject.ExchangerList.Count + 1);
                if (string.IsNullOrEmpty(UnitName))
                    UnitName = "Exc Unit 01";
                this.IsHENameEditable = false;
                this.UnitNameColor = new SolidColorBrush(Colors.Black);
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

        private void OnAddFloorClicked()
        {
            WorkFlowContext.FloorNames = new List<string>();
            NavigationParameters param = new NavigationParameters();
            param.Add("EnableSaveButtons", true);
            _winService.ShowView(ViewKeys.FloorTab, Langauge.Current.GetMessage("ADD_OR_EDIT_FLOOR"), param);
        }

        private void OpenGetFloorList()
        {
            BindFloor();
        }

        private void OnAddEditRoomClicked()
        {
            NavigationParameters param = new NavigationParameters();
            param.Add("ShowSaveCancel", true);
            _winService.ShowView(ViewKeys.AddEditRoom, Langauge.Current.GetMessage("ADDEDITROOMS"), param, true, 850, 550);
        }



        void btnChangeTempUClicked()
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

        //BIND FAN SPEED
        private void BindFanSpeed()
        {
            FanSpeeds = new ObservableCollection<FanSpeed>() { FanSpeed.High, FanSpeed.Medium, FanSpeed.Low };
            this.SFanSpeed = JCHVRF_New.Model.FanSpeed.High;
        }
        private void ValidateOnTempChange()
        {
            ValidateHDDBT();
            ValidateOdb();
            ValidateOutdoorHDRH();
            ValidateCoolWetBulb();

        }
        private string _designConditionTempMasureUnite;
        public string DesignConditionTempMasureUnit
        {
            get { return _designConditionTempMasureUnite; }
            set { this.SetValue(ref _designConditionTempMasureUnite, value); }
        }

        //VARIABLE FOR ENABLE OR DISABLE TEXT BOX UNIT NAME
        private bool _bUnit;
        public bool bUnit
        {
            get
            {
                return _bUnit;
            }
            set
            {
                this.SetValue(ref _bUnit, value);
            }
        }

        //BINDING FLOOR WITH COMBOBOX
        private void BindFloor()
        {
            this.FloorList = new ObservableCollection<JCHVRF.Model.Floor>(JCHVRF.Model.Project.GetProjectInstance.FloorList);
            if (this.FloorList != null && this.FloorList.Count > 0 && this.SIndex == null)
            {
                this.SIndex = this.FloorList.FirstOrDefault();
            }
        }

        //BINDING DESIGN CONDITION TEMP VARIABLES FROM DESIGN CONDITION VARIABLES
        private void BindInternalDesignConditions()
        {
            var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
            this.outdoorCoolingDB = Unit.ConvertToControl(Convert.ToDouble(InternalDesignConditions.outdoorCoolingDB), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            this.outdoorCoolingWB = Unit.ConvertToControl(Convert.ToDouble(InternalDesignConditions.outdoorHeatingWB), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            this.outdoorHeatingDB = Unit.ConvertToControl(Convert.ToDouble(InternalDesignConditions.outdoorHeatingDB), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            this.outdoorHeatingRH = Convert.ToDouble(InternalDesignConditions.outdoorHeatingRH);

        }


        //GET COUNT OF INDOORS(ES) ADDED
        private int GetIndoorCount()
        {
            int exNodeCount = 0;
            if (JCHVRF.Model.Project.GetProjectInstance.ExchangerList != null)
                foreach (RoomIndoor ri in JCHVRF.Model.Project.GetProjectInstance.ExchangerList)
                {
                    exNodeCount = exNodeCount > ri.IndoorNO ? exNodeCount : ri.IndoorNO;
                }
            if (ListRoomIndoor != null)
                foreach (RoomIndoor ri in ListRoomIndoor)
                {
                    exNodeCount = exNodeCount > ri.IndoorNO ? exNodeCount : ri.IndoorNO;
                }
            return exNodeCount + 1;
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
                Initialisationvalues();
                if (IsCreateButtonEnabled())
                {
                    _eventAggregator.GetEvent<CreateButtonEnability>().Publish(true);
                }
                else
                {
                    _eventAggregator.GetEvent<CreateButtonEnability>().Publish(false);
                }
            }
        }

        private Floor _sIndex;
        public Floor SIndex
        {
            get { return _sIndex; }
            set
            {
                this.SetValue(ref _sIndex, value);

            }
        }

        private FreshAirArea _currentFreshAirArea;

        public FreshAirArea CurrentFreshAirArea
        {
            get { return _currentFreshAirArea; }
            private set
            {
                SetValue<FreshAirArea>(ref _currentFreshAirArea, value);
            }
        }
        private ObservableCollection<FanSpeed> _fanSpeeds;

        public ObservableCollection<FanSpeed> FanSpeeds
        {
            get { return _fanSpeeds; }
            set { this.SetValue(ref _fanSpeeds, value); }
        }


        private ObservableCollection<Floor> _floor;

        public ObservableCollection<Floor> FloorList
        {
            get { return _floor; }
            set { this.SetValue(ref _floor, value); }
        }

        private ObservableCollection<SeriesModel> listSeries;

        public ObservableCollection<SeriesModel> ListSeries
        {
            get { return listSeries; }
            set
            {
                this.SetValue(ref listSeries, value);
                BindPowerMode();
            }

        }

        private string UserSelSeries;

        private SeriesModel _selectedSeries;
        public SeriesModel SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                this.SetValue(ref _selectedSeries, value);
                //ACC - SHIV
                if (SelectedSeries != null)
                {
                    UserSelSeries = SelectedSeries.SelectedValues;
                }
                BindPowerMode();
            }
        }

        //ACC - RAG START 
        private PowerModel _selectedPower;
        public PowerModel SelectedPower
        {
            get { return _selectedPower; }
            set
            {
                this.SetValue(ref _selectedPower, value);
            }
        }

        private ObservableCollection<ComboBox> _listModel;
        public ObservableCollection<ComboBox> ListModel
        {
            get
            {
                return _listModel;
            }
            set
            {
                this.SetValue(ref _listModel, value);
            }
        }
        //ACC - RAG END

        private ObservableCollection<PowerModel> _power;

        public ObservableCollection<PowerModel> Power
        {
            get { return _power; }
            set
            {
                this.SetValue(ref _power, value);

            }

        }

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

        private double _eSPVal;
        public double ESPVal
        {
            get { return _eSPVal; }
            set
            {
                this.SetValue(ref _eSPVal, value);

            }
        }

        private double _area;
        public double Area
        {
            get { return _area; }
            set
            {
                this.SetValue(ref _area, value);
            }
        }

        private string _unitName;
        public string UnitName
        {
            get { return _unitName; }
            set
            {
                this.SetValue(ref _unitName, value);
                if (string.IsNullOrWhiteSpace(UnitName)) { flgValidateUnit = false; } else { flgValidateUnit = true; }
            }
        }

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
                BindInternalDesignConditions();
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
                //  BindFreshAir();
            }
        }
        private string _freshAirError;
        public string FreshAirError
        {
            get { return _freshAirError == null ? string.Empty : _freshAirError; }
            set
            {
                this.SetValue(ref _freshAirError, value);
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
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                this.SetValue(ref _name, value);
            }
        }

        private void BindDefaultFanSpeed()
        {
            this.SFanSpeed = JCHVRF_New.Model.FanSpeed.High;
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
        private string _sfloor;

        public string SFloor
        {
            get { return _sfloor; }
            set
            {
                this.SetValue(ref _sfloor, value);
            }
        }


        private string _sR_Name;
        public string SRName
        {
            get { return _sR_Name; }
            set
            {
                this.SetValue(ref _sR_Name, value);
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
            get { return SystemSetting.UserSetting.unitsSetting.settingESP; }
            set
            {
                SystemSetting.UserSetting.unitsSetting.settingESP = value;
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

        private double _indoorCoolingRH;
        public double indoorCoolingRH
        {
            get
            {
                return _indoorCoolingRH;
            }
            set
            {
                this.SetValue(ref _indoorCoolingRH, value);

            }

        }

        private decimal _indoorHeatingDB;
        public decimal indoorHeatingDB
        {
            get
            {
                return _indoorHeatingDB;
            }
            set
            {
                this.SetValue(ref _indoorHeatingDB, value);

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
        }//= 7.0m;


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

        #region Delegate Commands

        public DelegateCommand ChangeTempCommand { get; set; }

        // public DelegateCommand ChangeRoom { get; set; }

        public DelegateCommand AddFloorCommand { get; set; }
        public DelegateCommand AddEditRoomCommand { get; set; }


        #endregion


        private bool _isSelected;
        private IndoorBLL _indoorBll;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    Init();
                }

                RaisePropertyChanged("IsSelected");
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
        private IModalWindowService _winService;

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
        public void OnBeforeCreate()
        {
            try
            {
                if (!WorkFlowContext.Systemid.Equals("2"))
                {
                    return;
                }
                CreateHeatExchanger();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private void CreateHeatExchanger()
        {
            string newRoomName = "";
            string newRoomId = "";
            double newRoomFreshAir = 0;
            string powerSelected = "";

            double airFlow = 0;
            double ESP = 0;

            Indoor inItem = null;
            var fanspeedLevel = (int)_sfanspeed;

            if (UserSelSeries != null && SelectedPower != null)
            {
                _indoorBll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                DataTable dt = _indoorBll.GetExchnagerListStd(UserSelSeries, "", SelectedPower.Code);
                dt.DefaultView.Sort = "AirFlow";

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    //DataRow dr = GetAutoModel(dt.DefaultView.ToTable().Rows);
                    _indoorBll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                    string ProductType = string.Empty;
                    if (thisProject.RegionCode== "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
                    {
                        ProductType = "Universal IDU";
                    }
                    else { ProductType = UserSelSeries; }

                    inItem = _indoorBll.GetItem(dr["ModelFull"].ToString(), UserSelSeries, ProductType, UserSelSeries);
                    if (inItem != null)
                    {
                        inItem.Series = UserSelSeries;
                        //Required field for Report
                        inItem.DisplayName = UserSelSeries;
                    }
                }

            }
            if (SelectedRoom != null)
            {
                newRoomName = SelectedRoom.Name;
                newRoomId = SelectedRoom.Id;
                newRoomFreshAir = SelectedRoom.FreshAir;
                SelectedRoom.FreshAir = FreshAir;
                SelectedRoom.StaticPressure = ESPVal;
                SelectedRoom.Area = Area;
                SelectedRoom.PeopleNumber = NoOfPeople;
            }
            else
            {
                newRoomName = "";
                newRoomId = null;
                newRoomFreshAir = FreshAir;
            }
            if (SelectedPower != null)
            {
                powerSelected = SelectedPower.SelectedValues;
            }

            airFlow = newRoomFreshAir;
            ESP = ESPVal;

            Project.CurrentProject.ExchangerList.Add(new RoomIndoor
            {
                IndoorName = UnitName,
                IndoorItem = inItem,
                Power = powerSelected,
                FanSpeedLevel = fanspeedLevel,
                RqFreshAir = airFlow,
                RqAirflow = airFlow,
                Area = Area,
                RqStaticPressure = ESP,
                NumberOfPeople = NoOfPeople,
                DBCooling = outdoorCoolingDB,
                WBCooling = outdoorCoolingWB,
                DBHeating = outdoorHeatingDB,
                RHCooling = outdoorHeatingRH,
                RoomName = newRoomName,
                RoomID = newRoomId,
                SelectedFloor = _sIndex,
                SystemID = WorkFlowContext.NewSystem.Id,
                IsExchanger = true,
                HECanvFreshAirUnit = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW,
                HECanvAreaUnit = SystemSetting.UserSetting.unitsSetting.settingAREA,
                HECanvTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE
            });

        }

        /* function to selct the model based on entered airflow data */
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

        // ACC-SKM-1638 END

        //Outdoor Cooling Dry Bulb
        private bool ValidateOdb()
        {
            double nOdb = Convert.ToDouble(outdoorCoolingDB); //Convert.ToDouble(outdoorCoolingDB);
            lbloutdoorCoolingDB = string.Empty;
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
            if (string.IsNullOrWhiteSpace(uName))
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
    }

}