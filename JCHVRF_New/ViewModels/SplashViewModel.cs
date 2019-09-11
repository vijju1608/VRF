using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Helpers;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JCHVRF_New.ViewModels
{
    public class SplashViewModel : ViewModelBase
    {
        private JCHVRF.Model.Project _project;
        public JCHVRF.Model.Project Project
        {
            get { return _project; }
            set { this.SetValue(ref _project, value); }
        }
        public SplashViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            _eventAggregator = eventAggregator;


        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if (navigationContext.Parameters.ContainsKey("Project"))
            {
                Project = navigationContext.Parameters["Project"] as JCHVRF.Model.Project;

                //Thread.Sleep(5000);

                NavigationParameters param = new NavigationParameters();
                param.Add("Project", Project);

                regionManager.RequestNavigate(RegionNames.ContentRegion, ViewKeys.MasterDesigner, param);
            }
        }

        IRegionManager regionManager;
        private IEventAggregator _eventAggregator;
    }
}
