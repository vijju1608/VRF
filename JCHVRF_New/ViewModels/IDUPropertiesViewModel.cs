using JCBase.UI;
using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using JCHVRF_New.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using NextGenModel = JCHVRF.Model.NextGen;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Utility;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;

namespace JCHVRF_New.ViewModels
{
    public class IDUPropertiesViewModel : ViewModelBase
    {
        #region Fields

        private IEventAggregator _eventAggregator;
        private NextGenModel.SystemVRF _currentSystem;

        #endregion 
        #region Local_Property
        private bool _IsEnabledName;
        public bool IsEnabledName
        {
            get { return _IsEnabledName; }
            set
            {
                this.SetValue(ref _IsEnabledName, value);
            }
        }
        private bool _enableManualSelection;
        public bool EnableManualSelection
        {
            get { return _enableManualSelection; }
            set { this.SetValue(ref _enableManualSelection, value); }
        }
        private bool _enableManualselectionIduModel;
        public bool EnableManualselectionIduModel { get { return _enableManualselectionIduModel; } set { this.SetValue(ref _enableManualselectionIduModel, value); } }
        private string _PowerUnit;
        public string PowerUnit
        {
            get
            {
                return _PowerUnit;
            }
            set
            {
                this.SetValue(ref _PowerUnit, value);
            }
        }

        private bool _isHeatingOnlyMode;
        public bool IsHeatingOnlyMode
        {
            get
            {
                return _isHeatingOnlyMode;
            }
            set
            {
                this.SetValue(ref _isHeatingOnlyMode, value);
            }
        }
        private bool _isCoolingOnlyMode;
        public bool IsCoolingOnlyMode
        {
            get
            {
                return _isCoolingOnlyMode;
            }
            set
            {
                this.SetValue(ref _isCoolingOnlyMode, value);
            }
        }

        private double outdoorCoolingDB = 0;
        private double outdoorHeatingWB = 0;
        private double outdoorCoolingIW = 0;
        private double outdoorHeatingIW = 0;
        DataTable IDuModelTypeList = new DataTable();
        string _factory = "";
        string _type = "";
        string ODUProductTypeUniversal = "Universal IDU";
        List<string> _typeList = new List<string>();
        List<Indoor> IDUIndoorList;
        string ODUSeries;// = "Commercial VRF HP, FSXNK"; // "Commercial VRF HP, FSXNK",  Residential VRF HP, FS(V/Y)N1Q/FSNMQ
        string ODUProductType;// = "Comm. Tier 2, HP"; // "Comm. Tier 2, HP"  Res. Tier 1, HP
        IndoorBLL bll;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        string navigateToFolderWithNewImage = "..\\..\\Image\\TypeImages";
        string navigateToFolderWithLegacyImage = "..\\..\\Image\\TypeImageProjectCreation";
        string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        NextGenModel.ImageData imgData = null;

        DataRow datarow;
        #endregion


        #region DelegateCommand
        public DelegateCommand btnEditIndoorUnitNameCommand { get; set; }
        public DelegateCommand OpenAddFloorWindowCommand { get; set; }
        public DelegateCommand OpenAddRoomWindowCommand { get; set; }
        public DelegateCommand UseRoomTemperatueCheckedCommand { get; set; }
        public DelegateCommand UseRoomTemperatueUnCheckedCommand { get; set; }
        public DelegateCommand ChangeTempCommand { get; set; }

        public DelegateCommand AddAccessoryCommand { get; set; }
        public DelegateCommand LostFocusIndoorUnitName { get; set; }
        public DelegateCommand SelectionChangedSelectedIndoorType { get; set; }
        public DelegateCommand SelectionChangedSelectedModel { get; set; }
        public DelegateCommand SelectionChangedSelectedFloor { get; set; }
        public DelegateCommand SelectionChangedSelectedRoom { get; set; }
        public DelegateCommand SelectionChangedSelectedIduPosition { get; set; }
        public DelegateCommand SelectionChangedSelectedFanSpeed { get; set; }
        public DelegateCommand LostFocusCoolingDryBulb { get; set; }
        public DelegateCommand LostFocusCoolingWetBulb { get; set; }
        public DelegateCommand LostFocusRelativeHumidity { get; set; }
        public DelegateCommand LostFocusHeatingDryBulb { get; set; }
        public DelegateCommand LostFocusHeightDifference { get; set; }

        public DelegateCommand ManualSelectionCheckedCommand { get; set; }
        public DelegateCommand ManualSelectionUnCheckedCommand { get; set; }

        #endregion

