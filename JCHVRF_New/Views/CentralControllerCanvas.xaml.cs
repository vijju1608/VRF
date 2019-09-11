using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Prism.Events;
using JCHVRF.Model;
using JCHVRF.MyPipingBLL;
using JCBase.Utility;
using JCHVRF.DAL;
using JCHVRF.Const;
using JCHVRF_New.ViewModels;
using JCHVRF.VRFMessage;
using ng = JCHVRF.Model.NextGen;
using NextGenModel = JCHVRF.Model.NextGen;
using ControllerModel = JCHVRF.Model.Controller;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using Point = System.Windows.Point;
using System.Drawing.Imaging;
using drawing = System.Drawing;
using JCHVRF_New.Utility;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection.Emit;
using System.Windows.Documents;
using JCHVRF.BLL;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Common.UI.Extensions;
using Brushes = System.Windows.Media.Brushes;


namespace JCHVRF_New.Views
{

    public class ChildView : UIElement
    {

    }
    /// <summary>
    /// Interaction logic for ControllerCanvas.xaml
    /// </summary>
    public partial class CentralControllerCanvas : UserControl
    {          
        
       
        public CentralControllerCanvas()
        {                                  
            InitializeComponent();
            scrvControllers.ScrollToHorizontalOffset(scrvControllers.MaxWidth);
        }
        
        private void controller_Drop(object sender, DragEventArgs e)
        {
           var viewModel = ((CentralControllerCanvasViewModel) (this.DataContext));
           viewModel.CheckCompatibility(e);
                                                                      
        }

      
    }
}
