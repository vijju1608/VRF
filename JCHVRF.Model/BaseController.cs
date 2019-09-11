//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace JCHVRF.Model
//{
//    /// 基础组件
//    /// <summary>
//    /// 基础组件
//    /// </summary>
//    [Serializable]
//    public class BaseController
//    {
//        public BaseController()
//        {
//            this._id = Guid.NewGuid().ToString("N");
//        }

//        private string _id;
//        /// <summary>
//        /// 唯一编号
//        /// </summary>
//        public string Id
//        {
//            get { return _id; }
//        }

//        private string _name;
//        /// <summary>
//        /// 名称
//        /// </summary>
//        public string Name
//        {
//            get { return _name; }
//        }
        
//        public void SetName(string name) { this._name = name; }

//    }

//    /// 控制器组的类型
//    /// <summary>
//    /// 控制器组的类型 MODBUS | BACNET
//    /// </summary>
//    public enum ControllerLayoutType { BACNET, MODBUS }

//    /// 控制器类型
//    /// <summary>
//    /// 控制器类型 ONOFF | TOUCH
//    /// </summary>
//    public enum ControllerType
//    {
//        /// <summary>
//        /// 开关
//        /// </summary>
//        ONOFF,
//        /// <summary>
//        /// 触摸屏
//        /// </summary>
//        CC
//    }

//    /// 空调外机出风口类型 TOP | HORIZONTAL
//    /// <summary>
//    /// 空调外机出风口类型 TOP | HORIZONTAL
//    /// </summary>
//    public enum AirDirectionType
//    {
//        /// <summary>
//        /// 侧出风
//        /// </summary>
//        HORIZONTAL,
//        /// <summary>
//        /// 上出风
//        /// </summary>
//        TOP
//    }
//}
