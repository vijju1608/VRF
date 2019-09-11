using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace JCHVRF.Model.New
{
    //[XmlInclude(typeof(condition))]

    [Serializable]
    public class DesignCondition
    {


        public static string SettingFile = AppDomain.CurrentDomain.BaseDirectory + @"\Settings.config.xml";
        public decimal NumericAltitude { get; set; }
        public decimal indoorCoolingDB { get; set; } //= 27.0m
        public decimal indoorCoolingWB { get; set; }// = 19.6m;
        public decimal indoorCoolingRH { get; set; }// = 0.0m;
        public decimal indoorCoolingHDB { get; set; }//= 20.0m;
        public decimal outdoorCoolingDB { get; set; }// = 35.0m;
        public decimal NumericOutdoorIntelWater { get; set; }
        public decimal outdoorHeatingDB { get; set; }//= 7.0m;
        public decimal outdoorHeatingWB { get; set; }//= 3.1m;
        public decimal outdoorHeatingRH { get; set; } //= 87.00m;
        public decimal outdoorCoolingIW { get; set; }//= 35;
        public decimal outdoorHeatingIW { get; set; } //= 15;


        // public string ProjectName = "VRFProject";

        //public DesignCondition(DesignCondition designcondition)
        //{
        //    condition = designcondition;
        //}

        public static void Serialize(string filePath, DesignCondition design)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(DesignCondition));
                    //序列化
                    //xs.Serialize(fs, DesignCondition.UserSetting);
                    xs.Serialize(fs, design);
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Serialize Error:" + e.Message);
            }
        }
        public static DesignCondition Deserialize()
        {
            try
            {
                DesignCondition returnSetting = new DesignCondition();

                // if (checkFile(DesignCondition.SettingFile))
                // {

                if (File.Exists(DesignCondition.SettingFile))
                {

                    using (FileStream fs = new FileStream(DesignCondition.SettingFile, FileMode.Open))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(DesignCondition));
                        //反序列化
                        returnSetting = (DesignCondition)xs.Deserialize(fs);
                        fs.Close();
                    }
                }

                //}
                return returnSetting;
            }
            catch (Exception e)
            {
                throw new Exception("Deserialize Error:" + e.Message);
            }
        }
        public static bool checkFile(string filePath)
        {
            bool returnCheck = false;

            System.Xml.XmlDocument docXML = new System.Xml.XmlDocument();
            try
            {
                docXML.Load(filePath);
            }
            catch
            {
                return returnCheck;
            }

            System.Xml.XmlNode root = docXML.SelectSingleNode("DesignCondition");
            if (root == null)
            {
                return returnCheck;
            }

            return true;
        }
    }
}


