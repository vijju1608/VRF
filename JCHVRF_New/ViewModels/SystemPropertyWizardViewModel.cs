/****************************** File Header ******************************\
File Name:	SystemPropertyWizardViewModel.cs
Date Created:	7/3/2019
Description:	View Model for  SystemPropertyWizard View
\*************************************************************************/

namespace JCHVRF_New.ViewModels
{
    using JCHVRF.BLL.New;
    using JCHVRF.Model;
    using JCHVRF_New.Common.Constants;
    using JCHVRF_New.Common.Helpers;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Regions;
    using System;
    using System.Windows;
    using NextGenModel = JCHVRF.Model.NextGen;
    using System.Linq;
    using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
    using JCHVRF_New.Utility;

    public class SystemPropertyWizardViewModel : ViewModelBase
    {
        #region Fields
        private int _selectedTabIndex;
        private IEventAggregator _eventAggregator;
        private IProjectInfoBAL _projectBAL;
        #endregion Fields

        #region Properties
        public DelegateCommand<Window> CancelClickCommand { get; private set; }
        public DelegateCommand<Window> SaveClickCommand { get; private set; }
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

        private bool _savebuttonenable;
        public bool SaveButtonEnable
        {
            get { return this._savebuttonenable; }
            set { this.SetValue(ref _savebuttonenable, value); }
        }

