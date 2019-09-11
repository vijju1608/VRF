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
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        public SettingForm(string name)
        {
            InitializeComponent();
            this.txtName.Text = name;
        }

        private string _settingName;
        /// <summary>
        /// 设置的名称
        /// </summary>
        public string SettingName
        {
            get { return _settingName; }
            set { _settingName = value; }
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtName.Text))
            {
                this._settingName = this.txtName.Text.Trim();
            }
            DialogResult = DialogResult.OK;
            //Close();
        }

    }
}
