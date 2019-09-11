using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JCHVRF.Model;

namespace JCHVRF
{
    public partial class ucDropControlSystem : UserControl
    {
        public ucDropControlSystem()
        {
            InitializeComponent();
            this.Height = 400;
            this.Dock = DockStyle.Top;
        }

        public ucDropControlSystem(string id)
        {
            InitializeComponent();
            this._controlSystemID = id;

            this.Height = 400;
            this.Dock = DockStyle.Top;

        }

        public void HideDeleteLine()
        {
            this.pnlSpan_ControlSystem.Visible = false;
        }

        public void ShowDeleteLine()
        {
            this.pnlSpan_ControlSystem.Visible = true;
        }

        private string _controlSystemID;
        public string ControlSystemID
        {
            get { return _controlSystemID; }
        }

        /// 将当前控制器系统控件关联到指定的ControlSystem对象
        /// <summary>
        /// 将当前控制器系统控件关联到指定的ControlSystem对象
        /// </summary>
        /// <param name="controlSystemID"></param>
        public void BindControlSystem(string controlSystemID)
        {
            this._controlSystemID = controlSystemID;
        }

        public bool IsActive()
        {
            foreach(Control item in this.Controls)
            {
                if (item is ucDropControlGroup && (item as ucDropControlGroup).IsActive)
                    return true;
                if (item is ucDropController && (item as ucDropController).IsActive)
                    return true;
            }
            return false;
        }

        // 暂不考虑Control System操作，先保留控件，20141029 clh
        ///// <summary>
        ///// 获取当前Control System中Controller的总数，含Group中的
        ///// </summary>
        ///// <returns></returns>
        //public int GetControllerQty()
        //{
        //    int qty = 0;
        //    foreach (Control item in this.Controls)
        //    {
        //        if (item is ucDropController && (item as ucDropController).GetControllerImage() != null)
        //        {
        //            qty++;
        //        }
        //        else if (item is ucDropControlGroup && (item as ucDropControlGroup).IsActive)
        //        {
        //            int qtyGroup = (item as ucDropControlGroup).GetControllerQty();
        //            qty += qtyGroup;
        //        }
        //    }
        //    return qty;
        //}

        ///// 获取当前控件中指定控制器类型的ucDropController控件的数量，用于验证
        ///// <summary>
        ///// 获取当前控件中指定控制器类型的ucDropController控件的数量，用于验证,需求更改
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public int GetControllerQty(ControllerType type)
        //{
        //    int qty = 0;
        //    foreach (Control item in this.Controls)
        //    {
        //        if (item is ucDropController && (item as ucDropController).GetControllerImage() != null)
        //        {
        //            if ((item as ucDropController).GetControllerTyep() == type)
        //                qty++;
        //        }
        //        else if (item is ucDropControlGroup && (item as ucDropControlGroup).IsActive)
        //        {
        //            int qtyGroup = (item as ucDropControlGroup).GetControllerQty(type);
        //            qty += qtyGroup;
        //        }
        //    }
        //    return qty;
        //}

        ///// <summary>
        ///// 获取当前Control System中Controller的总数，不包含Group中的
        ///// </summary>
        ///// <returns></returns>
        //public int GetControllerQtyWithoutGroup()
        //{
        //    int qty = 0;
        //    foreach (Control item in this.Controls)
        //    {
        //        if (item is ucDropController && (item as ucDropController).GetControllerImage() != null)
        //        {
        //            qty++;
        //        }
        //    }
        //    return qty;
        //}
    }
}
