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
using JCBase.Utility;
using JCHVRF.VRFMessage; 
using System.Collections;

namespace JCHVRF
{
    public partial class frmPipingLengthSetting : JCBase.UI.JCForm
    {
        string ut_length;
        SystemVRF sysItemSource = null;
        Project thisProject;
        List<string> ERRList;
        List<string> MSGList;
        SelectOutdoorResult result;
        double originFirstPipeLength = 0;
        double originPipeEquivalentLength = 0;
        PipingPositionType originPipingPositionType = PipingPositionType.Upper;
        double originHeightDiff = 0;

        List<RoomIndoor> listRISelected = new List<RoomIndoor>();
        List<SelectedIDUList> SelIDUList = new List<SelectedIDUList>(); //选中室内机的集合
        PipingBLL pbll;
        bool isVal = true;
        private class SelectedIDUList
        {
            public int IndoorNo { get; set; }   //IndoorNo
            public string IndoorName { get; set; } //IndoorName
            public string IndoorModel { get; set; }
            public MyNode IndoorTag { get; set; }
        }
        public frmPipingLengthSetting(SystemVRF curSystemItem,List<RoomIndoor> listRISelected,Project thisProject)
        {
            InitializeComponent();
            this.JCSetLanguage();

            this.listRISelected = listRISelected;
            this.thisProject = thisProject;
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            this.JCCallValidationManager = true;
            panelHighDifference.Visible = true;
            pbll= new PipingBLL(thisProject);
            if (curSystemItem != null)
            {
                //初始化数据 
                sysItemSource = curSystemItem; 
                initPipLength();

                CreateDataGridViewColumns();
                BindPositionType();
                // BindIndoorDiff(); 
                BindHighDifference();
            }
        }


        private void BindHighDifference()
        {
            dgvIndoorDiff.Rows.Clear();
            List<MyNode> nodes = new List<MyNode>(); 
            pbll.GetHeightDifferenceNodes(sysItemSource.MyPipingNodeOut, null, sysItemSource, nodes);
            int i = -1;
            foreach (MyNode node in nodes)
            {
                string modelName = "";
                if (node is MyNodeCH)
                {
                    MyNodeCH nodeCH = node as MyNodeCH;
                    string PositionType = PipingPositionType.Upper.ToString();
                    if (!string.IsNullOrEmpty(nodeCH.Model))
                    {
                        modelName = "CHBox [" + nodeCH.Model + "]";
                    }
                    else {
                        modelName = "CHBox";
                    }
                    double HeightDiff = nodeCH.HeightDiff;
                    if (HeightDiff < 0)
                    {
                        PositionType = PipingPositionType.Lower.ToString();
                        HeightDiff = -HeightDiff;
                    }
                    else if (HeightDiff == 0)
                    {
                        PositionType = PipingPositionType.SameLevel.ToString();
                    }
                    i =dgvIndoorDiff.Rows.Add(0,true,modelName,PositionType,Unit.ConvertToControl(HeightDiff, UnitType.LENGTH_M, ut_length).ToString("n1"));
                    dgvIndoorDiff.Rows[i].Cells[0].Tag = nodeCH;

                 }
                if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeCH = node as MyNodeMultiCH;
                    string PositionType = PipingPositionType.Upper.ToString();
                    if (!string.IsNullOrEmpty(nodeCH.Model))
                    {
                        modelName = "MultiCHBox [" + nodeCH.Model + "]";
                    }
                    else
                    {
                        modelName = "MultiCHBox";
                    }
                    double HeightDiff = nodeCH.HeightDiff;
                    if (HeightDiff < 0)
                    {
                        PositionType = PipingPositionType.Lower.ToString();
                        HeightDiff = -HeightDiff;
                    }
                    else if (HeightDiff == 0)
                    {
                        PositionType = PipingPositionType.SameLevel.ToString();
                    }
                    i = dgvIndoorDiff.Rows.Add(0,true,modelName,PositionType,Unit.ConvertToControl(HeightDiff, UnitType.LENGTH_M, ut_length).ToString("n1"));
                    dgvIndoorDiff.Rows[i].Cells[0].Tag = nodeCH;
                }
                if (node is MyNodeIn)
                {
                    MyNodeIn nodeIn = node as MyNodeIn;
                    string Room = "";
                    if (!string.IsNullOrEmpty((new ProjectBLL(thisProject)).GetFloorAndRoom(nodeIn.RoomIndooItem.RoomID)))
                    {
                        Room = (new ProjectBLL(thisProject)).GetFloorAndRoom(nodeIn.RoomIndooItem.RoomID) + " : ";
                    }
                    Room = Room + nodeIn.RoomIndooItem.IndoorName +" ["+ (thisProject.BrandCode == "Y" ? nodeIn.RoomIndooItem.IndoorItem.Model_York : nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi)+"]";                  
                    i = dgvIndoorDiff.Rows.Add(nodeIn.RoomIndooItem.IndoorNO,false, Room, nodeIn.RoomIndooItem.PositionType, Unit.ConvertToControl(nodeIn.RoomIndooItem.HeightDiff, UnitType.LENGTH_M, ut_length).ToString("n1"));   
                    dgvIndoorDiff.Rows[i].Cells[0].Tag = nodeIn;
                }
            }
            BindGridAlignment();
            RecalculateMaxHeightDifference(nodes);
        }


