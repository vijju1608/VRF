using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Controls;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace JCHVRF_New.Common.Helpers
{
    /// <summary>
    /// Class to Open View In A Window.
    /// Creating an Instance Of Window and referencing Actual View to set its content was done earlier.
    /// This server MVVM approach allows loosecoupling and no references to actual View or Window class needs to be given in ViewModel.
    /// </summary>
    public class ModalWindowService : IModalWindowService
    {
        IUnityContainer _container;
        Dictionary<string, Tuple<JCHModalWindow, NavigationParameters>> _allOpenedWindows = new Dictionary<string, Tuple<JCHModalWindow, NavigationParameters>>();
        private IEventAggregator _eventAggregator;

        public ModalWindowService(IUnityContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;
        }

        public void ShowView(string viewKey, string title="" , NavigationParameters parameters = null, bool showAsDialog = true, double width= 0, double height = 0)
        {
            if (_allOpenedWindows.ContainsKey(viewKey))
            {
                if (showAsDialog)
                {
                    _allOpenedWindows[viewKey].Item1.ShowDialog();
                }
                else
                {
                    _allOpenedWindows[viewKey].Item1.Show();
                }
                return;
            }

            JCHModalWindow window = new JCHModalWindow();
            window.Title = title;
            if (width != 0 || height != 0)
            {
                window.SizeToContent = SizeToContent.Manual;
                window.Height = height;
                window.Width = width;                
            }
            else
            {
                window.SizeToContent = SizeToContent.WidthAndHeight;
            }

            window.Closed += Window_Closed; ;

            _allOpenedWindows.Add(viewKey, Tuple.Create(window, parameters));
            window.Content = _container.Resolve<object>(viewKey);
            window.Tag = viewKey;

            if (showAsDialog)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (sender is JCHModalWindow)
            { 
            string viewKey = (sender as JCHModalWindow).Tag?.ToString();
            _allOpenedWindows.Remove(viewKey);
            _eventAggregator.GetEvent<ModalWindowClosed>().Publish(viewKey);
            }
        }


        public void Close(string viewKey) {
            if (_allOpenedWindows.ContainsKey(viewKey))
            {
                _allOpenedWindows[viewKey].Item1.Close();
            }
        }

        public NavigationParameters GetParameters(string viewKey)
        {
            if (_allOpenedWindows.ContainsKey(viewKey))
            {
              return _allOpenedWindows[viewKey].Item2;
            }
            return null;
        }
    }
}