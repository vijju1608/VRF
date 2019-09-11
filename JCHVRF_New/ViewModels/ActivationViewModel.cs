using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCBase.Utility;
using JCHVRF_New.Model;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using Registr;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using System.Windows.Input;
using System.Windows;
using JCHVRF.Model;
using System.Data;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class ActivationViewModel : ViewModelBase
    {
        private IRegionManager _regionManager;
        public DelegateCommand OkClickCommand { get; set; }
        public DelegateCommand CancelClickCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }

        public bool ISVALID_Region = false;
        public bool ISVALID_Date = false;
        private string _password;
        private string _statusText;

        public string StatusText
        {
            get { return this._statusText; }
            set { this.SetValue(ref _statusText, value); }
        }

        public string Password
        {
            get { return this._password; }
            set { this.SetValue(ref _password, value); }
        }
        public ActivationViewModel(IRegionManager regionManager)
        {
            try
            {
                _regionManager = regionManager;
                OkClickCommand = new DelegateCommand(OnActivationClicked);
                CancelClickCommand = new DelegateCommand(OnCancelClicked);
                //StatusText = "Activation Password";
                StatusText = Language.Current.GetMessage("ALERT_ACTIVATION_PASSWORD"); 

                LoadedCommand = new DelegateCommand(() =>
                {
                    if (checkRegistration())
                    {
                        _regionManager.RequestNavigate(RegionNames.MainAppRegion, ViewKeys.MainApp);
                    }
                });
            }
            catch (Exception ex)
            {
                //int? id = Project?.projectID;
                Logger.LogProjectError(null,ex);
            }
        }
        private void OnActivationClicked()
        {
            Registration.DoValidation(Password);
            try
            {
                if (checkRegistration())
                {
                    //StatusText = "Please wait. Customizing it for you...";
                    StatusText = Language.Current.GetMessage("ALERT_PLEASE_WAIT");
                    Mouse.OverrideCursor = Cursors.Wait;
                    _regionManager.RequestNavigate(RegionNames.MainAppRegion, ViewKeys.MainApp);
                }
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }
        private void OnCancelClicked()
        {
            Environment.Exit(0);
        }
        private bool checkRegistration()
        {
            bool validRegistration = false;

            if (Registration.IsValid())
            {
                try
                {
                    ISVALID_Region = true;
                    ISVALID_Date = true;
                    validRegistration = true;

                    if (string.IsNullOrEmpty(SystemSetting.UserSetting.locationSetting.region))
                    {
                        if (Registration.IsSuperUser())
                        {
                            SystemSetting.UserSetting.locationSetting.region = "ASEAN";
                            SystemSetting.UserSetting.locationSetting.subRegion = "ASIA";
                        }
                        else
                        {
                            //Region
                            string regionCode = Registration.GetRegionCode();
                            SystemSetting.UserSetting.locationSetting.region = regionCode;

                            //SubRegion
                            JCHVRF.BLL.RegionBLL objRegionBll = new JCHVRF.BLL.RegionBLL();
                            DataTable dtSubRegion = objRegionBll.GetSubRegionList(regionCode);
                            if (dtSubRegion.Rows.Count > 0)
                                SystemSetting.UserSetting.locationSetting.subRegion = dtSubRegion.Rows[0].ItemArray[0].ToString();
                        }
                        SystemSetting.Serialize();
                    }
                    //this.DialogResult = DialogResult.OK;
                    //Close();
                }
                catch (Exception ex)
                {
                    Logger.LogProjectError(null, ex);
                }
            }
            return validRegistration;

        }
    }
}
