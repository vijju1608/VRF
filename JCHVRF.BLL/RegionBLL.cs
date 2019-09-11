//********************************************************************
// 文件名: RegionBLL.cs
// 描述: 定义 VRF 项目中的区域BLL类
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
using JCHVRF.DAL;

namespace JCHVRF.BLL
{
    public class RegionBLL
    {
        RegionDAL _dal;

        public RegionBLL()
        {
            _dal = new RegionDAL();
        }

        public MyRegion GetItem(string code)
        {
            return _dal.GetItem(code);
        }

        public DataTable GetParentRegionTable()
        {
            return _dal.GetParentRegionTable();
        }

        public List<MyRegion> GetParentRegionList()
        {
            return _dal.GetParentRegionList();
        }

        public DataTable GetSubRegionList(string pCode)
        {
            return _dal.GetSubRegionTable(pCode);
        }

        public DataTable GetActiveLanguage()
        {
            return _dal.LoadActiveLanguage();
        }
    }
}
