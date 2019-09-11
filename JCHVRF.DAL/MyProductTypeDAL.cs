using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DALFactory;

namespace JCHVRF.DAL
{
    public class MyProductTypeDAL : IMyProductTypeDAL
    {
        IDataAccessObject _dao;
        public MyProductTypeDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        public MyProductType GetItem(string brandCode, string regionCode, string productType)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");
            DataView dv = dt.DefaultView;
            string sql = " BrandCode='" + brandCode + "' and RegionCode = '" + regionCode + "' and ProductType = '" + productType + "'";//FactoryCode='" + factoryCode + "' and 
            dv.RowFilter = sql;
            dt = dv.ToTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                MyProductType item = new MyProductType();
                item.BrandCode = brandCode;
                item.FactoryCode = dr["FactoryCode"].ToString();
                item.RegionCode = regionCode;
                item.ProductType = productType;
                item.Series = (dr["Series"] is DBNull) ? "" : dr["Series"].ToString();
                item.MinCoolingDB = (dr["MinCoolingDB"] is DBNull) ? 0 : Convert.ToInt32(dr["MinCoolingDB"].ToString());
                item.MaxCoolingDB = (dr["MaxCoolingDB"] is DBNull) ? 0 : Convert.ToInt32(dr["MaxCoolingDB"].ToString());
                item.MinHeatingWB = (dr["MinHeatingWB"] is DBNull) ? 0 : Convert.ToInt32(dr["MinHeatingWB"].ToString());
                item.MaxHeatingWB = (dr["MaxHeatingWB"] is DBNull) ? 0 : Convert.ToInt32(dr["MaxHeatingWB"].ToString());
                if (dt.Columns.Contains("L2SizeDownRule"))
                {
                    item.L2SizeDownRule = Convert.ToString(dr["L2SizeDownRule"]);
                }
                return item;
            }
            return null;
        }

        public DataTable GetProductTypeListBySeries(string brandCode, string regionCode, List<string> series)
        {
            if (series == null || series.Count == 0)
            {
                return null;
            }

            string filter = "BrandCode='" + brandCode + "' and FactoryCode<>'G' and RegionCode = '" + regionCode + "'";
                string joinedSeries = string.Join("','", series.ToArray());
                filter += " and Series in ('" + joinedSeries + "')";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");
            DataView dv = dt.DefaultView;
            dv.RowFilter = filter;
            dt = dv.ToTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            return null;
        }

        /// <summary>
        /// 排除FactoryCode为G的情况
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <returns></returns>
        public DataTable GetProductTypeData(string brandCode, string regionCode)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");
            DataView dv = dt.DefaultView;
            dv.RowFilter ="BrandCode='" + brandCode+ "' and FactoryCode<>'G' and RegionCode = '" + regionCode + "'";
            //string sql = "select * from JCHVRF_ProductType where BrandCode='" + brandCode
            //        + "' and FactoryCode<>'G' and RegionCode = '" + regionCode + "'";
            dv.Sort = " ProductType asc";
            dt = dv.ToTable();
           
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            return null;
        }


        public DataTable GetProductTypeData(string brandCode, string factoryCode, string regionCode)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");        
            string sql = " BrandCode='" + brandCode  + "' and RegionCode = '" + regionCode + "'";
            if (factoryCode != "G")
            {
                sql = "BrandCode='" + brandCode
                + "' and FactoryCode<>'G' and RegionCode = '" + regionCode + "'";
            } 
            DataView dv = dt.DefaultView;
            dv.Sort = " ProductType asc";
            dv.RowFilter = sql;
            dt = dv.ToTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            return null;
        }

        /// <summary>
        /// 获取新风机的productType
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <returns></returns>
        public DataTable GetFreshAirProductTypeData(string brandCode, string regionCode)
        {

            List<MyProductType> strList = new List<MyProductType>();
            // 此处GetStdTableName中的参数flag直接用“Indoor”，因为Indoor与FreshAir的数据放在同一张标准表中
            string sTableName = GetStdTableName(IndoorType.Indoor, regionCode);
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(sTableName))
            {
                String sql = "select DISTINCT t1.productType,t1.series from JCHVRF_ProductType t1 " +
                    "left join " + sTableName + " t2 on t1.productType = t2.productType " +
                    " where t2.selectionType = 'FreshAir' and t2.deleteflag = 1 and t1.BrandCode='" + brandCode + "' and t1.FactoryCode<>'G' and t1.RegionCode = '" + regionCode + "'";
                dt = _dao.GetDataTable(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        MyProductType productType = new MyProductType();
                        productType.ProductType = dr["productType"].ToString();
                        productType.Series = dr["series"].ToString();
                        //strList.Add(productType);
                    }

                }
                //if (strList != null && strList.Count > 0)
                //    return strList;
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt;
            }
            return null;
        }

        /// <summary>
        /// 根据控制器获取productType
        /// </summary>
        /// <param name="brandCode"></param>
        /// <param name="regionCode"></param>
        /// <returns></returns>
        public List<String> GetControllerProductTypeData(string regionCode,string brandCode,List<Controller> groupControllerList)
        {

            List<String> strList = new List<string>();
            string model =  brandCode.Equals("H") ? "Model_Hitachi" : "Model_York";
            string modelFilter = "";
            foreach (Controller cItem in groupControllerList)
            {
                modelFilter += "'" + cItem.Model + "',";
            }
            if (!string.IsNullOrEmpty(modelFilter))
                modelFilter = modelFilter.Substring(0, modelFilter.Length - 1);

            if (!string.IsNullOrEmpty(modelFilter))
            {
                String sql = "select DISTINCT productType from JCHVRF_CentralController " +
                    " where RegionCode = '" + regionCode + "' and BrandCode='" + brandCode + "' and " + model + " in (" + modelFilter + ")";
                DataTable dt = _dao.GetDataTable(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        strList.Add(dr["productType"].ToString());
                }
                if (strList != null && strList.Count > 0)
                    return strList;
            }
            return null;
        }
            
        //ACC-SHIV START
        public List<String> GetControllerProductTypeData(string regionCode,string brandCode,List<string> Controller)
        {
            List<String> strList = new List<string>();
            string model =  brandCode.Equals("H") ? "Model_Hitachi" : "Model_York";
            string modelFilter = "";
            foreach (var cItem in Controller)
            {
                modelFilter += "'" + cItem + "',";
            }
            //if (!string.IsNullOrEmpty(modelFilter))
            //    modelFilter = modelFilter.Substring(0, modelFilter.Length - 1);

            if (!string.IsNullOrEmpty(modelFilter))
            {
                String sql = "select DISTINCT productType from JCHVRF_CentralController " +
                             " where RegionCode = '" + regionCode + "' and BrandCode='" + brandCode + "' and " + model + " in (" + modelFilter + ")";
                DataTable dt = _dao.GetDataTable(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        strList.Add(dr["productType"].ToString());
                }
                if (strList != null && strList.Count > 0)
                    return strList;
            }
            return null;
        }
        //ACC-SHIV END

        public List<string> GetBrandCodeList(string regionCode)
        {
            //string sql = "select distinct BrandCode from JCHVRF_ProductType where RegionCode = '" + regionCode + "'";
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");        
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter =" RegionCode = '" + regionCode + "'";
                dt=dv.ToTable(true,"BrandCode");
                List<string> list = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    string code = (dr["BrandCode"] is DBNull) ? "" : dr["BrandCode"].ToString();
                    list.Add(code);
                }
                return list;
            }
            return null;
        }

        public List<string> GetFactoryCodeList(string brandCode, string regionCode)
        {
            //string sql = "select distinct FactoryCode from JCHVRF_ProductType where BrandCode = '"+brandCode
            //    + "' and RegionCode = '" + regionCode + "'";
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");        
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = " BrandCode = '" + brandCode + "' and RegionCode = '" + regionCode + "'";
                dt = dv.ToTable(true, "FactoryCode");

                List<string> list = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    string code = (dr["FactoryCode"] is DBNull) ? "" : dr["FactoryCode"].ToString();
                    list.Add(code);
                }
                return list;
            }
            return null;
        }

        /// 获取VRF数据库中标准表关系表【dbo_Relationship_Std】中的某一【标准表表名】
        /// <summary>
        /// 获取VRF数据库中标准表关系表【dbo_Relationship_Std】中的某一【标准表表名】
        /// </summary>
        /// <param name="flag">IndoorType：Indoor|FreshAir|Outdoor</param>
        /// <returns></returns>
        private string GetStdTableName(IndoorType flag, string regionCode)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Relationship_Std");
            //OleDbParameter[] paras = {
            //                             new OleDbParameter("@sType",DbType.String), 
            //                             new OleDbParameter("@sRegion",DbType.String)
            //                             //,new OleDbParameter("@sProductType",DbType.String)
            //                         };
            //paras[0].Value = flag.ToString();
            //paras[1].Value = _region;
            //paras[2].Value = _productType;

            // 注意：
            // Indoor与FreshAir表对应列名均为“IndoorTableName”;Outdoor表对应的列名为“OutdoorTableName”

            string colName = "TableName";
            //string sql = "SELECT DISTINCT " + colName + " as TableName From JCHVRF_Relationship_Std "
            //+ "Where SelectionType = @sType and RegionCode = @sRegion and Deleteflag = 1"; //and ProductType = @sProductType 

            DataView dv = dt.DefaultView;
            dv.RowFilter = "  SelectionType ='" + flag.ToString() + "' and RegionCode ='" + regionCode + "' and Deleteflag = 1";
            dt = dv.ToTable(true, colName);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0][colName].ToString();
            }
            return "";
        }

        public DataTable GetOduSeriesByIduType(string brandCode, string regionCode, string types)
        {
            if (string.IsNullOrEmpty(types))
            {
                return null;
            }

            string filter = "BrandCode='" + brandCode + "' and RegionCode like '%/" + regionCode + "/%'"
                            + " and IDU_UnitType in (" + types + ")";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Relation_Universal");
            DataView dv = dt.DefaultView; 
            dv.RowFilter = filter;
            dt =  dv.ToTable(true, new string[] { "ODU_Series"}); 
            if (dt != null && dt.Rows.Count > 0)
            {
                //判断是否存在Commercial VRF HP, FSXNSE
                dt = Datatable_Sort(dt, "ODU_Series", "Commercial VRF HP, FSXNSE");
                return dt;
            }
            return null;
        }


        public DataTable GetOduSeries(string brandCode, string regionCode)
        {

            string filter = "BrandCode='" + brandCode + "' and RegionCode like '%/" + regionCode + "/%'";
                       
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Relation_Universal");
            DataView dv = dt.DefaultView;
            dv.RowFilter = filter;
            dt = dv.ToTable(true, new string[] { "ODU_Series" });
            if (dt != null && dt.Rows.Count > 0)
            {
                //判断是否存在Commercial VRF HP, FSXNSE
                dt = Datatable_Sort(dt, "ODU_Series", "Commercial VRF HP, FSXNSE");
                return dt;
            }
            return null;
        }

        //指定排序列值进行排序
        private DataTable Datatable_Sort(DataTable dt,string colmun,string values)
        {
            DataTable dtResult = new DataTable();
            dtResult = dt.Clone();
            
            bool isExists = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][colmun].ToString() == values)
                {
                    isExists = true;
                    break;
                }
            }
            if (isExists)
            {
                if (values == "Commercial VRF HP, FSXNSE")
                {
                    dtResult.Rows.Add(values);
                    dtResult.Rows.Add("Commercial VRF HR, FSXNSE");
                }
                DataView dv = dt.DefaultView;
                dv.RowFilter = colmun + " not in ('Commercial VRF HP, FSXNSE','Commercial VRF HR, FSXNSE')";
                dt = dv.ToTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dtResult.Rows.Add(dt.Rows[i][colmun].ToString());
                }
            }
            else
            {
                return dt;
            }

            return dtResult;
        }

        public DataTable GetIduTypeBySeries(string brandCode, string regionCode, string series)
        {
            if (string.IsNullOrEmpty(series))
            {
                return null;
            }

            string filter = "BrandCode='" + brandCode + "' and RegionCode like '%/" + regionCode + "/%' and ODU_Series = '" + series+"'";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Relation_Universal");
            DataView dv = dt.DefaultView;
            dv.RowFilter = filter;
            dt = dv.ToTable(true, new string[] { "IDU_UnitType", "IDU_FactoryCode", "IDU_Model_Hitachi" });
            if (dt != null && dt.Rows.Count > 0)
            {

                return dt;
            }
            return null;
        }

        public string GetProductTypeBySeries(string brandCode, string regionCode, string series)
        {
            if (string.IsNullOrEmpty(series))
            {
                return null;
            }

            string filter = "BrandCode='" + brandCode + "' and RegionCode = '" + regionCode + "' and Series = '" + series + "'";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");
            DataView dv = dt.DefaultView;
            dv.RowFilter = filter;
            dt = dv.ToTable(true, new string[] { "ProductType" });
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0]["ProductType"].ToString();               
            
            return null;
        }

        public string GetSeriesByProductType(string brandCode, string regionCode, string productType)
        {
            if (string.IsNullOrEmpty(productType))
            {
                return null;
            }

            string filter = "BrandCode='" + brandCode + "' and RegionCode = '" + regionCode + "' and ProductType = '" + productType + "'";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_ProductType");
            DataView dv = dt.DefaultView;
            dv.RowFilter = filter;
            dt = dv.ToTable(true, new string[] { "Series" });
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0]["Series"].ToString();

            return null;
        }
    }
}
