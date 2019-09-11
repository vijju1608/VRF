namespace JCHVRF
{
    partial class ucDropOutdoor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucDropOutdoor));
            this.jclblIndoorCount = new JCBase.UI.JCLabel(this.components);
            this.jclblOutdoorCount = new JCBase.UI.JCLabel(this.components);
            this.jclblIndoor_DropOutdoor = new JCBase.UI.JCLabel(this.components);
            this.jclblOutdoor_DropOutdoor = new JCBase.UI.JCLabel(this.components);
            this.jclblTitle = new JCBase.UI.JCLabel(this.components);
            this.lvOutdoor = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListController = new System.Windows.Forms.ImageList(this.components);
            this.pnlVisible = new System.Windows.Forms.Panel();
            this.pbWarning = new System.Windows.Forms.PictureBox();
            this.pbSetting = new System.Windows.Forms.PictureBox();
            this.pbRemove = new System.Windows.Forms.PictureBox();
            this.pnlSpan_left = new System.Windows.Forms.Panel();
            this.pnlSpan_top = new System.Windows.Forms.Panel();
            this.pnlVisible.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSetting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemove)).BeginInit();
            this.SuspendLayout();
            // 
            // jclblIndoorCount
            // 
            this.jclblIndoorCount.AutoSize = true;
            this.jclblIndoorCount.CausesValidation = false;
            this.jclblIndoorCount.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.jclblIndoorCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(92)))), ((int)(((byte)(116)))));
            this.jclblIndoorCount.FormatString = null;
            this.jclblIndoorCount.Location = new System.Drawing.Point(64, 76);
            this.jclblIndoorCount.Name = "jclblIndoorCount";
            this.jclblIndoorCount.Size = new System.Drawing.Size(0, 14);
            this.jclblIndoorCount.TabIndex = 2;
            // 
            // jclblOutdoorCount
            // 
            this.jclblOutdoorCount.AutoSize = true;
            this.jclblOutdoorCount.CausesValidation = false;
            this.jclblOutdoorCount.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.jclblOutdoorCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(92)))), ((int)(((byte)(116)))));
            this.jclblOutdoorCount.FormatString = null;
            this.jclblOutdoorCount.Location = new System.Drawing.Point(64, 56);
            this.jclblOutdoorCount.Name = "jclblOutdoorCount";
            this.jclblOutdoorCount.Size = new System.Drawing.Size(0, 14);
            this.jclblOutdoorCount.TabIndex = 2;
            // 
            // jclblIndoor_DropOutdoor
            // 
            this.jclblIndoor_DropOutdoor.AutoSize = true;
            this.jclblIndoor_DropOutdoor.CausesValidation = false;
            this.jclblIndoor_DropOutdoor.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jclblIndoor_DropOutdoor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(92)))), ((int)(((byte)(116)))));
            this.jclblIndoor_DropOutdoor.FormatString = null;
            this.jclblIndoor_DropOutdoor.Location = new System.Drawing.Point(4, 76);
            this.jclblIndoor_DropOutdoor.Name = "jclblIndoor_DropOutdoor";
            this.jclblIndoor_DropOutdoor.Size = new System.Drawing.Size(50, 13);
            this.jclblIndoor_DropOutdoor.TabIndex = 2;
            this.jclblIndoor_DropOutdoor.Text = "# Indoors";
            // 
            // jclblOutdoor_DropOutdoor
            // 
            this.jclblOutdoor_DropOutdoor.AutoSize = true;
            this.jclblOutdoor_DropOutdoor.CausesValidation = false;
            this.jclblOutdoor_DropOutdoor.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jclblOutdoor_DropOutdoor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(92)))), ((int)(((byte)(116)))));
            this.jclblOutdoor_DropOutdoor.FormatString = null;
            this.jclblOutdoor_DropOutdoor.Location = new System.Drawing.Point(4, 56);
            this.jclblOutdoor_DropOutdoor.Name = "jclblOutdoor_DropOutdoor";
            this.jclblOutdoor_DropOutdoor.Size = new System.Drawing.Size(58, 13);
            this.jclblOutdoor_DropOutdoor.TabIndex = 2;
            this.jclblOutdoor_DropOutdoor.Text = "# Outdoors";
            // 
            // jclblTitle
            // 
            this.jclblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(92)))), ((int)(((byte)(116)))));
            this.jclblTitle.CausesValidation = false;
            this.jclblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.jclblTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jclblTitle.ForeColor = System.Drawing.Color.White;
            this.jclblTitle.FormatString = null;
            this.jclblTitle.Location = new System.Drawing.Point(0, 0);
            this.jclblTitle.Name = "jclblTitle";
            this.jclblTitle.Size = new System.Drawing.Size(248, 23);
            this.jclblTitle.TabIndex = 0;
            this.jclblTitle.Text = "Outdoor Units";
            this.jclblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvOutdoor
            // 
            this.lvOutdoor.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvOutdoor.Dock = System.Windows.Forms.DockStyle.Left;
            this.lvOutdoor.FullRowSelect = true;
            this.lvOutdoor.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvOutdoor.LargeImageList = this.imageListController;
            this.lvOutdoor.Location = new System.Drawing.Point(4, 27);
            this.lvOutdoor.Name = "lvOutdoor";
            this.lvOutdoor.Size = new System.Drawing.Size(157, 139);
            this.lvOutdoor.SmallImageList = this.imageListController;
            this.lvOutdoor.TabIndex = 4;
            this.lvOutdoor.UseCompatibleStateImageBehavior = false;
            this.lvOutdoor.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 130;
            // 
            // imageListController
            // 
            this.imageListController.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListController.ImageStream")));
            this.imageListController.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListController.Images.SetKeyName(0, "Outdoor_nor.png");
            this.imageListController.Images.SetKeyName(1, "Outdoor_White.png");
            this.imageListController.Images.SetKeyName(2, "Outdoor with component_nor.png");
            this.imageListController.Images.SetKeyName(3, "Outdoor with component_nor-White.png");
            // 
            // pnlVisible
            // 
            this.pnlVisible.BackColor = System.Drawing.Color.Transparent;
            this.pnlVisible.Controls.Add(this.pbWarning);
            this.pnlVisible.Controls.Add(this.pbSetting);
            this.pnlVisible.Controls.Add(this.jclblOutdoor_DropOutdoor);
            this.pnlVisible.Controls.Add(this.jclblIndoor_DropOutdoor);
            this.pnlVisible.Controls.Add(this.pbRemove);
            this.pnlVisible.Controls.Add(this.jclblOutdoorCount);
            this.pnlVisible.Controls.Add(this.jclblIndoorCount);
            this.pnlVisible.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlVisible.Location = new System.Drawing.Point(161, 27);
            this.pnlVisible.Name = "pnlVisible";
            this.pnlVisible.Size = new System.Drawing.Size(87, 139);
            this.pnlVisible.TabIndex = 5;
            // 
            // pbWarning
            // 
            this.pbWarning.Image = global::JCHVRF.Properties.Resources.remove_nor;
            this.pbWarning.Location = new System.Drawing.Point(52, 107);
            this.pbWarning.Name = "pbWarning";
            this.pbWarning.Size = new System.Drawing.Size(25, 25);
            this.pbWarning.TabIndex = 5;
            this.pbWarning.TabStop = false;
            // 
            // pbSetting
            // 
            this.pbSetting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbSetting.Image = global::JCHVRF.Properties.Resources.setting_Nor;
            this.pbSetting.Location = new System.Drawing.Point(12, 10);
            this.pbSetting.Name = "pbSetting";
            this.pbSetting.Size = new System.Drawing.Size(25, 25);
            this.pbSetting.TabIndex = 4;
            this.pbSetting.TabStop = false;
            // 
            // pbRemove
            // 
            this.pbRemove.BackColor = System.Drawing.Color.Transparent;
            this.pbRemove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbRemove.Image = global::JCHVRF.Properties.Resources.trash_can_nor;
            this.pbRemove.Location = new System.Drawing.Point(52, 10);
            this.pbRemove.Name = "pbRemove";
            this.pbRemove.Size = new System.Drawing.Size(25, 25);
            this.pbRemove.TabIndex = 3;
            this.pbRemove.TabStop = false;
            // 
            // pnlSpan_left
            // 
            this.pnlSpan_left.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSpan_left.Location = new System.Drawing.Point(0, 27);
            this.pnlSpan_left.Name = "pnlSpan_left";
            this.pnlSpan_left.Size = new System.Drawing.Size(4, 139);
            this.pnlSpan_left.TabIndex = 6;
            // 
            // pnlSpan_top
            // 
            this.pnlSpan_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpan_top.Location = new System.Drawing.Point(0, 23);
            this.pnlSpan_top.Name = "pnlSpan_top";
            this.pnlSpan_top.Size = new System.Drawing.Size(248, 4);
            this.pnlSpan_top.TabIndex = 5;
            // 
            // ucDropOutdoor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(233)))), ((int)(((byte)(118)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lvOutdoor);
            this.Controls.Add(this.pnlSpan_left);
            this.Controls.Add(this.pnlVisible);
            this.Controls.Add(this.pnlSpan_top);
            this.Controls.Add(this.jclblTitle);
            this.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ucDropOutdoor";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.Size = new System.Drawing.Size(248, 170);
            this.pnlVisible.ResumeLayout(false);
            this.pnlVisible.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSetting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRemove)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.JCLabel jclblTitle;
        private JCBase.UI.JCLabel jclblOutdoor_DropOutdoor;
        private System.Windows.Forms.PictureBox pbRemove;
        private JCBase.UI.JCLabel jclblIndoor_DropOutdoor;
        private JCBase.UI.JCLabel jclblOutdoorCount;
        private JCBase.UI.JCLabel jclblIndoorCount;
        private System.Windows.Forms.ListView lvOutdoor;
        private System.Windows.Forms.Panel pnlVisible;
        private System.Windows.Forms.ImageList imageListController;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.PictureBox pbSetting;
        private System.Windows.Forms.Panel pnlSpan_left;
        private System.Windows.Forms.Panel pnlSpan_top;
        private System.Windows.Forms.PictureBox pbWarning;
    }
}
