using JCHVRF_New.Common.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using System.Text.RegularExpressions;
using JCHVRF_New.Common.Contracts;
using JCHVRF.BLL.New;
using Prism.Events;
using JCHVRF_New.Model;
using JCHVRF.Model.New;
using System.Windows;
using JCHVRF_New.Views;
using System.Windows.Input;
using System.Data;
using JCHVRF.DALFactory;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{
    public class AddNewClientViewModel : ViewModelBase
    {

        #region Fields

        public int clientVal;
        #endregion Fields
        private IEventAggregator _eventAggregator;
        private IModalWindowService _winService;
        private IProjectInfoBAL _projectInfoBll;
        #region Delegate Commands
        public DelegateCommand EmailValidationDBCommand { get; set; }
        public DelegateCommand CancelNewClientClickedCommand { get; set; }
        public DelegateCommand<TextCompositionEventArgs> PhoneValidationDBCommand { get; set; }
        public DelegateCommand AddNewClientClickedCommand { get; set; }



        #endregion Delegate Commands
        public AddNewClientViewModel(IEventAggregator eventAggregator,IModalWindowService winService)
        {
            try
            {
                _eventAggregator = eventAggregator;
                _winService = winService;
                EmailValidationDBCommand = new DelegateCommand(EmailId_LostFocus);
                CancelNewClientClickedCommand = new DelegateCommand(OnCancelClickedCommand);
                PhoneValidationDBCommand = new DelegateCommand<TextCompositionEventArgs>(PhoneNumber_LostFocus);
                AddNewClientClickedCommand = new DelegateCommand(OnNewClientClickedCommand);

                if (Application.Current.Properties["Value"] != null)
                {
                    if (Application.Current.Properties["SelectedClient"] != null)
                    {
                        clientVal = Convert.ToInt32(Application.Current.Properties["SelectedClient"]);
                        GetClientDetails(clientVal);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }

        }

        #region Properties

        private string _txtCompanyName;
        public string TxtCompanyName
        {
            get { return _txtCompanyName; }
            set { this.SetValue(ref _txtCompanyName, value); }
        }


        private string _Errormsg;
        public string ANCErrorMessage
        {
            get { return _Errormsg; }
            set
            {
                this.SetValue(ref _Errormsg, value);
                RaisePropertyChanged("IsError");
            }
        }
        public bool IsError
        {
            get { return !string.IsNullOrEmpty(ANCErrorMessage); }
        }


        private string _txtStreetAddress;
        public string TxtStreetAddress
        {
            get { return _txtStreetAddress; }
            set { this.SetValue(ref _txtStreetAddress, value); }
        }

        private string _txtSuburb;
        public string TxtSuburb
        {
            get { return _txtSuburb; }
            set { this.SetValue(ref _txtSuburb, value); }
        }


        private string _txtTownCity;
        public string TxtTownCity
        {
            get { return _txtTownCity; }
            set { this.SetValue(ref _txtTownCity, value); }
        }

        private string _txtCountry;
        public string TxtCountry
        {
            get { return _txtCountry; }
            set { this.SetValue(ref _txtCountry, value); }
        }

        private string _txtGpsPosition;
        public string TxtGpsPosition
        {
            get { return _txtGpsPosition; }
            set { this.SetValue(ref _txtGpsPosition, value); }
        }


        private string _txtExistingClient;
        public string TxtExistingClient
        {
            get { return _txtExistingClient; }
            set { this.SetValue(ref _txtExistingClient, value); }
        }

        private string _txtPhone;
        [Phone]
        [Required(ErrorMessage = "Filling this is Mandatory")]
        public string TxtPhone
        {
            get { return string.IsNullOrEmpty(_txtPhone) ? "" : _txtPhone; }
            set { this.SetValue(ref _txtPhone, value); }
        }

        private string _txtContactEmail;

        [EmailAddress(ErrorMessage = "Email Address is not Valid")]
        public string TxtContactEmail
        {
            get { return string.IsNullOrEmpty(_txtContactEmail) ? "" : _txtContactEmail; }
            set { this.SetValue(ref _txtContactEmail, value); }
        }


        private string _lblContactEmail;
        public string lblContactEmail
        {
            get
            {
                return _lblContactEmail;
            }
            set
            {
                this.SetValue(ref _lblContactEmail, value);
            }
        }

        private string _lblPhoneNumber;
        public string lblPhoneNumber
        {
            get
            {
                return _lblPhoneNumber;
            }
            set
            {
                this.SetValue(ref _lblPhoneNumber, value);
            }
        }
        private string _txtContactName;
        [Required(ErrorMessage = "Contact Name is Mandatory")]
        public string TxtContactName
        {
            get { return _txtContactName; }
            set { this.SetValue(ref _txtContactName, value); }
        }

        private string _txtIdNumber;
        // private IEventAggregator _eventAggregator;

        //[RegularExpression(@"!|@|#|\$|%|\?|\>|\<|\*", ErrorMessage = "Special characters not allowed.")]
        public string TxtIdNumber
        {
            get { return _txtIdNumber; }
            set { this.SetValue(ref _txtIdNumber, value); }
        }

        public string ErrorMessage { get; private set; }

        #endregion Properties



        #region Methods

        private void OnNewClientClickedCommand()
        {
            try
            {
                if (ValidateViewModel() == true && ValidateEmailId() == true)
                {
                    var bll = new ProjectInfoBLL();
                    var ClientDetails = bll.GetClientDetails(clientVal);
                    if (ClientDetails != null)
                    {
                        Client objClient = new Client();
                        _projectInfoBll = new ProjectInfoBLL();
                        objClient.CompanyName = TxtCompanyName;
                        objClient.ContactName = TxtContactName;
                        objClient.StreetAddress = TxtStreetAddress;
                        objClient.Suburb = TxtSuburb;
                        objClient.TownCity = TxtTownCity;
                        objClient.Country = TxtCountry;
                        objClient.GpsPosition = TxtGpsPosition;
                        objClient.Phone = TxtPhone;
                        objClient.ContactEmail = TxtContactEmail;
                        objClient.IdNumber = TxtIdNumber;
                        objClient.Id = clientVal;
                        var result = _projectInfoBll.UpdateClientInfo(objClient);
                        if (result == 1)
                        {
                            _eventAggregator.GetEvent<AddCreatorPayLoad>().Publish();
                            JCHMessageBox.Show(Langauge.Current.GetMessage("UPDATE_SUCCESSFULLY"));//Update Successfully
                            Application.Current.Properties["TxtContactName"] = objClient.ContactName;
                            ResetValue(); 
                            _winService.Close(ViewKeys.Addnewclient);
                            //objClientWindow.RequestClose();
                        }
                    }
                    else
                    {
                        Client objClient = new Client();
                        _projectInfoBll = new ProjectInfoBLL();
                        objClient.CompanyName = TxtCompanyName;
                        objClient.ContactName = TxtContactName;
                        objClient.StreetAddress = TxtStreetAddress;
                        objClient.Suburb = TxtSuburb;
                        objClient.TownCity = TxtTownCity;
                        objClient.Country = TxtCountry;
                        objClient.GpsPosition = TxtGpsPosition;
                        objClient.Phone = TxtPhone;
                        objClient.ContactEmail = TxtContactEmail;
                        objClient.IdNumber = TxtIdNumber;
                        var result = _projectInfoBll.InsertClientInfo(objClient);
                        if (result == 1)
                        {
                            _eventAggregator.GetEvent<AddCreatorPayLoad>().Publish();
                            JCHMessageBox.Show(Langauge.Current.GetMessage("SAVE_SUCCESSFULLY"));//Save Successfully
                            Application.Current.Properties["TxtContactName"] = objClient.ContactName;
                            ResetValue();
                            _winService.Close(ViewKeys.Addnewclient);
                           // objClientWindow.RequestClose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }

        private void ResetValue()
        {
            this.TxtCompanyName = "";
            this.TxtStreetAddress = "";
            this.TxtSuburb = "";
            this.TxtTownCity = "";
            this.TxtCountry = "";
            this.TxtGpsPosition = "";
            this.TxtContactName = "";
            this.TxtPhone = "";
            this.TxtContactEmail = "";
            this.TxtIdNumber = "";
        }
        private void OnCancelClickedCommand()
        {

            _winService.Close(ViewKeys.Addnewclient);

        }
        private void NewClientOpenWindowCommand()
        {
            AddNewClient objClient = new AddNewClient();
           //objClient.ShowDialog();
        }
        private bool ValidateEmailId()
        {
            if (GetErrors(nameof(TxtContactEmail)).Cast<string>().Count() > 0)
            {
                ANCErrorMessage = GetErrors(nameof(TxtContactEmail)).Cast<string>().First();
                return false;
            }
            else
            {
                ANCErrorMessage = "";
                return true;
            }
        }
        private bool ValidatePhoneNumber(TextCompositionEventArgs e)
        {

            Regex regex = new Regex(@"\+?[0-9]{10}");
            e.Handled = regex.IsMatch(e.Text);

            return false;

        }


        private void EmailId_LostFocus()
        {
            ValidateEmailId();
        }

        private void GetClientDetails(int val)
        {
            var bll = new ProjectInfoBLL();
            var ClientDetails = bll.GetClientDetails(val);
            if (ClientDetails != null)
            {
                TxtCompanyName = ClientDetails.CompanyName;
                TxtContactName = ClientDetails.ContactName;
                TxtStreetAddress = ClientDetails.StreetAddress;
                TxtSuburb = ClientDetails.Suburb;
                TxtTownCity = ClientDetails.TownCity;
                TxtCountry = ClientDetails.Country;
                TxtGpsPosition = ClientDetails.GpsPosition;
                TxtContactEmail = ClientDetails.ContactEmail;
                TxtIdNumber = ClientDetails.IdNumber;
            }
        }

        private void PhoneNumber_LostFocus(TextCompositionEventArgs e)
        {
            if (ValidatePhoneNumber(e) == false)
            {

            }
        }
        private bool ValidateViewModel()
        {
            Regex regex = new Regex("^[A-Za-z0-9- ]+$");
            Regex phoneregex = new Regex("^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-/s/./0-9]*$");

            ANCErrorMessage = string.Empty;
            if (!string.IsNullOrEmpty(TxtCompanyName))
            {
                if (!string.IsNullOrWhiteSpace(TxtCompanyName))
                {
                    Match match = regex.Match(TxtCompanyName);
                    if (!match.Success)
                    {

                        ANCErrorMessage = Langauge.Current.GetMessage("COMPANY_NAME_VALIDATION");//"Company Name sholud be alphanumeric only."
                        //JCHMessageBox.Show("Company Name sholud be alphanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(TxtStreetAddress))
            {
                if (!string.IsNullOrWhiteSpace(TxtStreetAddress))
                {
                    Match match = regex.Match(TxtStreetAddress);
                    if (!match.Success)
                    {
                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_STREET_VALIDATION");//"Street Name sholud be alphanumeric only."
                        //JCHMessageBox.Show("Street Name sholud be alphanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(TxtSuburb))
            {
                if (!string.IsNullOrWhiteSpace(TxtSuburb))
                {
                    Match match = regex.Match(TxtSuburb);
                    if (!match.Success)
                    {
                        
                           ANCErrorMessage =Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_SUBURB_VALIDATION");//"Suburb sholud be alphanumeric only."
                        // JCHMessageBox.Show("Suburb sholud be alphanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(TxtTownCity))
            {
                if (!string.IsNullOrWhiteSpace(TxtTownCity))
                {
                    Match matchtowncity = regex.Match(TxtTownCity);
                    if (!matchtowncity.Success)
                    {                        
                           ANCErrorMessage =Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_TOWN_CITY_VALIDATION");//"Town/City Name Should be aplhanumeric only."
                        // JCHMessageBox.Show("Town/City Name Should be aplhanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(TxtCountry))
            {
                if (!string.IsNullOrWhiteSpace(TxtCountry))
                {
                    Match matchcountry = regex.Match(TxtCountry);
                    if (!matchcountry.Success)
                    {                        
                           ANCErrorMessage =Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_COUNTRY_NAME_VALIDATION");//Country Name Should be aplhanumeric only.
                        // JCHMessageBox.Show("Country Name Should be aplhanumeric only.");
                        return false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(TxtContactName))
            {               

                ANCErrorMessage =Langauge.Current.GetMessage("CONTACT_NAME_EMPTY");//Contact Name should not be Empty.
                // JCHMessageBox.Show("Contact Name should not be Empty.");
                return false;

            }

            else
            {
                if (!string.IsNullOrEmpty(TxtContactName))
                {

                    Match matchcontact = regex.Match(TxtContactName);
                    if (!matchcontact.Success)
                    {
                        
                           ANCErrorMessage =Langauge.Current.GetMessage("CONTACT_NAME_ALPHA_NUMERIC");//Contact Name Should be aplhanumeric only.
                        // JCHMessageBox.Show("Contact Name Should be aplhanumeric only.");
                        return false;
                    }

                }

            }

            if (!string.IsNullOrWhiteSpace(TxtPhone))
            {
                if (!string.IsNullOrWhiteSpace(TxtPhone))
                {
                    Match match = phoneregex.Match(TxtPhone);
                    if (!match.Success)
                    {
                        
                           ANCErrorMessage =Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_PHONE_VALIDATION");//"Phone Should be alphanumeric only."
                        // JCHMessageBox.Show("Phone Should be alphanumeric only.");
                        return false;
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(TxtIdNumber))
            {
                if (!string.IsNullOrWhiteSpace(TxtIdNumber))
                {
                    Match match = regex.Match(TxtIdNumber);
                    if (!match.Success)
                    {
                        
                           ANCErrorMessage =Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_ID_VALIDATION");//"Id Should be alphanumeric only."
                        // JCHMessageBox.Show("Id Should be alphanumeric only.");
                        return false;
                    }
                }

            }
            return true;
        }
    }
}
#endregion Methods

