using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using JCBase.UI;

namespace JCHVRF
{
    public partial class frmLoadIndexAddCity : JCBase.UI.JCForm
    {
        /// <summary>
        /// for Add City
        /// </summary>
        public frmLoadIndexAddCity()
        {
            InitializeComponent();
            this.JCSetLanguage();
            this.JCFormMode = FormMode.NEW;
        }

        /// <summary>
        /// for Edit City
        /// </summary>
        /// <param name="cityname"></param>
        public frmLoadIndexAddCity(string cityname)
        {
            InitializeComponent();
            this.JCSetLanguage();
            this.JCFormMode = FormMode.EDIT;
            this._cityName = cityname;
        }

        private string _cityName;

        public string CityName
        {
            get { return _cityName; }
        }

        private void frmLoadIndexAddCity_Load(object sender, EventArgs e)
        {
            this.JCCallValidationManager = true;
            this.txtName.Text = this._cityName;
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (!this.JCValidateForm())
                return;
            this._cityName = txtName.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            this._cityName = "";
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
