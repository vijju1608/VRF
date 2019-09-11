//********************************************************************
// 文件名: NameArray.cs
// 描述: 定义 VRF 项目中所有表格的列名数组，某些对应DataGridView控件的【Name】【DataPropertyName】【HeaderText】属性。
// 作者: clh
// 创建时间: 2011-10-10
// 修改历史: 
// 2012-04-01 整理结构
// 注意：对应的枚举值不要再修改，否则会产生与老版本的兼容问题！！！（标记 2012-04-01）
// 同窗口列表的【Name】属性不能重复，【DataPropertyName】允许重复
// 2013-05-06 更新代码 for New VRF
//********************************************************************
using JCHVRF.VRFMessage;
using JCHVRF.Model;
namespace JCHVRF.Const
{
    #region LoadIndex 管理窗口 OK
    public class NameArray_LoadIndex
    {
        // 【HeaderText】
        public static string[] LoadIndex_HeaderText
        {
            get
            {
                string[] s =
                {
                    ShowText.RoomType,
                    ShowText.CoolingIndex,
                    ShowText.HeatingIndex
                };
                return s;
            }
        }

        // 【DataPropertyName】
        public static string[] LoadIndex_DataName = { "RoomType", "COOLIndex", "HEATIndex","IsDefault" };
    }
    #endregion

    #region Indoor 相关 OK
    public class NameArray_Indoor
    {
        // tag: 已修改，20130506 clh for Main_Indoor
        #region RoomIndoor ,【DataPropertyName】
        public string[] RoomIndoor_DataName =
        {
            DataPropertyName.TYPEIMAGE,
            DataPropertyName.NO,    //编号，hide
            DataPropertyName.ROOMNAME,
            DataPropertyName.NAME,  //名称
            DataPropertyName.MODELFULL,
            DataPropertyName.MODELOPTION, // ModelNameAfterOption
            DataPropertyName.DB_C,
            DataPropertyName.CAPACITY_C,
            DataPropertyName.SENSIBLEHEAT,
            DataPropertyName.AIRFLOW,
            DataPropertyName.DB_H,
            DataPropertyName.CAPACITY_H,
            DataPropertyName.SYSNAME
        };
        #endregion

        #region RoomIndoor, 【Name】
        public string[] RoomIndoor_Name =
        {
            Name_Common.TypeImage, 
            Name_Common.NO,
            Name_Common.RoomName,
            Name_Common.Name,
            Name_Common.ModelFull, 
            Name_Common.ModelOption,
            Name_Common.DB_C,
            Name_Common.Capacity_C,
            Name_Common.SensibleHeat,
            Name_Common.AirFlow,
            Name_Common.DB_H,
            Name_Common.Capacity_H,
            Name_Common.SysName // SystemID
        };
        #endregion

        #region RoomIndoor, 【HeaderText】
        public string[] RoomIndoor_HeaderText
        {
            get
            {
                string ut_height = SystemSetting.UserSetting.unitsSetting.settingHeight;
                string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                string[] s ={
                                "",
                                ShowText.NO,
                                ShowText.Room,
                                ShowText.Name,
                                "",
                                ShowText.Model,
                                ShowText.DB_C + "\n(" + ut_temperature + ")",
                                ShowText.Capacity_C + "\n(" + ut_power + ")",
                                ShowText.SensibleHeat + "\n(" + ut_power + ")",
                                ShowText.AirFlow + "\n(" + ut_airflow + ")",
                                ShowText.DB_H + "\n(" + ut_temperature + ")",
                                ShowText.Capacity_H + "\n(" + ut_power + ")",
                                ""
                            };
                return s;
            }
        }
        #endregion

