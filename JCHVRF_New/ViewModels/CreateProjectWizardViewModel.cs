/****************************** File Header ******************************\
File Name:	CreateProjectWizardViewModel.cs
Date Created:	2/8/2019
Description:	View Model for CreateProjectWizard View.
\*************************************************************************/

using JCHVRF.Entity;

namespace JCHVRF_New.ViewModels
{
    using JCHVRF.BLL.New;
    using JCHVRF.Model;
    using JCHVRF_New.Common.Constants;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Utility;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Regions;
    using System.Windows;
    using System;
    using JCHVRF_New.Common.Contracts;
    using Language = JCHVRF_New.LanguageData.LanguageViewModel;
    public class CreateProjectWizardViewModel : ViewModelBase
    {
        #region Fields
        private int _selectedTabIndex;
        private IEventAggregator _eventAggregator;
        private IProjectInfoBAL _projectBAL;
        #endregion Fields
        string systemTypeSelected = null;

        #region Properties
        public DelegateCommand CancelClickCommand { get; private set; }
        public DelegateCommand CreateClickCommand { get; private set; }
        public DelegateCommand NextClickCommand { get; set; }
        public DelegateCommand PreviousClickCommand { get; set; }

        /// <summary>
        /// Gets or sets the SelectedTabIndex
        /// </summary>
        /// 
        
        private bool _createbuttonenable;
        public bool CreateButtonEnable
        {
            get { return this._createbuttonenable; }
            set { this.SetValue(ref _createbuttonenable, value); }
        }

        private Visibility _floorvisibility;
        public Visibility FloorVisibility
        {
            get { return this._floorvisibility; }
            set { this.SetValue(ref _floorvisibility, value); }
        }

        private void GetSetFloorTabVisibility(Visibility floorvisibility)
        {
            FloorVisibility = floorvisibility;
        }
        private Visibility _theuInfoVisibility=Visibility.Collapsed;
        public Visibility TheuInfoVisibility
        {
            get { return this._theuInfoVisibility; }
            set { this.SetValue(ref _theuInfoVisibility, value); }
        }
        private void GetSetTHEUITabVisibility(Visibility theuInfoVisibility)
        {
            TheuInfoVisibility = theuInfoVisibility;
        }
        bool IsProjectInfovalidateTabinfo = false;
        private void SetProjectInfoValidateValue(bool IsvalidateTabinfoReturn)
        {
            IsProjectInfovalidateTabinfo = IsvalidateTabinfoReturn;
        }
        bool IsDesignTabvalidateTabinfo = false;
        private void SetDesignTabValidateValue(bool IsvalidateTabinfoReturn)
        {
            IsDesignTabvalidateTabinfo = IsvalidateTabinfoReturn;
        }

        bool IsTypeTabvalidateTabinfo = false;
        private void SetTypeTabValidateValue(bool IsvalidateTabinfoReturn)
        {
            IsTypeTabvalidateTabinfo = IsvalidateTabinfoReturn;
        }

        bool IsFloorTabvalidateinfo = false;
        private void SetFloorTabValidateValue(bool IsvalidateTabinfoReturn)
        {
            IsFloorTabvalidateinfo = IsvalidateTabinfoReturn;
        }

        bool IsOduTabvalidateinfo = false;
        private void SetOduTabValidateValue(bool IsvalidateTabinfoReturn)
        {
            IsOduTabvalidateinfo = IsvalidateTabinfoReturn;
        }

        bool IsIDUTabvalidateinfo = false;
        private void SetIDUTabValidateValue(bool IsvalidateTabinfoReturn)
        {
            IsIDUTabvalidateinfo = IsvalidateTabinfoReturn;
        }

        private Visibility _iduVisibility = Visibility.Collapsed;
        public Visibility IduVisibility
        {
            get { return this._iduVisibility; }
            set { this.SetValue(ref _iduVisibility, value); }
        }

        private void GetSetIduTabVisibility(Visibility iduvisibility)
        {
            IduVisibility = iduvisibility;
        }

        /// <summary>
        /// Gets or sets the SelectedTabIndex
        /// </summary>
        public int SelectedTabIndex
        {
            get { return this._selectedTabIndex; }
            set { this.SetValue(ref _selectedTabIndex, value); }
        }

        public IRegionManager RegionManager { get; set; }


        #endregion Properties

