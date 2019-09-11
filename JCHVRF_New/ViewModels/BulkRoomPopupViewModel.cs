using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Constants;


namespace JCHVRF_New.ViewModels
{
    class BulkRoomPopupViewModel: ViewModelBase
    {
        #region Deleget Commands
        public DelegateCommand PlushRoomClickCommand { get; set; }
        public DelegateCommand MinusClickCommand { get; set; }
        public DelegateCommand CancelBulkPopupClickedCommand { get; set; }
        public DelegateCommand AddBulkPopupClickedCommand { get; set; }
        public DelegateCommand<TextCompositionEventArgs> BulkTextChangedCommand { get; set; }
        #endregion
        #region Property Member
        private string roomCount = string.Empty;
        private IEventAggregator _eventAggregator;
        private string txtBulkAdd;
        private IModalWindowService _winService;

        public string TxtBulkAdd
        {
            get { return txtBulkAdd; }
            set
            {
                this.SetValue(ref txtBulkAdd, value);
                RaisePropertyChanged("IsMinusEnabled");
            }
        }

        public bool IsMinusEnabled { get {
                int a = 0;
                return int.TryParse(txtBulkAdd, out a) && a > 0;
            } }
        #endregion

        #region constructor
        public BulkRoomPopupViewModel(IEventAggregator eventAggregator, IModalWindowService winService)
        {
            try
            {
            _eventAggregator = eventAggregator;
            _winService = winService;
            PlushRoomClickCommand = new DelegateCommand(PlushClick);
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
           
            if (!string.IsNullOrWhiteSpace(TxtBulkAdd))
            {
                Int64 introomCount = Int64.Parse(TxtBulkAdd);
                if (introomCount <= 1000)
                {
                    _eventAggregator.GetEvent<PubSubEvent<int>>().Publish(Convert.ToInt32(introomCount));
                    _winService.Close(ViewKeys.BulkRoomPopup);
                }
                else
                {
                    JCHMessageBox.ShowError(Language.Current.GetMessage("ALERT_ENTER_COUNT"));
                }

            }
            else
            {
                //JCHMessageBox.Show("Please Enter RoomNo");               
                JCHMessageBox.Show(Language.Current.GetMessage("ALERT_ENTER_ROOMNO"));
            }

}
        private void OnCancelBulkPopupClickedCommand()
        {
            _eventAggregator.GetEvent<PubSubEvent>().Publish();
            _winService.Close(ViewKeys.BulkRoomPopup);
        }
        private void MinusClick()
        {
            if (roomCount != "")
            {
                if (Convert.ToInt32(roomCount) > 0)
                {
                    roomCount = string.IsNullOrEmpty(TxtBulkAdd) ? "0" : TxtBulkAdd;
                    if (roomCount != "0")
                        TxtBulkAdd = Convert.ToString(Convert.ToInt32(roomCount) - 1);
                }
            }
            //else {

            //}
        }

        private void PlushClick()
        {
            roomCount = string.IsNullOrEmpty(TxtBulkAdd) ? "0" : TxtBulkAdd;
            TxtBulkAdd = Convert.ToString(Convert.ToInt32(roomCount) + 1);
        }
        #endregion
    }
}
