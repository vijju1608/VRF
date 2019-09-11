//********************************************************************
// 文件名: Project.cs
// 描述: 定义 VRF 项目中的项目类
// 作者: clh
// 创建时间: 2012-04-01
// 修改历史: 
// 2016-1-29 迁入JCHVRF
// 2016-2-2 增加 FactoryCode、BrandCode 属性
//********************************************************************

using JCHVRF.Model.New;
using System;
using System.Collections.Generic;
using NextGenModel = JCHVRF.Model.NextGen;

namespace JCHVRF.Model
{
    /// <summary>
    /// 全局项目对象，用于保存与 AECWorks 对应的项目属性
    /// </summary>
    public static class glProject
    {
        /// <summary>
        /// 当前项目在AECWorks数据库（Project.mdb）中的ID编号，取决于添加的顺序
        /// </summary>
        public static int ProjectID = 0;
        /// <summary>
        /// VRF产品在 AECWorks 中所属的ID号，此处是固定常量，305
        /// </summary>
        public const int ProductID = 306; //SVRF 为305
        public static string UnitName = "System";     // 系统名称
        public static string Name = "GlobalVRF";    // 工程名称?
        /// <summary>
        /// AECworks 的注册区域
        /// </summary>
        public static string RegionAEC = "";        // AECworks Region  "MiddleEast"

        public static ControllerLayoutType ControllerLayoutType = ControllerLayoutType.MODBUS;
    }

    [Serializable]
    public class Project : ModelBase
    {
        public static Project CurrentProject = null;
        public Project()
        {
            this._name = "Project 1";
            this.RegionCode = string.Empty;  // For default value
            this.SubRegionCode = string.Empty; // For default value
            this._altitude = 0;// Design conditions
            this._isCoolingModeEffective = true;
            this._isHeatingModeEffective = true;
            this._orderDate = DateTime.Now;
            this._deliveryRequiredDate = DateTime.Now;
            //this._version = MyConfig.Version;
            this._location = string.Empty;
            this._soldTo = string.Empty;
            this._shipTo = string.Empty;
            this._purchaseOrderNO = string.Empty;
            this._contractNO = string.Empty;
            this._salesOffice = string.Empty;
            this._salesEngineer = string.Empty;
            this._salesYINO = string.Empty;
            this._remarks = string.Empty;
            this._salesCompany = string.Empty;
            this._isRoomInfoChecked = true; // Is this needed.Its only needed for legacy.TODO remove this.

            // TODO do we need the following fields.
            this._isIndoorListChecked = true;
            this._isOutdoorListChecked = true;
            this._isOptionChecked = true;
            this._isPipingDiagramChecked = true;
            this._isWiringDiagramChecked = true;
            this._isControllerChecked = true;
            this._isExchangerChecked = true;
            this._isActualCapacityChecked = true;
            this._isNormalCapacityChecked = true;

            this._createDate = DateTime.Now;
            this._updateDate = DateTime.Now;

            //Part of Design Conditions.Maintained here for legacy.TODO to clean up later.
            this._enableAltitudeCorrectionFactor = false; //海拔修正系数默认关闭

            //View related.TODO check and clean
            this._centralControllerOK = true; //控制器选型是否完成默认为true，兼容旧项目 20160829 by Yunxiao Lin
        }
        public static Project GetProjectInstance
        {
            get
            {
                return CurrentProject;
            }
        }


        #region 公共成员

        public bool IsPerformingUndoRedo { get; set; }
        public string SelectedSystemID { get; set; }
        public List<Lassalle.WPF.Flow.Item> SystemDrawing { get; set; }
        //Legacy not going to be used.
        public List<SystemVRF> SystemList = new List<SystemVRF>();

        public static string CurrentSystemId=string.Empty;
        public static bool isSystemPropertyWiz = false;

        //VRF systems in the project
        public List<NextGenModel.SystemVRF> SystemListNextGen = new List<NextGenModel.SystemVRF>();
        public List<NextGenModel.SystemVRF> CanvasODUList = new List<NextGenModel.SystemVRF>();
        //central controller systems in the project
        public List<ControlSystem> ControlSystemList = new List<ControlSystem>();
        //heat exchanger systems in the project
        public List<SystemHeatExchanger> HeatExchangerSystems = new List<SystemHeatExchanger>();
        //heat exchanger system list

