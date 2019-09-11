using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Windows;
using JCHVRF_New.Views;
using System.Windows.Input;
using System.Windows.Controls;
using NextGenModel = JCHVRF.Model.NextGen;
using System.Collections.Generic;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using System.Data;
using System.Linq;
using System.IO;
using System;
using System.Collections.ObjectModel;
using JCHVRF.BLL;
using JCHVRF.DAL;
using System.Text.RegularExpressions;

namespace JCHVRF_New.ViewModels
{
    class SystemTabViewModel : ViewModelBase
    {
        private Views.ucDesignerCanvas designerCanvas;
        string ODUProductTypeUniversal = "Universal IDU";
        string ODUProductType;
        DataTable IDuList = new DataTable();
        string _factory = "";
        List<string> _typeList = new List<string>();
        string _type = "";
        IndoorBLL indoorBLL;
        
        CentralControllerBLL controllerBLL;
        private IEventAggregator _eventAggregator;

        private ItemsControl imageThumbnails;

        private DataTable dtFullPath;// dtFullImagePath;

        //private TextBox txtSearch;
        public equipmentTypeButtonClick enumEQSrch;
        public ControllerLayoutType enumCCSrch;
        private DataTable dtFillVrfImageType;
        HashSet<CentralController> allCentralControllers = new HashSet<CentralController>();

        private string _EquipmentListBckClr;
        public string EquipmentListBckClr
        {
            get { return this._EquipmentListBckClr; }
            set { this.SetValue(ref _EquipmentListBckClr, value); }
        }

        public static ObservableCollection<NextGenModel.ImageData> EquipmentList { get; set; }

        //ACC - RAG
        //Mandatory data for Auto-Control-Wiring
        public static ObservableCollection<NextGenModel.ImageData> VRFEquipmentList { get; set; }
        public static ObservableCollection<NextGenModel.ImageData> HEEquipmentList { get; set; }
        bool HEclicked = false;
        bool VRFclicked = false;
        //

        public DelegateCommand OnBACNETClickCommand { get; set; }
        public DelegateCommand OnAllSysClickCommand { get; set; }
        public DelegateCommand OnHEClickCommand { get; set; }
        public DelegateCommand OnVRFClickCommand { get; set; }

        public DelegateCommand OnCCClickCommand { get; set; }
        public DelegateCommand OnMODBUSClickCommand { get; set; }
        public DelegateCommand OnLonWorksClickCommand { get; set; }
        public DelegateCommand OnKNXClickCommand { get; set; }
        public DelegateCommand OnNOBMSClickCommand { get; set; }
        public DelegateCommand OnSearchClickCommand { get; set; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnSearchClick();
            }
        }

        public IEventAggregator EventAggregator { get; set; }

        private SystemBase _currentSystem;
        public SystemBase CurrentSystem {
            get { return _currentSystem; }
            internal set
            {
                SetValue(ref _currentSystem, value);
                if(_currentSystem != null)
                    {
                    LoadSystemSpecificEquipmentList();
                }
            }           
        }
                        
        public bool IsAllSelected
        {
            get { return enumEQSrch == equipmentTypeButtonClick.All; }
            set { if (value)
                {
                    enumEQSrch = equipmentTypeButtonClick.All;
                };
                OnEquipmentFiltered();
            }
        }

        public bool IsIDUSelected
        {
            get { return enumEQSrch == equipmentTypeButtonClick.IDU; }
            set
            {
                if (value)
                {
                    enumEQSrch = equipmentTypeButtonClick.IDU;
                };
                OnEquipmentFiltered();
            }
        }
        public bool IsPipeSelected
        {
            get { return enumEQSrch == equipmentTypeButtonClick.Pipe; }
            set
            {
                if (value)
                {
                    enumEQSrch = equipmentTypeButtonClick.Pipe;
                };
                OnEquipmentFiltered();
            }
        }


