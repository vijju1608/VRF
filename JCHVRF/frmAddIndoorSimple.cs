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
    public partial class frmAddIndoorSimple : JCBase.UI.JCForm
    {
        #region Initialization 初始化

        bool IsAuto;
        string utAirflow;
        string utPower;
        string utTemperature;
        string utPressure; // static pressure add on 20170703 by Shen Junjie
        
        Project thisProject;
        ProjectBLL pbll;
        IndoorBLL bll;
        RoomIndoor curRI;
        string newRoom = "New Room";
        string noRoom = "No Room";
        string defaultRoom;
        string _factory = ""; //当前室内机类型的工厂 add by Shen Junjie on 20170707
        string _type = ""; //当前室内机类型  add by Shen Junjie on 20170707
        string _productType; //当前选择的ProductType add in 20160821 by Yunxiao Lin
        string _series;   //当前选择的series add on 20161027
        //string _shf_mode;   //当前选择的shfmode，三档：High，Medium，Low add on 20161111 by Yunxiao Lin
        int _fanSpeedLevel = -1; //风扇速度等级 -1:Max, 0:High2, 1:High, 2:Med, 3:Low add on 20170703 by Shen Junjie
        UndoRedo.UndoRedoHandler UndoRedoUtil = null; //注册撤销实体对象 add by axj 20161228
        //初始化工况温度 add by axj 20170116
        private double outdoorCoolingDB = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
        private double outdoorHeatingWB = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;
        private double outdoorCoolingIW = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
        private double outdoorHeatingIW = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;

        double roomDB = 0;
        double roomWB = 0;
        double roomRH = 0;

        Trans trans = new Trans();  //翻译初始化

        public frmAddIndoorSimple(Project thisProj)
        {
            InitializeComponent();
            this.JCSetLanguage();

            thisProject = thisProj;
            JCFormMode = FormMode.NEW;
            curRI = new RoomIndoor();


            IsAuto = SystemSetting.UserSetting.defaultSetting.IsIndoorAuto;
        }

        public frmAddIndoorSimple(RoomIndoor ri, Project thisProj)
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
            ChkOutdoorTemperature(ri, thisProj);//检查工况温度 add by axj 20170116
        }

        private void frmAddIndoorSimple_Load(object sender, EventArgs e)
        {
            Initialization();
            /*注册撤销功能 add by axj 20161228 begin */
            UndoRedoUtil = new UndoRedo.UndoRedoHandler(true);
            UndoRedoUtil.ShowIconsOnTabPage(tapPageTrans1, new Rectangle(970, 6, 16, 18), new Rectangle(942, 6, 16, 18));

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
            this.JCCallValidationManager = true;    // 启用界面控件验证
            toolStripStatusLabel1.Text = "";

            pbll = new ProjectBLL(thisProject);
            //bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode);
            bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);

            InitDGV();
            BindUnit();
            BindRoom();
            if (this.JCFormMode == FormMode.NEW)
            {
                if (thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S")
                {
                    cboTreeRoom.Name = "0";
                    cboTreeRoom.Text = noRoom;
                    //int no = pbll.GetMaxFloorNO();
                    //Floor floor = GetFloorByNO(no);
                    //cboTreeRoom.Name = floor.Id + "_0";
                    //cboTreeRoom.Text = newRoom;
                }
                else
                {
                    cboTreeRoom.Name = "0";
                    cboTreeRoom.Text = noRoom;
                }
            }
            BindToControl();
            BindEvent();
            this.uc_CheckBox_Auto.TextString = ShowText.Auto;
            this.uc_CheckBox_Manual.TextString = ShowText.Manual;
            cboTreeRoom.MyChange += new JCBase.UI.ComboBoxTreeView.MyChangeEventHandler(UpdateRoomName);
        }

        #endregion

        #region Controls events


        /// <summary>
        /// 更新房间名称 on 20170927 by xyj
        /// </summary>
        void UpdateRoomName()
        {
            if (!string.IsNullOrEmpty(cboTreeRoom.Room_Id) && !string.IsNullOrEmpty(cboTreeRoom.Room_Name))
            {
                ModifyRoomName(cboTreeRoom.Room_Id, cboTreeRoom.Room_Name);
            }

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

        /// 筛选掉没有需求的空房间 
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

        private void BindRoom()
        {
            TreeView treeView = this.cboTreeRoom.TreeView;
            treeView.Nodes.Clear();
            TreeNode rootZ = new TreeNode();
            rootZ.Text = noRoom;
            rootZ.Name = "0";

            treeView.Nodes.Add(rootZ);
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
            // sNode.NodeFont = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);

            node.Nodes.Add(sNode);
            foreach (Room ri in list)
            {
                TreeNode subNode = new TreeNode();
                subNode.Text = ri.Name;
                subNode.Name = ri.Id;
                subNode.NodeFont = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);

                node.Nodes.Add(subNode);
            }
            node.ExpandAll();
        }

        private void jctxtName_TextChanged(object sender, EventArgs e)
        {
            JCValidateSingle(jctxtName);
            //状态栏提示名称不为空字符串和空格串
            if (jctxtName.Text.Trim().Length == 0 && jctxtName.Text.Length != 0)
            {
                toolStripStatusLabel1.Text = Msg.NAME_BLANKSTRING;
            }
            else
            { toolStripStatusLabel1.Text = null; }
        }

        private void jccmbType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            UpdateUnitType();
            SetFreshAirTemperature();

            BindStdIndoorList();

            if (IsAuto)
            {
                // 检验需求信息是否填写
                if (!this.JCValidateGroup(this.pnlRoomRq))
                    return;

                DoAutoSelect();
            }
        }

        private void SetFreshAirTemperature()
        {
            if (jccmbType.SelectedValue.ToString().Contains("Fresh Air"))
            {   //Set Fresh Air Temp and RH Range
                FreshAirRange();
                //set default values
                this.jctxtDBCool.Text = Unit.ConvertToControl(33, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtWBCool.Text = Unit.ConvertToControl(28, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtDBHeat.Text = Unit.ConvertToControl(0, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtRH.Text = Unit.ConvertToControl(68, UnitType.TEMPERATURE, utTemperature).ToString("n1");
            }
            else
            {
                NonFreshAirRange();
                //set default values
                this.jctxtDBCool.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtWBCool.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtDBHeat.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                this.jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0");
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
                _type = type;
                //_type = type.Substring(0, i);
            }
            else
            {
                _factory = "";
                _type = type;
            }

            //This static pressure information is based on the power supply 230V/1Ph/60Hz 提示只给High Static Ducted (NA)
            // jclblIndStaticPressureNodtes.Visible = (_type == "High Static Ducted (NA)");
        }

        private void dgvStdIndoor_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStdIndoor.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvStdIndoor.SelectedRows[0];
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
                    if (pbIndoor.BackgroundImage != img)
                    {
                        pbIndoor.BackgroundImage = img;
                        pbIndoor.BackgroundImageLayout = ImageLayout.Center;
                    }
                }

                ////显示已选室内机的信息
                //string modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
                //Indoor inItem = bll.GetItem(modelfull, _type, _productType);
                //if (inItem != null)
                //    inItem.Series = _series;

                //double est_cool = Convert.ToDouble(r.Cells[Name_Common.StdCapacity_C].Value);
                //double est_heat = Convert.ToDouble(r.Cells[Name_Common.StdCapacity_H].Value);
                //double airflow = 0;
                //double staticPressure = 0;
                //double shf = 0;
                //double fa = 0;
                //if (inItem != null)
                //{
                //    shf = inItem.GetSHF(_fanSpeedLevel);
                //    if (IndoorBLL.IsFreshAirUnit(_type))  // 新风机计算新风风量合计
                //    {
                //        fa = inItem.AirFlow;
                //    }
                //    else
                //    {
                //        airflow = inItem.GetAirFlow(_fanSpeedLevel);
                //        //staticPressure = inItem.GetStaticPressure(_fanSpeedLevel);
                //        staticPressure = inItem.GetStaticPressure();
                //    }
                //}
                //double est_sh = est_cool * shf;


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

        private void dgvStdIndoor_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                DoOK();
            }
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

        private void jctxtRoomCapC_TextChanged(object sender, EventArgs e)
        {
            if (IsAuto && JCValidateGroup(this.pnlRoomRq))
                DoAutoSelect();
        }

        private void jctxtRoomStaticPressure_TextChanged(object sender, EventArgs e)
        {
            if (IsAuto && JCValidateGroup(this.pnlRoomRq))
            {
                if (!jccmbType.SelectedValue.ToString().ToLower().Contains("ducted"))
                {
                    string unitType = bll.GetDefaultDuctedUnitType(thisProject.SubRegionCode, thisProject.BrandCode, _series);
                    if (!string.IsNullOrEmpty(unitType))
                    {
                        //this.jccmbType.Text = unitType;
                        this.jccmbType.SelectedValue = unitType;
                    }
                }
                else
                {
                    DoAutoSelect();
                }
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
                        {
                            return;
                        }
                        DoCalculateByOption(UnitTemperature.DB.ToString());
                        BatchCalculateEstValue();
                        if (JCValidateGroup(pnlRoomCooling))
                        {

                            BindRoomCooling();
                            //添加修改工况温度 add by axj 20170116
                            if (IsAuto)
                            {
                                // 检验需求信息是否填写
                                if (!this.JCValidateGroup(this.pnlRoomRq))
                                    return;
                                DoAutoSelect();
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
                        {
                            return;
                        }
                        DoCalculateByOption(UnitTemperature.WB.ToString());
                        BatchCalculateEstValue();
                        if (JCValidateGroup(pnlRoomCooling))
                        {
                            BindRoomCooling();
                            //添加修改工况温度 add by axj 20170116
                            if (IsAuto)
                            {
                                // 检验需求信息是否填写
                                if (!this.JCValidateGroup(this.pnlRoomRq))
                                    return;
                                DoAutoSelect();
                            }
                        }
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
                        {
                            return;
                        }
                        if (Convert.ToDouble(jctxtRH.Text) > 100)
                        {
                            jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0");
                        }

                        DoCalculateByOption(UnitTemperature.RH.ToString());
                        BatchCalculateEstValue();
                        if (JCValidateGroup(pnlRoomCooling))
                        {
                            BindRoomCooling();
                            //添加修改工况温度 add by xyj 20170116
                            if (IsAuto)
                            {
                                // 检验需求信息是否填写
                                if (!this.JCValidateGroup(this.pnlRoomRq))
                                    return;
                                DoAutoSelect();
                            }
                        }
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
                    {
                        jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0");
                    }
                    DoCalculateByOption(UnitTemperature.RH.ToString());
                    BatchCalculateEstValue();
                    if (JCValidateGroup(pnlRoomCooling))
                    {
                        //添加修改工况温度 add by xyj 20170116
                        if (IsAuto)
                        {
                            // 检验需求信息是否填写
                            if (!this.JCValidateGroup(this.pnlRoomRq))
                                return;
                            DoAutoSelect();
                        }
                    }
                }
            }
        }

        private void jctxtDBCool_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (jctxtDBCool.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctxtDBCool.Text))
                {

                    DoCalculateByOption(UnitTemperature.DB.ToString());
                    BatchCalculateEstValue();
                    if (JCValidateGroup(pnlRoomCooling))
                    {
                        //添加修改工况温度 add by axj 20170116
                        if (IsAuto)
                        {
                            // 检验需求信息是否填写
                            if (!this.JCValidateGroup(this.pnlRoomRq))
                                return;
                            DoAutoSelect();
                        }
                    }
                }
            }

        }

        private void jctxtWBCool_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (jctxtWBCool.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctxtWBCool.Text))
                {
                    DoCalculateByOption(UnitTemperature.WB.ToString());
                    BatchCalculateEstValue();
                    if (JCValidateGroup(pnlRoomCooling))
                    {
                        //添加修改工况温度 add by axj 20170116
                        if (IsAuto)
                        {
                            // 检验需求信息是否填写
                            if (!this.JCValidateGroup(this.pnlRoomRq))
                                return;
                            DoAutoSelect();
                        }
                    }
                }
            }
        }

        private void jctxtDBHeat_Leave(object sender, EventArgs e)
        {
            ControllerInputDBHeat(sender as TextBox);
            //添加修改工况温度 
            BatchCalculateEstValue();
            if (IsAuto)
            {
                // 检验需求信息是否填写
                if (!this.JCValidateGroup(this.pnlRoomRq))
                    return;
                DoAutoSelect();
            }
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                jcbtnOK.Cursor = Cursors.WaitCursor;
                DoOK();
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

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            frmMain f = this.Owner as frmMain;

            f.BindIndoorList();         // Add

            DialogResult = DialogResult.Cancel;

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

            NameArray_Indoor Arr_Indoor = new NameArray_Indoor();
            Global.SetDGVDataName(ref dgvStdIndoor, Arr_Indoor.StdIndoor_DataName);
            Global.SetDGVName(ref dgvStdIndoor, Arr_Indoor.StdIndoor_Name);
            Global.SetDGVHeaderText(ref dgvStdIndoor, Arr_Indoor.StdIndoor_HeaderText);
            //Global.SetDGVNotSortable(ref dgvStdIndoor);
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
            // jclblUnitFA2.Text = ut_airflow;
            jclblUnitAirflow.Text = utAirflow;
            // jclblUnitAirflow2.Text = ut_airflow;
            jclblUnitkW1.Text = utPower;
            jclblUnitkW2.Text = utPower;
            jclblUnitkW3.Text = utPower;
            //jclblUnitkW4.Text = ut_power;
            //jclblUnitkW5.Text = ut_power;
            //jclblUnitkW6.Text = ut_power;
            jclblUnitTemperature1.Text = utTemperature;
            jclblUnitTemperature2.Text = utTemperature;
            jclblUnitTemperature3.Text = utTemperature;
            jclblUnitStaticPressure.Text = utPressure;
            //  jclblUnitStaticPressure2.Text = ut_pressure;
        }

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
            if (_productType == "Comm. Tier 2, HP")
            {
                //Comm. Tier 2, HP 有3个series, FSN6Q, FSXN, FSXN1 能够选择的室内机系列是不同的 20161201 by Yunxiao Lin
                if (_series == "Commercial VRF HP, FSN6Q" || _series == "Commercial VRF HP, JVOH-Q")
                {
                    //FSN6Q, JVOH不能使用HAPE High Static Ducted / Medium Static Ducted / Low Static Ducted
                    //FSN6Q, JVOH不能使用SMZ High Static Ducted / Medium Static Ducted / Four Way Cassette
                    dv.RowFilter = "UnitType not in ('High Static Ducted-HAPE','Medium Static Ducted-HAPE','Low Static Ducted-HAPE','High Static Ducted-SMZ','Medium Static Ducted-SMZ','Four Way Cassette-SMZ')";
                }
                else
                {
                    //FSXN, FSXN1不能使用HAPQ Four Way Cassette
                    dv.RowFilter = "UnitType <>'Four Way Cassette-HAPQ' and UnitType <>'Four Way Cassette (FSN1Q)'"; //RCI FSN1Q的Unit Type改为Four Way Cassette (FSN1Q). 20190122 by Yunxiao Lin
                }
            }

            //this.jccmbType.DisplayMember = colName;
            this.jccmbType.DisplayMember = displayColName;
            this.jccmbType.ValueMember = colName;
            this.jccmbType.DataSource = dv;

            UpdateUnitType();

            if (isFirst && this.JCFormMode == FormMode.EDIT)
                //this.jccmbType.Text = curRI.IndoorItem.Type;
                //unittypeList的值设定需要特殊处理，加上厂名, 20161118 by Yunxiao Lin
                BindUnitTypeListText(curRI.IndoorItem);
        }

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
                    //this.jccmbProductType.SelectedValue = thisProject.ProductType;
                    this.jccmbProductType.SelectedIndex = 0;

                //_productType = this.jccmbProductType.SelectedValue.ToString();                
                //_series = productTypeBll.GetSeriesByProductType(brandCode, regionCode, _productType);
                _series = this.jccmbProductType.SelectedValue.ToString();
                _productType = productTypeBll.GetProductTypeBySeries(brandCode, regionCode, _series);
            }
        }

        // 绑定标准表
        /// <summary>
        /// 绑定标准表
        /// </summary>
        private void BindStdIndoorList()
        {
            this.dgvStdIndoor.Rows.Clear();
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
                    DataGridViewRow newRow = dgvStdIndoor.Rows[dgvStdIndoor.Rows.Count - 1];
                    newRow.Tag = inItem;
                }
                BatchCalculateEstValue();
                BatchCalculateAirFlow();
            }
            //当前是新风机 显示新风量 on20170914 by xyj
            if (_type.Contains("Fresh Air"))
            {
                this.dgvStdIndoor.Columns[Name_Common.StdFreshAir].Visible = true;
            }
            else
            {
                this.dgvStdIndoor.Columns[Name_Common.StdFreshAir].Visible = false;
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

        // 窗口加载时控件初始值绑定
        /// <summary>
        /// 窗口加载时控件初始值绑定
        /// </summary>
        private void BindToControl()
        {
            BindCmbProductType(); //20160821 新增productType选项 by Yunxiao Lin
            BindIndoorTypeList(true);
            if (jccmbType.SelectedValue.ToString().Contains("Fresh Air"))
            {
                FreshAirRange();
            }
            else
            {
                NonFreshAirRange();
            }
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
                if (jccmbType.SelectedValue.ToString().Contains("Fresh Air"))
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
            BindStdIndoorList();
            if (JCFormMode == FormMode.NEW)
            {
                BindNextIndoorName();
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
                
                BindShfMode(); //绑定显热模式选择下拉框 20161112 by Yunxiao Lin
            }
            else if (JCFormMode == FormMode.EDIT)
            {
                IsAuto = curRI.IsAuto;
                this.uc_CheckBox_Auto.Checked = curRI.IsAuto;
                this.uc_CheckBox_Manual.Checked = !curRI.IsAuto;
                this.jctxtName.Text = curRI.IndoorName;
               
                //绑定SHF档位 20161112 by Yunxiao Lin
                BindShfMode(); //绑定显热模式选择下拉框 20161112 by Yunxiao Lin
                _fanSpeedLevel = curRI.FanSpeedLevel;
                this.shfComboBox.SelectedIndex = _fanSpeedLevel + 1;

                if (curRI.IsAuto) // 绑定自动选型时的容量需求信息
                {
                    this.jctxtRoomCapC.Text = Unit.ConvertToControl(curRI.RqCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                    this.jctxtRoomCapH.Text = Unit.ConvertToControl(curRI.RqHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                    this.jctxtRoomAirflow.Text = Unit.ConvertToControl(curRI.RqAirflow, UnitType.AIRFLOW, utAirflow).ToString("n1");
                    //this.jctxtRoomStaticPressure.Text = curRI.RqStaticPressure.ToString("n1");
                    this.jctxtRoomStaticPressure.Text = Unit.ConvertToControl(curRI.RqStaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
                    this.jctxtRoomSensiCapC.Text = Unit.ConvertToControl(curRI.RqSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                    this.jctxtRoomFA.Text = Unit.ConvertToControl(curRI.RqFreshAir, UnitType.AIRFLOW, utAirflow).ToString("n1");
                }

                foreach (DataGridViewRow r in dgvStdIndoor.Rows)
                {
                    if (r.Cells[Name_Common.StdModelFull].Value.ToString() == curRI.IndoorItem.ModelFull)
                    {
                        r.Selected = true;
                        // Add on 20130719 clh 定位到选中行
                        if (thisProject.BrandCode == "Y")
                            this.dgvStdIndoor.CurrentCell = r.Cells[1];
                        else
                            this.dgvStdIndoor.CurrentCell = r.Cells[2];
                        break;
                    }
                }
            }

            BindRoomCooling();
            //DoCalculateRH();
            ResetControlState(IsAuto);

        }

        private void FreshAirRange()
        {
            //Set Default Fresh Air Temp and RH Range
            this.jctxtDBCool.JCMinValue = float.Parse(Unit.ConvertToControl(20, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(43, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtWBCool.JCMinValue = float.Parse(Unit.ConvertToControl(10.9, UnitType.TEMPERATURE, utTemperature).ToString("n1"));  //11
            this.jctxtWBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(32, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBHeat.JCMinValue = float.Parse(Unit.ConvertToControl(-5, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBHeat.JCMaxValue = float.Parse(Unit.ConvertToControl(15, UnitType.TEMPERATURE, utTemperature).ToString("n1"));  //30
            this.jctxtRH.JCMinValue = float.Parse("30");
            this.jctxtRH.JCMaxValue = float.Parse("90");
        }

        private void NonFreshAirRange()
        {
            //Set Default Temp and RH Range
            this.jctxtDBCool.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(30, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtWBCool.JCMinValue = float.Parse(Unit.ConvertToControl(14, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtWBCool.JCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBHeat.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtDBHeat.JCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, utTemperature).ToString("n1"));
            this.jctxtRH.JCMinValue = float.Parse("13");
            this.jctxtRH.JCMaxValue = float.Parse("100");
        }

        // 绑定下一个室内机Name
        /// <summary>
        /// 绑定下一个室内机Name
        /// </summary>
        private void BindNextIndoorName()
        {
            string prefix = SystemSetting.UserSetting.defaultSetting.IndoorName;
            this.jctxtName.Text = prefix + pbll.GetNextRoomIndoorNo().ToString();
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
            //this.jctxtDBHeat.TextChanged += new EventHandler(jctxtDBHeat_TextChanged);
            // this.jctxtWBCool.TextChanged += new EventHandler(jctxtWBCool_TextChanged);
            // this.jctxtRH.TextChanged += new EventHandler(jctxtRH_TextChanged);
            this.jctxtDBCool.Leave += new EventHandler(jctxtDBCool_Leave);
            this.jctxtDBHeat.Leave += new EventHandler(jctxtDBHeat_Leave);
            this.jctxtWBCool.Leave += new EventHandler(jctxtWBCool_Leave);
            this.jctxtRH.Leave     += new EventHandler(jctxtRH_Leave);

            this.uc_CheckBox_Auto.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
            this.uc_CheckBox_Manual.CheckedChanged += new EventHandler(uc_CheckBox_Auto_CheckedChanged);
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

            this.dgvStdIndoor.Enabled = !isAuto;
        }

        double rq_cool = 0;
        double rq_heat = 0;
        double rq_sensiable = 0;
        double rq_airflow = 0;
        double rq_StaticPressure = 0;
        double rq_fa = 0;

        // 打开添加 Option 的窗口
        /// <summary>
        /// 打开添加 Option 的窗口(无)
        /// </summary>
        private void DoAddOptions()
        {
            // TODO: 
            JCMsg.ShowInfoOK("DoAddOptions()!");
        }

        // 清空选中室内机记录的 Option
        /// <summary>
        /// 清空选中室内机记录的 Option（无）
        /// </summary>
        private void DoClearOptions()
        {
            // TODO: 
            JCMsg.ShowInfoOK("DoClearOptions()!");
        }

        //double est_cool = 0;
        //double est_heat = 0;
        //double est_sh = 0;
        //double airflow = 0;
        //double staticPressure = 0;
        //// 将选择的室内机预添加到已选室内机
        ///// <summary>
        ///// 将选择的室内机预添加到已选室内机，并执行容量估算
        ///// </summary>
        ///// <param name="stdRow">标准表记录行</param>
        ///// <param name="isAppend">是否附加到已选记录行，true则附加且count值可修改；false则添加唯一记录且count值不可修改</param>
        //private void DoCalculateEstValue(DataGridViewRow stdRow)
        //{
        //    string modelFull = stdRow.Cells[Name_Common.StdModelFull].Value.ToString();
        //    Indoor inItem = bll.GetItem(modelFull, _type, _productType);
        //    if (inItem != null)
        //        inItem.Series = _series;   //将当前的series封装室内机列表   add on 20161027

        //    //if (_type.Contains("YDCF"))
        //    if (_type.Contains("YDCF") || _type.Contains("Fresh Air") || _type.Contains("Ventilation")) //YDCF是SVRF的FreshAir, Global VRF 需要更多判断 20171212 by Yunxiao Lin
        //    {
        //        est_cool = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value);
        //        est_heat = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value);
        //        est_sh = Convert.ToDouble(stdRow.Cells[Name_Common.StdSensibleHeat].Value);
        //    }
        //    //Hydro Free和DX-Interface没有partload数据，取标准容量 20171212 by Yunxiao Lin
        //    else if (inItem.Type.Contains("Hydro Free") || inItem.Type == "DX-Interface")
        //    {
        //        est_cool = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value);
        //        est_heat = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value);
        //        est_sh = 0d;
        //    }
        //    else
        //    {
        //        // 执行容量估算
        //        double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);
        //        double db_c = inItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;//eidt by axj 20170116 old:SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB; //Cooling Outdoor DB
        //        double db_h = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBHeat.Text), UnitType.TEMPERATURE, ut_temperature);
        //        double wb_h = inItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;//eidt by axj 20170116 old:SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB; //Heating Outdoor WB
        //        //double est_sh_h = 0;

        //        //est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, out est_sh, false);
        //        //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
        //        est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
        //        if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName))
        //            return;
        //        //显热由估算容量乘以SHF系数得到 20161112 by Yunxiao Lin
        //        double shf = inItem.GetSHF(_fanSpeedLevel);
        //        est_sh = est_cool * shf;

        //        if (!inItem.ProductType.Contains(", CO"))
        //        {
        //            //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
        //            //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
        //            est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
        //            if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName))
        //                return;
        //        }
        //    }
        //    //airflow = Convert.ToDouble(stdRow.Cells[Name_Common.StdAirFlow].Value);
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
                if (type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation")) //YDCF是SVRF的FreshAir, Global VRF 需要更多判断 20171212 by Yunxiao Lin
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
                    if (type.ToLower().Contains("ducted") || type.Contains("Total Heat Exchanger")) 
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

        //private void DoCalculateEstValue(RoomIndoor ri)
        //{
        //    string type = ri.IndoorItem.Type;
        //    string modelFull = ri.IndoorItem.ModelFull;

        //    if (type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation"))
        //    {
        //        est_cool = ri.IndoorItem.CoolingCapacity;
        //        est_heat = ri.IndoorItem.HeatingCapacity;
        //        est_sh = ri.IndoorItem.SensibleHeat;
        //        airflow = ri.IndoorItem.AirFlow; //新风机还是用标准表的Air Flow  add on 20170704 by Shen Junjie
        //        staticPressure = ri.StaticPressure;
        //    }
        //    else
        //    {
        //        // 执行容量估算
        //        //Hydro Free和DX-Interface没有partload数据，取标准容量 20171204 by Yunxiao Lin
        //        if (type.Contains("Hydro Free") || type == "DX-Interface")
        //        {
        //            est_cool = ri.IndoorItem.CoolingCapacity;
        //            est_heat = ri.IndoorItem.HeatingCapacity;
        //            est_sh = 0d;
        //            airflow = 0d;
        //            staticPressure = 0d;
        //        }
        //        else
        //        {
        //            double wb_c = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, ut_temperature);
        //            double db_c = ri.IndoorItem.ProductType.Contains("Water Source") ? outdoorCoolingIW : outdoorCoolingDB;//edit by axj 20170116 old:SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB; //Cooling Outdoor DB
        //            double db_h = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBHeat.Text), UnitType.TEMPERATURE, ut_temperature);
        //            double wb_h = ri.IndoorItem.ProductType.Contains("Water Source") ? outdoorHeatingIW : outdoorHeatingWB;//edit by axj 20170116 old:SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB;//Heating Outdoor WB
        //            //double est_sh_h = 0;

        //            //est_cool = bll.CalIndoorEstCapacity(ri.IndoorItem, db_c, wb_c, out est_sh, false);
        //            //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
        //            est_cool = bll.CalIndoorEstCapacity(ri.IndoorItem, db_c, wb_c, false);
        //            if (!ValidateEstCapacity(est_cool, ri.IndoorItem.PartLoadTableName))
        //                return;
        //            if (!ri.IndoorItem.ProductType.Contains(", CO"))
        //            {
        //                //est_heat = bll.CalIndoorEstCapacity(ri.IndoorItem, wb_h, db_h, out est_sh_h, true);
        //                //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
        //                est_heat = bll.CalIndoorEstCapacity(ri.IndoorItem, wb_h, db_h, true);
        //                if (!ValidateEstCapacity(est_heat, ri.IndoorItem.PartLoadTableName))
        //                    return;
        //            }
        //            //Sensible Heat 由set. Cooling Capacity 乘以 SHF系数得到 20161112 by Yunxiao Lin
        //            double shf = ri.IndoorItem.GetSHF(_fanSpeedLevel);
        //            est_sh = est_cool * shf;
        //            airflow = ri.IndoorItem.GetAirFlow(_fanSpeedLevel); //室内机用分档的Air Flow数据  add on 20170704 by Shen Junjie
        //            //staticPressure = ri.IndoorItem.GetStaticPressure(_fanSpeedLevel);
        //            staticPressure = ri.IndoorItem.GetStaticPressure();
        //        }                
        //    } 

        //}

        // 计算相对湿度
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateRH()
        {
            if (!string.IsNullOrEmpty(this.jctxtDBCool.Text) && !string.IsNullOrEmpty(this.jctxtWBCool.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                this.jctxtRH.Text = (rh * 100).ToString("n0");
                this.jclblRHValue.Text = (rh * 100).ToString("n1");
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


        // 根据当前需求信息自动选型
        /// <summary>
        /// 根据当前需求信息自动选型
        /// </summary>
        private void DoAutoSelect()
        {
            this.toolStripStatusLabel1.Text = "";
            if (string.IsNullOrEmpty(_type))
                return;

            rq_cool = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomCapC.Text), UnitType.POWER, utPower);
            rq_heat = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomCapH.Text), UnitType.POWER, utPower);
            rq_sensiable = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomSensiCapC.Text), UnitType.POWER, utPower);
            rq_airflow = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomAirflow.Text), UnitType.AIRFLOW, utAirflow);
            //rq_StaticPressure = Convert.ToDouble(this.jctxtRoomStaticPressure.Text);
            rq_StaticPressure = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomStaticPressure.Text), UnitType.STATICPRESSURE, utPressure);
            rq_fa = Unit.ConvertToSource(Convert.ToDouble(this.jctxtRoomFA.Text), UnitType.AIRFLOW, utAirflow);

            bool isPass = false;
            int indexStart = 0;
            int indexEnd = this.dgvStdIndoor.Rows.Count - 1;

            #region
            // 优化自动选型速度。。。先与当前选中的记录比较
            //if (this.dgvStdIndoor.SelectedRows.Count > 0)
            //{
            //    DataGridViewRow stdRow = this.dgvStdIndoor.SelectedRows[0];
            //    //bool isPass = autoCompare(type, rSel);

            //    double std_cool = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_C].Value);
            //    double std_heat = Convert.ToDouble(stdRow.Cells[Name_Common.StdCapacity_H].Value);
            //    isPass = (rq_cool < std_cool) && (rq_heat < std_heat);

            //    if (isPass)
            //    {
            //        indexStart = 0;
            //        indexEnd = stdRow.Index;
            //    }
            //    else
            //    {
            //        indexStart = stdRow.Index + 1;
            //        indexEnd = this.dgvStdIndoor.Rows.Count - 1;
            //    }
            //}
            #endregion

            // 遍历当前标准表，查找最合适的室内机
            //foreach (DataGridViewRow r in this.dgvStdIndoor.Rows)
            for (int i = indexStart; i <= indexEnd; ++i)
            {
                DataGridViewRow r = dgvStdIndoor.Rows[i];
                double std_cool = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
                double std_heat = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);
                isPass = (rq_cool <= std_cool) && (rq_heat <= std_heat);
                if (!isPass || !autoCompare(_type, r))
                    continue;

                r.Selected = true;
                // updated on 20160228 clh
                if (thisProject.BrandCode == "Y")
                    this.dgvStdIndoor.CurrentCell = r.Cells[1];
                else
                    this.dgvStdIndoor.CurrentCell = r.Cells[2];
                dgvStdIndoor_SelectionChanged(null, null);
                return;
            }
            //自动选型时，如果没有返回结果，则加载警告提示信息并清空室内机结果文本框。
            this.toolStripStatusLabel1.Text = Msg.IND_NOTMATCH;
            this.jclblSelectIndoor.Text = "";
            dgvStdIndoor.ClearSelection();
        }

        /// <summary>
        /// 比较指定行的室内机容量与需求容量，满足则返回true
        /// </summary>
        /// <param name="type"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private bool autoCompare(string type, DataGridViewRow r)
        {
            bool pass = true;
            //bool isFreshAir = type.Contains("YDCF");
            bool isFreshAir = type.Contains("YDCF") || type.Contains("Fresh Air") || type.Contains("Ventilation"); //YDCF是SVRF中的FreshAir,Global VRF 需要增加判断 20171212 by Yunxiao Lin
            // 计算估算容量
            //DoCalculateEstValue(r);  此处不需要再计算估算容量，绑定列表时已经是估算的容量 modified by Shen Junjie on 2018/2/27

            //从控件中取值进行直接比较  add by Shen Junjie on 2018/2/28
            double est_cool = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
            double est_heat = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);
            double est_sh = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdSensibleHeat].Value), UnitType.POWER, utPower);
            double airflow = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdAirFlow].Value), UnitType.AIRFLOW, utAirflow);
            //double staticPressure = Convert.ToDouble(r.Cells[Name_Common.StdStaticPressure].Value);
            double staticPressure = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdStaticPressure].Value), UnitType.STATICPRESSURE, utPressure);

            //将估算容量与当前需求进行比较
            if (isFreshAir)
            {
                airflow = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdFreshAir].Value), UnitType.AIRFLOW, utAirflow);
                if (airflow < rq_fa)
                    pass = false;
            }
            else
            {
                if (thisProject.IsCoolingModeEffective)
                {
                    if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow || staticPressure < rq_StaticPressure)
                        pass = false;
                }
                //if (thisProject.IsHeatingModeEffective && !thisProject.ProductType.Contains(", CO"))
                if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
                {
                    if (est_heat < rq_heat)
                        pass = false;
                }
            }

            return pass;
        }


        public bool ModifyRoomName(string roomId, string roomName)
        {

            //更新房间名称
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
        /// 保存房间
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
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

        // 根据当前窗口的模式添加、更新室内机记录
        /// <summary>
        /// 根据当前窗口的模式添加、更新室内机记录
        /// </summary>
        private void DoOK()
        {
            if (JCValidateSingle(jctxtName))
            {
                // 校验 Indoor Name ,输入的字符中不能含有特殊字符和空格串
                string nameType = "[Name]";
                if (!CDF.CCL.ValidateName(jctxtName.Text, nameType) || jctxtName.Text.Trim().Length == 0)
                    return;
                //如果是自动选型且需要的信息不满足，返回不增加室内机
                if (!this.JCValidateGroup(pnlRoomRq))
                {
                    return;
                }
                if (dgvStdIndoor.SelectedRows.Count == 0)
                    return;

                SystemSetting.UserSetting.defaultSetting.IsIndoorAuto = uc_CheckBox_Auto.Checked;
                SystemSetting.Serialize();
                //室内机名称唯一限制   --add on 20180418 by Vince
                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    if (ri.IndoorName == jctxtName.Text)
                    {
                        if (this.JCFormMode == FormMode.EDIT)
                        {
                            if (curRI.IndoorName != jctxtName.Text)
                            {
                                JCMsg.ShowWarningOK(Msg.WARNING_IND_NAME_EXIST(jctxtName.Text));
                                return;
                            }
                        }
                        else
                        {
                            JCMsg.ShowWarningOK(Msg.WARNING_IND_NAME_EXIST(jctxtName.Text));
                            return;
                        }

                    }
                }

                // add on 20160408 clh SensibleHeat值不大于CoolingCapacity
                //增加 Sensib Heat小于Total Capacity提示
                if (IsAuto)
                {
                    if ((!string.IsNullOrEmpty(jctxtRoomSensiCapC.Text.Trim())) && (!string.IsNullOrEmpty(jctxtRoomCapC.Text.Trim())))
                    {
                        if (double.Parse(jctxtRoomSensiCapC.Text) > double.Parse(jctxtRoomCapC.Text))
                        {
                            JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(jclblRoomSensiCapC.Text, "[" + jclblRoomCapC.Text + "]"));
                            jctxtRoomCapC.Focus();
                            return;
                        }
                    }
                }

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

                DataGridViewRow r = dgvStdIndoor.SelectedRows[0];
                //string modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
                //Indoor indItem = bll.GetItem(modelfull, _type, _productType);
                Indoor indItem = r.Tag as Indoor;
                if (indItem == null) return;
                indItem = indItem.DeepClone();   //避免不同的室内机用同一个Indoor对象，引起混乱 by Shen Junjie on 2018/5/11

                //如果没有合适的机组，返回不增加室内机
                if (this.jclblSelectIndoor.Text == "" || this.toolStripStatusLabel1.Text == Msg.OUTD_NOTMATCH)
                {
                    return;
                }

                if (this.JCFormMode == FormMode.NEW)
                {
                    curRI = pbll.AddIndoor(newRoomId, indItem);
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
                    else
                    {
                        curRI.RoomID = "";
                        curRI.RoomName = "";
                    }
                }


                double est_cool = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_C].Value), UnitType.POWER, utPower);
                double est_heat = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdCapacity_H].Value), UnitType.POWER, utPower);
                double est_sh = Unit.ConvertToSource(Convert.ToDouble(r.Cells[Name_Common.StdSensibleHeat].Value), UnitType.POWER, utPower);

                //if (!IsAuto)// && dgvStdIndoor.SelectedRows != null
                //{
                //    //    DoCalculateEstValue(curRI);
                //}

                curRI.IndoorName = this.jctxtName.Text;
                //curRI.IndoorFullName = thisProject.BrandCode == "Y" ? curRI.IndoorName + "[" + curRI.IndoorItem.Model_York + "]" : curRI.IndoorName + "[" + curRI.IndoorItem.Model_Hitachi + "]";  //记录indoor对象的indoor名
                //curRI.IndoorItem.IndoorName = curRI.IndoorFullName;
                curRI.IsAuto = IsAuto;
                curRI.IsDelete = false;

                curRI.DBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtDBCool.Text), UnitType.TEMPERATURE, utTemperature);
                curRI.WBCooling = Unit.ConvertToSource(Convert.ToDouble(jctxtWBCool.Text), UnitType.TEMPERATURE, utTemperature);
                curRI.DBHeating = Unit.ConvertToSource(Convert.ToDouble(jctxtDBHeat.Text), UnitType.TEMPERATURE, utTemperature);
                curRI.RHCooling = Convert.ToDouble(jctxtRH.Text);
                // 自动选型（一次选择一台室内机）时需记录当前的容量需求
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
                //curRI.AirFlow = airflow;
                ////记录显热档位 20161112 by Yunxiao Lin
                //curRI.SHF_Mode = _shf_mode;
                //记录风扇速度档位 20170703 by Shen Junjie
                curRI.FanSpeedLevel = _fanSpeedLevel;

                // 实现连续添加
                if (this.JCFormMode == FormMode.EDIT)
                {
                    DialogResult = DialogResult.OK;
                    return;
                }

                //TODO: 临时屏蔽
                frmMain f = this.Owner as frmMain;
                if (JCFormMode == FormMode.NEW)
                {
                    f.BindIndoorList();         // Add
                }
                BindNextIndoorName();

                if (newRoomId != "0" && !string.IsNullOrEmpty(newRoomId))
                {
                    BindRoom();
                    cboTreeRoom.Name = newRoomId;
                    // Room rom = GetRoom(newRoomId);
                    cboTreeRoom.Text = pbll.GetFloorAndRoom(newRoomId);
                }
                else
                {
                    cboTreeRoom.Name = "0";
                    cboTreeRoom.Text = noRoom;
                }
                UndoRedoUtil.SaveProjectHistory();//保存历史痕迹 add by axj 20161228
            }
        }




        #endregion

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

        private void jccmbProductType_SelectionChangeCommitted(object sender, EventArgs e)
        {           
            string brandCode = Registr.Registration.SelectedBrand.Code;
            string regionCode = Registr.Registration.SelectedSubRegion.Code;
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            //同步改变室内机Unit Type列表
            //_series = productTypeBll.GetSeriesByProductType(brandCode, regionCode, _productType);
            _series = jccmbProductType.SelectedValue.ToString();
            _productType = productTypeBll.GetProductTypeBySeries(brandCode, regionCode, _series);           
            BindIndoorTypeList(false);

            //如果房间有静压需求则需要切换Type为Ducted型号
            if (IsAuto && JCValidateGroup(this.pnlRoomRq))
            {
                //string unitType = bll.GetDefaultDuctedUnitType(thisProject.SubRegionCode, thisProject.BrandCode, _series);
                string unitType = bll.GetDefaultDuctedUnitType(regionCode, brandCode, _series);
                if (!string.IsNullOrEmpty(unitType))
                {
                    //this.jccmbType.Text = unitType;
                    this.jccmbType.SelectedValue = unitType;
                    UpdateUnitType();
                }
            }
            SetFreshAirTemperature();
            BindStdIndoorList();

            if (IsAuto)
            {
                // 检验需求信息是否填写
                if (!this.JCValidateGroup(this.pnlRoomRq))
                    return;

                DoAutoSelect();
            }
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

        private void shfComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fanSpeedLevel = shfComboBox.SelectedIndex - 1;
            string Modelfull = "";
            if (dgvStdIndoor.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvStdIndoor.SelectedRows[0];
                Modelfull = r.Cells[Name_Common.StdModelFull].Value.ToString();
            }
            BindStdIndoorList();
            BatchCalculateAirFlow();
            if (IsAuto)
            {
                // 检验需求信息是否填写
                if (!this.JCValidateGroup(this.pnlRoomRq))
                    return;
                DoAutoSelect();
            }
            else
            {
                for (int i = 0; i <= dgvStdIndoor.Rows.Count - 1; ++i)
                {
                    DataGridViewRow ri = dgvStdIndoor.Rows[i];
                    if (ri.Cells[Name_Common.StdModelFull].Value.ToString() == Modelfull)
                    {
                        ri.Selected = true;
                        break;
                    }
                }
                dgvStdIndoor_SelectionChanged(null, null);
            }
        }

        /// 绑定UnitTypeList的Text值 20161118 by Yunxiao lin
        /// <summary>
        /// 绑定UnitTypeList的Text值
        /// </summary>
        /// <param name="selectText"></param>
        private void BindUnitTypeListText(Indoor IndoorItem)
        {
            string curSelectType = trans.getTypeTransStr(TransType.Indoor.ToString(), IndoorItem.Type);
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

            this.jccmbType.Text = curSelectType;   //带厂过滤的室内机type比较特殊，因数据库缺乏标示厂字段，故暂时以显示参数作为过滤源
            //this.jccmbType.SelectedValue = curSelectType;
            UpdateUnitType();
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
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            return ((long)(DateTime.Now - startTime).TotalMilliseconds).ToString(); // 相差毫秒数
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

        private void frmAddIndoorSimple_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil.Dispose();
        }
    }
}
