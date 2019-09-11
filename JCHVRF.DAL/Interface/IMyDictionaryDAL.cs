using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    interface IMyDictionaryDAL
    {
        MyDictionary GetItem(MyDictionary.DictionaryType type, string code);
        List<MyDictionary> GetList(MyDictionary.DictionaryType type);

    }
}
