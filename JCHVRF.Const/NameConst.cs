//********************************************************************
// 文件名: NameConst.cs
// 描述: 定义 VRF 项目中DataGridView控件的【Name】以及【DataPropertyName】属性。
// 作者: clh
// 创建时间: 2012-04-01 整理
// 修改历史: 
//********************************************************************

using System.IO;

namespace JCHVRF.Const
{
    #region Name_Common
    public struct Name_Common
    {
        public const string NO = "NO";
        public const string Name = "Name";

        public const string RoomID = "RoomID";
        public const string RoomNo = "RoomNo";
        public const string RoomName = "RoomName";
        public const string FloorName = "FloorName";

        public const string Order = "Order";
        public const string Model = "Model";
        public const string Count = "Count";
        public const string ModelFull = "ModelFull";
        public const string ModelFull_York = "ModelFull_York";
        public const string ModelFull_Hitachi = "ModelFull_Hitachi";
        public const string ModelOption = "ModelOption";
        public const string DB_C = "DB_C";
        public const string DB_H = "DB_H";
        public const string Capacity_C = "Capacity_C";
        public const string Capacity_H = "Capacity_H";
        public const string SensibleHeat = "SensibleHeat";
        public const string AirFlow = "AirFlow";
        public const string Price = "Price";
        public const string TypeImage = "TypeImage";
        public const string Length = "Length";
        public const string Width = "Width";
        public const string Height = "Height";
        public const string Weight = "Weight";
        public const string Remark = "Remark";
        public const string StdModel = "StdModel";
        public const string StdCapacity_C = "StdCapacity_C";
        public const string StdCapacity_H = "StdCapacity_H";
        public const string StdSensibleHeat = "StdSensibleHeat";
        public const string StdAirFlow = "StdAirFlow";
        public const string StdPrice = "StdPrice";
        public const string StdModelFull = "StdModelFull";
        public const string StdModelFull_York = "StdModelFull_York";
        public const string StdModelFull_Hitachi = "StdModelFull_Hitachi";
        public const string SelModel_York = "SelModel_York";
        public const string SelModel_Hitachi = "SelModel_Hitachi";
        public const string SelType = "SelType";
        public const string SelTypeDisplay = "SelTypeDisplay";   //类别显示名称 add on 20180919 by Vince
        public const string SelMaxNumber = "SelMaxNumber";
        public const string SelIsDefault = "SelIsDefault";
        public const string MaxNumber = "MaxNumber";
        public const string IsDefault = "IsDefault";
        public const string StdStaticPressure = "StdStaticPressure";
        public const string StdFreshAir = "StdFreshAir";

        public const string IsExchanger = "IsExchanger"; //区分是Indoor 还是Echanger   add on 20171222 by xyj
        // Report
        public const string Qty = "Qty";
        public const string UnitPrice = "UnitPrice";
        public const string TotalPrice = "TotalPrice";
        public const string Flag = "Flag";// add on 20120514
        public const string UnitType = "UnitType";   //add on 20161019

        public const string Type = "Type"; //机组分类
        public const string TypeDisplay = "TypeDisplay";   //类别显示名称   add on 20180919 by Vince

        public const string CoolingDB = "CoolingDB"; 
        public const string CoolingWB = "CoolingWB"; 
        public const string HeatingDB = "HeatingDB"; 
        public const string HeatingWB = "HeatingWB";

        //新增--Alex 20170728
        public const string CoolingInletWater = "CoolingInletWater";
        public const string HeatingInletWater = "HeatingInletWater";


        public const string RH = "RH";

        public const string SysName = "SysName";

        public const string Description = "Description";

        // add on 20150619 clh，新增需求
        public const string PowerInput = "PowerInput";
        public const string MaxOperationPI = "MaxOperationPI";
        public const string ProductType = "ProductType";

        //shf add on 20161111 by Lingjia Qiu
        //public const string SHF_Hi = "SHF_Hi";
        //public const string SHF_Med = "SHF_Med";
        //public const string SHF_Lo = "SHF_Lo";
    }
    #endregion

