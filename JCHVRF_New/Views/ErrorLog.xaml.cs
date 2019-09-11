using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using JCHVRF_New.ViewModels;
using JCHVRF_New.Model;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for ErrorLog.xaml
    /// </summary>
    public partial class ErrorLog : UserControl
    {
    
        public ErrorLog()
        {
            InitializeComponent();
            //ErrorGrid.ItemsSource = _GetPipingError;
            //ErrorGrid.Items.Refresh();
         
        }

        //private void AddMockedData()
        //{
        //    var ctx = this.DataContext as ErrorLogViewModel;

        //    ctx.LogError(ErrorType.Error, ErrorCategory.PipingErrors, new List<string>{
        //        "Sample 1 piping error",
        //    });

        //    ctx.LogError(ErrorType.Message, ErrorCategory.PipingErrors, new List<string>{
        //        "Sample 2 piping message adfasd asd fasd fasdf asdfas",
        //    });

        //    ctx.LogError(ErrorType.Warning, ErrorCategory.PipingErrors, new List<string>{
        //        "Sample 3 piping warning",
        //    });
        //}

        /// <summary>
        /// Logs system errors
        /// </summary>
        /// <param name="Err">Error Type</param>
        /// <param name="ErrCat">Category</param>
        /// <param name="ErrMsg">Message</param>
        public static void LogError(ErrorType Err, ErrorCategory ErrCat, string ErrMsg)
        {
            if (string.IsNullOrEmpty(ErrMsg)) return;

            var evm = new ErrorLogViewModel();
           
            if (!(evm.GetPipingError.Any(x => x.Description == ErrMsg)))
            {
                var imgpath = string.Empty;
                if (ErrorType.Error == Err)
                {
                    imgpath = "..\\..\\Image\\TypeImages\\Error.png";
                }
                else imgpath = "..\\..\\Image\\TypeImages\\Warning.png";

                var err = new SystemError()
                {
                    Type = Err,
                    Category = ErrCat,
                    Description = ErrMsg
                };
                
                evm.GetPipingError.Add(err);
            }
            else return;
        }        

        private void Error_Click(object sender, RoutedEventArgs e)
        {
            var evm = this.DataContext as ErrorLogViewModel;
            evm.ToggleErrors = !evm.ToggleErrors;

            if (evm.ToggleErrors)
            {
                evm.ToggleWarning = false;
                var result = evm.GetPipingError.AsEnumerable().Where(myRow => myRow.Type == ErrorType.Error);
                ErrorGrid.ItemsSource = result;
            }
            else
            {
                var result = evm.GetPipingError;
                ErrorGrid.ItemsSource = result;
            }
        }

        private void Warning_Click(object sender, RoutedEventArgs e)
        {
            var evm = this.DataContext as ErrorLogViewModel;
            evm.ToggleWarning = !evm.ToggleWarning;
            if (evm.ToggleWarning)
            {
                evm.ToggleErrors = false;
                var result = evm.GetPipingError.AsEnumerable().Where(myRow => myRow.Type == ErrorType.Warning);
                ErrorGrid.ItemsSource = result;
            }
            else
            {
                var result = evm.GetPipingError;
                ErrorGrid.ItemsSource = result;
            }
        }

        //private void Add_Click(object sender, RoutedEventArgs e)
        //{
        //    AddMockedData();

        //}
    }
}

