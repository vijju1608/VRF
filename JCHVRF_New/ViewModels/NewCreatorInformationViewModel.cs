using JCHVRF.BLL.New;
using JCHVRF.Model.New;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Constants;
using System.Text.RegularExpressions;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
namespace JCHVRF_New.ViewModels
{
    public class NewCreatorInformationViewModel : ViewModelBase
    {
        #region Fields
        private IProjectInfoBAL _projectInfoBll;
        public int creatorVal;
        private string _companyName;
        #endregion Fields
        public string CompanyName
        {
            get { return _companyName; }
            set { this.SetValue(ref _companyName, value); }
        }

        private string _streetAddress;
        public string StreetAddress
        {
            get { return _streetAddress; }
            set { this.SetValue(ref _streetAddress, value); }
        }

        private string _suburb;
        public string Suburb
        {
            get { return _suburb; }
            set { this.SetValue(ref _suburb, value); }
        }


        private string _townCity;
        public string TownCity
        {
            get { return _townCity; }
            set { this.SetValue(ref _townCity, value); }
        }

        private string _country;
        public string Country
        {
            get { return _country; }
            set { this.SetValue(ref _country, value); }
        }

        private string _gpsPosition;
        public string GpsPosition
        {
            get { return _gpsPosition; }
            set { this.SetValue(ref _gpsPosition, value); }
        }


       private string _contactName;
        //[Required(ErrorMessage = "Contact Name is Mandatory")]
        public string ContactName
        {
            get { return _contactName; }
            set { this.SetValue(ref _contactName, value); }
        }

        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set { this.SetValue(ref _phone, value); }
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
        private string _contactEmail;

        [EmailAddress(ErrorMessage = "Please Enter Valid Email")]
        public string ContactEmail
        {
            get { return _contactEmail; }
            set { this.SetValue(ref _contactEmail, value); }
        }

        private string _idNumber;
        private IEventAggregator _eventAggregator;
        private IModalWindowService _winService;

        [RegularExpression(@"!|@|#|\$|%|\?|\>|\<|\*", ErrorMessage = "Special characters not allowed.")]
        public string IdNumber
        {
            get { return _idNumber; }
            set { this.SetValue(ref _idNumber, value); }
        }

        public DelegateCommand AddNewCreatorClickedCommand { get; set; }

