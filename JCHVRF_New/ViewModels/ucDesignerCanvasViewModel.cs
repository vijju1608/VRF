using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Helpers;
using Lassalle.WPF.Flow;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using JCHVRF.BLL;
using JCHVRF.VRFMessage;
using JCHVRF_New.Utility;
using NextGenModel = JCHVRF.Model.NextGen;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Contracts;

namespace JCHVRF_New.ViewModels
{
    class ucDesignerCanvasViewModel : ViewModelBase
    {

        public DelegateCommand<Item> CanvasItemSelectedCommand { get; set; }
        public IEventAggregator _eventAggregator;
        string utTemperature;
        public NextGenModel.SystemVRF currentSystemVRF { get; private set; }
        public ControlGroup _group;
        public ControlSystem controlsystem;
        public List<SystemsOnCanvas> SelectedSystems;
        public List<string> TempControllerList = new List<string>();
        MyProductTypeBLL productTypeBll = new MyProductTypeBLL();

        private SystemBase _currentSystem;

        public SystemBase CurrentSystem
        {
            get { return _currentSystem; }
            set { SetValue(ref _currentSystem, value); }
        }

        public IGlobalProperties globalProperties { get; }

        IRegionManager regionManager;
        public ucDesignerCanvasViewModel(IProjectInfoBAL projctInfoBll, IEventAggregator eventAggregator, IRegionManager regionManager,IGlobalProperties globalProperties=null)
        {
            try
            {
                this.regionManager = regionManager;
                _eventAggregator = eventAggregator;
                this.globalProperties = globalProperties;
                utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                CanvasItemSelectedCommand = new DelegateCommand<Item>(OnSelectedItemChanged);

                //validateController = new ControllerValidation(_group, Project.CurrentProject, _currentSystem);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        ////ToDo: Used to Initialize Hvacsystem in a better way
        //internal void UpdateHvacSystem(SystemBase newSystem)
        //{
        //    CurrentSystem = newSystem;           
        //}

        private void OnSelectedItemChanged(Item selectedItem)
        {
            try
            {
                GetCurrentVRFSystem();
                // TODO: handle in case of manual piping
                //if (currentSystemVRF != null && selectedItem!=null && !(selectedItem is NextGenModel.MyLink) && !(selectedItem is NextGenModel.MyNodeCH))
                //{
                //    //currentSystemVRF.IsManualPiping = true;
                //}
                //Start bug 2959
                //var views = regionManager.Regions["MasterDesignerRightTopArea"].Views.OfType<UIElement>().ToList();
                //views[1].Visibility = Visibility.Visible;
                //End bug 2959
                JCHNode item = selectedItem as JCHNode;
                if (item == null) return;

                if (!item.IsSelected)
                {
                    regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.CanvasProperties);
                    return;
                }
                NavigationParameters param = new NavigationParameters();
                if (item is MyNodeCHBase)
                {

                    int itemIndex = selectedItem.AddFlow.Items.IndexOf(selectedItem);
                    param.Add("AddFlow", item.AddFlow);
                    param.Add("Index", itemIndex);
                    regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.CHBoxProperties, param);
                }
                else if (item.ImageData != null)
                {
                    ImageData data = item.ImageData;

                    if (item is MyNodeCHBase)
                    {
                        int itemIndex = selectedItem.AddFlow.Items.IndexOf(selectedItem.AddFlow.Items.OfType<JCHNode>().FirstOrDefault(a => a.ImageData.NodeNo == item.ImageData.NodeNo));
                        param.Add("AddFlow", item.AddFlow);
                        param.Add("Index", itemIndex);
                        regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.CHBoxProperties, param);
                    }
                    else
                    {
                       
                        switch (data.equipmentType)
                        {
                             
                            case "Indoor":
                                if (CurrentSystem != null)
                                {
                                    RoomIndoor roomIndoor = JCHVRF.Model.Project.CurrentProject.RoomIndoorList.FirstOrDefault(mm => mm.SystemID.Equals(CurrentSystem.Id) && mm.IndoorNO == data.NodeNo);
                                    param.Add("IndoorItem", roomIndoor);
                                    param.Add("CurrentSystem", CurrentSystem);
                                    regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.IDUProperties, param);
                                }
                                break;
                            case "Outdoor":
                                var tempSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(sys => sys.Id == CurrentSystem.Id);
                                if (tempSystem != null)
                                {
                                    param.Add("OduImageData", data);
                                    param.Add("CurrentSystem", tempSystem);
                                    regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.ODUProperties, param);
                                }
                                break;
                            case "Pipe":
                                int itemIndex = selectedItem.AddFlow.Items.IndexOf(selectedItem.AddFlow.Items.OfType<JCHNode>().FirstOrDefault(a => a.ImageData.NodeNo == item.ImageData.NodeNo));
                                param.Add("AddFlow", item.AddFlow);
                                param.Add("Index", itemIndex);
                                regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.CHBoxProperties, param);
                                break;
                            case "HeatExchanger":
                                RoomIndoor roomHeatExchanger = JCHVRF.Model.Project.CurrentProject.ExchangerList.FirstOrDefault(mm => mm.SystemID.Equals(CurrentSystem.Id));
                                param.Add("IndoorItem", roomHeatExchanger);
                                regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, "HeatExchangerCanvasProperties", param); //sss
                                break;
                            //Start bug 2959
                            //case "HeaderBranch":
                            //    views[1].Visibility = Visibility.Collapsed;
                            //    break;
                            //End bug 2959
                            default:
                                regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.CanvasProperties);
                                break;
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