        //components part of systemvrf
        public List<RoomIndoor> RoomIndoorList = new List<RoomIndoor>();
        public List<FreshAirArea> FreshAirAreaList = new List<FreshAirArea>();
        public List<RoomIndoor> ExchangerList = new List<RoomIndoor>();
        public List<Floor> FloorList = new List<Floor>();
        public List<Room> RoomList = new List<Room>();

        // add 201409 clh legacy need to revisit.
        public List<ControlGroup> ControlGroupList = new List<ControlGroup>();
        public List<Controller> ControllerList = new List<Controller>();
        public List<ControllerAssembly> ControllerAssemblyList = new List<ControllerAssembly>();

        /// <summary>
        /// 项目数量，为1表示当前项目，>1时表示导入项目
        /// </summary>
        public static int ProjectCount = 1;
        #endregion

        #region 字段
        private string _name;
        /// <summary>
        /// 工程项目名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        private int _ClientId;
        public int ClientId
        {
            get { return _ClientId; }
            set { _ClientId = value; }
        }
        private int _CreatorId;
        public int CreatorId
        {
            get { return _CreatorId; }
            set { _CreatorId = value; }
        }

        private string _version;
        /// <summary>
        /// 项目文件版本号
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { this.SetValue(ref _version, value); }
        }

        private string _remarks;
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remarks
        {
            get { return _remarks; }
            set { this.SetValue(ref _remarks, value); }
        }

        #region 影响项目选型的相关属性
        private int _altitude;
        /// <summary>
        /// 海拔高度
        /// </summary>
        public int Altitude
        {
            get { return _altitude; }
            set { this.SetValue(ref _altitude, value); }
        }

        private bool _enableAltitudeCorrectionFactor;
        /// 是否启用海拔修正系数 add on 20160517 by Yunxiao Lin
        /// <summary>
        /// 是否启用海拔高度修正
        /// </summary>
        public bool EnableAltitudeCorrectionFactor
        {
            get { return _enableAltitudeCorrectionFactor; }
            set { this.SetValue(ref _enableAltitudeCorrectionFactor, value); }
        }

        private string _regionCode;
        /// <summary>
        /// JCHVRF 父区域Code
        /// </summary>
        public string RegionCode
        {
            get { return _regionCode; }
            set { this.SetValue(ref _regionCode, value); }
        }

        private string _subRegionCode;
        /// <summary>
        /// JCHVRF 子区域Code
        /// </summary>
        public string SubRegionCode
        {
            get { return _subRegionCode; }
            set { this.SetValue(ref _subRegionCode, value); }
        }

        private string _factoryCode;
        /// <summary>
        /// 工厂代码
        /// </summary>
        public string FactoryCode
        {
            get { return _factoryCode; }
            set { this.SetValue(ref _factoryCode, value); }
        }

        private string _brandCode;
        /// <summary>
        /// 名牌代码，Y|H
        /// </summary>
        public string BrandCode
        {
            get { return _brandCode; }
            set { this.SetValue(ref _brandCode, value); }
        }

