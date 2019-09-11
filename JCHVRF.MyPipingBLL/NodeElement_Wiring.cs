using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace JCHVRF.MyPipingBLL
{

    public class NodeElement_Wiring
    {
        string _keyName;
        /// 关键字，用于获取节点图片的坐标文件
        /// <summary>
        /// 关键字，用于获取节点图片的坐标文件
        /// </summary>
        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

        string _shortModel;
        /// YVOH200等
        /// <summary>
        /// YVOH200等
        /// </summary>
        public string ShortModel
        {
            get { return _shortModel; }
            set { _shortModel = value; }
        }

        int _unitCount;
        /// 室外机内部组合单元机数量
        /// <summary>
        /// 室外机内部组合单元机数量
        /// </summary>
        public int UnitCount
        {
            get { return _unitCount; }
            set { _unitCount = value; }
        }

        private string[] _modelFullGroup;
        /// 室外机内部单机完整型号 add on 20160521 by Yunxiao Lin
        /// <summary>
        /// 室外机内部单机完整型号
        /// </summary>
        public string[] ModelFullGroup
        {
            get { return _modelFullGroup; }
            set { _modelFullGroup = value; }
        }

        private string[] _modelGroup;
        /// 节点中间显示的型号短名，YVOH120等
        /// <summary>
        /// 节点中间显示的型号短名，YVOH120等
        /// </summary>
        public string[] ModelGroup
        {
            get { return _modelGroup; }
            set { _modelGroup = value; }
        }

        List<Point> _ptModelGroup;
        /// ModelName 坐标列表
        /// <summary>
        /// ModelName 坐标列表
        /// </summary>
        public List<Point> PtModelGroup
        {
            get { return _ptModelGroup; }
            set { _ptModelGroup = value; }
        }

        private string _str1;
        /// IN A B | A B
        /// <summary>
        /// IN A B | A B
        /// </summary>
        public string Str1
        {
            get { return _str1; }
            set { _str1 = value; }
        }

        Point _ptStr1;
        /// IN A B | A B 文字显示的坐标点
        /// <summary>
        /// IN A B | A B 文字显示的坐标点
        /// </summary>
        public Point PtStr1
        {
            get { return _ptStr1; }
            set { _ptStr1 = value; }
        }

        //Point _ptStrLine1;
        ///// IN A B | A B 下划线坐标位置,挪至坐标文件中
        ///// <summary>
        ///// IN A B | A B 下划线坐标位置,挪至坐标文件中
        ///// </summary>
        //public Point PtStrLine1
        //{
        //    get { return _ptStrLine1; }
        //    set { _ptStrLine1 = value; }
        //}

        private string[] _strGroup1;
        /// X Y | X Y
        /// <summary>
        /// X Y | X Y
        /// </summary>
        public string[] StrGroup1
        {
            get { return _strGroup1; }
            set { _strGroup1 = value; }
        }

        List<Point> _ptStrGroup1;
        /// X Y...坐标列表
        /// <summary>
        /// X Y...坐标列表
        /// </summary>
        public List<Point> PtStrGroup1
        {
            get { return _ptStrGroup1; }
            set { _ptStrGroup1 = value; }
        }

        private string[] _strGroup2;
        /// L1 L2 L3 N | L1 L2 L3 N...
        /// <summary>
        /// L1 L2 L3 N | L1 L2 L3 N...
        /// </summary>
        public string[] StrGroup2
        {
            get { return _strGroup2; }
            set { _strGroup2 = value; }
        }

        List<Point> _ptStrGroup2;
        /// L1 L2 L3 N... 文字坐标列表
        /// <summary>
        /// L1 L2 L3 N... 文字坐标列表
        /// </summary>
        public List<Point> PtStrGroup2
        {
            get { return _ptStrGroup2; }
            set { _ptStrGroup2 = value; }
        }

        List<Point> _ptStrGroupLine2L;
        /// L1 L2 L3 N... 下划线坐标列表
        /// <summary>
        /// L1 L2 L3 N... 下划线坐标列表
        /// </summary>
        public List<Point> PtStrGroupLine2L
        {
            get { return _ptStrGroupLine2L; }
            set { _ptStrGroupLine2L = value; }
        }


        List<Point> _ptStrGroupLine2R;
        /// L1 L2 L3 N... 下划线坐标列表
        /// <summary>
        /// L1 L2 L3 N... 下划线坐标列表
        /// </summary>
        public List<Point> PtStrGroupLine2R
        {
            get { return _ptStrGroupLine2R; }
            set { _ptStrGroupLine2R = value; }
        }

        private string[] _strGroup3;
        /// 电流数据数组 “23A 3Nph...”
        /// <summary>
        /// 电流数据数组 “23A 3Nph...”
        /// </summary>
        public string[] StrGroup3
        {
            get { return _strGroup3; }
            set { _strGroup3 = value; }
        }

        List<Point> _ptStrGroup3;
        /// 电流数据坐标列表 “23A 3Nph”
        /// <summary>
        /// 电流数据坐标列表 “23A 3Nph”
        /// </summary>
        public List<Point> PtStrGroup3
        {
            get { return _ptStrGroup3; }
            set { _ptStrGroup3 = value; }
        }

        private string[] _strGroup4;
        /// 电源线2芯3芯N芯标识 add on 20160520 by Yunxiao Lin
        /// <summary>
        /// 电源线2芯3芯N芯标识
        /// </summary>
        public string[] StrGroup4
        {
            get { return _strGroup4; }
            set { _strGroup4 = value; }
        }

        List<Point> _ptStrGroup4;
        /// 电源线2芯3芯N芯标识坐标列表 add on 20160520 by Yunxiao Lin
        /// <summary>
        /// 电源线2芯3芯N芯标识坐标列表
        /// </summary>
        public List<Point> PtStrGroup4
        {
            get { return _ptStrGroup4; }
            set { _ptStrGroup4 = value; }
        }

        List<Point> _ptCircles;
        /// 每个小圆圈的坐标位置列表
        /// <summary>
        /// 每个小圆圈的坐标位置列表
        /// </summary>
        public List<Point> PtCircles
        {
            get { return _ptCircles; }
            set { _ptCircles = value; }
        }

        Size _nodeSize;
        /// 节点尺寸
        /// <summary>
        /// 节点尺寸
        /// </summary>
        public Size NodeSize
        {
            get { return _nodeSize; }
            set { _nodeSize = value; }
        }

        Size _circleSize;
        /// 节点中四个圆圈尺寸
        /// <summary>
        /// 节点中四个圆圈尺寸
        /// </summary>
        public Size CircleSize
        {
            get { return _circleSize; }
            set { _circleSize = value; }
        }

        List<Point> _ptNodeNames;
        /// <summary>
        /// 节点标题的坐标
        /// </summary>
        public List<Point> PtNodeNames
        {
            get { return _ptNodeNames; }
            set { _ptNodeNames = value; }
        }

        /// 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="keyName">关键字</param>
        /// <param name="shortModel">型号前七位，YVOH200</param>
        /// <param name="count">内部单元机组数量</param>
        /// <param name="modelGroup">组合机组的Model，“*，*，*”</param>
        /// <param name="strGroup3">右侧 23A 3Nph</param>
        /// <param name="brandCode">"Y"-York  "H"-Hitachi</param>
        public NodeElement_Wiring(string keyName, string shortModel, int count, string modelGroup,string strGroup2, string strGroup3,string strGroup4, string brandCode)
        {
            this._keyName = keyName;
            this._shortModel = shortModel;
            this._unitCount = count;
            this._modelGroup = modelGroup?.Split(',');//null check added

            #region  /// small size
            #endregion

            if (brandCode == "Y") //York
            {
                #region York
                // 线条尺寸颜色等
                this._nodeSize = new Size(202, 200);
                this._circleSize = new Size(4, 4);

                // 坐标位置
                // 室外机增加到4组 20161028 by Yunxiao Lin
                //string ptModelGroup = "(74,20)(74,80)(74,140)";
                //string ptModelGroup = "(74,20)(74,80)(74,140)(74,200)";
                string ptModelGroup = "(64,20)(64,80)(64,140)(64,200)";
                string ptStr1 = "(142,0)";  // 1 2 | 3 4
                //string ptStrGroup1 = "(35,38)(35,98)"; // 1 2 | 3 4 | A B
                string ptStrGroup1 = "(35,38)(35,98)(35,158)(35,218)"; // 1 2 | 3 4 | A B
                /*
                 三相 <380V: R,S,T
                      380V-415V: L1,L2,L3,N
                 单相 <380V: R,S
                      380V-415V: L,N
                 */
                
                //string ptStrGroup2 = "(142,38)(142,98)(142,158)";
                //string ptStrGroupLine2L = "(140,50)(140,110)(140,170)";
                //string ptStrGroupLine2R = "(201,50)(201,110)(201,170)";
                //string ptStrGroup3 = "(142,53)(142,113)(142,173)"; // 220V 60Hz 5A | 380V 50Hz 23A ...
                string ptStrGroup2 = "(142,38)(142,98)(142,158)(142,218)";
                string ptStrGroupLine2L = "(140,50)(140,110)(140,170)(140,230)";
                string ptStrGroupLine2R = "(201,50)(201,110)(201,170)(201,230)";
                string ptStrGroup3 = "(142,53)(142,113)(142,173)(142,233)"; // 220V 60Hz 5A | 380V 50Hz 23A ...
                //电源线标识 /// | //
                //string ptStrGroup4 = "(185,43)(185,103)(185,163)";
                //string ptCircles = "(123,3)(133,3)(63, 43)(73,43)(123,63)(133,63)(63, 103)(73,103)(123,123)(133,123)(63, 163)(73, 163)";
                //string ptNodeNames = "(86,-26)(74,-13)"; // Out1\nYVOH400 | Ind1
                string ptStrGroup4 = "(185,43)(185,103)(185,163)(185,223)";
                string ptCircles = "(123,3)(133,3)(63, 43)(73,43)(123,63)(133,63)(63, 103)(73,103)(123,123)(133,123)(63, 163)(73, 163)(123,183)(133,183)(63, 223)(73, 223)";
                string ptNodeNames = "(86,-26)(74,-13)"; // Out1\nYVOH400 | Ind1

                // 文字内容
                this._str1 = "IN A B";  // 室外机
                string strGroup1 = "X Y,X Y,X Y,X Y";

                if (keyName == "Ind")
                {
                    this._str1 = "A B";
                    strGroup1 = "A B";
                    //ptStrGroupLine2R = "(230,50)(230,110)(230,170)";
                    ptStrGroupLine2R = "(230,50)(230,110)(230,170)(230,230)";
                }
                this._strGroup1 = strGroup1.Split(',');
                this._strGroup2 = strGroup2.Split('|');
                this._strGroup3 = strGroup3.Split('|');
                this._strGroup4 = strGroup4.Split(',');

                this._ptModelGroup = UtilEMF.transPoints(ptModelGroup);
                this._ptStr1 = UtilEMF.transPoints(ptStr1)[0];
                this._ptStrGroup1 = UtilEMF.transPoints(ptStrGroup1);
                this._ptStrGroup2 = UtilEMF.transPoints(ptStrGroup2);
                this._ptStrGroupLine2L = UtilEMF.transPoints(ptStrGroupLine2L);
                this._ptStrGroupLine2R = UtilEMF.transPoints(ptStrGroupLine2R);
                this._ptStrGroup3 = UtilEMF.transPoints(ptStrGroup3);
                this._ptStrGroup4 = UtilEMF.transPoints(ptStrGroup4);
                this._ptCircles = UtilEMF.transPoints(ptCircles);
                this._ptNodeNames = UtilEMF.transPoints(ptNodeNames);
                #endregion
            }
            else
            {
                #region Hitachi add on 20160519 by Yunxiao Lin
                // 线条尺寸颜色等
                this._nodeSize = new Size(202, 200);
                this._circleSize = new Size(4, 4);

                // 坐标位置
                //string ptModelGroup = "(74,20)(74,80)(74,140)";
                //string ptModelGroup = "(74,20)(74,80)(74,140)(74,200)";
                string ptModelGroup = "(64,20)(64,80)(64,140)(64,200)";
                string ptStr1 = "(142,0)";  // 1 2 | 3 4
                //string ptStrGroup1 = "(35,38)(35,98)"; // 1 2 | 3 4 | A B
                string ptStrGroup1 = "(35,38)(35,98)(35,158)(35,218)"; // 1 2 | 3 4 | A B
                /*
                 三相 <380V: R,S,T
                      380V-415V: L1,L2,L3,N
                 单相 <380V: R,S
                      380V-415V: L,N
                 */
                //string ptStrGroup2 = "(142,38)(142,98)(142,158)"; 
                //string ptStrGroupLine2L = "(140,50)(140,110)(140,170)";
                //string ptStrGroupLine2R = "(241,50)(241,110)(241,170)";
                //string ptStrGroup3 = "(142,53)(142,113)(142,173)"; // 220V 60Hz 5A | 380V 50Hz 23A ...
                string ptStrGroup2 = "(142,38)(142,98)(142,158)(142,218)";
                string ptStrGroupLine2L = "(140,50)(140,110)(140,170)(140,230)";
                string ptStrGroupLine2R = "(241,50)(241,110)(241,170)(241,230)";
                string ptStrGroup3 = "(142,53)(142,113)(142,173)(142,233)"; // 220V 60Hz 5A | 380V 50Hz 23A ...
                //电源线标识 /// | //
                //string ptStrGroup4 = "(185,43)(185,103)(185,163)";
                //string ptCircles = "(123,3)(133,3)(63, 43)(73,43)(123,63)(133,63)(63, 103)(73,103)(123,123)(133,123)(63, 163)(73, 163)";
                //string ptNodeNames = "(86,-26)(74,-13)"; // Out1\nYVOH400 | Ind1
                string ptStrGroup4 = "(185,43)(185,103)(185,163)(185,223)";
                string ptCircles = "(123,3)(133,3)(63, 43)(73,43)(123,63)(133,63)(63, 103)(73,103)(123,123)(133,123)(63, 163)(73, 163)(123,183)(133,183)(63, 223)(73, 223)";
                string ptNodeNames = "(86,-26)(74,-13)"; // Out1\nYVOH400 | Ind1

                // 文字内容
                this._str1 = "1 2";  // 室外机
                //string strGroup1 = "3 4,3 4";
                string strGroup1 = "3 4,3 4,3 4,3 4";

                if (keyName == "CH")
                {
                    this._str1 = "1 2";
                    strGroup1 = "3 4";
                    //ptStrGroupLine2R = "(230,50)(230,110)(230,170)";
                    ptStrGroupLine2R = "(230,50)(230,110)(230,170)(230,230)";
                }
                if (keyName == "Ind")
                {
                    this._str1 = "A B";
                    strGroup1 = "1 2";
                    //ptStrGroupLine2R = "(250,50)(250,110)(250,170)";
                    ptStrGroupLine2R = "(250,50)(250,110)(250,170)(250,230)";
                }
                this._strGroup1 = strGroup1.Split(',');
                this._strGroup2 = strGroup2.Split('|');
                this._strGroup3 = strGroup3.Split('|');
                this._strGroup4 = strGroup4.Split(',');

                this._ptModelGroup = UtilEMF.transPoints(ptModelGroup);
                this._ptStr1 = UtilEMF.transPoints(ptStr1)[0];
                this._ptStrGroup1 = UtilEMF.transPoints(ptStrGroup1);
                this._ptStrGroup2 = UtilEMF.transPoints(ptStrGroup2);
                this._ptStrGroupLine2L = UtilEMF.transPoints(ptStrGroupLine2L);
                this._ptStrGroupLine2R = UtilEMF.transPoints(ptStrGroupLine2R);
                this._ptStrGroup3 = UtilEMF.transPoints(ptStrGroup3);
                this._ptStrGroup4 = UtilEMF.transPoints(ptStrGroup4);
                this._ptCircles = UtilEMF.transPoints(ptCircles);
                this._ptNodeNames = UtilEMF.transPoints(ptNodeNames);
                #endregion
            }
            
        }

    }

}