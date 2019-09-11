using JCHVRF_New.Common.Contracts;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JCHVRF.Model.NextGen;
using System.Collections.ObjectModel;

namespace JCHVRF_New.Common.Helpers
{
    public class GlobalProperties: IGlobalProperties
    {
        
        public GlobalProperties(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        private string _projTitle;
        private IEventAggregator _eventAggregator;

        public string ProjectTitle
        {
            get { return _projTitle; }
            set { _projTitle = value;
                RaiseGlobalPropertyChanged();
            }
        }

        //Bug 4253
        private Dictionary<int, HashSet<int>> _defaultAccessoryDictionary;
        public Dictionary<int, HashSet<int>> DefaultAccessoryDictionary
        {
            get {
                if (_defaultAccessoryDictionary == null)
                    _defaultAccessoryDictionary = new Dictionary<int, HashSet<int>>();
                    return _defaultAccessoryDictionary; }
            set
            {
                _defaultAccessoryDictionary = value;
                RaiseGlobalPropertyChanged();
            }
        }
        //Bug 4253

        private ObservableCollection<Notification> _notifications;

        public ObservableCollection<Notification> Notifications
        {
            get {
                if (_notifications == null)
                {
                    _notifications = new ObservableCollection<Notification>();
                    _notifications.CollectionChanged += (o, e) =>
                    {
                        RaiseGlobalPropertyChanged();
                    };
                }
                return _notifications; }
        }


        private void RaiseGlobalPropertyChanged([CallerMemberName] string propertyName =null)
        {
            _eventAggregator.GetEvent<GlobalPropertiesUpdated>().Publish(propertyName);
        }
    }
}
