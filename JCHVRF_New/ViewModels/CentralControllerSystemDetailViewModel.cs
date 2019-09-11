using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.ViewModels
{
    using System.Collections.Generic;
    using JCBase.Utility;
    using JCHVRF.Model;
    using JCHVRF_New.Common.Contracts;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Prism.Events;
    using JCHVRF.Model.NextGen;
    using System;

    public class CentralControllerSystemDetailViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;
        private ObservableCollection<MaterialList> _modelDetails;
        private SystemBase system = WorkFlowContext.CurrentSystem;
        private bool flgEventSubscribe;
        public ObservableCollection<MaterialList> ModelDetail
        {
            get { return _modelDetails; }
            set { SetValue(ref _modelDetails, value); }
        }

      
        public CentralControllerSystemDetailViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            ModelDetail = new ObservableCollection<MaterialList>();

            if (!flgEventSubscribe)
            {
                _eventAggregator.GetEvent<SendControllerDetails>().Subscribe(Refresh);
                flgEventSubscribe = true;
            }

        }
       
        public void Refresh(SystemBase system)
        {
            if (system != null)
            {
                var cProj = new List<Controller>();
                // cProj = Project.CurrentProject.ControllerList.FindAll(x => x.ControlSystemID.Equals(system.Id));
                ModelDetail.Clear();
                ControlGroup _group = null;
                //if (cProj.Count <= 0)
                //{
                _group = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID.Equals(system.Id));
                if (_group != null)
                {
                    cProj = Project.CurrentProject.ControllerList.FindAll(x => x.ControlGroupID.Equals(_group.Id));
                }
                //  }

                foreach (var sys in cProj)
                {
                    ModelDetail.Add(new MaterialList
                    {
                        Model = sys.Model,
                        Description = sys.Description,
                        Qty = sys.Quantity
                    });
                }
            }
        }

        ~CentralControllerSystemDetailViewModel()
        {
            _eventAggregator.GetEvent<SendControllerDetails>().Unsubscribe(Refresh);
        }
    }
}
