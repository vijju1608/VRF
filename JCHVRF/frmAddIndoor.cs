using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Util;
using JCHVRF.VRFMessage;
using JCHVRF.Const;
using JCBase.Utility;
using JCHVRF.VRFTrans;

namespace JCHVRF
{
    public partial class frmAddIndoor : JCBase.UI.JCForm
    {
        #region Initialization 初始化

        bool IsAuto = false;        // 当前Auto按钮的选中状态
        string curSelectType = "";  // 当前室内机类型下拉框的值
        const string FRESH_AIR_AREA_ROOT_NODE_NAME = "FreshAirAreaRoot";

        bool PassValidation = true;

        string utAirflow;   // 单位表达式
        string utPower;
        string utTemperature;
        string utPressure; // static pressure add on 20170703 by Shen Junjie

        bool flag = false;

        Project thisProject;
        ProjectBLL pbll;
        IndoorBLL bll;
        Room curRoom = null;
        FreshAirArea curFreshAirArea = null;
        RoomIndoor curRI = null;

        string _factory = ""; //当前室内机类型的工厂 add by Shen Junjie on 20170707
        string _type = ""; //当前室内机类型  add by Shen Junjie on 20170707
        string _productType; //当前选择的ProductType Add in 20160821 by Yunxiao Lin
        string _series;   //当前选择的Series Add in 20161027 by Yunxiao Lin
        //string _shf_mode;   //当前选择的shf，三档：High，Medium，Low
        int _fanSpeedLevel = -1; //风扇速度等级 -1:Max, 0:High2, 1:High, 2:Med, 3:Low add on 20170703 by Shen Junjie
        UndoRedo.UndoRedoHandler UndoRedoUtil = null;//注册撤销实体对象 add by axj 20161228
        
        //初始化工况温度 add by axj 20170116
        private double outdoorCoolingDB = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
        private double outdoorHeatingWB = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;
        private double outdoorCoolingIW = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
        private double outdoorHeatingIW = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;

        public static Color ColorAssigned = Color.Blue;         //Indoor界面
        public static Color ColorOriginal = Color.Black;

        bool RoomHasIndoor = false;
        double roomDB = 0;
        double roomWB = 0;
        double roomRH = 0;

        Trans trans = new Trans();  //翻译初始化
        /// <summary>
        /// 构造函数，新增
        /// </summary>
        /// <param name="thisProj"> 当前项目对象 </param>
        public frmAddIndoor(Project thisProj)
        {
            InitializeComponent();
            JCSetLanguage();    // 设置界面语言 
            thisProject = thisProj;
            JCFormMode = FormMode.NEW;
            curRI = new RoomIndoor();
            this.IsAuto = this.uc_CheckBox_Auto.Checked;
            this.pnlRoom_1.Enabled = true;
        }

        /// <summary>
        /// 构造函数，编辑
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="thisProj">当前项目对象</param>
        /// <param name="rflag">true 则表示当前房间已选室内机不合适（Error图标）</param>
        public frmAddIndoor(RoomIndoor ri, Project thisProj, Boolean rflag)
        {
            InitializeComponent();
            JCSetLanguage();    // 设置界面语言

            thisProject = thisProj;
            JCFormMode = FormMode.EDIT;
            curRI = ri;
            pbll = new ProjectBLL(thisProject);
            if (!ri.IsFreshAirArea)
                curRoom = pbll.GetRoom(ri.RoomID);
            else
            {
                foreach (FreshAirArea area in thisProject.FreshAirAreaList)
                {
                    if (area.Id == ri.RoomID)
                    {
                        curFreshAirArea = area;
                        break;
                    }
                }
            }
            //this.pnlRoom_1.Enabled = false;

            // 室内机编辑界面隐藏Room选择，用显示Room名称代替。2016-4-18 by lin
            this.pnlRoom_1.Enabled = true; //enabled=False会引起容器内控件字体颜色变灰，所以设为True。2016-4-18 by lin
            this.pnlIndoor_3.Visible = true;
            ChkOutdoorTemperature(ri, thisProj);//检查工况温度 add by axj 20170116
            flag = rflag;
        }

        private void frmAddIndoor_Load(object sender, EventArgs e)
        {
            Initialization();
            /*注册撤销功能 add by axj 20161228 begin */
            UndoRedoUtil = new UndoRedo.UndoRedoHandler(true);
            UndoRedoUtil.ShowIconsOnTabPage(tapPageTrans1, new Rectangle(970, 6, 16, 18), new Rectangle(942, 6, 16, 18));

            UndoRedoUtil.GetCurrentProjectEventHandler += delegate (out Project prj) //获取最新项目数据
            {
                prj = thisProject.DeepClone();//返回当前项目数据的副本
            };
            UndoRedoUtil.ReloadProjectEventHandler += delegate(Project prj) //重新加载历史记录里面的项目数据
            {
                thisProject = prj;
                Initialization();
                //this.Invalidate();
            };
            /*注册撤销功能 end*/

        }

        /// <summary>
        /// 初始化界面 add by axj 20161228
        /// </summary>
        protected void Initialization()
        {
            pbll = new ProjectBLL(thisProject);
            // 由于多ProductType功能不再使用thisProject.ProductType参数，因此删除
            //bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.ProductType,thisProject.BrandCode);


            this.JCCallValidationManager = true;    // 启用界面控件验证
            this.uc_CheckBox_Auto.CheckedChanged -= new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            this.uc_CheckBox_Manual.CheckedChanged -= new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            toolStripStatusLabel1.Text = "";

            // 当已选室内机的房间需求信息改变，导致已选的室内机不满足房间需求时，系统给出相应的提示信息
            if (JCFormMode == FormMode.EDIT)
            {
                //List<string> roomList;
                //if (!CheckSelectedIndoorList(out roomList))
                //{
                //    string str = "";
                //    foreach (string s in roomList)
                //    {
                //        str += s + " ";
                //    }
                //    JCMsg.ShowWarningOK("( " + str + ")" + Msg.IND_NOTMATCH_ROOMCHANGE);
                //}
                if (flag)
                    JCMsg.ShowWarningOK(Msg.IND_NOTMATCH_ROOMCHANGE);

            }

            InitDGV();
            BindUnit();
            BindShfMode();
            BindCmbProductType(); //绑定Indoor的ProductType列表 add on 20160821 by Yunxiao Lin
            // bll初始化放在这边，下面的BindIndoorTypeList要用到 20160906 by Yunxiao Lin
            bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            BindIndoorTypeList(true);
            // 多productType 20160821 by Yunxiao Lin
            //bll = new IndoorBLL(thisProject.SubRegionCode, _productType, thisProject.BrandCode);
            //bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);

            BindDBWB(); // Updated on 20140623 clh // 必须放到BindTreeViewRoom之前执行，因为BindTreeViewRoom()会触发Auto选型

            BindTreeViewRoom();

            ResetControlState(this.uc_CheckBox_Auto.Checked);

            if (uc_CheckBox_Auto.Checked == true && flag == true)
            {
                DoAutoSelect();
            }
            this.uc_CheckBox_Auto.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            this.uc_CheckBox_Manual.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            this.uc_CheckBox_Auto.TextString = ShowText.Auto;
            this.uc_CheckBox_Manual.TextString = ShowText.Manual;

            this.tvRoom.ExpandAll();
            if (this.tvRoom.Nodes.Count > 0 && this.JCFormMode == FormMode.NEW)
            {
                this.tvRoom.Nodes[0].EnsureVisible();
            }
        }

        #endregion

        #region Controls events

        ///// <summary>
        ///// 当已选室内机的房间更改了房间需求时，检测已选的室内机是否满足新的需求
        ///// </summary>
        //private bool CheckSelectedIndoorList(out List<string> roomList)
        //{
        //    roomList = new List<string>();
        //    bool pass = true;

        //    // 遍历房间
        //    foreach (Floor f in thisProject.FloorList)
        //    {
        //        foreach (Room room in f.RoomList)
        //        {
        //            List<RoomIndoor> list = thisProject.GetSelectedIndoorByRoom(room.Id);
        //            if (list.Count == 0)
        //                continue;

        //            double capSum_c = 0;
        //            double capSum_sh = 0;
        //            double capSum_h = 0;
        //            double capSum_freshair = 0;
        //            foreach (RoomIndoor riItem in list)
        //            {
        //                capSum_c += riItem.CoolingCapacity;
        //                capSum_sh += riItem.SensibleHeat;
        //                capSum_h += riItem.HeatingCapacity;
        //                capSum_freshair += riItem.AirFlow;
        //            }
        //            if (thisProject.IsCoolingModeEffective &&
        //                (capSum_c < room.RqCapacityCool || capSum_sh < room.SensibleHeat))
        //                pass = false;
        //            else if (thisProject.IsHeatingModeEffective && capSum_h < room.RqCapacityHeat)
        //                pass = false;
        //            else if (capSum_freshair < room.FreshAir)
        //                pass = false;

        //            if (!pass && !roomList.Contains(f.Name + ":" + room.Name))
        //            {
        //                roomList.Add(f.Name + ":" + room.Name);
        //            }
        //        }
        //    }
        //    return pass;
        //}

        private void tvRoom_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "";
            bool isEnable = true;
            this.jclblSelectedRoom.Text = Msg.GetResourceString("SelectRoom");
            this.jclblSelectedRoom_2.Text = this.jclblSelectedRoom.Text; //当编辑室内机时，用Room名称代替Room选择界面。 2014-4-18 by lin

            curRoom = null;
            curFreshAirArea = null;

            if (e.Node.Level == 1 && e.Node.Parent.Name == FRESH_AIR_AREA_ROOT_NODE_NAME)
            {
                //如果父节点是新风区域
                this.jclblSelectedRoom.Text = e.Node.Parent.Text + ":" + e.Node.Text;
                this.jclblSelectedRoom_2.Text = this.jclblSelectedRoom.Text; //当编辑室内机时，用Room名称代替Room选择界面。 2014-4-18 by lin
                curFreshAirArea = (FreshAirArea)e.Node.Tag;
                BindRqCapacity();
                isEnable = BindToControl(e.Node);

                //当前区域是新风区域 显示新风量 on20170914 by xyj
                this.dgvStdIndoor.Columns[Name_Common.StdFreshAir].Visible = true;
            }
            // Room
            else if (e.Node.Level == 2)
            {
                this.jclblSelectedRoom.Text = e.Node.Text;
                this.jclblSelectedRoom_2.Text = this.jclblSelectedRoom.Text; //当编辑室内机时，用Room名称代替Room选择界面。 2014-4-18 by lin
                curRoom = (Room)e.Node.Tag;
                if (curRoom != null)
                {
                    pbll = new ProjectBLL(thisProject);
                    List<RoomIndoor> list = pbll.GetSelectedIndoorByRoom(curRoom.Id);

                    if (list.Count > 0)
                        //jccmbProductType.SelectedValue = list[0].IndoorItem.ProductType;
                        jccmbProductType.SelectedValue = list[0].IndoorItem.Series;
                    if (jccmbProductType.SelectedIndex >= 0)
                        //_productType = jccmbProductType.SelectedValue.ToString();
                        _series = jccmbProductType.SelectedValue.ToString();
                    else
                    {
                        if (e.Node.ForeColor == ColorOriginal)
                            jccmbProductType.SelectedIndex = 0;

                    }

                }
                //当前区域不是是新风区域 隐藏新风量 on20170914 by xyj
                this.dgvStdIndoor.Columns[Name_Common.StdFreshAir].Visible = false;
                BindRqCapacity();
                BindToControl(e.Node);
            }
            //BindRqCapacity();
            if (isEnable)
                ResetControlState(this.uc_CheckBox_Auto.Checked);
        }

        private void jccmbType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            UpdateUnitType();
            if (this.jccmbType.Text.Contains("Fresh Air"))
            {
                this.dgvStdIndoor.Columns[Name_Common.StdFreshAir].Visible = true;
            }
            else
            {
                this.dgvStdIndoor.Columns[Name_Common.StdFreshAir].Visible = false;
            }
            BindStdIndoorList();

            if (curSelectType == this.jccmbType.Text)
                //if (curSelectType == this.jccmbType.SelectedValue.ToString() && _productType == jccmbProductType.SelectedValue.ToString())
                return;

            curSelectType = this.jccmbType.Text;
            //curSelectType = this.jccmbType.SelectedValue.ToString();

            // 检验需求信息是否填写
            if (!this.JCValidateGroup(this.pnlRoom_2))
                return;

