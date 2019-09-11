

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using System.Windows;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{

    public class EditReportContentsViewModel : ViewModelBase
    {
        #region Fields
        IRegionManager regionManager;
        public DelegateCommand uc_CheckBox_RptSelectAllModuleCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptSelectAllModuleUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptRoomInfoCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptRoomInfoUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptOutdoorDetailCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptWiringCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptOutdoorDetailUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptWiringUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptIndoorDetailCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptIndoorDetailUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptPipingCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptPipingUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptControllerCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptControllerUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptExchangerCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_RptExchangerUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_ActualCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_ActualUnCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_NominalCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_NominalUnCheckedCommand { get; set; }

        public DelegateCommand uc_CheckBox_IduCapacityWCheckedCommand { get; set; }
        public DelegateCommand uc_CheckBox_IduCapacityWUnCheckedCommand { get; set; }

        readonly IEventAggregator _eventAggregator;
        private string _imageRelativePath = @"/JCHVRF_New;component/Image/";
        #endregion
        public DelegateCommand treeView1 { get; set; }
        public DelegateCommand<object> SelectedTreeViewItemCommand { get; set; }
        public DelegateCommand CancelClickCommand { get; private set; }
        public DelegateCommand Apply_Click { get; private set; }

       // public DelegateCommand Apply_Click { get; set; }
        private LeftSideBarChild _selectedLeftSideBarChild;
        private ObservableCollection<LeftSideBarChild> _vrfSystemsObservableCollection;
        private ObservableCollection<LeftSideBarChild> _centralContollerObservableCollection;
        private ObservableCollection<LeftSideBarChild> _heatExchangerObservableCollection;
        private LeftSideBarItem _centarlControllerItemsSideBarItem;
        private LeftSideBarItem _heatExchangerLeftSideBarItem;
        private LeftSideBarItem _vrfLeftSideBarItem;
        private IModalWindowService _winService;

        public ObservableCollection<JCHVRF_New.Model.LeftSideBarItem> LeftSideBarItems { get; set; }

        public EditReportContentsViewModel(IEventAggregator eventAggregator, IProjectInfoBAL projectInfoBll, IModalWindowService winService)
        {
            if (SystemSetting.UserSetting.unitsSetting.settingPOWER == "kW")
            {
                this.VisisbleIduCapacityW = Visibility.Visible;
            }
            else
            {
                this.VisisbleIduCapacityW = Visibility.Collapsed;
            }
            this._eventAggregator = eventAggregator;
            LeftSideBarItems = new ObservableCollection<LeftSideBarItem>();
            _winService = winService;
            uc_CheckBox_RptSelectAllModuleCheckedCommandEvent();
            uc_CheckBox_ActualCheckedCommandEvent();

            Apply_Click = new DelegateCommand(this.OnApplyClicked);
            CancelClickCommand = new DelegateCommand(this.CancelClick);

            uc_CheckBox_RptSelectAllModuleCheckedCommand = new DelegateCommand(uc_CheckBox_RptSelectAllModuleCheckedCommandEvent);
            uc_CheckBox_RptSelectAllModuleUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptSelectAllModuleUnCheckedCommandEvent);
            uc_CheckBox_RptRoomInfoCheckedCommand = new DelegateCommand(uc_CheckBox_RptRoomInfoCheckedCommandEvent);
            uc_CheckBox_RptRoomInfoUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptRoomInfoUnCheckedCommandEvent);
            uc_CheckBox_RptOutdoorDetailCheckedCommand = new DelegateCommand(uc_CheckBox_RptOutdoorDetailCheckedCommandEvent);
            uc_CheckBox_RptWiringCheckedCommand = new DelegateCommand(uc_CheckBox_RptWiringCheckedCommandEvent);
            uc_CheckBox_RptOutdoorDetailUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptOutdoorDetailUnCheckedCommandEvent);
            uc_CheckBox_RptWiringUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptWiringUnCheckedCommandEvent);
            uc_CheckBox_RptIndoorDetailCheckedCommand = new DelegateCommand(uc_CheckBox_RptIndoorDetailCheckedCommandEvent);
            uc_CheckBox_RptIndoorDetailUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptIndoorDetailUnCheckedCommandEvent);
            uc_CheckBox_RptPipingCheckedCommand = new DelegateCommand(uc_CheckBox_RptPipingCheckedCommandEvent);
            uc_CheckBox_RptPipingUnCheckedCommand = new DelegateCommand(uc_uc_CheckBox_RptPipingUnCheckedCommandEvent);
            uc_CheckBox_RptControllerCheckedCommand = new DelegateCommand(uc_CheckBox_RptControllerCheckedCommandEvent);
            uc_CheckBox_RptControllerUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptControllerUnCheckedCommandEvent);
            uc_CheckBox_RptExchangerCheckedCommand = new DelegateCommand(uc_CheckBox_RptExchangerCheckedCommandEvent);
            uc_CheckBox_RptExchangerUnCheckedCommand = new DelegateCommand(uc_CheckBox_RptExchangerUnCheckedCommandEvent);
            uc_CheckBox_ActualCheckedCommand = new DelegateCommand(uc_CheckBox_ActualCheckedCommandEvent);
            uc_CheckBox_ActualUnCheckedCommand = new DelegateCommand(uc_CheckBox_ActualUnCheckedCommandEvent);
            uc_CheckBox_NominalCheckedCommand = new DelegateCommand(uc_CheckBox_NominalCheckedCommandEvent);
            uc_CheckBox_IduCapacityWCheckedCommand = new DelegateCommand(uc_CheckBox_IduCapacityCheckedCommandEvent);
            uc_CheckBox_IduCapacityWUnCheckedCommand = new DelegateCommand(uc_CheckBox_IduCapacityUnCheckedCommandEvent);
            uc_CheckBox_NominalUnCheckedCommand = new DelegateCommand(uc_CheckBox_NominalUnCheckedCommandEvent);
            var proj = JCHVRF.Model.Project.GetProjectInstance;
            if (proj.IsIduCapacityW != null)
            {
                IsIduCapacityW = proj.IsIduCapacityW;
            }

            //  _eventAggregator.GetEvent<SystemCreated>().Subscribe(NewSystemCreated);
            //   _eventAggregator.GetEvent<SystemSelectedTabSubscriber>().Subscribe(OnSelectedSystemTab);
        }

        private void OnApplyClicked()
        {
                var proj = JCHVRF.Model.Project.GetProjectInstance;
                proj.IsRoomInfoChecked = IsRoomInfoChecked;
                proj.IsOutdoorListChecked = IsOutdoorListChecked;
                proj.IsWiringDiagramChecked = IsWiringDiagramChecked;
                proj.IsIndoorListChecked = IsIndoorListChecked;
                proj.IsPipingDiagramChecked = IsPipingDiagramChecked;
                proj.IsControllerChecked = IsControllerChecked;
                proj.IsExchangerChecked = IsExchangerChecked;
                proj.IsActualCapacityChecked = IsActualCapacityChecked;
                proj.IsNormalCapacityChecked = IsNormalCapacityChecked;
                proj.IsIduCapacityW = IsIduCapacityW;
                JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_SUCCESSFULLY_APPLIED"));//"Successfully applied"
                _winService.Close(ViewKeys.EditReportContents);
                JCHVRF.Model.Project.CurrentProject.SystemListNextGen.ForEach((p) =>
                {
                    if (p.IsExportToReport==false)
                         p.editreportapply = true;
                    else
                        p.editreportapply = false;
                });
        }
        private void CancelClick()
        {
                var proj = JCHVRF.Model.Project.GetProjectInstance;
                proj.IsRoomInfoChecked = true;
                proj.IsOutdoorListChecked = true;
                proj.IsWiringDiagramChecked = true;
                proj.IsIndoorListChecked = true;
                proj.IsPipingDiagramChecked = true;
                proj.IsControllerChecked = true;
                proj.IsExchangerChecked = true;
                proj.IsActualCapacityChecked = true;
                proj.IsNormalCapacityChecked = false;
                JCHVRF.Model.Project.CurrentProject.SystemListNextGen.ForEach((p) =>
                {
                    if (p.editreportapply==false)
                         p.IsExportToReport = true;
                    else
                        p.IsExportToReport = false;
                });
                _winService.Close(ViewKeys.EditReportContents);
        }
        private void OnCancelClicked()
        {

        }

        private Visibility _visisbleIduCapacityW;
        public Visibility VisisbleIduCapacityW
        {
            get { return _visisbleIduCapacityW; }
            set
            {
                this.SetValue(ref _visisbleIduCapacityW, value);

            }
        }
        private bool _uc_CheckBox_RptSelectAllModuleChecked;
        public bool uc_CheckBox_RptSelectAllModuleChecked
        {
            get { return _uc_CheckBox_RptSelectAllModuleChecked; }
            set { this.SetValue(ref _uc_CheckBox_RptSelectAllModuleChecked, value); }
        }

        private bool _isRoomInfoChecked;

        public bool IsRoomInfoChecked
        {
            get { return _isRoomInfoChecked; }
            set { this.SetValue(ref _isRoomInfoChecked, value); }

        }
        private bool _isOutdoorListChecked;
        public bool IsOutdoorListChecked
        {
            get { return _isOutdoorListChecked; }
            set { this.SetValue(ref _isOutdoorListChecked, value); }

        }
        private bool _isWiringDiagramChecked;
        public bool IsWiringDiagramChecked
        {
            get { return _isWiringDiagramChecked; }
            set { this.SetValue(ref _isWiringDiagramChecked, value); }
        }
        private bool _isIndoorListChecked;
        public bool IsIndoorListChecked
        {
            get { return _isIndoorListChecked; }
            set { this.SetValue(ref _isIndoorListChecked, value); }
        }
        private bool _isPipingDiagramChecked;

        public bool IsPipingDiagramChecked
        {
            get { return _isPipingDiagramChecked; }
            set { this.SetValue(ref _isPipingDiagramChecked, value); }
        }
        private bool _isControllerChecked;
        public bool IsControllerChecked
        {
            get { return _isControllerChecked; }
            set { this.SetValue(ref _isControllerChecked, value); }
        }
        private bool _isExchangerChecked;
        public bool IsExchangerChecked
        {
            get { return _isExchangerChecked; }
            set { this.SetValue(ref _isExchangerChecked, value); }
        }
        private bool _IsActualCapacityChecked;
        public bool IsActualCapacityChecked
        {
            get { return _IsActualCapacityChecked; }
            set { this.SetValue(ref _IsActualCapacityChecked, value); }
        }

        private bool _IsIduCapacityW;
        public bool IsIduCapacityW
        {
            get { return _IsIduCapacityW; }
            set { this.SetValue(ref _IsIduCapacityW, value); }
        }
        private bool _IsNormalCapacityChecked;
        public bool IsNormalCapacityChecked
        {
            get { return _IsNormalCapacityChecked; }
            set { this.SetValue(ref _IsNormalCapacityChecked, value); }
        }
        private void uc_CheckBox_RptSelectAllModuleCheckedCommandEvent()
        {
            uc_CheckBox_RptSelectAllModuleChecked = true;
            IsRoomInfoChecked = true;
            IsIndoorListChecked = true;
            IsOutdoorListChecked = true;
            IsPipingDiagramChecked = true;
            IsWiringDiagramChecked = true;
            IsControllerChecked = true;
            IsExchangerChecked = true;
        }
        private void uc_CheckBox_RptSelectAllModuleUnCheckedCommandEvent()
        {
            uc_CheckBox_RptSelectAllModuleChecked = false;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                IsRoomInfoChecked = false;
                IsIndoorListChecked = false;
                IsOutdoorListChecked = false;
                IsPipingDiagramChecked = false;
                IsWiringDiagramChecked = false;
                IsControllerChecked = false;
                IsExchangerChecked = false;
            }
        }
        private void uc_CheckBox_RptRoomInfoCheckedCommandEvent()
        {
            IsRoomInfoChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_CheckBox_RptRoomInfoUnCheckedCommandEvent()
        {
            IsRoomInfoChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_RptOutdoorDetailCheckedCommandEvent()
        {
            IsOutdoorListChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_CheckBox_RptWiringCheckedCommandEvent()
        {
            IsWiringDiagramChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_CheckBox_RptOutdoorDetailUnCheckedCommandEvent()
        {
            IsOutdoorListChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_RptWiringUnCheckedCommandEvent()
        {
            IsWiringDiagramChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_RptIndoorDetailCheckedCommandEvent()
        {
            IsIndoorListChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_CheckBox_RptIndoorDetailUnCheckedCommandEvent()
        {
            IsIndoorListChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_RptPipingCheckedCommandEvent()
        {
            IsPipingDiagramChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_uc_CheckBox_RptPipingUnCheckedCommandEvent()
        {
            IsPipingDiagramChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_RptControllerCheckedCommandEvent()
        {
            IsControllerChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_CheckBox_RptControllerUnCheckedCommandEvent()
        {
            IsControllerChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_RptExchangerCheckedCommandEvent()
        {
            IsExchangerChecked = true;
            if (IsRoomInfoChecked == true && IsIndoorListChecked == true && IsOutdoorListChecked == true && IsPipingDiagramChecked == true &&
                 IsWiringDiagramChecked == true && IsControllerChecked == true && IsExchangerChecked == true)
            {
                uc_CheckBox_RptSelectAllModuleChecked = true;
            }
        }
        private void uc_CheckBox_RptExchangerUnCheckedCommandEvent()
        {
            IsExchangerChecked = false;
            if (uc_CheckBox_RptSelectAllModuleChecked == true)
                uc_CheckBox_RptSelectAllModuleChecked = false;
        }
        private void uc_CheckBox_ActualCheckedCommandEvent()
        {
            IsActualCapacityChecked = true;
            if (IsNormalCapacityChecked == true)
                IsNormalCapacityChecked = false;
        }
        private void uc_CheckBox_ActualUnCheckedCommandEvent()
        {
            IsActualCapacityChecked = false;
        }
        private void uc_CheckBox_NominalCheckedCommandEvent()
        {

            if (IsActualCapacityChecked == true)
                IsActualCapacityChecked = false;
            IsNormalCapacityChecked = true;
        }
        private void uc_CheckBox_IduCapacityCheckedCommandEvent()
        {

                IsIduCapacityW = true;
        }
        private void uc_CheckBox_IduCapacityUnCheckedCommandEvent()
        {
            IsIduCapacityW = false;
        }
        private void uc_CheckBox_NominalUnCheckedCommandEvent()
        {
            IsNormalCapacityChecked = false;
        }

        private void SelectedTreeViewItemClick(object leftSideBarChild)
        {
            if (leftSideBarChild.GetType().Equals(typeof(LeftSideBarChild)))
            {
                var object2 = ((LeftSideBarChild)leftSideBarChild).Source;
                _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)object2);
            }
        }
        private void OnSelectedSystemTab(SystemBase system)
        {
            string systemType = system.HvacSystemType;
            int systemTypeAsInt = Convert.ToInt32(systemType);
            switch (systemTypeAsInt)
            {
                case 1:
                    UpdateItemSelection(system, _vrfSystemsObservableCollection);
                    break;

                case 6:
                    UpdateItemSelection(system, _centralContollerObservableCollection);
                    break;

                case 2:
                    UpdateItemSelection(system, _heatExchangerObservableCollection);
                    break;
            }
        }
        private void UpdateItemSelection(SystemBase system, ObservableCollection<LeftSideBarChild> collection)
        {
            foreach (LeftSideBarChild vrfleftSideBarChildItem in collection)
            {
                if (system.Name.Equals(((SystemBase)vrfleftSideBarChildItem.Source).Name))
                {
                    if (!vrfleftSideBarChildItem.IsSelected)
                    {
                        vrfleftSideBarChildItem.IsSelected = true;
                    }
                }
            }
        }
        private void NewSystemCreated(SystemBase system)
        {
            string systemType = system.HvacSystemType;
            int systemTypeAsInt = Convert.ToInt32(systemType);
            switch (systemTypeAsInt)
            {
                case 1:
                    LeftSideBarChild child = new LeftSideBarChild(system.Name, "VRF", system.StatusIcon, system);
                    child.IsSelected = true;
                    _vrfSystemsObservableCollection.Add(child);
                    break;

                case 2:
                    child = new LeftSideBarChild(system.Name, "Heat Exchanger", system.StatusIcon, system);
                    child.IsSelected = true;
                    _heatExchangerObservableCollection.Add(child);
                    break;

                case 6:

                    child = new LeftSideBarChild(system.Name, "Central Controller", system.StatusIcon, system);
                    child.IsSelected = true;
                    _centralContollerObservableCollection.Add(child);
                    break;
            }
            updateLeftSideBarItems();
            _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)system);
        }
        private string GetSystemStatusImg(JCHVRF.Model.NextGen.SystemVRF system)
        {
            switch (system.SystemStatus)
            {
                case JCHVRF_New.Model.SystemStatus.WIP:
                    return _imageRelativePath + "Path 111.png";

                case JCHVRF_New.Model.SystemStatus.VALID:
                    return _imageRelativePath + "Path 113.png";

                case JCHVRF_New.Model.SystemStatus.INVALID:
                    return _imageRelativePath + "Path 112.png";

                default:
                    return _imageRelativePath + "Path 111.png";
            }
        }
        private void DisplayProjectDetails(IProjectInfoBAL projectInfoBll)
        {
            var currentProject = JCHVRF.Model.Project.GetProjectInstance;
            //currentProject = projectInfoBll.GetProjectInfo(currentProject.projectID).ProjectLegacy; //CurrentProject object is already being filled in the line above. commenting this to fix bug#2234

            List<LeftSideBarChild> vrfSystems = new List<LeftSideBarChild>();
            foreach (JCHVRF.Model.NextGen.SystemVRF systemVRF in currentProject.SystemListNextGen)
            {
                vrfSystems.Add(new LeftSideBarChild(systemVRF.Name, "VRF", systemVRF.StatusIcon, systemVRF));
            }
            _vrfSystemsObservableCollection.AddRange(vrfSystems);

            List<LeftSideBarChild> centralControllers = new List<LeftSideBarChild>();
            foreach (JCHVRF.Model.ControlSystem controlSystem in currentProject.ControlSystemList)
            {
                centralControllers.Add(new LeftSideBarChild(controlSystem.Name, "VRF", controlSystem.StatusIcon, controlSystem));
            }

            _centralContollerObservableCollection.AddRange(centralControllers);

            List<LeftSideBarChild> HeatExchangerSystems = new List<LeftSideBarChild>();
            foreach (JCHVRF.Model.SystemHeatExchanger systemHeatExch in currentProject.HeatExchangerSystems)
            {
                HeatExchangerSystems.Add(new LeftSideBarChild(systemHeatExch.Name, "VRF", systemHeatExch.StatusIcon, systemHeatExch));
            }


            _heatExchangerObservableCollection.AddRange(HeatExchangerSystems);

            if (vrfSystems.Count > 0)
            {
                _selectedLeftSideBarChild = vrfSystems[0];
            }
            else if (centralControllers.Count > 0)
            {
                _selectedLeftSideBarChild = centralControllers[0];
            }
            else if (HeatExchangerSystems.Count > 0)
            {
                _selectedLeftSideBarChild = HeatExchangerSystems[0];
            }

            updateLeftSideBarItems();

            _selectedLeftSideBarChild.IsSelected = true;
            _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)_selectedLeftSideBarChild.Source);
        }

        private void updateLeftSideBarItems()
        {
            if (_vrfSystemsObservableCollection.Count > 0)
            {
                // InsertOrAdd(LeftSideBarItems, 0, _vrfLeftSideBarItem);

            }

            if (_centralContollerObservableCollection.Count > 0)
            {
                // InsertOrAdd(LeftSideBarItems, 1, _centarlControllerItemsSideBarItem);
            }

            if (_heatExchangerObservableCollection.Count > 0)
            {
                //InsertOrAdd(LeftSideBarItems, 2, _heatExchangerLeftSideBarItem);
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

    }



}
