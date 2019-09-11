using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IIndoorDAL
    {
        //Indoor GetItem(string model, string unitType);
        //DataTable GetListStd(string type);
        //DataTable GetTypeList(out string colName);
        //增加多ProductType功能 20160821 by Yunxiao Lin
        Indoor GetItem(string model, string unitType, string productType, string Series);
        //DataTable GetListStd(string type, string productType);
        //增加厂名参数 20161118 by Yunxiao Lin
        DataTable GetListStd(string type, string productType, string factoryName);
        DataTable GetTypeList(out string colName, string productType);

        //double CalEstCapacity(Indoor inItem, double OutTemperature, double InTemperature, out double estSH, bool isHeating);
        //估算室内机容量改为不用返回显热值 20161111 by Yunxiao Lin
        double CalEstCapacity(Indoor inItem, double OutTemperature, double InTemperature, bool isHeating);
    }
}
