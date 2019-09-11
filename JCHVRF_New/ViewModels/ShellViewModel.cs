/****************************** File Header ******************************\
File Name:	ShellViewModel.cs
Date Created:	2/7/2019
Description:	View Model for the Shell.
\*************************************************************************/

namespace JCHVRF_New.ViewModels
{
    using FontAwesome.WPF;
    using JCHVRF_New.Common.Constants;
    using JCHVRF_New.Common.Contracts;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Regions;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Unity;
    using LanguageData=JCHVRF_New.LanguageData;

    public class ShellViewModel : ViewModelBase
    {
        private IRegionManager _regionManager;
        private IUnityContainer _container;
        private IEventAggregator _eventAggregator;
        private IGlobalProperties _globalProperties;

        /// <summary>
        /// Gets or sets the ShellLoadedCommand
        /// </summary>
        public DelegateCommand ShellLoadedCommand { get; set; }
        public ShellViewModel(IRegionManager regionManager, IUnityContainer container, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _container = container;
            _eventAggregator = eventAggregator;
            LanguageData.LanguageViewModel.Current.UpdateLanguageData();
            ShellLoadedCommand = new DelegateCommand(
               () =>
                   {
                       _regionManager.RequestNavigate(RegionNames.MainAppRegion, ViewKeys.Activation);
                   }
               );
        }
    }
}
