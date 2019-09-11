namespace JCHVRF
{
    partial class frmLoadIndexAddIndex
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoadIndexAddIndex));
            this.jclblRoomType = new JCBase.UI.JCLabel(this.components);
            this.jctxtRoomType = new JCBase.UI.JCTextBox(this.components);
            this.jclblCoolIndex = new JCBase.UI.JCLabel(this.components);
            this.jctxtCoolingIndex = new JCBase.UI.JCTextBox(this.components);
            this.jclblHeatIndex = new JCBase.UI.JCLabel(this.components);
            this.jctxtHeatingIndex = new JCBase.UI.JCTextBox(this.components);
            this.jclblUnitLoadIndex1 = new JCBase.UI.JCLabel(this.components);
            this.jclblUnitLoadIndex2 = new JCBase.UI.JCLabel(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.jcbtnCancel = new JCBase.UI.VRFButton(this.components);
            this.jcbtnOK = new JCBase.UI.VRFButton(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // jclblRoomType
            // 
            this.jclblRoomType.CausesValidation = false;
            resources.ApplyResources(this.jclblRoomType, "jclblRoomType");
            this.jclblRoomType.FormatString = null;
            this.jclblRoomType.Name = "jclblRoomType";
            // 
            // jctxtRoomType
            // 
            this.jctxtRoomType.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtRoomType.ErrorMessage = null;
            resources.ApplyResources(this.jctxtRoomType, "jctxtRoomType");
            this.jctxtRoomType.JCFormatString = null;
            this.jctxtRoomType.JCMaxValue = null;
            this.jctxtRoomType.JCMinValue = null;
            this.jctxtRoomType.JCRegularExpression = null;
            this.jctxtRoomType.JCValidationType = JCBase.Validation.ValidationType.NOTNULL;
            this.jctxtRoomType.Name = "jctxtRoomType";
            this.jctxtRoomType.RequireValidation = true;
            // 
            // jclblCoolIndex
            // 
            this.jclblCoolIndex.CausesValidation = false;
            resources.ApplyResources(this.jclblCoolIndex, "jclblCoolIndex");
            this.jclblCoolIndex.FormatString = null;
            this.jclblCoolIndex.Name = "jclblCoolIndex";
            // 
            // jctxtCoolingIndex
            // 
            this.jctxtCoolingIndex.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtCoolingIndex.ErrorMessage = null;
            resources.ApplyResources(this.jctxtCoolingIndex, "jctxtCoolingIndex");
            this.jctxtCoolingIndex.JCFormatString = null;
            this.jctxtCoolingIndex.JCMaxValue = null;
            this.jctxtCoolingIndex.JCMinValue = null;
            this.jctxtCoolingIndex.JCRegularExpression = null;
            this.jctxtCoolingIndex.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            this.jctxtCoolingIndex.Name = "jctxtCoolingIndex";
            this.jctxtCoolingIndex.RequireValidation = true;
            // 
            // jclblHeatIndex
            // 
            this.jclblHeatIndex.CausesValidation = false;
            resources.ApplyResources(this.jclblHeatIndex, "jclblHeatIndex");
            this.jclblHeatIndex.FormatString = null;
            this.jclblHeatIndex.Name = "jclblHeatIndex";
            // 
            // jctxtHeatingIndex
            // 
            this.jctxtHeatingIndex.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtHeatingIndex.ErrorMessage = null;
            resources.ApplyResources(this.jctxtHeatingIndex, "jctxtHeatingIndex");
            this.jctxtHeatingIndex.JCFormatString = null;
            this.jctxtHeatingIndex.JCMaxValue = null;
            this.jctxtHeatingIndex.JCMinValue = null;
            this.jctxtHeatingIndex.JCRegularExpression = null;
            this.jctxtHeatingIndex.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            this.jctxtHeatingIndex.Name = "jctxtHeatingIndex";
            this.jctxtHeatingIndex.RequireValidation = true;
            // 
            // jclblUnitLoadIndex1
            // 
            resources.ApplyResources(this.jclblUnitLoadIndex1, "jclblUnitLoadIndex1");
            this.jclblUnitLoadIndex1.CausesValidation = false;
            this.jclblUnitLoadIndex1.FormatString = null;
            this.jclblUnitLoadIndex1.Name = "jclblUnitLoadIndex1";
            // 
            // jclblUnitLoadIndex2
            // 
            resources.ApplyResources(this.jclblUnitLoadIndex2, "jclblUnitLoadIndex2");
            this.jclblUnitLoadIndex2.CausesValidation = false;
            this.jclblUnitLoadIndex2.FormatString = null;
            this.jclblUnitLoadIndex2.Name = "jclblUnitLoadIndex2";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.jcbtnCancel);
            this.panel1.Controls.Add(this.jcbtnOK);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // jcbtnCancel
            // 
            resources.ApplyResources(this.jcbtnCancel, "jcbtnCancel");
            this.jcbtnCancel.FlatAppearance.BorderSize = 0;
            this.jcbtnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnCancel.Name = "jcbtnCancel";
            this.jcbtnCancel.UseVisualStyleBackColor = true;
            this.jcbtnCancel.Click += new System.EventHandler(this.jcbtnCancel_Click);
            // 
            // jcbtnOK
            // 
            resources.ApplyResources(this.jcbtnOK, "jcbtnOK");
            this.jcbtnOK.FlatAppearance.BorderSize = 0;
            this.jcbtnOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnOK.Name = "jcbtnOK";
            this.jcbtnOK.UseVisualStyleBackColor = true;
            this.jcbtnOK.Click += new System.EventHandler(this.jcbtnOK_Click);
            // 
            // frmLoadIndexAddIndex
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.jclblUnitLoadIndex2);
            this.Controls.Add(this.jclblUnitLoadIndex1);
            this.Controls.Add(this.jctxtHeatingIndex);
            this.Controls.Add(this.jctxtCoolingIndex);
            this.Controls.Add(this.jctxtRoomType);
            this.Controls.Add(this.jclblHeatIndex);
            this.Controls.Add(this.jclblCoolIndex);
            this.Controls.Add(this.jclblRoomType);
            this.MaximizeBox = false;
            this.Name = "frmLoadIndexAddIndex";
            this.Load += new System.EventHandler(this.frmLoadIndexAddIndex_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private JCBase.UI.JCLabel jclblRoomType;
        private JCBase.UI.JCTextBox jctxtRoomType;
        private JCBase.UI.JCLabel jclblCoolIndex;
        private JCBase.UI.JCTextBox jctxtCoolingIndex;
        private JCBase.UI.JCLabel jclblHeatIndex;
        private JCBase.UI.JCTextBox jctxtHeatingIndex;
        private JCBase.UI.JCLabel jclblUnitLoadIndex1;
        private JCBase.UI.JCLabel jclblUnitLoadIndex2;
        private System.Windows.Forms.Panel panel1;
        private JCBase.UI.VRFButton jcbtnCancel;
        private JCBase.UI.VRFButton jcbtnOK;
    }
}
