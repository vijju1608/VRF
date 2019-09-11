using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IAccessoryDAL
    {
        List<Accessory> GetDefault(Indoor inItem, string RegionCode,string SubRegionCode, string Series);

        Accessory GetItems(string type, string model, Indoor inItem, string RegionCode, string SubRegionCode);

        Accessory GetItem(string type, Indoor inItem, string RegionCode, string SubRegionCode);

        DataTable GetAllAvailable(Indoor inItem, string RegionCode, string SubRegionCode);

       
            
    }
}
