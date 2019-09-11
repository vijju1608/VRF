using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Utility;

namespace JCHVRF
{
    public partial class frmLoadIndexAddIndex : JCBase.UI.JCForm
    {
        string utLoadIndex = "";

        /// <summary>
        /// for Add Index
        /// </summary>
        /// <param name="city"></param>
        public frmLoadIndexAddIndex(string city)
        {
            InitializeComponent();
            this.JCSetLanguage();
            this.JCFormMode = FormMode.NEW;
            this._loadIndexItem = new RoomLoadIndex(this.JCCurrentLanguage);
            this._loadIndexItem.City = city;
        }

        /// <summary>
        /// for Edit Index
        /// </summary>
        /// <param name="item"></param>
        public frmLoadIndexAddIndex(RoomLoadIndex item)
        {
            InitializeComponent();
            this.JCSetLanguage();
            this.JCFormMode = FormMode.EDIT;
            this._loadIndexItem = item;
            this.jctxtRoomType.ReadOnly = true;
            this.jctxtRoomType.Enabled = false;
        }

        private RoomLoadIndex _loadIndexItem;
        /// <summary>
        /// RoomLoadInex 对象
        /// </summary>
        public RoomLoadIndex LoadIndexItem
        {
            get { return _loadIndexItem; }
        }

        private void frmLoadIndexAddIndex_Load(object sender, EventArgs e)
        {
            this.JCCallValidationManager = true;
            BindUnit();
            BindSourceToControl();
        }

        private void BindUnit()
        {
            utLoadIndex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
            jclblUnitLoadIndex1.Text = utLoadIndex;
            jclblUnitLoadIndex2.Text = utLoadIndex;
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (!this.JCValidateForm())
                return;
            BindControlToSource();
            this.DialogResult = DialogResult.OK;
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BindSourceToControl()
        {
            this.jctxtRoomType.Text = _loadIndexItem.RoomType;
            this.jctxtCoolingIndex.Text = Unit.ConvertToControl(_loadIndexItem.CoolingIndex, UnitType.LOADINDEX, utLoadIndex).ToString("n2");
            this.jctxtHeatingIndex.Text = Unit.ConvertToControl(_loadIndexItem.HeatingIndex, UnitType.LOADINDEX, utLoadIndex).ToString("n2");
            if (this.JCFormMode == FormMode.EDIT)
                jctxtRoomType.Enabled = false;
        }

        private void BindControlToSource()
        {
            _loadIndexItem.RoomType = jctxtRoomType.Text;
            _loadIndexItem.CoolingIndex = Unit.ConvertToSource(double.Parse(jctxtCoolingIndex.Text), UnitType.LOADINDEX, utLoadIndex);
            _loadIndexItem.HeatingIndex = Unit.ConvertToSource(double.Parse(jctxtHeatingIndex.Text), UnitType.LOADINDEX, utLoadIndex);
        }
    }
}
