using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

using JCHVRF.Model;
using JCHVRF.DALFactory;
using System.Collections;

namespace JCHVRF.DAL
{
    public class ProjectDAL : IProjectDAL
    {
        IDataAccessObject _dao;

        public ProjectDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        /// 获取产品类型列表
        /// <summary>
        /// 获取产品类型列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetProductTypeList(string regionVRF)
        {
            OleDbParameter[] paras = { new OleDbParameter("@sRegion", DbType.String) };
            paras[0].Value = regionVRF;
            string sql = "SELECT DISTINCT ProductType From dbo_Relationship_Std Where Region =@sRegion and Deleteflag=1";
            DataTable dt = _dao.GetDataTable(sql, paras);
            if (dt != null && dt.Rows.Count > 0)
                return dt;
            return null;
        }

        public string CompatOldProjectRegion()
        {
            //获取所有子region的数据
            DataTable dtr = _dao.GetRegionData("select Code, ParentRegionCode from JCHVRF_Region where ParentRegionCode is not null");
            Hashtable tbSubRegion = new Hashtable();
            foreach (DataRow dr in dtr.Rows)
            {
                tbSubRegion.Add(dr["Code"].ToString(), dr["ParentRegionCode"].ToString());
            }
            //轮询Project库中的UnitInfo表，变更Label数据
            string ConnectionString_Project = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\Project.mdb;Persist Security Info=False;Jet OLEDB:Database Password=YqJz2010Co04Ir15Kf;";
            using (OleDbConnection conn = new OleDbConnection(ConnectionString_Project))
            {
                OleDbDataAdapter da = new OleDbDataAdapter("select * from UnitInfo", ConnectionString_Project);
                //OleDbCommandBuilder cd = new OleDbCommandBuilder(da);
                DataTable dtp = new DataTable();
                da.Fill(dtp);
                string strLabel = "";
                bool bUpdate = false;
                foreach(DataRow dr in dtp.Rows)
                {
                    strLabel = dr["Label"].ToString();
                    if(tbSubRegion.ContainsKey(strLabel))
                    {
                        strLabel = tbSubRegion[strLabel].ToString() + "/" + strLabel;
                        dr["Label"] = strLabel;
                        bUpdate = true;
                    }
                }
                if (bUpdate)
                {
                    try
                    {
                        da.Update(dtp);
                        dtp.AcceptChanges();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return "";
        }
    }
}
