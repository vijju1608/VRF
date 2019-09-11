using JCBase.UI;
using JCHVRF.BLL;
using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF.MyPipingBLL;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class NotificationsBarViewModel : ViewModelBase
    {
        public DelegateCommand OpenProjectClickCommand { get; set; }
        public DelegateCommand SaveProjectClickCommand { get; set; }
        public DelegateCommand HomeClickCommand { get; set; }

        private bool _isContextMenuOpened;
                
        public bool IsContextMenuOpened
        {
            get { return this._isContextMenuOpened; }
            set { this.SetValue(ref _isContextMenuOpened, value); }
        }

        IRegionManager regionManager;
        public string ProjectTitle
        {
            get { return this._globalProperties.ProjectTitle; }
        }


        #region ViewModel Properties

        private JCHVRF.Model.Project _project;
        private IGlobalProperties _globalProperties;
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;

        public JCHVRF.Model.Project Project
        {
            get { return _project; }
            set { this.SetValue(ref _project, value); }
        }
        #endregion

        string GetUserName()
        {
            string name = Environment.UserName;
            StringBuilder actualName = new StringBuilder();
            if (name != null && name.Length > 0)
            {
                actualName.Append(char.ToUpper(name[0]));
                for (int i = 1; i < name.Length; i++)
                {
                    if (char.IsUpper(name[i]))
                    {
                        actualName.Append(" ");
                    }
                    actualName.Append(name[i]);

                }
            }
            return actualName.ToString();
        }

        public string UserName
        {
            get
            {
                return GetUserName();
            }
        }

        public DelegateCommand UserNameClickedCommand { get; private set; }

        public NotificationsBarViewModel(IRegionManager regionManager,IEventAggregator eventAggregator, IGlobalProperties globalProperties)
        {
            try
            {
                this.regionManager = regionManager;
                this._globalProperties = globalProperties;
                this._eventAggregator = eventAggregator;
                this._regionManager = regionManager;
                _eventAggregator.GetEvent<GlobalPropertiesUpdated>().Subscribe(RaisePropertyChanged, ThreadOption.BackgroundThread, false, a => { return (a == nameof(ProjectTitle)); });

                OpenProjectClickCommand = new DelegateCommand(OpenProjectClicked);
                SaveProjectClickCommand = new DelegateCommand(SaveProjectClicked);
                HomeClickCommand = new DelegateCommand(OnHomeClicked);
                UserNameClickedCommand = new DelegateCommand(()=> {                    
                    IsContextMenuOpened = !IsContextMenuOpened;
                });
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnHomeClicked()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Settings);
        }

        private void SaveProjectClicked()
        {
            try
            {
                bool isActive = regionManager.Regions[RegionNames.ContentRegion].ActiveViews.Select(a => a.ToString()?.Split('.').Last()).Contains("MasterDesigner");
                if (isActive)
                {
                    Project = Project.GetProjectInstance;
                    DoSavePipingTempNodeOutStructure();
                    ProjectInfoBLL objProjectInfoBll = new ProjectInfoBLL();
                    bool status = objProjectInfoBll.UpdateProject(Project);
                    System.Windows.Application.Current.Properties["ProjectId"] = Project.projectID;
                    JCHMessageBox.Show(Langauge.Current.GetMessage("SAVED_SUCCESSFULLY"));
                }
                else
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_SAVE_PROJECT"));
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
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
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private PipingBLL GetPipingBLLInstance()
        {
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            bool isInch = CommonBLL.IsDimension_inch();
            UtilPiping utilPiping = new UtilPiping();
            return new PipingBLL(Project, utilPiping, null, isInch, ut_weight, ut_length, ut_power);
        }
        private void OpenProjectClicked()
        {
            try
            {
                int projectId = CommonBLL.ImportVRFProject("VRF");
                if (projectId <= 0)
                {
                    JCHMessageBox.ShowWarning(Langauge.Current.GetMessage("ALERT_PROJECT_IMPORT"));
                    return;
                }
                else
                {
                    openVRFProject(projectId);
                    JCHMessageBox.Show(string.Format(Langauge.Current.GetMessage("ALERT_PROJECT_OPEN_SUCCESS")));
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }



        private void openVRFProject(int projectId)
        {
            Project importProj = CommonBLL.OpenVRFProject(projectId);
            Project currentProject = JCHVRF.Model.Project.GetProjectInstance;
            //if (currentProject != null && importProj.BrandCode != currentProject.BrandCode)
            //{
            //    JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_IMPORT_PROJECT_DIFFERENT_BRAND"));
            //    return;
            //}
            //Project = importProj;
            JCHVRF.Model.Project.CurrentProject = importProj;
            NavigationParameters param = new NavigationParameters();
            param.Add("Project", importProj);

            regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, param);
            
        }
    }
}
