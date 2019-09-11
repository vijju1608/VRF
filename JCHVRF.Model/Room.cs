//********************************************************************
// 文件名: Room.cs
// 描述: 定义 VRF 项目中的房间类
// 作者: clh
// 创建时间: 2012-04-01
// 修改历史: 
// 2016-1-29 迁入JCHVRF
//********************************************************************

using JCBase.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    [Serializable]
    public class Room : ModelBase
    {
        public Room() { }
        public Room(int number)
        {
            this._no = number;
            this._area = 30;
            this._loadIndexCool = 170;
            this._rqCapacityCool = 5.1;
            this._loadIndexHeat = 150;
            this._rqCapacityHeat = 4.5;
            this._peopleNumber = 6;
            this._freshAirIndex = 0.5;    // m3/h时为30，m3/min时为0.5，默认为m3/min
            this._freshAir = 3;               // _peopleNumber * _freshAirIndex  m3/h时为180
        }

        private string _id;
        /// <summary>
        /// 唯一编号
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { this.SetValue(ref _id, value); }
        }

        private int _no;
        /// <summary>
        /// 递增编号，自动生成
        /// </summary>
        public int NO
        {
            get { return _no; }
            set { this.SetValue(ref _no, value); }
        }

        private string _name;
        /// <summary>
        /// 房间自定义名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        private string _location;
        /// <summary>
        /// 房间所在的城市，add on 20130812 clh
        /// </summary>
        public string Location
        {
            get { return _location; }
            set { this.SetValue(ref _location, value); }
        }

        private string _type;
        /// <summary>
        /// 房间类型
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { this.SetValue(ref _type, value); }
        }

        private double _area;
        /// <summary>
        /// 房间面积m^2 (默认值30)
        /// </summary>
        public double Area
        {
            get { return _area; }
            set { this.SetValue(ref _area, value); }
        }

        private double _loadIndexCool;
        /// <summary>
        /// 冷指标w/m^2
        /// </summary>
        public double LoadIndexCool
        {
            get { return _loadIndexCool; }
            set { this.SetValue(ref _loadIndexCool, value); }
        }

        private double _loadIndexHeat;
        /// <summary>
        /// 热指标w/m^2
        /// </summary>
        public double LoadIndexHeat
        {
            get { return _loadIndexHeat; }
            set { this.SetValue(ref _loadIndexHeat, value); }
        }

        private double _rqCapacityCool;
        /// <summary>
        /// 冷负荷需求kw
        /// </summary>
        public double RqCapacityCool
        {
            get { return _rqCapacityCool; }
            set { this.SetValue(ref _rqCapacityCool, value);
                if (_rqCapacityCool != null)
                    ValidateHeatSensible();
            }
        }

        private double _rqCapacityHeat;
        /// <summary>
        /// 冷负荷需求kw
        /// </summary>
        public double RqCapacityHeat
        {
            get { return _rqCapacityHeat; }
            set { this.SetValue(ref _rqCapacityHeat, value); }
        }

        private double _sensibleHeat;
        /// <summary>
        /// 显热 Sensible Heat (kW)
        /// </summary>
        public double SensibleHeat
        {
            get { return _sensibleHeat; }
            set { this.SetValue(ref _sensibleHeat, value);
                if (_sensibleHeat != null)
                    ValidateHeatSensible();
            }
        }

        private double _airFlow;
        /// <summary>
        /// 风量 Air Flow (m3/h)
        /// </summary>
        public double AirFlow
        {
            get { return _airFlow; }
            set { this.SetValue(ref _airFlow, value); }
        }

        private double _staticPressure;
        /// <summary>
        /// 静压 Static Pressure (Pa)
        /// </summary>
        public double StaticPressure
        {
            get { return _staticPressure; }
            set { this.SetValue(ref _staticPressure, value); }
        }

        private int _peopleNumber;
        /// <summary>
        /// 房间人数
        /// </summary>
        public int PeopleNumber
        {
            get { return _peopleNumber; }
            set { this.SetValue(ref _peopleNumber, value); }
        }

        private bool _isSensibleHeatEnable;
        /// <summary>
        /// 显热值是否有效
        /// </summary>
        public bool IsSensibleHeatEnable
        {
            get { return _isSensibleHeatEnable; }
            set { this.SetValue(ref _isSensibleHeatEnable, value); }
        }

        private bool _isAirFlowEnable;
        /// <summary>
        /// 风量值是否有效
        /// </summary>
        public bool IsAirFlowEnable
        {
            get { return _isAirFlowEnable; }
            set { this.SetValue(ref _isAirFlowEnable, value); }
        }

        private bool _isStaticPressureEnable;
        /// <summary>
        /// 静压值是否有效
        /// </summary>
        public bool IsStaticPressureEnable
        {
            get { return _isStaticPressureEnable; }
            set { this.SetValue(ref _isStaticPressureEnable, value); }
        }

        private double _freshAirIndex;
        /// <summary>
        /// 新风指标
        /// </summary>
        public double FreshAirIndex
        {
            get { return _freshAirIndex; }
            set { this.SetValue(ref _freshAirIndex, value); }
        }

        private double _freshAir;
        /// <summary>
        /// 新风风量需求
        /// </summary>
        public double FreshAir
        {
            get { return _freshAir; }
            set { this.SetValue(ref _freshAir, value); }
        }
        //end 20120313 

        private string _freshAirAreaId;
        /// <summary>
        /// 所属的新风区域的ID
        /// </summary>
        public string FreshAirAreaId
        {
            get { return _freshAirAreaId; }
            set { this.SetValue(ref _freshAirAreaId, value); }
        }

        //New Properties added for Add/Edit Room start
        private double _CoolingDryBulb;

        public double CoolingDryBulb
        {
            get { return _CoolingDryBulb; }
            set
            {
                this.SetValue(ref _CoolingDryBulb, value);
                ChangeFunction("CoolingDryBulb");
                IsMethordCheck = false;
            }
        }

        private double _CoolingWetBulb;

        public double CoolingWetBulb
        {
            get { return _CoolingWetBulb; }
            set { this.SetValue(ref _CoolingWetBulb, value);
                ChangeFunction("CoolingWetBulb");
                IsMethordCheck = false;
            }
        }

        private double _CoolingRelativeHumidity;

        public double CoolingRelativeHumidity
        {
            get { return _CoolingRelativeHumidity; }
            set { this.SetValue(ref _CoolingRelativeHumidity, value);
                //ChangeFunction("CoolingRelativeHumidity");
                //IsMethordCheck = false;
            }
        }

        
        private double _HeatingDryBulb;

        public double HeatingDryBulb
        {
            get { return _HeatingDryBulb; }
            set { this.SetValue(ref _HeatingDryBulb, value);
                ChangeFunction("HeatingDryBulb");
                IsMethordCheck = false;
            }
        }

        private bool _IsRoomChecked = false;
        public bool IsRoomChecked
        {
            get
            { return _IsRoomChecked; }
            set
            {
                this.SetValue(ref _IsRoomChecked, value);
            }
        }

        private string _lblindoorCoolingDB;
        public string lblindoorCoolingDB
        {
            get
            {
                return _lblindoorCoolingDB;
            }
            set
            {
                this.SetValue(ref _lblindoorCoolingDB, value);
            }
        }
        private string _lblindoorSensible;
        public string lblindoorSensible
        {
            get
            {
                return _lblindoorSensible;
            }
            set
            {
                this.SetValue(ref _lblindoorSensible, value);
            }
        }
        private string _lblindoorCoolingWB;
        public string lblindoorCoolingWB
        {
            get
            {
                return _lblindoorCoolingWB;
            }
            set
            {
                this.SetValue(ref _lblindoorCoolingWB, value);
               
            }
        }

        private string _lblindoorHeatingDB;
        public string lblindoorHeatingDB
        {
            get
            {
                return _lblindoorHeatingDB;
            }
            set
            {
                this.SetValue(ref _lblindoorHeatingDB, value);
            }
        }

        /// Validation 

        private bool ValidateHeatSensible()
        {

            if (Convert.ToDecimal(RqCapacityCool) < Convert.ToDecimal(SensibleHeat))
            {
                lblindoorSensible = string.Format("["+RqCapacityCool+">"+SensibleHeat+"]");
                //lblindoorSensible = string.Empty;
                return true;

            }
            else
            {
                lblindoorSensible = string.Empty;
                return false;
            }
        }

        /// </summary>

        //New Properties added for Add/Edit Room end


        #region Validation 
        bool IsMethordCheck = false;
        private void ChangeFunction(string Type)
        {
            if (Type == "CoolingDryBulb" && IsMethordCheck == false)
            {
                NumericCoolDryBulb_LostFocus();
            }
            else if (Type == "CoolingWetBulb" && IsMethordCheck == false)
            {
                NumericCoolWetBulb_LostFocus();
            }
            else if (Type == "CoolingRelativeHumidity" && IsMethordCheck == false)
            {
                NumericInternalRH_LostFocus();
            }
            else if (Type == "HeatingDryBulb" && IsMethordCheck == false)
            {
                NumericHeatingDryBulb_LostFocus();
            }

        }

        private void NumericHeatingDryBulb_LostFocus()
        {
            if (ValidateHDB() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd(UnitTemperature.RH.ToString()); //issue fix 1647
                //  DoCalculateByOptionInd(NumericInternalRH.Value.ToString());
            }
        }

        

        public static string SettingFile = AppDomain.CurrentDomain.BaseDirectory + @"\Settings.config.xml";
        private string _currentTempUnit;
        public string CurrentTempUnit
        {
            get { return SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE; }
            set
            {
                SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = value;
                string display = value == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c;
                this.SetValue(ref _currentTempUnit, value);
                DisplayCurrentTempUnit = display;
            }
        }
        private string _displayCurrentTempUnit;
        public string DisplayCurrentTempUnit
        {
            get { return CurrentTempUnit == Unit.ut_Temperature_c ? Unit.ut_Temperature_f : Unit.ut_Temperature_c; ; }
            set
            {

               this.SetValue(ref _displayCurrentTempUnit, value);
            }
        }
        public void NumericCoolDryBulb_LostFocus()
        {
            if (ValidateCoolDryBulb() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd("DB");
            }
        }
        private void NumericCoolWetBulb_LostFocus()
        {
            /*else*/
            if (ValidateCoolWetBulb() == false)
            {

            }


            else
            {
                DoCalculateByOptionInd("WB");
            }
        }
        private bool ValidateRH()
        {
            double nRHVal = Convert.ToDouble(CoolingRelativeHumidity);

            if ((nRHVal >= 13) && (nRHVal <= 100))
            {
               // lblindoorCoolingRH = string.Empty;
                return true;

            }
            else
            {
               // lblindoorCoolingRH = "Range[13, 100]";
                return false;
            }

        }
        private void NumericInternalRH_LostFocus()
        {

            if (ValidateRH() == false)
            {

            }
            else
            {
                DoCalculateByOptionInd(UnitTemperature.RH.ToString()); //issue fix 1647
                                                                       // DoCalculateByOptionInd(NumericInternalRH.Value.ToString());
            }
        }

        public void DoCalculateByOptionInd(string Opt)
        {
            //double dbcool = Unit.ConvertToSource(Convert.ToDouble(CoolingDryBulb.ToString()), UnitType.TEMPERATURE, CurrentTempUnit);
            //double wbcool = Unit.ConvertToSource(Convert.ToDouble(CoolingWetBulb.ToString()), UnitType.TEMPERATURE, CurrentTempUnit);
            double dbcool = Convert.ToDouble(CoolingDryBulb.ToString());
            double wbcool = Convert.ToDouble(CoolingWetBulb.ToString());
            double rhcool = Convert.ToDouble(CoolingRelativeHumidity);
            FormulaCalculate fc = new FormulaCalculate();
            decimal pressure = fc.GetPressure(Convert.ToDecimal(0));
            if (Opt == UnitTemperature.WB.ToString())
            {
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));

                if (this.CoolingRelativeHumidity.ToString() != (rh * 100).ToString("n0"))
                {
                    IsMethordCheck = true;
                    CoolingRelativeHumidity = Convert.ToDouble((rh * 100).ToString("n0"));
                    
                }
            }
            else if (Opt == UnitTemperature.DB.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (CoolingDryBulb.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {
                        IsMethordCheck = true;
                        CoolingWetBulb = Convert.ToDouble(wb.ToString("n1"));
                    }
                }

            }
            else if (Opt == UnitTemperature.RH.ToString())
            {
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));

                if (this.CoolingWetBulb.ToString() != wb.ToString("n1"))
                {
                    if (rhcool != 0)
                    {
                        IsMethordCheck = true;
                        this.CoolingWetBulb = (double)wb;
                        
                    }

                }
            }
        }
        private bool ValidateCoolDryBulb()
        {

            double nCDBVal =(Convert.ToDouble(CoolingDryBulb));
            if ((nCDBVal >= 16) && (nCDBVal <= 30))
            {

                 lblindoorCoolingDB = string.Empty;
                return true;

            }
            else
            {
                 lblindoorCoolingDB = string.Format("[{0}, {1}]", Unit.ConvertToControl(16, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(30, UnitType.TEMPERATURE, CurrentTempUnit));

                if (Convert.ToDecimal(CoolingDryBulb) < Convert.ToDecimal(CoolingWetBulb) && !(CoolingDryBulb == 0))
                {

                    //JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));

                }

                return false;
            }
        }
        private bool ValidateHDB()
        {
            double nHDBVal = Math.Round((Convert.ToDouble(HeatingDryBulb)));// Convert.ToDouble(indoorHeatingDB);

            if ((nHDBVal >= 16) && (nHDBVal <= 24))
            {
                //DCErrorMessage = string.Empty;
                lblindoorHeatingDB = string.Empty;
                return true;

            }
            else
            {
                lblindoorHeatingDB = string.Format("Range[{0}, {1}]", Unit.ConvertToControl(16, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(24, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[16, 24]";
                //DCErrorMessage = "Tempreture values entered are abnormal";
                return false;
            }

        }
        private bool ValidateCoolWetBulb()
        {
            double nCWBVal =(Convert.ToDouble(CoolingWetBulb)); //Convert.ToDouble(indoorCoolingWB);

            if ((nCWBVal >= 14) && (nCWBVal <= 24))
            {

                 lblindoorCoolingWB = string.Empty;
                if (Convert.ToDecimal(CoolingWetBulb) > Convert.ToDecimal(CoolingDryBulb))
                {

                    //JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));

                    return false;
                }

                return true;

            }
            else
            {
                 lblindoorCoolingWB = string.Format("[{0}, {1}]", Unit.ConvertToControl(14, UnitType.TEMPERATURE, CurrentTempUnit), Unit.ConvertToControl(24, UnitType.TEMPERATURE, CurrentTempUnit)); //"Range[14, 24]";

                if (Convert.ToDecimal(CoolingWetBulb) > Convert.ToDecimal(CoolingWetBulb))
                {

                    //JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));

                }
                return false;
            }


        }
        #endregion
    }

}
