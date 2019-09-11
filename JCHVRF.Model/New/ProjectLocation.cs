using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.Model.New
{
    public class ProjectLocation : ModelBase
    {
        public string Region { get; set; }
        public string SubRegion { get; set; }

        public string GpsCoordinate { get; set; }
    }
}
