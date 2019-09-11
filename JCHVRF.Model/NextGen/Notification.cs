using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.Model.NextGen
{
    public enum NotificationType
    {
        POLICY,
        RECOMMENDATION,
        APPLICATION
    }

    public class Notification : ModelBase
    {
        public Notification(NotificationType type, string message)
        {
            _type = type;
            _message = message;
            _occurence = DateTime.Now;
        }

        private string _message;

        public string Message
        {
            get { return this._message; }
            set { this.SetValue(ref _message, value); }
        }

        private NotificationType _type;

        public NotificationType Type
        {
            get { return this._type; }
            set { this.SetValue(ref _type, value);
                RaisePropertyChanged("Icon");
            }
        }

        private DateTime _occurence;

        public DateTime Occurence
        {
            get { return this._occurence; }
            set { this.SetValue(ref _occurence, value);
                RaisePropertyChanged("OccurenceDiffText");
            }
        }

        public string Icon {
            get {
                switch (Type)
                {
                    case NotificationType.POLICY:
                        return "PoliciesIcon";
                    case NotificationType.RECOMMENDATION:
                        return "PoliciesIcon";
                    case NotificationType.APPLICATION:
                        return "PoliciesIcon";
                    default:
                        return "PoliciesIcon";
                }
            }
        }

        public string OccurenceDiffText
        {
            get {
                string text = "";
                if ((DateTime.Now - Occurence).Hours > 0)
                {
                    text = (DateTime.Now - Occurence).Hours.ToString("00")+ " Hours Ago";
                }
                else if ((DateTime.Now - Occurence).Minutes > 0)
                {
                    text = (DateTime.Now - Occurence).Minutes.ToString("00") + " Minutes Ago";
                }
                else if ((DateTime.Now - Occurence).Seconds > 0)
                {
                    text = (DateTime.Now - Occurence).Seconds.ToString("00") + " Seconds Ago";
                }
                return text;
            }
        }
    }
}