        private string _WorkingCondition;
        /// <summary>
        /// 工况 T1 or T3
        /// </summary>
        public string WorkingCondition
        {
            get { return _WorkingCondition; }
            set { this.SetValue(ref _WorkingCondition, value); }
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

        private string _airSpeedType;
        /// <summary>
        /// 风速类型 （高、中风速<中东>）
        /// </summary>
        public string AirSpeedType
        {
            get { return _airSpeedType; }
            set { this.SetValue(ref _airSpeedType, value); }
        }

        private bool _isCoolingModeEffective;
        /// <summary>
        /// 制冷模式是否有效
        /// </summary>
        public bool IsCoolingModeEffective
        {
            get { return _isCoolingModeEffective; }
            set { this.SetValue(ref _isCoolingModeEffective, value); }
        }

        private bool _isHeatingModeEffective;
        /// <summary>
        /// 制热模式是否有效
        /// </summary>
        public bool IsHeatingModeEffective
        {
            get { return _isHeatingModeEffective; }
            set { this.SetValue(ref _isHeatingModeEffective, value); }
        }

        private bool _isBothMode;
        /// <summary>
        /// 制热模式是否有效
        /// </summary>
        public bool IsBothMode
        {
            get { return _isBothMode; }
            set { this.SetValue(ref _isBothMode, value); }
        }
        private string _projectUpdateDate;
        /// <summary>
        /// 项目修改日期
        /// </summary>
        public string ProjectUpdateDate
        {
            get { return _projectUpdateDate; }
            set { this.SetValue(ref _projectUpdateDate, value); }
        }

        private string _projectRevision;
        /// <summary>
        /// 项目修订
        /// </summary>
        public string ProjectRevision
        {
            get { return _projectRevision; }
            set { this.SetValue(ref _projectRevision, value); }
        }

        private bool _IsIduCapacityW;
        /// <summary>
        /// 项目修订
        /// </summary>
        public bool IsIduCapacityW
        {
            get { return _IsIduCapacityW; }
            set { _IsIduCapacityW = value; }
        }

        #endregion

        #region 销售及订单信息相关属性

        private string _customer;
        /// <summary>
        /// 客户名称
        /// </summary>
        public string Customer
        {
            get { return _customer; }
            set { this.SetValue(ref _customer, value); }
        }

        private string _soldTo;
        /// <summary>
        /// 
        /// </summary>
        public string SoldTo
        {
            get { return _soldTo; }
            set { this.SetValue(ref _soldTo, value); }
        }

        private string _shipTo;
        /// <summary>
        /// 运送地址
        /// </summary>
        public string ShipTo
        {
            get { return _shipTo; }
            set { this.SetValue(ref _shipTo, value); }
        }

        private string _contractNO;
        /// <summary>
        /// 合同编号
        /// </summary>
        public string ContractNO
        {
            get { return _contractNO; }
            set { this.SetValue(ref _contractNO, value); }
        }

        private string _salesOffice;
        /// <summary>
        /// 销售办公室
        /// </summary>
        public string SalesOffice
        {
            get { return _salesOffice; }
            set { this.SetValue(ref _salesOffice, value); }
        }

        private string _salesEngineer;
        /// <summary>
        /// 销售工程师
        /// </summary>
        public string SalesEngineer
        {
            get { return _salesEngineer; }
            set { this.SetValue(ref _salesEngineer, value); }
        }

        private string _salesYINO;
        /// <summary>
        /// Sales Engineer's YI NO.
        /// </summary>
        public string SalesYINO
        {
            get { return _salesYINO; }
            set { this.SetValue(ref _salesYINO, value); }
        }

        private string _purchaseOrderNO;
        /// <summary>
        /// 采购订单编号
        /// </summary>
        public string PurchaseOrderNO
        {
            get { return _purchaseOrderNO; }
            set { this.SetValue(ref _purchaseOrderNO, value); }
        }

        private DateTime _orderDate;
        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime OrderDate
        {
            get { return _orderDate; }
            set { this.SetValue(ref _orderDate, value); }
        }

        private DateTime _deliveryRequiredDate;
        /// <summary>
        /// 客户要求的运输日期
        /// </summary>
        public DateTime DeliveryRequiredDate
        {
            get { return _deliveryRequiredDate; }
            set { this.SetValue(ref _deliveryRequiredDate, value); }
        }

        private string _location;
        /// <summary>
        /// 工程地址
        /// </summary>
        public string Location
        {
            get { return _location; }
            set { this.SetValue(ref _location, value); }
        }

        private string _dearler;
        /// <summary>
        /// 经销商
        /// </summary>
        public string Dearler
        {
            get { return _dearler; }
            set { this.SetValue(ref _dearler, value); }
        }

        #endregion

        #region 其他属性
        //private bool _isDearler;
        ///// <summary>
        ///// 客户版
        ///// </summary>
        //public bool IsDearler
        //{
        //    get { return _isDearler; }
        //    set { this.SetValue(ref _isDearler, value); }
        //}

        //private bool _isSuperUser;
        ///// <summary>
        ///// 超级用户
        ///// </summary>
        //public bool IsSuperUser
        //{
        //    get { return _isSuperUser; }
        //    set { this.SetValue(ref _isSuperUser, value); }
        //}

        //private bool _isPriceValid;
        ///// <summary>
        ///// 价格有效
        ///// </summary>
        //public bool IsPriceValid
        //{
        //    get { return _isPriceValid; }
        //    set { this.SetValue(ref _isPriceValid, value); }
        //}

        private string _currency;
        /// <summary>
        /// 币别（人民币：RMB；其他：USD）
        /// </summary>
        public string Currency
        {
            get
            {
                //if (string.IsNullOrEmpty(_currency))
                //{
                //    if (Util.IsChinese())
                //        _currency = "RMB";
                //    else
                //        _currency = "USD";
                //}
                return _currency;
            }
            set { this.SetValue(ref _currency, value); }
        }

        private DateTime _MLPEffDate;
        /// <summary>
        /// 价格的版本日期
        /// </summary>
        public DateTime MLPEffDate
        {
            get { return _MLPEffDate; }
            set { this.SetValue(ref _MLPEffDate, value); }
        }

        private DateTime _createDate;
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate
        {
            get
            {
                return _createDate;
            }
            set { this.SetValue(ref _createDate, value); }
        }

        private DateTime _updateDate;
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime UpdateDate
        {
            get { return _updateDate; }
            set { this.SetValue(ref _updateDate, value); }
        }
        #endregion

        #region 指定报告中各个模块是否输出，对应于 Main_Report 界面 Report contents 部分的选项值

        private bool _isRoomInfoChecked;
        /// <summary>
        /// 是否输出房间记录信息
        /// </summary>
        public bool IsRoomInfoChecked
        {
            get { return _isRoomInfoChecked; }
            set { this.SetValue(ref _isRoomInfoChecked, value); }
        }

        private bool _isIndoorListChecked;
        /// <summary>
        /// 是否输出选择的室内机记录
        /// </summary>
        public bool IsIndoorListChecked
        {
            get { return _isIndoorListChecked; }
            set { this.SetValue(ref _isIndoorListChecked, value); }
        }

        private bool _isOptionChecked;
        /// <summary>
        /// 是否输出选配项设备记录
        /// </summary>
        public bool IsOptionChecked
        {
            get { return _isOptionChecked; }
            set { this.SetValue(ref _isOptionChecked, value); }
        }

        private bool _isOutdoorListChecked;
        /// <summary>
        /// 是否输出选择的室外机记录
        /// </summary>
        public bool IsOutdoorListChecked
        {
            get { return _isOutdoorListChecked; }
            set { this.SetValue(ref _isOutdoorListChecked, value); }
        }

        private bool _isPipingDiagramChecked;
        /// <summary>
        /// 是否输出系统的配管图
        /// </summary>
        public bool IsPipingDiagramChecked
        {
            get { return _isPipingDiagramChecked; }
            set { this.SetValue(ref _isPipingDiagramChecked, value); }
        }

        private bool _isWiringDiagramChecked;
        /// <summary>
        /// 是否输出系统的配管图
        /// </summary>
        public bool IsWiringDiagramChecked
        {
            get { return _isWiringDiagramChecked; }
            set { this.SetValue(ref _isWiringDiagramChecked, value); }
        }

        private bool _isControllerChecked;
        /// <summary>
        /// 是否输出控制器部分
        /// </summary>
        public bool IsControllerChecked
        {
            get { return _isControllerChecked; }
            set { this.SetValue(ref _isControllerChecked, value); }
        }

        private bool _isOptionPriceChecked;
        /// <summary>
        /// 是否输出Option价格表
        /// </summary>
        public bool IsOptionPriceChecked
        {
            get { return _isOptionPriceChecked; }
            set { this.SetValue(ref _isOptionPriceChecked, value); }
        }
      

        private bool _isActualCapacityChecked;
        public bool IsActualCapacityChecked
        {
            get { return _isActualCapacityChecked; }
            set { this.SetValue(ref _isActualCapacityChecked, value); }
        }

        private bool _isNormalCapacityChecked;
        public bool IsNormalCapacityChecked
        {
            get { return _isNormalCapacityChecked; }
            set { this.SetValue(ref _isNormalCapacityChecked, value); }
        }

        private bool _isExchangerChecked;
        public bool IsExchangerChecked
        {
            get { return _isExchangerChecked; }
            set { this.SetValue(ref _isExchangerChecked, value); }
        }
        #endregion

        #region EU特殊需求相关属性

        private string _salesCompany;
        /// <summary>
        /// 销售所属公司
        /// </summary>
        public string salesCompany
        {
            get { return _salesCompany; }
            set { this.SetValue(ref _salesCompany, value); }
        }

        private string _salesAddress;
        /// <summary>
        /// 销售地址
        /// </summary>
        public string salesAddress
        {
            get { return _salesAddress; }
            set { this.SetValue(ref _salesAddress, value); }
        }

        private string _salesPhoneNo;
        /// <summary>
        /// 销售手机号码
        /// </summary>
        public string salesPhoneNo
        {
            get { return _salesPhoneNo; }
            set { this.SetValue(ref _salesPhoneNo, value); }
        }

        private string _clientName;
        /// <summary>
        /// 客户名
        /// </summary>
        public string clientName
        {
            get { return _clientName; }
            set { this.SetValue(ref _clientName, value); }
        }

        private string _postCode;
        /// <summary>
        /// 客户邮编
        /// </summary>
        public string postCode
        {
            get { return _postCode; }
            set { this.SetValue(ref _postCode, value); }
        }

        private string _clientTel;
        /// <summary>
        /// 客户电话
        /// </summary>
        public string clientTel
        {
            get { return _clientTel; }
            set { this.SetValue(ref _clientTel, value); }
        }

        private string _clientMail;
        /// <summary>
        /// 客户邮箱
        /// </summary>
        public string clientMail
        {
            get { return _clientMail; }
            set { this.SetValue(ref _clientMail, value); }
        }
        #endregion


        // add 20140905 clh
        private ControllerLayoutType _controllerLayoutType;
        /// 控制器布局类型，Modbus|Bacnet
        /// <summary>
        /// 控制器布局类型，Modbus|Bacnet
        /// </summary>
        public ControllerLayoutType ControllerLayoutType
        {
            get { return _controllerLayoutType; }
            set { this.SetValue(ref _controllerLayoutType, value); }
        }

        #endregion

        private bool _centralControllerOK;
        /// 控制器选型是否完成，默认为true 20160829 by Yunxiao Lin
        /// <summary>
        /// 控制器选型是否完成
        /// </summary>
        public bool CentralControllerOK
        {
            get { return _centralControllerOK; }
            set { this.SetValue(ref _centralControllerOK, value); }
        }

        //Created by Shweta. Used in MainWindow->SaveProjectData
        private int _projectID;
        public int projectID
        {
            get { return _projectID; }
            set { this.SetValue(ref _projectID, value); }
        }
        private string _creatorName;

        public string CreatorName
        {
            get { return _creatorName; }
            set { this.SetValue(ref _creatorName, value); }
        }

        //SystemType
        #region 
        private string _systemType;
        public string SystemType
        {
            get { return _systemType; }
            set { this.SetValue(ref _systemType, value); }
        }
        private string _systemName;
        public string SystemName
        {
            get { return _systemName; }
            set { this.SetValue(ref _systemName, value); }
        }
        private int _floorcounts;
        public int FloorCount
        {
            get { return _floorcounts; }
            set { this.SetValue(ref _floorcounts, value); }
        }
        private bool _isRegular;
        public bool IsRegular
        {
            get { return _isRegular; }
            set { this.SetValue(ref _isRegular, value); }
        }

        private DesignCondition _designCondition;
        public DesignCondition DesignCondition
        {
            get { return _designCondition; }
            set { this.SetValue(ref _designCondition, value); }
        }
        #endregion


    }
}
