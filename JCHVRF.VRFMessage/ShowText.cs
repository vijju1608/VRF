//********************************************************************
// 文件名: ShowText.cs
// 描述: 定义 VRF 项目中DataGridView控件的【HeaderText】属性。
// 作者: clh
// 创建时间: 2012-01-03 
// 修改历史: 
//********************************************************************

namespace JCHVRF.VRFMessage  
{
    /// <summary>
    /// add on 20111230 -clh 
    /// 生成界面上DataGridView的列名文字、Lable控件显示文字
    /// ---根据项目相关属性自动切换（中英文、公英制、区域等）
    /// </summary>
    public class ShowText
    {
        /// <summary>
        /// 编号 | No.
        /// </summary>
        public static string NO
        {
            get
            {
                return Msg.GetResourceString("Grid_NO");
            }
        }

        /// <summary>
        /// 名称 | Name
        /// </summary>
        public static string Name
        {
            get 
            {
                return Msg.GetResourceString("Grid_Name");
            }
        }

        /// <summary>
        /// 编号 | Index
        /// </summary>
        public static string Index
        {
            get
            {
                return Msg.GetResourceString("Grid_Index");
            }
        }

        /// <summary>
        /// 顺序 | Order
        /// </summary>
        public static string Order
        {
            get
            {
                return Msg.GetResourceString("Grid_Order");
            }
        }

        /// <summary>
        /// 楼层名称 | Floor Name
        /// </summary>
        public static string FloorName
        {
            get
            {
                return Msg.GetResourceString("Grid_FloorName");
            }
        }
        /// <summary>
        /// 房间编号 | Room No
        /// </summary>
        public static string RoomNo
        {
            get
            {
                return Msg.GetResourceString("Grid_RoomNo");
            }
        }
        /// <summary>
        /// 房间名称 | Room Name
        /// </summary>
        public static string RoomName
        {
            get
            {
                return Msg.GetResourceString("Grid_RoomName");
            }
        }
        /// <summary>
        /// 房间 | Room
        /// </summary>
        public static string Room
        {
            get
            {
                return Msg.GetResourceString("Grid_Room");
            }
        }

        /// <summary>
        /// 位置类型
        /// </summary>
        public static string Position
        {
            get
            {
                return Msg.GetResourceString("Grid_Position");
            }
        }

        /// <summary>
        /// 高度差
        /// </summary>
        public static string HeightDifference
        {
            get
            {
                return Msg.GetResourceString("Grid_HeightDifference");
            }
        }

        /// <summary>
        /// 房间类型 | Room Type
        /// </summary>
        public static string RoomType
        {
            get
            {
                return Msg.GetResourceString("Grid_RoomType");
            }
        }

        /// <summary>
        /// 干球温度_制冷 | DB_C
        /// </summary>
        public static string DB_C
        {
            get
            {
                return Msg.GetResourceString("Grid_DB_C");
            }
        }


        /// <summary>
        /// 干球温度_制热 | DB_H
        /// </summary>
        public static string DB_H
        {
            get
            {
                return Msg.GetResourceString("Grid_DB_H");
            }
        }


        /// <summary>
        /// 制冷容量 | Capacity_C
        /// </summary>
        public static string Capacity_C
        {
            get
            {
                return Msg.GetResourceString("Grid_Capacity_C");
            }
        }
        /// <summary>
        /// 制热容量 | Capacity_H
        /// </summary>
        public static string Capacity_H
        {
            get
            {
                return Msg.GetResourceString("Grid_Capacity_H");
            }
        }

        /// <summary>
        /// 额定容量 | Rated Cooling/Heating Capacity (kW|Ton)
        /// </summary>
        public static string RatCapacity
        {
            get 
            {
                return Msg.GetResourceString("Grid_RatCapacity");
            }
        }

        /// <summary>
        /// 估算容量 | Estimated Cooling/Heating Capacity (kW|Ton)
        /// </summary>
        public static string EstCapacity
        {
            get 
            {
                return Msg.GetResourceString("Grid_EstCapacity");
            }
        }


        //deleted by Shen Junjie on 2018/4/26  不应该这么拼接，不同语言语序是不同的。
        ///// <summary>
        ///// 估算显热 | Estimated Sensible Heat ( UnitPower )
        ///// </summary>
        //public static string EstSensibleHeat
        //{
        //    get
        //    {
        //        return Msg.GetResourceString("Grid_EstSensibleHeat")+SensibleHeat;
        //    }
        //}


