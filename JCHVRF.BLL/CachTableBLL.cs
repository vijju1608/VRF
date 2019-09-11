using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF.DAL;

namespace JCHVRF.BLL
{
    public class CachTableBLL
    {
         CachTableDAL _dal;
          public CachTableBLL()
        {
            _dal = new CachTableDAL();
        }

          public void CreateCachTable()
          {
              _dal.CreateCachData();
          }
    }
}
