using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JCHVRF_New.ViewModels
{
    public class NavigatorViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;

        public DelegateCommand<FrameworkElement> LoadedCommand { get; set; }

        private double _sliderValue;

        public double SliderValue
        {
            get { return this._sliderValue; }
            set { this.SetValue(ref _sliderValue, value);
                _eventAggregator.GetEvent<NavigatorZoomValueChangeSubscriber>().Publish(_sliderValue);
            }
        }


        public NavigatorViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            LoadedCommand = new DelegateCommand<FrameworkElement>(OnLoaded);
        }

        private void OnLoaded(FrameworkElement viewFinder)
        {
            _eventAggregator.GetEvent<NavigatorZoomBoxSubscriber>().Publish(viewFinder);
        }
    }
}
