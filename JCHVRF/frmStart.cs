using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using JCBase.Utility;

using Registr;

namespace JCHVRF
{
    public partial class frmStart : JCBase.UI.JCForm
    {

        public bool ISVALID_Region = false;
        public bool ISVALID_Date = false;

        public frmStart()
        {
            InitializeComponent();

            panel1.Visible = true;            

            // TODO:正式使用时需要放开
            if (Registration.IsValid())
            {
                panel1.Visible = false;
                ISVALID_Region = true;
                ISVALID_Date = true;
            }
            
        }

        private void frmStart_Load(object sender, EventArgs e)
        {
            ////如果是非中文系统，将数字和日期格式强制设为英美模式。 20180417 by Yunxiao Lin
            //if (CultureInfo.CurrentCulture.Name == "zh-CN")
            //{ }
            //else
            //{
            //    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            //    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            //}
            this.JCSetLanguage();
            this.lblVersion.Text = "Ver." + MyConfig.Version;   //添加版本号   add on 20180605 by Vince
            if (!Registration.IsValid())
                this.lblVersion.Visible = true;
        }

        private void btnActivation_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtActivation.Text))
            {
                this.txtActivation.Focus();
                return;
            }
            Registration.DoValidation(this.txtActivation.Text);

            if (Registration.IsValid())
            {
                ISVALID_Region = true;
                ISVALID_Date = true;
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                this.txtActivation.Focus();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
