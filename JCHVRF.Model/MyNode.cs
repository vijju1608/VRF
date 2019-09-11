using System;
using System.Collections.Generic;
using System.Text;
using Lassalle.Flow;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace JCHVRF.Model
{
    //public interface IInLink
    //{
    //    MyLink InLink { get; set; }
    //}
    
    public static class ObjectExtender
    {
        public static T GetValue<T>(this object obj, string name)
        {
            return (T)obj.GetType().GetProperty(name).GetValue(obj, null);
        }

        public static void SetValue<T>(this object obj, string name, T value)
        {
            obj.GetType().GetProperty(name).SetValue(obj, value, null);
        }
    }

    public interface ICoolingonly
    {
        bool IsCoolingonly { get; set; }
    }

    public interface ISingleChild<T>
    {
        T ChildNode { get; set; }
    }

    public interface IChildNodeArray<T>
    {
        T[] ChildNodes { get; set; }
    }

    public interface IChildNodeList<T>
    {
        List<T> ChildNodes { get; }
    }

    [Serializable]
    public class MyNode : Node
    {
        public MyNode()
        {
            MyInLinks = new List<MyLink>();
        }

        /// <summary>
        /// 管类型
        /// </summary>
        public PipeCombineType PipesType
        {
            get;
            set;
        }

        /// <summary>
        /// Parent node
        /// add by Junjie.SHEN on 2017/08/11
        /// </summary>
        public MyNode ParentNode { get; set; }

        // add by Shen Junjie on 2018/01/09
        /// <summary>
        /// 与父节点的连线
        /// </summary>
        public List<MyLink> MyInLinks
        {
            get;
            set;
        }

        //add by Shen Junjie on 2018/3/26
        /// <summary>
        /// 是否需要SizeUp
        /// </summary>
        public bool NeedSizeUP
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 配管图---室外机节点
    /// </summary>
    [Serializable]
    public class MyNodeOut : MyNode, ISingleChild<Node>
    {
        #region Methods
        // 设置子节点
        /// <summary>
        /// 设置子节点
        /// </summary>
        /// <param name="childNode"></param>
        public void SetChildNode(Node childNode)
        {
            this.ChildNode = childNode;
        }

        #endregion

        string name;
        /// <summary>
        /// Out1
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        string model;
        /// <summary>
        /// AuxModel
        /// </summary>
        public string Model
        {
            get { return model; }
            set { model = value; }
        }

        private Node _childNode;
        /// <summary>
        /// 子节点
        /// </summary>
        public Node ChildNode
        {
            get { return _childNode; }
            set
            {
                if (_childNode != null && _childNode is MyNode)
                {
                    MyNode orgChildNode = _childNode as MyNode;
                    if (orgChildNode.ParentNode == this)
                    {
                        orgChildNode.ParentNode = null;
                    }
                }
                if (value != null && value is MyNode)
                {
                    (value as MyNode).ParentNode = this;
                }
                _childNode = value;
            }
        }

        /// <summary>
        /// 室外机组合中机组数目
        /// </summary>
        public int UnitCount { get; set; }

        /// <summary>
        /// 室外机机组内部各室外机、分歧管之间的管长
        /// </summary>
        public double[] PipeLengthes { get; set; }
        
        /// <summary>
        /// 连接管管径，数量取决于UnitCount数值
        /// </summary>
        public string[] PipeSize { get; set; }
    }

    /// <summary>
    /// 配管图Piping---室内机节点; 兼容Wiring图中的室内机节点
    /// </summary>
    [Serializable]
    public class MyNodeIn : MyNode, ICoolingonly
    {
        private RoomIndoor _riItem;
        public RoomIndoor RoomIndooItem
        {
            get { return _riItem; }
            set { _riItem = value; }
        }

        //private MyLink _inLink;
        ///// <summary>
        ///// 室内机节点的连接线
        ///// </summary>
        //public MyLink InLink
        //{
        //    get { return _inLink; }
        //    set { _inLink = value; }
        //}

        //是否Cooling only 节点 by Yunxiao Lin 20160429
        /// <summary>
        /// 是否Cooling only 节点
        /// </summary>
        private bool _isCoolingonly;
        public bool IsCoolingonly
        {
            get { return _isCoolingonly; }
            set { _isCoolingonly = value; }
        }
    }

    /// <summary>
    /// 配管图---分歧管节点
    /// </summary>
    [Serializable]
    public class MyNodeYP : MyNode, ICoolingonly, IChildNodeArray<Node>
    {
        public const int MaxOutlinksCount = 2;
        public static int MaxOutlinksCountCP = 8;

        public MyNodeYP(bool isCP)
            : base()
        {
            this._coolingCapacity = 0;
            this._isCP = isCP;
            this._maxCount = isCP ? MaxOutlinksCountCP : MaxOutlinksCount;
            this._childCount = 0;
            this._isCoolingonly = false;

            if (_childNodes == null)
            {
                if (!isCP)
                    _childNodes = new Node[MaxOutlinksCount];
                else if (isCP)
                    _childNodes = new Node[MaxOutlinksCountCP];
            }
        }

        #region Methods
        /// 添加子节点, 默认添加在末尾
        /// <summary>
        /// 添加子节点, 默认添加在末尾
        /// </summary>
        /// <param name="childNode"></param>
        public bool AddChildNode(Node childNode)
        {
            return AddChildNode(childNode, _childCount);
        }

        /// 在指定位置插入子节点，指定位置开始的节点顺次后移一位
        /// <summary>
        /// 在指定位置插入子节点，指定位置开始的节点顺次后移一位
        /// </summary>
        /// <param name="childNode"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool AddChildNode(Node childNode, int index)
        {
            if (childNode == null) return false;
            if (_childCount < _maxCount && index < _maxCount)
            {
                for (int i = _maxCount - 1; i > index; --i)
                {
                    _childNodes[i] = _childNodes[i - 1];
                }
                _childNodes[index] = childNode; 

                ++_childCount;
                if (childNode is MyNode)
                {
                    (childNode as MyNode).ParentNode = this;
                }
                return true;
            }
            return false;
        }

        /// 添加子节点, 添加在空白位置
        /// <summary>
        /// 添加子节点, 添加在空白位置
        /// </summary>
        /// <param name="childNode"></param>
        public bool AddChildNodeManually(Node childNode)
        {
            if (childNode == null) return false;
            if (_childCount >= _maxCount) return false;
            int index = -1; 
            for (int i = 0; i < _maxCount; i++)
            {
                if (_childNodes[i] == null)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0) return false;
             
            _childNodes[index] = childNode;
            ++_childCount;
            if (childNode is MyNode)
            {
                (childNode as MyNode).ParentNode = this;
            }
            return true; 
        }

        /// 添加子节点, 添加在空白位置
        /// <summary>
        /// 添加子节点, 添加在空白位置
        /// </summary>
        /// <param name="childNode"></param>
        public bool AddChildNodeManually(Node childNode, int index)
        {
            if (childNode == null) return false;
            if (_childCount >= _maxCount) return false;
            if (index > _maxCount - 1) return false;
            if (index < 0)
            {
                for (int i = 0; i < _maxCount; i++)
                {
                    if (_childNodes[i] == null)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0) return false;
            }

            if (_childNodes[index] == null)
            {
                _childNodes[index] = childNode;
                ++_childCount;
                if (childNode is MyNode)
                {
                    (childNode as MyNode).ParentNode = this;
                } 
                return true;
            } 
            return false;
        }

        /// 移除某个子节点，其后的节点顺次前移一位
        /// <summary>
        /// 移除某个子节点，其后的节点顺次前移一位
        /// </summary>
        /// <param name="childNode"></param>
        /// <returns></returns>
        public bool RemoveChildNode(Node childNode)
        {
            int index = GetIndex(childNode);
            if (index == -1)
                return false;

            for (int i = index; i < _childCount; ++i)
            {
                if (i < _childCount - 1)
                    _childNodes[i] = _childNodes[i + 1];
                else
                    _childNodes[i] = null;
            }

            if (childNode != null && childNode is MyNode)
            {
                MyNode myChildNode = childNode as MyNode;
                if (myChildNode.ParentNode == this)
                {
                    myChildNode.ParentNode = null;
                }
            }
            --_childCount;
            return true;
        }

        /// 移除某个子节点，其他的节点不变
        /// <summary>
        /// 移除某个子节点，其他的节点不变
        /// </summary>
        /// <param name="childNode"></param>
        /// <returns></returns>
        public bool RemoveChildNodeManually(Node childNode)
        {
            for (int i = 0; i < _maxCount; ++i)
            {
                if (_childNodes[i] == childNode)
                {
                    if (childNode != null && childNode is MyNode)
                    {
                        MyNode myChildNode = childNode as MyNode;
                        if (myChildNode.ParentNode == this)
                        {
                            myChildNode.ParentNode = null;
                        }
                    }
                    _childNodes[i] = null;
                    --_childCount;
                    return true;
                }
            }
            return false;
        }

        /// 替换子节点
        /// <summary>
        /// 替换子节点
        /// </summary>
        /// <param name="oldNode"></param>
        /// <param name="newNode"></param>
        /// <returns></returns>
        public bool ReplaceChildNode(Node oldNode, Node newNode)
        {
            int index = GetIndex(oldNode);
            if (index == -1)
                return false;

            _childNodes[index] = newNode;
            if (oldNode != null && oldNode is MyNode)
            {
                MyNode myOldNode = oldNode as MyNode;
                if (myOldNode.ParentNode == this)
                {
                    myOldNode.ParentNode = null;
                }
            }
            if (newNode != null && newNode is MyNode)
            {
                MyNode myNewNode = newNode as MyNode;
                myNewNode.ParentNode = this;
            }
            return true;
        }

        /// 拖拽兄弟节点
        /// <summary>
        /// 拖拽兄弟节点
        /// </summary>
        /// <param name="sn"> 选中节点 </param>
        /// <param name="an"> 目标节点 </param>
        /// <returns></returns>
        public bool DragDropBrotherNodes(Node sn, Node an)
        {
            int snIndex = GetIndex(sn);
            int anIndex = GetIndex(an);

            if (snIndex == -1 || anIndex == -1)
                return false;

            RemoveChildNode(sn);
            AddChildNode(sn, anIndex);
            return true;
        }

        /// 获取指定节点的 索引值
        /// <summary>
        /// 获取指定节点的 索引值
        /// </summary>
        /// <param name="childNode"></param>
        /// <returns></returns>
        public int GetIndex(Node childNode)
        {
            for (int i = 0; i < _maxCount; ++i)
            {
                if (_childNodes[i] == childNode)
                    return i;
            }
            return -1;
        }
        #endregion

        #region Fields
        //private MyLink _inLink;
        ///// <summary>
        ///// YP节点的InLink
        ///// </summary>
        //public MyLink InLink
        //{
        //    get { return _inLink; }
        //    set { _inLink = value; }
        //}

        private Node[] _childNodes;
        /// <summary>
        /// 分歧管的子节点
        /// </summary>
        public Node[] ChildNodes
        {
            get { return _childNodes; }
            set
            {
                _childNodes = value;
                for (int i = 0; i < _maxCount; ++i)
                {
                    if (_childNodes[i] != null && _childNodes[i] is MyNode)
                    {
                        (_childNodes[i] as MyNode).ParentNode = this;
                    }
                }
                // 计算当前的 ChildCount 值
                //int maxCount = IsCP ? MaxOutlinksCountCP : MaxOutlinksCount;
                for (int i = 0; i < _maxCount; ++i)
                {
                    if (_childNodes[i] == null)
                    {
                        _childCount = i;
                        return;
                    }
                }
            }
        }

        private int _childCount = 0;
        /// <summary>
        /// 当前对象中的子节点数量，只读
        /// </summary>
        public int ChildCount
        {
            get { return _childCount; }
        }

        private int _maxCount = 0;
        /// <summary>
        /// 最大子节点数量，只读
        /// </summary>
        public int MaxCount
        {
            get { return _maxCount; }
        }

        private bool _isCP;
        /// <summary>
        /// 是否是梳型管，只读
        /// </summary>
        public bool IsCP
        {
            get { return _isCP; }
        }

        //private bool _isFirstYP;
        /// add on 20160317 clh 是否是第一分歧管
        /// <summary>
        /// 是否是第一分歧管
        /// </summary>
        public bool IsFirstYP
        {
            get { return ParentNode is MyNodeOut; }
            //set { _isFirstYP = value; }
        }

        private double _coolingCapacity;
        /// <summary>
        /// 下游室内机估算容量总和
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { _coolingCapacity = value; }
        }
        
        private double _heatingCapacity;
        /// <summary>
        /// 下游室内机估算制热容量总和
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { _heatingCapacity = value; }
        }

        private double _horsePower;
        /// <summary>
        /// 下游室内机估算马力总和
        /// </summary>
        public double HorsePower
        {
            get { return _horsePower; }
            set { _horsePower = value; }
        }

        private string _model;
        /// <summary>
        /// 分歧管型号（JCHVRF）
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private string _priceG;
        /// <summary>
        /// 分歧管价格
        /// </summary>
        public string PriceG
        {
            get { return _priceG; }
            set { _priceG = value; }
        }

        // add on 20160429 by Yunxiao Lin
        /// <summary>
        /// 是否Cooling only 节点
        /// </summary>
        private bool _isCoolingonly;
        public bool IsCoolingonly
        {
            get { return _isCoolingonly; }
            set { _isCoolingonly = value; }
        }
        #endregion
    }

    /// <summary>
    /// 配管图--CH-BOX 兼容wiring
    /// </summary>
    public class MyNodeCH : MyNode, ISingleChild<Node>
    {
        public MyNodeCH()
        {
        }

        //private MyLink _inLink;
        ///// <summary>
        ///// 节点的InLink
        ///// </summary>
        //public MyLink InLink
        //{
        //    get { return _inLink; }
        //    set { _inLink = value; }
        //}

        private Node _childNode;
        /// <summary>
        /// 子节点(MyNodeIn / MyNodeYP(含CP))
        /// </summary>
        public Node ChildNode
        {
            get { return _childNode; }
            set
            {
                if (_childNode != null && _childNode is MyNode)
                {
                    MyNode myChildNode = _childNode as MyNode;
                    if (myChildNode.ParentNode == this)
                    {
                        myChildNode.ParentNode = null;
                    }
                }
                if (value != null && value is MyNode)
                {
                    (value as MyNode).ParentNode = this;
                }
                _childNode = value;
            }
        }

        private double _coolingCapacity;
        /// <summary>
        /// 下游室内机估算容量总和
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { _coolingCapacity = value; }
        }

        private double _heatingCapacity;
        /// <summary>
        /// 下游室内机估算制热容量总和
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { _heatingCapacity = value; }
        }

        private double _horsePower;
        /// <summary>
        /// 下游室内机估算马力总和
        /// </summary>
        public double HorsePower
        {
            get { return _horsePower; }
            set { _horsePower = value; }
        }

        private string _model;
        /// <summary>
        /// CHbox型号
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        #region 增加CH-Box下端所有管长总和限制 add on 20160515 by Yunxiao Lin

        private double _actualTotalCHIndoorPipeLength;
        /// <summary>
        /// CH-Box下端所有管长总和(实际值)
        /// </summary>
        public double ActualTotalCHIndoorPipeLength
        {
            get { return _actualTotalCHIndoorPipeLength; }
            set { _actualTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength;
        /// <summary>
        /// 当系统中室内机数量没有超过推荐数量，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength
        {
            get { return _maxTotalCHIndoorPipeLength; }
            set { _maxTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength_MaxIU;
        /// <summary>
        /// 当系统中室内机数量超过推荐数量，没有超过允许最大数量时，CH-Box下端所有管长总和限制
        /// comment by Shen Junjie on 2018/9/22 此字段是冗余字段，没有实际意义
        /// </summary>
        public double MaxTotalCHIndoorPipeLength_MaxIU
        {
            get { return _maxTotalCHIndoorPipeLength_MaxIU; }
            set { _maxTotalCHIndoorPipeLength_MaxIU = value; }
        }
        private int _maxIndoorCount;
        /// <summary>
        /// CH-Box允许连接的最大室内机数量
        /// </summary>
        public int MaxIndoorCount
        {
            get { return _maxIndoorCount; }
            set { _maxIndoorCount = value; }
        }

        /// 电源 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源
        /// </summary>
        public string PowerSupply { get; set; }

        /// 电源线形 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源线形
        /// </summary>
        public string PowerLineType { get; set; }

        /// 电流 add by Shen Junjie on 2018/4/12
        /// <summary>
        /// 电流
        /// </summary>
        public double PowerCurrent { get; set; } 

        /// <summary>
        /// CHBox 与室外机之间高度差 add by xyj on 20180619
        /// </summary>
        public double HeightDiff { get; set; } 

        //add by Shen Junjie on 2018/6/15
        /// <summary>
        /// 电源功耗
        /// </summary>
        public double PowerConsumption { get; set; } 
        #endregion
    }

    /// <summary>
    /// 配管图--CH-BOX 兼容wiring
    /// </summary>
    public class MyNodeMultiCH : MyNode, IChildNodeList<Node>
    {
        public MyNodeMultiCH()
            : base()
        {
            _childNodes = new List<Node>();
        }

        //private MyLink _inLink;
        ///// <summary>
        ///// 节点的InLink
        ///// </summary>
        //public MyLink InLink
        //{
        //    get { return _inLink; }
        //    set { _inLink = value; }
        //}

        private List<Node> _childNodes;
        /// <summary>
        /// 布线时的子节点列表
        /// </summary>
        public List<Node> ChildNodes
        {
            get { return _childNodes; }
        }

        public void AddChildNode(Node childNode)
        {
            if (childNode == null) return;
            _childNodes.Add(childNode);
            if (childNode is MyNode)
            {
                (childNode as MyNode).ParentNode = this;
            }
        }

               /// 替换子节点
        /// <summary>
        /// 替换子节点
        /// </summary>
        /// <param name="oldNode"></param>
        /// <param name="newNode"></param>
        /// <returns></returns>
        public bool ReplaceChildNode(Node oldNode, Node newNode)
        {
            if (oldNode == null || newNode == null) return false;

            int index = GetIndex(oldNode);
            if (index == -1)
                return false;

            ChildNodes.RemoveAt(index);
            ChildNodes.Insert(index, newNode);

            if (oldNode is MyNode)
            {
                MyNode oldNode1 = oldNode as MyNode;
                if (oldNode1.ParentNode == this)
                {
                    oldNode1.ParentNode = null;
                }
            }
            if (newNode is MyNode)
            {
                MyNode newNode1 = newNode as MyNode;
                newNode1.ParentNode = this;
            }
            return true;
        }

        /// 获取指定节点的 索引值
        /// <summary>
        /// 获取指定节点的 索引值
        /// </summary>
        /// <param name="childNode"></param>
        /// <returns></returns>
        public int GetIndex(Node childNode)
        {
            for (int i = 0; i < _childNodes.Count; ++i)
            {
                if (_childNodes[i] == childNode) return i;
            }
            return -1;
        }

        #region 新属性

        public string PowerSupply { get; set; }
        public double PowerConsumption { get; set; }
        public double PowerCurrent { get; set; }
        public int MaxBranches { get; set; }
        public double MaxCapacityPerBranch { get; set; }
        public int MaxIUPerBranch { get; set; }
        /// 电源线形 add by Shen Junjie on 2018/01/16
        /// <summary>
        /// 电源线形
        /// </summary>
        public string PowerLineType { get; set; }


        /// <summary>
        /// MultiCHBox 与室外机之间 高度差 add by xyj on 20180619
        /// </summary>
        public double HeightDiff { get; set; }
        #endregion

        #region 共同的属性
        private double _coolingCapacity;
        /// <summary>
        /// 下游室内机估算容量总和
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { _coolingCapacity = value; }
        }

        private double _heatingCapacity;
        /// <summary>
        /// 下游室内机估算制热容量总和
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { _heatingCapacity = value; }
        }

        private double _horsePower;
        /// <summary>
        /// 下游室内机估算马力总和
        /// </summary>
        public double HorsePower
        {
            get { return _horsePower; }
            set { _horsePower = value; }
        }

        private string _model;
        /// <summary>
        /// CHbox型号
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        #region 增加CH-Box下端所有管长总和限制 add on 20160515 by Yunxiao Lin

        private double _actualTotalCHIndoorPipeLength;
        /// <summary>
        /// CH-Box下端所有管长总和(实际值)
        /// </summary>
        public double ActualTotalCHIndoorPipeLength
        {
            get { return _actualTotalCHIndoorPipeLength; }
            set { _actualTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength;
        /// <summary>
        /// 当系统中室内机数量没有超过推荐数量，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength
        {
            get { return _maxTotalCHIndoorPipeLength; }
            set { _maxTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength_MaxIU;
        /// <summary>
        /// 当系统中室内机数量超过推荐数量，没有超过允许最大数量时，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength_MaxIU
        {
            get { return _maxTotalCHIndoorPipeLength_MaxIU; }
            set { _maxTotalCHIndoorPipeLength_MaxIU = value; }
        }
        private int _maxIndoorCount;
        /// <summary>
        /// CH-Box允许连接的最大室内机数量
        /// </summary>
        public int MaxIndoorCount
        {
            get { return _maxIndoorCount; }
            set { _maxIndoorCount = value; }
        }
        #endregion

        #endregion
    }

    /// 图例文字节点 add on 20160524 by Yunxiao Lin
    /// <summary>
    /// 图例文字节点
    /// </summary>
    [Serializable]
    public class MyNodeLegend : MyNode
    {
        private string _legendType;
        public string LegendType
        {
            get { return _legendType; }
            set { _legendType = value; }
        }
    }
    /// 布线接地节点 add on 20160525 by Yunxiao Lin
    /// <summary>
    /// 布线接地节点
    /// </summary>
    [Serializable]
    public class MyNodeGround_Wiring : MyNode
    {
        private string _groupType;
        public string GroundType
        {
            get { return _groupType; }
            set { _groupType = value; }
        }
    }
    /// 布线远程控制器节点 add on 20160525 by Yunxiao Lin
    /// <summary>
    /// 布线远程控制器节点
    /// </summary>
    public class MyNodeRemoteControler_Wiring : MyNode
    {
        private string _remoteControlerType;
        public string RemoteControlerType
        {
            get { return _remoteControlerType; }
            set { _remoteControlerType = value; }
        }
    }

    public class MyNodePlottingScale : MyNode
    {
        public double ActualLength { get; set; }
        public bool IsVertical { get; set; }
        public string ActualLengthString { get; set; }
        /// <summary>
        /// 获取画面与实际的比例
        /// </summary>
        public double PlottingScale
        {
            get
            {
                double plottingScale;
                if (this.IsVertical)
                {
                    plottingScale = this.ActualLength / this.Size.Height;
                }
                else
                {
                    plottingScale = this.ActualLength / this.Size.Width;
                }
                return plottingScale;
            }
        }
    }

    [Serializable]
    public class MyLink : Link
    {
        private double _elbowQty;
        /// <summary>
        /// 弯头数
        /// </summary>
        public double ElbowQty
        {
            get { return _elbowQty; }
            set { _elbowQty = value; }
        }

        private double _oilTrapQty;
        /// <summary>
        /// 存油弯数
        /// </summary>
        public double OilTrapQty
        {
            get { return _oilTrapQty; }
            set { _oilTrapQty = value; }
        }

        private double _length;
        /// <summary>
        /// 配管长度
        /// </summary>
        public double Length
        {
            get { return _length; }
            set { _length = value; }
        }

        //private double _coolingCapacity;
        ///// <summary>
        ///// 下游室内机估算制冷容量总和
        ///// </summary>
        //public double CoolingCapacity
        //{
        //    get { return _coolingCapacity; }
        //    set { _coolingCapacity = value; }
        //}

        //private double _heatingCapacity;
        ///// <summary>
        ///// 下游室内机估算制热容量总和
        ///// </summary>
        //public double HeatingCapacity
        //{
        //    get { return _heatingCapacity; }
        //    set { _heatingCapacity = value; }
        //}

        private string _specG_h;
        /// 气管管径规格
        /// <summary>
        /// 气管管径规格
        /// </summary>
        public string SpecG_h
        {
            get { return _specG_h; }
            set { _specG_h = value; }
        }

        private string _specG_l;
        /// 气管管径规格
        /// <summary>
        /// 气管管径规格(2Pipe时为"-")
        /// </summary>
        public string SpecG_l
        {
            get { return _specG_l; }
            set { _specG_l = value; }
        }

        private string _specL;
        /// 液管管径规格
        /// <summary>
        /// 液管管径规格
        /// </summary>
        public string SpecL
        {
            get { return _specL; }
            set { _specL = value; }
        }

        private double _valveLength;
        /// 电子膨胀阀管长 add on 20160616 by Yunxiao Lin
        /// <summary>
        /// 电子膨胀阀管长
        /// </summary>
        public double ValveLength
        {
            get { return _valveLength; }
            set { _valveLength = value; }
        }
        
        private string _specG_h_Normal;
        /// 气管管径规格   add on 20170516 by Shen Junjie
        /// <summary>
        /// 气管管径规格
        /// </summary>
        public string SpecG_h_Normal
        {
            get { return _specG_h_Normal; }
            set { _specG_h_Normal = value; }
        }

        private string _specG_l_Normal;
        /// 气管管径规格   add on 20170516 by Shen Junjie
        /// <summary>
        /// 气管管径规格(2Pipe时为"-")
        /// </summary>
        public string SpecG_l_Normal
        {
            get { return _specG_l_Normal; }
            set { _specG_l_Normal = value; }
        }

        private string _specL_Normal;
        /// 液管管径规格   add on 20170516 by Shen Junjie
        /// <summary>
        /// 液管管径规格
        /// </summary>
        public string SpecL_Normal
        {
            get { return _specL_Normal; }
            set { _specL_Normal = value; }
        }
    }


    #region for MyNodeOut 序列化
    [Serializable]
    public class tmpMyNode
    {
        public PointF Location { get; set; }
        public Color TextColor { get; set; }
        public Color FillColor { get; set; }

        /// <summary>
        /// add by Shen Junjie on 2018/01/04
        /// </summary>
        public tmpMyLink[] MyInLinks
        {
            get;
            set;
        }
    }

    [Serializable]
    //change on 20160429 by Yunxiao Lin.
    //public class tmpMyNodeOut
    public class tmpMyNodeOut : tmpMyNode, ISingleChild<tmpMyNode>
    {
        private tmpMyNode _childNode;

        public tmpMyNode ChildNode
        {
            get { return _childNode; }
            set { _childNode = value; }
        }

        /// <summary>
        /// 室外机机组内部各室外机、分歧管之间的管长
        /// </summary>
        public double[] PipeLengthes { get; set; }
        public Color PipeColor { get; set; }
        public Color BranchKitColor { get; set; }
        public Color NodeBgColor { get; set; }
    }

    /// <summary>
    /// 背景图片节点 add by Shen Junjie on 20170802
    /// </summary>
    [Serializable]
    public class tmpNodeBgImage : tmpMyNode
    {
        public SizeF Size { get; set; }
        /// <summary>
        /// 已废弃，只为兼容老项目
        /// </summary>
        public Image Image { get; set; }
        /// <summary>
        /// 图片二进制流，保存项目时用
        /// </summary>
        public Stream ImageStream { get; set; }
        /// <summary>
        /// 图片缓存的ID
        /// </summary>
        public string ImageCacheId { get; set; }
    }

    /// <summary>
    /// 比例尺 add by Shen Junjie on 20171025
    /// </summary>
    [Serializable]
    public class tmpNodePlottingScale : tmpMyNode
    {
        public SizeF Size { get; set; }
        public double ActualLength { get; set; }
        public Color DrawColor { get; set; }
        public Color FillColor { get; set; }
        public Color TextColor { get; set; }
        public bool IsVertical { get; set; }
    }

    [Serializable]
    public class tmpMyNodeIn : tmpMyNode
    {
        public tmpMyNodeIn(RoomIndoor riItem) { this._riItem = riItem; }

        private RoomIndoor _riItem;
        public RoomIndoor RoomIndoorItem
        {
            get { return _riItem; }
            set { _riItem = value; }
        }

        private tmpMyLink _inLink;
        /// <summary>
        /// 已弃用， 保留此属性仅为了打开老项目可以反序列化  modified by Shen Junjie on 2018/1/4
        /// </summary>
        public tmpMyLink InLink
        {
            get { return _inLink; }
            set { _inLink = value; }
        }

        //private tmpMyLink[] _myInLinks;
        ///// <summary>
        ///// 为了支持多水管和气管的室内机机型 add by Shen Junjie on 2018/1/3
        ///// </summary>
        //public tmpMyLink[] MyInLinks
        //{
        //    get
        //    {
        //        if (_myInLinks == null && _inLink != null)
        //        {
        //            _myInLinks = new tmpMyLink[] { _inLink };
        //            _inLink = null;
        //        }
        //        return _myInLinks;
        //    }
        //    set
        //    {
        //        _myInLinks = value;
        //    }
        //}
    }

    [Serializable]
    public class tmpMyNodeYP : tmpMyNode, IChildNodeArray<tmpMyNode>
    {
        public const int MaxOutlinksCount = 2;
        public const int MaxOutlinksCountCP = 8;

        public tmpMyNodeYP(bool isCP)
            : base()
        {
            this._isCP = isCP;
            this._maxCount = isCP ? MaxOutlinksCountCP : MaxOutlinksCount;
            this._childCount = 0;
            //add on 20160429 by Yunxiao Lin
            this._isCoolingonly = false;

            if (_childNodes == null)
            {
                if (!isCP)
                    _childNodes = new tmpMyNode[MaxOutlinksCount];
                else if (isCP)
                    _childNodes = new tmpMyNode[MaxOutlinksCountCP];
            }

        }

        private bool _isCP;
        /// <summary>
        /// 是否是梳型管，只读
        /// </summary>
        public bool IsCP
        {
            get { return _isCP; }
        }

        private int _maxCount = 0;
        /// <summary>
        /// 最大子节点数量，只读
        /// </summary>
        public int MaxCount
        {
            get { return _maxCount; }
        }

        private int _childCount = 0;
        /// <summary>
        /// 当前对象中的子节点数量，只读
        /// </summary>
        public int ChildCount
        {
            get { return _childCount; }
        }

        private tmpMyNode[] _childNodes;
        /// <summary>
        /// 分歧管的子节点
        /// </summary>
        public tmpMyNode[] ChildNodes
        {
            get { return _childNodes; }
            set
            {
                _childNodes = value;
                // 计算当前的 ChildCount 值
                //int maxCount = IsCP ? MaxOutlinksCountCP : MaxOutlinksCount;
                for (int i = 0; i < _maxCount; ++i)
                {
                    if (_childNodes[i] == null)
                    {
                        _childCount = i;
                        return;
                    }
                }
            }
        }

        private tmpMyLink _inLink;     
        /// <summary>
        /// 已弃用， 保留此属性仅为了打开老项目可以反序列化  modified by Shen Junjie on 2018/1/4
        /// </summary>
        public tmpMyLink InLink
        {
            get { return _inLink; }
            set { _inLink = value; }
        }


        private double _coolingCapacity;
        /// <summary>
        /// 下游室内机估算容量总和
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { _coolingCapacity = value; }
        }

        private double _heatingCapacity;
        /// <summary>
        /// 下游室内机估算制热容量总和
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { _heatingCapacity = value; }
        }

        private string _model;
        /// <summary>
        /// 气管型号
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }
        
        private string _priceG;
        /// <summary>
        /// YP 气管价格
        /// </summary>
        public string PriceG
        {
            get { return _priceG; }
            set { _priceG = value; }
        }

        /// 添加子节点, 默认添加在末尾
        /// <summary>
        /// 添加子节点, 默认添加在末尾
        /// </summary>
        /// <param name="childNode"></param>
        public bool AddChildNode(tmpMyNode childNode)
        {
            return AddChildNode(childNode, _childCount);
        }

        /// 在指定位置插入子节点，指定位置开始的节点顺次后移一位
        /// <summary>
        /// 在指定位置插入子节点，指定位置开始的节点顺次后移一位
        /// </summary>
        /// <param name="childNode"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool AddChildNode(tmpMyNode childNode, int index)
        {
            if (_childCount < _maxCount && index < _maxCount)
            {
                for (int i = _maxCount - 1; i > index; --i)
                {
                    _childNodes[i] = _childNodes[i - 1];
                }
                _childNodes[index] = childNode;

                ++_childCount;
                return true;
            }

            return false;
        }

        // add on 20160429 by Yunxiao Lin
        /// <summary>
        /// 是否Cooling only 节点
        /// </summary>
        private bool _isCoolingonly;
        public bool IsCoolingonly
        {
            get { return _isCoolingonly; }
            set { _isCoolingonly = value; }
        }
    }

    [Serializable]
    public class tmpMyNodeCH : tmpMyNode, ISingleChild<tmpMyNode>
    {
        public tmpMyNodeCH(tmpMyNode chNode)
        {
            _childNode = chNode;
        }

        private tmpMyLink _inLink;
        /// <summary>
        /// 已弃用， 保留此属性仅为了打开老项目可以反序列化  modified by Shen Junjie on 2018/1/4
        /// </summary>
        public tmpMyLink InLink
        {
            get { return _inLink; }
            set { _inLink = value; }
        }


        private tmpMyNode _childNode;
        /// <summary>
        /// 子节点(MyNodeIn / MyNodeYP(含CP))
        /// </summary>
        public tmpMyNode ChildNode
        {
            get { return _childNode; }
            set { _childNode = value; }
        }

        private double _coolingCapacity;
        /// <summary>
        /// 下游室内机估算容量总和
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { _coolingCapacity = value; }
        }

        private double _heatingCapacity;
        /// <summary>
        /// 下游室内机估算制热容量总和
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { _heatingCapacity = value; }
        }

        private string _model;
        /// <summary>
        /// CHbox型号
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }
        #region 增加CH-Box下端所有管长总和限制 add on 20160515 by Yunxiao Lin

        private double _actualTotalCHIndoorPipeLength;
        /// <summary>
        /// CH-Box下端所有管长总和(实际值)
        /// </summary>
        public double ActualTotalCHIndoorPipeLength
        {
            get { return _actualTotalCHIndoorPipeLength; }
            set { _actualTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength;
        /// <summary>
        /// 当系统中室内机数量没有超过推荐数量，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength
        {
            get { return _maxTotalCHIndoorPipeLength; }
            set { _maxTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength_MaxIU;
        /// <summary>
        /// 当系统中室内机数量超过推荐数量，没有超过允许最大数量时，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength_MaxIU
        {
            get { return _maxTotalCHIndoorPipeLength_MaxIU; }
            set { _maxTotalCHIndoorPipeLength_MaxIU = value; }
        }
        private int _maxIndoorCount;
        /// <summary>
        /// CH-Box允许连接的最大室内机数量
        /// </summary>
        public int MaxIndoorCount
        {
            get { return _maxIndoorCount; }
            set { _maxIndoorCount = value; }
        }
        #endregion

        /// 电源 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源
        /// </summary>
        public string PowerSupply { get; set; }

        /// 电源线形 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源线形
        /// </summary>
        public string PowerLineType { get; set; }
        
        //add by Shen Junjie on 2018/6/15
        /// <summary>
        /// 电源功耗
        /// </summary>
        public double PowerConsumption { get; set; }

        /// <summary>
        /// CHBox 与室外机之间 高度差 add by xyj on 20180619
        /// </summary>
        public double HeightDiff { get; set; }

        //add by Shen Junjie on 2017/6/15
        /// <summary>
        /// 电流
        /// </summary>
        public double PowerCurrent { get; set; }
    }

    [Serializable]
    public class tmpMyNodeMultiCH : tmpMyNode, IChildNodeList<tmpMyNode>
    {
        public tmpMyNodeMultiCH()
            : base()
        {
            _childNodes = new List<tmpMyNode>();
        }

        private tmpMyLink _inLink;
        /// <summary>
        /// 节点的InLink
        /// 已弃用， 保留此属性仅为了打开老项目可以反序列化  modified by Shen Junjie on 2018/1/4
        /// </summary>
        public tmpMyLink InLink
        {
            get { return _inLink; }
            set { _inLink = value; }
        }

        private List<tmpMyNode> _childNodes;
        /// <summary>
        /// 布线时的子节点列表
        /// </summary>
        public List<tmpMyNode> ChildNodes
        {
            get { return _childNodes; }
        }

        #region 新属性

        public string PowerSupply { get; set; }
        public double PowerConsumption { get; set; }
        public double PowerCurrent { get; set; }
        public int MaxBranches { get; set; }
        public double MaxCapacityPerBranch { get; set; }
        public int MaxIUPerBranch { get; set; }
        /// 电源线形 add by Shen Junjie on 2018/01/16
        /// <summary>
        /// 电源线形
        /// </summary>
        public string PowerLineType { get; set; }

        /// <summary>
        /// MultiCHBox 与室外机之间 高度差 add by xyj on 20180619
        /// </summary>
        public double HeightDiff { get; set; }
        #endregion

        #region 共同的属性
        private double _coolingCapacity;
        /// <summary>
        /// 下游室内机估算容量总和
        /// </summary>
        public double CoolingCapacity
        {
            get { return _coolingCapacity; }
            set { _coolingCapacity = value; }
        }

        private double _heatingCapacity;
        /// <summary>
        /// 下游室内机估算制热容量总和
        /// </summary>
        public double HeatingCapacity
        {
            get { return _heatingCapacity; }
            set { _heatingCapacity = value; }
        }

        private string _model;
        /// <summary>
        /// CHbox型号
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        #region 增加CH-Box下端所有管长总和限制 add on 20160515 by Yunxiao Lin

        private double _actualTotalCHIndoorPipeLength;
        /// <summary>
        /// CH-Box下端所有管长总和(实际值)
        /// </summary>
        public double ActualTotalCHIndoorPipeLength
        {
            get { return _actualTotalCHIndoorPipeLength; }
            set { _actualTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength;
        /// <summary>
        /// 当系统中室内机数量没有超过推荐数量，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength
        {
            get { return _maxTotalCHIndoorPipeLength; }
            set { _maxTotalCHIndoorPipeLength = value; }
        }

        private double _maxTotalCHIndoorPipeLength_MaxIU;
        /// <summary>
        /// 当系统中室内机数量超过推荐数量，没有超过允许最大数量时，CH-Box下端所有管长总和限制
        /// </summary>
        public double MaxTotalCHIndoorPipeLength_MaxIU
        {
            get { return _maxTotalCHIndoorPipeLength_MaxIU; }
            set { _maxTotalCHIndoorPipeLength_MaxIU = value; }
        }
        private int _maxIndoorCount;
        /// <summary>
        /// CH-Box允许连接的最大室内机数量
        /// </summary>
        public int MaxIndoorCount
        {
            get { return _maxIndoorCount; }
            set { _maxIndoorCount = value; }
        }
        #endregion

        #endregion
    }

    [Serializable]
    public class tmpMyLink
    {
        public List<PointF> Points { get; set; }
        public Color DrawColor { get; set; }

        //public Line Line { get; set; }
        public LineStyle Style { get; set; }

        private double _elbowQty;
        /// <summary>
        /// 弯头数
        /// </summary>
        public double ElbowQty
        {
            get { return _elbowQty; }
            set { _elbowQty = value; }
        }

        private double _oilTrapQty;
        /// <summary>
        /// 存油弯数
        /// </summary>
        public double OilTrapQty
        {
            get { return _oilTrapQty; }
            set { _oilTrapQty = value; }
        }

        private double _length;
        /// <summary>
        /// 配管长度
        /// </summary>
        public double Length
        {
            get { return _length; }
            set { _length = value; }
        }

        private string _specG_h;
        /// <summary>
        /// 气管管径规格
        /// </summary>
        public string SpecG_h
        {
            get { return _specG_h; }
            set { _specG_h = value; }
        }

        private string _specG_l;
        /// <summary>
        /// 气管管径规格
        /// </summary>
        public string SpecG_l
        {
            get { return _specG_l; }
            set { _specG_l = value; }
        }

        private string _specL;
        /// <summary>
        /// 液管管径规格
        /// </summary>
        public string SpecL
        {
            get { return _specL; }
            set { _specL = value; }
        }

        private double _valveLength;
        /// 电子膨胀阀管长 add on 20160616 by Yunxiao Lin
        /// <summary>
        /// 电子膨胀阀管长
        /// </summary>
        public double ValveLength
        {
            get { return _valveLength; }
            set { _valveLength = value; }
        }

        private string _specG_h_Normal;
        /// 气管管径规格   add on 20170516 by Shen Junjie
        /// <summary>
        /// 气管管径规格
        /// </summary>
        public string SpecG_h_Normal
        {
            get { return _specG_h_Normal; }
            set { _specG_h_Normal = value; }
        }

        private string _specG_l_Normal;
        /// 气管管径规格   add on 20170516 by Shen Junjie
        /// <summary>
        /// 气管管径规格(2Pipe时为"-")
        /// </summary>
        public string SpecG_l_Normal
        {
            get { return _specG_l_Normal; }
            set { _specG_l_Normal = value; }
        }

        private string _specL_Normal;
        /// 液管管径规格   add on 20170516 by Shen Junjie
        /// <summary>
        /// 液管管径规格
        /// </summary>
        public string SpecL_Normal
        {
            get { return _specL_Normal; }
            set { _specL_Normal = value; }
        }
    }
    #endregion

    public class WiringNode : MyNode
    {
        public WiringNode()
        {
            this.Selectable = false;
            this.LabelEdit = false;
            this.DrawColor = Color.Transparent;
            this.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);

            _ChildNodes = new List<WiringNode>();
        }
        public Node LeftControlContact { get; set; }
        public Node RightControlContact { get; set; }
        public Node PowerContact { get; set; }
        public Node GroundContact { get; set; }

        private List<WiringNode> _ChildNodes;
        public List<WiringNode> ChildNodes
        {
            get { return _ChildNodes; }
        }
    }

    public class WiringNodeOut : Node
    {
        public WiringNodeOut()
        {
            _ChildNodes = new List<WiringNode>();
            _Models = new List<WiringNode>();
        }

        public SystemVRF SystemItem { get; set; }

        private List<WiringNode> _Models;
        public List<WiringNode> Models
        {
            get { return _Models; }
        }

        private List<WiringNode> _ChildNodes;
        public List<WiringNode> ChildNodes
        {
            get { return _ChildNodes; }
        }
    }

    public class WiringNodeGroup : Node
    {
        public List<WiringNodeCentralControl> ControllerListLevel1 = new List<WiringNodeCentralControl>();
        public List<WiringNodeCentralControl> ControllerListLevel2 = new List<WiringNodeCentralControl>();
        public List<WiringNodeOut> OutdoorList = new List<WiringNodeOut>();
        public List<WiringNodeIn> TotalHeatExchangerList = new List<WiringNodeIn>();
    }

    public class WiringNodeCentralControl: WiringNode
    {
        public Controller Controller { get; set; }

        public int Level { get; set; }
    }

    public class WiringNodeIn : WiringNode
    {
        private RoomIndoor _riItem;
        public RoomIndoor RoomIndoorItem
        {
            get { return _riItem; }
            set { _riItem = value; }
        }

        public bool IsNewBranchOfParent { get; set; }
    }

    public class WiringNodeCH : WiringNode
    {
        public WiringNodeCH()
        {
            IsMultiCHBox = false;
            RightControlContacts = new List<Node>();
        }

        private string _model;
        /// <summary>
        /// CHbox型号
        /// </summary>
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        /// <summary>
        /// 是否Multiple CH Box
        /// </summary>
        public bool IsMultiCHBox
        {
            get;
            protected set;
        }

        /// 电源 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源
        /// </summary>
        public string PowerSupply { get; set; }

        /// 电源线形 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电源线形
        /// </summary>
        public string PowerLineType { get; set; }

        /// 电流 add by Shen Junjie on 2017/12/21
        /// <summary>
        /// 电流
        /// </summary>
        public double PowerCurrent { get; set; }

        public List<Node> RightControlContacts { get; private set; }
    }

    public class WiringNodeMultiCH : WiringNodeCH
    {
        public WiringNodeMultiCH()
        {
            IsMultiCHBox = true;
        }
    }
}
