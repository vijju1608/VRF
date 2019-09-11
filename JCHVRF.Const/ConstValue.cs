//********************************************************************
// 文件名: ConstValue.cs
// 描述: 存放字符串常量，便于统一管理
// 作者: clh
// 创建时间: 2012-04-01
// 修改历史: --
//********************************************************************

using JCHVRF.VRFMessage;
namespace JCHVRF.Const
{
    public class ConstValue
    {
        #region Piping界面 -- 连接管管径规格前缀
        public const string prefixSpecG = "G";
        public const string prefixSpecL = "L";
        public const string prefixSpecFlag = "Φ";
        #endregion

        #region RegionCode (CDL.Sec) add on 20120817 clh
        /* 21 China;22 HongKong;23 Asia;24 MiddleEast;25 European;26 LatinAmerica;27 India;28 Thailand;29 Other;99 ALL;00 None */
        public const string RegionCodeChina = "21";
        
        public static string[] RegionCodeAisa = { "22", "23", "27", "28" };
        #endregion

        #region Unique Outdoor Name 注意检验此处列出的唯一对应的室外机型号与数据库一致！！
        public static string[] ArrUniqueOutdoor_Asia_IndModel = 
        {
            "YDCF2500H0NE-0E1",
            "YDCF3000H0NE-0E1",
            "YDCF3500H0NE-0E1",
            "YDCF4000H0NE-0E1",
            "YDCF5000H0NE-0E1",
            "YDCF6000H0NE-0E1"
        };
        public static string[] ArrUniqueOutdoor_Asia = 
        {
            "YVOH100VVEE-0A1",
            "YVOH100VVEE-0A1",
            "YVOH140VVEE-0A1",
            "YVOH160VVEE-0A1",
            "YVOH200VAEE-0A1",
            "YVOH200VAEE-0A1"
            //"YVOH200VNEE-0A1",
            //"YVOH200VNEE-0A1"
        };
        public static string[] ArrUniqueOutdoor_ME_IndModel = 
        {
            "YDCF2500H0NE-0E1",
            "YDCF3000H0NE-0E1",
            "YDCF3500H0NE-0E1",
            "YDCF3500H0NE-0E1",
            "YDCF4000H0NE-0E1",
            "YDCF6000H0NE-0E1"
        };
        public static string[] ArrUniqueOutdoor_ME = 
        {
            "YVOH100VVEE-0A1",
            "YVOH100VVEE-0A1",
            "YVOH140VVEE-0A1",
            "YVOH160VVEE-0A1",
            "YVOH200VAEE-0A1",
            "YVOH200VAEE-0A1"
            //"YVOH200VNEE-0A1",
            //"YVOH200VNEE-0A1"
        };
        public static string[] ArrUniqueOutdoor_China_IndModel = 
        {
            "YDCF2500H0NE-0E",
            "YDCF3000H0NE-0E",
            "YDCF3500H0NE-0E",
            "YDCF3500H0NE-0E",
            "YDCF4000H0NE-0E",
            "YDCF6000H0NE-0E"
        };
        public static string[] ArrUniqueOutdoor_China = 
        {
            "YVOH100VVEE",
            "YVOH100VVEE",
            "YVOH140VVEE",
            "YVOH160VVEE",
            "YVOH200VAEE",
            "YVOH200VAEE"
            // changed on 20130726 去掉“-0A”后缀
        };
        public static string[] ArrUniqueOutdoor_China_T1 = 
        {
            "YDOH100EVV-0C",
            "YDOH100EVV-0C",
            "YDOH140EVV-0C",
            "YDOH160EVV-0C",
            "YDOH200EVN-0C",
            "YDOH200EVN-0C"
        };

        #endregion


        #region 控制器相关型号以及室外机组件
        //public const string Controller_TOUCH = "CVH-01";
        //public const string Controller_ONOFF = "KVH-02";
        //public const string ControllerAssembly_ModbusHor = "YORK-DRV-002";
        //public const string ControllerAssembly_ModbusTop = "YORK-DRV-007";
        //public const string ControllerAssembly_BacnetHor = "YORK-TRS-003A";
        //public const string ControllerAssembly_BacnetTop = "YORK-TRS-003B";

        //public static string Controller_TOUCH_MEMO
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("Controller_TOUCH_MEMO");
        //    }
        //}

        //public static string Controller_ONOFF_MEMO
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("Controller_ONOFF_MEMO");
        //    }
        //}

        //public static string ControllerAssembly_ModbusHor_MEMO
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("Controller_ONOFF_MEMO");
        //    }
        //}

        //public static string ControllerAssembly_ModbusTop_MEMO
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("ControllerAssembly_ModbusTop_MEMO");
        //    }
        //}

        //public static string ControllerAssembly_BacnetHor_MEMO
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("ControllerAssembly_BacnetHor_MEMO");
        //    }
        //}

        //public static string ControllerAssembly_BacnetTop_MEMO
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("ControllerAssembly_BacnetTop_MEMO");
        //    }
        //}
        #endregion

    }
}

