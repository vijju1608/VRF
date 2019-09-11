using JCHVRF.BLL;
using JCHVRF.BLL.New;
using JCHVRF.Entity;
using JCHVRF.Model;
using JCHVRF_New.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using JCHVRF.VRFMessage;
using System.Windows.Forms;
using JCBase.UI;
using JCHVRF_New.Common.Helpers;
using Prism.Regions;
using NextGenModel = JCHVRF.Model.NextGen;
using Prism.Events;
using Prism.Commands;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{
    public class AddAccessoriesTemplateViewModel : ViewModelBase
    {

        #region fields
        public RoomIndoor objIndoor;
        private RoomIndoor _selectRoomIndoor;
        public DataTable dtDisplayName;
        private ObservableCollection<Accessory> _accessories;
        private ObservableCollection<Accessory> _listAccessory;
        private List<Accessory> _selectedAccessoryList;
        private List<Accessory> _selectedAccessoryListAll;
        private List<Accessory> _applyToSimilarIDUAccessoryList;
        private Accessory _selectedAccessory;
        AccessoryBLL bll = new AccessoryBLL();
        IEventAggregator eventaggregator;
        IGlobalProperties _globalProperties;
        bool AccessoryMaxCountError = true;
        string RemoteControl = "Remote Control Switch";                        //控制器
        string ShareRemoteControl = "Share Remote Control";                    //共享控制器
        string HalfRemoteControl = "Half-size Remote Control Switch";
        string ReceiverRemoteControl = "Receiver Kit for Wireless Control";
        string WirelessRemoteControl = "Wireless Remote Control Switch";
        string ReceiverKit = "Receiver Kit";                                   //无线控制器
        string Panel = "Panel";                                                //面板
        string Filter = "Filter";                                              //过滤器
        string FilterBox = "Filter Box";                                       //过滤盒
        string DrainUp = "Drain-up";                                           //排水
        string Others = "Others";
        string DividingLine = "\n----------------------------------------\n";  //换行显示分割线
        string br = "\n";                                                      //换行符
        string BuiltInIndoor = "Built in at Indoor unit";
        bool flag = false;
        string airPanelWith = "Air Panel with motion sensor";
        string airPanelWo = "Air Panel w/o motion sensor";
        bool wireless = false;
        string[] typeName1 =
         {
            "Wireless Remote Control Switch",
            //"T-Tube Connecting Kit", 
            //"Deodorant Air Filter", 
            "Long-life Filter Kit",
            "Long-life Filter Kit", 
            //"Antibacterial Long Life Air Filter",
            "Antibacterial Long-life Filter",
            "Anti-bacterial Air Filter",
            "Kit for Deodorant Filter (Deodorant Filter)",
            "Antibacterial Long-life Filter",
            "Antibacterial Air Filter"
        };

        string[] typeName2 =
        {
            "Receiver Kit for Wireless Control",//Wireless Receiver Kit
            //"Fresh Air Intake Kit", 
            //"Filter Box",
            "Filter Box",
            "Filter Box",
            //"Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Filter Box"

        };
        string[] unitType =
        {
            "",
            //"Four Way Cassette",
            //"Four Way Cassette",
            "Medium Static Ducted",
            "High Static Ducted",
            //"Two Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Two Way Cassette",
            "Ceiling Suspended (NA)"
        };

        string[] typeName1EU =
        {
            "Wireless Remote Control Switch",
            //"T-Tube Connecting Kit", 
            //"Deodorant Air Filter", 
            "Long-Life Filter Kit",
            "Long-Life Filter Kit",
            "Long-Life Filter Kit", 
            //"Antibacterial Long Life Air Filter",
            "Antibacterial Long-life Filter",
            "Anti-bacterial Air Filter",
            "Kit for Deodorant Filter (Deodorant Filter)",
            "Antibacterial Long-life Filter",
            "Antibacterial Air Filter"
        };
        string[] typeName2EU =
        {
            "Receiver Kit for Wireless Control",//Wireless Receiver Kit
            //"Fresh Air Intake Kit", 
            //"Filter Box",
            "Filter Box",
            "Filter Box",
            "Filter Box",
            //"Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Filter Box"

        };
        string[] unitTypeEU =
        {
            "",
            //"Four Way Cassette",
            //"Four Way Cassette",
            "Ceiling Suspended",
            "Four Way Cassette",
            "Two Way Cassette",
            //"Two Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Two Way Cassette",
            "Ceiling Suspended (NA)"
        };
        string[] AccUnitType =
        {
                "Receiver Kit",
                "Panel",
                "Filter",
                "Filter Box",
                "Drain-up",
                "Others"
        };
        private static Dictionary<string, int> accType = new Dictionary<string, int>()
        { {"Half-size Remote Control Switch",0 },
            {"Remote Control Switch",0},
            { "Wireless Remote Control Switch",0 },
            { "Receiver Kit for Wireless Control",0} };
        //超出长度的室内机配件名称
        string[] maxAccessoryType = {
                "抗菌加工高清淨濾網(比色法65%)",
                "Outdoor air inlet kit (single inlet)",
                "Kit for Deodorant Filter (Filter Box)",
                "Indoor wired room temperature sensor",
                "Universal water temperature sensor",
                "Differential pressure overflow valve",
                "Remote temperature sensor (THM4)",
                "Kit for Deodorant Filter (Deodorant Filter)",
                "Receiver kit for wireless remote control",
                "Wired Thermostat  PC-ARFWE for HYDRO FREE",
                "2nd zone mixing kit (Wall mounted model)",
                "Primary Filter Kit + Activated Carbon Filter Kit",
                "Duct connecting flange for outdoor air outlet",
                "Outdoor air inlet T-shaped duct connection kit",
                "3-way valve (Internal thread and spring return)",
                "Receiver kit for wireless remote control(On the wall)",
                "Receiver kit for wireless remote control(On the panel)",
                "Wireless ON/OFF thermostat (Receiver + Room thermostat)",
                "Flexible water pipe for a high temperature HYDRO FREE"
         };
        //High Wall High Wall (w/o EXV)
        string[] HighWallType =
        {
              "High Wall",
              "High Wall (w/o EXV)"
        };

        #endregion

        #region properties
        public string BrandCode { get; set; }
        public string Region { get; set; }
        public ObservableCollection<Accessory> ListAccessory
        {
            get
            {
                if (_listAccessory == null)
                    _listAccessory = new ObservableCollection<Accessory>();
                return _listAccessory;
            }
            set { this.SetValue(ref _listAccessory, value); }
        }

        public ObservableCollection<Accessory> Accessories
        {

            get
            {
                if (_accessories == null)
                {
                    _accessories = new ObservableCollection<Accessory>();
                    //return GetSelectedIDUAccessories(objIndoor);
                }
                return _accessories;
            }
            set
            {
                this.SetValue(ref _accessories, value);
            }
        }


        public RoomIndoor SelectedRoomIndoor
        {
            get
            {
                return _selectRoomIndoor = objIndoor;
            }
            set
            {
                this.SetValue(ref _selectRoomIndoor, value);
            }
        }

        public List<Accessory> SelectedAccessoryList
        {
            get
            {
                //if(_selectedAccessoryList==null)
                //    _selectedAccessoryList= new ObservableCollection<Accessory>();
                return _selectedAccessoryList;
            }
            set
            {
                this.SetValue(ref _selectedAccessoryList, value);
            }

        }

        public List<Accessory> AccessoryListAll
        {
            get
            {
                return _selectedAccessoryListAll;
            }
            set
            {
                this.SetValue(ref _selectedAccessoryListAll, value);
            }

        }
        private List<int> _similarIDUonCanvas;
        public List<int> similarIDUonCanvas
        {
            get
            {
                if (_similarIDUonCanvas == null)
                {
                    new List<int>();
                }
                return _similarIDUonCanvas;
            }
            set
            {
                this.SetValue(ref _similarIDUonCanvas, value);
            }

        }
        //List<int> similarIDUonCanvas = new List<int>();
        public List<Accessory> ApplyToSimilarIDUAccessoryList
        {
            get
            {
                return _applyToSimilarIDUAccessoryList;
            }
            set
            {
                this.SetValue(ref _applyToSimilarIDUAccessoryList, value);
            }

        }

        public Accessory SelectedAccessory
        {
            get
            {
                if (_selectedAccessory == null)
                    _selectedAccessory = new Accessory();
                return _selectedAccessory;
            }
            set
            {
                this.SetValue(ref _selectedAccessory, value);
            }

        }
        #endregion

        #region delegateproperties

        /// <summary>
        /// Gets or sets the Cancelcommand
        /// </summary>
        public DelegateCommand Cancelcommand { get; set; }

        /// <summary>
        /// Gets or sets the AddAccessoriesToIDUCommand
        /// </summary>
        public DelegateCommand AddAccessoriesToIDUCommand { get; set; }

        /// <summary>
        /// Gets or sets the AddAccessoriesToIDUCommand
        /// </summary>
        public DelegateCommand RemoveFromAllCommand { get; set; }
        public DelegateCommand CheckAllDependentAccessoryCommand { get; set; }
        public DelegateCommand UncheckAllDependentAccessoryCommand { get; set; }
        public DelegateCommand CountIncreaseCommand { get; set; }
        #endregion
        public AddAccessoriesTemplateViewModel(IEventAggregator _eventaggregator, IModalWindowService winService, IGlobalProperties globalProperties = null)
        {
            try
            {
                this.eventaggregator = _eventaggregator;
                this._globalProperties = globalProperties;
                this._winService = winService;
                if (_winService != null)
                {
                    if (_winService.GetParameters(ViewKeys.AddAccessories) != null)
                    {
                        objIndoor = _winService.GetParameters(ViewKeys.AddAccessories)["IndoorObject"] as RoomIndoor;
                    }
                }
                Cancelcommand = new DelegateCommand(OnCancelClicked);
                AddAccessoriesToIDUCommand = new DelegateCommand(OnAddAccessoriesToIDUCommandClick);
                RemoveFromAllCommand = new DelegateCommand(OnRemoveFromAllCommandClick);
                CheckAllDependentAccessoryCommand = new DelegateCommand(CheckAllDependentAccessoryCommandEvent);
                UncheckAllDependentAccessoryCommand = new DelegateCommand(UncheckAllDependentAccessoryCommandEvent);
                CountIncreaseCommand = new DelegateCommand(CountIncreaseCommandClick);
                Project.CurrentProject = Project.GetProjectInstance;
                if (_globalProperties.DefaultAccessoryDictionary == null)
                    _globalProperties.DefaultAccessoryDictionary = new Dictionary<int, HashSet<int>>();
                
                if (!_globalProperties.DefaultAccessoryDictionary.ContainsKey(Project.GetProjectInstance.projectID))
                        _globalProperties.DefaultAccessoryDictionary.Add(Project.GetProjectInstance.projectID, new HashSet<int>());
                

                if (objIndoor != null)
                    GetAccessory(objIndoor);


            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void CheckAllDependentAccessoryCommandEvent()
        {
            try
            {
                if (!MaxTypeCountCheck())
                    return;

                string newType = GetNewTypeName(SelectedAccessory.Type);
                //if (Project.CurrentProject.RegionCode == "EU_W" ||
                //    Project.CurrentProject.RegionCode == "EU_S" ||
                //    Project.CurrentProject.RegionCode == "EU_E" ||
                //    Project.CurrentProject.SubRegionCode == "TW")
                //{
                //    var checkIsSameModel = (from itms in ListAccessory where itms.Model == SelectedAccessory.Model && itms.IsSelect == true select itms).ToList();
                //    if (checkIsSameModel.Count > 1)
                //    {   //JCHMessageBox.Show("Indoor unit already exists the accessories,click ok button below interface will be deleted, reconfigure");
                //        JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_ACCESSORIES_EXISTS"));
                //        ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).IsSelect = false;
                //        return;
                //    }
                //}
                var item = (from s in ListAccessory where s.Type == newType select s.Type).ToList();
                CheckAccessryCount(item.Count);
                if (!string.IsNullOrEmpty(newType) && item.Count > 0 && !flag)
                {
                    //Accessory newItem = bll.GetItem(newType, objIndoor.IndoorItem, Project.CurrentProject.RegionCode, Project.CurrentProject.SubRegionCode);
                    if (item.Count > 1)
                    {
                        if (!(ListAccessory.FirstOrDefault(a => a.Type == newType).IsSelect))
                        {
                            var count = ListAccessory.Where(a => a.Type == newType && a.Model.Contains(SelectedAccessory.Model.Substring(SelectedAccessory.Model.Length - 6))).Select(a => a).ToList().Count();
                            if (count > 0)
                            {
                                ListAccessory.FirstOrDefault(a => a.Type == newType && a.Model.Contains(SelectedAccessory.Model.Substring(SelectedAccessory.Model.Length - 6))).IsSelect = true;
                                ListAccessory.FirstOrDefault(a => a.Type == newType && a.Model.Contains(SelectedAccessory.Model.Substring(SelectedAccessory.Model.Length - 6))).Count = 1;

                                ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model.Contains(SelectedAccessory.Model.Substring(SelectedAccessory.Model.Length - 6))).IsSelect = true;
                                ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model.Contains(SelectedAccessory.Model.Substring(SelectedAccessory.Model.Length - 6))).Count = 1;
                            }
                            else
                            {
                                ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).IsSelect = true;
                                ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).Count = 1;
                                ListAccessory.FirstOrDefault(a => a.Type == newType).IsSelect = true;
                                ListAccessory.FirstOrDefault(a => a.Type == newType).Count = 1;
                            }
                        }
                    }
                    else
                    {
                        ListAccessory.FirstOrDefault(a => a.Type == newType).IsSelect = true;
                        ListAccessory.FirstOrDefault(a => a.Type == newType).Count = 1;

                        ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).IsSelect = true;
                        ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).Count = 1;
                    }

                }

                //foreach (var item in ListAccessory)
                //{
                //    if (item.Type == "Wireless Remote Control Switch" && item.IsSelect)

                //    {
                //        ListAccessory.FirstOrDefault(a => a.Type == "Receiver Kit for Wireless Control").Count = 1;
                //        ListAccessory.FirstOrDefault(a => a.Type == "Receiver Kit for Wireless Control").IsSelect = true;
                //    }
                //    else if (item.Type == "Receiver Kit for Wireless Control" && item.IsSelect)
                //    {
                //        ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").Count = 1;
                //        ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").IsSelect = true;
                //    }
                //}
            }
            catch (Exception ex)
            {

            }

        }


        private void UncheckAllDependentAccessoryCommandEvent()
        {
            string newType = GetNewTypeName(SelectedAccessory.Type);
            var item = (from s in ListAccessory where s.Type == newType select s.Type).ToList();

            if (SelectedAccessory.Type == "Half-size Remote Control Switch" || SelectedAccessory.Type == "Remote Control Switch")
            {
                accType[SelectedAccessory.Type] = 0;
            }
            else if (SelectedAccessory.Type == "Wireless Remote Control Switch"
                    || SelectedAccessory.Type == "Receiver Kit for Wireless Control")
            {
                accType["Wireless Remote Control Switch"] = 0;
                accType["Receiver Kit for Wireless Control"] = 0;
            }
            if (!string.IsNullOrEmpty(newType) && item.Count > 0)
            {

                //Accessory newItem = bll.GetItem(newType, objIndoor.IndoorItem, Project.CurrentProject.RegionCode, Project.CurrentProject.SubRegionCode);

                if (ListAccessory.FirstOrDefault(a => a.Type == newType).IsSelect)
                {
                    ListAccessory.FirstOrDefault(a => a.Type == newType).IsSelect = false;
                    ListAccessory.FirstOrDefault(a => a.Type == newType).Count = 0;

                    ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).IsSelect = false;
                    ListAccessory.FirstOrDefault(a => a.Type == SelectedAccessory.Type && a.Model == SelectedAccessory.Model).Count = 0;
                }


            }
        }

        private void CountIncreaseCommandClick()
        {
            if (!AccessoryMaxCountError)
            {
                if (!MaxTypeCountCheck())
                {
                    AccessoryMaxCountError = true;
                    return;
                }

            }

            string newType = GetNewTypeName(SelectedAccessory.Type);
            var item = (from s in ListAccessory where s.Type == newType select s.Type).ToList();
            if (SelectedAccessory.Type != null && SelectedAccessory.Count != null && SelectedAccessory.IsSelect)
            {
                if ((SelectedAccessory.Type == "Wireless Remote Control Switch" && SelectedAccessory.Count == 0)
                    || (SelectedAccessory.Type == "Receiver Kit for Wireless Control" && SelectedAccessory.Count == 0))
                {
                    accType["Wireless Remote Control Switch"] = 0;
                    accType["Receiver Kit for Wireless Control"] = 0;
                }
                else if ((SelectedAccessory.Type == "Wireless Remote Control Switch" && SelectedAccessory.Count == 1)
                    || (SelectedAccessory.Type == "Receiver Kit for Wireless Control" && SelectedAccessory.Count == 1))
                {
                    if (item.Count > 0)
                    {
                        accType["Wireless Remote Control Switch"] = 1;
                        accType["Receiver Kit for Wireless Control"] = 1;
                    }
                    else
                    {
                        accType[SelectedAccessory.Type] = 1;
                    }
                }
                else
                    accType[SelectedAccessory.Type] = SelectedAccessory.Count;
                CheckAccessryCount();
            }
            //foreach (var item in ListAccessory)
            //{
            //    //CheckAccessoryCount(item);
            //    if (item.Type == "Wireless Remote Control Switch")
            //    {
            //        ListAccessory.FirstOrDefault(a => a.Type == "Receiver Kit for Wireless Control").Count = item.Count;
            //        if (item.Count == 0)
            //        {
            //            ListAccessory.FirstOrDefault(a => a.Type == "Receiver Kit for Wireless Control").IsSelect = false;
            //            item.IsSelect = false;
            //        }
            //        else
            //        {
            //            ListAccessory.FirstOrDefault(a => a.Type == "Receiver Kit for Wireless Control").IsSelect = true;
            //            item.IsSelect = true;
            //        }

            //    }
            //    else if (item.Type == "Receiver Kit for Wireless Control")
            //    {
            //        ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").Count = item.Count;
            //        if (item.Count == 0)
            //        {
            //            ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").IsSelect = false;
            //            item.IsSelect = false;
            //        }
            //        else
            //        {
            //            ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").IsSelect = true;
            //            item.IsSelect = true;
            //        }
            //    }
            //}
        }

        #region priavate methods
        private void OnCancelClicked()
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.Title == "Accessories")
                    window.Close();
            }
        }

        private void RefreshAccessories(bool iduTypeChange)
        {
            if (iduTypeChange)
            {
                SelectedAccessoryList = null;
                ApplyToSimilarIDUAccessoryList = null;
                //ListAccessory = null;
            }
        }

        string _factory = "";
        private IModalWindowService _winService;

        private void UpdateUnitType(string SelectedUnitType)
        {
            int i = SelectedUnitType.IndexOf("-");
            if (i > 0)
            {
                _factory = SelectedUnitType.Substring(i + 1, SelectedUnitType.Length - i - 1);
            }
            else
            {
                _factory = "";
            }
        }

        public void GetFactoryCode(string modelFull)
        {
            _factory = modelFull.Substring(modelFull.Length - 1, 1); ;
            //return factoryCode;
        }


        private void OnAddAccessoriesToIDUCommandClick()
        {
            try
            {
                if (ListAccessory != null)
                {
                    bool FlageAdd = false;
                    bool FlageCurrent = false;
                    List<Accessory> Acceslist = new List<Accessory>();

                    if (objIndoor.ListAccessory == null)
                    {
                        objIndoor.ListAccessory = new List<Accessory>();
                    }

                    List<int> CountAdd = (from a in ListAccessory where a.Count > 0 select a.Count).ToList();
                    List<bool> SelectAcc = (from a in ListAccessory where a.IsSelect == true || a.IsApplyToSimilarUnit == true select a.IsSelect).ToList();
                    if (SelectAcc.Count == 0)
                    {
                        JCHMessageBox.Show(Langauge.Current.GetMessage("ACCESSORY_SELECTION"));// "Please select atleast one accessory"
                        return;
                    }
                    if (CountAdd.Count > 0)
                    {
                        SelectedAccessoryList = (from s in ListAccessory where s.IsSelect == true && s.IsApplyToSimilarUnit == false select s).ToList();
                        ApplyToSimilarIDUAccessoryList = (from a in ListAccessory where a.IsApplyToSimilarUnit == true && a.IsSelect == false select a).ToList();
                        AccessoryListAll = (from s in ListAccessory where s.IsSelect == true && s.IsApplyToSimilarUnit == true select s).ToList();
                        similarIDUonCanvas = (from idu in Project.CurrentProject.RoomIndoorList where objIndoor.IndoorItem.Model == idu.IndoorItem.Model && objIndoor.IndoorNO != idu.IndoorNO && objIndoor.SystemID == idu.SystemID select idu.IndoorNO).ToList();//Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => (objIndoor.IndoorFullName == a.IndoorFullName));
                        bool flageAlAdd = false;                                                                                                                                                                                                                                //Start Select IDU List
                        if (SelectedAccessoryList.Count > 0)
                        {
                            foreach (Accessory accessory in SelectedAccessoryList)
                            {
                                if (flageAlAdd == false)
                                {
                                    Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory = new List<Accessory>();
                                    flageAlAdd = true;
                                }
                                if (accessory.Count > 0)
                                {

                                    if (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory == null)
                                    {
                                        Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory = new List<Accessory>();
                                    }
                                    if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => objIndoor.IndoorNO == a.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Model == accessory.Model))
                                    {
                                        Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => objIndoor.IndoorNO == a.IndoorNO && objIndoor.SystemID == a.SystemID)?.ListAccessory.Add(GetAccessory(accessory));
                                        FlageAdd = true;
                                    }
                                    else
                                    {
                                        if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Count == accessory.Count && a.Model == accessory.Model))
                                        {
                                            (from s in (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID)?.ListAccessory.Where(a => a.Description == accessory.Description && a.Model == accessory.Model))
                                             select s).ToList().ForEach(x => x.Count = accessory.Count);
                                        }
                                        FlageAdd = true;
                                    }
                                }
                                else
                                {
                                    CLeareAllDublicatevalue(SelectedAccessoryList);
                                    JCHMessageBox.Show(Langauge.Current.GetMessage("COUNT_CAN_NOT_ZERO")); //Count can not be zero
                                    return;
                                }
                            }
                        }
                        //End Select IDU List
                        //Start Similer type of IDU List
                        if (similarIDUonCanvas.Count > 0)
                        {
                            if (ApplyToSimilarIDUAccessoryList.Count > 0)
                            {
                                foreach (Accessory Similertype in ApplyToSimilarIDUAccessoryList)
                                {
                                    if (Similertype.Count > 0)
                                    {
                                        //Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => objIndoor.IndoorNO == a.IndoorNO)?.ListAccessory.Add(GetAccessory(Similertype));
                                        foreach (var item in similarIDUonCanvas)
                                        {
                                            if (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == item && objIndoor.SystemID == a.SystemID).ListAccessory == null)
                                            {
                                                Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == item && objIndoor.SystemID == a.SystemID).ListAccessory = new List<Accessory>();
                                            }
                                            if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == item).ListAccessory.Any(a => a.Description == Similertype.Description && a.Model == Similertype.Model))
                                            {
                                                if (Similertype.IsApplyToSimilarUnit == true && Similertype.IsSelect == false)
                                                {
                                                    Similertype.IsSelect = true;
                                                    Similertype.IsApplyToSimilarUnit = false;
                                                }

                                                Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == item && objIndoor.SystemID == a.SystemID)?.ListAccessory.Add(GetAccessory(Similertype));

                                                FlageAdd = true;
                                            }
                                            else
                                            {
                                                if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == item && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Count == Similertype.Count && a.Model == Similertype.Model))
                                                {
                                                    (from s in (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == item && objIndoor.SystemID == a.SystemID)?.ListAccessory.Where(a => a.Description == Similertype.Description && a.Model == Similertype.Model))
                                                     select s).ToList().ForEach(x => x.Count = Similertype.Count);
                                                    FlageAdd = true;
                                                }
                                            }

                                        }
                                        if (objIndoor.ListAccessory != null)
                                        {
                                            if (objIndoor.IndoorNO > 0 && FlageCurrent == false)
                                            {

                                                if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Description == Similertype.Description && a.Model == Similertype.Model))
                                                {
                                                    Similertype.IsApplyToSimilarUnit = true;
                                                    Similertype.IsSelect = false;
                                                    Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID)?.ListAccessory.Add(GetAccessory(Similertype));
                                                }
                                                else
                                                {
                                                    if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Count == Similertype.Count && a.Model == Similertype.Model))
                                                    {
                                                        (from s in (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID)?.ListAccessory.Where(a => a.Description == Similertype.Description && a.Model == Similertype.Model))
                                                         select s).ToList().ForEach(x => x.Count = Similertype.Count);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        CLeareAllDublicatevalue(ApplyToSimilarIDUAccessoryList);
                                        JCHMessageBox.Show(Langauge.Current.GetMessage("COUNT_CAN_NOT_ZERO"));//Count can not be zero
                                        return;
                                    }
                                }
                            }
                        }
                        if (AccessoryListAll.Count > 0)
                        {
                            foreach (Accessory Similertype in AccessoryListAll)
                            {
                                if (Similertype.Count > 0)
                                {
                                    foreach (var idu in similarIDUonCanvas)
                                    {
                                        if (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == idu && objIndoor.SystemID == a.SystemID).ListAccessory == null)
                                        {
                                            Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == idu && objIndoor.SystemID == a.SystemID).ListAccessory = new List<Accessory>();
                                        }
                                        if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == idu && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Description == Similertype.Description && a.Model == Similertype.Model))
                                        {

                                            Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == idu && objIndoor.SystemID == a.SystemID)?.ListAccessory.Add(GetAccessory(Similertype));
                                            FlageAdd = true;
                                        }

                                        else
                                        {
                                            if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == idu && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Count == Similertype.Count))
                                            {
                                                (from s in (Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == idu && objIndoor.SystemID == a.SystemID)?.ListAccessory.Where(a => a.Description == Similertype.Description && a.Model == Similertype.Model))
                                                 select s).ToList().ForEach(x => x.Count = Similertype.Count);
                                                FlageAdd = true;
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    CLeareAllDublicatevalue(AccessoryListAll);
                                    JCHMessageBox.Show(Langauge.Current.GetMessage("COUNT_CAN_NOT_ZERO"));
                                    return;
                                }

                            }
                            foreach (Accessory CurrentSelectIdu in AccessoryListAll)
                            {
                                if (CurrentSelectIdu.Count > 0)
                                {
                                    if (objIndoor.ListAccessory == null)
                                    {
                                        objIndoor.ListAccessory = new List<Accessory>();
                                    }
                                    if (!Project.CurrentProject.RoomIndoorList.FirstOrDefault(a => a.IndoorNO == objIndoor.IndoorNO && objIndoor.SystemID == a.SystemID).ListAccessory.Any(a => a.Description == CurrentSelectIdu.Description && a.Model == CurrentSelectIdu.Model))
                                    {
                                        objIndoor.ListAccessory.Add(GetAccessory(CurrentSelectIdu));
                                        FlageAdd = true;
                                    }
                                }
                                else
                                {

                                    JCHMessageBox.Show(Langauge.Current.GetMessage("COUNT_CAN_NOT_ZERO"));//Count can not be zero
                                    return;
                                }
                            }
                        }

                        if (FlageAdd == true)
                        {
                            if (MaxTypeCountCheck())
                                JCHMessageBox.Show(Langauge.Current.GetMessage("SAVED_SUCCESSFULLY"));//Saved successfully    
                            else
                                return;
                        }
                        else
                        {
                            JCHMessageBox.ShowWarning(Msg.WARNING_DATA_EXCEED);
                        }

                        _winService.Close(ViewKeys.AddAccessories);
                    }
                    else
                    {
                        JCHMessageBox.Show(Langauge.Current.GetMessage("COUNT_CAN_NOT_ZERO"));
                    }
                    //End Similer type of IDU List
                }

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void CLeareAllDublicatevalue(List<Accessory> ListAcc)
        {
            try
            {
                List<bool> SelectAcc = (from a in ListAccessory where a.IsSelect == true || a.IsApplyToSimilarUnit == true select a.IsSelect).ToList();
                if (SelectAcc.Count > 0)
                {
                    foreach (Accessory accessory in ListAcc)
                    {
                        if (Project.CurrentProject.RoomIndoorList != null) //ri.IndoorItem.ListAccessory.Count > 0
                        {
                            if (similarIDUonCanvas != null && similarIDUonCanvas.Count > 0)
                                foreach (var item in similarIDUonCanvas)
                                {
                                    if (Project.CurrentProject.RoomIndoorList.FirstOrDefault(s => s.IndoorNO == item)?.ListAccessory != null)
                                    {
                                        Project.CurrentProject.RoomIndoorList.FirstOrDefault(s => s.IndoorNO == item)?.ListAccessory.RemoveAll(c => c.Type == accessory.Type && c.Model == accessory.Model);
                                    }
                                }
                        }

                        if (objIndoor != null && objIndoor.IndoorItem != null) //ri.IndoorItem.ListAccessory.Count > 0
                        {
                            if (objIndoor.ListAccessory != null && objIndoor.ListAccessory.Count > 0)
                                objIndoor.ListAccessory.RemoveAll(c => c.Type == accessory.Type && c.Model == accessory.Model);
                        }
                    }
                }
                else
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ACCESSORY_SELECTION"));// "Please select atleast one accessory"
                    return;
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        private Accessory GetAccessory(Accessory accessory)
        {
            var defaultaccessory = new Accessory()
            {
                Type = accessory.Type,
                BrandCode = Project.CurrentProject.BrandCode,
                FactoryCode = Project.CurrentProject.FactoryCode,
                IsDefault = accessory.IsDefault, //? todo 
                IsShared = false,//todo
                MaxCapacity = 10,//accessory.
                Model_Hitachi = accessory.Model,
                MaxNumber = accessory.MaxNumber,
                Count = accessory.Count,
                MinCapacity = 5,
                Model_York = "",//todo
                UnitType = objIndoor.IndoorItem.Type,
                Model = accessory.Model,
                IsSelect = accessory.IsSelect,
                IsApplyToSimilarUnit = accessory.IsApplyToSimilarUnit,
                Description = accessory.Description,
            };

            if (ApplyToSimilarIDUAccessoryList != null && ApplyToSimilarIDUAccessoryList.Count > 0)
            {
                if (ApplyToSimilarIDUAccessoryList.Any(a => a.Description == accessory.Description && a.Model == accessory.Model) && !accessory.IsSelect)
                {
                    defaultaccessory = new Accessory()
                    {
                        Type = accessory.Type,
                        BrandCode = Project.CurrentProject.BrandCode,
                        FactoryCode = Project.CurrentProject.FactoryCode,
                        IsDefault = accessory.IsDefault, //? todo 
                        IsShared = false,//todo
                        MaxCapacity = 10,//accessory.
                        Model_Hitachi = accessory.Model,
                        MaxNumber = accessory.MaxNumber,
                        Count = accessory.Count,
                        MinCapacity = 5,
                        Model_York = "",//todo
                        UnitType = objIndoor.IndoorItem.Type,
                        Model = accessory.Model,
                        IsApplyToSimilarUnit = accessory.IsApplyToSimilarUnit,
                        IsSelect = false,
                        Description = accessory.Description,
                    };
                }

            }

            return defaultaccessory;
        }

        private string GetNewTypeName(string type)
        {
            string s = "";

            if (Project.CurrentProject.RegionCode == "EU_W" ||
                Project.CurrentProject.RegionCode == "EU_S" ||
                Project.CurrentProject.RegionCode == "EU_E" || Project.CurrentProject.SubRegionCode == "TW")
            {
                for (int i = 0; i < typeName1EU.Length; ++i)
                {
                    if (typeName1EU[i] == type)
                        if (unitTypeEU[i] == "" || unitTypeEU[i] == objIndoor.IndoorItem.Model)
                            return typeName2EU[i];
                }

                for (int i = 0; i < typeName2EU.Length; ++i)
                {
                    if (typeName2EU[i] == type)
                    {
                        if (unitTypeEU[i] == "" || unitTypeEU[i] == objIndoor.IndoorItem.Model)
                            return typeName1EU[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < typeName1.Length; ++i)
                {
                    if (typeName1[i] == type)
                        if (unitType[i] == "" || unitType[i] == objIndoor.IndoorItem.Type)
                            return typeName2[i];
                }

                for (int i = 0; i < typeName2.Length; ++i)
                {
                    if (typeName2[i] == type)
                    {
                        if (unitType[i] == "" || unitType[i] == objIndoor.IndoorItem.Type)
                            return typeName1[i];
                    }
                }
            }
            return s;

        }

        private void OnRemoveFromAllCommandClick()
        {
            try
            {
                if (objIndoor.ListAccessory.Count > 0)
                {
                    ApplyToSimilarIDUAccessoryList = objIndoor.ListAccessory?.Where(a => a.IsApplyToSimilarUnit).ToList();
                    similarIDUonCanvas = (from idu in Project.CurrentProject.RoomIndoorList where objIndoor.IndoorItem.Model == idu.IndoorItem.Model && objIndoor.IndoorNO != idu.IndoorNO && objIndoor.SystemID == idu.SystemID select idu.IndoorNO).ToList();
                    if (JCHMessageBox.Show(Langauge.Current.GetMessage("REMOVE_ACCESSORIES"), MessageType.Information, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes) //Are you sure want to remove Accessories
                    {
                        if (similarIDUonCanvas.Count() > 0 && similarIDUonCanvas != null)
                        {
                            if (ApplyToSimilarIDUAccessoryList != null && ApplyToSimilarIDUAccessoryList.Count > 0)
                            {
                                foreach (Accessory accessory in ApplyToSimilarIDUAccessoryList)
                                {
                                    if (Project.CurrentProject.RoomIndoorList != null && accessory.IsApplyToSimilarUnit) //ri.IndoorItem.ListAccessory.Count > 0
                                    {

                                        foreach (var item in similarIDUonCanvas)
                                        {
                                            //if (item == objIndoor.IndoorNO)
                                            Project.CurrentProject.RoomIndoorList.FirstOrDefault(s => s.IndoorNO == item && s.SystemID == objIndoor.SystemID)?.ListAccessory.RemoveAll(c => c.Type == accessory.Type && c.Model == accessory.Model);
                                        }


                                    }
                                }
                            }
                        }
                        objIndoor.ListAccessory = new List<Accessory>();

                        foreach (var item in this.ListAccessory)
                        {
                            item.IsApplyToSimilarUnit = false;
                            item.IsSelect = false;
                            item.Count = 0;
                        }
                        if(accType!=null)
                        {
                            if(accType.Count>0)
                            {
                                accType = new Dictionary<string, int>()
                                            { {"Half-size Remote Control Switch",0 },
                                            {"Remote Control Switch",0},
                                            { "Wireless Remote Control Switch",0 },
                                            { "Receiver Kit for Wireless Control",0} };
                            }
                        }
                        JCHMessageBox.Show(Langauge.Current.GetMessage("REMOVED_SUCCESSFULLY"));//Removed successfully
                                                                                                //Bug 4253
                                                                                                //Bug 4253
                        if (_globalProperties.DefaultAccessoryDictionary != null)
                        {
                            if (_globalProperties.DefaultAccessoryDictionary.ContainsKey(Project.GetProjectInstance.projectID))
                            {
                                if (_globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID] == null)
                                    _globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID] = new HashSet<int>();

                                _globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID].Add(objIndoor.IndoorNO);

                            }
                        }

                        //Bug 4253
                        _winService.Close(ViewKeys.AddAccessories);
                    }
                }
                else
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ACCESSORY_SELECTION"));// "Please select atleast one accessory"
                    return;
                }


            }



            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }


        }

        private void GetAccessory(RoomIndoor roomIndoor)
        {
            try
            {
                List<AccessoryModel> objListAccessory = new List<AccessoryModel>();
                AccessoryBLL accessoryBLL = new AccessoryBLL();
                var ri = roomIndoor;//Project.CurrentProject.RoomIndoorList.Find(x => x.IndoorNO == roomIndoor.IndoorNO);
                DataTable dtAccessory = null;

                //Project.CurrentProject.RoomIndoorList.FirstOrDefault().IndoorItem.Model_Hitachi = "H";
                //string brandcode = "H";//ri.IndoorItem.Model_Hitachi;
                //string factoryCode = "S"; //ri.IndoorItem.GetFactoryCodeForAccess();
                string typeItem = "";
                if (ri != null && ri.IndoorItem != null)
                {
                    typeItem = ri.IndoorItem.Type;
                    string fCode = ri.IndoorItem.GetFactoryCodeForAccess();

                    dtAccessory = accessoryBLL.GetInDoorAccessoryItemList(Project.CurrentProject.BrandCode,
                    fCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity,
                    Project.CurrentProject.RegionCode, ri, Project.CurrentProject.SubRegionCode);

                }


                //Display Name
                dtDisplayName = accessoryBLL.GetUniversal_ByAccessory(Project.CurrentProject.SubRegionCode, Project.CurrentProject.BrandCode);
                if (dtAccessory != null)
                {
                    if (dtAccessory.Rows.Count != 0 && dtAccessory != null)
                    {
                        foreach (DataRow row in dtAccessory.Rows)
                        {

                            var ListAccessoryDistinct = (from item in ListAccessory where item.Model.Equals((Project.CurrentProject.BrandCode == "H") ? row["Model_Hitachi"].ToString() : row["Model_York"].ToString()) select item.Model).ToList();
                            if (ListAccessoryDistinct != null)
                                if (ListAccessoryDistinct.Count <= 0)
                                    ListAccessory.Add(new Accessory
                                    {
                                        Type = row["Type"].ToString(),
                                        Model = (Project.CurrentProject.BrandCode == "H") ? row["Model_Hitachi"].ToString() : row["Model_York"].ToString(),
                                        Description = GetAccessoryDisplayName(row["Type"].ToString()) == null ? row["Type"].ToString() : GetAccessoryDisplayName(row["Type"].ToString()),//AccessoryDisplayType.GetAccessoryDisplayTypeByModel(Project.CurrentProject.SubRegionCode, dtDisplayName, row["Type"].ToString(), row["Model_Hitachi"].ToString(), ri),
                                        Count = Convert.ToInt32(row["IsDefault"]),
                                        MaxNumber = Convert.ToInt32(row["MaxNumber"]),
                                        IsSelect = false,
                                        IsApplyToSimilarUnit = false,
                                        IsDefault = (bool)row["IsDefault"]
                                    });
                            //ListAccessory.Last()?.ModelName.Add((Project.CurrentProject.BrandCode == "H") ? row["Model_Hitachi"].ToString() : row["Model_York"].ToString());
                        }
                        if ((objIndoor.ListAccessory?.Count == 0 || objIndoor.ListAccessory == null)
                            && ((_globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID]).Count == 0|| !(_globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID].Contains(objIndoor.IndoorNO))))
                        {
                            GetDefaultAccessory(ListAccessory);
                        }

                        //Add DefaultAccessory to accType Dictionary
                        if (objIndoor.ListAccessory != null)
                        {
                            if (objIndoor.ListAccessory.Count > 0)
                            {
                                var list = (from s in ListAccessory where accType.ContainsKey(s.Type) && s.IsSelect select s.Type).ToList();
                                foreach (var item in list)
                                    accType[item] = 1;
                            }
                            if (Project.CurrentProject.RegionCode == "EU_W" ||
                                Project.CurrentProject.RegionCode == "EU_S" ||
                                Project.CurrentProject.RegionCode == "EU_E" || Project.CurrentProject.SubRegionCode == "TW")
                            {
                                DoRemoveAccessoryReceiverKit();
                            }
                            if (objIndoor.ListAccessory == null)
                            {
                                objIndoor.ListAccessory = new List<Accessory>();
                            }
                            if (Project.CurrentProject.RoomIndoorList != null && Project.CurrentProject.RoomIndoorList.Count > 0)
                            {
                                GetSelectedIDUAccessories();
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

        private void DoRemoveAccessoryReceiverKit()
        {
            if (IncludeHighWall(objIndoor))
            {
                ListAccessory.Remove(ListAccessory.FirstOrDefault(p => p.Type == ReceiverRemoteControl));
            }

        }

        private bool IncludeHighWall(RoomIndoor ri)
        {

            //判断当前室内机类型是否是High Wall,High Wall (w/o EXV)
            bool includeHigh = false;
            for (int i = 0; i < HighWallType.Length; ++i)
            {
                if (ri.IndoorItem.Type == HighWallType[i])
                {
                    includeHigh = true;
                    break;
                }
            }
            if (includeHigh)
            {
                //判断室内机配件是否有Receiver Kit for Wireless Control
                if (ListAccessory != null && ListAccessory.Count > 0)
                {
                    if (ListAccessory.FirstOrDefault(p => p.Type == ReceiverRemoteControl) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 清除MainIndoor下 不存在不匹配的室内机与热交换机
        /// </summary>
        private void DoRemoveIndoorItemGroup()
        {
            List<RoomIndoor> list = new List<RoomIndoor>();
            if (objIndoor != null)
            {
                List<RoomIndoor> rlist = Project.CurrentProject.RoomIndoorList;
                List<RoomIndoor> elist = Project.CurrentProject.ExchangerList;
                if (rlist != null && rlist.Count > 0)
                {
                    list.Add(objIndoor);

                }
                if (elist != null && elist.Count > 0)
                {
                    list.Add(objIndoor);
                }
                //主Indoor 集合
                //List<RoomIndoor> Mainlist = GetMainIndoorItems();
                //foreach (RoomIndoor ri in Mainlist)
                //{
                //    if (ri.IndoorItemGroup != null)
                //    {
                //        bool isExists = true;
                //        RoomIndoor indoor = null;
                //        foreach (RoomIndoor r in ri.IndoorItemGroup)
                //        {
                //            RoomIndoor rom = list.Find(p => p.IndoorNO == r.IndoorNO && p.IsExchanger == r.IsExchanger && p.IndoorItem.ModelFull == r.IndoorItem.ModelFull);
                //            if (rom == null)
                //            {
                //                indoor = r;
                //                isExists = false;
                //                break;
                //            }
                //        }
                //        if (!isExists && indoor != null)
                //        {
                //            ri.IndoorItemGroup.Remove(indoor);
                //        }
                //    }
                //}
            }
        }

        private void RefreshHighWall_ReceiverKit(RoomIndoor ri)
        {
            if (ri == null) return;
            //判断当前室内机类型是否是High Wall,High Wall (w/o EXV)
            bool IncludeHighWall = false;
            for (int i = 0; i < HighWallType.Length; ++i)
            {
                if (ri.IndoorItem.Type == HighWallType[i])
                {
                    IncludeHighWall = true;
                    break;
                }
            }
            if (!IncludeHighWall) return;//如果当前室内机不是High Wall,High Wall (w/o EXV) 直接返回
            bool IncludeWireless = false;
            //判断当前室内机是否Wireless Remote Control Switch 配件
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                foreach (Accessory item in ri.ListAccessory)
                {
                    if (item.Type == WirelessRemoteControl)
                    {
                        IncludeWireless = true;
                        break;
                    }
                }
            }
            bool isShared = false;
            if (IncludeHighWall && IncludeWireless || IncludeHighWall && isShared)
            {
                ListAccessory.FirstOrDefault(a => a.Description.Contains(ReceiverKit)).Description = BuiltInIndoor;
            }

        }

        private string GetAccessoryDisplayName(string type)
        {
            string description = null;
            foreach (DataRow row in dtDisplayName.Rows)
            {
                foreach (DataColumn col in dtDisplayName.Columns)
                {
                    if (row[col].ToString() == type)
                        description = row[col].ToString();
                }
            }
            return description;
        }
        private void GetSelectedIDUAccessories()
        {
            similarIDUonCanvas = (from idu in Project.CurrentProject.RoomIndoorList where objIndoor.IndoorItem.Model == idu.IndoorItem.Model && objIndoor.IndoorNO != idu.IndoorNO && objIndoor.SystemID == idu.SystemID select idu.IndoorNO).ToList();
            Accessories = new ObservableCollection<Accessory>();
            if (Project.CurrentProject.RoomIndoorList != null)
            {
                if (objIndoor != null && objIndoor.IndoorItem != null)
                {
                    if (objIndoor.ListAccessory != null && objIndoor.ListAccessory.Count > 0)
                    {
                        foreach (Accessory row in objIndoor.ListAccessory)
                        {
                            var model = Project.CurrentProject.BrandCode == "H" ? row.Model_Hitachi : row.Model_York;
                            if (model == "-" || model == " " || model == null)
                                model = (from m in this.ListAccessory where m.Type == row.Type select m.Model).FirstOrDefault().ToString();
                            foreach (var item in this.ListAccessory)
                            {
                                
                                if (model == item.Model)
                                {
                                    item.IsSelect = true;//row.IsSelect;
                                    item.Count = row.Count;
                                    item.IsApplyToSimilarUnit = row.IsApplyToSimilarUnit;

                                }



                            }
                        }
                    }

                }
                if (similarIDUonCanvas != null && similarIDUonCanvas.Count > 0)
                {
                    foreach (var item in similarIDUonCanvas)
                    {
                        if (Project.CurrentProject.RoomIndoorList.FirstOrDefault(s => s.IndoorNO == item).ListAccessory != null)
                        {
                            List<Accessory> List = Project.CurrentProject.RoomIndoorList.FirstOrDefault(s => s.IndoorNO == item).ListAccessory.ToList();
                            foreach (Accessory row in List)
                            {
                                foreach (var currentrow in this.ListAccessory)
                                {
                                    if (row.Type == currentrow.Description && objIndoor.IndoorItem.Model == currentrow.Model)
                                    {
                                        currentrow.IsSelect = row.IsSelect;
                                        currentrow.Count = row.Count;
                                        currentrow.IsApplyToSimilarUnit = row.IsApplyToSimilarUnit;
                                        currentrow.Model = row.Model;
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetDefaultAccessory(ObservableCollection<Accessory> AccessoryList)
        {
            List<Accessory> defaultAccessory = new List<Accessory>();
            defaultAccessory = (from a in AccessoryList where a.IsDefault == true select a).ToList();
            if (defaultAccessory != null && defaultAccessory.Count > 0)
            {
                foreach (var item in defaultAccessory)
                {
                    if (objIndoor != null)
                    {
                        if (objIndoor.ListAccessory == null)
                        {
                            objIndoor.ListAccessory = new List<Accessory>();
                        }
                        //int count = objIndoor.ListAccessory.Where(a => a.Model_Hitachi == item.Model && a.IsSelect).Count();
                        //if (count == 0)
                        //{
                            item.IsSelect = true;
                            item.Count = 1;
                            objIndoor.ListAccessory.Add(GetAccessory(item));
                        //}

                    }
                }
            }

        }

        public void CheckAccessryCount(int count = 0)
        {
            try
            {
                int wirelessRemote = ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").Count;
                int wirelessReceiver = ListAccessory.FirstOrDefault(a => a.Type == "Wireless Remote Control Switch").Count;
                if (wirelessRemote > 1 && wirelessReceiver > 1)
                {
                    wireless = true;
                }
                if (ListAccessory != null && ListAccessory.Count > 0 && accType.ContainsKey(SelectedAccessory.Type))
                {
                    if (count == 0)
                    {
                        flag = false;
                        accType[SelectedAccessory.Type] = SelectedAccessory.Count;
                    }
                    else
                    {
                        if (!wireless)
                        {
                            flag = false;
                            accType["Wireless Remote Control Switch"] = 1;
                            accType["Receiver Kit for Wireless Control"] = 1;
                        }
                        else
                        {
                            flag = false;
                            accType["Wireless Remote Control Switch"] = 2;
                            accType["Receiver Kit for Wireless Control"] = 2;

                        }
                    }


                    if (!flag && accType["Half-size Remote Control Switch"] + accType["Remote Control Switch"] + accType["Wireless Remote Control Switch"] + accType["Receiver Kit for Wireless Control"] > 2)
                    {
                        //JCMsg.ShowWarningOK("Number exceeds limitation");

                        if (SelectedAccessory.Type == "Wireless Remote Control Switch" || SelectedAccessory.Type == "Receiver Kit for Wireless Control")
                        {
                            accType["Wireless Remote Control Switch"] = 0;
                            accType["Receiver Kit for Wireless Control"] = 0;
                        }
                        else
                            accType[SelectedAccessory.Type] = SelectedAccessory.Count - 1;
                        JCHMessageBox.ShowError(Langauge.Current.GetMessage("ERROR_ACCESSORY_TYPE_LIMITATION"));
                        flag = true;
                        SelectedAccessory.IsSelect = false;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private bool MaxTypeCountCheck()
        {
            int typecount = 0;
            if (ListAccessory != null)
            {
                var selectedItems = (from s in ListAccessory where s.IsSelect == true select s).ToList();
                foreach (Accessory accessory in selectedItems)
                {
                    if (SelectedAccessory.Description == accessory.Description || GetDefaultAirPanel())
                    {
                        typecount += accessory.Count;
                    }
                    if (typecount > SelectedAccessory.MaxNumber)
                    {
                        JCHMessageBox.ShowError(Langauge.Current.GetMessage("ERROR_ACCESSORY_TYPE_LIMITATION"));
                        SelectedAccessory.IsSelect = false;
                        AccessoryMaxCountError = false;
                        return false;
                    }
                }

            }
            return true;
        }

        private bool GetDefaultAirPanel()
        {
            var selectedItems = new List<Accessory>();
            if (ListAccessory != null && ListAccessory.Count > 0)
                selectedItems = (from s in ListAccessory where s.IsSelect && (s.Description == airPanelWo || s.Description == airPanelWith) select s).ToList();

            if (SelectedAccessory != null)
            {
                if ((SelectedAccessory.Description == airPanelWith || SelectedAccessory.Description == airPanelWo) && selectedItems.Count > 0)
                    return true;
                else
                    return false;
            }
            return false;

        }
        #endregion

        #region misc methods
        //public static bool IsSupportedUniversalSelection(Project proj)
        //{

        //    return Project.CurrentProject.RegionCode.StartsWith("EU_") || proj.SubRegionCode == "TW";
        //}
        //private bool checkMutiSelected(out string reContrIndoorName)
        //{
        //    reContrIndoorName = "";
        //    //related to sharing remote control in different systems
        //    return true;
        //}
        //private void DoAddOptions()
        //{
        //    if (ListAccessory.Count>0)
        //    {

        //        List<RoomIndoor> rinItemList = new List<RoomIndoor>();
        //        List<Indoor> indItemList = new List<Indoor>();
        //        string reContrIndoorName = "";
        //        string optionIndoorName = "";
        //        string groupIndoorName = "";

        //        //维护已选的记录并检查多选条件限制
        //        if (!checkMutiSelected(out reContrIndoorName))
        //            return;

        //        indItemList = objIndoor.ToList();
        //        //indItemList.ToList().OrderBy(p => p.IndoorName);

        //        AccessoryBLL bill = new AccessoryBLL();
        //        DataTable allAvailable = bill.GetAllAvailableSameAccessory(objIndoor, Project.CurrentProject.RegionCode, Project.CurrentProject.SubRegionCode);
        //        if (allAvailable == null)
        //        {
        //            JCMsg.ShowInfoOK(Msg.IND_NO_SAME_ACCESSORY);
        //            return;
        //        }
        //        if (dgvIndoor.SelectedRows.Count > 1)
        //        {
        //            if (!string.IsNullOrEmpty(reContrIndoorName))   //已共享控制器的室内机进行关联关系清空
        //            {
        //                reContrIndoorName = reContrIndoorName.Substring(0, reContrIndoorName.Length - 1);
        //                string allIndoorName = "";
        //                if (!string.IsNullOrEmpty(groupIndoorName))
        //                {
        //                    groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
        //                    allIndoorName = reContrIndoorName + "," + groupIndoorName;   //所有关联室内机
        //                }
        //                else
        //                    allIndoorName = reContrIndoorName;



        //                //提示并进行YES OR NO处理                       
        //                DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_Sharing_RemoteController(reContrIndoorName, groupIndoorName));
        //                if (result != DialogResult.OK)
        //                    return;

        //                //对已选共享控制器的室内机及其相关共享的室内机进行关联清空
        //                foreach (DataGridViewRow r in this.dgvIndoor.Rows)
        //                {
        //                    string indNo = r.Cells[Name_Common.NO].Value.ToString();
        //                    string indName = r.Cells[Name_Common.Name].Value.ToString();
        //                    foreach (string indoorName in allIndoorName.Split(','))   //循环遍历已选共享控制器的室内机及相关共享的室内机名
        //                    {
        //                        if (indoorName.Equals(indName))   //如果共享控制器
        //                        {
        //                            RoomIndoor ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
        //                            if (ri.IndoorItemGroup != null)
        //                            {
        //                                if (ri.IndoorItemGroup.Count != 0)
        //                                    ri.IndoorItemGroup = null;   //清空关联关系
        //                                if (ri.IsMainIndoor)
        //                                    ri.IsMainIndoor = false;   //重置主室内机
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //            else if (!string.IsNullOrEmpty(optionIndoorName))   //室内机多选已有配件清空   
        //            {
        //                optionIndoorName = optionIndoorName.Substring(0, optionIndoorName.Length - 1);
        //                DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_ACCESSORY(optionIndoorName));
        //                if (result != DialogResult.OK)
        //                    return;
        //            }

        //        }

        //        frmAddAccessory f = new frmAddAccessory(rinItemList, thisProject, allAvailable);
        //        f.ShowDialog();
        //        #endregion

        //    }
        //}

        #endregion
        public void GetAccessorie(RoomIndoor roomIndoor)
        {
            try
            {
                DataTable dtDisplayName;
                if (objIndoor == null)
                    objIndoor = roomIndoor;
                List<AccessoryModel> objListAccessory = new List<AccessoryModel>();
                AccessoryBLL accessoryBLL = new AccessoryBLL();
                var ri = roomIndoor;//Project.CurrentProject.RoomIndoorList.Find(x => x.IndoorNO == roomIndoor.IndoorNO);
                DataTable dtAccessory = null;
                //Project.CurrentProject.RoomIndoorList.FirstOrDefault().IndoorItem.Model_Hitachi = "H";
                //string brandcode = "H";//ri.IndoorItem.Model_Hitachi;
                //string factoryCode = "S"; //ri.IndoorItem.GetFactoryCodeForAccess();
                string typeItem = "";
                if (ri != null && ri.IndoorItem != null)
                {
                    typeItem = ri.IndoorItem.Type;
                    string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
                    dtAccessory = accessoryBLL.GetInDoorAccessoryItemList(Project.CurrentProject.BrandCode,
                          fCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity,
                          Project.CurrentProject.RegionCode, ri, Project.CurrentProject.SubRegionCode);
                }


                //Display Name
                dtDisplayName = accessoryBLL.GetUniversal_ByAccessory(Project.CurrentProject.SubRegionCode, Project.CurrentProject.BrandCode);
                if (dtAccessory != null)
                {
                    if (dtAccessory.Rows.Count != 0 && dtAccessory != null)
                    {
                        foreach (DataRow row in dtAccessory.Rows)
                        {

                            ListAccessory.Add(new Accessory
                            {
                                Type = row["Type"].ToString(),
                                Model = (Project.CurrentProject.BrandCode == "H") ? row["Model_Hitachi"].ToString() : row["Model_York"].ToString(),
                                Description = row["Type"].ToString(),
                                Count = Convert.ToInt32(row["IsDefault"]),
                                MaxNumber = Convert.ToInt32(row["MaxNumber"]),
                                IsSelect = false,
                                IsApplyToSimilarUnit = false,
                                IsDefault = (bool)row["IsDefault"]
                            });

                            ListAccessory.Last()?.ModelName.Add((Project.CurrentProject.BrandCode == "H") ? row["Model_Hitachi"].ToString() : row["Model_York"].ToString());
                        }

                        ////bind default accessories to objIndoor
                        //if (_globalProperties.DefaultAccessoryDictionary == null)
                        //    _globalProperties.DefaultAccessoryDictionary = new Dictionary<int, HashSet<int>>();
                        //if (!(_globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID]).Contains(objIndoor.IndoorNO))
                        //{
                        //    GetDefaultAccessory(ListAccessory);
                        //    _globalProperties.DefaultAccessoryDictionary[Project.GetProjectInstance.projectID].Add(objIndoor.IndoorNO);
                        //}
                        //Add DefaultAccessory to accType Dictionary


                        //if (objIndoor.ListAccessory == null)
                        //{
                        //    objIndoor.ListAccessory = new List<Accessory>();
                        //}
                        //if (Project.CurrentProject.RoomIndoorList != null && Project.CurrentProject.RoomIndoorList.Count > 0)
                        //{
                        //    GetSelectedIDUAccessories();
                        //}
                    }
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




