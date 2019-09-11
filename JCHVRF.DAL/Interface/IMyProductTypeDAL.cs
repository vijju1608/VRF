using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IMyProductTypeDAL
    {
        MyProductType GetItem(string brandCode, string regionCode, string productType);
        DataTable GetProductTypeData(string brandCode, string factoryCode, string regionCode);
        DataTable GetProductTypeData(string brandCode, string regionCode);
        List<string> GetBrandCodeList(string regionCode);
        List<string> GetFactoryCodeList(string brandCode, string regionCode);
    }
}
