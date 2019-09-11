using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using System.Collections.ObjectModel;
using JCHVRF.DAL.NextGen;
using JCHVRF_New.Common.Contracts;
using language=JCHVRF_New.LanguageData.LanguageViewModel;
using Prism.Regions;
using JCHVRF.Model.NextGen;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{
    public class AddEventViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;
        private IModalWindowService _winService;       

        public DelegateCommand AddEventCommand { get; set; }
        public DelegateCommand UpdateEventCommand { get; set; }
        public DelegateCommand AddEventCancelCommand { get; set; }

        private ObservableCollection<Event> _objEventList;
        public ObservableCollection<Event> objEventList
        {
            get
            {
                return _objEventList;
            }
            set
            {
                this.SetValue(ref _objEventList, value);
            }
        }

        private Event selectedEvent;

        public Event SelectedEvent
        {
            get
            {
                if (selectedEvent == null)
                {
                    selectedEvent = new Event();
                }
                return selectedEvent;
            }
            set { selectedEvent = value; }
        }
        private bool _isUpdateEnable;

        public bool IsUpdateEnable
        {
            get { return _isUpdateEnable; }
            set { this.SetValue(ref _isUpdateEnable, value); }
        }
        private string _isSaveEnable;
        private IGlobalProperties _globalProperties;
        private IEventDAL _eventDAL;

        public string IsSaveEnable
        {
            get { return _isSaveEnable; }
            set { this.SetValue(ref _isSaveEnable, value); }
        }

        public AddEventViewModel(IEventAggregator eventAggregator, IModalWindowService winService, IGlobalProperties globalProperties, IEventDAL eventDAL)
        {
            IsSaveEnable = "Visible";
            IsUpdateEnable = false;
            _eventAggregator = eventAggregator;
            _winService = winService;
            _globalProperties = globalProperties;
            _eventDAL = eventDAL;
            AddEventCommand = new DelegateCommand(AddEventCommandClick);
            UpdateEventCommand = new DelegateCommand(UpdateEventCommandClick);
            AddEventCancelCommand = new DelegateCommand(AddEventCancelCommandClick);
            //_eventAggregator.GetEvent<EditEventEventId>().Subscribe(EditLoadEvents);
            if (_winService.GetParameters(ViewKeys.AddEvent)["EventId"] != null)
            {
                int EventID = Convert.ToInt16(_winService.GetParameters(ViewKeys.AddEvent)["EventId"]);
                EditLoadEvents(EventID);
            }
            if (_winService.GetParameters(ViewKeys.AddEvent)["clickedDate"] != null)
            {
                SelectedEvent.StartDate = Convert.ToDateTime(_winService.GetParameters(ViewKeys.AddEvent)["clickedDate"]);
                SelectedEvent.EndDate = Convert.ToDateTime(_winService.GetParameters(ViewKeys.AddEvent)["clickedDate"]);
            }

        }
        public void AddEventCommandClick()
        {
            
            objEventList = new ObservableCollection<Event>();
            objEventList.Add(SelectedEvent);

            Boolean result = false;
            result = _eventDAL.InsertEventData(selectedEvent.EventTitle,selectedEvent.EventLocation,selectedEvent.StartDate,selectedEvent.EndDate,selectedEvent.Notes);
            if (result) { JCHMessageBox.Show(language.Current.GetMessage("ALERT_EVENT_SUCCESS"));
               _winService.Close(ViewKeys.AddEvent);
                _globalProperties.Notifications.Insert(0, new Notification(NotificationType.APPLICATION, string.Format("Event {0} Created", selectedEvent.EventTitle)));
            }
        }
        public void UpdateEventCommandClick()
        {

            //objEventList = new ObservableCollection<Event>();
            //objEventList.Add(SelectedEvent);

            Boolean result = false;
            result = _eventDAL.UpdateEventData(selectedEvent.EventId, selectedEvent.EventTitle, selectedEvent.EventLocation, selectedEvent.StartDate, selectedEvent.EndDate, selectedEvent.Notes);

            if (result)
            {
                JCHMessageBox.Show("Event Successfully Updated");
                // JCHMessageBox.Show(language.Current.GetMessage("ALERT_EVENT_UPDATE"));
                _winService.Close(ViewKeys.AddEvent);
            }
        }

        public void EditLoadEvents(int? EventId)
        {
            //objEventList = new ObservableCollection<Event>();
            
            IsSaveEnable = "Hidden";
            IsUpdateEnable = true;
            SelectedEvent = _eventDAL.EditEvent(EventId);
        }
        public void AddEventCancelCommandClick()
        {
            _winService.Close(ViewKeys.AddEvent);
        }

        protected override void RaiseIsActiveChanged()
        {
            base.RaiseIsActiveChanged();
        }
    }
}
