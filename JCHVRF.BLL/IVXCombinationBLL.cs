//********************************************************************
// 文件名: IVXCombinationBLL.cs
// 描述: 定义 IVX 系统中ODU和IDU型号组合BLL类
// 作者: Yunxiao Lin
// 创建时间: 2017-7-1
// 修改历史: 
//********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCHVRF.Model;
using JCHVRF.DAL;

namespace JCHVRF.BLL
{
    public class IVXCombinationBLL
    {
        IVXCombinationDAL _dal;

        public IVXCombinationBLL()
        {
            _dal = new IVXCombinationDAL();
        }

        public IVXCombination getCombination(string ODUmodel)
        {
            return _dal.getCombination(ODUmodel);
        }

        public bool existsCombination(string ODUmodel, string IDUmodels)
        {
            return _dal.existsCombination(ODUmodel, IDUmodels);
        }
    }
}
