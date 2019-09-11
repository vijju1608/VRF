using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Model
{
    public class IDUODUCapacityDetails:ModelBase
    {
        #region Fields
        private double _ratedCapacity;
        private string _ratedCapacityNew;
        private double _correctedCapacity;
        private string _correctedCapacityNew;
        private string _capacity;
        private bool _IsValidMode = true;
        #endregion


        #region Properties
        /// <summary>
        /// Gets or sets the RatedCapacity
        /// </summary>
        public double RatedCapacity
        {
            get { return _ratedCapacity; }
            set { this.SetValue(ref _ratedCapacity, value); }
        }
        public string RatedCapacityNew
        {
            get { return _ratedCapacityNew; }
            set { this.SetValue(ref _ratedCapacityNew, value); }
        }

        public string CorrectedCapacityNew
        {
            get { return _correctedCapacityNew; }
            set { this.SetValue(ref _correctedCapacityNew, value); }
        }
        /// <summary>
        /// Gets or sets the CorrectedCapacity
        /// </summary>
        public double CorrectedCapacity
        {
            get { return _correctedCapacity; }
            set { this.SetValue(ref _correctedCapacity, value); }
        }
     
        /// <summary>
        /// Gets or sets the CorrectedCapacity
        /// </summary>
        public string Capacity
        {
            get { return _capacity; }
            set { this.SetValue(ref _capacity, value); }
        }

        public bool IsValidMode
        {
            get
            {
                return _IsValidMode;
            }
            set
            {
                this.SetValue(ref _IsValidMode, value);
            }
        }
        #endregion

    }

}
