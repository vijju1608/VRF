using JCHVRF.Model.New;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using JCHVRF_New.Views;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using JCHVRF_New.Common.Contracts;
using System.Windows.Input;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Constants;
using Prism.Regions;

namespace JCHVRF_New.ViewModels
{
    public class FloorTabViewModel : ViewModelBase
    {
        #region  Variable and constructions 
        private ObservableCollection<Floor> _listFloor;

        private string _isCancelEnable;

        public string IsCancelEnable
        {
            get { return _isCancelEnable; }
            set { this.SetValue(ref _isCancelEnable, value); }
        }

        private string _isSaveEnable;

        public string IsSaveEnable
        {
            get { return _isSaveEnable; }
            set { this.SetValue(ref _isSaveEnable, value); }
        }

        private IEventAggregator _eventAggregator;
        public DelegateCommand BulkRemoveFloorCommand { get; set; }
        public DelegateCommand BulkAddCommand { get; set; }
        public DelegateCommand AddFloorCommand { get; set; }
        public DelegateCommand RemoveFloorCommand { get; set; }
        public DelegateCommand AddEditFloorCommand { get; set; }
        public DelegateCommand<string> TestCommandOnFloorTxtChng { get; set; }

        public DelegateCommand CheckUpdateCommand { get; set; }

        JCHVRF.Model.Project CurrentProject;
            
        public bool AreAllFloorChecked
        {
            get { return ListFloor!=null && ListFloor.All(a=>a.IsFloorChecked); }
            set {
                if (ListFloor != null) {
                    foreach(var item in ListFloor)
                    {
                        item.IsFloorChecked = value;
                    }
                }
                RaisePropertyChanged();
            }
        }



        public FloorTabViewModel(IEventAggregator eventAggregator, IModalWindowService winService)
        {
            _eventAggregator = eventAggregator;
            _winService = winService;
            ListFloor = new ObservableCollection<Floor>();
            CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            BulkRemoveFloorCommand = new DelegateCommand(BulkRemoveFloor);
            AddFloorCommand = new DelegateCommand(AddFloorIconClick);
            AddEditFloorCommand = new DelegateCommand(AddEditFloorClick);
            RemoveFloorCommand = new DelegateCommand(RemoveFloorIconClick);
            CheckUpdateCommand = new DelegateCommand(()=> {
                RaisePropertyChanged(nameof(AreAllFloorChecked));
            });
            _eventAggregator.GetEvent<PubSubEvent<int?>>().Subscribe(OnMultiFloorValueTypeTab);
            _eventAggregator.GetEvent<PubSubEvent>().Subscribe(OnAddFloorEdit);
            _eventAggregator.GetEvent<PubSubEvent<string>>().Subscribe(OnMultiFloorValueBulkUpload);
            

            _eventAggregator.GetEvent<FloorListAddSubscriber>().Subscribe(AddFloor);
            BulkAddCommand = new DelegateCommand(BulkAddClick);
            ProjectInitialisation();
            _eventAggregator.GetEvent<FloorTabNext>().Subscribe(FloorNextClick);
            AddDefoultFloorlist();
            SaveClickCommand = new DelegateCommand(OnSaveClicked);
            NavigationParameters param = _winService.GetParameters(ViewKeys.FloorTab);
            if (param != null)
            {
                EnableFLoorSavebutton((bool)param["EnableSaveButtons"]);
            }
        }

        private void OnSaveClicked()
        {
              List<JCHVRF.Model.Floor> floorLegacy = new List<JCHVRF.Model.Floor>();         
                foreach (var floor in this.ListFloor)
                {
                    floorLegacy.Add(new JCHVRF.Model.Floor
                    {
                        Name = floor.floorName,
                        Height = floor.elevationFromGround,
                        Id = Convert.ToString(floor.Id)
                    });
                }
                CurrentProject.FloorList = floorLegacy;           
        }

        void ProjectInitialisation()
        {
            CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            IsSaveEnable = "Hidden";
            IsCancelEnable = "Hidden";
        }