        // tag: 20130506 clh for Add_Indoor
        // Add SHF_Hi,SHF_Med, SHF_Lo in stdIndoor 20161111 by Yunxiao Lin
        #region StdIndoor, 【DataPropertyName】
        public string[] StdIndoor_DataName =
        {
            DataPropertyName.MODELFULL,
            "Model_York",
            "Model_Hitachi",
            "CoolCapacity", 
            "SensibleHeat",
            "AirFlow",
            "StaticPressure",
            "HeatCapacity",
            "FreshAir",
            "TypeImage"//,
            //"SHF_Hi",
            //"SHF_Med",
            //"SHF_Lo"
        };
        #endregion

        #region StdIndoor, 【Name】
        public string[] StdIndoor_Name =
        {
            Name_Common.StdModelFull,
            Name_Common.StdModelFull_York,
            Name_Common.StdModelFull_Hitachi,
            Name_Common.StdCapacity_C,
            Name_Common.StdSensibleHeat,
            Name_Common.StdAirFlow,
            Name_Common.StdStaticPressure,
            Name_Common.StdCapacity_H,
            Name_Common.StdFreshAir,
            Name_Common.TypeImage
            //Name_Common.SHF_Hi,
            //Name_Common.SHF_Med,
            //Name_Common.SHF_Lo
        };
        #endregion

        #region StdIndoor, 【HeaderText】
        public string[] StdIndoor_HeaderText 
        {
            get
            {
                string ut_height = SystemSetting.UserSetting.unitsSetting.settingHeight;
                string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                string ut_esp = SystemSetting.UserSetting.unitsSetting.settingESP;
                string[] s =
                {
                    ShowText.Model,
                    ShowText.Model,
                    ShowText.Model, 
                    ShowText.Capacity_C+ "\n(" + ut_power + ")",
                    ShowText.SensibleHeat+"\n(" + ut_power + ")",
                    ShowText.AirFlow+ "\n(" + ut_airflow + ")",
                    //ShowText.StaticPressure+"\n(Pa)",
                    ShowText.StaticPressure+"\n(" + ut_esp + ")",
                    ShowText.Capacity_H+ "\n(" + ut_power + ")",
                    ShowText.FreshAir+"\n(" + ut_airflow + ")",
                    ShowText.TypeImage
                };
                return s;
            }
        }
        #endregion

        //Add ProductType, SHF_Hi, SHF_Med, SHF_Lo in selIndoor 20161111 by Yunxiao Lin
        #region Select Indoor, 【DataPropertyName】
        public string[] SelIndoor_DataName =
        {
            DataPropertyName.NO,
            DataPropertyName.NAME,
            DataPropertyName.MODELFULL,
            DataPropertyName.MODELOPTION,
            "Model_York",
            "Model_Hitachi",
            DataPropertyName.COUNT,
            DataPropertyName.TYPE,
            DataPropertyName.CAPACITY_C,
            DataPropertyName.CAPACITY_H,
            DataPropertyName.SENSIBLEHEAT,
           //DataPropertyName.AIRFLOW,
            "ProductType"//,
            //"SHF_Hi",
            //"SHF_Med",
            //"SHF_Lo"
        };
        #endregion

        #region Select Indoor, 【Name】
        public string[] SelIndoor_Name = 
        {
            Name_Common.NO,
            Name_Common.Name,
            Name_Common.ModelFull,
            Name_Common.ModelFull_York,
            Name_Common.ModelFull_Hitachi,
            Name_Common.ModelOption,
            Name_Common.Count,
            Name_Common.Type,
            Name_Common.Capacity_C,
            Name_Common.Capacity_H,
            Name_Common.SensibleHeat,
            //Name_Common.AirFlow,
            Name_Common.ProductType,
            //Name_Common.SHF_Hi,
            //Name_Common.SHF_Med,
            //Name_Common.SHF_Lo
        };
        #endregion

        #region Select Indoor, 【HeaderText】
        public string[] SelIndoor_HeaderText = 
        {
            ShowText.NO,
            ShowText.Name,
            "",
            ShowText.Model,
            ShowText.Model,
            ShowText.Model,
            ShowText.Count
        };
        #endregion

