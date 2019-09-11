//********************************************************************
// 文件名: IVXCombinationDAL.cs
// 描述: 定义 IVX 系统中ODU和IDU型号组合DAL类
// 作者: Yunxiao Lin
// 创建时间: 2017-7-1
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
    public class IVXCombinationDAL : IIVXCombinationDAL
    {
        IDataAccessObject _dao;

        public IVXCombinationDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        public IVXCombination getCombination(string ODUmodel)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_IVX_Combination");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "ODU_model='" + ODUmodel + "'";
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    IVXCombination IVXCom = new IVXCombination();
                    IVXCom.ODUModel = ODUmodel;
                    IVXCom.IDUModels = new List<string>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        IVXCom.IDUModels.Add(dr["IDU_models"].ToString());
                    }
                    return IVXCom;
                }
            }
            return null;
        }

        public bool existsCombination(string ODUmodel, string IDUmodels)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_IVX_Combination");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "ODU_model='" + ODUmodel + "' and IDU_models='" + IDUmodels + "'";
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