        private void EnableFLoorSavebutton(bool IsValid)
        {
            if (IsValid == true)
            {
                IsSaveEnable = "Visible";
                IsCancelEnable = "Visible";
            }
            else if (IsValid == false)
            {
                IsSaveEnable = "Hidden";
            IsCancelEnable = "Hidden";}
        }
        private void FloorNextClick()
        {
            if (FloorTabInfoToProjectLegacy())
            {
                _eventAggregator.GetEvent<FloorTabSubscriber>().Publish(true);
            }
            else
            {
                _eventAggregator.GetEvent<FloorTabSubscriber>().Publish(false);
            }
        }

        private void AddFloorIconClick()
        {
            if (this.ListFloor != null) 
            {
                string lblFloor = string.Empty;
                int Id = this.ListFloor.Count + 1;
                lblFloor = JCHVRF.Model.SystemSetting.UserSetting.defaultSetting.FloorName +" " + (this.ListFloor.Count + 1);
                this.ListFloor.Add(new Floor { Id= Id, floorName = lblFloor, elevationFromGround = 0, IsFloorChecked = false });

             }
            else
            {
                this.ListFloor = new ObservableCollection<Floor>();
                string lblFloor = string.Empty;
                int Id = this.ListFloor.Count + 1;
                lblFloor = JCHVRF.Model.SystemSetting.UserSetting.defaultSetting.FloorName+" " + (this.ListFloor.Count + 1);
                this.ListFloor.Add(new Floor { Id = Id,floorName = lblFloor, elevationFromGround = 0, IsFloorChecked = false });
            }
            FloorTabInfoToProjectLegacy();
            EnableDisable();
        }

