using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DAL;

namespace JCHVRF.BLL
{
    public class MyProductTypeBLL
    {
        MyProductTypeDAL _dal;

        public MyProductTypeBLL()
        {
            _dal = new MyProductTypeDAL();
        }

        public MyProductType GetItem(string brandCode, string regionCode, string productType)
        {
            return _dal.GetItem(brandCode, regionCode, productType);
        }

        /// <summary>
        /// 得到可以分配给新系统的所有室内机的系列
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <param name="indoors"></param>
        /// <returns></returns>
        public DataTable GetProductTypeListOfAssignableIndoors(string brandCode, string regionCode, List<RoomIndoor> indoors)
        {
            List<string> series = new List<string>();
            foreach (RoomIndoor ri in indoors)
            {
                if (string.IsNullOrEmpty(ri.SystemID) && !series.Contains(ri.IndoorItem.Series))
                {
                    series.Add(ri.IndoorItem.Series);
                }
            }
            return _dal.GetProductTypeListBySeries(brandCode, regionCode, series);
        }

        public DataTable GetUsedProductTypeData(Project project)
        {
            string brandCode = project.BrandCode;
            string regionCode = project.SubRegionCode;
            List<string> series = new List<string>();
            foreach (SystemVRF sys in project.SystemList)
            {
                string s = sys.OutdoorItem.Series;
                if (!series.Contains(s))
                {
                    series.Add(s);
                }
            }
            
            DataTable dt = _dal.GetProductTypeListBySeries(brandCode, regionCode, series);
            //为Total Heat Exchanger Central Controller 增加product type 类型。
            List<string> exchangerSeries = new List<string>();
            foreach (RoomIndoor ri in project.ExchangerList)
            {
                string Series = ri.IndoorItem.Series;
                if (project.RegionCode == "EU_W" || project.RegionCode == "EU_E" || project.RegionCode == "EU_S")
                    Series = ri.IndoorItem.Type; 
                if (!exchangerSeries.Contains(Series))
                {
                    exchangerSeries.Add(Series);
                    
                    DataRow dr = dt.NewRow();
                    dr["RegionCode"] = regionCode;
                    dr["BrandCode"] = brandCode;
                    dr["FactoryCode"] = ri.IndoorItem.ModelFull.Last();
                    dr["ProductType"] = Series;
                    dr["Series"] = Series;
                    dr["MinCoolingDB"] = 10;
                    dr["MaxCoolingDB"] = 43;
                    dr["MinHeatingWB"] = 14;
                    dr["MaxHeatingWB"] = 24;
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        public DataTable GetProductTypeData(string brandCode, string regionCode)
        {
            return _dal.GetProductTypeData(brandCode, regionCode);
        }

        public DataTable GetFreshAirProductTypeData(string brandCode, string regionCode)
        {
            return _dal.GetFreshAirProductTypeData(brandCode, regionCode);
        }

        public List<string> GetBrandCodeList(string regionCode)
        {
            return _dal.GetBrandCodeList(regionCode);
        }

        public List<string> GetFactoryCodeList(string brandCode, string regionCode)
        {
            return _dal.GetFactoryCodeList(brandCode, regionCode);
        }

        public List<String> GetControllerProductTypeData(string regionCode,string brandCode, List<Controller> GroupControllerList)
        {
            return _dal.GetControllerProductTypeData(regionCode, brandCode, GroupControllerList);
        }
        //ACC-SHIV START
        public List<String> GetControllerProductTypeData(string regionCode,string brandCode, List<string> controller)
        {
            return _dal.GetControllerProductTypeData(regionCode, brandCode, controller);
        }
        //ACC-SHIV END

        /// <summary>
        /// 得到可以分配给新系统的所有室内机的系列
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <param name="indoors"></param>
        /// <returns></returns>
        public DataTable GetOduSeriesByIduType(string brandCode, string regionCode, List<RoomIndoor> indoors)
        {
            List<string> types = new List<string>();
            string typeStr = "";
            foreach (RoomIndoor ri in indoors)
            {
                if (string.IsNullOrEmpty(ri.SystemID) && !types.Contains(ri.IndoorItem.Type))   //维护未分配室内机的unitType
                {
                    types.Add(ri.IndoorItem.Type);
                }
            }

            if (types.Count > 0)
            {
                foreach (string type in types)
                {
                    typeStr += "'"+type + "',";   //维护unitType条件字符串

                }

            }
            else
                return null;

            return _dal.GetOduSeriesByIduType(brandCode, regionCode, typeStr.Substring(0,typeStr.Length - 1));
        }

        public DataTable GetOduSeries(string brandCode, string regionCode)
        {
            return _dal.GetOduSeries(brandCode, regionCode);
        }


        /// <summary>
        /// 通过室外机的系列获取室内机的类型
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        public DataTable GetIduTypeBySeries(string brandCode, string regionCode, string series)
        {
            return _dal.GetIduTypeBySeries(brandCode, regionCode, series);
        }

        /// <summary>
        /// 通过室外机的系列获取productType
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        public string GetProductTypeBySeries(string brandCode, string regionCode, string series)
        {
            return _dal.GetProductTypeBySeries(brandCode, regionCode, series); 
        }

        /// <summary>
        /// 通过室外机的ProductType获取系列
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <param name="productType"></param>
        /// <returns></returns>
        public string GetSeriesByProductType(string brandCode, string regionCode, string productType)
        {
            return _dal.GetSeriesByProductType(brandCode, regionCode, productType);
        }
    }
}
