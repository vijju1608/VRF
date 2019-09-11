using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF.Model.New;
using JCHVRF_New.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.ViewModels
{

    public class IDUEquipmentPropertiesViewModel : ViewModelBase
    {

        IndoorBLL indoorBLL;
        ProjectBLL pbll;
        public ObservableCollection<IDUEquipmentModel> IDUEquipmentList { get; set; }
        public List<TypeModel> ModelsLookUpData = new List<TypeModel>();
        public JCHVRF.Model.Project ProjectLegacy { get; set; }
        int _fanSpeedLevel = -1;
        #region IDUEquipmentProperties


        private string _EquipmentName;
        public string EquipmentName
        {
            get
            {
                return _EquipmentName;
            }
            set
            {
                _EquipmentName = value;
                OnPropertyChanged("EquipmentName");
            }
        }

        private string _SelectedFloor;
        public string SelectedFloor
        {
            get
            {
                return _SelectedFloor;
            }
            set
            {
                _SelectedFloor = value;
                OnPropertyChanged("SelectedFloor");
            }
        }


        private int _SelectedFanSpeed;
        public int SelectedFanSpeed
        {
            get
            {
                return _SelectedFanSpeed;
            }
            set
            {
                _SelectedFanSpeed = value;
                OnPropertyChanged("SelectedFanSpeed");
                GetIDUModelsLookUpData(_selectedType);
                GetIDUProperties(SelectedModel);
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
                _selectedType = value;
                OnPropertyChanged("SelectedType");
                GetIDUModelsLookUpData(_selectedType);
            }
        }
        private string _selectedModel;
        public string SelectedModel
        {

            get
            {
                return _selectedModel;
            }

            set
            {
                _selectedModel = value;
                OnPropertyChanged("SelectedModel");
                GetIDUProperties(_selectedModel);
            }
        }
        public string TemperatureUnit { get; set; }
        public string PowerUnit { get; set; }
        public string AirFlowUnit { get; set; }
        private double _lblAirFlow;
        public double LblAirFlow
        {
            get
            {
                return _lblAirFlow;
            }
            set
            {
                _lblAirFlow = value;
                OnPropertyChanged("LblAirFlow");
            }
        }
        private double _lblHeatCapacity;
        public double LblHeatCapacity
        {
            get
            {
                return _lblHeatCapacity;
            }
            set
            {
                _lblHeatCapacity = value;
                OnPropertyChanged("LblHeatCapacity");
            }
        }
        private double _lblCoolCapacity;
        public double LblCoolCapacity
        {
            get
            {
                return _lblCoolCapacity;
            }
            set
            {
                _lblCoolCapacity = value;
                OnPropertyChanged("LblCoolCapacity");

            }
        }
        private double _lblSensibleHeating;
        public double LblSensibleHeating
        {
            get
            {
                return _lblSensibleHeating;
            }
            set
            {
                _lblSensibleHeating = value;
                OnPropertyChanged("LblSensibleHeating");

            }
        }
        private double _lblDbHeat;
        public double LblDBHeat
        {
            get
            {
                return _lblDbHeat;
            }
            set
            {
                _lblDbHeat = value;
                OnPropertyChanged("LblDBHeat");
            }
        }
        private double _lblOutCoolingDB;
        public double LblOutCoolingDB
        {
            get
            {
                return _lblOutCoolingDB;
            }
            set
            {
                _lblOutCoolingDB = value;
                OnPropertyChanged("LblOutCoolingDB");
            }
        }

        private double _lblOutHeatingDB;
        public double LblOutHeatingDB
        {
            get
            {
                return _lblOutHeatingDB;
            }
            set
            {
                _lblOutHeatingDB = value;
                OnPropertyChanged("LblOutHeatingDB");
            }
        }
        private double _lblOutHeatingWB;
        public double LblOutHeatingWB
        {
            get
            {
                return _lblOutHeatingWB;
            }
            set
            {
                _lblOutHeatingWB = value;
                OnPropertyChanged("LblOutHeatingWB");
            }
        }
        private double _lblDbCool;
        public double LblDbCool
        {
            get
            {
                return _lblDbCool;
            }
            set
            {
                _lblDbCool = value;
                OnPropertyChanged("LblDbCool");

            }
        }

        private double _lblWbCool;
        public double LblWbCool
        {
            get
            {
                return _lblWbCool;
            }
            set
            {
                _lblWbCool = value;
                OnPropertyChanged("LblWbCool");

            }
        }

        private double _lblfreshAir;
        public double LblFreshAir
        {
            get
            {
                return _lblfreshAir;
            }
            set
            {
                _lblfreshAir = value;
                OnPropertyChanged("LblFreshAir");

            }
        }

        private double _lblEsp;
        public double LblEsp
        {
            get
            {
                return _lblEsp;
            }
            set
            {
                _lblEsp = value;
                OnPropertyChanged("LblEsp");

            }
        }
        private int _indexEquipmentType;
        public int IndexEquipmentType
        {
            get
            {
                return _indexEquipmentType;
            }
            set
            {
                _indexEquipmentType = value;
                OnPropertyChanged("IndexEquipmentType");
            }
        }

        private int _selectedIndexModel;
        public int SelectedIndexModel
        {
            get
            {
                return _selectedIndexModel;
            }
            set
            {
                _selectedIndexModel = value;
                OnPropertyChanged("SelectedIndexModel");
            }
        }
        public string PressureUnit { get; set; }

        #endregion IDUEquipmentProperties

        #region LookUpData
        public List<string> Types { get; set; }
        private List<ComboBox> listModel;
        public List<ComboBox> ListModel
        {
            get
            {
                return listModel;
            }
            set
            {
                listModel = value;
                OnPropertyChanged("ListModel");
            }
        }
        public ObservableCollection<string> Floors { get; set; }
        #endregion LookUpData      

        public IDUEquipmentPropertiesViewModel(JCHVRF.Model.Project projectLegacy, RoomIndoor roomIndoor)
        {
            try
            {
                this.ProjectLegacy = projectLegacy;
                pbll = new ProjectBLL(this.ProjectLegacy);

                this.indoorBLL = new IndoorBLL(ProjectLegacy.SubRegionCode, ProjectLegacy.RegionCode, ProjectLegacy.BrandCode);
                // Bind Equipment Pre Selected Data To View Model
                if (roomIndoor != null)
                {
                    this.EquipmentName = roomIndoor.IndoorName;
                    this.SelectedFloor = roomIndoor.RoomName;
                }
                // End Bind Equipment Pre Selected Data To View Model
                GetFloorList();


                BindFanSpeed();
                BindToControl();


                // Bind Equipment Pre Selected Data To View Model
                //if (this.SelectedFanSpeed != null)
                //{
                if (roomIndoor != null)
                {
                    this.SelectedFanSpeed = roomIndoor.FanSpeedLevel;
                    if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
                    {
                        GetTypeLookUpData(roomIndoor.DisplayImageName);
                    }
                    else
                    {
                        GetTypeLookUpDataRegionWise(roomIndoor.DisplayImageName);
                    }
                }// roomIndoor.FanSpeedLevel == 1 ? "Max" : roomIndoor.FanSpeedLevel == 0 ? "High2" : roomIndoor.FanSpeedLevel == 1 ? "High" : roomIndoor.FanSpeedLevel == 2 ? "Medium" : roomIndoor.FanSpeedLevel == 3 ? "Low" : "";
                 // }
                 // End Bind Equipment Pre Selected Data To View Model



                // Bind Equipment Pre Selected Data To View Model
                if (roomIndoor != null)
                {
                    if (roomIndoor.IndoorItem.DisplayName != null)
                        this.SelectedType = roomIndoor.IndoorItem.DisplayName;
                    // End Bind Equipment Pre Selected Data To View Model
                    // GetSelectedIDUAccessories();


                    //this.AddAccessoryViewModel = new AddAccessoriesTemplateViewModel(this.ProjectLegacy, roomIndoor);
                    OnPropertyChanged(AddAccessoryViewModel.Accessories.ToString());
                }


                //SetEquipmentProperties();
                //To Select 

                if (roomIndoor != null && roomIndoor.IndoorItem != null)
                {
                    // this.SelectedType = roomIndoor.IndoorItem.Type;
                }

                // Bind Equipment for Pre Selected Data   
                if (roomIndoor != null)
                {
                    if (roomIndoor.IndoorItem != null && roomIndoor.IndoorItem.Model != null)
                    {
                        this.SelectedModel = roomIndoor.IndoorItem.Model;
                        //this.SelectedIndexModel = this.ListModel.FindIndex(a => a.Value == roomIndoor.IndoorItem.Model);
                    }
                    else
                    {
                        if (ListModel != null && ListModel.Count > 0)
                            this.SelectedModel = ListModel.FirstOrDefault().Value;
                    }

                }

                if (roomIndoor != null && roomIndoor.IndoorItem != null && roomIndoor.IndoorItem.ListAccessory != null)
                {
                    var itemIndex = this.ProjectLegacy.RoomIndoorList.FindIndex(x => x.IndoorNO == roomIndoor.IndoorNO);
                    var item = this.ProjectLegacy.RoomIndoorList.ElementAt(itemIndex);
                    this.ProjectLegacy.RoomIndoorList.RemoveAt(itemIndex);
                    item.ListAccessory = roomIndoor.IndoorItem.ListAccessory;
                    this.ProjectLegacy.RoomIndoorList.Insert(itemIndex, item);
                }
                // this.SelectedFanSpeed = 0;
                // End Bind Equipment Pre Selected Data To View Model
            }
            catch (Exception ex)
            {
                int? id = JCHVRF.Model.Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        void ShowSelectedIDUData(string iduName = "")
        {
            if (iduName == "")
            {
                //To Load Data of First Indoor Unit
                var idu = ProjectLegacy.RoomIndoorList.FirstOrDefault();
                if (idu != null)
                {
                    SetEquipmentProperties(idu.IndoorItem);
                }
            }
        }
        void SetEquipmentProperties(Indoor ri)
        {
            this.SelectedType = ri.Type;
            this.SelectedModel = ri.Model;

        }

        #region BindingEquipmentData
        private void BindIndoorUnitProperty(List<IDUEquipmentModel> equipementModel)
        {
            if (equipementModel != null)
            {
                this.LblAirFlow = Unit.ConvertToControl((equipementModel.Select(mm => mm.AirFlow).FirstOrDefault()), UnitType.AIRFLOW, AirFlowUnit);
                this.LblCoolCapacity = Unit.ConvertToControl((equipementModel.Select(mm => mm.CoolCapacity).FirstOrDefault()), UnitType.POWER, PowerUnit);
                this.LblHeatCapacity = Unit.ConvertToControl((equipementModel.Select(mm => mm.HeatCapacity).FirstOrDefault()), UnitType.POWER, PowerUnit);
                this.LblSensibleHeating = Unit.ConvertToControl((equipementModel.Select(mm => mm.SensibleHeat).FirstOrDefault()), UnitType.POWER, PowerUnit);
                this.LblDbCool = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB, UnitType.TEMPERATURE, TemperatureUnit);
                this.LblDBHeat = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB, UnitType.TEMPERATURE, TemperatureUnit);
                this.LblWbCool = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB, UnitType.TEMPERATURE, TemperatureUnit);
                this.LblOutCoolingDB = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB, UnitType.TEMPERATURE, TemperatureUnit);
                this.LblOutHeatingDB = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB, UnitType.TEMPERATURE, TemperatureUnit);
                this.LblOutHeatingWB = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB, UnitType.TEMPERATURE, TemperatureUnit);
                this.LblEsp = (equipementModel.Select(mm => mm.ESP).FirstOrDefault()); //Pressure Unit, no need to convert
                this.LblFreshAir = 0;//Default Value;
            }
        }
        private void BindUnit()
        {
            AirFlowUnit = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            PowerUnit = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            TemperatureUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            PressureUnit = Unit.ut_Pressure;
        }
        private void BindToControl()
        {
            BindUnit();
            LblDbCool = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB, UnitType.TEMPERATURE, TemperatureUnit);
            LblDBHeat = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB, UnitType.TEMPERATURE, TemperatureUnit);
            LblWbCool = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB, UnitType.TEMPERATURE, TemperatureUnit);
            LblOutCoolingDB = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB, UnitType.TEMPERATURE, TemperatureUnit);
            LblOutHeatingDB = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB, UnitType.TEMPERATURE, TemperatureUnit);
            LblOutHeatingWB = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB, UnitType.TEMPERATURE, TemperatureUnit);
            // LblEsp = 0;//Default Value;
            LblFreshAir = 0;//Default Value;
        }
        #endregion BindingEquipmentData

        #region LookUpData
        /// <summary>
        /// To Get TypeLookUp Data
        /// </summary>
        private void GetTypeLookUpData(string indoorType)
        {
            List<TypeModel> listType = new List<TypeModel>();
            DataTable dtIndoorType = indoorBLL.GetIndoorDisplayName();
            Types = new List<string>();
            if (dtIndoorType != null && dtIndoorType.Rows.Count > 0)
            {
                TypeModel objTypeModel;
                int i = -1;
                foreach (DataRow typeRow in dtIndoorType.Rows)
                {
                    i++;
                    string DisplayName = typeRow.ItemArray[0].ToString();
                    if (DisplayName.Equals(indoorType))
                    {
                        this.IndexEquipmentType = i;
                        SelectedType = indoorType;
                    }
                    objTypeModel = new TypeModel();
                    objTypeModel.DisplayName = DisplayName;
                    listType.Add(objTypeModel);
                    Types.Add(objTypeModel.DisplayName);

                }

            }
        }
        /// <summary>
        /// To get Models on the basis Type/Product Type/Factory
        /// </summary>
        public void GetIDUModelsLookUpData(string type)
        {
            string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string _productType = "Universal IDU";
            List<IDUEquipmentModel> objEquipmentList = new List<IDUEquipmentModel>();
            List<IDUEquipmentModel> IDUEquipmentModellist = new List<IDUEquipmentModel>();
            List<string> listType = new List<string>();
            listType = Types;// Needs further analsis
            ProjectLegacy.FactoryCode = indoorBLL.GetFCodeByDisUnitType(type, out listType);
            DataTable dtIndore = indoorBLL.GetUniversalIndoorListStd(type, ProjectLegacy.FactoryCode, listType);
            objEquipmentList = (from DataRow row in dtIndore.Rows
                                select new IDUEquipmentModel
                                {
                                    Model = row["Model"].ToString(),
                                    ModelFull = row["ModelFull"].ToString(),
                                    ModelYork = row["Model_York"].ToString(),
                                    ModelHitachi = row["Model_Hitachi"].ToString(),
                                    CoolCapacity = Convert.ToDouble(row["CoolCapacity"]),
                                    SensibleHeat = Convert.ToDouble(row["SensibleHeat"]),
                                    HeatCapacity = Convert.ToDouble(row["HeatCapacity"]),
                                    AirFlow = Convert.ToDouble(row["AirFlow"]),
                                    Type = type,
                                    ProductType = _productType

                                }).ToList();
            BatchCalculateEstValue(objEquipmentList, ref IDUEquipmentModellist);
            IDUEquipmentList = new ObservableCollection<IDUEquipmentModel>(IDUEquipmentModellist);
            PopulateModel();
        }
        private void BatchCalculateEstValue(List<IDUEquipmentModel> IDUEquipmentModelList, ref List<IDUEquipmentModel> objEquipmentList)
        {
            if (SelectedFanSpeed != -1)
                _fanSpeedLevel = SelectedFanSpeed - 1;
            double outdoorCoolingDB = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
            double outdoorHeatingWB = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;
            double outdoorCoolingIW = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
            double outdoorHeatingIW = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;
            double wb_c = Unit.ConvertToSource(Convert.ToDouble(LblWbCool), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE);
            double db_h = Unit.ConvertToSource(Convert.ToDouble(LblDBHeat), UnitType.TEMPERATURE, SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE);
            double fa = 0;
            double airflow = 0;
            double staticPressure = 0;
            for (int i = 0; i < IDUEquipmentModelList.Count; ++i)
            {
                Indoor inItem = indoorBLL.GetItem(IDUEquipmentModelList[i].ModelFull, null, IDUEquipmentModelList[i].ProductType, null);
                if (inItem == null) continue;
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
                    est_cool = indoorBLL.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
                    double shf = inItem.GetSHF(_fanSpeedLevel);
                    est_sh = est_cool * shf;
                    if (!inItem.ProductType.Contains(", CO"))
                    {
                        est_heat = indoorBLL.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
                    }
                }
                if (IndoorBLL.IsFreshAirUnit(type))
                {
                    fa = inItem.AirFlow;
                }
                else if (!type.Contains("Hydro Free") && type != "DX-Interface")
                {
                    airflow = inItem.GetAirFlow(_fanSpeedLevel);
                    if (type.Contains("Ducted") || type.Contains("Total Heat Exchanger"))
                    {
                        staticPressure = inItem.GetStaticPressure();
                    }
                }
                objEquipmentList.Add(new IDUEquipmentModel
                {
                    Model = IDUEquipmentModelList[i].Model,
                    ModelFull = IDUEquipmentModelList[i].ModelFull,
                    ModelYork = IDUEquipmentModelList[i].ModelYork,
                    ModelHitachi = IDUEquipmentModelList[i].ModelHitachi,
                    CoolCapacity = Math.Round(est_cool, 1),
                    SensibleHeat = Math.Round(est_sh, 1),
                    HeatCapacity = Math.Round(est_heat, 1),
                    AirFlow = Math.Round(airflow, 1, MidpointRounding.AwayFromZero),
                    Type = IDUEquipmentModelList[i].Type,
                    ESP = staticPressure

                });

            }
        }
        private double ConvertStringToDouble(string doubleString)
        {
            double returnDouble = 0;
            if (double.TryParse(doubleString, out returnDouble))
                return returnDouble;
            else
                return 0;
        }
        /// <summary>
        /// Populate Model Data
        /// </summary>
        /// <returns></returns>
        public void PopulateModel()
        {
            listModel = new List<ComboBox>();
            if (IDUEquipmentList != null)
            {
                foreach (var item in IDUEquipmentList)
                {
                    if (ProjectLegacy.BrandCode == "H")
                    {
                        listModel.Add(new ComboBox
                        {
                            DisplayName = item.ModelHitachi,
                            Value = item.Model,
                            //Model = item.Model,
                            //ModelFullName = item.ModelFull,
                            //ModelHitachi = item.ModelHitachi
                        });
                    }
                    else if (ProjectLegacy.BrandCode == "S")
                    {
                        listModel.Add(new ComboBox
                        {
                            DisplayName = item.ModelHitachi,
                            Value = item.Model,
                            //Model = item.Model,
                            //ModelFullName = item.ModelFull,
                            //ModelYork = item.ModelYork
                        });
                    }
                }
            }
            this.ListModel = listModel;
            if (listModel.Count > 0)
                this.SelectedIndexModel = 0;
        }
        #endregion LookUpData

        #region SetIDUData
        public void GetIDUProperties(string selectedModel)
        {
            List<IDUEquipmentModel> iduProperties = new List<IDUEquipmentModel>();
            var prop = IDUEquipmentList.Where(x => x.Model.Equals((selectedModel)));
            iduProperties = new List<IDUEquipmentModel>(prop);
            if (IDUEquipmentList != null)
            {

                BindIndoorUnitProperty(iduProperties);
            }
        }
        #region BindFanSpeed

        public void BindFanSpeed()
        {
            FanSpeeds = new ObservableCollection<IDUEquipmentModel>() {
            new IDUEquipmentModel(){FanSpeed="Max"}
            ,new IDUEquipmentModel(){FanSpeed="High2"}
            ,new IDUEquipmentModel(){FanSpeed="High"}
            ,new IDUEquipmentModel(){FanSpeed="Medium"}
            ,new IDUEquipmentModel() {FanSpeed="Low" }
            };
        }

        private ObservableCollection<IDUEquipmentModel> _fanSpeeds;

        public ObservableCollection<IDUEquipmentModel> FanSpeeds
        {
            get { return _fanSpeeds; }
            set { _fanSpeeds = value; }
        }


        private IDUEquipmentModel _sfanspeed;

        public IDUEquipmentModel SFanSpeed
        {
            get { return _sfanspeed; }
            set { _sfanspeed = value; }
        }


        #endregion BindFanSpeed

        #region BindFloorList
        public void GetFloorList()
        {
            Floors = new ObservableCollection<string>();
            if (ProjectLegacy.FloorList != null)
            {
                foreach (var floor in ProjectLegacy.FloorList)
                {
                    Floors.Add(floor.Name);
                }
            }
        }
        #endregion BindFloorList


        public AddAccessoriesTemplateViewModel AddAccessoryViewModel
        {
            get { return _addAccessoryViewModel; }
            set
            {
                _addAccessoryViewModel = value;
                OnPropertyChanged("AddAccessoryViewModel");
            }
        }

        private AddAccessoriesTemplateViewModel _addAccessoryViewModel;



        #endregion SetIDUData


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GetTypeLookUpDataRegionWise(string indoorType)
        {
            // TODO : It will completely implemented once all system will available. thsi code will be uncomment once all trhings are set.
            string _series = string.Empty;
            string _ProductType = string.Empty;
            string SeletedSystem = string.Empty;// Todo : To be assign once It will get value selected System
            if (JCHVRF.Model.Project.CurrentProject.SystemListNextGen.Where(MM => MM.IsActiveSystem == true).FirstOrDefault() != null)
            {
                var system = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.Where(MM => MM.IsActiveSystem == true).FirstOrDefault();
                _series = system.OutdoorItem.Series;
                _ProductType = system.OutdoorItem.ProductType;
            }
            else
            {
                return;
            }

            _series = JCHVRF.Model.Project.CurrentProject.SystemListNextGen[0].OutdoorItem.Series;
            List<TypeModel> listType = new List<TypeModel>();
            DataTable dtIndoorType = indoorBLL.GetIndoorDisplayNameRegionWise(_ProductType, _series);
            Types = new List<string>();
            if (dtIndoorType != null && dtIndoorType.Rows.Count > 0)
            {
                TypeModel objTypeModel;
                int i = -1;
                foreach (DataRow typeRow in dtIndoorType.Rows)
                {
                    i++;
                    string DisplayName = typeRow.ItemArray[0].ToString();
                    if (DisplayName.Equals(indoorType))
                    {
                        this.IndexEquipmentType = i;
                        SelectedType = indoorType;
                    }
                    objTypeModel = new TypeModel();
                    objTypeModel.DisplayName = DisplayName;
                    listType.Add(objTypeModel);
                    Types.Add(objTypeModel.DisplayName);
                }

            }
        }

    }
}
