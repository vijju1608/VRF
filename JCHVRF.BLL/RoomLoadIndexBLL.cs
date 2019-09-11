using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DAL;
using System.Collections;

namespace JCHVRF.BLL
{
    public class RoomLoadIndexBLL
    {
        RoomLoadIndexDAL dal;

        public RoomLoadIndexBLL()
        {
            dal = new RoomLoadIndexDAL();
        }

        // 获取系统中设置的默认城市
        /// <summary>
        /// 获取系统中设置的默认城市
        /// </summary>
        /// <param name="str"></param>
        public string GetDefaultCity()
        {
            return dal.GetDefaultCity();
        }

        // 将指定城市设为默认城市
        /// <summary>
        /// 将指定城市设为默认城市
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public bool SetDefaultCity(string cityName, out string errMsg)
        {
            return dal.SetDefaultCity(cityName, out errMsg);
        }

        // 检验城市名称是否已经存在
        /// <summary>
        /// 检验城市名称是否已经存在
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public bool IsCityExist(string cityName)
        {
            return dal.IsCityExist(cityName);
        } 

        // 获取所有城市的名称列表
        /// <summary>
        /// 获取所有城市的名称列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetCityList()
        {
            return dal.GetCityList();
        }

        // 添加一个城市记录
        /// <summary>
        /// 添加一个城市记录
        /// </summary>
        /// <param name="cityName">新增的城市名称</param>
        /// <returns>返回受影响的记录数;1:成功添加；0：已存在；-1：Error</returns>
        public int AddCity(string cityName, out string errMsg)
        {
            return dal.AddCity(cityName, out errMsg);
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
            return dal.UpdateCity(newName, oldName, out errMsg);
        }

        // 删除指定的城市
        /// <summary>
        /// 删除指定的城市
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public bool DeleteCity(string cityName, out string errMsg)
        {
            return dal.DeleteCity(cityName, out errMsg);
        }




        // 获取当前选中房间的 LoadIndex 对象
        /// <summary>
        /// 获取当前选中房间的 LoadIndex 对象
        /// </summary>
        /// <param name="cityName">城市选项</param>
        /// <param name="rType">房间类型选项</param>
        /// <returns>返回当前选中房间的 Load index 对象</returns>
        public RoomLoadIndex GetRoomLoadIndexItem(string cityName, string rType)
        {
            return dal.GetRoomLoadIndexItem(cityName, rType);
        }

        // 获取默认的 LoadIndex 对象
        /// <summary>
        /// 获取默认的 LoadIndex 对象
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns></returns>
        public RoomLoadIndex GetDefaultRoomLoadIndexItem(string cityName)
        {
            return dal.GetDefaultRoomLoadIndexItem(cityName);
        }

        // 设置默认房间类型
        /// <summary>
        /// 设置默认房间类型
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <param name="rType">房间类型</param>
        /// <returns></returns>
        public bool SetDefaultRoomLoadIndex(string cityName, string rType, out string errMsg)
        {
            return dal.SetDefaultRoomLoadIndex(cityName, rType, out errMsg);
        }

        // 验证该城市中该房间类型记录是否已存在
        /// <summary>
        /// 验证该城市中该房间类型记录是否已存在
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <param name="rType">房间类型</param>
        /// <returns></returns>
        public bool IsRoomTypeExist(string cityName, string rType)
        {
            return dal.IsRoomTypeExist(cityName, rType);
        }

        // 得到当前城市下的 LoadIndex 记录列表
        /// <summary>
        /// 得到当前城市下的 LoadIndex 记录列表
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <param name="utText">单位表达式</param>
        /// <returns></returns>
        public DataTable GetRoomLoadIndexList(string cityName, string utText)
        {
            return dal.GetRoomLoadIndexList(cityName, utText);
        }

        // 得到所有的房间类型的List, 用于房间下拉框
        /// <summary>
        /// 得到所有的房间类型的List, 用于房间下拉框，始终将默认房间类型排在第一位
        /// </summary>
        /// <returns></returns>
        public DataTable GetRoomTypeList()
        {
            return dal.GetRoomTypeList();
        }

