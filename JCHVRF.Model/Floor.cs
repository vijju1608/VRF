//********************************************************************
// 文件名: Floor.cs
// 描述: 定义 VRF 项目中的楼层类
// 作者: clh
// 创建时间: 2012-04-01
// 修改历史: 
// 2013-3-19 新增属性 IsImport、ParentID、ParentName for NewVRF
// 2016-1-29 迁入JCHVRF
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    [Serializable]
    public class Floor : ModelBase
    {
        public Floor() { }
        public Floor(int number)
        {
            this._no = number;
            this.RoomList = new List<Room>();
            this._parentId = 1;
            this._isImport = false;
        }

        public List<Room> RoomList = new List<Room>();

        private string _id;
        /// <summary>
        /// 唯一编号
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { this.SetValue(ref _id, value); }
        }

        private int _no;
        /// <summary>
        /// 递增编号，自动生成
        /// </summary>
        public int NO
        {
            get { return _no; }
            set { this.SetValue(ref _no, value); }
        }

        private double _height;
        /// <summary>
        /// 层高
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { this.SetValue(ref _height, value); }
        }

        private string _name;
        /// <summary>
        /// 楼层扩展名,用户自定义名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        private bool _isImport;
        /// <summary>
        /// 是否是导入的楼层
        /// </summary>
        public bool IsImport
        {
            get { return _isImport; }
            set { this.SetValue(ref _isImport, value); }
        }

        private int _parentId;
        /// <summary>
        /// 所属的项目
        /// </summary>
        public int ParentId
        {
            get { return _parentId; }
            set { this.SetValue(ref _parentId, value); }
        }
    }

}
