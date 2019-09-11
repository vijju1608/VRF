/****************************** File Header ******************************\
File Name:	CentralControllerCanvasViewModel.cs
Date Created:	3/20/2019
Description:	
\*************************************************************************/

using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JCBase.Utility;
using JCHVRF.BLL;
using NextGenModel = JCHVRF.Model.NextGen;
using JCHVRF_New.Model;
using JCHVRF_New.Views;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{

    using JCHVRF.BLL.New;
    using JCHVRF.Model;
    using JCHVRF.VRFMessage;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using JCHVRF_New.Utility;
    using Prism.Commands;
    using Prism.Events;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using Language = JCHVRF_New.LanguageData.LanguageViewModel;

    public class CentralControllerCanvasViewModel : ViewModelBase
    {
        #region Fields

        public DelegateCommand<object> SelectedSystemCommand { get; set; }
        public DelegateCommand<object> MouseDownCommand { get; set; }
        public DelegateCommand btnEditGroupNameCommand { get; set; }

        public DelegateCommand<object> UnSelectSystemCommand { get; set; }
        public DelegateCommand<object> DeleteControllerCommand { get; set; }

        public DelegateCommand<object> IncrementControllersCommand { get; set; }

        public JCHVRF.Model.Project _currentProject;

        public IEventAggregator _eventAggregator;

        private CanvasListItem _selectedLeftSideBarItem;
        //public List<NextGenModel.SystemVRF> groupSystemList = new List<NextGenModel.SystemVRF>();
        public List<string> controllerList = new List<string>();
        private object _selectedTreeItem;
        private CentralControllerBLL _centralControllerBll = new CentralControllerBLL(Project.CurrentProject);

        private CanvasItemChild _selectedLeftSideBarChild;
        private ObservableCollection<CanvasItemChild> _vrfSystemsObservableCollection;
        private ObservableCollection<CanvasItemChild> _heatExchangerObservableCollection;
        private CanvasListItem _heatExchangerTreeHeader;
        private CanvasListItem _vrfTreeHeader;
        private List<Controller> _controllers;
        private ControlGroup _group;
        MyProductTypeBLL productTypeBll = new MyProductTypeBLL();        
        private ControlSystem _currentSystem;


        #endregion Fields

        #region Constructors

        public CentralControllerCanvasViewModel(IEventAggregator eventAggregator, IProjectInfoBAL projectInfoBll)
        {
            try
            {
                this._eventAggregator = eventAggregator;

                SelectedSystems = new ObservableCollection<SelectedSystemViewModel>();

                SelectedControllerSystems = new ObservableCollection<SelectedCentralControllerViewModel>();

                _vrfSystemsObservableCollection = new ObservableCollection<CanvasItemChild>();
                _heatExchangerObservableCollection = new ObservableCollection<CanvasItemChild>();
                _vrfTreeHeader = new CanvasListItem("VRF")
                {
                    Children = _vrfSystemsObservableCollection
                };
                _heatExchangerTreeHeader = new CanvasListItem("Heat Exchanger")
                {
                    Children = _heatExchangerObservableCollection
                };
                SystemsAvailableForSelection = new ObservableCollection<CanvasListItem>();
                btnEditGroupNameCommand = new DelegateCommand(btnEditGroupNameClickEvent);
                LostFocusGroupName = new DelegateCommand(LostFocusGroupNameEvent);
                SelectedSystemCommand = new DelegateCommand<object>(onSystemSelected);
                MouseDownCommand = new DelegateCommand<object>(OnMouseDownClick);
                UnSelectSystemCommand = new DelegateCommand<object>(OnUnSelectSystem);
                DeleteControllerCommand = new DelegateCommand<object>(DeleteController);
                IncrementControllersCommand = new DelegateCommand<object>(IncreaseControllers);
                _eventAggregator.GetEvent<SystemCreated>().Subscribe(NewSystemCreated);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        #endregion Constructors

        private void IncreaseControllers(object obj)
        {
            try
            {
                SelectedCentralControllerViewModel controlerViewModelToIncrement = (SelectedCentralControllerViewModel)obj;
                Controller controller = controlerViewModelToIncrement.Controller;
                if (controlerViewModelToIncrement.Controller != null)
                {
                    if (!CheckBeforeAddController(controller))
                    {
                        return;
                    }
                    controller.Quantity = controller.Quantity + 1;
                    controlerViewModelToIncrement.Quantity = controller.Quantity;
                    if (controlerViewModelToIncrement.Controller.CentralController.ControllersInSet != null)
                    {
                        IncrementControllersInSet(controlerViewModelToIncrement);
                    }
                    ValidateMandatoryCompatibleControllers();
                    UtilTrace.SaveHistoryTraces();
                    _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private int OduCount;
        private int IduCount;

        private bool CheckBeforeAddController(Controller Controller)
        {
            //no Check for EU region
            if (Project.CurrentProject.RegionCode.StartsWith("EU"))return true;

            int deviceQty = IduCount + OduCount + (SelectedControllerSystems.Count - 1);
            int ccControllerQty = GetControllerQty(ControllerType.CentralController);
           
            if (Controller.CentralController.Type == ControllerType.CentralController)
            {
                ccControllerQty = ccControllerQty + 1;
            }
            if (CheckSystemQtyLimitation(Controller.CentralController, IduCount, OduCount, deviceQty + 1) == false)
            {
                return false;
            }

            bool isHLinkII = !(Controller.CentralController.Protocol.Trim().ToUpper() == "H-LINK");
            if (CheckHLinkLimiation(isHLinkII, IduCount, OduCount, deviceQty + 1) == false)
            {
                return false;
            }

            if (Controller.CentralController.MaxControllerNumber > 0 && ccControllerQty > Controller.CentralController.MaxControllerNumber)
            {
                MessageBox.Show(Msg.CONTROLLER_HLINK_CENTRAL_CONTROLLER_QTY(ControllerConstValue.HLINKII_MAX_CC_QTY));
                return false;
            }
            var model = Controller.Model;
            /* int controllerQty;
             bool firstOfThisModel = true;
             if( == 1 && firstOfThisModel == true)
             {
                 controllerQty = 0;
                 firstOfThisModel = false;
             }
             else
             {
                 controllerQty = Controller.Quantity;
             }*/
            if (Controller.CentralController.MaxSameModel > 0 && (Controller.Quantity + 1) > Controller.CentralController.MaxSameModel)
            {
                MessageBox.Show(Msg.CONTROLLER_HLINK_CONTROLLER_QTY(Controller.CentralController.MaxSameModel));
                return false;
            }

            if (Controller.CentralController.MaxSameType > 0 && GetControllerQty(Controller.CentralController.Type) + 1 > Controller.CentralController.MaxSameType)
            {
                MessageBox.Show(Msg.CONTROLLER_HLINK_CONTROLLER_QTY(Controller.CentralController.MaxSameType));
                return false;
            }
            return true;
        }

        public int GetControllerQty(ControllerType modelName)
        {
            int qty = 0;
            foreach (var item in SelectedControllerSystems)
            {
                if (item.Controller != null && item.ModelName != null)
                {
                    if (modelName == item.Controller.Type)
                    {
                        qty += item.Quantity;
                    }
                }
            }
            return qty;
        }

        private bool CheckSystemQtyLimitation(CentralController typeInfo, int indoorQty, int outdoorQty, int deviceQty)
        {
            //1 - 200 devices (incl. ODU, IDU, remote controller, central controller) can be connected in 1 H-Link
            if (typeInfo.MaxDeviceNumber > 0 && deviceQty > typeInfo.MaxDeviceNumber)
            {
                MessageBox.Show(Msg.CONTROLLER_TOTAL_DEVICE_QTY(typeInfo.MaxDeviceNumber));
                return false;
            }
            
            //2 - Indoor quantity limitation
            if (typeInfo.MaxIndoorUnitNumber > 0 && indoorQty > typeInfo.MaxIndoorUnitNumber)
            {
                MessageBox.Show(Msg.CONTROLLER_MAX_INDOOR_QTY(typeInfo.Model, typeInfo.MaxIndoorUnitNumber));
                return false;
            }

            //3 - Outdoor quantity limitation
            if (typeInfo.MaxSystemNumber > 0 && outdoorQty > typeInfo.MaxSystemNumber)
            {
                MessageBox.Show(Msg.CONTROLLER_MAX_OUTDOOR_QTY(typeInfo.Model, typeInfo.MaxSystemNumber));
                return false;
            }
            return true;
        }

        private bool CheckHLinkLimiation(bool isHLinkII, int indoorQty, int outdoorQty, int deviceQty)
        {
            if (isHLinkII)
            {
                if (indoorQty > ControllerConstValue.HLINKII_MAX_IDU_QTY)
                {
                    MessageBox.Show(Msg.CONTROLLER_HLINK_INDOOR_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_IDU_QTY));
                    return false;
                }
                if (outdoorQty > ControllerConstValue.HLINKII_MAX_ODU_QTY)
                {
                    MessageBox.Show(Msg.CONTROLLER_HLINK_OUTDOOR_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_ODU_QTY));
                    return false;
                }
                if (deviceQty > ControllerConstValue.HLINKII_MAX_DEVICE_QTY)
                {
                    MessageBox.Show(Msg.CONTROLLER_HLINK_DEVICE_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_DEVICE_QTY));
                    return false;
                }
            }
            else
            {
                if (indoorQty > ControllerConstValue.HLINK_MAX_IDU_QTY)
                {
                    MessageBox.Show(Msg.CONTROLLER_HLINK_INDOOR_QTY("H-Link", ControllerConstValue.HLINKII_MAX_IDU_QTY));
                    return false;
                }
                if (outdoorQty > ControllerConstValue.HLINK_MAX_ODU_QTY)
                {
                    MessageBox.Show(Msg.CONTROLLER_HLINK_OUTDOOR_QTY("H-Link", ControllerConstValue.HLINKII_MAX_ODU_QTY));
                    return false;
                }
                if (deviceQty > ControllerConstValue.HLINK_MAX_DEVICE_QTY)
                {
                    MessageBox.Show(Msg.CONTROLLER_HLINK_DEVICE_QTY("H-Link", ControllerConstValue.HLINK_MAX_DEVICE_QTY));// HLINK_MAX_DEVICE_QTY 限制145 on 20180306 by xyj
                    return false;
                }
            }
            return true;
        }

        private void OnUnSelectSystem(object obj)
        {
            try
            {
                SelectedSystemViewModel systemToUnselect = (SelectedSystemViewModel)obj;
                RemoveFromSelectedSystems(systemToUnselect);
                AddToSystemsAvailableForSelection(new CanvasItemChild(systemToUnselect.System.Name, "VRF",
                    systemToUnselect.System.StatusIcon, systemToUnselect.System));
                UtilTrace.SaveHistoryTraces();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void DeleteController(object obj)
        {
            try
            {
                SelectedCentralControllerViewModel controllerToDelete = (SelectedCentralControllerViewModel)obj;
                Controller controller = controllerToDelete.Controller;
                if (controllerToDelete.Controller != null && controllerToDelete.Controller != null)
                {
                    if (controller.Quantity > 1)
                    {
                        controller.Quantity = controller.Quantity - 1;
                        controllerToDelete.Quantity = controller.Quantity;

                    }
                    else
                    {
                        Project.CurrentProject.ControllerList.Remove(controller);
                        SelectedControllerSystems.Remove(controllerToDelete);
                        if (_currentSystem.Errors.Count.Equals(0))
                        {
                            ClearErrorLogs();
                        }

                    }
                    if (controllerToDelete.Controller.CentralController.ControllersInSet != null)
                    {
                        DeleteControllersInSet(controllerToDelete);
                    }
                    ValidateMandatoryCompatibleControllers();
                    UtilTrace.SaveHistoryTraces();
                    _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        public class SelectedSystemViewModel
        {
            public string SystemName { get; set; }
            public int NoOfODU { get; set; }
            public int NoOfIDU { get; set; }

            public SystemBase System { get; set; }

            public Visibility CountVisibility { get; set; }
            public bool isNeedChk { get; set; }
        }

        private void OnMouseDownClick(object objSystemBase)
        {
            try
            {
                objSystemBase = (objSystemBase != null) ? objSystemBase : WorkFlowContext.CurrentSystem;
                if (objSystemBase != null)
                    _eventAggregator.GetEvent<CentralControllerCanvasMouseDownClickSubscriber>()
                        .Publish((SystemBase)objSystemBase);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void NewSystemCreated(SystemBase system)
        {
            try
            {
                string systemType = system.HvacSystemType;
                int systemTypeAsInt = Convert.ToInt32(systemType);
                switch (systemTypeAsInt)
                {
                    case 1:
                        CanvasItemChild child = new CanvasItemChild(system.Name, "VRF", system.StatusIcon, system);
                        child.IsSelected = true;
                        _vrfSystemsObservableCollection.Add(child);
                        break;

                    case 2:
                        child = new CanvasItemChild(system.Name, "Heat Exchanger", system.StatusIcon, system);
                        child.IsSelected = true;
                        _heatExchangerObservableCollection.Add(child);
                        break;
                }

                updateListItems();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        internal void Refresh(SystemBase currentSystem)
        {
            this._currentSystem = (ControlSystem)currentSystem;
            _group = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID.Equals(_currentSystem.Id));
            if (_group != null)
            {
                _controllers = Project.CurrentProject.ControllerList.FindAll(x => x.ControlGroupID.Equals(_group.Id));
            }
            else
            {
                _group = new ControlGroup();
                _group.SetName("Group " + (Project.CurrentProject.ControlGroupList.Count + 1));
                GroupName = _group.Name;
                _group.AddToControlSystem(_currentSystem.Id);
                Project.CurrentProject.ControlGroupList.Add(_group);
            }

            if (_currentSystem.Errors.Count > 0)
            {
                _eventAggregator.GetEvent<ErrorLogVM>().Publish(_currentSystem.Errors);
            }

            RefreshItems();
        }

        void LostFocusGroupNameEvent()
        {
            try
            {
                _group.SetName(_groupName);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        private void RefreshItems()
        {
            if (_group != null)
            {
                GroupName = _group.Name;
            }

            SelectedSystems.Clear();

            List<CanvasItemChild> vrfSystems = new List<CanvasItemChild>();
            foreach (JCHVRF.Model.NextGen.SystemVRF systemVRF in Project.CurrentProject.SystemListNextGen)
            {

                if (!(systemVRF.ControlGroupID.Contains(_group.Id)))
                {
                    if (systemVRF.IsPipingOK)
                    {
                        vrfSystems.Add(new CanvasItemChild(systemVRF.Name, "VRF", null, systemVRF));
                    }
                }
                else if (systemVRF.ControlGroupID.Contains(_group.Id))
                {
                    UpdateSelectedSystemsList(systemVRF);
                }

            }
            _vrfSystemsObservableCollection.Clear();
            _vrfSystemsObservableCollection.AddRange(vrfSystems);

            //ACC - RAG START
            //For Heat Exchanger system visibility in Project Details tab           
            List<CanvasItemChild> HeatExchangerSystems = new List<CanvasItemChild>();
            foreach (var heatExchangerSystems in Project.CurrentProject.HeatExchangerSystems)
            {

                var heatExchangerUnit =
                    Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(heatExchangerSystems.Id));

                // To avoid the addition of empty heat exchanger system.
                if (heatExchangerUnit.IndoorItem != null)
                {
                    //if (heatExchangerUnit.ControlGroupID.Count<=0)
                    if (!(heatExchangerUnit.ControlGroupID.Contains(_group.Id)))
                    {
                        HeatExchangerSystems.Add(
                            new CanvasItemChild(heatExchangerSystems.Name, "Heat Exchanger", null, heatExchangerSystems));
                    }
                    else if (heatExchangerUnit.ControlGroupID.Contains(_group.Id))
                    {
                        UpdateSelectedSystemsList(heatExchangerSystems);
                    }
                }
            }

            _heatExchangerObservableCollection.Clear();
            _heatExchangerObservableCollection.AddRange(HeatExchangerSystems);

            //if (vrfSystems.Count > 0)
            //{
            //    _selectedLeftSideBarChild = vrfSystems[0];
            //}
            //else if (HeatExchangerSystems.Count > 0)
            //{
            //    _selectedLeftSideBarChild = HeatExchangerSystems[0];
            //}

            updateListItems();

            //refresh controllers list
            List<Controller> controllers =
                Project.CurrentProject.ControllerList.FindAll(x => x.ControlGroupID.Equals(_group.Id));
            SelectedControllerSystems.Clear();
            //Dummy model for drag drop
            string strPath = string.Empty;
            GetImagesSourceDir(ref strPath);
            strPath += "\\" + "Plus_Icon_CC.png";
            SelectedControllerSystems.Add(new SelectedCentralControllerViewModel
            {
                Image = strPath,
                ModelName = Language.Current.GetMessage("DRAG_NEW_CONTROLLER")
            });
            foreach (var controller in controllers)
            {

                UpdateSelectedControllerSystems(controller, false);
            }
            if (SelectedSystems.Count < 1 && SelectedControllerSystems.Count > 1)
            {
                ErrorLoggingForCentralController(60);
            }                    
        }

        private void updateListItems()
        {
            if (_vrfSystemsObservableCollection.Count > 0)
            {
                _vrfTreeHeader.Header = "VRF";
                InsertOrAdd(SystemsAvailableForSelection, 0, _vrfTreeHeader);
            }
            else
            {
                SystemsAvailableForSelection.Remove(_vrfTreeHeader);
                //_vrfTreeHeader.Header = String.Empty;
            }

            if (_heatExchangerObservableCollection.Count > 0)
            {
                _heatExchangerTreeHeader.Header = "Heat Exchanger";
                InsertOrAdd(SystemsAvailableForSelection, 2, _heatExchangerTreeHeader);
            }
            else
            {
                SystemsAvailableForSelection.Remove(_heatExchangerTreeHeader);
                //heatExchangerTreeHeader.Header = String.Empty;
            }
        }

        private void InsertOrAdd<T>(ObservableCollection<T> collection, int position, T item)
        {
            if (!collection.Contains(item))
            {
                if (collection.Count > position)
                {
                    collection.Insert(position, item);
                }
                else
                {
                    collection.Add(item);
                }
            }
        }

        #region Properties

        /// <summary>
        /// Gets the LeftSideBarItems
        /// </summary>
        public ObservableCollection<CanvasListItem> SystemsAvailableForSelection { get; set; }

        #endregion

        public string _selectedItem;

        public string SelectedItem
        {
            get { return _selectedItem; }
            set { this.SetValue(ref _selectedItem, value); }
        }

        private bool _isEditable;
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                this.SetValue(ref _isEditable, value);
            }
        }

        private void btnEditGroupNameClickEvent()
        {
            IsEditable = !IsEditable;
        }

        private ObservableCollection<SelectedSystemViewModel> _selectedSystems;

        public ObservableCollection<SelectedSystemViewModel> SelectedSystems
        {
            get { return _selectedSystems; }
            set { this.SetValue(ref _selectedSystems, value); }
        }

        private ObservableCollection<SelectedCentralControllerViewModel> _selectedControllerSystems;

        public ObservableCollection<SelectedCentralControllerViewModel> SelectedControllerSystems
        {
            get { return _selectedControllerSystems; }
            set { this.SetValue(ref _selectedControllerSystems, value); }
        }


        private string _groupName;
        //[Required(ErrorMessage = "Required *")]
        //[CustomValidation(typeof(CustomValidations), nameof(CustomValidations.IsLength10))]                       
        public string GroupName
        {
            get { return _groupName; }
            set { this.SetValue(ref _groupName, value); }
        }

        private ImageSource _imageSource;

        public ImageSource ControllerImage
        {
            get { return _imageSource; }
            set { this.SetValue(ref _imageSource, value); }
        }

        private void RemoveFromSystemsAvailableForSelections(CanvasItemChild item)
        {
            var systype = (SystemBase)item.Source;
            switch (systype.HvacSystemType)
            {
                case "1":
                    _vrfSystemsObservableCollection.Remove((CanvasItemChild)item);
                    break;

                case "2":
                    _heatExchangerObservableCollection.Remove((CanvasItemChild)item);
                    break;
            }
            updateListItems();
        }

        private void AddToSystemsAvailableForSelection(CanvasItemChild item)
        {
            var systype = (SystemBase)item.Source;
            switch (systype.HvacSystemType)
            {
                case "1":
                    _vrfSystemsObservableCollection.Add((CanvasItemChild)item);
                    break;

                case "2":
                    _heatExchangerObservableCollection.Add((CanvasItemChild)item);
                    break;
            }
            updateListItems();
        }


        private void RemoveFromSelectedSystems(SelectedSystemViewModel selectedSystem)
        {
            SelectedSystems.Remove(selectedSystem);
            if (selectedSystem.System.HvacSystemType.Equals("1"))
            {
             List<string> strListContrG= ((NextGenModel.SystemVRF)selectedSystem.System).ControlGroupID;
                for (int i = 0; i < strListContrG.Count; i++)
                {
                    if(strListContrG[i].Equals(_group.Id))
                    ((NextGenModel.SystemVRF)selectedSystem.System).ControlGroupID.RemoveAt(i);
                }
              
                // ((NextGenModel.SystemVRF) selectedSystem.System).ControlGroupID = string.Empty;
            }
            else if (selectedSystem.System.HvacSystemType.Equals("2"))
            {
              
                var heatExchanger =
                    Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(selectedSystem.System.Id));

               // _group
                       List<string> strListContrG = (heatExchanger).ControlGroupID;
                for (int i = 0; i < strListContrG.Count; i++)
                {
                    if (strListContrG[i].Equals(_group.Id))
                        (heatExchanger).ControlGroupID.RemoveAt(i);
                }
                //  heatExchanger.ControlGroupID = string.Empty;   skm
                //   Project.CurrentProject.ControlGroupList
            }
            if (SelectedSystems.Count < 1 && SelectedControllerSystems.Count > 1)
            {
                ErrorLoggingForCentralController(60);
            }
            else
            {
                ClearErrorLogs();
            }
        }

        private void onSystemSelected(object obj)
        {
            if (obj != null && obj.GetType().Equals(typeof(CanvasItemChild)))
            {
                var sysObject = ((CanvasItemChild)obj).Source; //systemVRF or room indoor
                bool isValid = CheckSystemCompatibilityWithExistingControllers((SystemBase)sysObject);
                //validate with existing controllers.
                //if valid do below
                if (isValid)
                {
                    RemoveFromSystemsAvailableForSelections((CanvasItemChild)obj);
                    UpdateSelectedSystemsList((SystemBase)sysObject);
                    if (SelectedSystems.Count > 0)
                    {
                        ClearErrorLogs();
                    }
                }
                else
                {
                    // JCHMessageBox.Show("Selected system is Not Compatible with existing controllers");
                    //---------------- Below code added for multi-language----------//
                    JCHMessageBox.Show(Language.Current.GetMessage("SELECT_SYS"));
                    //else show message to user that selection is not compatible with existing controllers
                }
                UtilTrace.SaveHistoryTraces();
            }
        }

        public void UpdateSelectedSystemsList(SystemBase system)
        {
            try
            {
                SelectedSystemViewModel selectedSystemViewModel = new SelectedSystemViewModel();
                selectedSystemViewModel.SystemName = system.Name;
                selectedSystemViewModel.System = system;

                var idusCount = Project.CurrentProject.RoomIndoorList.FindAll(x => x.SystemID.Equals(system.Id)).Count;
                selectedSystemViewModel.CountVisibility = (system.HvacSystemType == "1") ? Visibility.Visible : Visibility.Collapsed;

                if (system.HvacSystemType.Equals("1"))
                {
                    selectedSystemViewModel.NoOfIDU = idusCount;
                    selectedSystemViewModel.NoOfODU = 1;

                    NextGenModel.SystemVRF systemVrf = (NextGenModel.SystemVRF)system;
                    if (systemVrf.IsPipingOK == true)
                    {
                        SelectedSystems.Add(selectedSystemViewModel);
                        if (!systemVrf.ControlGroupID.Contains(_group.Id))
                            systemVrf.ControlGroupID.Add(_group.Id);
                    }
                    else
                    {
                        //SelectedSystems.Remove(selectedSystemViewModel);
                        OnUnSelectSystem(selectedSystemViewModel);
                    }
                }
                else
                {
                    SelectedSystems.Add(selectedSystemViewModel);
                    SystemHeatExchanger heatExchanger = (SystemHeatExchanger)system;
                    var heatExchangerUnit =
                        Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(heatExchanger.Id));
                    if (!heatExchangerUnit.ControlGroupID.Contains(_group.Id))
                        heatExchangerUnit.ControlGroupID.Add(_group.Id);
                }

                OduCount = selectedSystemViewModel.NoOfODU;
                IduCount = selectedSystemViewModel.NoOfIDU;

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        public bool CheckSystemCompatibilityWithExistingControllers(SystemBase system)
        {
            controllerList.Clear();
            var productType = string.Empty;
            foreach (var sys in SelectedControllerSystems)
            {
                if (sys.Controller != null)
                {
                    controllerList.Add(sys.Controller.Model);
                    if (system.HvacSystemType.Equals("1"))
                    {
                        productType = ((NextGenModel.SystemVRF)system).OutdoorItem.ProductType;
                    }
                    else
                    {
                        RoomIndoor heatExchanger =
                            Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(system.Id));
                        productType = heatExchanger.IndoorItem.ProductType;
                    }
                }
            }

            var controlProductType = productTypeBll.GetControllerProductTypeData(Project.CurrentProject.SubRegionCode, Project.CurrentProject.BrandCode, controllerList);
            if (controlProductType != null)
                {
                    if (!controlProductType.Contains(productType))
                    {                       
                        return false;
                    }

                }
            return true;
        }

        internal void CheckCompatibility(DragEventArgs e)
        {
            if (SelectedSystems.Count == 0)
            {
                ErrorLoggingForCentralController(60);
            }
            else
            {
                ClearErrorLogs();
            }

            var SenderData = (NextGenModel.ImageData)e.Data.GetData(typeof(NextGenModel.ImageData));
            BitmapImage bitmap = new BitmapImage(new Uri(SenderData.imagePath));
            ImageSource img = bitmap;
            ControllerImage = img;
            CheckControllerCompatibility(SenderData); //image name is model name
        }

        private void CheckControllerCompatibility(NextGenModel.ImageData selectedControllerData)
        {
            bool isValid = true;
            controllerList.Clear();
            foreach (var ctrl in SelectedControllerSystems)
            {
                if (ctrl.Controller != null)
                {
                    controllerList.Add(ctrl.Controller.Model);
                }                
            }
            controllerList.Add(selectedControllerData.imageName);
            //Check all the Conditions are satisfied or not
            isValid = CheckBeforeAddingController(selectedControllerData);
            if (isValid)
            {
                //check if Dropped controller is already added
                var findcontroller = Project.CurrentProject.ControllerList.Find(x =>
                x.ControlGroupID.Equals(_group.Id) && x.Model.Equals(selectedControllerData.imageName));
                if (findcontroller != null)
                {
                    if (!CheckBeforeAddController(findcontroller))
                    {
                        return;
                    }
                    //increment the count for the project level
                    findcontroller.Quantity = findcontroller.Quantity + 1;
                    _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
                    //increment on the view part
                    foreach (var selectedControllerViewModel in SelectedControllerSystems)
                    {
                        if (!string.IsNullOrEmpty(selectedControllerViewModel.ModelName) &&
                            selectedControllerViewModel.ModelName.Equals(findcontroller.Model))
                        {
                            selectedControllerViewModel.Quantity = selectedControllerViewModel.Quantity + 1;
                            IncrementControllersInSet(selectedControllerViewModel);
                        }
                    }
                    ValidateMandatoryCompatibleControllers();
                    UtilTrace.SaveHistoryTraces();                                    
                }            
                else
                {
                    var centralController = (CentralController)selectedControllerData.Source;
                    Controller centralControllerForCheck = new Controller(centralController);
                    if (!CheckBeforeAddController(centralControllerForCheck))
                    {
                        return;
                    }
                    var ctrlController = AddControllerToProject(centralController);
                    UpdateSelectedControllerSystems(ctrlController, true);
                    ValidateMandatoryCompatibleControllers();
                    UtilTrace.SaveHistoryTraces();
                }                
            }
        }

        private bool CheckBeforeAddingController(NextGenModel.ImageData selectedControllerData)
        {
            if (CheckForSystems(true) == false)
            {
                return false;
            }
            //No limitation for EU region
            if (Project.CurrentProject.RegionCode.StartsWith("EU"))return true;

            if (CheckCombinations((CentralController) selectedControllerData.Source) == false)
            {
                return false;
            }
            
            return true;
        }

        private bool CheckForSystems(bool valid)
        {
            string productType = string.Empty;
            var controlProductType = productTypeBll.GetControllerProductTypeData(Project.CurrentProject.SubRegionCode,
                Project.CurrentProject.BrandCode, controllerList);
            if (SelectedSystems.Count > 0)
            {
                foreach (var sys in SelectedSystems)
                {
                    if (sys.System.HvacSystemType.Equals("1"))
                    {
                        productType = ((NextGenModel.SystemVRF)sys.System).OutdoorItem.ProductType;
                    }
                    else
                    {
                        RoomIndoor heatExchanger =
                            Project.CurrentProject.ExchangerList.Find(x => x.SystemID.Equals(sys.System.Id));
                        productType = heatExchanger.IndoorItem.ProductType;
                    }

                    if (controlProductType != null)
                    {
                        if (!controlProductType.Contains(productType))
                        {
                            JCHMessageBox.Show(Msg.ERR_CONTROLLER_PRODUCTTYPE_COMPATIBILITY(sys.System.Name));
                            valid = false;
                            break;
                        }
                    }
                }
            }
            return valid;
        }

        private void IncrementControllersInSet(SelectedCentralControllerViewModel selectedControllerViewModel)
        {
            var centralControllerControllersInSet = selectedControllerViewModel.Controller.CentralController.ControllersInSet;
            if (centralControllerControllersInSet != null)
            {

                foreach (SelectedCentralControllerViewModel viewModel in SelectedControllerSystems)
                {
                    if (viewModel.Controller != null)
                    {
                        if (selectedControllerViewModel.Equals(viewModel))
                        {
                            continue;
                        }

                        if (centralControllerControllersInSet.Contains(viewModel.Controller.CentralController))
                        {
                            viewModel.Quantity = viewModel.Quantity + 1;
                            viewModel.Controller.Quantity = viewModel.Quantity;
                        }
                    }
                }
            }
        }

        private void DeleteControllersInSet(SelectedCentralControllerViewModel selectedControllerViewModel)
        {
            var centralControllerControllersInSet = selectedControllerViewModel.Controller.CentralController.ControllersInSet;
            if (centralControllerControllersInSet != null)
            {
                var viewModelsToDelete = new List<SelectedCentralControllerViewModel>();
                foreach (SelectedCentralControllerViewModel viewModel in SelectedControllerSystems)
                {
                    if (viewModel.Controller != null)
                    {
                        if (selectedControllerViewModel.Equals(viewModel))
                        {
                            continue;
                        }

                        if (centralControllerControllersInSet.Contains(viewModel.Controller.CentralController))
                        {
                            if (viewModel.Quantity > 1)
                            {
                                viewModel.Quantity = viewModel.Quantity - 1;
                                viewModel.Controller.Quantity = viewModel.Quantity;
                            }
                            else
                            {
                                Project.CurrentProject.ControllerList.Remove(viewModel.Controller);
                                viewModelsToDelete.Add(viewModel);
                            }

                        }
                    }
                }

                foreach (var viewModel in viewModelsToDelete)
                {
                    SelectedControllerSystems.Remove(viewModel);
                }


            }
        }

        private Controller AddControllerToProject(CentralController centralController)
        {
            Controller ctrlController = new Controller(centralController);
            ctrlController.AddToControlGroup(_group.Id);
            ctrlController.Quantity = 1;
            ctrlController.AddToControlSystem(_group.ControlSystemID);
            Project.CurrentProject.ControllerList.Add(ctrlController);
            return ctrlController;
        }

        private void AddControllerInSet(List<CentralController> src2Controller)
        {
            foreach (var sysController in src2Controller)
            {
                var found = false;
                foreach (SelectedCentralControllerViewModel viewModel in SelectedControllerSystems)
                {
                    if (viewModel.Controller != null)
                    {
                        if (sysController.Equals(viewModel.Controller.CentralController))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    var controller = AddControllerToProject(sysController);
                    UpdateSelectedControllerSystems(controller, true);
                }
            }
        }

        private bool CheckCombinations(CentralController newType)
        {
            foreach (var existingController in SelectedControllerSystems)
            {
                if (existingController.Controller != null)
                {
                    CentralController existControllerType = existingController.Controller.CentralController;
                    if (newType == existControllerType) continue;

                    if (CheckControllerCompatible(newType, existControllerType, 1) == false) return false;
                }
            }
            return true;
        }

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
                JCHMessageBox.Show(Language.Current.GetMessage("ERROR_CONTROLLER"));

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
                        JCHMessageBox.Show(Language.Current.GetMessage("ERROR_NOT_COMPATIABLE") + (number == 1 ? type2.Model : type1.Model));
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

        private void UpdateSelectedControllerSystems(Controller controller, bool addControllersInSet)
        {
            SelectedCentralControllerViewModel centralControllerViewModel = new SelectedCentralControllerViewModel();
            centralControllerViewModel.ModelName = controller.Model;
            centralControllerViewModel.Image = GetImagesSourceDir() + "\\" + controller.Image;
            centralControllerViewModel.Quantity = controller.Quantity;
            centralControllerViewModel.Controller = controller;
            SelectedControllerSystems.Insert(0, centralControllerViewModel);


            var centralControllerControllersInSet = controller.CentralController.ControllersInSet;
            if (addControllersInSet && centralControllerControllersInSet != null)
            {
                AddControllerInSet(centralControllerControllersInSet);

            }

            _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
        }

        private string GetImagesSourceDir()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\ControllerImage";

            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        private void GetImagesSourceDir(ref string path)
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image";

            path = System.IO.Path.Combine(defaultFolder, navigateToFolder);
        }

        private BitmapImage boxImage;

        public BitmapImage BoxImage
        {
            get { return boxImage; }
            set { this.SetValue(ref boxImage, value); }
        }

        public DelegateCommand LostFocusGroupName { get; set; }

        public class SelectedCentralControllerViewModel : ViewModelBase
        {
            public string ModelName { get; set; }
            public string Image { get; set; }

            public int _qty;

            public int Quantity
            {
                get { return _qty; }

                set { SetValue(ref _qty, value); }
                //set
                //{
                //    _qty = value;
                //}
            }

            private string _textdragImage;

            public string TextdragImage
            {
                get { return _textdragImage; }

                set { SetValue(ref _textdragImage, value); }
                //set
                //{
                //    _textdragImage = value;
                //}
            }

            public Controller Controller { get; internal set; }

        }

        public void ValidateMandatoryCompatibleControllers()
        {
            try
            {
                if (SelectedControllerSystems.Count > 1 && Project.CurrentProject.RegionCode.StartsWith("EU"))
                {
                    if (SelectedSystems.Count > 0)
                    {
                        foreach (var vrfsys in SelectedSystems)
                        {
                            if (vrfsys.System.HvacSystemType == "2")
                            {
                                //Does nothing when it is heat exchanger
                            }
                            else
                            {
                                var SelOutdoorType = ((NextGenModel.SystemVRF)vrfsys.System).SelOutdoorType;

                                if (SelOutdoorType == "FSXNPE (Top discharge)" || SelOutdoorType == "FSXNSE (Top discharge)")
                                {
                                    Dictionary<string, int> dicNum = new Dictionary<string, int>();
                                    dicNum["CSNET MANAGER 2 T10"] = 0;
                                    dicNum["CSNET MANAGER 2 T15"] = 0;
                                    dicNum["HC-A64NET"] = 0;
                                    dicNum["PSC-A160WEB1"] = 0;

                                    foreach (var item in SelectedControllerSystems)
                                    {
                                        if (item.Controller != null)
                                        {
                                            string model = item.Controller.Model;
                                            switch (model)
                                            {
                                                case "CSNET MANAGER 2 T10": dicNum["CSNET MANAGER 2 T10"] = item.Quantity; break;
                                                case "CSNET MANAGER 2 T15": dicNum["CSNET MANAGER 2 T15"] = item.Quantity; break;
                                                case "HC-A64NET": dicNum["HC-A64NET"] = item.Quantity; break;
                                                case "PSC-A160WEB1": dicNum["PSC-A160WEB1"] = item.Quantity; break;
                                                default: ClearErrorLogs(); break;
                                            }
                                        }
                                    }

                                    if ((dicNum["CSNET MANAGER 2 T10"] + dicNum["CSNET MANAGER 2 T15"]) > 0)
                                    {
                                        if ((dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"]) <= 0)
                                        {
                                            ErrorLoggingForCentralController(61);
                                            //JCHMessageBox.Show("PSC-A160WEB1 or  HC-A64NET must be added.");
                                            //---------------- Below code added for multi-language----------//
                                            JCHMessageBox.Show(Language.Current.GetMessage("MUST_ADDED"));
                                            break;
                                        }
                                        ClearErrorLogs();
                                    }

                                    if (dicNum["HC-A64NET"] > 0 || dicNum["PSC-A160WEB1"] > 0)
                                    {
                                        if ((dicNum["CSNET MANAGER 2 T10"] + dicNum["CSNET MANAGER 2 T15"]) <= 0)
                                        {
                                            ErrorLoggingForCentralController(62);
                                            //JCHMessageBox.Show("CSNET MANAGER 2 T10 or CSNET MANAGER 2 T15 must be selected");
                                            //---------------- Below code added for multi-language----------//
                                            JCHMessageBox.Show(Language.Current.GetMessage("CSNET"));
                                            break;
                                        }
                                        else if (SelectedSystems.Count < dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"])
                                        {
                                            ErrorLoggingForCentralController(63);
                                            //JCHMessageBox.Show("Selected System Count must be Equal or Greater than count of HSC-A64NET or PSC-A160WEB1");
                                            //---------------- Below code added for multi-language----------//
                                            JCHMessageBox.Show(Language.Current.GetMessage("ERROR_SYSTEM_COUNT"));

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
            }
            catch (Exception e)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, e);
            }
        }

        public void ClearErrorLogs()
        {
            _currentSystem._errors.Clear();
            _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
            _currentSystem.SystemStatus = SystemStatus.WIP;
            //_group.IsValid = true;
        }

        public void ErrorLoggingForCentralController(int errorId)
        {
            _group.IsValid = false;
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
                    if (!_currentSystem._errors.Contains("Selected System Count must be Equal or Greater than count of HSC-A64NET or PSC-A160WEB1"))
                    {
                        _currentSystem._errors.Add("Selected System Count must be Equal or Greater than count of HSC-A64NET or PSC-A160WEB1");
                    }
                    break;
                case 60:
                    if (!_currentSystem._errors.Contains("No Systems Selected"))
                    {
                        _currentSystem._errors.Add("No Systems Selected");
                    }
                    break;
            }

            _currentSystem.SystemStatus = SystemStatus.INVALID;
            _eventAggregator.GetEvent<ErrorLogVM>().Publish(_currentSystem.Errors);
            // _currentSystem.Errors.Clear();
        }
    }
}