        private NextGenModel.SystemVRF GetCurrentVRFSystem()
        {
            if (currentSystemVRF == null)
            {
                if (WorkFlowContext.CurrentSystem is NextGenModel.SystemVRF)
                    currentSystemVRF = WorkFlowContext.CurrentSystem as NextGenModel.SystemVRF;
            }
            return currentSystemVRF;
        }
        /// <summary>
        /// Create Group and add it to Project, later Compatibility Check
        /// </summary>
        /// <param name="data"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public bool AssignGroupToEquipment(ImageData data, Project project)
        {
            bool isValid = false;
            _group = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID.Equals(CurrentSystem.Id));
            if (_group != null)
            {
            }
            else
            {
                _group = new ControlGroup();
                _group.SetName("Group " + (Project.CurrentProject.ControlGroupList.Count + 1));
                _group.AddToControlSystem(_currentSystem.Id);
                _group.IsValidGrp = false;
                Project.CurrentProject.ControlGroupList.Add(_group);
            }

            controlsystem = Project.CurrentProject.ControlSystemList.Find(c => c.Id == _currentSystem.Id);
            if (controlsystem != null)
            {
                SelectedSystems = controlsystem.SystemsOnCanvasList;
            }
            var systemtoadd = new SystemsOnCanvas();
            var systemType = data.Source.GetType();
            switch (systemType.Name)
            {
                case "CentralController":
                    isValid = CheckForController(data);
                    break;

                case "SystemVRF":
                    isValid = CheckForVrf(data, systemtoadd);
                    break;

                case "RoomIndoor":
                    isValid = CheckForHe(data, systemtoadd);
                    break;
            }
            CheckForErrors(SelectedSystems, Project.CurrentProject.ControllerList.FindAll(c => c.ControlGroupID == _group.Id).Count);
            return isValid;
        }

        /// <summary>
        /// To check whether HE System is already in the group, type2 compatibilty between System and existing Controller
        /// </summary>
        /// <param name="data"></param>
        /// <param name="systemtoadd"></param>
        /// <returns></returns>
        private bool CheckForHe(ImageData data, SystemsOnCanvas systemtoadd)
        {
            bool isValid;
            var heatExchangerUnit = (RoomIndoor) data.Source;
            systemtoadd.SystemName = heatExchangerUnit.IndoorName;
            systemtoadd.System = heatExchangerUnit;
            isValid = CheckSystemCompatibilityWithExistingControllers(data.Source);
            if (isValid)
            {
                if (!heatExchangerUnit.ControlGroupID.Contains(_group.Id))
                {
                    SelectedSystems.Add(systemtoadd);
                    heatExchangerUnit.ControlGroupID.Add(_group.Id);
                }
            }
            else
            {
                JCHMessageBox.Show(Langauge.Current.GetMessage("SELECT_SYS"));
            }

            return isValid;
        }

        /// <summary>
        /// To check whether VRFSystem is already in the group, type2 compatibilty between System and existing Controller
        /// </summary>
        /// <param name="data"></param>
        /// <param name="systemtoadd"></param>
        /// <returns>bool</returns>
        private bool CheckForVrf(ImageData data, SystemsOnCanvas systemtoadd)
        {
            bool isValid;
            var systemVrf = (NextGenModel.SystemVRF) data.Source;
            systemtoadd.SystemName = systemVrf.Name;
            systemtoadd.System = systemVrf;
            isValid = CheckSystemCompatibilityWithExistingControllers(data.Source);
            if (isValid)
            {
                if (!systemVrf.ControlGroupID.Contains(_group.Id))
                {
                    SelectedSystems.Add(systemtoadd);
                    systemVrf.ControlGroupID.Add(_group.Id);
                }
            }
            else
            {
                JCHMessageBox.Show(Langauge.Current.GetMessage("SELECT_SYS"));
            }
            return isValid;
        }

        /// <summary>
        /// Compatibility Check between controller, Controller-In-Sets
        /// </summary>
        /// <param name="data"></param>
        /// <returns>bool</returns>
        private bool CheckForController(ImageData data)
        {
            bool valid;
            var selectedController = (CentralController) data.Source;
            valid = CheckControllerCompatibility(data);
            if (valid)
            {
                //Controller ctrlController = new Controller(selectedController);//ToDo: To Be used for limitation on count
                AddControllerToProject(selectedController);

                AddControllerInSet(selectedController.ControllersInSet);
                _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
            }

            return valid;
        }

        /// <summary>
        /// Adding Valid controller to the CurrentProject Instance
        /// </summary>
        /// <param name="centralController"></param>
        private void AddControllerToProject(CentralController centralController)
        {
            var findcontroller = Project.CurrentProject.ControllerList.Find(x =>
                x.ControlGroupID.Equals(_group.Id) && x.Model.Equals(centralController.Model));
            if (findcontroller != null)
            {
                findcontroller.Quantity = findcontroller.Quantity + 1;
            }
            else
            {
                Controller ctrlController = new Controller(centralController);
                ctrlController.AddToControlGroup(_group.Id);
                ctrlController.Quantity = 1;
                ctrlController.AddToControlSystem(_group.ControlSystemID);
                Project.CurrentProject.ControllerList.Add(ctrlController);
            }
        }

        /// <summary>
        /// Algorithm to check Type2 Compatiblity between existing controllers and system
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public bool CheckSystemCompatibilityWithExistingControllers(object system)
        {
            TempControllerList.Clear();
            var productType = string.Empty;
            foreach (var sys in Project.CurrentProject.ControllerList)
            {
                if (sys.ControlGroupID == _group.Id)
                {
                    TempControllerList.Add(sys.Model);
                }
            }

            if (system.GetType().Equals(typeof(NextGenModel.SystemVRF)))
            {
                productType = ((NextGenModel.SystemVRF) system).OutdoorItem.ProductType;
            }
            else
            {
                productType = ((RoomIndoor)system).IndoorItem.ProductType;
            }

            var controlProductType = productTypeBll.GetControllerProductTypeData(Project.CurrentProject.SubRegionCode,
                Project.CurrentProject.BrandCode, TempControllerList);
            if (controlProductType != null)
            {
                if (!controlProductType.Contains(productType))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Combinations of controllers
        /// </summary>
        /// <param name="newType"></param>
        /// <returns></returns>
        private bool CheckCombinations(CentralController newType)
        {
            foreach (var existingController in Project.CurrentProject.ControllerList)
            {
                if (existingController.ControlGroupID == _group.Id)
                {
                    if (existingController.CentralController != null)
                    {
                        CentralController existControllerType = existingController.CentralController;
                        if (newType.Model == existControllerType.Model) continue;

                        if (CheckControllerCompatible(newType, existControllerType, 1) == false) return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Algorithm to Check combination/Combatibilty between the Controllers
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private bool CheckControllerCompatible(CentralController type1, CentralController type2, int number)
        {
            if (string.IsNullOrEmpty(type1.CompatibleModel))
            {
                if (number == 1)
                {
                    return CheckControllerCompatible(type2, type1, 2);
                }
                else
                {
                    return true;
                }
            }
            else if (type1.CompatibleModel == "none")
            {
                //2.1 - Not compatible with other central controler 
                //JCHMessageBox.Show("ERROR: Controller Not compatible");
                //---------------- Below code added for multi-language----------//
                JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_CONTROLLER"));

                return false;
            }
            else
            {
                //2.2 - is combinable
                List<string> compatibleModels = new List<string>();
                compatibleModels.AddRange(type1.CompatibleModel.Split(',').Select(r => r.Trim()));
                //if (type2.Type == ControllerType.CentralController || type2.Type == ControllerType.ONOFF)
                //{
                if (!compatibleModels.Contains(type2.Model))
                {
                    if (number == 2 || string.IsNullOrEmpty(type2.CompatibleModel))
                    {
                        //JCHMessageBox.Show("ERROR: Controller Not compatible with " + (number ==1 ? type2.Model : type1.Model));
                        //---------------- Below code added for multi-language----------//
                        JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_NOT_COMPATIABLE") + (number == 1 ? type2.Model : type1.Model));
                        return false;
                    }
                    else
                    {
                        return CheckControllerCompatible(type2, type1, 2);
                    }
                }
                //}
            }
            return true;
        }

        /// <summary>
        /// Compatibility check between controllers
        /// </summary>
        /// <param name="selectedControllerData"></param>
        /// <returns></returns>
        private bool CheckControllerCompatibility(NextGenModel.ImageData selectedControllerData)
        {
            bool isValid = true;
            TempControllerList.Clear();
            foreach (var ctrl in Project.CurrentProject.ControllerList)
            {
                if (ctrl.ControlGroupID == _group.Id)
                {
                    TempControllerList.Add(ctrl.Model);
                }
            }
            TempControllerList.Add(selectedControllerData.imageName);
            //Check all the Conditions are satisfied or not
            isValid = CheckBeforeAddingController(selectedControllerData);

            return isValid;
        }
        /// <summary>
        /// Checking based on the Regions
        /// </summary>
        /// <param name="selectedControllerData"></param>
        /// <returns></returns>
        private bool CheckBeforeAddingController(NextGenModel.ImageData selectedControllerData)
        {
            if (CheckForSystems(true) == false)
            {
                return false;
            }
            //No limitation for EU region
            if (Project.CurrentProject.RegionCode == "EU_W" ||
                    Project.CurrentProject.RegionCode == "EU_S" ||
                    Project.CurrentProject.RegionCode == "EU_E" )
             return true;

            if (CheckCombinations((CentralController)selectedControllerData.Source) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Type1 Compatibilty check between Controllers and Selected Systems
        /// </summary>
        /// <param name="valid"></param>
        /// <returns></returns>
        private bool CheckForSystems(bool valid)
        {
            string productType = string.Empty;
            var controlProductType = productTypeBll.GetControllerProductTypeData(Project.CurrentProject.SubRegionCode,
                Project.CurrentProject.BrandCode, TempControllerList);
            if (SelectedSystems.Count > 0)
            {
                foreach (var sys in SelectedSystems)
                {
                    if (sys.System.GetType().Equals(typeof(NextGenModel.SystemVRF)))
                    {
                        productType = ((NextGenModel.SystemVRF)sys.System).OutdoorItem.ProductType;
                    }
                    else
                    {
                        productType = ((RoomIndoor)sys.System).IndoorItem.ProductType;
                    }

                    if (controlProductType != null)
                    {
                        if (!controlProductType.Contains(productType))
                        {
                            JCHMessageBox.Show(Msg.ERR_CONTROLLER_PRODUCTTYPE_COMPATIBILITY(sys.SystemName),MessageType.Warning);
                            valid = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                JCHMessageBox.ShowWarning("No Systems available");
            }
            //appropriate message to add systems
            return valid;
        }

        /// <summary>
        /// To Check the In-Set Controller
        /// </summary>
        /// <param name="src2Controller"></param>
        private void AddControllerInSet(List<CentralController> src2Controller)
        {
            if (src2Controller == null) return;
            foreach (var sysController in src2Controller)
            {
                AddControllerToProject(sysController);
                _eventAggregator.GetEvent<InSetController>().Publish(sysController);
            }
        }

        /// <summary>
        /// Delete the node ofType WiringNodeCentralControl
        /// </summary>
        /// <param name="deleteNode"></param>
        public void DeleteEquipmentOnCanvas(JCHNode deleteNode)
        {
            var findcontrolsystem = Project.CurrentProject.ControlSystemList.Find(t => t.Id == _currentSystem.Id);
            var node = (NextGenModel.WiringNodeCentralControl) deleteNode;
            _group = Project.CurrentProject.ControlGroupList.Find(x =>
                x.ControlSystemID == _currentSystem.Id);

            //For VRF
            if (node.SystemItem != null)
            {
                var objNode = node.SystemItem;
                var vrfList = findcontrolsystem.SystemsOnCanvasList.FindAll(item => item.System.GetType() == typeof(NextGenModel.SystemVRF));
                var findHE = vrfList.Find(item => ((NextGenModel.SystemVRF)item.System).Id == objNode.Id);
                findcontrolsystem.SystemsOnCanvasList.Remove(findHE);
                foreach (NextGenModel.SystemVRF r in Project.CurrentProject.SystemListNextGen)
                {
                    if (r.Id == node.SystemItem.Id)
                    {
                        int index = r.ControlGroupID.IndexOf(_group.Id);
                        r.ControlGroupID.RemoveAt(index);
                        break;
                    }
                }
            }
            //For HE
            else if (node.RoomIndoorItem != null)
            {
                var objNode = node.RoomIndoorItem;
                var heList = findcontrolsystem.SystemsOnCanvasList.FindAll(item => item.System.GetType() == typeof(RoomIndoor));
                var findHE = heList.Find(item => ((RoomIndoor)item.System).SystemID == objNode.SystemID);
                findcontrolsystem.SystemsOnCanvasList.Remove(findHE);
                foreach (RoomIndoor r in Project.CurrentProject.ExchangerList)
                {
                    if (r.SystemID == node.RoomIndoorItem.SystemID)
                    {
                        int index = r.ControlGroupID.IndexOf(_group.Id);
                        r.ControlGroupID.RemoveAt(index);
                        break;
                    }
                }
            }
            //For Controller
            else if (node.Controller != null)
            {
                var ct = Project.CurrentProject.ControllerList.Find(c =>
                    c.ControlGroupID == _group.Id && c.Model == node.Controller.Model);
                if (ct != null)
                {
                    if (ct.Quantity <= 1)
                    {
                        Project.CurrentProject.ControllerList.Remove(ct);
                    }
                    else
                    {
                        ct.Quantity -= 1;
                    }
                }
            }
            SelectedSystems = findcontrolsystem.SystemsOnCanvasList;            
             if (SelectedSystems != null)
            {
                CheckForErrors(SelectedSystems, Project.CurrentProject.ControllerList.FindAll(c => c.ControlGroupID == _group.Id).Count);
            }
        }

        /// <summary>
        /// Delete the node ofType JCHNode
        /// </summary>
        /// <param name="deleteNode"></param>
        public void DeleteNodeForController(JCHNode deleteNode)
        {
            var findcontrolsystem =
                Project.CurrentProject.ControlSystemList.Find(t => t.Id == _currentSystem.Id);
            _group = Project.CurrentProject.ControlGroupList.Find(x =>
                x.ControlSystemID == _currentSystem.Id);
            if (deleteNode.ImageData.equipmentType == "System")
            {
                //For VRF
                if (deleteNode.ImageData.Source.GetType() == typeof(NextGenModel.SystemVRF))
                {
                    var node = (NextGenModel.SystemVRF) deleteNode.ImageData.Source;
                    var find = findcontrolsystem.SystemsOnCanvasList.Find(item =>
                        item.System == (object) node);
                    findcontrolsystem.SystemsOnCanvasList.Remove(find);                    
                    foreach (NextGenModel.SystemVRF r in Project.CurrentProject.SystemListNextGen)
                    {
                        if (r.Id == node.Id)
                        {
                            int index = r.ControlGroupID.IndexOf(_group.Id);
                            r.ControlGroupID.RemoveAt(index);
                            break;
                        }
                    }
                }
                //For HE
                if (deleteNode.ImageData.Source.GetType() == typeof(RoomIndoor))
                {
                    var node = (RoomIndoor) deleteNode.ImageData.Source;
                    var find = findcontrolsystem.SystemsOnCanvasList.Find(item =>
                        item.System == (object) node);
                    findcontrolsystem.SystemsOnCanvasList.Remove(find);
                    foreach (RoomIndoor r in Project.CurrentProject.ExchangerList)
                    {
                        if (r.SystemID == node.SystemID)
                        {
                            int index = r.ControlGroupID.IndexOf(_group.Id);
                            r.ControlGroupID.RemoveAt(index);
                            break;
                        }
                    }
                }
            }
            //For Controller
            if (deleteNode.ImageData.equipmentType == "Controller")
            {
                var ct = Project.CurrentProject.ControllerList.FindLast(c =>
                    c.Model == deleteNode.ImageData.imageName);
                if (ct != null)
                {
                    if (ct.Quantity <= 1)
                    {
                        Project.CurrentProject.ControllerList.Remove(ct);
                    }
                    else
                    {
                        ct.Quantity -= 1;
                    }
                }
            }

            SelectedSystems = findcontrolsystem.SystemsOnCanvasList;           
            if (SelectedSystems != null)
            {
               CheckForErrors(SelectedSystems, Project.CurrentProject.ControllerList.FindAll(c => c.ControlGroupID == _group.Id).Count);
            }
        }


       /// <summary>
       /// Checking for Errors on Central Controller system
       /// </summary>
       /// <param name="selectedSystems"></param>
       /// <param name="count"></param>
        public void CheckForErrors(List<SystemsOnCanvas> selectedSystems, int count)
        {
            Project.CurrentProject.ControlSystemList.Find(c => c.Id == _currentSystem.Id).IsAutoWiringPerformed = false;
            if (selectedSystems.Count > 0 && count > 0 || selectedSystems.Count + count == 0)
            {
                ClearErrorLogs();
            }
            else
            {
                if (selectedSystems.Count == 0)
                {
                    ErrorLoggingController(60);
                }
                else
                {
                    ErrorLoggingController(59);
                }
            }
            if (Project.CurrentProject.RegionCode == "EU_W" || Project.CurrentProject.RegionCode == "EU_S" || Project.CurrentProject.RegionCode == "EU_E" )           
            {
                ValidateMandatoryCompatibleControllers();
            }
            UtilTrace.SaveHistoryTraces();
        }

        /// <summary>
        /// Algorithm to Check Paired controller which are mandotory for EU region
        /// </summary>
        public void ValidateMandatoryCompatibleControllers()
        {
            try
            {
                var SelectedControllerSystems =
                    Project.CurrentProject.ControllerList.FindAll(x => x.ControlGroupID == _group.Id);
                if (SelectedSystems.Count > 0)
                {
                    foreach (var vrfsys in SelectedSystems)
                    {
                        if (vrfsys.System.GetType() == typeof(RoomIndoor))
                        {
                            //Does nothing when it is heat exchanger
                        }
                        else
                        {
                            var SelOutdoorType = ((JCHVRF.Model.NextGen.SystemVRF)vrfsys.System).SelOutdoorType;

                            if (SelOutdoorType == "FSXNPE (Top discharge)" ||
                                SelOutdoorType == "FSXNSE (Top discharge)")
                            {
                                Dictionary<string, int> dicNum = new Dictionary<string, int>();
                                dicNum["CSNET MANAGER 2 T10"] = 0;
                                dicNum["CSNET MANAGER 2 T15"] = 0;
                                dicNum["HC-A64NET"] = 0;
                                dicNum["PSC-A160WEB1"] = 0;

                                foreach (var item in SelectedControllerSystems)
                                {
                                    string model = item.Model;
                                    switch (model)
                                    {
                                        case "CSNET MANAGER 2 T10":
                                            dicNum["CSNET MANAGER 2 T10"] = item.Quantity;
                                            break;
                                        case "CSNET MANAGER 2 T15":
                                            dicNum["CSNET MANAGER 2 T15"] = item.Quantity;
                                            break;
                                        case "HC-A64NET":
                                            dicNum["HC-A64NET"] = item.Quantity;
                                            break;
                                        case "PSC-A160WEB1":
                                            dicNum["PSC-A160WEB1"] = item.Quantity;
                                            break;
                                        default:
                                            ClearErrorLogs();
                                            break;
                                    }

                                }

                                if ((dicNum["CSNET MANAGER 2 T10"] + dicNum["CSNET MANAGER 2 T15"]) > 0)
                                {
                                    if ((dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"]) <= 0)
                                    {
                                        ErrorLoggingController(61);
                                        //JCHMessageBox.Show("PSC-A160WEB1 or  HC-A64NET must be added.");
                                        //---------------- Below code added for multi-language----------//
                                        JCHMessageBox.Show(Langauge.Current.GetMessage("MUST_ADDED"));
                                        break;
                                    }

                                    ClearErrorLogs();
                                }

                                if (dicNum["HC-A64NET"] > 0 || dicNum["PSC-A160WEB1"] > 0)
                                {
                                    if ((dicNum["CSNET MANAGER 2 T10"] + dicNum["CSNET MANAGER 2 T15"]) <= 0)
                                    {
                                        ErrorLoggingController(62);
                                        //JCHMessageBox.Show("CSNET MANAGER 2 T10 or CSNET MANAGER 2 T15 must be selected");
                                        //---------------- Below code added for multi-language----------//
                                        JCHMessageBox.Show(Langauge.Current.GetMessage("CSNET"));
                                        break;
                                    }
                                    else if (SelectedSystems.Count < dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"])
                                    {
                                        ErrorLoggingController(63);
                                        //JCHMessageBox.Show("Selected System Count must be Equal or Greater than count of HSC-A64NET or PSC-A160WEB1");
                                        //---------------- Below code added for multi-language----------//
                                        JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_SYSTEM_COUNT"));

                                        break;
                                    }
                                    else
                                    {
                                        ClearErrorLogs();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, e);
            }
        }

        /// <summary>
        /// Clear error logs for control system
        /// </summary>
        public void ClearErrorLogs()
        {
            controlsystem.IsValid = true;
            ((ControlSystem)_currentSystem).IsValid = true;
            _group.IsValidGrp = true;
            _currentSystem._errors.Clear();
            _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
        }

        /// <summary>
        /// Relaying message to error log for Control System
        /// </summary>
        /// <param name="errorId"></param>
        public void ErrorLoggingController(int errorId)
        {
            ((ControlSystem)_currentSystem).IsValid = false;
            ((ControlSystem)_currentSystem).IsAutoWiringPerformed = false;
            controlsystem.IsValid = false;
            controlsystem.IsAutoWiringPerformed = false;
            _group.IsValidGrp = false;

            _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
            switch (errorId)
            {
                case 61:
                    if (!_currentSystem._errors.Contains(" PSC-A160WEB1 or  HC-A64NET must be added."))
                    {
                        _currentSystem._errors.Add(" PSC-A160WEB1 or  HC-A64NET must be added.");
                    }

                    break;

                case 62:
                    if (!_currentSystem._errors.Contains("CSNET MANAGER LT or CSNET MANAGER XT must be selected"))
                    {
                        _currentSystem._errors.Add("CSNET MANAGER LT or CSNET MANAGER XT must be selected");
                    }

                    break;

                case 63:
                    if (!_currentSystem._errors.Contains(
                        "Selected System Count must be Equal or Greater than count of HSC-A64NET or PSC-A160WEB1"))
                    {
                        _currentSystem._errors.Add(
                            "Selected System Count must be Equal or Greater than count of HSC-A64NET or PSC-A160WEB1");
                    }

                    break;

                case 60:
                    if (!_currentSystem._errors.Contains("No Systems Selected"))
                    {
                        _currentSystem._errors.Add("No Systems Selected");
                    }

                    break;

                case 59:
                    if (!_currentSystem._errors.Contains("No Controller Selected"))
                    {
                        _currentSystem._errors.Add("No Controller Selected");
                    }

                    break;
            }

            _eventAggregator.GetEvent<ErrorLogVM>().Publish(_currentSystem.Errors);
        }
       
    }
}
