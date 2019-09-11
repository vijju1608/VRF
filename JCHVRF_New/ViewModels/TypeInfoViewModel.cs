using JCHVRF.Model;
using JCHVRF_New.Common.Helpers;
using JCHVRF.Model.New;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Events;
using System.Windows;
using JCHVRF.Model.NextGen;
using JCHVRF_New.Model;
using JCHVRF.BLL.New;
using JCHVRF.BLL;
using System.Data;
using JCHVRF.DAL;
using Language= JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class TypeInfoViewModel : ViewModelBase
    {

        #region Delegate Commands
        JCHVRF.Model.Project CurrentProject;
        private CentralControllerDAL _controllerDAL;

        public DelegateCommand<int?> LoadFloor { get; set; }
        private IProjectInfoBAL _projectInfoBll;
        private ObservableCollection<SystemTypeItem> _systemTypeCollection;
        public ICommand ShowCommand { get; private set; }
        public DelegateCommand SingleFloorCheckedCommand { get; set; }
        public DelegateCommand SingleFloorUnCheckedCommand { get; set; }
        public DelegateCommand MultipleFloorCheckedCommand { get; set; }
        public DelegateCommand MultipleFloorUnCheckedCommand { get; set; }
        public DelegateCommand RegularCheckedCommnd { get; set; }
        public ICommand SystemTypeCommad { get; private set; }
        private IEventAggregator _eventAggregator;
        public ICommand HideCommand { get; private set; }
        #endregion
        #region Viewmodel properties
        public ObservableCollection<SystemTypeItem> SystemTypeCollection
        {
            get
            {
                if (_systemTypeCollection == null)
                    _systemTypeCollection = new ObservableCollection<SystemTypeItem>();
                return _systemTypeCollection;
            }
            set { this.SetValue(ref _systemTypeCollection, value); }
        }

        private JCHVRF.Model.Project _listFloor;
        public JCHVRF.Model.Project ListFloor
        {
            get { return _listFloor; }
            set
            {
                this.SetValue(ref _listFloor, value);

            }
        }
        private SystemTypeItem _selectedsystemName;
        public SystemTypeItem SelectedsystemName
        {
            get { return _selectedsystemName; }
            set
            {
                this.SetValue(ref _selectedsystemName, value);
                if (SelectedsystemName != null)
                {
                    ShowOduAndIduSystem();
                    HideControllerTypeText();
                    ShowTheuInfoSystem(); //ACC - RAG
                    ShowCreateBtnOnClick(); //ACC - RAG
                    ShowSaveBtnOnClick();//Acc IA
                    ShowNextBtnOnClick(); //ACC - RAG
                    UpdateSystemName();
                    WorkFlowContext.Systemid = SelectedsystemName.SystemID;
                }
            }
        }

        public void UpdateSystemName()
        {

            switch(SelectedsystemName.SystemID)
            {
                case "1":
                    SystemName = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName + " " + (JCHVRF.Model.Project.CurrentProject.SystemListNextGen.Count + 1);
                    break;
                case "6":
                    SystemName = SystemSetting.UserSetting.defaultSetting.ControllerName + " " + (JCHVRF.Model.Project.CurrentProject.ControlSystemList.Count + 1);
                    break;
                case "2":
                    SystemName = SystemSetting.UserSetting.defaultSetting.ExchangerName + " " + (JCHVRF.Model.Project.CurrentProject.ExchangerList.Count + 1);
                    break;

            }
        }


        private string _systemName;
        public string SystemName
        {
            get { return _systemName; }
            set { this.SetValue(ref _systemName, value); }
        }

        private bool _isSingleFoorChecked;
        public bool IsSingleFoorChecked
        {
            get { return _isSingleFoorChecked; }
            set
            {
                this.SetValue(ref _isSingleFoorChecked, value);
            }
        }

        private bool _ismultipleFloorChecked;
        public bool IsMultipleFloorChecked
        {
            get { return _ismultipleFloorChecked; }
            set { this.SetValue(ref _ismultipleFloorChecked, value); }
        }
        private int _floorcounts;

        public int FloorCount
        {
            get { return _floorcounts = IsSingleFoorChecked == false ? _floorcounts : 1; }
            set
            {
                this.SetValue(ref _floorcounts, value);

            }
        }
        private bool _isRegular;
        public bool IsRegular
        {
            get { return _isRegular; }
            set { this.SetValue(ref _isRegular, value); }
        }
        private bool _isCAD;
        public bool IsCAD
        {
            get { return _isCAD; }
            set { this.SetValue(ref _isCAD, value); }
        }

        private List<JCHVRF.Model.Floor> _floorList;
        public List<JCHVRF.Model.Floor> FloorList
        {
            get { return _floorList; }
            set
            {
                this.SetValue(ref _floorList, value);

            }
        }

        private bool _isSelected;
       

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    _eventAggregator.GetEvent<TypeInfoTabNext>().Subscribe(TypeTabNextClick);
                    GetSystemTypeList();
                }
                else {
                    _eventAggregator.GetEvent<TypeInfoTabNext>().Unsubscribe(TypeTabNextClick);
                }
                RaisePropertyChanged("IsSelected");
            }
        }

        #endregion
        #region constructor and Initisation
        public TypeInfoViewModel(IProjectInfoBAL projctInfoBll, IEventAggregator EventAggregator)
        {
            try
            {
                Initializations();
                _eventAggregator = EventAggregator;
                this.ButtonVisibility = "Hidden";
                SingleFloorCheckedCommand = new DelegateCommand(SingleFloorCheckedEvent);
                SingleFloorUnCheckedCommand = new DelegateCommand(SingleFloorUnCheckedEvent);
                MultipleFloorCheckedCommand = new DelegateCommand(MultipleFloorCheckedEvent);
                MultipleFloorUnCheckedCommand = new DelegateCommand(MultipleFloorUnCheckedEvent);
                _eventAggregator.GetEvent<cntrlTexthiding>().Subscribe(GetTypeTabVisibility);

                _eventAggregator.GetEvent<BeforeCreate>().Subscribe(OnBeforeCreate);
                LoadFloor = new DelegateCommand<int?>(OnMultiFloorLostFocus);
                RegularCheckedCommnd = new DelegateCommand(RegularCheckedEvent);
                _projectInfoBll = projctInfoBll;
                _controllerDAL = new CentralControllerDAL();
                //GetSystemTypeList();


                ShowCommand = new DelegateCommand(ShowMethod);
                HideCommand = new DelegateCommand(HideMethod);
                _eventAggregator.GetEvent<Cleanup>().Subscribe(OnCleanup);
            }
            catch (Exception ex)
            {
                int? id = CurrentProject?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnCleanup()
        {
            _eventAggregator.GetEvent<cntrlTexthiding>().Unsubscribe(GetTypeTabVisibility);
            _eventAggregator.GetEvent<Cleanup>().Unsubscribe(OnCleanup);
        }

        //for systems which dont have extra steps after type selection, system initialization happens in this method.TODO to find a better design.
        private void OnBeforeCreate()
        {
            try
            { 
            int sysTypeAsInt = Convert.ToInt32(SelectedsystemName.SystemID);
            if (sysTypeAsInt == 6)
            {
                WorkFlowContext.NewSystem = createCentralControllerSystem();
            }
            //unsubscribe the evenets.SHould be trigerred for all tabs.
            _eventAggregator.GetEvent<BeforeCreate>().Unsubscribe(OnBeforeCreate);
            }
            catch (Exception ex)
            {
                int? id = CurrentProject?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        #endregion
        #region Delegate Command Events
        private void OnMultiFloorLostFocus(int? value)
        {
           _eventAggregator.GetEvent<PubSubEvent<int?>>().Publish(value);
        }



        private Visibility _cntrlvisibility;
        public Visibility Cntrlvisibility
        {
            get { return this._cntrlvisibility; }
            set { this.SetValue(ref _cntrlvisibility, value); }
        }

        private void GetTypeTabVisibility(Visibility cntrlvisibility)
        {
            Cntrlvisibility = cntrlvisibility;
        }

        public void ShowMethod()
        {
            this.ButtonVisibility = "Visible";
            SetFloorVisibility(false);
        }
        public void ShowOduAndIduSystem()
        {
            Visibility IduOduVisibility = Visibility.Visible;
            if (SelectedsystemName != null)
            {
                if (SelectedsystemName.SystemID == "1")
                    IduOduVisibility = Visibility.Visible;
                else
                    IduOduVisibility = Visibility.Collapsed;

                _eventAggregator.GetEvent<OduIduVisibility>().Publish(IduOduVisibility);
            }

        }

        public Visibility FloorVisibility_bck = new Visibility();
        private void SetFloorVisibility(bool IsTrue)
        {
            Visibility FloorVisibility;
            if (IsTrue == true)
                FloorVisibility = Visibility.Collapsed;
            else
                FloorVisibility = Visibility.Visible;

            FloorVisibility_bck = FloorVisibility;
            _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Publish(FloorVisibility);
        }
        public void HideMethod()
        {
            this.ButtonVisibility = "Hidden";
            SetFloorVisibility(true);

        }
        private string btnVisibility;
        private IndoorBLL _indoorBll;

        public string ButtonVisibility
        {
            get
            {
                return btnVisibility;
            }
            set
            {
                this.SetValue(ref btnVisibility, value);
            }

        }


        //This will show/hide Create button when user selects different System types
        public void ShowCreateBtnOnClick()
        {
            Visibility CreateBtnVisibility = Visibility.Collapsed;
            if (SelectedsystemName != null)
            {
                if (SelectedsystemName.SystemID == "6")
                { 
                    CreateBtnVisibility = Visibility.Visible;                    
                }
                else                 
                    CreateBtnVisibility = Visibility.Collapsed;                

                _eventAggregator.GetEvent<CreateButtonVisibility>().Publish(CreateBtnVisibility);
            }
        }
        //ACC - RAG END

        //Acc start IA
        public void ShowSaveBtnOnClick()
        {
            Visibility SaveBtnVisibility = Visibility.Collapsed;
            if (SelectedsystemName != null)
            {
                if (SelectedsystemName.SystemID == "6")
                {
                    SaveBtnVisibility = Visibility.Visible;
                }
                else
                    SaveBtnVisibility = Visibility.Collapsed;

                _eventAggregator.GetEvent<SaveButton_Visibility>().Publish(SaveBtnVisibility);
            }
        }
        //Acc end IA


        //ACC - RAG START
        //This will show/hide next button when user selects different System types
        public void ShowNextBtnOnClick()
        {
            Visibility NextBtnVisibility = Visibility.Visible;
            if (SelectedsystemName != null)
            {
                if (SelectedsystemName.SystemID == "6")
                    NextBtnVisibility = Visibility.Collapsed;
                else
                    NextBtnVisibility = Visibility.Visible;


                _eventAggregator.GetEvent<NextButtonVisibility>().Publish(NextBtnVisibility);
            }
        }
        //ACC - RAG END

        private void SingleFloorCheckedEvent()
        {
            IsSingleFoorChecked = true;
            IsMultipleFloorChecked = false;

        }
        private void SingleFloorUnCheckedEvent()
        {
            IsSingleFoorChecked = false;

        }
        private void Initializations()
        {
            CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            IsSingleFoorChecked = true;
            IsRegular = true;
        }
        private void RegularCheckedEvent()
        {
            IsRegular = true;
        }
        private void MultipleFloorCheckedEvent()
        {
            IsMultipleFloorChecked = true;
            IsSingleFoorChecked = false;
        }
        private void MultipleFloorUnCheckedEvent()
        {
            IsMultipleFloorChecked = false;
        }
        private void TypeTabNextClick()
        {
            try
            { 
            if (SystemType())
            {
                _eventAggregator.GetEvent<TypeTabSubscriber>().Publish(true);
            }
            else
            {
                _eventAggregator.GetEvent<TypeTabSubscriber>().Publish(false);
            }
            }
            catch (Exception ex)
            {
                int? id = CurrentProject?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        bool SystemType()
        {
            try
            {
                string errorMessage = null;
                bool IsValid = ValidateSystemTypeValid(out errorMessage);
                if (IsValid)
                {

                    int sysTypeAsInt = Convert.ToInt32(SelectedsystemName.SystemID);
                    SystemBase system = null;
                    switch (sysTypeAsInt)
                    {
                        case 1:
                            system = createVRFSystem();
                            break;

                        case 2:
                            system = createHeatExchangerSystem();
                            break;
                    }
                    WorkFlowContext.NewSystem = system;
                    if (IsSingleFoorChecked && CurrentProject.FloorList.Count < 1)
                    {
                        CurrentProject.FloorList.Add(new JCHVRF.Model.Floor() { Name = "Floor 1", Id = "0" });
                    }
                    else if (IsMultipleFloorChecked)
                    {
                         CurrentProject.FloorList.Clear();
                        _eventAggregator.GetEvent<PubSubEvent<int?>>().Publish(FloorCount);
                    }
                }
                else
                {
                    JCHMessageBox.Show(errorMessage, MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                int? id = CurrentProject?.projectID;
                Logger.LogProjectError(id, ex);
            }
            return IsValid;
        }


        private SystemBase createHeatExchangerSystem()
        {
            string Count = (CurrentProject.HeatExchangerSystems.Count + 1).ToString();
            SystemBase system = new SystemHeatExchanger();
            
            system.Name = SystemName;
            system.HvacSystemType = SelectedsystemName.SystemID;
            return system;
        }

        private SystemBase createCentralControllerSystem()
        {
            string Count = (CurrentProject.ControlSystemList.Count + 1).ToString();
            SystemBase system = new ControlSystem();
            
            system.Name = SystemName;
            system.HvacSystemType = SelectedsystemName.SystemID;
            return system;
        }

        private SystemBase createVRFSystem()
        {
            
            string Count = (CurrentProject.SystemListNextGen.Count + 1).ToString();
            SystemBase system = new JCHVRF.Model.NextGen.SystemVRF
            {
                Name = SystemName,
                SystemTypeNexGen = SelectedsystemName.Name,
                HvacSystemType = SelectedsystemName.SystemID,
                IsActiveSystem = true,
                FloorCount = FloorCount,
                IsRegular = IsRegular,
                DBCooling = Convert.ToDouble(CurrentProject.DesignCondition.outdoorCoolingDB),
                DBHeating = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingDB),
                WBHeating = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingWB),
                IWCooling = Convert.ToDouble(CurrentProject.DesignCondition.outdoorCoolingIW),
                IWHeating = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingIW),
                RHHeating = Convert.ToDouble(CurrentProject.DesignCondition.outdoorHeatingRH),

                //  TO DO  // PLEASE DO NOT DELETE
             
                PipeEquivalentLength = SystemSetting.UserSetting.pipingSetting.pipingEqLength,
                PipeEquivalentLengthbuff = SystemSetting.UserSetting.pipingSetting.pipingEqLength,
                FirstPipeLength = SystemSetting.UserSetting.pipingSetting.firstBranchLength,
                FirstPipeLengthbuff = SystemSetting.UserSetting.pipingSetting.firstBranchLength,
                HeightDiff = SystemSetting.UserSetting.pipingSetting.pipingHighDifference,
                PipingLengthFactor = SystemSetting.UserSetting.pipingSetting.pipingCorrectionFactor,
                PipingPositionType = SystemSetting.UserSetting.pipingSetting.pipingPositionType,

            };
            return system;
        }

                #endregion
        #region helping Methods
        private ObservableCollection<SystemTypeItem> GetSystemTypeList()
        {
            SystemTypeCollection.Clear();
            List<Tuple<string, string, string>> getSystemTypeList = new List<Tuple<string, string, string>>();
            getSystemTypeList = _projectInfoBll.GetAllSystemType();
            
            getSystemTypeList.ForEach((item) =>
            {
                //"2" is HeatExchanger
                if (item.Item1.Equals("2"))
                {
                    
                    if(CurrentProjectRegionSupportsHeatExchanger())
                    {
                        SystemTypeCollection.Add(new SystemTypeItem { SystemID = item.Item1, Name = Language.Current.GetMessage("TOTAL_HEAT_EXCHANGER"), Path = item.Item3 });
                    }   
                }
                else if(item.Item1.Equals("1")) {

                    SystemTypeCollection.Add(new SystemTypeItem { SystemID = item.Item1, Name = Language.Current.GetMessage("VARIABLE_REF_FLOW"), Path = item.Item3 });
                }
                else
                {
                    SystemTypeCollection.Add(new SystemTypeItem { SystemID = item.Item1, Name = Language.Current.GetMessage("CENTRAL_CONTROLLER"), Path = item.Item3 });
                }
            });

            if (SystemTypeCollection != null && SystemTypeCollection.Count > 0)
                SelectedsystemName = SystemTypeCollection.FirstOrDefault();
            SystemName = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName + " " + (JCHVRF.Model.Project.CurrentProject.SystemListNextGen.Count + 1);
            CurrentProject.SystemName = SystemName; //takes name onto the system Name Texbox

            return SystemTypeCollection;
        }


        private bool CurrentProjectRegionSupportsHeatExchanger()
        {
            bool hasHeatExchangers = false;

            var thisProject = JCHVRF.Model.Project.CurrentProject;
            if (thisProject != null)
            {
                //判断是否存在Exchanger 类型
               // if (!thisProject.RegionCode.StartsWith("EU"))
               // {
                    //DataTable dt = indoor.GetExchangerTypeList(_mainRegion);
                    _indoorBll = new JCHVRF.BLL.IndoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode,JCHVRF.Model.Project.CurrentProject.RegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
                    DataTable dt = _indoorBll.GetExchangerTypeList();
                    if (dt != null)
                        hasHeatExchangers = dt.Rows.Count > 0;
              //  }
               
            }
            return hasHeatExchangers;
           
        }


        #endregion       
        #region validation methods
        private bool ValidateSystemTypeValid(out string errormessage)
        {

            bool IsValid = true;
            errormessage = "";
            if (string.IsNullOrEmpty(SystemName))
            {
                //----------------- Code below for multi-langauge------------//
                errormessage = Language.Current.GetMessage("ALERT_SYSTEM_NAME_BLANK");// "System Name cannot be blank";
                IsValid = false;

            }
            if (string.IsNullOrEmpty(SelectedsystemName?.Name))
            {
                //----------------- Code below for multi-langauge------------//
                errormessage = Language.Current.GetMessage("ALERT_SELECT_SYSTEM");//Select system type ";
                IsValid = false;

            }
            else if (IsMultipleFloorChecked)
            {
                if (FloorCount <= 0)
                {
                    //----------------- Code below for multi-langauge------------//
                    errormessage = Language.Current.GetMessage("ALERT_PLEASE_ADD");//Please add atleast one floor";
                    IsValid = false;
                }
            }

            return IsValid;
        }
        #endregion

        //ACC - RAG START
        public void ShowTheuInfoSystem()
        {
            Visibility HeuVisibility = Visibility.Visible;
            if (SelectedsystemName != null)
            {
                if (SelectedsystemName.SystemID.Equals("2"))
                    HeuVisibility = Visibility.Visible;
                else
                    HeuVisibility = Visibility.Collapsed;

                _eventAggregator.GetEvent<TheuInfoVisibility>().Publish(HeuVisibility);
            }
        }
        //ACC - RAG END

        //Accord - CC
        public void HideControllerTypeText()
        {
            Visibility Textboxhiding = Visibility.Visible;
            if (SelectedsystemName != null)
            {
                if (SelectedsystemName.SystemID == "6")
                {
                    if (FloorVisibility_bck == Visibility.Visible)
                        _eventAggregator.GetEvent<PubSubEvent<Visibility>>().Publish(Visibility.Collapsed);

                    Textboxhiding = Visibility.Collapsed;
                }
                else
                {
                    if (IsMultipleFloorChecked)
                    {
                        SetFloorVisibility(false);
                    }
                }
            }
            _eventAggregator.GetEvent<cntrlTexthiding>().Publish(Textboxhiding);
        }
    }
}
