using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCHVRF.MyPipingBLL;
using JCBase.Util;
using JCHVRF.Const;
using JCHVRF.VRFMessage;
using JCBase.Utility;
using JCHVRF.VRFTrans;

namespace JCHVRF
{
    public partial class frmAddOutdoorUniversal : JCBase.UI.JCForm
    {
        #region Initialization 初始化
        string ut_power;
        string ut_temperature;
        string ut_length;
        string ut_waterFlowRate;
        Color colorRed = Color.Red;
        Color colorBlack = Color.Black;
        Color colorChocolate = Color.Chocolate;

        ProjectBLL pbll;
        Project thisProject;
        OutdoorBLL bll;

        SystemVRF curSystemItem;
        DataTable dtOutdoorStd;
        List<RoomIndoor> listRISelected = new List<RoomIndoor>();
        List<RoomIndoor> listRIAvailable = new List<RoomIndoor>(); 
        //string _productType = ""; //缓存当前ProductType 20160822 by Yunxiao Lin
        string _series = "";   //缓存当前series 20161027   

        List<SelectedIDUList> SelIDUList = new List<SelectedIDUList>(); //选中室内机的集合
        Trans trans = new Trans();

        private class SelectedIDUList
        {
            public int IndoorNo { get; set; }   //IndoorNo
            public string IndoorName { get; set; } //IndoorName
            public string IndoorModel { get; set; }
        }
        /// <summary>
        /// 构造函数，新增Outdoor
        /// </summary>
        /// <param name="thisProj"></param>
        public frmAddOutdoorUniversal(Project thisProj)
        {
            InitializeComponent();
            JCSetLanguage();

            this.JCFormMode = FormMode.NEW;
            this.thisProject = thisProj;
            curSystemItem = new SystemVRF();
        }

        /// <summary>
        ///  构造函数， 编辑Outdoor
        /// </summary>
        /// <param name="item"></param>
        /// <param name="thisProj"></param>
        public frmAddOutdoorUniversal(SystemVRF item, Project thisProj)
        {
            InitializeComponent();
            JCSetLanguage();

            this.JCFormMode = FormMode.EDIT;
            this.thisProject = thisProj;
            curSystemItem = item;
        }

    
        private void frmAddOutdoorUniversal_Load(object sender, EventArgs e)
        {
            pbll = new ProjectBLL(thisProject);
           
            //bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType,thisProject.BrandCode);
            bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode, Registr.Registration.SelectedSubRegion.ParentRegionCode);
            if (this.JCFormMode == FormMode.NEW)
            {
                curSystemItem = new SystemVRF(pbll.GetNextSystemNo());
                string namePrefix = SystemSetting.UserSetting.defaultSetting.OutdoorName;
                curSystemItem.Name = namePrefix + curSystemItem.NO.ToString();
                curSystemItem.DBCooling = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
                curSystemItem.DBHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB;
                curSystemItem.WBHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;
                curSystemItem.RHHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingRH;
                curSystemItem.PipeEquivalentLength = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                curSystemItem.PipeEquivalentLengthbuff = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                curSystemItem.FirstPipeLength = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
                curSystemItem.FirstPipeLengthbuff = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
                curSystemItem.HeightDiff = SystemSetting.UserSetting.pipingSetting.pipingHighDifference;
                curSystemItem.PipingLengthFactor = SystemSetting.UserSetting.pipingSetting.pipingCorrectionFactor;
                curSystemItem.PipingPositionType = SystemSetting.UserSetting.pipingSetting.pipingPositionType;
                //Add Water Source Inlet Water Temp. on 20160615 by Yunxiao Lin
                curSystemItem.IWCooling = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
                curSystemItem.IWHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;
            }

            this.JCCallValidationManager = true;
            NameArray_Indoor arr = new NameArray_Indoor();
            Global.SetLVHeaderText(ref lvIndoorList, arr.RoomIndoorNotAttached_HeaderText);

            if (this.JCFormMode == FormMode.NEW)
            {
                if (1 != curSystemItem.MaxRatio)
                {
                    curSystemItem.MaxRatio = 1;
                }
            }

            BindUnit();
            BindCmbProductType(); //新增多ProductType功能 add on 20160822 by Yunxiao Lin
            Init();
            BindMaxRatio(); //绑定MaxRation列表 20170331 by Yunxiao Lin
            BindOutdoorTypeList();
            BindPowerList();
            if (this.JCFormMode != FormMode.NEW)
            {
                jccmbPower.SelectedValue = curSystemItem.Power;
            }
            else
            {
                if (jccmbPower.SelectedValue != null)
                {
                    curSystemItem.Power = jccmbPower.SelectedValue.ToString();
                }
            }

            BindOutdoorList();//自动绑定室外机列表 2016/12/23 by axj
            if (this.JCFormMode != FormMode.NEW)
            {
                if (curSystemItem.OutdoorItem != null)
                {
                    jccmbOutdoor.SelectedValue = curSystemItem.OutdoorItem.ModelFull;
                }
            }
            //BindIndoorAvailable(_productType);
            BindIndoorAvailable(_series);
            BindTreeViewOutdoor();

            //TODO: 设置systemItem的FlowRateLevel 20170216
          
            BindEvent();
        } 

        #endregion

        #region Controls events

        void jctxtName_TextChanged(object sender, EventArgs e)
        {
            JCValidateSingle(jctxtName);
            //状态栏提示名称不为空格串
            if (jctxtName.Text.Trim().Length == 0)
            {
                toolStripStatusLabel1.Text = Msg.NAME_BLANKSTRING;
            }
            else
            { toolStripStatusLabel1.Text = null; }
        }

        /// 选择当前系统中室内机（含新风机）的组合模式，区分是否是混合模式
        /// <summary>
        /// 选择当前系统中室内机（含新风机）的组合模式，区分是否是混合模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jccbxCompositeMode_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as JCCheckBox).Checked;

