using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF.Model;

namespace JCHVRF_New.ViewModels
{
    public class WorkFlowContext
    {
        public static List<string> FloorNames;                       //sharad-add floor on TotalHeatEx -03/07/19

        //internal static bool _flgHideCentralC=true;

        public static string Systemid
        {
            get; set;
        }

        public static string systemName
        {
            get; set;
        }

        public static SystemBase NewSystem { get; set; }

        public static SystemBase CurrentSystem { get; set; }

        public static void Clear()
        {
            if (FloorNames != null)
            {
                FloorNames.Clear();
            }
            Systemid = "1";
            NewSystem = null;
        }
    }
}
