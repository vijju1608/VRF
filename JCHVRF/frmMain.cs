using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.IO;
using System.Drawing.Imaging;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;

using System.Drawing.Drawing2D;
using Lassalle.Flow;
using CADImport;
//using CADImport.RasterImage;

using JCBase.UI;
using JCBase.Util;

using Registr;

using JCHVRF.Model;
using JCHVRF.BLL;
using JCHVRF.MyPipingBLL;
using JCBase.Utility;
using System.Globalization;
using System.Collections;
using JCHVRF.VRFMessage;
using JCHVRF.Report;
using JCHVRF.Const;
using JCHVRF.VRFTrans;

namespace JCHVRF
{
    public partial class frmMain : JCForm
    {
        public string[] Args { get; set; }

        #region Initialization 初始化
        Project _thisProject;
        public Project thisProject
        {
            get { return _thisProject; }
            set { _thisProject = value; Project.CurrentProject = value; }
        }
        //_projectCopy拷贝当前_thisProject
        Project _projectCopy;

        //public Project thisProject;
        string _productType;
        string _mainRegion;
        List<CentralController> contrTypes;
        List<CentralController> controllerTypeList;
        bool isIndoorOK;
        /*---当前的单位表达式---*/
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;
        string ut_weight;
        string ut_dimension;
        private UndoRedo.UndoRedoHandler UndoRedoUtil = null;

        /// <summary>
        /// 选项卡切换是否是撤销操作导致
        /// </summary>
        private bool isTabSwitching = false;

        /*---当前的单位表达式---*/

        public frmMain()
        {
            InitializeComponent();            
        }

        public frmMain(string[] args) : base()
        {
            //接收启动参数
            Args = args; 
        }

        private void frmNewVRF_Load(object sender, EventArgs e)
        {
            ////如果是非中文系统，将数字格式强制设为英美模式。 20160701 by Yunxiao Lin
            //if (CultureInfo.CurrentCulture.Name == "zh-CN")
            //{ }
            //else
            //{
            //    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            //    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            //}

            this.StartPosition = FormStartPosition.CenterScreen;
            ShowWarningMsg("");
            this.JCCallValidationManager = true;   //界面验证器

            this.tabControl1.SelectedTab = tpgProject;
            this.tlblVersion.Text = "Ver. " + MyConfig.Version;


            PipingBLL.PipingImageDir = MyConfig.PipingNodeImageDirectory;
            PipingBLL.WiringImageDir = MyConfig.WiringNodeImageDirectory;
                        
            Init();
            initProjectInfo();


            //目前没有实现带参数启动程序 deleted by Shen Junjie on 2018/2/7 
            ////2013-8-28 新增AECworks主界面BOMofMaterial打开时，加载项目数据
            //if (Args == null)
            //{
            //    DoCreateNewProject();
            //}
            //else
            //{
            //    // 打开项目
            //    OpenaAECworksProject();
            //    // 将获取的 Project 对象绑定到当前项目
            //    BindToControl();
            //} 
            DoCreateNewProject(); //目前没有实现带参数启动程序 add by Shen Junjie on 2018/2/7 

            //非EU区域暂时隐藏ControllerWiring on 20171229 by Shen Junjie
            //ANZ区域可以使用ControllerWiring modify by Shen Junjie on 20180103
            if (thisProject.RegionCode!="EU_W" && thisProject.RegionCode != "EU_S" && thisProject.RegionCode != "EU_E" && thisProject.SubRegionCode != "ANZ")
            {
                tabControlController.TabPages.Remove(tpgControllerWiring);
            }

            SetTabControlImageKey();

            // 隐藏wiring与Control页面
            //this.tpgWiring.Parent = null;
            //this.tpgController.Parent = null;

            //设置Altitude控件是否隐藏 add on 20160601 by Yunxiao Lin
            //是否启用海拔修正是项目属性，并且不需要隐藏海拔输入 20160819 by Yunxiao Lin
            //setAltitudeVisible(SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor);

            //如果当前Region不是T3,则室外机最高制冷工况温度为43摄氏度 20161031 by Yunxiao Lin
            if (thisProject.SubRegionCode != "ME_T3A" && thisProject.SubRegionCode != "ME_T3B")
            {
                if (SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB > 43)
                    SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB = 43;
            }

           
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E" || thisProject.SubRegionCode == "ANZ")
            {
                uc_CheckBox_Actual.Visible = true;
                uc_CheckBox_Nominal.Visible = true;
            }
            else
            {
                uc_CheckBox_Actual.Visible = false;
                uc_CheckBox_Nominal.Visible = false;
            }

            if (thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S")
            {
                //初始化帮助信息                
                jclblHelp_Indoor_1.Visible = false;
                jclblHelp_Indoor_2.Visible = false;

                jccmbIndoorType.Visible = true;
                uc_CheckBox_IsRoomBase.Visible = false;
                BindCmbIndoorType();
            }
            else
            {
                //初始化帮助信息  
                jclblHelp_Indoor_1_EU.Visible = false;
                jclblHelp_Indoor_2_EU.Visible = false;
            }

            /*注册撤销功能 add by axj 20161228 begin */
            UndoRedoUtil = UndoRedo.UndoRedoHandler.MainInstance;
            UndoRedoUtil.ShowIconsOnTabPage(tabControl1, new Rectangle(912, 95, 16, 18), new Rectangle(884, 95, 16, 18));
            
            UndoRedoUtil.GetCurrentProjectEventHandler += delegate (out Project prj)  //获取最新项目数据
            {
                DoSavePipingStructure();
                prj = thisProject.DeepClone();//返回当前项目数据的副本
            };
            UndoRedoUtil.ReloadProjectEventHandler += delegate(Project prj)   //重新加载历史记录里面的项目数据
            {
                thisProject = prj;

                _isSameOutdoorNode = false;
                BindToControl();
            };
            UndoRedoUtil.RestoreTabPageEventHandler += delegate (TabPage tab) //切换历史记录的TabPage
            {
                isTabSwitching = true;
                tabControl1.SelectedTab = tab;
                isTabSwitching = false;
            };
            UndoRedoUtil.SaveProjectHistory();
            UndoRedoUtil.SaveTabHistory(this.tabControl1.SelectedTab);
            /*注册撤销功能 end*/

            //add by axj 20170623 删除临时emf文件
            string dir = Application.StartupPath;
            DirectoryInfo fileTempFolder = new DirectoryInfo(dir);
            foreach (FileInfo file in fileTempFolder.GetFiles())
            {
                if (file.Extension == ".emf" && file.Name.StartsWith("temp"))
                {
                    file.Delete();
                }
            }
            //更新LoadCity Index add 2018-02-07 axj
            UpdateLoadIndexData();

            BindToSource_ProjectBaseInfo();
            //拷贝_thisProject对象
            _projectCopy = (Project)_thisProject.DeepClone();
            //SystemSetting.UserSetting.defaultSetting.salesEngieer = this.jctxtSalesEngineer1.Text;
            //SystemSetting.Serialize();                      

        }

        /// <summary>
        /// 解决窗体闪屏问题
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        /// <summary>
        /// 更新LoadIndex数据
        /// </summary>
        private void UpdateLoadIndexData()
        {
            System.Action action = new System.Action(() =>
            {
                try
                {
                    if (File.Exists(System.Windows.Forms.Application.StartupPath + @"\NVRF\VRFPS_Update.dat"))
                    {
                        RoomLoadIndexBLL loadIndexBll = new RoomLoadIndexBLL();
                        loadIndexBll.UpdateLoadIndexData();
                    }
                }
                catch (Exception ex)
                {

                }
            });
            action.BeginInvoke(null, null);
        }

        #endregion

        #region Control events 界面控件事件

        #region ToolScriptButtons click event 工具栏按钮点击事件

        private void tbtnNew_Click(object sender, EventArgs e)
        {
            BindToSource_ProjectBaseInfo();
            bool projectEqual = Util.CompareObject(_projectCopy, _thisProject);
            if (!projectEqual)
            {
                DialogResult res = JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_SAVEPROJ);
                if (res == DialogResult.Yes)
                {
                    if (!DoSaveProject())
                        return;
                }
                else if (res == DialogResult.Cancel)
                    return;
            }
            DoCreateNewProject();

            // 新建项目后回到Project Page页面
            tabControl1.SelectedTab = tpgProject;
        }

        private void tbtnOpen_Click(object sender, EventArgs e)
        {
            
            BindToSource_ProjectBaseInfo();
            //比较对象_project1, _thisProject相等性
            bool projectEqual = Util.CompareObject(_projectCopy, _thisProject);
            if (!projectEqual)
            {
                DialogResult res = JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_SAVEPROJ);
                if (res == DialogResult.Yes)
                {
                    if (!DoSaveProject())
                        return;
                }
                else if (res == DialogResult.Cancel)
                    return;
            }

          
            DoOpenProject();
        }

        private void tbtnSave_Click(object sender, EventArgs e)
        {
            DoSaveProject();
            //拷贝_thisProject对象
            _projectCopy = (Project)_thisProject.DeepClone();
        }

        private void tbtnSaveAs_Click(object sender, EventArgs e)
        {
            DoSaveAsProject();
            //拷贝_thisProject对象
            _projectCopy = (Project)_thisProject.DeepClone();
        }

        private void tbtnImport_Click(object sender, EventArgs e)
        {
            DoImportProject();
        }

        private void tbtnExport_Click(object sender, EventArgs e)
        {
            DoExportProject();
        }

        public void BindLanguage(string language)
        {
            //更改系统语言时（选项系统添加项目名称 on 20170912 by xyj）
            this.Text = Msg.GetResourceString("PROJECT_TITLE_NAME") + thisProject.Name;
            DoSwitchLanguage(language);
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }

        }

        //private void setAltitudeVisible(bool visible)
        //{
        //    this.lblProjAltitude.Visible = visible;
        //    this.jclblUnitAltitude_m.Visible = visible;
        //    this.jctxtAltitude.Visible = visible;
        //}
        private void tbtnSetting_Click(object sender, EventArgs e)
        {
            frmSetting f = new frmSetting(thisProject);
            this.AddOwnedForm(f);
            bool oldAltitudeStatus = SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor;
            if (f.ShowDialog() == DialogResult.OK)
            {
                BindUnit();
                BindSelectedIndoorList();
                NameArray_Indoor arr = new NameArray_Indoor();
                Global.SetDGVHeaderText(ref dgvIndoor, arr.RoomIndoor_HeaderText);
                Global.SetDGVHeaderText(ref dgvExchanger, arr.RoomIndoor_HeaderText);
                Global.SetDGVHeaderText(ref dgvIndoorNotAssigned, arr.RoomIndoorNotAttached_HeaderText);

                if (tvOutdoor.Nodes.Count > 0 && tvOutdoor.SelectedNode != null)
                {
                    BindTreeViewOutNodeInfo(tvOutdoor.SelectedNode);
                    //2013-10-28  by Yang Setting配置修改时，重新绘制Piping图 (管径长度单位变更时，m --- ft) 
                    if (curSystemItem != null && !isBinding)
                    {
                        curSystemItem.IsInputLengthManually = this.uc_CheckBox_PipingLength.Checked;
                        if (curSystemItem.IsManualPiping)
                        {
                            DoDrawingPiping(false);
                        }
                        else
                        {
                            DoDrawingPiping(true);
                        }
                    }

                }
                jclblDXFFilePath.Text = SystemSetting.UserSetting.fileSetting.DXFFiles;
                jclblRptFilePath.Text = SystemSetting.UserSetting.fileSetting.reportFiles;

                if (string.IsNullOrEmpty(jclblDXFFilePath.Text))
                {
                    jclblDXFFilePath.Text = Msg.GetResourceString("Msg_DXFPath");
                }
                if (string.IsNullOrEmpty(jclblRptFilePath.Text))
                {
                    jclblRptFilePath.Text = Msg.GetResourceString("Msg_ReportPath");
                }
                //当海拔修正选项变化时，海拔归零
                //不需要隐藏海拔输入，也不需要归零 20160819 by Yunxiao Lin
                //if (oldAltitudeStatus != SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor && this.jctxtAltitude.Text != "0")
                //{
                //    this.altitudetextIsAutoChange = true;
                //    this.jctxtAltitude.Text = "0";
                //    thisProject.Altitude = 0;

                //    setAltitudeVisible(SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor);

                //}

            }

        }

        private void tbtnAbout_Click(object sender, EventArgs e)
        {
            string path = MyConfig.AppPath + "\\NVRF\\Help.pdf";
            if (File.Exists(path))
            {
                string msg = "";
                Util.Process_Start(path, out msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    JCMsg.ShowErrorOK(msg);
                }
            }
        }

        private void tbtnExit_Click(object sender, EventArgs e)
        {
            DialogResult res = JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_SAVEPROJ);
            if (res == DialogResult.Yes)
            {
                if (!DoSaveProject())
                    return;
            }
            else if (res == DialogResult.Cancel)
                return;

            //frmRegionBrand f = this.Owner as frmRegionBrand;
            //f.Show();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbtnChangeLanguage_Click(object sender, EventArgs e)
        {
            
            DoSwitchLanguage((sender as ToolStripMenuItem).Name);
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }
            this.Text = Msg.GetResourceString("PROJECT_TITLE_NAME") + thisProject.Name;
           
        }

        private void frmNewVRF_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.N:
                        tbtnNew_Click(null, null);
                        break;
                    case Keys.O:
                        tbtnOpen_Click(null, null);
                        break;
                    case Keys.S:
                        tbtnSave_Click(null, null);
                        break;
                    case Keys.I:
                        tbtnImport_Click(null, null);
                        break;
                    case Keys.T:
                        tbtnSetting_Click(null, null);
                        break;
                    case Keys.Q:
                        Close();
                        break;
                    default:
                        break;
                }
            }
            else if (e.Alt)
            {
                if (e.KeyCode == Keys.S)
                {
                    tbtnSaveAs_Click(null, null);
                }
            }                       

        }

        private void frmNewVRF_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult res = DialogResult.OK;
            if (JCCallValidationManager && !JCValidateForm())
            {
                res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_FORMCLOSE_WARN, true);
            }
            else if (!JCFormIsModified)
            {                
                res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_FORMCLOSE);
                if (res == DialogResult.No)
                {                    
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                res = JCMsg.ShowConfirmYesNoCancel(JCMsg.CONFIRM_FORMCLOSE_SAVE);
                if (res == DialogResult.Yes)
                {
                    DoSaveProject();
                }
            }

            if (res == DialogResult.Cancel)
            {
                // 返回当前窗体，不关闭
                e.Cancel = true;
                return;
            }

            //frmRegionBrand f = this.Owner as frmRegionBrand;
            //f.Show();

            e.Cancel = false;


        }

        private void tbtn_MouseHover(object sender, EventArgs e)
        {
            ToolStripButton changeButton = sender as ToolStripButton;
            if (changeButton.Name == tbtnExit.Name)
            {
                changeButton.Image = Properties.Resources.Menu_exit_hi_48x48;
            }
            else if (changeButton.Name == tbtnAbout.Name)
            {
                changeButton.Image = Properties.Resources.Menu_info_hi_48x48;
            }

            if (CommonBLL.IsChinese())
            {
                if (changeButton.Name == tbtnNew.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_New_Pro_CH;
                }
                else if (changeButton.Name == tbtnOpen.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Open_CN;
                }
                else if (changeButton.Name == tbtnSave.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Save_CN;
                }
                else if (changeButton.Name == tbtnSaveAs.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Save_as_CN;
                }
                else if (changeButton.Name == tbtnImport.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Import_Pro_CH;
                }
                else if (changeButton.Name == tbtnExport.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Export_Pro_CH;
                }
                else if (changeButton.Name == tbtnSetting.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Setting_CN;
                }
            }
            else
            {
                if (changeButton.Name == tbtnNew.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_New_Pro;   //_01_1__prompt
                }
                else if (changeButton.Name == tbtnOpen.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Open_Pro;
                }
                else if (changeButton.Name == tbtnSave.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Save_Pro;
                }
                else if (changeButton.Name == tbtnSaveAs.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Save_as_Pro;
                }
                else if (changeButton.Name == tbtnImport.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Import_Pro;
                }
                else if (changeButton.Name == tbtnExport.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Export_Pro;
                }
                else if (changeButton.Name == tbtnSetting.Name)
                {
                    changeButton.Image = Properties.Resources.Menu_Setting_Pro;
                }
            }
        }

        private void tbtn_MouseLeave(object sender, EventArgs e)
        {
            ToolStripButton changeButton = sender as ToolStripButton;
            if (changeButton.Name == tbtnNew.Name)
            {
                changeButton.Image = Properties.Resources.Menu_New_Nor;  //_01_1__normal
            }
            else if (changeButton.Name == tbtnOpen.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Open_Nor;   //_01_2__normal
            }
            else if (changeButton.Name == tbtnSave.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Save_Nor;
            }
            else if (changeButton.Name == tbtnSaveAs.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Save_as_Nor;
            }
            else if (changeButton.Name == tbtnImport.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Import_Nor;
            }
            else if (changeButton.Name == tbtnExport.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Export_Nor;
            }
            else if (changeButton.Name == tbtnSetting.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Setting_Nor;
            }
            else if (changeButton.Name == tbtnExit.Name)
            {
                changeButton.Image = Properties.Resources.Menu_exit_nr_48x48;
            }
            else if (changeButton.Name == tbtnAbout.Name)
            {
                changeButton.Image = Properties.Resources.Menu_info_nr_48x48;
            }
        }

        private void tbtn_MouseDown(object sender, MouseEventArgs e)
        {
            ToolStripButton changeButton = sender as ToolStripButton;
            if (changeButton.Name == tbtnNew.Name)
            {
                changeButton.Image = Properties.Resources.Menu_New_High;
            }
            else if (changeButton.Name == tbtnOpen.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Open_High;
            }
            else if (changeButton.Name == tbtnSave.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Save_High;
            }
            else if (changeButton.Name == tbtnSaveAs.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Save_as_High;
            }
            else if (changeButton.Name == tbtnImport.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Import_High;
            }
            else if (changeButton.Name == tbtnExport.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Export_High;
            }
            else if (changeButton.Name == tbtnSetting.Name)
            {
                changeButton.Image = Properties.Resources.Menu_Setting_High;
            }
            else if (changeButton.Name == tbtnExit.Name)
            {
                changeButton.Image = Properties.Resources.Menu_exit_hi_48x48;
            }
            else if (changeButton.Name == tbtnAbout.Name)
            {
                changeButton.Image = Properties.Resources.Menu_info_hi_48x48;
            }
        }

        /// <summary>
        /// toolstrip重绘,C#中去掉Toolstrip边框的方法  
        /// 在C#中使用toolstrip时，当RenderMode设置为System时会出现下面有条灰线，无法通过基本的设置属性除去； 
        /// 解决办法：只需要重绘一下toolstrip即可 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip1_Paint(object sender, PaintEventArgs e)
        {

            if ((sender as ToolStrip).RenderMode == ToolStripRenderMode.System)
            {
                Rectangle rect = new Rectangle(0, 0, this.toolStrip1.Width, this.toolStrip1.Height - 2); //处理RenderMode为System类型时下方默认的2px缝隙
                e.Graphics.SetClip(rect);
            }
        }

        #endregion

        #region Main_Project

        //输入框内的海拔高度是否是自动修改的
        //海拔高度输入不用再归零，所以这个变量用不上了 20160819 by Yunxiao Lin
        //bool altitudetextIsAutoChange = false;

        private void jctxtAltitude_TextChanged(object sender, EventArgs e)
        {
            TextBox tx = sender as TextBox;
            if (NumberUtil.IsNumber(tx.Text))
            {
                //如果数值出现负数则清空当前文本框值
                if (tx.Text.Contains("-"))
                {
                    tx.Text = "";
                }
                try
                {
                    double ret; 
                    double.TryParse(tx.Text, out ret);
                }
                finally
                {
                    this.jctxtAltitude.RequireValidation = true;
                }
            }
            else
            {
                tx.Text = "0"; 
            }
            if (tx.Text == "0")
            {
                this.jctxtAltitude.RequireValidation = false;
                JCValidateSingle(jctxtAltitude);
            }
            //if (JCValidateSingle(jctxtAltitude))
            //{
            //    if (!altitudetextIsAutoChange && SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor && tpgPiping.ImageKey == "Check")
            //    {
            //        //海拔高度的变化会影响到选型的合理性，需要重新选型
            //        if (JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGEALTITUDE()) == DialogResult.Cancel)
            //        {
            //            altitudetextIsAutoChange = true;
            //            jctxtAltitude.Text = Unit.ConvertToControl(thisProject.Altitude, UnitType.LENGTH_M, ut_length).ToString("n0");
            //            return;
            //        }
            //        else
            //        {
            //            thisProject = new Project();
            //            thisProject.Name = this.jctxtProjName.Text;
            //            thisProject.Altitude = int.Parse(this.jctxtAltitude.Text);
            //            thisProject.ContractNO = this.jctxtContractNo.Text;
            //            thisProject.Location = this.jctxtLocation.Text;
            //            thisProject.PurchaseOrderNO = this.jctxtPurchaseOrderNo.Text;
            //            thisProject.Remarks = this.jctxtRemarks.Text;
            //            thisProject.SalesEngineer = this.jctxtSalesName.Text;
            //            thisProject.SalesOffice = this.jctxtSalesOffice.Text;
            //            thisProject.SalesYINO = this.jctxtSalesYINo.Text;
            //            thisProject.ShipTo = this.jctxtShipTo.Text;
            //            thisProject.SoldTo = this.jctxtSoldTo.Text;
            //            thisProject.DeliveryRequiredDate = this.timeDeliveryDate.Value;
            //            thisProject.OrderDate = this.timeOrderDate.Value;

            //            string productType = this.jccmbProductType.SelectedValue.ToString();

            //            BindToControl();
            //            thisProject.ProductType = productType;
            //        }
            //    }
            //    else
            //        altitudetextIsAutoChange = false;
            //    thisProject.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(jctxtAltitude.Text.Trim()), UnitType.LENGTH_M, ut_length));
            //}

            //当项目启用海拔高度修正时，修改海拔高度会引起室外机工况变化，重新选型 20160819 by Yunxiao Lin
            //if (JCValidateSingle(jctxtAltitude))
            //{
            //    if (thisProject.EnableAltitudeCorrectionFactor)
            //    {
            //        //弹出警告提示，让用户确认是否继续
            //        if (JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGEALTITUDE()) == DialogResult.Cancel)
            //        {
            //            jctxtAltitude.Text = Unit.ConvertToControl(thisProject.Altitude, UnitType.LENGTH_M, ut_length).ToString("n0");
            //            return;
            //        }
            //        else
            //        {
            //            thisProject.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(jctxtAltitude.Text.Trim()), UnitType.LENGTH_M, ut_length));
            //            foreach (TreeNode tnOut in tvOutdoor.Nodes)
            //            {
            //                // TODO: 室外机重新选型，注意可能会有选型失败的情况出现 20160819 by Yunxiao Lin
            //                SystemVRF sysItem = tnOut.Tag as SystemVRF;

            //                List<string> ERRList = new List<string>();
            //                List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(sysItem.Id);
            //                Global.DoSelectOutdoor(sysItem, listRISelected, thisProject, out ERRList);
            //                Global.BindTreeNodeOut(tnOut, sysItem, listRISelected, thisProject.BrandCode);
            //                //重新选型之后需要判断并设置TabPage状态 20160819 by Yunxiao Lin
            //                SetTabControlImageKey();
            //                //if (sysItem == curSystemItem)
            //                //{
            //                //    BindPipingDrawing(tnOut);
            //                //    DoDrawingWiring(tnOut);
            //                //}
            //                //foreach (TreeNode tn in tnOut.Nodes)
            //                //{
            //                //    if (tn.Name == ri.IndoorNO.ToString())
            //                //        tvOutdoor.SelectedNode = tn;
            //                //}
            //            }
            //        }
            //    }
            //    else
            //        thisProject.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(jctxtAltitude.Text.Trim()), UnitType.LENGTH_M, ut_length));
            //}
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tab = sender as TabControl;
            ShowWarningMsg("");  //切换TAB时候清除消息栏
            switch (tab.SelectedTab.Name)
            {
                case "tpgProject":
                    re_LayoutProjectPage();
                    break;
                case "tpgExchanger":
                    re_LayoutExchangerPage();
                    break;
                case "tpgIndoor":
                    re_LayoutIndoorPage();
                    break;
                case "tpgOutdoor":
                    re_LayoutOutdoorPage();
                    break;
                case "tpgPiping":
                    re_LayoutPipingPage();
                    break;
                case "tpgWiring":
                    re_LayoutWiringPage();
                    break;
                case "tpgController":
                    re_LayoutControllerPage();
                    break;
                case "tpgReport":
                    re_LayoutReportPage();
                    break;
            }
            if (isTabSwitching == false)
            {
                UndoRedoUtil.SaveTabHistory(tab.SelectedTab);//保存历史痕迹 add by axj 20161228
            }
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage.ImageKey != "Check")
                e.Cancel = true;
        }

        private void tabControlController_SelectedIndexChanged(object sender, EventArgs e)
        { 
            if (tabControlController.SelectedIndex == 1)
            {
                re_LayoutControllerWiringPage();
            }
        }

        private void re_LayoutProjectPage()
        {
            this.pnlHelpInfo_Project.Controls.Add(pnlHelpInfo_Icon);
        }

        private void re_LayoutIndoorPage()
        {
            this.pnlHelpInfo_Indoor.Controls.Add(pnlHelpInfo_Icon);
        }

        private void re_LayoutExchangerPage()
        {
            this.pnlHelpInfo_Exchanger.Controls.Add(pnlHelpInfo_Icon);
        }

        private void re_LayoutOutdoorPage()
        {
            this.pnlHelpInfo_Outdoor.Controls.Add(pnlHelpInfo_Icon);
            this.pnlTreeViewOutdoor.Controls.Add(tvOutdoor);
            tvOutdoor.Dock = DockStyle.Fill;
            dgvSystemInfo.Rows.Clear();
            BindToControl_SelectedUnits(); //重新绑定
        }

        private void re_LayoutPipingPage()
        {
            this.jcbtnManualPiping.Visible = false;

            //刚打开的项目curSystemItem有可能为空，需要指定一个 20161229 by Yunxiao Lin
            if (thisProject.SystemList == null)
                return;
            if (curSystemItem == null && thisProject.SystemList != null)
            {
                if (thisProject.SystemList.Count == 0)
                    return;
                curSystemItem = thisProject.SystemList[0];
            }

            //Delete "Binary Tree" and "2 Branch Binary Tree" from pipe interface. only for LA    add by Shen Junjie on 2018/4/4
            //巴西的Piping LayoutType选项还是要保留"Binary Tree" 和 "2 Branch Binary Tree"。  modified by Shen Junjie on 2018/5/30
            if (thisProject.RegionCode == "LA" && thisProject.SubRegionCode != "LA_BR")
            {
                jccmbPipingLayoutType.Visible = false;
                jclblLayoutType.Visible = false;
            }

            pnlContent_Piping_Tree.Controls.Add(tvOutdoor);
            pnlCMBOutdoor.Controls.Add(jccmbOutdoor);

            if (this.tvOutdoor.Nodes.Count > 0)
            {
                TreeNode tnOut = Global.GetTopParentNode(this.tvOutdoor.SelectedNode == null ? this.tvOutdoor.Nodes[0] : this.tvOutdoor.SelectedNode);//edit by axj 20161228 修正SelectedNode为null的bug
                BindPipingDrawing(tnOut); // 恢复当前选中节点绑定的 Piping 图
            }
        }

        private void re_LayoutWiringPage()
        {
            //刚打开的项目curSystemItem有可能为空，需要指定一个 20161229 by Yunxiao Lin
            if (thisProject.SystemList == null)
                return;
            if (curSystemItem == null && thisProject.SystemList != null)
            {
                if (thisProject.SystemList.Count == 0)
                    return;
                curSystemItem = thisProject.SystemList[0];
            }
            pnlContent_Wiring_Tree.Controls.Add(tvOutdoor);
            pnlCMBOutdoor1.Controls.Add(jccmbOutdoor);

            TreeNode tnOut = Global.GetTopParentNode(this.tvOutdoor.SelectedNode == null ? this.tvOutdoor.Nodes[0] : this.tvOutdoor.SelectedNode);//edit by axj 20161228 修正SelectedNode为null的bug
            //string imgDir = MyConfig.WiringNodeImageDirectory;

            DoDrawingWiring(tnOut);
        }

        private void re_LayoutControllerPage()
        {
            pnlController_Right_2.Visible = false;
            if (thisProject.RegionCode=="EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                //为EU增加提醒：   add by Shen Junjie on 2018/6/29
                //  Central controls and gateways can be freely selected, user must check configuration with technical documentation
                pnlController_Right_2.Visible = true;
            }

            CorrectedControlGroup();

            dgvMaterialList.Rows.Clear();
            InitWiringNodes(addFlowControllerWiring); //初始化Controller wiring 图 on 20180308 by xyj
            //先绑定Controller的ProductType                    
            BindControllerProductType();
            controllerTypeList = new List<CentralController>();
            BindToControl_Controller();
            //验证
            varificationCheck();
            //判断当前Control group 对应的室外机（室内机数量大于160）
            if (!ValidateIndoor_ControlGroup() && _mainRegion!="EU_E" && _mainRegion != "EU_W" && _mainRegion != "EU_S")   //EU暂时忽略限制
            {
                thisProject.CentralControllerOK = false;
                ShowWarningMsg(Msg.WARNING_PAYATTENTION);
                SetTabControlImageKey();
            }
            tabControlController.SelectedIndex = 0;
            //if (tabControlController.SelectedIndex == 1)
            //{
            //    re_LayoutControllerWiringPage();
            //}
        }

        private void re_LayoutReportPage()
        {
            if (SystemSetting.UserSetting.unitsSetting.settingPOWER == "kW")
            {
                uc_CheckBox_IduCapacityW.Visible = true;
            }
            else
            {
                uc_CheckBox_IduCapacityW.Visible = false;
            }
            
            this.pnlHelpInfo_Report.Controls.Add(pnlHelpInfo_Icon);
            uc_CheckBox_RptExchanger.Visible = thisProject.IsExchangerChecked;
            //无heatExchanger时repor隐藏开关   add on 20180606 by Vince
            if (thisProject.ExchangerList == null || thisProject.ExchangerList.Count == 0)
            {
                thisProject.IsExchangerChecked = false;
                uc_CheckBox_RptExchanger.Visible = false;
            }
            else
            {
                thisProject.IsExchangerChecked = true;
                uc_CheckBox_RptExchanger.Visible = true;
            }
        }

        private void re_LayoutControllerWiringPage()
        { 
            BindControlGroupsCMB();
            if (jccmbControlGroups.Items.Count == 0)
            {
                InitWiringNodes(addFlowControllerWiring); //初始化Controller wiring 图 on 20180308 by xyj
                return;
            }
            jccmbControlGroups.SelectedIndex = 0;
        }

        private void uc_CheckBox_Cooling_CheckedChanged(object sender, EventArgs e)
        {
            uc_CheckBox cbx = sender as uc_CheckBox;
            if (!cbx.Checked)
            {
                if (cbx.TextString == uc_CheckBox_Cooling.TextString)
                {
                    this.uc_CheckBox_Heating.Checked = true;
                }
                else
                {
                    this.uc_CheckBox_Cooling.Checked = true;
                }
            }

            thisProject.IsCoolingModeEffective = this.uc_CheckBox_Cooling.Checked;
            //thisProject.IsHeatingModeEffective = this.uc_CheckBox_Heating.Checked && !thisProject.ProductType.Contains(", CO");
            // 一个Project下有可能存在多个不同的ProductType，所以在这里thisProject.ProductType不能作为判断的依据 20160825 by Yunxiao Lin
            thisProject.IsHeatingModeEffective = this.uc_CheckBox_Heating.Checked;

            BindToControl_ConditionState();
        }


        private void jccmbProductType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (thisProject == null)
                return;

            if (!string.IsNullOrEmpty(thisProject.ProductType) &&
                !string.IsNullOrEmpty(jccmbProductType.Text) && thisProject.ProductType != jccmbProductType.Text)
            {
                if (JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGEREGION) == DialogResult.Cancel)
                {
                    jccmbProductType.SelectedValue = thisProject.ProductType;
                }
                else
                {
                    
                    //切换ProductType后，界面项目基本信息不清空
                    //thisProject = new Project();
                    //thisProject.Name = this.jctxtProjName.Text;
                    //thisProject.Altitude = int.Parse(this.jctxtAltitude.Text);
                    //thisProject.ContractNO = this.jctxtContractNo.Text;
                    //thisProject.ProjectRevision = this.jctxtProjectRevision.Text;
                    //thisProject.Location = this.jctxtLocation.Text;
                    //thisProject.PurchaseOrderNO = this.jctxtPurchaseOrderNo.Text;
                    //thisProject.Remarks = this.jctxtRemarks.Text;
                    //thisProject.SalesEngineer = this.jctxtSalesName.Text;
                    //thisProject.SalesOffice = this.jctxtSalesOffice.Text;
                    //thisProject.SalesYINO = this.jctxtSalesYINo.Text;
                    //thisProject.ShipTo = this.jctxtShipTo.Text;
                    //thisProject.SoldTo = this.jctxtSoldTo.Text;
                    //thisProject.DeliveryRequiredDate = this.timeDeliveryDate.Value;
                    //thisProject.OrderDate = this.timeOrderDate.Value;

                    bindProjectInfo(_mainRegion,true);
                    string productType = this.jccmbProductType.SelectedValue.ToString();                 

                    // 新风单冷系列强制选择制冷，不选制热 modified on 20160614 by Yunxiao Lin
                    //if (productType.Contains(", CO"))
                    //{
                    //    thisProject.IsCoolingModeEffective = true;
                    //    thisProject.IsHeatingModeEffective = false;
                    //    this.uc_CheckBox_Cooling.Enabled = false;
                    //    this.uc_CheckBox_Heating.Enabled = false;
                    //}
                    //else
                    //{
                    //    this.uc_CheckBox_Cooling.Enabled = true;
                    //    this.uc_CheckBox_Heating.Enabled = true;
                    //}
                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭Project.ProductType无法确定一个System是否需要制热。
                    // 一个系统是否是单冷系统应该由系统的ProductType决定，因此不需要上面的判断代码。20160826 by Yunxiao Lin

                    BindToControl();
                    thisProject.ProductType = productType;
                }
            }
        }
        #region bug 623 amended by HCL 20180927
        private void timeDeliveryDate1_CloseUp(object sender, EventArgs e)
        {
            if (timeDeliveryDate1.Value < DateTime.Today)
            {
                timeDeliveryDate1.Value = DateTime.Today;
            }
        }

        private void timeDeliveryDate_CloseUp(object sender, EventArgs e)
        {
            if (timeDeliveryDate.Value < DateTime.Today)
            {
                timeDeliveryDate.Value = DateTime.Today;
            }
        }
        #endregion


        #endregion

        #region Main_Indoor
        private void jcbtnManageRoom_Click(object sender, EventArgs e)
        {
            DoManageRoom();

            // 重新绑定已选的室内机,不能省略
            BindSelectedIndoorList();
            //重新绑定已选的全热交换机 on 2017-07-13 by xyj
            BindSelectedExchangerList();
        }

        private void pbHelpIndoor_Click(object sender, EventArgs e)
        {
            List<string> info = new List<string>();
            string title = Msg.GetResourceString("HelpIndoorTitle");
            info.Add(Msg.GetResourceString("HelpIndoorDB_C"));
            info.Add(Msg.GetResourceString("HelpIndoor_Capacity_C"));
            info.Add(Msg.GetResourceString("HelpIndoor_SensibleHeat"));
            info.Add(Msg.GetResourceString("HelpIndoor_Airflow"));
            info.Add(Msg.GetResourceString("HelpIndoor_DB_H"));
            info.Add(Msg.GetResourceString("HelpIndoor_Capacity_H"));

            if (!CommonBLL.IsChinese())
            {
                info.Add("Capacity_C -- Available cooling capacity");
                info.Add("Sensible Heat --  Available sensible cooling capacity");
                info.Add("Airflow -- Air flow volume of indoor unit or fresh air volume flow of fresh air machine.");
                info.Add("DB_H -- Dry Bulb Temperature (conditions in Heating)");
                info.Add("Capacity_H -- Available heating capacity");
            }
            else
            {

                info.Add("显热 --  Available sensible cooling capacity");
                info.Add("风量 -- Air flow volume of indoor unit or fresh air volume flow of fresh air machine.");
                info.Add("制热温度 -- Dry Bulb Temperature (conditions in Heating)");
                info.Add("制热容量 -- Available heating capacity");
            }

            frmHelpInfo f = new frmHelpInfo(info, title, MousePosition);
            f.Show();

            //Daikin
            //Name - Logical name of the device, possibly preceeded by room name
            //FCU - Device model name
            //Tmp C - Indoor conditions in cooling (dry bulb temp. / RH)
            //Rq TC - Required total cooling capacity
            //TC - Available total cooling capacity
            //Rq SC - Required sensible cooling capacity
            //SC - Available sensible cooling capacity
            //Tmp H - Indoor temperature in heating
            //Rq HC - Required heating capacity
            //HC - Available heating capacity
        }

        private void pbAddIndoor_Click(object sender, EventArgs e)
        {
            DoAddIndoor();
        }

        private void pbEditIndoor_Click(object sender, EventArgs e)
        {
            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }

            DataGridViewRow r = this.dgvIndoor.SelectedRows[0];
            int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
            bool isExchanger = Convert.ToBoolean(r.Cells[Name_Common.IsExchanger].Value);
            RoomIndoor ri = new RoomIndoor();
            if (!isExchanger)
                ri = (new ProjectBLL(thisProject)).GetIndoor(indNo);
            else
                ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);

            Boolean rflag = false;
            if (!ri.IsFreshAirArea)
                rflag = Global.ImageCompareString((System.Drawing.Bitmap)(r.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
            DoEditIndoor(ri, rflag);
        }

        private void dgvIndoor_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }
            DataGridViewRow r = this.dgvIndoor.SelectedRows[0];
            int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
            bool isExchanger = Convert.ToBoolean(r.Cells[Name_Common.IsExchanger].Value);
            RoomIndoor ri = new RoomIndoor();
            if (!isExchanger)
                ri = (new ProjectBLL(thisProject)).GetIndoor(indNo);
            else
                ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);

            Boolean rflag = false;
            if (!ri.IsFreshAirArea)
                rflag = Global.ImageCompareString((System.Drawing.Bitmap)(r.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
            //Boolean rflag = Global.ImageCompareString((System.Drawing.Bitmap)(r.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
            DoEditIndoor(ri, rflag);
        }

        private void pbCopyIndoor_Click(object sender, EventArgs e)
        {
            DoCopyIndoor();
        }

        private void pbDeleteIndoor_Click(object sender, EventArgs e)
        {
            DoDeleteIndoor();
        }

        private void pbAddIndoorOption_Click(object sender, EventArgs e)
        {
            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }

          if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
           // if (thisProject.RegionCode.Contains("EU_")) //目前只有EU使用新Accessory选型界面 20180727 by Yunxiao Lin
            {
                if ((thisProject.RoomIndoorList != null && thisProject.RoomIndoorList.Count > 0) || (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0))
                {
                    frmNewAccessory f = new frmNewAccessory(thisProject); 
                    f.StartPosition = FormStartPosition.CenterScreen;
                    this.AddOwnedForm(f); 
                    f.ShowDialog();
                }
            }
            else
            {
                DoAddOptions();
            }
        }

        private void pbClearIndoorOption_Click(object sender, EventArgs e)
        {
            DoClearOptions();
        }

        // 点击已选室内机表列名时的排序问题（默认是按照字符顺序，改为按照数值大小排序）
        private void dgvIndoor_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            switch (e.Column.Name)
            {
                // 以下列点击列名时按照数值大小排序
                case Name_Common.Name:
                    string s1 = e.CellValue1.ToString();
                    string s2 = e.CellValue2.ToString();
                    int i1 = Util.ParseToInt(s1);
                    int i2 = Util.ParseToInt(s2);
                    int i = i1 - i2;
                    e.SortResult = i > 0 ? 1 : i < 0 ? -1 : 0;
                    break;
                case Name_Common.DB_C:
                case Name_Common.Capacity_C:
                case Name_Common.SensibleHeat:
                case Name_Common.AirFlow:
                case Name_Common.DB_H:
                case Name_Common.Capacity_H:
                    double d = Convert.ToDouble(e.CellValue1) - Convert.ToDouble(e.CellValue2);
                    e.SortResult = d > 0 ? 1 : d < 0 ? -1 : 0;
                    break;
                // 默认按照字符顺序排序
                default:
                    e.SortResult = String.Compare(Convert.ToString(e.CellValue1), Convert.ToString(e.CellValue2));
                    break;
            }

            // 若数值相同时，则按照 编号来排序
            if (e.SortResult == 0 && e.Column.Name != Name_Common.Name)
            {
                e.SortResult = Convert.ToInt32(dgvIndoor.Rows[e.RowIndex1].Cells[Name_Common.NO].Value.ToString())
                    - Convert.ToInt32(dgvIndoor.Rows[e.RowIndex2].Cells[Name_Common.NO].Value.ToString());
            }
            e.Handled = true;
        }

        // 更新选中行的图标
        private void dgvIndoor_SelectionChanged(object sender, EventArgs e)
        {

            foreach (DataGridViewRow row in dgvIndoor.Rows)
            {
                Boolean rflag = Global.ImageCompareString((System.Drawing.Bitmap)(row.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
                if (rflag)
                    continue;

                if (row.Selected)
                {
                    row.Cells[Name_Common.TypeImage].Value = Properties.Resources.chart_indoor_white_40x28;
                }
                else
                    row.Cells[Name_Common.TypeImage].Value = Properties.Resources.chart_indoor_grey_40x28;
            }
        }

        // 更新选中行的图标
        private void dgvExchanger_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvExchanger.Rows)
            {
                Boolean rflag = Global.ImageCompareString((System.Drawing.Bitmap)(row.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
                if (rflag)
                    continue;

                if (row.Selected)
                {
                    row.Cells[Name_Common.TypeImage].Value = Properties.Resources.chart_indoor_white_40x28;
                }
                else
                    row.Cells[Name_Common.TypeImage].Value = Properties.Resources.chart_indoor_grey_40x28;
            }
        }

        #endregion

        #region Main_Exchanger

        /// <summary>
        /// 新增全热交换机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbAddExchanger_Click(object sender, EventArgs e)
        {
            if (uc_CheckBox_IsRoomBase1.Checked)
            {
                frmExchangerByRoom f = new frmExchangerByRoom(thisProject);
                f.StartPosition = FormStartPosition.CenterScreen;
                this.AddOwnedForm(f);
                f.ShowDialog();
            }
            else
            {
                frmExchanger f = new frmExchanger(thisProject);
                f.StartPosition = FormStartPosition.CenterScreen;
                this.AddOwnedForm(f);
                f.ShowDialog();
            }
        }

        /// <summary>
        /// 双击修改全热交换机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExchanger_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            if (this.dgvExchanger.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }


            DataGridViewRow r = this.dgvExchanger.SelectedRows[0];
            int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
            RoomIndoor ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);

            //rflag = Global.ImageCompareString((System.Drawing.Bitmap)(r.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
            Boolean rflag = Global.ImageCompareString((System.Drawing.Bitmap)(r.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
            DoEditExchanger(ri, rflag);
        }


        /// <summary>
        /// 编辑已选的全热交换机
        /// </summary>
        /// <param name="ri"></param>
        private Boolean DoEditExchanger(RoomIndoor ri, Boolean rflag)
        {
            List<RoomIndoor> romIndSaveList = new List<RoomIndoor>();
            if (curSystemItem != null)
                curSystemItem.IsUpdated = true;

            if (this.thisProject.ExchangerList != null)
                romIndSaveList = UtilEx.DeepClone(this.thisProject.ExchangerList);

            bool isBaseOfRoom = !string.IsNullOrEmpty(ri.RoomID);
            DialogResult result = DialogResult.Cancel;
            if (isBaseOfRoom)
            {
                //判断当前Room 是否是一个空的Room ON20170926 by xyj
                if ((new ProjectBLL(thisProject)).isEmptyRoom(ri.RoomID))
                {
                    frmExchanger f = new frmExchanger(ri, thisProject);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    this.AddOwnedForm(f);
                    result = f.ShowDialog();
                }
                else
                {

                    frmExchangerByRoom f = new frmExchangerByRoom(ri, thisProject, rflag);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    result = f.ShowDialog();
                }
            }
            else
            {
                frmExchanger f = new frmExchanger(ri, thisProject);
                f.StartPosition = FormStartPosition.CenterScreen;
                this.AddOwnedForm(f);
                result = f.ShowDialog();
            }


            if (result == DialogResult.OK)
            {
                BindExchangerList();
                foreach (DataGridViewRow r in dgvExchanger.Rows)
                {
                    r.Selected = false;
                    if (r.Cells[Name_Common.NO].Value.ToString() == ri.IndoorNO.ToString())
                    {
                        r.Selected = true;
                        break;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 修改Exchanger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbEditExchanger_Click(object sender, EventArgs e)
        {
            if (this.dgvExchanger.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }
            DataGridViewRow r = this.dgvExchanger.SelectedRows[0];
            int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
            RoomIndoor ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);
            Boolean rflag = Global.ImageCompareString((System.Drawing.Bitmap)(r.Cells[Name_Common.TypeImage].Value), Properties.Resources._02_2__active);
            DoEditExchanger(ri, rflag);
        }

        /// <summary>
        /// 删除Exchanger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbDeleteExchanger_Click(object sender, EventArgs e)
        {
            DoDeleteExchanger();
        }


        /// <summary>
        /// 删除Exchanger 方法
        /// </summary>
        private void DoDeleteExchanger()
        {
            if (this.dgvExchanger.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }
            DialogResult res;
            res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL);
            // 可以直接删除
            if (res == DialogResult.OK)
            {
                ProjectBLL bll = new ProjectBLL(thisProject);
                foreach (DataGridViewRow r in this.dgvExchanger.SelectedRows)
                {
                    int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
                    RoomIndoor ri = bll.GetExchanger(indNo);


                    List<RoomIndoor> list = bll.GetSelectedExchangerByRoom(ri.RoomID);
                    if (list != null && list.Count > 1)
                    {
                        //判断房间是否是空房间；空房间可以删除 否则不可以删除提示 on 20170927 by xyj
                        if (!(new ProjectBLL(thisProject)).isEmptyRoom(ri.RoomID))
                        {
                            JCMsg.ShowWarningOK(Msg.Exc_INFO_DEL_MAIN);
                            return;
                        }
                        else
                        {
                            //空房间Indoor 可以删除
                            bll.RemoveExchanger(indNo);
                        }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ri.ControlGroupID[0]))
                        {
                            DialogResult res1;
                            res1 = JCMsg.ShowConfirmOKCancel(Msg.EXC_INFOCONTROL_DEL);
                            if (res1 == DialogResult.OK)
                            {
                                bll.RemoveExchanger(indNo);
                            }
                        }
                        else
                        {
                            bll.RemoveExchanger(indNo);
                        }
                    }

                }

            }

            BindExchangerList();
        }

        /// <summary>
        /// Copy 全热交换机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbCopyExchanger_Click(object sender, EventArgs e)
        {
            DoCopyExchanger();
        }


        // 拷贝已选的全热交换机系统
        /// <summary>
        /// 拷贝已选择的全热交换机系统
        /// </summary>
        private void DoCopyExchanger()
        {
            List<RoomIndoor> selectedExchangerList = new List<RoomIndoor>();

            if (this.dgvExchanger.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }

            //确认框
            if (JCMsg.ShowConfirmOKCancel(JCMsg.COMMON_COPY_CHECK) != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < this.dgvExchanger.SelectedRows.Count; i++)
            {
                DataGridViewRow row = dgvExchanger.SelectedRows[i];
                //string model = row.Cells[Name_Common.ModelFull].Value.ToString();
                string indNo = row.Cells[Name_Common.NO].Value.ToString();
                if (NumberUtil.IsNumber(indNo))
                {
                    RoomIndoor ri = (new ProjectBLL(thisProject)).GetExchanger(int.Parse(indNo));
                    if (ri != null)
                    {
                        selectedExchangerList.Add(ri);
                    }
                }
            }

            //判断是否还存在的该房间的全热交换机
            if (!checkSelectedAllExchangerInRoom(selectedExchangerList))
            {
                JCMsg.ShowInfoOK(Msg.Exc_EXIST_INDUNIT);
                return;
            }

            //调用拷贝室内机方法
            ProjectBLL projectBLL = new ProjectBLL(thisProject);
            projectBLL.CopyExchanger(selectedExchangerList,
                SystemSetting.UserSetting.defaultSetting.ExchangerName,
                SystemSetting.UserSetting.defaultSetting.FloorName,
                SystemSetting.UserSetting.defaultSetting.RoomName,
                SystemSetting.UserSetting.defaultSetting.FreshAirAreaName, "");

            //刷新室内机列表
            BindExchangerList();
            UndoRedoUtil.SaveProjectHistory();
        }

        #endregion

        #region Main_Outdoor
        bool _isSameOutdoorNode = false;
        SystemVRF currentPipingSystem = null;  //当前配管图的系统 add by Shen Junjie 
        private void tvOutdoor_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            node.BackColor = UtilColor.bg_selected;
            if (node.ForeColor != Color.Chocolate)
                node.ForeColor = UtilColor.font_selected;
            if (node.Parent == null)
                pbLoadUnitInfo.Visible = false;
            else
                pbLoadUnitInfo.Visible = true;

            BindTreeViewOutNodeInfo(node);
            TreeNode tnOut = Global.GetTopParentNode(node);
            if (tnOut.Name != null && tnOut.Name != this.jccmbOutdoor.SelectedValue.ToString())
                SetCMBOutdoorText(tnOut.Name);

            // 仅当选择的节点跨室外机系统时，配管配线图才重新绘制，20130807 clh
            if (!_isSameOutdoorNode)
            {
                this.jcbtnAddCP.Visible = false;

                curSystemItem = tnOut.Tag as SystemVRF;
                //当移除所有室内机时，必须加上非null判断，否则会出错 modify on 20160602 by Yunxiao Lin
                if (curSystemItem.OutdoorItem != null)
                {
                    string model = curSystemItem.OutdoorItem.ModelFull;
                    string fcode = model.Substring(model.Length - 1, 1);
                    string series = curSystemItem.OutdoorItem.Series;
                    string outdoorType = curSystemItem.OutdoorItem.Type;
                    //巴西还是要保留Header Branch功能。 modified by Shen Junjie on 2018/5/30
                    if (thisProject.RegionCode == "LA" && thisProject.SubRegionCode != "LA_BR")
                    {
                        //Delete Header Branch Option from pipe interface for region LA. add by Shen Junjie 2018/04/04
                    }
                    //HAPQ 除了YVAHP/R 系列以外均没有梳形管，HAPB的JTWH没有梳形管 20170513 by Yunxiao Lin
                    else if ((fcode == "Q" && !series.Contains("YVAHP") && !series.Contains("YVAHR")) || outdoorType.Contains("JTWH"))
                    {
                        // Belinda,只有SMZ与HAPB有梳型分歧管
                    }
                    //moved here by Shen Junjie on 2018/5/30
                    else if (series.Contains("IVX") && outdoorType == "HVN(P/C/C1)/HVRNM2 (Side flow)")
                    {
                        //SMZ IVX 屏蔽header branch 选项  20170704 by Yunxiao Lin
                    }
                    else if (thisProject.SubRegionCode == "TW")
                    {
                        //台湾所有系列都没有Header branch数据 add by Shen Junjie on 2018/8/9
                    }
                    else if (series == "Commercial VRF High ambient, JNBBQ" || series == "Commercial VRF CO, CNCQ")
                    {
                        //JNBBQ 和 CNCQ没有header branch add by Shen Junjie on 2018/9/25
                    }
                    else
                    {
                        this.jcbtnAddCP.Visible = true;
                    }

                    BindPipingDrawing(tnOut);
                    //string imgDir = MyConfig.WiringNodeImageDirectory;
                    DoDrawingWiring(tnOut);
                }
            }

            SetTreeViewOutdoorState();
        }

        /// <summary>
        /// 设置室外机节点的文字颜色，若当前配管图未成功，则显示红色警告
        /// </summary>
        private void SetTreeViewOutdoorState()
        {
            foreach (TreeNode tnOut in tvOutdoor.Nodes)
            {
                if (tnOut.Tag != null && tnOut.Tag is SystemVRF)
                {
                    SystemVRF sysItem = tnOut.Tag as SystemVRF;
                    if (!sysItem.IsPipingOK)
                    {
                        tnOut.ForeColor = UtilColor.ColorWarning;
                    }
                    else
                        tnOut.ForeColor = UtilColor.ColorOriginal;
                }
            }
        }

        private void tvOutdoor_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (this.Cursor == Cursors.Default)
            {
                _isSameOutdoorNode = false;
                TreeNode selNode = tvOutdoor.SelectedNode;
                if (selNode == null)
                    return;
                else
                {
                    selNode.BackColor = tvOutdoor.BackColor;
                    if (selNode.ForeColor != Color.Chocolate)   //已标注巧克力色特殊提醒忽略恢复字体颜色
                        selNode.ForeColor = tvOutdoor.ForeColor;
                }

                TreeNode selPNode = Global.GetTopParentNode(selNode);
                TreeNode newNode = e.Node;
                TreeNode newPNode = Global.GetTopParentNode(newNode);

                if (selPNode == newPNode)
                {
                    _isSameOutdoorNode = true;
                }
            }
            else
                e.Cancel = true;
        }

        private void tvOutdoor_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            _isSameOutdoorNode = true;

            TreeNode nodeOut = Global.GetTopParentNode(e.Node);
            DoEditTreeNode(e.Node, nodeOut);
        }

        private void pbAddOutdoor_Click(object sender, EventArgs e)
        {
            DoAddOutdoor();
        }

        private void pbEditOutdoor_Click(object sender, EventArgs e)
        {
            TreeNode node = tvOutdoor.SelectedNode;
            if (node != null)
            {
                TreeNode nodeOut = Global.GetTopParentNode(node);
                DoEditTreeNode(node, nodeOut);
            }
            else {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }
        }

        private void pbCopyOutdoor_Click(object sender, EventArgs e)
        {
            DoCopyOutdoor();
        }

        private void pbDeleteOutdoor_Click(object sender, EventArgs e)
        {
            if (this.tvOutdoor.SelectedNode == null) 
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }
            if (this.tvOutdoor.SelectedNode.Level == 0)
            {
                DoDeleteOutdoor(tvOutdoor.SelectedNode.Name);
                BindToControl_SelectedUnits();
                UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228
            }
        }

        #endregion

        #region Main_Piping

        private void jcbtnCollapsePiping_Click(object sender, EventArgs e)
        {
            SetSplitCollapse(this.splitPiping, this.jcbtnCollapsePiping);
        }

        // 切换室外机下拉框的值
        private void jccmbOutdoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.jccmbOutdoor.SelectedValue != null)
            {
                string sysID = this.jccmbOutdoor.SelectedValue.ToString();
                if (this.tvOutdoor.SelectedNode != null)
                {
                    TreeNode nodeOut = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                    if (nodeOut.Name != sysID)
                        SetSelectedOutdoorNode(sysID);
                }
            }
        }

        // 更改状态：是否手动输入连接管长度
        private void uc_CheckBox_PipingLength_CheckedChanged(object sender, EventArgs e)
        {
            if (curSystemItem != null && !isBinding)
            {
                curSystemItem.IsInputLengthManually = this.uc_CheckBox_PipingLength.Checked;
                if (curSystemItem.IsInputLengthManually)
                {
                    selNode = null;
                    aimItem = null;
                    SetSystemPipingOK(curSystemItem, false);
                    if (curSystemItem.IsManualPiping)
                    {
                        DoDrawingPiping(false);
                    }
                    else
                    {
                        DoDrawingPiping(true);
                    }
                    ResetScrollPosition();
                    PipingBLL pipBll = GetPipingBLLInstance();
                    PipingErrors errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
                    ShowWarningMsg(errorType);
                    if (errorType != PipingErrors.OK)
                    {
                        return;
                    }
                }
                else
                {
                    //取消手动管长选择时，将所有节点状态恢复默认 add on 20160509 by Yunxiao Lin
                    //foreach (Node nd in addFlowPiping.Nodes)
                    //{
                    //    setItemDefault(nd);
                    //}
                    selNode = null;
                    selNode2 = null;
                    aimItem = null;
                    SetSystemPipingOK(curSystemItem, false);
                    ////恢复最大等效管长
                    //curSystemItem.PipeEquivalentLength = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                    //恢复上次在室外机界面输入的管长
                    curSystemItem.PipeEquivalentLength = curSystemItem.PipeEquivalentLengthbuff;
                    curSystemItem.FirstPipeLength = curSystemItem.FirstPipeLengthbuff;
                    //自动配管时 重置Total Piping Length on 20190129 by xyj
                    curSystemItem.TotalPipeLength = 0;
                    //消息栏清空
                    ShowWarningMsg(PipingErrors.OK);
                    // 重新计算管长修正系数
                    PipingBLL pipBll = GetPipingBLLInstance();
                    curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, "Cooling");
                    curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, "Heating");   //添加制热Factor计算
                    ////恢复所有默认颜色
                    //foreach (Node node in addFlowPiping.Nodes)
                    //{
                    //    if (node is MyNodeIn || node is MyNodeCH || node is MyNodeYP)
                    //        setItemDefault(node);
                    //}
                    //DoDrawingPiping(true);
                }
                //DoDrawingPiping(true);
                // TODO:
                // 需要重新执行自动选型
                BindTreeNodeOutdoor(curSystemItem.Id);
                if (curSystemItem.OutdoorItem == null)
                {
                    JCMsg.ShowWarningOK("Please click 'Verification' button first!");
                    return;
                }

                curSystemItem.MyPipingNodeOut.Model = curSystemItem.OutdoorItem.AuxModelName;
                curSystemItem.MyPipingNodeOut.Name = curSystemItem.Name;

                if (curSystemItem.IsManualPiping)
                {
                    DoDrawingPiping(false);
                }
                else
                {
                    DoDrawingPiping(true);
                }
                ResetScrollPosition();

                SetTreeViewOutdoorState();
            }
        }

        // 改变配管图排版方向，垂直|水平
        private void uc_CheckBox_PipingVertical_CheckedChanged(object sender, EventArgs e)
        {
            uc_CheckBox cbx = sender as uc_CheckBox;
            if (cbx == uc_CheckBox_PipingVertical)
            {
                uc_CheckBox_PipingHorizontal.Checked = !uc_CheckBox_PipingVertical.Checked;
            }
            else
            {
                uc_CheckBox_PipingVertical.Checked = !uc_CheckBox_PipingHorizontal.Checked;
            }
            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                //还原选中状态
                uc_CheckBox_PipingVertical.Checked = !uc_CheckBox_PipingVertical.Checked;
                uc_CheckBox_PipingHorizontal.Checked = !uc_CheckBox_PipingHorizontal.Checked;
                return;
            }

            if (curSystemItem != null && !isBinding && curSystemItem.MyPipingNodeOut != null)
            {
                bool isVertical = this.uc_CheckBox_PipingVertical.Checked;
                curSystemItem.IsPipingVertical = isVertical;
                DoDrawingPiping(true);
                UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228
            }
        }

        // 改变配管图缩放比例
        private void jccmbScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            string scale = jccmbScale.Text.Trim();
            float f;
            if (!string.IsNullOrEmpty(scale) && float.TryParse(scale, out f))
            {
                f /= 100;
                addFlowPiping.Zoom = new Zoom(f, f);
            }
            else
            {
                jccmbScale.Text = "100";
            }
        }

        // 改变配管图缩放比例
        private void jccmbScale1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string scale = jccmbScale1.Text.Trim();
            float f;
            if (!string.IsNullOrEmpty(scale) && float.TryParse(scale, out f))
            {
                f /= 100;
                addFlowWiring.Zoom = new Zoom(f, f);
            }
            else
            {
                jccmbScale1.Text = "100";
            }
        }

        // 自适应窗口大小
        private void jcbtnFitWindow_Click(object sender, EventArgs e)
        {
            //GetMaxLocation(); // 计算 mx & my

            //// 若未撑满画布，则默认scale为100%，否则以实际撑满画布为准
            //float f = addFlowPiping.Size.Width / mX_piping;
            //float f1 = addFlowPiping.Size.Height / mY_piping;
            //f = f < f1 ? f : f1;
            //if (f > 1)
            //    f = 1; // 是否限制 FitWindow 时的 Zoom值 不超过100%

            float f = (new PipingBLL(thisProject)).GetFitWindowZoom(addFlowPiping);
            addFlowPiping.Zoom = new Zoom(f, f);

            jccmbScale.Text = (f * 100).ToString("n0"); // TODO:此处赋值无效
        }

        // 自适应窗口大小 Wiring 窗口
        private void jcbtnFitWindowWiring_Click(object sender, EventArgs e)
        {
            //SizeF sz = GetActualSize_wiring(addFlowWiring); // 计算 mx & my

            //// 若未撑满画布，则默认scale为100%，否则以实际撑满画布为准
            //float f = addFlowWiring.Size.Width / sz.Width;
            //float f1 = addFlowWiring.Size.Height / sz.Height;
            //f = f < f1 ? f : f1;
            //if (f > 1)
            //    f = 1; // 是否限制 FitWindow 时的 Zoom值 不超过100%
            float f = (new PipingBLL(thisProject)).GetFitWindowZoom(addFlowWiring);
            addFlowWiring.Zoom = new Zoom(f, f);

            jccmbScale1.Text = (f * 100).ToString("n0"); //TODO:此处赋值无效
        }

        // 重新按 TreeView 中节点的顺序生成默认结构的配管图
        private void jcbtnReset_Click(object sender, EventArgs e)
        {
            if (ManualPipingCleanConfirmation(false) != DialogResult.Yes)
            {
                return;
            }
            if (ResetPiping())
            {
                UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228
            }
        }

        // 绘制拖拽路径，实现鼠标松开时路径消失的效果
        private void addFlowPiping_BeforeAddLink(object sender, BeforeAddLinkEventArgs e)
        {
            e.Cancel = new CancelEventArgs(true);
        }

        // 鼠标在配管图区域内移动的处理，包括悬浮移动与拖拽节点的移动
        private void addFlowPiping_MouseMove(object sender, MouseEventArgs e)
        {
            scrollPosition = this.addFlowPiping.ScrollPosition;
            bool isLeftButton = (e.Button == MouseButtons.Left);

            if (isLeftButton || e.Button == MouseButtons.None)
            {
                CheckPipingNodeHover(addFlowPiping.PointedItem, isLeftButton);
            }
        }

        // 处理节点拖拽之后的重新排列
        private void addFlowPiping_MouseUp(object sender, MouseEventArgs e)
        {
            bool isLeftButton = (e.Button == MouseButtons.Left);
            if (!isLeftButton || e.Clicks != 1) return;

            PipingBLL pipBll = GetPipingBLLInstance();
             
            //检查是否可以拖动，两个方法合并成了一个 add by Shen Junjie 2018/01/24
            if (!pipBll.DraggingCheck(selNode, aimItem, curSystemItem))
            {
                return;
            }

            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                return;
            }

            SetSystemPipingOK(curSystemItem, false);

            //HR和HP统一处理，Heat Pump同样适用 add by Shen Junjie 2018/01/24
            pipBll.DragNode(selNode, aimItem, curSystemItem);
            
            utilPiping.setItemDefault(aimItem);

            //同步TreeView中室内机的排序
            DoUpdateTreeNodeOrder(curSystemItem);
            
            DoDrawingPiping(true);
            ResetScrollPosition();
            PipingErrors errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
            if (errorType == PipingErrors.OK)
            {
                BindTreeNodeOutdoor(curSystemItem.Id);
                if (curSystemItem.OutdoorItem != null)
                {
                    curSystemItem.MyPipingNodeOut.Model = curSystemItem.OutdoorItem.AuxModelName;
                    curSystemItem.MyPipingNodeOut.Name = curSystemItem.Name;
                }

                DoDrawingPiping(true);
                ResetScrollPosition();
            }
            ShowWarningMsg(errorType);
            SetTreeViewOutdoorState();
            UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228 
        }

        // 配管图中选中项目的切换事件
        private void addFlowPiping_SelectionChange(object sender, SelectionChangeArgs e)
        {
            Graphics g = addFlowPiping.CreateGraphics();
            PipingBLL pipBll = GetPipingBLLInstance();
            if (!e.Item.Selected)
            {
                selNode = null;
                //selPNode = null;
                bool flag = false;

                // 恢复显示红色警告连接线
                //if (e.Item.DrawColor != utilPiping.colorWarning)
                //    utilPiping.setItemDefault(e.Item);

                if (e.Item is Node)
                {
                    Node nd = (Node)e.Item;
                    if (nd.InLinks != null && nd.InLinks.Count > 0)
                    {
                        if (nd.DrawColor != utilPiping.colorWarning && nd.InLinks[0].DrawColor != utilPiping.colorWarning)
                            flag = true;
                    }
                }
                else if (e.Item is Link)
                {
                    Link lnk = (Link)e.Item;
                    if (lnk.DrawColor != utilPiping.colorWarning && lnk.Dst.DrawColor != utilPiping.colorWarning)
                        flag = true;
                }
                if (flag && curSystemItem.OutdoorItem != null)
                {
                    utilPiping.setItemDefault(e.Item);
                }
                //int i = pipBll.ValidatePipeLength(0, curSystemItem, ref addFlowPiping);
                //ShowWarningMsg(i);
            }

            if (e.Item.Selected)
            {
                bool flag = false;
                if (e.Item is MyNode)
                {
                    selNode = (MyNode)e.Item;
                    if (selNode.InLinks.Count > 0)
                    {
                        if (selNode.DrawColor != utilPiping.colorWarning && selNode.InLinks[0].DrawColor != utilPiping.colorWarning)
                            flag = true;

                        //selPNode = selNode.InLinks[0].Org;
                        bool isSelect = selNode.InLinks[0].Selected;
                        if (!isSelect)
                            selNode.InLinks[0].Selected = true;
                    }
                }
                else if (e.Item is Link)
                {
                    Link lnk = (Link)e.Item;
                    if (lnk.DrawColor != utilPiping.colorWarning && lnk.Dst.DrawColor != utilPiping.colorWarning)
                        flag = true;

                    if (!lnk.Dst.Selected)
                        lnk.Dst.Selected = true;
                }
                if (flag)
                    utilPiping.setItemSelected(e.Item);
            }

        }
        
        // 显示验证警告信息
        private void ShowWarningMsg(PipingErrors errorType)
        {
            double len = 0;
            int count = 0;
            string rate = "";
            string msg = "";
            string templen = "";
            string temphei = "";
            switch (errorType)
            {
                case PipingErrors.LINK_LENGTH://-1:
                    msg = Msg.PIPING_LINK_LENGTH;
                    break;
                case PipingErrors.WARN_ACTLENGTH://-2:
                    len = Unit.ConvertToControl(curSystemItem.MaxPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_WARN_ACTLENGTH(curSystemItem.MaxPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.EQLENGTH://-3:
                    len = Unit.ConvertToControl(curSystemItem.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_EQLENGTH(curSystemItem.MaxEqPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.FIRSTLENGTH://-4:
                    len = Unit.ConvertToControl(curSystemItem.MaxIndoorLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_FIRSTLENGTH(curSystemItem.TotalPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.LENGTHFACTOR://-5:
                    len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                    break;
                case PipingErrors.TOTALLIQUIDLENGTH://-6:
                    len = Unit.ConvertToControl(curSystemItem.MaxTotalPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_TOTALLIQUIDLENGTH(curSystemItem.MaxTotalPipeLength.ToString("n0"),len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MKTOINDOORLENGTH://-7:
                    len = Unit.ConvertToControl(curSystemItem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH(curSystemItem.ActualMaxMKIndoorPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MKTOINDOORLENGTH1://-8:
                    len = Unit.ConvertToControl(PipingBLL.MaxCHToIndoorTotalLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH(curSystemItem.ActualMaxMKIndoorPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MAINBRANCHCOUNT://-9:
                    count = PipingBLL.MaxMainBranchCount;
                    msg = Msg.PIPING_MAINBRANCHCOUNT(count.ToString());
                    break;
                case PipingErrors.COOLINGCAPACITYRATE://-10:
                    rate = PipingBLL.MinMainBranchCoolingCapacityRate;
                    msg = Msg.PIPING_COOLINGCAPACITYRATE(rate);
                    break;
                //case -11:
                //    rate = PipingBLL.MinMainBranchHeatingCapacityRate;
                //    msg = Msg.PIPING_HEATINGCAPACITYRATE(rate);
                //    break;
                case PipingErrors.COOLINGONLYCAPACITY://-12:
                    msg = Msg.PIPING_COOLINGONLYCAPACITY();
                    break;
                case PipingErrors.INDOORNUMBERTOCH://-13:
                    count = PipingBLL.MaxIndoorNumberConnectToCH;
                    msg = Msg.PIPING_INDOORNUMBERTOCH(count.ToString());
                    break;
                // 多台室外机组成机组时，校验第一分歧管到第一Piping Connection kit之间的管长不能小于0.5m add on 20170720 by Shen Junjie
                case PipingErrors.FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH://-14:
                    double betweenConnectionKits_Min = Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length);
                    string betweenConnectionKits_Msg = betweenConnectionKits_Min.ToString("n2") + ut_length;
                    msg = Msg.PIPING_FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH(betweenConnectionKits_Msg);
                    break;
                case PipingErrors._3RD_MAIN_BRANCH://-15:
                    //不能有第三层主分支。
                    msg = Msg.GetResourceString("ERROR_PIPING_3RD_MAIN_BRANCH");
                    break;
                case PipingErrors._4TH_BRANCH_NOT_MAIN_BRANCH://-16:
                    //第4(或更远的)分支不能是一个主分支。
                    msg = Msg.GetResourceString("ERROR_PIPING_4TH_BRANCH_NOT_MAIN_BRANCH");
                    break;
                case PipingErrors.DIFF_LEN_FURTHEST_CLOSESST_IU: //-17
                    msg = Msg.GetResourceString("ERROR_PIPING_DIFF_LEN_FURTHEST_CLOSESST_IU");
                    break;
                case PipingErrors.NO_MATCHED_BRANCH_KIT://-18
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_BRANCH_KIT");
                    break;
                case PipingErrors.NO_MATCHED_CHBOX://-19
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_CHBOX");
                    break;
                case PipingErrors.NO_MATCHED_MULTI_CHBOX: //-20
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_MULTI_CHBOX");
                    break;
                case PipingErrors.NO_MATCHED_SIZE_UP_IU: //-21
                    msg = Msg.WARNING_DATA_EXCEED;
                    break;
                case PipingErrors.MAX_HIGHDIFF_UPPER://-22:
                    len = Unit.ConvertToControl(curSystemItem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MAX_HIGHDIFF_LOWER://-23:
                    len = Unit.ConvertToControl(curSystemItem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_HeightDiffL(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MAX_HIGHDIFF_INDOOR://-24:
                    len = Unit.ConvertToControl(curSystemItem.MaxDiffIndoorHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_Indoor_HeightDiff(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MAX_CHBOXHIGHDIFF://-25:
                    len = Unit.ConvertToControl(curSystemItem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MAX_MULTICHBOXHIGHDIFF://-26:
                    len = Unit.ConvertToControl(curSystemItem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MAX_CHBOX_INDOORHIGHDIFF://-27:
                    len = Unit.ConvertToControl(curSystemItem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.INDOORLENGTH_HIGHDIFF://-28
                    templen = Unit.ConvertToControl(PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    msg = Msg.INDOORLENGTH_HIGHDIFF_MSG(templen,temphei);
                    break;
                case PipingErrors.CHBOXLENGTH_HIGHDIFF://-29
                    templen = Unit.ConvertToControl(PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    msg = Msg.CHBOXLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF://-30
                    templen = Unit.ConvertToControl(PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    msg = Msg.CHBOX_INDOORLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case PipingErrors.PIPING_CHTOINDOORTOTALLENGTH: //-31
                    len = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_CHTOINDOORTOTALLENGTH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.PIPING_LENGTH_HEIGHT_DIFF: //-32
                    len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diffs = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diffs).ToString("n2") + ut_length);
                    break;
                case PipingErrors.MIN_DISTANCE_BETWEEN_MULTI_KITS://-33
                    msg = Msg.PIPING_MIN_LEN_BETWEEN_MULTI_KITS(Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length).ToString() + ut_length);
                    break;
                case PipingErrors.ODU_PIPE_LENGTH_LIMITS://-34
                    msg = Msg.GetResourceString("PIPING_ODU_PIPE_LENGTH_LIMITS");
                    break;
                case PipingErrors.MAX_BIG_IDU_OF_MUlTI_CHBOX: //-35
                    //每个multiple CH-Box最多只能连接2个8HP/10HP的IDU。 add by Shen Junjie on 2018/8/17
                    //适用所有multi CH Box 不限区域 modified by Shen Junjie on 2018/9/28
                    msg = Msg.GetResourceString("MAX_BIG_IDU_OF_MUlTI_CHBOX");
                    break;
                case PipingErrors.MANUAL_PIPING_INDOORS_MISSING: //-36
                    //手工配管图缺少部分室内机 add by Shen Junjie on 2018/11/22
                    msg = Msg.GetResourceString("MANUAL_PIPING_INDOORS_MISSING");
                    break;
                case PipingErrors.MANUAL_PIPING_REDUNDANT_UNITS:  //-37
                    //手工配管图有多余的节点 add by Shen Junjie on 2018/11/22
                    msg = Msg.GetResourceString("MANUAL_PIPING_REDUNDANT_UNITS");
                    break;
                case PipingErrors.WARN_ACTLENGTH_FA://-38:
                    len = Unit.ConvertToControl(curSystemItem.MaxPipeLengthwithFA, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_WARN_ACTLENGTH(curSystemItem.MaxPipeLengthwithFA.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.OK: //0
                default:
                    msg = "";
                    break;
            }
            ShowWarningMsg(msg);
        }

        private void ShowWarningMsg(string msg)
        {
            ShowWarningMsg(msg, Color.Red);
        }

        private void ShowWarningMsg(string msg, Color msgColor)
        {
            tlblStatus.Text = msg;
            tlblStatus.ForeColor = msgColor;
        }

        // 添加树型管 按钮事件
        private void jcbtnAddCP_Click(object sender, EventArgs e)
        {
            // 每个系统中只允许添加一个 CP 
            // 检查当前配管图中是否允许添加树形管CP节点
            if (!CheckCPNodeAddible())
            {
                return;
            }

            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                return;
            }

            MyNode selPNode = null;
            if (selNode != null)
            {
                selPNode = selNode.ParentNode;
            }
            MyNodeYP cp = utilPiping.createNodeYP(true);
            cp.AddChildNode(selNode);
            //MyNodeYP spn = selPNode as MyNodeYP;
            //spn.ReplaceChildNode(selNode, cp);
            // 添加CH 判断 20160505 by Yunxiao Lin
            PipingBLL.ReplaceChildNode(selPNode, selNode, cp);

            DoDrawingPiping(true);
            ResetScrollPosition();
            UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228
        }

        // 鼠标双击配管图 Link 对象，输入管长信息
        private void addFlowPiping_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (addFlowPiping.SelectedItem == null) return;
            Item selectedItem = addFlowPiping.SelectedItem;

            //双击室内机打开编辑对话框  add by Shen Junjie on 20170619
            if (selectedItem is MyNodeIn)
            {
                MyNodeIn nodeIn = selectedItem as MyNodeIn;
                DoEditIndoor(nodeIn.RoomIndooItem, false);
                //BindToControl_SelectedUnits();
                ResetScrollPosition();
                return;
            }

            if (curSystemItem == null || !curSystemItem.IsInputLengthManually)
                return;

            //双击室外机机组内部的连接管的各部分管长  add by Shen Junjie on 20170718
            if (selectedItem is MyNodeOut)
            {
                MyNodeOut nodeOut = selectedItem as MyNodeOut;
                if (nodeOut.UnitCount > 1)
                {
                    frmInputOutdoorPipeLength frm = new frmInputOutdoorPipeLength(curSystemItem, _thisProject.RegionCode);
                    if (DialogResult.OK == frm.ShowDialog())
                    {
                        SetSystemPipingOK(curSystemItem, false);

                        if (curSystemItem.IsManualPiping)
                        {
                            DoDrawingPiping(false);
                        }
                        else
                        {
                            DoDrawingPiping(true);
                        }
                        ResetScrollPosition();
                        PipingBLL pipBll = GetPipingBLLInstance();
                        PipingErrors errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
                        ShowWarningMsg(errorType);
                    }
                    return;
                }
            }

            // 输入管长信息
            if (e.Button == MouseButtons.Left && e.Clicks == 2)
            {
                if (selectedItem != null && selectedItem is MyLink)
                {
                    MyLink mylink = selectedItem as MyLink;
                    frmInputPipeLength f = new frmInputPipeLength(mylink, curSystemItem);
                    Node node = mylink.Dst;
                    if (node is MyNodeIn)
                    {
                        Indoor indoor = (node as MyNodeIn).RoomIndooItem.IndoorItem;
                        string outDoorModel = indoor.Model_Hitachi;
                        //Regex reg = new Regex(@"RPK-(.*?)FSNH3M");
                        Regex reg = new Regex(@"RPK-(.*?)(FSNH3M|FSNSH3)"); //HAPM IDU High Wall (w/o EXV)有两种model: FSNH3M & FSNSH3 20161024 by Yunxiao Lin
                        var mat = reg.Match(outDoorModel);
                        if (mat.Success)
                        {
                            f.IsValve = true;
                        }
                    }
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        PipingBLL pipBll = GetPipingBLLInstance();
                        if (f.ApplyToAll)
                        {
                            pipBll.SetAllLinks(f.ElbowQty, f.OilTrapQty, f.Length, f.ValveLength);
                        }
                        else
                        {
                            foreach (Link lk in node.InLinks)
                            {
                                if (lk is MyLink)
                                {
                                    MyLink mylink1 = lk as MyLink;
                                    mylink1.ElbowQty = f.ElbowQty;
                                    mylink1.OilTrapQty = f.OilTrapQty;
                                    mylink1.Length = f.Length;
                                    mylink1.ValveLength = f.ValveLength;
                                }
                            }
                        }
                        
                        SetSystemPipingOK(curSystemItem, false);

                        if (curSystemItem.IsManualPiping)
                        {
                            DoDrawingPiping(false);
                        }
                        else
                        {
                            DoDrawingPiping(true);
                        }
                        ResetScrollPosition();
                        PipingErrors errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
                        if (errorType == PipingErrors.OK)
                        {
                            BindTreeNodeOutdoor(curSystemItem.Id);
                            if (curSystemItem.OutdoorItem != null)
                            {
                                curSystemItem.MyPipingNodeOut.Model = curSystemItem.OutdoorItem.AuxModelName;
                                curSystemItem.MyPipingNodeOut.Name = curSystemItem.Name;
                            }

                            if (curSystemItem.IsManualPiping)
                            {
                                DoDrawingPiping(false);
                            }
                            else
                            {
                                DoDrawingPiping(true);
                            }
                            ResetScrollPosition();
                        }
                        SetTreeViewOutdoorState();
                        ShowWarningMsg(errorType);
                    }

                }
            }

        }

        // 配管规则帮助说明
        private void pbPipingRule_Click(object sender, EventArgs e)
        {
            string path = MyConfig.AppPath + "\\NVRF\\PipingRule.pdf";
            if (File.Exists(path))
            {
                string msg = "";
                Util.Process_Start(path, out msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    JCMsg.ShowErrorOK(msg);
                }
            }
        }

        // 手动验证按钮
        private void jcbtnVarification_Click(object sender, EventArgs e)
        {
            //验证系统
            DoPipingFinalVerification();
        }

        /// <summary>
        /// 验证系统
        /// </summary>
        private void DoPipingFinalVerification()
        {
            PipingErrors errorType = PipingErrors.OK;
            if (curSystemItem == null || curSystemItem.OutdoorItem == null)
                return;

            if (curSystemItem.IsManualPiping && curSystemItem.IsUpdated)
            {
                //如果手工配管之后更改系统，则不能通过验证。
                return;
            }
            PipingBLL pipBll = GetPipingBLLInstance();
            bool isHR = PipingBLL.IsHeatRecovery(curSystemItem);
            //判断每个节点的管型 并 重置线条颜色
            pipBll.CheckPipesType(curSystemItem.MyPipingNodeOut, isHR);

            if (errorType == PipingErrors.OK && curSystemItem.IsManualPiping)
            {
                errorType = pipBll.ValidateManualPiping(curSystemItem);
            }

            #region 室内机与室外机直接的高度差验证提醒 暂时关闭  on 20180615 by xyj
            ////欧洲区域 高度差提醒  //on 20180607  by xyj
            //if (_mainRegion.StartsWith("EU"))
            //{  
            //    //验证系统是否设置了IDU High Difference 
            //    bool isMsg = false; //默认不提醒设置高度差 
            //                        //获取当前系统 下的IDU  
            //    List<RoomIndoor> iduList = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(curSystemItem.Id);
            //    List<RoomIndoor> iduType = iduList.FindAll(p => p.PositionType == PipingPositionType.SameLevel.ToString());
            //    if (iduType != null && iduType.Count > 0)
            //    {
            //        if (iduType.Count == iduList.Count)  //如果当前室内机 未设置高度差
            //        {
            //            isMsg = true;
            //        }
            //    }
            //    if (isMsg)
            //    {
            //        DialogResult res = JCMsg.ShowConfirmYesNoCancel(Msg.PIPING_IDUHIGHDIFFERENCE);
            //        if (res == DialogResult.Yes)
            //        {
            //            PipingLengthAndHighDifference(); //打开高度差界面 设置高度差
            //            return;
            //        }
            //        else if (res == DialogResult.Cancel)
            //        {
            //            return;
            //        }
            //    }
            //}
            #endregion

            // 检验配管长度，必须在执行配管计算之前执行
            // CH-Box到Indoor总管长检验，因为需要CH-Box型号，所以需要放到配管计算之后 20160515 by Yunxiao Lin
            // 检验配管实际长度以及配管估算长度
            this.Cursor = Cursors.WaitCursor;
            if (errorType == PipingErrors.OK)
            {
                pipBll.SetPipingLimitation(curSystemItem);
                errorType = pipBll.ValidateSystemHighDifference(curSystemItem);

                if (errorType == PipingErrors.OK)
                {
                    errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
                }
            }

            if (errorType == PipingErrors.OK)
            {
                //如果排管未完成或存在错误，每次点击验证按钮都需要将上次标红的对象恢复。add on 20160727 by Yunxiao Lin
                if (!curSystemItem.IsPipingOK)
                {
                    pipBll.SetDefaultColor(addFlowPiping);
                }
                if (errorType == PipingErrors.OK)
                {
                    //前面已经做了“实际管长不小于高度差”校验，所以这里已经不可能成立。 comment by Shen Junjie on 2018/7/18
                    if (curSystemItem.PipeEquivalentLength < curSystemItem.HeightDiff)
                    {
                        errorType = PipingErrors.PIPING_LENGTH_HEIGHT_DIFF; //-32;
                    }
                }

                //检验MainBranch数量-9 和分支室内机容量比例-10 -11
                if (curSystemItem.IsInputLengthManually)
                {
                    //机型必须满足以下条件才能验证
                    //JCValidate jcv = new JCValidate();
                    //string model = curSystemItem.OutdoorItem.Model_Hitachi.Trim();
                    //string outMsg = "";
                    //if (jcv.ValiRegularRexp(model, "RAS-(.*?)FSXNQ", out outMsg) ||
                    //    jcv.ValiRegularRexp(model, "RAS-(.*?)FSN6Q", out outMsg) ||
                    //    jcv.ValiRegularRexp(model, "RAS-(.*?)FSNA6Q", out outMsg) ||
                    //    jcv.ValiRegularRexp(model, "RAS-(.*?)FSDNQ", out outMsg) ||
                    //    jcv.ValiRegularRexp(model, "RAS-(.*?)FSXN", out outMsg)// || // 已和PM确认 RAS-FS(X)N(S|P|S7B|S5B|P7B|P5B) 不需要check main branch count 20171128 Yunxiao Lin
                    //    //jcv.ValiRegularRexp(model, "RAS-(.*?)FSN(S|P|S7B|S5B|P7B|P5B)", out outMsg) ||
                    //    //jcv.ValiRegularRexp(model, "RAS-(.*?)FSXN(S|P)", out outMsg)) //20170831 增加FSXNS/FSXNP的piping限制 Yunxiao Lin
                    //    )
                    //{
                    //}
                    errorType = pipBll.ValMainBranch(curSystemItem, addFlowPiping);
                }
                if (errorType == PipingErrors.OK)
                {
                    //检验Heat Recovery系统内Cooling Only内机容量是否超过全部室内机容量的50% -12
                    //string HeatType = curSystemItem.OutdoorItem.ModelFull.Substring(3, 1);
                    //string HeatType = curSystemItem.OutdoorItem.ProductType.Contains("Heat Recovery") || curSystemItem.OutdoorItem.ProductType.Contains(", HR") ? "R":"H";
                    if (PipingBLL.IsHeatRecovery(curSystemItem) && !pipBll.ValCoolingOnlyIndoorCapacityRate(curSystemItem, addFlowPiping))
                    {
                        errorType = PipingErrors.COOLINGONLYCAPACITY; //-12;
                    }
                }

                if (errorType == PipingErrors.OK)
                {
                    //每个multiple CH-Box最多只能连接2个8HP/10HP的IDU
                    //适用所有multi CH Box 不限区域 modified by Shen Junjie on 2018/9/28
                    errorType = pipBll.ValidateIDUOfMultiCHBox(curSystemItem);
                }

                if (errorType == PipingErrors.OK)
                {
                    SetSystemPipingOK(curSystemItem, true);
                    // 执行配管计算并绑定配管数据，连接管管径规格等
                    DoPipingCalculation(pipBll, curSystemItem.MyPipingNodeOut, out errorType);
                    if (curSystemItem.IsPipingOK)
                    {
                        //检验CH-Box到远端Indoor的总长-8 add on 20160516 by Yunxiao Lin
                        if (curSystemItem.IsInputLengthManually)
                        {
                            errorType = pipBll.ValCHToIndoorMaxTotalLength(curSystemItem, curSystemItem.MyPipingNodeOut); 
                        }
                        if (errorType == PipingErrors.OK)
                        {
                            //检验CH-Box连接的室内机数量 -13
                            if (!pipBll.ValMaxIndoorNumberConnectToCH(curSystemItem, addFlowPiping))
                            {
                                errorType = PipingErrors.INDOORNUMBERTOCH; //-13;
                            }
                        }
                        if (errorType == PipingErrors.OK)
                        {
                            SetSystemPipingOK(curSystemItem, true);

                            // 计算冷媒追加量
                            if (curSystemItem.IsInputLengthManually)
                            {
                                double d1 = pipBll.GetAddRefrigeration(curSystemItem, ref addFlowPiping);
                                curSystemItem.AddRefrigeration = d1;

                                //为管线图添加加注冷媒标注 2016-12-22 by shen junjie
                                pipBll.DrawAddRefrigerationText(curSystemItem);
                            }
                            else
                                curSystemItem.AddRefrigeration = 0;
                        }
                    }
                    pipBll.DrawTextToAllNodes(curSystemItem.MyPipingNodeOut, null, curSystemItem);
                    UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228
                }
            }

            if (errorType != PipingErrors.OK)
            {
                SetSystemPipingOK(curSystemItem, false);
            }

            //判断当前系统无ControlGroup指向重置状态
            if (thisProject.ControlSystemList.Count > 0 && string.IsNullOrEmpty(curSystemItem.ControlGroupID[0]))
            {
                thisProject.CentralControllerOK = false;
            }
                

            ShowWarningMsg(errorType);
            UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228

            SetTabControlImageKey();
            SetTreeViewOutdoorState();
            this.Cursor = Cursors.Default;
        }

        #endregion

        #region Main_Wiring

        private void jcbtnCollapseWiring_Click(object sender, EventArgs e)
        {
            SetSplitCollapse(this.splitWiring, this.jcbtnCollapseWiring);
        }

        ///// 绘制 Wiring 中的连接线
        ///// <summary>
        ///// 绘制 Wiring 中的连接线
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void addFlowWiring_Paint(object sender, PaintEventArgs e)
        //{
        //    //Graphics g = e.Graphics;
        //    //float fl = float.Parse(jccmbScale1.Text.Trim()) / 100;

        //    //foreach (PointF[] pt in ptArrayList)
        //    //{
        //    //    g.DrawLines(new Pen(utilPiping.colorWiring, fl), pt);
        //    //}

        //    //Pen pen_dash = new Pen(utilPiping.colorWiring, fl);
        //    //pen_dash.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        //    //pen_dash.DashPattern = new float[] { 5, 5 };
        //    //foreach (PointF[] pt in ptArrayList_power)
        //    //{
        //    //    g.DrawLines(pen_dash, pt);
        //    //}
        //}


        #endregion

        #region Main_Report

        private void uc_CheckBox_RptSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool checkedAll = uc_CheckBox_RptSelectAllModule.Checked;
            thisProject.IsRoomInfoChecked = checkedAll;
            thisProject.IsIndoorListChecked = checkedAll;
            //thisProject.IsOptionChecked = checkedAll;
            thisProject.IsOutdoorListChecked = checkedAll;
            thisProject.IsPipingDiagramChecked = checkedAll;
            thisProject.IsWiringDiagramChecked = checkedAll;
            thisProject.IsControllerChecked = checkedAll;
            thisProject.IsExchangerChecked = checkedAll;
            //thisProject.IsOptionPriceChecked = checkedAll;
            loadModuleExportFlag();
        }

        private void uc_CheckBox_RptSelectOne(object sender, EventArgs e)
        {
            saveModuleExportFlag();
        }

        private void uc_CheckBox_RptOptionPrice_CheckedChanged(object sender, EventArgs e)
        {
            saveModuleExportFlag();
        }

        private void uc_CheckBox_RptAllSystem_CheckedChanged(object sender, EventArgs e)
        {
            bool isAll = uc_CheckBox_RptAllSystem.Checked;
            //this.jcbtnRptSystemIn.Enabled = !isAll;
            //this.jcbtnRptSystemEx.Enabled = !isAll;

            if (isAll)
            {
                List<object> list = new List<object>();
                foreach (object item in lbRptOutdoorExcept.Items)
                {
                    list.Add(item);
                }

                foreach (object item in list)
                {
                    lbRptOutdoorIncluded.Items.Add(item);
                    lbRptOutdoorExcept.Items.Remove(item);
                    SystemVRFSimpleObject obj = item as SystemVRFSimpleObject;
                    if (obj == null)
                        return;
                    SystemVRF sysItem = (new ProjectBLL(thisProject)).GetSystem(obj.Id);
                    sysItem.IsExportToReport = true;
                }
            }
        }

        // 移入
        private void jcbtnRptSystemIn_Click(object sender, EventArgs e)
        {
            List<object> list = new List<object>();
            foreach (object item in lbRptOutdoorExcept.SelectedItems)
            {
                list.Add(item);
            }

            foreach (object item in list)
            {
                lbRptOutdoorIncluded.Items.Add(item);
                lbRptOutdoorExcept.Items.Remove(item);
                SystemVRFSimpleObject obj = item as SystemVRFSimpleObject;
                SystemVRF sysItem = (new ProjectBLL(thisProject)).GetSystem(obj.Id);
                sysItem.IsExportToReport = true;
                if (lbRptOutdoorExcept.Items.Count == 0)
                    this.uc_CheckBox_RptAllSystem.Checked = true;
            }
        }

        // 移出
        private void jcbtnRptSystemEx_Click(object sender, EventArgs e)
        {
            List<object> list = new List<object>();
            foreach (object item in lbRptOutdoorIncluded.SelectedItems)
            {
                list.Add(item);
            }

            foreach (object item in list)
            {
                lbRptOutdoorExcept.Items.Add(item);
                lbRptOutdoorIncluded.Items.Remove(item);
                SystemVRFSimpleObject obj = item as SystemVRFSimpleObject;
                SystemVRF sysItem = (new ProjectBLL(thisProject)).GetSystem(obj.Id);
                sysItem.IsExportToReport = false;
                this.uc_CheckBox_RptAllSystem.Checked = false;
            }
        }

        private void jcbtnRptDXFBrowse_Click(object sender, EventArgs e)
        {
            DialogResult ret = folderBrowserDialog1.ShowDialog();
            if (ret == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                jclblDXFFilePath.Text = path;
            }
        }

        private void jcbtnRptDXFPathClear_Click(object sender, EventArgs e)
        {
            this.jclblDXFFilePath.Text = Msg.GetResourceString("Msg_DXFPath");
        }

        private bool CheckRptDXFPath()
        {
            string dir = this.jclblDXFFilePath.Text;
            string msg = Msg.GetResourceString("Msg_DXFPath");
            if (string.IsNullOrEmpty(dir) || dir == msg)
            {
                DialogResult ret = folderBrowserDialog1.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    dir = folderBrowserDialog1.SelectedPath;
                    jclblDXFFilePath.Text = dir;
                }
                else
                {
                    jclblDXFFilePath.Text = msg;
                    return false;
                }
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch { return false; }
                }
            }
            return true;
        }

        private void jcbtnRptDXFOutput_Click(object sender, EventArgs e)
        {
            #region CADImportNet注册
            string Name = "Johnson Controls Building Equipment Technology (Wuxi) Co., Ltd.";
            string EMail = "Qizhen.Yang@jci.com";
            string Key = "7FDA60E381C19678F2EEE947EEA979286812A5C90F205A5D51C1F8681C261DED46051F58DE9CCA54945F1BDA87FB2E46571328EB509A3BDAF1B65738AFE60D46|353B0E3860D69AB5FBBAB1A27A877612CD10948BC488DBD2BA0D6347AC9CEF18F069C9FA0CC481022DE28D0BAA52EB27860B86855F454C1F25F93702F058D461|";

            System.Collections.ArrayList regDat = new System.Collections.ArrayList();
            regDat.Add(Name);
            regDat.Add(EMail);
            Protection.Register(Key, regDat, false);
            #endregion

            if (!CheckRptDXFPath()) return;
            string dir = this.jclblDXFFilePath.Text;

            try
            {
                //PipingBLL pipBll=GetPipingBLLInstance();//delete by axj 20170622 

                this.Cursor = Cursors.WaitCursor;
                bool allPass = true;
                //string wiringImgDir = MyConfig.WiringNodeImageDirectory; 
                // 遍历每个系统，导出对应的 DXF 文件
                for (int i = 0; i < tvOutdoor.Nodes.Count; i++)
                {
                    TreeNode tnOut = tvOutdoor.Nodes[i];
                    SystemVRF sysItem = thisProject.SystemList[i];

                    curSystemItem = sysItem;

                    // 1、绘制系统对应的 Piping 图（DoDrawing）
                    BindPipingDrawing(tnOut);

                    // 2、根据当前 AddFlow 中的 Piping 图绘制对应的emf文件
                    string emfFile = Application.StartupPath + "\\temp" + Guid.NewGuid().ToString() + ".emf";//
                    ExportVictorGraph(emfFile, addFlowPiping, curSystemItem);

                    // 3、加载 emf 文件，并将 emf 转为 dxf 文件, 新线程处理
                    LoadEMFFile(emfFile);

                    string dxfFile = System.IO.Path.Combine(dir, sysItem.Name + "(" + sysItem.OutdoorItem.AuxModelName + ").dxf");
                    if (!SaveAsDXF(dxfFile))
                    {
                        JCMsg.ShowErrorOK(sysItem.Name + ".dxf Save Failed!");
                        allPass = false;
                    }

                    //// TODO:
                    //// 1、绘制系统对应的 Wiring 图
                    DoDrawingWiring(tnOut);
                    //// 2、根据当前 AddFlow 中的 Wiring 图绘制对应的emf文件
                    string emfFile_wiring = Application.StartupPath + "\\temp2" + Guid.NewGuid().ToString() + ".emf";//
                    ExportVictorGraph_wiring(emfFile_wiring);

                    //// 3、加载 emf 文件，并将 emf 转为 dxf 文件, 新线程处理
                    LoadEMFFile(emfFile_wiring);

                    string dxfFile_wiring = dir + "\\" + sysItem.Name + "_wiring.dxf";
                    if (!SaveAsDXF(dxfFile_wiring))
                    {
                        JCMsg.ShowErrorOK(sysItem.Name + "_wiring.dxf Save Failed!");
                        allPass = false;
                    }
                }

                if (allPass)
                {
                    TreeNode tnOut = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                    BindPipingDrawing(tnOut); // 恢复当前选中节点绑定的 Piping 图
                    //DoDrawingWiring(tnOut, wiringImgDir);
                }

                //非EU区域暂时隐藏ControllerWiring on 20171229 by Shen Junjie
                //ANZ区域可以使用ControllerWiring modify by Shen Junjie on 20180117
                if (thisProject.RegionCode=="EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E" || thisProject.SubRegionCode == "ANZ")
                {
                    // 遍历每个Control Group，导出对应的 DXF 文件
                    ControllerWiringBLL cwBLL = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
                    int groupIndex = 0;
                    foreach (ControlGroup group in thisProject.ControlGroupList)
                    {
                        // 1、绘制Control Group对应的 Wiring 图
                        if (cwBLL.DrawWiring(group))
                        {
                            groupIndex++;
                            // 2、根据当前 AddFlow 中的 Wiring 图绘制对应的emf文件
                            string emfFile_controllerWiring = Application.StartupPath + "\\temp3_" + Guid.NewGuid().ToString() + ".emf";//
                            ExportVictorGraph_ContollerWiring(emfFile_controllerWiring, group);

                            // 3、加载 emf 文件，并将 emf 转为 dxf 文件, 新线程处理
                            LoadEMFFile(emfFile_controllerWiring);
                            string dxfFileName = "controller_wiring_" + groupIndex + "_(" + group.Name + ").dxf";
                            string dxfFile_controllerWiring = dir + "\\" + dxfFileName;
                            if (!SaveAsDXF(dxfFile_controllerWiring))
                            {
                                JCMsg.ShowErrorOK(dxfFileName + " Save Failed!");
                                allPass = false;
                            }
                        }
                    }
                }

                if (allPass)
                {
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                }
            }
            catch (Exception exc)
            {
                //JCMsg.ShowErrorOK(exc.Message + "\n\n" + exc.StackTrace);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void jcbtnRptBrowse_Click(object sender, EventArgs e)
        {
            DialogResult ret = folderBrowserDialog1.ShowDialog();
            if (ret == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                this.jclblRptFilePath.Text = path;
            }
        }

        private void jcbtnRptPathClear_Click(object sender, EventArgs e)
        {
            this.jclblRptFilePath.Text = Msg.GetResourceString("Msg_ReportPath");
        }

        private bool CheckRptPath()
        {
            string dir = jclblRptFilePath.Text;
            string msg = Msg.GetResourceString("Msg_ReportPath");
            if (string.IsNullOrEmpty(dir) || dir == msg)
            {
                DialogResult ret = folderBrowserDialog1.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    dir = folderBrowserDialog1.SelectedPath;
                    jclblRptFilePath.Text = dir;
                }
                else
                {
                    jclblRptFilePath.Text = msg;
                    return false;
                }
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch { return false; }
                }
            }
            return true;
        }

        private void jcbtnOutputReport_Click(object sender, EventArgs e)
        {
            DoSaveProject();

            if (!CheckRptPath()) return;
            string dir = this.jclblRptFilePath.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                // 遍历每个系统图，并导出对应的 DXF 文件 
                foreach (TreeNode tnOut in this.tvOutdoor.Nodes)
                {
                    // 1、绘制系统对应的 Piping图（DoDrawing）
                    if (tnOut.Tag != null && tnOut.Tag is SystemVRF)
                    {
                        SystemVRF sysItem = tnOut.Tag as SystemVRF;
                        curSystemItem = sysItem;
                        BindPipingDrawing(tnOut);
                        //string imgDir = MyConfig.WiringNodeImageDirectory;
                        DoDrawingWiring(tnOut);
                        DoSavePipingFilePicture(sysItem.NO.ToString());
                        DoSaveWiringFilePicture(sysItem.NO.ToString());
                    }
                }
                // 将Controller布局图保存为图片
                BindToControl_Controller_Report();
                DoSaveControllerPicture();
                //GetControllerPicture();

                //非EU区域暂时隐藏ControllerWiring on 20171229 by Shen Junjie
                //ANZ区域可以使用ControllerWiring modify by Shen Junjie on 20180117
                if (thisProject.RegionCode.StartsWith("EU_") || thisProject.SubRegionCode == "ANZ" || thisProject.RegionCode.StartsWith("ME_A") || thisProject.RegionCode.StartsWith("LA") || thisProject.RegionCode.StartsWith("ASEAN") || thisProject.RegionCode.StartsWith("INDIA"))
                {
                    //保存Controller Wiring图
                    ControllerWiringBLL cwBLL = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
                    foreach (ControlGroup group in thisProject.ControlGroupList)
                    {
                        if (cwBLL.DrawWiring(group))
                        {
                            DoSaveControllerWiringFilePicture(group.Id, group);
                        }
                    }
                }
                string rptFile = System.IO.Path.Combine(dir, thisProject.Name + ".doc");
                // 将数据写入指定的模板，然后另存至上方指定路径
                if (ProjectBLL.IsSupportedNewReport(thisProject))
                {
                    
                    thisProject.IsIduCapacityW = uc_CheckBox_IduCapacityW.Checked;
                    NewReport rpt = new NewReport(thisProject);
                    rpt.isActual = uc_CheckBox_Actual.Checked;
                    
                    // changed for york model on 20 Nov 2018
                    if (thisProject.BrandCode == "Y")
                    {
                        if (!rpt.ExportReportWord(MyConfig.ReportTemplateDirectory + "\\NewReport\\NewReportYork.doc", rptFile))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!rpt.ExportReportWord(MyConfig.ReportTemplateDirectory + "\\NewReport\\NewReport.doc", rptFile))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    ReportForAspose rpt = new ReportForAspose(thisProject);

                    if (!rpt.ExportReportWord(MyConfig.ReportTemplateNamePath, rptFile))
                        return;
                }

                TreeNode tnOut1 = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                BindPipingDrawing(tnOut1); // 恢复当前选中节点绑定的 Piping 图
                DoDrawingWiring(tnOut1);
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void jcbtnRptPipingOutput_Click(object sender, EventArgs e)
        {
            //DoSaveProject();

            if (!CheckRptDXFPath()) return;
            string dir = this.jclblDXFFilePath.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                //导出图片缩放到150% add by Shen Junjie on 2017/12/29
                Zoom orignalZoom = addFlowPiping.Zoom;
                addFlowPiping.Zoom = new Zoom(1.5f, 1.5f);

                // 遍历每个系统图，并导出对应的 DXF 文件 
                foreach (TreeNode tnOut in this.tvOutdoor.Nodes)
                {
                    // 1、绘制系统对应的 Piping 图（DoDrawing）
                    if (tnOut.Tag != null && tnOut.Tag is SystemVRF)
                    {
                        SystemVRF sysItem = tnOut.Tag as SystemVRF;
                        curSystemItem = sysItem;

                        BindPipingDrawing(tnOut);

                        DoSavePipingFilePicture(sysItem.Name, dir);
                    }
                }
                TreeNode tnOut1 = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                BindPipingDrawing(tnOut1); // 恢复当前选中节点绑定的 Piping 图

                //还原缩放 add by Shen Junjie on 2017/12/29
                addFlowPiping.Zoom = orignalZoom;

                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void jcbtnRptWiringOutput_Click(object sender, EventArgs e)
        {
            //DoSaveProject();

            if (!CheckRptDXFPath()) return;
            string dir = this.jclblDXFFilePath.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                //导出图片缩放到150% add by Shen Junjie on 2017/12/29
                Zoom orignalZoom = addFlowWiring.Zoom;
                addFlowWiring.Zoom = new Zoom(1.5f, 1.5f);

                // 遍历每个系统图，并导出对应的 DXF 文件 
                foreach (TreeNode tnOut in this.tvOutdoor.Nodes)
                {
                    // 1、绘制系统对应的 Piping 图（DoDrawing）
                    if (tnOut.Tag != null && tnOut.Tag is SystemVRF)
                    {
                        SystemVRF sysItem = tnOut.Tag as SystemVRF;
                        curSystemItem = sysItem;

                        BindPipingDrawing(tnOut);  //画Wiring图之前要刷新Piping图 add by Shen Junjie on 2017/12/29
                        DoDrawingWiring(tnOut);

                        DoSaveWiringFilePicture(sysItem.Name, dir);
                    }
                }
                TreeNode tnOut1 = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                BindPipingDrawing(tnOut1); // 恢复当前选中节点绑定的 Piping 图 add by Shen Junjie on 2017/12/29
                DoDrawingWiring(tnOut1);  // 恢复当前选中节点绑定的 Wiring 图

                //还原缩放 add by Shen Junjie on 2017/12/29
                addFlowWiring.Zoom = orignalZoom;

                //非EU区域暂时隐藏ControllerWiring on 20171229 by Shen Junjie
                //ANZ区域可以使用ControllerWiring modify by Shen Junjie on 20180117
                if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E" || thisProject.SubRegionCode == "ANZ")
                {
                    //保存Controller Wiring图
                    ControllerWiringBLL cwBLL = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
                    foreach (ControlGroup group in thisProject.ControlGroupList)
                    {
                        if (cwBLL.DrawWiring(group))
                        {
                            DoSaveControllerWiringFilePicture(group.Name,group, dir);
                        }
                    }
                }

                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void jcbtnOutputSpreadsheet_Click(object sender, EventArgs e)
        {
            DoSaveProject();

            if (!CheckRptPath()) return;
            string dir = this.jclblRptFilePath.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // 遍历每个系统图，并导出对应的 DXF 文件 
                foreach (TreeNode tnOut in this.tvOutdoor.Nodes)
                {
                    // 1、绘制系统对应的 Piping 图（DoDrawing）
                    if (tnOut.Tag != null && tnOut.Tag is SystemVRF)
                    {
                        SystemVRF sysItem = tnOut.Tag as SystemVRF;
                        curSystemItem = sysItem;

                        BindPipingDrawing(tnOut);
                        //string imgDir = MyConfig.WiringNodeImageDirectory;
                        DoDrawingWiring(tnOut);
                    }
                }

                string newFileName = System.IO.Path.Combine(dir, String.Format("VRF Order Form Report({0}).xls", thisProject.Name));

                //ExcelReport test = new ExcelReport(thisProject);
                //test.ExportReportExcel(AppDomain.CurrentDomain.BaseDirectory + "\\NVRF\\Template\\Selection Report Template.xlt", newFileName);

                ExcelReportAspose excelAspose = new ExcelReportAspose(thisProject);
                string fileName = "Selection Report Template.xlt";
                if (this.thisProject.BrandCode == "H")
                {
                    fileName = "Selection Report Template_H.xlt";
                }
                excelAspose.ExportReportExcel(AppDomain.CurrentDomain.BaseDirectory + "\\NVRF\\Template\\" + fileName, newFileName);

                //TreeNode tnOut = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                //BindPipingDrawing(tnOut.Tag as SystemVRF, tnOut); // 恢复当前选中节点绑定的 Piping 图
                //DoDrawingWiring(tnOut, wiringImgDir);
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // 重绘Report界面中ListBox
        private void lbRptOutdoorIncluded_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.Items.Count < 1)
                return;
            if (e.Index < 0)
                return;

            using (Graphics g = e.Graphics)
            {

                Brush myBrush = new SolidBrush(UtilColor.font_dgv);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(UtilColor.bg_selected), e.Bounds);
                    myBrush = new SolidBrush(UtilColor.font_selected);
                }
                else
                {
                    e.Graphics.FillRectangle(new SolidBrush(lb.BackColor), e.Bounds);
                }

                // 画出文本
                StringFormat f = new StringFormat();
                f.LineAlignment = StringAlignment.Center;
                f.Alignment = StringAlignment.Near;
                e.Graphics.DrawString(lb.GetItemText(lb.Items[e.Index]), e.Font, myBrush, e.Bounds, f);
                e.DrawFocusRectangle();
            }

        }

        // 价格权限验证
        private void jcbtnPriceOK_Click(object sender, EventArgs e)
        {
            CDL.SVRFreg sv = new CDL.SVRFreg();
            sv = CDL.Sec.GetSVRF();
            if (sv.Region == "China")
            {
                if (jctxtPriceActivation.Text == Global.ActivationPwd_Price_CN)
                    sv.PriceValid = true;
                else
                    sv.PriceValid = false;

            }
            else if (sv.Region == "Middle East")
            {
                if (jctxtPriceActivation.Text == Global.ActivationPwd_Price_ME)
                    sv.PriceValid = true;
                else
                    sv.PriceValid = false;
            }
            else if (sv.Region == "Asia")
            {
                if (jctxtPriceActivation.Text == Global.ActivationPwd_Price_ASIA)
                    sv.PriceValid = true;
                else
                    sv.PriceValid = false;
            }

            if (Global.IsSuperUser)
            {
                sv.PriceValid = true;
            }

            if (sv.PriceValid)
            {
                lblPriceActivation.Text = "Price Activation";
                jctxtPriceActivation.Visible = false;
                jcbtnPriceOK.Visible = false;
            }

            CDL.Sec.UpdateValidDate(sv);
        }

        #endregion

        #region 表头绘制

        // 重新绘制dgv的表头，背景色、边框等
        void dgvIndoor_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DrawPainTing(sender, e);

            //if (e.RowIndex == dgvIndoor.Rows.Count - 1)
            //{
            //    Rectangle border = e.CellBounds;
            //    border.X -= 1;
            //    //border.Y -= 1;

            //    e.Graphics.DrawRectangle(pen_dgvBorder, border);
            //    e.Graphics.DrawLine(new Pen(e.CellStyle.BackColor), e.CellBounds.X+1, e.CellBounds.Top, e.CellBounds.Right-1, e.CellBounds.Top);
            //    e.PaintContent(e.CellBounds);
            //    e.Handled = true;
            //    return;
            //}

        }

        // 重新绘制dgv的表头，背景色、边框等
        void dgvExchanger_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DrawPainTing(sender, e);
        }

        private void dgvIndoorNotAssigned_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = Color.FromArgb(130, 130, 130);
            Pen pen_dgvBorder = new Pen(Color.FromArgb(127, 127, 127), 0.1f);

            DataGridView dgv = sender as DataGridView;

            if (e.RowIndex == -1)
            {
                SolidBrush brush = new SolidBrush(color_bg_dgvHeader);
                using (brush)
                {
                    Rectangle border = e.CellBounds;
                    e.Graphics.FillRectangle(brush, border);
                    border.X -= 1;
                    border.Y -= 1;
                    if (e.ColumnIndex == 0)
                    {
                        border.X += 1;
                    }
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private void dgvMaterialList_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = Color.FromArgb(130, 130, 130);
            Pen pen_dgvBorder = new Pen(Color.FromArgb(127, 127, 127), 0.1f);

            DataGridView dgv = sender as DataGridView;

            if (e.RowIndex == -1)
            {
                SolidBrush brush = new SolidBrush(color_bg_dgvHeader);
                using (brush)
                {
                    Rectangle border = e.CellBounds;
                    e.Graphics.FillRectangle(brush, border);
                    border.X -= 1;
                    border.Y -= 1;
                    if (e.ColumnIndex == 0)
                    {
                        border.X += 1;
                    }
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        #endregion

        #endregion

        #region Response codes (Methods && Events) 事件响应代码

        #region 初始化

        /// 获取系统当前语言,初始化界面语言
        /// <summary>
        /// 获取系统当前语言,初始化界面语言
        /// </summary>
        private void InitFormLanguage()
        {

            string currLanguage = SystemSetting.UserSetting.fileSetting.settingLanguage == "" ? this.JCCurrentLanguage : SystemSetting.UserSetting.fileSetting.settingLanguage;

            switch (currLanguage)
            {
                case LangType.ENGLISH:
                    DoSwitchLanguage(tbtnLanguage_en.Name);
                    break;
                case LangType.CHINESE:
                    DoSwitchLanguage(tbtnLanguage_zh.Name);
                    break;
                case LangType.CHINESE_TRADITIONAL:
                    DoSwitchLanguage(tbtnLanguage_zht.Name);
                    break;
                case LangType.FRENCH:
                    DoSwitchLanguage(tbtnLanguage_fr.Name);
                    break;
                case LangType.TURKISH:
                    DoSwitchLanguage(tbtnLanguage_tr.Name);
                    break;
                case LangType.SPANISH:
                    DoSwitchLanguage(tbtnLanguage_es.Name);
                    break;
                case LangType.GERMANY:
                    DoSwitchLanguage(tbtnLanguage_de.Name);
                    break;
                case LangType.ITALIAN:
                    DoSwitchLanguage(tbtnLanguage_it.Name);
                    break;
                case LangType.BRAZILIAN_PORTUGUESS:
                    DoSwitchLanguage(tbtnLanguage_pt_BR.Name);
                    break;
            }
        }

        /// 初始化事件绑定
        /// <summary>
        /// 初始化事件绑定
        /// </summary>
        private void InitEvent()
        {
            #region ToolkitButton 工具栏按钮事件
            tbtnNew.MouseHover += new System.EventHandler(tbtn_MouseHover);
            tbtnOpen.MouseHover += new System.EventHandler(tbtn_MouseHover);
            tbtnSave.MouseHover += new System.EventHandler(tbtn_MouseHover);
            tbtnSaveAs.MouseHover += new System.EventHandler(tbtn_MouseHover);
            tbtnImport.MouseHover += new System.EventHandler(tbtn_MouseHover);
            tbtnExport.MouseHover += new System.EventHandler(tbtn_MouseHover);
            tbtnSetting.MouseHover += new System.EventHandler(tbtn_MouseHover);

            tbtnNew.MouseLeave += new System.EventHandler(tbtn_MouseLeave);
            tbtnOpen.MouseLeave += new System.EventHandler(tbtn_MouseLeave);
            tbtnSave.MouseLeave += new System.EventHandler(tbtn_MouseLeave);
            tbtnSaveAs.MouseLeave += new System.EventHandler(tbtn_MouseLeave);
            tbtnImport.MouseLeave += new System.EventHandler(tbtn_MouseLeave);
            tbtnExport.MouseLeave += new System.EventHandler(tbtn_MouseLeave);
            tbtnSetting.MouseLeave += new System.EventHandler(tbtn_MouseLeave);

            tbtnNew.MouseDown += new MouseEventHandler(tbtn_MouseDown);
            tbtnOpen.MouseDown += new MouseEventHandler(tbtn_MouseDown);
            tbtnSave.MouseDown += new MouseEventHandler(tbtn_MouseDown);
            tbtnSaveAs.MouseDown += new MouseEventHandler(tbtn_MouseDown);
            tbtnImport.MouseDown += new MouseEventHandler(tbtn_MouseDown);
            tbtnExport.MouseDown += new MouseEventHandler(tbtn_MouseDown);
            tbtnSetting.MouseDown += new MouseEventHandler(tbtn_MouseDown);

            #endregion

            #region 注销

            //this.jcbtnManageRoom.Click += new System.EventHandler(jcbtnManageRoom_Click);
            //this.dgvIndoor.CellDoubleClick += new DataGridViewCellEventHandler(dgvIndoor_CellDoubleClick);

            //this.tvOutdoor.AfterSelect += new TreeViewEventHandler(tvOutdoor_AfterSelect);
            //this.tvOutdoor.BeforeSelect += new TreeViewCancelEventHandler(tvOutdoor_BeforeSelect);
            //this.tvOutdoor.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(tvOutdoor_NodeMouseDoubleClick);

            //this.jccmbOutdoor.SelectedIndexChanged += new System.EventHandler(jccmbOutdoor_SelectedIndexChanged);
            #endregion

            // TreeView 控件事件绑定 不同系统室内机拖动事件 add by axj 20170119
            this.tvOutdoor.AllowDrop = true;
            tvOutdoor.ItemDrag += new ItemDragEventHandler(tvOutdoor_ItemDrag);
            tvOutdoor.DragOver += new DragEventHandler(tvOutdoor_DragOver);
            tvOutdoor.DragDrop += new DragEventHandler(tvOutdoor_DragDrop);

        }

        /// 新建项目时给 RegionVRF 下拉框赋值，基于CDL.Sec判断当前用户的区域权限
        /// <summary>
        /// 新建项目时给 RegionVRF 下拉框赋值，基于CDL.Sec判断当前用户的区域权限
        /// TODO:独立版需要更新
        /// </summary>
        /// <param name="regionAEC"></param>
        private void InitRegionVRF()
        {
            _mainRegion = Registration.SelectedSubRegion.ParentRegionCode;
            BindCmbSubRegion();
        }

        private void BindCmbSubRegion()
        {
            this.jccmbSubRegion.Enabled = false;
            RegionBLL bll = new RegionBLL();
            DataTable dt = bll.GetSubRegionList(_mainRegion);
            this.jccmbSubRegion.DisplayMember = "Region";
            this.jccmbSubRegion.ValueMember = "Code";
            this.jccmbSubRegion.DataSource = dt;
            this.jccmbSubRegion.SelectedValue = Registration.SelectedSubRegion.Code;

            if (!string.IsNullOrEmpty(this.jccmbSubRegion.Text))
            {
                BindCmbProductType();
                this.jccmbProductType.SelectedIndex = 0;
            }
        }

        private void BindCmbProductType()
        {
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            DataTable dt = productTypeBll.GetProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
            if (dt != null && dt.Rows.Count > 0)
            {
                this.jccmbProductType.ValueMember = "ProductType";
                this.jccmbProductType.DisplayMember = "Series";
                this.jccmbProductType.DataSource = dt;
            }
        }

        private void BindCmbSubRegion(Project myProj)
        {
            this.jccmbSubRegion.SelectedValue = myProj.SubRegionCode;

            if (!string.IsNullOrEmpty(this.jccmbSubRegion.Text))
            {
                BindCmbProductType(myProj);
                this.jccmbProductType.SelectedValue = myProj.ProductType;
            }
        }


        private void BindCmbProductType(Project myProj)
        {
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            //if (myProj.FactoryCode == null)
            //{
            //    string fcode="";
            //    if (myProj.RoomIndoorList != null && myProj.RoomIndoorList.Count > 0)
            //    {
            //        string model = myProj.RoomIndoorList[0].IndoorItem.ModelFull;
            //        fcode = model.Substring(model.Length - 1, 1);
            //    }
            //    myProj.FactoryCode = fcode;
            //    if (string.IsNullOrEmpty(fcode))
            //    {
            //        myProj.FactoryCode = Registration.SelectedFactoryCode;
            //    }
            //}
            DataTable dt = productTypeBll.GetProductTypeData(myProj.BrandCode, myProj.SubRegionCode);
            if (dt != null && dt.Rows.Count > 0)
            {
                this.jccmbProductType.ValueMember = "ProductType";
                this.jccmbProductType.DisplayMember = "Series";
                this.jccmbProductType.DataSource = dt;
            }
        }


        /// 初始化 AddFlow 控件属性，frmMain加载事件中调用
        /// <summary>
        /// 初始化 AddFlow 控件属性，frmMain加载事件中调用
        /// </summary>
        private void InitAddFlow()
        {
            addFlowPiping.MouseAction = MouseAction.Selection;   // 鼠标动作
            addFlowPiping.MultiSel = false;       // 是否允许多选
            addFlowPiping.AllowDrop = false;     // 外界拖入addflow
            addFlowPiping.AutoScroll = true;
            addFlowPiping.ScrollbarsDisplayMode = ScrollbarsDisplayMode.SizeOfDiagramOnly;

            //addFlowPiping.CanChangeOrg = true;   // 是否允许更改连线的源节点
            //addFlowPiping.CanChangeDst = true;   // 是否允许更改连线的目标节点
            // 不允许用户直接拖动连线
            addFlowPiping.CanChangeOrg = false;   // 是否允许更改连线的源节点
            addFlowPiping.CanChangeDst = false;   // 是否允许更改连线的目标节点
            addFlowPiping.CanDrawNode = false;   // 是否允许用户绘制节点

            addFlowPiping.CanLabelEdit = false;  // 是否允许用户编辑节点文字
            addFlowPiping.CanSizeNode = false;   // 是否允许用户更改节点尺寸
            addFlowPiping.CanMoveNode = false;   // 是否允许用户拖动节点

            addFlowPiping.LinkCreationMode = LinkCreationMode.AllNodeArea; // 绘制 Link 时选中的控制区域，整个Node区域
            addFlowPiping.LinkHandleSize = HandleSize.Large;
            addFlowPiping.LinkSelectionAreaWidth = LinkSelectionAreaWidth.Large;
            addFlowPiping.SelectionHandleSize = HandleSize.Medium;

            addFlowPiping.SendToBack();
            addFlowPiping.Images.Clear();

            jccmbScale.Text = "100";
            jccmbScale1.Text = "100";
            jccmbControllerWiringScale.Text = "100";

            // Wiring
            addFlowWiring.MouseAction = MouseAction.None;
            addFlowWiring.MultiSel = false;       // 是否允许多选
            addFlowWiring.AllowDrop = false;     // 外界拖入addflow
            addFlowWiring.AutoScroll = true;
            addFlowWiring.ScrollbarsDisplayMode = ScrollbarsDisplayMode.SizeOfDiagramOnly;

            addFlowWiring.CanChangeOrg = false;   // 是否允许更改连线的源节点
            addFlowWiring.CanChangeDst = false;   // 是否允许更改连线的目标节点
            addFlowWiring.CanDrawNode = false;   // 是否允许用户绘制节点
            addFlowWiring.CanDrawLink = false;
            addFlowWiring.CanDrawNode = false;

            addFlowWiring.CanLabelEdit = false;  // 是否允许用户编辑节点文字
            addFlowWiring.CanSizeNode = false;   // 是否允许用户更改节点尺寸
            addFlowWiring.CanMoveNode = false;   // 是否允许用户拖动节点
            //addFlowWiring.PageGrid = new Grid(true, true, GridStyle.DottedLines, Color.LightGray, new Size(3000, 100));

            addFlowWiring.LinkCreationMode = LinkCreationMode.AllNodeArea; // 绘制 Link 时选中的控制区域，整个Node区域
            addFlowWiring.LinkHandleSize = HandleSize.Small;
            addFlowWiring.LinkSelectionAreaWidth = LinkSelectionAreaWidth.Small;
            addFlowWiring.SelectionHandleSize = HandleSize.Small;

            addFlowWiring.SendToBack();
            addFlowWiring.Images.Clear();

        }

        private void InitControlProperties()
        {
            // 初始化树控件的显示属性
            this.tvOutdoor.ShowLines = false;
            this.tvOutdoor.FullRowSelect = true;
            this.tvOutdoor.ShowNodeToolTips = true; // false时，点击节点文字即展开子节点
            this.tvOutdoor.ItemHeight = 30;
            this.tvOutdoor.Indent = 20;

            this.pnlIndoorInfo.Dock = DockStyle.Fill;
            this.pnlOutdoorInfo.Dock = DockStyle.Fill;

            this.lbRptOutdoorIncluded.ItemHeight = 30;
            this.lbRptOutdoorIncluded.DrawMode = DrawMode.OwnerDrawFixed;


            // 初始化已选室内机表格的列名
            this.dgvIndoor.AutoGenerateColumns = false;
            this.dgvIndoorNotAssigned.AutoGenerateColumns = false;
            this.dgvExchanger.AutoGenerateColumns = false;
            NameArray_Indoor arr = new NameArray_Indoor();
            Global.SetDGVDataName(ref dgvIndoor, arr.RoomIndoor_DataName);
            Global.SetDGVName(ref dgvIndoor, arr.RoomIndoor_Name);
            Global.SetDGVHeaderText(ref dgvIndoor, arr.RoomIndoor_HeaderText);

            //加入exchanger 
            Global.SetDGVDataName(ref dgvExchanger, arr.RoomIndoor_DataName);
            Global.SetDGVName(ref dgvExchanger, arr.RoomIndoor_Name);
            Global.SetDGVHeaderText(ref dgvExchanger, arr.RoomIndoor_HeaderText);

            Global.SetDGVDataName(ref dgvIndoorNotAssigned, arr.RoomIndoorNotAttached_DataName);
            Global.SetDGVName(ref dgvIndoorNotAssigned, arr.RoomIndoorNotAttached_Name);
            Global.SetDGVHeaderText(ref dgvIndoorNotAssigned, arr.RoomIndoorNotAttached_HeaderText);

            // Controller 中控件的初始属性            
            if (jccmbControllerProductType.Items.Count == 0)
                jccmbControllerProductType.Items.Add("111");
            this.jccmbControllerType.SelectedIndex = 0;
            this.jccmbControllerProductType.SelectedIndex = 0;
            contrTypes = new List<CentralController>();

            //    // 初始化价格密码
            //    if (Global.IsPriceValid)
            //    {
            //        this.pnlActivationPrice.Visible = false;
            //    }
            //    else
            //    {
            //        this.pnlActivationPrice.Visible = true;
            //    }
        }


        /// 设置当前 TabPage 的图标，反应各个 Page 的状态
        /// <summary>
        /// 设置当前 TabPage 的图标，反应各个 Page 的状态
        /// </summary>
        private void SetTabControlImageKey()
        {
            foreach (TabPage tp in tabControl1.TabPages)
            {
                tp.ImageKey = "Check";
            }

            // TODO:临时增加
            //tabControl1.TabPages[3].ImageKey = "Error";
            //tabControl1.TabPages[4].ImageKey = "Error";
            //tabControl1.TabPages[5].ImageKey = "Error";
            //新增Exchanger 2017-07-07 by xyj

            int j = 3;
            //默认不绑定Exchanger选项 on 20170822 by xyj
            //j=3 不显示Exchanger选项，j=4 显示Exchanger选项
            j = IsContainsExchanger(j);           
            int index = 2;
            if (dgvIndoor.Rows.Count == 0 || !isIndoorOK)
                index = 2;
            else if (tvOutdoor.Nodes.Count == 0)
            {
                index = j;
            }
            else if (thisProject.SystemList != null)
            {
                bool isOK = true;
                //foreach (SystemVRF sysItem in thisProject.SystemList)
                //{
                //    if (!sysItem.IsPipingOK || sysItem.IsUpdated)
                //    {
                //        index = 4; //6
                //        isOK = false;
                //        break;
                //    }
                //}
                // 判断室外机选型错误和重新确认piping
                foreach (SystemVRF sysItem in thisProject.SystemList)
                {
                    if (sysItem.SysType == SystemType.OnlyIndoor)
                    {
                        // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                        if (sysItem.Ratio < 0.5 || sysItem.Ratio > sysItem.MaxRatio)
                        {
                            index = j;
                            isOK = false;
                            break;
                        }
                    }
                    else
                    {
                        // 多新风机或者混连模式 (配置率为80%～100%)
                        if (sysItem.Ratio < 0.8 || sysItem.Ratio > 1)
                        {
                            index = j;
                            isOK = false;
                            break;
                        }
                        //混连模式新风制冷容量有30%限制
                        if (sysItem.SysType == SystemType.CompositeMode && sysItem.RatioFA > 0.3)
                        {
                            index = j;
                            isOK = false;
                            break;
                        }
                        //if (thisProject.RoomIndoorList.Count == 1 && thisProject.RoomIndoorList[0].IndoorItem.Model.Equals("JTAF1080"))
                        //{
                        //    index = j;
                        //    isOK = false;
                        //    break;
                        //}
                    }

                    if (!sysItem.IsPipingOK || sysItem.IsUpdated)
                    {
                        index = j + 1; //6
                        isOK = false;
                        break;
                    }
                    if (!thisProject.CentralControllerOK)
                    {
                        index = j + 3;
                        isOK = false;
                    }
                }


                if (isOK)
                    return;
            }

            #region 判断是否有室外机选型错误 add on 20160802 by Yunxiao Lin
            //if (curSystemItem != null && curSystemItem.Ratio != 0d)
            //{
            //    if (curSystemItem.SysType == SystemType.OnlyIndoor)
            //    {
            //        // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
            //        if (curSystemItem.Ratio < 0.5 || curSystemItem.Ratio > curSystemItem.MaxRatio)
            //        {
            //            index = 3;
            //        }

            //    }
            //    else
            //    {
            //        // 多新风机或者混连模式，则配比率校验规则有变 (配置率为80%～100%)
            //        if (curSystemItem.Ratio < 0.8 || curSystemItem.Ratio > 1)
            //        {
            //            index = 3;
            //        }
            //        //混连模式新风制冷容量有30%限制
            //        if (curSystemItem.SysType == SystemType.CompositeMode && curSystemItem.RatioFA > 0.3)
            //        {
            //            index = 3;
            //        }
            //    }
            //}
            #endregion
             

            for (int i = index; i < tabControl1.TabPages.Count; ++i)
                tabControl1.TabPages[i].ImageKey = "Error";

            if (tabControl1.SelectedIndex > index - 1)
                tabControl1.SelectedIndex = index - 1;

        }

        /// <summary>
        /// 是否显示Exchanger 选项
        /// </summary>
        /// <param name="index">索引（如果存在Exchanger 数据 返回4 不存在返回3）</param>
        /// <returns></returns>
        private int IsContainsExchanger(int index)
        {
            int _count = 0;
            if (thisProject != null)
            {
                //判断是否存在Exchanger 类型
                if (thisProject.RegionCode=="EU_W" || thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_S")
                {
                    _count = 0;
                }
                else
                {
                    IndoorBLL indoor = new IndoorBLL(Registr.Registration.SelectedSubRegion.Code,
                        Registr.Registration.SelectedSubRegion.ParentRegionCode, 
                        Registr.Registration.SelectedBrand.Code);
                    //DataTable dt = indoor.GetExchangerTypeList(_mainRegion);
                    DataTable dt = indoor.GetExchangerTypeList();
                    if (dt != null)
                        _count = dt.Rows.Count;
                }

            }
            if (_count > 0)
            {
                //存在显示在选项卡中
                index = 4;
            }
            else
            {
                //移除Exchanger选项
                tabControl1.TabPages.Remove(tpgExchanger);
                //重新绘制选项卡的长度
                //tabControl1.ItemSize = new Size(120,40);  
            }
            return index;
        }

        /// 初始化Indoor界面数据列表标题
        /// <summary>
        /// 初始化Indoor界面数据列表标题
        /// </summary>
        private void Init()
        {
            InitEvent();

            InitFormLanguage();

            BindUnit();

            InitRegionVRF();

            InitAddFlow();

            InitController();

            InitControlProperties();
        }

        #endregion

        #region 绑定事件

        // 绑定单位表达式
        /// <summary>
        /// 绑定单位表达式
        /// </summary>
        private void BindUnit()
        {
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            this.jctxtAltitude.JCMinValue = 0;
            //当启用海拔修正时，海拔上限放到5000米。 20160531 by Yunxiao Lin
            //if (thisProject.EnableAltitudeCorrectionFactor)
            //    this.jctxtAltitude.JCMaxValue = (float)Unit.ConvertToControl(5000, UnitType.LENGTH_M, ut_length);
            //else
                this.jctxtAltitude.JCMaxValue = (float)Unit.ConvertToControl(5000, UnitType.LENGTH_M, ut_length);
            double value = Unit.ConvertToControl(Convert.ToDouble(this.jctxtAltitude.Text == "" ? "0" : this.jctxtAltitude.Text), UnitType.LENGTH_M, ut_length);
            //altitudetextIsAutoChange = true;
            this.jctxtAltitude.Text = value.ToString("n0");
            jclblUnitAltitude_m.Text = ut_length;
            //setAltitudeVisible(SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor);
        }

        // 绑定 Project 基本信息到界面控件
        /// <summary>
        /// 绑定 Project 基本信息到界面控件
        /// </summary>
        private void BindToControl_ProjectBaseInfo()
        {
            //选项系统添加项目名称 on 20170912 by xyj
            this.Text = Msg.GetResourceString("PROJECT_TITLE_NAME") + thisProject.Name;

            this.jctxtProjName.Text = thisProject.Name;
            //altitudetextIsAutoChange = true;
            this.jctxtAltitude.Text = thisProject.Altitude.ToString();
            //setAltitudeVisible(SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor);

            thisProject.RegionCode = _mainRegion;

            if (!String.IsNullOrEmpty(thisProject.SubRegionCode) && !String.IsNullOrEmpty(thisProject.ProductType))
            {
                BindCmbSubRegion(thisProject);
            }
            else
            {
                thisProject.SubRegionCode = Registration.SelectedSubRegion.Code;
                thisProject.BrandCode = Registration.SelectedBrand.Code;
            }

            if (String.IsNullOrEmpty(thisProject.FactoryCode))
            {
                thisProject.FactoryCode = Registration.SelectedFactoryCode;
            } 
            this.uc_CheckBox_Cooling.Checked = thisProject.IsCoolingModeEffective;
            this.uc_CheckBox_Heating.Checked = thisProject.IsHeatingModeEffective;

            //this.jctxtContractNo.Text = thisProject.ContractNO;
            //this.jctxtProjectRevision.Text = thisProject.ProjectRevision;
            //this.jctxtLocation.Text = thisProject.Location;
            //this.jctxtPurchaseOrderNo.Text = thisProject.PurchaseOrderNO;
            //this.jctxtRemarks.Text = thisProject.Remarks;
            //this.jctxtSalesName.Text = thisProject.SalesEngineer;
            //this.jctxtSalesOffice.Text = thisProject.SalesOffice;
            //this.jctxtSalesYINo.Text = thisProject.SalesYINO;
            //this.jctxtShipTo.Text = thisProject.ShipTo;
            //this.jctxtSoldTo.Text = thisProject.SoldTo;
            //this.timeDeliveryDate.Text = thisProject.DeliveryRequiredDate.ToShortDateString();
            //this.timeOrderDate.Text = thisProject.OrderDate.ToShortDateString();
            bindProjectInfo(_mainRegion, false);

            if (!String.IsNullOrEmpty(thisProject.ProjectUpdateDate)) //打开项目时加载项目最后修订日期 on 20180312 by xyj
            {
                this.jctxtProjectUpdateDate.Text = thisProject.ProjectUpdateDate;
            }
            else {
                this.jctxtProjectUpdateDate.Text = "";
            }
        }

        // 绑定制冷、制热工况信息
        /// <summary>
        /// 绑定制冷、制热工况信息
        /// </summary>
        private void BindToControl_ConditionState()
        {
            this.pnlCoolingInd.Enabled = thisProject.IsCoolingModeEffective;
            this.pnlCoolingOut.Enabled = thisProject.IsCoolingModeEffective;
            this.pnlHeatingInd.Enabled = thisProject.IsHeatingModeEffective;
            this.pnlCoolingOut.Enabled = thisProject.IsHeatingModeEffective;
        }



        /// <summary>
        /// 绑定Exchager 列表
        /// </summary>
        private void BindSelectedExchangerList()
        {
            this.dgvExchanger.Rows.Clear();
            if (thisProject.ExchangerList != null)
            {
                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    string roomID = ri.RoomID;
                    Indoor indoor = ri.IndoorItem;
                    //获取房间名称 on 20180808 by xyj
                    string roomStr = (new ProjectBLL(thisProject)).GetFloorAndRoom(roomID);

                    if (IndoorBLL.IsExchanger(indoor.Type))
                    {  

                        string modelAfterOption = (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
                        // ri.OptionItem == null ? ri.IndoorItem.ModelFull : ri.OptionItem.GetNewModelWithOptionB();

                        this.dgvExchanger.Rows.Add(
                            null,
                            ri.IndoorNO,
                            roomStr == "" ? ri.DisplayRoom : roomStr,
                            ri.IndoorName,
                            ri.IndoorItem.ModelFull,
                            modelAfterOption,
                            Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),
                            Unit.ConvertToControl(ri.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),
                            Unit.ConvertToControl(ri.SensibleHeat, UnitType.POWER, ut_power).ToString("n1"),
                            Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, ut_airflow).ToString("n1"),
                            Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),
                            Unit.ConvertToControl(ri.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),
                            //ri.RoomNOString,
                            ri.SystemID,
                            ri.RoomID
                            );

                        #region  标记不满足校验条件的RoomIndoor记录（仅需校验基于房间的全热交换机）
                        if (string.IsNullOrEmpty(ri.RoomID))
                            continue;

                        // 是否将不满足需求的记录标记出？？
                        DataGridViewRow row = dgvExchanger.Rows[dgvExchanger.Rows.Count - 1];
                        List<RoomIndoor> list = (new ProjectBLL(thisProject)).GetSelectedExchangerByRoom(ri.RoomID);
                        //if (!string.IsNullOrEmpty(ri.RoomID))
                        //{
                        //    row.Cells[Name_Common.RoomName].Style.ForeColor = System.Drawing.Color.Gray;
                        //}
                        double capSum_esp = 0;
                        double capSum_air = 0;
                        foreach (RoomIndoor riItem in list)
                        {
                            capSum_esp += Convert.ToDouble(ri.RqFreshAir);
                            capSum_air += riItem.AirFlow;
                        }
                        if (capSum_esp < Convert.ToDouble(ri.RqStaticPressure) * 0.9) // （静压）全热交换机与房间的配比率放宽10%，允许[90%～100%]
                        {
                            row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                        }
                        //else if (thisProject.IsHeatingModeEffective && capSum_h < ri.RqHeatingCapacity * 0.9)
                        // 一个Project中可能存在多个productType，所以判断是否需要制热，不仅仅需要判断IsHeatingModeEffective，还需要判断室内机的ProductType。 20160826 by Yunxiao Lin
                        else if (capSum_air < ri.RqAirflow * 0.9)
                        {
                            row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                        }

                        #endregion

                    }
                }
            }
            SetTabControlImageKey();
        }


        /// <summary>
        /// 获取房间信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public Room GetRoomInfo(string roomId)
        {
            Room room = new Room();
            if (!string.IsNullOrEmpty(roomId))
            {
                foreach (Floor f in thisProject.FloorList)
                {
                    foreach (Room rm in f.RoomList)
                    {
                        if (rm.Id == roomId)
                        {
                            room = rm;
                            break;
                        }
                    }
                }
            }
            return room;
        }




        // a、绑定已选室内机列表
        /// <summary>
        /// a、绑定已选室内机列表
        /// </summary>
        private void BindSelectedIndoorList()
        {
            this.dgvIndoor.Rows.Clear();
            isIndoorOK = true;
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                string roomID = ri.RoomID;
                Indoor indoor = ri.IndoorItem;

                //获取房间名称 或新风区域名称 on 20180808 by xyj
                string roomStr = (new ProjectBLL(thisProject)).GetFloorAndRoom(roomID);

                string modelAfterOption = (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
                // ri.OptionItem == null ? ri.IndoorItem.ModelFull : ri.OptionItem.GetNewModelWithOptionB();

                this.dgvIndoor.Rows.Add(
                    null,
                    ri.IndoorNO,
                    roomStr == "" ? ri.DisplayRoom : roomStr,
                    ri.IndoorName,
                    ri.IndoorItem.ModelFull,
                    modelAfterOption,
                    Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),
                    Unit.ConvertToControl(ri.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),
                    Unit.ConvertToControl(ri.SensibleHeat, UnitType.POWER, ut_power).ToString("n1"),
                    Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, ut_airflow).ToString("n1"),
                    Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),
                    Unit.ConvertToControl(ri.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),
                    //ri.RoomNOString, 
                    ri.SystemID,
                    ri.RoomID
                    );

                #region  标记不满足校验条件的RoomIndoor记录（仅需校验基于房间的室内机）
                if (string.IsNullOrEmpty(ri.RoomID))
                    continue;

                // 是否将不满足需求的记录标记出？？
                DataGridViewRow row = dgvIndoor.Rows[dgvIndoor.Rows.Count - 1];
                List<RoomIndoor> list = (new ProjectBLL(thisProject)).GetSelectedIndoorByRoom(ri.RoomID);
                //if (!string.IsNullOrEmpty(ri.RoomID))
                //{
                //    row.Cells[Name_Common.RoomName].Style.ForeColor = System.Drawing.Color.Gray;
                //}
                //如果是新风机需要验证房间的FreshAir 是否满足新风机 on 20181130 by xyj
                if (ri.IsFreshAirArea)
                {
                    if (!string.IsNullOrEmpty(ri.RoomID))
                    {
                        if (!SatisfyIsFreshAir(ri.RoomID))
                        {
                            row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                        }
                    }
                    continue;
                }
                double capSum_c = 0;
                double capSum_sh = 0;
                double capSum_h = 0;
                foreach (RoomIndoor riItem in list)
                {
                    capSum_c += riItem.CoolingCapacity;
                    capSum_sh += riItem.SensibleHeat;
                    capSum_h += riItem.HeatingCapacity;
                }
                if (thisProject.IsCoolingModeEffective && !ri.IndoorItem.Type.Contains("Hydro Free-High temp.") && (capSum_c < ri.RqCoolingCapacity * 0.9 || capSum_sh < ri.RqSensibleHeat * 0.9)) // 室内机与房间的配比率放宽10%，允许[90%～100%]
                {
                    row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                    isIndoorOK = false;
                }
                //else if (thisProject.IsHeatingModeEffective && capSum_h < ri.RqHeatingCapacity * 0.9)
                // 一个Project中可能存在多个productType，所以判断是否需要制热，不仅仅需要判断IsHeatingModeEffective，还需要判断室内机的ProductType。 20160826 by Yunxiao Lin
                else if (thisProject.IsHeatingModeEffective && !ri.IndoorItem.ProductType.Contains(", CO") && capSum_h < ri.RqHeatingCapacity * 0.9)
                {
                    row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                }

                #endregion

            }

            #region  欧洲区域的加上 ExchangerList  on 2017-12-22  by xyj
            if (_mainRegion=="EU_E" || _mainRegion == "EU_W" || _mainRegion == "EU_S")
            {
                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    string roomID = ri.RoomID;
                    Indoor indoor = ri.IndoorItem;
                    //获取房间名称 或新风区域名称 on 20180808 by xyj
                    string roomStr = (new ProjectBLL(thisProject)).GetFloorAndRoom(roomID);

                    string modelAfterOption = (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
                    // ri.OptionItem == null ? ri.IndoorItem.ModelFull : ri.OptionItem.GetNewModelWithOptionB();

                    this.dgvIndoor.Rows.Add(
                        null,
                        ri.IndoorNO,
                        roomStr == "" ? ri.DisplayRoom : roomStr,
                        ri.IndoorName,
                        ri.IndoorItem.ModelFull,
                        modelAfterOption,
                        Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),
                        Unit.ConvertToControl(ri.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),
                        Unit.ConvertToControl(ri.SensibleHeat, UnitType.POWER, ut_power).ToString("n1"),
                        Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, ut_airflow).ToString("n1"),
                        Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),
                        Unit.ConvertToControl(ri.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),
                        //ri.RoomNOString, 
                        ri.SystemID,
                        ri.RoomID,
                        ri.IsExchanger
                        );

                    #region  标记不满足校验条件的RoomIndoor记录（仅需校验基于房间的室内机）
                    if (string.IsNullOrEmpty(ri.RoomID))
                        continue;

                    // 是否将不满足需求的记录标记出？？
                    DataGridViewRow row = dgvIndoor.Rows[dgvIndoor.Rows.Count - 1];
                    List<RoomIndoor> list = (new ProjectBLL(thisProject)).GetSelectedIndoorByRoom(ri.RoomID);
                    //if (!string.IsNullOrEmpty(ri.RoomID))
                    //{
                    //    row.Cells[Name_Common.RoomName].Style.ForeColor = System.Drawing.Color.Gray;
                    //}

                    double capSum_c = 0;
                    double capSum_sh = 0;
                    double capSum_h = 0;
                    foreach (RoomIndoor riItem in list)
                    {
                        capSum_c += riItem.CoolingCapacity;
                        capSum_sh += riItem.SensibleHeat;
                        capSum_h += riItem.HeatingCapacity;
                    }
                    if (thisProject.IsCoolingModeEffective &&
                        (capSum_c < ri.RqCoolingCapacity * 0.9 || capSum_sh < ri.RqSensibleHeat * 0.9)) // 室内机与房间的配比率放宽10%，允许[90%～100%]
                    {
                        row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                        isIndoorOK = false;
                    }
                    //else if (thisProject.IsHeatingModeEffective && capSum_h < ri.RqHeatingCapacity * 0.9)
                    // 一个Project中可能存在多个productType，所以判断是否需要制热，不仅仅需要判断IsHeatingModeEffective，还需要判断室内机的ProductType。 20160826 by Yunxiao Lin
                    else if (thisProject.IsHeatingModeEffective && !ri.IndoorItem.ProductType.Contains(", CO") && capSum_h < ri.RqHeatingCapacity * 0.9)
                    {
                        row.Cells[Name_Common.TypeImage].Value = Properties.Resources._02_2__active;
                    }

                    #endregion

                }
            }
            #endregion
            SetTabControlImageKey();
        }

        //判断是否满足FreshAir
        private bool SatisfyIsFreshAir(string RoomId)
        {
            bool isTrue = true;
            FreshAirArea area = thisProject.FreshAirAreaList.Find(f => f.Id == RoomId);
            if (area != null)
            {
                List<RoomIndoor> list = thisProject.RoomIndoorList.FindAll(f => f.IsFreshAirArea == true && f.RoomID == RoomId);
                if (list.Count > 0)
                {
                    double freshAir = 0;
                    foreach (RoomIndoor r in list)
                    {
                        freshAir += r.IndoorItem.AirFlow;
                    }
                    if (area.FreshAir > freshAir)
                    {
                        isTrue = false;
                    }
                }

            }
            return isTrue;
        }

        // b、绑定尚未分配给室外机的室内机列表
        /// <summary>
        /// b、绑定尚未分配给室外机的室内机列表
        /// </summary>
        private void BindIndoorAvailable()
        {
            this.dgvIndoorNotAssigned.Rows.Clear();
            foreach (RoomIndoor riItem in thisProject.RoomIndoorList)
            {
                string model = thisProject.BrandCode == "Y" ? riItem.IndoorItem.Model_York : riItem.IndoorItem.Model_Hitachi;
                if (string.IsNullOrEmpty(riItem.SystemID))
                {
                    string DisplayRoom = string.IsNullOrEmpty(riItem.DisplayRoom) ? "" : riItem.DisplayRoom;
                    this.dgvIndoorNotAssigned.Rows.Add(
                        riItem.IndoorName,
                        model,
                        string.IsNullOrEmpty(riItem.RoomID) ? DisplayRoom : riItem.RoomName
                        // riItem.RoomName
                        );
                }
            }
        }

        // d、绑定Outdoor的树控件
        /// <summary>
        /// 绑定Outdoor的树控件
        /// </summary>
        private void BindTreeViewOutdoor()
        {
            //add by axj 20161229 暂存SelectedNode
            TreeNode nodeTemp = null;
            if (tvOutdoor.Nodes.Count > 0)
            {
                //选中室内机的时候，应记录对应的室外机节点  modify by Shen Junjie on 2019/05/29
                nodeTemp = Global.GetTopParentNode(tvOutdoor.SelectedNode);
            }
            tvOutdoor.Nodes.Clear();
            for (int i = thisProject.SystemList.Count - 1; i >= 0; i--)
            {
                //在绑定Outdoor树之前先删除空的系统 add on 20160606 by Yunxiao Lin
                SystemVRF sysItem = thisProject.SystemList[i];
                //if (sysItem == null || sysItem.OutdoorItem == null)
                if (sysItem == null)
                {
                    thisProject.SystemList.RemoveAt(i);
                    continue;
                }
                if (sysItem.GetIndoorCount(thisProject) == 0)
                {
                    thisProject.SystemList.RemoveAt(i);
                    continue;
                }
            }
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(sysItem.Id);
                TreeNode tnOut = tvOutdoor.Nodes.Add(sysItem.Name);
                Global.BindTreeNodeOut(tnOut, sysItem, listRISelected, thisProject);
            }
            if (tvOutdoor.Nodes.Count > 0)
            {
                this.Cursor = Cursors.Default;//add by axj 20161228 修正SelectedNode不能赋值的bug
                //add by axj 20161229 实现撤销过程中SelectNode节点不变
                if (nodeTemp != null)
                {
                    foreach (TreeNode childNode in tvOutdoor.Nodes)
                    {
                        if (childNode.Text == nodeTemp.Text)
                        {
                            tvOutdoor.SelectedNode = childNode;
                            break;
                        }
                    }
                    if (tvOutdoor.SelectedNode == null)
                    {
                        tvOutdoor.SelectedNode = tvOutdoor.Nodes[0];
                    }
                }
                else
                {
                    tvOutdoor.SelectedNode = tvOutdoor.Nodes[0]; // 初始绑定时，设置第一个室外机节点为选中节点
                }
            }
            if (thisProject.SystemList.Count == 0)
            { 
                ClearODUInformation();
            }

            // 若将树节点完全删除则重新绑定tabPage状态
            SetTabControlImageKey();
        }

        /// <summary>
        /// 恢复Outdoor 默认界面
        /// </summary>
        private void ClearODUInformation()
        {
            this.pnlOutdoorInfo.Visible = true;
            this.pnlIndoorInfo.Visible = false;
            this.dgvSystemInfo.Visible = false;
            this.jclblUnitInfo.Text = "-";  
            this.jclblProductTypeValue.Text = "-";
            this.jclblActualRatioValue.Text = "-";
            this.jclblMaxRatioValue.Text = "-";
            this.jclblOutDBCoolValue.Text = "-";
            this.jclblOutDBHeatValue.Text = "-";
            this.jclblOutWBHeatValue.Text = "-";
            this.jclblOutIWCoolValue.Text = "-";
            this.jclblOutIWHeatValue.Text = "-";
            this.jclblOutRHValue.Text = "-";
            this.jclblAvailableCValue.Text = "-"; 
            this.jclblRequiredCValue.Text = "-";  
            this.jclblAvailableHValue.Text = "-";
            this.jclblRequiredHValue.Text = "-";
            this.jclblOutModelValue.Text = "-";
            this.jclblOutdoorTypeValue.Text = "-";
        }
        /// e、当某个系统组织结构改变后，重新绑定指定的系统对应的TreeNode节点
        /// <summary>
        /// e、当某个系统组织结构改变后，重新绑定指定的系统对应的TreeNode节点
        /// </summary>
        /// <param name="systemID"></param>
        public void BindTreeNodeOutdoor(RoomIndoor ri)
        {
            List<string> ERRList;
            List<string> MSGList;
            string systemID = ri.SystemID;
            foreach (TreeNode tnOut in tvOutdoor.Nodes)
            {
                if (tnOut.Name == systemID)
                {
                    SystemVRF sysItem = tnOut.Tag as SystemVRF;
                    if (sysItem == null) continue;

                    List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(systemID);

                    //是否自动模式的判断  add by Shen Junjie on 20170619
                    if (sysItem.IsAuto)
                    {
                        // 根据系统中的室内机自动计算匹配
                        //Global.DoSelectOutdoor(sysItem, listRISelected, thisProject, out ERRList);
                        //室外机选型统一改用新逻辑 Global.DoSelectOutdoorODUFirst 20161112 by Yunxiao Lin
                        Global.DoSelectOutdoorODUFirst(sysItem, listRISelected, thisProject, out ERRList, out MSGList);
                    }
                    else
                    {
                        Global.DoSelectOutdoorManual(sysItem, listRISelected, thisProject, out ERRList);
                    }
                    Global.BindTreeNodeOut(tnOut, sysItem, listRISelected, thisProject);

                    if (sysItem == curSystemItem)
                    {
                        currentPipingSystem = null;
                        BindPipingDrawing(tnOut);
                        DoDrawingWiring(tnOut);
                    }
                    foreach (TreeNode tn in tnOut.Nodes)
                    {
                        if (tn.Name == ri.IndoorNO.ToString())
                            tvOutdoor.SelectedNode = tn;
                    }
                }
            }

            SetTabControlImageKey();
        }

        /// f、当手动改变管长后，重新自动选型，并绑定指定的系统对应的TreeNode节点
        /// <summary>
        /// f、当手动改变管长后，重新自动选型，并绑定指定的系统对应的TreeNode节点
        /// add on 20160401 clh 
        /// </summary>
        /// <param name="systemID"></param>
        public void BindTreeNodeOutdoor(string systemID)
        {
            List<string> ERRList;
            List<string> MSGList;
            //string systemID = ri.SystemID;
            foreach (TreeNode tnOut in tvOutdoor.Nodes)
            {
                if (curSystemItem.OutdoorItem == null)
                    return;

                if (tnOut.Name == systemID)
                {
                    SystemVRF sysItem = tnOut.Tag as SystemVRF;
                    List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(systemID);

                    //是否自动模式的判断  add by Shen Junjie on 20170619
                    if (sysItem != null && sysItem.IsAuto)
                    {
                        // 根据系统中的室内机自动计算匹配
                        SelectOutdoorResult result;
                        //Global.DoSelectOutdoor(sysItem, listRISelected, thisProject, out ERRList);
                       
                        result = Global.DoSelectOutdoorODUFirst(sysItem, listRISelected, thisProject, out ERRList, out MSGList);
                    }
                    Global.BindTreeNodeOut(tnOut, sysItem, listRISelected, thisProject);

                }
            }
        }

        /// 更改Outdoor下拉框后，同步TreeViewOut的选中节点
        /// <summary>
        /// d、更改Outdoor下拉框后，同步TreeViewOut的选中节点
        /// </summary>
        /// <param name="sysID"></param>
        private void SetSelectedOutdoorNode(string sysID)
        {
            foreach (TreeNode tn in tvOutdoor.Nodes)
            {
                if (tn.Name == sysID)
                {
                    tvOutdoor.SelectedNode = tn;
                    tvOutdoor.Focus();
                }
            }
        }

        /// c、绑定 Piping & Wiring 界面中的 Outdoor 下拉框
        /// <summary>
        /// c、绑定 Piping & Wiring 界面中的 Outdoor 下拉框
        /// </summary>
        private void BindCMBOutdoor()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                DataRow dr = dt.NewRow();
                dr["Id"] = sysItem.Id;
                dr["Name"] = sysItem.Name;
                dt.Rows.Add(dr);
            }

            this.jccmbOutdoor.DisplayMember = "Name";
            this.jccmbOutdoor.ValueMember = "Id";
            this.jccmbOutdoor.DataSource = dt;
        }

        /// 更改TreeViewOut的选中节点后，同步Outdoor下拉框的选项
        /// <summary>
        /// 更改TreeViewOut的选中节点后，同步Outdoor下拉框的选项
        /// </summary>
        /// <param name="text"></param>
        private void SetCMBOutdoorText(string sysID)
        {
            this.jccmbOutdoor.SelectedValue = sysID;
        }

        /// 绑定到界面控件 -- 需要先加载打开或创建的 Project 对象
        /// <summary>
        /// 绑定 项目信息 到界面控件 -- 需要先加载打开或创建的 Project 对象
        /// </summary>
        private void BindToControl()
        {
            BindToControl_ProjectBaseInfo();
            BindToControl_ConditionState();
            BindToControl_SelectedUnits();
            //Compatible_IndoorData(); //兼容老的项目 原室外机的高度差 赋值与室内机的高度差 on 20180425 by xyj 
        }

        /// 绑定界面控件的值到当前 thisProject 对象
        /// <summary>
        /// 绑定界面控件的值到当前 thisProject 对象
        /// </summary>
        private void BindToSource_ProjectBaseInfo()
        {
            double ret;
            if (double.TryParse(this.jctxtAltitude.Text, out ret))
            {
                double altitude = Unit.ConvertToSource(ret, UnitType.LENGTH_M, ut_length);
                //yang 临时屏蔽
                thisProject.Altitude = Convert.ToInt32(altitude);
            }
            thisProject.Name = this.jctxtProjName.Text.Trim();
            this.jccmbProductType.SelectedIndex = 0;
            thisProject.ProductType = this.jccmbProductType.SelectedValue.ToString();
            //thisProject.IsHeatingModeEffective = (this.uc_CheckBox_Heating.Checked && !thisProject.ProductType.Contains(", CO"));
            // 一个Project可能存在多个不同的productType，所以这里thisProject.ProductType不能作为判断依据 20160825 by Yunxiao lin
            thisProject.IsHeatingModeEffective = this.uc_CheckBox_Heating.Checked;

            //thisProject.Location = this.jctxtLocation.Text; // thisProject.Location;
            //thisProject.SoldTo = this.jctxtSoldTo.Text;
            //thisProject.ShipTo = this.jctxtShipTo.Text;
            //thisProject.PurchaseOrderNO = this.jctxtPurchaseOrderNo.Text;
            //thisProject.ContractNO = this.jctxtContractNo.Text;
            //thisProject.ProjectRevision = this.jctxtProjectRevision.Text;  //添加项目修订 add on20180312 by xyj
            //thisProject.SalesOffice = this.jctxtSalesOffice.Text;
            //thisProject.SalesEngineer = this.jctxtSalesName.Text;
            //thisProject.SalesYINO = this.jctxtSalesYINo.Text;
            //thisProject.DeliveryRequiredDate = this.timeDeliveryDate.Value;
            //thisProject.OrderDate = this.timeOrderDate.Value;
            //thisProject.Remarks = this.jctxtRemarks.Text;
            //thisProject.Version = MyConfig.Version;  //维护当前版本号   add on 20161130 by Lingjia Qiu
            //thisProject.ProjectUpdateDate = DateTime.Now.ToShortDateString();  //添加项目最后修改时间 on 20180312 by xyj
            bindProjectInfo(_mainRegion, true);

        }

        /// 将当前的控制器布局模式绑定到当前项目的 ControllerLayoutType 属性
        /// <summary>
        /// 将当前的控制器布局模式绑定到当前项目的 ControllerLayoutType 属性
        /// </summary>
        private void BindToSource_ControllerLayoutType()
        {
            ControllerLayoutType value = (ControllerLayoutType)this.jccmbControllerType.SelectedIndex;
            thisProject.ControllerLayoutType = value;
            glProject.ControllerLayoutType = value;
        }

 

        /// 更改的室内机已加入到室外机系统（a、e）
        /// <summary>
        /// 更改的室内机已加入到室外机系统（a、e）
        /// </summary>
        /// <param name="nodeOut"></param>
        private void RefreshSelectedIndoor(RoomIndoor ri)
        {
            BindSelectedIndoorList();
            BindIndoorAvailable();
            BindTreeNodeOutdoor(ri);

            //不解绑，便于修改室内机后可以重新自动选型 delete by Shen Junjie on 2018/3/14
            ////如果室外机重新选型失败，则需要清空原有的RoomIndoor与System的绑定关系 add on 20160803 by Yunxiao Lin
            //if (curSystemItem != null && curSystemItem.OutdoorItem == null)
            //{
            //    foreach (RoomIndoor roomIn in thisProject.RoomIndoorList)
            //    {
            //        if (roomIn.SystemID == curSystemItem.Id)
            //            roomIn.SetToSystemVRF(string.Empty);
            //    }
            //}
            #region 外机超出配额不绘制配线图，配管图，并提示错误  Add on 20160802 LingjiaQiu
            if (curSystemItem != null && curSystemItem.Ratio != 0d)
            {
                if (curSystemItem.SysType == SystemType.OnlyIndoor)
                {
                    // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                    if (curSystemItem.Ratio < 0.5 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        DrawDiagError(); //配线图错误
                        DrawPipError();  //配管图错误
                        return;
                    }

                }
                else
                {
                    // 多新风机或者混连模式，则配比率校验规则有变 (配置率为80%～100%)
                    if (curSystemItem.Ratio < 0.8 || curSystemItem.Ratio > 1)
                    {
                        DrawDiagError();
                        DrawPipError();
                        return;
                    }
                    //混连模式新风制冷容量有30%限制
                    if (curSystemItem.SysType == SystemType.CompositeMode && curSystemItem.RatioFA > 0.3)
                    {
                        DrawDiagError();
                        DrawPipError();
                        return;
                    }
                }

            }

            #endregion

            BindToControl_ReportItems(); // add on 20160321
        }

        /// 仅绑定室内机相关的列表(a、b)
        /// <summary>
        /// 仅绑定室内机相关的列表(a、b)
        /// </summary>
        public void BindIndoorList()
        {
            BindSelectedIndoorList();
            BindIndoorAvailable();
        }


        /// <summary>
        /// 绑定全热交换机列表
        /// </summary>
        public void BindExchangerList()
        {
            BindSelectedExchangerList();
        }


        /// 刷新所有已选的机组信息（a、b、c、d）
        /// <summary>
        /// 刷新所有已选的机组信息（a、b、c、d）
        /// </summary>
        public void BindToControl_SelectedUnits()
        {
           
            //绑定Exchanger 
            BindSelectedExchangerList();
            BindSelectedIndoorList();
            BindIndoorAvailable();           
            BindCMBOutdoor();
            BindTreeViewOutdoor();
            //BindToControl_Controller(); // 此处加载次数太多 20141105
            BindToControl_ReportItems();
            ChangeIndoorNodesColor();
        }

        private void CheckOldSeriesChanged()
        {
            
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.Series == "Residential VRF HP, 3 phase, FSNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini S - FSNME (3 phase)";
                }
                else if (sysItem.Series == "Residential VRF HR, FSXNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini L - FSXNME (HR)";
                }
                else if (sysItem.Series == "Residential VRF HP, FSXNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini L - FSXNME (HP)";
                }
                else if (sysItem.Series == "Residential VRF HP, 1 phase, FSVNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini S - FSVNME (1 phase)";
                }
            }
        }

        /// 绑定Main_Report界面中需要输出的室外机系统记录
        /// <summary>
        /// 绑定Main_Report界面中需要输出的室外机系统记录
        /// </summary>
        private void BindToControl_ReportItems()
        {
            loadModuleExportFlag(); // 绑定Report中各个模块是否输出的标记

            lbRptOutdoorIncluded.Items.Clear();
            lbRptOutdoorExcept.Items.Clear();

            lbRptOutdoorIncluded.DisplayMember = "Name";
            lbRptOutdoorIncluded.ValueMember = "Id";
            lbRptOutdoorExcept.DisplayMember = "Name";
            lbRptOutdoorExcept.ValueMember = "Id";

            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.OutdoorItem == null)
                    continue;

                //string text = sysItem.Name + "[" + sysItem.OutdoorItem.ModelFull + "]";  // changed on 20130911
                string text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "]";
                if (sysItem.IsExportToReport)
                    lbRptOutdoorIncluded.Items.Add(new SystemVRFSimpleObject(sysItem.Id, text));
                else
                    lbRptOutdoorExcept.Items.Add(new SystemVRFSimpleObject(sysItem.Id, text));
            }

            jclblDXFFilePath.Text = SystemSetting.UserSetting.fileSetting.DXFFiles;
            jclblRptFilePath.Text = SystemSetting.UserSetting.fileSetting.reportFiles;

            if (lbRptOutdoorIncluded.Items.Count == thisProject.SystemList.Count)
            {
                uc_CheckBox_RptAllSystem.Checked = true;
            }
            else
            {
                uc_CheckBox_RptAllSystem.Checked = false;
            }

            if (string.IsNullOrEmpty(jclblDXFFilePath.Text))
            {
                jclblDXFFilePath.Text = Msg.GetResourceString("Msg_DXFPath");
            }
            if (string.IsNullOrEmpty(jclblRptFilePath.Text))
            {
                jclblRptFilePath.Text = Msg.GetResourceString("Msg_ReportPath");
            }
        }
        #endregion

        #region Main_Project

        /// 切换界面控件语言
        /// <summary>
        /// 切换界面控件语言
        /// </summary>
        /// <param name="name"> 切换语言菜单的 Name </param>
        private void DoSwitchLanguage(string name)
        {
            string lang = LangType.ENGLISH;
            if (name == tbtnLanguage_zh.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = true;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.CHINESE;
            }
            else if (name == tbtnLanguage_zht.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_zht.Checked = true;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.CHINESE_TRADITIONAL;
            }
            else if (name == tbtnLanguage_en.Name)
            {
                tbtnLanguage_en.Checked = true;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.ENGLISH;
            }
            else if (name == tbtnLanguage_es.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = true;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.SPANISH;
            }
            else if (name == tbtnLanguage_tr.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = true;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.TURKISH;
            }
            else if (name == tbtnLanguage_fr.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = true;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.FRENCH;
            }
            else if (name == tbtnLanguage_de.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_de.Checked = true;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.GERMANY;
            }
            else if (name == tbtnLanguage_it.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = true;
                tbtnLanguage_pt_BR.Checked = false;
                lang = LangType.ITALIAN;
            }
            else if (name == tbtnLanguage_pt_BR.Name)
            {
                tbtnLanguage_en.Checked = false;
                tbtnLanguage_es.Checked = false;
                tbtnLanguage_tr.Checked = false;
                tbtnLanguage_fr.Checked = false;
                tbtnLanguage_zh.Checked = false;
                tbtnLanguage_zht.Checked = false;
                tbtnLanguage_de.Checked = false;
                tbtnLanguage_it.Checked = false;
                tbtnLanguage_pt_BR.Checked = true;
                lang = LangType.BRAZILIAN_PORTUGUESS;
            }
            JCSetLanguage(lang);


            //更新Tooltip文字
            toolTip1.SetToolTip(lblProjAltitude, Msg.GetResourceString("TOOLTIP_TEXT_ALTITUDE"));

            //同步更新界面中动态文字的语言
            NameArray_Indoor arr = new NameArray_Indoor();
            Global.SetDGVHeaderText(ref dgvIndoor, arr.RoomIndoor_HeaderText);

            Global.SetDGVHeaderText(ref dgvExchanger, arr.RoomIndoor_HeaderText);
            Global.SetDGVHeaderText(ref dgvSystemInfo, arr.SysInfo_HeaderText);
            Global.SetDGVHeaderText(ref dgvIndoorNotAssigned, arr.RoomIndoorNotAttached_HeaderText);

            NameArray_Controller arr1 = new NameArray_Controller();
            Global.SetDGVHeaderText(ref dgvMaterialList, arr1.MaterialList_HeaderText);


            if (thisProject != null)
            {
                BindMaterialList(); // 更新Controller部分的组件list的中英文
                if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_S")  //重新绑定EU室内机类型Comb空间及更新内容语言
                {
                    jccmbIndoorType.Visible = true;
                    uc_CheckBox_IsRoomBase.Visible = false;
                    if (jccmbIndoorType.SelectedIndex == 1)
                        isChangeRoomIndex = true;
                    BindCmbIndoorType();
                }
            }

            if (!string.IsNullOrEmpty(_mainRegion))
                initProjectInfo();

            //在切换语言后需要重新绑定内外机参数显示，不然全部变为“-” add on 20160526 by Yunxiao Lin
            if (tvOutdoor.SelectedNode != null)
            {
                TreeNode node = tvOutdoor.SelectedNode;
                switch (node.Level)
                {
                    case 0:
                        if (node.Tag is SystemVRF)
                            BindOutdoorItemInfo(node.Tag as SystemVRF);
                        break;
                    case 1:
                        if (node.Tag is RoomIndoor)
                        {
                            BindIndoorItemInfo(node.Tag as RoomIndoor);
                            //选中室内机显示改系统的信息表
                            BindSysTableInfo(node.Tag as RoomIndoor);
                        }
                        break;
                    case 2:
                        if (node.Tag is RoomIndoor)
                        {
                            BindIndoorItemInfo(node.Tag as RoomIndoor);
                            //选中室内机显示改系统的信息表
                            BindSysTableInfo(node.Tag as RoomIndoor);
                        }
                        break;
                    default:
                        break;
                }
                //控制控制器切换语言信息翻译刷新   add by VinceQiu
                if (tabControl1.SelectedTab == tpgController)
                {
                    BindControllerProductType();   //刷新控制器productType翻译
                    BindToControl_Controller();   //刷新控制器翻译
                }                
            }
        }

        /// 创建新项目，用于切换Region下拉框选项时调用
        /// <summary>
        /// 创建新项目，用于切换Region下拉框选项时调用
        /// </summary>
        /// <param name="regionVRF"></param>
        private void DoCreateNewProject()
        {
            ControlGroupNo = 0;

            glProject.ProjectID = 0;

            thisProject = new Project();
            thisProject.RegionCode = _mainRegion;

            BindToControl();

            thisProject.ProductType = this.jccmbProductType.SelectedValue.ToString(); //须在BindToControl()之后执行，否则会重复绑定ProductType下拉框
            //thisProject.IsHeatingModeEffective = this.uc_CheckBox_Heating.Checked && !thisProject.ProductType.Contains(", CO");
            //一个Project可能存在多个productType，所以不能以thisProject.ProductType作为判断依据 20160825 by Yunxiao Lin
            thisProject.IsHeatingModeEffective = this.uc_CheckBox_Heating.Checked;
            tabControl1.SelectedIndex = 0;
            this.jctxtSalesEngineer1.Text = SystemSetting.UserSetting.defaultSetting.SalesEngineer;
            this.jctxtSalesName.Text = SystemSetting.UserSetting.defaultSetting.SalesName;
            this.jctxtSalesOffice.Text = SystemSetting.UserSetting.defaultSetting.SalesOffice;
            BindToSource_ProjectBaseInfo(); 
            _projectCopy = (Project)_thisProject.DeepClone();
        }
         

        /// 打开现有项目
        /// <summary>
        /// 打开现有项目
        /// </summary>
        private void DoOpenProject()
        {
            //bool isProjectOK;
            //isImportProject = true; 
            //isProjectOK = OpenVRFProject();
            if (OpenVRFProject() == false) return;
            //打开已保存项目时，如果选择取消按钮，则退出打开操作
            if (CDF.CDF.OutputProjectID == 0)
                return;

            //if (!isProjectOK)
            //{
            //    JCMsg.ShowInfoOK(Msg.GetResourceString("PEOJECT_REGION_DIFFERENT"));
            //    return;
            //}
            //else if (!isImportProject)
            //    return;
            //if (!isImportProject)
            //    return;

            //将获取的 Project 对象绑定到当前项目
            BindToControl();
            SetTabControlImageKey();// 20140918 挪到第一步，加载项目时，需要根据该状态执行判断


            tabControl1.SelectedIndex = 0;

        }

        /// 从VRF界面打开项目，读取AECWorks中Project.db数据库
        /// <summary>
        /// 从VRF界面打开项目，读取AECWorks中Project.db数据库
        /// </summary>
        private bool OpenVRFProject()
        {
            //if (CDF.CDF.OpenVRFInfo(glProject.ProjectID, glProject.ProductID, glProject.UnitName, Registration.SelectedSubRegion.Code))  //glProject.RegionAEC
            string regionCode = _mainRegion;
            if (regionCode != "")
                regionCode = regionCode + "/" + Registration.SelectedSubRegion.Code;
            //选择项目对话框
            if (CDF.CDF.OpenVRFInfo(glProject.ProjectID, glProject.ProductID, glProject.UnitName, regionCode)) //此处验证应该使用父Region和子Region结合方式，兼容旧项目 20170720 by Yunxiao Lin
            {
                //基本AECworks项目信息
                glProject.ProjectID = CDF.CDF.OutputProjectID; //获取打开项目的ID编号,取决于数据库中的顺序
                glProject.UnitName = CDF.CDF.OutputUnitName;

                return OpenVRFProject(glProject.ProjectID);
            }
            return true;
        }

        /// 从VRF界面打开项目，读取AECWorks中Project.db数据库
        /// <summary>
        /// 从VRF界面打开项目，读取AECWorks中Project.db数据库
        /// </summary>
        private bool OpenVRFProject(int projectId)
        {
            byte[] PorjectBlob = null;              //   Project global class

            //从Project.mdb数据库获取 project 序列化对象
            PorjectBlob = CDF.CDF.GetProjectBlob(projectId, 1, 1);
            if (PorjectBlob == null) return false; 

            // 此次需要补充， 保存global VRF project 数据到全局静态类
            // 1. Readd global project class from Blob1 byte[] 
            MemoryStream stream1 = new MemoryStream(PorjectBlob);
            BinaryFormatter bf = new BinaryFormatter();
            Project importProj;
            //thisProject = (Project)bf.Deserialize(stream1);
            importProj = (Project)bf.Deserialize(stream1);
            stream1.Dispose();
            stream1.Close();

            //获取项目名称  on 20180910 by xyj
            string projectName = CDF.CDF.GetProjectName(projectId, 1);
            //如果当前的项目名称不等于数据中得到的名称 则变更当前项目名称
            if (!string.IsNullOrEmpty(projectName) && importProj.Name != projectName)
            {
                importProj.Name = projectName;
            }

            //if (thisProject.RegionCode != _mainRegion)
            //{
            //    JCMsg.ShowInfoOK(Msg.GetResourceString("PEOJECT_REGION_DIFFERENT"));
            //    return;
            //}
            if (importProj.SubRegionCode != thisProject.SubRegionCode) //这里应该限制子Region 20170720 by Yunxiao Lin
            {
                JCMsg.ShowInfoOK(Msg.GetResourceString("PEOJECT_REGION_DIFFERENT") + "[" + importProj.SubRegionCode + "]");
                return false;
            }
            else if (importProj.BrandCode != thisProject.BrandCode)   //添加品牌限制
            {
                JCMsg.ShowInfoOK(Msg.GetResourceString("PEOJECT_BRAND_DIFFERENT"));
                return false;
            }
            thisProject = importProj;

            ControlGroupNo = 0;

            // added on 12june 2019
               CheckOldSeriesChanged();
            //--------------------

            //兼容没有Total Exchanger的老项目
            if (thisProject.ExchangerList == null)
            {
                thisProject.ExchangerList = new List<RoomIndoor>();
            }

            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_S")   //EU选择室内机带房间与否有特殊标准
            {
                //有房间分配的，默认选中基于房间选型               
                if (isWithRoom())
                    jccmbIndoorType.SelectedIndex = 1;
                else
                    jccmbIndoorType.SelectedIndex = 0;
            }
            else
            {
                //有房间分配的，勾选基于房间选型
                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    if (!string.IsNullOrEmpty(ri.RoomID) && !uc_CheckBox_IsRoomBase.Checked)
                    {
                        uc_CheckBox_IsRoomBase.Checked = true;
                        break;
                    }
                    else
                    {
                        if (uc_CheckBox_IsRoomBase.Checked)
                            uc_CheckBox_IsRoomBase.Checked = false;
                    }
                }
            }
            

            //将老项目中Indoor对象里的ListAccessory挪到RoomIndoor对象 add by Shen Junjie 2018/4/27
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.IndoorItem != null)
                {
                    List<Accessory> ListAccessory = ri.IndoorItem.ListAccessory;
                    if (ListAccessory != null && ListAccessory.Count!=0)
                    {     
                            ri.ListAccessory.Union(ListAccessory);                       
                    }
                }
            }

            if (!ProjectBLL.IsSupportedUniversalSelection(thisProject))
            {
                //A_SA部分旧HAPQ室内机UnitType变更，需要对旧项目做兼容处理。20190122 by Yunxiao Lin
                compatSAOldIDUType();
                //将ANZ的项目父region改为OCEANIA,因为之后可能还有其他操作需要读取该父region，所以需要先执行。 20170612 by Yunxiao Lin
                compatOldProject2230();
                //修改TW FSNS系列的Type名称 20181212 by Yunxiao Lin
                compatTWFSNSType();
                //修改巴西FSNS7B/5B系列的Type名称和Series名称 20190110 by Yunxiao Lin
                compatBrazilFSNSTypeAndSeries();
                //导入项目后，需要做兼容性处理 20160707 Yunxiao Lin
                compatOldProject2120();
                //旧项目兼容V1.3.0处理 20161020 by Yunxiao Lin
                compatOldProject2130();
                //LA_MMA旧项目中RPI-8FSNQH和RPI-10FSNQH改为RPI-8.0FSNQH和RPI-10.0FSNQH 20190318 by Yunxiao Lin
                compatOldProject_RPI_8_10_FSNQH();

                //兼容性处理 判断系统功率 add by axj 20160125
                if (thisProject != null && thisProject.SystemList != null)
                {
                    foreach (var sys in thisProject.SystemList)
                    {
                        if (sys.Power == null || sys.Power == "")
                        {
                            if (sys.OutdoorItem != null)
                            {
                                sys.Power = sys.OutdoorItem.ModelFull.Substring(10, 1);
                            }
                        }
                    }
                }
            }
            if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
            {
                compatOldProjectODU();
            }

                //室外机的系列 为了Universal自动重新选型 需要Series  add by Shen Junjie on 2018/3/29
                foreach (var sys in thisProject.SystemList)
            {
                if (sys.OutdoorItem != null)
                {
                    sys.Series = sys.OutdoorItem.Series; 
                }
                //打开项目 重新赋值CHbox 的高度差限制 on 20180626 by xyj 
                sys.NormalCHBoxHighDiffLength = 15;
                sys.NormalCHBox_IndoorHighDiffLength = 15;
                sys.NormalSameCHBoxHighDiffLength = 4;
            }
            CorrectedControlGroup(); //验证ControllerGroup
            //打开旧项目 需要验证下CentralController on 20190311 by xyj
            if (thisProject.CentralControllerOK)
            {
                re_LayoutControllerPage();
            }
            //LockOldProject();

            //打开项目时将Piping户型图分离出Project对象，防止克隆Project时内存不足 add on 20171024 by Shen Junjie 
            PipingBLL.SeparateAllBuildingImage(thisProject, true);

            //避免Piping图数据不正确，要求老项目需要重新验证Piping
            if (thisProject.SystemList.Count > 0 && CompareVersion(thisProject.Version, MyConfig.Version) < 0)
            {
                JCMsg.ShowInfoOK(Msg.GetResourceString("INFO_OLD_VERSION_PIPING_VERIFICATION"));
                foreach (SystemVRF sysItem in thisProject.SystemList)
                {
                    sysItem.IsPipingOK = false;
                }
            }

            //拷贝_thisProject对象
            _projectCopy = (Project)_thisProject.DeepClone();
            return true;
        }

        /// <summary>
        /// 打开旧项目需要修正ODU 数据（EU,TW） on 20190318 by xyj 
        /// </summary>
        private void compatOldProjectODU()
        {
            //欧洲区域 ODU 数据需要读取最新的数据信息 on 20190315 by xyj 
            foreach (SystemVRF sys in thisProject.SystemList)
            {
                if (IsReadODUdata(sys))
                {
                    Outdoor outItem = sys.OutdoorItem.DeepClone();
                    sys.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItemBySeries(sys.OutdoorItem.ModelFull, sys.OutdoorItem.Series);
                    if (sys.OutdoorItem == null)
                    {
                        sys.OutdoorItem = outItem;
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否需要重新读取ODU 数据 on 20190318 by xyj 
        /// </summary>
        /// <returns></returns>
        private bool IsReadODUdata(SystemVRF sys)
        {
            //需要修正ODU EER,COP,SEER,SCOP,SOUNDPOWER CSPF
            if ((thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E") && (sys.OutdoorItem.SEER == 0 || sys.OutdoorItem.COP == 0 || sys.OutdoorItem.SCOP == 0 || sys.OutdoorItem.SoundPower == 0))
            {
                return true;
            }
            else if (thisProject.SubRegionCode == "TW" && sys.OutdoorItem.CSPF == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 验证ControllerGroup 是否存在于ControlGroupList,如果不存在修正数据  on 20190314 by xyj
        /// </summary>
        private void CorrectedControlGroup()
        {
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (!string.IsNullOrEmpty(sysItem.ControlGroupID[0]))
                {
                    //需要判断当前系统的ControlGroupID 是否存在于ControlGroupList
                    if (!ExistControllerGroup(sysItem.ControlGroupID[0]))
                    {
                        //删除当前ControlGroupID
                        sysItem.BindToControlGroup("");
                    }
                }
            }

            foreach (RoomIndoor ri in thisProject.ExchangerList)
            {
                if (!string.IsNullOrEmpty(ri.ControlGroupID[0]))
                {
                    //需要判断当前ControlGroupID 是否存在于ControlGroupList
                    if (!ExistControllerGroup(ri.ControlGroupID[0]))
                    {
                        //删除当前ControlGroupID
                        ri.ControlGroupID[0] = "";
                    }
                }
            }
            foreach (Controller control in thisProject.ControllerList)
            {
                if (!string.IsNullOrEmpty(control.ControlGroupID))
                {
                    //需要判断当前ControlGroupID 是否存在于ControlGroupList
                    if (!ExistControllerGroup(control.ControlGroupID))
                    {
                        //删除当前ControlGroupID
                        control.AddToControlGroup("");
                    }
                }
            }
        }


        /// 导入旧项目时，更新旧项目到1.2.0 add on 20160707 by Yunxiao Lin
        /// <summary>
        /// 导入旧项目时，更新旧项目到1.2.0
        /// </summary>
        private void compatOldProject2120()
        {

            if (thisProject != null && thisProject.RoomIndoorList != null)
            {
                foreach (RoomIndoor roomItem in thisProject.RoomIndoorList)
                {
                    if (roomItem != null && roomItem.IndoorItem != null && roomItem.IndoorItem.Type.Trim() == "High Wall")
                    {
                        string model = roomItem.IndoorItem.ModelFull;
                        if (string.IsNullOrEmpty(model))
                        {
                            model = "";
                        }
                        model = model.Trim();
                        if (model.Length > 2)
                        {
                            string strCode = model.Substring(model.Length - 1, 1);
                            if (strCode == "B")
                                roomItem.IndoorItem.Type = "High Wall (w/o EXV)";
                        }
                    }
                    AccessoryBLL.CompatType(roomItem);
                }
            }
        }

        private int CompareVersion(string version, string str1)
        {
            if (string.IsNullOrEmpty(version))
            {
                return -1;
            }
            //对版本进行处理，去掉尾部的测试版本符号
            string lastch = version.Substring(version.Length - 1, 1);
            while (string.CompareOrdinal(lastch, "0") < 0 || string.CompareOrdinal(lastch, "9") > 0)
            {
                version = version.Remove(version.Length - 1);
                lastch = version.Substring(version.Length - 1, 1);
            }

            //对版本进行处理，去掉尾部的测试版本符号
            lastch = str1.Substring(str1.Length - 1, 1);
            while (string.CompareOrdinal(lastch, "0") < 0 || string.CompareOrdinal(lastch, "9") > 0)
            {
                str1 = str1.Remove(str1.Length - 1);
                lastch = str1.Substring(str1.Length - 1, 1);
            }

            return string.CompareOrdinal(version, str1);
        }
        /// <summary>
        /// 打开旧项目，将LA_MMA中的RPI-8FSNQH和RPI-10FSNQH改为RPI-8.0FSNQH和RPI-10.0FSNQH (V4.5.0)
        /// </summary>
        private void compatOldProject_RPI_8_10_FSNQH()
        {
            if (thisProject == null || thisProject.RoomIndoorList == null)
                return;
            if (CompareVersion(thisProject.Version, "4.5.0") < 0) //V4.5.0
            {
                if (thisProject.SubRegionCode == "LA_MMA")
                {
                    foreach (RoomIndoor roomItem in thisProject.RoomIndoorList)
                    {
                        if (roomItem != null && roomItem.IndoorItem != null)
                        {
                            switch (roomItem.IndoorItem.Model_Hitachi)
                            {
                                case "RPI-8FSNQH":
                                    roomItem.IndoorItem.Model_Hitachi = "RPI-8.0FSNQH"; break;
                                case "RPI-10FSNQH":
                                    roomItem.IndoorItem.Model_Hitachi = "RPI-10.0FSNQH"; break;
                                default: break;
                            }
                            switch (roomItem.IndoorItem.Model)
                            {
                                case "RPI-8FSNQH":
                                    roomItem.IndoorItem.Model = "RPI-8.0FSNQH"; break;
                                case "RPI-10FSNQH":
                                    roomItem.IndoorItem.Model = "RPI-10.0FSNQH"; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }

        /// 打开旧项目时，更新旧项目到1.3.0 add on 20161020 by Yunxiao Lin
        /// <summary>
        /// 打开旧项目时，更新旧项目到1.3.0
        /// </summary>
        /// 
        //bool isImportProject;
        private void compatOldProject2130()
        {
            if (thisProject == null || thisProject.SystemList == null) return;

            //兼容没有控制器的老项目默认值为true
            thisProject.CentralControllerOK = true;

            if (CompareVersion(thisProject.Version, "1.3.0") < 0)
            {
                for (int i = thisProject.SystemList.Count - 1; i >= 0; i--)
                {
                    SystemVRF sys = thisProject.SystemList[i];
                    if (sys.OutdoorItem == null) continue;
                    string model = sys.OutdoorItem.ModelFull;
                    if (string.IsNullOrEmpty(model) || model.Length < 4) continue;
                    string factoryCode = model.Substring(model.Length - 1, 1);
                    string modelCode = model.Substring(0, 4);
                    if (factoryCode == "S" && (modelCode == "JUOH" || modelCode == "JVOH")) //FSXN(JVOH)、FSXNH(JUOH)型号的第4位从"H"变为"U"
                    {
                        model = model.Substring(0, 3) + "U" + model.Substring(4, model.Length - 4);
                        sys.OutdoorItem.ModelFull = model;
                        if (modelCode == "JVOH") //FSXN的Partload表名从ODU_PartLoad_S02_RAS_FSXN1改为ODU_PartLoad_S04_RAS_FSXN 20161021 by Yunxiao Lin
                            sys.OutdoorItem.PartLoadTableName = "ODU_PartLoad_S04_RAS_FSXN";
                    }
                }
                for (int j = thisProject.RoomIndoorList.Count - 1; j >= 0; j--)
                {
                    RoomIndoor ri = thisProject.RoomIndoorList[j]; 
                    //如果版本小于1.3.0，Indoor需要设置ActualValue,并增加SHF系数 20161121 by Yunxiao Lin
                    ri.ActualCoolingCapacity = 0;
                    ri.ActualHeatingCapacity = 0;
                    ri.ActualSensibleHeat = 0;
                    ri.FanSpeedLevel = -1;//ri.SHF_Mode = "High"; 
                }
            }

            //导入老项目会影响选型，做出提示
            //add by Shen Junjie on 2018/2/11
            if (CompareVersion(thisProject.Version, "2.0.0") < 0)
            {
                JCMsg.ShowWarningOK(Msg.UNMAINTAINABLE_PROJECT_WARNING);
            }
            //if (JCMsg.ShowConfirmOKCancel(Msg.GetResourceString("PEOJECT_IMPORT_MSG")) == DialogResult.Cancel)
            //{
            //    isImportProject = false;
            //    return;
            //}
            //else
            //    isImportProject = true;

            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            IndoorBLL indbll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            //ProjectBLL projBll = new ProjectBLL(thisProject);
            Outdoor outdoor = null;
            Indoor indoor = null;
            //数据库中已经不存在的室外机和室内机的提示内容 add by Shen Junjie on 2018/2/13
            List<string> unmaintainableOutdoorsAndIndoors = new List<string>();
            for (int i = thisProject.SystemList.Count - 1; i >= 0; i--)
            {
                SystemVRF sys = thisProject.SystemList[i];
                if (sys.OutdoorItem == null) continue;
                outdoor = bll.GetOutdoorItem(sys.OutdoorItem.ModelFull, sys.OutdoorItem.ProductType);

                //如果没有此型号的室外机，或者此型号为无效状态，则变更为属性相同的有效型号
                if (outdoor == null)
                {
                    outdoor = bll.GetOutdoorItem(sys.OutdoorItem.ProductType, sys.OutdoorItem.Series, sys.OutdoorItem.Model_Hitachi);
                    if (outdoor != null)
                    {
                        sys.OutdoorItem = outdoor;
                        sys.Power = outdoor.ModelFull.Substring(10, 1);
                    }
                }

                if (outdoor == null)
                {
                    //室外机型号不存在，则提示 add by Shen Junjie on 2018/2/11
                    sys.Unmaintainable = true;
                    string sysModel = sys.OutdoorItem.Model_York;
                    if (string.IsNullOrEmpty(sysModel) || sysModel.Trim() == "-")
                    {
                        sysModel = sys.OutdoorItem.Model_Hitachi;
                    }
                    unmaintainableOutdoorsAndIndoors.Add(string.Format("{0}[{1}]", sys.Name, sysModel));
                    continue;
                }
                sys.Unmaintainable = false;
                //增加Series 20161028 by Yunxiao Lin
                sys.OutdoorItem.Series = outdoor.Series;
                //增加室外机马力值 20161121 by Yunxiao Lin
                sys.OutdoorItem.Horsepower = outdoor.Horsepower;

                for (int j = thisProject.RoomIndoorList.Count - 1; j >= 0; j--)
                {
                    RoomIndoor ri = thisProject.RoomIndoorList[j];
                    if (ri == null || ri.IndoorItem == null || ri.SystemID != sys.Id) continue;

                    indoor = indbll.GetItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                    if (indoor == null)
                    {
                        //室内机型号不存在，则锁定系统 add by Shen Junjie on 2018/2/11
                        sys.Unmaintainable = true;
                        string indModel = ri.IndoorItem.Model_York;
                        if (string.IsNullOrEmpty(indModel) || indModel.Trim() == "-")
                        {
                            indModel = ri.IndoorItem.Model_Hitachi;
                        }
                        unmaintainableOutdoorsAndIndoors.Add(string.Format("{0}[{1}]", ri.IndoorName, indModel));
                        continue;
                    }

                    if (ri.IndoorItem.Type != "Fresh Air")
                    {
                        //indoor.ListAccessory = ri.IndoorItem.ListAccessory;  //防止室内机的accessory丢失 add by Shen Junjie on 2018/4/9
                        ri.SetIndoorItem(indoor); //维持原来的Accessory，所以只要替换IndoorItem add by Shen Junjie on 2018/4/28
                        ri.IndoorItem.Series = outdoor.Series; 
                    }
                    //else
                    //{
                    //    //如果查不到相应型号的室内机或新风机，删除室内机 20161121 by Yunxiao Lin
                    //    projBll.RemoveRoomIndoor(ri.IndoorNO);
                    //    continue;
                    //}
                }
            }
            if (unmaintainableOutdoorsAndIndoors.Count > 0)
            {
                JCMsg.ShowWarningOK(Msg.UNMAINTAINABLE_ODU_IDU_WARNING(
                    string.Join(",", unmaintainableOutdoorsAndIndoors.ToArray())));
            }            
        }

        /// 打开旧项目时，更新旧项目到2.3.0 add on 20170612 by Yunxiao Lin
        /// <summary>
        /// 打开旧项目时，更新旧项目到2.3.0
        /// </summary>
        /// 
        private void compatOldProject2230()
        {
            //ANZ的父region从2.3.0开始改为OCEANIA
            string region = thisProject.RegionCode;
            string sub_region = thisProject.SubRegionCode;
            if (region == "ASEAN" && sub_region == "ANZ")
            {
                thisProject.RegionCode = "OCEANIA";
            }
        }
        /// <summary>
        /// 对4.2.1及以前版本中A_SA HAPQ 部分IDU的unit type进行兼容处理
        /// </summary>
        private void compatSAOldIDUType()
        {
            if (thisProject == null || thisProject.RoomIndoorList == null) return;
            if (thisProject.SubRegionCode == "A_SA" && CompareVersion(thisProject.Version, "4.2.1") < 0)
            {
                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    Indoor indoor = ri.IndoorItem;
                    if (indoor != null && indoor.Series != "Commercial VRF CO, CNCQ")
                    {
                        if (indoor.Type == "Compact Ducted" && indoor.Model_Hitachi.EndsWith("FSN1Q"))
                            indoor.Type = "Compact Ducted (FSN1Q)";
                        else if (indoor.Type == "High Static Ducted" && indoor.Model_Hitachi.EndsWith("FSNQH"))
                            indoor.Type = "High Static Ducted (FSNQH)";
                        else if (indoor.Type == "Medium Static Ducted" && indoor.Model_Hitachi.EndsWith("FSNQH"))
                            indoor.Type = "Medium Static Ducted (FSNQH)";
                        else if (indoor.Type == "Low Static Ducted" && indoor.Model_Hitachi.EndsWith("FSNQL"))
                            indoor.Type = "Low Static Ducted (FSNQL)";
                        else if (indoor.Type == "Four Way Cassette" && indoor.Model_Hitachi.EndsWith("FSN1Q"))
                            indoor.Type = "Four Way Cassette (FSN1Q)";
                    }
                }
            }
        }
        /// <summary>
        /// 对V4.1.0及之前版本中的TW FSNS系列的UnitType及系列名称做兼容处理 20181212 by Yunxiao Lin
        /// </summary>
        private void compatTWFSNSType()
        {
            if (thisProject == null || thisProject.SystemList == null || thisProject.RoomIndoorList == null) return;

            if (thisProject.SubRegionCode == "TW" && CompareVersion(thisProject.Version, "4.2.0") < 0)
            {
                foreach (SystemVRF sysItem in thisProject.SystemList)
                {
                    Outdoor outdoor = sysItem.OutdoorItem;
                    if (outdoor != null && outdoor.Type == "FSNS (Top discharge)")
                    {
                        sysItem.SelOutdoorType = outdoor.Type = "FSNS(TW) (Top discharge)";
                        sysItem.Series = outdoor.Series = "Commercial VRF HP, FSNS(TW)";
                    }
                }

                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    Indoor indoor = ri.IndoorItem;
                    if (indoor != null && indoor.Series == "Commercial VRF HP, FSNS")
                    {
                        indoor.Series = "Commercial VRF HP, FSNS(TW)";
                    }
                }
            }
        }

        /// <summary>
        /// 对V4.2.0及之前版本中的TW FSNS7B/5B系列的UnitType及系列名称做兼容处理 20181212 by Yunxiao Lin
        /// </summary>
        private void compatBrazilFSNSTypeAndSeries()
        {
            if (thisProject == null || thisProject.SystemList == null || thisProject.RoomIndoorList == null) return;

            if (thisProject.SubRegionCode == "LA_BR")
            {
                if (CompareVersion(thisProject.Version, "4.2.1") < 0)
                {
                    #region
                    if (thisProject.SystemList != null)
                    {
                        foreach (SystemVRF sysItem in thisProject.SystemList)
                        {
                            if (sysItem != null)
                            {
                                Outdoor outdoor = sysItem.OutdoorItem;
                                if (outdoor != null)
                                {
                                    if (outdoor.Type == "FSNS7B (Top discharge)")
                                        sysItem.SelOutdoorType = outdoor.Type = "Comm. 380V VRF HP, FSNS7B";
                                    if (outdoor.Type == "FSNS5B (Top discharge)")
                                        sysItem.SelOutdoorType = outdoor.Type = "Comm. 220V VRF HP, FSNS5B";
                                    if (outdoor.Series == "Commercial VRF HP, FSNS7B")
                                        sysItem.Series = outdoor.Series = "Comm. 380V VRF HP, FSNS7B";
                                    if (outdoor.Series == "Commercial VRF HP, FSNS5B")
                                        sysItem.Series = outdoor.Series = "Comm. 220V VRF HP, FSNS5B";
                                    if (outdoor.Series == "Commercial VRF CO, FSNS7B")
                                        sysItem.Series = outdoor.Series = "Comm. 380V VRF CO, FSNS7B";
                                    if (outdoor.Series == "Commercial VRF CO, FSNS5B")
                                        sysItem.Series = outdoor.Series = "Comm. 220V VRF CO, FSNS5B";
                                }
                            }
                        }
                    }
                    #endregion
                    #region
                    if (thisProject.RoomIndoorList != null)
                    {
                        foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                        {
                            if (ri.IndoorItem != null)
                            {
                                if (ri.IndoorItem.Series == "Commercial VRF HP, FSNS7B")
                                    ri.IndoorItem.Series = "Comm. 380V VRF HP, FSNS7B";
                                if (ri.IndoorItem.Series == "Commercial VRF HP, FSNS5B")
                                    ri.IndoorItem.Series = "Comm. 220V VRF HP, FSNS5B";
                                if (ri.IndoorItem.Series == "Commercial VRF CO, FSNS7B")
                                    ri.IndoorItem.Series = "Comm. 380V VRF CO, FSNS7B";
                                if (ri.IndoorItem.Series == "Commercial VRF CO, FSNS5B")
                                    ri.IndoorItem.Series = "Comm. 220V VRF CO, FSNS5B";
                            }
                        }
                    }
                    #endregion
                }
                #region
                // old data FSNS5B (54-72HP) Calibration data on 20190619 by xyj
                if (thisProject.SystemList != null)
                {
                    foreach (SystemVRF sysItem in thisProject.SystemList)
                    {
                        if (sysItem.OutdoorItem.Series == "Comm. 220V VRF HP, FSNS5B" && sysItem.OutdoorItem.AuxModelName.Contains("FSNS7B"))
                        {
                            sysItem.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItemBySeries(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.Series);
                        }
                    }
                }
                #endregion
            }
        }

        //目前没有带参数启动程序 deleted by Shen Junjie on 2018/2/7 
        ///// 从AECworks主界面打开项目
        ///// <summary>
        ///// 从AECworks主界面打开项目
        ///// </summary>
        //private void OpenaAECworksProject()
        //{
        //    ControlGroupNo = 0;

        //    //定义初始blob数组
        //    byte[] PorjectBlob = null;              //   Project global class

        //    //获取project类信息
        //    PorjectBlob = CDF.CDF.GetProjectBlob(glProject.ProjectID, 1, 1);
        //    if (PorjectBlob != null)
        //    {
        //        // 此次需要补充， 保存global VRF project 数据到全局静态类
        //        // 1. Readd global project class from Blob1 byte[] 
        //        MemoryStream stream1 = new MemoryStream(PorjectBlob);
        //        BinaryFormatter bf = new BinaryFormatter();
        //        thisProject = (Project)bf.Deserialize(stream1);
        //        stream1.Dispose();
        //        stream1.Close();
        //    }

        //}

        /// Save base info. of "thisProject" to AECworks
        /// <summary>
        /// Save base info. of "thisProject" to AECworks
        /// </summary>
        private void DoSaveAECworkProjectInfo()
        {
            // Project 类信息，项目信息和AECworks同步更新
            CDF.CDF.InputProjectInfo.Name = thisProject.Name;
            CDF.CDF.InputProjectInfo.Version = ConfigurationManager.AppSettings["Version"].ToString();
            CDF.CDF.InputProjectInfo.DBVer = ConfigurationManager.AppSettings["DBVersion"].ToString();
            //CDF.CDF.InputProjectInfo.Measure =;
            CDF.CDF.InputProjectInfo.Location = thisProject.Location;
            CDF.CDF.InputProjectInfo.SoldTo = thisProject.SoldTo;
            CDF.CDF.InputProjectInfo.ShipTo = thisProject.ShipTo;
            CDF.CDF.InputProjectInfo.OrderNo = thisProject.PurchaseOrderNO;
            CDF.CDF.InputProjectInfo.ContractNo = thisProject.ContractNO;
            //CDF.CDF.InputProjectInfo.Region = Registration.SelectedSubRegion.Code;
            CDF.CDF.InputProjectInfo.Region = thisProject.SubRegionCode;   //改成当前系统的region，解决PM登录导入其他子区项目，保存无法更新正确区域问题
            CDF.CDF.InputProjectInfo.Office = thisProject.SalesOffice;
            CDF.CDF.InputProjectInfo.Engineer = thisProject.SalesEngineer;
            CDF.CDF.InputProjectInfo.YINo = thisProject.SalesYINO;
            CDF.CDF.InputProjectInfo.DeliveryDate = thisProject.DeliveryRequiredDate;
            CDF.CDF.InputProjectInfo.OrderDate = thisProject.OrderDate;
            CDF.CDF.InputProjectInfo.Remarks = thisProject.Remarks;
            CDF.CDF.InputProjectInfo.LastDate = DateTime.Now;
            //CDF.CDF.InputProjectInfo.ProjectType
            //CDF.CDF.InputProjectInfo.Vendor

            //Product 类信息
            CDF.CDF.InputProductInfo.ProjectID = glProject.ProjectID;
            CDF.CDF.InputProductInfo.ProductID = glProject.ProductID;
            //CDF.CDF.InputProductInfo.Name = glProject.Name + "[" + mySubRegion.Region + "]"; //"GlobalVRF"
            //CDF.CDF.InputProductInfo.Measure 
            //CDF.CDF.InputProductInfo.Label 
            //CDF.CDF.InputProductInfo.ProductVer 
            //CDF.CDF.InputProductInfo.DBVer 

            // Unit 类信息
            //CDF.CDF.InputUnitInfo.ProductVer = CDF.CCL.Ver;
            //CDF.CDF.InputUnitInfo.DBVer = CDF.CCL.DBVer;
            //CDF.CDF.InputUnitInfo.MLPDate = DateTime.Today;
            //CDF.CDF.InputUnitInfo.LastDate 
            //CDF.CDF.InputUnitInfo.Measure = 0;   //  Unit:  0:English, 1: Metric
            //CDF.CDF.InputUnitInfo.Label = Registration.SelectedSubRegion.Code; // thisProject.SubRegionCode
            //为兼容以前的项目，Unit中存放的是Region和子Region的组合。 20170720 by Yunxiao Lin
            string regionCode = thisProject.RegionCode + "/" + thisProject.SubRegionCode;
            CDF.CDF.InputUnitInfo.Label = regionCode;
            CDF.CDF.InputUnitInfo.ProjectID = glProject.ProjectID;
            CDF.CDF.InputUnitInfo.Name = glProject.UnitName;
            CDF.CDF.InputRegion = Registration.SelectedSubRegion.Code;// thisProject.SubRegionCode
            CDF.CDF.InputUnitInfo.LastDate = DateTime.Now;
            CDF.CDF.InputUnitInfo.ProductVer = ConfigurationManager.AppSettings["Version"].ToString();
            CDF.CDF.InputUnitInfo.DBVer = ConfigurationManager.AppSettings["DBVersion"].ToString();
        }

        /// 保存Project数据表基本信息
        /// <summary>
        /// 保存Project数据表基本信息
        /// </summary>
        private void DoSaveSystemtoUnitDataBase()
        {
            // Project table blob: ProjectBlob,SystemBlob,SQBlob
            // Product table blob: ProductBolb,FanBlob,SystemBlob,FolderBlob
            // Unit Table Blob:  UnitBlob,SQBlob,PriceBlob,SystemBlob,ProjectBlob,FolderBlob
            // VRF System:  1:XML tree, 2: Piping xml, 3: piping jpg, 4: Wring xml, 5: Wring jpg, 6: control xml, 7: control jpg

            DoSaveAECworkProjectInfo();
            // 保存Controller图片文件到数据库

            PipingBLL.AttachAllBuildingImage(thisProject); //add on 20171024 by Shen Junjie

            byte[] ProjectBlob = null;                    // Project global class
            byte[] SystemBlob = null;                   // Controller图结果
            // 1. Save global project class to Blob1 byte[]  保存project类-VRF 选型属性
            MemoryStream stream1 = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream1, thisProject);
            ProjectBlob = stream1.ToArray();
            stream1.Dispose();
            stream1.Close();

            PipingBLL.SeparateAllBuildingImage(thisProject, false); //add on 20171024 by Shen Junjie

            // 保存Controller图
            MemoryStream stream2 = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            // 将Controller连接图转换为BMP
            Pen pen = new Pen(Color.FromArgb(48, 48, 48));
            SetEmptyController(false);  //将空白的控件隐藏
            // 当前只支持一个Control System的图像保存，TODO：支持多个待完善
            pnlController_Center.BackColor = Color.White;
            Bitmap bmp = ControlImage.GetPanel(pnlController_Center);
            bf1.Serialize(stream2, bmp);
            SystemBlob = stream2.ToArray();
            stream2.Dispose();
            stream2.Close();
            SetEmptyController(true);   //将空白的控件恢复显示

            //add for test,20130508 clh
            CDF.CDF.UpdateProjectInfo(thisProject.Name, CDF.CDF.InputProjectInfo, ProjectBlob, SystemBlob, null);

            // 更新Product数据表  CDF.CDF.UpdateProjectInfo
            CDF.CDF.UpdateProductInfo(glProject.ProjectID, glProject.ProductID, CDF.CDF.InputProductInfo, null, null, null, null);

            // 更新Unit信息表
            CDF.CDF.UpdateUnitInfo(glProject.ProjectID, glProject.ProductID, CDF.CDF.InputUnitInfo.Name, CDF.CDF.InputUnitInfo,
                null, null, null, null, null, null);

        }

        /// 检验 Main_Project 文本框内容的输入格式
        /// <summary>
        /// 检验 Main_Project 文本框内容的输入格式
        /// </summary>
        /// <returns></returns>
        private bool CheckInputFormat()
        {
            int altitude;
            #region bug 623 amended by HCL 20180927
            if (int.TryParse(this.jctxtAltitude.Text, out altitude))
            {
                if (!JCValidateSingle(this.jctxtAltitude))
                    return false;
            }
            else
            {
                this.jctxtAltitude.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(this.jctxtPurchaseOrderNo1.Text))
            {
                jctxtPurchaseOrderNo1.RequireValidation = true;
                if (!JCValidateSingle(this.jctxtPurchaseOrderNo1))
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(this.jctxtPurchaseOrderNo.Text))
            {
                jctxtPurchaseOrderNo.RequireValidation = true;
                if (!JCValidateSingle(this.jctxtPurchaseOrderNo))
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(this.jctxtContractNo.Text))
            {
                jctxtContractNo.RequireValidation = true;
                if (!JCValidateSingle(this.jctxtContractNo))
                    return false;
            }
            #endregion

            if (!CDF.CCL.ValidateName(this.jctxtProjName.Text, "[" + lblProjName.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtLocation.Text, "[" + lblProjLocation.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtSoldTo.Text, "[" + jclblSoldTo.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtShipTo.Text, "[" + jclblShipTo.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtPurchaseOrderNo.Text, "[" + jclblPurchaseOrderNo.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtContractNo.Text, "[" + lblProjContractNo.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtSalesOffice.Text, "[" + jclblSalesOffice.Text + "]"))
                return false;
            if (!CDF.CCL.ValidateName(this.jctxtProjectRevision.Text, "[" + jclblProjectRevision.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtSalesName.Text, "[" + lblProjSalesName.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtSalesYINo.Text, "[" + jclblSalesYINo.Text + "]"))
                return false;

            if (!CDF.CCL.ValidateName(this.jctxtRemarks.Text, "[" + jclblRemarks.Text + "]"))
                return false;

            return true;
        }

        /// 保存当前项目文件
        /// <summary>
        /// 保存当前项目文件
        /// </summary>
        private bool DoSaveProject()
        {
            // Controller部分将空白的ControlSystem与ControlGroup对象除去
            ProjectBLL bll = new ProjectBLL(thisProject);
            bll.ClearInvalidControllerData();

            if (CheckInputFormat())
            {
                bool isSuccess = false;

                //glProject.RegionAEC = thisProject.SubRegionCode;

                // 1、更新已保存的项目信息
                if (glProject.ProjectID > 0 && glProject.ProductID > 0 && !string.IsNullOrEmpty(glProject.UnitName))//
                {
                    // 保存控件的值到当前 thisProject 对象
                    BindToSource_ProjectBaseInfo();
                    DoSavePipingStructure();

                    // 保存 thisProject 对象数据到 Project.mdb 数据库
                    DoSaveSystemtoUnitDataBase();
                    isSuccess = true;
                }
                else
                {
                    // 2、保存新项目（另存为。。。）
                    isSuccess = DoSaveAsProject();
                }
                return isSuccess;
            }
            else
            {
                tabControl1.SelectedIndex = 0;
                return false;
            }
        }

        /// 另存为。。。当前项目文件
        /// <summary>
        /// 另存为。。。当前项目文件
        /// </summary>
        private bool DoSaveAsProject()
        {
            // Controller部分将空白的ControlSystem与ControlGroup对象除去
            ProjectBLL bll = new ProjectBLL(thisProject);
            bll.ClearInvalidControllerData();

            if (CheckInputFormat())//增加Main_Project界面的文本框输入类型检测
            {
                // 保存控件的值到当前 thisProject 对象
                BindToSource_ProjectBaseInfo();
                DoSavePipingStructure();

                // isSuccess变量，判断保存是否成功
                bool isSuccess = false;

                CDF.CDF.InputProjectInfo.Name = thisProject.Name;

                //glProject.RegionAEC = thisProject.SubRegionCode;

                isSuccess = CDF.CDF.SaveVRF(glProject.ProjectID, glProject.ProductID, glProject.UnitName, Registration.SelectedSubRegion.Code);//

                if (isSuccess)
                {
                    glProject.ProjectID = CDF.CDF.OutputProjectID;
                    //glProject.ProjectName = CDF.CDF.OutputProjectName;
                    glProject.UnitName = CDF.CDF.OutputUnitName;
                    // 需要更新frmmain主要窗体的 项目名称
                    thisProject.Name = CDF.CDF.OutputProjectName;
                    jctxtProjName.Text = thisProject.Name;
                    jctxtProjectUpdateDate.Text = thisProject.ProjectUpdateDate;
                    // 保存VRF所有数据到 Project.mdb 数据库
                    DoSaveSystemtoUnitDataBase();

                }

                return isSuccess;
            }
            else
            {
                tabControl1.SelectedIndex = 0;
                return false;
            }
        }

        /// 导入其他项目，与当前项目文件合并
        /// <summary>
        /// 导入其他项目，与当前项目文件合并
        /// </summary>
        private void DoImportProject()
        {
            int projectId = CDF.CDF.ImportVRFProject("VRF");
            if (projectId <= 0)
            {
                JCMsg.ShowWarningOK(Msg.IMPORT_PROJECT_FAILED);
                return;
            }

            //基本AECworks项目信息
            glProject.ProjectID = projectId;
            List<string> unitNames = CDF.CDF.GetAllUnitName(projectId, glProject.ProductID, 1);
            if (unitNames.Count > 0)
            {
                glProject.UnitName = unitNames[0];
            }
            if (OpenVRFProject(projectId) == false) return;
            //Project importProject;

            ////获取当前项目的Region和产品ID
            //// ImportVRFInfo()
            //if (CDF.CDF.ImportVRFProject(glProject.ProjectID, glProject.ProductID, "System", Registration.SelectedSubRegion.Code))
            //{
            //    //基本AECworks项目信息
            //    glProject.ProjectID = CDF.CDF.OutputProjectID; // 暂不能屏蔽，会导致带房间的导入出错
            //    glProject.UnitName = CDF.CDF.OutputUnitName;

            //    byte[] PorjectBlob = null;                    //   Project global class
            //    //获取project类信息
            //    PorjectBlob = CDF.CDF.GetProjectBlob(glProject.ProjectID, 1, 1);
            //    if (PorjectBlob != null)
            //    {
            //        // 此次需要补充， 保存global VRF project 数据到全局静态类
            //        // 1. Readd global project class from Blob1 byte[] 
            //        MemoryStream stream1 = new MemoryStream(PorjectBlob);
            //        BinaryFormatter bf = new BinaryFormatter();
            //        importProject = (Project)bf.Deserialize(stream1);
            //        stream1.Dispose();
            //        stream1.Close();

            //        // 调用import system/outdoor/indoor等命令
            //        (new ProjectBLL(thisProject)).AddImportProject(ref importProject);

            //    }


            //    //增加打开已保存项目时，如果选择取消按钮，则不运行下面的程序
            //    if (CDF.CDF.OutputProjectID == 0)
            //        return;
            //}

            BindToControl();

            this.pnlController_Center.Controls.Clear();
            tabControl1.SelectedIndex = 0;
        }

        /// 导出当前打开的项目
        /// <summary>
        /// 导出当前打开的项目
        /// </summary>
        private void DoExportProject()
        {
            BindToSource_ProjectBaseInfo();
            bool projectEqual = Util.CompareObject(_projectCopy, _thisProject);
            if (!projectEqual)
            {
                if (!DoSaveProject())
                    return;
                //DialogResult res = JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_SAVEPROJ_EXPORT);
                //if (res == DialogResult.Yes)
                //{
                //if (!DoSaveProject())
                //    return;
                //}
                //else if (res == DialogResult.Cancel)
                //    return;
            }

            CDF.CDF.ExportProject(glProject.ProjectID, "VRF");
        }

        #endregion

        #region Main_Indoor

        // 打开房间管理窗口
        /// <summary>
        /// 房间管理窗口
        /// </summary>
        private void DoManageRoom()
        {
            Project thisProjectTemp = thisProject.DeepClone();
            frmRoomManage f = new frmRoomManage(thisProject);
            DialogResult dialogResult = f.ShowDialog();
            //EU特殊需求：点击OK绑定信息，点击Cancel取消信息绑定
            if (dialogResult == DialogResult.Cancel && (_mainRegion=="EU_E" || _mainRegion == "EU_W" || _mainRegion == "EU_S"))
            {
                thisProject = thisProjectTemp;
            }
            else
            {
                UndoRedoUtil.SaveProjectHistory();
            }
        }

        // 打开室内机选型窗口
        /// <summary>
        /// 打开室内机选型窗口
        /// </summary>
        /// <param name="opName"></param>
        private void DoAddIndoor()
        {
            if (this.uc_CheckBox_IsRoomBase.Checked)
            {
                if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
                {
                    frmAddIndoorUniversal f = new frmAddIndoorUniversal(thisProject);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    this.AddOwnedForm(f);
                    f.ShowDialog();
                }
                else
                {
                    frmAddIndoor f = new frmAddIndoor(thisProject);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    this.AddOwnedForm(f);
                    f.ShowDialog();
                }
                BindIndoorList();
            }
            else
            {
                if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
                {
                    frmAddIndoorSimpleUniversal f = new frmAddIndoorSimpleUniversal(thisProject);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    this.AddOwnedForm(f);
                    f.ShowDialog();
                }
                else
                {
                    frmAddIndoorSimple f = new frmAddIndoorSimple(thisProject);
                    f.StartPosition = FormStartPosition.CenterScreen;
                    this.AddOwnedForm(f);
                    f.ShowDialog();
                }

            }
         
        }

        // 编辑已选的室内机
        /// <summary>
        /// 编辑已选的室内机
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="flag">true 则表示当前房间已选室内机不合适（Error图标） </param>
        private Boolean DoEditIndoor(RoomIndoor ri, Boolean flag)
        {
            #region 手动配管,管长为0则不允许修改室内机   add on 20160825 by Lingjia Qiu
            if (curSystemItem != null)
            {
                if (curSystemItem.IsInputLengthManually)
                {
                    double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    if (len <= 0)
                    {
                        JCMsg.ShowErrorOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));
                        return false;
                    }
                }
            }

            #endregion
            List<RoomIndoor> romIndSaveList = null; 
            if (ri.IsExchanger)
            {
                 romIndSaveList = thisProject.ExchangerList;
                if (romIndSaveList != null)
                {
                    thisProject.ExchangerList = UtilEx.DeepClone(romIndSaveList);
                    //用副本的RoomIndoor对象处理。 add by Shen Junjie 2018/3/13
                    ri = thisProject.ExchangerList.Find(p =>
                    {
                        return p.IndoorNO == ri.IndoorNO;
                    });
                    if (ri == null) return false;
                }
            }
            else
            {
                 romIndSaveList = thisProject.RoomIndoorList;
                if (romIndSaveList != null)
                {
                    thisProject.RoomIndoorList = UtilEx.DeepClone(romIndSaveList);
                    //用副本的RoomIndoor对象处理。 add by Shen Junjie 2018/3/13
                    ri = thisProject.RoomIndoorList.Find(p =>
                    {
                        return p.IndoorNO == ri.IndoorNO;
                    });
                    if (ri == null) return false;
                }
            }

            bool isBaseOfRoom = !string.IsNullOrEmpty(ri.RoomID);
            DialogResult dialogResult = DialogResult.Cancel;
            Form f;
            //判断当前Room 是否是一个空的Room ON20170926 by xyj
            if (isBaseOfRoom && !(new ProjectBLL(thisProject)).isEmptyRoom(ri.RoomID))
            {
                if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
                {
                    f = new frmAddIndoorUniversal(ri, thisProject, flag);
                }
                else
                {
                    f = new frmAddIndoor(ri, thisProject, flag);
                }
            }
            else
            {
                if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
                {
                    f = new frmAddIndoorSimpleUniversal(ri, thisProject);
                }
                else
                {
                    f = new frmAddIndoorSimple(ri, thisProject);
                }
            }
            f.StartPosition = FormStartPosition.CenterScreen;
            this.AddOwnedForm(f);
            dialogResult = f.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                //还原 RoomIndoorList  add by Shen Junjie on 2018/3/13
                if (ri.IsExchanger)
                {
                    thisProject.ExchangerList = romIndSaveList;
                }
                else {
                    thisProject.RoomIndoorList = romIndSaveList;
                }
                return false;
            }


            if (string.IsNullOrEmpty(ri.SystemID))
            {
                BindIndoorList();
            }
            else
            {
                #region   当编辑室内机为新风机则做出相应提示   add on 20160901 by Lingjia Qiu
                if (curSystemItem.SysType == SystemType.CompositeMode ||
                    curSystemItem.SysType == SystemType.OnlyFreshAir ||
                    curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
                {
                    if (ri.IndoorItem.Flag == IndoorType.Indoor)
                    {
                        if (ri.IsExchanger)
                        {
                            thisProject.ExchangerList = romIndSaveList;
                        }
                        else
                        {
                            thisProject.RoomIndoorList = romIndSaveList;
                        }

                       // thisProject.RoomIndoorList = romIndSaveList; 
                        JCMsg.ShowInfoOK(Msg.OUTD_NOTMATCH_COMPOSITE_NoIndoor);
                        return false;
                    }
                }
                //任何模式下都无法替换新风机
                //if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                //{
                //    if (romIndSaveList != null)
                //        thisProject.RoomIndoorList = romIndSaveList;
                //    JCMsg.ShowInfoOK(Msg.OUTD_NOTMATCH_NoFreshAir);
                //    return false;
                //}

                //if (thisProject.RoomIndoorList.Count>0 && !string.IsNullOrEmpty(thisProject.RoomIndoorList[0].SystemID))
                //{
                //    //如果剩余新风机型号“JTAF1080”单独选型找不到适合的室外机，解绑并提示   add on 20160928 by Lingjia Qiu                
                //    if (thisProject.RoomIndoorList.Count == 1 && thisProject.RoomIndoorList[0].IndoorItem.Model.Equals("JTAF1080"))
                //    {
                //        thisProject.RoomIndoorList[0].SetToSystemVRF(null);
                //        JCMsg.ShowInfoOK(Msg.IND_DEL_FRESHAIR_HINT);
                //    }

                //}
                #endregion
            }

            //IsUpdated属性移到这里更改，解决按Cancel以后不能过Piping的问题。 add by Shen Junjie on 2018/3/14
            if (curSystemItem != null)
                curSystemItem.IsUpdated = true;

            RefreshSelectedIndoor(ri);

            foreach (DataGridViewRow r in dgvIndoor.Rows)
            {
                r.Selected = false;
                if (r.Cells[Name_Common.NO].Value.ToString() == ri.IndoorNO.ToString())
                {
                    r.Selected = true;
                    break;
                }
            } 
            return true;
        }


        private List<RoomIndoor> GetSelectedIndoorOrExchangerByRoom(string RoomID)
        {
            List<RoomIndoor> list = new ProjectBLL(thisProject).GetSelectedIndoorByRoom(RoomID);

            if (_mainRegion == "EU_E" || _mainRegion == "EU_W" || _mainRegion == "EU_S")
            {
                List<RoomIndoor> lists = new ProjectBLL(thisProject).GetSelectedExchangerByRoom(RoomID);
                if (lists != null && lists.Count > 0)
                {
                    return list;
                }
            }
            return list;
        }

        // 删除已选择的室内机
        /// <summary>
        /// 删除已选择的室内机
        /// </summary>
        private void DoDeleteIndoor()
        {
            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }
            string shareRindName = "";
            List<RoomIndoor> shareRiList = new List<RoomIndoor>();
            foreach (DataGridViewRow r in this.dgvIndoor.SelectedRows)
            {
                #region
                int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value); 
                if (r.Cells[Name_Common.RoomName].Value != null)
                {
                    ProjectBLL bll = new ProjectBLL(thisProject);
                    // 检查是否可以在此处删除 TODO:考虑改为可以在此直接删除。。20140519 clh
                    //string roomNOString = r.Cells[Name_Common.RoomName].Value.ToString();
                    bool isExchanger = Convert.ToBoolean(r.Cells[Name_Common.IsExchanger].Value);
                    RoomIndoor ri = new RoomIndoor();
                    if (!isExchanger)
                        ri = (new ProjectBLL(thisProject)).GetIndoor(indNo);
                    else
                        ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);

                    if (ri.IsAuto)  // 自动选型可以在此删除
                        continue;
                    //判断房间是否是空房间；空房间可以删除 否则不可以删除提示 on 20170927 by xyj
                    if (!(new ProjectBLL(thisProject)).isEmptyRoom(ri.RoomID))
                    {

                        List<RoomIndoor> list = GetSelectedIndoorOrExchangerByRoom(ri.RoomID);

                        if (list != null && list.Count > 1)
                        {
                            JCMsg.ShowWarningOK(Msg.IND_INFO_DEL_MAIN);
                            return;
                        }
                        if (ri.IndoorItemGroup != null)
                        {
                            shareRindName += ri.IndoorName + ",";
                            shareRiList.Add(ri);
                        }
                    }

                }
                #endregion
            }

            DialogResult res;
            if (!string.IsNullOrEmpty(shareRindName))
                res = JCMsg.ShowConfirmOKCancel(Msg.IND_Delete_Sharing_RelationShip(shareRindName.Substring(0, shareRindName.Length - 1), ""));
            else
                res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL);
            // 可以直接删除
            if (res == DialogResult.OK)
            {
                //存在共享，删除牵连的共享关系
                if (!string.IsNullOrEmpty(shareRindName) && shareRiList.Count != 0)
                {
                    foreach (RoomIndoor shareRi in shareRiList)
                    {
                        deleteShareRelationShip(shareRi, false);
                    }
                }
                List<string> sysIDList = new List<string>();
                ProjectBLL bll = new ProjectBLL(thisProject);
                foreach (DataGridViewRow r in this.dgvIndoor.SelectedRows)
                {
                    #region
                    int indNo = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
                    bool isExchanger = Convert.ToBoolean(r.Cells[Name_Common.IsExchanger].Value);
                    RoomIndoor indItem = new RoomIndoor();
                    if (!isExchanger)
                        indItem = bll.GetIndoor(indNo);
                    else
                        indItem = bll.GetExchanger(indNo);

                    object sysID = r.Cells[Name_Common.SysName].Value;
                    //  RoomIndoor indItem = bll.GetIndoor(indNo);
                    if (sysID != null && !sysID.Equals(""))
                    {
                        //if (!isDoDelIndoor(indNo)) //如果混连模式下，随意删除则会造成室外机系统崩溃，需做出提示  add on 20160909 by Lingjia Qiu
                        //{
                        //    continue;
                        //}

                        //混连模式无法删除
                        if (curSystemItem.SysType == SystemType.CompositeMode)
                        {
                            JCMsg.ShowErrorOK(Msg.IND_DEL_FAIL);
                            BindToControl_SelectedUnits();
                            return;
                        }

                        //删除RoomIndoor对象应该放到System之前 20160818 by Yunxiao Lin
                        if (!isExchanger)
                            bll.RemoveRoomIndoor(indNo);
                        else
                            bll.RemoveExchanger(indNo);

                        //如果剩余新风机型号“JTAF1080”单独选型找不到适合的室外机，解绑并提示   add on 20160928 by Lingjia Qiu
                        //if (thisProject.RoomIndoorList.Count == 1 && thisProject.RoomIndoorList[0].IndoorItem.Model.Equals("JTAF1080"))
                        //{
                        //    thisProject.RoomIndoorList[0].SetToSystemVRF(null);
                        //    JCMsg.ShowInfoOK(Msg.IND_DEL_FRESHAIR_HINT);
                        //}
                        if (sysID.ToString() != "" && !sysIDList.Contains(sysID.ToString()))
                        {
                            sysIDList.Add(sysID.ToString());
                            SystemVRF sysItem = bll.GetSystem(sysID.ToString());
                            sysItem.IsUpdated = true;
                            //如果删除室内机，则需要判断室外机是否为空，如果为空，则需要删除室外机所在系`统
                            if (sysItem.GetIndoorCount(thisProject) == 0)
                            {
                                (new ProjectBLL(thisProject)).RemoveSystemVRF(sysID.ToString());

                            }
                        }
                    }
                    else
                    {
                        if (!isExchanger)
                            bll.RemoveRoomIndoor(indNo);
                        else
                            bll.RemoveExchanger(indNo);
                    }
                    #endregion
                }
                //ToDo: 删除室内机之后，对涉及的室外机系统重新选型。 20160815 Yunxiao Lin

                if (sysIDList.Count == 0)
                    BindIndoorList();
                else
                {
                    BindTreeNodeOutdoor(curSystemItem.Id);
                    BindToControl_SelectedUnits();
                }

                if (thisProject.RoomIndoorList.Count == 0)
                {
                    thisProject.SystemList = new List<SystemVRF>();
                }

                UndoRedoUtil.SaveProjectHistory();
            }
        }

        // 拷贝已选的室内机系统
        /// <summary>
        /// 拷贝已选择的室内机系统
        /// </summary>
        private void DoCopyIndoor()
        {
            List<RoomIndoor> selectedInoorList = new List<RoomIndoor>();

            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }

            //确认框
            if (JCMsg.ShowConfirmOKCancel(JCMsg.COMMON_COPY_CHECK) != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < this.dgvIndoor.SelectedRows.Count; i++)
            {
                DataGridViewRow row = dgvIndoor.SelectedRows[i];
                //string model = row.Cells[Name_Common.ModelFull].Value.ToString();
                string indNo = row.Cells[Name_Common.NO].Value.ToString();
                bool isExchanger = Convert.ToBoolean(row.Cells[Name_Common.IsExchanger].Value);
                if (NumberUtil.IsNumber(indNo))
                {
                    RoomIndoor ri = new RoomIndoor();
                    if (!isExchanger)
                        ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
                    else
                        ri = (new ProjectBLL(thisProject)).GetExchanger(int.Parse(indNo));
                    if (ri != null)
                    {
                        selectedInoorList.Add(ri);
                    }
                }
            }

            //判断是否还存在的该房间的室内机
            if (!checkSelectedAllIndoorsInRoom(selectedInoorList))
            {
                JCMsg.ShowInfoOK(Msg.GetResourceString("IND_EXIST_INDUNIT"));
                return;
            }

            //调用拷贝室内机方法
            ProjectBLL projectBLL = new ProjectBLL(thisProject);
            if (_mainRegion == "EU_E" || _mainRegion == "EU_W" || _mainRegion == "EU_S")
            {
                List<RoomIndoor> IDU_List = selectedInoorList.FindAll(p => p.IsExchanger == false);
                projectBLL.CopyIndoor(IDU_List,
                     SystemSetting.UserSetting.defaultSetting.IndoorName,
                     SystemSetting.UserSetting.defaultSetting.FloorName,
                     SystemSetting.UserSetting.defaultSetting.RoomName,
                     SystemSetting.UserSetting.defaultSetting.FreshAirAreaName, "");

                List<RoomIndoor> Exc_List = selectedInoorList.FindAll(p => p.IsExchanger == true);
                projectBLL.CopyExchanger(Exc_List,
                     SystemSetting.UserSetting.defaultSetting.ExchangerName,
                     SystemSetting.UserSetting.defaultSetting.FloorName,
                     SystemSetting.UserSetting.defaultSetting.RoomName,
                     SystemSetting.UserSetting.defaultSetting.FreshAirAreaName, "");

            }
            else
            {
                projectBLL.CopyIndoor(selectedInoorList,
                   SystemSetting.UserSetting.defaultSetting.IndoorName,
                   SystemSetting.UserSetting.defaultSetting.FloorName,
                   SystemSetting.UserSetting.defaultSetting.RoomName,
                   SystemSetting.UserSetting.defaultSetting.FreshAirAreaName, "");
            }

            //刷新室内机列表
            BindIndoorList();
            UndoRedoUtil.SaveProjectHistory();
        }

        private bool checkSelectedAllIndoorsInRoom(List<RoomIndoor> selectedInoorList)
        {
            List<string> roomIdList = new List<string>();
            for (int m = 0; m < selectedInoorList.Count; m++)
            {
                string roomId = selectedInoorList[m].RoomID;
                //判断是否存在房间
                if (!string.IsNullOrEmpty(roomId))
                {
                    if (!roomIdList.Contains(roomId))
                    {
                        roomIdList.Add(roomId);
                    }
                }
            }

            //判断存在房间的室内机（是否完全拷贝该房间的室内机）
            foreach (string roomId in roomIdList)
            {
                //如果房间类型是空房间 则不提醒 on20170927 by xyj
                if ((new ProjectBLL(thisProject)).isEmptyRoom(roomId))
                {
                    return true;
                }
                int indoorCount = thisProject.RoomIndoorList.Count(ri =>
                {
                    return ri.RoomID == roomId;
                });
                if (_mainRegion == "EU_E" || _mainRegion == "EU_W" || _mainRegion == "EU_S")
                {
                    int exchangerCount = thisProject.ExchangerList.Count(ri =>
                    {
                        return ri.RoomID == roomId;
                    });
                    indoorCount = indoorCount + exchangerCount;
                }

                int selectCount = selectedInoorList.Count(ri =>
                {
                    return ri.RoomID == roomId;
                });

                //判断是否还存在的该房间的室内机
                if (indoorCount > selectCount)
                {
                    return false;
                }
            }
            return true;
        }


        private bool checkSelectedAllExchangerInRoom(List<RoomIndoor> selectedExchangerList)
        {
            List<string> roomIdList = new List<string>();
            for (int m = 0; m < selectedExchangerList.Count; m++)
            {
                string roomId = selectedExchangerList[m].RoomID;
                //判断是否存在房间
                if (!string.IsNullOrEmpty(roomId))
                {
                    if (!roomIdList.Contains(roomId))
                    {
                        roomIdList.Add(roomId);
                    }
                }
            }

            //判断存在房间的室内机（是否完全拷贝该房间的室内机）
            foreach (string roomId in roomIdList)
            {

                //如果房间类型是空房间 则不提醒 on20170927 by xyj
                if ((new ProjectBLL(thisProject)).isEmptyRoom(roomId))
                {
                    return true;
                }

                int indoorCount = thisProject.ExchangerList.Count(ri =>
                {
                    return ri.RoomID == roomId;
                });

                int selectCount = selectedExchangerList.Count(ri =>
                {
                    return ri.RoomID == roomId;
                });

                //判断是否还存在的该房间的室内机
                if (indoorCount > selectCount)
                {
                    return false;
                }
            }
            return true;
        }

        //  验证是否能进行删除室内机
        /// <summary>
        /// 验证是否能进行删除室内机   -- add on 20160909 by Lingjia Qiu
        /// </summary>
        /// <param name="indNo"></param>
        private Boolean isDoDelIndoor(int indNo)
        {
            int indCount = 0;
            int freshAirCount = 0;
            ProjectBLL bll = new ProjectBLL(thisProject);
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.IndoorItem.ProductType == curSystemItem.OutdoorItem.ProductType)
                {
                    if (ri.IndoorItem.Flag == IndoorType.Indoor)
                        indCount++;
                    if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                        freshAirCount++;
                }
            }
            if (curSystemItem.SysType == SystemType.CompositeMode)
            {
                RoomIndoor indItem = bll.GetIndoor(indNo);
                if (indItem.IndoorItem.Flag == IndoorType.Indoor)
                {
                    if (indCount == 1)
                    {
                        JCMsg.ShowErrorOK(Msg.IND_DEL_FAIL);
                        return false;
                    }
                }
                else if (indItem.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (freshAirCount == 1)
                    {
                        JCMsg.ShowErrorOK(Msg.IND_DEL_FAIL);
                        return false;
                    }
                }
            }
            return true;
        }

        // 添加 Options
        /// <summary>
        /// 添加 Options
        /// </summary>
        private void DoAddOptions()
        {
            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }
            if (this.dgvIndoor.Rows.Count > 0 && this.dgvIndoor.SelectedRows.Count > 0)
            {
                //DataGridViewRow row = dgvIndoor.SelectedRows[0];
                //string model = row.Cells[Name_Common.ModelFull].Value.ToString();
                //string indNo = row.Cells[Name_Common.NO].Value.ToString();
                //if (NumberUtil.IsNumber(indNo))
                //{
                //    RoomIndoor ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
                //    frmAddAccessory f = new frmAddAccessory(ri.IndoorItem);
                //    f.ShowDialog();
                //}
                #region   --modify on 20170613 by Lingjia Qiu
                List<RoomIndoor> rinItemList = new List<RoomIndoor>();
                // List<RoomIndoor> riGroupSeletedList = new List<RoomIndoor>();
                List<Indoor> indItemList = new List<Indoor>();
                string reContrIndoorName = "";
                string optionIndoorName = "";
                string groupIndoorName = "";

                //维护已选的记录并检查多选条件限制
                if (!checkMutiSelected(out reContrIndoorName))
                    return;

                foreach (DataGridViewRow r in this.dgvIndoor.SelectedRows)
                {
                    string indNo = r.Cells[Name_Common.NO].Value.ToString();
                    if (NumberUtil.IsNumber(indNo))
                    {
                        RoomIndoor ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));

                        //if (string.IsNullOrEmpty(ri.IndoorItem.IndoorName))
                        //{
                        //    ri.IndoorFullName = thisProject.BrandCode == "Y" ? ri.IndoorName + "[" + ri.IndoorItem.Model_York + "]" : ri.IndoorName + "[" + ri.IndoorItem.Model_Hitachi + "]";  //记录indoor对象的indoor名
                        //    ri.IndoorItem.IndoorName = ri.IndoorFullName;
                        //}

                        rinItemList.Add(ri);
                        //indItemList.Add(ri.IndoorItem);

                        if (dgvIndoor.SelectedRows.Count > 1 && ri.IndoorItemGroup != null)  //多选已共享控制器的室内机维护indoorName
                        {
                            if (ri.IndoorItemGroup.Count != 0)
                            {
                                foreach (RoomIndoor rind in ri.IndoorItemGroup)
                                {
                                    //将已选共享控制器所关联的室内机维护indoorName
                                    if (!reContrIndoorName.Contains(rind.IndoorName) && !groupIndoorName.Contains(r.Cells[Name_Common.Name].Value.ToString()) && !groupIndoorName.Contains(rind.IndoorName))
                                    {
                                        groupIndoorName += rind.IndoorName + ",";
                                    }
                                }

                            }

                        }
                        else if (dgvIndoor.SelectedRows.Count > 1 && ri.ListAccessory != null)  //多选已分配配件的室内机维护indoorName
                        {
                            if (ri.ListAccessory.Count != 0)
                                optionIndoorName += r.Cells[Name_Common.Name].Value.ToString() + ",";
                        }

                    }

                }

                //对List进行排序
                rinItemList.Sort(delegate(RoomIndoor x, RoomIndoor y)
                {
                    return x.IndoorNO.CompareTo(y.IndoorNO);
                });
                //rinItemList.ToList().OrderBy(p => p.IndoorNO);
                //indItemList.Sort(delegate(Indoor x, Indoor y)
                //{
                //    string xIndName = x.IndoorName.Split('[')[0];
                //    string yIndName = y.IndoorName.Split('[')[0];
                //    return xIndName.CompareTo(yIndName);
                //});
                //转写排序，不要在Indoor加IndoorName属性 modified by Shen Junjie 2018/3/21
                indItemList = (from p in rinItemList orderby p.IndoorFullName select p.IndoorItem).ToList();
                //indItemList.ToList().OrderBy(p => p.IndoorName);

                AccessoryBLL bill = new AccessoryBLL();
                DataTable allAvailable = bill.GetAllAvailableSameAccessory(indItemList, thisProject.RegionCode, thisProject.SubRegionCode);
                if (allAvailable == null)
                {
                    JCMsg.ShowInfoOK(Msg.IND_NO_SAME_ACCESSORY);
                    return;
                }
                if (dgvIndoor.SelectedRows.Count > 1)
                {
                    if (!string.IsNullOrEmpty(reContrIndoorName))   //已共享控制器的室内机进行关联关系清空
                    {
                        reContrIndoorName = reContrIndoorName.Substring(0, reContrIndoorName.Length - 1);
                        string allIndoorName = "";
                        if (!string.IsNullOrEmpty(groupIndoorName))
                        {
                            groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
                            allIndoorName = reContrIndoorName + "," + groupIndoorName;   //所有关联室内机
                        }
                        else
                            allIndoorName = reContrIndoorName;



                        //提示并进行YES OR NO处理                       
                        DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_Sharing_RemoteController(reContrIndoorName, groupIndoorName));
                        if (result != DialogResult.OK)
                            return;

                        //对已选共享控制器的室内机及其相关共享的室内机进行关联清空
                        foreach (DataGridViewRow r in this.dgvIndoor.Rows)
                        {
                            string indNo = r.Cells[Name_Common.NO].Value.ToString();
                            string indName = r.Cells[Name_Common.Name].Value.ToString();
                            foreach (string indoorName in allIndoorName.Split(','))   //循环遍历已选共享控制器的室内机及相关共享的室内机名
                            {
                                if (indoorName.Equals(indName))   //如果共享控制器
                                {
                                    RoomIndoor ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
                                    if (ri.IndoorItemGroup != null)
                                    {
                                        if (ri.IndoorItemGroup.Count != 0)
                                            ri.IndoorItemGroup = null;   //清空关联关系
                                        if (ri.IsMainIndoor)
                                            ri.IsMainIndoor = false;   //重置主室内机
                                    }
                                }
                            }
                        }

                    }
                    else if (!string.IsNullOrEmpty(optionIndoorName))   //室内机多选已有配件清空   
                    {
                        optionIndoorName = optionIndoorName.Substring(0, optionIndoorName.Length - 1);
                        DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_ACCESSORY(optionIndoorName));
                        if (result != DialogResult.OK)
                            return;
                    }

                }

                frmAddAccessory f = new frmAddAccessory(rinItemList, thisProject, allAvailable);
                f.ShowDialog();
                #endregion

            }
        }

        // 清空 Options
        /// <summary>
        /// 清空 Options
        /// </summary>
        private void DoClearOptions()
        {

            if (this.dgvIndoor.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }
            if (this.dgvIndoor.Rows.Count > 0 && this.dgvIndoor.SelectedRows.Count > 0)
            {
                DialogResult res = JCMsg.ShowConfirmOKCancel(Msg.IDU_ACCESSORYE_CLEAR);
                // 可以清空配件
                if (res == DialogResult.OK)
                {

                    foreach (DataGridViewRow row in dgvIndoor.SelectedRows)
                    {
                        int indNo = Convert.ToInt32(row.Cells[Name_Common.NO].Value);

                        bool isExchanger = Convert.ToBoolean(row.Cells[Name_Common.IsExchanger].Value);
                        RoomIndoor ri = new RoomIndoor();
                        if (!isExchanger)
                            ri = (new ProjectBLL(thisProject)).GetIndoor(indNo);
                        else
                            ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);

                        RoomIndoor mainInd = new RoomIndoor();
                        //判断当前的RoomIndoor 是否被共享
                        if (RoomIndoorIsShared(ri, out mainInd))
                        {
                            if (mainInd.IndoorNO > 0 && mainInd.IndoorItemGroup != null)
                            { 
                                mainInd.IndoorItemGroup.Remove(ri);
                            } 
                        }

                        //清空共享控制器关联  add on 20170622 by Lingjia Qiu  
                        if (ri.IndoorItemGroup != null && ri.IndoorItemGroup.Count > 0)
                        {
                            //判断当前共享的室内机是否是Wireless Remote Control Switch  add 20180502 by xyj
                            if (ri.ListAccessory.FindAll(p => p.Type == "Wireless Remote Control Switch" && p.IsShared == true).Count > 0)
                            {
                                foreach (RoomIndoor ind in ri.IndoorItemGroup)
                                {
                                    Accessory acitem = ind.ListAccessory.Find(p => p.Type == "Receiver Kit for Wireless Control");
                                    if (acitem != null && acitem.Type != null)
                                        ind.ListAccessory.Remove(acitem);
                                }
                            } 
                            ri.IndoorItemGroup = null;
                            if (ri.IsMainIndoor)
                                ri.IsMainIndoor = false;
                        }
                        ri.IsMainIndoor = false;
                        List<Accessory> list = (new AccessoryBLL()).GetDefault(ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode, ri.IndoorItem.Series);
                        ri.ListAccessory = list;

                    }
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                }
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        
        }

        //判断当前室内机或热交换机是否被共享
        private bool RoomIndoorIsShared(RoomIndoor ri, out RoomIndoor mainIndoor)
        {
            mainIndoor = null;
            List<RoomIndoor> list = new List<RoomIndoor>();
            list = thisProject.RoomIndoorList.FindAll(p => p.IsMainIndoor == true);
            List<RoomIndoor> lists = thisProject.ExchangerList.FindAll(p => p.IsMainIndoor == true);
            if (lists != null && lists.Count > 0)
            {
                foreach (RoomIndoor ritem in lists)
                {
                    list.Add(ritem);
                }
            } 
            foreach (RoomIndoor ind in list)
            {
                if (ind.IndoorItemGroup != null && ind.IndoorItemGroup.Count > 0)
                { 
                    RoomIndoor listInd = ind.IndoorItemGroup.Find(p => p.IndoorNO == ri.IndoorNO && p.IsExchanger == ri.IsExchanger);
                    if (listInd != null)
                    {
                        mainIndoor = ind;
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion


       


        #region Main_Outdoor
        // 绑定树节点中各个节点关联的室内机室外机的信息
        /// <summary>
        /// 绑定树节点中各个节点关联的室内机室外机的信息
        /// </summary>
        /// <param name="node">当前选中的节点</param>
        private void BindTreeViewOutNodeInfo(TreeNode node)
        {
            switch (node.Level)
            {
                case 0:
                    {
                        if (node.Tag is SystemVRF)
                            BindOutdoorItemInfo(node.Tag as SystemVRF);
                    }
                    break;
                case 1:
                case 2:
                    if (node.Tag is RoomIndoor)
                    {
                        BindIndoorItemInfo(node.Tag as RoomIndoor);
                        //选中室内机显示改系统的信息表
                        BindSysTableInfo(node.Tag as RoomIndoor);
                    }
                    break;
                default:
                    break;
            }
        }

        // 选中 Indoor 节点时绑定的信息
        /// <summary>
        /// 选中 Indoor 节点时绑定的信息
        /// </summary>
        /// <param name="riItem"></param>
        private void BindIndoorItemInfo(RoomIndoor riItem)
        {
            if (riItem == null)
                return;
            //bll = new ProjectBLL(thisProject);
            //this.jclblUnitInfo.Text = ShowText.IndoorUnit +" - "+ riItem.IndoorName;
            this.pnlOutdoorInfo.Visible = false;
            this.pnlIndoorInfo.Visible = false;

            this.jclblIndModelValue.Text = riItem.IndoorItem.Model_Hitachi;
            if (thisProject.BrandCode == "Y")
                this.jclblIndModelValue.Text = riItem.IndoorItem.Model_York;
            this.jclblIndoorTypeValue.Text = riItem.IndoorItem.Type;
            this.jclblInDBCoolValue.Text = Unit.ConvertToControl(riItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblInWBCoolValue.Text = Unit.ConvertToControl(riItem.WBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblInDBHeatValue.Text = Unit.ConvertToControl(riItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            double rh = (new ProjectBLL(thisProject)).CalculateRH(riItem.DBCooling, riItem.WBCooling, thisProject.Altitude);
            this.jclblIndRHValue.Text = (rh * 100).ToString("n0") + "%";

            this.jclblAvailableCValue1.Text = Unit.ConvertToControl(riItem.ActualCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            //this.jclblAvailableCValue2.Text = Unit.ConvertToControl(riItem.SensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            //室外机选型结果中的显热应该取实际显热 20161114 by Yunxiao Lin
            this.jclblAvailableCValue2.Text = Unit.ConvertToControl(riItem.ActualSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            this.jclblAvailableHValue1.Text = Unit.ConvertToControl(riItem.ActualHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            this.jclblRequiredCValue1.Text = riItem.IsAuto ?
                Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            this.jclblRequiredCValue2.Text = riItem.IsAuto ?
                Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            this.jclblRequiredHValue1.Text = riItem.IsAuto ?
                Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";

            pnlCoolingInd.Enabled = thisProject.IsCoolingModeEffective;
            //pnlHeatingInd.Enabled = thisProject.IsHeatingModeEffective;
            //由于一个Project中可能存在多个ProductType，因此单凭thisProject.IsHeatingModeEffective无法确定当前室内机是否需要制热功能。
            //还需要判断室内机的productType中是否包含", CO"，如果包含，就不需要制热。 20160826 by Yunxiao Lin
            pnlHeatingInd.Enabled = thisProject.IsHeatingModeEffective && !riItem.IndoorItem.ProductType.Contains(", CO");
            
            ShowWarningMsg("");

            if (!CommonBLL.FullMeetRoomRequired(riItem, thisProject))
            {
                ShowWarningMsg(Msg.GetResourceString("IND_CAPACITY_MSG"), Color.Chocolate);
            }
        }

        // 选中 Outdoor 节点时绑定的信息
        /// <summary>
        /// 选中 Outdoor 节点时绑定的信息
        /// </summary>
        /// <param name="sysItem"></param>
        private void BindOutdoorItemInfo(SystemVRF sysItem)
        {
            if (sysItem == null)
                return;
            if (sysItem.OutdoorItem == null)
                return;
            ProjectBLL bll = new ProjectBLL(thisProject);
            Trans trans = new Trans();

            this.jclblUnitInfo.Text = ShowText.OutdoorUnit + " - " + sysItem.Name;

            this.pnlOutdoorInfo.Visible = true;
            this.pnlIndoorInfo.Visible = false;
            this.dgvSystemInfo.Visible = false;

            //MyProductType ptype = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, thisProject.ProductType);
            //MyProductType ptype = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, sysItem.OutdoorItem.ProductType);
            //if (ptype != null)
                //this.jclblProductTypeValue.Text = ptype.Series;
            this.jclblProductTypeValue.Text = trans.getTypeTransStr(TransType.Series.ToString(), sysItem.OutdoorItem.Series);
            double ratio = sysItem.Ratio;
            this.jclblActualRatioValue.Text = (ratio * 100).ToString("n0") + "%";
            this.jclblMaxRatioValue.Text = sysItem.MaxRatio * 100 + "%";
            this.jclblOutDBCoolValue.Text = Unit.ConvertToControl(sysItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblOutDBHeatValue.Text = Unit.ConvertToControl(sysItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblOutWBHeatValue.Text = Unit.ConvertToControl(sysItem.WBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblOutIWCoolValue.Text = Unit.ConvertToControl(sysItem.IWCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblOutIWHeatValue.Text = Unit.ConvertToControl(sysItem.IWHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            double rh = bll.CalculateRH(sysItem.DBHeating, sysItem.WBHeating, thisProject.Altitude);
            this.jclblOutRHValue.Text = (rh * 100).ToString("n0") + "%";

            double totestIndCap_h = 0;
            double totestIndCap_c = getTotestIndCap_c(sysItem, bll, out totestIndCap_h);
            this.jclblAvailableCValue.Text = Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            if (totestIndCap_c != 0d)
                this.jclblRequiredCValue.Text = Unit.ConvertToControl(totestIndCap_c, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            else
                this.jclblRequiredCValue.Text = "-";
            this.jclblAvailableHValue.Text = Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            this.jclblRequiredHValue.Text = Unit.ConvertToControl(totestIndCap_h, UnitType.POWER, ut_power).ToString("n1") + ut_power;

            this.pnlCoolingOut.Enabled = thisProject.IsCoolingModeEffective;
            //this.pnlHeatingOut.Enabled = thisProject.IsHeatingModeEffective;
            //由于一个project中可能存在多个不同的productType，所以单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
            //还需要判断当前系统的室外机的productType，如果ProductType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
            this.pnlHeatingOut.Enabled = thisProject.IsHeatingModeEffective && !sysItem.OutdoorItem.ProductType.Contains(", CO");

            //Add total indoor units number and capacity on 20190619 by xyj
            var IDUList = thisProject.RoomIndoorList.FindAll(p => p.SystemID == sysItem.Id);
            if (IDUList.Count > 0)
            {
                double horsepower = 0;
                this.jclblIndoorNumberValue.Text = IDUList.Count.ToString();
                foreach (RoomIndoor ri in IDUList)
                {
                    horsepower += ri.IndoorItem.Horsepower;
                }
                this.jclbltotalNominalCapacityValue.Text = horsepower + "HP";
            }
            if (sysItem.OutdoorItem != null)
            {
                // this.jclblOutModelValue.Text = sysItem.OutdoorItem.ModelFull;
                // changed on 20130911 新需求

                this.jclblOutModelValue.Text = sysItem.OutdoorItem.AuxModelName;
                this.jclblOutdoorTypeValue.Text = sysItem.OutdoorItem.Type;

                //显示进水温度 add by Junjie Shen on 20160622
                bool isWaterSource = !string.IsNullOrEmpty(sysItem.OutdoorItem.ProductType) &&
                    sysItem.OutdoorItem.ProductType.Contains("Water Source");

                //制冷-DB温度
                this.jclblOutDBCool.Visible = !isWaterSource;
                this.jclblOutDBCoolValue.Visible = !isWaterSource;

                //制热-DB
                this.jclblOutDBHeat.Visible = !isWaterSource;
                this.jclblOutDBHeatValue.Visible = !isWaterSource;

                //制热-WB
                this.jclblOutWBHeat.Visible = !isWaterSource;
                this.jclblOutWBHeatValue.Visible = !isWaterSource;

                //制热-RH 
                this.jclblOutRH.Visible = !isWaterSource;
                this.jclblOutRHValue.Visible = !isWaterSource;

                //制冷-进水温度
                this.jclblOutIWCool.Visible = isWaterSource;
                this.jclblOutIWCoolValue.Visible = isWaterSource;

                //制热-进水温度
                this.jclblOutIWHeat.Visible = isWaterSource;
                this.jclblOutIWHeatValue.Visible = isWaterSource;

                if (isWaterSource)
                {
                    //调整制冷-进水温度位置
                    this.jclblOutIWCool.Location = new Point(18, 33);
                    this.jclblOutIWCoolValue.Location = new Point(132, 33);

                    //调整制冷-进水温度位置
                    this.jclblOutIWHeat.Location = new Point(31, 33);
                    this.jclblOutIWHeatValue.Location = new Point(132, 33);
                }
            }
        }

        // 添加室外机窗口
        /// <summary>
        /// 添加室外机窗口
        /// </summary>
        /// <param name="opName"></param>
        private void DoAddOutdoor()
        {
            if (dgvIndoorNotAssigned.Rows.Count == 0)
            {
                string msg = Msg.GetResourceString("NO_IDU_CAN_BE_ASSIGNED");
                ShowWarningMsg(msg);
                return;
            }
            //复制一份RoomIndoorList,以便Cancel以后恢复 20161130 by Yunxiao Lin
            List<RoomIndoor> rilist_Original = thisProject.RoomIndoorList;
            thisProject.RoomIndoorList = rilist_Original.DeepClone();
            Form f;
            if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
            {
                f = new frmAddOutdoorUniversal(thisProject);
            }
            else
            {
                f = new frmAddOutdoor(thisProject);
            }
            f.StartPosition = FormStartPosition.CenterScreen;
            if (f.ShowDialog() == DialogResult.OK)
                BindToControl_SelectedUnits();
            else
                //当选择Cancel时恢复RoomIndoorList
                thisProject.RoomIndoorList = rilist_Original;


            if (tvOutdoor.Nodes.Count > 0)
            {
                tvOutdoor.SelectedNode = tvOutdoor.Nodes[tvOutdoor.Nodes.Count - 1];
            }
        }

        // 编辑已选的室外机
        /// <summary>
        /// 编辑已选的室外机
        /// </summary>
        private bool DoEditOutdoor(SystemVRF sysItem)
        {
            #region 手动配管,管长为0则不允许修改室外机   add on 20160825 by Lingjia Qiu
            if (sysItem.IsInputLengthManually)
            {
                double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                if (len <= 0)
                {
                    JCMsg.ShowErrorOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));
                    return false;
                }
            }
            #endregion

            //对于编辑室外机选型，需要克隆一个SystemVRF对象，以便取消编辑后能够恢复原来的属性 20161126 Yunxiao Lin
            SystemVRF sysItem_temp = sysItem.DeepClone();
            //还需要克隆相应的RoomIndoorList 20161130 by Yunxiao Lin
            List<RoomIndoor> rilist_Original = thisProject.RoomIndoorList;
            thisProject.RoomIndoorList = rilist_Original.DeepClone();
            Form f;
            if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
            {
                f = new frmAddOutdoorUniversal(sysItem, thisProject);
            }
            else
            {
                f = new frmAddOutdoor(sysItem, thisProject);
            }
            f.StartPosition = FormStartPosition.CenterScreen;
            if (f.ShowDialog() == DialogResult.Cancel)
            {
                //恢复原来的System属性
                sysItem.Copy(sysItem_temp);
                //恢复RoomIndoorList
                thisProject.RoomIndoorList = rilist_Original;
                return false;
            }

            sysItem.IsUpdated = true;


            //如果移除所有室内机，则室外机也会移除，需要清空piping和wiring图并锁定piping和wiring页面 modify on 20160602 by Yunxiao Lin
            if (sysItem.OutdoorItem == null)
            {
                string sysID = tvOutdoor.SelectedNode.Name;
                (new ProjectBLL(thisProject)).RemoveSystemVRF(sysID);
                BindToControl_SelectedUnits();
                if (sysItem == curSystemItem)
                {
                    if (tpgPiping.ImageKey != "Error")
                        InitPipingNodes();
                    if (tpgWiring.ImageKey != "Error")
                        InitWiringNodes(addFlowWiring);
                    SetTabControlImageKey();
                }

            }

            if (thisProject.SystemList.Count == 0)
            {
                BindOutdoorItemInfo(null);
            }

            return true;
        }

        // 删除已选的室外机
        /// <summary>
        /// 删除已选择的室外机
        /// </summary>
        private void DoDeleteOutdoor(string sysID)
        {
            DialogResult res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL);
            if (res == DialogResult.OK)
            {
                //SystemVRF sysItem = bll.GetSystem(sysID);
                //bll.RemoveControlGroup(sysItem.ControlGroupID);//不对，删除一个室外机并不表示整个ControlGroup无效了
                (new ProjectBLL(thisProject)).RemoveSystemVRF(sysID);
                UndoRedoUtil.SaveProjectHistory();
            }
        }

        // 拷贝已选的室外机系统，连同室内机一同拷贝
        /// <summary>
        /// 拷贝已选择的室外机系统，连同室内机一同拷贝
        /// </summary>
        private void DoCopyOutdoor()
        {
            if (this.tvOutdoor.SelectedNode == null)
            {
                //请选择一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONE);
                return;
            }

            //确认框
            if (JCMsg.ShowConfirmOKCancel(JCMsg.COMMON_COPY_CHECK) != DialogResult.OK)
            {
                return;
            }

            TreeNode tnOut = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);

            if (tnOut == null) return;

            SystemVRF sysItem = tnOut.Tag as SystemVRF;
            if (sysItem == null || sysItem.OutdoorItem == null) return;

            //调用拷贝室内机方法
            ProjectBLL projectBLL = new ProjectBLL(thisProject);
            SystemVRF sysCopy = projectBLL.CopyOutdoor(sysItem,
                SystemSetting.UserSetting.defaultSetting.OutdoorName,
                SystemSetting.UserSetting.defaultSetting.IndoorName,
                SystemSetting.UserSetting.defaultSetting.FloorName,
                SystemSetting.UserSetting.defaultSetting.RoomName,
                SystemSetting.UserSetting.defaultSetting.FreshAirAreaName);

            //复制配管图
            CopyPipingDiagram(sysItem, sysCopy);

            //刷新系统列表
            BindToControl_SelectedUnits();
            UndoRedoUtil.SaveProjectHistory();
        }

        public void CopyPipingDiagram(SystemVRF sysItem, SystemVRF sysCopy)
        {
            if (sysItem.IsUpdated) return;

            if (sysCopy.MyPipingNodeOutTemp == null && sysItem.MyPipingNodeOut != null)
            {
                PipingBLL pipBll = GetPipingBLLInstance();
                pipBll.SavePipingStructure(sysItem);
                sysCopy.MyPipingNodeOutTemp = sysItem.MyPipingNodeOutTemp.DeepClone();
            }

            if (sysItem.MyPipingNodeOutTemp != null)
            {
                //用复制出来的室内机替换掉indoor节点的RoomIndoorItem属性值
                int index = 0;
                List<RoomIndoor> riList = thisProject.RoomIndoorList.FindAll(ind => ind.SystemID == sysCopy.Id);
                ReplaceIndoorsForPipingDiagramCopy(sysCopy.MyPipingNodeOutTemp, riList, ref index);
            }
        }

        private void ReplaceIndoorsForPipingDiagramCopy(tmpMyNode node, List<RoomIndoor> riList, ref int index)
        {
            if (node == null) return;

            if (node is tmpMyNodeIn)
            {
                //用复制出来的室内机替换掉indoor节点的RoomIndoorItem属性值
                tmpMyNodeIn nodeIn = node as tmpMyNodeIn;
                nodeIn.RoomIndoorItem = riList[index];
                index++;
            }
            else if (node is tmpMyNodeOut)
            {
                tmpMyNodeOut nodeOut = node as tmpMyNodeOut;
                ReplaceIndoorsForPipingDiagramCopy(nodeOut.ChildNode, riList, ref index);
            }
            else if (node is tmpMyNodeYP)
            {
                tmpMyNodeYP nodeYP = node as tmpMyNodeYP;

                foreach (tmpMyNode nd in nodeYP.ChildNodes)
                {
                    ReplaceIndoorsForPipingDiagramCopy(nd, riList, ref index);
                }
            }
            else if (node is tmpMyNodeCH)
            {
                tmpMyNodeCH nodeCH = node as tmpMyNodeCH;
                ReplaceIndoorsForPipingDiagramCopy(nodeCH.ChildNode, riList, ref index);
            }
        }

        // 选中TreeView节点点击“编辑” 按钮
        /// <summary>
        /// 选中TreeView节点点击“编辑” 按钮
        /// </summary>
        /// <param name="node"></param>
        private void DoEditTreeNode(TreeNode node, TreeNode nodeOut)
        {
            int level = node.Level;
            Boolean doEditFlag = true;
            switch (level)
            {
                case 0:
                    if (node.Tag is SystemVRF && DoEditOutdoor(node.Tag as SystemVRF))
                    {
                        BindToControl_SelectedUnits();
                    }
                    break;
                case 1:
                case 2:
                    if (node.Tag is RoomIndoor)
                    {
                        doEditFlag = DoEditIndoor(node.Tag as RoomIndoor, false);
                        return; //防止再次刷新tvOutdoor树控件  add by Shen Junjie on 2018/3/13
                    }
                    break;
            }

            //if (level > 0)
            //{
            //    //RefreshSelectedIndoor(nodeOut.Name);
            //}
            BindToControl_SelectedUnits();

            //BindWiringDrawing(tvOutdoor.Nodes[nodeOut.Index]);
        }

        #endregion

        #region Main_Piping
        /// 标记是否正在执行控件值的绑定
        /// <summary>
        /// 标记是否正在执行控件值的绑定
        /// </summary>
        bool isBinding = false;
        bool isHitachi = false;
        //bool isInch = false;
        //SystemVRF curSystemItem = null;
        SystemVRF curSystemItem = new SystemVRF();   //初始化对象

        private PipingBLL GetPipingBLLInstance()
        {
            bool isInch = CommonBLL.IsDimension_inch();
            return new PipingBLL(thisProject, utilPiping, addFlowPiping, isInch, ut_weight, ut_length, ut_power);
        }

        /// 设置Piping & Wiring界面中 SplitCollapse 控件的收缩、展开
        /// <summary>
        /// 设置Piping & Wiring界面中 SplitCollapse 控件的收缩、展开
        /// </summary>
        /// <param name="splitContainer"></param>
        /// <param name="btn"></param>
        private void SetSplitCollapse(SplitContainer splitContainer, JCButton btn)
        {
            // Expand
            if (splitContainer.Panel1Collapsed)
            {
                splitContainer.Panel1Collapsed = false;
                //btn.Text = "<";
                btn.BackgroundImage = Properties.Resources._140103_04_piping_arrow;
            }
            // Collapse
            else
            {
                splitContainer.Panel1Collapsed = true;
                //btn.Text = ">";
                btn.BackgroundImage = Properties.Resources._140103_04_piping_arrow_left_to_right;
            }
        }

        /// 绑定选中系统的配管图，入口
        /// <summary>
        /// 绑定选中系统的配管图
        /// </summary>
        /// <param name="curSystemItem"></param>
        private void BindPipingDrawing(TreeNode tnOut)
        {
            this.jcbtnManualPiping.Visible = false;

            string currentTabName = this.tabControl1.SelectedTab.Name;
            if (currentTabName != "tpgPiping" && currentTabName != "tpgReport" && currentTabName != "tpgWiring")
                return;
            
            if (curSystemItem == null)
                return;

            //切换Piping图显示的时候保存先保存当前的Piping图 added by Shen Junjie on 2019/04/29
            DoSavePipingStructure();

            // 若系统室外机对象为空，则不能绘制配管图，显示相应提示
            if (curSystemItem.OutdoorItem == null)
            {
                //InitPipingNodes();
                //Label lbl = new Label();
                //lbl.Dock = DockStyle.Fill;
                //lbl.Text = "ERROR! Cannot draw the piping diagrams.";
                //lbl.ForeColor = Color.Red;
                //addFlowPiping.Controls.Add(lbl);

                DrawPipError();
                return;
            }

            //IVX暂时屏蔽手动piping 20170704 by Yunxiao Lin
            if (!curSystemItem.OutdoorItem.Series.Contains("IVX"))
            {
                this.jcbtnManualPiping.Visible = true;
            }

            if (curSystemItem.IsUpdated == false && curSystemItem == currentPipingSystem)
                return;

            this.jcbtnVarification.Enabled = false;

            PipingBLL pipBll = GetPipingBLLInstance();

            bool enableMultipCHBox = false;
            if (PipingBLL.IsHeatRecovery(curSystemItem))
            {
                //只允许存在Multiple CH Box的区域和系列使用这个功能
                //目前（2018/1/16）只有 EU 和 ANZ 区域有 Multiple CH Box
                enableMultipCHBox = pipBll.ExistsMultiCHBoxStd(thisProject.SubRegionCode, curSystemItem.Series);
            }

            isBinding = true;
            this.uc_CheckBox_PipingLength.Checked = curSystemItem.IsInputLengthManually;
            this.uc_CheckBox_PipingVertical.Checked = curSystemItem.IsPipingVertical;
            this.uc_CheckBox_PipingHorizontal.Checked = !this.uc_CheckBox_PipingVertical.Checked;
            //Delete "Binary Tree" and "2 Branch Binary Tree" from pipe interface. only for LA     add by Shen Junjie on 2018/4/4
            if (thisProject.RegionCode == "LA")
            {
                this.jccmbPipingLayoutType.SelectedIndex = 0;
                curSystemItem.PipingLayoutType = PipingLayoutTypes.Normal;
            }
            else
            {
                this.jccmbPipingLayoutType.SelectedIndex = GetPipingLayoutTypeIndex(curSystemItem.PipingLayoutType);
            }
            this.jcbtnAddMultipleCHBox.Visible = enableMultipCHBox;
            // 绑定完成,使相关控件的事件可以触发
            isBinding = false;

            if (curSystemItem.MyPipingNodeOut == null) // 新增的outdoor system 或者打开项目时的 load
            {
                if (curSystemItem.MyPipingNodeOutTemp != null)
                {
                    // 转化当前 systemVRF 类中保存的 MyPipingNodeOutTemp 对象，构造对应的 MyPipingNodeOut 对象
                    //CreatePipingNodeStructure(curSystemItem);
                    pipBll.LoadPipingNodeStructure(curSystemItem);
                }
            }

            if (curSystemItem.IsUpdated)   // 配管图是否需要重绘
            {
                SetSystemPipingOK(curSystemItem, false);
                if (curSystemItem.IsManualPiping)
                {
                    UpdateManualPipingNodes(curSystemItem);
                    curSystemItem.IsUpdated = false;
                }
                else
                {
                    utilPiping.ResetColors();
                    UpdatePipingNodeStructure(curSystemItem);
                    curSystemItem.IsUpdated = false;
                }
            }

            if (curSystemItem.MyPipingNodeOutTemp != null && curSystemItem.IsManualPiping)
            {
                //根据MyPipingNodeOutTemp保存的坐标还原各节点和连接线，不重算坐标
                DoDrawingPiping(false);
            }
            else
            {
                // 根据构建的Node结构绘制配管图，重新计算节点坐标
                DoDrawingPiping(true);
            }
        }

        /// 外机不匹配，则不能绘制配管图，显示相应提示
        /// <summary>
        /// 外机不匹配，则不能绘制配管图，显示相应提示   Add on 20160801 by Lingjia Qiu
        /// </summary>
        private void DrawPipError(string msg = null)
        {
            //若外机为空，系统ID置空作为尚未分配给室外机的室内机列表
            //foreach (RoomIndoor riItem in thisProject.RoomIndoorList)
            //{
            //    if (curSystemItem.Id == riItem.SystemID)
            //        riItem.SetToSystemVRF(null);
            //}

            if (string.IsNullOrEmpty(msg))
            {
                msg = "ERROR! Cannot draw the piping diagrams.";
            }

            InitPipingNodes();
            Label lbl = new Label();
            lbl.Dock = DockStyle.Fill;
            lbl.Text = msg;
            lbl.ForeColor = Color.Red;
            addFlowPiping.Controls.Add(lbl);
        }

        /// 外机不匹配，则不能绘制配线图，显示相应提示
        /// <summary>
        /// 外机不匹配，则不能绘制配线图，显示相应提示   Add on 20160801 by Lingjia Qiu
        /// </summary>
        private void DrawDiagError()
        {
            Label lbl2 = new Label();
            lbl2.Dock = DockStyle.Fill;
            lbl2.Text = "ERROR! Cannot draw the Wiring diagrams because there is no indoor unit";
            lbl2.ForeColor = Color.Red;
            addFlowWiring.Controls.Add(lbl2);

        }

        /// 绘制配管布局图，V型 或 H型
        /// <summary>
        /// 绘制配管布局图，V型 或 H型，需要先创建节点结构
        /// </summary>
        /// <param name="reset">true: 按原来的方式自动重新布局 false: 根据MyPipingNodeOutTemp保存的坐标还原各节点和连接线，不重算坐标 add by Shen Junjie on 20170801</param>
        private void DoDrawingPiping(bool reset)
        {
            isHitachi = thisProject.BrandCode == "H";
            bool isHR = PipingBLL.IsHeatRecovery(curSystemItem);
            //isInch = CommonBLL.IsDimension_inch();

            InitPipingNodes();
            MyNodeOut pipingNodeOut = curSystemItem.MyPipingNodeOut;
            if (pipingNodeOut == null || curSystemItem.OutdoorItem == null) return;
            if (pipingNodeOut.ChildNode == null) return;

            string dir = MyConfig.PipingNodeImageDirectory;
            PipingBLL pipBll = GetPipingBLLInstance();
            
            //如果是Heat Recovery系列，需要在绘图之前计算每个节点的isCoolingonly属性 Add on 20160509 by Yunxiao Lin
            pipBll.CheckPipesType(curSystemItem.MyPipingNodeOut, isHR);

            if (reset)
            {
                curSystemItem.IsManualPiping = false;
                //bool isVertical = this.uc_CheckBox_PipingVertical.Checked;

                utilPiping.ResetColors();
                pipBll.DrawPipingNodes(curSystemItem, dir);

                // 完成Nodes之后，绘制Links
                pipBll.DrawPipingLinks(curSystemItem, addFlowPiping);

                pipBll.DrawTextToAllNodes(curSystemItem.MyPipingNodeOut, null, curSystemItem);
            }
            else
            {
                if (curSystemItem.IsManualPiping)
                {
                    utilPiping.colorDefault = curSystemItem.MyPipingNodeOutTemp.PipeColor;
                    utilPiping.colorText = curSystemItem.MyPipingNodeOutTemp.TextColor;
                    utilPiping.colorYP = curSystemItem.MyPipingNodeOutTemp.BranchKitColor;
                    utilPiping.colorNodeBg = curSystemItem.MyPipingNodeOutTemp.NodeBgColor;
                }
                else
                {
                    utilPiping.ResetColors();
                }
                //根据MyPipingNodeOutTemp保存的坐标还原各节点和连接线，不重算坐标
                pipBll.DrawPipingNodesNoCaculation(dir, curSystemItem);
            }

            // 绘制Correction Factor标注。 2016-4-18 by lin
            pipBll.DrawCorrectionFactorText(curSystemItem);

            //为管线图添加加注冷媒标注 2016-12-22 by shen junjie
            if (curSystemItem.IsInputLengthManually && curSystemItem.IsPipingOK)
            {
                pipBll.DrawAddRefrigerationText(curSystemItem);
            }

            //绘制管型图例 20160510 by Yunxiao Lin
            pipBll.drawPipelegend(isHR);

            this.jcbtnVarification.Enabled = true;
             
            //在绘图完成后重新设置Node颜色
            pipBll.SetDefaultColor(addFlowPiping);

            //绘制背景图
            pipBll.DrawBuildingImageNode(curSystemItem.MyPipingBuildingImageNodeTemp);

            currentPipingSystem = curSystemItem;
        }

        /// 拖拽完后，重新定位滚动条位置
        /// <summary>
        /// 拖拽完后，重新定位滚动条位置
        /// </summary>
        private void ResetScrollPosition()
        {
            if (scrollPosition != null)
                this.addFlowPiping.ScrollPosition = new Point(Math.Abs(scrollPosition.X), Math.Abs(scrollPosition.Y));
        }

        /// 检查当前配管图中是否允许加入CP节点
        /// <summary>
        /// 检查当前配管图中是否允许加入CP节点
        /// </summary>
        /// <param name="myNodeOut"></param>
        /// <returns></returns>
        private bool CheckCPNodeAddible()
        {
            if (curSystemItem == null || curSystemItem.MyPipingNodeOut == null)
            {
                return false;
            }

            PipingBLL pipBll = GetPipingBLLInstance();

            MyNode selPNode = null;
            if (selNode != null)
            {
                selPNode = selNode.ParentNode;
            }

            //添加HR判断 on 20160505 by Yunxiao Lin
            if (PipingBLL.IsHeatRecovery(curSystemItem))
            {
                if (selNode != null && selPNode != null)
                {
                    //梳形管上层不能再有梳形管
                    if (pipBll.ExistCPUpward(selNode))
                        return false;

                    //梳形管下层不能再有梳形管或分歧管
                    if (pipBll.ExistYPDownward(selNode))
                        return false;

                    //梳形管下层不能有Multi CH Box
                    if (pipBll.ExistsMultiCHBoxDownward(selNode))
                        return false;

                    return true;
                }
            }
            else
            {
                if (selNode != null && selPNode != null && selNode is MyNodeIn && selPNode is MyNodeYP)
                {
                    if (selPNode is MyNodeYP && (selPNode as MyNodeYP).IsCP)
                    {
                        // 树型管后面不能再接分歧管
                        return false;
                    }
                    MyNodeIn nodeIn = selNode as MyNodeIn;
                    if (PipingBLL.IsTwoInlinkIndoor(nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi))
                    {
                        //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。此Indoor前必须是YP，所以不能添加到梳形管下。
                        // add by Shen Junjie on 2018/01/26
                        return false;
                    }
                    return true;
                }
                else
                {
                    JCMsg.ShowInfoOK(Msg.PIPING_ADDCP);
                }
            }
            return false;
        }

        #region 刷新Indoor的排列顺序(TreeView & Wiring)
        /// 刷新 RoomIndoor List 中的排列顺序
        /// <summary>
        /// 刷新 RoomIndoor List 中的排列顺序
        /// </summary>
        /// <param name="selNode"></param>
        /// <param name="aimNode"></param>
        private void DoUpdateTreeNodeOrder(SystemVRF sysItem)
        {
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            TreeNode tnOut = Global.GetTopParentNode(tvOutdoor.SelectedNode);
            tnOut.Nodes.Clear();
            List<RoomIndoor> riList = new List<RoomIndoor>(); //按照当前Piping图排序
            List<RoomIndoor> riOrginalList = thisProject.RoomIndoorList.FindAll(c => (c.SystemID == sysItem.Id));

            PipingBLL.EachNode(nodeOut, (node) =>
            {
                if (node is MyNodeIn)
                {
                    MyNodeIn ndIn = node as MyNodeIn;
                    RoomIndoor ri = ndIn.RoomIndooItem;
                    if (ri == null) return;

                    riList.Add(ri);

                    TreeNode tn = new TreeNode();
                    tn.Tag = ri;
                    tn.Name = ri.IndoorNO.ToString();
                    string model = ri.IndoorItem.Model_Hitachi;
                    if (thisProject.BrandCode == "Y")
                        model = ri.IndoorItem.Model_York;
                    tn.Text = ri.IndoorName + "[" + model + "]";
                    Global.SetTreeNodeImage(tn, 2, 4);
                    tnOut.Nodes.Add(tn);
                }
            });

            //因Manual Piping要支持保存不完整的Piping图，会产生Piping图上的Indoor比系统内的少的情况  add by Shen Junjie on 2018/11/21
            if (riList.Count < riOrginalList.Count)
            {
                foreach(RoomIndoor ri in riOrginalList)
                {
                    //把系统内其余的Indoor放到新列表末尾
                    if (!riList.Exists(p => p.IndoorNO == ri.IndoorNO))
                    {
                        riList.Add(ri);

                        TreeNode tn = new TreeNode();
                        tn.Tag = ri;
                        tn.Name = ri.IndoorNO.ToString();
                        string model = ri.IndoorItem.Model_Hitachi;
                        if (thisProject.BrandCode == "Y")
                            model = ri.IndoorItem.Model_York;
                        tn.Text = ri.IndoorName + "[" + model + "]";
                        Global.SetTreeNodeImage(tn, 2, 4);
                        tnOut.Nodes.Add(tn);
                    }
                }
            }

            thisProject.RoomIndoorList.RemoveAll(c => (c.SystemID == sysItem.Id));
            thisProject.RoomIndoorList.AddRange(riList);
        }

        #endregion

        #region 根据系统 TreeView 构造配管图节点结构

        ///// 根据 TreeView 布局构造 piping 图中的机组节点对象（绑定机组的图片属性到各个节点对象）
        ///// <summary>
        ///// 根据 TreeView 布局构造piping节点对象及初始化一般属性
        ///// </summary>
        ///// <param name="tnOut"></param>
        ///// <param name="isVertical"></param>
        //public void CreatePipingNodeStructure(TreeNode tnOut)
        //{
        //    curSystemItem.MyPipingNodeOut = utilPiping.createNodeOut();
        //    curSystemItem.MyPipingNodeOut.Model = curSystemItem.OutdoorItem.AuxModelName;
        //    curSystemItem.MyPipingNodeOut.Name = curSystemItem.Name;
        //    MyNodeOut nodeOut = curSystemItem.MyPipingNodeOut;

        //    if (tnOut.Nodes.Count > 0)
        //    {
        //        TreeNode tn = tnOut.Nodes[0];
        //        if (tnOut.Nodes.Count == 1)
        //        {
        //            // Outdoor Indoor 一对一
        //            RoomIndoor riItem = tn.Tag as RoomIndoor;
        //            MyNodeIn inFirst = utilPiping.createNodeIn(riItem);
        //            nodeOut.ChildNode = inFirst;
        //        }
        //        else if (tnOut.Nodes.Count == 3 && curSystemItem.OutdoorItem.Type == "HVN(P/C/C1)/HVRNM2 (Side flow)")
        //        {
        //            //SMZ IVX 3 Indoor 组合固定使用梳形管连接 20170704 by Yunxiao Lin
        //            MyNodeYP ypFirst = createYPNodeGroup(tn, true);
        //            nodeOut.ChildNode = ypFirst;
        //        }
        //        else
        //        {
        //            MyNodeYP ypFirst = createYPNodeGroup(tn, false);
        //            nodeOut.ChildNode = ypFirst;
        //        }
        //    }
        //}

        ///// 递归程序，构造 MyNodeYP 的组合结构
        ///// <summary>
        ///// 递归程序，构造 MyNodeYP 的组合结构
        ///// </summary>
        ///// <param name="tn"></param>
        ///// <param name="isCP"></param>
        ///// <returns></returns>
        //private MyNodeYP createYPNodeGroup(TreeNode tn, bool isCP)
        //{
        //    MyNodeYP yp = utilPiping.createNodeYP(isCP);

        //    RoomIndoor riItem = tn.Tag as RoomIndoor;
        //    MyNodeIn ind1 = utilPiping.createNodeIn(riItem);

        //    yp.AddChildNode(ind1);

        //    // 当前室内机不是最后一个,则添加一个YP节点
        //    if (tn.NextNode != tn.Parent.Nodes[tn.Parent.Nodes.Count - 1] && !isCP)
        //    {
        //        MyNodeYP chYP = createYPNodeGroup(tn.NextNode, false);
        //        yp.AddChildNode(chYP);
        //    }
        //    else if (!isCP)
        //    {
        //        riItem = tn.NextNode.Tag as RoomIndoor;
        //        MyNodeIn ind2 = utilPiping.createNodeIn(riItem);
        //        yp.AddChildNode(ind2);
        //    }
        //    else
        //    {
        //        while (tn != tn.Parent.Nodes[tn.Parent.Nodes.Count - 1])
        //        {
        //            tn = tn.NextNode;
        //            riItem = tn.Tag as RoomIndoor;
        //            MyNodeIn ind2 = utilPiping.createNodeIn(riItem);
        //            yp.AddChildNode(ind2);
        //        }
        //    }
        //    return yp;
        //}

        ///// 根据 TreeView 布局构造 piping 图中的机组节点对象（绑定机组的图片属性到各个节点对象）
        ///// <summary>
        ///// 根据 TreeView 布局构造piping节点对象及初始化一般属性
        ///// </summary>
        ///// <param name="tnOut"></param>
        ///// <param name="isVertical"></param>
        //public void CreatePipingNodeStructure_CHbox(TreeNode tnOut)
        //{
        //    SystemVRF sysItem = tnOut.Tag as SystemVRF;
        //    sysItem.MyPipingNodeOut = utilPiping.createNodeOut();
        //    sysItem.MyPipingNodeOut.Model = sysItem.OutdoorItem.AuxModelName;
        //    sysItem.MyPipingNodeOut.Name = sysItem.Name;
        //    MyNodeOut nodeOut = sysItem.MyPipingNodeOut;

        //    if (tnOut.Nodes.Count > 0)
        //    {
        //        TreeNode tn = tnOut.Nodes[0];
        //        if (tnOut.Nodes.Count == 1)
        //        {
        //            // Outdoor CH-Box Indoor 一对一对一 Modify on 20160603 by Yunxiao Lin
        //            RoomIndoor riItem = tn.Tag as RoomIndoor;
        //            MyNodeIn nodeIn = utilPiping.createNodeIn(riItem);
        //            MyNodeCH chFirst = utilPiping.createNodeCHbox(nodeIn);
        //            nodeOut.ChildNode = chFirst;
        //        }
        //        else
        //        {
        //            MyNodeYP ypFirst = createYPNodeGroup_CHbox(tn, false);
        //            nodeOut.ChildNode = ypFirst;
        //        }
        //    }
        //}

        ///// 递归程序，构造 MyNodeYP 的组合结构
        ///// <summary>
        ///// 递归程序，构造 MyNodeYP 的组合结构
        ///// </summary>
        ///// <param name="tn"></param>
        ///// <param name="isCP"></param>
        ///// <returns></returns>
        //private MyNodeYP createYPNodeGroup_CHbox(TreeNode tn, bool isCP)
        //{
        //    MyNodeYP yp = utilPiping.createNodeYP(isCP);

        //    RoomIndoor riItem = tn.Tag as RoomIndoor;
        //    MyNodeIn ind1 = utilPiping.createNodeIn(riItem);
        //    MyNodeCH ch1 = utilPiping.createNodeCHbox(ind1);

        //    yp.AddChildNode(ch1);

        //    // 当前室内机不是最后一个,则添加一个YP节点
        //    if (tn.NextNode != tn.Parent.Nodes[tn.Parent.Nodes.Count - 1])
        //    {
        //        MyNodeYP chYP = createYPNodeGroup_CHbox(tn.NextNode, false);
        //        yp.AddChildNode(chYP);
        //    }
        //    else
        //    {
        //        riItem = tn.NextNode.Tag as RoomIndoor;
        //        MyNodeIn ind2 = utilPiping.createNodeIn(riItem);
        //        MyNodeCH ch2 = utilPiping.createNodeCHbox(ind2);
        //        yp.AddChildNode(ch2);
        //    }
        //    return yp;
        //}

        /// <summary>
        /// 更新手工配管图上的标注信息
        /// </summary>
        /// <param name="sysItem"></param>
        private void UpdateManualPipingNodes(SystemVRF sysItem)
        {
            if (sysItem == null || sysItem.OutdoorItem == null) return;
            PipingBLL pipBll = GetPipingBLLInstance();
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            List<RoomIndoor> indoors = thisProject.RoomIndoorList.FindAll(r => r.SystemID == sysItem.Id);
            int indoorCount = indoors != null ? indoors.Count : 0;
            int indoorIndex = 0;

            List<MyNodeIn> nodeInListToDelete = new List<MyNodeIn>();

            PipingBLL.EachNode(nodeOut, (node) =>
            {
                if (!(node is MyNodeIn)) return false;
                MyNodeIn nodeIn = node as MyNodeIn;
                if (nodeIn.RoomIndooItem == null) 
                {
                    nodeInListToDelete.Add(nodeIn);
                    return false;
                }
                
                //如果室内机列表已遍历结束
                if (indoorIndex >= indoorCount)
                {
                    //Piping图表节点需要删除
                    nodeInListToDelete.Add(nodeIn);   //防止出错不在EachNode中做删除操作
                    //直接遍历下一个Piping图表节点
                    return false;
                }
                RoomIndoor nodeIndoor = nodeIn.RoomIndooItem;
                RoomIndoor newIndoor = indoors[indoorIndex];

                bool isSame = nodeIndoor.IndoorNO == newIndoor.IndoorNO;

                if (isSame)
                {
                    //如果对象被改变则替换对象 add by Shen Junjie
                    if (nodeIndoor != newIndoor)
                    {
                        nodeIn.RoomIndooItem = newIndoor;
                    }
                    indoorIndex++;
                }
                else
                {
                    //需要从Piping图表中删除节点
                    nodeInListToDelete.Add(nodeIn);  //防止出错不在EachNode中做删除操作
                }
                return false;
            });

            //防止出错不在EachNode中做删除操作 
            foreach (MyNodeIn nodeIn in nodeInListToDelete)
            {
                pipBll.DeleteNode(nodeIn, false);
            }
        }

        private void UpdatePipingNodeStructure(SystemVRF sysItem)
        {
            if (sysItem == null || sysItem.OutdoorItem == null) return;
            bool isHR = PipingBLL.IsHeatRecovery(sysItem);
            PipingBLL pipBll = GetPipingBLLInstance();
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            if (nodeOut == null)
            {
                pipBll.CreatePipingNodeStructure(sysItem);
                return;
            }

            List<RoomIndoor> indoors = thisProject.RoomIndoorList.FindAll(r => r.SystemID == sysItem.Id);
            int indoorCount = indoors != null ? indoors.Count : 0;
            int indoorIndex = 0;
            bool isOrderChanged = false;

            List<MyNodeIn> nodeInListToDelete = new List<MyNodeIn>();

            //遍历Piping图表节点, 比对室内机列表，检查是否有顺序变化，
            //同时删除已经从室内机列表中删除的图表节点
            PipingBLL.EachNode(nodeOut, (node) =>
            {
                if (!(node is MyNodeIn)) return false;
                MyNodeIn nodeIn = node as MyNodeIn;

                //如果室内机列表已遍历结束
                if (indoorIndex >= indoorCount)
                {
                    //Piping图表节点需要删除
                    nodeInListToDelete.Add(nodeIn);   //防止出错不在EachNode中做删除操作 modify by Shen Junjie on 2018/01/29
                    //直接遍历下一个Piping图表节点
                    return false;
                }
                RoomIndoor nodeIndoor = nodeIn.RoomIndooItem;

                if (nodeIndoor == null)
                {
                    nodeInListToDelete.Add(nodeIn); //如果室内机信息已经不存在，则要删除室内机节点 add by Shen Junjie on 2019/3/28
                    return false;
                }

                RoomIndoor newIndoor = indoors[indoorIndex];

                bool isSame = nodeIndoor.IndoorNO == newIndoor.IndoorNO;

                if (isSame)
                {
                    //如果对象被改变则替换对象 add by Shen Junjie
                    if (nodeIndoor != newIndoor)
                    {
                        nodeIn.RoomIndooItem = newIndoor;
                    }
                    indoorIndex++;
                }
                else
                {
                    //检查是否移动到了后面
                    bool removedToBelow = false;
                    for (int i = indoorIndex + 1; i < indoorCount; i++)
                    {
                        if (nodeIndoor.IndoorNO == indoors[i].IndoorNO)
                        {
                            removedToBelow = true;
                            break;
                        }
                    }
                    if (removedToBelow)
                    {
                        //如果移动到了后面，则说明有循序调整
                        isOrderChanged = true;
                        //终止循环Piping图表节点
                        return true;
                    }
                    else
                    {
                        //如果后面没有说明已经从室内机列表里删除了，
                        //需要从Piping图表中删除节点
                        nodeInListToDelete.Add(nodeIn);  //防止出错不在EachNode中做删除操作 modify by Shen Junjie on 2018/01/29
                    }
                }
                return false;
            });

            //如果室内机顺序已经改变，或者将新室内机添加到了中间，则只能重新生成Piping节点树
            if (isOrderChanged)
            {
                pipBll.CreatePipingNodeStructure(sysItem);
                return;
            }

            //防止出错不在EachNode中做删除操作 add by Shen Junjie on 2018/01/29
            foreach (MyNodeIn nodeIn in nodeInListToDelete)
            {
                pipBll.DeleteNode(nodeIn, true);
            }

            //系列（Heat Pump 和Heat Recovery)改变后，Piping 图需要处理CH Box add by Shen Junjie on 20171215
            if (PipingBLL.IsHeatRecovery(curSystemItem))
            {
                if (!pipBll.ExistsCHBoxDownward(nodeOut))
                {
                    PipingBLL.EachNode(nodeOut, (node) =>
                    {
                        if (node is MyNodeIn)
                        {
                            //为每一个Indoor添加CH Box
                            pipBll.InsertCHBox(node);
                        }
                    });

                    //重置状态
                    SetSystemPipingOK(curSystemItem, false);
                }
            }
            else
            {
                if (pipBll.ExistsCHBoxDownward(nodeOut))
                {
                    pipBll.DeleteCHBoxDownward(nodeOut);

                    //重置状态
                    SetSystemPipingOK(curSystemItem, false);
                }
            }

            Node lastChild = nodeOut.ChildNode;
            Node lastParent = nodeOut;

            //查找最后一个子节点 
            while (lastChild is MyNodeYP)
            {
                MyNodeYP yp = (lastChild as MyNodeYP);
                if (yp.IsCP == false && yp.ChildCount > 1) //RPI-16.0,20.0FSN3PE(-f) 的YP的ChildCount为1
                {
                    lastParent = yp;
                    lastChild = yp.ChildNodes[yp.ChildCount - 1];
                }
                else
                {
                    break;
                }
            }

            if (indoorIndex < indoorCount)
            {
                //普通布局可以添加到最后
                for (int i = indoorIndex; i < indoorCount; i++)
                {
                    pipBll.AppendNodeIn(isHR, lastParent, indoors[i], out lastParent);
                }
            }
        }

        //#region add on 20130815 clh 解决AddFlow的节点不能序列化的问题
        ///// 通过自定义的可序列化的临时对象,构造当前系统的配管结构对象
        ///// 增加CH-Box直连Outdoor的处理 20160525 by Yunxiao Lin
        ///// <summary>
        ///// 通过自定义的可序列化的临时对象,构造当前系统的配管结构对象
        ///// </summary>
        ///// <param name="tnOut"></param>
        ///// <param name="tmpNodeOut"></param>
        //public void CreatePipingNodeStructure(SystemVRF sysItem)
        //{
        //    sysItem.MyPipingNodeOut = utilPiping.createNodeOut();
        //    sysItem.MyPipingNodeOut.Model = sysItem.OutdoorItem.AuxModelName;
        //    sysItem.MyPipingNodeOut.Name = sysItem.Name;
        //    MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
        //    tmpMyNodeOut tmpNodeOut = sysItem.MyPipingNodeOutTemp;

        //    tmpMyNode cNode = tmpNodeOut.ChildNode;
        //    if (cNode is tmpMyNodeIn)
        //    {
        //        // 一对一
        //        RoomIndoor riItem = (cNode as tmpMyNodeIn).RoomIndooItem;
        //        MyNodeIn inFirst = utilPiping.createNodeIn(riItem);
        //        nodeOut.ChildNode = inFirst;

        //        tmpMyLink tmpLink = (cNode as tmpMyNodeIn).InLink;
        //        MyLink myLink = inFirst.InLink;
        //        if (myLink != null && tmpLink != null)
        //        {
        //            LoadLinkValue(ref myLink, tmpLink);
        //        }
        //    }
        //    else if(cNode is tmpMyNodeYP)
        //    {
        //        MyNodeYP ypFirst = createYPNodeGroup(cNode as tmpMyNodeYP);
        //        nodeOut.ChildNode = ypFirst;
        //    }
        //    else if (cNode is tmpMyNodeCH)
        //    {
        //        tmpMyNodeCH tmpCH = cNode as tmpMyNodeCH;
        //        if (tmpCH.ChildNode is tmpMyNodeIn)
        //        {
        //            RoomIndoor riItem = (tmpCH.ChildNode as tmpMyNodeIn).RoomIndooItem;
        //            MyNodeIn nodeIn = utilPiping.createNodeIn(riItem);
        //            tmpMyLink tmpLink = (tmpCH.ChildNode as tmpMyNodeIn).InLink;
        //            MyLink myLink = nodeIn.InLink;
        //            if (myLink != null && tmpLink != null)
        //            {
        //                LoadLinkValue(ref myLink, tmpLink);
        //            }

        //            MyNodeCH chFirst = utilPiping.createNodeCHbox(nodeIn);

        //            LoadMyNodeCHValue(tmpCH, ref chFirst);

        //            nodeOut.ChildNode = chFirst;
        //        }
        //        else if (tmpCH.ChildNode is tmpMyNodeYP)
        //        {
        //            MyNodeYP yp = createYPNodeGroup(tmpCH.ChildNode as tmpMyNodeYP);
        //            MyNodeCH chFirst = utilPiping.createNodeCHbox(yp);
        //            LoadMyNodeCHValue(tmpCH, ref chFirst);

        //            nodeOut.ChildNode = chFirst;
        //        }
        //    }
        //}

        //private MyNodeYP createYPNodeGroup(tmpMyNodeYP tmpNodeYP)
        //{
        //    MyNodeYP yp = utilPiping.createNodeYP(tmpNodeYP.IsCP);

        //    foreach (tmpMyNode nd in tmpNodeYP.ChildNodes)
        //    {
        //        if (nd != null)
        //        {
        //            if (nd is tmpMyNodeIn)
        //            {
        //                RoomIndoor riItem = (nd as tmpMyNodeIn).RoomIndooItem;
        //                MyNodeIn ind = utilPiping.createNodeIn(riItem);
        //                yp.AddChildNode(ind);


        //                tmpMyLink tmpLink = (nd as tmpMyNodeIn).InLink;
        //                MyLink myLink = ind.InLink;
        //                if (myLink != null && tmpLink != null)
        //                {
        //                    LoadLinkValue(ref myLink, tmpLink);
        //                }
        //            }
        //            else if (nd is tmpMyNodeYP)
        //            {
        //                MyNodeYP chYP = createYPNodeGroup(nd as tmpMyNodeYP);
        //                yp.AddChildNode(chYP);
        //            }
        //            else if (nd is tmpMyNodeCH)
        //            {
        //                tmpMyNodeCH tmpNode = nd as tmpMyNodeCH;
        //                if (tmpNode.ChildNode is tmpMyNodeIn)
        //                {
        //                    RoomIndoor riItem = (tmpNode.ChildNode as tmpMyNodeIn).RoomIndooItem;
        //                    MyNodeIn ind = utilPiping.createNodeIn(riItem);

        //                    MyNodeCH ch = utilPiping.createNodeCHbox(ind);
        //                    LoadMyNodeCHValue(tmpNode, ref ch);


        //                    yp.AddChildNode(ch);

        //                    tmpMyLink tmpLinkIn = (tmpNode.ChildNode as tmpMyNodeIn).InLink;
        //                    MyLink linkIn = ind.InLink;
        //                    if (linkIn != null && tmpLinkIn != null)
        //                    {
        //                        LoadLinkValue(ref linkIn, tmpLinkIn);
        //                    }
        //                }
        //                else if (tmpNode.ChildNode is tmpMyNodeYP)
        //                {
        //                    MyNodeYP chYP = createYPNodeGroup(tmpNode.ChildNode as tmpMyNodeYP);
        //                    MyNodeCH ch = utilPiping.createNodeCHbox(chYP);
        //                    LoadMyNodeCHValue(tmpNode, ref ch);


        //                    yp.AddChildNode(ch);
        //                }
        //            }
        //        }
        //    }

        //    LoadNodeYPValue(ref yp, tmpNodeYP);

        //    tmpMyLink tmpLinkYP = tmpNodeYP.InLink;
        //    MyLink myLinkYP = yp.InLink;
        //    if (myLinkYP != null && tmpLinkYP != null)
        //    {
        //        LoadLinkValue(ref myLinkYP, tmpLinkYP);
        //    }
        //    return yp;
        //}

        ///// 加载 NodeYP 对象中绑定的数据
        ///// <summary>
        ///// 加载 NodeYP 对象中绑定的数据
        ///// </summary>
        ///// <param name="yp"></param>
        ///// <param name="tmpNodeYP"></param>
        //private void LoadNodeYPValue(ref MyNodeYP yp, tmpMyNodeYP tmpNodeYP)
        //{
        //    yp.CoolingCapacity = tmpNodeYP.CoolingCapacity;
        //    yp.HeatingCapacity = tmpNodeYP.HeatingCapacity;
        //    yp.Model = tmpNodeYP.Model;
        //    yp.PriceG = tmpNodeYP.PriceG;
        //    //add on 20160429 YunxiaoLin.
        //    yp.IsCoolingonly = tmpNodeYP.IsCoolingonly;
        //}

        ///// 加载 Link 对象绑定的数据
        ///// <summary>
        ///// 加载 Link 对象绑定的数据
        ///// </summary>
        ///// <param name="myLink"></param>
        ///// <param name="tmpLink"></param>
        //private void LoadLinkValue(ref MyLink myLink, tmpMyLink tmpLink)
        //{
        //    myLink.ElbowQty = tmpLink.ElbowQty;
        //    myLink.OilTrapQty = tmpLink.OilTrapQty;
        //    myLink.Length = tmpLink.Length;

        //    myLink.SpecG_h = tmpLink.SpecG_h;
        //    myLink.SpecG_l = tmpLink.SpecG_l;
        //    myLink.SpecL = tmpLink.SpecL;
        //    myLink.ValveLength = tmpLink.ValveLength;
        //    myLink.SpecG_h_Normal = tmpLink.SpecG_h_Normal;
        //    myLink.SpecG_l_Normal = tmpLink.SpecG_l_Normal;
        //    myLink.SpecL_Normal = tmpLink.SpecL_Normal;
        //}

        //#endregion
        ///// <summary>
        ///// 将tmpMyNodeCH的属性复制到MyNodeCH
        ///// </summary>
        ///// <param name="tmpch"></param>
        ///// <param name="ch"></param>
        //private void LoadMyNodeCHValue(tmpMyNodeCH tmpch, ref MyNodeCH ch)
        //{
        //    ch.CoolingCapacity = tmpch.CoolingCapacity;
        //    ch.HeatingCapacity = tmpch.HeatingCapacity;
        //    ch.Model = tmpch.Model;
        //    ch.MaxTotalCHIndoorPipeLength = tmpch.MaxTotalCHIndoorPipeLength;
        //    ch.MaxTotalCHIndoorPipeLength_MaxIU = tmpch.MaxTotalCHIndoorPipeLength_MaxIU;
        //    ch.MaxIndoorCount = tmpch.MaxIndoorCount;

        //    if (tmpch.InLink != null)
        //    {
        //        if (ch.InLink == null)
        //            ch.InLink = new MyLink();
        //        MyLink link = ch.InLink;
        //        LoadLinkValue(ref link, tmpch.InLink);
        //    }
        //}
        ///// 将MyNodeCH的属性复制到tmpMyNodeCH add on 20160525 by Yunxiao Lin
        ///// <summary>
        ///// 将MyNodeCH的属性复制到tmpMyNodeCH
        ///// </summary>
        ///// <param name="ch"></param>
        ///// <param name="tmpch"></param>
        //private void SaveMyNodeCHValue(MyNodeCH ch, ref tmpMyNodeCH tmpch)
        //{
        //    tmpch.CoolingCapacity = ch.CoolingCapacity;
        //    tmpch.HeatingCapacity = ch.HeatingCapacity;
        //    tmpch.Model = ch.Model;
        //    tmpch.MaxTotalCHIndoorPipeLength = ch.MaxTotalCHIndoorPipeLength;
        //    tmpch.MaxTotalCHIndoorPipeLength_MaxIU = ch.MaxTotalCHIndoorPipeLength_MaxIU;
        //    tmpch.MaxIndoorCount = ch.MaxIndoorCount;

        //    if (ch.InLink != null)
        //    {
        //        if (tmpch.InLink == null)
        //            tmpch.InLink = new tmpMyLink();
        //        tmpMyLink link = tmpch.InLink;
        //        SaveLinkValue(ref link, ch.InLink);
        //    }
        //}

        #region add on 20130815 clh 在保存系统项目之前将每个系统的配管图结构转化为自定义的可序列化的对象进行保存
        /// 在保存系统项目之前将每个系统的配管图结构转化为自定义的可序列化的对象进行保存
        /// <summary>
        /// 在保存系统项目之前将每个系统的配管图结构转化为自定义的可序列化的对象进行保存
        /// </summary>
        public void DoSavePipingStructure()
        {
            PipingBLL pipBll = GetPipingBLLInstance();
            //pipBll.SaveAllPipingStructure();   //保存每个Piping图改为保存当前Piping图 deleted by Shen Junjie on 2019/04/29

            //保存当前Piping图 added by Shen Junjie on 2019/04/29
            if (currentPipingSystem != null && currentPipingSystem.MyPipingNodeOut != null)
            {
                pipBll.SavePipingStructure(currentPipingSystem);
            }
        }

        #endregion

        #endregion

        #region 绘制配管图过程

        /// 初始化Piping中的Node状态，绘制配管图之前置为 null
        /// <summary>
        /// 初始化几个 Node 变量，绘配管图之前置为 null
        /// </summary>
        private void InitPipingNodes()
        {
            addFlowPiping.Controls.Clear();
            addFlowPiping.Items.Clear();
            addFlowPiping.Images.Clear();

            selNode = null;
            //selPNode = null;
            selNode2 = null;
            aimItem = null;
            //aimPNode = null;
            hoverItem = null;
            
            currentPipingSystem = null;
        }

        #endregion


        #region 配管计算
        // 执行配管计算并绑定配管数据，连接管长、管径规格等 
        /// <summary>
        /// 执行配管计算并绑定配管数据，连接管长、管径规格等
        /// </summary>
        private void DoPipingCalculation(PipingBLL pipBll, MyNodeOut nodeOut, out PipingErrors errorType)
        {
            errorType = PipingErrors.OK;
            if (nodeOut.ChildNode == null) return;
            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            //getSumCalculation(ref firstDstNode, factoryCode, type, unitType);

            pipBll.GetSumCapacity(nodeOut.ChildNode);

            pipBll.IsBranchKitNeedSizeUp(curSystemItem);

            PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is MyNodeYP)
            {
                MyNodeYP nodeYP = nodeOut.ChildNode as MyNodeYP;
                if (nodeYP.IsCP)
                {
                    //第一分歧管可能是梳形管 20170711 by Yunxiao Lin
                    firstBranchKit = pipBll.getFirstHeaderBranchPipeCalculation(nodeYP, curSystemItem, out errorType);
                }
                else
                {
                    // 第一分歧管放大一号计算
                    firstBranchKit = pipBll.getFirstPipeCalculation(nodeYP, curSystemItem, out errorType);
                }
                if (errorType != PipingErrors.OK)
                {
                    SetSystemPipingOK(curSystemItem, false);
                    return;
                }
            }
            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            var L2SizeDownRule = pipBll.GetL2SizeDownRule(curSystemItem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            pipBll.GetSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, curSystemItem, L2SizeDownRule, out errorType);
            if (errorType != PipingErrors.OK)
            {
                SetSystemPipingOK(curSystemItem, false);
                return;
            }
            
            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut); 
        }

        #endregion

        #region 拖拽节点时的辅助方法

        UtilPiping utilPiping = new UtilPiping();

        // for 配管图拖拽
        MyNode selNode = null;
        //Node selPNode = null;
        /// <summary>
        /// 右键点击的节点
        /// </summary>
        Node selNode2 = null;
        Item aimItem = null;
        //Node aimPNode = null;
        Item hoverItem = null;
        Point scrollPosition;


        /// 检测鼠标是否移动到指定 Node 区域内
        /// <summary>
        /// 检测鼠标是否移动到指定 Node 区域内
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isLeftButton">是否按下鼠标左键</param>
        public void CheckPipingNodeHover(Item item, bool isLeftButton)
        {
            if (item == hoverItem)
            {
                return;
            }

            MyNode node = null;
            Link lk = null;
            PipingBLL pipBll = GetPipingBLLInstance();
            if (hoverItem != null)
            {
                utilPiping.cancelItemHover(hoverItem);
                hoverItem = null;
            }
            aimItem = null;
            //aimPNode = null;

            if (item == null) return;
            
            if (item is MyNode)
            {
                node = item as MyNode;
                if (node.InLinks.Count > 0)
                {
                    lk = node.InLinks[0];
                }
            }
            else if (item is MyLink)
            {
                lk = item as Link;
                node = lk.Dst as MyNode;
            }
            if (node == null || lk == null)
            {
                return;
            }
            if (node == selNode || (isLeftButton && selNode is MyNodeOut))
            {
                // 鼠标移到选中的节点上方时，不变色
                return;
            }

            if (node is MyNodeYP || node is MyNodeIn || node is MyNodeCH || node is MyNodeMultiCH)
            {
                if (isLeftButton && selNode != null)
                {
                    if (!utilPiping.isContain(selNode, node))
                    {
                        aimItem = item;
                    }
                    else
                    {
                        return;
                    }
                }
                if (isLeftButton && node is MyNodeIn && selNode is MyNodeIn)
                {
                    //将indoor拖动到indoor时，目标是link还是node会有不同的结果 add on 2018/5/18 by Shen Junjie
                    utilPiping.setItemHover(item);
                    hoverItem = item;
                }
                else
                {
                    utilPiping.setItemHover(lk);
                    utilPiping.setItemHover(node);
                    hoverItem = item;
                }
            }
        }

        // 显示、隐藏 遮罩图层
        /// <summary>
        /// 显示、隐藏 遮罩图层
        /// </summary>
        /// <param name="isVisible"></param>
        private void showOverBox(bool isVisible)
        {
            pbOver.Visible = isVisible;
            if (isVisible)
            {
                pbOver.Location = addFlowPiping.Location;
                pbOver.Size = addFlowPiping.Size;
                pbOver.SendToBack();
            }
        }

        // 绘制拖拽时产生的虚线，暂未实现
        /// <summary>
        /// 绘制拖拽时产生的虚线，暂未实现
        /// </summary>
        private void drawDashLine()
        {
            pbOver.Refresh(); //  强制刷新pbOver
        }


        #endregion

        #endregion

        #region Main_Wiring
        /// 保存配线图中每条连接线上的点坐标
        /// <summary>
        /// 保存配线图中每条连接线上的点坐标
        /// </summary>
        List<PointF[]> ptArrayList = new List<PointF[]>();
        List<PointF[]> ptArrayList_RC = new List<PointF[]>();
        /// 保存配线图中每虚线上的点坐标
        /// <summary>
        /// 保存配线图中每虚线上的点坐标
        /// </summary>
        List<PointF[]> ptArrayList_power = new List<PointF[]>();

        List<PointF[]> ptArrayList_ground = new List<PointF[]>();
        /// <summary>
        /// 室内机总电源线坐标列表
        /// </summary>
        List<PointF[]> ptArrayList_mainpower = new List<PointF[]>();
        /// <summary>
        /// 室内机总电源类型列表(R S T/L N/L1 L2 L3 N)
        /// </summary>
        List<string> strArrayList_powerType = new List<string>();
        /// <summary>
        /// 室内机总电源电压列表
        /// </summary>
        List<string> strArrayList_powerVoltage = new List<string>();
        /// <summary>
        /// 室内机总电源电流列表
        /// </summary>
        List<double> dArrayList_powerCurrent = new List<double>();

        /// <summary>
        /// 初始化配线界面，清空Node和Line
        /// </summary>
        /// <param name="addFlowWiring"></param>
        private void InitWiringNodes(AddFlow addFlowWiring)
        {
            addFlowWiring.Controls.Clear();
            addFlowWiring.Nodes.Clear();
            addFlowWiring.AutoScrollPosition = new Point(0, 0);
        }
        /// <summary>
        /// 绘制配线图
        /// </summary>
        private void DoDrawingWiring(TreeNode tnOut)
        {
            if (tnOut == null) return;
            if (this.tabControl1.SelectedTab.Name != "tpgWiring" && this.tabControl1.SelectedTab.Name != "tpgReport")
                return;
            SystemVRF sysItem = tnOut.Tag as SystemVRF;
            if (sysItem == null || sysItem.OutdoorItem == null)
                return;
            curSystemItem = sysItem;
            string imgDir = MyConfig.WiringNodeImageDirectory;
             
            InitWiringNodes(addFlowWiring); 
            if (addFlowPiping.Nodes.Count == 0)
                BindPipingDrawing(tnOut);

            ControllerWiringBLL bll = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
            bll.CreateWiringNodeStructure(sysItem); 

            ptArrayList.Clear();
            ptArrayList_RC.Clear();
            ptArrayList_power.Clear();
            ptArrayList_ground.Clear();
            ptArrayList_mainpower.Clear();
            strArrayList_powerType.Clear();
            strArrayList_powerVoltage.Clear();
            dArrayList_powerCurrent.Clear();

            // 当删除所有的室内机后，curSystemItem.MyWiringNodeOut即为空 
            if (sysItem == null || sysItem.MyWiringNodeOut == null)
                return;

            //----------------
            // 检测是否可以绘图
            WiringNodeOut nodeOut = sysItem.MyWiringNodeOut;
            if (nodeOut.ChildNodes.Count == 0 || sysItem.OutdoorItem == null)
            {
                DrawDiagError();
                return;
            }

            if (PipingBLL.IsHeatRecovery(sysItem))
            {
                DoDrawingHRWiring(sysItem, nodeOut, imgDir);
            }
            else
            {
                DoDrawingWiring(sysItem, nodeOut, imgDir);
            }
        }
        /// 绘制Wiring图例 add on 20160524 by Yunxiao Lin
        /// <summary>
        /// 绘制Wiring图例
        /// </summary>
        private void DrawWiringLegend(WiringNodeOut nodeOut)
        {
            //CAD显示中文有问题，这里一律改成英文
            //string text0 = Msg.GetResourceString("WIRING_LEGEND_TITLE");
            //string text1 = Msg.GetResourceString("WIRING_LEGEND_1");
            //string text2 = Msg.GetResourceString("WIRING_LEGEND_2");
            //string text3 = Msg.GetResourceString("WIRING_LEGEND_3");
            string text0 = "";
            string text1 = "Transmission Line";
            string text2 = "Electrical power line";
            string text3 = "Ground wire";
            PointF ptf1 = new PointF(0, nodeOut.Size.Height + 90f);
            utilPiping.createTextNode_wiring(text0, ptf1, nodeOut);
            ptf1 = new PointF(105, ptf1.Y + 15f);
            utilPiping.createTextNode_wiring(text1, ptf1, nodeOut);
            ptf1 = new PointF(105, ptf1.Y + 30f);
            utilPiping.createTextNode_wiring(text2, ptf1, nodeOut);
            ptf1 = new PointF(105, ptf1.Y + 30f);
            utilPiping.createTextNode_wiring(text3, ptf1, nodeOut);
            PointF ptf2 = new PointF(nodeOut.Location.X, nodeOut.Location.Y + nodeOut.Size.Height + 110f);
            PointF ptf3 = new PointF(ptf2.X + 100, ptf2.Y);
            ptArrayList.Add(new PointF[] { ptf2, ptf3 });
            ptf2 = new PointF(ptf2.X, ptf2.Y + 30f);
            ptf3 = new PointF(ptf2.X + 100, ptf2.Y);
            ptArrayList_power.Add(new PointF[] { ptf2, ptf3 });
            ptf2 = new PointF(ptf2.X, ptf2.Y + 30f);
            ptf3 = new PointF(ptf2.X + 100, ptf2.Y);
            ptArrayList_ground.Add(new PointF[] { ptf2, ptf3 });
        }
        /// <summary>
        /// 绘制Heat Recovery配线图
        /// </summary>
        /// <param name="imgDir"></param>
        private void DoDrawingHRWiring(SystemVRF sysItem, WiringNodeOut nodeOut, string imgDir)
        {
            //MyDictionaryBLL dicBll = new MyDictionaryBLL();
            //MyNodeRemoteControler_Wiring mainNodeRC = new MyNodeRemoteControler_Wiring();
            //Dictionary<MyNodeIn, MyNodeRemoteControler_Wiring> mainNodeRcDic = new Dictionary<MyNodeIn, MyNodeRemoteControler_Wiring>();
            //List<MyNodeIn> nodeDrawList = new List<MyNodeIn>();
            //int rcNum = 0;
            //int rcNumCH = 0;

            Node textNode;
            PointF ptText = new PointF();

            // 1. 绘制室外机节点
            NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_ODU(sysItem.OutdoorItem, thisProject.BrandCode);
            nodeOut.Location = new PointF(10f, UtilPiping.HeightForNodeText + (UtilPiping.OutdoorOffset_Y_wiring) * 2 + 36f); // 必须先设置好 Location
            // 设置主节点加载的图片
            string imgFile = Path.Combine(imgDir, item_wiring.KeyName + ".png");
            utilPiping.setNode_wiring(nodeOut, imgFile, addFlowWiring);

            //接地连接线
            PointF ptf1 = new PointF(nodeOut.Location.X + 140, nodeOut.Location.Y + 20);
            PointF ptf2 = new PointF(ptf1.X + 74, ptf1.Y + 4);
            PointF ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
            ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });

            //----------------
            // 2. 室外机节点文字
            string text = "";
            ptText = item_wiring.PtNodeNames[0];

            text = sysItem.Name;
            utilPiping.createTextNode_wiring(text, ptText, nodeOut);
            if (item_wiring.UnitCount > 1)
            {
                text = sysItem.OutdoorItem.AuxModelName;
                // curSystemItem.OutdoorItem.Model;
                utilPiping.createTextNode_wiring(text, item_wiring.PtNodeNames[1], nodeOut);
            }

            //----------------  
            utilPiping.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeOut);
            // 3. 室外机上的电流线,控制线，以及室外机节点中的文字
            for (int i = 0; i < item_wiring.UnitCount; ++i)
            {
                ptf1 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[i], nodeOut.Location);
                ptf2 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[i], nodeOut.Location);
                ptArrayList_power.Add(new PointF[] { ptf1, ptf2 });

                utilPiping.createTextNode_wiring(item_wiring.ModelGroup[i], item_wiring.PtModelGroup[i], nodeOut);
                //if (i < 2)
                if (i < 4) //室外机最大有4个分机 20161028 by Yunxiao Lin
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup1[i], item_wiring.PtStrGroup1[i], nodeOut);
                utilPiping.createTextNode_wiring(item_wiring.StrGroup2[i], item_wiring.PtStrGroup2[i], nodeOut);
                utilPiping.createTextNode_wiring(item_wiring.StrGroup3[i], item_wiring.PtStrGroup3[i], nodeOut);
                // 室外机电源线线型标识
                utilPiping.createTextNode_wiring(item_wiring.StrGroup4[i], item_wiring.PtStrGroup4[i], nodeOut, true);
            }

            //绘制Outdoor的子节点
            PointF ptf4 = new PointF(0, 0);
            PointF ptf5 = new PointF(0, 0);
            ptText = item_wiring.PtNodeNames[0];
            ptText.Y += UtilPiping.HeightForNodeText / 2;
            //List<Node> hrChildNodesList = getSortNodeInListByChBox(nodeOut);
            //foreach (Node node in hrChildNodesList)

            List<WiringNodeIn> wiringNodeInList = new List<WiringNodeIn>();
            DrawWiringNodes(nodeOut, nodeOut.ChildNodes, wiringNodeInList, imgDir, true);

            DrawWiringRemoteControllers(wiringNodeInList, imgDir);

            //if (mainNodeRcDic.Count > 0)  //非主共享成员
            //{
            //    foreach (MyNodeIn nodeItem in myNodeInList)
            //    {
            //        if (nodeItem.Location.Y == 0)   //node还未获取位置信息                                                                  
            //            break;

            //        if (nodeDrawList.Contains(nodeItem))   //忽略已经画过的node
            //            continue;

            //        if (nodeItem.RoomIndooItem.IndoorItemGroup != null && !nodeItem.RoomIndooItem.IsMainIndoor)   //非主室内机共享控制器成员
            //        {
            //            int index = 0;   //记录主共享组index                                    
            //            foreach (MyNodeIn mainNodeIn in mainNodeRcDic.Keys)   //对比主共享控制器Map
            //            {
            //                if (nodeItem.RoomIndooItem.IndoorItemGroup.Contains(mainNodeIn.RoomIndooItem))
            //                {
            //                    mainNodeRC = mainNodeRcDic[mainNodeIn];   //获取共享控制器的信息
            //                    float lx = mainNodeRC.Location.X + (mainNodeRC.Size.Width / 2) - 20f;
            //                    float firstLine = mainNodeRC.Location.Y + 42f;
            //                    if (index > 0)   //区分不同共享组
            //                        lx -= 10f;
            //                    //Remote Control的连接线
            //                    PointF ptf1 = new PointF(lx < 0 ? 0 : lx, firstLine < 0 ? Math.Abs(firstLine) : firstLine);
            //                    PointF ptf2 = new PointF(lx < 0 ? 0 : lx, nodeItem.Location.Y + mainNodeRC.Size.Height - 24f);
            //                    PointF ptf3 = new PointF(lx < 0 ? 0 : mainNodeRC.Location.X + (mainNodeRC.Size.Width / 2) - 210, nodeItem.Location.Y + mainNodeRC.Size.Height - 24f);
            //                    //if (rcNum > 1)   //共享多个控制器节点微调
            //                    //{
            //                    //    ptf1.X -= 20f;
            //                    //    ptf1.Y += 7f;
            //                    //    ptf2.X -= 20f;
            //                    //    ptf3.X -= 20f;
            //                    //}

            //                    ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3 });
            //                    nodeDrawList.Add(nodeItem);
            //                    break;
            //                }
            //                index++;

            //            }
            //        }
            //    }

            //    //mainNodeRC = new MyNodeRemoteControler_Wiring();
            //}

            //室内机总电源参数
            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                ptArrayList_power.Add(ptArrayList_mainpower[i]); //室内机右侧电源线汇总
                //电源参数汇总
                ptf4 = ptArrayList_mainpower[i][1];
                text = strArrayList_powerVoltage[i] + "/" + dArrayList_powerCurrent[i].ToString() + "A";
                textNode = utilPiping.createTextNode_wiring(text, new PointF(ptf4.X + 122, ptf4.Y + 2));
                addFlowWiring.Nodes.Add(textNode);
                text = strArrayList_powerType[i];
                textNode = utilPiping.createTextNode_wiring(text, new PointF(ptf4.X + 166, ptf4.Y - 12));
                addFlowWiring.Nodes.Add(textNode);
            }
            //ptf4 = new PointF(ptf5.X, nodeOut.HRChildNodes[0].Location.Y - (nodeOut.HRChildNodes[0].Size.Height / 2) + 5);
            //ptf6 = new PointF(ptf5.X + 90, ptf4.Y);
            //ptArrayList_power.Add(new PointF[] { ptf5, ptf4, ptf6 }); // 节点右侧的电流汇总，虚线

            ////----------------
            //// 9. 电流汇总显示
            //text = sumCurrent.ToString() + "A";
            //textNode = createTextNode_wiring(text, new PointF(ptf4.X + 12, ptf4.Y + 2));
            //textNode.Location = new PointF(ptf4.X + 12, ptf4.Y + 2);
            //addFlowWiring.Nodes.Add(textNode);

            ////绘制Remote Controler
            //MyNodeRemoteControler_Wiring nodeRC = DrawRemoteControlerWiring(nodeOut, imgDir);
            //if (nodeRC != null)
            //{
            //    //Remote Controler接地
            //    MyNodeGround_Wiring nodeground = new MyNodeGround_Wiring();
            //    nodeground.Location = new PointF(nodeRC.Location.X + 22, nodeRC.Location.Y + nodeRC.Size.Height+20);
            //    imgFile = Path.Combine(imgDir, "Ground.png");
            //    utilPiping.setNode_wiring(nodeground, imgFile, ref addFlowWiring);
            //    nodeground.DrawColor = Color.Transparent;
            //    //接地连接线
            //    PointF ptf1 = new PointF(nodeRC.Location.X + 11, nodeRC.Location.Y + nodeRC.Size.Height);
            //    PointF ptf2 = new PointF(nodeRC.Location.X + 32, nodeRC.Location.Y + nodeRC.Size.Height + 20);
            //    ptArrayList_ground.Add(new PointF[] { ptf1, ptf2 });
            //    //Remote Controler 连接总线
            //    ptf1 = new PointF(lastIn.Location.X + lastIn.Size.Width - 1, nodeRC.Location.Y + nodeRC.Size.Height);
            //    ptf2 = new PointF(ptf1.X, lastIn.Location.Y + 12);
            //    ptArrayList.Add(new PointF[] { ptf1, ptf2 });
            //}
            //----------------
            //绘制图例
            DrawWiringLegend(nodeOut);
            //----------------

            // 10.绘制连线
            // 实线
            foreach (PointF[] pt in ptArrayList)
            {
                if (pt[0].Y < 0)
                {
                    pt[0].Y = 0;
                }

                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[pt.Length - 1]);
                addFlowWiring.Nodes.Add(nd2);
                Link lnk1 = utilPiping.createLine();
                lnk1.Jump = Jump.Arc; //add by Shen Junjie on 2017/12/30
                nd1.OutLinks.Add(lnk1, nd2);
                if (pt.Length > 2)
                    lnk1.Points.Add(pt[1]);
                if (pt.Length > 3)
                    lnk1.Points.Add(pt[2]);
            }

            foreach (PointF[] pt in ptArrayList_RC)
            {
                if (pt[0].Y < 0)
                {
                    pt[0].Y = 0;
                }

                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[pt.Length - 1]);
                addFlowWiring.Nodes.Add(nd2);
                Link lnk1 = utilPiping.createLine();
                lnk1.Jump = Jump.Arc; //add by Shen Junjie on 2017/12/30
                nd1.OutLinks.Add(lnk1, nd2);
                if (pt.Length > 2)
                    lnk1.Points.Add(pt[1]);
                if (pt.Length > 3)
                    lnk1.Points.Add(pt[2]);
            }

            // 电源线用红色粗实线 modify on 20160521 by Yunxiao Lin
            foreach (PointF[] pt in ptArrayList_power)
            {
                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[pt.Length - 1]);
                Link lnk1 = utilPiping.createLine();
                if (pt.Length > 2)
                    lnk1.Line.Style = LineStyle.VH;
                lnk1.Jump = Jump.Arc;
                lnk1.DrawWidth = 1;
                lnk1.DrawColor = Color.Red;
                nd1.OutLinks.Add(lnk1, nd2);
            }
            //接地线用虚线 add on 20160523 by Yunxiao Lin
            foreach (PointF[] pt in ptArrayList_ground)
            {
                if (pt.Length > 2)
                {
                    MyNodeGround_Wiring nodeground = new MyNodeGround_Wiring();
                    nodeground.Location = pt[2];
                    imgFile = Path.Combine(imgDir, "Ground.png");
                    utilPiping.setNode_wiring(nodeground, imgFile, addFlowWiring);
                    nodeground.DrawColor = Color.Transparent;
                }
                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[1]);
                Link lnk1 = utilPiping.createLine();
                lnk1.DashStyle = DashStyle.Dash;
                nd1.OutLinks.Add(lnk1, nd2);
            }
        }

        private void DrawWiringNodes(Node parent, List<WiringNode> nodes, List<WiringNodeIn> myNodeInList, string imgDir, bool isHR)
        {
            bool prevBrotherNodeIsIndoor = false; //上一个同级别节点是否是Indoor
            double sumCurrent = 0.0d; //室内机电源总安培数
            WiringNodeIn lastIn = null; //最后一台室内机
            PointF ptf1, ptf2, ptf3, ptf4, ptf5, ptf6;
            NodeElement_Wiring item_wiring;
            string imgFile;
            int lastYIndex = 0;
            int index1 = 0;
            foreach (WiringNode node in nodes)   //去除排序：CHBOX可能会接多台室内机，无法进行共享控制器规则的排序
            {
                if (node is WiringNodeCH)
                {
                    prevBrotherNodeIsIndoor = false;
                    //绘制CH-Box
                    WiringNodeCH nodeCH = node as WiringNodeCH;
                    item_wiring = utilPiping.GetNodeElement_Wiring_CH(nodeCH.Model, nodeCH.PowerSupply, nodeCH.PowerLineType, nodeCH.PowerCurrent);
                    //current = nodeIn.RoomIndooItem.IndoorItem.RatedCurrent; //CH-Box暂时没有电流指标 
                    //sumCurrent += current;
                    nodeCH.Location = utilPiping.getLocationChild_wiring(parent, node, index1, isHR);
                    nodeCH.Text = item_wiring.ShortModel;
                    //// 设置主节点加载的图片
                    //imgFile = Path.Combine(imgDir, item_wiring.KeyName + ".png");
                    //utilPiping.setNode_wiring(nodeCH, imgFile, addFlowWiring);
                    utilPiping.setNode_wiring(nodeCH, new SizeF(80, 52), addFlowWiring);
                    // CH节点文字
                    utilPiping.createTextNode_wiring("CH Unit", new PointF(86, -13), nodeCH);
                    //createTextNode_wiring(current.ToString() + "A", item_wiring.PtStrGroup3[0], nodeCH);

                    //绘制CH-Box下的Indoor
                    DrawWiringNodes(nodeCH, nodeCH.ChildNodes, myNodeInList, imgDir, isHR);

                    //生成连接线的坐标集合
                    float x = parent.Location.X + parent.Size.Width - 1;
                    float y = nodeCH.Location.Y + nodeCH.Size.Height - 2; // 因为CH节点的高度为52
                    float enlargedHeight = 0;
                    if (nodeCH.IsMultiCHBox && nodeCH.ChildNodes.Count > 1)
                    {
                        enlargedHeight = UtilPiping.VDistanceVertical_wiring * (nodeCH.ChildNodes.Count - 1);
                        nodeCH.Size = new SizeF(nodeCH.Size.Width, nodeCH.Size.Height + enlargedHeight);
                    }

                    //Change the wiring terminal name of Single CH-Box for all regions. modified by Shen Junjie on 2018/04/16
                    //Single CH Box 1,2 is connected to Outdoor unit 1,2				
                    //Single CH Box 3,4 is connected to Indoor unit 1,2
                    if (nodeCH.IsMultiCHBox)
                    {
                        int newBranchIndex = 0;
                        for (int i = 0; i < nodeCH.ChildNodes.Count; i++)
                        {
                            WiringNodeIn wiringIn = nodeCH.ChildNodes[i] as WiringNodeIn;
                            if (wiringIn != null && wiringIn.IsNewBranchOfParent)
                            {
                                utilPiping.createTextNode_wiring((newBranchIndex * 2 + 3) + " " + (newBranchIndex * 2 + 4)
                                    , new PointF(142, UtilPiping.VDistanceVertical_wiring * i), nodeCH);
                                newBranchIndex++;
                            }
                        }
                    }
                    else
                    {
                        utilPiping.createTextNode_wiring(item_wiring.StrGroup1[0], item_wiring.PtStr1, nodeCH);
                    }
                    //utilPiping.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStrGroup1[0], nodeCH);

                    utilPiping.createTextNode_wiring(item_wiring.Str1, new PointF(35, nodeCH.Size.Height - enlargedHeight - 14), nodeCH);
                    //createTextNode_wiring(item_wiring.StrGroup2[0], item_wiring.PtStrGroup2[0], nodeCH);

                    if (index1 == 0)
                    {
                        ptf1 = new PointF(x, y);
                        ptf2 = new PointF(nodeCH.Location.X + 1, y);
                        ptArrayList.Add(new PointF[] { ptf1, ptf2 });
                    }
                    else
                    {
                        x = nodeCH.Location.X;
                        ptf1 = new PointF(x, y);
                        ptf2 = new PointF(x - 60, y);
                        ptf3 = new PointF(x - 60, y - 15);
                        ptf4 = new PointF(x, y - UtilPiping.VDistanceVertical_wiring * (index1 - lastYIndex));
                        ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3, ptf4 });
                    }

                    //接地连接线
                    ptf1 = new PointF(nodeCH.Location.X + 140 - 60, nodeCH.Location.Y + 20);
                    ptf2 = new PointF(ptf1.X + 74, ptf1.Y + 4);
                    ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
                    ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });


                    //电源线型号
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup2[0], item_wiring.PtStrGroup2[0], nodeCH);
                    //电源线电压
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup3[0], item_wiring.PtStrGroup3[0], nodeCH);
                    //电源线类型
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup4[0], item_wiring.PtStrGroup4[0], nodeCH, true);

                    //CH Box分支电源线
                    ptf4 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[0], nodeCH.Location);
                    ptf5 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[0], nodeCH.Location);
                    ptf4.X -= 60;
                    ptf5.X -= 60;
                    ptArrayList_power.Add(new PointF[] { ptf4, ptf5 });

                    lastYIndex = index1;
                    if (nodeCH.ChildNodes.Count > 1)
                    {
                        index1 += nodeCH.ChildNodes.Count - 1;
                    }
                    index1++;
                }
                else if (node is WiringNodeIn)
                {
                    //绘制Outdoor下的Indoor
                    WiringNodeIn nodeIn = node as WiringNodeIn;
                    lastIn = nodeIn;
                    int powerIndex = 0;
                    bool isNewPower = false;
                    item_wiring = utilPiping.GetNodeElement_Wiring_IDU(nodeIn.RoomIndoorItem.IndoorItem, thisProject.BrandCode, curSystemItem.OutdoorItem.Type, ref strArrayList_powerType, ref strArrayList_powerVoltage, ref dArrayList_powerCurrent, ref powerIndex, ref isNewPower);
                    double current = nodeIn.RoomIndoorItem.IndoorItem.RatedCurrent;
                    sumCurrent += current;
                    nodeIn.Location = utilPiping.getLocationChild_wiring(parent, node, index1, isHR);
                    nodeIn.Text = item_wiring.ShortModel;
                    // 设置主节点加载的图片
                    imgFile = Path.Combine(imgDir, item_wiring.KeyName + ".png");
                    utilPiping.setNode_wiring(nodeIn, imgFile, addFlowWiring);

                    //----------------
                    // 6. 室内机节点文字                
                    string indoorName = nodeIn.RoomIndoorItem.IndoorName;         
                    if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.RoomID))
                    {
                        RoomLoadIndexBLL roomBill = new RoomLoadIndexBLL();
                        string floorRoomName = roomBill.getFloorRoomName(nodeIn.RoomIndoorItem, thisProject);
                        //如果当前室内机是新风机 取房间名称 on 20180903 by xyj
                        if (nodeIn.RoomIndoorItem.IsFreshAirArea)
                        {
                            floorRoomName = nodeIn.RoomIndoorItem.RoomName;
                        }
                        //foreach (Floor f in thisProject.FloorList)
                        //{
                        //    foreach (Room rm in f.RoomList)
                        //    {
                        //        if (rm.Id == nodeIn.RoomIndoorItem.RoomID)
                        //        {
                        //            floorName = f.Name;
                        //        }
                        //    }
                        //}
                        //indoorName = floorName + ":" + nodeIn.RoomIndoorItem.RoomName + ":" + indoorName;
                        indoorName = floorRoomName + ":" + indoorName;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.DisplayRoom))
                        {
                            indoorName = nodeIn.RoomIndoorItem.DisplayRoom + ":" + indoorName;
                        }
                    }
                    utilPiping.createTextNode_wiring(indoorName, new PointF(66, -13), nodeIn);
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup3[0], item_wiring.PtStrGroup3[0], nodeIn);

                    utilPiping.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeIn);
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup1[0], item_wiring.PtStrGroup1[0], nodeIn);
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup2[0], item_wiring.PtStrGroup2[0], nodeIn);
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup4[0], item_wiring.PtStrGroup4[0], nodeIn, true);


                    //----------------
                    // 8.生成连接线的坐标点集合
                    float x = parent.Location.X + parent.Size.Width - 1;
                    float y = nodeIn.Location.Y + nodeIn.Size.Height - 2; // 因为室内机节点的高度为52
                    if (isHR && parent is WiringNodeOut && !prevBrotherNodeIsIndoor)
                    {
                        //第一个cooling only的indoor连接到最后一个CH Box的加长连接线
                        x = nodeIn.Location.X;
                        ptf1 = new PointF(x, y);
                        ptf2 = new PointF(x - UtilPiping.HDistanceVertical_wiring - 60, y);
                        ptf3 = new PointF(x - UtilPiping.HDistanceVertical_wiring - 60, y - 15);
                        ptf4 = new PointF(x - UtilPiping.HDistanceVertical_wiring, y - UtilPiping.VDistanceVertical_wiring * (index1 - lastYIndex));
                        ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3, ptf4 });
                    }
                    else
                    {
                        if (index1 == 0 || nodeIn.IsNewBranchOfParent)
                        { 
                            //indoor与outdoor/CH Box的水平连接线
                            ptf1 = new PointF(x, y);
                            ptf2 = new PointF(nodeIn.Location.X + 1, y);
                            ptArrayList.Add(new PointF[] { ptf1, ptf2 });
                        }
                        else
                        {
                            //indoor与上一个indoor的连接线
                            x = nodeIn.Location.X;
                            ptf1 = new PointF(x, y);
                            ptf2 = new PointF(x, y - 15);
                            ptf3 = new PointF(x + 60, y - UtilPiping.VDistanceVertical_wiring);
                            ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3 });
                        }
                    }
                    
                    ////判断Indoor是否当前层最后一个节点
                    //if (node == nodeOut.HRChildNodes.Last())
                    //{
                    //    //新增接地线节点
                    //    x = nodeIn.Location.X;
                    //    y = nodeIn.Location.Y;
                    //    ptf3 = new PointF(x + 20, y + 82);
                    //    //接地连接线
                    //    ptf1 = new PointF(x + 60, y + 51);
                    //    ptf2 = new PointF(x + 30, y + 82);
                    //    ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });
                    //}

                    //接地连接线  
                    ptf1 = new PointF(nodeIn.Location.X + 140, nodeIn.Location.Y + 20);
                    ptf2 = new PointF(ptf1.X + 74, ptf1.Y + 4);
                    ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
                    ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });

                    ////绘制Remote Control
                    //if (nodeIn.RoomIndooItem.ListAccessory != null && nodeIn.RoomIndooItem.ListAccessory.Count > 0)
                    //{
                    //    int RCIndex = 0;
                    //    foreach (Accessory acc in nodeIn.RoomIndooItem.ListAccessory)
                    //    {
                    //        if (acc.Type.ToLower() == "remote controler" || acc.Type.ToLower() == "remote control switch" || acc.Type.ToLower() == "half-size remote control switch" || acc.Type.ToLower() == "receiver kit for wireless control")
                    //        {
                    //            MyNodeRemoteControler_Wiring nodeRC = DrawWiringRCNode(nodeIn, acc, RCIndex, imgDir);

                    //            if (nodeIn.RoomIndooItem.IsMainIndoor)
                    //            {
                    //                if (RCIndex == 0)
                    //                {
                    //                    mainNodeRcDic.Add(nodeIn, nodeRC);
                    //                }
                    //                //mainNodeRC = nodeRC;   //主室内机共享的控制器
                    //            }
                    //            if (nodeRC != null)
                    //            {
                    //                //Remote Control的连接线
                    //                PointF ptf1 = new PointF(nodeRC.Location.X + (nodeRC.Size.Width / 2), nodeRC.Location.Y + nodeRC.Size.Height);
                    //                PointF ptf2 = new PointF(nodeRC.Location.X + (nodeRC.Size.Width / 2), nodeRC.Location.Y + nodeRC.Size.Height + 8f);
                    //                PointF ptf3 = new PointF(nodeRC.Location.X + (nodeRC.Size.Width / 2) - 82f, nodeRC.Location.Y + nodeRC.Size.Height + 8f);
                    //                if (RCIndex == 0)
                    //                    ptf3.X -= 73f;
                    //                ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3 });

                    //                //Remote Control的接地线
                    //                if (RCIndex == 0)
                    //                {
                    //                    ptf1 = new PointF(nodeIn.Location.X + 140, nodeIn.Location.Y + 12);
                    //                    ptf2 = new PointF(ptf1.X + 74, ptf1.Y + 12);
                    //                    ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
                    //                    ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });
                    //                }
                    //                RCIndex++;
                    //            }
                    //            rcNum = RCIndex;
                    //        }
                    //    }
                    //}

                    myNodeInList.Add(nodeIn);


                    //室内机分支电源线
                    ptf4 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[0], nodeIn.Location);
                    ptf5 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[0], nodeIn.Location);
                    ptf5.X += (10f * powerIndex) + 55f;
                    ptArrayList_power.Add(new PointF[] { ptf4, ptf5 });
                    //室内机主电源线
                    if (isNewPower)
                    {
                        ptf4 = new PointF(ptf5.X, nodeIn.Location.Y + (nodeIn.Size.Height / 2));
                        ptf6 = new PointF(ptf5.X + 240, ptf4.Y);
                        ptArrayList_mainpower.Add(new PointF[] { ptf5, ptf4, ptf6 });
                    }
                    else
                    {
                        ptArrayList_mainpower[powerIndex][0] = ptf5;
                    }

                    prevBrotherNodeIsIndoor = true;
                    lastYIndex = index1;
                    index1++;
                }
            }
        }

        /// <summary>
        /// 绘制单个Remote Control Switch
        /// </summary>
        /// <param name="nodeIn">室内机节点</param>
        /// <param name="RC">室内机的Remote Control Switch附件</param>
        /// <param name="RCIndex"></param>
        /// <param name="imgDir"></param>
        /// <returns></returns>
        private MyNodeRemoteControler_Wiring DrawWiringRCNode(WiringNodeIn nodeIn, Accessory RC, int RCIndex, string imgDir)
        {
            MyNodeRemoteControler_Wiring nodeRC = null;
            float x = nodeIn.Location.X + nodeIn.Size.Width + ((RCIndex + 1) * 82f) + 60f;
            float y = nodeIn.Location.Y - 32f;

            //bool isCovered = true;  //是否被其它共享控制器的垂直连线给占用
            //while (isCovered)
            //{
            //    isCovered = false;
            //    foreach (PointF[] pts in ptArrayList)
            //    {
            //        float yMin = Math.Min(pts[0].Y, pts[1].Y);
            //        float yMax = Math.Max(pts[0].Y, pts[1].Y);
            //        //如果有垂直线条经过此RC的位置，则RemoteController右移一格
            //        if (pts[0].X >= x && pts[0].X <= x + 30 && yMin <= y && yMax >= y)
            //        {
            //            x += 82;
            //            isCovered = true;
            //        }
            //    }
            //}

            string model = "";
            if (RC.BrandCode == "Y")
                model = RC.Model_York;
            else
                model = RC.Model_Hitachi;
            if (model != "")
            {
                //增加Remote Controler节点
                nodeRC = new MyNodeRemoteControler_Wiring();
                nodeRC.Location = new PointF(x, y);
                string imgFile = Path.Combine(imgDir, "RemoteControler.png");
                utilPiping.setNode_wiring(nodeRC, imgFile, addFlowWiring);
                nodeRC.DrawColor = Color.Transparent;
                //增加Remote Controler型号
                PointF ptf = new PointF(nodeRC.Size.Width, 5);
                utilPiping.createTextNode_wiring(model, ptf, nodeRC);
                ////增加Remote Controler名称
                //ptf = new PointF(-35, -15);
                //createTextNode_wiring("Remote Controler", ptf, nodeRC);

                //Shared标注
                if (RC.IsShared)
                {
                    utilPiping.createTextNode_wiring("Shared", new PointF(ptf.X, ptf.Y + 15), nodeRC);
                }
            }
            return nodeRC;
        }
        ///// 在布线图中绘制Remote Controler add on 20160523 by Yunxiao Lin
        ///// <summary>
        ///// 在布线图中绘制Remote Controler
        ///// </summary>
        ///// <param name="nodeOut"></param>
        ///// <param name="imgDir"></param>
        ///// <returns></returns>
        //private MyNodeRemoteControler_Wiring DrawRemoteControlerWiring(MyNodeOut_Wiring nodeOut, string imgDir)
        //{
        //    MyNodeRemoteControler_Wiring nodeRC = null;
        //    float x = 0.0f;
        //    float y = 0.0f;
        //    Dictionary<string, int> rcdic = new Dictionary<string, int>();
        //    if (nodeOut.NodeInList != null && nodeOut.NodeInList.Length > 0)
        //    {
        //        //Heat Pump
        //        foreach (MyNodeIn nodeIn in nodeOut.NodeInList)
        //        {
        //            if (nodeIn == nodeOut.NodeInList[0])
        //            {
        //                x = nodeIn.Location.X + nodeIn.Size.Width - 12f;
        //                y = nodeIn.Location.Y - 36f;
        //            }
        //            if (nodeIn.RoomIndooItem.ListAccessory != null && nodeIn.RoomIndooItem.ListAccessory.Count > 0)
        //            {
        //                foreach (Accessory acc in nodeIn.RoomIndooItem.ListAccessory)
        //                {
        //                    if (acc.Type.ToLower() == "remote controler" || acc.Type.ToLower() == "remote control switch")
        //                    {
        //                        string model = "";
        //                        if (acc.BrandCode == "Y")
        //                            model = acc.Model_York;
        //                        else
        //                            model = acc.Model_Hitachi;
        //                        if (rcdic.ContainsKey(model))
        //                            rcdic[model]++;
        //                        else
        //                            rcdic.Add(model, 1);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else if (nodeOut.HRChildNodes != null && nodeOut.HRChildNodes.Count > 0)
        //    {
        //        //Heat Recovery
        //        bool firstNodeIn = true;
        //        foreach (Node node in addFlowWiring.Nodes)
        //        {
        //            if (node is MyNodeIn)
        //            {
        //                MyNodeIn nodeIn = node as MyNodeIn;
        //                if (firstNodeIn)
        //                {
        //                    x = nodeIn.Location.X + nodeIn.Size.Width - 12f;
        //                    y = nodeIn.Location.Y - 36f;
        //                    firstNodeIn = false;
        //                }
        //                if (nodeIn.RoomIndooItem.ListAccessory != null && nodeIn.RoomIndooItem.ListAccessory.Count > 0)
        //                {
        //                    foreach (Accessory acc in nodeIn.RoomIndooItem.ListAccessory)
        //                    {
        //                        if (acc.Type.ToLower() == "remote controler" || acc.Type.ToLower() == "remote control switch")
        //                        {
        //                            string model = "";
        //                            if (acc.BrandCode == "Y")
        //                                model = acc.Model_York;
        //                            else
        //                                model = acc.Model_Hitachi;
        //                            if (rcdic.ContainsKey(model))
        //                                rcdic[model]++;
        //                            else
        //                                rcdic.Add(model, 1);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    string models = "";
        //    foreach (var item in rcdic)
        //    {
        //        if (models != "")
        //            models += "\n";
        //        models += item.Key;
        //        if (item.Value > 1)
        //            models += " * " + item.Value.ToString();
        //    }
        //    if (models != "")
        //    {
        //        //增加Remote Controler节点
        //        nodeRC = new MyNodeRemoteControler_Wiring();
        //        nodeRC.Location = new PointF(x, y);
        //        string imgFile = Path.Combine(imgDir, "RemoteControler.png");
        //        utilPiping.setNode_wiring(nodeRC, imgFile, ref addFlowWiring);
        //        nodeRC.DrawColor = Color.Transparent;
        //        //增加Remote Controler型号
        //        PointF ptf = new PointF(nodeRC.Size.Width, 0);
        //        createTextNode_wiring(models, ptf, nodeRC);
        //        //增加Remote Controler名称
        //        ptf = new PointF(-35, -15);
        //        createTextNode_wiring("Remote Controler", ptf, nodeRC);
        //    }
        //    return nodeRC;
        //}

        /// 绘制配线图
        /// <summary>
        /// 绘制配线图，界面显示
        /// </summary>
        private void DoDrawingWiring(SystemVRF sysItem, WiringNodeOut nodeOut, string imgDir)
        {
            //double sumCurrent = 0.0d; //室内机电源总安培数
            //MyNodeIn lastIn = null;

            Node textNode;
            PointF ptText = new PointF();

            // 1. 绘制室外机节点
            NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_ODU(sysItem.OutdoorItem, thisProject.BrandCode);
            nodeOut.Location = new PointF(10f, UtilPiping.HeightForNodeText + UtilPiping.OutdoorOffset_Y_wiring + 36f); // 必须先设置好 Location
            // 设置主节点加载的图片
            string imgFile = Path.Combine(imgDir, item_wiring.KeyName + ".png");
            utilPiping.setNode_wiring(nodeOut, imgFile, addFlowWiring);

            //接地连接线
            PointF ptf1 = new PointF(nodeOut.Location.X + 140, nodeOut.Location.Y + 20);
            PointF ptf2 = new PointF(ptf1.X + 74, ptf1.Y + 4);
            PointF ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
            ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });

            //----------------
            // 2. 室外机节点文字
            string text = "";
            ptText = item_wiring.PtNodeNames[0];
            //if (item_wiring.UnitCount == 1)
            //{
            //    //nodeOut.Text = item_wiring.ShortModel;
            //    ptText.Y += UtilPiping.HeightForNodeText / 2;

            //    // 4. 独立的室外机节点节点右下方电流参数
            //    text = current.ToString() + "A";
            //    createTextNode_wiring(text, item_wiring.PtStrGroup3[0], nodeOut);
            //}

            text = sysItem.Name;
            utilPiping.createTextNode_wiring(text, ptText, nodeOut);
            if (item_wiring.UnitCount > 1)
            {
                text = sysItem.OutdoorItem.AuxModelName;
                // curSystemItem.OutdoorItem.Model;
                utilPiping.createTextNode_wiring(text, item_wiring.PtNodeNames[1], nodeOut);
            }

            //---------------- 
            utilPiping.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeOut);
            // 3. 室外机上的电流线,虚线，以及室外机节点中的文字
            for (int i = 0; i < item_wiring.UnitCount; ++i)
            {
                ptf1 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[i], nodeOut.Location);
                ptf2 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[i], nodeOut.Location);
                ptArrayList_power.Add(new PointF[] { ptf1, ptf2 });

                //createTextNode_wiring(item_wiring.ModelGroup[i], item_wiring.PtModelGroup[i], nodeOut);
                //if (i < 4)
                //    createTextNode_wiring(item_wiring.StrGroup1[i], item_wiring.PtStrGroup1[i], nodeOut);
                //createTextNode_wiring(item_wiring.StrGroup2[i], item_wiring.PtStrGroup2[i], nodeOut);
                //createTextNode_wiring(item_wiring.StrGroup3[i], item_wiring.PtStrGroup3[i], nodeOut);
                //// 室外机电源线线型标识
                //createTextNode_wiring(item_wiring.StrGroup4[i], item_wiring.PtStrGroup4[i], nodeOut, true);

                if (i < item_wiring.ModelGroup.Length)   //判断数组是否存在越界
                    utilPiping.createTextNode_wiring(item_wiring.ModelGroup[i], item_wiring.PtModelGroup[i], nodeOut);
                if (item_wiring.ModelGroup.Length > 1 && i < item_wiring.StrGroup1.Length)  
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup1[i], item_wiring.PtStrGroup1[i], nodeOut); 
                if (i < item_wiring.StrGroup2.Length)
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup2[i], item_wiring.PtStrGroup2[i], nodeOut);
                if (i < item_wiring.StrGroup3.Length)
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup3[i], item_wiring.PtStrGroup3[i], nodeOut);
                if (i < item_wiring.StrGroup4.Length)
                    utilPiping.createTextNode_wiring(item_wiring.StrGroup4[i], item_wiring.PtStrGroup4[i], nodeOut, true); // 室外机电源线线型标识           
            }

            //----------------
            // 5. 绘制室内机节点
            PointF ptf4 = new PointF(0, 0);
            PointF ptf5 = new PointF(0, 0);
            ptText = item_wiring.PtNodeNames[0];
            ptText.Y += UtilPiping.HeightForNodeText / 2;

            //主indoor优先进行排序
            List<WiringNodeIn> sortNodeInList = getSortNodeInList(nodeOut);

            //for (int i = 0; i < nodeOut.NodeInList.Length; i++)
            if (sortNodeInList == null)
                return;

            List<WiringNodeIn> wiringNodeInList = new List<WiringNodeIn>();
            DrawWiringNodes(nodeOut, sortNodeInList.ToList<WiringNode>(), wiringNodeInList, imgDir, false);

            DrawWiringRemoteControllers(wiringNodeInList, imgDir);

            //室内机总电源参数
            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                ptArrayList_power.Add(ptArrayList_mainpower[i]); //室内机右侧电源线汇总
                //电源参数汇总
                ptf4 = ptArrayList_mainpower[i][1];
                text = strArrayList_powerVoltage[i] + "/" + dArrayList_powerCurrent[i].ToString() + "A";
                textNode = utilPiping.createTextNode_wiring(text, new PointF(ptf4.X + 122, ptf4.Y + 2));
                addFlowWiring.Nodes.Add(textNode);
                text = strArrayList_powerType[i];
                textNode = utilPiping.createTextNode_wiring(text, new PointF(ptf4.X + 166, ptf4.Y - 12));
                addFlowWiring.Nodes.Add(textNode);
            }
            //ptf4 = new PointF(ptf5.X, nodeOut.NodeInList[0].Location.Y + (nodeOut.NodeInList[0].Size.Height / 2));
            //ptf6 = new PointF(ptf5.X + 90, ptf4.Y);
            //ptArrayList_power.Add(new PointF[] { ptf5, ptf4, ptf6 }); // 节点右侧的电流汇总，虚线

            ////----------------
            //// 9. 电流汇总显示
            //text = sumCurrent.ToString() + "A";
            //textNode = createTextNode_wiring(text, new PointF(ptf4.X + 12, ptf4.Y + 2));
            //textNode.Location = new PointF(ptf4.X + 12, ptf4.Y + 2);
            //addFlowWiring.Nodes.Add(textNode);
            //---------------------
            //绘制Remote Controler
            //MyNodeRemoteControler_Wiring nodeRC = DrawRemoteControlerWiring(nodeOut, imgDir);
            //if (nodeRC != null)
            //{
            //    //Remote Controler接地
            //    MyNodeGround_Wiring nodeground = new MyNodeGround_Wiring();
            //    nodeground.Location = new PointF(nodeRC.Location.X + 22, nodeRC.Location.Y + nodeRC.Size.Height + 20);
            //    imgFile = Path.Combine(imgDir, "Ground.png");
            //    utilPiping.setNode_wiring(nodeground, imgFile, ref addFlowWiring);
            //    nodeground.DrawColor = Color.Transparent;
            //    //接地连接线
            //    PointF ptf1 = new PointF(nodeRC.Location.X + 11, nodeRC.Location.Y + nodeRC.Size.Height);
            //    PointF ptf2 = new PointF(nodeRC.Location.X + 32, nodeRC.Location.Y + nodeRC.Size.Height + 20);
            //    ptArrayList_ground.Add(new PointF[] { ptf1, ptf2 });
            //    //Remote Controler 连接总线
            //    ptf1 = new PointF(lastIn.Location.X + lastIn.Size.Width - 1, nodeRC.Location.Y + nodeRC.Size.Height);
            //    ptf2 = new PointF(ptf1.X, lastIn.Location.Y + 12);
            //    ptArrayList.Add(new PointF[] { ptf1, ptf2 });
            //}
            //----------------
            //绘制图例 add on 2060524 by Yunxiao Lin
            DrawWiringLegend(nodeOut);
            //----------------

            // 10.绘制连线
            // 实线
            foreach (PointF[] pt in ptArrayList)
            {
                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[pt.Length - 1]);
                addFlowWiring.Nodes.Add(nd2);
                Link lnk1 = utilPiping.createLine();
                nd1.OutLinks.Add(lnk1, nd2);
                if (pt.Length > 2)
                    lnk1.Points.Add(pt[1]);
            }

            foreach (PointF[] pt in ptArrayList_RC)
            {
                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[pt.Length - 1]);
                addFlowWiring.Nodes.Add(nd2);
                Link lnk1 = utilPiping.createLine();
                nd1.OutLinks.Add(lnk1, nd2);
                if (pt.Length > 2)
                    lnk1.Points.Add(pt[1]);
            }
            // 电源线用红色粗实线 modify on 20160521 by Yunxiao Lin
            foreach (PointF[] pt in ptArrayList_power)
            {
                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[pt.Length - 1]);
                Link lnk1 = utilPiping.createLine();
                if (pt.Length > 2)
                    lnk1.Line.Style = LineStyle.VH;
                lnk1.Jump = Jump.Arc;
                lnk1.DrawWidth = 1;
                lnk1.DrawColor = Color.Red;
                nd1.OutLinks.Add(lnk1, nd2);
            }
            //接地线用虚线 add on 20160523 by Yunxiao Lin
            foreach (PointF[] pt in ptArrayList_ground)
            {
                if (pt.Length > 2)
                {
                    MyNodeGround_Wiring nodeground = new MyNodeGround_Wiring();
                    nodeground.Location = pt[2];
                    imgFile = Path.Combine(imgDir, "Ground.png");
                    utilPiping.setNode_wiring(nodeground, imgFile, addFlowWiring);
                    nodeground.DrawColor = Color.Transparent;
                }
                Node nd1 = utilPiping.createLinePoint(pt[0]);
                addFlowWiring.Nodes.Add(nd1);
                Node nd2 = utilPiping.createLinePoint(pt[1]);
                Link lnk1 = utilPiping.createLine();
                lnk1.DashStyle = DashStyle.Dash;
                nd1.OutLinks.Add(lnk1, nd2);
            }
        }

        /// <summary>
        /// 绘制Remote Control 
        /// </summary>
        /// <param name="nodeIn"></param>
        /// <param name="imgDir"></param>
        private void DrawWiringRemoteControllers(List<WiringNodeIn> nodeInList, string imgDir)
        {
            List<bool[]> rcPositionTable = new List<bool[]>();
            int rowIndex = 0;
            foreach (WiringNodeIn nodeIn in nodeInList)
            {
                if (nodeIn.RoomIndoorItem == null || nodeIn.RoomIndoorItem.IndoorItem == null) continue;

                MyNodeRemoteControler_Wiring shardRCNode = null;
                List<Accessory> accList = nodeIn.RoomIndoorItem.ListAccessory;
                List<RoomIndoor> indoorGroup = nodeIn.RoomIndoorItem.IndoorItemGroup;
                if (accList != null && accList.Count > 0)
                {
                    int RCIndex = 0;
                    if (nodeIn.RoomIndoorItem.IsMainIndoor)
                    {
                        //兼容老项目数据, 如果不存在共享Accessory，则设置第一个是共享Accessory
                        if (!accList.Exists(acc => acc.IsShared))
                        {
                            accList[0].IsShared = true;
                        }
                    }

                    foreach (Accessory acc in accList)
                    {
                        if (acc.Type.ToLower() == "remote controler"
                            || acc.Type.ToLower() == "remote control switch"
                            || acc.Type.ToLower() == "half-size remote control switch"
                            || acc.Type.ToLower() == "receiver kit for wireless control"
                            || acc.Type.Contains("有線遙控器") //增加TW 控制器类型 20180728 by Yunxiao Lin
                            || acc.Type == "受光器"
                            )
                        {
                            int colIndex = RCIndex;
                            bool isCovered = true;

                            int firstShare = rowIndex;
                            int lastShare = rowIndex;
                            if (acc.IsShared && indoorGroup != null)
                            {
                                for (int index1 = 0; index1 < nodeInList.Count; index1++)
                                {
                                    WiringNodeIn n = nodeInList[index1];
                                    if (indoorGroup.Contains(n.RoomIndoorItem))
                                    {
                                        firstShare = Math.Min(firstShare, index1);
                                        lastShare = Math.Max(lastShare, index1);
                                    }
                                }
                            }
                            while (isCovered)
                            {
                                isCovered = false;
                                if (rcPositionTable.Count - 1 < colIndex)
                                {
                                    rcPositionTable.Add(new bool[nodeInList.Count]);
                                }
                                for (int index1 = firstShare; index1 <= lastShare; index1++)
                                {
                                    if (rcPositionTable[colIndex][index1])
                                    {
                                        isCovered = true;
                                        break;
                                    }
                                }
                                if (isCovered)
                                {
                                    colIndex++;
                                }
                            }
                            for (int index1 = firstShare; index1 <= lastShare; index1++)
                            {
                                rcPositionTable[colIndex][index1] = true;
                            }
                            
                            MyNodeRemoteControler_Wiring nodeRC = DrawWiringRCNode(nodeIn, acc, colIndex, imgDir);
                            if (nodeIn.RoomIndoorItem.IsMainIndoor)
                            {
                                if (acc.IsShared)
                                {
                                    shardRCNode = nodeRC;   //主室内机共享的控制器
                                }
                            }

                            if (nodeRC != null)
                            {
                                //Remote Control的连接线
                                PointF ptf1 = new PointF(nodeRC.Location.X + (nodeRC.Size.Width / 2), nodeRC.Location.Y + nodeRC.Size.Height);
                                PointF ptf2 = new PointF(nodeRC.Location.X + (nodeRC.Size.Width / 2), nodeIn.Location.Y + 12);
                                PointF ptf3 = new PointF(nodeIn.Location.X + nodeIn.Size.Width - 1, nodeIn.Location.Y + 12);
                                ptArrayList_RC.Add(new PointF[] { ptf1, ptf2, ptf3 });
                                //earth shouldn’t be connected to remote control. modified by Shen Junjie on 2018/6/6
                                ////Remote Control的接地线
                                ////ptf1 = new PointF(nodeIn.Location.X + 140, nodeIn.Location.Y + 12);
                                ////ptf2 = new PointF(ptf1.X + 74, ptf1.Y + 12);
                                ////ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
                                //ptf1 = new PointF(nodeRC.Location.X + nodeRC.Size.Width, nodeRC.Location.Y + nodeRC.Size.Height - 15);
                                //ptf2 = new PointF(ptf1.X + 40, ptf1.Y);
                                //ptf3 = new PointF(ptf2.X - 10, ptf2.Y);
                                //ptArrayList_ground.Add(new PointF[] { ptf1, ptf2, ptf3 });
                                RCIndex++;
                            }
                        }
                    }
                }
                if (shardRCNode != null && indoorGroup != null && indoorGroup.Count > 0)
                {
                    foreach (RoomIndoor ri in indoorGroup)
                    {
                        //共享其它Remote Controller的室内机
                        if (ri == nodeIn.RoomIndoorItem) continue;
                        WiringNodeIn nodeInShareTo = nodeInList.Find(n => n.RoomIndoorItem == ri);
                        if (nodeInShareTo == null) continue;

                        //共享连接线
                        PointF ptf1 = new PointF(shardRCNode.Location.X + (shardRCNode.Size.Width / 2), shardRCNode.Location.Y + shardRCNode.Size.Height); //nodeInShareTo.Location.Y - 70
                        if (nodeInShareTo.Location.Y + 12 < shardRCNode.Location.Y)
                        {
                            //室内机在共享控制器的上面
                            ptf1.Y = shardRCNode.Location.Y;
                        }
                        PointF ptf2 = new PointF(shardRCNode.Location.X + (shardRCNode.Size.Width / 2), nodeInShareTo.Location.Y + 12);
                        PointF ptf3 = new PointF(nodeInShareTo.Location.X + nodeInShareTo.Size.Width - 1, nodeInShareTo.Location.Y + 12);
                        ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3 });
                    }
                }
                rowIndex++;
            }
        }

        #endregion

        #region Main_Controller

        string type_PictureBox = typeof(PictureBox).ToString();
        string type_ListViewItem = typeof(ListViewItem).ToString();
        Point resetPosition = new Point(0, 0);

        public static int ControlGroupNo = 0;

        /// 初始化Controller相关的属性以及方法
        /// <summary>
        /// 初始化Controller相关的属性以及方法
        /// </summary>
        private void InitController()
        {
            // 初始化Outdoor List备选区控件
            this.lvController_OutdoorList.MultiSelect = false;
            this.lvController_OutdoorList.AllowDrop = true;
            this.lvController_OutdoorList.FullRowSelect = true;
            this.lvController_OutdoorList.ItemDrag += new ItemDragEventHandler(lvOutdoorList_ItemDrag);
            this.lvController_OutdoorList.DragOver += new DragEventHandler(lvOutdoorList_DragOver);
            this.lvController_OutdoorList.DragDrop += new DragEventHandler(lvOutdoorList_DragDrop);


            //// Controller布局图的容器，ControlSystem控件的父控件,20141029 clh 暂不考虑Control System层
            //this.pnlController_Center.ControlAdded += new ControlEventHandler(pnlController_Center_ControlAdded);
            //this.pnlController_Center.ControlRemoved += new ControlEventHandler(pnlController_Center_ControlRemoved);

            // 初始化 Material List 
            dgvMaterialList.AutoGenerateColumns = false;
            dgvMaterialList.AllowUserToResizeRows = false;
            dgvMaterialList.AllowUserToOrderColumns = false;
            dgvMaterialList.AllowUserToResizeColumns = true;
            dgvMaterialList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMaterialList.BorderStyle = BorderStyle.None;
            dgvMaterialList.BackgroundColor = Color.White;

            NameArray_Controller arr = new NameArray_Controller();
            Global.SetDGVDataName(ref dgvMaterialList, arr.MaterialList_DataName);
            Global.SetDGVName(ref dgvMaterialList, arr.MaterialList_Name);
            Global.SetDGVNotSortable(ref dgvMaterialList);

            //BindToControl_Controller();
        }

        /// 将指定Model型号的记录添加到MaterialList中
        /// <summary>
        /// 将指定Model型号的记录添加到MaterialList中
        /// </summary>
        private void AddToMaterialList(Controller controller)
        {
            foreach (DataGridViewRow r in dgvMaterialList.Rows)
            {
                if (Convert.ToString(r.Cells[0].Value) == controller.Name)
                {
                    int i = Convert.ToInt32(r.Cells[1].Value) + controller.Quantity;
                    r.Cells[1].Value = i;
                    return;
                }
            }
            Trans trans = new Trans();
            //翻译
            string description = trans.getTypeTransStr(TransType.Controller.ToString(), controller.Description);
            //还未添加过此物料的情况
            dgvMaterialList.Rows.Add(controller.Name, controller.Quantity, description);
        }

        /// 获取Controller指定组件数量
        /// <summary>
        /// 获取Controller指定组件数量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private int getMaterialCount(string name)
        {
            foreach (DataGridViewRow r in dgvMaterialList.Rows)
            {
                if (Convert.ToString(r.Cells[0].Value) == name)
                {
                    return Convert.ToInt32(r.Cells[1].Value);
                }
            }
            return 0;
        }

        /// 绑定当前的Material List到界面控件
        /// <summary>
        /// 绑定当前的Material List到界面控件
        /// </summary>
        private void BindMaterialList()
        {
            dgvMaterialList.Rows.Clear();
            // add on 20141010 clh 控制器型号
            foreach (Controller item in thisProject.ControllerList)
            {
                if (thisProject.ControlGroupList.Find(p=>p.Id == item.ControlGroupID) == null)
                {
                    continue; //临时过滤掉因bug产生的无效的控制器数据。 add by Shen Junjie on 2018/8/28
                }
                AddToMaterialList(item);
            }

            //foreach (ControllerAssembly item in thisProject.ControllerAssemblyList)
            //{
            //    AddToMaterialList(item);
            //}

        }
        /// 绑定现有项目的Controller信息，导出报表专用 add on 20160530 by Yunxiao Lin
        /// <summary>
        /// 绑定现有项目的Controller信息，导出报表专用
        /// </summary>
        private void BindToControl_Controller_Report()
        {
            if (pnlController_Center.Controls.Count == 0)
            {
                glProject.ControllerLayoutType = thisProject.ControllerLayoutType;
                this.jccmbControllerType.SelectedIndex = (int)thisProject.ControllerLayoutType;

                this.pnlController_Center.Controls.Clear();
                // 将无效的对象删除
                if (thisProject.ControlSystemList == null)
                    thisProject.ControlSystemList = new List<ControlSystem>();

                ProjectBLL bll = new ProjectBLL(thisProject);
                bll.ClearInvalidControllerData();

                CentralControllerBLL bllCentralController = new CentralControllerBLL(thisProject);
                //List<CentralController> controllerTypes = bllCentralController.GetAvailableControllers(_productType);
                // 加载现有的Controller布局
                ControlGroupNo = 0;
                foreach (ControlSystem controlSystem in thisProject.ControlSystemList)
                {
                    ucDropControlSystem ucSystem = AddControlSystem(this.pnlController_Center, controlSystem);
                    if (controlSystem == thisProject.ControlSystemList[0])
                        ucSystem.HideDeleteLine();

                    // 添加已有的ControlGroup控件到Control System
                    for (int i = 0; i < thisProject.ControlGroupList.Count; ++i)
                    {
                        ControlGroupNo++;
                        ControlGroup controlGroup = thisProject.ControlGroupList[i];

                        if (!string.IsNullOrEmpty(controlGroup.ControlSystemID) && controlGroup.ControlSystemID == controlSystem.Id)
                        {
                            ucDropControlGroup ucGroup = AddControlGroup(controlGroup, ucSystem);
                            // 添加已有的Outdoor记录到Control Group，注意更新机组数量，以及Material List数据
                            foreach (SystemVRF sysItem in thisProject.SystemList)
                            {
                                if (!string.IsNullOrEmpty(sysItem.ControlGroupID[0]) && sysItem.ControlGroupID[0] == controlGroup.Id)
                                {
                                    ListViewItem lvItem = Controller_CreatListViewItem(sysItem);

                                    // 后台加载指定的Outdoor记录到当前Control Group区域，用于打开已有的项目时
                                    ucGroup.AddOutdoorItem(lvItem);
                                }
                            }

                            //不再添加多条相同型号的记录了，所以废弃   （20160525)
                            //// 添加已有的Controller控件到Control Group
                            //var controllerGroups = from p in thisProject.ControllerList 
                            //        where p.ControlGroupID == controlGroup.Id 
                            //        group p by p.Name into g 
                            //        select g;

                            //// 后台加载控制器控件到当前Control Group区域，用于打开已有的项目时
                            //foreach (var group in controllerGroups)
                            //{
                            //    ucGroup.AddController(group.ToArray(), controllerTypes.First(p => p.Model == group.Key));
                            //}
                            //foreach (Controller controller in thisProject.ControllerList)
                            //{
                            //    if (controller.ControlGroupID == controlGroup.Id)
                            //    {
                            //        ucGroup.AddController(controller, controllerTypes.FirstOrDefault(p => p.Model == controller.Model));
                            //    }
                            //}

                            foreach (Controller controller in thisProject.ControllerList)
                            {
                                //由于Controller支持多productType筛选，所以在导入项目时必须将保持一致的系统ProductType作为条件进行筛选
                                foreach (SystemVRF sysItem in thisProject.SystemList)
                                {
                                    if (controller.ControlGroupID == controlGroup.Id && sysItem.ControlGroupID[0] == controlGroup.Id)
                                    {
                                        List<CentralController> controllerTypes = bllCentralController.GetAvailableControllers(sysItem.OutdoorItem.ProductType);
                                        ucGroup.AddController(controller, controllerTypes.FirstOrDefault(p => p.Model == controller.Model));
                                    }
                                }
                            }
                            ucGroup.CheckControlGroupComplete();

                            // add on 20150403 clh,删除已经无效的ControlGroup控件以及对象
                            if (!ucGroup.IsActive)
                            {
                                ucGroup.Remove();
                                thisProject.ControlGroupList.Remove(controlGroup);
                                i--; //control group 不在界面上显示的问题 add by Shen Junjie on 2018/3/16
                            }
                            else
                            {
                                ucGroup.AddController();
                            }

                        }
                    }

                    // 最后再添加空的Controller以及Control Group控件到每一个Control System
                    //AddController(ucSystem);//Update on 20141029,PM会议决定，暂时屏蔽
                    AddControlGroup(ucSystem);
                    //UtilControl.CheckControlGroupComplete(ucSystem);
                }

                if (thisProject.ControlSystemList.Count == 0)
                {
                    // 若当前没有Controller布局，则加载空的控件
                    ucDropControlSystem ucSystem = AddControlSystem(this.pnlController_Center);
                    ucSystem.HideDeleteLine();
                    //AddController(ucSystem);
                    AddControlGroup(ucSystem);
                }

                this.pnlController_Center.AutoScrollPosition = resetPosition; // 初始加载后滚动条恢复原位

                //this.pnlController_Center.Refresh();
            }
        }

        /// 绑定现有项目的Controller信息
        /// <summary>
        /// 绑定现有项目的Controller信息
        /// </summary>
        private void BindToControl_Controller()
        {
            glProject.ControllerLayoutType = thisProject.ControllerLayoutType;
            this.jccmbControllerType.SelectedIndex = (int)thisProject.ControllerLayoutType;
            List<CentralController> controllerTypes = new List<CentralController>();

            this.pnlController_Center.Controls.Clear();
            // 将无效的对象删除
            if (thisProject.ControlSystemList == null)
                thisProject.ControlSystemList = new List<ControlSystem>();

            ProjectBLL bll = new ProjectBLL(thisProject);
            bll.ClearInvalidControllerData();
            // add on 20150122 clh
            // 当更新Indoor信息导致所属系统的室外机无效后，Controller部分会受到影响
            // TODO 20150122

            CentralControllerBLL bllCentralController = new CentralControllerBLL(thisProject);

            controllerTypes = bllCentralController.GetAvailableControllers(_productType);
            if (!_productType.Contains("Heat Exchanger"))
            {
                foreach (RoomIndoor ri in this.thisProject.ExchangerList)
                {
                    List<CentralController> listContr = bllCentralController.GetAvailableControllers(ri.IndoorItem.Series);
                    foreach (CentralController c in listContr)
                    {
                        //不同的_productType拷贝数据值至新的集合中
                        controllerTypeList.Add(c);
                    }
                }
                foreach (CentralController c in controllerTypes)
                {
                    controllerTypeList.Add(c);
                }
            }


            //可能存在无控制器的productType，如果不存在控制器，则插入上条数据进行布局
            if (controllerTypes.Count == 0)
            {
                controllerTypes = contrTypes;
                pnlControllerList.Visible = false;
            }
            else
            {
                if (!_productType.Contains("Heat Exchanger"))
                {
                    contrTypes = controllerTypeList.DeepClone();
                }
                else
                {
                    contrTypes = controllerTypes.DeepClone();
                }
                pnlControllerList.Visible = true;
            }


            // 加载现有的Controller布局
            ControlGroupNo = 0;
            foreach (ControlSystem controlSystem in thisProject.ControlSystemList)
            {
                ucDropControlSystem ucSystem = AddControlSystem(this.pnlController_Center, controlSystem);
                if (controlSystem == thisProject.ControlSystemList[0])
                    ucSystem.HideDeleteLine();

                //// 添加已有的Controller控件到Control System
                //foreach (Controller controller in thisProject.ControllerList)
                //{
                //    if (!string.IsNullOrEmpty(controller.ControlSystemID) && controller.ControlSystemID == controlSystem.Id)
                //        AddController(controller, ucSystem);
                //}

                // 添加已有的ControlGroup控件到Control System
                for (int i = 0; i < thisProject.ControlGroupList.Count; ++i)
                {
                    ControlGroupNo++;
                    ControlGroup controlGroup = thisProject.ControlGroupList[i];

                    if (!string.IsNullOrEmpty(controlGroup.ControlSystemID) && controlGroup.ControlSystemID == controlSystem.Id)
                    {
                        ucDropControlGroup ucGroup = AddControlGroup(controlGroup, ucSystem);
                        // 添加已有的Outdoor记录到Control Group，注意更新机组数量，以及Material List数据
                        foreach (SystemVRF sysItem in thisProject.SystemList)
                        {
                            if (!string.IsNullOrEmpty(sysItem.ControlGroupID[0]) && sysItem.ControlGroupID[0] == controlGroup.Id)
                            {
                                if (!bll.ValidateSystemForController(sysItem))
                                {
                                    bll.RemoveOutdoorFromControlGroup(sysItem.Id);
                                    continue;
                                }

                                ListViewItem lvItem = Controller_CreatListViewItem(sysItem);

                                // 后台加载指定的Outdoor记录到当前Control Group区域，用于打开已有的项目时
                                ucGroup.AddOutdoorItem(lvItem);
                            }
                        }
                        foreach (RoomIndoor ri in this.thisProject.ExchangerList)
                        {
                            if (!string.IsNullOrEmpty(ri.ControlGroupID[0]) && ri.ControlGroupID[0] == controlGroup.Id)
                            {


                                ListViewItem lvItem = Controller_CreatListViewItem(ri);

                                // 后台加载指定的Outdoor记录到当前Control Group区域，用于打开已有的项目时
                                ucGroup.AddOutdoorItem(lvItem);
                            }
                        }


                        //不再添加多条相同型号的记录了，所以废弃   （20160525)
                        //// 添加已有的Controller控件到Control Group
                        //var controllerGroups = from p in thisProject.ControllerList 
                        //        where p.ControlGroupID == controlGroup.Id 
                        //        group p by p.Name into g 
                        //        select g;

                        //// 后台加载控制器控件到当前Control Group区域，用于打开已有的项目时
                        //foreach (var group in controllerGroups)
                        //{
                        //    ucGroup.AddController(group.ToArray(), controllerTypes.First(p => p.Model == group.Key));
                        //}
                        foreach (Controller controller in thisProject.ControllerList)
                        {
                            if (controller.ControlGroupID == controlGroup.Id)
                            {
                                CentralController controllerType = controllerTypeList.FirstOrDefault(p => p.Model == controller.Model);
                                //  CentralController controllerType = controllerTypes.FirstOrDefault(p => p.Model == controller.Model);
                                if (controllerType != null)
                                {
                                    ucGroup.AddController(controller, controllerType);
                                }
                                else
                                {
                                    //判断如果当前的model 不存在，添加一个带有错误标识的Controller on 20190320 by xyj
                                    CentralController control = new CentralController();
                                    control.Model = controller.Model;
                                    control.Image = "NoImage.png";
                                    control.Description = controller.Description;
                                    ucGroup.AddController(controller, control);
                                }
                            }
                        }
                        ucGroup.CheckControlGroupComplete();

                        // add on 20150403 clh,删除已经无效的ControlGroup控件以及对象
                        if (!ucGroup.IsActive)
                        {
                            ucGroup.Remove();
                            thisProject.ControlGroupList.Remove(controlGroup);
                            i--; //control group 不在界面上显示的问题 add by Shen Junjie on 2018/3/16
                        }
                        else
                        {
                            ucGroup.AddController();
                        }
                    }
                }

                // 最后再添加空的Controller以及Control Group控件到每一个Control System
                //AddController(ucSystem);//Update on 20141029,PM会议决定，暂时屏蔽
                AddControlGroup(ucSystem);
                //UtilControl.CheckControlGroupComplete(ucSystem);
            }
             

            // 绑定Material List记录
            BindMaterialList();

             
            // 绑定Controller备选区列表
            BindCandidateControllerList(controllerTypes); 


            //Controller备选区列表隐藏不可用的 controller
            HideInvalidControllerType();

            if (thisProject.ControlSystemList.Count == 0)
            {
                // 若当前没有Controller布局，则加载空的控件
                ucDropControlSystem ucSystem = AddControlSystem(this.pnlController_Center);
                ucSystem.HideDeleteLine();
                //AddController(ucSystem);
                AddControlGroup(ucSystem);
            }

            // 加载已分配以及未分配的室外机记录到Controller中
            this.lvController_OutdoorList.Items.Clear();
            this.lvController_OutdoorList.Columns[0].Width = 190;
            if (thisProject != null)
            {
                foreach (SystemVRF sysItem in thisProject.SystemList)
                {
                    if (String.IsNullOrEmpty(sysItem.ControlGroupID[0]))
                    {
                        ListViewItem lvItem = Controller_CreatListViewItem(sysItem);
                        if (!bll.ValidateSystemForController(sysItem))
                        {
                            lvItem.ForeColor = Color.FromArgb(150, 150, 150);
                            lvItem.Tag = false;
                        }
                        this.lvController_OutdoorList.Items.Add(lvItem);
                    }
                }

                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    if (String.IsNullOrEmpty(ri.ControlGroupID[0]))
                    {
                        ListViewItem lvItem = Controller_CreatListViewItem(ri);
                        this.lvController_OutdoorList.Items.Add(lvItem);
                    }
                }
            } 
            
           

            this.pnlController_Center.AutoScrollPosition = resetPosition; // 初始加载后滚动条恢复原位
        }

        //验证已有的ControlGroup控件到Control System 下室内机总和
        private bool ValidateIndoor_ControlGroup()
        {
            bool isPass = true;            
            if (thisProject.ControlGroupList != null && thisProject.ControlGroupList.Count > 0)
            {
                for (int i = 0; i < thisProject.ControlGroupList.Count; ++i)
                {
                    int indoorNumber = 0;   //室内机逻辑只存在一个ContorlGroup中
                    ControlGroup controlGroup = thisProject.ControlGroupList[i];
                    // 添加已有的Outdoor记录到Control Group， 
                    foreach (SystemVRF sysItem in thisProject.SystemList)
                    {
                        if (!string.IsNullOrEmpty(sysItem.ControlGroupID[0]) && sysItem.ControlGroupID[0] == controlGroup.Id)
                        {
                            List<RoomIndoor> list = thisProject.RoomIndoorList.FindAll(p => p.SystemID == sysItem.Id);
                            if (list != null && list.Count > 0)
                                indoorNumber = indoorNumber + list.Count;
                        }
                    }
                    if (indoorNumber > 160)
                    {
                        JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_INDOOR_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_IDU_QTY));
                       return false; 
                    } 

                } 
                 
            }
            return isPass;

        }

        //绑定备选区控制器列表数据
        private void BindCandidateControllerList(List<CentralController> controllerTypes)
        {
            //移除所有已显示的Controller备选区控件
            this.pnlControllerList.Controls.Clear();
            bool hasScrollBar = controllerTypes.Count > 9;
            Size smallSize = new Size(56, 56);
            // 初始化Controller备选区控件
            this.pnlControllerList.SuspendLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            foreach (CentralController type in controllerTypes)
            {
                System.Windows.Forms.PictureBox pbNew = new System.Windows.Forms.PictureBox();
                Trans trans = new Trans();
                pbNew.BackColor = System.Drawing.Color.White;
                pbNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                pbNew.TabStop = false;
                pbNew.AllowDrop = true;
                resources.ApplyResources(pbNew, "pbController1");

                Util.SetControllerImage(pbNew, type.Image);
                pbNew.Tag = type; //ControllerType.ONOFF; 
                if (hasScrollBar)
                {
                    pbNew.Size = smallSize;
                }
                pbNew.MouseDown += new MouseEventHandler(pbCandidateController_MouseDown);
                pbNew.DragOver += new DragEventHandler(pbCandidateController_DragOver);
                pbNew.DragDrop += new DragEventHandler(pbCandidateController_DragDrop);

                //toolTipController.SetToolTip(pbNew, type.Model + "\n" + type.Description);
                //翻译
                string description = trans.getTypeTransStr(TransType.Controller.ToString(), type.Description);
                toolTipController.SetToolTip(pbNew, type.Model + "\n" + description);
                this.pnlControllerList.Controls.Add(pbNew);
                //将接口的图标放到最前面
                switch (type.Type)
                {
                    case ControllerType.LonWorksInterface:
                    case ControllerType.ModBusInterface:
                    case ControllerType.BACNetInterface:
                    case ControllerType.KNXInterface:
                        this.pnlControllerList.Controls.SetChildIndex(pbNew, 0);
                        break;
                }
            }

            this.pnlControllerList.ResumeLayout(true);
        }

        // 隐藏Contoller备选区不可用的图标
        /// <summary>
        /// 隐藏Contoller备选区不可用的图标
        /// </summary>
        private void HideInvalidControllerType()
        {
            foreach (Control c in this.pnlControllerList.Controls)
            {
                CentralController type = c.Tag as CentralController;
                //Controller备选区只能显示跟当前ControllerLayoutType有关的Controller 
                bool visible = true;
                switch (type.Type)
                {
                    case ControllerType.LonWorksInterface:
                        if (thisProject.ControllerLayoutType != ControllerLayoutType.LONWORKS)
                            visible = false;
                        break;
                    case ControllerType.ModBusInterface:
                        if (thisProject.ControllerLayoutType != ControllerLayoutType.MODBUS)
                            visible = false;
                        break;
                    case ControllerType.BACNetInterface:
                        if (thisProject.ControllerLayoutType != ControllerLayoutType.BACNET)
                            visible = false;
                        break;
                    case ControllerType.KNXInterface:
                        if (thisProject.ControllerLayoutType != ControllerLayoutType.KNX)
                            visible = false;
                        break;
                }
                c.Visible = visible;
            }
        }

        /// 用于加载已保存的数据
        /// <summary>
        /// 用于加载已保存的数据
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        private ListViewItem Controller_CreatListViewItem(SystemVRF sysItem)
        {
            int indoorQty = sysItem.GetIndoorCount(thisProject);

            ListViewItem lvItem = new ListViewItem();
            lvItem.Name = sysItem.Id;
            lvItem.Tag = new Object[] { sysItem, indoorQty };
            lvItem.Text = sysItem.Name + " (" + indoorQty + ")";
            lvItem.ImageIndex = 0;
            return lvItem;
        }

        /// 用于加载已保存的数据Exchanger
        /// <summary>
        /// 用于加载已保存的数据
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        private ListViewItem Controller_CreatListViewItem(RoomIndoor ri)
        {
            int indoorQty = 1;
            ListViewItem lvItem = new ListViewItem();
            //设置为true 表示TTL Heat Exchanger 类型 
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_S")
            {
                lvItem.Name = ri.IndoorItem.Type;
            }
            else
            {
                lvItem.Name = ri.IndoorItem.Series;
            }
            lvItem.Tag = new Object[] { ri, indoorQty };
            if (thisProject.BrandCode == "H")
            {
                lvItem.Text = ri.IndoorName + " (" + ri.IndoorItem.Model_Hitachi + ")";
            }
            else
            {
                lvItem.Text = ri.IndoorName + " (" + ri.IndoorItem.Model_York + ")";
            }
            lvItem.ImageIndex = 0;
            return lvItem;
        }


        /// 重新定位 Controller 中控件的大小 & 位置
        /// <summary>
        /// 重新定位 Controller 中控件的大小 & 位置
        /// </summary>
        /// <param name="e"></param>
        private void ReLocationController(object sender, EventArgs e, Panel container)
        {
            int MaxY_ControllerGroup = UtilControl.vLocationController;
            int MaxY_Controller = UtilControl.vLocationGroup;
            this.pnlController_Center.AutoScrollPosition = resetPosition;
            ucDropControlSystem ucControlSystem = null;
            foreach (Control ctrlSystem in container.Controls)
            {
                if (ctrlSystem is ucDropControlSystem)
                {
                    foreach (Control ctrl in (ctrlSystem as ucDropControlSystem).Controls)
                    {
                        if (ctrl is ucDropController)
                        {
                            ctrl.Location = new Point(UtilControl.hLocationController, MaxY_Controller);
                            MaxY_Controller += ctrl.Size.Height + 5;
                            ucControlSystem = (ctrl as ucDropController).Parent as ucDropControlSystem;
                        }
                        else if (ctrl is ucDropControlGroup)
                        {
                            ctrl.Location = new Point(UtilControl.hLocationGroup, MaxY_ControllerGroup);
                            MaxY_ControllerGroup += ctrl.Size.Height + 5;
                            ucControlSystem = (ctrl as ucDropControlGroup).Parent as ucDropControlSystem;
                        }
                    }
                }
            }

            if (ucControlSystem != null)
            {
                if (MaxY_Controller > MaxY_ControllerGroup)
                {
                    ucControlSystem.Size = new Size(this.Width, MaxY_Controller);
                }
                else
                {
                    ucControlSystem.Size = new Size(this.Width, MaxY_ControllerGroup);
                }
            }

            int max = this.pnlController_Center.VerticalScroll.Maximum;
            int y = 0;
            if (sender is ucDropControlGroup)
            {
                y = (sender as ucDropControlGroup).Location.Y;
                y = y < max ? y : max;
            }
            else if (sender is ucDropControlSystem)
                y = max;

            this.pnlController_Center.AutoScrollPosition = new Point(0, y);
        }

        /// 添加空的 ucDropControllerGroup 控件,并绑定新创建的ControlGroup对象
        /// <summary>
        /// 添加空的 ucDropControllerGroup 控件,并绑定新创建的ControlGroup对象
        /// </summary>
        private void AddControlGroup(ucDropControlSystem ucSystem)
        {
            string groupID = (new ProjectBLL(thisProject)).AddGroupToControlSystem(ucSystem.ControlSystemID);
            ucDropControlGroup uc = new ucDropControlGroup(_mainRegion);
            uc.BindToControlGroup(groupID);
            ucSystem.Controls.Add(uc);

            uc.BeforeRemove += new System.EventHandler(ucControlGroup_BeforeRemove);
            uc.BeforeAdd += new System.EventHandler(ucControlGroup_BeforeAdd);
            uc.BeforeDrop += new DropEventHandler(ucControlGroup_BeforeDrop);
            uc.AfterDrop += new DropEventHandler(ucControlGroup_AfterDrop);
            uc.Relocation += new System.EventHandler(ucControlGroup_Relocation);
            uc.AfterSetting += new System.EventHandler(ucControlGroup_AfterSetting);
            uc.RemoveGroup = new System.Action(() =>
            {
                (new ProjectBLL(thisProject)).RemoveControlGroup(uc.ControlGroupID);
            });
        }

        /// 后台加载 ucDropControlGroup 控件，用于打开已有的布局，需要先删除空白的Controller控件
        /// <summary>
        /// 后台加载 ucDropControlGroup 控件，用于打开已有的布局，需要先删除空白的Controller控件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ucSystem"></param>
        private ucDropControlGroup AddControlGroup(ControlGroup item, ucDropControlSystem ucSystem)
        {
            ucDropControlGroup uc = new ucDropControlGroup(item.Id ,_mainRegion);
            uc.Title = item.Name;
            uc.RemoveController();
            ucSystem.Controls.Add(uc);

            uc.BeforeRemove += new System.EventHandler(ucControlGroup_BeforeRemove);
            uc.BeforeAdd += new System.EventHandler(ucControlGroup_BeforeAdd);
            uc.BeforeDrop += new DropEventHandler(ucControlGroup_BeforeDrop);
            uc.AfterDrop += new DropEventHandler(ucControlGroup_AfterDrop);
            uc.Relocation += new System.EventHandler(ucControlGroup_Relocation);
            uc.AfterSetting += new System.EventHandler(ucControlGroup_AfterSetting);
            uc.RemoveGroup = new System.Action(() => {
                (new ProjectBLL(thisProject)).RemoveControlGroup(uc.ControlGroupID);
            });
            return uc;
        }

        /// 添加空的 ucDropControlSystem 控件，并创建ControlSystem对象与之绑定
        /// <summary>
        /// 添加空的 ucDropControlSystem 控件，并创建ControlSystem对象与之绑定
        /// </summary>
        private ucDropControlSystem AddControlSystem(Panel container)
        {
            string sysID = (new ProjectBLL(thisProject)).AddControlSystem();
            ucDropControlSystem uc = new ucDropControlSystem();
            uc.BindControlSystem(sysID);
            container.Controls.Add(uc);

            uc.ControlAdded += new ControlEventHandler(ucControlSystem_ControlAdded);
            uc.ControlRemoved += new ControlEventHandler(ucControlSystem_ControlRemoved);
            //uc.Paint += new PaintEventHandler(ucControlSystem_Paint);

            return uc;
        }

        /// 后台加载 ucDropControlSystem 控件，用于打开已有的布局
        /// <summary>
        /// 后台加载 ucDropControlSystem 控件，用于打开已有的布局
        /// </summary>
        /// <param name="item"></param>
        private ucDropControlSystem AddControlSystem(Panel container, ControlSystem item)
        {
            ucDropControlSystem uc = new ucDropControlSystem(item.Id);
            uc.ShowDeleteLine();
            container.Controls.Add(uc);

            uc.ControlAdded += new ControlEventHandler(ucControlSystem_ControlAdded);
            uc.ControlRemoved += new ControlEventHandler(ucControlSystem_ControlRemoved);
            //uc.Paint += new PaintEventHandler(ucControlSystem_Paint);

            return uc;
        }

        // 暂时屏蔽Control System层次的操作
        ///// 鼠标拖拽悬停时检查是否允许拖放,此处检验ucDropControlSystem中的Controller的拖拽
        ///// <summary>
        ///// 鼠标拖拽悬停时检查是否允许拖放,此处检验ucDropControlSystem中的Controller的拖拽
        ///// </summary>
        ///// <param name="sender">目标控件，PictureBox</param>
        ///// <param name="e">源控件，PictureBox</param>
        ///// <returns>返回true则允许拖放；false则不允许</returns>
        //private bool CheckDropController(object sender, DragEventArgs e)
        //{
        //    if (UtilControl.CheckDragType_PictureBox(e))
        //    {
        //        PictureBox pbTarget = sender as PictureBox;// 目标控件
        //        PictureBox pbSrc = (PictureBox)e.Data.GetData(type_PictureBox);// 源控件

        //        ucDropControlSystem ucSystem = (sender as PictureBox).Parent.Parent as ucDropControlSystem;
        //        ControlSystem controlSystem = bll.GetControlSystem(ucSystem.ControlSystemID);
        //        ControllerLayoutType type = GetControlLayoutType();

        //        int controllerQtySystem = ucSystem.GetControllerQty();
        //        int maxControllerQtySystem = controlSystem.GetMaxControllerCount(type);
        //        int controllerQtyWithoutGroup = ucSystem.GetControllerQtyWithoutGroup();

        //        if (pbTarget.Image != null)
        //            return false;
        //        else
        //        {
        //            // Bacnet模式下，同一个Control System中触摸屏数量（4）限制总数（16）限制
        //            if (type == ControllerLayoutType.BACNET)
        //            {
        //                int qty_Touch = ucSystem.GetControllerQty(ControllerType.CC);
        //                if (qty_Touch >= 4 && (ControllerType)pbSrc.Tag == ControllerType.CC)
        //                {
        //                    JCMsg.ShowWarningOK(Msg.CONTROLLER_TOUCH_QTY);
        //                    return false;
        //                }

        //                if (controllerQtySystem < maxControllerQtySystem)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    JCMsg.ShowWarningOK(Msg.CONTROLLER_CONTROLLER_QTY);
        //                    return false;
        //                }
        //            }
        //            else if (type == ControllerLayoutType.MODBUS)
        //            {
        //                // Modbus模式下，整个控制系统连一个Controller或者每一个Group中最多连一个Controller
        //                if (controllerQtyWithoutGroup < maxControllerQtySystem)
        //                {
        //                    //// 如果某个Group中已有Controller，则控制系统级别就不允许拖入Controller
        //                    //if (controllerQtySystem > 0)
        //                    //{
        //                    //    JCMsg.ShowWarningOK(Msg.CONTROLLER_CONTROLLER_QTY);
        //                    //    return false;
        //                    //}
        //                    return true;
        //                }
        //                else
        //                {
        //                    JCMsg.ShowWarningOK(Msg.CONTROLLER_CONTROLLER_QTY);
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

        /// 拖放 PictureBox 图片后执行的事件
        /// <summary>
        /// 拖放 PictureBox 图片后执行的事件
        /// </summary>
        /// <param name="sender">目标控件，PictureBox</param>
        /// <param name="e">源控件，PictureBox</param>
        private void DoDropController(object sender, DragEventArgs e)
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            PictureBox pbTarget = sender as PictureBox;// 目标控件
            PictureBox pbSrc = (PictureBox)e.Data.GetData(type_PictureBox);// 源控件

            //本次新增的控制器控件
            ucDropController ucControllerNew = null;

            // 1 若源控件属于ucDropController，则拖放后源控件自动移除
            if (pbSrc.Parent is ucDropController)
            {
                ucDropController ucController_Src = (pbSrc.Parent as ucDropController);
                Controller controllerSrc = ucController_Src.Controller;
                CentralController typeInfoSrc = ucController_Src.TypeInfo;
                ucDropControlGroup ucControlGroup_Src = null;
                if (ucController_Src.Parent is ucDropControlGroup)
                {
                    ucControlGroup_Src = (ucController_Src.Parent as ucDropControlGroup);
                }

                // 1-1 若目标控件也属于ucDropController(空或非空)，则不需要删除,只需要更新目标Controller控件绑定的ID，以及关联的GroupID或者SystemID;
                if (pbTarget.Parent is ucDropController)
                {
                    ucDropController ucController_target = pbTarget.Parent as ucDropController;

                    // 1-1-1 若目标控件属于Control Group
                    if (ucController_target.Parent is ucDropControlGroup)
                    {
                        ucDropControlGroup ucControlGroup_target = ucController_target.Parent as ucDropControlGroup;
                        string groupID = ucControlGroup_target.ControlGroupID;

                        //1-1-1-1 若目标控件为空
                        if (ucController_target.Controller == null)
                        {
                            //源controller数据对象绑定到目标控件
                            ucController_target.BindToControl_Controller(controllerSrc, typeInfoSrc);

                            //修改controller数据对象的groupID
                            controllerSrc.AddToControlGroup(groupID);

                            //添加空白的控制器控件
                            ucControllerNew = ucControlGroup_target.AddController();
                        }
                        //2-1-1-2 若目标控件类型和源类型相同
                        else if (typeInfoSrc.Model == ucController_target.Controller.Model)
                        {
                            //将数量累计到目标控件 
                            ucController_target.Add(controllerSrc.Quantity);

                            //删除源controller数据对象
                            bll.RemoveController(controllerSrc.Id);
                        }

                        //添加成套使用的控制器
                        AddControllerInSet(ucControlGroup_Src, ucControlGroup_target, typeInfoSrc, controllerSrc.Quantity);
                    }
                    //// 1-1-2 若目标控件属于Control System，暂时注销
                    //else if (ucController_target.Parent is ucDropControlSystem)
                    //{
                    //    string sysID = (ucController_target.Parent as ucDropControlSystem).ControlSystemID;
                    //    bll.GetControlSystem(sysID).IsValid = true;
                    //    controllerItem.AddToControlSystem(sysID);
                    //}
                }
                // 1-2 若目标控件为备选区，则需要删除当前控制器关联的 Controller 对象
                else
                {
                    bll.RemoveController(controllerSrc.Id);
                    RemoveInSetControllers(ucControlGroup_Src, typeInfoSrc);
                    BindMaterialList();
                }

                //删除源ucDropController控件
                (pbSrc.Parent as ucDropController).Remove();

            }
            // 2 源控件来自备选区，则需要添加 Controller 对象
            else
            {
                // 2-1 此时只要考虑目标控件为ucDropControllelr的情况
                if (pbTarget.Parent is ucDropController)
                {
                    CentralController typeInfoSrc = (CentralController)pbSrc.Tag;
                    //ControllerType type = (ControllerType)pbSrc.Tag;
                    ucDropController ucController_target = pbTarget.Parent as ucDropController;

                    // 2-1-1 若目标控件属于Control Group
                    if (ucController_target.Parent is ucDropControlGroup)
                    {
                        ucDropControlGroup ucControlGroup_target = ucController_target.Parent as ucDropControlGroup;
                        //2-1-1-1 若目标控件为空
                        if (ucController_target.Controller == null)
                        {
                            //新增controller数据对象
                            string groupID = ucControlGroup_target.ControlGroupID;
                            Controller controllerItem = bll.AddControllerToControlGroup(typeInfoSrc, groupID);
                            //绑定到目标控件
                            ucController_target.BindToControl_Controller(controllerItem, typeInfoSrc);

                            //添加空白的控制器控件
                            ucControlGroup_target.AddController();
                        }
                        //2-1-1-2 若目标控件类型和源类型相同
                        else if (typeInfoSrc.Model == ucController_target.Controller.Model)
                        {
                            //只需目标控件的controller数据对象的数量加1
                            ucController_target.Add(1);
                        }

                        //添加成套使用的控制器
                        AddControllerInSet(null, ucControlGroup_target, typeInfoSrc, 1);
                    }
                    //// 2-1-2 若目标控件属于Control System
                    //else if (ucController_target.Parent is ucDropControlSystem)
                    //{
                    //    string sysID = (ucController_target.Parent as ucDropControlSystem).ControlSystemID;
                    //    controllerID = bll.AddControllerToControlSystem(type, sysID);
                    //}
                    //Controller controllerItem = bll.GetController(controllerID);

                    BindMaterialList();
                }
            }
            //拖动重置状态
            thisProject.CentralControllerOK = false;
            SetTabControlImageKey();

            varificationCheck();
        }

        /// 添加或移动成套使用的控制器
        /// <summary>
        /// 添加或移动成套使用的控制器
        /// </summary>
        /// <param name="ucControlGroup_Src">当来源是备选区时此项设为空</param>
        /// <param name="ucControlGroup_target"></param>
        /// <param name="typeInfo"></param>
        /// <param name="quantity">增加或移动的数量</param>
        private void AddControllerInSet(
            ucDropControlGroup ucControlGroup_Src,
            ucDropControlGroup ucControlGroup_target,
            CentralController typeInfo,
            int quantity)
        {
            //是否需要添加成套使用的控制器？
            if (typeInfo.ControllersInSet == null || typeInfo.ControllersInSet.Count == 0)
            {
                return;
            }

            ProjectBLL bll = (new ProjectBLL(thisProject));
            string groupID = ucControlGroup_target.ControlGroupID;

            foreach (CentralController typeInfoInSet in typeInfo.ControllersInSet)
            {
                //查找来源group是否包含此成套控制器
                ucDropController ucController_InSet_Src = null;
                Controller controllerInSetSrc = null;
                if (ucControlGroup_Src != null)
                {
                    //从来源group里面查找这个成套的控制器
                    ucController_InSet_Src = ucControlGroup_Src.GetControllerByModelName(typeInfoInSet.Model);
                    if (ucController_InSet_Src != null)
                    {
                        controllerInSetSrc = ucController_InSet_Src.Controller;
                    }
                }

                //查找目标group是否包含此成套控制器
                ucDropController ucController_InSet_target = ucControlGroup_target.GetControllerByModelName(typeInfoInSet.Model);

                //如果目标group没有添加过这个控制器
                if (ucController_InSet_target == null)
                {
                    //查找空白的控制器控件
                    ucController_InSet_target = ucControlGroup_target.GetEmptyController();
                    if (ucController_InSet_target == null)
                    {
                        ucController_InSet_target = ucControlGroup_target.AddController();
                    }

                    //如果从来源group里面找到了这个成套的控制器，则只需要更改绑定
                    if (controllerInSetSrc != null)
                    {
                        //源controller数据对象绑定到目标空白控件
                        ucController_InSet_target.BindToControl_Controller(controllerInSetSrc, typeInfoInSet);

                        //修改controller数据对象的groupID
                        controllerInSetSrc.AddToControlGroup(groupID);
                    }
                    //如果来源不是group，或者来源group没有这个成套的控制器，则添加新的控制器到界面
                    else
                    {
                        //新增controller数据对象
                        Controller controllerInSetNew = bll.AddControllerToControlGroup(typeInfoInSet, groupID);

                        //源controller数据对象绑定到目标控件
                        ucController_InSet_target.BindToControl_Controller(controllerInSetNew, typeInfoInSet);
                    }
                    //再次添加空白的控制器控件
                    ucControlGroup_target.AddController();
                }
                //如果目标group已经添加过这个控制器
                else
                {
                    if (controllerInSetSrc != null)
                    {
                        //如果来源group有这个成套控制器, 增加的数量就是这个数量
                        quantity = controllerInSetSrc.Quantity;

                        //删除源controller数据对象
                        bll.RemoveController(controllerInSetSrc.Id);
                    }

                    //将数量累计到目标控件
                    ucController_InSet_target.Add(quantity);
                }

                //如果来源group存在此成套控制器，则删除源ucDropController控件
                if (ucController_InSet_Src != null)
                {
                    ucController_InSet_Src.Remove();
                }
            }
        }

        /// <summary>
        /// 在一个group中，移除跟指定控制器成套使用的其它控制器
        /// </summary>
        /// <param name="ucControlGroup_Src"></param>
        /// <param name="typeInfo"></param>
        private void RemoveInSetControllers(
            ucDropControlGroup ucControlGroup_Src,
            CentralController typeInfo)
        {
            //-1代表删除全部
            RemoveInSetControllers(ucControlGroup_Src, typeInfo, -1);
        }

        /// <summary>
        /// 在一个group中，跟指定控制器成套使用的其它控制器，移除指定的数量
        /// </summary>
        /// <param name="ucControlGroup_Src"></param>
        /// <param name="typeInfo"></param>
        /// <param name="reduceQuantity">删除的数量；-1代表删除全部</param>
        private void RemoveInSetControllers(
            ucDropControlGroup ucControlGroup_Src,
            CentralController typeInfo,
            int reduceQuantity)
        {
            if (ucControlGroup_Src == null) return;
            if (typeInfo.ControllersInSet == null || typeInfo.ControllersInSet.Count == 0)
            {
                return;
            }

            ProjectBLL bll = (new ProjectBLL(thisProject));

            foreach (CentralController typeInfoInSet in typeInfo.ControllersInSet)
            {
                //查找来源group是否包含此成套控制器
                ucDropController ucController_InSet_Src = null;
                Controller controllerInSetSrc = null;
                //从来源group里面查找这个成套的控制器
                ucController_InSet_Src = ucControlGroup_Src.GetControllerByModelName(typeInfoInSet.Model);
                if (ucController_InSet_Src != null)
                {
                    controllerInSetSrc = ucController_InSet_Src.Controller;
                    if (reduceQuantity < 0 || controllerInSetSrc.Quantity - reduceQuantity <= 0)
                    {
                        //删除数据对象
                        bll.RemoveController(controllerInSetSrc.Id);
                        //删除控件
                        ucController_InSet_Src.Remove();
                    }
                    else
                    {
                        controllerInSetSrc.Quantity -= reduceQuantity;
                        ucController_InSet_Src.UpdateQuantity();
                    }
                }
            }
        }

        private string outExchangerName(string item)
        {
            char[] ch = { '(' };
            string[] st = item.Split(ch);
            return Regex.Replace(st[0], @"\s+", " ").Trim();
        }

        /// 拖放 ListView 项目后执行的事件
        /// <summary>
        /// 拖放 ListView 项目后执行的事件
        /// </summary>
        /// <param name="sender">目标控件</param>
        /// <param name="e">源控件</param>
        private void DoDropOutdoor(object sender, DragEventArgs e)
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            ListView lvTarget = (sender as ListView);// 目标控件
            ListViewItem item = (ListViewItem)e.Data.GetData(type_ListViewItem, false);// 源控件
            ListView lvSrc = item.ListView;
            item.Remove();// 必须先移除源项目            
            lvTarget.Items.Add(item);// 然后添加到目标 ListView

            string systemVRFID = item.Name;
            SystemVRF sysItem = new SystemVRF();
            RoomIndoor ri = new RoomIndoor();
            //定义类型
            bool isSystem = true;

            //判断接受类型
            if (item.Name.Contains("Heat Exchanger"))
            {
                isSystem = false;
                //char[] ch = { '(' };
                //string[] st = item.Text.Split(ch);
                //exchangerName = Regex.Replace(st[0], @"\s+", " ").Trim();
                ri = bll.GetExchangerByName(outExchangerName(item.Text));
            }
            else
            {
                sysItem = bll.GetSystem(item.Name);
            }



            // 1 若源控件来自 ucDropControlGroup
            if (lvSrc.Parent is ucDropOutdoor)
            {
                //如果删除系统但控制器模块依然有数据，则删除系统后检查group的完整性 
                if (lvSrc.Items.Count == 0 && thisProject.ControllerList.Count > 0)
                {
                    ucDropOutdoor ucOutdoor = lvSrc.Parent as ucDropOutdoor;
                    ucOutdoor.OnRemove(e);
                    ucOutdoor.UpdateQuantity(); //更新系统面板的个数
                    ucDropControlGroup ucGroup = ucOutdoor.Parent as ucDropControlGroup;
                    if (ucGroup != null)
                    {
                        // 移除一个室外机记录，需要检查此时Control Group的完整性状态
                        ucGroup.CheckControlGroupComplete();
                    }

                }
                // 1-1 拖拽时仅剩一条记录，则该 ucDropControlGroup 控件将被移除
                else if (lvSrc.Items.Count == 0 && thisProject.ControllerList.Count == 0)
                {
                    (lvSrc.Parent.Parent as ucDropControlGroup).Remove();
                    string groupID = (lvSrc.Parent.Parent as ucDropControlGroup).ControlGroupID;
                    bll.RemoveControlGroup(groupID);
                }
                // 1-2 更新源ucDropControlGroup中显示的机组数量
                else
                {
                    ucDropOutdoor ucOutdoor = lvSrc.Parent as ucDropOutdoor;
                    ucOutdoor.UpdateQuantity();
                    ucDropControlGroup ucGroup = ucOutdoor.Parent as ucDropControlGroup;
                    if (ucGroup != null)
                    {
                        // 移除一个室外机记录，需要检查此时Control Group的完整性状态
                        ucGroup.CheckControlGroupComplete();
                    }
                }
            }

            // 2 若目标控件属于 ucDropControlGroup
            if (lvTarget.Parent is ucDropOutdoor)
            {
                // 2-1 更新目标group中的机组数量
                ucDropOutdoor ucOutdoor = lvTarget.Parent as ucDropOutdoor;

                // 2-2 判断是否带面板，若带则更新Outdoor记录的前缀图标
                if (ucOutdoor.IsComplete)
                {
                    item.ImageIndex = 2;
                }

                // 2-3 更新SystemVRF对象关联的ControlGroup对象
                string controlGroupID = (ucOutdoor.Parent as ucDropControlGroup).ControlGroupID;
                // 2-3-1 源控件来自 ucDropControlGroup，不需要新增控制器组件，只需要重新绑定SystemVRF关联的ControlGroupID即可
                if (lvSrc.Parent is ucDropOutdoor)
                {
                    if (isSystem)
                    {
                        sysItem.BindToControlGroup(controlGroupID);
                    }
                    else
                    {
                        ri.ControlGroupID[0] = controlGroupID;
                    }
                }
                // 2-3-2 源控件属于备选区，则绑定SystemVRF关联的ControlGroupID时，需要新增与该Outdoor对象类型相关的控制器组件,同步添加组件记录到界面上
                else
                {
                    ControllerLayoutType type = glProject.ControllerLayoutType;
                    if (isSystem)
                    {
                        bll.AddOutdoorToControlGroup(sysItem, controlGroupID, type);
                    }
                    else
                    {
                        ri.ControlGroupID[0] = controlGroupID;
                    }

                    if (bll.GetControlGroup(controlGroupID) != null)
                        bll.GetControlGroup(controlGroupID).IsValidGrp = true; 
                    string sysID = (ucOutdoor.Parent.Parent as ucDropControlSystem).ControlSystemID;
                    bll.GetControlSystem(sysID).IsValid = true;
                    // 更新Metarial List控件的记录
                    //string name = bll.GetControllerAssembly(sysItem.Id).Name;
                    BindMaterialList();
                }
            }
            // 3 若目标控件属于备选区
            else
            {
                // 3-1 只需要考虑源控件来自Control Group的情况，需要删除源Outdoor记录关联的控制器组件，并同步更新组件记录到界面上
                if (lvSrc.Parent is ucDropOutdoor)
                {
                    if (isSystem)
                    {
                        bll.RemoveOutdoorFromControlGroup(sysItem.Id);
                    }
                    else
                    {
                        bll.RemoveOutdoorFromControlGroupByExchanger(outExchangerName(item.Text));
                    }
                    BindMaterialList();
                }
            }
        }

        /// 更新备选区OutdoorList的项目图标为不带面板的
        /// <summary>
        /// 更新备选区OutdoorList的项目图标为不带面板的
        /// </summary>
        private void ResetListViewImage_Controller()
        {
            foreach (ListViewItem item in this.lvController_OutdoorList.Items)
            {
                item.ImageIndex = 0;
            }
        }


        private void jccmbControllerType_MouseClick(object sender, MouseEventArgs e)
        {
            currentControllerLayoutType = this.jccmbControllerType.Text;
        }

        string currentControllerLayoutType = "";
        private void jccmbControllerType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            //// updated on 20141231 clh
            //// 原来的做法如果先拖入了室外机，则切换模式时室外机以及关联的面板记录不会清空，会有问题

            //if (JCMsg.ShowConfirmOKCancel(Msg.CONTROLLER_CONFIRM_CHANGELAYOUT) == DialogResult.OK)
            //{
            //    BindToSource_ControllerLayoutType();
            //    bll.ClearControllerData();
            //    BindToControl_Controller();
            //}
            //else
            //{
            //    this.jccmbControllerType.Text = currentControllerLayoutType;
            //}

            long start = DateTime.Now.Ticks;

            //不再显示确认消息框，自动删除跟当前ControllerLayoutType冲突的controller
            BindToSource_ControllerLayoutType();
            RemoveInvalidBMSAdapter();

            //在Controller备选区，重新隐藏不可用的并显示可用的controller
            HideInvalidControllerType();

            //刷新Material List
            BindMaterialList();
        }

        //清除不符合当前ControllerLayoutType的BMS Adapter
        /// <summary>
        /// 清除不符合当前ControllerLayoutType的BMS Adapter
        /// </summary>
        public void RemoveInvalidBMSAdapter()
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            foreach (Control cSystem in this.pnlController_Center.Controls)
            {
                //控件类型检查 system level
                if (!(cSystem is ucDropControlSystem))
                {
                    continue;
                }

                //不能在foreach里面直接删除控件，先定义一个List存起来
                List<ucDropControlGroup> ucGroupsToRemove = new List<ucDropControlGroup>();
                foreach (Control cGroup in cSystem.Controls)
                {
                    //控件类型检查  group level
                    if (!(cGroup is ucDropControlGroup))
                    {
                        continue;
                    }

                    ucDropControlGroup ucGroup = cGroup as ucDropControlGroup;
                    int controllerQty = ucGroup.GetControllerQty();

                    //如果控制器数量为0则忽略
                    if (controllerQty == 0)
                    {
                        continue;
                    }

                    //不能在foreach里面直接删除控件，先定义一个List存起来
                    List<ucDropController> ucControllersToRemove = new List<ucDropController>();
                    foreach (Control cController in cGroup.Controls)
                    {
                        //控件类型检查 controller level
                        if (!(cController is ucDropController))
                        {
                            continue;
                        }
                        ucDropController ucController = cController as ucDropController;
                        bool needToRemove = false;

                        if (!ucController.IsActive)
                        {
                            continue;
                        }
                        switch (ucController.TypeInfo.Type)
                        {
                            case ControllerType.LonWorksInterface:
                                if (thisProject.ControllerLayoutType != ControllerLayoutType.LONWORKS)
                                    needToRemove = true;
                                break;
                            case ControllerType.ModBusInterface:
                                if (thisProject.ControllerLayoutType != ControllerLayoutType.MODBUS)
                                    needToRemove = true;
                                break;
                            case ControllerType.BACNetInterface:
                                if (thisProject.ControllerLayoutType != ControllerLayoutType.BACNET)
                                    needToRemove = true;
                                break;
                            case ControllerType.KNXInterface:
                                if (thisProject.ControllerLayoutType != ControllerLayoutType.KNX)
                                    needToRemove = true;
                                break;
                        }
                        if (needToRemove)
                        {
                            //将需要删除的ucDropController控件缓存起来，在循环以外删除
                            ucControllersToRemove.Add(ucController);
                        }
                    }

                    //删除ucDropController控件
                    ucControllersToRemove.ForEach(p =>
                    {
                        //从thisProject.ControllerList里移除相关数据
                        bll.RemoveController(p.Controller.Id);

                        //移除ucDropController控件
                        p.Remove();
                    });

                    int outQty = ucGroup.GetOutdoorQty();
                    controllerQty = ucGroup.GetControllerQty();
                    if (outQty == 0 && controllerQty == 0)
                    {
                        //将这一步骤中被清空的ucDropControlGroup缓存起来，在循环以外删除
                        ucGroupsToRemove.Add(ucGroup);
                    }
                }

                //删除已被清空的ucDropControlGroup
                ucGroupsToRemove.ForEach(p => p.Remove());
            }
        }

        void lvOutdoorList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                e.Item.ImageIndex = 1;
            }
            else
            {
                e.Item.ImageIndex = 0;
            }
        }

        void lvOutdoorList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is ListViewItem)
            {
                ListViewItem lvItem = (e.Item as ListViewItem);
                //如果tag是false, 表示此项不可拖动到group
                if (lvItem.Tag is Boolean && (bool)lvItem.Tag == false)
                {
                    return;
                }
            }
            (sender as ListView).DoDragDrop(e.Item, DragDropEffects.Move);
        }

        void lvOutdoorList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(type_ListViewItem, false))
            {
                e.Effect = DragDropEffects.Move;
            }
            //拖动重置状态
            thisProject.CentralControllerOK = false;
            SetTabControlImageKey();

            varificationCheck();
        }

        void lvOutdoorList_DragDrop(object sender, DragEventArgs e)
        {
            DoDropOutdoor(sender, e);
            ResetListViewImage_Controller();
            varificationCheck();
        }

        void pbCandidateController_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as PictureBox).DoDragDrop(sender as PictureBox, DragDropEffects.Move);
        }

        void pbCandidateController_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (UtilControl.CheckDragType_PictureBox(e))
            {
                PictureBox pb = (PictureBox)e.Data.GetData(type_PictureBox);
                if (pb.Image == null || pnlController_Left_1.Contains(pb))
                    e.Effect = DragDropEffects.None;
                else
                    e.Effect = DragDropEffects.Move;
            }
            //拖动重置状态
            thisProject.CentralControllerOK = false;
            SetTabControlImageKey();

            varificationCheck();
        }

        void pbCandidateController_DragDrop(object sender, DragEventArgs e)
        {
            DoDropController(sender, e);
        }

        void ucControlGroup_Relocation(object sender, EventArgs e)
        {
            ReLocationController(sender, e, this.pnlController_Center);
        }

        void ucControlGroup_AfterSetting(object sender, EventArgs e)
        {
            ucDropControlGroup ucGroup = sender as ucDropControlGroup;
            ProjectBLL bll = (new ProjectBLL(thisProject));
            ControlGroup group = bll.GetControlGroup(ucGroup.ControlGroupID);
            if (group != null)
            {
                group.SetName(ucGroup.Title);
            }
        }

        void ucControlGroup_BeforeRemove(object sender, EventArgs e)
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            if (sender is ListView)
            {
                ListView lv = sender as ListView;
                if (lv.Items.Count > 0)
                {
                    foreach (ListViewItem item in lv.Items)
                    {
                        item.Remove();
                        this.lvController_OutdoorList.Items.Add(item);
                        if (item.Name.Contains("Heat Exchanger"))
                        {
                            RoomIndoor ri = bll.GetExchangerByName(outExchangerName(item.Text));
                            ri.ControlGroupID[0] = "";
                        }
                        else
                        {

                            foreach (SystemVRF sysItem in thisProject.SystemList)
                            {
                                if (sysItem.Id.Equals(item.Name))
                                {
                                    sysItem.SetControlGroupID(new SystemVRF().ControlGroupID); 
                                }
                            }
                        }
                    }

                    ResetListViewImage_Controller();
                }

                // 删除SystemVRF对象与ControlGroup的关联关系
                //string groupID = (((sender as ListView).Parent as ucDropOutdoor).Parent as ucDropControlGroup).ControlGroupID;
                //bll.RemoveControlGroup(groupID);
            }
            // 单独删除Control Group中的一个ucController对象,需要删除这个Controller控件中关联的Controller对象
            else if (sender is ucDropController)
            {
                ucDropController ucController = (sender as ucDropController);
                Controller controller = ucController.Controller;
                if (controller != null)
                {
                    //删除相关的成套使用的控制器
                    RemoveInSetControllers(ucController.Parent as ucDropControlGroup, ucController.TypeInfo, 1);

                    //数量减少一个
                    controller.Quantity--;

                    if (controller.Quantity > 0)
                    {
                        ucController.UpdateQuantity();
                    }
                    else
                    {
                        bll.RemoveController(controller.Id);
                        ucController.Remove();
                    }
                }
            }

            BindMaterialList();

            this.pnlController_Center.VerticalScroll.Value = this.pnlController_Center.VerticalScroll.Minimum;
            //拖动重置状态
            thisProject.CentralControllerOK = false;
            SetTabControlImageKey();

            varificationCheck();
        }
        void ucControlGroup_BeforeAdd(object sender, EventArgs e)
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            if (sender is ucDropController)
            {
                ucDropController ucController = (sender as ucDropController);
                ucDropControlGroup ucGroup = ucController.Parent as ucDropControlGroup;
                if (ucController.Controller != null)
                {
                    ucController.Add(1);
                    AddControllerInSet(null, ucGroup, ucController.TypeInfo, 1);

                    BindMaterialList();
                }
            }
            varificationCheck();
        }

        void ucControlGroup_BeforeDrop(object sender, DragEventArgs e)
        {
            // 挪至ControlGroup控件中判断
        }

        void ucControlGroup_AfterDrop(object sender, DragEventArgs e)
        {
            ProjectBLL bll = (new ProjectBLL(thisProject));
            ShowWarningMsg("");
            int outQty = 0;
            int controllerQty = 0;
            if (sender is PictureBox)
            {
                bool isNewGroup = false;
                ucDropControlGroup ucGroup_target = null;
                ucDropControlSystem ucSystem_target = null;

                if ((sender as PictureBox).Parent.Parent is ucDropControlGroup)
                {
                    ucGroup_target = (sender as PictureBox).Parent.Parent as ucDropControlGroup;
                    ucSystem_target = ucGroup_target.Parent as ucDropControlSystem;
                    outQty = ucGroup_target.GetOutdoorQty();
                    controllerQty = ucGroup_target.GetControllerQty();
                    if (outQty == 0 && controllerQty == 0)
                    {
                        isNewGroup = true;
                    }
                }

                DoDropController(sender, e);

                if (ucGroup_target != null)
                {
                    controllerQty = ucGroup_target.GetControllerQty();
                    if (isNewGroup && controllerQty > 0)
                    {
                        ControlGroupNo++;
                        if (thisProject.ControlGroupList != null && thisProject.ControlGroupList.Count > 0)
                        {
                            while (true)
                            {
                                string name = "H-Link Group " + ControlGroupNo.ToString();
                                var gp = thisProject.ControlGroupList.Find(p => p.Name == name);
                                if (gp == null)
                                {
                                    break;
                                }
                                ControlGroupNo++;
                            }
                        }
                        ucGroup_target.Title = "H-Link Group " + ControlGroupNo.ToString();
                        ControlGroup item = bll.GetControlGroup(ucGroup_target.ControlGroupID);
                        item.SetName(ucGroup_target.Title);
                        item.IsValidGrp = true;
                        AddControlGroup(ucSystem_target);
                    }
                }
            }
            else if (sender is ListView)
            {
                DoDropOutdoor(sender, e);

                ucDropControlSystem ucSystem = (sender as ListView).Parent.Parent.Parent as ucDropControlSystem;
                ucDropControlGroup ucGroup = ((sender as ListView).Parent.Parent as ucDropControlGroup);
                outQty = (sender as ListView).Items.Count;
                controllerQty = ucGroup.GetControllerQty();
                if (outQty == 1 && controllerQty == 0)
                {
                    ControlGroupNo++;
                    if (thisProject.ControlGroupList != null && thisProject.ControlGroupList.Count > 0)
                    {
                        while (true)
                        {
                            string name = "H-Link Group " + ControlGroupNo.ToString();
                            var gp = thisProject.ControlGroupList.Find(p => p.Name == name);
                            if (gp == null)
                            {
                                break;
                            }
                            ControlGroupNo++;
                        }
                    }
                    ucGroup.Title = "H-Link Group " + ControlGroupNo.ToString();
                    ControlGroup item = bll.GetControlGroup(ucGroup.ControlGroupID);
                    if (item != null)
                    {
                        item.SetName(ucGroup.Title);
                        item.IsValidGrp = true;
                    }
                    AddControlGroup(ucSystem);
                }
            }
            //拖动重置状态
            thisProject.CentralControllerOK = false;
            SetTabControlImageKey();

            varificationCheck();

        }

        void ucControlSystem_ControlAdded(object sender, ControlEventArgs e)
        {
            ReLocationController(sender, e, this.pnlController_Center);
        }

        // 移除Control group || Controller
        void ucControlSystem_ControlRemoved(object sender, ControlEventArgs e)
        {
            ReLocationController(sender, e, this.pnlController_Center);
        }

        #endregion

        #region Main_Report
        UtilEMF utilEMF = new UtilEMF();

        /// 将各个模块在报告中是否输出的选项，绑定到当前项目
        /// <summary>
        /// 将各个模块在报告中是否输出的选项，绑定到当前项目
        /// </summary>
        private void saveModuleExportFlag()
        {
            thisProject.IsRoomInfoChecked = uc_CheckBox_RptRoomInfo.Checked;
            thisProject.IsIndoorListChecked = uc_CheckBox_RptIndoorDetail.Checked;
            //thisProject.IsOptionChecked = uc_CheckBox_RptIndoorOption.Checked;
            thisProject.IsOutdoorListChecked = uc_CheckBox_RptOutdoorDetail.Checked;
            thisProject.IsPipingDiagramChecked = uc_CheckBox_RptPiping.Checked;
            thisProject.IsWiringDiagramChecked = uc_CheckBox_RptWiring.Checked;
            thisProject.IsControllerChecked = uc_CheckBox_RptController.Checked;
            thisProject.IsExchangerChecked = uc_CheckBox_RptExchanger.Checked;
            //thisProject.IsOptionPriceChecked = uc_CheckBox_RptOptionPrice.Checked;

            uc_CheckBox_RptSelectAllModule.Checked =
                thisProject.IsRoomInfoChecked
                && thisProject.IsIndoorListChecked
                && thisProject.IsOutdoorListChecked
                //&& thisProject.IsOptionChecked 
                && thisProject.IsPipingDiagramChecked
                && thisProject.IsWiringDiagramChecked
                && thisProject.IsControllerChecked
                 && thisProject.IsExchangerChecked;
            //&& thisProject.IsOptionPriceChecked;
        }

        /// 根据当前项目的属性，绑定各个模块在报告中是否输出的选项
        /// <summary>
        /// 根据当前项目的属性，绑定各个模块在报告中是否输出的选项
        /// </summary>
        private void loadModuleExportFlag()
        {
            uc_CheckBox_RptRoomInfo.Checked = thisProject.IsRoomInfoChecked;
            uc_CheckBox_RptIndoorDetail.Checked = thisProject.IsIndoorListChecked;
            //uc_CheckBox_RptIndoorOption.Checked = thisProject.IsOptionChecked;
            uc_CheckBox_RptOutdoorDetail.Checked = thisProject.IsOutdoorListChecked;
            uc_CheckBox_RptPiping.Checked = thisProject.IsPipingDiagramChecked;
            uc_CheckBox_RptWiring.Checked = thisProject.IsWiringDiagramChecked;
            uc_CheckBox_RptController.Checked = thisProject.IsControllerChecked;
            uc_CheckBox_RptExchanger.Checked = thisProject.IsExchangerChecked;
            //uc_CheckBox_RptOptionPrice.Checked = thisProject.IsOptionPriceChecked;

            uc_CheckBox_RptSelectAllModule.Checked =
                thisProject.IsRoomInfoChecked
                && thisProject.IsIndoorListChecked
                && thisProject.IsOutdoorListChecked
                //&& thisProject.IsOptionChecked 
                && thisProject.IsPipingDiagramChecked
                && thisProject.IsWiringDiagramChecked
                && thisProject.IsControllerChecked
                  && thisProject.IsExchangerChecked;
            //&& thisProject.IsOptionPriceChecked;
        }

        #region AddFlow 导出矢量图（emf 或 wmf格式）

        /// 导出矢量图： Emf 或 Wmf 文件
        /// <summary>
        /// 导出矢量图： Emf 或 Wmf 文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        public bool ExportVictorGraph(string filePath, AddFlow addFlowItem, SystemVRF sysItem,bool isBackWhite=false)
        {
            try
            {
                Metafile emf = addFlowItem.ExportMetafile(false, true, true, false, false);

                Bitmap bmp = new Bitmap(emf.Size.Width, emf.Size.Height);
                Graphics gs = Graphics.FromImage(bmp);

                Metafile mf = new Metafile(filePath, gs.GetHdc());
                Graphics g = Graphics.FromImage(mf);
                string PipingImageDir = MyConfig.PipingNodeImageDirectory;
                if (isBackWhite)
                {
                    g.Clear(Color.White);
                }
                DrawEMF_Piping(g, addFlowItem, sysItem, PipingImageDir, thisProject);

                g.Save();
                g.Dispose();
                gs.Dispose();
                mf.Dispose();
                bmp.Dispose();
                emf.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// 绘制 EMF 图片(Piping)
        /// <summary>
        /// 绘制 EMF 图片(Piping)
        /// </summary>
        /// <param name="addFlow1"></param>
        /// <param name="g"></param>
        public void DrawEMF_Piping(Graphics g, AddFlow addFlowPiping, SystemVRF curSystemItem, string PipingImageDir, Project thisProject)
        {
            PipingBLL pipBll = new PipingBLL(_thisProject);
            bool isHitachi = thisProject.BrandCode == "H";
            bool isInch = CommonBLL.IsDimension_inch();
            bool isVertical = this.uc_CheckBox_PipingVertical.Checked;
            utilEMF.utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;//设置长度单位 add by axj 20161229
            utilEMF.isManualLength = curSystemItem.IsInputLengthManually;//设置是否输入管长 add by axj 20161229
            string title = "Additional Refrigerant Charge";//添加追加冷媒title add by axj 20161229
            foreach (Item item in addFlowPiping.Items)
            {
                if (item is Node)
                {
                    Node nd = item as Node;
                    if (nd is MyNodeYP)
                    {
                        if (!(nd as MyNodeYP).IsCP)
                        {
                            utilEMF.DrawYP(g, nd, false);
                        }
                        else
                        {
                            utilEMF.DrawYP(g, nd, true);
                        }
                    }
                    else if (nd is MyNodeOut)
                    {
                        // 绘制室外机组合内部的YP型号以及连接管管径数据
                        MyNodeOut ndOut = nd as MyNodeOut;
                        //string outModel = curSystemItem.OutdoorItem.Model;
                        string outModel = curSystemItem.OutdoorItem.Model_York; //根据PM要求室外机以及分机一律显示Model_York或Model_Hitachi 20180214 by Yunxiao Lin
                        //JTWH 室外机需要做SizeUp计算 add on 20160615 by Yunxiao Lin
                        NodeElement_Piping itemOut = pipBll.GetPipingNodeOutElement(curSystemItem, isHitachi);
                        string nodeImageFile = PipingImageDir + itemOut.Name + ".txt";

                        if (itemOut.UnitCount == 1)
                        {
                            if (isHitachi)
                                outModel = curSystemItem.OutdoorItem.Model_Hitachi;
                            utilEMF.DrawNode(g, nd, nodeImageFile);
                        }
                        else
                        {
                            outModel = curSystemItem.OutdoorItem.AuxModelName;
                            utilEMF.DrawNode_OutdoorGroup(g, nd, nodeImageFile, outModel, curSystemItem.Name, itemOut, isInch);
                        }
                    }
                    else if (nd is MyNodeIn)
                    {
                        MyNodeIn ndIn = nd as MyNodeIn;

                        NodeElement_Piping itemInd = pipBll.GetPipingNodeIndElement(ndIn.RoomIndooItem.IndoorItem.Type, isHitachi);
                        string nodeImageFile = PipingImageDir + itemInd.Name + ".txt";
                        utilEMF.DrawNode(g, nd, nodeImageFile);
                    }
                    else if (nd is MyNodeCH)
                    {
                        MyNodeCH ndCH = nd as MyNodeCH;
                        string nodeImageFile = PipingImageDir + "CHbox.txt";
                        utilEMF.DrawNode(g, nd, nodeImageFile);
                    }
                    else if (nd is MyNodeMultiCH)
                    {
                        MyNodeMultiCH ndMCH = nd as MyNodeMultiCH;
                        string nodeImageFile = PipingImageDir + "MultiCHbox.txt";
                        utilEMF.DrawNode(g, nd, nodeImageFile);
                    }
                    else if (nd is MyNodeLegend)
                    {
                        //绘制图例文字
                        utilEMF.DrawText(g, nd);
                    }
                    else if (nd.Tooltip == title)//添加追加冷媒文字 add by axj 20161229
                    {
                        utilEMF.DrawText(g, nd);
                    }
                    else if (nd.Text != null && nd.Text.Contains("Cooling") && nd.Text.Contains("Heating"))//添加实际容量文字 add by axj 20161229
                    {
                        utilEMF.DrawActualCapacityText(g, nd);
                    }
                    else
                    {
                        utilEMF.DrawLabelText(g, nd);
                    }
                }
                else if (item is Link)
                {
                    utilEMF.DrawLine(g, (Link)item);
                }
            }
        }



        /// 导出矢量图： Emf 或 Wmf 文件 (Wiring 导出DXF)
        /// <summary>
        /// 导出矢量图： Emf 或 Wmf 文件 (Wiring)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        private bool ExportVictorGraph_wiring(string filePath, bool isBackWhite = false)
        {
            try
            {
                string imageDir = MyConfig.WiringNodeImageDirectory;

                Metafile emf = this.addFlowWiring.ExportMetafile(false, true, true, false, false);

                Bitmap bmp = new Bitmap(emf.Size.Width, emf.Size.Height);
                Graphics gs = Graphics.FromImage(bmp);
                Metafile mf = new Metafile(filePath, gs.GetHdc());

                Graphics g = Graphics.FromImage(mf);
                if (isBackWhite)
                {
                    g.Clear(Color.White);
                }
                DrawEMF_wiring(g, addFlowWiring, curSystemItem, imageDir);

                g.Save();
                g.Dispose();
                gs.Dispose();
                mf.Dispose();
                bmp.Dispose();
                emf.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// 绘制 EMF 图片 (Wiring 导出DXF)
        /// <summary>
        /// 绘制 EMF 图片 (Wiring)
        /// </summary>
        /// <param name="addFlow1"></param>
        /// <param name="g"></param>
        private void DrawEMF_wiring(Graphics g, AddFlow addFlowWiring, SystemVRF curSystemItem, string WiringImageDir)
        {
            PointF ptf1 = new PointF(0, 0);
            PointF ptf2 = new PointF(0, 0);
            PointF ptf3 = new PointF(0, 0);

            //try
            //{   
            foreach (Item item in addFlowWiring.Items)
            {
                if (item is Node)
                {
                    Node nd = item as Node;
                    if (nd is WiringNodeOut)
                    {

                        WiringNodeOut ndOut = nd as WiringNodeOut;
                        //NodeElement_Wiring item_wiring = GetNodeElement_Piping_wiring("Out", curSystemItem.OutdoorItem.Model);
                        NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_ODU(curSystemItem.OutdoorItem, thisProject.BrandCode);

                        //string nodePointsFile = FileLocal.GetNodeImageTextWiring(item_wiring.KeyName);
                        string nodePointsFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");

                        utilEMF.DrawNode_wiring(g, ndOut, nodePointsFile, curSystemItem.Name, item_wiring);

                    }
                    else if (nd is WiringNodeIn)
                    {

                        WiringNodeIn ndIn = nd as WiringNodeIn;
                        //string sModel = ndIn.RoomIndooItem.IndoorItem.Model;
                        //NodeElement_Wiring item_wiring = pipBll.GetNodeElement_Piping_wiring("Ind", sModel);
                        //NodeElement_Wiring item_wiring = utilEMF.GetNodeElement_Wiring_IDU(ndIn.RoomIndoorItem.IndoorItem, thisProject.BrandCode, curSystemItem.OutdoorItem.Type);
                        int powerIndex = 0;
                        bool isNewPower = false;
                        NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_IDU(ndIn.RoomIndoorItem.IndoorItem, thisProject.BrandCode, curSystemItem.OutdoorItem.Type, ref strArrayList_powerType, ref strArrayList_powerVoltage, ref dArrayList_powerCurrent, ref powerIndex, ref isNewPower);
                        //string nodePointsFile = FileLocal.GetNodeImageTextWiring(item_wiring.KeyName);
                        string nodePointsFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                        utilEMF.DrawNode_wiring(g, ndIn, nodePointsFile, ndIn.RoomIndoorItem.IndoorName, item_wiring);
                    }
                    else if (nd is WiringNodeCH)
                    {
                        //WiringNodeCH ndCH = nd as WiringNodeCH;
                        //NodeElement_Wiring item_wiring = utilEMF.GetNodeElement_Wiring_CH(ndCH);
                        //string nodePointsFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                        //utilEMF.DrawNode_wiring(g, ndCH, nodePointsFile, "CH Unit", item_wiring);
                        utilEMF.DrawNode(g, nd);
                    }
                    else if (nd is MyNodeGround_Wiring)
                    {
                        MyNodeGround_Wiring ndGnd = nd as MyNodeGround_Wiring;
                        string nodePointsFile = Path.Combine(WiringImageDir, "Ground.txt");
                        utilEMF.DrawNode_wiring(g, ndGnd, nodePointsFile, "", null);
                    }
                    else if (nd is MyNodeRemoteControler_Wiring)
                    {

                        MyNodeRemoteControler_Wiring ndRC = nd as MyNodeRemoteControler_Wiring;
                        string nodePointsFile = Path.Combine(WiringImageDir, "RemoteControler.txt");
                        utilEMF.DrawNode_wiring(g, ndRC, nodePointsFile, "", null);
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(nd.Text))
                        {
                            PointF pf = nd.Location;
                            //g.DrawString(nd.Text, utilEMF.textFont_wiring, utilEMF.textBrush_wiring, pf);
                            if (nd.Text != "//" && nd.Text != "///" && nd.Text != "////")
                                g.DrawString(nd.Text, utilEMF.textFont_wiring, utilEMF.textBrush_wiring, pf);
                            else
                            {
                                Brush brush = new SolidBrush(Color.Red);
                                pf.Y += 2.5f;
                                g.DrawString(nd.Text, utilEMF.textFont_wiring, brush, pf);
                            }
                        }
                    }
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    LogHelp.WriteLog(n.ToString()+"  "+ex.Message, ex);
            //}

            foreach (PointF[] pt in ptArrayList)
            {
                Pen pen = new Pen(Color.Cyan, 0.1f);
                g.DrawLines(pen, pt);
                //g.DrawLines(utilEMF.pen_wiring, pt);
            }
            foreach (PointF[] pt in ptArrayList_RC)
            {
                Pen pen = new Pen(Color.Blue, 0.1f);
                g.DrawLines(pen, pt);
                //g.DrawLines(utilEMF.pen_wiring, pt);
            }
            
            foreach (PointF[] pt in ptArrayList_power)
            {
                Pen pen = new Pen(Color.Red, 0.3f);
                g.DrawLines(pen, pt);
                //g.DrawLines(utilEMF.pen_wiring_power, pt); 
            }

            foreach (PointF[] pt in ptArrayList_ground)
            {
                Pen pen = new Pen(Color.LightGreen, 0.1f);
                PointF[] pt1 = pt;
                if (pt.Length > 2)
                {
                    pt1 = new PointF[] { pt[0], pt[1] };
                }
                g.DrawLines(pen, pt1);
                //g.DrawLines(utilEMF.pen_wiring_dash, pt); 
            }
        }

        /// 导出矢量图： Emf 或 Wmf 文件 (Wiring 导出DXF)
        /// <summary>
        /// 导出矢量图： Emf 或 Wmf 文件 (Wiring)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        private bool ExportVictorGraph_ContollerWiring(string filePath, ControlGroup group, bool isBackWhite = false)
        {
            try
            {
                Metafile emf = this.addFlowControllerWiring.ExportMetafile(false, true, true, false, false);
                Bitmap bmp = new Bitmap(emf.Size.Width, emf.Size.Height);
                Graphics gs = Graphics.FromImage(bmp);
                Metafile mf = new Metafile(filePath, gs.GetHdc());
                Graphics g = Graphics.FromImage(mf);
                if(isBackWhite)
                {
                    g.Clear(Color.White);
                }
                DrawEMF_ControllerWiring(g, addFlowControllerWiring, group, MyConfig.WiringNodeImageDirectory);   
                             
                g.Save();              
                g.Dispose();
                gs.Dispose();
                mf.Dispose();
                bmp.Dispose();
                emf.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// 绘制 EMF 图片 (Wiring 导出DXF)
        /// <summary>
        /// 绘制 EMF 图片 (Wiring)
        /// </summary>
        /// <param name="addFlow1"></param>
        /// <param name="g"></param>
        private void DrawEMF_ControllerWiring(Graphics g, AddFlow addFlowItem, ControlGroup group, string WiringImageDir)
        {
            foreach (Item item in addFlowItem.Items)
            {
                utilEMF.DrawItemRecursion(g, item, WiringImageDir);
            }
        }

        #endregion

        #region 保存为 DXF 文件
        private CADImage cadImage;
        private Thread loadFileThread;  // 加载 emf 文件的辅线程对象

        #region 加载 emf 文件，新线程
        // 加载 emf 文件到当前的 cadImage 对象
        /// <summary>
        /// 加载 emf 文件到当前的 cadImage 对象
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadEMFFile(string fileName)
        {
            if (loadFileThread != null)
            {
                if (loadFileThread.IsAlive)
                    return;
            }

            if (fileName != null)
            {
                if (cadImage != null)
                {
                    cadImage.Dispose();
                    cadImage = null;
                }

                this.cadImage = CADImage.CreateImageByExtension(fileName);
            }

            if (this.cadImage != null)
            {
                CADImage.CodePage = System.Text.Encoding.Default.CodePage;//note - charset was set here
                CADImage.LastLoadedFilePath = Path.GetDirectoryName(fileName);
                //CreateNewLoadThread(fileName);                
                cadImage.LoadFromFile(fileName);
            }

            //while (!loadFileThread.IsAlive);// 等待线程终止
            //Thread.Sleep(10);


            //loadFileThread.Join();
        }

        /// <summary>
        /// 创建新线程
        /// </summary>
        /// <param name="fileName"></param>
        private void CreateNewLoadThread(string fileName)
        {
            loadFileThread = new Thread(LoadCADImage);
            loadFileThread.Name = "LoadFileThread";
            loadFileThread.IsBackground = true;
            loadFileThread.Start(fileName);
        }

        /// <summary>
        /// 将 emf 文件加载到当前的 cadImage 对象
        /// </summary>
        /// <param name="fileNameObj"></param>
        private void LoadCADImage(object fileNameObj)
        {
            // 锁定线程
            lock (cadImage)
            {
                string fileName = (string)fileNameObj;
                cadImage.LoadFromFile(fileName);
            }
        }

        #endregion

        #region 保存 DXF 文件
        public bool SaveAsDXF()
        {
            if (this.cadImage == null)
                return false;

            string fname = "";
            if (this.cadImage.FileInf != null)
                fname = this.cadImage.FileInf.FullName.Replace(this.cadImage.FileInf.Extension, ".dxf");
            if (string.IsNullOrEmpty(fname))
                return false;
            return SaveAsDXF(fname);    //this.dlgSaveDXF.FileName
        }

        /// <summary>
        /// Emf Convert To DXF
        /// </summary>
        /// <param name="fName"></param>
        private bool SaveAsDXF(string fName)
        {
            if (cadImage is CADImport.RasterImage.CADRasterImage && cadImage.FileInf != null)
            {
                if (cadImage.FileInf.Extension.ToUpper() == ApplicationConstants.emfextstr)
                {
                    //cadImage.EmfConvertToDXF(this.cadImage.FileInf.FullName, this.cadPictBox.Handle, fName); // 9 之前的版本
                    cadImage.EmfConvertToDXF(this.cadImage.FileInf.FullName, fName); // 新版本

                    return true;
                }
            }
            return false;
        }
        #endregion


        #endregion

        /// <summary>
        /// 图片处理
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public Image PictureProcess(Image sourceImage, int targetWidth, int targetHeight)
        {
            int width;//图片最终的宽  
            int height;//图片最终的高  
            try
            {
                System.Drawing.Imaging.ImageFormat format = sourceImage.RawFormat;
                using (Bitmap targetPicture = new Bitmap(targetWidth, targetHeight))
                {
                    
                    Graphics g = Graphics.FromImage(targetPicture);
                    g.Clear(Color.White);

                    //计算缩放图片的大小  
                    if (sourceImage.Width > targetWidth && sourceImage.Height <= targetHeight)
                    {
                        width = targetWidth;
                        height = (width * sourceImage.Height) / sourceImage.Width;
                    }
                    else if (sourceImage.Width <= targetWidth && sourceImage.Height > targetHeight)
                    {
                        height = targetHeight;
                        width = (height * sourceImage.Width) / sourceImage.Height;
                    }
                    else if (sourceImage.Width <= targetWidth && sourceImage.Height <= targetHeight)
                    {
                        width = sourceImage.Width;
                        height = sourceImage.Height;
                    }
                    else
                    {
                        width = targetWidth;
                        height = (width * sourceImage.Height) / sourceImage.Width;
                        if (height > targetHeight)
                        {
                            height = targetHeight;
                            width = (height * sourceImage.Width) / sourceImage.Height;
                        }
                    }
                    g.DrawImage(sourceImage, (targetWidth - width) / 2, (targetHeight - height) / 2, width, height);
                    sourceImage.Dispose();

                    return targetPicture; 
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }  


        /// 保存当前系统的配管图图片文件，Report中需要获取该图片文件
        /// <summary>
        /// 保存当前系统的配管图图片文件，Report中需要获取该图片文件
        /// </summary>
        /// <param name="sysNO">当前系统的编号</param>
        private void DoSavePipingFilePicture(string sysNO, string outputDir = null)
        {
            string filePath_pic = FileLocal.GetNamePathPipingPicture(sysNO, outputDir);
            //addFlowPiping.BackColor = Color.White;
            //Metafile mf = this.addFlowPiping.ExportMetafile(false, true, false, true);
            //mf.Save(filePath_pic, ImageFormat.Jpeg);
            //mf.Save(filePath_pic);
            if (outputDir != null)
            {
                string fileName = Application.StartupPath + "\\temp" + Guid.NewGuid().ToString() + ".emf";//
                ExportVictorGraph(fileName, addFlowPiping, curSystemItem, true);
                Bitmap bmp = new Bitmap(fileName);
                Image img = PictureProcess(bmp, Convert.ToInt32(bmp.Width * 1.25d), Convert.ToInt32(bmp.Height * 1.25));
                img.Save(filePath_pic, ImageFormat.Jpeg);
                bmp.Dispose();
                img.Dispose();
            }
            else
            {
                ExportVictorGraph(filePath_pic, addFlowPiping, curSystemItem, true);
            }
        }

        /// 保存当前系统的配线图图片，插入Report中
        /// <summary>
        /// 保存当前系统的配线图图片，插入Report中
        /// </summary>
        /// <param name="addFlow1"></param>
        /// <param name="sysNO"></param>
        private void DoSaveWiringFilePicture(string sysNO, string outputDir = null)
        {
            string filePath_pic = FileLocal.GetNamePathWiringPicture(sysNO, outputDir);
            //Metafile mf = addFlowWiring.ExportMetafile(false, true, true);
            //mf.Save(filePath_pic, ImageFormat.Jpeg);
            //Bitmap bmp = ControlImage.GetAddFlow(addFlowWiring);
            //bmp.Save(filePath_pic, ImageFormat.Jpeg);
            if (outputDir != null)
            {
                string fileName = Application.StartupPath + "\\temp" + Guid.NewGuid().ToString() + ".emf";//
                ExportVictorGraph_wiring(fileName,  true);
                Bitmap bmp = new Bitmap(fileName);
                Image img = PictureProcess(bmp, Convert.ToInt32(bmp.Width * 1.25d), Convert.ToInt32(bmp.Height * 1.25));
                img.Save(filePath_pic, ImageFormat.Jpeg);
                bmp.Dispose();
                img.Dispose();
            }
            else
            {
                ExportVictorGraph_wiring(filePath_pic, true);
            }

        }

        /// 保存当前Controller Group的配线图图片，插入Report中
        /// <summary>
        /// 保存当前Controller Group的配线图图片，插入Report中
        /// </summary>
        /// <param name="addFlow1"></param>
        /// <param name="sysNO"></param>
        private void DoSaveControllerWiringFilePicture(string groupId,ControlGroup group=null, string outputDir = null)
        {
            string filePath_pic = FileLocal.GetNamePathControllerWiringPicture(groupId, outputDir);
            //Metafile mf = addFlowControllerWiring.ExportMetafile(false, true, true);
            //mf.Save(filePath_pic, ImageFormat.Jpeg);

            if (outputDir != null)
            {
                try
                {                  
                    string fileName = Application.StartupPath + "\\temp" + Guid.NewGuid().ToString() + ".emf";//
                    ExportVictorGraph_ContollerWiring(fileName, group, true);
                    Bitmap bmp = new Bitmap(fileName);
                    Image img = PictureProcess(bmp, Convert.ToInt32(bmp.Width * 1.25d), Convert.ToInt32(bmp.Height * 1.25));
                    Graphics g = Graphics.FromImage(img);
                    bmp = new Bitmap(img.Width, img.Height, g);
                    bmp.Save(filePath_pic, ImageFormat.Jpeg);
                    bmp.Dispose();                    
                    g.Dispose();
                    img.Dispose();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Save controller wiring drawing failes. "+ex.Message);
                }
            }
            else
            {
                ExportVictorGraph_ContollerWiring(filePath_pic, group, true);
            }
        }

        /// 保存 Controller 图片，用于插入Report中
        /// <summary>
        /// 保存 Controller 图片，用于插入Report中
        /// </summary>
        private void DoSaveControllerPicture()
        {
            //Pen pen = new Pen(Color.FromArgb(48, 48, 48));

            //Bitmap bitmap = new Bitmap(this.pnlController_Center.Width, this.pnlController_Center.Height);
            //this.pnlController_Center.DrawToBitmap(bitmap, this.pnlController_Center.Bounds);// 只能保存截图,不包括滚动条内部，不符合需要


           // SetEmptyController(false);  //将空白的控件隐藏

            string filePath_pic = FileLocal.GetNamePathControllerPicture("1"); // 当前只支持一个Control System的图像保存，TODO：支持多个待完善

            Control pControl = this.pnlController_Center.Parent;
            DockStyle dockstyle = this.pnlController_Center.Dock;
            bool oldAutoSize = this.pnlController_Center.AutoSize;
            this.pnlController_Center.Dock = DockStyle.None;
            int oldleft = this.pnlController_Center.Left;
            this.pnlController_Center.Parent = this;
            this.pnlController_Center.Left = 5000;
            this.pnlController_Center.AutoSize = true;
            this.pnlController_Center.Visible = true;
            this.pnlController_Center.BringToFront();
            this.pnlController_Center.Refresh();
            //GetSystemControllerHeight() 获取SystemController高度（除验证不通过） on 20180404 by xyj
            Bitmap bmp1 = new Bitmap(this.pnlController_Center.ClientSize.Width, GetSystemControllerHeight() == 0 ? 1 : GetSystemControllerHeight());
            this.pnlController_Center.DrawToBitmap(bmp1, new Rectangle(0, 0, bmp1.Width, bmp1.Height)); 

            //pnlController_Center.BackColor = Color.White;
            //Bitmap bmp = ControlImage.GetPanel(pnlController_Center);


            bmp1.Save(filePath_pic, ImageFormat.Jpeg);
            SetEmptyController(true);   //将空白的控件恢复显示
            this.pnlController_Center.Parent = pControl;
            this.pnlController_Center.Left = oldleft;
            this.pnlController_Center.Dock = dockstyle;
            this.pnlController_Center.AutoSize = oldAutoSize;
            bmp1.Dispose();
        }

        //private Boolean GetControllerPicture()
        //{
        //    string filePath_pic = FileLocal.GetNamePathControllerPicture("1"); // 当前只支持一个Control System的图像保存，支持多个待完善
        //    //Bitmap bmp = ControlImage.GetPanel(pnlController_Center);
        //    Bitmap bmp = null;
        //    Byte[] Systemblob = null;
        //    Systemblob = CDF.CDF.GetProjectBlob(glProject.ProjectID, 2, 1);
        //    if (Systemblob != null)
        //    {
        //        MemoryStream stream1 = new MemoryStream(Systemblob);

        //        BinaryFormatter bf = new BinaryFormatter();
        //        bmp = (Bitmap)bf.Deserialize(stream1);
        //        stream1.Dispose();
        //        stream1.Close();
        //        bmp.Save(filePath_pic, ImageFormat.Jpeg);
        //        return true;
        //    }
        //    else
        //    {
        //        string SaveProject = Msg.GetResourceString("Msg_SaveProject");
        //        JCMsg.ShowInfoOK(SaveProject);
        //        return false;
        //    }
        //}

        /// 设置Controller界面中空白控件的显示状态，仅用于Report输出Controller布局截图
        /// <summary>
        /// 设置Controller界面中空白控件的显示状态，仅用于Report输出Controller布局截图
        /// </summary>
        /// <param name="isVisible"></param>
        private void SetEmptyController(bool isVisible)
        {
            foreach (Control c in this.pnlController_Center.Controls)
            {
                if (c is ucDropControlSystem)
                {
                    ucDropControlSystem ucSystem = c as ucDropControlSystem;
                    if (ucSystem.IsActive())
                    {
                        int ucGroupCount = 0;
                        if (isVisible)
                        {
                            AddControlGroup(ucSystem);
                        }

                        foreach (Control c1 in ucSystem.Controls)
                        {
                            if (c1 is ucDropControlGroup)
                            {
                                ucDropControlGroup ucGroup = c1 as ucDropControlGroup;

                                if (!ucGroup.IsActive)
                                {
                                    if (!isVisible)
                                        ucSystem.Controls.Remove(ucGroup);
                                }
                                else
                                {
                                    ucGroupCount++;
                                    foreach (Control c2 in ucGroup.Controls)
                                    {
                                        if (c2 is ucDropOutdoor && !(c2 as ucDropOutdoor).IsActive)
                                        {
                                            (c2 as ucDropOutdoor).Visible = isVisible;
                                        }
                                        else if (c2 is ucDropController && !(c2 as ucDropController).IsActive)
                                        {
                                            (c2 as ucDropController).Visible = isVisible;
                                        }
                                    }
                                }
                            }
                            else if (c1 is ucDropController && !(c1 as ucDropController).IsActive)
                            {
                                (c1 as ucDropController).Visible = isVisible;
                            }
                        }

                        int height = (new ucDropControlGroup(_mainRegion)).Size.Height;
                        if (!isVisible && ucGroupCount == 1)
                        {
                            pnlController_Center.Dock = DockStyle.Top;
                        }
                        else
                        {
                            pnlController_Center.Dock = DockStyle.Fill;
                        }
                    }
                    else
                    {
                        ucSystem.Visible = isVisible;
                    } 
                }
                
            }
          
        }


        /// 返回Controller除验证不通过的SystemController高度，仅用于Report输出Controller布局截图
        /// <summary>
        /// 设置Controller界面中空白控件的显示状态，仅用于Report输出Controller布局截图
        /// </summary>
        /// <param name="isVisible"></param>
        private int GetSystemControllerHeight()
        {
            int Height = 0;
            foreach (Control c in this.pnlController_Center.Controls)
            {
                if (c is ucDropControlSystem)
                {
                    ucDropControlSystem ucSystem = c as ucDropControlSystem;
                    if (ucSystem.IsActive())
                    {
                        foreach (Control c1 in ucSystem.Controls)
                        {
                            if (c1 is ucDropControlGroup)
                            {
                                ucDropControlGroup ucGroup = c1 as ucDropControlGroup; 
                                if (!ucGroup.IsActive)
                                {
                                    ucSystem.Controls.Remove(ucGroup);
                                }
                            }
                        }
                        Height = ucSystem.Height; 
                    }
                }
            }
            return Height;
        }

        #endregion

        #endregion
        
        private void contextMenuStripPiping_Opening(object sender, CancelEventArgs e)
        {
            //在处理piping右键事件前先判断当前对象是节点还是线段 20171127 by Yunxiao Lin
            Node currNode = null;
            if (addFlowPiping.PointedItem is MyLink)
                currNode = (addFlowPiping.PointedItem as MyLink).Dst;
            else if (addFlowPiping.PointedItem is MyNode)
                currNode = addFlowPiping.PointedItem as Node;
            bool menuVisible = false;
            if (currNode == null)
            {
                e.Cancel = true; //不加这一句，右键菜单还是会显示。20171127 by Yunxiao Lin
                return;
            }

            //右键菜单转换--wq2016-05-09
            setCoolingOnlyToolStripMenuItem.Text = Msg.GetResourceString("UseofCoolingOperation");
            cancelCoolingOnlyToolStripMenuItem.Text = Msg.GetResourceString("CancelUseofCoolingOperation");
            exchangeIndoorsToolStripMenuItem.Text = Msg.GetResourceString("PIPING_EXCHANGE_INDOORS");

            foreach (ToolStripItem menu in contextMenuStripPiping.Items)
            {
                menu.Visible = false;
            }

            if (currNode is MyNodeIn)
            {
                selNode2 = currNode;

                if (selNode != null && selNode2 != selNode)
                {
                    exchangeIndoorsToolStripMenuItem.Visible = true;
                    menuVisible = true;
                }
            }

            //判断是否需要弹出设置单冷的右键菜单
            if (curSystemItem == null || addFlowPiping.SelectedItem == null || selNode == null ||
                !PipingBLL.IsHeatRecovery(curSystemItem))
            {
            }
            else
            {
                if (selNode is MyNodeCH)
                {
                    //CH-Box右键只能显示Set Cooling Only
                    setCoolingOnlyToolStripMenuItem.Visible = true;
                    menuVisible = true;
                }
                else if (selNode is MyNodeMultiCH)
                {
                    setCoolingOnlyToolStripMenuItem.Visible = true;
                    convertToCHBoxToolStripMenuItem.Visible = true;
                    deleteMultiCHBoxToolStripMenuItem.Visible = true;
                    menuVisible = true;
                }
                else if (selNode is MyNodeYP)
                {
                    //分歧管和梳形管需要先判断原来的状态
                    MyNodeYP yp = selNode as MyNodeYP;
                    if (yp.IsCoolingonly)
                    {
                        cancelCoolingOnlyToolStripMenuItem.Visible = true;
                        menuVisible = true;
                    }
                    else
                    {
                        setCoolingOnlyToolStripMenuItem.Visible = true;
                        menuVisible = true;
                    }
                }
                else if (selNode is MyNodeIn)
                {
                    //Indoor也需要先判断原来的状态
                    MyNodeIn noodIn = selNode as MyNodeIn;
                    if (noodIn.IsCoolingonly)
                    {
                        cancelCoolingOnlyToolStripMenuItem.Visible = true;
                        menuVisible = true;
                    }
                    else
                    {
                        setCoolingOnlyToolStripMenuItem.Visible = true;  
                        menuVisible = true;
                    }
                }
            }

            if (!menuVisible)
            {
                e.Cancel = true;
            }
        }

        private void deleteMultiCHBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selNode == null || !(selNode is MyNodeMultiCH)) return;

            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                return;
            }

            PipingBLL pipBll = GetPipingBLLInstance();
            Node child = pipBll.DeleteMultiCHBox(selNode as MyNodeMultiCH);
            PipingBLL.EachNode(child, (node) =>
            {
                if (node is MyNodeIn)
                {
                    //为每一个Indoor添加CH Box
                    pipBll.InsertCHBox(node);
                }
            });

            //重置状态
            SetSystemPipingOK(curSystemItem, false);

            //重新绘图
            DoDrawingPiping(true);
        }

        private void convertToCHBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selNode == null || !(selNode is MyNodeMultiCH)) return;

            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                return;
            }

            PipingBLL pipBll = GetPipingBLLInstance();
            pipBll.ReplaceMultiCH2CH(selNode as MyNodeMultiCH);

            //重置状态
            SetSystemPipingOK(curSystemItem, false);

            //重新绘图
            DoDrawingPiping(true);
        }

        private void setCoolingOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selNode == null) return;
            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                return;
            }
            Node selPNode = selNode.ParentNode;

            PipingBLL pipBll = GetPipingBLLInstance();
            //Set Cooling Only
            if (selNode is MyNodeIn)
            {
                MyNodeIn nodeIn = selNode as MyNodeIn;
                //如果选中节点是Indoor,搜索上层节点是否存在CH，直到firstChildNode，如果发现CH，则删除。
                pipBll.DeleteCHBoxUpward(nodeIn);
            }
            else if (selNode is MyNodeCH || selNode is MyNodeMultiCH)
            {
                //如果选中节点是CH，则删除CH，将前后节点直接相连
                pipBll.DeleteCHBoxUpward(selPNode);
                pipBll.DeleteCHBoxDownward(selNode);
            }
            else if (selNode is MyNodeYP)
            {
                MyNodeYP nodeYP = selNode as MyNodeYP;
                //如果选中节点是YP，则需要级联搜索上下层是否存在CH，如果发现CH，则删除。
                pipBll.DeleteCHBoxUpward(selPNode);
                pipBll.DeleteCHBoxDownward(nodeYP);
            }

            //删除CHBOX，重置状态
            SetSystemPipingOK(curSystemItem, false);

            //重新绘图
            DoDrawingPiping(true);
        }

        private void cancelCoolingOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selNode == null) return;

            //Cancel Cooling Only
            if (selNode is MyNodeIn || selNode is MyNodeYP)
            {
                if (ManualPipingCleanConfirmation() != DialogResult.Yes)
                {
                    return;
                }

                PipingBLL pipBll = GetPipingBLLInstance();
                pipBll.InsertCHBox(selNode);

                //重新绘图
                DoDrawingPiping(true);
            }
        }

        private void jctxtAltitude_MouseDown(object sender, MouseEventArgs e)
        {
            //this.altitudetextIsAutoChange = false;
        }

        private void jctxtAltitude_Leave(object sender, EventArgs e)
        {
            //当项目启用海拔高度修正时，修改海拔高度会引起室外机工况变化，重新选型 20160819 by Yunxiao Lin
            if (JCValidateSingle(jctxtAltitude))
            {
                if (thisProject.Altitude != Convert.ToInt32(Unit.ConvertToSource(double.Parse(jctxtAltitude.Text.Trim()), UnitType.LENGTH_M, ut_length)))
                {
                    if (thisProject.EnableAltitudeCorrectionFactor)
                    {
                        //弹出警告提示，让用户确认是否继续
                        if (JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGEALTITUDE()) == DialogResult.Cancel)
                        {
                            jctxtAltitude.Text = Unit.ConvertToControl(thisProject.Altitude, UnitType.LENGTH_M, ut_length).ToString("n0");
                            return;
                        }
                        else
                        {
                            thisProject.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(jctxtAltitude.Text.Trim()), UnitType.LENGTH_M, ut_length));
                            foreach (TreeNode tnOut in tvOutdoor.Nodes)
                            {
                                // TODO: 室外机重新选型，注意可能会有选型失败的情况出现 20160819 by Yunxiao Lin
                                SystemVRF sysItem = tnOut.Tag as SystemVRF;
                                //是否自动模式的判断  add by Shen Junjie on 20170619
                                if (sysItem != null && sysItem.IsAuto)
                                {
                                    List<string> ERRList = new List<string>();
                                    List<string> MSGList = new List<string>();
                                    List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(sysItem.Id);
                                    //Global.DoSelectOutdoor(sysItem, listRISelected, thisProject, out ERRList);
                                    //室外机选型统一改用新逻辑 Global.DoSelectOutdoorODUFirst 20161112 by Yunxiao Lin
                                    SelectOutdoorResult result = Global.DoSelectOutdoorODUFirst(sysItem, listRISelected, thisProject, out ERRList, out MSGList);
                                    Global.BindTreeNodeOut(tnOut, sysItem, listRISelected, thisProject);
                                }
                                sysItem.IsUpdated = true;
                            }
                            //重新选型之后需要判断并设置TabPage状态 20160819 by Yunxiao Lin
                            SetTabControlImageKey();
                        }
                    }
                    else
                        thisProject.Altitude = Convert.ToInt32(Unit.ConvertToSource(double.Parse(jctxtAltitude.Text.Trim()), UnitType.LENGTH_M, ut_length));
                }
            }
        }

        /// <summary>
        /// 校验控制器选型 20160830 by Yunxiao Lin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool varificationCheck()
        {
            //校验Central Controller
            //如果Project内没有分配任何系统和控制器，则检验通过
            thisProject.CentralControllerOK = true;
            ShowWarningMsg("");
            if (thisProject.ControlGroupList.Count != 0)
            {
                List<Controller> groupControllerList = new List<Controller>(); //当前Group中Control列表
                List<SystemVRF> groupSystemList = new List<SystemVRF>(); //当前Group中System列表
                List<string> groupSysProductType = new List<string>(); //当前Group中System的ProductType的列表
                List<string> groupControProductType = new List<string>(); //当前Group中Controller的ProductType的列表
                MyProductTypeBLL productTypeBll = new MyProductTypeBLL();


                foreach (ControlGroup cg in thisProject.ControlGroupList)
                {
                    List<RoomIndoor> indoorList = new List<RoomIndoor>();
                    string groupID = cg.Id;
                    groupControllerList.Clear();
                    groupSystemList.Clear();
                    groupSysProductType.Clear();

                    foreach (SystemVRF sysItem in thisProject.SystemList)
                    {
                        if (!string.IsNullOrEmpty(sysItem.ControlGroupID[0]))
                        {
                            if (sysItem.ControlGroupID[0] == groupID)
                            {
                                groupSystemList.Add(sysItem);
                                if (sysItem.OutdoorItem != null)
                                {
                                    if (!groupSysProductType.Contains(sysItem.OutdoorItem.ProductType))
                                        groupSysProductType.Add(sysItem.OutdoorItem.ProductType);

                                }
                            }

                        }

                    }
                    foreach (RoomIndoor ri in thisProject.ExchangerList)
                    {
                        if (!string.IsNullOrEmpty(ri.ControlGroupID[0]))
                        {
                            if (ri.ControlGroupID[0] == groupID)
                            {
                                indoorList.Add(ri);

                            }

                        }

                    }



                    //当前Group中Control列表
                    foreach (Controller c in thisProject.ControllerList)
                    {
                        if (c.ControlGroupID == groupID)
                            groupControllerList.Add(c);
                    }

                    // 需要验证Controller model 是否存在于Controller集合 on 20190321 by xyj
                    foreach (Controller cn in groupControllerList)
                    {
                        //显示警告信息：model doesn't exist component  , please remove it
                        CentralController controllerType = controllerTypeList.FirstOrDefault(p => p.Model == cn.Model);
                        if (controllerType == null)
                        {
                            thisProject.CentralControllerOK = false;
                            ShowWarningMsg(Msg.ERR_CONTROLLER_NOCONTROLLER(cn.Model));
                            break;
                        }
                    }

                    //校验是否有只分配了系统，未分配控制器的情况
                    // if (groupSystemList.Count > 0 && groupControllerList.Count == 0 || indoorList.Count > 0 && groupSystemList.Count ==0 && groupControllerList.Count == 0)
                    if (groupSystemList.Count > 0 && groupControllerList.Count == 0 && indoorList.Count == 0)
                    {
                        //TODO 显示警告信息：没有controller！
                        thisProject.CentralControllerOK = false;
                        ShowWarningMsg(Msg.WARNING_CONTROLLER_UNDONE("controller"));
                        break;
                    }
                    else if (groupControllerList.Count > 0 && groupSystemList.Count == 0 && indoorList.Count == 0) //校验是否有只分配了控制器，未分配系统的情况
                    {
                        //TODO 显示警告信息：没有system。
                        thisProject.CentralControllerOK = false;
                        ShowWarningMsg(Msg.WARNING_CONTROLLER_UNDONE("system"));
                        break;
                    }



                    if (indoorList.Count > 0 && groupSystemList.Count == 0 && groupControllerList.Count == 0)
                    {

                        //  jccmbControllerProductType.SelectedValue = indoorList[0].IndoorItem.Series;
                        //  _productType = indoorList[0].IndoorItem.Series;
                        // BindToControl_Controller();
                        //CentralControllerBLL bll = new CentralControllerBLL(thisProject);
                        //DataTable dt = bll.GetModels(thisProject.SubRegionCode, thisProject.BrandCode, indoorList[0].IndoorItem.ProductType, indoorList[0].IndoorItem.Series);
                        //if (dt != null && dt.Rows.Count > 0)
                        //{
                        //     BindExchangerController(indoorList);
                        //}
                        //TODO 显示警告信息：控制器组内室外机系统不能为空。
                        thisProject.CentralControllerOK = false;
                        ShowWarningMsg(Msg.WARNING_CONTROLLER_UNDONE("system"));
                        break;
                    }
                    //else {
                    //    _productType = jccmbControllerProductType.SelectedValue.ToString();
                    //    //刷新控制器
                    //    BindToControl_Controller();
                    //}

                    //特殊逻辑判断 add by axj 2018-03-30
                    if (groupControllerList != null && groupControllerList.Count > 0  && (_mainRegion == "EU_E" || _mainRegion == "EU_W" || _mainRegion == "EU_S"))   //目前该逻辑只针对EU
                    {
                        bool isNeedChk = false;
                        groupSystemList.ForEach((p) => {
                            if (p.SelOutdoorType == "FSXNPE (Top discharge)" || p.SelOutdoorType == "FSXNSE (Top discharge)")
                            { isNeedChk = true; }
                        });
                        if (isNeedChk)
                        {
                            Dictionary<string, int> dicNum = new Dictionary<string, int>();
                            dicNum["CSNET MANAGER 2 T10"] = 0;
                            dicNum["CSNET MANAGER 2 T15"] = 0;
                            dicNum["HC-A64NET"] = 0;
                            dicNum["PSC-A160WEB1"] = 0;
                            foreach (Controller item in groupControllerList)
                            {
                                string model = item.Model;
                                switch (model)
                                {
                                    case "CSNET MANAGER 2 T10": dicNum["CSNET MANAGER 2 T10"] = item.Quantity; break;
                                    case "CSNET MANAGER 2 T15": dicNum["CSNET MANAGER 2 T15"] = item.Quantity; break;
                                    case "HC-A64NET": dicNum["HC-A64NET"] = item.Quantity; break;
                                    case "PSC-A160WEB1": dicNum["PSC-A160WEB1"] = item.Quantity; break;
                                    default: break;
                                }
                            }
                            if ((dicNum["CSNET MANAGER 2 T10"] + dicNum["CSNET MANAGER 2 T15"]) > 0)
                            {
                                if ((dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"]) <= 0)//< (dicNum["CSNET MANAGER LT"] + dicNum["CSNET MANAGER XT"]))
                                {
                                    thisProject.CentralControllerOK = false;
                                    //int num = dicNum["CSNET MANAGER LT"] + dicNum["CSNET MANAGER XT"] - (dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"]);
                                    ShowWarningMsg(Msg.ERR_CONTROLLER_CSNET_MANAGER_LT_XT());//" PSC-A160WEB1 or  HC-A64NET must be added.";
                                    break;
                                }
                            }
                            //if (dicNum["HC-A64NET"] > 0)
                            //{
                            //    if ((dicNum["CSNET MANAGER LT"] + dicNum["CSNET MANAGER XT"]) <= 0)//< dicNum["HC-A64NET"])
                            //    {
                            //        thisProject.CentralControllerOK = false;
                            //        int num = dicNum["HC-A64NET"] - (dicNum["CSNET MANAGER LT"] + dicNum["CSNET MANAGER XT"]);
                            //        ShowWarningMsg(Msg.ERR_CONTROLLER_HC_A64NET());//" CSNET MANAGER LT or CSNET MANAGER XT must be selected";
                            //        break;
                            //    }
                            //}

                            if (dicNum["HC-A64NET"] > 0 || dicNum["PSC-A160WEB1"] > 0)
                            {
                                if ((dicNum["CSNET MANAGER 2 T10"] + dicNum["CSNET MANAGER 2 T15"]) <= 0)//< dicNum["HC-A64NET"])
                                {
                                    thisProject.CentralControllerOK = false;
                                    //int num = dicNum["HC-A64NET"] - (dicNum["CSNET MANAGER LT"] + dicNum["CSNET MANAGER XT"]);
                                    ShowWarningMsg(Msg.ERR_CONTROLLER_HC_A64NET());//" CSNET MANAGER LT or CSNET MANAGER XT must be selected";
                                    break;
                                }
                                else if (groupSystemList.Count < dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"])   //系统数要大于等于A64 + A160 的总数   add on 20180621
                                {
                                    thisProject.CentralControllerOK = false;
                                    //int num = dicNum["HC-A64NET"] - (dicNum["CSNET MANAGER LT"] + dicNum["CSNET MANAGER XT"]);
                                    ShowWarningMsg(Msg.ERR_CONTROLLER_SYSTEM_NUM());//dicNum["HC-A64NET"] + dicNum["PSC-A160WEB1"]//" CSNET MANAGER LT or CSNET MANAGER XT must be selected";
                                    break;
                                }
                            }
                        }
                    }

                    if (groupControllerList != null && groupSysProductType.Count > 0)
                        //获取当前group控制器所支持的productType列表
                        groupControProductType = productTypeBll.GetControllerProductTypeData(thisProject.SubRegionCode, thisProject.BrandCode, groupControllerList);
                    else
                        continue;



                    //校验是否有分配了ProductType错误的控制器
                    if (groupControProductType != null)
                    {

                        foreach (SystemVRF sysItem in groupSystemList)
                        {
                            if (!groupControProductType.Contains(sysItem.OutdoorItem.ProductType))
                            {
                                thisProject.CentralControllerOK = false;
                                ShowWarningMsg(Msg.ERR_CONTROLLER_PRODUCTTYPE_COMPATIBILITY(sysItem.Name));
                                break;
                            }
                        }
                    }

                }
            }
            SetTabControlImageKey();
            UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228

            return thisProject.CentralControllerOK;
        }

        //判断是否存在ControllerGroup
        private bool ExistControllerGroup(string GroupId)
        {
            foreach (ControlGroup c in thisProject.ControlGroupList)
            {
                if (c.Id == GroupId)
                {
                    return true;
                }
            }
            return false;
        }

        private void jccmbControllerProductType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //改变Central Controller的ProductType选项
            _productType = jccmbControllerProductType.SelectedValue.ToString();
            //刷新控制器
            BindToControl_Controller();
        }

        //初始productType列表
        private void BindControllerProductType()
        {
            //if (string.IsNullOrEmpty(_productType))
            //{
            _productType = "";
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            Trans trans = new Trans();
            //DataTable dt = productTypeBll.GetProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
            DataTable dt = productTypeBll.GetUsedProductTypeData(thisProject);
            if (dt != null && dt.Rows.Count > 0)
            {
                string colName = "Series";
                //翻译               
                dt = trans.getTypeTransDt(TransType.Series.ToString(), dt, colName);
                jccmbControllerProductType.ValueMember = "ProductType";
                //jccmbControllerProductType.DisplayMember = "Series";
                this.jccmbControllerProductType.DisplayMember = "Trans_Name";
                jccmbControllerProductType.DataSource = dt;
                jccmbControllerProductType.SelectedValue = thisProject.ProductType;
                this.jccmbControllerProductType.SelectedIndex = 0;
                _productType = jccmbControllerProductType.SelectedValue.ToString();
            }

            //}
        }

        public bool ResetPiping()
        {
            TreeNode tnNode = this.tvOutdoor.SelectedNode;
            if (tnNode == null)
                return false;

            TreeNode tnOut = Global.GetTopParentNode(tnNode);
            //curSystemItem = tnOut.Tag as SystemVRF;

            if (tnOut == null || (tnOut.Tag as SystemVRF).OutdoorItem == null)
                return false;

            ShowWarningMsg("");

            curSystemItem.IsManualPiping = false;
            utilPiping.ResetColors();

            ResetSystemHighDiff(curSystemItem);     //清除系统高度差 on 20180620 by xyj
            ResetRoomIndoorHighDiff(curSystemItem); //清除室内机的高度差 on 20180620 by xyj

            PipingBLL pipBll = GetPipingBLLInstance();
            pipBll.CreatePipingNodeStructure(curSystemItem);
            curSystemItem.IsUpdated = false;

            //重置也需重置状态
            SetSystemPipingOK(curSystemItem, false);

            DoDrawingPiping(true);
            return true;
        }
        /// <summary>
        /// 清除室内机的高度差
        /// </summary>
        /// <param name="sysItem"></param>
        private void ResetRoomIndoorHighDiff(SystemVRF sysItem)
        {
            List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(sysItem.Id);
            if (listRISelected.Count > 0)
            {
                foreach (RoomIndoor ri in listRISelected)
                {
                    ri.PositionType = PipingPositionType.SameLevel.ToString();
                    ri.HeightDiff = 0;
                }
            }
        }

        /// <summary>
        /// 清除系统高度差
        /// </summary>
        /// <param name="system"></param>
        private void ResetSystemHighDiff(SystemVRF system)
        {
            system.MaxCHBoxHighDiffLength = 0.0;
            system.MaxCHBox_IndoorHighDiffLength = 0.0; 
            system.MaxIndoorHeightDifferenceLength = 0.0;
            system.MaxLowerHeightDifferenceLength = 0.0;
            system.MaxSameCHBoxHighDiffLength = 0.0;
            system.MaxUpperHeightDifferenceLength = 0.0;
        }

        //点击管长设置按钮事件
        private void jcbtnPipingLengths_Click(object sender, EventArgs e)
        {
            if (curSystemItem == null)
                return;
            //获取当前选择的室内机集合
            List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(curSystemItem.Id);

            frmPipingLengthSetting frmPipLength = new frmPipingLengthSetting(curSystemItem, listRISelected, thisProject);
            if (frmPipLength.ShowDialog() == DialogResult.OK)
            {
                SetSystemPipingOK(curSystemItem, false);

                PipingBLL pipBll = GetPipingBLLInstance();
                PipingErrors errorType = pipBll.ValidateSystemHighDifference(curSystemItem);
                if (errorType == PipingErrors.OK)
                {
                    if (curSystemItem.IsManualPiping)
                    {
                        DoDrawingPiping(false);
                    }
                    else
                    {
                        DoDrawingPiping(true);
                    }
                    ResetScrollPosition();
                    errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
                }
                ShowWarningMsg(errorType);
                SetTreeViewOutdoorState();
            }
        }

        private DialogResult ManualPipingCleanConfirmation(bool needValidate = true)
        {
            if (curSystemItem.MyPipingNodeOut != null && curSystemItem.IsManualPiping)
            {
                PipingBLL pipBll = GetPipingBLLInstance();
                if (needValidate)
                {
                    //在主界面改动Manual Piping之前需要先保证Manual Piping的完整性,以避免转换成Auto Piping之后发生问题。 add by Shen Junjie on 2019/05/17
                    PipingErrors errorType = pipBll.ValidateManualPiping(curSystemItem);
                    if (errorType != PipingErrors.OK)
                    {
                        ShowWarningMsg(errorType);
                        return DialogResult.No;
                    }
                }
                return JCMsg.ShowConfirmYesNoCancel(Msg.GetResourceString("WARNING_MANUAL_PIPING_LOST"), true);
            }
            return DialogResult.Yes;
        }

        private void jccmbPipingLayoutType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (curSystemItem == null) return;
            PipingLayoutTypes layoutType = GetPipingLayoutTypeByIndex(jccmbPipingLayoutType.SelectedIndex);
            if (layoutType == curSystemItem.PipingLayoutType) return;

            if (JCMsg.ShowConfirmOKCancel(Msg.GetResourceString("PIPING_LAYOUT_CHANGE_CONFIRM"), true) != DialogResult.OK)
            {
                jccmbPipingLayoutType.SelectedIndex = GetPipingLayoutTypeIndex(curSystemItem.PipingLayoutType);
                return;
            }

            jcbtnVarification.Focus();
            curSystemItem.PipingLayoutType = layoutType;
            //重置配管图
            ResetPiping();
            //保存历史痕迹
            UndoRedoUtil.SaveProjectHistory();
        }

        private PipingLayoutTypes GetPipingLayoutTypeByIndex(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 1:
                    return PipingLayoutTypes.BinaryTree;
                case 2:
                    return PipingLayoutTypes.SchemaA;
                case 0:
                default:
                    return PipingLayoutTypes.Normal;
            }
        }

        private int GetPipingLayoutTypeIndex(PipingLayoutTypes pipingLayoutType)
        {
            switch (pipingLayoutType)
            {
                case PipingLayoutTypes.BinaryTree:
                    return 1;
                case PipingLayoutTypes.SchemaA:
                    return 2;
                case PipingLayoutTypes.Normal:
                default:
                    return 0;
            }
        }

        //private void uc_CheckBox_Altitude_CheckedChanged(object sender, EventArgs e)
        //{
        //    uc_CheckBox cbx = sender as uc_CheckBox;
        //    //海拔高度的变化会影响到选型的合理性，需要重新选型
        //    if (JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGEALTITUDE()) == DialogResult.Cancel)
        //    {
        //        cbx.Checked = !cbx.Checked;
        //    }
        //    else
        //    {
        //        thisProject = new Project();
        //        thisProject.Name = this.jctxtProjName.Text;
        //        thisProject.Altitude = int.Parse(this.jctxtAltitude.Text);
        //        thisProject.ContractNO = this.jctxtContractNo.Text;
        //        thisProject.Location = this.jctxtLocation.Text;
        //        thisProject.PurchaseOrderNO = this.jctxtPurchaseOrderNo.Text;
        //        thisProject.Remarks = this.jctxtRemarks.Text;
        //        thisProject.SalesEngineer = this.jctxtSalesName.Text;
        //        thisProject.SalesOffice = this.jctxtSalesOffice.Text;
        //        thisProject.SalesYINO = this.jctxtSalesYINo.Text;
        //        thisProject.ShipTo = this.jctxtShipTo.Text;
        //        thisProject.SoldTo = this.jctxtSoldTo.Text;
        //        thisProject.DeliveryRequiredDate = this.timeDeliveryDate.Value;
        //        thisProject.OrderDate = this.timeOrderDate.Value;
        //        string productType = this.jccmbProductType.SelectedValue.ToString();

        //        BindToControl();

        //        if (!cbx.Checked)
        //        {
        //            //在使用高度修正时，已设置的高度不可修改
        //            this.jctxtAltitude.Enabled = false;
        //            thisProject.EnableAltitudeCorrectionFactor = false;
        //        }
        //        else
        //        {
        //            this.jctxtAltitude.Enabled = true;
        //            thisProject.EnableAltitudeCorrectionFactor = true;
        //        }
        //    }
        //}

        public void ExchangeIndoors(SystemVRF sysItem, Node node1, Node node2)
        {
            //MyLink inlink1, inlink2;
            if (node1 is MyNodeIn && node2 is MyNodeIn)
            {
                MyNodeIn nodeIn1 = node1 as MyNodeIn;
                MyNodeIn nodeIn2 = node2 as MyNodeIn;

                PipingBLL pipBll = GetPipingBLLInstance();
                pipBll.SwapNodeIn(nodeIn1, nodeIn2);
            }

            //同步TreeView中室内机的排序
            DoUpdateTreeNodeOrder(sysItem);
            BindTreeNodeOutdoor(sysItem.Id);

            if (sysItem.IsManualPiping)
            {
                DoDrawingPiping(false);
            }
            else
            {
                DoDrawingPiping(true);
            }
            ResetScrollPosition();
        }

        private void SetSystemPipingOK(SystemVRF sysItem, bool isPipingOK)
        {
            if (sysItem.IsPipingOK != isPipingOK)
            {
                sysItem.IsPipingOK = isPipingOK;
                SetTabControlImageKey();
            }
        }

        private void exchangeIndoorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (curSystemItem == null || curSystemItem.MyPipingNodeOut == null || selNode == null || selNode2 == null) return;

            ExchangeIndoors(curSystemItem, selNode, selNode2);
        }

        #region 室内机在系统间互相拖动  add by axj 20170119
        void tvOutdoor_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        /// 拖拽可行性检查
        /// <summary>
        /// 拖拽可行性检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvOutdoor_DragOver(object sender, DragEventArgs e)
        {
            //resetTreeNodeColor(tvOutdoor.Nodes[0]);

            // 获取目标节点
            TreeView tv = sender as TreeView;
            Point pt = tv.PointToClient(new Point(e.X, e.Y));
            TreeNode dstNode = tv.GetNodeAt(pt);
            if (dstNode != null && dstNode.Level == 0)
            {
                var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
                if (dstNode.Nodes.Contains(node))
                {
                    e.Effect = DragDropEffects.None;
                }
                else
                {
                    //dstNode.ForeColor = Color.Red;
                    e.Effect = DragDropEffects.Move;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// TreeView -> TreeView
        /// <summary>
        /// TreeView -> TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvOutdoor_DragDrop(object sender, DragEventArgs e)
        {
            SystemVRF selectOutdoor = null;
            SystemVRF targetOutdoor = null;
            TreeView tv = sender as TreeView;
            Point pt = tv.PointToClient(new Point(e.X, e.Y));
            TreeNode dstNode = tv.GetNodeAt(pt);
            TreeNode node = null;
            RoomIndoor ri = null;
            if (dstNode != null && dstNode.Level == 0)
            {
                node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
                if (node.Parent == null)   //室外机拖动父节点为空，拖动失效
                    return;
                selectOutdoor = node.Parent.Tag as SystemVRF;
                targetOutdoor = dstNode.Tag as SystemVRF;
                ri = node.Tag as RoomIndoor;
            }

            if (selectOutdoor.OutdoorItem == null && node.Parent.Nodes.Count == 1)
            {

                DoDeleteOutdoor(selectOutdoor.Id);
                BindToControl_SelectedUnits();
                return;

            }
            else
            {
                if (targetOutdoor.OutdoorItem != null && selectOutdoor.OutdoorItem != null)
                {
                    //product type 不同,不能移动
                    if (selectOutdoor.OutdoorItem.Series != targetOutdoor.OutdoorItem.Series)
                    {
                        //TODO 提示？
                        var alertMsg = Msg.OUTD_CANNOT_MOVE_Series; //"室外机系统的产品类型不一样，不能移动！"
                        JCMsg.ShowWarningOK(alertMsg);
                        return;
                    }

                }

            }

            //判断system type
            //OnlyIndoor, OnlyFreshAir, CompositeMode, OnlyFreshAirMulti
            if (selectOutdoor.SysType == SystemType.CompositeMode)
            {
                //TODO 提示？
                var alertMsg = Msg.OUTD_CANNOT_MOVE_CompositeMode(selectOutdoor.Name);// selectOutdoor.Name); //"室外机系统{0}的类型是混连模式，不能移动！"
                JCMsg.ShowWarningOK(alertMsg);
                return;
            }
            if (targetOutdoor.SysType == SystemType.CompositeMode)
            {
                //TODO 提示？
                var alertMsg = Msg.OUTD_CANNOT_MOVE_CompositeMode(targetOutdoor.Name);// targetOutdoor.Name);// "室外机系统{0}的类型是混连模式，不能移动！"
                JCMsg.ShowWarningOK(alertMsg);
                return;
            }
            if (selectOutdoor.SysType == 0 && targetOutdoor.SysType != 0)
            {
                //TODO 提示？
                var alertMsg = Msg.OUTD_CANNOT_MOVE_IndoorToFresh;//"室内机不能移动到新风机系统！"
                JCMsg.ShowWarningOK(alertMsg);
                return;
            }
            if (selectOutdoor.SysType != 0 && targetOutdoor.SysType == 0)
            {
                //TODO 提示？
                var alertMsg = Msg.OUTD_CANNOT_MOVE_FreshToIndoor;//"新风机不能移动到室内机系统！"
                JCMsg.ShowWarningOK(alertMsg);
                return;
            }
            var indoorlist = thisProject.RoomIndoorList.FindAll(p => p.SystemID == ri.SystemID);
            if (ri.RoomID != null && ri.RoomID != "")
            {
                //var selectNodes = _.where(treeNodes, { 'RoomID': treeNodes[i].RoomID });
                var roomIndlist = thisProject.RoomIndoorList.FindAll(p => p.RoomID == ri.RoomID);
                string alertMsg = "";

                //system下室内机全部移除掉
                if (roomIndlist.Count > 1 && roomIndlist.Count == indoorlist.Count)
                {
                    //TODO 提示
                    alertMsg = Msg.OUTD_TOGETHER_MOVE_DELETE_SYSTEM(selectOutdoor.Name);//选中的室内机所在房间存在多个室内机将会被一起移动，室外机系统{0}移动后将被删除，确认是否继续操作？？
                }
                else if (roomIndlist.Count > 1)
                {
                    alertMsg = Msg.OUTD_TOGETHER_MOVE_BYROOM;//");// "选中的室内机所在房间存在多个室内机将会被一起移动，确认是否继续操作？";
                }
                else if (roomIndlist.Count == indoorlist.Count)
                {
                    //TODO 提示
                    alertMsg = Msg.OUTD_CONFIRM_DELETE_SYSTEM(selectOutdoor.Name);//室外机系统{0}移动后将被删除，确认是否移动？
                }
                else
                {
                    ChangeRoomIndoorList(roomIndlist, selectOutdoor, targetOutdoor);
                    return; //这里必须有
                }
                DialogResult res = JCMsg.ShowConfirmOKCancel(alertMsg);
                if (res == DialogResult.OK)
                {
                    ChangeRoomIndoorList(roomIndlist, selectOutdoor, targetOutdoor);
                }
                return;
            }

            if (ri.IndoorItemGroup != null)
            {
                if (!deleteShareRelationShip(ri, true))
                    return;
            }


            List<RoomIndoor> list = new List<RoomIndoor>();
            list.Add(ri);
            if (indoorlist.Count == 1)
            {

                var alertMsg = Msg.OUTD_CONFIRM_DELETE_SYSTEM(selectOutdoor.Name);//室外机系统{0}移动后将被删除，确认是否移动？
                DialogResult res = JCMsg.ShowConfirmOKCancel(alertMsg);
                if (res == DialogResult.OK)
                {
                    ChangeRoomIndoorList(list, selectOutdoor, targetOutdoor);
                }
                return;
            }
            ChangeRoomIndoorList(list, selectOutdoor, targetOutdoor);

        }
        /// <summary>
        /// 修改室内机列表
        /// </summary>
        /// <param name="riList"></param>
        /// <param name="selectOutdoor"></param>
        /// <param name="targetOutdoor"></param>
        private void ChangeRoomIndoorList(List<RoomIndoor> riList, SystemVRF selectOutdoor, SystemVRF targetOutdoor)
        {
            if (riList[0].IsFreshAirArea)
            {
                MoveRoomIndoor(riList, selectOutdoor, targetOutdoor);
                return;
            }
            var flag = CheckIndoorReCalculate(selectOutdoor, targetOutdoor);
            //工况不一致
            if (flag)
            {
                double outCool = 0;
                double outHeat = 0;
                //List<RoomIndoor> moveRoomIndoorList = new List<RoomIndoor>();
                var seriesName = targetOutdoor.OutdoorItem.Series;
                if (seriesName.Contains("Water Source"))
                {
                    outCool = targetOutdoor.IWCooling;
                    outHeat = targetOutdoor.IWHeating;
                }
                else
                {
                    outCool = targetOutdoor.DBCooling;
                    outHeat = targetOutdoor.WBHeating;
                }
                if (riList.Count < 1)
                    return;
                frmMatchIndoorResult fm = new frmMatchIndoorResult(riList, thisProject, outCool, outHeat);
                fm.StartPosition = FormStartPosition.CenterScreen;
                if (fm.ShowDialog() == DialogResult.OK)
                {
                    if (fm.ReselectionResultList != null)
                    {
                        foreach (var ent in fm.ReselectionResultList)
                        {
                            if (ent.Seccessful == false)
                            {
                                var ri = riList.Find(p => p.IndoorNO.ToString() == ent.IndoorNo);
                                riList.Remove(ri);
                            }
                            else
                            {
                                var ri_new = fm.ReselectedIndoorList.Find(p => p.IndoorNO.ToString() == ent.IndoorNo);
                                var index = thisProject.RoomIndoorList.FindIndex(p => p.IndoorNO.ToString() == ent.IndoorNo);
                                thisProject.RoomIndoorList[index] = ri_new;
                            }
                        }
                        if (riList.Count == 0)
                        {
                            return;
                        }
                        MoveRoomIndoor(riList, selectOutdoor, targetOutdoor);
                    }
                }
            }
            else
            {
                MoveRoomIndoor(riList, selectOutdoor, targetOutdoor);
            }
        }
        /// <summary>
        /// 移动室内机
        /// </summary>
        /// <param name="riList"></param>
        /// <param name="selectOutdoor"></param>
        /// <param name="targetOutdoor"></param>
        private void MoveRoomIndoor(List<RoomIndoor> riList, SystemVRF selectOutdoor, SystemVRF targetOutdoor)
        {
            foreach (var ent in riList)
            {
                var ri = thisProject.RoomIndoorList.Find(p => p.IndoorNO == ent.IndoorNO);
                thisProject.RoomIndoorList.Remove(ri);
                thisProject.RoomIndoorList.Add(ri);
                ri.SystemID = targetOutdoor.Id;
            }
            foreach (TreeNode tnOut in tvOutdoor.Nodes)
            {
                // TODO: 室外机重新选型，注意可能会有选型失败的情况出现 20160819 by Yunxiao Lin
                SystemVRF sysItem = tnOut.Tag as SystemVRF;
                if (!sysItem.Name.Equals(selectOutdoor.Name) && !sysItem.Name.Equals(targetOutdoor.Name))
                    continue;
                List<string> ERRList = new List<string>();
                List<string> MSGList = new List<string>();
                List<RoomIndoor> listRISelected = (new ProjectBLL(thisProject)).GetSelectedIndoorBySystem(sysItem.Id);
                if (listRISelected.Count > 0)
                {
                    //是否自动模式的判断  add by Shen Junjie on 20170619
                    if (sysItem != null && sysItem.IsAuto)
                    {
                        //Global.DoSelectOutdoor(sysItem, listRISelected, thisProject, out ERRList);
                        //室外机选型统一改用新逻辑 Global.DoSelectOutdoorODUFirst 20161112 by Yunxiao Lin
                        SelectOutdoorResult result = Global.DoSelectOutdoorODUFirst(sysItem, listRISelected, thisProject, out ERRList, out MSGList);
                    }
                    Global.BindTreeNodeOut(tnOut, sysItem, listRISelected, thisProject);
                    sysItem.IsUpdated = true;
                    //重新选型之后需要判断并设置TabPage状态 20160819 by Yunxiao Lin
                }
                else
                {
                    thisProject.SystemList.Remove(sysItem);
                    tnOut.Remove();
                }
            }
            SetTabControlImageKey();
            UndoRedoUtil.SaveProjectHistory();
        }

        /// <summary>
        /// 判断2个室外机系统的工况是否一致
        /// </summary>
        /// <param name="selectOutdoor"></param>
        /// <param name="targetOutdoor"></param>
        /// <returns></returns>
        private bool CheckIndoorReCalculate(SystemVRF selectOutdoor, SystemVRF targetOutdoor)
        {
            var seriesName = selectOutdoor.Series;
            if (seriesName.Contains("Water Source"))
            {
                if (selectOutdoor.IWCooling != targetOutdoor.IWCooling)
                {
                    return true;
                }
                if (selectOutdoor.IWHeating != targetOutdoor.IWHeating)
                {
                    return true;
                }
            }
            else
            {
                if (selectOutdoor.DBCooling != targetOutdoor.DBCooling)
                {
                    return true;
                }
                if (selectOutdoor.WBHeating != targetOutdoor.WBHeating)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        private void jcbtnOutputPdf_Click(object sender, EventArgs e)
        {
            DoSaveProject();

            if (!CheckRptPath()) return;
            string dir = this.jclblRptFilePath.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // 遍历每个系统图，并导出对应的 DXF 文件 
                foreach (TreeNode tnOut in this.tvOutdoor.Nodes)
                {
                    // 1、绘制系统对应的 Piping 图（DoDrawing）
                    if (tnOut.Tag != null && tnOut.Tag is SystemVRF)
                    {
                        SystemVRF sysItem = tnOut.Tag as SystemVRF;
                        curSystemItem = sysItem;

                        BindPipingDrawing(tnOut);
                        //string imgDir = MyConfig.WiringNodeImageDirectory;
                        DoDrawingWiring(tnOut);

                        DoSavePipingFilePicture(sysItem.NO.ToString());
                        DoSaveWiringFilePicture(sysItem.NO.ToString());
                    }
                } 
                // 将Controller布局图保存为图片
                BindToControl_Controller_Report();
                DoSaveControllerPicture();
                //GetControllerPicture();
                //ANZ区域可以使用ControllerWiring add axj 20180412
                if (thisProject.RegionCode.StartsWith("EU_") || thisProject.SubRegionCode == "ANZ" || thisProject.RegionCode.StartsWith("ME_A") || thisProject.RegionCode.StartsWith("LA") || thisProject.RegionCode.StartsWith("ASEAN") || thisProject.RegionCode.StartsWith("INDIA"))
                {
                    //保存Controller Wiring图
                    ControllerWiringBLL cwBLL = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
                    foreach (ControlGroup group in thisProject.ControlGroupList)
                    {
                        if (cwBLL.DrawWiring(group))
                        {
                            DoSaveControllerWiringFilePicture(group.Id, group);
                        }
                    }
                }
                string rptFile = System.IO.Path.Combine(dir, thisProject.Name + ".pdf");
                // 将数据写入指定的模板，然后另存至上方指定路径
                if (ProjectBLL.IsSupportedNewReport(thisProject))
                {
                    NewReport rpt = new NewReport(thisProject);
                    rpt.isActual = uc_CheckBox_Actual.Checked;
                    // changed for york model on 20 Nov 2018 
                    if (thisProject.BrandCode == "Y")
                    {
                        if (!rpt.ExportReportPDF(MyConfig.ReportTemplateDirectory + "\\NewReport\\NewReportYork.doc", rptFile))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!rpt.ExportReportPDF(MyConfig.ReportTemplateDirectory + "\\NewReport\\NewReport.doc", rptFile))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    ReportForAspose rpt = new ReportForAspose(thisProject);
                    if (!rpt.ExportReportPDF(MyConfig.ReportTemplateNamePath, rptFile))
                        return;
                }
                //TreeNode tnOut1 = Global.GetTopParentNode(this.tvOutdoor.SelectedNode);
                //BindPipingDrawing(tnOut1.Tag as SystemVRF, tnOut1); // 恢复当前选中节点绑定的 Piping 图
                //DoDrawingWiring(tnOut1, wiringImgDir);
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // 室内机node改变颜色   --add on 20170516 by Lingjia Qiu
        /// <summary>
        /// 室内机node改变颜色
        /// </summary>
        /// <param name="riItem"></param>
        /// <param name="node"></param>
        private void ChangeIndoorNodesColor()
        {
            if (tvOutdoor.Nodes.Count > 0)
            {
                RoomIndoor riItem;
                foreach (TreeNode childNode in tvOutdoor.Nodes)
                {
                    foreach (TreeNode nodeIn in childNode.Nodes)
                    {
                        if (nodeIn.Level == 1 || nodeIn.Level == 2)
                        {
                            if (nodeIn.Tag is RoomIndoor)
                            {
                                riItem = nodeIn.Tag as RoomIndoor;
                                //需求容量大于实际容量，改变字体颜色
                                if (riItem != null)
                                {
                                    if (!CommonBLL.FullMeetRoomRequired(riItem, thisProject))
                                        nodeIn.ForeColor = Color.Chocolate;
                                }

                            }
                        }
                    }

                }
            }

        }

        private void dgvSystem_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DrawPainTing(sender, e);
        }


        // 重新绘制dgv的表头，背景色、边框等   --add on 20170610 by Lingjia Qiu
        /// <summary>
        /// 重新绘制dgv的表头，背景色、边框等
        /// </summary>
        /// <param name="sender"></param>
        private void DrawPainTing(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = UtilColor.bg_dgvHeader_Indoor;
            Pen pen_dgvBorder = new Pen(UtilColor.border_dgvHeader, 0.1f);

            DataGridView dgv = sender as DataGridView;

            if (e.RowIndex == -1) //标题行
            {
                //bool mouseOver = e.CellBounds.Contains(dgv.PointToClient(Cursor.Position));
                //System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                //    e.CellBounds,
                //    //mouseOver ? Color.PeachPuff : Color.LightGray,
                //    //Color.DarkOrange,
                //    System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                SolidBrush brush = new SolidBrush(color_bg_dgvHeader);

                using (brush)
                {
                    e.Graphics.FillRectangle(brush, e.CellBounds);
                    Rectangle border = e.CellBounds;
                    border.X -= 1;
                    //border.Y -= 1;
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
                return;
            }

        }


        /// 绑定系统信息表   --add on 20170610 by Lingjia Qiu
        /// <summary>
        /// 绑定系统信息表
        /// </summary>
        /// <param name="riItem"></param>
        private void BindSysTableInfo(RoomIndoor r)
        {
            if (thisProject.SystemList.Count == 0)
            {
                this.dgvSystemInfo.Visible = false;
                return;
            }

            dgvSystemInfo.Rows.Clear();  //清空数据                                    
            ProjectBLL bll = new ProjectBLL(thisProject);
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.OutdoorItem == null)
                {
                    this.dgvSystemInfo.Visible = false;
                    return;
                }
                if (sysItem.Id.Equals(r.SystemID))
                {
                    this.jclblUnitInfo.Text = sysItem.Name + ShowText.SystemInfo;
                    double totestIndCap_h = 0;
                    double totestIndCap_c = getTotestIndCap_c(sysItem, bll, out totestIndCap_h);
                    string coolingReqd = "";
                    if (totestIndCap_c != 0d)
                        coolingReqd = Unit.ConvertToControl(totestIndCap_c, UnitType.POWER, ut_power).ToString("n1");
                    else
                        coolingReqd = "-";

                    double sysRH = bll.CalculateRH(sysItem.DBHeating, sysItem.WBHeating, thisProject.Altitude);
                    //先添加一条系统室外机记录
                    this.dgvSystemInfo.Rows.Add(
                                                  "",
                                                  coolingReqd,   // cooling Req'd
                                                  totestIndCap_h != 0d ? Unit.ConvertToControl(totestIndCap_h, UnitType.POWER, ut_power).ToString("n1") : "-",   //Heating Req'd
                                                  "-",   //Sensible Req'd
                                                  sysItem.Name,   //系统名字
                                                  sysItem.OutdoorItem.AuxModelName,   //室外机型号
                                                  Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),   //Cooling Actual
                                                  Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),    //Heating Actual
                                                  "-",
                                                  Unit.ConvertToControl(sysItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //CoolingDB
                                                  "-",   //CoolingWB
                                                  Unit.ConvertToControl(sysItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //HeatingDB
                                                  Unit.ConvertToControl(sysItem.WBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //HeatingWB
                                                 (sysRH * 100).ToString("n0")  //RH
                                                );


                    //添加系统室内机记录
                    List<RoomIndoor> riItemList = bll.GetSelectedIndoorBySystem(sysItem.Id);
                    int colorIndex = 0;
                    string RoomId = "";
                    string SensibleHeat = "";
                    foreach (RoomIndoor riItem in riItemList)
                    {


                        double riRH = (new ProjectBLL(thisProject)).CalculateRH(riItem.DBCooling, riItem.WBCooling, thisProject.Altitude);
                        //添加不基于房间的室内机riItem.IndoorName 改为 DisplayRoom
                        string DisplayRoom = string.IsNullOrEmpty(riItem.DisplayRoom) ? riItem.IndoorName : riItem.DisplayRoom + ":" + riItem.IndoorName;
                        string RqCoolingCapacity = riItem.RqCoolingCapacity != 0d ? Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") : "-";
                        string RqHeatingCapacity = riItem.RqHeatingCapacity != 0d ? Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") : "-";
                        string RqSensibleHeat = riItem.RqSensibleHeat != 0d ? Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") : "-";
                        string RoomName = "-";
                        if (!string.IsNullOrEmpty(riItem.RoomName) && !string.IsNullOrEmpty(riItem.RoomID))
                        {
                            if (RoomId != riItem.RoomID)
                            {
                                RoomName = riItem.RoomName;  
                                if (!riItem.IsFreshAirArea)
                                {
                                    Room ri = bll.GetRoom(riItem.RoomID);
                                    RqCoolingCapacity = ri.RqCapacityCool != 0d ? Unit.ConvertToControl(ri.RqCapacityCool, UnitType.POWER, ut_power).ToString("n1") : " ";
                                    RqHeatingCapacity = ri.RqCapacityHeat != 0d ? Unit.ConvertToControl(ri.RqCapacityHeat, UnitType.POWER, ut_power).ToString("n1") : " ";
                                    RqSensibleHeat = ri.SensibleHeat != 0d ? Unit.ConvertToControl(ri.SensibleHeat, UnitType.POWER, ut_power).ToString("n1") : "-";
                                    RoomId = riItem.RoomID;
                                    SensibleHeat = RqSensibleHeat;
                                }
                                else
                                {
                                    FreshAirArea air = bll.GetFreshAirArea(riItem.RoomID);
                                    RqCoolingCapacity = air.RqCapacityCool != 0d ? Unit.ConvertToControl(air.RqCapacityCool, UnitType.POWER, ut_power).ToString("n1") : " ";
                                    RqHeatingCapacity = air.RqCapacityHeat != 0d ? Unit.ConvertToControl(air.RqCapacityHeat, UnitType.POWER, ut_power).ToString("n1") : " ";
                                    RqSensibleHeat = air.SensibleHeat != 0d ? Unit.ConvertToControl(air.SensibleHeat, UnitType.POWER, ut_power).ToString("n1") : "-";
                                    RoomId = riItem.RoomID;
                                    SensibleHeat = RqSensibleHeat;
                                } 
                            }
                            else
                            {
                                RoomName = "";
                                RqCoolingCapacity = "";
                                RqHeatingCapacity = "";
                                RqSensibleHeat = "";
                                //if (!string.IsNullOrEmpty(SensibleHeat) && SensibleHeat == "-")
                                //    RqSensibleHeat = "-";
                            }

                        }

                        this.dgvSystemInfo.Rows.Add(
                                                RoomName,
                            //riItem.IsAuto ? Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-",   // cooling Req'd
                                                RqCoolingCapacity,   // cooling Req'd
                                                RqHeatingCapacity,   //Heating Req'd
                                                RqSensibleHeat,
                            //riItem.IsAuto ? Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-",   //Sensible Req'd
                                                riItem.IndoorName,   //室内机名字
                                                thisProject.BrandCode == "Y" ? riItem.IndoorItem.Model_York : riItem.IndoorItem.Model_Hitachi,   //室内机型号
                                                Unit.ConvertToControl(riItem.ActualCoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),   //Cooling Actual  
                            //riItem.IsAuto ?Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-",   //Heating Req'd 
                                                Unit.ConvertToControl(riItem.ActualHeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),   //Heating Actual
                                                Unit.ConvertToControl(riItem.ActualSensibleHeat, UnitType.POWER, ut_power).ToString("n1"),   //Sensible Actual
                                                Unit.ConvertToControl(riItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //CoolingDB
                                                Unit.ConvertToControl(riItem.WBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //CoolingWB
                                                Unit.ConvertToControl(riItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //HeatingDB
                                                riItem.WBHeating != 0d ? Unit.ConvertToControl(riItem.WBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") : "-",   //HeatingWB
                                                (riRH * 100).ToString("n0")  //RH
                                                );
                        colorIndex++;
                        string wType;
                        if (!CommonBLL.MeetRoomRequired(riItem, thisProject, 0, thisProject.RoomIndoorList, out wType))
                        {
                            dgvSystemInfo.Rows[colorIndex].Cells[1].Style.ForeColor = Color.Chocolate;
                            switch (wType)
                            {
                                case "reqCool":
                                    dgvSystemInfo.Rows[colorIndex].Cells[4].Style.ForeColor = Color.Chocolate;
                                    dgvSystemInfo.Rows[colorIndex].Cells[5].Style.ForeColor = Color.Chocolate;
                                    break;
                                case "reqHeat":
                                    dgvSystemInfo.Rows[colorIndex].Cells[10].Style.ForeColor = Color.Chocolate;
                                    dgvSystemInfo.Rows[colorIndex].Cells[11].Style.ForeColor = Color.Chocolate;
                                    break;
                                case "sensible":
                                    dgvSystemInfo.Rows[colorIndex].Cells[6].Style.ForeColor = Color.Chocolate;
                                    dgvSystemInfo.Rows[colorIndex].Cells[7].Style.ForeColor = Color.Chocolate;
                                    break;

                            }

                        }


                    }

                    break;
                }

            }
            dgvSystemInfo.ClearSelection();   //取消默认选中
            this.dgvSystemInfo.Visible = true;
        }


        /// 获取制冷制热需求容量   --add on 20170610 by Lingjia Qiu
        /// <summary>
        /// 获取制冷制热需求容量
        /// </summary>
        /// <param name="riItem"></param>
        private double getTotestIndCap_c(SystemVRF sysItem, ProjectBLL bll, out double totestIndCap_h)
        {
            List<RoomIndoor> listRI = bll.GetSelectedIndoorBySystem(sysItem.Id);
            double totestIndCap_c = bll.CalIndoorEstCapacitySum(listRI, out totestIndCap_h);
            return totestIndCap_c;
        }

        /// <summary>
        /// 打开手动配管界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jcbtnManualPiping_Click(object sender, EventArgs e)
        {
            if (curSystemItem == null || curSystemItem.OutdoorItem == null) return;

            PipingBLL pipBll = GetPipingBLLInstance();
            pipBll.SavePipingStructure(curSystemItem); //保存当前结构，后面可以复制结构，和回滚修改。

            frmManualPiping frm = new frmManualPiping(thisProject, curSystemItem);
            //frm.WindowState = this.WindowState;
            //frm.Location = this.Location;
            //frm.Size = this.Size;
            frm.DiagramScale = jccmbScale.Text;
            frm.WindowState = FormWindowState.Maximized;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //确定修改以后，替换thisProject和curSystemItem对象
                thisProject = frm.thisProject;
                curSystemItem = frm.curSystemItem;

                //如果进Manual piping以前curSystemItem被修改过，那么manual piping完成以后就和curSystemItem一致了,IsUpdated应该为false。 add by Shen Junjie on 2018/4/13
                curSystemItem.IsUpdated = false;
                ShowWarningMsg("");

                jccmbScale.Text = frm.DiagramScale;

                utilPiping.colorDefault = curSystemItem.MyPipingNodeOutTemp.PipeColor;
                utilPiping.colorText = curSystemItem.MyPipingNodeOutTemp.TextColor;
                utilPiping.colorYP = curSystemItem.MyPipingNodeOutTemp.BranchKitColor;
                utilPiping.colorNodeBg = curSystemItem.MyPipingNodeOutTemp.NodeBgColor;

                BindTreeViewOutdoor(); //thisProject和curSystemItem被替换以后，重新绑定控件。 add by Shen Junjie on 2019/05/29 

                //同步TreeView中室内机的排序
                DoUpdateTreeNodeOrder(curSystemItem);

                DoDrawingPiping(false);
                ResetScrollPosition();
                PipingErrors errorType = PipingErrors.OK;
                if (errorType == PipingErrors.OK)
                {
                    errorType = pipBll.ValidateManualPiping(curSystemItem);
                }
                if (errorType == PipingErrors.OK)
                {
                    pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
                }
                if (errorType == PipingErrors.OK)
                {
                    BindTreeNodeOutdoor(curSystemItem.Id);

                    if (curSystemItem.OutdoorItem != null)
                    {
                        curSystemItem.MyPipingNodeOut.Model = curSystemItem.OutdoorItem.AuxModelName;
                        curSystemItem.MyPipingNodeOut.Name = curSystemItem.Name;
                    }

                    //根据MyPipingNodeOutTemp保存的坐标还原各节点和连接线，不重算坐标
                    DoDrawingPiping(false);
                    ResetScrollPosition();
                }
                ShowWarningMsg(errorType);

                SetTreeViewOutdoorState();
                SetTabControlImageKey();

                UndoRedoUtil.SaveProjectHistory();
            }
        }

        /// <summary>
        /// 共享控制器主室内机优先排序   --add on 20170620 by Lingjia Qiu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private List<WiringNodeIn> getSortNodeInList(Object objList)
        {
            //MyNodeOut_Wiring nodeOut
            List<WiringNodeIn> myNodeList = new List<WiringNodeIn>();
            List<WiringNodeIn> sortNodeInList = new List<WiringNodeIn>();
            List<WiringNodeIn> mainNodeInList = new List<WiringNodeIn>();
            if (objList is WiringNodeOut)
            {
                WiringNodeOut nodeOut = objList as WiringNodeOut;
                foreach (Node node in nodeOut.ChildNodes)
                {
                    if (node is WiringNodeIn)
                    {
                        WiringNodeIn nodeIn = node as WiringNodeIn;
                        myNodeList.Add(nodeIn);
                    }
                }
            }
            else if (objList is List<WiringNodeIn>)
            {
                myNodeList = objList as List<WiringNodeIn>;
            }
            else
            {
                return sortNodeInList;
            }
            //int nodeCout = 0; //delete by Shen Junjie on 2017/12/27 不需要此变量
            foreach (WiringNodeIn nodeIn in myNodeList)
            {
                if (nodeIn.RoomIndoorItem.IsMainIndoor)
                    mainNodeInList.Add(nodeIn);
                //nodeCout++;
            }
            if (mainNodeInList.Count == 0)
            {
                //非共享控制器节点
                //sortNodeInList = new List<MyNodeIn>();   //delete by Shen Junjie on 2017/12/27 上面已经赋值
                for (int i = 0; i < myNodeList.Count; i++)
                {
                    sortNodeInList.Add(myNodeList[i]);
                }
            }
            else
            {
                //共享控制器节点主室内机节点优先于共享关系组其他成员排序
                foreach (WiringNodeIn mianNodeIn in mainNodeInList)
                {
                    ////主室内机优先遍历 //delete by Shen Junjie on 2017/12/27 循环比较没有意义
                    //foreach (MyNodeIn nodeIn in myNodeList)
                    //{
                    //    if (nodeIn == mianNodeIn)
                    //    {
                    //        sortNodeInList.Add(nodeIn);
                    //        break;
                    //    }
                    //}
                    sortNodeInList.Add(mianNodeIn); //add by Shen Junjie on 2017/12/27 
                    //组成员后添加
                    if (mianNodeIn.RoomIndoorItem.IndoorItemGroup != null)
                    {
                        foreach (WiringNodeIn nodeIn in myNodeList)
                        {
                            foreach (RoomIndoor rind in mianNodeIn.RoomIndoorItem.IndoorItemGroup)
                            {
                                if (nodeIn.RoomIndoorItem == rind && !nodeIn.RoomIndoorItem.IsMainIndoor)
                                {
                                    sortNodeInList.Add(nodeIn);
                                    break;
                                }
                            }
                        }
                    }
                }

                //其他
                //if (sortNodeInList.Count != nodeCout) // delete by Shen Junjie on 2017/12/27
                if (sortNodeInList.Count < myNodeList.Count) //等同于上面的表达式  add by Shen Junjie on 2017/12/27
                {
                    foreach (WiringNodeIn nodeIn in myNodeList)
                    {
                        //if (nodeIn.RoomIndooItem.IndoorItemGroup == null && !nodeIn.RoomIndooItem.IsMainIndoor)  // delete by Shen Junjie on 2017/12/27
                        if (!sortNodeInList.Contains(nodeIn)) //解决bug: 因IndoorItemGroup为null，共享accessory的Indoor被添加了两次   (add by Shen Junjie on 2017/12/27)
                            sortNodeInList.Add(nodeIn);
                    }
                }

            }
            return sortNodeInList;
        }

        ///// <summary>
        ///// CHbox共享控制器主室内机优先排序   --add on 20170620 by Lingjia Qiu
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private List<Node> getSortNodeInListByChBox(MyNodeOut_Wiring nodeOut)
        //{
        //    List<Node> sortNodeInList = new List<Node>();
        //    List<MyNodeIn> mainNodeInList = new List<MyNodeIn>();
        //    int nodeCout = 0;
        //    foreach (Node node in nodeOut.ChildNodes)
        //    {
        //        if (node is ControllerWiringNodeCH)
        //        {
        //            ControllerWiringNodeCH nodeCH = node as ControllerWiringNodeCH;
        //            foreach (Node node1 in nodeCH.ChildNodes)
        //            {
        //                if (node1 is MyNodeIn)
        //                {
        //                    MyNodeIn nodeIn = node1 as MyNodeIn;
        //                    if (nodeIn.RoomIndooItem.IsMainIndoor)
        //                        mainNodeInList.Add(nodeIn);
        //                }
        //            }
        //        }
        //        else if (node is MyNodeIn)
        //        {
        //            MyNodeIn nodeIn = node as MyNodeIn;
        //            if (nodeIn.RoomIndooItem.IsMainIndoor)
        //                mainNodeInList.Add(nodeIn);
        //        }
        //        nodeCout++;
        //    }

        //    if (mainNodeInList.Count == 0)
        //    {
        //        //非共享控制器节点
        //        sortNodeInList = new List<Node>();
        //        foreach (Node node in nodeOut.ChildNodes)
        //        {
        //            sortNodeInList.Add(node);
        //        }
        //    }
        //    else
        //    {
        //        //共享控制器节点主室内机节点优先于共享关系组其他成员排序
        //        foreach (MyNodeIn mianNodeIn in mainNodeInList)
        //        {
        //            //主室内机优先遍历
        //            foreach (Node node in nodeOut.ChildNodes)
        //            {
        //                bool flag = false;
        //                if (node is ControllerWiringNodeCH)
        //                {
        //                    ControllerWiringNodeCH nodeCH = node as ControllerWiringNodeCH;
        //                    foreach (Node node1 in nodeCH.ChildNodes)
        //                    {
        //                        if (node1 is MyNodeIn)
        //                        {
        //                            MyNodeIn nodeIn = node1 as MyNodeIn;
        //                            if (nodeIn == mianNodeIn)
        //                            {
        //                                sortNodeInList.Add(node);
        //                                flag = true;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //                else if (node is MyNodeIn)
        //                {
        //                    MyNodeIn nodeIn = node as MyNodeIn;
        //                    if (nodeIn == mianNodeIn)
        //                    {
        //                        sortNodeInList.Add(node);
        //                        flag = true;
        //                        break;
        //                    }

        //                }
        //                if (flag)
        //                    break;
        //            }

        //            //组成员后添加  
        //            foreach (Node node in nodeOut.ChildNodes)
        //            {
        //                if (node is ControllerWiringNodeCH)
        //                {
        //                    ControllerWiringNodeCH nodeCH = node as ControllerWiringNodeCH;
        //                    foreach (Node node1 in nodeCH.ChildNodes)
        //                    {
        //                        if (node1 is MyNodeIn)
        //                        {
        //                            MyNodeIn nodeIn = node1 as MyNodeIn;
        //                            foreach (RoomIndoor rind in mianNodeIn.RoomIndooItem.IndoorItemGroup)
        //                            {
        //                                if (nodeIn.RoomIndooItem == rind && !nodeIn.RoomIndooItem.IsMainIndoor)
        //                                {
        //                                    sortNodeInList.Add(node);
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else if (node is MyNodeIn)
        //                {
        //                    MyNodeIn nodeIn = node as MyNodeIn;
        //                    foreach (RoomIndoor rind in mianNodeIn.RoomIndooItem.IndoorItemGroup)
        //                    {
        //                        if (nodeIn.RoomIndooItem == rind && !nodeIn.RoomIndooItem.IsMainIndoor)
        //                        {
        //                            sortNodeInList.Add(node);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }

        //        }

        //        //其他
        //        if (sortNodeInList.Count != nodeCout)
        //        {
        //            foreach (Node node in nodeOut.ChildNodes)
        //            {
        //                if (node is ControllerWiringNodeCH)
        //                {
        //                    ControllerWiringNodeCH nodeCH = node as ControllerWiringNodeCH;
        //                    foreach (Node node1 in nodeCH.ChildNodes)
        //                    {
        //                        if (node1 is MyNodeIn)
        //                        {
        //                            MyNodeIn nodeIn = node1 as MyNodeIn;
        //                            if (nodeIn.RoomIndooItem.IndoorItemGroup == null && !nodeIn.RoomIndooItem.IsMainIndoor)
        //                                sortNodeInList.Add(node);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    return sortNodeInList;
        //}

        /// <summary>
        /// 删除共享关联   --add on 20170621 by Lingjia Qiu
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="msgFlg">消息是否开启</param>
        private bool deleteShareRelationShip(RoomIndoor ri, bool msgFlg)
        {
            string groupIndoorName = "";
            if (ri.IndoorItemGroup == null||ri.IndoorItemGroup.Count==0)
                return false;

            foreach (RoomIndoor rind in ri.IndoorItemGroup)
            {
                groupIndoorName = groupIndoorName += rind.IndoorName + ",";
            }
            groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
            if (msgFlg)
            {
                DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_Delete_Sharing_RelationShip(ri.IndoorName, groupIndoorName));
                if (result != DialogResult.OK)
                    return false;
            }
            foreach (string indNo in groupIndoorName.Split(','))
            {
                RoomIndoor riItem = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo.Substring(indNo.Length - 1)));
                riItem.IndoorItemGroup = null;
                if (riItem.IsMainIndoor)
                    riItem.IsMainIndoor = false;
            }

            return true;
        }

        /// <summary>
        /// 维护已选的记录并检查多选条件限制   --add on 20170622 by Lingjia Qiu
        /// </summary>
        private bool checkMutiSelected(out string reContrIndoorName)
        {

            List<RoomIndoor> riGroupSeletedList = new List<RoomIndoor>();
            reContrIndoorName = "";
            string selectedInd = "";
            string unSelectedInd = "";
            if (this.dgvIndoor.SelectedRows.Count > 1)
            {
                //维护已选的记录
                foreach (DataGridViewRow r in this.dgvIndoor.SelectedRows)
                {
                    string indNo = r.Cells[Name_Common.NO].Value.ToString();
                    if (NumberUtil.IsNumber(indNo))
                    {
                        RoomIndoor rid = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
                        if (rid.IndoorItemGroup != null)
                        {
                            reContrIndoorName += r.Cells[Name_Common.Name].Value.ToString() + ",";
                            if (!string.IsNullOrEmpty(rid.SystemID))
                                riGroupSeletedList.Add(rid);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(rid.SystemID))
                                unSelectedInd = rid.IndoorName;
                            else
                                selectedInd = rid.IndoorName;
                        }
                    }
                }
                //已经选型的室内机和未参加选型的室内机无法共享控制器
                if (!string.IsNullOrEmpty(selectedInd) && !string.IsNullOrEmpty(unSelectedInd))
                {
                    JCMsg.ShowWarningOK(Msg.IND_MutiSelected_Sys_Different(selectedInd, unSelectedInd));
                    return false;
                }
                //已经进行系统选型并共享控制器的室内机不能多选分配ACCESSORY
                if (riGroupSeletedList.Count != 0)
                {
                    foreach (RoomIndoor riGroupItem in riGroupSeletedList)
                    {
                        if (!riGroupItem.SystemID.Equals(riGroupSeletedList[0].SystemID))
                        {
                            JCMsg.ShowWarningOK(Msg.IND_MutiSelected_Sys_Different(riGroupItem.IndoorName, riGroupSeletedList[0].IndoorName));
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        private void pbAddExchangerOption_Click(object sender, EventArgs e)
        {
            DoAddExchangerOptions();
        }

        // 添加Exchanger Options
        /// <summary>
        /// 添加Exchanger Options
        /// </summary>
        private void DoAddExchangerOptions()
        {

            if (this.dgvExchanger.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }

            if (this.dgvExchanger.Rows.Count > 0 && this.dgvExchanger.SelectedRows.Count > 0)
            {
                //DataGridViewRow row = dgvIndoor.SelectedRows[0];
                //string model = row.Cells[Name_Common.ModelFull].Value.ToString();
                //string indNo = row.Cells[Name_Common.NO].Value.ToString();
                //if (NumberUtil.IsNumber(indNo))
                //{
                //    RoomIndoor ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
                //    frmAddAccessory f = new frmAddAccessory(ri.IndoorItem);
                //    f.ShowDialog();
                //}
                #region   --modify on 20170613 by Lingjia Qiu
                List<RoomIndoor> rinItemList = new List<RoomIndoor>();
                // List<RoomIndoor> riGroupSeletedList = new List<RoomIndoor>();
                List<Indoor> indItemList = new List<Indoor>();
                string reContrIndoorName = "";
                string optionIndoorName = "";
                string groupIndoorName = "";

                //维护已选的记录并检查多选条件限制
                if (!checkMutiSelected(out reContrIndoorName))
                    return;

                foreach (DataGridViewRow r in this.dgvExchanger.SelectedRows)
                {
                    string indNo = r.Cells[Name_Common.NO].Value.ToString();
                    if (NumberUtil.IsNumber(indNo))
                    {
                        RoomIndoor ri = (new ProjectBLL(thisProject)).GetExchanger(int.Parse(indNo));

                        //if (string.IsNullOrEmpty(ri.IndoorItem.IndoorName))
                        //{
                        //    ri.IndoorFullName = thisProject.BrandCode == "Y" ? ri.IndoorName + "[" + ri.IndoorItem.Model_York + "]" : ri.IndoorName + "[" + ri.IndoorItem.Model_Hitachi + "]";  //记录indoor对象的indoor名
                        //    ri.IndoorItem.IndoorName = ri.IndoorFullName;
                        //}

                        rinItemList.Add(ri);
                        //indItemList.Add(ri.IndoorItem);

                        if (dgvExchanger.SelectedRows.Count > 1 && ri.IndoorItemGroup != null)  //多选已共享控制器的室内机维护indoorName
                        {
                            if (ri.IndoorItemGroup.Count != 0)
                            {
                                foreach (RoomIndoor rind in ri.IndoorItemGroup)
                                {
                                    //将已选共享控制器所关联的室内机维护indoorName
                                    if (!reContrIndoorName.Contains(rind.IndoorName) && !groupIndoorName.Contains(r.Cells[Name_Common.Name].Value.ToString()) && !groupIndoorName.Contains(rind.IndoorName))
                                    {
                                        groupIndoorName += rind.IndoorName + ",";
                                    }
                                }

                            }

                        }
                        else if (dgvExchanger.SelectedRows.Count > 1 && ri.ListAccessory != null)  //多选已分配配件的室内机维护indoorName
                        {
                            if (ri.ListAccessory.Count != 0)
                                optionIndoorName += r.Cells[Name_Common.Name].Value.ToString() + ",";
                        }

                    }

                }

                //对List进行排序
                rinItemList.Sort(delegate(RoomIndoor x, RoomIndoor y)
                {
                    return x.IndoorNO.CompareTo(y.IndoorNO);
                });
                //rinItemList.ToList().OrderBy(p => p.IndoorNO);
                //indItemList.Sort(delegate(Indoor x, Indoor y)
                //{
                //    string xIndName = x.IndoorName.Split('[')[0];
                //    string yIndName = y.IndoorName.Split('[')[0];
                //    return xIndName.CompareTo(yIndName);
                //});
                //转写排序，不要在Indoor加IndoorName属性 modified by Shen Junjie 2018/3/21
                indItemList = (from p in rinItemList orderby p.IndoorFullName select p.IndoorItem).ToList();
                //indItemList.ToList().OrderBy(p => p.IndoorName);

                AccessoryBLL bill = new AccessoryBLL();
                DataTable allAvailable = bill.GetAllAvailable(indItemList, thisProject.RegionCode, thisProject.SubRegionCode);
                if (allAvailable == null)
                {
                    JCMsg.ShowInfoOK(Msg.EXC_NO_AVAILABLE_ACCESSORY);
                    return;
                }
                //插入type翻译列
                allAvailable = getAccessoryTypeTransDt(allAvailable);
                if (dgvExchanger.SelectedRows.Count > 1)
                {
                    if (!string.IsNullOrEmpty(reContrIndoorName))   //已共享控制器的室内机进行关联关系清空
                    {
                        reContrIndoorName = reContrIndoorName.Substring(0, reContrIndoorName.Length - 1);
                        string allIndoorName = "";
                        if (!string.IsNullOrEmpty(groupIndoorName))
                        {
                            groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
                            allIndoorName = reContrIndoorName + "," + groupIndoorName;   //所有关联室内机
                        }
                        else
                            allIndoorName = reContrIndoorName;



                        //提示并进行YES OR NO处理                       
                        DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_Sharing_RemoteController(reContrIndoorName, groupIndoorName));
                        if (result != DialogResult.OK)
                            return;

                        //对已选共享控制器的全热交换机及其相关共享的室内机进行关联清空
                        foreach (DataGridViewRow r in this.dgvExchanger.Rows)
                        {
                            string indNo = r.Cells[Name_Common.NO].Value.ToString();
                            string indName = r.Cells[Name_Common.Name].Value.ToString();
                            foreach (string indoorName in allIndoorName.Split(','))   //循环遍历已选共享控制器的全热交换机及相关共享的室内机名
                            {
                                if (indoorName.Equals(indName))   //如果共享控制器
                                {
                                    RoomIndoor ri = (new ProjectBLL(thisProject)).GetIndoor(int.Parse(indNo));
                                    if (ri.IndoorItemGroup != null)
                                    {
                                        if (ri.IndoorItemGroup.Count != 0)
                                            ri.IndoorItemGroup = null;   //清空关联关系
                                        if (ri.IsMainIndoor)
                                            ri.IsMainIndoor = false;   //重置主室内机
                                    }
                                }
                            }
                        }

                    }
                    else if (!string.IsNullOrEmpty(optionIndoorName))   //全热交换机多选已有配件清空   
                    {
                        optionIndoorName = optionIndoorName.Substring(0, optionIndoorName.Length - 1);
                        DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_ACCESSORY(optionIndoorName));
                        if (result != DialogResult.OK)
                            return;
                    }

                }

                frmAddAccessory f = new frmAddAccessory(rinItemList, thisProject, allAvailable);
                f.ShowDialog();
                #endregion

            }
        }

        private void pbClearExchangerOption_Click(object sender, EventArgs e)
        {
            DoClearExchangerOptions();
        }

        /// <summary>
        /// 清空Exchanger Options
        /// </summary>
        private void DoClearExchangerOptions()
        {
            if (this.dgvExchanger.SelectedRows.Count == 0)
            {
                //请选择至少一项
                JCMsg.ShowInfoOK(JCMsg.WARN_SELECTONEMORE);
                return;
            }
            if (this.dgvExchanger.Rows.Count > 0 && this.dgvExchanger.SelectedRows.Count > 0)
            {
                DialogResult res = JCMsg.ShowConfirmOKCancel(Msg.IDU_ACCESSORYE_CLEAR);
                // 可以清空配件
                if (res == DialogResult.OK)
                {
                    foreach (DataGridViewRow row in dgvExchanger.SelectedRows)
                    {
                        int indNo = Convert.ToInt32(row.Cells[Name_Common.NO].Value);
                        RoomIndoor ri = (new ProjectBLL(thisProject)).GetExchanger(indNo);

                        List<Accessory> list = (new AccessoryBLL()).GetDefault(ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode, ri.IndoorItem.Series);
                        ri.ListAccessory = list;
                        //清空共享控制器关联  add on 20170622 by Lingjia Qiu
                        if (ri.IndoorItemGroup != null)
                        {
                            ri.IndoorItemGroup = null;
                            if (ri.IsMainIndoor)
                                ri.IsMainIndoor = false;
                        }
                    }
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                }
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        private void jctxtProjName_TextChanged(object sender, EventArgs e)
        {
            thisProject.Name = jctxtProjName.Text;
            this.Text = Msg.GetResourceString("PROJECT_TITLE_NAME") + thisProject.Name;
        }

        /// <summary>
        /// 绑定 Controller Wiring 界面中的 ControlGroups 下拉框
        /// </summary>
        private void BindControlGroupsCMB()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            foreach (ControlGroup group in thisProject.ControlGroupList)
            { 
                if (group.IsValidGrp)
                {
                    DataRow dr = dt.NewRow();
                    dr["Id"] = group.Id;
                    dr["Name"] = group.Name;
                    //ControlGroupList过滤没有匹配到对应的system on 20180308 by xyj
                    bool isTrue = false; 
                    foreach (SystemVRF sysItem in thisProject.SystemList)
                    {

                        if (sysItem.ControlGroupID!=null && sysItem.ControlGroupID[0] != "" &&sysItem.ControlGroupID.Contains(group.Id))
                        {
                            isTrue = true;
                            break;
                        }
                        if (thisProject.ExchangerList.FindAll(p => p.ControlGroupID[0] == group.Id).Count>0)
                        {
                            isTrue = true;
                            break;
                        }
                    }
                    if (isTrue)
                        dt.Rows.Add(dr);
                }
            }

            this.jccmbControlGroups.DisplayMember = "Name";
            this.jccmbControlGroups.ValueMember = "Id";
            this.jccmbControlGroups.DataSource = dt;
        }

        private void jcbtnControllerWiringFitWindow_Click(object sender, EventArgs e)
        {
            float f = ControllerWiringBLL.GetFitWindowZoom(addFlowControllerWiring);
            addFlowControllerWiring.Zoom = new Zoom(f, f);

            jccmbControllerWiringScale.Text = (f * 100).ToString("n0");
        }

        private void jccmbControlGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.jccmbControlGroups.SelectedValue != null)
            {
                string groupId = this.jccmbControlGroups.SelectedValue.ToString();
                foreach (ControlGroup group in thisProject.ControlGroupList)
                {
                    if (group.Id == groupId)
                    {
                        ControllerWiringBLL bll = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
                        bll.DrawWiring(group);
                    }
                }
            }
        }

        private void jccmbControllerWiringScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            string scale = jccmbControllerWiringScale.Text.Trim();
            float f;
            if (!string.IsNullOrEmpty(scale) && float.TryParse(scale, out f))
            {
                f /= 100;
                addFlowControllerWiring.Zoom = new Zoom(f, f);
            }
            else
            {
                jccmbControllerWiringScale.Text = "100";
            }
        }

        private void jcbtnAddMultipleCHBox_Click(object sender, EventArgs e)
        {
            if (ManualPipingCleanConfirmation() != DialogResult.Yes)
            {
                return;
            }
            PipingBLL pipBll = GetPipingBLLInstance();
            MyNodeMultiCH nodeMCH = null;

            if (pipBll.ExistCPUpward(selNode))
            {
                //梳形管后面不能添加Multi CH Box
                return;
            }

            if (selNode is MyNodeIn)
            {
                MyNodeIn nodeIn = selNode as MyNodeIn;
                MyNode parentNode = nodeIn.ParentNode;
                if (parentNode is MyNodeCH)
                {
                    nodeMCH = pipBll.ReplaceCH2MultiCH(parentNode as MyNodeCH);
                }
                else if (pipBll.ExistCHBoxUpward(selNode))
                {
                    //Do not do anything.
                    return;
                }
                else
                {
                    nodeMCH = pipBll.InsertMultiCH(nodeIn);
                }
            }
            else if (selNode is MyNodeYP)
            {
                MyNodeYP nodeYP = selNode as MyNodeYP;
                MyNode parentNode = nodeYP.ParentNode;
                if (parentNode is MyNodeCH)
                {
                    nodeMCH = pipBll.ReplaceCH2MultiCH(parentNode as MyNodeCH);
                }
                else if (pipBll.ExistCHBoxUpward(selNode))
                {
                    //Do not do anything.
                    return;
                }
                else
                {
                    nodeMCH = pipBll.InsertMultiCH(nodeYP);
                    pipBll.DeleteCHBoxDownward(nodeYP);
                }
            }
            else if (selNode is MyNodeCH)
            {
                nodeMCH = pipBll.ReplaceCH2MultiCH(selNode as MyNodeCH);
            }

            if (nodeMCH == null) return;

            //DoPipingCalculation之前需要先做Piping Verfication,所以这边不自动调用 deleted by Shen Junjie on 2018/3/29
            //PipingErrors errorType;
            //DoPipingCalculation(pipBll, curSystemItem.MyPipingNodeOut, out errorType);
            //if (errorType != PipingErrors.OK)
            //{
            //    ShowWarningMsg(errorType);
            //    return;
            //}

            DoDrawingPiping(true);
            SetSystemPipingOK(curSystemItem, false);
        }

        private void pbLoadUnitInfo_Click(object sender, EventArgs e)
        {

            RoomIndoor ri = new RoomIndoor();
            if (tvOutdoor.SelectedNode != null)
            {
                TreeNode node = tvOutdoor.SelectedNode;
                switch (node.Level)
                {
                    case 1:
                        ri = node.Tag as RoomIndoor;

                        break;
                    case 2:
                        if (node.Tag is RoomIndoor)
                        {
                            ri = node.Tag as RoomIndoor;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (ri != null)
            {
                frmSystemUnit f = new frmSystemUnit(thisProject, ri);
                f.StartPosition = FormStartPosition.CenterScreen;
                f.FormBorderStyle = FormBorderStyle.None;
                f.ShowDialog();
            }
        }

        private void jcbtnPipingRules_Click(object sender, EventArgs e)
        {
            if (curSystemItem == null || curSystemItem.OutdoorItem == null) return;
            //PipingBLL pipBll = GetPipingBLLInstance();
            //pipBll.SetPipingLimitation(curSystemItem);
            //PipingErrors errorType = pipBll.ValidatePipeLength(curSystemItem, addFlowPiping);
            //ShowWarningMsg(errorType);
            DoPipingFinalVerification();
            frmPipingRules f = new frmPipingRules(thisProject, curSystemItem); 
            f.StartPosition = FormStartPosition.CenterScreen;
            this.AddOwnedForm(f); 
            f.ShowDialog();
        }
        /// <summary>
        /// 初始化ProjectInfo   --add on 20180309
        /// </summary>
        private void initProjectInfo()
        {
            if (ProjectBLL.IsUsingEuropeProjectInfo(_mainRegion))   //对EU ProjectInfo的元素特殊处理
                this.pnlContent_ProjectInfo_Eu.Visible = true;   
            else
                this.pnlContent_ProjectInfo_Eu.Visible = false;
        }


        /// <summary>
        /// 绑定project的信息   --add on 20180412 by Vince    
        /// </summary>
        /// <param name="mainRegion"></param>       
        /// <param name="isSourceToProject">是否是从控件值分装project</param>       
        private void bindProjectInfo(string mainRegion,bool isSourceToProject)
        {
            
            thisProject.Name = this.jctxtProjName.Text;
            thisProject.Altitude = int.Parse(this.jctxtAltitude.Text == "" ? "0" : this.jctxtAltitude.Text);
            thisProject.Version = MyConfig.Version;  //维护当前版本号   add on 20161130 by Lingjia Qiu
            thisProject.ProjectUpdateDate = DateTime.Now.ToString("yyyy/MM/dd");  //添加项目最后修改时间 on 20180312 by xyj

            if (ProjectBLL.IsUsingEuropeProjectInfo(mainRegion))   //对EU ProjectInfo的元素特殊处理
            {
                if (isSourceToProject)
                {
                    thisProject.SalesEngineer = this.jctxtSalesEngineer1.Text;
                    thisProject.salesCompany = this.jctxtCompany.Text;
                    thisProject.salesAddress = this.jctxtAddress.Text;
                    thisProject.salesPhoneNo = this.jctxtPhoneNo.Text;
                    thisProject.clientName = this.jctxtClientName.Text;
                    thisProject.postCode = this.jctxtPostCode.Text;
                    thisProject.clientTel = this.jctxtTel.Text;
                    thisProject.clientMail = this.jctxtMail.Text;
                    thisProject.PurchaseOrderNO = this.jctxtPurchaseOrderNo1.Text;
                    thisProject.OrderDate = this.timeOrderDate1.Value;
                    thisProject.DeliveryRequiredDate = this.timeDeliveryDate1.Value;
                    thisProject.Location = this.jctxtProjectLocation1.Text;
                    thisProject.ProjectRevision = this.jctxtProjectRevision1.Text;
                    thisProject.Remarks = this.jctxtRemarks1.Text;
                }
                else
                {
                    this.jctxtSalesEngineer1.Text = thisProject.SalesEngineer;
                    this.jctxtCompany.Text = thisProject.salesCompany;
                    this.jctxtAddress.Text = thisProject.salesAddress;
                    this.jctxtPhoneNo.Text = thisProject.salesPhoneNo;
                    this.jctxtClientName.Text = thisProject.clientName;
                    this.jctxtPostCode.Text = thisProject.postCode;
                    this.jctxtTel.Text = thisProject.clientTel;
                    this.jctxtMail.Text = thisProject.clientMail;
                    this.jctxtPurchaseOrderNo1.Text = thisProject.PurchaseOrderNO;
                    this.timeOrderDate1.Text = thisProject.OrderDate.ToShortDateString();
                    this.timeDeliveryDate1.Text = thisProject.DeliveryRequiredDate.ToShortDateString();
                    this.jctxtProjectLocation1.Text = thisProject.Location;
                    this.jctxtProjectRevision1.Text = thisProject.ProjectRevision;
                    this.jctxtRemarks1.Text = thisProject.Remarks;

                    if (!String.IsNullOrEmpty(thisProject.ProjectUpdateDate)) //打开项目时加载项目最后修订日期 on 20180312 by xyj
                    {
                        this.jctxtLastUpdateDate1.Text = thisProject.ProjectUpdateDate;
                    }
                    else
                    {
                        this.jctxtLastUpdateDate1.Text = "";
                    }
                
                }
                
            }
            else
            {
                if (isSourceToProject)
                {
                    thisProject.ContractNO = this.jctxtContractNo.Text;
                    thisProject.ProjectRevision = this.jctxtProjectRevision.Text;
                    thisProject.Location = this.jctxtLocation.Text;
                    thisProject.PurchaseOrderNO = this.jctxtPurchaseOrderNo.Text;
                    thisProject.Remarks = this.jctxtRemarks.Text;
                    thisProject.SalesEngineer = this.jctxtSalesName.Text;
                    thisProject.SalesOffice = this.jctxtSalesOffice.Text;
                    thisProject.SalesYINO = this.jctxtSalesYINo.Text;
                    thisProject.ShipTo = this.jctxtShipTo.Text;
                    thisProject.SoldTo = this.jctxtSoldTo.Text;
                    thisProject.DeliveryRequiredDate = this.timeDeliveryDate.Value;
                    thisProject.OrderDate = this.timeOrderDate.Value;

                }
                else
                {
                    this.jctxtContractNo.Text = thisProject.ContractNO;
                    this.jctxtProjectRevision.Text = thisProject.ProjectRevision;
                    this.jctxtLocation.Text = thisProject.Location;
                    this.jctxtPurchaseOrderNo.Text = thisProject.PurchaseOrderNO;
                    this.jctxtRemarks.Text = thisProject.Remarks;
                    this.jctxtSalesName.Text = thisProject.SalesEngineer;
                    this.jctxtSalesOffice.Text = thisProject.SalesOffice;
                    this.jctxtSalesYINo.Text = thisProject.SalesYINO;
                    this.jctxtShipTo.Text = thisProject.ShipTo;
                    this.jctxtSoldTo.Text = thisProject.SoldTo;
                    this.timeDeliveryDate.Text = thisProject.DeliveryRequiredDate.ToShortDateString();
                    this.timeOrderDate.Text = thisProject.OrderDate.ToShortDateString();

                    if (!String.IsNullOrEmpty(thisProject.ProjectUpdateDate)) //打开项目时加载项目最后修订日期 on 20180312 by xyj
                    {
                        this.jctxtProjectUpdateDate.Text = thisProject.ProjectUpdateDate;
                    }
                    else
                    {
                        this.jctxtProjectUpdateDate.Text = "";
                    }
                }
            }
            ValidateControl(this.jctxtContractNo);
            ValidateControl(this.jctxtPurchaseOrderNo);
            ValidateControl(this.jctxtPurchaseOrderNo1);
        }

        /// <summary>
        /// 验证控件
        /// </summary>
        /// <param name="trol">控件</param>
        private void ValidateControl(JCTextBox trol)
        {
            if (!string.IsNullOrWhiteSpace(trol.Text))
            {
                trol.RequireValidation = true;
            }
            else
            {
                trol.RequireValidation = false;
            }
            JCValidateSingle(trol);
        }

        /// <summary>
        /// Actual 复选框勾选事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uc_CheckBox_Actual_CheckedChanged(object sender, EventArgs e)
        {
            if (uc_CheckBox_Actual.Checked == true)
            {
                uc_CheckBox_Nominal.Checked = false;
            }
            else
            {
                uc_CheckBox_Nominal.Checked = true;
            }
        }

        /// <summary>
        /// Nominal 复选框勾选事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uc_CheckBox_Nominal_CheckedChanged(object sender, EventArgs e)
        {
            if (uc_CheckBox_Nominal.Checked == true)
            {
                uc_CheckBox_Actual.Checked = false;
            }
            else
            {
                uc_CheckBox_Actual.Checked = true;
            }
        }

        private void uc_CheckBox_Heating_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 绑定EU室内机选择方式
        /// 1，带房间的选型   2，不带房间的选型
        /// </summary>
        /// <returns></returns>
        private void BindCmbIndoorType()
        {
            List<string> typeList = new List<string>();
            typeList.Add(Msg.INDOOR_SELECTED_TYPE(false));   //不带房间室内机选型
            typeList.Add(Msg.INDOOR_SELECTED_TYPE(true));   //带房间
            jccmbIndoorType.DataSource = typeList;
            if (thisProject.RoomIndoorList.Count == 0)
                jccmbIndoorType.SelectedIndex = 0;
            else
            {
                //有房间分配的，默认选中基于房间选型               
                if (isWithRoom())
                    jccmbIndoorType.SelectedIndex = 1;

            }

        }


        bool isChangeRoomIndex = false;   //对于基于房间的室内机选型后切换语言的保持标示（初始Combo控件导致index无法赋值先跳事件问题）
        private void jccmbIndoorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isChangeRoomIndex)
                jccmbIndoorType.SelectedIndex = 1;
            isChangeRoomIndex = false;//恢复标识

            if (jccmbIndoorType.SelectedIndex == 0)   //不带房间的室内机选型
            {
                //EU特殊逻辑：在project保持清一色带房间或不带房间的室内机，两者不能混合选型。
                if (_thisProject.RoomIndoorList.Count > 0 && isWithRoom())
                {
                    DialogResult res;
                    res = JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGE_INDOOR_TYPE(Msg.INDOOR_SELECTED_TYPE(true)));
                    // 可以直接室内室外机
                    if (res == DialogResult.OK)
                    {
                        clearIndAndSys();
                    }
                    else
                    {
                        jccmbIndoorType.SelectedIndex = 1;
                        return;
                    }

                }
                uc_CheckBox_IsRoomBase.Checked = false;
            }
            else
            {
                //带房间的室内机选型
                if (_thisProject.RoomIndoorList.Count > 0 && !isWithRoom())
                {
                    DialogResult res;
                    res = JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGE_INDOOR_TYPE(Msg.INDOOR_SELECTED_TYPE(false)));
                    // 可以直接室内室外机
                    if (res == DialogResult.OK)
                    {
                        clearIndAndSys();
                    }
                    else
                    {
                        jccmbIndoorType.SelectedIndex = 0;
                        return;
                    }

                }
                uc_CheckBox_IsRoomBase.Checked = true;
            }
        }


        /// <summary>
        /// 是否存在基于room选型的室内机   --add on 20180524 by Vince
        /// </summary>
        /// <returns></returns>
        private bool isWithRoom()
        {
            //有房间分配的，默认选中基于房间选型
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (string.IsNullOrEmpty(ri.RoomID))                
                    return false;  
                else
                    continue;

            }
            return true;
        }


        /// <summary>
        /// 清除所选室内机和室外机，保留project信息   --add on 20180525 by Vince
        /// </summary>
        /// <returns></returns>
        private void clearIndAndSys()
        {
            _thisProject.RoomIndoorList = new List<RoomIndoor>();
            _thisProject.SystemList = new List<SystemVRF>();
            _thisProject.ExchangerList = new List<RoomIndoor>();
            //初始后刷新
            BindToControl_SelectedUnits();
        }

        private void jctxtSalesEngineer1_TextChanged(object sender, EventArgs e)
        {
                SystemSetting.UserSetting.defaultSetting.SalesEngineer = this.jctxtSalesEngineer1.Text;
                SystemSetting.Serialize();
        }

        private void jctxtSalesOffice_TextChanged(object sender, EventArgs e)
        {
                SystemSetting.UserSetting.defaultSetting.SalesOffice =this.jctxtSalesOffice.Text;
                SystemSetting.Serialize();
        }

        private void jctxtSalesName_TextChanged(object sender, EventArgs e)
        {
                SystemSetting.UserSetting.defaultSetting.SalesName =this.jctxtSalesName.Text;
                SystemSetting.Serialize();
            
        }

        #region bug 632 amended by HCL 20180927
        private void jctxtPurchaseOrderNo1_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.jctxtPurchaseOrderNo1.Text))
            {
                jctxtPurchaseOrderNo1.RequireValidation = true;
            }
            else {
                jctxtPurchaseOrderNo1.RequireValidation = false;
            }
            JCValidateSingle(this.jctxtPurchaseOrderNo1);
        }

        private void jctxtPurchaseOrderNo_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.jctxtPurchaseOrderNo.Text))
            {
                jctxtPurchaseOrderNo.RequireValidation = true;
            }
            else {
                jctxtPurchaseOrderNo.RequireValidation = false;
            }
            JCValidateSingle(this.jctxtPurchaseOrderNo);
        }

        private void jctxtContractNo_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.jctxtContractNo.Text))
            {
                jctxtContractNo.RequireValidation = true; 
            }
            else
            {
                jctxtContractNo.RequireValidation = false;
            }
            JCValidateSingle(this.jctxtContractNo);
        }
     
        #endregion

        /// 获取AccessoryType翻译重新构造Exchanger有效Accessory
        /// <summary>
        /// 获取AccessoryType翻译重新构造Exchanger有效Accessory   add on 20181026 by Vince
        /// </summary>
        /// <param name="dataTable"></param>
        public DataTable getAccessoryTypeTransDt(DataTable dataTable)
        {            
            if (!dataTable.Columns.Contains("TypeDisplay"))
                dataTable.Columns.Add("TypeDisplay");   //添加新列用于存放翻译后的名称
            Trans trans = new Trans();
            foreach (DataRow dr in dataTable.Rows)
            {
                string typeResource = dr["Type"].ToString();
                string typeTrans = trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), typeResource);
                if (string.IsNullOrEmpty(typeTrans))
                    dr["TypeDisplay"] = typeResource;   //取不到翻译默认显示英文
                else
                    dr["TypeDisplay"] = typeTrans;

            }
            return dataTable;
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil.Dispose();
        }
    }
}