    #region Outdoor 界面
    public struct Name_Outdoor
    {
        //Outdoor -- Name_Common
        public const string StdPic = "StdPic";
        public const string StdMaxIU = "StdMaxIU";
        //RoomUnite
        public const string CapacityIn = "CapacityIn";
        public const string RatCapacityIn = "RatCapacityIn";
        //RoomUniteFA
        public const string RoomNoFA = "RoomNoFA";
        public const string RoomNameFA = "RoomNameFA";
        public const string CapacityInFA = "CapacityInFA";
        public const string RatCapacityInFA = "RatCapacityInFA";
        public const string RoomIDFA = "RoomIDFA";
    }
    #endregion

    #region Piping 界面
    public struct Name_Piping
    {
        // Outdoor
        public const string ModelOut = "ModelOut";
        public const string RatCapacityOut = "RatCapacityOut";
        public const string CapacityOut = "CapacityOut";
        public const string ModelFullOut = "ModelFullOut";
        public const string TypeImageOut = "TypeImageOut";

        // Indoor
        public const string ModelIn = "ModelIn";
        public const string RatCapacityIn = "RatCapacityIn";
        public const string CapacityIn = "CapacityIn";
        public const string TypeImageIn = "TypeImageIn";

        // Node & Link 扩展属性名
        #region  Node & Link 扩展属性名，，不要修改！！
        // 注意：关系到AddFlow配管图XML文件内容，不要修改！！ on 20120331 clh
        // OUT、IN
        public const string csModelName = "ModelName";
        public const string csHeight = "Height";
        public const string csEstCapacity = "EstCapacity";
        public const string csIndoorOrder = "IndoorOrder"; // IN
        public const string csIndoorNO = "IndoorNO";
        public const string csRoomID = "RoomID";           // IN
        public const string csFlag = "Flag";   // IN - Indoor / FreshAir
        // YP、CP
        public const string csModelG = "ModelG";
        public const string csModelL = "ModelL";
        public const string csSumCapacity = "SumCapacity";
        public const string csPriceG = "PriceG";
        public const string csPriceL = "PriceL";

        public const string csChildCount = "ChildCount";
        // Link 连接管
        public const string csElbowQty = "ElbowQty";
        public const string csOilTrapQty = "OilTrapQty";
        public const string csLength = "Length";
        public const string csSpecG = "SpecG";
        public const string csSpecL = "SpecL";
        #endregion
    }
    #endregion

    #region Verification 界面
    public struct Name_Verif
    {
        //Indoor
        public const string Id = "Id";
        public const string RoomLoad = "RoomLoad";
        public const string ModelIn = "ModelIn";
        public const string ActCapacityIn = "ActCapacityIn";
        //Fresh Air
        public const string IdFA = "IdFA";
        public const string RoomAreaFA = "RoomAreaFA";
        public const string ModelFA = "ModelFA";
        public const string AirFlowFA = "AirFlowFA";
        public const string AirFlowDemandedFA = "AirFlowDemandedFA";
        public const string RemarkFA = "RemarkFA";
    }
    #endregion

    #region Option 界面
    public struct Name_Option
    {
        //Indoor
        public const string ModelIn = "ModelIn";
        public const string ModelOptionIn = "ModelOptionIn";
        //public const string RoomID = "RoomId"; 
        public const string OrderIn = "OrderIn";
        public const string ModelFullIn = "ModelFullIn";
        //add on 20120424 clh for phase2
        public const string IndoorType = "IndoorType";
    }
    #endregion

    #region 构造程序中间数据表的 [ColumnName]，即对应DataGridView控件的【DataPropertyName】属性

    public struct DataPropertyName
    {
        /// <summary>
        /// 室内机编号
        /// </summary>
        public const string NO = "NO";
        /// <summary>
        /// 室内机名称
        /// </summary>
        public const string NAME = "NAME";