            if (listRISelected.Count > 0)
            {
                if (JCMsg.ShowConfirmOKCancel(Msg.OUTD_CONFIRM_SYSTYPE) == DialogResult.OK)
                {
                    // 将清空的已选记录恢复到未分配的记录中
                    for (int i = listRISelected.Count - 1; i >= 0; --i)
                    {
                        doAddItemToListView(listRISelected[i]);
                        listRISelected.Remove(listRISelected[i]);
                    }

                    if (isChecked)
                        curSystemItem.SysType = SystemType.CompositeMode;
                    else
                        curSystemItem.SysType = SystemType.OnlyIndoor;
                    ReselectOutdoor();
                }
                else
                {
                    this.jccbxCompositeMode.Checked = !isChecked;
                    return;
                }
            }
            curSystemItem.SysType = isChecked ? SystemType.CompositeMode : SystemType.OnlyIndoor;
        }

        /// 切换室外机类型下拉框
        /// <summary>
        /// 切换室外机类型下拉框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jccmbType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // 如果当前系统选型模式为全新风机，则不允许切换
            if (curSystemItem.SysType == SystemType.OnlyFreshAir)
            {
                //jccmbType.Text = curSystemItem.SelOutdoorType;
                jccmbType.SelectedValue = curSystemItem.SelOutdoorType;
                switchInletWater();
                return;
            }
            // 如果提交的类型与当前类型一样，则不执行切换
            //string value = this.jccmbType.Text.Trim();
            string value = this.jccmbType.SelectedValue.ToString();
            if (value == curSystemItem.SelOutdoorType)
                return;

            // 切换 Type 时更新室外机待选标准数据表，仅当非一对一时
            curSystemItem.SelOutdoorType = value;
            setTypeImage("");
            BindPowerList();
            BindOutdoorList();//自动绑定室外机列表 2016/12/23 by axj
            //SelectOutdoorResult result = DoSelectOutdoor(out ERRList);
            //室外机选型统一改用新逻辑 Global.DoSelectUniversalODUFirst 20161112 by Yunxiao Lin
            if (jccmbPower.SelectedValue == null) //需要预先判断jccmbPower.SelectedValue是否为null才能继续，20171016 by Yunxiao Lin
                return;
            curSystemItem.Power = jccmbPower.SelectedValue.ToString().Trim();
            ReselectOutdoor();
            switchInletWater();
        }

        private void jccmbDiversity_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string value = this.jccmbDiversity.Text.Trim();
            if (string.IsNullOrEmpty(value) || (value == curSystemItem.DiversityFactor.ToString()))
                return;
            curSystemItem.DiversityFactor = Convert.ToDouble(value);

            ReselectOutdoor();
        }

        private void jccmbMaxRatio_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //if (curSystemItem.SysType !=SystemType.OnlyIndoor)
            if (curSystemItem.SysType == SystemType.OnlyFreshAir)
                return;

            string value = jccmbMaxRatio.Text.Trim();
            if (value == (curSystemItem.MaxRatio * 100).ToString())
                return;

            curSystemItem.MaxRatio = Convert.ToDouble(value) / 100;

            ReselectOutdoor();
        }

        private void RematchIndoor()
        {
            if (curSystemItem.OutdoorItem == null)
            {
                return;
            }
            if (!JCValidateSingle(jctxtDBC) || !JCValidateSingle(jctxtWBH))
            {
                return;
            }
            double WBHeating = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBH.Text == "" ? curSystemItem.WBHeating.ToString() : this.jctxtWBH.Text), UnitType.TEMPERATURE, ut_temperature);
            double DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBC.Text == "" ? curSystemItem.DBCooling.ToString() : this.jctxtDBC.Text), UnitType.TEMPERATURE, ut_temperature);
           
            //if (WBHeating == curSystemItem.WBHeating && DBCooling == curSystemItem.DBCooling)
            //{
            //    return;
            //}
            
            if (listRISelected.Count > 0)
            {
                //室外机工况温度修改 add by axj 20170118
                frmMatchIndoorResult fm = new frmMatchIndoorResult(listRISelected, thisProject, DBCooling, WBHeating);
                if (fm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (fm.ReselectionResultList != null)
                {
                    foreach (var ent in fm.ReselectionResultList)
                    {
                        if (ent.Seccessful == false)
                        {
                            var ri = listRISelected.Find(p => p.IndoorNO.ToString() == ent.IndoorNo);
                            listRISelected.Remove(ri);
                            doAddItemToListView(ri);
                        }
                        else
                        {
                            var ri_new = fm.ReselectedIndoorList.Find(p => p.IndoorNO.ToString() == ent.IndoorNo);
                            var index = thisProject.RoomIndoorList.FindIndex(p => p.IndoorNO.ToString() == ent.IndoorNo);
                            thisProject.RoomIndoorList[index] = ri_new;
                            index = listRISelected.FindIndex(p => p.IndoorNO.ToString() == ent.IndoorNo);
                            listRISelected[index] = ri_new;
                        }
                    }
                }
            }
            curSystemItem.DBCooling = DBCooling;
            curSystemItem.WBHeating = WBHeating;
            ReselectOutdoor();
        }

        private void RematchIndoorForWaterSource()
        {
            if (curSystemItem.OutdoorItem == null)
            {
                return;
            }
            if (!JCValidateSingle(jctxtIWC) || !JCValidateSingle(jctxtIWH))
            {
                return;
            } 
            double IWHeating = Unit.ConvertToSource(Convert.ToDouble(this.jctxtIWH.Text == "" ? curSystemItem.IWHeating.ToString() : this.jctxtIWH.Text), UnitType.TEMPERATURE, ut_temperature);
            double IWCooling = Unit.ConvertToSource(Convert.ToDouble(this.jctxtIWC.Text == "" ? curSystemItem.IWCooling.ToString() : this.jctxtIWC.Text), UnitType.TEMPERATURE, ut_temperature);
            if (IWHeating == curSystemItem.IWHeating && IWCooling == curSystemItem.IWCooling)
            {
                return;
            }
            if (listRISelected.Count < 1)
                return;

            //室外机工况温度修改 add by axj 20170118
            frmMatchIndoorResult fm = new frmMatchIndoorResult(listRISelected, thisProject, IWCooling, IWHeating);
            if (fm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (fm.ReselectionResultList != null)
            {
                foreach (var ent in fm.ReselectionResultList)
                {
                    if (ent.Seccessful == false)
                    {
                        var ri = listRISelected.Find(p => p.IndoorNO.ToString() == ent.IndoorNo);
                        listRISelected.Remove(ri);
                        doAddItemToListView(ri);
                    }
                    else
                    {
                        var ri_new = fm.ReselectedIndoorList.Find(p => p.IndoorNO.ToString() == ent.IndoorNo);
                        var index = thisProject.RoomIndoorList.FindIndex(p => p.IndoorNO.ToString() == ent.IndoorNo);
                        thisProject.RoomIndoorList[index] = ri_new;
                        index = listRISelected.FindIndex(p => p.IndoorNO.ToString() == ent.IndoorNo);
                        listRISelected[index] = ri_new;
                    }
                }
            }
            curSystemItem.IWCooling = IWCooling;
            curSystemItem.IWHeating = IWHeating;
            ReselectOutdoor();
        }

        void jctxtDBC_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            bool isTrue = true;
            if (JCValidateSingle(jctxtDBC))
            {
                isTrue = isRematch();
                curSystemItem.DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBC.Text == "" ? curSystemItem.DBCooling.ToString() : this.jctxtDBC.Text), UnitType.TEMPERATURE, ut_temperature);
            }
            if (isTrue)
            {
                RematchIndoor();
            }
        }

        private bool isRematch()
        {
            bool Auto = true;
            if (curSystemItem.DBCooling == Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBC.Text), UnitType.TEMPERATURE, ut_temperature) && curSystemItem.DBHeating == Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBH.Text), UnitType.TEMPERATURE, ut_temperature) && curSystemItem.WBHeating == Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBH.Text), UnitType.TEMPERATURE, ut_temperature))
            {
                Auto = false;
            }
            return Auto;
        }

        void jctxtDBH_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (JCValidateSingle(jctxtDBH) && JCValidateSingle(jctxtRH))
            {
                if (curSystemItem.OutdoorItem == null)
                    return;
                bool isTrue = isRematch();
                double DBH = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBH.Text == "" ? curSystemItem.DBHeating.ToString() : this.jctxtDBH.Text), UnitType.TEMPERATURE, ut_temperature);
                if (DBH == curSystemItem.DBHeating)
                    return;
                curSystemItem.DBHeating = DBH;
                double rhcool = Convert.ToDouble((this.jctxtRH.Text == "" ? SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH.ToString("n0") : this.jctxtRH.Text));
                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(DBH), Convert.ToDecimal(rhcool / 100), pressure));
               // double rh = pbll.CalculateRH(DBH, curSystemItem.WBHeating, thisProject.Altitude);
                //this.jclblRHValue.Text = (rh * 100).ToString("n0") + "%";
                jctxtWBH.Text = wb.ToString("n1"); 
                //this.jclblRHValue.Text = (rh * 100).ToString("n1");
                if (isTrue)
                {
                    RematchIndoor();
                    curSystemItem.WBHeating = Convert.ToDouble(wb.ToString("n1"));
                }
            }
        }
        
        void jctxtWBH_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);

            if (JCValidateSingle(jctxtWBH) && JCValidateSingle(jctxtDBH))
            {
                double DBH = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBH.Text == "" ? curSystemItem.DBHeating.ToString() : this.jctxtDBH.Text), UnitType.TEMPERATURE, ut_temperature);
                double WBH = Unit.ConvertToSource(Convert.ToDouble(this.jctxtWBH.Text == "" ? curSystemItem.WBHeating.ToString() : this.jctxtWBH.Text), UnitType.TEMPERATURE, ut_temperature);
                if (Convert.ToDecimal(DBH) < Convert.ToDecimal(WBH))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                    return;
                }
                 bool isTrue = isRematch();
                if (WBH == curSystemItem.WBHeating)
                    return; 
                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(DBH), Convert.ToDecimal(WBH), pressure));
                this.jctxtRH.Text = Math.Truncate((rh * 100)).ToString("n0");
                if (isTrue)
                {
                    RematchIndoor();
                    curSystemItem.WBHeating = WBH;
                    curSystemItem.RHHeating = Math.Truncate((rh * 100));
                }
            }
            
        }

        void jctxtRH_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            if (jctxtRH.Text.Length > 0)
            {
                if (JCValidateSingle(jctxtRH) && JCValidateSingle(jctxtDBH))
                {
                    if (Convert.ToDouble(jctxtRH.Text) > 100)
                        jctxtRH.Text = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingRH.ToString("n0");

                    if (jctxtRH.Text.ToString() == curSystemItem.RHHeating.ToString("n0"))
                        return;
                    double DBH = Unit.ConvertToSource(Convert.ToDouble(this.jctxtDBH.Text == "" ? curSystemItem.DBHeating.ToString() : this.jctxtDBH.Text), UnitType.TEMPERATURE, ut_temperature);
                    bool isTrue = isRematch();
                    FormulaCalculate fc = new FormulaCalculate();
                    decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                    double rhcool = Convert.ToDouble((this.jctxtRH.Text == "" ? SystemSetting.UserSetting.defaultSetting.OutdoorHeatingRH.ToString("n0") : this.jctxtRH.Text));
                    curSystemItem.RHHeating = rhcool;
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(DBH), Convert.ToDecimal(rhcool / 100), pressure));
                    this.jctxtWBH.Text = wb.ToString("n1");
                    if (isTrue)
                    {
                        RematchIndoor();
                    }
                }
            }
        }

        void jcbtnOK_Click(object sender, EventArgs e)
        {
             
            if (ERRList != null && ERRList.Count > 0)
            {
                ShowWarningInfo(ERRType.PopupWin);
                return;
            }
            if (curSystemItem.OutdoorItem == null)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (!JCValidateSingle(jctxtDBH) || !JCValidateSingle(jctxtRH) || !JCValidateSingle(jctxtWBH) || !JCValidateSingle(jctxtDBC))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }
            if (Convert.ToDecimal(jctxtDBH.Text) < Convert.ToDecimal(jctxtWBH.Text))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                return ;
            }
            if (!JCValidateSingle(jctxtEqPipeLength) || !JCValidateSingle(jctxtFirstPipeLength))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }
            DoOK();
             
            UndoRedo.UndoRedoHandler.SaveHistoryTraces();//保存历史痕迹 add by axj 20161228
        }

        void jcbtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

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
            resetTreeNodeColor(tvOutdoor.Nodes[0]);

            // 获取目标节点
            TreeView tv = sender as TreeView;
            Point pt = tv.PointToClient(new Point(e.X, e.Y));
            TreeNode dstNode = tv.GetNodeAt(pt);

            if (dstNode != null && dstNode.Level == 0)
            {
                dstNode.ForeColor = colorRed;
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        //判断当前的室内机是否已具有共享关系 on 2017-12-28 by xyj 
        private bool IsShareRemoteControl(RoomIndoor ri)
        {
            if (ri.IsMainIndoor) 
                return true;
            bool isShare = false;
            List<RoomIndoor> list = thisProject.RoomIndoorList.FindAll(p => p.IsMainIndoor == true);
            if (list != null && list.Count > 0)
            {
                foreach (RoomIndoor indoor in list)
                {
                    if (indoor.IndoorItemGroup != null)
                    {
                        foreach (RoomIndoor rind in indoor.IndoorItemGroup)
                        {
                            if (rind.IndoorNO == ri.IndoorNO && rind.IsExchanger == ri.IsExchanger)
                            {
                               return true;
                            }
                        }
                    }
                }
            }
            return isShare;
        }

        //判断当前的室内机是否已具有共享关系 返回MainIndoor on 2017-12-28 by xyj 
        private RoomIndoor GetMainIndoorByIndoor(RoomIndoor ri)
        {
            if (ri.IsMainIndoor)
                return ri;
            RoomIndoor RI = null;
            List<RoomIndoor> list = thisProject.RoomIndoorList.FindAll(p => p.IsMainIndoor == true);
            if (list != null && list.Count > 0)
            {
                foreach (RoomIndoor indoor in list)
                {
                    if (indoor.IndoorItemGroup != null)
                    {
                        foreach (RoomIndoor rind in indoor.IndoorItemGroup)
                        {
                            if (rind.IndoorNO == ri.IndoorNO && rind.IsExchanger == ri.IsExchanger)
                            {
                                return indoor;
                            }
                        }
                    }
                }
            }
            return RI;
        }



        /// ListView -> TreeView
        /// <summary>
        /// ListView -> TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvOutdoor_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode nodeOut = tvOutdoor.Nodes[0];
            resetTreeNodeColor(nodeOut);

            string typeLVItem = typeof(ListViewItem).ToString();
            if (e.Data.GetDataPresent(typeLVItem, false))
            {
                //foreach (ListViewItem sItem in lvIndoorList.SelectedItems)
                //{
                //    RoomIndoor ri = (RoomIndoor)sItem.Tag;
                //    listRISelected.Add(ri);
                //    sItem.Remove();
                //}
                List<string> roomIdList = new List<string>();  //选中的室内机所在的房间列表 add by Shen Junjie on 20170710
                List<RoomIndoor> movingIndoorList = new List<RoomIndoor>();

                #region  共享关联关系的室内机捆绑选型  modify on 20170623 by Lingjia Qiu
                List<RoomIndoor> riShareList = new List<RoomIndoor>();
                foreach (ListViewItem sItem in lvIndoorList.SelectedItems)
                {
                    RoomIndoor ri = (RoomIndoor)sItem.Tag;
                    movingIndoorList.Add(ri);

                    if (ri.RoomID != null && ri.RoomID != "" && !roomIdList.Contains(ri.RoomID))
                    {
                        roomIdList.Add(ri.RoomID); //add by Shen Junjie on 20170710
                    }
                }

                #region 同一个房间内的室内机需要同时移动  add by Shen Junjie on 20170710
                //同一房间中有部分室内机不能用于当前系统 add by Shen Junjie 2018/3/21
                for (int i = 0; i < roomIdList.Count; i++)
                {
                    string roomId = roomIdList[i];
                    List<RoomIndoor> unavilabeIndoorsInTheRoom = thisProject.RoomIndoorList.FindAll(p => p.RoomID == roomId && !listRIAvailable.Contains(p));
                    if (unavilabeIndoorsInTheRoom.Count > 0)
                    {
                        List<RoomIndoor> selectedIndInTheRoom = movingIndoorList.FindAll(p => p.RoomID == roomId);
                        string[] indoorNames = selectedIndInTheRoom.ConvertAll(p => p.IndoorFullName).ToArray();
                        Room r = pbll.GetRoom(roomId);
                        JCMsg.ShowWarningOK(Msg.OUTD_OTHERS_UNAVAILABLE_IN_SAME_ROOM(indoorNames, r == null ? "" : r.Name));
                        movingIndoorList.RemoveAll(p => p.RoomID == roomId);
                        roomIdList.RemoveAt(i);
                        i--;
                    }
                }

                dragIndWithSameRoom(roomIdList, movingIndoorList, false);
                #endregion

                foreach (var ri in movingIndoorList)
                {
                    if (!listRISelected.Contains(ri))
                        listRISelected.Add(ri);

                    if (ri.IndoorItemGroup != null)   //维护具有共享关系的室内机对象
                    {
                        bool isAdd = true;
                        foreach (RoomIndoor item in riShareList)
                        {
                            if (item.IndoorNO == ri.IndoorNO && item.IsExchanger == ri.IsExchanger)
                            {
                                isAdd = false;
                                break;
                            }
                        }
                        if (isAdd)
                            riShareList.Add(ri);
                    }
                    else
                    {
                        //判断当前的室内机是否已具有共享关系 on 2017-12-28 by xyj 
                        if (IsShareRemoteControl(ri))
                        {
                            //判断是否存在主INDOOR 
                            RoomIndoor main = GetMainIndoorByIndoor(ri);
                            bool isAdd = true;
                            foreach (RoomIndoor item in riShareList)
                            {
                                if (item.IndoorNO == main.IndoorNO && item.IsExchanger == main.IsExchanger)
                                {
                                    isAdd = false;
                                    break;
                                }
                            }
                            if (isAdd)
                                riShareList.Add(main);
                        }
                    }
                }

                //关联拖拽具有共享信息的对象至室内机节点
                if (!selectionBySharingRelationShip(riShareList, "Enter"))
                    return;
                #endregion
                #region   选中的室内机与其他房间室内机存在共享控制器关系   add on 20180714 by Vince
                bool isNeedDrag = false;
                //检查是否需要再次room关联拖拽
                foreach (RoomIndoor ridSelected in listRISelected)
                {
                    if (ridSelected.RoomID != null && ridSelected.RoomID != "" && !roomIdList.Contains(ridSelected.RoomID))
                    {
                        roomIdList.Add(ridSelected.RoomID); 
                        isNeedDrag = true;
                    }
                }
                if (isNeedDrag)
                {
                    //获取需要添加的ind信息进行添加列表
                    List<RoomIndoor> availableIndoorsInSameRoom = dragIndWithSameRoom(roomIdList, movingIndoorList, true);
                    if (availableIndoorsInSameRoom != null)
                    {
                        foreach (RoomIndoor avaiInd in availableIndoorsInSameRoom)
                        {
                            listRISelected.Add(avaiInd);
                        }
                    }
                    else
                        return;
                }
                #endregion
                //最后将所选择项及关联想删除节点  add on 20170623 by Lingjia Qiu
                foreach (ListViewItem sItem in lvIndoorList.Items)
                {
                    RoomIndoor ri = (RoomIndoor)sItem.Tag;
                    foreach (RoomIndoor ridSelected in listRISelected)
                    {
                        if (ri == ridSelected)
                        {                          
                            sItem .Remove();
                        }                      
                    }
                    
                    //删除重复的节点
                    if (listRISelected.FindAll(p => p.IndoorNO == ri.IndoorNO).Count > 1)
                    {
                        sItem.Remove();
                    }
                }
                

                ReselectOutdoor(); 
                curSystemItem.IsUpdated = true;
            }
        }

        void lvIndoorList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        void lvIndoorList_DragEnter(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.Move;
            string typeNode = typeof(TreeNode).ToString();
            if (e.Data.GetDataPresent(typeNode, false))
            {
                TreeNode sNode = (TreeNode)e.Data.GetData(typeNode);
                if (sNode != null && sNode.Level > 0)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                    e.Effect = DragDropEffects.None; // 室外机节点不允许拖拽

            }
        }

        // TreeView -> ListView
        void lvIndoorList_DragDrop(object sender, DragEventArgs e)
        {
            ListView lv = sender as ListView;

            string typeNode = typeof(TreeNode).ToString();
            if (e.Data.GetDataPresent(typeNode, false))
            {
                List<RoomIndoor> movingIndoors = new List<RoomIndoor>();
                for (int i = tvOutdoor.SelectedNodes.Count - 1; i >= 0; i--)
                {
                    TreeNode sNode = (TreeNode)tvOutdoor.SelectedNodes[i];
                    if (sNode.Level == 0)
                        continue;
                    RoomIndoor ri = sNode.Tag as RoomIndoor;
                    if (ri != null)
                    {
                        movingIndoors.Add(ri);
                    }
                }
                DetachIndoors(movingIndoors);

                ReselectOutdoor();
                curSystemItem.IsUpdated = true;
            }
        }

        private bool DetachIndoors(List<RoomIndoor> movingIndoors)
        {
            List<string> roomIdList = new List<string>();
            foreach (RoomIndoor ri in movingIndoors)
            {
                if (!string.IsNullOrEmpty(ri.RoomID) && !roomIdList.Contains(ri.RoomID))
                {
                    roomIdList.Add(ri.RoomID);  //先添加拖动室内机的房间ID
                    List<RoomIndoor> curRomlist = listRISelected.FindAll(p => p.RoomID == ri.RoomID);
                    //遍历拖动室内机房间下所有室内机是否存在关联remoteController关系
                    foreach (RoomIndoor curRi in curRomlist)
                    {
                        if (IsShareRemoteControl(curRi))   //如果拖拽的Indoor是共享控制器的室内机
                        {
                            //判断是否存在主INDOOR 
                            RoomIndoor main = GetMainIndoorByIndoor(curRi);
                            //维护存在关联关系的房间
                            foreach (RoomIndoor rid in main.IndoorItemGroup)
                            {
                                if (!string.IsNullOrEmpty(rid.RoomID) && !roomIdList.Contains(rid.RoomID))
                                {
                                    roomIdList.Add(rid.RoomID);
                                }
                            }
                            //主关联室外机进行维护
                            if (!string.IsNullOrEmpty(main.RoomID) && !roomIdList.Contains(main.RoomID))
                            {
                                roomIdList.Add(main.RoomID);
                            }
                        }
                    }

                }

            }

            //同一个房间内的室内机需要同时移动  add by Shen Junjie on 20170710
            bool needMoveOthers = false;
            List<string> roomIdExList = new List<string>();
            foreach (string roomId in roomIdList)
            {
                var list1 = listRISelected.FindAll(p => p.RoomID == roomId);
                var list2 = movingIndoors.FindAll(p => p.RoomID == roomId);

                if (list2.Count < list1.Count)
                {
                    needMoveOthers = true;

                    //将同一个Room的其它ind添加到movingIndoors
                    foreach (var ind in list1)
                    {
                        if (!movingIndoors.Contains(ind))
                            movingIndoors.Add(ind);
                    }
                }
            }

            string alertMsg = null;
            var indoorList = listRISelected; // thisProject.RoomIndoorList.FindAll(p => p.SystemID == curSystemItem.Id);
            //system下室内机全部移除掉
            if (movingIndoors.Count == indoorList.Count)
            {
                if (needMoveOthers)
                {
                    alertMsg = Msg.OUTD_TOGETHER_MOVE_DELETE_SYSTEM(curSystemItem.Name);//选中的室内机所在房间存在多个室内机将会被一起移动，室外机系统{0}移动后将被删除，确认是否继续操作？？
                }
                else
                {
                    alertMsg = Msg.OUTD_CONFIRM_DELETE_SYSTEM(curSystemItem.Name);//室外机系统{0}移动后将被删除，确认是否移动？
                }
            }
            else if (needMoveOthers)
            {
                alertMsg = Msg.OUTD_TOGETHER_MOVE_BYROOM; // "选中的室内机所在房间存在多个室内机将会被一起移动，确认是否继续操作？";
            }

            if (!string.IsNullOrEmpty(alertMsg))
            {
                DialogResult res = JCMsg.ShowConfirmOKCancel(alertMsg);
                if (res != System.Windows.Forms.DialogResult.OK)
                {
                    return false;
                }
            }
            List<RoomIndoor> riShareList = new List<RoomIndoor>();
            TreeNode outdoorTreeNode = tvOutdoor.Nodes[0];
            for (int i = 0; i < outdoorTreeNode.Nodes.Count; i++)
            {
                TreeNode sNode = (TreeNode)outdoorTreeNode.Nodes[i];
                if (sNode.Level == 0)
                    continue;
                RoomIndoor ri = (RoomIndoor)sNode.Tag;
                if (!movingIndoors.Contains(ri)) continue;
                outdoorTreeNode.Nodes.Remove(sNode);
                i--;
                listRISelected.Remove(ri);
                if (ri.IndoorItemGroup != null)   //维护具有共享关系的室内机对象
                    riShareList.Add(ri);
                else
                {
                    //判断当前的室内机是否已具有共享关系 on 2017-12-28 by xyj 
                    if (IsShareRemoteControl(ri))
                    {
                        //判断是否存在主INDOOR 
                        RoomIndoor main = GetMainIndoorByIndoor(ri);
                        bool isAdd = true;
                        foreach (RoomIndoor item in riShareList)
                        {
                            if (item.IndoorNO == main.IndoorNO && item.IsExchanger == main.IsExchanger)
                            {
                                isAdd = false;
                                break;
                            }
                        }
                        if (isAdd)
                            riShareList.Add(main);
                    }
                    doAddItemToListView(ri);
                }
            }

            ////关联拖拽具有共享信息的对象离开室内机节点
            if (!selectionBySharingRelationShip(riShareList, "Back"))
                return false;

            //当拖拽导致重新选型信息选项卡重新定位ODU
            pnlCondition_Cooling_indoor.Visible = false;
            pnlCondition_Heating_indoor.Visible = false;

            return true;
        }


        void dgvIndoorList_DragEnter(object sender, DragEventArgs e)
        {
            string typeNode = typeof(TreeNode).ToString();
            if (e.Data.GetDataPresent(typeNode, false))
            {
                TreeNode sNode = (TreeNode)e.Data.GetData(typeNode);
                if (sNode != null && sNode.Level > 0)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                    e.Effect = DragDropEffects.None; // 室外机节点不允许拖拽
            }
        }

        private void rbtnSameLevel_CheckedChanged(object sender, EventArgs e)
        {
            //this.pnlHighDiff.Visible = !rbtnSameLevel.Checked;
            ////2013-10-21 by Yang 修正了SameLevel判断取反的问题
            //if (rbtnSameLevel.Checked)
            //{
            //    jctxtHighDifference.Text = "0";
            //}
            //else
            //{
            //    BindMaxDiffHeight();
            //    jctxtHighDifference.Text = Math.Abs(Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length)).ToString("n1");
            //    CheckMaxDiffHeight();
            //}
        }

        private void tvOutdoor_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvOutdoor.SelectedNodes == null || tvOutdoor.SelectedNodes.Count == 0)
                return;           
            foreach (TreeNode node in tvOutdoor.SelectedNodes)
            {
                node.BackColor = UtilColor.bg_selected;
                if(node.ForeColor != Color.Chocolate)
                    node.ForeColor = UtilColor.font_selected;
            }
            TreeNode singlenNode = e.Node;
            BindTreeViewOutNodeInfo(singlenNode);
        }

        private void tvOutdoor_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (tvOutdoor.SelectedNodes == null || tvOutdoor.SelectedNodes.Count == 0)
                return;
            foreach (TreeNode selNode in tvOutdoor.SelectedNodes)
            {
                selNode.BackColor = tvOutdoor.BackColor;
                if (selNode.ForeColor != Color.Chocolate)   //已标注巧克力色特殊提醒忽略恢复字体颜色
                    selNode.ForeColor = tvOutdoor.ForeColor;
            }
        }

        private void jcbtnSetLength_Click(object sender, EventArgs e)
        {
            //if (!JCValidateSingle(jctxtHighDifference))
            //{
            //    JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
            //    return;
            //}
            if (!JCValidateSingle(jctxtFirstPipeLength))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }
            if (!JCValidateSingle(jctxtEqPipeLength))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                BindPipeLength();
                ReselectOutdoor();
            }
            catch (Exception exc)
            {
                JCMsg.ShowWarningOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void jctxtHighDifference_TextChanged(object sender, EventArgs e)
        {
            // 绑定高度差的最大最小值
            //this.jctxtHighDifference.JCMinValue = 0;

            //CheckMaxDiffHeight();

            //JCValidateSingle(jctxtHighDifference);
        }

        #endregion

        #region Response codes (Methods && Events)

        // 绑定界面单位表达式
        /// <summary>
        /// 绑定界面单位表达式
        /// </summary>
        private void BindUnit()
        {
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ut_waterFlowRate = SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate;

           // jclblUnitkW1.Text = ut_power;
            jclblUnitkW2.Text = ut_power;
           // jclblUnitkW3.Text = ut_power;
            jclblUnitkW4.Text = ut_power;
            jclblUnitkW5.Text = ut_power;
            jclblUnitkW6.Text = ut_power;
            jclblUnitkW7.Text = ut_power;
            jclblUnitkW8.Text = ut_power;
            jclblUnitTemperature1.Text = ut_temperature;
            jclblUnitTemperature2.Text = ut_temperature;
            jclblUnitTemperature3.Text = ut_temperature;

            //Add Water Source Inlet Water Temp. on 20160615 by Yunxiao Lin
            jclblUnitTemperature4.Text = ut_temperature;
            jclblUnitTemperature5.Text = ut_temperature;

            //2013-10-17 
            jcLabel7.Text = ut_length;
            jcLabel8.Text = ut_length;
           // jcLabel6.Text = ut_length;

            jclblCoolInletWaterFlowRateUnit.Text = ut_waterFlowRate;
            jclblHeatInletWaterFlowRateUnit.Text = ut_waterFlowRate;
        }

        // 绑定变量及控件的初始值
        /// <summary>
        /// 绑定变量及控件的初始值
        /// </summary>
        private void Init()
        {
            try
            {
                tvOutdoor.ShowLines = false;
                tvOutdoor.ItemHeight = 30;
                tvOutdoor.FullRowSelect = true;
                tvOutdoor.Nodes.Clear();
                tvOutdoor.Nodes.Add(curSystemItem.Name);

                // GroupBox 状态绑定
                pnlCondition_Cooling.Enabled = thisProject.IsCoolingModeEffective;
                pnlCondition_Heating.Enabled = thisProject.IsHeatingModeEffective;

                // 变量初始值绑定，当前系统已选的室内机列表
                if (JCFormMode == FormMode.EDIT)
                {
                    listRISelected = pbll.GetSelectedIndoorBySystem(curSystemItem.Id);
                    jccbxCompositeMode.Checked = (curSystemItem.SysType == SystemType.CompositeMode);
                }
                // 绑定系统名称
                this.jctxtName.Text = curSystemItem.Name;

                // 绑定环境温度的取值范围
                this.jctxtDBC.JCMinValue = float.Parse(Unit.ConvertToControl(10, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//-5 

                this.jctxtDBC.Text = Unit.ConvertToControl(curSystemItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                if (_series.Contains("FSXNSE"))
                    this.jctxtDBC.JCMaxValue = float.Parse(Unit.ConvertToControl(48, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//48
                else
                    this.jctxtDBC.JCMaxValue = float.Parse(Unit.ConvertToControl(52, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//52
                JCValidateSingle(jctxtDBC);
                // T3的制冷室外温度最高为52 20161028 by Yunxiao Lin
                if (thisProject.SubRegionCode == "ME_T3A" || thisProject.SubRegionCode == "ME_T3B")
                    this.jctxtDBC.JCMaxValue = float.Parse(Unit.ConvertToControl(52, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//52
                this.jctxtDBH.JCMinValue = float.Parse(Unit.ConvertToControl(-18, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
                this.jctxtDBH.JCMaxValue = float.Parse(Unit.ConvertToControl(33, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
                this.jctxtWBH.JCMinValue = float.Parse(Unit.ConvertToControl(-20, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
                this.jctxtWBH.JCMaxValue = float.Parse(Unit.ConvertToControl(15, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//24
                this.jctxtRH.JCMinValue = float.Parse("0");
                this.jctxtRH.JCMaxValue = float.Parse("100");
               
                //制冷进水温度限制20-35度
                this.jctxtIWC.JCMinValue = float.Parse(Unit.ConvertToControl(20, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//20
                this.jctxtIWC.JCMaxValue = float.Parse(Unit.ConvertToControl(35, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//35
                //制热进水温度因为只有15度可用，所以不允许修改
                //this.jctxtIWH.JCMinValue = float.Parse(Unit.ConvertToControl(10, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//10
                //this.jctxtIWH.JCMaxValue = float.Parse(Unit.ConvertToControl(45, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//45
                this.jctxtIWH.Enabled = false;
                // 绑定环境温度值
                
                this.jctxtDBH.Text = Unit.ConvertToControl(curSystemItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtWBH.Text = Unit.ConvertToControl(curSystemItem.WBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtIWC.Text = Unit.ConvertToControl(curSystemItem.IWCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                this.jctxtIWH.Text = Unit.ConvertToControl(curSystemItem.IWHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1");
                 
                double rh = (new FormulaCalculate()).CalculateRH(curSystemItem.DBHeating, curSystemItem.WBHeating, thisProject.Altitude);
                this.jctxtRH.Text = Math.Truncate((rh * 100)).ToString("n0");
                
              //  this.jclblRHValue.Text = (rh * 100).ToString("n1");
                // 绑定管长默认值
                // add on 20160401 clh 当前系统管长为手动输入时，此处不能修改
                this.jctxtEqPipeLength.Enabled = !curSystemItem.IsInputLengthManually;
                this.jctxtFirstPipeLength.Enabled = !curSystemItem.IsInputLengthManually;

                //if (curSystemItem.PipingPositionType == PipingPositionType.Upper)
                //    this.rbtnHigher.Checked = true;
                //else if (curSystemItem.PipingPositionType == PipingPositionType.Lower)
                //    this.rbtnLower.Checked = true;
                //else
                //    this.rbtnSameLevel.Checked = true;
                 
                //BindMaxDiffHeight();
                //this.jctxtHighDifference.Text = Math.Abs(Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length)).ToString("n1");
                //CheckMaxDiffHeight();
                this.jclblPipingFactorValue.Text = "0";
                //(new PipingBLL()).GetPipeLengthFactor(sysItem, "Cooling").ToString("n3");
                if (curSystemItem.IsAuto)
                {
                    jccbxAuto.Checked = true;
                    jccbxManual.Checked = false;
                    jccmbOutdoor.Enabled = false;
                }
                else
                {
                    jccbxAuto.Checked = false;
                    jccbxManual.Checked = true;
                    jccmbOutdoor.Enabled = true;
                }
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK(exc.Message + "\n" + exc.StackTrace);
                Close();
            }
        }

        private void switchInletWater()
        {
            //bool isWaterSource = jccmbType.Text.ToLower().Contains("water source");
            bool isWaterSource = jccmbType.SelectedValue.ToString().ToLower().Contains("water source");
            //Cooling DB
            jclblCoolDB.Visible = !isWaterSource;
            jctxtDBC.Visible = !isWaterSource;
            jclblUnitTemperature1.Visible = !isWaterSource;
            //Cooling Inlet Water
            jclblCoolIW.Visible = isWaterSource;
            jctxtIWC.Visible = isWaterSource;
            jclblUnitTemperature4.Visible = isWaterSource;
            //Heating DB
            jclblDBH.Visible = !isWaterSource;
            jctxtDBH.Visible = !isWaterSource;
            jclblUnitTemperature2.Visible = !isWaterSource;
            //Heating WB
            jclblWBH.Visible = !isWaterSource;
            jctxtWBH.Visible = !isWaterSource;
            jclblUnitTemperature3.Visible = !isWaterSource;
            //Heating RH
            jclblRH.Visible = !isWaterSource;
            jclblRHValue.Visible = !isWaterSource;
            jcLabel1.Visible = !isWaterSource;
            //Heating Inlet Water
            jclblHeatIW.Visible = isWaterSource;
            jctxtIWH.Visible = isWaterSource;
            jclblUnitTemperature5.Visible = isWaterSource;

            jclblInletWaterFlowRateLevel.Visible = isWaterSource;
            jccmbInletWaterFlowRateLevel.Visible = isWaterSource;
            jclblCoolInletWaterFlowRate.Visible = isWaterSource;
            jctxtCoolInletWaterFlowRate.Visible = isWaterSource;
            jclblCoolInletWaterFlowRateUnit.Visible = isWaterSource;
            jclblHeatInletWaterFlowRate.Visible = isWaterSource;
            jctxtHeatInletWaterFlowRate.Visible = isWaterSource;
            jclblHeatInletWaterFlowRateUnit.Visible = isWaterSource;

            if (isWaterSource)
            {
                if (curSystemItem.FlowRateLevel == FlowRateLevels.NA)
                {
                    curSystemItem.FlowRateLevel = FlowRateLevels.High;
                }
                jccmbInletWaterFlowRateLevel.SelectedIndex = 3 - (int)curSystemItem.FlowRateLevel;
            }
            else
            {
                curSystemItem.FlowRateLevel = FlowRateLevels.NA;
            }

            //if (isWaterSource)
            //{
            //    //Cooling Inlet Water
            //    jclblCoolIW.Location = new Point(10, 150);
            //    jctxtIWC.Location = new Point(97, 149);
            //    jclblUnitTemperature4.Location = new Point(129, 153);
            //    //Heating Inlet Water
            //    jclblHeatIW.Location = new Point(10, 180);
            //    jctxtIWH.Location = new Point(97, 179);
            //    jclblUnitTemperature5.Location = new Point(129, 183);
            //}
            //else
            //{
            //    //Cooling Inlet Water
            //    jclblCoolIW.Location = new Point(108, 150);
            //    jctxtIWC.Location = new Point(195, 149);
            //    jclblUnitTemperature4.Location = new Point(227, 153);
            //    //Heating Inlet Water
            //    jclblHeatIW.Location = new Point(108, 180);
            //    jctxtIWH.Location = new Point(195, 179);
            //    jclblUnitTemperature5.Location = new Point(227, 183);
            //}
        }

        // 绑定室外机类型列表
        /// <summary>
        /// 绑定室外机类型列表
        /// </summary>
        private void BindOutdoorTypeList()
        {
            //if (thisProject == null || string.IsNullOrEmpty(thisProject.SubRegionCode) || string.IsNullOrEmpty(thisProject.ProductType))
            //if (thisProject == null || string.IsNullOrEmpty(thisProject.SubRegionCode) || string.IsNullOrEmpty(_productType))
            if (thisProject == null || string.IsNullOrEmpty(thisProject.SubRegionCode) || string.IsNullOrEmpty(_series))
                return;

            string colName = "";
            //DataTable dt = bll.GetOutdoorTypeList(out colName);
            //DataTable dt = bll.GetOutdoorTypeList(out colName, _productType);
            DataTable dt = bll.GetOutdoorTypeListBySeries(out colName, _series);
            //this.jccmbType.DisplayMember = colName;
            this.jccmbType.DisplayMember = "Trans_Name";
            this.jccmbType.ValueMember = colName;
            dt = trans.getTypeTransDt(TransType.Outdoor.ToString(), dt, colName);
            this.jccmbType.DataSource = dt;

            //if (JCFormMode == FormMode.NEW)
            //{
            //    curSystemItem.SelOutdoorType = jccmbType.Text.Trim();
            //    setTypeImage("");
            //}
            //else if (isFirst)
            //{
            //    jccmbType.Text = curSystemItem.SelOutdoorType;
            //}           
            //curSystemItem.SelOutdoorType = jccmbType.Text.Trim();
            curSystemItem.SelOutdoorType = jccmbType.SelectedValue.ToString();
            setTypeImage("");            
           
            switchInletWater();
        }

        private void BindCmbProductType()
        {
            this.jccmbProductType.Items.Clear();
            //_productType = "";
            _series = "";
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            DataTable dt;
            if (this.JCFormMode == FormMode.EDIT)
            {
                dt = productTypeBll.GetProductTypeData(Registr.Registration.SelectedBrand.Code, Registr.Registration.SelectedSubRegion.Code);
            }
            else
            {
                //新增室外机时，显示所有未指派的室内机的Series on 20170302 by shen junjie
                //dt = productTypeBll.GetProductTypeListOfAssignableIndoors(
                //    Registr.Registration.SelectedBrand.Code,
                //    Registr.Registration.SelectedSubRegion.Code,
                //    thisProject.RoomIndoorList);

                dt = productTypeBll.GetOduSeriesByIduType(
                    Registr.Registration.SelectedBrand.Code,
                    Registr.Registration.SelectedSubRegion.Code,
                    thisProject.RoomIndoorList);
            }
            string colName = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                //this.jccmbProductType.ValueMember = "ProductType";
                if (this.JCFormMode == FormMode.EDIT)
                    //this.jccmbProductType.ValueMember = "Series";
                    colName = "Series";
                else
                    //this.jccmbProductType.ValueMember = "ODU_Series";
                    colName = "ODU_Series";
                this.jccmbProductType.ValueMember = colName;
                //翻译                
                dt = trans.getTypeTransDt(TransType.Series.ToString(), dt, colName);
                this.jccmbProductType.DisplayMember = "Trans_Name";

                this.jccmbProductType.DataSource = dt;
                if (this.JCFormMode == FormMode.EDIT)
                {
                    //this.jccmbProductType.SelectedValue = curSystemItem.OutdoorItem.ProductType;
                    if (curSystemItem.OutdoorItem == null)
                        this.jccmbProductType.SelectedIndex = 0;
                    else
                        this.jccmbProductType.SelectedValue = curSystemItem.OutdoorItem.Series;
                    //this.jccmbProductType.Enabled = false;
                }
                else
                {
                    ////新增室外机的ProductType默认为当前项目第一个未分配室内机的ProductType 20160826 by Yunxiao Lin
                    //if (thisProject.RoomIndoorList.Count > 0)
                    //{
                    //    RoomIndoor validri = null;
                    //    foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                    //    {
                    //        if (string.IsNullOrEmpty(ri.SystemID))
                    //        {
                    //            validri = ri;
                    //            break;
                    //        }
                    //    }
                    //    if (validri != null)
                    //        //this.jccmbProductType.SelectedValue = validri.IndoorItem.ProductType;
                    //        this.jccmbProductType.SelectedIndex = this.jccmbProductType.FindString(validri.IndoorItem.Series);
                    //    //else
                    //    //    this.jccmbProductType.SelectedValue = thisProject.ProductType;
                    //}
                    ////else
                    ////    this.jccmbProductType.SelectedValue = thisProject.ProductType;

                    //只有未指派室内机的Series，所以只要选中第一个即可  on 20170302 by shen junjie
                    this.jccmbProductType.SelectedIndex = 0;
                }
                //_productType = this.jccmbProductType.SelectedValue.ToString();
                _series = this.jccmbProductType.SelectedValue.ToString();
                //_productType = productTypeBll.GetProductTypeBySeries(Registr.Registration.SelectedBrand.Code,
                //    Registr.Registration.SelectedSubRegion.Code, _series);
            }
        }

        // 绑定尚未分配给室外机的室内机列表
        // 由于ProductType的需求，增加ProductType参数 20160822 by Yunxiao Lin
        /// <summary>
        /// 绑定尚未分配给室外机的室内机列表
        /// </summary>
        private void BindIndoorAvailable(string series)
        {
            listRIAvailable.Clear();
            lvIndoorList.Items.Clear();

            if (string.IsNullOrEmpty(series))
                return;
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            DataTable typeDt = productTypeBll.GetIduTypeBySeries(Registr.Registration.SelectedBrand.Code,
                    Registr.Registration.SelectedSubRegion.Code, series);

            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (!string.IsNullOrEmpty(ri.SystemID) && ri.SystemID != curSystemItem.Id)
                {
                    continue;
                }
                bool isAvailable = false;
                foreach (DataRow dr in typeDt.Rows)
                {
                    //从未分配给室外机的室内机列表中过滤出指定productType的室内机 20160822 by Yunxiao Lin
                    if (ri.IndoorItem.Type == dr["IDU_UnitType"].ToString())
                    {
                        //关联表中IDU_Model_Hitachi 为白名单，过滤可添加的室内机分配列表
                        string modelHitachi = dr["IDU_Model_Hitachi"].ToString();
                        if (string.IsNullOrEmpty(modelHitachi) || modelHitachi.Contains(";" + ri.IndoorItem.Model_Hitachi + ";"))
                        {
                            listRIAvailable.Add(ri);
                            isAvailable = true;
                            break;
                        }
                    }
                }
                if (!isAvailable)
                {
                    continue;
                }
                if (listRISelected != null && listRISelected.Contains(ri))
                {
                    //跳过已选择列表中的Indoor unit add by Shen Junjie 2018/3/20
                    continue;
                }
                doAddItemToListView(ri);
            }
        }

        // 绑定已选的信息
        /// <summary>
        /// 绑定已选的信息
        /// </summary>
        private void BindTreeViewOutdoor()
        {
            tvOutdoor.Nodes.Clear();
            tvOutdoor.Nodes.Add(curSystemItem.Name);

            ReselectOutdoor();

            if (tvOutdoor.Nodes.Count > 0)
            {
                tvOutdoor.SelectedNode = tvOutdoor.Nodes[0];
            }
        }

        // 控件事件绑定
        /// <summary>
        /// 控件事件绑定
        /// </summary>
        private void BindEvent()
        {
            // TreeView 控件事件绑定
            this.tvOutdoor.AllowDrop = true;
            tvOutdoor.ItemDrag += new ItemDragEventHandler(tvOutdoor_ItemDrag);
            tvOutdoor.DragOver += new DragEventHandler(tvOutdoor_DragOver);
            tvOutdoor.DragDrop += new DragEventHandler(tvOutdoor_DragDrop);

            // ListView 控件事件绑定
            this.lvIndoorList.AllowDrop = true;
            this.lvIndoorList.FullRowSelect = true;
            this.lvIndoorList.MultiSelect = true;
            lvIndoorList.ItemDrag += new ItemDragEventHandler(lvIndoorList_ItemDrag);
            lvIndoorList.DragEnter += new DragEventHandler(lvIndoorList_DragEnter);
            lvIndoorList.DragDrop += new DragEventHandler(lvIndoorList_DragDrop);

            this.jctxtDBC.Leave += new EventHandler(jctxtDBC_Leave);
            this.jctxtDBH.Leave += new EventHandler(jctxtDBH_Leave);
            this.jctxtWBH.Leave += new EventHandler(jctxtWBH_Leave);
            this.jctxtRH.Leave += new EventHandler(jctxtRH_Leave);
            //Add Water Source Inlet Temp. on 20160615 by Yunxiao Lin
            this.jctxtIWC.Leave += new EventHandler(jctxtIWC_Leave);
            this.jctxtIWH.Leave += new EventHandler(jctxtIWH_Leave);
        }

        // 计算室内机容量之和
        /// <summary>
        /// 计算室内机容量之和
        /// </summary>
        private void DoCalculateIndoorEstCapSum(List<RoomIndoor> listRISelected)
        {
            tot_indcap_c = 0;
            tot_indcap_h = 0;
            tot_act_indcap_c = 0;
            tot_act_indcap_h = 0;
            tot_FAcap = 0;

            tot_indcap_c_rat = 0;
            tot_indcap_h_rat = 0;

            foreach (RoomIndoor ri in listRISelected)
            {
                tot_indcap_c += ri.CoolingCapacity;
                tot_indcap_h += ri.HeatingCapacity;
                tot_act_indcap_c += ri.ActualCoolingCapacity;
                tot_act_indcap_h += ri.ActualHeatingCapacity;
                tot_indcap_c_rat += ri.IndoorItem.CoolingCapacity;
                tot_indcap_h_rat += ri.IndoorItem.HeatingCapacity;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    tot_FAcap += ri.IndoorItem.CoolingCapacity;
                }
            }
        }

        // 将系统当前数据绑定到控件
        /// <summary>
        /// 将系统当前数据绑定到控件
        /// </summary>
        private void BindToControl(SelectOutdoorResult result)
        {
            TreeNode nodeOut = tvOutdoor.Nodes[0];
            BindTreeNodeOut(nodeOut, curSystemItem, listRISelected);

            DoCalculateIndoorEstCapSum(listRISelected);

            bool IsOnlyFA = (curSystemItem.SysType == SystemType.OnlyFreshAir);
            if (result == SelectOutdoorResult.OK)
            {
                setTypeImage(curSystemItem.OutdoorItem.TypeImage);

                //this.jccmbType.Text = curSystemItem.OutdoorItem.Type;
                this.jccmbType.SelectedValue = curSystemItem.OutdoorItem.Type;
                curSystemItem.SelOutdoorType = curSystemItem.OutdoorItem.Type;
                this.jclblModelValue.Text = curSystemItem.OutdoorItem.AuxModelName; // 辅助型号名
                this.jclblMaxIndNumValue.Text = curSystemItem.OutdoorItem.MaxIU.ToString();
                double rh = pbll.CalculateRH(curSystemItem.DBHeating, curSystemItem.WBHeating, thisProject.Altitude);
                this.jclblRHValue.Text = (rh * 100).ToString("n1");

                this.jclblOutRatCapCValue.Text = Unit.ConvertToControl(curSystemItem.OutdoorItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n2");
                this.jclblOutRatCapHValue.Text = Unit.ConvertToControl(curSystemItem.OutdoorItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n2");

                this.jclblPipingFactorValue.Text = curSystemItem.PipingLengthFactor.ToString("n3");

            }
            else
            {
                this.jclblModelValue.Text = "-";
                this.jclblMaxIndNumValue.Text = "-";
                //edit by axj 20170110 显示室外机额定容量
                if (curSystemItem != null && curSystemItem.OutdoorItem != null)
                {
                    this.jclblOutRatCapCValue.Text = Unit.ConvertToControl(curSystemItem.OutdoorItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n2");
                    this.jclblOutRatCapHValue.Text = Unit.ConvertToControl(curSystemItem.OutdoorItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n2");
                }
                else
                {
                    this.jclblOutRatCapCValue.Text = "0.00";
                    this.jclblOutRatCapHValue.Text = "0.00";
                }
                //this.jclblPipingFactorValue.Text = "0";
            }
            this.jccmbType.Enabled = !IsOnlyFA;
            this.jccmbMaxRatio.Enabled = !IsOnlyFA;
            this.jccmbDiversity.Enabled = !IsOnlyFA;

            //this.jccbxCompositeMode.Checked = (sysItem.SysType == SystemType.CompositeMode);

            this.jclblActualRatioValue.Text = (curSystemItem.Ratio * 100).ToString("n0") + "%";

            //this.jclblInTotalCapCValue.Text = Unit.ConvertToControl(tot_indcap_c, UnitType.POWER, ut_power).ToString("n2");
            //this.jclblInTotalCapHValue.Text = Unit.ConvertToControl(tot_indcap_h, UnitType.POWER, ut_power).ToString("n2");

            //this.jclblTotalActCapCValue.Text = Unit.ConvertToControl(tot_act_indcap_c, UnitType.POWER, ut_power).ToString("n2");
            //this.jclblTotalActCapHValue.Text = Unit.ConvertToControl(tot_act_indcap_h, UnitType.POWER, ut_power).ToString("n2");

            this.jclblInTotalRatCapCValue.Text = Unit.ConvertToControl(tot_indcap_c_rat, UnitType.POWER, ut_power).ToString("n2");
            this.jclblInTotalRatCapHValue.Text = Unit.ConvertToControl(tot_indcap_h_rat, UnitType.POWER, ut_power).ToString("n2");

            if (!thisProject.IsCoolingModeEffective)
                this.jclblInTotalRatCapCValue.Text = "0.00";
            //if (!thisProject.IsHeatingModeEffective)
            // 由于一个Project中可能存在多个不同的ProductType，因此单凭IsHeatingModeEffective无法确定一个System是否需要制热。
            // 如果productType中包含", CO"，那么这个System是单冷系统，就算IsHeatingModeEffective=true，也不需要制热功能。 20160826 by Yunxiao Lin
            //if (!thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
            if (!thisProject.IsHeatingModeEffective)
                this.jclblInTotalRatCapHValue.Text = "0.00";

            this.jclblSystemRatCapCValue1.Text = Unit.ConvertToControl(curSystemItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n2");
            this.jclblSystemRatCapHValue1.Text = Unit.ConvertToControl(curSystemItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n2");

            this.jccmbMaxRatio.Text = (curSystemItem.MaxRatio * 100).ToString();
            this.jccmbDiversity.Text = curSystemItem.DiversityFactor.ToString(); 

            this.jctxtEqPipeLength.JCMinValue = 0;
            this.jctxtEqPipeLength.JCMaxValue = (float)Unit.ConvertToControl(curSystemItem.MaxEqPipeLength==0? 190 : curSystemItem.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);

            this.jctxtFirstPipeLength.JCMinValue = 0;
            this.jctxtFirstPipeLength.JCMaxValue = (float)Unit.ConvertToControl(curSystemItem.MaxIndoorLength == 0 ? 90 : curSystemItem.MaxIndoorLength, UnitType.LENGTH_M, ut_length);

            this.jctxtEqPipeLength.Text = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length).ToString("n1");
            this.jctxtFirstPipeLength.Text = Unit.ConvertToControl(curSystemItem.FirstPipeLength, UnitType.LENGTH_M, ut_length).ToString("n1");

            //自动绑定当前室外机 2016/12/23 by axj
            if (curSystemItem.OutdoorItem != null)
            {
                this.jccmbOutdoor.SelectedValue = curSystemItem.OutdoorItem.ModelFull;
                BindMaxRatio();
            }
            else
            {
                this.jccmbOutdoor.SelectedIndex = 0;
            }
             


            if (curSystemItem.FlowRateLevel != FlowRateLevels.NA)
            {
                this.jccmbInletWaterFlowRateLevel.SelectedIndex = 3 - (int)curSystemItem.FlowRateLevel;
            }
            this.jctxtCoolInletWaterFlowRate.Text = Unit.ConvertToControl(curSystemItem.CoolingFlowRate, UnitType.WATERFLOWRATE, ut_waterFlowRate).ToString("n1");
            this.jctxtHeatInletWaterFlowRate.Text = Unit.ConvertToControl(curSystemItem.HeatingFlowRate, UnitType.WATERFLOWRATE, ut_waterFlowRate).ToString("n1");
        }

        // 将系统对象添加到系统树节点
        /// <summary>
        /// 将系统对象添加到系统树节点
        /// </summary>
        /// <param name="tv"> TreeView 控件</param>
        /// <param name="sysItem"> 系统对象 </param>
        public void BindTreeNodeOut(TreeNode nodeOut, SystemVRF sysItem, List<RoomIndoor> listRISelected)
        {
            nodeOut.Tag = sysItem;
            nodeOut.Name = sysItem.Id;
            nodeOut.ForeColor = UtilColor.ColorOriginal;
            if (listRISelected == null || listRISelected.Count == 0 || sysItem.OutdoorItem == null)
            {
                nodeOut.Text = sysItem.Name;
                nodeOut.ForeColor = UtilColor.ColorWarning;
                sysItem.Ratio = 0;
                Global.SetTreeNodeImage(nodeOut, 0, 0);
            }
            else
            {
                string sRatio = (sysItem.Ratio * 100).ToString("n0") + "%";
                nodeOut.Text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "] - " + sRatio;
                //if (thisProject.BrandCode == "Y")
                //    nodeOut.Text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "] - " + sRatio;
                //else
                //    nodeOut.Text = sysItem.Name + "[" + sysItem.OutdoorItem.Model_Hitachi + "] - " + sRatio;

                if (ERRList != null && ERRList.Count > 0)
                {
                    Global.SetTreeNodeImage(nodeOut, 0, 0);
                    nodeOut.ForeColor = UtilColor.ColorWarning;
                }
                else
                {
                    Global.SetTreeNodeImage(nodeOut, 1, 3);
                    nodeOut.ForeColor = UtilColor.ColorOriginal;
                }
            }


            nodeOut.Nodes.Clear();
            foreach (RoomIndoor ri in listRISelected)
            {
                TreeNode nodeIn = new TreeNode();
                nodeIn.Tag = ri;
                nodeIn.Name = ri.IndoorNO.ToString();
                string model = thisProject.BrandCode == "Y" ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
                nodeIn.Text = ri.IndoorName + "[" + model + "]";
                nodeOut.Nodes.Add(nodeIn);
                Global.SetTreeNodeImage(nodeIn, 2, 4);
                ChangeIndoorNodesColor(ri, nodeIn);
            }
            nodeOut.Expand();
        }

        /// <summary>
        /// 将当前选择的室外机系统数据加入当前项目文件
        /// </summary>
        private void BindPipeLength()
        {
            double fl = Convert.ToDouble(this.jctxtFirstPipeLength.Text);
            double eql = Convert.ToDouble(this.jctxtEqPipeLength.Text);
            //double hd = Convert.ToDouble(this.jctxtHighDifference.Text);
            curSystemItem.FirstPipeLength = Unit.ConvertToSource(fl, UnitType.LENGTH_M, ut_length);
            curSystemItem.PipeEquivalentLength = Unit.ConvertToSource(eql, UnitType.LENGTH_M, ut_length);
            if (!curSystemItem.IsInputLengthManually)
            {
                //自动配管需要保存修改的管长 20160701 by Yunxiao Lin
                curSystemItem.FirstPipeLengthbuff = curSystemItem.FirstPipeLength;
                curSystemItem.PipeEquivalentLengthbuff = curSystemItem.PipeEquivalentLength;
            }
            //if (this.rbtnSameLevel.Checked)
            //{
            //    curSystemItem.PipingPositionType = PipingPositionType.SameLevel;
            //    //curSystemItem.HeightDiff = 0;
            //}
            //else
            //{
            //    double diff = hd;
            //    if (this.rbtnHigher.Checked)
            //    {
            //        curSystemItem.PipingPositionType = PipingPositionType.Upper;
            //    }
            //    else
            //    {
            //        curSystemItem.PipingPositionType = PipingPositionType.Lower;
            //        //diff = 0 - hd;
            //    }
            //    curSystemItem.HeightDiff = Unit.ConvertToSource(diff, UnitType.LENGTH_M, ut_length);
            //}

            double factor = GetPipeLengthRevisedFactor(curSystemItem, "Cooling");
            this.jclblPipingFactorValue.Text = factor.ToString("n3");
        }

        // 确认保存当前结构
        /// <summary>
        /// 确认保存当前结构
        /// </summary>
        private void DoOK()
        {

            if (JCValidateSingle(jctxtName))
            {
                // 校验 Outdoor Name ,输入的字符中不能含有特殊字符
                string nameType = "[" + jclblName.Text + "]";
                if (!CDF.CCL.ValidateName(jctxtName.Text, nameType) || jctxtName.Text.Trim().Length == 0)
                    return;

                curSystemItem.Name = this.jctxtName.Text;


                //2013-10-22 by Yang 增加配管长度和高度的判断
                //最长等效管长判断
                if (curSystemItem.OutdoorItem.MaxEqPipeLength < curSystemItem.PipeEquivalentLength)
                {
                    // JCMsg.ShowConfirmOKCancel(Msg.PIPING_EQLENGTH(curSystemItem.MaxEqPipeLength.ToString("n0"))); //已设置好的 EqPipeLength 大于MaxEqPipeLength 190 on 20180307 by xyj
                    JCMsg.ShowErrorOK(Msg.PIPING_EQLENGTH(curSystemItem.MaxEqPipeLength.ToString("n0"),curSystemItem.MaxEqPipeLength.ToString("n0")));
                    return;
                }
                //第一分歧管到最远端室内机判断, 自动配管计算时，不判断
                //if (sysItem.MaxIndoorLength < (sysItem.PipeEquivalentLength - sysItem.FirstPipeLength))
                //{
                //    return;
                //}

                //室内外机高度差判断
                //if (curSystemItem.PipingPositionType == PipingPositionType.Upper)
                //{
                //    if ((curSystemItem.MaxOutdoorAboveHeight < curSystemItem.HeightDiff))
                //    {
                //        JCMsg.ShowConfirmOKCancel(Msg.PIPING_DIFF_INABOVE(""));
                //        return;
                //    }
                //}
                //if (curSystemItem.PipingPositionType == PipingPositionType.Lower)
                //{
                //    if ((curSystemItem.MaxOutdoorBelowHeight < curSystemItem.HeightDiff))
                //    {
                //        JCMsg.ShowConfirmOKCancel(Msg.PIPING_DIFF_INBELOW(""));
                //        return;
                //    }
                //}



                //判断室内机是否完全满足需求 20161125 by Yunxiao Lin
                if (thisProject.RoomIndoorList != null)
                {
                    foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                    {
                        if (ri.SystemID == curSystemItem.Id && !CommonBLL.FullMeetRoomRequired(ri, thisProject))
                        {
                            if (JCMsg.ShowConfirmYesNoCancel(Msg.GetResourceString("CONFIRM_TOLERANCE")) == DialogResult.No)
                            {
                                curSystemItem.IDUFirst = true;
                                SelectOutdoorResult result = Global.DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
                                BindToControl(result);
                                ShowWarningInfo(ERRType.StatusBar);
                                return;
                            }
                            else
                                break;

                        }
                    }
                }

                if (this.JCFormMode == FormMode.NEW)
                {
                    pbll.AddSystemVRF(curSystemItem);
                }
                else
                {
                    pbll.ClearIndoorListFromSystem(curSystemItem.Id);
                }

                // 更新当前系统的室内机
                foreach (RoomIndoor ri in listRISelected)
                {
                    ri.SetToSystemVRF(curSystemItem.Id);
                }

                //更新更新当前系统的室内机的顺序
                DoUpdateRoomIndoorListOrder(curSystemItem.Id);

                if (curSystemItem.ControlGroupID.Count>0)
                    thisProject.CentralControllerOK = false;

                DialogResult = DialogResult.OK;
            }
        }

        private void DoUpdateRoomIndoorListOrder(string systemId)
        {
            thisProject.RoomIndoorList.RemoveAll(c => (c.SystemID == systemId));
            thisProject.RoomIndoorList.AddRange(listRISelected);
        }

        #region 辅助方法

        // 设置界面机组图片
        /// <summary>
        /// 设置界面机组图片
        /// </summary>
        /// <param name="imageName">若为空则默认显示当前类型的标准机组，第一条记录的图片</param>
        private void setTypeImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                Util.SetTypeImage(pbOutdoor, imageName);
                return;
            }

            if (dtOutdoorStd == null)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
            }
            if (dtOutdoorStd != null)
            {
                //string filter = "UnitType = '" + jccmbType.Text.Trim() + "'";
                string filter = "UnitType = '" + jccmbType.SelectedValue.ToString() + "'";
                DataRow[] rows = dtOutdoorStd.Select(filter);
                if (string.IsNullOrEmpty(imageName) && rows.Length > 0)
                {
                    imageName = rows[0][Name_Common.TypeImage].ToString();
                    Util.SetTypeImage(pbOutdoor, imageName);
                }
            }
            else
            {
                JCMsg.ShowErrorOK("Outdoor Std table " + Msg.DB_NODATA);
            }
        }

        /// <summary>
        /// 将 RoomIndoor 对象添加到 ListView 中
        /// </summary>
        /// <param name="ri"></param>
        private void doAddItemToListView(RoomIndoor ri)
        {
            ListViewItem lvItem = new ListViewItem();
            lvItem.Tag = ri;
            if (thisProject.BrandCode == "Y")
                lvItem.Text = ri.IndoorItem.Model_York;
            else
                lvItem.Text = ri.IndoorItem.Model_Hitachi;
            lvItem.SubItems.Add(ri.IndoorName);

            string RoomName = "";
            if (!string.IsNullOrEmpty(ri.RoomID))
            {
                Room room = pbll.GetRoom(ri.RoomID);
                if (room != null)
                    RoomName = room.Name;
            }
            else {
                if (!string.IsNullOrEmpty(ri.DisplayRoom))
                {
                    RoomName = ri.DisplayRoom;
                }
            } 
            lvItem.SubItems.Add(RoomName);

            bool isdefaultAdd = true;

            //判断当前的lvIndoorList 是否存在Indoor
            if (lvIndoorList.Items.Count > 0)
            {
                foreach (ListViewItem sItem in lvIndoorList.Items)
                {
                    RoomIndoor roomindoor = (RoomIndoor)sItem.Tag;
                    if (roomindoor != null)
                    {
                        if (roomindoor.IndoorNO == ri.IndoorNO && roomindoor.IsExchanger == ri.IsExchanger)
                        {
                            isdefaultAdd = false;
                            break;
                        }
                    }
                }
            }
            if (isdefaultAdd) 
                lvIndoorList.Items.Insert(lvIndoorList.Items.Count, lvItem);

        }

        /// <summary>
        /// 重置节点字体颜色
        /// </summary>
        /// <param name="nodes"></param>
        private void resetTreeNodeColor(TreeNode node)
        {
            node.ForeColor = colorBlack;
        }

        ///// <summary>
        ///// 校验拖拽的ListViewItem中是否包含严格一对一的新风机记录
        ///// </summary>
        ///// <param name="list"></param>
        ///// <returns>true:有；false：无</returns>
        //private bool hasUniqueOutdoor(Indoor inItem)
        //{
        //    string outModel = bll.GetUniqueOutdoorModel(thisProject.RegionVRF, thisProject.ProductType, inItem);
        //    if (!string.IsNullOrEmpty(outModel))
        //        return true;
        //    return false;
        //}

        double tot_indcap_c;
        double tot_indcap_h;
        double tot_act_indcap_c;
        double tot_act_indcap_h;
        double tot_FAcap;

        double tot_indcap_c_rat;
        double tot_indcap_h_rat;

        #endregion

        /// 计算指定系统中的管长修正系数
        /// <summary>
        /// 计算当前系统的管长修正系数
        /// </summary>
        /// <returns></returns>
        private double GetPipeLengthRevisedFactor(SystemVRF curSystemItem, string condition)
        {
            PipingBLL pipBll = new PipingBLL(thisProject);
            //bool isAbove = !(curSystemItem.PipingPositionType == PipingPositionType.Lower);
            return pipBll.GetPipeLengthFactor(curSystemItem, condition);
        }

        // 以下内容更新与20141231，将室内机组合改为四种模式后自动选择室外机的过程
        List<string> ERRList = null;
        List<string> MSGList = null;   //日志列表   add on 20170823 by LingjiaQiu
        ///// 自动选择匹配的室外机
        ///// 1.混连模式
        ///// 1-1.必须同时包含室内机与新风机；ok
        ///// 1-2.新风机冷量与室外机的配置率不大于30%
        ///// 1-3.内机总冷量配置率为80%～105%；201509clh-需求更改，改为80%-100%
        ///// 1-4.当配置率超出100%时，要有提示；
        ///// 
        ///// 2.全室内机模式
        ///// 2-1.不允许同时包含室内机和新风机；ok
        ///// 2-2.内机总冷量配置率为50%～130%；
        ///// 
        ///// 3.全新风机模式
        ///// 3-1.0510～2100不支持一对一模式；ok
        ///// 3-1需求更新：0510～2100支持一对一模式，同时也支持混连模式（(add on 20151126 clh)）
        ///// 3-2.多台新风机时不能包含一对一的新风机；ok
        ///// 3-3.与室外机配置率为80～105%；201509clh-需求更改，改为80%-100%
        ///// 3-4.当配置率超出100%时，要有提示；
        ///// 3-5.若连续选了两台0510新风机，则屏蔽此时可选的室外机，提示无合适的室外机！（特殊需求）
        ///// 
        ///// 4.201509clh-需求更新
        ///// 4-1.ME T3系列mini与super数据合并；YDCH224、YDCH280/YDCD018、YDCD022/YDCP018、YDCP022/YDHW022，当Type为Horizontal时不可选
        ///// <summary>
        ///// 自动选择匹配的室外机
        ///// </summary>
        //public SelectOutdoorResult DoSelectOutdoor(out List<string> ERRList)
        //{
        //    ERRList = new List<string>();
        //    ERRList.Clear();    // 每次选型前清空错误记录表
        //    curSystemItem.OutdoorItem = null;
        //    string OutdoorModelFull = "";

        //    PipingBLL pipBll = new PipingBLL();

        //    // 若所选室内机数量为0，则终止选型
        //    if (listRISelected == null || listRISelected.Count == 0)
        //    {
        //        return SelectOutdoorResult.Null;
        //    }

        //    // 获取室外机标准表（初次加载或者更改室外机类型时）
        //    if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
        //    {
        //        dtOutdoorStd = bll.GetOutdoorListStd();
        //        if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
        //        {
        //            // ERROR:室外机标准表无数据记录！
        //            ERRList.Add(Msg.DB_NODATA);
        //            return SelectOutdoorResult.Null;
        //        }
        //    }

        //    // 计算此时的室内机容量和（新风机与室内机分开汇总）
        //    double tot_indcap_c = 0;
        //    double tot_indcap_h = 0;
        //    double tot_FAcap = 0;
        //    double tot_indstdcap_c = 0;
        //    double ratioFA = 0;
        //    double tot_indcap_cOnly = 0;
        //    foreach (RoomIndoor ri in listRISelected)
        //    {
        //        if (ri.IndoorItem.Flag == IndoorType.FreshAir)
        //        {
        //            if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
        //            {
        //                // JCHVRF:混连模式下，1680与2100新风机取另一条记录
        //                //20160821 新增productType 参数 by Yunxiao Lin
        //                //Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, ri.IndoorItem.ProductType, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType);
        //                Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType);
        //                if (inItem != null)
        //                {
        //                    inItem.Series = ri.IndoorItem.Series;
        //                    ri.IndoorItem = inItem;
        //                }
        //            }
        //            else if (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100"))
        //            {
        //                Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, false, ri.IndoorItem.ProductType);
        //                if (inItem != null)
        //                {
        //                    inItem.Series = ri.IndoorItem.Series;
        //                    ri.IndoorItem = inItem;
        //                }
        //            }
        //            tot_FAcap += ri.IndoorItem.CoolingCapacity;
        //        }
        //        tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
        //        tot_indcap_h += ri.HeatingCapacity;
        //        tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
        //    }
        //    tot_indcap_cOnly = tot_indcap_c - tot_FAcap;

        //    bool isComposite = this.jccbxCompositeMode.Checked;
        //    #region //确定当前内机组合模式
        //    // 1.混连模式
        //    if (isComposite)
        //    {
        //        curSystemItem.SysType = SystemType.CompositeMode;
        //        // 1-1.必须同时包含室内机与新风机；
        //        if (tot_indcap_cOnly == 0 || tot_FAcap == 0)
        //        {
        //            ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
        //            return SelectOutdoorResult.Null;
        //        }

        //    }
        //    //2.单一模式
        //    else
        //    {
        //        // 2-1.不允许同时包含室内机和新风机；
        //        if (tot_indcap_cOnly > 0 && tot_FAcap > 0)
        //        {
        //            ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
        //            return SelectOutdoorResult.Null;
        //        }

        //        // 2.全室内机模式
        //        if (tot_indcap_cOnly > 0)
        //        {
        //            curSystemItem.SysType = SystemType.OnlyIndoor;
        //        }
        //        // 3.全新风机模式
        //        else if (tot_FAcap > 0)
        //        {
        //            curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

        //            if (listRISelected.Count == 1)
        //            {
        //                Indoor inItem = listRISelected[0].IndoorItem;

        //                #region 一对一新风机
        //                curSystemItem.SysType = SystemType.OnlyFreshAir;

        //                // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
        //                // 20160821 新增productType参数 by Yunxiao Lin
        //                //inItem = (new IndoorBLL(thisProject.SubRegionCode,thisProject.ProductType, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType);
        //                inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType);
        //                string UniqueOutdoorName = inItem.UniqueOutdoorName;

        //                //curSystemItem.OutdoorItem = bll.GetOutdoorItem(UniqueOutdoorName);
        //                //curSystemItem.OutdoorItem = bll.GetOutdoorItem(UniqueOutdoorName, _productType);
        //                //这里取一对一室外机对象时需要判断品牌，调用不同的方法 20161023 by Yunxiao Lin
        //                if (thisProject.BrandCode == "Y")
        //                    //curSystemItem.OutdoorItem = bll.GetOutdoorItem(UniqueOutdoorName, _productType);
        //                    curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, _series);
        //                else
        //                    //curSystemItem.OutdoorItem = bll.GetHitachiItem(UniqueOutdoorName, _productType);
        //                    curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, _series);
        //                if (curSystemItem.OutdoorItem == null)
        //                {
        //                    // ERROR:数据库中的一对一室外机ModelName写错
        //                    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
        //                    return SelectOutdoorResult.Null;
        //                }
        //                else
        //                {
        //                    curSystemItem.MaxRatio = 1;
        //                    curSystemItem.Ratio = inItem.CoolingCapacity / curSystemItem.OutdoorItem.CoolingCapacity;
        //                    // FreshAir时不需要估算容量,直接绑定室外机的标准值
        //                    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
        //                    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
        //                    curSystemItem.MaxPipeLength = curSystemItem.OutdoorItem.MaxPipeLength;
        //                    curSystemItem.MaxEqPipeLength = curSystemItem.OutdoorItem.MaxEqPipeLength;
        //                    curSystemItem.MaxOutdoorAboveHeight = curSystemItem.OutdoorItem.MaxOutdoorAboveHeight;
        //                    curSystemItem.MaxOutdoorBelowHeight = curSystemItem.OutdoorItem.MaxOutdoorBelowHeight;
        //                    curSystemItem.MaxDiffIndoorHeight = curSystemItem.OutdoorItem.MaxDiffIndoorHeight;
        //                    curSystemItem.MaxIndoorLength = curSystemItem.OutdoorItem.MaxIndoorLength;
        //                    curSystemItem.MaxPipeLengthwithFA = curSystemItem.OutdoorItem.MaxPipeLengthwithFA;
        //                    curSystemItem.MaxDiffIndoorLength = curSystemItem.OutdoorItem.MaxDiffIndoorLength;
        //                    //增加系统液管总长上限变量，用于兼容IVX选型 20170704 by Yunxiao lin
        //                    curSystemItem.MaxTotalPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //                    curSystemItem.MaxTotalPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //                    curSystemItem.MaxMKIndoorPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //                    curSystemItem.MaxMKIndoorPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //                    // 一对一新风机选型成功！
        //                    return SelectOutdoorResult.OK;
        //                }

        //                #endregion

        //            }
        //        }// 全新风机 END
        //    }// 模式确定 END
        //    #endregion

        //    SelectOutdoorResult returnType = SelectOutdoorResult.OK;

        //    #region // 遍历室外机标准表逐个筛选
        //    // update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
        //    double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
        //    double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
        //    // 遍历选型过程 START 
        //    //室外机选型改为使用Series 20161031 by Yunxiao Lin
        //    //DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "'" + " and TypeImage <> ''");
        //    DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "'" + " and Series='" + _series + "' and TypeImage <> '' and deleteFlag=1", "Model asc");
        //    foreach (DataRow r in rows)
        //    {
        //        // 检查最大连接数
        //        //int maxIU = Convert.ToInt32(r["MaxIU"].ToString());
        //        int maxIU = 0;
        //        int.TryParse(r["MaxIU"].ToString(), out maxIU);
        //        if (maxIU < listRISelected.Count)
        //            continue;

        //        // 检查容量配比率（此处仅校验上限值）
        //        double outstdcap_c = Convert.ToDouble(r["CoolCapacity"].ToString());
        //        curSystemItem.Ratio = Math.Round(tot_indstdcap_c / outstdcap_c, 3);
        //        ratioFA = Math.Round(tot_FAcap / outstdcap_c, 3);

        //        if (curSystemItem.SysType == SystemType.OnlyIndoor)
        //        {
        //            // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
        //            if (curSystemItem.Ratio < 0.5)
        //            {
        //                OutdoorModelFull = r["ModelFull"].ToString();
        //                ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
        //                break;
        //            }
        //            if (curSystemItem.Ratio > curSystemItem.MaxRatio)
        //                continue;
        //        }
        //        else
        //        {
        //            // 多新风机或者混连模式，则配比率校验规则有变
        //            if (curSystemItem.Ratio < 0.8)
        //            {
        //                OutdoorModelFull = r["ModelFull"].ToString();
        //                ERRList.Add(Msg.OUTD_RATIO_Composite);
        //                break;
        //            }
        //            if (curSystemItem.Ratio > 1) // 1.05 改为1，201509 clh
        //                continue;
        //            if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
        //                continue; // add on 20160307 clh 
        //        }

        //        // 3、比较估算容量与室内机容量和
        //        //Outdoor outItem = bll.GetOutdoorItem(r["ModelFull"].ToString());
        //        // 多ProductType功能需要新增productType参数 20160823 by Yunxiao Lin
        //        //Outdoor outItem = bll.GetOutdoorItem(r["ModelFull"].ToString(), _productType);
        //        Outdoor outItem = bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), _series);
        //        //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160615 by Yunxiao Lin
        //        if (!outItem.ProductType.Contains("Water Source"))
        //            //curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.DBCooling, inWB, false);
        //            curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.DBCooling, inWB, false, curSystemItem);
        //        else
        //            //curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.IWCooling, inWB, false);
        //            curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.IWCooling, inWB, false, curSystemItem);

        //        if (thisProject.IsCoolingModeEffective)
        //        {
        //            curSystemItem.PipingLengthFactor = (double)pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
        //            if (curSystemItem.PipingLengthFactor == 0)
        //            {
        //                double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
        //                double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
        //                JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n0") + ut_length, Math.Abs(diff).ToString("n0") + ut_length));
        //                return SelectOutdoorResult.Null;
        //            }
        //            curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
        //            if (curSystemItem.CoolingCapacity < tot_indcap_c * curSystemItem.DiversityFactor) // updated on 20140625 clh
        //                continue;
        //        }
        //        //  Hitachi的Fresh Air 不需要比较HeatCapacity值
        //        if (!outItem.ProductType.Contains(", CO") && thisProject.IsHeatingModeEffective)
        //        {
        //            //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
        //            if (!outItem.ProductType.Contains("Water Source"))
        //                //curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.WBHeating, inDB, true);
        //                curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.WBHeating, inDB, true, curSystemItem);
        //            else
        //                //curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.IWHeating, inDB, true);
        //                curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.IWHeating, inDB, true, curSystemItem);
        //            //水冷机不需要除霜计算
        //            if (!outItem.ProductType.Contains("Water Source"))
        //            {
        //                double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
        //                curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
        //            }
        //            if (curSystemItem.HeatingCapacity < tot_indcap_h * curSystemItem.DiversityFactor) // changed on 20130710
        //                continue;
        //        }

        //        //海拔修正 add on 20160517 by Yunxiao Lin
        //        //注意某些机型可能无此限制? Wait check
        //        //是否启用海拔修正是项目属性 20160819 by Yunxiao Lin
        //        //if (SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor)
        //        if (thisProject.EnableAltitudeCorrectionFactor)
        //        {
        //            //获取海拔修正系数
        //            double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
        //            curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;
        //            if (curSystemItem.CoolingCapacity < tot_indcap_c * curSystemItem.DiversityFactor)
        //                continue;
        //            //if (thisProject.IsHeatingModeEffective)
        //            // 由于一个Project中可能存在多个不同的ProductType，因此单凭IsHeatingModeEffective无法确定一个System是否需要制热。
        //            // 如果productType中包含", CO"，那么这个System是单冷系统，就算IsHeatingModeEffective=true，也不需要制热功能。 20160826 by Yunxiao Lin
        //            //if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
        //            if (thisProject.IsHeatingModeEffective && !_productType.Contains("VRF CO,"))
        //            {
        //                curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
        //                if (curSystemItem.HeatingCapacity < tot_indcap_h * curSystemItem.DiversityFactor)
        //                    continue;
        //            }
        //        }

        //        ////除霜修正,只有制热容量需要此修正 add on 20160525 by Yunxiao Lin 
        //        //if (!outItem.ProductType.Contains(", CO") && thisProject.IsHeatingModeEffective) //注意只有具备制热功能的室外机需要除霜修正
        //        //{
        //        //    double dcf = getDefrostCorrectionFactor(curSystemItem.DBCooling);
        //        //    curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * dcf;
        //        //    if (curSystemItem.HeatingCapacity < tot_indcap_h * curSystemItem.DiversityFactor)
        //        //        continue;
        //        //}


        //        // 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
        //        if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
        //        {
        //            if (curSystemItem.Ratio > 1)
        //            {
        //                if (!(JCMsg.ShowConfirmYesNoCancel(Msg.OUTD_RATIO_Composite2) == DialogResult.Yes))
        //                {
        //                    curSystemItem.AllowExceedRatio = false;
        //                    continue;
        //                }
        //                else
        //                    curSystemItem.AllowExceedRatio = true;
        //            }
        //        }

        //        OutdoorModelFull = r["ModelFull"].ToString();
        //        break; // 找到合适的室外机即跳出循环
        //    }
        //    // 遍历自动选型 END
        //    #endregion


        //    if (curSystemItem.SysType == SystemType.OnlyIndoor)
        //    {
        //        // 全室内机
        //        // 2-2.内机总冷量配置率为50%～130%；
        //        if (curSystemItem.Ratio > curSystemItem.MaxRatio)
        //        {
        //            OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
        //            ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
        //            returnType = SelectOutdoorResult.NotMatch;
        //        }
        //    }
        //    else
        //    {
        //        // 多新风机或者混连模式，则配比率校验规则有变

        //        // 1-2.新风机冷量与室外机的配置率不大于30%
        //        if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
        //        {
        //            if (string.IsNullOrEmpty(OutdoorModelFull))
        //                OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
        //            ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
        //            returnType = SelectOutdoorResult.NotMatch;
        //        }

        //        // 1-3.内机总冷量配置率为80%～105%；
        //        // TODO: add on 20150902 clh,此处更改为80%～100%
        //        if (curSystemItem.Ratio > 1) // 1.05 改为1，201509 clh
        //        {
        //            OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
        //            ERRList.Add(Msg.OUTD_RATIO_Composite);
        //            returnType = SelectOutdoorResult.NotMatch;
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(OutdoorModelFull))
        //    {
        //        //curSystemItem.OutdoorItem = bll.GetOutdoorItem(OutdoorModelFull);
        //        //多ProductType功能需要新增productType参数 20160823 by Yunxiao Lin
        //        //curSystemItem.OutdoorItem = bll.GetOutdoorItem(OutdoorModelFull, _productType);
        //        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, _series);
        //        // updated by clh
        //        if (curSystemItem.OutdoorItem != null)
        //        {
        //            curSystemItem.MaxPipeLength = curSystemItem.OutdoorItem.MaxPipeLength;
        //            curSystemItem.MaxEqPipeLength = curSystemItem.OutdoorItem.MaxEqPipeLength;
        //            curSystemItem.MaxOutdoorAboveHeight = curSystemItem.OutdoorItem.MaxOutdoorAboveHeight;
        //            curSystemItem.MaxOutdoorBelowHeight = curSystemItem.OutdoorItem.MaxOutdoorBelowHeight;
        //            curSystemItem.MaxDiffIndoorHeight = curSystemItem.OutdoorItem.MaxDiffIndoorHeight;
        //            curSystemItem.MaxIndoorLength = curSystemItem.OutdoorItem.MaxIndoorLength;
        //            curSystemItem.MaxPipeLengthwithFA = curSystemItem.OutdoorItem.MaxPipeLengthwithFA;
        //            curSystemItem.MaxDiffIndoorLength = curSystemItem.OutdoorItem.MaxDiffIndoorLength;
        //            //增加系统液管总长上限变量，用于兼容IVX选型 20170704 by Yunxiao lin
        //            curSystemItem.MaxTotalPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //            curSystemItem.MaxTotalPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //            curSystemItem.MaxMKIndoorPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //            curSystemItem.MaxMKIndoorPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //            string series = curSystemItem.OutdoorItem.Series;
        //            if (series.Contains("IVX"))
        //            {
        //                //IVX系统根据环境温度的不同，管长上限会变化 20170704 by Yunxiao lin
        //                if (series.Contains("H(R/Y)NM1Q")) //目前仅有HAPQ的H(R/Y)NM1Q需要改变管长上限 20170704 by Yunxiao Lin
        //                {
        //                    if (curSystemItem.DBCooling <= -10)
        //                    {
        //                        switch (curSystemItem.OutdoorItem.Model_Hitachi)
        //                        {
        //                            case "RAS-3HRNM1Q":
        //                                curSystemItem.MaxPipeLength = 30;
        //                                curSystemItem.MaxTotalPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 40;
        //                                curSystemItem.MaxIndoorLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength = 5;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 5;
        //                                break;
        //                            case "RAS-4HRNM1Q":
        //                            case "RAS-5HRNM1Q":
        //                            case "RAS-5HYNM1Q":
        //                                curSystemItem.MaxPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 40;
        //                                curSystemItem.MaxIndoorLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength = 5;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 5;
        //                                break;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        switch (curSystemItem.OutdoorItem.Model_Hitachi)
        //                        {
        //                            case "RAS-3HRNM1Q":
        //                                curSystemItem.MaxPipeLength = 30;
        //                                curSystemItem.MaxTotalPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 40;
        //                                curSystemItem.MaxIndoorLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength = 5;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 5;
        //                                break;
        //                            case "RAS-4HRNM1Q":
        //                            case "RAS-5HRNM1Q":
        //                            case "RAS-5HYNM1Q":
        //                                curSystemItem.MaxPipeLength = 50;
        //                                curSystemItem.MaxTotalPipeLength = 60;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 60;
        //                                curSystemItem.MaxIndoorLength = 20;
        //                                curSystemItem.MaxMKIndoorPipeLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 10;
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //            return returnType;
        //        }

        //    }
        //    //if(!thisProject.ProductType.Contains("Water Source"))
        //    //if (_productType.Contains("Water Source"))
        //    if (_series.Contains("Water Source"))
        //        ERRList.Add(Msg.OUTD_NOTMATCH);
        //    else
        //        ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
        //    return SelectOutdoorResult.Null;
        //}

        //#region 计算除霜修正系数 add on 20160525 by Yunxiao Lin
        ///// <summary>
        ///// 计算除霜修正系数
        ///// </summary>
        ///// <param name="tmpDB">室外干球温度(摄氏度)</param>
        ///// <returns></returns>
        //public double getDefrostCorrectionFactor(double tmpDB)
        //{
        //    double dcf = 1.0d;
        //    if (tmpDB >= 6.1d)
        //        dcf = 1.0d;
        //    else if (tmpDB >= 5d)
        //        dcf = 0.9d;
        //    else if (tmpDB >= 3.9d)
        //        dcf = 0.88d;
        //    else if (tmpDB >= 1.7d)
        //        dcf = 0.86d;
        //    else if (tmpDB >= -0.6d)
        //        dcf = 0.85d;
        //    else if (tmpDB >= -2.8d)
        //        dcf = 0.88d;
        //    else if (tmpDB >= -5d)
        //        dcf = 0.93d;
        //    else if (tmpDB >= -7.2d)
        //        dcf = 0.95d;
        //    return dcf;
        //}
        //#endregion

        //#region 计算海拔修正系数 add on 20160517 by Yunxiao Lin
        ///// <summary>
        ///// 计算室外机海拔修正系数
        ///// </summary>
        ///// <param name="Altitude">海拔</param>
        ///// <returns></returns>
        //public double getAltitudeCorrectionFactor(double Altitude)
        //{
        //    double acf = 1.0d;  //0ft
        //    double a = Altitude;
        //    if (a >= 305d)
        //    {
        //        if (a >= 305d && a < 610d) //1000ft
        //            acf = 0.97d;
        //        else if (a < 914d)  //2000ft
        //            acf = 0.93d;
        //        else if (a < 1219d)  //3000ft
        //            acf = 0.9d;
        //        else if (a < 1524d)  //4000ft
        //            acf = 0.87d;
        //        else if (a < 1829d)  //5000ft
        //            acf = 0.83d;
        //        else if (a < 2133d)  //6000ft
        //            acf = 0.8d;
        //        else if (a < 2438d)  //7000ft
        //            acf = 0.77d;
        //        else if (a < 2743d)  //8000ft
        //            acf = 0.75d;
        //        else if (a < 3048d)  //9000ft
        //            acf = 0.72d;
        //        else
        //            acf = 0.69d;   //10000ft    
        //    }
        //    return acf;
        //}
        //#endregion

        /// 显示验证信息
        /// <summary>
        /// 显示验证信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="lbl"></param>
        public void ShowWarningInfo(ERRType errType)
        {
            string err = "";
            toolStripStatusLabel1.Text = "";
            if (ERRList != null && ERRList.Count > 0)
            {
                if (errType == ERRType.StatusBar)
                {
                    foreach (string s in ERRList)
                        err += s + "  ";
                    toolStripStatusLabel1.Text = err;
                    toolStripStatusLabel1.ToolTipText = err;
                    List<string> msgbox = new List<string>();                  
                    //messageList显示
                    if (MSGList != null && MSGList.Count > 0)
                    {
                        foreach (string msg in MSGList)
                        {
                            if (!msgbox.Contains(msg))
                                msgbox.Add(msg);
                        }
                        msgbox.Add(err);
                        string title = "Message List";
                        frmHelpInfo f = new frmHelpInfo(msgbox, title);
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.Show();      
                    }

                }
                else if (errType == ERRType.PopupWin)
                {
                    foreach (string s in ERRList)
                        err += s + "\n";
                    JCMsg.ShowWarningOK(err);
                }
            }
        }

        private void jctxtIWC_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            RematchIndoorForWaterSource();
        }

        private void jctxtIWH_Leave(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox);
            RematchIndoorForWaterSource();
        }

        private void jccmbProductType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string series = jccmbProductType.SelectedValue.ToString();
            if (series == _series.ToString()) //20170401 by Yunxiao Lin
                return;
            if (series.Contains("FSXNSE"))
                this.jctxtDBC.JCMaxValue = float.Parse(Unit.ConvertToControl(48, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//48
            else
                this.jctxtDBC.JCMaxValue = float.Parse(Unit.ConvertToControl(52, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//52
            JCValidateSingle(jctxtDBC);
            

            List<RoomIndoor> unmatchedIndoorList = new List<RoomIndoor>(); //筛选出与当前系列不匹配的室内机列表  add by Shen Junjie 2018/3/19
            unmatchedIndoorList.AddRange(listRISelected);

            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            DataTable typeDt = productTypeBll.GetIduTypeBySeries(Registr.Registration.SelectedBrand.Code,
                    Registr.Registration.SelectedSubRegion.Code, series);
            //筛选出与当前系列不匹配的室内机列表  add by Shen Junjie 2018/3/19
            for (int i = 0; i < unmatchedIndoorList.Count; i++)
            {
                RoomIndoor ri = unmatchedIndoorList[i];
                foreach (DataRow dr in typeDt.Rows)
                {
                    //从未分配给室外机的室内机列表中过滤出指定productType的室内机
                    if (ri.IndoorItem.Type == dr["IDU_UnitType"].ToString())
                    {
                        //关联表中IDU_Model_Hitachi 为白名单，过滤可添加的室内机分配列表
                        string modelHitachi = dr["IDU_Model_Hitachi"].ToString();
                        if (string.IsNullOrEmpty(modelHitachi) || modelHitachi.Contains(";" + ri.IndoorItem.Model_Hitachi + ";"))
                        {
                            unmatchedIndoorList.Remove(ri);
                            i--;
                            break;
                        }
                    }
                }
            }

            //_productType = jccmbProductType.SelectedValue.ToString();
            //_series = jccmbProductType.SelectedValue.ToString();
            //变更ProductType会清空已选的选单，所以要弹出对话框 20160826 by Yunxiao Lin
            if (unmatchedIndoorList.Count > 0)
            {
                string[] unmatchedIndoorNames = unmatchedIndoorList.ConvertAll(p =>
                {
                    return p.IndoorFullName;
                }).ToArray();
                if (JCMsg.ShowConfirmOKCancel(Msg.CONFIRM_CHANGE_SERIES_REMOVE_IDU(series, unmatchedIndoorNames)) == DialogResult.Cancel)
                {
                    //jccmbProductType.SelectedValue = _productType;
                    jccmbProductType.SelectedValue = _series;
                    return;
                }
                //从系统中移出不可用的室内机
                if (!DetachIndoors(unmatchedIndoorList))
                {
                    jccmbProductType.SelectedValue = _series;
                    return;
                }
            }

            //变更ProductType选项会引起unit Type刷新 20160822 by Yunxiao Lin
            //_productType = jccmbProductType.SelectedValue.ToString();
            _series = series;
            BindMaxRatio(); //重新绑定MaxRation选项值 20170331 by Yunxiao Lin
            BindMaxDiffHeight();
            //BindTreeViewOutdoor();
            //tvOutdoor.Nodes.Clear();
            //tvOutdoor.Nodes.Add(curSystemItem.Name);
            //listRISelected.Clear();
            BindOutdoorTypeList();
            BindPowerList();
            if (jccmbPower.SelectedValue != null)
                curSystemItem.Power = jccmbPower.SelectedValue.ToString().Trim();
            BindOutdoorList();
            //BindIndoorAvailable(_productType);
            BindIndoorAvailable(_series);

            ReselectOutdoor();
            curSystemItem.IsUpdated = true;


        }

        private void jclblOutRatCapH_Click(object sender, EventArgs e)
        {

        }

        // 20141231 end


        #endregion


        //手动选型
        private void jccbxManual_Click(object sender, EventArgs e)
        {
            if (jccbxManual.Checked)
            {
                jccbxAuto.Checked = false;
                jccmbOutdoor.Enabled = true;
                curSystemItem.IsAuto = false;
            }
            else
            {
                jccbxAuto.Checked = true;
                jccmbOutdoor.Enabled = false;
                curSystemItem.IsAuto = true;
                ReselectOutdoor();
            }
        }
        //自动选型
        private void jccbxAuto_Click(object sender, EventArgs e)
        {
            if (jccbxAuto.Checked)
            {
                jccbxManual.Checked = false;
                jccmbOutdoor.Enabled = false;
                curSystemItem.IsAuto = true;
                ReselectOutdoor();
            }
            else
            {
                jccbxManual.Checked = true;
                jccmbOutdoor.Enabled = true;
                curSystemItem.IsAuto = false;
            }
        }

        /// <summary>
        /// 重新选择室外机
        /// </summary>
        /// <returns></returns>
        private void ReselectOutdoor()
        {
            SelectOutdoorResult result;
            if (curSystemItem.IsAuto)
            {
                result = Global.DoSelectUniversalODUFirst(curSystemItem, listRISelected, thisProject, _series, out ERRList, out MSGList);
            }
            else
            {
                MSGList = new List<string>(); // 手动选型模式下，需要清除MSGList on 20181221 by xyj
                string model = Convert.ToString(jccmbOutdoor.SelectedValue);
                //string productType = jccmbProductType.Text.Trim();
                OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode, Registr.Registration.SelectedSubRegion.ParentRegionCode);
                //curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, productType);
                curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, _series);
                result = Global.DoSelectUniversalOduManual(curSystemItem, listRISelected, thisProject, _series, out ERRList);
            }

            BindToControl(result);
            ShowWarningInfo(ERRType.StatusBar);
        }

        /// <summary>
        /// 绑定室外机下拉框
        /// </summary>
        private void BindOutdoorList()
        {
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode, Registr.Registration.SelectedSubRegion.ParentRegionCode);
            DataTable dt = bll.GetOutdoorListStd();
            DataRow[] rows;
            // jccmbPower.SelectedValue 有可能是null， 需要判断后才能使用
            string powerValue = "";
            if (jccmbPower.SelectedValue != null)
                powerValue = jccmbPower.SelectedValue.ToString().Trim();
            //过滤“JVOL060VVEM0AQ”机型，此机型无法单独使用
            if ("EU_E".Equals(thisProject.SubRegionCode) && "H".Equals(thisProject.BrandCode))
                //rows = dt.Select("SubString(ModelFull,11,1)='" + powerValue + "' and UnitType='" + jccmbType.Text.Trim() + "' and Series='" + jccmbProductType.Text.Trim() + "'" + " and TypeImage <> '' and ModelFull <> 'JVOL060VVEM0AQ'", "Model asc");
                rows = dt.Select("SubString(ModelFull,11,1)='" + powerValue + "' and UnitType='" + jccmbType.SelectedValue.ToString() + "' and Series='" + _series + "'" + " and TypeImage <> '' and ModelFull <> 'JVOL060VVEM0AQ'", "Model asc");
            else
                //rows = dt.Select("SubString(ModelFull,11,1)='" + powerValue + "' and UnitType='" + jccmbType.Text.Trim() + "' and Series='" + jccmbProductType.Text.Trim() + "'" + " and TypeImage <> ''", "Model asc");
                rows = dt.Select("SubString(ModelFull,11,1)='" + powerValue + "' and UnitType='" + jccmbType.SelectedValue.ToString() + "' and Series='" + _series + "'" + " and TypeImage <> ''", "Model asc");

            jccmbOutdoor.DisplayMember = "AuxModelName";
            jccmbOutdoor.ValueMember = "ModelFull";
            jccmbOutdoor.DataSource = rows.Length > 0 ? rows.CopyToDataTable() : null; //修复没有数据时候的异常 20170628 by Shen Junjie
        }

        private void jccmbOutdoor_SelectionChangeCommitted(object sender, EventArgs e)
        { 
            if (!curSystemItem.IsAuto)
            {
                ReselectOutdoor(); 
            } 
        }

        private void BindPowerList()
        {
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode, Registr.Registration.SelectedSubRegion.ParentRegionCode);
            //DataTable dt = bll.GetOutdoorPowerListBySeriesAndType(jccmbProductType.Text.Trim(), jccmbType.Text.Trim());
            if (jccmbType.SelectedValue == null)
                return;
            string selectedPower = jccmbType.SelectedValue.ToString();
            DataTable dt = bll.GetOutdoorPowerListBySeriesAndType(_series, selectedPower);
            jccmbPower.DisplayMember = "name";
            jccmbPower.ValueMember = "value";
            jccmbPower.DataSource = dt;
        }

        private void jccmbPower_SelectionChangeCommitted(object sender, EventArgs e)
        {
            curSystemItem.Power = jccmbPower.SelectedValue.ToString().Trim();
            BindOutdoorList();//自动绑定室外机列表 2016/12/23 by axj
            ReselectOutdoor();
            switchInletWater();
        }

        private void jccmbInletWaterFlowRate_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //水流速参数只支持水机
            //if (curSystemItem.OutdoorItem == null || !curSystemItem.OutdoorItem.Series.Contains("Water Source"))
            //    return;

            FlowRateLevels value;
            switch (jccmbInletWaterFlowRateLevel.SelectedIndex)
            {
                case 0:
                    value = FlowRateLevels.High;
                    break;
                case 1:
                    value = FlowRateLevels.Medium;
                    break;
                default:
                    value = FlowRateLevels.Low;
                    break;
            }
            if (curSystemItem.FlowRateLevel.Equals(value))
                return;

            curSystemItem.FlowRateLevel = value;
            ReselectOutdoor();
        }
        
        /// <summary>
        /// 绑定MaxRatio列表值 20170331 by Yunxiao Lin
        /// </summary>
        private void BindMaxRatio()
        {
            //绑定MaxRatio列表值
            this.jccmbMaxRatio.Items.Clear();
            double max = 1.3;
            double min = 0.5;
            double defaultMaxRatio = 1;

            if (_series.Contains("FSNP") || _series.Contains("FSXNP")
                || _series.Contains("FSNS7B") || _series.Contains("FSNS5B")
                 || _series.Contains("FSNC7B") || _series.Contains("FSNC5B") 
                ) //FSXNPE 上线也可以达到150% on 20180419 by xyj
            {
                //巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
                //FSNP FSNP6B FSNP5B 50%~150%
                max = 1.5;
            }
            else if (thisProject.SubRegionCode == "TW" && _series.StartsWith("IVX,"))
            { 
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/6
                min = 1;
            }
            if (curSystemItem.OutdoorItem != null)
            {
                //RAS-56-72FSXNPE - 50-130% on 20180507 by xyj
                if (_series.Contains("FSXNPE") && curSystemItem.OutdoorItem.CoolingCapacity > 150)
                {
                    max = 1.3;
                }
                //RAS-5-54FSXNPE - 50-150% on 20180507 by xyj
                else if (_series.Contains("FSXNPE") && curSystemItem.OutdoorItem.CoolingCapacity <= 150)
                {
                    max = 1.5;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyFreshAir)
                return;

            if (curSystemItem.MaxRatio > max)
            {
                curSystemItem.MaxRatio = max;
            }
            defaultMaxRatio = curSystemItem.MaxRatio;

            //其他系列 50%~130%
            for (int i = (int)(max * 100); i >= min * 100; i -= 10)
            {
                this.jccmbMaxRatio.Items.Add(i.ToString());
            }
            this.jccmbMaxRatio.SelectedIndex = (int)(Math.Round(max - defaultMaxRatio, 2) * 10); //选中默认值
        }

        private void BindMaxDiffHeight()
        {
            //if (this.rbtnHigher.Checked)
            //{
            //    if (_series.Contains("FSNS") || _series.Contains("FSNP") || _series.Contains("FSXNS") || _series.Contains("FSXNP") || _series.Contains("JTOH-BS1") || _series.Contains("JTOR-BS1"))
            //    {
            //        this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(110, UnitType.LENGTH_M, ut_length).ToString("n0"));
            //    }
            //    else
            //        this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(50, UnitType.LENGTH_M, ut_length).ToString("n0"));
            //}
            //else
            //{
            //    this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(40, UnitType.LENGTH_M, ut_length).ToString("n0"));
            //}
        }

        private void CheckMaxDiffHeight()
        {
            //if (this.rbtnHigher.Checked)
            //{
            //    if (_series.Contains("FSNS") || _series.Contains("FSNP") || _series.Contains("FSXNS") || _series.Contains("FSXNP") || _series.Contains("JTOH-BS1") || _series.Contains("JTOR-BS1"))
            //    {
            //        //如果特殊机型高度超过50M，提示消息
            //        float value = 0;
            //        float.TryParse(jctxtHighDifference.Text, out value);
            //        if (value > 50)
            //        {
            //            this.toolStripStatusLabel1.Text = Msg.GetResourceString("PIPING_POSITION_ODU_MSG");
            //            toolStripStatusLabel1.ForeColor = System.Drawing.Color.Chocolate;
            //        }
            //        else
            //        {
            //            this.toolStripStatusLabel1.Text = "";
            //            toolStripStatusLabel1.ForeColor = System.Drawing.Color.Red;
            //        }
            //    }
            //}
        }

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
                    if (node.Tag is SystemVRF)
                    {
                        pnlCondition_Cooling_indoor.Visible = false;
                        pnlCondition_Heating_indoor.Visible = false;
                    }
                    break;
                case 1:
                    if (node.Tag is RoomIndoor)
                    {
                        BindIndoorItemInfo(node.Tag as RoomIndoor , node);
                    }
                    break;
                case 2:
                    if (node.Tag is RoomIndoor)
                        BindIndoorItemInfo(node.Tag as RoomIndoor , node);
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
        /// <param name="node"></param>
        private void BindIndoorItemInfo(RoomIndoor riItem ,TreeNode node)
        {
            if (riItem == null)
                return;

            pnlCondition_Cooling_indoor.Visible = true;
            pnlCondition_Heating_indoor.Visible = true;
            //pnlCondition_Cooling.Visible = false;
            //pnlCondition_Heating.Visible = false;

            this.jclblInDBCoolValue.Text = Unit.ConvertToControl(riItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblInWBCoolValue.Text = Unit.ConvertToControl(riItem.WBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            this.jclblInDBHeatValue.Text = Unit.ConvertToControl(riItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") + ut_temperature;
            double rh = (new ProjectBLL(thisProject)).CalculateRH(riItem.DBCooling, riItem.WBCooling, thisProject.Altitude);
            this.jclblIndRHValue.Text = Math.Round((rh * 100)).ToString("n0") + "%";

            this.jclblAvailableCValue1.Text = Unit.ConvertToControl(riItem.ActualCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            //this.jclblAvailableCValue2.Text = Unit.ConvertToControl(riItem.SensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            //室外机选型结果中的显热应该取实际显热 20161114 by Yunxiao Lin
            this.jclblAvailableCValue2.Text = Unit.ConvertToControl(riItem.ActualSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            this.jclblAvailableHValue1.Text = Unit.ConvertToControl(riItem.ActualHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            //this.jclblRequiredCValue1.Text = riItem.IsAuto ?
            //    Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            //this.jclblRequiredCValue2.Text = riItem.IsAuto ?
            //    Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            //this.jclblRequiredHValue1.Text = riItem.IsAuto ?
            //    Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            this.jclblRequiredCValue1.Text = riItem.RqCoolingCapacity != 0d ?
                Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            this.jclblRequiredCValue2.Text = riItem.RqSensibleHeat != 0d ?
                Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";
            this.jclblRequiredHValue1.Text = riItem.RqHeatingCapacity != 0d ?
                Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-";

            if (CommonBLL.FullMeetRoomRequired(riItem, thisProject))
                this.toolStripStatusLabel1.Text = "";
            else
            {
                this.toolStripStatusLabel1.Text = Msg.GetResourceString("IND_CAPACITY_MSG");
                toolStripStatusLabel1.ForeColor = colorChocolate;
            }           

        }

        // 室内机node改变颜色   add on 20170522 by Lingjia Qiu
        /// <summary>
        /// 室内机node改变颜色
        /// </summary>
        /// <param name="riItem"></param>
        /// <param name="node"></param>
        private void ChangeIndoorNodesColor(RoomIndoor riItem, TreeNode node)
        {
            if (riItem == null)
                return;
            //需求容量大于实际容量，改变字体颜色
            if (!CommonBLL.FullMeetRoomRequired(riItem, thisProject))
                node.ForeColor = colorChocolate;
        }

        // 具有共享关系的室内机对象捆绑选型   add on 20170623 by Lingjia Qiu
        /// <summary>
        /// 具有共享关系的室内机对象捆绑选型
        /// </summary>
        /// <param name="riShareList"></param>
        /// <param name="type"> Enter:室内机对象至室外机系统节点；Back:室内机对象离开室外机系统节点</param>
        private bool selectionBySharingRelationShip(List<RoomIndoor> riShareList,string type)
        {
            if (riShareList.Count > 0)
            {
                string selectedIndoorName = "";
                string groupIndoorName = "";
                List<RoomIndoor> ridNoSelected = new List<RoomIndoor>();
                //判断是否存在IsMainIndoor
                bool IsMainIndoor = false; 
                int shareCount = 0;
                //如果存在Exchanger 去除当前室内机共享的Exchanger on 20171228 by xyj 
                //foreach (RoomIndoor rid in riShareList)
                //{
                 
                //    if (rid.IsMainIndoor && rid.IndoorItemGroup != null)
                //    {
                //        IsMainIndoor = true;
                //        rid.IndoorItemGroup = rid.IndoorItemGroup.FindAll(p => p.IsExchanger == false);

                //        //foreach (RoomIndoor ridGroup in rid.IndoorItemGroup)
                //        //{
                //        //    if (ridGroup.IsExchanger)
                //        //    {
                //        //        ri = ridGroup;
                //        //        isExists = true;
                //        //    }
                //        //    if (isExists && ri != null)
                //        //        rid.IndoorItemGroup.Remove(ri); 
                //        //}
                        
                //    } 
                //} 

                //初始选择项及群组对象信息及提示信息
                foreach (RoomIndoor rid in riShareList)
                {
                    shareCount = shareCount+1;
                    ridNoSelected.Add(rid);  //添加主Indoor on20171228 by xyj
                    selectedIndoorName += rid.IndoorName + ",";
                    foreach (RoomIndoor ridGroup in rid.IndoorItemGroup)
                    {
                        if (!ridGroup.IsExchanger)
                        {
                            shareCount++;
                            if (type.Equals("Enter") && !ridNoSelected.Contains(ridGroup))   //维护选择项信息 //添加主Indoor on20171228 by xyj 去除!listRISelected.Contains(ridGroup)
                            //if (!listRISelected.Contains(ridGroup) && type.Equals("Enter") && !ridNoSelected.Contains(ridGroup))   //维护选择项信息
                            {
                                ridNoSelected.Add(ridGroup);
                                if (!groupIndoorName.Contains(ridGroup.IndoorName))
                                    groupIndoorName += ridGroup.IndoorName + ",";
                            }
                            else if (type.Equals("Back") && !ridNoSelected.Contains(ridGroup))   //维护关系组信息
                            {
                                ridNoSelected.Add(ridGroup);
                                if (listRISelected.Contains(ridGroup) && !groupIndoorName.Contains(ridGroup.IndoorName))
                                    groupIndoorName += ridGroup.IndoorName + ",";
                            }
                        }
                    }
                }


                if (IsMainIndoor)
                { 
                        //判断当前选中的Indoor 是否都包含已共享的室内机
                        bool isShare = true;
                        foreach (RoomIndoor ind in ridNoSelected)
                        {
                            bool isShared = false;
                            foreach (RoomIndoor selInd in listRISelected)
                            {
                                if (ind.IndoorNO == selInd.IndoorNO)
                                {
                                    isShared = true;
                                    break;
                                }
                            }
                            if (!isShared)
                            {
                                isShare = false;
                                break;
                            }
                        }
                        if(!isShare)
                        {
                        //提示信息，选择否初始选项并退出
                            if (!string.IsNullOrEmpty(groupIndoorName) && !string.IsNullOrEmpty(selectedIndoorName))
                            {
                                groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
                                selectedIndoorName = selectedIndoorName.Substring(0, selectedIndoorName.Length - 1);
                                DialogResult res = JCMsg.ShowConfirmOKCancel(Msg.OUTD_RELATIONSHIP_ITEM_DRAP(selectedIndoorName, groupIndoorName));
                                if (res != DialogResult.OK)
                                {
                                    foreach (RoomIndoor rid in riShareList)  //回滚数据
                                    {
                                        if (type.Equals("Enter"))
                                        {
                                            if (rid.IndoorItemGroup != null)
                                            {
                                                foreach (RoomIndoor ris in rid.IndoorItemGroup)
                                                {
                                                    listRISelected.Remove(ris);
                                                }
                                            }
                                            listRISelected.Remove(rid);

                                        }
                                        else
                                        {
                                            listRISelected.Add(rid);
                                            if (lvIndoorList.Items.Count > 0)
                                            {
                                                foreach (ListViewItem sItem in lvIndoorList.Items)
                                                {
                                                    RoomIndoor ri = (RoomIndoor)sItem.Tag;
                                                    foreach (RoomIndoor ris in rid.IndoorItemGroup)
                                                    {
                                                        if (ri.IndoorNO == ris.IndoorNO)
                                                        {
                                                            sItem.Remove();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        return false;
                                    }
                                }
                            }
                        } 
                }

                //对拖拽全局对象List进行修改
                foreach (RoomIndoor ridItem in ridNoSelected)
                {
                    if (type.Equals("Enter"))
                    {
                        //去除重复的Indoor 
                        bool isAdd = true; 
                        foreach (RoomIndoor item in listRISelected)
                        {
                            if (ridItem.IndoorNO == item.IndoorNO && ridItem.IsExchanger == item.IsExchanger)
                            {
                                isAdd = false;
                                break;
                            }
                        }
                        if (isAdd)
                            listRISelected.Add(ridItem);
                    }
                    else
                    {
                        listRISelected.Remove(ridItem);
                        doAddItemToListView(ridItem);
                    } 
                }
            }
            return true;
        }

        /// <summary>
        /// 控制输入
        /// </summary>
        /// <param name="sender"></param>
        public void ControllerInput(object sender)
        {
            TextBox tx = sender as TextBox;
            if (!NumberUtil.IsNumber(tx.Text))
            {
                if (!tx.Text.Contains("."))
                {
                    tx.Text = "";
                }
            }
             
        }

        private void frmAddOutdoorUniversal_Shown(object sender, EventArgs e)
        {
            //修改缺数据的老系统，弹出提示
            if (curSystemItem != null && curSystemItem.Unmaintainable)
            {
                string msg = Msg.UNMAINTAINABLE_SYS_WARNING;
                toolStripStatusLabel1.Text = msg;
                toolStripStatusLabel1.ToolTipText = msg;
                JCMsg.ShowWarningOK(msg);
            }
        }

       

        private void jctxtEqPipeLength_Leave(object sender, EventArgs e)
        {
            if (JCValidateSingle(jctxtEqPipeLength)&& JCValidateSingle(jctxtFirstPipeLength))
            {
                BindPipeLength(); 
            }
        }

        private void jctxtFirstPipeLength_Leave(object sender, EventArgs e)
        {
            if (JCValidateSingle(jctxtFirstPipeLength)&& JCValidateSingle(jctxtEqPipeLength))
            {
                BindPipeLength(); 
            }
        }

        private void frmAddOutdoorUniversal_FormClosing(object sender, FormClosingEventArgs e)
        {
            //jclblPipingLength.Focus();
            //DialogResult = DialogResult.OK;
        }

        private void tapPageTrans1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.jctxtEqPipeLength.JCMinValue = 0;
            this.jctxtEqPipeLength.JCMaxValue = (float)Unit.ConvertToControl(curSystemItem.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);

            this.jctxtFirstPipeLength.JCMinValue = 0;
            this.jctxtFirstPipeLength.JCMaxValue = (float)Unit.ConvertToControl(curSystemItem.MaxIndoorLength, UnitType.LENGTH_M, ut_length);

            this.jctxtEqPipeLength.Text = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length).ToString("n1");
            this.jctxtFirstPipeLength.Text = Unit.ConvertToControl(curSystemItem.FirstPipeLength, UnitType.LENGTH_M, ut_length).ToString("n1");
        }


        //  同一房间室内机关联拖动   add on 20180714 by Vince
        /// <summary>
        /// 同一房间室内机关联拖动
        /// </summary>
        /// <param name="roomIdList"></param>
        /// <param name="movingIndoorList"></param>
        /// <param name="isRemoteRelation"></param>
        private List<RoomIndoor> dragIndWithSameRoom(List<string> roomIdList, List<RoomIndoor> movingIndoorList, bool isRemoteRelation)
        {           
            List<RoomIndoor> availableIndoorsInSameRoom = new List<RoomIndoor>(); //同一房间中的其它室内机
            foreach (RoomIndoor ind in listRIAvailable)
            {
                if (!roomIdList.Contains(ind.RoomID)) continue;
                if (listRISelected.Contains(ind)) continue;
                if (movingIndoorList.Contains(ind)) continue;

                availableIndoorsInSameRoom.Add(ind);
            }

            if (availableIndoorsInSameRoom.Count > 0)
            {
                DialogResult res = new DialogResult();
                if (!isRemoteRelation)
                    // "选中的室内机所在房间存在多个室内机将会被一起移动，确认是否继续操作？";
                    res = JCMsg.ShowConfirmOKCancel(Msg.OUTD_TOGETHER_MOVE_BYROOM);
                else
                    //选中的室内机与其他房间室内机存在共享控制器关系，是否将所有存在关系的房间室内机一起移动？
                    res = JCMsg.ShowConfirmOKCancel(Msg.OUTD_TOGETHER_MOVE_BYROOM_WITH_RELATIONSHIP);

                if (res != System.Windows.Forms.DialogResult.OK)
                {
                    return null;
                }
                //将同一个Room的其它ind添加到listRISelected
                //movingIndoorList.AddRange(availableIndoorsInSameRoom);
                foreach (RoomIndoor rid in availableIndoorsInSameRoom)
                {
                    movingIndoorList.Add(rid);
                }                
            }
            return availableIndoorsInSameRoom;
        }
    }
}
