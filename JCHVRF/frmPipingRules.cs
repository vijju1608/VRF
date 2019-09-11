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
using Lassalle.Flow;  
using JCHVRF.MyPipingBLL;
using JCHVRF.VRFMessage;
using JCBase.Utility;
namespace JCHVRF
{
    public partial class frmPipingRules : JCBase.UI.JCForm
    {
        Project thisProject;
        SystemVRF currSystem;
        int row = 8;
        int strarRow = 11;
        string utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;

        bool HeatRecovery = false;
        public frmPipingRules(Project thisProj, SystemVRF sysItem)
        {
            InitializeComponent();
            thisProject = thisProj;
            currSystem = sysItem;
            isHeatRecovery(currSystem);
        }
        private void frmPipingRules_Load(object sender, EventArgs e)
        {

            this.JCCallValidationManager = true;
            JCSetLanguage();
            this.Text = Msg.GetResourceString("FormPipingRules");
            //创建动态列
            CreateDataGridViewColumns();
            InitData(); 
            if (!HeatRecovery)
            {
                this.Height = 435;
            }
        }

        private void isHeatRecovery(SystemVRF system)
        { 
            if(system.OutdoorItem.ProductType.Contains("Heat Recovery") || system.OutdoorItem.ProductType.Contains(", HR"))
            {
                HeatRecovery = true; 
            } 
         }

        private void InitData()
        {
            if (currSystem == null)
                return;
            var inds = thisProject.RoomIndoorList.FindAll(p => p.SystemID == currSystem.Id);
            int indsCount = 0;
            if (inds != null && inds.Count > 0)
            {
                indsCount = inds.Count;
            }
            var lengthList = new List<dynamic>();
            var heightList = new List<dynamic>();
            var pipingRules = new { Length = lengthList, Height = heightList }; 
            int starRow = 0;



            #region Length
            dgvPipingRules.Rows.Add(1);
            //Total pipe length
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_TotalPipeLength");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.TotalPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxTotalPipeLength == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxTotalPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxTotalPipeLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.TotalPipeLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxTotalPipeLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);
            
            //Maximum piping length (Actual length)
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_PipeActualLength");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.PipeActualLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxPipeLength == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxPipeLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.PipeActualLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxPipeLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);
           
            //Maximum piping length (Equivalent length)
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_PipeEquivalentLength");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.PipeEquivalentLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxEqPipeLength == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxEqPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxEqPipeLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.PipeEquivalentLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxEqPipeLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);

            //Maximum Piping Length between Multi-kit of 1st Branch and Each Indoor Unit
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_FirstPipeLength");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.FirstPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxIndoorLength == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxIndoorLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxIndoorLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.FirstPipeLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxIndoorLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);

            //Maximum Piping Length between Each Multi-kit and Each Indoor Unit
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_ActualMaxMKIndoorPipeLength");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.ActualMaxMKIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxMKIndoorPipeLength == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxMKIndoorPipeLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.ActualMaxMKIndoorPipeLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);
         
            //Piping Length between Piping Connection Kit 1 and Each Outdoor Unit
            //单独的室外机不需要显示该项限制  
            if (!string.IsNullOrEmpty(currSystem.OutdoorItem.JointKitModelG.Trim()) && currSystem.OutdoorItem.JointKitModelG.Trim() != "-")
            {
                starRow = starRow + 1;
                dgvPipingRules.Rows.Add(1);
                dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
                dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_PipeLengthes");
                double MaxPipeLengths = 0;
                if (currSystem.IsInputLengthManually)
                {
                    MaxPipeLengths = PipingBLL.GetMaxPipeLengthOfNodeOut(currSystem.MyPipingNodeOut);
                }
                dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(MaxPipeLengths, UnitType.LENGTH_M, utLength).ToString("n0");
                dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxFirstConnectionKitToEachODU == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxFirstConnectionKitToEachODU, UnitType.LENGTH_M, utLength).ToString("n0");
                dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxFirstConnectionKitToEachODU == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(MaxPipeLengths > 0 ? MaxPipeLengths : 0, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxFirstConnectionKitToEachODU, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);
            }


