using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JCHVRF.DALFactory;

namespace JCHVRF.DAL.NextGen
{
    public class NamePrefixDAL
    {
        DAOAccess _dao = new DAOAccess();
        public bool updateNamePrefixData(string uname, string subregion, List<string> namePrefixData)
        {
            string query = "update NamePrefix set buildingName=" + namePrefixData[0] +"','"+ "floorname = " + namePrefixData[1] +
                            "','" + "roomname = " + namePrefixData[2]+
                            "','" + "Indoorunitname = " + namePrefixData[3]+
                            "','" + "OutdoorUnitname = " + namePrefixData[4]+
                            "','" + "System = " + namePrefixData[5]+
                            "','" + "Controller = " + namePrefixData[6]+
                            "','" + "TotalHeatExchangers = " + namePrefixData[7];
            string con = _dao.GetConnString();
            DataTable insertNameprefixRecord = new DataTable();
            int result = _dao.ExecuteSql(query);
            if (result > 0)
                return true;
            else
                return false;
        }

        public bool InsertNamePrefixData(string buildingName, string floorname,string roomName, string indoorUnitName, 
                                         string outdoorUnitName, string systemName, string Controllers, string totalHeatExchangers)
        {
            string query = "Insert into Nameprefix (Buildingname, floorname, roomname, indoorUnitName, OutdoorUnitName, systemName, Controllers,totalHeatExchangers) values" +
                            "'('"+buildingName +"', '"+ floorname + "','"+roomName+"','"+indoorUnitName+"','"+outdoorUnitName+"','"+
                            "','"+ systemName+"','"+ Controllers+"','"+ totalHeatExchangers+"')'";
            string con = _dao.GetConnString();
            //DataTable insertNameprefixRecord = new DataTable();
            int result = _dao.ExecuteSql(query,con);
            if (result > 0)
                return true;
            else
                return false;
        }

        public DataTable getNamePrefixData(string username)
        {
            string uname = username;
            string query = "Select * from NamePrefix NP inner join User U on NP.userId=U.Userid" +
                            "inner join UserToRegionMapping UR on UR.userID=U.userID where username = " + uname; //to be written
            DataTable defaultNameprefixList = new DataTable();
            defaultNameprefixList = _dao.GetDataTable(query);
            if (defaultNameprefixList.Rows.Count > 0 && defaultNameprefixList!=null)
                return defaultNameprefixList;
            else
                return null;
        }
    }
}