            if (IsAuto)
            {
                DoAutoSelect();
            }
        }

        private void UpdateUnitType()
        {
            //string type = this.jccmbType.Text.Trim();
            string type = this.jccmbType.SelectedValue.ToString();
            string typeText = this.jccmbType.Text.Trim();
            //需要将type进行处理，获取厂名 20161118 by Yunxiao Lin
            int i = typeText.IndexOf("-");
            if (i > 0)
            {
                _factory = typeText.Substring(i + 1, typeText.Length - i - 1);
                //_type = type.Substring(0, i);
                _type = type;
            }
            else
            {
                _factory = "";
                _type = type;
            }

            SetFreshAirTemperature();

        }

        private void SetFreshAirTemperature()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(frmAddIndoorSimple));
            if (this.jccmbType.Text.Contains("Fresh Air"))
            {   //Set Fresh Air Temp and RH Range
                this.jctxtDBCool.JCMinValue = float.Parse(Unit.ConvertToControl(20, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtDBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(43, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtWBCool.JCMinValue = float.Parse(Unit.ConvertToControl(10.9, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtWBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(32, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtDBHeat.JCMinValue = float.Parse(Unit.ConvertToControl(-5, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtDBHeat.JCMaxValue = float.Parse(Unit.ConvertToControl(15, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtRH.JCMinValue = float.Parse("30");
                this.jctxtRH.JCMaxValue = float.Parse("90");
                //set default values
                if (JCFormMode == FormMode.NEW)
                {
                    this.jctxtDBCool.Text = Unit.ConvertToControl(33, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtWBCool.Text = Unit.ConvertToControl(28, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtDBHeat.Text = Unit.ConvertToControl(0, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtRH.Text = Unit.ConvertToControl(68, UnitType.TEMPERATURE, utTemperature).ToString("n0");
                }
            }
            else
            {    //Set Default Temp and RH Range
                this.jctxtDBCool.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtDBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(30, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
                this.jctxtWBCool.JCMinValue = (float)Unit.ConvertToControl(14, UnitType.TEMPERATURE, utTemperature);
                this.jctxtWBCool.JCMaxValue = (float)Unit.ConvertToControl(24, UnitType.TEMPERATURE, utTemperature);
                this.jctxtDBHeat.JCMinValue = (float)Unit.ConvertToControl(16, UnitType.TEMPERATURE, utTemperature);
                this.jctxtDBHeat.JCMaxValue = (float)Unit.ConvertToControl(24, UnitType.TEMPERATURE, utTemperature);
                this.jctxtRH.JCMinValue = float.Parse("13");
                this.jctxtRH.JCMaxValue = float.Parse("100");
                //set default values
                if (JCFormMode == FormMode.NEW)
                {
                    this.jctxtDBCool.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtWBCool.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtDBHeat.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0");
                }
            }
        }

        private void uc_CheckBox_Auto_CheckedChanged(object sender, EventArgs e)
        {
            uc_CheckBox cbx = sender as uc_CheckBox;
            if (cbx.TextString == uc_CheckBox_Auto.TextString)
            {
                uc_CheckBox_Manual.Checked = !uc_CheckBox_Auto.Checked;
                //新风区域加入是否自动选型
                if (curFreshAirArea != null)
                    curFreshAirArea.IsAuto = true;
            }
            else
            {
                uc_CheckBox_Auto.Checked = !uc_CheckBox_Manual.Checked;
                //新风区域加入是否自动选型
                if (curFreshAirArea != null)
                    curFreshAirArea.IsAuto = false;
            }

            IsAuto = this.uc_CheckBox_Auto.Checked;

            ResetControlState(IsAuto);

            // 检验需求信息是否填写
            if (!this.JCValidateGroup(this.pnlRoom_2))
                return;

            if (IsAuto)
                DoAutoSelect();
            else
            {
                BindSelectedList();
                if (RoomHasIndoor)
                {
                    PassValidation = DoValidateCapacity();
                }
            }
        }

        private void dgvStdIndoor_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || (curRoom == null && curFreshAirArea == null) || IsAuto)
                return;

            if (dgvSelectedIndoor.Rows.Count > 1)
            {
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    if (_productType != r.Cells[Name_Common.ProductType].Value.ToString())
                    {
                        JCMsg.ShowWarningOK(Msg.WARNING_ROOM_PRODUCTTYPE);
                        return;
                    }
                }

            }

            DataGridViewRow stdRow = dgvStdIndoor.Rows[e.RowIndex]; // 新选择的机组

            if (dgvSelectedIndoor.SelectedCells.Count > 0)
            {
                int rIndex = dgvSelectedIndoor.SelectedCells[0].RowIndex;
                DataGridViewRow selRow = dgvSelectedIndoor.Rows[rIndex];

                // 如果新选择的机组与已选的相同，则不执行替换
                //if (stdRow.Cells[Name_Common.StdModelFull].Value.ToString() == selRow.Cells[Name_Common.ModelFull].Value.ToString())
                //光凭ModelName无法确定是不是统一机型，还需要判断productType 20161111 by Yunxiao Lin
                if (stdRow.Cells[Name_Common.StdModelFull].Value.ToString() == selRow.Cells[Name_Common.ModelFull].Value.ToString() && _productType == selRow.Cells[Name_Common.ProductType].Value.ToString())
                    return;

                //DoCalculateEstValue(stdRow);
                updateSelectedRow(stdRow, selRow, false);
                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
            }

        }

        private void dgvStdIndoor_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStdIndoor.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvStdIndoor.SelectedRows[0];
                string imageName = r.Cells[Name_Common.TypeImage].Value.ToString();
                Util.SetTypeImage(this.pbIndoor, imageName);
            }
        }

        private void jcbtnSelectAll_Click(object sender, EventArgs e)
        {
            if (dgvStdIndoor.Rows.Count > 0)
            {
                foreach (DataGridViewRow r in dgvStdIndoor.Rows)
                {
                    //DoCalculateEstValue(r);
                    addToSelectedRow(r, false);
                }

                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
            }
        }

        private void jcbtnSelect_Click(object sender, EventArgs e)
        {
            if (dgvStdIndoor.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    if (_productType != r.Cells[Name_Common.ProductType].Value.ToString())
                    {
                        JCMsg.ShowWarningOK(Msg.WARNING_ROOM_PRODUCTTYPE);
                        return;
                    }
                }
                foreach (DataGridViewRow r in dgvStdIndoor.SelectedRows)
                {
                    //DoCalculateEstValue(r);
                    addToSelectedRow(r, false);
                }

                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
            }
        }

        private void jcbtnIndoorDetails_Click(object sender, EventArgs e)
        {
            if (this.dgvStdIndoor.SelectedRows.Count > 0)
            {
                DataGridViewRow r = this.dgvStdIndoor.SelectedRows[0];
                string modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
                //Indoor inItem = bll.GetItem(modelfull, _type, _productType);
                Indoor inItem = r.Tag as Indoor;
                if (inItem == null) return;

                List<string> info = new List<string>();
                string title = "【" + inItem.ModelFull + "】";

                FieldInfo[] fieldFroms = inItem.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (FieldInfo field in fieldFroms)
                {
                    info.Add(field.Name + ":\t" + field.GetValue(inItem)); // 思考以下如何加上单位表达式？
                }

                frmHelpInfo f = new frmHelpInfo(info, title);
                f.StartPosition = FormStartPosition.CenterScreen;
                f.Show();
            }
        }

        private void pbAddIndoorOption_Click(object sender, EventArgs e)
        {
            // 有点问题，暂时注销，需要参考Main_Indoor界面的同名方法进行修改TODO
            //DoAddOptions();
        }

        private void pbClearIndoorOption_Click(object sender, EventArgs e)
        {
            DoClearOptions();
        }

        private void pbRemoveSelectedIndoor_Click(object sender, EventArgs e)
        {
            bool success;
            DoRemoveSelectedIndoorList(out success);
            if (success)
            {
                if (this.dgvSelectedIndoor.Rows.Count > 0)
                {
                    DoCalculateSelectedSumCapacity();
                    PassValidation = DoValidateCapacity();
                }
                else
                    PassValidation = true;
            }
        }

        private void dgvSelectedIndoor_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            // 如果改变的是 Count 列的数值
            if (dgvSelectedIndoor.CurrentCell.OwningColumn.Name == Name_Common.Count)
            {
                if (e.RowIndex == dgvSelectedIndoor.CurrentCell.RowIndex)
                {
                    DataGridViewCell cell = dgvSelectedIndoor.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (Util.IsNumber(cell.Value.ToString()))
                    {
                        DoCalculateSelectedSumCapacity();
                        PassValidation = DoValidateCapacity();
                    }
                    else
                    {
                        cell.Value = "1";
                    }

                }
            }
        }

        private void jctxtRH_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (JCValidateSingle(jctxtRH) && JCValidateSingle(jctxtDBCool))
            {
                if (jctxtRH.Text.Length > 1)
                {
                    if (NumberUtil.IsNumber(jctxtRH.Text))
                    {
                        if (Convert.ToDouble(jctxtRH.Text) > 100)
                            jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0");

                        if (roomRH == Convert.ToDouble(jctxtRH.Text))
                            return;
                        DoCalculateByOption(UnitTemperature.RH.ToString());
                        BatchCalculateEstValue();
                        if (JCValidateGroup(pnlIndCooling))
                        {
                            BindRoomCooling();
                            //DoCalculateRH();
                            if (!this.JCValidateGroup(this.pnlIndCooling_1) || !this.JCValidateGroup(this.pnlIndHeating_1))
                                return;
                            if (IsAuto)
                            {
                                DoAutoSelect();
                            }
                            else
                            {
                                DoCalculateSelectedSumCapacity();
                                PassValidation = DoValidateCapacity();
                            }
                        }
                    }
                }
            }
        }

        private void BindRoomCooling()
        {
            roomDB = Convert.ToDouble(jctxtDBCool.Text);
            roomWB = Convert.ToDouble(jctxtWBCool.Text);
            roomRH = Convert.ToDouble(jctxtRH.Text);
        }

        private void jctxtDBCool_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (JCValidateSingle(jctxtDBCool) && JCValidateSingle(jctxtRH))
            {
                if (jctxtDBCool.Text.Length > 1)
                {
                    if (NumberUtil.IsNumber(jctxtDBCool.Text))
                    {
                        if (roomDB == Convert.ToDouble(jctxtDBCool.Text))
                            return;
                        DoCalculateByOption(UnitTemperature.DB.ToString());
                        BatchCalculateEstValue();
                        if (JCValidateGroup(pnlIndCooling))
                        {
                            BindRoomCooling();
                            //DoCalculateRH();
                            if (!this.JCValidateGroup(this.pnlIndCooling_1) || !this.JCValidateGroup(this.pnlIndHeating_1))
                                return;
                            if (IsAuto)
                            {
                                DoAutoSelect();
                            }
                            else
                            {
                                DoCalculateSelectedSumCapacity();
                                PassValidation = DoValidateCapacity();
                            }
                        }
                    }
                }
            }
        }

        private void jctxtWBCool_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (JCValidateSingle(jctxtWBCool) && JCValidateSingle(jctxtDBCool))
            {

                if (NumberUtil.IsNumber(jctxtWBCool.Text))
                {
                    if (Convert.ToDouble(jctxtWBCool.Text) > 1)
                    {
                        if (roomWB == Convert.ToDouble(jctxtWBCool.Text))
                            return;
                        DoCalculateByOption(UnitTemperature.WB.ToString());
                        BatchCalculateEstValue();
                        if (JCValidateGroup(pnlIndCooling))
                        {
                            BindRoomCooling();
                            //DoCalculateRH();
                            if (!this.JCValidateGroup(this.pnlIndCooling_1) || !this.JCValidateGroup(this.pnlIndHeating_1))
                                return;
                            if (IsAuto)
                            {
                                DoAutoSelect();
                            }
                            else
                            {
                                DoCalculateSelectedSumCapacity();
                                PassValidation = DoValidateCapacity();
                            }
                        }
                    }
                }
            }
        }

        private void jctxtDBHeat_Leave(object sender, EventArgs e)
        {
            ControllerInputDBHeat(sender as TextBox);
            if (JCValidateGroup(pnlIndHeating))
            {
                if (!this.JCValidateGroup(this.pnlIndCooling_1) || !this.JCValidateGroup(this.pnlIndHeating_1))
                    return;
                BatchCalculateEstValue();
                if (IsAuto)
                {
                    DoAutoSelect();
                }
                else
                {
                    DoCalculateSelectedSumCapacity();
                    PassValidation = DoValidateCapacity();
                }
            }
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (!JCValidateGroup(pnlSelected_1_1))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }
            if (!JCValidateGroup(pnlIndCooling_1))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }
            pbll = new ProjectBLL(thisProject);
            // add on 20140619 clh 当带房间的室内机界面中删除室内机，且已加入到系统中，则该系统的配管图需重绘
            List<string> sysIDList = new List<string>();
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.IsDelete && !String.IsNullOrEmpty(ri.SystemID) && !sysIDList.Contains(ri.SystemID))
                {
                    sysIDList.Add(ri.SystemID);
                    //SystemVRF sysItem = pbll.GetSystem(ri.SystemID);
                    //sysItem.IsUpdated = true; // 配管图重绘的标记
                }
            }

            if (this.tvRoom.SelectedNode == null)
            {
                JCMsg.ShowWarningOK(Msg.INDBYROOM_NO_ROOM);
                return;
            }

            // 若当前房间删除了所有的已选室内机，则不需校验直接从当前项目中删除
            if (this.dgvSelectedIndoor.Rows.Count == 0)
            {
                thisProject.RoomIndoorList.RemoveAll(c => { return c.IsDelete; });
                SetNodeState(this.tvRoom.SelectedNode, false); // add on 20130822 clh
                //删除所有室内机后也需要校验，不然不会更新室内机列表 20160818 by Yunxiao Lin
                //return;
            }

            if (PassValidation)
            {
                string roomID = curRoom != null ? curRoom.Id : "";  //房间ID
                string freshAirAreaID = curRoom == null && curFreshAirArea != null ? curFreshAirArea.Id : "";  //新风区域ID

                #region 更新已选室内机的记录
                string productTypeStr = "";
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    if (!string.IsNullOrEmpty(productTypeStr))
                    {
                        if (productTypeStr != r.Cells[Name_Common.ProductType].Value.ToString())
                        {
                            JCMsg.ShowWarningOK(Msg.WARNING_ROOM_PRODUCTTYPE);
                            return;
                        }
                    }
                    productTypeStr = r.Cells[Name_Common.ProductType].Value.ToString();

                }
                List<RoomIndoor> IndsAdd = new List<RoomIndoor>();
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    int count = Convert.ToInt32(r.Cells[Name_Common.Count].Value);
                    int len = dgvSelectedIndoor.Rows.Count;
                    string modelfull = r.Cells[Name_Common.ModelFull].Value.ToString();
                    string type = r.Cells[Name_Common.Type].Value.ToString();
                    //Indoor inItem = bll.GetItem(modelfull, type, _productType); // 新室内机对象
                    Indoor inItem = r.Tag as Indoor;
                    if (inItem == null) continue;
                    inItem = inItem.DeepClone();   //避免不同的室内机用同一个Indoor对象，引起混乱 by Shen Junjie on 2018/5/11
                    int indexNum = 1;
                    while (count > 0)
                    {
                        string indNOStr = r.Cells[Name_Common.NO].Value == null ? "" : r.Cells[Name_Common.NO].Value.ToString();
                        string indName = r.Cells[Name_Common.Name].Value == null ? "" : r.Cells[Name_Common.Name].Value.ToString();
                        //IndoorName 唯一性  add by axj 20180515 
                        string nameBack = indexNum > 1 && !string.IsNullOrEmpty(indName) ? ("_" + indexNum) : "";
                        if (!string.IsNullOrEmpty(indName) && string.IsNullOrEmpty(indNOStr))
                        {
                            indName = indName + nameBack;
                        }
                        RoomIndoor ri;

                        if (string.IsNullOrEmpty(indNOStr))
                        {

                            // 将NO为空的记录添加到项目的RoomIndoor中
                            ri = pbll.AddIndoor(roomID, inItem);
                            string prefix = SystemSetting.UserSetting.defaultSetting.IndoorName;
                            ri.IndoorName = prefix + ri.IndoorNO.ToString() + nameBack;
                            // add by axj 添加indoorName 唯一性判断
                            foreach (RoomIndoor d in thisProject.RoomIndoorList)
                            {
                                if (indName == d.IndoorName || ri.IndoorName == d.IndoorName)
                                {
                                    if (ri.IndoorNO != d.IndoorNO)
                                    {
                                        thisProject.RoomIndoorList.Remove(ri);
                                        if (IndsAdd.Count > 0)
                                        {
                                            IndsAdd.ForEach((p) => { thisProject.RoomIndoorList.Remove(p); });
                                        }
                                        JCMsg.ShowWarningOK(Msg.WARNING_IND_NAME_EXIST(d.IndoorName));
                                        return;
                                    }
                                }
                            }
                            ri.RoomName = "";
                            if (!string.IsNullOrEmpty(roomID))
                            {
                                Room room = pbll.GetRoom(roomID);
                                if (room != null)
                                {
                                    ri.RoomName = room.Name; //GetRoomNOString 
                                }
                            }
                            else if (!string.IsNullOrEmpty(freshAirAreaID))
                            {
                                ri.RoomID = freshAirAreaID;
                                FreshAirArea freshAirArea = pbll.GetFreshAirArea(freshAirAreaID);
                                if (freshAirArea != null)
                                {
                                    ri.RoomName = freshAirArea.Name;
                                }
                            }
                        }
                        else
                        {
                            // NO不为空的记录则仅更新 IndoorItem
                            int indNO = Convert.ToInt32(indNOStr);
                            ri = pbll.GetIndoor(indNO);
                            ri.SetIndoorItemWithAccessory(inItem);
                            // add by axj 添加indoorName 唯一性判断
                            foreach (RoomIndoor d in thisProject.RoomIndoorList)
                            {
                                if (indName == d.IndoorName || ri.IndoorName == d.IndoorName)
                                {
                                    if (ri.IndoorNO != d.IndoorNO)
                                    {
                                        if (IndsAdd.Count > 0)
                                        {
                                            IndsAdd.ForEach((p) => { thisProject.RoomIndoorList.Remove(p); });
                                        }
                                        JCMsg.ShowWarningOK(Msg.WARNING_IND_NAME_EXIST(d.IndoorName));
                                        return;
                                    }
                                }
                            }
                        }
                        ri.IsFreshAirArea = false;
                        if (curFreshAirArea != null && curRoom == null)
                            ri.IsFreshAirArea = true;

                        if (!string.IsNullOrEmpty(indName)) // 该Name可以手动修改
                            ri.IndoorName = indName;

                        ri.CoolingCapacity = Convert.ToDouble(r.Cells[Name_Common.Capacity_C].Value);
                        ri.HeatingCapacity = Convert.ToDouble(r.Cells[Name_Common.Capacity_H].Value);
                        ri.SensibleHeat = Convert.ToDouble(r.Cells[Name_Common.SensibleHeat].Value);
                        //ri.AirFlow = Convert.ToDouble(r.Cells[Name_Common.AirFlow].Value);
                        ri.IsAuto = this.uc_CheckBox_Auto.Checked;
                        ri.IsDelete = false;
                        //记录显热档位 20161112 by Yunxiao lin
                        //ri.SHF_Mode = _shf_mode;
                        ri.FanSpeedLevel = _fanSpeedLevel;

                        ri.DBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                        ri.WBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
                        ri.DBHeating = Unit.ConvertToSource(Convert.ToDouble(jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
                        ri.RHCooling = Convert.ToDouble(jctxtRH.Text);

                        // 自动选型（一次选择一台室内机）时需记录当前的容量需求数值
                        //ri.RqCoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomCapC.Text), UnitType.POWER, utPower);
                        //ri.RqSensibleHeat = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomSensiCapC.Text), UnitType.POWER, utPower);
                        //ri.RqHeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomCapH.Text), UnitType.POWER, utPower);
                        //ri.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomAirflow.Text), UnitType.AIRFLOW, utAirflow);
                        //ri.RqStaticPressure = Convert.ToDouble(jctxtRoomStaticPressure.Text);
                        //ri.RqFreshAir = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomFA.Text), UnitType.AIRFLOW, utAirflow);
                        if (len == 1)   //暂时逻辑，自动选型及手动只选一条记录维护需求容量   modify on 20170711 by Lingjia Qiu
                        {
                            ri.RqCoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomCapC.Text), UnitType.POWER, utPower);
                            ri.RqSensibleHeat = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomSensiCapC.Text), UnitType.POWER, utPower);
                            ri.RqHeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomCapH.Text), UnitType.POWER, utPower);
                            ri.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomAirflow.Text), UnitType.AIRFLOW, utAirflow);
                            //ri.RqStaticPressure = Convert.ToDouble(jctxtRoomStaticPressure.Text);
                            ri.RqStaticPressure = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomStaticPressure.Text), UnitType.STATICPRESSURE, utPressure);
                            ri.RqFreshAir = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomFA.Text), UnitType.AIRFLOW, utAirflow);
                        }
                        else
                        {
                            ri.RqCoolingCapacity = 0d;
                            ri.RqSensibleHeat = 0d;
                            ri.RqHeatingCapacity = 0d;
                            ri.RqAirflow = 0d;
                            ri.RqStaticPressure = 0d;
                            ri.RqFreshAir = 0d;

                        }
                        if (string.IsNullOrEmpty(indNOStr))
                        {
                            IndsAdd.Add(ri);
                        }
                        indexNum++;
                        --count;
                    }
                }
                #endregion

                // 将预删除的 RoomIndoor 记录从项目对象中正式删除
                thisProject.RoomIndoorList.RemoveAll(c => { return c.IsDelete; });
                //如果剩余新风机型号“JTAF1080”单独选型找不到适合的室外机，解绑并提示   add on 20160928 by Lingjia Qiu
                //if (thisProject.RoomIndoorList.Count > 0)
                //{
                //    if (thisProject.RoomIndoorList[0].SystemID != null && !thisProject.RoomIndoorList[0].SystemID.Equals(""))
                //    {
                //        if (thisProject.RoomIndoorList.Count == 1 && thisProject.RoomIndoorList[0].IndoorItem.Model.Equals("JTAF1080"))
                //        {
                //            thisProject.RoomIndoorList[0].SetToSystemVRF(null);
                //            JCMsg.ShowInfoOK(Msg.IND_DEL_FRESHAIR_HINT);
                //        }
                //    }
                //}

                if (this.dgvSelectedIndoor.Rows.Count > 0)
                    SetNodeState(this.tvRoom.SelectedNode, true); // add on 20130822 clh
                //增加绑定Room信息
                BindSelectedList();
                SystemSetting.UserSetting.defaultSetting.IsRoomIndoorAuto = uc_CheckBox_Auto.Checked;
                SystemSetting.Serialize();
                if (this.JCFormMode == FormMode.EDIT)
                    DialogResult = DialogResult.OK;
                UndoRedoUtil.SaveProjectHistory();
            }
            else
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
            }
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Response codes (Methods && Events)

        // 初始化界面 DGV 控件的列标题
        /// <summary>
        /// 初始化界面 DGV 控件的列标题
        /// </summary>
        private void InitDGV()
        {
            this.dgvStdIndoor.AutoGenerateColumns = false;
            this.dgvSelectedIndoor.AutoGenerateColumns = false;

            NameArray_Indoor Arr_Indoor = new NameArray_Indoor();
            Global.SetDGVDataName(ref dgvStdIndoor, Arr_Indoor.StdIndoor_DataName);
            Global.SetDGVName(ref dgvStdIndoor, Arr_Indoor.StdIndoor_Name);
            Global.SetDGVHeaderText(ref dgvStdIndoor, Arr_Indoor.StdIndoor_HeaderText);


            Global.SetDGVDataName(ref dgvSelectedIndoor, Arr_Indoor.SelIndoor_DataName);
            Global.SetDGVHeaderText(ref dgvSelectedIndoor, Arr_Indoor.SelIndoor_HeaderText);
            Global.SetDGVName(ref dgvSelectedIndoor, Arr_Indoor.SelIndoor_Name);


            this.dgvStdIndoor.Columns[Name_Common.StdModelFull].Visible = false;
            if (thisProject.BrandCode == "H")
            {
                this.dgvStdIndoor.Columns[Name_Common.StdModelFull_York].Visible = false;
                this.dgvStdIndoor.Columns[Name_Common.StdModelFull_Hitachi].Visible = true;
                this.dgvSelectedIndoor.Columns[Name_Common.ModelFull_York].Visible = false;
                this.dgvSelectedIndoor.Columns[Name_Common.ModelFull_Hitachi].Visible = true;
            }
            else
            {
                this.dgvStdIndoor.Columns[Name_Common.StdModelFull_York].Visible = true;
                this.dgvStdIndoor.Columns[Name_Common.StdModelFull_Hitachi].Visible = false;
                this.dgvSelectedIndoor.Columns[Name_Common.ModelFull_York].Visible = true;
                this.dgvSelectedIndoor.Columns[Name_Common.ModelFull_Hitachi].Visible = false;
            }
        }

        // 绑定单位表达式
        /// <summary>
        /// 绑定单位表达式
        /// </summary>
        private void BindUnit()
        {
            utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            //utPressure = Unit.ut_Pressure;
            utPressure = SystemSetting.UserSetting.unitsSetting.settingESP;
            jclblUnitFA1.Text = utAirflow;
            jclblUnitFA2.Text = utAirflow;
            jclblUnitAirflow1.Text = utAirflow;
            jclblUnitAirflow2.Text = utAirflow;
            jclblUnitStaticPressure1.Text = utPressure;
            jclblUnitStaticPressure2.Text = utPressure;
            jclblUnitkW1.Text = utPower;
            jclblUnitkW2.Text = utPower;
            jclblUnitkW3.Text = utPower;
            jclblUnitkW4.Text = utPower;
            jclblUnitkW5.Text = utPower;
            jclblUnitkW6.Text = utPower;
            jclblUnitTemperature1.Text = utTemperature;
            jclblUnitTemperature2.Text = utTemperature;
            jclblUnitTemperature3.Text = utTemperature;
        }

        // 检测是否已经包含当前楼层所属的父节点
        /// <summary>
        /// 检测是否已经包含当前楼层所属的父节点
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        private bool hasParentNode(int pId)
        {
            foreach (TreeNode tn in tvRoom.Nodes)
            {
                if (Convert.ToInt32(tn.Text) == pId)
                    return true;
            }
            return false;
        }

        //// 绑定产品类别下拉框
        ///// <summary>
        ///// 绑定产品类别下拉框
        ///// </summary>
        //private void BindIndoorTypeList(bool isFirst)
        //{
        //    //if (thisProject == null || string.IsNullOrEmpty(thisProject.SubRegionCode) || string.IsNullOrEmpty(thisProject.ProductType))
        //    //添加多ProductType功能后，ThisProject的ProductType不能再作参数 20160821 by Yunxiao Lin
        //    if (thisProject == null || string.IsNullOrEmpty(thisProject.SubRegionCode) || string.IsNullOrEmpty(_productType)) 
        //        return;
        //    string colName = "";
        //    DataTable dt = bll.GetIndoorTypeList(out colName, _productType);
        //    DataView dv = new DataView(dt);
        //    if (curRoom != null)
        //        dv.RowFilter = "UnitType not in ('YDCF', 'Fresh Air', 'Ventilation')"; //房间不能选新风机，只有新风区域才能选新风机。 add on 20160929 by Lingjia Qiu
        //    else if(curFreshAirArea != null)
        //        dv.RowFilter = "UnitType  in ('YDCF', 'Fresh Air', 'Ventilation')"; //房间不能选新风机，只有新风区域才能选新风机。 add on 20160929 by Lingjia Qiu
        //    this.jccmbType.DisplayMember = colName;
        //    this.jccmbType.DataSource = dv;

        //    if (isFirst && this.JCFormMode == FormMode.EDIT)
        //        this.jccmbType.Text = curRI.IndoorItem.Type;

        //    BindStdIndoorList();
        //}
        // 绑定产品类别,如果该类别不只一个生产厂，在Indoor Unit Type 之后加上厂名 20161118
        /// <summary>
        /// 绑定产品类别
        /// </summary>
        private void BindIndoorTypeList(bool isFirst)
        {
            if (thisProject == null || string.IsNullOrEmpty(thisProject.SubRegionCode) || string.IsNullOrEmpty(_productType))
                return;
            string colName = "UnitType";
            string displayColName = "Trans_Name";
            DataTable dt = bll.GetIndoorFacCodeList(_productType);
            dt = trans.getTypeTransDt(TransType.Indoor.ToString(), dt, colName);
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToInt32(dr["FactoryCount"].ToString()) > 1)
                {
                    switch (dr["FactoryCode"].ToString())
                    {
                        case "G":
                            dr[displayColName] += "-GZF";
                            break;
                        case "E":
                            dr[displayColName] += "-HAPE";
                            break;
                        case "Q":
                            dr[displayColName] += "-HAPQ";
                            break;
                        case "B":
                            dr[displayColName] += "-HAPB";
                            break;
                        case "I":
                            dr[displayColName] += "-HHLI";
                            break;
                        case "M":
                            dr[displayColName] += "-HAPM";
                            break;
                        case "S":
                            dr[displayColName] += "-SMZ";
                            break;
                    }
                }
            }

            DataView dv = new DataView(dt);
            string strFilter = "";
            if (curRoom != null)
            {
                //暂时开放巴西区域，可以直接选择FreshAir on 20190107 by xyj
                if (thisProject.SubRegionCode != "LA_BR")
                {
                    strFilter = "UnitType not in ('YDCF', 'Fresh Air', 'Ventilation')"; //房间不能选新风机，只有新风区域才能选新风机。 add on 20160929 by Lingjia Qiu
                }
                else
                {
                    strFilter = "UnitType not in ('YDCF', 'Ventilation')"; //房间能选新风机  add on 20180104 by xyj
                }
            }
            else if (curFreshAirArea != null)
            {
                //暂时开放巴西区域，可以直接选择FreshAir on 20190107 by xyj
                if (thisProject.SubRegionCode != "LA_BR")
                {
                    strFilter = "UnitType  in ('YDCF', 'Fresh Air', 'Ventilation')"; //房间不能选新风机，只有新风区域才能选新风机。 add on 20160929 by Lingjia Qiu 
                }
                else
                {
                    strFilter = "UnitType  in ('YDCF','Ventilation') or SelectionType='FreshAir'"; //房间能选新风机  add on 20180104 by xyj
                }
            }
            if (_productType == "Comm. Tier 2, HP")
            {
                //Comm. Tier 2, HP 有3个series, FSN6Q, FSXN, FSXN1 能够选择的室内机系列是不同的 20161201 by Yunxiao Lin
                if (_series == "Commercial VRF HP, FSN6Q" || _series == "Commercial VRF HP, JVOH-Q")
                {
                    //FSN6Q, JVOH不能使用HAPE High Static Ducted / Medium Static Ducted / Low Static Ducted
                    //FSN6Q, JVOH不能使用SMZ High Static Ducted / Medium Static Ducted / Four Way Cassette
                    if (!string.IsNullOrEmpty(strFilter))
                        strFilter += "and ";
                    strFilter += "UnitType not in ('High Static Ducted-HAPE','Medium Static Ducted-HAPE','Low Static Ducted-HAPE','High Static Ducted-SMZ','Medium Static Ducted-SMZ','Four Way Cassette-SMZ')";
                }
                else
                {
                    //FSXN, FSXN1不能使用HAPQ Four Way Cassette
                    if (!string.IsNullOrEmpty(strFilter))
                        strFilter += "and ";
                    //strFilter += "UnitType <>'Four Way Cassette-HAPQ'";
                    strFilter += "UnitType <>'Four Way Cassette-HAPQ' and UnitType <>'Four Way Cassette (FSN1Q)'"; //RCI FSN1Q的Unit Type改为Four Way Cassette (FSN1Q). 20190122 by Yunxiao Lin
                }
            }
            dv.RowFilter = strFilter;
            //this.jccmbType.DisplayMember = colName;
            this.jccmbType.DisplayMember = displayColName;
            this.jccmbType.ValueMember = colName;
            this.jccmbType.DataSource = dv;

            UpdateUnitType();

            if (isFirst && this.JCFormMode == FormMode.EDIT)
            {
                BindUnitTypeListText(curRI.IndoorItem);

            }
        }
        /// 绑定UnitTypeList的Text值 20161118 by Yunxiao lin
        /// <summary>
        /// 绑定UnitTypeList的Text值
        /// </summary>
        /// <param name="selectText"></param>
        private void BindUnitTypeListText(Indoor IndoorItem)
        {
            curSelectType = trans.getTypeTransStr(TransType.Indoor.ToString(), IndoorItem.Type);
            if (jccmbType.FindStringExact(curSelectType, -1) < 0)
            {
                //增加判断室内机的生产厂
                string model = IndoorItem.ModelFull.Trim();
                string factorCode = model.Substring(model.Length - 1, 1);
                string tempSelectType = curSelectType;
                switch (factorCode)
                {
                    case "G":
                        curSelectType += "-GZF";
                        break;
                    case "E":
                        curSelectType += "-HAPE";
                        break;
                    case "Q":
                        curSelectType += "-HAPQ";
                        break;
                    case "B":
                        curSelectType += "-HAPB";
                        break;
                    case "I":
                        curSelectType += "-HHLI";
                        break;
                    case "M":
                        curSelectType += "-HAPM";
                        break;
                    case "S":
                        curSelectType += "-SMZ";
                        break;
                }
                if (jccmbType.FindStringExact(curSelectType, -1) < 0)
                    curSelectType = tempSelectType;
            }

            this.jccmbType.Text = curSelectType;  //带厂过滤的室内机type比较特殊，因数据库缺乏标示厂字段，故暂时以显示参数作为过滤源
            //this.jccmbType.SelectedValue = curSelectType;
            UpdateUnitType();
        }
        /// 绑定ProductType列表
        /// <summary>
        /// 绑定ProductType列表
        /// </summary>
        private void BindCmbProductType()
        {
            _productType = "";
            string colName = "Series";
            string brandCode = Registr.Registration.SelectedBrand.Code;
            string regionCode = Registr.Registration.SelectedSubRegion.Code;

            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            //DataTable dt = productTypeBll.GetProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
            DataTable dt = productTypeBll.GetProductTypeData(brandCode, regionCode);
            dt = trans.getTypeTransDt(TransType.Series.ToString(), dt, colName);
            if (dt != null && dt.Rows.Count > 0)
            {
                //this.jccmbProductType.ValueMember = "ProductType";
                this.jccmbProductType.ValueMember = colName;
                //this.jccmbProductType.DisplayMember = "Series";
                this.jccmbProductType.DisplayMember = "Trans_Name";
                this.jccmbProductType.DataSource = dt;
                if (this.JCFormMode == FormMode.EDIT)
                {
                    //this.jccmbProductType.SelectedValue = curRI.IndoorItem.ProductType;
                    this.jccmbProductType.SelectedValue = curRI.IndoorItem.Series;
                    this.jccmbProductType.Enabled = false;
                }
                else
                    this.jccmbProductType.SelectedIndex = 0;
                _series = jccmbProductType.SelectedValue.ToString();
                //_series = productTypeBll.GetSeriesByProductType(brandCode, regionCode, _productType);
                //_productType = jccmbProductType.SelectedValue.ToString();
                _productType = productTypeBll.GetProductTypeBySeries(brandCode, regionCode, _series);
            }
        }

        // 绑定房间树
        /// <summary>
        /// 绑定房间树
        /// </summary>
        private void BindTreeViewRoom()
        {
            this.tvRoom.Nodes.Clear();
            foreach (Floor f in thisProject.FloorList)
            {
                if (!hasParentNode(f.ParentId))
                {
                    tvRoom.Nodes.Add(f.ParentId.ToString());
                    tvRoom.Nodes[0].Expand();
                }
                foreach (TreeNode tn in tvRoom.Nodes)
                {
                    if (tn.Text == f.ParentId.ToString())
                    {
                        TreeNode nodeFloor = new TreeNode();
                        nodeFloor.Tag = f;
                        nodeFloor.Text = f.Name;
                        tn.Nodes.Add(nodeFloor);
                        foreach (Room r in f.RoomList)
                        {
                            TreeNode nodeRoom = new TreeNode();
                            nodeRoom.Tag = r;
                            //string roomNO = bll.GetRoomNOString(r.NO, f);
                            //nodeRoom.Text = roomNO + ":" + r.Name;
                            nodeRoom.Text = r.Name; // 20130709会议 Sam意见

                            nodeFloor.Nodes.Add(nodeRoom);
                            pbll = new ProjectBLL(thisProject);
                            List<RoomIndoor> list = pbll.GetSelectedIndoorByRoom(r.Id);
                            if (list.Count > 0)
                                SetNodeState(nodeRoom, true);
                            else
                                SetNodeState(nodeRoom, false);

                            if (curRoom != null && curRoom == r)
                            {
                                tvRoom.SelectedNode = nodeRoom;
                            }
                        }
                    }
                }
            }

            //添加新风区域的节点
            if (thisProject.FreshAirAreaList.Count > 0)
            {
                TreeNode nodeFreshAirAreaRoot = new TreeNode("Fresh Air Area");
                nodeFreshAirAreaRoot.Name = FRESH_AIR_AREA_ROOT_NODE_NAME;
                tvRoom.Nodes.Add(nodeFreshAirAreaRoot);
                foreach (FreshAirArea area in thisProject.FreshAirAreaList)
                {
                    TreeNode nodeFreshAirArea = new TreeNode();
                    nodeFreshAirArea.Tag = area;
                    nodeFreshAirArea.Text = area.Name;
                    nodeFreshAirAreaRoot.Nodes.Add(nodeFreshAirArea);

                    pbll = new ProjectBLL(thisProject);
                    List<RoomIndoor> list = pbll.GetSelectedIndoorByRoom(area.Id);
                    SetNodeState(nodeFreshAirArea, list.Count > 0);

                    if (curFreshAirArea != null && curFreshAirArea == area)
                    {
                        tvRoom.SelectedNode = nodeFreshAirArea;
                    }
                }
            }
            BindRqCapacity();
        }

        private void SetNodeState(TreeNode nodeRoom, bool isHighlight)
        {

            if (isHighlight)
                nodeRoom.ForeColor = ColorAssigned;
            else
                nodeRoom.ForeColor = ColorOriginal;
            //nodeRoom.NodeFont = Global.SetFont(isHighlight);
            ////如何加大节点宽度。。。
            nodeRoom.NodeFont = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        }

        // 绑定标准表
        /// <summary>
        /// 绑定标准表
        /// </summary>
        private void BindStdIndoorList()
        {
            dgvStdIndoor.Rows.Clear();
            DataTable dt = bll.GetIndoorListStd(_type, _productType, _factory);
            dt.DefaultView.Sort = "CoolCapacity";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Indoor inItem = bll.GetItem(dr["ModelFull"].ToString(), _type, _productType, _series);
                    if (inItem == null) continue;

                    inItem.Series = _series;   //将当前的series封装室内机列表
                    double est_cool = Convert.ToDouble(dr["CoolCapacity"].ToString());
                    double est_heat = Convert.ToDouble(dr["HeatCapacity"].ToString());
                    double airflow = 0;
                    double staticPressure = 0;
                    double shf = 0;
                    double fa = 0;
                    if (inItem != null)
                    {
                        shf = inItem.GetSHF(-1);
                        if (IndoorBLL.IsFreshAirUnit(_type))  // 新风机计算新风风量合计
                        {
                            fa = inItem.AirFlow;
                        }
                        else
                        {
                            airflow = inItem.GetAirFlow(-1);
                            //staticPressure = inItem.GetStaticPressure(-1);
                            staticPressure = inItem.GetStaticPressure();
                        }
                    }
                    double est_sh = est_cool * shf;
                    //绑定数据 
                    this.dgvStdIndoor.Rows.Add(dr["ModelFull"].ToString(),
                         dr["Model_York"].ToString(),
                         dr["Model_Hitachi"].ToString(),
                         Unit.ConvertToControl(est_cool, UnitType.POWER, utPower).ToString("n1"),
                         Unit.ConvertToControl(est_sh, UnitType.POWER, utPower).ToString("n1"),
                         Unit.ConvertToControl(airflow, UnitType.AIRFLOW, utAirflow).ToString("n0"),
                         //staticPressure.ToString("n0"),
                         Unit.ConvertToControl(staticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2"),
                         Unit.ConvertToControl(est_heat, UnitType.POWER, utPower).ToString("n1"),
                         Unit.ConvertToControl(fa, UnitType.AIRFLOW, utAirflow).ToString("n0"),
                         dr["TypeImage"].ToString()
                        );

                    DataGridViewRow newStdRow = dgvStdIndoor.Rows[dgvStdIndoor.Rows.Count - 1];
                    newStdRow.Tag = inItem;
                }
                BatchCalculateEstValue();
                BatchCalculateAirFlow();
            }

            this.dgvStdIndoor.Columns[Name_Common.StdModelFull].Visible = false;
            if (this.dgvStdIndoor.Rows.Count > 0)
                this.dgvStdIndoor.Rows[0].Selected = true;
        }

        // 绑定初始环境温度
        /// <summary>
        /// 绑定初始环境温度
        /// </summary>
        private void BindDBWB()
        {
            this.jctxtDBHeat.Leave += new EventHandler(jctxtDBHeat_Leave);
            this.jctxtDBCool.Leave += new EventHandler(jctxtDBCool_Leave);
            this.jctxtWBCool.Leave += new EventHandler(jctxtWBCool_Leave);
            this.jctxtRH.Leave     += new EventHandler(jctxtRH_Leave);

            // add on 20130820 clh
            this.jctxtDBCool.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(30, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtWBCool.JCMinValue = (float)Unit.ConvertToControl(14, UnitType.TEMPERATURE, utTemperature);//11
            this.jctxtWBCool.JCMaxValue = (float)Unit.ConvertToControl(24, UnitType.TEMPERATURE, utTemperature);
            this.jctxtDBHeat.JCMinValue = (float)Unit.ConvertToControl(16, UnitType.TEMPERATURE, utTemperature);
            this.jctxtDBHeat.JCMaxValue = (float)Unit.ConvertToControl(24, UnitType.TEMPERATURE, utTemperature);//30
            this.jctxtRH.JCMinValue = float.Parse("13");
            this.jctxtRH.JCMaxValue = float.Parse("100");
            // end on 20130820

            if (JCFormMode == FormMode.NEW)
            {
                double dbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB;
                double wbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
                double dbHeat = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
                double rhCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH;
                this.jctxtDBCool.Text = Unit.ConvertToControl(dbCool, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtWBCool.Text = Unit.ConvertToControl(wbCool, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtDBHeat.Text = Unit.ConvertToControl(dbHeat, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtRH.Text = rhCool.ToString("n0");
                if (jccmbType.SelectedValue.ToString() == "Fresh Air")
                {
                    this.jctxtDBCool.Text = Unit.ConvertToControl(33, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtWBCool.Text = Unit.ConvertToControl(28, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtDBHeat.Text = Unit.ConvertToControl(0, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    this.jctxtRH.Text = Unit.ConvertToControl(68, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                }
                curRI.DBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                curRI.WBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
                curRI.WBHeating = Unit.ConvertToSource(Convert.ToDouble(jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
                curRI.RHCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtRH.Text), UnitType.TEMPERATURE, utTemperature);
            }
            else if (JCFormMode == FormMode.EDIT)
            {
                this.jctxtDBCool.Text = Unit.ConvertToControl(curRI.DBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtWBCool.Text = Unit.ConvertToControl(curRI.WBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtDBHeat.Text = Unit.ConvertToControl(curRI.DBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                if (curRI.RHCooling != 0)
                    this.jctxtRH.Text = curRI.RHCooling.ToString("n0");
                else
                    DoCalculateRH();
            }
            BindRoomCooling();
            //  DoCalculateRH(); 
        }

        // 重置控件状态
        /// <summary>
        /// 重置控件状态
        /// </summary>
        private void ResetControlState(bool isAuto)
        {
            bool editableItemSelected = (curRoom != null || curFreshAirArea != null);

            // this.pbRemoveSelectedIndoor.Visible = !isAuto;
            if (!editableItemSelected)
            {
                this.dgvSelectedIndoor.Rows.Clear();
            }
            this.pnlManualButton.Enabled = !isAuto && editableItemSelected;
            this.pnlSelected_1_2_1.Enabled = !isAuto && editableItemSelected;
            this.pnlAuto.Enabled = editableItemSelected;

            this.jcbtnOK.Enabled = editableItemSelected;
            this.jccmbType.Enabled = editableItemSelected;
            this.jccmbProductType.Enabled = editableItemSelected;
        }

        // 绑定选中房间节点的信息,若选中非房间节点，则置空
        /// <summary>
        /// 绑定选中房间节点的信息,若选中非房间节点，则置空
        /// </summary>
        /// <param name="isRoomNode"></param>
        private void BindRqCapacity()
        {
            double rq_cool = 0;
            double rq_heat = 0;
            double rq_sensiable = 0;
            double rq_airflow = 0;
            double rq_StaticPressure = 0;
            double rq_fa = 0;

            if (curRoom != null)
            {
                rq_cool = curRoom.RqCapacityCool;
                rq_heat = curRoom.RqCapacityHeat;
                rq_sensiable = curRoom.SensibleHeat;
                rq_airflow = curRoom.AirFlow;
                rq_StaticPressure = curRoom.StaticPressure;
                rq_fa = curRoom.FreshAir;
            }
            else if (curFreshAirArea != null)
            {
                foreach (Floor f in thisProject.FloorList)
                {
                    foreach (Room r in f.RoomList)
                    {
                        if (r.FreshAirAreaId == curFreshAirArea.Id)
                        {
                            rq_cool += r.RqCapacityCool;
                            rq_heat += r.RqCapacityHeat;
                            rq_sensiable += r.SensibleHeat;
                            rq_airflow += r.AirFlow;
                            rq_StaticPressure += r.StaticPressure;
                            rq_fa += r.FreshAir;
                        }
                    }
                }
                curFreshAirArea.RqCapacityCool = rq_cool;
                curFreshAirArea.RqCapacityHeat = rq_heat;
                curFreshAirArea.SensibleHeat = rq_sensiable;
                curFreshAirArea.AirFlow = rq_airflow;
                curFreshAirArea.StaticPressure = rq_StaticPressure;
                curFreshAirArea.FreshAir = rq_fa;
            }

            this.jctxtRoomCapC.Text = rq_cool == 0 ? "0" : Unit.ConvertToControl(rq_cool, UnitType.POWER, utPower).ToString("n1");
            this.jctxtRoomCapH.Text = rq_heat == 0 ? "0" : Unit.ConvertToControl(rq_heat, UnitType.POWER, utPower).ToString("n1");
            this.jctxtRoomSensiCapC.Text = rq_sensiable == 0 ? "0" : Unit.ConvertToControl(rq_sensiable, UnitType.POWER, utPower).ToString("n1");
            this.jctxtRoomAirflow.Text = Unit.ConvertToControl(rq_airflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //this.jctxtRoomStaticPressure.Text = rq_StaticPressure.ToString("n0");
            this.jctxtRoomStaticPressure.Text = Unit.ConvertToControl(rq_StaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
            this.jctxtRoomFA.Text = Unit.ConvertToControl(rq_fa, UnitType.AIRFLOW, utAirflow).ToString("n0");
        }

        // 绑定指定房间的已选室内机记录
        /// <summary>
        /// 绑定指定房间的已选室内机记录
        /// </summary>
        /// <param name="room"></param>
        private void BindSelectedList()
        {
            string roomId;
            if (curRoom != null)
            {
                roomId = curRoom.Id;
            }
            else if (curFreshAirArea != null)
            {
                roomId = curFreshAirArea.Id;
            }
            else
            {
                return;
            }

            pbll = new ProjectBLL(thisProject);
            this.dgvSelectedIndoor.Rows.Clear();
            List<RoomIndoor> list = pbll.GetSelectedIndoorByRoom(roomId);
            //if (list.Count > 0)
            //    jccmbProductType.SelectedValue = list[0].IndoorItem.ProductType;
            //if(jccmbProductType.SelectedIndex != 0)
            //    _productType = jccmbProductType.SelectedValue.ToString();
            foreach (RoomIndoor ri in list)
            {
                ri.IsDelete = false; // 恢复中途的删除标记

                //string modelAfterOption = ri.OptionItem == null ? ri.IndoorItem.ModelFull : ri.OptionItem.GetNewModelWithOptionB();

                this.dgvSelectedIndoor.Rows.Add(
                    ri.IndoorNO,
                    ri.IndoorName,
                    ri.IndoorItem.ModelFull,
                    ri.IndoorItem.Model_York,
                    ri.IndoorItem.Model_Hitachi,
                    ri.IndoorItem.ModelFull, //modelAfterOption
                    "1",
                    ri.IndoorItem.Type,
                    ri.CoolingCapacity,
                    ri.HeatingCapacity,
                    ri.SensibleHeat,
                    //ri.AirFlow,
                    ri.IndoorItem.ProductType);
                //ri.IndoorItem.SHF_Hi,
                //ri.IndoorItem.SHF_Med,
                //ri.IndoorItem.SHF_Lo);
                DataGridViewRow newSelRow = dgvSelectedIndoor.Rows[dgvSelectedIndoor.Rows.Count - 1];
                newSelRow.Tag = ri.IndoorItem;
            }
            this.dgvSelectedIndoor.Columns[Name_Common.Count].ReadOnly = true;
        }

        // 窗口加载时控件初始值绑定
        /// <summary>
        /// 窗口加载时控件初始值绑定
        /// </summary>
        private Boolean BindToControl(TreeNode roomNode)
        {
            //string originalType = this.jccmbType.Text;
            string originalType = this.jccmbType.SelectedValue.ToString();
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            string brandCode = Registr.Registration.SelectedBrand.Code;
            string regionCode = Registr.Registration.SelectedSubRegion.Code;
            string colName = "Series";
            //if (curRoom != null)
            //{
            //    DataView dv = this.jccmbType.DataSource as DataView;
            //    dv.RowFilter = "UnitType not in ('YDCF', 'Fresh Air', 'Ventilation')"; //房间不能选新风机，只有新风区域才能选新风机。 add on 20160929 by Lingjia Qiu
            //    //if (!string.IsNullOrEmpty(dv.RowFilter))
            //    //{
            //    //    dv.RowFilter = "";
            //    //}
            //}
            //else if (curFreshAirArea != null)
            //{
            //    DataView dv = this.jccmbType.DataSource as DataView;
            //    dv.RowFilter = "UnitType in ('YDCF', 'Fresh Air', 'Ventilation')";
            //}
            # region  基于房间的室内机选型多个product切换：普通房间不能选新风机；新风区域只能选新风机 modify on 20161009 by Lingjia Qiu
            if (curRoom != null)
            {
                //DataTable dt = productTypeBll.GetProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
                DataTable dt = productTypeBll.GetProductTypeData(brandCode, regionCode);
                dt = trans.getTypeTransDt(TransType.Series.ToString(), dt, colName);
                if (dt != null && dt.Rows.Count > 0)
                {
                    this.jccmbProductType.DataSource = dt;
                    //this.jccmbProductType.ValueMember = "ProductType";                    
                    this.jccmbProductType.ValueMember = colName;
                    //this.jccmbProductType.DisplayMember = "Series";
                    this.jccmbProductType.DisplayMember = "Trans_Name";
                    //_productType = jccmbProductType.SelectedValue.ToString();
                    //_series = jccmbProductType.Text;
                }
                DataView dv = this.jccmbType.DataSource as DataView;
                if (thisProject.SubRegionCode != "LA_BR")
                {
                    dv.RowFilter = "UnitType not in ('YDCF', 'Fresh Air', 'Ventilation')"; //房间不能选新风机，只有新风区域才能选新风机。 add on 20160929 by Lingjia Qiu
                }
                else
                {
                    dv.RowFilter = "UnitType not in ('YDCF', 'Ventilation')"; //房间可以选择新风机 add on 20190104 by xyj
                }
            }
            else if (curFreshAirArea != null)
            {
                //List<MyProductType> productTypeStrList = productTypeBll.GetFreshAirProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
                DataTable dt = productTypeBll.GetFreshAirProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
                dt = trans.getTypeTransDt(TransType.Series.ToString(), dt, colName);
                if (dt != null && dt.Rows.Count > 0)
                {
                    this.jccmbProductType.DataSource = dt;
                    //this.jccmbProductType.ValueMember = "ProductType";
                    this.jccmbProductType.ValueMember = colName;
                    //this.jccmbProductType.DisplayMember = "Series";
                    this.jccmbProductType.DisplayMember = "Trans_Name";
                    //_productType = jccmbProductType.SelectedValue.ToString();                    
                    //_series = productTypeBll.GetSeriesByProductType(brandCode, regionCode, _productType);
                    _series = jccmbProductType.SelectedValue.ToString();
                    _productType = productTypeBll.GetProductTypeBySeries(brandCode, regionCode, _series);
                    BindIndoorTypeList(false);
                }
                else
                {
                    // 如果新风区域无新风机，则重置控件状态
                    this.dgvSelectedIndoor.Rows.Clear();
                    this.jccmbProductType.Enabled = false;
                    this.jccmbType.Enabled = false;
                    this.pnlManualButton.Enabled = false;
                    this.jcbtnOK.Enabled = false;
                    this.pnlAuto.Enabled = false;
                    return false;
                }

            }
            #endregion

            if (roomNode.ForeColor == ColorOriginal) // 该房间尚未分配室内机
            {
                RoomHasIndoor = false;
                IsAuto = SystemSetting.UserSetting.defaultSetting.IsRoomIndoorAuto;
                if (IsAuto)
                {
                    this.uc_CheckBox_Auto.Checked = true;
                    this.uc_CheckBox_Manual.Checked = false;
                }
                else
                {
                    this.uc_CheckBox_Auto.Checked = false;
                    this.uc_CheckBox_Manual.Checked = true;
                }
                if (roomNode.Tag is Room)
                {
                    if ((new ProjectBLL(thisProject)).isEmptyRoom(curRoom.Id))   //判断当前房间是否是空房间 ，空房间默认手动 on 20180504 by xyj
                    {
                        this.uc_CheckBox_Auto.Checked = false;
                        this.uc_CheckBox_Manual.Checked = true;
                        IsAuto = false;
                    }
                }

                // 将默认状态改为 Auto ，TODO：待确认，20140521 clh
                //jccmbProductType.SelectedIndex = 0; //切换room初始series列表
                BindIndoorTypeList(false);
                //jccmbType.SelectedIndex = 0;      //切换room初始type列表
                //curSelectType = jccmbType.SelectedText;

                //切换room定位上次选择type项
                if (roomNode.Text.IndexOf("FreshAir") > -1)  //如果选中的是新风机
                {
                    jccmbProductType.SelectedIndex = 0;
                    jccmbType.SelectedIndex = 0;
                    curSelectType = jccmbType.SelectedText;
                    //curSelectType = jccmbType.SelectedValue.ToString();
                    UpdateUnitType();
                }
                else
                {
                    jccmbType.SelectedIndex = jccmbType.FindString(curSelectType);
                    //jccmbType.SelectedValue = curSelectType;
                    //如果房间有静压需求则需要切换Type为Ducted型号
                    if (IsAuto && curRoom != null && curRoom.StaticPressure != 0 && !jccmbType.SelectedValue.ToString().ToLower().Contains("ducted"))
                    {
                        string unitType = bll.GetDefaultDuctedUnitType(thisProject.SubRegionCode, thisProject.BrandCode, _series);
                        if (!string.IsNullOrEmpty(unitType))
                        {
                            //jccmbType.SelectedIndex = jccmbType.FindString(unitType);6
                            jccmbType.SelectedValue = unitType;
                        }
                    }
                    //jccmbProductType.SelectedIndex = jccmbProductType.FindString(_series);
                    //jccmbProductType.SelectedValue = _productType;
                    jccmbProductType.SelectedValue = _series;
                    if (jccmbType.SelectedIndex < 0)
                        jccmbType.SelectedIndex = 0;
                    UpdateUnitType();
                }

                //if (this.jccmbType.Text != originalType)
                //{
                BindStdIndoorList();
                //}

                //IsAuto = false;
                PassValidation = true;
                if (IsAuto)
                {
                    DoAutoSelect();
                }
                else
                {
                    this.dgvSelectedIndoor.Rows.Clear();
                }
            }
            else
            {
                RoomHasIndoor = true;
                List<RoomIndoor> list = new List<RoomIndoor>();
                if (curRoom != null)
                {
                    pbll = new ProjectBLL(thisProject);
                    list = pbll.GetSelectedIndoorByRoom(curRoom.Id);

                    if (list.Count == 0)
                        return false;

                    //选中已经分配室内机的房间，需要记录当前productType及series
                    _productType = list[0].IndoorItem.ProductType;
                    _series = list[0].IndoorItem.Series;
                    //curSelectType = list[0].IndoorItem.Type;

                    BindIndoorTypeList(false);

                    // 绑定当前已选室内机的类型
                    IsAuto = list[0].IsAuto;

                    BindUnitTypeListText(list[0].IndoorItem);

                    //jccmbProductType.SelectedIndex = jccmbProductType.FindString(_series);
                    //jccmbType.SelectedIndex = jccmbType.FindString(curSelectType);
                    //jccmbProductType.SelectedValue = _productType;
                    jccmbProductType.SelectedValue = _series;
                    //jccmbType.SelectedValue = curSelectType;
                    this.jccmbType.Text = curSelectType;
                    UpdateUnitType();


                    //绑定当前已选室内机的显热模式 20161112 by Yunxiao Lin
                    //_shf_mode = list[0].SHF_Mode;
                    //shf = 0d;
                    //if (_shf_mode == "High")
                    //{
                    //    shf = list[0].IndoorItem.SHF_Hi;
                    //    shfComboBox.SelectedIndex = 0;
                    //}
                    //else if (_shf_mode == "Medium")
                    //{
                    //    shf = list[0].IndoorItem.SHF_Med;
                    //    shfComboBox.SelectedIndex = 1;
                    //}
                    //else
                    //{
                    //    shf = list[0].IndoorItem.SHF_Lo;
                    //    shfComboBox.SelectedIndex = 2;
                    //}
                    //if (shf == 0d)
                    //    shf = list[0].IndoorItem.SHF_Hi;
                    _fanSpeedLevel = list[0].FanSpeedLevel;
                    //shf = list[0].SHF;
                    shfComboBox.SelectedIndex = _fanSpeedLevel + 1;
                }
                else if (curFreshAirArea != null)
                {
                    //IsAuto = true;
                    IsAuto = curFreshAirArea.IsAuto;
                    //if (this.jccmbType.Items.Count > 0)
                    //{
                    //    this.jccmbType.SelectedIndex = 0;
                    //}
                    //curSelectType = this.jccmbType.Text;

                    this.jccmbType.Text = curSelectType;
                    //curSelectType = this.jccmbType.SelectedValue.ToString();
                    UpdateUnitType();

                    //绑定当前已选室内机的显热模式 20161112 by Yunxiao Lin
                    pbll = new ProjectBLL(thisProject);
                    list = pbll.GetSelectedIndoorByRoom(curFreshAirArea.Id);
                    //_shf_mode = list[0].SHF_Mode;
                    //shf = 0d;
                    //if (_shf_mode == "High")
                    //{
                    //    shf = list[0].IndoorItem.SHF_Hi;
                    //    shfComboBox.SelectedIndex = 0;
                    //}
                    //else if (_shf_mode == "Medium")
                    //{
                    //    shf = list[0].IndoorItem.SHF_Med;
                    //    shfComboBox.SelectedIndex = 1;
                    //}
                    //else
                    //{
                    //    shf = list[0].IndoorItem.SHF_Lo;
                    //    shfComboBox.SelectedIndex = 2;
                    //}
                    //if (shf == 0d)
                    //    shf = list[0].IndoorItem.SHF_Hi;
                    _fanSpeedLevel = list[0].FanSpeedLevel;
                    //shf = list[0].SHF;
                    shfComboBox.SelectedIndex = _fanSpeedLevel + 1;
                }
                BindStdIndoorList();

                //if (IsAuto)
                //{
                //    DoAutoSelect();
                //}
                //else
                //{

                //    BindSelectedList();
                //    DoCalculateSelectedSumCapacity();
                //    PassValidation = DoValidateCapacity();
                //}

                if (IsAuto && list != null)
                {
                    foreach (DataGridViewRow stdRow in this.dgvStdIndoor.Rows)
                    {
                        //stdRow.Selected = true;
                        if (thisProject.BrandCode == "Y")
                        {
                            if (list[0].IndoorItem.Model_York.Equals(stdRow.Cells[Name_Common.StdModelFull_York].Value))
                                this.dgvStdIndoor.CurrentCell = stdRow.Cells[Name_Common.StdModelFull_York]; // 定位标准数据中的选中行
                        }
                        else if (thisProject.BrandCode == "H")
                        {
                            if (list[0].IndoorItem.Model_Hitachi.Equals(stdRow.Cells[Name_Common.StdModelFull_Hitachi].Value))
                                this.dgvStdIndoor.CurrentCell = stdRow.Cells[Name_Common.StdModelFull_Hitachi];
                        }
                    }

                }
                BindSelectedList();
                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
            }
            if (IsAuto)
            {
                this.uc_CheckBox_Auto.Checked = true;
                this.uc_CheckBox_Manual.Checked = false;
            }
            else
            {
                this.uc_CheckBox_Auto.Checked = false;
                this.uc_CheckBox_Manual.Checked = true;
            }
            //this.uc_CheckBox_Auto.Checked = IsAuto;
            //this.uc_CheckBox_Manual.Checked = !IsAuto;
            return true;

        }

        // 打开房间楼层管理窗口
        /// <summary>
        /// 打开房间楼层管理窗口
        /// </summary>
        private void DoManageRoom()
        {
            frmRoomManage f = new frmRoomManage(thisProject);
            f.ShowDialog();
        }

        // 清空选中室内机记录的 Option
        /// <summary>
        /// 清空选中室内机记录的 Option
        /// </summary>
        private void DoClearOptions()
        {
            if (dgvSelectedIndoor.SelectedCells.Count > 0)
            {
                DataGridViewRow r = dgvSelectedIndoor.Rows[dgvSelectedIndoor.SelectedCells[0].RowIndex];
                if (string.IsNullOrEmpty(r.Cells[Name_Common.NO].Value.ToString()))
                    return;
                pbll = new ProjectBLL(thisProject);
                RoomIndoor ri = pbll.GetIndoor(Convert.ToInt32(r.Cells[Name_Common.NO].Value));
                //ri.OptionItem = null;
                r.Cells[Name_Common.ModelOption].Value = r.Cells[Name_Common.ModelFull].Value;

                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        // 删除已选的室内机记录，若其中包含了已添加到项目的记录，则弹出提示说明此处仅执行预删除，需点击OK按钮才从项目中彻底删除
        /// <summary>
        /// 删除已选的室内机记录，若其中包含了已添加到项目的记录，则弹出提示说明此处仅执行预删除，需点击OK按钮才从项目中彻底删除
        /// </summary>
        private void DoRemoveSelectedIndoorList(out bool success)
        {
            success = false;
            if (dgvSelectedIndoor.SelectedCells.Count > 0)
            {
                List<string> strNameList = new List<string>();
                List<int> rIndexList = new List<int>();
                string shareRindName = "";
                List<RoomIndoor> shareRiList = new List<RoomIndoor>();

                foreach (DataGridViewCell c in dgvSelectedIndoor.SelectedCells)
                {
                    if (rIndexList.Contains(c.RowIndex))
                        continue;
                    rIndexList.Add(c.RowIndex); // 获取要删除的行号list

                    string indName = dgvSelectedIndoor.Rows[c.RowIndex].Cells[Name_Common.Name].Value.ToString();
                    string indNumber = dgvSelectedIndoor.Rows[c.RowIndex].Cells[Name_Common.NO].Value.ToString();

                    if (indName != "")
                    {
                        if (!string.IsNullOrEmpty(indNumber))
                        {
                            int indNo = Convert.ToInt32(indNumber);
                            RoomIndoor indItem = pbll.GetIndoor(indNo);
                            //混连模式无法删除
                            if (indItem.SystemID != null && !indItem.SystemID.Equals(""))
                            {
                                SystemVRF curSysItem = pbll.GetSystem(indItem.SystemID);
                                if (curSysItem.SysType == SystemType.CompositeMode)
                                {
                                    JCMsg.ShowErrorOK(Msg.IND_DEL_FAIL);
                                    return;
                                }
                            }
                            strNameList.Add(indName);  // 获取已添加到项目的室内机的名称list

                            if (indItem.IndoorItemGroup != null && indItem.IndoorItemGroup.Count != 0)
                            {
                                shareRindName += indItem.IndoorName + ",";
                                shareRiList.Add(indItem);
                            }
                        }
                    }
                }

                if (strNameList.Count > 0)
                {
                    string names = "( ";
                    foreach (string s in strNameList)
                        names += s + " )";

                    // 提示对已经添加到项目的室内机仅执行预删除
                    if (JCMsg.ShowConfirmYesNoCancel(Msg.IND_INFO_DEL) != DialogResult.Yes)
                        return;
                }


                if (!string.IsNullOrEmpty(shareRindName))
                {
                    if (JCMsg.ShowConfirmOKCancel(Msg.IND_Delete_Sharing_RelationShip(shareRindName.Substring(0, shareRindName.Length - 1), "")) != DialogResult.OK)
                        return;
                    //存在共享，删除牵连的共享关系
                    if (!string.IsNullOrEmpty(shareRindName) && shareRiList.Count != 0)
                    {
                        foreach (RoomIndoor shareRi in shareRiList)
                        {
                            deleteShareRelationShip(shareRi, false);
                        }
                    }
                }


                // 执行删除及预删除
                List<DataGridViewRow> selectedRowList = new List<DataGridViewRow>();
                foreach (int rIndex in rIndexList)
                {
                    DataGridViewRow r = dgvSelectedIndoor.Rows[rIndex];
                    //DoRemoveSelectedIndoorRow(r);
                    //Modify on 20160817 by Yunxiao Lin 先取出要删除的行，存入缓存List，不要删除，直接删除会引起后续处理Index的变化
                    selectedRowList.Add(r);
                }
                //Add on 20160817 by Yunxiao Lin 在这里统一处理要删除的行
                foreach (DataGridViewRow r in selectedRowList)
                {
                    DoRemoveSelectedIndoorRow(r);
                }
                success = true;
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        // 删除已选的室内机记录行，给对应的 RoomIndoor 对象加删除标记
        /// <summary>
        /// 删除已选的室内机记录行，给对应的 RoomIndoor 对象加删除标记
        /// </summary>
        /// <param name="r"></param>
        private void DoRemoveSelectedIndoorRow(DataGridViewRow r)
        {
            string indName = r.Cells[Name_Common.Name].Value.ToString();
            string indNumber = r.Cells[Name_Common.NO].Value.ToString();
            if (indName != "" && !string.IsNullOrEmpty(indNumber))
            {
                pbll = new ProjectBLL(thisProject);
                int indNO = Convert.ToInt32(indNumber);
                RoomIndoor riItem = pbll.GetIndoor(indNO);
                riItem.IsDelete = true;
            }
            this.dgvSelectedIndoor.Rows.Remove(r);
        }

        //double est_cool = 0;
        //double est_heat = 0;
        //double est_sh = 0;
        //double airflow = 0;
        //double staticPressure = 0;
        //double shf = 0;
        //// 对当前标准表的指定行执行容量估算
        ///// <summary>
        ///// 对当前标准表的指定行执行容量估算
        ///// </summary>
        ///// <param name="stdRow">标准表记录行</param>
        ///// <param name="isAppend">是否附加到已选记录行，true则附加且count值可修改；false则添加唯一记录且count值不可修改</param>
        //private void DoCalculateEstValue(DataGridViewRow stdRow)
        //{
        //    string modelFull = stdRow.Cells[Name_Common.StdModelFull].Value.ToString();
        //    Indoor inItem = bll.GetItem(modelFull, _type, _productType);
        //    if (inItem != null)
        //        inItem.Series = _series;

        //    //if (IndoorBLL.IsFreshAirUnit(type))
        //    //{
        //    //    est_cool = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value);
        //    //    est_heat = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value);
        //    //    est_sh = Convert.ToDouble(stdRow.Cells[Name_Common.StdSensibleHeat].Value);
        //    //}
        //    if (curFreshAirArea != null)
        //    {
        //        est_cool = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value);
        //        est_heat = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value);
        //        //est_sh = Convert.ToDouble(stdRow.Cells[Name_Common.StdSensibleHeat].Value);
        //        //显热改为由估算容量*SHF计算得到 20161111
        //        //if (_shf_mode.Equals("High"))
        //        //    shf = inItem.SHF_Hi;
        //        //else if (_shf_mode.Equals("Medium"))
        //        //    shf = inItem.SHF_Med;
        //        //else if (_shf_mode.Equals("Low"))
        //        //    shf = inItem.SHF_Lo;

        //        //if(shf == 0d)
        //        //    shf = inItem.SHF_Hi;
        //        shf = inItem.GetSHF(_fanSpeedLevel);
        //        est_sh = est_cool * shf;
        //    }
        //    else
        //    {
        //        // 执行容量估算
        //        double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
        //        double db_c = inItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;//SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB;
        //        double db_h = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
        //        double wb_h = inItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;//SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB;

        //        //double est_sh_h = 0;
        //        //室内机估算容量不再返回显热
        //        //est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, out est_sh, false);
        //        est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
        //        if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName))
        //            return;

        //        if (!inItem.ProductType.Contains(", CO"))
        //        {
        //            //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
        //            est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
        //            if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName))
        //                return;
        //        }

        //        //显热改为由估算容量*SHF计算得到 20161111
        //        //if (_shf_mode.Equals("High"))
        //        //    shf = inItem.SHF_Hi;
        //        //else if (_shf_mode.Equals("Medium"))
        //        //    shf = inItem.SHF_Med;
        //        //else if (_shf_mode.Equals("Low"))
        //        //    shf = inItem.SHF_Lo;

        //        //if (shf == 0d)
        //        //    shf = inItem.SHF_Hi;
        //        shf = inItem.GetSHF(_fanSpeedLevel);
        //        est_sh = est_cool * shf;

        //    }
        //    airflow = inItem.GetAirFlow(_fanSpeedLevel);
        //    //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
        //    staticPressure = inItem.GetStaticPressure();
        //}

        /// <summary>
        /// 为列表中的每一行执行容量估算
        /// </summary>
        private void BatchCalculateEstValue()
        {
            if (!JCValidateSingle(jctxtWBCool) || !JCValidateSingle(jctxtDBHeat)) return;

            double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
            double db_h = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
            for (int i = 0; i < dgvStdIndoor.Rows.Count; ++i)
            {
                DataGridViewRow stdRow = dgvStdIndoor.Rows[i];
                //string modelFull = stdRow.Cells[Name_Common.StdModelFull].Value.ToString();
                //Indoor inItem = bll.GetItem(modelFull, _type, _productType);
                Indoor inItem = stdRow.Tag as Indoor;
                if (inItem == null) continue;

                double est_cool = 0;
                double est_heat = 0;
                double est_sh = 0;
                string type = inItem.Type;
                if (type.Contains("YDCF") || (type.Contains("Fresh Air") && thisProject.SubRegionCode != "LA_BR") || type.Contains("Ventilation")) //YDCF是SVRF的FreshAir, Global VRF 需要更多判断 20171212 by Yunxiao Lin
                {
                    //保持标准表数据
                    est_cool = inItem.CoolingCapacity;
                    est_heat = inItem.HeatingCapacity;
                    est_sh = inItem.SensibleHeat;
                }
                //Hydro Free和DX-Interface没有partload数据，取标准容量 20171212 by Yunxiao Lin
                else if (type.Contains("Hydro Free") || type == "DX-Interface")
                {
                    est_cool = inItem.CoolingCapacity;
                    est_heat = inItem.HeatingCapacity;
                    est_sh = 0d;
                }
                else
                {
                    // 执行容量估算
                    double db_c = inItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;//eidt by axj 20170116 old:SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB; //Cooling Outdoor DB
                    double wb_h = inItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;//eidt by axj 20170116 old:SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB; //Heating Outdoor WB
                    //double est_sh_h = 0;

                    //est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, out est_sh, false);
                    //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
                    est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
                    if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName))
                        return;
                    //显热由估算容量乘以SHF系数得到 20161112 by Yunxiao Lin
                    double shf = inItem.GetSHF(_fanSpeedLevel);
                    est_sh = est_cool * shf;

                    if (!inItem.ProductType.Contains(", CO"))
                    {
                        //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
                        //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
                        est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
                        if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName))
                            return;
                    }
                }
                //airflow = Convert.ToDouble(stdRow.Cells[Name_Common.StdAirFlow].Value);
                //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
                stdRow.Cells[Name_Common.StdCapacity_C].Value = Unit.ConvertToControl(est_cool, UnitType.POWER, utPower).ToString("n1");
                stdRow.Cells[Name_Common.StdCapacity_H].Value = Unit.ConvertToControl(est_heat, UnitType.POWER, utPower).ToString("n1");
                stdRow.Cells[Name_Common.StdSensibleHeat].Value = Unit.ConvertToControl(est_sh, UnitType.POWER, utPower).ToString("n1");

                for (int j = 0; j < dgvSelectedIndoor.Rows.Count; j++)
                {
                    DataGridViewRow selRow = dgvSelectedIndoor.Rows[j];
                    if (stdRow.Cells[Name_Common.StdModelFull].Value.ToString() == selRow.Cells[Name_Common.ModelFull].Value.ToString()
                        && _productType == selRow.Cells[Name_Common.ProductType].Value.ToString())
                    {
                        //将估算值保存到已选择列表中
                        selRow.Cells[Name_Common.Capacity_C].Value = est_cool;
                        selRow.Cells[Name_Common.Capacity_H].Value = est_heat;
                        selRow.Cells[Name_Common.SensibleHeat].Value = est_sh;
                    }
                }
            }

            DoCalculateSelectedSumCapacity();
            if (RoomHasIndoor)
            {
                PassValidation = DoValidateCapacity();
            }
        }

        /// <summary>
        /// 刷新跟风速有关的值
        /// </summary>
        private void BatchCalculateAirFlow()
        {
            for (int i = 0; i < dgvStdIndoor.Rows.Count; ++i)
            {
                DataGridViewRow stdRow = dgvStdIndoor.Rows[i];
                Indoor inItem = stdRow.Tag as Indoor;
                if (inItem == null) continue;

                double fa = 0;
                double airflow = 0;
                double staticPressure = 0;
                string type = inItem.Type;
                if (IndoorBLL.IsFreshAirUnit(type))  // 新风机计算新风风量合计
                {
                    fa = inItem.AirFlow;
                }
                else if (!type.Contains("Hydro Free") && type != "DX-Interface") //Hydro Free和DX-Interface没有Air Flow和Static Pressure属性 20171204 by Yunxiao Lin
                {
                    airflow = inItem.GetAirFlow(_fanSpeedLevel);
                    // staticPressure = inItem.GetStaticPressure();
                    if (type.ToLower().Contains("ducted".ToLower()) || type.Contains("Total Heat Exchanger"))
                    {
                        //只要包含Ducted，Total Heat Exchanger 才会显示staticPressure on 20180626 by xyj
                        staticPressure = inItem.GetStaticPressure();
                    }
                }
                stdRow.Cells[Name_Common.StdAirFlow].Value = Unit.ConvertToControl(airflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                //stdRow.Cells[Name_Common.StdStaticPressure].Value = staticPressure.ToString("n0");
                stdRow.Cells[Name_Common.StdStaticPressure].Value = Unit.ConvertToControl(staticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2"); 
                stdRow.Cells[Name_Common.StdFreshAir].Value = Unit.ConvertToControl(fa, UnitType.AIRFLOW, utAirflow).ToString("n0");
            }
        }

        private bool ValidateEstCapacity(double est, string partLoadTableName)
        {
            bool res = false;
            if (est == 0)
                //JCMsg.ShowWarningOK(Msg.DB_NOTABLE_CAP + "[" + partLoadTableName + "]\nRegion:" + thisProject.SubRegionCode + ";ProductType:" + thisProject.ProductType + "");
                JCMsg.ShowWarningOK(Msg.DB_NOTABLE_CAP + "[" + partLoadTableName + "]\nRegion:" + thisProject.SubRegionCode + ";ProductType:" + _productType + "");
            else if (est == -1)
                JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
            else
                res = true;
            return res;
        }

        // 计算相对湿度
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateRH()
        {
            if (!string.IsNullOrEmpty(this.jctxtDBCool.Text) && !string.IsNullOrEmpty(this.jctxtWBCool.Text))
            {
                double db_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(db_c), Convert.ToDecimal(wb_c), pressure));
                this.jctxtRH.Text = (rh * 100).ToString("n0");
                this.jclblRHValue.Text = (rh * 100).ToString("n0");
            }
        }

        // 计算相对湿度(根据对应的DB，WB,RH 改变值)
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateByOption(string Opt)
        {
            if (!string.IsNullOrEmpty(this.jctxtDBCool.Text) && !string.IsNullOrEmpty(this.jctxtWBCool.Text) && !string.IsNullOrEmpty(this.jctxtRH.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
                double rhcool = Convert.ToDouble(this.jctxtRH.Text);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                if (Opt == UnitTemperature.WB.ToString())
                {
                    double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                    if (this.jctxtRH.Text != (rh * 100).ToString("n0"))
                    {
                        this.jctxtRH.Text = (rh * 100).ToString("n0");
                    }
                }
                else if (Opt == UnitTemperature.DB.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                    if (this.jctxtWBCool.Text != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.jctxtWBCool.Text = wb.ToString("n1");
                        }
                    }
                }
                else if (Opt == UnitTemperature.RH.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                    if (this.jctxtWBCool.Text != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.jctxtWBCool.Text = wb.ToString("n1");
                        }
                    }
                }
            }
        }


        double tot_cool = 0;
        double tot_heat = 0;
        double tot_sensible = 0;
        double tot_airflow = 0;
        double tot_staticPressure = 0;
        double tot_fa = 0;
        // 计算当前房间已选室内机列表的估算容量总和
        /// <summary>
        /// 计算当前房间已选室内机列表的估算容量总和(Cooling)
        /// </summary>
        private void DoCalculateSelectedSumCapacity()
        {
            tot_cool = 0;
            tot_heat = 0;
            tot_sensible = 0;
            tot_airflow = 0;
            tot_staticPressure = 0;
            tot_fa = 0;
            foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
            {
                string modelfull = r.Cells[Name_Common.ModelFull].Value.ToString();
                string type = r.Cells[Name_Common.Type].Value.ToString();
                //Indoor inItem = bll.GetItem(modelfull, type, _productType);
                Indoor inItem = r.Tag as Indoor;
                if (inItem == null) continue;
                int count = Convert.ToInt32(r.Cells[Name_Common.Count].Value);

                double est_cool = Convert.ToDouble(r.Cells[Name_Common.Capacity_C].Value);
                double est_heat = Convert.ToDouble(r.Cells[Name_Common.Capacity_H].Value);
                double airflow = 0;//= Convert.ToDouble(r.Cells[Name_Common.AirFlow].Value);
                double staticPressure = 0;
                //est_sh = Convert.ToDouble(r.Cells[Name_Common.SensibleHeat].Value);
                //if (_shf_mode.Equals("High"))
                //    shf = Convert.ToDouble(r.Cells[Name_Common.SHF_Hi].Value);
                //else if (_shf_mode.Equals("Medium"))
                //    shf = Convert.ToDouble(r.Cells[Name_Common.SHF_Med].Value);
                //else if (_shf_mode.Equals("Low"))
                //    shf = Convert.ToDouble(r.Cells[Name_Common.SHF_Lo].Value);

                //if(shf == 0d)
                //    shf = Convert.ToDouble(r.Cells[Name_Common.SHF_Hi].Value);

                double shf = 0;
                if (inItem != null)
                {
                    shf = inItem.GetSHF(_fanSpeedLevel);

                    if (IndoorBLL.IsFreshAirUnit(type))  // 新风机计算新风风量合计
                    {
                        tot_fa += inItem.AirFlow * count;
                    }
                    else
                    {
                        airflow = inItem.GetAirFlow(_fanSpeedLevel);
                        //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
                        // 只要包含Ducted，Total Heat Exchanger 才会显示staticPressure on 20180626 by xyj
                        if (inItem.Type.ToLower().Contains("ducted".ToLower()) || inItem.Type.Contains("Total Heat Exchanger"))
                        {
                            staticPressure = inItem.GetStaticPressure();
                        }
                        // staticPressure = inItem.GetStaticPressure();

                        tot_airflow += airflow * count;
                        tot_staticPressure += staticPressure * count;
                    }
                }
                double est_sh = est_cool * shf;   //原本逻辑显热直接取数据库，新逻辑为  capacity * SHF (not actual capacity * SHF) 20161111


                // 计算容量总和
                tot_cool += est_cool * count;
                tot_heat += est_heat * count;
                tot_sensible += est_sh * count;
            }

            // 绑定已选室内机估算容量总和
            this.jclblCapacityCValue.Text = Unit.ConvertToControl(tot_cool, UnitType.POWER, utPower).ToString("n1");
            this.jclblCapacityHValue.Text = Unit.ConvertToControl(tot_heat, UnitType.POWER, utPower).ToString("n1");
            this.jclblSensiableValue.Text = Unit.ConvertToControl(tot_sensible, UnitType.POWER, utPower).ToString("n1");
            this.jclblAirflowValue.Text = Unit.ConvertToControl(tot_airflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //this.jclblStaticPressureValue.Text = tot_staticPressure.ToString("n0");
            this.jclblStaticPressureValue.Text = Unit.ConvertToControl(tot_staticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
            this.jclblFAValue.Text = Unit.ConvertToControl(tot_fa, UnitType.AIRFLOW, utAirflow).ToString("n0");
        }

        // 校验选择的室内机是否满足房间需求
        /// <summary>
        /// 校验选择的室内机是否满足房间需求,20140903 增加0.9的余量 
        /// </summary>
        private bool DoValidateCapacity()
        {
            // 手动选型，每次都需要校验需求
            toolStripStatusLabel1.Text = "";
            if (!IsAuto && this.dgvSelectedIndoor.Rows.Count > 0)
            {
                double rqFreshAir = 0;

                if (curRoom != null)
                {
                    rqFreshAir = curRoom.FreshAir;
                }
                else if (curFreshAirArea != null)
                {
                    rqFreshAir = curFreshAirArea.FreshAir;
                }

                // 新风机
                if (this.jccmbType.SelectedValue.ToString().Contains("YDCF") || this.jccmbType.SelectedValue.ToString().Contains("Fresh Air"))
                {
                    if (tot_fa < rqFreshAir * 1)
                    {
                        toolStripStatusLabel1.Text = Msg.IND_NOTMEET_FA;
                        return false;
                    }
                }
                else
                {
                    if (thisProject.IsCoolingModeEffective)
                    {
                        if (tot_cool < curRoom.RqCapacityCool * 1 || tot_sensible < curRoom.SensibleHeat * 1 || tot_airflow < curRoom.AirFlow * 1 || tot_staticPressure < curRoom.StaticPressure)
                        {
                            if (tot_cool >= curRoom.RqCapacityCool * 0.9)
                            {
                                if (!(JCMsg.ShowConfirmYesNoCancel(Msg.IND_WARN_CAPLower2) == DialogResult.Yes))
                                {
                                    toolStripStatusLabel1.Text = Msg.IND_NOTMEET_COOLING;
                                    return false;
                                }
                            }
                            else
                            {
                                toolStripStatusLabel1.Text = Msg.IND_NOTMEET_COOLING;
                                return false;
                            }
                        }
                    }
                    //if (thisProject.IsHeatingModeEffective)
                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭IsHeatingModeEffective无法确定一个System是否需要制热。
                    // 如果productType中包含", CO"，那么这个System是单冷系统，就算IsHeatingModeEffective=true，也不需要制热功能。 20160826 by Yunxiao Lin
                    if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
                    {
                        if (tot_heat < curRoom.RqCapacityHeat * 1)
                        {
                            if (tot_heat >= curRoom.RqCapacityHeat * 0.9)
                            {
                                if (!(JCMsg.ShowConfirmYesNoCancel(Msg.IND_WARN_CAPLower2) == DialogResult.Yes))
                                {
                                    toolStripStatusLabel1.Text = Msg.IND_NOTMEET_HEATING;
                                    return false;
                                }
                            }
                            else
                            {
                                toolStripStatusLabel1.Text = Msg.IND_NOTMEET_HEATING;
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        // 根据当前房间信息自动选型
        /// <summary>
        /// 根据当前房间信息自动选型
        /// </summary>
        private void DoAutoSelect()
        {
            BindSelectedList(); // 绑定当前房间已选的室内机记录

            // 若当前房间尚未分配室内机，则先新增一空行记录
            if (this.dgvSelectedIndoor.Rows.Count == 0)
                addToSelectedRow(null, true);
            else
            {
                // 若已分配过室内机，则仅保留第一行记录
                for (int i = dgvSelectedIndoor.Rows.Count - 1; i > 0; --i)
                    DoRemoveSelectedIndoorRow(dgvSelectedIndoor.Rows[i]);
            }

            // 遍历当前类型下的标准表记录，查找最适合的室内机
            bool isOK = false;
            foreach (DataGridViewRow stdRow in this.dgvStdIndoor.Rows)
            {
                bool isFreshAir = IndoorBLL.IsFreshAirUnit(_type);
                bool isPass = true;

                // 速度优化：先比较标准容量数值 20130802 clh   
                //update on 2018-2-2  by xyj 数据值进行公英制转换
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);

                if (curRoom != null)
                {
                    if (thisProject.IsCoolingModeEffective && std_cool < curRoom.RqCapacityCool)
                        isPass = false;

                    //if (thisProject.IsHeatingModeEffective && std_heat < curRoom.RqCapacityHeat)
                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭IsHeatingModeEffective无法确定一个System是否需要制热。
                    // 如果productType中包含", CO"，那么这个System是单冷系统，就算IsHeatingModeEffective=true，也不需要制热功能。 20160826 by Yunxiao Lin
                    if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO") && std_heat < curRoom.RqCapacityHeat)
                        isPass = false;

                    if (!isPass || !autoCompare(isFreshAir, stdRow, curRoom))
                        continue;
                    else
                        isOK = true;
                }
                else if (curFreshAirArea != null)
                {
                    if (!isPass || !autoCompare(stdRow, curFreshAirArea))
                        continue;
                    else
                        isOK = true;
                }

                stdRow.Selected = true;
                if (thisProject.BrandCode == "Y")
                    this.dgvStdIndoor.CurrentCell = stdRow.Cells[1]; // 定位标准数据中的选中行
                else if (thisProject.BrandCode == "H")
                {
                    this.dgvStdIndoor.CurrentCell = stdRow.Cells[2];
                }

                updateSelectedRow(stdRow, dgvSelectedIndoor.Rows[0], true);

                DoCalculateSelectedSumCapacity();
                break;
            }

            if (dgvSelectedIndoor.Rows[0].Cells[Name_Common.ModelFull].Value.ToString() == "")
            {
                this.dgvSelectedIndoor.Rows.Clear();
            }
            this.toolStripStatusLabel1.Text = "";
            if (this.dgvSelectedIndoor.Rows.Count == 0 || !isOK)
            {
                this.toolStripStatusLabel1.Text = Msg.IND_NOTMATCH;
                PassValidation = false;
            }
            else
                PassValidation = true;
        }

        /// <summary>
        /// 自动选型模式，比较指定标准行的室内机估算容量与需求容量，满足则返回true
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stdRow"></param>
        /// <param name="room">当前房间对象</param>
        /// <returns></returns>
        private bool autoCompare(bool isFreshAir, DataGridViewRow stdRow, Room room)
        {
            if (stdRow == null) return false;
            bool pass = true;
            // 计算估算容量
            //DoCalculateEstValue(stdRow);

            //从控件中取值进行直接比较  add by Shen Junjie on 2018/2/28
            double est_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdSensibleHeat].Value), UnitType.POWER, utPower);
            double airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdAirFlow].Value), UnitType.AIRFLOW, utAirflow);
            //double staticPressure = Convert.ToDouble(stdRow.Cells[Name_Common.StdStaticPressure].Value);
            double staticPressure = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdStaticPressure].Value), UnitType.STATICPRESSURE, utPressure);

            if (isFreshAir)
            {
                airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdFreshAir].Value), UnitType.AIRFLOW, utAirflow);
                //将估算容量与当前需求进行比较
                if (airflow < room.FreshAir)
                    pass = false;
            }
            else
            {
                if (thisProject.IsCoolingModeEffective)
                {
                    if (est_cool < room.RqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
                        pass = false;
                }
                //if (thisProject.IsHeatingModeEffective && !thisProject.ProductType.Contains(", CO"))
                if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
                {
                    if (est_heat < room.RqCapacityHeat)
                        pass = false;
                }
            }
            return pass;
        }

        /// <summary>
        /// 自动选型模式(新风区域)，只比较风量
        /// </summary>
        /// <param name="stdRow"></param>
        /// <returns></returns>
        private bool autoCompare(DataGridViewRow stdRow, FreshAirArea area)
        {
            bool pass = true;
            // 计算估算容量
            //DoCalculateEstValue(stdRow);

            //从控件中取值进行直接比较  add by Shen Junjie on 2018/2/28
            double airflow = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdFreshAir].Value), UnitType.AIRFLOW, utAirflow);

            //将估算容量与当前需求进行比较
            if (airflow < area.FreshAir)
                pass = false;
            return pass;
        }

        /// <summary>
        /// 将选中的标准行记录，添加到已选室内机列表
        /// </summary>
        /// <param name="stdRow">选中的标准行记录，若为null，则已选记录中新增一空行</param>
        /// <param name="isAuto">是否是自动选型模式</param>
        private void addToSelectedRow(DataGridViewRow stdRow, bool isAuto)
        {
            //从控件中取值进行直接比较  add by Shen Junjie on 2018/4/17
            double est_cool = 0;
            double est_heat = 0;
            double est_sh = 0;

            dgvSelectedIndoor.Rows.Add();
            DataGridViewRow newRow = dgvSelectedIndoor.Rows[dgvSelectedIndoor.Rows.Count - 1];
            newRow.Cells[Name_Common.NO].Value = "";
            newRow.Cells[Name_Common.Name].Value = "";
            newRow.Cells[Name_Common.ModelFull].Value = "";
            newRow.Cells[Name_Common.ModelOption].Value = newRow.Cells[Name_Common.ModelFull].Value;

            newRow.Cells[Name_Common.ModelFull_York].Value = "";
            newRow.Cells[Name_Common.ModelFull_Hitachi].Value = "";
            newRow.Cells[Name_Common.ProductType].Value = "";
            //newRow.Cells[Name_Common.SHF_Hi].Value = 0d;
            //newRow.Cells[Name_Common.SHF_Med].Value = 0d;
            //newRow.Cells[Name_Common.SHF_Lo].Value = 0d;

            if (stdRow != null)
            {
                newRow.Tag = stdRow.Tag;
                newRow.Cells[Name_Common.ModelFull].Value = stdRow.Cells[Name_Common.StdModelFull].Value;
                newRow.Cells[Name_Common.ModelOption].Value = newRow.Cells[Name_Common.ModelFull].Value;
                newRow.Cells[Name_Common.ModelFull_York].Value = stdRow.Cells[Name_Common.StdModelFull_York].Value;
                newRow.Cells[Name_Common.ModelFull_Hitachi].Value = stdRow.Cells[Name_Common.StdModelFull_Hitachi].Value;
                ////将标准室内机的SHF系数添加到新的已选室内机 20161111 by Yunxiao Lin
                //newRow.Cells[Name_Common.SHF_Hi].Value = stdRow.Cells[Name_Common.SHF_Hi].Value;
                //newRow.Cells[Name_Common.SHF_Med].Value = stdRow.Cells[Name_Common.SHF_Med].Value;
                //newRow.Cells[Name_Common.SHF_Lo].Value = stdRow.Cells[Name_Common.SHF_Lo].Value;

                est_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
                est_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);
                est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdSensibleHeat].Value), UnitType.POWER, utPower);
            }

            newRow.Cells[Name_Common.ModelOption].Value = newRow.Cells[Name_Common.ModelFull].Value;
            newRow.Cells[Name_Common.Count].Value = "1";
            //newRow.Cells[Name_Common.Type].Value = this.jccmbType.Text.Trim();
            newRow.Cells[Name_Common.Type].Value = _type;

            newRow.Cells[Name_Common.Capacity_C].Value = est_cool;
            newRow.Cells[Name_Common.Capacity_H].Value = est_heat;
            newRow.Cells[Name_Common.SensibleHeat].Value = est_sh;
            //newRow.Cells[Name_Common.AirFlow].Value = airflow;
            newRow.Cells[Name_Common.ProductType].Value = _productType;

            if (this.dgvSelectedIndoor.Columns[Name_Common.Count] != null)
                this.dgvSelectedIndoor.Columns[Name_Common.Count].ReadOnly = isAuto;

        }

        void updateSelectedRow(DataGridViewRow stdRow, DataGridViewRow selRow, bool isAuto)
        {
            if (stdRow == null) return;
            pbll = new ProjectBLL(thisProject);
            // 如果更新已选的室内机，则需要清空原先已选的 Option
            //if (selRow.Cells[Name_Common.ModelFull].Value.ToString() != stdRow.Cells[Name_Common.StdModelFull].Value.ToString()
            //    && !string.IsNullOrEmpty(selRow.Cells[Name_Common.NO].Value.ToString()))
            //{
            //    RoomIndoor ri = pbll.GetIndoor(Convert.ToInt32(selRow.Cells[Name_Common.NO].Value));
            //    //ri.OptionItem = null;  //因这句话已被注销所以注销整体已提高性能  deleted by Shen Junjie 2018/4/25
            //}

            selRow.Tag = stdRow.Tag;
            selRow.Cells[Name_Common.ModelFull].Value = stdRow.Cells[Name_Common.StdModelFull].Value;
            selRow.Cells[Name_Common.ModelOption].Value = selRow.Cells[Name_Common.ModelFull].Value;
            selRow.Cells[Name_Common.ModelFull_York].Value = stdRow.Cells[Name_Common.StdModelFull_York].Value;
            selRow.Cells[Name_Common.ModelFull_Hitachi].Value = stdRow.Cells[Name_Common.StdModelFull_Hitachi].Value;
            ////将标准室内机中的SHF系数更新到已选室内机列表 20161111 by Yunxiao Lin
            //selRow.Cells[Name_Common.SHF_Hi].Value = stdRow.Cells[Name_Common.SHF_Hi].Value;
            //selRow.Cells[Name_Common.SHF_Med].Value = stdRow.Cells[Name_Common.SHF_Med].Value;
            //selRow.Cells[Name_Common.SHF_Lo].Value = stdRow.Cells[Name_Common.SHF_Lo].Value;

            //selRow.Cells[Name_Common.Count].Value = "1";
            //selRow.Cells[Name_Common.Type].Value = this.jccmbType.Text.Trim();
            selRow.Cells[Name_Common.Type].Value = _type;

            //从控件中取值进行直接比较  add by Shen Junjie on 2018/4/17
            double est_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdSensibleHeat].Value), UnitType.POWER, utPower);

            selRow.Cells[Name_Common.Capacity_C].Value = est_cool;
            selRow.Cells[Name_Common.Capacity_H].Value = est_heat;
            selRow.Cells[Name_Common.SensibleHeat].Value = est_sh;
            //selRow.Cells[Name_Common.AirFlow].Value = airflow;
            selRow.Cells[Name_Common.ProductType].Value = _productType;

            if (this.dgvSelectedIndoor.Columns[Name_Common.Count] != null)
                this.dgvSelectedIndoor.Columns[Name_Common.Count].ReadOnly = isAuto;

        }

        #endregion

        private void dgvSelectedIndoor_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = UtilColor.bg_dgvHeader_Indoor;
            Pen pen_dgvBorder = new Pen(UtilColor.border_dgvHeader, 0.1f);
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex == -1) //标题行
            {
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

        private void jccmbProductType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //Indoor 选择ProductType会引起Unit Type列表刷新 20160821 by Yunxiao Lin
            string brandCode = Registr.Registration.SelectedBrand.Code;
            string regionCode = Registr.Registration.SelectedSubRegion.Code;
            //_productType = jccmbProductType.SelectedValue.ToString();
            //_series = jccmbProductType.Text;
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            //_series = productTypeBll.GetSeriesByProductType(brandCode, regionCode, _productType);
            _series = jccmbProductType.SelectedValue.ToString();
            _productType = productTypeBll.GetProductTypeBySeries(brandCode, regionCode, _series);
            BindIndoorTypeList(false);

            //如果房间有静压需求则需要切换Type为Ducted型号
            if (IsAuto && curRoom != null && curRoom.StaticPressure != 0)
            {
                //string unitType = bll.GetDefaultDuctedUnitType(thisProject.SubRegionCode, thisProject.BrandCode, _series);
                string unitType = bll.GetDefaultDuctedUnitType(regionCode, brandCode, _series);
                if (!string.IsNullOrEmpty(unitType))
                {
                    //jccmbType.SelectedIndex = jccmbType.FindString(unitType);
                    jccmbType.SelectedValue = unitType;
                    if (jccmbType.SelectedIndex < 0) jccmbType.SelectedIndex = 0;
                    UpdateUnitType();
                }
            }

            BindStdIndoorList();
            //重新选择product Type 也会触发重新选型 20161107
            //if (curSelectType == this.jccmbType.SelectedValue.ToString() && _series == jccmbProductType.Text.ToString())
            if (curSelectType == this.jccmbType.Text && _series == jccmbProductType.Text.ToString())
                return;

            curSelectType = this.jccmbType.Text;
            //curSelectType = this.jccmbType.SelectedValue.ToString();

            // 检验需求信息是否填写
            if (!this.JCValidateGroup(this.pnlRoom_2))
                return;

            if (IsAuto)
            {
                DoAutoSelect();
            }
        }

        /// 绑定SHF列表
        /// <summary>
        /// 绑定SHF列表
        /// </summary>
        private void BindShfMode()
        {
            List<String> shfList = new List<string>();
            shfList.Add("Max");
            shfList.Add("High2");
            shfList.Add("High");
            shfList.Add("Medium");
            shfList.Add("Low");
            shfComboBox.DataSource = shfList;
            shfComboBox.SelectedIndex = 0;
            _fanSpeedLevel = -1;
        }

        private void shfComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fanSpeedLevel = shfComboBox.SelectedIndex - 1;
            if (!this.JCValidateGroup(this.pnlIndCooling_1) || !this.JCValidateGroup(this.pnlIndHeating_1))
                return;
            BatchCalculateEstValue();
            BatchCalculateAirFlow();
            if (IsAuto)
            {
                DoAutoSelect();
            }
            else
            {
                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
            }
        }
        
        /// 检查工况温度 add by axj 20170116
        /// <summary>
        /// 检查工况温度
        /// </summary>
        /// <param name="r"></param>
        /// <param name="prj"></param>
        private void ChkOutdoorTemperature(RoomIndoor r, Project prj)
        {
            if (r.SystemID != null && r.SystemID != "")
            {
                var sys = prj.SystemList.Find(p => p.Id == r.SystemID);
                if (sys != null)
                {
                    outdoorCoolingDB = sys.DBCooling;
                    outdoorHeatingWB = sys.WBHeating;
                    outdoorCoolingIW = sys.IWCooling;
                    outdoorHeatingIW = sys.IWHeating;
                }
            }
        }

        /// <summary>
        /// 删除共享关联   --add on 20170621 by Lingjia Qiu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool deleteShareRelationShip(RoomIndoor ri, bool msgFlg)
        {
            string groupIndoorName = "";
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
        /// 控制输入参数 
        /// </summary>
        /// <param name="sender"></param>
        public void ControllerInput(object sender)
        {
            TextBox tx = sender as TextBox;
            if (NumberUtil.IsNumber(tx.Text))
            {
                //如果数值出现负数则清空当前文本框值
                if (tx.Text.Contains("-"))
                    tx.Text = "";
            }
            else
            {
                //非数字包含 不包含数值清空，包含过滤（例如2.）
                if (!tx.Text.Contains("."))
                    tx.Text = "";
            }
        }

        public void ControllerInputDBHeat(object sender)
        {
            TextBox tx = sender as TextBox;
            if (!NumberUtil.IsNumber(tx.Text))
            {
                //非数字包含 不包含数值清空，包含过滤（例如2.）
                if (!tx.Text.Contains("."))
                    tx.Text = "";
            }
        }

        /// <summary>
        /// 表头排序  --add on 20170914 by xyj
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvStdIndoor_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            switch (e.Column.Name)
            {
                // 以下列点击列名时按照数值大小排序
                case Name_Common.StdCapacity_C:
                case Name_Common.StdSensibleHeat:
                case Name_Common.StdAirFlow:
                case Name_Common.StdCapacity_H:
                case Name_Common.StdStaticPressure:
                case Name_Common.StdFreshAir:
                    double d = Convert.ToDouble(e.CellValue1) - Convert.ToDouble(e.CellValue2);
                    e.SortResult = d > 0 ? 1 : d < 0 ? -1 : 0;
                    break;
                // 默认按照字符顺序排序
                default:
                    e.SortResult = String.Compare(Convert.ToString(e.CellValue1), Convert.ToString(e.CellValue2));
                    break;
            }
            e.Handled = true;
        }

        private void frmAddIndoor_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil.Dispose();
        }
    }
}
