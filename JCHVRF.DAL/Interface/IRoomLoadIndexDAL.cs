using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    public interface IRoomLoadIndexDAL
    {
        string GetDefaultCity();
        bool SetDefaultCity(string cityName, out string errMsg);
        bool IsCityExist(string cityName);
        DataTable GetCityList();
        int AddCity(string cityName, out string errMsg);
        int UpdateCity(string newName, string oldName, out string errMsg);
        bool DeleteCity(string cityName, out string errMsg);

        RoomLoadIndex GetRoomLoadIndexItem(string cityName, string rType);
        RoomLoadIndex GetDefaultRoomLoadIndexItem(string cityName);
        bool SetDefaultRoomLoadIndex(string cityName, string rType, out string errMsg);
        bool IsRoomTypeExist(string cityName, string rType);
        DataTable GetRoomLoadIndexList(string cityName, string utLoadIndex);
        DataTable GetRoomTypeList();
        int AddLoadIndex(string city, string rType, string coolingIndex, string heatingIndex, out string errMsg);
        int AddLoadIndex(RoomLoadIndex newItem, out string errMsg);
        int UpdateLoadIndex(RoomLoadIndex item, out string errMsg);
        bool DeleteLoadIndex(string city, string rType, out string errMsg);

    }
}
