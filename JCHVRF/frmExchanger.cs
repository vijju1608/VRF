using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
    public partial class frmExchanger : JCBase.UI.JCForm
    {

        Project thisProject;

        ProjectBLL pbll;
        IndoorBLL bll;
        RoomIndoor curRI;
        bool IsAuto;
        string ut_airflow;
        string ut_power;
        string ut_temperature;
        string ut_pressure;
        double rq_cool = 0;
        double rq_heat = 0;
        double rq_sensiable = 0;
        double rq_airflow = 0;
        double rq_StaticPressure = 0;
        double rq_fa = 0;
        double est_cool = 0;
        double est_heat = 0;
        double est_sh = 0;
        double airflow = 0;
        double staticPressure = 0;
        string _series; 
        int _fanSpeedLevel = -1;
        string newRoom = "New Room";
        string noRoom = "No Room";
        string defaultRoom;
        private double outdoorCoolingDB = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
        private double outdoorHeatingWB = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;
        private double outdoorCoolingIW = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
        private double outdoorHeatingIW = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;

        double roomDB = 0;
        double roomWB = 0;
        double roomRH = 0;

        UndoRedo.UndoRedoHandler UndoRedoUtil = null;

        Trans trans = new Trans();    //翻译初始化

        public frmExchanger()
        {
            InitializeComponent();
        }

        /// 绑定SHF列表 20161112 by Yunxiao Lin
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
                jccmbExchanger.DataSource = dtList;
                //this.jccmbExchanger.DisplayMember = "UnitType";
                //this.jccmbExchanger.ValueMember = "UnitType";
                this.jccmbExchanger.DisplayMember = "Trans_Name";
                this.jccmbExchanger.ValueMember = colName;

                if (this.JCFormMode == FormMode.EDIT)
                {
                    this.jccmbExchanger.SelectedValue = curRI.IndoorItem.Series;
                    //_series = jccmbExchanger.Text.ToString();
                }
                else
                {
                    jccmbExchanger.SelectedIndex = 0;
                    //_series = jccmbExchanger.Text.ToString();
                }

                _series = jccmbExchanger.SelectedValue.ToString();
                BindPowerlist();
                _fanSpeedLevel = -1;
            }
        }

        private void BindPowerlist()
        {
            DataTable dt = Global.InitPowerList(thisProject, _series);
            jccmbPower.DataSource = dt;
            this.jccmbPower.DisplayMember = "PowerKey";
            this.jccmbPower.ValueMember = "PowerValue";
            jccmbPower.SelectedIndex = 0;
        }

        /// <summary>
        /// 构造函数，新增
        /// </summary>
        /// <param name="thisProj"> 当前项目对象 </param>
        public frmExchanger(Project thisProj)
        {
            InitializeComponent();
            this.JCSetLanguage();
            thisProject = thisProj;
            JCFormMode = FormMode.NEW;
            curRI = new RoomIndoor();
            IsAuto = false;
        }

        /// <summary>
        /// 构造函数，编辑
        /// </summary>
        /// <param name="ri"> 当前室内机 </param>
        /// <param name="thisProj"> 当前项目对象 </param>
        public frmExchanger(RoomIndoor ri,Project thisProj)
        {
            InitializeComponent();
            this.JCSetLanguage(); 
            thisProject = thisProj;
            pbll = new ProjectBLL(thisProject);
            JCFormMode = FormMode.EDIT; 
            curRI = ri;
            if (!string.IsNullOrEmpty(ri.RoomID))
            {
                cboTreeRoom.Name = ri.RoomID;
                cboTreeRoom.Text = pbll.GetFloorAndRoom(ri.RoomID);
            }
            else
            {
                cboTreeRoom.Name = "0";
                cboTreeRoom.Text = noRoom;
            }
            IsAuto = curRI.IsAuto;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                ///
                jcbtnOK.Cursor = Cursors.WaitCursor;
                DoOK();
                UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by xyj 20170822
            }
            catch (Exception exc)
            {
                JCMsg.ShowWarningOK(exc.Message);
            }
            finally
            {
                jcbtnOK.Cursor = Cursors.Default;
            }
        }
 
        // 根据当前窗口的模式添加、更新全热交换机记录
        /// <summary>
        /// 根据当前窗口的模式添加、更新全热交换机记录
        /// </summary>
        private void DoOK()
        {
            if (dgvStdExchanger.SelectedRows.Count == 0)
                return;
            //如果没有合适的机组，返回不增加全热交换机
            if (this.jclblSelectIndoor.Text == "") 
                return;

            string newRoomId = "";
            string roomId = cboTreeRoom.Name;
            string room = cboTreeRoom.Text;
            string floorId = "";
            if (roomId != "0")
            {
                if (roomId.Contains("_0") && room.Contains(newRoom))
                {
                    string[] strr = roomId.Split('_');
                    floorId = strr[0];
                    newRoomId = SaveRoom(GetFloor(floorId));
                }
                else
                {
                    newRoomId = roomId;
                }
            }
            if (!this.JCValidateGroup(pnlRoomRq))
            {
                return;
            }
            DataGridViewRow r = dgvStdExchanger.SelectedRows[0];
            string modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
            Indoor indItem = bll.GetItem(modelfull, _series, _series, _series);
            if (indItem != null)
                indItem.Series = _series;   //将当前的TTL Heat Exchanger列表   add on 20170710 xyj
            if (this.JCFormMode == FormMode.NEW)
            {
                curRI = pbll.AddExchanger(newRoomId, indItem);
                if (!string.IsNullOrEmpty(newRoomId))
                {
                    curRI.RoomName = pbll.GetRoom(newRoomId).Name;
                }
            }
            else
            {
                curRI.SetIndoorItemWithAccessory(indItem);
                if (!string.IsNullOrEmpty(newRoomId))
                {
                    curRI.RoomID = newRoomId;
                    curRI.RoomName = pbll.GetRoom(newRoomId).Name;
                }
            } 
            curRI.IndoorName = this.jctxtName.Text;
            //curRI.IndoorFullName = thisProject.BrandCode == "Y" ? curRI.IndoorName + "[" + curRI.IndoorItem.Model_York + "]" : curRI.IndoorName + "[" + curRI.IndoorItem.Model_Hitachi + "]";  //记录indoor对象的indoor名
            //curRI.IndoorItem.IndoorName = curRI.IndoorFullName;
            curRI.IsAuto = IsAuto;
            curRI.IsDelete = false;
           // curRI.AirFlow = Unit.ConvertToSource(Convert.ToDouble(indItem.AirFlow), UnitType.AIRFLOW, ut_temperature);
            curRI.DBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtDBCool.Text), UnitType.TEMPERATURE, ut_temperature);
            curRI.WBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);
            curRI.DBHeating = Unit.ConvertToSource(Convert.ToDouble(jctxtDBHeat.Text), UnitType.TEMPERATURE, ut_temperature);
            curRI.RHCooling = Convert.ToDouble(jctxtRH.Text);
            // 自动选型（一次选择一台全热交换机）时需记录当前的容量需求
            if (IsAuto)
            {
                curRI.RqCoolingCapacity = rq_cool;
                curRI.RqSensibleHeat = rq_sensiable;
                curRI.RqHeatingCapacity = rq_heat;
                curRI.RqAirflow = rq_airflow;
                curRI.RqStaticPressure = rq_StaticPressure;
                curRI.RqFreshAir = rq_fa;
            }

            curRI.CoolingCapacity = est_cool;
            curRI.HeatingCapacity = est_heat;
            curRI.SensibleHeat = est_sh;
            curRI.FanSpeedLevel = _fanSpeedLevel;
            curRI.IsExchanger = true;
            
            // 实现连续添加
            if (this.JCFormMode == FormMode.EDIT)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            frmMain f = (frmMain)this.Owner;
             if (JCFormMode == FormMode.NEW)
             {
                f.BindExchangerList();         // Add
            }
            
            BindNextExchangerName();

            if (newRoomId != "0" && !string.IsNullOrEmpty(newRoomId))
            {
                BindRoom();
                cboTreeRoom.Name = newRoomId; 
                cboTreeRoom.Text =pbll.GetFloorAndRoom(newRoomId);
            }
            else
            {
                cboTreeRoom.Name = "0";
                cboTreeRoom.Text = noRoom;
            }
        }

        private void frmExchanger_Load(object sender, EventArgs e)
        {
            //初始化页面
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
        /// 初始化界面 add by xyj 20170710
        /// </summary>
        protected void Initialization()
        {
            this.JCCallValidationManager = true;    // 启用界面控件验证
            toolStripStatusLabel1.Text = "";

            pbll = new ProjectBLL(thisProject);
            //bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode);
            bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
          
            InitDGV();
            BindUnit();

             BindToControl();
             BindEvent();
             BindRoom();

             if (this.JCFormMode == FormMode.NEW)
             {
                 if (thisProject.SubRegionCode == "EU_E")
                 {
                     int no = pbll.GetMaxFloorNO();
                     Floor floor = GetFloorByNO(no);
                     cboTreeRoom.Name = floor.Id + "_0";
                     cboTreeRoom.Text = newRoom;
                 }
                 else
                 {
                     cboTreeRoom.Name = "0";
                     cboTreeRoom.Text = noRoom;
                 }
             }
            this.uc_CheckBox_Auto.TextString = ShowText.Auto;
            this.uc_CheckBox_Manual.TextString = ShowText.Manual;
            ResetControlState(this.uc_CheckBox_Auto.Checked);
            cboTreeRoom.MyChange += new JCBase.UI.ComboBoxTreeView.MyChangeEventHandler(UpdateRoomName);
        }

        /// <summary>
        /// 更新房间名称
        /// </summary>
        void UpdateRoomName()
        {
            if (!string.IsNullOrEmpty(cboTreeRoom.Room_Id) && !string.IsNullOrEmpty(cboTreeRoom.Room_Name))
            {
                ModifyRoomName(cboTreeRoom.Room_Id, cboTreeRoom.Room_Name);
            }

        }

        public bool ModifyRoomName(string roomId, string roomName)
        {
            //更新房间信息
            if (!string.IsNullOrEmpty(roomId))
            {
                foreach (Floor f in thisProject.FloorList)
                {
                    foreach (Room ri in f.RoomList)
                    {
                        if (ri.Id == roomId)
                        {
                            ri.Name = roomName;
                        }
                    }
                }
            }

            //更新室内机对应的RoomName
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (ri.RoomID == roomId)
                {
                    ri.RoomName = roomName;
                }
            }
            //更新全热交换机对应的RoomName
            foreach (RoomIndoor ri in thisProject.ExchangerList)
            {
                if (ri.RoomID == roomId)
                {
                    ri.RoomName = roomName;
                }
            } 
            return false;

        }

        /// <summary>
        /// 筛选掉没有需求的空房间 on20170926 by xyj
        /// </summary>
        /// <returns></returns>
        public List<Floor> FilterEmptyRoom()
        {
            List<Floor> list = new List<Floor>();
            List<Floor> Floorlist = thisProject.FloorList.DeepClone();
            if (thisProject.FloorList.Count > 0)
            {

                foreach (Floor f in thisProject.FloorList)
                {

                    foreach (Room ri in f.RoomList)
                    {
                        if (!pbll.isEmptyRoom(ri.Id))
                        {
                            RemoveRoom(ri.Id, Floorlist);
                        }
                    }

                }

            }

            return Floorlist;
        }
        public void RemoveRoom(string id, List<Floor> Floorlist)
        {
            foreach (Floor f in Floorlist)
            {
                foreach (Room ri in f.RoomList)
                {
                    if (id == ri.Id)
                    {
                        f.RoomList.Remove(ri);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// 绑定楼层房间
        /// </summary>
        private void BindRoom()
        {
            TreeView treeView = this.cboTreeRoom.TreeView;
           
            treeView.Nodes.Clear();
            TreeNode rootZ = new TreeNode();

            //节点显示不基于房间
            rootZ.Text = noRoom;
            rootZ.Name = "0"; 
            treeView.Nodes.Add(rootZ); 
            // 筛选掉没有需求的空房间
            List<Floor> emptyRoom = FilterEmptyRoom();
            if (emptyRoom.Count > 0)
            {
                foreach (Floor f in emptyRoom)
                {
                    TreeNode root = new TreeNode();

                    root.Text = f.Name.ToString();
                    root.Name = f.Id.ToString();
                    //增加树的根节点   
                    treeView.Nodes.Add(root);
                    root.ExpandAll();
                    addNode(root, f.RoomList, f.Id);
                }
            }
            else
            {
                //创建楼层
                Floor floor = new Floor();
                floor.Id = Guid.NewGuid().ToString("N");
                floor.NO = 1;
                floor.Name = SystemSetting.UserSetting.defaultSetting.FloorName + "1";
                floor.Height = SystemSetting.UserSetting.defaultSetting.RoomHeight;
                floor.ParentId = 1;
                pbll.AddFloor(floor); 
                TreeNode root = new TreeNode();
                root.Text = floor.Name.ToString();
                root.Name = floor.Id.ToString(); 
                treeView.Nodes.Add(root);
                root.ExpandAll();
                TreeNode sNode = new TreeNode();
                //创建楼层节点下 NewRoom
                sNode.Text = newRoom;
                sNode.Name = floor.Id + "_0"; 
                root.Nodes.Add(sNode);
            }

        }


        /// <summary>  
        /// 递规添加TreeView节点  
        /// </summary>  
        /// <param name="node"></param>  
        /// <param name="parentID"></param>  
        public void addNode(TreeNode node, List<Room> list, string floorId)
        {

            TreeNode sNode = new TreeNode();
            sNode.Text = newRoom;
            sNode.Name = floorId + "_0";  
            node.Nodes.Add(sNode);
            foreach (Room ri in list)
            {
                TreeNode subNode = new TreeNode();
                subNode.Text = ri.Name;
                subNode.Name = ri.Id;  
                node.Nodes.Add(subNode);
            }
            node.ExpandAll();
        }

        // 控件事件绑定
        /// <summary>
        /// 控件事件绑定
        /// </summary>
        private void BindEvent()
        {
            this.jctxtRoomCapC.TextChanged += new EventHandler(jctxtRoomCapC_TextChanged);
            this.jctxtRoomCapH.TextChanged += new EventHandler(jctxtRoomCapC_TextChanged);
            this.jctxtRoomSensiCapC.TextChanged += new EventHandler(jctxtRoomCapC_TextChanged);
            this.jctxtRoomAirflow.TextChanged += new EventHandler(jctxtRoomCapC_TextChanged);
            this.jctxtRoomStaticPressure.TextChanged += new EventHandler(jctxtRoomStaticPressure_TextChanged);
            this.jctxtRoomFA.TextChanged += new EventHandler(jctxtRoomCapC_TextChanged);
           // this.jctxtDBCool.TextChanged += new EventHandler(jctxtDBCool_TextChanged);
            this.jctxtDBHeat.TextChanged += new EventHandler(jctxtDBHeat_TextChanged);
          //  this.jctxtWBCool.TextChanged += new EventHandler(jctxtWBCool_TextChanged);
          //  this.jctxtRH.TextChanged += new EventHandler(jctxtRH_TextChanged);

            this.jctxtDBCool.Leave += new EventHandler(jctxtDBCool_Leave);

            this.jctxtWBCool.Leave += new EventHandler(jctxtWBCool_Leave);
            this.jctxtRH.Leave += new EventHandler(jctxtRH_Leave);
            this.uc_CheckBox_Auto.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            this.uc_CheckBox_Manual.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
        }


         // 窗口加载时控件初始值绑定
        /// <summary>
        /// 窗口加载时控件初始值绑定
        /// </summary>
        private void BindToControl()
        {
            BindPowerMode();//绑定电影模式选择下拉框 20170726 by xyj
            BindStdExchangerList(); 
            ResetControlState(IsAuto);
            BindShfMode();
            this.jctxtDBCool.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctxtDBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(30, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctxtWBCool.JCMinValue = float.Parse(Unit.ConvertToControl(14, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));  //11
            this.jctxtWBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctxtDBHeat.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctxtDBHeat.JCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));  //30
            this.jctxtRH.JCMinValue = float.Parse("13");
            this.jctxtRH.JCMaxValue = float.Parse("100");
            if (JCFormMode == FormMode.NEW)
            {
                BindNextExchangerName();
                double dbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB;
                double wbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
                double dbHeat = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;

                double rhCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH;
                this.jctxtDBCool.Text = Unit.ConvertToControl(dbCool, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtWBCool.Text = Unit.ConvertToControl(wbCool, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtDBHeat.Text = Unit.ConvertToControl(dbHeat, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtRH.Text = rhCool.ToString("n0");
                curRI.DBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtDBCool.Text), UnitType.TEMPERATURE, ut_temperature);
                curRI.WBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);
                curRI.WBHeating = Unit.ConvertToSource(Convert.ToDouble(jctxtDBHeat.Text), UnitType.TEMPERATURE, ut_temperature);
                curRI.RHCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtRH.Text), UnitType.TEMPERATURE, ut_temperature);
            }
            else if (JCFormMode == FormMode.EDIT)
            { 
                this.jctxtName.Text = curRI.IndoorName;
                this.uc_CheckBox_Auto.Checked = curRI.IsAuto;
                this.uc_CheckBox_Manual.Checked = !curRI.IsAuto;
              
                string PowerValue = curRI.IndoorItem.ModelFull;
                //截取最后四位的第一位 on 2017-07-26 by xyj
               // PowerValue = PowerValue.Substring(PowerValue.Length - 4, 4).Substring(0,1);
                PowerValue = PowerValue.Substring(10, 1);
                this.jccmbPower.SelectedValue = PowerValue;
                this.jccmbExchanger.SelectedValue = curRI.IndoorItem.Series;
                BindStdExchangerList();
                this.jctxtDBCool.Text = Unit.ConvertToControl(curRI.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtWBCool.Text = Unit.ConvertToControl(curRI.WBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtDBHeat.Text = Unit.ConvertToControl(curRI.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                if (curRI.RHCooling != 0)
                    this.jctxtRH.Text = curRI.RHCooling.ToString("n0");
                else
                    DoCalculateRH();
                //绑定SHF档位  on 2017-08-21 by xyj
                _fanSpeedLevel = curRI.FanSpeedLevel;
                this.shfComboBox.SelectedIndex = _fanSpeedLevel + 1;

                if (curRI.IsAuto) // 绑定自动选型时的容量需求信息
                {
                    this.jctxtRoomCapC.Text = Unit.ConvertToControl(curRI.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1");
                    this.jctxtRoomCapH.Text = Unit.ConvertToControl(curRI.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1");
                    this.jctxtRoomAirflow.Text = Unit.ConvertToControl(curRI.RqAirflow, UnitType.AIRFLOW, ut_airflow).ToString("n1");
                    //this.jctxtRoomStaticPressure.Text = curRI.RqStaticPressure.ToString("n1");
                    this.jctxtRoomStaticPressure.Text = Unit.ConvertToControl(curRI.RqStaticPressure, UnitType.STATICPRESSURE, ut_pressure).ToString("n2");
                    this.jctxtRoomSensiCapC.Text = Unit.ConvertToControl(curRI.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1");
                    this.jctxtRoomFA.Text = Unit.ConvertToControl(curRI.RqFreshAir, UnitType.AIRFLOW, ut_airflow).ToString("n1");
                } 
                foreach (DataGridViewRow r in dgvStdExchanger.Rows)
                {
                    if (r.Cells[Name_Common.StdModelFull].Value.ToString() == curRI.IndoorItem.ModelFull)
                    {
                        r.Selected = true;
                        // Add on 20170710 xyj 定位到选中行
                        if (thisProject.BrandCode == "Y")
                            this.dgvStdExchanger.CurrentCell = r.Cells[1];
                        else
                            this.dgvStdExchanger.CurrentCell = r.Cells[2];
                        break;
                    }
                } 
            }
            BindRoomCooling();
           // DoCalculateRH();
        }

        // 绑定下一个全热交换机Name
        /// <summary>
        /// 绑定下一个全热交换机Name
        /// </summary>
        private void BindNextExchangerName()
        {
            string prefix = SystemSetting.UserSetting.defaultSetting.ExchangerName;
            this.jctxtName.Text = prefix + pbll.GetNextExchangerNo().ToString();
        }

        // 计算相对湿度
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateRH()
        {
            if (!string.IsNullOrEmpty(this.jctxtDBCool.Text) && !string.IsNullOrEmpty(this.jctxtWBCool.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBCool.Text), UnitType.TEMPERATURE, ut_temperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
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
            //this.jctxtDBCool.TextChanged -= new EventHandler(jctxtDBCool_TextChanged); 
            //this.jctxtWBCool.TextChanged -= new EventHandler(jctxtWBCool_TextChanged);
            //this.jctxtRH.TextChanged -= new EventHandler(jctxtRH_TextChanged);

            if (!string.IsNullOrEmpty(this.jctxtDBCool.Text) && !string.IsNullOrEmpty(this.jctxtWBCool.Text) && !string.IsNullOrEmpty(this.jctxtRH.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBCool.Text), UnitType.TEMPERATURE, ut_temperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);
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
            //this.jctxtDBCool.TextChanged += new EventHandler(jctxtDBCool_TextChanged);
            //this.jctxtWBCool.TextChanged += new EventHandler(jctxtWBCool_TextChanged);
            //this.jctxtRH.TextChanged += new EventHandler(jctxtRH_TextChanged);
        }

        private void shfComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fanSpeedLevel = shfComboBox.SelectedIndex - 1;
            string Modelfull = "";
            if (dgvStdExchanger.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvStdExchanger.SelectedRows[0];
                Modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
            }
            BindStdExchangerList();
            for (int i = 0; i <= dgvStdExchanger.Rows.Count - 1; ++i)
            {
                DataGridViewRow ri = dgvStdExchanger.Rows[i];
                if (ri.Cells[Name_Common.StdModelFull].Value.ToString() == Modelfull)
                {
                    ri.Selected = true;
                    break;
                }
            }  
            dgvStdExchanger_SelectionChanged(null, null);
        }

        // 绑定标准表
        /// <summary>
        /// 绑定标准表
        /// </summary>
        private void BindStdExchangerList()
        {
            this.dgvStdExchanger.Rows.Clear();
            string type = Convert.ToString(this.jccmbPower.SelectedValue);
            if (string.IsNullOrEmpty(type)) return;
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
                        shf = inItem.GetSHF(_fanSpeedLevel);

                        airflow = inItem.GetAirFlow(_fanSpeedLevel);
                        //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
                        staticPressure = inItem.GetStaticPressure();
                    }
                    double est_sh = est_cool * shf;
                    this.dgvStdExchanger.Rows.Add(dr["ModelFull"].ToString(),
                         dr["Model_York"].ToString(),
                         dr["Model_Hitachi"].ToString(),
                         Unit.ConvertToControl(est_cool, UnitType.POWER, ut_power).ToString("n1"),
                         Unit.ConvertToControl(est_sh, UnitType.POWER, ut_power).ToString("n1"),
                         Unit.ConvertToControl(airflow, UnitType.AIRFLOW, ut_airflow).ToString("n0"),
                         //staticPressure.ToString("n0"),
                         Unit.ConvertToControl(staticPressure, UnitType.STATICPRESSURE, ut_pressure).ToString("n2"),
                         Unit.ConvertToControl(est_heat, UnitType.POWER, ut_power).ToString("n1"),
                          Unit.ConvertToControl(fa, UnitType.AIRFLOW, ut_airflow).ToString("n0"),
                         dr["TypeImage"].ToString()
                        );
                }
            } 
            
            if (thisProject.BrandCode == "Y")
            {
                this.dgvStdExchanger.Columns[Name_Common.StdModelFull_York].Visible = true;
            }
            else if (thisProject.BrandCode == "H")
            {
                this.dgvStdExchanger.Columns[Name_Common.StdModelFull_Hitachi].Visible = true;
            }
            this.dgvStdExchanger.Columns[Name_Common.StdModelFull].Visible = false;
        }
        // 初始化界面 DGV 控件的列标题
        /// <summary>
        /// 初始化界面 DGV 控件的列标题
        /// </summary>
        private void InitDGV()
        {
            this.dgvStdExchanger.AutoGenerateColumns = false;
            NameArray_Indoor Arr_Indoor = new NameArray_Indoor();
            Global.SetDGVDataName(ref dgvStdExchanger, Arr_Indoor.StdIndoor_DataName);
            Global.SetDGVName(ref dgvStdExchanger, Arr_Indoor.StdIndoor_Name);
            Global.SetDGVHeaderText(ref dgvStdExchanger, Arr_Indoor.StdIndoor_HeaderText);
            
        }

        // 绑定单位表达式
        /// <summary>
        /// 绑定单位表达式
        /// </summary>
        private void BindUnit()
        {
            ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            //ut_pressure = Unit.ut_Pressure;
            ut_pressure = SystemSetting.UserSetting.unitsSetting.settingESP;

            jclblUnitFA1.Text = ut_airflow;
          //  jclblUnitFA2.Text = ut_airflow;
            jclblUnitAirflow.Text = ut_airflow;
           // jclblUnitAirflow2.Text = ut_airflow;
            jclblUnitkW1.Text = ut_power;
            jclblUnitkW2.Text = ut_power;
            jclblUnitkW3.Text = ut_power;
          //  jclblUnitkW4.Text = ut_power;
          //  jclblUnitkW5.Text = ut_power;
          //  jclblUnitkW6.Text = ut_power;
            jclblUnitTemperature1.Text = ut_temperature;
            jclblUnitTemperature2.Text = ut_temperature;
            jclblUnitTemperature3.Text = ut_temperature;
            jclblUnitStaticPressure.Text = ut_pressure;
          //  jclblUnitStaticPressure2.Text = ut_pressure;
        }

        /// <summary>
        /// 选中绑定图片 替换选中值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvStdExchanger_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStdExchanger.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvStdExchanger.SelectedRows[0];
                if (thisProject.BrandCode == "Y")
                    jclblSelectIndoor.Text = r.Cells[Name_Common.StdModelFull_York].Value.ToString();
                else
                    jclblSelectIndoor.Text = r.Cells[Name_Common.StdModelFull_Hitachi].Value.ToString();
                string imageName = r.Cells[Name_Common.TypeImage].Value.ToString();
                if (string.IsNullOrEmpty(imageName))
                    return;
                string fullPath = MyConfig.TypeImageDirectory + imageName;
                if (System.IO.File.Exists(fullPath))
                {
                    Image img = new Bitmap(fullPath);
                    if (pbExchanger.BackgroundImage != img)
                    {
                        pbExchanger.BackgroundImage = img;
                        pbExchanger.BackgroundImageLayout = ImageLayout.Center;
                    }
                }
            
              //显示全热交换机的信息 
                string modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
                Indoor indItem = bll.GetItem(modelfull, _series, _series, _series);
                if (indItem != null)
                    indItem.Series = _series;   //将当前的TTL Heat Exchanger列表   add on 20170710 xyj 
                double est_cool = Convert.ToDouble(r.Cells[Name_Common.StdCapacity_C].Value);
                double est_heat = Convert.ToDouble(r.Cells[Name_Common.StdCapacity_H].Value);
                double airflow = 0;
                double staticPressure = 0;
                double shf = 0;
                double fa = 0;
                if (indItem != null)
                {
                    shf = indItem.GetSHF(_fanSpeedLevel);
                    airflow = indItem.GetAirFlow(_fanSpeedLevel);
                    //staticPressure = indItem.GetStaticPressure(_fanSpeedLevel);
                    staticPressure = indItem.GetStaticPressure(); 
                }
                double est_sh = est_cool * shf;
                //this.jclblCapacityCValue.Text = Unit.ConvertToControl(est_cool, UnitType.POWER, ut_power).ToString("n1");
                //this.jclblCapacityHValue.Text = Unit.ConvertToControl(est_heat, UnitType.POWER, ut_power).ToString("n1");
                //this.jclblSensiableValue.Text = Unit.ConvertToControl(est_sh, UnitType.POWER, ut_power).ToString("n1");
                //this.jclblAirflowValue.Text = Unit.ConvertToControl(airflow, UnitType.AIRFLOW, ut_airflow).ToString("n0");
                //this.jclblStaticPressureValue.Text = staticPressure.ToString("n0");
                //this.jclblFAValue.Text = Unit.ConvertToControl(fa, UnitType.AIRFLOW, ut_airflow).ToString("n0");
            }
            else
            {
                //this.jclblCapacityCValue.Text = "-";
                //this.jclblCapacityHValue.Text = "-";
                //this.jclblSensiableValue.Text = "-";
                //this.jclblAirflowValue.Text = "-";
                //this.jclblStaticPressureValue.Text = "-";
                //this.jclblFAValue.Text = "-";
            }
        
        }

        private void dgvStdExchanger_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DoOK();
            }
        }

        private void jctxtRoomCapC_TextChanged(object sender, EventArgs e)
        {
            
            if (IsAuto && JCValidateGroup(this.pnlRoomRq))
                DoAutoSelect();
        }

        private void jctxtRoomStaticPressure_TextChanged(object sender, EventArgs e)
        {
            if (IsAuto && JCValidateGroup(this.pnlRoomRq))
            {
                DoAutoSelect();
            }
        }


        void jctxtDBCool_Leave(object sender, EventArgs e)
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
                        BindRoomCooling();
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

        void jctxtWBCool_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (JCValidateSingle(jctxtWBCool) && JCValidateSingle(jctxtDBCool))
            {
                if (jctxtWBCool.Text.Length > 1)
                {
                    if (NumberUtil.IsNumber(jctxtWBCool.Text))
                    {
                        if (roomWB == Convert.ToDouble(jctxtWBCool.Text)) 
                            return; 

                        DoCalculateByOption(UnitTemperature.WB.ToString());
                        BindRoomCooling();
                    }
                }
            }
        }

        void jctxtRH_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (JCValidateSingle(jctxtRH) && JCValidateSingle(jctxtDBCool))
            {
                if (jctxtRH.Text.Length > 1)
                {
                    if (NumberUtil.IsNumber(jctxtRH.Text))
                    {
                        if (roomRH == Convert.ToDouble(jctxtRH.Text)) 
                            return; 
                        if (Convert.ToDouble(jctxtRH.Text) > 100) 
                            jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0"); 

                        DoCalculateByOption(UnitTemperature.RH.ToString());
                        BindRoomCooling();
                    }
                }
            }
        }

        private void jctxtRH_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (jctxtRH.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctxtRH.Text))
                {
                    if (Convert.ToDouble(jctxtRH.Text) > 100) 
                        jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0"); 

                    DoCalculateByOption(UnitTemperature.RH.ToString());
                }
            }
        }

        private void jctxtDBCool_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (jctxtDBCool.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctxtDBCool.Text)) 
                    DoCalculateByOption(UnitTemperature.DB.ToString()); 
            }

            //去除自动选型全热交换机 on 2017-07-31 by xyj
            //if (JCValidateGroup(pnlRoomCooling))
            //{
                //DoCalculateRH();
                ////添加修改工况温度 add by axj 20170116
                //if (IsAuto)
                //{
                //    // 检验需求信息是否填写
                //    if (!this.JCValidateGroup(this.pnlRoomRq))
                //        return;
                //    DoAutoSelect();
                //}
           // }
        }

        private void jctxtWBCool_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (jctxtWBCool.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctxtWBCool.Text))
                {
                    DoCalculateByOption(UnitTemperature.WB.ToString());
                }
            }


            //去除自动选型全热交换机 on 2017-07-31 by xyj
            //if (JCValidateGroup(pnlRoomCooling))
            //{
            //    //DoCalculateRH();
            //    ////添加修改工况温度 add by axj 20170116
            //    if (IsAuto)
            //    {
            //        // 检验需求信息是否填写
            //        if (!this.JCValidateGroup(this.pnlRoomRq))
            //            return;
            //        DoAutoSelect();
            //    }
            //}
        }

        private void jctxtDBHeat_TextChanged(object sender, EventArgs e)
        {
            //去除自动选型全热交换机 on 2017-07-31 by xyj
            //JCValidateGroup(pnlRoomHeating);
            ////添加修改工况温度 add by axj 20170116
            //if (IsAuto)
            //{
            //    // 检验需求信息是否填写
            //    if (!this.JCValidateGroup(this.pnlRoomRq))
            //        return;
            //    DoAutoSelect();
            //}
        }
        // 根据当前需求信息自动选型
        /// <summary>
        /// 根据当前需求信息自动选型
        /// </summary>
        private void DoAutoSelect()
        {
            this.toolStripStatusLabel1.Text = "";
            rq_cool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomCapC.Text), UnitType.POWER, ut_power);
            rq_heat = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomCapH.Text), UnitType.POWER, ut_power);
            rq_sensiable = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomSensiCapC.Text), UnitType.POWER, ut_power);
            rq_airflow = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomAirflow.Text), UnitType.AIRFLOW, ut_airflow);
            //rq_StaticPressure = Convert.ToDouble(this.jctxtRoomStaticPressure.Text);
            rq_StaticPressure = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomStaticPressure.Text), UnitType.STATICPRESSURE, ut_pressure);
            rq_fa = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomFA.Text), UnitType.AIRFLOW, ut_airflow);

           // bool isPass = false;
            int indexStart = 0;
            int indexEnd = this.dgvStdExchanger.Rows.Count - 1; 
           // // 遍历当前标准表，查找最合适的全热交换机
           // //foreach (DataGridViewRow r in this.dgvStdIndoor.Rows)
            for (int i = indexStart; i <= indexEnd; ++i)
            {
                DataGridViewRow r = dgvStdExchanger.Rows[i];
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, ut_power);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, ut_power); 
                //isPass = (rq_cool < std_cool) && (rq_heat < std_heat);

                //if (!isPass || !autoCompare(r))
                if (!autoCompare(r))
                    continue;

                r.Selected = true;
                if (thisProject.BrandCode == "Y")
                    this.dgvStdExchanger.CurrentCell = r.Cells[1];
                else
                    this.dgvStdExchanger.CurrentCell = r.Cells[2];
                // dgvStdIndoor_SelectionChanged(null, null);
                return;
            }
            //自动选型时，如果没有返回结果，则加载警告提示信息并清空全热交换机结果文本框。
            this.toolStripStatusLabel1.Text = Msg.EXC_NOTMATCH;
            this.jclblSelectIndoor.Text = "";
            dgvStdExchanger.ClearSelection();
        }

        /// <summary>
        /// 比较指定行的全热交换机容量与需求容量，满足则返回true
        /// </summary>
        /// <param name="type"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private bool autoCompare(DataGridViewRow r)
        {
            bool pass = true;
            // 计算估算容量
            DoCalculateEstValue(r);
            if (thisProject.IsCoolingModeEffective)
            {
                if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow || staticPressure < rq_StaticPressure)
                    pass = false;
            }
            //if (thisProject.IsHeatingModeEffective && !thisProject.ProductType.Contains(", CO"))
            if (thisProject.IsHeatingModeEffective)
            {
                if (est_heat < rq_heat)
                    pass = false;
            }
            return pass;
        }

        // 将选择的全热交换机预添加到已选全热交换机
        /// <summary>
        /// 将选择的全热交换机预添加到已选全热交换机，并执行容量估算
        /// </summary>
        /// <param name="stdRow">标准表记录行</param>
        /// <param name="isAppend">是否附加到已选记录行，true则附加且count值可修改；false则添加唯一记录且count值不可修改</param>
        private void DoCalculateEstValue(DataGridViewRow stdRow)
        {
            string modelFull = stdRow.Cells[Name_Common.StdModelFull].Value.ToString();
            Indoor indItem = bll.GetItem(modelFull, _series, _series, _series);
            if (indItem != null)
                indItem.Series = _series;   //将当前的TTL Heat Exchanger列表   add on 20170710 xyj  
            // 执行容量估算
            double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);
            double db_c = indItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;
            double db_h = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBHeat.Text), UnitType.TEMPERATURE, ut_temperature);
            double wb_h = indItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;

            //est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, out est_sh, false); 
            est_cool = bll.CalIndoorEstCapacity(indItem, db_c, wb_c, false);
            double shf = indItem.GetSHF(_fanSpeedLevel);
            est_sh = est_cool * shf;

            if (!indItem.ProductType.Contains(", CO"))
            {
                est_heat = bll.CalIndoorEstCapacity(indItem, wb_h, db_h, true);
            }  
            airflow = indItem.GetAirFlow(_fanSpeedLevel);
            //staticPressure = indItem.GetStaticPressure(_fanSpeedLevel);
            staticPressure = indItem.GetStaticPressure();
            //取消风速等级
            // airflow = indItem.AirFlow;
            // staticPressure=Convert.ToDouble(indItem.ESP);
        } 

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

            if (IsAuto)
            {
                // 检验需求信息是否填写
                if (!this.JCValidateGroup(this.pnlRoomRq))
                    return;

                 DoAutoSelect();
            }
        }


        // 设置需求信息是否允许为空
        /// <summary>
        /// 设置需求信息是否允许为空,手动则允许为空
        /// </summary>
        /// <param name="isManual"></param>
        private void ResetControlState(bool isAuto)
        {
            this.jctxtRoomCapC.RequireValidation = isAuto;
            this.jctxtRoomSensiCapC.RequireValidation = isAuto;
            this.jctxtRoomAirflow.RequireValidation = isAuto;
            this.jctxtRoomStaticPressure.RequireValidation = isAuto;
            this.jctxtRoomCapH.RequireValidation = isAuto;
            this.jctxtRoomFA.RequireValidation = isAuto;
            //this.pnlContent_1.Enabled = isAuto;

            this.jctxtRoomCapC.Enabled = isAuto;
            this.jctxtRoomSensiCapC.Enabled = isAuto;
            this.jctxtRoomAirflow.Enabled = isAuto;
            this.jctxtRoomStaticPressure.Enabled = isAuto;
            this.jctxtRoomCapH.Enabled = isAuto;
            this.jctxtRoomFA.Enabled = isAuto;
            //this.jctxtDBCool.Enabled = isAuto;
            //this.jctxtWBCool.Enabled = isAuto;
            //this.jctxtRH.Enabled = isAuto;
            //this.jctxtDBHeat.Enabled = isAuto;
            this.dgvStdExchanger.Enabled = !isAuto;
        }

        private void jccmbPower_SelectionChangeCommitted(object sender, EventArgs e)
        {
            BindStdExchangerList(); 
        }

        private void jccmbExchanger_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //_series = jccmbExchanger.Text.ToString();
            _series = jccmbExchanger.SelectedValue.ToString();
            BindPowerlist();
            BindStdExchangerList(); 
        }

        private void dgvStdExchanger_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
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

        private void dgvStdExchanger_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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

        private string SaveRoom(Floor floor)
        {

            Room curRoom = new Room();
            int Number = pbll.GetNextRoomNO(floor);
            curRoom.Id = Guid.NewGuid().ToString("N");
            curRoom.NO = Number;
            curRoom.Name = SystemSetting.UserSetting.defaultSetting.RoomName + Number;
            curRoom.Location = "";
            curRoom.Type = "";
            curRoom.Area = 0;
            curRoom.PeopleNumber = 0;
            curRoom.LoadIndexCool = 0;
            curRoom.RqCapacityCool = 0;
            curRoom.IsSensibleHeatEnable = false;
            curRoom.SensibleHeat = 0;
            curRoom.IsAirFlowEnable = false;
            curRoom.AirFlow = 0;
            curRoom.IsStaticPressureEnable = false;
            curRoom.StaticPressure = 0;
            curRoom.LoadIndexHeat = 0;
            curRoom.RqCapacityHeat = 0;
            curRoom.FreshAirIndex = 0;
            curRoom.FreshAir = 0;
            floor.RoomList.Add(curRoom);
            return curRoom.Id;
        }

        //获取楼层信息
        public Floor GetFloorByNO(int floorNo)
        {
            return thisProject.FloorList.Find(p => p.NO == floorNo);
        }


        //获取楼层信息
        public Floor GetFloor(string floorId)
        {
            return thisProject.FloorList.Find(p => p.Id == floorId);
        }

        private void cboTreeRoom_Click(object sender, EventArgs e)
        {
            defaultRoom = cboTreeRoom.Text;
        }

        private void cboTreeRoom_TextUpdate(object sender, EventArgs e)
        {
            if (defaultRoom != cboTreeRoom.Text)
            {
                cboTreeRoom.Text = defaultRoom;
            }
            return;
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

        private void frmExchanger_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil.Dispose();
        }
    }
}
