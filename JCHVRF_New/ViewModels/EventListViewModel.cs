using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Model;
using System.Collections.ObjectModel;
using JCHVRF.DAL.NextGen;
using Prism.Events;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using JCHVRF_New.Common.Contracts;
using Prism.Regions;
using JCHVRF_New.Common.Constants;

namespace JCHVRF_New.ViewModels
{
    public class EventListViewModel : ViewModelBase
    {
        private ObservableCollection<Event> _events;
        private IModalWindowService _winService;
        public DelegateCommand<int?> EditEventClickedCommand { get; set; }

        public DelegateCommand<int?> DeleteEventClickedCommand { get; set; }

        public ObservableCollection<Event> Events
        {
            get { return this._events; }
            set { this.SetValue(ref _events, value); }
        }
        private DateTime _currentdate = DateTime.Now;
        public DateTime Currentdate
        {
            get { return _currentdate; }
            set { this.SetValue(ref _currentdate, value); }
        }
        private IEventAggregator _eventAggregator;
        private IEventDAL _eventDAL;

        public EventListViewModel(IEventAggregator EventAggregator, IModalWindowService winService, IEventDAL eventDAL)
        {
            _eventAggregator = EventAggregator;
            _winService = winService;
            _eventDAL = eventDAL;
            _eventAggregator.GetEvent<AddEventClickedDate>().Subscribe(LoadEvents);
            EditEventClickedCommand = new DelegateCommand<int?>(EditEventClick);
            DeleteEventClickedCommand = new DelegateCommand<int?>(DeleteEventClick);
        }

        public void LoadEvents(string clickeddate)
        {
            Events = new ObservableCollection<Event>(_eventDAL.GetEventList(Convert.ToDateTime(clickeddate)));
            Currentdate = Convert.ToDateTime(clickeddate);

            //EditEventClick();
        }

        public void EditEventClick(int? eventId)
        {
            NavigationParameters param = new NavigationParameters();
            param.Add("EventId", eventId);
            _winService.ShowView(ViewKeys.AddEvent, "Edit Event",param);
            _eventAggregator.GetEvent<EditEventEventId>().Publish(eventId);

        }

        public void DeleteEventClick(int? eventId)
        {

            Boolean result = false;
            result = _eventDAL.DeleteEventClick(eventId);
            if(result)
            {
                JCHMessageBox.Show("Event Successfully Deleted");
                if (Events.Count > 0)
                {
                    LoadEvents(Currentdate.ToString());
                }
                if (Events.Count== 0)
                {
                    _winService.Close(ViewKeys.EventList);
                }
            }
        }
    }

}