        /// <summary>
        /// 室内机实际容量 | Indoor Actual Capacity (kW|Ton)
        /// </summary>
        public static string IndoorActCapacity
        {
            get
            {
                return Msg.GetResourceString("Grid_IndoorActCapacity");
            }
        }

        /// <summary>
        /// 室外机实际容量 -- Outdoor Actual Capacity (kW|Ton)
        /// </summary>
        public static string OutdoorActCapacity
        {
            get
            {
                return Msg.GetResourceString("Grid_OutdoorActCapacity");
            }
        }
        /// <summary>
        /// 室外机最大容量 -- Outdoor Max Capacity (kW|Ton)
        /// --（ucVerification界面Label控件）
        /// </summary>
        public static string OutdoorMaxCapacity
        {
            get
            {
                return Msg.GetResourceString("Grid_OutdoorMaxCapacity");
            }
        }

        /// <summary>
        /// 机组价格 -- MLP (RMB|USD)
        /// </summary>
        public static string MLP
        {
            get 
            {
                return Msg.GetResourceString("Grid_MLP");
            }
        }


        /// <summary>
        /// 房间负荷 | Load (kW|Ton)
        /// </summary>
        public static string Load
        {
            get
            {
                return Msg.GetResourceString("Grid_Load");
            }
        }


        /// <summary>
        /// 显热 -- SensibleHeat (kW|Ton)
        /// </summary>
        public static string SensibleHeat
        {
            get
            {
                return Msg.GetResourceString("Grid_SensibleHeat");
            }
        }


        /// <summary>
        /// 风量 -- AirFlow (m³/h|cfm)
        /// </summary>
        public static string AirFlow
        {
            get 
            {
                return Msg.GetResourceString("Grid_AirFlow");
            }
        }

        /// <summary>
        /// 图片 -- TypeImage (pa|kpa)
        /// </summary>
        public static string TypeImage
        {
            get
            {
                return Msg.GetResourceString("Grid_TypeImage");
            }
        }

        /// <summary>
        /// 静压 -- StaticPressure (pa|kpa)
        /// </summary>
        public static string StaticPressure
        {
            get
            {
                return Msg.GetResourceString("Grid_StaticPressure");
            }
        }

        /// <summary>
        /// 新风量 -- FreshAir (m³/h|cfm)
        /// </summary>
        public static string FreshAir
        {
            get
            {
                return Msg.GetResourceString("Grid_FreshAir");
            }
        }


        /// <summary>
        /// 容量 -- Capacity
        /// </summary>
        public static string Capacity
        {
            get
            {
                return Msg.GetResourceString("Grid_Capacity");
            }
        }

        /// <summary>
        /// 型号 -- Model
        /// </summary>
        public static string Model
        {
            get
            {
                return Msg.GetResourceString("Grid_Model");
            }
        }

        public static string MaxNumber
        {
            get
            {
                return Msg.GetResourceString("Grid_MaxNumber");
            }
        }

        /// <summary>
        /// 数量 -- Count
        /// </summary>
        public static string Count
        {
            get
            {
                return Msg.GetResourceString("Grid_Count");
            }
        }

        /// <summary>
        /// 室内机型号 -- Indoor Unit Model
        /// </summary>
        public static string IndoorUnitModel
        {
            get 
            {
                return Msg.GetResourceString("Grid_IndoorUnitModel");
            }
        }

        /// <summary>
        /// 室外机型号 -- Outdoor Unit Model
        /// </summary>
        public static string OutdoorUnitModel
        {
            get 
            {
                return Msg.GetResourceString("Grid_OutdoorUnitModel");
            }
        }

        /// <summary>
        /// 机组图片 -- Drawing
        /// </summary>
        public static string Drawing
        {
            get
            {
                return Msg.GetResourceString("Grid_Drawing");
            }
        }


