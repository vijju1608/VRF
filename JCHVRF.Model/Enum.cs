//********************************************************************
// 文件名: Enum.cs
// 描述: 定义 VRF 项目中使用的枚举类型
// 作者: clh
// 创建时间: 2011-10-10
// 修改历史: 
// 2012-04-01 整理结构
// 注意：枚举值不要再修改，否则会产生与老版本的兼容问题！！！（标记 2012-04-01）
//********************************************************************

namespace JCHVRF.Model
{
    #region 程序中的通用枚举值

    /// <summary>
    /// 枚举：VRF产品销售区域
    /// </summary>
    public enum RegionVRF { Asia, China, MiddleEast, NorthAmerica }

    /// <summary>
    /// 枚举：工况
    /// </summary>
    public enum WorkingCondition { T1, T3 }

    /// <summary>
    /// 枚举：对应程序中的 flag 值以及数据库中的 [DoorType] 字段
    /// --添加一个“Outdoor”，用于包括外机的Type值； update on 20111208 clh
    /// 新增"Exchanger" 全热交换机 by 20170710 xyj
    /// </summary>
    public enum IndoorType { Indoor, FreshAir, Outdoor, Exchanger}

    /// <summary>
    /// 枚举：房间楼层树 节点的分组名称
    /// </summary>
    public enum TreeNodeGroupName { PROJECT, FLOOR, ROOM }

    /// <summary>
    /// 枚举：连接管类型，气管、液管
    /// </summary>
    public enum PipeType { Gas, Liquid }

    /// <summary>
    /// 枚举：系统类型（增加新风机之后新增的属性）
    /// add on 20130722 增加非严格一对一新风机类型 NonStrictFreshAir
    /// </summary>
    public enum SystemType { OnlyIndoor, OnlyFreshAir, CompositeMode, OnlyFreshAirMulti } 

    /// <summary>
    /// 枚举：文件操作类型
    /// </summary>
    public enum OperaterType { NEW, OPEN, SAVE, IMPORT, EXPORT, MANAGE }

    /// <summary>
    /// 自动选择室外机时可能遇到的结果
    /// </summary>
    public enum SelectOutdoorResult
    {
        OK,
        Null,
        NotMatch
    }

    public enum ERRType { StatusBar, PopupWin }

    /// <summary>
    /// 配管布局，室外机相对室内机的高度位置类型，高于|同水平线|低于
    /// </summary>
    public enum PipingPositionType { Upper, SameLevel, Lower }

    #endregion

    #region Piping界面--AddFlow中节点的扩展属性枚举值（整理 on 20120330）
    /// <summary>
    /// AddFlow 中各个扩展节点的ToolTip属性值（需运用枚举与string的转换）
    /// </summary>
    public enum NodeType { OUT, IN, YP, CP, MyLink, FA, CHbox, MultiCHbox ,YP4};

    /// <summary>
    /// AddFlow 中各个扩展节点的Custom属性值（需运用枚举与string的转换）
    /// </summary>
    public enum NodeCustom { MyNodeOut, MyNodeIn, MyNodeYP, MyNodeCP, MyLink };

    /// <summary>
    /// 配管图布局类型
    /// </summary>
    public enum PipingLayoutTypes
    {
        /// <summary>
        /// 普通
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 二叉树
        /// </summary>
        BinaryTree = 1,
        /// <summary>
        /// 对称
        /// </summary>
        Symmetries = 2,
        /// <summary>
        /// Schema A
        /// </summary>
        SchemaA = 3
    }

    /// <summary>
    /// 配管题布局方向
    /// </summary>
    public enum PipingOrientation
    {
        Unknown,
        Left,
        Right,
        Up,
        Down
    }

    #endregion

    /// <summary>
    /// 水流速等级
    /// </summary>
    public enum FlowRateLevels
    {
        NA = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>
    /// 翻译Type定义（需运用枚举与string的转换）
    /// </summary
    public enum TransType
    {
        Indoor,
        Outdoor,
        IDU_Accessory,
        ODU_Accessory,
        Controller,
        Series
    }

    /// <summary>
    /// 管类型
    /// </summary>
    public enum PipeCombineType
    {
        /// <summary>
        /// 液管，气管 (Heat Pump)
        /// </summary>
        HP_L_G,
        /// <summary>
        /// 液管，气管 (Heat Recovery)(在CH Box后面)
        /// </summary>
        HR_L_G,
        /// <summary>
        /// 液管，低压气管 (Heat Recovery)(Cooling Only)
        /// </summary>
        HR_L_LG,
        /// <summary>
        /// 液管，低压气管，高压气管 (Heat Recovery)(在CH Box前面)
        /// </summary>
        HR_L_LG_HG,   
    }
}                                                                          