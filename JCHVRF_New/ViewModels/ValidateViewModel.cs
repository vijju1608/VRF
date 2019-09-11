using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF.MyPipingBLL.NextGen;
using JCHVRF.Model.NextGen;
using JCHVRF.BLL;
using System.Drawing;
using JCHVRF.VRFMessage;
using JCBase.Utility;
using JCBase.UI;
using JCHVRF.Model;
using Lassalle.WPF.Flow;

namespace JCHVRF_New.ViewModels
{
    public class ValidateViewModel
    {
        JCHVRF.Model.NextGen.SystemVRF curSystemItem;
        private Lassalle.WPF.Flow.AddFlow addFlowPiping;
        JCHVRF.Model.Project ProjectLegacy { get; set; }

        UtilPiping utilPiping;
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;
        string ut_weight;


        public ValidateViewModel(JCHVRF.Model.Project project,ref AddFlow addFlow)
        {
            this.ProjectLegacy = project;
            this.addFlowPiping = addFlow;
            if (ProjectLegacy.SystemListNextGen[0] == null)
                return;
            if (curSystemItem == null && ProjectLegacy.SystemListNextGen[0] != null)
            {
                if (ProjectLegacy.SystemListNextGen.Count == 0)
                    return;
                curSystemItem = ProjectLegacy.SystemListNextGen[0];
            }

            utilPiping = new UtilPiping();            
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;

            DoPipingFinalVerification();
            addFlow = this.addFlowPiping;
        }


        private PipingBLL GetPipingBLLInstance()
        {
            UtilPiping utilPiping = new UtilPiping();
            bool isInch = CommonBLL.IsDimension_inch();
            return new PipingBLL(this.ProjectLegacy, utilPiping, this.addFlowPiping, isInch, ut_weight, ut_length, ut_power);
        }

