//********************************************************************
// 文件名: IVXCombination.cs
// 描述: 定义 IVX 系统中ODU 和 IDU 的型号组合。
// 作者: Yunxiao Lin
// 创建时间: 2017-7-1
// 修改历史: 
//********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class IVXCombination : ModelBase
    {
        string _ODUModel;
        public string ODUModel
        {
            get { return _ODUModel; }
            set { this.SetValue(ref _ODUModel, value); }
        }

        List<string> _IDUModels;
        public List<string> IDUModels
        {
            get { return _IDUModels; }
            set { this.SetValue(ref _IDUModels, value); }
        }
    }
}
