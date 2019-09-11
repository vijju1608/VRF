//********************************************************************
// 文件名: MyDictionaryDAL.cs
// 描述: 定义 VRF 项目中的字典DAL类
// 作者: clh
// 创建时间: 2016-2-17
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
    public class MyDictionaryDAL : IMyDictionaryDAL
    {
        IDataAccessObject _dao;

        public MyDictionaryDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        public MyDictionary GetItem(MyDictionary.DictionaryType type, string code)
        {
            //string sql = "select * from JCHVRF_Dictionary where Type='" + type.ToString()
            //    + "' and Code = '" + code + "'";
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Dictionary");        
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "Type='" + type.ToString()+ "' and Code = '" + code + "'";
                dt = dv.ToTable();
                DataRow dr = dt.Rows[0];
                MyDictionary dic = new MyDictionary();
                dic.Code = code;
                dic.DicType = type;
                dic.Name = (dr["Name"] is DBNull) ? "" : dr["Name"].ToString().Trim();
                return dic;
            }
            return null;
        }

        public List<MyDictionary> GetList(MyDictionary.DictionaryType type)
        {
            //string sql = "select * from JCHVRF_Dictionary where trim(Type)='" + type.ToString() + "'";
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Dictionary");        
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter ="trim(Type)='" + type.ToString() + "'";
                dt = dv.ToTable();
                List<MyDictionary> list = new List<MyDictionary>();
                foreach (DataRow dr in dt.Rows)
                {
                    MyDictionary dic = new MyDictionary();
                    dic.DicType=type;
                    dic.Code = (dr["Code"] is DBNull) ? "" : dr["Code"].ToString().Trim();
                    dic.Name = (dr["Name"] is DBNull) ? "" : dr["Name"].ToString().Trim();
                    list.Add(dic);
                }
                return list;
            }
            return null;
        }
    }
}