        /// <summary>
        /// 室外机最大的室内机连接数（Outdoor标准表列名） -- Max. No. of Indoor Unit
        /// </summary>
        public static string MaxIndoorUnitCount
        { 
            get
            {
                return Msg.GetResourceString("Grid_MaxIndoorUnitCount");
            }
        }
        /// <summary>
        /// 机组类型 | Type (for SelForm_RoomUnit)
        /// </summary>
        public static string DoorUnitType
        {
            get
            {
                return Msg.GetResourceString("Grid_DoorUnitType");
            }
        }
        /// <summary>
        /// 备注 | Remark
        /// </summary>
        public static string Remark
        { 
            get
            {
                return Msg.GetResourceString("Grid_Remark");
            }
        }
        /// <summary>
        /// 短型号 | Short Model Name
        /// </summary>
        public static string ShortModelName
        {
            get
            {
                return Msg.GetResourceString("Grid_ShortModelName");
            }
        }
        /// <summary>
        /// 完整型号 | Short Model Name
        /// </summary>
        public static string FullModelName
        {
            get
            {
                return Msg.GetResourceString("Grid_FullModelName");
            }
        }
        //add on 20120312 clh  FreshAir
        /// <summary>
        /// 新风机型号 -- Fresh Air Unit Model
        /// </summary>
        public static string FreshAirUnitModel
        {
            get
            {
                return Msg.GetResourceString("Grid_FreshAirUnitModel");
            }
        }
        /// <summary>
        /// 新风机风量 -- FreshAirFlow (m³/h | cfm)
        /// </summary>
        public static string FreshAirFlow
        {
            get
            {
                return Msg.GetResourceString("Grid_FreshAirUnitModel") + "()";
            }
        }
        //end 20120312

        //add on 20120406 clh  FreshAir
        /// <summary>
        /// 新风需求 | 
        /// </summary>
        public static string FreshAirDemanded
        {
            get
            {
                return Msg.GetResourceString("Grid_FreshAirDemanded") + "()";
            }
        }
        /// <summary>
        /// 新风区域 | Fresh Air Area
        /// </summary>
        public static string FreshAirArea
        {
            get
            {
                return Msg.GetResourceString("Grid_FreshAirArea");
            }
        }
        //end 20120406

        #region for Piping
        // add on 20120330 clh
        /// <summary>
        /// Piping界面--选中AddFlow中Outdoor节点时表格第一列内容
        /// </summary>
        public static string NodeTypeOut
        {
            get
            {
                return Msg.GetResourceString("Grid_NodeTypeOut");
            }
        }
        /// <summary>
        /// Piping界面--选中AddFlow中Indoor节点时表格第一列内容
        /// </summary>
        public static string NodeTypeIn
        {
            get
            {
                return Msg.GetResourceString("Grid_NodeTypeIn");
            }
        }
        /// <summary>
        /// Piping界面--选中AddFlow中FreshAir节点时表格第一列内容
        /// </summary>
        public static string NodeTypeFA
        {
            get
            {
                return Msg.GetResourceString("Grid_NodeTypeFA");
            }
        }
        /// <summary>
        /// Piping界面--选中AddFlow中 Link （连接管）时表格第一列内容
        /// </summary>
        public static string NodeTypeLink
        {
            get
            {
                return Msg.GetResourceString("Grid_NodeTypeLink");
            }
        }

        /// <summary>
        /// Type | 类型
        /// </summary>
        public static string Type
        {
            get
            {
                return Msg.GetResourceString("Grid_Type");
            }
        }

        /// <summary>
        /// Height | 高度
        /// </summary>
        public static string Height
        {
            get
            {
                return Msg.GetResourceString("Grid_Height");
            }
        }

        /// <summary>
        /// Height (m) | 高度 (米)
        /// </summary>
        public static string HeightWithUnit
        {
            get 
            {
                string unit = "";
                return Height + " (" + unit + ")";
            }
        }

        /// <summary>
        /// Elbow Qty | 弯头数
        /// </summary>
        public static string ElbowQty
        {
            get
            {
                return Msg.GetResourceString("Grid_ElbowQty");
            }
        }

        /// <summary>
        /// Oil Trap Qty | 存油弯数
        /// </summary>
        public static string OilTrapQty
        {
            get
            {
                return Msg.GetResourceString("Grid_OilTrapQty");
            }
        }
        
        /// <summary>
        /// Length | 长度
        /// </summary>
        public static string Length
        {
            get
            {
                return Msg.GetResourceString("Grid_Length");
            }
        }

        /// <summary>
        /// 等效长度 | Equivalent Length
        /// </summary>
        public static string EQLength
        {
            get
            {
                return Msg.GetResourceString("Grid_EQLength");
            }
        }