        #region ColName_Room
        /// <summary>
        /// 工程名称
        /// </summary>
        public const string PROJECTNAME = "PROJECTNAME";
        /// <summary>
        /// 楼层索引
        /// </summary>
        public const string FLOORINDEX = "FLOORINDEX";
        /// <summary>
        /// 楼层名称
        /// </summary>
        public const string FLOORNAME = "FLOORNAME";
        /// <summary>
        /// 楼层高度
        /// </summary>
        public const string FLOORHEIGHT = "FLOORHEIGHT";
        /// <summary>
        /// 房间ID
        /// </summary>
        public const string ROOMID = "ROOMID";
        /// <summary>
        /// 房间编号
        /// </summary>
        public const string ROOMNO = "ROOMNO";
        /// <summary>
        /// 房间名称
        /// </summary>
        public const string ROOMNAME = "ROOMNAME";
        /// <summary>
        /// 房间类型
        /// </summary>
        public const string ROOMTYPE = "ROOMTYPE";
        /// <summary>
        /// 房间面积
        /// </summary>
        public const string ROOMAREA = "ROOMAREA";
        /// <summary>
        /// 房间冷指标 Load Index
        /// </summary>
        public const string ROOMLOADINDEX = "ROOMLOADINDEX";
        /// <summary>
        /// 房间冷负荷 Load
        /// </summary>
        public const string ROOMLOAD = "ROOMLOAD";
        /// <summary>
        /// LoadIndex是否有效
        /// </summary>
        public const string ROOMLOADINDEXENABLE = "ROOMLOADINDEXENABLE";
        /// <summary>
        /// 房间显热 Sensible Heat
        /// </summary>
        public const string ROOMSENSIBLEHEAT = "ROOMSENSIBLEHEAT";
        /// <summary>
        /// 房间显热是否有效
        /// </summary>
        public const string ROOMSENSIBLEHEATENABLE = "ROOMSENSIBLEHEATENABLE";
        /// <summary>
        /// 房间风量 AirFlow
        /// </summary>
        public const string ROOMAIRFLOW = "ROOMAIRFLOW";
        /// <summary>
        /// 房间风量是否有效
        /// </summary>
        public const string ROOMAIRFLOWENABLE = "ROOMAIRFLOWENABLE";
        /// <summary>
        /// 房间人数 People Number
        /// </summary>
        public const string ROOMPEOPLENUMBER = "ROOMPEOPLENUMBER";
        //add on 20120313 clh 
        /// <summary>
        /// 房间新风指标
        /// </summary>
        public const string ROOMFRESHAIRINDEX = "ROOMFRESHAIRINDEX";
        /// <summary>
        /// 房间新风指标FreshAirIndex是否有效
        /// </summary>
        public const string ROOMFRESHAIRINDEXENABLE = "ROOMFRESHAIRINDEXENABLE";
        /// <summary>
        /// 房间新风风量需求
        /// </summary>
        public const string ROOMFRESHAIR = "ROOMFRESHAIR";
        //end 20120313
        #endregion

        #region ColName_RoomIndoor
        /// <summary>
        /// 顺序
        /// </summary>
        public const string ORDER = "ORDER";
        /// <summary>
        /// 机器型号(SHORT)-标准
        /// </summary>
        public const string MODEL = "MODEL";

        public const string COUNT = "COUNT";
        /// <summary>
        /// 机器型号(FULL)-标准
        /// </summary>
        public const string MODELFULL = "MODELFULL";
        /// <summary>
        /// 经过Option选择之后的型号名（FULL）-非标
        /// add on 20111223 -clh
        /// </summary>
        public const string MODELOPTION = "MODELOPTION";
        /// <summary>
        /// 干球温度，制冷工况
        /// </summary>
        public const string DB_C = "DB_C";
        /// <summary>
        /// 干球温度，制热工况
        /// </summary>
        public const string DB_H = "DB_H";
        /// <summary>
        /// 估算容量（制冷量）
        /// </summary>
        public const string CAPACITY_C = "COOLCAPACITY";
        /// <summary>
        /// 制热容量
        /// </summary>
        public const string CAPACITY_H = "HEATCAPACITY";
        /// <summary>
        /// 额定容量
        /// </summary>
        public const string RATCAPACITY = "RATCAPACITY";
        /// <summary>
        /// 显热
        /// </summary>
        public const string SENSIBLEHEAT = "SENSIBLEHEAT";
        /// <summary>
        /// 风量
        /// </summary>
        public const string AIRFLOW = "AIRFLOW";
        /// <summary>
        /// 所属系统名称
        /// </summary>
        public const string SYSNAME = "SYSNAME";
        /// <summary>
        /// 室内机or新风机or室外机
        /// </summary>
        public const string FLAG = "FLAG";
        /// <summary>
        /// 产品类型
        /// </summary>
        public const string TYPE = "TYPE";
        /// <summary>
        /// 内机or外机的类型图片名
        /// add on 20120423 clh
        /// </summary>
        public const string TYPEIMAGE = "TYPEIMAGE";

