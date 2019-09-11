using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
    public class IDU : ModelBase
    {
        public bool _isChecked;
        
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                this.SetValue(ref _isChecked, value);
            }
        }

        private string _displayModelName;

        public string DisplayModelName
        {
            get { return _displayModelName; }
            set
            {
                this.SetValue(ref _displayModelName, value);
            }
        }

        private RoomIndoor _roomIndoor;

        public RoomIndoor RoomIndoor
        {
            get { return _roomIndoor; }
            set
            {
                this.SetValue(ref _roomIndoor, value);
            }
        }
        public IDU()
        {
            _roomIndoor = new RoomIndoor();
        }
        public IDU(RoomIndoor obj)
        {
            this._roomIndoor = obj;
            this._isChecked = _roomIndoor.IsDelete;
        } 
    }
}
