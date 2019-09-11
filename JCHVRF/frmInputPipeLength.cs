using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

//using Lassalle.Flow;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Utility;
using JCBase.UI;
using JCHVRF.VRFMessage;
namespace JCHVRF 
{
    public partial class frmInputPipeLength : JCBase.UI.JCForm
    {
        public frmInputPipeLength()
        {
            InitializeComponent();
        }

        public frmInputPipeLength(MyLink link, SystemVRF sysItem)
        {
            InitializeComponent();
            this.mylink = link;
            this._sysItem = sysItem;
        }

        MyLink mylink;
        string ut_length;

        private double _elbowQty;
        /// <summary>
        /// 弯头数量
        /// </summary>
        public double ElbowQty
        {
            get { return _elbowQty; }
            set { _elbowQty = value; }
        }

        private double _oilTrapQty;
        /// <summary>
        /// 油弯数量
        /// </summary>
        public double OilTrapQty
        {
            get { return _oilTrapQty; }
            set { _oilTrapQty = value; }
        }

        private double _length;
        /// <summary>
        /// 连接管长度
        /// </summary>
        public double Length
        {
            get { return _length; }
            set { _length = value; }
        }

        private double _valveLength;
        /// 电子膨胀阀管长 add on 20160616 by Yunxiao Lin
        /// <summary>
        /// 电子膨胀阀管长
        /// </summary>
        public double ValveLength
        {
            get { return _valveLength; }
            set { _valveLength = value; }
        }

        private bool _isvalve;
        /// <summary>
        /// 是否有电子膨胀阀
        /// </summary>
        public bool IsValve
        {
            get { return _isvalve; }
            set{ _isvalve = value; }
        }

        private bool _applyToAll;
        /// <summary>
        /// 是否应用到所有连接管
        /// </summary>
        public bool ApplyToAll
        {
            get { return _applyToAll; }
            set { _applyToAll = value; }
        }

        //sysItem变量用于传递系统相关参数
        private SystemVRF _sysItem;

        private void frmInputPipeLength_Load(object sender, EventArgs e)
        {
            if (_isvalve)
            {
                this.jclblUnitValveLength.Visible = true;
                this.jclblValveLength.Visible = true;
                this.jctxtValveLength.Visible = true;
            }

            //Hide elbows and oil tramps setting and Q'ty=0. Only for LA_MMA, LA_PERU, LA_SC, LA_BV
            //add by Shen Junjie on 2018/02/01
            //Modified by Yunxiao Lin 2018/02/12. When region is LA and Series is'nt Residential VRF, hide oil tramps Q'ty setting. 
            //Hide the oil trap for EU.  add by Shen Junjie on 2018/4/8
            string regionCode = Project.CurrentProject.RegionCode;
            string subRegionCode = Project.CurrentProject.SubRegionCode;
            string series = "";
            if (_sysItem != null && _sysItem.OutdoorItem != null)
            {
                series = _sysItem.OutdoorItem.Series;
            }
            if (regionCode=="EU_W" || regionCode == "EU_S" || regionCode=="EU_E" ||
                ((subRegionCode == "LA_MMA" ||
                    subRegionCode == "LA_PERU" ||
                    subRegionCode == "LA_SC" ||
                    subRegionCode == "LA_BV") && 
                    !series.Contains("Residential")))
            {
                this.jclblOilTrapQty.Visible = false;
                this.jctxtOilTrapQty.Visible = false;
            }

            this.JCSetLanguage();
            this.JCCallValidationManager = true;
            ut_length=SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            this.jclblUnitLength.Text = ut_length;
            this.jclblUnitValveLength.Text = ut_length;
            BindToControl();
        }

        private void BindToControl()
        {
            this.jctxtElbowQty.Text = mylink.ElbowQty.ToString();
            this.jctxtOilTrapQty.Text = mylink.OilTrapQty.ToString();
            this.jctxtLength.Text = Unit.ConvertToControl(mylink.Length, UnitType.LENGTH_M, ut_length).ToString("n2"); //20141218的Natallia的反馈邮件
            this.jctxtValveLength.Text = Unit.ConvertToControl(mylink.ValveLength, UnitType.LENGTH_M, ut_length).ToString("n2");
        }

        private void BindToSource()
        {
            double len = Convert.ToDouble(this.jctxtLength.Text);
            double valvelen = Convert.ToDouble(this.jctxtValveLength.Text);
            this._elbowQty = Convert.ToDouble(this.jctxtElbowQty.Text);
            this._oilTrapQty = Convert.ToDouble(this.jctxtOilTrapQty.Text);
            this._length = Unit.ConvertToSource(len, UnitType.LENGTH_M, ut_length);
            this._valveLength = Unit.ConvertToSource(valvelen, UnitType.LENGTH_M, ut_length);
            this._applyToAll = this.jccbxApplyToAll.Checked;
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (this.JCValidateForm())
            {
                BindToSource();
                DialogResult = DialogResult.OK;
            }
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