            if (HeatRecovery)
            {
                bool isAllOK = true;
                var chBoxs = new List<dynamic>();
                JCHVRF.MyPipingBLL.PipingBLL.EachNode(currSystem.MyPipingNodeOut, (node1) =>
                {
                    double actual;
                    double max;
                    string model;
                    if (node1 is MyNodeCH)
                    {
                        var item = (MyNodeCH)node1;
                        actual = item.ActualTotalCHIndoorPipeLength;
                        max = item.MaxTotalCHIndoorPipeLength;
                        model = item.Model;
                    }
                    else if (node1 is MyNodeMultiCH)
                    {
                        var item = (MyNodeMultiCH)node1;
                        actual = item.ActualTotalCHIndoorPipeLength;
                        max = item.MaxTotalCHIndoorPipeLength;
                        model = item.Model;
                    }
                    else
                    {
                        return;
                    }

                    bool isOK = !(max > 0 && actual > max);
                    isAllOK = isAllOK && isOK;

                    var chbox = chBoxs.Find(p => p.Rules == model);
                    if (chbox == null)
                    {
                        chbox = new System.Dynamic.ExpandoObject();
                        chbox.Rules = model;
                        chbox.Actual = actual;
                        chbox.Max = max;
                        chbox.isOK = isOK;
                        chBoxs.Add(chbox);
                    }
                    else
                    {
                        if ((chbox.isOK && isOK && actual > chbox.Actual)  //高的覆盖低的
                            || (chbox.isOK && !isOK))   //出错的覆盖正常的
                        {
                            chbox.Actual = actual;
                            chbox.Max = max;
                            chbox.isOK = isOK;
                        }
                    }
                });
                if (chBoxs.Count > 0)
                {
                    //Total piping length between CH-Box and Each Indoor Unit
                    starRow = starRow + 1;
                    dgvPipingRules.Rows.Add(1);
                    dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
                    dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_CHBoxs");
                    dgvPipingRules.Rows[starRow].Cells[2].Value = "-";
                    dgvPipingRules.Rows[starRow].Cells[3].Value = "-";
                    dgvPipingRules.Rows[starRow].Cells[4].Value = isAllOK ? Properties.Resources.piping_ok : Properties.Resources.piping_error;

                    chBoxs.ForEach((c) =>
                    {
                        starRow = starRow + 1;
                        dgvPipingRules.Rows.Add(1);
                        dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Length");
                        dgvPipingRules.Rows[starRow].Cells[1].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                        dgvPipingRules.Rows[starRow].Cells[1].Value = c.Rules == null ? Msg.GetResourceString("PipingRules_CH_Box") : c.Rules;
                        dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(c.Actual, UnitType.LENGTH_M, utLength).ToString("n0");
                        dgvPipingRules.Rows[starRow].Cells[3].Value = c.Max > 0 ? Unit.ConvertToControl(c.Max, UnitType.LENGTH_M, utLength).ToString("n0") : "-";
                        dgvPipingRules.Rows[starRow].Cells[4].Value = c.isOK ? Properties.Resources.piping_ok : Properties.Resources.piping_error;
                    });
                }
            }

            #endregion