        #endregion

        #region ColName_Outdoor
        /// <summary>
        /// 最大室内机连接数量
        /// </summary>
        public const string OUTMAXIU = "OUTMAXIU";
        /// <summary>
        /// 室外机价格，重复可省
        /// </summary>
        public const string PRICEOUT = "PRICEOUT";
        #endregion

        #region ColName_MyNode
        /// <summary>
        /// 节点类型（Outdoor Unit | Indoor Unit）
        /// </summary>
        public const string NODETYPE = "NODETYPE";
        /// <summary>
        /// 型号（OutdoorModel | IndoorModel）
        /// </summary>
        public const string MODELNAME = "MODELNAME";
        /// <summary>
        /// 高度
        /// </summary>
        public const string HEIGHT = "HEIGHT";
        #endregion

        #region ColName_MyLink
        /// <summary>
        /// 节点类型（Pipe）
        /// </summary>
        public const string LINKTYPE = "LINKTYPE";
        /// <summary>
        /// 弯头数
        /// </summary>
        public const string ELBOWQTY = "ELBOWQTY";
        /// <summary>
        /// 存油弯数
        /// </summary>
        public const string OILTRAPQTY = "OILTRAPQTY";
        /// <summary>
        /// 配管长度
        /// </summary>
        public const string LENGTH = "LENGTH";
        #endregion

        #region ColName_Material List
        /// <summary>
        /// 数量
        /// </summary>
        public const string QUANTITY = "QUANTITY";
        /// <summary>
        /// 描述
        /// </summary>
        public const string DESCRIPTION = "DESCRIPTION";
        #endregion
    }

    #endregion

    #region Report 表格 （Word模板）
    // Room属性列名
    #region RptColName_Room
    /// <summary>
    /// Room属性列名
    /// </summary>
    public struct RptColName_Room
    {
        /// <summary>
        /// 房间编号
        /// </summary>
        public const string RoomNO = "RoomNO";
        /// <summary>
        /// 楼层编号
        /// </summary>
        public const string FloorNO = "FloorNO";
        /// <summary>
        /// 
        /// </summary>
        public const string RoomName = "RoomName";
        /// <summary>
        /// 
        /// </summary>
        public const string RoomType = "RoomType";
        /// <summary>
        /// 
        /// </summary>
        public const string RoomArea = "RoomArea";
        /// <summary>
        /// 
        /// </summary>
        public const string RLoad = "RLoad";
        public const string RLoadHeat = "RLoadHeat";
        /// <summary>
        /// 
        /// </summary>
        public const string RSensibleHeatLoad = "RSensibleHeatLoad";
        /// <summary>
        /// 
        /// </summary>
        public const string RAirFlow = "RAirFlow";
        //add on 20120511 clh
        public const string NoOfPerson = "NoOfPerson";
        public const string FAIndex = "FAIndex";
        public const string FreshAir = "FreshAir";
        //add by axj 20170621 
        public const string CoolingDB_WB = "CoolingDB_WB";
        public const string HeatingDB = "HeatingDB";
        //add by axj 20170710 静压
        public const string RStaticPressure = "RStaticPressure";
    }
    #endregion

    // Indoor和Outdoor机组基本属性对应的列名
    #region RptColName_Unit
    /// <summary>
    /// Indoor和Outdoor机组基本属性对应的列名
    /// </summary>
    public struct RptColName_Unit
    {
        /// <summary>
        /// 机组名称
        /// </summary>
        public const string UnitName = "UnitName";
        /// <summary>
        /// 额定制冷容量
        /// </summary>
        public const string RatedCoolingCapacity = "RatedCoolingCapacity";
        /// <summary>
        /// 额定制热容量
        /// </summary>
        public const string RatedHeatingCapacity = "RatedHeatingCapacity";
        /// <summary>
        /// 实际容量
        /// </summary>
        public const string ActualCapacity = "ActualCapacity";
        public const string ActualCapacityHeat = "ActualCapacityHeat";
        /// <summary>
        /// 
        /// </summary>
        public const string ActualSensibleCapacity = "ActualSensibleCapacity";
        public const string StaticPressure = "StaticPressure";
        public const string IndoorName = "IndoorName";
    }
    #endregion

