namespace JCHVRF
{
    partial class ucDropControlGroup
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ucDropOutdoor1 = new JCHVRF.ucDropOutdoor();
            this.ucDropController1 = new JCHVRF.ucDropController();
            this.SuspendLayout();
            // 
            // jclblTitle
            // 
            this.jclblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(194)))), ((int)(((byte)(198)))));
            this.jclblTitle.CausesValidation = false;
            this.jclblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.jclblTitle.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.jclblTitle.ForeColor = System.Drawing.Color.White;
            this.jclblTitle.FormatString = null;
            this.jclblTitle.Location = new System.Drawing.Point(0, 0);
            this.jclblTitle.Name = "jclblTitle";
            this.jclblTitle.Size = new System.Drawing.Size(360, 23);
            this.jclblTitle.TabIndex = 1;
            this.jclblTitle.Text = "Control Group";
            this.jclblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 4);
            this.panel1.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(4, 191);
            this.panel2.TabIndex = 6;
            // 
            // ucDropOutdoor1
            // 
            this.ucDropOutdoor1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.ucDropOutdoor1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ucDropOutdoor1.Dock = System.Windows.Forms.DockStyle.Left;
            this.ucDropOutdoor1.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ucDropOutdoor1.Location = new System.Drawing.Point(4, 27);
            this.ucDropOutdoor1.Name = "ucDropOutdoor1";
            this.ucDropOutdoor1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.ucDropOutdoor1.Size = new System.Drawing.Size(248, 191);
            this.ucDropOutdoor1.TabIndex = 4;
            this.ucDropOutdoor1.Title = null;
            // 
            // ucDropController1
            // 
            this.ucDropController1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(225)))));
            this.ucDropController1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ucDropController1.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ucDropController1.Location = new System.Drawing.Point(256, 27);
            this.ucDropController1.Margin = new System.Windows.Forms.Padding(0);
            this.ucDropController1.Name = "ucDropController1";
            this.ucDropController1.Size = new System.Drawing.Size(98, 92);
            this.ucDropController1.TabIndex = 7;
            this.ucDropController1.Title = "Controller";
            // 
            // ucDropControlGroup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ucDropController1);
            this.Controls.Add(this.ucDropOutdoor1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.jclblTitle);
            this.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ucDropControlGroup";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.Size = new System.Drawing.Size(360, 222);
            this.Load += new System.EventHandler(this.ucDropControlGroup_Load);
            this.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.ucDropControllerGroup_ControlAdded);
            this.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.ucDropControllerGroup_ControlRemoved);
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.JCLabel jclblTitle;
        private ucDropOutdoor ucDropOutdoor1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private ucDropController ucDropController1;

    }
}
