using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF_New.Model;

namespace JCHVRF.Model
{
    #region 控制器 Controller : BaseController
    /// <summary>
    /// 控制器
    /// </summary>
    [Serializable]
    public class Controller : BaseController
    {
        private readonly CentralController _centralController;

        public CentralController CentralController
        {
            get { return _centralController; }
        }

        public Controller(CentralController type)
            : base()
        {
            
            this.Quantity = 0;
            this._centralController = type;
            Name = _centralController.Model;
        }


        public string Image
        {
            get { return _centralController.Image; }
        }

        
        /// 控制器类型
        /// <summary>
        /// 控制器类型
        /// </summary>
        public ControllerType Type
        {
            get
            {
                return _centralController.Type;
            }
        }

        /// <summary>
        /// 控制器型号
        /// </summary>
        public string Model
        {
            get { return _centralController.Model; }
        }
        /// 设置或更改控制器类型，Name属性随之改变
        /// <summary>
        /// 设置或更改控制器类型，Name属性随之改变
        /// </summary>
      

        /// 描述内容
        /// <summary>
        /// 描述内容
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Description
        {
            get { return _centralController.Description; }
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }

        private string _controlGroupID;
        /// 所属的 ControlGroup
        /// <summary>
        /// 所属的 ControlGroup
        /// </summary>
        public string ControlGroupID
        {
            get { return _controlGroupID; }
            set { _controlGroupID = value; }
        }

        private string _controlSystemID;
        /// 所属的 ControlSystem
        /// <summary>
        /// 所属的 ControlSystem
        /// </summary>
        public string ControlSystemID
        {
            get { return _controlSystemID; }
            set { _controlSystemID = value; }
        }

        /// 添加到指定的Control Group
        /// <summary>
        /// 添加到指定的Control Group
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public void AddToControlGroup(string groupID)
        {
            this._controlGroupID = groupID;
            //this._controlSystemID = "";
        }

        /// 添加到指定的Control System
        /// <summary>
        /// 添加到指定的Control System
        /// </summary>
        /// <param name="sysID"></param>
        /// <returns></returns>
        public void AddToControlSystem(string sysID)
        {
            this._controlSystemID = sysID;
            ////this._controlGroupID = "";
        }
    }
    #endregion

    #region 控制器组 ControlGroup : BaseController
    /// <summary>
    /// 控制器组
    /// </summary>
    [Serializable]
    public class ControlGroup : BaseController
    {
        public ControlGroup()
            : base()
        {
            this._isValidGrp = false;
        }

        private string _controlSystemID;
        /// 所属的Control System
        /// <summary>
        /// 所属的Control System
        /// </summary>
        public string ControlSystemID
        {
            get { return _controlSystemID; }
        }
        
        /// 添加到指定的Control System
        /// <summary>
        /// 添加到指定的Control System
        /// </summary>
        /// <param name="sysID"></param>
        /// <returns></returns>
        public void AddToControlSystem(string sysID)
        {
            this._controlSystemID = sysID;
        }

        /// 获取最大的控制器数量
        /// <summary>
        /// 获取最大的控制器数量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetMaxControllerCount(ControllerLayoutType type)
        {
            if (type == ControllerLayoutType.MODBUS)
            {
                return 1;
            }
            return 16;
        }
       
        ///// 获取最大的可连接的室外机数量，用于拖放Controller控件前的检测
        ///// <summary>
        ///// 获取最大的可连接的室外机数量，用于拖放Controller控件前的检测
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="controllerType"></param>
        ///// <returns></returns>
        //public int GetMaxOutdoorCount(ControllerLayoutType type, ControllerType controllerType)
        //{
        //    if (type == ControllerLayoutType.MODBUS && controllerType == ControllerType.ONOFF)
        //    {
        //        return 4;
        //    }
        //    return 16;
        //}

        /// 获取当前控制器组所属的控制器系统对象
        /// <summary>
        /// 获取当前控制器组所属的控制器系统对象
        /// </summary>
        /// <param name="thisProject">当前项目对象</param>
        /// <returns></returns>
        public ControlSystem GetParent_ControlSystem(ref Project thisProject)
        {
            if (!String.IsNullOrEmpty(this._controlSystemID))
            {
                return thisProject.ControlSystemList.Where(p => p.Id == this._controlSystemID).FirstOrDefault();
            }
            return null;
        }


        private bool _isValidGrp;
        /// <summary>
        /// 是否有效，false即为空白的ControlGroup
        /// </summary>
        public bool IsValidGrp
        {
            get { return _isValidGrp; }
            set
            {
                this.SetValue(ref _isValidGrp, value);
            }
        }

