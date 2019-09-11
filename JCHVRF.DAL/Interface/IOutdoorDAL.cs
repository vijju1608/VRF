using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IOutdoorDAL
    {
        //Outdoor GetItem(string model);
        // 多ProductType功能，增加productType参数 20160823 by Yunxiao Lin
        Outdoor GetItem(string model, string productType);
        DataTable GetOutdoorListStd();
        //DataTable GetOutdoorTypeList(out string colName);
        DataTable GetOutdoorTypeList(out string colName, string productType);

        //double CalOutdoorEstCapacity(string type, string shortModel, double maxRatio, double OutTemperature, double InTemperature, bool isHeating);
        double CalOutdoorEstCapacity(Outdoor outItem, double maxRatio, double OutTemperature, double InTemperature, bool isHeating, SystemVRF sysItem);
    }
}
