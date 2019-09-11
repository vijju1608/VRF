using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class PipingBranchKit : ModelBase
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

        private string _type;
        /// <summary>
        /// 2pipes|3pipes
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { this.SetValue(ref _type, value); }
        }

        private string _unitType;
        /// <summary>
        /// ODU机组类别
        /// </summary>
        public string UnitType
        {
            get { return _unitType; }
            set { this.SetValue(ref _unitType, value); }
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

        private int _pipingSets;
        /// <summary>
        /// 内部分歧管数量
        /// </summary>
        public int PipingSets
        {
            get { return _pipingSets; }
            set { this.SetValue(ref _pipingSets, value); }
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

        private string _liquidPipe;
        /// <summary>
        /// 液管管径
        /// </summary>
        public string LiquidPipe
        {
            get { return _liquidPipe; }
            set { this.SetValue(ref _liquidPipe, value); }
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

        
    }

    public enum PipingSizeUPType
    {
        TRUE, FALSE, NA
    }
}
