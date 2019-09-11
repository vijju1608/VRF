using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Converters;
namespace JCHVRF_New.Model
{
    class HeatExEquipmentInfoModel
    {
        //Air Flow Rate
        private string _AirFlowRate_low;

        public string AirFlowProp_low
        {
            get { return _AirFlowRate_low; }
            set { _AirFlowRate_low = value; }
        }

        private string _AirFlowRate_medium;

        public string AirFlowProp_medium
        {
            get { return _AirFlowRate_medium; }
            set { _AirFlowRate_medium = value; }
        }

        private string _AirFlowRate_High;

        public string AirFlowProp_High
        {
            get { return _AirFlowRate_High; }
            set { _AirFlowRate_High = value; }
        }

        //External Static Pressure
        private string _externalStaticPressure_Low;

        public string ExternalStaticPressure_low
        {
            get { return _externalStaticPressure_Low; }
            set { _externalStaticPressure_Low = value; }
        }

        private string _externalStaticPressure_medium;

        public string ExternalStaticPressure_medium
        {
            get { return _externalStaticPressure_medium; }
            set { _externalStaticPressure_medium = value; }
        }

        private string _externalStaticPressure_high;

        public string ExternalStaticPressure_high
        {
            get { return _externalStaticPressure_high; }
            set { _externalStaticPressure_high = value; }
        }

        //temp exchange efficiency
        private string _tempExchangeEff_Low;

        public string TempExchangeEff_Low
        {
            get { return _tempExchangeEff_Low; }
            set { _tempExchangeEff_Low = value; }
        }

        private string _tempExchangeEff_medium;

        public string TempExchangeEff_medium
        {
            get { return _tempExchangeEff_medium; }
            set { _tempExchangeEff_medium = value; }
        }

        private string _tempExchangeEff_high;

        public string TempExchangeEff_high
        {
            get { return _tempExchangeEff_high; }
            set { _tempExchangeEff_high = value; }
        }

        //Enthalpy Exchange Efficiency - for heating
        private string _enthalpyExEffHeat_low;

        public string EnthalpyExchangeEffHeat_low
        {
            get { return _enthalpyExEffHeat_low; }
            set { _enthalpyExEffHeat_low = value; }
        }

        private string _enthalpyExEffHeat_medium;

        public string EnthalpyExchangeEffHeat_medium
        {
            get { return _enthalpyExEffHeat_medium; }
            set { _enthalpyExEffHeat_medium = value; }
        }

        private string _enthalpyExEffHeat_high;

        public string EnthalpyExchangeEffHeat_high
        {
            get { return _enthalpyExEffHeat_high; }
            set { _enthalpyExEffHeat_high = value; }
        }

        //Enthalpy Exchange Efficiency - for cooling
        private string _enthalpyExEffCool_low;

        public string EnthalpyExEffCool_low
        {
            get { return _enthalpyExEffCool_low; }
            set { _enthalpyExEffCool_low = value; }
        }

        private string _enthalpyExEffCool_medium;

        public string EnthalpyExEffCool_medium
        {
            get { return _enthalpyExEffCool_medium; }
            set { _enthalpyExEffCool_medium = value; }
        }

        private string _enthalpyExEffCool_high;

        public string EnthalpyExEffCool_high
        {
            get { return _enthalpyExEffCool_high; }
            set { _enthalpyExEffCool_high = value; }
        }

        //Outer Dimensions
        private string _outerDimensions_height;

        public string OuterDimensions_height
        {
            get { return _outerDimensions_height; }
            set { _outerDimensions_height = value; }
        }

        private string _outerDimensions_depth;

        public string OuterDimensions_depth
        {
            get { return _outerDimensions_depth; }
            set { _outerDimensions_depth = value; }
        }

        private string _outerDimensions_width;

        public string OuterDimensions_width
        {
            get { return _outerDimensions_width; }
            set { _outerDimensions_width = value; }
        }

        //Net Weight
        private string _netWeight;

        public string NetWeight
        {
            get { return _netWeight; }
            set { _netWeight = value; }
        }

        //Connection Duct Diameter

        private string _connectionDuctDiameter;

        public string ConnectionDuctDiameter
        {
            get { return _connectionDuctDiameter; }
            set { _connectionDuctDiameter = value; }
        }

    
    }
}