        // tag: 20130506 clh for Available indoor units（Outdoor界面）
        #region Available indoor units，【DataPropertyName】
        public string[] AvailableIndoor_DataName = 
        {
            DataPropertyName.MODEL,
            DataPropertyName.FLOORNAME,
            DataPropertyName.ROOMNAME
        };
        #endregion

        #region Available indoor units，【Name】
        public string[] AvailableIndoor_Name = 
        {
            Name_Common.Model,
            Name_Common.FloorName,
            Name_Common.RoomName
        };
        #endregion

        #region Available indoor units，【HeaderText】
        public string[] AvailableIndoor_HeaderText = 
        {
            ShowText.Model,
            ShowText.FloorName,
            ShowText.RoomName
        };
        #endregion

        #region RoomIndoor not attached 【DataPropertyName】
        public string[] RoomIndoorNotAttached_DataName = 
        {
            DataPropertyName.MODELFULL,
            DataPropertyName.NAME,
            DataPropertyName.ROOMNAME
        };
        #endregion

        #region RoomIndoor not attached 【Name】
        public string[] RoomIndoorNotAttached_Name = 
        {
            Name_Common.ModelFull,
            Name_Common.Name,
            Name_Common.RoomName
        };
        #endregion

        #region RoomIndoor not attached 【Text】 列标题
        public string[] RoomIndoorNotAttached_HeaderText = 
        {
            ShowText.Model,
            ShowText.Name,
            ShowText.Room
        };
        #endregion

        #region SysInfo, 【HeaderText】
        public string[] SysInfo_HeaderText
        {
            get
            {
                string ut_height = SystemSetting.UserSetting.unitsSetting.settingHeight;
                string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                string[] s ={                
                                ShowText.RoomName,
                                ShowText.Reqd_Capacity_C+ "\n(" + ut_power + ")",
                                ShowText.Reqd_Capacity_H+ "\n(" + ut_power + ")",
                                ShowText.Reqd_Sensible + "\n(" + ut_power + ")",
                                ShowText.Name,
                                ShowText.Model,
                                ShowText.Actual_Capacity_C+ "\n(" + ut_power + ")",
                                ShowText.Actual_Capacity_H+ "\n(" + ut_power + ")",
                                ShowText.Actual_Sensible + "\n(" + ut_power + ")",
                                ShowText.DB_C + "\n(" + ut_temperature + ")",
                                ShowText.WB_C + "\n(" + ut_temperature + ")",
                                ShowText.DB_H + "\n(" + ut_temperature + ")",
                                ShowText.WB_H + "\n(" + ut_temperature + ")",
                                "RH"+ "\n(%)"
                            };
                return s;
            }
        }
        #endregion

    }
    #endregion

    #region Accessory 界面

    public class NameArray_Accessory
    {
        public string[] Accessory_DataName = 
        {
            Name_Common.Type,
            Name_Common.TypeDisplay,
            "Model_York",
            "Model_Hitachi"
        };

        public string[] Accessory_Name = 
        {
            Name_Common.Type,
            Name_Common.TypeDisplay,
            Name_Common.ModelFull_York,
            Name_Common.ModelFull_Hitachi,
            Name_Common.MaxNumber,
            Name_Common.IsDefault
        };

        public string[] Accessory_Name_Sel =
        {
            Name_Common.SelType,
            Name_Common.SelTypeDisplay,
            Name_Common.SelModel_York,
            Name_Common.SelModel_Hitachi,
            Name_Common.Count
        };

        public string[] Accessory_HeaderText = 
        {
            ShowText.Type,
            ShowText.Type,
            ShowText.Model,
            ShowText.Model,
            ShowText.MaxNumber,
            ShowText.Description
        };


        public string[] Accessory_HeaderText_Sel = 
        {
            ShowText.Type,
            ShowText.Type,
            ShowText.Model,
            ShowText.Model,
            ShowText.Description,
            ShowText.Count
          
        };
    }
    #endregion