            #region Height
            //Height Difference between (O.U. is Upper)
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Height");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_HeightDiffH");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.MaxUpperHeightDifferenceLength, UnitType.LENGTH_M, utLength).ToString("0.#");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxOutdoorAboveHeight == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, utLength).ToString("0.#");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxOutdoorAboveHeight == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.MaxUpperHeightDifferenceLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);

            //Height Difference between (O.U. is Lower)
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Height");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_HeightDiffL");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.MaxLowerHeightDifferenceLength, UnitType.LENGTH_M, utLength).ToString("0.#");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxOutdoorBelowHeight == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, utLength).ToString("0.#");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxOutdoorBelowHeight == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.MaxLowerHeightDifferenceLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);

            //Height Difference between Indoor units
            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Height");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_DiffIndoorHeight");
            dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.MaxIndoorHeightDifferenceLength, UnitType.LENGTH_M, utLength).ToString("0.#");
            dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.MaxDiffIndoorHeight == 0 ? "-" : Unit.ConvertToControl(currSystem.MaxDiffIndoorHeight, UnitType.LENGTH_M, utLength).ToString("0.#");
            dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.MaxDiffIndoorHeight == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.MaxIndoorHeightDifferenceLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.MaxDiffIndoorHeight, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);
            #endregion

            if (HeatRecovery)
            {
                starRow = starRow + 1;
                dgvPipingRules.Rows.Add(1);
                //Height Difference between CH-Box and Indoor Units
                dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Height");
                dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_DiffCHBox_IndoorHeight");
                dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.MaxCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#");
                dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.NormalCHBox_IndoorHighDiffLength == 0 ? "-" : Unit.ConvertToControl(currSystem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#");
                dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.NormalCHBox_IndoorHighDiffLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.MaxCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);

                starRow = starRow + 1;
                dgvPipingRules.Rows.Add(1);
                //Height Difference between Indoor Units using the Same Branch of CH-Box
                dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Height");
                dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_DiffMulitBoxHeight");
                dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.MaxSameCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#");
                dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.NormalSameCHBoxHighDiffLength == 0 ? "-" : Unit.ConvertToControl(currSystem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#");
                dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.NormalSameCHBoxHighDiffLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.MaxSameCHBoxHighDiffLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);

                starRow = starRow + 1;
                dgvPipingRules.Rows.Add(1);
                //Height Difference between CH-Boxes
                dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_Height");
                dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_DiffCHBoxHeight");
                dgvPipingRules.Rows[starRow].Cells[2].Value = Unit.ConvertToControl(currSystem.MaxCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#");
                dgvPipingRules.Rows[starRow].Cells[3].Value = currSystem.NormalCHBoxHighDiffLength == 0 ? "-" : Unit.ConvertToControl(currSystem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#");
                dgvPipingRules.Rows[starRow].Cells[4].Value = currSystem.NormalCHBoxHighDiffLength == 0 ? Properties.Resources.piping_ok : (Unit.ConvertToControl(currSystem.MaxCHBoxHighDiffLength, UnitType.LENGTH_M, utLength) <= Unit.ConvertToControl(currSystem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, utLength) ? Properties.Resources.piping_ok : Properties.Resources.piping_error);
            }

            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = "";
            dgvPipingRules.Rows[starRow].Cells[1].Value = "";
            dgvPipingRules.Rows[starRow].Cells[2].Value = "";
            dgvPipingRules.Rows[starRow].Cells[3].Value = "";
            dgvPipingRules.Rows[starRow].Cells[4].Value = Properties.Resources.piping_Transparent;


            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_IUConnectable");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_IUConnectable");
            dgvPipingRules.Rows[starRow].Cells[2].Value = indsCount;
            dgvPipingRules.Rows[starRow].Cells[3].Value = "1 / "+currSystem.OutdoorItem.RecommendedIU+" / "+currSystem.OutdoorItem.MaxIU;
            dgvPipingRules.Rows[starRow].Cells[4].Value = Properties.Resources.piping_ok;

            string Ratio = "50%-130%";
            if (currSystem.OutdoorItem.Series.Contains("FSNP") 
                || currSystem.OutdoorItem.Series.Contains("FSXNP")
                || currSystem.OutdoorItem.Series.Contains("FSNS7B") || currSystem.OutdoorItem.Series.Contains("FSNS5B")
                || currSystem.OutdoorItem.Series.Contains("FSNC7B") || currSystem.OutdoorItem.Series.Contains("FSNC5B") //巴西的Connection Ratio可以达到150% 20190105 by Yunxiao Lin
                )
                Ratio = "50%-150%";
            if (currSystem.OutdoorItem.Series.Contains("FSXNPE"))
            {
                if (currSystem.OutdoorItem.CoolingCapacity > 150)
                {
                    Ratio = "50%-130%";
                } 
            }

            starRow = starRow + 1;
            dgvPipingRules.Rows.Add(1);
            dgvPipingRules.Rows[starRow].Cells[0].Value = Msg.GetResourceString("PipingRules_ConnectedCap");
            dgvPipingRules.Rows[starRow].Cells[1].Value = Msg.GetResourceString("PipingRules_ConnectedCap");
            dgvPipingRules.Rows[starRow].Cells[2].Value = (currSystem.Ratio * 100).ToString("n0")+"%";
            dgvPipingRules.Rows[starRow].Cells[3].Value = Ratio;
            dgvPipingRules.Rows[starRow].Cells[4].Value = Properties.Resources.piping_ok;
            
        }
        

        /// <summary>
        /// 创建PipingRules 列
        /// </summary> 
        private void CreateDataGridViewColumns()
        { 
            //创建表头 固定列
            dgvPipingRules.Columns.Add("IndexNumber","");
            dgvPipingRules.Columns.Add("Title", currSystem.OutdoorItem.Series + " " + currSystem.OutdoorItem.AuxModelName);
            dgvPipingRules.Columns.Add("ProjectLength", Msg.GetResourceString("PipingRules_Project") + " \n" + utLength);
            dgvPipingRules.Columns.Add("MaxLength", Msg.GetResourceString("PipingRules_Max") + "\n" + utLength);
            DataGridViewImageColumn column = new DataGridViewImageColumn();
            column.HeaderText = Msg.GetResourceString("PipingRules_Result");
            dgvPipingRules.Columns.Add(column); 

            //指定列不可编辑及列宽
            InitializeDataGridView_ColumnsWidth();
            //添加行
            if (!string.IsNullOrEmpty(currSystem.OutdoorItem.JointKitModelG.Trim()) && currSystem.OutdoorItem.JointKitModelG.Trim() != "-")
            {
               // dgvPipingRules.Rows.Add(strarRow + 1);
                row = row + 1;
            }
            else
            {
               // dgvPipingRules.Rows.Add(strarRow);
            }

            var inds = thisProject.RoomIndoorList.FindAll(p => p.SystemID == currSystem.Id);
            int indsCount = 0;
            if (inds != null && inds.Count > 0)
            {
                indsCount = inds.Count;
            }
            //判断 当前系统是否是2pipe 3pipe 
            if (currSystem.OutdoorItem.ProductType.Contains("Heat Recovery") || currSystem.OutdoorItem.ProductType.Contains(", HR"))
            {
                var chBoxs = new List<dynamic>();
                row = row + 3;
                PipingBLL.EachNode(currSystem.MyPipingNodeOut, (node1) =>
                {
                    if (node1 is MyNodeCH)
                    {
                        var item = (MyNodeCH)node1;
                        var chbox = chBoxs.Find(p => p.Rules == item.Model);
                        if (chbox == null)
                        {
                            chbox = new System.Dynamic.ExpandoObject();
                            chbox.Rules = item.Model;
                            chbox.Actual = Unit.ConvertToControl(item.ActualTotalCHIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
                            chbox.Max = Unit.ConvertToControl(item.MaxTotalCHIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
                            chBoxs.Add(chbox);
                        }
                        else
                        {
                            var actual = Unit.ConvertToControl(item.ActualTotalCHIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
                            if (Convert.ToDouble(chbox.Actual) < Convert.ToDouble(actual))
                            {
                                chbox.Actual = actual;
                            }
                        }
                    }
                    else if (node1 is MyNodeMultiCH)
                    {
                        var item = (MyNodeMultiCH)node1;
                        var chbox = chBoxs.Find(p => p.Rules == item.Model);
                        if (chbox == null)
                        {
                            chbox = new System.Dynamic.ExpandoObject();
                            chbox.Rules = item.Model;
                            chbox.Actual = Unit.ConvertToControl(item.ActualTotalCHIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
                            chbox.Max = Unit.ConvertToControl(item.MaxTotalCHIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
                            chBoxs.Add(chbox);
                        }
                        else
                        {
                            var actual = Unit.ConvertToControl(item.ActualTotalCHIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0");
                            if (Convert.ToDouble(chbox.Actual) < Convert.ToDouble(actual))
                            {
                                chbox.Actual = actual;
                            }
                        }
                    }
                });

                if (chBoxs.Count > 0)
                {
                    row = row + chBoxs.Count + 1;
                  //  dgvPipingRules.Rows.Add(chBoxs.Count + 4);
                }
            }
        }

        
        

        /// <summary>
        ///   指定列不可编剧及列宽
        /// </summary>
        private void InitializeDataGridView_ColumnsWidth()
        {
            dgvPipingRules.Columns[0].Width = 80;
            dgvPipingRules.Columns[1].Width = 443;
            dgvPipingRules.Columns[2].Width = 90;
            dgvPipingRules.Columns[3].Width = 90;
            dgvPipingRules.Columns[4].Width = 80;
            for (int i = 0; i < this.dgvPipingRules.Columns.Count; i++)
            {
                this.dgvPipingRules.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                if (i == 1)
                    dgvPipingRules.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;  
            }
        }

         /// <summary>
        /// 重汇标题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvPipingRules_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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
            #region
            Brush gridBrush = new SolidBrush(this.dgvPipingRules.GridColor);
            SolidBrush backBrush = new SolidBrush(e.CellStyle.BackColor);
            SolidBrush fontBrush = new SolidBrush(e.CellStyle.ForeColor);
            int cellheight;
            int fontheight;
            int cellwidth;
            int fontwidth;
            int countU = 0;
            int countD = 0;
            int count = 0;
            // 对第1列相同单元格进行合并
            if (this.dgvPipingRules.Columns["IndexNumber"].Index == 0 && e.RowIndex != -1 && e.RowIndex <= row - 1)
            {
                cellheight = e.CellBounds.Height;
                fontheight = (int)e.Graphics.MeasureString(e.Value.ToString(), e.CellStyle.Font).Height;
                cellwidth = e.CellBounds.Width;
                fontwidth = (int)e.Graphics.MeasureString(e.Value.ToString(), e.CellStyle.Font).Width;
                Pen gridLinePen = new Pen(gridBrush);
                string curValue = e.Value == null ? "" : e.Value.ToString().Trim();
                string curSelected = this.dgvPipingRules.Rows[e.RowIndex].Cells[0].Value == null ? "" : this.dgvPipingRules.Rows[e.RowIndex].Cells[0].Value.ToString().Trim();
                if (!string.IsNullOrEmpty(curValue))
                {
                    for (int i = e.RowIndex; i < this.dgvPipingRules.Rows.Count; i++)
                    {
                        if (this.dgvPipingRules.Rows[i].Cells[0].Value != null && this.dgvPipingRules.Rows[i].Cells[0].Value.ToString().Equals(curValue))
                        {
                            this.dgvPipingRules.Rows[i].Cells[0].Selected = this.dgvPipingRules.Rows[e.RowIndex].Selected;
                            this.dgvPipingRules.Rows[i].Selected = this.dgvPipingRules.Rows[e.RowIndex].Selected;
                            countD++;
                        }
                        else
                            break;
                    }
                    for (int i = e.RowIndex; i >= 0; i--)
                    {
                        if (this.dgvPipingRules.Rows[i].Cells[0].Value != null && this.dgvPipingRules.Rows[i].Cells[0].Value.ToString().Equals(curValue))
                        {
                            this.dgvPipingRules.Rows[i].Cells[0].Selected = this.dgvPipingRules.Rows[e.RowIndex].Selected;
                            this.dgvPipingRules.Rows[i].Selected = this.dgvPipingRules.Rows[e.RowIndex].Selected;
                            countU++;
                        }
                        else
                            break;
                    }
                    count = countD + countU - 1;
                    if (count < 2) { return; }
                }
                if (this.dgvPipingRules.Rows[e.RowIndex].Selected)
                {
                    backBrush.Color = e.CellStyle.SelectionBackColor;
                    fontBrush.Color = e.CellStyle.SelectionForeColor;
                }
                e.Graphics.FillRectangle(backBrush, e.CellBounds);
                e.Graphics.DrawString((String)e.Value, e.CellStyle.Font, fontBrush, e.CellBounds.X + (cellwidth - fontwidth) / 2, e.CellBounds.Y - cellheight * (countU - 1) + (cellheight * count - fontheight) / 2);
                if (countD == 1)
                {
                    e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                    count = 0;
                }
                // 画右边线
                e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                e.Handled = true;
            }
            #endregion 纵向合并

            if (e.RowIndex == row)
            {
                using (
                   Brush gridBrush1 = new SolidBrush(this.dgvPipingRules.GridColor),
                   backColorBrush = new SolidBrush(e.CellStyle.BackColor))
                {
                    using (Pen gridLinePen = new Pen(gridBrush1))
                    {
                        // 擦除原单元格背景
                        e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left, e.CellBounds.Bottom - 1,
                                                   e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Handled = true;
                    }
                }
            }


            #region
            if (this.dgvPipingRules.Columns["IndexNumber"].Index == e.ColumnIndex && e.RowIndex > row)
            {

                using (
                    Brush gridBrush1 = new SolidBrush(this.dgvPipingRules.GridColor),
                    backColorBrush = new SolidBrush(e.CellStyle.BackColor))
                {
                    using (Pen gridLinePen = new Pen(gridBrush1))
                    {
                        // 擦除原单元格背景
                        e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

                        if (e.Value.ToString() != this.dgvPipingRules.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value.ToString())
                        {

                            //右侧的线
                            e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1, e.CellBounds.Top,
                                e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                            //绘制值
                            if (e.Value != null)
                            {
                                e.Graphics.DrawString((String)e.Value, e.CellStyle.Font,
                                    Brushes.Crimson, e.CellBounds.X + 2,
                                    e.CellBounds.Y + 2, StringFormat.GenericDefault);
                            }
                        }

                        //下边缘的线
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left, e.CellBounds.Bottom - 1,
                                                    e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Handled = true;
                    }
                }

            }
            #endregion 横向合并 

        }

        private void dgvPipingRules_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            dgv.ClearSelection();
        }
       
         
    }
}
