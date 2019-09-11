using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DAL;
using JCBase.Utility;

namespace JCHVRF.BLL
{
    public class CentralControllerBLL
    {
        Project _thisProject;
        CentralControllerDAL _dal;
        public CentralControllerBLL(Project project)
        {
            _thisProject = project;
            _dal = new CentralControllerDAL();
        }

        /// <summary>
        /// 获取备选区可见的控制器型号
        /// </summary>
        /// <returns></returns>
        public List<CentralController> GetAvailableControllers(string productType)
        {
            //一个Project中可能存在多个不同的ProductType，所以需要遍历获取 20160829 by Yunxiao Lin
            //string productTypeFilter = "";
            //foreach (SystemVRF sysItem in _thisProject.SystemList)
            //{
            //    if (sysItem.OutdoorItem != null)
            //    {
            //        productTypeFilter += "'"+sysItem.OutdoorItem.ProductType + "',";
            //    }
            //}
            //if (!string.IsNullOrEmpty(productTypeFilter))
            //{
            //    productTypeFilter = productTypeFilter.Substring(0, productTypeFilter.Length - 1);
            //}
            //DataTable dt = _dal.GetModels(_thisProject.SubRegionCode, _thisProject.BrandCode, _thisProject.ProductType);
            //之前逻辑：遍历多个系统的productType，sql检索in效率较低
             DataTable dt;
            if (_thisProject.RegionCode=="EU_W" || _thisProject.RegionCode == "EU_E" || _thisProject.RegionCode == "EU_S")
                dt = _dal.GetUniversalModels(_thisProject.SubRegionCode, _thisProject.BrandCode, productType);
            else
                dt = _dal.GetModels(_thisProject.SubRegionCode, _thisProject.BrandCode, productType);

            List<CentralController> types = new List<CentralController>();

            if (dt == null) return types;

            foreach (DataRow dr in dt.Rows)
            {
                CentralController ct = new CentralController();

                ct.RegionCode = dr.IsNull("RegionCode") ? "" : dr["RegionCode"].ToString();
                ct.BrandCode = dr.IsNull("BrandCode") ? "" : dr["BrandCode"].ToString();
                ct.FactoryCode = dr.IsNull("FactoryCode") ? "" : dr["FactoryCode"].ToString();
                ct.ProductType = dr.IsNull("ProductType") ? "" : dr["ProductType"].ToString();
                ParseControllerType(ct, dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString());
                ct.Description = dr.IsNull("Description") ? (dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString()) : dr["Description"].ToString();
                ct.Model_York = dr.IsNull("Model_York") ? "" : dr["Model_York"].ToString();
                ct.Model_Hitachi = dr.IsNull("Model_Hitachi") ? "" : dr["Model_Hitachi"].ToString();
                ct.Model = ct.BrandCode == "H" ? ct.Model_Hitachi : ct.Model_York;
                ct.Protocol = dr.IsNull("Protocol") ? "" : dr["Protocol"].ToString();
                ct.CompatibleModel = dr.IsNull("CompatibleModel") ? "all" : dr["CompatibleModel"].ToString();
                ct.MaxControllerNumber = dr.IsNull("MaxControllerNumber") ? -1 : (int)dr["MaxControllerNumber"];
                ct.MaxUnitNumber = dr.IsNull("MaxUnitNumber") ? -1 : (int)dr["MaxUnitNumber"];
                ct.MaxSameModel = dr.IsNull("MaxSameModel") ? -1 : (int)dr["MaxSameModel"];
                ct.MaxSameType = dr.IsNull("MaxSameType") ? -1 : (int)dr["MaxSameType"];
                ct.MaxDeviceNumber = dr.IsNull("MaxDeviceNumber") ? -1 : (int)dr["MaxDeviceNumber"];
                ct.MaxSystemNumber = dr.IsNull("MaxSystemNumber") ? -1 : (int)dr["MaxSystemNumber"];
                //ct.MaxRemoteControllerNumber = dr.IsNull("MaxRemoteControllerNumber1") ? -1 : (int)dr["MaxRemoteControllerNumber1"];
                ct.MaxRemoteControllerNumber = dr.IsNull("MaxRemoteControllerNumber") ? -1 : (int)dr["MaxRemoteControllerNumber"];
                ct.MaxIndoorUnitNumber = dr.IsNull("MaxIndoorUnitNumber") ? -1 : (int)dr["MaxIndoorUnitNumber"];
                ct.MaxWiringLength = dr.IsNull("MaxWiringLength") ? -1 : (int)dr["MaxWiringLength"];
                ct.Image = dr.IsNull("Image") ? "" : (string)dr["Image"];
                types.Add(ct);
            }

            ////Project中可能存在多个不同的productType，不同productType的控制器型号可能是一样的，所以需要对获得的控制器数据进行汇总 20160829 by Yunxiao Lin
            //foreach (DataRow dr in dt.Rows)
            //{
            //    CentralController ct = null;
            //    foreach (CentralController cc in types)
            //    {
            //        //先查找目前的控制器列表中是否已经有相同型号，如果有，则更新productType列表 20160829 by Yunxiao Lin
            //        if (cc.Model_Hitachi == dr["Model_Hitachi"].ToString())
            //        {
            //            ct = cc;
            //            if (!dr.IsNull("ProductType") && !ct.ProductTypes.Contains(dr["ProductType"].ToString()))
            //            {
            //                ct.ProductTypes.Add(dr["ProductType"].ToString());
            //            }
            //            break;
            //        }
            //    }
            //    //如果找不到该型号的控制器，则新增 20160829 by Yunxiao Lin
            //    if (ct == null)
            //    {
            //        ct = new CentralController();

            //        ct.RegionCode = dr.IsNull("RegionCode") ? "" : dr["RegionCode"].ToString();
            //        ct.BrandCode = dr.IsNull("BrandCode") ? "" : dr["BrandCode"].ToString();
            //        ct.FactoryCode = dr.IsNull("FactoryCode") ? "" : dr["FactoryCode"].ToString();
            //        ct.ProductTypes.Add(dr["ProductType"].ToString());
            //        switch ((dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString()).Trim().ToUpper())
            //        {
            //            case Model.ConstValue.ControllerType_LonWorksInterface:
            //                ct.Type = ControllerType.LonWorksInterface;
            //                break;
            //            case Model.ConstValue.ControllerType_ModBusInterface:
            //                ct.Type = ControllerType.ModBusInterface;
            //                break;
            //            case Model.ConstValue.ControllerType_BacnetAdapter:
            //            case Model.ConstValue.ControllerType_BACNetInterface:
            //                ct.Type = ControllerType.BACNetInterface;
            //                break;
            //            case Model.ConstValue.ControllerType_ONOFF:
            //                ct.Type = ControllerType.ONOFF;
            //                break;
            //            default:
            //                ct.Type = ControllerType.CentralController;
            //                break;
            //        }
            //        ct.Description = dr.IsNull("Description") ? (dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString()) : dr["Description"].ToString();
            //        ct.Model_York = dr.IsNull("Model_York") ? "" : dr["Model_York"].ToString();
            //        ct.Model_Hitachi = dr.IsNull("Model_Hitachi") ? "" : dr["Model_Hitachi"].ToString();
            //        ct.Model = ct.BrandCode == "H" ? ct.Model_Hitachi : ct.Model_York;
            //        ct.Protocol = dr.IsNull("Protocol") ? "" : dr["Protocol"].ToString();
            //        ct.CompatibleModel = dr.IsNull("CompatibleModel") ? "all" : dr["CompatibleModel"].ToString();
            //        ct.MaxControllerNumber = dr.IsNull("MaxControllerNumber") ? -1 : (int)dr["MaxControllerNumber"];
            //        ct.MaxUnitNumber = dr.IsNull("MaxUnitNumber") ? -1 : (int)dr["MaxUnitNumber"];
            //        ct.MaxSameModel = dr.IsNull("MaxSameModel") ? -1 : (int)dr["MaxSameModel"];
            //        ct.MaxSameType = dr.IsNull("MaxSameType") ? -1 : (int)dr["MaxSameType"];
            //        ct.MaxDeviceNumber = dr.IsNull("MaxDeviceNumber") ? -1 : (int)dr["MaxDeviceNumber"];
            //        ct.MaxSystemNumber = dr.IsNull("MaxSystemNumber") ? -1 : (int)dr["MaxSystemNumber"];
            //        ct.MaxRemoteControllerNumber = dr.IsNull("MaxRemoteControllerNumber") ? -1 : (int)dr["MaxRemoteControllerNumber"];
            //        ct.MaxIndoorUnitNumber = dr.IsNull("MaxIndoorUnitNumber") ? -1 : (int)dr["MaxIndoorUnitNumber"];
            //        ct.MaxWiringLength = dr.IsNull("MaxWiringLength") ? -1 : (int)dr["MaxWiringLength"];
            //        ct.Image = dr.IsNull("Image") ? "" : (string)dr["Image"];
            //        types.Add(ct);
            //    }
            //}

            //判断是否有成套使用的控制器
            seachControllerSets(types);

            return types;
        }


