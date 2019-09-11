namespace JCHVRF
{
    partial class frmLoadIndex
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLoadIndex));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnl_bottom = new System.Windows.Forms.Panel();
            this.tapPageTrans1 = new JCBase.UI.TapPageTrans();
            this.tpgIndex = new System.Windows.Forms.TabPage();
            this.pnlOK = new System.Windows.Forms.Panel();
            this.jcbtnCancel = new JCBase.UI.VRFButton(this.components);
            this.jcbtnOK = new JCBase.UI.VRFButton(this.components);
            this.jclblUnitLoadIndex = new JCBase.UI.JCLabel(this.components);
            this.jclblIndexList = new JCBase.UI.JCLabel(this.components);
            this.pnlContent_Index = new System.Windows.Forms.Panel();
            this.dgvLoadIndex = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlContent_Index_1 = new System.Windows.Forms.Panel();
            this.pbDefaultIndex = new JCBase.UI.VRFIcon(this.components);
            this.pbDeleteIndex = new JCBase.UI.VRFIcon(this.components);
            this.pbEditIndex = new JCBase.UI.VRFIcon(this.components);
            this.pbAddIndex = new JCBase.UI.VRFIcon(this.components);
            this.jclblTitle_IndexList = new JCBase.UI.JCLabel(this.components);
            this.pnlSpan_1 = new System.Windows.Forms.Panel();
            this.pnlContent_Location = new System.Windows.Forms.Panel();
            this.pbDefaultLocation = new JCBase.UI.VRFIcon(this.components);
            this.pbDeleteLocation = new JCBase.UI.VRFIcon(this.components);
            this.pbEditLocation = new JCBase.UI.VRFIcon(this.components);
            this.pbAddLocation = new JCBase.UI.VRFIcon(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.jclblTitle_Location = new JCBase.UI.JCLabel(this.components);
            this.jccmbLocation = new JCBase.UI.JCComboBox(this.components);
            this.statusStrip1.SuspendLayout();
            this.tapPageTrans1.SuspendLayout();
            this.tpgIndex.SuspendLayout();
            this.pnlOK.SuspendLayout();
            this.pnlContent_Index.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLoadIndex)).BeginInit();
            this.pnlContent_Index_1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeleteIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbEditIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddIndex)).BeginInit();
            this.pnlContent_Location.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeleteLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbEditLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.toolStripStatusLabel1.BackColor = System.Drawing.Color.White;
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
            // tapPageTrans1
            // 
            this.tapPageTrans1.Color_bg_control = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            this.tapPageTrans1.Color_border_top_selected = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.tapPageTrans1.Controls.Add(this.tpgIndex);
            resources.ApplyResources(this.tapPageTrans1, "tapPageTrans1");
            this.tapPageTrans1.Name = "tapPageTrans1";
            this.tapPageTrans1.SelectedIndex = 0;
            this.tapPageTrans1.TitleLogo = null;
            // 
            // tpgIndex
            // 
            this.tpgIndex.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.tpgIndex.Controls.Add(this.pnlOK);
            this.tpgIndex.Controls.Add(this.jclblUnitLoadIndex);
            this.tpgIndex.Controls.Add(this.jclblIndexList);
            this.tpgIndex.Controls.Add(this.pnlContent_Index);
            this.tpgIndex.Controls.Add(this.pnlSpan_1);
            this.tpgIndex.Controls.Add(this.pnlContent_Location);
            resources.ApplyResources(this.tpgIndex, "tpgIndex");
            this.tpgIndex.Name = "tpgIndex";
            // 
            // pnlOK
            // 
            this.pnlOK.Controls.Add(this.jcbtnCancel);
            this.pnlOK.Controls.Add(this.jcbtnOK);
            resources.ApplyResources(this.pnlOK, "pnlOK");
            this.pnlOK.Name = "pnlOK";
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
            // jclblUnitLoadIndex
            // 
            resources.ApplyResources(this.jclblUnitLoadIndex, "jclblUnitLoadIndex");
            this.jclblUnitLoadIndex.CausesValidation = false;
            this.jclblUnitLoadIndex.FormatString = null;
            this.jclblUnitLoadIndex.Name = "jclblUnitLoadIndex";
            // 
            // jclblIndexList
            // 
            resources.ApplyResources(this.jclblIndexList, "jclblIndexList");
            this.jclblIndexList.CausesValidation = false;
            this.jclblIndexList.FormatString = null;
            this.jclblIndexList.Name = "jclblIndexList";
            // 
            // pnlContent_Index
            // 
            this.pnlContent_Index.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlContent_Index.Controls.Add(this.dgvLoadIndex);
            this.pnlContent_Index.Controls.Add(this.pnlContent_Index_1);
            this.pnlContent_Index.Controls.Add(this.jclblTitle_IndexList);
            resources.ApplyResources(this.pnlContent_Index, "pnlContent_Index");
            this.pnlContent_Index.Name = "pnlContent_Index";
            // 
            // dgvLoadIndex
            // 
            this.dgvLoadIndex.AllowUserToAddRows = false;
            this.dgvLoadIndex.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            this.dgvLoadIndex.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvLoadIndex.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLoadIndex.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvLoadIndex.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvLoadIndex.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dgvLoadIndex, "dgvLoadIndex");
            this.dgvLoadIndex.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvLoadIndex.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvLoadIndex.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvLoadIndex.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvLoadIndex.Name = "dgvLoadIndex";
            this.dgvLoadIndex.ReadOnly = true;
            this.dgvLoadIndex.RowHeadersVisible = false;
            this.dgvLoadIndex.RowTemplate.Height = 30;
            this.dgvLoadIndex.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLoadIndex.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLoadIndex_CellDoubleClick);
            this.dgvLoadIndex.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvLoadIndex_CellPainting);
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "RoomType";
            this.Column1.FillWeight = 200F;
            resources.ApplyResources(this.Column1, "Column1");
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.FillWeight = 120F;
            resources.ApplyResources(this.Column2, "Column2");
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.FillWeight = 120F;
            resources.ApplyResources(this.Column3, "Column3");
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column4
            // 
            this.Column4.DataPropertyName = "ISDEFAULT";
            resources.ApplyResources(this.Column4, "Column4");
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // pnlContent_Index_1
            // 
            this.pnlContent_Index_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.pnlContent_Index_1.Controls.Add(this.pbDefaultIndex);
            this.pnlContent_Index_1.Controls.Add(this.pbDeleteIndex);
            this.pnlContent_Index_1.Controls.Add(this.pbEditIndex);
            this.pnlContent_Index_1.Controls.Add(this.pbAddIndex);
            resources.ApplyResources(this.pnlContent_Index_1, "pnlContent_Index_1");
            this.pnlContent_Index_1.Name = "pnlContent_Index_1";
            // 
            // pbDefaultIndex
            // 
            this.pbDefaultIndex.BackgroundImage = global::JCHVRF.Properties.Resources.default_place_nr_36x36;
            resources.ApplyResources(this.pbDefaultIndex, "pbDefaultIndex");
            this.pbDefaultIndex.ClickImage = null;
            this.pbDefaultIndex.DefaultImage = global::JCHVRF.Properties.Resources.default_place_nr_36x36;
            this.pbDefaultIndex.HoverImage = global::JCHVRF.Properties.Resources.default_place_hi_36x36;
            this.pbDefaultIndex.Name = "pbDefaultIndex";
            this.pbDefaultIndex.TabStop = false;
            this.pbDefaultIndex.Tooltips = null;
            this.pbDefaultIndex.Click += new System.EventHandler(this.jcbtnDefaultIndex_Click);
            // 
            // pbDeleteIndex
            // 
            this.pbDeleteIndex.BackgroundImage = global::JCHVRF.Properties.Resources.delete_place_nr_36x36;
            resources.ApplyResources(this.pbDeleteIndex, "pbDeleteIndex");
            this.pbDeleteIndex.ClickImage = null;
            this.pbDeleteIndex.DefaultImage = global::JCHVRF.Properties.Resources.delete_place_nr_36x36;
            this.pbDeleteIndex.HoverImage = global::JCHVRF.Properties.Resources.delete_place_hi_36x36;
            this.pbDeleteIndex.Name = "pbDeleteIndex";
            this.pbDeleteIndex.TabStop = false;
            this.pbDeleteIndex.Tooltips = null;
            this.pbDeleteIndex.Click += new System.EventHandler(this.pbDeleteIndex_Click);
            // 
            // pbEditIndex
            // 
            this.pbEditIndex.BackgroundImage = global::JCHVRF.Properties.Resources.Edit_option_nr_48x48;
            resources.ApplyResources(this.pbEditIndex, "pbEditIndex");
            this.pbEditIndex.ClickImage = null;
            this.pbEditIndex.DefaultImage = global::JCHVRF.Properties.Resources.Edit_option_nr_48x48;
            this.pbEditIndex.HoverImage = global::JCHVRF.Properties.Resources.Edit_option_hi_48x48;
            this.pbEditIndex.Name = "pbEditIndex";
            this.pbEditIndex.TabStop = false;
            this.pbEditIndex.Tooltips = null;
            this.pbEditIndex.Click += new System.EventHandler(this.pbEditIndex_Click);
            // 
            // pbAddIndex
            // 
            this.pbAddIndex.BackgroundImage = global::JCHVRF.Properties.Resources.add_place_nr_36x36;
            resources.ApplyResources(this.pbAddIndex, "pbAddIndex");
            this.pbAddIndex.ClickImage = null;
            this.pbAddIndex.DefaultImage = global::JCHVRF.Properties.Resources.add_place_nr_36x36;
            this.pbAddIndex.HoverImage = global::JCHVRF.Properties.Resources.add_place_hi_36x36;
            this.pbAddIndex.Name = "pbAddIndex";
            this.pbAddIndex.TabStop = false;
            this.pbAddIndex.Tooltips = null;
            this.pbAddIndex.Click += new System.EventHandler(this.pbAddIndex_Click);
            // 
            // jclblTitle_IndexList
            // 
            this.jclblTitle_IndexList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(93)))), ((int)(((byte)(59)))));
            this.jclblTitle_IndexList.CausesValidation = false;
            resources.ApplyResources(this.jclblTitle_IndexList, "jclblTitle_IndexList");
            this.jclblTitle_IndexList.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_IndexList.FormatString = null;
            this.jclblTitle_IndexList.Name = "jclblTitle_IndexList";
            // 
            // pnlSpan_1
            // 
            resources.ApplyResources(this.pnlSpan_1, "pnlSpan_1");
            this.pnlSpan_1.Name = "pnlSpan_1";
            // 
            // pnlContent_Location
            // 
            this.pnlContent_Location.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.pnlContent_Location.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlContent_Location.Controls.Add(this.pbDefaultLocation);
            this.pnlContent_Location.Controls.Add(this.pbDeleteLocation);
            this.pnlContent_Location.Controls.Add(this.pbEditLocation);
            this.pnlContent_Location.Controls.Add(this.pbAddLocation);
            this.pnlContent_Location.Controls.Add(this.pictureBox1);
            this.pnlContent_Location.Controls.Add(this.jclblTitle_Location);
            this.pnlContent_Location.Controls.Add(this.jccmbLocation);
            resources.ApplyResources(this.pnlContent_Location, "pnlContent_Location");
            this.pnlContent_Location.Name = "pnlContent_Location";
            // 
            // pbDefaultLocation
            // 
            this.pbDefaultLocation.BackgroundImage = global::JCHVRF.Properties.Resources.default_area_nr_36x36;
            resources.ApplyResources(this.pbDefaultLocation, "pbDefaultLocation");
            this.pbDefaultLocation.ClickImage = null;
            this.pbDefaultLocation.DefaultImage = global::JCHVRF.Properties.Resources.default_area_nr_36x36;
            this.pbDefaultLocation.HoverImage = global::JCHVRF.Properties.Resources.default_area_hi_36x36;
            this.pbDefaultLocation.Name = "pbDefaultLocation";
            this.pbDefaultLocation.TabStop = false;
            this.pbDefaultLocation.Tooltips = null;
            this.pbDefaultLocation.Click += new System.EventHandler(this.pbDefaultLocation_Click);
            // 
            // pbDeleteLocation
            // 
            this.pbDeleteLocation.BackgroundImage = global::JCHVRF.Properties.Resources.delete_area_nr_36x36;
            resources.ApplyResources(this.pbDeleteLocation, "pbDeleteLocation");
            this.pbDeleteLocation.ClickImage = null;
            this.pbDeleteLocation.DefaultImage = global::JCHVRF.Properties.Resources.delete_area_nr_36x36;
            this.pbDeleteLocation.HoverImage = global::JCHVRF.Properties.Resources.delete_area_hi_36x36;
            this.pbDeleteLocation.Name = "pbDeleteLocation";
            this.pbDeleteLocation.TabStop = false;
            this.pbDeleteLocation.Tooltips = null;
            this.pbDeleteLocation.Click += new System.EventHandler(this.pbDeleteLocation_Click);
            // 
            // pbEditLocation
            // 
            this.pbEditLocation.BackgroundImage = global::JCHVRF.Properties.Resources.edit_area_nr_36x36;
            resources.ApplyResources(this.pbEditLocation, "pbEditLocation");
            this.pbEditLocation.ClickImage = null;
            this.pbEditLocation.DefaultImage = global::JCHVRF.Properties.Resources.edit_area_nr_36x36;
            this.pbEditLocation.HoverImage = global::JCHVRF.Properties.Resources.edit_area_hi_36x36;
            this.pbEditLocation.Name = "pbEditLocation";
            this.pbEditLocation.TabStop = false;
            this.pbEditLocation.Tooltips = null;
            this.pbEditLocation.Click += new System.EventHandler(this.pbEditLocation_Click);
            // 
            // pbAddLocation
            // 
            this.pbAddLocation.BackgroundImage = global::JCHVRF.Properties.Resources.add_area_nr_36x36;
            resources.ApplyResources(this.pbAddLocation, "pbAddLocation");
            this.pbAddLocation.ClickImage = null;
            this.pbAddLocation.DefaultImage = global::JCHVRF.Properties.Resources.add_area_nr_36x36;
            this.pbAddLocation.HoverImage = global::JCHVRF.Properties.Resources.add_area_hi_36x36;
            this.pbAddLocation.Name = "pbAddLocation";
            this.pbAddLocation.TabStop = false;
            this.pbAddLocation.Tooltips = null;
            this.pbAddLocation.Click += new System.EventHandler(this.pbAddLocation_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::JCHVRF.Properties.Resources.search_area_nr_36x36;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // jclblTitle_Location
            // 
            this.jclblTitle_Location.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jclblTitle_Location.CausesValidation = false;
            resources.ApplyResources(this.jclblTitle_Location, "jclblTitle_Location");
            this.jclblTitle_Location.ForeColor = System.Drawing.Color.White;
            this.jclblTitle_Location.FormatString = null;
            this.jclblTitle_Location.Name = "jclblTitle_Location";
            // 
            // jccmbLocation
            // 
            this.jccmbLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbLocation.ErrorMessage = null;
            resources.ApplyResources(this.jccmbLocation, "jccmbLocation");
            this.jccmbLocation.FormattingEnabled = true;
            this.jccmbLocation.Items.AddRange(new object[] {
            resources.GetString("jccmbLocation.Items"),
            resources.GetString("jccmbLocation.Items1"),
            resources.GetString("jccmbLocation.Items2"),
            resources.GetString("jccmbLocation.Items3")});
            this.jccmbLocation.JCIgnoreSelectItem = null;
            this.jccmbLocation.Name = "jccmbLocation";
            this.jccmbLocation.SelectedIndexChanged += new System.EventHandler(this.jccmbLocation_SelectedIndexChanged);
            // 
            // frmLoadIndex
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.Controls.Add(this.tapPageTrans1);
            this.Controls.Add(this.pnl_bottom);
            this.Controls.Add(this.statusStrip1);
            this.MaximizeBox = false;
            this.Name = "frmLoadIndex";
            this.Load += new System.EventHandler(this.frmLoadIndex_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tapPageTrans1.ResumeLayout(false);
            this.tpgIndex.ResumeLayout(false);
            this.tpgIndex.PerformLayout();
            this.pnlOK.ResumeLayout(false);
            this.pnlContent_Index.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLoadIndex)).EndInit();
            this.pnlContent_Index_1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeleteIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbEditIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddIndex)).EndInit();
            this.pnlContent_Location.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbDefaultLocation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeleteLocation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbEditLocation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddLocation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private JCBase.UI.JCLabel jclblIndexList;
        private System.Windows.Forms.DataGridView dgvLoadIndex;
        private JCBase.UI.JCComboBox jccmbLocation;
        private JCBase.UI.JCLabel jclblUnitLoadIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private JCBase.UI.TapPageTrans tapPageTrans1;
        private System.Windows.Forms.TabPage tpgIndex;
        private System.Windows.Forms.Panel pnlContent_Location;
        private JCBase.UI.JCLabel jclblTitle_Location;
        private System.Windows.Forms.Panel pnlSpan_1;
        private System.Windows.Forms.Panel pnlContent_Index;
        private JCBase.UI.JCLabel jclblTitle_IndexList;
        private System.Windows.Forms.Panel pnlContent_Index_1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlOK;
        private JCBase.UI.VRFButton jcbtnCancel;
        private JCBase.UI.VRFButton jcbtnOK;
        private System.Windows.Forms.Panel pnl_bottom;
        private JCBase.UI.VRFIcon pbAddLocation;
        private JCBase.UI.VRFIcon pbEditLocation;
        private JCBase.UI.VRFIcon pbDeleteLocation;
        private JCBase.UI.VRFIcon pbDefaultLocation;
        private JCBase.UI.VRFIcon pbAddIndex;
        private JCBase.UI.VRFIcon pbEditIndex;
        private JCBase.UI.VRFIcon pbDefaultIndex;
        private JCBase.UI.VRFIcon pbDeleteIndex;


    }
}
