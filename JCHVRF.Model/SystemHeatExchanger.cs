using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    /// <summary>
    /// 控制器系统
    /// </summary>
    [Serializable]
    public class SystemHeatExchanger : SystemBase
    {
        public SystemHeatExchanger()
            : base()
        {
            _isValid = false;
            HvacSystemType = "2";
        }

        private bool _isValid;
        /// <summary>
        /// true if the system is valid, else returns false.
        /// </summary>
        public bool IsValid
        {
            get { return _isValid; }
            set { SetValue(ref _isValid, value); }
        }

        //ACC - RAG START      

        public string _unitName;
        public string UnitName
        {
            get { return _unitName; }
            set { SetValue(ref _unitName, value); }
        }

        public string _roomName;
        public string RoomName
        {
            get { return _roomName; }
            set { SetValue(ref _roomName, value); }
        }

        public string _floorName;
        public string FloorName
        {
            get { return _floorName; }
            set { SetValue(ref _floorName, value); }
        }

        public string _power;
        public string Power
        {
            get { return _power; }
            set { SetValue(ref _power, value); }
        }

        public string _series;
        public string Series
        {
            get { return _series; }
            set { SetValue(ref _series, value); }
        }

        public string _fanSpeed;
        public string FanSpeed
        {
            get { return _fanSpeed; }
            set { SetValue(ref _fanSpeed, value); }
        }

        public double _esp;
        public double ESP
        {
            get { return _esp; }
            set { SetValue(ref _esp, value); }
        }

        public double _area;
        public double Area
        {
            get { return _area; }
            set { SetValue(ref _area, value); }
        }

        public double _freshAir;
        public double FreshAir
        {
            get { return _freshAir; }
            set { SetValue(ref _freshAir, value); }
        }

        public int _people;
        public int NumberOfPeople
        {
            get { return _people; }
            set { SetValue(ref _people, value); }
        }

        public double _coolingDryBulb;
        public double CoolingDryBulb
        {
            get { return _coolingDryBulb; }
            set { SetValue(ref _coolingDryBulb, value); }
        }

        public double _coolingWetBulb;
        public double CoolingWetBulb
        {
            get { return _coolingWetBulb; }
            set { SetValue(ref _coolingWetBulb, value); }
        }

        public double _heatingDryBulb;
        public double HeatingDryBulb
        {
            get { return _heatingDryBulb; }
            set { SetValue(ref _heatingDryBulb, value); }
        }

        public double _relativeHumidity;
        public double RelativeHumidity
        {
            get { return _relativeHumidity; }
            set { SetValue(ref _relativeHumidity, value); }
        }

        //ACC - RAG END
    }
}
