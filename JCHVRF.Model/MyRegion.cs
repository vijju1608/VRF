//********************************************************************
// 文件名: Region.cs
// 描述: 定义 VRF 项目中的区域类
// 作者: clh
// 创建时间: 2016-2-15
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class MyRegion : ModelBase
    {
        string _code;
        /// <summary>
        /// 区域代码
        /// </summary>
        public string Code
        {
            get { return _code; }
            set { this.SetValue(ref _code, value); }
        }

        string _region;
        /// <summary>
        /// 区域描述，界面显示内容
        /// </summary>
        public string Region
        {
            get { return _region; }
            set { this.SetValue(ref _region, value); }
        }

        string _parentRegionCode;
        /// <summary>
        /// 父区域代码
        /// </summary>
        public string ParentRegionCode
        {
            get { return _parentRegionCode; }
            set { this.SetValue(ref _parentRegionCode, value); }
        }

        string _registPassword;
        /// <summary>
        /// 注册密码
        /// </summary>
        public string RegistPassword
        {
            get { return _registPassword; }
            set { this.SetValue(ref _registPassword, value); }
        }

        string _pricePassword;
        /// <summary>
        /// 价格密码
        /// </summary>
        public string PricePassword
        {
            get { return _pricePassword; }
            set { this.SetValue(ref _pricePassword, value); }
        }

        string _yorkPassword;
        /// <summary>
        /// 约克权限密码
        /// </summary>
        public string YorkPassword
        {
            get { return _yorkPassword; }
            set { this.SetValue(ref _yorkPassword, value); }
        }

        string _hitachiPassword;
        /// <summary>
        /// 日立权限密码
        /// </summary>
        public string HitachiPassword
        {
            get { return _hitachiPassword; }
            set { this.SetValue(ref _hitachiPassword, value); }
        }
    }
}
