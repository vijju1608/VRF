using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
    public class LightProject : INotifyPropertyChanged
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public DateTime CreatedDate { get; set; }
        public String ModifiedDate { get; set; } //Modified to display only Date 
        public DateTime DeliveryDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string RemainingDays { get; set; }
		// Added on Date 30-01-2018 for Displaying split no of days
        public string RemainingDaysInNos { get; set; }
        public int ProjectStatusPer { get; set; }
        // End Added on Date 30-01-2018 for Displaying split no of days

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
