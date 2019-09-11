using JCHVRF_New.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
    public class SystemTypeItem : ModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }
        private string _path;

        public string Path
        {
            get { return _path; }
            set { this.SetValue(ref _path, value); }

        }
        private string _systemID;

        public string SystemID
        {
            get { return _systemID; }
            set { this.SetValue(ref _systemID, value); }

        }
    }
}
