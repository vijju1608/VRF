using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Model
{
   public class MaterialList:ModelBase
    {
        #region Fields
        public string _Model;
        public int _Qty;
        public string _Description;
        #endregion


        #region Properties
        /// <summary>
        /// Gets or sets the Model
        /// </summary>
        public string Model
        {
            get { return _Model; }
            set { this.SetValue(ref _Model, value); }
        }

        /// <summary>
        /// Gets or sets the Qty
        /// </summary>
        public int Qty
        {
            get { return _Qty; }
            set { this.SetValue(ref _Qty, value); }
        }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set { this.SetValue(ref _Description, value); }
        }


        #endregion
    }
}
