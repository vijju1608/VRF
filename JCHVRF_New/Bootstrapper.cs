/****************************** File Header ******************************\
File Name:	Bootstrapper.cs
Date Created:	2/7/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New
{
    using JCHVRF.BLL.New;
    using JCHVRF.DAL.New;
    using JCHVRF.DAL.NextGen;
    using JCHVRF_New.Common.Constants;
    using JCHVRF_New.Common.Contracts;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Views;
    using Prism.Regions;
    using Prism.Unity;
    using System.Windows;
    using Unity;
    using Xceed.Wpf.AvalonDock;

    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        /// <summary>
        /// The ConfigureContainer
        /// </summary>
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            RegisterViewsForNavigation();
            RegisterTypesForDependencies();
        }

        /// <summary>
        /// The CreateShell
        /// </summary>
        /// <returns>The <see cref="DependencyObject"/></returns>
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        /// <summary>
        /// The InitializeShell
        /// </summary>
        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }

        /// <summary>
        /// The RegisterViewsForNavigation
        /// </summary>
        private void RegisterViewsForNavigation()
        {
            //Ideally this would have been done in the module file, 
            //but we are working on a single project and no different modules
            //If Plan changes, This code should be moved to Initialize() method of Module file later.

            Container.RegisterType<object, Dashboard>(ViewKeys.Dashboard);
            Container.RegisterType<object, Views.Settings>(ViewKeys.Settings);
            Container.RegisterType<object, NotificationsBar>(ViewKeys.NotificationsBar);
            Container.RegisterType<object, MasterDesigner>(ViewKeys.MasterDesigner);
            
            Container.RegisterType<object, CanvasProperties>(ViewKeys.CanvasProperties);
            Container.RegisterType<object, IDUProperties>(ViewKeys.IDUProperties);
            Container.RegisterType<object, ODUProperties>(ViewKeys.ODUProperties);
            Container.RegisterType<object, CHBoxProperties>(ViewKeys.CHBoxProperties);
            Container.RegisterType<object, MainApp>(ViewKeys.MainApp);           
            Container.RegisterType<object, Activation>(ViewKeys.Activation);
            Container.RegisterType<object, Tools>(ViewKeys.Tools);
            Container.RegisterType<object, Help>(ViewKeys.Help);
            Container.RegisterType<object, Splash>(ViewKeys.Splash);
            Container.RegisterType<object, CentralControllerSystemDetail>(ViewKeys.CentralControllerSystemDetail);
            Container.RegisterType<object, VRFSystemDetails>(ViewKeys.VRFSystemDetails);
            Container.RegisterType<object, CreateProjectWizard>(ViewKeys.CreateProjectWizard);
            Container.RegisterType<object, AddIndoorUnitView>(ViewKeys.AddIndoorUnitView);
            Container.RegisterType<object, AddEvent>(ViewKeys.AddEvent);
            Container.RegisterType<object, EventList>(ViewKeys.EventList);
            Container.RegisterType<object, ProjectSettingsView>(ViewKeys.ProjectSettingsView);
            Container.RegisterType<object, FloorTab>(ViewKeys.FloorTab);
            Container.RegisterType<object, AddEditRoom>(ViewKeys.AddEditRoom);
            Container.RegisterType<object, AddNewClient>(ViewKeys.Addnewclient);
            Container.RegisterType<object, NewCreatorInformation>(ViewKeys.NewCreatorInformation);
            Container.RegisterType<object, AddEditRoom>(ViewKeys.AddEditRoom);
            Container.RegisterType<object, BulkFloorPopup>(ViewKeys.BulkFloorPopup);
            Container.RegisterType<object, BulkRoomPopup>(ViewKeys.BulkRoomPopup);
            Container.RegisterType<object, EditSystemDetails>(ViewKeys.EditSystemDetails);
            Container.RegisterType<object, EditReportContents>(ViewKeys.EditReportContents);
            Container.RegisterType<object, AddAccessoriesTemplate>(ViewKeys.AddAccessories);
        }

        /// <summary>
        /// The RegisterTypesForDependencies
        /// </summary>
        void RegisterTypesForDependencies()
        {
            //Ideally this would have been done in the module file, 
            //but we are working on a single project and no different modules
            //If Plan changes, This code should be moved to Initialize() method of Module file later.
            Container.RegisterType<IProjectInfoDAL, ProjectInfoDAL>();
            Container.RegisterType<IProjectInfoBAL, ProjectInfoBLL>();
            Container.RegisterType<IEventDAL, EventDAL>();
            Container.RegisterSingleton<IGlobalProperties, GlobalProperties>();
            Container.RegisterSingleton<IModalWindowService, ModalWindowService>();
        }

        #endregion
    }
}
