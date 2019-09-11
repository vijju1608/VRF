using JCBase.UI;
using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF.Model.New;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Windows;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class DesignerConditionViewModel : ViewModelBase
    {
        #region Varaible and Constant intialization
        private IProjectInfoBAL iProjectInfoBll;
        JCHVRF.Model.Project projectLegacy = null;
        JCHVRF.Model.DefaultSettingModel designConditionsLegacy = SystemSetting.UserSetting.defaultSetting;
        DesignCondition designconditions = null;
        UnitsSettingModel unitsSettingModel = new UnitsSettingModel();
        FileSettingModel fileSettingModel = new FileSettingModel();
        string ut_length = string.Empty;
        private IEventAggregator _eventAggregator;
        #endregion Varaible and Constant intialization

        #region Local Properties
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

        #region Viewmodel properties
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
        private double _NumericAltitude;
        public double NumericAltitude
        {
            get
            {
                return _NumericAltitude;
            }
            set
            {
                this.SetValue(ref _NumericAltitude, value);
            }
        }
        private decimal _indoorCoolingDB;
        public decimal indoorCoolingDB
        {
            get
            {
                return _indoorCoolingDB;
            }
            set
            {

                this.SetValue(ref _indoorCoolingDB, value);
                //    NumericCoolDryBulb_LostFocus();
            }
        } //= 27.0m
        private decimal _indoorCoolingWB;
        public decimal indoorCoolingWB
        {
            get
            {
                return _indoorCoolingWB;
            }
            set
            {
                this.SetValue(ref _indoorCoolingWB, value);
                //  NumericCoolWetBulb_LostFocus();
            }
        }// = 19.6m;
        private decimal _indoorCoolingRH;
        public decimal indoorCoolingRH
        {
            get
            {
                return _indoorCoolingRH;
            }
            set
            {
                this.SetValue(ref _indoorCoolingRH, value);
                //  NumericInternalRH_LostFocus();
            }
        }// = 0.0m;
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
                //  NumericHeatingDryBulb_LostFocus();
            }
        }

        #region label Indoor value
        private string _lblindoorCoolingDB;
        public string lblindoorCoolingDB
        {
            get
            {
                return _lblindoorCoolingDB;
            }
            set
            {

                this.SetValue(ref _lblindoorCoolingDB, value);
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
        private string _lblindoorCoolingRH;
        public string lblindoorCoolingRH
        {
            get
            {
                return _lblindoorCoolingRH;
            }
            set
            {
                this.SetValue(ref _lblindoorCoolingRH, value);
            }
        }
        private string _lblindoorHeatingDB;
        public string lblindoorHeatingDB
        {
            get
            {
                return _lblindoorHeatingDB;
            }
            set
            {
                this.SetValue(ref _lblindoorHeatingDB, value);
            }
        }
        #endregion label value

        private string _lbloutdoorCoolingIW;
        public string lbloutdoorCoolingIW
        {
            get
            {
                return _lbloutdoorCoolingIW;
            }
            set
            {

                this.SetValue(ref _lbloutdoorCoolingIW, value);
            }
        }
        private string _lbloutdoorHeatingIW;
        public string lbloutdoorHeatingIW
        {
            get
            {
                return _lbloutdoorHeatingIW;
            }
            set
            {

                this.SetValue(ref _lbloutdoorHeatingIW, value);
            }
        }
        private decimal _outdoorCoolingDB;
        public decimal outdoorCoolingDB
        {
            get
            {
                return _outdoorCoolingDB;
            }
            set
            {
                this.SetValue(ref _outdoorCoolingDB, value);
                //  NumericOutdoorDB_LostFocus();
            }
        }// = 35.0m;
        private decimal _outdoorHeatingDB;
        public decimal outdoorHeatingDB
        {
            get
            {
                return _outdoorHeatingDB;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingDB, value);
                //  NumeroutdoorHDDBT_LostFocus();
            }
        }//= 7.0m;
        private decimal _outdoorHeatingWB;
        public decimal outdoorHeatingWB
        {
            get
            {
                return _outdoorHeatingWB;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingWB, value);
                //  NumeroutdoorHDWBT_LostFocus();
            }
        }//= 3.1m;
        private decimal _outdoorHeatingRH;
        public decimal outdoorHeatingRH
        {
            get
            {
                return _outdoorHeatingRH;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingRH, value);
                //  NumeroutdoorHDRH_LostFocus();
            }
        } //= 87.00m;
        private decimal _outdoorHeatingIW;
        public decimal outdoorHeatingIW
        {
            get
            {
                return _outdoorHeatingIW;
            }
            set
            {
                this.SetValue(ref _outdoorHeatingIW, value);

            }
        }
        private decimal _outdoorCoolingIW;
        public decimal outdoorCoolingIW
        {
            get
            {
                return _outdoorCoolingIW;
            }
            set
            {
                this.SetValue(ref _outdoorCoolingIW, value);

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
        }//= 3.1m;

        private string _lbllblAltitude;
        public string LbllblAltitude
        {
            get
            {
                return _lbllblAltitude;
            }
            set
            {
                this.SetValue(ref _lbllblAltitude, value);
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
        private string _SelectedCountry;

        public string SelectedCountry
        {
            get { return _SelectedCountry; }
            set { this.SetValue(ref _SelectedCountry, value); }
        }

        #endregion label outdoor value

        private List<ComboBox> _Country;
        public List<ComboBox> ListCountry
        {
            get { return _Country; }
            set { this.SetValue(ref _Country, value); }
        }
        private string _SelectedCity;

        public string SelectedCity
        {
            get { return _SelectedCity; }
            set { this.SetValue(ref _SelectedCity, value); }
        }

        private bool _IsCheck;
        public bool IsCheck
        {
            get { return _IsCheck; }
            set { this.SetValue(ref _IsCheck, value); }
        }

        private List<ComboBox> _City;
        public List<ComboBox> ListCity
        {
            get { return _City; }
            set { this.SetValue(ref _City, value); }
        }
        private string _txtAltitudeUnit;

        public string txtAltitudeUnit
        {
            get { return _txtAltitudeUnit; }
            set { this.SetValue(ref _txtAltitudeUnit, value); }
        }
        private bool _EnableAltitudeCorrectionFactor;
        public bool EnableAltitudeCorrectionFactor
        {
            get { return _EnableAltitudeCorrectionFactor; }
            set { _EnableAltitudeCorrectionFactor = value; }
        }
        #endregion

        #region Delegate Commands

        public DelegateCommand NumericAltitudeCommand { get; set; }
        public DelegateCommand ChangeTempCommand { get; set; }
        //  public DelegateCommand CountryChangeCommmand { get; set; }
        // public DelegateCommand CityChangeCommmand { get; set; }
        public DelegateCommand NumericOutdoorDBCommand { get; set; }
        public DelegateCommand NumeroutdoorHDWBTCommand { get; set; }
        public DelegateCommand NumeroutdoorHDRHCommand { get; set; }
        public DelegateCommand NumeroutdoorHDDBTCommand { get; set; }
        public DelegateCommand NumericCoolDryBulbCommand { get; set; }
        public DelegateCommand NumericCoolWetBulbCommand { get; set; }
        public DelegateCommand NumericInternalRHCommand { get; set; }
        public DelegateCommand NumericHeatingDryBulbCommand { get; set; }
        public DelegateCommand NumericOutdoorIntelWaterCCommand { get; set; }
        public DelegateCommand NumeroutdoorIntelWaterTempHCommand { get; set; }
        public DelegateCommand AltitudeCorrectionCheckedCommand { get; set; }
        public DelegateCommand AltitudeCorrectionUnCheckedCommand { get; set; }
        public DelegateCommand DesignConditionInfoWindowLoaded { get; set; }
        public string ErrorMessage { get; private set; }

        #endregion Delegate Commands

        #region constructor and Initisation
        public DesignerConditionViewModel(IProjectInfoBAL projectInfoBll, IEventAggregator eventAggregator)
        {
            projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            _eventAggregator = eventAggregator;
            iProjectInfoBll = projectInfoBll;
            Initialisationvalues();
            NumericAltitudeCommand = new DelegateCommand(NumericAltitude_LostFocus);
            ChangeTempCommand = new DelegateCommand(btnChangeTempClicked);
            NumericOutdoorDBCommand = new DelegateCommand(NumericOutdoorDB_LostFocus);
            NumeroutdoorHDWBTCommand = new DelegateCommand(NumeroutdoorHDWBT_LostFocus);
            NumeroutdoorHDRHCommand = new DelegateCommand(NumeroutdoorHDRH_LostFocus);
            NumeroutdoorHDDBTCommand = new DelegateCommand(NumeroutdoorHDDBT_LostFocus);
            NumericCoolDryBulbCommand = new DelegateCommand(NumericCoolDryBulb_LostFocus);
            NumericCoolWetBulbCommand = new DelegateCommand(NumericCoolWetBulb_LostFocus);
            NumericInternalRHCommand = new DelegateCommand(NumericInternalRH_LostFocus);
            NumericHeatingDryBulbCommand = new DelegateCommand(NumericHeatingDryBulb_LostFocus);
            NumericOutdoorIntelWaterCCommand = new DelegateCommand(NumericOutdoorIntelWaterC_LostFocus);
            NumeroutdoorIntelWaterTempHCommand = new DelegateCommand(NumeroutdoorIntelWaterTempH_LostFocus);
            // CountryChangeCommmand = new DelegateCommand(cmbCountryChangeEvent);
            AltitudeCorrectionCheckedCommand = new DelegateCommand(AltitudeCorrectionCheckedEvent);
            AltitudeCorrectionUnCheckedCommand = new DelegateCommand(AltitudeCorrectionUnCheckedEvent);
            //CityChangeCommmand = new DelegateCommand(cmbCityChangedEvent);
            _eventAggregator.GetEvent<DesignConditionTabNext>().Subscribe(DesignerTabNextClick);
            this.ListCountry = GetCountryList();
            SetDefaultDesignConditions();
            SettingsControlDisable();
           
            DesignConditionInfoWindowLoaded = new DelegateCommand(GetDesignConditionsDetails);
          
               
         
           

        }
        /// <summary>
        /// value initialisation
        /// </summary>
        void Initialisationvalues()
        {
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            txtAltitudeUnit = ut_length;
            // projectLegacy.EnableAltitudeCorrectionFactor = fileSettingModel.EnableAltitudeCorrectionFactor;

            if (projectLegacy != null)
            {
                if (projectLegacy.EnableAltitudeCorrectionFactor == true)
                {
                    EnableAltitudeCorrectionFactor = true;
                }
                else
                    EnableAltitudeCorrectionFactor = false;
                NumericAltitude = Unit.ConvertToControl(projectLegacy.Altitude, UnitType.LENGTH_M, ut_length);
            }
            else
            {
                projectLegacy.EnableAltitudeCorrectionFactor = fileSettingModel.EnableAltitudeCorrectionFactor;
            }
            // projectLegacy.EnableAltitudeCorrectionFactor = fileSettingModel.EnableAltitudeCorrectionFactor;
            if (projectLegacy != null)
            {
                if (projectLegacy.EnableAltitudeCorrectionFactor == true)
                {
                    EnableAltitudeCorrectionFactor = true;
                }
                else
                    EnableAltitudeCorrectionFactor = false;
                NumericAltitude = Unit.ConvertToControl(projectLegacy.Altitude, UnitType.LENGTH_M, ut_length);
            }
            else
            {
                projectLegacy.EnableAltitudeCorrectionFactor = fileSettingModel.EnableAltitudeCorrectionFactor;
            }
            TemperatureTypeOCDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;//updated as part of default measurement units
            TemperatureTypeOCIW = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeOHIW = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeOCWB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeOHDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeICDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeICWB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            TemperatureTypeIHDB = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            //if (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE == Celcisous)
            //    ChangeTemp = ChangeToF;
            //else
            //    ChangeTemp = ChangeToC;
        }
        #endregion constructor

        #region Delegate Command Events
        /// <summary>
        /// AltitudeCorrection check box
        /// </summary>
        void AltitudeCorrectionCheckedEvent()
        {
            EnableAltitudeCorrectionFactor = true;
            projectLegacy.EnableAltitudeCorrectionFactor = true;
        }
        /// <summary>
        /// AltitudeCorrection  Uncheck box
        /// </summary>
        void AltitudeCorrectionUnCheckedEvent()
        {
            EnableAltitudeCorrectionFactor = false;
            projectLegacy.EnableAltitudeCorrectionFactor = false;
        }
        /// <summary>
        /// validate Alittude range [0,5000]
        /// </summary>
        /// <param name="NumericAltitude"></param>
        /// <returns></returns>
        bool JCValidateSingle(double NumericAltitude)
        {
            if (NumericAltitude >= 0 && NumericAltitude <= 5000)
            {
                LbllblAltitude = string.Empty;
                return true;
            }
            else
            {
                LbllblAltitude = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(0, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(5000, UnitType.TEMPERATURE, CurrentTempUnit));
                return false;
            }
        }
        /// <summary>
        /// Altitude change
        /// </summary>
        /// <summary>
        /// Change to Celsious and ferienhiet
        /// </summary>
        void btnChangeTempClicked()
        {
            if (IsCountryAndCitySelected())
            {
                CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                CurrentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = CurrentTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c;
                
                RaisePropertyChanged("indoorCoolingDB");
                RaisePropertyChanged("indoorCoolingWB");
                RaisePropertyChanged("indoorHeatingDB");
                RaisePropertyChanged("outdoorCoolingDB");
                RaisePropertyChanged("outdoorHeatingDB");
                RaisePropertyChanged("outdoorHeatingWB");
                RaisePropertyChanged("outdoorCoolingIW");
                RaisePropertyChanged("outdoorHeatingIW");
                
                TemperatureTypeOCDB = TemperatureTypeOCWB = TemperatureTypeOHDB = TemperatureTypeICDB = TemperatureTypeICWB = TemperatureTypeIHDB = TemperatureTypeOCIW = TemperatureTypeOHIW = CurrentTempUnit;

            }
            else
            {
               
                JCHMessageBox.ShowWarning(Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_CONTRY_SELECTION"));//"Please select country and city first"
                return;
            }

        }
        /// <summary>
        /// validation for country and city selection
        /// </summary>
        /// <returns></returns>
        bool IsCountryAndCitySelected()
        {
            //if (!string.IsNullOrWhiteSpace(SelectedCity) && IntergerParser(SelectedCity) > 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }
        /// <summary>
        /// country change
        /// </summary>
        void cmbCountryChangeEvent()
        {
            //if (!string.IsNullOrWhiteSpace(SelectedCountry) && IntergerParser(SelectedCountry) > 0)
            //{
            //    this.ListCity = GetCityList(IntergerParser(SelectedCountry));
            //    SelectedCity = "0";
            //}
            //else
            //{
            //    JCHMessageBox.ShowWarning("Please select country!");
            //    return;
            //}

        }
        /// <summary>
        /// City Drop down change event
        /// </summary>
        void cmbCityChangedEvent()
        {
            //Todo
            if (!string.IsNullOrWhiteSpace(SelectedCountry) && IntergerParser(SelectedCountry) > 0 && !string.IsNullOrWhiteSpace(SelectedCity) && IntergerParser(SelectedCity) > 0)
            {
                // SetDefaultDesignConditions(IntergerParser(SelectedCity));
            }
            else
                
            {
               
                JCHMessageBox.ShowWarning(Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_CITY_SELECTION"));//"Please select city!"
                return;
            }

        }
        /// <summary>
        /// Altitude Change event
        /// </summary>
        void NumericAltitude_LostFocus()
        {
            //if (string.IsNullOrWhiteSpace(Convert.ToString(NumericAltitude)) && Convert.ToString(NumericAltitude).Length > 0)
            //{
                if (JCValidateSingle(NumericAltitude))
                {
                    if (projectLegacy.Altitude != Convert.ToInt32(Unit.ConvertToSource(double.Parse(NumericAltitude.ToString()), UnitType.LENGTH_M, ut_length)))
                    {

                        if (projectLegacy.EnableAltitudeCorrectionFactor)
                        {
                            if (JCHMessageBox.Show(Msg.CONFIRM_CHANGEALTITUDE(), MessageType.Information, MessageBoxButton.YesNoCancel) == MessageBoxResult.Cancel)
                            {
                                NumericAltitude = Unit.ConvertToControl(projectLegacy.Altitude, UnitType.LENGTH_M, ut_length);
                                return;
                            }
                            else
                            {
                                projectLegacy.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(NumericAltitude.ToString()), UnitType.LENGTH_M, ut_length));
                            }
                        }
                        else
                            projectLegacy.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(NumericAltitude.ToString()), UnitType.LENGTH_M, ut_length));
                    }
                }
            //}
            //else
            //    JCHMessageBox.ShowWarning("Altitude should not Contain blank spaces");
            
        }
        /// <summary>
        /// NumericOutdoorIntelWater
        /// </summary>
        private void NumericOutdoorIntelWaterC_LostFocus()
        {
            if (ValidateOIw() == false)
            {

            }
            else
            {

            }
        }
        /// <summary>
        /// NumeroutdoorIntelWaterTemp
        /// </summary>
        private void NumeroutdoorIntelWaterTempH_LostFocus()
        {
            if (ValidateOHDIW() == false)
            {

            }

        }
        /// <summary>
        /// NumericCoolDryBulb
        /// </summary>
        public void NumericCoolDryBulb_LostFocus()
        {
            if (ValidateCoolDryBulb() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd("DB");
            }
        }
        /// <summary>
        /// NumericCoolWetBulb
        /// </summary>
        private void NumericCoolWetBulb_LostFocus()
        {
            if (ValidateCoolWetBulb() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd("WB");
            }

        }
        /// <summary>
        /// InternalRH
        /// </summary>
        private void NumericInternalRH_LostFocus()
        {

            if (ValidateRH() == false)
            {

            }
            else
            {
                if(_indoorCoolingRH.ToString().Length>1) //As per Legacy implemented
                { 
                     DoCalculateByOptionInd(UnitTemperature.RH.ToString()); //issue fix 1647
                }                                                       // DoCalculateByOptionInd(NumericInternalRH.Value.ToString());
            }
        }
        /// <summary>
        /// HeatingDryBulb
        /// </summary>
        private void NumericHeatingDryBulb_LostFocus()
        {
            if (ValidateHDB() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd(UnitTemperature.RH.ToString()); //issue fix 1647
                //  DoCalculateByOptionInd(NumericInternalRH.Value.ToString());
            }
        }
        /// <summary>
        /// NumeroutdoorHDWBT_LostFocus
        /// </summary>
        private void NumeroutdoorHDWBT_LostFocus()
        {
            if (ValidateOHDWBT() == false)
            {

            }
            else
            {
                DoCalculateByOptionOut(UnitTemperature.WB.ToString());

            }

        }
        /// <summary>
        /// NumeroutdoorHDDBT_LostFocus
        /// </summary>
        private void NumeroutdoorHDDBT_LostFocus()
        {
            if (ValidateHDDBT() == false)
            {

            }
            else
            {
                DoCalculateByOptionOut(UnitTemperature.DB.ToString());

            }


        }
        /// <summary>
        /// 
        /// </summary>
        private void NumeroutdoorHDRH_LostFocus()
        {
            if (ValidateOutdoorHDRH() == false)
            {

            }
            else
            {
                DoCalculateByOptionOut(UnitTemperature.RH.ToString());
            }

        }
        /// <summary>
        /// 
        /// </summary>
        private void NumericOutdoorDB_LostFocus()
        {
            if (ValidateOdb() == false)
            {

            }
        }
        /// <summary>
        /// Design condition next button event
        /// </summary>
        ///
        private bool DesignerTabGetinfo()
        {
            bool IsValidData = true;
            JCHVRF.Model.Project CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            if (ValidateCoolDryBulb() == false || ValidateCoolWetBulb() == false || ValidateRH() == false || ValidateHDDBT() == false || ValidateOdb() == false || ValidateOIw() == false || ValidateOHDIW() == false || ValidateOHDWBT() == false || ValidateOutdoorHDRH() == false || ValidateOHDIW() == false || ValidateHDB() == false || JCValidateSingle(NumericAltitude) ==false)
            {
                IsValidData = false;
            }
            else
            {
                SaveDesigncondition();
                projectLegacy.Altitude = Convert.ToInt32(NumericAltitude);
                projectLegacy.EnableAltitudeCorrectionFactor = EnableAltitudeCorrectionFactor;
                projectLegacy.DesignCondition = designconditions;
                #region Measurement Unit Save
                SetData();
                SystemSetting.Serialize();
                #endregion Measurement Unit Save
            }
            return IsValidData;
        }

        public void DesignerTabNextClick()
        {
            if (DesignerTabGetinfo())
            {
                _eventAggregator.GetEvent<DesignerTabSubscriber>().Publish(true);
            }
            else
            {
                _eventAggregator.GetEvent<DesignerTabSubscriber>().Publish(false);
            }
        }
        #endregion Delegate Command Events

        public void GetDesignConditionsDetails()
        {
            if (Application.Current.Properties["Value"] != null)
            {
               
                var proj = JCHVRF.Model.Project.GetProjectInstance;
                if (proj.DesignCondition != null)
                {
                    outdoorCoolingDB = proj.DesignCondition.outdoorCoolingDB;
                    outdoorCoolingIW = proj.DesignCondition.outdoorCoolingIW;
                    outdoorHeatingDB = proj.DesignCondition.outdoorHeatingDB;
                    outdoorHeatingWB = proj.DesignCondition.outdoorHeatingWB;
                    outdoorHeatingRH = proj.DesignCondition.outdoorHeatingRH;
                    outdoorHeatingIW = proj.DesignCondition.outdoorHeatingIW;
                    indoorCoolingDB = proj.DesignCondition.indoorCoolingDB;
                    indoorCoolingWB = proj.DesignCondition.indoorCoolingWB;
                    indoorCoolingRH = proj.DesignCondition.indoorCoolingRH;
                    indoorHeatingDB = proj.DesignCondition.indoorCoolingHDB;
                    IsCheck = false;
                }
               
            }
            

        }



        #region Private Methods
        private void SetData()
        {
            //SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = ChangeTemp == ChangeToF ? Celcisous : Farienhiet;
            SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB = Convert.ToDouble(indoorCoolingDB);
            SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB = Convert.ToDouble(indoorCoolingWB); //designConditionsLegacy.IndoorCoolingWB;
            SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH = Convert.ToDouble(indoorCoolingRH); //designConditionsLegacy.IndoorCoolingRH;
            SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB = Convert.ToDouble(indoorHeatingDB); //designConditionsLegacy.IndoorHeatingDB;
            SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB = Convert.ToDouble(outdoorCoolingDB); //designConditionsLegacy.OutdoorCoolingDB;
            SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW = Convert.ToDouble(outdoorCoolingIW); //designConditionsLegacy.OutdoorCoolingIW;
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB = Convert.ToDouble(outdoorHeatingDB); //designConditionsLegacy.OutdoorHeatingDB;
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB = Convert.ToDouble(outdoorHeatingWB.ToString("n1")); //designConditionsLegacy.OutdoorHeatingWB;
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingRH = Convert.ToDouble(outdoorHeatingRH); //designConditionsLegacy.OutdoorHeatingRH;
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW = Convert.ToDouble(outdoorHeatingIW); //designConditionsLegacy.OutdoorHeatingIW;
        }
        private void SettingsControlDisable()
        {

            //if (MasterDesignerViewModel.IsDisable == "true") //have to modify that condition for design condition control disable
            //{
            //    IsCheck = true;
            //}
            if (string.IsNullOrEmpty(ProjectInfoViewModel.checkDesignConditionVal)) //have to modify that condition for design condition control disable
            {
                IsCheck = true;
            }
            if (!string.IsNullOrEmpty(ProjectInfoViewModel.checkDesignConditionVal))//have to modify that condition for design condition control disable
            {
                IsCheck = true;
            }
        }

        #endregion  Private Methods

        #region helping Methods
        /// <summary>
        /// Interger parser
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        static int IntergerParser(object Value)
        {
            int valueReturn = 0;
            if (Value != null)
            {
                if (Value != DBNull.Value)
                {
                    int.TryParse(Convert.ToString(Value), out valueReturn);
                }
            }
            return valueReturn;
        }
        /// <summary>
        /// get all country list
        /// </summary>
        /// <returns></returns>
        public List<ComboBox> GetCountryList()
        {
            List<Tuple<string, string>> getCountryList = null;
            List<ComboBox> ListCountry = new List<ComboBox>();
            getCountryList = iProjectInfoBll.GetCountry();
            getCountryList.ForEach((item) =>
            {
                ListCountry.Add(new ComboBox { Value = item.Item1, DisplayName = item.Item2 });
            });
            SelectedCountry = "0";
            return ListCountry;
        }
        /// <summary>
        /// get list of city
        /// </summary>
        /// <param name="CountryID"></param>
        /// <returns></returns>
        public List<ComboBox> GetCityList(int? CountryID)
        {
            List<Tuple<string, string>> getCityList = null;
            List<ComboBox> ListCity = new List<ComboBox>();
            getCityList = iProjectInfoBll.GetCity(CountryID);
            getCityList.ForEach((item) =>
            {
                ListCity.Add(new ComboBox { Value = item.Item1, DisplayName = item.Item2 });
            });

            return ListCity;
        }

        /// <summary>
        /// set default design condition
        /// </summary>
        /// <param name="SelectedCity"></param>
        //void SetDefaultDesignConditions(int? SelectedCity)
        void SetDefaultDesignConditions()
        {

            indoorCoolingDB = Convert.ToDecimal(designConditionsLegacy.IndoorCoolingDB);//Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorCoolingDB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            indoorCoolingWB = Convert.ToDecimal(designConditionsLegacy.IndoorCoolingWB); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorCoolingWB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            indoorCoolingRH = Convert.ToDecimal(designConditionsLegacy.IndoorCoolingRH); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorCoolingRH), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            indoorHeatingDB = Convert.ToDecimal(designConditionsLegacy.IndoorHeatingDB); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorHeatingDB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));

            if (projectLegacy.SubRegionCode == "ME_T3A" || projectLegacy.SubRegionCode == "ME_T3B")
            {
                outdoorCoolingDB = 52;
            }
            else
            {
                outdoorCoolingDB = Convert.ToDecimal(designConditionsLegacy.OutdoorCoolingDB); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorCoolingDB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));

            }
          
            outdoorCoolingIW = Convert.ToDecimal(designConditionsLegacy.OutdoorCoolingIW); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorCoolingIW), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            outdoorHeatingDB = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingDB); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingDB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            outdoorHeatingWB = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingWB.ToString("n1")); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingWB), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            outdoorHeatingRH = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingRH); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingRH), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            outdoorHeatingIW = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingIW); //Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingIW), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE));
            
            calRH((decimal)designConditionsLegacy.IndoorCoolingDB, (decimal)designConditionsLegacy.IndoorCoolingWB, true);
            calRH((decimal)designConditionsLegacy.OutdoorCoolingDB, (decimal)designConditionsLegacy.OutdoorHeatingWB, false);

        }
        /// <summary>
        /// cal R H
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="wt"></param>
        /// <param name="isIn"></param>
        void calRH(decimal dt, decimal wt, bool isIn)
        {
            FormulaCalculate fcal = new FormulaCalculate();
            decimal p = fcal.GetPressure(0);
            decimal rh = fcal.GetRH(dt, wt, p);
            if (isIn)
            {
                indoorCoolingRH = Convert.ToDecimal((rh * 100).ToString("n0"));
            }
            else
            {
                //NumeroutdoorHDRH.Value = Convert.ToDecimal((rh * 100).ToString("n0")); // This sets RH to zero everytime. commenting it for now.
            }

        }
        
        public void SaveDesigncondition()
        {
            designconditions = new DesignCondition();
            string TemperatureFormate = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            if (TemperatureFormate == "°F")
            {
                TemperatureFormate = "°C";
                indoorCoolingDB = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorCoolingDB), UnitType.TEMPERATURE, TemperatureFormate));
                indoorCoolingWB = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorCoolingWB), UnitType.TEMPERATURE, TemperatureFormate));
                indoorCoolingRH = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorCoolingRH), UnitType.TEMPERATURE, TemperatureFormate));
                indoorHeatingDB = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.IndoorHeatingDB), UnitType.TEMPERATURE, TemperatureFormate));
                outdoorCoolingDB = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorCoolingDB), UnitType.TEMPERATURE, TemperatureFormate));
                outdoorCoolingIW = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorCoolingIW), UnitType.TEMPERATURE, TemperatureFormate));
                outdoorHeatingDB = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingDB), UnitType.TEMPERATURE, TemperatureFormate));
                outdoorHeatingWB = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingWB.ToString("n1")), UnitType.TEMPERATURE, TemperatureFormate));
                outdoorHeatingRH = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingRH), UnitType.TEMPERATURE, TemperatureFormate));
                outdoorHeatingIW = Convert.ToDecimal(Unit.ConvertToControl(Convert.ToDouble(designConditionsLegacy.OutdoorHeatingIW), UnitType.TEMPERATURE, TemperatureFormate));
            }
            designconditions = (new DesignCondition
            {
                indoorCoolingDB = indoorCoolingDB,
                indoorCoolingWB = indoorCoolingWB,
                indoorCoolingRH = indoorCoolingRH,
                indoorCoolingHDB = indoorHeatingDB,
                outdoorHeatingDB = outdoorHeatingDB,
                outdoorCoolingDB = outdoorCoolingDB,
                outdoorHeatingWB = outdoorHeatingWB,
                outdoorHeatingRH = outdoorHeatingRH,
                outdoorCoolingIW = outdoorCoolingIW,
                outdoorHeatingIW = outdoorHeatingIW
            });
        }
        #endregion helping Methods

        #region validation methods
        
        private bool ValidateOIw()
        {
            double nOdb = Math.Round((Convert.ToDouble(outdoorCoolingIW))); //Convert.ToDouble(outdoorCoolingIW);

            if ((nOdb >= 10) && (nOdb <= 45))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorCoolingIW = string.Empty;
                return true;
            }
            else
            {

                lbloutdoorCoolingIW = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(45, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[10, 45]";
                DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                return false;
            }

        }
        
        private bool ValidateOHDIW()
        {
            double nOHDIW = Math.Round((Convert.ToDouble(outdoorHeatingIW))); //Convert.ToDouble(outdoorHeatingIW);

            if ((nOHDIW >= 10) && (nOHDIW <= 45))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorHeatingIW = string.Empty;
                return true;

            }
            else
            {
                lbloutdoorHeatingIW = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(45, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[10, 45]";
                DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                return false;
            }

        }
        private void DoCalculateByOptionOut(string Opt)
        {
            
            if (!string.IsNullOrEmpty(outdoorHeatingDB.ToString()) && !string.IsNullOrEmpty(outdoorHeatingWB.ToString()) && !string.IsNullOrEmpty(this.outdoorHeatingRH.ToString()))
            {
                double dbcool = (Convert.ToDouble(outdoorHeatingDB.ToString()));
                double wbcool = (Convert.ToDouble(this.outdoorHeatingWB.ToString()));
                double rhcool = Convert.ToDouble(this.outdoorHeatingRH);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(0));
                if (Opt == UnitTemperature.WB.ToString())
                {
                    double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));

                    if (this.outdoorHeatingRH.ToString() != (rh * 100).ToString("n0"))
                    {
                        this.outdoorHeatingRH = (decimal)(rh * 100);
                    }
                }
                else if (Opt == UnitTemperature.DB.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                    if (this.outdoorHeatingWB.ToString() != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.outdoorHeatingWB = (decimal)wb;
                        }
                    }
                }
                else if (Opt == UnitTemperature.RH.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                    if (this.outdoorHeatingWB.ToString() != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.outdoorHeatingWB = (decimal)wb;
                        }

                    }
                }

            }

        }
        
        private bool ValidateOdb()
        {
            double nOdb = Math.Round(Convert.ToDouble(outdoorCoolingDB)); //Convert.ToDouble(outdoorCoolingDB);

            if (projectLegacy.SubRegionCode == "ME_T3A" || projectLegacy.SubRegionCode == "ME_T3B")
            {
                //outdoorCoolingDB = 52;

                if ((nOdb >= 10) && (nOdb <= 52))
                {
                    DCErrorMessage = string.Empty;
                    lbloutdoorCoolingDB = string.Empty;
                    return true;

                }
                else
                {
                    lbloutdoorCoolingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(52, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[10, 52]";
                    DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                    return false;
                }
            }

            if ((nOdb >= 10) && (nOdb <= 43))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorCoolingDB = string.Empty;
                return true;

            }
            else
            {
                lbloutdoorCoolingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(10, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(43, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[10, 43]";
                DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                return false;
            }

        }
        
        private bool ValidateCoolWetBulb()
        {
            double nCWBVal = Math.Round((Convert.ToDouble(indoorCoolingWB))); //Convert.ToDouble(indoorCoolingWB);
            
            if ((nCWBVal >= 14) && (nCWBVal <= 24))
            {
                DCErrorMessage = string.Empty;
                lblindoorCoolingWB = string.Empty;
                if (Convert.ToDecimal(indoorCoolingWB) > Convert.ToDecimal(indoorCoolingDB))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                   
                    return false;
                }

                return true;

            }
            else
            {
                lblindoorCoolingWB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(14, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(24, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[14, 24]";

                if (Convert.ToDecimal(indoorCoolingWB) > Convert.ToDecimal(indoorCoolingDB))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                    ErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                }
                return false;
            }


        }
        private bool ValidateRH()
        {
            double nRHVal = Convert.ToDouble(indoorCoolingRH);

            if ((nRHVal >= 0) && (nRHVal <= 100))
            {
                DCErrorMessage = string.Empty;
                lblindoorCoolingRH = string.Empty;
                return true;

            }
            else
            {               
                lblindoorCoolingRH = "Range[0, 100]";
                DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                return false;
            }

        }
        private bool ValidateHDB()
        {
            double nHDBVal = Math.Round((Convert.ToDouble(indoorHeatingDB)));// Convert.ToDouble(indoorHeatingDB);

            if ((nHDBVal >= 16) && (nHDBVal <= 24))
            {
                DCErrorMessage = string.Empty;
                lblindoorHeatingDB = string.Empty;
                return true;

            }
            else
            {
                lblindoorHeatingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(16, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(24, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[16, 24]";
                DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                return false;
            }

        }
        
        private bool ValidateHDDBT()
        {
            double nOHDDBT = Math.Round(Convert.ToDouble(outdoorHeatingDB));

            if ((nOHDDBT >= -18) && (nOHDDBT <= 33))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorHeatingDB = string.Empty;
                if (Convert.ToDecimal(outdoorHeatingDB) < Convert.ToDecimal(outdoorHeatingDB))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                    return false;
                }
                return true;

            }
            else
            {

                lbloutdoorHeatingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-18, UnitType.TEMPERATURE, CurrentTempUnit),  Unit.ConvertToControl(33, UnitType.TEMPERATURE, CurrentTempUnit));

                if (Convert.ToDecimal(outdoorHeatingDB) < Convert.ToDecimal(outdoorHeatingWB))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                   DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                }
                return false;
            }
        }
        private bool ValidateOHDWBT()
        {
            double nOHDWBT = Math.Round((Convert.ToDouble(outdoorHeatingWB)));

            if ((nOHDWBT >= -20) && (nOHDWBT <= 15))
            {
                DCErrorMessage = string.Empty;
                lbloutdoorHeatingWB = string.Empty;
                if (Convert.ToDecimal(outdoorHeatingDB) < Convert.ToDecimal(outdoorHeatingWB))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                   
                    return false;
                }
                return true;

            }
            else
            {

                lbloutdoorHeatingWB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(-20, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(15, UnitType.TEMPERATURE, CurrentTempUnit));

                if (Convert.ToDecimal(outdoorHeatingDB) < Convert.ToDecimal(outdoorHeatingWB))
                {

                    JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                    DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                }
                return false;
            }
        }
        private bool ValidateOutdoorHDRH()
        {
            double nOdb = Convert.ToDouble(outdoorHeatingRH);
            if ((nOdb >= 0) && (nOdb <= 100))
            {
                lbloutdoorHeatingRH = string.Empty;
                return true;
            }
            else
            {
                lbloutdoorHeatingRH = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(0, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(100, UnitType.TEMPERATURE, CurrentTempUnit));//"Range[13, 100]";
                return false;
            }
        }
        private bool ValidateCoolDryBulb()
        {

            double nCDBVal = Math.Round(Convert.ToDouble(indoorCoolingDB));
            if ((nCDBVal >= 16) && (nCDBVal <= 30))
            {
                DCErrorMessage = string.Empty;
                lblindoorCoolingDB = string.Empty;
                return true;

            }
            else
            {
                lblindoorCoolingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(16, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(30, UnitType.TEMPERATURE, CurrentTempUnit));

                if (Convert.ToDecimal(indoorCoolingDB) < Convert.ToDecimal(indoorCoolingWB) && !(indoorCoolingDB == 0))
                {
                    DCErrorMessage = Langauge.Current.GetMessage("ERROR_MSG_DESIGNERCONDITION_TEMPRETURE");//"Tempreture values entered are abnormal"
                    // JCHMessageBox.ShowWarning(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));

                }

                return false;
            }
        }
        public void DoCalculateByOptionInd(string Opt)
        {
            double dbcool =Convert.ToDouble(indoorCoolingDB.ToString());
            double wbcool = Convert.ToDouble(indoorCoolingWB.ToString());
            double rhcool = Convert.ToDouble(indoorCoolingRH);
            FormulaCalculate fc = new FormulaCalculate();
            decimal pressure = fc.GetPressure(Convert.ToDecimal(0));
            if (Opt == UnitTemperature.WB.ToString())
            {
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));

                if (this.indoorCoolingRH.ToString() != (rh * 100).ToString("n0"))
                {
                    this.indoorCoolingRH = Convert.ToDecimal((rh * 100).ToString("n0"));
                }
            }
            else if (Opt == UnitTemperature.DB.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (indoorCoolingDB.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {

                        indoorCoolingWB = Convert.ToDecimal(wb.ToString("n1"));

                    }
                }

            }
            else if (Opt == UnitTemperature.RH.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (this.indoorCoolingWB.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {
                        this.indoorCoolingWB = (Decimal)wb;
                    }

                }
            }
        }
        #endregion validation methods
        
    }
}
