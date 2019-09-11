using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using Prism.Commands;

namespace JCHVRF_New.ViewModels
{
    public class PropertiesViewModel : ViewModelBase
    {
        private IRegionManager _regionManager;

        public PropertiesViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public DelegateCommand LoadedCommand { get; private set; }

        protected override void RaiseIsActiveChanged()
        {
            base.RaiseIsActiveChanged();
            if (!IsActive)
           {
              _regionManager.Regions.Remove(RegionNames.MasterDesignerPropertiesRegion);
               // _regionManager.Regions.Add(RegionNames.MasterDesignerPropertiesRegion);
            }
        }
    }
}