        #region Constructors
        public CreateProjectWizardViewModel(IEventAggregator EventAggregator, IProjectInfoBAL projctInfoBll, IRegionManager _regionManager, IGlobalProperties globalProperties, IModalWindowService winService)
        {
            _eventAggregator = EventAggregator;
            _projectBAL = projctInfoBll;
            _globalProperties = globalProperties;
            _winService = winService;
            _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Subscribe(GetSetFloorTabVisibility);
            _eventAggregator.GetEvent<OduIduVisibility>().Subscribe(GetSetIduTabVisibility);
            _eventAggregator.GetEvent<TheuInfoVisibility>().Subscribe(GetSetTHEUITabVisibility);  //ACC - RAG 
            //_eventAggregator.GetEvent<RefreshDashboard>().Subscribe(RefreshDashBoard);
            CancelClickCommand = new DelegateCommand(this.CancelClick);
            CreateClickCommand = new DelegateCommand(this.CreateClick);
            NextClickCommand = new DelegateCommand(NextClick);
            PreviousClickCommand = new DelegateCommand(PreviousClick);
            this.FloorVisibility = Visibility.Collapsed;
            this.TheuInfoVisibility = Visibility.Collapsed;  //ACC - RAG 
            _eventAggregator.GetEvent<ProjectInfoSubscriber>().Subscribe(SetProjectInfoValidateValue);
            _eventAggregator.GetEvent<DesignerTabSubscriber>().Subscribe(SetDesignTabValidateValue);
            _eventAggregator.GetEvent<TypeTabSubscriber>().Subscribe(SetTypeTabValidateValue);
            _eventAggregator.GetEvent<FloorTabSubscriber>().Subscribe(SetFloorTabValidateValue);
            _eventAggregator.GetEvent<NextButtonVisibility>().Subscribe(GetSetNextBtnVisibility); //ACC - RAG
            this.NextButtonVisibility = Visibility.Visible; //ACC - RAG
            _eventAggregator.GetEvent<OduTabSubscriber>().Subscribe(SetOduTabValidateValue);
            _eventAggregator.GetEvent<IduTabSubscriber>().Subscribe(SetIDUTabValidateValue);
            _eventAggregator.GetEvent<CreateButtonVisibility>().Subscribe(GetSetCreateBtnVisibility); //ACC - RAG
            this.CreateButtonVisibility = Visibility.Collapsed; //ACC - RAG
            CreateButtonEnable = true;
            _eventAggregator.GetEvent<CreateButtonEnability>().Subscribe(GetSetCreateBtnEnability);
            _eventAggregator.GetEvent<Cleanup>().Subscribe(OnCleanup);
            RegionManager = _regionManager;
            Project.isSystemPropertyWiz = false;
        }

        #endregion

        private void OnCleanup()
        {
            _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Unsubscribe(GetSetFloorTabVisibility);
            _eventAggregator.GetEvent<OduIduVisibility>().Unsubscribe(GetSetIduTabVisibility);
            _eventAggregator.GetEvent<TheuInfoVisibility>().Unsubscribe(GetSetTHEUITabVisibility);
            _eventAggregator.GetEvent<ProjectInfoSubscriber>().Unsubscribe(SetProjectInfoValidateValue);
            _eventAggregator.GetEvent<DesignerTabSubscriber>().Unsubscribe(SetDesignTabValidateValue);
            _eventAggregator.GetEvent<TypeTabSubscriber>().Unsubscribe(SetTypeTabValidateValue);
            _eventAggregator.GetEvent<FloorTabSubscriber>().Unsubscribe(SetFloorTabValidateValue);
            _eventAggregator.GetEvent<NextButtonVisibility>().Unsubscribe(GetSetNextBtnVisibility);
            _eventAggregator.GetEvent<OduTabSubscriber>().Unsubscribe(SetOduTabValidateValue);
            _eventAggregator.GetEvent<IduTabSubscriber>().Unsubscribe(SetIDUTabValidateValue);
            _eventAggregator.GetEvent<CreateButtonVisibility>().Unsubscribe(GetSetCreateBtnVisibility);
            _eventAggregator.GetEvent<CreateButtonEnability>().Unsubscribe(GetSetCreateBtnEnability);
            _eventAggregator.GetEvent<Cleanup>().Unsubscribe(OnCleanup);
        }

        private void RefreshDashBoard()
        {
            _eventAggregator.GetEvent<RefreshDashboard>().Publish();
        }