        private string _systemName;
        public string SystemName
        {
            get { return _systemName; }
            set { this.SetValue(ref _systemName, value); }
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
        public SystemPropertyWizardViewModel(IEventAggregator EventAggregator, IProjectInfoBAL projctInfoBll, IRegionManager _regionManager)
        {
            try
            {
                _eventAggregator = EventAggregator;
                _projectBAL = projctInfoBll;
                _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Subscribe(GetSetFloorTabVisibility);
                _eventAggregator.GetEvent<OduIduVisibility>().Subscribe(GetSetIduTabVisibility);
                //_eventAggregator.GetEvent<RefreshDashboard>().Subscribe(RefreshDashBoard);           
                CancelClickCommand = new DelegateCommand<Window>(this.CancelClick);
                SaveClickCommand = new DelegateCommand<Window>(this.SaveClick);
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
                _eventAggregator.GetEvent<SaveButton_Visibility>().Subscribe(GetSetSaveBtnVisibility);//Acc IA
                SaveButtonEnable = true;
                _eventAggregator.GetEvent<SaveButtonEnability>().Subscribe(GetSetSaveBtnEnability);
                this.SaveButtonVisibility = Visibility.Collapsed;
                _eventAggregator.GetEvent<CleanupSystemWizard>().Subscribe(OnCleanup);
                //ACC - RAG END
                RegionManager = _regionManager;

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        #endregion
        private void OnCleanup()
        {
            try
            {
                _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Unsubscribe(GetSetFloorTabVisibility);
                _eventAggregator.GetEvent<OduIduVisibility>().Unsubscribe(GetSetIduTabVisibility);
                _eventAggregator.GetEvent<TheuInfoVisibility>().Unsubscribe(GetSetTHEUITabVisibility);
                _eventAggregator.GetEvent<TypeTabSubscriber>().Unsubscribe(SetTypeTabValidateValue);
                _eventAggregator.GetEvent<FloorTabSubscriber>().Unsubscribe(SetFloorTabValidateValue);
                _eventAggregator.GetEvent<OduTabSubscriber>().Unsubscribe(SetOduTabValidateValue);
                _eventAggregator.GetEvent<IduTabSubscriber>().Unsubscribe(SetIDUTabValidateValue);
                _eventAggregator.GetEvent<NextButtonVisibility>().Unsubscribe(GetSetNextBtnVisibility);
                _eventAggregator.GetEvent<SaveButton_Visibility>().Unsubscribe(GetSetSaveBtnVisibility);
                _eventAggregator.GetEvent<SaveButtonEnability>().Unsubscribe(GetSetSaveBtnEnability);

                _eventAggregator.GetEvent<CleanupSystemWizard>().Unsubscribe(OnCleanup);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void GetSetSaveBtnEnability(bool saveButtonEnability)
        {
            SaveButtonEnable = saveButtonEnability;
        }

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
                        SaveButtonVisibility = Visibility.Collapsed;
                        NextButtonVisibility = Visibility.Visible;
                        return;
                    }
                    if (this.SelectedTabIndex == 2)
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 1;
                        SaveButtonVisibility = Visibility.Visible;
                        NextButtonVisibility = Visibility.Collapsed;
                        return;
                    }
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 4;
                    SaveButtonVisibility = Visibility.Visible;
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
                            SaveButtonVisibility = Visibility.Collapsed;
                            NextButtonVisibility = Visibility.Visible;
                            return;
                        }
                        if (this.SelectedTabIndex == 1)
                        {
                            this.SelectedTabIndex = this.SelectedTabIndex + 3;
                            SaveButtonVisibility = Visibility.Visible;
                            NextButtonVisibility = Visibility.Collapsed;
                            return;
                        }
                    }
                    else
                    {
                        this.SelectedTabIndex = this.SelectedTabIndex + 1;
                        NextButtonVisibility = Visibility.Visible;
                        SaveButtonVisibility = Visibility.Collapsed;
                        return;
                    }
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex + 1;
                    SaveButtonVisibility = Visibility.Visible;
                    NextButtonVisibility = Visibility.Collapsed;
                    return;
                }
            }
        }
        //ACC - RAG END

        //ACC - RAG START
        private void SelectWizardPreviousTab()
        {
            if (this.FloorVisibility == Visibility.Collapsed)
            {
                if (this.SelectedTabIndex == 3)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 1;
                    SaveButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
                else if (this.SelectedTabIndex == 2 || this.SelectedTabIndex == 4)
                {
                    this.SelectedTabIndex = 0;
                    SaveButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
            }
            else
            {
                if (this.SelectedTabIndex == 4)
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 3;
                    SaveButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
                else
                {
                    this.SelectedTabIndex = this.SelectedTabIndex - 1;
                    SaveButtonVisibility = Visibility.Collapsed;
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
            MasterDesignerViewModel._isSysProp = false;
            MasterDesignerViewModel._isSysPropFA = false;
        }

        //IRegionManager regionManager;
        //
        private void SaveClick(Window win)
        {
            try
            {
                var proj = Project.GetProjectInstance;
                int sysTypeAsInt = System.Convert.ToInt32(_hvacSystem.HvacSystemType);
                NextGenModel.SystemVRF currentSystem = new NextGenModel.SystemVRF();
                switch (sysTypeAsInt)
                {
                    case 1:
                        proj.SystemListNextGen = JCHVRF.Model.Project.CurrentProject.SystemListNextGen;
                        currentSystem = proj.SystemListNextGen.Find(sys => sys.Id == JCHVRF.Model.Project.CurrentSystemId);
                        _eventAggregator.GetEvent<BeforeSaveVRF>().Publish(currentSystem);
                        if (this.SelectedTabIndex!=0)
                        {
                            _eventAggregator.GetEvent<ODUTypeTabSave>().Publish();
                            if (!currentSystem.IsOutDoorUpdated)
                                return;
                        }
                        else {
                            currentSystem.IsOutDoorUpdated = false;
                        }
                        UtilTrace.SaveHistoryTraces();
                        //proj.CanvasODUList = null;
                        break;
                    case 2:
                        _eventAggregator.GetEvent<BeforeHESave>().Publish();
                        int k = proj.HeatExchangerSystems.IndexOf((SystemHeatExchanger)_hvacSystem);
                        proj.HeatExchangerSystems[k] = ((SystemHeatExchanger)_hvacSystem);
                        if(PropertyInfoViewModel._strHEName!=null)
                        {
                            if(proj.HeatExchangerSystems[k].Id.Equals(PropertyInfoViewModel._strHEName[0]))
                                proj.HeatExchangerSystems[k].Name = PropertyInfoViewModel._strHEName[1];
                        }

                        UtilTrace.SaveHistoryTraces();
                        break;
                    case 6:
                        _eventAggregator.GetEvent<BeforeSave>().Publish();
                        int j = proj.ControlSystemList.IndexOf((ControlSystem)_hvacSystem);
                        proj.ControlSystemList[j] = ((ControlSystem)_hvacSystem);
                        UtilTrace.SaveHistoryTraces();
                        break;
                }

                if (_projectBAL.UpdateProject(proj))
                {
                    _eventAggregator.GetEvent<RefreshSystems>().Publish();
                    if (sysTypeAsInt == 1)
                    {
                        HeatExchangerCanvasEquipmentViewModel.IsPropOrNewExch = false;
                        if (currentSystem.IsOutDoorUpdated)
                        {
                            bool IsSystemValidated = ((NextGenModel.SystemVRF)_hvacSystem).IsPipingOK;
                            JCHMessageBox.Show(Langauge.Current.GetMessage("SYSTEM_SAVED_SUCCESSFULLY")); //"System Saved Successfully");
                            _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Publish((NextGenModel.SystemVRF)_hvacSystem);
                       
                            if(IsSystemValidated==true)
                            {
                                currentSystem.IsPipingOK = true;
                                _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Publish((NextGenModel.SystemVRF)_hvacSystem);
                            }
                            
                        }
                        else
                        {
                            JCHMessageBox.Show(Langauge.Current.GetMessage("SYSTEM_SAVED_SUCCESSFULLY")); //"System Saved Successfully");
                        }
                    }
                    else
                    {
                        JCHMessageBox.Show(Langauge.Current.GetMessage("SYSTEM_SAVED_SUCCESSFULLY"));// "System Saved Successfully");
                    }
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
                        _eventAggregator.GetEvent<CleanupSystemWizard>().Publish();
                        //RegionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, (a) => { win.Close(); }, param);
                        RegionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.MasterDesigner, (a) => { win.Close(); }, param);
                        w.Show();

                    }
                } 
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        //ACC - IA START
        //Save Button Visibility Property binding
        private Visibility _SaveButtonVisibility;
        public Visibility SaveButtonVisibility
        {
            get { return WorkFlowContext.CurrentSystem.HvacSystemType == "1" ? Visibility.Visible : this._SaveButtonVisibility; }
            set
            {
                this.SetValue(ref _SaveButtonVisibility, value);
                RaisePropertyChanged();
            }
        }
        private void GetSetSaveBtnVisibility(Visibility savebuttonvisibility)
        {
            SaveButtonVisibility = savebuttonvisibility;
        }
        //ACC - IA END


        //ACC - RAG START
        //Next Button Visibility Property binding
        private Visibility _nextButtonVisibility;
        public Visibility NextButtonVisibility
        {
            get { return this._nextButtonVisibility; }
            set { this.SetValue(ref _nextButtonVisibility, value); }
        }

        private SystemBase _hvacSystem;

        public SystemBase SelectedHvacSystem
        {
            get { return _hvacSystem; }
            set
            {
                SetValue(ref _hvacSystem, value);
            }
        }

        private void GetSetNextBtnVisibility(Visibility nextButtonVisibility)
        {
            NextButtonVisibility = nextButtonVisibility;
        }
        //ACC - RAG END

        private void NextClick()
        {
            try
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

                    //Room Tab

                    case 2:

                    //SelectWizardNextTab();

                    //break;

                    //ODU Tab

                    case 3:

                        _eventAggregator.GetEvent<ODUTypeTabNext>().Publish();

                        SelectWizardNextTab();

                        //if (Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(x => x.Id == Project.CurrentSystemId).IsOutDoorUpdated)

                        //{

                        //    SelectWizardNextTab();

                        //}

                        break;

                    //IDU Tab

                    case 4:

                        SelectWizardNextTab();

                        break;

                    default:

                        break;

                }

            }

            catch (Exception ex)
            {

                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);

            }

        }


        private void PreviousClick()
        {
            try
            {
                //ACC - RAG START
                if (this.SelectedTabIndex <= 2)
                {
                    SaveButtonVisibility = Visibility.Collapsed;
                    NextButtonVisibility = Visibility.Visible;
                }
                //ACC - RAG END
                SelectWizardPreviousTab();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
    }
}
