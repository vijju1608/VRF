using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF.Model;
using System.Data;
using JCBase.UI;
using System.Collections;
using JCHVRF.VRFTrans;

namespace JCHVRF.BLL
{
    public class AccessoryDisplayType
    {

        /// <summary>
        /// 获取DisplayName 通过对应类型名称
        /// </summary>
        /// <param name="dtUniversal">DisplayName 类型表</param>
        /// <param name="type">类型</param>
        /// <param name="Model">配件名称</param>
        /// <param name="ri">室内机</param>
        /// <returns></returns>
        /// 
        public static string GetAccessoryDisplayTypeByModel(string SubRegionCode, DataTable dtUniversal, string type, string Model, RoomIndoor ri)
        {
            string result = type;
            if (dtUniversal == null || Model == null || ri == null)
                return result;
            string factoryCode = ri.IndoorItem.ModelFull.Substring(ri.IndoorItem.ModelFull.Length - 1, 1);
            if (type == "Remote Control Cable")
            {
                switch (Model)
                {
                    case "PRC-10E1":
                        result = "2P-Extension cord (10 metres)";
                        break;
                    case "PRC-15E1":
                        result = "2P-Extension cord (15 metres)";
                        break;
                    case "PRC-20E1":
                        result = "2P-Extension cord (20 metres)";
                        break;
                    case "PRC-30E1":
                        result = "2P-Extension cord (30 metres)";
                        break;
                    default:
                        result = "Remote Control Cable";
                        break;
                }
                return result;
            }
            if (type == "Share Remote Control")
            {
                switch (Model)
                {
                    case "PC-ARH":
                        result = "Simplified remote control";
                        break;
                    default:
                        result = "Remote control with timer";
                        break;
                }
                return result;
            }

            if (type == "Receiver Kit for Wireless Control")
            {
                if (SubRegionCode == "TW")  //如果当前区域是台湾 直接获取其Display Name 返回 on 20180801 by xyj
                {
                    return GetDisplayName(SubRegionCode, factoryCode, type, Model, dtUniversal, ri.IndoorItem.Type);
                }
                if (ri.IndoorItem.Model_Hitachi.StartsWith("RPC-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3E") && Model == "PC-ALHZ1")
                {
                    result = "Receiver kit for wireless remote control";
                }
                else if (Model == "PC-ALH3" || Model == "PC-ALHP1" || Model == "PC-ALH3" || Model == "PC-ALHC1" || Model == "PC-ALHD1")
                {
                    result = "Receiver kit for wireless remote control(On the panel)";
                }
                else
                {
                    result = "Receiver kit for wireless remote control(On the wall)";
                }
                return result;
            }
            result = GetDisplayName(SubRegionCode, factoryCode, type, Model, dtUniversal, ri.IndoorItem.Type);
            return result;
        }
        public static string GetAccessoryDisplayTypeByModel(string SubRegionCode, DataTable dtUniversal, string type, string Model, RoomIndoor ri, bool isTrans)
        {
            string result = type;
            if (dtUniversal == null || Model == null || ri == null)
                return result;
            string factoryCode = ri.IndoorItem.ModelFull.Substring(ri.IndoorItem.ModelFull.Length - 1, 1);
            if (type == "Remote Control Cable")
            {
                switch (Model)
                {
                    case "PRC-10E1":
                        result = "2P-Extension cord (10 metres)";
                        break;
                    case "PRC-15E1":
                        result = "2P-Extension cord (15 metres)";
                        break;
                    case "PRC-20E1":
                        result = "2P-Extension cord (20 metres)";
                        break;
                    case "PRC-30E1":
                        result = "2P-Extension cord (30 metres)";
                        break;
                    default:
                        result = "Remote Control Cable";
                        break;
                }
                return result;
            }
            if (type == "Share Remote Control")
            {
                switch (Model)
                {
                    case "PC-ARH":
                        result = "Simplified remote control";
                        break;
                    default:
                        result = "Remote control with timer";
                        break;
                }
                return result;
            }

            if (type == "Receiver Kit for Wireless Control")
            {
                if (SubRegionCode == "TW")  //如果当前区域是台湾 直接获取其Display Name 返回 on 20180801 by xyj
                {
                    return GetDisplayName(SubRegionCode, factoryCode, type, Model, dtUniversal, ri.IndoorItem.Type);
                }
                if (ri.IndoorItem.Model_Hitachi.StartsWith("RPC-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3E") && Model == "PC-ALHZ1")
                {
                    result = "Receiver kit for wireless remote control";
                }
                else if (Model == "PC-ALH3" || Model == "PC-ALHP1" || Model == "PC-ALH3" || Model == "PC-ALHC1" || Model == "PC-ALHD1")
                {
                    result = "Receiver kit for wireless remote control(On the panel)";
                }
                else
                {
                    result = "Receiver kit for wireless remote control(On the wall)";
                }
                return result;
            }
            
            if (isTrans)
            {
                //翻译
                Trans trans = new Trans();
                result = trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), GetDisplayName(SubRegionCode, factoryCode, type, Model, dtUniversal, ri.IndoorItem.Type));               
            }
            else
                result = GetDisplayName(SubRegionCode, factoryCode, type, Model, dtUniversal, ri.IndoorItem.Type);

            return result;

        }

       
        private static string GetDisplayName(string regionCode, string factoryCode, string type, string Model, DataTable dtUniversal, string indoorType)
        {
            string result = "";
            string sql = " 1=1 and AccessoryType='" + indoorType + "' and FactoryCode='" + factoryCode + "'";
            DataView dv = dtUniversal.DefaultView;
            dv.RowFilter = sql;
            DataTable dt = dv.ToTable();

            foreach (DataRow dr in dt.Rows)
            {
                if (regionCode == "TW")
                {
                    if (dr["Model_Hitachi"].ToString() == Model && dr["Universal_Name"].ToString() == type)
                    {
                        result = dr["Display_Name"].ToString();
                        return result;
                    }
                }
                else
                {
                    if (dr["Universal_Name"].ToString() == type)
                    {
                        result = dr["Display_Name"].ToString();
                        return result;
                    }
                }
            }
            return result;
        }

        public static string GetAccessoryDisplayTypeReport(string SubRegionCode, string BrandCode, Accessory item, RoomIndoor ri)
        {
            string result = item.Type;
            if (item == null || ri == null)
                return result;
            DataTable dtUniversal = new AccessoryBLL().GetUniversal_ByAccessory(SubRegionCode, BrandCode);

            if (dtUniversal == null|| dtUniversal.Rows.Count == 0)
                return result;

            return GetAccessoryDisplayTypeByModel(SubRegionCode, dtUniversal, item.Type, item.Model_Hitachi, ri, false);
        }
    }
}