        private void SelectWizardNextTab()
        {
            if (this.SelectedTabIndex == 2)
            {
                if (this.FloorVisibility == Visibility.Collapsed)
                {
                    if (this.TheuInfoVisibility == Visibility.Collapsed)
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 2;
                        return;
                    }
                    else
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 4;
                        CreateButtonVisibility = Visibility.Visible;
                        NextButtonVisibility = Visibility.Collapsed;
                        return;
                    }
                }
                else if (this.FloorVisibility == Visibility.Visible)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 1;
                    NextButtonVisibility = Visibility.Visible;
                    return;
                }
            }
            else
            {
                if (this.TheuInfoVisibility == Visibility.Visible && this.SelectedTabIndex >= 2) //Accord - Bug fix
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 3;
                    CreateButtonVisibility = Visibility.Visible;
                    NextButtonVisibility = Visibility.Collapsed;
                    return;
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 1;
                    if (this.SelectedTabIndex <= 2)
                    {
                        CreateButtonVisibility = Visibility.Collapsed;
                        NextButtonVisibility = Visibility.Visible;
                        return;
                    }
                    else
                    {
                        if (this.SelectedTabIndex == 4)
                        {
                            CreateButtonVisibility = Visibility.Collapsed;
                            NextButtonVisibility = Visibility.Visible;
                            return;
                        }
                        else
                        {
                            CreateButtonVisibility = Visibility.Visible;
                            NextButtonVisibility = Visibility.Collapsed;
                            return;
                        }
                    }
                }
            }
        }


        private void SelectWizardPreviousTab()
        {
            if (this.FloorVisibility == Visibility.Collapsed)
            {
                if (this.SelectedTabIndex == 4)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 2;
                    return;
                }
                else if (this.SelectedTabIndex == 6)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 4;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                    return;
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 1;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                    return;
                }
            }
            else
            {
                if (this.SelectedTabIndex == 6)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 3;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                    return;
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 1;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                    return;
                }
            }
        }


        //Create Button Visibility Property binding
        private Visibility _createButtonVisibility;
        public Visibility CreateButtonVisibility
        {
            get { return this._createButtonVisibility; }
            set { this.SetValue(ref _createButtonVisibility, value); }
        }
        private void GetSetCreateBtnVisibility(Visibility createButtonVisibility)
        {
            CreateButtonVisibility = createButtonVisibility;
        }

        private void GetSetCreateBtnEnability(bool createButtonEnability)
        {
            CreateButtonEnable = createButtonEnability;
        }

        //Next Button Visibility Property binding
        private Visibility _nextButtonVisibility;
        private IGlobalProperties _globalProperties;
        private IModalWindowService _winService;

        public Visibility NextButtonVisibility
        {
            get { return this._nextButtonVisibility; }
            set { this.SetValue(ref _nextButtonVisibility, value); }
        }
        private void GetSetNextBtnVisibility(Visibility nextButtonVisibility)
        {
            NextButtonVisibility = nextButtonVisibility;
        }
        //ACC - RAG END

        private void CancelClick()
        {
            _winService.Close("CreateProjectWizard");
            _eventAggregator.GetEvent<Cleanup>().Publish();
        }
       
      
        //ACC - RAG END

        //IRegionManager regionManager;
        private void CreateClick()
        {
            var proj = JCHVRF.Model.Project.GetProjectInstance;

            _eventAggregator.GetEvent<BeforeCreate>().Publish();

            SystemBase newSystem = WorkFlowContext.NewSystem;
           
            int sysTypeAsInt = System.Convert.ToInt32(newSystem.HvacSystemType);
            switch (sysTypeAsInt)
            {
                case 1:
                    proj.SystemListNextGen.Add((JCHVRF.Model.NextGen.SystemVRF)newSystem);                   
                    break;
                case 2:
                    proj.HeatExchangerSystems.Add((SystemHeatExchanger)newSystem);                    
                    break;

                case 6:
                    proj.ControlSystemList.Add((ControlSystem)newSystem);
                    break;
            }


            if (_projectBAL.CreateProject(proj))
            {
                    var w = Application.Current.MainWindow;

                    RefreshDashBoard();
                    var projectId = proj.projectID;
                    int projectid = (int)projectId;
                    Application.Current.Properties["ProjectId"] = projectId;
                    ProjectInfoBLL bll = new ProjectInfoBLL();
                    JCHVRF.Entity.ProjectInfo projectNextGen = JCHVRF.Entity.ProjectInfo.create();
                    projectNextGen = bll.GetProjectInfo(projectId);


                    if (projectNextGen != null)
                    {
                        

                        if (WorkFlowContext.Systemid == "2")
                        {
                            if (TotalHeatExUnitInfoViewModel.flgValidateUnit == false)
                            {
                            //JCHMessageBox.Show("Unit name of Heat Exchanger can not be blank");
                            //---------------- Below code added for multi-language----------//
                                 JCHMessageBox.Show(Language.Current.GetMessage("ALERT_HEATEXCHANGER_UNITNAME"));                              
                                return;
                            }

                            //JCHMessageBox.Show("Heat Exchanger has no system available for connection");
                            //---------------- Below code added for multi-language----------//
                            JCHMessageBox.Show(Language.Current.GetMessage("ALERT_EXCHANGER_NO_SYSTEM"));
                        }

                        else if (WorkFlowContext.Systemid == "6")
                        {
                        //JCHMessageBox.Show("Central Controller has no system available for connection");
                        //---------------- Below code added for multi-language----------//

                        _eventAggregator.GetEvent<ErrorMessageUC>().Publish(Language.Current.GetMessage( "CENTRAL_CONTROLLER_BLANK"));
                        Views.ucDesignerCanvas.__errorMessage = Language.Current.GetMessage("CENTRAL_CONTROLLER_BLANK");
                         //   JCHMessageBox.Show(Language.Current.GetMessage("ALERT_CENTRALCONTROLLER_NO_SYSTEM"));
                    }
                    else
                        {
                             //  JCHMessageBox.Show("Project Saved Successfully");
                            //---------------- Below code added for multi-language----------//
                            JCHMessageBox.Show(Language.Current.GetMessage("ALERT_PROJECT_SAVED"));
                        _globalProperties.ProjectTitle = JCHVRF.Model.Project.CurrentProject.Name;
                        _globalProperties.Notifications.Insert(0, new JCHVRF.Model.NextGen.Notification(JCHVRF.Model.NextGen.NotificationType.APPLICATION, String.Format("Project: {0} Created !", _globalProperties.ProjectTitle)));
                        }
                       
                        // ACC - RAG END
                    }
                    
                    projectNextGen.ProjectLegacy.RegionCode = JCHVRF.Model.Project.CurrentProject.RegionCode;
                    projectNextGen.ProjectLegacy.SubRegionCode = JCHVRF.Model.Project.CurrentProject.SubRegionCode;
                    projectNextGen.ProjectLegacy.projectID = projectid;

                    w.Hide();
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Project", projectNextGen.ProjectLegacy);

                    //event to trigger to all view models to unsubscribe events they have subscibed to
                    _eventAggregator.GetEvent<Cleanup>().Publish();
                    //Test this
                    _eventAggregator.GetEvent<CleanupOnClose>().Publish();
                    _eventAggregator.GetEvent<ErrorLogVMClearAll>().Publish();
                    RegionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, (a) => {
                        _winService.Close("CreateProjectWizard");
                    }, param);
                    //RegionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.MasterDesigner, (a) => { win.Close(); }, param);

                    w.Show();

            }
        }      

        private void NextClick()
        {
            switch (this.SelectedTabIndex)
            {
                //ProjectInfo
                case 0:
                    _eventAggregator.GetEvent<ProjectTypeInfoTabNext>().Publish();
                    if (IsProjectInfovalidateTabinfo)
                        SelectWizardNextTab();// 
                    break;
                //Design Conditions Tab
                case 1:
                    _eventAggregator.GetEvent<DesignConditionTabNext>().Publish();
                    if (IsDesignTabvalidateTabinfo)
                        SelectWizardNextTab();
                    break;
                //Type Tab
                case 2:
                    _eventAggregator.GetEvent<TypeInfoTabNext>().Publish();
                    if (IsTypeTabvalidateTabinfo)
                        SelectWizardNextTab();
                    break;
                //Floor Tab
                case 3:
                    _eventAggregator.GetEvent<FloorTabNext>().Publish();
                    if (IsFloorTabvalidateinfo)
                        SelectWizardNextTab();
                    break;
                //ODU Tab
                case 4:
                    _eventAggregator.GetEvent<ODUTypeTabNext>().Publish();
                    if (IsOduTabvalidateinfo)
                        SelectWizardNextTab();
                    break;
                //IDU Tab
                case 5:
                    SelectWizardNextTab();
                    break;
                default:
                    break;
            }
        }
        private void PreviousClick()
        {

            if (this.SelectedTabIndex <= 2)
            {
                CreateButtonVisibility = Visibility.Collapsed;
                NextButtonVisibility = Visibility.Visible;
            }
            SelectWizardPreviousTab();
        }

        #region Properties

        /// <summary>
        /// Gets or sets the SelectedTabIndex
        /// </summary>



        #endregion Properties        
    }
}
