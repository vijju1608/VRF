namespace JCHVRF
{
    partial class frmStart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStart));
            this.lblActivation = new System.Windows.Forms.Label();
            this.txtActivation = new JCBase.UI.JCTextBox(this.components);
            this.btnActivation = new JCBase.UI.JCButton(this.components);
            this.btnCancel = new JCBase.UI.JCButton(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblVersion = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblActivation
            // 
            resources.ApplyResources(this.lblActivation, "lblActivation");
            this.lblActivation.BackColor = System.Drawing.Color.Transparent;
            this.lblActivation.ForeColor = System.Drawing.Color.White;
            this.lblActivation.Name = "lblActivation";
            // 
            // txtActivation
            // 
            this.txtActivation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtActivation.ErrorMessage = null;
            resources.ApplyResources(this.txtActivation, "txtActivation");
            this.txtActivation.JCFormatString = null;
            this.txtActivation.JCMaxValue = null;
            this.txtActivation.JCMinValue = null;
            this.txtActivation.JCRegularExpression = null;
            this.txtActivation.JCValidationType = JCBase.Validation.ValidationType.NOTNULL;
            this.txtActivation.Name = "txtActivation";
            // 
            // btnActivation
            // 
            this.btnActivation.BackColor = System.Drawing.SystemColors.InactiveCaption;
            resources.ApplyResources(this.btnActivation, "btnActivation");
            this.btnActivation.Name = "btnActivation";
            this.btnActivation.UseVisualStyleBackColor = false;
            this.btnActivation.Click += new System.EventHandler(this.btnActivation_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.InactiveCaption;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.btnActivation);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.lblActivation);
            this.panel1.Controls.Add(this.txtActivation);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // lblVersion
            // 
            resources.ApplyResources(this.lblVersion, "lblVersion");
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.ForeColor = System.Drawing.Color.White;
            this.lblVersion.Name = "lblVersion";
            // 
            // frmStart
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.Color.SeaShell;
            this.CausesValidation = false;
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmStart";
            this.Load += new System.EventHandler(this.frmStart_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblActivation;
        private JCBase.UI.JCTextBox txtActivation;
        private JCBase.UI.JCButton btnActivation;
        private JCBase.UI.JCButton btnCancel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblVersion;
    }
}