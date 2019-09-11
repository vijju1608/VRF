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
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
namespace JCHVRF_New.ViewModels
{
    public class PropertyInfoViewModel : ViewModelBase
    {

        #region Delegate Commands
        JCHVRF.Model.Project CurrentProject;
        internal static string[] _strHEName;
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
                ShowOduAndIduSystem();
                HideControllerTypeText();
                ShowTheuInfoSystem(); //ACC - RAG
                ShowCreateBtnOnClick(); //ACC - RAG
                ShowSaveBtnOnClick();//Acc IA
                ShowNextBtnOnClick(); //ACC - RAG              
                WorkFlowContext.Systemid = SelectedsystemName.SystemID;
              
            }
        }

        private string _systemName;
        public string SystemName
        {
            get { return _systemName; }
            set { this.SetValue(ref _systemName, value);
                if (HvacSystem.HvacSystemType.Equals("2"))
                {
                    _strHEName = new string[2];
                    _strHEName[0] = HvacSystem.Id;
                    _strHEName[1] = value;
                }
            }
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
        #endregion
        #region constructor and Initisation
        public PropertyInfoViewModel(IProjectInfoBAL projctInfoBll, IEventAggregator EventAggregator)
        {
            Initializations();
            _eventAggregator = EventAggregator;
            this.ButtonVisibility = "Hidden";
            SingleFloorCheckedCommand = new DelegateCommand(SingleFloorCheckedEvent);
            SingleFloorUnCheckedCommand = new DelegateCommand(SingleFloorUnCheckedEvent);
            MultipleFloorCheckedCommand = new DelegateCommand(MultipleFloorCheckedEvent);
            MultipleFloorUnCheckedCommand = new DelegateCommand(MultipleFloorUnCheckedEvent);
            _eventAggregator.GetEvent<cntrlTexthiding>().Subscribe(GetTypeTabVisibility);
            _eventAggregator.GetEvent<BeforeSave>().Subscribe(OnBeforeSave);
            _eventAggregator.GetEvent<BeforeSaveVRF>().Subscribe(OnBeforeSaveVRF);

            LoadFloor = new DelegateCommand<int?>(OnMultiFloorLostFocus);
            RegularCheckedCommnd = new DelegateCommand(RegularCheckedEvent);
            _eventAggregator.GetEvent<TypeInfoTabNext>().Subscribe(TypeTabNextClick);
            _projectInfoBll = projctInfoBll;
            ShowCommand = new DelegateCommand(ShowMethod);
            HideCommand = new DelegateCommand(HideMethod);
            SaveClickCommand = new DelegateCommand(OnSaveClick);
            _eventAggregator.GetEvent<CleanupSystemWizard>().Subscribe(OnCleanup);
            loadFloorTabVisibility();
        }


        private void loadFloorTabVisibility()
        {
            JCHVRF.Model.NextGen.SystemVRF _currentSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(x => ((SystemBase)x).Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
            if (_currentSystem != null)
            {
                if (_currentSystem.FloorCount > 1)
                {
                    IsSingleFoorChecked = false;
                    IsMultipleFloorChecked = true;
                    FloorCount = _currentSystem.FloorCount;
                    ShowMethod();
                }
                else
                {
                    IsSingleFoorChecked = true;
                    IsMultipleFloorChecked = false;
                }
            }
        }
        private void OnCleanup()
        {
            _eventAggregator.GetEvent<cntrlTexthiding>().Unsubscribe(GetTypeTabVisibility);
            _eventAggregator.GetEvent<BeforeSave>().Unsubscribe(OnBeforeSave);
            _eventAggregator.GetEvent<TypeInfoTabNext>().Unsubscribe(TypeTabNextClick);
            _eventAggregator.GetEvent<Cleanup>().Unsubscribe(OnCleanup);
        }

        private void OnSaveClick()
        {
            if (!string.IsNullOrEmpty(JCHVRF.Model.Project.CurrentSystemId))
            {
                JCHVRF.Model.NextGen.SystemVRF _currentSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(x => ((SystemBase)x).Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
                if (_currentSystem != null)
                {
                    _currentSystem.Name = SystemName;
                }
              if(HvacSystem.HvacSystemType.Equals(2))
                    {
                    JCHVRF.Model.SystemHeatExchanger objHE = JCHVRF.Model.Project.CurrentProject.HeatExchangerSystems.Find(x => x.Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
                    objHE.Name = SystemName;
                }
              
            }
        }

        //for systems which dont have extra steps after type selection, system initialization happens in this method.TODO to find a better design.
        private void OnBeforeSave()
        {
            HvacSystem.Name = SystemName;
        }
        private void OnBeforeSaveVRF(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
           
            this.SystemName = SystemName;
            var index = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FindIndex(mm => mm.Name == currentSystem.Name);
            if (index != -1)
            {
                JCHVRF.Model.Project.CurrentProject.SystemListNextGen[index].Name = this.SystemName;
               
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
        private SystemBase _hvacSystem;

        public SystemBase HvacSystem
        {
            get { return _hvacSystem; }
            set
            {
                SetValue(ref _hvacSystem, value);
                GetSystemTypeList();
                this.SystemName = HvacSystem.Name;

            }
        }

        public DelegateCommand SaveClickCommand { get; private set; }


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

            _eventAggregator.GetEvent<TypeTabSubscriber>().Publish(true);

        }

        #endregion
        #region helping Methods
        private ObservableCollection<SystemTypeItem> GetSystemTypeList()
        {
            List<Tuple<string, string, string>> getSystemTypeList = new List<Tuple<string, string, string>>();
            getSystemTypeList = _projectInfoBll.GetAllSystemType();

            var i = getSystemTypeList.Find(x => x.Item1.Equals(HvacSystem.HvacSystemType));
            {
                var systemTypeItem = new SystemTypeItem { SystemID = i.Item1, Name = i.Item2, Path = i.Item3 };
                SelectedsystemName = systemTypeItem;
                SystemTypeCollection.Add(new SystemTypeItem { SystemID = i.Item1, Name = i.Item2, Path = i.Item3 });

            }
            return SystemTypeCollection;
        }
        #endregion       
        #region validation methods
        private bool ValidateSystemTypeValid(out string errormessage)
        {

            bool IsValid = true;
            errormessage = "";
            if (string.IsNullOrEmpty(SystemName))
            {
                errormessage = Langauge.Current.GetMessage("SYSNAMEBLANK");// "System Name cannot be blank";
                IsValid = false;

            }
            if (string.IsNullOrEmpty(SelectedsystemName.Name))
            {
                errormessage = Langauge.Current.GetMessage("SELECTSYS");// "Select system type ";
                IsValid = false;

            }
            else if (IsMultipleFloorChecked)
            {
                if (FloorCount <= 0)
                {
                    errormessage = Langauge.Current.GetMessage("PLEASEADD"); //"Please add atleast one floor";
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
                if (SelectedsystemName.SystemID.Equals("6") || SelectedsystemName.SystemID.Equals("2"))
                {
                    SetFloorVisibility(true);
                    Textboxhiding = Visibility.Collapsed;
                }
            }
            _eventAggregator.GetEvent<cntrlTexthiding>().Publish(Textboxhiding);
        }
    }
}
