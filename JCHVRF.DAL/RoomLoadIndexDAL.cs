using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

using JCHVRF.Model;
using JCHVRF.DALFactory;
using JCBase.UI;
using JCBase.Utility;

namespace JCHVRF.DAL
{
    public class RoomLoadIndexDAL : IRoomLoadIndexDAL
    {
        IDataAccessObject _dao;
        string _lang;

        public RoomLoadIndexDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject(); 
            _lang = JCBase.UI.LangType.CurrentLanguage;
            if (_lang == "pt-BR")  //因为葡萄牙语 还没有增加对应LoadCity 及 LoadIndex 所以直接使用默认英语的值 on 20180418 by xyj
                _lang = "en-US";
        }
        
        #region 城市数据管理
        // 获取系统中设置的默认城市
        /// <summary>
        /// 获取系统中设置的默认城市
        /// </summary>
        /// <param name="str"></param>
        public string GetDefaultCity()
        {
            string query = "SELECT DISTINCT LoadCity FROM dbo_LoadCity Where LangType='" + _lang + "' and IsDefault=1";
            DataTable dt = _dao.GetLoadIndex(query);
            if (dt.Rows.Count > 0)
                return dt.Rows[0].ItemArray[0].ToString();
            return "";
        }

        // 将指定城市设为默认城市
        /// <summary>
        /// 将指定城市设为默认城市
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public bool SetDefaultCity(string cityName, out string errMsg)
        {
            errMsg = "";
            try
            {
                ArrayList mySql = new ArrayList();
                mySql.Add("UPDATE dbo_LoadCity SET dbo_LoadCity.IsDefault = 0 Where LangType='" + _lang + "'");
                mySql.Add("UPDATE dbo_LoadCity SET dbo_LoadCity.IsDefault = 1 Where LangType='" + _lang + "' and LoadCity= '" + cityName + "'");
                _dao.ExecuteSqlTran(mySql, _dao.GetConnLoadIndex());
                return true;
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return false;
            }
        }