        private void RemoveFloorIconClick()
        {
            if (this.ListFloor != null && this.ListFloor.Count == 1)
            {
              
                JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_FLOOR_TAB_DELETE"), MessageType.Information, MessageBoxButton.OK);//"You can not delete this floor.At Least one floor must be available"
            }
            else
            {
                if (this.ListFloor != null && this.ListFloor.Count > 0)
                {
                    if (JCHMessageBox.Show(Langauge.Current.GetMessage("CONFIRM_FLOOR_DELETE"), MessageType.Information, MessageBoxButton.OKCancel) == MessageBoxResult.OK)//"Are you sure you want to delete the Floor?"
                    {
                        Floor floor = this.ListFloor.LastOrDefault();
                        this.ListFloor.Remove(floor);
                        if (JCHVRF.Model.Project.CurrentProject != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                        {
                            UpdateFloorList(floor);
                        }
                        EnableDisable();
                        ListFloor = ListFloor;
                      
                    }
                }
 
            }
            
        }

        private void UpdateFloorList( Floor obFloor)
        {
            foreach (var item in JCHVRF.Model.Project.CurrentProject.RoomIndoorList)
            {
                if (item.SelectedFloor != null)
                {
                    if (item.SelectedFloor.Name.Equals(obFloor.floorName))
                    {
                        item.SelectedFloor = null;
                    }
                }
            }
        }
        ObservableCollection<Floor> newListFloor = new ObservableCollection<Floor>();
        private void BulkRemoveFloor()
        {
            bool isCheckedAll = ListFloor.All(x => x.IsFloorChecked == true);
            if (isCheckedAll) 
            {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_FLOOR_TAB_DELETE_MINIMUM"), MessageType.Information, MessageBoxButton.OK);//"At Least one floor must be available"
                return;              
            }
            bool isCheckedAny = ListFloor.Any(x => x.IsFloorChecked == true);
            if (isCheckedAny)
            {
                if (this.ListFloor != null && this.ListFloor.Count == 1)
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_FLOOR_TAB_DELETE"), MessageType.Information, MessageBoxButton.OK);
                }
                else
                {
                    
                    if (JCHMessageBox.Show(Langauge.Current.GetMessage("CONFIRM_FLOOR_DELETE"), MessageType.Information, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {                                         
                        if (ListFloor != null && ListFloor.Count > 0)
                        {
                            newListFloor = new ObservableCollection<Floor>(this.ListFloor);
                            foreach (var item in this.ListFloor)
                            {
                                if (item.IsFloorChecked)
                                {
                                    newListFloor.Remove(item);
                                    if (JCHVRF.Model.Project.CurrentProject != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList != null && JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Count > 0)
                                    {
                                        UpdateFloorList(item);
                                    }
                                }
                                
                            }
                        }
                        ListFloor.Clear();
                        ListFloor = newListFloor;
                        #region Bug Fix 3812
                        List<JCHVRF.Model.Floor> floorLegacy = new List<JCHVRF.Model.Floor>();
                        if (floorLegacy != null)
                        {

                            foreach (var floor in ListFloor)
                            {
                                var fl = new JCHVRF.Model.Floor()
                                {
                                    Name = floor.floorName,
                                    Height = floor.elevationFromGround,
                                    Id = Convert.ToString(floor.Id)
                                };


                                floorLegacy.Add(fl);
                            }
                        }
                        if (CurrentProject.FloorList != null)

                        {
                            CurrentProject.FloorList = floorLegacy;
                        }
                        #endregion Bug Fix 3812
                    }
     
                }
            }
        }

        private void AddDefoultFloorlist()
        {
            if (CurrentProject.FloorList.Count > 0)
            {
                int Id = CurrentProject.FloorList.Count + 1;
                IsEnableRemove = true;
                for (int i = 0; i < CurrentProject.FloorList.Count; i++)
                {
                    this.ListFloor.Add(new Floor { Id = Convert.ToInt16(CurrentProject.FloorList[i].Id), floorName = CurrentProject.FloorList[i].Name.ToString(), elevationFromGround = CurrentProject.FloorList[i].Height, IsFloorChecked = false });
                }
            }
        }
        private bool FloorTabInfoToProjectLegacy()
        {
            var currentRoomList = new List<JCHVRF.Model.Room>();
            if (CurrentProject.FloorList.Count > 0)
            {
                currentRoomList = CurrentProject.RoomList;
            }
            List<JCHVRF.Model.Floor> floorLegacy = new List<JCHVRF.Model.Floor>();
            if (ListFloor.Count >= 1)
            {
                foreach (var floor in ListFloor)
                {
                    var fl = new JCHVRF.Model.Floor()
                    {
                        Name = floor.floorName,
                        Height = floor.elevationFromGround,
                        Id = Convert.ToString(floor.Id)
                    };
                   

                    floorLegacy.Add(fl);
                }
                CurrentProject.FloorList = floorLegacy;
                //CurrentProject.RoomList = currentRoomList;
                return true;
            }
            else
            {
                return false;
            }
        }
        private void BulkAddClick()
        {
            _winService.ShowView(ViewKeys.BulkFloorPopup, Langauge.Current.GetMessage("ADD_FLOOR"));
        }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }

        private bool _isEnableRemove;
        private IModalWindowService _winService;

        public bool IsEnableRemove
        {
            get { return _isEnableRemove; }
            set { this.SetValue(ref _isEnableRemove, value); }
        }
        #endregion

        #region View model properties
        public ObservableCollection<Floor> ListFloor
        {
            get { return _listFloor; }
            set
            {
                this.SetValue(ref _listFloor, value);
            }
        }

        public DelegateCommand SaveClickCommand { get; private set; }

        #endregion

        private void EnableDisable()
        {
            if (this.ListFloor != null && this.ListFloor.Count < 1)
            {
                IsEnableRemove = false;
            }
            else
            {
                IsEnableRemove = true;
            }

        }

        #region Methods and Command methods
        private void OnAddFloorEdit()
        {
            try
            {
                if (CurrentProject != null)
                {
                    if (CurrentProject.FloorList != null)
                    {

                        this.ListFloor.Clear();
                        if (CurrentProject.FloorList.Count > 0)
                        {
                            for (int i = 0; i < CurrentProject.FloorList.Count; i++)
                            {

                                this.ListFloor.Add(new Floor { Id = Convert.ToInt16(CurrentProject.FloorList[i].Id), floorName = CurrentProject.FloorList[i].Name.ToString(), elevationFromGround = CurrentProject.FloorList[i].Height, IsFloorChecked = false });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Logger.LogProjectError(null, ex);
            }
        }
        private void OnMultiFloorValueTypeTab(int? floorCount)
        {
            if (floorCount != null)
            {
                int Id = 0;
                this.ListFloor.Clear();
                if (this.ListFloor.Count == 0)
                {
                    if (CurrentProject.FloorList.Count > 0)
                    {
                        Id = CurrentProject.FloorList.Count;
                    }
                    for (int i = 0; i < floorCount; i++)
                    {
                        Id = Id + 1;
                        string lblFloor = string.Empty;
                        lblFloor = JCHVRF.Model.SystemSetting.UserSetting.defaultSetting.FloorName + " " + (Id);
                        this.ListFloor.Add(new Floor {Id= Id, floorName = lblFloor, elevationFromGround = 0, IsFloorChecked = false });
                        EnableDisable();
                    }
                }
                //else
                //{
                //    for (int i = this.ListFloor.Count; i < floorCount; i++)
                //    {
                //        string lblFloor = string.Empty;
                //        lblFloor = "Floor " + (i);
                //        this.ListFloor.Add(new Floor { floorName = lblFloor, elevationFromGround = 0, IsFloorChecked = false });
                //    }

                //}
            }
        }
        private void OnMultiFloorValueBulkUpload(string floorCount)
        {           
            int intfloorCount = int.Parse(floorCount);
            int Id = 0;
            if (intfloorCount > 999 || ( this.ListFloor != null && this.ListFloor.Count > 999)||(intfloorCount + this.ListFloor.Count)>1000)
            {
                //JCHMessageBox.Show("You can enter maximum 1000 floor");
                return;
            }
            if (floorCount != null)
            {
                IsEnableRemove = true;
                if (this.ListFloor.Count == 0)
                {
                    if (CurrentProject.FloorList.Count > 0)
                    {
                        Id = CurrentProject.FloorList.Count;
                    }
                    for (int i = 0; i < intfloorCount; i++)
                    {
                        Id = Id + 1;
                        string lblFloor = string.Empty;
                        lblFloor = JCHVRF.Model.SystemSetting.UserSetting.defaultSetting.FloorName + " " + (Id);
                        this.ListFloor.Add(new Floor { Id=Id,floorName = lblFloor, elevationFromGround = 0, IsFloorChecked = false });
                    }
                }
                else
                {
                    //if (this.ListFloor.Count < intfloorCount)
                    //{
                    int BulkAddCount = this.ListFloor.Count + intfloorCount;
                    Id =this.ListFloor.Count+1;
                    for (int i = this.ListFloor.Count; i < BulkAddCount; i++)
                    {
                        string lblFloor = string.Empty;
                        lblFloor = JCHVRF.Model.SystemSetting.UserSetting.defaultSetting.FloorName + " " + (this.ListFloor.Count + 1);
                        this.ListFloor.Add(new Floor { Id= Id, floorName = lblFloor, elevationFromGround = 0, IsFloorChecked = false });
                    }
                }
            }
        }

        private void AddFloor()
        {
            List<JCHVRF.Model.Floor> floorLegacy = new List<JCHVRF.Model.Floor>();
         
            if (this.ListFloor.Count > 1)
            {
                foreach (var floor in this.ListFloor)
                {
                    floorLegacy.Add(new JCHVRF.Model.Floor
                    {
                        Name = floor.floorName,
                        Height = floor.elevationFromGround,
                        Id = Convert.ToString(floor.Id)
                    });
                }
                CurrentProject.FloorList = floorLegacy;
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Publish();
                _eventAggregator.GetEvent<PubSubEvent>().Publish();
            }
        }
        private void AddEditFloorClick()
        {
            List<JCHVRF.Model.Floor> floorLegacy = new List<JCHVRF.Model.Floor>();
                foreach (var floor in this.ListFloor)
                {
                    floorLegacy.Add(new JCHVRF.Model.Floor
                    {
                        Name = floor.floorName,
                        Height = floor.elevationFromGround,
                        Id = Convert.ToString(floor.Id)
                    });
                }
                CurrentProject.FloorList = floorLegacy;
                _eventAggregator.GetEvent<FloorListSaveSubscriber>().Publish();
            _eventAggregator.GetEvent<PubSubEvent>().Publish();        
            _winService.Close(ViewKeys.FloorTab);

        }
        #endregion  Methods and Command methods
    }
}
