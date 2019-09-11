using JCHVRF.Model;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Commands;
using Prism.Events;
using Registr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Contracts;
using language = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{

    public class IndoorUnitInfoViewModel : ViewModelBase
    {
        #region Privates
        private JCHVRF.Model.Project _project;
        private JCHVRF.BLL.ProjectBLL _bllProject;
        private IEventAggregator _eventAggregator;
        #endregion

        private bool _passValidation;
        private SubscriptionToken _subscriptionToken;

        public bool PassValidation
        {
            get { return _passValidation; }
            set { _passValidation = value; }
        }

        #region Measurement Unit Related Properties

        public string PowerUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingPOWER;
            }
        }

        public string TemperatureUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            }
        }

        public string AirFlowUnit
        {
            get
            {
                return SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            }
        }

        #endregion

        public bool IsCheckAll
        {
            get { return SelectedIDU!=null && SelectedIDU.Count>0 && SelectedIDU.All(a=>a.IsChecked); }
            set
            {
                foreach (var item in SelectedIDU)
                {
                    item.IsChecked = value;
                }
                RefreshHeaderAndRemoveButton();
            }
        }
        

        public bool CanRemove
        {
            get { return SelectedIDU !=null && SelectedIDU.Count>0 && SelectedIDU.Any(i => i.IsChecked); }
        }

        public string Settings { get; set; }

        public DelegateCommand RemoveCommand { get; set; }
        public DelegateCommand SelectCommand { get; set; }

        public DelegateCommand OpenAddIndoorUnit { get; set; }
        private ObservableCollection<IDU> _selectedIDU;
        public ObservableCollection<IDU> SelectedIDU
        {
            get
            {
                return _selectedIDU;
            }
            set
            {
                _selectedIDU = value;
                RaisePropertyChanged("SelectedIDU");
            }
        }
        private IModalWindowService _winService;

        public IndoorUnitInfoViewModel(IEventAggregator EventAggregator ,IModalWindowService winService)
        {
            try
            {
                _project = JCHVRF.Model.Project.GetProjectInstance;

                _bllProject = new JCHVRF.BLL.ProjectBLL(_project);

                _eventAggregator = EventAggregator;


                SelectedIDU = new ObservableCollection<IDU>();
                SetIndoorSystemSpecificIndoorInfo();

                RemoveCommand = new DelegateCommand(OnRemove);
                SelectCommand = new DelegateCommand(RefreshHeaderAndRemoveButton);
                _winService = winService;
                OpenAddIndoorUnit = new DelegateCommand(OpenAddIndoorUnitView);
            }
            catch (Exception ex)
            {
                int? id = _project?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }

        private void OpenAddIndoorUnitView()
        {
            SubscribeAddAll();
            _winService.ShowView(ViewKeys.AddIndoorUnitView,"Add Indoor Unit");
           
        }

        private void SetIndoorSystemSpecificIndoorInfo()
        {
            if (!string.IsNullOrEmpty(JCHVRF.Model.Project.CurrentSystemId))
            {
                JCHVRF.Model.NextGen.SystemVRF _currentSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(x => ((SystemBase)x).Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
                if (_currentSystem != null)
                {
                    SelectedIDU = new ObservableCollection<IDU>();

                    foreach (var rid in JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList.Where(i => i.SystemID == _currentSystem.Id))
                    {
                        // TODO : this can be incorported in RoomIndoor if no other dependency
                        var model = Convert.ToBoolean(_project?.BrandCode.Equals("Y"))
                            ? rid.IndoorItem.Model_York : rid.IndoorItem.Model_Hitachi;

                        SelectedIDU.Add(new IDU
                        {
                            RoomIndoor = rid,
                            IsChecked = rid.IsDelete,
                            DisplayModelName = model
                        });
                    }
                }
            }
        }

        public void SubscribeAddAll()
        {
            if(_subscriptionToken !=null )
                _eventAggregator.GetEvent<PubSubEvent<ObservableCollection<RoomIndoor>>>().Unsubscribe(_subscriptionToken);

            _subscriptionToken = _eventAggregator.GetEvent<PubSubEvent<ObservableCollection<RoomIndoor>>>().Subscribe(BindAllTheList);
        }

        public void UnsubscribeAddAll()
        {
            _eventAggregator.GetEvent<PubSubEvent<ObservableCollection<RoomIndoor>>>().Unsubscribe(BindAllTheList);
        }

        private void BindAllTheList(ObservableCollection<RoomIndoor> item)
        {
            try
            {
                var sysID = item.FirstOrDefault().SystemID;

                UpdateProject(item);

                Refresh();

                UnsubscribeAddAll();
            }
            catch (Exception ex)
            {
                int? id = _project?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        /// <summary>
        /// Add all IDU to the legacy project object and do the validation
        /// </summary>
        private void UpdateProject(ObservableCollection<RoomIndoor> item)
        {
            Debug.WriteLine("Update project called");

            if (_project == null)
                _project = JCHVRF.Model.Project.GetProjectInstance;


            //_project.RoomIndoorList.AddRange(item.ToList());
            AddIndoors(item.ToList());

        }

        private void AddIndoors(List<RoomIndoor> riItem)
        {

            if (riItem == null)
                return;

            _project.RoomIndoorList.AddRange(riItem);
            _project.RoomIndoorList.RemoveAll(c => { return c.IsDelete; });

            //If the remaining new fan model "JTAF1080" is selected separately, the suitable outdoor unit cannot be found, untie and prompt
            //if (_project.RoomIndoorList.Count > 0)
            //{
            //    if (_project.RoomIndoorList[0].SystemID != null && !_project.RoomIndoorList[0].SystemID.Equals(""))
            //    {
            //        if (_project.RoomIndoorList.Count == 1 && _project.RoomIndoorList[0].IndoorItem.Model.Equals("JTAF1080"))
            //        {
            //            _project.RoomIndoorList[0].SetToSystemVRF(null);
            //        }
            //    }
            //}
        }

        private List<RoomIndoor> GetSelectedIndoorOrExchangerByRoom(string RoomID)
        {
            List<RoomIndoor> list = _bllProject.GetSelectedIndoorByRoom(RoomID);


            if (Registration.SelectedSubRegion.ParentRegionCode== "EU_W" || Registration.SelectedSubRegion.ParentRegionCode == "EU_S" || Registration.SelectedSubRegion.ParentRegionCode == "EU_E")
            {
                List<RoomIndoor> lists = _bllProject.GetSelectedExchangerByRoom(RoomID);
                if (lists != null && lists.Count > 0)
                {
                    return list;
                }
            }
            return list;
        }

        private void DeleteShareRelationShip(RoomIndoor ri)
        {
            string groupIndoorName = "";
            if (ri.IndoorItemGroup == null || ri.IndoorItemGroup.Count == 0)
                return;

            foreach (RoomIndoor rind in ri.IndoorItemGroup)
            {
                groupIndoorName = groupIndoorName += rind.IndoorName + ",";
            }

            groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
            
            foreach (string indNo in groupIndoorName.Split(','))
            {
                RoomIndoor riItem = _bllProject.GetIndoor(int.Parse(indNo.Substring(indNo.Length - 1)));
                riItem.IndoorItemGroup = null;
                if (riItem.IsMainIndoor)
                    riItem.IsMainIndoor = false;
            }

            return;
        }

        private void RemoveIdu(IEnumerable<RoomIndoor> rmList)
        {
            if (rmList == null) return;

            var shareRindName = string.Empty;
            var shareRiList = new List<RoomIndoor>();

            #region Building shared list

            foreach (var ri in rmList)
            {
                var indNo = ri.IndoorNO;

                // TODO : to confirm this
                //if (ri.IsAuto)
                //    continue;

                if (!string.IsNullOrEmpty(ri.RoomID) && !_bllProject.isEmptyRoom(ri.RoomID))
                {
                    // show warning, if required
                    if (ri.IndoorItemGroup != null)
                    {
                        shareRindName += ri.IndoorName + ",";
                        shareRiList.Add(ri);
                    }
                }

            }

            #endregion

            #region DeleteShareRelationships
            // delete share lists
            if (!string.IsNullOrEmpty(shareRindName) && shareRiList.Count != 0)
            {
                foreach (RoomIndoor shareRi in shareRiList)
                {
                    DeleteShareRelationShip(shareRi);
                }
            }
            #endregion

            if (_project.RoomIndoorList != null)
            {
                //_project.RoomIndoorList
                //       .RemoveAll(i => rmList.Any(r => r.IndoorNO == i.IndoorNO));
                foreach (var ri in rmList)
                    _bllProject.RemoveRoomIndoor(ri.IndoorNO);
            }
            Refresh();
        }

        private void OnRemove()
        {
            if (JCHMessageBox.Show(language.Current.GetMessage("CONFIRM_IDU_DELETE"), MessageType.Information, System.Windows.MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.OK)
            {
                try
                {
                    var toRemove = SelectedIDU.Where(i => i.IsChecked).Select(i => i.RoomIndoor);
                    RemoveIdu(toRemove);
                    RefreshHeaderAndRemoveButton();
                }
                catch (Exception ex)
                {
                    int? id = _project?.projectID;
                    Logger.LogProjectError(id, ex);
                }
            }
        }

        private void RefreshHeaderAndRemoveButton()
        {
            RaisePropertyChanged(nameof(IsCheckAll));
            RaisePropertyChanged(nameof(CanRemove));
        }

        private void Refresh()
        {

            var currentSys = WorkFlowContext.NewSystem==null?WorkFlowContext.CurrentSystem: WorkFlowContext.NewSystem;

            if(currentSys == null)
            {
                Debug.WriteLine("Current system not found");
            }

            SelectedIDU = new ObservableCollection<IDU>();

            foreach (var rid in JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList.Where(i => i.SystemID == currentSys.Id))
            {
                // TODO : this can be incorported in RoomIndoor if no other dependency
                var model = Convert.ToBoolean(_project?.BrandCode.Equals("Y"))
                    ? rid.IndoorItem.Model_York : rid.IndoorItem.Model_Hitachi;

                SelectedIDU.Add(new IDU
                {
                    RoomIndoor = rid,
                    IsChecked = rid.IsDelete,
                    DisplayModelName = model
                });
            }
            RefreshHeaderAndRemoveButton();
        }
    }
}
