using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    /// <summary>
    /// 20170216 增加室外机水流速/风速属性 by Yunxiao Lin
    /// </summary>
    [Serializable]
    public class SystemVRF : ModelBase
    {
        #region 构造函数
        public SystemVRF(int number) : this()
        {
            this._no = number;
            //string namePrefix = SystemSetting.UserSetting.defaultSetting.outdoorName;
            //this._name = namePrefix + number.ToString();

            this._sysType = SystemType.OnlyIndoor;
            this._ratio = 0;
            this._maxRatio = 1;
            this._diversityFactor = 0.8; //update on 20141020 clh
            //this._dbCooling = SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB;
            //this._dbHeating = SystemSetting.UserSetting.defaultSetting.outdoorHeatingDB;
            //this._wbHeating = SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB;
            //this._pipeEquivalentLength = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
            //this._firstPipeLength = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
            //this._heightDiff = SystemSetting.UserSetting.pipingSetting.pipingHighDifference;
            //this._pipingCoefficient = SystemSetting.UserSetting.pipingSetting.pipingCorrectionFactor;
            //this._pipingPositionType = SystemSetting.UserSetting.pipingSetting.pipingPositionType;

            this._isExportToReport = true;
            this._isUpdated = false;
            this._isPipingOK = true;

            this._allowExceedRatio = false;

            this._flowRateLevel = FlowRateLevels.NA;
            this._coolingFlowRate = 0;
            this._heatingFlowRate = 0;
        }

        public SystemVRF()
        {
            //this._isPipingVertical = true; //默认垂直排布 add by Shen Junjie on 20170928
            this._isPipingVertical = false; //默认水平排布 add by Shen Junjie on 20171027
            //if (_controlGroupID == null) { _controlGroupID = new List<string>();}

        }
        #endregion

        #region 数据成员
        public Outdoor OutdoorItem = null;

        /// <summary>
        /// 室外机的系列 为了自动重新选型 需要Series  add by Shen Junjie on 2018/3/14
        /// </summary>
        private string _series = "";
        public string Series
        {
            get
            {
                _series = OutdoorItem == null ? _series : OutdoorItem.Series;
                return _series;
            }
            set
            {
                this.SetValue(ref _series, value);
            }
        }

        private OptionOutdoor _optionItem;
        /// <summary>
        /// 选配项对象
        /// </summary>
        public OptionOutdoor OptionItem
        {
            get { return _optionItem; }
            set { this.SetValue(ref _optionItem, value); }
        }

        // Lassalle.Flow.Items 未标记为 Serialized
        [NonSerialized]
        public MyNodeOut MyPipingNodeOut = null;

        public tmpMyNodeOut MyPipingNodeOutTemp = null;

        /// <summary>
        /// 背景图片节点 add by Shen Junjie on 20170802
        /// </summary>
        public tmpNodeBgImage MyPipingBuildingImageNodeTemp = null;
        /// <summary>
        /// 比例尺节点（用途：计算管长等等） add by Shen Junjie on 20171025
        /// </summary>
        public tmpNodePlottingScale MyPipingPlottingScaleNodeTemp = null;

        [NonSerialized]
        public WiringNodeOut MyWiringNodeOut = null;


        #endregion

        #region 字段
        private string _id;
        /// <summary>
        /// 系统ID
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { this.SetValue(ref _id, value); }
        }

        private int _no;
        /// <summary>
        /// 系统编号
        /// </summary>
        public int NO
        {
            get { return _no; }
            set { this.SetValue(ref _no, value); }
        }

        private string _name;
        /// <summary>
        /// 系统名 Out1...
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

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

        private double _diversityFactor;
        /// <summary>
        /// 同时使用系数
        /// </summary>
        public double DiversityFactor
        {
            get { return _diversityFactor; }
            set { this.SetValue(ref _diversityFactor, value); }
        }

        private double _pipeActualLength;
        /// <summary>
        /// 属性：最长实际管长
        /// </summary>
        public double PipeActualLength
        {
            get { return _pipeActualLength; }
            set { this.SetValue(ref _pipeActualLength, value); }
        }

        private double _pipeEquivalentLength;
        /// <summary>
        /// 属性：最长等效管长
        /// </summary>
        public double PipeEquivalentLength
        {
            get { return _pipeEquivalentLength; }
            set { this.SetValue(ref _pipeEquivalentLength, value); }
        }

        private double _firstPipeLength;
        /// <summary>
        /// 属性：第一分歧管到最远端室内机的距离
        /// </summary>
        public double FirstPipeLength
        {
            get { return _firstPipeLength; }
            set { this.SetValue(ref _firstPipeLength, value); }
        }

        private double _pipeEquivalentLengthbuff;
        /// <summary>
        /// 属性：最长等效管长缓存
        /// </summary>
        public double PipeEquivalentLengthbuff
        {
            get { return _pipeEquivalentLengthbuff; }
            set { this.SetValue(ref _pipeEquivalentLengthbuff, value); }
        }

        private double _firstPipeLengthbuff;
        /// <summary>
        /// 属性：第一分歧管到最远端室内机的距离缓存
        /// </summary>
        public double FirstPipeLengthbuff
        {
            get { return _firstPipeLengthbuff; }
            set { this.SetValue(ref _firstPipeLengthbuff, value); }
        }

        private double _heightDiff;
        /// <summary>
        /// 室内外机高度差，0表示SameLevel
        /// </summary>
        public double HeightDiff
        {
            get
            {
                if (this.PipingPositionType == Model.PipingPositionType.SameLevel)
                {
                    return 0;  //修复老项目SameLevel时值为5的情况 add by Shen Junjie on 2017/12/27
                }
                else
                {
                    return Math.Abs(_heightDiff);//修复老项目有负数的情况 add by Shen Junjie on 2017/12/27
                }
            }
            set { this.SetValue(ref _heightDiff, value); }
        }

        private PipingPositionType _pipingPositionType;
        /// <summary>
        /// 配管中室外机相对于室内机的高度类型，高于|同水平线|低于
        /// </summary>
        public PipingPositionType PipingPositionType
        {
            get { return _pipingPositionType; }
            set { this.SetValue(ref _pipingPositionType, value); }
        }

        private double _addRefrigeration;
        /// <summary>
        /// 该系统室外机需要追加的制冷剂量
        /// </summary>
        public double AddRefrigeration
        {
            get { return _addRefrigeration; }
            set { this.SetValue(ref _addRefrigeration, value); }
        }

        private double _pipingLengthFactor;
        /// <summary>
        /// 该系统的管长修正系数
        /// </summary>
        public double PipingLengthFactor
        {
            get { return _pipingLengthFactor; }
            set { this.SetValue(ref _pipingLengthFactor, value); }
        }

        private double _pipingLengthFactor_H;
        /// <summary>
        /// 该系统的Heating管长修正系数   add on 20180615 by LingjiaQiu
        /// </summary>
        public double PipingLengthFactor_H
        {
            get { return _pipingLengthFactor_H; }
            set { this.SetValue(ref _pipingLengthFactor_H, value); }
        }

        private string _selOutdoorType;
        /// <summary>
        /// 室外机的选型类型--ucOutdoor界面
        /// </summary>
        public string SelOutdoorType
        {
            get { return _selOutdoorType; }
            set { this.SetValue(ref _selOutdoorType, value); }
        }

        private double _dbCooling;
        /// <summary>
        /// 制冷工况，干球温度
        /// </summary>
        public double DBCooling
        {
            get { return _dbCooling; }
            set { this.SetValue(ref _dbCooling, value); }
        }


        private double _rhHeatling;
        /// <summary>
        /// 制冷工况，室外湿度
        /// </summary>
        public double RHHeating
        {
            get { return _rhHeatling; }
            set { this.SetValue(ref _rhHeatling, value); }
        }


        private double _dbHeating;
        /// <summary>
        /// 制热工况，干球温度
        /// </summary>
        public double DBHeating
        {
            get { return _dbHeating; }
            set { this.SetValue(ref _dbHeating, value); }
        }

        private double _wbHeating;
        /// <summary>
        /// 制热工况，湿球温度
        /// </summary>
        public double WBHeating
        {
            get { return _wbHeating; }
            set { this.SetValue(ref _wbHeating, value); }
        }

        #region add water source inlet water Temp. on 20160615 by Yunxiao Lin
        private double _iwCooling;
        /// <summary>
        /// 制冷工况，进水温度(水冷机专用)
        /// </summary>
        public double IWCooling
        {
            get { return _iwCooling; }
            set { this.SetValue(ref _iwCooling, value); }
        }

        private double _iwHeating;
        /// <summary>
        /// 制热工况，进水温度(水冷机专用)
        /// </summary>
        public double IWHeating
        {
            get { return _iwHeating; }
            set { this.SetValue(ref _iwHeating, value); }
        }
        #endregion

        private bool _isExportToReport;
        private bool _editreportapply;
        /// <summary>
        /// 是否输出到报告文件
        /// </summary>
        public bool IsExportToReport
        {
            get { return _isExportToReport; }
            set { this.SetValue(ref _isExportToReport, value); }
        }
        public bool editreportapply
        {
            get { return _editreportapply; }
            set { this.SetValue(ref _editreportapply, value); }
        }
        private double _ratio;
        /// <summary>
        /// 实际配比率
        /// </summary>
        public double Ratio
        {
            get { return _ratio; }
            set { this.SetValue(ref _ratio, value); }
        }

        private double _maxRatio;
        /// <summary>
        /// 最大配比率，50%~130%（0.5~1.3）
        /// </summary>
        public double MaxRatio
        {
            get { return _maxRatio; }
            set { this.SetValue(ref _maxRatio, value); }
        }

        // Add on 20140519  LingjiaQiu
        private double _ratioFA;
        /// <summary>
        /// 冷容量   
        /// </summary>
        public double RatioFA
        {
            get { return _ratioFA; }
            set { this.SetValue(ref _ratioFA, value); }
        }

        private SystemType _sysType;
        /// <summary>
        /// 系统类型，OnlyIndoor等等
        /// </summary>
        public SystemType SysType
        {
            get { return _sysType; }
            set { this.SetValue(ref _sysType, value); }
        }


        private bool _isPipingVertical;
        /// <summary>
        /// 配管图默认方向，是否竖直布局
        /// </summary>
        public bool IsPipingVertical
        {
            get { return _isPipingVertical; }
            set { this.SetValue(ref _isPipingVertical, value); }
        }

        /// <summary>
        /// 配管图布局类型
        /// </summary>
        public PipingLayoutTypes PipingLayoutType
        {
            get;
            set;
        }
        private bool isInputLengthManually;
        /// <summary>
        /// 是否手动输入配管长度，默认为false
        /// </summary>
        public bool IsInputLengthManually
        {
            get { return isInputLengthManually; }
            set { this.SetValue(ref isInputLengthManually, value); }
        }

        private double _MaxPipeLength;
        /// <summary>
        /// 系统允许的最大的配管实际长度，从标准室外机表直接获取
        /// </summary>
        public double MaxPipeLength
        {
            get { return _MaxPipeLength; }
            set { this.SetValue(ref _MaxPipeLength, value); }
        }

        private double _MaxEqPipeLength;
        /// <summary>
        /// 系统允许的最大的配管等效管长度，从标准室外机表直接获取
        /// </summary>
        public double MaxEqPipeLength
        {
            get { return _MaxEqPipeLength; }
            set { this.SetValue(ref _MaxEqPipeLength, value); }
        }


        private double _MaxOutdoorAboveHeight;
        /// <summary>
        /// 室外机在室内机上方时，系统允许的最大高度差，从标准室外机表直接获取
        /// </summary>
        public double MaxOutdoorAboveHeight
        {
            get { return _MaxOutdoorAboveHeight; }
            set { this.SetValue(ref _MaxOutdoorAboveHeight, value); }
        }

        private double _MaxOutdoorBelowHeight;
        /// <summary>
        /// 室外机在室内机下方时，系统允许的最大高度差，从标准室外机表直接获取
        /// </summary>
        public double MaxOutdoorBelowHeight
        {
            get { return _MaxOutdoorBelowHeight; }
            set { this.SetValue(ref _MaxOutdoorBelowHeight, value); }
        }

        private double _MaxDiffIndoorHeight;
        /// <summary>
        /// 系统允许的室内机最大高度差，从标准室外机表直接获取
        /// </summary>
        public double MaxDiffIndoorHeight
        {
            get { return _MaxDiffIndoorHeight; }
            set { this.SetValue(ref _MaxDiffIndoorHeight, value); }
        }

        private double _MaxIndoorLength;
        /// <summary>
        /// 系统允许的从第一分歧管到最远端室外机的最长距离，从标准室外机表直接获取
        /// </summary>
        public double MaxIndoorLength
        {
            get { return _MaxIndoorLength; }
            set { this.SetValue(ref _MaxIndoorLength, value); }
        }

        private double _MaxPipeLengthwithFA;
        /// <summary>
        /// 当系统中只存在新风机时，系统允许的实际最大配管长度，从标准室外机表直接获取
        /// </summary>
        public double MaxPipeLengthwithFA
        {
            get { return _MaxPipeLengthwithFA; }
            set { this.SetValue(ref _MaxPipeLengthwithFA, value); }
        }

        private double _MaxDiffIndoorLength;
        /// <summary>
        /// 一对一新风机组时，最大的等效果长度，从标准室外机表直接获取
        /// </summary>
        public double MaxDiffIndoorLength
        {
            get { return _MaxDiffIndoorLength; }
            set { this.SetValue(ref _MaxDiffIndoorLength, value); }
        }

        /// <summary>
        /// 一对一新风机组时，最大的等效果长度，从标准室外机表直接获取
        /// </summary>
        public double MaxFirstConnectionKitToEachODU
        {
            get { return 10; }
        }

        // Add on 20140519 clh
        private bool _isUpdated;
        /// 标记系统中的机组数量是否有更新
        /// <summary>
        /// 标记系统中的机组数量是否有更新
        /// </summary>
        public bool IsUpdated
        {
            get { return _isUpdated; }
            set { this.SetValue(ref _isUpdated, value); }
        }

        private bool _isPipingOK;
        /// 标记配管图是否完成
        /// <summary>
        /// 标记配管图是否完成
        /// </summary>
        public bool IsPipingOK
        {
            get { return _isPipingOK; }
            set { this.SetValue(ref _isPipingOK, value); }
        }

        /// 是否是手工绘制的配管图
        /// <summary>
        /// 是否是手工绘制的配管图
        /// </summary>
        public bool IsManualPiping
        {
            get;
            set;
        }

        // add on 20140911 clh, for Controller
        private List<string> _controlGroupID;
        /// 所属的Control Group
        /// <summary>
        /// 所属的Control Group
        /// </summary>
        public List<string> ControlGroupID
        {
            get { return _controlGroupID; }
        }

        // add on 20150130 clh,for 主界面绑定TreeNodeOutdoor自动选型时
        private bool _allowExceedRatio;
        /// <summary>
        /// 配比率是否允许超出100%，用于主界面绑定TreeNodeOutdoor自动选型时
        /// </summary>
        public bool AllowExceedRatio
        {
            get { return _allowExceedRatio; }
            set { this.SetValue(ref _allowExceedRatio, value); }
        }

        private bool _IDUFirst = false;
        /// 当前Actual Capacity不满足需求时，优先重选Indoor，默认为False Add on20161130 by Yunxiao Lin
        /// <summary>
        /// 当前Actual Capacity不满足需求时，优先重选Indoor，默认为False
        /// </summary>
        public bool IDUFirst
        {
            get { return _IDUFirst; }
            set { this.SetValue(ref _IDUFirst, value); }
        }

        private bool _isAuto = true;
        /// <summary>
        /// 是否是自动选型
        /// </summary>
        public bool IsAuto
        {
            get { return _isAuto; }
            set { this.SetValue(ref _isAuto, value); }
        }

        /// <summary>
        /// 系统功率
        /// </summary>
        public string Power
        {
            get;
            set;
        }


        #endregion

        private FlowRateLevels _flowRateLevel;
        /// <summary>
        /// 室外机水流速等级/风速等级 Lo, Med, Hi
        /// </summary>
        public FlowRateLevels FlowRateLevel
        {
            get { return _flowRateLevel; }
            set { this.SetValue(ref _flowRateLevel, value); }
        }

        private double _coolingFlowRate;
        /// <summary>
        /// 水流速(制冷)
        /// </summary>
        public double CoolingFlowRate
        {
            get { return _coolingFlowRate; }
            set { this.SetValue(ref _coolingFlowRate, value); }
        }

        private double _heatingFlowRate;
        /// <summary>
        /// 水流速(制热)
        /// </summary>
        public double HeatingFlowRate
        {
            get { return _heatingFlowRate; }
            set { this.SetValue(ref _heatingFlowRate, value); }
        }

        private double _maxTotalPipeLength;
        /// 液管总长上限，可以从std表获取，但是IVX系列会根据条件变化 20170702 by Yunxiao Lin
        /// <summary>
        /// 液管总长，可以从std表获取，但是IVX系列会根据条件变化
        /// </summary>
        public double MaxTotalPipeLength
        {
            get { return _maxTotalPipeLength; }
            set { this.SetValue(ref _maxTotalPipeLength, value); }
        }

        //private double _maxTotalPipelength_MaxIU;
        ///// 超出建议室内机数量时的液管总长上限，可以从std表获取，但是IVX系列会根据条件变化 20170704 by Yunxiao Lin
        ///// <summary>
        ///// 超出建议室内机数量时的液管总长上限，可以从std表获取，但是IVX系列会根据条件变化
        ///// </summary>
        //public double MaxTotalPipeLength_MaxIU
        //{
        //    get { return _maxTotalPipelength_MaxIU; }
        //    set { this.SetValue(ref _maxTotalPipelength_MaxIU, value); }
        //}

        private double _actualMaxMKIndoorPipeLength;
        /// 内机数量小于等于推荐值时，每个Multi_kit到每个IDU的最大长度(实际值) 20170704 by Shen Junjie
        /// <summary>
        /// 内机数量小于等于推荐值时，每个Multi_kit到每个IDU的最大长度(实际值)
        /// </summary>
        public double ActualMaxMKIndoorPipeLength
        {
            get { return _actualMaxMKIndoorPipeLength; }
            set { this.SetValue(ref _actualMaxMKIndoorPipeLength, value); }
        }

        private double _maxMKIndoorPipeLength;
        /// 内机数量小于等于推荐值时，每个Multi_kit到每个IDU的最大长度上限 20170704 by Yunxiao Lin
        /// <summary>
        /// 内机数量小于等于推荐值时，每个Multi_kit到每个IDU的最大长度上限
        /// </summary>
        public double MaxMKIndoorPipeLength
        {
            get { return _maxMKIndoorPipeLength; }
            set { this.SetValue(ref _maxMKIndoorPipeLength, value); }
        }

        //private double _maxMKIndoorPipeLength_MaxIU;
        ///// 内机数量大于推荐值小于等于允许最大值时，每个Multi_kit到每个IDU的最大长度上限 20170704 by Yunxiao Lin
        ///// <summary>
        ///// 内机数量大于推荐值小于等于允许最大值时，每个Multi_kit到每个IDU的最大长度上限
        ///// </summary>
        //public double MaxMKIndoorPipeLength_MaxIU
        //{
        //    get { return _maxMKIndoorPipeLength_MaxIU; }
        //    set { this.SetValue(ref _maxMKIndoorPipeLength_MaxIU, value); }
        //}

        private double _totalPipeLength;
        /// 系统液管总长度 20170704 by Yunxiao Lin
        /// <summary>
        /// 系统液管总长度
        /// </summary>
        public double TotalPipeLength
        {
            get { return _totalPipeLength; }
            set { this.SetValue(ref _totalPipeLength, value); }
        }

        private double _maxIndoorHeightDifferenceLength = 0.00;
        /// Maximum height difference between each Indoor unit 
        /// <summary>
        /// 每个室内单元之间的最大高度差 20180502 by xyj
        /// </summary>
        public double MaxIndoorHeightDifferenceLength
        {
            get { return _maxIndoorHeightDifferenceLength; }
            set { this.SetValue(ref _maxIndoorHeightDifferenceLength, value); }
        }


        private double _maxCHBoxHeghDiffLength = 0.00;
        /// height difference between CH-Boxes
        /// <summary>
        /// CHbox 之间的高度差 20180620 by xyj
        /// </summary>
        public double MaxCHBoxHighDiffLength
        {
            get { return _maxCHBoxHeghDiffLength; }
            set { this.SetValue(ref _maxCHBoxHeghDiffLength, value); }
        }


        private double _maxCHBox_IndoorHighDiffLength = 0.00;
        /// height difference between CH-Box and Indoor Units
        /// <summary>
        /// CHbox与Indoor 直接的高度差 20180620 by xyj
        /// </summary>
        public double MaxCHBox_IndoorHighDiffLength
        {
            get { return _maxCHBox_IndoorHighDiffLength; }
            set { this.SetValue(ref _maxCHBox_IndoorHighDiffLength, value); }
        }

        private double _maxSameCHBoxHighDiffLength = 0.00;
        /// height difference between  Indoor Units using the Same Branch of CH-Box 
        /// <summary>
        /// CHbox与Indoor 直接的高度差 20180620 by xyj
        /// </summary>
        public double MaxSameCHBoxHighDiffLength
        {
            get { return _maxSameCHBoxHighDiffLength; }
            set { this.SetValue(ref _maxSameCHBoxHighDiffLength, value); }
        }

        private double _normalCHBoxHighDiffLength = 15.00;
        /// height difference between CH-Boxes Normal
        /// <summary>
        /// CHbox与Indoor 直接的高度差 20180625 by xyj
        /// </summary>
        public double NormalCHBoxHighDiffLength
        {
            get { return _normalCHBoxHighDiffLength; }
            set { this.SetValue(ref _normalCHBoxHighDiffLength, value); }
        }


        private double _normalCHBox_IndoorHighDiffLength = 15.00;
        /// height difference between CH-Box and Indoor Units Normal
        /// <summary>
        /// CHbox与Indoor 直接的高度差 20180625 by xyj
        /// </summary>
        public double NormalCHBox_IndoorHighDiffLength
        {
            get { return _normalCHBox_IndoorHighDiffLength; }
            set { this.SetValue(ref _normalCHBox_IndoorHighDiffLength, value); }
        }

        private double _normalSameCHBoxHighDiffLength = 4.00;
        /// height difference between  Indoor Units using the Same Branch of CH-Box  Normal
        /// <summary>
        /// CHbox与Indoor 直接的高度差 20180625 by xyj
        /// </summary>
        public double NormalSameCHBoxHighDiffLength
        {
            get { return _normalSameCHBoxHighDiffLength; }
            set { this.SetValue(ref _normalSameCHBoxHighDiffLength, value); }
        }

        private double _maxUpperHeightDifferenceLength = 0.00;
        /// Maximum height difference between outdoor unit and  indoor units  
        /// <summary>
        /// 室外机和室内机之间的最大高度差(高) 20180502 by xyj
        /// </summary>
        public double MaxUpperHeightDifferenceLength
        {
            get { return _maxUpperHeightDifferenceLength; }
            set { this.SetValue(ref _maxUpperHeightDifferenceLength, value); }
        }


        private double _maxLowerHeightDifferenceLength = 0.00;
        /// Maximum height difference between outdoor unit and  indoor units  
        /// <summary>
        /// 室外机和室内机之间的最大高度差（低） 20180502 by xyj
        /// </summary>
        public double MaxLowerHeightDifferenceLength
        {
            get { return _maxLowerHeightDifferenceLength; }
            set { this.SetValue(ref _maxLowerHeightDifferenceLength, value); }
        }


        //add by Shen Junjie on 2018/02/11
        /// <summary>
        /// 老项目锁定标志。如果为true，则此系统不可更改 
        /// </summary>
        public bool Unmaintainable
        {
            set;
            get;
        }

        #region 方法

        /// 实时获取室外机中室内机的数量
        /// <summary>
        /// 实时获取室外机中室内机的数量
        /// </summary>
        /// <param name="thisProject"></param>
        public int GetIndoorCount(Project thisProject)
        {
            return thisProject.RoomIndoorList.Where(p => p.SystemID == this.Id).Count();
        }

        /// 将SystemVRF对象绑定到指定的ControlGroup对象
        /// <summary>
        /// 将SystemVRF对象绑定到指定的ControlGroup对象
        /// </summary>
        /// <param name="controlGroupID"></param>
        public void BindToControlGroup(string controlGroupID)
        {
            this._controlGroupID.Add(controlGroupID);
        }

        /// 计算并获取SystemVRF对象中所有室内机的估算容量总和 20160921 by Yunxiao Lin
        /// <summary>
        /// 计算并获取SystemVRF对象中所有室内机的估算容量总和
        /// </summary>
        /// <param name="thisProject"></param>
        /// <returns></returns>
        public double GetTotalIndoorEstCapacity(ref Project thisProject)
        {
            double capacity = 0;
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.IndoorItem != null && ri.SystemID == this._id)
                {
                    capacity += ri.CoolingCapacity;
                }
            }
            return capacity;
        }
        /// 计算并获取SystemVRF对象中所有室内机的标准容量总和 20161116 by Yunxiao Lin
        /// <summary>
        /// 计算并获取SystemVRF对象中所有室内机的标准容量总和
        /// </summary>
        /// <param name="thisProject"></param>
        /// <returns></returns>
        public double GetTotalIndoorRatedCapacity(ref Project thisProject)
        {
            double capacity = 0;
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.IndoorItem != null && ri.SystemID == this._id)
                {
                    capacity += ri.IndoorItem.CoolingCapacity;
                }
            }
            return capacity;
        }

        public void SetControlGroupID(List<string> value)
        {
            _controlGroupID = value;
        }

        /// 从另一个System中复制属性到当前System Add on 20161126 by Yunxiao Lin
        /// <summary>
        /// 从另一个System中复制属性到当前System
        /// </summary>
        /// <param name="sys"></param>
        public void Copy(SystemVRF sys)
        {
            this._addRefrigeration = sys._addRefrigeration;
            this._allowExceedRatio = sys._allowExceedRatio;
            this._controlGroupID = sys._controlGroupID;
            this._coolingCapacity = sys._coolingCapacity;
            this._dbCooling = sys._dbCooling;
            this._dbHeating = sys._dbHeating;
            this._rhHeatling = sys._rhHeatling;
            this._diversityFactor = sys._diversityFactor;
            this._firstPipeLength = sys._firstPipeLength;
            this._firstPipeLengthbuff = sys._firstPipeLengthbuff;
            this._heatingCapacity = sys._heatingCapacity;
            this._heightDiff = sys._heightDiff;
            this._isExportToReport = sys._isExportToReport;
            this._isPipingOK = sys._isPipingOK;
            this._isUpdated = sys._isUpdated;
            this._iwCooling = sys._iwCooling;
            this._iwHeating = sys._iwHeating;
            this._MaxDiffIndoorHeight = sys._MaxDiffIndoorHeight;
            this._MaxDiffIndoorLength = sys._MaxDiffIndoorLength;
            this._MaxEqPipeLength = sys._MaxEqPipeLength;
            this._MaxIndoorLength = sys._MaxIndoorLength;
            this._MaxOutdoorAboveHeight = sys._MaxOutdoorAboveHeight;
            this._MaxOutdoorBelowHeight = sys._MaxOutdoorBelowHeight;
            this._MaxPipeLength = sys._MaxPipeLength;
            this._MaxPipeLengthwithFA = sys._MaxPipeLengthwithFA;
            this._maxRatio = sys._maxRatio;
            this._name = sys._name;
            this._no = sys._no;
            this._optionItem = sys._optionItem;
            this._pipeActualLength = sys._pipeActualLength;
            this._pipeEquivalentLength = sys._pipeEquivalentLength;
            this._pipeEquivalentLengthbuff = sys._pipeEquivalentLengthbuff;
            this._pipingLengthFactor = sys._pipingLengthFactor;
            this._pipingPositionType = sys._pipingPositionType;
            this._ratio = sys._ratio;
            this._ratioFA = sys._ratioFA;
            this._selOutdoorType = sys._selOutdoorType;
            this._sysType = sys._sysType;
            this._wbHeating = sys._wbHeating;
            this.OutdoorItem = sys.OutdoorItem;
            this._IDUFirst = sys._IDUFirst;

            this._isAuto = sys._isAuto;
            this._coolingFlowRate = sys._coolingFlowRate;
            this._heatingFlowRate = sys._heatingFlowRate;
            this._flowRateLevel = sys._flowRateLevel;

            //添加新的属性  add by Shen Junjie on 2018/02/01
            this.MaxTotalPipeLength = sys.MaxTotalPipeLength;
            //this.MaxTotalPipeLength_MaxIU = sys.MaxTotalPipeLength_MaxIU;
            this.MaxMKIndoorPipeLength = sys.MaxMKIndoorPipeLength;
            //this.MaxMKIndoorPipeLength_MaxIU = sys.MaxMKIndoorPipeLength_MaxIU;
            //添加新的属性  add by xyj on 2018/06/25
            this.MaxSameCHBoxHighDiffLength = sys.MaxSameCHBoxHighDiffLength;
            this.MaxCHBoxHighDiffLength = sys.MaxCHBoxHighDiffLength;
            this.MaxCHBox_IndoorHighDiffLength = sys.MaxCHBox_IndoorHighDiffLength;
            this.MaxIndoorHeightDifferenceLength = sys.MaxIndoorHeightDifferenceLength;
            this.MaxLowerHeightDifferenceLength = sys.MaxLowerHeightDifferenceLength;
            this.MaxUpperHeightDifferenceLength = sys.MaxUpperHeightDifferenceLength;
            this.NormalCHBoxHighDiffLength = sys.NormalCHBoxHighDiffLength;
            this.NormalCHBox_IndoorHighDiffLength = sys.NormalCHBox_IndoorHighDiffLength;
            this.NormalSameCHBoxHighDiffLength = sys.NormalSameCHBoxHighDiffLength;
        }

        #endregion
    }
}
