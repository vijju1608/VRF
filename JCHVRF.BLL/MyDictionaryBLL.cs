//********************************************************************
// 文件名: MyDictionaryBLL.cs
// 描述: 定义 VRF 项目中的字典BLL类
// 作者: clh
// 创建时间: 2016-2-17
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
    public class MyDictionaryBLL
    {
        MyDictionaryDAL _dal;

        public MyDictionaryBLL()
        {
            _dal = new MyDictionaryDAL();
        }

        public MyDictionary GetItem(MyDictionary.DictionaryType type, string code)
        {
            return _dal.GetItem(type, code);
        }

        public List<MyDictionary> GetList(MyDictionary.DictionaryType type)
        {
            return GetList(type);
        }
    }
}
