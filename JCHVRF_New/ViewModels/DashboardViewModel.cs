using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF.Model;
using JCBase.UI;
using JCHVRF.VRFMessage;
using JCHVRF.BLL;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF.Model.NextGen;
using System.Collections.ObjectModel;
using JCHVRF_New.Common.Contracts;
using Prism.Events;
using JCHVRF_New.Model;
using JCHVRF.DAL.NextGen;

namespace JCHVRF_New.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        #region Fields
        IGlobalProperties _globalProperties;
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;
        #endregion Fields

        #region Delegate Command
        public DelegateCommand OpenProjectClickCommand { get; set; }

        public DelegateCommand RefreshClickedCommand { get; set; }

        public DelegateCommand ViewAllClickedCommand { get; set; }

        public ObservableCollection<Notification> Notifications { get
            {
                return this._globalProperties.Notifications;
            } }

        public ObservableCollection<DateTime> SignificantDates
        {
            get {
                HashSet<DateTime> dates = new HashSet<DateTime>();
                _eventDAL.GetEvent()?.ForEach((item) =>
                {
                    DateTime day = item.Item1;
                    while (day <= item.Item2)
                    {
                        dates.Add(day.Date);
                        day = day.AddDays(1);
                    }
                });
                return new ObservableCollection<DateTime>(dates);
            }
        }

        #endregion Delegate Command

        private IEventDAL _eventDAL;

        public DashboardViewModel(IRegionManager regionManager, IGlobalProperties globalProperties, IEventAggregator eventAggregator, IEventDAL eventDAL)
        {
            try
            {
                OpenProjectClickCommand = new DelegateCommand(OpenProjectClicked);
                _regionManager = regionManager;
                _globalProperties = globalProperties;
                _eventAggregator = eventAggregator;
                _eventDAL = eventDAL;
                _eventAggregator.GetEvent<GlobalPropertiesUpdated>().Subscribe(OnGlobalPropertyUpdated, ThreadOption.BackgroundThread, false, a => { return (a == nameof(Notifications)); });

                _eventAggregator.GetEvent<ModalWindowClosed>().Subscribe(OnAddEventClosed, ThreadOption.BackgroundThread, false, obj => { return obj == ViewKeys.AddEvent || obj == ViewKeys.EventList; });

                RefreshClickedCommand = new DelegateCommand(RefreshNotifications);
                ViewAllClickedCommand = new DelegateCommand(OnViewAllClicked);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnAddEventClosed(string obj)
        {
            RaisePropertyChanged(nameof(SignificantDates));
        }

        private void OnViewAllClicked()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Settings);
            _eventAggregator.GetEvent<PubSubEvent<ISideBarItem>>().Publish(new SideBarItem("Notifications", language.Current.GetMessage("NOTIFICATIONS"), null));
        }

        private void OnGlobalPropertyUpdated(string propname)
        {
            RaisePropertyChanged(propname);
            RefreshNotifications();
        }

        void RefreshNotifications()
        {
            foreach (var item in Notifications)
            {
                item.RaisePropertyChanged("OccurenceDiffText");
            }
        }

        #region Private Methods
        private void OpenProjectClicked()
        {
            try
            {
                int projectId = JCHVRF.BLL.CommonBLL.ImportVRFProject("VRF");
                if (projectId <= 0)
                {
                    JCHMessageBox.ShowWarning(Msg.IMPORT_PROJECT_FAILED);
                    return;
                }
                else
                {
                    openVRFProject(projectId);
                    JCHMessageBox.Show(string.Format(language.Current.GetMessage("ALERT_PROJECT_OPEN_SUCCESS")));
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
            //    //JCHMessageBox.Show(Msg.GetResourceString("PEOJECT_BRAND_DIFFERENT"));
            //    JCHMessageBox.Show(language.Current.GetMessage("ERROR_IMPORT_PROJECT_DIFFERENT_BRAND"));
            //    return;
            //}

            //TODO : unmaintenable idu odus

            //Project = importProj;
            JCHVRF.Model.Project.CurrentProject = importProj;
            NavigationParameters param = new NavigationParameters();
            param.Add("Project", importProj);

            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, param);

        }

        #endregion Private Methods

    }
}
