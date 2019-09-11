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
using JCHVRF.Const;
using JCHVRF.VRFMessage;
using JCBase.Utility;
using JCHVRF.VRFTrans;

namespace JCHVRF
{
    public partial class frmExchangerByRoom : JCBase.UI.JCForm
    {
        #region Initialization 初始化

        bool IsAuto = false;        // 当前Auto按钮的选中状态 
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
        double roomDB = 0;
        double roomWB = 0;
        double roomRH = 0;

        Trans trans = new Trans();   //翻译初始化
        
        public frmExchangerByRoom()
        {
            InitializeComponent();
        }

        /// 绑定Power列表 20170726 by xyj
        /// <summary>
        /// 绑定SHF列表
        /// </summary>
        private void BindPowerMode()
        {
            string colName = "UnitType";
            DataTable dtList = Global.InitExchangerTypeList(this.thisProject);
            dtList = trans.getTypeTransDt(TransType.Series.ToString(), dtList, colName);
            if (dtList.Rows.Count > 0)
            {
                jccmbProductType.DataSource = dtList;
                //this.jccmbProductType.DisplayMember = "UnitType";
                //this.jccmbProductType.ValueMember = "UnitType";
                this.jccmbProductType.DisplayMember = "Trans_Name";
                this.jccmbProductType.ValueMember = colName;
                if (this.JCFormMode == FormMode.EDIT)
                {
                    this.jccmbProductType.SelectedValue = curRI.IndoorItem.Series;
                    //_series = jccmbProductType.Text.ToString();
                }
                else
                {
                    jccmbProductType.SelectedIndex = 0;
                    //_series = jccmbProductType.Text.ToString();
                }
                _series = jccmbProductType.SelectedValue.ToString();
                BindPowerlist();
                jccmbPower.SelectedIndex = 0;
                _fanSpeedLevel = -1;
            }

        }

        private void BindPowerlist()
        {
            DataTable dt = Global.InitPowerList(thisProject, _series);
            jccmbPower.DataSource = dt;
            this.jccmbPower.DisplayMember = "PowerKey";
            this.jccmbPower.ValueMember = "PowerValue";

        }
        /// <summary>
        /// 构造函数，新增
        /// </summary>
        /// <param name="thisProj"> 当前项目对象 </param>
        public frmExchangerByRoom(Project thisProj)
        {
            InitializeComponent();
            JCSetLanguage();    // 设置界面语言
            thisProject = thisProj;
            JCFormMode = FormMode.NEW;
            curRI = new RoomIndoor();
            this.IsAuto = false;
            this.pnlRoom_1.Enabled = true;
        }

        /// <summary>
        /// 构造函数，编辑
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="thisProj">当前项目对象</param>
        /// <param name="rflag">true 则表示当前房间已选全热交换机不合适（Error图标）</param>
        public frmExchangerByRoom(RoomIndoor ri, Project thisProj, Boolean rflag)
        {
            InitializeComponent();
            JCSetLanguage();    // 设置界面语言
            thisProject = thisProj;
            JCFormMode = FormMode.EDIT;
            curRI = ri;
            pbll = new ProjectBLL(thisProject);
            curRoom = pbll.GetRoom(ri.RoomID);
            //this.pnlRoom_1.Enabled = false;

            // 全热交换机编辑界面隐藏Room选择，用显示Room名称代替。2016-4-18 by lin
            this.pnlRoom_1.Enabled = true; //enabled=False会引起容器内控件字体颜色变灰，所以设为True。2016-4-18 by lin
            this.pnlIndoor_3.Visible = true;
            ChkOutdoorTemperature(ri, thisProj);//检查工况温度 add by axj 20170116
            flag = rflag;
        }

        private void frmExchangerByRoom_Load(object sender, EventArgs e)
        {
            Initialization();
            /*注册撤销功能 add by axj 20161228 begin */
            UndoRedoUtil = new UndoRedo.UndoRedoHandler(true);
            UndoRedoUtil.ShowIconsOnTabPage(tapPageTrans1, new Rectangle(968, 6, 16, 18), new Rectangle(940, 6, 16, 18));

            UndoRedoUtil.GetCurrentProjectEventHandler += delegate (out Project prj) //获取最新项目数据
            {
                prj = thisProject.DeepClone();//返回当前项目数据的副本
            };
            UndoRedoUtil.ReloadProjectEventHandler += delegate (Project prj) //重新加载历史记录里面的项目数据
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
            // this.uc_CheckBox_Auto.CheckedChanged -= new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            // this.uc_CheckBox_Manual.CheckedChanged -= new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            toolStripStatusLabel1.Text = "";

            // 当已选全热交换机的房间需求信息改变，导致已选的全热交换机不满足房间需求时，系统给出相应的提示信息
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
                    JCMsg.ShowWarningOK(Msg.Exc_NOTMATCH_ROOMCHANGE);

            }
            //-------------------
            InitDGV();
            BindUnit();
            BindPowerMode();
            BindShfMode();
            // BindCmbProductType(); //绑定Indoor的ProductType列表 add on 20160821 by Yunxiao Lin
            // bll初始化放在这边，下面的BindIndoorTypeList要用到 20160906 by Yunxiao Lin
            bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            
            //绑定Exchanger on 2017-07-12 by xyj
            //BindStdExchangerList();

