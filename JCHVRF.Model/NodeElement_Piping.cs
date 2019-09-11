using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System;
using System.Text.RegularExpressions;

namespace JCHVRF.Model
{
    /// 室外机组合的类
    /// <summary>
    /// 室外机组合的类
    /// </summary>
    public class NodeElement_Piping : ModelBase
    {
        int _key;
        /// <summary>
        /// （没有使用）
        /// </summary>
        public int Key
        {
            get { return _key; }
            set { this.SetValue(ref _key, value); }
        }

        string _name;
        /// <summary>
        /// 节点图名称，此处对应图片名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        int _unitCount;
        /// <summary>
        /// 室外机组合中机组数目
        /// </summary>
        public int UnitCount
        {
            get { return _unitCount; }
            set { this.SetValue(ref _unitCount, value); }
        }

        string[] _connectionKitModel;
        /// <summary>
        /// 分歧管型号
        /// </summary>
        public string[] ConnectionKitModel
        {
            get { return _connectionKitModel; }
            set { this.SetValue(ref _connectionKitModel, value); }
        }

        List<Point> _ptConnectionKit;
        /// <summary>
        /// 分歧管型号的显示坐标
        /// </summary>
        public List<Point> PtConnectionKit
        {
            get { return _ptConnectionKit; }
            set { this.SetValue(ref _ptConnectionKit, value); }
        }

        string[] _pipeSize;
        /// <summary>
        /// 连接管管径，数量取决于UnitCount数值
        /// </summary>
        public string[] PipeSize
        {
            get { return _pipeSize; }
            set { this.SetValue(ref _pipeSize, value); }
        }

        List<Point> _ptPipeSize;
        /// <summary>
        /// 管径型号的显示坐标
        /// </summary>
        public List<Point> PtPipeDiameter
        {
            get { return _ptPipeSize; }
            set { this.SetValue(ref _ptPipeSize, value); }
        }

        Point _ptVLine;
        /// <summary>
        /// 左下角竖直的线条坐标
        /// </summary>
        public Point PtVLine
        {
            get { return _ptVLine; }
            set { this.SetValue(ref _ptVLine, value); }
        }

        List<Point> _ptModelLocation;
        /// <summary>
        /// model显示坐标
        /// </summary>
        public List<Point> PtModelLocation
        {
            get { return _ptModelLocation; }
            set { this.SetValue(ref _ptModelLocation, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">节点图片名称</param>
        /// <param name="count">单元机数量</param>
        /// <param name="jointKit">分歧管型号</param>
        /// <param name="ptJointKit"> 分歧管型号文字坐标,"(x,y)" </param>
        /// <param name="pipes">管径型号"LxG,LxG..."</param>
        /// <param name="ptPipeDiameter"> 管径型号文字显示坐标"(x,y)(x,y)(x,y)" </param>
        /// <param name="ptVLine">节点左下方竖直线段坐标</param>
        public NodeElement_Piping(string name, int count, string jointKit, string ptJointKit, string pipes, string ptPipeDiameter, string ptVLine,string ptModelLocation)
        {
            this._name = name;
            this._unitCount = count;

            if (!string.IsNullOrEmpty(jointKit))
            {
                this._connectionKitModel = jointKit.Split(',');
                this._pipeSize = pipes.Split(',');
                this._ptConnectionKit = transPoints(ptJointKit);
                this._ptPipeSize = transPoints(ptPipeDiameter);
                this._ptVLine = transPoints(ptVLine)[0]; // 仅有一个坐标
                if(!string.IsNullOrEmpty(ptModelLocation))
                {
                    this._ptModelLocation = transPoints(ptModelLocation);
                }
            }
        }

        public NodeElement_Piping(string name, int count)
        {
            this._name = name;
            this._unitCount = count;

        }


        /// 将字符串转换为坐标列表
        /// <summary>
        /// 将字符串转换为坐标列表
        /// </summary>
        /// <param name="ptString">匹配格式(x,y)(x,y)...</param>
        /// <returns></returns>
        public List<Point> transPoints(string ptString)
        {
            // 验证输入格式
            string pattern = @"\(.*?\)"; // 格式
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(ptString);

            // 转换过程
            List<Point> ptList = new List<Point>();
            foreach (Match match in matches)
            {
                string s = match.Value.Trim('(', ')');
                string[] ss = s.Split(',');
                Point pt = new Point(Convert.ToInt32(ss[0].Trim()), Convert.ToInt32(ss[1].Trim()));
                ptList.Add(pt);
            }
            return ptList;
        }

    }

}