using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
   public class AutoSelectODUModel
    {
        public List<string> MSGList = new List<string>();
        public List<string> ERRList = new List<string>();
        public SelectOutdoorResult SelectionResult { get; set; }
    }
}
