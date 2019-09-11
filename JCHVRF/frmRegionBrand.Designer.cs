namespace JCHVRF
{
    partial class frmRegionBrand
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRegionBrand));
            this.jclblRegion = new JCBase.UI.JCLabel(this.components);
            this.jclblSubRegion = new JCBase.UI.JCLabel(this.components);
            this.jccmbSubRegion = new JCBase.UI.JCComboBox(this.components);
            this.jccmbRegion = new JCBase.UI.JCComboBox(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.jcbtnOK = new JCBase.UI.JCButton(this.components);
            this.jcbtnCancel = new JCBase.UI.JCButton(this.components);
            this.pnlOK = new System.Windows.Forms.Panel();
            this.jcbtnLogOff = new JCBase.UI.JCButton(this.components);
            this.pnlRegion = new System.Windows.Forms.Panel();
            this.jclblTitle_Region = new JCBase.UI.JCLabel(this.components);
            this.pnlSource_P = new System.Windows.Forms.Panel();
            this.pnlSource = new System.Windows.Forms.Panel();
            this.jclblTitle_Source = new JCBase.UI.JCLabel(this.components);
            this.pnlBrand_P = new System.Windows.Forms.Panel();
            this.pnlBrand = new System.Windows.Forms.Panel();
            this.jclblTitle_Brand = new JCBase.UI.JCLabel(this.components);
            this.pnlOK.SuspendLayout();
            this.pnlRegion.SuspendLayout();
            this.pnlSource_P.SuspendLayout();
            this.pnlBrand_P.SuspendLayout();
            this.SuspendLayout();
            // 
            // jclblRegion
            // 
            this.jclblRegion.AutoSize = true;
            this.jclblRegion.CausesValidation = false;
            this.jclblRegion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jclblRegion.FormatString = null;
            this.jclblRegion.Location = new System.Drawing.Point(67, 54);
            this.jclblRegion.Name = "jclblRegion";
            this.jclblRegion.Size = new System.Drawing.Size(47, 15);
            this.jclblRegion.TabIndex = 0;
            this.jclblRegion.Text = "Region";
            // 
            // jclblSubRegion
            // 
            this.jclblSubRegion.AutoSize = true;
            this.jclblSubRegion.CausesValidation = false;
            this.jclblSubRegion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jclblSubRegion.FormatString = null;
            this.jclblSubRegion.Location = new System.Drawing.Point(67, 95);
            this.jclblSubRegion.Name = "jclblSubRegion";
            this.jclblSubRegion.Size = new System.Drawing.Size(72, 15);
            this.jclblSubRegion.TabIndex = 0;
            this.jclblSubRegion.Text = "Sub Region";
            // 
            // jccmbSubRegion
            // 
            this.jccmbSubRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbSubRegion.ErrorMessage = null;
            this.jccmbSubRegion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jccmbSubRegion.FormattingEnabled = true;
            this.jccmbSubRegion.JCIgnoreSelectItem = null;
            this.jccmbSubRegion.Location = new System.Drawing.Point(172, 91);
            this.jccmbSubRegion.Name = "jccmbSubRegion";
            this.jccmbSubRegion.Size = new System.Drawing.Size(220, 23);
            this.jccmbSubRegion.TabIndex = 2;
            this.jccmbSubRegion.SelectionChangeCommitted += new System.EventHandler(this.jccmbSubRegion_SelectionChangeCommitted);
            // 
            // jccmbRegion
            // 
            this.jccmbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbRegion.ErrorMessage = null;
            this.jccmbRegion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jccmbRegion.FormattingEnabled = true;
            this.jccmbRegion.JCIgnoreSelectItem = null;
            this.jccmbRegion.Location = new System.Drawing.Point(172, 49);
            this.jccmbRegion.Name = "jccmbRegion";
            this.jccmbRegion.Size = new System.Drawing.Size(220, 23);
            this.jccmbRegion.TabIndex = 5;
            this.jccmbRegion.SelectionChangeCommitted += new System.EventHandler(this.jccmbRegion_SelectionChangeCommitted);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 3000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // jcbtnOK
            // 
            this.jcbtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.jcbtnOK.BackColor = System.Drawing.SystemColors.Control;
            this.jcbtnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.jcbtnOK.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jcbtnOK.Location = new System.Drawing.Point(117, 10);
            this.jcbtnOK.Name = "jcbtnOK";
            this.jcbtnOK.Size = new System.Drawing.Size(90, 25);
            this.jcbtnOK.TabIndex = 6;
            this.jcbtnOK.Text = "OK";
            this.jcbtnOK.UseVisualStyleBackColor = false;
            this.jcbtnOK.Click += new System.EventHandler(this.jcbtnOK_Click);
            // 
            // jcbtnCancel
            // 
            this.jcbtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.jcbtnCancel.BackColor = System.Drawing.SystemColors.Control;
            this.jcbtnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.jcbtnCancel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jcbtnCancel.Location = new System.Drawing.Point(278, 10);
            this.jcbtnCancel.Name = "jcbtnCancel";
            this.jcbtnCancel.Size = new System.Drawing.Size(90, 25);
            this.jcbtnCancel.TabIndex = 6;
            this.jcbtnCancel.Text = "Close";
            this.jcbtnCancel.UseVisualStyleBackColor = false;
            this.jcbtnCancel.Click += new System.EventHandler(this.jcbtnCancel_Click);
            // 
            // pnlOK
            // 
            this.pnlOK.BackColor = System.Drawing.Color.White;
            this.pnlOK.Controls.Add(this.jcbtnCancel);
            this.pnlOK.Controls.Add(this.jcbtnOK);
            this.pnlOK.Location = new System.Drawing.Point(0, 432);
            this.pnlOK.Name = "pnlOK";
            this.pnlOK.Size = new System.Drawing.Size(484, 44);
            this.pnlOK.TabIndex = 7;
            // 
            // jcbtnLogOff
            // 
            this.jcbtnLogOff.BackColor = System.Drawing.SystemColors.Control;
            this.jcbtnLogOff.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.jcbtnLogOff.Font = new System.Drawing.Font("微软雅黑", 8.25F);
            this.jcbtnLogOff.Location = new System.Drawing.Point(12, 687);
            this.jcbtnLogOff.Name = "jcbtnLogOff";
            this.jcbtnLogOff.Size = new System.Drawing.Size(90, 25);
            this.jcbtnLogOff.TabIndex = 7;
            this.jcbtnLogOff.Text = "Log off";
            this.jcbtnLogOff.UseVisualStyleBackColor = false;
            this.jcbtnLogOff.Visible = false;
            this.jcbtnLogOff.Click += new System.EventHandler(this.jcbtnLogOff_Click);
            // 
            // pnlRegion
            // 
            this.pnlRegion.BackColor = System.Drawing.Color.White;
            this.pnlRegion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRegion.Controls.Add(this.jclblTitle_Region);
            this.pnlRegion.Controls.Add(this.jccmbRegion);
            this.pnlRegion.Controls.Add(this.jclblRegion);
            this.pnlRegion.Controls.Add(this.jclblSubRegion);
            this.pnlRegion.Controls.Add(this.jccmbSubRegion);
            this.pnlRegion.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRegion.Location = new System.Drawing.Point(0, 0);
            this.pnlRegion.Name = "pnlRegion";
            this.pnlRegion.Size = new System.Drawing.Size(484, 146);
            this.pnlRegion.TabIndex = 8;
            // 
            // jclblTitle_Region
            // 
            this.jclblTitle_Region.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jclblTitle_Region.CausesValidation = false;
            this.jclblTitle_Region.Dock = System.Windows.Forms.DockStyle.Top;
            this.jclblTitle_Region.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.jclblTitle_Region.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_Region.FormatString = null;
            this.jclblTitle_Region.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.jclblTitle_Region.Location = new System.Drawing.Point(0, 0);
            this.jclblTitle_Region.Name = "jclblTitle_Region";
            this.jclblTitle_Region.Size = new System.Drawing.Size(482, 30);
            this.jclblTitle_Region.TabIndex = 1;
            this.jclblTitle_Region.Text = "Region selection";
            this.jclblTitle_Region.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlSource_P
            // 
            this.pnlSource_P.BackColor = System.Drawing.Color.White;
            this.pnlSource_P.Controls.Add(this.pnlSource);
            this.pnlSource_P.Controls.Add(this.jclblTitle_Source);
            this.pnlSource_P.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSource_P.Location = new System.Drawing.Point(0, 292);
            this.pnlSource_P.Name = "pnlSource_P";
            this.pnlSource_P.Size = new System.Drawing.Size(484, 140);
            this.pnlSource_P.TabIndex = 10;
            // 
            // pnlSource
            // 
            this.pnlSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSource.Location = new System.Drawing.Point(0, 30);
            this.pnlSource.Name = "pnlSource";
            this.pnlSource.Size = new System.Drawing.Size(484, 110);
            this.pnlSource.TabIndex = 3;
            // 
            // jclblTitle_Source
            // 
            this.jclblTitle_Source.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jclblTitle_Source.CausesValidation = false;
            this.jclblTitle_Source.Dock = System.Windows.Forms.DockStyle.Top;
            this.jclblTitle_Source.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.jclblTitle_Source.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_Source.FormatString = null;
            this.jclblTitle_Source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.jclblTitle_Source.Location = new System.Drawing.Point(0, 0);
            this.jclblTitle_Source.Name = "jclblTitle_Source";
            this.jclblTitle_Source.Size = new System.Drawing.Size(484, 30);
            this.jclblTitle_Source.TabIndex = 2;
            this.jclblTitle_Source.Text = "Source";
            this.jclblTitle_Source.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlBrand_P
            // 
            this.pnlBrand_P.BackColor = System.Drawing.Color.White;
            this.pnlBrand_P.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pnlBrand_P.Controls.Add(this.pnlBrand);
            this.pnlBrand_P.Controls.Add(this.jclblTitle_Brand);
            this.pnlBrand_P.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBrand_P.Location = new System.Drawing.Point(0, 146);
            this.pnlBrand_P.Name = "pnlBrand_P";
            this.pnlBrand_P.Size = new System.Drawing.Size(484, 146);
            this.pnlBrand_P.TabIndex = 9;
            // 
            // pnlBrand
            // 
            this.pnlBrand.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBrand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBrand.Location = new System.Drawing.Point(0, 30);
            this.pnlBrand.Name = "pnlBrand";
            this.pnlBrand.Size = new System.Drawing.Size(484, 116);
            this.pnlBrand.TabIndex = 3;
            // 
            // jclblTitle_Brand
            // 
            this.jclblTitle_Brand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jclblTitle_Brand.CausesValidation = false;
            this.jclblTitle_Brand.Dock = System.Windows.Forms.DockStyle.Top;
            this.jclblTitle_Brand.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.jclblTitle_Brand.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_Brand.FormatString = null;
            this.jclblTitle_Brand.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.jclblTitle_Brand.Location = new System.Drawing.Point(0, 0);
            this.jclblTitle_Brand.Name = "jclblTitle_Brand";
            this.jclblTitle_Brand.Size = new System.Drawing.Size(484, 30);
            this.jclblTitle_Brand.TabIndex = 2;
            this.jclblTitle_Brand.Text = "Brand selection";
            this.jclblTitle_Brand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmRegionBrand
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(484, 482);
            this.Controls.Add(this.pnlSource_P);
            this.Controls.Add(this.pnlBrand_P);
            this.Controls.Add(this.pnlRegion);
            this.Controls.Add(this.jcbtnLogOff);
            this.Controls.Add(this.pnlOK);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 520);
            this.MinimumSize = new System.Drawing.Size(500, 520);
            this.Name = "frmRegionBrand";
            this.Text = "Region & Brand";
            this.Load += new System.EventHandler(this.frmRegionBrand_Load);
            this.pnlOK.ResumeLayout(false);
            this.pnlRegion.ResumeLayout(false);
            this.pnlRegion.PerformLayout();
            this.pnlSource_P.ResumeLayout(false);
            this.pnlBrand_P.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.JCLabel jclblRegion;
        private JCBase.UI.JCLabel jclblSubRegion;
        private JCBase.UI.JCComboBox jccmbSubRegion;
        private JCBase.UI.JCComboBox jccmbRegion;
        private System.Windows.Forms.Timer timer1;
        private JCBase.UI.JCButton jcbtnOK;
        private JCBase.UI.JCButton jcbtnCancel;
        private System.Windows.Forms.Panel pnlOK;
        private JCBase.UI.JCButton jcbtnLogOff;
        private System.Windows.Forms.Panel pnlRegion;
        private JCBase.UI.JCLabel jclblTitle_Region;
        private System.Windows.Forms.Panel pnlBrand_P;
        private JCBase.UI.JCLabel jclblTitle_Brand;
        private System.Windows.Forms.Panel pnlSource_P;
        private JCBase.UI.JCLabel jclblTitle_Source;
        private System.Windows.Forms.Panel pnlBrand;
        private System.Windows.Forms.Panel pnlSource;
    }
}

