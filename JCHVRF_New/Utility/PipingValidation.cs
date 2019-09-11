using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WL = Lassalle.WPF.Flow;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using NextGenModel = JCHVRF.Model.NextGen;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCHVRF_New.Views;

namespace JCHVRF_New.Utility
{
    public class PipingValidation
    {
        NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
        NextGenBLL.PipingBLL pipBll = null;
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;
        string ut_weight;
        public JCHVRF.Model.Project projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
        AutoPiping autoPipingObj = new AutoPiping();
        WarningMessage WarningMessageObj = new WarningMessage();

        private NextGenBLL.PipingBLL GetPipingBLLInstanceValidation(WL.AddFlow AddFlowAutoPiping)
        {
            bool isInch = CommonBLL.IsDimension_inch();
            return new NextGenBLL.PipingBLL(this.projectLegacy, utilPiping, AddFlowAutoPiping, isInch, ut_weight, ut_length, ut_power);
        }
        private void DoPipingCalculation(NextGenBLL.PipingBLL pipBll, JCHVRF.Model.NextGen.MyNodeOut nodeOut, JCHVRF.Model.NextGen.SystemVRF currentSystem, out NextGenBLL.PipingErrors errorType)
        {
            errorType = NextGenBLL.PipingErrors.OK;
            if (nodeOut.ChildNode == null)
            {
                return;
            }
            pipBll.GetSumCapacity(nodeOut.ChildNode);
            pipBll.IsBranchKitNeedSizeUp(currentSystem);
            PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = nodeOut.ChildNode as JCHVRF.Model.NextGen.MyNodeYP;
                if (nodeYP.IsCP)
                {
                    firstBranchKit = pipBll.getFirstHeaderBranchPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                else
                {
                    firstBranchKit = pipBll.getFirstPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                if (errorType != NextGenBLL.PipingErrors.OK)
                {
                    SetSystemPipingOK(currentSystem, false);
                    return;
                }
            }
            //bug 3489
            var L2SizeDownRule = pipBll.GetL2SizeDownRule(currentSystem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            //bug 3489
            pipBll.getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, currentSystem, false, out errorType, L2SizeDownRule);
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);
                return;
            }
            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut);
        }
        private void SetSystemPipingOK(JCHVRF.Model.NextGen.SystemVRF CurrentSystem, bool isPipingOK)
        {
            if (CurrentSystem.IsPipingOK != isPipingOK)
            {
                CurrentSystem.IsPipingOK = isPipingOK;
            }
        }
        public WL.AddFlow Validate(JCHVRF.Model.NextGen.SystemVRF CurrentSystem, WL.AddFlow AddFlowAutoPiping)
        {
            if (CurrentSystem == null)
            {
                return AddFlowAutoPiping;
            }
            utilPiping = new NextGenBLL.UtilPiping();
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            AddFlowAutoPiping=DoPipingFinalVerification(CurrentSystem, AddFlowAutoPiping);
            return AddFlowAutoPiping;
        }
        private WL.AddFlow DoPipingFinalVerification(JCHVRF.Model.NextGen.SystemVRF currentSystem, WL.AddFlow AddFlowAutoPiping)
        {
          
            NextGenBLL.PipingErrors errorType = NextGenBLL.PipingErrors.OK;
            if (currentSystem.OutdoorItem == null)
            {
                return AddFlowAutoPiping;
            }
            if (currentSystem.IsManualPiping && currentSystem.IsUpdated)
            {

                return AddFlowAutoPiping;
            }
            //this.Cursor = Cursors.WaitCursor;
            UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref AddFlowAutoPiping);
            JCHVRF.MyPipingBLL.NextGen.PipingBLL pipBll = GetPipingBLLInstanceValidation(AddFlowAutoPiping);
            bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(currentSystem);
            pipBll.SetPipingLimitation(currentSystem);

            errorType = pipBll.ValidateSystemHighDifference(currentSystem);

            if (errorType == NextGenBLL.PipingErrors.OK)
            {
                errorType = pipBll.ValidatePipeLength(currentSystem, ref AddFlowAutoPiping);
            }
            #region
            if (errorType == NextGenBLL.PipingErrors.OK)
            {
                if (!currentSystem.IsManualPiping)
                AddFlowAutoPiping =autoPipingObj.DoDrawingPiping(true, currentSystem, AddFlowAutoPiping);
                if (!currentSystem.IsPipingOK)
                {
                    pipBll.SetDefaultColor(ref AddFlowAutoPiping, isHR);
                }
                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    if (currentSystem.PipeEquivalentLength < currentSystem.HeightDiff)
                    {
                        errorType = NextGenBLL.PipingErrors.PIPING_LENGTH_HEIGHT_DIFF; //-32;
                    }
                }

                if (currentSystem.IsInputLengthManually)
                {

                    errorType = pipBll.ValMainBranch(currentSystem, ref AddFlowAutoPiping);
                }
                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    if (NextGenBLL.PipingBLL.IsHeatRecovery(currentSystem) && !pipBll.ValCoolingOnlyIndoorCapacityRate(currentSystem, ref AddFlowAutoPiping))
                    {
                        errorType = NextGenBLL.PipingErrors.COOLINGONLYCAPACITY; //-12;
                    }
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    errorType = pipBll.ValidateIDUOfMultiCHBox(currentSystem);
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                   // SetSystemPipingOK(currentSystem, true);
                    DoPipingCalculation(pipBll, currentSystem.MyPipingNodeOut, currentSystem, out errorType);
                    if (currentSystem.IsPipingOK)
                    {
                        if (currentSystem.IsInputLengthManually && !pipBll.ValCHToIndoorMaxTotalLength(currentSystem, ref AddFlowAutoPiping))
                        {
                            errorType = NextGenBLL.PipingErrors.MKTOINDOORLENGTH1; //-8;
                        }
                        else if (!pipBll.ValMaxIndoorNumberConnectToCH(currentSystem, ref AddFlowAutoPiping))
                        {
                            errorType = NextGenBLL.PipingErrors.INDOORNUMBERTOCH; //-13;
                        }
                        else
                        {
                            //SetSystemPipingOK(currentSystem, true);

                            if (currentSystem.IsInputLengthManually)
                            {
                                double d1 = pipBll.GetAddRefrigeration(currentSystem, ref AddFlowAutoPiping);
                                currentSystem.AddRefrigeration = d1;

                                pipBll.DrawAddRefrigerationText(currentSystem);
                            }
                            else
                            {
                                currentSystem.AddRefrigeration = 0;
                            }
                        }
                    }
                    ObjPipValidation.DrawTextToAllNodes(currentSystem.MyPipingNodeOut, null, currentSystem);
                    UtilTrace.SaveHistoryTraces();
                }
            }
            #endregion
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);

                ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, WarningMessageObj.ShowWarningMsg(errorType, currentSystem));
            }
            WarningMessageObj.ShowWarningMsg(errorType, currentSystem);
            return AddFlowAutoPiping;
        }
    }
}
