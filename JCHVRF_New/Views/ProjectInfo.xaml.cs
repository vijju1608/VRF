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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for ProjectInfo.xaml
    /// </summary>
    public partial class ProjectInfo : UserControl
    {
        public ProjectInfo()
        {
            InitializeComponent();
        }
        //public void btnNewclient_Click(object sender, RoutedEventArgs e)
        //{
        //    AddNewClient frmNewClient = new AddNewClient();
        //    frmNewClient.ShowDialog();
        //}

        //public void btnCreatorInformation_Click(object sender, RoutedEventArgs e)
        //{
        //    //NewCreatorInformation frmNewCreatorInfo = new NewCreatorInformation();
        //    //frmNewCreatorInfo.ShowDialog();
        //}
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get DatePicker reference.
            var picker = sender as DatePicker;

            // ... Get nullable DateTime from SelectedDate.
            DateTime? date = picker.SelectedDate;
            if (date == null)
            {
                // ... A null object.
                //this = "No date";
            }
            //else if (date != null)
            //{
            //    if (picker.SelectedDate < DateTime.Now.Date)
            //    {
            //        JCHMessageBox.Show("Delivery Date cannot be before than Current date");
            //        DeliveryDate.SelectedDate = DateTime.Now;
            //        // DeliveryDate.SelectedDate = Convert.ToDateTime(date.Value.ToString("dd-MM-yyyy"));
            //    }

            //}
            else
            {
                // ... No need to display the time.
                //this.Title = date.Value.ToShortDateString();
            }
        }

       
    }
}
