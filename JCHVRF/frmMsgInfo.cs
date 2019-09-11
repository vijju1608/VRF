using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JCHVRF
{
    public partial class frmMsgInfo : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public frmMsgInfo()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMsgInfo_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="msg"></param>
        public void ShowMsg(string msg)
        {
            jctxtMsgInfo.Text = msg;
        }
        /// <summary>
        /// 关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jcbtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
