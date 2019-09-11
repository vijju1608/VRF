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

namespace JCHVRF_New.Utility
{
    public class AutoPiping
    {
        public JCHVRF.Model.Project projectLegacy { get; set; }
        private NextGenBLL.PipingBLL GetPipingBLLInstance()
        {
            projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;
            bool isInch = CommonBLL.IsDimension_inch();
            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            return new NextGenBLL.PipingBLL(projectLegacy, utilPiping, null, isInch, ut_weight, ut_length, ut_power);
        }
        private string GetImagePathPiping()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\TypeImages\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        private void InitAndRemovePipingNodes(ref WL.AddFlow AddFlowAutoPiping)
        {
            var Nodes = Enumerable.OfType<WL.Node>(AddFlowAutoPiping.Items).ToList();
            var Captions = Enumerable.OfType<WL.Caption>(AddFlowAutoPiping.Items).ToList();
            var Links = Enumerable.OfType<WL.Link>(AddFlowAutoPiping.Items).ToList();
            foreach (var item in Nodes)
            {
                AddFlowAutoPiping.RemoveNode(item);
            }
            foreach (var item in Captions)
            {
                AddFlowAutoPiping.RemoveCaption(item);
            }
            foreach (var item in Links)
            {
                AddFlowAutoPiping.RemoveLink(item);
            }
        }
        public WL.AddFlow DoDrawingPiping(bool reset, JCHVRF.Model.NextGen.SystemVRF CurrentSystem, WL.AddFlow AddFlowAutoPiping)
        {
            try
            {
                AddFlowAutoPiping.Clear();
                if (CurrentSystem.MyPipingNodeOut.AddFlow != null)
                {
                    AddFlowAutoPiping = CurrentSystem.MyPipingNodeOut.AddFlow;
                }
                NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();

                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                //pipBll.SaveAllPipingStructure();
                //pipBll.CreatePipingNodeStructure(CurrentSystem);
                bool isHitachi = projectLegacy.BrandCode == "H";
                bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(CurrentSystem);
                //string dir = GetBinDirectoryPath(ConfigurationManager.AppSettings["PipingNodeImageDirectory"].ToString());
                //TO DO Pick VRF system in case of multi system
                string dir = GetImagePathPiping();
                NextGenModel.MyNodeOut pipingNodeOut = CurrentSystem.MyPipingNodeOut;
                if (pipingNodeOut == null || CurrentSystem.OutdoorItem == null)
                {
                    return AddFlowAutoPiping;
                }
                if (pipingNodeOut.ChildNode == null)
                {
                    return AddFlowAutoPiping;
                }
                if (isHR)
                {
                    //SetAllNodesIsCoolingonlyFrom();
                    pipBll.SetIsCoolingOnly(CurrentSystem.MyPipingNodeOut);
                }
                if (!reset)
                {
                    utilPiping.ResetColors();
                    InitAndRemovePipingNodes(ref AddFlowAutoPiping);
                    pipBll.DrawPipingNodes(CurrentSystem, dir, ref AddFlowAutoPiping);
                    pipBll.DrawPipingLinks(CurrentSystem, ref AddFlowAutoPiping);
                    pipBll.DrawLegendText(CurrentSystem, ref AddFlowAutoPiping);
                    pipBll.LoadPipingPlottingScaleNode(CurrentSystem, ref AddFlowAutoPiping);
                    CurrentSystem.MyPipingOrphanNodes = null;
                    CurrentSystem.MyPipingOrphanNodesTemp = null;
                }
                if (reset)
                {
                    CurrentSystem.IsManualPiping = false;
                    utilPiping.ResetColors();
                    InitAndRemovePipingNodes(ref AddFlowAutoPiping);
                    pipBll.DrawPipingNodes(CurrentSystem, dir, ref AddFlowAutoPiping);
                    pipBll.DrawPipingLinks(CurrentSystem, ref AddFlowAutoPiping);
                    pipBll.DrawLegendText(CurrentSystem, ref AddFlowAutoPiping);
                    pipBll.LoadPipingPlottingScaleNode(CurrentSystem, ref AddFlowAutoPiping);
                    CurrentSystem.MyPipingOrphanNodes = null;
                    CurrentSystem.MyPipingOrphanNodesTemp = null;
                }
                else
                {
                    if (CurrentSystem.IsManualPiping)
                    {
                    }
                    else
                    {
                        utilPiping.ResetColors();
                    }
                    pipBll.DrawPipingNodesNoCaculation(dir, CurrentSystem);
                }
                //added for internal Bug Find zero length Issue
                pipBll.DrawCorrectionFactorText(CurrentSystem);
                if (CurrentSystem.IsPipingOK)
                {

                    if (CurrentSystem.IsInputLengthManually && CurrentSystem.IsPipingOK)
                    {
                        pipBll.DrawAddRefrigerationText(CurrentSystem);
                    }

                    pipBll.SetDefaultColor(ref AddFlowAutoPiping, isHR);
                }
                pipBll.drawPipelegend(isHR, ref AddFlowAutoPiping);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex,false);
            }
            return AddFlowAutoPiping;
        }

        public void UpdateAllPipingNodeStructure()
        {
            try
            {
                if (JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen != null)
                {
                    foreach (var sysvrf in JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen)
                    {
                        sysvrf.MyPipingNodeOut = null;
                        NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                        pipBll.LoadPipingNodeStructure(sysvrf);
                        //pipBll.CreatePipingNodeStructure(sysvrf);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

    }
}
