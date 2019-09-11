using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;

namespace JCHVRF_New.ViewModels
{
    public class EditSystemDetailsViewModel:ViewModelBase
    {
        private List<JCHVRF.Model.NextGen.SystemVRF> _systemList;
        private ObservableCollection<LeftSideBarChild> _vrfSystemsObservableCollection;
        public List<JCHVRF.Model.NextGen.SystemVRF> SystemList
        {
            get {return _systemList; }
            set { _systemList = value; }
        }



        public EditSystemDetailsViewModel()
        {

        }

        public void DisplaySystem()
        {
            var currentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;

            List<LeftSideBarChild> vrfSystems = new List<LeftSideBarChild>();
            foreach (JCHVRF.Model.NextGen.SystemVRF systemVRF in currentSystem)
            {
                vrfSystems.Add(new LeftSideBarChild(systemVRF.Name, "VRF", systemVRF.StatusIcon, systemVRF));
            }


            _vrfSystemsObservableCollection.AddRange(vrfSystems);
        }

        private void InsertOrAdd<T>(ObservableCollection<T> collection, int position, T item)
        {
            if (!collection.Contains(item))
            {
                if (collection.Count > position)
                {
                    collection.Insert(position, item);
                }
                else
                {
                    collection.Add(item);
                }
            }
        }
    }
}
