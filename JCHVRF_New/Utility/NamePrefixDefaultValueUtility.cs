using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Model;

namespace JCHVRF_New.Utility
{
    public static class NamePrefixDefaultValueUtility
    {
        public static NamePrefixModel GetAllDefaultNamePrefixValues()
        {
            NamePrefixModel NamePrefixDefaultValues = new NamePrefixModel();
            NamePrefixDefaultValues.BuildingName = "Build";
            NamePrefixDefaultValues.FloorName = "Floor";
            NamePrefixDefaultValues.RoomName = "Room";
            NamePrefixDefaultValues.IndoorUnitsName = "Ind";
            NamePrefixDefaultValues.OutdoorUnitName = "SYS";
            NamePrefixDefaultValues.SystemName = "";
            NamePrefixDefaultValues.Controllers = "Control group";
            NamePrefixDefaultValues.TotalHeatExchangers = "Exc";
            return NamePrefixDefaultValues;
        }
    }
}