    #region Report 文件
    public class NameArray_Report
    {
        // 对应Report中【1. Room Information】的列名
        #region ColNameArray_Room
        /// <summary>
        /// 对应Report中【1. Room Information】的列名
        /// </summary>
        public static string[] ColNameArray_Room = 
        {
            RptColName_Room.RoomNO,
            RptColName_Room.FloorNO,
            RptColName_Room.RoomName,
            RptColName_Room.RoomType,
            RptColName_Room.RoomArea,
            RptColName_Room.RLoad,
            RptColName_Room.RLoadHeat,
            RptColName_Room.RSensibleHeatLoad,
            RptColName_Room.RAirFlow
        };
        #endregion

        // 对应Report中【2.Indoor/Outdoor Selection】Indoor的列名
        #region ColNameArray_InSel
        /// <summary>
        /// 对应Report中【2.Indoor/Outdoor Selection】Indoor的列名
        /// </summary>
        public static string[] ColNameArray_InSel = 
        {
            RptColName_Room.RoomNO,
            RptColName_Room.RoomName,
            RptColName_Room.CoolingDB_WB,
            RptColName_Room.HeatingDB,
            RptColName_Room.RLoad,
            RptColName_Room.RLoadHeat, 
            RptColName_Room.RSensibleHeatLoad,
            RptColName_Room.RAirFlow,
            RptColName_Room.RStaticPressure,
            RptColName_Unit.IndoorName,
            Name_Common.Model,
            Name_Common.Qty,
            RptColName_Unit.ActualCapacity,
            RptColName_Unit.ActualCapacityHeat,
            RptColName_Unit.ActualSensibleCapacity,
            Name_Common.AirFlow,
            RptColName_Unit.StaticPressure,
            Name_Common.Remark
        };

        /// <summary>
        /// 对应Report中【2.Indoor/Outdoor Selection】FA的列名
        /// </summary>
        public static string[] ColNameArray_FASel = 
        {
            RptColName_Room.RoomNO,
            RptColName_Room.RoomName,
            RptColName_Room.NoOfPerson,
            RptColName_Room.CoolingDB_WB,
            RptColName_Room.HeatingDB,
            RptColName_Room.FAIndex,
            RptColName_Room.FreshAir,
            RptColName_Unit.IndoorName,
            Name_Common.Model,
            Name_Common.Qty,
            RptColName_Unit.ActualCapacity, //即Capacity
            RptColName_Unit.ActualCapacityHeat,
            Name_Common.AirFlow,
            Name_Common.Remark
        };
        #endregion

        // 对应Report中【2.Indoor/Outdoor Selection】Outdoor的列名
        #region ColNameArray_OutSel
        /// <summary>
        ///  对应Report中【2.Indoor/Outdoor Selection】Outdoor的列名
        /// </summary>
        public static string[] ColNameArray_OutSel =
        {
            Name_Common.Model,
            RptColName_Outdoor.HeatingDB_WB,
            RptColName_Outdoor.CoolingDB,
            RptColName_Unit.ActualCapacity,
            RptColName_Unit.ActualCapacityHeat,
            RptColName_Outdoor.AddRefrigerant,
            RptColName_Outdoor.CapacityRatio
        };
        #endregion

        // 对应 Report 中【3、Drawing】PipeSpec 统计表的列名
        #region PipeSpec，【Name】&【DataPropertyName】共用，PipeSpec_Name
        /// <summary>
        /// 连接管管径规格及长度汇总表的列名数组
        /// </summary>
        public static string[] PipeSpec_Name =
        {
            RptColName_PipeSpec.PipeSpec,    // 规格
            RptColName_PipeSpec.PipeType,    // 类别（气管 or 液管）
            Name_Common.Qty,       // 数量
            Name_Common.Length,   // 实际管长
            RptColName_PipeSpec.EqLength,    // 等效管长
            RptColName_PipeSpec.SysName      // 所属系统
        };
        #endregion

