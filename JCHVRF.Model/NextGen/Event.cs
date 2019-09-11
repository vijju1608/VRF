using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace JCHVRF_New.Model
{
   public class Event : ModelBase
    {
        private int? _EventId;       
        public int? EventId
        {
            get { return _EventId; }
            set { this.SetValue(ref _EventId, value); }
        }

        private string _EventTitle;
        [Required(ErrorMessage = "Please enter Event Title")]
        public string EventTitle
        {
            get { return _EventTitle; }
            set { this.SetValue(ref _EventTitle, value); }
        }

        private string _EventLocation;
        [Required(ErrorMessage = "Please enter Event Location")]
        public string EventLocation
        {
            get { return _EventLocation; }
            set { this.SetValue(ref _EventLocation, value); }
        }

        private DateTime _StartDate = DateTime.Now;
        public DateTime StartDate
        {
            get { return _StartDate; }
            set { this.SetValue(ref _StartDate, value); }
        }

        private DateTime _EndDate = DateTime.Now;
        public DateTime EndDate
        {
            get { return _EndDate; }
            set { this.SetValue(ref _EndDate, value); }
        }
        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { this.SetValue(ref _notes, value); }
        }
        public DateTime EVENT_STARTDATE { get; set; }
        public DateTime EVENT_ENDDATE { get; set; }

    }
}
