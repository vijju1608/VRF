using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.ViewModels;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unity;
using JCHVRF.Entity;
using JCHVRF.Model;
using JCHVRF_New.Model;
using JCHVRF.DAL.New;
using JCHVRF.DALFactory;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl, IClosable
    {
        
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        private IUnityContainer _container;
        
        public Settings(IRegionManager regionManager, IUnityContainer container, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _container = container;
            InitializeComponent();
        
        }

        public void RequestClose()
        {
            //this
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, "Home");
        }

       
    }
}
