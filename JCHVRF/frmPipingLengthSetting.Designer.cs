namespace JCHVRF
{
    partial class frmPipingLengthSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPipingLengthSetting));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHelp_1 = new System.Windows.Forms.Panel();
            this.jctxtFirstPipeLength = new JCBase.UI.JCTextBox(this.components);
            this.jcbtnSetLengthOK = new JCBase.UI.JCButton(this.components);
            this.jcLabel8 = new JCBase.UI.JCLabel(this.components);
            this.jctxtEqPipeLength = new JCBase.UI.JCTextBox(this.components);
            this.jclblFirstBranch = new JCBase.UI.JCLabel(this.components);
            this.jcLabel7 = new JCBase.UI.JCLabel(this.components);
            this.jclblPipingFactorValue = new JCBase.UI.JCLabel(this.components);
            this.jclblPipingLength = new JCBase.UI.JCLabel(this.components);
            this.jclblPipingFactor = new JCBase.UI.JCLabel(this.components);
            this.jclblEqPipeLength = new JCBase.UI.JCLabel(this.components);
            this.pnl_bottom = new System.Windows.Forms.Panel();
            this.jclblPipingPositionInd = new JCBase.UI.JCLabel(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.jcLabMaxOutdoorHeightDifference = new JCBase.UI.JCLabel(this.components);
            this.jcLabMaxIndoorHeightDifference = new JCBase.UI.JCLabel(this.components);
            this.jcbtnSetting = new JCBase.UI.JCButton(this.components);
            this.dgvIndoorDiff = new System.Windows.Forms.DataGridView();
            this.panelHighDifference = new System.Windows.Forms.Panel();
            this.btnControlCancel = new JCBase.UI.VRFButton(this.components);
            this.btnControlOK = new JCBase.UI.VRFButton(this.components);
            this.groupIndoorUnit = new System.Windows.Forms.GroupBox();
            this.cklistIndoor = new System.Windows.Forms.CheckedListBox();
            this.groupSetting = new System.Windows.Forms.GroupBox();
            this.jccmbPosition = new JCBase.UI.JCComboBox(this.components);
            this.jctxtPosition = new JCBase.UI.JCLabel(this.components);
            this.jctxtIndoorDifference = new JCBase.UI.JCTextBox(this.components);
            this.jctxtHigherDiff = new JCBase.UI.JCLabel(this.components);
            this.panel3 = new System.Windows.Forms.Panel();
            this.jclabTitle = new JCBase.UI.JCLabel(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.jcLabMsg = new JCBase.UI.JCLabel(this.components);
            this.pnlHelp_1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndoorDiff)).BeginInit();
            this.panelHighDifference.SuspendLayout();
            this.groupIndoorUnit.SuspendLayout();
            this.groupSetting.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHelp_1
            // 
            this.pnlHelp_1.BackColor = System.Drawing.Color.White;
            this.pnlHelp_1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlHelp_1.CausesValidation = false;
            this.pnlHelp_1.Controls.Add(this.jctxtFirstPipeLength);
            this.pnlHelp_1.Controls.Add(this.jcbtnSetLengthOK);
            this.pnlHelp_1.Controls.Add(this.jcLabel8);
            this.pnlHelp_1.Controls.Add(this.jctxtEqPipeLength);
            this.pnlHelp_1.Controls.Add(this.jclblFirstBranch);
            this.pnlHelp_1.Controls.Add(this.jcLabel7);
            this.pnlHelp_1.Controls.Add(this.jclblPipingFactorValue);
            this.pnlHelp_1.Controls.Add(this.jclblPipingLength);
            this.pnlHelp_1.Controls.Add(this.jclblPipingFactor);
            this.pnlHelp_1.Controls.Add(this.jclblEqPipeLength);
            resources.ApplyResources(this.pnlHelp_1, "pnlHelp_1");
            this.pnlHelp_1.Name = "pnlHelp_1";
            // 
            // jctxtFirstPipeLength
            // 
            this.jctxtFirstPipeLength.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtFirstPipeLength.ErrorMessage = null;
            resources.ApplyResources(this.jctxtFirstPipeLength, "jctxtFirstPipeLength");
            this.jctxtFirstPipeLength.JCFormatString = null;
            this.jctxtFirstPipeLength.JCMaxValue = null;
            this.jctxtFirstPipeLength.JCMinValue = null;
            this.jctxtFirstPipeLength.JCRegularExpression = null;
            this.jctxtFirstPipeLength.JCValidationType = JCBase.Validation.ValidationType.RANGE;
            this.jctxtFirstPipeLength.Name = "jctxtFirstPipeLength";
            this.jctxtFirstPipeLength.RequireValidation = true;
            this.jctxtFirstPipeLength.Leave += new System.EventHandler(this.jctxtFirstPipeLength_Leave);
            // 
            // jcbtnSetLengthOK
            // 
            this.jcbtnSetLengthOK.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.jcbtnSetLengthOK, "jcbtnSetLengthOK");
            this.jcbtnSetLengthOK.Name = "jcbtnSetLengthOK";
            this.jcbtnSetLengthOK.UseVisualStyleBackColor = false;
            this.jcbtnSetLengthOK.Click += new System.EventHandler(this.jcbtnSetLengthOK_Click);
            // 
            // jcLabel8
            // 
            resources.ApplyResources(this.jcLabel8, "jcLabel8");
            this.jcLabel8.CausesValidation = false;
            this.jcLabel8.FormatString = null;
            this.jcLabel8.Name = "jcLabel8";
            // 
            // jctxtEqPipeLength
            // 
            this.jctxtEqPipeLength.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtEqPipeLength.ErrorMessage = null;
            resources.ApplyResources(this.jctxtEqPipeLength, "jctxtEqPipeLength");
            this.jctxtEqPipeLength.JCFormatString = null;
            this.jctxtEqPipeLength.JCMaxValue = null;
            this.jctxtEqPipeLength.JCMinValue = null;
            this.jctxtEqPipeLength.JCRegularExpression = null;
            this.jctxtEqPipeLength.JCValidationType = JCBase.Validation.ValidationType.RANGE;
            this.jctxtEqPipeLength.Name = "jctxtEqPipeLength";
            this.jctxtEqPipeLength.RequireValidation = true;
            this.jctxtEqPipeLength.Leave += new System.EventHandler(this.jctxtEqPipeLength_Leave);
            // 
            // jclblFirstBranch
            // 
            this.jclblFirstBranch.AutoEllipsis = true;
            this.jclblFirstBranch.CausesValidation = false;
            resources.ApplyResources(this.jclblFirstBranch, "jclblFirstBranch");
            this.jclblFirstBranch.FormatString = null;
            this.jclblFirstBranch.Name = "jclblFirstBranch";
            // 
            // jcLabel7
            // 
            resources.ApplyResources(this.jcLabel7, "jcLabel7");
            this.jcLabel7.CausesValidation = false;
            this.jcLabel7.FormatString = null;
            this.jcLabel7.Name = "jcLabel7";
            // 
            // jclblPipingFactorValue
            // 
            this.jclblPipingFactorValue.CausesValidation = false;
            resources.ApplyResources(this.jclblPipingFactorValue, "jclblPipingFactorValue");
            this.jclblPipingFactorValue.ForeColor = System.Drawing.Color.Blue;
            this.jclblPipingFactorValue.FormatString = null;
            this.jclblPipingFactorValue.Name = "jclblPipingFactorValue";
            // 
            // jclblPipingLength
            // 
            this.jclblPipingLength.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jclblPipingLength.CausesValidation = false;
            resources.ApplyResources(this.jclblPipingLength, "jclblPipingLength");
            this.jclblPipingLength.ForeColor = System.Drawing.Color.White;
            this.jclblPipingLength.FormatString = null;
            this.jclblPipingLength.Name = "jclblPipingLength";
            // 
            // jclblPipingFactor
            // 
            resources.ApplyResources(this.jclblPipingFactor, "jclblPipingFactor");
            this.jclblPipingFactor.CausesValidation = false;
            this.jclblPipingFactor.FormatString = null;
            this.jclblPipingFactor.Name = "jclblPipingFactor";
            // 
            // jclblEqPipeLength
            // 
            this.jclblEqPipeLength.AutoEllipsis = true;
            this.jclblEqPipeLength.CausesValidation = false;
            resources.ApplyResources(this.jclblEqPipeLength, "jclblEqPipeLength");
            this.jclblEqPipeLength.FormatString = null;
            this.jclblEqPipeLength.Name = "jclblEqPipeLength";
            // 
            // pnl_bottom
            // 
            this.pnl_bottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(203)))), ((int)(((byte)(254)))));
            resources.ApplyResources(this.pnl_bottom, "pnl_bottom");
            this.pnl_bottom.Name = "pnl_bottom";
            // 
            // jclblPipingPositionInd
            // 
            this.jclblPipingPositionInd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(69)))), ((int)(((byte)(125)))));
            this.jclblPipingPositionInd.CausesValidation = false;
            resources.ApplyResources(this.jclblPipingPositionInd, "jclblPipingPositionInd");
            this.jclblPipingPositionInd.ForeColor = System.Drawing.Color.White;
            this.jclblPipingPositionInd.FormatString = null;
            this.jclblPipingPositionInd.Name = "jclblPipingPositionInd";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(125)))), ((int)(((byte)(81)))));
            this.panel2.Controls.Add(this.jcLabMaxOutdoorHeightDifference);
            this.panel2.Controls.Add(this.jcLabMaxIndoorHeightDifference);
            this.panel2.Controls.Add(this.jcbtnSetting);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // jcLabMaxOutdoorHeightDifference
            // 
            resources.ApplyResources(this.jcLabMaxOutdoorHeightDifference, "jcLabMaxOutdoorHeightDifference");
            this.jcLabMaxOutdoorHeightDifference.CausesValidation = false;
            this.jcLabMaxOutdoorHeightDifference.FormatString = null;
            this.jcLabMaxOutdoorHeightDifference.Name = "jcLabMaxOutdoorHeightDifference";
            // 
            // jcLabMaxIndoorHeightDifference
            // 
            resources.ApplyResources(this.jcLabMaxIndoorHeightDifference, "jcLabMaxIndoorHeightDifference");
            this.jcLabMaxIndoorHeightDifference.CausesValidation = false;
            this.jcLabMaxIndoorHeightDifference.FormatString = null;
            this.jcLabMaxIndoorHeightDifference.Name = "jcLabMaxIndoorHeightDifference";
            // 
            // jcbtnSetting
            // 
            this.jcbtnSetting.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.jcbtnSetting, "jcbtnSetting");
            this.jcbtnSetting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnSetting.Name = "jcbtnSetting";
            this.jcbtnSetting.UseVisualStyleBackColor = false;
            this.jcbtnSetting.Click += new System.EventHandler(this.jcbtnSetting_Click);
            // 
            // dgvIndoorDiff
            // 
            this.dgvIndoorDiff.AllowUserToAddRows = false;
            this.dgvIndoorDiff.AllowUserToDeleteRows = false;
            this.dgvIndoorDiff.AllowUserToResizeColumns = false;
            this.dgvIndoorDiff.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgvIndoorDiff.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvIndoorDiff.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvIndoorDiff.BackgroundColor = System.Drawing.Color.White;
            this.dgvIndoorDiff.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvIndoorDiff.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(112)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(112)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndoorDiff.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dgvIndoorDiff, "dgvIndoorDiff");
            this.dgvIndoorDiff.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndoorDiff.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvIndoorDiff.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvIndoorDiff.EnableHeadersVisualStyles = false;
            this.dgvIndoorDiff.Name = "dgvIndoorDiff";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndoorDiff.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvIndoorDiff.RowHeadersVisible = false;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.dgvIndoorDiff.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvIndoorDiff.RowTemplate.Height = 24;
            this.dgvIndoorDiff.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIndoorDiff.ShowEditingIcon = false;
            this.dgvIndoorDiff.TabStop = false;
            this.dgvIndoorDiff.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvIndoorDiff_CellPainting);
            this.dgvIndoorDiff.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIndoorDiff_RowEnter);
            // 
            // panelHighDifference
            // 
            this.panelHighDifference.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHighDifference.Controls.Add(this.btnControlCancel);
            this.panelHighDifference.Controls.Add(this.btnControlOK);
            this.panelHighDifference.Controls.Add(this.groupIndoorUnit);
            this.panelHighDifference.Controls.Add(this.groupSetting);
            this.panelHighDifference.Controls.Add(this.panel3);
            resources.ApplyResources(this.panelHighDifference, "panelHighDifference");
            this.panelHighDifference.Name = "panelHighDifference";
            this.panelHighDifference.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelHighDifference_MouseDown);
            this.panelHighDifference.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelHighDifference_MouseMove);
            // 
            // btnControlCancel
            // 
            this.btnControlCancel.AutoEllipsis = true;
            resources.ApplyResources(this.btnControlCancel, "btnControlCancel");
            this.btnControlCancel.FlatAppearance.BorderSize = 0;
            this.btnControlCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnControlCancel.Name = "btnControlCancel";
            this.btnControlCancel.Click += new System.EventHandler(this.btnControlCancel_Click);
            // 
            // btnControlOK
            // 
            this.btnControlOK.AutoEllipsis = true;
            resources.ApplyResources(this.btnControlOK, "btnControlOK");
            this.btnControlOK.FlatAppearance.BorderSize = 0;
            this.btnControlOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.btnControlOK.Name = "btnControlOK";
            this.btnControlOK.Click += new System.EventHandler(this.btnControlOK_Click);
            // 
            // groupIndoorUnit
            // 
            this.groupIndoorUnit.Controls.Add(this.cklistIndoor);
            resources.ApplyResources(this.groupIndoorUnit, "groupIndoorUnit");
            this.groupIndoorUnit.Name = "groupIndoorUnit";
            this.groupIndoorUnit.TabStop = false;
            // 
            // cklistIndoor
            // 
            this.cklistIndoor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.cklistIndoor, "cklistIndoor");
            this.cklistIndoor.FormattingEnabled = true;
            this.cklistIndoor.Name = "cklistIndoor";
            // 
            // groupSetting
            // 
            this.groupSetting.Controls.Add(this.jccmbPosition);
            this.groupSetting.Controls.Add(this.jctxtPosition);
            this.groupSetting.Controls.Add(this.jctxtIndoorDifference);
            this.groupSetting.Controls.Add(this.jctxtHigherDiff);
            resources.ApplyResources(this.groupSetting, "groupSetting");
            this.groupSetting.Name = "groupSetting";
            this.groupSetting.TabStop = false;
            // 
            // jccmbPosition
            // 
            this.jccmbPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbPosition.DropDownWidth = 178;
            this.jccmbPosition.ErrorMessage = null;
            resources.ApplyResources(this.jccmbPosition, "jccmbPosition");
            this.jccmbPosition.FormattingEnabled = true;
            this.jccmbPosition.JCIgnoreSelectItem = null;
            this.jccmbPosition.Name = "jccmbPosition";
            this.jccmbPosition.SelectionChangeCommitted += new System.EventHandler(this.jccmbPosition_SelectionChangeCommitted);
            // 
            // jctxtPosition
            // 
            this.jctxtPosition.AutoEllipsis = true;
            this.jctxtPosition.CausesValidation = false;
            resources.ApplyResources(this.jctxtPosition, "jctxtPosition");
            this.jctxtPosition.FormatString = null;
            this.jctxtPosition.Name = "jctxtPosition";
            // 
            // jctxtIndoorDifference
            // 
            this.jctxtIndoorDifference.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtIndoorDifference.ErrorMessage = null;
            resources.ApplyResources(this.jctxtIndoorDifference, "jctxtIndoorDifference");
            this.jctxtIndoorDifference.JCFormatString = null;
            this.jctxtIndoorDifference.JCMaxValue = null;
            this.jctxtIndoorDifference.JCMinValue = null;
            this.jctxtIndoorDifference.JCRegularExpression = null;
            this.jctxtIndoorDifference.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            this.jctxtIndoorDifference.Name = "jctxtIndoorDifference";
            this.jctxtIndoorDifference.RequireValidation = true;
            this.jctxtIndoorDifference.Leave += new System.EventHandler(this.jctxtIndoorDifference_TextChanged);
            // 
            // jctxtHigherDiff
            // 
            this.jctxtHigherDiff.AutoEllipsis = true;
            this.jctxtHigherDiff.CausesValidation = false;
            resources.ApplyResources(this.jctxtHigherDiff, "jctxtHigherDiff");
            this.jctxtHigherDiff.FormatString = null;
            this.jctxtHigherDiff.Name = "jctxtHigherDiff";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(88)))), ((int)(((byte)(131)))));
            this.panel3.Controls.Add(this.jclabTitle);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            this.panel3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelHighDifference_MouseDown);
            this.panel3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelHighDifference_MouseMove);
            // 
            // jclabTitle
            // 
            resources.ApplyResources(this.jclabTitle, "jclabTitle");
            this.jclabTitle.CausesValidation = false;
            this.jclabTitle.ForeColor = System.Drawing.Color.White;
            this.jclabTitle.FormatString = null;
            this.jclabTitle.Name = "jclabTitle";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.dgvIndoorDiff);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.jclblPipingPositionInd);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // jcLabMsg
            // 
            this.jcLabMsg.BackColor = System.Drawing.Color.White;
            this.jcLabMsg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.jcLabMsg.CausesValidation = false;
            resources.ApplyResources(this.jcLabMsg, "jcLabMsg");
            this.jcLabMsg.ForeColor = System.Drawing.Color.Red;
            this.jcLabMsg.FormatString = null;
            this.jcLabMsg.Name = "jcLabMsg";
            // 
            // frmPipingLengthSetting
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panelHighDifference);
            this.Controls.Add(this.jcLabMsg);
            this.Controls.Add(this.pnl_bottom);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlHelp_1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmPipingLengthSetting";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPipingLengthSetting_FormClosing);
            this.pnlHelp_1.ResumeLayout(false);
            this.pnlHelp_1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndoorDiff)).EndInit();
            this.panelHighDifference.ResumeLayout(false);
            this.groupIndoorUnit.ResumeLayout(false);
            this.groupSetting.ResumeLayout(false);
            this.groupSetting.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHelp_1;
        private JCBase.UI.JCTextBox jctxtFirstPipeLength;
        private JCBase.UI.JCLabel jcLabel8;
        private JCBase.UI.JCTextBox jctxtEqPipeLength;
        private JCBase.UI.JCLabel jclblFirstBranch;
        private JCBase.UI.JCLabel jcLabel7;
        private JCBase.UI.JCLabel jclblPipingFactorValue;
        private JCBase.UI.JCLabel jclblPipingLength;
        private JCBase.UI.JCLabel jclblPipingFactor;
        private JCBase.UI.JCLabel jclblEqPipeLength;
        private JCBase.UI.JCButton jcbtnSetLengthOK;
        private System.Windows.Forms.Panel pnl_bottom;
        private JCBase.UI.JCLabel jclblPipingPositionInd;
        private System.Windows.Forms.Panel panel2;
        private JCBase.UI.JCLabel jcLabMaxOutdoorHeightDifference;
        private JCBase.UI.JCLabel jcLabMaxIndoorHeightDifference;
        private JCBase.UI.JCButton jcbtnSetting;
        private System.Windows.Forms.DataGridView dgvIndoorDiff;
        private System.Windows.Forms.Panel panelHighDifference;
        private System.Windows.Forms.GroupBox groupSetting;
        private JCBase.UI.JCComboBox jccmbPosition;
        private JCBase.UI.JCLabel jctxtPosition;
        private JCBase.UI.JCTextBox jctxtIndoorDifference;
        private JCBase.UI.JCLabel jctxtHigherDiff;
        private System.Windows.Forms.Panel panel3;
        private JCBase.UI.JCLabel jclabTitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupIndoorUnit;
        private System.Windows.Forms.CheckedListBox cklistIndoor;
        private JCBase.UI.VRFButton btnControlOK;
        private JCBase.UI.VRFButton btnControlCancel;
        private JCBase.UI.JCLabel jcLabMsg;
    }
}