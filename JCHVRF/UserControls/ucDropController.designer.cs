namespace JCHVRF
{
    partial class ucDropController
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
            this.jclblTitle = new JCBase.UI.JCLabel(this.components);
            this.pbRemove = new System.Windows.Forms.PictureBox();
            this.pbController = new System.Windows.Forms.PictureBox();
            this.lblQuantity = new System.Windows.Forms.Label();
            this.pbAdd = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbRemove)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbController)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdd)).BeginInit();
            this.SuspendLayout();
            // 
            // jclblTitle
            // 
            this.jclblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(194)))), ((int)(((byte)(198)))));
            this.jclblTitle.CausesValidation = false;
            this.jclblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.jclblTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.jclblTitle.ForeColor = System.Drawing.Color.White;
            this.jclblTitle.FormatString = null;
            this.jclblTitle.Location = new System.Drawing.Point(0, 0);
            this.jclblTitle.Name = "jclblTitle";
            this.jclblTitle.Size = new System.Drawing.Size(98, 23);
            this.jclblTitle.TabIndex = 1;
            this.jclblTitle.Text = "Controller";
            this.jclblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbRemove
            // 
            this.pbRemove.BackColor = System.Drawing.Color.Transparent;
            this.pbRemove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbRemove.Image = global::JCHVRF.Properties.Resources.trash_can_nor;
            this.pbRemove.Location = new System.Drawing.Point(69, 29);
            this.pbRemove.Margin = new System.Windows.Forms.Padding(0);
            this.pbRemove.Name = "pbRemove";
            this.pbRemove.Size = new System.Drawing.Size(24, 24);
            this.pbRemove.TabIndex = 4;
            this.pbRemove.TabStop = false;
            // 
            // pbController
            // 
            this.pbController.BackColor = System.Drawing.Color.White;
            this.pbController.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbController.Location = new System.Drawing.Point(4, 26);
            this.pbController.Margin = new System.Windows.Forms.Padding(0);
            this.pbController.Name = "pbController";
            this.pbController.Size = new System.Drawing.Size(61, 61);
            this.pbController.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbController.TabIndex = 2;
            this.pbController.TabStop = false;
            // 
            // lblQuantity
            // 
            this.lblQuantity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(223)))), ((int)(((byte)(36)))));
            this.lblQuantity.Font = new System.Drawing.Font("Arial Black", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblQuantity.ForeColor = System.Drawing.Color.Blue;
            this.lblQuantity.Location = new System.Drawing.Point(66, 2);
            this.lblQuantity.Name = "lblQuantity";
            this.lblQuantity.Size = new System.Drawing.Size(28, 18);
            this.lblQuantity.TabIndex = 6;
            this.lblQuantity.Text = "1";
            this.lblQuantity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbAdd
            // 
            this.pbAdd.BackColor = System.Drawing.Color.Transparent;
            this.pbAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbAdd.Image = global::JCHVRF.Properties.Resources.Simple_unit_add_nr_48x48;
            this.pbAdd.Location = new System.Drawing.Point(69, 60);
            this.pbAdd.Margin = new System.Windows.Forms.Padding(0);
            this.pbAdd.Name = "pbAdd";
            this.pbAdd.Size = new System.Drawing.Size(24, 24);
            this.pbAdd.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbAdd.TabIndex = 7;
            this.pbAdd.TabStop = false;
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 0;
            this.toolTip1.UseAnimation = false;
            this.toolTip1.UseFading = false;
            // 
            // ucDropController
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lblQuantity);
            this.Controls.Add(this.pbAdd);
            this.Controls.Add(this.pbRemove);
            this.Controls.Add(this.jclblTitle);
            this.Controls.Add(this.pbController);
            this.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ucDropController";
            this.Size = new System.Drawing.Size(98, 92);
            ((System.ComponentModel.ISupportInitialize)(this.pbRemove)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbController)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAdd)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.JCLabel jclblTitle;
        private System.Windows.Forms.PictureBox pbController;
        private System.Windows.Forms.PictureBox pbRemove;
        private System.Windows.Forms.Label lblQuantity;
        private System.Windows.Forms.PictureBox pbAdd;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
