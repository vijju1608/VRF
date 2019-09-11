/****************************** File Header ******************************\
File Name:          MasterDesignerViewModel.cs
Date Created:    2/15/2019
Description:        View Model for the MasterDesigner.
\*************************************************************************/

using Prism.Events;
using JCHVRF_New.Views;
using WL = Lassalle.WPF.Flow;

using JCBase.Utility;
namespace JCHVRF_New.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using JCHVRF_New.Common.Constants;
    using JCHVRF_New.Common.Helpers;
    using Lassalle.Flow;
    using Prism.Commands;
    using Prism.Regions;
    using JCHVRF.MyPipingBLL.NextGen;
    using JCHVRF.Model.NextGen;
    using JCHVRF.BLL;
    using JCHVRF.Model;
    using System.IO;
    using System.Linq;
    using JCHVRF.BLL.New;
    using Common.Contracts;
    using JCHVRF;
    using JCHVRF.Report;
    using CDF;
    using JCBase.UI;
    using forms = System.Windows.Forms;
    using JCHVRF.VRFMessage;
    using System.Runtime.Serialization.Formatters.Binary;
    using WL = Lassalle.WPF.Flow;
    using NextGenModel = JCHVRF.Model.NextGen;
    using drawing = System.Drawing;
    using System.Drawing.Imaging;
    using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
    using newUtilTrace = JCHVRF_New.Utility;
    using System.Data;
    using Microsoft.VisualBasic.CompilerServices;
    using System.Runtime.CompilerServices;
    using System.Collections;
    using JCHVRF.Const;
    using System.Drawing;
    using cwb = JCHVRF.MyPipingBLL;
    using JCHVRF_New.Utility;
    using pt = System.Windows;

    using language = JCHVRF_New.LanguageData.LanguageViewModel;
    using JCHVRF_New.Model;
    using System.Windows.Input;
    using Svg;

    public class MasterDesignerViewModel : ViewModelBase
    {
        public DelegateCommand<string> LayoutViewChangedCommand { get; set; }

        private List<AnchorableHandle> _anchorables;
        
        public List<AnchorableHandle> Anchorables
        {
            get {
                if (_anchorables == null)
                {
                    _anchorables = new List<AnchorableHandle>() { ProjectDetailsHandle, SystemDetailsHandle, PipingInfoHandle, PropertiesHandle, ErrorLogHandle, NavigatorHandle };
                }
                return _anchorables; }
        }



        private AnchorableHandle _projectDetailsHandle;

        public AnchorableHandle ProjectDetailsHandle
        {
            get {
                if (_projectDetailsHandle == null) {
                    _projectDetailsHandle = new AnchorableHandle()
                    {
                        Title = language.Current.GetMessage("PROJECT_DETAILS"),
                        FloaterHeight = 410,
                        FloaterWidth = 200,
                        FloaterLeft = 500,
                        FloaterTop = 200,
                        IsPaneVisible = true
                    };
                }
                return this._projectDetailsHandle;
            }
        }

        private AnchorableHandle _systemDetailsHandle;

        public AnchorableHandle SystemDetailsHandle
        {
            get
            {
                if (_systemDetailsHandle == null)
                {
                    _systemDetailsHandle = new AnchorableHandle()
                    {
                        Title = language.Current.GetMessage("SYSTEM_DETAILS"),
                        FloaterHeight = 410,
                        FloaterWidth = 200,
                        FloaterLeft = 500,
                        FloaterTop = 200,
                        IsPaneVisible = true
                    };
                }
                return this._systemDetailsHandle;
            }
        }

        private AnchorableHandle _pipingInfoHandle;

        public AnchorableHandle PipingInfoHandle
        {
            get
            {
                if (_pipingInfoHandle == null)
                {
                    _pipingInfoHandle = new AnchorableHandle()
                    {
                        Title = language.Current.GetMessage("PIPING_INFO"),
                        FloaterHeight = 500,
                        FloaterWidth = 700,
                        FloaterLeft = 100,
                        FloaterTop = 100,
                        IsPaneVisible = true
                    };
                }
                return this._pipingInfoHandle;
            }
        }

        private AnchorableHandle _propertiesHandle;

        public AnchorableHandle PropertiesHandle
        {
            get
            {
                if (_propertiesHandle == null)
                {
                    _propertiesHandle = new AnchorableHandle()
                    {
                        Title = language.Current.GetMessage("PROPERTIES"),
                        FloaterHeight = 370,
                        FloaterWidth = 320,
                        FloaterLeft = 500,
                        FloaterTop = 200,
                        IsPaneVisible = true
                    };
                }
                return this._propertiesHandle;
            }
        }

        private AnchorableHandle _errorLogHandle;

        public AnchorableHandle ErrorLogHandle
        {
            get
            {
                if (_errorLogHandle == null)
                {
                    _errorLogHandle = new AnchorableHandle()
                    {
                        Title = language.Current.GetMessage("ERROR_LOG"),
                        FloaterHeight = 250,
                        FloaterWidth = 300,
                        FloaterLeft = 500,
                        FloaterTop = 200,
                        IsPaneVisible = true
                    };
                }
                return this._errorLogHandle;
            }
        }

        private AnchorableHandle _navigatorHandle;

        public AnchorableHandle NavigatorHandle
        {
            get
            {
                if (_navigatorHandle == null)
                {
                    _navigatorHandle = new AnchorableHandle()
                    {
                        Title = language.Current.GetMessage("NAVIGATOR"),
                        FloaterHeight = 300,
                        FloaterWidth = 240,
                        FloaterLeft = 500,
                        FloaterTop = 200,
                        IsPaneVisible = true
                    };
                }
                return this._navigatorHandle;
            }
        }


        public DelegateCommand wordClickCommand { get; set; }
        public DelegateCommand excelClickCommand { get; set; }

        public DelegateCommand verticalClickCommand { get; set; }
        public DelegateCommand horizontalClickCommand { get; set; }

        public DelegateCommand<SystemBase> CloseTabCommand { get; set; }
        public DelegateCommand SystemTabSelectionChangedCommand { get; set; }
        public ObservableCollection<SystemBase> Systems { get; set; }
        public DelegateCommand OnpdfClickCommand { get; set; }
        public DelegateCommand lnkBtnFind0Length { get; set; }
        private IEventAggregator _eventAggregator;
        AutoPiping AutoPipingObj = new AutoPiping();
        PipingValidation pipingValidation = new PipingValidation();
        public DelegateCommand ZoomInCanvasClickCommand { get; set; }
        public DelegateCommand ZoomOutCanvasClickCommand { get; set; }
        public DelegateCommand CenterStageCanvasClickCommand { get; set; }
        public DelegateCommand ToolBarGridLineClickCommand { get; set; }
        public DelegateCommand DuplicateClickCommand { get; set; }

        public WL.AddFlow addFlowControllerWiring;
        public UtilityWiring utilWiring = new UtilityWiring();
        public static List<PointF> ptStart = new List<PointF>();
        public static List<PointF> ptEnd = new List<PointF>();
        float minx = 0;
        float miny = 0;
        float maxX = 0;
        float maxY = 0;

        #region Fields

        public static string IsDisable;
        private IGlobalProperties _globalProperties;
        private JCHVRF.Model.Project _project;
        // private JCHVRF.Model.NextGen.SystemVRF CurrentSystem;
        JCHVRF.Model.NextGen.SystemVRF curSystemItem = new JCHVRF.Model.NextGen.SystemVRF();
        IRegionManager regionManager;
        private IProjectInfoBAL _projectBAL;
        public readonly string projectPath = "C:\\JCH_VSTS";
        NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
        NextGenBLL.PipingBLL pipBll = null;
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;
        string ut_weight;
        public static bool blnCallfromDelete = false;
        // public bool blnCallfromDelete = false;
        private Lassalle.Flow.AddFlow addFlowPiping;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        internal  static bool _isSysProp;
        internal static bool _isSysPropFA;
        newUtilTrace.RegUndoEnt regEnt = UndoRedoSetup.Instance.RegEnt;
        public newUtilTrace.UndoEnableEnt UndoEnable = UndoRedoSetup.Instance.UndoEnable;

        private SystemBase _hvacSystem;
        public static string checkValDuplicate;
        #endregion

        #region Constructors

        public MasterDesignerViewModel(IRegionManager regionManager, IProjectInfoBAL projctInfoBll, Prism.Events.IEventAggregator eventAggregator, IGlobalProperties globalProperties, IModalWindowService winService)
        {
            try
            {
                this.regionManager = regionManager;
                _eventAggregator = eventAggregator;
                _globalProperties = globalProperties;
                _winService = winService;
                _projectBAL = projctInfoBll;

                LoadedCommand = new DelegateCommand(OnMasterDesignerLoad);
                wordClickCommand = new DelegateCommand(OnWordClickedCommand);
                excelClickCommand = new DelegateCommand(OnExcelClickedCommand);
                verticalClickCommand = new DelegateCommand(OnVerticalClickCommand);
                horizontalClickCommand = new DelegateCommand(OnHorizontalClickCommand);
                NewClickCommand = new DelegateCommand(OnNewProjectClicked);
                UnloadedCommand = new DelegateCommand(Cleanup);
                CloseClickCommand = new DelegateCommand(OnCloseProjectClicked);
                OpenRecentClickCommand = new DelegateCommand<string>(OnOpenRecentClicked);
                SettingsClickCommand = new DelegateCommand(OnNewSettingsClicked);
                CloseTabCommand = new DelegateCommand<SystemBase>(OnTabClose);
                SaveClickCommand = new DelegateCommand(OnSaveClicked);
                SaveAsClickCommand = new DelegateCommand(OnSaveAsClicked);
                OnpdfClickCommand = new DelegateCommand(OnpdfClickedCommand);
                DuplicateClickCommand = new DelegateCommand(OnDuplicateClickCommand);
                SystemTabSelectionChangedCommand = new DelegateCommand(OnTabSelected);
                EditReportContent = new DelegateCommand(OnEditReportContentClicked);
                Wiring = new DelegateCommand(OnWiringClicked);
                Piping = new DelegateCommand(OnPipingClicked);
                OpenProjectClickCommand = new DelegateCommand(OpenProjectClicked);
                NormalViewCommand = new DelegateCommand(OnNormalViewClick);
                FullScreenCommand = new DelegateCommand(OnFullScreenViewClick);
                ZoomInCanvasClickCommand = new DelegateCommand(OnZoomInCanvasClick);
                ZoomOutCanvasClickCommand = new DelegateCommand(OnZoomOutCanvasClick);
                CenterStageCanvasClickCommand = new DelegateCommand(OnCenterStageCanvasClick);
                ToolBarGridLineClickCommand = new DelegateCommand(OnToolBarGridLineEnableClick);
                DeleteSystemCommand = new DelegateCommand(OnDeleteSystemClicked);
                PipingLengthClick = new DelegateCommand(OnPipingLengthClick);
                UndoClickCommand = new DelegateCommand(OnUndoClick);
                RedoClickCommand = new DelegateCommand(OnRedoClick);
                CoolingSystemCommand = new DelegateCommand(OnCoolingSystemClicked);
                CancelCoolingSystemCommand = new DelegateCommand(OnCancelCoolingSystemClicked);
                LayoutViewChangedCommand = new DelegateCommand<string>(OnLayoutViewChanged);
                SubscribeToEvents();

                //system menu submenu commands
                NewSystemCommand = new DelegateCommand(OnNewSystemClicked);


                //ACC - RAG
                PropertyCommand = new DelegateCommand(OnPropertyClicked);

                //RecentProjectFilesData.Add("1");
                //RecentProjectFilesData.Add("2");
                //RecentProjectFilesData.Add("3");

                //System Specific Menu Visibility Bindings
                VRFMenuVisibility = Visibility.Collapsed;
                ConrollerMenuVisibility = Visibility.Collapsed;
                HeatExhangerMenuVisibility = Visibility.Collapsed;

                Systems = new ObservableCollection<SystemBase>();
                GetRecentProjectFilesData();
                setupUndoRedo();
                _eventAggregator.GetEvent<SystemDetailsSubscriber>().Subscribe(loadSystem);                
                //_eventAggregator.GetEvent<SystemDetailsAccessoriesSubscriber>().Publish();

            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnLayoutViewChanged(string obj)
        {
            string ErrMsg = string.Empty;
            bool IsValidDraw = Utility.Validation.IsValidatedSystemVRF(JCHVRF.Model.Project.GetProjectInstance, CurrentSystem, out ErrMsg);
            if (!IsValidDraw)
            {
                JCHMessageBox.Show(string.Format(ErrMsg));
                ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, ErrMsg);
                return;
            }
            var messageBoxResult = JCHMessageBox.Show(language.Current.GetMessage("PIPING_LAYOUT_CHANGE_CONFIRM"), MessageType.Warning, MessageBoxButton.OKCancel);
            if (messageBoxResult == MessageBoxResult.Cancel)
                return;
            if (SelectedTabIndex >= 0)
            {
                var Sys = (JCHVRF.Model.NextGen.SystemVRF) Systems[SelectedTabIndex];
                if (Sys != null)
                {
                    switch (obj)
                    {
                        case "Normal":
                            Sys.PipingLayoutType = PipingLayoutTypes.Normal;
                            break;
                        case "BinaryTree":
                            Sys.PipingLayoutType = PipingLayoutTypes.BinaryTree;
                            break;
                        case "SchemaA":
                            Sys.PipingLayoutType = PipingLayoutTypes.SchemaA;
                            break;
                    }
                    _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(Sys);
                    _eventAggregator.GetEvent<SystemTabSubscriber>().Publish(Sys);
                    if(Sys.PipingLayoutType != PipingLayoutTypes.SchemaA)
                      _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(Sys);
                }

            }
        }

        private void OnMasterDesignerLoad()
        {
            Logger.LogProjectInfo("OnMasterDesignerLoad -> Master Designer Loading Started");
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            regionManager.Regions[RegionNames.MasterDesignerPropertiesRegion].NavigationService.Navigated += (o, e) =>
            {
                PropertiesHandle.IsPaneVisible = true;
                PropertiesHandle.IsPaneSelected = true;
                switch (e.Uri.OriginalString)
                {
                    case ViewKeys.CanvasProperties:
                        PropertiesHandle.FloaterHeight = 370;
                        PropertiesHandle.FloaterWidth = 320;
                        PropertiesHandle.Title = language.Current.GetMessage("CANVAS") +" " + language.Current.GetMessage("PROPERTIES");
                        break;
                    case ViewKeys.IDUProperties:
                        PropertiesHandle.FloaterHeight = 700;
                        PropertiesHandle.FloaterWidth = 700;
                        PropertiesHandle.Title = language.Current.GetMessage("IDU") + " " + language.Current.GetMessage("PROPERTIES");
                        break;
                    case ViewKeys.ODUProperties:
                        PropertiesHandle.FloaterHeight = 550;
                        PropertiesHandle.FloaterWidth = 700;
                        PropertiesHandle.Title = language.Current.GetMessage("ODU") + " " + language.Current.GetMessage("PROPERTIES");
                        break;
                    case ViewKeys.CHBoxProperties:
                        PropertiesHandle.FloaterHeight = 300;
                        PropertiesHandle.FloaterWidth = 250;
                        PropertiesHandle.Title = language.Current.GetMessage("CHBOX") + " " + language.Current.GetMessage("PROPERTIES");
                        break;
                }
            };

            regionManager.RequestNavigate(RegionNames.MasterDesignerSystemDetailsRegion, "SystemDetails");
            regionManager.RequestNavigate(RegionNames.MasterDesignerPropertiesRegion, ViewKeys.CanvasProperties);

            if (_currentSystem != null)
            {
                setSystemSpecificMenuVisibility(_currentSystem);
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            Logger.LogProjectInfo("OnMasterDesignerLoad -> Master Designer Loading Completed");
        }

        private void clearDeletedSystems(JCHVRF.Model.Project Project)
        {
            List<string> idsToBeRemoved = new List<string>();
            foreach (var item in Systems)
            {
                var result = Project.SystemListNextGen.Where(system => system.Id == item.Id);
                if(result == null || result.Count() < 1)
                {
                    idsToBeRemoved.Add(item.Id);
                }
            }
            foreach (var item in idsToBeRemoved)
            {
                Systems.Remove(Systems.FirstOrDefault(sys => sys.Id == item));
            }
            //if(Systems.Count == proj)
            //foreach (var item in Project.SystemListNextGen)
            //{
            //    Systems.Where(sys=> sys.Id )
            //}
        }

        private void setupUndoRedo()
        {
            if (!UndoRedoSetup.Instance.IsInitialized)
            {
                //newUtilTrace.UtilTrace.EnableUndo = delegate ()
                //{
                //    IsUndoEnabled = !regEnt.enable.revoke;
                //};

                newUtilTrace.UtilTrace.ReLoadMain = delegate ()
                {
                    Project = Project.GetProjectInstance;
                    OnMasterDesignerLoad();
                    //if (CurrentSystem.Id == Project.SelectedSystemID)
                    //{
                    Project.GetProjectInstance.IsPerformingUndoRedo = true;
                    //clearDeletedSystems(Project);

                    _eventAggregator.GetEvent<RefreshCanvas>().Publish();
                    _eventAggregator.GetEvent<RefreshSystems>().Publish();

                    Project.GetProjectInstance.IsPerformingUndoRedo = false;
                    //}
                    //else
                    //{
                    //    var lastSystem = Project.SystemListNextGen.FirstOrDefault(list => list.Id == Project.SelectedSystemID);
                    //    if (lastSystem != null)
                    //    {
                    //        onSystemSelected(lastSystem);
                    //    }
                    //}
                    //NavigationParameters param = new NavigationParameters();
                    //param.Add("Project", Project);
                    //param.Add("MenuIndex", SelectedRibbonTabIndex);
                    //regionManager.RequestNavigate(RegionNames.ContentRegion, "Null");
                    //regionManager.RequestNavigate(RegionNames.ContentRegion, "MasterDesigner", param);
                };
                UndoRedoSetup.Instance.IsInitialized = true;

            }
            newUtilTrace.UtilTrace.EnableUndo = delegate ()
            {
                IsUndoEnabled = !regEnt.enable.revoke;
                IsRedoEnabled = !regEnt.enable.back;
            };
            IsUndoEnabled = !regEnt.enable.revoke;

            newUtilTrace.UtilTrace.EnableRedo = delegate ()
            {
                IsRedoEnabled = !regEnt.enable.back;
                IsUndoEnabled = !regEnt.enable.revoke;
            };
            IsRedoEnabled = !regEnt.enable.back;
            //if (Project.GetProjectInstance.SystemListNextGen != null && Project.GetProjectInstance.SystemListNextGen.Count > 1)
            //{
            //    UndoRedoSetup.SetInstanceNull();
            //}
            UndoRedoSetup.ResetUndoRedo += delegate ()
            {
                IsRedoEnabled = IsUndoEnabled = false;
            };
        }

        public void OnUndoClick()
        {
            try
            {
                if (regEnt.enable.revoke == false)
                {
                    blnCallfromDelete = true;
                    regEnt.undo();
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        public void OnRedoClick()
        {

            try
            {
                if (regEnt.enable.back == false)
                {
                    blnCallfromDelete = true;
                    regEnt.redo();
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnHorizontalClickCommand()
        {
            try
            {
                var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
                var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                CurrentSystems = Lists;
                NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)CurrentSystems;
                WL.AddFlow addFlowItemPiping = CurrentSystems.MyPipingNodeOut.AddFlow;
                if (sysItem != null && !isBinding && sysItem.MyPipingNodeOut != null)
                {
                    bool isVertical = false;
                    sysItem.IsPipingVertical = isVertical;
                    DoDrawPipingAllignment(sysItem.IsPipingVertical, sysItem, ref addFlowItemPiping);
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnVerticalClickCommand()
        {
            try
            {
                var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
                var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                CurrentSystems = Lists;
                NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)CurrentSystems;
                WL.AddFlow addFlowItemPiping = CurrentSystems.MyPipingNodeOut.AddFlow;
                if (sysItem != null && !isBinding && sysItem.MyPipingNodeOut != null)
                {
                    bool isVertical = true;
                    sysItem.IsPipingVertical = isVertical;

                    DoDrawPipingAllignment(sysItem.IsPipingVertical, sysItem, ref addFlowItemPiping);
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }

        }

        private void SubscribeToEvents()
        {            
            _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Subscribe(onSystemSelected);
            _eventAggregator.GetEvent<RefreshSystems>().Subscribe(RefreshSystemsList);
            _eventAggregator.GetEvent<FloatProperties>().Subscribe(FloatPropertiesTab);
        }

        private void FloatPropertiesTab()
        {
            PropertiesHandle.IsPaneFloating = false;
            PropertiesHandle.IsPaneFloating = true;
        }

        private void RefreshSystemsList()
        {
            if (Project.GetProjectInstance?.SystemListNextGen != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.SystemListNextGen)
                {
                    if (Systems.FirstOrDefault(a => a.Id == system.Id) != null)
                    {
                        Systems.FirstOrDefault(a => a.Id == system.Id).Name = system.Name;
                        Systems.FirstOrDefault(a => a.Id == system.Id).StatusIcon = system.StatusIcon;
                    }
                }
            }
            if (Project.GetProjectInstance?.HeatExchangerSystems != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.HeatExchangerSystems)
                {
                    if (Systems.FirstOrDefault(a => a.Id == system.Id) != null)
                    {
                        Systems.FirstOrDefault(a => a.Id == system.Id).Name = system.Name;
                        Systems.FirstOrDefault(a => a.Id == system.Id).StatusIcon = system.StatusIcon;
                    }
                }
            }

            if (Project.GetProjectInstance?.ControlSystemList != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.ControlSystemList)
                {
                    if (Systems.FirstOrDefault(a => a.Id == system.Id) != null)
                    {
                        Systems.FirstOrDefault(a => a.Id == system.Id).Name = system.Name;
                        Systems.FirstOrDefault(a => a.Id == system.Id).StatusIcon = system.StatusIcon;
                    }
                }
            }
        }

        //private void SystemDelete(bool bCallfromDelete)
        //{
        //    blnCallfromDelete = bCallfromDelete;
        //    if(bCallfromDelete==true)
        //    DeleteSystem();
        //    _eventAggregator.GetEvent<CanvasEqupmentDeleteSubscriber>().Unsubscribe(SystemDelete);
        //}

        private void Cleanup()
        {
            _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Unsubscribe(onSystemSelected);
            _eventAggregator.GetEvent<FloatProperties>().Unsubscribe(FloatPropertiesTab);            
            //_eventAggregator.GetEvent<SystemTypeCanvasSubscriber>().Unsubscribe(OnCanvasClickSystemSelected);
            //_eventAggregator.GetEvent<CentralControllerCanvasMouseDownClickSubscriber>().Unsubscribe(OnCentralControllerCanvasClickSystemSelected);
            //_eventAggregator.GetEvent<HeatExchangerCanvasMouseDownClickSubscriber>().Unsubscribe(OnHeatExchangerCanvasClickSystemSelected);

            if (!blnCallfromDelete)
            {

                MessageBoxResult messageBoxResult = JCHMessageBox.Show(language.Current.GetMessage("ALERT_CHANGES_IN_PROJECT"), MessageType.Warning, MessageBoxButton.YesNo);

                //MessageBoxResult messageBoxResult = MessageBox.Show("Your changes in the project will be lost. To save, click Yes. To quit, click No. ", "Closing Project - " + Project.Name, MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    OnSaveClicked();
                }
                blnCallfromDelete = false;
                _eventAggregator.GetEvent<Cleanup>().Publish();
                _eventAggregator.GetEvent<CleanupHE>().Publish();
                _eventAggregator.GetEvent<CleanupOnClose>().Publish();
                _eventAggregator.GetEvent<CleanUpCanvaspropertyTab>().Publish();

            }

        }


        private void OnTabSelected()
        {
            try
            {
                if (SelectedTabIndex != -1)
                {
                    var system = Systems[SelectedTabIndex];
                    setSystemSpecificMenuVisibility(system);
                    _eventAggregator.GetEvent<SystemSelectedTabSubscriber>().Publish(system);

                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnTabClose(SystemBase systemBase)
        {
            try
            {
                int index = Systems.IndexOf(systemBase);

                if (systemBase.UnSavedDrawing != null)
                {
                    systemBase.UnSavedDrawing.Clear();
                }

                if (Systems.Count <= 1)
                {
                    MessageBoxResult messageBoxResult = JCHMessageBox.Show(language.Current.GetMessage("WARNING_SAVE_PROJECT"), MessageType.Warning, MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                        OnSaveClicked();
                }
                Systems.Remove(systemBase);
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }
        protected override void RaiseIsActiveChanged()
        {
            base.RaiseIsActiveChanged();
            if (!IsActive)
            {
                regionManager.Regions.Remove(RegionNames.MasterDesignerPropertiesRegion);
                regionManager.Regions.Remove(RegionNames.MasterDesignerSystemDetailsRegion);
                _globalProperties.ProjectTitle = "";
            }
        }


        #endregion

        #region Properties

        private int _selectedRibbonTabIndex;

        public int SelectedRibbonTabIndex
        {
            get { return _selectedRibbonTabIndex; }
            set { this.SetValue(ref _selectedRibbonTabIndex, value); }
        }


        private bool _IsFileMenuSelected;
        public bool IsFileMenuSelected
        {
            get { return _IsFileMenuSelected; }
            set
            {
                SetValue(ref _IsFileMenuSelected, value);
            }
        }

        private bool _IsUndoEnabled;
        public bool IsUndoEnabled
        {
            get { return _IsUndoEnabled; }
            set
            {
                SetValue(ref _IsUndoEnabled, value);
            }
        }

        private bool _IsRedoEnabled;
        public bool IsRedoEnabled
        {
            get { return _IsRedoEnabled; }
            set
            {
                SetValue(ref _IsRedoEnabled, value);
            }
        }

        private Visibility _contollerMenuVisibility;

        public Visibility ConrollerMenuVisibility
        {
            get { return _contollerMenuVisibility; }
            set
            {
                SetValue(ref _contollerMenuVisibility, value);
            }
        }

        private Visibility _vrfMenuVisibility;

        public Visibility VRFMenuVisibility
        {
            get { return _vrfMenuVisibility; }
            set
            {
                SetValue(ref _vrfMenuVisibility, value);
            }
        }

        private Visibility _wiringExport;

        public Visibility WiringExport
        {
            get { return _wiringExport; }
            set
            {
                SetValue(ref _wiringExport, value);
            }
        }
        private Visibility _pipingExport;

        public Visibility PipingExport
        {
            get { return _pipingExport; }
            set
            {
                SetValue(ref _pipingExport, value);
            }
        }
        private Visibility _heatExchangerMenuVisibility;

        public Visibility HeatExhangerMenuVisibility
        {
            get { return _heatExchangerMenuVisibility; }
            set
            {
                SetValue(ref _heatExchangerMenuVisibility, value);
            }
        }

        private bool _vrfMenuTabSelected;

        public bool VRFMenuTabSelected
        {
            get { return _vrfMenuTabSelected; }
            set
            {
                SetValue(ref _vrfMenuTabSelected, value);
            }
        }

        private bool _controllerMenuTabSelected;

        public bool ControllerMenuTabSelected
        {
            get { return _controllerMenuTabSelected; }
            set
            {
                SetValue(ref _controllerMenuTabSelected, value);
            }
        }





        private bool _heatExchangerMenuTabSelected;

        public bool HeatExchangerMenuTabSelected
        {
            get { return _heatExchangerMenuTabSelected; }
            set
            {
                SetValue(ref _heatExchangerMenuTabSelected, value);
            }
        }

        /// <summary>
        /// Gets or sets the LoadedCommand
        /// </summary>
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand UnloadedCommand { get; set; }
        /// <summary>
        /// Gets or sets the Project
        /// </summary>
        public JCHVRF.Model.Project Project
        {
            get { return _project; }
            set { this.SetValue(ref _project, value); }
        }

        public DelegateCommand CloseClickCommand { get; set; }

        public DelegateCommand NewClickCommand { get; set; }
        public DelegateCommand EditReportContents { get; set; }
        public DelegateCommand ExportToExcel { get; set; }

        public DelegateCommand NewSystemCommand { get; set; }

        //ACC - RAG
        //System --> Property Click
        public DelegateCommand PropertyCommand { get; set; }

        public DelegateCommand<string> OpenRecentClickCommand { get; set; }
        public DelegateCommand SettingsClickCommand { get; set; }

        public DelegateCommand SaveClickCommand { get; set; }
        public DelegateCommand SaveAsClickCommand { get; set; }
        public DelegateCommand OpenProjectClickCommand { get; set; }
        public string recentProjectPaths { get; set; }

        private ObservableCollection<string> _recentProjectFiles;

        public DelegateCommand EditReportContent { get; set; }
        public DelegateCommand Piping { get; set; }
        public DelegateCommand Wiring { get; set; }
        public DelegateCommand PipingLengthClick { get; set; }
        public DelegateCommand RedoClickCommand { get; set; }
        public DelegateCommand UndoClickCommand { get; set; }


        public ObservableCollection<string> RecentProjectFilesData
        {
            get
            {
                if (_recentProjectFiles == null)
                    _recentProjectFiles = new ObservableCollection<string>();
                return _recentProjectFiles;
            }
        }

        private int selectedIndex;

        public int SelectedTabIndex
        {
            get { return selectedIndex; }
            set
            {
                this.SetValue(ref selectedIndex, value);
                OnTabSelected();
            }
        }
        #region Zoom and CenterStage
        private void OnZoomInCanvasClick()
        {
            _eventAggregator.GetEvent<ToolBarZoomInSubscriber>().Publish();
        }
        private void OnZoomOutCanvasClick()
        {
            _eventAggregator.GetEvent<ToolBarZoomOutSubscriber>().Publish();
        }
        private void OnCenterStageCanvasClick()
        {
            _eventAggregator.GetEvent<CanvasCenterStageEnableChangeSubscriber>().Publish(true);
        }

        private void OnToolBarGridLineEnableClick()
        {
            _eventAggregator.GetEvent<ToolBarGridLineEnableChangeScubscriber>().Publish();
        }
        #endregion

        #region FullScreen and Normal Screen Selection
        public DelegateCommand NormalViewCommand { get; set; }
        public DelegateCommand FullScreenCommand { get; set; }
        private void OnNormalViewClick()
        {
            Anchorables.ForEach(a => {
                if (a.IsPaneVisible && a.IsPaneFloating)
                {
                    a.IsPaneFloating = false;
                }
            });
        }
        private void OnFullScreenViewClick()
        {
            try
            {
                int i = 0;
                Anchorables.ForEach(a => {
                    if (a.IsPaneVisible && !a.IsPaneFloatingMinimized)
                    {
                        a.FloaterLeft = 1100;
                        a.FloaterTop = 300 + (i * 50);
                        a.IsPaneFloatingMinimized = false;
                        a.IsPaneFloatingMinimized = true;
                        i++;
                    }
                });
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        #endregion
        #region Tool Bar Selection
        bool isBinding = false;
        #endregion
        public DelegateCommand DeleteSystemCommand { get; set; }
        public DelegateCommand CoolingSystemCommand { get; set; }
        public DelegateCommand CancelCoolingSystemCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The OnNavigatedTo
        /// </summary>
        /// <param name="navigationContext">The navigationContext<see cref="NavigationContext"/></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                base.OnNavigatedTo(navigationContext);
                if (navigationContext.Parameters.ContainsKey("Project"))
                {
                    Project = navigationContext.Parameters["Project"] as JCHVRF.Model.Project;
                }
                if (navigationContext.Parameters.ContainsKey("MenuIndex"))
                {
                    SelectedRibbonTabIndex = Convert.ToInt32(navigationContext.Parameters["MenuIndex"]);
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        /// <summary>
        /// The OnNewProjectClicked
        /// </summary>

        private void OnNewSettingsClicked()
        {
            try
            {
                _winService.ShowView(ViewKeys.ProjectSettingsView, language.Current.GetMessage("PROJECT_SETTINGS"), null, true,1080,700);
                //var projectSettingsView = new Views.ProjectSettingsView();
                //projectSettingsView.ShowDialog();
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        //ACC - RAG START
        private void OnPropertyClicked()
        {
            try
            {
                HeatExchangerCanvasEquipmentViewModel.flgTabChanged = false;
                if (Systems.Count > 0)
                {
                    _isSysProp = true;
                    _isSysPropFA = true;
                    Project.isSystemPropertyWiz = true;
                    JCHVRF.Model.Project.CurrentSystemId = Systems[SelectedTabIndex].Id;
                    var SystemPropWiz = new Views.SystemPropertyWizard();
                    SystemPropWiz.HvacSystem = Systems[SelectedTabIndex];
                 
                    var obj=  Application.Current.Windows.OfType<Window>().First(x => x.IsActive);
                    SystemPropWiz.Owner =obj;
          
                    SystemPropWiz.ShowDialog();
                  
                  
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }
     
        //ACC - RAG END

        private void OnNewProjectClicked()
        {
            try
            {
                Project.isSystemPropertyWiz = false;
                if (JCHMessageBox.Show(language.Current.GetMessage("CONFIRM_NEW_PROJECT"), MessageType.Information, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    OnSaveNewProjectClicked();
                    regionManager.RequestNavigate(RegionNames.ContentRegion, "Home");
                }
                else
                {
                    return;
                }

                //IsDisable = "true";//have to modify that condition design condition control disabled for.
                JCHVRF.Model.Project.CurrentProject = new JCHVRF.Model.Project();
                JCHVRF.Model.Project.CurrentProject.RegionCode = JCHVRF.Model.SystemSetting.UserSetting.locationSetting.region;
                JCHVRF.Model.Project.CurrentProject.SubRegionCode = JCHVRF.Model.SystemSetting.UserSetting.locationSetting.subRegion;

                //var createProjectWizard = new Views.CreateProjectWizard();
                //createProjectWizard.ShowDialog();
                _winService.ShowView(ViewKeys.CreateProjectWizard, language.Current.GetMessage("NEWPROJECT_CREATE"), null, true, 1080, 700);
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }

        }


        private void OnNewSystemClicked()
        {
            try
            {
                Project.isSystemPropertyWiz = false;
                //JCHMessageBox.Show("Would you like to save changes in the current project?", "Alert", MessageBoxButton.OKCancel);
                //if (MessageBox.Result == DialogResult.Ok)
                //    //we have to use the Save functionlity to save the current project details in db. Development in-progress;
                //else if (MessageBox.Result == DialogResult.Cancel)
                //    close the current project on that tab;
                JCHVRF.Model.Project.CurrentSystemId = null;
                var createSystemWizard = new Views.CreateSystemWizard();
                createSystemWizard.ShowDialog();
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }
        private void OnEditReportContentClicked()
        {
            try
            {
                _winService.ShowView(ViewKeys.EditReportContents, language.Current.GetMessage("SELECT_REPORT_CONTENT"));
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }
        private void OnWiringClicked()
        {
            int includedtotalSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.Count();
            int isPipingOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.IsPipingOK == true).Count();
            int isWiringOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.MyWiringNodeOut == null).Count();
            bool isAddflowexists = false;
            foreach (var item in JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen)
            {
                if (item.MyWiringNodeOut != null)
                {
                    if (item.MyWiringNodeOut.AddFlow == null)
                    {
                        isAddflowexists = true;
                    }
                }
            }
            if (includedtotalSystems == isPipingOkSystems)
            {
                if (isAddflowexists == false && isWiringOkSystems <= 0)
                {
                    try
                    {
                        isWiring = true;
                        Wiring.IsActive = false;
                        var projs = JCHVRF.Model.Project.CurrentProject.SystemListNextGen;
                        for (var i = 0; i < projs.Count(); i++)
                        {
                            // projs[i].IsExportToReport = true;
                            //Lassalle.WPF.Flow.AddFlow tempa = new Lassalle.WPF.Flow.AddFlow();
                            //ucDesignerCanvas obj = new ucDesignerCanvas();

                            //SystemTab objsys = new SystemTab();
                            isWiring = true;
                            Wiring.IsActive = false;
                            string outputDir = "";
                            JCBase.Util.FileUtil util = new JCBase.Util.FileUtil();
                            string name = projs[i].Id + "_wiring.jpeg";
                            string dir = null;
                            if (string.IsNullOrEmpty(dir))
                            {
                                dir = MyConfig.ProjectFileDirectory;
                            }
                            string filePath_picwiring = util.GetFullPathName(dir, name);
                            ExportVictorGraph_wiring(filePath_picwiring, true, projs[i].Id, (NextGenModel.SystemVRF)projs[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        int? id = Project?.projectID;
                        Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
                    }
                }
                else
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_DO_WIRING_FOR_ALL_SYSTEMS"));
                }
            }
        }
        private JCHVRF.Model.NextGen.SystemVRF CurrentSystem;
        private JCHVRF.Model.NextGen.SystemVRF CurrentSystems;
        private JCHVRF.Model.NextGen.SystemVRF CurrentSystemWiring;

        private SystemBase _currentSystem;
        private void loadSystem(JCHVRF.Model.SystemBase obj)
        {
            try
            {
                //CurrentSystem = null;
                CurrentSystem = (JCHVRF.Model.NextGen.SystemVRF)obj;
                CurrentSystemWiring = (JCHVRF.Model.NextGen.SystemVRF)obj;
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        public bool ExportVictorGraph(string filePath, bool isBackWhite = false, string Id = null, JCHVRF.Model.NextGen.SystemVRF systems = null)
        {
            try
            {
                if (CurrentSystem != null)
                {
                    if (CurrentSystem.IsPipingOK == true)
                    {
                        if (isWordOrPDF == true)
                        {
                            NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)systems;
                            WL.AddFlow addFlowItemPiping = systems.MyPipingNodeOut.AddFlow;
                            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(900, 900);
                            System.Drawing.Graphics gs = System.Drawing.Graphics.FromImage(bmp);
                            System.Drawing.Imaging.Metafile mf = new System.Drawing.Imaging.Metafile(filePath, gs.GetHdc());
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(mf);
                            string PipingImageDir = GetImagePathPiping();
                            DrawEMF_PipingNextGen(g, addFlowItemPiping, sysItem, PipingImageDir, this.Project);
                            g.ResetTransform();
                            g.Save();
                            g.Flush();
                            g.Dispose();
                            gs.Dispose();
                            mf.Dispose();
                            if (isPiping == true)
                            {
                                var proj = JCHVRF.Model.Project.GetProjectInstance;
                                JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                                if (!rpt.ExportReportPipingJPEG(filePath))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
                            //var IDUList = (JCHVRF.Model.NextGen.SystemVRF)JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                            var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                            CurrentSystems = Lists;

                            NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)systems;
                            WL.AddFlow addFlowItemPiping = systems.MyPipingNodeOut.AddFlow;
                            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(900, 900);
                            System.Drawing.Graphics gs = System.Drawing.Graphics.FromImage(bmp);
                            System.Drawing.Imaging.Metafile mf = new System.Drawing.Imaging.Metafile(filePath, gs.GetHdc());
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(mf);
                            string PipingImageDir = GetImagePathPiping();
                            DrawEMF_PipingNextGen(g, addFlowItemPiping, sysItem, PipingImageDir, this.Project);
                            g.ResetTransform();
                            g.Save();
                            g.Flush();
                            g.Dispose();
                            gs.Dispose();
                            mf.Dispose();
                            if (isPiping == true)
                            {
                                var proj = JCHVRF.Model.Project.GetProjectInstance;
                                JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                                if (!rpt.ExportReportPipingJPEG(filePath))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //private void ResetScrollPosition()
        //{
        //    if (scrollPosition != null)
        //        this.addFlowPiping.ScrollPosition = new Point(Math.Abs(scrollPosition.X), Math.Abs(scrollPosition.Y));
        //}
        //public void BindTreeNodeOutdoor(string systemID)
        //{
        //    List<string> ERRList;
        //    List<string> MSGList;
        //    foreach()

        //}
        //public void BindTreeNodeOutdoor(string systemID, ref WL.AddFlow addFlowPiping)
        //{
        //    List<string> ERRList;
        //    List<string> MSGList;
        //    //string systemID = ri.SystemID;
        //    var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
        //    var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
        //    CurrentSystems = Lists;
        //    NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)CurrentSystems;


        //    foreach (WL.Node node in addFlowPiping.Nodes())
        //    {
        //        if (CurrentSystems.OutdoorItem == null)
        //            return;
        //        List<RoomIndoor> listRISelected = (new ProjectBLL(this.Project)).GetSelectedIndoorBySystem(systemID);
        //        //是否自动模式的判断  add by Shen Junjie on 20170619
        //        if (sysItem != null && sysItem.IsAuto)
        //        {
        //            this.Project.
        //               // 根据系统中的室内机自动计算匹配
        //               SelectOutdoorResult result;
        //            //Global.DoSelectOutdoor(sysItem, listRISelected, thisProject, out ERRList);

        //            result = JCHVRF_New.Global.DoSelectOutdoorODUFirst(sysItem, listRISelected, this.Project, out ERRList, out MSGList);
        //        }
        //        BindTreeNodeOut(node, sysItem, listRISelected, this.Project);


        //    }
        //}
        //public static void BindTreeNodeOut(WL.Node nodeOut, JCHVRF.Model.NextGen.SystemVRF sysItem, List<RoomIndoor> listRISelected, Project thisProject)
        //{

        //    nodeOut.AddFlow.Tag = sysItem;
        //    nodeOut.AddFlow.Name = sysItem.Id;
        //    nodeOut.Foreground = UtilColor.ColorOriginal;
        //    if (listRISelected == null || listRISelected.Count == 0 || sysItem.OutdoorItem == null)
        //    {
        //        nodeOut.Text = sysItem.Name;
        //        nodeOut.ForeColor = UtilColor.ColorWarning;
        //        sysItem.Ratio = 0;
        //        SetTreeNodeImage(nodeOut, 0, 0);
        //    }
        //    else
        //    {
        //        string sRatio = (sysItem.Ratio * 100).ToString("n0") + "%";
        //        nodeOut.Text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "] - " + sRatio;

        //        if (sysItem.SysType == SystemType.OnlyIndoor)
        //        {
        //            if (sysItem.Ratio < 0.5 || sysItem.Ratio > sysItem.MaxRatio)
        //                Global.SetTreeNodeImage(nodeOut, 0, 0);
        //            else
        //                Global.SetTreeNodeImage(nodeOut, 1, 3);
        //        }
        //        else if (sysItem.SysType == SystemType.OnlyFreshAirMulti || sysItem.SysType == SystemType.CompositeMode)
        //        {
        //            //if (sysItem.Ratio < 0.8 || sysItem.Ratio > 1.05)
        //            if (sysItem.Ratio < 0.8 || sysItem.Ratio > 1) //纯新风和混连比例范围80%~100% 20160819 by Yunxiao Lin
        //                Global.SetTreeNodeImage(nodeOut, 0, 0);
        //            else
        //                Global.SetTreeNodeImage(nodeOut, 1, 3);
        //            //混连时，新风机容量不能超过所有室内机容量的30% 20160819 by Yunxiao Lin
        //            if (sysItem.SysType == SystemType.CompositeMode)
        //            {
        //                if (sysItem.RatioFA > 0.3)
        //                    Global.SetTreeNodeImage(nodeOut, 0, 0);
        //                else
        //                    Global.SetTreeNodeImage(nodeOut, 1, 3);
        //            }
        //        }
        //        else
        //        {
        //            //if (sysItem.Ratio == 1) //updated on 20151130 clh,不是必须的限制条件
        //            Global.SetTreeNodeImage(nodeOut, 1, 3); // 一对一通过！
        //        }
        //        //如果室外机有更新，必须重新验证Piping add on 20160819 by Yunxiao Lin
        //        if (sysItem.IsUpdated)
        //            sysItem.IsPipingOK = false;

        //        if (sysItem.IsPipingOK)
        //            nodeOut.ForeColor = UtilColor.ColorOriginal;
        //        else
        //            nodeOut.ForeColor = UtilColor.ColorWarning;
        //    }

        //    nodeOut.Nodes.Clear();
        //    //RoomLoadIndexBLL roomBill = new RoomLoadIndexBLL();
        //    foreach (RoomIndoor ri in listRISelected)
        //    {
        //        TreeNode nodeIn = new TreeNode();
        //        nodeIn.Tag = ri;
        //        nodeIn.Name = ri.IndoorNO.ToString();
        //        //string floorRoomName = roomBill.getFloorRoomName(ri,thisProject);               
        //        if (thisProject.BrandCode == "Y")
        //            nodeIn.Text = ri.IndoorName + "[" + ri.IndoorItem.Model_York + "]";
        //        else
        //            nodeIn.Text = ri.IndoorName + "[" + ri.IndoorItem.Model_Hitachi + "]";
        //        nodeOut.Nodes.Add(nodeIn);
        //        Global.SetTreeNodeImage(nodeIn, 2, 4);
        //    }
        //    nodeOut.Expand();
        //}
        private string GetImagePathPiping()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\TypeImages\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public void DrawEMF_PipingNextGen(drawing.Graphics g, WL.AddFlow addFlowPiping, NextGenModel.SystemVRF curSystemItem, string PipingImageDir, JCHVRF.Model.Project thisProject)
        {
            try
            {
                NextGenBLL.PipingBLL pipBll = new NextGenBLL.PipingBLL(thisProject);
                bool isHitachi = thisProject.BrandCode == "H";
                bool isInch = CommonBLL.IsDimension_inch();
                NextGenBLL.UtilEMF utilEMF = new NextGenBLL.UtilEMF();
                utilEMF.utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;//设置长度单位 add by axj 20161229
                utilEMF.isManualLength = curSystemItem.IsInputLengthManually;//设置是否输入管长 add by axj 20161229
                string title = language.Current.GetMessage("PIPING_ADDITIONAL_REFRIGERANT_CHARGE");//添加追加冷媒title add by axj 20161229
                var nodes = addFlowPiping.Items.OfType<WL.Node>().ToArray();

                var links = addFlowPiping.Items.OfType<WL.Link>().ToArray();
                foreach (var item in nodes)
                {
                    if (item is NextGenModel.MyNodeYP)
                    {
                        if (!(item as NextGenModel.MyNodeYP).IsCP)
                        {
                            //utilEMF.DrawYPNextGen(g, item, false);
                            NextGenModel.MyNodeYP ndOut = item as NextGenModel.MyNodeYP;

                            string ImageName = string.Format("YBranch{0}", ndOut.YBranchOrientation == "Horizontal" ? string.Empty : ndOut.YBranchOrientation);
                            string sourceDir = GetcompleteImagepath(ImageName, "svg");
                            var svgDoc = SvgDocument.Open<SvgDocument>(sourceDir, null);
                            var bitmap = svgDoc.Draw();
                            System.Drawing.PointF pt = new System.Drawing.PointF(Convert.ToInt64(item.Location.X + 6), Convert.ToInt64(item.Location.Y + 2));
                            g.DrawImage(bitmap, pt);
                        }
                        else
                        {
                            utilEMF.DrawYPNextGen(g, item, true);
                        }
                    }
                    else if (item is NextGenModel.MyNodeOut)
                    {
                        //Draw the YP model inside the outdoor unit combination and the pipe diameter data
                        NextGenModel.MyNodeOut ndOut = item as NextGenModel.MyNodeOut;
                        ImageData data = new ImageData();
                        data.imageName = ndOut.ImageName;
                        string imagename = ndOut.Text;
                        //string outModel = curSystemItem.OutdoorItem.Model;
                        string outModel = curSystemItem.OutdoorItem.Model_York;
                        string nodeImageFile = ndOut.ImageData.imagePath;

                        if (isHitachi)
                        {
                            outModel = curSystemItem.OutdoorItem.Model_Hitachi;
                        }
                        // outModel = "RAS-14-18FSNSE";
                        utilEMF.DrawNodeNextGenPipingForReporting(g, item, nodeImageFile, data, imagename);
                    }
                    else if (item is NextGenModel.MyNodeIn)
                    {
                        NextGenModel.MyNodeIn ndIn = item as NextGenModel.MyNodeIn;
                        ImageData data = new ImageData();
                        data = ndIn.ImageData;
                        string imagename = ndIn.Text;
                        string nodeImageFile = ndIn.ImageData.imagePath;
                        utilEMF.DrawNodeNextGenPipingForReporting(g, item, nodeImageFile, data, imagename);
                    }
                    else if (item is NextGenModel.MyNodeCH)
                    {
                        NextGenModel.MyNodeCH ndCH = item as NextGenModel.MyNodeCH;
                        ImageData data = new ImageData();
                        data = ndCH.ImageData;
                        string nodeImageFile = ((System.Windows.Media.Imaging.BitmapImage)ndCH.Image).UriSource.OriginalString;
                        utilEMF.DrawNodeNextGenPiping(g, item, nodeImageFile, data);
                    }
                    else if (item is NextGenModel.MyNodeMultiCH)
                    {
                        NextGenModel.MyNodeMultiCH ndMCH = item as NextGenModel.MyNodeMultiCH;
                        ImageData data = new ImageData();
                        data = ndMCH.ImageData;
                        string imagename = ndMCH.Text;
                        string nodeImageFile = PipingImageDir + "MultiCHbox.png";
                        utilEMF.DrawNodeNextGenPiping(g, item, nodeImageFile, data, imagename);
                    }
                    else if (item is NextGenModel.MyNodeLegend)
                    {
                        //绘制图例文字
                        utilEMF.DrawText(g, item);
                    }
                    else if (item.Tooltip == title)
                    {
                        utilEMF.DrawText(g, item);
                    }
                    else if (item.Text != null && item.Text.Contains(language.Current.GetMessage("COOLING")) && item.Text.Contains(language.Current.GetMessage("HEATING")))
                    {
                        utilEMF.DrawActualCapacityText(g, item);
                    }
                    else
                    {
                        utilEMF.DrawLabelText(g, item);
                    }
                }
                foreach (var item in links)
                {
                    if (item.Org is NextGenModel.MyNodeOut)
                    {
                        WL.Link line = item;
                        List<System.Windows.Point> ptl = new List<System.Windows.Point>();
                        foreach (System.Windows.Point p in line.Points)
                        {
                            ptl.Add(p);
                        }
                        if (ptl.Count <= 1)
                            return;

                        System.Windows.Point[] pts = ptl.ToArray();
                        System.Drawing.PointF[] ptf = new PointF[ptl.Count];
                        //Pen pen = new Pen(Color.Black, 0.1f);                // To be Fix latter
                        WL.Node nd = line.Dst;
                        //if (nd != null && nd is MyNodeGround_Wiring)
                        //    pen = new Pen(Color.Yellow, 0.1f);
                        for (var i = 0; i < ptl.Count; i++)
                        {
                            if (i == 0)
                            {
                                ptf[i] = new PointF(Convert.ToInt64(ptl[i].X), Convert.ToInt64(ptl[i].Y + 15));
                            }
                            else
                            {                                
                                if(i == 2)
                                    ptf[i] = new PointF(Convert.ToInt64(ptl[i].X + 5), Convert.ToInt64(ptl[i].Y));
                                else
                                    ptf[i] = new PointF(Convert.ToInt64(ptl[i].X), Convert.ToInt64(ptl[i].Y));

                            }   
                        }


                        Pen pen;
                        try
                        {
                            System.Drawing.Color c = System.Drawing.ColorTranslator.FromHtml(Convert.ToString(item.Stroke));
                            pen = new Pen(c, 0.1f);
                        }
                        catch { pen = new Pen(Color.Black, 0.1f); }


                        g.DrawLines(pen, ptf);
                }
                    else
                    {
                    utilEMF.DrawLine(g, item);
                }
            }

                var Captions = addFlowPiping.Items.OfType<WL.Caption>().ToArray();
                foreach(var caps in Captions)
                {
                    utilEMF.DrawLabelText(g, caps);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static string GetcompleteImagepath(string ImageName, string ImageType = "png")
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Image\\TypeImages";
            string navigateToFolder = ImageName + "." + ImageType;
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        private string GetImagePathWiring()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\NodeImageWiring\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        bool isPiping = false;
        bool isWiring = false;
        bool isWordOrPDF = false;
        private void OnPipingClicked()
        {
            if (JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen != null)
            {
                NextGenBLL.PipingBLL pipBllNG = GetPipingBLLInstance();

                DoSavePipingTempNodeOutStructure();

                int includedtotalSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.Count();
                int isPipingOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.IsPipingOK == true).Count();
                if (includedtotalSystems == isPipingOkSystems)
                {
                    try
                    {
                        string outputDir = "";
                        isPiping = true;
                        var projs = JCHVRF.Model.Project.CurrentProject.SystemListNextGen;
                        for (var i = 0; i < projs.Count(); i++)
                        {
                            //  projs[i].IsExportToReport = true;
                            pipBllNG.LoadPipingNodeStructure(projs[i]);

                            Lassalle.WPF.Flow.AddFlow tempa = new Lassalle.WPF.Flow.AddFlow();
                            //ucDesignerCanvas obj = new ucDesignerCanvas();

                            //SystemTab objsys = new SystemTab();
                            //objsys.UpdatePipingNodeStructure(projs[i]);
                            AutoPipingObj.DoDrawingPiping(true, (NextGenModel.SystemVRF)projs[i], tempa);
                            pipingValidation.Validate((NextGenModel.SystemVRF)projs[i], tempa);

                            JCBase.Util.FileUtil util = new JCBase.Util.FileUtil();
                            string name = projs[i].Id + "_piping.jpeg";
                            string dir = null;
                            if (string.IsNullOrEmpty(dir))
                            {
                                dir = MyConfig.ProjectFileDirectory;
                            }
                            string filePath_pic = util.GetFullPathName(dir, name);
                            ExportVictorGraph(filePath_pic, true, projs[i].Id, (NextGenModel.SystemVRF)projs[i]);
                        }
                        isPiping = false;
                        AutoPipingObj.UpdateAllPipingNodeStructure();
                    }
                    catch (Exception ex)
                    {
                        AutoPipingObj.UpdateAllPipingNodeStructure();
                        //int? id = Project?.projectID;
                        //Logger.LogProjectError(id, ex);
                    }
                }
                else
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_VALIDATION_FOR_ALL_SYSTEMS"));
                }
            }
        }
        private void OnPipingLengthClick()
        {
            try
            {
                var PipingLengthWizard = new Views.PipingLengthSettings(_eventAggregator, CurrentSystem);
                PipingLengthWizard.ShowDialog();
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        public bool ExportVictorGraph_wiring(string filePath, bool isBackWhite = false, string Id = null, JCHVRF.Model.NextGen.SystemVRF systems = null)
        {

            if (isWordOrPDF == true)
            {
                if (systems.MyWiringNodeOut == null)
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ERROR_REPORT_WIRING"));
                    isWordOrPDF = false;
                    return false;
                }
                if (CurrentSystemWiring.IsPipingOK == true && CurrentSystemWiring.MyWiringNodeOut != null)
                {
                    if (systems.MyWiringNodeOut.AddFlow == null)
                    {
                        JCHMessageBox.Show(language.Current.GetMessage("ERROR_REPORT_WIRING"));
                        isWordOrPDF = false;
                        return false;
                    }

                    WL.AddFlow addFlowItemWiring = systems.MyWiringNodeOut.AddFlow;
                    NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)systems;

                    try
                    {
                        string imageDir = GetImagePathWiring();
                        drawing.Bitmap bmp = new drawing.Bitmap(875, 348);
                        drawing.Graphics gs = drawing.Graphics.FromImage(bmp);
                        Metafile mf = new Metafile(filePath, gs.GetHdc());
                        drawing.Graphics g = drawing.Graphics.FromImage(mf);
                        if (isBackWhite)
                        {
                            g.Clear(System.Drawing.Color.White);
                        }
                        DrawEMF_wiringNextGen(g, addFlowItemWiring, sysItem, imageDir);
                        g.Save();
                        g.Dispose();
                        gs.Dispose();
                        mf.Dispose();
                        if (isWiring == true)
                        {
                            var proj = JCHVRF.Model.Project.GetProjectInstance;
                            JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                            if (!rpt.ExportReportWiringJPEG(filePath))
                            {
                                return true;
                            }
                            isWiring = false;
                        }
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            else
            {
                var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;

                var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                CurrentSystemWiring = Lists;

                if (systems.MyWiringNodeOut == null)
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ERROR_REPORT_WIRING"));
                    isWordOrPDF = false;
                    return false;
                }
                if (systems.IsPipingOK == true && systems.MyWiringNodeOut != null)
                {
                    if (systems.MyWiringNodeOut.AddFlow == null)
                    {
                        JCHMessageBox.Show(language.Current.GetMessage("ERROR_REPORT_WIRING"));
                        return false;
                    }
                    else
                    {
                        WL.AddFlow addFlowItemWiring = systems.MyWiringNodeOut.AddFlow;
                        NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF)systems;

                        try
                        {
                            string imageDir = GetImagePathWiring();
                            drawing.Bitmap bmp = new drawing.Bitmap(875, 348);
                            drawing.Graphics gs = drawing.Graphics.FromImage(bmp);
                            Metafile mf = new Metafile(filePath, gs.GetHdc());
                            drawing.Graphics g = drawing.Graphics.FromImage(mf);
                            if (isBackWhite)
                            {
                                g.Clear(System.Drawing.Color.White);
                            }
                            DrawEMF_wiringNextGen(g, addFlowItemWiring, sysItem, imageDir);
                            g.Save();
                            g.Dispose();
                            gs.Dispose();
                            mf.Dispose();
                            if (isWiring == true)
                            {
                                var proj = JCHVRF.Model.Project.GetProjectInstance;
                                JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                                if (!rpt.ExportReportWiringJPEG(filePath))
                                {
                                    return true;
                                }
                                isWiring = false;
                            }
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        JCHVRF.MyPipingBLL.UtilEMF utilEMF = new JCHVRF.MyPipingBLL.UtilEMF();
        private IModalWindowService _winService;

        private bool ExportVictorGraph_ContollerWiring(string filePath, ControlGroup group, bool isBackWhite = false)
        {
            try
            {

                string imageDir = GetImagePathWiring();

                Bitmap bmp = new drawing.Bitmap(5000, 800);
                Graphics gs = Graphics.FromImage(bmp);
                Metafile mf = new Metafile(filePath, gs.GetHdc());

                Graphics g = Graphics.FromImage(mf);
                if (isBackWhite)
                {
                    g.Clear(Color.White);
                }

                DrawEMF_ControllerWiring(g, addFlowControllerWiring, group, imageDir);

                g.Save();
                g.Dispose();
                gs.Dispose();
                mf.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void DrawEMF_ControllerWiring(Graphics g, WL.AddFlow addFlowItem, ControlGroup group, string WiringImageDir)
        {
            foreach (WL.Item item in addFlowItem.Items)
            {
                DrawItemRecursion(g, item, WiringImageDir);
            }
        }

        private void DrawEMF_wiringNextGen(drawing.Graphics g, WL.AddFlow addFlowWiring, NextGenModel.SystemVRF curSystemItem, string WiringImageDir)
        {
            try
            {
                NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
                var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
                //Wiring WiringPage = new Wiring(this.Project, this.Project.SystemListNextGen[0], addFlowWiring);
                Wiring WiringPage = new Wiring(this.Project, curSystemItem, addFlowWiring);
                addFlowWiring = WiringPage.GetAddFlowInstance();
                JCHVRF.Model.Project thisProject = this.Project;
                drawing.PointF ptf1 = new drawing.PointF(0, 0);
                drawing.PointF ptf2 = new drawing.PointF(0, 0);
                drawing.PointF ptf3 = new drawing.PointF(0, 0);
                NextGenBLL.UtilEMF utilEMF = new NextGenBLL.UtilEMF();
                foreach (WL.Item item in addFlowWiring.Items)
                {
                    if (item is WL.Node)
                    {
                        WL.Node nd = item as WL.Node;
                        if (nd is NextGenModel.WiringNodeOut)
                        {
                            NextGenModel.WiringNodeOut ndOut = nd as NextGenModel.WiringNodeOut;
                            JCHVRF.MyPipingBLL.NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_ODUNextGen(curSystemItem.OutdoorItem, thisProject.BrandCode);
                            string nodePointsFile = System.IO.Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                            //apply per new nextgen class
                            utilEMF.DrawNode_wiringNextGen(g, ndOut, nodePointsFile, curSystemItem.Name, item_wiring);
                        }
                        else if (nd is NextGenModel.WiringNodeIn)
                        {

                            NextGenModel.WiringNodeIn ndIn = nd as NextGenModel.WiringNodeIn;
                            List<string> strArrayList_powerType = new List<string>();
                            List<string> strArrayList_powerVoltage = new List<string>();
                            List<double> dArrayList_powerCurrent = new List<double>();

                            int powerIndex = 0;
                            bool isNewPower = false;
                            JCHVRF.MyPipingBLL.NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_IDUNextGen(ndIn.RoomIndoorItem.IndoorItem, thisProject.BrandCode, curSystemItem.OutdoorItem.Type, ref strArrayList_powerType, ref strArrayList_powerVoltage, ref dArrayList_powerCurrent, ref powerIndex, ref isNewPower);

                            string nodePointsFile = System.IO.Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                            //apply as per new nextgen class
                            utilEMF.DrawNode_wiringNextGen(g, ndIn, nodePointsFile, ndIn.RoomIndoorItem.IndoorName, item_wiring);
                        }
                        else if (nd is NextGenModel.WiringNodeCH)
                        {
                            utilEMF.DrawNodeTextWiringCHbox(g, nd);
                        }
                        else if (nd is NextGenModel.MyNodeGround_Wiring)
                        {
                            NextGenModel.MyNodeGround_Wiring ndGnd = nd as NextGenModel.MyNodeGround_Wiring;
                            string nodePointsFile = System.IO.Path.Combine(WiringImageDir, "Ground.txt");
                            //aaply as per new next gen class
                            utilEMF.DrawNode_wiringNextGen(g, ndGnd, nodePointsFile, "", null);

                        }
                        else if (nd is NextGenModel.MyNodeRemoteControler_Wiring)
                        {
                            NextGenModel.MyNodeRemoteControler_Wiring ndRC = nd as NextGenModel.MyNodeRemoteControler_Wiring;
                            string nodePointsFile = System.IO.Path.Combine(WiringImageDir, "RemoteControler.txt");
                            utilEMF.DrawNode_wiringNextGen(g, ndRC, nodePointsFile, "", null);
                        }
                        else
                        {

                            if (!string.IsNullOrEmpty(nd.Text))
                            {
                                drawing.PointF pf = new drawing.PointF(Convert.ToInt64(nd.Location.X), Convert.ToInt64(nd.Location.Y));
                                //g.DrawString(nd.Text, utilEMF.textFont_wiring, utilEMF.textBrush_wiring, pf);
                                if (nd.Text != "//" && nd.Text != "///" && nd.Text != "////")
                                {
                                    Font ft = new Font("Arial Narrow", 7.5f, System.Drawing.FontStyle.Regular);
                                    g.DrawString(nd.Text, ft, utilEMF.textBrush_wiring, pf);
                                }
                                else
                                {
                                    System.Drawing.Brush brush = new System.Drawing.SolidBrush(drawing.Color.Red);
                                    pf.Y += 2.5f;
                                    g.DrawString(nd.Text, utilEMF.textFont_wiring, brush, pf);
                                }
                            }
                        }
                    }
                }
                var links = addFlowWiring.Items.OfType<WL.Link>().ToArray();
                foreach (var item in links)
                {
                    utilEMF.DrawLine(g, item);
                }
                List<drawing.PointF[]> ptArrayList_power = new List<drawing.PointF[]>();

                List<drawing.PointF[]> ptArrayList_ground = new List<drawing.PointF[]>();
                /// <summary>
                /// 室内机总电源线坐标列表
                /// </summary>
                List<drawing.PointF[]> ptArrayList_mainpower = new List<drawing.PointF[]>();
                foreach (drawing.PointF[] pt in ptArrayList_power)
                {
                    drawing.Pen pen = new drawing.Pen(drawing.Color.Red, 0.3f);
                    g.DrawLines(pen, pt);
                }

                foreach (drawing.PointF[] pt in ptArrayList_ground)
                {
                    drawing.Pen pen = new drawing.Pen(drawing.Color.Yellow, 0.1f);
                    drawing.PointF[] pt1 = pt;
                    if (pt.Length > 2)
                    {
                        pt1 = new drawing.PointF[] { pt[0], pt[1] };
                    }
                    g.DrawLines(pen, pt1);
                }
            }
            catch (Exception)
            {

            }
        }
        public void DoDrawPipingAllignment(bool reset, JCHVRF.Model.NextGen.SystemVRF CurrentSystem, ref WL.AddFlow addflows)
        {


            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            //utilPiping


            bool isHitachi = this.Project.BrandCode == "H";
            bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(CurrentSystem);


            string dir = GetBinDirectoryPath(System.Configuration.ConfigurationManager.AppSettings["PipingNodeImageDirectory"].ToString());
            //TO DO Pick VRF system in case of multi system


            NextGenModel.MyNodeOut pipingNodeOut = CurrentSystem.MyPipingNodeOut;
            if (pipingNodeOut == null || CurrentSystem.OutdoorItem == null)
            {
                return;
            }

            if (pipingNodeOut.ChildNode == null)
            {
                return;
            }

            if (isHR)
            {

                //SetAllNodesIsCoolingonlyFrom();
                pipBll.SetIsCoolingOnly(CurrentSystem.MyPipingNodeOut);
            }
            if (reset == false)
            {
                utilPiping.ResetColors();
                InitAndRemovePipingNodes(ref addflows);
                pipBll.DrawPipingNodes(CurrentSystem, dir, ref addflows);
                pipBll.DrawPipingLinks(CurrentSystem, ref addflows);
                pipBll.DrawLegendText(CurrentSystem, ref addflows);
            }
            if (reset)
            {
                CurrentSystem.IsManualPiping = false;
                utilPiping.ResetColors();
                InitAndRemovePipingNodes(ref addflows);
                pipBll.DrawPipingNodes(CurrentSystem, dir, ref addflows);
                pipBll.DrawPipingLinks(CurrentSystem, ref addflows);
                pipBll.DrawLegendText(CurrentSystem, ref addflows);

            }
        }
        public void DoDrawingPiping(bool reset, JCHVRF.Model.NextGen.SystemVRF CurrentSystem, ref WL.AddFlow addflows)
        {


            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            //utilPiping


            bool isHitachi = this.Project.BrandCode == "H";
            bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(CurrentSystem);


            string dir = GetBinDirectoryPath(System.Configuration.ConfigurationManager.AppSettings["PipingNodeImageDirectory"].ToString());
            //TO DO Pick VRF system in case of multi system


            NextGenModel.MyNodeOut pipingNodeOut = CurrentSystem.MyPipingNodeOut;
            if (pipingNodeOut == null || CurrentSystem.OutdoorItem == null)
            {
                return;
            }

            if (pipingNodeOut.ChildNode == null)
            {
                return;
            }

            if (isHR)
            {

                //SetAllNodesIsCoolingonlyFrom();
                pipBll.SetIsCoolingOnly(CurrentSystem.MyPipingNodeOut);
            }

            if (reset)
            {
                utilPiping.ResetColors();
                InitAndRemovePipingNodes(ref addflows);
                pipBll.DrawPipingNodes(CurrentSystem, dir, ref addflows);
                pipBll.DrawPipingLinks(CurrentSystem, ref addflows);
                pipBll.DrawLegendText(CurrentSystem, ref addflows);
                //pipBll.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem, ref addflows);


            }
            pipBll.drawPipelegend(isHR, ref addflows);
            pipBll.SetDefaultColor(ref addflows, isHR);
            Validate(CurrentSystem, ref addflows);
            //CurrentSystem.IsPipingOK = false;

            //Start: Bug#1646
            //lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Transparent);
            //lblCanvasError.BorderThickness = new Thickness(0);
            //lblCanvasError.Content = string.Empty;
            //End: Bug#1646
        }



        public void Validate(JCHVRF.Model.NextGen.SystemVRF CurrentSystem, ref WL.AddFlow addflows)
        {
            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            string ut_length;
            string ut_power;
            string ut_temperature;
            string ut_airflow;
            string ut_weight;
            //try
            //{
            if (CurrentSystem == null)
                return;
            //if (CurrentSystem.IsPipingOK == true)
            //    return;
            utilPiping = new NextGenBLL.UtilPiping();
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            DoPipingFinalVerification(CurrentSystem, ref addflows);

            //}
            //catch { }
        }
        private void DoPipingFinalVerification(JCHVRF.Model.NextGen.SystemVRF currentSystem, ref WL.AddFlow addflows)
        {
            NextGenBLL.PipingErrors errorType = NextGenBLL.PipingErrors.OK;
            if (currentSystem.OutdoorItem == null)
            {
                return;
            }
            if (currentSystem.IsManualPiping && currentSystem.IsUpdated)
            {

                return;
            }
            //this.Cursor = Cursors.WaitCursor;
            JCHVRF_New.Utility.UtilityValidation ObjPipValidation = new JCHVRF_New.Utility.UtilityValidation(this.Project, ref addflows);
            JCHVRF.MyPipingBLL.NextGen.PipingBLL pipBll = GetPipingBLLInstanceValidation();
            bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(currentSystem);
            pipBll.SetPipingLimitation(currentSystem);

            errorType = pipBll.ValidateSystemHighDifference(currentSystem);

            if (errorType == NextGenBLL.PipingErrors.OK)
            {
                errorType = pipBll.ValidatePipeLength(currentSystem, ref addflows);
            }
            #region
            if (errorType == NextGenBLL.PipingErrors.OK)
            {
                if (!currentSystem.IsPipingOK)
                {
                    pipBll.SetDefaultColor(ref addflows, isHR);
                }
                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    if (currentSystem.PipeEquivalentLength < currentSystem.HeightDiff)
                    {
                        errorType = NextGenBLL.PipingErrors.PIPING_LENGTH_HEIGHT_DIFF; //-32;
                    }
                }

                if (currentSystem.IsInputLengthManually)
                {

                    errorType = pipBll.ValMainBranch(currentSystem, ref addflows);
                }
                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    if (NextGenBLL.PipingBLL.IsHeatRecovery(currentSystem) && !pipBll.ValCoolingOnlyIndoorCapacityRate(currentSystem, ref addflows))
                    {
                        errorType = NextGenBLL.PipingErrors.COOLINGONLYCAPACITY; //-12;
                    }
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    errorType = pipBll.ValidateIDUOfMultiCHBox(currentSystem);
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    SetSystemPipingOK(currentSystem, true);
                    DoPipingCalculation(pipBll, currentSystem.MyPipingNodeOut, currentSystem, out errorType);
                    if (currentSystem.IsPipingOK)
                    {
                        if (currentSystem.IsInputLengthManually && !pipBll.ValCHToIndoorMaxTotalLength(currentSystem, ref addflows))
                        {
                            errorType = NextGenBLL.PipingErrors.MKTOINDOORLENGTH1; //-8;
                        }
                        else if (!pipBll.ValMaxIndoorNumberConnectToCH(currentSystem, ref addflows))
                        {
                            errorType = NextGenBLL.PipingErrors.INDOORNUMBERTOCH; //-13;
                        }
                        else
                        {
                            SetSystemPipingOK(currentSystem, true);

                            if (currentSystem.IsInputLengthManually)
                            {
                                double d1 = pipBll.GetAddRefrigeration(currentSystem, ref addflows);
                                currentSystem.AddRefrigeration = d1;

                                pipBll.DrawAddRefrigerationText(currentSystem);
                            }
                            else
                            {
                                currentSystem.AddRefrigeration = 0;
                            }
                        }
                    }
                    ObjPipValidation.DrawTextToAllNodes(currentSystem.MyPipingNodeOut, null, currentSystem);
                    newUtilTrace.UtilTrace.SaveHistoryTraces();
                }
            }
            #endregion
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);
            }
            //  ShowWarningMsg(errorType);
            //UtilTrace.SaveHistoryTraces();                        
            //this.Cursor = Cursors.Default;
        }
        private void DoPipingCalculation(NextGenBLL.PipingBLL pipBll, JCHVRF.Model.NextGen.MyNodeOut nodeOut, JCHVRF.Model.NextGen.SystemVRF currentSystem, out NextGenBLL.PipingErrors errorType)
        {
            errorType = NextGenBLL.PipingErrors.OK;
            if (nodeOut.ChildNode == null)
            {
                return;
            }
            //????????????????????,???????????????  by Shen Junjie on 20170409
            //getSumCalculation(ref firstDstNode, factoryCode, type, unitType);

            pipBll.GetSumCapacity(nodeOut.ChildNode);

            pipBll.IsBranchKitNeedSizeUp(currentSystem);

            PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = nodeOut.ChildNode as JCHVRF.Model.NextGen.MyNodeYP;
                if (nodeYP.IsCP)
                {
                    //??????????? 20170711 by Yunxiao Lin
                    firstBranchKit = pipBll.getFirstHeaderBranchPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                else
                {
                    // ???????????
                    firstBranchKit = pipBll.getFirstPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                if (errorType != NextGenBLL.PipingErrors.OK)
                {
                    SetSystemPipingOK(currentSystem, false);
                    return;
                }
            }

            //bug 3489
            var L2SizeDownRule = pipBll.GetL2SizeDownRule(currentSystem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            //bug 3489

            pipBll.getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, currentSystem, false, out errorType, L2SizeDownRule);
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);
                return;
            }

            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut);
        }
        private NextGenBLL.PipingBLL GetPipingBLLInstanceValidation()
        {
            bool isInch = CommonBLL.IsDimension_inch();
            return new NextGenBLL.PipingBLL(this.Project, utilPiping, CurrentSystem.MyPipingNodeOut.AddFlow, isInch, ut_weight, ut_length, ut_power);
        }
        private void SetSystemPipingOK(JCHVRF.Model.NextGen.SystemVRF sysItem, bool isPipingOK)
        {
            if (sysItem.IsPipingOK != isPipingOK)
            {
                sysItem.IsPipingOK = isPipingOK;
                //SetTabControlImageKey();
            }
        }
        private void InitAndRemovePipingNodes(ref WL.AddFlow addflows)
        {
            //     addflow = new AddFlow();
            var Nodes = Enumerable.OfType<WL.Node>(addflows.Items).ToList();
            var Captions = Enumerable.OfType<WL.Caption>(addflows.Items).ToList();
            var Links = Enumerable.OfType<WL.Link>(addflows.Items).ToList();
            foreach (var item in Nodes)
                addflows.RemoveNode(item);

            foreach (var item in Captions)
                addflows.RemoveCaption(item);


            foreach (var item in Links)
                addflows.RemoveLink(item);


            //Nodes.ForEach((item) =>
            //{
            //    addflows.RemoveNode(item);
            //});
            //Captions.ForEach((item) =>
            //{
            //    addflows.RemoveCaption(item);
            //});
            //Links.ForEach((item) =>
            //{
            //    addflows.RemoveLink(item);
            //});
        }
        private string GetBinDirectoryPath(string AppSettingPath)
        {
            string binDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').ToString();
            binDirectory += AppSettingPath;
            return binDirectory;
        }



        /// <summary>
        /// The OnCloseProjectClicked
        /// </summary>

        private void OnCloseProjectClicked()
        {
            try
            {
                regionManager.RequestNavigate(RegionNames.ContentRegion, "Home");
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void GetRecentProjectFilesData()
        {
            string recentFilesPath = Path.Combine(Paths.JchTempDir, Paths.RecentFileList);
            if (File.Exists(recentFilesPath))
            {
                string[] files = File.ReadAllLines(recentFilesPath);
                if (files != null)
                {
                    RecentProjectFilesData.Clear();
                    foreach (var item in files)
                    {
                        RecentProjectFilesData.Add(item);
                    }
                    if (!RecentProjectFilesData.Contains("Clear Recent Files") && RecentProjectFilesData.Count > 0)
                    {
                        RecentProjectFilesData.Add("Clear Recent Files");
                    }
                }
                else
                {
                    //_recentProjectFiles.Add("Clear Recent Files");
                }

            }

        }
        private void clearRecentFiles()
        {
            try
            {
                if (!Directory.Exists(Paths.JchTempDir))
                {
                    Directory.CreateDirectory(Paths.JchTempDir);
                }
                string recentOpenedFiles = Path.Combine(Paths.JchTempDir, Paths.RecentFileList);

                if (!File.Exists(recentOpenedFiles))
                {
                    using (File.Create(recentOpenedFiles)) ;
                }

                List<string> fileLines = File.ReadAllLines(recentOpenedFiles)?.ToList();
                fileLines.Clear();
                File.WriteAllLines(recentOpenedFiles, fileLines.ToArray());
                RecentProjectFilesData.Clear();
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void OnOpenRecentClicked(string header)
        {
            try
            {
                if (header.Equals(language.Current.GetMessage("OPEN_RECENT"), StringComparison.OrdinalIgnoreCase))
                {
                    //do nothing, if the header is "Open Recent" text.
                    return;
                }
                else if(header.Equals("Clear Recent Files", StringComparison.OrdinalIgnoreCase))
                {
                    clearRecentFiles();
                }
                else
                {
                    int projectId = CommonBLL.ImportVRFProject("VRF", header);
                    if (projectId <= 0)
                    {
                        //JCHMessageBox.ShowWarning(Msg.IMPORT_PROJECT_FAILED);
                        JCHMessageBox.ShowWarning(language.Current.GetMessage("ALERT_PROJECT_IMPORT"));// "Project importing has been cancelled");
                        return;
                    }
                    else
                    {
                        //string recentDirectoryFile = Path.Combine(System.IO.Path.GetTempPath(), "JchVrfTempDir");//, "recentFiles.txt");
                        //if (!Directory.Exists(recentDirectoryFile))
                        //{
                        //    // Try to create the directory.
                        //    DirectoryInfo di = Directory.CreateDirectory(recentDirectoryFile);
                        //}
                        //string recentFilePath = Path.Combine(recentDirectoryFile, "recentFiles.txt");
                        //if (File.Exists(recentFilePath))
                        //{
                        //    using (var tw = new StreamWriter(recentFilePath, true))
                        //    {
                        //        tw.WriteLine("The next line!");
                        //    }
                        //}

                        //JCHMessageBox.Show(string.Format("Project ID {0} Loading...", projectId));
                        openVRFProject(projectId);
                        JCHMessageBox.Show(string.Format(language.Current.GetMessage("ALERT_PROJECT_OPEN_SUCCESS")));
                    }
                    GetRecentProjectFilesData();
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void OnExcelClickedCommand()
        {
            try
            {

                int isWiringOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.MyWiringNodeOut == null && s.IsExportToReport).Count();

                bool isAddflowexists = false;
                foreach (var item in JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen)
                {
                    if (item.MyWiringNodeOut != null)
                    {
                        if (item.MyWiringNodeOut.AddFlow == null)
                        {
                            isAddflowexists = true;
                        }
                    }
                }


                var proj = JCHVRF.Model.Project.GetProjectInstance;
                if (isAddflowexists == false && isWiringOkSystems <= 0)
                {
                    //var projs = JCHVRF.Model.Project.CurrentProject.SystemListNextGen;
                    //for (var i = 0; i < projs.Count(); i++)
                    //{
                    //    if (projs[i].IsExportToReport)
                    //    {
                    //        List<RoomIndoor> defaultAccessory = new List<RoomIndoor>();
                    //        defaultAccessory = (from idu in Project.CurrentProject.RoomIndoorList where projs[i].Id == idu.SystemID select idu).ToList();
                    //        AddAccessoriesTemplateViewModel childViewModel;
                    //        foreach (var indoor in defaultAccessory)
                    //        {
                    //            childViewModel = new AddAccessoriesTemplateViewModel(_eventAggregator, _winService);
                    //            childViewModel.GetAccessorie(indoor);
                    //        }
                    //    }
                    //}
                    if (ProjectBLL.IsSupportedNewReport(proj))
                    {

                        JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                        string reportTempalte = GetExcelReportTemplate();
                        string writeReport = ExcelWriteReport();
                        string newFileName = writeReport;
                        ExcelReportAspose excelAspose = new ExcelReportAspose(proj);
                        string fileName = "Selection Report Template.xlt";
                        if (proj.BrandCode == "H")
                        {
                            fileName = "Selection Report Template_H.xlt";
                        }
                        excelAspose.ExportReportExcel(reportTempalte + fileName, newFileName);

                    }
                    else
                    {
                        string reportTempalte = GetExcelReportTemplate();
                        string writeReport = WriteExcelReportTemplateRegionWise();
                        string newFileName = writeReport;
                        ExcelReportAspose excelAspose = new ExcelReportAspose(proj);
                        string fileName = "Selection Report Template.xlt";
                        if (proj.BrandCode == "H")
                        {
                            fileName = "Selection Report Template_H.xlt";
                        }
                        excelAspose.ExportReportExcel(reportTempalte + fileName, newFileName);

                    }
                }
                else
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_DO_WIRING_FOR_ALL_SYSTEMS"));
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        public string GetExcelReportTemplate()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string GetExcelReportTemplateRegionWise()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\VRF_Report.xls";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string WriteExcelReportTemplateRegionWise()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..Report\\Template\\VRF_Report.xls";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string ExcelWriteReport()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\Report.xls";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }


        #region Generate Report

        private void UpdatePipingNodeStructure(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            try
            {
                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstances();
                pipBll.CreatePipingNodeStructure(CurrentSystem);
                newUtilTrace.UtilTrace.SaveHistoryTraces();
            }
            catch (Exception ex)
            {
                JCHMessageBox.Show(language.Current.GetMessage("ERROR_OCCURRED") + ex.Message);
            }
        }
        private NextGenBLL.PipingBLL GetPipingBLLInstances()
        {
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            bool isInch = CommonBLL.IsDimension_inch();
            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            return new NextGenBLL.PipingBLL(JCHVRF.Model.Project.GetProjectInstance, utilPiping, null, isInch, ut_weight, ut_length, ut_power);
        }
        private void OnWordClickedCommand()
        {
            try
            {
                NextGenBLL.PipingBLL pipBllNG = GetPipingBLLInstance();

                DoSavePipingTempNodeOutStructure();
                if (JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen != null)
                {
                    isWordOrPDF = true;
                    //var proj = JCHVRF.Model.Project.GetProjectInstance;   

                    int includedtotalSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.IsExportToReport == true).Count();
                    //var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                    //CurrentSystemWiring = Lists;
                    int isPipingOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.IsPipingOK == true).Count();
                    int isWiringOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.MyWiringNodeOut == null).Count();

                    bool isAddflowexists = false;
                    foreach (var item in JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen)
                    {
                        if (item.MyWiringNodeOut != null)
                        {
                            if (item.MyWiringNodeOut.AddFlow == null)
                            {
                                isAddflowexists = true;
                            }
                        }
                    }
                    if (includedtotalSystems <= isPipingOkSystems) //&& includedControlSystems == isControlSystemOk)
                    {
                        if (isAddflowexists == false && isWiringOkSystems <= 0)
                        {
                            string outputDir = "";
                            try
                            {
                                var projs = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FindAll(s => s.IsExportToReport == true);
                                for (var i = 0; i < projs.Count(); i++)
                                {
                                    Lassalle.WPF.Flow.AddFlow tempa = new Lassalle.WPF.Flow.AddFlow();
                                    if (!projs[i].IsManualPiping)
                                    {
                                        pipBllNG.LoadPipingNodeStructure(projs[i]);
                                        //ucDesignerCanvas obj = new ucDesignerCanvas();    

                                        //SystemTab objsys = new SystemTab();
                                        //objsys.UpdatePipingNodeStructure(projs[i]);
                                        AutoPipingObj.DoDrawingPiping(true, (NextGenModel.SystemVRF)projs[i], tempa);
                                        pipingValidation.Validate((NextGenModel.SystemVRF)projs[i], tempa);
                                        //projs[i].IsExportToReport = true;      
                                    }
                                    else
                                    {
                                        PrepareForManualPipingImport((NextGenModel.SystemVRF)projs[i], ref tempa);
                                        pipingValidation.Validate((NextGenModel.SystemVRF)projs[i], tempa);
                                    }
                                    string filePath_pic = FileLocal.GetNamePathPipingPicture(projs[i].Id, outputDir);

                                    ExportVictorGraph(filePath_pic, true, projs[i].Id, (NextGenModel.SystemVRF)projs[i]);
                                    string filePath_picwiring = FileLocal.GetNamePathWiringPicture(projs[i].Id, outputDir);
                                    ExportVictorGraph_wiring(filePath_picwiring, true, projs[i].Id, (NextGenModel.SystemVRF)projs[i]);
                                    projs[i].IsPipingOK = true;
                                }


                                var projcc = JCHVRF.Model.Project.CurrentProject;
                                addFlowControllerWiring = new WL.AddFlow();

                                for (var i = 0; i < projcc.ControlGroupList.Count(); i++)
                                {
                                    addFlowControllerWiring.AllowDrop = false;
                                    addFlowControllerWiring.CanDrawNode = false;

                                    foreach (ControlGroup group in projcc.ControlGroupList)
                                    {
                                        if (group.IsValidGrp)
                                        {
                                            ControlSystem controlSystem = projcc.ControlSystemList.Find(x => x.Id.Equals(group.ControlSystemID));
                                            JCHVRF_New.Utility.CentralControllerWiring cwBLL = new JCHVRF_New.Utility.CentralControllerWiring(projcc, controlSystem, addFlowControllerWiring);
                                            DoSaveControllerWiringFilePicture(controlSystem.Id, group);
                                        }
                                    }
                                }
                            }

                            catch (Exception ex)
                            { }

                            if (isWordOrPDF != false)
                            {
                                var proj = JCHVRF.Model.Project.CurrentProject;
                                JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                                string reportTempalte = GetReportTemplate();
                                string writeReport = WriteReport();
                                if (!rpt.ExportReportWord(reportTempalte, writeReport))
                                {
                                    isWordOrPDF = true;
                                    return;
                                }
                            }
                        }

                        else
                        {
                            JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_DO_WIRING_FOR_ALL_SYSTEMS"));
                        }
                    }
                    else
                    {
                        JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_VALIDATION_FOR_ALL_SYSTEMS"));

                    }
                }
                else
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_CREATE"));
                }

                AutoPipingObj.UpdateAllPipingNodeStructure();

            }
            catch (Exception ex)
            {
                AutoPipingObj.UpdateAllPipingNodeStructure();
                //int? id = Project?.projectID;
                //Logger.LogProjectError(id, ex);
            }

        }

        private void DoSaveControllerWiringFilePicture(string groupId, ControlGroup group = null, string outputDir = null)
        {
            string filePath_pic = FileLocal.GetNamePathWiringPicture(group.Id, outputDir);
            if (outputDir != null)
            {
                string fileName = defaultFolder + "\\temp" + Guid.NewGuid().ToString() + ".emf";//
                ExportVictorGraph_ContollerWiring(fileName, group, true);
                Bitmap bmp = new Bitmap(fileName);
                Image img = PictureProcess(bmp, Convert.ToInt32(bmp.Width * 1.25d), Convert.ToInt32(bmp.Height * 1.25));
                img.Save(filePath_pic, ImageFormat.Jpeg);
            }
            else
            {
                ExportVictorGraph_ContollerWiring(filePath_pic, group, true);
            }
        }

        public Image PictureProcess(Image sourceImage, int targetWidth, int targetHeight)
        {
            int width;//图片最终的宽  
            int height;//图片最终的高  
            try
            {
                System.Drawing.Imaging.ImageFormat format = sourceImage.RawFormat;
                Bitmap targetPicture = new Bitmap(targetWidth, targetHeight);
                Graphics g = Graphics.FromImage(targetPicture);
                g.Clear(Color.White);

                //计算缩放图片的大小  
                if (sourceImage.Width > targetWidth && sourceImage.Height <= targetHeight)
                {
                    width = targetWidth;
                    height = (width * sourceImage.Height) / sourceImage.Width;
                }
                else if (sourceImage.Width <= targetWidth && sourceImage.Height > targetHeight)
                {
                    height = targetHeight;
                    width = (height * sourceImage.Width) / sourceImage.Height;
                }
                else if (sourceImage.Width <= targetWidth && sourceImage.Height <= targetHeight)
                {
                    width = sourceImage.Width;
                    height = sourceImage.Height;
                }
                else
                {
                    width = targetWidth;
                    height = (width * sourceImage.Height) / sourceImage.Width;
                    if (height > targetHeight)
                    {
                        height = targetHeight;
                        width = (height * sourceImage.Width) / sourceImage.Height;
                    }
                }
                g.DrawImage(sourceImage, (targetWidth - width) / 2, (targetHeight - height) / 2, width, height);
                sourceImage.Dispose();

                return targetPicture;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void PrepareForManualPipingImport(NextGenModel.SystemVRF newSystem, ref Lassalle.WPF.Flow.AddFlow addflow)
        {
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            string dir = GetImagePathPiping();
            if (newSystem != null && newSystem.MyPipingNodeOutTemp != null)
            {
                pipBll.LoadPipingNodeStructure(newSystem);
                pipBll.LoadPipingOrphanNodes(newSystem);
                pipBll.DrawManualPipingNodes(newSystem, dir, ref addflow);
                //pipBll.DrawOrphanIndoorNodes(newSystem, ref addflow);
                // TODO
                //SetManualPipingModelOnAddFlow(ref addflow);

                pipBll.DrawManualLinkForNormal(newSystem.MyPipingNodeOut, newSystem.MyPipingNodeOut.ChildNode, true,
                    CurrentSystem, ref addflow);
                pipBll.DrawLegendText(CurrentSystem, ref addflow);
                //addflow.LinkCreating += Addflow_LinkCreating;
                //addflow.LinkCreated += Addflow_LinkCreated;
            }
        }

        private void OnpdfClickedCommand()
        {
            try
            {
                NextGenBLL.PipingBLL pipBllNG = GetPipingBLLInstance();

                DoSavePipingTempNodeOutStructure();

                isWordOrPDF = true;
                int includedtotalSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.IsExportToReport).Count();
                int isPipingOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.IsPipingOK == true).Count();
                int isWiringOkSystems = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FindAll(s => s.MyWiringNodeOut == null).Count();

                bool isAddflowexists = false;
                foreach (var item in JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen)
                {
                    if (item.MyWiringNodeOut != null)
                    {
                        if (item.MyWiringNodeOut.AddFlow == null)
                        {
                            isAddflowexists = true;
                        }
                    }
                }
                if (includedtotalSystems <= isPipingOkSystems)
                {
                    if (isAddflowexists == false && isWiringOkSystems <= 0)
                    {
                        string outputDir = "";
                        try
                        {
                            var projs = JCHVRF.Model.Project.CurrentProject.SystemListNextGen;
                            for (var i = 0; i < projs.Count(); i++)
                            {
                                Lassalle.WPF.Flow.AddFlow tempa = new Lassalle.WPF.Flow.AddFlow();
                                if (!projs[i].IsManualPiping)
                                {
                                    //ucDesignerCanvas obj = new ucDesignerCanvas();
                                    pipBllNG.LoadPipingNodeStructure(projs[i]);
                                    //pipBllNG.LoadPipingNodeStructure(projs[i]);
                                    //ucDesignerCanvas obj = new ucDesignerCanvas();
                                    //SystemTab objsys = new SystemTab();
                                    //objsys.UpdatePipingNodeStructure(projs[i]);
                                    AutoPipingObj.DoDrawingPiping(true, (NextGenModel.SystemVRF)projs[i], tempa);
                                    pipingValidation.Validate((NextGenModel.SystemVRF)projs[i], tempa);
                                }
                                else
                                {
                                    PrepareForManualPipingImport((NextGenModel.SystemVRF)projs[i], ref tempa);
                                    pipingValidation.Validate((NextGenModel.SystemVRF)projs[i], tempa);

                                }
                                //  projs[i].IsExportToReport = true;
                                string filePath_pic = FileLocal.GetNamePathPipingPicture(projs[i].Id, outputDir);
                                ExportVictorGraph(filePath_pic, true, projs[i].Id, (NextGenModel.SystemVRF)projs[i]);
                                string filePath_picwiring = FileLocal.GetNamePathWiringPicture(projs[i].Id, outputDir);
                                ExportVictorGraph_wiring(filePath_picwiring, true, projs[i].Id, (NextGenModel.SystemVRF)projs[i]);
                            }

                            var projcc = JCHVRF.Model.Project.CurrentProject;
                            addFlowControllerWiring = new WL.AddFlow();
                            for (var i = 0; i < projcc.ControlSystemList.Count(); i++)
                            {
                                addFlowControllerWiring.AllowDrop = false;
                                addFlowControllerWiring.CanDrawNode = false;

                                ControlSystem controlSystem = projcc.ControlSystemList.Find(x => x.Id.Equals(projcc.ControlGroupList[i].ControlSystemID));

                                JCHVRF_New.Utility.CentralControllerWiring cwBLL = new JCHVRF_New.Utility.CentralControllerWiring(projcc, controlSystem, addFlowControllerWiring);
                                foreach (ControlGroup group in projcc.ControlGroupList)
                                {
                                    if (group.ControlSystemID == controlSystem.Id)
                                    {
                                        DoSaveControllerWiringFilePicture(group.Id, group);
                                    }
                                }
                            }
                        }
                        catch { }
                        if (isWordOrPDF != false)
                        {
                            var proj = JCHVRF.Model.Project.CurrentProject;
                            JCHVRF.NewReport rpt = new JCHVRF.NewReport(proj);
                            string reportTempalte = GetReportTemplate();
                            string writeReport = WriteReportpdf();
                            if (!rpt.ExportReportPDF(reportTempalte, writeReport))
                            {
                                isWordOrPDF = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_DO_WIRING_FOR_ALL_SYSTEMS"));
                    }
                }
                else
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_REPORT_VALIDATION_FOR_ALL_SYSTEMS"));

                }
                AutoPipingObj.UpdateAllPipingNodeStructure();
            }
            catch (Exception ex)
            {
                AutoPipingObj.UpdateAllPipingNodeStructure();
                //int? id = Project?.projectID;
                //Logger.LogProjectError(id, ex);
            }
        }



        #endregion Generate Report
        #endregion

        public string GetReportTemplate()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "";
            if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                navigateToFolder = "..\\..\\Report\\Template\\NewReport\\NewReport.doc";
            else if (JCHVRF.Model.Project.CurrentProject.BrandCode == "Y")
                navigateToFolder = "..\\..\\Report\\Template\\NewReport\\NewReportYork.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        public string WriteReportTemplateRegionWise()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\VRF_Report.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string WriteReportTemplatepdfRegionWise()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\VRF_Report.pdf";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string WriteReport()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "";
            if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                navigateToFolder = "..\\..\\Report\\Template\\Report.doc";
            else if (JCHVRF.Model.Project.CurrentProject.BrandCode == "Y")
                navigateToFolder = "..\\..\\Report\\Template\\NewReportYork.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string WriteReportpdf()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\Report.pdf";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        public string GetReportTemplateRegionWise()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\VRF_Report.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        private void onSystemSelected(SystemBase systemBase)
        {
            try
            {
                if (!Systems.Contains(systemBase))
                {
                    Systems.Add(systemBase);
                }
                else
                {
                    //int index = Systems.IndexOf(systemBase);
                    //Systems.RemoveAt(index);
                    //Systems.Insert(index, systemBase);
                }

                SelectedTabIndex = Systems.IndexOf(systemBase);
                setSystemSpecificMenuVisibility(systemBase);
                _eventAggregator.GetEvent<GetCurrentSystem>().Publish(systemBase);
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnCanvasClickSystemSelected(SystemBase systemBase)
        {
            try
            {
                setSystemSpecificMenuVisibility(systemBase);
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnCentralControllerCanvasClickSystemSelected(SystemBase systemBase)
        {
            try
            {
                setSystemSpecificMenuVisibility(systemBase);
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnHeatExchangerCanvasClickSystemSelected(SystemBase systemBase)
        {
            try
            {
                setSystemSpecificMenuVisibility(systemBase);
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void setSystemSpecificMenuVisibility(SystemBase system)
        {
            this._currentSystem = system;
            string hvacSystemType = system.HvacSystemType;
            int hvacSystemTypeAsInt = Convert.ToInt32(hvacSystemType);
            //_currentSystem._errors.Clear();
            switch (hvacSystemTypeAsInt)
            {
                case 1:
                    VRFMenuVisibility = Visibility.Visible;
                    ConrollerMenuVisibility = Visibility.Collapsed;
                    HeatExhangerMenuVisibility = Visibility.Collapsed;
                    VRFMenuTabSelected = true;
                    ControllerMenuTabSelected = false;
                    HeatExchangerMenuTabSelected = false;

                    ProjectDetailsHandle.CanTogglePaneVisibility = true;
                    SystemDetailsHandle.CanTogglePaneVisibility = true;
                    PipingInfoHandle.CanTogglePaneVisibility = true;
                    PropertiesHandle.CanTogglePaneVisibility = true;
                    ErrorLogHandle.CanTogglePaneVisibility = true;
                    NavigatorHandle.CanTogglePaneVisibility = true;

                    regionManager.RequestNavigate(RegionNames.MasterDesignerSystemDetailsRegion, ViewKeys.VRFSystemDetails);

                    _eventAggregator.GetEvent<ErrorLogVMClearAll>().Publish();

                    break;

                case 6:
                    ConrollerMenuVisibility = Visibility.Visible;
                    VRFMenuVisibility = Visibility.Collapsed;
                    HeatExhangerMenuVisibility = Visibility.Collapsed;
                    VRFMenuTabSelected = false;
                    ControllerMenuTabSelected = true;
                    HeatExchangerMenuTabSelected = false;

                    ProjectDetailsHandle.CanTogglePaneVisibility = true;
                    SystemDetailsHandle.CanTogglePaneVisibility = true;
                    PipingInfoHandle.CanTogglePaneVisibility = false;
                    PropertiesHandle.CanTogglePaneVisibility = false;
                    ErrorLogHandle.CanTogglePaneVisibility = true;
                    NavigatorHandle.CanTogglePaneVisibility = true;

                    regionManager.RequestNavigate(RegionNames.MasterDesignerSystemDetailsRegion, ViewKeys.CentralControllerSystemDetail);
                    _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
                    _eventAggregator.GetEvent<ErrorLogVMClearAll>().Publish();
                    if (_currentSystem._errors.Count > 0)
                    {
                        _eventAggregator.GetEvent<ErrorLogVM>().Publish(_currentSystem.Errors);
                    }
                    break;

                case 2:

                    HeatExhangerMenuVisibility = Visibility.Visible;
                    VRFMenuVisibility = Visibility.Collapsed;
                    ConrollerMenuVisibility = Visibility.Collapsed;
                    VRFMenuTabSelected = false;
                    ControllerMenuTabSelected = false;
                    HeatExchangerMenuTabSelected = true;

                    ProjectDetailsHandle.CanTogglePaneVisibility = true;
                    SystemDetailsHandle.CanTogglePaneVisibility = false;
                    PipingInfoHandle.CanTogglePaneVisibility = false;
                    PropertiesHandle.CanTogglePaneVisibility = false;
                    ErrorLogHandle.CanTogglePaneVisibility = false;
                    NavigatorHandle.CanTogglePaneVisibility = false;
                    if (_isSysProp||_isSysPropFA) { HeatExchangerCanvasEquipmentViewModel.flgTabChanged = false; }
                    else
                    { HeatExchangerCanvasEquipmentViewModel.flgTabChanged = true; }
                    break;
            }
            IsFileMenuSelected = IsUndoEnabled || IsRedoEnabled;
        }

        private void OnSaveNewProjectClicked()
        {
            try
            {
                DoSavePipingTempNodeOutStructure();
                ProjectInfoBLL objProjectInfoBll = new ProjectInfoBLL();
                bool status = objProjectInfoBll.UpdateProject(Project.CurrentProject);
                Application.Current.Properties["ProjectId"] = Project.CurrentProject.projectID;
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }
        private void OnSaveClicked()
        {
            try
            {
                DoSavePipingTempNodeOutStructure();
                ProjectInfoBLL objProjectInfoBll = new ProjectInfoBLL();
                bool status = objProjectInfoBll.UpdateProject(Project.CurrentProject);
                Application.Current.Properties["ProjectId"] = Project.CurrentProject.projectID;
                //regionManager.RequestNavigate(RegionNames.ContentRegion, "Home");
                JCHMessageBox.Show(language.Current.GetMessage("SAVED_SUCCESSFULLY"));
                // _globalProperties.ProjectTitle =JCHVRF.Model.Project.CurrentProject.Name;//refactor code
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void OnSaveAsClicked()
        {
            try
            {

                DoSavePipingTempNodeOutStructure();
                ProjectInfoBLL objProjectInfoBll = new ProjectInfoBLL();
                bool status = objProjectInfoBll.UpdateProject(Project.GetProjectInstance);
                Application.Current.Properties["ProjectId"] = Project.GetProjectInstance.projectID;

                CDF.ExportProject(Project.GetProjectInstance.projectID, "VRF");
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }

        }

        private void OpenProjectClicked()
        {
            try
            {
                int projectId = CommonBLL.ImportVRFProject("VRF");
                if (projectId <= 0)
                {
                    //JCHMessageBox.ShowWarning(Msg.IMPORT_PROJECT_FAILED);
                    JCHMessageBox.ShowWarning(language.Current.GetMessage("ALERT_PROJECT_IMPORT"));// "Project importing has been cancelled");
                    return;
                }
                else
                {
                    //string recentDirectoryFile = Path.Combine(System.IO.Path.GetTempPath(), "JchVrfTempDir");//, "recentFiles.txt");
                    //if (!Directory.Exists(recentDirectoryFile))
                    //{
                    //    // Try to create the directory.
                    //    DirectoryInfo di = Directory.CreateDirectory(recentDirectoryFile);
                    //}
                    //string recentFilePath = Path.Combine(recentDirectoryFile, "recentFiles.txt");
                    //if (File.Exists(recentFilePath))
                    //{
                    //    using (var tw = new StreamWriter(recentFilePath, true))
                    //    {
                    //        tw.WriteLine("The next line!");
                    //    }
                    //}

                    //JCHMessageBox.Show(string.Format("Project ID {0} Loading...", projectId));
                    openVRFProject(projectId);
                    JCHMessageBox.Show(string.Format(language.Current.GetMessage("ALERT_PROJECT_OPEN_SUCCESS")));
                    GetRecentProjectFilesData();
                }
            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        private void openVRFProject(int projectId)
        {
            Project importProj = CommonBLL.OpenVRFProject(projectId);


            Project = importProj;
            JCHVRF.Model.Project.CurrentProject = Project;
            NavigationParameters param = new NavigationParameters();
            param.Add("Project", Project);
            regionManager.RequestNavigate(RegionNames.ContentRegion, "Null");
            regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.MasterDesigner, param);
        }

        //TODO : <sandeep>Will Move this duplicate method to CommonBLL. So that It can be used at MasterDesignerViewmodel and NotificationBarViewModel.
        public void DoSavePipingTempNodeOutStructure()
        {
            try
            {
                PipingBLL pipBllNG = GetPipingBLLInstance();
                pipBllNG.SaveAllPipingStructure();
            }
            catch (Exception ex)
            {
                JCHMessageBox.Show(language.Current.GetMessage("ERROR_OCCURRED") + ex.Message);
            }
        }
        public PipingBLL GetPipingBLLInstance()
        {
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            bool isInch = CommonBLL.IsDimension_inch();
            UtilPiping utilPiping = new UtilPiping();
            return new PipingBLL(JCHVRF.Model.Project.GetProjectInstance, utilPiping, null, isInch, ut_weight, ut_length, ut_power);
        }
        public void OnDeleteSystemClicked()
        {
            try
            {
                //var currentProject = Project.CurrentProject;
                var systemToDelete = Systems[SelectedTabIndex];
                var systemType = Convert.ToInt32(systemToDelete.HvacSystemType);
                switch (systemType)
                {
                    case 1:
                        if (Project.CurrentProject.SystemListNextGen.Count > 1)
                            if (AskDelete())
                            {
                                DeleteSystem();
                                //newUtilTrace.UtilTrace.SaveHistoryTraces();
                                //UndoRedoSetup.SetInstanceNull();
                                //setupUndoRedo();

                            }
                            else
                                return;
                        else
                            ShowMessageCantD();
                        break;
                    case 2:
                        if (Project.HeatExchangerSystems.Count > 1)
                            if (AskDelete())
                            {
                                DeleteSystem();
                                //DeleteSystemExch();
                                //Project.GetProjectInstance.SelectedSystemID = string.Empty;
                                //newUtilTrace.UtilTrace.SaveHistoryTraces();
                                //UndoRedoSetup.SetInstanceNull();
                                //setupUndoRedo();
                            }
                            else
                                return;
                        else
                            ShowMessageCantD();
                        break;
                    case 6:
                        if (Project.ControlSystemList.Count > 1)
                            if (AskDelete())
                            {
                                DeleteSystem();
                                //Project.GetProjectInstance.SelectedSystemID = string.Empty;
                                //newUtilTrace.UtilTrace.SaveHistoryTraces();
                                //UndoRedoSetup.SetInstanceNull();
                                //setupUndoRedo();
                            }
                            else
                                return;
                        else
                            ShowMessageCantD();
                        break;

                }

            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }
        }

        public void OnCoolingSystemClicked()
        {
            var systemToCooling = Systems[SelectedTabIndex];
        }
        public void OnCancelCoolingSystemClicked()
        {
            var systemToCancelCooling = Systems[SelectedTabIndex];
        }
        private void ShowMessageCantD()
        {
            JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_DELETE"));
        }
        private bool AskDelete()
        {
            return JCHMessageBox.Show(language.Current.GetMessage("CONFIRM_SYSTEM_DELETE"), MessageType.Information, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes;
        }

        public void DeleteSystem()
        {
            try
            {
                blnCallfromDelete = true;
                Project = Project.CurrentProject;
                List<RoomIndoor> RoomIndoorList = Project.RoomIndoorList;
                List<RoomIndoor> ExchList = Project.ExchangerList;
                NavigationParameters param = new NavigationParameters();
                var systemToDelete = Systems[SelectedTabIndex];
                var systemType = Convert.ToInt32(systemToDelete.HvacSystemType);

                switch (systemType)
                {
                    case 1:
                        Project.SystemListNextGen.Remove((NextGenModel.SystemVRF)systemToDelete);

                        for (int i = RoomIndoorList.Count - 1; i >= 0; i--)
                        {
                            if (systemToDelete.Id == RoomIndoorList[i].SystemID)
                                RoomIndoorList.RemoveAt(i);

                        }
                        Project.RoomIndoorList = RoomIndoorList;
                        JCHVRF.Model.Project.CurrentProject = Project;
                        Project.GetProjectInstance.SelectedSystemID = string.Empty;
                        UndoRedoSetup.SetInstanceNull();
                        Systems.RemoveAt(SelectedTabIndex);
                        _eventAggregator.GetEvent<RefreshSystems>().Publish();
                        JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_DELETE_SUCCESS"));
                        break;

                    case 2: //Heat Exchanger
                        Project.HeatExchangerSystems.Remove((SystemHeatExchanger)systemToDelete);
                        for (int i = ExchList.Count - 1; i >= 0; i--)
                        {
                            if (systemToDelete.Id == ExchList[i].SystemID)
                                ExchList.RemoveAt(i);
                        }
                        Project.ExchangerList = ExchList;
                        JCHVRF.Model.Project.CurrentProject = Project;
                        param.Add("Project", Project);
                        Project.GetProjectInstance.SelectedSystemID = string.Empty;
                        UndoRedoSetup.SetInstanceNull();
                        Systems.RemoveAt(SelectedTabIndex);
                        regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, param);
                        _eventAggregator.GetEvent<RefreshSystems>().Publish();
                        JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_DELETE_SUCCESS"));
                        break;

                    case 6: //Controller
                        Project.ControlSystemList.Remove((ControlSystem)systemToDelete);
                        int indexDelete = Project.ControlGroupList.FindIndex(x => x.ControlSystemID == systemToDelete.Id);
                        // this.Project.ControlSystemList.Remove((ControlSystem)systemToDelete);
                        if (indexDelete != -1)
                        {
                            Project.GetProjectInstance.SelectedSystemID = string.Empty;
                            Project.ControlGroupList.RemoveAt(indexDelete);
                        }
                        UndoRedoSetup.SetInstanceNull();
                        Systems.RemoveAt(SelectedTabIndex);
                        JCHVRF.Model.Project.CurrentProject = Project;
                        param.Add("Project", Project);
                        regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, param);
                        _eventAggregator.GetEvent<RefreshSystems>().Publish();
                        JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_DELETE_SUCCESS"));
                        break;
                }



                //List<RoomIndoor> RoomIndoorList = Project.RoomIndoorList;
                //Project.SystemListNextGen.Remove(CurrentSystem);

                //for (int i = RoomIndoorList.Count - 1; i >= 0; i--)
                //{
                //    if (CurrentSystem.Id == RoomIndoorList[i].SystemID)
                //        RoomIndoorList.RemoveAt(i);

                //}
                //Project.RoomIndoorList = RoomIndoorList;
                //JCHVRF.Model.Project.CurrentProject = Project;
                //NavigationParameters param = new NavigationParameters();
                //param.Add("Project", Project);
                //regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, param);
                //JCHMessageBox.Show(" System Successfully Deleted!");

            }
            catch (Exception ex) { }

        }

        private void OnDuplicateClickCommand()
        {
            #region saving current state
            try
            {
                DoSavePipingTempNodeOutStructure();
                (new ProjectInfoBLL()).UpdateProject(Project.CurrentProject);
            }
            catch (Exception) { }
            #endregion
            try
            {
                if (SelectedTabIndex < 0 || Systems[SelectedTabIndex] == null)
                {
                    JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_DUPLICATE"));
                    return;
                }

                var systemToDuplicate = Systems[SelectedTabIndex];

                var systemType = Convert.ToInt32(systemToDuplicate.HvacSystemType);


                SystemBase newSystem = null;

                switch (systemType)
                {
                    case 1:
                        string validationMessage = "";
                        var validationResult = SystemValidationHelper.IsSystemValidForDuplication(((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate), out validationMessage);
                        
                        if(!validationResult)
                        {
                            JCHMessageBox.Show(validationMessage);
                            return;
                        }
                        
                        if (!((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).IsManualPiping || (((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).SystemStatus != SystemStatus.WIP && ((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).SystemStatus != SystemStatus.INVALID))
                        {
                            newSystem = ((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).Duplicate();
                            if ((JCHVRF.Model.NextGen.SystemVRF)newSystem != null)
                            {
                                ((JCHVRF.Model.NextGen.SystemVRF)newSystem).IsOutDoorUpdated = false;
                                CurrentSystem.IsOutDoorUpdated = false;
                            }
                            UndoRedoSetup.SetInstanceNull();
                            Project.GetProjectInstance.SelectedSystemID = newSystem.Id;
                            if(((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).IsPipingOK == true)
                            {
                                PublishDuplicationEvents(newSystem, (JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate);
                            }
                            else
                            {
                                PublishDuplicationEventWithoutValidate(newSystem, (JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate);
                            }

                        }
                        else
                        {
                            JCHMessageBox.Show(language.Current.GetMessage("ALERT_SYSTEM_DUPLICATE_DO_MANUAL_VALIDATION"));
                            return;
                        }
                        break;
                    case 2:
                        if (systemToDuplicate.SystemStatus == JCHVRF_New.Model.SystemStatus.VALID)
                        {
                            newSystem = ((SystemHeatExchanger)systemToDuplicate).Duplicate();
                            ProjectInfoBLL objProjectInfoBll = new ProjectInfoBLL();
                            bool status = objProjectInfoBll.UpdateProject(Project.CurrentProject);
                            Application.Current.Properties["ProjectId"] = Project.CurrentProject.projectID;
                        }
                        else
                        {
                            JCHMessageBox.Show("System doesn’t allow user to duplicate system unless validation is not complete");
                            return;
                        }
                        Project = JCHVRF.Model.Project.CurrentProject;
                        break;

                    case 6:
                        if (systemToDuplicate.SystemStatus == JCHVRF_New.Model.SystemStatus.VALID)
                        {
                            newSystem = ((ControlSystem)systemToDuplicate).Duplicate();
                            ControlGroup _group = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID.Equals(_currentSystem.Id));
                            var controller = Project.ControllerList.FindAll(x => x.ControlGroupID.Equals(systemToDuplicate.Id));

                            Project.GetProjectInstance.SelectedSystemID = newSystem.Id;
                            ProjectInfoBLL objProjectInfoBll = new ProjectInfoBLL();
                            objProjectInfoBll.UpdateProject(Project.CurrentProject);
                            Application.Current.Properties["ProjectId"] = Project.CurrentProject.projectID;
                        }
                        else
                        {
                            JCHMessageBox.Show("System doesn’t allow user to duplicate system unless validation is not complete");
                            return;
                        }
                        Project = JCHVRF.Model.Project.CurrentProject;

                        break;
                }

                _eventAggregator.GetEvent<SystemCreated>().Publish(newSystem);                
                _eventAggregator.GetEvent<RefreshSystems>().Publish();

            }
            catch (Exception ex)
            {
                int? id = Project?.projectID;
                Logger.LogProjectError(id, ex); //if Id not available/present pass null. ex : Logger.LogProjectError(null, ex); 
            }

        }

        private void PublishDuplicationEvents(SystemBase newSystem, NextGenModel.SystemVRF systemToDupliate)
        {
            if (newSystem.HvacSystemType == "1")
            {
               // _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Publish((NextGenModel.SystemVRF)newSystem);

                _eventAggregator.GetEvent<SystemDuplicateEvent>().Publish(new DuplicatedEventParams {
                    NewSystem = (NextGenModel.SystemVRF)newSystem,
                    OldSystem = (NextGenModel.SystemVRF)systemToDupliate });
                
                //_eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish((JCHVRF.Model.NextGen.SystemVRF)newSystem);

                //_eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Publish((JCHVRF.Model.NextGen.SystemVRF)newSystem);

            }
        }

        private void PublishDuplicationEventWithoutValidate(SystemBase newSystem, NextGenModel.SystemVRF systemToDupliate)
        {
            if (newSystem.HvacSystemType == "1")
            {
                _eventAggregator.GetEvent<SystemDuplicateEvent>().Publish(new DuplicatedEventParams {
                    NewSystem = (NextGenModel.SystemVRF)newSystem,
                    OldSystem = (NextGenModel.SystemVRF)systemToDupliate });

                //_eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish((JCHVRF.Model.NextGen.SystemVRF)newSystem);

            }
        }

        public void DrawItemRecursion(Graphics g, WL.Item item, string WiringImageDir)
        {
            if (item is WL.Node)
            {
                if (item is NextGenModel.MyNodeGround_Wiring)
                {
                    NextGenModel.MyNodeGround_Wiring ndGnd = item as NextGenModel.MyNodeGround_Wiring;
                    string nodePointsFile = Path.Combine(WiringImageDir, "Ground.txt");
                    DrawNode_wiring(g, ndGnd, nodePointsFile, "", null);
                }
                else
                {
                    WL.Node nd = item as WL.Node;
                    DrawNode(g, nd);
                    if (nd.AddFlow != null)
                    {
                        if (nd.AddFlow.Children != null)
                        {
                            foreach (WL.Item child in nd.AddFlow.Children)
                            {
                                DrawItemRecursion(g, child, WiringImageDir);
                            }
                        }
                    }
                }
            }
            else if (item is WL.Link)
            {
                DrawLine(g, item as WL.Link);
            }

        }

        public void DrawNode(Graphics g, WL.Node nd)
        {
            if (nd == null) return;

            SizeF sf = new SizeF((float)nd.Bounds.Width, (float)nd.Bounds.Height);

            RectangleF rect = new RectangleF(utilWiring.convertSystemPointToDrawingPoint(nd.Location), sf);
            string text = nd.Text;

            pt.Media.SolidColorBrush newBrush = (pt.Media.SolidColorBrush)nd.Stroke;
            Color clr = System.Drawing.Color.FromArgb(newBrush.Color.A, newBrush.Color.R, newBrush.Color.G, newBrush.Color.B);

            if (newBrush.Color.R != 255 || newBrush.Color.G != 255 || newBrush.Color.B != 255)
            {
                Pen pen = new Pen(clr, (float)(0.3f));
                if (nd.Geometry is System.Windows.Media.RectangleGeometry)
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }

            pt.Media.SolidColorBrush newFill = (pt.Media.SolidColorBrush)nd.Fill;
            if (newFill.Color.R != 255 || newFill.Color.G != 255 || newFill.Color.B != 255)
            {
                Color clr1 = System.Drawing.Color.FromArgb(newFill.Color.A, newFill.Color.R, newFill.Color.G, newFill.Color.B);
                Brush brush = new SolidBrush(clr1);
                g.FillRectangle(brush, rect);
            }
            if (!string.IsNullOrEmpty(text))
            {
                Font ft1;

                if (nd.FontSize > 8f)
                {
                    ft1 = new Font("Arial", 8f, GraphicsUnit.Pixel);
                }
                else
                {
                    ft1 = new Font("Arial", 6f, GraphicsUnit.Pixel);
                }
                SizeF textSize = g.MeasureString(text, ft1);
                textSize.Height = textSize.Height * 0.7f;

                float x = 0, y = 0;

                pt.Media.SolidColorBrush textClr = (pt.Media.SolidColorBrush)nd.Foreground;
                Color clr2 = System.Drawing.Color.FromArgb(textClr.Color.A, textClr.Color.R, textClr.Color.G, textClr.Color.B);

                x = rect.X + (rect.Width - textSize.Width) / 2;
                y = rect.Y + (rect.Height - textSize.Height) / 2;

                g.DrawString(text, ft1, new SolidBrush(clr2), x, y);
            }
        }

        public void FillEllipse(Graphics g, Brush brush, RectangleF rect)
        {
            float x = rect.X, y = rect.Y, w = rect.Width, h = rect.Height;
            float w1 = w / h * 2, w2, x1, y1;
            for (int i = 0; i < h / 2; i++)
            {
                w2 = w1 * (i + 1);
                x1 = x + (w - w2) / 2;
                y1 = y + i;
                g.FillRectangle(brush, x1, y1, w2, h - i * 2);
            }
        }

        public void DrawLine(Graphics g, WL.Link line)
        {
            List<PointF> ptl = new List<PointF>();
            foreach (pt.Point p in line.Points)
            {
                ptl.Add(utilWiring.convertSystemPointToDrawingPoint(p));
            }
            if (ptl.Count <= 1)
                return;
            PointF[] pts = ptl.ToArray();
            pt.Media.SolidColorBrush newBrush = (pt.Media.SolidColorBrush)line.Stroke;
            Color clr = System.Drawing.Color.FromArgb(newBrush.Color.A, newBrush.Color.R, newBrush.Color.G, newBrush.Color.B);
            Pen pen = new Pen(clr, 0.1f);
            WL.Node nd = line.Dst;
            if (nd != null && nd is NextGenModel.MyNodeGround_Wiring)
                pen = new Pen(Color.Yellow, 0.1f);
            g.DrawLines(pen, pts);
        }

        public void DrawNode_wiring(Graphics g, WL.Node nd, string nodeTextFile, string name, JCHVRF.MyPipingBLL.NodeElement_Wiring item)
        {
            Font textFont_wiring = new Font("Arial", 8f, System.Drawing.FontStyle.Regular);
            Brush textBrush_wiring = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black, 0.1f);
            if (nd is NextGenModel.MyNodeGround_Wiring)
                pen = new Pen(Color.Black, 0.1f);

            SizeF ndSize = new SizeF((float)nd.Size.Width, (float)nd.Size.Height);
            PointF ndLocation = new PointF((float)nd.Location.X, (float)nd.Location.Y);
            if (nd is NextGenModel.WiringNodeIn)
            {
                ndLocation = new PointF((float)nd.Location.X, (float)nd.Location.Y);
            }

            RectangleF rect = new RectangleF(ndLocation, ndSize);
            InitPointF(nodeTextFile);
            RelocatedNode(rect);

            if (ptStart.Count == ptEnd.Count)
            {
                for (int i = 0; i < ptStart.Count; ++i)
                {
                    g.DrawLine(pen, ptStart[i], ptEnd[i]);
                }
            }
            g.ResetTransform();

            if (item != null && item.PtCircles.Count >= item.UnitCount * 4)
            {
                pt.Point pt = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStr1), utilWiring.convertPointFToWinPoint(ndLocation));
                g.DrawString(item.Str1, textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt));

                for (int i = 0; i < item.UnitCount; ++i)
                {
                    RectangleF r1 = new RectangleF(item.PtCircles[i * 4], item.CircleSize);
                    RelocateLocation(ref r1, ndLocation);
                    RectangleF r2 = new RectangleF(item.PtCircles[i * 4 + 1], item.CircleSize);
                    RelocateLocation(ref r2, ndLocation);
                    RectangleF r3 = new RectangleF(item.PtCircles[i * 4 + 2], item.CircleSize);
                    RelocateLocation(ref r3, ndLocation);
                    RectangleF r4 = new RectangleF(item.PtCircles[i * 4 + 3], item.CircleSize);
                    RelocateLocation(ref r4, ndLocation);
                    g.DrawEllipse(pen, r1);
                    g.DrawEllipse(pen, r2);
                    g.DrawEllipse(pen, r3);
                    g.DrawEllipse(pen, r4);

                    pt.Point pt1 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtModelGroup[i]), utilWiring.convertPointFToWinPoint(ndLocation));

                    g.DrawString(item.ModelGroup[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt1));      // YVOH200
                    pt.Point pt2 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStrGroup1[i]), utilWiring.convertPointFToWinPoint(ndLocation));
                    if (i < 2)
                    {
                        g.DrawString(item.StrGroup1[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt2));    // X Y | X Y
                    }
                    pt.Point pt3 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStrGroup2[i]), utilWiring.convertPointFToWinPoint(ndLocation));
                    g.DrawString(item.StrGroup2[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt3));        // L1L2L3N

                    pt.Point pt4 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStrGroup3[i]), utilWiring.convertPointFToWinPoint(ndLocation));
                    if (item.UnitCount > 1)
                    {
                        g.DrawString(item.StrGroup3[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt4));    // 19A 3Nph
                    }
                }
            }
        }

        private void RelocateLocation(ref RectangleF rectSrc, PointF ptOffset)
        {
            rectSrc.X += ptOffset.X;
            rectSrc.Y += ptOffset.Y;
        }

        private void InitPointF(string filePath)
        {
            ptStart.Clear();
            ptEnd.Clear();

            minx = 100000;
            miny = 100000;
            maxX = 0;
            maxY = 0;

            string s;
            StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default);
            while ((s = sr.ReadLine()) != null)
            {
                string[] ch = { " ", ",", "\t" };
                string[] str = s.Split(ch, System.StringSplitOptions.RemoveEmptyEntries);
                if (str.Length == 4)
                {
                    float x = float.Parse(str[0]);
                    float y = System.Math.Abs(float.Parse(str[1]));
                    minx = minx < x ? minx : x;
                    miny = miny < y ? miny : y;
                    maxX = maxX > x ? maxX : x;
                    maxY = maxY > y ? maxY : y;
                    PointF pts = new PointF(x, y);

                    x = float.Parse(str[2]);
                    y = System.Math.Abs(float.Parse(str[3]));
                    minx = minx < x ? minx : x;
                    miny = miny < y ? miny : y;
                    maxX = maxX > x ? maxX : x;
                    maxY = maxY > y ? maxY : y;
                    PointF pte = new PointF(x, y);

                    ptStart.Add(pts);
                    ptEnd.Add(pte);
                }
            }
        }

        private void RelocatedNode(RectangleF rect)
        {
            SizeF actSize = new SizeF(maxX - minx + 1, maxY - miny - 1);
            if (actSize.Width == 0 || actSize.Height == 0)
                return;
            float scaleX = rect.Width / actSize.Width;
            float scaleY = rect.Height / actSize.Height;
            float scale = scaleX;

            if (ptStart.Count == ptEnd.Count)
            {
                for (int i = 0; i < ptStart.Count; ++i)
                {
                    PointF pts = new PointF((ptStart[i].X - minx) * scale, (ptStart[i].Y - miny) * scale);

                    pts.X += rect.X;
                    pts.Y += rect.Y;
                    ptStart[i] = pts;

                    PointF pte = new PointF((ptEnd[i].X - minx) * scale, (ptEnd[i].Y - miny) * scale);

                    pte.X += rect.X;
                    pte.Y += rect.Y;
                    ptEnd[i] = pte;
                }
            }
        }

        
    }
}