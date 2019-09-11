using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IRegionDAL
    {
        MyRegion GetItem(string code);
        DataTable GetParentRegionTable();
        List<MyRegion> GetParentRegionList();
        DataTable GetSubRegionTable(string pCode);

    }
}
