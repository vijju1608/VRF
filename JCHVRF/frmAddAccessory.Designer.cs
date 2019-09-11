namespace JCHVRF
{
    partial class frmAddAccessory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddAccessory));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnl_bottom = new System.Windows.Forms.Panel();
            this.pnlModelName = new System.Windows.Forms.Panel();
            this.uc_CheckBox_Sharing_RemoteController = new JCBase.UI.uc_CheckBox();
            this.jclblNameMore = new JCBase.UI.JCLabel(this.components);
            this.jccmbIndoorItem = new JCBase.UI.JCComboBox(this.components);
            this.lblHiddenModel_York = new System.Windows.Forms.Label();
            this.jclblModelNameLabel = new JCBase.UI.JCLabel(this.components);
            this.jclblModelName = new JCBase.UI.JCLabel(this.components);
            this.pnlAvailable = new System.Windows.Forms.Panel();
            this.dgvAvailableItems = new System.Windows.Forms.DataGridView();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TypeDisplay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ModelFull_York = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ModelFull_Hitachi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaxNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsDefault = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.jclblTitle_IndoorSel = new JCBase.UI.JCLabel(this.components);
            this.pnlSelected = new System.Windows.Forms.Panel();
            this.btnOK = new JCBase.UI.VRFButton(this.components);
            this.btnReset = new JCBase.UI.VRFButton(this.components);
            this.btnDelete = new JCBase.UI.VRFButton(this.components);
            this.dgvSelectedItems = new System.Windows.Forms.DataGridView();
            this.SelType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelTypeDisplay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelModel_York = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelModel_Hitachi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelMaxNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelIsDefault = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.jclblTitle_Selected = new JCBase.UI.JCLabel(this.components);
            this.pnlDoSelect = new System.Windows.Forms.Panel();
            this.jcbtnSelect = new JCBase.UI.JCButton(this.components);
            this.statusStrip1.SuspendLayout();
            this.pnlModelName.SuspendLayout();
            this.pnlAvailable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableItems)).BeginInit();
            this.pnlSelected.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelectedItems)).BeginInit();
            this.pnlDoSelect.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            this.toolStripStatusLabel1.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Spring = true;
            // 
            // pnl_bottom
            // 
            this.pnl_bottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(203)))), ((int)(((byte)(254)))));
            resources.ApplyResources(this.pnl_bottom, "pnl_bottom");
            this.pnl_bottom.Name = "pnl_bottom";
            // 
            // pnlModelName
            // 
            this.pnlModelName.Controls.Add(this.uc_CheckBox_Sharing_RemoteController);
            this.pnlModelName.Controls.Add(this.jclblNameMore);
            this.pnlModelName.Controls.Add(this.jccmbIndoorItem);
            this.pnlModelName.Controls.Add(this.lblHiddenModel_York);
            this.pnlModelName.Controls.Add(this.jclblModelNameLabel);
            this.pnlModelName.Controls.Add(this.jclblModelName);
            resources.ApplyResources(this.pnlModelName, "pnlModelName");
            this.pnlModelName.Name = "pnlModelName";
            // 
            // uc_CheckBox_Sharing_RemoteController
            // 
            this.uc_CheckBox_Sharing_RemoteController.BackColor = System.Drawing.Color.Transparent;
            this.uc_CheckBox_Sharing_RemoteController.CheckBoxLocation = new System.Drawing.Point(0, 0);
            resources.ApplyResources(this.uc_CheckBox_Sharing_RemoteController, "uc_CheckBox_Sharing_RemoteController");
            this.uc_CheckBox_Sharing_RemoteController.ForeColor = System.Drawing.SystemColors.ControlText;
            this.uc_CheckBox_Sharing_RemoteController.LabelAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.uc_CheckBox_Sharing_RemoteController.LabelLocation = new System.Drawing.Point(36, 0);
            this.uc_CheckBox_Sharing_RemoteController.LabelSize = new System.Drawing.Size(169, 26);
            this.uc_CheckBox_Sharing_RemoteController.Name = "uc_CheckBox_Sharing_RemoteController";
            this.uc_CheckBox_Sharing_RemoteController.CheckedChanged += new System.EventHandler(this.uc_CheckBox_Sharing_RemoteController_CheckedChanged);
            // 
            // jclblNameMore
            // 
            this.jclblNameMore.CausesValidation = false;
            resources.ApplyResources(this.jclblNameMore, "jclblNameMore");
            this.jclblNameMore.ForeColor = System.Drawing.Color.Blue;
            this.jclblNameMore.FormatString = null;
            this.jclblNameMore.Name = "jclblNameMore";
            this.jclblNameMore.MouseClick += new System.Windows.Forms.MouseEventHandler(this.jclblNameMore_MouseClick);
            this.jclblNameMore.MouseEnter += new System.EventHandler(this.jclblNameMore_MouseEnter);
            this.jclblNameMore.MouseLeave += new System.EventHandler(this.jclblNameMore_MouseLeave);
            // 
            // jccmbIndoorItem
            // 
            this.jccmbIndoorItem.DropDownHeight = 200;
            this.jccmbIndoorItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbIndoorItem.DropDownWidth = 220;
            this.jccmbIndoorItem.ErrorMessage = null;
            resources.ApplyResources(this.jccmbIndoorItem, "jccmbIndoorItem");
            this.jccmbIndoorItem.FormattingEnabled = true;
            this.jccmbIndoorItem.JCIgnoreSelectItem = null;
            this.jccmbIndoorItem.Name = "jccmbIndoorItem";
            this.jccmbIndoorItem.SelectionChangeCommitted += new System.EventHandler(this.jccmbIndoorItem_SelectionChangeCommitted);
            // 
            // lblHiddenModel_York
            // 
            resources.ApplyResources(this.lblHiddenModel_York, "lblHiddenModel_York");
            this.lblHiddenModel_York.Name = "lblHiddenModel_York";
            // 
            // jclblModelNameLabel
            // 
            this.jclblModelNameLabel.CausesValidation = false;
            resources.ApplyResources(this.jclblModelNameLabel, "jclblModelNameLabel");
            this.jclblModelNameLabel.FormatString = null;
            this.jclblModelNameLabel.Name = "jclblModelNameLabel";
            // 
            // jclblModelName
            // 
            this.jclblModelName.CausesValidation = false;
            resources.ApplyResources(this.jclblModelName, "jclblModelName");
            this.jclblModelName.ForeColor = System.Drawing.Color.Blue;
            this.jclblModelName.FormatString = null;
            this.jclblModelName.Name = "jclblModelName";
            // 
            // pnlAvailable
            // 
            this.pnlAvailable.Controls.Add(this.dgvAvailableItems);
            this.pnlAvailable.Controls.Add(this.jclblTitle_IndoorSel);
            resources.ApplyResources(this.pnlAvailable, "pnlAvailable");
            this.pnlAvailable.Name = "pnlAvailable";
            // 
            // dgvAvailableItems
            // 
            this.dgvAvailableItems.AllowUserToAddRows = false;
            this.dgvAvailableItems.AllowUserToDeleteRows = false;
            this.dgvAvailableItems.AllowUserToResizeColumns = false;
            this.dgvAvailableItems.AllowUserToResizeRows = false;
            this.dgvAvailableItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAvailableItems.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAvailableItems.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.dgvAvailableItems, "dgvAvailableItems");
            this.dgvAvailableItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvAvailableItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Type,
            this.TypeDisplay,
            this.ModelFull_York,
            this.ModelFull_Hitachi,
            this.MaxNumber,
            this.IsDefault});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAvailableItems.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvAvailableItems.MultiSelect = false;
            this.dgvAvailableItems.Name = "dgvAvailableItems";
            this.dgvAvailableItems.ReadOnly = true;
            this.dgvAvailableItems.RowHeadersVisible = false;
            this.dgvAvailableItems.RowTemplate.DividerHeight = 1;
            this.dgvAvailableItems.RowTemplate.Height = 32;
            this.dgvAvailableItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAvailableItems.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvAvailableItems_CellPainting);
            this.dgvAvailableItems.DoubleClick += new System.EventHandler(this.dgvAvailableItems_DoubleClick);
            // 
            // Type
            // 
            this.Type.DataPropertyName = "Type";
            this.Type.FillWeight = 137F;
            resources.ApplyResources(this.Type, "Type");
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            // 
            // TypeDisplay
            // 
            this.TypeDisplay.DataPropertyName = "TypeDisplay";
            resources.ApplyResources(this.TypeDisplay, "TypeDisplay");
            this.TypeDisplay.Name = "TypeDisplay";
            this.TypeDisplay.ReadOnly = true;
            // 
            // ModelFull_York
            // 
            this.ModelFull_York.DataPropertyName = "Model_York";
            this.ModelFull_York.FillWeight = 93.21623F;
            resources.ApplyResources(this.ModelFull_York, "ModelFull_York");
            this.ModelFull_York.Name = "ModelFull_York";
            this.ModelFull_York.ReadOnly = true;
            // 
            // ModelFull_Hitachi
            // 
            this.ModelFull_Hitachi.DataPropertyName = "Model_Hitachi";
            resources.ApplyResources(this.ModelFull_Hitachi, "ModelFull_Hitachi");
            this.ModelFull_Hitachi.Name = "ModelFull_Hitachi";
            this.ModelFull_Hitachi.ReadOnly = true;
            // 
            // MaxNumber
            // 
            this.MaxNumber.DataPropertyName = "MaxNumber";
            this.MaxNumber.FillWeight = 70F;
            resources.ApplyResources(this.MaxNumber, "MaxNumber");
            this.MaxNumber.Name = "MaxNumber";
            this.MaxNumber.ReadOnly = true;
            // 
            // IsDefault
            // 
            this.IsDefault.DataPropertyName = "IsDefault";
            resources.ApplyResources(this.IsDefault, "IsDefault");
            this.IsDefault.Name = "IsDefault";
            this.IsDefault.ReadOnly = true;
            // 
            // jclblTitle_IndoorSel
            // 
            this.jclblTitle_IndoorSel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(125)))), ((int)(((byte)(81)))));
            this.jclblTitle_IndoorSel.CausesValidation = false;
            resources.ApplyResources(this.jclblTitle_IndoorSel, "jclblTitle_IndoorSel");
            this.jclblTitle_IndoorSel.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_IndoorSel.FormatString = null;
            this.jclblTitle_IndoorSel.Name = "jclblTitle_IndoorSel";
            // 
            // pnlSelected
            // 
            this.pnlSelected.Controls.Add(this.btnOK);
            this.pnlSelected.Controls.Add(this.btnReset);
            this.pnlSelected.Controls.Add(this.btnDelete);
            this.pnlSelected.Controls.Add(this.dgvSelectedItems);
            this.pnlSelected.Controls.Add(this.jclblTitle_Selected);
            resources.ApplyResources(this.pnlSelected, "pnlSelected");
            this.pnlSelected.Name = "pnlSelected";
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
            // btnReset
            // 
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.FlatAppearance.BorderSize = 0;
            this.btnReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnReset.Name = "btnReset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // dgvSelectedItems
            // 
            this.dgvSelectedItems.AllowUserToAddRows = false;
            resources.ApplyResources(this.dgvSelectedItems, "dgvSelectedItems");
            this.dgvSelectedItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSelectedItems.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSelectedItems.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvSelectedItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSelectedItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SelType,
            this.SelTypeDisplay,
            this.SelModel_York,
            this.SelModel_Hitachi,
            this.Description,
            this.Count,
            this.SelMaxNumber,
            this.SelIsDefault});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSelectedItems.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvSelectedItems.MultiSelect = false;
            this.dgvSelectedItems.Name = "dgvSelectedItems";
            this.dgvSelectedItems.ReadOnly = true;
            this.dgvSelectedItems.RowHeadersVisible = false;
            this.dgvSelectedItems.RowTemplate.DividerHeight = 1;
            this.dgvSelectedItems.RowTemplate.Height = 32;
            this.dgvSelectedItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSelectedItems.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvSelectedItems_CellPainting);
            this.dgvSelectedItems.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dgvSelectedItems_RowsAdded);
            this.dgvSelectedItems.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dgvSelectedItems_RowsRemoved);
            // 
            // SelType
            // 
            this.SelType.DataPropertyName = "Type";
            this.SelType.FillWeight = 149.2386F;
            resources.ApplyResources(this.SelType, "SelType");
            this.SelType.Name = "SelType";
            this.SelType.ReadOnly = true;
            // 
            // SelTypeDisplay
            // 
            this.SelTypeDisplay.DataPropertyName = "TypeDisplay";
            resources.ApplyResources(this.SelTypeDisplay, "SelTypeDisplay");
            this.SelTypeDisplay.Name = "SelTypeDisplay";
            this.SelTypeDisplay.ReadOnly = true;
            // 
            // SelModel_York
            // 
            this.SelModel_York.DataPropertyName = "Model_York";
            resources.ApplyResources(this.SelModel_York, "SelModel_York");
            this.SelModel_York.Name = "SelModel_York";
            this.SelModel_York.ReadOnly = true;
            // 
            // SelModel_Hitachi
            // 
            this.SelModel_Hitachi.DataPropertyName = "Model_Hitachi";
            resources.ApplyResources(this.SelModel_Hitachi, "SelModel_Hitachi");
            this.SelModel_Hitachi.Name = "SelModel_Hitachi";
            this.SelModel_Hitachi.ReadOnly = true;
            // 
            // Description
            // 
            this.Description.DataPropertyName = "Description";
            resources.ApplyResources(this.Description, "Description");
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            // 
            // Count
            // 
            this.Count.FillWeight = 50.76142F;
            resources.ApplyResources(this.Count, "Count");
            this.Count.Name = "Count";
            this.Count.ReadOnly = true;
            // 
            // SelMaxNumber
            // 
            resources.ApplyResources(this.SelMaxNumber, "SelMaxNumber");
            this.SelMaxNumber.Name = "SelMaxNumber";
            this.SelMaxNumber.ReadOnly = true;
            // 
            // SelIsDefault
            // 
            this.SelIsDefault.DataPropertyName = "IsDefault";
            resources.ApplyResources(this.SelIsDefault, "SelIsDefault");
            this.SelIsDefault.Name = "SelIsDefault";
            this.SelIsDefault.ReadOnly = true;
            // 
            // jclblTitle_Selected
            // 
            this.jclblTitle_Selected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(93)))), ((int)(((byte)(58)))));
            this.jclblTitle_Selected.CausesValidation = false;
            resources.ApplyResources(this.jclblTitle_Selected, "jclblTitle_Selected");
            this.jclblTitle_Selected.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_Selected.FormatString = null;
            this.jclblTitle_Selected.Name = "jclblTitle_Selected";
            // 
            // pnlDoSelect
            // 
            this.pnlDoSelect.Controls.Add(this.jcbtnSelect);
            resources.ApplyResources(this.pnlDoSelect, "pnlDoSelect");
            this.pnlDoSelect.Name = "pnlDoSelect";
            // 
            // jcbtnSelect
            // 
            this.jcbtnSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jcbtnSelect.BackgroundImage = global::JCHVRF.Properties.Resources._140103_04_piping_arrow_left_to_right;
            resources.ApplyResources(this.jcbtnSelect, "jcbtnSelect");
            this.jcbtnSelect.FlatAppearance.BorderSize = 0;
            this.jcbtnSelect.Name = "jcbtnSelect";
            this.jcbtnSelect.UseVisualStyleBackColor = false;
            this.jcbtnSelect.Click += new System.EventHandler(this.jcbtnSelect_Click);
            // 
            // frmAddAccessory
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlSelected);
            this.Controls.Add(this.pnlDoSelect);
            this.Controls.Add(this.pnlAvailable);
            this.Controls.Add(this.pnlModelName);
            this.Controls.Add(this.pnl_bottom);
            this.Controls.Add(this.statusStrip1);
            this.MaximizeBox = false;
            this.Name = "frmAddAccessory";
            this.Load += new System.EventHandler(this.frmAddAccessory_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.pnlModelName.ResumeLayout(false);
            this.pnlModelName.PerformLayout();
            this.pnlAvailable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvailableItems)).EndInit();
            this.pnlSelected.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSelectedItems)).EndInit();
            this.pnlDoSelect.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Panel pnl_bottom;
        private System.Windows.Forms.Panel pnlModelName;
        private JCBase.UI.JCLabel jclblModelNameLabel;
        private System.Windows.Forms.Panel pnlAvailable;
        private JCBase.UI.JCLabel jclblTitle_IndoorSel;
        private System.Windows.Forms.Panel pnlSelected;
        private JCBase.UI.JCLabel jclblTitle_Selected;
        private System.Windows.Forms.Panel pnlDoSelect;
        private JCBase.UI.JCButton jcbtnSelect;
        private System.Windows.Forms.DataGridView dgvSelectedItems;
        private System.Windows.Forms.Label lblHiddenModel_York;
        private JCBase.UI.VRFButton btnOK;
        private JCBase.UI.VRFButton btnReset;
        private JCBase.UI.VRFButton btnDelete;
        private JCBase.UI.JCComboBox jccmbIndoorItem;
        private JCBase.UI.JCLabel jclblModelName;
        private JCBase.UI.JCLabel jclblNameMore;
        private JCBase.UI.uc_CheckBox uc_CheckBox_Sharing_RemoteController;
        private System.Windows.Forms.DataGridView dgvAvailableItems;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn TypeDisplay;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModelFull_York;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModelFull_Hitachi;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaxNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelType;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelTypeDisplay;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelModel_York;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelModel_Hitachi;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Count;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelMaxNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelIsDefault;
    }
}