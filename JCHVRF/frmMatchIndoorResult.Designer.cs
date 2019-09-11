namespace JCHVRF
{
    partial class frmMatchIndoorResult
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMatchIndoorResult));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.jclblError = new JCBase.UI.JCLabel(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.jcbtnClose = new JCBase.UI.VRFButton(this.components);
            this.jcbtnOK = new JCBase.UI.VRFButton(this.components);
            this.Model = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Memo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Model,
            this.Memo});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 25;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.jclblError);
            this.panel1.Controls.Add(this.dataGridView1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // jclblError
            // 
            this.jclblError.AutoEllipsis = true;
            this.jclblError.BackColor = System.Drawing.Color.White;
            this.jclblError.CausesValidation = false;
            resources.ApplyResources(this.jclblError, "jclblError");
            this.jclblError.ForeColor = System.Drawing.Color.Red;
            this.jclblError.FormatString = null;
            this.jclblError.Name = "jclblError";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.jcbtnClose);
            this.panel2.Controls.Add(this.jcbtnOK);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // jcbtnClose
            // 
            resources.ApplyResources(this.jcbtnClose, "jcbtnClose");
            this.jcbtnClose.FlatAppearance.BorderSize = 0;
            this.jcbtnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnClose.Name = "jcbtnClose";
            this.jcbtnClose.UseVisualStyleBackColor = true;
            this.jcbtnClose.Click += new System.EventHandler(this.jcbtnClose_Click);
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
            // Model
            // 
            this.Model.DataPropertyName = "Model";
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Model.DefaultCellStyle = dataGridViewCellStyle1;
            this.Model.FillWeight = 46F;
            resources.ApplyResources(this.Model, "Model");
            this.Model.Name = "Model";
            this.Model.ReadOnly = true;
            // 
            // Memo
            // 
            this.Memo.DataPropertyName = "Memo";
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Memo.DefaultCellStyle = dataGridViewCellStyle2;
            this.Memo.FillWeight = 159.3909F;
            resources.ApplyResources(this.Memo, "Memo");
            this.Memo.Name = "Memo";
            this.Memo.ReadOnly = true;
            // 
            // frmMatchIndoorResult
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.MaximizeBox = false;
            this.Name = "frmMatchIndoorResult";
            this.Load += new System.EventHandler(this.frmMatchIndoorResult_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private JCBase.UI.VRFButton jcbtnOK;
        private JCBase.UI.VRFButton jcbtnClose;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private JCBase.UI.JCLabel jclblError;
        private System.Windows.Forms.DataGridViewTextBoxColumn Model;
        private System.Windows.Forms.DataGridViewTextBoxColumn Memo;

    }
}