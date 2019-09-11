using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JCHVRF_New
{
    public class IDUEquipmentModel : INotifyPropertyChanged
    {
        public IDUEquipmentModel()
        {

        }
        public string Model { get; set; }
        public string ModelFull { get; set; }
        public string ModelYork { get; set; }
        public string ModelHitachi { get; set; }
        public double CoolCapacity { get; set; }
        public double HeatCapacity { get; set; }
        public double SensibleHeat { get; set; }
        public double AirFlow { get; set; }
        public string FanSpeed { get; set; }

        private string _regionCode;
        /// <summary>
        /// 区域
        /// </summary>
        public string RegionCode
        {
            get { return _regionCode; }
            set { _regionCode = value; }
        }

        private string _productType;
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }
        private string _type;
        /// <summary>
        /// 室内机类型 [UnitType]
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _length;         //mm
        /// <summary>
        /// 长度 mm
        /// </summary>
        public string Length
        {
            get { return _length; }
            set { _length = value; }
        }
        private string _width;          //mm
        /// <summary>
        /// 宽度 mm
        /// </summary>
        public string Width
        {
            get { return _width; }
            set { _width = value; }
        }
        private string _height;         //mm
        /// <summary>
        /// 高度 mm
        /// </summary>
        public string Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private string _gasPipe;        //mm
        /// <summary>
        /// 气管管长 mm
        /// </summary>
        public string GasPipe
        {
            get { return _gasPipe; }
            set { _gasPipe = value; }
        }

        private string _liquidPipe;     //mm
        /// <summary>
        /// 液管管长 mm
        /// </summary>
        public string LiquidPipe
        {
            get { return _liquidPipe; }
            set { _liquidPipe = value; }
        }
        private double _powerInput_Cooling;     //kw
        /// <summary>
        /// 功率
        /// </summary>
        public double PowerInput_Cooling
        {
            get { return _powerInput_Cooling; }
            set { _powerInput_Cooling = value; }
        }
        private double _powerInput_Heating;     //kw
        /// <summary>
        /// 功率,
        /// </summary>
        public double PowerInput_Heating
        {
            get { return _powerInput_Heating; }
            set { _powerInput_Heating = value; }
        }

        private double _ratedCurrent;   //A
        /// <summary>
        /// 电流
        /// </summary>
        public double RatedCurrent
        {
            get { return _ratedCurrent; }
            set { _ratedCurrent = value; }
        }
        private double _MCCB;           //A
       
        public double MCCB
        {
            get { return _MCCB; }
            set { _MCCB = value; }
        }

        private double _weight;         //kg
       
        public double Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        private double _noiseLevel;     //dBA
       
        public double NoiseLevel
        {
            get { return _noiseLevel; }
            set { _noiseLevel = value; }
        }

        private double _noiseLevel_Hi;     //dBA
       
        public double NoiseLevel_Hi
        {
            get { return _noiseLevel_Hi; }
            set { _noiseLevel_Hi = value; }
        }

        private double _noiseLevel_Med;     //dBA
       
        public double NoiseLevel_Med
        {
            get { return _noiseLevel_Med; }
            set { _noiseLevel_Med = value; }
        }

        private double _ESP;     //dBA

        public double ESP
        {
            get { return _ESP; }
            set { _ESP = value; }
        }

        private double _noiseLevel_Lo;     //dBA
       
        public double NoiseLevel_Lo
        {
            get { return _noiseLevel_Lo; }
            set { _noiseLevel_Lo = value; }
        }

        private double _drainagePipe;   //mm
       
        public double DrainagePipe
        {
            get { return _drainagePipe; }
            set { _drainagePipe = value; }
        }

        private string _typeImage;
       
        public string TypeImage
        {
            get { return _typeImage; }
            set { _typeImage = value; }
        }

        private string _uniqueOutdoorName;
       
        public string UniqueOutdoorName
        {
            get { return _uniqueOutdoorName; }
            set { _uniqueOutdoorName = value; }
        }

        private string _partLoadTableName;
       
        public string PartLoadTableName
        {
            get { return _partLoadTableName; }
            set { _partLoadTableName = value; }
        }

        private IndoorType _flag;
      
        public IndoorType Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        private string _series;
       
        public string Series
        {
            get { return _series; }
            set { _series = value; }
        }

        private double _horsepower;
       
        public double Horsepower
        {
            get { return _horsepower; }
            set { _horsepower = value; }
        }

        #region Fan Speed 相关 add on 20170703 by Shen Junjie
      
        public double[] SHF_Levels { get; set; }
      
        public double[] ESP_Levels { get; set; }
        
        public double[] AirFlow_Levels { get; set; }

        private double GetValueByFanSpeed(double[] values, int fanSpeedLevel)
        {
            if (values == null || values.Length == 0) return 0;
            if (fanSpeedLevel < 0 || fanSpeedLevel >= values.Length || values[fanSpeedLevel] == 0)
            {
                if (values[0] > 0) return values[0]; //High2
                return values[1];//High
            }
            return values[fanSpeedLevel];
        }
      
        public double GetSHF(int fanSpeedLevel)
        {
            return GetValueByFanSpeed(this.SHF_Levels, fanSpeedLevel);
        }
       
        public double GetStaticPressure()
        {
            string type = this.Type;
            
            if (type.Contains("Hydro Free") || type == "DX-Interface")
            {
                return 0;
            }
            double[] values = this.ESP_Levels;
            if (values == null || values.Length == 0) return 0;
            if (values[0] > 0) return values[0]; //High2
            return values[1];//High
        }
       
        //public double GetStaticPressure(int fanSpeedLevel)
        //{
        //    return GetValueByFanSpeed(this.ESP_Levels, fanSpeedLevel);
        //}
       
        public double GetAirFlow(int fanSpeedLevel)
        {
            string type = this.Type;

            
            if (type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation"))
            {
                return this.AirFlow;
            }
           
            if (type.Contains("Hydro Free") || type == "DX-Interface")
            {
                return 0;
            }

            return GetValueByFanSpeed(this.AirFlow_Levels, fanSpeedLevel);
        }
        #endregion
        private string _displayName;
      
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        public string GetFactoryCode()
        {
            string factoryCode = ModelFull.Substring(ModelFull.Length - 1, 1); ;
            return factoryCode;
        }
       
        public string GetFactoryCodeForAccess()
        {
            Regex reg = new Regex(@"(RPIZ|RCIR)-(.*?)HNGTAQ");
            var mat = reg.Match(ModelHitachi);
            if (mat.Success)
                return "W";
            reg = new Regex(@"(RPIH|RPIM)-(.*?)(HNGUAQ|HNGUEQ)");
            mat = reg.Match(ModelHitachi);
            if (mat.Success)
                return "W";
            return GetFactoryCode();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

    }
}
