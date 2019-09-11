using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JCHVRF_New.ViewModels;
using JCHVRF.BLL;
using System.Collections.ObjectModel;
using JCHVRF_New.Model;
using Prism.Regions;
using Prism.Events;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Common.Contracts;
using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Constants;
using System.Windows.Controls.Primitives;
using JCHVRF.Entity;
using JCHVRF.DAL.NextGen;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        private IGlobalProperties _globalProperties;
        private IModalWindowService _winService;
        private IEventDAL _eventDAL;

        public Dashboard(IRegionManager regionManager, IEventAggregator eventAggregator, IGlobalProperties globalProperties, IModalWindowService winService, IEventDAL eventDAL)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _globalProperties = globalProperties;
            _eventDAL = eventDAL;
            _winService = winService;
            CachTableBLL CommonBll = new CachTableBLL();
            CommonBll.CreateCachTable();
            ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
            vm.LoadProjects("All");
            GridAllProject.DataContext = vm;
            _eventAggregator.GetEvent<RefreshDashboard>().Subscribe(RefreshDashboard);
        }

        private void RefreshDashboard()
        {
            ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
            vm.LoadProjects(String.Empty);
            GridAllProject.DataContext = vm;
            ProjectListTabControl.SelectedIndex = 0;
        }

        private void btnNewProject_Click(object sender, RoutedEventArgs e)
        {
            //var wizard = new MainWindow();
            JCHVRF.Model.Project.CurrentProject = new JCHVRF.Model.Project();
            JCHVRF.Model.Project.CurrentSystemId = null;
            JCHVRF.Model.Project.CurrentProject.RegionCode = JCHVRF.Model.SystemSetting.UserSetting.locationSetting.region;
            JCHVRF.Model.Project.CurrentProject.SubRegionCode = JCHVRF.Model.SystemSetting.UserSetting.locationSetting.subRegion;
            //var wizard = new Views.CreateProjectWizard();
            _winService.ShowView(ViewKeys.CreateProjectWizard, language.Current.GetMessage("NEWPROJECT_CREATE"), null, true, 1080,700);
        }

        private void btnOpenProject_Click(object sender,RoutedEventArgs e)
        {
            try
            {
               
                int projectId = JCHVRF.BLL.CommonBLL.ImportVRFProject("VRF");
                if (projectId <= 0)
                {
                    //JCHMessageBox.ShowWarning(Msg.IMPORT_PROJECT_FAILED);
                    return;
                }
                else
                {
                    openVRFProject(projectId);
                   
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
            //    JCHMessageBox.Show(Msg.GetResourceString("PEOJECT_BRAND_DIFFERENT"));
            //    return;
            //}

            JCHVRF.Model.Project.CurrentProject = importProj;
            NavigationParameters param = new NavigationParameters();
            param.Add("Project", importProj);

            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.Splash, param);
            JCHMessageBox.Show(string.Format(language.Current.GetMessage("ALERT_PROJECT_OPEN_SUCCESS")));

        }

        private void SearchProject()
        {
            if (txtProjectSearch.Text.Trim().Length > 3)
            {
                ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
                vm.LoadProjects("All");
                TabAllProject.IsSelected = true;
                var res = vm.Projects.Where(x => x.ProjectName.IndexOf(txtProjectSearch.Text, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
                vm.Projects = new ObservableCollection<LightProject>(res);
                GridAllProject.DataContext = vm;
            }
            else
            {
                ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
                vm.LoadProjects("All");
                GridAllProject.DataContext = vm;
                TabAllProject.IsSelected = true;
            }
        }
        private void txtProjectSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchProject();
            }
        }
        private void lnkBtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            SearchProject();
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string searchType = "all";
            if (TabWeekProject != null && GridWeekProject != null && TabWeekProject.IsSelected)
            {
                searchType = "week";
                ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
                vm.LoadProjects(searchType);
                GridWeekProject.DataContext = vm;
            }
            else if (TabMonthProject != null && GridMonthProject != null && TabMonthProject.IsSelected)
            {
                searchType = "month";
                ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
                vm.LoadProjects(searchType);
                GridMonthProject.DataContext = vm;
            }
            else if (TabAllProject != null && GridAllProject != null && TabAllProject.IsSelected)
            {
                ProjectTemplateViewModel vm = new ProjectTemplateViewModel(_regionManager, _globalProperties);
                vm.LoadProjects(searchType);
                GridAllProject.DataContext = vm;
            }
        }

        private void btnAddEvent_Click(object sender, RoutedEventArgs e)
        {
            NavigationParameters param = new NavigationParameters();
            _winService.ShowView(ViewKeys.AddEvent, language.Current.GetMessage("ADD_EVENT") , param);
        }

        private void CalendarDayButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Primitives.CalendarDayButton button = sender as System.Windows.Controls.Primitives.CalendarDayButton;
            Event objEvent = new Event();
            DateTime clickedDate = (DateTime)button.DataContext;
            if (_eventDAL.GetEventList(clickedDate).Count > 0)
            {
                if (clickedDate != null)
                {
                    _winService.ShowView(ViewKeys.EventList, "Event List");
                    _eventAggregator.GetEvent<AddEventClickedDate>().Publish(clickedDate.ToString());
                }
            }
            else
            {
                if (clickedDate.Date > DateTime.Now.Date)
                {
                    clickedDate = DateTime.Now;
                }
                NavigationParameters param = new NavigationParameters();
                param.Add("clickedDate", clickedDate);
                _winService.ShowView(ViewKeys.AddEvent, "Edit Event", param);
                //JCHMessageBox.Show("No Event added");
            }
        }
    }
}
