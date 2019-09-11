namespace JCHVRF
{
    partial class frmLoadIndexAddCity
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoadIndexAddCity));
            this.jclblName = new JCBase.UI.JCLabel(this.components);
            this.txtName = new JCBase.UI.JCTextBox(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.jcbtnCancel = new JCBase.UI.VRFButton(this.components);
            this.jcbtnOK = new JCBase.UI.VRFButton(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // jclblName
            // 
            resources.ApplyResources(this.jclblName, "jclblName");
            this.jclblName.CausesValidation = false;
            this.jclblName.FormatString = null;
            this.jclblName.Name = "jclblName";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.BackColor = System.Drawing.Color.MistyRose;
            this.txtName.ErrorMessage = null;
            this.txtName.JCFormatString = null;
            this.txtName.JCMaxValue = null;
            this.txtName.JCMinValue = null;
            this.txtName.JCRegularExpression = null;
            this.txtName.JCValidationType = JCBase.Validation.ValidationType.NOTNULL;
            this.txtName.Name = "txtName";
            this.txtName.RequireValidation = true;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.jcbtnCancel);
            this.panel1.Controls.Add(this.jcbtnOK);
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
            // frmLoadIndexAddCity
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.jclblName);
            this.Name = "frmLoadIndexAddCity";
            this.Load += new System.EventHandler(this.frmLoadIndexAddCity_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private JCBase.UI.JCLabel jclblName;
        private JCBase.UI.JCTextBox txtName;
        private System.Windows.Forms.Panel panel1;
        private JCBase.UI.VRFButton jcbtnOK;
        private JCBase.UI.VRFButton jcbtnCancel;
    }
}
