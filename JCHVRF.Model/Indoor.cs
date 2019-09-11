using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JCHVRF.Model
{
    [Serializable]
    public class Indoor : ModelBase
    {
        public Indoor() { }
        public Indoor(string model)
        {
            this._model = model;
        }

        /// <summary>
        ///  附件列表  （此属性已弃用，被挪到RoomIndoor里面）  by Shen Junjie on 2018/4/27
        /// </summary>
        public List<Accessory> ListAccessory = null;
         
        #region 字段

        private string _regionCode;
        /// <summary>
        /// 区域
        /// </summary>
        public string RegionCode
        {
            get { return _regionCode; }
            set { this.SetValue(ref _regionCode, value); }
        }

        private string _productType;
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType
        {
            get { return _productType; }
            set { this.SetValue(ref _productType, value); }
        }

        private string _model;
        /// <summary>
        /// 型号 -- Short Name,普通内机取7位，新风机取8位
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { this.SetValue(ref _model, value); }
        }

        private string _modelFull;
        /// <summary>
        /// 型号全名，对部分机组来说为虚拟的ModelName
        /// </summary>
        public string ModelFull
        {
            get { return _modelFull; }
            set { this.SetValue(ref _modelFull, value); }
        }

        private string _model_York;
        /// <summary>
        /// York Name,可能为空
        /// </summary>
        public string Model_York
        {
            get { return _model_York; }
            set { this.SetValue(ref _model_York, value); }
        }

        private string _model_Hitachi;
        /// <summary>
        /// Hitachi Name
        /// </summary>
        public string Model_Hitachi
        {
            get { return _model_Hitachi; }
            set { this.SetValue(ref _model_Hitachi, value); }
        }

        private string _type;
        /// <summary>
        /// 室内机类型 [UnitType]
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { this.SetValue(ref _type, value); }
        }

        private double _coolingCapacity;    //kW
        /// <summary>
        /// 制冷容量 kW
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { this.SetValue(ref _coolingCapacity, value); }
        }

        private double _heatingCapacity;    //kW
        /// <summary>
        /// 制热容量 kW
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { this.SetValue(ref _heatingCapacity, value); }
        }

        private string _length;         //mm
        /// <summary>
        /// 长度 mm
        /// </summary>
        public string Length
        {
            get { return _length; }
            set { this.SetValue(ref _length, value); }
        }

        private string _width;          //mm
        /// <summary>
        /// 宽度 mm
        /// </summary>
        public string Width
        {
            get { return _width; }
            set { this.SetValue(ref _width, value); }
        }

        private string _height;         //mm
        /// <summary>
        /// 高度 mm
        /// </summary>
        public string Height
        {
            get { return _height; }
            set { this.SetValue(ref _height, value); }
        }

        private string _gasPipe;        //mm
        /// <summary>
        /// 气管管长 mm
        /// </summary>
        public string GasPipe
        {
            get { return _gasPipe; }
            set { this.SetValue(ref _gasPipe, value); }
        }

        private string _liquidPipe;     //mm
        /// <summary>
        /// 液管管长 mm
        /// </summary>
        public string LiquidPipe
        {
            get { return _liquidPipe; }
            set { this.SetValue(ref _liquidPipe, value); }
        }

        private double _airFlow;
        /// <summary>
        /// 风量  （只用于新风机的新风量 20170704 by Shen Junjie）
        /// </summary>
        public double AirFlow
        {
            get { return _airFlow; }
            set { this.SetValue(ref _airFlow, value); }
        }

        //private double _airFlow_Hi;
        ///// <summary>
        ///// 风量
        ///// </summary>
        //public double AirFlow_Hi
        //{
        //    get { return _airFlow_Hi; }
        //    set { this.SetValue(ref _airFlow_Hi, value); }
        //}

        //private double _airFlow_Med;
        ///// <summary>
        ///// 风量
        ///// </summary>
        //public double AirFlow_Med
        //{
        //    get { return _airFlow_Med; }
        //    set { this.SetValue(ref _airFlow_Med, value); }
        //}

        //private double _airFlow_Lo;
        ///// <summary>
        ///// 风量
        ///// </summary>
        //public double AirFlow_Lo
        //{
        //    get { return _airFlow_Lo; }
        //    set { this.SetValue(ref _airFlow_Lo, value); }
        //}


        private double _sensibleHeat;   //kw
        /// <summary>
        /// 显热 kw
        /// </summary>
        public double SensibleHeat
        {
            get { return _sensibleHeat; }
            set { this.SetValue(ref _sensibleHeat, value); }
        }

        private double _powerInput_Cooling;     //kw
        /// <summary>
        /// 功率
        /// </summary>
        public double PowerInput_Cooling
        {
            get { return _powerInput_Cooling; }
            set { this.SetValue(ref _powerInput_Cooling, value); }
        }

        private double _powerInput_Heating;     //kw
        /// <summary>
        /// 功率,
        /// </summary>
        public double PowerInput_Heating
        {
            get { return _powerInput_Heating; }
            set { this.SetValue(ref _powerInput_Heating, value); }
        }

        private double _ratedCurrent;   //A
        /// <summary>
        /// 电流
        /// </summary>
        public double RatedCurrent
        {
            get { return _ratedCurrent; }
            set { this.SetValue(ref _ratedCurrent, value); }
        }

        private double _MCCB;           //A
        /// <summary>
        /// Moulded Case Circuit Breaker(塑壳断路器电流)
        /// </summary>
        public double MCCB
        {
            get { return _MCCB; }
            set { this.SetValue(ref _MCCB, value); }
        }

        private double _weight;         //kg
        /// <summary>
        /// 重量 kg
        /// </summary>
        public double Weight
        {
            get { return _weight; }
            set { this.SetValue(ref _weight, value); }
        }

        private double _noiseLevel;     //dBA
        /// <summary>
        /// 噪音级别
        /// </summary>
        public double NoiseLevel
        {
            get { return _noiseLevel; }
            set { this.SetValue(ref _noiseLevel, value); }
        }

        private double _noiseLevel_Hi;     //dBA
        /// <summary>
        /// 噪音级别
        /// </summary>
        public double NoiseLevel_Hi
        {
            get { return _noiseLevel_Hi; }
            set { this.SetValue(ref _noiseLevel_Hi, value); }
        }

        private double _noiseLevel_Med;     //dBA
        /// <summary>
        /// 噪音级别
        /// </summary>
        public double NoiseLevel_Med
        {
            get { return _noiseLevel_Med; }
            set { this.SetValue(ref _noiseLevel_Med, value); }
        }

        private double _noiseLevel_Lo;     //dBA
        /// <summary>
        /// 噪音级别
        /// </summary>
        public double NoiseLevel_Lo
        {
            get { return _noiseLevel_Lo; }
            set { this.SetValue(ref _noiseLevel_Lo, value); }
        }

        private double _drainagePipe;   //mm
        /// <summary>
        /// 排水管长
        /// </summary>
        public double DrainagePipe
        {
            get { return _drainagePipe; }
            set { this.SetValue(ref _drainagePipe, value); }
        }

        //private string _ESP;            //Pa
        ///// <summary>
        ///// 静压
        ///// </summary>
        //public string ESP
        //{
        //    get { return _ESP; }
        //    set { this.SetValue(ref _ESP, value); }
        //}
        
        private string _typeImage;
        /// <summary>
        /// 类型图片文件名（不带路径）
        /// </summary>
        public string TypeImage
        {
            get { return _typeImage; }
            set { this.SetValue(ref _typeImage, value); }
        }

        private string _uniqueOutdoorName;
        /// <summary>
        /// 对应的一对一的 Outdoor 的 ModelName
        /// （add on 20120425 clh for Phase2 某些新风机与室外机一对一的情况）
        /// </summary>
        public string UniqueOutdoorName
        {
            get { return _uniqueOutdoorName; }
            set { this.SetValue(ref _uniqueOutdoorName, value); }
        }

        private string _partLoadTableName;
        /// <summary>
        /// 对应的容量表的表名
        /// </summary>
        public string PartLoadTableName
        {
            get { return _partLoadTableName; }
            set { this.SetValue(ref _partLoadTableName, value); }
        }

        private IndoorType _flag;
        /// <summary>
        /// 室内机类型, 区分 Indoor | Fresh Air
        /// </summary>
        public IndoorType Flag
        {
            get { return _flag; }
            set { this.SetValue(ref _flag, value); }
        }

        private string _series; 
        /// <summary>
        /// 系列              add on 20161027
        /// </summary>
        public string Series
        {
            get { return _series; }
            set { this.SetValue(ref _series, value); }
        }

        private double _horsepower;
        /// 匹数 add on 20161109 by Yunxiao Lin
        /// <summary>
        /// 匹数
        /// </summary>
        public double Horsepower
        {
            get { return _horsepower; }
            set { this.SetValue(ref _horsepower, value); }
        }

        #region Fan Speed 相关 add on 20170703 by Shen Junjie
        /// <summary>
        /// 各风速档位的显热值
        /// </summary>
        public double[] SHF_Levels { get; set; }
        /// <summary>
        /// 各风速档位的静压值
        /// </summary>
        public double[] ESP_Levels { get; set; }
        /// <summary>
        /// 各风速档位的气流值
        /// </summary>
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
        /// <summary>
        /// 根据风扇速度获取显热值
        /// </summary>
        /// <param name="fanSpeedLevel"></param>
        /// <returns></returns>
        public double GetSHF(int fanSpeedLevel)
        {
            return GetValueByFanSpeed(this.SHF_Levels, fanSpeedLevel);
        }
        /// <summary>
        /// 获取静压值 （20170927最新逻辑：跟风扇速度没有关系）
        /// </summary>
        /// <returns></returns>
        public double GetStaticPressure()
        {
            string type = this.Type;
            //Hydro Free和DX-Interface没有Static Pressure属性，返回0。 20180227 by Shen Junjie
            if (type.Contains("Hydro Free") || type == "DX-Interface")
            {
                return 0;
            }
            double[] values = this.ESP_Levels;
            if (values == null || values.Length == 0) return 0;
            if (values[0] > 0) return values[0]; //High2
            return values[1];//High
        }
        ///// <summary>
        ///// 根据风扇速度获取静压值
        ///// </summary>
        ///// <param name="fanSpeedLevel"></param>
        ///// <returns></returns>
        //public double GetStaticPressure(int fanSpeedLevel)
        //{
        //    return GetValueByFanSpeed(this.ESP_Levels, fanSpeedLevel);
        //}
        /// <summary>
        /// 根据风扇速度获取气流值
        /// </summary>
        /// <param name="fanSpeedLevel"></param>
        /// <returns></returns>
        public double GetAirFlow(int fanSpeedLevel)
        {
            string type = this.Type;

            //新风机只有新风量，取用标准表的Air Flow字段
            if (type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation"))
            {
                return this.AirFlow;
            }
            //Hydro Free和DX-Interface没有风量属性，返回0。 20171204 by Yunxiao Lin
            if (type.Contains("Hydro Free") || type == "DX-Interface")
            {
                return 0;
            }

            return GetValueByFanSpeed(this.AirFlow_Levels, fanSpeedLevel);
        }
        #endregion

        ///// 显热系数Hi add on 20161109 by Yunxiao lin
        ///// <summary>
        ///// 显热系数Hi
        ///// </summary>
        //public double SHF_Hi
        //{
        //    get { return _shf_hi; }
        //    set { this.SetValue(ref _shf_hi, value); }
        //}
        ///// 显热系数Mi add on 20161109 by Yunxiao lin
        ///// <summary>
        ///// 显热系数Mi
        ///// </summary>
        //public double SHF_Med
        //{
        //    get { return _shf_med; }
        //    set { this.SetValue(ref _shf_med, value); }
        //}
        ///// 显热系数Lo add on 20161109 by Yunxiao lin
        ///// <summary>
        ///// 显热系数Lo
        ///// </summary>
        //public double SHF_Lo
        //{
        //    get { return _shf_lo; }
        //    set { this.SetValue(ref _shf_lo, value); }
        //}

        //private string _indoorName;
        ///// 室内机名   --add on 20170605 by Lingjia Qiu
        ///// <summary>
        ///// 室内机名
        ///// </summary>
        //public string IndoorName
        //{
        //    get { return _indoorName; }
        //    set { this.SetValue(ref _indoorName, value); }
        //}

        private string _displayName;
        /// 通用显示名   --add on 20171208 by Lingjia Qiu
        /// <summary>
        /// 通用显示名
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { this.SetValue(ref _displayName, value); }
        }

        /// 获取室内机工厂代码 20180627 add by Yunxiao Lin
        /// <summary>
        /// 获取室内机工厂代码
        /// </summary>
        /// <returns></returns>
        public string GetFactoryCode()
        {
            string factoryCode = _modelFull.Substring(_modelFull.Length - 1, 1); ;
            return factoryCode;
        }
        /// 获取室内机工厂代码，用于配件选型。配件数据中Wuxi designed工厂代码为W，实际的IDU工厂代码为Q，因此需要将Q转换为W。 20180627 add by Yunxiao Lin
        /// <summary>
        /// 获取室内机工厂代码，用于配件选型
        /// </summary>
        /// <returns></returns>
        public string GetFactoryCodeForAccess()
        {
            Regex reg = new Regex(@"(RPIZ|RCIR)-(.*?)HNGTAQ");
            var mat = reg.Match(_model_Hitachi);
            if (mat.Success)
                return "W";
            reg = new Regex(@"(RPIH|RPIM)-(.*?)(HNGUAQ|HNGUEQ)");
            mat = reg.Match(_model_Hitachi);
            if (mat.Success)
                return "W";
            return GetFactoryCode();
        }
        #endregion
    }
}
