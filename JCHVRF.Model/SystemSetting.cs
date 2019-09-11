using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using JCHVRF.Model;
using JCBase.Utility;

namespace JCHVRF.Model
{
    public static class SystemSetting
    {
        public static string SettingFile = MyConfig.AppPath + @"\Settings.config.xml";
        //public static string SettingFile = MyConfig.AppPath + @"NVRF\Settings.config.xml";
        public static SystemSettingModel UserSetting = Deserialize();
        private static string _selectedLanguage;
        private static string _selectedLanguageCode;
        public static string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { _selectedLanguage = value; }
        }

        public static string SelectedLanguageCode
        {
            get { return _selectedLanguageCode; }
            set { _selectedLanguageCode = value; }
        }
        /// <summary>
        /// 获得个人设置，配置文件Settings.config.xml
        /// </summary>
        /// <returns></returns>
        public static SystemSettingModel Deserialize()
        {
            try
            {
                SystemSettingModel returnSetting = new SystemSettingModel();
                if (File.Exists(SystemSetting.SettingFile))
                {
                    using (FileStream fs = new FileStream(SystemSetting.SettingFile, FileMode.Open))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(SystemSettingModel));
                        //反序列化
                        returnSetting = (SystemSettingModel)xs.Deserialize(fs);
                        fs.Close();
                    }
                }
                return returnSetting;
            }
            catch(Exception e)
            {
                throw new Exception("Deserialize Error:" + e.Message);
            }
        }

        /// <summary>
        /// 配置个人设置，配置文件Settings.config.xml
        /// </summary>
        /// <returns></returns>
        public static void Serialize()
        {
            try
            {
                using (FileStream fs = new FileStream(SystemSetting.SettingFile, FileMode.Create))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(SystemSettingModel));
                    //序列化
                    xs.Serialize(fs, SystemSetting.UserSetting);
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Serialize Error:" + e.Message);
            }
        }

        /// <summary>
        /// 配置个人设置，配置文件Settings.config.xml
        /// </summary>
        /// <returns></returns>
        public static void Serialize(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(SystemSettingModel));
                    //序列化
                    xs.Serialize(fs, SystemSetting.UserSetting);
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Serialize Error:" + e.Message);
            }
        }
    }

    //system setting model
    [Serializable]
    public class SystemSettingModel
    {
        public UnitsSettingModel unitsSetting = new UnitsSettingModel();
        public DefaultSettingModel defaultSetting = new DefaultSettingModel();
        public AdvancedSettingModel advancedSetting = new AdvancedSettingModel();
        public FileSettingModel fileSetting = new FileSettingModel();
        public KeySettingModel keySetting = new KeySettingModel();
        public PipingSettingModel pipingSetting = new PipingSettingModel();
        public Location locationSetting = new Location();
        public LanguageModel language = new LanguageModel();
    }

    //units setting
    public class UnitsSettingModel
    {
        public string settingPOWER = Unit.ut_Capacity_kw;
        public string settingAIRFLOW = Unit.ut_Airflow_m3hr;
        public string settingLENGTH = Unit.ut_Size_m;
        public string settingDimension = Unit.ut_Dimension_mm;
        public string settingDimensionUnit = Unit.ut_Dimension_mm;
        public string settingTEMPERATURE = Unit.ut_Temperature_c;
        public string settingWEIGHT = Unit.ut_Weight_kg;
        public string settingHeight = Unit.ut_Height_m;
        public string settingAREA = Unit.ut_Area_m2;
        public string settingLOADINDEX = Unit.ut_LoadIndex_w;
        public string settingWaterFlowRate = Unit.ut_WaterFlowRate_m3h;
        public string settingESP = Unit.ut_Pressure;
        public string settingPressure = Unit.ut_Pressure;
    }

    // Region/SubRegion Setting
    public class Location
    {
        public string region = string.Empty;
        public string subRegion = string.Empty;
    }

    //default setting
    public class DefaultSettingModel
    {
        public string BuildingName { get; set; }
        public string FloorName { get; set; }
        public string RoomName { get; set; }
        public string FreshAirAreaName { get; set; }
        public string IndoorName { get; set; }
        public string OutdoorName { get; set; }
        public string ControllerName { get; set; }
        public string ExchangerName { get; set; }
        public double IndoorCoolingDB { get; set; }
        public double IndoorCoolingWB { get; set; }
        public double IndoorCoolingRH { get; set; }
        public double IndoorHeatingDB { get; set; }
        public double OutdoorCoolingDB { get; set; }
        public double OutdoorHeatingDB { get; set; }
        public double OutdoorHeatingWB { get; set; }
        public double OutdoorHeatingRH { get; set; }
        public double RoomHeight { get; set; }
        public string SalesEngineer { get; set; }
        public string SalesName { get; set; }
        public string SalesOffice { get; set; }
        public string Region { get; set; }
        public string SubRegion { get; set; }
        public string Language { get; set; }
        public string LanguageCode { get; set; }
        public double OutdoorCoolingIW { get; set; }
        public double OutdoorHeatingIW { get; set; }
        public bool IsIndoorAuto { get; set; }
        public bool IsRoomIndoorAuto { get; set; }

        public DefaultSettingModel()
        {
            BuildingName = "Building";
            FloorName = "Floor";
            RoomName = "Room";
            //FreshAirAreaName = "FreshAirArea";
            FreshAirAreaName = "VRF System";
            IndoorName = "Indoor";
            OutdoorName = "Outdoor";            
            ControllerName = "Central Control";
            ExchangerName = "Total Heat Exchanger";
            IndoorCoolingDB = 27;
            IndoorCoolingWB = 19.6;
            IndoorCoolingRH = 50;
            IndoorHeatingDB = 20;
            OutdoorCoolingDB = 35;
            OutdoorHeatingDB = 16;
            OutdoorHeatingWB = 10.5;
            OutdoorHeatingRH = 50;
            RoomHeight = 2.8;
            SalesEngineer = "";
            SalesName = "";
            SalesOffice = "";
            Region = "ME_A";
            SubRegion = "ME_T1";
            OutdoorCoolingIW = 35;
            OutdoorHeatingIW = 15;
            IsIndoorAuto = false;
            IsRoomIndoorAuto = true;
            Language = "English";
            LanguageCode = "EN";
        }
        
    }

    //advaced setting
    public class AdvancedSettingModel
    {
        public int indoorCooling = 0;
        public int indoorHeating = 0;
        public int outdoorCooling = 0;
        public int outdoorHeating = 0;
    }

    //file paths setting
    public class FileSettingModel
    {
        //public string projectFiles = System.Windows.Forms.Application.StartupPath + @"\VRF\ProjectFiles\";
        //public string pipingFiles = System.Windows.Forms.Application.StartupPath + @"\VRF\ProjectFiles\";
        public string DXFFiles = MyConfig.AppPath + @"\NVRF\ProjectFiles\";
        public string reportFiles = MyConfig.AppPath + @"\NVRF\ProjectFiles\";
        public bool EnableAltitudeCorrectionFactor = false;
        //设置系统默认语言
        public string settingLanguage = "";
    }

    //keyboard setting
    public class KeySettingModel
    {
        public string firstNewKey = "Ctrl";
        public string lastNewKey = "N";
        public string firstOpenKey = "Ctrl";
        public string lastOpenKey = "O";
        public string firstSaveKey = "Ctrl";
        public string lastSaveKey = "S";
        public string firstSaveAsKey = "Alt";
        public string lastSaveAsKey = "S";
        public string firstImportKey = "Ctrl";
        public string lastImportKey = "I";
        public string firstSettingKey = "Ctrl";
        public string lastSettingKey = "T";
        public string firstExitKey = "Ctrl";
        public string lastExitKey = "Q";
    }

    //piping setting
    public class PipingSettingModel
    {
        // 需要修改setting.config文件
        public double pipingEqLength = 40;      //60,update on 20141015 Gary提出
        public double firstBranchLength = 20;   //30
        public double pipingCorrectionFactor = 1;
        public PipingPositionType pipingPositionType = PipingPositionType.SameLevel;
        public double pipingHighDifference = 5; //10
    }

    //Language

    public class LanguageModel
    {
        public string Language = string.Empty;
        public string LanguageCode = string.Empty;
    }
}
