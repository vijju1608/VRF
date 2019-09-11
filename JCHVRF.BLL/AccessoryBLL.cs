//********************************************************************
// 文件名: AccessoryBLL.cs
// 描述: 定义 VRF 项目中的附件BLL类
// 作者: clh
// 创建时间: 2016-2-15
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCHVRF.Model;
using JCHVRF.DAL;
using System.Data;

namespace JCHVRF.BLL
{
    public class AccessoryBLL
    {
        AccessoryDAL _dal;

        public AccessoryBLL()
        {
            _dal = new AccessoryDAL();
        }

        public List<Accessory> GetDefault(Indoor inItem, string RegionCode, string SubRegionCode, string Series)
        {
            return _dal.GetDefault(inItem, RegionCode, SubRegionCode, Series);
        }

        public Accessory GetItems(string type, string model_Hitachi, Indoor inItem, string RegionCode, string SubRegionCode)
        {
            return _dal.GetItems(type, model_Hitachi, inItem, RegionCode, SubRegionCode);
        }

        public Accessory GetItem(string type, Indoor inItem, string RegionCode, string SubRegionCode)
        {
            return _dal.GetItem(type, inItem, RegionCode, SubRegionCode);
        }

        public DataTable GetAllAvailable(Indoor inItem, string RegionCode, string SubRegionCode)
        {
            return _dal.GetAllAvailable(inItem, RegionCode, SubRegionCode);
        }

        public DataTable GetAllAvailable(List<Indoor> inItemList, string RegionCode, string SubRegionCode)
        {
            return _dal.GetAllAvailable(inItemList, RegionCode, SubRegionCode);
        }

        public DataTable GetAllAvailableSameAccessory(List<Indoor> inItemList, string RegionCode, string SubRegionCode)
        {
            return _dal.GetAllAvailableSameAccessory(inItemList, RegionCode, SubRegionCode);
        }
       

        public DataTable GetAvailableType(string BrandCode, string FactoryCode, string ItemType, string RegionCode, string SubRegionCode)
        {
            return _dal.GetAvailableType(BrandCode, FactoryCode, ItemType, RegionCode, SubRegionCode);
        }

        public DataTable GetAvailableListType(string BrandCode, string FactoryCode, string ItemType, string RegionCode, RoomIndoor ri, string SubRegionCode)
        {
            return _dal.GetAvailableListType(BrandCode, FactoryCode, ItemType, RegionCode, ri, SubRegionCode);
        }


        public DataTable GetInDoorAccessoryItemList(string BrandCode, string FactoryCode, string UnitType, double Capacity, string RegionCode, RoomIndoor ri, string SubRegionCode)
        {
            return _dal.GetInDoorAccessoryItemList(BrandCode, FactoryCode, UnitType, Capacity, RegionCode, ri, SubRegionCode);
        }

        public DataTable GetShareAccessoryItemList(string BrandCode, string FactoryCode, string UnitType, string Type, double Capacity, string RegionCode, string SubRegionCode)
        {
            return _dal.GetShareAccessoryItemList(BrandCode, FactoryCode, UnitType, Type, Capacity, RegionCode, SubRegionCode);
        }

        public DataTable GetUniversal_ByAccessory(string SubRegionCode, string BrandCode)
        {
            return _dal.GetUniversal_ByAccessory(SubRegionCode, BrandCode);
        }

        /// <summary>
        /// 兼容旧数据 axj 20160707
        /// </summary>
        /// <param name="inItem"></param>
        public static void CompatType(RoomIndoor ri)
        {
            if (ri.ListAccessory == null) return;
            Indoor inItem = ri.IndoorItem;
            if (inItem == null)
                return;
            var list = ri.ListAccessory;
            for (int i = 0; i < list.Count; i++)
            {
                var ent = list[i];
                if (ent == null) continue;
                //修正产品类型
                if (ent.UnitType != inItem.Type)
                {
                    ent.UnitType = inItem.Type;
                }
                //修正工厂代码
                //string modelfull = inItem.ModelFull;
                //string fCode = modelfull.Substring(modelfull.Length - 1, 1);
                // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
                string fCode = inItem.GetFactoryCodeForAccess();
                if (ent.FactoryCode != fCode)
                {
                    ent.FactoryCode = fCode;
                }
                //旧数据Type修正
                switch (ent.Type)
                {
                    case "Drainage Pump": ent.Type = "Drain Pump"; break;
                    case "Half size remote control switch": ent.Type = "Half-size Remote Control Switch"; break;
                    case "Liquid Crystal Remote Control Switch": ent.Type = "Remote Control Switch"; break;
                    case "Panel": ent.Type = "Air panel"; break;
                    case "Remote Controler": ent.Type = "Remote Control Switch"; break;
                    case "Wireless Receiver Kit": ent.Type = "Receiver Kit for Wireless Control"; break;
                    default: break;
                }
            }
        }

    }
}