        /// <summary>
        /// Length (m) | 长度 (米)
        /// </summary>
        public static string LengthWithUnit
        {
            get { return Length + ""; }
        }

        /// <summary>
        /// Units Height | 机组高度
        /// </summary>
        public static string UnitsHeight
        {
            get
            {
                return Msg.GetResourceString("Grid_UnitsHeight");
            }
        }

        /// <summary>
        /// Pipes Length | 配管长度
        /// </summary>
        public static string PipesLength
        {
            get
            {
                return Msg.GetResourceString("Grid_PipesLength");
            }
        }

        #endregion

        #region for Phase3

        public static string Cooling
        {
            get 
            {
                return Msg.GetResourceString("Grid_Cooling");
            }
        }

        public static string Heating
        {
            get 
            {
                return Msg.GetResourceString("Grid_Heating");
            }
        }

        public static string Range
        {
            get
            {
                return Msg.GetResourceString("Grid_Range");
            }
        }

        /// <summary>
        /// Load Index List 列名
        /// </summary>
        public static string CoolingIndex
        {
            get
            {
                return Msg.GetResourceString("Grid_CoolingIndex");
            }
        }

        public static string HeatingIndex
        {
            get
            {
                return Msg.GetResourceString("Grid_HeatingIndex");
            }
        }

        public static string btnOK
        {
            get
            {
                return Msg.GetResourceString("Grid_btnOK");
            }
        }

        public static string btnCancel
        {
            get
            {
                return Msg.GetResourceString("Grid_btnCancel");
            }
        }

        public static string btnDelete
        {
            get
            {
                return Msg.GetResourceString("Grid_btnDelete");
            }
        }

        public static string btnSelectFiles
        {
            get
            {
                return Msg.GetResourceString("Grid_btnSelectFiles");
            }
        }

        /// <summary>
        /// for AddItemForm
        /// </summary>
        public static string lblAdd
        {
            get
            {
                return Msg.GetResourceString("Grid_lblAdd");
            }
        }

        /// <summary>
        ///  for AddItemForm
        /// </summary>
        public static string lblName
        {
            get
            {
                return Msg.GetResourceString("Grid_lblName");
            }
        }

        /// <summary>
        /// for AddItemForm--Add System
        /// </summary>
        public static string lblSystem
        {
            get
            {
                return Msg.GetResourceString("Grid_lblSystem");
            }
        }

        /// <summary>
        /// for AddItemForm--Add System
        /// </summary>
        public static string ProjectFile
        {
            get
            {
                return Msg.GetResourceString("Grid_ProjectFile");
            }
        }

        public static string Op_Open
        {
            get
            {
                return Msg.GetResourceString("Grid_Op_Open");
            }
        }

        public static string Op_Save
        {
            get
            {
                return Msg.GetResourceString("Grid_Op_Save");
            }
        }

        public static string Op_Manage
        {
            get
            {
                return Msg.GetResourceString("Grid_Op_Manage");
            }
        }

        public static string Op_Import
        {
            get
            {
                return Msg.GetResourceString("Grid_Op_Import");
            }
        }

        public static string Op_Export
        {
            get
            {
                return Msg.GetResourceString("Grid_Op_Export");
            }
        }

        /// <summary>
        /// 经销商 (Project 中几个属性的默认初始值)
        /// </summary>
        public static string DealerNameValue
        {
            get
            {
                return Msg.GetResourceString("Grid_DealerNameValue");
            }
        }

        public static string ProjectNameValue
        {
            get
            {
                return Msg.GetResourceString("Grid_ProjectNameValue");
            }
        }

        public static string FactoryValue
        {
            get
            {
                return Msg.GetResourceString("Grid_FactoryValue");
            }
        }

        // for Report Text
        /// <summary>
        /// 项目名称
        /// </summary>
        public static string ProjectName
        { 
            get
            {
                return Msg.GetResourceString("Grid_ProjectName");
            }
        }

        /// <summary>
        /// 地点
        /// </summary>
        public static string Location
        {
            get
            {
                return Msg.GetResourceString("Grid_Location");
            }
        }

        /// <summary>
        /// 客户名称
        /// </summary>
        public static string CustomerName
        {
            get
            {
                return Msg.GetResourceString("Grid_CustomerName");
            }
        }

