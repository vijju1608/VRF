using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Controls;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Views;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{

    public class AddEditRoomViewModel : ViewModelBase
    {

        private string _TempUnit;
        JCHVRF.Model.Project CurrentProject;
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        public DelegateCommand RemoveRoomCommand { get; set; }
        public DelegateCommand BulkRemoveRoomCommand { get; set; }
        public DelegateCommand BulkAddCommand { get; set; }
        public DelegateCommand RoomCheckChangedCommand { get; set; }
        public DelegateCommand chkSelectAll_UnCheckedCommand { get; set; }
        public DelegateCommand chkSelectAll_CheckedCommand { get; set; }
        public DelegateCommand GridTextchanged { get; set; }

        private bool _areSaveCancelVisible;

        public bool AreSaveCancelVisible
        {
            get { return this._areSaveCancelVisible; }
            set { this.SetValue(ref _areSaveCancelVisible, value); }
        }

        public DelegateCommand SaveClickCommand { get; set; }
        public string TemperatureUnit
        {
            get
            {
                return _TempUnit;
            }
            set
            {
                this.SetValue(ref _TempUnit, value);
            }
        }

        private string _areaUnit;

        public string AreaUnit
        {
            get
            {
                return _areaUnit;
            }
            set
            {
                this.SetValue(ref _areaUnit, value);
            }
        }

        private string _powerUnit;

        public string PowerUnit
        {
            get
            {
                return _powerUnit;
            }
            set
            {
                this.SetValue(ref _powerUnit, value);
            }
        }

        private string _airFlowRate;

        public string AirFlowRateUnit
        {
            get
            {
                return _airFlowRate;
            }
            set
            {
                this.SetValue(ref _airFlowRate, value);
            }
        }
        private string _pressureUnit;
        public string PressureUnit
        {
            get
            {
                return _pressureUnit;
            }
            set
            {
                this.SetValue(ref _pressureUnit, value);
            }
        }
        #region Local_Property
        private IEventAggregator _eventAggregator;
        #endregion
        string lblRoom = string.Empty;
        string sRoomName = SystemSetting.UserSetting.defaultSetting.RoomName;
        #region View_Model_Property
        private double? _CoolingDryBulb;
        public double? CoolingDryBulb
        {
            get
            {
                return SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB;
            }
            set { this.SetValue(ref _CoolingDryBulb, value); }
        }
        private double _CoolingWetBulb;
        public double CoolingWetBulb
        {
            get
            {
                return SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
            }
            set { this.SetValue(ref _CoolingWetBulb, value); }
        }
        private double _CoolingRelativeHumidity;
        public double CoolingRelativeHumidity
        {
            get
            {
                return SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH;
            }
            set { this.SetValue(ref _CoolingRelativeHumidity, value); }
        }
        private double _HeatingDryBulb;
        public double HeatingDryBulb
        {
            get
            {
                return SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
            }
            set { this.SetValue(ref _HeatingDryBulb, value); }
        }
        private double _area = 0.0;
        public double Area
        {
            get { return _area; }
            set { this.SetValue(ref _area, value); }
        }
        private int _peopleNumber = 0;
        public int PeopleNumber
        {
            get { return _peopleNumber; }
            set { this.SetValue(ref _peopleNumber, value); }
        }

        private double _SensibleHeat = 0.0;

        public double SensibleHeat
        {
            get { return _SensibleHeat; }
            set { this.SetValue(ref _SensibleHeat, value); }
        }
        #endregion
        private double _RqCapacityCool = 0.0;

        public double RqCapacityCool
        {
            get { return _RqCapacityCool; }
            set { this.SetValue(ref _RqCapacityCool, value); }
        }
        private double _RqCapacityHeat = 0.0;

        public double RqCapacityHeat
        {
            get { return _RqCapacityHeat; }
            set { this.SetValue(ref _RqCapacityHeat, value); }
        }

        private double _airflow = 0.0;

        public double Airflow
        {
            get { return _airflow; }
            set { this.SetValue(ref _airflow, value); }
        }

        private double _staticPressure = 0.0;

        public double StaticPressure
        {
            get { return _staticPressure; }
            set { this.SetValue(ref _staticPressure, value); }
        }

        private double _freshAir;

        public double FreshAir
        {
            get { return _freshAir; }
            set { _freshAir = value; }
        }

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
        public string CapacityMasureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingPOWER;
            }
        }

        public DelegateCommand AddRoomCommand { get; set; }

        private ObservableCollection<Room> _RoomList;
        private IModalWindowService _modalWindowService;

        public ObservableCollection<Room> RoomList
        {
            get
            {
                if (_RoomList == null)
                {
                    _RoomList = new ObservableCollection<Room>();
                    _RoomList.CollectionChanged += (o, e) =>
                    {
                        RaisePropertyChanged("RoomList");
                    };
                }
                return _RoomList;
            }
        }

        public AddEditRoomViewModel(IEventAggregator eventAggregator, IModalWindowService modalWindowService)
        {
            try
            {
                CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
                _eventAggregator = eventAggregator;
                _modalWindowService = modalWindowService;
                TemperatureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                AreaUnit = SystemSetting.UserSetting.unitsSetting.settingAREA;
                PowerUnit = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                AirFlowRateUnit = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                PressureUnit = SystemSetting.UserSetting.unitsSetting.settingESP;
                SaveClickCommand = new DelegateCommand(AddEditRoomClick);
                AddRoomCommand = new DelegateCommand(AddRoomIconClick);
                RemoveRoomCommand = new DelegateCommand(RemoveRoomIconClick);
                BulkRemoveRoomCommand = new DelegateCommand(BulkRemoveRoom);
                chkSelectAll_UnCheckedCommand = new DelegateCommand(chkSelectAll_UnChecked);
                chkSelectAll_CheckedCommand = new DelegateCommand(chkSelectAll_Checked);
                BulkAddCommand = new DelegateCommand(BulkAddClick);
                _eventAggregator.GetEvent<PubSubEvent<int>>().Subscribe(OnMultiRoomValueBulkUpload);
                AddRoomDefoult();
                RoomCheckChangedCommand = new DelegateCommand(() =>
                {
                    RaisePropertyChanged(nameof(AreAllRoomChecked));
                });
                GridTextchanged = new DelegateCommand(RoomListchanged_event);
                if (_modalWindowService.GetParameters(ViewKeys.AddEditRoom) != null)
                {
                    AreSaveCancelVisible = (bool)_modalWindowService.GetParameters(ViewKeys.AddEditRoom)["ShowSaveCancel"];
                }

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void AddRoomDefoult()
        {
            //JCHVRF.Model.Project.GetProjectInstance.RoomList.Clear();
            if (RoomList.Count > 0)
            {
                RoomList.Clear();
            }
            else
            {

                //RoomList = new ObservableCollection<Room>(JCHVRF.Model.Project.GetProjectInstance.RoomList);
                List<Room> RoomListTemp = new List<Room>();
                /*Backward compatibility : added null check to avoid null reference exception*/
                if (JCHVRF.Model.Project.GetProjectInstance.RoomList != null)
                {
                    foreach (var room in JCHVRF.Model.Project.GetProjectInstance.RoomList)
                    {
                        Room localRoom = new Room();
                        localRoom.Id = room.Id;
                        localRoom.Name = room.Name;
                        localRoom.SensibleHeat = Unit.ConvertToControl(Convert.ToDouble(room.SensibleHeat), UnitType.POWER, CapacityMasureUnit);

                        localRoom.RqCapacityCool = Unit.ConvertToControl(Convert.ToDouble(room.RqCapacityCool), UnitType.POWER, utPower);
                        localRoom.RqCapacityHeat = Unit.ConvertToControl(Convert.ToDouble(room.RqCapacityHeat), UnitType.POWER, utPower);
                        localRoom.HeatingDryBulb = room.HeatingDryBulb;
                        localRoom.CoolingDryBulb = room.CoolingDryBulb;
                        localRoom.CoolingWetBulb = room.CoolingWetBulb;
                        localRoom.CoolingRelativeHumidity = room.CoolingRelativeHumidity;
                        localRoom.Area = room.Area;
                        localRoom.PeopleNumber = room.PeopleNumber;
                        localRoom.AirFlow = Unit.ConvertToControl(room.AirFlow, UnitType.AIRFLOW, utAirflow);
                        localRoom.StaticPressure = Convert.ToDouble(Unit.ConvertToControl(room.StaticPressure, UnitType.STATICPRESSURE, PressureUnit).ToString("n2"));//room.StaticPressure;
                        localRoom.FreshAir = room.FreshAir;
                        RoomListTemp.Add(localRoom);
                    }
                }
                RoomList.Clear();
                RoomList.AddRange(RoomListTemp);
                //JCHVRF.Model.Project.GetProjectInstance.RoomList.Clear();
                //JCHVRF.Model.Project.GetProjectInstance.RoomList.ToList().ForEach((item) =>
                //{
                //    //JCHVRF.Model.Project.GetProjectInstance.RoomList.Add(new Room
                //    //{
                //        Id = item.Id,
                //        Name = item.Name,
                //        SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(item.SensibleHeat), UnitType.POWER, CapacityMasureUnit),


                //        RqCapacityCool = Unit.ConvertToControl(Convert.ToDouble(item.RqCapacityCool), UnitType.POWER, utPower),
                //        RqCapacityHeat = Unit.ConvertToControl(Convert.ToDouble(item.RqCapacityHeat), UnitType.POWER, utPower),
                //        HeatingDryBulb = item.HeatingDryBulb,
                //        CoolingDryBulb = item.CoolingDryBulb,
                //        CoolingWetBulb = item.CoolingWetBulb,
                //        CoolingRelativeHumidity = item.CoolingRelativeHumidity,
                //        Area = item.Area,
                //        PeopleNumber = item.PeopleNumber,
                //        AirFlow = Unit.ConvertToSource(item.AirFlow, UnitType.AIRFLOW, utAirflow),
                //        StaticPressure = item.StaticPressure,
                //        FreshAir = item.FreshAir
                //        //FreshAir = Unit.ConvertToSource(item.FreshAir, UnitType.AIRFLOW, utAirflow)
                //   // });
                //});



                //CurrentProject.FloorList[0].RoomList.ForEach((item) =>
                //{
                //    RoomList.Add(new Room
                //    {
                //        Id = item.Id,
                //        Name = item.Name,
                //        SensibleHeat = Unit.ConvertToControl(Convert.ToDouble(item.SensibleHeat), UnitType.POWER, CapacityMasureUnit),
                //        RqCapacityCool = Unit.ConvertToControl(Convert.ToDouble(item.RqCapacityCool), UnitType.POWER, utPower),
                //        RqCapacityHeat = Unit.ConvertToControl(Convert.ToDouble(item.RqCapacityHeat), UnitType.POWER, utPower),
                //        HeatingDryBulb = item.HeatingDryBulb,
                //        CoolingDryBulb = item.CoolingDryBulb,
                //        CoolingWetBulb = item.CoolingWetBulb,
                //        CoolingRelativeHumidity = item.CoolingRelativeHumidity,
                //        Area = item.Area,
                //        PeopleNumber = item.PeopleNumber,
                //        AirFlow = Convert.ToDouble(Unit.ConvertToControl(item.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n1")),
                //        StaticPressure = item.StaticPressure,
                //        FreshAir = item.FreshAir,
                //        //FreshAir = Unit.ConvertToControl(item.FreshAir, UnitType.AIRFLOW, utAirflow)
                //        //SensibleHeat = item.SensibleHeat,
                //        //RqCapacityCool = item.RqCapacityCool,
                //        //RqCapacityHeat = item.RqCapacityHeat,
                //        //HeatingDryBulb = item.HeatingDryBulb,
                //        //CoolingDryBulb = item.CoolingDryBulb,
                //        //CoolingWetBulb = item.CoolingWetBulb,
                //        //CoolingRelativeHumidity = item.CoolingRelativeHumidity,
                //        //Area = item.Area,
                //        //PeopleNumber = item.PeopleNumber,
                //        //AirFlow = item.AirFlow,
                //        //StaticPressure = item.StaticPressure,
                //        //FreshAir = item.FreshAir
                //    });
                //});
            }
            // RoomList.Add(new Room { });
        }
        private void RemoveRoomIconClick()
        {
            if (JCHMessageBox.Show(language.Current.GetMessage("CONFIRM_ROOM_DELETE"), MessageType.Information, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (this.RoomList != null && this.RoomList.Count > 0)
                {
                    Room room = this.RoomList.LastOrDefault();
                    this.RoomList.Remove(room);
                    if (JCHVRF.Model.Project.CurrentProject != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                    {
                        UpdateRoomList(room);
                    }
                }
                if (!AreSaveCancelVisible)
                {
                    AddEditRoomClick();
                }
            }
        }

        private void UpdateRoomList(Room obRoom)
        {
            foreach (var item in JCHVRF.Model.Project.CurrentProject.RoomIndoorList)
            {
                if (item.SelectedRoom != null)
                {
                    if (item.SelectedRoom.Name.Equals(obRoom.Name))
                    {
                        item.SelectedRoom = null;
                    }
                }

            }
        }
        private void OnMultiRoomValueBulkUpload(int introomCount)
        {
            try
            {
                //int introomCount = int.Parse(RoomCount);
                if (introomCount > 0)
                {
                    int BulkAddCount = this.RoomList.Count + introomCount;
                    int Id = 0;
                    if (BulkAddCount <= 1000)
                    {
                        for (int i = this.RoomList.Count; i < BulkAddCount; i++)
                        {
                            Id = this.RoomList.Count + 1;
                            RoomList.Add(new Room
                            {
                                Id = (Id.ToString()),
                                Name = sRoomName + (Id),
                                SensibleHeat = SensibleHeat,
                                RqCapacityCool = RqCapacityCool,
                                RqCapacityHeat = RqCapacityHeat,
                                //HeatingDryBulb = HeatingDryBulb,
                                //CoolingDryBulb = CoolingDryBulb,
                                //CoolingWetBulb = CoolingWetBulb,
                                //CoolingRelativeHumidity = CoolingRelativeHumidity
                                HeatingDryBulb = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingHDB),
                                CoolingDryBulb = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingDB),
                                CoolingWetBulb = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingWB),
                                CoolingRelativeHumidity = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingRH)
                            });
                        }
                    }
                    else
                    {
                        JCHMessageBox.Show(language.Current.GetMessage("ALERT_ENTER_COUNT"));
                    }
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void BulkAddClick()
        {
            _modalWindowService.ShowView(ViewKeys.BulkRoomPopup, language.Current.GetMessage("ADD_ROOM"));
            if (!AreSaveCancelVisible)
            {
                AddEditRoomClick();
            }
        }
        private void BulkRemoveRoom()
        {
            bool isCheckedAny = RoomList.Any(x => x.IsRoomChecked == true);
            if (isCheckedAny)
            {
                if (JCHMessageBox.Show(language.Current.GetMessage("CONFIRM_ROOM_DELETE"), MessageType.Information, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    for (int i = RoomList.Count - 1; i >= 0; i--)
                    {
                        if (RoomList[i].IsRoomChecked)
                        {
                            if (JCHVRF.Model.Project.CurrentProject != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                            {
                                UpdateRoomList(RoomList[i]);
                            }
                            RoomList.RemoveAt(i);
                        }
                    }
                    if (!AreSaveCancelVisible)
                    {
                        AddEditRoomClick();
                    }
                }
            }
        }


        private void AddRoomIconClick()
        {
            try
            {
                if (this.RoomList.Count + 1 <= 1000)
                {
                    RoomList.Add(new Room(this.RoomList.Count)
                    {
                        Id = (this.RoomList.Count + 1).ToString(),
                        Name = sRoomName + (this.RoomList.Count + 1),
                        SensibleHeat = SensibleHeat,
                        RqCapacityCool = RqCapacityCool,
                        RqCapacityHeat = RqCapacityHeat,
                        Area = Area,
                        PeopleNumber = PeopleNumber,

                        HeatingDryBulb = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingHDB),
                        CoolingDryBulb = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingDB),
                        CoolingWetBulb = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingWB),
                        CoolingRelativeHumidity = Convert.ToDouble(CurrentProject.DesignCondition.indoorCoolingRH)
                    });
                    if (!AreSaveCancelVisible)
                    {
                        AddEditRoomClick();
                    }
                }
                else
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_ENTER_COUNT"));
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void chkSelectAll_UnChecked()
        {
            if (this.RoomList != null && this.RoomList.Count > 0)
            {
                foreach (var item in this.RoomList)
                {
                    item.IsRoomChecked = false;
                }
            }
        }
        private void chkSelectAll_Checked()
        {
            if (this.RoomList != null && this.RoomList.Count > 0)
            {
                foreach (var item in RoomList)
                {
                    item.IsRoomChecked = true;
                }
            }
        }
        private void RoomListchanged_event()
        {
            if (!AreSaveCancelVisible)
            {
                if (this.RoomList != null && this.RoomList.Count > 0)
                {
                    AddEditRoomClick();

                }

            }
        }

        public bool AreAllRoomChecked
        {
            get { return RoomList != null && RoomList.Count>0 && RoomList.All(a => a.IsRoomChecked); }
            set
            {
                if (RoomList != null)
                {
                    foreach (var item in RoomList)
                    {
                        item.IsRoomChecked = value;
                    }
                }
                RaisePropertyChanged();
            }
        }       

        private void AddEditRoomClick()
        {
            try
            {
                string errorMessage;
                bool IsValid = IsValidate(out errorMessage);
                if (IsValid)
                {
                    JCHVRF.Model.Project.GetProjectInstance.RoomList.Clear();
                    RoomList.ToList().ForEach((item) =>
                    {
                        JCHVRF.Model.Project.GetProjectInstance.RoomList.Add(new Room
                        {
                            Id = item.Id,
                            Name = item.Name,
                            SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(item.SensibleHeat), UnitType.POWER, CapacityMasureUnit),


                            RqCapacityCool = Unit.ConvertToSource(Convert.ToDouble(item.RqCapacityCool), UnitType.POWER, utPower),
                            RqCapacityHeat = Unit.ConvertToSource(Convert.ToDouble(item.RqCapacityHeat), UnitType.POWER, utPower),
                            HeatingDryBulb = item.HeatingDryBulb,
                            CoolingDryBulb = item.CoolingDryBulb,
                            CoolingWetBulb = item.CoolingWetBulb,
                            CoolingRelativeHumidity = item.CoolingRelativeHumidity,
                            Area = item.Area,
                            PeopleNumber = item.PeopleNumber,
                            AirFlow = Unit.ConvertToSource(item.AirFlow, UnitType.AIRFLOW, utAirflow),
                            StaticPressure = Unit.ConvertToSource(item.StaticPressure, UnitType.STATICPRESSURE, PressureUnit),//item.StaticPressure,
                            FreshAir = item.FreshAir
                            //FreshAir = Unit.ConvertToSource(item.FreshAir, UnitType.AIRFLOW, utAirflow)
                        });
                    });

                    //JCHVRF.Model.Project.GetProjectInstance.RoomList = RoomList.ToList();

                    //JCHVRF.Model.Project.GetProjectInstance.RoomList = CurrentProject.RoomList;


                    // CurrentProject.FloorList[0].RoomList = (RoomList.ToList());
                    //RoomList.ToList().ForEach((item) =>
                    //if (CurrentProject.FloorList[0].RoomList.Count > 0)
                    //{
                    //    CurrentProject.FloorList[0].RoomList.Clear();
                    //}


                    //RoomList.ToList().ForEach((item) =>
                    //{
                    //    CurrentProject.FloorList[0].RoomList.Add(new Room
                    //    {
                    //        Id = item.Id,
                    //        Name = item.Name,
                    //        SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(item.SensibleHeat), UnitType.POWER, CapacityMasureUnit),
                    //        RqCapacityCool = Unit.ConvertToSource(Convert.ToDouble(item.RqCapacityCool), UnitType.POWER, utPower),
                    //        RqCapacityHeat = Unit.ConvertToSource(Convert.ToDouble(item.RqCapacityHeat), UnitType.POWER, utPower),
                    //        HeatingDryBulb = item.HeatingDryBulb,
                    //        CoolingDryBulb = item.CoolingDryBulb,
                    //        CoolingWetBulb = item.CoolingWetBulb,
                    //        CoolingRelativeHumidity = item.CoolingRelativeHumidity,
                    //        Area = item.Area,
                    //        PeopleNumber = item.PeopleNumber,
                    //        AirFlow = Unit.ConvertToSource(item.AirFlow, UnitType.AIRFLOW, utAirflow),
                    //        StaticPressure = item.StaticPressure,
                    //        FreshAir = item.FreshAir
                    //        //FreshAir = Unit.ConvertToSource(item.FreshAir, UnitType.AIRFLOW, utAirflow)
                    //    });
                    //});

                    _eventAggregator.GetEvent<RoomListSaveSubscriber>().Publish();
                    _modalWindowService.Close(ViewKeys.AddEditRoom);
                    //if (win != null)
                    //{
                    //    //win.Close();
                    //}
                }
                else
                {
                    JCHMessageBox.Show(errorMessage, MessageType.Error);
                    
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        #region Validation 
        public bool IsValidate(out string errormessage)
        {
            bool IsValid = true;
            errormessage = "";
            foreach (var item in RoomList)
            {
                if (item.lblindoorCoolingDB != null && item.lblindoorCoolingDB != "")
                {
                    errormessage = language.Current.GetMessage("INVALID_RANGE") + item.lblindoorCoolingDB;
                    IsValid = false;
                }
                else if (item.lblindoorCoolingWB != null && item.lblindoorCoolingWB != "")
                {
                    errormessage = language.Current.GetMessage("INVALID_RANGE") + item.lblindoorCoolingWB;
                    IsValid = false;
                }
                else if (item.lblindoorHeatingDB != null && item.lblindoorHeatingDB != "")
                {
                    errormessage = language.Current.GetMessage("INVALID_RANGE") + item.lblindoorCoolingWB;
                    IsValid = false;
                }
                else if (item.lblindoorSensible != null && item.lblindoorSensible != "")
                {
                    errormessage = language.Current.GetMessage("ERROR_IDU_VALIDATION");
                    IsValid = false;
                }
            }
            return IsValid;

        }
        #endregion


    }
}
