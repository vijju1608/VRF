//********************************************************************
// 文件名: RoomLoadIndex.cs
// 描述: 管理不同城市不同房间类型的制冷、制热指标数据
// 作者: clh
// 创建时间: 2013-03-14
// 修改历史: 
// 2016-1-29 迁入JCHVRF
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.OleDb;
using System.Collections;

namespace JCHVRF.Model
{
    /// <summary>
    /// 管理不同城市不同房间类型的制冷、制热指标数据
    /// </summary>
    public class RoomLoadIndex : ModelBase
    {
        public RoomLoadIndex(string lang)
        {
            this._language = lang;
        }

        public RoomLoadIndex(string city, string lang)
        {
            this._city = city;
            this._language = lang;
        }

        public RoomLoadIndex(string city, string rType, string lang)
        {
            this._city = city;
            this._roomType = rType;
            this._language = lang;
        }

        private string _city;
        /// <summary>
        /// 所属城市
        /// </summary>
        public string City
        {
            get { return _city; }
            set { this.SetValue(ref _city, value); }
        }

        private string _roomType;
        /// <summary>
        /// 房间类型
        /// </summary>
        public string RoomType
        {
            get { return _roomType; }
            set { this.SetValue(ref _roomType, value); }
        }

        private double _coolingIndex;
        /// <summary>
        /// 制冷指标
        /// </summary>
        public double CoolingIndex
        {
            get { return _coolingIndex; }
            set { this.SetValue(ref _coolingIndex, value); }
        }

        private double _heatingIndex;
        /// <summary>
        /// 制热指标
        /// </summary>
        public double HeatingIndex
        {
            get { return _heatingIndex; }
            set { this.SetValue(ref _heatingIndex, value); }
        }

        private string _language;
        /// <summary>
        /// 语言代码
        /// </summary>
        public string Language
        {
            get { return _language; }
            set { this.SetValue(ref _language, value); }
        }
    }
}