        /// <summary>
        /// 获取备选区可见的控制器型号
        /// </summary>
        /// <returns></returns>
        public List<CentralController> GetAvailableControllers(string productType, string OutdoorUnitType)
        {
            //一个Project中可能存在多个不同的ProductType，所以需要遍历获取 20160829 by Yunxiao Lin
            //string productTypeFilter = "";
            //foreach (SystemVRF sysItem in _thisProject.SystemList)
            //{
            //    if (sysItem.OutdoorItem != null)
            //    {
            //        productTypeFilter += "'"+sysItem.OutdoorItem.ProductType + "',";
            //    }
            //}
            //if (!string.IsNullOrEmpty(productTypeFilter))
            //{
            //    productTypeFilter = productTypeFilter.Substring(0, productTypeFilter.Length - 1);
            //}
            //DataTable dt = _dal.GetModels(_thisProject.SubRegionCode, _thisProject.BrandCode, _thisProject.ProductType);
            //之前逻辑：遍历多个系统的productType，sql检索in效率较低
            DataTable dt = _dal.GetModels(_thisProject.SubRegionCode, _thisProject.BrandCode, productType, OutdoorUnitType);

            List<CentralController> types = new List<CentralController>();

            if (dt == null) return types;

            foreach (DataRow dr in dt.Rows)
            {
                CentralController ct = new CentralController();

                ct.RegionCode = dr.IsNull("RegionCode") ? "" : dr["RegionCode"].ToString();
                ct.BrandCode = dr.IsNull("BrandCode") ? "" : dr["BrandCode"].ToString();
                ct.FactoryCode = dr.IsNull("FactoryCode") ? "" : dr["FactoryCode"].ToString();
                ct.ProductType = dr.IsNull("ProductType") ? "" : dr["ProductType"].ToString();
                ParseControllerType(ct, dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString());
                ct.Description = dr.IsNull("Description") ? (dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString()) : dr["Description"].ToString();
                ct.Model_York = dr.IsNull("Model_York") ? "" : dr["Model_York"].ToString();
                ct.Model_Hitachi = dr.IsNull("Model_Hitachi") ? "" : dr["Model_Hitachi"].ToString();
                ct.Model = ct.BrandCode == "H" ? ct.Model_Hitachi : ct.Model_York;
                ct.Protocol = dr.IsNull("Protocol") ? "" : dr["Protocol"].ToString();
                ct.CompatibleModel = dr.IsNull("CompatibleModel") ? "all" : dr["CompatibleModel"].ToString();
                ct.MaxControllerNumber = dr.IsNull("MaxControllerNumber") ? -1 : (int)dr["MaxControllerNumber"];
                ct.MaxUnitNumber = dr.IsNull("MaxUnitNumber") ? -1 : (int)dr["MaxUnitNumber"];
                ct.MaxSameModel = dr.IsNull("MaxSameModel") ? -1 : (int)dr["MaxSameModel"];
                ct.MaxSameType = dr.IsNull("MaxSameType") ? -1 : (int)dr["MaxSameType"];
                ct.MaxDeviceNumber = dr.IsNull("MaxDeviceNumber") ? -1 : (int)dr["MaxDeviceNumber"];
                ct.MaxSystemNumber = dr.IsNull("MaxSystemNumber") ? -1 : (int)dr["MaxSystemNumber"];
                //ct.MaxRemoteControllerNumber = dr.IsNull("MaxRemoteControllerNumber1") ? -1 : (int)dr["MaxRemoteControllerNumber1"];
                ct.MaxRemoteControllerNumber = dr.IsNull("MaxRemoteControllerNumber") ? -1 : (int)dr["MaxRemoteControllerNumber"];
                ct.MaxIndoorUnitNumber = dr.IsNull("MaxIndoorUnitNumber") ? -1 : (int)dr["MaxIndoorUnitNumber"];
                ct.MaxWiringLength = dr.IsNull("MaxWiringLength") ? -1 : (int)dr["MaxWiringLength"];
                ct.Image = dr.IsNull("Image") ? "" : (string)dr["Image"];
                types.Add(ct);
            }

            ////Project中可能存在多个不同的productType，不同productType的控制器型号可能是一样的，所以需要对获得的控制器数据进行汇总 20160829 by Yunxiao Lin
            //foreach (DataRow dr in dt.Rows)
            //{
            //    CentralController ct = null;
            //    foreach (CentralController cc in types)
            //    {
            //        //先查找目前的控制器列表中是否已经有相同型号，如果有，则更新productType列表 20160829 by Yunxiao Lin
            //        if (cc.Model_Hitachi == dr["Model_Hitachi"].ToString())
            //        {
            //            ct = cc;
            //            if (!dr.IsNull("ProductType") && !ct.ProductTypes.Contains(dr["ProductType"].ToString()))
            //            {
            //                ct.ProductTypes.Add(dr["ProductType"].ToString());
            //            }
            //            break;
            //        }
            //    }
            //    //如果找不到该型号的控制器，则新增 20160829 by Yunxiao Lin
            //    if (ct == null)
            //    {
            //        ct = new CentralController();

            //        ct.RegionCode = dr.IsNull("RegionCode") ? "" : dr["RegionCode"].ToString();
            //        ct.BrandCode = dr.IsNull("BrandCode") ? "" : dr["BrandCode"].ToString();
            //        ct.FactoryCode = dr.IsNull("FactoryCode") ? "" : dr["FactoryCode"].ToString();
            //        ct.ProductTypes.Add(dr["ProductType"].ToString());
            //        switch ((dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString()).Trim().ToUpper())
            //        {
            //            case Model.ConstValue.ControllerType_LonWorksInterface:
            //                ct.Type = ControllerType.LonWorksInterface;
            //                break;
            //            case Model.ConstValue.ControllerType_ModBusInterface:
            //                ct.Type = ControllerType.ModBusInterface;
            //                break;
            //            case Model.ConstValue.ControllerType_BacnetAdapter:
            //            case Model.ConstValue.ControllerType_BACNetInterface:
            //                ct.Type = ControllerType.BACNetInterface;
            //                break;
            //            case Model.ConstValue.ControllerType_ONOFF:
            //                ct.Type = ControllerType.ONOFF;
            //                break;
            //            default:
            //                ct.Type = ControllerType.CentralController;
            //                break;
            //        }
            //        ct.Description = dr.IsNull("Description") ? (dr.IsNull("ControllerType") ? "" : dr["ControllerType"].ToString()) : dr["Description"].ToString();
            //        ct.Model_York = dr.IsNull("Model_York") ? "" : dr["Model_York"].ToString();
            //        ct.Model_Hitachi = dr.IsNull("Model_Hitachi") ? "" : dr["Model_Hitachi"].ToString();
            //        ct.Model = ct.BrandCode == "H" ? ct.Model_Hitachi : ct.Model_York;
            //        ct.Protocol = dr.IsNull("Protocol") ? "" : dr["Protocol"].ToString();
            //        ct.CompatibleModel = dr.IsNull("CompatibleModel") ? "all" : dr["CompatibleModel"].ToString();
            //        ct.MaxControllerNumber = dr.IsNull("MaxControllerNumber") ? -1 : (int)dr["MaxControllerNumber"];
            //        ct.MaxUnitNumber = dr.IsNull("MaxUnitNumber") ? -1 : (int)dr["MaxUnitNumber"];
            //        ct.MaxSameModel = dr.IsNull("MaxSameModel") ? -1 : (int)dr["MaxSameModel"];
            //        ct.MaxSameType = dr.IsNull("MaxSameType") ? -1 : (int)dr["MaxSameType"];
            //        ct.MaxDeviceNumber = dr.IsNull("MaxDeviceNumber") ? -1 : (int)dr["MaxDeviceNumber"];
            //        ct.MaxSystemNumber = dr.IsNull("MaxSystemNumber") ? -1 : (int)dr["MaxSystemNumber"];
            //        ct.MaxRemoteControllerNumber = dr.IsNull("MaxRemoteControllerNumber") ? -1 : (int)dr["MaxRemoteControllerNumber"];
            //        ct.MaxIndoorUnitNumber = dr.IsNull("MaxIndoorUnitNumber") ? -1 : (int)dr["MaxIndoorUnitNumber"];
            //        ct.MaxWiringLength = dr.IsNull("MaxWiringLength") ? -1 : (int)dr["MaxWiringLength"];
            //        ct.Image = dr.IsNull("Image") ? "" : (string)dr["Image"];
            //        types.Add(ct);
            //    }
            //}

            //判断是否有成套使用的控制器
            seachControllerSets(types);

            return types;
        }

