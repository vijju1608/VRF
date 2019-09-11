using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace JCHVRF.DAL
{
    public interface IProjectDAL
    {
        DataTable GetProductTypeList(string regionVRF);
    }
}
