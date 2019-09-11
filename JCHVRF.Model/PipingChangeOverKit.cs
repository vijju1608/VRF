using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class PipingChangeOverKit : ModelBase
    {
        private string _factoryCode;
        /// <summary>
        /// 工厂代码
        /// </summary>
        public string FactoryCode
        {
            get { return _factoryCode; }
            set { this.SetValue(ref _factoryCode, value); }
        }

        private string _model_York;
        /// <summary>
        /// Connection Kit型号--York型号
        /// </summary>
        public string Model_York
        {
            get { return _model_York; }
            set { this.SetValue(ref _model_York, value); }
        }

        private string _model_Hitachi;
        /// <summary>
        /// Connection Kit型号--Hitachi型号
        /// </summary>
        public string Model_Hitachi
        {
            get { return _model_Hitachi; }
            set { this.SetValue(ref _model_Hitachi, value); }
        }

        private int _maxIU;
        /// <summary>
        /// 最大分支数量
        /// </summary>
        public int MaxIU
        {
            get { return _maxIU; }
            set { this.SetValue(ref _maxIU, value); }
        }

        private double _capacity;
        /// <summary>
        /// 制冷容量
        /// </summary>
        public double Capacity
        {
            get { return _capacity; }
            set { this.SetValue(ref _capacity, value); }
        }


        private string _gasPipe;
        /// <summary>
        ///下游气管管径（原来该字段为液管管径）
        /// </summary>
        public string GasPipe
        {
            get { return _gasPipe; }
            set { this.SetValue(ref _gasPipe, value); }
        }

        private string _highPressureGasPipe;
        /// <summary>
        /// 高压气管
        /// </summary>
        public string HighPressureGasPipe
        {
            get { return _highPressureGasPipe; }
            set { this.SetValue(ref _highPressureGasPipe, value); }
        }

        private string _lowPressureGasPipe;
        /// <summary>
        /// 低压气管
        /// </summary>
        public string LowPressureGasPipe
        {
            get { return _lowPressureGasPipe; }
            set { this.SetValue(ref _lowPressureGasPipe, value); }
        }

        private string _sizeUP;
        /// <summary>
        /// TRUE| FALSE| NA
        /// </summary>
        public string SizeUP
        {
            get { return _sizeUP; }
            set { this.SetValue(ref _sizeUP, value); }
        }

        private string _partNumber;
        /// <summary>
        /// 零件号
        /// </summary>
        public string PartNumber
        {
            get { return _partNumber; }
            set { this.SetValue(ref _partNumber, value); }
        }
        private double _maxTotalCHIndoorPipeLength;
        /// <summary>
        /// CH-Box到每个Indoor的距离之和，当前系统Indoor数量小于推荐值
        /// </summary>
        public double MaxTotalCHIndoorPipeLength
        {
            get { return _maxTotalCHIndoorPipeLength; }
            set { this.SetValue(ref _maxTotalCHIndoorPipeLength, value); }
        }

        private double _maxTotalCHIndoorPipeLength_MaxIU;
        /// <summary>
        /// CH-Box到每个Indoor的距离之和，当前系统Indoor数量超过推荐值
        /// </summary>
        public double MaxTotalCHIndoorPipeLength_MaxIU
        {
            get { return _maxTotalCHIndoorPipeLength_MaxIU; }
            set { this.SetValue(ref _maxTotalCHIndoorPipeLength_MaxIU, value); }
        }

        /// 电源 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源
        /// </summary>
        public string PowerSupply { get; set; }

        /// 电源线形 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源线形
        /// </summary>
        public string PowerLineType { get; set; }

        //add by Shen Junjie on 2018/6/15
        /// <summary>
        /// 电源功耗
        /// </summary>
        public double PowerConsumption { get; set; }

        //add by Shen Junjie on 2017/6/15
        /// <summary>
        /// 电流
        /// </summary>
        public double PowerCurrent { get; set; }
    }
}
