using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCHVRF.Model;

namespace JCHVRF.DAL
{
    interface IIVXCombinationDAL
    {
        IVXCombination getCombination(string ODUmodel);
        bool existsCombination(string ODUmodel, string IDUmodels);
    }
}
