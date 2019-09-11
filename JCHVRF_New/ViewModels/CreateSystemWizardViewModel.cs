/****************************** File Header ******************************\
File Name:	CreateProjectWizardViewModel.cs
Date Created:	2/8/2019
Description:	View Model for CreateProjectWizard View.
\*************************************************************************/

using System;

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
    using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

    public class CreateSystemWizardViewModel : ViewModelBase
    {
        #region Fields
        private int _selectedTabIndex;
        private IEventAggregator _eventAggregator;
        private IProjectInfoBAL _projectBAL;
        #endregion Fields

        #region Properties
        public DelegateCommand<Window> CancelClickCommand { get; private set; }
        public DelegateCommand<Window> CreateClickCommand { get; private set; }
        public DelegateCommand NextClickCommand { get; set; }
        public DelegateCommand PreviousClickCommand { get; set; }

        /// <summary>
        /// Gets or sets the SelectedTabIndex
        /// </summary>
        private Visibility _floorvisibility;
        public Visibility FloorVisibility
        {
            get { return this._floorvisibility; }
            set { this.SetValue(ref _floorvisibility, value); }
        }

        private bool _createbuttonenable;
        public bool CreateButtonEnable
        {
            get { return this._createbuttonenable; }
            set { this.SetValue(ref _createbuttonenable, value); }
        }

        private void GetSetFloorTabVisibility(Visibility floorvisibility)
        {
            FloorVisibility = floorvisibility;
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

        private Visibility _iduVisibility;
        public Visibility IduVisibility
        {
            get { return this._iduVisibility; }
            set { this.SetValue(ref _iduVisibility, value); }
        }

        private void GetSetIduTabVisibility(Visibility iduvisibility)
        {
            IduVisibility = iduvisibility;
        }

        //ACC - RAG START
        //Total Heat Exchange Unit Info Tab binding variable
        /// </summary>
        private Visibility _theuInfoVisibility;
        public Visibility TheuInfoVisibility
        {
            get { return this._theuInfoVisibility; }
            set { this.SetValue(ref _theuInfoVisibility, value); }
        }
        private void GetSetTHEUITabVisibility(Visibility theuInfoVisibility)
        {
            TheuInfoVisibility = theuInfoVisibility;
        }
        //ACC - RAG END
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
        public CreateSystemWizardViewModel(IEventAggregator EventAggregator, IProjectInfoBAL projctInfoBll, IRegionManager _regionManager)
        {
            _eventAggregator = EventAggregator;
            _projectBAL = projctInfoBll;
            _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Subscribe(GetSetFloorTabVisibility);
            _eventAggregator.GetEvent<OduIduVisibility>().Subscribe(GetSetIduTabVisibility);
            //_eventAggregator.GetEvent<RefreshDashboard>().Subscribe(RefreshDashBoard);
            CancelClickCommand = new DelegateCommand<Window>(this.CancelClick);
            CreateClickCommand = new DelegateCommand<Window>(this.CreateClick);
            NextClickCommand = new DelegateCommand(NextClick);
            PreviousClickCommand = new DelegateCommand(PreviousClick);
            this.FloorVisibility = Visibility.Collapsed;
            //ACC - RAG START
            _eventAggregator.GetEvent<TheuInfoVisibility>().Subscribe(GetSetTHEUITabVisibility);
            this.TheuInfoVisibility = Visibility.Collapsed;
            //ACC - RAG END
            _eventAggregator.GetEvent<TypeTabSubscriber>().Subscribe(SetTypeTabValidateValue);
            _eventAggregator.GetEvent<FloorTabSubscriber>().Subscribe(SetFloorTabValidateValue);
            _eventAggregator.GetEvent<OduTabSubscriber>().Subscribe(SetOduTabValidateValue);
            _eventAggregator.GetEvent<IduTabSubscriber>().Subscribe(SetIDUTabValidateValue);
            //ACC - RAG START
            _eventAggregator.GetEvent<NextButtonVisibility>().Subscribe(GetSetNextBtnVisibility);
            this.NextButtonVisibility = Visibility.Collapsed;
            CreateButtonEnable = true;
            _eventAggregator.GetEvent<CreateButtonVisibility>().Subscribe(GetSetCreateBtnVisibility);
            _eventAggregator.GetEvent<CreateButtonEnability>().Subscribe(GetSetCreateBtnEnability);
            this.CreateButtonVisibility = Visibility.Collapsed;
            _eventAggregator.GetEvent<Cleanup>().Subscribe(OnCleanup);
            //ACC - RAG END
            RegionManager = _regionManager;
        }

        private void OnCleanup()
        {
            _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Unsubscribe(GetSetFloorTabVisibility);
            _eventAggregator.GetEvent<OduIduVisibility>().Unsubscribe(GetSetIduTabVisibility);
            _eventAggregator.GetEvent<TheuInfoVisibility>().Unsubscribe(GetSetTHEUITabVisibility);
            _eventAggregator.GetEvent<TypeTabSubscriber>().Unsubscribe(SetTypeTabValidateValue);
            _eventAggregator.GetEvent<FloorTabSubscriber>().Unsubscribe(SetFloorTabValidateValue);
            _eventAggregator.GetEvent<OduTabSubscriber>().Unsubscribe(SetOduTabValidateValue);
            _eventAggregator.GetEvent<IduTabSubscriber>().Unsubscribe(SetIDUTabValidateValue);
            _eventAggregator.GetEvent<NextButtonVisibility>().Unsubscribe(GetSetNextBtnVisibility);
            _eventAggregator.GetEvent<CreateButtonVisibility>().Unsubscribe(GetSetCreateBtnVisibility);
            _eventAggregator.GetEvent<CreateButtonEnability>().Unsubscribe(GetSetCreateBtnEnability);
            _eventAggregator.GetEvent<Cleanup>().Unsubscribe(OnCleanup);
        }
        #endregion

            //TODO to get rid of this selectedTabIndex
            //ACC - RAG START
        private void SelectWizardNextTab()
        {
            int systemId = System.Convert.ToInt32(WorkFlowContext.Systemid);

            if (this.FloorVisibility == Visibility.Collapsed)
            {
                if (this.TheuInfoVisibility == Visibility.Collapsed)
                {
                    if (this.SelectedTabIndex == 0)
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 2;
                        CreateButtonVisibility = Visibility.Collapsed;
                        NextButtonVisibility = Visibility.Visible;
                        return;
                    }
                    if (this.SelectedTabIndex == 2)
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 1;
                        CreateButtonVisibility = Visibility.Visible;
                        NextButtonVisibility = Visibility.Collapsed;
                        return;
                    }
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 4;
                    CreateButtonVisibility = Visibility.Visible;
                    NextButtonVisibility = Visibility.Collapsed;
                    return;
                }
            }
            else
            {
                if (this.SelectedTabIndex != 2)
                {
                    if (systemId == 2)
                    {
                        if (this.SelectedTabIndex == 0)
                        {
                            this.SelectedTabIndex = this.SelectedTabIndex + 1;
                            CreateButtonVisibility = Visibility.Collapsed;
                            NextButtonVisibility = Visibility.Visible;
                            return;
                        }
                        if (this.SelectedTabIndex == 1)
                        {
                            this.SelectedTabIndex = this.SelectedTabIndex + 3;
                            CreateButtonVisibility = Visibility.Visible;
                            NextButtonVisibility = Visibility.Collapsed;
                            return;
                        }
                    }
                    else
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 1;
                        NextButtonVisibility = Visibility.Visible;
                        CreateButtonVisibility = Visibility.Collapsed;
                        return;
                    }
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 1;
                    CreateButtonVisibility = Visibility.Visible;
                    NextButtonVisibility = Visibility.Collapsed;
                    return;
                }
            }
        }
        //ACC - RAG END

        //ACC - RAG START
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
        //ACC - RAG END


        //ACC - RAG START
        //Next Button Visibility Property binding
        private Visibility _nextButtonVisibility;
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

        private void GetSetCreateBtnEnability(bool createButtonEnability)
        {
            CreateButtonEnable = createButtonEnability;
        }

        //ACC - RAG START
        private void SelectWizardPreviousTab()
        {
            if (this.FloorVisibility == Visibility.Collapsed)
            {
                if (this.SelectedTabIndex == 3)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 1;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
                else if (this.SelectedTabIndex == 2 || this.SelectedTabIndex == 4)
                {
                    this.SelectedTabIndex = 0;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
            }
            else
            {
                if (this.SelectedTabIndex == 4)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 3;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 1;
                    CreateButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
            }
        }
        //ACC - RAG END

        private void CancelClick(Window win)
        {
            if (win != null)
            {
                win.Close();
            }
            _eventAggregator.GetEvent<Cleanup>().Publish();

        } 
        //ACC - RAG START
      
        //ACC - RAG END

        //IRegionManager regionManager;
        private void CreateClick(Window win)
        {

            _eventAggregator.GetEvent<BeforeCreate>().Publish();
           
            var proj = Project.GetProjectInstance;
            SystemBase newSystem = WorkFlowContext.NewSystem;

            int sysTypeAsInt = Convert.ToInt32(newSystem.HvacSystemType);
            switch (sysTypeAsInt)
            {
                case 1:
                    proj.SystemListNextGen.Add((JCHVRF.Model.NextGen.SystemVRF)newSystem);
                    _eventAggregator.GetEvent<ErrorMessageUC>().Publish(string.Empty);
                    Views.ucDesignerCanvas.__errorMessage = string.Empty;
                    break;
                case 2:
                    proj.HeatExchangerSystems.Add((SystemHeatExchanger)newSystem);
                    _eventAggregator.GetEvent<ErrorMessageUC>().Publish(string.Empty);
                    Views.ucDesignerCanvas.__errorMessage = string.Empty;
                    break;

                case 6:
                    if((proj.SystemListNextGen.Count<=0)&&(proj.HeatExchangerSystems.Count <= 0))
                    {
                        _eventAggregator.GetEvent<ErrorMessageUC>().Publish(JCHVRF_New.LanguageData.LanguageViewModel.Current.GetMessage("CENTRAL_CONTROLLER_BLANK"));
                        Views.ucDesignerCanvas.__errorMessage = JCHVRF_New.LanguageData.LanguageViewModel.Current.GetMessage("CENTRAL_CONTROLLER_BLANK");
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ErrorMessageUC>().Publish(string.Empty);
                        Views.ucDesignerCanvas.__errorMessage = string.Empty;
                    }
                    proj.ControlSystemList.Add((ControlSystem)newSystem);
                    break;
            }

            if (_projectBAL.UpdateProject(proj))
            {
                Project.GetProjectInstance.SelectedSystemID = newSystem.Id;
                UndoRedoSetup.SetInstanceNull();
                //WorkFlowContext.Clear();
                JCHMessageBox.Show(Langauge.Current.GetMessage("SYSTEM_SAVED_SUCCESSFULLY"));//System Saved Successfully
                if (win != null)
                {
                    var w = Application.Current.MainWindow;
                    w.Hide();
                    //RefreshDashBoard();
                    var projectId = proj.projectID;
                    Application.Current.Properties["ProjectId"] = projectId;
                    ProjectInfoBLL bll = new ProjectInfoBLL();
                    JCHVRF.Entity.ProjectInfo projectNextGen = bll.GetProjectInfo(projectId);
                    projectNextGen.ProjectLegacy.RegionCode = JCHVRF.Model.Project.CurrentProject.RegionCode;
                    projectNextGen.ProjectLegacy.SubRegionCode = JCHVRF.Model.Project.CurrentProject.SubRegionCode;
                    projectNextGen.ProjectLegacy.projectID = projectId;

                    NavigationParameters param = new NavigationParameters();
                    param.Add("Project", projectNextGen.ProjectLegacy);

                    RegionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.MasterDesigner, (a) => { win.Close(); }, param);
                    w.Show();
                }
                //event to trigger to all view models to unsubscribe events they have subscibed to
                _eventAggregator.GetEvent<Cleanup>().Publish();
                _eventAggregator.GetEvent<SystemCreated>().Publish(newSystem);
                _eventAggregator.GetEvent<RefreshSystems>().Publish();
                
            }
        }
        private void NextClick()
        {
            switch (this.SelectedTabIndex)
            {
                //Type Tab
                case 0:
                    _eventAggregator.GetEvent<TypeInfoTabNext>().Publish();
                    if (IsTypeTabvalidateTabinfo)
                        SelectWizardNextTab();
                    break;
                //Floor Tab
                case 1:
                    _eventAggregator.GetEvent<FloorTabNext>().Publish();
                    if (IsFloorTabvalidateinfo)
                    SelectWizardNextTab();
                    break;
                //ODU Tab
                case 2:
                    _eventAggregator.GetEvent<ODUTypeTabNext>().Publish();
                    if (IsOduTabvalidateinfo)
                        SelectWizardNextTab();
                    break;
                //IDU Tab
                case 3:
                    SelectWizardNextTab();
                    break;
                default:
                    break;
            }
        }
        private void PreviousClick()
        {
            //ACC - RAG START
            if (this.SelectedTabIndex <= 2)
            {
                CreateButtonVisibility = Visibility.Collapsed;
                NextButtonVisibility = Visibility.Visible;
            }
            //ACC - RAG END
            SelectWizardPreviousTab();
        }
    }
}