        //列的对齐方式
        private void BindGridAlignment()
        {
            if (dgvIndoorDiff.Rows.Count == 0)
                return;
            for (int i = 0; i < dgvIndoorDiff.Rows.Count; i++)
            {
                if (dgvIndoorDiff.Rows[i].Cells[0].Value.ToString() == "0")
                {
                    dgvIndoorDiff.Rows[i].Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }
                else {
                    dgvIndoorDiff.Rows[i].Cells[2].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    dgvIndoorDiff.Rows[i].Cells[2].Style.Padding = new Padding(37, 0, 0, 0);
                }

                dgvIndoorDiff.Rows[i].Cells[4].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            } 
        } 
        private void BindPositionType()
        {
            List<String> TypeList = new List<string>();
            TypeList.Add(PipingPositionType.Upper.ToString());
            TypeList.Add(PipingPositionType.SameLevel.ToString());
            TypeList.Add(PipingPositionType.Lower.ToString());
            jccmbPosition.DataSource = TypeList;

            //DataTable dt = new DataTable();
            //dt.Columns.Add("Name");
            //dt.Columns.Add("Value");
            //dt.Rows.Add(new object[] { Msg.OUT_HIGH_DIFFERENCE_TYPE(PipingPositionType.Upper.ToString()), PipingPositionType.Upper.ToString() });
            //dt.Rows.Add(new object[] { Msg.OUT_HIGH_DIFFERENCE_TYPE(PipingPositionType.SameLevel.ToString()), PipingPositionType.SameLevel.ToString() });
            //dt.Rows.Add(new object[] { Msg.OUT_HIGH_DIFFERENCE_TYPE(PipingPositionType.Lower.ToString()), PipingPositionType.Lower.ToString() }); 
            //jccmbPosition.DisplayMember = "Name";
            //jccmbPosition.ValueMember = "Value";
            //jccmbPosition.DataSource = dt; 
            jccmbPosition.SelectedIndex = 1;
        }

        /// <summary>
        /// 创建Accessory 列
        /// </summary> 
        private void CreateDataGridViewColumns()
        {
            //加载数据
            dgvIndoorDiff.AutoGenerateColumns = true;
            dgvIndoorDiff.RowsDefaultCellStyle.BackColor = Color.White;
            dgvIndoorDiff.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            //创建表头 固定列
            dgvIndoorDiff.Columns.Add("Ind", "Ind");
            dgvIndoorDiff.Columns.Add("IsCH", "IsCH"); 
            dgvIndoorDiff.Columns.Add("Name", ShowText.Name);
            dgvIndoorDiff.Columns.Add("Position", ShowText.Position);
            dgvIndoorDiff.Columns.Add("Height Difference", ShowText.HeightDifference + "(" + ut_length + ")");
            //创建动态列
            this.dgvIndoorDiff.Columns[0].Visible = false;
            this.dgvIndoorDiff.Columns[1].Visible = false; 
            this.dgvIndoorDiff.Columns[2].Width = 300;
            this.dgvIndoorDiff.Columns[4].Width = 160;
        }

