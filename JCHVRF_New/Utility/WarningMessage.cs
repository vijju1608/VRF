using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using NextGenModel = JCHVRF.Model.NextGen;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCHVRF_New.Views;
using JCHVRF.VRFMessage;
using JCBase.Utility;
using JCHVRF.MyPipingBLL;
using JCHVRF_New.Common.Helpers;

namespace JCHVRF_New.Utility
{
   
   public class WarningMessage
    {
        private IEventAggregator _eventAggregator;
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;
        string ut_weight;

        public string ShowWarningMsg(NextGenBLL.PipingErrors errorType, NextGenModel.SystemVRF currentSystem) //Shweta: updated to get the msg string as per errorType
        {
            double len = 0;
            int count = 0;
            string rate = "";
            string msg = "";
            string templen = "";
            string temphei = "";            
            switch (errorType)
            {
                case NextGenBLL.PipingErrors.LINK_LENGTH://-1:
                    msg = Msg.PIPING_LINK_LENGTH;
                    break;
                case NextGenBLL.PipingErrors.WARN_ACTLENGTH://-2:
                    len = Unit.ConvertToControl(currentSystem.MaxPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_WARN_ACTLENGTH(currentSystem.MaxPipeLength.ToString("n0"), len.ToString("n0") + ut_length);

                    break;
                case NextGenBLL.PipingErrors.EQLENGTH://-3:
                    len = Unit.ConvertToControl(currentSystem.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_EQLENGTH(currentSystem.MaxEqPipeLength.ToString("n0") ,len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.FIRSTLENGTH://-4:
                    len = Unit.ConvertToControl(currentSystem.MaxIndoorLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_FIRSTLENGTH(currentSystem.TotalPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.LENGTHFACTOR://-5:
                    len = Unit.ConvertToControl(currentSystem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(currentSystem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(currentSystem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.TOTALLIQUIDLENGTH://-6:
                    len = Unit.ConvertToControl(currentSystem.MaxTotalPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_TOTALLIQUIDLENGTH(currentSystem.MaxTotalPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MKTOINDOORLENGTH://-7:
                    len = Unit.ConvertToControl(currentSystem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH(currentSystem.ActualMaxMKIndoorPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MKTOINDOORLENGTH1://-8:
                    len = Unit.ConvertToControl(PipingBLL.MaxCHToIndoorTotalLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH("0",len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAINBRANCHCOUNT://-9:
                    count = PipingBLL.MaxMainBranchCount;
                    msg = Msg.PIPING_MAINBRANCHCOUNT(count.ToString());
                    break;
                case NextGenBLL.PipingErrors.COOLINGCAPACITYRATE://-10:
                    rate = PipingBLL.MinMainBranchCoolingCapacityRate;
                    msg = Msg.PIPING_COOLINGCAPACITYRATE(rate);
                    break;
                //case -11:
                //    rate = PipingBLL.MinMainBranchHeatingCapacityRate;
                //    msg = Msg.PIPING_HEATINGCAPACITYRATE(rate);
                //    break;
                case NextGenBLL.PipingErrors.COOLINGONLYCAPACITY://-12:
                    msg = Msg.PIPING_COOLINGONLYCAPACITY();
                    break;
                case NextGenBLL.PipingErrors.INDOORNUMBERTOCH://-13:
                    count = PipingBLL.MaxIndoorNumberConnectToCH;
                    msg = Msg.PIPING_INDOORNUMBERTOCH(count.ToString());
                    break;
                // 多台室外机组成机组时，校验第一分歧管到第一Piping Connection kit之间的管长不能小于0.5m add on 20170720 by Shen Junjie
                case NextGenBLL.PipingErrors.FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH://-14:
                    double betweenConnectionKits_Min = Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length);
                    string betweenConnectionKits_Msg = betweenConnectionKits_Min.ToString("n2") + ut_length;
                    msg = Msg.PIPING_FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH(betweenConnectionKits_Msg);
                    break;
                case NextGenBLL.PipingErrors._3RD_MAIN_BRANCH://-15:
                    //不能有第三层主分支。
                    msg = Msg.GetResourceString("ERROR_PIPING_3RD_MAIN_BRANCH");
                    break;
                case NextGenBLL.PipingErrors._4TH_BRANCH_NOT_MAIN_BRANCH://-16:
                    //第4(或更远的)分支不能是一个主分支。
                    msg = Msg.GetResourceString("ERROR_PIPING_4TH_BRANCH_NOT_MAIN_BRANCH");
                    break;
                case NextGenBLL.PipingErrors.DIFF_LEN_FURTHEST_CLOSESST_IU: //-17
                    msg = Msg.GetResourceString("ERROR_PIPING_DIFF_LEN_FURTHEST_CLOSESST_IU");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_BRANCH_KIT://-18
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_BRANCH_KIT");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_CHBOX://-19
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_CHBOX");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_MULTI_CHBOX: //-20
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_MULTI_CHBOX");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_SIZE_UP_IU: //-21
                    msg = Msg.WARNING_DATA_EXCEED;
                    break;
                case NextGenBLL.PipingErrors.MAX_HIGHDIFF_UPPER://-22:
                    len = Unit.ConvertToControl(currentSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_HIGHDIFF_LOWER://-23:
                    len = Unit.ConvertToControl(currentSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_HeightDiffL(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_HIGHDIFF_INDOOR://-24:
                    len = Unit.ConvertToControl(currentSystem.MaxDiffIndoorHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_Indoor_HeightDiff(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_CHBOXHIGHDIFF://-25:
                    len = Unit.ConvertToControl(currentSystem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_MULTICHBOXHIGHDIFF://-26:
                    len = Unit.ConvertToControl(currentSystem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_CHBOX_INDOORHIGHDIFF://-27:
                    len = Unit.ConvertToControl(currentSystem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.INDOORLENGTH_HIGHDIFF://-28
                    templen = Unit.ConvertToControl(PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    msg = Msg.INDOORLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case NextGenBLL.PipingErrors.CHBOXLENGTH_HIGHDIFF://-29
                    templen = Unit.ConvertToControl(PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    msg = Msg.CHBOXLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case NextGenBLL.PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF://-30
                    templen = Unit.ConvertToControl(PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length).ToString("n2") + ut_length;
                    msg = Msg.CHBOX_INDOORLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case NextGenBLL.PipingErrors.PIPING_CHTOINDOORTOTALLENGTH: //-31
                    len = Unit.ConvertToControl(40, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_CHTOINDOORTOTALLENGTH(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.PIPING_LENGTH_HEIGHT_DIFF: //-32
                    len = Unit.ConvertToControl(currentSystem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diffs = Unit.ConvertToControl(currentSystem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(currentSystem.Name, len.ToString("n2") + ut_length, Math.Abs(diffs).ToString("n2") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MIN_DISTANCE_BETWEEN_MULTI_KITS://-33
                    msg = Msg.PIPING_MIN_LEN_BETWEEN_MULTI_KITS(Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length).ToString() + ut_length);
                    break;
                case NextGenBLL.PipingErrors.ODU_PIPE_LENGTH_LIMITS://-34
                    msg = Msg.GetResourceString("PIPING_ODU_PIPE_LENGTH_LIMITS");
                    break;
                case NextGenBLL.PipingErrors.ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX: //-35                    
                    msg = Msg.GetResourceString("ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX");
                    break;
                case NextGenBLL.PipingErrors.OK: //0
                default:
                    msg = "";
                  
                    break;
            }
            //ShowWarningMsg(msg);
            return msg;
        }
    }
}