        private void DoPipingCalculation(PipingBLL pipBll, JCHVRF.Model.NextGen.MyNodeOut nodeOut, out PipingErrors errorType)
        {
            errorType = PipingErrors.OK;
            if (nodeOut.ChildNode == null) return;
            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            //getSumCalculation(ref firstDstNode, factoryCode, type, unitType);

            pipBll.GetSumCapacity(nodeOut.ChildNode);

            pipBll.IsBranchKitNeedSizeUp(curSystemItem);

            PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = nodeOut.ChildNode as JCHVRF.Model.NextGen.MyNodeYP;
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
            pipBll.getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, curSystemItem, false, out errorType);
            if (errorType != PipingErrors.OK)
            {
                SetSystemPipingOK(curSystemItem, false);
                return;
            }

            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut);
        }

        private void SetSystemPipingOK(JCHVRF.Model.NextGen.SystemVRF sysItem, bool isPipingOK)
        {
            if (sysItem.IsPipingOK != isPipingOK)
            {
                sysItem.IsPipingOK = isPipingOK;
                //SetTabControlImageKey();
            }
        }
       
        private void DoPipingFinalVerification()
        {
            PipingErrors errorType = PipingErrors.OK;
            if (curSystemItem.OutdoorItem == null)
                return;

            if (curSystemItem.IsManualPiping && curSystemItem.IsUpdated)
            {              

                return;
            }
            //this.Cursor = Cursors.WaitCursor;
            JCHVRF.MyPipingBLL.NextGen.PipingBLL pipBll = GetPipingBLLInstance();
            bool isHR = PipingBLL.IsHeatRecovery(curSystemItem);
            pipBll.SetPipingLimitation(curSystemItem);

            errorType = pipBll.ValidateSystemHighDifference(curSystemItem);

            if (errorType == PipingErrors.OK)
            {
                errorType = pipBll.ValidatePipeLength(curSystemItem,ref addFlowPiping);
            }
            #region
            if (errorType == PipingErrors.OK)
            {
                //如果排管未完成或存在错误，每次点击验证按钮都需要将上次标红的对象恢复。add on 20160727 by Yunxiao Lin
                if (!curSystemItem.IsPipingOK)
                {
                    pipBll.SetDefaultColor(ref addFlowPiping, isHR);
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
                    
                    errorType = pipBll.ValMainBranch(curSystemItem,ref addFlowPiping);
                }
                if (errorType == PipingErrors.OK)
                {
                    //检验Heat Recovery系统内Cooling Only内机容量是否超过全部室内机容量的50% -12
                    //string HeatType = curSystemItem.OutdoorItem.ModelFull.Substring(3, 1);
                    //string HeatType = curSystemItem.OutdoorItem.ProductType.Contains("Heat Recovery") || curSystemItem.OutdoorItem.ProductType.Contains(", HR") ? "R":"H";
                    if (PipingBLL.IsHeatRecovery(curSystemItem) && !pipBll.ValCoolingOnlyIndoorCapacityRate(curSystemItem, ref addFlowPiping))
                    {
                        errorType = PipingErrors.COOLINGONLYCAPACITY; //-12;
                    }
                }

                if (errorType == PipingErrors.OK)
                {
                    //ANZ 每个multiple CH-Box最多只能连接2个8HP/10HP的IDU
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
                        if (curSystemItem.IsInputLengthManually && !pipBll.ValCHToIndoorMaxTotalLength(curSystemItem,ref addFlowPiping))
                        {
                            errorType = PipingErrors.MKTOINDOORLENGTH1; //-8;
                        }
                        //检验CH-Box连接的室内机数量 -13
                        else if (!pipBll.ValMaxIndoorNumberConnectToCH(curSystemItem,ref addFlowPiping))
                        {
                            errorType = PipingErrors.INDOORNUMBERTOCH; //-13;
                        }
                        else
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
                    //UtilTrace.SaveHistoryTraces();//保存历史痕迹 add by axj 20161228
                }
            }
            #endregion
            if (errorType != PipingErrors.OK)
            {
                SetSystemPipingOK(curSystemItem, false);
            }
            ShowWarningMsg(errorType);
            //UtilTrace.SaveHistoryTraces();//保存历史痕迹 add by axj 20161228

            //SetTabControlImageKey();
            //SetTreeViewOutdoorState();
            //this.Cursor = Cursors.Default;
        }

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
                    msg = Msg.PIPING_WARN_ACTLENGTH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.EQLENGTH://-3:
                    len = Unit.ConvertToControl(curSystemItem.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_EQLENGTH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.FIRSTLENGTH://-4:
                    len = Unit.ConvertToControl(curSystemItem.MaxIndoorLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_FIRSTLENGTH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.LENGTHFACTOR://-5:
                    len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                    break;
                case PipingErrors.TOTALLIQUIDLENGTH://-6:
                    len = Unit.ConvertToControl(curSystemItem.MaxTotalPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_TOTALLIQUIDLENGTH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MKTOINDOORLENGTH://-7:
                    len = Unit.ConvertToControl(curSystemItem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH(len.ToString("n0") + ut_length);
                    break;
                case PipingErrors.MKTOINDOORLENGTH1://-8:
                    len = Unit.ConvertToControl(PipingBLL.MaxCHToIndoorTotalLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH(len.ToString("n0") + ut_length);
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
                    msg = Msg.INDOORLENGTH_HIGHDIFF_MSG(templen, temphei);
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
                    len = Unit.ConvertToControl(40, UnitType.LENGTH_M, ut_length);
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
                case PipingErrors.ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX: //-35
                    //每个multiple CH-Box最多只能连接2个8HP/10HP的IDU。仅限ANZ。 add by Shen Junjie on 2018/8/17
                    msg = Msg.GetResourceString("ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX");
                    break;
                case PipingErrors.OK: //0
                default:
                    msg = "Verirfied";   //should be empty later
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
            //tlblStatus.Text = msg;
            //tlblStatus.ForeColor = msgColor;

            JCMsg.ShowInfoOK(msg);
        }

    }
}