        public DelegateCommand CancelNewCreatorClickedCommand { get; set; }
        public NewCreatorInformationViewModel(IEventAggregator eventAggregator,IModalWindowService winService)
        {
            try
            {
                _eventAggregator = eventAggregator;
                _winService = winService;
                AddNewCreatorClickedCommand = new DelegateCommand(OnNewCreatorClickedCommand);
                CancelNewCreatorClickedCommand = new DelegateCommand(OnCancelClickedCommand);

                if (Application.Current.Properties["Value"] != null)
                {
                    if (Application.Current.Properties["SelectedCreator"] != null)
                    {
                        creatorVal = Convert.ToInt32(Application.Current.Properties["SelectedCreator"]);
                        GetCreatorDetails(creatorVal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }
        private void GetCreatorDetails(int val)
        {
            var bll = new ProjectInfoBLL();
            var CreatorDetails = bll.GetCreatorDetails(val);
            if (CreatorDetails != null)
            {
                CompanyName = CreatorDetails.CompanyName;
                StreetAddress = CreatorDetails.StreetAddress;
                TownCity = CreatorDetails.TownCity;
                Country = CreatorDetails.Country;
                GpsPosition = CreatorDetails.GpsPosition;
                ContactName = CreatorDetails.ContactName;
                ContactEmail = CreatorDetails.ContactEmail;
                IdNumber = CreatorDetails.IdNumber;
            }
        }
            
        private void OnCancelClickedCommand()
        {
            try
            {
                _winService.Close(ViewKeys.NewCreatorInformation);
                //objClosable.RequestClose();
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }

        }

        private void OnNewCreatorClickedCommand()
        {
            try
            {
                if (ValidateViewModel()==true)
                {
                    var bll = new ProjectInfoBLL();
                    var CreatorDetails = bll.GetCreatorDetails(creatorVal);
                    if (CreatorDetails != null)
                    {
                        Creator objCreator = new Creator();
                        _projectInfoBll = new ProjectInfoBLL();
                        objCreator.CompanyName = CompanyName;
                        objCreator.StreetAddress = StreetAddress;
                        objCreator.Suburb = Suburb;
                        objCreator.TownCity = TownCity;
                        objCreator.Country = Country;
                        objCreator.GpsPosition = GpsPosition;
                        objCreator.ContactName = ContactName;
                        objCreator.Phone = Phone;
                        objCreator.ContactEmail = ContactEmail;
                        objCreator.IdNumber = IdNumber;
                        objCreator.Id = creatorVal;
                        var result = _projectInfoBll.UpdateCreatorInfo(objCreator);
                        if (result == 1)
                        {
                            _eventAggregator.GetEvent<AddCreatorPayLoad>().Publish();
                            JCHMessageBox.Show(Langauge.Current.GetMessage("UPDATE_SUCCESSFULLY"));//"Update Successfully"
                            ResetValue();
                            //objCreatorWindow.RequestClose();
                        }

                    }
                    else
                    {
                        Creator objCreator = new Creator();
                        _projectInfoBll = new ProjectInfoBLL();
                        objCreator.CompanyName = CompanyName;
                        objCreator.StreetAddress = StreetAddress;
                        objCreator.Suburb = Suburb;
                        objCreator.TownCity = TownCity;
                        objCreator.Country = Country;
                        objCreator.GpsPosition = GpsPosition;
                        objCreator.ContactName = ContactName;
                        objCreator.Phone = Phone;
                        objCreator.ContactEmail = ContactEmail;
                        objCreator.IdNumber = IdNumber;
                        var result = _projectInfoBll.InsertCreatorInfo(objCreator);
                        if (result == 1)
                        {
                            _eventAggregator.GetEvent<AddCreatorPayLoad>().Publish();
                            JCHMessageBox.Show(Langauge.Current.GetMessage("SAVE_SUCCESSFULLY"));// "Save Successfully");
                            ResetValue();
                            // objCreatorWindow.RequestClose();
                            _winService.Close(ViewKeys.NewCreatorInformation);
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
            this.CompanyName = "";
            this.StreetAddress = "";
            this.Suburb = "";
            this.TownCity = "";
            this.Country = "";
            this.GpsPosition = "";
            this.ContactName = "";
            this.Phone = "";
            this.ContactEmail = "";
            this.IdNumber = "";
        }


        private bool ValidateViewModel()
        {

            Regex regex = new Regex("^[A-Za-z0-9- ]+$");
            Regex phoneregex = new Regex("^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-/s/./0-9]*$");

            ANCErrorMessage = string.Empty;
            if (!string.IsNullOrEmpty(CompanyName))
            {
                if (!string.IsNullOrWhiteSpace(CompanyName))
                {
                    Match match = regex.Match(CompanyName);
                    if (!match.Success)
                    {

                        ANCErrorMessage = Langauge.Current.GetMessage("COMPANY_NAME_VALIDATION");//"Company Name sholud be alphanumeric only."
                        //JCHMessageBox.Show("Company Name sholud be alphanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(StreetAddress))
            {
                if (!string.IsNullOrWhiteSpace(StreetAddress))
                {
                    Match match = regex.Match(StreetAddress);
                    if (!match.Success)
                    {
                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_STREET_VALIDATION");//"Street Name sholud be alphanumeric only."
                        //JCHMessageBox.Show("Street Name sholud be alphanumeric only.");
                        return false;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(Suburb))
            {
                if (!string.IsNullOrWhiteSpace(Suburb))
                {
                    Match match = regex.Match(Suburb);
                    if (!match.Success)
                    {

                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_SUBURB_VALIDATION");//"Suburb sholud be alphanumeric only."
                        // JCHMessageBox.Show("Suburb sholud be alphanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(TownCity))
            {
                if (!string.IsNullOrWhiteSpace(TownCity))
                {
                    Match matchtowncity = regex.Match(TownCity);
                    if (!matchtowncity.Success)
                    {
                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_TOWN_CITY_VALIDATION");//"Town/City Name Should be aplhanumeric only."
                        // JCHMessageBox.Show("Town/City Name Should be aplhanumeric only.");
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Country))
            {
                if (!string.IsNullOrWhiteSpace(Country))
                {
                    Match matchcountry = regex.Match(Country);
                    if (!matchcountry.Success)
                    {
                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_COUNTRY_NAME_VALIDATION");//Country Name Should be aplhanumeric only.
                        // JCHMessageBox.Show("Country Name Should be aplhanumeric only.");
                        return false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(ContactName))
            {

                ANCErrorMessage = Langauge.Current.GetMessage("CONTACT_NAME_EMPTY");//Contact Name should not be Empty.
                // JCHMessageBox.Show("Contact Name should not be Empty.");
                return false;

            }

            else
            {
                if (!string.IsNullOrEmpty(ContactName))
                {

                    Match matchcontact = regex.Match(ContactName);
                    if (!matchcontact.Success)
                    {

                        ANCErrorMessage = Langauge.Current.GetMessage("CONTACT_NAME_ALPHA_NUMERIC");//Contact Name Should be aplhanumeric only.
                        // JCHMessageBox.Show("Contact Name Should be aplhanumeric only.");
                        return false;
                    }

                }

            }

            if (!string.IsNullOrWhiteSpace(Phone))
            {
                if (!string.IsNullOrWhiteSpace(Phone))
                {
                    Match match = phoneregex.Match(Phone);
                    if (!match.Success)
                    {

                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_PHONE_VALIDATION");//"Phone Should be alphanumeric only."
                        // JCHMessageBox.Show("Phone Should be alphanumeric only.");
                        return false;
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(IdNumber))
            {
                if (!string.IsNullOrWhiteSpace(IdNumber))
                {
                    Match match = regex.Match(IdNumber);
                    if (!match.Success)
                    {

                        ANCErrorMessage = Langauge.Current.GetMessage("ERROR_ADDNEWCLIENT_ID_VALIDATION");//"Id Should be alphanumeric only."
                        // JCHMessageBox.Show("Id Should be alphanumeric only.");
                        return false;
                    }
                }

            }
            return true;
        }
        
            //return this.Validate("ContactName");
        //}
        
    }
}