        // 对应Report中【6. Product List】(1)(2) Unit List列名
        #region ColNameArray_UnitList
        /// <summary>
        /// 对应Report中【6. Product List】（1）Unit List列名
        /// </summary>
        public static string[] ColNameArray_UnitList =
        {
            Name_Common.ModelFull,
            Name_Common.UnitType,
            Name_Common.Qty,
            RptColName_Unit.RatedCoolingCapacity,
            RptColName_Unit.RatedHeatingCapacity,
            Name_Common.PowerInput,
            Name_Common.AirFlow,
            Name_Common.Length,
            Name_Common.Width,
            Name_Common.Height,
            Name_Common.Weight
        };


        /// <summary>
        /// 对应Report中【6. Product List】（2）Unit List列名
        /// </summary>
        public static string[] ColNameArray_UnitList_Outdoor =
        {
            Name_Common.ModelFull,
            Name_Common.ProductType,
            Name_Common.Qty,
            RptColName_Unit.RatedCoolingCapacity,
            RptColName_Unit.RatedHeatingCapacity,
            Name_Common.PowerInput,
            Name_Common.MaxOperationPI,
            Name_Common.AirFlow,
            Name_Common.Length,
            Name_Common.Width,
            Name_Common.Height,
            Name_Common.Weight
        };
        #endregion

        // 对应Report中【4. Product List】(3) Standard Option B 列名
        #region ColNameArray_StdOptionListB
        public static string[] ColNameArray_StdOptionListB = 
        {
            Name_Common.ModelFull,
            Name_Common.Qty,
            RptColName_Option.AuxiliaryElectricHeater,
            RptColName_Option.DrainagePumpandExpansionDevice,
            RptColName_Option.Controller,
            RptColName_Option.UnitPanel,
            RptColName_Option.DuctTypeTIO2,
            RptColName_Option.OptionPower,
            RptColName_Option.OptionInsulation
        };
        #endregion

        // 对应Report中【5. Pricing Form】机组价格统计
        #region ColNameArray_UnitPrice
        /// <summary>
        /// 对应Report中【5. Pricing Form】机组价格统计
        /// </summary>
        public static string[] ColNameArray_UnitPrice =
        {
            RptColName_Unit.UnitName,
            Name_Common.ModelFull,
            Name_Common.Qty,
            Name_Common.UnitPrice,
            Name_Common.TotalPrice,
            Name_Common.Remark
        };
        #endregion

        // 对应Report中【5. Pricing Form】分歧管型号价格统计
        #region ColNameArray_JointKitPrice
        /// <summary>
        /// 对应Report中【5. Pricing Form】分歧管价格统计
        /// </summary>
        public static string[] ColNameArray_JointKitPrice =
        {
            Name_Common.ModelFull,
            Name_Common.Qty,
            Name_Common.UnitPrice,
            Name_Common.TotalPrice,
            Name_Common.Remark
        };
        #endregion
    }

    #endregion


    #region Controller

    public class NameArray_Controller
    {
        #region MaterialList ,【DataPropertyName】
        public string[] MaterialList_DataName =
        {
            DataPropertyName.MODEL,
            DataPropertyName.QUANTITY,
            DataPropertyName.DESCRIPTION
        };
        #endregion

        #region MaterialList ,【Name】
        public string[] MaterialList_Name =
        {
            Name_Common.Model,
            Name_Common.Qty,
            Name_Common.Description
        };
        #endregion

        #region MaterialList, 【HeaderText】
        public string[] MaterialList_HeaderText
        {
            get
            {
                string[] s ={
                                ShowText.Model,
                                ShowText.Qty,
                                ShowText.Description
                            };
                return s;
            }
        }
        #endregion
    }
    #endregion

    #region
   
    #endregion

}
