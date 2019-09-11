using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

using JCHVRF.Model;
using JCHVRF.DALFactory;

namespace JCHVRF.DAL
{
    public class CentralControllerDAL
    {
        IDataAccessObject _dao;

        public CentralControllerDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        /// 获取Controller型号列表
        /// <summary>
        /// 获取Controller型号列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetModels(string regionCode, string brandCode, string productType)
        {
            OleDbParameter[] paras = { 
                new OleDbParameter("@RegionCode", DbType.String),
                new OleDbParameter("@BrandCode", DbType.String),
                new OleDbParameter("@ProductType", DbType.String)
            };
            paras[0].Value = regionCode;
            paras[1].Value = brandCode;
            //paras[2].Value = productType;
//            string sql = @"
//            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
//                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
//                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
//                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber1,
//                MaxIndoorUnitNumber,MaxWiringLength,Image
//            From JCHVRF_CentralController
//            Where RegionCode=@RegionCode AND BrandCode=@BrandCode AND ProductType = @ProductType 
//            Group by RegionCode,BrandCode,FactoryCode,ProductType,
//                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
//                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
//                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            //一个Project可能存在多个不同的ProductType 20160829 by Yunxiao Lin
//            string sql = @"
//            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
//                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
//                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
//                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber,
//                MaxIndoorUnitNumber,MaxWiringLength,Image
//            From JCHVRF_CentralController
//            Where RegionCode=@RegionCode AND BrandCode=@BrandCode AND ProductType in (" + productType+")"
//            +@"Group by RegionCode,BrandCode,FactoryCode,ProductType,
//                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
//                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
//                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            string sql = @"
            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber,
                MaxIndoorUnitNumber,MaxWiringLength,Image
            From JCHVRF_CentralController
            Where RegionCode=@RegionCode AND BrandCode=@BrandCode AND ProductType = '" + productType
            + @"' Group by RegionCode,BrandCode,FactoryCode,ProductType,
                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            DataTable dt = _dao.GetDataTable(sql, paras);
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }

        //add by Shen Junjie on 2018/5/5
        /// 获取Controller型号列表
        /// <summary>
        /// 获取Controller型号列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetPowerSupply(string regionCode, string brandCode, string model)
        {
            OleDbParameter[] paras = { 
                new OleDbParameter("@RegionCode", DbType.String),
                new OleDbParameter("@BrandCode", DbType.String),
                new OleDbParameter("@Model", DbType.String)
            };
            paras[0].Value = regionCode;
            paras[1].Value = brandCode;
            paras[2].Value = model;
            string sql = @"
            SELECT DISTINCT PowerSupply,PowerLineType
            From JCHVRF_CentralController
            Where instr(RegionCode,@RegionCode)>0 AND BrandCode=@BrandCode AND Model_Hitachi = @Model";
            DataTable dt = _dao.GetDataTable(sql, paras);
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }

        /// 获取Controller型号列表
        /// <summary>
        /// 获取Controller型号列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetModels(string regionCode, string brandCode, string productType, string OutdoorUnitType)
        {
            OleDbParameter[] paras = { 
                new OleDbParameter("@RegionCode", DbType.String),
                new OleDbParameter("@BrandCode", DbType.String),
                new OleDbParameter("@ProductType", DbType.String)
            };
            paras[0].Value = regionCode;
            paras[1].Value = brandCode;
            //paras[2].Value = productType;
            //            string sql = @"
            //            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
            //                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
            //                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
            //                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber1,
            //                MaxIndoorUnitNumber,MaxWiringLength,Image
            //            From JCHVRF_CentralController
            //            Where RegionCode=@RegionCode AND BrandCode=@BrandCode AND ProductType = @ProductType 
            //            Group by RegionCode,BrandCode,FactoryCode,ProductType,
            //                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
            //                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
            //                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            //一个Project可能存在多个不同的ProductType 20160829 by Yunxiao Lin
            //            string sql = @"
            //            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
            //                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
            //                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
            //                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber,
            //                MaxIndoorUnitNumber,MaxWiringLength,Image
            //            From JCHVRF_CentralController
            //            Where RegionCode=@RegionCode AND BrandCode=@BrandCode AND ProductType in (" + productType+")"
            //            +@"Group by RegionCode,BrandCode,FactoryCode,ProductType,
            //                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
            //                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
            //                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            string sql = @"
            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber,
                MaxIndoorUnitNumber,MaxWiringLength,Image
            From JCHVRF_CentralController
            Where RegionCode=@RegionCode AND BrandCode=@BrandCode AND ProductType = '" + productType
            + @"' and OutdoorUnitType='"+OutdoorUnitType+@"' Group by RegionCode,BrandCode,FactoryCode,ProductType,
                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            DataTable dt = _dao.GetDataTable(sql, paras);
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }

