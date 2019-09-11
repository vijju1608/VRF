//********************************************************************
// 文件名: ProductType.cs
// 描述: 定义 VRF 项目中的产品类型类
// 作者: clh
// 创建时间: 2016-2-17
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class MyProductType : ModelBase
    {
        string _brandCode;
        /// <summary>
        /// 品牌代码
        /// </summary>
        public string BrandCode
        {
            get { return _brandCode; }
            set { this.SetValue(ref _brandCode, value); }
        }

        string _factoryCode;
        /// <summary>
        /// 工厂代码
        /// </summary>
        public string FactoryCode
        {
            get { return _factoryCode; }
            set { this.SetValue(ref _factoryCode, value); }
        }

        string _regionCode;
        /// <summary>
        /// 区域代码，此处指子区域
        /// </summary>
        public string RegionCode
        {
            get { return _regionCode; }
            set { this.SetValue(ref _regionCode, value); }
        }

        string _productType;
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType
        {
            get { return _productType; }
            set { this.SetValue(ref _productType, value); }
        }

        string _series;
        /// <summary>
        /// 系列名称
        /// </summary>
        public string Series
        {
            get { return _series; }
            set { this.SetValue(ref _series, value); }
        }

        int _minCoolingDB;
        /// <summary>
        /// 最小制冷干球温度
        /// </summary>
        public int MinCoolingDB
        {
            get { return _minCoolingDB; }
            set { this.SetValue(ref _minCoolingDB, value); }
        }

        int _maxCoolingDB;
        /// <summary>
        /// 最大制冷干球温度
        /// </summary>
        public int MaxCoolingDB
        {
            get { return _maxCoolingDB; }
            set { this.SetValue(ref _maxCoolingDB, value); }
        }

        int _minHeatingWB;
        /// <summary>
        /// 最小制热湿球温度
        /// </summary>
        public int MinHeatingWB
        {
            get { return _minHeatingWB; }
            set { this.SetValue(ref _minHeatingWB, value); }
        }

        int _maxHeatingWB;
        /// <summary>
        /// 最大制热湿球温度
        /// </summary>
        public int MaxHeatingWB
        {
            get { return _maxHeatingWB; }
            set { this.SetValue(ref _maxHeatingWB, value); }
        }

        /// <summary>
        /// 增大或减小1st/2nd Multi-kit的规则标识
        /// </summary>
        public string L2SizeDownRule
        {
            get;
            set;
        }
    }
}
