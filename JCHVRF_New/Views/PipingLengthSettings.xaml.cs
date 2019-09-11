using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using JCHVRF_New.Common.Helpers;
using Prism.Events;
using System.ComponentModel;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for PipingLengthSettings.xaml
    /// </summary>
    public partial class PipingLengthSettings : Window
    {             
        private IEventAggregator _eventAggregator;
        public PipingLengthSettings()
        {
            
           InitializeComponent();
        }
        private JCHVRF.Model.NextGen.SystemVRF CurrentSystem;
        public PipingLengthSettings(IEventAggregator eventAggregator, JCHVRF.Model.NextGen.SystemVRF obj)
        {
            InitializeComponent();
            this._eventAggregator = eventAggregator;
            _eventAggregator = eventAggregator;
            CurrentSystem = obj;    
           
        }               
      
            
        private void txtBindPropertyChange_KeyDown(object sender, KeyEventArgs e)
        {          
        }        
        private void myTestBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.IsReadOnly == false)
            {
                if (e.Key > Key.D9 || e.Key == Key.Space || e.Key == Key.Delete)
                {
                    if (e.Key == Key.OemPeriod || e.Key == Key.Delete)
                    {
                       if (tb.Text == "0")
                        {
                            tb.Text = "";
                        }
                        e.Handled = false;
                    }
                    else
                    {
                        if (tb.Text == "")
                        {
                            tb.Text = "0";
                            e.Handled = true;
                        }
                        else
                            tb.Text = "0";
                    }
                }
            }
        }       
        private void txtBindPropertyChange_TextChanged(object sender, TextChangedEventArgs e)
        {           
        }       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var system = JCHVRF.Model.Project.CurrentProject.SystemListNextGen;                
                var Lists = system.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();
                if (Lists.HvacSystemType.Equals("1"))
                {
                    var sys = (JCHVRF.Model.NextGen.SystemVRF)Lists;
                    sys.PipeEquivalentLength = Convert.ToDouble(EqPipeLenght.Text);
                    sys.FirstPipeLength = Convert.ToDouble(FirstPipeLength.Text);
                    // underline code comment fix bug 2846// 
                    //JCHVRF.MyPipingBLL.NextGen.PipingBLL pipBll = new JCHVRF.MyPipingBLL.NextGen.PipingBLL(JCHVRF.Model.Project.GetProjectInstance);
                    //NextGenBLL.PipingErrors Message = NextGenBLL.PipingErrors.OK;
                    //pipBll.DoPipingInfo(pipBll, sys.MyPipingNodeOut, sys, out Message);
                    
                    JCHVRF_New.ViewModels.PipingInfoPropertiesViewModel PipingInfoProperties;
                    PipingInfoProperties = new JCHVRF_New.ViewModels.PipingInfoPropertiesViewModel(_eventAggregator);
                    PipingInfoProperties.BindAdditionalPipingInfo(sys);
                    PipingInfoProperties.BindPipingInfoLength(sys);
                    PipingInfoProperties.BindPipingInfoHeight(sys);
                    //_eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Publish(sys);
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(sys);
                    _eventAggregator.GetEvent<SystemExportSubscriber>().Publish(sys);
                   // _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(sys);
                }
                e.Cancel = false;
            }
            catch(Exception ex)
            {
            }
        }
    }
}