        public SystemTabViewModel(IProjectInfoBAL projctInfoBll, IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            try
            {
                EventAggregator = eventAggregator;
                RegionManager = regionManager;
                EquipmentList = new ObservableCollection<NextGenModel.ImageData>();
                HEEquipmentList = new ObservableCollection<NextGenModel.ImageData>();
                VRFEquipmentList = new ObservableCollection<NextGenModel.ImageData>();
                OnAllSysClickCommand = new DelegateCommand(OnAllSysClick);
                OnHEClickCommand = new DelegateCommand(OnHEClick);
                OnVRFClickCommand = new DelegateCommand(OnVRFClick);


                //Command for controller filters
                OnCCClickCommand = new DelegateCommand(OnCCClick);
                OnBACNETClickCommand = new DelegateCommand(OnBACNETClick);
                OnMODBUSClickCommand = new DelegateCommand(OnMODBUSClick);
                OnLonWorksClickCommand = new DelegateCommand(OnLonWorksClick);
                OnKNXClickCommand = new DelegateCommand(OnKNXClick);
                OnNOBMSClickCommand = new DelegateCommand(OnNOBMSClick);

                OnSearchClickCommand = new DelegateCommand(OnSearchClick);

                //event aggregators
                EventAggregator.GetEvent<DeleteEquipmentList>().Subscribe(DeleteFromEquipmentList);
                EventAggregator.GetEvent<AddEquipmentList>().Subscribe(AddToEquipmentList);
                EventAggregator.GetEvent<CleanEqupmentList>().Subscribe(OnCleanupEqp);
                EventAggregator.GetEvent<LoadAllSystem>().Subscribe(OnAllSysClick);

                JCHVRF.Model.Project.CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
                this.indoorBLL = new IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.RegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                this.controllerBLL = new CentralControllerBLL(JCHVRF.Model.Project.GetProjectInstance);

                //EquipmentList.Add(new Equipment("Down", @"C:\projects\VRF NextGen\Dev\VRFDesktopApplication\JCHVRF\JCHVRF_New\Image\Down.png"));
                //Frame.Content = designerCanvas.Content;
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private Visibility _heatexchangerreqdvis;
        public Visibility HeatExchangerReqdVis
        {
            get { return this._heatexchangerreqdvis; }
            set { this.SetValue(ref _heatexchangerreqdvis, value); }
        }

        private Visibility _selectedVRFType;
        public Visibility SelectedVRFType
        {
            get { return this._selectedVRFType; }
            set { this.SetValue(ref _selectedVRFType, value); }
        }

        private Visibility _selectedControllerType;
        public Visibility SelectedControllerType
        {
            get { return this._selectedControllerType; }
            set { this.SetValue(ref _selectedControllerType, value); }
        }

        private int _AllSystemCount;
        public int AllSystemCount
        {
            get { return this._AllSystemCount; }
            set { this.SetValue(ref _AllSystemCount, value); }
        }

        private int _HECount;
        public int HECount
        {
            get { return this._HECount; }
            set { this.SetValue(ref _HECount, value); }
        }

        private int _VRFCount;
        public int VRFCount
        {
            get { return this._VRFCount; }
            set { this.SetValue(ref _VRFCount, value); }
        }

        public IRegionManager RegionManager { get; internal set; }
        private Visibility _filterBacnet;
        public Visibility FilterBacnet
        {
            get { return this._filterBacnet; }
            set { this.SetValue(ref _filterBacnet, value); }
        }
        private Visibility _filterNoBMS;
        public Visibility FilterNoBMS
        {
            get { return this._filterNoBMS; }
            set { this.SetValue(ref _filterNoBMS, value); }
        }
        private Visibility _filterModbus;
        public Visibility FilterModbus
        {
            get { return this._filterModbus; }
            set { this.SetValue(ref _filterModbus, value); }
        }
        private Visibility _filterKnx;
        public Visibility FilterKnx
        {
            get { return this._filterKnx; }
            set { this.SetValue(ref _filterKnx, value); }
        }
        private Visibility _filterLonWorks;
        public Visibility FilterLonWorks
        {
            get { return this._filterLonWorks; }
            set { this.SetValue(ref _filterLonWorks, value); }
        }
        public void GetFiltersVisbility(string systemType)
        {
            switch(systemType)
            {
                case "1":
                    SelectedVRFType = Visibility.Visible;
                    SelectedControllerType = Visibility.Collapsed;                   
                    HeatExchangerReqdVis = Visibility.Visible;
                    FilterBacnet = Visibility.Collapsed;
                    FilterNoBMS = Visibility.Collapsed;
                    FilterModbus = Visibility.Collapsed;
                    FilterKnx = Visibility.Collapsed;
                    FilterLonWorks = Visibility.Collapsed;
                    break;

                case "6":
                    SelectedVRFType = Visibility.Collapsed;
                    SelectedControllerType = Visibility.Visible;
                    FilterVisibilityForController();
                    HeatExchangerReqdVis = Visibility.Visible;
                    break;

                default: 
                    SelectedVRFType = Visibility.Collapsed;
                    SelectedControllerType = Visibility.Collapsed;
                    HeatExchangerReqdVis = Visibility.Collapsed;
                    break;
            }           
        }

        private void OnCleanupEqp()
        {
            EventAggregator.GetEvent<DeleteEquipmentList>().Unsubscribe(DeleteFromEquipmentList);
            EventAggregator.GetEvent<AddEquipmentList>().Unsubscribe(AddToEquipmentList);
            EventAggregator.GetEvent<CleanEqupmentList>().Unsubscribe(OnCleanupEqp);
            EventAggregator.GetEvent<LoadAllSystem>().Unsubscribe(OnAllSysClick);

        }

        private void DeleteFromEquipmentList(object sel)
        {
            var DraggedItem = (NextGenModel.ImageData)sel;
            if (EquipmentList.Count == 0 && DraggedItem.equipmentType == "System")
            {
                if (DraggedItem.Source.GetType() == typeof(NextGenModel.SystemVRF))
                {
                    VRFCount -= 1;
                }
                else
                {
                    HECount -= 1;
                }
                AllSystemCount = VRFCount + HECount;
                return;
            }

            if (DraggedItem.equipmentType == "System")
            {
                NextGenModel.ImageData Remove = EquipmentList.FirstOrDefault(mm => mm.UniqName.Equals(DraggedItem.UniqName));
                if (Remove != null)
                {
                    EquipmentList.Remove(Remove);
                }
                if (DraggedItem.Source.GetType() == typeof(NextGenModel.SystemVRF))
                {
                    VRFCount -= 1;
                }
                else
                {
                    HECount -= 1;
                }
                AllSystemCount = VRFCount + HECount;
            }
        }

        private void AddToEquipmentList(object sel)
        {            
            if(((NextGenModel.JCHNode)sel).ImageData != null)
            {
                var DraggedItem = ((NextGenModel.JCHNode)sel).ImageData;
                if (DraggedItem.equipmentType == "System" && VRFEquipmentList != null)
                {
                    if (DraggedItem.Source.GetType() == typeof(NextGenModel.SystemVRF))
                    {
                        OnVRFClick();
                        EventAggregator.GetEvent<VRFFilterColor>().Publish();
                    }
                    else if (DraggedItem.Source.GetType() == typeof(RoomIndoor))
                    {
                        OnHEClick();
                        EventAggregator.GetEvent<HEFilterColor>().Publish();
                    }
                }
            }
            else
            {
                var checkType = (NextGenModel.WiringNodeCentralControl)sel;
                if (checkType.SystemItem != null)
                {
                    OnVRFClick();
                    EventAggregator.GetEvent<VRFFilterColor>().Publish();
                }
                else if (checkType.RoomIndoorItem != null)
                {
                    OnHEClick();
                    EventAggregator.GetEvent<HEFilterColor>().Publish();
                }                                                               
            }
            AllSystemCount = VRFCount + HECount;
           
        }

        private void FilterVisibilityForController()
        {
            ForBacnet();
            ForNoBMS();
            ForModbus();
            ForKnx();
            ForLonWorks();           
        }

        private void ForBacnet()
        {
            List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.BACNetInterface)).ToList();   
            if(result.Count != 0)
            {
                FilterBacnet = Visibility.Visible;
            }
            else
            {
                FilterBacnet = Visibility.Collapsed;
            }            
        }
        private void ForNoBMS()
        {
            List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.CentralController) || myRow.Type.Equals(ControllerType.ONOFF) || myRow.Type.Equals(ControllerType.Software)).ToList();   
            if(result.Count != 0)
            {
                FilterNoBMS = Visibility.Visible;
            }
            else
            {
                FilterNoBMS = Visibility.Collapsed;
            } 
        }
        private void ForModbus()
        {
            List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.ModBusInterface)).ToList();  
            if(result.Count != 0)
            {
                FilterModbus = Visibility.Visible;
            }
            else
            {
                FilterModbus = Visibility.Collapsed;
            } 
        }
        private void ForKnx()
        {
            List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.KNXInterface)).ToList();   
            if(result.Count != 0)
            {
                FilterKnx = Visibility.Visible;
            }
            else
            {
                FilterKnx = Visibility.Collapsed;
            } 
        }       
        private void ForLonWorks()
        {
            List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.LonWorksInterface)).ToList();   
            if(result.Count != 0)
            {
                FilterLonWorks = Visibility.Visible;
            }
            else
            {
                FilterLonWorks = Visibility.Collapsed;
            } 
        }
        public void LoadSystemSpecificEquipmentList()
        {
            try
            {
                //to check which system is created and call according data to load onto the tabs
                string systemType = CurrentSystem.HvacSystemType;
                GetFiltersVisbility(systemType);
                GetSystemsCount();

                EquipmentList.Clear();
                switch (systemType)
                {
                    case "1":
                        GetIduDisplayName();
                        PopulateImage(dtFillVrfImageType);
                        fillImages(dtFillVrfImageType);
                        break;

                    case "6":
                        GetControllers();
                        PopulateImage(allCentralControllers);
                        fillImages(allCentralControllers);
                        EventAggregator.GetEvent<FilterColor>().Publish();
                        break;

                    default:
                        break;
                }
                GetFiltersVisbility(systemType);

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        private void GetSystemsCount()
        {
            int HEcount = 0;
            int VRFcount = 0;

            HECount = 0;
            VRFCount = 0;
            foreach (RoomIndoor dr in Project.CurrentProject.ExchangerList)
            {
                var grp = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID == _currentSystem.Id);

                if (grp != null && dr.ControlGroupID.Count > 0)
                {
                    int index = dr.ControlGroupID.FindIndex(mm => mm.Equals(grp.Id));
                    if (index != -1)
                    {
                        continue;
                    }
                }

                foreach (var sys in Project.CurrentProject.HeatExchangerSystems)
                {
                    if (dr.SystemID == sys.Id)
                    {
                        if (sys.SystemStatus == Model.SystemStatus.VALID)
                        {
                            HEcount++;
                        }
                        HECount = HEcount;
                    }
                }
            }

            foreach(NextGenModel.SystemVRF sys in Project.CurrentProject.SystemListNextGen)
            {
                var grp = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID == _currentSystem.Id);

                if (grp != null && sys.ControlGroupID.Count > 0)
                {
                    int index = sys.ControlGroupID.FindIndex(mm => mm.Equals(grp.Id));
                    if (index != -1)
                    {
                        continue;
                    }

                }

                if (sys.SystemStatus == Model.SystemStatus.VALID)
                {
                    VRFcount++;
                }
                VRFCount = VRFcount;
            }            
            AllSystemCount = HECount + VRFCount;
        }

        private void UpdateUnitTypeUniversal(string SelectedUnitType)
        {
            _factory = indoorBLL.GetFCodeByDisUnitType(SelectedUnitType, out _typeList);
        }
        private void UpdateUnitType(string SelectedUnitType)
        {
            int i = SelectedUnitType.IndexOf("-");
            if (i > 0)
            {
                _factory = SelectedUnitType.Substring(i + 1, SelectedUnitType.Length - i - 1);
                _type = SelectedUnitType.Substring(0, i);
            }
            else
            {
                _factory = "";
                _type = SelectedUnitType;
            }
        }
        private void BindIDuModelType(DataTable IndoorTypeList)
        {
            dtFillVrfImageType = new DataTable();
            dtFillVrfImageType.Columns.Add(new DataColumn("TypeImage", typeof(System.String)));
            dtFillVrfImageType.Columns.Add(new DataColumn("UnitType", typeof(System.String)));
            dtFillVrfImageType.Columns.Add(new DataColumn("SelectionType", typeof(System.String)));
            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                if (IndoorTypeList.Rows.Count > 0)
                {                    
                    foreach (DataRow row in IndoorTypeList.Select())
                    {
                        if (!row["Display_Name"].ToString().Contains("Exchanger"))
                        { 
                            UpdateUnitTypeUniversal(row["Display_Name"].ToString());
                            IDuList = indoorBLL.GetUniversalIndoorListStd(row["Display_Name"].ToString(), _factory, _typeList);
                            if (IDuList.Rows.Count > 0)
                            {
                                foreach (DataRow rowidu in IDuList.Select())
                                {
                                    dtFillVrfImageType.Rows.Add(rowidu["TypeImage"].ToString(), row["Display_Name"].ToString(), "Indoor");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {

                if (IndoorTypeList.Rows.Count > 0)
                {
                    string colName = "UnitType";
                    foreach (DataRow row in IndoorTypeList.Select())
                    {
                        if (Convert.ToInt32(row["FactoryCount"].ToString()) > 1)
                        {
                            switch (row["FactoryCode"].ToString())
                            {
                                case "G":
                                    row[colName] += "-GZF";
                                    break;
                                case "E":
                                    row[colName] += "-HAPE";
                                    break;
                                case "Q":
                                    row[colName] += "-HAPQ";
                                    break;
                                case "B":
                                    row[colName] += "-HAPB";
                                    break;
                                case "I":
                                    row[colName] += "-HHLI";
                                    break;
                                case "M":
                                    row[colName] += "-HAPM";
                                    break;
                                case "S":
                                    row[colName] += "-SMZ";
                                    break;
                            }
                        }
                        UpdateUnitType(row["UnitType"].ToString());
                        IDuList = indoorBLL.GetIndoorListStd(_type, ODUProductType, _factory);
                        if (IDuList.Rows.Count > 0)
                        {
                            foreach (DataRow rowidu in IDuList.Select())
                            {
                                dtFillVrfImageType.Rows.Add(rowidu["TypeImage"].ToString(), row["UnitType"].ToString(), "Indoor");
                                break;
                            }
                        }
                    }
                }
                
            }
            //getPipingEquipments(ref dtFillVrfImageType);
        }

        public void getPipingEquipments(ref DataTable table)
        {
            //table.Rows.Add(new Object[] {  "YBranch_Seperator.png", "Y Branch Seperator", "Pipe" });
            table.Rows.Add(new Object[] {  "HeaderBranch_4Seperator.png", "Header Branch 4 Seperator", "HeaderBranch" }); // Changed Pipe to HeaderBranch For bug# 2959
            table.Rows.Add(new Object[] {  "HeaderBranch_8Seperator.png", "Header Branch 8 Seperator", "HeaderBranch" }); // Changed Pipe to HeaderBranch For bug# 2959
            table.Rows.Add(new Object[] {  "CHBox.png", "CHBox", "Pipe" });
            table.Rows.Add(new Object[] {  "MultiCHBox.png", "Multi CHBox", "Pipe" });
        }
        public void GetIduDisplayName()
        {
            ODUProductType = ((JCHVRF.Model.NextGen.SystemVRF)CurrentSystem).ProductType;
            
            //Start Backward Compatibility : Added a null/empty check to avoid Null reference exception.
            if (string.IsNullOrEmpty(ODUProductType))
            {
                ODUProductType = ((JCHVRF.Model.NextGen.SystemVRF)CurrentSystem)?.OutdoorItem?.ProductType;
            }
            //End Backward Compatibility : Added a null/empty check to avoid Null reference exception.

            if (ProjectBLL.IsSupportedUniversalSelection(JCHVRF.Model.Project.GetProjectInstance))
            {
                if (WorkFlowContext.CurrentSystem is NextGenModel.SystemVRF)
                {
                    var CurrSystem = WorkFlowContext.CurrentSystem as NextGenModel.SystemVRF;
                    //DataTable IndoorTypeList = indoorBLL.GetIndoorDisplayName();
                    var IndoorTypeList=indoorBLL.GetIndoorDisplayNameForODUSeries(CurrSystem.Series);
                    if (IndoorTypeList.Rows.Count > 0)
                    {
                        BindIDuModelType(IndoorTypeList);
                    }
                }
            }
            else
            {
                if (JCHVRF.Model.Project.GetProjectInstance == null || string.IsNullOrEmpty(JCHVRF.Model.Project.GetProjectInstance.SubRegionCode) || string.IsNullOrEmpty(ODUProductType))
                    return;
                DataTable dt = indoorBLL.GetIndoorFacCodeList(ODUProductType);
                if (dt.Rows.Count > 0)
                {
                    BindIDuModelType(dt);
                }
            }
            
            bool ChBoxCheckRecovery = false;
            if (JCHVRF.Model.Project.CurrentProject.IsCoolingModeEffective == true && JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective == true)
            {
                ChBoxCheckRecovery = true;
            }
            else if (JCHVRF.Model.Project.CurrentProject.IsHeatingModeEffective == true)
            {
                ChBoxCheckRecovery = true;
            }
            if (ChBoxCheckRecovery == true)
            {
                if (dtFillVrfImageType != null && dtFillVrfImageType.Rows.Count > 0)
                {
                    foreach (DataRow row in dtFillVrfImageType.Select())
                    {                        
                            if (row["UnitType"].ToString().Equals("CHBox") || row["UnitType"].ToString().Equals("Multi CHBox"))
                            {
                                    dtFillVrfImageType.Rows.Remove(row);
                            }                        
                    }
                }
            }
        }

        public void GetControllers()
        {
            if (Project.CurrentProject.SystemListNextGen != null)
            {
                foreach (var system in Project.CurrentProject.SystemListNextGen)
                {
                    if(system.OutdoorItem != null)
                    {
                        List<CentralController> centralControllers = controllerBLL.GetAvailableControllers(system.OutdoorItem.ProductType);
                        List<CentralController> addedControllers = allCentralControllers.ToList();
                        if (centralControllers != null)
                        {
                            foreach (var cc in centralControllers)
                            {
                                if (allCentralControllers.Contains(cc)) { continue; }
                                else
                                {
                                    addedControllers = allCentralControllers.ToList();
                                    bool flgControllerAdded = false;

                                    for (int i = 0; i < centralControllers.Count; i++)
                                    {
                                        if (addedControllers.Count > i)
                                        {
                                            if ((cc.Model == addedControllers[i].Model))
                                            {
                                                flgControllerAdded = true;
                                            }

                                        }
                                        else
                                        {
                                            allCentralControllers.Add(cc);
                                        }

                                    }
                                    if (!flgControllerAdded) { allCentralControllers.Add(cc); }
                                }

                            }
                        }                    
                    }
                }
            }

            if (Project.CurrentProject.HeatExchangerSystems != null)
            {
                foreach (var heSystem in Project.CurrentProject.ExchangerList)
                {
                    if (heSystem.IndoorItem != null)
                    {
                        RoomIndoor heatExchanger = Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(heSystem.SystemID));
                        List<CentralController> centralControllers;
                        if (Project.CurrentProject.RegionCode.Substring(0, 2) == "EU")
                        {
                            centralControllers = controllerBLL.GetAvailableControllers(heatExchanger.IndoorItem.Series);
                        }
                        else
                        {
                            centralControllers = controllerBLL.GetAvailableControllers(heatExchanger.IndoorItem.ProductType);
                        }
                        
                        List<CentralController> addedControllers = allCentralControllers.ToList();
                        if (centralControllers != null)
                        {
                            foreach (var cc in centralControllers)
                            {
                                if (allCentralControllers.Contains(cc)) { continue; }
                                else
                                {
                                    addedControllers = allCentralControllers.ToList();
                                    bool flgControllerAdded = false;
                                    for (int i = 0; i < centralControllers.Count; i++)
                                    {
                                        if (addedControllers.Count > i)
                                        {
                                            if ((cc.Model == addedControllers[i].Model))
                                            {
                                                flgControllerAdded = true;
                                            }
                                        }
                                        else
                                        {
                                            allCentralControllers.Add(cc);
                                        }
                                       
                                    }
                                    if (!flgControllerAdded) { allCentralControllers.Add(cc); }
                                 
                                }
                                
                            }
                        }
                    }                    
                }
            }
        }

        /// <summary>
        ///  Populate all the Images 
        /// </summary>
        /// <param name="dtEquipment"></param>
        private void PopulateImage(DataTable dtEquipment)
        {
            if (dtEquipment != null && dtEquipment.Rows.Count > 0)
            {
                var sourceDir = GetImagesSourceDir();

                foreach (DataRow dr in dtEquipment.Rows)
                {   
                        AddEquipment(dr, sourceDir);   
                }
            }
        }

        /// <summary>
        ///  Populate all the Images 
        /// </summary>
        /// <param name="dtEquipment"></param>
        private void PopulateImage(HashSet<CentralController> controllers)
        {
         
                var sourceDir = GetImagesSourceDir();

                foreach (CentralController dr in controllers)
                {
                    AddEquipment(dr, sourceDir);
                }
           
        }

        private string GetImagesSourceDir()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\TypeImages";
            //check which system is created and get images from appropriate places
            if (CurrentSystem.HvacSystemType.Equals("1"))
            {
                navigateToFolder = "..\\..\\Image\\TypeImages";
            }
            else if (CurrentSystem.HvacSystemType.Equals("6"))
            {
                navigateToFolder = "..\\..\\Image\\ControllerImage";
            }

            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        private void AddEquipment(DataRow dr, string sourceDir)
        {          
            string imageFullPath = sourceDir + "\\" + dr[0].ToString();
            if (File.Exists(imageFullPath))
            {                
                var imageData = new NextGenModel.ImageData();
                if (dr[1].ToString().Contains("Seperator"))
                {
                    imageData.imageName = dr[1].ToString().Substring(0, dr[1].ToString().LastIndexOf(" ")).Trim();
                }                
                else
                {
                    imageData.imageName = dr[1].ToString();
                }
                imageData.imagePath = imageFullPath;
                imageData.equipmentType = dr[2].ToString();
                EquipmentList.Add(imageData);                                
            }
        }

        private void AddEquipment(CentralController dr, string sourceDir)
        {
            string imageFullPath = sourceDir + "\\" + dr.Image;
            if (File.Exists(imageFullPath))
            {
                var imageData = new NextGenModel.ImageData();
                imageData.imageName = dr.Model;
                imageData.imagePath = imageFullPath;
                imageData.equipmentType = "Controller";
                imageData.Source = dr;
                EquipmentList.Add(imageData);
            }
        }

        private ObservableCollection<NextGenModel.ImageData> AddEquipment(RoomIndoor dr, string sourceDir)
        {
            string imageFullPath = sourceDir + "\\" + "u1483.png";
            int index = Project.CurrentProject.HeatExchangerSystems.FindIndex(x => x.Id == dr.SystemID);
            string modelname = "";
            if (File.Exists(imageFullPath) && index != -1)
            {
                string systemname = Project.CurrentProject.HeatExchangerSystems[index].Name;
                var imageData = new NextGenModel.ImageData();
                if (dr.IndoorItem != null)
                {
                    imageData.controllerLayoutType = dr.IndoorItem.Flag.ToString();
                    modelname = Project.CurrentProject.BrandCode == "H" ? dr.IndoorItem.Model_Hitachi : dr.IndoorItem.Model_York;
                }
                imageData.imageName = "\n" + systemname + "/" + modelname;
                imageData.imagePath = imageFullPath;
                imageData.SysStatusIcon = Project.CurrentProject.HeatExchangerSystems[index].StatusIcon;
                imageData.equipmentType = "System";
                imageData.Source = dr;
                imageData.UniqName = dr.SystemID;
                
                int indexinsert1 = HEEquipmentList.IndexOf(
                    HEEquipmentList.FirstOrDefault(mm => mm.imageName.Equals("VRF")));

                if (indexinsert1 != -1)
                {
                    HEEquipmentList.Insert(indexinsert1, imageData);
                }
                else
                {
                    if (HEEquipmentList.Count > 0)
                    {
                        if (!HEEquipmentList.Contains(imageData))
                        {
                            HEEquipmentList.Add(imageData);
                        }
                    }
                    else if (HEEquipmentList.Count == 0)
                    {
                        HEEquipmentList.Add(imageData);
                    }    
                }
            }
            return HEEquipmentList;
        }

        private ObservableCollection<NextGenModel.ImageData> AddEquipment(NextGenModel.SystemVRF dr, string sourceDir)
        {
            string imageFullPath = sourceDir + "\\" + "VRFoncc.png";
            string iducount = Project.CurrentProject.RoomIndoorList.FindAll(x => x.SystemID.Equals(dr.Id)).Count.ToString();
            var imageData = new NextGenModel.ImageData();
            imageData.imagePath = imageFullPath;
            imageData.imageName = dr.Name + "\n" + "IDU count " + iducount + "\n" + "ODU count " + "1";
            imageData.controllerLayoutType = dr.SysType.ToString();
            imageData.SysStatusIcon = dr.StatusIcon;
            imageData.equipmentType = "System";
            imageData.Source = dr;
            imageData.UniqName = dr.Id;
            int indexinsert1 = VRFEquipmentList.IndexOf(
                   VRFEquipmentList.FirstOrDefault(mm => mm.imageName.Equals("VRF")));

            if (indexinsert1 != -1)
            {
                VRFEquipmentList.Insert(indexinsert1, imageData);
            }
            else
            {
                if (VRFEquipmentList.Count > 0)
                {
                    if (!VRFEquipmentList.Contains(imageData))
                    {
                        VRFEquipmentList.Add(imageData);
                    }
                }
                else if (VRFEquipmentList.Count == 0)
                {
                    VRFEquipmentList.Add(imageData);
                }               
            }        
            return VRFEquipmentList;
        }

        private void fillImages(DataTable data)
        {
            if (data == null)
            {
                //imageThumbnails.ItemsSource = null;
                EquipmentList.Clear();                
                return;
            }
            if (data.Rows.Count > 0)
            {                
                var sourceDir = GetImagesSourceDir();
                EquipmentList.Clear();

                foreach (DataRow dr in data.Rows)
                {
                    AddEquipment(dr, sourceDir);
                }
            }
            else
                EquipmentList = new ObservableCollection<NextGenModel.ImageData>();
        }

        private void fillImages(IEnumerable<CentralController> data)
        {
            if (data == null)
            {
                //imageThumbnails.ItemsSource = null;
                EquipmentList.Clear();
                return;
            }

            var sourceDir = GetImagesSourceDir();

            EquipmentList.Clear();
            int indexNOBMS = 0;
            int indexBacknet = 0;
             int indexNXINT = 0;
            int indexLONW = 0;
            int indexModBus = 0;
            foreach (var item in data)
            {
                if ((item.Type == ControllerType.Software)|| (item.Type == ControllerType.ONOFF)|| (item.Type == ControllerType.CentralController))
                {
                    if (indexNOBMS == 1) continue;
                    AddSeparator("No BMS");
                    indexNOBMS++;
                    foreach (CentralController dr in data)
                    {
                        if ((dr.Type == ControllerType.Software) || (dr.Type == ControllerType.ONOFF) || (dr.Type == ControllerType.CentralController))
                        {
                            AddEquipment(dr, sourceDir);
                        }
                    }
                }
                else if(item.Type == ControllerType.BACNetInterface)
                {
                    if (indexBacknet == 1) continue;
                    AddSeparator("BACNet");
                    indexBacknet++;
                    foreach (CentralController dr in data)
                    {
                        if (dr.Type == ControllerType.BACNetInterface)
                        {
                            AddEquipment(dr, sourceDir);
                        }
                    }
                }
                else if (item.Type == ControllerType.KNXInterface)
                {
                    if (indexNXINT == 1)
                        continue;
                    AddSeparator("KNX");
                    indexNXINT++;
                    foreach (CentralController dr in data)
                    {
                        if (dr.Type == ControllerType.KNXInterface)
                        {
                            AddEquipment(dr, sourceDir);
                        }
                    }
                }
                else if (item.Type == ControllerType.LonWorksInterface)
                {
                    if (indexLONW == 1) continue;
                    AddSeparator("LonWorks");
                    indexLONW++;
                    foreach (CentralController dr in data)
                    {
                        if (dr.Type == ControllerType.LonWorksInterface)
                        {
                            AddEquipment(dr, sourceDir);
                        }
                    }
                }
                else if (item.Type == ControllerType.ModBusInterface)
                {
                    if (indexModBus == 1) continue; AddSeparator("ModBus");

                    indexModBus++;
                    foreach (CentralController dr in data)
                    {
                        if (dr.Type == ControllerType.ModBusInterface)
                        {
                            AddEquipment(dr, sourceDir);
                        }
                    }
                }

            }
           
          
            //foreach (CentralController dr in data)
            //{
            //    AddEquipment(dr, sourceDir);
            //}
        }

        private void fillImages(IEnumerable<RoomIndoor> data)
        {
            ObservableCollection<NextGenModel.ImageData> HEEquipmentListNew = new ObservableCollection<JCHVRF.Model.NextGen.ImageData>();
            HECount = 0;

            if (data == null)
            {
                //imageThumbnails.ItemsSource = null;
                EquipmentList.Clear();
                HEEquipmentList.Clear();
                return;
            }

            var sourceDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Image");
            if (Isallsystem == false)
            {
                EquipmentList.Clear();
                HEEquipmentList.Clear();
            }

            foreach (RoomIndoor dr in data)
            {
                if(CurrentSystem == null) return;
                                
                var grp = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID == CurrentSystem.Id);

                if (grp != null && dr.ControlGroupID.Count > 0)
                {
                    int index = dr.ControlGroupID.FindIndex(mm => mm.Equals(grp.Id));
                    if (index != -1)
                    {
                        continue;
                    }                    
                }

                foreach (var sys in Project.CurrentProject.HeatExchangerSystems)
                {
                    if(dr.SystemID == sys.Id)
                    {
                        if(sys.SystemStatus == Model.SystemStatus.VALID)
                        {                           
                            HEEquipmentListNew = AddEquipment(dr, sourceDir);
                            foreach (NextGenModel.ImageData img in HEEquipmentListNew)
                            {
                                int indexinsert1 = EquipmentList.IndexOf(EquipmentList.FirstOrDefault(mm => mm.imageName.Equals("VRF")));
                                if (indexinsert1 != -1)
                                {
                                    if (!EquipmentList.Contains(img))
                                    {
                                        EquipmentList.Insert(indexinsert1, img);
                                    }
                                }
                                else
                                {
                                    if (EquipmentList.Count > 0)
                                    {
                                        if (!EquipmentList.Contains(img))
                                        {
                                            EquipmentList.Add(img);
                                            HECount += 1;
                                        }
                                    }
                                    else if (EquipmentList.Count == 0)
                                    {
                                        EquipmentList.Add(img);
                                        HECount += 1;
                                    }
                                }
                            }                           
                        }
                    }
                }
            }
            AllSystemCount = VRFCount + HECount;
        }

        private void fillImages(IEnumerable<NextGenModel.SystemVRF> data)
        {
            ObservableCollection<NextGenModel.ImageData> VRFEquipmentListNew = new ObservableCollection<JCHVRF.Model.NextGen.ImageData>();
            VRFCount = 0;
            if (data == null)
            {
                //imageThumbnails.ItemsSource = null;
                EquipmentList.Clear();
                VRFEquipmentList.Clear();
                return;
            }

            var sourceDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Image");
            if (Isallsystem == false)
            {
                EquipmentList.Clear();
                VRFEquipmentList.Clear();
            }

            foreach (NextGenModel.SystemVRF dr in data)
            {
                if(CurrentSystem == null) return;
                var grp = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID == _currentSystem.Id);

                if (grp != null && dr.ControlGroupID.Count > 0)
                {
                    int index = dr.ControlGroupID.FindIndex(mm => mm.Equals(grp.Id));
                    if (index != -1)
                    {
                        continue;
                    }
                }

                if (dr.SystemStatus == Model.SystemStatus.VALID)
                {
                    VRFEquipmentListNew = AddEquipment(dr, sourceDir);
                    foreach (NextGenModel.ImageData img in VRFEquipmentListNew)
                    {
                        int indexinsert1 = EquipmentList.IndexOf(EquipmentList.FirstOrDefault(mm => mm.imageName.Equals("VRF")));
                        if (indexinsert1 != -1)
                        {
                            if (!EquipmentList.Contains(img))
                            {
                                EquipmentList.Insert(indexinsert1 + 1, img);
                            }                           
                        }
                        else
                        {
                            if (EquipmentList.Count > 0)
                            {
                                if (!EquipmentList.Contains(img))
                                {
                                    EquipmentList.Add(img);
                                }
                            }
                            else if (EquipmentList.Count == 0)
                            {
                                EquipmentList.Add(img);
                            }
                        }
                    }
                    VRFCount += 1;
                }               
            }
            AllSystemCount = VRFCount + HECount;
        }


        public enum equipmentTypeButtonClick
        {
            All = 0,
            IDU = 1,
            ODU = 2,
            Pipe = 3,
        }

        private void onSearchVRFEquipmentList(string b)
        {
            DataRow[] filteredRows;
            switch (enumEQSrch)
            {

                case equipmentTypeButtonClick.All:
                    filteredRows = dtFillVrfImageType.Rows.Cast<DataRow>().Where(r => r.ItemArray.Any(
                          c => c.ToString().IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();

                    if (filteredRows.Length > 0)
                    {
                        fillImages(filteredRows.CopyToDataTable());
                    }
                    else fillImages((DataTable)null);
                    return;
                case equipmentTypeButtonClick.IDU:

                    filteredRows = dtFillVrfImageType.Rows.Cast<DataRow>().Where(z => z.Field<string>("SelectionType") == "Indoor").Where(r => r.ItemArray.Any(
                      c => c.ToString().IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();

                    if (filteredRows.Length > 0)
                    {
                        fillImages(filteredRows.CopyToDataTable());
                    }
                    else fillImages((DataTable)null);
                    return;

                case equipmentTypeButtonClick.ODU:

                    filteredRows = dtFillVrfImageType.Rows.Cast<DataRow>().Where(z => z.Field<string>("SelectionType") == "Outdoor").Where(r => r.ItemArray.Any(
                          c => c.ToString().IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();

                    if (filteredRows.Length > 0)
                    {
                        fillImages(filteredRows.CopyToDataTable());
                    }
                    else fillImages((DataTable)null);
                    return;
                case equipmentTypeButtonClick.Pipe:


                    filteredRows = dtFillVrfImageType.Rows.Cast<DataRow>().Where(r => r.ItemArray.Any(
                   c => c.ToString().IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();

                    if (filteredRows.Length > 0)
                    {
                        fillImages(filteredRows.CopyToDataTable());
                    }
                    else fillImages((DataTable)null);
                    return;

            }
        }

        private void onSearchControllerEquipmentList(string b)
        {
            try
            {
                List<CentralController> filteredRows;
                switch (enumCCSrch)
                {

                    case ControllerLayoutType.ALL:
                        filteredRows = allCentralControllers.Where(r => r.Model.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                        if(filteredRows.Count <= 0)
                        {
                            JCHMessageBox.Show("Entered Controller is not not found");
                        }
                        else
                        {
                            fillImages(filteredRows);
                        }
                        return;

                    case ControllerLayoutType.BACNET:

                        filteredRows = allCentralControllers.Where(r => r.Type == ControllerType.BACNetInterface).Where(r => r.Model.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                        if (filteredRows.Count <= 0)
                        {
                            JCHMessageBox.Show("Entered Controller is not not found");
                        }
                        else
                        {
                            fillImages(filteredRows);
                        }
                        return;

                    case ControllerLayoutType.KNX:

                        filteredRows = allCentralControllers.Where(r => r.Type == ControllerType.KNXInterface).Where(r => r.Model.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                        if (filteredRows.Count <= 0)
                        {
                            JCHMessageBox.Show("Entered Controller is not not found");
                        }
                        else
                        {
                            fillImages(filteredRows);
                        }
                        return;

                    case ControllerLayoutType.LONWORKS:

                        filteredRows = allCentralControllers.Where(r => r.Type == ControllerType.LonWorksInterface).Where(r => r.Model.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                        if (filteredRows.Count <= 0)
                        {
                            JCHMessageBox.Show("Entered Controller is not not found");
                        }
                        else
                        {
                            fillImages(filteredRows);
                        }
                        return;

                    case ControllerLayoutType.MODBUS:

                        filteredRows = allCentralControllers.Where(r => r.Type == ControllerType.ModBusInterface).Where(r => r.Model.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                        if (filteredRows.Count <= 0)
                        {
                            JCHMessageBox.Show("Entered Controller is not not found");
                        }
                        else
                        {
                            fillImages(filteredRows);
                        }
                        return;

                    case ControllerLayoutType.NoBMS:

                        filteredRows = allCentralControllers.Where(r => r.Type == ControllerType.ONOFF || r.Type == ControllerType.Software || r.Type == ControllerType.CentralController).
                            Where(r => r.Model.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                        if (filteredRows.Count <= 0)
                        {
                            JCHMessageBox.Show("Entered Controller is not not found");
                        }
                        else
                        {
                            fillImages(filteredRows);
                        }
                        return;

                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnSearchClick()
        {
            try
            { 
            string b = SearchText;

            if (allCentralControllers != null)
            {
                
                if(CurrentSystem.HvacSystemType.Equals("1"))
                {
                    onSearchVRFEquipmentList(b);
                }
               
                else if(CurrentSystem.HvacSystemType.Equals("6"))
                {
                    onSearchControllerEquipmentList(b);
                }                    
            }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnEquipmentFiltered()
        {
            RaisePropertyChanged(nameof(IsAllSelected));
            RaisePropertyChanged(nameof(IsIDUSelected));
            RaisePropertyChanged(nameof(IsPipeSelected));
            try
            {
                var result = dtFillVrfImageType.AsEnumerable();
                switch (enumEQSrch)
                {
                    case equipmentTypeButtonClick.All:
                        break;
                    case equipmentTypeButtonClick.IDU:
                        result = result.Where(myRow => myRow.Field<string>("SelectionType") == "Indoor");
                        break;
                    case equipmentTypeButtonClick.ODU:
                        break;
                    case equipmentTypeButtonClick.Pipe:
                        result = result.Where(myRow => myRow.Field<string>("SelectionType") == "HeaderBranch" || myRow.Field<string>("SelectionType") == "Pipe");
                        break;
                }

            if (result.AsDataView<DataRow>().Count > 0)
            {
                var res = result.CopyToDataTable<DataRow>();
                fillImages(res);
            }
            else
                fillImages((DataTable)null);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
      
        private bool Isallsystem = false;
        public void OnAllSysClick()
        {            
            EquipmentListBckClr = "#FFADD8E6";
            Isallsystem = true;
            try
            {
                EquipmentList.Clear();             
                AddSeparator("Heat Exchanger");
                HEEquipmentList.Clear();
                OnHEClick();
                AddSeparator("VRF");
                VRFEquipmentList.Clear();
                OnVRFClick();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
            finally
            {
                Isallsystem = false;
                HEclicked = true;
                VRFclicked = true;
            }
        }
        private void AddSeparator(string sepratorName)
        {         
            NextGenModel.ImageData imgV = new JCHVRF.Model.NextGen.ImageData();
            imgV.imageName = sepratorName;
            imgV.UniqName = "";
            EquipmentList.Add(imgV);
        }

        private void OnHEClick()
        {
            HEclicked = true;
            VRFclicked = false;
            //Isallsystem = false;
            EquipmentListBckClr = "#FFADD8E6";
            try
            {
                List<RoomIndoor> result = Project.CurrentProject.ExchangerList;
                if (result.Count() > 0)
                {
                    fillImages(result);
                }
                else if(Isallsystem == false)
                {
                    fillImages((List<RoomIndoor>)null);
                }                
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnVRFClick()
        {
            HEclicked = false;
            VRFclicked = true;
            EquipmentListBckClr = "#FFADD8E6";
            try
            {
                List<NextGenModel.SystemVRF> result = Project.CurrentProject.SystemListNextGen;
                if (result.Count() > 0)
                {
                    fillImages(result);
                }
                else if(Isallsystem == false)
                {
                    fillImages((List<NextGenModel.SystemVRF>)null);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        //For showing Central Controller Equipment List
        private void OnCCClick()
        {
            HEclicked = false;
            VRFclicked = false;
            EquipmentListBckClr = "White";
            try
            { 
            enumCCSrch = ControllerLayoutType.ALL;
            if (allCentralControllers == null)
            {
                return;
            }
            //ToDo: Add Central Controller list
            HashSet<CentralController> result = allCentralControllers;
            
                fillImages(result);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private void OnBACNETClick()
        {
            HEclicked = false;
            VRFclicked = false;
            EquipmentListBckClr = "White";
            try
            {
                if (allCentralControllers == null)
                {
                    return;
                }  
                enumCCSrch = ControllerLayoutType.BACNET;
                List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.BACNetInterface)).ToList();            
                if (result.Count() > 0)
                {
                    fillImages(result);
                }
                else
                {
                    fillImages((List<CentralController>)null);
                }                    
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        /// <summary>
        /// Filter for MODBUS
        /// </summary>
        private void OnMODBUSClick()
        {
            HEclicked = false;
            VRFclicked = false;
            EquipmentListBckClr = "White";
            try
            {
                if (allCentralControllers == null)
                {
                    return;
                } 
                enumCCSrch = ControllerLayoutType.MODBUS;
                List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.ModBusInterface)).ToList();
                if (result.Count() > 0)
                {                
                    fillImages(result);
                }
                else
                {
                     fillImages((List<CentralController>)null);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void OnKNXClick()
        {
            HEclicked = false;
            VRFclicked = false;
            EquipmentListBckClr = "White";
            try
            {
                if (allCentralControllers == null)
                {
                    return;
                }
                enumCCSrch = ControllerLayoutType.KNX;
                var result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.KNXInterface)).ToList();

                if (result.Count() > 0)
                {                
                    fillImages(result);
                }
                else
                {
                    fillImages((List<CentralController>)null);
                }                
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void OnLonWorksClick()
        {
            HEclicked = false;
            VRFclicked = false;
            EquipmentListBckClr = "White";
            try
            { 
                if (allCentralControllers == null)
                {
                    return;
                }
                enumCCSrch = ControllerLayoutType.LONWORKS;
                var result = allCentralControllers.AsEnumerable().Where(myRow => myRow.Type.Equals(ControllerType.LonWorksInterface)).ToList();

                if (result.Count() > 0)
                {                
                    fillImages(result);
                }
                else
                {
                    fillImages((List<CentralController>)null);
                }                    
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void OnNOBMSClick()
        {
            HEclicked = false;
            VRFclicked = false;
            EquipmentListBckClr = "White";
            try
            { 
                if (allCentralControllers == null)
                {
                    return;
                }
                enumCCSrch = ControllerLayoutType.NoBMS;
                List<CentralController> result = allCentralControllers.Where(myRow => myRow.Type.Equals(ControllerType.CentralController) || myRow.Type.Equals(ControllerType.ONOFF) || myRow.Type.Equals(ControllerType.Software)).ToList();

                if (result.Count() > 0)
                {                
                    fillImages(result);
                }
                else
                {
                    fillImages((List<CentralController>)null);
                }                    
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }       
    }
}
