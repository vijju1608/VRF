using JCBase.Utility;
using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace JCHVRF_New.Common.UI.Converters
{
    class SourceToControlWithUnitConverter:MarkupExtension, IValueConverter
    {
        public string CurrentUnit { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double sourceValue = 0.0;
            if (value != null && parameter != null)
            {

                string valueAsString = System.Convert.ToString(value);
                string unitType = System.Convert.ToString(parameter);
                string returnVal = string.Empty;

                if (double.TryParse(valueAsString, out sourceValue))
                    switch (unitType.ToUpper())
                    {
                        case "POWER":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.POWER, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "AIRFLOW":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.AIRFLOW, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "LENGTH_M":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.LENGTH_M, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "LENGTH_MM":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingDimensionUnit;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.LENGTH_MM, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "TEMPERATURE":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.TEMPERATURE, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "WEIGHT":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.WEIGHT, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "AREA":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingAREA;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.AREA, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "LOADINDEX":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.LOADINDEX, CurrentUnit))) + " " + CurrentUnit;
                            break;
                        case "WATERFLOWRATE":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate;
                            returnVal = string.Format("{0:0.00}", (Unit.ConvertToControl(sourceValue, UnitType.WATERFLOWRATE, CurrentUnit))) + " " + CurrentUnit;
                            break;

                        default:
                            break;
                    }
                return returnVal;
            }


            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double sourceValue = 0.0;
            if (value != null && parameter != null)
            {
                string valueAsString = System.Convert.ToString(value);
                string unitType = System.Convert.ToString(parameter);
                string returnVal = string.Empty;

                if (double.TryParse(valueAsString, out sourceValue))
                    switch (unitType.ToUpper())
                    {
                        case "POWER":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.POWER, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "AIRFLOW":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.AIRFLOW, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "LENGTH_M":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.LENGTH_M, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "LENGTH_MM":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingDimensionUnit;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.LENGTH_MM, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "TEMPERATURE":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.TEMPERATURE, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "WEIGHT":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.WEIGHT, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "AREA":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingAREA;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.AREA, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "LOADINDEX":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.LOADINDEX, CurrentUnit)) + " " + CurrentUnit;
                            break;
                        case "WATERFLOWRATE":
                            CurrentUnit = SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate;
                            returnVal = string.Format("{0:0.00}", Unit.ConvertToSource(sourceValue, UnitType.WATERFLOWRATE, CurrentUnit)) + " " + CurrentUnit;
                            break;

                        default:
                            break;
                    }
                return returnVal;
            }


            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }


    }
}
