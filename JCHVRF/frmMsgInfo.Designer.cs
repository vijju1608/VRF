namespace JCHVRF
{
    partial class frmMsgInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMsgInfo));
            this.jcbtnClose = new JCBase.UI.VRFButton(this.components);
            this.jctxtMsgInfo = new JCBase.UI.JCTextBox(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // jcbtnClose
            // 
            resources.ApplyResources(this.jcbtnClose, "jcbtnClose");
            this.jcbtnClose.FlatAppearance.BorderSize = 0;
            this.jcbtnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnClose.Name = "jcbtnClose";
            this.jcbtnClose.UseVisualStyleBackColor = true;
            this.jcbtnClose.Click += new System.EventHandler(this.jcbtnClose_Click);
            // 
            // jctxtMsgInfo
            // 
            resources.ApplyResources(this.jctxtMsgInfo, "jctxtMsgInfo");
            this.jctxtMsgInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.jctxtMsgInfo.ErrorMessage = null;
            this.jctxtMsgInfo.JCFormatString = null;
            this.jctxtMsgInfo.JCMaxValue = null;
            this.jctxtMsgInfo.JCMinValue = null;
            this.jctxtMsgInfo.JCRegularExpression = null;
            this.jctxtMsgInfo.JCValidationType = JCBase.Validation.ValidationType.NOTNULL;
            this.jctxtMsgInfo.Name = "jctxtMsgInfo";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.jcbtnClose);
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.jctxtMsgInfo);
            this.panel2.Name = "panel2";
            // 
            // frmMsgInfo
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "frmMsgInfo";
            this.Load += new System.EventHandler(this.frmMsgInfo_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.VRFButton jcbtnClose;
        private JCBase.UI.JCTextBox jctxtMsgInfo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}