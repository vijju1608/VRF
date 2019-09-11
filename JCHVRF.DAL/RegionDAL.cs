//********************************************************************
// 文件名: RegionDAL.cs
// 描述: 定义 VRF 项目中的区域DAL类
// 作者: clh
// 创建时间: 2016-2-15
// 修改历史: 
//********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DALFactory;

namespace JCHVRF.DAL
{
    public class RegionDAL : IRegionDAL
    {

        IDataAccessObject _dao;

        public RegionDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        public MyRegion GetItem(string code)
        {
            //string sql = "select * from JCHVRF_Region where Code = '" + code + "'";

            string sql = "select t1.*,t2.RegistPassword,t2.PricePassword,t2.YorkPassword,t2.HitachiPassword from JCHVRF_Region t1 "
                         +"left join JCHVRF_Region_Password t2 on t1.code = t2.code "
                         +"where t1.Code = '" + code + "'";
            DataTable dt = _dao.GetRegionData(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                MyRegion reg = new MyRegion();
                reg.Code = code;
                reg.Region = (dr["Region"] is DBNull) ? "" : dr["Region"].ToString().Trim();
                reg.ParentRegionCode = (dr["ParentRegionCode"] is DBNull) ? "" : dr["ParentRegionCode"].ToString().Trim();
                reg.RegistPassword = (dr["RegistPassword"] is DBNull) ? "" : dr["RegistPassword"].ToString().Trim();
                reg.PricePassword = (dr["PricePassword"] is DBNull) ? "" : dr["PricePassword"].ToString().Trim();
                return reg;
            }
            return null;
        }

        /// <summary>
        /// 去掉Super选项
        /// </summary>
        /// <returns></returns>
        public DataTable GetParentRegionTable()
        {
            string sql = "select * from JCHVRF_Region where trim(ParentRegionCode)='" + "' or ParentRegionCode is null and Code <> 'Super' and DeleteFlag = 1";//
            DataTable dt = _dao.GetRegionData(sql);
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null; 
        }

        public List<MyRegion> GetParentRegionList()
        {
            //string sql = "select * from JCHVRF_Region where trim(ParentRegionCode)='" + "' or ParentRegionCode is null";//

            string sql = "select t1.*,t2.RegistPassword,t2.PricePassword,t2.YorkPassword,t2.HitachiPassword from JCHVRF_Region t1 "
                        +"left join JCHVRF_Region_Password t2 on t1.code = t2.code "
                        + "where trim(t1.ParentRegionCode)='' or t1.ParentRegionCode is null ";//
            DataTable dt = _dao.GetRegionData(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                List<MyRegion> list = new List<MyRegion>();
                foreach (DataRow dr in dt.Rows)
                {
                    string parentCode = (dr["ParentRegionCode"] is DBNull) ? "" : dr["ParentRegionCode"].ToString().Trim();
                    MyRegion reg = new MyRegion();
                    reg.Code = (dr["Code"] is DBNull) ? "" : dr["Code"].ToString().Trim();
                    reg.Region = (dr["Region"] is DBNull) ? "" : dr["Region"].ToString().Trim();
                    reg.ParentRegionCode = parentCode;
                    reg.RegistPassword = (dr["RegistPassword"] is DBNull) ? "" : dr["RegistPassword"].ToString().Trim();
                    reg.PricePassword = (dr["PricePassword"] is DBNull) ? "" : dr["PricePassword"].ToString().Trim();
                    reg.YorkPassword = (dr["YorkPassword"] is DBNull) ? "" : dr["YorkPassword"].ToString().Trim();
                    reg.HitachiPassword = (dr["HitachiPassword"] is DBNull) ? "" : dr["HitachiPassword"].ToString().Trim();
                    list.Add(reg);
                }
                return list;
            }
            return null;
        }

        public DataTable GetSubRegionTable(string pCode)                                  
        {
            string sql = "select * from JCHVRF_Region where trim(ParentRegionCode)='" + pCode + "'";
            DataTable dt = _dao.GetRegionData(sql);
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }

        
        public DataTable LoadActiveLanguage()
        {
            string sql = "select * from JCHVRF_Language where IsActive=1";
            _dao = (new GetDatabase()).GetDataAccessObject();

            DataTable dtsql = _dao.GetDataTable(sql);
            return dtsql;
        }

    }
}
