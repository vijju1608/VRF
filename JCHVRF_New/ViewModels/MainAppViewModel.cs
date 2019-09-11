using JCHVRF_New.Common.Helpers;
using FontAwesome.WPF;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Unity;
using System.Windows.Input;
using System;
using JCHVRF_New.Common.Contracts;
using JCHVRF.Model.NextGen;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Utility;
using JCHVRF.Model;

namespace JCHVRF_New.ViewModels
{
    public class MainAppViewModel : ViewModelBase
    {
        #region Fields

        private IEventAggregator _eventAggregator;

        private ScrollBarVisibility _horizontalScrollBarVisibilityRequirement;

        private bool _isLeftDrawerOpen;

        private Thickness _marginRequirement;

        private SideBarItem _selectedSideBarItem;

        private ScrollBarVisibility _verticalScrollBarVisibilityRequirement;

        IUnityContainer _container;

        IRegionManager _regionManager;
        private IGlobalProperties _globalProperties;

        #endregion

        #region Constructors

        public MainAppViewModel(IGlobalProperties globalProperties, IRegionManager regionManager, IUnityContainer container, IEventAggregator eventAggregator)
        {
            try
            {
                _regionManager = regionManager;
                _container = container;
                _eventAggregator = eventAggregator;
                _globalProperties = globalProperties;
                _globalProperties.Notifications.Insert(0, new Notification(NotificationType.APPLICATION, "Application Started !"));

                MainAppLoadedCommand = new DelegateCommand(
                   () =>
                   {
                       _regionManager.RequestNavigate(RegionNames.ToolbarRegion, ViewKeys.NotificationsBar);
                       SelectedSideBarItem = SideBarItems.First();
                       Mouse.OverrideCursor = null;

                       _regionManager.Regions[RegionNames.ContentRegion].NavigationService.Navigated += (o, e) =>
                       {
                           if (!SideBarItems.Select(a => a.MenuViewKey).Contains(e.Uri.OriginalString))
                           {
                               SelectedSideBarItem = null;
                           }
                           else if (SelectedSideBarItem == null || SelectedSideBarItem.MenuViewKey != e.Uri.OriginalString)
                           {
                               SelectedSideBarItem = SideBarItems.First(a => a.MenuViewKey == e.Uri.OriginalString);
                           }

                           if (e.Uri.OriginalString == "Home")
                           {
                               if (Project.GetProjectInstance != null)
                                   Project.GetProjectInstance.SelectedSystemID = string.Empty;
                               UndoRedoSetup.SetInstanceNull();
                           }

                       };
                   }
                   );
                DrawerItemSelectionChangedCommand = new DelegateCommand<ISideBarItem>(OnDrawerItemSelected);
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DrawerItemSelectionChangedCommand
        /// </summary>
        public DelegateCommand<ISideBarItem> DrawerItemSelectionChangedCommand { get; set; }

        /// <summary>
        /// Gets or sets the HorizontalScrollBarVisibilityRequirement
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibilityRequirement
        {
            get { return _horizontalScrollBarVisibilityRequirement; }
            set { this.SetValue(ref _horizontalScrollBarVisibilityRequirement, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsLeftDrawerOpen
        /// </summary>
        public bool IsLeftDrawerOpen
        {
            get { return _isLeftDrawerOpen; }
            set
            {
                this.SetValue(ref _isLeftDrawerOpen, value);
                if (value)
                {
                    foreach (var item in SideBarItems)
                    {
                        item.RefreshHeaders();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the MarginRequirement
        /// </summary>
        public Thickness MarginRequirement
        {
            get { return _marginRequirement; }
            set { this.SetValue(ref _marginRequirement, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedSideBarItem
        /// </summary>
        public SideBarItem SelectedSideBarItem
        {
            get
            {
                return _selectedSideBarItem;
            }
            set
            {
                this.SetValue(ref _selectedSideBarItem, value);
                if (value != null)
                    _regionManager.RequestNavigate(RegionNames.ContentRegion, SelectedSideBarItem?.MenuViewKey);
            }
        }

        /// <summary>
        /// Gets or sets the MainAppLoadedCommand
        /// </summary>
        public DelegateCommand MainAppLoadedCommand { get; set; }

        private ObservableCollection<SideBarItem> _sideBarItems;

        /// <summary>
        /// Gets the SideBarItems
        /// </summary>
        public ObservableCollection<SideBarItem> SideBarItems
        {
            get
            {
                if (_sideBarItems == null)
                {
                    _sideBarItems = new ObservableCollection<SideBarItem>()
            {
                new SideBarItem(ViewKeys.Dashboard,"HOME", "HomeLogo"),
                new SideBarItem(ViewKeys.Settings,"SETTINGS", "SettingLogo")
        {
            Children = new ObservableCollection<SideBarChild>(){
                        new SideBarChild("GENERAL", ViewKeys.Settings),
                        //new SideBarChild("MY_ACCOUNT", ViewKeys.Settings),
                        new SideBarChild("LOCATION", ViewKeys.Settings),
                        //new SideBarChild("PAGE_VIEW", ViewKeys.Settings),
                        //new SideBarChild("USERS", ViewKeys.Settings),
                        //new SideBarChild("REPORT_LAYOUT", ViewKeys.Settings),
                        new SideBarChild("NOTIFICATIONS", ViewKeys.Settings),
                        new SideBarChild("MEASUREMENT_UNIT", ViewKeys.Settings),
                        new SideBarChild("NAME_PREFIXES", ViewKeys.Settings)
                    }
                },
                new SideBarItem(ViewKeys.Tools,"TOOLS", "ToolsLogo")
        {
            Children = new ObservableCollection<SideBarChild>(){
                        new SideBarChild("HEAT_LOAD_CALCULATOR", ViewKeys.Tools),
                        new SideBarChild("CONTROLLER_SIMULATOR", ViewKeys.Tools),
                        new SideBarChild("CONSUMPTION_CALCULATOR", ViewKeys.Tools),
                        new SideBarChild("PEAK_LOAD_CALCULATOR", ViewKeys.Tools),
                        new SideBarChild("TEMPERATURE_SIMULATOR", ViewKeys.Tools),
                        new SideBarChild("DOWNLOAD_TEMPLATES", ViewKeys.Tools),
                        new SideBarChild("EMMISSIONS_CALCULATOR", ViewKeys.Tools),
                        new SideBarChild("STANDARDS_LIBRARY", ViewKeys.Tools),
                        new SideBarChild("BROWSE_CATALOGUE", ViewKeys.Tools),
                        new SideBarChild("PRODUCT_COMPARISIONS", ViewKeys.Tools)
                    }
                },
                new SideBarItem(ViewKeys.Help, "HELP", "HelpLogo")
                    };
                }
                return _sideBarItems;
            }
        }

        /// <summary>
        /// Gets or sets the VerticalScrollBarVisibilityRequirement
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibilityRequirement
        {
            get { return _verticalScrollBarVisibilityRequirement; }
            set { this.SetValue(ref _verticalScrollBarVisibilityRequirement, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The OnDrawerItemSelected
        /// </summary>
        /// <param name="e">The e<see cref="RoutedPropertyChangedEventArgs{object}"/></param>
        private void OnDrawerItemSelected(ISideBarItem selectedItem)
        {
            try
            {
                //ISideBarItem selectedItem = e.NewValue as ISideBarItem;
                SelectedSideBarItem = null;
                SelectedSideBarItem = SideBarItems.FirstOrDefault(a => a.MenuViewKey == selectedItem.MenuViewKey);

                _eventAggregator.GetEvent<PubSubEvent<ISideBarItem>>().Publish(selectedItem);

                IsLeftDrawerOpen = false;
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }


        #endregion
    }
}