    // Outdoor单独拥有的属性列
    #region RptColName_Outdoor
    /// <summary>
    /// Outdoor单独拥有的属性列
    /// </summary>
    public struct RptColName_Outdoor
    {
        /// <summary>
        /// 
        /// </summary>
        public const string AddRefrigerant = "AddRefrigerant"; //追加制冷剂的量
        /// <summary>
        /// 
        /// </summary>
        public const string CapacityRatio = "CapacityRatio";   //配比率
        public const string HeatingDB_WB = "HeatingDB_WB";
        public const string CoolingDB = "CoolingDB";  
    }
    #endregion

    // 连接管管径规格及长度汇总表的列名
    #region RptColName_PipeSpec
    /// <summary>
    /// 连接管管径规格及长度汇总表的列名
    /// add on 20111214
    /// </summary>
    public struct RptColName_PipeSpec
    {
        /// <summary>
        /// 管径规格
        /// </summary>
        public const string PipeSpec = "PipeSpec";
        /// <summary>
        /// 管径类型
        /// </summary>
        public const string PipeType = "PipeType";
        /// <summary>
        /// 等效管长
        /// </summary>
        public const string EqLength = "EqLength";
        /// <summary>
        /// 所属系统名
        /// </summary>
        public const string SysName = "SysName";
    }
    #endregion

    //Option部分的字段列名
    #region RptColName_Option
    /// <summary>
    /// Option部分的字段列名
    /// </summary>
    public struct RptColName_Option
    {
        /*
        /// <summary>
        /// 
        /// </summary>
        public const string ReturnPlenum = "ReturnPlenum";
        /// <summary>
        /// 
        /// </summary>
        public const string PipingDirection = "PipingDirection";
        /// <summary>
        /// 
        /// </summary>
        public const string ExpansionDevice = "ExpansionDevice";
        /// <summary>
        /// 
        /// </summary>
        public const string SpecialOptionName = "SpecialOptionName";
        /// <summary>
        /// 
        /// </summary>
        public const string SpecialOptionModel = "SpecialOptionModel";
        /// <summary>
        /// 
        /// </summary>
        public const string SpecialOptionDescription = "SpecialOptionDescription";
        /// <summary>
        /// 
        /// </summary>
        public const string NonStdProductsName = "NonStdProductsName";
        /// <summary>
        /// 
        /// </summary>
        public const string NonStdProductsDescription = "NonStdProductsDescription";
         */

        /// <summary>
        /// AEH 电加热
        /// </summary>
        public const string AuxiliaryElectricHeater = "AuxiliaryElectricHeater";
        /// <summary>
        /// DPED 水泵 & 排水管
        /// </summary>
        public const string DrainagePumpandExpansionDevice = "DrainagePumpandExpansionDevice";
        /// <summary>
        /// CF 控制器 & 过滤网
        /// </summary>
        public const string Controller = "Controller";

        /// <summary>
        /// UP 面板, 20130922 clh
        /// </summary>
        public const string UnitPanel = "UnitPanel";
        /// <summary>
        /// TIO2, 20130922 clh
        /// </summary>
        public const string DuctTypeTIO2 = "DuctTypeTIO2";

        public const string OptionPower = "OptionPower";

        public const string OptionInsulation = "OptionInsulation";
    }
    #endregion

    #endregion
    #region Validation Message
    public struct ValidationMessage
    {
        public const string MorethanOneODU = "Outdoor units can not be more than 1";
        public const string AtleastOneODUAndOneIDU = "Please add atleast 1 indoor unit and 1 outdoor unit";
        public const string AtleastOneIDU = "Please add atleast 1 indoor unit";
        public const string AtleastOneODU = "Please add atleast 1 outdoor unit";
    }
    #endregion

    #region Paths
    public class Paths
    {
        public static readonly string JchTempDir = Path.Combine(Path.GetTempPath(), "JCHTemp");
        public static readonly string RecentFileList = "recentFilesList.txt";
    }
    #endregion Paths

}
