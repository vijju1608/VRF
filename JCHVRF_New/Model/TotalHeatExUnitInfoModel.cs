using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Model
{
    class TotalHeatExUnitInfoModel : ModelBase
    {
        #region Fields
        private string _unitName;
        private string _name;
        private string _floor;
        private string _fanSpeed;
        private string _series;
        private string _power;
        private int _freshAir;
        private int _eSP;
        private int _area;
        private int _noOfPeople;
        private int _coolDryBulb;
        private int _heatDryBulb;
        private int _relatHumidity;
        #endregion

        #region Properties
        public string UnitName
        {
            get { return _unitName; }
            set { this.SetValue(ref _unitName, value); }
        }

        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        public string Floor
        {
            get { return _floor; }
            set { this.SetValue(ref _floor, value); }
        }
        public string FanSpeeds
        {
            get { return _fanSpeed; }
            set { this.SetValue(ref _fanSpeed, value); }
        }
        public string Series
        {
            get { return _series; }
            set { this.SetValue(ref _series, value); }
        }
        public string Power
        {
            get { return _power; }
            set { this.SetValue(ref _power, value); }
        }
        public int FreshAir
        {
            get { return _freshAir; }
            set { this.SetValue(ref _freshAir, value); }
        }
        public int ESP
        {
            get { return _eSP; }
            set { this.SetValue(ref _eSP, value); }
        }
        public int Area
        {
            get { return _area; }
            set { this.SetValue(ref _area, value); }
        }
        public int NoOfPeople
        {
            get { return _noOfPeople; }
            set { this.SetValue(ref _noOfPeople, value); }
        }
        public int CoolDryBulb
        {
            get { return _coolDryBulb; }
            set { this.SetValue(ref _coolDryBulb, value); }
        }
        public int HeatDryBulb
        {
            get { return _heatDryBulb; }
            set { this.SetValue(ref _heatDryBulb, value); }
        }
        public int RelatHumidity
        {
            get { return _relatHumidity; }
            set { this.SetValue(ref _relatHumidity, value); }
        }
        #endregion
    }

}