        /// <summary>
        /// 销售到 | Sold To
        /// </summary>
        public static string SoldTo
        {
            get
            {
                return Msg.GetResourceString("Grid_SoldTo");
            }
        }

        /// <summary>
        /// 地区 add on 20120810
        /// </summary>
        public static string RegionVRF
        {
            get
            {
                return Msg.GetResourceString("Grid_RegionVRF");
            }
        }

        /// <summary>
        /// 销售工程师
        /// </summary>
        public static string SalesEngineerName
        {
            get
            {
                return Msg.GetResourceString("Grid_SalesEngineerName");
            }
        }

        /// <summary>
        /// SelectionMode
        /// </summary>
        public static string SelectionMode
        {
            get
            {
                return Msg.GetResourceString("Grid_SelectionMode");
            }
        }

        /// <summary>
        /// ProductType
        /// </summary>
        public static string ProductType
        {
            get
            {
                return Msg.GetResourceString("Grid_ProductType");
            }
        }

        /// <summary>
        /// IndoorDB
        /// </summary>
        public static string IndoorDB
        {
            get
            {
                return Msg.GetResourceString("Grid_IndoorDB");
            }
        }

        /// <summary>
        /// OutdoorDB
        /// </summary>
        public static string OutdoorDB
        {
            get
            {
                return Msg.GetResourceString("Grid_OutdoorDB");
            }
        }

        /// <summary>
        /// IndoorWB
        /// </summary>
        public static string IndoorWB
        {
            get
            {
                return Msg.GetResourceString("Grid_IndoorWB");
            }
        }

        /// <summary>
        /// 室外湿球温度
        /// </summary>
        public static string OutdoorWB
        {
            get
            {
                return Msg.GetResourceString("Grid_OutdoorWB");
            }
        }

        /// <summary>
        /// Diameter
        /// </summary>
        public static string Diameter
        {
            get
            {
                return Msg.GetResourceString("Grid_Diameter");
            }
        }

        /// <summary>
        /// 等效管长
        /// </summary>
        public static string EquivalentLength
        {
            get
            {
                return Msg.GetResourceString("Grid_EquivalentLength");
            }
        }

        /// <summary>
        /// 经销商
        /// </summary>
        public static string DealerName
        {
            get
            {
                return Msg.GetResourceString("Grid_DealerName");
            }
        }

        /// <summary>
        /// 工厂
        /// </summary>
        public static string Factory
        {
            get
            {
                return Msg.GetResourceString("Grid_Factory");
            }
        }

        /// <summary>
        /// 合同号
        /// </summary>
        public static string ContractNumber
        {
            get
            {
                return Msg.GetResourceString("Grid_ContractNumber");
            }
        }

        /// <summary>
        /// 货币类别
        /// </summary>
        public static string Currency
        {
            get
            {
                return Msg.GetResourceString("Grid_Currency");
            }
        }

        /// <summary>
        /// 版本日期 | Rev Date
        /// </summary>
        public static string RevDate
        {
            get
            {
                return Msg.GetResourceString("Grid_RevDate");
            }
        }

        /// <summary>
        /// 价格生效日期 | MLP Eff Date
        /// </summary>
        public static string MLPDate
        {
            get
            {
                return Msg.GetResourceString("Grid_MLPDate");
            }
        }

        /// <summary>
        /// Room 创建时，默认的Room Type值
        /// </summary>
        public static string DefaultRoomTypeValue
        {
            get
            {
                return Msg.GetResourceString("Grid_DefaultRoomTypeValue");
            }
        }

        /// <summary>
        /// 楼层拷贝时弹出框的标题
        /// </summary>
        public static string SelFormTitle_CopyFloor
        {
            get
            {
                return Msg.GetResourceString("Grid_SelFormTitle_CopyFloor");
            }
        }

        /// <summary>
        /// 房间拷贝时弹出框的标题
        /// </summary>
        public static string SelFormTitle_CopyRoom
        {
            get
            {
                return Msg.GetResourceString("Grid_SelFormTitle_CopyRoom");
            }
        }

        #endregion

        /// <summary>
        /// 室外机
        /// </summary>
        public static string OutdoorUnit
        {
            get
            {
                return Msg.GetResourceString("Grid_OutdoorUnit");
            }
        }

        /// <summary>
        /// 室内机
        /// </summary>
        public static string IndoorUnit
        {
            get
            {
                return Msg.GetResourceString("Grid_IndoorUnit");
            }
        }