        // 添加一条新的LoadIndex记录（新的房间类型）
        /// <summary>
        /// 添加一条新的LoadIndex记录（新的房间类型）
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns>返回受影响的记录数;1:成功添加；0：已存在；-1：Error</returns>
        public int AddLoadIndex(string city, string rType, string coolingIndex, string heatingIndex, out string errMsg)
        {
            return dal.AddLoadIndex(city, rType, coolingIndex, heatingIndex, out errMsg);
        }

        // 添加一条新的LoadIndex记录（新的房间类型）
        /// <summary>
        /// 添加一条新的LoadIndex记录（新的房间类型）
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns>返回受影响的记录数;1:成功添加；0：已存在；-1：Error</returns>
        public int AddLoadIndex(RoomLoadIndex newItem, out string errMsg)
        {
            return dal.AddLoadIndex(newItem, out errMsg);
        }

        // 更新一条新的LoadIndex记录（房间类型不可改）
        /// <summary>
        /// 更新一条新的LoadIndex记录（房间类型不可改）
        /// </summary>
        /// <param name="unitText"> 当前 Load index 使用的单位表达式 </param>
        /// <param name="errMsg"></param>
        /// <returns>返回受影响的记录数;1:执行成功；-1：Error</returns>
        public int UpdateLoadIndex(RoomLoadIndex item, out string errMsg)
        {
            return dal.UpdateLoadIndex(item, out errMsg);
        }

        // 删除指定城市下指定的房间类型的LoadIndex记录
        /// <summary>
        /// 删除指定城市下指定的房间类型的LoadIndex记录
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool DeleteLoadIndex(string city, string rType, out string errMsg)
        {
            return dal.DeleteLoadIndex(city, rType, out errMsg);
        }

        /// <summary>
        /// 更新LoadCity LoadIndex 数据
        /// </summary>
        public void UpdateLoadIndexData()
        {
            bool isUpdate = dal.GetLoadIndexDataUpdateChk();
            if (!isUpdate)
            {
                Hashtable htCity = new Hashtable();
                Hashtable htIndex = new Hashtable();
                DataTable dtCity = dal.GetLoadCityData();
                DataTable dtCityUpt = dal.GetLoadCityDataUpdate();
                DataTable dtIndex = dal.GetLoadIndexData();
                DataTable dtIndexUpt = dal.GetLoadIndexDataUpdate();
                DataTable dtCityInsert = dtCity.Clone();
                DataTable dtIndexInsert = dtIndex.Clone();
                foreach (DataRow dr in dtCity.Rows)
                {
                    htCity[dr["LoadCity"] + "#" + dr["LangType"]] = "";
                }
                foreach (DataRow dr in dtCityUpt.Rows)
                {
                    string key = dr["LoadCity"] + "#" + dr["LangType"];
                    if (htCity[key] == null)
                    {
                        dtCityInsert.Rows.Add(dr.ItemArray);
                    }
                }
                foreach (DataRow dr in dtIndex.Rows)
                {
                    htIndex[dr["LoadCity"] + "#" + dr["LangType"] + "#" + dr["RoomType"] + "#" + dr["COOLIndex"] + "#" + dr["HEATIndex"]] = "";
                }
                foreach (DataRow dr in dtIndexUpt.Rows)
                {
                    string key = dr["LoadCity"] + "#" + dr["LangType"] + "#" + dr["RoomType"] + "#" + dr["COOLIndex"] + "#" + dr["HEATIndex"];
                    if (htIndex[key] == null)
                    {
                        dtIndexInsert.Rows.Add(dr.ItemArray);
                    }
                }
                dal.UpdateLoadCityAndIndex(dtCityInsert,dtIndexInsert);
                dal.UpdateOKLoadIndex();
            }
        }

        /// 获取楼层房间名 
        /// <summary>
        /// 获取楼层房间名   add on 20180608 by Vince
        /// </summary>
        /// <param name="ri"></param>
        /// /// <param name="thisPro"></param>
        public string getFloorRoomName(RoomIndoor ri, Project thisPro)
        {
            foreach (Floor f in thisPro.FloorList)
            {
                foreach (Room rm in f.RoomList)
                {
                    if (rm.Id == ri.RoomID)
                    {
                        return f.Name + ":" + ri.RoomName;
                    }
                }
            }
            return null;
        }

    }
}
