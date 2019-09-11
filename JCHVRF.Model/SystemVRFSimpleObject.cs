using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    /// 简化的室外机系统对象,用于Controller界面以及Report界面
    /// <summary>
    /// 简化的室外机系统对象,用于Controller界面以及Report界面
    /// </summary>
    public class SystemVRFSimpleObject : ModelBase
    {
        public SystemVRFSimpleObject(string id, string text)
        {
            this._id = id;
            this._name = text;
        }

        private string _id;
        /// <summary>
        /// SystemVRF 对象的ID
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        private string _name;
        /// <summary>
        /// 显示名 Out1 ...
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        private string _type;
        /// <summary>
        /// 系统外机的类型，TOP|Horizontal
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { this.SetValue(ref _type, value); }
        }

        /// 实时获取室外机中室内机的数量
        /// <summary>
        /// 实时获取室外机中室内机的数量
        /// </summary>
        /// <param name="thisProject"></param>
        public int GetIndoorCount(Project thisProject)
        {
            int i = 0;
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.SystemID == this.Id)
                    i++;
            }
            return i;
        }
    }
}
