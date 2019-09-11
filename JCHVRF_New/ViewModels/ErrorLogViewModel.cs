using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JCHVRF_New.ViewModels
{
    public class ErrorLogViewModel : ViewModelBase
    {
       private static bool flgSubscribed;
        // ACC - SKM - 1211  START
        public ErrorLogViewModel()
        {
            OnErrorClearAll();
            _GetPipingError.Clear();
        }

        public ErrorLogViewModel(IEventAggregator EventAggregator)
        {
            _eventAggregator = EventAggregator;
            if (!flgSubscribed)
            {
                _eventAggregator.GetEvent<ErrorLogVMClear>().Subscribe(OnErrorClear);
                _eventAggregator.GetEvent<ErrorLogVM>().Subscribe(OnError);
                _eventAggregator.GetEvent<ErrorLogVMClearAll>().Subscribe(OnErrorClearAll);
               // _eventAggregator.GetEvent<CleanupOnClose>().Subscribe(OnCloseCleanup);
                flgSubscribed = true;
            }
        
           
        }
        private void OnCloseCleanup()
        {
            _eventAggregator.GetEvent<ErrorLogVM>().Unsubscribe(OnError);
            _eventAggregator.GetEvent<ErrorLogVMClear>().Unsubscribe(OnErrorClear);
            _eventAggregator.GetEvent<CleanupOnClose>().Unsubscribe(OnCloseCleanup);
        }
        public ObservableCollection<SystemError> GetPipingError
        {
            get {
                return _GetPipingError;
            }
            set
            {
                SetValue(ref _GetPipingError, value);
            }
        }
       
        internal void OnError(List<string> strErr)
        {
            LogError(Model.ErrorType.Error, Model.ErrorCategory.WiringError, strErr);
            //_eventAggregator.GetEvent<ErrorLogVM>().Unsubscribe(OnError);
            //_eventAggregator.GetEvent<ErrorLogVM>().Subscribe(OnError);
        }

        void OnErrorClear()
        {
            RemoveErrorFromList(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors);
            RemoveErrorFromList(Model.ErrorType.Error, Model.ErrorCategory.WiringError);
            RemoveErrorFromList(Model.ErrorType.Warning, Model.ErrorCategory.PipingErrors);
            RemoveErrorFromList(Model.ErrorType.Warning, Model.ErrorCategory.WiringError);
            RemoveErrorFromList(Model.ErrorType.Message, Model.ErrorCategory.PipingErrors);
            RemoveErrorFromList(Model.ErrorType.Message, Model.ErrorCategory.WiringError);

            //_eventAggregator.GetEvent<ErrorLogVMClear>().Unsubscribe(OnErrorClear);
            //_eventAggregator.GetEvent<ErrorLogVMClear>().Subscribe(OnErrorClear);

        }

        public void LogError(JCHVRF_New.Model.ErrorType Err, JCHVRF_New.Model.ErrorCategory ErrCat, List<string> ErrMsg)
        {
            try
            {
                var imgpath = string.Empty;
                if (JCHVRF_New.Model.ErrorType.Error == Err)
                {
                    imgpath = "..\\..\\Image\\TypeImages\\Error.png";
                }
                else imgpath = "..\\..\\Image\\TypeImages\\Warning.png";

                if (ErrMsg != null && ErrMsg.Count > 0)
                    foreach (var Msg in ErrMsg)
                    {
                        var evm = new SystemError
                        {
                            Type = Err,
                            Category = ErrCat,
                            Description = Msg
                        };

                        GetPipingError.Add(evm);
                    }
            }

            catch { }

        }
        public void RemoveErrorFromList(JCHVRF_New.Model.ErrorType Err, JCHVRF_New.Model.ErrorCategory ErrCat)
        {
            try
            {
                foreach (var Item in GetPipingError)
                {
                    if (Item.ErrorType == Err.ToString() && Item.Category == ErrCat)
                    {
                        GetPipingError.RemoveAt(GetPipingError.Count - 1);
                    }
                }
            }
            catch { }
        }

        internal void OnErrorClearAll()
        {
            if(GetPipingError!=null)

           GetPipingError.Clear();
        }

        #region Fields
        public string ErrorType
        {
            get { return _errorType; }
            set
            {
                SetValue(ref _errorType, value);
                
            }
        }
        public string ErrorTypeImg { get; set; }

        private string _category;
        public string Category
        {
            get { return _category; }
            set
            {
                SetValue(ref _category, value);
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                SetValue(ref _description, value);
            }
        }

        private bool _toggleErrors;

        public bool ToggleErrors
        {
            get { return _toggleErrors; }
            set {
                SetValue(ref _toggleErrors, value);
            }
        }

        private bool _toggleWarning;

        public bool ToggleWarning
        {
            get { return _toggleWarning; }
            set {
                SetValue(ref _toggleWarning, value);
            }
        }
        
        private static ObservableCollection<SystemError> _GetPipingError =
            new ObservableCollection<SystemError>();

        IEventAggregator _eventAggregator;

        private string _errorType;
        ~ErrorLogViewModel()
        {
            if (_eventAggregator != null)
            {
                _eventAggregator.GetEvent<ErrorLogVMClear>().Unsubscribe(OnErrorClear);
                _eventAggregator.GetEvent<ErrorLogVM>().Unsubscribe(OnError);
                _eventAggregator.GetEvent<ErrorLogVMClearAll>().Unsubscribe(OnErrorClearAll);
                //  _eventAggregator.GetEvent<CleanupOnClose>().Publish();
            }
        }
        #endregion


    }
}
