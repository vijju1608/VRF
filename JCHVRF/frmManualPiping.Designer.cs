namespace JCHVRF
{
    partial class frmManualPiping
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManualPiping));
            this.tvOutdoor = new System.Windows.Forms.TreeView();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.jcLabel1 = new JCBase.UI.JCLabel(this.components);
            this.jccmbScale = new JCBase.UI.JCComboBox(this.components);
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.jcbtnEnablePlottingScale = new JCBase.UI.VRFButton(this.components);
            this.pbPlottingScaleRotation = new System.Windows.Forms.PictureBox();
            this.pbPlottingScaleColor = new System.Windows.Forms.PictureBox();
            this.jctxtPlottingScale = new JCBase.UI.JCTextBox(this.components);
            this.jclblPlottingScaleUnit = new JCBase.UI.JCLabel(this.components);
            this.jclblPlottingScaleNotAvailable = new JCBase.UI.JCLabel(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblNodeBgColor = new JCBase.UI.JCLabel(this.components);
            this.pbNodeBgColor = new System.Windows.Forms.PictureBox();
            this.lblBranchKitColor = new JCBase.UI.JCLabel(this.components);
            this.pbBranchKitColor = new System.Windows.Forms.PictureBox();
            this.lblTextColor = new JCBase.UI.JCLabel(this.components);
            this.lblLineColor = new JCBase.UI.JCLabel(this.components);
            this.pbTextColor = new System.Windows.Forms.PictureBox();
            this.pbLineColor = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rdoLineStyleVHV = new System.Windows.Forms.RadioButton();
            this.rdoLineStyleHVH = new System.Windows.Forms.RadioButton();
            this.rdoLineStyleVH = new System.Windows.Forms.RadioButton();
            this.rdoLineStyleHV = new System.Windows.Forms.RadioButton();
            this.rdoLineStylePolyLine = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pbAddBuildingImage = new JCBase.UI.VRFIcon(this.components);
            this.pbDeleteBgImage = new JCBase.UI.VRFIcon(this.components);
            this.chkBuildingImageLocked = new JCBase.UI.uc_CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.jcbtnClear = new JCBase.UI.VRFButton(this.components);
            this.jcbtnCancel = new JCBase.UI.VRFButton(this.components);
            this.jcbtnOK = new JCBase.UI.VRFButton(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuAddInd = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAddYP = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAddCP = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAddCHBox = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAddMultiCHBox = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDeleteNode = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pbUndo = new System.Windows.Forms.PictureBox();
            this.pbRedo = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.addFlowPiping = new Lassalle.Flow.AddFlow();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlTop.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlottingScaleRotation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlottingScaleColor)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbNodeBgColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBranchKitColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbTextColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLineColor)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddBuildingImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeleteBgImage)).BeginInit();
            this.panel2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbUndo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRedo)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvOutdoor
            // 
            this.tvOutdoor.AllowDrop = true;
            this.tvOutdoor.BackColor = System.Drawing.Color.White;
            this.tvOutdoor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.tvOutdoor, "tvOutdoor");
            this.tvOutdoor.FullRowSelect = true;
            this.tvOutdoor.ItemHeight = 30;
            this.tvOutdoor.Name = "tvOutdoor";
            this.tvOutdoor.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("tvOutdoor.Nodes")))});
            this.tvOutdoor.ShowLines = false;
            this.tvOutdoor.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvOutdoor_BeforeSelect);
            this.tvOutdoor.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvOutdoor_AfterSelect);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.tableLayoutPanel1.SetColumnSpan(this.pnlTop, 2);
            this.pnlTop.Controls.Add(this.groupBox5);
            this.pnlTop.Controls.Add(this.groupBox4);
            this.pnlTop.Controls.Add(this.groupBox3);
            this.pnlTop.Controls.Add(this.groupBox2);
            this.pnlTop.Controls.Add(this.groupBox1);
            resources.ApplyResources(this.pnlTop, "pnlTop");
            this.pnlTop.Name = "pnlTop";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.jcLabel1);
            this.groupBox5.Controls.Add(this.jccmbScale);
            resources.ApplyResources(this.groupBox5, "groupBox5");
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.TabStop = false;
            // 
            // jcLabel1
            // 
            resources.ApplyResources(this.jcLabel1, "jcLabel1");
            this.jcLabel1.CausesValidation = false;
            this.jcLabel1.FormatString = null;
            this.jcLabel1.Name = "jcLabel1";
            // 
            // jccmbScale
            // 
            this.jccmbScale.AllowNull = false;
            this.jccmbScale.BackColor = System.Drawing.Color.MistyRose;
            this.jccmbScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jccmbScale.ErrorMessage = null;
            resources.ApplyResources(this.jccmbScale, "jccmbScale");
            this.jccmbScale.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.jccmbScale.FormattingEnabled = true;
            this.jccmbScale.Items.AddRange(new object[] {
            resources.GetString("jccmbScale.Items"),
            resources.GetString("jccmbScale.Items1"),
            resources.GetString("jccmbScale.Items2"),
            resources.GetString("jccmbScale.Items3"),
            resources.GetString("jccmbScale.Items4"),
            resources.GetString("jccmbScale.Items5"),
            resources.GetString("jccmbScale.Items6"),
            resources.GetString("jccmbScale.Items7")});
            this.jccmbScale.JCIgnoreSelectItem = null;
            this.jccmbScale.Name = "jccmbScale";
            this.jccmbScale.RequireValidation = true;
            this.jccmbScale.SelectedIndexChanged += new System.EventHandler(this.jccmbScale_SelectedIndexChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.jcbtnEnablePlottingScale);
            this.groupBox4.Controls.Add(this.pbPlottingScaleRotation);
            this.groupBox4.Controls.Add(this.pbPlottingScaleColor);
            this.groupBox4.Controls.Add(this.jctxtPlottingScale);
            this.groupBox4.Controls.Add(this.jclblPlottingScaleUnit);
            this.groupBox4.Controls.Add(this.jclblPlottingScaleNotAvailable);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // jcbtnEnablePlottingScale
            // 
            resources.ApplyResources(this.jcbtnEnablePlottingScale, "jcbtnEnablePlottingScale");
            this.jcbtnEnablePlottingScale.FlatAppearance.BorderSize = 0;
            this.jcbtnEnablePlottingScale.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnEnablePlottingScale.Name = "jcbtnEnablePlottingScale";
            this.jcbtnEnablePlottingScale.UseVisualStyleBackColor = true;
            this.jcbtnEnablePlottingScale.Click += new System.EventHandler(this.jcbtnEnablePlottingScale_Click);
            // 
            // pbPlottingScaleRotation
            // 
            this.pbPlottingScaleRotation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbPlottingScaleRotation.Image = global::JCHVRF.Properties.Resources.PlottingScale_V;
            resources.ApplyResources(this.pbPlottingScaleRotation, "pbPlottingScaleRotation");
            this.pbPlottingScaleRotation.Name = "pbPlottingScaleRotation";
            this.pbPlottingScaleRotation.TabStop = false;
            this.toolTip1.SetToolTip(this.pbPlottingScaleRotation, resources.GetString("pbPlottingScaleRotation.ToolTip"));
            this.pbPlottingScaleRotation.Click += new System.EventHandler(this.pbPlottingScaleRotation_Click);
            // 
            // pbPlottingScaleColor
            // 
            this.pbPlottingScaleColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pbPlottingScaleColor, "pbPlottingScaleColor");
            this.pbPlottingScaleColor.Name = "pbPlottingScaleColor";
            this.pbPlottingScaleColor.TabStop = false;
            this.toolTip1.SetToolTip(this.pbPlottingScaleColor, resources.GetString("pbPlottingScaleColor.ToolTip"));
            this.pbPlottingScaleColor.Click += new System.EventHandler(this.pbPlottingScaleColor_Click);
            // 
            // jctxtPlottingScale
            // 
            this.jctxtPlottingScale.BackColor = System.Drawing.Color.MistyRose;
            this.jctxtPlottingScale.ErrorMessage = null;
            resources.ApplyResources(this.jctxtPlottingScale, "jctxtPlottingScale");
            this.jctxtPlottingScale.JCFormatString = null;
            this.jctxtPlottingScale.JCMaxValue = null;
            this.jctxtPlottingScale.JCMinValue = null;
            this.jctxtPlottingScale.JCRegularExpression = null;
            this.jctxtPlottingScale.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            this.jctxtPlottingScale.Name = "jctxtPlottingScale";
            this.jctxtPlottingScale.RequireValidation = true;
            this.jctxtPlottingScale.TextChanged += new System.EventHandler(this.jctxtPlottingScale_TextChanged);
            // 
            // jclblPlottingScaleUnit
            // 
            resources.ApplyResources(this.jclblPlottingScaleUnit, "jclblPlottingScaleUnit");
            this.jclblPlottingScaleUnit.CausesValidation = false;
            this.jclblPlottingScaleUnit.FormatString = null;
            this.jclblPlottingScaleUnit.Name = "jclblPlottingScaleUnit";
            // 
            // jclblPlottingScaleNotAvailable
            // 
            this.jclblPlottingScaleNotAvailable.CausesValidation = false;
            resources.ApplyResources(this.jclblPlottingScaleNotAvailable, "jclblPlottingScaleNotAvailable");
            this.jclblPlottingScaleNotAvailable.FormatString = null;
            this.jclblPlottingScaleNotAvailable.Name = "jclblPlottingScaleNotAvailable";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblNodeBgColor);
            this.groupBox3.Controls.Add(this.pbNodeBgColor);
            this.groupBox3.Controls.Add(this.lblBranchKitColor);
            this.groupBox3.Controls.Add(this.pbBranchKitColor);
            this.groupBox3.Controls.Add(this.lblTextColor);
            this.groupBox3.Controls.Add(this.lblLineColor);
            this.groupBox3.Controls.Add(this.pbTextColor);
            this.groupBox3.Controls.Add(this.pbLineColor);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // lblNodeBgColor
            // 
            resources.ApplyResources(this.lblNodeBgColor, "lblNodeBgColor");
            this.lblNodeBgColor.CausesValidation = false;
            this.lblNodeBgColor.ForeColor = System.Drawing.Color.Blue;
            this.lblNodeBgColor.FormatString = null;
            this.lblNodeBgColor.Name = "lblNodeBgColor";
            this.lblNodeBgColor.Click += new System.EventHandler(this.lblNodeBgColor_Click);
            // 
            // pbNodeBgColor
            // 
            this.pbNodeBgColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pbNodeBgColor, "pbNodeBgColor");
            this.pbNodeBgColor.Name = "pbNodeBgColor";
            this.pbNodeBgColor.TabStop = false;
            this.toolTip1.SetToolTip(this.pbNodeBgColor, resources.GetString("pbNodeBgColor.ToolTip"));
            this.pbNodeBgColor.Click += new System.EventHandler(this.pbNodeBgColor_Click);
            // 
            // lblBranchKitColor
            // 
            resources.ApplyResources(this.lblBranchKitColor, "lblBranchKitColor");
            this.lblBranchKitColor.CausesValidation = false;
            this.lblBranchKitColor.ForeColor = System.Drawing.Color.Blue;
            this.lblBranchKitColor.FormatString = null;
            this.lblBranchKitColor.Name = "lblBranchKitColor";
            this.lblBranchKitColor.Click += new System.EventHandler(this.lblBranchKitColor_Click);
            // 
            // pbBranchKitColor
            // 
            this.pbBranchKitColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pbBranchKitColor, "pbBranchKitColor");
            this.pbBranchKitColor.Name = "pbBranchKitColor";
            this.pbBranchKitColor.TabStop = false;
            this.toolTip1.SetToolTip(this.pbBranchKitColor, resources.GetString("pbBranchKitColor.ToolTip"));
            this.pbBranchKitColor.Click += new System.EventHandler(this.pbBranchKitColor_Click);
            // 
            // lblTextColor
            // 
            resources.ApplyResources(this.lblTextColor, "lblTextColor");
            this.lblTextColor.CausesValidation = false;
            this.lblTextColor.ForeColor = System.Drawing.Color.Blue;
            this.lblTextColor.FormatString = null;
            this.lblTextColor.Name = "lblTextColor";
            this.lblTextColor.Click += new System.EventHandler(this.lblTextColor_Click);
            // 
            // lblLineColor
            // 
            resources.ApplyResources(this.lblLineColor, "lblLineColor");
            this.lblLineColor.CausesValidation = false;
            this.lblLineColor.ForeColor = System.Drawing.Color.Blue;
            this.lblLineColor.FormatString = null;
            this.lblLineColor.Name = "lblLineColor";
            this.lblLineColor.Click += new System.EventHandler(this.lblLineColor_Click);
            // 
            // pbTextColor
            // 
            this.pbTextColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pbTextColor, "pbTextColor");
            this.pbTextColor.Name = "pbTextColor";
            this.pbTextColor.TabStop = false;
            this.toolTip1.SetToolTip(this.pbTextColor, resources.GetString("pbTextColor.ToolTip"));
            this.pbTextColor.Click += new System.EventHandler(this.pbTextColor_Click);
            // 
            // pbLineColor
            // 
            this.pbLineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.pbLineColor, "pbLineColor");
            this.pbLineColor.Name = "pbLineColor";
            this.pbLineColor.TabStop = false;
            this.toolTip1.SetToolTip(this.pbLineColor, resources.GetString("pbLineColor.ToolTip"));
            this.pbLineColor.Click += new System.EventHandler(this.pbLineColor_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rdoLineStyleVHV);
            this.groupBox2.Controls.Add(this.rdoLineStyleHVH);
            this.groupBox2.Controls.Add(this.rdoLineStyleVH);
            this.groupBox2.Controls.Add(this.rdoLineStyleHV);
            this.groupBox2.Controls.Add(this.rdoLineStylePolyLine);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // rdoLineStyleVHV
            // 
            resources.ApplyResources(this.rdoLineStyleVHV, "rdoLineStyleVHV");
            this.rdoLineStyleVHV.Image = global::JCHVRF.Properties.Resources.LineStyle_VHV;
            this.rdoLineStyleVHV.Name = "rdoLineStyleVHV";
            this.rdoLineStyleVHV.TabStop = true;
            this.toolTip1.SetToolTip(this.rdoLineStyleVHV, resources.GetString("rdoLineStyleVHV.ToolTip"));
            this.rdoLineStyleVHV.UseVisualStyleBackColor = true;
            this.rdoLineStyleVHV.CheckedChanged += new System.EventHandler(this.rdoLineStyleVHV_CheckedChanged);
            // 
            // rdoLineStyleHVH
            // 
            resources.ApplyResources(this.rdoLineStyleHVH, "rdoLineStyleHVH");
            this.rdoLineStyleHVH.Image = global::JCHVRF.Properties.Resources.LineStyle_HVH;
            this.rdoLineStyleHVH.Name = "rdoLineStyleHVH";
            this.rdoLineStyleHVH.TabStop = true;
            this.toolTip1.SetToolTip(this.rdoLineStyleHVH, resources.GetString("rdoLineStyleHVH.ToolTip"));
            this.rdoLineStyleHVH.UseVisualStyleBackColor = true;
            this.rdoLineStyleHVH.CheckedChanged += new System.EventHandler(this.rdoLineStyleHVH_CheckedChanged);
            // 
            // rdoLineStyleVH
            // 
            resources.ApplyResources(this.rdoLineStyleVH, "rdoLineStyleVH");
            this.rdoLineStyleVH.Image = global::JCHVRF.Properties.Resources.LineStyle_VH;
            this.rdoLineStyleVH.Name = "rdoLineStyleVH";
            this.rdoLineStyleVH.TabStop = true;
            this.toolTip1.SetToolTip(this.rdoLineStyleVH, resources.GetString("rdoLineStyleVH.ToolTip"));
            this.rdoLineStyleVH.UseVisualStyleBackColor = true;
            this.rdoLineStyleVH.CheckedChanged += new System.EventHandler(this.rdoLineStyleVH_CheckedChanged);
            // 
            // rdoLineStyleHV
            // 
            resources.ApplyResources(this.rdoLineStyleHV, "rdoLineStyleHV");
            this.rdoLineStyleHV.Image = global::JCHVRF.Properties.Resources.LineStyle_HV;
            this.rdoLineStyleHV.Name = "rdoLineStyleHV";
            this.rdoLineStyleHV.TabStop = true;
            this.toolTip1.SetToolTip(this.rdoLineStyleHV, resources.GetString("rdoLineStyleHV.ToolTip"));
            this.rdoLineStyleHV.UseVisualStyleBackColor = true;
            this.rdoLineStyleHV.CheckedChanged += new System.EventHandler(this.rdoLineStyleHV_CheckedChanged);
            // 
            // rdoLineStylePolyLine
            // 
            resources.ApplyResources(this.rdoLineStylePolyLine, "rdoLineStylePolyLine");
            this.rdoLineStylePolyLine.Image = global::JCHVRF.Properties.Resources.LineStyle_Polyline1;
            this.rdoLineStylePolyLine.Name = "rdoLineStylePolyLine";
            this.rdoLineStylePolyLine.TabStop = true;
            this.toolTip1.SetToolTip(this.rdoLineStylePolyLine, resources.GetString("rdoLineStylePolyLine.ToolTip"));
            this.rdoLineStylePolyLine.UseVisualStyleBackColor = true;
            this.rdoLineStylePolyLine.CheckedChanged += new System.EventHandler(this.rdoLineStylePolyLine_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pbAddBuildingImage);
            this.groupBox1.Controls.Add(this.pbDeleteBgImage);
            this.groupBox1.Controls.Add(this.chkBuildingImageLocked);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // pbAddBuildingImage
            // 
            this.pbAddBuildingImage.BackgroundImage = global::JCHVRF.Properties.Resources.Simple_unit_add_nr_48x48;
            resources.ApplyResources(this.pbAddBuildingImage, "pbAddBuildingImage");
            this.pbAddBuildingImage.ClickImage = null;
            this.pbAddBuildingImage.DefaultImage = global::JCHVRF.Properties.Resources.Simple_unit_add_nr_48x48;
            this.pbAddBuildingImage.HoverImage = global::JCHVRF.Properties.Resources.Simple_unit_add_hi_48x48;
            this.pbAddBuildingImage.Name = "pbAddBuildingImage";
            this.pbAddBuildingImage.TabStop = false;
            this.toolTip1.SetToolTip(this.pbAddBuildingImage, resources.GetString("pbAddBuildingImage.ToolTip"));
            this.pbAddBuildingImage.Tooltips = "Add";
            this.pbAddBuildingImage.Click += new System.EventHandler(this.pbAddBuildingImage_Click);
            // 
            // pbDeleteBgImage
            // 
            this.pbDeleteBgImage.BackgroundImage = global::JCHVRF.Properties.Resources.Simple_unit_delete_nr_48x48;
            resources.ApplyResources(this.pbDeleteBgImage, "pbDeleteBgImage");
            this.pbDeleteBgImage.ClickImage = null;
            this.pbDeleteBgImage.DefaultImage = global::JCHVRF.Properties.Resources.Simple_unit_delete_nr_48x48;
            this.pbDeleteBgImage.HoverImage = global::JCHVRF.Properties.Resources.Simple_unit_delete_hi_48x48;
            this.pbDeleteBgImage.Name = "pbDeleteBgImage";
            this.pbDeleteBgImage.TabStop = false;
            this.toolTip1.SetToolTip(this.pbDeleteBgImage, resources.GetString("pbDeleteBgImage.ToolTip"));
            this.pbDeleteBgImage.Tooltips = "Delete";
            this.pbDeleteBgImage.Click += new System.EventHandler(this.pbDeleteBgImage_Click);
            // 
            // chkBuildingImageLocked
            // 
            this.chkBuildingImageLocked.BackColor = System.Drawing.Color.Transparent;
            this.chkBuildingImageLocked.CheckBoxLocation = new System.Drawing.Point(22, 3);
            resources.ApplyResources(this.chkBuildingImageLocked, "chkBuildingImageLocked");
            this.chkBuildingImageLocked.LabelAlignment = System.Drawing.ContentAlignment.TopCenter;
            this.chkBuildingImageLocked.LabelLocation = new System.Drawing.Point(0, 32);
            this.chkBuildingImageLocked.LabelSize = new System.Drawing.Size(61, 24);
            this.chkBuildingImageLocked.Name = "chkBuildingImageLocked";
            this.chkBuildingImageLocked.CheckedChanged += new System.EventHandler(this.chkBuildingImageLocked_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.jcbtnClear);
            this.panel2.Controls.Add(this.jcbtnCancel);
            this.panel2.Controls.Add(this.jcbtnOK);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // jcbtnClear
            // 
            resources.ApplyResources(this.jcbtnClear, "jcbtnClear");
            this.jcbtnClear.FlatAppearance.BorderSize = 0;
            this.jcbtnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(59)))), ((int)(((byte)(75)))));
            this.jcbtnClear.Name = "jcbtnClear";
            this.jcbtnClear.UseVisualStyleBackColor = true;
            this.jcbtnClear.Click += new System.EventHandler(this.jcbtnClear_Click);
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
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAddInd,
            this.menuAddYP,
            this.menuAddCP,
            this.menuAddCHBox,
            this.menuAddMultiCHBox,
            this.menuDeleteNode});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // menuAddInd
            // 
            this.menuAddInd.Name = "menuAddInd";
            resources.ApplyResources(this.menuAddInd, "menuAddInd");
            // 
            // menuAddYP
            // 
            this.menuAddYP.Name = "menuAddYP";
            resources.ApplyResources(this.menuAddYP, "menuAddYP");
            // 
            // menuAddCP
            // 
            this.menuAddCP.Name = "menuAddCP";
            resources.ApplyResources(this.menuAddCP, "menuAddCP");
            // 
            // menuAddCHBox
            // 
            this.menuAddCHBox.Name = "menuAddCHBox";
            resources.ApplyResources(this.menuAddCHBox, "menuAddCHBox");
            // 
            // menuAddMultiCHBox
            // 
            this.menuAddMultiCHBox.Name = "menuAddMultiCHBox";
            resources.ApplyResources(this.menuAddMultiCHBox, "menuAddMultiCHBox");
            // 
            // menuDeleteNode
            // 
            this.menuDeleteNode.Name = "menuDeleteNode";
            resources.ApplyResources(this.menuDeleteNode, "menuDeleteNode");
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // pbUndo
            // 
            this.pbUndo.BackColor = System.Drawing.Color.Transparent;
            this.pbUndo.InitialImage = global::JCHVRF.Properties.Resources.undo;
            resources.ApplyResources(this.pbUndo, "pbUndo");
            this.pbUndo.Name = "pbUndo";
            this.pbUndo.TabStop = false;
            this.toolTip1.SetToolTip(this.pbUndo, resources.GetString("pbUndo.ToolTip"));
            // 
            // pbRedo
            // 
            this.pbRedo.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.pbRedo, "pbRedo");
            this.pbRedo.InitialImage = global::JCHVRF.Properties.Resources.redo;
            this.pbRedo.Name = "pbRedo";
            this.pbRedo.TabStop = false;
            this.toolTip1.SetToolTip(this.pbRedo, resources.GetString("pbRedo.ToolTip"));
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.panel2, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnlTop, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tvOutdoor, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.addFlowPiping, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 2, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // addFlowPiping
            // 
            this.addFlowPiping.BackColor = System.Drawing.Color.White;
            this.addFlowPiping.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.addFlowPiping.CanChangeDst = false;
            this.addFlowPiping.CanChangeOrg = false;
            this.addFlowPiping.CanMoveNode = false;
            this.addFlowPiping.CanReflexLink = false;
            this.addFlowPiping.CanSizeNode = false;
            this.addFlowPiping.CanStretchLink = false;
            this.tableLayoutPanel1.SetColumnSpan(this.addFlowPiping, 3);
            resources.ApplyResources(this.addFlowPiping, "addFlowPiping");
            this.addFlowPiping.Name = "addFlowPiping";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pbRedo);
            this.panel1.Controls.Add(this.pbUndo);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // frmManualPiping
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "frmManualPiping";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmManualPiping_FormClosed);
            this.pnlTop.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlottingScaleRotation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlottingScaleColor)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbNodeBgColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBranchKitColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbTextColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLineColor)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbAddBuildingImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbDeleteBgImage)).EndInit();
            this.panel2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbUndo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRedo)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TreeView tvOutdoor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuAddYP;
        private System.Windows.Forms.ToolStripMenuItem menuAddCHBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuDeleteNode;
        private System.Windows.Forms.ToolStripMenuItem menuAddCP;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.ToolStripMenuItem menuAddInd;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private JCBase.UI.VRFIcon pbAddBuildingImage;
        private JCBase.UI.VRFIcon pbDeleteBgImage;
        private System.Windows.Forms.RadioButton rdoLineStylePolyLine;
        private System.Windows.Forms.RadioButton rdoLineStyleVHV;
        private System.Windows.Forms.RadioButton rdoLineStyleHVH;
        private System.Windows.Forms.RadioButton rdoLineStyleVH;
        private System.Windows.Forms.RadioButton rdoLineStyleHV;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.PictureBox pbTextColor;
        private System.Windows.Forms.PictureBox pbLineColor;
        private JCBase.UI.JCLabel lblNodeBgColor;
        private System.Windows.Forms.PictureBox pbNodeBgColor;
        private JCBase.UI.JCLabel lblBranchKitColor;
        private System.Windows.Forms.PictureBox pbBranchKitColor;
        private JCBase.UI.JCLabel lblTextColor;
        private JCBase.UI.JCLabel lblLineColor;
        private System.Windows.Forms.Panel panel2;
        private JCBase.UI.VRFButton jcbtnClear;
        private JCBase.UI.VRFButton jcbtnCancel;
        private JCBase.UI.VRFButton jcbtnOK;
        private JCBase.UI.JCComboBox jccmbScale;
        private System.Windows.Forms.GroupBox groupBox4;
        private JCBase.UI.JCLabel jclblPlottingScaleUnit;
        private JCBase.UI.JCTextBox jctxtPlottingScale;
        private System.Windows.Forms.PictureBox pbPlottingScaleColor;
        private System.Windows.Forms.PictureBox pbPlottingScaleRotation;
        private System.Windows.Forms.ToolTip toolTip1;
        private JCBase.UI.VRFButton jcbtnEnablePlottingScale;
        private JCBase.UI.uc_CheckBox chkBuildingImageLocked;
        private JCBase.UI.JCLabel jclblPlottingScaleNotAvailable;
        private System.Windows.Forms.GroupBox groupBox5;
        private JCBase.UI.JCLabel jcLabel1;
        private System.Windows.Forms.ToolStripMenuItem menuAddMultiCHBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Lassalle.Flow.AddFlow addFlowPiping;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pbRedo;
        private System.Windows.Forms.PictureBox pbUndo;
    }
}