        // 检验城市名称是否已经存在
        /// <summary>
        /// 检验城市名称是否已经存在
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public bool IsCityExist(string cityName)
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM dbo_LoadCity Where LangType='" + _lang + "' and LoadCity= '" + cityName + "'";
            dt = _dao.GetLoadIndex(query);
            return dt.Rows.Count > 0;
        }

        // 获取所有城市的名称列表
        /// <summary>
        /// 获取所有城市的名称列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetCityList()
        {
            DataTable dt = new DataTable(); 
            string query = "SELECT LoadCity FROM dbo_LoadCity Where LangType='" + _lang + "' order by IsDefault desc";  // 始终将默认城市排在第一位
            dt = _dao.GetLoadIndex(query);
            return dt;
        }

        // 添加一个城市记录
        /// <summary>
        /// 添加一个城市记录
        /// </summary>
        /// <param name="cityName">新增的城市名称</param>
        /// <returns>返回受影响的记录数;1:成功添加；0：已存在；-1：Error</returns>
        public int AddCity(string cityName, out string errMsg)
        {
            errMsg = "";
            try
            {
                //若城市已存在，则返回数值0
                if (IsCityExist(cityName))
                {
                    errMsg = "The city has been exist!";
                    return 0;
                }
                //新增成功，返回数值1
                string query = "INSERT INTO dbo_LoadCity (LoadCity, LangType, IsDefault) VALUES ('"
                    + cityName + "','" + _lang + "',0)";
                return _dao.ExecuteSql(query, _dao.GetConnLoadIndex());
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return -1;
            }
        }

        // 更新当前城市名称
        /// <summary>
        /// 更新当前城市名称
        /// </summary>
        /// <param name="newName">新输入名称</param>
        /// <param name="oldName">原城市名称</param>
        /// <returns>返回受影响的记录数;1:成功更新；0：已存在；-1：Error</returns>
        public int UpdateCity(string newName, string oldName, out string errMsg)
        {
            errMsg = "";
            try
            {
                string query = "UPDATE dbo_LoadCity SET LoadCity ='" + newName
                    + "' WHERE LoadCity = '" + oldName + "' AND LangType='" + _lang+"'";
                int flag = _dao.ExecuteSql(query, _dao.GetConnLoadIndex());
                if (flag == 1)   //更新完成主表后，子表也需要更新   add on 20180710 by Vince
                {
                    string sql = "UPDATE dbo_LoadIndex SET LoadCity ='" + newName
                    + "' WHERE LoadCity = '" + oldName + "' AND LangType='" + _lang + "'";
                    _dao.ExecuteSql(sql, _dao.GetConnLoadIndex());
                    return flag;
                }
                else
                    return flag;
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return -1;
            }
        }

        // 删除指定的城市
        /// <summary>
        /// 删除指定的城市
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public bool DeleteCity(string cityName, out string errMsg)
        {
            errMsg = "";
            try
            {
                ArrayList query = new ArrayList();
                query.Add("DELETE * FROM dbo_LoadCity Where LoadCity= '" + cityName + "' and LangType= '" + _lang + "'");
                query.Add("DELETE * FROM dbo_LoadIndex Where LoadCity= '" + cityName + "' and LangType= '" + _lang + "'");
                _dao.ExecuteSqlTran(query, _dao.GetConnLoadIndex());
                return true;
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return false;
            }
        }
        #endregion

        #region 房间类型对应的 Load index 记录管理

        /// 获取当前选中房间的 LoadIndex 对象
        /// <summary>
        /// 获取当前选中房间的 LoadIndex 对象
        /// </summary>
        /// <param name="cityName">城市选项</param>
        /// <param name="rType">房间类型选项</param>
        /// <returns>返回当前选中房间的 Load index 对象</returns>
        public RoomLoadIndex GetRoomLoadIndexItem(string cityName, string rType)
        {
            if (String.IsNullOrEmpty(cityName) || String.IsNullOrEmpty(rType))
                return null;
            RoomLoadIndex item = new RoomLoadIndex(cityName, rType, _lang);// LangType.CurrentLanguage 改为_lang
            DataTable dt = new DataTable();
            string query = "SELECT * FROM dbo_LoadIndex Where LoadCity= '" + cityName
                   // + "' and LangType='" + item.Language + "' and RoomType='" + rType + "'"; 
                   //房间名称前加上城市名 20160418 by lin
                    + "' and LangType='" + item.Language + "' and LoadCity+' - '+RoomType='" + rType + "'";

            dt = _dao.GetLoadIndex(query);
            if (dt.Rows.Count > 0)
            {
                double cIndex = Convert.ToDouble(dt.Rows[0]["COOLIndex"].ToString());
                double hIndex = Convert.ToDouble(dt.Rows[0]["HEATIndex"].ToString());
                item.City = cityName;
                item.RoomType = rType;
                item.CoolingIndex = cIndex;
                item.HeatingIndex = hIndex;
                return item;
            }
            return null;
        }

        /// 获取默认的 LoadIndex 对象
        /// <summary>
        /// 获取默认的 LoadIndex 对象
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns></returns>
        public RoomLoadIndex GetDefaultRoomLoadIndexItem(string cityName)
        {
            if (String.IsNullOrEmpty(cityName))
                return null;
            RoomLoadIndex item = new RoomLoadIndex(cityName, _lang);
            DataTable dt = new DataTable();
            string query = "SELECT * FROM dbo_LoadIndex Where LoadCity= '" + cityName
                    + "' and LangType='" + _lang + "' and IsDefault=1";

            dt = _dao.GetLoadIndex(query);
            if (dt.Rows.Count > 0)
            {
                double cIndex = Convert.ToDouble(dt.Rows[0]["COOLIndex"].ToString());
                double hIndex = Convert.ToDouble(dt.Rows[0]["HEATIndex"].ToString());
                item.City = cityName;
                item.RoomType = dt.Rows[0]["RoomType"].ToString();
                item.CoolingIndex = cIndex;
                item.HeatingIndex = hIndex;
                return item;
            }
            return null;
        }

        /// 设置默认房间类型
        /// <summary>
        /// 设置默认房间类型
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <param name="rType">房间类型</param>
        /// <returns></returns>
        public bool SetDefaultRoomLoadIndex(string cityName, string rType, out string errMsg)
        {
            errMsg = "";
            try
            {
                ArrayList mySql = new ArrayList();
                mySql.Add("UPDATE dbo_LoadIndex SET dbo_LoadIndex.IsDefault = 0 Where LangType='" + _lang + "' and LoadCity= '" + cityName + "'");
                mySql.Add("UPDATE dbo_LoadIndex SET dbo_LoadIndex.IsDefault = 1 Where LangType='"
                    + _lang + "' and LoadCity= '" + cityName + "' and RoomType='" + rType + "'");
                _dao.ExecuteSqlTran(mySql, _dao.GetConnLoadIndex());
                return true;
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return false;
            }
        }

        /// 验证该城市中该房间类型记录是否已存在
        /// <summary>
        /// 验证该城市中该房间类型记录是否已存在
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <param name="rType">房间类型</param>
        /// <returns></returns>
        public bool IsRoomTypeExist(string cityName, string rType)
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM dbo_LoadIndex Where LangType='" + _lang
                + "' and LoadCity= '" + cityName + "' and RoomType='" + rType + "'";
            dt = _dao.GetLoadIndex(query);
            return dt.Rows.Count > 0;
            //return AccessOleDb.Exists(query, AccessOleDb.PerformanceCS); //also OK
        }

        /// 得到当前城市下的 LoadIndex 记录列表
        /// <summary>
        /// 得到当前城市下的 LoadIndex 记录列表
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns></returns>
        public DataTable GetRoomLoadIndexList(string cityName, string utLoadIndex)
        {
            DataTable dt = new DataTable();
            OleDbDataAdapter da = new OleDbDataAdapter();
            string query = "SELECT RoomType, COOLIndex, HEATIndex, IsDefault FROM dbo_LoadIndex Where LoadCity= '"
                + cityName.Trim() + "' and LangType= '" + _lang + "' order by IsDefault desc, RoomType asc";
            dt = _dao.GetLoadIndex(query);
            foreach (DataRow dr in dt.Rows)
            {
                double cool = dr["COOLIndex"] is DBNull ? 0 : double.Parse(dr["COOLIndex"].ToString());
                double heat = dr["HEATIndex"] is DBNull ? 0 : double.Parse(dr["HEATIndex"].ToString());
                dr["COOLIndex"] = Unit.ConvertToControl(cool, UnitType.LOADINDEX, utLoadIndex).ToString("n2");
                dr["HEATIndex"] = Unit.ConvertToControl(heat, UnitType.LOADINDEX, utLoadIndex).ToString("n2");
            }
            return dt;
        }

        /// 得到所有的房间类型的List, 用于房间下拉框
        /// <summary>
        /// 得到所有的房间类型的List, 用于房间下拉框，始终将默认房间类型排在第一位
        /// </summary>
        /// <returns></returns>
        public DataTable GetRoomTypeList()
        {
            DataTable dt = new DataTable();
            //string query = "SELECT LoadCity, RoomType FROM dbo_LoadIndex Where LangType='" + _lang + "' order by IsDefault desc, RoomType asc";
            //房间类型前面加上城市名 20160418 by lin
            string query = "SELECT LoadCity, RoomType, LoadCity+' - '+RoomType as CityRoomType FROM dbo_LoadIndex Where LangType='" + _lang + "' order by IsDefault desc, RoomType asc";
            dt = _dao.GetLoadIndex(query);
            return dt;
        }
        
        /// 添加一条新的LoadIndex记录（新的房间类型）
        /// <summary>
        /// 添加一条新的LoadIndex记录（新的房间类型）
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns>返回受影响的记录数;1:成功添加；0：已存在；-1：Error</returns>
        public int AddLoadIndex(string city, string rType, string coolingIndex, string heatingIndex, out string errMsg)
        {
            errMsg = "";
            try
            {
                if (IsRoomTypeExist(city, rType))
                    return 0;

                string query = "INSERT INTO dbo_LoadIndex (LoadCity, LangType, RoomType, COOLIndex, HEATIndex) "
                    + "VALUES ('" + city + "','" + _lang + "','" + rType + "',"
                    + "'" + coolingIndex + "','" + heatingIndex + "')";
                return _dao.ExecuteSql(query, _dao.GetConnLoadIndex());
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return -1;
            }
        }
        

        public int AddLoadIndex(RoomLoadIndex newItem, out string errMsg)
        {
            errMsg = "";
            try
            {
                if (IsRoomTypeExist(newItem.City, newItem.RoomType))
                    return 0;

                string query = "INSERT INTO dbo_LoadIndex (LoadCity, LangType, RoomType, COOLIndex, HEATIndex) "
                    + "VALUES ('" + newItem.City + "','" + _lang + "','" + newItem.RoomType + "',"
                    + "'" + newItem.CoolingIndex + "','" + newItem.HeatingIndex + "')";
                return _dao.ExecuteSql(query, _dao.GetConnLoadIndex());
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return -1;
            }
        }

        /// 更新一条新的LoadIndex记录（房间类型不可改）
        /// <summary>
        /// 更新一条新的LoadIndex记录（房间类型不可改）
        /// </summary>
        /// <param name="unitText"> 当前 Load index 使用的单位表达式 </param>
        /// <param name="errMsg"></param>
        /// <returns>返回受影响的记录数;1:执行成功；-1：Error</returns>
        public int UpdateLoadIndex(RoomLoadIndex item, out string errMsg)
        {
            errMsg = "";
            try
            {
                string query = "UPDATE dbo_LoadIndex SET COOLIndex ='" + item.CoolingIndex + "', "
                + "HEATIndex ='" + item.HeatingIndex + "' Where RoomType='" + item.RoomType + "'"
                + " and LoadCity ='" + item.City + "' and LangType ='" + item.Language + "'";
                return _dao.ExecuteSql(query, _dao.GetConnLoadIndex());
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return -1;
            }
        }

        /// 删除指定城市下指定的房间类型的LoadIndex记录
        /// <summary>
        /// 删除指定城市下指定的房间类型的LoadIndex记录
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool DeleteLoadIndex(string city, string rType, out string errMsg)
        {
            errMsg = "";
            try
            {
                string query = "DELETE * FROM dbo_LoadIndex Where LoadCity= '" + city + "' "
                + "and LangType= '" + _lang + "' and RoomType='" + rType + "'";
                _dao.ExecuteSql(query, _dao.GetConnLoadIndex());
                return true;
            }
            catch (Exception exc)
            {
                errMsg = exc.Message;
                return false;
            }
        }

        #region loadIndex Update axj 2018-02-07 
        /// <summary>
        /// 获取LoadCity 全部数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetLoadCityData()
        {
            return _dao.GetLoadIndex("SELECT * FROM dbo_LoadCity");
        }
        /// <summary>
        /// 获取LoadIndex 全部数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetLoadIndexData()
        {
            return _dao.GetLoadIndex("SELECT * FROM dbo_LoadIndex");
        }

        /// <summary>
        /// 获取LoadCity 更新数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetLoadCityDataUpdate()
        {
            DataTable dt = new DataTable();
            DataSet ds = _dao.Query("SELECT * FROM dbo_LoadCity", _dao.GetConnLoadIndexUpdate());
            if (ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;
        }
        /// <summary>
        /// 获取LoadIndex 更新数据
        /// </summary>
        /// <returns></returns>
        public DataTable GetLoadIndexDataUpdate()
        {
            DataTable dt = new DataTable();
            DataSet ds = _dao.Query("SELECT * FROM dbo_LoadIndex", _dao.GetConnLoadIndexUpdate());
            if (ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;
        }
        /// <summary>
        /// 获取 更新成功标识
        /// </summary>
        /// <returns></returns>
        public bool GetLoadIndexDataUpdateChk()
        {
            DataTable dt = new DataTable();
            bool isOk = (bool)_dao.GetSingle("SELECT IsUpdate FROM dbo_UpdateChk", _dao.GetConnLoadIndexUpdate());
            return isOk;
        }
        /// <summary>
        /// LoadIndex 更新成功标识
        /// </summary>
        /// <param name="isOk"></param>
        public void UpdateOKLoadIndex()
        {
            string sql = "Update dbo_UpdateChk set IsUpdate=true";
            _dao.ExecuteSql(sql, _dao.GetConnLoadIndexUpdate());
        }
        /// <summary>
        /// 更新LoadCity LoadIndex
        /// </summary>
        /// <param name="dtCity"></param>
        /// <param name="dtIndex"></param>
        public void UpdateLoadCityAndIndex(DataTable dtCity, DataTable dtIndex)
        {
            List<dynamic> list = new List<dynamic>();
            if (dtCity.Rows.Count > 0)
            {
                foreach (DataRow dr in dtCity.Rows)
                {
                    string sql = "insert into dbo_LoadCity(LoadCity,LangType,IsDefault)values(@LoadCity,@LangType,0)";
                    OleDbParameter[] cmdParms = new OleDbParameter[] {
                        new OleDbParameter("@LoadCity", dr["LoadCity"]),
                        new OleDbParameter("@LangType", dr["LangType"])
                    };
                    list.Add(new { sql = sql, cmdParms = cmdParms });
                }

            }
            if (dtIndex.Rows.Count > 0)
            {
                foreach (DataRow dr in dtIndex.Rows)
                {
                    string sql = "insert into dbo_LoadIndex(LoadCity,LangType,RoomType,COOLIndex,HEATIndex,IsDefault)values(@LoadCity,@LangType,@RoomType,@COOLIndex,@HEATIndex,0)";
                    OleDbParameter[] cmdParms = new OleDbParameter[] {
                        new OleDbParameter("@LoadCity", dr["LoadCity"]),
                        new OleDbParameter("@LangType", dr["LangType"]),
                        new OleDbParameter("@RoomType", dr["RoomType"]),
                        new OleDbParameter("@COOLIndex", dr["COOLIndex"]),
                        new OleDbParameter("@HEATIndex", dr["HEATIndex"])
                    };
                    list.Add(new { sql = sql, cmdParms = cmdParms });
                }
            }
            if (list.Count > 0)
            {
                _dao.ExecuteSqlTran(list, _dao.GetConnLoadIndex());
            }
        }
        #endregion



        #endregion

    }
}
