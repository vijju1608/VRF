using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    [Serializable]
    public class Outdoor : ModelBase
    {
        #region 构造函数
        public Outdoor() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"> 完整型号 </param>
        public Outdoor(string model)
        {
            this._model = model;
        }
        #endregion

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


        private double _maxRatio;
        /// <summary>
        /// 产品类型
        /// </summary>
        public double MaxRatio
        {
            get { return _maxRatio; }
            set { this.SetValue(ref _maxRatio, value); }
        }


        private string _model;
        /// <summary>
        /// 型号 --ShortName
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { this.SetValue(ref _model, value); }
        }

        private string _modelFull;
        /// <summary>
        /// 型号名 Full Name
        /// </summary>
        public string ModelFull
        {
            get { return _modelFull; }
            set { this.SetValue(ref _modelFull, value); }
        }

        private string _model_York;
        /// <summary>
        /// 型号 --ShortName
        /// </summary>
        public string Model_York
        {
            get { return _model_York; }
            set { this.SetValue(ref _model_York, value); }
        }

        private string _model_Hitachi;
        /// <summary>
        /// 型号 --ShortName
        /// </summary>
        public string Model_Hitachi
        {
            get { return _model_Hitachi; }
            set { this.SetValue(ref _model_Hitachi, value); }
        }
        
        private string _auxModelName;
        /// <summary>
        /// 辅助型号名 —— 新增 20130910
        /// </summary>
        public string AuxModelName
        {
            get { return _auxModelName; }
            set { this.SetValue(ref _auxModelName, value); }
        }

        private double _coolingCapacity;    //kw
        /// <summary>
        /// 制冷量
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { this.SetValue(ref _coolingCapacity, value); }
        }

        private double _heatingCapacity;    //kw
        /// <summary>
        /// 制热量
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { this.SetValue(ref _heatingCapacity, value); }
        }

        private string _length;             //mm
        public string Length
        {
            get { return _length; }
            set { this.SetValue(ref _length, value); }
        }

        private string _width;              //mm
        public string Width
        {
            get { return _width; }
            set { this.SetValue(ref _width, value); }
        }

        private string _height;             //mm
        public string Height
        {
            get { return _height; }
            set { this.SetValue(ref _height, value); }
        }

        private string _gasPipe_Hi;            //mm
        /// <summary>
        /// 气管管长（高压或默认）
        /// </summary>
        public string GasPipe_Hi
        {
            get { return _gasPipe_Hi; }
            set { this.SetValue(ref _gasPipe_Hi, value); }
        }

        private string _gasPipe_Lo;            //mm
        /// <summary>
        /// 气管管长(低压)
        /// </summary>
        public string GasPipe_Lo
        {
            get { return _gasPipe_Lo; }
            set { this.SetValue(ref _gasPipe_Lo, value); }
        }

        private string _liquidPipe;         //mm
        /// <summary>
        /// 液管管长
        /// </summary>
        public string LiquidPipe
        {
            get { return _liquidPipe; }
            set { this.SetValue(ref _liquidPipe, value); }
        }

        private double _airFlow;            //m3/h
        /// <summary>
        /// 风量
        /// </summary>
        public double AirFlow
        {
            get { return _airFlow; }
            set { this.SetValue(ref _airFlow, value); }
        }

        private double _power_Cooling;         //kw
        /// <summary>
        /// 输入功率（PI）
        /// </summary>
        public double Power_Cooling
        {
            get { return _power_Cooling; }
            set { this.SetValue(ref _power_Cooling, value); }
        }

        private double _power_Heating;         //kw
        /// <summary>
        /// 输入功率（PI）
        /// </summary>
        public double Power_Heating
        {
            get { return _power_Heating; }
            set { this.SetValue(ref _power_Heating, value); }
        }

        private double _maxCurrent;
        /// <summary>
        /// 最大电流值，替换原来的RatedCurrent
        /// </summary>
        public double MaxCurrent
        {
            get { return _maxCurrent; }
            set { this.SetValue(ref _maxCurrent, value); }
        }

        private double _MCCB;               //A
        /// <summary>
        /// 塑料外壳式断路器(MCCB) 电流
        /// </summary>
        public double MCCB
        {
            get { return _MCCB; }
            set { this.SetValue(ref _MCCB, value); }
        }

        private double _weight;             //kg
        public double Weight
        {
            get { return _weight; }
            set { this.SetValue(ref _weight, value); }
        }

        private double _noiseLevel;         //dBA
        /// <summary>
        /// 噪音级别
        /// </summary>
        public double NoiseLevel
        {
            get { return _noiseLevel; }
            set { this.SetValue(ref _noiseLevel, value); }
        }

        private double _maxRefrigerantCharge;
        /// <summary>
        /// 最大冷媒充注量
        /// </summary>
        public double MaxRefrigerantCharge
        {
            get { return _maxRefrigerantCharge; }
            set { this.SetValue(ref _maxRefrigerantCharge, value); }
        }

        private double _refrigerantCharge;   //kg
        /// <summary>
        /// 冷媒充注量
        /// </summary>
        public double RefrigerantCharge
        {
            get { return _refrigerantCharge; }
            set { this.SetValue(ref _refrigerantCharge, value); }
        }

        private int _maxIU;
        /// <summary>
        /// 最大室内机连接数量
        /// </summary>
        public int MaxIU
        {
            get { return _maxIU; }
            set { this.SetValue(ref _maxIU, value); }
        }

        private int _recommendedIU;
        /// <summary>
        /// 推荐室内机连接数量
        /// </summary>
        public int RecommendedIU
        {
            get { return _recommendedIU; }
            set { this.SetValue(ref _recommendedIU, value); }
        }

        //private double _ESP;                //Pa
        ///// <summary>
        ///// 机外静压
        ///// </summary>
        //public double ESP
        //{
        //    get { return _ESP; }
        //    set { this.SetValue(ref _ESP, value); }
        //}

        private string _type;
        /// <summary>
        /// 室外机类型，YVOH(Horizontal.../Top...)
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { this.SetValue(ref _type, value); }
        }

        private double _price;
        /// <summary>
        /// 价格
        /// </summary>
        public double Price
        {
            get { return _price; }
            set { this.SetValue(ref _price, value); }
        }

        private string _typeImage;
        /// <summary>
        /// 类型图片文件名（不带路径）
        /// </summary>
        public string TypeImage
        {
            get { return _typeImage; }
            set { this.SetValue(ref _typeImage, value); }
        }

        //private string _powerSupply;
        ///// <summary>
        ///// 电源功率需求
        ///// </summary>
        //public string PowerSupply
        //{
        //    get { return _powerSupply; }
        //    set { this.SetValue(ref _powerSupply, value); }
        //}

        private string _fullModuleName;
        /// <summary>
        /// 室外机组合机组
        /// </summary>
        public string FullModuleName
        {
            get { return _fullModuleName; }
            set { this.SetValue(ref _fullModuleName, value); }
        }


        // add on 20140429 clh

        private double _MaxPipeLength;
        /// <summary>
        /// 系统允许的最大的配管实际长度，从标准室外机表直接获取
        /// </summary>
        public double MaxPipeLength
        {
            get { return _MaxPipeLength; }
            set { this.SetValue(ref _MaxPipeLength, value); }
        }

        private double _MaxEqPipeLength;
        /// <summary>
        /// 系统允许的最大的配管等效管长度，从标准室外机表直接获取
        /// </summary>
        public double MaxEqPipeLength
        {
            get { return _MaxEqPipeLength; }
            set { this.SetValue(ref _MaxEqPipeLength, value); }
        }


        private double _MaxOutdoorAboveHeight;
        /// <summary>
        /// 室外机在室内机上方时，系统允许的最大高度差，从标准室外机表直接获取
        /// </summary>
        public double MaxOutdoorAboveHeight
        {
            get { return _MaxOutdoorAboveHeight; }
            set { this.SetValue(ref _MaxOutdoorAboveHeight, value); }
        }

        private double _MaxOutdoorBelowHeight;
        /// <summary>
        /// 室外机在室内机下方时，系统允许的最大高度差，从标准室外机表直接获取
        /// </summary>
        public double MaxOutdoorBelowHeight
        {
            get { return _MaxOutdoorBelowHeight; }
            set { this.SetValue(ref _MaxOutdoorBelowHeight, value); }
        }

        private double _MaxDiffIndoorHeight;
        /// <summary>
        /// 系统允许的室内机最大高度差，从标准室外机表直接获取
        /// </summary>
        public double MaxDiffIndoorHeight
        {
            get { return _MaxDiffIndoorHeight; }
            set { this.SetValue(ref _MaxDiffIndoorHeight, value); }
        }

        private double _MaxIndoorLength;
        /// <summary>
        /// 系统允许的从第一分歧管到最远端室外机的最长距离，从标准室外机表直接获取
        /// </summary>
        public double MaxIndoorLength
        {
            get { return _MaxIndoorLength; }
            set { this.SetValue(ref _MaxIndoorLength, value); }
        }

        private double _maxIndoorLength_MaxIU;
        /// <summary>
        /// 系统允许的从第一分歧管到最远端室外机的最长距离，当连接的室内机数量大于推荐数量时的取值
        /// </summary>
        public double MaxIndoorLength_MaxIU
        {
            get { return _maxIndoorLength_MaxIU; }
            set { this.SetValue(ref _maxIndoorLength_MaxIU, value); }
        }

        private double _MaxPipeLengthwithFA;
        /// <summary>
        /// 一对一新风机组时，最大的等效果长度，从标准室外机表直接获取
        /// </summary>
        public double MaxPipeLengthwithFA
        {
            get { return _MaxPipeLengthwithFA; }
            set { this.SetValue(ref _MaxPipeLengthwithFA, value); }
        }

        private double _MaxDiffIndoorLength;
        /// <summary>
        /// 系统允许的从第一分歧管到最远端室内机与第一分歧管到最近室内机的距离差，从标准室外机表直接获取
        /// </summary>
        public double MaxDiffIndoorLength
        {
            get { return _MaxDiffIndoorLength; }
            set { this.SetValue(ref _MaxDiffIndoorLength, value); }
        }

        private double _maxDiffIndoorLength_MaxIU;
        /// <summary>
        /// 系统允许的从第一分歧管到最远端室内机与第一分歧管到最近室内机的距离差，当连接的室内机数量大于推荐数量时的取值
        /// </summary>
        public double MaxDiffIndoorLength_MaxIU
        {
            get { return _maxDiffIndoorLength_MaxIU; }
            set { this.SetValue(ref _maxDiffIndoorLength_MaxIU, value); }
        }

        // add on 20150128 clh 组合室外机内部分歧管数据
        private string _jointKitModelL;
        /// <summary>
        /// 分歧管型号，液管（若有多个，则用斜杠分隔）
        /// </summary>
        public string JointKitModelL
        {
            get { return _jointKitModelL; }
            set { this.SetValue(ref _jointKitModelL, value); }
        }

        private string _jointKitPriceL;
        /// <summary>
        /// 分歧管价格，液管（若有多个，则为价格之和）
        /// </summary>
        public string JointKitPriceL
        {
            get { return _jointKitPriceL; }
            set { this.SetValue(ref _jointKitPriceL, value); }
        }

        private string _jointKitModelG;
        /// <summary>
        /// 分歧管型号，气管（若有多个，则用斜杠分隔）
        /// </summary>
        public string JointKitModelG
        {
            get { return _jointKitModelG; }
            set { this.SetValue(ref _jointKitModelG, value); }
        }

        private string _jointKitPriceG;
        /// <summary>
        /// 分歧管价格，气管（若有多个，则为价格之和）
        /// </summary>
        public string JointKitPriceG
        {
            get { return _jointKitPriceG; }
            set { this.SetValue(ref _jointKitPriceG, value); }
        }
        
        private string _maxOperationPI_Cooling;
        /// <summary>
        /// 最大输入功率--制冷工况
        /// </summary>
        public string MaxOperationPI_Cooling
        {
            get { return _maxOperationPI_Cooling; }
            set { this.SetValue(ref _maxOperationPI_Cooling, value); }
        }

        private string _maxOperationPI_Heating;
        /// <summary>
        /// 最大输入功率--制热工况
        /// </summary>
        public string MaxOperationPI_Heating
        {
            get { return _maxOperationPI_Heating; }
            set { this.SetValue(ref _maxOperationPI_Heating, value); }
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

        #region 液管最大长度计算及L3验证所需属性 add on 20160515 by Yunxiao Lin
        private double _maxTotalPipeLength;
        /// <summary>
        /// 内机数量小于等于推荐值时的液管长度和上限
        /// </summary>
        public double MaxTotalPipeLength
        {
            get { return _maxTotalPipeLength; }
            set { this.SetValue(ref _maxTotalPipeLength, value); }
        }

        private double _maxTotalPipeLength_MaxIU;
        /// <summary>
        /// 内机数量大于推荐值，小于等于允许最大值时的液管长度和上限
        /// </summary>
        public double MaxTotalPipeLength_MaxIU
        {
            get { return _maxTotalPipeLength_MaxIU; }
            set { this.SetValue(ref _maxTotalPipeLength_MaxIU, value); }
        }

        private double _maxMKIndoorPipeLength;
        /// <summary>
        /// 内机数量小于等于推荐值时，每个Multi_kit到每个IDU的最大长度上限
        /// </summary>
        public double MaxMKIndoorPipeLength
        {
            get { return _maxMKIndoorPipeLength; }
            set { this.SetValue(ref _maxMKIndoorPipeLength, value); }
        }

        private double _maxMKIndoorPipeLength_MaxIU;
        /// <summary>
        /// 内机数量大于推荐值小于等于允许最大值时，每个Multi_kit到每个IDU的最大长度上限
        /// </summary>
        public double MaxMKIndoorPipeLength_MaxIU
        {
            get { return _maxMKIndoorPipeLength_MaxIU; }
            set { this.SetValue(ref _maxMKIndoorPipeLength_MaxIU, value); }
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

        #endregion

        #endregion

        private double _minRefrigerantCharge;
        /// <summary>
        /// 最小冷媒充注量
        /// </summary>
        public double MinRefrigerantCharge
        {
            get { return _minRefrigerantCharge; }
            set { this.SetValue(ref _minRefrigerantCharge, value); }
        }

        #region 新增能效比参数 add by axj 20180504

        private double _eer;
        /// <summary>
        /// EER
        /// </summary>
        public double EER
        {
            get { return _eer; }
            set { this.SetValue(ref _eer, value); }
        }

        private double _cop;
        /// <summary>
        /// COP
        /// </summary>
        public double COP
        {
            get { return _cop; }
            set { this.SetValue(ref _cop, value); }
        }

        private double _seer;
        /// <summary>
        /// SEER
        /// </summary>
        public double SEER
        {
            get { return _seer; }
            set { this.SetValue(ref _seer, value); }
        }
        private double _scop;
        /// <summary>
        /// SCOP
        /// </summary>
        public double SCOP
        {
            get { return _scop; }
            set { this.SetValue(ref _scop, value); }
        }
        private double _soudPower;
        /// <summary>
        /// SoundPower
        /// </summary>
        public double SoundPower
        {
            get { return _soudPower; }
            set { this.SetValue(ref _soudPower, value); }
        }

        private double _cspf;
        /// <summary>
        /// CSPF
        /// </summary>
        //public double CSPF
        //{
        //    get { return _cspf; }
        //    set { _cspf = value; }
        //}
        public double CSPF
        {
            get { return _cspf; }
            set { this.SetValue(ref _cspf, value); }
        }
        #endregion

    }
}
