using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace JCHVRF.Model.New
{
    [Serializable]
    public class Floor: ModelBase
    {
        public string Cooling { get; set; }
        public string CollingAndHeating { get; set; }

        public string SingleFloor { get; set; }

        public string MultipleFloor { get; set; }
        public int Id { get; set; }
        public string floorName { get; set; }
        public int MultiFloorCount { get; set; } = 0;

        //Shweta: added variable and updated the property
        private double _elevationFromGround;
        
        public double elevationFromGround
        {
            get { return _elevationFromGround; }
            set{this.SetValue(ref _elevationFromGround, value); }
        }
        //Shweta: added variable and updated the property
        private bool _IsFloorChecked = false;
        public bool IsFloorChecked { get
            { return _IsFloorChecked; } set
            {
                this.SetValue(ref _IsFloorChecked, value);
            }
        }
        
        public List<Floor> FloorDetails { get; set; }

    }
}
