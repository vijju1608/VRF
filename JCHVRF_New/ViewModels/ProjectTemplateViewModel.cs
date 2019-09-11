using JCHVRF.BLL;
using JCHVRF.BLL.New;
using JCHVRF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Model;
using Prism.Commands;
using System.Windows;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;
using language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class ProjectTemplateViewModel
    {
        private IGlobalProperties _globalProperties;
        Random rnd = new Random();
        #pragma warning disable CS0169 // The field 'ProjectTemplateViewModel._regionManager' is never used
        private IRegionManager _regionManager;
        #pragma warning restore CS0169 // The field 'ProjectTemplateViewModel._regionManager' is never used

        public ObservableCollection<LightProject> Projects { get; set; }
        public DelegateCommand<object> EditCommand { get; set; }
        public string SearchType { get; set; }

        public ProjectTemplateViewModel(IRegionManager regionManager, IGlobalProperties globalProperties)
        {
            _globalProperties = globalProperties;
            //_regionManager = regionManager;
            //LoadProjects();
            // AddAccessoryViewModel obj= new AddAccessoryViewModel();

            EditCommand = new DelegateCommand<object>((projectID) =>
            {
                //Button senderButton = sender as Button;
                //var item = senderButton.DataContext;
                //var projectID = ((Project)item).ProjectID;
                int projectId = (int)projectID;
                Application.Current.Properties["ProjectId"] = projectId;
                ProjectInfoBLL bll = new ProjectInfoBLL();

                JCHVRF.Entity.ProjectInfo projectNextGen = bll.GetProjectInfo(projectId);
                if (projectNextGen.ProjectLegacy.RoomList == null)
                {
                    projectNextGen.ProjectLegacy.RoomList = new List<JCHVRF.Model.Room>();
                }
                JCHVRF.Model.Project.CurrentProject = projectNextGen.ProjectLegacy;
                //This case only for old created project 
                if (projectNextGen.ProjectLegacy.projectID.Equals(0))
                {
                    JCHVRF.Model.Project.CurrentProject.projectID = projectId;
                }

                //projectNextGen.ProjectLegacy.RegionCode = "EU_W";
                //projectNextGen.ProjectLegacy.SubRegionCode = "GBR";
                //projectNextGen.ProjectLegacy.projectID = projectId;

                NavigationParameters param = new NavigationParameters();
                param.Add("Project", projectNextGen.ProjectLegacy);
                _globalProperties.ProjectTitle = JCHVRF.Model.Project.CurrentProject.Name;
                regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.MasterDesigner, param);
                //var winMain = new MasterDesigner(projectNextGen.ProjectLegacy);
                //winMain.ShowDialog();
            });

            //SearchType = "all";
            // LoadProjects("All");


        }


        public void LoadProjects(string searchType)
        {
            /// TODO : Check weather region is available or Not, need to notify to do location settings. 
            //if (string.IsNullOrWhiteSpace(JCHVRF.Model.SystemSetting.UserSetting.locationSetting.region))
            //{
            //    return;
            //}
            //else
            //{
            string region = JCHVRF.Model.SystemSetting.UserSetting.locationSetting.region;
            string subRegion = JCHVRF.Model.SystemSetting.UserSetting.locationSetting.subRegion;
            ObservableCollection<LightProject> projects = new ObservableCollection<LightProject> { };
            List<JCHVRF.Entity.ProjectInfo> prjList = new List<JCHVRF.Entity.ProjectInfo>();
            ProjectInfoBLL bll = new ProjectInfoBLL();
            prjList = bll.GetAllProjectsRegionWise(searchType, region, subRegion);
            prjList = prjList.OrderByDescending(mm => mm.LastUpdateDate).ToList();
            foreach (var item in prjList)
            {
                projects.Add(new LightProject
                {
                    CreatedDate = DateTime.Today,
                    DeliveryDate = item.DeliveryDate,
                    LastModifiedBy = "UserName",
                    ModifiedDate = " : " + item.LastUpdateDate.Year.ToString() + "/" + item.LastUpdateDate.Month.ToString() + "/" + item.LastUpdateDate.Day.ToString(),
                    ProjectID = item.ProjectID,
                    RemainingDays = language.Current.GetMessage("DASHBOARD_PROJECT_DAYS_REMAINING"), // Added on 30-11-2018 for split days and remain text
                    RemainingDaysInNos = Convert.ToString((item.DeliveryDate - DateTime.UtcNow).Days),  //  Added on 30-11-2018 for split days and remain text
                    ProjectStatusPer = rnd.Next(1, 100), //Added on 30-11-2018 for perc
                    ProjectName = item.ProjectName
                    //RemainingDays = (item.DeliveryDate - DateTime.UtcNow).Days + " days remaining until due date"

                });

            }
            Projects = projects;
            // }
        }

    }
}