        private void ParseControllerType(CentralController ct, string controllerType)
        {
            switch (controllerType.Trim().ToUpper())
            {
                case "LONWORKS INTERFACE":
                case "LONWORKS ADAPTER":
                    ct.Type = ControllerType.LonWorksInterface;
                    break;
                case "GATEWAY TO MODBUS SYSTEMS FOR 64 IU":
                case "GATEWAY TO MODBUS SYSTEMS FOR 8 IU":
                case "MODBUS ADAPTER":
                case "MODBUS GATEWAY":
                case "MODBUS INTERFACE":
                    ct.Type = ControllerType.ModBusInterface;
                    break;
                case "BACNET ADAPTER":
                case "BACNET GATEWAY CONNECTABLE TO HC-A64MB":
                case "BACNET GATEWAY CONNECTABLE TO HC-A8MB":
                case "BACNET INTERFACE":
                    ct.Type = ControllerType.BACNetInterface;
                    break;
                case "GATEWAY TO KNX SYSTEMS FOR 16 IU":
                case "GATEWAY TO KNX SYSTEMS FOR CSNET WEB":
                    ct.Type = ControllerType.KNXInterface;
                    break;
                case "CENTRALIZED ON-OFF CONTROLLER":
                    ct.Type = ControllerType.ONOFF;
                    break;
                case "CENTRAL STATION DX SOFTWARE":
                case "CONMUTERIZED CENTRAL CONTROLLER SOFTWARE":
                    ct.Type = ControllerType.Software;
                    break;
                default:
                    ct.Type = ControllerType.CentralController;
                    break;
            }
        }

        public DataTable GetModels(string regionCode, string brandCode, string productType, string OutdoorUnitType)
        { 
            return _dal.GetModels(regionCode,brandCode,productType,OutdoorUnitType);
        }

        /// <summary>
        /// 判断是否有成套使用的控制器
        /// </summary>
        private void seachControllerSets(List<CentralController> types)
        {
            //成套使用的控制器组定义
            string[][] sets = new string[][] {
                new string[] {"PSC-AS2048WXB2", "PSC-A128WX2"}
            };

            for (int i = 0; i < sets.Length; i++)
            {
                //找出每组成套的控制器
                List<CentralController> ctInSet = types.FindAll(p => sets[i].Contains(p.Model_Hitachi));
                if (ctInSet == null) continue;
                //互相添加引用
                foreach (CentralController ct1 in ctInSet)
                {
                    ct1.ControllersInSet = new List<CentralController>();

                    foreach (CentralController ct2 in ctInSet)
                    {
                        //防止自己添加自己
                        if (ct1 == ct2) continue;

                        ct1.ControllersInSet.Add(ct2);
                    }
                }
            }
        }
    }
}
