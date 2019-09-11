using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.DALFactory.New
{
   public class Connection
    {
        public static string GetConnectionString()
        {
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\\DBVRF.accdb;";

        }
        public static string GetConnection()
        {
            //return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\\VRF.accdb;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
            //return "Provider = Microsoft.Jet.OLEDB.4.0; Data Source =| DataDirectory |\\VRF.dat; Persist Security Info = False; Jet OLEDB:Database Password = VwA15CxRfN2012ThL";
            //return "Provider = Microsoft.ACE.OLEDB.12.0; Data Source =C:\\Users\\ajay_pal\\Documents\\Visual Studio 2015\\Projects\\JCHAPP\\JCHAPP\\bin\\Debug\\VRF.dat;Jet OLEDB:Database Password = VwA15CxRfN2012ThL";
            return "Provider = Microsoft.ACE.OLEDB.12.0; Data Source =C:\\VSTS\\VRFDesktopApplication\\JCHVRF\\JCHVRF_New\\bin\\Debug\\VRF.dat; Jet OLEDB:Database Password = VwA15CxRfN2012ThL";

        }

        public static string GetOldConnection()
        {
            return "Provider = Microsoft.ACE.OLEDB.12.0; Data Source =C:\\VSTS\\VRFDesktopApplication\\JCHVRF\\JCHVRF_New\\bin\\Debug\\Project.mdb; Jet OLEDB:Database Password = YqJz2010Co04Ir15Kf";

        }
    }
}
