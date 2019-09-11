using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Helpers;
using JCHVRF.Model;
using JCHVRF_New.Common.Contracts;
using Prism.Commands;
using Prism.Events;
using System.Windows;
using System.Data;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using JCBase.Utility;
using JCHVRF_New.Model;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using System.Windows.Media;
using JCHVRF.BLL.New;
using System.IO;
using JCHVRF_New.Views;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Windows.Threading;
using Microsoft.Win32;
using Prism.Regions;
using Prism.Commands;
using Prism.Events;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using JCHVRF_New.Utility;


namespace JCHVRF_New.ViewModels
{
    public class HeatExchangerCanvasEquipmentViewModel : ViewModelBase
    {

        JCHVRF.DAL.IndoorDAL _dal;
        JCHVRF.BLL.IndoorBLL _bll;
        private Project _project;
        JCHVRF.Model.RoomIndoor proj;
        Project thisProject;
        Indoor inItem = null;
        internal static bool flgTabChanged=true;

        private bool initialized;
        double modelFreshAir;
        double modelEsp;
        string modelfull;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        string navigateToFolderWithLegacyImage = "..\\..\\Image\\TypeImageProjectCreation";
        internal static bool IsPropOrNewExch;

        private bool IsroomSelected = false;

        NavigationParameters param = new NavigationParameters();
        public DelegateCommand MouseClickCommand { get; set; }

        private IEventAggregator _eventAggregator;
        IRegionManager regionManager;

        public DelegateCommand AddEditRoomCommand { get; set; }
        public DelegateCommand AddFloorCommand { get; set; }
        public DelegateCommand NumericOutdoorCDBCommand { get; set; }
        public DelegateCommand NumericOutdoorHDBCommand { get; set; }
        public DelegateCommand NumericOutdoorHDWBTCommand { get; set; }
        public DelegateCommand NumeroutdoorHDRHCommand { get; set; }
        public DelegateCommand ChangeTempCommand { get; set; }
        public DelegateCommand AddAccessoryCommandHE { get; set; }

        public DelegateCommand ShowGADrawingPDF { get; set; }

        public DelegateCommand ShowFanPerformance { get; set; }
        public DelegateCommand ShowInstallationGuide { get; set; }

        public DelegateCommand AddSalesDataCommandHe { get; set; }
        string listGlobal_var;
        #region properties

        #region unit name

        private string _unitName;
        [Required(ErrorMessage = "Unit Name is Mandatory")]
        public string UnitName
        {
            get { return _unitName; }
            set
            {
                this.SetValue(ref _unitName, value);
                if (IsroomSelected == false)
                {
                    NotifyPropertiesUpdate();
                }
            }
        }

        private bool _isHENameEditable;
        public bool IsHENameEditable
        {
            get { return _isHENameEditable; }
            set
            {
                SetValue(ref _isHENameEditable, value);
            }
        }

        private Brush _unitNameColor;

        public Brush UnitNameColor
        {
            get { return _unitNameColor; }
            set
            {
                SetValue(ref _unitNameColor, value);
            }
        }
        #endregion unitname

        #region room 

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

        private Room _SelectedRoom;
        public Room SelectedRoom
        {
            get { return _SelectedRoom; }
            set
            {
                this.SetValue(ref _SelectedRoom, value);
                IsroomSelected = true;
                UpdateUnitName();
                AutoModelForRoomChange();
                NotifyPropertiesUpdate();
                IsroomSelected = false;
            }
        }
        #endregion room

        #region floor 
        private ObservableCollection<Floor> _floor;
        public ObservableCollection<Floor> FloorList
        {
            get { return _floor; }
            set { this.SetValue(ref _floor, value); }
        }

        private Floor _sIndex;
        public Floor SelectedFloor
        {
            get { return _sIndex; }
            set
            {
                this.SetValue(ref _sIndex, value);
                if (_sIndex != null)
                {
                    NotifyPropertiesUpdate();
                }
            }
        }
        #endregion floor

        #region outdoor conditions
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

