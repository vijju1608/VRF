using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
  

        public class OutDoorUnitInfo
        {

            private int _id;

            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }


            private string _producttype;

            public string ProductType
            {
                get { return _producttype; }
                set { _producttype = value; }
            }

        private string _series;

        public string Series
        {
            get { return _series; }
            set { _series = value; }
        }


        private string _maxratio;

        public string MaxRatio
        {
            get { return _maxratio; }
            set { _maxratio = value; }
        }

    }
   
}