        /// 获取通用Controller型号列表   add on 20171214 by Lingjia Qiu
        /// <summary>
        /// 获取通用Controller型号列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetUniversalModels(string regionCode, string brandCode, string productType)
        {
            OleDbParameter[] paras = { 
                new OleDbParameter("@RegionCode", DbType.String),
                new OleDbParameter("@BrandCode", DbType.String),
                //new OleDbParameter("@ProductType", DbType.String)
            };
            paras[0].Value = regionCode;
            paras[1].Value = brandCode;            

            string sql = @"
            SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,
                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
                MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber,
                MaxIndoorUnitNumber,MaxWiringLength,Image
            From JCHVRF_CentralController
            Where instr(RegionCode,@RegionCode)>0 AND BrandCode=@BrandCode AND ProductType = '" + productType
            + @"' Group by RegionCode,BrandCode,FactoryCode,ProductType,
                ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,
                MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";
            DataTable dt = _dao.GetDataTable(sql, paras);

             //string query = "SELECT DISTINCT RegionCode,BrandCode,FactoryCode,ProductType,"
             //           +"ControllerType,Description,Model_York,Model_Hitachi,Protocol,"
             //           +"CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,"
             //           +"MaxDeviceNumber,MaxSystemNumber,Max(MaxRemoteControllerNumber) as MaxRemoteControllerNumber,"
             //           +"MaxIndoorUnitNumber,MaxWiringLength,Image "
             //           +"From JCHVRF_CentralController "
             //           + "Where RegionCode like '*" + regionCode + "*' AND BrandCode='" + brandCode + "' AND ProductType = '" + productType
             //           + "' Group by RegionCode,BrandCode,FactoryCode,ProductType,"
             //           +"ControllerType,Description,Model_York,Model_Hitachi,Protocol,"
             //           +"CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,MaxUnitNumber,"
             //           +"MaxDeviceNumber,MaxSystemNumber,MaxIndoorUnitNumber,MaxWiringLength,Image";           

             //   DataTable dt = _dao.GetDataTable(query);
         
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }
        //ACC - SHIV START
        //public DataTable GetUniversalModels(string regionCode, string brandCode)
        //{
        //    OleDbParameter[] paras = {
        //        new OleDbParameter("@RegionCode", DbType.String),
        //        new OleDbParameter("@BrandCode", DbType.String),
        //    };
        //    paras[0].Value = regionCode;
        //    paras[1].Value = brandCode;
        //    string sql = @"
        //    SELECT DISTINCT Image as TypeImage, ControllerType as UnitType, Model_Hitachi as Model, 'Controller' as SelectionType From JCHVRF_CentralController
        //    where RegionCode = '" + regionCode + @"' AND BrandCode = '" + brandCode + @"' ";
        //    /* string sql = @"
        //     SELECT DISTINCT ProductType,ControllerType,Description,Model_York,Model_Hitachi,Protocol,
        //         CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,Image,MaxUnitNumber,
        //         MaxDeviceNumber From JCHVRF_CentralController "; */
        //    DataTable dt = _dao.GetDataTable(sql);   

        //    //   DataTable dt = _dao.GetDataTable(query);

        //    if (dt != null && dt.Rows.Count > 0)
        //        return dt;
        //    return null;
        //}

        public DataTable GetCompatibleModels(string regionCode, string brandCode, string controllerModel)
        {
            OleDbParameter[] paras = {
                new OleDbParameter("@RegionCode", DbType.String),
                new OleDbParameter("@BrandCode", DbType.String),
                new OleDbParameter("@Model_Hitachi", DbType.String),
            };
            paras[0].Value = regionCode;
            paras[1].Value = brandCode;
            paras[2].Value = controllerModel;

            string sql = @"
            SELECT ControllerType,CompatibleModel, ProductType From JCHVRF_CentralController
            where Model_Hitachi = '" + controllerModel + "' AND RegionCode = '" + regionCode + @"' AND BrandCode = '" + brandCode + @"' ";
            /* string sql = @"
             SELECT DISTINCT ProductType,ControllerType,Description,Model_York,Model_Hitachi,Protocol,
                 CompatibleModel,MaxSameModel,MaxSameType,MaxControllerNumber,Image,MaxUnitNumber,
                 MaxDeviceNumber From JCHVRF_CentralController "; */
            DataTable dt = _dao.GetDataTable(sql);

            //   DataTable dt = _dao.GetDataTable(query);

            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }
    }
    //ACC - SHIV END
}