        #region Cooling Dry bulb
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
                if (TempUnitChange == false)
                {
                    NotifyPropertiesUpdate();
                }
            }
        }

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

        private string _displayCurrentTempUnit;
        public string DisplayCurrentTempUnit
        {
            get { return CurrentTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c; ; }
            set
            {
                this.SetValue(ref _displayCurrentTempUnit, value);
                NotifyPropertiesUpdate();
            }
        }

        #endregion cooling dry bulb

        #region Heating dry bulb
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
                if (TempUnitChange == false)
                {
                    NotifyPropertiesUpdate();
                }
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
        }
        #endregion heating dry bulb

        #region Heating wet bulb
        private double _outdoorHeatingWB;
        public double outdoorHeatingWB
        {
            get
            {
                return _outdoorHeatingWB;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingWB, value);
                if (TempUnitChange == false)
                {
                    NotifyPropertiesUpdate();
                }
            }
        }

        private string _lbloutdoorHeatingWB;
        public string lbloutdoorHeatingWB
        {
            get
            {
                return _lbloutdoorHeatingWB;
            }
            set
            {
                this.SetValue(ref _lbloutdoorHeatingWB, value);
            }
        }
        #endregion Heating wet bulb

        #region Heating relative humidity
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
                NotifyPropertiesUpdate();
            }
        }

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
        #endregion Heating relative humidity 

        #endregion outdoor condition

        private ObservableCollection<SeriesModel> listSeries;
        public ObservableCollection<SeriesModel> ListSeries
        {
            get { return listSeries; }
            set
            {
                this.SetValue(ref listSeries, value);
            }
        }

        private SeriesModel _selectedSeries;
        public SeriesModel SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                JCHVRF_New.Model.SeriesModel oldSeries = _selectedSeries;
                this.SetValue(ref _selectedSeries, value);

                if (proj.Power == null)
                {
                    SelectedPower = null;
                }
                if (SelectedSeries != null)
                {
                    IsPropOrNewExch = false;
                    BindPowerMode();
                    if(oldSeries != null && oldSeries != _selectedSeries)
                    {
                        if (proj.ListAccessory != null)
                        {
                            proj.ListAccessory.Clear();
                        }
                        RemoveControllerGroupID();
                    }
                }
            }
        }

        private void RemoveControllerGroupID()
        {
            var system = Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(_currentSystem.Id));         
            if (system.ControlGroupID.Count > 0)
            {
                foreach(var controlsystem in Project.CurrentProject.ControlSystemList)
                {
                    var HElistonly = controlsystem.SystemsOnCanvasList.FindAll(item => item.System.GetType() == typeof(RoomIndoor));
                    var findHE = HElistonly.Find(he => ((RoomIndoor)he.System).SystemID == system.SystemID);
                    controlsystem.SystemsOnCanvasList.Remove(findHE);
                }
                system.ControlGroupID.Clear();
                JCHMessageBox.Show("Change in the series will delete this System from Central controller!", MessageType.Warning, MessageBoxButton.OK);
            }
        }

        private ObservableCollection<PowerModel> _power;
        public ObservableCollection<PowerModel> Power
        {
            get { return _power; }
            set
            {
                this.SetValue(ref _power, value);

            }
        }

        private PowerModel _selectedPower;
        public PowerModel SelectedPower
        {
            get { return _selectedPower; }
            set
            {
                this.SetValue(ref _selectedPower, value);
                BindModel();
                if (_selectedPower != null)
                {
                    NotifyPropertiesUpdate();
                }
            }
        }

        private ObservableCollection<string> _listModel;
        public ObservableCollection<string> ListModel
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

        private string _selectedModel;
        public string SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                this.SetValue(ref _selectedModel, value);
                if (_selectedModel != null && SelectedSeries != null)
                {
                    ReadDataFromExcel();
                }
                SelectionChangedSelectedModelEvent();
                if (manualSelChecked == true && _selectedModel != null)
                {
                    GetModelFull();
                    NotifyPropertiesUpdate();
                }
                ValidateHeatEx();
            }
        }
        string DataValue, DataValue_dataSheet;
        OleDbConnection conn, conn_Sales;
        private void ReadDataFromExcel()
        {
            var output = new StringBuilder();
            String CurDir = Directory.GetCurrentDirectory();
            String dirPath = Directory.GetCurrentDirectory();
            string FilePath = Path.GetFullPath(Path.Combine(dirPath, @"..\..\"));
            FilePath += "TotalHeatExchangerPDFFiles";
            FilePath += "\\Total Heat Exchanger Models Data Extraction.xls";
            //String FileName = "F:\\Shilpa\\Total Heat Exchanger Models Data Extraction_new.xls";
            string PathConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FilePath + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1;\";";
            conn = new OleDbConnection(PathConn);
            conn_Sales = new OleDbConnection(PathConn);

            OleDbDataAdapter adapter, adapter_salesData, adapterSample;
            //if(SelectedSeries==null)
            //{
            //}
            try
            {
                DataValue = SelectedSeries.SelectedValues;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => JCHMessageBox.Show("Please select the series for getting Sales Data!", MessageType.Warning, MessageBoxButton.OK)));
            }

            if (DataValue == null)
            {
                return;
            }
            else if (DataValue.Contains("HAPQ"))
            {
                DataValue = "KPI(HAPQ)";
            }
            else if (DataValue.Contains("E4E"))
            {
                DataValue = "KPI-E4E";
            }
            else if (DataValue.Contains("-1"))
            {
                DataValue = "KPI-1";
            }
            else
            {
                //Do nothing
            }
            if (SelectedModel != null)
            {
                conn.Open();
                if (DataValue == "KPI-1")
                {
                    string Value_Power = SelectedPower.DisplayName;
                    adapter = new OleDbDataAdapter("Select * FROM [" + DataValue + "$] WHERE [Model name] = '" + SelectedModel + "' AND [Power Supply] = '" + Value_Power + "'", conn);
                    DataTable dt = new DataTable();
                    int NoofRows = adapter.Fill(dt);
                    FillObjectData(dt);
                }
                else
                {
                    string modelName = "";
                    modelName = SelectedModel;
                    // Added to handle the name mismatch for YORK in database(with or without O)
                    if (Project.CurrentProject.BrandCode == "Y")
                    {
                        int indexOfO = modelName.IndexOf('O');
                        if (indexOfO == -1)
                        {
                            modelName = modelName.Insert(10, "O");
                        }
                    }
                    adapter = new OleDbDataAdapter("Select * FROM [" + DataValue + "$] WHERE [Model name] = '" + modelName + "'", conn);
                    DataTable dt = new DataTable();
                    int NoofRows = adapter.Fill(dt);
                    FillObjectData(dt);
                }

                DataValue_dataSheet = "Sales Data";
                adapter_salesData = new OleDbDataAdapter("Select * FROM [" + DataValue_dataSheet + "$] WHERE [Model] = '" + SelectedModel + "'", conn_Sales);
                DataTable dt1 = new DataTable();
                int NoofRows_salesData = adapter_salesData.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    FillObjectSalesData(dt1);
                }

                conn.Close();
                //Selected heatexchanger info
                switch ((int)SFanSpeed)
                {
                    case 3:
                        TempExEff = obj.TempExchangeEff_Low;
                        EnEfCooling = obj.EnthalpyExEffCool_low;
                        EnEfHeating = obj.EnthalpyExchangeEffHeat_low;
                        break;
                    case 2:
                        TempExEff = obj.TempExchangeEff_medium;
                        EnEfCooling = obj.EnthalpyExEffCool_medium;
                        EnEfHeating = obj.EnthalpyExchangeEffHeat_medium;
                        break;
                    default:
                        TempExEff = obj.TempExchangeEff_high;
                        EnEfCooling = obj.EnthalpyExEffCool_high;
                        EnEfHeating = obj.EnthalpyExchangeEffHeat_high;
                        break;
                }

            }
            else
            {
                return;
            }

        }

        private void FillObjectSalesData(DataTable dt_Sales)
        {
            int Col_index = 1;
            obj.OuterDimensions_height = dt_Sales.Rows[0].ItemArray[Col_index++].ToString();
            obj.OuterDimensions_width = dt_Sales.Rows[0].ItemArray[Col_index++].ToString();
            obj.OuterDimensions_depth = dt_Sales.Rows[0].ItemArray[Col_index++].ToString();
            obj.NetWeight = dt_Sales.Rows[0].ItemArray[Col_index++].ToString();
            if (SelectedModel == "KPI-125H-A-GQ")
            {
                obj.ConnectionDuctDiameter = "320 x 250 + 320 x 250";
            }
            else if (SelectedModel == "KPI-150H-E-GQ")
            {
                obj.ConnectionDuctDiameter = "400×320 + 400×320";
            }
            else if (SelectedModel == "KPI-200H-E-GQ")
            {
                obj.ConnectionDuctDiameter = "400×320 + 400×320";
            }
            else if (SelectedModel == "KPI-250H-E-GQ")
            {
                obj.ConnectionDuctDiameter = "500×350 + 500×350";
            }
            else if (SelectedModel == "KPI-300H-E-GQ")
            {
                obj.ConnectionDuctDiameter = "500×350 + 500×350";
            }
            else if (SelectedModel == "KPF-400H-E-GQ")
            {
                obj.ConnectionDuctDiameter = "400×320 + 590×320";
            }
            else if (SelectedModel == "KPF-500H-E-GQ")
            {
                obj.ConnectionDuctDiameter = "500×350 + 700×320";
            }
            else
            {
                obj.ConnectionDuctDiameter = dt_Sales.Rows[0].ItemArray[Col_index].ToString();
            }

        }

        HeatExEquipmentInfoModel obj;
        private void FillObjectData(DataTable dataTable)
        {
            obj = new HeatExEquipmentInfoModel();
            //int Col_index=3;
            Regex objNumber = new Regex("[0-9]+[.]*[0-9]*");
            //DataRow row = dataTable.Rows[0];

            if (DataValue.Contains("HAPQ") && dataTable.Rows.Count > 0)
            {
                LoadHAPQData(dataTable);
            }
            else if (DataValue.Contains("E4E"))
            {
                LoadFPIE4EData(dataTable);
            }
            else if (DataValue.Contains("-1"))
            {
                LoadKPIIData(dataTable);
            }
            else
            {
                //Do nothing
            }


        }

        private void LoadKPIIData(DataTable dataTable)
        {

            int Col_index = 3;
            Regex objNumber = new Regex("[0-9]+[.]*[0-9]*");
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (Hi)"))
            {
                obj.AirFlowProp_High = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_High != "-" && obj.AirFlowProp_High != "")
                {
                }
                else
                {
                    obj.AirFlowProp_High = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_High = "-";
            }

            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (Me)"))
            {
                obj.AirFlowProp_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_medium != "-" && obj.AirFlowProp_medium != "")
                {
                }
                else
                {
                    obj.AirFlowProp_medium = "-";
                }

                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (Lo)"))
            {
                obj.AirFlowProp_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_low != "-" && obj.AirFlowProp_low != "")
                {
                }
                else
                {
                    obj.AirFlowProp_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_low = "-";
            }
            //ExternalStaticPressure
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (Hi)"))
            {
                obj.ExternalStaticPressure_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_high != "-" && obj.ExternalStaticPressure_high != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (Me)"))
            {
                obj.ExternalStaticPressure_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_medium != "-" && obj.ExternalStaticPressure_medium != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (Lo)"))
            {
                obj.ExternalStaticPressure_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_low != "-" && obj.ExternalStaticPressure_low != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_low = "-";
            }

            //TempExchangeEff
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && (dataTable.Columns.Contains("Temp Exchange Efficiency (High)")))
            {
                obj.TempExchangeEff_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_high != "-" && obj.TempExchangeEff_high != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Temp Exchange Efficiency (Me)"))
            {
                obj.TempExchangeEff_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_medium != "-" && obj.TempExchangeEff_medium != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Temp Exchange Efficiency (Low)"))
            {
                obj.TempExchangeEff_Low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_Low != "-" && obj.TempExchangeEff_Low != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_Low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_Low = "-";
            }

            //EnthalpyExchangeEffHeat
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (Hi)"))
            {
                obj.EnthalpyExchangeEffHeat_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_high != "-" && obj.EnthalpyExchangeEffHeat_high != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (Me)"))
            {
                obj.EnthalpyExchangeEffHeat_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_medium != "-" && obj.EnthalpyExchangeEffHeat_medium != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (Lo)"))
            {
                obj.EnthalpyExchangeEffHeat_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_low != "-" && obj.EnthalpyExchangeEffHeat_low != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_low = "-";
            }

            //EnthalpyExchangeEffCool
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for Cooling (Hi)"))
            {
                obj.EnthalpyExEffCool_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_high != "-" && obj.EnthalpyExEffCool_high != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExEffCool_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for cooling (Me)"))
            {
                obj.EnthalpyExEffCool_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_medium != "-" && obj.EnthalpyExEffCool_medium != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExEffCool_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && (dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for Cooling (Lo)") || dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for Cooling (LO)")))
            {
                obj.EnthalpyExEffCool_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_low != "-" && obj.EnthalpyExEffCool_low != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_low = "-";
                }
            }
            else
            {
                obj.EnthalpyExEffCool_low = "-";
            }
        }

        private void LoadFPIE4EData(DataTable dataTable)
        {
            int Col_index = 3;
            Regex objNumber = new Regex("[0-9]+[.]*[0-9]*");
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (Low)"))
            {
                obj.AirFlowProp_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_low != "-" && obj.AirFlowProp_low != "")
                {
                }
                else
                {
                    obj.AirFlowProp_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_low = "-";
            }
            string sam = dataTable.Rows[0].ItemArray[Col_index].ToString();
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow(Me)"))
            {
                obj.AirFlowProp_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_medium != "-" && obj.AirFlowProp_medium != "")
                {
                }
                else
                {
                    obj.AirFlowProp_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (High)"))
            {
                obj.AirFlowProp_High = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_High != "-" && obj.AirFlowProp_High != "")
                {
                }
                else
                {
                    obj.AirFlowProp_High = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_High = "-";
            }
            //ExternalStaticPressure
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (HI)"))
            {
                obj.ExternalStaticPressure_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_high != "-" && obj.ExternalStaticPressure_high != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (Me)"))
            {
                obj.ExternalStaticPressure_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_medium != "-" && obj.ExternalStaticPressure_medium != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (Lo)"))
            {
                obj.ExternalStaticPressure_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_low != "-" && obj.ExternalStaticPressure_low != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_low = "-";
            }

            //TempExchangeEff
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && (dataTable.Columns.Contains("Temp Exchange Efficiency (Low)")))
            {
                obj.TempExchangeEff_Low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_Low != "-" && obj.TempExchangeEff_Low != "")
                {
                }
                else
                {

                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_Low = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Temp Exchange Efficiency (Me)"))
            {
                obj.TempExchangeEff_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_medium != "-" && obj.TempExchangeEff_medium != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_medium = "-";
            }
            string valcheck = dataTable.Rows[0].ItemArray[Col_index].ToString();
            bool check = dataTable.Columns.Contains("Temp Exchange Efficiency (High)");
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Temp Exchange Effciency (High)"))
            {
                obj.TempExchangeEff_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_high != "-" && obj.TempExchangeEff_high != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_high = "-";
            }

            //EnthalpyExchangeEffHeat
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (High)"))
            {
                obj.EnthalpyExchangeEffHeat_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_high != "-" && obj.EnthalpyExchangeEffHeat_high != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (Me)"))
            {
                obj.EnthalpyExchangeEffHeat_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_medium != "-" && obj.EnthalpyExchangeEffHeat_medium != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (Lo)"))
            {
                obj.EnthalpyExchangeEffHeat_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_low != "-" && obj.EnthalpyExchangeEffHeat_low != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_low = "-";
            }

            //EnthalpyExchangeEffCool
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for cooling (High)"))
            {
                obj.EnthalpyExEffCool_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_high != "-" && obj.EnthalpyExEffCool_high != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExEffCool_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for cooling (Me)"))
            {
                obj.EnthalpyExEffCool_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_medium != "-" && obj.EnthalpyExEffCool_medium != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExEffCool_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for cooling (Lo)"))
            {
                obj.EnthalpyExEffCool_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_low != "-" && obj.EnthalpyExEffCool_low != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_low = "-";
                }
            }
            else
            {
                obj.EnthalpyExEffCool_low = "-";
            }
        }

        private void LoadHAPQData(DataTable dataTable)
        {
            int Col_index = 3;
            Regex objNumber = new Regex("[0-9]+[.]*[0-9]*");
            bool check = objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString());
            string val = dataTable.Rows[0].ItemArray[Col_index].ToString();
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (Low)"))
            {
                obj.AirFlowProp_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_low != "-" && obj.AirFlowProp_low != "")
                {

                }
                else
                {
                    obj.AirFlowProp_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_low = "-";
            }
            string Temp = dataTable.Rows[0].ItemArray[Col_index].ToString();
            //check = dataTable.Columns.Contains("Air Flow(Me)");
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "NA" || Temp == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow(Me)"))
            {
                if (Temp == "NA" || Temp == "-")
                {
                    obj.AirFlowProp_medium = "-";
                    Col_index = Col_index + 2;
                }
                else
                {
                    obj.AirFlowProp_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                    if (obj.AirFlowProp_medium != "-" && obj.AirFlowProp_medium != "")
                    {
                    }
                    else
                    {
                        obj.AirFlowProp_medium = "-";
                    }
                    Col_index = Col_index + 2;
                }

            }
            else
            {
                obj.AirFlowProp_medium = "-";
            }

            //check = dataTable.Columns.Contains("Air Flow (High)");
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Air Flow (High)"))
            {
                obj.AirFlowProp_High = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.AirFlowProp_High != "-" && obj.AirFlowProp_High != "")
                {
                }
                else
                {
                    obj.AirFlowProp_High = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.AirFlowProp_High = "-";
            }
            //ExternalStaticPressure
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure [Hi(H-ESP)]") || dataTable.Columns.Contains("External Static Pressure (Hi(H-ESP))"))
            {
                obj.ExternalStaticPressure_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_high != "-" && obj.ExternalStaticPressure_high != "")
                {
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure [Hi(L-ESP)]") || dataTable.Columns.Contains("External Static Pressure (Hi(L-ESP))"))
            {
                obj.ExternalStaticPressure_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_medium != "-" && obj.ExternalStaticPressure_medium != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("External Static Pressure (Lo)"))
            {
                obj.ExternalStaticPressure_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.ExternalStaticPressure_low != "-" && obj.ExternalStaticPressure_low != "")
                {
                }
                else
                {
                    obj.ExternalStaticPressure_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.ExternalStaticPressure_low = "-";
            }

            //TempExchangeEff
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && (dataTable.Columns.Contains("Temp Exchange Efficiency (High)")))
            {
                obj.TempExchangeEff_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_high != "-" && obj.TempExchangeEff_high != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Temp Exchange Efficiency (Me)"))
            {
                obj.TempExchangeEff_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_medium != "-" && obj.TempExchangeEff_medium != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Temp Exchange Efficiency (Low)"))
            {
                obj.TempExchangeEff_Low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.TempExchangeEff_Low != "-" && obj.TempExchangeEff_Low != "")
                {
                }
                else
                {
                    obj.TempExchangeEff_Low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.TempExchangeEff_Low = "-";
            }

            //EnthalpyExchangeEffHeat
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-")
                && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating - Hi(H-ESP)"))
            {
                obj.EnthalpyExchangeEffHeat_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_high != "-" && obj.EnthalpyExchangeEffHeat_high != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_high = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_high = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating  - Hi(L-ESP)"))
            {
                obj.EnthalpyExchangeEffHeat_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_medium != "-" && obj.EnthalpyExchangeEffHeat_medium != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for heating (Lo)"))
            {
                obj.EnthalpyExchangeEffHeat_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExchangeEffHeat_low != "-" && obj.EnthalpyExchangeEffHeat_low != "")
                {
                }
                else
                {
                    obj.EnthalpyExchangeEffHeat_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExchangeEffHeat_low = "-";
            }

            //EnthalpyExchangeEffCool
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for cooling - LO"))
            {
                obj.EnthalpyExEffCool_low = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_low != "-" && obj.EnthalpyExEffCool_low != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_low = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExEffCool_low = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for cooling (Me)"))
            {
                obj.EnthalpyExEffCool_medium = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_medium != "-" && obj.EnthalpyExEffCool_medium != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_medium = "-";
                }
                Col_index = Col_index + 2;
            }
            else
            {
                obj.EnthalpyExEffCool_medium = "-";
            }
            if ((objNumber.IsMatch(dataTable.Rows[0].ItemArray[Col_index].ToString()) || dataTable.Rows[0].ItemArray[Col_index].ToString() == "" || dataTable.Rows[0].ItemArray[Col_index].ToString() == "-") && dataTable.Columns.Contains("Enthalpy Exchange Efficiency - for Cooling (Lo)"))
            {
                obj.EnthalpyExEffCool_high = dataTable.Rows[0].ItemArray[Col_index].ToString();
                if (obj.EnthalpyExEffCool_high != "-" && obj.EnthalpyExEffCool_high != "")
                {
                }
                else
                {
                    obj.EnthalpyExEffCool_high = "-";
                }
            }
            else
            {
                obj.EnthalpyExEffCool_high = "-";
            }
        }

        private bool _manualSelChecked;
        public bool manualSelChecked
        {
            get { return _manualSelChecked; }
            set
            {
                this.SetValue(ref _manualSelChecked, value);
                if (initialized)
                {
                    BindModel();
                }
                if (manualSelChecked == false)
                {
                    isListModelEditable = false;
                    NotifyPropertiesUpdate();
                }
                else
                {
                    DCErrorMessage = string.Empty;
                    isListModelEditable = true;
                    //Clears the data when manual selecton is checked and model is null
                    if(SelectedModel == null)
                    {
                        HEImagePath = "";
                        SelHEModel = "";
                        SelHEAirflow = "";
                        SelHEESP = "";
                        SelTEF = "";
                        SelCooling = "";
                        SelHeating = "";
                    }
                }
            }
        }

        private bool _isListModelEditable;
        public bool isListModelEditable
        {
            get { return _isListModelEditable; }
            set { this.SetValue(ref _isListModelEditable, value); }
        }

        private ObservableCollection<FanSpeed> _fanSpeeds;
        public ObservableCollection<FanSpeed> FanSpeeds
        {
            get { return _fanSpeeds; }
            set { this.SetValue(ref _fanSpeeds, value); }
        }

        private FanSpeed _sfanspeed;
        public FanSpeed SFanSpeed
        {
            get { return _sfanspeed; }
            set
            {
                this.SetValue(ref _sfanspeed, value);
                AutoModelForRoomChange();
                //ReadDataFromExcel();
                FanSpeedDepProp();
                if (ChangedSelectedModelFlag == false)
                {
                    NotifyPropertiesUpdate();
                }
            }
        }

        #region Capacity requirement Properties
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
                if (IsroomSelected == false)
                {
                    AutoModelForRoomChange();
                    NotifyPropertiesUpdate();
                }
            }
        }

        private ObservableCollection<string> _AirFlowUnit = new ObservableCollection<string>() { "l/s", "m³/min", "cfm", "m³/h" };
        public ObservableCollection<string> AirFlowUnit
        {
            get { return _AirFlowUnit; }
            set { _AirFlowUnit = value; }
        }

        private string _currentFreshAirUnit;
        public string CurrentFreshAirUnit
        {
            get { return _currentFreshAirUnit; }
            set
            {
                string prevunit = _currentFreshAirUnit;
                this.SetValue(ref _currentFreshAirUnit, value);
                if (prevunit != null)
                {
                    if (!flgTabChanged)
                    {
                        if (MasterDesignerViewModel._isSysProp) { prevunit=SystemSetting.UserSetting.unitsSetting.settingAIRFLOW; }
                        FreshAir = ConvertFreshAirval(prevunit, _currentFreshAirUnit, FreshAir);
                       // flgTabChanged = true;
                    }
                  //  flgTabChanged = false;
                  MasterDesignerViewModel._isSysProp = false;
                }
                if (manualSelChecked == true)
                {
                    FanSpeedDepProp();
                }
            }
        }

        private double _eSPVal;
        public double ESPVal
        {
            get { return _eSPVal; }
            set
            {
                this.SetValue(ref _eSPVal, value);
                if (IsroomSelected == false)
                {
                    AutoModelForRoomChange();
                    NotifyPropertiesUpdate();
                }
                if (proj.SelectedRoom != null)
                {
                    proj.SelectedRoom.StaticPressure = value;
                }             

            }
        }

        private ObservableCollection<string> _EspUnit = new ObservableCollection<string>() { "pa" };
        public ObservableCollection<string> EspUnit
        {
            get { return _EspUnit; }
            set { _EspUnit = value; }
        }

        private string _currentESPUnit;
        public string CurrentESPUnit
        {
            get { return _currentESPUnit; }
            set
            {
                this.SetValue(ref _currentESPUnit, value);
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
                if (IsroomSelected == false)
                {
                    NotifyPropertiesUpdate();
                }
                if (proj.SelectedRoom != null)
                {
                    proj.SelectedRoom.PeopleNumber = value;
                }
            }
        }

        private double _area;
        public double Area
        {
            get { return _area; }
            set
            {
                this.SetValue(ref _area, value);

                if (IsroomSelected == false)
                {
                    NotifyPropertiesUpdate();
                }

            } 
            
        }

        private ObservableCollection<string> _AreaUnit = new ObservableCollection<string>() { "m²", "ft²" };
        public ObservableCollection<string> AreaUnit
        {
            get { return _AreaUnit; }
            set { _AreaUnit = value; }
        }

        private string _currentAreaUnit;
        public string CurrentAreaUnit
        {
            get { return _currentAreaUnit; }
            set
            {
                string PrevAreaUnit = _currentAreaUnit;
                this.SetValue(ref _currentAreaUnit, value);
                if (PrevAreaUnit != null)
                {
                    if (!flgTabChanged)
                    {
                        if (MasterDesignerViewModel._isSysPropFA) { PrevAreaUnit = SystemSetting.UserSetting.unitsSetting.settingAREA; }
                        Area = ConvertAreaval(PrevAreaUnit, _currentAreaUnit,Area);
                        flgTabChanged = true;
                    }
                    //  flgTabChanged = false;
                    MasterDesignerViewModel._isSysPropFA = false;
                  
                }
                if (manualSelChecked == true)
                {
                    FanSpeedDepProp();
                }
            }
        }
        #endregion Capacity requirement Properties

        #region Selected Heat exchanger Properties 
        private string _hEImagePath;
        public string HEImagePath
        {
            get { return _hEImagePath; }
            set { this.SetValue(ref _hEImagePath, value); }
        }

        private string _SelHEModel;
        public string SelHEModel
        {
            get
            {
                return _SelHEModel;
            }
            set
            {
                this.SetValue(ref _SelHEModel, value);
            }
        }

        private string _SelHEAirflow;
        public string SelHEAirflow
        {
            get
            {
                return _SelHEAirflow;
            }
            set
            {
                this.SetValue(ref _SelHEAirflow, value);
            }
        }

        private string _SelHEESP;
        public string SelHEESP
        {
            get
            {
                return _SelHEESP;
            }
            set
            {
                this.SetValue(ref _SelHEESP, value);
            }
        }

        private string _SelTEF;
        public string SelTEF
        {
            get
            {
                return _SelTEF;
            }
            set
            {
                this.SetValue(ref _SelTEF, value);
            }
        }

        private string _SelCooling;
        public string SelCooling
        {
            get
            {
                return _SelCooling;
            }
            set
            {
                this.SetValue(ref _SelCooling, value);
            }
        }

        private string _SelHeating;
        public string SelHeating
        {
            get
            {
                return _SelHeating;
            }
            set
            {
                this.SetValue(ref _SelHeating, value);
            }
        }

        private string _tempexeff;
        public string TempExEff
        {
            get
            {
                return _tempexeff;
            }
            set
            {
                this.SetValue(ref _tempexeff, value);
            }
        }

        private string _enefcooling;
        public string EnEfCooling
        {
            get
            {
                return _enefcooling;
            }
            set
            {
                this.SetValue(ref _enefcooling, value);
            }
        }

        private string _enefheating;
        public string EnEfHeating
        {
            get
            {
                return _enefheating;
            }
            set
            {
                this.SetValue(ref _enefheating, value);
            }
        }
        #endregion Selected Heat exchanger Properties
        #region errormsg 
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
        #endregion
        #endregion properties

        private string _ErrorMsg;
        public string ErrorMsg
        {
            get { return _ErrorMsg; }
            set { this.SetValue(ref _ErrorMsg, value); }
        }

        public HeatExchangerCanvasEquipmentViewModel(IEventAggregator EventAggregator, IRegionManager _regionManager)
        {
        }
        internal void UpdateEquipProp(SystemBase newSystem)
        {
            _currentSystem = newSystem;
            proj = JCHVRF.Model.Project.CurrentProject.ExchangerList.FirstOrDefault(mm => mm.SystemID.Equals(_currentSystem.Id));
            UpdatedHEProperties(false);
        }

        public HeatExchangerCanvasEquipmentViewModel(IEventAggregator EventAggregator, IRegionManager _regionManager, IModalWindowService winService)
        {
            regionManager = _regionManager;
            _winService = winService;
            _dal = new JCHVRF.DAL.IndoorDAL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.RegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
            _bll = new JCHVRF.BLL.IndoorBLL(Project.CurrentProject.SubRegionCode, Project.CurrentProject.BrandCode);
            flgTabChanged = true;
            //manualSelChecked = false;
            AddEditRoomCommand = new DelegateCommand(OnAddEditRoomClicked);
            AddFloorCommand = new DelegateCommand(OnAddFloorClicked);
            NumericOutdoorCDBCommand = new DelegateCommand(NumericOutdoorCDB_LostFocus);
            NumericOutdoorHDBCommand = new DelegateCommand(NumericOutdoorHDBCommand_LostFocus);
            NumericOutdoorHDWBTCommand = new DelegateCommand(NumericOutdoorHDWBTCommand_LostFocus);
            NumeroutdoorHDRHCommand = new DelegateCommand(NumeroutdoorHDRHCommand_LostFocus);
            ChangeTempCommand = new DelegateCommand(btnChangeTempUClicked);
            AddAccessoryCommandHE = new DelegateCommand(OnAddAccessoryCommandClick);
            ShowGADrawingPDF = new DelegateCommand(OnGADrawingBtnClick);
            ShowFanPerformance = new DelegateCommand(onFanPerformenceClick);
            ShowInstallationGuide = new DelegateCommand(onInstallationGuideClick);
            AddSalesDataCommandHe = new DelegateCommand(OnAddSalesDataCommandClick);

            _eventAggregator = EventAggregator;
            _eventAggregator.GetEvent<SendHEDetails>().Subscribe(UpdatedHEProperties);
            _eventAggregator.GetEvent<FloorListSaveSubscriber>().Subscribe(OpenGetFloorList);
            _eventAggregator.GetEvent<RoomListSaveSubscriber>().Subscribe(OpenGetRoomList);
            _eventAggregator.GetEvent<CleanupHE>().Subscribe(OnCleanup);
            initialized = true;
            InitItems();
           
        }

        private void OnCleanup()
        {
            _eventAggregator.GetEvent<SendHEDetails>().Unsubscribe(UpdatedHEProperties);
            _eventAggregator.GetEvent<FloorListSaveSubscriber>().Unsubscribe(OpenGetFloorList);
            _eventAggregator.GetEvent<RoomListSaveSubscriber>().Unsubscribe(OpenGetRoomList);
            _eventAggregator.GetEvent<CleanupHE>().Unsubscribe(OnCleanup);
        }

        private double ConvertFreshAirval(string PrevUnitComp, string NextToConvert, double valFreshAir)
        {
            double FreshAirConversion = valFreshAir;

            if (FreshAirConversion != 0.0)
            {
                if (NextToConvert == PrevUnitComp)
                {
                    // Does nothing
                }
                else
                {
                    // Convert to m3/min
                    if (PrevUnitComp == Unit.ut_Airflow_cfm)
                    {
                        FreshAirConversion = FreshAirConversion * 0.028316847;
                    }
                    else if (PrevUnitComp == Unit.ut_Airflow_ls)
                    {
                        FreshAirConversion = FreshAirConversion * 0.06;
                    }
                    else if (PrevUnitComp == Unit.ut_Airflow_m3hr)
                    {
                        FreshAirConversion = FreshAirConversion / 60;
                    }

                    // convert from m3/min
                    if (NextToConvert == Unit.ut_Airflow_cfm)
                    {
                        FreshAirConversion = Math.Round((FreshAirConversion * 35.3146667), 4);
                    }
                    else if (NextToConvert == Unit.ut_Airflow_ls)
                    {
                        FreshAirConversion = Math.Round((FreshAirConversion * 16.6667), 4);
                    }
                    else if (NextToConvert == Unit.ut_Airflow_m3hr)
                    {
                        FreshAirConversion = Math.Round((FreshAirConversion * 60), 4);
                    }
                    else
                    {
                        FreshAirConversion = Math.Round((FreshAirConversion), 4);
                    }
                }
            }
            return FreshAirConversion;
        }

        private double ConvertAreaval(string PrevUnitComp, string NextToConvert, double valArea)
        {
            double AreaConversion = valArea;

            if (AreaConversion != 0.0)
            {
                if (PrevUnitComp == NextToConvert)
                {
                    //does nothing
                }
                else
                {
                    if (PrevUnitComp == Unit.ut_Area_m2 && NextToConvert == Unit.ut_Area_ft2)
                    {
                        //conversion from m2 to ft2
                        AreaConversion = AreaConversion * 10.7639;
                    }
                    else
                    {
                        // conversion from ft2 to m2
                        AreaConversion = AreaConversion * 0.092903;
                    }
                }
            }
            return Math.Round(AreaConversion, 2);
        }

        private void InitItems()
        {
            InitSeries();
            FanSpeeds = new ObservableCollection<FanSpeed>() { FanSpeed.High, FanSpeed.Medium, FanSpeed.Low };
        }

        private void InitSeries()
        {
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
                }

            }
        }

        private void BindPowerMode()
        {
            var powerModels = new ObservableCollection<PowerModel>();
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
                    powerModels.Add(powerModel);

                    if (powerModel.SelectedValues.Equals(proj.Power))
                    {
                        SelectedPower = powerModel;
                    }
             
                }
                Power = powerModels;
                if (SelectedPower == null)
                {
                    SelectedPower = Power[0];
                }
            }
            if (proj.IndoorItem != null)
            {
                //HEImagePath = "";
                proj.IndoorItem.TypeImage = "";
            }
        }

        private DataTable dt_airflow;

        private void BindModel()
        {
            ListModel = new ObservableCollection<string>();
            if (SelectedPower != null)
            {
                string _series = SelectedSeries.SelectedValues;
                DataTable dt = _bll.GetExchnagerListStd(_series, "", SelectedPower.Code);
                dt_airflow = dt;
                dt.DefaultView.Sort = "AirFlow";
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dtModelTypeRow in dt.Rows)
                    {
                        if (Project.CurrentProject.BrandCode == "H")
                        {
                            ListModel.Add(dtModelTypeRow.ItemArray[3].ToString());
                        }
                        else
                        {
                            ListModel.Add(dtModelTypeRow.ItemArray[2].ToString());
                        }
                    }
                    if (manualSelChecked == false)
                    {
                        DataRow dr = GetAutoModel(dt.DefaultView.ToTable().Rows);
                        SelectedModel = Project.CurrentProject.BrandCode == "H" ? dr["Model_Hitachi"].ToString() : dr["Model_York"].ToString();
                        modelfull = dr["ModelFull"].ToString();
                        if (Project.CurrentProject.BrandCode == "Y")
                        {
                            proj.IndoorItem.Model_York = SelectedModel;
                        }
                    }
                    else
                    {
                        //To select default model name when manual selection is checked
                        if (SelectedModel == null && ListModel != null && ListModel.Count > 0)
                        {
                            SelectedModel = ListModel[0];
                        }
                        if (lbloutdoorHeatingRH == string.Empty && lbloutdoorHeatingWB == string.Empty &&
                                lbloutdoorHeatingDB == string.Empty && lbloutdoorCoolingDB == string.Empty)
                        {
                            _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                        }
                    }

                }
            }

        }

        //Function to get the full name of model in mannual selection
        private void GetModelFull()
        {
            DataRow dr = GetSelectedModDetails();
            if(dr != null)
            {
                modelfull = (dr["ModelFull"] is DBNull) ? null : (dr["ModelFull"].ToString());
            }
        }

        private bool autoSelError = false;
        public DataRow GetAutoModel(DataRowCollection rows)
        {
            int sel_row = 0;
            double modelAirFlowInfo = 0;
            string tablename = _bll.GetStdTableName();

            int modelESPInfo = 0;

            Double FreshAirInfo = new double();

            FreshAirInfo = FreshAir;

            if (CurrentFreshAirUnit == Unit.ut_Airflow_cfm)
            {
                FreshAirInfo = FreshAirInfo * 0.028316847;
            }
            else if (CurrentFreshAirUnit == Unit.ut_Airflow_ls)
            {
                DCErrorMessage = string.Empty;
                FreshAirInfo = FreshAirInfo * 0.06;
            }
            else if (CurrentFreshAirUnit == Unit.ut_Airflow_m3hr)
            {
                FreshAirInfo = FreshAirInfo / 60;
            }

            if (FreshAirInfo == 0 && ESPVal == 0)
            {
                if (lbloutdoorHeatingRH == string.Empty && lbloutdoorHeatingWB == string.Empty &&
                      lbloutdoorHeatingDB == string.Empty && lbloutdoorCoolingDB == string.Empty)
                {
                    _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                }
                DCErrorMessage = string.Empty;
                return rows[sel_row];
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    // to read the values from table JCHVRF_IDU_AirFlow
                    DataRow frmairflowtab = _bll.GetAirflowEsp(rows[i]["Model_Hitachi"].ToString(), SelectedPower.SelectedValues);

                    if (frmairflowtab != null)
                    {
                        modelAirFlowInfo = Double.Parse(frmairflowtab["AirFlow_Hi"].ToString());
                        modelESPInfo = Convert.ToInt16(frmairflowtab["StaticPressure_Hi"]);
                    }

                    //for float comparision
                    FreshAirInfo = Math.Round(FreshAirInfo, 3);
                    modelAirFlowInfo = Math.Round(modelAirFlowInfo, 3);
                    if ((FreshAirInfo <= modelAirFlowInfo && ESPVal == 0) || (ESPVal <= modelESPInfo && FreshAirInfo == 0)
                                       || (FreshAirInfo <= modelAirFlowInfo && ESPVal <= modelESPInfo))
                    {
                        sel_row = i;
                        break;
                    }
                    sel_row = i;
                }

                _currentSystem.SystemStatus = Model.SystemStatus.INVALID;
                if (FreshAirInfo > modelAirFlowInfo && ESPVal > modelESPInfo)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_FRESH_AIR_ESP"), MessageType.Warning, MessageBoxButton.OK)));
                    autoSelError = true;
                }
                else if (FreshAirInfo > modelAirFlowInfo)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_FRESH_AIR"), MessageType.Warning, MessageBoxButton.OK)));
                    autoSelError = true;
                }
                else if (ESPVal > modelESPInfo)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_FRESH_ESP"), MessageType.Warning, MessageBoxButton.OK)));
                    autoSelError = true;
                }
                else
                {
                    if (lbloutdoorHeatingRH == string.Empty && lbloutdoorHeatingWB == string.Empty &&
                        lbloutdoorHeatingDB == string.Empty && lbloutdoorCoolingDB == string.Empty)
                    {
                        _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                        autoSelError = false;
                    }
                    DCErrorMessage = string.Empty;
                }
            }
            if (proj.IndoorItem != null)
            {
                proj.IndoorItem.Model_Hitachi = rows[sel_row]["Model_Hitachi"].ToString();
            }
            return rows[sel_row];
        }

        private DataRow GetSelectedModDetails()
        {
            int row_sel = 0;
            DataTable dt = _bll.GetExchnagerListStd(SelectedSeries.SelectedValues, "", SelectedPower.Code);
            dt_airflow = dt;
            if (dt_airflow != null)
            {
                for (int i = 0; i < dt_airflow.Rows.Count; i++)
                {
                    string tocomparemodel = Project.CurrentProject.BrandCode == "H" ? dt_airflow.Rows[i]["Model_Hitachi"].ToString() : dt_airflow.Rows[i]["Model_York"].ToString();
                    if (SelectedModel == tocomparemodel)
                    {
                        row_sel = i;
                        break;
                    }
                }
                return dt_airflow.Rows[row_sel];
            }
            return null;
        }

        /*Function added to get Hitachi name of selected YORK model 
         * since table JCHVRF_IDU_AirFlow contains only Hitachi model names */
        private string GetCorrsHitachiname(string modelnameyork)
        {
            string modelHitachi = "";
            if (modelnameyork != null || modelnameyork != "-")
            {
                switch (modelnameyork)
                {
                    case "JDTH500H0NEGQ":
                    case "JDTH500H0NOEGQ":
                        modelHitachi = "KPF-500H-E-GQ";
                        break;
                    case "JDTH020H0NAGQ":
                    case "JDTH020H0NOAGQ":
                        modelHitachi = "KPI-20H-A-GQ";
                        break;
                    case "JDTH030H0NAGQ":
                    case "JDTH030H0NOAGQ":
                        modelHitachi = "KPI-30H-A-GQ";
                        break;
                    case "JDTH040H0NAGQ":
                    case "JDTH040H0NOAGQ":
                        modelHitachi = "KPI-40H-A-GQ";
                        break;
                    case "JDTH050H0NAGQ":
                    case "JDTH050H0NOAGQ":
                        modelHitachi = "KPI-50H-A-GQ";
                        break;
                    case "JDTH065H0NAGQ":
                    case "JDTH065H0NOAGQ":
                        modelHitachi = "KPI-65H-A-GQ";
                        break;
                    case "JDTH080H0NAGQ":
                    case "JDTH080H0NOAGQ":
                        modelHitachi = "KPI-80H-A-GQ";
                        break;
                    case "JDTH100H0NAGQ":
                    case "JDTH100H0NOAGQ":
                        modelHitachi = "KPI-100H-A-GQ";
                        break;
                    case "JDTH125H0NAGQ":
                    case "JDTH125H0NOAGQ":
                        modelHitachi = "KPI-125H-A-GQ";
                        break;
                    case "JDTH150H0NEGQ":
                    case "JDTH150H0NOEGQ":
                        modelHitachi = "KPI-150H-E-GQ";
                        break;
                    case "JDTH200H0NEGQ":
                    case "JDTH200H0NOEGQ":
                        modelHitachi = "KPI-200H-E-GQ";
                        break;
                    case "JDTH250H0NEGQ":
                    case "JDTH250H0NOEGQ":
                        modelHitachi = "KPI-250H-E-GQ";
                        break;
                    case "JDTH300H0NEGQ":
                    case "JDTH300H0NOEGQ":
                        modelHitachi = "KPI-300H-E-GQ";
                        break;
                    case "JDTH400H0NEGQ":
                    case "JDTH400H0NOEGQ":
                        modelHitachi = "KPF-400H-E-GQ";
                        break;
                    default:
                        break;
                }
            }
            return modelHitachi;
        }

        // Flag used to avoid multiple call of function ReflectChanges
        private bool ChangedSelectedModelFlag = false;
        private void SelectionChangedSelectedModelEvent()
        {
            if (this.SelectedModel != null)
            {
                string modelName = "";
                if (Project.CurrentProject.BrandCode == "Y")
                {
                    modelName = GetCorrsHitachiname(SelectedModel);
                }
                else
                {
                    modelName = SelectedModel;
                }

                /* binding fan speed options based on selected model values */
                DataRow modelVal;
                modelVal = _bll.GetAirflowEsp(modelName, SelectedPower.SelectedValues);

                if (modelVal != null)
                {
                    if (modelVal["AirFlow_Hi"].ToString() == string.Empty /* || 
                                         modelVal["StaticPressure_Hi"].ToString() == string.Empty */)
                    {
                        FanSpeeds.Remove(FanSpeed.High);
                    }
                    else
                    {
                        int index = FanSpeeds.IndexOf(FanSpeed.High);
                        if (index == -1)
                        {
                            FanSpeeds.Add(FanSpeed.High);
                        }
                    }

                    if (modelVal["AirFlow_Med"].ToString() == string.Empty /* || 
                                        modelVal["StaticPressure_Med"].ToString() == string.Empty */)
                    {
                        FanSpeeds.Remove(FanSpeed.Medium);
                    }
                    else
                    {
                        int index = FanSpeeds.IndexOf(FanSpeed.Medium);
                        if (index == -1)
                        {
                            FanSpeeds.Add(FanSpeed.Medium);
                        }
                    }

                    if (modelVal["AirFlow_Lo"].ToString() == string.Empty /*|| 
                                        modelVal["StaticPressure_Lo"].ToString() == string.Empty */)
                    {
                        FanSpeeds.Remove(FanSpeed.Low);
                    }
                    else
                    {
                        int index = FanSpeeds.IndexOf(FanSpeed.Low);
                        if (index == -1)
                        {
                            FanSpeeds.Add(FanSpeed.Low);
                        }
                    }
                }

                if (FanSpeeds.Count > 0 && FanSpeeds.IndexOf(SFanSpeed) == -1)
                {
                    ChangedSelectedModelFlag = true;
                    SFanSpeed = FanSpeeds[0];
                    ChangedSelectedModelFlag = false;
                }

                BindIndoorImageToUI();
                SelHEModel = SelectedModel;
                FanSpeedDepProp();
            
            }
        }

        private void FanSpeedDepProp()
        {
            if (SelectedModel != null)
            {
                string modelName = "";

                if (Project.CurrentProject.BrandCode == "Y")
                {
                    modelName = GetCorrsHitachiname(SelectedModel);
                }
                else
                {
                    modelName = SelectedModel;
                }

                DataRow modelVal;
                modelVal = _bll.GetAirflowEsp(modelName, SelectedPower.SelectedValues);

                if (obj != null && modelVal != null)
                {
                    switch ((int)SFanSpeed)
                    {
                        case 3:
                            SelTEF = obj.TempExchangeEff_Low;
                            SelCooling = obj.EnthalpyExEffCool_low;
                            SelHeating = obj.EnthalpyExchangeEffHeat_low;
                            modelFreshAir = double.Parse(modelVal["AirFlow_Lo"].ToString());
                            modelEsp = Convert.ToInt16(modelVal["StaticPressure_Lo"].ToString());
                            break;
                        case 2:
                            SelTEF = obj.TempExchangeEff_medium;
                            SelCooling = obj.EnthalpyExEffCool_medium;
                            SelHeating = obj.EnthalpyExchangeEffHeat_medium;
                            modelFreshAir = double.Parse(modelVal["AirFlow_Med"].ToString());
                            modelEsp = Convert.ToInt16(modelVal["StaticPressure_Med"].ToString());
                            break;
                        default:
                            SelTEF = obj.TempExchangeEff_high;
                            SelCooling = obj.EnthalpyExEffCool_high;
                            SelHeating = obj.EnthalpyExchangeEffHeat_high;
                            modelFreshAir = double.Parse(modelVal["AirFlow_Hi"].ToString());
                            modelEsp = Convert.ToInt16(modelVal["StaticPressure_Hi"].ToString());
                            break;
                    }
                }
                if (CurrentFreshAirUnit == Unit.ut_Airflow_cfm)
                {
                    modelFreshAir = Math.Round((modelFreshAir * 35.3146667));
                }
                else if (CurrentFreshAirUnit == Unit.ut_Airflow_ls)
                {
                    modelFreshAir = Math.Round((modelFreshAir * 16.6667));
                }
                else if (CurrentFreshAirUnit == Unit.ut_Airflow_m3hr)
                {
                    modelFreshAir = Math.Round((modelFreshAir * 60));
                }
                else
                {
                    modelFreshAir = Math.Round((modelFreshAir));
                }

                SelHEAirflow = modelFreshAir.ToString() + "  " + CurrentFreshAirUnit;
                SelHEESP = modelEsp.ToString() + "  " + "Pa";
                if (SelTEF != "-")
                {
                    SelTEF += " %";
                }

                if (SelCooling != "-")
                {
                    SelCooling += " %";
                }

                if (SelHeating != "-")
                {
                    SelHeating += " %";
                }

            }
            else
            {
                SelHEModel = string.Empty;
                SelHEAirflow = string.Empty;
                SelHEESP = string.Empty;
                SelTEF = string.Empty;
                SelCooling = string.Empty;
                SelHeating = string.Empty;
            }
        }

        private void BindIndoorImageToUI()
        {
            if (SelectedPower != null)
            {
                var sourceDir = Path.Combine(defaultFolder, navigateToFolderWithLegacyImage);
                _bll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                DataTable dt = _bll.GetExchnagerListStd(SelectedSeries.SelectedValues, "", SelectedPower.Code);

                if (SelectedModel != null)
                {
                    DataRow dr;
                    if (Project.CurrentProject.BrandCode == "H")
                    {
                        dr = dt.AsEnumerable().FirstOrDefault(r => r.Field<string>("Model_Hitachi") == _selectedModel);
                    }
                    else
                    {
                        dr = dt.AsEnumerable().FirstOrDefault(r => r.Field<string>("Model_York") == _selectedModel);
                    }
                    if (dr != null)
                    {
                        HEImagePath = this.HEImagePath = sourceDir + "\\" + Convert.ToString(dr["TypeImage"]);
                        System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                        try
                        {
                            img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(HEImagePath));
                        }
                        catch
                        {
                            if (!IsPropOrNewExch)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() => JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_IMAGE_NOT_FOUND"), MessageType.Warning, MessageBoxButton.OK)));
                                IsPropOrNewExch = true;

                            }

                        }
                        if (proj.IndoorItem != null)
                        {
                            proj.IndoorItem.TypeImage = dr["TypeImage"].ToString();
                            proj.DisplayImagePath = dr["TypeImage"].ToString();
                        }
                    }
                }
                else
                {
                    HEImagePath = "";
                    if (proj.IndoorItem != null)
                    {
                        proj.IndoorItem.TypeImage = "";
                    }
                }
            }
            else
            {
                HEImagePath = "";
                if (proj.IndoorItem != null)
                {
                    proj.IndoorItem.TypeImage = "";
                }
            }
        }

        private bool TempUnitChange = false;
        void btnChangeTempUClicked()
        {
            TempUnitChange = true;
            // CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = proj.HECanvTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c;

            if (CurrentTempUnit == Unit.ut_Temperature_f)
            {
                outdoorCoolingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(outdoorCoolingDB), UnitType.TEMPERATURE, CurrentTempUnit));
                outdoorHeatingWB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(outdoorHeatingWB), UnitType.TEMPERATURE, CurrentTempUnit));
                outdoorHeatingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(outdoorHeatingDB), UnitType.TEMPERATURE, CurrentTempUnit));

                // TemperatureTypeOCDB = Celsius;
            }
            else
            {
                outdoorCoolingDB = Convert.ToDouble(Unit.ConvertToSource(Convert.ToDouble(outdoorCoolingDB), UnitType.TEMPERATURE, Unit.ut_Temperature_f));
                outdoorHeatingWB = Convert.ToDouble(Unit.ConvertToSource(Convert.ToDouble(outdoorHeatingWB), UnitType.TEMPERATURE, Unit.ut_Temperature_f));
                outdoorHeatingDB = Convert.ToDouble(Unit.ConvertToSource(Convert.ToDouble(outdoorHeatingDB), UnitType.TEMPERATURE, Unit.ut_Temperature_f));
                //  TemperatureTypeOCDB = Fahrenheit;
            }

            ValidateOnTempChange();
            TempUnitChange = false;
        }

        private void ValidateOnTempChange()
        {
            ValidateHDDBT();
            ValidateOdb();
            ValidateHeatWetBulb();
            ValidateOutdoorHDRH();
        }

        private void NumericOutdoorCDB_LostFocus()
        {
            ValidateOdb();
        }

        //Outdoor Cooling Dry Bulb
        private bool ValidateOdb()
        {
            double nOdb = Convert.ToDouble(outdoorCoolingDB); //Convert.ToDouble(outdoorCoolingDB);

            if ((nOdb >= Unit.ConvertToControl(10.0, UnitType.TEMPERATURE, CurrentTempUnit)) && (nOdb <= Unit.ConvertToControl(43.0, UnitType.TEMPERATURE, CurrentTempUnit)))
            {
                lbloutdoorCoolingDB = string.Empty;
                if (lbloutdoorHeatingRH == string.Empty && lbloutdoorHeatingWB == string.Empty &&
                            lbloutdoorHeatingDB == string.Empty && autoSelError == false)
                {
                    _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                }
                return true;

            }
            else
            {
                lbloutdoorCoolingDB = string.Format("Range[{0},{1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(43, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[16,30]";
                _currentSystem.SystemStatus = Model.SystemStatus.INVALID;

                return false;
            }

        }

        private void NumericOutdoorHDBCommand_LostFocus()
        {
            ValidateHDDBT();
        }

        //Outdoor Heating Dry Bulb
        private bool ValidateHDDBT()
        {
            double nOHDDBT = Convert.ToDouble(outdoorHeatingDB);

            if ((nOHDDBT >= Unit.ConvertToControl(-18.0, UnitType.TEMPERATURE, CurrentTempUnit)) && (nOHDDBT <= Unit.ConvertToControl(33.0, UnitType.TEMPERATURE, CurrentTempUnit)))
            {
                lbloutdoorHeatingDB = string.Empty;
                if (lbloutdoorHeatingRH == string.Empty && lbloutdoorHeatingWB == string.Empty &&
                      lbloutdoorCoolingDB == string.Empty && autoSelError == false)
                {
                    _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                }
                return true;
            }
            else
            {
                lbloutdoorHeatingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-18, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(33, UnitType.TEMPERATURE, CurrentTempUnit));
                _currentSystem.SystemStatus = Model.SystemStatus.INVALID;
                return false;
            }
        }

        private void NumericOutdoorHDWBTCommand_LostFocus()
        {
            ValidateHeatWetBulb();
        }

        //Outdoor Cooling Wet Bulb
        private bool ValidateHeatWetBulb()
        {
            double nCWBVal = Convert.ToDouble(outdoorHeatingWB); //Convert.ToDouble(indoorCoolingWB);

            if ((nCWBVal >= Unit.ConvertToControl(-20.0, UnitType.TEMPERATURE, CurrentTempUnit)) && (nCWBVal <= Unit.ConvertToControl(15.0, UnitType.TEMPERATURE, CurrentTempUnit)))
            {
                lbloutdoorHeatingWB = string.Empty;
                if (lbloutdoorHeatingRH == string.Empty && lbloutdoorHeatingDB == string.Empty
                    && lbloutdoorCoolingDB == string.Empty && autoSelError == false)
                {
                    _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                }
                return true;

            }
            else
            {
                lbloutdoorHeatingWB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-20, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(15, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[-20, 15]";
                _currentSystem.SystemStatus = Model.SystemStatus.INVALID;
                return false;
            }

        }

        private void NumeroutdoorHDRHCommand_LostFocus()
        {
            ValidateOutdoorHDRH();
        }

        //Outdoor RH
        private bool ValidateOutdoorHDRH()
        {
            double nOdb = Convert.ToDouble(outdoorHeatingRH);
            if ((nOdb >= 13.0) && (nOdb <= 100.0))
            {
                lbloutdoorHeatingRH = string.Empty;
                if (lbloutdoorHeatingWB == string.Empty && lbloutdoorHeatingDB == string.Empty
                           && lbloutdoorCoolingDB == string.Empty && autoSelError == false)
                {
                    _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                }
                return true;
            }
            else
            {
                lbloutdoorHeatingRH = string.Format("Range[{0}, {1}]", 13, 100);//"Range[13, 100]";
                _currentSystem.SystemStatus = Model.SystemStatus.INVALID;
                return false;
            }
        }

        private void OnAddEditRoomClicked()
        {
            try
            {
                NavigationParameters param = new NavigationParameters();
                param.Add("ShowSaveCancel", true);
                _winService.ShowView(ViewKeys.AddEditRoom, language.Current.GetMessage("ADDEDITROOMS"), param, true, 850, 550);
                //GetRoomList();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private void OnAddFloorClicked()
        {
            try
            {
                NavigationParameters param = new NavigationParameters();
                param.Add("EnableSaveButtons", true);
                _winService.ShowView(ViewKeys.FloorTab, language.Current.GetMessage("ADD_OR_EDIT_FLOOR"), param);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void UpdateUnitName()
        {
            if (SelectedRoom != null)
            {
                if (!string.IsNullOrWhiteSpace(SelectedRoom.Name))
                {
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
                if (proj != null)
                {
                    this.UnitName = proj.IndoorName;
                }
                this.IsHENameEditable = false;
                this.UnitNameColor = new SolidColorBrush(Colors.Black);
            }
        }

        private void AutoModelForRoomChange()
        {
            if (SelectedSeries != null)
            {
                if (manualSelChecked == false && initialized)
                {
                    BindModel();
                }
            }
        }

        public void MouseClickCommandClick()
        {
            RoomIndoor roomHeatExchanger = JCHVRF.Model.Project.CurrentProject.ExchangerList.FirstOrDefault(mm => mm.SystemID.Equals(_currentSystem.Id));
        }

        private void DeleteEqu()
        {
            RoomIndoor roomIndoor = Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(_currentSystem.Id));
            Project.CurrentProject.ExchangerList.Remove(roomIndoor);
        }

        private SystemBase _currentSystem;
        private IModalWindowService _winService;

        private void GetRoomList()
        {
            try
            {
                if (JCHVRF.Model.Project.GetProjectInstance.RoomList.Count > 0)
                {
                    //CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
                    _project = JCHVRF.Model.Project.GetProjectInstance;
                    RoomName = new ObservableCollection<Room>(JCHVRF.Model.Project.GetProjectInstance.RoomList);
                }

                foreach (var room in Project.GetProjectInstance.RoomList)
                {
                    if (proj != null)
                    {
                        if (room.Id.Equals(proj.RoomID))
                        {
                            SelectedRoom = room;
                            break;
                        }
                    }
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

        //To bind properties based on selected room 
        private void SetEditedPropertyValue()
        {
            if(proj.HECanvFreshAirUnit == SystemSetting.UserSetting.unitsSetting.settingAIRFLOW)
            {
                FreshAir = SelectedRoom.FreshAir;
            }
            else
            {
                FreshAir = ConvertFreshAirval(SystemSetting.UserSetting.unitsSetting.settingAIRFLOW,
                                              proj.HECanvFreshAirUnit, SelectedRoom.FreshAir);
            }

            if (proj.HECanvAreaUnit == SystemSetting.UserSetting.unitsSetting.settingAREA)
            {
                Area = SelectedRoom.Area;
            }
            else
            {
                Area = ConvertAreaval(SystemSetting.UserSetting.unitsSetting.settingAREA, proj.HECanvAreaUnit, 
                                    SelectedRoom.Area);
            }

            ESPVal = SelectedRoom.StaticPressure;
            NoOfPeople = SelectedRoom.PeopleNumber;
        }

        private void BindFloor()
        {
            try
            {
                if (proj != null && proj.SelectedFloor != null)
                {
                    if (Project.CurrentProject != null && Project.CurrentProject.FloorList != null)
                    {
                        FloorList = new ObservableCollection<Floor>(Project.CurrentProject.FloorList);
                    }
                    if (FloorList != null && FloorList.Count > 0 && Project.CurrentProject.HeatExchangerSystems.Count > 0)
                    {
                        SelectedFloor = FloorList.FirstOrDefault(mm => mm.Id.Equals(proj.SelectedFloor.Id));
                        if (SelectedFloor == null)
                        {
                            SelectedFloor = FloorList[0];
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

        private void OpenGetFloorList()
        {
            BindFloor();
        }

        private void OpenGetRoomList()
        {
            GetRoomList();
        }

        public void UpdatedHEProperties(bool fromPropertiesContainer)
        {
             flgTabChanged = false;
            if (this.proj != null)
            {
                if (this.proj.IndoorItem != null)
                {
                    RefreshItems();
                }
                else
                {
                    ResetProperties();
                }
                ValidateHeatEx();
            }
        }

        private void RefreshItems()
        {
            initialized = false;
            RefreshProperties();
            GetRoomList();
            BindFloor();
            GetHeatExchangerProperties();
            ValidateOnTempChange();
            initialized = true;
        }

        private void ResetProperties()
        {
            flgTabChanged = true;
            initialized = false;
            SelectedPower = null;
            GetRoomList();
            if (proj.RoomID == null)
            {
                SelectedRoom = null;
            }

            BindFloor();
            UnitName = this.proj.IndoorName;
            outdoorCoolingDB = proj.DBCooling;
            outdoorHeatingWB = proj.WBCooling;
            outdoorHeatingDB = proj.DBHeating;
            outdoorHeatingRH = proj.RHCooling;
            CurrentFreshAirUnit = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            CurrentESPUnit = SystemSetting.UserSetting.unitsSetting.settingESP;
            CurrentAreaUnit = SystemSetting.UserSetting.unitsSetting.settingAREA;
            manualSelChecked = proj.IsAuto;
            Area = proj.Area;
            NoOfPeople = proj.NumberOfPeople;
            ESPVal = proj.RqStaticPressure;
            FreshAir = proj.RqFreshAir;
            SFanSpeed = 0;
            SelectedFloor = proj.SelectedFloor;
            SelectedModel = null;
            HEImagePath = null;
            proj.IndoorItem = null;
            SelectedSeries = null;
            initialized = true;
            NotifyPropertiesUpdate();
        }

        public void NotifyPropertiesUpdate()
        {
            if (initialized)
            {
                if (SelectedSeries != null)
                {
                    ReflectChanges();
                    UtilTrace.SaveHistoryTraces();
                }
            }
        }

        private void ReflectChanges()
        {
            ProjectInfoBLL bll = new ProjectInfoBLL();

            JCHVRF.Entity.ProjectInfo projectInfo = bll.GetProjectInfo(Project.CurrentProject.projectID);
            int index = Project.CurrentProject.ExchangerList.FindIndex(mm => mm.SystemID.Equals(proj.SystemID));
            Project.CurrentProject.ExchangerList.RemoveAt(index);

            string newRoomName = "";
            string newRoomId = "";

            if (SelectedRoom != null)
            {
                newRoomName = SelectedRoom.Name;
                newRoomId = SelectedRoom.Id;
                proj.SelectedRoom = SelectedRoom;
            }
            else
            {
                newRoomName = "";
            }

            if (SelectedSeries != null)
            {
                if (!string.IsNullOrEmpty(SelectedSeries.SelectedValues))
                {
                    if (proj.IndoorItem != null)
                    {
                        proj.IndoorItem.ModelFull = modelfull;
                        proj.IndoorItem.Series = SelectedSeries.SelectedValues;
                        if (Project.CurrentProject.BrandCode == "H")
                        {
                            proj.IndoorItem.Model_Hitachi = SelectedModel;
                        }
                        else
                        {
                            proj.IndoorItem.Model_York = SelectedModel;
                        }
                        proj.IndoorItem.Type = SelectedSeries.SelectedValues;
                        proj.IndoorItem.ProductType = SelectedSeries.SelectedValues;
                        proj.IndoorItem.DisplayName = SelectedSeries.SelectedValues;
                    }
                    else
                    {
                        if (SelectedPower != null)
                        {
                            DataTable dt = _bll.GetExchnagerListStd(SelectedSeries.SelectedValues, "", SelectedPower.Code);
                            dt.DefaultView.Sort = "AirFlow";

                            if (dt.Rows.Count > 0)
                            {
                                DataRow dr = GetAutoModel(dt.DefaultView.ToTable().Rows);
                                string ProductType = string.Empty;
                                if (thisProject.RegionCode== "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
                                { ProductType = "Universal IDU"; }
                                else { ProductType = SelectedSeries.SelectedValues; }
                                inItem = _bll.GetItem(dr["ModelFull"].ToString(), SelectedSeries.SelectedValues, ProductType, SelectedSeries.SelectedValues);
                                if (inItem != null)
                                {
                                    inItem.Series = SelectedSeries.SelectedValues;
                                    //Required field for Report
                                    inItem.DisplayName = SelectedSeries.SelectedValues;
                                    if (Project.CurrentProject.BrandCode == "H")
                                    {
                                        inItem.Model_Hitachi = SelectedModel;
                                    }
                                    else
                                    {
                                        inItem.Model_York = SelectedModel;
                                    }
                                    inItem.Type = SelectedSeries.SelectedValues;
                                    inItem.ProductType = SelectedSeries.SelectedValues;

                                    proj.IndoorItem = inItem;

                                }

                            }
                        }
                    }
                }
            }
            proj.RoomName = newRoomName;
            proj.RoomID = newRoomId;
            proj.Power = SelectedPower != null ? SelectedPower.SelectedValues : "";
            proj.FanSpeedLevel = (int)SFanSpeed;
            proj.IndoorName = UnitName;
            proj.SelectedFloor = SelectedFloor;
            proj.DBCooling = outdoorCoolingDB;
            proj.WBCooling = outdoorHeatingWB;
            proj.DBHeating = outdoorHeatingDB;
            proj.RHCooling = outdoorHeatingRH;
            proj.DisplayImagePath = HEImagePath;
            proj.IsAuto = manualSelChecked;

            proj.HECanvAreaUnit = CurrentAreaUnit;
            proj.HECanvFreshAirUnit = CurrentFreshAirUnit;

            //Calling this function to conver the entered value to units choosen in measuring units tab.
            proj.RqFreshAir = ConvertFreshAirval(CurrentFreshAirUnit, SystemSetting.UserSetting.unitsSetting.settingAIRFLOW, FreshAir);
            proj.Area = ConvertAreaval(CurrentAreaUnit, SystemSetting.UserSetting.unitsSetting.settingAREA, Area);
            proj.RqStaticPressure = ESPVal;
            proj.NumberOfPeople = NoOfPeople;
            proj.HECanvTempUnit = CurrentTempUnit;

            if (SelectedRoom != null)
            {
                proj.SelectedRoom.FreshAir = proj.RqFreshAir;
                proj.SelectedRoom.Area = proj.Area;
                proj.SelectedRoom.StaticPressure = ESPVal;
                proj.SelectedRoom.PeopleNumber = NoOfPeople;
            }

            Project.CurrentProject.ExchangerList.Insert(index, proj);
            ProjectInfoBLL objProjectinfoBLL = new ProjectInfoBLL();
            bool status = objProjectinfoBLL.UpdateProject(Project.CurrentProject);
        }   

        private void GetHeatExchangerProperties()
        {
            if (proj != null)
            {
                /* outdoorCoolingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(proj.DBCooling), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
                 outdoorHeatingWB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(proj.WBCooling), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
                 outdoorHeatingRH = Convert.ToDouble((proj.RHCooling));
                 outdoorHeatingDB = Convert.ToDouble(Unit.ConvertToControl(Convert.ToDouble(proj.DBHeating), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
                 CurrentTempUnit = proj.HECanvTempUnit;
                 btnChangeTempUClicked(); */

                manualSelChecked = proj.IsAuto;
                GetCanvasCapReqValue();

                if (proj.IndoorItem != null)
                {
                    foreach (var series in ListSeries)
                    {
                        SeriesModel seriesModel = (SeriesModel)series;
                        if (proj.IndoorItem.Series.Equals(seriesModel.SelectedValues))
                        {
                            SelectedSeries = seriesModel;
                        }
                    }
                    if(Project.CurrentProject.BrandCode == "H")
                    {
                        SelectedModel = proj.IndoorItem.Model_Hitachi;
                    }
                    else
                    {
                        if(proj.IndoorItem.Model_York != "-")
                            SelectedModel = proj.IndoorItem.Model_York;
                    }
                }

                outdoorCoolingDB = proj.DBCooling;
                outdoorHeatingWB = proj.WBCooling;
                outdoorHeatingRH = proj.RHCooling;
                outdoorHeatingDB = proj.DBHeating;
                CurrentTempUnit = proj.HECanvTempUnit;

                SFanSpeed = (FanSpeed)proj.FanSpeedLevel;
            }
        }

        private void GetCanvasCapReqValue()
        {
            CurrentFreshAirUnit = proj.HECanvFreshAirUnit;
            CurrentAreaUnit = proj.HECanvAreaUnit;

            FreshAir = ConvertFreshAirval(SystemSetting.UserSetting.unitsSetting.settingAIRFLOW,
                  proj.HECanvFreshAirUnit, proj.RqFreshAir);
            Area = ConvertAreaval(SystemSetting.UserSetting.unitsSetting.settingAREA, proj.HECanvAreaUnit,
                    proj.Area);

            ESPVal = proj.RqStaticPressure;
            CurrentESPUnit = SystemSetting.UserSetting.unitsSetting.settingESP;
            NoOfPeople = proj.NumberOfPeople;
        }

        private void RefreshProperties()
        {
            SelectedSeries = null;
            SelectedPower = null;
            SelectedRoom = null;
            lbloutdoorHeatingRH = string.Empty;
            lbloutdoorHeatingWB = string.Empty;
            lbloutdoorHeatingDB = string.Empty;
            lbloutdoorCoolingDB = string.Empty;
            //RoomName = null;
        }

        public void OnAddAccessoryCommandClick()
        {
            if (SelectedModel != null)
            {
                ProjectInfoBLL bll = new ProjectInfoBLL();

                AddHEAccessoriesTemplate addAccessories = new AddHEAccessoriesTemplate();
                AddHEAccessoriesTemplateViewModel addAccessoriesVM = new AddHEAccessoriesTemplateViewModel(bll) { objIndoor = proj };
                addAccessories.DataContext = addAccessoriesVM;

                Window window = new Window
                {
                    Title = "Accessories",
                    Content = addAccessories
                };
                window.ShowDialog();
            }
            else
            {
                JCHMessageBox.Show("Please select the model and then click on Accessory");
            }
        }

        //button function
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.NORMAL, 8, Font.BOLD, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5,
                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }

        static int Color_Counter, Column_Count;
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.NORMAL, 8, Font.BOLD, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5,
                BackgroundColor = iTextSharp.text.BaseColor.WHITE
            });
        }
        //SalesData-ButtonClick function
        string Fan_Speed_Value;
        public void OnAddSalesDataCommandClick()
        {
            if ((int)SFanSpeed == 1)
            {
                Fan_Speed_Value = "Hi";
            }
            else if ((int)SFanSpeed == 2)
            {
                Fan_Speed_Value = "Med";
            }
            else
            {
                Fan_Speed_Value = "Lo";
            }

            Document doc = new Document(iTextSharp.text.PageSize.LETTER, 10, 10, 42, 35);
            PdfPTable tableLayout = new PdfPTable(4);
            try
            {
                PdfWriter wri = PdfWriter.GetInstance(doc, new FileStream("SalesData.pdf", FileMode.Create));
                if (SelectedModel != null)
                {
                    doc.Open();
                    doc.Add(Add_Content_To_PDF(tableLayout));
                    doc.Close();
                    Process.Start("SalesData.pdf");
                }
                else
                {
                    JCHMessageBox.Show("Please select the model for getting Sales Data");
                }
            }
            catch (Exception)
            {
                JCHMessageBox.Show("SalesData pdf is already open.\nPlease close the pdf and try again.");
            }
        }
        private PdfPTable Add_Content_To_PDF(PdfPTable tableLayout)
        {
            string title;
            Color_Counter = 0;
            float[] headers = {
                        50,
                        10,
                        10,
                        30
                    }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 80; //Set the PDF File witdh percentage  
                                              //Add Title to the PDF file at the top  
            tableLayout.AddCell(new PdfPCell(new Phrase("Sales Data", new Font(Font.NORMAL, 8, 16, new iTextSharp.text.BaseColor(0, 0, 0))))
            {
                Colspan = 4,
                Border = 0,
                PaddingBottom = 20,
                HorizontalAlignment = Element.ALIGN_LEFT
            });
            if (SelectedSeries.SelectedValues.Contains("HAPQ"))
            {
                title = "KPI(HAPQ)";
            }
            else
            {
                title = "KPI-1,KPI-E4E";
            }
            tableLayout.AddCell(new PdfPCell(new Phrase(title, new Font(Font.NORMAL, 8, Font.BOLD, new iTextSharp.text.BaseColor(0, 0, 0))))
            {
                Colspan = 4,
                Border = 0,
                PaddingBottom = 10,
                HorizontalAlignment = Element.ALIGN_LEFT,
                BackgroundColor = new iTextSharp.text.BaseColor(119, 136, 153)
            });

            //Add header  
            AddCellToHeader(tableLayout, "Model Name");
            AddCellToHeader(tableLayout, " ");
            AddCellToHeader(tableLayout, " ");
            AddCellToHeader(tableLayout, SelectedModel);
            //Add body  
            AddCellToBody(tableLayout, "Unit Power Supply");
            AddCellToBody(tableLayout, " ");
            AddCellToBody(tableLayout, " ");
            AddCellToBody(tableLayout, SelectedPower.DisplayName);
            Color_Counter++;

            Column_Count = 0;
            AddCellToBody(tableLayout, "Air Flow Rate");
            Column_Count++;
            AddCellToBody(tableLayout, "Hi");
            Column_Count++;
            AddCellToBody(tableLayout, "m³/h");
            Column_Count++;
            AddCellToBody(tableLayout, obj.AirFlowProp_High);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Med");
            Column_Count++;
            AddCellToBody(tableLayout, "m³/h");
            Column_Count++;
            AddCellToBody(tableLayout, obj.AirFlowProp_medium);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Lo");
            Column_Count++;
            AddCellToBody(tableLayout, "m³/h");
            Column_Count++;
            AddCellToBody(tableLayout, obj.AirFlowProp_low);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            //External Static Pressure
            AddCellToBody(tableLayout, "External Static Pressure");
            Column_Count++;
            AddCellToBody(tableLayout, "Hi");
            Column_Count++;
            AddCellToBody(tableLayout, "Pa");
            Column_Count++;
            AddCellToBody(tableLayout, obj.ExternalStaticPressure_high);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Med");
            Column_Count++;
            AddCellToBody(tableLayout, "Pa");
            Column_Count++;
            AddCellToBody(tableLayout, obj.ExternalStaticPressure_medium);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Lo");
            Column_Count++;
            AddCellToBody(tableLayout, "Pa");
            Column_Count++;
            AddCellToBody(tableLayout, obj.ExternalStaticPressure_low);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;


            //Temp. Exchange Efficiency
            if (!DataValue.Contains("HAPQ"))
            {
                Column_Count = 0;
                AddCellToBody(tableLayout, "Temp. Exchange Efficiency");
                Column_Count++;
                AddCellToBody(tableLayout, "Hi");
                Column_Count++;
                AddCellToBody(tableLayout, "%");
                Column_Count++;
                AddCellToBody(tableLayout, obj.TempExchangeEff_high);
                Column_Count++;
                Color_Counter++;
            }
            Column_Count = 0;

            if (!DataValue.Contains("HAPQ"))
            {
                Column_Count = 0;
                AddCellToBody(tableLayout, "");
                Column_Count++;
                AddCellToBody(tableLayout, "Med");
                Column_Count++;
                AddCellToBody(tableLayout, "%");
                Column_Count++;
                AddCellToBody(tableLayout, obj.TempExchangeEff_medium);
                Column_Count++;
                Color_Counter++;
            }
            Column_Count = 0;

            if (!DataValue.Contains("HAPQ"))
            {
                Column_Count = 0;
                AddCellToBody(tableLayout, "");
                Column_Count++;
                AddCellToBody(tableLayout, "Lo");
                Column_Count++;
                AddCellToBody(tableLayout, "%");
                Column_Count++;
                AddCellToBody(tableLayout, obj.TempExchangeEff_Low);
                Column_Count++;
                Color_Counter++;
            }
            Column_Count = 0;

            //Enthalpy Exchange Effieiency - for Heating
            AddCellToBody(tableLayout, "Enthalpy Exchange Efficiency - for Heating");
            Column_Count++;
            AddCellToBody(tableLayout, "Hi");
            Column_Count++;
            AddCellToBody(tableLayout, "%");
            Column_Count++;
            AddCellToBody(tableLayout, obj.EnthalpyExchangeEffHeat_high);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Med");
            Column_Count++;
            AddCellToBody(tableLayout, "%");
            Column_Count++;
            AddCellToBody(tableLayout, obj.EnthalpyExchangeEffHeat_medium);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Lo");
            Column_Count++;
            AddCellToBody(tableLayout, "%");
            Column_Count++;
            AddCellToBody(tableLayout, obj.EnthalpyExchangeEffHeat_low);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            //Enthalpy Exchange Effieiency - for Cooling
            AddCellToBody(tableLayout, "Enthalpy Exchange Effieiency - for Cooling");
            Column_Count++;
            AddCellToBody(tableLayout, "Hi");
            Column_Count++;
            AddCellToBody(tableLayout, "%");
            Column_Count++;
            AddCellToBody(tableLayout, obj.EnthalpyExEffCool_high);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Med");
            Column_Count++;
            AddCellToBody(tableLayout, "%");
            Column_Count++;
            AddCellToBody(tableLayout, obj.EnthalpyExEffCool_medium);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Lo");
            Column_Count++;
            AddCellToBody(tableLayout, "%");
            Column_Count++;
            AddCellToBody(tableLayout, obj.EnthalpyExEffCool_low);
            Column_Count++;
            Color_Counter++;
            Column_Count = 0;

            AddCellToBody(tableLayout, "Outer Dimensions");
            Column_Count++;
            AddCellToBody(tableLayout, "Height");
            Column_Count++;
            AddCellToBody(tableLayout, "mm");
            Column_Count++;
            AddCellToBody(tableLayout, obj.OuterDimensions_height);
            Column_Count++;
            Color_Counter++;

            Column_Count = 0;
            AddCellToBody(tableLayout, " ");
            Column_Count++;
            AddCellToBody(tableLayout, "Width");
            Column_Count++;
            AddCellToBody(tableLayout, "mm");
            Column_Count++;
            AddCellToBody(tableLayout, obj.OuterDimensions_width);
            Column_Count++;
            Color_Counter++;

            Column_Count = 0;
            AddCellToBody(tableLayout, "");
            Column_Count++;
            AddCellToBody(tableLayout, "Depth");
            Column_Count++;
            AddCellToBody(tableLayout, "mm");
            Column_Count++;
            AddCellToBody(tableLayout, obj.OuterDimensions_depth);
            Column_Count++;
            Color_Counter++;

            Column_Count = 0;
            AddCellToBody(tableLayout, "Net Weight");
            Column_Count++;
            AddCellToBody(tableLayout, " ");
            Column_Count++;
            AddCellToBody(tableLayout, "Kg");
            Column_Count++;
            AddCellToBody(tableLayout, obj.NetWeight);
            Column_Count++;
            Color_Counter++;

            Column_Count = 0;
            AddCellToBody(tableLayout, "Connection Duct Diameter");
            Column_Count++;
            AddCellToBody(tableLayout, " ");
            Column_Count++;
            AddCellToBody(tableLayout, "mm");
            Column_Count++;
            AddCellToBody(tableLayout, obj.ConnectionDuctDiameter);
            Column_Count++;
            return tableLayout;
        }

        public void OnGADrawingBtnClick()
        {
            String ModelName_GA;
            String dirPath = Directory.GetCurrentDirectory();
            string newPath = Path.GetFullPath(Path.Combine(dirPath, @"..\..\"));
            newPath += "TotalHeatExchangerPDFFiles\\";
            newPath += SystemSetting.UserSetting.defaultSetting.Language;
            if (!Directory.Exists(newPath))
            {
                JCHMessageBox.Show("The selected Language Doesn't exist, opening in English Language");
                newPath = Path.GetFullPath(Path.Combine(newPath, @"..\"));
                newPath += "English";
            }


            //newPath += "\\GD_Drawing_PDF";
            newPath += "\\GD_";
            try
            {
                ModelName_GA = SelectedModel.ToString();
                // Added to handle the name mismatch for YORK in database(with or without O)
                if (Project.CurrentProject.BrandCode == "Y")
                {
                    int indexOfO = SelectedModel.IndexOf('O');
                    if (indexOfO == -1)
                    {
                        ModelName_GA = ModelName_GA.Insert(10, "O");
                    }
                }
                newPath += ModelName_GA;
                newPath += ".pdf";

                try
                {
                    System.Diagnostics.Process.Start(newPath);
                }
                catch (Exception e)
                {
                    JCHMessageBox.Show(e.Message, MessageType.Warning, MessageBoxButton.OK);
                    return;
                }
            }
            catch (Exception)
            {
                JCHMessageBox.Show("Please select the model, to get GA drawings");
            }

        }
        public void onFanPerformenceClick()
        {
            String ModelName_GA;
            String dirPath = Directory.GetCurrentDirectory();
            string newPath = Path.GetFullPath(Path.Combine(dirPath, @"..\..\"));
            newPath += "TotalHeatExchangerPDFFiles\\";
            newPath += SystemSetting.UserSetting.defaultSetting.Language;
            if (!Directory.Exists(newPath))
            {
                JCHMessageBox.Show("The selected Language Doesn't exist, opening in English Language");
                newPath = Path.GetFullPath(Path.Combine(newPath, @"..\"));
                newPath += "English";
            }
            //newPath += "\\Fan_Performance_PDF";
            newPath += "\\FA_";
            try
            {
                ModelName_GA = SelectedModel.ToString();
                // Added to handle the name mismatch for YORK in database(with or without O)
                if (Project.CurrentProject.BrandCode == "Y")
                {
                    int indexOfO = SelectedModel.IndexOf('O');
                    if (indexOfO == -1)
                    {
                        ModelName_GA = ModelName_GA.Insert(10, "O");
                    }
                }
                newPath += ModelName_GA;
                newPath += ".pdf";

                try
                {
                    System.Diagnostics.Process.Start(newPath);
                }
                catch (Exception e)
                {
                    JCHMessageBox.Show(e.Message, MessageType.Warning, MessageBoxButton.OK);
                    return;
                }
            }
            catch (Exception)
            {
                JCHMessageBox.Show("Please select the model, to get Fan performance");
            }

        }


        public void onInstallationGuideClick()
        {
            if (SelectedModel != null)
            {
                String dirPath = Directory.GetCurrentDirectory();
                string newPath = Path.GetFullPath(Path.Combine(dirPath, @"..\..\"));
                newPath += "TotalHeatExchangerPDFFiles\\";
                newPath += SystemSetting.UserSetting.defaultSetting.Language;
                if (!Directory.Exists(newPath))
                {
                    JCHMessageBox.Show("The selected Language Doesn't exist, opening in English Language");
                    newPath = Path.GetFullPath(Path.Combine(newPath, @"..\"));
                    newPath += "English";
                }
                if (Project.CurrentProject.BrandCode == "Y")
                {
                    if (SelectedModel.Contains("200") || SelectedModel.Contains("250") || SelectedModel.Contains("300") || SelectedModel.Contains("400") || SelectedModel.Contains("500") || SelectedModel.Contains("65") || SelectedModel.Contains("80") || SelectedModel.Contains("100") || SelectedModel.Contains("125") || SelectedModel.Contains("150"))
                    {
                        newPath += "\\20190613 IoM JDTH0650NAGQ - JDTH500H0NEGQ";
                    }
                    else
                    {
                        newPath += "\\20190613 IoM JDTH020H0NAGQ-JDTH050H0NAGQ";
                    }
                }
                else
                {
                    newPath += "\\20190424 Installation guide idea";
                }
                newPath += ".pdf";
                try
                {
                    System.Diagnostics.Process.Start(newPath);
                }
                catch (Exception e)
                {
                    JCHMessageBox.Show(e.Message, MessageType.Warning, MessageBoxButton.OK);
                    return;
                }
            }
            else
            {
                JCHMessageBox.Show("Please select the model, to get installation guide");
            }
        }

        private void ValidateHeatEx()
        {
            if (_currentSystem != null)
            {
                proj = JCHVRF.Model.Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(_currentSystem.Id));
                if (proj != null)
                {
                    if (proj.IndoorItem != null)
                    {
                        if (string.IsNullOrWhiteSpace(proj.IndoorItem.Series))
                        {
                            if (_currentSystem._errors != null)
                            {
                                _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                                _currentSystem._errors.Clear();
                                _currentSystem._errors.Add("Model Not Selected");
                                _currentSystem.SystemStatus = Model.SystemStatus.INVALID;
                                _eventAggregator.GetEvent<ErrorLogVM>().Publish(_currentSystem.Errors);
                                _currentSystem.Errors.Clear();
                            }
                        }
                        else
                        {
                            if (_currentSystem.SystemStatus != Model.SystemStatus.INVALID)
                            {
                                _currentSystem.SystemStatus = Model.SystemStatus.VALID;
                            }
                            _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                            _currentSystem.Errors.Clear();
                        }
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                        _currentSystem._errors.Add("Model Not Selected");
                        _currentSystem.SystemStatus = Model.SystemStatus.INVALID;


                        _eventAggregator.GetEvent<ErrorLogVM>().Publish(_currentSystem.Errors);
                        _currentSystem.Errors.Clear();
                    }

                }
            }


        }

        internal void ClearError(JCHVRF.Model.SystemBase system, Prism.Events.IEventAggregator _eventAggregator)
        {
            _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
            if (system._errors != null)
            {
                system._errors.Clear();
            }

        }

        internal void ClearAllError(Prism.Events.IEventAggregator _eventAggregator)
        {
            _eventAggregator.GetEvent<ErrorLogVMClearAll>().Publish();

        }

        ~HeatExchangerCanvasEquipmentViewModel()
        {
            IsPropOrNewExch = false;
        }

    }

}