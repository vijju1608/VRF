using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    [Serializable]
    public class RoomIndoor : ModelBase
    {
        #region 构造函数
        public RoomIndoor() { ControlGroupID = new List<string>(); }

        public RoomIndoor(int NO)
        {
            this.IndoorNO = NO;
            this._isFreshAirArea = false;
            this._isExchanger = false;

        }
        #endregion

        private string _selectedUnitType = null;
        /// <summary>
        /// 选择的室内机
        /// </summary>
        public string SelectedUnitType
        {
            get { return _selectedUnitType; }
            set { this.SetValue(ref _selectedUnitType, value); }
        }


        private Room _selectedRoom = null;
        /// <summary>
        /// 选择的室内机
        /// </summary>
        public Room SelectedRoom
        {
            get { return _selectedRoom; }
            set { this.SetValue(ref _selectedRoom, value); }
        }

        private Floor _selectedFloor = null;
        /// <summary>
        /// 选择的室内机
        /// </summary>
        public Floor SelectedFloor
        {
            get { return _selectedFloor; }
            set { this.SetValue(ref _selectedFloor, value); }
        }

        private Indoor _indoorItem = null;
        /// <summary>
        /// 选择的室内机
        /// </summary>
        public Indoor IndoorItem
        {
            get { return _indoorItem; }
            set { this.SetValue(ref _indoorItem, value); }
        }

        private OptionB _optionItem;
        /// <summary>
        /// 当前室内机选择的 Option
        /// </summary>
        public OptionB OptionItem
        {
            get
            {
                return _optionItem;
            }
            set { this.SetValue(ref _optionItem, value); }
        }

        #region 字段
        private string _roomID;
        /// <summary>
        /// 房间的ID，可以为空
        /// </summary>
        public string RoomID
        {
            get { return _roomID; }
            set { this.SetValue(ref _roomID, value); }
        }

        private string _roomName;
        /// <summary>
        /// 1001,1002 ……
        /// 改为RoomName，20141218需求（from Natallia）
        /// </summary>
        public string RoomName
        {
            get { return _roomName; }
            set { this.SetValue(ref _roomName, value); }
        }

        private int _indoorNO;
        /// <summary>
        /// 室内机的递增编号
        /// </summary>
        public int IndoorNO
        {
            get { return _indoorNO; }
            set { this.SetValue(ref _indoorNO, value); }
        }

        private string _indoorName;
        /// <summary>
        /// 室内机记录的名称，Ind1
        /// </summary>
        public string IndoorName
        {
            get { return _indoorName; }
            set { this.SetValue(ref _indoorName, value); }
        }

        #region add on 20130514 选型时保存的需求信息
        private double _rqCoolingCapacity;
        /// <summary>
        /// 制冷容量需求，基于房间时以房间数据为准
        /// </summary>
        public double RqCoolingCapacity
        {
            get { return _rqCoolingCapacity; }
            set { this.SetValue(ref _rqCoolingCapacity, value); }
        }

        private double _rqsensibleHeat;
        /// <summary>
        /// 显热需求，基于房间时以房间数据为准
        /// </summary>
        public double RqSensibleHeat
        {
            get { return _rqsensibleHeat; }
            set { this.SetValue(ref _rqsensibleHeat, value); }
        }

        private double _rqAirflow;
        /// <summary>
        /// 风量需求
        /// </summary>
        public double RqAirflow
        {
            get { return _rqAirflow; }
            set { this.SetValue(ref _rqAirflow, value); }
        }

        private double _rqStaticPressure;
        /// <summary>
        /// 静压需求
        /// </summary>
        public double RqStaticPressure
        {
            get { return _rqStaticPressure; }
            set { this.SetValue(ref _rqStaticPressure, value); }
        }

        private double _rqheatingCapacity;
        /// <summary>
        /// 制热容量需求，基于房间时以房间数据为准
        /// </summary>
        public double RqHeatingCapacity
        {
            get { return _rqheatingCapacity; }
            set { this.SetValue(ref _rqheatingCapacity, value); }
        }

        private double _rqFreshair;
        /// <summary>
        /// 新风风量需求
        /// </summary>
        public double RqFreshAir
        {
            get { return _rqFreshair; }
            set { this.SetValue(ref _rqFreshair, value); }
        }
        #endregion

        private double _coolingCapacity;
        /// <summary>
        /// 制冷估算容量 
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { this.SetValue(ref _coolingCapacity, value); }
        }

        private double _heatingCapacity;
        /// <summary>
        /// 制热估算容量
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { this.SetValue(ref _heatingCapacity, value); }
        }

        private double _sensibleHeat;
        /// <summary>
        /// 所选机器的估算显热
        /// </summary>
        public double SensibleHeat
        {
            get { return _sensibleHeat; }
            set { this.SetValue(ref _sensibleHeat, value); }
        }

        //private double _airFlow;
        ///// <summary>
        ///// 所选机器的估算风量
        ///// </summary>
        //public double AirFlow
        //{
        //    get { return _airFlow; }
        //    set { this.SetValue(ref _airFlow, value); }
        //}

        private string _systemID;
        /// <summary>
        /// 所属的系统ID
        /// </summary>
        public string SystemID
        {
            get { return _systemID; }
            set { this.SetValue(ref _systemID, value); }
        }

        private double _dbCooling;
        /// <summary>
        /// 制冷工况，室外干球温度
        /// </summary>
        public double DBCooling
        {
            get { return _dbCooling; }
            set { this.SetValue(ref _dbCooling, value); }
        }

        private double _rhCooling;
        /// <summary>
        /// 制冷工况，室外湿度
        /// </summary>
        public double RHCooling
        {
            get { return _rhCooling; }
            set { this.SetValue(ref _rhCooling, value); }
        }

        private double _wbCooling;
        /// <summary>
        /// 制冷工况，室内湿球温度
        /// </summary>
        public double WBCooling
        {
            get { return _wbCooling; }
            set { this.SetValue(ref _wbCooling, value); }
        }

        private double _dbHeating;
        /// <summary>
        /// 制热工况，室内干球温度
        /// </summary>
        public double DBHeating
        {
            get { return _dbHeating; }
            set { this.SetValue(ref _dbHeating, value); }
        }

        private double _wbHeating;
        /// <summary>
        /// 制热工况，室内干球温度
        /// </summary>
        public double WBHeating
        {
            get { return _wbHeating; }
            set { this.SetValue(ref _wbHeating, value); }
        }

        private bool _isDelete;
        /// <summary>
        /// 预删除标记，add on 20130510 clh for AddIndoor界面
        /// </summary>
        public bool IsDelete
        {
            get { return _isDelete; }
            set { this.SetValue(ref _isDelete, value); }
        }

        private bool _isAuto;
        /// <summary>
        /// 是否是自动选型
        /// </summary>
        public bool IsAuto
        {
            get { return _isAuto; }
            set { this.SetValue(ref _isAuto, value); }
        }

        private bool _isFreshAirArea;
        /// 是否是新风区域 add on 20160728 by Yunxiao Lin
        /// <summary>
        /// 是否是新风区域
        /// </summary>
        public bool IsFreshAirArea
        {
            get { return _isFreshAirArea; }
            set { this.SetValue(ref _isFreshAirArea, value); }
        }

        //是否是全热交换机 add on 2017-07-07 by xyj
        public bool _isExchanger;

        public bool IsExchanger
        {
            get { return _isExchanger; }
            set { this.SetValue(ref _isExchanger, value); }
        }

        /// 共享RemoteController室内机组   --add on 20170614 by Lingjia Qiu
        /// <summary>
        /// 共享RemoteController室内机组
        /// </summary>
        public List<RoomIndoor> IndoorItemGroup = null;

        //private string _indoorFullName;
        /// 室内机名   --add on 20170605 by Lingjia Qiu
        /// <summary>
        /// 室内机名
        /// </summary>
        public string IndoorFullName
        {
            get
            {
                bool isYork = false;
                if (Project.CurrentProject != null)
                {
                    isYork = Project.CurrentProject.BrandCode == "Y";
                }
                return IndoorName + "[" + (isYork ? IndoorItem.Model_York : IndoorItem.Model_Hitachi) + "]";
            }
            //set { this.SetValue(ref _indoorFullName, value); }
        }

        public string IndoorNodeName
        {
            get
            {
                bool isYork = false;
                if (Project.CurrentProject != null)
                {
                    isYork = Project.CurrentProject.BrandCode == "Y";
                }
                return (isYork ? IndoorItem.Model_York : IndoorItem.Model_Hitachi);
            }
            //set { this.SetValue(ref _indoorFullName, value); }
        }

        private bool _isMianIndoor = false;
        /// 共享RemoteController主室内机标示   --add on 20170614 by Lingjia Qiu
        /// <summary>
        /// 共享RemoteController主室内机标示
        /// </summary>
        public bool IsMainIndoor
        {
            get { return _isMianIndoor; }
            set { this.SetValue(ref _isMianIndoor, value); }
        }

        private List<string> _controlGroupID;
        /// <summary>
        /// 交换机关联Control
        /// </summary>
        public List<string> ControlGroupID
        {
            get { return _controlGroupID; }
            set { this.SetValue(ref _controlGroupID, value); }
        }

        #endregion

        private string _displayRoom;
        /// 房间名称   --add on 20170918 by xyj
        /// <summary>
        /// 房间名称
        /// </summary>
        public string DisplayRoom
        {
            get { return _displayRoom; }
            set { this.SetValue(ref _displayRoom, value); }
        }

        private string _dispalyImagePath;
        public string DisplayImagePath
        {
            get { return _dispalyImagePath; }
            set { this.SetValue(ref _dispalyImagePath, value); }
        }

        private string _dispalyImageName;
        public string DisplayImageName
        {
            get { return _dispalyImageName; }
            set { this.SetValue(ref _dispalyImageName, value); }
        }

        #region 方法

        // 将RoomIndoor对象分配给指定系统
        /// <summary>
        /// 将RoomIndoor对象分配给指定系统
        /// </summary>
        /// <param name="sysID"></param>
        public void SetToSystemVRF(string sysID)
        {
            this._systemID = sysID;
        }

        //private string _shf_mode = "";
        ///// SHF模式 add on20161109 by Yunxiao Lin
        ///// <summary>
        ///// SHF模式 High/Medium/Low
        ///// </summary>
        //public string SHF_Mode
        //{
        //    get { return _shf_mode; }
        //    set { this.SetValue(ref _shf_mode, value); }
        //}

        #region Fan Speed Related  -- add on 20170703 by Shen Junjie

        private int _FanSpeedLevel = -1;
        /// <summary>
        /// 风扇速度等级 -1:Max, 0:High2, 1:High, 2:Med, 3:Low
        /// </summary>
        public int FanSpeedLevel
        {
            get { return _FanSpeedLevel; }
            set { this.SetValue(ref _FanSpeedLevel, value); }
        }

        /// <summary>
        /// 所选机器的估算SHF (根据风扇速度获取)
        /// </summary>
        /// <param name="fanSpeedLevel"></param>
        /// <returns></returns>
        public double SHF
        {
            get
            {
                return this.IndoorItem.GetSHF(this.FanSpeedLevel);
            }
        }

        /// <summary>
        /// 所选机器的估算静压
        /// </summary>
        /// <param name="fanSpeedLevel"></param>
        /// <returns></returns>
        public double StaticPressure
        {
            get
            {
                //return this.IndoorItem.GetStaticPressure(this.FanSpeedLevel);
                return this.IndoorItem.GetStaticPressure(); //跟风扇速度脱钩 add by Shen Junjie on 20170927
            }
        }

        /// <summary>
        /// 所选机器的估算风量 (根据风扇速度获取)
        /// </summary>
        /// <param name="fanSpeedLevel"></param>
        /// <returns></returns>
        public double AirFlow
        {
            get
            {
                return this.IndoorItem.GetAirFlow(this.FanSpeedLevel);
            }
        }

        #endregion

        private double _actualCoolingCapacity;
        /// 室内机实际制冷容量， 由室外机实际制冷容量分配得到 20161110 by Yunxiao Lin
        /// <summary>
        /// 实际制冷容量
        /// </summary>
        public double ActualCoolingCapacity
        {
            get { return _actualCoolingCapacity; }
            set { this.SetValue(ref _actualCoolingCapacity, value); }
        }

        private double _actualHeatingCapacity;
        /// 室内机实际制热容量， 由室外机实际制热容量分配得到 20161110 by Yunxiao Lin
        /// <summary>
        /// 室内机的实际制热容量
        /// </summary>
        public double ActualHeatingCapacity
        {
            get { return _actualHeatingCapacity; }
            set { this.SetValue(ref _actualHeatingCapacity, value); }
        }

        private double _actualSensibleheat;
        /// 室内机的实际显热，由室内机的制冷容量乘以显热系数(SHF)得到 20161110 by Yunxiao Lin
        /// <summary>
        /// 室内机的实际显热
        /// </summary>
        public double ActualSensibleHeat
        {
            get { return _actualSensibleheat; }
            set { this.SetValue(ref _actualSensibleheat, value); }
        }


        private string _positionType = PipingPositionType.SameLevel.ToString();
        /// <summary>
        /// PositionType  室内机高度类型，高于|同水平线|低于
        /// </summary>
        public string PositionType
        {
            get { return _positionType; }
            set { this.SetValue(ref _positionType, value); }
        }



        private double _heightDiff = 0.00;
        /// <summary>
        /// HeightDiff  室内机高度差
        /// </summary>
        public double HeightDiff
        {
            get { return _heightDiff; }
            set { this.SetValue(ref _heightDiff, value); }
        }

        /// <summary>
        /// 附件列表 add by Shen Junjie on 2018/4/27
        /// </summary>
        public List<Accessory> ListAccessory = null;

        /// <summary>
        /// 设置Indoor，保留原Accessory
        /// </summary>
        public void SetIndoorItem(Indoor inItem)
        {
            //不能回传对象给Indoor.ListAccessory
            //可能会交叉污染RoomIndoor对象，因为RoomIndoor对象可能引用同一个Indoor对象 modify by Shen Junjie on 2018/5/11
            //if (inItem != null)
            //{
            //    inItem.ListAccessory = ListAccessory;  //统一用一个对象, 防止出问题
            //}
            IndoorItem = inItem;
        }

        /// <summary>
        /// 设置Indoor，并且替换Accessory列表
        /// </summary>
        public void SetIndoorItemWithAccessory(Indoor inItem)
        {
            if (inItem != null)
            {
                ListAccessory = inItem.ListAccessory;
            }
            IndoorItem = inItem;
        }



        private string _power;
        public string Power
        {
            get { return _power; }
            set { this.SetValue(ref _power, value); }
        }

        private double _area;
        public double Area
        {
            get { return _area; }
            set { this.SetValue(ref _area, value); }
        }

        private int _numberOfPeople;
        public int NumberOfPeople
        {
            get { return _numberOfPeople; }
            set { this.SetValue(ref _numberOfPeople, value); }
        }

        /* Properties created only for HE to handle multiple units on canvas  */
        private string _heCanvFreshAirUnit;
        public string HECanvFreshAirUnit
        {
            get { return _heCanvFreshAirUnit; }
            set { this.SetValue(ref _heCanvFreshAirUnit, value); }
        }

        private string _heCanvAreaUnit;
        public string HECanvAreaUnit
        {
            get { return _heCanvAreaUnit; }
            set { this.SetValue(ref _heCanvAreaUnit, value); }
        }

        private string _heCanTempUnit;
        public string HECanvTempUnit
        {
            get { return _heCanTempUnit; }
            set { this.SetValue(ref _heCanTempUnit, value); }
        }

        #endregion
    }
}
