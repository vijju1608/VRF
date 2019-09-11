using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class ODUPartload
    {
        /// <summary>
        /// 制冷/制热
        /// </summary>
        public string Condition { get; set; }
        
        public string ShortModel { get; set; }

        /// <summary>
        /// 室外机干球温度
        /// </summary>
        public int OutDB { get; set; }

        /// <summary>
        /// 室内机湿球温度
        /// </summary>
        public int InWB { get; set; }

        /// <summary>
        /// 温度修正容量
        /// </summary>
        public double TC { get; set; }

        /// <summary>
        /// 输入功率
        /// </summary>
        public double PI { get; set; }

        /// <summary>
        /// 配比率
        /// </summary>
        public double Ratio { get; set; }

        /// <summary>
        /// 水流速
        /// </summary>
        public double FlowRate { get; set; }
    }
}