        public void SetName(string title)
        {
            Name = title;
        }
    }

    #endregion

    #region 控制器系统 ControlSystem : BaseController
    /// <summary>
    /// 控制器系统
    /// </summary>
    [Serializable]
    public class ControlSystem : SystemBase
    {
        public ControlSystem()
            : base()
        {
            _isValid = false;
        }

        private bool _isValid;
        /// <summary>
        /// 是否有效，false即为空白的ControlSystem
        /// </summary>
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                this.SetValue(ref _isValid, value);
                SystemStatus = value ? SystemStatus.WIP : SystemStatus.INVALID;
            }
        }

        private bool _isAutoWiringPerformed;
        /// <summary>
        /// True or False, AutoWiring Status
        /// </summary>
        public bool IsAutoWiringPerformed
        {
            get { return _isAutoWiringPerformed;}
            set
            {
                this.SetValue(ref _isAutoWiringPerformed, value);
            }
        }

        /// 获取最大的控制器数量
        /// <summary>
        /// 获取最大的控制器数量,挪至Control System中
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetMaxControllerCount(ControllerLayoutType type)
        {
            if (type == ControllerLayoutType.MODBUS)
            {
                return 1;
            }
            return 16;
        }

        public void SetName(string systemName)
        {
            Name = systemName;
        }

        public List<SystemsOnCanvas> SystemsOnCanvasList = new List<SystemsOnCanvas>();
    }
    [Serializable]
    public class SystemsOnCanvas
    {
        public string SystemName { get; set; }
        public object System { get; set; }
    }
    #endregion

    #region 控制器组件 ControllerAssembly:BaseController
    /// <summary>
    /// 控制器组件
    /// </summary>
    [Serializable]
    public class ControllerAssembly : BaseController
    {
        public ControllerAssembly(ControllerLayoutType layoutType, SystemVRF sysItem)
        {
            if(sysItem.OutdoorItem==null)
                return;

            this._systemVRFID = sysItem.Id;
            this._controllerAssemblyType = sysItem.OutdoorItem.Type.ToUpper().Contains("TOP") ? AirDirectionType.TOP : AirDirectionType.HORIZONTAL;

            switch (layoutType)
            {
                case ControllerLayoutType.MODBUS:

                    switch (_controllerAssemblyType)
                    {
                        case AirDirectionType.HORIZONTAL:
                            Name = (ControllerConstValue.ControllerAssembly_ModbusHor);
                            break;
                        case AirDirectionType.TOP:
                            Name = (ControllerConstValue.ControllerAssembly_ModbusTop);
                            break;
                        default:
                            break;
                    }
                    break;
                case ControllerLayoutType.BACNET:
                    switch (_controllerAssemblyType)
                    {
                        case AirDirectionType.HORIZONTAL:
                            Name = (ControllerConstValue.ControllerAssembly_BacnetHor);
                            break;
                        case AirDirectionType.TOP:
                            Name = (ControllerConstValue.ControllerAssembly_BacnetTop);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

        }

        //private string _name;
        ///// <summary>
        ///// 名称，型号
        ///// </summary>
        //public string Name
        //{
        //    get { return _name; }
        //}

        private string _controlGroupID;
        /// 所属的 ControlGroup
        /// <summary>
        /// 所属的 ControlGroup
        /// </summary>
        public string ControlGroupID
        {
            get { return _controlGroupID; }
        }

        private string _systemVRFID;
        /// 关联的室外机系统ID
        /// <summary>
        /// 关联的室外机系统ID
        /// </summary>
        public string SystemVRFID
        {
            get { return _systemVRFID; }
        }

        private AirDirectionType _controllerAssemblyType;
        /// 空调外机出风口方向，侧出风|上出风
        /// <summary>
        /// 空调外机出风口方向，侧出风|上出风
        /// </summary>
        public AirDirectionType ControllerAssemblyType
        {
            get { return _controllerAssemblyType; }
        }

        /// 添加到指定的Control Group
        /// <summary>
        /// 添加到指定的Control Group
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public bool AddToControlGroup(string groupID)
        {
            this._controlGroupID = groupID;
            return true;
        }

        //void SetName(string name) { this._name = name; }

        /// 获取描述内容
        /// <summary>
        /// 获取描述内容
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetDescription(string name)
        {
            switch (name)
            {
                case ControllerConstValue.ControllerAssembly_ModbusHor:
                    return ControllerConstValue.ControllerAssembly_ModbusHor_MEMO;
                case ControllerConstValue.ControllerAssembly_ModbusTop:
                    return ControllerConstValue.ControllerAssembly_ModbusTop_MEMO;
                case ControllerConstValue.ControllerAssembly_BacnetHor:
                    return ControllerConstValue.ControllerAssembly_BacnetHor_MEMO;
                case ControllerConstValue.ControllerAssembly_BacnetTop:
                    return ControllerConstValue.ControllerAssembly_BacnetTop_MEMO;
                default:
                    break;
            }
            return "";
        }

    }
    #endregion

    #region baseInfo

    /// 基础组件
    /// <summary>
    /// 基础组件
    /// </summary>
    [Serializable]
    public class BaseController :ModelBase
    {
        public BaseController()
        {
            this._id = Guid.NewGuid().ToString("N");
        }

        private string _id;
        /// <summary>
        /// 唯一编号
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name { get; set; }
    }

    /// 控制器组的类型
    /// <summary>
    /// 控制器组的类型 MODBUS | BACNET
    /// </summary>
    public enum ControllerLayoutType { 
        NoBMS,
        BACNET, 
        MODBUS,
        LONWORKS,
        KNX,
        ALL
    }

    ///// 控制器类型
    ///// <summary>
    ///// 控制器类型 ONOFF | TOUCH
    ///// </summary>
    //public enum ControllerType
    //{
    //    /// <summary>
    //    /// 开关
    //    /// </summary>
    //    ONOFF,
    //    /// <summary>
    //    /// 触摸屏
    //    /// </summary>
    //    CC
    //}
    public enum ControllerType
    {
        BACNetInterface,
        LonWorksInterface,
        ModBusInterface,
        KNXInterface,
        CentralController,
        ONOFF,
        Software
    }

    /// 空调外机出风口类型 TOP | HORIZONTAL
    /// <summary>
    /// 空调外机出风口类型 TOP | HORIZONTAL
    /// </summary>
    public enum AirDirectionType
    {
        /// <summary>
        /// 侧出风
        /// </summary>
        HORIZONTAL,
        /// <summary>
        /// 上出风
        /// </summary>
        TOP
    }
    #endregion

    #region 备选区控制器类型 CentralController
    [Serializable]
    public class CentralController
    {
        public CentralController()
        {
        }
     
        /// <summary>
        /// 区域代码
        /// </summary>
        public string RegionCode { get; set; }
        /// <summary>
        /// 品牌代码
        /// </summary>
        public string BrandCode { get; set; }
        /// <summary>
        /// 工厂代码
        /// </summary>
        public string FactoryCode { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// 控制器类型
        /// </summary>
        public ControllerType Type { get; set; }
        /// <summary>
        /// 控制器描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// York型号
        /// </summary>
        public string Model_York { get; set; }
        /// <summary>
        /// Hitachi型号
        /// </summary>
        public string Model_Hitachi { get; set; }
        /// <summary>
        /// 型号，根据品牌代码选择York型号或Hitachi型号
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// H-Link版本(H-Link I 或者 H-Link II）
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// 与这个控制器兼容的控制器型号
        /// </summary>
        public string CompatibleModel { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大控制器数量（不区分型号）
        /// </summary>
        public int MaxControllerNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大（室内机+室外机）的数量
        /// </summary>
        public int MaxUnitNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大控制器数量（这个型号）
        /// </summary>
        public int MaxSameModel { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大控制器数量（与这个控制器类型相同的控制器）
        /// </summary>
        public int MaxSameType { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大设备数量（包括室内机、室外机、控制器）
        /// </summary>
        public int MaxDeviceNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大系统数量
        /// </summary>
        public int MaxSystemNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大遥控器数量
        /// </summary>
        public int MaxRemoteControllerNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大控制器大组数量
        /// </summary>
        public int MaxGroupNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大控制器小组数量
        /// </summary>
        public int MaxBlockNumber { get; set; }
        /// <summary>
        /// 在一个H-Link中，最大室内机数量
        /// </summary>
        public int MaxIndoorUnitNumber { get; set; }
        /// <summary>
        /// 最大配线长度
        /// </summary>
        public int MaxWiringLength { get; set; }
        /// <summary>
        /// 控制器图片名称
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 成套使用的其它控制器
        /// </summary>
        public List<CentralController> ControllersInSet { get; set; }

        public override bool Equals(object obj)
        {
            var controller = obj as CentralController;
            return controller != null &&
                   RegionCode == controller.RegionCode &&
                   BrandCode == controller.BrandCode &&
                   FactoryCode == controller.FactoryCode &&
                   ProductType == controller.ProductType &&
                   Type == controller.Type &&
                   Model == controller.Model;
        }

        public override int GetHashCode()
        {
            var hashCode = -1245819066;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RegionCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BrandCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FactoryCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProductType);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Model);
            return hashCode;
        }
    }
    #endregion

    public class ControllerConstValue
    {
        #region H-Link 与 H-Link II 不同的限制条件
        public const int HLINK_MAX_ODU_QTY = 16;   //Max. refrigerant system q'ty(Max. connectable ODU) of H-Link I
        public const int HLINKII_MAX_ODU_QTY = 64; //Max. refrigerant system q'ty(Max. connectable ODU) of H-Link II
        public const int HLINK_MAX_IDU_QTY = 128; //Max. connectable IDU q'ty of H-Link I
        public const int HLINKII_MAX_IDU_QTY = 160; //Max. connectable IDU q'ty of H-Link II
        public const int HLINK_MAX_DEVICE_QTY = 145; //Max. connectable IDU q'ty of H-Link I
        public const int HLINKII_MAX_DEVICE_QTY = 200; //Max. connectable IDU q'ty of H-Link II
        public const int HLINK_MAX_CC_QTY = 8; //upto 8 Central Controllers in 1 H-Link
        public const int HLINKII_MAX_CC_QTY = 8; //upto 8 Central Controllers in 1 H-Link
        #endregion

        #region 其它限制条件
        public const int BACNET_MAX_CC_QTY = 8;
        #endregion
        #region 控制器相关型号以及室外机组件
        public const string Controller_TOUCH = "CVH-01";
        public const string Controller_ONOFF = "KVH-02";
        public const string ControllerAssembly_ModbusHor = "YORK-DRV-002";
        public const string ControllerAssembly_ModbusTop = "YORK-DRV-007";
        public const string ControllerAssembly_BacnetHor = "YORK-TRS-003A";
        public const string ControllerAssembly_BacnetTop = "YORK-TRS-003B";

        //public const string ControllerType_CentralStationNT = "CENTRAL STATION NT";
        //public const string ControllerType_CentralStationDX = "CENTRAL STATION DX";
        //public const string ControllerType_BACNetInterface = "BACNET INTERFACE";
        //public const string ControllerType_LonWorksInterface = "LONWORKS INTERFACE";
        //public const string ControllerType_CentralStationMini = "CENTRAL STATION MINI";
        //public const string ControllerType_WebBasedCotnrol = "WEB BASED COTNROL";
        //public const string ControllerType_ModBusInterface = "MODBUS INTERFACE";
        //public const string ControllerType_CentralController = "CENTRAL CONTROLLER";
        //public const string ControllerType_iAMS = "I-AMS";
        //public const string ControllerType_ONOFF = "CENTRALIZED ON-OFF CONTROLLER";
        //public const string ControllerType_CentralStationEZ = "CENTRAL STATION EZ";
        //public const string ControllerType_CentralStationDXSoftware = "CENTRAL STATION DX SOFTWARE";
        //public const string ControllerType_ConmuterizedCentralControllerSoftware = "CONMUTERIZED CENTRAL CONTROLLER SOFTWARE";
        //public const string ControllerType_BacnetAdapter = "BACNET ADAPTER";
        //public const string ControllerType_7DayTimer = "7-DAY TIMER";
        public static string Controller_TOUCH_MEMO
        {
            get
            {
                //if (Util.IsChinese())
                //    return "集中控制器";
                //else
                    return "Centralized  Controllers";
            }
        }

        public static string Controller_ONOFF_MEMO
        {
            get
            {
                //if (Util.IsChinese())
                //    return "简易on/off集中控制器";
                //else
                    return "Simple on/off control";
            }
        }

        public static string ControllerAssembly_ModbusHor_MEMO
        {
            get
            {
                //if (Util.IsChinese())
                //    return "Modbus接口板(侧出风)";
                //else
                    return "Modbus Interface Board(Horizontal)";
            }
        }

        public static string ControllerAssembly_ModbusTop_MEMO
        {
            get
            {
                //if (Util.IsChinese())
                //    return "Modbus接口板(上出风)";
                //else
                    return "Modbus Interface Board(Top)";
            }
        }

        public static string ControllerAssembly_BacnetHor_MEMO
        {
            get
            {
                //if (Util.IsChinese())
                //    return "Bacnet转换器(侧出风)";
                //else
                    return "Bacnet Converter(Horizontal)";
            }
        }

        public static string ControllerAssembly_BacnetTop_MEMO
        {
            get
            {
                //if (Util.IsChinese())
                //    return "Bacnet转换器(上出风)";
                //else
                    return "Bacnet Converter(Top)";
            }
        }

        #endregion

    }
}
