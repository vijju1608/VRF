//********************************************************************
// 文件名: FreshAirArea.cs
// 描述: 定义 VRF 项目中的新风区域类
// 作者: Shen Junjie
// 创建时间: 2016-06-24
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    [Serializable]
    public class FreshAirArea : ModelBase
    {
        public FreshAirArea() { }
        public FreshAirArea(int number)
        {
            this._no = number;
            this._parentId = 1;
        }

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

        private string _name;
        /// <summary>
        /// 用户自定义名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
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

        private double _rqCapacityCool;
        /// <summary>
        /// 冷负荷需求kw
        /// </summary>
        public double RqCapacityCool
        {
            get { return _rqCapacityCool; }
            set { this.SetValue(ref _rqCapacityCool, value); }
        }

        private double _rqCapacityHeat;
        /// <summary>
        /// 冷负荷需求kw
        /// </summary>
        public double RqCapacityHeat
        {
            get { return _rqCapacityHeat; }
            set { this.SetValue(ref _rqCapacityHeat, value); }
        }

        private double _sensibleHeat;
        /// <summary>
        /// 显热 Sensible Heat (kW)
        /// </summary>
        public double SensibleHeat
        {
            get { return _sensibleHeat; }
            set { this.SetValue(ref _sensibleHeat, value); }
        }

        private double _airFlow;
        /// <summary>
        /// 风量 Air Flow (m3/h)
        /// </summary>
        public double AirFlow
        {
            get { return _airFlow; }
            set { this.SetValue(ref _airFlow, value); }
        }

        private double _staticPressure;
        /// <summary>
        /// 静压 Static Pressure (Pa)
        /// </summary>
        public double StaticPressure
        {
            get { return _staticPressure; }
            set { this.SetValue(ref _staticPressure, value); }
        }

        private double _freshAir;
        /// <summary>
        /// 新风风量需求
        /// </summary>
        public double FreshAir
        {
            get { return _freshAir; }
            set { this.SetValue(ref _freshAir, value); }
        }

        private bool _isAuto;
        /// <summary>
        /// 是否是自动选型   Add on 20160818 by Lingjia Qiu
        /// </summary>
        public bool IsAuto
        {
            get { return _isAuto; }
            set { this.SetValue(ref _isAuto, value); }
        }
    }


}