        private void jcbtnSetLengthOK_Click(object sender, EventArgs e)
        {

            if (!this.JCValidateForm()) return;

            //DialogResult = DialogResult.OK;
            bool errFlag = false;
            //if (this.rbtnHigher.Checked)
            //{
            //    //this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(50, UnitType.LENGTH_M, ut_length).ToString("n0"));

            //    //FSNS与FSNP机型高度能超过50，最大110m
            //    if (sysItemCopy.SelOutdoorType.Contains("FSNS") || sysItemCopy.SelOutdoorType.Contains("FSNP") || sysItemCopy.SelOutdoorType.Contains("FSXNS") || sysItemCopy.SelOutdoorType.Contains("FSXNP") || sysItemCopy.SelOutdoorType.Contains("JTOH-BS1") || sysItemCopy.SelOutdoorType.Contains("JTOR-BS1"))
            //    {
                    
            //        //高度超过50.提示信息
            //        if (Convert.ToDecimal(jctxtHighDifference.Text) > 50)
            //            msgFlag = true;
            //        else
            //            msgFlag = false;

            //        this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(110, UnitType.LENGTH_M, ut_length).ToString("n0"));

            //    }
            //    else
            //        this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(50, UnitType.LENGTH_M, ut_length).ToString("n0"));
            //}
            //else
            //{
            //    this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(40, UnitType.LENGTH_M, ut_length).ToString("n0"));
            //}

            //if (!JCValidateSingle(jctxtHighDifference))
            //    return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                BindPipeLength();

                // 修改了Pipe Length之后要重新执行自动选型（注：若用户手动输入的pipe长度值则无效）
                //SelectOutdoorResult result = DoSelectOutdoor(out ERRList);

                //如果最长等效管长小于设置高度则加入错误列表
                if (sysItemSource.PipeEquivalentLength < sysItemSource.HeightDiff)
                {
                    double len = Unit.ConvertToControl(double.Parse(jctxtEqPipeLength.Text), UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(0, UnitType.LENGTH_M, ut_length);
                    JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(sysItemSource.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));
                    errFlag = true;
                    return;
                }

        
                //是否自动模式的判断  add by Shen Junjie on 20170619
                if (sysItemSource != null && sysItemSource.IsAuto)
                {
                    //室外机选型统一改用新逻辑 Global.DoSelectOutdoorODUFirst 20161112 by Yunxiao Lin
                    //result = Global.DoSelectOutdoorODUFirst(sysItemCopy, listRISelected, thisProject, out ERRList, out MSGList);
                    result = Global.DoSelectOutdoorODUFirst(sysItemSource, listRISelected, thisProject, out ERRList, out MSGList);
                    if (result == SelectOutdoorResult.Null)
                    {
                        errFlag = true;
                        return;
                    }
                }

                //if (!SaveToSourceSystem())
                //{
                //    errFlag = true;
                //}
            }
            catch (Exception exc)
            {
                JCMsg.ShowWarningOK(exc.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                if(!errFlag)
                    DialogResult = DialogResult.OK;     
            }
            
        }

        //private void jctxtHighDifference_TextChanged(object sender, EventArgs e)
        //{
        //    // 绑定高度差的最大最小值
        //    this.jctxtHighDifference.JCMinValue = 0;

        //    if (this.rbtnHigher.Checked)
        //    {

        //        if (sysItemCopy.SelOutdoorType.Contains("FSNS") || sysItemCopy.SelOutdoorType.Contains("FSNP") || sysItemCopy.SelOutdoorType.Contains("FSXNS") || sysItemCopy.SelOutdoorType.Contains("FSXNP") || sysItemCopy.SelOutdoorType.Contains("JTOH-BS1") || sysItemCopy.SelOutdoorType.Contains("JTOR-BS1"))
        //        {
        //               this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(110, UnitType.LENGTH_M, ut_length).ToString("n0"));                  
        //        }
                    
        //        else
        //            this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(50, UnitType.LENGTH_M, ut_length).ToString("n0"));
        //    }
        //    else
        //    {
        //        this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(40, UnitType.LENGTH_M, ut_length).ToString("n0"));
        //    }

        //    JCValidateSingle(jctxtHighDifference);
        //}

        private void jcbtnSetLengthCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnControlCancel_Click(object sender, EventArgs e)
        {
            this.panelHighDifference.Visible = false;
            RefreshPanel();
        }

        private void btnControlOK_Click(object sender, EventArgs e)
        {
            if (!JCValidateSingle(jctxtIndoorDifference))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            } 
            RoomIndoor emptyIndoor = new RoomIndoor();
            if (this.jccmbPosition.SelectedIndex == 0)
            {
                emptyIndoor.PositionType = PipingPositionType.Upper.ToString();
            }
            else if (this.jccmbPosition.SelectedIndex == 1)
            {
                emptyIndoor.PositionType = PipingPositionType.SameLevel.ToString();
            }
            else
            {
                emptyIndoor.PositionType = PipingPositionType.Lower.ToString();
            }
            emptyIndoor.HeightDiff = Unit.ConvertToSource(Convert.ToDouble(this.jctxtIndoorDifference.Text == "" ? "0" : this.jctxtIndoorDifference.Text), UnitType.LENGTH_M, ut_length);

