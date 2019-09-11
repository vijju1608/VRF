namespace JCHVRF
{
    partial class frmSystemUnit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSystemUnit));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnOK = new JCBase.UI.VRFButton(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgvSystemInfo = new System.Windows.Forms.DataGridView();
            this.RoomName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column25 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column27 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column22 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column26 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column28 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column29 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column30 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystemInfo)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOK.BackgroundImage")));
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.FlatAppearance.BorderSize = 0;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.btnOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnOK.Location = new System.Drawing.Point(912, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 26);
            this.btnOK.TabIndex = 85;
            this.btnOK.Text = "Close";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dgvSystemInfo);
            this.panel1.Location = new System.Drawing.Point(4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1001, 531);
            this.panel1.TabIndex = 86;
            // 
            // dgvSystemInfo
            // 
            this.dgvSystemInfo.AllowUserToAddRows = false;
            this.dgvSystemInfo.AllowUserToDeleteRows = false;
            this.dgvSystemInfo.AllowUserToResizeColumns = false;
            this.dgvSystemInfo.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            this.dgvSystemInfo.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvSystemInfo.BackgroundColor = System.Drawing.Color.White;
            this.dgvSystemInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvSystemInfo.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this.dgvSystemInfo.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(112)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(112)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSystemInfo.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvSystemInfo.ColumnHeadersHeight = 58;
            this.dgvSystemInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSystemInfo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RoomName,
            this.Column21,
            this.Column25,
            this.Column23,
            this.Column27,
            this.dataGridViewTextBoxColumn3,
            this.Column22,
            this.Column26,
            this.Column24,
            this.Column28,
            this.Column29,
            this.Column19,
            this.Column30,
            this.Column20});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvSystemInfo.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvSystemInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSystemInfo.EnableHeadersVisualStyles = false;
            this.dgvSystemInfo.Location = new System.Drawing.Point(0, 0);
            this.dgvSystemInfo.Margin = new System.Windows.Forms.Padding(0);
            this.dgvSystemInfo.Name = "dgvSystemInfo";
            this.dgvSystemInfo.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSystemInfo.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvSystemInfo.RowHeadersVisible = false;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.dgvSystemInfo.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvSystemInfo.RowTemplate.Height = 30;
            this.dgvSystemInfo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSystemInfo.Size = new System.Drawing.Size(1001, 531);
            this.dgvSystemInfo.TabIndex = 139;
            this.dgvSystemInfo.Visible = false;
            this.dgvSystemInfo.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvSystemInfo_CellPainting);
            // 
            // RoomName
            // 
            this.RoomName.HeaderText = "RoomName";
            this.RoomName.Name = "RoomName";
            this.RoomName.ReadOnly = true;
            this.RoomName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.RoomName.Width = 69;
            // 
            // Column21
            // 
            this.Column21.FillWeight = 7F;
            this.Column21.HeaderText = "Req\'d Capacity_C";
            this.Column21.Name = "Column21";
            this.Column21.ReadOnly = true;
            this.Column21.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column21.Width = 85;
            // 
            // Column25
            // 
            this.Column25.HeaderText = "Req\'d Capacity_H";
            this.Column25.Name = "Column25";
            this.Column25.ReadOnly = true;
            this.Column25.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column25.Width = 85;
            // 
            // Column23
            // 
            this.Column23.HeaderText = "Req\'d_Sensible";
            this.Column23.Name = "Column23";
            this.Column23.ReadOnly = true;
            this.Column23.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column23.Width = 80;
            // 
            // Column27
            // 
            this.Column27.FillWeight = 7F;
            this.Column27.HeaderText = "Name";
            this.Column27.Name = "Column27";
            this.Column27.ReadOnly = true;
            this.Column27.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column27.Width = 65;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.FillWeight = 7F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Model";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.Width = 110;
            // 
            // Column22
            // 
            this.Column22.FillWeight = 7F;
            this.Column22.HeaderText = "Actual Capacity_C";
            this.Column22.Name = "Column22";
            this.Column22.ReadOnly = true;
            this.Column22.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column22.Width = 78;
            // 
            // Column26
            // 
            this.Column26.HeaderText = "Actual Capacity_H";
            this.Column26.Name = "Column26";
            this.Column26.ReadOnly = true;
            this.Column26.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column26.Width = 78;
            // 
            // Column24
            // 
            this.Column24.HeaderText = "Actual_Sensible";
            this.Column24.Name = "Column24";
            this.Column24.ReadOnly = true;
            this.Column24.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column24.Width = 65;
            // 
            // Column28
            // 
            this.Column28.FillWeight = 7F;
            this.Column28.HeaderText = "DB_C";
            this.Column28.Name = "Column28";
            this.Column28.ReadOnly = true;
            this.Column28.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column28.Width = 70;
            // 
            // Column29
            // 
            this.Column29.HeaderText = "DB_H";
            this.Column29.Name = "Column29";
            this.Column29.ReadOnly = true;
            this.Column29.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column29.Width = 55;
            // 
            // Column19
            // 
            this.Column19.FillWeight = 7F;
            this.Column19.HeaderText = "WB_C";
            this.Column19.Name = "Column19";
            this.Column19.ReadOnly = true;
            this.Column19.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column19.Width = 55;
            // 
            // Column30
            // 
            this.Column30.HeaderText = "WB_H";
            this.Column30.Name = "Column30";
            this.Column30.ReadOnly = true;
            this.Column30.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column30.Width = 55;
            // 
            // Column20
            // 
            this.Column20.HeaderText = "RH";
            this.Column20.Name = "Column20";
            this.Column20.ReadOnly = true;
            this.Column20.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column20.Width = 50;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Location = new System.Drawing.Point(3, 534);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1002, 46);
            this.panel2.TabIndex = 87;
            // 
            // frmSystemUnit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(88)))), ((int)(((byte)(131)))));
            this.ClientSize = new System.Drawing.Size(1008, 583);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmSystemUnit";
            this.Text = "frmSystemUnit";
            this.Load += new System.EventHandler(this.frmSystemUnit_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystemInfo)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.VRFButton btnOK;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvSystemInfo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridViewTextBoxColumn RoomName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column21;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column25;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column23;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column27;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column22;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column26;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column24;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column28;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column29;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column19;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column30;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column20;
    }
}