        public static string Auto
        {
            get
            {
                return Msg.GetResourceString("Grid_Auto");
            }
        }

        public static string Manual
        {
            get
            {
                return Msg.GetResourceString("Grid_Manual");
            }
        }

        public static string Qty
        {
            get
            {
                return Msg.GetResourceString("Grid_Qty");
            }
        }

        public static string Description
        {
            get
            {
                return Msg.GetResourceString("Grid_Description");
            }
        }

        /// <summary>
        /// 系统信息表
        /// </summary>
        public static string SystemInfo
        {
            get
            {
                return Msg.GetResourceString("Grid_SystemInfo");
            }
        }

        /// <summary>
        /// 湿球温度_制冷 | WB_C
        /// </summary>
        public static string WB_C
        {
            get
            {
                return Msg.GetResourceString("Grid_WB_C");
            }
        }

        /// <summary>
        /// 湿球温度_制热 | WB_H
        /// </summary>
        public static string WB_H
        {
            get
            {
                return Msg.GetResourceString("Grid_WB_H");
            }
        }

        /// <summary>
        /// 需求容量_制冷 | Reqd_Capacity_C
        /// </summary>
        public static string Reqd_Capacity_C
        {
            get
            {
                return Msg.GetResourceString("Grid_Reqd_Capacity_C");
            }
        }

        /// <summary>
        /// 实际容量_制冷 | Actual_Capacity_C
        /// </summary>
        public static string Actual_Capacity_C
        {
            get
            {
                return Msg.GetResourceString("Grid_Actual_Capacity_C");
            }
        }

        /// <summary>
        /// 需求容量_制热 | Reqd_Capacity_H
        /// </summary>
        public static string Reqd_Capacity_H
        {
            get
            {
                return Msg.GetResourceString("Grid_Reqd_Capacity_H");
            }
        }

        /// <summary>
        /// 实际容量_制热 | Actual_Capacity_H
        /// </summary>
        public static string Actual_Capacity_H
        {
            get
            {
                return Msg.GetResourceString("Grid_Actual_Capacity_H");
            }
        }

        /// <summary>
        /// 需求容量_显热 | Reqd_Sensible
        /// </summary>
        public static string Reqd_Sensible
        {
            get
            {
                return Msg.GetResourceString("Grid_Reqd_Sensible");
            }
        }

        /// <summary>
        /// 实际容量_显热 | Actual_Sensible
        /// </summary>
        public static string Actual_Sensible
        {
            get
            {
                return Msg.GetResourceString("Grid_Actual_Sensible");
            }
        }


        /// <summary>
        /// 控制器 | Remote Control Switch
        /// </summary>
        public static string Accessory_RemoteControlSwitch
        {
            get
            {
                return Msg.GetResourceString("Grid_RemoteControlSwitch");
            }
        }

        /// <summary>
        /// 共享控制器 | Share Remote Controller
        /// </summary>
        public static string Accessory_ShareRemoteController
        {
            get
            {
                return Msg.GetResourceString("Grid_ShareRemoteController");
            }
        }

        /// <summary>
        /// 无线接收器 | Receiver Kit
        /// </summary>
        public static string Accessory_ReceiverKit
        {
            get
            {
                return Msg.GetResourceString("Grid_ReceiverKit");
            }
        }

        /// <summary>
        /// 面板 | Panel
        /// </summary>
        public static string Accessory_Panel
        {
            get
            {
                return Msg.GetResourceString("Grid_Panel");
            }
        }
        /// <summary>
        /// 过滤器 | Filter
        /// </summary>
        public static string Accessory_Filter
        {
            get
            {
                return Msg.GetResourceString("Grid_Filter");
            }
        }

        /// <summary>
        /// 过滤盒 | Filter Box
        /// </summary>
        public static string Accessory_FilterBox
        {
            get
            {
                return Msg.GetResourceString("Grid_FilterBox");
            }
        }

        /// <summary>
        /// 排水 | Drain-up
        /// </summary>
        public static string Accessory_Drainup
        {
            get
            {
                return Msg.GetResourceString("Grid_Drainup");
            }
        }

        /// <summary>
        /// 其他 | Others
        /// </summary>
        public static string Accessory_Others
        {
            get
            {
                return Msg.GetResourceString("Grid_Others");
            }
        }
    }
}