            if (emptyIndoor.PositionType != PipingPositionType.SameLevel.ToString())
            {
                if (emptyIndoor.HeightDiff <= 0)
                {
                    JCMsg.ShowErrorOK(Msg.GetResourceString("INDOOR_HIGHERDIFFERENCE_LENGTH"));
                    return;
                }
            }
            //判断当前室外机高度差
            if (emptyIndoor.PositionType == PipingPositionType.Upper.ToString() && emptyIndoor.HeightDiff > sysItemSource.MaxOutdoorAboveHeight)
            {
                double len = Unit.ConvertToControl(sysItemSource.MaxOutdoorAboveHeight, UnitType.LENGTH_M, ut_length); 
                JCMsg.ShowErrorOK(Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length));
                return;
            }
            if (emptyIndoor.PositionType == PipingPositionType.Lower.ToString() && emptyIndoor.HeightDiff > sysItemSource.MaxOutdoorBelowHeight)
            {
                double len = Unit.ConvertToControl(sysItemSource.MaxOutdoorBelowHeight, UnitType.LENGTH_M, ut_length);
                JCMsg.ShowErrorOK(Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length));
                return;
            }
            foreach (SelectedIDUList ind in SelIDUList)
            { 
                int IDU_ID = ind.IndoorNo;
                double HeightDiff= emptyIndoor.HeightDiff;
                if (ind.IndoorTag is MyNodeCH)
                {
                    MyNodeCH nodech= ind.IndoorTag as MyNodeCH;
                    if (emptyIndoor.PositionType == PipingPositionType.Lower.ToString())
                    {
                        HeightDiff = -HeightDiff;
                    } 
                    nodech.HeightDiff = HeightDiff; 
                }
               else if (ind.IndoorTag is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodech = ind.IndoorTag as MyNodeMultiCH;
                    if (emptyIndoor.PositionType == PipingPositionType.Lower.ToString())
                    {
                        HeightDiff = -HeightDiff;
                    }
                    nodech.HeightDiff = HeightDiff;
                }
                else if (ind.IndoorTag is MyNodeIn)
                { 

                    MyNodeIn node=ind.IndoorTag as MyNodeIn;
                    node.RoomIndooItem.PositionType = emptyIndoor.PositionType.ToString();
                    node.RoomIndooItem.HeightDiff = HeightDiff;
                    //RoomIndoor ri = listRISelected.Find(p => p.IndoorNO == IDU_ID);
                   // UpdateHeightDiff(ri, emptyIndoor);
                    //RoomIndoor inds = thisProject.RoomIndoorList.Find(p => p.IndoorNO == IDU_ID);
                    //UpdateHeightDiff(inds, emptyIndoor);
                }
            } 
            RefreshPanel();
            BindHighDifference();  
            //验证当前输入的高度差 是否大于系统
            VerificationHighDiff();
        }


        private void VerificationHighDiff()
        {
            ERRList = new List<string>();
            double maxValue = CalculateHighDiff();
            if (maxValue > sysItemSource.MaxDiffIndoorHeight)
            {
                double len = Unit.ConvertToControl(sysItemSource.MaxDiffIndoorHeight, UnitType.LENGTH_M, ut_length);
                this.jcLabMsg.Text = Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length);
                ERRList.Add(Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length));
            }
            else if (sysItemSource.MaxCHBoxHighDiffLength > sysItemSource.NormalCHBoxHighDiffLength)
            {
                double len = Unit.ConvertToControl(sysItemSource.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                this.jcLabMsg.Text = Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length);
                ERRList.Add(Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length));
            }
            else if (sysItemSource.MaxCHBox_IndoorHighDiffLength > sysItemSource.NormalCHBox_IndoorHighDiffLength)
            {
                double len = Unit.ConvertToControl(sysItemSource.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, ut_length);
                this.jcLabMsg.Text = Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length);
                ERRList.Add(Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length));
            }
            else if (sysItemSource.MaxSameCHBoxHighDiffLength > sysItemSource.NormalSameCHBoxHighDiffLength)
            {
                double len = Unit.ConvertToControl(sysItemSource.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                this.jcLabMsg.Text = Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length);
                ERRList.Add(Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length));
            }
            else
            {
                this.jcLabMsg.Text = "";
            } 
        }

        private double CalculateHighDiff()
        {
            List<double> diffList = new List<double>(); //高度差集合
            double maxValue = 0; //最大高度差
            double minValue = 0; //最小高度差
            double indDiff = 0; //室内机与室内机直接的高度差
            if (listRISelected.Count > 0)
            {
                foreach (RoomIndoor ri in listRISelected)
                {
                    double val = Convert.ToDouble(ri.HeightDiff);
                    if (ri.PositionType == PipingPositionType.Lower.ToString())
                    {
                        double m = 0 - val;
                        diffList.Add(m);
                    }
                    else
                    {
                        diffList.Add(val);
                    }
                }
                maxValue = Convert.ToDouble(diffList.Max().ToString("n1"));
                minValue = Convert.ToDouble(diffList.Min().ToString("n1"));
                if (maxValue > minValue)
                {
                    indDiff = maxValue - minValue;
                }
            }
            return indDiff;
        }

        /// <summary>
        /// 计算HeightDifference 的最大高度差
        /// </summary>
        private bool CalculateMaxHeightDifference(RoomIndoor ri)
        {
            bool isReturn = true;
            List<double> diffList = new List<double>(); //高度差集合
            double maxValue = 0; //最大高度差
            double minValue = 0; //最小高度差 
            double indDiff = 0;  //室内机高度差
            double inputValue = ri.HeightDiff;//输入值
            if (listRISelected.Count > 0)
            {

                foreach (RoomIndoor ind in listRISelected)
                {
                    double val = Convert.ToDouble(ind.HeightDiff);
                    if (ind.PositionType == PipingPositionType.Lower.ToString())
                    {
                        double m = 0 - val;
                        diffList.Add(m);
                    }
                    else
                    {
                        diffList.Add(val);
                    }
                }
                maxValue = Convert.ToDouble(diffList.Max().ToString("n1"));
                minValue = Convert.ToDouble(diffList.Min().ToString("n1"));

                if (ri.PositionType == PipingPositionType.Lower.ToString())
                {
                    inputValue = 0 - ri.HeightDiff;
                }
                if (inputValue > maxValue)
                {
                    maxValue = inputValue;
                }
                if (inputValue < minValue)
                {
                    minValue = inputValue;
                }
                if (maxValue > minValue)
                {
                    indDiff = maxValue - minValue;
                }

            }

            if (indDiff > sysItemSource.MaxDiffIndoorHeight)
            {
                isReturn = false;
            }
            return isReturn;
        }


        private void jccmbPosition_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (this.jccmbPosition.SelectedIndex == 1)
            {
                this.jctxtIndoorDifference.Text = "0.0";
                this.jctxtIndoorDifference.Enabled = false;
            }
            else
            {
                this.jctxtIndoorDifference.Enabled = true;
            }
        }

        private void RefreshPanel()
        {
            this.jccmbPosition.SelectedIndex = 1;
            this.jctxtIndoorDifference.Text = "0.0";
            this.jctxtIndoorDifference.Enabled = false;
            this.cklistIndoor.Items.Clear();

        }
        private void jcbtnSetting_Click(object sender, EventArgs e)
        {
            SelIDUList.Clear();
            RefreshPanel();
            foreach (DataGridViewRow r in this.dgvIndoorDiff.SelectedRows)
            {
                SelectedIDUList sel = new SelectedIDUList();
                sel.IndoorNo = Convert.ToInt32(r.Cells[0].Value.ToString());
                
                sel.IndoorTag = r.Cells[0].Tag as MyNode;
                if (!string.IsNullOrEmpty(r.Cells[3].Value.ToString()))
                     sel.IndoorModel = r.Cells[3].Value.ToString();
                SelIDUList.Add(sel);
            }

            if (SelIDUList.Count > 0)
            {
                //foreach (SelectedIDUList ind in SelIDUList)
                //{
                //    cklistIndoor.Items.Add(new CheckBoxItem(ind.IndoorName + " [" + ind.IndoorModel + "] ", ind.IndoorNo.ToString()), true);
                //}

                //CHbox
                if (SelIDUList[0].IndoorTag is MyNodeCH)  
                {
                    MyNodeCH nodeCh = SelIDUList[0].IndoorTag as MyNodeCH;
                    if (nodeCh.HeightDiff < 0)
                    {
                        this.jccmbPosition.SelectedIndex = 2;
                        this.jctxtIndoorDifference.Enabled = true;
                        nodeCh.HeightDiff = -nodeCh.HeightDiff;
                        this.jctxtIndoorDifference.Text = nodeCh.HeightDiff.ToString("n1");
                    }
                    else if (nodeCh.HeightDiff == 0)
                    {
                        this.jccmbPosition.SelectedIndex = 1;
                        this.jctxtIndoorDifference.Enabled = false;
                        this.jctxtIndoorDifference.Text = nodeCh.HeightDiff.ToString("n1");
                    }
                    else
                    {
                        this.jccmbPosition.SelectedIndex = 0;
                        this.jctxtIndoorDifference.Enabled = true;
                        this.jctxtIndoorDifference.Text = nodeCh.HeightDiff.ToString("n1");
                    }
                }
                //MultiCHbox
                else if (SelIDUList[0].IndoorTag is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeCh = SelIDUList[0].IndoorTag as MyNodeMultiCH;
                    if (nodeCh.HeightDiff < 0)
                    {
                        this.jccmbPosition.SelectedIndex = 2;
                        this.jctxtIndoorDifference.Enabled = true;
                        nodeCh.HeightDiff = -nodeCh.HeightDiff;
                        this.jctxtIndoorDifference.Text = nodeCh.HeightDiff.ToString("n1");
                    }
                    else if (nodeCh.HeightDiff == 0)
                    {
                        this.jccmbPosition.SelectedIndex = 1;
                        this.jctxtIndoorDifference.Enabled = false;
                        this.jctxtIndoorDifference.Text = nodeCh.HeightDiff.ToString("n1");
                    }
                    else
                    {
                        this.jccmbPosition.SelectedIndex = 0;
                        this.jctxtIndoorDifference.Enabled = true;
                        this.jctxtIndoorDifference.Text = nodeCh.HeightDiff.ToString("n1");
                    }
                }
                //RoomIndoor
                else {
                    MyNodeIn nodeCh = SelIDUList[0].IndoorTag as MyNodeIn;
                    if (nodeCh.RoomIndooItem.PositionType == PipingPositionType.Lower.ToString())
                    {
                        this.jccmbPosition.SelectedIndex = 2;
                        this.jctxtIndoorDifference.Enabled = true; 
                        this.jctxtIndoorDifference.Text = nodeCh.RoomIndooItem.HeightDiff.ToString("n1");
                    }
                    else if (nodeCh.RoomIndooItem.PositionType == PipingPositionType.SameLevel.ToString())
                    {
                        this.jccmbPosition.SelectedIndex = 1;
                        this.jctxtIndoorDifference.Enabled = false;
                        this.jctxtIndoorDifference.Text = nodeCh.RoomIndooItem.HeightDiff.ToString("n1");
                    }
                    else
                    {
                        this.jccmbPosition.SelectedIndex = 0;
                        this.jctxtIndoorDifference.Enabled = true;
                        this.jctxtIndoorDifference.Text = nodeCh.RoomIndooItem.HeightDiff.ToString("n1");
                    }
                }
            }
            panelHighDifference.Location = new Point(303, 164);
            panelHighDifference.Visible = true;
           
        }

        /// <summary>
        /// 将当前选择的室外机系统数据加入当前项目文件
        /// </summary>
        private void BindPipeLength()
        {
            double fl = Convert.ToDouble(this.jctxtFirstPipeLength.Text);
            double eql = Convert.ToDouble(this.jctxtEqPipeLength.Text);
            // double hd = Convert.ToDouble(this.jctxtHighDifference.Text);
            sysItemSource.FirstPipeLength = Unit.ConvertToSource(fl, UnitType.LENGTH_M, ut_length);
            sysItemSource.PipeEquivalentLength = Unit.ConvertToSource(eql, UnitType.LENGTH_M, ut_length);
            if (!sysItemSource.IsInputLengthManually)
            {
                //自动配管需要保存修改的管长 20160701 by Yunxiao Lin
                sysItemSource.FirstPipeLengthbuff = sysItemSource.FirstPipeLength;
                sysItemSource.PipeEquivalentLengthbuff = sysItemSource.PipeEquivalentLength;
            }
           
            //if (this.rbtnSameLevel.Checked)
            //{
            //    sysItemCopy.PipingPositionType = PipingPositionType.SameLevel;
            //    //sysItemCopy.HeightDiff = 0;
            //}
            //else
            //{
            //    double diff = hd;
            //    if (this.rbtnHigher.Checked)
            //    {
            //        sysItemCopy.PipingPositionType = PipingPositionType.Upper;
            //    }
            //    else
            //    {
            //        sysItemCopy.PipingPositionType = PipingPositionType.Lower;
            //        //diff = 0 - hd;
            //    }
            //    sysItemCopy.HeightDiff = Unit.ConvertToSource(diff, UnitType.LENGTH_M, ut_length);
            //}

            double factor = GetPipeLengthRevisedFactor(sysItemSource, "Cooling");
            this.jclblPipingFactorValue.Text = factor.ToString("n3");
        }

        private bool SaveToSourceSystem()
        {
            if (sysItemSource == null || sysItemSource.OutdoorItem == null)
            {
                return false;
            }

            sysItemSource.Copy(sysItemSource);

            //sysItemSource.FirstPipeLength = sysItemCopy.FirstPipeLength;
            //sysItemSource.PipeEquivalentLength = sysItemCopy.PipeEquivalentLength;
            //sysItemSource.PipingPositionType = sysItemCopy.PipingPositionType;
            //sysItemSource.HeightDiff = sysItemCopy.HeightDiff;
            //sysItemSource.PipingLengthFactor = sysItemCopy.PipingLengthFactor;

            //sysItemSource.FirstPipeLengthbuff = sysItemCopy.FirstPipeLengthbuff;
            //sysItemSource.PipeEquivalentLengthbuff = sysItemCopy.PipeEquivalentLengthbuff;

            ////SelectOutdoorResult result = Global.DoSelectOutdoorODUFirst(sysItemSource, listRISelected, thisProject, out ERRList, out MSGList);
            //SelectOutdoorResult result;
            //if (thisProject.RegionCode.Substring(0, 2) == "EU")
            //    result = Global.DoSelectUniversalODUFirst(sysItemSource, listRISelected, thisProject, sysItemSource.OutdoorItem.Series, sysItemSource.OutdoorItem.ProductType, out ERRList, out MSGList);
            //else
            //    result = Global.DoSelectOutdoorODUFirst(sysItemSource, listRISelected, thisProject, out ERRList, out MSGList);
            //if (result == SelectOutdoorResult.Null)
            //{
            //    return false;
            //}
            return true;
        }

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


        //初始化控件属性
        private void initPipLength()
        {
            //记录初始值 add by Shen Junjie on 20171117
            this.originFirstPipeLength = sysItemSource.FirstPipeLength;
            this.originPipeEquivalentLength = sysItemSource.PipeEquivalentLength;
            this.originPipingPositionType = sysItemSource.PipingPositionType;
            this.originHeightDiff = sysItemSource.HeightDiff;

            //this.jctxtFirstPipeLength.Text = sysItemCopy.FirstPipeLength.ToString();
            //this.jctxtEqPipeLength.Text = sysItemCopy.PipeEquivalentLength.ToString();
            //Pepi Length 单位数据转换
            this.jctxtEqPipeLength.Text = Unit.ConvertToControl(sysItemSource.PipeEquivalentLength, UnitType.LENGTH_M, ut_length).ToString("n1");
            this.jctxtFirstPipeLength.Text = Unit.ConvertToControl(sysItemSource.FirstPipeLength, UnitType.LENGTH_M, ut_length).ToString("n1");
            //if (thisProject.RegionCode.Substring(0, 2) == "EU")
            //{
                this.jctxtEqPipeLength.JCMinValue = 0;
                this.jctxtEqPipeLength.JCMaxValue = (float)Unit.ConvertToControl(sysItemSource.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);
            // }
            isVal = sysItemSource.MaxIndoorLength > 0 ? true : false;
            if (isVal)
            {
                this.jctxtFirstPipeLength.JCMinValue = 0;
                this.jctxtFirstPipeLength.JCMaxValue = (float)Unit.ConvertToControl(sysItemSource.MaxIndoorLength, UnitType.LENGTH_M, ut_length);
            }

            //FSNS与FSNP机型高度能超过50，最大110m
            //if (sysItemCopy.SelOutdoorType.Contains("FSNS") || sysItemCopy.SelOutdoorType.Contains("FSNP") || sysItemCopy.SelOutdoorType.Contains("FSXNS") || sysItemCopy.SelOutdoorType.Contains("FSXNP") || sysItemCopy.SelOutdoorType.Contains("JTOH-BS1") || sysItemCopy.SelOutdoorType.Contains("JTOR-BS1"))
            //    this.jctxtHighDifference.JCMaxValue = float.Parse(Unit.ConvertToControl(110, UnitType.LENGTH_M, ut_length).ToString("n0"));
            //else
            //    this.jctxtHighDifference.Text = Math.Abs(Unit.ConvertToControl(sysItemCopy.HeightDiff, UnitType.LENGTH_M, ut_length)).ToString("n1");
            //if (sysItemCopy.PipingPositionType == PipingPositionType.SameLevel)
            //    this.rbtnSameLevel.Checked = true;
            //else if (sysItemCopy.PipingPositionType == PipingPositionType.Upper)
            //    this.rbtnHigher.Checked = true;
            //else
            //    this.rbtnLower.Checked = true;



            // add on 20160401 clh 当前系统管长为手动输入时，此处不能修改
            this.jctxtEqPipeLength.Enabled = !sysItemSource.IsInputLengthManually;
            this.jctxtFirstPipeLength.Enabled = !sysItemSource.IsInputLengthManually;


            jcLabel7.Text = ut_length.ToString();
            jcLabel8.Text = ut_length.ToString();
           // jcLabel6.Text = ut_length.ToString();
        }

        //private void rbtnSameLevel_CheckedChanged(object sender, EventArgs e)
        //{
        //    this.pnlHighDiff.Visible = !rbtnSameLevel.Checked;
        //    //2013-10-21 by Yang 修正了SameLevel判断取反的问题
        //    if (rbtnSameLevel.Checked)
        //    {
        //        jctxtHighDifference.Text = "0";
        //    }
        //    else
        //    {
        //        jctxtHighDifference.Text = Math.Abs(Unit.ConvertToControl(sysItemCopy.HeightDiff, UnitType.LENGTH_M, ut_length)).ToString("n1");
        //    }
        //}
        
        //获取重新选型结果集
        /// <summary>
        /// 获取重新选型结果集
        /// </summary>
        /// <returns></returns>
        public SelectOutdoorResult getResult()
        {
            return result;
        }

        //获取重新选型错误信息列表
        /// <summary>
        /// 获取重新选型错误信息列表
        /// </summary>
        /// <returns></returns>
        public List<string> getErrList()
        {
            return ERRList;
        }

        private void frmPipingLengthSetting_FormClosing(object sender, FormClosingEventArgs e)
        {

            //室内机距离室外机的最大高度差 重新赋值给室外机HeightDiff  on 20180628 by xyj
            if (sysItemSource.MaxUpperHeightDifferenceLength > 0 || sysItemSource.MaxLowerHeightDifferenceLength > 0)
            {
                if (sysItemSource.MaxUpperHeightDifferenceLength > sysItemSource.MaxLowerHeightDifferenceLength)
                {
                    sysItemSource.HeightDiff = sysItemSource.MaxUpperHeightDifferenceLength;
                    sysItemSource.PipingPositionType = PipingPositionType.Upper;
                }
                else if (sysItemSource.MaxUpperHeightDifferenceLength < sysItemSource.MaxLowerHeightDifferenceLength)
                {
                    sysItemSource.HeightDiff = sysItemSource.MaxLowerHeightDifferenceLength;
                    sysItemSource.PipingPositionType = PipingPositionType.Lower;
                }
                //如果最大高度差 与最低高度差相同 随便取一个高度差 与位置 
                else if (sysItemSource.MaxUpperHeightDifferenceLength == sysItemSource.MaxLowerHeightDifferenceLength)
                {
                    sysItemSource.HeightDiff = sysItemSource.MaxUpperHeightDifferenceLength;
                    sysItemSource.PipingPositionType = PipingPositionType.Upper;
                }
            }
            else {
                sysItemSource.HeightDiff = 0;
                sysItemSource.PipingPositionType = PipingPositionType.SameLevel;
            }
            SaveToSourceSystem();
            dgvIndoorDiff.Focus();  //这段代码非常重要  如果焦点直接离开文本框至关闭窗体是不触发Leave 事件 所以需要先转移焦点 on 20180514 by xyj
            DialogResult = DialogResult.OK;
        }



        /// <summary>
        /// 计算HeightDifference 的最大高度差
        /// </summary>
        private void RecalculateMaxHeightDifference(List<MyNode> list)
        { 
            double MaxOutdoorLength = 0.0;
            //统计高度差
            pbll.StatisticsSystem_HighDiff(sysItemSource, list); 
            
            if (sysItemSource.MaxUpperHeightDifferenceLength > sysItemSource.MaxLowerHeightDifferenceLength)
            {
                MaxOutdoorLength = sysItemSource.MaxUpperHeightDifferenceLength;
            }
            else {
                MaxOutdoorLength = sysItemSource.MaxLowerHeightDifferenceLength;
            }
           
            jcLabMaxIndoorHeightDifference.Text = Msg.GetResourceString("INDOOR_EACH_HIGHERDIFFERENCE_LENGTH") + " : " + sysItemSource.MaxIndoorHeightDifferenceLength.ToString("n1") + ut_length;
            jcLabMaxOutdoorHeightDifference.Text = Msg.GetResourceString("OUTDOOR_EACH_HIGHERDIFFERENCE_LENGTH") + " : " + MaxOutdoorLength.ToString("n1") + ut_length;

            //验证当前输入的高度差 是否大于系统
            VerificationHighDiff();
        }
         
        private void dgvIndoorDiff_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = UtilColor.bg_dgvHeader_Indoor;
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
                        border.X += 1;
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private void dgvIndoorDiff_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.panelHighDifference.Visible = false;
            RefreshPanel();
        }

        /// <summary>
        /// 更新室内机高度差
        /// </summary>
        /// <param name="ri"></param>
        private void UpdateHeightDiff(RoomIndoor ri, RoomIndoor emptyIndoor)
        {
            if (ri != null && ri.IndoorNO > 0)
            {
                ri.PositionType = emptyIndoor.PositionType;
                ri.HeightDiff = emptyIndoor.HeightDiff;
            }
        }

        /// <summary>
        /// 判断是否选中室内机
        /// </summary>
        /// <returns></returns>
        private bool isSelectedIndoor()
        {
            bool isSelected = false;
            for (int i = 0; i < cklistIndoor.Items.Count; i++)
            {
                if (cklistIndoor.GetItemChecked(i))
                {
                    return true;
                }
            }
            return isSelected;
        }
         

        private void jctxtIndoorDifference_TextChanged(object sender, EventArgs e)
        {
            JCValidateSingle(jctxtIndoorDifference);
        }
         

        private void jctxtEqPipeLength_Leave(object sender, EventArgs e)
        {
            if (!isVal && JCValidateSingle(jctxtEqPipeLength))
            {
                BindPipeLength();
                return;
            } 
            if (JCValidateSingle(jctxtEqPipeLength)&&JCValidateSingle(jctxtFirstPipeLength))
            { 
                BindPipeLength(); 
            }
            
        }

        private void jctxtFirstPipeLength_Leave(object sender, EventArgs e)
        {
            if (!isVal && JCValidateSingle(jctxtEqPipeLength))
            {
                BindPipeLength();
                return;
            }
            if (JCValidateSingle(jctxtFirstPipeLength) && JCValidateSingle(jctxtEqPipeLength))
            { 
                BindPipeLength();
            }
        }
        Point pt;
        private void panelHighDifference_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int px = Cursor.Position.X - pt.X;
                int py = Cursor.Position.Y - pt.Y;
                panelHighDifference.Location = new Point(panelHighDifference.Location.X + px, panelHighDifference.Location.Y + py);

                pt = Cursor.Position;
            }
        }

        private void panelHighDifference_MouseDown(object sender, MouseEventArgs e)
        {
            pt = Cursor.Position;
        }
    }
}
