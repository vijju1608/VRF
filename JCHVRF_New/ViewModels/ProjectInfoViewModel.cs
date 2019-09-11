using JCHVRF.BLL;
using JCHVRF.BLL.New;
using JCHVRF.DAL.New;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.ViewModels;
using JCHVRF_New.Views;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JCHVRF_New.Common.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{
    public class ProjectInfoViewModel : ViewModelBase
    {
        #region Fields
        public static string checkDesignConditionVal;
        #endregion Fields

        private IProjectInfoBAL _projectInfoBll;
        public DelegateCommand NewClientClickedCommand { get; set; }
        public DelegateCommand NewCreatorClickedCommand { get; set; }
        public DelegateCommand ClientNameDBCommand { get; set; }

        public DelegateCommand CoolingCheckedCommand { get; set; }
        public DelegateCommand CoolingUnCheckedCommand { get; set; }
        public DelegateCommand HeatingCheckedCommand { get; set; }
        public DelegateCommand HeatingUnCheckedCommand { get; set; }
        public DelegateCommand BothCheckedCommand { get; set; }
        public DelegateCommand BothUnCheckedCommand { get; set; }
        public DelegateCommand HitachiCheckedCommand { get; set; }
        public DelegateCommand HitachiUnCheckedCommand { get; set; }
        public DelegateCommand YorkCheckedCommand { get; set; }
        public DelegateCommand YorkUnCheckedCommand { get; set; }
        public DelegateCommand SelectedDateChanged { get; set; }

        public DelegateCommand ProjectInfoWindowLoaded { get; set; }

        private string _lookupProjectName;
        JCHVRF.Model.Project CurrentProject;

        public string LookupProjectName
        {
            get { return _lookupProjectName; }
            set { this.SetValue(ref _lookupProjectName, value); }
        }

        //private string _lookupClientName;
        //public string LookupClientName
        //{
        //    get { return _lookupClientName; }
        //    set { this.SetValue(ref _lookupClientName, value); }
        //}
        private Visibility _txtVisibility;
        public Visibility TxtVisibility
        {
            get { return this._txtVisibility; }
            set { this.SetValue(ref _txtVisibility, value); }
        }

        private string _txtClientName;
        public string TxtClientName
        {
            get { return _txtClientName; }
            set { this.SetValue(ref _txtClientName, value); }
        }
        private string _txtCreatorName;
        public string TxtCreatorName
        {
            get { return _txtCreatorName; }
            set { this.SetValue(ref _txtCreatorName, value); }
        }

        private bool _isBrandSelectionEnabled = true;

        public bool IsBrandSelectionEnabled
        {
            get { return this._isBrandSelectionEnabled; }
            set { this.SetValue(ref _isBrandSelectionEnabled, value); }
        }

        //public List<ListBox> _listClient;
        //public List<ListBox> ListClient
        //{
        //    get
        //    {
        //        return _listClient;
        //    }
        //    set
        //    {
        //        this.SetValue(ref _listClient, value);
        //    }
        //}



        //public ObservableCollection<ListBox> _listCreatedBy;
        //public ObservableCollection<ListBox> ListCreatedBy
        //{
        //    get
        //    {
        //        return _listCreatedBy;
        //    }
        //    set
        //    {
        //        this.SetValue(ref _listCreatedBy, value);
        //    }
        //}



        public ObservableCollection<ListBox> _listBindClient;
        public ObservableCollection<ListBox> ListBindClient
        {
            get
            {
                return _listBindClient;
            }

            set
            {
                this.SetValue(ref _listBindClient, value);
            }
        }

        //public ObservableCollection<ListBox> _listBindCreator;
        //public ObservableCollection<ListBox> ListBindCreator
        //{
        //    get
        //    {
        //        return _listBindCreator;
        //    }

        //    set
        //    {
        //        this.SetValue(ref _listBindCreator, value);
        //    }
        //}
        private ObservableCollection<ComboBox> _listCreator;
        public ObservableCollection<ComboBox> ListCreator
        {
            get
            {
                return _listCreator;
            }
            set
            {
                this.SetValue(ref _listCreator, value);
            }
        }

        private ObservableCollection<ComboBox> _listClient;
        public ObservableCollection<ComboBox> ListClient
        {
            get
            {
                return _listClient;
            }
            set
            {
                this.SetValue(ref _listClient, value);
            }
        }

        private string _selectedClient;

        public string SelectedClient
        {
            get { return _selectedClient; }
            set { this.SetValue(ref _selectedClient, value); }
        }

        private string _selectedCreator;

        public string SelectedCreator
        {
            get { return _selectedCreator; }
            set { this.SetValue(ref _selectedCreator, value); }
        }


        //private ListBox _selectedListviewCreator;

        //public ListBox SelectedListviewCreator
        //{
        //    get { return _selectedListviewCreator; }
        //    set { this.SetValue(ref _selectedListviewCreator, value);
        //        if(_selectedListviewCreator!=null)
        //        this.TxtCreatorName = _selectedListviewCreator.DisplayName;
        //    }
        //}



        private ListBox _selectedListviewClient;

        public ListBox SelectedListviewClient
        {
            get { return _selectedListviewClient; }
            set
            {
                this.SetValue(ref _selectedListviewClient, value);
                if (_selectedListviewClient != null)
                    this.TxtClientName = _selectedListviewClient.DisplayName;
            }
        }

        private ListBox _selectedListviewCreator;

        public ListBox SelectedListviewCreator
        {
            get { return _selectedListviewCreator; }
            set
            {
                this.SetValue(ref _selectedListviewCreator, value);
                if (_selectedListviewCreator != null)
                    this.TxtCreatorName = _selectedListviewCreator.DisplayName;
            }
        }

        private bool _isCoolingChecked;

        public bool IsCoolingChecked
        {
            get { return _isCoolingChecked; }
            set { this.SetValue(ref _isCoolingChecked, value); }
        }


        private bool _isHeatingChecked;

        public bool IsHeatingChecked
        {
            get { return _isHeatingChecked; }
            set { this.SetValue(ref _isHeatingChecked, value); }
        }


        private bool _isBothChecked;

        public bool IsBothChecked
        {
            get { return _isBothChecked; }
            set { this.SetValue(ref _isBothChecked, value); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { this.SetValue(ref _notes, value); }
        }

        private bool _isHitachi;
        public bool IsHitachi
        {
            get { return _isHitachi; }
            set { this.SetValue(ref _isHitachi, value); }
        }

        private bool _isYork;
        public bool IsYork
        {
            get { return _isYork; }
            set { this.SetValue(ref _isYork, value); }
        }

        private bool _isEnableRemove;

        private bool _IsCheckSettingsVal;

        public bool IsCheckSettingsVal
        {
            get { return _IsCheckSettingsVal; }

            set { this.SetValue(ref _IsCheckSettingsVal, value); }
        }


        public bool IsEnableRemove
        {
            get { return _isEnableRemove; }
            set { this.SetValue(ref _isEnableRemove, value); }
        }
        private DateTime _deliveryDate;
        public DateTime DeliveryDate
        {
            get { return _deliveryDate; }
            set { this.SetValue(ref _deliveryDate, value); }
        }


        private IEventAggregator _eventAggregator;
        private IModalWindowService _winService;
        public ProjectInfoViewModel(IProjectInfoBAL projctInfoBll, IEventAggregator eventAggregator, IModalWindowService winService)
        {
            Initialization();
            _eventAggregator = eventAggregator;
            _winService = winService;
            CoolingCheckedCommand = new DelegateCommand(CoolingCheckedEvent);
            CoolingUnCheckedCommand = new DelegateCommand(CoolingUncheckedEvent);
            HeatingCheckedCommand = new DelegateCommand(HeatingCheckedEvent);
            HeatingUnCheckedCommand = new DelegateCommand(HeatingUncheckedEvent);
            BothCheckedCommand = new DelegateCommand(BothCheckedEvent);
            BothUnCheckedCommand = new DelegateCommand(BothUncheckedEvent);
            HitachiCheckedCommand = new DelegateCommand(HitachiCheckedEvent);
            HitachiUnCheckedCommand = new DelegateCommand(HitachiUncheckedEvent);
            YorkCheckedCommand = new DelegateCommand(YorkCheckedEvent);
            YorkUnCheckedCommand = new DelegateCommand(YorkUncheckedEvent);
            SelectedDateChanged = new DelegateCommand(DeliveryDateChangedEvent);
            ProjectInitialisation();
            _projectInfoBll = projctInfoBll;
            LookupProjectName = GetDefaultProjectName();
            this.PropertyChanged += ProjectInfoViewModel_PropertyChanged;
            NewClientClickedCommand = new DelegateCommand(OnNewClientClicked);

            NewCreatorClickedCommand = new DelegateCommand(OnNewCreatorClicked);

            ShowTextVisibility();
            ClientNameDBCommand = new DelegateCommand(OnClientNameKeyUpCommand);

            GetClientList();
            GetCreatorList();
            _eventAggregator.GetEvent<ProjectTypeInfoTabNext>().Subscribe(ProjectTypeNextClick);


            _eventAggregator.GetEvent<AddCreatorPayLoad>().Subscribe(RebindCreatorList);

            ProjectInfoWindowLoaded = new DelegateCommand(() =>
               {
                   _isOpenedFromProjectSettings = true;
                   IsCheckSettingsVal = true;
                   IsBrandSelectionEnabled = false;
                   Application.Current.Properties["Value"] = IsCheckSettingsVal;
                   checkDesignConditionVal = "true";
                   var proj = JCHVRF.Model.Project.GetProjectInstance;
                   LookupProjectName = proj.Name;
                   IsCoolingChecked = proj.IsCoolingModeEffective;
                   IsHeatingChecked = proj.IsHeatingModeEffective;
                   IsBothChecked = proj.IsBothMode;

                   Notes = proj.Remarks;
                   this.TxtClientName = proj.clientName;
                   this.TxtCreatorName = proj.CreatorName;
                   this.DeliveryDate = proj.DeliveryRequiredDate;
                   IsHitachiRdbVisible = proj.BrandCode;
                   TxtVisibility = Visibility.Collapsed;

                   if (proj.BrandCode == "H")
                   {
                       IsHitachi = true;
                   }
                   else
                   {
                       IsYork = true;
                   }
               }
               );
            _eventAggregator.GetEvent<ProjectSettingsSave>().Subscribe(SetProjectMode);
        }

        private void ShowTextVisibility()
        {
            if (string.IsNullOrEmpty(ProjectInfoViewModel.checkDesignConditionVal))//have to modify that condition for design condition control disable
            {
                TxtVisibility = Visibility.Visible;
            }
        }
     private void SetProjectMode()
        {
            JCHVRF.Model.Project.GetProjectInstance.IsCoolingModeEffective = IsCoolingChecked;
            JCHVRF.Model.Project.GetProjectInstance.IsHeatingModeEffective = IsHeatingChecked;
            JCHVRF.Model.Project.GetProjectInstance.IsBothMode = IsBothChecked;
        }
        private void ProjectInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var proj = JCHVRF.Model.Project.GetProjectInstance;
            switch (e.PropertyName)
            {

                case "LookupProjectName":
                    proj.Name = LookupProjectName;
                    break;
                //case "IsCoolingChecked":
                //    proj.IsCoolingModeEffective = IsCoolingChecked;
                //    break;
                //case "IsHeatingChecked":
                //    proj.IsHeatingModeEffective = IsHeatingChecked;
                //    break;
                //case "IsBothChecked":
                //    proj.IsBothMode = IsBothChecked;
                //    //if (!IsBothChecked && IsCoolingChecked)
                //    //    proj.IsHeatingModeEffective = false;
                //    //else if (!IsBothChecked && IsHeatingChecked)
                //    //    proj.IsCoolingModeEffective = false;
                //    //else if(proj.IsBothMode)
                //    //{
                //    //    proj.IsCoolingModeEffective = true;
                //    //    proj.IsHeatingModeEffective = true;
                //    //}
                //    break;
                case "Notes":
                    proj.Remarks = Notes;
                    break;

                case "TxtClientName":
                    proj.clientName = TxtClientName;
                    break;
                case "TxtCreatorName":
                    proj.CreatorName = TxtCreatorName;
                    break;

                case "DeliveryDate":
                    proj.DeliveryRequiredDate = DeliveryDate;
                    break;
                case "IsHitachiRdbVisible":
                    proj.BrandCode = IsHitachiRdbVisible;
                    break;

            }
        }

        private void OnClientNameKeyUpCommand()
        {
            string typedString = TxtClientName;
            ListBindClient = GetClientList(typedString);

            if (ListBindClient.Count > 0)
            {
                IsEnableRemove = true;
            }

            else
            {
                IsEnableRemove = false;

            }

        }
        private void RebindCreatorList()
        {
            GetCreatorList();
        }

        private string _isHitachiRdbVisible;

        public string IsHitachiRdbVisible
        {
            get { return _isHitachiRdbVisible; }
            set { this.SetValue(ref _isHitachiRdbVisible, value); }
        }

        private string _isHitachiImgVisible;

        public string IsHitachiImgVisible
        {
            get { return _isHitachiImgVisible; }
            set { this.SetValue(ref _isHitachiImgVisible, value); }
        }

        private string _isYorkRdbVisible;

        public string IsYorkRdbVisible
        {
            get { return _isYorkRdbVisible; }
            set { this.SetValue(ref _isYorkRdbVisible, value); }
        }

        private string _isYorkImgVisible;

        public string IsYorkImgVisible
        {
            get { return _isYorkImgVisible; }
            set { this.SetValue(ref _isYorkImgVisible, value); }
        }

        private string _PIErrorMsg;

        public string PIErrorMsg
        {
            get { return _PIErrorMsg; }
            set
            {
                this.SetValue(ref _PIErrorMsg, value);
                RaisePropertyChanged("PIErrorMsg");
                RaisePropertyChanged("PIError");
            }
        }

        private bool _PIError;
        private bool _isOpenedFromProjectSettings;

        public bool PIError
        {
            get { return !string.IsNullOrEmpty(PIErrorMsg); }
            //get { return _PIError; }
            //set { this.SetValue(ref _PIError, value); }
        }

        private void BindBrandCode()
        {
            MyProductTypeBLL bll = new MyProductTypeBLL();
            List<string> brandList = bll.GetBrandCodeList(CurrentProject.SubRegionCode.ToString());
            brandList.ForEach((item) =>
            {
                if (item.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsHitachiRdbVisible != "Visible")
                        IsYork = true;

                    IsYorkImgVisible = "Visible";
                    IsYorkRdbVisible = "Visible";
                }
                else if (item.Equals("H", StringComparison.OrdinalIgnoreCase))
                {
                    IsHitachiRdbVisible = "Visible";
                    IsHitachiImgVisible = "Visible";
                    IsYork = false;
                    IsHitachi = true;
                }
            });

        }

        private void ProjectTypeNextClick()
        {
            if (ProjectInfoToProjectLegacy())
            {
                _eventAggregator.GetEvent<ProjectInfoSubscriber>().Publish(true);
            }
            else
            {
                _eventAggregator.GetEvent<ProjectInfoSubscriber>().Publish(false);
            }

        }
        private void DeliveryDateChangedEvent()
        {
            // Do Validation for DeliveryDate
            DeliveryDate = this.DeliveryDate;
        }

        private void Initialization()
        {
            IsCoolingChecked = true;
            IsHitachi = true;
            //DeliveryDate = DateTime.Now;
        }
        private void CoolingUncheckedEvent()
        {
            IsCoolingChecked = false;
            if (!_isOpenedFromProjectSettings)
                JCHVRF.Model.Project.GetProjectInstance.IsCoolingModeEffective = false;
        }

        private void CoolingCheckedEvent()
        {
            IsCoolingChecked = true;
            if (!_isOpenedFromProjectSettings)
                JCHVRF.Model.Project.GetProjectInstance.IsCoolingModeEffective = true;
            //IsHeatingChecked = false;
            //IsBothChecked = false;
        }
        private void HeatingUncheckedEvent()
        {
            IsHeatingChecked = false;
            if(!_isOpenedFromProjectSettings)
            JCHVRF.Model.Project.GetProjectInstance.IsHeatingModeEffective = false;
        }

        private void HeatingCheckedEvent()
        {
            IsHeatingChecked = true;
            if (!_isOpenedFromProjectSettings)
                JCHVRF.Model.Project.GetProjectInstance.IsHeatingModeEffective = true;
            //IsCoolingChecked = false;
            //IsBothChecked = false;
        }
        private void BothUncheckedEvent()
        {
            IsBothChecked = false;
            
            if (!_isOpenedFromProjectSettings) { 
                JCHVRF.Model.Project.GetProjectInstance.IsHeatingModeEffective = false;
            //JCHVRF.Model.Project.GetProjectInstance.IsCoolingModeEffective = false;
            //JCHVRF.Model.Project.GetProjectInstance.IsBothMode = false;
            }
        }

        private void BothCheckedEvent()
        {
            IsBothChecked = true;
           
            if (!_isOpenedFromProjectSettings) { 
                JCHVRF.Model.Project.GetProjectInstance.IsHeatingModeEffective = true;
            //JCHVRF.Model.Project.GetProjectInstance.IsCoolingModeEffective = true;
            //JCHVRF.Model.Project.GetProjectInstance.IsBothMode = true;
            }
        }

        private void HitachiUncheckedEvent()
        {
            IsHitachi = false;
        }

        private void HitachiCheckedEvent()
        {
            IsHitachi = true;
        }

        private void YorkUncheckedEvent()
        {
            IsYork = false;
        }

        private void YorkCheckedEvent()
        {
            IsYork = true;
        }

        private void OnNewClientClicked()
        {
            string clientVal = SelectedClient;
            Application.Current.Properties["SelectedClient"] = clientVal;
            _winService.ShowView(ViewKeys.Addnewclient,"Add New Client");
            GetClientName();

        }

        private void OnNewCreatorClicked()
        {
            string creatorVal = SelectedCreator;
            Application.Current.Properties["SelectedCreator"] = creatorVal;
            _winService.ShowView(ViewKeys.NewCreatorInformation,"Add New Creator");

            //NewCreatorInformation newCreatorWindow = new NewCreatorInformation();
            //newCreatorWindow.ShowDialog();
        }

        public string GetDefaultProjectName()
        {
            return _projectInfoBll.GetDefaultProjectName();
        }

        public void GetClientName()
        {
            if (Application.Current.Properties["TxtContactName"] != null)
                //TxtClientName = Application.Current.Properties["TxtContactName"].ToString();
                GetClientList();

        }



        public ObservableCollection<ComboBox> GetClientList()
        {
            List<Tuple<string, string>> getClientList = null;
            ListClient = new ObservableCollection<ComboBox>();
            getClientList = _projectInfoBll.GetClientInfo();
            getClientList.ForEach((item) =>
            {
                ListClient.Add(new ComboBox { DisplayName = item.Item2, Value = item.Item1 });
            });
            return ListClient;
        }


        public ObservableCollection<ComboBox> GetCreatorList()
        {
            List<Tuple<string, string>> getCreatorList = null;
            ListCreator = new ObservableCollection<ComboBox>();
            getCreatorList = _projectInfoBll.GetCreatorInfo();
            getCreatorList.ForEach((item) =>
            {
                ListCreator.Add(new ComboBox { DisplayName = item.Item2, Value = item.Item1 });
            });
            return ListCreator;
        }


        public ObservableCollection<ListBox> GetClientList(string typeText)
        {
            List<Tuple<string, string>> getClientList = null;
            ListBindClient = new ObservableCollection<ListBox>();
            getClientList = _projectInfoBll.GetClientInfoList(typeText);
            if (getClientList != null)
            {
                getClientList.ForEach((item) =>
                {
                    //ListClient.Add(new ListBox { DisplayName = item });
                    ListBindClient.Add(new ListBox { Value = item.Item1, DisplayName = item.Item2, });

                });

            }

            return ListBindClient;
        }

        void ProjectInitialisation()
        {
            CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            if (CurrentProject.DeliveryRequiredDate != null)
            {
                DeliveryDate = CurrentProject.DeliveryRequiredDate;
            }
            IsHitachiImgVisible = "Collapsed";
            IsHitachiRdbVisible = "Collapsed";
            IsYorkImgVisible = "Collapsed";
            IsYorkRdbVisible = "Collapsed";
            BindBrandCode();
        }
        bool ProjectInfoToProjectLegacy()
        {
            string errorMessage = null;
            bool IsValid = ValidateProjectDataValid(out errorMessage);
            if (IsValid)
            {
                CurrentProject.Name = LookupProjectName;
                //CurrentProject.clientName = SelectedClient;
                CurrentProject.clientName = this.TxtClientName;
                CurrentProject.CreatorName = this.TxtCreatorName;
                CurrentProject.CreatorId = Convert.ToInt32(SelectedCreator);
                CurrentProject.ClientId = Convert.ToInt32(SelectedClient);
                CurrentProject.DeliveryRequiredDate = Convert.ToDateTime(DeliveryDate);
                CurrentProject.Remarks = string.IsNullOrEmpty(Notes) ? string.Empty : Notes;
                if (IsBothChecked)
                {
                    CurrentProject.IsCoolingModeEffective = true;
                    CurrentProject.IsHeatingModeEffective = true;
                }
                else if (IsHeatingChecked)
                {
                    CurrentProject.IsHeatingModeEffective = true;
                    CurrentProject.IsCoolingModeEffective = false;
                }
                else if (IsCoolingChecked)
                {
                    CurrentProject.IsCoolingModeEffective = true;
                    CurrentProject.IsHeatingModeEffective = false;
                }
                if (IsHitachi)
                    CurrentProject.BrandCode = "H";
                if (IsYork)
                    CurrentProject.BrandCode = "Y";
                CurrentProject.FactoryCode = "S";
                //projectLegacy.IsProjectTypeTabValid = true;
            }
            else
            {
                //JCHMessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //projectLegacy.IsProjectTypeTabValid = true;
            }
            return IsValid;

            //  projectLegacy.FactoryCode = "S";
        }


        public bool ValidateProjectDataValid(out string errormessage)
        {

            bool IsValid = true;
            errormessage = "";
            PIErrorMsg = string.Empty;
            string specialCharacters = @"\*?:'$%#!^&*|" + "\"";
            char[] specialCharactersArray = specialCharacters.ToCharArray();
            if (string.IsNullOrEmpty(LookupProjectName.Trim()))
            {
                PIErrorMsg = Langauge.Current.GetMessage("PROJECT_NAME_CANNOT_BLANK"); //"Project Name cannot be blank";
                //errormessage = "Project Name cannot be blank";
                IsValid = false;

            }
            else if (LookupProjectName.IndexOfAny(specialCharactersArray) > -1)
            {
                PIErrorMsg = Langauge.Current.GetMessage("PROJECT_NAME_CANNOT_CHARACTERS") + specialCharacters.ToString();//"Project Name cannot contain any of the following characters: "
                // errormessage = "Project Name cannot contain any of the following characters: " + specialCharacters.ToString();
                IsValid = false;
            }

            //else if (string.IsNullOrEmpty(SelectedClient))
            //{
            //    errormessage = "Client name cannot be blank";
            //    IsValid = false;

            //}
            //else if (string.IsNullOrEmpty(SelectedCreator))
            //{
            //    errormessage = "Creator Name cannot be blank";
            //    IsValid = false;

            //}
            else if (string.IsNullOrEmpty(Convert.ToString(DeliveryDate)))
            {
                PIErrorMsg = Langauge.Current.GetMessage("DELIVERY_DATE_BLANK");// "Delivery Date cannot be blank";
                //errormessage = "Delivery Date cannot be blank";
                IsValid = false;
            }
            else
            {
                DateTime dateValue;
                if (DateTime.TryParse(Convert.ToString(DeliveryDate), out dateValue))
                {
                    if (DeliveryDate < DateTime.Now.Date)
                    {
                        PIErrorMsg = Langauge.Current.GetMessage("DELIVERY_DATE_LESSTHAN_CURRENTDATE");// "Delivery Date cannot be less than Current date";
                        // errormessage = "Delivery Date cannot be less than Current date";
                        DeliveryDate = DateTime.Now;
                        IsValid = false;
                    }
                }
                else
                {
                    PIErrorMsg = Langauge.Current.GetMessage("PLEASE_ENTER_VALID_DATE");// "Please enter valid date";
                    // errormessage = "Please enter valid date";
                    IsValid = false;
                }
            }
            return IsValid;
        }
    }
}