            BindDBWB(); // Updated on 20140623 clh // 必须放到BindTreeViewRoom之前执行，因为BindTreeViewRoom()会触发Auto选型

            BindTreeViewRoom();

            ResetControlState(!this.uc_CheckBox_Manual.Checked);

            //if (uc_CheckBox_Auto.Checked == true && flag == true)
            //{
            //    DoAutoSelect();
            //}
            //this.uc_CheckBox_Auto.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            // this.uc_CheckBox_Manual.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            // this.uc_CheckBox_Auto.TextString = ShowText.Auto;
            // this.uc_CheckBox_Manual.TextString = ShowText.Manual;

            this.tvRoom.ExpandAll();
            if (this.tvRoom.Nodes.Count > 0 && this.JCFormMode == FormMode.NEW)
            {
                this.tvRoom.Nodes[0].EnsureVisible();
            }
        }

        #endregion

        #region Controls events


        private void tvRoom_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.toolStripStatusLabel1.Text = "";
            bool isEnable = true;
            this.jclblSelectedRoom.Text = Msg.GetResourceString("SelectRoom");
            this.jclblSelectedRoom_2.Text = this.jclblSelectedRoom.Text; //当编辑全热交换机时，用Room名称代替Room选择界面。 2014-4-18 by lin

            curRoom = null;
            if (e.Node.Level == 2)
            {
                this.jclblSelectedRoom.Text = e.Node.Text;
                this.jclblSelectedRoom_2.Text = this.jclblSelectedRoom.Text; //当编辑全热交换机时，用Room名称代替Room选择界面。 2014-4-18 by lin
                curRoom = (Room)e.Node.Tag;
                if (curRoom != null)
                {
                    pbll = new ProjectBLL(thisProject);
                    List<RoomIndoor> list = pbll.GetSelectedExchangerByRoom(curRoom.Id);
                    jccmbProductType.SelectedIndex = 0;
                }
                BindRqCapacity();
                BindToControl(e.Node);
            }
            //BindRqCapacity();
            if (isEnable)
                ResetControlState(!this.uc_CheckBox_Manual.Checked);
        }


        // 绑定标准表
        /// <summary>
        /// 绑定标准表
        /// </summary>
        private void BindStdExchangerList()
        {
            dgvStdIndoor.Rows.Clear();
            string type = this.jccmbPower.SelectedValue.ToString();
            DataTable dt = bll.GetExchnagerListStd(_series, "", type);
            dt.DefaultView.Sort = "AirFlow";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Indoor inItem = bll.GetItem(dr["ModelFull"].ToString(), _series, _series, _series);
                    double est_cool = Convert.ToDouble(dr["CoolCapacity"].ToString());
                    double est_heat = Convert.ToDouble(dr["HeatCapacity"].ToString());
                    double airflow = 0;
                    double staticPressure = 0;
                    double shf = 0;
                    double fa = 0;
                    if (inItem != null)
                    {
                        shf = inItem.GetSHF(-1);
                        airflow = inItem.GetAirFlow(-1);
                        //staticPressure = inItem.GetStaticPressure(-1);
                        staticPressure = inItem.GetStaticPressure();
                    }
                    double est_sh = est_cool * shf;
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
                }
            } 

            if (thisProject.BrandCode == "Y")
            {
                this.dgvStdIndoor.Columns[Name_Common.StdModelFull_York].Visible = true;
            }
            else if (thisProject.BrandCode == "H")
            {
                this.dgvStdIndoor.Columns[Name_Common.StdModelFull_Hitachi].Visible = true;
            }
            this.dgvStdIndoor.Columns[Name_Common.StdModelFull].Visible = false;
        }


        //选择
        private void uc_CheckBox_Auto_CheckedChanged(object sender, EventArgs e)
        {
            uc_CheckBox cbx = sender as uc_CheckBox;
            if (cbx.TextString == uc_CheckBox_Auto.TextString)
            {
                uc_CheckBox_Manual.Checked = !uc_CheckBox_Auto.Checked;
            }
            else
            {
                uc_CheckBox_Auto.Checked = !uc_CheckBox_Manual.Checked;
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
                PassValidation = DoValidateCapacity();
            }
        }

        private void dgvStdIndoor_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || (curRoom == null))
                return;

            if (dgvSelectedIndoor.Rows.Count > 1)
            {
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    if (_series != r.Cells[Name_Common.Type].Value.ToString())
                    {
                        JCMsg.ShowWarningOK(Msg.WARNING_EXC_PRODUCTTYPE);
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
                if (stdRow.Cells[Name_Common.StdModelFull].Value.ToString() == selRow.Cells[Name_Common.ModelFull].Value.ToString() && _series == selRow.Cells[Name_Common.Type].Value.ToString())
                    return;

                DoCalculateEstValue(stdRow);
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
                    DoCalculateEstValue(r);
                    addToSelectedRow(r, false);
                }

                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
            }
        }

        private void jcbtnSelect_Click(object sender, EventArgs e)
        {
            if (curRoom == null)
                return;
            if (dgvStdIndoor.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    if (_series != r.Cells[Name_Common.Type].Value.ToString())
                    {
                        JCMsg.ShowWarningOK(Msg.WARNING_EXC_PRODUCTTYPE);
                        return;
                    }
                }

                foreach (DataGridViewRow r in dgvStdIndoor.SelectedRows)
                {
                    DoCalculateEstValue(r);
                    addToSelectedRow(r, false);
                }

                DoCalculateSelectedSumCapacity();
                PassValidation = DoValidateCapacity();
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
            DoRemoveSelectedExchangerList(out success);
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

        private void BindRoomCooling()
        {
            roomDB = Convert.ToDouble(jctxtDBCool.Text);
            roomWB = Convert.ToDouble(jctxtWBCool.Text);
            roomRH = Convert.ToDouble(jctxtRH.Text);
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
                        {
                            return;
                        }

                        DoCalculateByOption(UnitTemperature.RH.ToString());
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
                        {
                            return;
                        }
                        DoCalculateByOption(UnitTemperature.DB.ToString());
                        if (JCValidateGroup(pnlIndCooling))
                        {
                            BindRoomCooling();
                            //  DoCalculateRH();
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
                if (jctxtWBCool.Text.Length > 1)
                {
                    if (NumberUtil.IsNumber(jctxtWBCool.Text))
                    {
                        if (roomWB == Convert.ToDouble(jctxtWBCool.Text))
                        {
                            return;
                        }
                        DoCalculateByOption(UnitTemperature.WB.ToString());
                        if (JCValidateGroup(pnlIndCooling))
                        {
                            BindRoomCooling();
                            // DoCalculateRH();
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

        private void jctxtDBHeat_TextChanged(object sender, EventArgs e)
        {

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
             
            if (this.tvRoom.SelectedNode == null)
            {
                JCMsg.ShowWarningOK(Msg.INDBYROOM_NO_ROOM);
                return;
            }
            pbll = new ProjectBLL(thisProject);
            // 若当前房间删除了所有的已选全热交换机，则不需校验直接从当前项目中删除
            if (this.dgvSelectedIndoor.Rows.Count == 0)
            {
                thisProject.ExchangerList.RemoveAll(c => { return c.IsDelete; });
                SetNodeState(this.tvRoom.SelectedNode, false); // add on 20130822 clh
                //删除所有全热交换机后也需要校验，不然不会更新全热交换机列表 20160818 by Yunxiao Lin
                //return;
            }

            if (PassValidation)
            {
                string roomID = curRoom != null ? curRoom.Id : "";  //房间ID

                #region 更新已选全热交换机的记录
                foreach (DataGridViewRow r in this.dgvSelectedIndoor.Rows)
                {
                    int count = Convert.ToInt32(r.Cells[Name_Common.Count].Value);
                    string modelfull = r.Cells[Name_Common.ModelFull].Value.ToString();
                    string type = r.Cells[Name_Common.Type].Value.ToString();
                    Indoor inItem = bll.GetItem(modelfull, type, type, _series); // 新全热交换机对象
                    if (inItem != null)
                        inItem.Series = _series;

                    while (count > 0)
                    {
                        string indNOStr = r.Cells[Name_Common.NO].Value.ToString();
                        string indName = r.Cells[Name_Common.Name].Value.ToString();

                        RoomIndoor ri;

                        if (string.IsNullOrEmpty(indNOStr))
                        {
                            // 将NO为空的记录添加到项目的RoomIndoor中
                            ri = pbll.AddExchanger(roomID, inItem);
                            string prefix = SystemSetting.UserSetting.defaultSetting.ExchangerName;
                            ri.IndoorName = prefix + ri.IndoorNO.ToString();

                            ri.RoomName = "";
                            if (!string.IsNullOrEmpty(roomID))
                            {
                                Room room = pbll.GetRoom(roomID);
                                if (room != null)
                                {
                                    ri.RoomName = room.Name; //GetRoomNOString 
                                }
                            }

                        }
                        else
                        {
                            // NO不为空的记录则仅更新 IndoorItem
                            int indNO = Convert.ToInt32(indNOStr);
                            ri = pbll.GetExchanger(indNO);
                            ri.SetIndoorItemWithAccessory(inItem);
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
                        ri.IsAuto = !this.uc_CheckBox_Auto.Checked;
                        ri.IsDelete = false;
                        //记录显热档位 20161112 by Yunxiao lin
                        //ri.SHF_Mode = _shf_mode;
                        ri.FanSpeedLevel = _fanSpeedLevel;
                        ri.IsExchanger = true;

                        ri.DBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                        ri.WBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
                        ri.DBHeating = Unit.ConvertToSource(Convert.ToDouble(jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
                        ri.RHCooling = Convert.ToDouble(jctxtRH.Text);
                        // 自动选型（一次选择一台全热交换机）时需记录当前的容量需求数值
                        ri.RqCoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomCapC.Text), UnitType.POWER, utPower);
                        ri.RqSensibleHeat = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomSensiCapC.Text), UnitType.POWER, utPower);
                        ri.RqHeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomCapH.Text), UnitType.POWER, utPower);
                        ri.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomAirflow.Text), UnitType.AIRFLOW, utAirflow);
                        //ri.RqStaticPressure = Convert.ToDouble(jctxtRoomStaticPressure.Text);
                        ri.RqStaticPressure = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomStaticPressure.Text), UnitType.STATICPRESSURE, utPressure);
                        ri.RqFreshAir = Unit.ConvertToSource(Convert.ToDouble(jctxtRoomFA.Text), UnitType.AIRFLOW, utAirflow);

                        --count;
                    }
                }
                #endregion

                // 将预删除的 RoomIndoor 记录从项目对象中正式删除
                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    //删除全热交换机对应的Controller on20170901 by xyj
                    if (ri.IsDelete && (ri.ControlGroupID.Count > 0))
                    { 
                        thisProject.ControllerList.RemoveAll(p => ri.ControlGroupID.Contains(p.Id)); 
                    }
                }
                thisProject.ExchangerList.RemoveAll(c => { return c.IsDelete; });
                
                if (this.dgvSelectedIndoor.Rows.Count > 0)
                    SetNodeState(this.tvRoom.SelectedNode, true); // add on 20130822 clh
                //增加绑定Room信息
                BindSelectedList();

                if (this.JCFormMode == FormMode.EDIT)
                    DialogResult = DialogResult.OK;

                frmMain f = (frmMain)this.Owner;
                if (JCFormMode == FormMode.NEW)
                {
                    f.BindExchangerList();         // Add
                }
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
           // Global.SetDGVNotSortable(ref dgvStdIndoor);

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
                            List<RoomIndoor> list = pbll.GetSelectedExchangerByRoom(r.Id);
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

            BindRqCapacity();
        }

        private void SetNodeState(TreeNode nodeRoom, bool isHighlight)
        {
            if (isHighlight)
                nodeRoom.ForeColor = ColorAssigned;
            else
                nodeRoom.ForeColor = ColorOriginal;

           // nodeRoom.NodeFont = Global.SetFont(isHighlight);
            nodeRoom.NodeFont = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            //如何加大节点宽度。。。
        }



        // 绑定初始环境温度
        /// <summary>
        /// 绑定初始环境温度
        /// </summary>
        private void BindDBWB()
        {
            this.jctxtDBHeat.TextChanged += new EventHandler(jctxtDBHeat_TextChanged);
            this.jctxtDBCool.Leave += new EventHandler(jctxtDBCool_Leave);
            this.jctxtWBCool.Leave += new EventHandler(jctxtWBCool_Leave);
            this.jctxtRH.Leave += new EventHandler(jctxtRH_Leave);

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
           // DoCalculateRH();
        }

        // 重置控件状态
        /// <summary>
        /// 重置控件状态
        /// </summary>
        private void ResetControlState(bool isAuto)
        {
            bool editableItemSelected = (curRoom != null);

            // this.pbRemoveSelectedIndoor.Visible = !isAuto;
            this.pbRemoveSelectedIndoor.Visible = true;
            if (!editableItemSelected)
            {
                this.dgvSelectedIndoor.Rows.Clear();
            }
            //  this.pnlManualButton.Enabled = true;
            this.pnlAuto.Enabled = true;
            // this.pnlManualButton.Enabled = !isAuto && editableItemSelected;
            // this.pnlAuto.Enabled = editableItemSelected;
            this.pnlManualButton.Enabled = editableItemSelected;
            this.jcbtnOK.Enabled = editableItemSelected;
            this.jccmbPower.Enabled = editableItemSelected;
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

        // 绑定指定房间的已选全热交换机记录
        /// <summary>
        /// 绑定指定房间的已选全热交换机记录
        /// </summary>
        /// <param name="room"></param>
        private void BindSelectedList()
        {
            string roomId;
            if (curRoom != null)
            {
                roomId = curRoom.Id;
            }
            else
            {
                return;
            }

            pbll = new ProjectBLL(thisProject);
            this.dgvSelectedIndoor.Rows.Clear();
            List<RoomIndoor> list = pbll.GetSelectedExchangerByRoom(roomId);
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
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();

            if (roomNode.ForeColor == ColorOriginal) // 该房间尚未分配全热交换机
            {
                IsAuto = false;
                PassValidation = true;
                // DoAutoSelect();
                this.dgvSelectedIndoor.Rows.Clear();
                //jccmbProductType.Text = _series;
                jccmbProductType.SelectedValue = _series;

                //_series = jccmbProductType.SelectedValue.ToString();
                //jccmbPower.SelectedIndex = 0;
                //BindPowerlist();
                //BindStdExchangerList();
                DoCalculateSelectedSumCapacity();
            }
            else
            {
                List<RoomIndoor> list = new List<RoomIndoor>();
                if (curRoom != null)
                {
                    pbll = new ProjectBLL(thisProject);
                    list = pbll.GetSelectedExchangerByRoom(curRoom.Id);

                    if (list.Count == 0)
                        return false;

                    //选中已经分配全热交换机的房间，需要记录当前productType及series
                    _productType = list[0].IndoorItem.ProductType;
                    _series = list[0].IndoorItem.Series;
                    //curSelectType = list[0].IndoorItem.Type;

                    //  BindIndoorTypeList(false);

                    // 绑定当前已选全热交换机的类型
                    IsAuto = false;

                    string PowerValue = list[0].IndoorItem.ModelFull;
                    //截取最后四位的第一位 on 2017-07-26 by xyj
                    // PowerValue = PowerValue.Substring(PowerValue.Length - 4, 4).Substring(0, 1);
                    jccmbProductType.SelectedValue = _series;
                    BindPowerlist();
                    PowerValue = PowerValue.Substring(10, 1);
                    this.jccmbPower.SelectedValue = PowerValue;
                    //绑定当前已选全热交换机的显热模式 20161112 by Yunxiao Lin
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
                    shf = list[0].SHF;
                    shfComboBox.SelectedIndex = _fanSpeedLevel + 1;
                }


                //if (IsAuto)
                //{
                //    DoAutoSelect();
                //}
                //else
                //{

                //  BindSelectedList();
                //    DoCalculateSelectedSumCapacity();
                //    PassValidation = DoValidateCapacity();
                //}

                if (!IsAuto && list != null)
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
            BindStdExchangerList();
            this.uc_CheckBox_Auto.Checked = !IsAuto;
            this.uc_CheckBox_Manual.Checked = IsAuto;
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

        // 清空选中全热交换机记录的 Option
        /// <summary>
        /// 清空选中全热交换机记录的 Option
        /// </summary>
        private void DoClearOptions()
        {
            if (dgvSelectedIndoor.SelectedCells.Count > 0)
            {
                DataGridViewRow r = dgvSelectedIndoor.Rows[dgvSelectedIndoor.SelectedCells[0].RowIndex];
                if (string.IsNullOrEmpty(r.Cells[Name_Common.NO].Value.ToString()))
                    return;
                pbll = new ProjectBLL(thisProject);
                RoomIndoor ri = pbll.GetExchanger(Convert.ToInt32(r.Cells[Name_Common.NO].Value));
                //ri.OptionItem = null;
                r.Cells[Name_Common.ModelOption].Value = r.Cells[Name_Common.ModelFull].Value;

                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        // 删除已选的全热交换机记录，若其中包含了已添加到项目的记录，则弹出提示说明此处仅执行预删除，需点击OK按钮才从项目中彻底删除
        /// <summary>
        /// 删除已选的全热交换机记录，若其中包含了已添加到项目的记录，则弹出提示说明此处仅执行预删除，需点击OK按钮才从项目中彻底删除
        /// </summary>
        private void DoRemoveSelectedExchangerList(out bool success)
        {
            success = false;
            if (dgvSelectedIndoor.SelectedCells.Count > 0)
            {
                List<string> strNameList = new List<string>();
                List<string> strControllerList = new List<string>();
                List<int> rIndexList = new List<int>();
                string shareRindName = "";
                List<RoomIndoor> shareRiList = new List<RoomIndoor>();

                foreach (DataGridViewCell c in dgvSelectedIndoor.SelectedCells)
                {
                    if (rIndexList.Contains(c.RowIndex))
                        continue;
                    rIndexList.Add(c.RowIndex); // 获取要删除的行号list

                    string indName = dgvSelectedIndoor.Rows[c.RowIndex].Cells[Name_Common.Name].Value.ToString();

                    if (indName != "")
                    {
                        int indNo = Convert.ToInt32(dgvSelectedIndoor.Rows[c.RowIndex].Cells[Name_Common.NO].Value);
                        RoomIndoor indItem = pbll.GetExchanger(indNo);

                        strNameList.Add(indName);  // 获取已添加到项目的全热交换机的名称list

                        if (indItem.IndoorItemGroup != null && indItem.IndoorItemGroup.Count != 0)
                        {
                            shareRindName += indItem.IndoorName + ",";
                            shareRiList.Add(indItem);
                        }
                        if (indItem.ControlGroupID.Count>0)
                        {
                           // strControllerList.Add(indItem.ControlGroupID);                skm
                        }
                    }
                }

                if (strNameList.Count > 0)
                {
                    string names = "( ";
                    foreach (string s in strNameList)
                        names += s + " )";

                    // 提示对已经添加到项目的全热交换机仅执行预删除
                    if (JCMsg.ShowConfirmYesNoCancel(Msg.EXC_INFO_DEL) != DialogResult.Yes)
                        return;
                }

                if (strControllerList.Count > 0)
                {
                    foreach (string s in strControllerList)
                    {
                        //以绑定Controller提醒
                        if (JCMsg.ShowConfirmYesNoCancel(Msg.EXC_INFOCONTROL_DEL) != DialogResult.Yes)
                            return;
                    }
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
                    //DoRemoveSelectedExchnagerRow(r);
                    //Modify on 20160817 by Yunxiao Lin 先取出要删除的行，存入缓存List，不要删除，直接删除会引起后续处理Index的变化
                    selectedRowList.Add(r);
                }
                //Add on 20160817 by Yunxiao Lin 在这里统一处理要删除的行
                foreach (DataGridViewRow r in selectedRowList)
                {
                    DoRemoveSelectedExchangerRow(r);
                }
                success = true;
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        // 删除已选的全热交换机记录行，给对应的 RoomIndoor 对象加删除标记
        /// <summary>
        /// 删除已选的全热交换机记录行，给对应的 RoomIndoor 对象加删除标记
        /// </summary>
        /// <param name="r"></param>
        private void DoRemoveSelectedExchangerRow(DataGridViewRow r)
        {
            string indName = r.Cells[Name_Common.Name].Value.ToString();
            if (indName != "")
            {
                pbll = new ProjectBLL(thisProject);
                int indNO = Convert.ToInt32(r.Cells[Name_Common.NO].Value);
                RoomIndoor riItem = pbll.GetExchanger(indNO);
                riItem.IsDelete = true;
            }
            this.dgvSelectedIndoor.Rows.Remove(r);
        }

        double est_cool = 0;
        double est_heat = 0;
        double est_sh = 0;
        double airflow = 0;
        double staticPressure = 0;
        double shf = 0;
        // 对当前标准表的指定行执行容量估算
        /// <summary>
        /// 对当前标准表的指定行执行容量估算
        /// </summary>
        /// <param name="stdRow">标准表记录行</param>
        /// <param name="isAppend">是否附加到已选记录行，true则附加且count值可修改；false则添加唯一记录且count值不可修改</param>
        private void DoCalculateEstValue(DataGridViewRow stdRow)
        {
            string modelFull = stdRow.Cells[Name_Common.StdModelFull].Value.ToString();
            Indoor inItem = bll.GetItem(modelFull, _series, _series, _series);
            if (inItem != null)
                inItem.Series = _series;   //将当前的TTL Heat Exchanger列表   add on 20170710 xyj
            // 执行容量估算
            double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
            double db_c = inItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;//SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB;
            double db_h = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
            double wb_h = inItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;//SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB;

            //double est_sh_h = 0;
            //全热交换机估算容量不再返回显热
            //est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, out est_sh, false);
            est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
            //if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName))
            //    return;

            if (!inItem.ProductType.Contains(", CO"))
            {
                //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
                est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
                //if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName))
                //    return;
            }

            //显热改为由估算容量*SHF计算得到 20161111
            //if (_shf_mode.Equals("High"))
            //    shf = inItem.SHF_Hi;
            //else if (_shf_mode.Equals("Medium"))
            //    shf = inItem.SHF_Med;
            //else if (_shf_mode.Equals("Low"))
            //    shf = inItem.SHF_Lo;

            //if (shf == 0d)
            //    shf = inItem.SHF_Hi;
            shf = inItem.GetSHF(_fanSpeedLevel);
            est_sh = est_cool * shf;
            airflow = inItem.GetAirFlow(_fanSpeedLevel);
            //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
            staticPressure = inItem.GetStaticPressure();
            //取消风速等级 on 2017-07-14 by xyj
            //airflow = inItem.AirFlow;
            //staticPressure = Convert.ToDouble(inItem.ESP);
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
        // 计算当前房间已选全热交换机列表的估算容量总和
        /// <summary>
        /// 计算当前房间已选全热交换机列表的估算容量总和(Cooling)
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
                Indoor inItem = bll.GetItem(modelfull, _series, _series, _series);
                if (inItem != null)
                    inItem.Series = _series;
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
                    airflow = inItem.GetAirFlow(_fanSpeedLevel);
                    //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
                    staticPressure = inItem.GetStaticPressure();
                    //取消风速等级 on 2017-07-14 by xyj
                    //airflow = inItem.AirFlow;
                    //staticPressure = Convert.ToDouble(inItem.ESP);
                    tot_airflow += airflow * count;
                    tot_staticPressure += staticPressure * count;

                }
                double est_sh = est_cool * shf;   //原本逻辑显热直接取数据库，新逻辑为  capacity * SHF (not actual capacity * SHF) 20161111

                // 计算容量总和
                tot_cool += est_cool * count;
                tot_heat += est_heat * count;
                tot_sensible += est_sh * count;
            }

            // 绑定已选全热交换机估算容量总和
            this.jclblCapacityCValue.Text = Unit.ConvertToControl(tot_cool, UnitType.POWER, utPower).ToString("n1");
            this.jclblCapacityHValue.Text = Unit.ConvertToControl(tot_heat, UnitType.POWER, utPower).ToString("n1");
            this.jclblSensiableValue.Text = Unit.ConvertToControl(tot_sensible, UnitType.POWER, utPower).ToString("n1");
            this.jclblAirflowValue.Text = Unit.ConvertToControl(tot_airflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //this.jclblStaticPressureValue.Text = tot_staticPressure.ToString("n0");
            this.jclblStaticPressureValue.Text = Unit.ConvertToControl(tot_staticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
            this.jclblFAValue.Text = Unit.ConvertToControl(tot_fa, UnitType.AIRFLOW, utAirflow).ToString("n0");
        }

        // 校验选择的全热交换机是否满足房间需求
        /// <summary>
        /// 校验选择的全热交换机是否满足房间需求,20140903 增加0.9的余量 
        /// </summary>
        private bool DoValidateCapacity()
        {

            // 手动选型，每次都需要校验需求
            toolStripStatusLabel1.Text = "";
            if (!IsAuto && this.dgvSelectedIndoor.Rows.Count > 0)
            {
                if (tot_airflow < curRoom.AirFlow)
                {
                    if (tot_airflow >= curRoom.AirFlow * 0.9)
                    {
                        if (!(JCMsg.ShowConfirmYesNoCancel(Msg.Exc_WARN_CAPFlowAir) == DialogResult.Yes))
                        {
                            toolStripStatusLabel1.Text = Msg.Exc_NOTMEET_AirFlow;
                            return false;
                        }
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = Msg.Exc_NOTMEET_AirFlow;
                        return false;
                    }

                }
                if (tot_staticPressure < curRoom.StaticPressure)
                {
                    if (tot_staticPressure >= curRoom.StaticPressure * 0.9)
                    {
                        if (!(JCMsg.ShowConfirmYesNoCancel(Msg.Exc_WARN_CAPESP) == DialogResult.Yes))
                        {
                            toolStripStatusLabel1.Text = Msg.Exc_NOTMEET_ESP;
                            return false;
                        }
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = Msg.Exc_NOTMEET_ESP;
                        return false;
                    }
                }

                //if (thisProject.IsCoolingModeEffective)
                //{
                //    if (tot_cool < curRoom.RqCapacityCool * 1 || tot_sensible < curRoom.SensibleHeat * 1 || tot_airflow < curRoom.AirFlow * 1 || tot_staticPressure < curRoom.StaticPressure)
                //    {
                //        if (tot_cool >= curRoom.RqCapacityCool * 0.9)
                //        {
                //if (!(JCMsg.ShowConfirmYesNoCancel(Msg.IND_WARN_CAPLower2) == DialogResult.Yes))
                //{
                //   toolStripStatusLabel1.Text = Msg.IND_NOTMEET_COOLING;
                //    return false;
                //}
                //        }
                //        else
                //        {
                //            toolStripStatusLabel1.Text = Msg.IND_NOTMEET_COOLING;
                //            return false;
                //        }
                //    }
                //}
                ////if (thisProject.IsHeatingModeEffective)
                //// 由于一个Project中可能存在多个不同的ProductType，因此单凭IsHeatingModeEffective无法确定一个System是否需要制热。
                //// 如果productType中包含", CO"，那么这个System是单冷系统，就算IsHeatingModeEffective=true，也不需要制热功能。 20160826 by Yunxiao Lin
                //if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
                //{
                //    if (tot_heat < curRoom.RqCapacityHeat * 1)
                //    {
                //        if (tot_heat >= curRoom.RqCapacityHeat * 0.9)
                //        {
                //            if (!(JCMsg.ShowConfirmYesNoCancel(Msg.IND_WARN_CAPLower2) == DialogResult.Yes))
                //            {
                //                toolStripStatusLabel1.Text = Msg.IND_NOTMEET_HEATING;
                //                return false;
                //            }
                //        }
                //        else
                //        {
                //            toolStripStatusLabel1.Text = Msg.IND_NOTMEET_HEATING;
                //            return false;
                //        }
                //    }
                //}

            }
            return true;
        }

        // 根据当前房间信息自动选型
        /// <summary>
        /// 根据当前房间信息自动选型
        /// </summary>
        private void DoAutoSelect()
        {
            BindSelectedList(); // 绑定当前房间已选的全热交换机记录

            // 若当前房间尚未分配全热交换机，则先新增一空行记录
            if (this.dgvSelectedIndoor.Rows.Count == 0)
                addToSelectedRow(null, true);
            else
            {
                // 若已分配过全热交换机，则仅保留第一行记录
                for (int i = dgvSelectedIndoor.Rows.Count - 1; i > 0; --i)
                    DoRemoveSelectedExchangerRow(dgvSelectedIndoor.Rows[i]);
            }
            //string type = this.jccmbType.Text.Trim();
            // 遍历当前类型下的标准表记录，查找最适合的全热交换机
            bool isOK = false;
            foreach (DataGridViewRow stdRow in this.dgvStdIndoor.Rows)
            {

                // bool isPass = true;

                // 速度优化：先比较标准容量数值 20130802 clh
                //double std_cool = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value);
                //double std_heat = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value);
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower); 
                if (curRoom != null)
                {
                    //if (thisProject.IsCoolingModeEffective && std_cool < curRoom.RqCapacityCool)
                    //    isPass = false;
                    //if (thisProject.IsHeatingModeEffective && std_heat < curRoom.RqCapacityHeat)
                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭IsHeatingModeEffective无法确定一个System是否需要制热。
                    // 如果productType中包含", CO"，那么这个System是单冷系统，就算IsHeatingModeEffective=true，也不需要制热功能。 20160826 by Yunxiao Lin
                    // if (thisProject.IsHeatingModeEffective && std_heat < curRoom.RqCapacityHeat)
                    //  isPass = false;

                    // if (!isPass || !autoCompare(stdRow, curRoom))
                    if (!autoCompare(stdRow, curRoom))
                        continue;
                    else
                        isOK = true;
                }


                stdRow.Selected = true;
                if (thisProject.BrandCode == "Y")
                    this.dgvStdIndoor.CurrentCell = stdRow.Cells[Name_Common.StdModelFull_York]; // 定位标准数据中的选中行
                else if (thisProject.BrandCode == "H")
                {
                    this.dgvStdIndoor.CurrentCell = stdRow.Cells[Name_Common.StdModelFull_Hitachi];
                }

                //updateSelectedRow(stdRow, dgvSelectedIndoor.Rows[0], true);

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
                this.toolStripStatusLabel1.Text = Msg.EXC_NOTMATCH;
                PassValidation = false;
            }
            else
                PassValidation = true;
        }

        /// <summary>
        /// 自动选型模式，比较指定标准行的全热交换机估算容量与需求容量，满足则返回true
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stdRow"></param>
        /// <param name="room">当前房间对象</param>
        /// <returns></returns>
        private bool autoCompare(DataGridViewRow stdRow, Room room)
        {
            bool pass = true;
            // 计算估算容量
            DoCalculateEstValue(stdRow);

            if (airflow < room.AirFlow || staticPressure < room.StaticPressure)
            {
                pass = false;
            }
            //if (thisProject.IsCoolingModeEffective)
            //{
            //    if (est_cool < room.RqCapacityCool || est_sh < room.SensibleHeat || airflow < room.AirFlow || staticPressure < room.StaticPressure)
            //        pass = false;
            //} 
            //if (thisProject.IsHeatingModeEffective)
            //{
            //    if (est_heat < room.RqCapacityHeat)
            //        pass = false;
            //}

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
            DoCalculateEstValue(stdRow);

            //将估算容量与当前需求进行比较
            if (airflow < area.FreshAir)
                pass = false;
            return pass;
        }

        /// <summary>
        /// 将选中的标准行记录，添加到已选全热交换机列表
        /// </summary>
        /// <param name="stdRow">选中的标准行记录，若为null，则已选记录中新增一空行</param>
        /// <param name="isAuto">是否是自动选型模式</param>
        private void addToSelectedRow(DataGridViewRow stdRow, bool isAuto)
        {
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
                newRow.Cells[Name_Common.ModelFull].Value = stdRow.Cells[Name_Common.StdModelFull].Value;
                newRow.Cells[Name_Common.ModelOption].Value = newRow.Cells[Name_Common.ModelFull].Value;
                newRow.Cells[Name_Common.ModelFull_York].Value = stdRow.Cells[Name_Common.StdModelFull_York].Value;
                newRow.Cells[Name_Common.ModelFull_Hitachi].Value = stdRow.Cells[Name_Common.StdModelFull_Hitachi].Value;
            }

            newRow.Cells[Name_Common.ModelOption].Value = newRow.Cells[Name_Common.ModelFull].Value;
            newRow.Cells[Name_Common.Count].Value = "1";
            //newRow.Cells[Name_Common.Type].Value = this.jccmbProductType.Text.Trim();
            newRow.Cells[Name_Common.Type].Value = this.jccmbProductType.SelectedValue.ToString();
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
            pbll = new ProjectBLL(thisProject);
            // 如果更新已选的全热交换机，则需要清空原先已选的 Option
            if (selRow.Cells[Name_Common.ModelFull].Value.ToString() != stdRow.Cells[Name_Common.StdModelFull].Value.ToString()
                && !string.IsNullOrEmpty(selRow.Cells[Name_Common.NO].Value.ToString()))
            {
                RoomIndoor ri = pbll.GetIndoor(Convert.ToInt32(selRow.Cells[Name_Common.NO].Value));
                //ri.OptionItem = null;
            }

            selRow.Cells[Name_Common.ModelFull].Value = stdRow.Cells[Name_Common.StdModelFull].Value;
            selRow.Cells[Name_Common.ModelOption].Value = selRow.Cells[Name_Common.ModelFull].Value;
            selRow.Cells[Name_Common.ModelFull_York].Value = stdRow.Cells[Name_Common.StdModelFull_York].Value;
            selRow.Cells[Name_Common.ModelFull_Hitachi].Value = stdRow.Cells[Name_Common.StdModelFull_Hitachi].Value;
            ////将标准全热交换机中的SHF系数更新到已选全热交换机列表 20161111 by Yunxiao Lin
            //selRow.Cells[Name_Common.SHF_Hi].Value = stdRow.Cells[Name_Common.SHF_Hi].Value;
            //selRow.Cells[Name_Common.SHF_Med].Value = stdRow.Cells[Name_Common.SHF_Med].Value;
            //selRow.Cells[Name_Common.SHF_Lo].Value = stdRow.Cells[Name_Common.SHF_Lo].Value;

            //selRow.Cells[Name_Common.Count].Value = "1";
            //selRow.Cells[Name_Common.Type].Value = this.jccmbType.Text.Trim();
            //从unitTypeList下拉菜单取出来的值需要先去掉厂名 20161118 by Yunxiao Lin
            //  string type = this.jccmbType.Text.Trim();
            //string type = "";
            //int i = type.IndexOf("-");
            //if (i > 0)
            //    type = type.Substring(0, i);
            //selRow.Cells[Name_Common.Type].Value = this.jccmbProductType.Text.Trim();
            selRow.Cells[Name_Common.Type].Value = this.jccmbProductType.SelectedValue.ToString();
            selRow.Cells[Name_Common.Capacity_C].Value = est_cool;
            selRow.Cells[Name_Common.Capacity_H].Value = est_heat;
            selRow.Cells[Name_Common.SensibleHeat].Value = est_sh;
            //selRow.Cells[Name_Common.AirFlow].Value = airflow;
            selRow.Cells[Name_Common.ProductType].Value = _productType;

            if (this.dgvSelectedIndoor.Columns[Name_Common.Count] != null)
                this.dgvSelectedIndoor.Columns[Name_Common.Count].ReadOnly = isAuto;

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


        #endregion

        private void jccmbPower_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //_series = jccmbProductType.Text.ToString();
            _series = jccmbProductType.SelectedValue.ToString();
            BindStdExchangerList();

            //if (curSelectType == this.jccmbType.Text)
            //if (curSelectType == this.jccmbPower.Text && _productType == jccmbProductType.SelectedValue.ToString())
            //    return;

            //curSelectType = this.jccmbType.Text;

            //// 检验需求信息是否填写
            //if (!this.JCValidateGroup(this.pnlRoom_2))
            //    return;

            //if (IsAuto)
            //{
            //    DoAutoSelect();
            //}
        }

        private void jccmbProductType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //_series = jccmbProductType.Text.ToString();
            _series = jccmbProductType.SelectedValue.ToString();
            BindPowerlist();
            BindStdExchangerList();
        }

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

        private void dgvStdIndoor_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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

        private void frmExchangerByRoom_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil.Dispose();
        }
    }
}
