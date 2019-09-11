using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontAwesome.WPF;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Model
{
    public class PipingInfoModel : ModelBase
    {
        #region Fields
        private string _description;

        private string _value;

        private string _min;

        private string _max;

        private string _longDescription;

        private string _maxDescription;

        private string _valueDescription;
        private bool _IsValid;

        #endregion
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                this.SetValue(ref _description, value);
            }
        }
        public bool IsValid
        {
            get
            {
                return _IsValid;
            }
            set
            {
                this.SetValue(ref _IsValid, value);
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                this.SetValue(ref _value, value);
                RaisePropertyChanged("IsValid");
            }
        }


        public string Min
        {
            get
            {
                return _min;
            }
            set
            {
                this.SetValue(ref _min, value);
                RaisePropertyChanged("IsValid");
            }
        }

        public string Max
        {
            get
            {
                return _max;
            }
            set
            {
                this.SetValue(ref _max, value);
                RaisePropertyChanged("IsValid");
            }
        }
        public string LongDescription
        {
            get
            {
                return _longDescription;
            }
            set
            {
                this.SetValue(ref _longDescription, value);
            }
        }

        public string MaxDescription
        {
            get
            {
                return _maxDescription;
            }
            set
            {
                this.SetValue(ref _maxDescription, value);
            }
        }

        public string ValueDescription
        {
            get
            {
                return _valueDescription;
            }
            set
            {
                this.SetValue(ref _valueDescription, value);
            }
        }

    }
}
