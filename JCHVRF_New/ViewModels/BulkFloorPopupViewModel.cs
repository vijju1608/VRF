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
using System.Windows.Input;
using System.Text.RegularExpressions;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{ 
    public class BulkFloorPopupViewModel : ViewModelBase
    {
        #region Deleget Commands
        public DelegateCommand PlushClickCommand {get;set;}
        public DelegateCommand MinusClickCommand { get; set; }
        public DelegateCommand CancelBulkPopupClickedCommand { get; set; }
        public DelegateCommand AddBulkPopupClickedCommand { get; set; }
        public DelegateCommand<TextCompositionEventArgs> BulkTextChangedCommand { get; set; }
        #endregion
        #region Property Member
        private string floorCount = string.Empty;
        private IEventAggregator _eventAggregator;
        private string txtBulkAdd;
        private IModalWindowService _winService;

        public string TxtBulkAdd
        {
            get { return txtBulkAdd; }
            set {
                this.SetValue(ref txtBulkAdd, value);
            }
        }
        #endregion

        #region constructor
        public BulkFloorPopupViewModel(IEventAggregator eventAggregator, IModalWindowService winService)
        {
            try
            {
                _eventAggregator = eventAggregator;
                _winService = winService;
                PlushClickCommand = new DelegateCommand(PlushClick);
                MinusClickCommand = new DelegateCommand(MinusClick);
                CancelBulkPopupClickedCommand = new DelegateCommand(OnCancelBulkPopupClickedCommand);
                AddBulkPopupClickedCommand = new DelegateCommand(OnAddBulkPopupClickedCommand);
                BulkTextChangedCommand = new DelegateCommand<TextCompositionEventArgs>(OnBulkTextChangedCommand);
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }

        private void OnBulkTextChangedCommand(TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);           
        }
        #endregion

        #region command methods
        private void OnAddBulkPopupClickedCommand()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TxtBulkAdd))
                {
                    if (Convert.ToInt32(TxtBulkAdd) < 1000)
                    {
                        _eventAggregator.GetEvent<PubSubEvent<string>>().Publish(TxtBulkAdd);
                    }
                    else
                    {
                        //JCHMessageBox.Show("You can enter maximum 1000 floor");
                        //---------------- Below code added for multi-language----------//
                        JCHMessageBox.Show(Language.Current.GetMessage("ALERT_MAXIMUM_FLOOR"));
                    }
                    _winService.Close(ViewKeys.BulkFloorPopup);
                }
                else
                {
                    //JCHMessageBox.Show("Please Enter Floor No");
                    //---------------- Below code added for multi-language----------//
                    JCHMessageBox.Show(Language.Current.GetMessage("ALERT_ENTER_FLOOR_NO"));                   
                }
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }
        private void OnCancelBulkPopupClickedCommand()
        {
            _winService.Close(ViewKeys.BulkFloorPopup);
        }
        private void MinusClick()
        {
            if (floorCount=="")
            {
                //JCHMessageBox.Show("No floors added to Remove");
                //---------------- Below code added for multi-language----------//
                JCHMessageBox.Show(Language.Current.GetMessage("ALERT_NO_FLOOR_ADD_TO_REMOVE"));
                return;
            }
            if (Convert.ToInt32(floorCount) > 1)
            {
                floorCount = string.IsNullOrEmpty(TxtBulkAdd) ? "0" : TxtBulkAdd;
                if (floorCount != "0")
                    TxtBulkAdd = Convert.ToString(Convert.ToInt32(floorCount) - 1);
            }
        }

        private void PlushClick()
        {
            floorCount = string.IsNullOrEmpty(TxtBulkAdd) ? "0" : TxtBulkAdd;
            TxtBulkAdd = Convert.ToString(Convert.ToInt32(floorCount) + 1);
        }
       #endregion
    }
}