        public IDUPropertiesViewModel(IEventAggregator EventAggregator, IModalWindowService winService)
        {
            try
            {

                imgData = new NextGenModel.ImageData();
                _eventAggregator = EventAggregator;
                _winService = winService;
                PowerUnit = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                JCHVRF.Model.Project.CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
                bll = new IndoorBLL(JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, JCHVRF.Model.Project.GetProjectInstance.BrandCode);
                OpenAddFloorWindowCommand = new DelegateCommand(OpenAddFloorWindowOnClick);
                OpenAddRoomWindowCommand = new DelegateCommand(OpenAddRoomWindowOnClick);
                UseRoomTemperatueCheckedCommand = new DelegateCommand(UseRoomTemperatueCheckedEvent);
                UseRoomTemperatueUnCheckedCommand = new DelegateCommand(UseRoomTemperatueUnCheckedEvent);
                ChangeTempCommand = new DelegateCommand(ChangeTempEvent);
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Subscribe(OpenGetFloorList);
                _eventAggregator.GetEvent<RoomListSaveSubscriber>().Subscribe(OpenGetRoomList);

                this.DesignConditionTempMasureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                DisplayCurrentTempUnit = Unit.ut_Temperature_f;
                LostFocusIndoorUnitName = new DelegateCommand(LostFocusIndoorUnitNameEvent);
                SelectionChangedSelectedIndoorType = new DelegateCommand(SelectionChangedSelectedIndoorTypeEvent);
                SelectionChangedSelectedModel = new DelegateCommand(SelectionChangedSelectedModelEvent);
                SelectionChangedSelectedFloor = new DelegateCommand(SelectionChangedSelectedFloorEvent);
                SelectionChangedSelectedRoom = new DelegateCommand(SelectionChangedSelectedRoomEvent);
                SelectionChangedSelectedIduPosition = new DelegateCommand(SelectionChangedSelectedIduPositionEvent);
                SelectionChangedSelectedFanSpeed = new DelegateCommand(SelectionChangedSelectedFanSpeedEvent);
                LostFocusCoolingDryBulb = new DelegateCommand(LostFocusCoolingDryBulbEvent);
                LostFocusCoolingWetBulb = new DelegateCommand(LostFocusCoolingWetBulbEvent);
                LostFocusRelativeHumidity = new DelegateCommand(LostFocusRelativeHumidityEvent);
                LostFocusHeatingDryBulb = new DelegateCommand(LostFocusHeatingDryBulbEvent);
                LostFocusHeightDifference = new DelegateCommand(LostFocusHeightDifferenceEvent);
                btnEditIndoorUnitNameCommand = new DelegateCommand(btnEditIndoorUnitNameClickEvent);
                ManualSelectionCheckedCommand = new DelegateCommand(ManualSelectionCheckedEvent);
                ManualSelectionUnCheckedCommand = new DelegateCommand(ManualSelectionUnCheckedEvent);


                BindFloorList();
                GetRoomList();
                BindDefaultFanSpeed();
                BindIDUPosition();
                BindInternalDesignConditions();
                RaisePropertyChanged("AccessoriesCount");
                AddAccessoryCommand = new DelegateCommand(OnAddAccessoryCommandClicked);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void ManualSelectionCheckedEvent()
        {
            EnableManualselectionIduModel = true;
            EnableManualSelection = true;
            UpdateSelectedEquipmentValues("ManualSelection");
        }
        private void ManualSelectionUnCheckedEvent()
        {
            EnableManualselectionIduModel = false;
            EnableManualSelection = false;
            UpdateSelectedEquipmentValues("ManualSelection");
        }
        private void btnEditIndoorUnitNameClickEvent()
        {
            IsEnabledName = !IsEnabledName;
        }

        private void LostFocusHeightDifferenceEvent()
        {
            try
            {
                string ListMaxMinidiff = "";
                bool MaxMincheck = false;
                List<string> ERRList;
                if (SelectedIduPosition == PipingPositionType.SameLevel.ToString())
                    this.HeightDifference = 0;
                else
                {
                    if (JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                    {
                        foreach (var roomindoor in JCHVRF.Model.Project.CurrentProject.RoomIndoorList)
                        {
                            if (SelectedIduPosition == PipingPositionType.Upper.ToString() && HeightDifference > _currentSystem.MaxOutdoorAboveHeight)
                            {
                                double len = Unit.ConvertToControl(_currentSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, SystemSetting.UserSetting.unitsSetting.settingLENGTH);
                                ListMaxMinidiff += " " + roomindoor.IndoorName + "- " + len.ToString("n0") + SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                                MaxMincheck = true;
                            }
                            else if (SelectedIduPosition == PipingPositionType.Lower.ToString() && HeightDifference > _currentSystem.MaxOutdoorBelowHeight)
                            {
                                double len = Unit.ConvertToControl(_currentSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, SystemSetting.UserSetting.unitsSetting.settingLENGTH);
                                ListMaxMinidiff += " " + roomindoor.IndoorName + "- " + len.ToString("n0") + SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                                MaxMincheck = true;
                            }
                        }
                    }
                }
                if (MaxMincheck == true)
                {
                    JCMsg.ShowErrorOK(Msg.Piping_HeightDiffH(ListMaxMinidiff));
                    HeightDifference = 0;
                }
                else
                {
                    UpdateSelectedEquipmentValues("HeightDifference");
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void LostFocusHeatingDryBulbEvent()
        {
            try
            {
                BatchCalculateEstValue();
                UpdateSelectedEquipmentValues("HeatingDB");
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
                UpdateSelectedEquipmentValues("CoolingRH");
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
                BatchCalculateEstValue();
                //BatchCalculateAirFlow();
                UpdateSelectedEquipmentValues("CoolingWB");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void LostFocusCoolingDryBulbEvent()
        {
            try
            {
                DoCalculateByOptionInd("DB");
                BatchCalculateEstValue();
                //BatchCalculateAirFlow();
                UpdateSelectedEquipmentValues("CoolingDB");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void SelectionChangedSelectedFanSpeedEvent()
        {
            try
            {
                if (this.SelectedFanSpeed != null)
                {
                    BatchCalculateEstValue();
                    BatchCalculateAirFlow();
                    UpdateSelectedEquipmentValues("SelectedFanSpeed");
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void SelectionChangedSelectedIduPositionEvent()
        {
            try
            {
                if (this.SelectedIduPosition != null)
                    UpdateSelectedEquipmentValues("IduPosition");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void SelectionChangedSelectedRoomEvent()
        {
            try
            {
                if (this.SelectedRoom != null)
                {
                    CurrentFreshAirArea = JCHVRF.Model.Project.CurrentProject.FreshAirAreaList.FirstOrDefault(i => i.Id == this.SelectedRoom.FreshAirAreaId);
                    BindCapacityRequirements();
                    UpdateSelectedEquipmentValues("SelectedRoom");
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
                this.CR_ESP = Unit.ConvertToControl(SelectedRoom.StaticPressure, UnitType.STATICPRESSURE, PressureMasureUnit);//Convert.ToDouble(SelectedRoom.StaticPressure);
                this.IndoorUnitName = this.SelectedRoom.Name;
                this.IsIndoorUnitEditable = false;
            }
        }


        private void SelectionChangedSelectedFloorEvent()
        {
            try
            {
                if (this.SelectedFloor != null)
                    UpdateSelectedEquipmentValues("SelectedFloor");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void SelectionChangedSelectedModelEvent()
        {
            try
            {
                if (this.SelectedModel != null)
                {
                    BindIndoorImageToUI();
                    BatchCalculateEstValue();
                    BatchCalculateAirFlow();
                    UpdateSelectedEquipmentValues("SelectedModel");

                    if (!_currentSystem.IsManualPiping)
                    {
                        if (_currentSystem.MyPipingNodeOut != null)
                        {
                            _currentSystem.MyPipingNodeOut = null;
                        }
                    }
                    UtilTrace.SaveHistoryTraces();
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void SelectionChangedSelectedIndoorTypeEvent()
        {
            try
            {
                //_eventAggregator.GetEvent<IduTypeChangeSubscriber>().Publish(true);
                //AccessoriesCount = 0;
                if (this.SelectedIndoorType != null)
                {
                    //Utility.UtilTrace.SaveHistoryTraces();
                    BindIDuModelType();
                    UpdateSelectedEquipmentValues("SelectedIndoorType");
                    if (this.SelectedModel != null)
                        PublishImageData();
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }



        void LostFocusIndoorUnitNameEvent()
        {
            try
            {
                UpdateSelectedEquipmentValues("IndoorUnitName");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        protected void UpdateSelectedEquipmentValues(string propertyName)
        {
            bool Isvalid = IsValidate();
            if (Isvalid)
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
                    if (JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                    {

                        // ProjectBLL pBLL = new ProjectBLL(JCHVRF.Model.Project.CurrentProject);

                        //var itemIndex = JCHVRF.Model.Project.CurrentProject.RoomIndoorList.FindIndex(x => x.SystemID == systemID && x.IndoorNO == objIndoor.IndoorNO);
                        var itemIndex = JCHVRF.Model.Project.CurrentProject.RoomIndoorList.FindIndex(x => x.IndoorNO == objIndoor.IndoorNO);
                        if (itemIndex >= 0)
                        {
                            switch (propertyName)
                            {
                                case "SelectedIndoorType":
                                    
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorItem.Type = SelectedIndoorType;
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DisplayImageName = SelectedIndoorType;

                                    break;
                                case "SelectedFanSpeed":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].FanSpeedLevel = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
                                    break;
                                case "SelectedRoom":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].SelectedRoom = SelectedRoom;
                                    break;
                                case "SelectedFloor":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].SelectedFloor = ListFloor.FirstOrDefault(MM => MM.Id == SelectedFloor);
                                    break;
                                case "SelectedModel":
                                    //if (SelectedIndoorType != null && SelectedIndoorType == "2-Way Cassette High Efficiency")
                                    //{
                                    //    SelectedIndoorType = "Two Way Cassette";
                                    //}
                                    imgData.NodeNo = this.objIndoor.IndoorNO;
                                    imgData.equipmentType = "Indoor";
                                   // imgData.imageName = JCHVRF.Model.Project.CurrentProject.BrandCode == "H" ? this.objIndoor.IndoorItem.Model_Hitachi : (JCHVRF.Model.Project.CurrentProject.BrandCode == "Y"? this.objIndoor.IndoorItem.ModelFull : this.objIndoor.IndoorItem.Model_York);
                                   imgData.imageName = JCHVRF.Model.Project.CurrentProject.BrandCode == "H" ? this.objIndoor.IndoorItem.Model_Hitachi : this.objIndoor.IndoorItem.ModelFull;
                                    imgData.UniqName = this.objIndoor.IndoorName;
                                    imgData.imagePath = GetImagePath(SelectedModel);
                                    if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective)
                                    {
                                        imgData.coolingCapacity = this.objIndoor.ActualCoolingCapacity == 0 ? this.objIndoor.CoolingCapacity : this.objIndoor.ActualCoolingCapacity;
                                        imgData.heatingCapacity = this.objIndoor.ActualHeatingCapacity;
                                        imgData.sensibleHeat = this.objIndoor.ActualSensibleHeat == 0 ? this.objIndoor.SensibleHeat : this.objIndoor.ActualSensibleHeat;
                                    }
                                   else if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective)
                                    {
                                        imgData.coolingCapacity = this.objIndoor.ActualCoolingCapacity;
                                        imgData.heatingCapacity = this.objIndoor.ActualHeatingCapacity == 0 ? this.objIndoor.HeatingCapacity : this.objIndoor.ActualHeatingCapacity;
                                        imgData.sensibleHeat = this.objIndoor.ActualSensibleHeat == 0 ? this.objIndoor.SensibleHeat : this.objIndoor.ActualSensibleHeat;
                                    }
                                    else
                                    {
                                        imgData.coolingCapacity = this.objIndoor.ActualCoolingCapacity == 0 ? this.objIndoor.CoolingCapacity : this.objIndoor.ActualCoolingCapacity;
                                        imgData.heatingCapacity = this.objIndoor.ActualHeatingCapacity == 0 ? this.objIndoor.HeatingCapacity : this.objIndoor.ActualHeatingCapacity;
                                        imgData.sensibleHeat = this.objIndoor.ActualSensibleHeat == 0 ? this.objIndoor.SensibleHeat : this.objIndoor.ActualSensibleHeat;
                                    }


                                    // JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex]


                                    Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorItem = inItem;
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorItem.ModelFull = SelectedModel;
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DisplayImageName = SelectedIndoorType;
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DisplayImagePath = GetImagePath(SelectedModel);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].CoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.POWER, this.PowerUnit);//Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].HeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.POWER, this.PowerUnit);//Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.POWER, this.PowerUnit);// Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].RqAirflow = Convert.ToDouble(SR_AirFlow);


                                    _eventAggregator.GetEvent<SetIduPropertiesOnCanvas>().Publish(imgData);
                                    break;
                                case "IndoorUnitName":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorName = IndoorUnitName;
                                    break;
                                case "ManualSelection":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IsAuto = EnableManualSelection;
                                    break;
                                case "IduPosition":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].PositionType = SelectedIduPosition;
                                    break;
                                case "HeightDifference":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].HeightDiff = HeightDifference ?? 0;
                                    break;
                                case "UseRoomTemperature":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].WBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].RHCooling = RelativeHumidity ?? 0;
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBHeating = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    break;

                                case "CoolingDB":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    break;
                                case "CoolingWB":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].WBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    break;
                                case "CoolingRH":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].RHCooling = RelativeHumidity ?? 0;
                                    break;
                                case "HeatingDB":
                                    JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBHeating = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                                    break;

                            }
                            IDURatedData();
                            IDUCorrectedData(JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex]);
                            this.objIndoor = JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex];
                        }
                    }
                }
            }

        }


        private void PublishImageData()
        {
            imgData.NodeNo = this.objIndoor.IndoorNO;
            imgData.equipmentType = "Indoor";
            imgData.imageName = this.objIndoor.DisplayImageName;
            imgData.UniqName = this.objIndoor.IndoorName;
            imgData.imagePath = GetImagePath(SelectedModel);
            if (File.Exists(imgData.imagePath))
            {
                _eventAggregator.GetEvent<SetIduPropertiesOnCanvas>().Publish(imgData);
            }
            else
            {
                //JCHMessageBox.Show("Image Is not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //return;
            }

        }

        private string GetImagePath(string selectedModel)
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolderWithNewImage);
            string ImageUrl = string.Empty;
            DataRow dr = IDuModelTypeList.AsEnumerable().FirstOrDefault(r => r.Field<string>("ModelFull") == selectedModel);
            if (dr != null)
            {
                ImageUrl = sourceDir + "\\" + Convert.ToString(dr["TypeImage"]);
            }

            return ImageUrl;
        }

        private RoomIndoor objIndoor;
        public void OnShowIduProperties(RoomIndoor objIndoor)
        {
            this.objIndoor = objIndoor;
            this.IndoorUnitName = objIndoor?.IndoorName;
            BindOutdoorProperty();
            BindIDuUnitType();
            BindIDuModelType();

            BindIndoorImageToUI();
            BindingControlsFromRoomIndoor(objIndoor);
            BatchCalculateEstValue();
            BatchCalculateAirFlow();
            BindRoomIndoorListOnLoad(objIndoor);
            _accessoriesCount = objIndoor == null || objIndoor.ListAccessory == null ? 0 : objIndoor.ListAccessory.Count();
            UpdateSelectedEquipmentValues("SelectedModel");
            RaisePropertyChanged("AccessoriesCount");
        }
        void BindOutdoorProperty()
        {
            //var CurrentSys = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(sys => sys.IsActiveSystem == true);
            //var CurrentSys = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.NewSystem;
            //if (CurrentSys == null)
            //    CurrentSys = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.CurrentSystem;
            if (_currentSystem != null)
            {
                outdoorCoolingDB = _currentSystem.DBCooling;
                outdoorHeatingWB = _currentSystem.WBHeating;
                outdoorCoolingIW = _currentSystem.IWCooling;
                outdoorHeatingIW = _currentSystem.IWHeating;
                ODUSeries = _currentSystem.Series;
                ODUProductType = _currentSystem.ProductType;

                //Start Backward Compatibility : Added a null/empty check to avoid Null reference exception.
                if (string.IsNullOrEmpty(ODUProductType))
                {
                    ODUProductType = _currentSystem.OutdoorItem?.ProductType;
                }
                //End Backward Compatibility : Added a null/empty check to avoid Null reference exception.

            }

        }


        private void BindingControlsFromRoomIndoor(RoomIndoor Item)
        {
            try
            {
                this.IsIndoorUnitEditable = true;

                if (Item.SelectedRoom != null)
                    this.SelectedRoom = Item.SelectedRoom;
                else
                    this.SelectedRoom = this.ListRoom.FirstOrDefault() != null ? this.ListRoom.FirstOrDefault() : null;
                if (Item.SelectedFloor != null)
                    this.SelectedFloor = Item.SelectedFloor.Id;
                else
                    this.SelectedFloor = this.ListFloor.FirstOrDefault().Id;
                this.ManualSelection = !Item.IsAuto;
                if (Item.IndoorItem != null && Item.IndoorItem.ModelFull != null)
                {
                    //this.SelectedIndoorType = Item.SelectedUnitType;
                    this.SelectedModel = Item.IndoorItem.ModelFull;
                }
                this.CR_TotalCapacity = Unit.ConvertToControl(Convert.ToDouble(Item.RqCoolingCapacity), UnitType.POWER, utPower);
                this.CR_SensibleCapacity = Unit.ConvertToControl(Convert.ToDouble(Item.RqSensibleHeat), UnitType.POWER, utPower);
                this.CR_HeatingCapacity = Unit.ConvertToControl(Convert.ToDouble(Item.RqHeatingCapacity), UnitType.POWER, utPower);
                this.CR_AirFlow = Unit.ConvertToControl(Convert.ToDouble(Item.RqAirflow), UnitType.AIRFLOW, utAirflow);
                this.CR_FreshAir = Unit.ConvertToControl(Convert.ToDouble(Item.RqFreshAir), UnitType.AIRFLOW, utAirflow);
                this.CR_ESP = Unit.ConvertToControl(Convert.ToDouble(Item.RqStaticPressure), UnitType.STATICPRESSURE, PressureMasureUnit);//Item.RqStaticPressure;

                if (Convert.ToDouble(Item.DBCooling) != 0 && Convert.ToDouble(Item.WBCooling) != 0 && Convert.ToDouble(Item.DBHeating) != 0)
                {
                    this.CoolingDryBulb = Unit.ConvertToControl(Convert.ToDouble(Item.DBCooling), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    this.CoolingWetBulb = Unit.ConvertToControl(Convert.ToDouble(Item.WBCooling), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    this.HeatingDryBulb = Unit.ConvertToControl(Convert.ToDouble(Item.DBHeating), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                    this.RelativeHumidity = Item.RHCooling;
                }
                else
                {
                    BindInternalDesignConditions();
                }
                this.HeightDifference = Item.HeightDiff;
                this.SelectedIduPosition = Item.PositionType;

                this.SelectedFanSpeed = (string)Enum.GetName(typeof(JCHVRF_New.Model.FanSpeed), Item.FanSpeedLevel);
                EnableManualSelection = Item.IsAuto;
                EnableManualselectionIduModel = Item.IsAuto;
            }
            catch { }
        }
        protected void BindRoomIndoorListOnLoad(RoomIndoor roomIndoor)
        {
            if (roomIndoor != null)
            {
                if (JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                {
                    var itemIndex = JCHVRF.Model.Project.CurrentProject.RoomIndoorList.FindIndex(x => x.IndoorNO == roomIndoor.IndoorNO);
                    if (itemIndex >= 0)
                    {
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorItem.Type = SelectedIndoorType;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DisplayImageName = SelectedIndoorType;
                        //Start Backward Compatibility : Added a null/empty check in order to avoid Null reference exception.
                        if (!string.IsNullOrEmpty(this.SelectedFanSpeed))
                        {
                            JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].FanSpeedLevel = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
                        }
                        else
                        {
                            JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].FanSpeedLevel = (int)JCHVRF_New.Model.FanSpeed.High;
                        }
                        //End Backward Compatibility : Added a null/empty check in order to avoid Null reference exception.
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].SelectedRoom = SelectedRoom;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].SelectedFloor = ListFloor.FirstOrDefault(MM => MM.Id == SelectedFloor);
                        if (IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel) != null)
                        {
                            Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
                            JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorItem = inItem;
                            JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorItem.ModelFull = SelectedModel;
                        }

                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DisplayImageName = SelectedIndoorType;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].CoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.POWER, this.PowerUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].HeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.POWER, this.PowerUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.POWER, this.PowerUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].RqAirflow = Convert.ToDouble(SR_AirFlow);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].IndoorName = IndoorUnitName;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].PositionType = SelectedIduPosition;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].HeightDiff = HeightDifference ?? 0;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].WBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].RHCooling = RelativeHumidity ?? 0;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBHeating = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].WBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].RHCooling = RelativeHumidity ?? 0;
                        JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex].DBHeating = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
                        IDUCorrectedData(JCHVRF.Model.Project.CurrentProject.RoomIndoorList[itemIndex]);
                    }
                }
            }
        }

        private void OpenAddRoomWindowOnClick()
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

        private void OpenAddFloorWindowOnClick()
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
            try
            {
                BindFloorList();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OpenGetRoomList()
        {
            try
            {
                GetRoomList();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        #region View_Model_Property
        public int IndoorNo { get; set; }

        private string _indoorUnitName;
        public string IndoorUnitName
        {
            get { return _indoorUnitName; }
            set
            {
                this.SetValue(ref _indoorUnitName, value);
                //if (IndoorUnitName != null)
                //    UpdateSelectedEquipmentValues("IndoorUnitName");
            }
        }

        private int _accessoriesCount = 0;
        public int AccessoriesCount
        {
            get
            {

                return objIndoor == null || objIndoor.ListAccessory == null ? 0 : objIndoor.ListAccessory.Where(a => a.IsSelect).Count();

            }

        }


        #region Indoor Prorperties

        private ObservableCollection<ComboBox> _listIndoorType;
        public ObservableCollection<ComboBox> ListIndoorType
        {
            get
            {
                return _listIndoorType;
            }
            set
            {
                this.SetValue(ref _listIndoorType, value);
            }
        }

        private string _selectedIndoorType;
        public string SelectedIndoorType
        {
            get { return _selectedIndoorType; }
            set
            {
                this.SetValue(ref _selectedIndoorType, value);
                if (SelectedIndoorType != null)
                {
                    //BindIDuModelType();
                    //UpdateSelectedEquipmentValues("SelectedIndoorType");
                }

            }
        }


        private string _indoorError;
        public string IndoorError
        {
            get { return _indoorError; }
            set { this.SetValue(ref _indoorError, value); }
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

        private Room _selectedRoom;
        public Room SelectedRoom
        {
            get { return _selectedRoom; }
            set
            {
                this.SetValue(ref _selectedRoom, value);
                //if (SelectedRoom != null)
                // UpdateSelectedEquipmentValues("SelectedRoom");
                //CurrentFreshAirArea = JCHVRF.Model.Project.CurrentProject.FreshAirAreaList.FirstOrDefault(i => i.Id == _SelectedRoom.FreshAirAreaId);
                //BindCapacityRequirements();
            }
        }

        private ObservableCollection<JCHVRF.Model.Floor> _listFloor;
        public ObservableCollection<JCHVRF.Model.Floor> ListFloor
        {
            get
            {
                return _listFloor;
            }
            set
            {
                this.SetValue(ref _listFloor, value);
            }
        }

        private string _selectedFloor;
        public string SelectedFloor
        {
            get { return _selectedFloor; }
            set
            {
                this.SetValue(ref _selectedFloor, value);
                //if (SelectedFloor != null)
                // UpdateSelectedEquipmentValues("SelectedFloor");

            }
        }

        private string _iduImagePath;
        public string IduImagePath
        {
            get { return _iduImagePath; }
            set { this.SetValue(ref _iduImagePath, value); }
        }

        public List<string> IduPosition
        {
            get
            {
                return Enum.GetNames(typeof(PipingPositionType)).ToList();
            }
        }

        private string _selectedIduPosition;
        public string SelectedIduPosition
        {
            get { return _selectedIduPosition; }
            set
            {
                this.SetValue(ref _selectedIduPosition, value);
                if (SelectedIduPosition != null)
                {
                    if (SelectedIduPosition == PipingPositionType.SameLevel.ToString())
                        this.HeightDifference = 0;
                    //UpdateSelectedEquipmentValues("IduPosition");
                }
            }
        }

        private double? _heightDifference;
        public double? HeightDifference
        {
            get
            {
                return _heightDifference;
            }
            set
            {
                this.SetValue(ref _heightDifference, value);
                //if (HeightDifference != null)
                //{
                //    UpdateSelectedEquipmentValues("HeightDifference");
                //}
            }
        }

        private string _heightDifferenceError;
        public string HeightDifferenceError
        {
            get
            {
                return _heightDifferenceError;
            }
            set
            {
                this.SetValue(ref _heightDifferenceError, value);
            }
        }

        public List<string> FanSpeed
        {
            get
            {
                return Enum.GetNames(typeof(FanSpeed)).ToList(); ;
            }

        }

        private string _selectedFanSpeed;
        public string SelectedFanSpeed
        {
            get { return _selectedFanSpeed; }
            set
            {
                this.SetValue(ref _selectedFanSpeed, value);
                if (SelectedFanSpeed != null)
                {
                    //BatchCalculateEstValue();
                    //BatchCalculateAirFlow();
                    //UpdateSelectedEquipmentValues("SelectedFanSpeed");
                }
            }
        }


        private bool _manualSelection;
        public bool ManualSelection
        {
            get { return _manualSelection; }
            set
            {
                this.SetValue(ref _manualSelection, value);
            }
        }

        private string _unitTypeError;
        public string UnitTypeError
        {
            get { return _unitTypeError; }
            set
            {
                this.SetValue(ref _unitTypeError, value);
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

        private string _listModelError;
        public string ListModelError
        {
            get { return _listModelError; }
            set
            {
                this.SetValue(ref _listModelError, value);
            }
        }

        private string _selectedModel;
        public string SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                this.SetValue(ref _selectedModel, value);
                if (SelectedModel != null)
                {

                    //BindIndoorImageToUI();
                    //BatchCalculateEstValue();
                    //BatchCalculateAirFlow();
                    //UpdateSelectedEquipmentValues("SelectedModel");
                }
            }
        }

        #endregion

        #region Internal_Design_Conditions_Property

        private string _changeToFandC;
        public string ChangeToFandC
        {
            get { return _changeToFandC; }
            set { this.SetValue(ref _changeToFandC, value); }
        }


        private bool _UseRoomTemperature;
        public bool UseRoomTemperature
        {
            get { return _UseRoomTemperature; }
            set
            {
                this.SetValue(ref _UseRoomTemperature, value);
                //if (UseRoomTemperature != null)
                //{
                //UpdateSelectedEquipmentValues("UseRoomTemperatue");
                // }
            }
        }

        private double? _coolingDryBulb;
        public double? CoolingDryBulb
        {
            get
            {
                return _coolingDryBulb;
            }
            set
            {
                this.SetValue(ref _coolingDryBulb, value);
                //if (_coolingDryBulb != null)
                //    UpdateSelectedEquipmentValues("CoolingDB");
            }
        }

        private string _coolingDryBulbError;
        public string CoolingDryBulbError
        {
            get { return _coolingDryBulbError; }
            set
            {
                this.SetValue(ref _coolingDryBulbError, value);
            }
        }


        private double? _coolingWetBulb;
        public double? CoolingWetBulb
        {
            get
            {
                return _coolingWetBulb;
            }
            set
            {
                this.SetValue(ref _coolingWetBulb, value);
                //if (CoolingWetBulb != null)
                //{
                //    UpdateSelectedEquipmentValues("CoolingWB");
                //}

            }
        }

        private string _coolingWetBulbError;
        public string CoolingWetBulbError
        {
            get { return _coolingWetBulbError; }
            set
            {
                this.SetValue(ref _coolingWetBulbError, value);
            }
        }

        private double? _heatingDryBulb;
        public double? HeatingDryBulb
        {
            get
            {
                return _heatingDryBulb;
            }
            set
            {
                this.SetValue(ref _heatingDryBulb, value);
                //if (HeatingDryBulb != null)
                //UpdateSelectedEquipmentValues("HeatingDB");
            }
        }

        private string _heatingDryBulbError;
        public string HeatingDryBulbError
        {
            get { return _heatingDryBulbError; }
            set
            {
                this.SetValue(ref _heatingDryBulbError, value);
            }
        }

        private double? _relativeHumidity;
        public double? RelativeHumidity
        {
            get
            {
                return _relativeHumidity;
            }
            set
            {
                this.SetValue(ref _relativeHumidity, value);
                //if (RelativeHumidity != null)
                //UpdateSelectedEquipmentValues("CoolingRH");
            }
        }

        private string _relativeHumidityError;
        public string RelativeHumidityError
        {
            get { return _relativeHumidityError; }
            set
            {
                this.SetValue(ref _relativeHumidityError, value);
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
            if (String.IsNullOrEmpty(this.IndoorUnitName))
            {
                this.IndoorError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else if (this.IndoorUnitName.HasSpecialCharacters())
            {

                this.IndoorError = Langauge.Current.GetMessage("ERROR_SPECIAL_CHARACTERS");//"Special characters not allowed"
                result = false;
            }
            else if (this.IndoorUnitName.Length > 30)
            {
                this.IndoorError = Langauge.Current.GetMessage("ERROR_IDU_EXCEEDING_CHAR");//Name exceeding 30 characters
                result = false;
            }
            else this.IndoorError = string.Empty;

            if (this.HeightDifference == null)
            {
                this.HeightDifferenceError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else this.HeightDifferenceError = string.Empty;


            if (String.IsNullOrEmpty(this.SelectedIndoorType))
            {
                this.UnitTypeError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else this.UnitTypeError = string.Empty;

            if (String.IsNullOrEmpty(this.SelectedModel))
            {
                this.ListModelError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else this.ListModelError = string.Empty;


            if (this.CoolingDryBulb == null)
            {
                this.CoolingDryBulbError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else if (this.CoolingDryBulb < DBCoolJCMinValue || this.CoolingDryBulb > DBCoolJCMaxValue)
            {
                this.CoolingDryBulbError = "Range[" + Convert.ToString(DBCoolJCMinValue) + "," + Convert.ToString(DBCoolJCMaxValue) + "] *";
                result = false;
            }
            else this.CoolingDryBulbError = string.Empty;


            if (this.CoolingWetBulb == null)
            {
                this.CoolingWetBulbError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else if (this.CoolingWetBulb < WBCoolJCMinValue || this.CoolingWetBulb > WBCoolJCMaxValue)
            {
                this.CoolingWetBulbError = "Range[" + Convert.ToString(WBCoolJCMinValue) + "," + Convert.ToString(WBCoolJCMaxValue) + "] *"; //"Range [14,24] *";
                result = false;
            }
            else this.CoolingWetBulbError = string.Empty;


            if (this.HeatingDryBulb == null)
            {
                this.HeatingDryBulbError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else if (this.HeatingDryBulb < DBHeatJCMinValue || this.HeatingDryBulb > DBHeatJCMaxValue)

            {
                AutoSelectionMessage = Langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(DBHeatJCMinValue) + ", " + Convert.ToString(DBHeatJCMaxValue) + "] * ";//"Range[16, 24] * ";";
                this.HeatingDryBulbError = Langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(DBHeatJCMinValue) + ", " + Convert.ToString(DBHeatJCMaxValue) + "] * ";//"Range[16,24] *";
                result = false;
            }
            else this.HeatingDryBulbError = string.Empty;


            if (this.RelativeHumidity == null)
            {
                this.RelativeHumidityError = Langauge.Current.GetMessage("REQUIRED");//"Required *"
                result = false;
            }
            else if (this.RelativeHumidity < RHJCMinValue || this.RelativeHumidity > RHJCMaxValue)
            {
                this.RelativeHumidityError = Langauge.Current.GetMessage("RANGE") + "[" + Convert.ToString(RHJCMinValue) + "," + Convert.ToString(RHJCMaxValue) + "] *"; // "Range[13,100] *";
                result = false;
            }
            else this.RelativeHumidityError = string.Empty;

            if (this.CR_TotalCapacity < this.CR_SensibleCapacity)
            {
                MessageBox.Show(Langauge.Current.GetMessage("ERROR_IDU_VALIDATION"));//"[Sensible Heat] must be less than [Capacity]"
                result = false;
            }

            return result;
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



        #endregion

        #endregion
        #region BindingData
        private int GetIndoorCount()
        {
            int InNodeCount = 0;
            if (JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList != null)
                foreach (RoomIndoor ri in JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList)
                {
                    InNodeCount = InNodeCount > ri.IndoorNO ? InNodeCount : ri.IndoorNO;
                }

            return InNodeCount + 1;
        }

        public ObservableCollection<Room> GetRoomList()
        {
            ListRoom = new ObservableCollection<Room>();
            //Start Backward Compatibility : Added a null check in order to avoid Null reference exception.
            if (JCHVRF.Model.Project.GetProjectInstance.RoomList != null)
            {
                ListRoom = new ObservableCollection<Room>(JCHVRF.Model.Project.GetProjectInstance.RoomList);
                if (this.SelectedRoom == null)
                {
                    this.SelectedRoom = this.ListRoom.FirstOrDefault() != null ? this.ListRoom.FirstOrDefault() : null;
                }
            }
            //End Backward Compatibility : Added a null check in order to avoid Null reference exception.
            return ListRoom;
        }

        private void BindFloorList()
        {
            this.ListFloor = new ObservableCollection<JCHVRF.Model.Floor>(JCHVRF.Model.Project.GetProjectInstance.FloorList);
            if (this.ListFloor != null && this.ListFloor.Count > 0 && this.SelectedFloor != null)
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
            ListIndoorType = new ObservableCollection<ComboBox>();
            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                //var IndoorTypeList = bll.GetIndoorDisplayName();
                var IndoorTypeList = bll.GetIndoorDisplayNameForODUSeries(ODUSeries);
                foreach (DataRow rowView in IndoorTypeList.Rows)
                {
                    if (Convert.ToString(rowView["Display_Name"]) != "Total Heat Exchanger (KPI-E4E)")
                        ListIndoorType.Add(new ComboBox { DisplayName = Convert.ToString(rowView["Display_Name"]), Value = Convert.ToString(rowView["Display_Name"]) });
                }
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
                    ListIndoorType.Add(new ComboBox { DisplayName = Convert.ToString(rowView.Row["UnitType"]), Value = Convert.ToString(rowView.Row["UnitType"]) });
                }

            }

            //Start Backward Compatibility :
            //Code Refactoring - taken out the common block of code from if-else.
            if (ListIndoorType.Count > 0)
            {
                //In case of indoor type is missing in objIndoor then try to take it from IndoorItem of objIndoor.
                if (string.IsNullOrEmpty(objIndoor.DisplayImageName))
                {
                    objIndoor.DisplayImageName = objIndoor.IndoorItem.Type;//.Split('.')[0];
                }
                //changed the string comparision to incase-sensitive.
                string selectedIndoorType = ListIndoorType.FirstOrDefault(s => s.DisplayName.Equals(objIndoor.DisplayImageName, StringComparison.OrdinalIgnoreCase))?.Value;
                if (selectedIndoorType != null)
                {
                    this.SelectedIndoorType = selectedIndoorType;
                }
            }
            //End Backward Compatibility : Added a null/empty check  to avoid Null reference exception.

        }
        private void UpdateUnitType()
        {
            if (this.SelectedIndoorType != null)
            {
                int i = this.SelectedIndoorType.IndexOf("-");
                if (i > 0)
                {
                    _factory = this.SelectedIndoorType.Substring(i + 1, this.SelectedIndoorType.Length - i - 1);
                    _type = this.SelectedIndoorType.Substring(0, i);
                }
                else
                {
                    _factory = "";
                    _type = this.SelectedIndoorType;
                }
            }
        }
        private void UpdateUnitTypeUniversal()
        {
            _factory = bll.GetFCodeByDisUnitType(this.SelectedIndoorType, out _typeList);
        }
        private void BindIDuModelType()
        {
            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                UpdateUnitTypeUniversal();
                IDuModelTypeList = bll.GetUniversalIndoorListStd(this.SelectedIndoorType, _factory, _typeList);
                IDuModelTypeList.DefaultView.Sort = "CoolCapacity";
                /* await Task.Run(() =>*/
                BindCompatibleIndoorUniversal();//);
            }
            else
            {
                UpdateUnitType();
                IDuModelTypeList = bll.GetIndoorListStd(_type, ODUProductType, _factory);
                IDuModelTypeList.DefaultView.Sort = "CoolCapacity";
                /*await Task.Run(() =>*/
                BindCompatibleIndoorSimple();//);
            }

        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                RoomIndoor objIndoor = navigationContext.Parameters["IndoorItem"] as RoomIndoor;
                _currentSystem = navigationContext.Parameters["CurrentSystem"] as NextGenModel.SystemVRF;
                if (_currentSystem != null && (Project.GetProjectInstance.IsHeatingModeEffective || (Project.GetProjectInstance.IsCoolingModeEffective && Project.GetProjectInstance.IsHeatingModeEffective)))
                {
                    IsHeatingOnlyMode = true;
                }
                else
                    IsHeatingOnlyMode = false;

                if (_currentSystem != null && (Project.GetProjectInstance.IsCoolingModeEffective || (Project.GetProjectInstance.IsCoolingModeEffective && Project.GetProjectInstance.IsHeatingModeEffective)))
                {
                    IsCoolingOnlyMode = true;
                }
                else
                    IsCoolingOnlyMode = false;
                if (_currentSystem.IsAuto)
                {
                    this.EnableManualSelection = false;
                    this.EnableManualselectionIduModel = false;
                }
                else
                {
                    this.EnableManualSelection = true;
                    this.EnableManualselectionIduModel = true;
                }
                if (objIndoor != null)
                    OnShowIduProperties(objIndoor);

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void BindIndoorImageToUI()
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolderWithLegacyImage);
            if (SelectedModel != null)
            {
                DataRow dr = IDuModelTypeList.AsEnumerable().FirstOrDefault(r => r.Field<string>("ModelFull") == SelectedModel);
                if (dr != null)
                {
                    this.IduImagePath = sourceDir + "\\" + Convert.ToString(dr["TypeImage"]);
                }
            }
        }

        private void BindCompatibleIndoorUniversal()
        {
            try
            {
                ListModel = new ObservableCollection<ComboBox>();
                List<ComboBox> ListModelTemp = new List<ComboBox>();
                //ODUSeries = "Commercial VRF HP, FSXNSE";
                IDUIndoorList = new List<Indoor>();
                MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
                DataTable typeDt = productTypeBll.GetIduTypeBySeries(JCHVRF.Model.Project.GetProjectInstance.BrandCode, JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, ODUSeries);
                foreach (DataRow drIduModel in IDuModelTypeList.Rows)
                {
                    var Indoor = bll.GetItem(drIduModel["ModelFull"].ToString(), _type, ODUProductTypeUniversal, ODUSeries);
                    IDUIndoorList.Add(Indoor);
                    if (typeDt != null)
                        foreach (DataRow dr in typeDt.Rows)
                        {
                            if (Indoor.Type == dr["IDU_UnitType"].ToString())
                            {
                                var modelHitachi = dr["IDU_Model_Hitachi"].ToString();
                                if (string.IsNullOrEmpty(modelHitachi) || modelHitachi.Contains(";" + Convert.ToString(drIduModel["Model_Hitachi"]) + ";"))
                                {
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
                    this.SelectedModel = ListModel.FirstOrDefault().Value;
                }
            }

            catch (Exception ex) { }


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
                    IDUIndoorList.Add(Indoor);
                    if (JCHVRF.Model.Project.GetProjectInstance.BrandCode == "Y")
                        ListModelTemp.Add(new ComboBox { DisplayName = Convert.ToString(drIduModel["Model_York"]), Value = Convert.ToString(drIduModel["ModelFull"]) });
                    else
                        ListModelTemp.Add(new ComboBox { DisplayName = Convert.ToString(drIduModel["Model_Hitachi"]), Value = Convert.ToString(drIduModel["ModelFull"]) });

                }
                if (ListModelTemp != null && ListModelTemp.Count > 0)
                {
                    ListModel = new ObservableCollection<ComboBox>(ListModelTemp);
                    this.SelectedModel = ListModel.FirstOrDefault().Value;
                }
            }

            catch (Exception ex) { }

        }

        #endregion

        #region Methods
        private void OnAddAccessoryCommandClicked()
        {
            try
            {
                var RoomIndoor = JCHVRF.Model.Project.CurrentProject.RoomIndoorList.FirstOrDefault(x => x.IndoorNO == objIndoor.IndoorNO);

                NavigationParameters param = new NavigationParameters();
                param.Add("IndoorObject", RoomIndoor);
                _winService.ShowView(ViewKeys.AddAccessories, "Accessories", param, true, 950, 580);

                RaisePropertyChanged("AccessoriesCount");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        #endregion

        //public DelegateCommand AddAccessoriesCommand { get; set; }

        #region DesignCondition
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
            get { return _displayCurrentTempUnit; }
            set
            {
                this.SetValue(ref _displayCurrentTempUnit, value);
            }
        }
        private string _designConditionTempMasureUnite;
        public string DesignConditionTempMasureUnit
        {
            get { return _designConditionTempMasureUnite; }
            set { this.SetValue(ref _designConditionTempMasureUnite, value); }
        }
        #region Selected_Capacity_Property
        private string _SR_TotalCapacity;
        public string SR_TotalCapacity
        {
            get
            {
                return string.IsNullOrWhiteSpace(_SR_TotalCapacity) ? "0.0" : _SR_TotalCapacity;
            }
            set
            {
                this.SetValue(ref _SR_TotalCapacity, value);
                //DoAutoSelect();
            }
        }

        private string _SR_SensibleCapacity;
        public string SR_SensibleCapacity
        {
            get
            {
                return string.IsNullOrWhiteSpace(_SR_SensibleCapacity) ? "0.0" : _SR_SensibleCapacity;
            }
            set
            {
                this.SetValue(ref _SR_SensibleCapacity, value);
                //DoAutoSelect();
            }
        }

        private string _SR_HeatingCapacity;
        public string SR_HeatingCapacity
        {
            get
            {
                return string.IsNullOrWhiteSpace(_SR_HeatingCapacity) ? "0.0" : _SR_HeatingCapacity;
            }
            set
            {
                this.SetValue(ref _SR_HeatingCapacity, value);
                //DoAutoSelect();
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
                //DoAutoSelect();
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
                //DoAutoSelect();
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
                //DoAutoSelect();
            }
        }

        #endregion
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

        //private double _tot_indcap_c_rat;

        //public double tot_indcap_c_rat
        //{
        //    get { return _tot_indcap_c_rat; }
        //    set { _tot_indcap_c_rat = value; }
        //}
        //private double _tot_indcap_h_rat;

        //public double tot_indcap_h_rat
        //{
        //    get { return _tot_indcap_c_rat; }
        //    set { _tot_indcap_c_rat = value; }
        //}


        private void BatchCalculateEstValue()
        {
            double wb_c, db_h;
            //Start Backward Compatibility : Added a null/empty check in order to avoid Null reference exception.
            var FanSpeed = 1;
            if (!string.IsNullOrEmpty(this.SelectedFanSpeed))
            {
                FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
            }
            //End Backward Compatibility : Added a null/empty check in order to avoid Null reference exception.
            var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
            wb_c = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            db_h = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            if (this.SelectedModel != null)
            {

                IDURatedData();

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
            //Start Backward Compatibility : Added a null/empty check to avoid Null reference exception.
            var FanSpeed = 1;
            if (!string.IsNullOrEmpty(this.SelectedFanSpeed))
            {
                FanSpeed = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
            }
            //End Backward Compatibility : Added a null/empty check to avoid Null reference exception.
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

                        this.SR_AirFlow =airflow.ToString();
                        this.SR_ESP = Unit.ConvertToControl(staticPressure, UnitType.STATICPRESSURE, PressureMasureUnit).ToString("n2");//staticPressure.ToString("n0");
                        this.SR_FreshAir = Unit.ConvertToControl(fa, UnitType.AIRFLOW, utAirflow).ToString("n0");

                    }
                }
            }

        }
        void ChangeDesignTempOnClick()
        {
            string currentTempUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            if (this.DesignConditionTempMasureUnit == Unit.ut_Temperature_c)
            {
                this.CoolingDryBulb = Unit.ConvertToControl(this.CoolingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                this.CoolingWetBulb = Unit.ConvertToControl(this.CoolingWetBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                this.HeatingDryBulb = Unit.ConvertToControl(this.HeatingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                this.DesignConditionTempMasureUnit = Unit.ut_Temperature_f;
                this.DisplayCurrentTempUnit = Unit.ut_Temperature_c;
                this.ChangeToFandC = "Change To °C";
            }
            else
            {
                this.CoolingDryBulb = Unit.ConvertToSource(this.CoolingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                this.CoolingWetBulb = Unit.ConvertToSource(this.CoolingWetBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                this.HeatingDryBulb = Unit.ConvertToSource(this.HeatingDryBulb ?? 0, UnitType.TEMPERATURE, Unit.ut_Temperature_f);
                this.DesignConditionTempMasureUnit = Unit.ut_Temperature_c;
                this.DisplayCurrentTempUnit = Unit.ut_Temperature_f;
                this.ChangeToFandC = "Change To °F";
            }
        }
        private void BindInternalDesignConditions()
        {
            if (this.UseRoomTemperature == false)
            {
                var InternalDesignConditions = JCHVRF.Model.Project.GetProjectInstance.DesignCondition;
                //Start Backward Compatibility : Added a null check to avoid Null reference exception.
                if (JCHVRF.Model.Project.GetProjectInstance.DesignCondition == null)
                {
                    InternalDesignConditions = new JCHVRF.Model.New.DesignCondition();
                }
                //End Backward Compatibility : Added a null check to avoid Null reference exception.
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
        private void UseRoomTemperatueCheckedEvent()
        {
            try
            {
                BindInternalDesignConditions();
                UpdateSelectedEquipmentValues("UseRoomTemperature");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void UseRoomTemperatueUnCheckedEvent()
        {
            try
            {
                BindInternalDesignConditions();
                UpdateSelectedEquipmentValues("UseRoomTemperature");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void ChangeTempEvent()
        {
            try
            {
                ChangeDesignTempOnClick();
                UpdateSelectedEquipmentValues("UseRoomTemperature");
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private string _RatedCoooling;
        public string RatedCooling
        {
            get { return string.IsNullOrWhiteSpace(_RatedCoooling) ? "0.0" : _RatedCoooling; }
            set
            {
                SetValue<string>(ref _RatedCoooling, value);
            }
        }

        private string _CorrectedCooling;
        public string CorrectedCooling
        {
            get { return string.IsNullOrWhiteSpace(_CorrectedCooling) ? "0.0" : _CorrectedCooling; }
            set
            {
                SetValue<string>(ref _CorrectedCooling, value);
            }
        }

        private string _RatedSensible;
        public string RatedSensible
        {
            get { return string.IsNullOrWhiteSpace(_RatedSensible) ? "0.0" : _RatedSensible; }
            set
            {
                SetValue<string>(ref _RatedSensible, value);
            }
        }

        private string _CorrectedSensible;
        public string CorrectedSensible
        {
            get { return string.IsNullOrWhiteSpace(_CorrectedSensible) ? "0.0" : _CorrectedSensible; }
            set
            {
                SetValue<string>(ref _CorrectedSensible, value);
            }
        }

        private string _RatedHeating;
        public string RatedHeating
        {
            get { return string.IsNullOrWhiteSpace(_RatedHeating) ? "0.0" : _RatedHeating; }
            set
            {
                SetValue<string>(ref _RatedHeating, value);
            }
        }
        private string _CorrectedHeating;
        public string CorrectedHeating
        {
            get { return string.IsNullOrWhiteSpace(_CorrectedHeating) ? "0.0" : _CorrectedHeating; }
            set
            {
                SetValue<string>(ref _CorrectedHeating, value);
            }
        }
        private void IDURatedData()
        {
            double tot_indcap_c_rat = 0;
            double tot_indcap_h_rat = 0;
            double tot_indcap_s_rat = 0;
            Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
            //foreach (RoomIndoor ri in JCHVRF.Model.Project.CurrentProject.RoomIndoorList)
            //{
            //    tot_indcap_c_rat += ri.IndoorItem.CoolingCapacity;
            //    tot_indcap_h_rat += ri.IndoorItem.HeatingCapacity;
            //    tot_indcap_s_rat += ri.IndoorItem.SensibleHeat;
            //    //if (ri.IndoorItem.Flag == IndoorType.FreshAir)
            //    //{
            //    //    tot_FAcap += ri.IndoorItem.CoolingCapacity;
            //    //}
            //}
            if (inItem != null)
            {
                tot_indcap_c_rat = inItem.CoolingCapacity;
                tot_indcap_h_rat = inItem.HeatingCapacity;
                tot_indcap_s_rat = inItem.SensibleHeat;
                RatedCooling = Unit.ConvertToControl(tot_indcap_c_rat, UnitType.POWER, utPower).ToString("n2");//Rate Cool
                RatedHeating = Unit.ConvertToControl(tot_indcap_h_rat, UnitType.POWER, utPower).ToString("n2");//Rate  Heat
                RatedSensible = Unit.ConvertToControl(tot_indcap_s_rat, UnitType.POWER, utPower).ToString("n2");//Rate Sensible Heat
            }
        }
        private void IDUCorrectedData(RoomIndoor roomIndoor)
        {
            if (roomIndoor != null)
            {
                CorrectedCooling = Unit.ConvertToControl(roomIndoor.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n2");
                CorrectedHeating = Unit.ConvertToControl(roomIndoor.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n2");
                CorrectedSensible = Unit.ConvertToControl(roomIndoor.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n2");
            }
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
                        DoAutoSelectUniversalWithRoom(IDUIndoorList, tmpRoom, CurrentFreshAirArea, SelectedIndoorType);
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
                        DoAutoSelectWithoutRoom(SelectedIndoorType, ODUSeries, IDUIndoorList, CR_TotalCapacity.GetValueOrDefault(), CR_HeatingCapacity.GetValueOrDefault(), CR_SensibleCapacity.GetValueOrDefault(), CR_AirFlow.GetValueOrDefault(), CR_ESP.GetValueOrDefault(), CR_FreshAir.GetValueOrDefault());
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
            var rq_StaticPressure = Convert.ToDouble(inputRoomStaticPressure);
            var rq_fa = Unit.ConvertToSource(Convert.ToDouble(inputFA), UnitType.AIRFLOW, AirFlowMasureUnit);

            bool isPass = false;
            var found = false;
            foreach (var r in srcIduList)
            {
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
                AutoSelectionMessage = Msg.IND_NOTMATCH;
            else
                AutoSelectionMessage = string.Empty;

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

            bool isOK = false;

            var found = false;
            foreach (var stdRow in srcIduList)
            {
                bool isFreshAir = IndoorBLL.IsFreshAirUnit(stdRow.Type);
                bool isPass = true;

                // unit conversion              
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.CoolingCapacity), JCBase.Utility.UnitType.POWER, CapacityMasureUnit);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.HeatingCapacity), JCBase.Utility.UnitType.POWER, CapacityMasureUnit);

                if (room != null)
                {
                    if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective && std_cool < room.RqCapacityCool)
                        isPass = false;

                    if (curSelectType == "Hydro Free-High temp.") isPass = true;

                    if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective && std_heat < room.RqCapacityHeat)
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
                AutoSelectionMessage = Msg.IND_NOTMATCH;
            else
                AutoSelectionMessage = string.Empty;

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
            var rq_StaticPressure = Convert.ToDouble(inputRoomStaticPressure);
            var rq_fa = Unit.ConvertToSource(Convert.ToDouble(inputFA), UnitType.AIRFLOW, AirFlowMasureUnit);

            bool pass = false;
            var found = false;

            foreach (var r in srcIduList)
            {
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
                AutoSelectionMessage = Msg.IND_NOTMATCH;
            else
                AutoSelectionMessage = string.Empty;

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
            var pass = true;
            var found = false;
            if (srcIduList == null || srcIduList.Count == 0)
                return;

            foreach (var idu in srcIduList)
            {

                var isFreshAir = IndoorBLL.IsFreshAirUnit(idu.Type);
                double stdCool = Unit.ConvertToSource(Convert.ToDouble(idu.CoolingCapacity), JCBase.Utility.UnitType.POWER, CapacityMasureUnit);
                double stdHeat = Unit.ConvertToSource(Convert.ToDouble(idu.HeatingCapacity), JCBase.Utility.UnitType.POWER, CapacityMasureUnit);

                if (room != null)
                {
                    if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective && stdCool < room.RqCapacityCool)
                    {
                        pass = false;
                    }

                    if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective && !oduSeries.Contains(", CO") && stdHeat < room.RqCapacityHeat)
                        pass = false;

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
                AutoSelectionMessage = Msg.IND_NOTMATCH;
            else
                AutoSelectionMessage = string.Empty;


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

            double est_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
            double staticPressure = Convert.ToDouble(stdRow.GetStaticPressure());

            if (isFreshAir)
            {
                airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
                // Compare estimated capacity to current demand
                if (airflow < room.FreshAir)
                    pass = false;
            }
            else
            {
                if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective)
                {
                    if (est_cool < room.RqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
                        pass = false;
                }

                if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective && !productType.Contains(", CO"))
                {
                    if (est_heat < room.RqCapacityHeat)
                        pass = false;
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

            double airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);

            if (airflow < area.FreshAir)
                pass = false;

            return pass;
        }

        private bool AutoCompareWithoutRoom(string type, string productType, Indoor r,
            double rq_fa, double rq_cool, double rq_sensiable, double rq_airflow, double rq_StaticPressure, double rq_heat)
        {
            bool pass = true;

            bool isFreshAir = type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation");


            double est_cool = Unit.ConvertToSource(Convert.ToDouble(r.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(r.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(r.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double airflow = Unit.ConvertToSource(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
            double staticPressure = r.GetStaticPressure();

            if (isFreshAir)
            {
                airflow = Unit.ConvertToSource(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
                if (airflow < rq_fa)
                    pass = false;
            }
            else
            {
                if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective)
                {
                    if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow || staticPressure < rq_StaticPressure)
                        pass = false;
                }

                if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective && !productType.Contains(", CO"))
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

            double est_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
            double staticPressure = Convert.ToDouble(stdRow.GetStaticPressure());

            if (isFreshAir)
            {
                airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);

                if (airflow < room.FreshAir)
                    pass = false;
            }
            else
            {
                if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective)
                {
                    if (est_cool < room.RqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
                        pass = false;
                }

                if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective)
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
            double est_cool = Unit.ConvertToSource(Convert.ToDouble(r.CoolingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(r.HeatingCapacity), UnitType.POWER, CapacityMasureUnit);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(r.SensibleHeat), UnitType.POWER, CapacityMasureUnit);
            double airflow = Unit.ConvertToSource(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
            double staticPressure = Convert.ToDouble(r.GetStaticPressure());

            if (isFreshAir)
            {
                airflow = Unit.ConvertToSource(Convert.ToDouble(r.AirFlow), UnitType.AIRFLOW, AirFlowMasureUnit);
                if (airflow < rq_fa)
                    pass = false;
            }
            else
            {
                if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective)
                {
                    if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow || staticPressure < rq_StaticPressure)
                        pass = false;
                }
                if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective && !productType.Contains(", CO"))
                {
                    if (est_heat < rq_heat)
                        pass = false;
                }
            }

            return pass;
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
        private string _autoSelectNotification;

        public string AutoSelectionMessage
        {
            get { return _autoSelectNotification; }
            private set
            {
                SetValue<string>(ref _autoSelectNotification, value);
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
        private bool _isIndoorUnitEditable;
        private IModalWindowService _winService;

        public bool IsIndoorUnitEditable
        {
            get { return _isIndoorUnitEditable; }
            set
            {
                SetValue(ref _isIndoorUnitEditable, value);
            }
        }
        #endregion
    }
}