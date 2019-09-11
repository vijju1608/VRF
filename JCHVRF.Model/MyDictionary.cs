//********************************************************************
// 文件名: MyDictionary.cs
// 描述: 定义 VRF 项目中的字典类
// 作者: clh
// 创建时间: 2016-2-17
// 修改历史: 
//********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class MyDictionary: ModelBase
    {
        public enum DictionaryType
        {
            Brand, Factory, PowerSupply
        }

        DictionaryType _dicType;
        /// <summary>
        /// 字典类型
        /// </summary>
        public DictionaryType DicType
        {
            get { return _dicType; }
            set { this.SetValue(ref _dicType, value); }
        }

        string _code;
        /// <summary>
        /// 代码
        /// </summary>
        public string Code
        {
            get { return _code; }
            set { this.SetValue(ref _code, value); }
        }

        string _name;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

    }
}
