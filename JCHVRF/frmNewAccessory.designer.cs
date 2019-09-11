namespace JCHVRF
{
    partial class frmNewAccessory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNewAccessory));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.dgvAvailableItems = new System.Windows.Forms.DataGridView();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.labTitle = new System.Windows.Forms.Label();
            this.jclMaxNumber = new JCBase.UI.JCLabel(this.components);
            this.panelDetail = new System.Windows.Forms.Panel();
            this.jclabModelType = new JCBase.UI.JCLabel(this.components);
            this.jclabModelTypeName = new JCBase.UI.JCLabel(this.components);
            this.jccmbAccessoryModel = new JCBase.UI.JCComboBox(this.components);
            this.btnCancel = new JCBase.UI.VRFButton(this.components);
            this.btnOK = new JCBase.UI.VRFButton(this.components);
            this.txtNumber = new JCBase.UI.JCTextBox(this.components);
            this.jclNumber = new JCBase.UI.JCLabel(this.components);
            this.txtMaxNumber = new JCBase.UI.JCTextBox(this.components);
            this.panelControl = new System.Windows.Forms.Panel();
            this.cklistIndoor = new System.Windows.Forms.CheckedListBox();
            this.btnControlOK = new JCBase.UI.VRFButton(this.components);
            this.btnControlCancel = new JCBase.UI.VRFButton(this.components);
            this.jcChkControl = new JCBase.UI.JCCheckBox(this.components);
            this.jccmbControlType = new JCBase.UI.JCComboBox(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.jclabTitle = new JCBase.UI.JCLabel(this.components);
            this.pnlHelpInfo_Project = new System.Windows.Forms.Panel();
            this.jclblHelp_Project_2 = new JCBase.UI.JCLabel(this.components);
            this.pnlHelpInfo_Icon = new System.Windows.Forms.Panel();
            this.line_HelpInfo = new System.Windows.Forms.Label();
            this.lbl_HelpInfo = new System.Windows.Forms.Label();
            this.pb_HelpInfo = new System.Windows.Forms.PictureBox();
            this.jclblHelp_Project_1 = new JCBase.UI.JCLabel(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableItems)).BeginInit();
            this.panelTitle.SuspendLayout();
            this.panelDetail.SuspendLayout();
            this.panelControl.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnlHelpInfo_Project.SuspendLayout();
            this.pnlHelpInfo_Icon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_HelpInfo)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // dgvAvailableItems
            // 
            this.dgvAvailableItems.AllowUserToAddRows = false;
            this.dgvAvailableItems.AllowUserToDeleteRows = false;
            this.dgvAvailableItems.AllowUserToResizeColumns = false;
            this.dgvAvailableItems.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgvAvailableItems.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAvailableItems.BackgroundColor = System.Drawing.Color.White;
            this.dgvAvailableItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvAvailableItems.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(112)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(112)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAvailableItems.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dgvAvailableItems, "dgvAvailableItems");
            this.dgvAvailableItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAvailableItems.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAvailableItems.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvAvailableItems.EnableHeadersVisualStyles = false;
            this.dgvAvailableItems.Name = "dgvAvailableItems";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAvailableItems.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvAvailableItems.RowHeadersVisible = false;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.dgvAvailableItems.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvAvailableItems.RowTemplate.Height = 30;
            this.dgvAvailableItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAvailableItems.ShowEditingIcon = false;
            this.dgvAvailableItems.TabStop = false;
            this.dgvAvailableItems.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvAvailableItems_CellBeginEdit);
            this.dgvAvailableItems.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvAvailableItems_CellPainting);
            this.dgvAvailableItems.RowLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAvailableItems_RowLeave);
            // 
            // panelTitle
            // 
            this.panelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(88)))), ((int)(((byte)(131)))));
            this.panelTitle.Controls.Add(this.labTitle);
            resources.ApplyResources(this.panelTitle, "panelTitle");
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelDetail_MouseDown);
            this.panelTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelDetail_MouseMove);
            // 
            // labTitle
            // 
            resources.ApplyResources(this.labTitle, "labTitle");
            this.labTitle.ForeColor = System.Drawing.Color.White;
            this.labTitle.Name = "labTitle";
            // 
            // jclMaxNumber
            // 
            resources.ApplyResources(this.jclMaxNumber, "jclMaxNumber");
            this.jclMaxNumber.CausesValidation = false;
            this.jclMaxNumber.FormatString = null;
            this.jclMaxNumber.Name = "jclMaxNumber";
            // 
            // panelDetail
            // 
            this.panelDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDetail.Controls.Add(this.jclabModelType);
            this.panelDetail.Controls.Add(this.jclabModelTypeName);
            this.panelDetail.Controls.Add(this.jccmbAccessoryModel);
            this.panelDetail.Controls.Add(this.btnCancel);
            this.panelDetail.Controls.Add(this.btnOK);
            this.panelDetail.Controls.Add(this.txtNumber);
            this.panelDetail.Controls.Add(this.jclNumber);
            this.panelDetail.Controls.Add(this.txtMaxNumber);
            this.panelDetail.Controls.Add(this.jclMaxNumber);
            this.panelDetail.Controls.Add(this.panelTitle);
            resources.ApplyResources(this.panelDetail, "panelDetail");
            this.panelDetail.Name = "panelDetail";
            this.panelDetail.Paint += new System.Windows.Forms.PaintEventHandler(this.panelDetail_Paint);
            this.panelDetail.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelDetail_MouseDown);
            this.panelDetail.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelDetail_MouseMove);
            // 
            // jclabModelType
            // 
            resources.ApplyResources(this.jclabModelType, "jclabModelType");
            this.jclabModelType.CausesValidation = false;
            this.jclabModelType.FormatString = null;
            this.jclabModelType.Name = "jclabModelType";
            // 
            // jclabModelTypeName
            // 
            this.jclabModelTypeName.BackColor = System.Drawing.Color.White;
            this.jclabModelTypeName.CausesValidation = false;
            resources.ApplyResources(this.jclabModelTypeName, "jclabModelTypeName");
            this.jclabModelTypeName.ForeColor = System.Drawing.Color.Black;
            this.jclabModelTypeName.FormatString = null;
            this.jclabModelTypeName.Name = "jclabModelTypeName";
            // 
            // jccmbAccessoryModel
            // 
            this.jccmbAccessoryModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbAccessoryModel.ErrorMessage = null;
            resources.ApplyResources(this.jccmbAccessoryModel, "jccmbAccessoryModel");
            this.jccmbAccessoryModel.FormattingEnabled = true;
            this.jccmbAccessoryModel.JCIgnoreSelectItem = null;
            this.jccmbAccessoryModel.Name = "jccmbAccessoryModel";
            this.jccmbAccessoryModel.SelectionChangeCommitted += new System.EventHandler(this.jccmbAccessoryModel_SelectionChangeCommitted);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.FlatAppearance.BorderSize = 0;
            this.btnOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtNumber
            // 
            this.txtNumber.BackColor = System.Drawing.Color.MistyRose;
            this.txtNumber.ErrorMessage = null;
            resources.ApplyResources(this.txtNumber, "txtNumber");
            this.txtNumber.JCFormatString = null;
            this.txtNumber.JCMaxValue = null;
            this.txtNumber.JCMinValue = null;
            this.txtNumber.JCRegularExpression = null;
            this.txtNumber.JCValidationType = JCBase.Validation.ValidationType.RANGE;
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.RequireValidation = true;
            this.txtNumber.TextChanged += new System.EventHandler(this.txtNumber_TextChanged);
            // 
            // jclNumber
            // 
            resources.ApplyResources(this.jclNumber, "jclNumber");
            this.jclNumber.CausesValidation = false;
            this.jclNumber.FormatString = null;
            this.jclNumber.Name = "jclNumber";
            // 
            // txtMaxNumber
            // 
            this.txtMaxNumber.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.txtMaxNumber, "txtMaxNumber");
            this.txtMaxNumber.ErrorMessage = null;
            this.txtMaxNumber.JCFormatString = null;
            this.txtMaxNumber.JCMaxValue = null;
            this.txtMaxNumber.JCMinValue = null;
            this.txtMaxNumber.JCRegularExpression = null;
            this.txtMaxNumber.JCValidationType = JCBase.Validation.ValidationType.NOTNULL;
            this.txtMaxNumber.Name = "txtMaxNumber";
            // 
            // panelControl
            // 
            this.panelControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelControl.Controls.Add(this.cklistIndoor);
            this.panelControl.Controls.Add(this.btnControlOK);
            this.panelControl.Controls.Add(this.btnControlCancel);
            this.panelControl.Controls.Add(this.jcChkControl);
            this.panelControl.Controls.Add(this.jccmbControlType);
            this.panelControl.Controls.Add(this.panel2);
            resources.ApplyResources(this.panelControl, "panelControl");
            this.panelControl.Name = "panelControl";
            this.panelControl.Paint += new System.Windows.Forms.PaintEventHandler(this.panelControl_Paint);
            this.panelControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelControl_MouseDown);
            this.panelControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelControl_MouseMove);
            // 
            // cklistIndoor
            // 
            this.cklistIndoor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.cklistIndoor.FormattingEnabled = true;
            resources.ApplyResources(this.cklistIndoor, "cklistIndoor");
            this.cklistIndoor.Name = "cklistIndoor";
            // 
            // btnControlOK
            // 
            resources.ApplyResources(this.btnControlOK, "btnControlOK");
            this.btnControlOK.FlatAppearance.BorderSize = 0;
            this.btnControlOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnControlOK.Name = "btnControlOK";
            this.btnControlOK.UseVisualStyleBackColor = true;
            this.btnControlOK.Click += new System.EventHandler(this.btnControlOK_Click);
            // 
            // btnControlCancel
            // 
            resources.ApplyResources(this.btnControlCancel, "btnControlCancel");
            this.btnControlCancel.FlatAppearance.BorderSize = 0;
            this.btnControlCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnControlCancel.Name = "btnControlCancel";
            this.btnControlCancel.UseVisualStyleBackColor = true;
            this.btnControlCancel.Click += new System.EventHandler(this.btnControlCancel_Click);
            // 
            // jcChkControl
            // 
            resources.ApplyResources(this.jcChkControl, "jcChkControl");
            this.jcChkControl.Name = "jcChkControl";
            this.jcChkControl.UseVisualStyleBackColor = true;
            // 
            // jccmbControlType
            // 
            this.jccmbControlType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbControlType.DropDownWidth = 201;
            this.jccmbControlType.ErrorMessage = null;
            resources.ApplyResources(this.jccmbControlType, "jccmbControlType");
            this.jccmbControlType.FormattingEnabled = true;
            this.jccmbControlType.JCIgnoreSelectItem = null;
            this.jccmbControlType.Name = "jccmbControlType";
            this.jccmbControlType.SelectionChangeCommitted += new System.EventHandler(this.jccmbControlType_SelectionChangeCommitted);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(88)))), ((int)(((byte)(131)))));
            this.panel2.Controls.Add(this.jclabTitle);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelControl_MouseDown);
            this.panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelControl_MouseMove);
            // 
            // jclabTitle
            // 
            resources.ApplyResources(this.jclabTitle, "jclabTitle");
            this.jclabTitle.CausesValidation = false;
            this.jclabTitle.ForeColor = System.Drawing.Color.White;
            this.jclabTitle.FormatString = null;
            this.jclabTitle.Name = "jclabTitle";
            // 
            // pnlHelpInfo_Project
            // 
            this.pnlHelpInfo_Project.BackColor = System.Drawing.Color.White;
            this.pnlHelpInfo_Project.Controls.Add(this.jclblHelp_Project_2);
            this.pnlHelpInfo_Project.Controls.Add(this.pnlHelpInfo_Icon);
            this.pnlHelpInfo_Project.Controls.Add(this.jclblHelp_Project_1);
            resources.ApplyResources(this.pnlHelpInfo_Project, "pnlHelpInfo_Project");
            this.pnlHelpInfo_Project.Name = "pnlHelpInfo_Project";
            // 
            // jclblHelp_Project_2
            // 
            this.jclblHelp_Project_2.BackColor = System.Drawing.Color.White;
            this.jclblHelp_Project_2.CausesValidation = false;
            resources.ApplyResources(this.jclblHelp_Project_2, "jclblHelp_Project_2");
            this.jclblHelp_Project_2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(110)))), ((int)(((byte)(200)))));
            this.jclblHelp_Project_2.FormatString = null;
            this.jclblHelp_Project_2.Name = "jclblHelp_Project_2";
            // 
            // pnlHelpInfo_Icon
            // 
            this.pnlHelpInfo_Icon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            this.pnlHelpInfo_Icon.Controls.Add(this.line_HelpInfo);
            this.pnlHelpInfo_Icon.Controls.Add(this.lbl_HelpInfo);
            this.pnlHelpInfo_Icon.Controls.Add(this.pb_HelpInfo);
            resources.ApplyResources(this.pnlHelpInfo_Icon, "pnlHelpInfo_Icon");
            this.pnlHelpInfo_Icon.Name = "pnlHelpInfo_Icon";
            // 
            // line_HelpInfo
            // 
            this.line_HelpInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(106)))), ((int)(((byte)(106)))));
            resources.ApplyResources(this.line_HelpInfo, "line_HelpInfo");
            this.line_HelpInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(106)))), ((int)(((byte)(106)))));
            this.line_HelpInfo.Name = "line_HelpInfo";
            // 
            // lbl_HelpInfo
            // 
            resources.ApplyResources(this.lbl_HelpInfo, "lbl_HelpInfo");
            this.lbl_HelpInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(92)))), ((int)(((byte)(116)))));
            this.lbl_HelpInfo.Name = "lbl_HelpInfo";
            // 
            // pb_HelpInfo
            // 
            this.pb_HelpInfo.BackgroundImage = global::JCHVRF.Properties.Resources.Help_info_36x36;
            resources.ApplyResources(this.pb_HelpInfo, "pb_HelpInfo");
            this.pb_HelpInfo.Name = "pb_HelpInfo";
            this.pb_HelpInfo.TabStop = false;
            // 
            // jclblHelp_Project_1
            // 
            this.jclblHelp_Project_1.BackColor = System.Drawing.Color.White;
            this.jclblHelp_Project_1.CausesValidation = false;
            resources.ApplyResources(this.jclblHelp_Project_1, "jclblHelp_Project_1");
            this.jclblHelp_Project_1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(110)))), ((int)(((byte)(200)))));
            this.jclblHelp_Project_1.FormatString = null;
            this.jclblHelp_Project_1.Name = "jclblHelp_Project_1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelControl);
            this.panel1.Controls.Add(this.panelDetail);
            this.panel1.Controls.Add(this.dgvAvailableItems);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(237)))), ((int)(((byte)(237)))));
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            this.toolStripStatusLabel1.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabel1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Spring = true;
            // 
            // frmNewAccessory
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlHelpInfo_Project);
            this.Controls.Add(this.statusStrip1);
            this.Name = "frmNewAccessory";
            this.Load += new System.EventHandler(this.FrmNewAccessory_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableItems)).EndInit();
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            this.panelDetail.ResumeLayout(false);
            this.panelDetail.PerformLayout();
            this.panelControl.ResumeLayout(false);
            this.panelControl.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnlHelpInfo_Project.ResumeLayout(false);
            this.pnlHelpInfo_Icon.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb_HelpInfo)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.DataGridView dgvAvailableItems;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label labTitle;
        private JCBase.UI.JCLabel jclMaxNumber;
        private System.Windows.Forms.Panel panelDetail;
        private JCBase.UI.JCTextBox txtNumber;
        private JCBase.UI.JCLabel jclNumber;
        private JCBase.UI.JCTextBox txtMaxNumber;
        private JCBase.UI.VRFButton btnCancel;
        private JCBase.UI.VRFButton btnOK;
        private System.Windows.Forms.Panel panelControl;
        private JCBase.UI.JCComboBox jccmbControlType;
        private System.Windows.Forms.Panel panel2;
        private JCBase.UI.JCLabel jclabTitle;
        private JCBase.UI.JCCheckBox jcChkControl;
        private JCBase.UI.VRFButton btnControlOK;
        private JCBase.UI.VRFButton btnControlCancel;
        private System.Windows.Forms.CheckedListBox cklistIndoor;
        private JCBase.UI.JCComboBox jccmbAccessoryModel;
        private System.Windows.Forms.Panel pnlHelpInfo_Project;
        private System.Windows.Forms.Panel pnlHelpInfo_Icon;
        private System.Windows.Forms.Label line_HelpInfo;
        private System.Windows.Forms.Label lbl_HelpInfo;
        private System.Windows.Forms.PictureBox pb_HelpInfo;
        private JCBase.UI.JCLabel jclblHelp_Project_1;
        private System.Windows.Forms.Panel panel1;
        private JCBase.UI.JCLabel jclblHelp_Project_2;
        private JCBase.UI.JCLabel jclabModelTypeName;
        private JCBase.UI.JCLabel jclabModelType;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}