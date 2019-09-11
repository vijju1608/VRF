using JCBase.Utility;
using JCHVRF.DAL;
using JCHVRF.Model.NextGen;
using Lassalle.WPF.Flow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Drawing.FontStyle;
using ng = JCHVRF.Model.NextGen;
using OldModel = JCHVRF.Model;
using Point = System.Windows.Point;

namespace JCHVRF.MyPipingBLL.NextGen
{
    public enum PipingErrors
    {
        OK = 0,
        LINK_LENGTH = -1,
        WARN_ACTLENGTH = -2,
        EQLENGTH = -3,
        FIRSTLENGTH = -4,
        LENGTHFACTOR = -5,
        TOTALLIQUIDLENGTH = -6,
        MKTOINDOORLENGTH = -7,
        MKTOINDOORLENGTH1 = -8,
        MAINBRANCHCOUNT = -9,
        COOLINGCAPACITYRATE = -10,
        COOLINGONLYCAPACITY = -12,
        INDOORNUMBERTOCH = -13,
        FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH = -14,
        _3RD_MAIN_BRANCH = -15,
        _4TH_BRANCH_NOT_MAIN_BRANCH = -16,
        DIFF_LEN_FURTHEST_CLOSESST_IU = -17,
        NO_MATCHED_BRANCH_KIT = -18,
        NO_MATCHED_CHBOX = -19,
        NO_MATCHED_MULTI_CHBOX = -20,
        NO_MATCHED_SIZE_UP_IU = -21,
        MAX_HIGHDIFF_UPPER = -22,  //Height Difference between (O.U. is Upper) add on 20180502 by xyj
        MAX_HIGHDIFF_LOWER = -23,  //Height Difference between (O.U. is Lower)
        MAX_HIGHDIFF_INDOOR = -24,  //Height Difference between OldModel.Indoor units
        MAX_CHBOXHIGHDIFF = -25,        //Height Difference between CH-boxes
        MAX_MULTICHBOXHIGHDIFF = -26,   //Height Difference between OldModel.Indoor Units using the Same Branch of CH-Box
        MAX_CHBOX_INDOORHIGHDIFF = -27, //Height Difference between CH-Box and OldModel.Indoor Units
        INDOORLENGTH_HIGHDIFF = -28,
        CHBOXLENGTH_HIGHDIFF = -29,
        CHBOX_INDOORLENGTH_HIGHDIFF = -30,
        PIPING_CHTOINDOORTOTALLENGTH = -31,
        PIPING_LENGTH_HEIGHT_DIFF = -32,
        MIN_DISTANCE_BETWEEN_MULTI_KITS = -33,  //minimum distances between multikits is 0.5 meters
        ODU_PIPE_LENGTH_LIMITS = -34,
        ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX = -35,
        WARN_ACTLENGTH_FA = -38,
    }

    public class PipingBLL
    {
        public Font textFont_piping = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Pixel);
        private static Dictionary<string, Image> BuildingImageCache = new Dictionary<string, Image>();
        public static string PipingImageDir = "";
        public static string WiringImageDir = "";
        OldModel.Project thisProject;
        PipingDAL _dal;
        UtilEMF utilEMF = new UtilEMF();
        UtilPiping utilPiping = new UtilPiping();
        static float legendLeft = 0;
        const float legendLeftTopPosition = 250;
        private int linkOrder = 0;
        private AddFlow addFlowPiping = null;
        private bool isInch = false;
        private bool isHitachi = false;
        private string ut_weight = "";
        private string ut_length = "";
        private string ut_power = "";
        private string ut_pipeSize = "";


        //public PipingBLL()
        //{
        //    _dal = new PipingDAL(thisProject);
        //}

        //public PipingBLL(OldModel.Project thisProj)
        //    : this()
        //{
        //    thisProject = thisProj;
        //    isHitachi = thisProject.BrandCode == "H";
        //}
        Graphics gMeasureString = null;
        Bitmap bmpMeasureString = null;
        public string CapacityUnits
        {
            get
            {
                return OldModel.SystemSetting.UserSetting.unitsSetting.settingPOWER;
            }
        }

        public PipingBLL(OldModel.Project thisProj)
        {
            thisProject = thisProj;
            isHitachi = thisProject.BrandCode == "H";
            _dal = new PipingDAL(thisProj);
            bmpMeasureString = new Bitmap(500, 100);
            gMeasureString = Graphics.FromImage(bmpMeasureString);
        }

        /// <summary>        
        /// </summary>
        /// <param name="thisProj"></param>
        /// <param name="addFlowPiping"></param>
        /// <param name="isInch"></param>
        public PipingBLL(OldModel.Project thisProj,
            UtilPiping utilPiping,
            AddFlow addFlowPiping,
            bool isInch,
            string ut_weight,
            string ut_length,
            string ut_power)
            : this(thisProj)
        {
            this.utilPiping = utilPiping;
            this.addFlowPiping = addFlowPiping;
            this.isInch = isInch;
            this.ut_weight = ut_weight;
            this.ut_length = ut_length;
            this.ut_power = ut_power;
            this.ut_pipeSize = isInch ? "\"" : Unit.ut_Dimension_mm;
        }

        public static bool IsHeatRecovery(SystemVRF sysItem)
        {
            return sysItem != null && sysItem.OutdoorItem != null && IsHeatRecovery(sysItem.OutdoorItem.ProductType);
        }

        public static bool IsHeatRecovery(string productType)
        {
            return !string.IsNullOrEmpty(productType) &&
                (productType.Contains("Heat Recovery") || productType.Contains(", HR"));
        }

        public static string GetFactoryCode(SystemVRF sysItem)
        {
            if (sysItem == null || sysItem.OutdoorItem == null) return "";
            return GetFactoryCode(sysItem.OutdoorItem.ModelFull);
        }

        public static string GetFactoryCode(string modelFull)
        {
            if (modelFull == null) return "";
            modelFull = modelFull.Trim();
            if (modelFull == "") return "";
            return modelFull.Substring(modelFull.Length - 1, 1);
        }

        #region Piping

        public OldModel.PipingBranchKit GetConnectionKit(OldModel.Outdoor outItem)
        {
            return _dal.GetConnectionKit(outItem);
        }

        public OldModel.PipingBranchKit GetFirstBranchKit(string factoryCode, string type, string unitType, double capacity, string sizeUP, bool isMal, int IDUNum, string region)
        {
            return _dal.GetFirstBranchKit(factoryCode, type, unitType, capacity, sizeUP, isMal, IDUNum, region);
        }

        public OldModel.PipingBranchKit GetTertiaryBranchKit(string factoryCode, string type, string unitType, double capacity, string sizeUP, bool isMal, int IDUNum, string region)
        {
            return _dal.GetTertiaryBranchKit(factoryCode, type, unitType, capacity, sizeUP, isMal, IDUNum, region);
        }

        public OldModel.PipingBranchKit ShrinkTertiaryBranchKit(OldModel.PipingBranchKit branchKit, bool isHitachi, bool IsCoolingonly, string sizeUP, bool isMal, int IDUNum, string region)
        {
            return _dal.ShrinkTertiaryBranchKit(branchKit, isHitachi, IsCoolingonly, sizeUP, isMal, IDUNum, region);
        }

        public OldModel.PipingHeaderBranch GetHeaderBranch(string type, double capacity, string sizeUP, int currentBranchCount, string ODUUnitType, string region)
        {
            return _dal.GetHeaderBranch(type, capacity, sizeUP, currentBranchCount, ODUUnitType, region);
        }

        public OldModel.PipingChangeOverKit GetChangeOverKit(string factoryCode, double capacity, string sizeUP, int IUCount, string Series, string region)
        {
            return _dal.GetChangeOverKit(factoryCode, capacity, sizeUP, IUCount, Series, region);
        }

        public OldModel.PipingMultiCHBox GetMultiCHBox(string region, string series, double capacity, bool sizeUP,
            int branchCount, double maxBranchCapacity, int maxBranchIDU)
        {
            return _dal.GetMultiCHBox(region, series, capacity, sizeUP, branchCount, maxBranchCapacity, maxBranchIDU);
        }

        public bool ExistsMultiCHBoxStd(string region, string series)
        {
            return _dal.ExistsMultiCHBox(region, series);
        }

        // add sizeup on 20160516 by Yunxiao Lin
        public string[] GetPipeSizeIDU_SizeUp(string regionCode, string factoryCode, double capacity, string model_Hitachi, string sizeUP)
        {
            return _dal.GetPipeSizeIDU_SizeUP(regionCode, factoryCode, capacity, model_Hitachi, sizeUP);
        }

        public NodeElement_Piping GetPipingNodeOutElement(SystemVRF sysItem, bool isHitachi)
        {
            string pipeSize = "2Pipe";
            string sizeUp = "NA";
            string outModel = sysItem.OutdoorItem.ModelFull;
            string unitType = sysItem.OutdoorItem.Type;
            if ((unitType != null) && unitType.Contains("JTWH (Water source)"))
            {
                if (!outModel.Contains("100"))
                {
                    if (sysItem.PipeEquivalentLength > 80d)
                    {
                        sizeUp = "TRUE";
                    }
                    else
                    {
                        sizeUp = "FALSE";
                    }
                }
            }
            else if (IsHeatRecovery(sysItem))
            {
                pipeSize = "3Pipe";
            }
            return _dal.GetPipingNodeOutElementNextGen(outModel, isHitachi, pipeSize, sizeUp, unitType);
        }

        public NodeElement_Piping GetPipingNodeIndElement(string unitType, bool isHitachi)
        {
            return _dal.GetPipingNodeIndElementNextGen(unitType, isHitachi);
        }

        public string GetPipeSize_Inch(string orgPipeSize)
        {
            return _dal.GetPipeSize_Inch(orgPipeSize);
        }

        public void CheckIndoorNumberConnectedCHBox(MyNodeOut nodeOut)
        {
            bool needSizeUp = false;
            List<string> listPipeSize = _dal.ListPipeSize.Where(p =>
            {
                double num = 0;
                return double.TryParse(p.Trim(), out num) && num != 44.5;
            }).OrderBy(p => double.Parse(p.Trim())).ToList();
            CheckIndoorNumberConnectedCHBox(listPipeSize, nodeOut, needSizeUp);
        }

        private void CheckIndoorNumberConnectedCHBox(List<string> listPipeSize, Node node, bool needSizeUp)
        {
            if (node == null)
            {
                return;
            }

            if (node is MyNodeOut)
            {
                CheckIndoorNumberConnectedCHBox(listPipeSize, (node as MyNodeOut).ChildNode, needSizeUp);
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                if (nodeCH.Model.EndsWith("N2"))
                {
                    int indoorNum = getIndoorCount(node);
                    if (indoorNum > 4)
                    {
                        needSizeUp = true;
                        CheckIndoorNumberConnectedCHBox(listPipeSize, nodeCH.ChildNode, needSizeUp);
                    }
                }
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    CheckIndoorNumberConnectedCHBox(listPipeSize, child, needSizeUp);
                }
            }

            if (needSizeUp)
            {
                SizeUpPipes(node, listPipeSize);
            }
        }

        /// add by Shen Junjie on 2018/7/27
        /// <summary>
        /// 某个节点前的管径增大一号
        /// </summary>
        /// <param name="node"></param>
        /// <param name="listPipeSize"></param>
        private void SizeUpPipes(Node node, List<string> listPipeSize)
        {
            if (node == null || !(node is MyNode)) return;
            List<MyLink> links = (node as MyNode).MyInLinks;

            //增大逻辑是否正确？ 
            foreach (MyLink myLink in links)
            {
                if (myLink != null)
                {
                    int index;
                    if (myLink.SpecL != null && myLink.SpecL != "-" && myLink.SpecL_Normal == myLink.SpecL)
                    {
                        index = listPipeSize.IndexOf(myLink.SpecL.Trim());
                        if (index >= 0 && index < listPipeSize.Count - 1)
                        {
                            myLink.SpecL = listPipeSize[index + 1];
                        }
                    }
                    if (myLink.SpecG_l != null && myLink.SpecG_l != "-" && myLink.SpecG_l_Normal == myLink.SpecG_l)
                    {
                        index = listPipeSize.IndexOf(myLink.SpecG_l.Trim());
                        if (index >= 0 && index < listPipeSize.Count - 1)
                        {
                            myLink.SpecG_l = listPipeSize[index + 1];
                        }
                    }
                    if (myLink.SpecG_h != null && myLink.SpecG_h != "-" && myLink.SpecG_h_Normal == myLink.SpecG_h)
                    {
                        index = listPipeSize.IndexOf(myLink.SpecG_h.Trim());
                        if (index >= 0 && index < listPipeSize.Count - 1)
                        {
                            myLink.SpecG_h = listPipeSize[index + 1];
                        }
                    }
                }
            }
        }

        #endregion


        #region 计算执行 Fitwindow 命令后的缩放比例

        /// 获取AddFlow画布实际尺寸
        /// <summary>
        /// 获取AddFlow画布实际尺寸
        /// </summary>
        /// <param name="addFlowItem"></param>
        /// <returns></returns>
        public System.Windows.Size GetAddFlowSize(AddFlow addFlowItem)
        {
            System.Windows.Size sz = new System.Windows.Size(0, 0);
            foreach (Node node in addFlowItem.Nodes())
            {
                if (node is MyNodeIn)
                {
                    float width = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width) + 10;
                    float height = AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) + 10;
                    sz.Width = (sz.Width < width) ? width : sz.Width;
                    sz.Height = (sz.Height < height) ? height : sz.Height;
                }
            }
            return sz;
        }

        /// 计算执行 Fitwindow 命令后的缩放比例
        /// <summary>
        /// 计算执行 Fitwindow 命令后的缩放比例
        /// </summary>
        /// <param name="addflowItem"></param>
        /// <returns></returns>
        public float GetFitWindowZoom(AddFlow addflowItem)
        {
            System.Windows.Size sz = GetAddFlowSize(addflowItem); // 计算 mx & my
            // 若未撑满画布，则默认scale为100%，否则以实际撑满画布为准
            float f = AddFlowExtension.FloatConverter(addflowItem.Extent.Width) / AddFlowExtension.FloatConverter(sz.Width);
            float f1 = AddFlowExtension.FloatConverter(addflowItem.Extent.Height) / AddFlowExtension.FloatConverter(sz.Height);
            f = f < f1 ? f : f1;
            if (f > 1)
                f = 1; // 是否限制 FitWindow 时的 Zoom值 不超过100%
            return f;
        }

        #endregion


        #region 管长检验

        /// 检验管长 modify on 20160515 by Yunxiao Lin
        /// <summary>
        /// 检验管长
        /// </summary>
        /// <param name="sysItem"></param>
        /// <param name="addFlowItem"></param>
        /// <returns>
        /// 0   -- OK;
        /// -1  -- 管长最小值验证失败；
        /// -2  -- 最长实际管长；
        /// -3  -- 最长等效管长；
        /// -4  -- 第一分歧管到最远端距离；TODO
        /// -5  -- 内外机高度差引起的管长修正系数
        /// -6  -- 全部液管总长
        /// -7  -- 末端分歧管到室内机最大管长
        /// </returns>
        public PipingErrors ValidatePipeLength(SystemVRF sysItem, ref AddFlow addFlowItem)
        {
            CalcPipingLimitValues(sysItem, addFlowItem);

            PipingErrors errorType = PipingErrors.OK;

            if (!sysItem.IsInputLengthManually)
            {
                return errorType;
            }

            if (errorType == PipingErrors.OK)
            {
                errorType = ValLinkLengthMin(sysItem, ref addFlowItem); //检验每段管长不能为0
            }
            if (errorType == PipingErrors.OK)
            {
                errorType = ValMaxRealLength(sysItem, addFlowItem);//检验最大实际管长
            }
            if (errorType == PipingErrors.OK)
            {
                //验证第一分歧管一个分支下的最近的室内机和最远的室内机的管长差是否大于40米
                errorType = ValDiffLengthFurthestIndoorAndClosestIndoor(sysItem);
            }
            if (errorType == PipingErrors.OK)
            {
                errorType = ValMaxEqLength(sysItem, addFlowItem); //检验最大等效管长和第一分歧管到远端的最远距离
            }
            if (errorType == PipingErrors.OK)
            {
                errorType = ValPipeLengthAndHighDiff(sysItem, sysItem.MyPipingNodeOut);//检验管长和高度差之间的关系
            }
            if (errorType == PipingErrors.OK)
            {
                //检验液管总长
                if (!ValTotalLquidLength(sysItem))
                {
                    errorType = PipingErrors.TOTALLIQUIDLENGTH;// -6;
                }
            }
            //检验每个Mutil-kit到Indoor的最远距离
            if (!ValMaxMKToIndoorLength(sysItem, addFlowItem, errorType != PipingErrors.OK) && errorType == PipingErrors.OK)
            {
                errorType = PipingErrors.MKTOINDOORLENGTH;// -7;
            }
            if (errorType == PipingErrors.OK)
            {
                //多台室外机组成机组时，校验第一分歧管到第一Piping Connection kit之间的管长不能小于0.5m
                if (!Val1stBranchTo1stCKLength(sysItem))
                {
                    errorType = PipingErrors.FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH;// -14;
                }
            }

            // 计算管长修正系数
            sysItem.PipingLengthFactor = GetPipeLengthFactor(sysItem, "Cooling");
            sysItem.PipingLengthFactor_H = GetPipeLengthFactor(sysItem, "Heating");

            return errorType;// 0;
        }
        //took latest code from legacy
        public PipingErrors ValDiffLengthFurthestIndoorAndClosestIndoor(SystemVRF curSystemItem)
        {
            if (curSystemItem == null) return PipingErrors.OK;

            //FSXNSE, FSXNPE, JNBBQ, CNCQ, HNCQ, HNBQ, FSNS, FSNP, FSXNS, FSXNP, FSNS5B, FSNS7B, FSNSCB, FSNC7B. 
            //For York brand, JVOHQ (Top flow, 3Ph), JTOH-BS1 (Top discharge), JTOR-BS1 (Top discharge).
            string[] series = new string[] {
                "Commercial VRF HR, FSXNPE",
                "Commercial VRF HP, FSXNSE",
                "Commercial VRF HP, FSXNPE",
                "Commercial VRF HR, FSXNSE",
                "Commercial VRF CO, CNCQ",
                "Commercial VRF High ambient, JNBBQ",
                "Commercial VRF HP, FSNS7B",
                "Commercial VRF HP, FSNS5B",
                "Commercial VRF HP, FSNC7B",
                "Commercial VRF HP, FSNC5B",
                "Commercial VRF HP, FSNS",
                "Commercial VRF HP, FSNP",
                "Commercial VRF HR, FSXNS",
                "Commercial VRF HR, FSXNP",
                "Commercial VRF HP, HNCQ",
                "Commercial VRF HP, HNBQ",
                "Commercial VRF HP, JVOHQ",
                "Commercial VRF HP, JTOH-BS1",
                "Commercial VRF HR, JTOR-BS1"
            };
            if (curSystemItem.OutdoorItem == null || !series.Contains(curSystemItem.OutdoorItem.Series))
            {
                return PipingErrors.OK;
            }

            MyNodeYP nodeYP = null;
            MyNodeOut nodeOut = curSystemItem.MyPipingNodeOut;
            if (nodeOut != null && nodeOut.ChildNode is MyNodeYP)
            {
                nodeYP = nodeOut.ChildNode as MyNodeYP;
            }
            if (nodeYP == null || !IsMainBranch(nodeYP)) return PipingErrors.OK;

            double maxLength1, minLength1;
            MyNodeIn furthestIU1, closestIU1;
            GetPipeLengthBetweenFirstBranchAndIndoors(nodeYP.ChildNodes[0], out furthestIU1, out closestIU1, out maxLength1, out minLength1);

            double maxLength2, minLength2;
            MyNodeIn furthestIU2, closestIU2;
            GetPipeLengthBetweenFirstBranchAndIndoors(nodeYP.ChildNodes[1], out furthestIU2, out closestIU2, out maxLength2, out minLength2);

            //IDU 到第一分歧管的距离差不能大于40m，可以是40m  
            if (maxLength2 - minLength1 > 40d)
            {
                setLinkPathColor(furthestIU2, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                setLinkPathColor(closestIU1, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                utilPiping.setItemDefault(nodeYP);
                return PipingErrors.DIFF_LEN_FURTHEST_CLOSESST_IU; //-17
            }
            if (maxLength1 - minLength2 > 40d)
            {
                setLinkPathColor(furthestIU1, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                setLinkPathColor(closestIU2, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                utilPiping.setItemDefault(nodeYP);
                return PipingErrors.DIFF_LEN_FURTHEST_CLOSESST_IU; //-17
            }
            return PipingErrors.OK;
        }
        //took latest code from legacy
        private void GetPipeLengthBetweenFirstBranchAndIndoors(Node node,
          out MyNodeIn furthestIU, out MyNodeIn closestIU,
          out double maxLength, out double minLength)
        {
            maxLength = 0;
            minLength = 0;
            furthestIU = null;
            closestIU = null;

            if (node == null) return;
            MyNode myNode = node as MyNode;
            if (myNode == null || myNode.MyInLinks == null || myNode.MyInLinks.Count == 0) return;
            Link lk = myNode.MyInLinks[0];
            if (!(lk is MyLink)) return;

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                double maxLength1, minLength1;
                MyNodeIn furthestIU1, closestIU1;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    GetPipeLengthBetweenFirstBranchAndIndoors(child, out furthestIU1, out closestIU1, out maxLength1, out minLength1);
                    if (furthestIU1 != null && (furthestIU == null || maxLength1 > maxLength))
                    {
                        maxLength = maxLength1;
                        furthestIU = furthestIU1;
                    }
                    if (closestIU1 != null && (closestIU == null || minLength1 < minLength))
                    {
                        minLength = minLength1;
                        closestIU = closestIU1;
                    }
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                GetPipeLengthBetweenFirstBranchAndIndoors(nodeCH.ChildNode, out furthestIU, out closestIU, out maxLength, out minLength);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                double maxLength1, minLength1;
                MyNodeIn furthestIU1, closestIU1;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    GetPipeLengthBetweenFirstBranchAndIndoors(child, out furthestIU1, out closestIU1, out maxLength1, out minLength1);
                    if (furthestIU1 != null && (furthestIU == null || maxLength1 > maxLength))
                    {
                        maxLength = maxLength1;
                        furthestIU = furthestIU1;
                    }
                    if (closestIU1 != null && (closestIU == null || minLength1 < minLength))
                    {
                        minLength = minLength1;
                        closestIU = closestIU1;
                    }
                }
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                furthestIU = nodeIn;
                closestIU = nodeIn;
            }

            MyLink link = (MyLink)lk;
            maxLength += link.Length;
            minLength += link.Length;
        }
        //took latest code from legacy
        public PipingErrors ValPipeLengthAndHighDiff(SystemVRF curSystemItem, Node node,
           MyNode parentCHBox = null, double chBox_ODU_Length = 0, double heightDiff = 0)
        {
            if (node == null) return PipingErrors.OK;

            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                return ValPipeLengthAndHighDiff(curSystemItem, nodeOut.ChildNode);
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;

                double IDU_ODU_Length = 0; // 室内机到室外机之间的管长
                getRealLength(nodeIn, ref IDU_ODU_Length);
                if (IDU_ODU_Length < nodeIn.RoomIndooItem.HeightDiff)
                {
                    // 室内机到室外机之间的管长不能小于高度差
                    setLinkPathColor(nodeIn, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                    TempActualLength = IDU_ODU_Length;
                    TempMaxLength = nodeIn.RoomIndooItem.HeightDiff;
                    return PipingErrors.INDOORLENGTH_HIGHDIFF;  //室内机到室外机的管长 小于室内机设置的高度差
                }

                if (parentCHBox != null)
                {
                    if (!ValCHbox_IndoorLength(nodeIn, heightDiff, chBox_ODU_Length, ref TempActualLength, ref TempMaxLength))
                    {
                        setLinkPathColor(nodeIn, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                        setLinkPathColor(parentCHBox, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
                        TempActualLength = Math.Abs(TempActualLength);
                        return PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF; //室外机到CHBOX的管长 小于室内机到CHBOX的高度差
                    }
                }
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    PipingErrors childResult = ValPipeLengthAndHighDiff(curSystemItem, child, parentCHBox, chBox_ODU_Length, heightDiff);
                    if (childResult != PipingErrors.OK)
                    {
                        return childResult;
                    }
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                chBox_ODU_Length = 0;
                getRealLength(nodeCH, ref chBox_ODU_Length);
                //得到每个CHbox到室外机之间的实际管长，赋给变量tempActualLength 
                if (chBox_ODU_Length < Math.Abs(nodeCH.HeightDiff))
                {
                    setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                    TempActualLength = chBox_ODU_Length;
                    TempMaxLength = Math.Abs(nodeCH.HeightDiff);
                    return PipingErrors.CHBOXLENGTH_HIGHDIFF; //CHBOX到室外机的管长 小于CHBox设置的高度差
                }

                return ValPipeLengthAndHighDiff(curSystemItem, nodeCH.ChildNode, nodeCH, chBox_ODU_Length, nodeCH.HeightDiff);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                chBox_ODU_Length = 0;
                getRealLength(node, ref chBox_ODU_Length);
                //得到每个MultiCHbox到室外机之间的实际管长，赋给变量tempActualLength 
                if (chBox_ODU_Length < Math.Abs(nodeMCH.HeightDiff))
                {
                    setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                    TempActualLength = chBox_ODU_Length;
                    TempMaxLength = Math.Abs(nodeMCH.HeightDiff);
                    return PipingErrors.CHBOXLENGTH_HIGHDIFF;  // //CHBOX到室外机的管长 小于CHBox设置的高度差
                }

                foreach (Node child in nodeMCH.ChildNodes)
                {
                    PipingErrors childResult = ValPipeLengthAndHighDiff(curSystemItem, child, nodeMCH, chBox_ODU_Length, nodeMCH.HeightDiff);
                    if (childResult != PipingErrors.OK)
                    {
                        return childResult;
                    }
                }
            }

            return PipingErrors.OK;
        }
        //took latest code from legacy
        private void getRealLength(Node node, ref double realLength)
        {
            if (node == null) return;
            MyNode myNode = node as MyNode;
            if (myNode == null || myNode.MyInLinks == null || myNode.MyInLinks.Count == 0) return;
            Link lk = myNode.MyInLinks[0];
            if (!(lk is MyLink)) return;
            MyLink link = (MyLink)lk;
            realLength = realLength + link.Length;
            if (node.Links != null && node.Links.Count > 0)
            {
                Link newLink = node.Links[0];
                if (newLink.Org is MyNodeOut)
                    return;
                else
                    getRealLength(newLink.Org, ref realLength);
            }
        }
        public PipingErrors ValidateIDUOfMultiCHBox(SystemVRF sysItem)
        {
            PipingErrors err = PipingErrors.OK;
            if (thisProject.SubRegionCode == "ANZ")
            {
                EachNode(sysItem.MyPipingNodeOut, node1 =>
                {
                    if (node1 is MyNodeMultiCH)
                    {
                        int count = 0;
                        EachNode(node1, node2 =>
                        {
                            if (node2 is MyNodeIn)
                            {
                                MyNodeIn nodeIn = node2 as MyNodeIn;
                                double horsePower = nodeIn?.RoomIndooItem?.IndoorItem?.Horsepower ?? 0;
                                //每个multiple CH-Box最多只能连接2个8HP/10HP的IDU。仅限ANZ。 add by Shen Junjie on 2018/8/17
                                if (horsePower == 8 || horsePower == 10)
                                {
                                    count++;
                                }
                            }
                        });
                        if (count > 2)
                        {
                            //每个multiple CH-Box最多只能连接2个8HP/10HP的IDU。仅限ANZ。 add by Shen Junjie on 2018/8/17
                            err = PipingErrors.ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX;
                        }
                        return true;  //结束遍历。
                    }
                    return false;
                }, false);
            }
            return err;
        }
        public void screencapture()
        {
            //ScreenCapturer
        }

        /// <summary>
        /// 验证Height Difference
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="sysItem"></param>
        /// <param name="addFlowItem"></param>
        /// <returns></returns>
        public PipingErrors ValidateSystemHighDifference(SystemVRF sysItem)
        {
            PipingErrors errorType = PipingErrors.OK;
            //验证Height Difference
            if (errorType == PipingErrors.OK)
            {
                if (!ValMaxUpperHeightDifference(sysItem))
                {
                    errorType = PipingErrors.MAX_HIGHDIFF_UPPER;//-22
                    return errorType;
                }
                if (!ValMaxLowerHeightDifference(sysItem))
                {
                    errorType = PipingErrors.MAX_HIGHDIFF_LOWER;//-23
                    return errorType;
                }
                if (!ValMaxIndoorHeightDifference(sysItem))
                {
                    errorType = PipingErrors.MAX_HIGHDIFF_INDOOR;//-24
                    return errorType;
                }
                if (!ValMaxCHBoxHeightDifference(sysItem))
                {
                    errorType = PipingErrors.MAX_CHBOXHIGHDIFF;//-25
                    return errorType;
                }
                if (!ValMaxSameCHBoxHeightDifference(sysItem))
                {
                    errorType = PipingErrors.MAX_MULTICHBOXHIGHDIFF;//-26
                    return errorType;
                }
                if (!ValMaxCHBox_IndoorHeightDifference(sysItem))
                {
                    errorType = PipingErrors.MAX_CHBOX_INDOORHIGHDIFF;//-27
                    return errorType;
                }
            }
            return errorType;// 0;
        }


        /// Height Difference between OldModel.Outdoor Units and OldModel.Indoor Units，室外单元和室内单元之间的高度差异(Upper) add on 20180502 by xyj
        /// <summary>
        /// 室外单元和室内单元之间的高度差异(Upper) add on 20180502 
        /// </summary>
        public bool ValMaxUpperHeightDifference(SystemVRF sysItem)
        {
            if (sysItem.MaxUpperHeightDifferenceLength > sysItem.MaxOutdoorAboveHeight)
            {
                return false;
            }
            return true;
        }

        /// Height Difference between OldModel.Outdoor Units and OldModel.Indoor Units，室外单元和室内单元之间的高度差异(Lower) add on 20180502 by xyj
        /// <summary>
        /// 室外单元和室内单元之间的高度差异(Lower) add on 20180502 
        /// </summary>
        public bool ValMaxLowerHeightDifference(SystemVRF sysItem)
        {
            if (sysItem.MaxLowerHeightDifferenceLength > sysItem.MaxOutdoorBelowHeight)
            {
                return false;
            }
            return true;
        }

        /// Height Difference between OldModel.Outdoor Units and OldModel.Indoor Units，室外单元和室内单元之间的高度差异(Lower) add on 20180502 by xyj
        /// <summary>
        /// 室外单元和室内单元之间的高度差异(Upper) add on 20180502 
        /// </summary>
        public bool ValMaxIndoorHeightDifference(SystemVRF sysItem)
        {
            if (sysItem.MaxIndoorHeightDifferenceLength > sysItem.MaxDiffIndoorHeight)
            {
                return false;
            }
            return true;
        }


        /// Height Difference between CH-Box and OldModel.Indoor Units，CH-Box和室内单元之间的高度差异 add on 20180620 by xyj
        /// <summary>
        /// CH-Box和室内单元之间的高度差异
        /// </summary>
        public bool ValMaxCHBox_IndoorHeightDifference(SystemVRF sysItem)
        {
            if (sysItem.MaxCHBox_IndoorHighDiffLength > sysItem.NormalCHBox_IndoorHighDiffLength)
            {
                return false;
            }
            return true;
        }

        /// Height Difference between OldModel.Indoor Units using the Same Branch of CH-Box，使用CH-Box的同一分支的室内单元之间的高度差异 add on 20180620 by xyj
        /// <summary>
        /// 使用CH-Box的同一分支的室内单元之间的高度差异  add on 20180620 by xyj
        /// </summary>
        public bool ValMaxSameCHBoxHeightDifference(SystemVRF sysItem)
        {
            if (sysItem.MaxSameCHBoxHighDiffLength > sysItem.NormalSameCHBoxHighDiffLength)
            {
                return false;
            }
            return true;
        }

        /// Height Difference between CH-boxes，CH-boxes之间的高度差异 add on 20180620 by xyj
        /// <summary>
        /// CH-boxes之间的高度差异 add on 20180620 by xyj 
        /// </summary>
        public bool ValMaxCHBoxHeightDifference(SystemVRF sysItem)
        {
            if (sysItem.MaxCHBoxHighDiffLength > sysItem.NormalCHBoxHighDiffLength)
            {
                return false;
            }
            return true;
        }


        ///// <summary>
        ///// 为Heat Recovery系统的节点连接线设置默认颜色
        ///// </summary>
        ///// <param name="node"></param>
        //public void DrawHRLinkDefaultColor(Node node)
        //{
        //    Color linkColor = utilPiping.color_piping_3pipe;
        //    if (node is MyNodeIn)
        //    {
        //        MyNodeIn nodeIn = node as MyNodeIn;
        //        if (nodeIn.IsCoolingonly)
        //            linkColor = utilPiping.color_piping_2pipe_lowgas;
        //        else
        //            linkColor = utilPiping.color_piping_2pipe;
        //    }
        //    else if (node is MyNodeYP)
        //    {
        //        MyNodeYP yp = node as MyNodeYP;
        //        if (yp.IsCoolingonly)
        //            linkColor = utilPiping.color_piping_2pipe_lowgas;
        //        else if (utilPiping.ExistsCHDownward(yp))
        //            linkColor = utilPiping.color_piping_3pipe;
        //        else
        //            linkColor = utilPiping.color_piping_2pipe;
        //    }
        //    foreach (Link link in node.Links)
        //    {
        //        link.Stroke = linkColor;
        //    }
        //}

        public void SetDefaultColor(ref AddFlow addFlowPiping, bool isHR)
        {
            foreach (Node node in addFlowPiping.Nodes())
            {
                if (node is MyNodeOut)
                {
                    node.Stroke = utilPiping.colorTransparent;
                }
                if (node is MyNode)
                {
                    utilPiping.setItemDefault(node, isHR);
                }
            }
        }

        /// 检验连接管长度是否都大于0
        /// <summary>
        /// 检验连接管长度是否都大于0，不符合要求的标记为红色
        /// </summary>
        /// <param name="minLength"></param>
        private PipingErrors ValLinkLengthMin(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            bool isHR = IsHeatRecovery(sysItem);
            SetDefaultColor(ref addFlowPiping, isHR);
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            //add by Shen Junjie on 2018/8/2
            if (nodeOut != null && nodeOut.UnitCount > 5)
            {

                if (nodeOut.PipeLengthes == null)
                {
                    nodeOut.Stroke = utilPiping.colorWarning;
                    return PipingErrors.ODU_PIPE_LENGTH_LIMITS;
                }
                for (int i = 0; i < nodeOut.PipeLengthes.Length; i++)
                {
                    double len = nodeOut.PipeLengthes[i];
                    //所有管长必须>0
                    if (len <= 0)
                    {
                        nodeOut.Stroke = utilPiping.colorWarning;
                        return PipingErrors.ODU_PIPE_LENGTH_LIMITS;
                    }
                    //connection kit前面的管长不能小于0.5m
                    if ((nodeOut.UnitCount == 3 && i < 1) || (nodeOut.UnitCount == 4 && i < 2))
                    {
                        if (len < 0.5)
                        {
                            nodeOut.Stroke = utilPiping.colorWarning;
                            return PipingErrors.ODU_PIPE_LENGTH_LIMITS;
                        }
                    }
                }
                if (nodeOut.Links.Count > 0)//POC OutLinks -- LInks   // To be Fix latter
                {
                    Link lk = nodeOut.Links[0];
                    MyLink link = (MyLink)lk;
                    if (link.Length < 0.5)
                    {
                        //Org是connection kit需要判断不能小于0.5m
                        link.Stroke = utilPiping.colorWarning;
                        return PipingErrors.MIN_DISTANCE_BETWEEN_MULTI_KITS;
                    }
                }
            }
            foreach (MyLink link in addFlowPiping.Items.OfType<MyLink>())
            {
                if (link.Dst is MyNodeYP && link.Length < 0.5) //改成Dst是multi kit就需要判断0.5m      modified by Shen Junjie on 2018/8/2
                {
                    link.Stroke = utilPiping.colorWarning;
                    return PipingErrors.MIN_DISTANCE_BETWEEN_MULTI_KITS;
                }
                else if (link.Length <= 0)
                {
                    link.Stroke = utilPiping.colorWarning;
                    return PipingErrors.LINK_LENGTH;
                }
            }
            foreach (Node node in addFlowPiping.Nodes(true))
            {
                foreach (Link lk in node.Links)
                {
                    if (lk is MyLink) //增加非MyLink过滤 add on 20160511 by Yunxiao Lin
                    {
                        MyLink link = (MyLink)lk;
                        //add by Shen Junjie on 2018/7/12
                        //The minimum distances between multikits is 0.5 meters. 
                        if (link.Dst is MyNodeYP && link.Length < 0.5) //改成Dst是multi kit就需要判断0.5m      modified by Shen Junjie on 2018/8/2
                        {
                            //link.Stroke = utilPiping.colorWarning;
                            //return PipingErrors.MIN_DISTANCE_BETWEEN_MULTI_KITS;
                        }
                        else if (link.Length <= 0)
                        {
                            //link.Stroke = utilPiping.colorWarning;
                            //return PipingErrors.LINK_LENGTH;
                        }
                    }
                }
            }
            return PipingErrors.OK;
        }

        /// 计算及检验最长实际管长
        /// <summary>
        /// 计算及检验最长实际管长
        /// </summary>
        /// <returns></returns>
        /// //Took latest code from legacy
        public PipingErrors ValMaxRealLength(SystemVRF curSystemItem, AddFlow addFlowPiping)
        {
            double maxRealLength = 0;
            Node furthestIU = null;
            Node firstBranch = null;
            foreach (Node node in addFlowPiping.Nodes(true))
            {
                if (node is MyNodeIn)
                {
                    double tempRealLength = 0;
                    getRealLength(node, ref tempRealLength);  //得到每个室内机到室外机之间的实际管长，赋给变量tempActualLength
                    //2013-10-24 by Yang 根据标准表判断最长管实际管长
                    //部分室外机系列没有MaxPipeLengthwithFA限制，需要判断是否为0. 20180815 by Yunxiao Lin
                    if (curSystemItem.SysType == OldModel.SystemType.OnlyFreshAir && curSystemItem.MaxPipeLengthwithFA > 0 && tempRealLength > curSystemItem.MaxPipeLengthwithFA)
                    {
                        //setLinkPathColor(node, utilPiping.colorWarning);
                        setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                        return PipingErrors.WARN_ACTLENGTH_FA;
                    }
                    else if (tempRealLength > curSystemItem.MaxPipeLength)
                    {
                        //setLinkPathColor(node, utilPiping.colorWarning);
                        setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                        return PipingErrors.WARN_ACTLENGTH;
                    }
                    //如果当前的最长管长值合法，则赋值给变量maxActualLength
                    if (maxRealLength < tempRealLength)
                    {
                        maxRealLength = tempRealLength;
                        furthestIU = node;
                    }
                }
                else if (node is MyNodeYP)
                {
                    MyNodeYP yp = node as MyNodeYP;
                    //第一分歧管节点 
                    if (yp.IsFirstYP)
                    {
                        firstBranch = yp;
                    }
                }

                //else if (node is MyNodeCH || node is MyNodeOut || node is MyNodeYP || node is MyNodeMultiCH) //排除图例node，add on 20160511 by Yunxiao Lin
                //{
                //    //setLinkPathColor(node, utilPiping.colorDefault);
                //    setLinkPathColor(node, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
                //}
            }

            #region L2的90米限制改用Actual length add by Shen Junjie on 2018/07/27
            if (firstBranch != null && curSystemItem.MaxIndoorLength > 0)   //添加 MaxIndoorLength(L2) 为0，不做判断 on 20180816 by xyj
            {
                double firstPipeLength = 0;
                getRealLength(firstBranch, ref firstPipeLength);   //此时的tempActualLength值为第一连接管的实际长度
                //得到此时第一分歧管到最远端室内机的等效管长
                if (maxRealLength - firstPipeLength > curSystemItem.MaxIndoorLength)
                {
                    //将最远的Indoor节点到Outdoor节点的路径标记为红色
                    setLinkPathColor(furthestIU, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                    //将第一分歧管到Outdoor的路径恢复为默认颜色
                    setLinkPathColor(firstBranch, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
                    return PipingErrors.FIRSTLENGTH; // -4;
                }
            }
            #endregion


            //最大管长和最大高度差做比较
            if (maxRealLength < Math.Abs(curSystemItem.HeightDiff))
            {
                return PipingErrors.LENGTHFACTOR; // -5;
            }
            return PipingErrors.OK;
        }
        //public PipingErrors ValMaxRealLength(SystemVRF curSystemItem, AddFlow addFlowPiping)
        //{
        //    bool isHR = IsHeatRecovery(curSystemItem);
        //    bool flag = true;
        //    double maxRealLength = 0;
        //    double minRealLength = double.MaxValue;
        //    Node firstMainBranch = null;
        //    Node furthestIU = null;
        //    Node closestIU = null;
        //    Node firstBranch = null;
        //    foreach (Node node in addFlowPiping.Nodes())
        //    {
        //        #region 检验室内机管长及高度差 on 20180621 by xyj
        //        if (node is MyNodeIn)
        //        {
        //            double tempRealLength = 0;
        //            getRealLength(node, 0, ref tempRealLength);  //得到每个室内机到室外机之间的实际管长，赋给变量tempActualLength
        //            //2013-10-24 by Yang 根据标准表判断最长管实际管长
        //            //部分室外机系列没有MaxPipeLengthwithFA限制，需要判断是否为0. 20180815 by Yunxiao Lin
        //            if (curSystemItem.SysType == OldModel.SystemType.OnlyFreshAir && curSystemItem.MaxPipeLengthwithFA > 0 && tempRealLength > curSystemItem.MaxPipeLengthwithFA)
        //            {
        //                flag = false;
        //            }
        //            else if (tempRealLength > curSystemItem.MaxPipeLength)
        //            {
        //                flag = false;
        //            }
        //            //20180621 by xyj 验证实际管长是否小于室内机设置的高度差
        //            MyNodeIn nodeIn = node as MyNodeIn;
        //            if (tempRealLength < nodeIn.RoomIndooItem.HeightDiff)
        //            {
        //                setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //                TempActualLength = tempRealLength;
        //                TempMaxLength = nodeIn.RoomIndooItem.HeightDiff;
        //                return PipingErrors.INDOORLENGTH_HIGHDIFF;
        //            }
        //            if (!flag)
        //            {
        //                //setLinkPathColor(node, utilPiping.colorWarning);
        //                setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //                return PipingErrors.WARN_ACTLENGTH;
        //            }
        //            //如果当前的最长管长值合法，则赋值给变量maxActualLength
        //            if (maxRealLength < tempRealLength)
        //            {
        //                maxRealLength = tempRealLength;
        //                furthestIU = node;
        //            }
        //            if (minRealLength > tempRealLength)
        //            {
        //                minRealLength = tempRealLength;
        //                closestIU = node;
        //            }
        //        }
        //        #endregion
        //        else if (node is MyNodeYP)
        //        {
        //            MyNodeYP yp = node as MyNodeYP;
        //            //第一分歧管节点 
        //            if (yp.IsFirstYP)
        //            {
        //                firstBranch = yp;
        //                if (IsMainBranch(yp))
        //                {
        //                    firstMainBranch = yp;
        //                }
        //            }
        //        }
        //        #region 检验CHBox 到室内机的管长及高度差 on 20180621 by xyj
        //        else if (node is MyNodeCH)
        //        {
        //            MyNodeCH nodeCH = node as MyNodeCH;
        //            double tempCHRealLength = 0;
        //            getRealLength(nodeCH, 0, ref tempCHRealLength);
        //            //得到每个CHbox到室外机之间的实际管长，赋给变量tempActualLength 
        //            if (tempCHRealLength < Math.Abs(nodeCH.HeightDiff))
        //            {
        //                setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //                TempActualLength = tempCHRealLength;
        //                TempMaxLength = Math.Abs(nodeCH.HeightDiff);
        //                return PipingErrors.CHBOXLENGTH_HIGHDIFF;
        //            }
        //            //当前节点如果是OldModel.Indoor 需要验证实际长度与高度的验证
        //            if (nodeCH.ChildNode is MyNodeIn)
        //            {
        //                MyNodeIn nodeIn = (nodeCH.ChildNode as MyNodeIn);
        //                double indff = 0;
        //                double indLength = CHBox_IndoorLength(nodeIn, ref indff); //当前室内机的实际长度
        //                if (indLength > 40)    //如果当前的室内机的实际长度大于40 
        //                {
        //                    DrawNodeErrorLinks(nodeIn, curSystemItem);  //绘制节点到CHbox 链接线
        //                    setLinkPathColor(nodeCH, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //                    TempActualLength = indLength;
        //                    TempMaxLength = 40;
        //                    return PipingErrors.PIPING_CHTOINDOORTOTALLENGTH;
        //                }

        //                if (!ValIndoor_HeightDiff(nodeIn, curSystemItem))
        //                {
        //                    return PipingErrors.INDOORLENGTH_HIGHDIFF;
        //                }
        //                // if (!ValCHboxToIndoorLinks(nodeIn, nodeCH.HeightDiff, 0,ref TempActualLength,ref TempMaxLength))  //当前长度与室内机的高度差验证 
        //                if (!ValCHbox_IndoorLength(nodeIn, nodeCH.HeightDiff, tempCHRealLength, ref TempActualLength, ref TempMaxLength))
        //                {
        //                    CHbox_IndoorErrorLinks(curSystemItem, nodeCH, tempCHRealLength, nodeIn);
        //                    return PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF;
        //                }
        //            }
        //            else if (nodeCH.ChildNode is MyNodeYP)
        //            {
        //                MyNodeYP nodeYp = (nodeCH.ChildNode as MyNodeYP);
        //                double tempCHRealLengths = 0;
        //                getRealLength(nodeYp, 0, ref tempCHRealLengths);  //得到每个CHbox到室外机之间的实际管长，赋给变量tempActualLength  
        //                double ypLength = getTotalToIndoorLength(nodeYp);  //CHBOX -YP -INDOOR 的实际管长
        //                if (ypLength > 40)
        //                {
        //                    DrawNodeErrorLinks(nodeYp, curSystemItem);
        //                    setLinkPathColor(nodeCH, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //                    TempActualLength = ypLength;
        //                    TempMaxLength = 40;
        //                    return PipingErrors.PIPING_CHTOINDOORTOTALLENGTH;
        //                }
        //                foreach (Node childs in nodeYp.ChildNodes)
        //                {
        //                    if (childs is MyNodeIn)
        //                    {
        //                        MyNodeIn nodeIn = (childs as MyNodeIn);

        //                        if (!ValIndoor_HeightDiff(nodeIn, curSystemItem))
        //                        {
        //                            return PipingErrors.INDOORLENGTH_HIGHDIFF;
        //                        }
        //                        //if (!ValCHboxToIndoorLinks(nodeIn, nodeCH.HeightDiff, tempCHRealLength,ref TempActualLength, ref TempMaxLength))
        //                        if (!ValCHbox_IndoorLength(nodeIn, nodeCH.HeightDiff, tempCHRealLength, ref TempActualLength, ref TempMaxLength))
        //                        {
        //                            CHbox_IndoorErrorLinks(curSystemItem, nodeCH, tempCHRealLength, nodeIn);
        //                            return PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        #endregion

        //        #region 检验MultiCHBox 到室内机的管长及高度差 on 20180621 by xyj
        //        else if (node is MyNodeMultiCH)
        //        {
        //            MyNodeMultiCH multiNode = node as MyNodeMultiCH;
        //            double tempCHRealLength = 0;
        //            getRealLength(node, 0, ref tempCHRealLength);
        //            //得到每个MultiCHbox到室外机之间的实际管长，赋给变量tempActualLength 
        //            if (tempCHRealLength < Math.Abs(multiNode.HeightDiff))
        //            {
        //                setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //                TempActualLength = tempCHRealLength;
        //                TempMaxLength = Math.Abs(multiNode.HeightDiff);
        //                return PipingErrors.CHBOXLENGTH_HIGHDIFF;
        //            }
        //            double sum = getTotalToIndoorLength(multiNode);
        //            foreach (Node child in multiNode.ChildNodes)
        //            {
        //                //室内机节点
        //                if (child is MyNodeIn)
        //                {
        //                    MyNodeIn nodeIn = (child as MyNodeIn);
        //                    double indff = 0;
        //                    double indLength = CHBox_IndoorLength(nodeIn, ref indff);
        //                    if (indLength > 40)
        //                    {
        //                        DrawNodeErrorLinks(nodeIn, curSystemItem);
        //                        setLinkPathColor(multiNode, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //                        TempActualLength = indLength;
        //                        TempMaxLength = 40;
        //                        return PipingErrors.PIPING_CHTOINDOORTOTALLENGTH;
        //                    }
        //                    if (!ValIndoor_HeightDiff(nodeIn, curSystemItem))
        //                    {
        //                        return PipingErrors.INDOORLENGTH_HIGHDIFF;
        //                    }
        //                    if (!ValCHbox_IndoorLength(nodeIn, multiNode.HeightDiff, tempCHRealLength, ref TempActualLength, ref TempMaxLength))
        //                    //   if (!ValCHboxToIndoorLinks(nodeIn, multiNode.HeightDiff, 0,ref TempActualLength,ref TempMaxLength))
        //                    {
        //                        CHbox_IndoorErrorLinks(curSystemItem, multiNode, tempCHRealLength, nodeIn);
        //                        return PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF;
        //                    }
        //                }
        //                else if (child is MyNodeYP)  //分歧管节点
        //                {
        //                    MyNodeYP nodeYp = (child as MyNodeYP);
        //                    double branchLengths = 0;
        //                    getRealLength(nodeYp, 0, ref branchLengths);  //得到每个MultiCHbox到室外机之间的实际管长，赋给变量tempActualLength 
        //                    double ypLength = getTotalToIndoorLength(nodeYp);  //CHBOX -YP -INDOOR 的实际管长
        //                    if (ypLength > 40)
        //                    {
        //                        DrawNodeErrorLinks(nodeYp, curSystemItem);
        //                        setLinkPathColor(multiNode, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //                        TempActualLength = ypLength;
        //                        TempMaxLength = 40;
        //                        return PipingErrors.PIPING_CHTOINDOORTOTALLENGTH;
        //                    }
        //                    foreach (Node childs in nodeYp.ChildNodes)
        //                    {
        //                        if (childs is MyNodeIn)
        //                        {
        //                            MyNodeIn nodeIn = (childs as MyNodeIn);
        //                            if (!ValIndoor_HeightDiff(nodeIn, curSystemItem))
        //                            {
        //                                return PipingErrors.INDOORLENGTH_HIGHDIFF;
        //                            }

        //                            //double Inddiff = getMultiBox_IndoorHeightDiff(nodeIn, multiNode);
        //                            //double indff = 0;
        //                            //double indoorLength = CHBox_IndoorLength(nodeIn, ref indff);
        //                            //if ((branchLengths - tempCHRealLength + indoorLength) < Inddiff)
        //                            //{
        //                            //    setLinkPathColor(nodeIn, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //                            //    setLinkPathColor(multiNode, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //                            //    TempMaxLength = Inddiff;
        //                            //    return PipingErrors.INDOORLENGTH_HIGHDIFF;
        //                            //} 
        //                            if (!ValCHbox_IndoorLength(nodeIn, multiNode.HeightDiff, tempCHRealLength, ref TempActualLength, ref TempMaxLength))
        //                            //if (!ValCHboxToIndoorLinks(nodeIn, multiNode.HeightDiff, branchLengths, ref TempActualLength, ref TempMaxLength))
        //                            {
        //                                CHbox_IndoorErrorLinks(curSystemItem, multiNode, tempCHRealLength, nodeIn);
        //                                return PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        #endregion

        //        //else if (node is MyNodeCH || node is MyNodeOut || node is MyNodeYP || node is MyNodeMultiCH) //排除图例node，add on 20160511 by Yunxiao Lin
        //        //{
        //        //    //setLinkPathColor(node, utilPiping.colorDefault);
        //        //    setLinkPathColor(node, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //        //}
        //    }

        //    #region L2的90米限制改用Actual length add by Shen Junjie on 2018/07/27
        //    if (firstBranch != null && curSystemItem.MaxIndoorLength > 0)   //添加 MaxIndoorLength(L2) 为0，不做判断 on 20180816 by xyj
        //    {
        //        double firstPipeLength = 0;
        //        getRealLength(firstBranch, 0, ref firstPipeLength);   //此时的tempActualLength值为第一连接管的实际长度
        //        //得到此时第一分歧管到最远端室内机的等效管长
        //        if (maxRealLength - firstPipeLength > curSystemItem.MaxIndoorLength)
        //        {
        //            //将最远的OldModel.Indoor节点到OldModel.Outdoor节点的路径标记为红色
        //            setLinkPathColor(furthestIU, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //            //将第一分歧管到OldModel.Outdoor的路径恢复为默认颜色
        //            setLinkPathColor(firstBranch, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
        //            return PipingErrors.FIRSTLENGTH; // -4;
        //        }
        //    }
        //    #endregion

        //    //赋给当前系统对应的属性
        //    //curSystemItem.PipeActualLength = maxRealLength; //通过GetSumEqAndActualLength计算得到 deleted by Shen Junjie on 2018/3/29
        //    //以下限制暂时只有EU的RAS-FSXNPE和RAS-FSXNSE有。 modify by Yunxiao Lin 20180406
        //    if (firstMainBranch != null && (curSystemItem.OutdoorItem.Series.Contains("FSXNSE") || curSystemItem.OutdoorItem.Series.Contains("FSXNPE")))
        //    {
        //        //if (maxRealLength - minRealLength >= 40d)
        //        if (maxRealLength - minRealLength > 40d) //IDU 到第一分歧管的距离差不能大于40m，可以是40m modify by Yunxiao Lin 20180406
        //        {
        //            setLinkPathColor(furthestIU, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //            setLinkPathColor(closestIU, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
        //            utilPiping.setItemDefault(firstMainBranch, isHR);
        //            return PipingErrors.DIFF_LEN_FURTHEST_CLOSESST_IU; //-17
        //        }
        //    }

        //    //最大管长和最大高度差做比较
        //    if (maxRealLength < Math.Abs(curSystemItem.HeightDiff))
        //    {
        //        return PipingErrors.LENGTHFACTOR; // -5;
        //    }
        //    return PipingErrors.OK;
        //}


        /// <summary>
        /// 验证室内到室外机直接的高度差
        /// </summary>
        /// <param name="nodeIn"></param>
        /// <param name="curSystemItem"></param>
        /// <returns></returns>
        private bool ValIndoor_HeightDiff(MyNodeIn nodeIn, SystemVRF curSystemItem)
        {
            bool isTrue = true;
            double tempIDURealLength = 0;
            getRealLength(nodeIn, 0, ref tempIDURealLength);
            if (tempIDURealLength < nodeIn.RoomIndooItem.HeightDiff)
            {
                setLinkPathColor(nodeIn, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                TempActualLength = tempIDURealLength;
                TempMaxLength = nodeIn.RoomIndooItem.HeightDiff;
                isTrue = false;
            }
            return isTrue;
        }
        private void CHbox_IndoorErrorLinks(SystemVRF curSystemItem, MyNode nodeCH, double tempCHRealLength, MyNodeIn nodeIn)
        {
            DrawNodeErrorLinks(nodeIn, curSystemItem);  //绘制节点到CHbox 链接线  
            setLinkPathColor(nodeCH, utilPiping.colorDefault, curSystemItem.OutdoorItem.ProductType);
            TempActualLength = Math.Abs(TempActualLength);

        }

        //绘制错误节点
        public int DrawNodeErrorLinks(Node node, SystemVRF curSystemItem)
        {
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                setLinkPathColor(nodeOut, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                return DrawNodeErrorLinks(nodeOut.ChildNode, curSystemItem);
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                setLinkPathColor(nodeCH, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                return DrawNodeErrorLinks(nodeCH.ChildNode, curSystemItem);
            }
            else if (node is MyNodeMultiCH)
            {
                int sum = 0;
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    setLinkPathColor(child, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                    sum += DrawNodeErrorLinks(child, curSystemItem);
                }
                return sum;
            }
            else if (node is MyNodeYP)
            {
                int sum = 0;
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    setLinkPathColor(child, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                    sum += DrawNodeErrorLinks(child, curSystemItem);
                }
                return sum;
            }
            else if (node is MyNodeIn)
            {
                setLinkPathColor(node, utilPiping.colorWarning, curSystemItem.OutdoorItem.ProductType);
                return 1;
            }
            return 0;
        }

        //Took latest code from legacy
        private bool ValCHbox_IndoorLength(MyNodeIn nodeIn, double chHighDiff, double ch_ODU_Length, ref double len, ref double height)
        {
            if (nodeIn == null)
                return false;
            double IDU_ODU_Length = 0; //IDU到ODU的管长
            getRealLength(nodeIn, ref IDU_ODU_Length);
            double IDU_CHBox_Length = Math.Abs(IDU_ODU_Length - ch_ODU_Length);  //IDU到CHBox的管长
            double IDU_ODU_HighDiff = 0.0;   //IDU到ODU的高度差
            double ch_indLength = CHBox_IndoorLength(nodeIn, ref IDU_ODU_HighDiff);
            double highDiff = Math.Abs(IDU_ODU_HighDiff - chHighDiff); //IDU到CHBox的高度差
            if (IDU_CHBox_Length < highDiff)
            {
                len = IDU_CHBox_Length;
                height = highDiff;
                return false;
            }
            return true;
        }


        /// <summary>
        /// 验证CHbox到OldModel.Indoor 链接线长度 
        /// </summary>
        /// <param name="nodeIn">室内机节点</param>
        /// <param name="HeightDiff">CHbox 或MultiBox 的高度差</param>
        /// <param name="length">室外机到需要验证到的节点长度</param>
        /// <returns></returns>
        private bool ValCHboxToIndoorLinks(MyNodeIn nodeIn, double HeightDiff, double length, ref double len, ref double height)
        {
            if (nodeIn == null)
                return false;
            double ch_indLength = 0.0;
            double Inddiff = 0.0;
            ch_indLength = CHBox_IndoorLength(nodeIn, ref Inddiff);
            ch_indLength = ch_indLength + length;
            if (ch_indLength > 0)
            {

                double highDiff = Math.Abs(Inddiff - HeightDiff); //取绝对值
                len = ch_indLength;
                height = highDiff;
                if (ch_indLength < highDiff)
                {
                    DrawCHboxToIndoorLinksColor(nodeIn);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 绘制CHbox到OldModel.Indoor 链接线 
        /// </summary>
        /// <param name="nodeIn">室内机节点</param>
        private void DrawCHboxToIndoorLinksColor(MyNodeIn nodeIn)
        {
            if (nodeIn == null)
                return;
            if (nodeIn.MyInLinks != null || nodeIn.Links.Count == 0)
            {
                Link lk = nodeIn.Links[0];
                if (lk is MyLink)
                {
                    MyLink link = (MyLink)lk;
                    link.Stroke = utilPiping.colorWarning;
                }
            }
        }




        /// <summary>
        /// 获取CHBox 到OldModel.Indoor 的长度
        /// </summary>
        /// <param name="nodeIn">室内机节点</param>
        /// <param name="Inddiff">返回室内机的高度差</param>
        /// <returns></returns>
        private double CHBox_IndoorLength(MyNodeIn nodeIn, ref double Inddiff)
        {
            double length = 0.0;
            if (nodeIn == null)
                return length;
            if (nodeIn.MyInLinks != null || nodeIn.MyInLinks.Count == 0)
            {
                Link lk = nodeIn.MyInLinks[0];
                if (lk is MyLink)
                {
                    MyLink link = (MyLink)lk;
                    length = link.Length;
                }
            }
            Inddiff = nodeIn.RoomIndooItem.HeightDiff;
            if (nodeIn.RoomIndooItem.PositionType == OldModel.PipingPositionType.Lower.ToString())
            {
                Inddiff = -nodeIn.RoomIndooItem.HeightDiff;
            }
            return length;
        }


        private double getMultiBox_IndoorHeightDiff(MyNodeIn nodeIn, MyNodeMultiCH nodeMu)
        {
            double length = 0.0;
            if (nodeIn is MyNodeIn)
            {
                length = nodeIn.RoomIndooItem.HeightDiff;
                if (nodeMu is MyNodeMultiCH)
                {
                    length = Math.Abs(length - Math.Abs(nodeMu.HeightDiff));
                }
            }
            return length;
        }

        /// 获取配管图中某个节点连接的IDU数量(递归) （提高了性能） Add on 20170511 by Shen Junjie
        /// <summary>
        /// 获取配管图中某个节点连接的IDU数量(递归) （提高了性能）
        /// </summary>
        /// <param name="pnode"></param>
        /// <param name="addFlowPiping"></param>
        /// <returns></returns>
        public int getIndoorCount(Node node)
        {
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                return getIndoorCount(nodeOut.ChildNode);
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                return getIndoorCount(nodeCH.ChildNode);
            }
            else if (node is MyNodeMultiCH)
            {
                int sum = 0;
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    sum += getIndoorCount(child);
                }
                return sum;
            }
            else if (node is MyNodeYP)
            {
                int sum = 0;
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    sum += getIndoorCount(child);
                }
                return sum;
            }
            else if (node is MyNodeIn)
            {
                return 1;
            }
            return 0;
        }

        /// 计算及校验液管总长 add on 20160511 by Yunxiao Lin
        /// <summary>
        /// 计算及校验液管总长
        /// </summary>
        public bool ValTotalLquidLength(SystemVRF sysItem)
        {
            double total_length = sysItem.TotalPipeLength;
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            if (sysItem.MaxTotalPipeLength > 0 && total_length > sysItem.MaxTotalPipeLength)//将参考Max value改为System属性 20170704 by Yunxiao Lin
            {
                setDownwardLinkPathColor(sysItem.MyPipingNodeOut, utilPiping.colorWarning);
                return false;
            }
            return true;
        }

        /// 计算液管总长 add on 2015060511 by Yunxiao Lin
        /// 包含室外机机组内部管长  add on 20170720 by Shen Junjie
        /// <summary>
        /// 计算液管总长
        /// </summary>
        public double getTotalLiquidLength(AddFlow addFlowPiping)
        {
            double total_length = 0.0d;
            foreach (Node node in addFlowPiping.Nodes(true))
            {
                if (node is MyNodeOut)
                {
                    //加上室外机机组内部管长  add on 20170720 by Shen Junjie
                    MyNodeOut nodeOut = node as MyNodeOut;
                    if (nodeOut.PipeLengthes != null)
                    {
                        for (int i = 0; i < nodeOut.PipeLengthes.Length; i++)
                        {
                            total_length += nodeOut.PipeLengthes[i];
                        }
                    }
                }
                else if (node is MyNode)
                {
                    foreach (MyLink link in (node as MyNode).MyInLinks)
                    {
                        total_length += link.Length;
                    }
                }
            }
            return total_length;
        }

        /// <summary>
        /// 获取最长的L3
        /// </summary>
        /// <param name="nodeStart"></param>
        /// <param name="nodeEnd"></param>
        /// <returns></returns>
        private void GetPipeLengthMaxMKToIndoor(Node parent, Node node, bool underCHBox, out Node nodeStart, out Node nodeEnd, out double L3)
        {
            L3 = 0;
            nodeStart = null;
            nodeEnd = null;
            if (node == null)
            {
                return;
            }

            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                GetPipeLengthMaxMKToIndoor(nodeOut, nodeOut.ChildNode, false, out nodeStart, out nodeEnd, out L3);
                if (nodeStart == null)
                {
                    L3 = 0;
                }
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    double childL3 = 0;
                    Node nodeStart1;
                    Node nodeEnd1;
                    GetPipeLengthMaxMKToIndoor(nodeYP, nodeYP.ChildNodes[i], underCHBox, out nodeStart1, out nodeEnd1, out childL3);

                    if (underCHBox)
                    {
                        if (childL3 > L3)
                        {
                            L3 = childL3;
                            nodeEnd = nodeEnd1;
                        }
                    }
                    else
                    {
                        if (nodeStart1 == null)
                        {
                            nodeStart1 = nodeYP;
                        }
                        if (nodeStart1 != null && childL3 > L3)
                        {
                            L3 = childL3;
                            nodeStart = nodeStart1;
                            nodeEnd = nodeEnd1;
                        }
                    }
                }
                if (underCHBox)
                {
                    L3 = L3 + nodeYP.MyInLinks[0].Length;
                }
            }
            else if (node is MyNodeCH)
            {
                double childL3 = 0;
                MyNodeCH nodeCH = node as MyNodeCH;
                GetPipeLengthMaxMKToIndoor(nodeCH, nodeCH.ChildNode, true, out nodeStart, out nodeEnd, out childL3);
                L3 = nodeCH.MyInLinks[0].Length + childL3;
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    double childL3 = 0;
                    Node nodeEnd1;
                    GetPipeLengthMaxMKToIndoor(nodeMCH, nodeMCH.ChildNodes[i], true, out nodeStart, out nodeEnd1, out childL3);
                    if (childL3 > L3)
                    {
                        L3 = childL3;
                        nodeEnd = nodeEnd1;
                    }
                }
                if (!(parent is MyNodeOut))
                {
                    nodeStart = node;
                }
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                L3 = nodeIn.MyInLinks[0].Length;
                nodeEnd = nodeIn;
            }
        }

        /// 计算并检验分歧管到室内机最大管长L3 add on 20160511 by Yunxiao Lin
        /// <summary>
        /// 计算并检验分歧管到室内机最大管长L3
        /// </summary>
        public bool ValMaxMKToIndoorLength(SystemVRF sysItem, AddFlow addFlowPiping, bool hadError)
        {
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            double maxlength = 0.0d;
            Node maxyp = null;
            Node maxin = null;

            GetPipeLengthMaxMKToIndoor(null, sysItem.MyPipingNodeOut, false, out maxyp, out maxin, out maxlength);

            sysItem.ActualMaxMKIndoorPipeLength = maxlength;
            if (!hadError && maxlength > 0 && maxyp != null && maxin != null)
            {
                //需要根据indoor建议连接数做不同判断
                if (sysItem.MaxMKIndoorPipeLength > 0 && maxlength > sysItem.MaxMKIndoorPipeLength) //将参考Max value改为System属性 20170704 by Yunxiao Lin
                {
                    //将indoor到yp的连接线设为警告色
                    setUpwardToNodeLinPathColor(maxin, maxyp, utilPiping.colorWarning);
                    return false;
                }
            }
            return true;
        }

        /// 多台室外机组成机组时，校验第一分歧管到第一Piping Connection kit之间的管长不能大于0.5m add on 20170720 by Shen Junjie
        /// <summary>
        /// 多台室外机组成机组时，校验第一分歧管到第一Piping Connection kit之间的管长不能大于0.5m
        /// </summary>
        public bool Val1stBranchTo1stCKLength(SystemVRF sysItem)
        {
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            if (nodeOut != null && nodeOut.UnitCount > 1)
            {
                if (nodeOut.ChildNode is MyNodeYP)
                {
                    MyNodeYP nodeYP = nodeOut.ChildNode as MyNodeYP;
                    foreach (MyLink link in nodeYP.MyInLinks)
                    {
                        if (link != null && link.Length < 0.5)
                        {
                            //将yp的连接线设为警告色
                            link.Stroke = utilPiping.colorWarning;
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// 计算并验证CH-Box下端所有管长总和 add on 20160515 by Yunxiao Lin
        /// <summary>
        /// 计算并验证CH-Box下端所有管长总和，本函数只能在配管计算得到CH-Box型号之后执行
        /// </summary>
        public bool ValCHToIndoorMaxTotalLength(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            int IndoorCount = getIndoorCount(sysItem.MyPipingNodeOut);
            Node nodeCH = null;
            double maxlength = 0.0d;
            double MaxTotalCHIndoorPipeLength_Std = 0;
            foreach (Node node in addFlowPiping.Nodes(true))
            {
                if (node is MyNodeCH)
                {
                    MyNodeCH ch = node as MyNodeCH;
                    if (maxlength < ch.ActualTotalCHIndoorPipeLength)
                    {
                        maxlength = ch.ActualTotalCHIndoorPipeLength;
                        nodeCH = ch;
                        if (IndoorCount <= outItem.RecommendedIU)
                        {
                            MaxTotalCHIndoorPipeLength_Std = ch.MaxTotalCHIndoorPipeLength;
                        }
                        else
                        {
                            MaxTotalCHIndoorPipeLength_Std = ch.MaxTotalCHIndoorPipeLength_MaxIU;
                        }
                        break;
                    }
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH mch = node as MyNodeMultiCH;
                    if (maxlength < mch.ActualTotalCHIndoorPipeLength)
                    {
                        maxlength = mch.ActualTotalCHIndoorPipeLength;
                        nodeCH = mch;
                        if (IndoorCount <= outItem.RecommendedIU)
                        {
                            MaxTotalCHIndoorPipeLength_Std = mch.MaxTotalCHIndoorPipeLength;
                        }
                        else
                        {
                            MaxTotalCHIndoorPipeLength_Std = mch.MaxTotalCHIndoorPipeLength_MaxIU;
                        }
                        break;
                    }
                }
            }
            //根据室内机数量进行判断
            if (maxlength > 0 && nodeCH != null)
            {
                if (MaxTotalCHIndoorPipeLength_Std > 0 && maxlength > MaxTotalCHIndoorPipeLength_Std)
                {
                    MaxCHToIndoorTotalLength = MaxTotalCHIndoorPipeLength_Std;
                    //将CH_Box下方所有连接线设为警告色
                    setDownwardLinkPathColor(nodeCH, utilPiping.colorWarning);
                    return false;
                }
            }
            return true;
        }

        /// 获取某节点到每个OldModel.Indoor的管长总和，包含节点进管的长度--递归 add on 20160511 by Yunxiao Lin
        /// <summary>
        /// 获取某节点到每个OldModel.Indoor的管长总和，包含节点进管的长度--递归
        /// </summary>
        public double getTotalToIndoorLength(Node node)
        {
            if (node == null) return 0;

            double total_length = 0.0d;

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                foreach (MyLink link in myNode.MyInLinks)
                {
                    total_length += link.Length;
                }
            }

            if (node is MyNodeCH)
            {
                MyNodeCH ch = node as MyNodeCH;
                total_length += getTotalToIndoorLength(ch.ChildNode);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH mch = node as MyNodeMultiCH;
                foreach (Node child in mch.ChildNodes)
                {
                    total_length += getTotalToIndoorLength(child);
                }
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP yp = node as MyNodeYP;
                for (int i = 0; i < yp.ChildCount; i++)
                {
                    total_length += getTotalToIndoorLength(yp.ChildNodes[i]);
                }
            }
            return total_length;
        }

        /// 获取某节点下方Mainbranch的数量--递归 add on 20160515 by Yunxiao Lin
        /// <summary>
        /// 获取某节点下方Mainbranch的数量--递归
        /// </summary>
        public int getMainBranchCount(Node pnode)
        {
            int count = 0;
            if (pnode is MyNodeYP)
            {
                MyNodeYP yp = pnode as MyNodeYP;
                if (!yp.IsCP)
                {
                    if (yp.ChildNodes[0] is MyNodeYP && yp.ChildNodes[1] is MyNodeYP)
                    {
                        count++;
                        count += getMainBranchCount(yp.ChildNodes[0]);
                        count += getMainBranchCount(yp.ChildNodes[1]);
                    }
                    else
                    {
                        for (int i = 0; i < yp.ChildCount; i++)
                        {
                            count += getMainBranchCount(yp.ChildNodes[i]);
                        }
                    }
                }
            }
            else if (pnode is MyNodeCH)
            {
                MyNodeCH ch = pnode as MyNodeCH;
                count += getMainBranchCount(ch.ChildNode);
            }
            else if (pnode is MyNodeMultiCH)
            {
                MyNodeMultiCH mch = pnode as MyNodeMultiCH;
                for (int i = 0; i < mch.ChildNodes.Count; i++)
                {
                    count += getMainBranchCount(mch.ChildNodes[i]);
                }
            }
            return count;
        }

        public static bool IsMainBranch(Node node)
        {
            if (node == null) return false;
            if (node is MyNodeYP)
            {
                MyNodeYP yp = node as MyNodeYP;
                if (!yp.IsCP)
                {
                    if (yp.ChildNodes[0] is MyNodeYP && yp.ChildNodes[1] is MyNodeYP)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        ///// 计算某YP下CoolingCapacity总和 add on 20160515 by Yunxiao Lin
        ///// <summary>
        ///// 计算某YP下CoolingCapacity总和
        ///// </summary>
        //public double getSumCoolingCapacity(MyNodeYP yp, AddFlow addFlowPiping)
        //{
        //    double sc = 0.0d;
        //    foreach (Node node in addFlowPiping.Nodes)
        //    {
        //        if (node is MyNodeIn && utilPiping.isContain(yp, node))
        //            //sc += (node as MyNodeIn).RoomIndooItem.CoolingCapacity;
        //            //排管计算改用室内机的标准容量 20161116 by Yunxiao Lin
        //            sc += (node as MyNodeIn).RoomIndooItem.IndoorItem.CoolingCapacity;
        //    }
        //    return sc;
        //}

        ///// 计算某YP下HeatingCapacity总和 add on 20160515 by Yunxiao Lin
        ///// <summary>
        ///// 计算某YP下HeatingCapacity总和
        ///// </summary>
        //public double getSumHeatingCapacity(MyNodeYP yp, AddFlow addFlowPiping)
        //{
        //    double sc = 0.0d;
        //    foreach (Node node in addFlowPiping.Nodes)
        //    {
        //        if (node is MyNodeIn && utilPiping.isContain(yp, node))
        //            //sc += (node as MyNodeIn).RoomIndooItem.HeatingCapacity;
        //            //排管计算改用室内机的标准容量 20161116 by Yunxiao Lin
        //            sc += (node as MyNodeIn).RoomIndooItem.IndoorItem.HeatingCapacity;
        //    }
        //    return sc;
        //}

        public void GetSumCapacity(Node node)
        {
            double sumCool = 0, sumHeat = 0, sumHP = 0;
            //GetSumCapacity(node, out sumCool, out sumHeat);
            GetSumCapacity(node, out sumCool, out sumHeat, out sumHP);
        }

        //public void GetSumCapacity(Node node, out double sumCool, out double sumHeat)
        private void GetSumCapacity(Node node, out double sumCool, out double sumHeat, out double sumHP)
        {
            sumCool = 0;
            sumHeat = 0;
            sumHP = 0;  //MutiChbox不需要特殊处理 
            if (node == null) return;

            double sumCool1 = 0, sumHeat1 = 0, sumHP1 = 0;
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                GetSumCapacity(nodeOut.ChildNode, out sumCool, out sumHeat, out sumHP);
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;

                sumHP = nodeIn.RoomIndooItem.IndoorItem.Horsepower;
                sumCool = nodeIn.RoomIndooItem.IndoorItem.CoolingCapacity;
                sumHeat = nodeIn.RoomIndooItem.IndoorItem.HeatingCapacity;
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    //GetSumCapacity(child, out sumCool1, out sumHeat1);
                    GetSumCapacity(child, out sumCool1, out sumHeat1, out sumHP1);
                    sumCool += sumCool1;
                    sumHeat += sumHeat1;
                    sumHP += sumHP1;
                }
                nodeYP.CoolingCapacity = sumCool;
                nodeYP.HeatingCapacity = sumHeat;
                nodeYP.HorsePower = sumHP;
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                //GetSumCapacity(nodeCH.ChildNode, out sumCool, out sumHeat);
                GetSumCapacity(nodeCH.ChildNode, out sumCool, out sumHeat, out sumHP);
                nodeCH.CoolingCapacity = sumCool;
                nodeCH.HeatingCapacity = sumHeat;
                nodeCH.HorsePower = sumHP;
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    //GetSumCapacity(child, out sumCool1, out sumHeat1);                    
                    GetSumCapacity(child, out sumCool1, out sumHeat1, out sumHP1);
                    sumCool += sumCool1;
                    sumHeat += sumHeat1;
                    sumHP += sumHP1;
                }
                nodeMCH.CoolingCapacity = sumCool;
                nodeMCH.HeatingCapacity = sumHeat;
                nodeMCH.HorsePower = sumHP;
            }
        }

        public void CalcPipingLimitValues(SystemVRF sysItem, AddFlow addFlowItem)
        {
            sysItem.PipeActualLength = 0;
            if (!sysItem.IsInputLengthManually)
            {
                //同一个OldModel.Project下面可能有多个ProductType，所以需要用室外机的属性来判断 20160825 by Yunxiao lin
                bool isHR = IsHeatRecovery(sysItem);
                foreach (Node nd in addFlowItem.Nodes(true))
                {
                    //如果排管存在错误，不恢复正常颜色。 modify on 20160727 by Yunxiao lin
                    if (nd is MyNodeOut || (nd.Links.Count > 0 &&
                        (nd.Links[0].Stroke != utilPiping.colorWarning || sysItem.IsPipingOK)))
                    {
                        utilPiping.setItemDefault(nd, isHR);
                    }
                }

                sysItem.ActualMaxMKIndoorPipeLength = 0;
                EachNode(sysItem.MyPipingNodeOut, (node1) =>
                {
                    if (node1 is MyNodeCH)
                    {
                        var item = (MyNodeCH)node1;
                        item.ActualTotalCHIndoorPipeLength = 0;
                    }
                });

                return;
            }

            sysItem.FirstPipeLength = 0;
            sysItem.PipeEquivalentLength = 0;

            double maxActualLength = 0;
            double maxEqLength = 0;
            double totalChildActualLength = 0;
            GetSumEqAndActualLength(sysItem, sysItem.MyPipingNodeOut, out maxActualLength, out maxEqLength, out totalChildActualLength);

            //实际总管长
            sysItem.PipeActualLength = maxActualLength;
            //估算总管长
            sysItem.PipeEquivalentLength = maxEqLength;

            //计算液管总长
            double total_length = getTotalLiquidLength(addFlowItem);
            sysItem.TotalPipeLength = total_length;
        }


        public void SetPipingLimitation(SystemVRF sysItem)
        {
            if (sysItem == null) return;
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            if (outItem == null) return;
            Node nodeOut = sysItem.MyPipingNodeOut;

            sysItem.MaxPipeLength = outItem.MaxPipeLength;
            sysItem.MaxEqPipeLength = outItem.MaxEqPipeLength;
            sysItem.MaxOutdoorAboveHeight = outItem.MaxOutdoorAboveHeight;
            sysItem.MaxOutdoorBelowHeight = outItem.MaxOutdoorBelowHeight;
            sysItem.MaxDiffIndoorHeight = outItem.MaxDiffIndoorHeight;
            sysItem.MaxIndoorLength = outItem.MaxIndoorLength;
            sysItem.MaxPipeLengthwithFA = outItem.MaxPipeLengthwithFA;
            sysItem.MaxDiffIndoorLength = outItem.MaxDiffIndoorLength;
            //增加系统液管总长上限变量，用于兼容IVX选型 20170704 by Yunxiao lin
            sysItem.MaxTotalPipeLength = outItem.MaxTotalPipeLength;
            sysItem.MaxMKIndoorPipeLength = outItem.MaxMKIndoorPipeLength;

            //获取室内机数量
            int indoorCount = 0;
            if (nodeOut != null)
            {
                indoorCount = getIndoorCount(nodeOut);
            }
            if (indoorCount > outItem.RecommendedIU)
            {
                sysItem.MaxIndoorLength = outItem.MaxIndoorLength_MaxIU;
                sysItem.MaxTotalPipeLength = outItem.MaxTotalPipeLength_MaxIU;
                sysItem.MaxMKIndoorPipeLength = outItem.MaxMKIndoorPipeLength_MaxIU;
                if (outItem.MaxTotalPipeLength_MaxIU > 0 && outItem.MaxIU > outItem.RecommendedIU)
                {
                    //SET FREE SIGMA series (RAS-FSXNSE and RAS-FSXNPE) 使用线性方程计算最大管长限制 add by Shen Junjie on 2019/06/19
                    switch (sysItem.Series)
                    {
                        case "Commercial VRF HP, FSXNSE":
                        case "Commercial VRF HR, FSXNSE":
                        case "Commercial VRF HP, FSXNPE":
                        case "Commercial VRF HR, FSXNPE":
                            sysItem.MaxTotalPipeLength = outItem.MaxTotalPipeLength + (outItem.MaxTotalPipeLength_MaxIU - outItem.MaxTotalPipeLength) / (outItem.MaxIU - outItem.RecommendedIU) * (indoorCount - outItem.RecommendedIU);
                            break;
                    }
                }
            }
            string series = sysItem.Series;

            if (series.Contains("IVX"))
            {
                //IVX系统根据环境温度的不同，管长上限会变化 20170704 by Yunxiao lin
                if (series.Contains("H(R/Y)NM1Q")) //目前仅有HAPQ的H(R/Y)NM1Q需要改变管长上限 20170704 by Yunxiao Lin
                {
                    if (sysItem.DBCooling <= -10)
                    {
                        switch (outItem.Model_Hitachi)
                        {
                            case "RAS-3HRNM1Q":
                                sysItem.MaxPipeLength = 30;
                                sysItem.MaxTotalPipeLength = 40;
                                //sysItem.MaxTotalPipeLength_MaxIU = 40;
                                sysItem.MaxIndoorLength = 10;
                                sysItem.MaxMKIndoorPipeLength = 5;
                                //sysItem.MaxMKIndoorPipeLength_MaxIU = 5;
                                break;
                            case "RAS-4HRNM1Q":
                            case "RAS-5HRNM1Q":
                            case "RAS-5HYNM1Q":
                                sysItem.MaxPipeLength = 40;
                                sysItem.MaxTotalPipeLength = 40;
                                //sysItem.MaxTotalPipeLength_MaxIU = 40;
                                sysItem.MaxIndoorLength = 10;
                                sysItem.MaxMKIndoorPipeLength = 5;
                                //sysItem.MaxMKIndoorPipeLength_MaxIU = 5;
                                break;
                        }
                    }
                    else
                    {
                        switch (outItem.Model_Hitachi)
                        {
                            case "RAS-3HRNM1Q":
                                sysItem.MaxPipeLength = 30;
                                sysItem.MaxTotalPipeLength = 40;
                                //sysItem.MaxTotalPipeLength_MaxIU = 40;
                                sysItem.MaxIndoorLength = 10;
                                sysItem.MaxMKIndoorPipeLength = 5;
                                //sysItem.MaxMKIndoorPipeLength_MaxIU = 5;
                                break;
                            case "RAS-4HRNM1Q":
                            case "RAS-5HRNM1Q":
                            case "RAS-5HYNM1Q":
                                sysItem.MaxPipeLength = 50;
                                sysItem.MaxTotalPipeLength = 60;
                                //sysItem.MaxTotalPipeLength_MaxIU = 60;
                                sysItem.MaxIndoorLength = 20;
                                sysItem.MaxMKIndoorPipeLength = 10;
                                //sysItem.MaxMKIndoorPipeLength_MaxIU = 10;
                                break;
                        }
                    }
                }
            }

            bool hasHeaderBranch = false;
            if (nodeOut != null)
            {
                EachNode(nodeOut, (n1) =>
                {
                    if (n1 is MyNodeYP)
                    {
                        MyNodeYP nodeYP = n1 as MyNodeYP;
                        if (nodeYP.IsCP)
                        {
                            hasHeaderBranch = true;
                        }
                    }
                });
            }
            if (hasHeaderBranch)
            {
                //When using header branch, L2 (from the first branch to the farthest indoor unit) is within 40m.
                sysItem.MaxIndoorLength = 40;
            }
        }

        public void GetSumEqAndActualLength(SystemVRF sysItem, Node node, out double maxActualLength, out double maxEqLength, out double totalChildActualLength)
        {
            maxActualLength = 0;
            maxEqLength = 0;
            totalChildActualLength = 0;
            if (node == null) return;
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                GetSumEqAndActualLength(sysItem, nodeOut.ChildNode, out maxActualLength, out maxEqLength, out totalChildActualLength);
                return;
            }

            //if (node.InLinks == null || node.InLinks.Count == 0) return;
            //Link lk = node.InLinks[0];
            //if (!(lk is MyLink)) return;
            //MyLink link = (MyLink)lk;
            //double actualLength = link.Length;
            //double eqLength = actualLength;
            MyNode myNode = node as MyNode;
            if (myNode.MyInLinks == null || myNode.MyInLinks.Count == 0) return;
            Link lk = myNode.MyInLinks[0];
            if (!(lk is MyLink)) return;
            MyLink link = (MyLink)lk;
            //maxActualLength += link.Length;
            //maxEqLength += link.Length;
            double actualLength = link.Length;
            double eqLength = actualLength;



            //以气管管径计算
            string gasPipeSize = "";
            if (!String.IsNullOrEmpty(link.SpecG_h) && link.SpecG_h != "-")
            {
                gasPipeSize = link.SpecG_h;
            }
            else if (!String.IsNullOrEmpty(link.SpecG_l) && link.SpecG_l != "-") //当找不到高压气管时，以低压气管计算
            {
                gasPipeSize = link.SpecG_l;
            }
            if (!string.IsNullOrEmpty(gasPipeSize))
            {
                double elbowL = link.ElbowQty == 0 ? 0 : GetElbowLength(Math.Round(link.ElbowQty), gasPipeSize);
                double oilL = link.OilTrapQty == 0 ? 0 : GetOilLength(Math.Round(link.OilTrapQty), gasPipeSize);
                eqLength += elbowL + oilL;
            }

            double childActualLength = 0, childEqLength = 0;
            double maxChildActualLength = 0, maxChildEqLength = 0;
            double totalChildActualLength1 = 0;
            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    GetSumEqAndActualLength(sysItem, child, out childActualLength, out childEqLength, out totalChildActualLength1);
                    maxChildActualLength = Math.Max(maxChildActualLength, childActualLength);
                    maxChildEqLength = Math.Max(maxChildEqLength, childEqLength);
                    totalChildActualLength += totalChildActualLength1;
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                GetSumEqAndActualLength(sysItem, nodeCH.ChildNode, out childActualLength, out childEqLength, out totalChildActualLength1);
                maxChildActualLength = childActualLength;
                maxChildEqLength = childEqLength;
                totalChildActualLength += totalChildActualLength1;
                nodeCH.ActualTotalCHIndoorPipeLength = totalChildActualLength;
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    GetSumEqAndActualLength(sysItem, child, out childActualLength, out childEqLength, out totalChildActualLength1);
                    maxChildActualLength = Math.Max(maxChildActualLength, childActualLength);
                    maxChildEqLength = Math.Max(maxChildEqLength, childEqLength);
                    totalChildActualLength = Math.Max(totalChildActualLength, totalChildActualLength1);
                }
                nodeMCH.ActualTotalCHIndoorPipeLength = totalChildActualLength;
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                if (nodeYP.IsCP)
                {
                    eqLength += 1;
                }
                else
                {
                    eqLength += 0.5;
                }

                if (nodeYP.IsFirstYP)
                {
                    sysItem.FirstPipeLength = maxChildActualLength;
                }
            }

            maxActualLength = actualLength + maxChildActualLength;
            maxEqLength = eqLength + maxChildEqLength;
            totalChildActualLength += actualLength;
        }

        public PipingErrors ValMainBranchForBrazilWaterSource(Node node, int branchLevel, int mainBranchLevel)
        {
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                return ValMainBranchForBrazilWaterSource(nodeOut.ChildNode, 0, 0);
            }
            else if (node is MyNodeYP)
            {
                branchLevel++;
                //判断是不是Main branch
                MyNodeYP yp = node as MyNodeYP;
                if (!yp.IsCP)
                {
                    bool isMainbranch = true;
                    for (int i = 0; i < yp.ChildCount; i++)
                    {
                        if (!(yp.ChildNodes[i] is MyNodeYP))
                        {
                            //Main Branch的子节点全都是YP或CP
                            isMainbranch = false;
                            break;
                        }
                    }
                    if (isMainbranch)
                    {
                        mainBranchLevel++;
                    }
                    else
                    {
                        mainBranchLevel = 0;
                    }
                }

                if (mainBranchLevel > 2)
                {
                    //将mainbranch下方所有的路径都设为警戒色
                    setDownwardLinkPathColor(yp, utilPiping.colorWarning);
                    return PipingErrors._3RD_MAIN_BRANCH; // -15;
                }

                if (branchLevel > 3 && mainBranchLevel > 0)
                {
                    //将mainbranch下方所有的路径都设为警戒色
                    setDownwardLinkPathColor(yp, utilPiping.colorWarning);
                    return PipingErrors._4TH_BRANCH_NOT_MAIN_BRANCH; // -16;
                }

                for (int i = 0; i < yp.ChildCount; i++)
                {
                    PipingErrors childResult = ValMainBranchForBrazilWaterSource(yp.ChildNodes[i], branchLevel, mainBranchLevel);
                    if (childResult != PipingErrors.OK)
                    {
                        return childResult;
                    }
                }
            }
            return PipingErrors.OK; // 0;
        }

        /// 计算及检验MainBranch add on 20160515 by Yunxiao Lin
        /// <summary>
        /// 计算及检验MainBranch
        /// </summary>
        public PipingErrors ValMainBranch(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            string factoryCode = GetFactoryCode(sysItem);
            string model = sysItem.OutdoorItem.Model_Hitachi.Trim();

            //add by Shen Junjie on 2018/3/12
            if (Regex.IsMatch(model, "^RAS-[0-9\\.]+FSXN$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSXNQ$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSN6Q$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNA6Q$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSDNQ$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSCNY1Q$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSN(S7B|S5B)$"))
            {
                //Yes
            }
            else if (Regex.IsMatch(model, "^RAS-[0-9\\.]+FSVN1Q$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSYN1Q$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNMQ$") ||
                Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNAMQ$"))
            {
                //Cannot have 2 main branches
            }
            else if (Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNWB$"))
            {
                //Upto 2 main branches(see below picture)
            }
            else if (Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNMA$"))
            {
                //No (Can have 1st&2nd Main branch piping)
                return ValMainBranchForBrazilWaterSource(sysItem.MyPipingNodeOut, 0, 0);
            }
            else
            {
                //No
                return PipingErrors.OK;// 0;
            }


            //获取室内机数量
            OldModel.Outdoor outItem = sysItem.OutdoorItem;

            double L2 = sysItem.FirstPipeLength; //第一分歧管到最远端室内机的等效管长

            GetSumCapacity(sysItem.MyPipingNodeOut);

            //计算max piping length after main branch
            foreach (Node node in addFlowPiping.Nodes())
            {
                if (node is MyNodeYP)
                {
                    //判断是不是Main branch
                    MyNodeYP yp = node as MyNodeYP;
                    if (!yp.IsCP)
                    {
                        bool isMainbranch = true;
                        for (int i = 0; i < yp.ChildCount; i++)
                        {
                            if (!(yp.ChildNodes[i] is MyNodeYP))
                            {
                                isMainbranch = false;
                                break; //Main Branch的子节点全都是YP或CP
                            }
                        }
                        if (isMainbranch)
                        {
                            //add by Shen Junjie on 2018/3/9
                            int count = getMainBranchCount(yp);
                            MaxMainBranchCount = 0;
                            if (factoryCode == "Q")
                            {
                                if (Regex.IsMatch(model, "^RAS-[0-9\\.]+FSVN1Q$") ||
                                    Regex.IsMatch(model, "^RAS-[0-9\\.]+FSYN1Q$") ||
                                    Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNMQ$") ||
                                    Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNAMQ$"))
                                {
                                    //Cannot have 2 main branches
                                    MaxMainBranchCount = 1;
                                }
                            }
                            else if (factoryCode == "B")
                            {
                                if (Regex.IsMatch(model, "^RAS-[0-9\\.]+FSNWB$"))
                                {
                                    //Upto 2 main branches(see below picture)
                                    MaxMainBranchCount = 2;
                                }
                            }
                            if (MaxMainBranchCount > 0 && count > MaxMainBranchCount)
                            {
                                //将mainbranch下方所有的路径都设为警戒色
                                setDownwardLinkPathColor(yp, utilPiping.colorWarning);
                                return PipingErrors.MAINBRANCHCOUNT;// -9; //Main Branch数量超限
                            }
                            ////计算L2，每个系统只需要计算1次
                            //if (L2 < 0d)
                            //{
                            //    double maxEqLength = 0d;
                            //    double fEqLength = 0d;
                            //    Node fNode = null;
                            //    foreach (Node n in addFlowPiping.Nodes)
                            //    {
                            //        if (n is MyNodeIn)
                            //        {
                            //            double tempRealLength = 0;
                            //            double tempExtraLength = 0;

                            //            getExtraLength(n, 0, ref tempExtraLength);
                            //            getRealLength(n, 0, ref tempRealLength);

                            //            double tempEqLength = tempExtraLength + tempRealLength; //当前室内机对应的等效管长
                            //            if (tempEqLength > sysItem.MaxEqPipeLength)
                            //            {
                            //                //setLinkPathColor(n, utilPiping.colorWarning);
                            //                setLinkPathColor(n, utilPiping.colorWarning, sysItem.OutdoorItem.ProductType);
                            //                return PipingErrors.EQLENGTH;// -3;
                            //            }
                            //            //如果当前的最长等效管长值合法，则赋值给变量maxEqLength
                            //            if (maxEqLength < tempEqLength)
                            //                maxEqLength = tempEqLength;
                            //        }
                            //        else if (n is MyNodeYP && n.Links.Count > 0 && n.Links[0].Org is MyNodeOut)
                            //        {
                            //            fNode = n; //第一分歧管节点
                            //            double tempRealLength = 0;
                            //            getRealLength(fNode, 0, ref tempRealLength);   //此时的tempActualLength值为第一连接管的实际长度
                            //            double tempExtraLength = 0;
                            //            getExtraLength(fNode, 0, ref tempExtraLength);  //此时的tempExtraLength值为第一连接管的额外长度
                            //            fEqLength = tempRealLength + tempExtraLength;  //得到此时第一连接管的等效管长
                            //        }
                            //    }
                            //    L2 = maxEqLength - fEqLength;
                            //}
                            if (L2 <= 40d)
                            {
                                //当1st最大等效管长<=40m
                                //获取每个分支的总长度
                                double length1 = getTotalToIndoorLength(yp.ChildNodes[0]);
                                double length2 = getTotalToIndoorLength(yp.ChildNodes[1]);
                                if (length1 > 30 && length2 > 30)
                                {
                                    //判断下方的Mainbranch数量
                                    MaxMainBranchCount = 2;
                                    if (count > MaxMainBranchCount)
                                    {
                                        //将mainbranch下方所有的路径都设为警戒色
                                        setDownwardLinkPathColor(yp, utilPiping.colorWarning);
                                        return PipingErrors.MAINBRANCHCOUNT;// -9; //Main Branch数量超限
                                    }
                                }
                            }
                            else
                            {
                                //当1st最大等效管长>40m <=90m
                                //判断下方的Mainbranch数量
                                MaxMainBranchCount = 1;
                                if (count > MaxMainBranchCount)
                                {
                                    //将mainbranch下方所有的路径都设为警戒色
                                    setDownwardLinkPathColor(yp, utilPiping.colorWarning);
                                    return PipingErrors.MAINBRANCHCOUNT;// -9; //Main Branch数量超限
                                }
                                //计算OldModel.Indoor Capacity Rate，每个分支的容量比例不能小于40%
                                //for (int i = 0; i < yp.ChildCount; i++)  //没必要循环 delete by Shen Junjie on 2018/01/26
                                {
                                    //计算Cooling容量
                                    //double scc = getSumCoolingCapacity(yp, addFlowPiping);
                                    //double cc1 = getSumCoolingCapacity(yp.ChildNodes[0] as MyNodeYP, addFlowPiping);
                                    //double cc2 = getSumCoolingCapacity(yp.ChildNodes[1] as MyNodeYP, addFlowPiping);
                                    double scc = yp.CoolingCapacity;
                                    double cc1 = 0;
                                    double cc2 = 0;
                                    if (yp.ChildNodes[0] is MyNodeYP)
                                    {
                                        cc1 = (yp.ChildNodes[0] as MyNodeYP).CoolingCapacity;
                                    }
                                    if (yp.ChildNodes[1] is MyNodeYP)
                                    {
                                        cc2 = (yp.ChildNodes[1] as MyNodeYP).CoolingCapacity;
                                    }

                                    if (cc1 >= cc2)
                                    {
                                        if (cc2 / scc < 0.4d)
                                        {
                                            MinMainBranchCoolingCapacityRate = "40";
                                            //设置cc2所在的path颜色为警告色
                                            setDownwardLinkPathColor(yp.ChildNodes[1], utilPiping.colorWarning);
                                            return PipingErrors.COOLINGCAPACITYRATE;// -10; //冷容量比例不满40%
                                        }
                                    }
                                    else
                                    {
                                        if (cc1 / scc < 0.4d)
                                        {
                                            MinMainBranchCoolingCapacityRate = "40";
                                            //设置cc1所在的path颜色为警告色
                                            setDownwardLinkPathColor(yp.ChildNodes[0], utilPiping.colorWarning);
                                            return PipingErrors.COOLINGCAPACITYRATE; // -10; //冷容量比例不满40%
                                        }
                                    }
                                    ////再计算Heating容量
                                    //double shc = getSumHeatingCapacity(yp, addFlowPiping);
                                    //double hc1 = getSumHeatingCapacity(yp.ChildNodes[0] as MyNodeYP, addFlowPiping);
                                    //double hc2 = getSumHeatingCapacity(yp.ChildNodes[1] as MyNodeYP, addFlowPiping);

                                    //if (hc1 >= hc2)
                                    //{
                                    //    if (hc2 / shc < 0.4d)
                                    //    {
                                    //        MinMainBranchHeatingCapacityRate = "40";
                                    //        //设置hc2所在的path颜色为警告色
                                    //        setDownwardLinkPathColor(yp.ChildNodes[1], utilPiping.colorWarning);
                                    //        return -11; //热容量比例不满40%
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    if (hc1 / shc < 0.4d)
                                    //    {
                                    //        MinMainBranchHeatingCapacityRate = "40";
                                    //        //设置hc1所在的path颜色为警告色
                                    //        setDownwardLinkPathColor(yp.ChildNodes[0], utilPiping.colorWarning);
                                    //        return -11; //热容量比例不满40%
                                    //    }
                                    //}
                                }
                            }
                        }
                    }
                }
            }
            return PipingErrors.OK;// 0;
        }

        /// 获取室内机制冷容量总和 add on 20160518 by Yunxiao Lin
        /// <summary>
        /// 获取室内机制冷容量总和
        /// </summary>
        /// <param name="addFlowPiping"></param>
        /// <returns></returns>
        public double getTotalIndoorCoolingCapacity(AddFlow addFlowPiping)
        {
            double totalCapacity = 0.0d;
            foreach (Node node in addFlowPiping.Nodes())
            {
                if (node is MyNodeIn)
                {
                    totalCapacity += (node as MyNodeIn).RoomIndooItem.IndoorItem.CoolingCapacity;
                }
            }
            return totalCapacity;
        }

        /// 获取Cooling Only室内机的制冷容量总和 add on 20160518 by Yunxiao Lin
        /// <summary>
        /// 获取Cooling Only室内机的制冷容量总和
        /// </summary>
        /// <param name="addFlowPiping"></param>
        /// <returns></returns>
        public double getTotalCoolingOnlyIndoorCapacity(AddFlow addFlowPiping)
        {
            double totalCapacity = 0.0d;
            foreach (Node node in addFlowPiping.Nodes())
            {
                if (node is MyNodeIn)
                {
                    MyNodeIn indoor = node as MyNodeIn;
                    if (indoor.IsCoolingonly)
                    {
                        totalCapacity += indoor.RoomIndooItem.IndoorItem.CoolingCapacity;
                    }
                }
            }
            return totalCapacity;
        }

        /// <summary>
        /// 标注Cooling only线段颜色
        /// </summary>
        /// <param name="addFlowPiping"></param>
        /// <param name="color"></param>
        public static void setCoolingOnlyItemColor(AddFlow addFlowPiping, System.Windows.Media.Brush color)
        {
            foreach (Node node in addFlowPiping.Nodes())
            {
                if (node is MyNodeIn)
                {
                    MyNodeIn nodeIn = node as MyNodeIn;
                    if (nodeIn.IsCoolingonly)
                    {
                        foreach (MyLink link in nodeIn.MyInLinks)
                        {
                            link.Stroke = color;
                        }
                    }
                }
                else if (node is MyNodeYP)
                {
                    MyNodeYP nodeYP = node as MyNodeYP;
                    if (nodeYP.IsCoolingonly)
                    {
                        foreach (MyLink link in nodeYP.MyInLinks)
                        {
                            link.Stroke = color;
                        }
                    }
                }
            }

        }
        /// 计算并验证Cooling only室内机的容量占比不能超过50% add on 20160518 by Yunxiao Lin
        /// <summary>
        /// 计算并验证Cooling only室内机的容量占比不能超过50%
        /// </summary>
        /// <param name="sysItem"></param>
        /// <param name="addFlowPiping"></param>
        /// <returns></returns>
        public bool ValCoolingOnlyIndoorCapacityRate(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            double totalCapacity = getTotalIndoorCoolingCapacity(addFlowPiping);
            double totalCoolingOnlyCapacity = getTotalCoolingOnlyIndoorCapacity(addFlowPiping);
            if (totalCoolingOnlyCapacity / totalCapacity > 0.5d)
            {
                //将所有Coolingonly线段标注警告色
                setCoolingOnlyItemColor(addFlowPiping, utilPiping.colorWarning);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 计算并验证CH-Box后连接的OldModel.Indoor数量，本函数需要用到CH-Box允许最大连接数，所以必须放到配管计算之后执行
        /// </summary>
        /// <param name="sysItem"></param>
        /// <param name="addFlowPiping"></param>
        /// <returns></returns>
        public bool ValMaxIndoorNumberConnectToCH(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            foreach (Node node in addFlowPiping.Nodes())
            {
                int MaxIndoorCount = 0;
                if (node is MyNodeCH)
                {
                    MyNodeCH ch = node as MyNodeCH;
                    MaxIndoorCount = ch.MaxIndoorCount;
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH mch = node as MyNodeMultiCH;
                    MaxIndoorCount = mch.MaxIndoorCount;
                }
                if (MaxIndoorCount > 0)
                {
                    int IndoorNumber = 0;
                    EachNode(node, (child) =>
                    {
                        if (child is MyNodeIn)
                        {
                            IndoorNumber++;
                        }
                    });

                    if (IndoorNumber > MaxIndoorCount)
                    {
                        MaxIndoorNumberConnectToCH = MaxIndoorCount;
                        //将该CH下面的分支颜色设为警告色
                        setDownwardLinkPathColor(node, utilPiping.colorWarning);
                        return false;
                    }
                }
            }
            return true;
        }

        /// 计算及检验最长等效管长
        /// <summary>
        /// 注意：该方法需要在配管计算之后执行，因为要用到各个连接管的管径规格？？？？
        /// 1.验证当前系统最长等效管长合法性
        /// 2.取最长等效管长赋值给maxEqLength变量
        /// 等效管长（Equivalent piping length）= 实际管长（Actual length）+ 弯头数量（Elbow Qty）*弯头等效长（Equivalent length）+ 存油弯数量（Oil Bend Qty）*存油弯等效长（Equivalent length）
        /// + 分歧管数量（Ref. joints Qty）* 分歧管等效长（Equivalent length） [ 其中：Y型管等效长0.5m；梳型管等效长1m ]
        /// </summary>
        /// <returns></returns>
        public PipingErrors ValMaxEqLength(SystemVRF sysItem, AddFlow addFlowPiping)
        {
            //double maxEqLength = 0;
            //Node fNode = null;

            foreach (Node node in addFlowPiping.Nodes(true))
            {
                if (node is MyNodeIn)
                {
                    double tempRealLength = 0;
                    double tempExtraLength = 0;

                    getExtraLength(node, 0, ref tempExtraLength);
                    getRealLength(node, 0, ref tempRealLength);

                    double tempEqLength = tempExtraLength + tempRealLength; //当前室内机对应的等效管长
                    if (tempEqLength > sysItem.MaxEqPipeLength)
                    {
                        //setLinkPathColor(node, utilPiping.colorWarning);
                        setLinkPathColor(node, utilPiping.colorWarning, sysItem.OutdoorItem.ProductType);
                        return PipingErrors.EQLENGTH;// -3;
                    }
                    ////如果当前的最长等效管长值合法，则赋值给变量maxEqLength
                    //if (maxEqLength < tempEqLength)
                    //    maxEqLength = tempEqLength;

                    //L2的90米限制改用Actual length modified by Shen Junjie on 2018/07/27
                    //#region 验证第一分歧管到最远端的最长等效管长 20120113-clh-
                    //if (fNode != null)
                    //{
                    //    tempRealLength = 0;
                    //    getRealLength(fNode, 0, ref tempRealLength);   //此时的tempActualLength值为第一连接管的实际长度
                    //    tempExtraLength = 0;
                    //    getExtraLength(fNode, 0, ref tempExtraLength);  //此时的tempExtraLength值为第一连接管的额外长度
                    //    double fEqLength = tempRealLength + tempExtraLength;  //得到此时第一连接管的等效管长
                    //    //sysItem.FirstPipeLength = fEqLength;
                    //    double temp = maxEqLength - fEqLength;  //得到此时第一分歧管到最远端室内机的等效管长
                    //    //sysItem.FirstPipeLength = temp; //通过GetSumEqAndActualLength计算得到 deleted by Shen Junjie on 2018/3/29
                    //    if (temp > sysItem.MaxIndoorLength)
                    //    {
                    //        //将当前OldModel.Indoor节点到OldModel.Outdoor节点的路径标记为红色
                    //        //setLinkPathColor(node, utilPiping.colorWarning);
                    //        setLinkPathColor(node, utilPiping.colorWarning, sysItem.OutdoorItem.ProductType);
                    //        //将第一分歧管到OldModel.Outdoor的路径恢复为默认颜色
                    //        //setLinkPathColor(fNode, utilPiping.colorDefault);
                    //        setLinkPathColor(fNode, utilPiping.colorDefault, sysItem.OutdoorItem.ProductType);
                    //        return PipingErrors.FIRSTLENGTH; // -4;
                    //    }
                    //    //赋给当前系统对应的属性，update on 20140822 clh,手动输入的管长不赋值给系统对应属性
                    //    //curSystemItem.FirstPipeLength = temp;
                    //}
                    //#endregion
                }
                //else if (node is MyNodeYP && (node as MyNodeYP).ParentNode is MyNodeOut)
                //{
                //    fNode = node; //第一分歧管节点 
                //}
            }

            //赋给当前系统对应的属性,update on 20140822 clh,手动输入的管长不赋值给系统对应属性
            //sysItem.PipeEquivalentLength = maxEqLength; //通过GetSumEqAndActualLength计算得到 deleted by Shen Junjie on 2018/3/29

            return PipingErrors.OK; //0;
        }

        /// 递归计算，得到指定节点到室外机之间弯头与油弯总长
        /// <summary>
        /// 递归计算，得到指定节点到室外机之间弯头与油弯总长,在执行配管计算之后执行
        /// add on 20120905 clh : 之前少算了分歧管的长度，Y型管0.5m，梳型管1m
        /// </summary>
        /// <param name="node"></param>
        /// <param name="node">起始节点</param>
        /// <param name="length1">子节点累计长度</param>
        /// <param name="maxLength">所有路径的最大长度</param>
        private void getExtraLength(Node node, double length1, ref double maxLength)
        {
            if (node == null) return;
            if (node is MyNodeYP)
            {
                if ((node as MyNodeYP).IsCP)
                {
                    maxLength += 1;
                }
                else
                {
                    maxLength += 0.5;
                }
            }
            MyNode myNode = node as MyNode;
            if (myNode.MyInLinks == null || myNode.MyInLinks.Count == 0) return;
            Link lk = myNode.MyInLinks[0];
            if (!(lk is MyLink)) return;
            MyLink link = (MyLink)lk;
            double length2 = length1;

            //以气管管径计算
            string gasPipeSize;
            if (!String.IsNullOrEmpty(link.SpecG_h) && link.SpecG_h != "-")
            {
                gasPipeSize = link.SpecG_h;
            }
            else if (!String.IsNullOrEmpty(link.SpecG_l) && link.SpecG_l != "-") //当找不到高压气管时，以低压气管计算
            {
                gasPipeSize = link.SpecG_l;
            }
            else
            {
                return;
            }
            double elbowL = link.ElbowQty == 0 ? 0 : GetElbowLength(Math.Round(link.ElbowQty), gasPipeSize);
            double oilL = link.OilTrapQty == 0 ? 0 : GetOilLength(Math.Round(link.OilTrapQty), gasPipeSize);
            length2 += elbowL + oilL;
            maxLength = Math.Max(maxLength, length2);

            //if (node.Links != null && node.Links.Count > 0)
            //{
            //    Link newLink = node.Links[0];
            //    if (newLink.Org is MyNodeOut)
            //        return;
            //    else
            //        getExtraLength(newLink.Org, length2, ref maxLength);
            //}
            if (link.Org is MyNodeOut)
                return;
            else
                getExtraLength(link.Org, length2, ref maxLength);
        }


        /// 递归计算，得到指定节点到室外机的实际管长之和
        /// <summary>
        /// 递归计算，得到指定节点到室外机的实际管长之和
        /// </summary>
        /// <param name="node">起始节点</param>
        /// <param name="length1">子节点累计长度</param>
        /// <param name="maxLength">所有路径的最大长度</param>
        private void getRealLength(Node node, double length1, ref double maxLength)
        {
            if (node == null) return;
            MyNode myNode = node as MyNode;
            if (myNode.MyInLinks == null || myNode.MyInLinks.Count == 0) return;
            Link lk = myNode.MyInLinks[0];
            if (!(lk is MyLink)) return;
            MyLink link = (MyLink)lk;
            double length2 = length1 + link.Length;
            maxLength = Math.Max(maxLength, length2);
            if (node.Links != null && node.Links.Count > 0)
            {
                Link newLink = node.Links[0];
                if (newLink.Org is MyNodeOut)
                    return;
                else
                    getRealLength(newLink.Org, length2, ref maxLength);
            }
            //if (link.Org is MyNodeOut)
            //    return;
            //else
            //    getRealLength(lk.Org, length2, ref maxLength);
        }
        /// 将指定节点到室外机的路径标记为红色--递归
        /// 由于多ProductType功能，同一个OldModel.Project下面可能存在不同的ProductType，所以需要增加productType参数 20160825 by Yunxiao Lin
        /// <summary>
        /// 将指定节点到室外机的路径标记为红色--递归
        /// </summary>
        /// <param name="node"></param>
        /// <param name="color"></param>
        private void setLinkPathColor(Node node, System.Windows.Media.Brush color, string productType)
        {
            if (node is MyNode)
            {
                foreach (Link lk in (node as MyNode).MyInLinks)
                {
                    if (IsHeatRecovery(productType) && color == utilPiping.colorDefault)
                    {
                        //增加Heat Recovery系列默认颜色的处理 
                        utilPiping.setItemDefault(node, true);
                    }
                    else
                    {
                        lk.Stroke = color;
                    }
                    if (lk.Org != null)
                        setLinkPathColor(lk.Org, color, productType);
                }
            }
        }

        /// 将指定节点下端的所有路径标记颜色--递归 add on 20160515 by Yunxiao Lin
        /// <summary>
        /// 将指定节点下端的所有路径标记颜色--递归
        /// </summary>
        public void setDownwardLinkPathColor(Node node, System.Windows.Media.Brush color)
        {
            if (node == null) return;

            EachNode(node, (child) =>
            {
                if (node != child)
                {
                    if (child is MyNode)
                    {
                        MyNode myNode = child as MyNode;
                        foreach (MyLink link in myNode.Links)
                        {
                            link.Stroke = color;
                        }
                    }
                }
            });
        }

        /// 将指定节点向上到指定节点的路径标记颜色--递归 add on 20160515 by Yunxiao Lin
        /// <summary>
        /// 将指定节点向上到指定节点的路径标记颜色--递归
        /// </summary>
        public void setUpwardToNodeLinPathColor(Node nodefrom, Node nodeto, System.Windows.Media.Brush color)
        {
            if (nodefrom == nodeto) return;
            if (nodefrom is MyNode)
            {
                if ((nodefrom as MyNode).MyInLinks[0] != null)
                {
                    (nodefrom as MyNode).MyInLinks[0].Stroke = color;
                    if ((nodefrom as MyNode).MyInLinks[0].Org != null)
                    {
                        setUpwardToNodeLinPathColor((nodefrom as MyNode).MyInLinks[0].Org, nodeto, color);
                    }
                }
            }
        }

        /// 获取某规格连接管的等效总长，注意部分连接可能没有高压管，这时候取低压管 modify on 20160525 by Yunxiao Lin
        /// <summary>
        /// 获取某规格连接管的等效总长
        /// </summary>
        /// <param name="link">对象的引用</param>
        /// <returns></returns>
        public double GetLinkLength_EQ(MyLink link)
        {
            if (!string.IsNullOrEmpty(link.SpecG_h) && link.SpecG_h != "-")
                return link.Length
                    + GetElbowLength(Int32.Parse(link.ElbowQty.ToString()), link.SpecG_h)
                    + GetOilLength(Int32.Parse(link.OilTrapQty.ToString()), link.SpecG_h);
            else if (!string.IsNullOrEmpty(link.SpecG_l) && link.SpecG_l != "-")
                return link.Length
                    + GetElbowLength(Int32.Parse(link.ElbowQty.ToString()), link.SpecG_l)
                    + GetOilLength(Int32.Parse(link.OilTrapQty.ToString()), link.SpecG_l);
            else
                return link.Length;
        }

        /// 获取某规格连接管的弯头总长度
        /// <summary>
        /// 获取某规格连接管的弯头总长度(double)
        /// </summary>
        /// <param name="qty">数量</param>
        /// <param name="spec">连接管管径（气管）</param>
        /// <returns></returns>
        public double GetElbowLength(double qty, string spec)
        {
            if (_dal.ListPipeSize.Contains(spec.Trim()))
            {
                double value = _dal.ListElbowLength[_dal.ListPipeSize.IndexOf(spec.Trim())];
                return qty * value;
            }
            //MessageBox.Show("can not find pipe size: " + spec);
            return 0;
        }

        /// 获取某规格连接管的存油弯头的总长度
        /// <summary>
        /// 获取某规格连接管的存油弯头的总长度(double)
        /// </summary>
        /// <param name="qty">数量</param>
        /// <param name="spec">连接管管径（气管）</param>
        /// <returns></returns>
        public double GetOilLength(double qty, string spec)
        {
            if (_dal.ListPipeSize.Contains(spec.Trim()))
            {
                double value = _dal.ListOilLength[_dal.ListPipeSize.IndexOf(spec.Trim())];
                return qty * value;
            }
            //MessageBox.Show("can not find pipe size: " + spec);
            return 0;
        }

        #endregion


        #region 获取管长修正系数 TODO

        /// 计算系统的管长修正系数
        /// <summary>
        /// 计算系统的管长修正系数
        /// </summary>
        /// <returns></returns>
        public double GetPipeLengthFactor(SystemVRF sysItem, string condition)
        {
            return GetPipeLengthFactor(sysItem, sysItem.OutdoorItem, condition);
        }

        /// 计算系统的管长修正系数, 自动选型时使用
        /// <summary>
        /// 计算系统的管长修正系数, 自动选型时使用
        /// </summary>
        /// <returns></returns>
        public double GetPipeLengthFactor(SystemVRF sysItem, OldModel.Outdoor item, string condition)
        {
            if (item == null)
                return 0;

            //decimal ret = 1;
            string model = item.ModelFull;
            string fcode = model.Substring(model.Length - 1, 1);
            //int maxEqLength = (int)sysItem.PipeEquivalentLength;
            //int maxHighDiff = (int)Math.Round(sysItem.HeightDiff);
            double maxEqLength = sysItem.PipeEquivalentLength;
            double maxHighDiff = sysItem.HeightDiff;

            if (maxEqLength < maxHighDiff)
                return 0;

            //maxEqLength = maxEqLength / 10 * 10;
            //maxHighDiff = maxHighDiff / 10 * 10;

            if (maxEqLength == 0)
            {
                return 0;
            }

            string PipeType = "2Pipe";
            if (item.Series.Contains(" HR,"))
                PipeType = "3Pipe";

            return _dal.GetPipingLengthFactor(fcode, item.Type, item.Model_Hitachi, condition, maxHighDiff, maxEqLength, PipeType);
        }

        #endregion


        #region 计算冷媒追加量

        /// 制冷剂追加量计算
        /// <summary>
        /// 制冷剂追加量计算
        /// </summary>
        /// <returns></returns>
        public double GetAddRefrigeration(SystemVRF sysItem, ref AddFlow addflowItem)
        {
            if (sysItem != null)
            {
                Dictionary<string, double> dicSpecL = new Dictionary<string, double>();
                GatherPipeSpec(addflowItem, ref dicSpecL, sysItem);
                MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
                string outModel = sysItem.OutdoorItem.ModelFull;
                string factoryCode = outModel.Substring(outModel.Length - 1, 1);
                double ret = AddRefrigeration(sysItem, factoryCode, dicSpecL["22.2"], dicSpecL["19.05"], dicSpecL["15.88"], dicSpecL["12.7"], dicSpecL["9.52"], dicSpecL["6.35"], dicSpecL["25.4"], dicSpecL["28.58"], dicSpecL["Valve"], addflowItem);
                return ret;
            }
            else
                return 0;
        }

        /// 汇总各个连接管的液管管径及长度
        /// <summary>
        /// 汇总各个连接管的液管管径及长度
        /// </summary>
        public void GatherPipeSpec(AddFlow addflowItem, ref Dictionary<string, double> dicSpecL, SystemVRF sysItem)
        {
            if (dicSpecL.Keys.Count > 0)
                dicSpecL.Clear();

            foreach (string s in _dal.ListPipeSize)
            {
                dicSpecL.Add(s, 0);
            }
            dicSpecL.Add("Valve", 0);

            if (sysItem == null || sysItem.OutdoorItem == null)
                return;
            string fCode = GetFactoryCode(sysItem.OutdoorItem.FullModuleName);

            foreach (Node node in addflowItem.Items.OfType<Node>())
            {
                if (!(node is MyNode)) continue;

                if (node is MyNodeOut)
                {
                    MyNodeOut nodeOut = node as MyNodeOut;
                    if (nodeOut.PipeSize != null && nodeOut.PipeLengthes != null)
                    {
                        for (int i = 0; i < nodeOut.PipeSize.Length && i < nodeOut.PipeLengthes.Length; i++)
                        {
                            string pipeSizeGroup = nodeOut.PipeSize[i];
                            string[] aa = pipeSizeGroup.Split('x');
                            if (aa.Length == 2 || aa.Length == 3)
                            {
                                string liquidPipeSize = aa[aa.Length - 1].Trim();
                                if (_dal.ListPipeSize.Contains(liquidPipeSize))
                                {
                                    dicSpecL[liquidPipeSize] += nodeOut.PipeLengthes[i];
                                }
                            }
                        }
                    }
                }

                MyNode myNode = node as MyNode;
                //需要排除标注和图例Node Modify on 20160517 by Yunxiao Lin
                foreach (MyLink link in myNode.MyInLinks)
                {
                    if (link == null) continue;

                    if (_dal.ListPipeSize.Contains(link.SpecL))
                    {
                        //use actual pipe length instead of Equivalent pipe length for all units.  modify by Shen Junjie on 2018/3/20
                        //if (fCode != "S")
                        //{
                        //    double eqLength = GetLinkLength_EQ(link);
                        //    dicSpecL[link.SpecL.Trim()] += eqLength;    //液管的等效总长
                        //}
                        //else
                        //{
                        //SMZ的追加制冷剂计算用实际管长 20180208 by Yunxiao Lin
                        dicSpecL[link.SpecL.Trim()] += link.Length;
                        //}
                    }

                    //增加电子膨胀管的计算 20160616 by Yunxiao Lin
                    if (node is MyNodeIn)
                    {
                        OldModel.Indoor indoor = (node as MyNodeIn).RoomIndooItem.IndoorItem;
                        string outDoorModel = indoor.Model_Hitachi;
                        //Regex reg = new Regex(@"RPK-(.*?)FSNH3M");
                        //Regex reg = new Regex(@"RPK-(.*?)(FSNH3M|FSNSH3)"); //HAPM IDU High Wall (w/o EXV)有两种model: FSNH3M & FSNSH3 20161024 by Yunxiao Lin
                        Regex reg = new Regex(@"RPK-(.*?)(FSNH3M|FSNSH3|FSNH4)"); //HAPM IDU High Wall (w/o EXV)有三种model: FSNH3M & FSNSH3 & FSNH4 20180729 by Yunxiao Lin
                        var mat = reg.Match(outDoorModel);
                        if (mat.Success)
                        {
                            if (dicSpecL.ContainsKey("Valve"))
                            {
                                dicSpecL["Valve"] += link.ValveLength;
                            }
                        }
                    }
                }
            }
        }


        #region 制冷剂追加量计算 by Angong

        /// 根据不同管径长度计算额外冷媒量
        /// <summary>
        /// 根据不同管径长度计算额外冷媒量
        /// </summary>
        /// <param name="FactoryCode">工厂标识</param>
        /// <param name="L1">管径22.2的合计长度</param>
        /// <param name="L2">管径19.05的合计长度</param>
        /// <param name="L3">管径15.88的合计长度</param>
        /// <param name="L4">管径12.7的合计长度</param>
        /// <param name="L5">管径9.52的合计长度</param>
        /// <param name="L6">管径6.35的合计长度</param>
        /// <param name="L7">管径25.4的合计长度</param>
        /// <param name="L8">管径28.58的合计长度</param>
        /// <param name="L_FSNSH3M">FSNSH3M—阀后段管径6.35的合计长度</param>
        /// <returns></returns>
        private double AddRefrigeration(SystemVRF sysItem, string FactoryCode, double L1, double L2, double L3, double L4, double L5, double L6, double L7, double L8, double L_FSNSH3M, AddFlow addflowItem)
        {
            //SMZ
            #region//FSNS和FSNP需要独立计算 20170417 by Yunxiao Lin
            string series = sysItem.OutdoorItem.Series;
            string unitType = sysItem.OutdoorItem.Type;
            if ("S".Equals(FactoryCode) && !series.Contains("FSNS") && !series.Contains("FSNP") && !series.Contains("FSXNS") && !series.Contains("FSXNP") && !series.Contains("JTOH-BS1") && !series.Contains("JTOR-BS1") && !series.Contains("IVX"))
            {
                //验证是否外机型是否是RAS-FSXN|RAS-FSXNH|RAS-FSXNHT SMZ增加新型号，20160615 by Yunxiao Lin
                string outDoorModel = sysItem.OutdoorItem.Model_Hitachi;
                Regex reg = new Regex(@"RAS-(.*?)(FSXN|FSXNH|FSXNHT)");
                var mat = reg.Match(outDoorModel);
                if (!mat.Success)
                {
                    return 0;
                }
                //W1-W1a
                decimal W1 = 0M;
                W1 = W1 + CvtDec(L1) * 0.36M;
                W1 = W1 + CvtDec(L2) * 0.26M;
                W1 = W1 + CvtDec(L3) * 0.17M;
                W1 = W1 + CvtDec(L4) * 0.11M;
                W1 = W1 + CvtDec(L5) * 0.056M;
                W1 = W1 + CvtDec(L6) * 0.024M;
                W1 = W1 + CvtDec(L_FSNSH3M) * 0.011M;

                //W2
                decimal W2 = GetW2ForSMZ(sysItem);
                //W3
                //decimal W3 = 0;
                decimal W3 = GetValueByPw(sysItem);
                //W4
                decimal W4 = 0;
                double ratio = sysItem.Ratio;
                if (ratio >= 1D)
                {
                    W4 = W4 + 0.5M;
                }
                return CvtDou(W1 + W2 + W3 + W4);
            }
            #endregion

            #region//HAPQ
            if ("Q".Equals(FactoryCode) && !series.Contains("IVX"))
            {
                string outDoorModel = sysItem.OutdoorItem.Model_Hitachi;
                Regex reg = new Regex(@"RAS-(.*?)FSNYW1Q"); //HAPQ的水冷机 add on 20160517 by Yunxiao Lin
                var mat = reg.Match(outDoorModel);
                if (mat.Success)
                {
                    //W0
                    decimal W0 = 0M;
                    if (outDoorModel == "RAS-8FSNYW1Q" || outDoorModel == "RAS-10FSNYW1Q")
                        W0 = 3M;
                    else
                        W0 = 6M;
                    //W1
                    decimal _L1 = CvtDec(L1) * 0.36M;
                    decimal _L2 = CvtDec(L2) * 0.25M;
                    decimal _L3 = CvtDec(L3) * 0.16M;
                    decimal _L4 = CvtDec(L4) * 0.1M;
                    decimal _L5 = CvtDec(L5) * 0.05M;
                    decimal _L6 = CvtDec(L6) * 0.03M;
                    decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6;
                    //W2
                    decimal W2 = GetValueByPw(sysItem);
                    return CvtDou(W0 + W1 + W2);
                }
                else
                {
                    #region//RAS-FSN6Q / RAS-FSNA6Q / RAS-FSXNQ / RAS-FSDNQ
                    reg = new Regex(@"RAS-(.*?)(FSN6Q|FSNA6Q|FSXNQ|FSDNQ)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        //W1
                        decimal _L1 = CvtDec(L1) * 0.39M;
                        decimal _L2 = CvtDec(L2) * 0.28M;
                        decimal _L3 = CvtDec(L3) * 0.19M;
                        decimal _L4 = CvtDec(L4) * 0.12M;
                        decimal _L5 = CvtDec(L5) * 0.06M;
                        decimal _L6 = CvtDec(L6) * 0.03M;
                        decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6;
                        //W2
                        decimal W2 = GetValueByPw(sysItem);
                        //W3
                        decimal W3 = 0;
                        double ratio = sysItem.Ratio;
                        if (ratio >= 1D && ratio <= 1.15D)
                        {
                            W3 = W3 + 0.5M;
                        }
                        else if (ratio > 1.15D && ratio <= 1.30D)
                        {
                            W3 = W3 + 1M;
                        }
                        return CvtDou(W1 + W2 + W3);
                    }
                    #endregion
                    #region//RAS-FSNMQ, (RAS-FSNAMQ)
                    reg = new Regex(@"RAS-(.*?)(FSNMQ|FSNAMQ)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        decimal _L4 = CvtDec(L4) * 0.12M;
                        decimal _L5 = CvtDec(L5) * 0.07M;
                        decimal _L6 = CvtDec(L6) * 0.03M;
                        return CvtDou(_L4 + _L5 + _L6);
                    }
                    #endregion
                    #region//RAS-FSVN1Q, RAS-FSYN1Q
                    reg = new Regex(@"RAS-(.*?)(FSVN1Q|FSYN1Q)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        decimal _L5 = CvtDec(L5) * 0.05M;
                        decimal _L6 = CvtDec(L6) * 0.02M;
                        return CvtDou(_L5 + _L6);
                    }
                    #endregion
                    #region//RAS-HRNM1Q, (RAS-HRNAM1Q)
                    reg = new Regex(@"RAS-(.*?)(HRNM1Q|HRNAM1Q)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        decimal _L5 = CvtDec(L5) * 0.04M;
                        decimal _L6 = CvtDec(L6) * 0.02M;
                        return CvtDou(_L5 + _L6);
                    }
                    #endregion
                    #region//YVAHP/R add on 20170324 by Yunxiao Lin
                    //reg = new Regex(@"TVAH(P|R)(.*?)(B31S|B41S)");
                    reg = new Regex(@"(T|Y)VAH(P|R)(.*?)(B31S|B41S)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        //W1
                        decimal _L1 = CvtDec(L1 * 0.24d * 3.28d / 2.2065d);
                        decimal _L2 = CvtDec(L2 * 0.17d * 3.28d / 2.2065d);
                        decimal _L3 = CvtDec(L3 * 0.11d * 3.28d / 2.2065d);
                        decimal _L4 = CvtDec(L4 * 0.074d * 3.28d / 2.2065d);
                        decimal _L5 = CvtDec(L5 * 0.038d * 3.28d / 2.2065d);
                        decimal _L6 = CvtDec(L6 * 0.016d * 3.28d / 2.2065d);
                        decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6;
                        //decimal W_out = CvtDec(6.6d * sysItem.OutdoorItem.CoolingCapacity / (0.293d * 2.2065d));
                        //根据室外机组合名称获取分机数量 20180208 by Yunxiao Lin
                        string fullModule = sysItem.OutdoorItem.FullModuleName;
                        string[] strs1 = fullModule.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> unitFullName = new List<string>();
                        foreach (string str1 in strs1)
                        {
                            //JCHVRF 中不仅仅有 model1+model2+...，还会有 model1+model2*2+.... Yunxiao Lin
                            if (str1.Contains("*"))
                            {
                                string[] strs2 = str1.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                                if (strs2.Length == 2)
                                {
                                    int num = 0;
                                    if (int.TryParse(strs2[1], out num))
                                    {
                                        for (int i = 0; i < num; i++)
                                        {
                                            unitFullName.Add(strs2[0]);
                                        }
                                    }
                                }
                            }
                            else
                                unitFullName.Add(str1);
                        }
                        int count = unitFullName.Count;
                        decimal W_out = CvtDec((6.6d * count) / 2.2065d); //每台分机最少需要6.6lbs冷媒追加。
                        if (W_out > W1)
                            W1 = W_out;

                        //W2
                        decimal W2 = getW2ForYVAHP(sysItem);
                        //W3
                        decimal W3 = 0;
                        if (sysItem.Ratio >= 1d)
                            W3 = CvtDec(1.1d / 2.2065d);

                        return CvtDou(W1 + W2 + W3);
                    }
                    //FSCNY1Q Low Ambient add on 20160626 by Yunxiao Lin
                    reg = new Regex(@"RAS-(.*?)FSCNY1Q");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        //W1
                        decimal _L1 = CvtDec(L1) * 0.36M;
                        decimal _L2 = CvtDec(L2) * 0.26M;
                        decimal _L3 = CvtDec(L3) * 0.17M;
                        decimal _L4 = CvtDec(L4) * 0.11M;
                        decimal _L5 = CvtDec(L5) * 0.056M;
                        decimal _L6 = CvtDec(L6) * 0.024M;
                        decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6;
                        //W1mini
                        decimal W1mini = GetMiniValueByOutPW(sysItem);
                        if (W1 < W1mini)
                            W1 = W1mini;
                        //W2
                        decimal W2 = GetValueByPw(sysItem);
                        //W3
                        decimal W3 = 0;
                        if (sysItem.Ratio > 1d)
                            W3 = 0.5M;
                        return CvtDou(W1 + W2 + W3);
                    }

                    //HNBRKQ,HNBRMQ Low Ambient add on 20180202 by Vince 
                    // Wuxi Gen2 Res. T1. 20180625 by Yunxiao Lin
                    // Res. Tier 1, HP, WX. 20190221 by xyj
                    reg = new Regex(@"RAS-(.*?)(HNBRKQ|HNBRMQ|HNBRKQ1|HNBRMQ1)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        int ODUModelNo = Convert.ToInt32(sysItem.OutdoorItem.ModelFull.Substring(4, 3).ToString());
                        if (ODUModelNo >= 30 && ODUModelNo <= 65)
                        {
                            //W                       
                            decimal _L4 = CvtDec(L4) * 0.085M;
                            decimal _L5 = CvtDec(L5) * 0.05M;
                            decimal _L6 = CvtDec(L6) * 0.02M;
                            decimal W = _L4 + _L5 + _L6;
                            ////W1mini
                            //decimal Wmini = GetMiniValueByOutPW(sysItem);
                            //if (W < Wmini)
                            //    W = Wmini;

                            return CvtDou(W);

                        }
                        else if (ODUModelNo >= 70 && ODUModelNo <= 120)
                        {
                            int N1 = 0;   //28~45
                            int N2 = 0;   //50~71
                            int N3 = 0;   //80~160
                            //统计各个范围内的环绕出风式卡式机数量
                            foreach (OldModel.RoomIndoor rinItem in thisProject.RoomIndoorList)
                            {
                                if (sysItem.Id == rinItem.SystemID)
                                {
                                    if (rinItem.IndoorItem.Type == "Round Way Cassette")
                                    {
                                        int modelNo = Convert.ToInt32(rinItem.IndoorItem.ModelFull.Substring(4, 3).ToString());
                                        if (modelNo >= 28 && modelNo <= 45)
                                            N1++;
                                        else if (modelNo >= 50 && modelNo <= 71)
                                            N2++;
                                        else if (modelNo >= 80 && modelNo <= 160)
                                            N3++;
                                    }
                                }
                            }

                            //W1                       
                            decimal _L4 = CvtDec(L4) * 0.12M;
                            decimal _L5 = CvtDec(L5) * 0.059M;
                            decimal _L6 = CvtDec(L6) * 0.02M;
                            decimal _L7 = 0.23M * CvtDec(N1) + 0.29M * CvtDec(N2) + 0.47M * CvtDec(N3);
                            decimal W = _L4 + _L5 + _L6 + _L7;
                            ////W1mini
                            //decimal Wmini = GetMiniValueByOutPW(sysItem);
                            //if (W < Wmini)
                            //    W = Wmini;                          
                            return CvtDou(W);
                        }

                    }
                    #endregion
                    #region//HNBCMQ Low Ambient add on 20180202 by Vince 
                    // Wuxi Gen 2. Comm. T1,T2, RAS-HNBBMQ, RAS-HNBCMQ, RAS-HNBCMQ1 20180625 by Yunxiao Lin
                    reg = new Regex(@"RAS-(.*?)(HNBCMQ|HNBBMQ|HNBCMQ1)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        //W1
                        decimal _L1 = CvtDec(L1) * 0.36M;
                        decimal _L2 = CvtDec(L2) * 0.26M;
                        decimal _L3 = CvtDec(L3) * 0.17M;
                        decimal _L4 = CvtDec(L4) * 0.11M;
                        decimal _L5 = CvtDec(L5) * 0.056M;
                        decimal _L6 = CvtDec(L6) * 0.024M;
                        decimal _L7 = CvtDec(L7) * 0.52M;
                        decimal _L8 = CvtDec(L8) * 0.67M;
                        decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6 + _L7 + _L8;
                        //W1mini
                        decimal W1mini = GetMiniValueByHNBCMQ(sysItem);
                        if (W1 < W1mini)
                            W1 = W1mini;
                        //W2
                        decimal W2 = GetValueByHNBCMQ(sysItem);
                        //W3
                        decimal W3 = 0;
                        if (sysItem.Ratio > 1d)
                            W3 = 0.5M;
                        return CvtDou(W1 + W2 + W3);
                    }
                    #endregion
                    #region// Wuxi Gen 2. Comm. T3, RAS-JNBBTQ add on 20180925 by Shen Junjie 
                    reg = new Regex(@"RAS-(.*?)(JNBBTQ)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        //W1
                        decimal _L1 = CvtDec(L1) * 0.36M;
                        decimal _L2 = CvtDec(L2) * 0.28M;
                        decimal _L3 = CvtDec(L3) * 0.19M;
                        decimal _L4 = CvtDec(L4) * 0.12M;
                        decimal _L5 = CvtDec(L5) * 0.06M;
                        decimal _L6 = CvtDec(L6) * 0.03M;
                        decimal _L7 = CvtDec(L7) * 0.52M;
                        decimal _L8 = CvtDec(L8) * 0.67M;
                        decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6 + _L7 + _L8;
                        //W1mini
                        decimal W1mini = GetMiniValueByJNBBTQ(sysItem);
                        if (W1 < W1mini)
                            W1 = W1mini;
                        //W2                                                                                                                                                                                                             
                        decimal W2 = GetValueByHNBCMQ(sysItem);
                        W2 = Math.Min(W2, 6.0M); //The maximum amount of additional refrigerant charge to the indoor unit W2 is 6.0Kg.
                        //W3
                        decimal W3 = 0;
                        if (sysItem.Ratio > 1d)
                            W3 = 0.5M;
                        //Maximum supplementary refrigerant charging amount
                        decimal Wmax = GetMaxValueByJNBBTQ(sysItem);
                        return CvtDou(Math.Min(W1 + W2 + W3, Wmax));
                    }
                    #endregion
                    #region// Wuxi Gen 2. Cooling Only, RAS-CNBCMQ add on 20180925 by Shen Junjie 
                    reg = new Regex(@"RAS-(.*?)(CNBCMQ)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        //W1
                        decimal _L1 = CvtDec(L1) * 0.36M;
                        decimal _L2 = CvtDec(L2) * 0.28M;
                        decimal _L3 = CvtDec(L3) * 0.19M;
                        decimal _L4 = CvtDec(L4) * 0.12M;
                        decimal _L5 = CvtDec(L5) * 0.06M;
                        decimal _L6 = CvtDec(L6) * 0.03M;
                        decimal _L7 = CvtDec(L7) * 0.52M;
                        decimal _L8 = CvtDec(L8) * 0.67M;
                        decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6 + _L7 + _L8;
                        //W1mini
                        decimal W1mini = GetMiniValueByCNBCMQ(sysItem);
                        if (W1 < W1mini)
                            W1 = W1mini;
                        //W2                                                                                                                                                                                                             
                        decimal W2 = GetValueByHNBCMQ(sysItem);
                        W2 = Math.Min(W2, 6.0M); //The maximum amount of additional refrigerant charge to the indoor unit W2 is 6.0Kg.
                        //W3
                        decimal W3 = 0;
                        if (sysItem.Ratio > 1d)
                            W3 = 0.5M;
                        return CvtDou(W1 + W2 + W3);
                    }
                    #endregion
                    #region RAS-HNSKQ 20190523 by Yunxiao Lin
                    reg = new Regex(@"RAS-(.*?)(HNSKQ)");
                    mat = reg.Match(outDoorModel);
                    if (mat.Success)
                    {
                        decimal W = 0M;
                        decimal W11 = 0M;
                        decimal W12 = 0M;
                        decimal W13 = 0M;
                        decimal W21 = 0M;
                        decimal W22 = 0M;
                        decimal W23 = 0M;
                        decimal Wmax = 0M;
                        switch (outDoorModel)
                        {
                            case "RAS-3.0HNSKQ":
                            case "RAS-3.5HNSKQ":
                            case "RAS-4.0HNSKQ":
                                W11 = CvtDec(L6) * 0.020M;
                                W12 = CvtDec(L5) * 0.050M;
                                W13 = CvtDec(L4) * 0.085M;
                                W = W11 + W12 + W13;
                                Wmax = 3.1M;
                                break;
                            case "RAS-4.5HNSKQ":
                            case "RAS-5.0HNSKQ":
                            case "RAS-6.0HNSKQ":
                                W11 = CvtDec(L6) * 0.020M;
                                W12 = CvtDec(L5) * 0.050M;
                                W13 = CvtDec(L4) * 0.085M;
                                W = W11 + W12 + W13;
                                Wmax = 2.7M;
                                break;
                            case "RAS-6.5HNSKQ":
                            case "RAS-7.0HNSKQ":
                                W11 = CvtDec(L6) * 0.020M;
                                W12 = CvtDec(L5) * 0.059M;
                                W13 = CvtDec(L4) * 0.120M;
                                string sId = sysItem.Id;
                                List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
                                for (int i = 0; i < RoomIndoorList.Count; i++)
                                {
                                    var ent = RoomIndoorList[i];
                                    //判断当前室内机是否属于该系统
                                    //判断当前室内机是否属于该系统
                                    if (ent.SystemID != sId)
                                        continue;
                                    //根据每台室内机的匹数追加制冷剂
                                    if (ent.IndoorItem != null && ent.IndoorItem.Type == "Four Way Cassette")
                                    {
                                        double cap = ent.IndoorItem.CoolingCapacity;
                                        if (cap >= 2.8d && cap <= 4.5d)
                                            W21 += 0.23M;
                                        else if (cap >= 5.0d && cap <= 7.1d)
                                            W22 += 0.29M;
                                        else if (cap >= 8.0d && cap <= 16.0d)
                                            W23 += 0.47M;

                                    }
                                }
                                W = W11 + W12 + W13 + W21 + W22 + W23;
                                Wmax = 2.7M;
                                break;
                        }
                        if (W > Wmax)
                            W = Wmax;
                        return CvtDou(W);
                    }
                    #endregion
                }
            }
            #endregion

            #region//HAPB 暂时只有Water source
            // HAPB FSNS7B和FSNS5B有另外的计算方式 20170417 Yunxiao Lin
            if ("B".Equals(FactoryCode) && !series.Contains("FSNS") && !series.Contains("FSNP") && !series.Contains("FSNC5B") && !series.Contains("FSNC7B"))
            {
                //W1
                decimal _L1 = CvtDec(L1) * 0.39M;
                decimal _L2 = CvtDec(L2) * 0.28M;
                decimal _L3 = CvtDec(L3) * 0.19M;
                decimal _L4 = CvtDec(L4) * 0.12M;
                decimal _L5 = CvtDec(L5) * 0.07M;
                decimal _L6 = CvtDec(L6) * 0.03M;
                decimal W1 = _L1 + _L2 + _L3 + _L4 + _L5 + _L6;
                //W2
                decimal W2 = GetValueByPw(sysItem);
                return CvtDou(W1 + W2);
            }
            #endregion
            #region//HAPE RAS-FSXNHE|FSNSE|FSXNSE|FSNPE|FSXNPE
            if ("E".Equals(FactoryCode))
            {
                //HAPE RAS-FSXNHE，计算方式和SMZ的RAS-FSXNH相同 20160615 by Yunxiao Lin
                string outDoorModel = sysItem.OutdoorItem.Model_Hitachi;
                decimal W1 = 0M;
                decimal W2 = 0M;
                decimal W3 = 0M;
                decimal W4 = 0M;
                decimal W5 = 0M;
                decimal W6 = 0M;
                Regex reg = new Regex(@"RAS-(.*?)(FSXNHE|FSNSE|FSXNSE|FSNPE|FSXNPE|FSXNME)");
                var mat = reg.Match(outDoorModel);
                if (!mat.Success)
                {
                    reg = new Regex(@"RAS-(.*?)(FSVNME|FSNME)");
                    mat = reg.Match(outDoorModel);
                    if (!mat.Success)
                    {
                        return 0;
                    }
                    //W0
                    decimal W0 = CvtDec(sysItem.OutdoorItem.RefrigerantCharge);
                    //W1
                    W1 = W1 + CvtDec(L4) * 0.085M;
                    W1 = W1 + CvtDec(L5) * 0.05M;
                    W1 = W1 + CvtDec(L6) * 0.02M;
                    decimal WT = W1 + W0;
                    if (WT > CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge))
                        W1 = CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge - sysItem.OutdoorItem.RefrigerantCharge);
                    return CvtDou(W1);
                }
                //W1-W1a
                W1 = W1 + CvtDec(L1) * 0.36M;
                W1 = W1 + CvtDec(L2) * 0.26M;
                W1 = W1 + CvtDec(L3) * 0.17M;
                W1 = W1 + CvtDec(L4) * 0.11M;
                W1 = W1 + CvtDec(L5) * 0.056M;
                W1 = W1 + CvtDec(L6) * 0.024M;
                W1 = W1 + CvtDec(L_FSNSH3M) * 0.011M;

                if (outDoorModel.Contains("FSXNHE"))
                {
                    //W2
                    W2 = GetW2ForSMZ(sysItem);
                    //W3
                    W3 = GetValueByPw(sysItem);
                    //W4
                    W4 = 0;
                    double ratio = sysItem.Ratio;
                    if (ratio >= 1D)
                    {
                        W4 = W4 + 0.5M;
                    }
                    return CvtDou(W1 + W2 + W3 + W4);
                }
                else //FSNSE|FSNPE|FSXNSE|FSXNPE的冷媒追加算法和FSXNHE不同
                {
                    if (sysItem.OutdoorItem != null)
                    {
                        //EU W1 有最小冷媒追加量，根据室外机判断 20180506 by Yunxiao Lin
                        if (W1 < CvtDec(sysItem.OutdoorItem.MinRefrigerantCharge))
                            W1 = CvtDec(sysItem.OutdoorItem.MinRefrigerantCharge);
                    }
                    //W2
                    W2 = GetW2ForFSNSE(sysItem);
                    //W3
                    W3 = GetValueByPw(sysItem);
                    //W4
                    W4 = 0;
                    double ratio = sysItem.Ratio;
                    if (ratio > 1D)
                    {
                        W4 = W4 + 0.5M;
                    }
                    //W5
                    W5 = 0;
                    if (sysItem.OutdoorItem != null)
                    {
                        switch (sysItem.OutdoorItem.Model_Hitachi.Trim())
                        {
                            //根据EU提供的TC修改每个型号的室外机对应的追加冷媒。 20180502 by Yunxiao Lin
                            case "RAS-24FSXNSE":
                            case "RAS-38FSXNSE":
                            case "RAS-42FSXNSE":
                            case "RAS-46FSXNSE":
                            case "RAS-56FSXNSE":
                            case "RAS-60FSXNSE":
                            case "RAS-64FSXNSE":
                            case "RAS-68FSXNSE":
                            case "RAS-74FSXNSE":
                            case "RAS-78FSXNSE": W5 += 1M; break;
                            case "RAS-48FSXNSE":
                            case "RAS-62FSXNSE":
                            case "RAS-66FSXNSE":
                            case "RAS-70FSXNSE":
                            case "RAS-80FSXNSE":
                            case "RAS-82FSXNSE":
                            case "RAS-84FSXNSE":
                            case "RAS-92FSXNSE": W5 += 2M; break;
                            case "RAS-72FSXNSE":
                            case "RAS-86FSXNSE":
                            case "RAS-88FSXNSE":
                            case "RAS-90FSXNSE":
                            case "RAS-94FSXNSE": W5 += 3M; break;
                            case "RAS-96FSXNSE": W5 += 4M; break;
                            default: break;
                        }
                    }
                    //W6
                    W6 = 0;
                    if (sysItem.OutdoorItem != null && addflowItem != null && series.Contains("HR,"))
                    {
                        foreach (Node node in addflowItem.Items.OfType<Node>())
                        {
                            if (node is MyNodeMultiCH)
                            {
                                string MCH_Model = (node as MyNodeMultiCH).Model;
                                switch (MCH_Model.Trim())
                                {
                                    case "CH-AP04MSSX": W6 += 0.1M; break;
                                    case "CH-AP08MSSX": W6 += 0.2M; break;
                                    case "CH-AP12MSSX": W6 += 0.3M; break;
                                    case "CH-AP16MSSX": W6 += 0.4M; break;
                                    default: break;
                                }
                            }
                        }
                    }
                    double result = CvtDou(W1 + W2 + W3 + W4 + W5 + W6);
                    //根据EU 提供的TC， 冷媒追加需要判断最大值 20180502 by Yunxiao Lin
                    if (sysItem.OutdoorItem != null)
                    {
                        if (result > sysItem.OutdoorItem.MaxRefrigerantCharge)
                            result = sysItem.OutdoorItem.MaxRefrigerantCharge;
                    }
                    return result;
                }
            }
            #endregion

            #region//FSXNK,FSXNK1,FSXNKV,FSXNKM   --add on 20171108 by Lingjia Qiu
            if ("I".Equals(FactoryCode))
            {
                string outDoorModel = sysItem.OutdoorItem.Model_Hitachi;
                Regex reg = new Regex(@"RAS-(.*?)(FSXNK|FSXNK1|FSXNKV|FSXNKM)");
                var mat = reg.Match(outDoorModel);
                if (mat.Success)
                {

                    //W1-W1a
                    decimal W1 = 0M;
                    W1 = W1 + CvtDec(L1) * 0.36M;
                    W1 = W1 + CvtDec(L2) * 0.26M;
                    W1 = W1 + CvtDec(L3) * 0.17M;
                    W1 = W1 + CvtDec(L4) * 0.11M;
                    W1 = W1 + CvtDec(L5) * 0.056M;
                    W1 = W1 + CvtDec(L6) * 0.024M;

                    //W2
                    decimal W2 = GetW2ForSMZ(sysItem);
                    //W3
                    //decimal W3 = 0;
                    decimal W3 = GetValueByPw(sysItem);
                    //W4
                    decimal W4 = 0;
                    double ratio = sysItem.Ratio;
                    if (ratio >= 1D)
                    {
                        W4 = W4 + 0.5M;
                    }
                    return CvtDou(W1 + W2 + W3 + W4);
                }
                else
                {
                    //FSNMA
                    reg = new Regex(@"RAS-(.*?)(FSNMA)");
                    mat = reg.Match(outDoorModel);
                    if (!mat.Success)
                        return 0;

                    //W1-W1a
                    decimal W1 = 0M;
                    W1 = W1 + CvtDec(L3) * 0.19M;
                    W1 = W1 + CvtDec(L4) * 0.12M;
                    W1 = W1 + CvtDec(L5) * 0.07M;
                    W1 = W1 + CvtDec(L6) * 0.03M;

                    //W2
                    decimal W2 = GetValueByPw(sysItem);
                    return CvtDou(W1 + W2);

                }
            }
            #endregion

            #region//FSNS|FSNP|FSNS7B|FSNS5B|FSNP7B|FSNP5B|FSNC5B|FSNC7B 20190105 by Yunxiao Lin
            if (series.Contains("FSNS") || series.Contains("FSNP") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC5B") || series.Contains("FSNC7B"))
            {
                //W1
                decimal W1 = 0M;
                W1 = W1 + CvtDec(L1) * 0.36M;
                W1 = W1 + CvtDec(L2) * 0.26M;
                W1 = W1 + CvtDec(L3) * 0.17M;
                W1 = W1 + CvtDec(L4) * 0.11M;
                W1 = W1 + CvtDec(L5) * 0.056M;
                W1 = W1 + CvtDec(L6) * 0.024M;
                W1 = W1 + CvtDec(L7) * 0.52M;
                W1 = W1 + CvtDec(L8) * 0.67M;
                W1 = W1 + CvtDec(L_FSNSH3M) * 0.011M;
                //根据室外机匹数修正W1 20170417 by Yunxiao Lin
                double hp = sysItem.OutdoorItem.Horsepower;
                if (series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC5B") || series.Contains("FSNC7B"))
                {
                    //FSNS, FSNC
                    if (hp >= 6d && hp <= 10d && W1 < 2M)
                        W1 = 2M;
                    else if (hp >= 12d && hp <= 18d && W1 < 3M)
                        W1 = 3M;
                    else if (hp >= 20d && hp <= 24d && W1 < 4M)
                        W1 = 4M;
                    else if (hp >= 26d && hp <= 36d && W1 < 6M)
                        W1 = 6M;
                    else if (hp >= 38d && hp <= 42d && W1 < 7M)
                        W1 = 7M;
                    else if (hp >= 44d && hp <= 48d && W1 < 8M)
                        W1 = 8M;
                    else if (hp >= 50d && hp <= 54d && W1 < 9M)
                        W1 = 9M;
                }
                else
                {
                    //FSNP
                    if (hp >= 5d && hp <= 10d && W1 < 2M)
                        W1 = 2M;
                    else if (hp >= 12d && hp <= 14d && W1 < 3M)
                        W1 = 3M;
                    else if (hp >= 16d && hp <= 20d && W1 < 4M)
                        W1 = 4M;
                    else if (hp == 22d && W1 < 5M)
                        W1 = 5M;
                    else if (hp >= 24d && hp <= 26d && W1 < 6M)
                        W1 = 6M;
                    else if (hp >= 28d && hp <= 32d && W1 < 7M)
                        W1 = 7M;
                    else if (hp >= 34d && hp <= 36d && W1 < 8M)
                        W1 = 8M;
                    else if (hp >= 38d && hp <= 42d && W1 < 9M)
                        W1 = 9M;
                    else if (hp >= 44d && hp <= 46d && W1 < 10M)
                        W1 = 10M;
                    else if (hp >= 48d && hp <= 50d && W1 < 11M)
                        W1 = 11M;
                    else if (hp >= 52d && hp <= 54d && W1 < 12M)
                        W1 = 12M;
                }
                //W2, W3
                decimal W2 = 0M;
                decimal W3 = 0M;
                string sId = sysItem.Id;
                List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
                for (int i = 0; i < RoomIndoorList.Count; i++)
                {
                    var ent = RoomIndoorList[i];
                    //判断当前室内机是否属于该系统
                    if (ent.SystemID != sId)
                        continue;
                    //根据每台室内机的匹数追加制冷剂
                    if (ent.IndoorItem != null)
                    {
                        if (ent.IndoorItem.Horsepower >= 0.4d && ent.IndoorItem.Horsepower <= 1d) //0.4HP~1.0HP 每台室内机追加0.3kg制冷剂
                            W2 += 0.3M;
                        else if (ent.IndoorItem.Horsepower >= 1.5d && ent.IndoorItem.Horsepower <= 6d) //1.5HP~6HP 每台室内机追加0.5kg制冷剂
                            W2 += 0.5M;
                        //else if (ent.IndoorItem.Horsepower >= 8d && ent.IndoorItem.Horsepower <= 10d) //8HP~10HP 每台室内机追加1kg制冷剂
                        else if (series == "Commercial VRF HP, FSNS" || series == "Commercial VRF HP, FSNP" || series == "Commercial VRF HR, FSXNS" || series == "Commercial VRF HR, FSXNP")
                        {
                            if (ent.IndoorItem.Horsepower >= 7d && ent.IndoorItem.Horsepower <= 10d) //7HP~10HP 每台室内机追加1kg制冷剂 on 20190225 by xyj
                                W3 += 1M;
                        }
                        else
                        {
                            if (ent.IndoorItem.Horsepower >= 8d && ent.IndoorItem.Horsepower <= 10d) //8HP~10HP 每台室内机追加1kg制冷剂
                                W3 += 1M;
                        }
                    }
                }
                //W4
                //根据Connection Ratio计算制冷剂追加量
                decimal W4 = 0M;
                if (sysItem.Ratio >= 1d)
                    W4 = 0.5M;
                //W5
                //根据室外机特殊型号追加制冷剂
                decimal W5 = 0M;
                if (sysItem.OutdoorItem.Model_Hitachi.Contains("RAS-24FSNS"))
                    W5 = 1M;
                else if (sysItem.OutdoorItem.Model_Hitachi.Contains("RAS-36FSNS"))
                    W5 = 1M;
                else if (sysItem.OutdoorItem.Model_Hitachi.Contains("RAS-42FSNS"))
                    W5 = 1M;
                else if (sysItem.OutdoorItem.Model_Hitachi.Contains("RAS-46FSNS"))
                    W5 = 1M;
                else if (sysItem.OutdoorItem.Model_Hitachi.Contains("RAS-48FSNS"))
                    W5 = 2M;
                return CvtDou(W1 + W2 + W3 + W4 + W5);
            }
            #endregion

            #region//HAPQ IVX 20170704 by Yunxiao Lin
            if ("Q".Equals(FactoryCode) && series.Contains("IVX"))
            {
                //W1
                decimal W1 = 0;
                if (sysItem.OutdoorItem.Model_Hitachi == "RAS-3HRNM1Q")
                    W1 = W1 + CvtDec(L5) * 0.03M;
                else
                    W1 = W1 + CvtDec(L5) * 0.04M;
                W1 = W1 + CvtDec(L6) * 0.02M;
                return CvtDou(W1);
            }
            #endregion
            #region//SMZ IVX 20170704 by Yunxiao Lin
            if ("S".Equals(FactoryCode) && series.Contains("IVX"))
            {
                //P 冷媒追加管长系数
                decimal P = 0;
                //MaxW 最大冷媒追加量
                decimal MaxW = 0;
                //l 计算时需要减掉的管长
                decimal l = 0;
                int indoorCount = sysItem.GetIndoorCount(thisProject);
                switch (sysItem.OutdoorItem.Model_Hitachi)
                {
                    case "RAS-2HVNP":
                        P = 0.03M;
                        MaxW = 1.5M;
                        l = 20M;
                        break;
                    case "RAS-2.5HVNP":
                        P = 0.03M;
                        MaxW = 1.2M;
                        l = 30M;
                        if (indoorCount == 2)
                            P = 0.024M;
                        break;
                    case "RAS-3HVNC":
                        P = 0.04M;
                        MaxW = 1.2M;
                        l = 30M;
                        break;
                    case "RAS-4HVNC1":
                        P = 0.04M;
                        MaxW = 1.6M;
                        l = 30M;
                        break;
                    case "RAS-5HVNC1":
                    case "RAS-6HVNC1":
                    case "RAS-7HVRNM2":
                        P = 0.06M;
                        MaxW = 2.7M;
                        l = 30M;
                        break;
                }
                decimal W = 0;
                if (sysItem.TotalPipeLength > 0)
                {
                    W = (CvtDec(sysItem.TotalPipeLength) - l) * P;
                    if (W < 0)
                        W = 0;
                    if (W > MaxW)
                        W = MaxW;
                }
                return CvtDou(W);
            }
            if ("T".Equals(FactoryCode) && (unitType == "FSQB (Side discharge)" || unitType == "FNS(B) (Side discharge)" || unitType == "FS(B) (Side discharge)")) //TW FSQB FNS(B) FS(B) 20180729 by Yunxiao Lin
            {
                //P 冷媒追加管长系数
                decimal P = 0;
                //MaxW 最大冷媒追加量
                decimal MaxW = CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge);
                //l 计算时需要减掉的管长
                decimal l = 0;
                int indoorCount = sysItem.GetIndoorCount(thisProject);
                switch (sysItem.OutdoorItem.Model_Hitachi)
                {
                    case "RAM-112FSQB":
                    case "RAM-125FSQB":
                    case "RAM-140FSQB":
                    case "RAM-155FSQB":
                        P = 0.04M;
                        l = 30M;
                        break;
                    case "RAM-5FNS":
                    case "RAM-6FNS":
                    case "RAM-5FNSB":
                    case "RAM-6FNSB":
                    case "RAM-106FS":
                    case "RAM-106FSB":
                        P = 0.06M;
                        l = 30M;
                        break;
                }
                decimal W = 0;
                if (sysItem.TotalPipeLength > 0)
                {
                    W = (CvtDec(sysItem.TotalPipeLength) - l) * P;
                    if (W < 0)
                        W = 0;
                    if (W > MaxW && MaxW > 0)
                        W = MaxW;
                }
                return CvtDou(W);
            }
            if ("T".Equals(FactoryCode) && unitType == "FB (Side discharge)") //TW FB 20180729 by Yunxiao Lin
            {
                //根据9.52mm 和 6.35mm 液管长度计算冷媒追加
                decimal W = 0M;

                W = W + CvtDec(L5) * 0.05M;
                W = W + CvtDec(L6) * 0.02M;
                //最大冷媒追加量
                decimal MaxW = CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge);
                if (W < 0)
                    W = 0;
                if (W > MaxW && MaxW > 0)
                    W = MaxW;

                return CvtDou(W);
            }
            if ("T".Equals(FactoryCode) && (unitType == "F(D) (Side discharge)" || unitType == "FS(D) (Side discharge)")) //TW F(D) FS(D) 20180729 by Yunxiao Lin
            {
                decimal W = 0M;
                W = W + CvtDec(L4) * 0.12M;
                W = W + CvtDec(L5) * 0.07M;
                W = W + CvtDec(L6) * 0.03M;
                //最大冷媒追加量
                decimal MaxW = CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge);
                if (W > MaxW && MaxW > 0)
                    W = MaxW;

                return CvtDou(W);
            }
            if ("T".Equals(FactoryCode) && unitType == "FSQ(D) (Side discharge)") //TW FB 20180729 by Yunxiao Lin
            {
                decimal W = 0M;
                if (sysItem.TotalPipeLength > 0)
                {
                    decimal W1 = 0M;
                    if (sysItem.TotalPipeLength <= 30d)
                    {
                        W1 = W1 + CvtDec(L5) * 0.04M;
                    }
                    else
                    {
                        W1 = W1 + CvtDec(L4) * 0.12M;
                        W1 = W1 + CvtDec(L5) * 0.07M;
                        W1 = W1 + CvtDec(L6) * 0.03M;
                        decimal W0 = 2M;
                        W1 = W1 - W0;
                        if (W1 < 0M)
                            W1 = 0M;
                    }
                    decimal W2 = 0M;
                    if (sysItem.GetIndoorCount(thisProject) > 4)
                    {
                        if (int.Parse((sysItem.Ratio * 100).ToString("n0")) >= 100 && int.Parse((sysItem.Ratio * 100).ToString("n0")) <= 115)
                            W2 = 0.5M;
                        else if (int.Parse((sysItem.Ratio * 100).ToString("n0")) >= 116 && int.Parse((sysItem.Ratio * 100).ToString("n0")) <= 130)
                            W2 = 1M;

                    }
                    W = W + W1 + W2;
                    decimal MaxW = CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge);
                    if (W > MaxW && MaxW > 0)
                        W = MaxW;
                }
                return CvtDou(W);
            }
            if ("T".Equals(FactoryCode) && (unitType == "MSD (Top discharge)" || unitType == "MS (Top discharge)"))
            {
                //W1
                decimal W1 = 0M;
                W1 = W1 + CvtDec(L1) * 0.36M;
                W1 = W1 + CvtDec(L2) * 0.26M;
                W1 = W1 + CvtDec(L3) * 0.17M;
                W1 = W1 + CvtDec(L4) * 0.11M;
                W1 = W1 + CvtDec(L5) * 0.056M;
                W1 = W1 + CvtDec(L6) * 0.024M;
                W1 = W1 + CvtDec(L7) * 0.52M;
                W1 = W1 + CvtDec(L8) * 0.67M;
                //W2
                decimal W2 = 0M;
                string sId = sysItem.Id;
                List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
                for (int i = 0; i < RoomIndoorList.Count; i++)
                {
                    var ent = RoomIndoorList[i];
                    //判断当前室内机是否属于该系统
                    if (ent.SystemID != sId)
                        continue;
                    //根据每台室内机的匹数追加制冷剂
                    if (ent.IndoorItem != null)
                    {
                        if (ent.IndoorItem.Horsepower >= 0.8d && ent.IndoorItem.Horsepower <= 1.3d) //0.8HP~1.3HP 每台室内机追加0.3kg制冷剂
                            W2 += 0.3M;
                        else if (ent.IndoorItem.Horsepower >= 1.5d && ent.IndoorItem.Horsepower <= 6d) //1.5HP~6HP 每台室内机追加0.5kg制冷剂
                            W2 += 0.5M;
                        else if (ent.IndoorItem.Horsepower >= 8d && ent.IndoorItem.Horsepower <= 10d) //8HP~10HP 每台室内机追加1kg制冷剂
                            W2 += 1M;
                    }
                }
                //W3
                //根据Connection Ratio计算制冷剂追加量
                decimal W3 = 0M;
                if (sysItem.Ratio >= 1d)
                    W3 = 0.5M;
                //W4
                decimal W4 = 0M;
                W4 = W4 + CvtDec(L_FSNSH3M) * 0.011M;
                //W5
                //根据室外机特殊型号追加制冷剂
                decimal W5 = 0M;
                int hp = Convert.ToInt32(sysItem.OutdoorItem.Horsepower);
                switch (hp)
                {
                    case 24:
                    case 38:
                    case 42:
                    case 46:
                    case 56:
                    case 60:
                    case 64:
                    case 68:
                    case 74:
                    case 78:
                        W5 = 1M;
                        break;
                    case 48:
                    case 62:
                    case 66:
                    case 70:
                    case 80:
                    case 82:
                    case 84:
                    case 92:
                        W5 = 2M;
                        break;
                    case 72:
                    case 86:
                    case 88:
                    case 90:
                    case 94:
                        W5 = 3M;
                        break;
                    case 96:
                        W5 = 4M;
                        break;
                }
                decimal W = W1 + W2 + W3 + W4 + W5;
                decimal MaxW = CvtDec(sysItem.OutdoorItem.MaxRefrigerantCharge);
                if (W > MaxW && MaxW > 0)
                    W = MaxW;
                return CvtDou(W);
            }
            #endregion
            return 0;
        }
        /// <summary>
        /// 计算YVAHP(NA)室内机冷媒追加量 W2
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        private decimal getW2ForYVAHP(SystemVRF sysItem)
        {
            decimal W2 = 0;
            string sId = sysItem.Id;
            List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
            for (int i = 0; i < RoomIndoorList.Count; i++)
            {
                var ent = RoomIndoorList[i];
                //判断当前室内机是否属于该系统
                if (ent.SystemID != sId)
                    continue;
                //不同型号的室内机分别计算
                Regex reg = new Regex(@"TIDM(.*?)B21S");
                var mat = reg.Match(ent.IndoorItem.Model_Hitachi);
                if (mat.Success)
                {
                    double D_In = ent.IndoorItem.CoolingCapacity / 0.293d;
                    if (D_In > 8d)
                    {
                        if (D_In <= 12d)
                            W2 += CvtDec(0.26d / 2.2065d);
                        else if (D_In <= 15d)
                            W2 += CvtDec(0.35d / 2.2065d);
                        else if (D_In <= 18d)
                            W2 += CvtDec(0.35d / 2.2065d);
                        else if (D_In <= 24d)
                            W2 += CvtDec(0.55d / 2.2065d);
                        else if (D_In <= 30d)
                            W2 += CvtDec(0.66d / 2.2065d);
                        else if (D_In <= 36d)
                            W2 += CvtDec(1.1d / 2.2065d);
                    }
                }
                else
                {
                    reg = new Regex(@"TIC4(.*?)B21S");
                    mat = reg.Match(ent.IndoorItem.Model_Hitachi);
                    if (mat.Success)
                    {
                        double D_In = ent.IndoorItem.CoolingCapacity / 0.293d;
                        if (D_In > 8d)
                        {
                            if (D_In <= 12d)
                                W2 += CvtDec(0.55d / 2.2065d);
                            else if (D_In <= 15d)
                                W2 += CvtDec(0.55d / 2.2065d);
                            else if (D_In <= 18d)
                                W2 += CvtDec(0.55d / 2.2065d);
                            else if (D_In <= 24d)
                                W2 += CvtDec(1.1d / 2.2065d);
                            else if (D_In <= 30d)
                                W2 += CvtDec(1.1d / 2.2065d);
                            else if (D_In <= 36d)
                                W2 += CvtDec(1.1d / 2.2065d);
                        }
                    }
                }
            }
            //注意室内机冷媒追加总量不能超过4.4lbs
            if (W2 > CvtDec(4.4d / 2.2065d))
                W2 = CvtDec(4.4d / 2.2065d);
            return W2;
        }
        private bool IsIndoorsMatchedW1MinAmount(SystemVRF sysItem, bool? isCassette, string[] modelsList = null, double[] capacityList = null)
        {
            string sId = sysItem.Id;
            //List<RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
            List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
            for (int i = 0; i < RoomIndoorList.Count; i++)
            {
                var ent = RoomIndoorList[i];
                //判断当前室内机是否属于该系统
                if (ent.SystemID != sId)
                {
                    continue;
                }

                //是否是Cassette
                if (isCassette != null)
                {
                    if (ent.IndoorItem?.Type?.EndsWith(" Cassette").CompareTo(isCassette) != 0)
                    {
                        return false;
                    }
                }

                //是否在型号范围内
                if (modelsList != null && !modelsList.Contains(ent.IndoorItem?.Model_Hitachi))
                {
                    return false;
                }

                //是否在容量范围内
                if (capacityList != null && !capacityList.Contains(ent.CoolingCapacity))
                {
                    return false;
                }
            }
            return true;
        }
        private decimal GetMiniValueByCNBCMQ(SystemVRF sysItem)
        {
            decimal dW = 0;

            //If the indoor units are Cassette, and the models are with in the models
            if (!IsIndoorsMatchedW1MinAmount(sysItem, true, new string[]
            {
                "RCI-1.0FSKDNQ",
                "RCI-1.5FSKDNQ",
                "RCI-3.0FSKDNQ",
                "RCI-3.3FSKDNQ"
            }))
            {
                return dW;
            }

            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            if (outItem != null)
            {
                Decimal pw = CvtDec(outItem.Horsepower);
                if (pw >= 8M && pw <= 10M)
                {
                    dW += 2M;
                }
                else if (pw == 12M)
                {
                    dW += 3M;
                }
                else if (pw >= 14M && pw <= 18M)
                {
                    dW += 3M;
                }
                else if (pw >= 20M && pw <= 24M)
                {
                    dW += 4M;
                }
                else if (pw == 26M)
                {
                    dW += 5M;
                }
                else if (pw >= 28M && pw <= 34M)
                {
                    dW += 6M;
                }
                else if (pw >= 36M && pw <= 42M)
                {
                    dW += 7M;
                }
                else if (pw >= 44M && pw <= 48M)
                {
                    dW += 8M;
                }
                else if (pw == 50M)
                {
                    dW += 9M;
                }
                else if (pw >= 52M && pw <= 58M)
                {
                    dW += 10M;
                }
                else if (pw >= 60M && pw <= 66M)
                {
                    dW += 11M;
                }
                else if (pw >= 68M && pw <= 72M)
                {
                    dW += 12M;
                }
                else if (pw == 74M)
                {
                    dW += 13;
                }
                else if (pw >= 76M && pw <= 78M)
                {
                    dW += 14M;
                }
                else if (pw >= 80M && pw <= 96M)
                {
                    dW += 16M;
                }
            }
            return dW;
        }
        /// <summary>
        /// 计算W2冷媒量
        /// </summary>
        /// <returns></returns>
        private decimal GetW2ForSMZ(SystemVRF sysItem)
        {
            decimal W2 = 0;
            string sId = sysItem.Id;
            int N1 = 0;
            int N2 = 0;
            int N3 = 0;
            List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
            for (int i = 0; i < RoomIndoorList.Count; i++)
            {
                var ent = RoomIndoorList[i];
                //判断当前室内机是否属于该系统
                if (ent.SystemID != sId)
                {
                    continue;
                }
                //if ("Four Way Cassette".Equals(ent.IndoorItem.Type) && !"Comm. Heat Recovery".Equals(ent.IndoorItem.ProductType) && !ent.IndoorItem.ProductType.Contains(", HR"))
                if (ent.IndoorItem.Type.Contains("Four Way Cassette") && !ent.IndoorItem.Type.Contains("Mini") && !"Comm. Heat Recovery".Equals(ent.IndoorItem.ProductType) && !ent.IndoorItem.ProductType.Contains(", HR"))
                {
                    //Regex reg = new Regex(@"^RCI-(.*?)FSN3$");
                    //var mat = reg.Match(ent.IndoorItem.Model_Hitachi);
                    //if (mat.Success)
                    if (ent.IndoorItem != null)
                    {
                        Decimal pw = CvtDec(ent.IndoorItem.Horsepower);
                        //double pw = Convert.ToDouble(mat.Groups[1].ToString());
                        if (pw == 2.0M)
                        {
                            N1++;
                        }
                        if (pw == 2.5M)
                        {
                            N2++;
                        }
                        if (pw >= 3.0M && pw <= 6.0M)
                        {
                            N3++;
                        }
                    }
                }
            }
            if (N1 > 0)
            {
                if (N1 == 1)
                {
                    //不额外添加
                }
                if (N1 == 2 || N1 == 3)
                {
                    W2 = W2 + 0.5M;
                }
                if (N1 >= 4)
                {
                    W2 = W2 + 1M;
                }
            }
            if (N2 > 0)
            {
                if (N2 == 1)
                {
                    //不额外添加
                }
                if (N2 == 2)
                {
                    W2 = W2 + 0.5M;
                }
                if (N2 == 3)
                {
                    W2 = W2 + 1M;
                }
                if (N2 >= 4)
                {
                    W2 = W2 + 1.5M;
                }
            }
            if (N3 > 0)
            {
                if (N3 == 1)
                {
                    W2 = W2 + 0.5M;
                }
                if (N3 == 2)
                {
                    W2 = W2 + 1M;
                }
                if (N3 == 3)
                {
                    W2 = W2 + 1.5M;
                }
                if (N3 >= 4)
                {
                    W2 = W2 + 2M;
                }
            }
            return W2;
        }

        private decimal GetMiniValueByOutPW(SystemVRF sysItem)
        {
            decimal dW = 0;
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            //string model = outItem.Model_Hitachi;
            //Regex reg = new Regex(@"-(([0-9]{1,}[.][0-9]*)|([0-9]{1,}))");
            //var mat = reg.Match(model);
            //if (mat.Success)
            //{
            //double pw = Convert.ToDouble(mat.Groups[1].ToString());
            if (outItem != null)
            {
                Decimal pw = CvtDec(outItem.Horsepower);
                if (pw == 8M || pw == 10M)
                {
                    dW += 2M;
                }
                else if (pw >= 12M && pw <= 20M)
                {
                    dW += 4M;
                }
                else if (pw == 22M)
                {
                    dW += 5M;
                }
                else if (pw >= 24M && pw <= 30M)
                {
                    dW += 6M;
                }
            }
            return dW;
        }

        private decimal GetMiniValueByHNBCMQ(SystemVRF sysItem)
        {
            decimal dW = 0;
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            if (outItem != null)
            {
                Decimal pw = CvtDec(outItem.Horsepower);
                if (pw >= 8M && pw <= 10M)
                {
                    dW += 2M;
                }
                else if (pw == 12M)
                {
                    dW += 3M;
                }
                else if (pw >= 14M && pw <= 18M)
                {
                    dW += 3M;
                }
                else if (pw >= 20M && pw <= 24M)
                {
                    dW += 4M;
                }
                else if (pw == 26M)
                {
                    dW += 5M;
                }
                else if (pw >= 28M && pw <= 34M)
                {
                    dW += 6M;
                }
                else if (pw >= 36M && pw <= 42M)
                {
                    dW += 7M;
                }
                else if (pw >= 44M && pw <= 48M)
                {
                    dW += 8M;
                }
                else if (pw == 50M)
                {
                    dW += 9M;
                }
                else if (pw >= 52M && pw <= 58M)
                {
                    dW += 10M;
                }
                else if (pw >= 60M && pw <= 66M)
                {
                    dW += 11M;
                }
                else if (pw >= 68M && pw <= 74M)
                {
                    dW += 12M;
                }
                else if (pw == 74M)
                {
                    dW += 13;
                }
                else if (pw >= 76M && pw <= 78M)
                {
                    dW += 14M;
                }
                else if (pw >= 80M && pw <= 96M)
                {
                    dW += 16M;
                }
            }
            return dW;
        }

        /// <summary>
        /// 8P或者10P室内机每一台额外添加1KG冷媒,16P及16P以上添加2KG
        /// </summary>
        /// <returns></returns>
        private decimal GetValueByPw(SystemVRF sysItem)
        {
            decimal dW = 0;
            string sId = sysItem.Id;
            List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;

            for (int i = 0; i < RoomIndoorList.Count; i++)
            {
                var ent = RoomIndoorList[i];
                //判断当前室内机是否属于该系统
                if (ent.SystemID != sId)
                {
                    continue;
                }
                //string model = ent.IndoorItem.Model_Hitachi;
                //Regex reg = new Regex(@"-(([0-9]{1,}[.][0-9]*)|([0-9]{1,}))");
                //var mat = reg.Match(model);
                //if (mat.Success)
                //{
                //double pw = Convert.ToDouble(mat.Groups[1].ToString());
                Decimal pw = CvtDec(ent.IndoorItem.Horsepower);
                if (pw == 8M || pw == 10M)
                {
                    dW = dW + 1M;
                }
                else if (pw == 16M || pw == 20M)
                {
                    dW += 2M; //16匹内机需要增加2kg冷媒，目前只有HAPB的Water source可能有16匹机 add on 20160517 by Yunxiao Lin
                }
                //}
            }

            return dW;
        }

        /// <summary>
        /// 18~36型号冷媒追加0.3kg,40~160型号追加0.5kg,224及以上追加1kg
        /// </summary>
        /// <returns></returns>
        private decimal GetValueByHNBCMQ(SystemVRF sysItem)
        {
            decimal dW = 0;
            string sId = sysItem.Id;
            List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;

            for (int i = 0; i < RoomIndoorList.Count; i++)
            {
                var ent = RoomIndoorList[i];
                //判断当前室内机是否属于该系统
                if (ent.SystemID != sId)
                {
                    continue;
                }
                int modelNo = Convert.ToInt32(ent.IndoorItem.ModelFull.Substring(4, 3).ToString());
                if (modelNo >= 18 && modelNo <= 36)
                {
                    dW = dW + 0.3M;
                }
                else if (modelNo >= 40 && modelNo <= 160)
                {
                    dW += 0.5M;
                }
                else if (modelNo <= 224)
                {
                    dW += 1M;
                }
                //}
            }

            return dW;
        }        
        private decimal GetMiniValueByJNBBTQ(SystemVRF sysItem)
        {
            decimal dW = 0;

            //If the indoor units are Cassette, and the capacities are 2.8kW, 4.0kW, 8.0kW, 9.0kW
            if (!IsIndoorsMatchedW1MinAmount(sysItem, true, null, new double[] { 2.8, 4.0, 8.0, 9.0 }))
            {
                return dW;
            }

            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            if (outItem != null)
            {
                Decimal pw = CvtDec(outItem.Horsepower);
                if (pw >= 8M && pw <= 10M)
                {
                    dW += 2M;
                }
                else if (pw == 12M)
                {
                    dW += 3M;
                }
                else if (pw == 14M)
                {
                    dW += 3M;
                }
                else if (pw == 16M)
                {
                    dW += 3.5M;
                }
                else if (pw == 18M)
                {
                    dW += 4M;
                }
            }
            return dW;
        }

        private decimal GetMaxValueByJNBBTQ(SystemVRF sysItem)
        {
            decimal dW = 0;

            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            if (outItem != null)
            {
                Decimal pw = CvtDec(outItem.Horsepower);
                if (pw >= 8 && pw <= 10)
                {
                    dW += 36;
                }
                else if (pw >= 12 && pw <= 20)
                {
                    dW += 40;
                }
                else if (pw >= 22 && pw <= 54)
                {
                    dW += 63;
                }
                else if (pw >= 56 && pw <= 72)
                {
                    dW += 73;
                }
            }
            return dW;
        }
        
        /// <summary>
        /// double转Decimal
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        private decimal CvtDec(double D)
        {
            return Convert.ToDecimal(D.ToString());
        }
        /// <summary>
        /// Decimal转double
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        private double CvtDou(decimal D)
        {
            return Convert.ToDouble(D.ToString());
        }

        /* 记录：追加制冷剂的Logic
         * 1、公式见Data文件《Additional refrigerant charge calculation 20160301.xlsx》
         */
        #endregion


        #endregion


        #region 输出EMF & DXF图--Piping & Wiring

        ///// 导出矢量图： Emf 或 Wmf 文件
        ///// <summary>
        ///// 导出矢量图： Emf 或 Wmf 文件
        ///// </summary>
        ///// <param name="filePath">文件路径</param>
        ///// <returns>是否成功</returns>
        //public bool ExportVictorGraph(string filePath, AddFlow addFlowItem, SystemVRF sysItem,bool isInch)
        //{
        //    try
        //    {
        //        Metafile emf = addFlowItem.ExportMetafile(false, true, true, false, false);

        //        Bitmap bmp = new Bitmap(emf.Size.Width, emf.Size.Height);
        //        Graphics gs = Graphics.FromImage(bmp);

        //        Metafile mf = new Metafile(filePath, gs.GetHdc());
        //        Graphics g = Graphics.FromImage(mf);

        //        DrawEMF_Piping(g, addFlowItem, sysItem, PipingImageDir, thisProject, isInch);

        //        g.Save();
        //        g.Dispose();
        //        gs.Dispose();
        //        mf.Dispose();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}


        ///// 绘制 EMF 图片(Piping)
        ///// <summary>
        ///// 绘制 EMF 图片(Piping)
        ///// </summary>
        ///// <param name="addFlow1"></param>
        ///// <param name="g"></param>
        //public void DrawEMF_Piping(Graphics g, AddFlow addFlowPiping, SystemVRF curSystemItem, string PipingImageDir, OldModel.Project thisProject, bool isInch)
        //{
        //    PipingBLL pipBll = new PipingBLL();
        //    bool isHitachi = thisProject.BrandCode == "H";
        //    foreach (Item item in addFlowPiping.Items)
        //    {
        //        if (item is Node)
        //        {
        //            Node nd = item as Node;
        //            if (nd is MyNodeYP)
        //            {
        //                if (!(nd as MyNodeYP).IsCP)
        //                {
        //                    utilEMF.DrawYP(g, nd, false);
        //                }
        //                else
        //                {
        //                    utilEMF.DrawYP(g, nd, true);
        //                }
        //            }
        //            else if (nd is MyNodeOut)
        //            {
        //                // 绘制室外机组合内部的YP型号以及连接管管径数据
        //                MyNodeOut ndOut = nd as MyNodeOut;
        //                string outModel = curSystemItem.OutdoorItem.Model;

        //                NodeElement_Piping itemOut = pipBll.GetPipingNodeOutElement(curSystemItem.OutdoorItem.ModelFull, isHitachi);
        //                string nodeImageFile = PipingImageDir + itemOut.Name + ".txt";

        //                if (itemOut.UnitCount == 1)
        //                {
        //                    if (isHitachi)
        //                        outModel = curSystemItem.OutdoorItem.Model_Hitachi;
        //                    utilEMF.DrawNode(g, nd, nodeImageFile, outModel, curSystemItem.Name);
        //                }
        //                else
        //                {
        //                    outModel = curSystemItem.OutdoorItem.AuxModelName;
        //                    utilEMF.DrawNode_OutdoorGroup(g, nd, nodeImageFile, outModel, curSystemItem.Name, itemOut,isInch);
        //                }
        //            }
        //            else if (nd is MyNodeIn)
        //            {
        //                MyNodeIn ndIn = nd as MyNodeIn;

        //                NodeElement_Piping itemInd = pipBll.GetPipingNodeIndElement(ndIn.RoomIndooItem.IndoorItem.Type, isHitachi);
        //                string nodeImageFile = PipingImageDir + itemInd.Name + ".txt";

        //                string model = ndIn.RoomIndooItem.IndoorItem.Model;
        //                if (isHitachi)
        //                    model = ndIn.RoomIndooItem.IndoorItem.Model_Hitachi;
        //                utilEMF.DrawNode(g, nd, nodeImageFile, model, ndIn.RoomIndooItem.IndoorName);
        //            }
        //        }
        //        else if (item is Link)
        //        {
        //            utilEMF.DrawLine(g, (Link)item);
        //        }
        //    }
        //}





        ///// 绘制 EMF 图片 (Wiring 导出DXF)
        ///// <summary>
        ///// 绘制 EMF 图片 (Wiring)
        ///// </summary>
        ///// <param name="addFlow1"></param>
        ///// <param name="g"></param>
        //public void DrawEMF_wiring(Graphics g, AddFlow addFlowWiring, SystemVRF curSystemItem)
        //{
        //    utilEMF.DrawEMF_wiring(g, addFlowWiring, curSystemItem, WiringImageDir, thisProject.BrandCode);
        //}

        ///// 绘制配线图
        ///// <summary>
        ///// 绘制配线图，界面显示
        ///// </summary>
        //private void DoDrawingWiring(TreeNode tnOut, AddFlow addFlowWiring, SystemVRF curSystemItem, string tabPageName)
        //{
        //    if (tabPageName != "tpgWiring" && tabPageName != "tpgReport")
        //        return;
        //    utilPiping.DoDrawingWiring(tnOut, addFlowWiring, curSystemItem, WiringImageDir, thisProject.BrandCode);
        //}

        #endregion


        /// 获取分歧管价格,TODO
        /// <summary>
        /// 获取分歧管价格,TODO
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string GetJointKitPrice(string model)
        {
            return "-";
            //return _dal.GetJointKitPrice(model);
        }

        #region PIPING过程中临时缓存的一些公共变量 add on 20160518 by Yunxiao Lin
        /// <summary>
        /// CH Unit到下端每个IDU的管长总和上限(临时缓存)
        /// </summary>
        public static double MaxCHToIndoorTotalLength = 0.0d;
        /// <summary>
        /// Main Branch数量上限(临时缓存)
        /// </summary>
        public static int MaxMainBranchCount = 0;
        /// <summary>
        /// Main Branch分支制冷容量最小比例(临时缓存)
        /// </summary>
        public static string MinMainBranchCoolingCapacityRate = "";
        /// <summary>
        /// Main Branch分支制热容量最小比例(临时缓存)
        /// </summary>
        public static string MinMainBranchHeatingCapacityRate = "";
        /// <summary>
        /// CH-Box允许连接的最大室内机数量(临时缓存)
        /// </summary>
        public static int MaxIndoorNumberConnectToCH = 0;

        #region   EU_NO_345 使用的公共变量   add on 20180607 by Vince
        /// <summary>
        /// MutiCH-Box允许连接的最大室内机匹数(临时缓存)
        /// </summary>
        public static double MaxIndoorCapacityConnectToMutiCH = 0d;
        /// <summary>
        /// 当前MutiCH-Box连接的室内机总匹数(临时缓存)
        /// </summary>
        public static double CurrentIndoorCapacityConnectToMutiCH = 0d;

        /// <summary>
        /// 需要提醒的实际管长
        /// </summary>
        public static double TempActualLength = 0d;

        /// <summary>
        /// 需要提醒的限制最小的实际管长管长
        /// </summary>
        public static double TempMaxLength = 0d;
        #endregion

        #endregion

        #region 从frmMain移植过来的方法 on 20170506 by Shen Junjie

        #region 绘制配管图过程

        #region 绘制节点 和 连接线

        /// 绘制配管图中的 Node 节点以及 Links
        /// <summary>
        /// 绘制配管图中的 Node 节点以及 Links
        /// </summary>
        public void DrawPipingNodes(SystemVRF sysItem, string dir, ref AddFlow addFlowPiping)
        {
            // 1、绘制室外机节点
            if (sysItem.OutdoorItem != null)
            {
                NodeElement_Piping outNodeItem = GetPipingNodeOutElement(sysItem, isHitachi);
                string imgFile = GetImagePath(sysItem.OutdoorItem.TypeImage);
                if (!File.Exists(imgFile))
                {
                    imgFile = GetImagePath("HNBQ 8-10, HNCQ 8-12.png");
                }
                string seriesName = sysItem.Series;
                MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
                nodeOut.NodeNo = sysItem.NO;
                nodeOut.UniqName = "Out" + Convert.ToString(sysItem.NO);

                nodeOut.ImageName = sysItem.Series;
                string outDoorModel = string.Empty;
                if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                {
                    outDoorModel = sysItem.OutdoorItem.Model_Hitachi;
                }
                else if (JCHVRF.Model.Project.CurrentProject.BrandCode == "Y" && sysItem.OutdoorItem.Model_York!="-")
                {
                    outDoorModel = sysItem.OutdoorItem.Model_York;
                }
                else
                {
                    outDoorModel = sysItem.OutdoorItem.ModelFull;
                }
                nodeOut.EquipmentType = "Outdoor";
                ImageData imageData = new ImageData
                {
                    NodeNo = sysItem.NO,
                    imageName = outDoorModel,
                    imagePath = imgFile,
                    equipmentType = "Outdoor",
                    UniqName = "Out" + Convert.ToString(sysItem.NO),
                    //coolingCapacity = Math.Round(sysItem.CoolingCapacity, 2),
                    //heatingCapacity = Math.Round(sysItem.HeatingCapacity, 2)
                    coolingCapacity = Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, ut_power),//Unit.ConvertToControl(Convert.ToDouble(sysItem.CoolingCapacity.ToString("n1")), UnitType.POWER, ut_power), // AddFlowExtension.FloatConverter(sysItem.CoolingCapacity.ToString("n1")),
                    heatingCapacity = Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, ut_power)//Unit.ConvertToControl(Convert.ToDouble(sysItem.HeatingCapacity.ToString("n1")), UnitType.POWER, ut_power) // AddFlowExtension.FloatConverter(sysItem.HeatingCapacity.ToString("n1"))

                };
                nodeOut.ImageData = imageData;
                //addFlowPiping.Items.Add(nodeOut);



                drawNodeImage(nodeOut, imgFile, imageData, sysItem.NO, ref addFlowPiping);
                if (!sysItem.IsManualPiping)
                    nodeOut.Location = new Point(50f, 20f + UtilPiping.HeightForNodeText);
                if (outNodeItem != null)
                {
                    nodeOut.UnitCount = outNodeItem.UnitCount; //室外机组合中机组数目  add by Shen Junjie on 20170718
                }
                sysItem.MyPipingNodeOut.Model = sysItem.OutdoorItem.AuxModelName;
                sysItem.MyPipingNodeOut.Name = sysItem.Name;

                // 2、绘制室外机的第一个子节点（ YP | CP | Single Indoor）
                Node firstChildNode = nodeOut.ChildNode;

                //绘制所有节点（除室外机）的图片
                //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                //string navigateToFolder = "..\\..\\Image\\TypeImages";
                //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                DrawChildNodesImage(nodeOut, firstChildNode, dir, ref addFlowPiping);

                if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
                {
                    //计算所有节点（除室外机）的位置
                    float maxY = AddFlowExtension.FloatConverter(nodeOut.Location.Y);
                    float maxX = 0;
                    LayoutBinaryTree(sysItem, nodeOut, firstChildNode, ref maxX, ref maxY, ref addFlowPiping);
                }
                else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.Symmetries)
                {
                    float offset = 0;
                    LayoutSymmetries(sysItem, nodeOut, firstChildNode, false, 0, ref offset, ref addFlowPiping);
                }
                else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.SchemaA)
                {
                    float minX = 0, minY = 0, offset = 0;
                    float[] maxPositions = new float[2];
                    LayoutSchemaA(sysItem, nodeOut, firstChildNode, OldModel.PipingOrientation.Unknown,
                        minX, minY, ref maxPositions, ref offset, ref addFlowPiping);
                }
                else
                {
                    float maxY = AddFlowExtension.FloatConverter(nodeOut.Location.Y);
                    LayoutNormal(sysItem, nodeOut, firstChildNode, dir, ref maxY, ref addFlowPiping);
                }
                //drawTextToODUNode(nodeOut, outNodeItem, sysItem, ref addFlowPiping);
            }
        }
        public void DrawManualPipingNodes(SystemVRF sysItem, string dir, ref AddFlow addFlowPiping)
        {
            // 1、绘制室外机节点
            if (sysItem.OutdoorItem != null)
            {
                NodeElement_Piping outNodeItem = GetPipingNodeOutElement(sysItem, isHitachi);
                string imgFile = GetImagePath(sysItem.OutdoorItem.TypeImage);
                if (!File.Exists(imgFile))
                {
                    imgFile = GetImagePath("HNBQ 8-10, HNCQ 8-12.png");
                }
                string seriesName = sysItem.Series;
                MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
                nodeOut.NodeNo = sysItem.NO;
                nodeOut.UniqName = "Out" + Convert.ToString(sysItem.NO);

                nodeOut.ImageName = sysItem.Series;
                string outDoorModel = string.Empty;
                if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                {
                    outDoorModel = sysItem.OutdoorItem.Model_Hitachi;
                }

                else
                {
                    outDoorModel = sysItem.OutdoorItem.ModelFull;
                }
                nodeOut.EquipmentType = "Outdoor";
                ImageData imageData = new ImageData
                {
                    NodeNo = sysItem.NO,
                    imageName = outDoorModel,
                    imagePath = imgFile,
                    equipmentType = "Outdoor",
                    UniqName = "Out" + Convert.ToString(sysItem.NO),
                    coolingCapacity = Math.Round(sysItem.CoolingCapacity, 2),
                    heatingCapacity = Math.Round(sysItem.HeatingCapacity, 2)
                };
                nodeOut.ImageData = imageData;
                //addFlowPiping.Items.Add(nodeOut);



                drawNodeImage(nodeOut, imgFile, imageData, sysItem.NO, ref addFlowPiping);

                if (outNodeItem != null)
                {
                    nodeOut.UnitCount = outNodeItem.UnitCount; //室外机组合中机组数目  add by Shen Junjie on 20170718
                }
                sysItem.MyPipingNodeOut.Model = sysItem.OutdoorItem.AuxModelName;
                sysItem.MyPipingNodeOut.Name = sysItem.Name;

                // 2、绘制室外机的第一个子节点（ YP | CP | Single Indoor）
                Node firstChildNode = nodeOut.ChildNode;

                //绘制所有节点（除室外机）的图片
                //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                //string navigateToFolder = "..\\..\\Image\\TypeImages";
                //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                DrawChildNodesImage(nodeOut, firstChildNode, dir, ref addFlowPiping);

                LayoutManual(sysItem, nodeOut, firstChildNode, dir, ref addFlowPiping);
                //drawTextToODUNode(nodeOut, outNodeItem, sysItem, ref addFlowPiping);
            }
        }
        public void DrawOrphanIndoorNodes(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            if (sysItem.MyPipingOrphanNodes != null)
            {
                foreach (var node in sysItem.MyPipingOrphanNodes)
                {
                    if (node is MyNodeIn)
                    {
                        MyNodeIn nodeIn = node as MyNodeIn;
                        if (nodeIn.RoomIndooItem == null) return;
                        ImageData imageData = new ImageData();
                        string imgFile = nodeIn.RoomIndooItem.DisplayImagePath;
                        if (imgFile.Contains("TypeImageProjectCreation"))
                        {
                            imgFile = imgFile.Replace("\\TypeImageProjectCreation\\", "\\TypeImages\\");
                            if (!File.Exists(imgFile))
                                imgFile = nodeIn.RoomIndooItem.DisplayImagePath;
                        }
                        string imgName = nodeIn.RoomIndooItem.DisplayImageName;
                        int nodeNo = nodeIn.RoomIndooItem.IndoorNO;
                        if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                            imageData.imageName = nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi;
                        else
                            imageData.imageName = nodeIn.RoomIndooItem.IndoorItem.ModelFull;
                        imageData.coolingCapacity = Math.Round(nodeIn.RoomIndooItem.CoolingCapacity, 2);
                        imageData.heatingCapacity = Math.Round(nodeIn.RoomIndooItem.HeatingCapacity, 2);
                        imageData.sensibleHeat = Math.Round(nodeIn.RoomIndooItem.SensibleHeat, 2);
                        if (!string.IsNullOrEmpty(nodeIn.RoomIndooItem.IndoorName))
                        {
                            imageData.UniqName = nodeIn.RoomIndooItem.IndoorName;
                        }
                        imageData.floorName = nodeIn.RoomIndooItem.SelectedFloor != null ? nodeIn.RoomIndooItem.SelectedFloor.Name : "";
                        imageData.roomName = nodeIn.RoomIndooItem.SelectedRoom != null ? nodeIn.RoomIndooItem.SelectedRoom.Name : "";
                        drawNodeImage(nodeIn, imgFile, imageData, nodeNo, ref addFlowPiping);
                    }
                }
            }
        }

        private void setNodeStyle(Node node, string imageName)
        {
            node.Geometry = new RectangleGeometry(new Rect(0, 0, 61, 65), 3, 3);
            node.Stroke = System.Windows.Media.Brushes.RoyalBlue;
            node.Fill = System.Windows.Media.Brushes.White;
            node.Text = imageName;
            node.ImageMargin = new Thickness(3);
            node.TextMargin = new Thickness(3);
            node.ImagePosition = ImagePosition.CenterTop;
            node.TextPosition = TextPosition.CenterBottom;
            node.FontSize = 9;
            // node.Tooltip = data.equipmentType + " : " + data.imageName;
            node.IsSelectable = true;
        }


        private string GetImagePath(string model)
        {
            if (File.Exists(model)) { return model; }
            string imageFullPath = "";
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\TypeImages";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            imageFullPath = sourceDir + "\\" + model;

            imageFullPath = File.Exists(imageFullPath) ? imageFullPath : imageFullPath.Replace("TypeImages", "TypeImageProjectCreation");

            return imageFullPath;
        }

        /// <summary>
        /// 递归设置所有indoor 和 CHBox 的图片路径
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dir"></param>
        private void DrawChildNodesImage(Node parent, Node node, string dir, ref AddFlow addFlowPiping)
        {
            if (node == null) return;
            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    DrawChildNodesImage(nodeYP, nodeYP.ChildNodes[i], dir, ref addFlowPiping);
                }
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                if (nodeIn.RoomIndooItem == null) return;
                //NodeElement_Piping indNodeItem = GetPipingNodeIndElement(nodeIn.RoomIndooItem.IndoorItem.Type, isHitachi);
                //if (indNodeItem != null)
                //{
                ImageData imageData = new ImageData();
                string imgFile = nodeIn.RoomIndooItem.DisplayImagePath;

                //Start Backward Compatibility : Added a null/empty check to avoid Null reference exception.
                if (string.IsNullOrEmpty(imgFile))
                {
                    imgFile = nodeIn.RoomIndooItem.DisplayImagePath = string.IsNullOrEmpty(nodeIn.RoomIndooItem.IndoorItem.TypeImage) ? string.Empty : Path.Combine(dir, nodeIn.RoomIndooItem.IndoorItem.TypeImage);
                }
                //End Backward Compatibility : Added a null/empty check to avoid Null reference exception.

                if (imgFile.Contains("TypeImageProjectCreation"))
                {
                    imgFile = imgFile.Replace("\\TypeImageProjectCreation\\", "\\TypeImages\\");
                    if (!File.Exists(imgFile))
                        imgFile = nodeIn.RoomIndooItem.DisplayImagePath;
                }
                string imgName = nodeIn.RoomIndooItem.DisplayImageName;
                int nodeNo = nodeIn.RoomIndooItem.IndoorNO;
                if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                    imageData.imageName = nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi;
                else if(!string.IsNullOrEmpty(nodeIn.RoomIndooItem.IndoorItem.Model_York) && nodeIn.RoomIndooItem.IndoorItem.Model_York!="-")
                    imageData.imageName = nodeIn.RoomIndooItem.IndoorItem.Model_York;
                else
                    imageData.imageName = nodeIn.RoomIndooItem.IndoorItem.ModelFull;
                imageData.coolingCapacity =Unit.ConvertToControl(nodeIn.RoomIndooItem.ActualCoolingCapacity, UnitType.POWER, ut_power); //Unit.ConvertToControl(Convert.ToDouble(nodeIn.RoomIndooItem.ActualCoolingCapacity.ToString("n1")), UnitType.POWER, ut_power);//AddFlowExtension.FloatConverter(nodeIn.RoomIndooItem.ActualCoolingCapacity.ToString("n1"));
                imageData.heatingCapacity = Unit.ConvertToControl(nodeIn.RoomIndooItem.ActualHeatingCapacity, UnitType.POWER, ut_power);//Unit.ConvertToControl(Convert.ToDouble(nodeIn.RoomIndooItem.ActualHeatingCapacity.ToString("n1")), UnitType.POWER, ut_power);//AddFlowExtension.FloatConverter(nodeIn.RoomIndooItem.ActualHeatingCapacity.ToString("n1"));
                imageData.sensibleHeat = Unit.ConvertToControl(nodeIn.RoomIndooItem.ActualSensibleHeat, UnitType.POWER, ut_power); //Unit.ConvertToControl(Convert.ToDouble(nodeIn.RoomIndooItem.ActualSensibleHeat.ToString("n1")), UnitType.POWER, ut_power);//AddFlowExtension.FloatConverter(nodeIn.RoomIndooItem.ActualSensibleHeat.ToString("n1"));
                if (!string.IsNullOrEmpty(nodeIn.RoomIndooItem.IndoorName))
                {
                    imageData.UniqName = nodeIn.RoomIndooItem.IndoorName;
                }
                //To Do need to implement null checks
                if (!File.Exists(imgFile))
                {
                    imgFile = GetImagePath("RWLT-10.0VNE.png");
                }
                imageData.floorName = nodeIn.RoomIndooItem.SelectedFloor != null ? nodeIn.RoomIndooItem.SelectedFloor.Name : "";
                imageData.roomName = nodeIn.RoomIndooItem.SelectedRoom != null ? nodeIn.RoomIndooItem.SelectedRoom.Name : "";

                drawNodeImage(nodeIn, imgFile, imageData, nodeNo, ref addFlowPiping);
                //}
            }
            else if (node is MyNodeCH)
            {
                // 仅3Pipe类型才添加CHbox节点，暂时只处理一个CHbox连接一个IDU
                MyNodeCH nodeCH = node as MyNodeCH;
                string imgFile = dir + "CHbox.png";
                drawCHImage(nodeCH, imgFile, ref addFlowPiping);

                DrawChildNodesImage(nodeCH, nodeCH.ChildNode, dir, ref addFlowPiping);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                //PipingMultiCHBox multiCHBox = _dal.GetMultiCHBox(nodeMCH.Model);
                //if (multiCHBox != null)
                //{
                //    //imgFile = dir + multiCHBox.Image;
                //}
                //string imgFile = dir + "multiCHBox.png";
                //drawNodeImage(nodeMCH, imgFile,  ref addFlowPiping);
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    DrawChildNodesImage(nodeMCH, nodeMCH.ChildNodes[i], dir, ref addFlowPiping);
                }
            }
        }


        /// <summary>
        /// 计算所有节点（除室外机）的位置 (二叉树布局)
        /// </summary>
        private void LayoutBinaryTree(SystemVRF sysItem, Node parent, Node node, ref float maxX, ref float maxY, ref AddFlow addFlowPiping)
        {
            if (node == null) return;
            addFlowPiping.Items.Add(node);
            
            bool isVertical = sysItem.IsPipingVertical;
            if (isVertical)
            {
                node.Location = utilPiping.GetBinaryTreeNodeLocationVertical(parent, node, maxY);
                Point ptNode = utilPiping.getBottomCenterPointF(node);
                maxY = AddFlowExtension.FloatConverter(Math.Max(maxY, ptNode.Y));
            }
            else
            {
                //横向布局                
                node.Location = utilPiping.GetBinaryTreeNodeLocationHorizontal(parent, node, maxX);
                Point ptNode = utilPiping.getCenterPointF(node);
                maxX = AddFlowExtension.FloatConverter(Math.Max(maxX, ptNode.X));
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                string ImageName = utilPiping.manageYBranchOrientation(parent, nodeYP, isVertical, sysItem.PipingLayoutType.ToString());
                string sourceDir = GetcompleteImagepath(ImageName);
                //System.Windows.Controls.Image ypImage = new System.Windows.Controls.Image();
                //ypImage.Source = new BitmapImage(new Uri(sourceDir));
                //var svgFilePath = sourceDir.Replace(".png", ".svg");
                CreateMyNodeYpFromSvg(sourceDir, nodeYP);

                //nodeYP.Image = ypImage.Source;
                //nodeYP.Size = new System.Windows.Size(24, 24);
                nodeYP.StrokeThickness = 0;
                nodeYP.IsEditable = false;
                nodeYP.IsSelectable = true;
                nodeYP.ImageData = new ImageData();
                addFlowPiping.AddNode(nodeYP);

                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    LayoutBinaryTree(sysItem, nodeYP, nodeYP.ChildNodes[i], ref maxX, ref maxY, ref addFlowPiping);
                }                               
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                LayoutBinaryTree(sysItem, nodeCH, nodeCH.ChildNode, ref maxX, ref maxY, ref addFlowPiping);

                Point ptChild = utilPiping.getCenterPointF(nodeCH.ChildNode);
                if (isVertical)
                {
                    //nodeCH.Location = new Point(nodeCH.Location.X, ptChild.Y - nodeCH.Size.Height / 2);
                }
                else
                {
                    nodeCH.Location = new Point(ptChild.X - nodeCH.Size.Width / 2, nodeCH.Location.Y);
                }                
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    LayoutBinaryTree(sysItem, nodeMCH, nodeMCH.ChildNodes[i], ref maxX, ref maxY, ref addFlowPiping);
                }                
            }
        }

        /// <summary>
        /// 计算所有节点（除室外机）的位置 (对称布局)
        /// </summary>
        private void LayoutSymmetries(SystemVRF sysItem, Node parent, Node node, bool forward, float minXY, ref float offset, ref AddFlow addFlowPiping)
        {
            if (node == null) return;
            bool isVertical = sysItem.IsPipingVertical;
            float subOffset = 0;
            if (parent is MyNodeOut)
            {
                if (isVertical)
                {
                    //最小X坐标
                    minXY = 50;
                }
                else
                {
                    //最小Y坐标
                    minXY = AddFlowExtension.FloatConverter(parent.Location.Y) + AddFlowExtension.FloatConverter(parent.Size.Height + 40);
                }
            }

            addFlowPiping.Items.Add(node);

            Point ptNode;
            if (isVertical)
            {
                //纵向布局
                node.Location = utilPiping.GetSymmetriesLayoutNodeLocationVertical(parent, node, forward, minXY, out subOffset);
                ptNode = utilPiping.getCenterPointF(node);
            }
            else
            {
                //横向布局
                node.Location = utilPiping.GetSymmetriesLayoutNodeLocationHorizontal(parent, node, forward, minXY, out subOffset);
                ptNode = utilPiping.getCenterPointF(node);
            }

            offset += subOffset;
            subOffset = 0;

            if (node is MyNodeYP)
            {
                MyNodeYP ypNode = node as MyNodeYP;
                string ImageName = "YBranch_Seperator_Symmetrical";
                string sourceDir = GetcompleteImagepath(ImageName);
                System.Windows.Controls.Image ypImage = new System.Windows.Controls.Image();
                ypImage.Source = new BitmapImage(new Uri(sourceDir));
                ypNode.Image = ypImage.Source;
                ypNode.Size = new System.Windows.Size(24, 24);
                ypNode.StrokeThickness = 0;
                ypNode.IsEditable = false;
                ypNode.IsSelectable = false;
                ypNode.ImageData = new ImageData();
                addFlowPiping.AddNode(ypNode);

                if (ypNode.IsCP)
                {
                    for (int i = 0; i < ypNode.ChildCount; ++i)
                    {
                        bool tempForward = (i % 2 == 1) ? !forward : forward;
                        LayoutSymmetries(sysItem, ypNode, ypNode.ChildNodes[i], tempForward, minXY, ref subOffset, ref addFlowPiping);
                    }
                }
                else
                {
                    LayoutSymmetries(sysItem, ypNode, ypNode.ChildNodes[0], forward, minXY, ref subOffset, ref addFlowPiping);

                    if (!forward && subOffset != 0)
                    {
                        if (isVertical)
                        {
                            node.Location = new Point(node.Location.X + subOffset, node.Location.Y);
                        }
                        else
                        {
                            node.Location = new Point(node.Location.X, node.Location.Y + subOffset);
                        }
                        offset += subOffset;
                    }
                    subOffset = 0;
                    LayoutSymmetries(sysItem, ypNode, ypNode.ChildNodes[1], !forward, minXY, ref subOffset, ref addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                LayoutSymmetries(sysItem, nodeCH, nodeCH.ChildNode, forward, minXY, ref subOffset, ref addFlowPiping);

                if (!forward && subOffset != 0)
                {
                    if (isVertical)
                    {


                        node.Location = new Point(node.Location.X + subOffset, node.Location.Y);
                    }
                    else
                    {
                        node.Location = new Point(node.Location.X, node.Location.Y + subOffset);
                    }
                    offset += subOffset;
                }
            }
        }

        /// <summary>
        /// 计算所有节点（除室外机）的位置 (对称布局)
        /// </summary>
        private void LayoutSchemaA(SystemVRF sysItem, Node parent, Node node, OldModel.PipingOrientation orientation,
            float minX, float minY, ref float[] maxPositions, ref float offset, ref AddFlow addFlowPiping)
        {
            if (node == null) return;
            bool isVertical = sysItem.IsPipingVertical;
            float subOffset = 0;
            if (parent is MyNodeOut)
            {
                if (isVertical)
                {
                    //最小X坐标
                    minX = 50;

                    //纵向布局时，室外机要在中间。
                    if (node is MyNodeYP && !(node as MyNodeYP).IsCP)
                    {
                        //parent.Location = new Point(200f, 5f + UtilPiping.HeightForNodeText);
                        parent.Location = new Point(0f, 5f + UtilPiping.HeightForNodeText);
                    }
                    orientation = OldModel.PipingOrientation.Right;
                }
                else
                {
                    //最小Y坐标
                    minY = AddFlowExtension.FloatConverter(parent.Location.Y + parent.Size.Height + 40);
                    orientation = OldModel.PipingOrientation.Down;
                }
            }
            addFlowPiping.Items.Add(node);

            if (orientation == OldModel.PipingOrientation.Right || orientation == OldModel.PipingOrientation.Left)
            {
                //纵向布局
                bool orientateRight = orientation == OldModel.PipingOrientation.Right;
                int mp = orientateRight ? 0 : 1;
                node.Location = utilPiping.GetSchemaALayoutNodeLocationVertical(parent, node, orientateRight,
                    minX, ref maxPositions[mp], out subOffset);
            }
            else
            {
                //横向布局
                bool orientateUp = orientation == OldModel.PipingOrientation.Up;
                int mp = orientateUp ? 0 : 1;
                node.Location = utilPiping.GetSchemaALayoutNodeLocationHorizontal(parent, node, orientateUp,
                    minY, ref maxPositions[mp], out subOffset);
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                string ImageName = utilPiping.manageYBranchOrientation(parent, nodeYP, isVertical, sysItem.PipingLayoutType.ToString());
                string sourceDir;
                //Draw YBranch image
                sourceDir = GetcompleteImagepath(ImageName);
                CreateMyNodeYpFromSvg(sourceDir, nodeYP);

                nodeYP.StrokeThickness = 0;
                nodeYP.IsEditable = false;
                nodeYP.IsSelectable = true;
                nodeYP.ImageData = new ImageData();
                addFlowPiping.AddNode(nodeYP);
                if (nodeYP.IsCP)
                {
                    for (int i = 0; i < nodeYP.ChildCount; ++i)
                    {
                        LayoutSchemaA(sysItem, nodeYP, nodeYP.ChildNodes[i], orientation, minX, minY, ref maxPositions, ref subOffset, ref addFlowPiping);
                    }
                }
                else
                {
                    OldModel.PipingOrientation orientation1 = orientation;
                    OldModel.PipingOrientation orientation2 = orientation;
                    Node child1, child2;
                    child1 = nodeYP.ChildNodes[0];
                    child2 = nodeYP.ChildNodes[1];
                    if (isVertical)
                    {
                        if (parent is MyNodeOut)
                        {
                            child2 = nodeYP.ChildNodes[0];
                            child1 = nodeYP.ChildNodes[1];
                            orientation2 = OldModel.PipingOrientation.Right;
                        }
                    }
                    else
                    {
                        if (parent is MyNodeOut)
                        {
                            //ACC-RAG from Up to Down
                            orientation1 = OldModel.PipingOrientation.Down;
                        }
                    }
                    LayoutSchemaA(sysItem, nodeYP, child1, orientation1, minX, minY, ref maxPositions, ref subOffset, ref addFlowPiping);
                    LayoutSchemaA(sysItem, nodeYP, child2, orientation2, minX, minY, ref maxPositions, ref subOffset, ref addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                OldModel.PipingOrientation orientation1 = orientation;
                if (parent is MyNodeOut)
                {
                    orientation1 = isVertical ? OldModel.PipingOrientation.Right : OldModel.PipingOrientation.Down;
                }
                LayoutSchemaA(sysItem, nodeCH, nodeCH.ChildNode, orientation1, minX, minY, ref maxPositions, ref subOffset, ref addFlowPiping);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                OldModel.PipingOrientation orientation1 = orientation;
                if (parent is MyNodeOut)
                {
                    orientation1 = isVertical ? OldModel.PipingOrientation.Right : OldModel.PipingOrientation.Down;
                }
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    LayoutSchemaA(sysItem, nodeMCH, nodeMCH.ChildNodes[i], orientation1, minX, minY, ref maxPositions, ref subOffset, ref addFlowPiping);
                }
            }

            if (subOffset > 0)
            {
                if (isVertical)
                {
                    parent.Location = new Point(parent.Location.X + subOffset, parent.Location.Y);
                }
                else if (!(parent is MyNodeOut))
                {
                    parent.Location = new Point(parent.Location.X, parent.Location.Y + subOffset);
                }
                offset += subOffset;
            }
        }

        /// 递归程序，绘制 YP | CP 节点组
        /// <summary>
        /// 递归程序，绘制 YP | CP 节点组
        /// </summary>
        /// <param name="pn"></param>
        private void LayoutNormal(SystemVRF sysItem, Node parent, Node node, string dir, ref float maxY, ref AddFlow addFlowPiping)
        {
            try
            {
                if (node == null)
                {
                    return;
                }

                if (!addFlowPiping.Items.Contains(node))
                {
                    addFlowPiping.AddNode(node);
                }
                bool isVertical = sysItem.IsPipingVertical;
                if (node is MyNodeYP)
                {
                    MyNodeYP nodeYP = node as MyNodeYP;

                    string ImageName = utilPiping.manageYBranchOrientation(parent, nodeYP, isVertical, sysItem.PipingLayoutType.ToString());
                    string sourceDir = GetcompleteImagepath(ImageName);
                    //System.Windows.Controls.Image ypImage = new System.Windows.Controls.Image();
                    //ypImage.Source = new BitmapImage(new Uri(sourceDir));
                    //var svgFilePath = sourceDir.Replace(".png", ".svg");

                    CreateMyNodeYpFromSvg(sourceDir, nodeYP);
                    //nodeYP.Image = ypImage.Source; //No need to apply image source as Yp node is now using Geometry
                    nodeYP.StrokeThickness = 0;
                    nodeYP.IsEditable = false;
                    nodeYP.AddFlow.BringToFront();
                    nodeYP.IsSelectable = true;
                    nodeYP.ImageData = new ImageData();
                    nodeYP.ImageData.UniqName = "Sep" + addFlowPiping.Items.OfType<MyNodeYP>().Count();
                    nodeYP.ImageData.equipmentType = "Pipe";
                    // addFlowPiping.AddNode(nodeYP);
                    if (!sysItem.IsManualPiping)
                        // 必须在节点加载后确定坐标位置，因为当子节点node（主要是InNode）的Size为0时，Piping中显示不正确
                        node.Location = utilPiping.getLocationChild(parent, node, isVertical, ref maxY);


                    if (isVertical)
                    {
                        for (int i = 0; i < nodeYP.ChildCount; ++i)
                        {
                            LayoutNormal(sysItem, nodeYP, nodeYP.ChildNodes[i], dir, ref maxY, ref addFlowPiping);

                            if (i == 0)
                            {
                                //如果子节点向下调整了位置，则需要同步调整
                                Point ptFirstChild = utilPiping.getCenterPointF(nodeYP.ChildNodes[0]);
                                if (utilPiping.getCenterPointF(node).Y < ptFirstChild.Y)
                                {
                                    if (!sysItem.IsManualPiping)
                                        node.Location = new Point(node.Location.X, ptFirstChild.Y - node.Size.Height / 2);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = nodeYP.ChildCount - 1; i >= 0; --i)
                        {
                            LayoutNormal(sysItem, nodeYP, nodeYP.ChildNodes[i], dir, ref maxY, ref addFlowPiping);
                        }
                    }
                }
                else if (node is MyNodeCH)
                {
                    // 仅3Pipe类型才添加CHbox节点，暂时只处理一个CHbox连接一个IDU
                    MyNodeCH nodeCH = node as MyNodeCH;
                    //string imgFile = dir + "CHbox.png";
                    //drawNodeImage(nodeCH, imgFile, ref addFlowPiping);
                    if (!sysItem.IsManualPiping)
                        // 必须在节点加载后确定坐标位置，因为当子节点node（主要是InNode）的Size为0时，Piping中显示不正确
                        node.Location = utilPiping.getLocationChild(parent, node, isVertical, ref maxY);
                    node.IsSelectable = true;
                    LayoutNormal(sysItem, nodeCH, nodeCH.ChildNode, dir, ref maxY, ref addFlowPiping);
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                    if (!sysItem.IsManualPiping)
                        node.Location = utilPiping.getLocationChild(parent, node, isVertical, ref maxY);
                    node.IsSelectable = false;
                    for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                    {
                        LayoutNormal(sysItem, nodeMCH, nodeMCH.ChildNodes[i], dir, ref maxY, ref addFlowPiping);
                    }

                    if (node is MyNodeMultiCH)
                    {
                        maxY = AddFlowExtension.FloatConverter(Math.Max(maxY, node.Location.Y + node.Size.Height - 40));
                    }
                }
                else if (node is MyNodeIn)
                {
                    MyNodeIn nodeIn = node as MyNodeIn;
                    //NodeElement_Piping indNodeItem = GetPipingNodeIndElement(nodeIn.RoomIndooItem.IndoorItem.Type, isHitachi);
                    //string imgFile = dir + indNodeItem.Name + ".png";

                    //drawNodeImage(nodeIn, imgFile, ref addFlowPiping);
                    if (!sysItem.IsManualPiping)
                        node.Location = utilPiping.getLocationChild(parent, node, isVertical, ref maxY);
                    maxY = AddFlowExtension.FloatConverter(Math.Max(maxY, utilPiping.getCenterPointF(node).Y));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void LayoutManual(SystemVRF sysItem, Node parent, Node node, string dir, ref AddFlow addFlowPiping)
        {
            try
            {
                if (node == null)
                {
                    return;
                }

                if (!addFlowPiping.Items.Contains(node))
                {
                    addFlowPiping.AddNode(node);
                }
                bool isVertical = sysItem.IsPipingVertical;
                if (node is MyNodeYP)
                {
                    MyNodeYP nodeYP = node as MyNodeYP;

                    string ImageName = utilPiping.manageYBranchOrientation(parent, nodeYP, isVertical, sysItem.PipingLayoutType.ToString());
                    string sourceDir = GetcompleteImagepath(ImageName);
                    //System.Windows.Controls.Image ypImage = new System.Windows.Controls.Image();
                    //ypImage.Source = new BitmapImage(new Uri(sourceDir));
                    //var svgFilePath = sourceDir.Replace(".png", ".svg");

                    CreateMyNodeYpFromSvg(sourceDir, nodeYP);
                    //nodeYP.Image = ypImage.Source; //No need to apply image source as Yp node is now using Geometry
                    nodeYP.StrokeThickness = 0;
                    nodeYP.IsEditable = false;
                    nodeYP.AddFlow.BringToFront();
                    nodeYP.IsSelectable = true;
                    nodeYP.ImageData = new ImageData();
                    nodeYP.ImageData.UniqName = "Sep" + addFlowPiping.Items.OfType<MyNodeYP>().Count();
                    nodeYP.ImageData.equipmentType = "Pipe";

                    for (int i = nodeYP.ChildCount - 1; i >= 0; --i)
                    {
                        LayoutManual(sysItem, nodeYP, nodeYP.ChildNodes[i], dir, ref addFlowPiping);
                    }

                }
                else if (node is MyNodeCH)
                {
                    MyNodeCH nodeCH = node as MyNodeCH;
                    node.IsSelectable = true;
                    LayoutManual(sysItem, nodeCH, nodeCH.ChildNode, dir, ref addFlowPiping);
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;

                    node.IsSelectable = false;
                    for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                    {
                        LayoutManual(sysItem, nodeMCH, nodeMCH.ChildNodes[i], dir,ref addFlowPiping);
                    }
                }
                //else if (node is MyNodeIn)
                //{
                //    MyNodeIn nodeIn = node as MyNodeIn;                   
                //}
            }
            catch (Exception ex)
            {

            }
        }

        private void setYPNodeStyle(Node nodeYP)
        {
            Point inPin = new Point(10, 52);
            Point outPin = new Point(120, 52);
            Point dropPin = new Point(52, 135);
            //inPin = utilPiping.getLeftCenterPointF(nodeYP);
            //outPin = utilPiping.getRightCenterPointF(nodeYP);
            //dropPin = utilPiping.getBottomCenterPointF(nodeYP);
            nodeYP.PinsLayout = new PointCollection { inPin, outPin, dropPin };
            nodeYP.AddFlow.PinSize = 6;
            nodeYP.AddFlow.PinStroke = Brushes.Aqua;
            nodeYP.AddFlow.PinFill = Brushes.Aqua;
        }

        public void CreateMyNodeYpFromSvg(string svgFilePath, MyNodeYP nodeYP)
        {
            #region My Node YP using SVG

            var doc = new XmlDocument();
            doc.Load(svgFilePath);

            XmlNode root = doc.DocumentElement;

            //var nodeList = doc.GetElementsByTagName("g");
            //var xmlNode = nodeList[0].FirstChild;
            var xmlNode = root.FirstChild;

            int width = 32;
            int height = 16;

            if(root.Attributes["viewBox"] != null)
            {
                var valArray = root.Attributes["viewBox"].Value.Split(' ');
                if(valArray.Length > 3 && Convert.ToInt32(valArray[2]) < Convert.ToInt32(valArray[3]))
                {
                    width = 16;
                    height = 32;
                }
            }
            //if (root.Attributes["width"] != null)
            //{
            //    int.TryParse(root.Attributes["width"].Value, out width);
            //    int.TryParse(root.Attributes["height"].Value, out height);
            //}
            //else
            //{
            //    width = 31;
            //    height = 16;
            //}

            var geometry = xmlNode.Attributes["d"].Value;
            var colorCode = xmlNode.Attributes["fill"].Value;

            nodeYP.Geometry = Geometry.Parse(geometry);
            nodeYP.Size = new System.Windows.Size(width, height);
            nodeYP.Image = null;
            var brush = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorCode));
            nodeYP.Fill = brush;

            utilPiping.SetNodeYpPinLayOut(nodeYP);

            #endregion
        }

        private void GetGeomatryFromSvg(string svgFilePath, MyNodeYP nodeYP)
        {
            var doc = new XmlDocument();
            doc.Load(svgFilePath);

            XmlNode root = doc.DocumentElement;

            var nodeList = doc.GetElementsByTagName("g");
            if (nodeList.Count == 0) return;
            var xmlNode = nodeList[0].FirstChild;
            //var xmlNode = root.FirstChild;

            int width = 0;
            int height = 0;
            if (root.Attributes["width"] != null)
            {
                int.TryParse(root.Attributes["width"].Value, out width);
                int.TryParse(root.Attributes["height"].Value, out height);
            }
            else
            {
                width = 31;
                height = 16;
            }

            var geometry = xmlNode.Attributes["d"].Value;
            var colorCode = xmlNode.Attributes["fill"].Value;

            nodeYP.Geometry = Geometry.Parse(geometry);
            nodeYP.Size = new System.Windows.Size(width, height);
            nodeYP.Image = null;
            var brush = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorCode));
            nodeYP.Fill = brush;

        }
        private static string GetcompleteImagepath(string ImageName, string ImageType = "svg")
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Image\\TypeImages";
            string navigateToFolder = ImageName + "." + ImageType;
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        #region Manula Piping

        public JCHNode CreateYpNode(int ypCount, bool isCp = false)
        {
            MyNodeYP nodeYP = utilPiping.createNodeYP(isCp);
            string imageName = "YBranch";
            string sourceDir = GetcompleteImagepath(imageName);
            //var svgFilePath = sourceDir.Replace(".png", ".svg");

            CreateMyNodeYpFromSvg(sourceDir, nodeYP);
            if (isCp)
            {
                imageName = "HeaderBranch_8Seperator";
            }

            nodeYP.StrokeThickness = 0;
            nodeYP.IsSelectable = true;
            nodeYP.IsRotatable = false;
            var data = new ImageData();
            data.imagePath = sourceDir;
            data.imageName = imageName + ".png";
            data.equipmentType = "Pipe";
            var seperatorIndex = ypCount + 1;
            data.UniqName = "Sep" + seperatorIndex;
            data.NodeNo = seperatorIndex;
            nodeYP.ImageData = data;
            return nodeYP;
        }
        public JCHNode CreateYp4BranchNode(int ypCount)
        {
            MyNodeYP nodeYP = utilPiping.createNodeYP4BranchKit();
            string imageName = "HeaderBranch_4Seperator";
            string sourceDir = GetcompleteImagepath(imageName);
            nodeYP.Size = new System.Windows.Size(45, 24);
            nodeYP.StrokeThickness = 0;
            nodeYP.IsSelectable = true;
            nodeYP.IsRotatable = false;
            var data = new ImageData();
            data.imagePath = sourceDir;
            data.imageName = imageName + ".png";
            data.equipmentType = "Pipe";
            var seperatorIndex = ypCount + 1;
            data.UniqName = "Sep" + seperatorIndex;
            data.NodeNo = seperatorIndex;
            nodeYP.ImageData = data;
            return nodeYP;
        }
        public void DrawAutomaticYpLinks(MyNodeYP ypNode, AddFlow addflow, SystemVRF system, Link deleteLink = null)
        {
            var parent = ypNode.ParentNode;
            if (IsOfEquipmentType(parent, "Outdoor"))
            {
                AddLink(addflow, parent, ypNode, 0, 0, system);
                (parent as MyNodeOut).SetChildNode(ypNode);
                MyNodeYP.SetInLetId(ypNode, parent.ImageData.UniqName);
                ypNode.ParentNode = parent;
            }
            else if (parent is MyNodeYP)
            {
                if (parent.Bounds.Left < ypNode.Bounds.Left)
                {
                    var rightOutletId = MyNodeYP.GetRightOutletId(parent);
                    var pinIndex = string.IsNullOrEmpty(rightOutletId) ? 2 : 1;
                    if (deleteLink != null)
                    {
                        pinIndex = parent == deleteLink.Org ? deleteLink.PinOrgIndex : deleteLink.PinDstIndex;
                    }
                    AddLink(addflow, parent, ypNode, pinIndex, 0, system);
                    MyNodeYP.SetInLetId(ypNode, parent.ImageData.UniqName);
                    MyNodeYP.SetRightOutletId(parent, ypNode.ImageData.UniqName);
                }
                else
                {
                    var rightOutletId = MyNodeYP.GetRightOutletId(parent);
                    var pinIndex = string.IsNullOrEmpty(rightOutletId) ? 2 : 1;
                    if (deleteLink != null)
                    {
                        pinIndex = parent == deleteLink.Org ? deleteLink.PinOrgIndex : deleteLink.PinDstIndex;
                    }
                    AddLink(addflow, parent, ypNode, pinIndex, 0, system);
                    MyNodeYP.SetInLetId(ypNode, parent.ImageData.UniqName);
                    MyNodeYP.SetRightOutletId(parent, ypNode.ImageData.UniqName);
                }
            }
            foreach (var ypNodeChildNode in ypNode.ChildNodes)
            {
                var lastChildNode = ypNodeChildNode;
                if (lastChildNode != null && !ypNodeChildNode.Links.Any())
                {
                    var bottomIndex = MyNodeYP.GetBottomOutletId(ypNode);
                    var pinIndex = string.IsNullOrEmpty(bottomIndex) ? 1 : 2;
                    AddLink(addflow, ypNode, lastChildNode, pinIndex, 0, system);
                    if (pinIndex == 1)
                    {
                        MyNodeYP.SetBottomOutletId(ypNode, (lastChildNode as JCHNode).ImageData.UniqName);
                    }
                    else
                    {
                        MyNodeYP.SetRightOutletId(ypNode, (lastChildNode as JCHNode).ImageData.UniqName);
                    }
                }
                else if (lastChildNode is MyNodeYP)
                {
                    var bottomIndex = MyNodeYP.GetBottomOutletId(ypNode);
                    var pinIndex = string.IsNullOrEmpty(bottomIndex) ? 1 : 2;
                    var childInletIndex = 0;
                    if (string.IsNullOrEmpty(MyNodeYP.GetInLetId(lastChildNode)))
                    {
                        childInletIndex = 0;
                    }
                    else
                    {
                        childInletIndex = string.IsNullOrEmpty(MyNodeYP.GetBottomOutletId(lastChildNode)) ? 1 : 2;
                    }
                    AddLink(addflow, ypNode, lastChildNode, pinIndex, childInletIndex, system);
                    if (pinIndex == 1)
                    {
                        MyNodeYP.SetBottomOutletId(ypNode, (lastChildNode as JCHNode).ImageData.UniqName);
                    }
                    else
                    {
                        MyNodeYP.SetRightOutletId(ypNode, (lastChildNode as JCHNode).ImageData.UniqName);
                    }
                }
            }
        }

        void AddLink(AddFlow addflow, Node org, Node dst, int pinOrgIndex, int pinDstIndex, SystemVRF system = null)
        {
            if (org == null || dst == null) return;
            Link link = new Link(org, dst, pinOrgIndex, pinDstIndex, "", addflow);
            link.IsAdjustDst = true;
            link.IsAdjustOrg = true;
            addflow.AddLink(link);
            SetManualPipingLineStyle(link);
        }
        public void SetManualPipingLineStyle(Link link)
        {
            if (link == null) return;
            Node org = link.Org;
            Node dst = link.Dst;
            int pinOrgIndex = link.PinOrgIndex;
            if (org is MyNodeYP)
            {
                if (dst is MyNodeYP && !IsFourHeaderYp(dst))
                {
                    if ((org as MyNodeYP).ChildNodes.Count() > 2)
                        link.LineStyle = org.Location.X > dst.Location.X ? LineStyle.VHVH : LineStyle.VH;
                    else
                        link.LineStyle = LineStyle.HVH;

                    //AutoCorrectIntersectingLinkWithNode(dst, link);
                }
                else
                {
                    if ((pinOrgIndex == 1 && (org as MyNodeYP).ChildNodes.Count() == 2) ||
                        (pinOrgIndex >= 1 && (org as MyNodeYP).ChildNodes.Count() > 2))
                    {
                        link.LineStyle = LineStyle.VHV;
                        //AutoCorrectIntersectingLinkWithNode(dst, link);
                        if (link.AddFlow != null)
                        {
                            foreach (var child in link.AddFlow.Items.OfType<Node>())
                            {
                                //AutoCorrectIntersectingLinkWithNode(child, link);
                            }
                        }
                    }
                    else
                    {
                        link.LineStyle = LineStyle.HV;
                        //AutoCorrectIntersectingLinkWithNode(dst, link);
                        foreach (var child in (org as MyNodeYP).ChildNodes)
                        {
                            //AutoCorrectIntersectingLinkWithNode(child, link);
                        }
                    }
                }
            }
        }
        private bool AutoCorrectIntersectingLinkWithNode(Node node, Link link)
        {
            if (node == null || link == null) return false;
            for (int i = 0; i < link.Points.Count; i++)
            {
                if (i + 1 == link.Points.Count) break;
                var pointA = new Point(link.Points[i].X, link.Points[i].Y);
                var pointB = new Point(link.Points[i + 1].X, link.Points[i + 1].Y);
                var lineRect = new Rect(pointA, pointB);
                var interSectRect = Rect.Intersect(lineRect, node.Bounds);
                if (node.Bounds.IntersectsWith(lineRect) && (interSectRect.Size.Width > 0 || interSectRect.Size.Height > 0))
                {
                    if (link.LineStyle == LineStyle.HV)
                    {
                        var intersectionRect = new Rect(link.Points.First(), link.Points.Last());
                        if (link.Points.First().Y > link.Points.Last().Y)
                        {
                            var firstPoint = link.Points.First();
                            var lastPoint = link.Points.Last();
                            var gap = 15;
                            var newPointCollection = new List<Point>()
                            {
                                new Point(firstPoint.X + gap, firstPoint.Y),
                                new Point(intersectionRect.TopLeft.X + gap, intersectionRect.TopLeft.Y - gap),
                                new Point(lastPoint.X, lastPoint.Y - gap)
                            };
                            var newLineRect = new Rect(newPointCollection[0], newPointCollection[1]);
                            var intersect = Rect.Intersect(newLineRect, node.Bounds);
                            if (intersect.Width > 0 || intersect.Height > 0)
                            {
                                if (node.Bounds.Right > firstPoint.X)
                                {
                                    newPointCollection[0] = new Point(node.Bounds.TopRight.X + gap, firstPoint.Y);
                                }
                                else
                                {
                                    newPointCollection[0] = new Point(firstPoint.X + gap, firstPoint.Y);
                                }
                                newPointCollection[1] = new Point(newPointCollection[0].X, lastPoint.Y - gap);
                                newPointCollection[2] = new Point(lastPoint.X, lastPoint.Y - gap);
                            }
                            link.Points.RemoveAt(1);
                            link.Points.Insert(1, newPointCollection[0]);
                            link.Points.Insert(2, newPointCollection[1]);
                            link.Points.Insert(3, newPointCollection[2]);
                        }
                        else
                        {
                            var firstPoint = link.Points.First();
                            var lastPoint = link.Points.Last();
                            var gap = 15;
                            link.Points.RemoveAt(1);
                            link.Points.Insert(1, new Point(node.Bounds.TopRight.X + gap, node.Bounds.TopRight.Y));
                            link.Points.Insert(2, new Point(node.Bounds.BottomRight.X + gap, node.Bounds.BottomRight.Y + gap));
                            link.Points.Insert(3, new Point(lastPoint.X, node.Bounds.BottomRight.Y + gap));
                        }
                    }
                    if (link.LineStyle == LineStyle.VHV)
                    {
                        var intersectionRect = new Rect(link.Points.First(), link.Points.Last());
                        var firstPoint = link.Points.First();
                        var lastPoint = link.Points.Last();
                        var gap = 15;
                        if (firstPoint.X > lastPoint.X)
                        {
                            var newPointCollection = new List<Point>()
                            {
                                new Point(firstPoint.X , firstPoint.Y+gap),
                                new Point(node.Bounds.TopRight.X + gap, firstPoint.Y+gap),
                                new Point(node.Bounds.BottomRight.X + gap, node.Bounds.BottomRight.Y+gap),
                                new Point(lastPoint.X,node.Bounds.BottomRight.Y+gap)
                            };
                            link.Points.RemoveAt(1);
                            link.Points.Insert(1, newPointCollection[0]);
                            link.Points.Insert(2, newPointCollection[1]);
                            link.Points.Insert(3, newPointCollection[2]);
                            link.Points.Insert(4, newPointCollection[3]);
                        }
                        else
                        {
                            if (interSectRect.Size.Height == 0)
                            {
                                link.Points.RemoveAt(i);
                                link.Points.RemoveAt(i + 1);
                                link.Points.Insert(i, new Point(pointA.X, node.Bounds.BottomLeft.Y + gap));
                                link.Points.Insert(i + 1, new Point(pointB.X, node.Bounds.BottomLeft.Y + gap));
                            }
                            else
                            {
                                //Intercation in Vertical Line: Move X co-ordinates.

                            }
                        }
                    }

                    if (link.LineStyle == LineStyle.VHVH)
                    {
                        if (interSectRect.Width > 0)//Horizontal line intersection
                        {
                            var firstPoint = link.Points.First();
                            var lastPoint = link.Points.Last();
                            if (link.Points[i + 1] == link.Points.Last())
                            {
                                link.Points[i - 1] = new Point(node.Bounds.Left - 15, link.Points[i - 1].Y);
                                link.Points[i] = new Point(node.Bounds.Left - 15, link.Points[i].Y);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        private bool IsOfEquipmentType(MyNode node, string type)
        {
            return node != null && node.ImageData != null && node.ImageData.equipmentType != null &&
                   node.ImageData.equipmentType.Equals(type);
        }
        #endregion

        public void drawNodeImage(JCHNode node, string imgFile, ImageData data, int nodeNo, ref AddFlow addFlowPiping)
        {
            try
            {
                if (System.IO.File.Exists(imgFile))
                {
                    System.Windows.Controls.Image inOutImage = new System.Windows.Controls.Image();
                    inOutImage.Source = new BitmapImage(new Uri(imgFile));

                    var filepath = imgFile;
                    System.Drawing.Image path = System.Drawing.Image.FromFile(filepath);
                    System.Drawing.Bitmap bitmaps = new System.Drawing.Bitmap(path, 60,55);
                    BitmapSource bitSrc = null;
                    var hBitmap = bitmaps.GetHbitmap();
                    bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    node.Image = bitSrc;
                    node.Fill = System.Windows.Media.Brushes.White;
                    System.Windows.Size nodeSize = new System.Windows.Size(node.Image.Width, node.Image.Height);
                    nodeSize.Width = 125;//(float)Math.Round(nodeSize.Width / 2f) * 2;  
                    nodeSize.Height = 90; //(float)Math.Round(nodeSize.Height / 2f) * 2; 
                    node.Size = nodeSize;
                    node.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
                    node.Stroke = System.Windows.Media.Brushes.Gray;
                    if (node.Links == null || node.Links.Count == 0)
                        node.DashStyle = DashStyles.Dash;
                    node.Text = data.imageName + "\n" + data.coolingCapacity.ToString("n1") + CapacityUnits + "/" + data.heatingCapacity.ToString("n1") + CapacityUnits;
                    node.Foreground = System.Windows.Media.Brushes.Blue;
                    node.ImageMargin = new Thickness(3);
                    node.TextMargin = new Thickness(3);
                    node.ImagePosition = ImagePosition.CenterTop;
                    node.TextPosition = TextPosition.CenterBottom;
                    //node.Tooltip = "Outdoor" + " : " + data.imageName;
                    node.FontSize = 9;
                    node.IsSelectable = true;
                    ImageData imageData = new ImageData();

                    if (node.GetType().FullName != "JCHVRF.Model.NextGen.MyNodeOut")
                    {
                        node.TextMargin = new Thickness(0);
                        node.TextPosition = TextPosition.CenterBottom;
                        imageData.NodeNo = nodeNo;
                        imageData.imageName = data.imageName;
                        imageData.imagePath = imgFile;
                        imageData.equipmentType = "Indoor";
                        imageData.UniqName = string.IsNullOrEmpty(data.UniqName) ? "Ind" + Convert.ToString(nodeNo) : data.UniqName;
                        //node.Tooltip = "Indoor" + " : " + data.imageName;
                        node.ImageData = imageData;
                        node.Text = data.imageName + "\n" + data.coolingCapacity.ToString("n1") + CapacityUnits + "/" + data.sensibleHeat.ToString("n1") + CapacityUnits + "/" + data.heatingCapacity.ToString("n1") + CapacityUnits + "\n" + data.floorName + "\n" + data.roomName;
                    }



                    addFlowPiping.AddNode(node);
                }
            }
            catch
            {
                Exception ex;
            }

            //System.Windows.Controls.Image outDoorimage = new System.Windows.Controls.Image();
            //outDoorimage.Source = new BitmapImage(new Uri(imgFile));
            //addFlowPiping.Images.Add(img);

            //SizeF nodeSize = new SizeF(img.Size.Width, img.Size.Height);
            //nodeSize.Width = (float)Math.Round(nodeSize.Width / 2f) * 2;  //奇数转偶数，避免粗线出现
            //nodeSize.Height = (float)Math.Round(nodeSize.Height / 2f) * 2; //奇数转偶数，避免粗线出现
            //node.Size = nodeSize;

            //node.ImageIndex = addFlowPiping.Images.Count - 1;
            //node.ImagePosition = ImagePosition.LeftBottom;
            //node.BackMode = BackMode.Transparent;
            // node.ZOrder = 0;

        }
        public void drawCHImage(Node node, string imgFile, ref AddFlow addFlowPiping)
        {
            if (System.IO.File.Exists(imgFile))
            {
                System.Windows.Controls.Image inOutImage = new System.Windows.Controls.Image();
                inOutImage.Source = new BitmapImage(new Uri(imgFile));
                node.Stroke = System.Windows.Media.Brushes.RoyalBlue;
                node.Image = inOutImage.Source;
                node.Fill = System.Windows.Media.Brushes.White;
                // node.Tooltip = "CHBox";
                node.StrokeThickness = 0;
                node.Size = new System.Windows.Size(inOutImage.Source.Width + 2, inOutImage.Source.Height + 2);
                addFlowPiping.AddNode(node);

                //System.Windows.Controls.Image outDoorimage = new System.Windows.Controls.Image();
                //outDoorimage.Source = new BitmapImage(new Uri(imgFile));
                //addFlowPiping.Images.Add(img);

                //SizeF nodeSize = new SizeF(img.Size.Width, img.Size.Height);
                //nodeSize.Width = (float)Math.Round(nodeSize.Width / 2f) * 2;  //奇数转偶数，避免粗线出现
                //nodeSize.Height = (float)Math.Round(nodeSize.Height / 2f) * 2; //奇数转偶数，避免粗线出现
                //node.Size = nodeSize;

                //node.ImageIndex = addFlowPiping.Images.Count - 1;
                //node.ImagePosition = ImagePosition.LeftBottom;
                //node.BackMode = BackMode.Transparent;
                // node.ZOrder = 0;
            }
        }


        #region 绘制连接线
        // 绘制配管图中的节点之间的连接线
        /// <summary>
        /// 绘制配管图中的节点之间的连接线
        /// </summary>
        public void DrawPipingLinks(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            Node firstNode = nodeOut.ChildNode;

            if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
            {
                DrawLinkForBinaryTree(nodeOut, firstNode, sysItem, ref addFlowPiping);
            }
            else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.Symmetries)
            {
                DrawLinkForSymmetries(nodeOut, firstNode, 0, sysItem, false, addFlowPiping);
            }
            else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.SchemaA)
            {
                OldModel.PipingOrientation orientation = sysItem.IsPipingVertical ? OldModel.PipingOrientation.Left : OldModel.PipingOrientation.Down;
                DrawLinkForSchemaA(nodeOut, firstNode, 0, sysItem, orientation, addFlowPiping);
            }
            else
            {
                //普通布局节点连线
                DrawLinkForNormal(nodeOut, firstNode, true, sysItem, ref addFlowPiping);
            }
        }

        /// <summary>
        /// 绘制节点之间的连接线
        /// </summary>
        private void DrawLinkForBinaryTree(Node parent, Node node, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            bool isVertical = sysItem.IsPipingVertical;
            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                for (int linkIndex = 0; linkIndex < myNode.MyInLinks.Count; ++linkIndex)
                {
                    MyLink myLink = myNode.MyInLinks[linkIndex];
                    string LinkName = myLink.Text;
                    var LinkLength = myLink.Length;
                    var ElbowQty = myLink.ElbowQty;
                    var OilTrapQty = myLink.OilTrapQty;
                    myLink = new MyLink(parent, node, LinkName, addFlowPiping);
                    myLink.Length = LinkLength;
                    myLink.ElbowQty = ElbowQty;
                    myLink.OilTrapQty = OilTrapQty;
                    myLink.LinkOrder = linkOrder + 1;
                    linkOrder = myLink.LinkOrder;
                    myLink.LineStyle = LineStyle.Polyline;

                    if (myLink.AddFlow == null)
                    {
                        Point point = new Point();
                        myLink.AddPoint(point);
                        //Link link = new Link(parent, node, LinkName, addFlowPiping);
                        addFlowPiping.AddLink(myLink);
                        myNode.MyInLinks[linkIndex] = myLink;    // 必须在重置 Link 顶点位置之前执行AddLink()方法
                    }
                    if (node is MyNodeYP)
                    {
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            myLink.Points[1] = new Point(utilPiping.getTopCenterPointF(node).X + 1, utilPiping.getTopCenterPointF(node).Y);
                        }

                        else if (parent is MyNodeYP)
                        {
                            MyNodeYP parentYP = parent as MyNodeYP;
                            int nodeIndex = parentYP.GetIndex(node);

                            if (nodeIndex == 0)
                            {                      
                                if(isVertical)
                                {                      
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                    myLink.LineStyle = LineStyle.VH;
                                }
                                else
                                {                                    
                                    myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 7);
                                    myLink.Points[myLink.Points.Count - 1] = new Point(utilPiping.getTopCenterPointF(node).X, utilPiping.getTopCenterPointF(node).Y);
                                    myLink.LineStyle = LineStyle.HV;
                                }
                            }
                            if (nodeIndex == 1)
                            {                       
                                if(isVertical)
                                {            
                                    if(parentYP.YBranchOrientation == MyNodeYP.BranchOrientations.VerticalMirror.ToString())
                                    {
                                        myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 8);
                                    }
                                    else
                                    {
                                        myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y);
                                    }                             
                                    myLink.Points[1] = utilPiping.getLeftCenterPointF(node);
                                    myLink.LineStyle = LineStyle.Polyline;
                                }  
                                else
                                {
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node) ;
                                }                             
                            }
                        }
                    }

                    if (node is MyNodeIn)
                    {                       
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            if (isVertical)
                            {
                                //垂直布局
                                myLink.LineStyle = LineStyle.VHV;
                            }
                            else
                            {
                                var points = myLink.Points;
                                //水平布局
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[1].X, myLink.Points[0].Y + 30);
                                myLink.Points[2] = new Point(utilPiping.getTopCenterPointF(node).X, myLink.Points[0].Y + 30);
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                if ((node is MyNodeYP))
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 10);
                                    myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 10);
                                    double ptX = myLink.Points[myLink.Points.Count - 1].X;
                                    double ptY = myLink.Points[myLink.Points.Count - 1].Y;
                                    myLink.Points[myLink.Points.Count - 1] = new Point(ptX, ptY);
                                }                               
                            }
                        }
                        else if (parent is MyNodeCH)
                        {
                            if (isVertical)
                            {
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                myLink.LineStyle = LineStyle.Polyline;
                            }
                            else
                            {
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            }
                        }
                        else if (parent is MyNodeMultiCH)
                        {
                            MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                            float pheight = AddFlowExtension.FloatConverter(parent.Size.Height / (parentMCH.ChildNodes.Count + 1));
                            Point pt1 = new Point();

                            int nodeIndex = parentMCH.GetIndex(node);
                            if (isVertical)
                            {
                                pt1.X = parent.Location.X + parent.Size.Width;
                                pt1.Y = parent.Location.Y + pheight * (nodeIndex + 1);
                                myLink.Points[0] = pt1;
                                //myLink.AddPoint(pt1);
                                myLink.LineStyle = LineStyle.HVH;
                                if (myLink.Points[3].Y > myLink.Points[0].Y)
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X + 10 + 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                                }
                                else
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X + 10 + 3 * (nodeIndex + 1), myLink.Points[0].Y);
                                }
                                myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                            }
                            else
                            {
                                Point ptNode = utilPiping.getCenterPointF(node);
                                if (ptNode.X < parent.Location.X)
                                {
                                    pt1.X = parent.Location.X;
                                    pt1.Y = parent.Location.Y + pheight * (nodeIndex + 1);
                                    myLink.Points[0] = pt1;
                                    //myLink.AddPoint(pt1);
                                    myLink.LineStyle = LineStyle.HV;
                                }
                                else if (ptNode.X > parent.Location.X + parent.Size.Width)
                                {
                                    pt1.X = parent.Location.X + parent.Size.Width;
                                    pt1.Y = parent.Location.Y + pheight * (parentMCH.ChildNodes.Count - nodeIndex);
                                    myLink.Points[0] = pt1;
                                    //myLink.AddPoint(pt1);
                                    myLink.LineStyle = LineStyle.HV;
                                }
                                else
                                {
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    //myLink.AddPoint(utilPiping.getBottomCenterPointF(parent));
                                    myLink.LineStyle = LineStyle.VHV;
                                    myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 3);
                                }
                            }
                        }
                        else if (parent is MyNodeYP)
                        {
                            MyNodeYP parentYP = parent as MyNodeYP;
                            int nodeIndex = parentYP.GetIndex(node);

                            if (isVertical)
                            {
                                if (nodeIndex == 0)
                                {                                                        
                                    myLink.LineStyle = LineStyle.VHV;
                                    myLink.Points[0] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getBottomCenterPointF(parent).Y);
                                    myLink.Points[1] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getBottomCenterPointF(parent).Y + 160);
                                    myLink.Points[2] = new Point(utilPiping.getTopCenterPointF(node).X, utilPiping.getBottomCenterPointF(parent).Y + 160);                                    
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                                if (nodeIndex == 1)
                                {
                                    if (parentYP.YBranchOrientation == MyNodeYP.BranchOrientations.VerticalMirror.ToString())
                                    {
                                        myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 8);
                                    }
                                    else
                                    {
                                        myLink.Points[0] = utilPiping.getRightCenterPointF(parent);                                        
                                    }
                                    myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                    myLink.LineStyle = LineStyle.HV;
                                }                               
                            }
                            if (!isVertical)
                            {                               
                                if (nodeIndex == 0)
                                {
                                    myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 8);
                                    myLink.LineStyle = LineStyle.HV; 
                                }
                                else
                                {  
                                    myLink.LineStyle = LineStyle.Polyline;
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                }   
                                if ((parent is MyNodeYP) && (node is MyNodeYP))
                                {
                                    double ptX = myLink.Points[myLink.Points.Count - 1].X;
                                    double ptY = myLink.Points[myLink.Points.Count - 1].Y;
                                    myLink.Points[myLink.Points.Count - 1] = new Point(ptX, ptY);
                                }                                 
                            }                    
                        }
                    }
                    else if (node is MyNodeCH)
                    {
                        MyNodeCH nodeCH = node as MyNodeCH;
                        if (isVertical)
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                        }
                        else
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                        }

                        if (parent is MyNodeOut)
                        {
                            //第一子节点为CH-Box时，连线需要特殊处理

                            //// myLink.AddPoint(utilPiping.getLeftBottomPointF(parent));
                            //if (isVertical)
                            //{
                            //    //修复纵向布局时OldModel.Outdoor连接CHBox的连线错误 add by Shen Junjie on 20171022
                            //    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            //    myLink.LineStyle = LineStyle.VH;
                            //}
                            //else
                            //{
                            myLink.LineStyle = LineStyle.VHV;
                            myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 40);
                            myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 40);
                            //}
                        }
                        else
                        {
                            if (isVertical)
                            {
                                if ((parent is MyNodeYP))
                                {
                                    MyNodeYP parentYP = parent as MyNodeYP;
                                    if (node.Location.Y < parent.Location.Y)
                                    {
                                        myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y);
                                        myLink.LineStyle = LineStyle.Polyline;
                                    }
                                    else
                                    {
                                        if (parentYP.YBranchOrientation == MyNodeYP.BranchOrientations.VerticalMirror.ToString())
                                        {
                                            myLink.Stroke = System.Windows.Media.Brushes.Aqua;

                                            if (node == parentYP.ChildNodes[0])
                                            {
                                                myLink.LineStyle = LineStyle.VHV;
                                                myLink.Points[0] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getBottomCenterPointF(parent).Y);
                                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 190);
                                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 190);
                                            }
                                            else
                                            {
                                                myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 8);
                                                myLink.LineStyle = LineStyle.HV;
                                            }

                                        }
                                        else
                                        {
                                            if (node == parentYP.ChildNodes[0])
                                            {
                                                myLink.LineStyle = LineStyle.VHV;
                                                myLink.Points[0] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getBottomCenterPointF(parent).Y);
                                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 200);
                                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 200);
                                            }
                                            else
                                            {
                                                myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y);
                                                myLink.LineStyle = LineStyle.HV;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MyNodeYP parentYP = parent as MyNodeYP;
                                int nodeIndex = parentYP.GetIndex(node);
                                if (nodeIndex == 0)
                                {
                                    myLink.LineStyle = LineStyle.HV;
                                }
                                else
                                {
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.LineStyle = LineStyle.Polyline;
                                }
                            }
                        }
                    }
                    else if (node is MyNodeMultiCH)
                    {
                        MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                        if (isVertical)
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                        }
                        else
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                        }

                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);
                            if (isVertical)
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                myLink.LineStyle = LineStyle.VH;
                            }
                            else
                            {
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 10);
                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 10);
                            }
                        }
                        else
                        {
                            if (isVertical)
                            {
                                myLink.Points[0] = utilPiping.getCenterPointF(parent);
                                //myLink.AddPoint(utilPiping.getCenterPointF(parent));
                                myLink.LineStyle = LineStyle.VH;
                            }
                            else
                            {
                                myLink.Points[0] = utilPiping.getCenterPointF(parent);
                                // myLink.AddPoint(utilPiping.getCenterPointF(parent));
                                myLink.LineStyle = LineStyle.HV;
                            }
                        }
                    }
                }
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    DrawLinkForBinaryTree(nodeYP, nodeYP.ChildNodes[i], sysItem, ref addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH chboxNode = node as MyNodeCH;
                DrawLinkForBinaryTree(chboxNode, chboxNode.ChildNode, sysItem, ref addFlowPiping);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    DrawLinkForBinaryTree(nodeMCH, nodeMCH.ChildNodes[i], sysItem, ref addFlowPiping);
                }
            }
        }

        /// <summary>
        /// 绘制节点之间的连接线
        /// </summary>
        private void DrawLinkForSymmetries(Node parent, Node node, int nodeIndex, SystemVRF sysItem, bool forward, AddFlow addFlowPiping)
        {
            //MyLink myLink = null;
            //if (node is IInLink)
            //{
            //    myLink = (node as IInLink).InLink;
            //}

            bool isVertical = sysItem.IsPipingVertical;

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                for (int i = 0; i < myNode.MyInLinks.Count; ++i)
                {
                    MyLink myLink = myNode.MyInLinks[i];
                    string LinkName = myLink.Text;
                    myLink = new MyLink(parent, node, LinkName, addFlowPiping);
                    myLink.LineStyle = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
                    if (myLink.AddFlow == null)
                    {
                        //Link link = new Link(parent, node, LinkName, addFlowPiping);
                        addFlowPiping.AddLink(myLink);
                        myNode.MyInLinks[i] = myLink;    // 必须在重置 Link 顶点位置之前执行AddLink()方法
                    }
                    if (node is MyNodeYP)
                    {
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);

                            if (isVertical)
                            {
                                myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 10);
                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 10);
                            }
                            else
                            {
                                myLink.Points[1] = utilPiping.getLeftCenterPointF(node);
                                myLink.LineStyle = LineStyle.VH;
                            }
                        }
                        else if (parent is MyNodeYP)
                        {
                            myLink.Points[0] = utilPiping.getCenterPointF(parent);
                            myLink.Points[1] = utilPiping.getCenterPointF(node);
                            if (isVertical)
                            {
                                // 垂直布局处理
                                myLink.LineStyle = LineStyle.HV; // 该行代码必须在重置连接线顶点的坐标之后执行
                            }
                            else
                            {
                                //水平布局
                                myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                            }
                        }
                        else if (parent is MyNodeCH)
                        {
                            if (isVertical)
                            {
                                // 垂直布局处理
                                myLink.LineStyle = LineStyle.HV; // 该行代码必须在重置连接线顶点的坐标之后执行
                            }
                            else
                            {
                                //水平布局
                                myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                            }
                        }
                    }
                    else if (node is MyNodeIn)
                    {
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            myLink.LineStyle = LineStyle.VH;
                        }
                        else if (parent is MyNodeYP)
                        {
                            myLink.Points[0] = utilPiping.getCenterPointF(parent);
                            if (isVertical)
                            {
                                if (forward)
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getRightCenterPointF(node);
                                }
                            }
                            else
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            }

                            if (isVertical)
                            {
                                if (nodeIndex < (parent as MyNodeYP).ChildCount - 1)
                                {
                                    myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                            }
                            else
                            {
                                if (nodeIndex < (parent as MyNodeYP).ChildCount - 1)
                                {
                                    myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                                else
                                {
                                    myLink.LineStyle = LineStyle.HVH; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                            }
                        }
                        else if (parent is MyNodeCH)
                        {
                            if (isVertical)
                            {
                                if (forward)
                                {
                                    myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[0] = utilPiping.getLeftCenterPointF(parent);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getRightCenterPointF(node);
                                }
                            }
                            else
                            {
                                if (forward)
                                {
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                }
                                else
                                {
                                    myLink.Points[0] = utilPiping.getTopCenterPointF(parent);
                                }
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                            }
                        }
                        //if (node is MyNodeIn)
                        //{
                        //    DrawTextToLinkForBinaryTree(node, parent, isInch, sysItem);
                        //}
                    }
                    else if (node is MyNodeCH)
                    {
                        MyNodeCH nodeCH = node as MyNodeCH;
                        if (isVertical)
                        {
                            if (forward)
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            }
                            else
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getRightCenterPointF(node);
                            }
                        }
                        else
                        {
                            if (forward)
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                            }
                            else
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getBottomCenterPointF(node);
                            }
                        }

                        if (parent is MyNodeOut)
                        {
                            //第一子节点为CH-Box时，连线需要特殊处理
                            myLink.Points[0] = utilPiping.getCenterPointF(parent);
                            myLink.LineStyle = LineStyle.VHV;
                        }
                        else if (parent is MyNodeYP)
                        {
                            myLink.Points[0] = utilPiping.getCenterPointF(parent);

                            if (isVertical)
                            {
                                if (nodeIndex == (parent as MyNodeYP).ChildCount - 1)
                                {
                                    myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                            }
                            else
                            {
                                if (nodeIndex == (parent as MyNodeYP).ChildCount - 1)
                                {
                                    myLink.LineStyle = LineStyle.HV; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                            }
                        }
                    }
                }
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    bool tempForward = (i % 2 == 1) ? !forward : forward;
                    DrawLinkForSymmetries(nodeYP, nodeYP.ChildNodes[i], i, sysItem, tempForward, addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                DrawLinkForSymmetries(nodeCH, nodeCH.ChildNode, 0, sysItem, forward, addFlowPiping);
            }
        }

        /// <summary>
        /// 绘制节点之间的连接线
        /// </summary>
        private void DrawLinkForSchemaA(Node parent, Node node, int nodeIndex, SystemVRF sysItem, OldModel.PipingOrientation orientation, AddFlow addFlowPiping)
        {
            //MyLink myLink = null;
            //if (node is IInLink)
            //{
            //    myLink = (node as IInLink).InLink;
            //}
            //if (myLink == null) return;

            bool isVertical = sysItem.IsPipingVertical;
            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                for (int linkIndex = 0; linkIndex < myNode.MyInLinks.Count; ++linkIndex)
                {
                    MyLink myLink = myNode.MyInLinks[linkIndex];    // 该行代码对重置link顶点位置很重要！
                    string LinkName = myLink.Text;
                    var LinkLength = myLink.Length;
                    var ElbowQty = myLink.ElbowQty;
                    var OilTrapQty = myLink.OilTrapQty;
                    myLink = new MyLink(parent, node, LinkName, addFlowPiping);
                    myLink.Length = LinkLength;
                    myLink.ElbowQty = ElbowQty;
                    myLink.OilTrapQty = OilTrapQty;
                    myLink.LinkOrder = linkOrder + 1;
                    linkOrder = myLink.LinkOrder;
                    myLink.LineStyle = LineStyle.Polyline;

                    if (myLink.AddFlow == null)
                    {
                        addFlowPiping.AddLink(myLink);  // 必须在重置 Link 顶点位置之前执行AddLink()方法                        
                    }
                    myNode.MyInLinks[linkIndex] = myLink;
                    if (node is MyNodeYP)
                    {
                        #region node is MyNodeYP
                        if (isVertical)
                        {
                            myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                        }                           
                        else
                            myLink.Points[1] = utilPiping.getLeftCenterPointF(node);
                        if (parent is MyNodeOut)
                        {
                            if (!isVertical)
                            {
                                //myLink.Stroke = System.Windows.Media.Brushes.Pink;
                                myLink.LineStyle = LineStyle.VH;
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                myLink.Points[1] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getLeftCenterPointF(node).Y);
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                //myLink.Points[myLink.Points.Count - 1] = utilPiping.getCenterPointF(node);
                                //double ptX = myLink.Points[myLink.Points.Count - 1].X + (node.Size.Width / 2);
                                /// double ptY = myLink.Points[myLink.Points.Count - 1].Y + 4;
                                //myLink.Points[myLink.Points.Count - 1] = new Point(ptX, ptY);
                            }
                            else
                            {
                                if (isVertical)
                                {
                                    //myLink.Stroke = System.Windows.Media.Brushes.Blue;
                                    myLink.LineStyle = LineStyle.Polyline;
                                    Point endPoint = utilPiping.getTopCenterPointF(node);
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.Points[1] = new Point(endPoint.X, endPoint.Y);
                                    //myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                    //double ptX = myLink.Points[myLink.Points.Count - 1].X;
                                    //double ptY = myLink.Points[myLink.Points.Count - 1].Y + (node.Size.Width / 2); ;
                                    //myLink.Points[myLink.Points.Count - 1] = new Point(ptX, ptY);
                                }
                            }                            
                        }
                        else if (parent is MyNodeYP)
                        {
                            myLink.LineStyle = LineStyle.VH;
                            //myLink.Points[0] = utilPiping.getCenterPointF(parent);
                            //myLink.Points[1] = utilPiping.getCenterPointF(node);
                            if (!isVertical)
                            {
                                if (nodeIndex > 0)
                                {
                                    myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                                    myLink.Points[1] = utilPiping.getLeftCenterPointF(node);
                                    //myLink.Stroke = System.Windows.Media.Brushes.Green;
                                }
                                else
                                {
                                    myLink.LineStyle = LineStyle.VH;
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    //myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y - 10);
                                    //myLink.Points[2] = utilPiping.getLeftCenterPointF(node);
                                    //myLink.Stroke = System.Windows.Media.Brushes.Orange;
                                }
                            }
                            else
                            {
                                if (nodeIndex > 0)
                                {
                                    myLink.LineStyle = LineStyle.Polyline;
                                    //myLink.Stroke = System.Windows.Media.Brushes.PowderBlue;
                                    Point endPoint = utilPiping.getTopCenterPointF(node);
                                    myLink.Points[0] = new Point(utilPiping.getBottomCenterPointF(parent).X , utilPiping.getBottomCenterPointF(parent).Y);
                                    myLink.Points[1] = new Point(endPoint.X , endPoint.Y);
                                    if(myLink.Points.Count > 2)
                                    {
                                        myLink.Points.RemoveAt(2);
                                    }
                                    //myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    //myLink.Points[1] = utilPiping.getRightCenterPointF(node);//new Point(utilPiping.getRightCenterPointF(node).X - 5, utilPiping.getRightCenterPointF(node).Y);
                                }
                                else
                                {
                                    myLink.LineStyle = LineStyle.HV;
                                    //myLink.Stroke = System.Windows.Media.Brushes.Brown;
                                    myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 8);                                    
                                    //myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);                                   
                                    //myLink.Points[myLink.Points.Count - 1] = new Point(utilPiping.getTopCenterPointF(node).X , utilPiping.getTopCenterPointF(node).Y);                                    
                                }
                            }
                        }
                        else if (parent is MyNodeCH)
                        {
                            //myLink.Stroke = System.Windows.Media.Brushes.Lime;
                            if (orientation == OldModel.PipingOrientation.Left)
                            {
                                myLink.Points[0] = utilPiping.getLeftCenterPointF(parent);
                            }
                            else if (orientation == OldModel.PipingOrientation.Right)
                            {
                                myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                            }
                            else if (orientation == OldModel.PipingOrientation.Up)
                            {
                                myLink.Points[0] = utilPiping.getTopCenterPointF(parent);
                            }
                            else if (orientation == OldModel.PipingOrientation.Down)
                            {
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            }
                        }
                        else if (parent is MyNodeMultiCH)
                        {
                            MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                            Point pt1 = new Point();
                            switch (orientation)
                            {
                                case OldModel.PipingOrientation.Left:
                                    pt1.X = parent.Location.X;
                                    break;
                                case OldModel.PipingOrientation.Right:
                                case OldModel.PipingOrientation.Up:
                                case OldModel.PipingOrientation.Down:
                                    pt1.X = parent.Location.X + parent.Size.Width;
                                    break;
                            }

                            if (isVertical)
                            {
                                pt1.Y = parent.Location.Y + parent.Size.Height / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                            }
                            else
                            {
                                if (orientation == OldModel.PipingOrientation.Up)
                                {
                                    pt1.Y = parent.Location.Y + parent.Size.Height / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                                }
                                else
                                {
                                    pt1.Y = parent.Location.Y + parent.Size.Height / (parentMCH.ChildNodes.Count + 1) * (parentMCH.ChildNodes.Count - nodeIndex);
                                }
                            }
                            myLink.Points[0] = pt1;
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getCenterPointF(node);
                            myLink.LineStyle = LineStyle.HVH;      // 该行代码必须在重置连接线顶点的坐标之后执行
                            if (isVertical)
                            {
                                if (orientation == OldModel.PipingOrientation.Left)
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X - 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                                }
                                else
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X + 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                                }
                                myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                            }
                            else
                            {
                                myLink.Points[1] = new Point(myLink.Points[3].X - 30, myLink.Points[0].Y);
                                myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                            }
                        }
                        #endregion
                    }
                    else if (node is MyNodeIn)
                    {
                        #region node is MyNodeIn
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                            myLink.LineStyle = LineStyle.VHV;
                        }
                        else if (parent is MyNodeYP)
                        {                            
                            //myLink.Points[0] = utilPiping.getCenterPointF(parent);
                            //myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            //node.Text = nodeIndex.ToString();
                            if (isVertical)
                            {
                                myLink.LineStyle = LineStyle.HV;
                                //myLink.Stroke = System.Windows.Media.Brushes.DeepPink;
                                Point endPoint = utilPiping.getRightCenterPointF(parent);
                                if (orientation == OldModel.PipingOrientation.Right)
                                {
                                    myLink.Points[0] = new Point(endPoint.X, endPoint.Y + 8);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);                                   
                                }
                                else
                                {
                                    myLink.Points[0] = new Point(endPoint.X, endPoint.Y + 8);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);                                    
                                }
                            }
                            else
                            {
                                myLink.LineStyle = LineStyle.VH;
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            }

                            //第二根link For FSN3PE add by Shen Junjie on 2018/01/22
                            if (nodeIndex == 0 && linkIndex == 1)
                            {
                                if (isVertical)
                                {
                                    myLink.Points[0] = new Point(myLink.Points[0].X + 3, myLink.Points[0].Y - 3);
                                    myLink.Points[1] = new Point(myLink.Points[1].X, myLink.Points[1].Y - 3);
                                }
                                else
                                {
                                    myLink.Points[0] = new Point(myLink.Points[0].X + 3, myLink.Points[0].Y);
                                    if (orientation == OldModel.PipingOrientation.Down)
                                    {
                                        myLink.Points[1] = new Point(myLink.Points[1].X, myLink.Points[1].Y - 3);
                                    }
                                    else if (orientation == OldModel.PipingOrientation.Up)
                                    {
                                        myLink.Points[1] = new Point(myLink.Points[1].X, myLink.Points[1].Y + 3);
                                    }
                                }
                            }

                            if (isVertical)
                            {                                
                                if (nodeIndex > 0)
                                {
                                    //ACC-RAG
                                    //myLink.Stroke = System.Windows.Media.Brushes.Red;
                                    myLink.LineStyle = LineStyle.VHV; // 该行代码必须在重置连接线顶点的坐标之后执行
                                    myLink.Points[0] = new Point(utilPiping.getBottomCenterPointF(parent).X , utilPiping.getBottomCenterPointF(parent).Y);
                                    myLink.Points[1] = new Point(utilPiping.getBottomCenterPointF(parent).X , utilPiping.getBottomCenterPointF(parent).Y + 123);
                                    myLink.Points[2] = new Point(utilPiping.getTopCenterPointF(node).X, utilPiping.getBottomCenterPointF(parent).Y + 123);
                                    if (orientation == OldModel.PipingOrientation.Right)
                                        myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                    else
                                        myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                            }
                            else
                            {
                                //ACC-RAG link to indoors from Ybranch
                                if (nodeIndex == 0)
                                {
                                    //myLink.Stroke = System.Windows.Media.Brushes.Red;
                                    myLink.LineStyle = LineStyle.Polyline;
                                    if ((parent is MyNodeYP) && (orientation == OldModel.PipingOrientation.Up))
                                    {
                                        myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                        myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                        if (myLink.Points.Count == 3)
                                        {
                                            myLink.Points.RemoveAt(2);
                                        }
                                    }
                                    if ((parent is MyNodeYP) && (orientation == OldModel.PipingOrientation.Down))
                                    {
                                        myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                        myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                        if (myLink.Points.Count == 3)
                                        {
                                            myLink.Points.RemoveAt(2);
                                        }
                                    }
                                }
                                else
                                {
                                    myLink.LineStyle = LineStyle.HV;
                                    //myLink.Stroke = System.Windows.Media.Brushes.Olive;
                                    Point endPoint = utilPiping.getTopCenterPointF(node);
                                    myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                                    myLink.Points[1] = new Point(endPoint.X, myLink.Points[0].Y);
                                }
                            }
                        }
                        else if (parent is MyNodeCH)
                        {
                            if (isVertical)
                            {
                                //myLink.Stroke = System.Windows.Media.Brushes.Gold;
                                myLink.LineStyle = LineStyle.Polyline;
                                if (orientation == OldModel.PipingOrientation.Right)
                                {
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                                else if (orientation == OldModel.PipingOrientation.Left)
                                {
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                            }
                            else
                            {
                                //myLink.Stroke = System.Windows.Media.Brushes.Gold;                                
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                myLink.LineStyle = LineStyle.Polyline;
                            }
                        }
                        else if (parent is MyNodeMultiCH)
                        {
                            MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                            Point pt1 = new Point();
                            switch (orientation)
                            {
                                case OldModel.PipingOrientation.Left:
                                    pt1.X = parent.Location.X;
                                    break;
                                case OldModel.PipingOrientation.Right:
                                case OldModel.PipingOrientation.Up:
                                case OldModel.PipingOrientation.Down:
                                    pt1.X = parent.Location.X + parent.Size.Width;
                                    break;
                            }

                            if (isVertical)
                            {
                                pt1.Y = parent.Location.Y + parent.Size.Height / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                                if (orientation == OldModel.PipingOrientation.Right)
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getRightCenterPointF(node);
                                }
                            }
                            else
                            {
                                if (orientation == OldModel.PipingOrientation.Up)
                                {
                                    pt1.Y = parent.Location.Y + parent.Size.Height / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                                }
                                else
                                {
                                    pt1.Y = parent.Location.Y + parent.Size.Height / (parentMCH.ChildNodes.Count + 1) * (parentMCH.ChildNodes.Count - nodeIndex);
                                }
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            }
                            myLink.Points[0] = pt1;
                            myLink.LineStyle = LineStyle.HVH;      // 该行代码必须在重置连接线顶点的坐标之后执行
                            if (isVertical)
                            {
                                if (orientation == OldModel.PipingOrientation.Left)
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X - 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                                }
                                else
                                {
                                    myLink.Points[1] = new Point(myLink.Points[0].X + 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                                }
                                myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                            }
                            else
                            {
                                myLink.Points[1] = new Point(myLink.Points[3].X - 30, myLink.Points[0].Y);
                                myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                            }
                        }
                        #endregion
                    }
                    else if (node is MyNodeCH)
                    {
                        #region node is MyNodeCH
                        MyNodeCH nodeCH = node as MyNodeCH;
                        
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);
                            if (isVertical)
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 30);
                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 30);

                                //myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                //myLink.LineStyle = LineStyle.VH;
                            }
                            else
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 30);
                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 30);
                            }
                        }
                        else if (parent is MyNodeYP)
                        {
                            if (isVertical)
                            {                                
                                if (nodeIndex > 0)
                                {
                                    myLink.LineStyle = LineStyle.VHV; // 该行代码必须在重置连接线顶点的坐标之后执行
                                    myLink.Points[0] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getBottomCenterPointF(parent).Y);
                                    myLink.Points[1] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getBottomCenterPointF(parent).Y + 155);
                                    myLink.Points[2] = new Point(utilPiping.getTopCenterPointF(node).X, utilPiping.getBottomCenterPointF(parent).Y + 155);
                                    if (orientation == OldModel.PipingOrientation.Right)
                                        myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                    else
                                        myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);// 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                                else if (nodeIndex == 0)
                                {
                                    myLink.Points[0] = new Point(utilPiping.getRightCenterPointF(parent).X, utilPiping.getRightCenterPointF(parent).Y + 8);
                                    myLink.LineStyle = LineStyle.HV;
                                    //myLink.Stroke = System.Windows.Media.Brushes.OrangeRed;
                                }
                                if (orientation == OldModel.PipingOrientation.Right)
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                            }
                            else
                            {                                
                                if (nodeIndex > 0)
                                {
                                    //myLink.Stroke = System.Windows.Media.Brushes.Red;
                                    myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                                    myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                    myLink.LineStyle = LineStyle.HV; // 该行代码必须在重置连接线顶点的坐标之后执行  
                                }
                                if (nodeIndex == 0)
                                {
                                    //myLink.Stroke = System.Windows.Media.Brushes.Purple;
                                    myLink.LineStyle = LineStyle.Polyline;
                                    myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                    myLink.Points[1] = utilPiping.getTopCenterPointF(node);
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (node is MyNodeMultiCH)
                    {
                        #region node is MyNodeMultiCH
                        MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                        if (parent is MyNodeOut)
                        {
                            myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);
                            if (isVertical)
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                myLink.LineStyle = LineStyle.VH;
                            }
                            else
                            {
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 10);
                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[0].Y + 10);
                            }
                        }
                        else if (parent is MyNodeYP)
                        {
                            myLink.Points[0] = utilPiping.getCenterPointF(parent);

                            if (isVertical)
                            {
                                if (orientation == OldModel.PipingOrientation.Right)
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getRightCenterPointF(node);
                                }
                                if (nodeIndex == (parent as MyNodeYP).ChildCount - 1)
                                {
                                    myLink.LineStyle = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                            }
                            else
                            {
                                if (orientation == OldModel.PipingOrientation.Up)
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getBottomCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                                if (nodeIndex == (parent as MyNodeYP).ChildCount - 1)
                                {
                                    myLink.LineStyle = LineStyle.HV; // 该行代码必须在重置连接线顶点的坐标之后执行
                                }
                            }
                        }
                        #endregion
                    }
                }
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                bool isFirstYP = !nodeYP.IsCP && parent is MyNodeOut;
                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    OldModel.PipingOrientation orientation1 = orientation;
                    if (isFirstYP && i == 0)
                    {
                        orientation1 = isVertical ? OldModel.PipingOrientation.Right : OldModel.PipingOrientation.Up;
                    }
                    DrawLinkForSchemaA(nodeYP, nodeYP.ChildNodes[i], i, sysItem, orientation1, addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                OldModel.PipingOrientation orientation1 = orientation;
                if (parent is MyNodeOut)
                {
                    orientation1 = isVertical ? OldModel.PipingOrientation.Right : OldModel.PipingOrientation.Down;
                }
                DrawLinkForSchemaA(nodeCH, nodeCH.ChildNode, 0, sysItem, orientation1, addFlowPiping);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                OldModel.PipingOrientation orientation1 = orientation;
                if (parent is MyNodeOut)
                {
                    orientation1 = isVertical ? OldModel.PipingOrientation.Right : OldModel.PipingOrientation.Down;
                }
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    DrawLinkForSchemaA(nodeMCH, nodeMCH.ChildNodes[i], i, sysItem, orientation1, addFlowPiping);
                }
            }
        }

        // 递归程序，绘制YP节点组的连接线
        /// <summary>
        /// 递归程序，绘制YP节点组的连接线
        /// </summary>
        private void DrawLinkForNormal(Node parent, Node node, bool isFirstChild, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            //MyLink myLink = null;
            //if (node is IInLink)
            //{
            //    myLink = (node as IInLink).InLink;
            //}
            //if (myLink == null) return;

            bool isVertical = sysItem.IsPipingVertical;

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                for (int linkIndex = 0; linkIndex < myNode.MyInLinks.Count; ++linkIndex)
                {
                    MyLink myLink = myNode.MyInLinks[linkIndex];
                    string LinkName = myLink.Text;
                    var LinkLength = myLink.Length;
                    var ElbowQty = myLink.ElbowQty;
                    var OilTrapQty = myLink.OilTrapQty;
                    myLink = new MyLink(parent, node, LinkName, addFlowPiping);
                    myLink.Length = LinkLength;
                    myLink.ElbowQty = ElbowQty;
                    myLink.OilTrapQty = OilTrapQty;
                    myLink.LinkOrder = linkOrder + 1;
                    linkOrder = myLink.LinkOrder;
                    myLink.LineStyle = LineStyle.Polyline;
                    if (myLink.AddFlow == null)
                    {
                        //myLink.AddPoint(utilPiping.getLeftBottomPointF(parent));
                        //myLink.AddPoint(utilPiping.getTopCenterPointF(node));                      
                        addFlowPiping.AddLink(myLink);
                    }
                    myNode.MyInLinks[linkIndex] = myLink;
                    if (parent is MyNodeOut)
                    {
                        myLink.Points[0] = utilPiping.getLeftCenterPointF(parent);
                        // myLink.AddPoint(utilPiping.getLeftBottomPointF(parent));

                    }
                    else if (parent is MyNodeYP)
                    {
                        MyNodeYP yp = parent as MyNodeYP;
                        Point ConnectingPoint = new Point();
                        if (isVertical)
                        {
                            if (node is MyNodeYP)
                                ConnectingPoint = utilPiping.getBottomCenterPointF(parent);
                            else
                                if(yp.ParentNode is MyNodeOut)
                                ConnectingPoint = utilPiping.getRightCenterPointF(parent);
                            else
                                ConnectingPoint = utilPiping.getRightCenterPointForVerticalMirrorYP(parent);
                        }
                        else
                        {
                            if (node is MyNodeYP)
                                ConnectingPoint = utilPiping.getRightCenterPointF(parent);
                            else
                                ConnectingPoint = utilPiping.getBottomCenterPointF(parent);
                        }

                        isFirstChild = yp.GetIndex(node) == 0;
                        if (isVertical && isFirstChild && yp.IsCP)
                        {
                            ConnectingPoint.Y = parent.Location.Y + 10;
                        }
                        //myLink.AddPoint(ptf);
                        myLink.Points[0] = ConnectingPoint;
                    }
                    else if (parent is MyNodeCH)
                    {
                        if (isVertical)
                        {
                            myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                            //myLink.AddPoint(utilPiping.getRightCenterPointF(parent));
                        }
                        else
                        {
                            myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                            //myLink.AddPoint(utilPiping.getBottomCenterPointF(parent));
                        }
                    }
                    else if (parent is MyNodeMultiCH)
                    {
                        MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                        Point pt1 = utilPiping.getRightTopPointF(parent);
                        int nodeIndex = parentMCH.GetIndex(node);
                        pt1.Y += parentMCH.Size.Height / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                        myLink.Points[0] = pt1;
                        //myLink.AddPoint(pt1);
                    }

                    if (node is MyNodeYP)
                    {
                        MyNodeYP nodeYP = node as MyNodeYP;
                        if (nodeYP.IsCP)
                        {
                            if (!isVertical)
                            {
                                if (myLink.Points[0].X == myLink.Points[myLink.Points.Count - 1].X)
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                }
                                else
                                {
                                    myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                }
                            }
                        }
                        else
                        {
                            //if (isVertical)
                            //    myLink.LineStyle = LineStyle.VH;
                            //else
                            //    myLink.LineStyle = LineStyle.VHV;
                            if (isVertical)
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                            else
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            //myLink.Points[myLink.Points.Count - 1] = getNodePins(node);
                        }

                        if (parent is MyNodeOut)
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            myLink.LineStyle = LineStyle.VH;
                            //Fixes for Gap issue between pipe and Y branch.
                            //double ptX = myLink.Points[myLink.Points.Count - 1].X + (node.Size.Width / 2);
                            //double ptX = myLink.Points[myLink.Points.Count - 1].X;
                            //double ptY = myLink.Points[myLink.Points.Count - 1].Y;
                            //myLink.Points[myLink.Points.Count - 1] = new Point(ptX, ptY);

                        }
                        else if (parent is MyNodeMultiCH)
                        {
                            myLink.LineStyle = LineStyle.HVH;

                            MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                            int nodeIndex = parentMCH.GetIndex(node);
                            myLink.Points[1] = new Point(myLink.Points[0].X + 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                            myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                        }
                        //myLink.AddFlow.SendToBack();
                        //myLink.AddFlow.SendBackward();
                    }
                    else if (node is MyNodeCH)
                    {
                        var points = myLink.Points;
                        MyNodeCH nodeCH = node as MyNodeCH;
                        if (isVertical)
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            //myLink.AddPoint(utilPiping.getLeftCenterPointF(node));
                            myLink.LineStyle = LineStyle.VH;
                            if (!isFirstChild)
                            {
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                myLink.Points[1] = new Point(utilPiping.getBottomCenterPointF(parent).X, utilPiping.getCenterPointF(node).Y );
                            }
                        }
                        else
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                            //myLink.AddPoint(utilPiping.getTopCenterPointF(node));
                            if (parent is MyNodeOut)
                            {
                                myLink.LineStyle = LineStyle.VHV;
                            }
                            else
                            {
                                myLink.LineStyle = LineStyle.HV;
                                if (isFirstChild)
                                    myLink.Points[0] = points[0];
                                else
                                {
                                    myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                                    myLink.Points[1] = new Point(utilPiping.getCenterPointF(node).X, utilPiping.getRightCenterPointF(parent).Y);
                                }
                            }
                        }
                    }
                    else if (node is MyNodeMultiCH)
                    {
                        MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                        if (isVertical)
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                            myLink.LineStyle = LineStyle.VH;
                        }
                        else
                        {
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                            if (parent is MyNodeOut)
                            {
                                myLink.LineStyle = LineStyle.VHV;
                                myLink.Points[1] = new Point(myLink.Points[0].X, myLink.Points[0].Y + 10);
                                myLink.Points[2] = new Point(myLink.Points[3].X, myLink.Points[1].Y);
                            }
                            else
                            {
                                myLink.LineStyle = LineStyle.HV;
                            }
                        }
                    }
                    else if (node is MyNodeIn)
                    {
                        //if (isVertical)
                        //    myLink.LineStyle = LineStyle.VH;
                        //else
                        //    myLink.LineStyle = LineStyle.VHV;
                        if (isVertical == true)
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                        else
                            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);

                        if (parent is MyNodeOut)
                        {
                            if (isVertical)
                                myLink.LineStyle = LineStyle.VH;
                            else
                                myLink.LineStyle = LineStyle.VHV;
                        }
                        else if (parent is MyNodeYP)
                        {
                            //if (isVertical)
                            //    myLink.LineStyle = LineStyle.VH;
                            //else
                            //    myLink.LineStyle = LineStyle.VHV;
                            //if (!isFirstChild)
                            //    myLink.LineStyle = LineStyle.VH;
                            //else
                            //    myLink.LineStyle = LineStyle.VHV;
                            if (!isVertical && !isFirstChild)
                            {
                                myLink.LineStyle = LineStyle.HV;
                                int index = 0;
                                Point ptParentCenter = utilPiping.getRightCenterPointF(parent);
                                MyNodeYP parentYP = parent as MyNodeYP;
                                index = parentYP.GetIndex(node);
                                float x = AddFlowExtension.FloatConverter(myLink.Points[0].X + UtilPiping.HDistanceHorizontal * index);
                                myLink.Points[0] = ptParentCenter;

                                myLink.Points[1] = new Point(x, myLink.Points[0].Y);
                                /*
                                double ptX = myLink.Points[0].X - (parent.Size.Width / 2);
                                double ptY = myLink.Points[0].Y;
                                myLink.Points[0] = new Point(ptX, ptY);
                                */

                                //myLink.Points[2] = new Point(x, myLink.Points[2].Y);
                            }
                            if (isVertical && !isFirstChild)
                            {
                                Point ptParentCenter = utilPiping.getBottomCenterPointF(parent);
                                myLink.LineStyle = LineStyle.VH;
                                Point pt = new Point();
                                pt.X = ptParentCenter.X;
                                pt.Y = ptParentCenter.Y + UtilPiping.HDistanceHorizontal - 9;
                                myLink.Points[0] = ptParentCenter;
                                myLink.Points[1] = pt;
                            }

                            if (isFirstChild && linkIndex == 1)
                            {
                                if (isVertical)
                                {
                                    myLink.Points[0] = new Point(myLink.Points[0].X + 3, myLink.Points[0].Y - 3);
                                    myLink.Points[1] = new Point(myLink.Points[1].X + 3, myLink.Points[1].Y - 3);
                                    myLink.Points[2] = new Point(myLink.Points[2].X, myLink.Points[2].Y - 3);
                                }
                                else
                                {
                                    myLink.Points[0] = new Point(myLink.Points[0].X + 3, myLink.Points[0].Y);
                                    myLink.Points[1] = new Point(myLink.Points[1].X + 3, myLink.Points[1].Y - 3);
                                    if (myLink.Points.Count > 2)
                                        myLink.Points[2] = new Point(myLink.Points[2].X, myLink.Points[2].Y - 3);
                                }
                            }
                        }
                        else if (parent is MyNodeMultiCH)
                        {
                            myLink.LineStyle = LineStyle.HVH;

                            MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                            int nodeIndex = parentMCH.GetIndex(node);
                            myLink.Points[1] = new Point(myLink.Points[0].X + 3 * (parentMCH.ChildNodes.Count - nodeIndex), myLink.Points[0].Y);
                            myLink.Points[2] = new Point(myLink.Points[1].X, myLink.Points[3].Y);
                        }
                    }
                }
            }

            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    DrawLinkForNormal(nodeYP, nodeYP.ChildNodes[i], (i == 0), sysItem, ref addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                DrawLinkForNormal(nodeCH, nodeCH.ChildNode, isFirstChild, sysItem, ref addFlowPiping);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    DrawLinkForNormal(nodeMCH, nodeMCH.ChildNodes[i], (i == 0), sysItem, ref addFlowPiping);
                }
            }
        }

        public void DrawManualLinkForNormal(Node parent, Node node, bool isFirstChild, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            bool isVertical = sysItem.IsPipingVertical;
            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                for (int linkIndex = 0; linkIndex < myNode.MyInLinks.Count; ++linkIndex)
                {
                    MyLink myLink = myNode.MyInLinks[linkIndex];
                    string LinkName = myLink.Text;
                    var LinkLength = myLink.Length;
                    //var orgIndex = myNode.MyInLinks[linkIndex].PinOriginIndex;
                    //var dstIndex = myNode.MyInLinks[linkIndex].PinDestinationIndex;

                    myLink = new MyLink(parent, node, LinkName, addFlowPiping);
                    myLink.Length = LinkLength;

                    myLink.LineStyle = myNode.MyInLinks[linkIndex].LineStyle;
                    //myLink.PinOrgIndex = orgIndex;
                    //myLink.PinDstIndex = dstIndex;
                    //myLink.IsAdjustDst = true;
                    //myLink.IsAdjustOrg = true;
                    if (myLink.AddFlow == null)
                    {
                        if (myNode.MyInLinks[linkIndex].Points != null && myNode.MyInLinks[linkIndex].Points.Count > 0)
                            addFlowPiping.AddLink(myLink);
                    }
                    myLink.Points.Clear();
                    for (int i = 0; i < myNode.MyInLinks[linkIndex].Points.Count; i++)
                    {
                        myLink.Points.Insert(i, myNode.MyInLinks[linkIndex].Points[i]);
                    }
                    myNode.MyInLinks[linkIndex] = myLink;
                    //SetManualPipingLineStyle(myLink);
                    //node.AddFlow.BringIntoView();
                }
            }
            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; ++i)
                {
                    DrawManualLinkForNormal(nodeYP, nodeYP.ChildNodes[i], (i == 0), sysItem, ref addFlowPiping);
                }

            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                DrawManualLinkForNormal(nodeCH, nodeCH.ChildNode, isFirstChild, sysItem, ref addFlowPiping);

            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    DrawManualLinkForNormal(nodeMCH, nodeMCH.ChildNodes[i], (i == 0), sysItem, ref addFlowPiping);
                }

            }
        }

        public void AutoConnectNodesWithYp(Node nodeA, Node nodeB, AddFlow addFlow)
        {
            try
            {
                var isautoYpRequired = IsAutoYoNeeded(nodeA, nodeB, addFlow);

                if (!isautoYpRequired)
                {
                    return;
                }

                var nodePair = GetParentChildRelation(nodeA, nodeB, addFlow);

                var autoYpNode = CreateYpNode(addFlow.Items.OfType<MyNodeYP>().Count());

                var ypParent = nodePair.Item1;
                var ypFirstChild = nodePair.Item2;
                var ypLastChild = nodePair.Item3;

                var ypNode = autoYpNode as MyNodeYP;

                ypNode.ParentNode = ypParent as MyNode;

                ypNode.AddChildNode(ypFirstChild);
                ypNode.AddChildNode(ypLastChild);


                var ypLocation = new Point(utilPiping.getTopCenterPointF(ypFirstChild).X - 12.5,
                    utilPiping.getTopCenterPointF(ypFirstChild).Y - 50);

                addFlow.AddNode(ypNode);

                SetAutoYpLocation(ypNode, ypLocation);

                var isNoLinkUseCase = (!nodeA.Links.Any() && !nodeB.Links.Any());

                if (!isNoLinkUseCase)
                {
                    var index = GetConnectedIndex(ypParent, ypFirstChild);

                    if (ypFirstChild != null)
                        addFlow.RemoveLink(ypFirstChild.Links.FirstOrDefault());
                    if (ypLastChild != null)
                        addFlow.RemoveLink(ypLastChild.Links.FirstOrDefault());

                    AddLink(addFlow, ypParent, ypNode, index, 1); //Parent To Auto Yp 0
                    AddLink(addFlow, ypNode, ypFirstChild, 1, 0); //Auto Yp To First Child 0
                    AddLink(addFlow, ypNode, ypLastChild, 2, 0); //Auto Yp To Last Child 0}
                }
                else
                {
                    AddLink(addFlow, ypParent, ypNode, ypParent is MyNodeYP ? 2 : 0, 0); //Parent To Auto Yp 0
                    AddLink(addFlow, ypNode, ypFirstChild, 1, 0); //Auto Yp To First Child 0
                }
            }
            catch (Exception e)
            {


            }
        }

        private void SetAutoYpLocation(Node ypNode, Point location)
        {
            if (ypNode == null) return;
            ypNode.Location = location;
        }

        private int GetConnectedIndex(Node ypParent, Node firstChild)
        {
            var index = 2;
            if (!ypParent.Links.Any()) return index;
            var oldLink = ypParent.Links.First();
            if (oldLink.Org == ypParent)
            {
                index = oldLink.PinOrgIndex;
            }

            if (oldLink.Org == firstChild)
            {
                index = oldLink.PinDstIndex;
            }

            return index;
        }


        private bool IsAutoYoNeeded(Node nodeA, Node nodeB, AddFlow addflow)
        {
            if (nodeA == null || nodeB == null) return false;

            if (!nodeA.Links.Any() && !nodeB.Links.Any())
            {
                return (addflow.Items.OfType<JCHNode>().Any(x => x != nodeA
                                                                && x != nodeB
                                                                && x is JCHNode
                                                                && (x as JCHNode).ImageData != null
                                                                && (x as JCHNode).ImageData.equipmentType != null
                                                                && ((x as JCHNode).ImageData.equipmentType == "Indoor")));

            }

            return true;
        }

        private Tuple<Node, Node, Node> GetParentChildRelation(Node nodeA, Node nodeB, AddFlow addflow)
        {
            Node firstChild = null;
            Node lastChild = null;
            Node ypParentNode = null;

            var isNoLinkUseCase = (!nodeA.Links.Any() && !nodeB.Links.Any());

            if (nodeA.Links.Any())
            {
                firstChild = nodeA;
                lastChild = nodeB;
            }
            else if (nodeB.Links.Any())
            {
                firstChild = nodeB;
                lastChild = nodeA;
            }

            if (firstChild == null) firstChild = nodeA;
            if (lastChild == null) lastChild = nodeB;

            if (!isNoLinkUseCase)
            {
                if (firstChild == nodeA)
                {
                    if (firstChild is MyNodeOut)
                    {
                        firstChild = nodeB;
                        lastChild = nodeA;
                    }

                    if (firstChild is MyNodeYP && IsFourHeaderYp(firstChild))
                    {
                        firstChild = nodeB;
                        lastChild = nodeA;
                    }
                }

                if (firstChild == nodeB)
                {
                    if (firstChild is MyNodeOut)
                    {
                        firstChild = nodeA;
                        lastChild = nodeB;
                    }

                    if (firstChild is MyNodeYP && IsFourHeaderYp(firstChild))
                    {
                        firstChild = nodeB;
                        lastChild = nodeA;

                    }
                }
                ypParentNode = firstChild.Links.FirstOrDefault().Org == firstChild ? firstChild.Links.FirstOrDefault().Dst :
                    firstChild.Links.FirstOrDefault().Org;
            }


            else
            {
                ypParentNode = firstChild;
                firstChild = lastChild;
                lastChild = null;
            }


            return new Tuple<Node, Node, Node>(ypParentNode, firstChild, lastChild);
        }


        private bool IsFourHeaderYp(Node node)
        {
            return node != null && node is MyNodeYP && node.Tooltip == JCHVRF.Model.NodeType.YP4.ToString();
        }

        #endregion

        private PointCollection getNodePins(Node node)
        {
            PointCollection points = new PointCollection();
            points = node.PinsLayout;

            return points;
        }
        public void drawTextToIDUNode(SystemVRF sysItem, MyNodeIn nodeUnit, ref AddFlow addFlowPiping)
        {
            var node = nodeUnit as Node;
            if (node != null)
            {
                //node.Clear();   //  To be Fix latter
            }

            string text = "";
            OldModel.RoomIndoor riItem = nodeUnit.RoomIndooItem;
            if (riItem == null) return;
            //string model = riItem.IndoorItem.Model;
            string model = riItem.IndoorItem.Model_York; //根据PM要求，piping 中的IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            if (isHitachi)
                model = riItem.IndoorItem.Model_Hitachi;
            if (sysItem.OutdoorItem.Type.Contains("YVAHP") || sysItem.OutdoorItem.Type.Contains("YVAHR"))
                model = riItem.IndoorItem.Model_York;

            text = riItem.IndoorName + "\n" + model;
            if (!string.IsNullOrEmpty(riItem.RoomID))
            {
                string floorName = "";
                foreach (OldModel.Floor f in thisProject.FloorList)
                {
                    foreach (OldModel.Room rm in f.RoomList)
                    {
                        if (rm.Id == riItem.RoomID)
                        {
                            floorName = f.Name;
                            break;
                        }
                    }
                }
                //text = riItem.RoomName + ":" + text;
                text = floorName + "\n" + riItem.RoomName + ":" + text;
            }
            else
            {
                //添加非基于房间的室内机 房间名称 on 20170919 by xyj
                if (!string.IsNullOrEmpty(riItem.DisplayRoom))
                {
                    text = riItem.DisplayRoom + ":" + text;
                }
            }

            //nodeUnit.Size = new System.Windows.Size(nodeUnit.Size.Width+100, nodeUnit.Size.Height + 100);
            //Caption label = new Caption(0, 2, 110, 50, text, nodeUnit, this.addFlowPiping);
            initTextMyNode(nodeUnit, text);
            //label.Alignment = Alignment.CenterBOTTOM;    // // To be Fix latter
            //if (label.Size.Width < nodeUnit.Size.Width)
            //    label.Size = new System.Windows.Size(nodeUnit.Size.Width, label.Size.Height);

            // label.Dock = DockStyle.Top;
            //addFlowPiping.Items.Add(nodeUnit);
            //label.Parent = nodeUnit;       // // To be Fix latter
            //nodeUnit.HighlightChildren = true;         // // To be Fix latter

            //实际容量
            //Caption label2 = new Caption(0, 2, 100, 100, text, nodeUnit, this.addFlowPiping);
            string text2 = "";
            text2 = "Cooling: " + Unit.ConvertToControl(riItem.ActualCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            text2 += "\nHeating: " + Unit.ConvertToControl(riItem.ActualHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            text2 += "\nSensible Cooling: " + Unit.ConvertToControl(riItem.ActualSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            initTextMyNode(node, text2);

            // label2.Alignment = Alignment.LeftJustifyMIDDLE;    // // To be Fix latter
            //if (label2.Size.Width < nodeUnit.Size.Width)
            //{
            //    label2.Size = new System.Windows.Size(nodeUnit.Size.Width, label2.Size.Height);
            //}

            addFlowPiping.Items.Add(node);
            //addFlowPiping.Items.Add(label2);
            // label2.Parent = nodeUnit;                    // // To be Fix latter

            // label.Location = new Point(nodeUnit.Location.X, nodeUnit.Location.Y - label.Size.Height);
            //label2.Location = new Point(nodeUnit.Location.X, nodeUnit.Location.Y + nodeUnit.Size.Height);
            //Math.Max(nodeUnit.Location.X - 10, nodeUnit.Location.X + (nodeUnit.Size.Width - label2.Size.Width) / 2),

            if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.Normal)
            {
                if (!sysItem.IsPipingVertical)
                {
                    //label2.Location = new Point(label2.Location.X, label2.Location.Y + 8);
                }
            }
            else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
            {
                if (!sysItem.IsPipingVertical)
                {
                    // label.Location = new Point(nodeUnit.Location.X, nodeUnit.Location.Y + nodeUnit.Size.Height);
                    //label2.Location = new Point(label2.Location.X, label2.Location.Y + label.Size.Height);
                }
            }
        }

        /// 为机组节点绘制文字
        /// <summary>
        /// 为机组节点绘制文字
        /// </summary>
        /// <param name="nodeUnit"></param>
        /// <param name="font"></param>
        public void drawTextToODUNode(MyNodeOut nodeUnit, NodeElement_Piping outNodeItem, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            //if (nodeUnit.Children != null)
            //{
            //    while (nodeUnit.Children.Count > 0)   // // To be Fix latter
            //    {
            //        nodeUnit.Children[0].Remove();
            //    }
            //}

            Point ptText = new Point();
            string topModelName = nodeUnit.Model;
            //if (thisProject.BrandCode == "Y")
            //{
            //    topModelName = sysItem.OutdoorItem.Model_York;
            //}
            //else
            //{
            //    topModelName = sysItem.OutdoorItem.Model_Hitachi;
            //}
            string text = nodeUnit.Name + " [" + topModelName + "]";

            Caption label = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);
            //initTextNodeMyNodeOut(label, text, nodeUnit);
            initTextNode(label, text);
            // label.Alignment = Alignment.CenterTOP;   
            if (label.Size.Width < nodeUnit.Size.Width)
                label.Size = new System.Windows.Size(nodeUnit.Size.Width, label.Size.Height);
            //     addFlowPiping.Items.Add(label);
            addFlowPiping.AddCaption(label);
            //label.Parent = nodeUnit;
            float yModel = AddFlowExtension.FloatConverter(nodeUnit.Location.Y - UtilPiping.HeightForNodeText);  // //// To be Fix latter
            if (outNodeItem.PtModelLocation != null && outNodeItem.PtModelLocation.Count == 4)
            {
                yModel = 1;
            }
            label.Location = new Point(nodeUnit.Location.X, yModel);//nodeUnit.Location.Y - UtilPiping.HeightForNodeText
            //nodeUnit.HighlightChildren = true;  // // To be Fix latter

            #region  配管图中显示室外机分机型号   add on 20170229 by Lingjia Qiu
            //获取室外机分机型号
            string[] outModelArry = utilPiping.getOutdoorItemArry(sysItem.OutdoorItem, thisProject.BrandCode);
            float lbx = AddFlowExtension.FloatConverter(nodeUnit.Location.X);
            int index = 0;
            foreach (string modelName in outModelArry)
            {
                text = modelName;
                label = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);
                initTextNode(label, text);
                //当室外机联机数大于3个，坐标位置需要单独微调
                if (outModelArry.Length < 3)
                {
                    if (outNodeItem.PtModelLocation == null)
                    {
                        //label.Alignment = Alignment.CenterBOTTOM;    // // To be Fix latter
                    }
                }
                else
                {
                    if (outNodeItem.PtModelLocation == null)
                    {
                        //label.Alignment = Alignment.LeftJustifyMIDDLE;    // // To be Fix latter
                    }
                    if (lbx == nodeUnit.Location.X)
                    {
                        if (outNodeItem.Name.Contains("HAPB_WS") || outNodeItem.Name.Contains("NA"))  //存在特殊多联机图微调X位置
                        {
                            if (outModelArry.Length > 3)
                                lbx = AddFlowExtension.FloatConverter(nodeUnit.Location.X) + 110;
                            else
                                lbx = AddFlowExtension.FloatConverter(nodeUnit.Location.X) + 70;
                        }
                        else
                            lbx = AddFlowExtension.FloatConverter(nodeUnit.Location.X) + 55;
                    }
                }

                if (label.Size.Width < nodeUnit.Size.Width)
                    label.Size = new System.Windows.Size(nodeUnit.Size.Width, label.Size.Height);

                //addFlowPiping.Items.Add(label);
                this.addFlowPiping.AddCaption(label);
                //label.Parent = nodeUnit;                                      
                //label.Location = new Point(lbx, nodeUnit.Location.Y + 20);                                            // To be Fix latter
                label.Location = new Point(lbx, nodeUnit.Location.Y - 12);   //piping室外机分机型号纵轴位置微调
                if (outNodeItem.Name.Contains("HAPB_WS") || outNodeItem.Name.Contains("NA")) //存在特殊多联机图微调X位置
                    lbx += 45;
                else
                    lbx += 65;
                //add axj model 位置修正 20180423
                if (outNodeItem.PtModelLocation != null)
                {
                    if (outModelArry.Length == outNodeItem.PtModelLocation.Count)
                    {
                        var point = outNodeItem.PtModelLocation[index];
                        label.Location = UtilEMF.OffsetLocation(point, nodeUnit.Location);
                    }
                }
                index++;
            }
            #endregion

            //室外机制冷、制热容量
            text = "Cooling: " + Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            text += "\nHeating: " + Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            label = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);
            initTextNode(label, text);
            //addFlowPiping.Items.Add(label);
            this.addFlowPiping.AddCaption(label);
            // label.Parent = nodeUnit;    // // To be Fix latter
            ptText = new Point(nodeUnit.Location.X + nodeUnit.Size.Width + 10, nodeUnit.Location.Y + 20);
            label.Location = ptText;

            // add on 20160328 clh
            if (outNodeItem == null || outNodeItem.UnitCount <= 1)
                return;

            // TODO:绘制组合室外机内部分歧管型号及管径尺寸
            // 分歧管型号
            for (int i = 0; i < outNodeItem.PtConnectionKit.Count; ++i)
            {
                text = outNodeItem.ConnectionKitModel[i];
                ptText = UtilEMF.OffsetLocation(outNodeItem.PtConnectionKit[i], nodeUnit.Location); //nodeUnit.Location
                label = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);
                initTextNode(label, text);
                //addFlowPiping.Items.Add(label);

                label.Location = ptText;
                this.addFlowPiping.AddCaption(label);
                // label.Parent = nodeUnit;    // // To be Fix latter
                //在绘制的同时记录分歧管型号，导出报表用 add on 20160727 by Yunxiao Lin
                if (sysItem.IsExportToReport)
                    InsertPipingKitTable("PipingConnectionKit", sysItem.Name, text, 1, sysItem.Id);
            }

            string p1 = "";
            string p2 = "";
            string p3 = "";
            string p5 = "";

            nodeUnit.PipeSize = outNodeItem.PipeSize;

            //管径尺寸
            for (int i = 0; i < outNodeItem.PtPipeDiameter.Count; ++i)
            {
                ptText = UtilEMF.OffsetLocation(outNodeItem.PtPipeDiameter[i], nodeUnit.Location);
                string s = outNodeItem.PipeSize[i];
                string[] aa = s.Split('x');
                if (aa.Length == 2)
                {
                    p1 = "φ" + aa[0] + ut_pipeSize;
                    //p2 = "φ" + aa[1] + ut_pipeSize;
                    p3 = "φ" + aa[1] + ut_pipeSize;
                    if (isInch)
                    {
                        p1 = GetPipeSize_Inch(aa[0].Trim()) + ut_pipeSize;
                        //p2 = GetPipeSize_Inch(aa[1].Trim()) + ut_pipeSize;
                        p3 = GetPipeSize_Inch(aa[1].Trim()) + ut_pipeSize;
                        ptText.X += 20;
                    }
                    //text = p1 + "\n" + p2;
                }
                else if (aa.Length == 3)
                {
                    //string p1 = "φ" + aa[0] + ut_pipeSize;
                    //string p2 = "φ" + aa[1] + ut_pipeSize;
                    //原High Pressure Gas / Lower Pressure Gas 改为 Low Pressure Gas / High Pressure Gas on 20180426 by xyj
                    p1 = "φ" + aa[1] + ut_pipeSize;
                    p2 = "φ" + aa[0] + ut_pipeSize;
                    p3 = "φ" + aa[2] + ut_pipeSize;
                    if (isInch)
                    {
                        p1 = GetPipeSize_Inch(aa[0].Trim()) + ut_pipeSize;
                        p2 = GetPipeSize_Inch(aa[1].Trim()) + ut_pipeSize;
                        p3 = GetPipeSize_Inch(aa[2].Trim()) + ut_pipeSize;
                        ptText.X += 20;
                    }
                    //text = p1 + "\n" + p2 + "\n" + p3;
                }

                //室外机机组内部的连接管的各部分管长  add by Shen Junjie on 20170718
                if (sysItem.IsInputLengthManually && outNodeItem.UnitCount > 1)
                {
                    double pipeLength = 0;
                    if (nodeUnit.PipeLengthes != null && nodeUnit.PipeLengthes.Length > i)
                    {
                        pipeLength = nodeUnit.PipeLengthes[i];
                    }
                    // text += "\n" + Unit.ConvertToControl(pipeLength, UnitType.LENGTH_M, ut_length).ToString("0.##") + ut_length;
                    if (aa.Length == 2)
                        p5 = "\n" + Unit.ConvertToControl(pipeLength, UnitType.LENGTH_M, ut_length).ToString("0.##") + ut_length;
                    else if (aa.Length == 3)
                        p5 = "\n" + Unit.ConvertToControl(pipeLength, UnitType.LENGTH_M, ut_length).ToString("0.##") + ut_length;

                }

                //label = new Node();
                //initTextNode(label, text);
                //addFlowPiping.Nodes.Add(label);

                Caption label1 = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);    //低压气管
                if (!string.IsNullOrEmpty(p1))
                {
                    //ptText.Y -= 10;
                    label1.Location = ptText;
                    if (aa.Length == 2)   //室外机分机两管只有：气管和液管
                        DrawPipeLableColor(label1, p1, nodeUnit, 4);
                    else
                        DrawPipeLableColor(label1, p1, nodeUnit, 1);
                }
                Caption label2 = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);   //高压气管
                if (!string.IsNullOrEmpty(p2))
                {
                    ptText.Y += 8;
                    label2.Location = ptText;
                    DrawPipeLableColor(label2, p2, nodeUnit, 2);
                    //this.addFlowPiping.AddCaption(label2);
                }
                Caption label3 = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);    //液管
                if (!string.IsNullOrEmpty(p3))
                {
                    ptText.Y += 8;
                    label3.Location = ptText;
                    DrawPipeLableColor(label3, p3, nodeUnit, 3);
                    //this.addFlowPiping.AddCaption(label3);
                }
                Caption label5 = new Caption(10, 10, 100, 30, text, nodeUnit, this.addFlowPiping);  //室外机占分机管长
                if (!string.IsNullOrEmpty(p5))
                {
                    label5.Location = ptText;
                    DrawPipeLableColor(label5, p5, nodeUnit, 5);
                    //this.addFlowPiping.AddCaption(label5);
                }

                ////3行 4行文字时，遮挡线条的问题  add by Shen Junjie 2018/01/11
                //不能解决所有机型的问题，改用数据库坐标调整。 delete by Shen Junjie 2018/01/29
                //if (outNodeItem.UnitCount == 2)
                //{
                //    if (i == 0)
                //    {
                //        ptText.X = 100;
                //        ptText.Y = 130 - label.Size.Height;
                //    }
                //    else
                //    {
                //        ptText.X = 170;
                //        ptText.Y = 130 - label.Size.Height;
                //    }
                //}
                //else if (outNodeItem.UnitCount == 3)
                //{
                //    if (i == 1)
                //    {
                //        ptText.X += 4;
                //        ptText.Y = 122 - label.Size.Height;
                //    }
                //    else if (i == 2)
                //    {
                //        ptText.Y = 122 - label.Size.Height;
                //    }
                //    else if (i == 3)
                //    {
                //        ptText.Y = 128 - label.Size.Height;
                //    }
                //}
                //else if (outNodeItem.UnitCount == 4)
                //{
                //    if (i > 0)
                //    {
                //        ptText.Y = 118 - label.Size.Height;
                //    }
                //}

            }
        }

        void initTextNode(JCHNode label, string text)
        {
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font ft = textFont_piping;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 8;
            label.Foreground = System.Windows.Media.Brushes.Black;
            label.Stroke = System.Windows.Media.Brushes.Black;
            label.Fill = System.Windows.Media.Brushes.Transparent;
            label.Text = text;
            SizeF size = gMeasureString.MeasureString(text, ft);
            label.Size = new System.Windows.Size(size.Width + 5, size.Height + 3);
            label.ImagePosition = ImagePosition.LeftTop;
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            // label.Logical = false;
            //label.Selectable = false;
            label.IsSelectable = false;
            // label.AttachmentStyle = AttachmentStyle.Item;
            // label.ZOrder = 10;
            try
            {
                label.ImageData = new JCHVRF.Model.NextGen.ImageData();
            }
            catch { }

        }
        void initTextNode(Caption label, string text)
        {
            label.FontFamily = new FontFamily("Arial");
            label.FontSize = 9;
            //label.Stroke = utilPiping.colorText;
            label.TextPosition = TextPosition.LeftMiddle;
            label.Text = text;
            //label.Size = new System.Windows.Size(100, 50);
            //label.Geometry = new RectangleGeometry(new Rect(10, 90, 61, 65), 3, 3);

            label.IsSelectable = false;

            label.Stroke = System.Windows.Media.Brushes.Transparent;
            //label.Fill = System.Windows.Media.Brushes.LightGray;
            label.Dock = DockStyle.Left;
            label.IsHitTestVisible = false;
        }

        void initTextMyNode(Node label, string text)
        {
            //label.FontFamily = new FontFamily("Arial");
            //label.FontSize = 9;
            //label.Stroke = utilPiping.colorText;
            label.TextPosition = TextPosition.LeftMiddle;
            label.Text = text;

            //label.Location = new Point(label.Location.X -10, label.Location.Y-10);
            //label.Size = new System.Windows.Size(100, 50);
            //label.Geometry = new RectangleGeometry(new Rect(10, 90, 61, 65), 3, 3);

            label.IsSelectable = false;

            // label.Stroke = System.Windows.Media.Brushes.Transparent;
            //label.Fill = System.Windows.Media.Brushes.LightGray;
            //label.Dock = DockStyle.Left;
            label.IsHitTestVisible = false;
            label.Stroke = System.Windows.Media.Brushes.RoyalBlue;
        }
        void initTextNodeMyNodeOut(Node label, string text, MyNodeOut nodeunit)
        {
            label.FontFamily = new FontFamily("Arial");
            label.FontSize = 9;
            //label.Stroke = utilPiping.colorText;
            //label.TextPosition = TextPosition.LeftMiddle;
            label.Text = text;
            label.Size = new System.Windows.Size(100, 50);
            label.Location = new Point(nodeunit.Location.X - 10, nodeunit.Location.Y);
            // label.Geometry = new RectangleGeometry(new Rect(10, 90, 61, 65), 3, 3);

            label.IsSelectable = false;
        }


        #endregion



        #region 绘制节点文字和连接线文字以及Correction Factor

        public void GetHeightDifferenceNodes(Node node, Node parent, SystemVRF sysItem, List<MyNode> list)
        {
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                GetHeightDifferenceNodes(nodeOut.ChildNode, nodeOut, sysItem, list);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node item in nodeYP.ChildNodes)
                {
                    GetHeightDifferenceNodes(item, nodeYP, sysItem, list);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                list.Add(nodeCH);
                GetHeightDifferenceNodes(nodeCH.ChildNode, nodeCH, sysItem, list);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                list.Add(nodeMCH);
                foreach (Node item in nodeMCH.ChildNodes)
                {
                    GetHeightDifferenceNodes(item, nodeMCH, sysItem, list);
                }
            }
            else if (node is MyNodeIn)
            {
                //因为DoPipingCalculation之后可能影响indoor的管径，
                //所以绘制YP型号的时候顺便绘制indoor的管径 add on 20170512 by Shen Junjie
                MyNodeIn nodeIn = node as MyNodeIn;
                list.Add(nodeIn);
            }
        }


        public void DrawTextToAllNodes(Node node, Node parent, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                DrawTextToAllNodes(nodeOut.ChildNode, nodeOut, sysItem, ref addFlowPiping);
                //nodeOut.Stroke = System.Windows.Media.Brushes.Red;
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                drawTextToOtherNode(node, parent, sysItem, ref addFlowPiping);

                foreach (Node item in nodeYP.ChildNodes)
                {
                    DrawTextToAllNodes(item, nodeYP, sysItem, ref addFlowPiping);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                drawTextToOtherNode(node, parent, sysItem, ref addFlowPiping);

                DrawTextToAllNodes(nodeCH.ChildNode, nodeCH, sysItem, ref addFlowPiping);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                drawTextToOtherNode(node, parent, sysItem);

                foreach (Node item in nodeMCH.ChildNodes)
                {
                    DrawTextToAllNodes(item, nodeMCH, sysItem, ref addFlowPiping);
                }
            }
            else if (node is MyNodeIn)
            {
                //因为DoPipingCalculation之后可能影响indoor的管径，
                //所以绘制YP型号的时候顺便绘制indoor的管径 add on 20170512 by Shen Junjie
                // MyNodeIn nodeIn = node as MyNodeIn;
                // drawTextToIDUNode(sysItem, nodeIn,ref addFlowPiping);
                //drawTextToLink(parent, node, isInch, sysItem);
                //node.Stroke = System.Windows.Media.Brushes.Red;
            }

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                for (int i = 0; i < myNode.MyInLinks.Count; i++)
                {
                    MyLink myLink = myNode.MyInLinks[i];
                    myLink.Points = myNode.Links[i].Points;
                    myLink.Length = (myNode.Links[i] as MyLink).Length;
                    drawTextToLink(myLink, i, parent, node, isInch, sysItem, ref addFlowPiping);
                }
            }
        }

        public void DoPipingCalculation(SystemVRF curSystemItem, MyNodeOut nodeOut, out PipingErrors errorType)
        {
            errorType = PipingErrors.OK;
            if (nodeOut.ChildNode == null) return;
            GetSumCapacity(nodeOut.ChildNode);

            IsBranchKitNeedSizeUp(curSystemItem);

            OldModel.PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is MyNodeYP)
            {
                MyNodeYP nodeYP = nodeOut.ChildNode as MyNodeYP;
                if (nodeYP.IsCP)
                {
                    firstBranchKit = getFirstHeaderBranchPipeCalculation(nodeYP, curSystemItem, out errorType);
                }
                else
                {
                    firstBranchKit = getFirstPipeCalculation(nodeYP, curSystemItem, out errorType);
                }
                if (errorType != PipingErrors.OK)
                {

                    return;
                }
            }
            //bug 3489
            var L2SizeDownRule = GetL2SizeDownRule(curSystemItem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            //bug 3489

            getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, curSystemItem, false, out errorType, L2SizeDownRule);
            if (errorType != PipingErrors.OK)
            {

                return;
            }
            CheckIndoorNumberConnectedCHBox(nodeOut);
        }


        public List<OldModel.PipingOrientation> CheckYPLinkOrientation(MyNodeYP nodeYP)
        {
            List<OldModel.PipingOrientation> result = new List<OldModel.PipingOrientation>();
            for (int i = 0; i < nodeYP.Links.Count; i++)
            {
                OldModel.PipingOrientation result1 = CheckLinkOrientation(nodeYP, nodeYP.Links[i]);
                if (result1 != OldModel.PipingOrientation.Unknown)
                {
                    result.Add(result1);
                }
            }
            return result;
        }

        public OldModel.PipingOrientation CheckLinkOrientation(Node node, Link link)
        {
            if (node == null || link == null) return OldModel.PipingOrientation.Unknown;

            bool left = false;
            bool right = false;
            bool up = false;
            bool down = false;
            //父节点四个顶点的坐标
            float x1 = AddFlowExtension.FloatConverter(node.Location.X);
            float x2 = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            float y1 = AddFlowExtension.FloatConverter(node.Location.Y);
            float y2 = AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height);

            //验证折线的每一段，正反各检测一次
            Point pt1, pt2;
            bool done = false;
            for (int i = 0; !done && i < link.Points.Count - 1; i++)
            {
                for (int j = 0; !done && j <= 1; j++)
                {
                    if (j == 0)
                    {
                        //正向
                        pt1 = link.Points[i];
                        pt2 = link.Points[i + 1];
                    }
                    else
                    {
                        //反向
                        pt1 = link.Points[i + 1];
                        pt2 = link.Points[i];
                    }

                    //判断线段起始坐标是否在节点范围内
                    if (pt1.X >= x1 && pt1.X <= x2 && pt1.Y >= y1 && pt1.Y <= y2)
                    {
                        //判断线段结束坐标是否在节点范围外
                        if (pt2.X < x1 || pt2.X > x2 || pt2.Y < y1 || pt2.Y > y2)
                        {
                            //判断线段1的方向
                            if (pt1.X == pt2.X)
                            {
                                //竖线
                                if (pt2.Y < y1) up = true; //向上
                                if (pt2.Y > y2) down = true; //向下
                            }
                            if (pt1.Y == pt2.Y)
                            {
                                //横线
                                if (pt2.X < x1) left = true; //向左
                                if (pt2.X > x2) right = true; //向右
                            }
                            done = true;
                        }
                    }
                }
            }

            if (up) return OldModel.PipingOrientation.Up;
            if (down) return OldModel.PipingOrientation.Down;
            if (left) return OldModel.PipingOrientation.Left;
            if (right) return OldModel.PipingOrientation.Right;

            return OldModel.PipingOrientation.Unknown;
        }

        /// 为Link添加文字，管径规格（气管|液管）
        /// <summary>
        /// 为Link添加文字，管径规格（气管|液管）
        /// </summary>
        /// <param name="myLink"></param>
        private void drawTextToLink(MyLink myLink, int linkIndex, Node parent, Node node, bool isInch, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            // 绘制连接管管径文字
            if (myLink == null || parent == null || node == null) return;

            //string text1 = "";
            string p1 = "";   //Low Pressure Gas
            string p2 = "";   //High Pressure Gas
            string p3 = "";   //Liquid
            string p5 = "";

            //YP CHBox等,Piping验证通过了才显示管径 防止显示错误的管径 add by Shen Junjie on 2018/02/01
            //因为可以改变室内机出现型号和管径不同步的现象，未验证通过时，OldModel.Indoor 也不要显示管径了。 add by Shen Junjie on 2018/5/29
            if (sysItem.IsPipingOK)
            {
                string SpecL = myLink.SpecL;
                string SpecG_h = myLink.SpecG_h;
                string SpecG_l = myLink.SpecG_l;
                if (!string.IsNullOrEmpty(SpecL) && !string.IsNullOrEmpty(SpecG_h))
                {
                    //string text1 = "φ" + SpecG_h + "mm\nφ" + SpecL + "mm";
                    //if (SpecG_l != null && SpecG_l != "-")
                    //    text1 = "φ" + SpecG_l + "mm\nφ" + SpecG_h + "mm\nφ" + SpecL + "mm";

                    // 对于is cooling only SpecG_h也有可能为空 modify on 20160517 by Yunxiao Lin
                    //High Pressure Gas / Low Pressure Gas / Liquid  顺序改为 Low Pressure Gas / High Pressure Gas / Liquid on 20180426 by xyj
                    //if (SpecG_l != null && SpecG_l != "-")
                    //    text1 += "φ" + SpecG_l + ut_pipeSize + "\n";
                    //if (SpecG_h != null && SpecG_h != "-")
                    //    text1 += "φ" + SpecG_h + ut_pipeSize + "\n";
                    //text1 += "φ" + SpecL + ut_pipeSize;

                    if (SpecG_l != null && SpecG_l != "-")
                        p1 = "φ" + SpecG_l + ut_pipeSize;
                    if (SpecG_h != null && SpecG_h != "-")
                        p2 = "φ" + SpecG_h + ut_pipeSize;
                    p3 = "φ" + SpecL + ut_pipeSize;

                    if (isInch)
                    {
                        SpecL = GetPipeSize_Inch(myLink.SpecL);
                        SpecG_h = GetPipeSize_Inch(myLink.SpecG_h);
                        SpecG_l = GetPipeSize_Inch(myLink.SpecG_l);
                        //text1=SpecG_h + "\"\n" + SpecL + "\"";
                        //if (SpecG_l != "-")
                        //    text1 = SpecG_h + "\"\n" + SpecG_l + "\"\n" + SpecL + "\"";

                        //同上 modify on 20160517 by Yunxiao Lin
                        //text1 = "";
                        //if (SpecG_l != null && SpecG_l != "-")
                        //    text1 += SpecG_l + ut_pipeSize + "\n";
                        //if (SpecG_h != null && SpecG_h != "-")
                        //    text1 += SpecG_h + ut_pipeSize + "\n";
                        //text1 += SpecL + ut_pipeSize;

                        p1 = "";
                        p2 = "";
                        p3 = "";
                        if (SpecG_l != null && SpecG_l != "-")
                            p1 = SpecG_l + ut_pipeSize;
                        if (SpecG_h != null && SpecG_h != "-")
                            p2 = SpecG_h + ut_pipeSize;
                        p3 = SpecL + ut_pipeSize;
                    }
                }
            }
            sysItem.IsInputLengthManually = true;
            //管长标注
            if (sysItem.IsInputLengthManually)
            {
                //text1 += "\n" + Unit.ConvertToControl(myLink.Length, UnitType.LENGTH_M, ut_length).ToString("0.##") + ut_length;
                p5 = Unit.ConvertToControl(myLink.Length, UnitType.LENGTH_M, ut_length).ToString("0.##") + ut_length;
            }

            //Node label1 = new Node();
            //initTextNode(label1, text1);
            //addFlowPiping.Nodes.Add(label1);
            //label1.Parent = node;

            bool isCoolingOnly;
            if (node is MyNodeIn)
                isCoolingOnly = (node as MyNodeIn).IsCoolingonly;
            else if (node is MyNodeYP)
                isCoolingOnly = (node as MyNodeYP).IsCoolingonly;
            else
                isCoolingOnly = false;

            float width = 0;  //预设最大宽度值
            float height = 5; //预设最大高度值
            //Caption label1 = new Caption(10, 10, 60, 50, "", node, this.addFlowPiping);
            JCHNode label1 = new JCHNode();
            //当HR是两管非CoolingOnly时，标注普通气管
            if (!string.IsNullOrEmpty(p1))
            {
                if (string.IsNullOrEmpty(p2) && !isCoolingOnly)
                    DrawPipeLableColor(label1, p1, node, 4, ref addFlowPiping);
                else
                    DrawPipeLableColor(label1, p1, node, 1, ref addFlowPiping);
                height += 8;
            }

            //Caption label2 = new Caption(10, 10, 60, 50, "", node, this.addFlowPiping);
            JCHNode label2 = new JCHNode();
            if (!string.IsNullOrEmpty(p2))
            {
                if (parent is MyNodeMultiCH || parent is MyNodeCH)  //父节点为CHBOX切换GasPipe 颜色
                    DrawPipeLableColor(label2, p2, node, 4, ref addFlowPiping);
                else if (string.IsNullOrEmpty(p1))  //当HR是两管是，标注普通气管
                    DrawPipeLableColor(label2, p2, node, 4, ref addFlowPiping);
                else
                {
                    //如果headerBranch的父节点是CHBOX那其下的子节点管径切换GasPipe颜色
                    if (parent is MyNodeYP)
                    {
                        MyNodeYP pNodeYp = parent as MyNodeYP;
                        if (pNodeYp.ParentNode is MyNodeMultiCH || pNodeYp.ParentNode is MyNodeCH)
                            DrawPipeLableColor(label2, p2, node, 4, ref addFlowPiping);
                        else
                            DrawPipeLableColor(label2, p2, node, 2, ref addFlowPiping);
                    }
                    else
                        DrawPipeLableColor(label2, p2, node, 2, ref addFlowPiping);
                }
                height += 8;
            }

            //Caption label3 = new Caption(10, 10, 60, 50, "", node, this.addFlowPiping);
            JCHNode label3 = new JCHNode();
            if (!string.IsNullOrEmpty(p3))
            {
                DrawPipeLableColor(label3, p3, node, 3, ref addFlowPiping);
                height += 8;
            }
            //Caption label5 = new Caption(10, 10, 60, 50, "", node, this.addFlowPiping);
            JCHNode label5 = new JCHNode();
            if (!string.IsNullOrEmpty(p5))
            {
                DrawPipeLableColor(label5, p5, node, 5, ref addFlowPiping);
                height += 8;
            }
            // width = Math.Max(Math.Max(AddFlowExtension.FloatConverter(label1.Size.Width), AddFlowExtension.FloatConverter(label2.Size.Width)), Math.Max(AddFlowExtension.FloatConverter(label3.Size.Width), AddFlowExtension.FloatConverter(label5.Size.Width)));   //预设最大宽度值
            label1.Size = new System.Windows.Size(width, height);
            //label1.Size = new SizeF(width, label1.Size.Height);
            if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
            {
                // SetLinkTextLocationForBinaryTree(myLink, linkIndex, parent, node, label1, sysItem.IsPipingVertical);
            }
            else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.SchemaA)
            {
                SetLinkTextLocationForSechmaA(myLink, linkIndex, parent, node, label1, sysItem.IsPipingVertical);
            }
            else
            {
                SetLinkTextLocationForNormal(myLink, linkIndex, parent, node, label1, sysItem.IsPipingVertical);
            }

            //管径位置微调
            Point pf = label1.Location;
            if (!string.IsNullOrEmpty(p1))
            {
                pf.Y += 8;
            }
            if (!string.IsNullOrEmpty(p2))
            {
                label2.Location = pf;
                pf.Y += 8;
            }
            if (!string.IsNullOrEmpty(p3))
            {
                label3.Location = pf;
                pf.Y += 8;
            }
            if (!string.IsNullOrEmpty(p5))
            {
                label5.Location = pf;
                pf.Y += 8;
            }
        }

        private void SetLinkTextLocationForBinaryTree(MyLink myLink, int linkIndex, Node parent, Node node, Caption label1, bool isVertical)
        {
            Point ptText, ptParent, ptNode;
            ptText = utilPiping.getLeftCenterPointF(node);
            if (parent is MyNodeOut)
            {
                ptParent = utilPiping.getLeftBottomPointF(parent);
            }
            else if (parent is MyNodeCH)
            {
                if (isVertical)
                {
                    ptParent = utilPiping.getRightCenterPointF(parent);
                }
                else
                {
                    ptParent = utilPiping.getBottomCenterPointF(parent);
                }
            }
            else if (parent is MyNodeMultiCH)
            {
                if (isVertical)
                {
                    ptParent = myLink.Points[2];
                }
                else
                {
                    ptParent = utilPiping.getBottomCenterPointF(parent);
                }
            }
            else
            {
                ptParent = utilPiping.getCenterPointF(parent);
            }

            if (isVertical)
            {
                //垂直布局
                ptNode = utilPiping.getLeftCenterPointF(node);

                ptText.X = (ptNode.X + ptParent.X - label1.Size.Width) / 2; //水平居中
                ptText.Y = ptNode.Y + 2;
            }
            else
            {
                //水平布局
                ptNode = utilPiping.getTopCenterPointF(node);

                ptText.X = ptNode.X - label1.Size.Width;
                ptText.Y = (ptNode.Y + ptParent.Y - label1.Size.Height) / 2; //垂直居中
            }

            //第二根link For FSN3PE add by Shen Junjie on 2018/01/23
            if (linkIndex == 1)
            {
                if (parent is MyNodeYP)
                {
                    if (node is MyNodeIn)
                    {
                        ptParent = utilPiping.getCenterPointF(parent);
                        if (isVertical)
                        {
                            ptText.Y = ptParent.Y - label1.Size.Height - 2;
                        }
                        else
                        {
                            ptText.X = ptParent.X + 5;
                        }
                    }
                }
            }
            ptText.X = Math.Max(0, ptText.X);
            ptText.Y = Math.Max(0, ptText.Y);
            label1.Location = ptText;
        }

        private void SetLinkTextLocationForSechmaA(MyLink myLink, int linkIndex, Node parent, Node node, Caption label1, bool isVertical)
        {
            Point ptText, ptParent, ptNode;
            ptText = utilPiping.getLeftCenterPointF(node);
            OldModel.PipingOrientation inlinkOrientation = CheckLinkOrientation(node, myLink);

            if (inlinkOrientation == OldModel.PipingOrientation.Down)
            {
                ptParent = parent.Location;
                ptNode = utilPiping.getBottomCenterPointF(node);

                ptText.X = ptNode.X - label1.Size.Width;
                ptText.Y = (ptNode.Y + ptParent.Y - label1.Size.Height) / 2;
            }
            else if (inlinkOrientation == OldModel.PipingOrientation.Up)
            {
                ptParent = utilPiping.getLeftBottomPointF(parent);
                ptNode = utilPiping.getTopCenterPointF(node);

                ptText.X = ptNode.X - label1.Size.Width;
                ptText.Y = (ptNode.Y + ptParent.Y - label1.Size.Height) / 2;
            }
            else if (inlinkOrientation == OldModel.PipingOrientation.Left)
            {
                if (parent is MyNodeOut)
                {
                    ptParent = utilPiping.getLeftBottomPointF(parent);
                }
                else if (parent is MyNodeMultiCH)
                {
                    ptParent = myLink.Points[myLink.Points.Count - 2];
                }
                else
                {
                    ptParent = utilPiping.getRightBottomPointF(parent);
                }
                ptNode = utilPiping.getLeftCenterPointF(node);

                ptText.X = (ptNode.X + ptParent.X - label1.Size.Width) / 2;
                ptText.Y = ptNode.Y;
            }
            else if (inlinkOrientation == OldModel.PipingOrientation.Right)
            {
                ptParent = utilPiping.getLeftBottomPointF(parent);
                ptNode = utilPiping.getRightCenterPointF(node);

                if (parent is MyNodeMultiCH)
                {
                    ptParent = myLink.Points[myLink.Points.Count - 2];
                }

                ptText.X = (ptNode.X + ptParent.X - label1.Size.Width) / 2;
                ptText.Y = ptNode.Y;
            }

            if (isVertical)
            {
                //垂直布局
                if (node is MyNodeIn)
                {
                    if (inlinkOrientation == OldModel.PipingOrientation.Right)
                    {
                        ptNode = utilPiping.getRightCenterPointF(node);
                        ptText.X = ptNode.X + 20;
                        ptText.Y = ptNode.Y;
                    }
                    else
                    {
                        ptNode = utilPiping.getLeftCenterPointF(node);
                        ptText.X = ptNode.X - label1.Size.Width;
                        ptText.Y = ptNode.Y;
                    }
                }
            }
            else
            {
                //水平布局
                if (node is MyNodeIn)
                {
                    ptNode = utilPiping.getLeftCenterPointF(node);
                    ptParent = myLink.Points[0];
                    ptText.X = ptNode.X - label1.Size.Width;
                    ptText.Y = ptNode.Y;
                    if (ptNode.Y < ptParent.Y)
                    {
                        //文字在线条上面
                        ptText.Y = ptNode.Y - label1.Size.Height;
                    }
                }
                else if (node is MyNodeYP)
                {
                    if (parent is MyNodeMultiCH)
                    {
                        ptParent = myLink.Points[1];
                        ptNode = myLink.Points[2];
                        ptText.X = ptParent.X - label1.Size.Width;
                        ptText.Y = ptParent.Y;
                        if (ptNode.Y < ptParent.Y)
                        {
                            //文字在线条上面
                            ptText.Y = ptParent.Y - label1.Size.Height;
                        }
                    }
                }
            }

            //第二根link For FSN3PE add by Shen Junjie on 2018/01/22
            if (linkIndex == 1)
            {
                if (node is MyNodeIn)
                {
                    if (parent is MyNodeYP)
                    {
                        ptParent = utilPiping.getCenterPointF(parent);
                        ptNode = utilPiping.getRightCenterPointF(node);
                        if (isVertical)
                        {
                            ptText.Y = ptParent.Y - label1.Size.Height - 2;
                        }
                        else
                        {
                            if (ptNode.Y < ptParent.Y)
                            {
                                //室内机在YP上面
                                ptText.X = ptParent.X + 5;
                                ptText.Y = ptParent.Y - label1.Size.Height - 25;
                            }
                            else
                            {
                                ptText.X = ptParent.X + 5;
                                ptText.Y = ptParent.Y + 15;
                            }
                        }
                    }
                }
            }
            ptText.X = Math.Max(0, ptText.X);
            ptText.Y = Math.Max(0, ptText.Y);
            label1.Location = ptText;
        }
        private void SetLinkTextLocationForSechmaA(ng.MyLink myLink, int linkIndex, Node parent, Node node, Node label1, bool isVertical)
        {
            PointF ptText, ptParent, ptNode;
            ptText = getLeftCenterPointF(node);
            OldModel.PipingOrientation inlinkOrientation = CheckLinkOrientation(node, myLink);

            if (inlinkOrientation == OldModel.PipingOrientation.Down)
            {
                ptParent = convertSystemPointToDrawingPoint(parent.Location);
                //ptParent = parent.Location;
                ptNode = getBottomCenterPointF(node);

                ptText.X = (float)(ptNode.X - label1.Size.Width);
                //ptText.X = ptNode.X - label1.Size.Width;
                ptText.Y = (float)((ptNode.Y + ptParent.Y - label1.Size.Height) / 2);
                //ptText.Y = (ptNode.Y + ptParent.Y - label1.Size.Height) / 2;
            }
            else if (inlinkOrientation == OldModel.PipingOrientation.Up)
            {
                ptParent = getLeftBottomPointF(parent);
                ptNode = getTopCenterPointF(node);

                ptText.X = (float)(ptNode.X - label1.Size.Width);
                ptText.Y = (float)((ptNode.Y + ptParent.Y - label1.Size.Height) / 2);
            }
            else if (inlinkOrientation == OldModel.PipingOrientation.Left)
            {
                if (parent is ng.MyNodeOut)
                {
                    ptParent = getLeftBottomPointF(parent);
                }
                else if (parent is ng.MyNodeMultiCH)
                {
                    ptParent = convertSystemPointToDrawingPoint(myLink.Points[myLink.Points.Count - 2]);
                    //ptParent = myLink.Points[myLink.Points.Count - 2];
                }
                else
                {
                    ptParent = getRightBottomPointF(parent);
                }
                ptNode = getLeftCenterPointF(node);

                ptText.X = (float)((ptNode.X + ptParent.X - label1.Size.Width) / 2);
                ptText.Y = ptNode.Y;
            }
            else if (inlinkOrientation == OldModel.PipingOrientation.Right)
            {
                ptParent = getLeftBottomPointF(parent);
                ptNode = getRightCenterPointF(node);

                if (parent is ng.MyNodeMultiCH)
                {
                    ptParent = convertSystemPointToDrawingPoint(myLink.Points[myLink.Points.Count - 2]);
                    //ptParent = myLink.Points[myLink.Points.Count - 2];
                }

                ptText.X = (float)(ptNode.X + ptParent.X - label1.Size.Width) / 2;
                ptText.Y = ptNode.Y;
            }

            if (isVertical)
            {
                if (node is ng.MyNodeIn)
                {
                    if (inlinkOrientation == OldModel.PipingOrientation.Right)
                    {
                        ptNode = getRightCenterPointF(node);
                        ptText.X = ptNode.X + 20;
                        ptText.Y = ptNode.Y;
                    }
                    else
                    {
                        ptNode = getLeftCenterPointF(node);
                        ptText.X = (float)(ptNode.X - label1.Size.Width);
                        ptText.Y = ptNode.Y;
                    }
                }
            }
            else
            {
                if (node is ng.MyNodeIn)
                {
                    ptNode = getLeftCenterPointF(node);
                    ptParent = convertSystemPointToDrawingPoint(myLink.Points[0]);
                    //ptParent = myLink.Points[0];
                    ptText.X = (float)(ptNode.X - label1.Size.Width);
                    ptText.Y = ptNode.Y;
                    if (ptNode.Y < ptParent.Y)
                    {
                        ptText.Y = (float)(ptNode.Y - label1.Size.Height);
                    }
                }
                else if (node is ng.MyNodeYP)
                {
                    if (parent is ng.MyNodeMultiCH)
                    {
                        ptParent = convertSystemPointToDrawingPoint(myLink.Points[1]);
                        //ptParent = myLink.Points[1];
                        ptNode = convertSystemPointToDrawingPoint(myLink.Points[2]);
                        //ptNode = myLink.Points[2];
                        ptText.X = (float)(ptParent.X - label1.Size.Width);
                        ptText.Y = ptParent.Y;
                        if (ptNode.Y < ptParent.Y)
                        {
                            ptText.Y = (float)(ptParent.Y - label1.Size.Height);
                        }
                    }
                }
            }

            if (linkIndex == 1)
            {
                if (node is ng.MyNodeIn)
                {
                    if (parent is ng.MyNodeYP)
                    {
                        ptParent = getCenterPointF(parent);
                        ptNode = getRightCenterPointF(node);
                        if (isVertical)
                        {
                            ptText.Y = (float)(ptParent.Y - label1.Size.Height - 2);
                        }
                        else
                        {
                            if (ptNode.Y < ptParent.Y)
                            {
                                ptText.X = ptParent.X + 5;
                                ptText.Y = (float)(ptParent.Y - label1.Size.Height - 25);
                            }
                            else
                            {
                                ptText.X = ptParent.X + 5;
                                ptText.Y = ptParent.Y + 15;
                            }
                        }
                    }
                }
            }
            ptText.X = Math.Max(0, ptText.X);
            ptText.Y = Math.Max(0, ptText.Y);
            label1.Location = convertPointFToWinPoint(ptText);
            //label1.Location = ptText;
        }
        private void SetLinkTextLocationForNormal(MyLink myLink, int linkIndex, Node parent, Node node, Caption label1, bool isVertical)
        {
            Point ptText, ptParent, ptNode;
            ptText = utilPiping.getLeftCenterPointF(node);
            if (node is MyNodeYP)
            {
                if (parent is MyNodeOut)
                {
                    ptText.X = (node.Location.X + parent.Location.X - label1.Size.Width) / 2;
                }
                else if (parent is MyNodeMultiCH)
                {
                    Point pt1 = myLink.Points[myLink.Points.Count - 2];
                    Point pt2 = myLink.Points[myLink.Points.Count - 1];
                    ptText = new Point((pt1.X + pt2.X - label1.Size.Width) / 2, pt2.Y);
                }
                else
                {
                    ptNode = utilPiping.getCenterPointF(node);
                    ptParent = utilPiping.getRightCenterPointF(parent);
                    if (ptParent.Y == ptText.Y)
                    {
                        // 纵坐标不变
                        ptText.X = (ptNode.X + ptParent.X - label1.Size.Width) / 2;
                    }
                    else
                    {
                        ptText.X = ptNode.X - label1.Size.Width;
                        ptText.Y = (ptNode.Y + ptParent.Y - label1.Size.Height) / 2;
                    }
                }
            }
            else if (node is MyNodeCH || node is MyNodeMultiCH)
            {
                if (isVertical)
                {
                    if (parent is MyNodeOut)
                    {
                        ptText.X = ptText.X - label1.Size.Width;
                    }
                    else
                    {
                        ptNode = utilPiping.getCenterPointF(node);
                        ptParent = utilPiping.getCenterPointF(parent);
                        ptText.X = (ptNode.X + ptParent.X - label1.Size.Width) / 2;
                    }
                }
                else
                {
                    ptNode = utilPiping.getTopCenterPointF(node);
                    ptText.X = ptNode.X - label1.Size.Width;
                    ptText.Y = ptNode.Y - label1.Size.Height;
                }
            }
            else if (node is MyNodeIn)
            {
                ptText.X = ptText.X - label1.Size.Width;
                //第二根link For FSN3PE add by Shen Junjie on 2018/01/22
                if (linkIndex == 1)
                {
                    ptParent = utilPiping.getCenterPointF(parent);
                    if (isVertical)
                    {
                        ptText.Y = ptParent.Y - label1.Size.Height - 2;
                    }
                    else
                    {
                        ptText.X = ptParent.X + 5;
                        ptText.Y = ptParent.Y + 15;
                    }
                }
            }
            ptText.X = Math.Max(0, ptText.X);
            ptText.Y = Math.Max(0, ptText.Y);
            label1.Location = ptText;
        }

        private void SetLinkTextLocationForNormal(ng.MyLink myLink, int linkIndex, Node parent, Node node, Node label1, bool isVertical)
        {
            PointF ptText, ptParent, ptNode;
            ptText = getLeftCenterPointF(node);
            if (node is ng.MyNodeYP)
            {
                if (parent is ng.MyNodeOut)
                {
                    ptText.X = (float)((node.Location.X + parent.Location.X - label1.Size.Width) / 2);
                }
                else if (parent is ng.MyNodeMultiCH)
                {
                    PointF pt1 = convertSystemPointToDrawingPoint(myLink.Points[myLink.Points.Count - 2]);
                    PointF pt2 = convertSystemPointToDrawingPoint(myLink.Points[myLink.Points.Count - 1]);
                    ptText = new PointF(pt1.X + pt2.X - (float)(label1.Size.Width) / 2, pt2.Y);
                }
                else
                {
                    ptNode = getCenterPointF(node);
                    ptParent = getRightCenterPointF(parent);
                    if (ptParent.Y == ptText.Y)
                    {
                        ptText.X = (float)((ptNode.X + ptParent.X - label1.Size.Width) / 2);
                    }
                    else
                    {
                        ptText.X = (float)(ptNode.X - label1.Size.Width);
                        ptText.Y = (float)((ptNode.Y + ptParent.Y - label1.Size.Height) / 2);
                    }
                }
            }
            else if (node is ng.MyNodeCH || node is ng.MyNodeMultiCH)
            {
                if (isVertical)
                {
                    if (parent is ng.MyNodeOut)
                    {
                        ptText.X = (float)(ptText.X - label1.Size.Width);
                    }
                    else
                    {
                        ptNode = getCenterPointF(node);
                        ptParent = getCenterPointF(parent);
                        ptText.X = (float)((ptNode.X + ptParent.X - label1.Size.Width) / 2);
                    }
                }
                else
                {
                    ptNode = getTopCenterPointF(node);
                    ptText.X = (float)(ptNode.X - label1.Size.Width);
                    ptText.Y = (float)(ptNode.Y - label1.Size.Height);
                }
            }
            else if (node is ng.MyNodeIn)
            {
                ptText.X = (float)(ptText.X - label1.Size.Width);

                if (linkIndex == 1)
                {
                    ptParent = getCenterPointF(parent);
                    if (isVertical)
                    {
                        ptText.Y = (float)(ptParent.Y - label1.Size.Height - 2);
                    }
                    else
                    {
                        ptText.X = ptParent.X + 5;
                        ptText.Y = ptParent.Y + 15;
                    }
                }
            }
            ptText.X = Math.Max(0, ptText.X);
            ptText.Y = Math.Max(0, ptText.Y);
            label1.Location = convertPointFToWinPoint(ptText);
        }
        /// 为YP|CP|CHBox|Multi CHBox节点添加文字（型号）
        /// <summary>
        /// 为YP|CP|CHBox|Multi CHBox节点添加文字（型号）
        /// </summary>
        /// <param name="myNodeYP"></param>
        private void drawTextToOtherNode(Node node, Node parent, SystemVRF sysItem)
        {
            try
            {
                if (node == null) return;
                //if (node.Children != null)                    // To be Fix latter
                //{
                //    node.Children.Clear();
                //}

                //只有Piping验证通过了才显示YP CHBox等配件的型号。防止显示错误的型号。 add by Shen Junjie on 2018/2/1
                if (!sysItem.IsPipingOK) return;

                if (node is MyNodeYP)
                {
                    MyNodeYP nodeYP = node as MyNodeYP;
                    if (!string.IsNullOrEmpty(nodeYP.Model) && sysItem.IsPipingOK)
                    {
                        Caption label1 = new Caption(10, 10, 100, 30, nodeYP.Model, nodeYP, this.addFlowPiping);
                        initTextNode(label1, nodeYP.Model);
                        label1.Location = new Point(nodeYP.Location.X + 10, nodeYP.Location.Y - label1.Size.Height);
                        if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
                        {
                            if (sysItem.IsPipingVertical)
                            {
                                //二叉树垂直布局 YP的型号标注放到左边
                                label1.Location = new Point(nodeYP.Location.X - label1.Size.Width + 10, label1.Location.Y);
                            }
                        }
                        else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.SchemaA)
                        {
                            if (!sysItem.IsPipingVertical)
                            {
                                //SchemaA水平布局 
                                //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。YP的型号标注往右再移动5px
                                label1.Location = new Point(label1.Location.X + 3, label1.Location.Y);
                            }
                        }
                        addFlowPiping.Items.Add(label1);
                        //label1.Parent = nodeYP;                       // To be Fix latter
                        //在绘制的同时将型号添加到PipingKit缓存表，导出报表用 add on 20160727 by Yunxiao Lin
                        if (nodeYP.Model != "")
                        {
                            if (sysItem.IsExportToReport)
                                InsertPipingKitTable("BranchKit", sysItem.Name, nodeYP.Model, 1, sysItem.Id);
                        }
                    }
                }
                else if (node is MyNodeCH)
                {
                    MyNodeCH nodeCH = node as MyNodeCH;
                    if (!string.IsNullOrEmpty(nodeCH.Model))
                    {
                        //在绘制的同时将型号添加到PipingKit缓存表，导出报表用 add on 20160727 by Yunxiao Lin
                        if (sysItem.IsExportToReport)
                            InsertPipingKitTable("CHBox", sysItem.Name, nodeCH.Model, 1, sysItem.Id);
                        if (sysItem.IsPipingOK)
                        {
                            Caption label1 = new Caption(10, 10, 100, 30, nodeCH.Model, nodeCH, this.addFlowPiping);
                            initTextNode(label1, nodeCH.Model);
                            if (!sysItem.IsPipingVertical)
                                label1.Location = new Point(nodeCH.Location.X + nodeCH.Size.Width / 2 + 5, nodeCH.Location.Y - label1.Size.Height);
                            else
                                label1.Location = new Point(nodeCH.Location.X + nodeCH.Size.Width / 2 - label1.Size.Width / 2, nodeCH.Location.Y - label1.Size.Height);
                            //addFlowPiping.Items.Add(label1);
                            this.addFlowPiping.AddCaption(label1);
                            //label1.Parent = nodeCH;               // To be Fix latter
                        }
                    }
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                    if (!string.IsNullOrEmpty(nodeMCH.Model))
                    {
                        //在绘制的同时将型号添加到PipingKit缓存表，导出报表用 add on 20160727 by Yunxiao Lin
                        if (sysItem.IsExportToReport)
                            InsertPipingKitTable("CHBox", sysItem.Name, nodeMCH.Model, 1, sysItem.Id);
                        if (sysItem.IsPipingOK)
                        {
                            Caption label1 = new Caption(10, 10, 100, 30, nodeMCH.Model, nodeMCH, this.addFlowPiping);
                            initTextNode(label1, nodeMCH.Model);
                            if (sysItem.IsPipingVertical)
                            {
                                label1.Location = new Point(nodeMCH.Location.X + nodeMCH.Size.Width / 2 - label1.Size.Width / 2, nodeMCH.Location.Y - label1.Size.Height);
                            }
                            else
                            {
                                if (parent is MyNodeYP && parent.Location.Y > node.Location.Y)
                                {
                                    label1.Location = new Point(nodeMCH.Location.X + nodeMCH.Size.Width / 2 - label1.Size.Width / 2, nodeMCH.Location.Y - label1.Size.Height);
                                }
                                else
                                {
                                    label1.Location = new Point(nodeMCH.Location.X + nodeMCH.Size.Width / 2 + 5, nodeMCH.Location.Y - label1.Size.Height);
                                }
                            }
                            //addFlowPiping.Items.Add(label1);
                            this.addFlowPiping.AddCaption(label1);
                            // label1.Parent = nodeMCH;               // To be Fix latter
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //if (parent != null)
            //{
            //    drawTextToLink(parent, node, isInch, sysItem);
            //}
        }

        private void drawTextToOtherNode(Node node, Node parent, SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            try
            {
                if (node == null) return;
                //if (node.Children != null)                    // To be Fix latter
                //{
                //    node.Children.Clear();
                //}

                //只有Piping验证通过了才显示YP CHBox等配件的型号。防止显示错误的型号。 add by Shen Junjie on 2018/2/1
                if (!sysItem.IsPipingOK) return;

                if (node is MyNodeYP)
                {
                    MyNodeYP nodeYP = node as MyNodeYP;
                    if (!string.IsNullOrEmpty(nodeYP.Model) && sysItem.IsPipingOK)
                    {
                        Caption label1 = new Caption(10, 10, 100, 30, nodeYP.Model, nodeYP, addFlowPiping);
                        initTextNode(label1, nodeYP.Model);
                        label1.Location = new Point(nodeYP.Location.X + 10, nodeYP.Location.Y - label1.Size.Height);
                        if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
                        {
                            if (sysItem.IsPipingVertical)
                            {
                                //二叉树垂直布局 YP的型号标注放到左边
                                label1.Location = new Point(nodeYP.Location.X - label1.Size.Width + 10, label1.Location.Y);
                            }
                        }
                        else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.SchemaA)
                        {
                            if (!sysItem.IsPipingVertical)
                            {
                                //SchemaA水平布局 
                                //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。YP的型号标注往右再移动5px
                                label1.Location = new Point(label1.Location.X + 3, label1.Location.Y);
                            }
                        }

                        addFlowPiping.Items.Add(label1);
                        //label1.Parent = nodeYP;                       // To be Fix latter
                        //在绘制的同时将型号添加到PipingKit缓存表，导出报表用 add on 20160727 by Yunxiao Lin
                        if (nodeYP.Model != "")
                        {
                            if (sysItem.IsExportToReport)
                                InsertPipingKitTable("BranchKit", sysItem.Name, nodeYP.Model, 1, sysItem.Id);
                        }
                    }
                }
                else if (node is MyNodeCH)
                {
                    MyNodeCH nodeCH = node as MyNodeCH;
                    if (!string.IsNullOrEmpty(nodeCH.Model))
                    {
                        //在绘制的同时将型号添加到PipingKit缓存表，导出报表用 add on 20160727 by Yunxiao Lin
                        if (sysItem.IsExportToReport)
                            InsertPipingKitTable("CHBox", sysItem.Name, nodeCH.Model, 1, sysItem.Id);
                        if (sysItem.IsPipingOK)
                        {
                            Caption label1 = new Caption(10, 10, 100, 30, nodeCH.Model, nodeCH, this.addFlowPiping);
                            initTextNode(label1, nodeCH.Model);
                            if (!sysItem.IsPipingVertical)
                                label1.Location = new Point(nodeCH.Location.X + nodeCH.Size.Width / 2 + 5, nodeCH.Location.Y - label1.Size.Height);
                            else
                                label1.Location = new Point(nodeCH.Location.X + nodeCH.Size.Width / 2 - label1.Size.Width / 2, nodeCH.Location.Y - label1.Size.Height);
                            //addFlowPiping.Items.Add(label1);
                            this.addFlowPiping.AddCaption(label1);
                            //label1.Parent = nodeCH;               // To be Fix latter
                        }
                    }
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                    if (!string.IsNullOrEmpty(nodeMCH.Model))
                    {
                        //在绘制的同时将型号添加到PipingKit缓存表，导出报表用 add on 20160727 by Yunxiao Lin
                        if (sysItem.IsExportToReport)
                            InsertPipingKitTable("CHBox", sysItem.Name, nodeMCH.Model, 1, sysItem.Id);
                        if (sysItem.IsPipingOK)
                        {
                            Caption label1 = new Caption(10, 10, 100, 30, nodeMCH.Model, nodeMCH, this.addFlowPiping);
                            initTextNode(label1, nodeMCH.Model);
                            if (sysItem.IsPipingVertical)
                            {
                                label1.Location = new Point(nodeMCH.Location.X + nodeMCH.Size.Width / 2 - label1.Size.Width / 2, nodeMCH.Location.Y - label1.Size.Height);
                            }
                            else
                            {
                                if (parent is MyNodeYP && parent.Location.Y > node.Location.Y)
                                {
                                    label1.Location = new Point(nodeMCH.Location.X + nodeMCH.Size.Width / 2 - label1.Size.Width / 2, nodeMCH.Location.Y - label1.Size.Height);
                                }
                                else
                                {
                                    label1.Location = new Point(nodeMCH.Location.X + nodeMCH.Size.Width / 2 + 5, nodeMCH.Location.Y - label1.Size.Height);
                                }
                            }
                            //addFlowPiping.Items.Add(label1);
                            this.addFlowPiping.AddCaption(label1);
                            // label1.Parent = nodeMCH;               // To be Fix latter
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //if (parent != null)
            //{
            //    drawTextToLink(parent, node, isInch, sysItem);
            //}
        }
        public void DrawLegendText(SystemVRF sysItem, ref AddFlow addFlowPiping)
        {
            System.Windows.Size diagramSize = addFlowPiping.GetDiagramBounds().Size;
            legendLeft = (float)diagramSize.Width + legendLeftTopPosition;

            MyNodeLegend label1 = new MyNodeLegend();
            initTextNode(label1, "Piping Correction Factor(Cooling): " + sysItem.PipingLengthFactor.ToString("n3"));
            label1.Location = new Point(legendLeft, 15);
            label1.StrokeThickness = 0;
            label1.Foreground = System.Windows.Media.Brushes.Black;
            addFlowPiping.AddNode(label1);

            MyNodeLegend label2 = new MyNodeLegend();
            initTextNode(label2, "Piping Correction Factor(Heating): " + sysItem.PipingLengthFactor_H.ToString("n3"));
            label2.Location = new Point(legendLeft, 30);
            label2.StrokeThickness = 0;
            label2.Foreground = System.Windows.Media.Brushes.Black;
            addFlowPiping.AddNode(label2);

            //MyNodeLegend label3 = new MyNodeLegend();
            //initTextNodeLegend(label3, "Gas pipe : Red");
            //label3.Location = new Point(legendLeft, 60);
            //label3.StrokeThickness = 0;
            //label3.Foreground = System.Windows.Media.Brushes.Red;
            //addFlowPiping.AddNode(label3);

            //MyNodeLegend label4 = new MyNodeLegend();
            //initTextNodeLegend(label4, "Liquid pipe : Green");
            //label4.Location = new Point(legendLeft, 75);
            //label4.StrokeThickness = 0;
            //label4.Foreground = System.Windows.Media.Brushes.Green;
            //addFlowPiping.AddNode(label4);

            MyNodeLegend label5 = new MyNodeLegend();
            initTextNode(label5, "");
            label5.Location = new Point(legendLeft, 45);
            label5.StrokeThickness = 0;
            label5.Foreground = System.Windows.Media.Brushes.Black;
            label5.LegendType = "RefrigerationText";
            addFlowPiping.AddNode(label5);
        }
        void initTextNodeLegend(Node label, string text)
        {
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font textFont = new Font("Arial", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font ft = textFont;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 11;
            SizeF size = gMeasureString.MeasureString(text, ft);
            label.Text = text;
            label.Size = new System.Windows.Size(size.Width + 15, size.Height);
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            label.IsSelectable = false;
        }

        /// 为管线图添加加注冷媒标注 2016-12-22 by shen junjie
        /// <summary>
        /// 为管线图添加加注冷媒标注
        /// </summary>
        public void DrawAddRefrigerationText(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            if (addFlowPiping == null) return;

            string title = "Additional Refrigerant Charge";
            var AllLegendNode = addFlowPiping.Items.OfType<Node>().ToArray();
            if (AllLegendNode != null)
            {
                foreach (Node node in AllLegendNode)
                {
                    if (node.GetType().Name == "MyNodeLegend")
                    {
                        if (((MyNodeLegend)node).LegendType == "RefrigerationText")
                        {
                            string text = title + ": " + Unit.ConvertToControl(sysItem.OutdoorItem.RefrigerantCharge, UnitType.WEIGHT, ut_weight).ToString("n1") + ut_weight;
                            node.Text = text;
                            Font textFont = new Font("Arial", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
                            Font ft = textFont;
                            SizeF size = gMeasureString.MeasureString(text, ft);
                            node.Size = new System.Windows.Size(size.Width + 15, size.Height);

                        }
                    }
                }
            }


            //MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            //bool hasFound = false;

            //if (nodeOut.ChildNode != null)
            //{


            //for (int i = 0; i < nodeOut.ChildNode.Count; i++)
            //    {
            //        if (nodeOut.ChildNode[i].Tooltip == title)
            //        {
            //            label1 = nodeOut.Children[i];
            //            hasFound = true;
            //            break;
            //        }
            //    }
            /// }

            //if (!hasFound)
            //{
            //    label1 = new Node();
            //    label1.Tooltip = title;
            //}

            //string text = title + ": " + Unit.ConvertToControl(sysItem.AddRefrigeration, UnitType.WEIGHT, ut_weight).ToString("n1") + ut_weight;

            ////由于CAD对中文的支持不好，去掉CAD文字国际化
            //initTextNode(label1, text);

            //if (!hasFound)
            //{
            //    label1.Location = new PointF(legendLeft, 45);
            //    addFlowPiping.Nodes.Add(label1);
            //    label1.Parent = nodeOut;
            //}
        }

        /// 为管线图添加线型图例 2016-5-10 by Yunxiao Lin
        /// <summary>
        /// 为管线图添加线型图例
        /// </summary>
        public void drawPipelegend(bool isHR, ref AddFlow refAddFlowPiping)
        {
            this.addFlowPiping = refAddFlowPiping;
            if (isHR)
            {
                MyNodeLegend label1 = new MyNodeLegend();
                MyNodeLegend label2 = new MyNodeLegend();
                MyNodeLegend label3 = new MyNodeLegend();
                //MyNodeLegend label4 = new MyNodeLegend();
                //MyNodeLegend label5 = new MyNodeLegend();

                MyNodeLegend label6 = new MyNodeLegend();   //低压气管
                MyNodeLegend label7 = new MyNodeLegend();   //高压气管
                MyNodeLegend label8 = new MyNodeLegend();   //液管
                MyNodeLegend label9 = new MyNodeLegend();   //气管

                Node node11 = new Node(legendLeft, 65, 1, 1, "node11", addFlowPiping);
                Node node12 = new Node(legendLeft + 100, 65, 1, 1, "node12", addFlowPiping);
                Node node21 = new Node(legendLeft, 80, 1, 1, "node21", addFlowPiping);
                Node node22 = new Node(legendLeft + 100, 80, 1, 1, "node22", addFlowPiping);
                Node node31 = new Node(legendLeft, 95, 1, 1, "node31", addFlowPiping);
                Node node32 = new Node(legendLeft + 100, 95, 1, 1, "node32", addFlowPiping);

                node11.Stroke = utilPiping.colorTransparent;
                node12.Stroke = utilPiping.colorTransparent;
                node21.Stroke = utilPiping.colorTransparent;
                node22.Stroke = utilPiping.colorTransparent;
                node31.Stroke = utilPiping.colorTransparent;
                node32.Stroke = utilPiping.colorTransparent;

                Link link1 = new Link(node11, node12, "", addFlowPiping);
                Link link2 = new Link(node21, node22, "", addFlowPiping);
                Link link3 = new Link(node31, node32, "", addFlowPiping);


                //CAD对中文的支持有问题，去掉国际化，一律使用英文
                //initTextNode(label1, Msg.GetResourceString("LEGEND_TXT_3PIPE"));
                //initTextNode(label2, Msg.GetResourceString("LEGEND_TXT_2PIPE"));
                //initTextNode(label3, Msg.GetResourceString("LEGEND_TXT_2PIPE_LOW"));
                initTextNode(label1, "Low/High Pressure Gas Pipe & Liquid Pipe");
                initTextNode(label2, "Gas Pipe & Liquid Pipe");
                initTextNode(label3, "Low Pressure Gas Pipe & Liquid Pipe (Exclusive use of cooling operation)");
                //initTextNode(label4, "Diameter order of 3 pipe : Low Pressure Gas Pipe / High Pressure Gas Pipe / Liquide line");
                //initTextNode(label5, "Diameter order of 2 pipe : Gas pipe / Liquid pipe");
                link1.Stroke = utilPiping.color_piping_3pipe; ;
                link2.Stroke = utilPiping.color_piping_2pipe;
                link3.Stroke = utilPiping.color_piping_2pipe_lowgas;
                link1.StrokeThickness = 1;
                link2.StrokeThickness = 1;
                link3.StrokeThickness = 1;
                link1.DashStyle = DashStyles.Solid;
                link2.DashStyle = DashStyles.Solid;
                link3.DashStyle = DashStyles.Solid;
                //link1.EndCap = LineCap.NoAnchor;
                //link2.EndCap = LineCap.NoAnchor;
                //link3.EndCap = LineCap.NoAnchor;
                //link1.ZOrder = 10;
                //link2.ZOrder = 10;
                //link3.ZOrder = 10;
                link1.IsSelectable = false;
                link2.IsSelectable = false;
                link3.IsSelectable = false;

                //管径颜色提示   --add on 20180522 by Vince
                DrawPipeLableColor(label6, "Low Pressure Gas pipe : Light purple", null, 1, ref addFlowPiping);
                DrawPipeLableColor(label7, "High Pressure Gas pipe : Blue", null, 2, ref addFlowPiping);
                DrawPipeLableColor(label8, "Liquid pipe : Green", null, 3, ref addFlowPiping);
                DrawPipeLableColor(label9, "Gas pipe : Red", null, 4, ref addFlowPiping);

                label1.Location = new Point(legendLeft + 110, 60);
                label2.Location = new Point(legendLeft + 110, 75);
                label3.Location = new Point(legendLeft + 110, 90);
                //label4.Location = new Point(legendLeft, 105);
                //label5.Location = new Point(legendLeft, 125);
                label6.Location = new Point(legendLeft, 105);
                label7.Location = new Point(legendLeft, 120);
                label9.Location = new Point(legendLeft, 135);
                label8.Location = new Point(legendLeft, 150);

                addFlowPiping.AddNode(label1);
                addFlowPiping.AddNode(label2);
                addFlowPiping.AddNode(label3);
                //addFlowPiping.Nodes.Add(label4);
                //addFlowPiping.Nodes.Add(label5);

                addFlowPiping.Items.Add(node11);
                addFlowPiping.Items.Add(node12);
                addFlowPiping.Items.Add(node21);
                addFlowPiping.Items.Add(node22);
                addFlowPiping.Items.Add(node31);
                addFlowPiping.Items.Add(node32);


                addFlowPiping.AddLink(link1);
                addFlowPiping.AddLink(link2);
                addFlowPiping.AddLink(link3);
            }
            else
            {
                MyNodeLegend label8 = new MyNodeLegend();   //液管
                MyNodeLegend label9 = new MyNodeLegend();   //气管
                DrawPipeLableColor(label8, "Liquid pipe : Green", null, 3, ref addFlowPiping);//Todo later
                DrawPipeLableColor(label9, "Gas pipe : Red", null, 4, ref addFlowPiping);//Todo Later
                label9.Location = new Point(legendLeft, 60);
                label8.Location = new Point(legendLeft, 75);
            }
        }

        #endregion

        #endregion

        #region 批量设置所有连接管的属性
        /// 批量设置所有连接管的属性
        /// <summary>
        /// 批量设置所有连接管的属性
        /// </summary>
        /// <param name="elbowQty">弯头数量</param>
        /// <param name="oilTrapQty">油弯数量</param>
        /// <param name="length">连接管管长</param>
        /// <param name="valvelength">电子膨胀阀管长</param>
        public void SetAllLinks(double elbowQty, double oilTrapQty, double length, double valvelength, ref AddFlow addFlowPiping)
        {
            foreach (Node node in addFlowPiping.Items.OfType<Node>())
            {
                if (node.Links.Count > 0)
                {
                    MyNode myNode = node as MyNode;
                    if (myNode == null || myNode.Links==null)
                        continue;
                    foreach (Link lk in myNode.MyInLinks)
                    {
                        if (!(lk is MyLink)) continue; //添加非MyLink过滤 add on 20160511 by Yunxiao Lin

                        MyLink mylink = lk as MyLink;
                        mylink.ElbowQty = elbowQty;
                        mylink.OilTrapQty = oilTrapQty;
                        mylink.Length = length;
                        mylink.ValveLength = 0;
                        //增加电子膨胀阀管长设置 add on 20160616 by Yunxiao Lin
                        if (myNode is MyNodeIn)
                        {
                            if ((myNode as MyNodeIn).RoomIndooItem != null && (myNode as MyNodeIn).RoomIndooItem.IndoorItem != null)
                            {
                                OldModel.Indoor indoor = (myNode as MyNodeIn).RoomIndooItem.IndoorItem;
                                string outDoorModel = indoor.Model_Hitachi;
                                if (!string.IsNullOrEmpty(outDoorModel))
                                {
                                    //Regex reg = new Regex(@"RPK-(.*?)FSNH3M");
                                    Regex reg = new Regex(@"RPK-(.*?)(FSNH3M|FSNSH3|FSNH4)"); //HAPM IDU High Wall (w/o EXV)有三种model: FSNH3M & FSNSH3 & FSNH4 20180729 by Yunxiao Lin
                                    var mat = reg.Match(outDoorModel);
                                    if (mat.Success)
                                    {
                                        mylink.ValveLength = valvelength;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetPipeLength(double length, ref AddFlow addFlowPiping)
        {
            foreach (Node node in addFlowPiping.Items.OfType<Node>())
            {
                if (node.Links.Count > 0)
                {
                    MyNode myNode = node as MyNode;
                    if (myNode == null)
                        continue;
                    foreach (Link lk in myNode.MyInLinks)
                    {
                        if (!(lk is MyLink)) continue; //添加非MyLink过滤 add on 20160511 by Yunxiao Lin

                        MyLink mylink = lk as MyLink;
                        mylink.Length = length;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 清空PipingKit缓存表
        /// </summary>
        public void ClearPipingKitTable()
        {
            DataTable tb = JCBase.Utility.Util.DsCach.Tables["T_PipingKitTable"];
            if (tb != null)
            {
                tb.Rows.Clear();
            }
        }

        /// <summary>
        /// 向PipingKit缓存表插入一条数据
        /// </summary>
        /// <param name="KitType"></param>
        /// <param name="KitSys"></param>
        /// <param name="KitModel"></param>
        /// <param name="KitQty"></param>
        private void InsertPipingKitTable(string KitType, string KitSys, string KitModel, int KitQty, string SysId)
        {
            DataTable tb = JCBase.Utility.Util.DsCach.Tables["T_PipingKitTable"];
            if (tb != null)
            {
                DataRow row = tb.NewRow();
                row["Type"] = KitType;
                row["System"] = KitSys;
                row["Model"] = KitModel;
                row["Qty"] = KitQty;
                row["Id"] = SysId;
                tb.Rows.Add(row);
            }
        }

        #endregion

        #region 保存和读取配管图节点数据 add by Shen Junjie on 20170728

        /// <summary>
        /// 绘制配管图。直接绘制，不重新计算坐标，并且绘制连接线
        /// </summary>
        public void DrawPipingNodesNoCaculation(string dir, SystemVRF sysItem)
        {
            //if (sysItem == null) return;
            //MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            //tmpMyNodeOut tempOut = sysItem.MyPipingNodeOutTemp;

            //绘制所有节点（除室外机）的图片
            //DrawChildNodesImage(nodeOut, nodeOut.ChildNode, dir);

            //DrawPipingNodesNoCaculation(dir, sysItem, tempOut, nodeOut, null);
        }

        private void DrawPipingNodesNoCaculation(string dir, SystemVRF sysItem, tmpMyNode tempNode, Node node, Node parent)
        {
            if (node == null || tempNode == null) return;

            node.Location = new Point(tempNode.Location.X, tempNode.Location.Y);
            addFlowPiping.Items.Add(node);

            if (node is MyNodeOut && tempNode is tmpMyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                tmpMyNodeOut tempOut = tempNode as tmpMyNodeOut;

                nodeOut.Name = sysItem.Name;

                if (sysItem.OutdoorItem != null)
                {
                    nodeOut.Model = sysItem.OutdoorItem.AuxModelName;

                    NodeElement_Piping outNodeItem = GetPipingNodeOutElement(sysItem, isHitachi);
                    nodeOut.UnitCount = outNodeItem.UnitCount; //室外机组合中机组数目  add by Shen Junjie on 20170827

                    string imgFile = dir + outNodeItem.Name + ".png";

                    //drawNodeImage(nodeOut, imgFile, ref addFlowPiping);
                    //drawTextToODUNode(nodeOut, outNodeItem, sysItem, ref addFlowPiping);
                }

                DrawPipingNodesNoCaculation(dir, sysItem, tempOut.ChildNode, nodeOut.ChildNode, nodeOut);
            }
            else if (node is MyNodeYP && tempNode is tmpMyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                tmpMyNodeYP tempYP = tempNode as tmpMyNodeYP;

                drawTextToOtherNode(nodeYP, parent, sysItem);

                for (int i = 0; i < nodeYP.MaxCount; i++)
                {
                    DrawPipingNodesNoCaculation(dir, sysItem, tempYP.ChildNodes[i], nodeYP.ChildNodes[i], nodeYP);
                }
            }
            else if (node is MyNodeCH && tempNode is tmpMyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                tmpMyNodeCH tempCH = tempNode as tmpMyNodeCH;

                drawTextToOtherNode(nodeCH, parent, sysItem);

                DrawPipingNodesNoCaculation(dir, sysItem, tempCH.ChildNode, nodeCH.ChildNode, node);
            }
            else if (node is MyNodeMultiCH && tempNode is tmpMyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                tmpMyNodeMultiCH tempMCH = tempNode as tmpMyNodeMultiCH;

                drawTextToOtherNode(nodeMCH, parent, sysItem);

                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    DrawPipingNodesNoCaculation(dir, sysItem, tempMCH.ChildNodes[i], nodeMCH.ChildNodes[i], nodeMCH);
                }
            }
            else if (node is MyNodeIn && tempNode is tmpMyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                tmpMyNodeIn tempIn = tempNode as tmpMyNodeIn;

                //drawTextToIDUNode(sysItem, nodeIn);
            }

            // node.Fill = tempNode.FillColor;   //// To be Fix latter

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                if (tempNode.MyInLinks != null && myNode.MyInLinks != null)
                {
                    for (int i = 0; i < tempNode.MyInLinks.Length && i < myNode.MyInLinks.Count; i++)
                    {
                        tmpMyLink tempLink = tempNode.MyInLinks[i];
                        MyLink myLink = myNode.MyInLinks[i];
                        string LinkName = myLink.Text;
                        myLink.LineStyle = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
                        if (myLink.AddFlow == null)
                        {
                            Link link = new Link(parent, node, LinkName, addFlowPiping);
                            addFlowPiping.AddLink(link);
                        }
                        LoadLinkStyle(myLink, tempLink);

                        //drawTextToLink(myLink, i, parent, node, isInch, sysItem);
                    }
                }
            }
        }

        public DataTable GetPipingKitTable()
        {
            DataTable tb = new DataTable();
            tb.Columns.Add("Type");
            tb.Columns.Add("System");
            tb.Columns.Add("Model");
            tb.Columns.Add("Qty");
            tb.Columns.Add("SystemId");

            foreach (SystemVRF sysItem in thisProject.SystemListNextGen)
            {
                if (!sysItem.IsExportToReport) continue;

                tmpMyNodeOut nodeOut = sysItem.MyPipingNodeOutTemp;
                NodeElement_Piping outNodeItem = GetPipingNodeOutElement(sysItem, isHitachi);
                if (outNodeItem != null && outNodeItem.ConnectionKitModel != null)
                {
                    for (int i = 0; i < outNodeItem.ConnectionKitModel.Length; ++i)
                    {
                        string model = outNodeItem.ConnectionKitModel[i];
                        InsertPipingKitTable(tb, "PipingConnectionKit", sysItem.Name, model, 1, sysItem.Id);
                    }
                }

                EachNode(nodeOut as tmpMyNode, (node) =>
                {
                    if (node is tmpMyNodeYP)
                    {
                        tmpMyNodeYP nodeYP = node as tmpMyNodeYP;
                        InsertPipingKitTable(tb, "BranchKit", sysItem.Name, nodeYP.Model, 1, sysItem.Id);
                    }
                    else if (node is tmpMyNodeCH)
                    {
                        tmpMyNodeCH nodeCH = node as tmpMyNodeCH;
                        InsertPipingKitTable(tb, "CHBox", sysItem.Name, nodeCH.Model, 1, sysItem.Id);
                    }
                    else if (node is tmpMyNodeMultiCH)
                    {
                        tmpMyNodeMultiCH nodeMCH = node as tmpMyNodeMultiCH;
                        InsertPipingKitTable(tb, "CHBox", sysItem.Name, nodeMCH.Model, 1, sysItem.Id);
                    }
                    return false;
                }, (a, b) => false);
            }

            return tb;
        }

        #region add on 20130815 clh 解决AddFlow的节点不能序列化的问题

        /// 通过自定义的可序列化的临时对象,构造当前系统的配管结构对象 add by Shen Junjie on 20170801
        /// <summary>
        /// 通过自定义的可序列化的临时对象,构造当前系统的配管结构对象 
        /// </summary>
        public void LoadPipingNodeStructure(SystemVRF sysItem)
        {
            if (sysItem.MyPipingNodeOutTemp == null) return;

            if (sysItem.IsManualPiping)
            {
                //utilPiping.colorDefault = sysItem.MyPipingNodeOutTemp.PipeColor;
                //utilPiping.colorText = sysItem.MyPipingNodeOutTemp.TextColor;
                //utilPiping.colorYP = sysItem.MyPipingNodeOutTemp.BranchKitColor;
                //utilPiping.colorNodeBg = sysItem.MyPipingNodeOutTemp.NodeBgColor;
            }
            else
            {
                utilPiping.ResetColors();
            }
            LoadPipingNodeRecursively(sysItem, sysItem.MyPipingNodeOutTemp, null);
        }

        /// 通过自定义的可序列化的临时对象还原节点（递归）add by Shen Junjie on 20170801
        /// <summary>
        /// 通过自定义的可序列化的临时对象还原节点，并添加到控件
        /// </summary>
        private MyNode LoadPipingNodeRecursively(SystemVRF sysItem, tmpMyNode tempNode, Node parent)
        {
            if (tempNode == null)
            {
                return null;
            }

            MyNode node = null;
            tmpMyLink tempLink = null;
            if (tempNode is tmpMyNodeOut)
            {
                tmpMyNodeOut temp = tempNode as tmpMyNodeOut;
                MyNodeOut nodeOut = utilPiping.createNodeOut();
                nodeOut.Model = sysItem.OutdoorItem?.AuxModelName;
                nodeOut.Name = sysItem.Name;
                sysItem.MyPipingNodeOut = nodeOut;

                nodeOut.PipeLengthes = temp.PipeLengthes;
                nodeOut.Location = temp.Location;
                nodeOut.ChildNode = LoadPipingNodeRecursively(sysItem, temp.ChildNode, nodeOut);

                node = nodeOut;
            }
            else if (tempNode is tmpMyNodeYP)
            {
                tmpMyNodeYP temp = tempNode as tmpMyNodeYP;
                MyNodeYP nodeYP = utilPiping.createNodeYP(temp.IsCP);
                if (temp.IsCP)
                {
                    nodeYP = temp.MaxCount == 4
                        ? utilPiping.createNodeYP4BranchKit()
                        : utilPiping.createNodeYP(temp.IsCP);
                }
                else
                {
                    string imageName = "YBranch_Seperator";
                    string sourceDir = GetcompleteImagepath(imageName);
                    var svgFilePath = sourceDir.Replace(".png", ".svg");

                    GetGeomatryFromSvg(svgFilePath, nodeYP);
                }

                LoadNodeYPValue(nodeYP, temp);

                tempLink = temp.InLink;
                nodeYP.Location = temp.Location;
                for (int i = 0; i < temp.ChildCount; i++)
                {
                    nodeYP.AddChildNode(LoadPipingNodeRecursively(sysItem, temp.ChildNodes[i], nodeYP));
                }

                node = nodeYP;
            }
            else if (tempNode is tmpMyNodeCH)
            {
                tmpMyNodeCH temp = tempNode as tmpMyNodeCH;
                MyNodeCH nodeCH = utilPiping.createNodeCHbox(null);

                LoadMyNodeCHValue(temp, nodeCH);

                tempLink = temp.InLink;
                nodeCH.Location = temp.Location;
                nodeCH.ChildNode = LoadPipingNodeRecursively(sysItem, temp.ChildNode, nodeCH);

                node = nodeCH;
            }
            else if (tempNode is tmpMyNodeMultiCH)
            {
                tmpMyNodeMultiCH temp = tempNode as tmpMyNodeMultiCH;
                MyNodeMultiCH nodeMCH = utilPiping.createNodeMultiCHbox();

                LoadMyNodeMultiCHValue(temp, nodeMCH);

                tempLink = temp.InLink;
                nodeMCH.Location = temp.Location;
                for (int i = 0; i < temp.ChildNodes.Count; i++)
                {
                    nodeMCH.AddChildNode(LoadPipingNodeRecursively(sysItem, temp.ChildNodes[i], nodeMCH));
                }

                node = nodeMCH;
            }
            else if (tempNode is tmpMyNodeIn)
            {
                tmpMyNodeIn temp = tempNode as tmpMyNodeIn;
                if (temp.RoomIndoorItem != null)
                {
                    OldModel.RoomIndoor riItem = thisProject.RoomIndoorList.Find(ind => ind.IndoorNO == temp.RoomIndoorItem.IndoorNO);

                    MyNodeIn nodeIn = utilPiping.createNodeIn(riItem);

                    tempLink = temp.InLink;
                    nodeIn.Location = temp.Location;
                    node = nodeIn;
                }
            }

            //兼容老项目数据 add by Shen Junjie on 2018/01/09
            if (tempLink != null)
            {
                LoadLinkValue(node.MyInLinks[0], tempLink);
            }

            if (tempNode.MyInLinks != null && tempNode.MyInLinks.Length > 0)
            {
                //Start Backward Compatibility : Added a null check to avoid Null reference exception.
                if (node == null)
                {
                    node = new MyNode();
                }
                //End Backward Compatibility : Added a null check to avoid Null reference exception.

                node.MyInLinks.Clear();
                for (int i = 0; i < tempNode.MyInLinks.Length; i++)
                {
                    tmpMyLink templink = tempNode.MyInLinks[i];

                    MyLink inlink = utilPiping.createMyLink();
                    LoadLinkValue(inlink, templink);
                    for (int j = 0; j < templink.Points.Count; j++)
                    {
                        inlink.Points.Insert(j, templink.Points[j]);
                    }
                    inlink.LineStyle = templink.Style;
                    //inlink.PinOriginIndex = templink.PinOriginIndex;
                    //inlink.PinDestinationIndex = templink.PinDestinationIndex;
                    node.MyInLinks.Add(inlink);
                }
            }

            return node;
        }

        /// 加载 NodeYP 对象中绑定的数据
        /// <summary>
        /// 加载 NodeYP 对象中绑定的数据
        /// </summary>
        /// <param name="yp"></param>
        /// <param name="tmpNodeYP"></param>
        private void LoadNodeYPValue(MyNodeYP yp, tmpMyNodeYP tmpNodeYP)
        {
            yp.CoolingCapacity = tmpNodeYP.CoolingCapacity;
            yp.HeatingCapacity = tmpNodeYP.HeatingCapacity;
            yp.Model = tmpNodeYP.Model;
            yp.PriceG = tmpNodeYP.PriceG;
            //add on 20160429 YunxiaoLin.
            yp.IsCoolingonly = tmpNodeYP.IsCoolingonly;
        }

        /// 加载 Link 对象绑定的数据
        /// <summary>
        /// 加载 Link 对象绑定的数据
        /// </summary>
        /// <param name="myLink"></param>
        /// <param name="tmpLink"></param>
        private void LoadLinkValue(MyLink myLink, tmpMyLink tmpLink)
        {
            myLink.ElbowQty = tmpLink.ElbowQty;
            myLink.OilTrapQty = tmpLink.OilTrapQty;
            myLink.Length = tmpLink.Length;

            myLink.SpecG_h = tmpLink.SpecG_h;
            myLink.SpecG_l = tmpLink.SpecG_l;
            myLink.SpecL = tmpLink.SpecL;
            myLink.ValveLength = tmpLink.ValveLength;
            myLink.SpecG_h_Normal = tmpLink.SpecG_h_Normal;
            myLink.SpecG_l_Normal = tmpLink.SpecG_l_Normal;
            myLink.SpecL_Normal = tmpLink.SpecL_Normal;
        }

        private void LoadLinkStyle(MyLink myLink, tmpMyLink tmpLink)
        {
            if (tmpLink == null || tmpLink.Points == null) return;

            myLink.Points[0] = tmpLink.Points[0];
            myLink.Points[myLink.Points.Count - 1] = tmpLink.Points[tmpLink.Points.Count - 1];
            myLink.LineStyle = tmpLink.Style;//tmpLink.LineStyle;
            for (int i = 1; i < tmpLink.Points.Count - 1; i++)
            {
                if (myLink.Points.Count < tmpLink.Points.Count)
                {
                    myLink.Points.Add(tmpLink.Points[i]);
                }
                else
                {
                    myLink.Points[i] = tmpLink.Points[i];
                }
            }
            // myLink.Stroke = tmpLink.Stroke;      // To be Fix latter
        }

        /// <summary>
        /// 将tmpMyNodeCH的属性复制到MyNodeCH
        /// </summary>
        /// <param name="tmpch"></param>
        /// <param name="ch"></param>
        private void LoadMyNodeCHValue(tmpMyNodeCH tmpch, MyNodeCH ch)
        {
            ch.CoolingCapacity = tmpch.CoolingCapacity;
            ch.HeatingCapacity = tmpch.HeatingCapacity;
            ch.Model = tmpch.Model;
            ch.MaxTotalCHIndoorPipeLength = tmpch.MaxTotalCHIndoorPipeLength;
            ch.MaxTotalCHIndoorPipeLength_MaxIU = tmpch.MaxTotalCHIndoorPipeLength_MaxIU;
            ch.MaxIndoorCount = tmpch.MaxIndoorCount;
            ch.ActualTotalCHIndoorPipeLength = tmpch.ActualTotalCHIndoorPipeLength;
            ch.PowerSupply = tmpch.PowerSupply;//add by Shen Junjie on 2017/12/22
            ch.PowerLineType = tmpch.PowerLineType;//add by Shen Junjie on 2017/12/22 
            ch.HeightDiff = tmpch.HeightDiff; //add by xyj on 20180620 
            ch.PowerCurrent = tmpch.PowerCurrent;//add by Shen Junjie on 2018/6/15
            ch.PowerConsumption = tmpch.PowerConsumption;//add by Shen Junjie on 2018/6/15 
        }


        /// <summary>
        /// 将tmpMyNodeMultiCH的属性复制到MyNodeMultiCH
        /// </summary>
        /// <param name="tmpch"></param>
        /// <param name="ch"></param>
        private void LoadMyNodeMultiCHValue(tmpMyNodeMultiCH tmpmch, MyNodeMultiCH mch)
        {
            mch.CoolingCapacity = tmpmch.CoolingCapacity;
            mch.HeatingCapacity = tmpmch.HeatingCapacity;
            mch.Model = tmpmch.Model;
            mch.MaxTotalCHIndoorPipeLength = tmpmch.MaxTotalCHIndoorPipeLength;
            mch.MaxTotalCHIndoorPipeLength_MaxIU = tmpmch.MaxTotalCHIndoorPipeLength_MaxIU;
            mch.MaxIndoorCount = tmpmch.MaxIndoorCount;
            mch.ActualTotalCHIndoorPipeLength = tmpmch.ActualTotalCHIndoorPipeLength;

            mch.PowerSupply = tmpmch.PowerSupply;
            mch.PowerConsumption = tmpmch.PowerConsumption;
            mch.PowerCurrent = tmpmch.PowerCurrent;
            mch.MaxBranches = tmpmch.MaxBranches;
            mch.MaxCapacityPerBranch = tmpmch.MaxCapacityPerBranch;
            mch.MaxIUPerBranch = tmpmch.MaxIUPerBranch;
            mch.PowerLineType = tmpmch.PowerLineType;
            mch.HeightDiff = tmpmch.HeightDiff; //add by xyj on 20180620
        }

        #endregion

        /// 在保存系统项目之前将每个系统的配管图结构转化为自定义的可序列化的对象进行保存
        /// <summary>
        /// 在保存系统项目之前将每个系统的配管图结构转化为自定义的可序列化的对象进行保存
        /// 支持节点拖动，并保存节点位置 add by Shen Junjie on 20170801
        /// </summary>
        public void SavePipingStructure(SystemVRF sysItem)
        {
            if (sysItem.MyPipingNodeOut == null) return;

            Dictionary<Node, tmpMyNode> mapping = new Dictionary<Node, tmpMyNode>();
            CreateTempNodeRecursively(sysItem, sysItem.MyPipingNodeOut);
        }

        public void SavePipingOrphanNodes(SystemVRF sysItem)
        {
            if (sysItem.MyPipingOrphanNodes != null)
            {
                sysItem.MyPipingOrphanNodesTemp = new List<tmpMyNodeIn>();
                foreach (var sysItemMyPipingOrphanNode in sysItem.MyPipingOrphanNodes)
                {
                    MyNodeIn nodeIn = sysItemMyPipingOrphanNode as MyNodeIn;
                    if (nodeIn != null)
                    {
                        OldModel.RoomIndoor riItem = nodeIn.RoomIndooItem;
                        tmpMyNodeIn tempIn = new tmpMyNodeIn(riItem);
                        tempIn.Location = sysItemMyPipingOrphanNode.Location;
                        sysItem.MyPipingOrphanNodesTemp.Add(tempIn);
                    }
                }
            }
        }
        public void LoadPipingOrphanNodes(SystemVRF sysItem)
        {
            if (sysItem.MyPipingOrphanNodesTemp != null)
            {
                sysItem.MyPipingOrphanNodes = new List<MyNodeIn>();
                foreach (var sysItemMyPipingOrphanNode in sysItem.MyPipingOrphanNodesTemp)
                {
                    var tempNode = new MyNodeIn();
                    tempNode.Location = sysItemMyPipingOrphanNode.Location;
                    tempNode.RoomIndooItem = sysItemMyPipingOrphanNode.RoomIndoorItem;
                    sysItem.MyPipingOrphanNodes.Add(tempNode);
                }
            }
        }


        /// <summary>
        /// 保存每个系统项目的配管图结构
        /// </summary>
        public void SaveAllPipingStructure()
        {
            foreach (SystemVRF sysItem in thisProject.SystemListNextGen)
            {
                if (!sysItem.IsManualPiping)
                {
                    Dictionary<Node, tmpMyNode> mapping = new Dictionary<Node, tmpMyNode>();
                    CreateTempNodeRecursively(sysItem, sysItem.MyPipingNodeOut);
                }
                else
                {
                    //手工Piping的图只更新属性值                   
                    UpdateTempNodeRecursively(sysItem, sysItem.MyPipingNodeOutTemp, sysItem.MyPipingNodeOut);
                    SavePipingOrphanNodes(sysItem);
                }
            }
        }

        /// <summary>
        /// 保存比例尺节点
        /// </summary>
        /// <param name="sysItem"></param>
        /// <param name="imageNode"></param>
        public void SavePipingPlottingScaleNode(SystemVRF sysItem, MyNodePlottingScale plottingNode)
        {
            tmpNodePlottingScale tmpNode = null;
            if (plottingNode != null)
            {
                tmpNode = sysItem.MyPipingPlottingScaleNodeTemp;
                if (tmpNode == null)
                {
                    tmpNode = new tmpNodePlottingScale();
                }
                tmpNode.ActualLength = plottingNode.ActualLength;
                tmpNode.IsVertical = plottingNode.IsVertical;

                tmpNode.Location = plottingNode.Location;
                tmpNode.Size = new SizeF((float)plottingNode.Size.Width, (float)plottingNode.Size.Height);
                //tmpNode.Stroke = plottingNode.Stroke;
                tmpNode.FillColor = System.Drawing.Color.FromArgb(plottingNode.PlottingScaleColor.Color.A, plottingNode.PlottingScaleColor.Color.R,
                    plottingNode.PlottingScaleColor.Color.G, plottingNode.PlottingScaleColor.Color.B);
                //tmpNode.TextColor = plottingNode.TextColor;
            }
            sysItem.MyPipingPlottingScaleNodeTemp = tmpNode;
        }
        public void LoadPipingPlottingScaleNode(SystemVRF sysItem, ref AddFlow addFlow)
        {
            tmpNodePlottingScale tmpNodePlottingScale = sysItem.MyPipingPlottingScaleNodeTemp;
            if (tmpNodePlottingScale != null)
            {
                var plottingScale = new MyNodePlottingScale();
                plottingScale.ActualLength = tmpNodePlottingScale.ActualLength;
                plottingScale.IsVertical = tmpNodePlottingScale.IsVertical;
                plottingScale.Location = tmpNodePlottingScale.Location;
                plottingScale.Size = new System.Windows.Size(tmpNodePlottingScale.Size.Width, tmpNodePlottingScale.Size.Height);
                plottingScale.PlottingScaleColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(tmpNodePlottingScale.FillColor.A,
                    tmpNodePlottingScale.FillColor.R,
                    tmpNodePlottingScale.FillColor.G,
                    tmpNodePlottingScale.FillColor.B));
                DrawPlottingScaleNode(sysItem, plottingScale, ref addFlow);
                addFlow.AddNode(plottingScale);
            }
        }

        /// <summary>
        /// 保存背景图片节点
        /// </summary>
        /// <param name="sysItem"></param>
        /// <param name="imageNode"></param>
        public void SavePipingBuildingImageNode(SystemVRF sysItem, Node imageNode, Image img)
        {
            if (imageNode != null && img != null)
            {
                tmpNodeBgImage tmpNode = sysItem.MyPipingBuildingImageNodeTemp;
                if (tmpNode == null)
                {
                    tmpNode = new tmpNodeBgImage();
                    sysItem.MyPipingBuildingImageNodeTemp = tmpNode;
                }
                //tmpNode.Image = img;   // // To be Fix latter
                tmpNode.Location = imageNode.Location;
                //tmpNode.Size = imageNode.Size;   // // To be Fix latter
                SaveBuildingImageCache(tmpNode);
            }
            else
            {
                sysItem.MyPipingBuildingImageNodeTemp = null;
            }
        }

        /// <summary>
        /// 新建或覆盖户型图临时文件（保存项目时，这些文件会被读取成二进制流附加到项目对象）
        /// </summary>
        /// <param name="buildingImageNode"></param>
        public static void SaveBuildingImageCache(tmpNodeBgImage buildingImageNode)
        {
            //    System.Windows.Controls.Image img = null;
            //    if (buildingImageNode.Image != null)
            //    {
            //        img = buildingImageNode.Image;
            //        buildingImageNode.Image = null;
            //    }
            //    if (buildingImageNode.ImageStream != null)
            //    {
            //        img = Image.FromStream(buildingImageNode.ImageStream);                    // To be Fix latter
            //        buildingImageNode.ImageStream = null;
            //    }
            //    if (img == null) return;

            //    //先保存到临时文件，保存项目时保存到ImageStream
            //    //string name = buildingImageNode.ImageCacheId;
            //    //if (string.IsNullOrEmpty(name))
            //    //{
            //    //    name = new Guid().ToString();
            //    //    name = dir + "/" + name + ".jpg";
            //    //}
            //    //else
            //    //{
            //    //    dir = Path.GetDirectoryName(name);
            //    //}
            //    //try
            //    //{
            //    //    if (!Directory.Exists(dir))
            //    //    {
            //    //        Directory.CreateDirectory(dir);
            //    //    }
            //    //    img.Save(name, ImageFormat.Jpeg);
            //    //    buildingImageNode.ImageCacheId = name;
            //    //}
            //    //catch
            //    //{
            //    //    buildingImageNode.ImageCacheId = null;
            //    //}

            //    string id = buildingImageNode.ImageCacheId;
            //    if (string.IsNullOrEmpty(id) || !BuildingImageCache.ContainsKey(id) || img != BuildingImageCache[id])
            //    {
            //        id = Guid.NewGuid().ToString();
            //        BuildingImageCache[id] = img;
            //        buildingImageNode.ImageCacheId = id;
            //    }
        }

        /// <summary>
        /// 保存项目时，读取户型图临时文件并附加到OldModel.Project对象
        /// </summary>
        /// <param name="buildingImageNode"></param>
        /// <param name="dir"></param>
        public static void AttachAllBuildingImage(OldModel.Project thisProject)
        {
            if (thisProject != null && thisProject.SystemListNextGen != null)
            {
                foreach (var sys in thisProject.SystemListNextGen)
                {
                    tmpNodeBgImage buildingImageNode = sys.MyPipingBuildingImageNodeTemp;
                    if (buildingImageNode != null && !string.IsNullOrEmpty(buildingImageNode.ImageCacheId))
                    {
                        try
                        {
                            MemoryStream ms = new MemoryStream();
                            //Image img = Image.FromFile(buildingImageNode.ImageCacheId);
                            Image img = BuildingImageCache[buildingImageNode.ImageCacheId];
                            img.Save(ms, ImageFormat.Jpeg);
                            buildingImageNode.ImageStream = ms;
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// 保存项目时，读取户型图临时文件并附加到OldModel.Project对象
        /// </summary>
        /// <param name="buildingImageNode"></param>
        /// <param name="dir"></param>
        public static void SeparateAllBuildingImage(OldModel.Project thisProject, bool needCache)
        {
            if (needCache)
            {
                BuildingImageCache.Clear();
            }
            if (thisProject != null && thisProject.SystemListNextGen != null)
            {
                foreach (var sys in thisProject.SystemListNextGen)
                {
                    tmpNodeBgImage buildingImageNode = sys.MyPipingBuildingImageNodeTemp;
                    if (buildingImageNode != null)
                    {
                        if (needCache)
                        {
                            SaveBuildingImageCache(buildingImageNode);
                        }
                        else
                        {
                            buildingImageNode.ImageStream = null;
                            //buildingImageNode.Image = null;   Implementaion
                        }
                    }
                }
            }
        }

        private void UpdateTempNodeRecursively(SystemVRF sysItem, tmpMyNode tempNode, Node node)
        {
            if (node == null || tempNode == null) return;

            if (node is MyNodeOut && tempNode is tmpMyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                tmpMyNodeOut tempOut = tempNode as tmpMyNodeOut;
                tempOut.Location = nodeOut.Location;
                tempOut.PipeLengthes = nodeOut.PipeLengthes;

                UpdateTempNodeRecursively(sysItem, tempOut.ChildNode, nodeOut.ChildNode);
            }
            else if (node is MyNodeYP && tempNode is tmpMyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                tmpMyNodeYP tempYP = tempNode as tmpMyNodeYP;

                SaveNodeYPValue(tempYP, nodeYP);
                tempYP.Location = nodeYP.Location;
                for (int i = 0; i < nodeYP.MaxCount; i++)
                {
                    UpdateTempNodeRecursively(sysItem, tempYP.ChildNodes[i], nodeYP.ChildNodes[i]);
                }
            }
            else if (node is MyNodeCH && tempNode is tmpMyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                tmpMyNodeCH tempCH = tempNode as tmpMyNodeCH;
                tempCH.Location = nodeCH.Location;
                SaveNodeCHValue(nodeCH, tempCH);

                UpdateTempNodeRecursively(sysItem, tempCH.ChildNode, nodeCH.ChildNode);
            }
            else if (node is MyNodeMultiCH && tempNode is tmpMyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                tmpMyNodeMultiCH tempMCH = tempNode as tmpMyNodeMultiCH;
                tempMCH.Location = nodeMCH.Location;
                SaveNodeMultiCHValue(nodeMCH, tempMCH);

                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    UpdateTempNodeRecursively(sysItem, tempMCH.ChildNodes[i], nodeMCH.ChildNodes[i]);
                }
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                tmpMyNodeIn tempIn = tempNode as tmpMyNodeIn;
                tempIn.Location = nodeIn.Location;
            }

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                if (tempNode.MyInLinks != null && myNode.MyInLinks.Count <= tempNode.MyInLinks.Length)//// To be Fix latter
                {
                    for (int i = 0; i < myNode.MyInLinks.Count; i++)//// To be Fix latter
                    {
                        SaveLinkValue(tempNode.MyInLinks[i], myNode.MyInLinks[i]);//poc // To be Fix latter
                    }
                }
            }
        }

        private tmpMyNode CreateTempNodeRecursively(SystemVRF sysItem, Node node)
        {
            if (node == null) return null;

            tmpMyNode tempNode = null;
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                tmpMyNodeOut tempOut = new tmpMyNodeOut();

                tempOut.Location = nodeOut.Location;
                tempOut.PipeLengthes = nodeOut.PipeLengthes;

                sysItem.MyPipingNodeOutTemp = tempOut;

                tempOut.ChildNode = CreateTempNodeRecursively(sysItem, nodeOut.ChildNode);

                tempNode = tempOut;
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                tmpMyNodeYP tempYP = null;
                if (!nodeYP.IsCP)
                {
                    tempYP = new tmpMyNodeYP(nodeYP.IsCP);
                }
                else
                {
                    tempYP = new tmpMyNodeYP(nodeYP.MaxCount);
                }
                tempYP.Location = nodeYP.Location;

                SaveNodeYPValue(tempYP, nodeYP);

                for (int i = 0; i < nodeYP.MaxCount; i++)
                {
                    tempYP.AddChildNode(CreateTempNodeRecursively(sysItem, nodeYP.ChildNodes[i]));
                }
                tempNode = tempYP;
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                tmpMyNodeCH tempCH = new tmpMyNodeCH(null);
                tempCH.Location = nodeCH.Location;

                SaveNodeCHValue(nodeCH, tempCH);

                tempCH.ChildNode = CreateTempNodeRecursively(sysItem, nodeCH.ChildNode);
                tempNode = tempCH;
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                tmpMyNodeMultiCH tempMCH = new tmpMyNodeMultiCH();
                tempMCH.Location = nodeMCH.Location;

                SaveNodeMultiCHValue(nodeMCH, tempMCH);

                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    tempMCH.ChildNodes.Add(CreateTempNodeRecursively(sysItem, nodeMCH.ChildNodes[i]));
                }
                tempNode = tempMCH;
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                OldModel.RoomIndoor riItem = nodeIn.RoomIndooItem;
                tmpMyNodeIn tempIn = new tmpMyNodeIn(riItem);
                tempIn.Location = new Point(nodeIn.Location.X, nodeIn.Location.Y);

                tempNode = tempIn;
            }

            if (tempNode == null) return null;

            //tempNode.TextColor = node.Foreground;
            //tempNode.FillColor = node.FillColor;    // // To be Fix latter
            tempNode.Location = node.Location;

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                int inlinkCount = myNode.MyInLinks.Count;
                tempNode.MyInLinks = new tmpMyLink[inlinkCount];
                for (int i = 0; i < inlinkCount; i++)
                {
                    tmpMyLink tmpLink = new tmpMyLink();
                    SaveLinkValue(tmpLink, myNode.MyInLinks[i]);
                    tempNode.MyInLinks[i] = tmpLink;
                }
            }

            return tempNode;
        }

        /// <summary>
        /// 保存 NodeYP 对象中绑定的数据
        /// </summary>
        /// <param name="tmpNodeYP"></param>
        /// <param name="yp"></param>
        private void SaveNodeYPValue(tmpMyNodeYP tmpNodeYP, MyNodeYP yp)
        {
            tmpNodeYP.CoolingCapacity = yp.CoolingCapacity;
            tmpNodeYP.HeatingCapacity = yp.HeatingCapacity;
            tmpNodeYP.Model = yp.Model;
            tmpNodeYP.PriceG = yp.PriceG;
            //add on 20160429 by Yunxiao Lin.
            tmpNodeYP.IsCoolingonly = yp.IsCoolingonly;
        }

        private void SaveNodeCHValue(MyNodeCH ch, tmpMyNodeCH tmpch)
        {
            tmpch.CoolingCapacity = ch.CoolingCapacity;
            tmpch.HeatingCapacity = ch.HeatingCapacity;
            tmpch.Model = ch.Model;
            tmpch.MaxTotalCHIndoorPipeLength = ch.MaxTotalCHIndoorPipeLength;
            tmpch.MaxTotalCHIndoorPipeLength_MaxIU = ch.MaxTotalCHIndoorPipeLength_MaxIU;
            tmpch.MaxIndoorCount = ch.MaxIndoorCount;
            tmpch.ActualTotalCHIndoorPipeLength = ch.ActualTotalCHIndoorPipeLength;
            tmpch.PowerSupply = ch.PowerSupply;//add by Shen Junjie on 2017/12/22
            tmpch.PowerLineType = ch.PowerLineType;//add by Shen Junjie on 2017/12/22 
            tmpch.HeightDiff = ch.HeightDiff; //add by xyj on 20180620 
            tmpch.PowerCurrent = ch.PowerCurrent;//add by Shen Junjie on 2018/6/15
            tmpch.PowerConsumption = ch.PowerConsumption;//add by Shen Junjie on 2018/6/15 
        }

        private void SaveNodeMultiCHValue(MyNodeMultiCH mch, tmpMyNodeMultiCH tmpmch)
        {
            tmpmch.CoolingCapacity = mch.CoolingCapacity;
            tmpmch.HeatingCapacity = mch.HeatingCapacity;
            tmpmch.Model = mch.Model;
            tmpmch.MaxTotalCHIndoorPipeLength = mch.MaxTotalCHIndoorPipeLength;
            tmpmch.MaxTotalCHIndoorPipeLength_MaxIU = mch.MaxTotalCHIndoorPipeLength_MaxIU;
            tmpmch.MaxIndoorCount = mch.MaxIndoorCount;
            tmpmch.ActualTotalCHIndoorPipeLength = mch.ActualTotalCHIndoorPipeLength;

            tmpmch.PowerSupply = mch.PowerSupply;
            tmpmch.PowerConsumption = mch.PowerConsumption;
            tmpmch.PowerCurrent = mch.PowerCurrent;
            tmpmch.MaxBranches = mch.MaxBranches;
            tmpmch.MaxCapacityPerBranch = mch.MaxCapacityPerBranch;
            tmpmch.MaxIUPerBranch = mch.MaxIUPerBranch;
            tmpmch.PowerLineType = mch.PowerLineType;
            tmpmch.HeightDiff = mch.HeightDiff; //add by xyj on 20180620
        }

        /// <summary>
        /// 保存 Link 对象绑定的数据
        /// </summary>
        /// <param name="tmpLink"></param>
        /// <param name="myLink"></param>
        private void SaveLinkValue(tmpMyLink tmpLink, MyLink myLink)
        {
            tmpLink.ElbowQty = myLink.ElbowQty;
            tmpLink.OilTrapQty = myLink.OilTrapQty;
            tmpLink.Length = myLink.Length;

            tmpLink.SpecG_h = myLink.SpecG_h;
            tmpLink.SpecG_l = myLink.SpecG_l;
            tmpLink.SpecL = myLink.SpecL;
            tmpLink.ValveLength = myLink.ValveLength;
            tmpLink.SpecG_h_Normal = myLink.SpecG_h_Normal;
            tmpLink.SpecG_l_Normal = myLink.SpecG_l_Normal;
            tmpLink.SpecL_Normal = myLink.SpecL_Normal;

            //tmpLink.Line = myLink.Line;
            tmpLink.Style = myLink.LineStyle;
            // if (myLink.AddFlow != null)
            {
                List<Point> points = new List<Point>();
                foreach (Point pt in myLink.Points)
                {
                    points.Add(pt);
                }
                tmpLink.Points = points;
                tmpLink.PinOriginIndex = myLink.PinOriginIndex;
                tmpLink.PinDestinationIndex = myLink.PinDestinationIndex;
            }
            // tmpLink.Stroke = myLink.Stroke;   // // To be Fix latter
        }

        public Node DrawBuildingImageNode(Image img)
        {
            if (img == null) return null;

            Node node = new Node();
            //node.Tooltip = "";
            //node.LabelEdit = false;
            //node.Stroke = System.Windows.Media.Brushes.Transparent;
            //node.Transparent = false;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);        // // To be Fix latter
            //node.Logical = false;
            //node.Selectable = false;

            //addFlowPiping.Items.Add(node);
            //addFlowPiping.Images.Add(img);

            //System.Windows.Size nodeSize = new System.Windows.Size(img.Size.Width, img.Size.Height);
            //node.Size = nodeSize;
            //node.ImageIndex = addFlowPiping.Images.Count - 1;
            //node.ImagePosition = ImagePosition.LeftBottom;
            //node.BackMode = BackMode.Opaque;
            //node.ZOrder = 0;

            return node;
        }

        //public Node DrawBuildingImageNode(Image img,Size size)
        //{
        //    if (img == null) return null;

        //    System.Drawing.Image newImg = img;
        //    //if (img.Size != size)
        //    //{
        //    //    int width = (int)Math.Round(size.Width);    // Will take care during implemetauin
        //    //    int height = (int)Math.Round(size.Height);
        //    //    newImg = new Bitmap(img, width, height);
        //    //}
        //    return DrawBuildingImageNode(newImg);
        //}

        //public Node DrawBuildingImageNode(tmpNodeBgImage tempBuildingImageNode)
        //{
        //    Image img;
        //    return DrawBuildingImageNode(tempBuildingImageNode, out img);
        //}

        //public Node DrawBuildingImageNode(tmpNodeBgImage tempBuildingImageNode, out Image originalImage)               // To be Fix latter  
        //{
        //    originalImage = null;
        //    if (tempBuildingImageNode == null) return null;

        //    string id = tempBuildingImageNode.ImageCacheId;
        //    if (string.IsNullOrEmpty(id) || !BuildingImageCache.ContainsKey(id)) return null;

        //    originalImage = BuildingImageCache[id];
        //    Node node = DrawBuildingImageNode(originalImage, tempBuildingImageNode.Size);
        //    if (node != null)
        //    {
        //        node.Location = tempBuildingImageNode.Location;
        //    }
        //    return node;
        //}


        public void MoveAllItems(float dx, float dy)
        {
            foreach (Item item in addFlowPiping.Items)
            {
                if (item is Node)
                {
                    Node node = (item as Node);
                    Point pt1 = node.Location;
                    double x = pt1.X;
                    double y = pt1.Y;
                    if (dx > 0) x += dx;
                    if (dy > 0) y += dy;
                    node.Location = new Point(x, y);
                }
                else if (item is Link)
                {
                    Link link = (item as Link);
                    for (int i = 0; i < link.Points.Count; i++)
                    {
                        Point pt1 = link.Points[i];
                        double x = pt1.X;
                        double y = pt1.Y;
                        if (dx > 0) x += dx;
                        if (dy > 0) y += dy;
                        link.Points[i] = new Point(x, y);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 遍历每一个节点
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="action">处理业务逻辑的函数(无返回值)</param>
        public static void EachNode(Node node, Action<Node> action)
        {
            EachNode(node, (node1) =>
            {
                action(node1);
                return false;  //true:结束遍历Node树，false：继续遍历Node树。
            }, (a, result) => result);
        }
        private static TResult EachNode<TNode, TResult>(TNode node, Func<TNode, TResult> predicate, Func<bool, TResult, bool> breakPredicate)
        {
            if (node == null) return default(TResult);

            TResult result = predicate(node);
            if (breakPredicate(false, result)) return result; //true:结束遍历Node树，false：继续遍历Node树。

            if (node is MyNodeOut || node is tmpMyNodeOut)
            {
                TNode childNode = (node as ISingleChild<TNode>).ChildNode;
                return EachNode(childNode, predicate, breakPredicate);
            }
            else if (node is MyNodeYP || node is tmpMyNodeYP)
            {
                TNode[] children = (node as IChildNodeArray<TNode>).ChildNodes;
                foreach (TNode child in children)
                {
                    result = EachNode(child, predicate, breakPredicate);
                    if (breakPredicate(true, result)) return result; //true:结束遍历Node树，false：继续遍历Node树。
                }
            }
            else if (node is MyNodeCH || node is tmpMyNodeCH)
            {
                TNode childNode = (node as ISingleChild<TNode>).ChildNode;
                return EachNode(childNode, predicate, breakPredicate);
            }
            else if (node is MyNodeMultiCH || node is tmpMyNodeMultiCH)
            {
                List<TNode> children = (node as IChildNodeList<TNode>).ChildNodes;
                foreach (TNode child in children)
                {
                    result = EachNode(child, predicate, breakPredicate);
                    if (breakPredicate(true, result)) return result; //true:结束遍历Node树，false：继续遍历Node树。
                }
            }
            return result;
        }
        /// <summary>
        /// 遍历每一个节点，根据predicate函数参数的返回值可以中断遍历。
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="predicate">处理业务逻辑的函数。返回值: true:结束遍历Node树，false：继续遍历Node树。</param>
        /// <param name="breakForeachChildNodes">是否允许中断遍历子节点的循环</param>
        public static void EachNode(Node node, Func<Node, bool> predicate, bool breakForeachChildNodes = true)
        {
            EachNode(node, predicate, (foreachChildNodes, result) =>
            {
                if (foreachChildNodes && !breakForeachChildNodes)
                {
                    //breakForeachChildNodes为False的时候, 不会跳出ChildNodes的遍历
                    return false;
                }
                return result; //true:结束遍历Node树，false：继续遍历Node树。
            });
        }

        /// <summary>
        /// 遍历每一个节点
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="node">当前节点</param>
        /// <param name="predicate">处理业务逻辑的函数。</param>
        /// <param name="breakPredicate">对业务处理函数的返回值进行处理，已决定是否结束遍历Node树。返回值: true:结束遍历Node树，false：继续遍历Node树。</param>
        /// <returns></returns>
        private static TResult EachNode<TResult>(Node node, Func<Node, TResult> predicate, Func<bool, TResult, bool> breakPredicate)
        {
            if (node == null) return default(TResult);

            TResult result = predicate(node);
            if (breakPredicate(false, result)) return result; //true:结束遍历Node树，false：继续遍历Node树。

            if (node is MyNodeOut)
            {
                return EachNode((node as MyNodeOut).ChildNode, predicate, breakPredicate);
            }
            else if (node is MyNodeYP)
            {
                Node[] children = (node as MyNodeYP).ChildNodes;
                foreach (Node child in children)
                {
                    result = EachNode(child, predicate, breakPredicate);
                    if (breakPredicate(true, result)) return result; //true:结束遍历Node树，false：继续遍历Node树。
                }
            }
            else if (node is MyNodeCH)
            {
                return EachNode((node as MyNodeCH).ChildNode, predicate, breakPredicate);
            }
            else if (node is MyNodeMultiCH)
            {
                List<Node> children = (node as MyNodeMultiCH).ChildNodes;
                foreach (Node child in children)
                {
                    result = EachNode(child, predicate, breakPredicate);
                    if (breakPredicate(true, result)) return result; //true:结束遍历Node树，false：继续遍历Node树。
                }
            }
            return result;
        }

        /// <summary>
        /// 绘制Manual Piping上的比例尺
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public MyNodePlottingScale DrawPlottingScaleNode(SystemVRF sysItem, MyNodePlottingScale node, ref AddFlow addFlowPiping, bool isDirectionChanginOnExistingNode = false)
        {
            if (node == null)
            {
                node = new MyNodePlottingScale();
                node.ActualLengthString = "0 m";
                node.PlottingScaleColor = new SolidColorBrush(Colors.Black);
                node.Size = new System.Windows.Size(150, 25);
                node.IsSelectable = true;
                node.IsVertical = false;
                node.ActualLength = 0;
                node.StrokeThickness = 0;
                node.TextPosition = TextPosition.CenterBottom;
                node.TextMargin = new Thickness(0, 1, 0, 1);
                node.Geometry = new RectangleGeometry(new Rect(0, 0, 64, 64));
                node.IsRotatable = false;
                node.IsInLinkable = false;
                node.IsOutLinkable = false;
                node.Location = new Point(10, Math.Max(addFlowPiping.ViewportHeight - 150, 370));
                DrawHorizontalPlottingScale(node);
                addFlowPiping.AddNode(node);
            }

            if (node != null)
            {
                node.StrokeThickness = 0;
                node.TextPosition = TextPosition.CenterBottom;
                node.TextMargin = new Thickness(0, 1, 0, 1);
                node.Geometry = new RectangleGeometry(new Rect(0, 0, 64, 64));
                node.IsRotatable = false;
                node.IsInLinkable = false;

                bool isVertical = node.IsVertical;
                if (isVertical)
                {
                    node.Size = new System.Windows.Size(25, Convert.ToSingle(Math.Max(10, isDirectionChanginOnExistingNode ? node.Size.Width : node.Size.Height)));
                   // node.Size = new System.Windows.Size(20, Math.Max(10, node.Size.Height));

                    node.IsXSizeable = false;
                    node.IsYSizeable = true;
                    DrawVerticalPlottingScale(node);
                }
                else
                {
                    node.Size = new System.Windows.Size(Math.Max(10, isDirectionChanginOnExistingNode ? node.Size.Height == 25 ? 150 : node.Size.Height : node.Size.Width), 25);
                   // node.Size = new System.Windows.Size(Math.Max(10, node.Size.Width), 20);

                    node.IsXSizeable = true;
                    node.IsYSizeable = false;
                    DrawHorizontalPlottingScale(node);
                }
                node.IsOutLinkable = false;
            }


            node.IsSelectable = true;
            if (sysItem != null)
            {
                SavePipingPlottingScaleNode(sysItem, node);
            }
            return node;
        }

        private void DrawHorizontalPlottingScale(MyNodePlottingScale node)
        {
            node.Visual.Children.Clear();
            var plotScaleText = node.ActualLengthString == null ? "0 m" : node.ActualLengthString;
            if (string.IsNullOrEmpty(node.ActualLengthString))
            {
                if (node.ActualLength > 0)
                {
                    plotScaleText = node.ActualLength.ToString() + " " + OldModel.SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                }
            }
            DrawingVisual dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            System.Windows.Media.Brush brush = node.PlottingScaleColor;
            node.Foreground = brush;
            var barHeight = 2;
            var horizontalBarHeight = 4;
            var sideBarLength = 10;
            var sideEmptySpace = 10;
            var nodeWidth = node.Size.Width;
            var nodeHeight = node.Size.Height;
            var nodeLocX = node.Location.X;
            var nodeLocY = node.Location.Y;
            var horizontalBar = new Rect(nodeLocX + sideEmptySpace, nodeLocY + nodeHeight - barHeight, nodeWidth - 2 * sideEmptySpace, horizontalBarHeight);
            var leftSidebar = new Rect(nodeLocX + sideEmptySpace, nodeLocY + nodeHeight - sideBarLength, barHeight, sideBarLength);
            var rightSidebar = new Rect(nodeLocX + nodeWidth - barHeight - sideEmptySpace, nodeLocY + nodeHeight - sideBarLength, barHeight, sideBarLength);
            node.IsYSizeable = false;
            node.IsXSizeable = true;
            dc.DrawRectangle(brush, null, horizontalBar);
            dc.DrawRectangle(brush, null, leftSidebar);
            dc.DrawRectangle(brush, null, rightSidebar);
            FormattedText scaleTextFormat = new FormattedText(plotScaleText,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("TimeNewRoman"), 14, brush);
            dc.DrawText(scaleTextFormat, new Point(node.Bounds.X + node.Size.Width / 2 - plotScaleText.Length / 2 - sideEmptySpace, node.Bounds.Y + nodeHeight / 2 - 2 * barHeight));
            dc.Close();
            node.Visual.Children.Add(dv);
            node.Fill = Brushes.Transparent;
        }
        private void DrawVerticalPlottingScale(MyNodePlottingScale node)
        {
            node.Visual.Children.Clear();
            var plotScaleText = node.ActualLengthString == null ? "0 m" : node.ActualLengthString;
            if (string.IsNullOrEmpty(node.ActualLengthString))
            {
                if (node.ActualLength > 0)
                {
                    plotScaleText = node.ActualLength.ToString() + " " + OldModel.SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                }
            }
            DrawingVisual dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            System.Windows.Media.Brush brush = node.PlottingScaleColor;
            node.Foreground = brush;
            var barHeight = 2;
            var sideBarLength = 10;
            var verticalBarWidth = 4;
            var sideEmptySpace = 10;
            var nodeHeight = node.Size.Height;
            var nodeLocX = node.Location.X;
            var nodeLocY = node.Location.Y;
            var horizontalBar = new Rect(nodeLocX - barHeight, nodeLocY + sideEmptySpace, verticalBarWidth, nodeHeight - 2 * sideEmptySpace);
            var leftSidebar = new Rect(new Point(horizontalBar.Location.X, horizontalBar.Location.Y), new System.Windows.Size(sideBarLength, barHeight));
            var rightSidebar = new Rect(new Point(leftSidebar.X, leftSidebar.Y + horizontalBar.Height - barHeight), new System.Windows.Size(sideBarLength, barHeight));
            node.IsYSizeable = true;
            node.IsXSizeable = false;
            dc.DrawRectangle(brush, null, horizontalBar);
            dc.DrawRectangle(brush, null, leftSidebar);
            dc.DrawRectangle(brush, null, rightSidebar);
            FormattedText scaleTextFormat = new FormattedText(plotScaleText,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface("TimeNewRoman"), 14, brush);
            dc.PushTransform(new RotateTransform(-270, node.Bounds.X + verticalBarWidth + sideEmptySpace + barHeight, node.Bounds.Y + nodeHeight / 2 - sideEmptySpace));
            dc.DrawText(scaleTextFormat, new Point(node.Bounds.X + verticalBarWidth + sideEmptySpace + barHeight, node.Bounds.Y + nodeHeight / 2 - sideEmptySpace));
            dc.Pop();
            dc.Close();
            node.Visual.Children.Add(dv);
            node.Fill = Brushes.Transparent;
        }

        /// <summary>
        /// 根据比例尺计算管长
        /// </summary>
        /// <param name="scale">图表与现实场景的比例</param>
        public void CalculatePipeLengthByPlottingScale(SystemVRF sysItem, double scale, Node node, Node parent)
        {
            if (node == null || parent == null) return;
            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                foreach (MyLink link in myNode.MyInLinks)
                {
                    double len = 0;
                    if (scale > 0)
                    {
                        for (int i = 1; i < link.Points.Count; i++)
                        {
                            Point pt1 = link.Points[i - 1];
                            Point pt2 = link.Points[i];
                            len += Math.Pow(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2), 0.5) * scale; //求直角对边公式 c =√a²+b²
                        }
                    }
                    link.Length = len;
                }

                if (node is MyNodeIn)
                {
                    //drawTextToIDUNode(sysItem, node as MyNodeIn);
                }
                else if (node is MyNodeYP || node is MyNodeCH || node is MyNodeMultiCH)
                {
                    drawTextToOtherNode(node, parent, sysItem);
                }

                for (int i = 0; i < myNode.MyInLinks.Count; i++)
                {
                    MyLink myLink = myNode.MyInLinks[i];
                    //drawTextToLink(myLink, i, parent, node, isInch, sysItem);
                }
            }
        }

        public void CalculateAllPipeLengthByPlottingScale(SystemVRF sysItem, double scale)
        {
            CalculateAllPipeLengthByPlottingScale(sysItem, scale, sysItem.MyPipingNodeOut.ChildNode, sysItem.MyPipingNodeOut);
        }

        /// <summary>
        /// 根据比例尺计算每一段的管长
        /// </summary>
        /// <param name="scale">图表与现实场景的比例</param>
        public void CalculateAllPipeLengthByPlottingScale(SystemVRF sysItem, double scale, Node node, Node parent)
        {
            if (node == null)
            {
                return;
            }

            if (node is MyNodeOut)
            {
                CalculateAllPipeLengthByPlottingScale(sysItem, scale, (node as MyNodeOut).ChildNode, node);
            }
            else
            {
                CalculatePipeLengthByPlottingScale(sysItem, scale, node, parent);

                if (node is MyNodeYP)
                {
                    foreach (Node child in (node as MyNodeYP).ChildNodes)
                    {
                        CalculateAllPipeLengthByPlottingScale(sysItem, scale, child, node);
                    }
                }
                else if (node is MyNodeCH)
                {
                    CalculateAllPipeLengthByPlottingScale(sysItem, scale, (node as MyNodeCH).ChildNode, node);
                }
                else if (node is MyNodeMultiCH)
                {
                    foreach (Node child in (node as MyNodeMultiCH).ChildNodes)
                    {
                        CalculateAllPipeLengthByPlottingScale(sysItem, scale, child, node);
                    }
                }
            }
        }

        public Node DrawAllShapesDemo()
        {
            //ShapeStyle[] ss = new ShapeStyle[] {
            //    ShapeStyle.AlternateProcess
            //    ,ShapeStyle.Card
            //    ,ShapeStyle.Collate
            //    ,ShapeStyle.Connector
            //    ,ShapeStyle.Custom
            //    ,ShapeStyle.Data
            //    ,ShapeStyle.Decision
            //    ,ShapeStyle.Delay
            //    ,ShapeStyle.DirectAccessStorage
            //    ,ShapeStyle.Display
            //    ,ShapeStyle.Document
            //    ,ShapeStyle.Ellipse
            //    ,ShapeStyle.Extract
            //    ,ShapeStyle.Hexagon
            //    ,ShapeStyle.InternalStorage
            //    ,ShapeStyle.Losange
            //    ,ShapeStyle.MagneticDisk
            //    ,ShapeStyle.ManualInput
            //    ,ShapeStyle.ManualOperation
            //    ,ShapeStyle.Merge
            //    ,ShapeStyle.MultiDocument
            //    ,ShapeStyle.Octogon
            //    ,ShapeStyle.OffPageConnection
            //    ,ShapeStyle.Or
            //    ,ShapeStyle.OrGate
            //    ,ShapeStyle.Pentagon
            //    ,ShapeStyle.PredefinedProcess
            //    ,ShapeStyle.Preparation
            //    ,ShapeStyle.Process
            //    ,ShapeStyle.ProcessIso9000
            //    ,ShapeStyle.PunchedTape
            //    ,ShapeStyle.Rectangle
            //    ,ShapeStyle.RectEdgeBump
            //    ,ShapeStyle.RectEdgeEtched
            //    ,ShapeStyle.RectEdgeRaised
            //    ,ShapeStyle.RectEdgeSunken
            //    ,ShapeStyle.RoundRect
            //    ,ShapeStyle.SequentialAccessStorage
            //    ,ShapeStyle.StoredData
            //    ,ShapeStyle.SummingJunction
            //    ,ShapeStyle.Sort
            //    ,ShapeStyle.Termination
            //    ,ShapeStyle.Transport
            //    ,ShapeStyle.Triangle
            //    ,ShapeStyle.TriangleRectangle
            //};

            //for (int i = 0; i < ss.Length; i++)
            //{
            //    Node node = new Node();
            //    node.Shape = new Shape(ss[i], ShapeOrientation.so_0);
            //    node.Location = new Point(80 * (i % 10) + 80, i / 10 * 80 + 200);
            //    node.Size = new SizeF(20, 20);
            //    node.FillColor = Color.Blue;
            //    node.Stroke = Color.Green;
            //    node.TextColor = Color.Red;
            //    addFlowPiping.Nodes.Add(node);

            //    Node label = new Node();
            //    string text = ss[i].ToString();
            //    initTextNode(label, text);
            //    label.Alignment = Alignment.CenterTOP;
            //    addFlowPiping.Nodes.Add(label);
            //    label.Parent = node;
            //    label.Location = new Point(node.Location.X + (node.Size.Width - label.Size.Width) / 2
            //        , node.Location.Y - UtilPiping.HeightForNodeText);
            //}
            return null;
        }


        #region CH_piping 20160503 by Yunxiao Lin

        /// 从某节点开始搜索直到室外机，找到CH-Box后返回CH-Box，否则返回null--递归 add on 20160515 by Yunxiao Lin
        /// <summary>
        /// 从某节点开始搜索直到室外机，找到CH-Box后返回CH-Box，否则返回null--递归
        /// </summary>
        public MyNodeCH getCHUpward(Node node)
        {
            if (node is MyNodeOut) return null;
            if (node.Links[0] != null)
            {
                if (node.Links[0].Org != null)
                {
                    if (node.Links[0].Org is MyNodeCH)
                        return node.Links[0].Org as MyNodeCH;
                    else
                        return getCHUpward(node.Links[0].Org);
                }
            }
            return null;
        }

        /// <summary>
        /// 从某节点开始搜索直到室外机，找到Multi CH-Box后返回Multi CH-Box，否则返回null--递归
        /// </summary>
        public MyNodeMultiCH GetMultiCHUpward(Node node)
        {
            if (node is MyNodeOut) return null;
            if (node.Links[0] != null)
            {
                Node org = node.Links[0].Org;
                if (org != null)
                {
                    if (org is MyNodeMultiCH)
                        return org as MyNodeMultiCH;
                    else
                        return GetMultiCHUpward(org);
                }
            }
            return null;
        }

        /// 检查某节点之前有没有CH Box 和Multi CH Box  add on 20171129 by Shen Junjie
        /// <summary>
        /// 检查某节点之前有没有CH Box 和Multi CH Box
        /// </summary>
        public bool ExistCHBoxUpward(Node node)
        {
            if (node is MyNodeCH || node is MyNodeMultiCH)
            {
                return true;
            }

            if (node.Links.Count > 0 && node.Links[0] != null)
            {
                Node org = node.Links[0].Org;
                return ExistCHBoxUpward(org);
            }
            return false;
        }

        /// <summary>
        /// 从某branch节点往下遍历搜索，是否存在CH-Box
        /// </summary>
        public bool ExistsCHBoxDownward(Node node)
        {
            if (node is MyNodeCH || node is MyNodeMultiCH)
            {
                return true;
            }
            else if (node is MyNodeOut)
            {
                return ExistsCHBoxDownward((node as MyNodeOut).ChildNode);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    if (ExistsCHBoxDownward(nodeYP.ChildNodes[i]))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从某branch节点往下遍历搜索，是否存在CH-Box
        /// </summary>
        public bool ExistsMultiCHBoxDownward(Node node)
        {
            if (node is MyNodeMultiCH)
            {
                return true;
            }
            else if (node is MyNodeOut)
            {
                return ExistsMultiCHBoxDownward((node as MyNodeOut).ChildNode);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    if (ExistsMultiCHBoxDownward(nodeYP.ChildNodes[i]))
                        return true;
                }
            }
            else if (node is MyNodeCH)
            {
                return ExistsMultiCHBoxDownward((node as MyNodeCH).ChildNode);
            }
            return false;
        }

        /// <summary>
        /// 判断指定节点上层遍历是否存在梳形管
        /// </summary>
        public bool ExistCPUpward(Node node)
        {
            if (node == null) return false;

            if (node is MyNodeYP && (node as MyNodeYP).IsCP)
            {
                return true;
            }

            if (node is MyNode)
            {
                Node Pnode = (node as MyNode).ParentNode; //getRealPNode(curSystemItem, node);
                return ExistCPUpward(Pnode);
            }
            return false;
        }

        /// <summary>
        /// 判断指定节点下层遍历是否存在梳形管或分歧管
        /// </summary>
        public bool ExistYPDownward(Node node)
        {
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                return ExistYPDownward(nodeOut.ChildNode);
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                return ExistYPDownward(nodeCH.ChildNode);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                return true;
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    if (ExistYPDownward(nodeMCH.ChildNodes[i]))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 拖拽完成后处理拖拽节点的原父节点
        /// </summary>
        public void CutOldRelation(Node parent, Node node)
        {
            if (parent == null) return;

            if (parent is MyNodeOut)
            {
                MyNodeOut parentOut = parent as MyNodeOut;
                parentOut.ChildNode = null;
            }
            else if (parent is MyNodeCH)
            {
                MyNodeCH parentCH = parent as MyNodeCH;
                parentCH.ChildNode = null;
                CutOldRelation(parentCH.ParentNode, parentCH);
            }
            else if (parent is MyNodeMultiCH)
            {
                MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                parentMCH.ChildNodes.Remove(node);

                if (parentMCH.ChildNodes.Count == 0)
                {
                    CutOldRelation(parentMCH.ParentNode, parentMCH);
                    parentMCH.ParentNode = null;
                }
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                parentYP.RemoveChildNode(node);

                if (!parentYP.IsCP)
                {
                    if (parentYP.ChildCount == 1)
                    {
                        MyNode ppNode = parentYP.ParentNode;
                        Node child = parentYP.ChildNodes[0];
                        ReplaceChildNode(ppNode, parentYP, child);
                    }
                    else if (parentYP.ChildCount == 0)
                    {
                        CutOldRelation(parentYP.ParentNode, parentYP);
                    }
                }
                else
                {
                    //如果父节点是梳形管，则判断是否还有剩余子节点
                    if (parentYP.ChildCount == 0)
                    {
                        CutOldRelation(parentYP.ParentNode, parentYP);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 将CHBox替换成 Multi-port CH Box
        /// </summary>
        /// <param name="nodeCH"></param>
        /// <returns></returns>
        public MyNodeMultiCH ReplaceCH2MultiCH(MyNodeCH nodeCH)
        {
            MyNode parent = nodeCH.ParentNode;
            MyNodeMultiCH mch = utilPiping.createNodeMultiCHbox();
            mch.AddChildNode(nodeCH.ChildNode);
            nodeCH.ChildNode = null;
            ReplaceChildNode(parent, nodeCH, mch);
            return mch;
        }

        /// <summary>
        /// 将Multi-port CH Box替换成 CHBox
        /// </summary>
        /// <param name="nodeMCH"></param>
        /// <returns></returns>
        public MyNodeCH ReplaceMultiCH2CH(MyNodeMultiCH nodeMCH)
        {
            MyNode parent = nodeMCH.ParentNode;
            Node headNode = DeleteMultiCHBox(nodeMCH);
            return InsertCHBox(headNode);
        }

        /// <summary>
        /// 在指定的节点之前插入一个CH Box
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public MyNodeCH InsertCHBox(Node node)
        {
            if (!(node is MyNode)) return null;

            MyNode parent = (node as MyNode).ParentNode;
            MyNodeCH nodeCH = utilPiping.createNodeCHbox(null);
            ReplaceChildNode(parent, node, nodeCH);
            nodeCH.ChildNode = node;
            return nodeCH;
        }

        /// <summary>
        /// 在指定的节点之前插入一个Multiple CH Box
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public MyNodeMultiCH InsertMultiCH(Node node)
        {
            if (!(node is MyNode)) return null;

            MyNode parent = (node as MyNode).ParentNode;
            if (!(parent is MyNodeOut) && !(parent is MyNodeYP))
            {
                return null;
            }

            //将CHBox替换成 Multi-port CH Box
            MyNodeMultiCH mch = utilPiping.createNodeMultiCHbox();
            ReplaceChildNode(parent, node, mch);
            mch.AddChildNode(node);
            return mch;
        }

        public Node DeleteMultiCHBox(MyNodeMultiCH nodeMCH)
        {
            MyNode parent = nodeMCH.ParentNode;
            Node newChild = null;
            MyNodeYP parentYP = null;
            for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
            {
                Node childOfMCH = nodeMCH.ChildNodes[i];
                if (nodeMCH.ChildNodes.Count - 1 > i)
                {
                    MyNodeYP newYP = utilPiping.createNodeYP(false);
                    if (parentYP != null)
                    {
                        parentYP.AddChildNode(newYP);
                    }
                    parentYP = newYP;
                }
                if (parentYP != null)
                {
                    parentYP.AddChildNode(childOfMCH);
                }
                if (i == 0)
                {
                    newChild = childOfMCH;
                    if (parentYP != null)
                    {
                        newChild = parentYP;
                    }
                }
            }

            ReplaceChildNode(parent, nodeMCH, newChild);

            return newChild;
        }

        /// <summary>
        /// 从某节点向上遍历，删除发现的CH Box 和 Multi CH Box，CH-Box前后节点直接相连
        /// </summary>
        public void DeleteCHBoxUpward(Node node)
        {
            if (node == null) return;
            if (!(node is MyNode)) return;
            MyNode parent = (node as MyNode).ParentNode;

            if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                ReplaceChildNode(parent, nodeCH, nodeCH.ChildNode);
            }
            else if (node is MyNodeMultiCH)
            {
                DeleteMultiCHBox(node as MyNodeMultiCH);
            }
            DeleteCHBoxUpward(parent);
        }

        /// <summary>
        /// 从某节点向下遍历，删除发现的CH Box 和 Multi CH Box，CH-Box前后节点直接相连
        /// </summary>
        /// <param name="node"></param>
        public void DeleteCHBoxDownward(Node node)
        {
            if (node == null) return;
            if (!(node is MyNode)) return;
            MyNode parent = (node as MyNode).ParentNode;
            if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                Node newChild = nodeCH.ChildNode;
                ReplaceChildNode(parent, nodeCH, newChild);

                DeleteCHBoxDownward(newChild);
            }
            else if (node is MyNodeMultiCH)
            {
                Node newChild = DeleteMultiCHBox(node as MyNodeMultiCH);
                DeleteCHBoxDownward(newChild);
                return;
            }
            else if (node is MyNodeOut)
            {
                DeleteCHBoxDownward((node as MyNodeOut).ChildNode);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    DeleteCHBoxDownward(nodeYP.ChildNodes[i]);
                }
            }
        }

        /// <summary>
        /// 设置某节点下方遍历所有节点的isCoolingonly属性
        /// </summary>
        public void SetIsCoolingOnly(MyNodeOut nodeOut)
        {
            bool hasCHBoxChild;
            SetIsCoolingOnlyRecursively(nodeOut.ChildNode, false, out hasCHBoxChild);
        }

        /// <summary>
        /// 设置某节点下方遍历所有节点的isCoolingonly属性
        /// </summary>
        private void SetIsCoolingOnlyRecursively(Node node, bool hasCHBoxParent, out bool hasCHBoxChild)
        {
            hasCHBoxChild = false;
            if (node == null) return;

            if (node is MyNodeCH)
            {
                //CH-Box没有isCoolingonly属性
                SetIsCoolingOnlyRecursively((node as MyNodeCH).ChildNode, true, out hasCHBoxChild);
                hasCHBoxChild = true;
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = (node as MyNodeMultiCH);
                for (var i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    SetIsCoolingOnlyRecursively(nodeMCH.ChildNodes[i], true, out hasCHBoxChild);
                }
                hasCHBoxChild = true;
            }
            else if (node is MyNodeIn)
            {
                (node as MyNodeIn).IsCoolingonly = !hasCHBoxParent;
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP yp = (node as MyNodeYP);
                for (var i = 0; i < yp.ChildCount; i++)
                {
                    bool hasCHBoxChildOfYP;
                    SetIsCoolingOnlyRecursively(yp.ChildNodes[i], hasCHBoxParent, out hasCHBoxChildOfYP);
                    hasCHBoxChild = hasCHBoxChildOfYP || hasCHBoxChild;
                }

                //分歧管的isCoolingonly属性取决于其上方和其下方遍历所有节点是否存在CH-Box
                yp.IsCoolingonly = !hasCHBoxParent && !hasCHBoxChild;
            }
            utilPiping.setItemDefault(node, true);
        }

        #region 拖拽
        /// <summary>
        /// 检测是否符合拖拽的条件
        /// </summary>
        public bool DraggingCheck(MyNode selNode, Item aimItem, SystemVRF sysItem)
        {
            if (sysItem == null || sysItem.OutdoorItem == null || sysItem.MyPipingNodeOut == null)
                return false;

            if (selNode == null || aimItem == null || selNode == sysItem.MyPipingNodeOut)
                return false;

            MyNode aimNode = null;
            if (aimItem is MyLink)
            {
                aimNode = (aimItem as MyLink).Dst as MyNode;
            }
            else if (aimItem is MyNode)
            {
                aimNode = aimItem as MyNode;
            }

            if (aimNode == null)
            {
                return false;
            }

            MyNode selPNode = selNode.ParentNode;
            MyNode aimPNode = aimNode.ParentNode;

            if (selPNode == null || aimPNode == null || selPNode == sysItem.MyPipingNodeOut)
                return false;

            //拖动的节点和目标节点不能是同一个
            if (selNode == aimNode)
                return false;

            if (aimNode == selPNode && (aimNode is MyNodeCH || aimNode is MyNodeOut))
                return false;

            //if (selNode is MyNodeCH) return false;

            // 拖拽节点为目标节点的上层节点（父辈、祖父辈等）
            if (utilPiping.isContain(selNode, aimNode))
                return false;

            if ((aimPNode is MyNodeYP) && (aimPNode as MyNodeYP).IsCP)
            {
                if (selPNode == null || selPNode != aimPNode)
                {
                    if ((aimPNode as MyNodeYP).ChildCount == 8)
                        return false;
                }
            }

            if ((aimNode is MyNodeYP) && (aimNode as MyNodeYP).IsCP)
            {
                if ((aimNode as MyNodeYP).ChildCount == 8)
                    return false;
            }

            if (aimPNode is MyNodeCH)
            {
                Node appn = (aimPNode as MyNodeCH).ParentNode;
                if ((appn is MyNodeYP) && (appn as MyNodeYP).IsCP)
                {
                    if (selPNode == null || selPNode != aimPNode)
                    {
                        if ((appn as MyNodeYP).ChildCount == 8)
                            return false;
                    }
                }
            }

            if (aimNode is MyNodeMultiCH)
            {
                if ((aimNode as MyNodeMultiCH).ChildNodes.Count == 16)
                    return false;
            }

            //add by Shen Junjie On 2017/12/15
            if (ExistCPUpward(aimNode))
            {
                if (selNode is MyNodeIn)
                {
                    //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。前面必须是YP所以不能再拖动到CP add by Shen Junjie On 2018/01/24
                    if (IsTwoInlinkIndoor((selNode as MyNodeIn).RoomIndooItem.IndoorItem.Model_Hitachi))
                    {
                        return false;
                    }
                }

                //梳形管后面不能有YP
                if (ExistYPDownward(selNode))
                    return false;

                //梳形管后面不能有Multiple CH Box
                if (ExistsMultiCHBoxDownward(selNode))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 在parent和child1之间插入一个新的YP，并将child1和child2作为新YP的两个子节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child1"></param>
        /// <param name="child2"></param>
        /// <returns></returns>
        private MyNodeYP InsertNodeYP(Node parent, Node child1, Node child2)
        {
            MyNodeYP yp = utilPiping.createNodeYP(false);

            yp.AddChildNode(child1);
            yp.AddChildNode(child2);

            ReplaceChildNode(parent, child1, yp);
            return yp;
        }

        private void DragToNodeYP(Node selPNode, Node selNode, MyNodeYP targetYP, bool isHR, Node cpChild = null)
        {
            if (targetYP.IsCP)
            {
                //拖拽节点与目标节点并列
                if (cpChild != null)
                {
                    int index = targetYP.GetIndex(cpChild);
                    targetYP.AddChildNode(selNode, index);
                }
                else
                {
                    targetYP.AddChildNode(selNode);
                }
            }
            else
            {
                InsertNodeYP(targetYP.ParentNode, targetYP, selNode);
            }

            if (isHR)
            {
                if (targetYP.IsCoolingonly || ExistCHBoxUpward(targetYP))
                {
                    DeleteCHBoxDownward(selNode);
                }
                else
                {
                    if (!ExistsCHBoxDownward(selNode))
                    {
                        InsertCHBox(selNode);
                    }
                }
            }

            //执行处理拖拽节点的原父节点
            CutOldRelation(selPNode, selNode);
        }

        private void DragToNodeCHBox(Node selPNode, Node selNode, MyNodeCH targetCH, bool isHR)
        {
            if (isHR && ExistCPUpward(targetCH))
            {
                //如果目标节点是CH
                //在目标CH并列增加CH
                if (targetCH.ParentNode is MyNodeYP && (targetCH.ParentNode as MyNodeYP).IsCP)
                {
                    DragToNodeYP(selPNode, selNode, targetCH.ParentNode as MyNodeYP, isHR);
                    return;
                }
                else
                {
                    return;
                }
            }
            else
            {
                Node child = selNode;
                if (isHR && !ExistsCHBoxDownward(selNode))
                {
                    child = utilPiping.createNodeCHbox(selNode);
                }
                InsertNodeYP(targetCH.ParentNode, targetCH, child);
            }
            //执行处理拖拽节点的原父节点
            CutOldRelation(selPNode, selNode);
        }

        private void DragToNodeMultiCHBox(Node selPNode, Node selNode, MyNodeMultiCH targetMCH)
        {
            targetMCH.ChildNodes.Add(selNode);
            if (selNode is MyNode)
            {
                (selNode as MyNode).ParentNode = targetMCH;
                DeleteCHBoxDownward(selNode);
            }
            else
            {
                return;
            }

            //拖拽完成之后需要处理原拖拽的父节点
            CutOldRelation(selPNode, selNode);
        }

        private void DragToNodeIn(Node selNode, MyNodeIn targetNodeIn, Node selPNode, Node aimPNode, bool isHR)
        {
            if (aimPNode is MyNodeYP)
            {
                MyNodeYP nodeYP = aimPNode as MyNodeYP;

                //如果目标节点的父节点是梳形管
                if (nodeYP.IsCP)
                {
                    if (nodeYP.ChildCount >= nodeYP.MaxCount)
                        return;

                    DragToNodeYP(selPNode, selNode, aimPNode as MyNodeYP, isHR, targetNodeIn);
                    return;
                }
                else
                {
                    if (nodeYP.ChildCount < nodeYP.MaxCount)
                    {
                        //RPI-16.0,20.0FSN3PE(-f) 前面的YP子节点没有满  add by Shen Junjie on 2018/01/24
                        nodeYP.AddChildNode(selNode);
                    }
                    else
                    {
                        InsertNodeYP(aimPNode, targetNodeIn, selNode);
                    }
                }
            }
            else if (ExistCPUpward(aimPNode))
            {
                return;
            }
            else
            {
                InsertNodeYP(aimPNode, targetNodeIn, selNode);
            }

            if (isHR)
            {
                //如果indoor的上面有CHBox，则要删掉拖拽过来的CHBox
                if (targetNodeIn.IsCoolingonly || ExistCHBoxUpward(targetNodeIn))
                {
                    DeleteCHBoxDownward(selNode);
                }
            }

            //拖拽完成之后需要处理原拖拽的父节点
            CutOldRelation(selPNode, selNode);
        }

        /// <summary>
        /// 交换室内机节点
        /// </summary>
        public void SwapNodeIn(MyNodeIn nodeIn1, MyNodeIn nodeIn2, bool isHR)
        {
            MyNode parent1 = nodeIn1.ParentNode;
            MyNode parent2 = nodeIn2.ParentNode;

            ReplaceChildNode(parent1, nodeIn1, nodeIn2);

            ReplaceChildNode(parent2, nodeIn2, nodeIn1);

            //修复交换indoor后，还是选中的颜色的问题。  add by Shen Junjie on 2018/3/22
            utilPiping.cancelItemHover(nodeIn2, isHR);
        }

        public void DragNode(MyNode selNode, Item aimItem, SystemVRF sysItem)
        {
            if (selNode == null || aimItem == null) return;

            //bool isDroppedToLink = false;
            MyNode aimNode = null;
            if (aimItem is MyNode)
            {
                aimNode = aimItem as MyNode;
            }
            else if (aimItem is MyLink)
            {
                Link lk = aimItem as Link;
                aimNode = lk.Dst as MyNode;
                //isDroppedToLink = true;
            }
            if (aimNode == null) return;

            MyNode selPNode = selNode.ParentNode;
            MyNode aimPNode = aimNode.ParentNode;

            if (selPNode == null || aimPNode == null)
                return;

            //YP、CP下面的节点可以互换位置  add by Shen Junjie on 2018/01/24
            if (selPNode is MyNodeYP)
            {
                MyNodeYP spn = selPNode as MyNodeYP; //选中节点的父节点
                // 目标节点 与 拖拽节点 为兄弟节点时，交换兄弟节点位置
                if (aimPNode == selPNode)
                {
                    spn.DragDropBrotherNodes(selNode, aimNode);
                    return;
                }

                if (!spn.IsCP)      // 若当前选中节点的父节点为 YP 节点
                {
                    // 目标节点 为 拖拽节点的直接父节点 时，将拖拽节点移动到第一个子节点的位置
                    if (aimNode == selPNode)
                    {
                        spn.DragDropBrotherNodes(selNode, spn.ChildNodes[0]);
                        return;
                    }
                }
            }

            bool isHR = IsHeatRecovery(sysItem);

            if (aimNode is MyNodeIn)
            {
                //if (!isDroppedToLink && selNode is MyNodeIn)
                //{
                //    SwapNodeIn(selNode as MyNodeIn, aimNode as MyNodeIn, isHR);
                //}
                //else
                //{
                //    DragToNodeIn(selNode, aimNode as MyNodeIn, selPNode, aimPNode, isHR);
                //}
                //与PM沟通后发现这不是交换室内机的真实需求，所以改回去。 modified by Shen Junjie on 20180615
                DragToNodeIn(selNode, aimNode as MyNodeIn, selPNode, aimPNode, isHR);
            }
            else if (aimNode is MyNodeCH)
            {
                DragToNodeCHBox(selPNode, selNode, aimNode as MyNodeCH, isHR);
            }
            else if (aimNode is MyNodeYP)
            {
                MyNodeYP aimNodeYP = aimNode as MyNodeYP;
                DragToNodeYP(selPNode, selNode, aimNodeYP, isHR);
            }
            else if (aimNode is MyNodeMultiCH)
            {
                DragToNodeMultiCHBox(selPNode, selNode, aimNode as MyNodeMultiCH);
            }

            foreach (Node n in new Node[] { selNode, aimNode })
            {
                if (n is MyNodeIn)
                {
                    MyNodeIn nodeIn = n as MyNodeIn;
                    //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。需要增加一个YP  add by Shen Junjie on 2018/01/24
                    if (IsTwoInlinkIndoor(nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi))
                    {
                        AddYPForTwoInlinkIndoor(nodeIn);
                    }
                }
            }
        }

        /// 计算W2冷媒量 for RAS-FSNSE|FSNPE|FSXNSE|FSXNPE 20180109 by Yunxiao Lin
        /// <summary>
        /// 计算W2冷媒量 for RAS-FSNSE|FSNPE|FSXNSE|FSXNPE
        /// </summary>
        /// <returns></returns>
        private decimal GetW2ForFSNSE(SystemVRF sysItem)
        {
            decimal W2 = 0;
            string sId = sysItem.Id;
            List<OldModel.RoomIndoor> RoomIndoorList = thisProject.RoomIndoorList;
            for (int i = 0; i < RoomIndoorList.Count; i++)
            {
                var ent = RoomIndoorList[i];
                //判断当前室内机是否属于该系统
                if (ent.SystemID != sId)
                {
                    continue;
                }
                else if (ent.IndoorItem != null)
                {
                    OldModel.Indoor ind = ent.IndoorItem;
                    if (ind.Horsepower >= 0.4d && ind.Horsepower <= 1d)
                        W2 += 0.3M;
                    //else if (ind.Horsepower >= 1.5d && ind.Horsepower <= 6d)
                    else if (ind.Horsepower >= 1.3d && ind.Horsepower <= 6d) //增加1.3HP switch IDU 判断 20180717 by Yunxiao Lin
                        W2 += 0.5M;
                }
            }
            if (sysItem.OutdoorItem != null && sysItem.OutdoorItem.Series.Contains("HR,"))
            {
                if (W2 > 6M) //Only for Heat Recovery System, maximun additional refrigerat charge must not exceed 6.0kg.
                    W2 = 6M;
            }
            return W2;
        }

        #endregion

        #region 生成节点树

        public void CreatePipingNodeStructure(SystemVRF sysItem)
        {
            MyNodeOut nodeOut = utilPiping.createNodeOut();
            nodeOut.Model = sysItem.OutdoorItem.AuxModelName;
            nodeOut.Name = sysItem.Name;
            sysItem.MyPipingNodeOut = nodeOut;

            string systemId = ((JCHVRF.Model.SystemBase)sysItem).Id;
            bool isHR = IsHeatRecovery(sysItem);
            List<OldModel.RoomIndoor> indoors = thisProject.RoomIndoorList.FindAll(ind => ind.SystemID == systemId);

            //SMZ IVX 3 OldModel.Indoor 组合固定使用梳形管连接 20170704 by Yunxiao Lin
            if (indoors.Count == 3 && sysItem.OutdoorItem.Type == "HVN(P/C/C1)/HVRNM2 (Side flow)")
            {
                MyNodeYP cp = utilPiping.createNodeYP(true);
                foreach (OldModel.RoomIndoor ri in indoors)
                {
                    MyNodeIn ind1 = utilPiping.createNodeIn(ri);
                    Node child = ind1;
                    if (isHR)
                    {
                        MyNodeCH ch1 = utilPiping.createNodeCHbox(ind1);
                        child = ch1;
                    }
                    cp.AddChildNode(child);
                }
                nodeOut.ChildNode = cp;
                return;
            }

            if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.BinaryTree)
            {
                //创建二叉树节点结构
                AppendNodeForBinaryTree(isHR, nodeOut, indoors.ToArray());
            }
            else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.Symmetries)
            {
                //Symmetries布局和Normal的节点结构相同
                AppendNodeForNormal(isHR, nodeOut, indoors.ToArray(), 0);
            }
            else if (sysItem.PipingLayoutType == OldModel.PipingLayoutTypes.SchemaA)
            {
                AppendNodeForSchemaA(isHR, nodeOut, indoors.ToArray(), 0);
            }
            else
            {
                //普通布局节点结构
                AppendNodeForNormal(isHR, nodeOut, indoors.ToArray(), 0);
            }

            //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。为每个此系列的OldModel.Indoor增加一个YP
            //add by Shen Junjie on 2018/01/22
            AddYPForTwoInlinkIndoor(indoors, nodeOut);
        }

        private void AppendNodeForBinaryTree(bool isHR, Node parentNode, OldModel.RoomIndoor[] indoors)
        {
            int indoorCount = indoors.Length;
            if (indoorCount == 0) return;

            Node newNode;
            if (indoorCount > 1)
            {
                int halfCount = (int)Math.Round(indoorCount / 2d, 0, MidpointRounding.AwayFromZero);
                OldModel.RoomIndoor[] arrPart1 = new OldModel.RoomIndoor[halfCount];
                OldModel.RoomIndoor[] arrPart2 = new OldModel.RoomIndoor[indoorCount - halfCount];

                for (int i = 0; i < halfCount; i++)
                {
                    arrPart1[i] = indoors[i];
                }

                for (int i = halfCount; i < indoorCount; i++)
                {
                    arrPart2[i - halfCount] = indoors[i];
                }

                newNode = utilPiping.createNodeYP(false);

                AppendNodeForBinaryTree(isHR, newNode, arrPart1);
                AppendNodeForBinaryTree(isHR, newNode, arrPart2);
            }
            else
            {
                MyNodeIn ind1 = utilPiping.createNodeIn(indoors[0]);
                newNode = ind1;

                if (isHR)
                {
                    MyNodeCH ch1 = utilPiping.createNodeCHbox(ind1);
                    newNode = ch1;
                }
            }

            if (parentNode is MyNodeYP)
            {
                MyNodeYP nodeYP = (parentNode as MyNodeYP);
                nodeYP.AddChildNode(newNode);
            }
            else if (parentNode is MyNodeCH)
            {
                MyNodeCH nodeCH = parentNode as MyNodeCH;
                nodeCH.ChildNode = newNode;
            }
            else if (parentNode is MyNodeOut)
            {
                MyNodeOut nodeOut = parentNode as MyNodeOut;
                nodeOut.ChildNode = newNode;
            }
        }

        private void AppendNodeForNormal(bool isHR, Node parentNode, OldModel.RoomIndoor[] indoors, int indoorIndex)
        {
            int indoorCount = indoors.Length;
            if (indoorCount == 0) return;

            Node newNode;

            MyNodeIn ind1 = utilPiping.createNodeIn(indoors[indoorIndex]);
            newNode = ind1;

            if (isHR)
            {
                MyNodeCH ch1 = utilPiping.createNodeCHbox(ind1);
                newNode = ch1;
            }

            if (indoorCount - indoorIndex > 1)
            {
                MyNodeYP parentYP = utilPiping.createNodeYP(false);
                parentYP.AddChildNode(newNode);
                newNode = parentYP;
            }

            if (parentNode is MyNodeYP)
            {
                MyNodeYP nodeYP = (parentNode as MyNodeYP);
                nodeYP.AddChildNode(newNode);
            }
            else if (parentNode is MyNodeCH)
            {
                MyNodeCH nodeCH = parentNode as MyNodeCH;
                nodeCH.ChildNode = newNode;
            }
            else if (parentNode is MyNodeOut)
            {
                MyNodeOut nodeOut = parentNode as MyNodeOut;
                nodeOut.ChildNode = newNode;
            }

            indoorIndex++;
            if (newNode is MyNodeYP)
            {
                AppendNodeForNormal(isHR, newNode, indoors, indoorIndex);
            }
        }

        private void AppendNodeForSchemaA(bool isHR, Node parentNode, OldModel.RoomIndoor[] indoors, int indoorIndex)
        {
            int indoorCount = indoors.Length;
            if (indoorCount == 0) return;

            Node newNode;
            if (indoorCount > 1 && indoorIndex == 0 && parentNode is MyNodeOut)
            {
                int halfCount = (int)Math.Round(indoorCount / 2d, 0, MidpointRounding.AwayFromZero);
                OldModel.RoomIndoor[] arrPart1 = new OldModel.RoomIndoor[halfCount];
                OldModel.RoomIndoor[] arrPart2 = new OldModel.RoomIndoor[indoorCount - halfCount];

                for (int i = 0; i < halfCount; i++)
                {
                    arrPart1[i] = indoors[i];
                }

                for (int i = halfCount; i < indoorCount; i++)
                {
                    arrPart2[i - halfCount] = indoors[i];
                }

                newNode = utilPiping.createNodeYP(false);

                AppendNodeForSchemaA(isHR, newNode, arrPart1, 0);
                AppendNodeForSchemaA(isHR, newNode, arrPart2, 0);
            }
            else
            {
                MyNodeIn ind1 = utilPiping.createNodeIn(indoors[indoorIndex]);
                newNode = ind1;

                if (isHR)
                {
                    MyNodeCH ch1 = utilPiping.createNodeCHbox(ind1);
                    newNode = ch1;
                }

                if (indoorCount - indoorIndex > 1)
                {
                    MyNodeYP parentYP = utilPiping.createNodeYP(false);
                    parentYP.AddChildNode(newNode);
                    newNode = parentYP;
                }

                indoorIndex++;
            }

            if (parentNode is MyNodeYP)
            {
                MyNodeYP nodeYP = (parentNode as MyNodeYP);
                nodeYP.AddChildNode(newNode);
            }
            else if (parentNode is MyNodeCH)
            {
                MyNodeCH nodeCH = parentNode as MyNodeCH;
                nodeCH.ChildNode = newNode;
            }
            else if (parentNode is MyNodeOut)
            {
                MyNodeOut nodeOut = parentNode as MyNodeOut;
                nodeOut.ChildNode = newNode;
            }

            if (newNode is MyNodeYP && !(newNode as MyNodeYP).IsCP)
            {
                AppendNodeForSchemaA(isHR, newNode, indoors, indoorIndex);
            }
        }

        public void AppendNodeIn(bool isHR, Node parentNode, OldModel.RoomIndoor roomIndoor, out Node newParentNode)
        {
            MyNodeIn ind1 = utilPiping.createNodeIn(roomIndoor);
            Node newChildNode = ind1;

            //RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。为每个此系列的OldModel.Indoor增加一个YP
            MyNodeYP yp = AddYPForTwoInlinkIndoor(ind1);
            if (yp != null)
            {
                newChildNode = yp;
            }

            //parentNode 有可能是OldModel.Outdoor，YP，不可能是CHBox, Multiple CH Box。Delete by Shen Junjie on 2018/01/23
            //if (parentNode is MyNodeMultiCH)
            //{
            //    MyNodeMultiCH parentMCH = (parentNode as MyNodeMultiCH);
            //    parentMCH.AddChildNode(newIndoorNode);
            //    return;
            //}

            //Heat Recovery 一个indoor搭配一个CH Box
            if (isHR)
            {
                MyNodeCH ch1 = utilPiping.createNodeCHbox(ind1);
                newChildNode = ch1;
            }

            newParentNode = parentNode;
            if (parentNode is MyNodeYP)
            {
                //在父YP节点和它的最后一个子节点之间插入一个新YP
                MyNodeYP parentYP = (parentNode as MyNodeYP);
                newParentNode = InsertNodeYP(parentYP, parentYP.ChildNodes[parentYP.ChildCount - 1], newChildNode);
            }
            //parentNode 有可能是OldModel.Outdoor，YP，不可能是CHBox, Multiple CH Box。Delete by Shen Junjie on 2018/01/23
            //else if (parentNode is MyNodeCH)
            //{
            //    MyNodeCH nodeCH = parentNode as MyNodeCH;
            //    if (nodeCH.ChildNode != null)
            //    {
            //        //在父节点和它的子节点之间插入一个YP
            //        MyNodeYP newYP = utilPiping.createNodeYP(false);
            //        newYP.AddChildNode(nodeCH.ChildNode);
            //        newYP.AddChildNode(headNode);

            //        nodeCH.ChildNode = newYP;
            //        headNode = newYP;
            //    }
            //    else
            //    {
            //        nodeCH.ChildNode = headNode;
            //        headNode = parentNode;
            //    }
            //}
            else if (parentNode is MyNodeOut)
            {
                MyNodeOut nodeOut = parentNode as MyNodeOut;
                if (nodeOut.ChildNode != null)
                {
                    //在父节点和它的子节点之间插入一个YP
                    newParentNode = InsertNodeYP(nodeOut, nodeOut.ChildNode, newChildNode);
                }
                else
                {
                    nodeOut.ChildNode = newChildNode;
                }
            }
        }

        public void DeleteNode(MyNode node)
        {
            if (node == null) return;

            MyNode parent = null;

            parent = node.ParentNode;
            if (node.AddFlow != null)  //防止报错
            {
                if (node.Links != null)
                {
                    foreach (Link link in node.Links)
                    {
                        //link.Remove();   // Will take care during implemetauin
                    }
                }
                //node.Remove();   // Will take care during implemetauin
            }
            node.ParentNode = null;

            if (parent == null) return;

            if (parent is MyNodeOut)
            {
                MyNodeOut parentOut = parent as MyNodeOut;
                parentOut.ChildNode = null;
            }
            else if (parent is MyNodeCH)
            {
                MyNodeCH parentCH = parent as MyNodeCH;
                parentCH.ChildNode = null;
                DeleteNode(parentCH);
            }
            else if (parent is MyNodeMultiCH)
            {
                MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                parentMCH.ChildNodes.Remove(node);

                if (parentMCH.ChildNodes.Count == 0)
                {
                    DeleteNode(parentMCH);
                }
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                parentYP.RemoveChildNode(node);
                if (parentYP.ChildCount == 0)
                {
                    DeleteNode(parentYP);
                }
                else if (parentYP.ChildCount == 1)
                {
                    //YP只有一个子节点，则删除此YP
                    Node grandParent = parentYP.ParentNode;
                    Node brother = null;
                    foreach (Node n in parentYP.ChildNodes)
                    {
                        if (n != null && n != node)
                        {
                            brother = n;
                            break;
                        }
                    }
                    if (brother != null)
                    {
                        //把兄弟节点替换YP节点的位置。
                        ReplaceChildNode(grandParent, parentYP, brother);

                        if (brother.AddFlow != null && brother.Links != null)  //防止报错
                        {
                            foreach (Link link in brother.Links)
                            {
                                link.Org = grandParent;
                            }
                        }
                    }
                    DeleteNode(parentYP);
                }
            }
        }

        /// <summary>
        /// RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管  add by Shen Junjie on 2018/01/24
        /// </summary>
        /// <param name="modelHitachi"></param>
        /// <returns></returns>
        public static bool IsTwoInlinkIndoor(string modelHitachi)
        {
            switch (modelHitachi)
            {
                case "RPI-16.0FSN3PE":
                case "RPI-20.0FSN3PE":
                case "RPI-16.0FSN3PE-f":
                case "RPI-20.0FSN3PE-f":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。为每个此系列的OldModel.Indoor增加一个YP add by Shen Junjie on 2018/01/19
        /// </summary>
        /// <param name="indoors"></param>
        /// <param name="nodeOut"></param>
        private void AddYPForTwoInlinkIndoor(List<OldModel.RoomIndoor> indoors, MyNodeOut nodeOut)
        {
            bool isExsits = indoors.Exists(ind =>
            {
                return IsTwoInlinkIndoor(ind.IndoorItem.Model_Hitachi);
            });
            if (!isExsits) return;

            EachNode(nodeOut, node =>
            {
                if (!(node is MyNodeIn)) return;
                MyNodeIn nodeIn = node as MyNodeIn;
                AddYPForTwoInlinkIndoor(nodeIn);
            });
        }

        /// <summary>
        /// RPI-16.0,20.0FSN3PE(-f) 有两条液管和两条气管。为此系列的OldModel.Indoor增加一个YP add by Shen Junjie on 2018/01/22
        /// </summary>
        /// <param name="indoors"></param>
        /// <param name="nodeOut"></param>
        private MyNodeYP AddYPForTwoInlinkIndoor(MyNodeIn nodeIn)
        {
            if (nodeIn == null) return null;

            if (!IsTwoInlinkIndoor(nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi))
                return null;

            while (nodeIn.MyInLinks.Count < 2)
            {
                nodeIn.MyInLinks.Add(utilPiping.createMyLink());
            }

            MyNode parent = nodeIn.ParentNode;

            if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                if (parentYP.ChildCount == 1)
                {
                    //已添加特殊的YP父节点
                    return parentYP;
                }
            }

            MyNodeYP nodeYP = utilPiping.createNodeYP(false);
            nodeYP.AddChildNode(nodeIn);

            ReplaceChildNode(parent, nodeIn, nodeYP);

            return nodeYP;
        }

        /// <summary>
        /// 替换子节点聚集成一个方法，适用各种情况
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="orgChild"></param>
        /// <param name="newChild"></param>
        public static void ReplaceChildNode(Node parent, Node orgChild, Node newChild)
        {
            if (parent == newChild)
                return;

            if (parent is MyNodeOut)
            {
                (parent as MyNodeOut).ChildNode = newChild;
            }
            else if (parent is MyNodeCH)
            {
                (parent as MyNodeCH).ChildNode = newChild;
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                parentYP.ReplaceChildNode(orgChild, newChild);
            }
            else if (parent is MyNodeMultiCH)
            {
                MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                parentMCH.ReplaceChildNode(orgChild, newChild);
            }
        }

        #endregion

        #region Size Up 相关

        //L2 > 40m branch kit size up的逻辑统一用IsBranchKitNeedSizeUp  delete by Shen Junjie 2018/4/26
        ///// Should TeriaryBranch be SizeUp add on 20160529 by Yunxiao Lin
        ///// <summary>
        ///// Should TeriaryBranch be SizeUp
        ///// </summary>
        ///// <param name="branchkit"></param>
        ///// <param name="yp"></param>
        ///// <param name="sysItem"></param>
        ///// <returns></returns>
        //private bool IsTeriaryBranchSizeUp(OldModel.PipingBranchKit branchkit, SystemVRF sysItem)
        //{
        //    if (PipingBLL.IsHeatRecovery(sysItem))
        //        return false;

        //    if (branchkit != null && branchkit.SizeUP == "FALSE")
        //    {
        //        //YVAHP, FSNS,FSNP 需要SizeUp 20170516 by Yunxiao Lin
        //        if (branchkit.UnitType == "YVAHP (Top discharge)"
        //            || branchkit.UnitType == "FSNP (Top discharge)"
        //            || branchkit.UnitType == "FSNP7B (Top discharge)"
        //            || branchkit.UnitType == "FSNP5B (Top discharge)"
        //            || branchkit.UnitType == "FSNS (Top discharge)"
        //            || branchkit.UnitType == "FSNS7B (Top discharge)"
        //            || branchkit.UnitType == "FSNS5B (Top discharge)"
        //            || branchkit.UnitType == "JTOH-BS1 (Top discharge)"
        //            || branchkit.UnitType == "JTOR-BS1 (Top discharge)")
        //        {
        //            //Node pnode = yp.InLink.Org;
        //            //if (pnode != null && pnode is MyNodeYP && (pnode as MyNodeYP).IsFirstYP)
        //            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
        //            if (nodeOut != null)
        //            {
        //                //计算第一分歧管到IDU的距离
        //                foreach (Node node in addFlowPiping.Nodes)
        //                {
        //                    //if (node is MyNodeIn && utilPiping.isContain(yp, node))
        //                    if (node is MyNodeIn)
        //                    {
        //                        //double length = PipingBLL.getLengthBetweenNodesUpward(node, yp);
        //                        double length = PipingBLL.getLengthBetweenNodesUpward(node, nodeOut.ChildNode);
        //                        //如果距离超过40m，则需要SizeUp
        //                        if (length > 40d)
        //                            return true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

        public void IsBranchKitNeedSizeUp(SystemVRF sysItem)
        {
            //重置SizeUp标志 add by Shen Junjie on 2018/6/29
            MyNodeOut nodeOut = sysItem.MyPipingNodeOut;
            if (nodeOut != null)
            {
                EachNode(nodeOut, node =>
                {
                    if (node is MyNode)
                    {
                        (node as MyNode).NeedSizeUP = false;
                    }
                });
            }

            if (sysItem == null || sysItem.OutdoorItem == null) return;
            if (!sysItem.IsInputLengthManually) return;
            //暂时只针对Heat Pump系统  add by Shen Junjie on 2018/3/26
            //确认了L2 > 40m增大的逻辑不适用Heat Recovery系统。comment by Shen Junjie on 2018/7/12
            if (PipingBLL.IsHeatRecovery(sysItem)) return;
            //暂时只针对EU  add by Shen Junjie on 2018/3/26
            //if (!thisProject.RegionCode.StartsWith("EU_")) return;  //L2 > 40m branch kit size up的逻辑统一用同一种  delete by Shen Junjie 2018/4/26
            //根据ODU的UnitType判断是否适用这个逻辑 add by Shen Junjie 2018/4/26
            switch (sysItem.OutdoorItem.Type)
            {
                case "YVAHP (Top discharge)":
                case "FSNP (Top discharge)":
                case "FSNP7B (Top discharge)":
                case "FSNP5B (Top discharge)":
                case "FSNS (Top discharge)":
                case "FSNS7B (Top discharge)":
                case "FSNS5B (Top discharge)":
                case "JTOH-BS1 (Top discharge)":
                case "JTOR-BS1 (Top discharge)":
                //EU的ODU UnitType
                case "FSXNPE (Top discharge)":
                case "FSXNSE (Top discharge)":
                    break;
                default:
                    //参考IsTeriaryBranchSizeUp的逻辑，不属于上述类型的不考虑这个限制。
                    return;
            }

            //40m size up 的需求适用于Main branch和非Main branch的情况 on 2018/6/8
            //if (IsMainBranch(nodeOut.ChildNode))
            //{
            //double lengthFirstBranchToClosestIU = GetPipeLengthFromFirstBranchKitToClosestIU(nodeOut.ChildNode);
            double lengthFirstBranchToFurthestIU;
            IsBranchKitNeedSizeUp(nodeOut.ChildNode, 0, out lengthFirstBranchToFurthestIU);
            //if (lengthFirstBranchToFurthestIU - lengthFirstBranchToClosestIU > 40d)
            //{
            //    errorMsg = "ERROR_PIPING_DIFF_LEN_FURTHEST_CLOSESST_IU";
            //    return;
            //}
            //}
        }

        //add by Shen Junjie on 2018/3/27
        /// <summary>
        /// firsh branch kit到最远的室内机之间的管长是否超过40m
        /// </summary>
        /// <param name="node"></param>
        /// <param name="lengthFirstBranchToParent"></param>
        /// <param name="lengthFirstBranchToFurthestIU"></param>
        private void IsBranchKitNeedSizeUp(Node node, double lengthFirstBranchToParent, out double lengthFirstBranchToFurthestIU)
        {
            lengthFirstBranchToFurthestIU = 0;

            if (node == null || !(node is MyNode))
            {
                return;
            }

            MyNode myNode = node as MyNode;

            double lengthParentToThis = 0; //当前节点到父节点的管长
            foreach (MyLink lk in myNode.MyInLinks)
            {
                lengthParentToThis = Math.Max(lengthParentToThis, lk.Length);
            }

            bool isFirstYP = false;
            double lengthFirstBranchToThis = lengthFirstBranchToParent + lengthParentToThis;
            if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                isFirstYP = nodeYP.IsFirstYP;
                if (isFirstYP)
                {
                    lengthFirstBranchToThis = 0;
                }
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    double maxLength = 0;
                    IsBranchKitNeedSizeUp(nodeYP.ChildNodes[i], lengthFirstBranchToThis, out maxLength);
                    lengthFirstBranchToFurthestIU = Math.Max(lengthFirstBranchToFurthestIU, maxLength);
                }

                //如果距离超过40m，则需要SizeUp
                if (lengthFirstBranchToFurthestIU > 40 && !isFirstYP)  //EU Bug ID 168 不包括1st branch之前的管道 modify by Shen Junjie 2018/4/9
                {
                    nodeYP.NeedSizeUP = true;
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                double maxLength = 0;
                IsBranchKitNeedSizeUp(nodeCH.ChildNode, lengthFirstBranchToThis, out maxLength);
                lengthFirstBranchToFurthestIU = Math.Max(lengthFirstBranchToFurthestIU, maxLength);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    double maxLength = 0;
                    IsBranchKitNeedSizeUp(nodeMCH.ChildNodes[i], lengthFirstBranchToThis, out maxLength);
                    lengthFirstBranchToFurthestIU = Math.Max(lengthFirstBranchToFurthestIU, maxLength);
                }
            }
            else if (node is MyNodeIn)
            {
                lengthFirstBranchToFurthestIU = lengthFirstBranchToThis;
            }

            ////The difference between the piping length from the first branch to the farthest indoor unit 
            ////  and the piping length from the first branch to the closeest indoor unit must be less than 40m.
            //if (lengthFirstBranchToFurthestIU - lengthFirstBranchToClosestIU > 40 && !isFirstYP)
            //{
            //    utilPiping.setItemWarning(node);
            //}
        }

        /// First Branch SizeUp add on 20160516 by Yunxiao Lin
        /// <summary>
        /// Should First Branch be SizeUp
        /// </summary>
        private bool IsFirstPipeNeedSizeUp(OldModel.PipingBranchKit branchkit, MyLink firstPipe, SystemVRF sysItem)
        {
            if (branchkit != null && branchkit.SizeUP == "FALSE") //注意部分机型不需要SizeUp
            {
                //特殊的几种机组类型会有另外的计算方法
                if (branchkit.UnitType == "JDOH (Front flow, 3Ph)"
                    || branchkit.UnitType == "JDOC (Front flow)"
                    || branchkit.UnitType == "FSNMQ (Front flow, 3Ph)"
                    || branchkit.UnitType == "FSNAMQ (Front flow, 3Ph)"
                    //add by Shen Junjie on 2018/11/27
                    || branchkit.UnitType == "HNRQ (Front flow, 1Ph)"
                    || branchkit.UnitType == "HNRQ (Front flow, 3Ph)"
                    || branchkit.UnitType == "JROHQ (Front flow, 1Ph)"
                    || branchkit.UnitType == "JROHQ (Front flow, 3Ph)")
                {
                    //第一分歧管进管长度超过70m
                    if (firstPipe.Length > 70)
                    {
                        return true;
                    }
                }
                else if (branchkit.UnitType.Contains("(Water source)"))
                {
                    //Water Source的等效管长不超过80m
                    if (sysItem.PipeEquivalentLength > 80d)
                    {
                        return true;
                    }
                }
                else if (branchkit.UnitType == "HVN(P/C/C1)/HVRNM2 (Side flow)")  //增加SMZ IVX 机型 20170704 by Yunxiao Lin
                {
                    if (sysItem.IsInputLengthManually && sysItem.PipeEquivalentLength >= 70d)
                        return true;
                }
                else if (sysItem.PipeEquivalentLength > 100d)//一般情况下，如果MaxEqPipeLength>100m，则肯定是要SizeUp
                    return true;

            }
            return false;
        }

        ///// Header Branch sizeUp add on 20160516 by Yunxiao Lin
        ///// 增加YVAHP的特殊处理 Modify on 20160529 by Yunxiao Lin
        ///// <summary>
        ///// Should Header Branch be sizeUp 
        ///// </summary>
        //public bool IsHeaderBranchSizeUp(OldModel.PipingBranchKit headerbranch, MyNodeYP cp, SystemVRF sysItem)
        //{
        //    if (headerbranch != null && headerbranch.SizeUP == "FALSE")
        //    {
        //        //判断梳形管上的每个indoor的capacity和管长
        //        for (int i = 0; i < cp.ChildCount; i++)
        //        {
        //            double length = 0.0d;
        //            MyNodeIn indoor = null;
        //            if (cp.ChildNodes[i] is MyNodeIn)
        //            {
        //                indoor = cp.ChildNodes[i] as MyNodeIn;
        //                length = indoor.InLink.Length;
        //            }
        //            else if (cp.ChildNodes[i] is MyNodeCH)
        //            {
        //                indoor = (cp.ChildNodes[i] as MyNodeCH).ChildNode as MyNodeIn;
        //                length = indoor.InLink.Length + (cp.ChildNodes[i] as MyNodeCH).InLink.Length;
        //            }

        //            if (indoor != null)
        //            {
        //                //如果容量小于或等于5.6kw,管长大于15m,则需要放大一号
        //                //if (indoor.RoomIndooItem.CoolingCapacity <= 5.6d && length > 15 && !headerbranch.UnitType.Contains("YVAHP"))
        //                //管径计算改用室内机的标准容量 20161116 by Yunxiao Lin
        //                if (indoor.RoomIndooItem.IndoorItem.CoolingCapacity <= 5.6d && length > 15 && !headerbranch.UnitType.Contains("YVAHP") && !headerbranch.UnitType.Contains("YVAHR"))
        //                {
        //                    return true;
        //                }
        //                //YVAHP需要特殊处理, Capacity范围为1.8kw~4.4kw 20160529 Yunxiao Lin
        //                //else if (indoor.RoomIndooItem.CoolingCapacity >= 1.8d && indoor.RoomIndooItem.CoolingCapacity <= 4.4d && length > 15 && headerbranch.UnitType.Contains("YVAHP"))
        //                //管径计算改用室内机的标准容量
        //                else if (indoor.RoomIndooItem.IndoorItem.CoolingCapacity >= 1.8d && indoor.RoomIndooItem.IndoorItem.CoolingCapacity <= 4.4d && length > 15 && (headerbranch.UnitType.Contains("YVAHP") || headerbranch.UnitType.Contains("YVAHR")))
        //                    return true;
        //            }
        //        }
        //    }
        //    else if (headerbranch.UnitType == "HVN(P/C/C1)/HVRNM2 Side Up flow)")  //增加SMZ IVX 机型 20170704 by Yunxiao Lin
        //    {
        //        if (sysItem.IsInputLengthManually && sysItem.TotalPipeLength >= 70d)
        //            return true;
        //    }
        //    return false;
        //}

        /// CH-Box sizeUp add on 20160516 by Yunxiao lin
        /// <summary>
        /// Should CH-Box be sizeUp
        /// </summary>
        private bool IsCHBoxSizeUp(OldModel.PipingChangeOverKit changeoverkit, MyNodeCH ch)
        {
            if (changeoverkit != null && changeoverkit.SizeUP != "FALSE")
            {
                if (ch.ChildNode is MyNodeYP)
                {
                    if (ch.CoolingCapacity >= 0.8d * 0.7456998715822702d && ch.CoolingCapacity <= 1.5d * 0.7456998715822702d)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// indoor piping sizeup add on 20160516 by Yunxiao Lin
        /// <summary>
        /// Should IDU piping be sizeUp
        /// </summary>
        private bool IsIndoorSizeUp(MyNodeIn indoor, string factoryCode, string regionCode)
        {
            bool isExceed15 = false;
            foreach (MyLink link in indoor.MyInLinks)
            {
                if (link.Length > 15)
                {
                    isExceed15 = true;
                    break;
                }
            }

            if (isExceed15)
            {
                //indoor 需要先获取原始配管
                string[] cc = GetPipeSizeIDU_SizeUp(regionCode, factoryCode,
                    indoor.RoomIndooItem.IndoorItem.CoolingCapacity,
                    indoor.RoomIndooItem.IndoorItem.Model_Hitachi,
                    JCHVRF.Model.PipingSizeUPType.TRUE.ToString());
                if (cc == null) return false;

                return true;
            }
            return false;
        }

        #endregion

        #region 管径、Branch kit、CH Box 选型

        /// <summary>
        /// 选型时是否使用Horse power作为容量单位
        /// </summary>
        /// <returns></returns>
        private bool IsUseHorsePower(SystemVRF sysItem)
        {
            string unitType = sysItem?.SelOutdoorType;
            //EU特殊处理piping使用HorsePower选型
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                return true;
            }
            else if (thisProject.SubRegionCode == "TW") //台湾地区都用KW
            {
                return false;
            }

            if (thisProject.SubRegionCode == "ANZ")
            {
                if (sysItem.SelOutdoorType == "FSNS (Top discharge)" ||
                    sysItem.SelOutdoorType == "FSNP (Top discharge)" ||
                    sysItem.SelOutdoorType == "FSXNS (Top discharge)" ||
                    sysItem.SelOutdoorType == "FSXNP (Top discharge)")
                {
                    return true;
                }
            }

            //特殊机型使用HorsePower选型
            if (sysItem.SelOutdoorType == "FSNS7B (Top discharge)" ||
                sysItem.SelOutdoorType == "FSNS5B (Top discharge)")
            {
                return true;
            }
            //Start Bug #2428 Fix.
            if (unitType == "FSNS (Top discharge)" ||
                unitType == "FSNP (Top discharge)" ||
                unitType == "FSXNS (Top discharge)" ||
                unitType == "FSXNP (Top discharge)" ||
                unitType == "FSXNSE (Top discharge)" ||
                unitType == "FSXNPE (Top discharge)" ||
                unitType == "FSNS7B (Top discharge)" ||
                unitType == "FSNS5B (Top discharge)" ||
                unitType == "JTOH-BS1 (Top discharge)" ||
                unitType == "JTOR-BS1 (Top discharge)")
            {
                return true;
            }
            //End Bug #2428 Fix.

            return false;
        }

        private bool IsFirstPipeUseHorsePower()
        {
            //EU特殊处理piping使用HorsePower选型
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                return true;
            }
            return false;
        }

        //private double GetPipeLengthFromFirstBranchKitToClosestIU(Node node)
        //{
        //    if (node == null)
        //    {
        //        return 0;
        //    }

        //    double length = 0;
        //    double childLength = double.MaxValue;
        //    foreach (MyLink lk in (node as MyNode).MyLinks)
        //    {
        //        length = Math.Max(length, lk.Length);
        //    }

        //    if (node is MyNodeYP)
        //    {
        //        MyNodeYP nodeYP = node as MyNodeYP;
        //        if (nodeYP.IsFirstYP)
        //        {
        //            length = 0;
        //        }
        //        for (int i = 0; i < nodeYP.ChildCount; i++)
        //        {
        //            childLength = Math.Min(childLength, GetPipeLengthFromFirstBranchKitToClosestIU(nodeYP.ChildNodes[i]));
        //        }
        //    }
        //    else if (node is MyNodeCH)
        //    {
        //        MyNodeCH nodeCH = node as MyNodeCH;
        //        childLength = GetPipeLengthFromFirstBranchKitToClosestIU(nodeCH.ChildNode);
        //    }
        //    else if (node is MyNodeMultiCH)
        //    {
        //        MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
        //        for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
        //        {
        //            childLength = Math.Min(childLength, GetPipeLengthFromFirstBranchKitToClosestIU(nodeMCH.ChildNodes[i]));
        //        }
        //    }
        //    else if (node is MyNodeIn)
        //    {
        //        childLength = 0;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //    return length + childLength;
        //}

        /// 计算第一梳形管的型号及管径，根据当前室外机数据 20170711 by Yunxiao Lin
        /// <summary>
        /// 计算第一梳形管的型号及管径，根据当前室外机数据
        /// </summary>
        /// <param name="nodeYP"></param>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        public OldModel.PipingBranchKit getFirstHeaderBranchPipeCalculation(MyNodeYP nodeYP, SystemVRF sysItem, out PipingErrors errorType)
        {
            errorType = PipingErrors.OK;
            string type = IsHeatRecovery(sysItem) ? "3Pipe" : "2Pipe";

            //第一分歧管可能是梳形管 20170711 by Yunxiao Lin
            //double coolingCapacity = sysItem.OutdoorItem.CoolingCapacity;
            //double horsePower = sysItem.OutdoorItem.Horsepower;

            //ALL_NO_319：HeaderBranch为第一分歧管时，使用IDU total capacity或horsePower选型   add on 20180529 by Vince
            double coolingCapacity = nodeYP.CoolingCapacity;
            OldModel.PipingBranchKit branchKit = null;
            OldModel.PipingBranchKit branchKitNormal = null;

            //EU特殊处理piping使用马力值处理逻辑 add on 20180328 by Vince
            if (IsUseHorsePower(sysItem))
            {
                coolingCapacity = nodeYP.HorsePower;
            }
            branchKit = GetHeaderBranch(type, coolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), nodeYP.ChildCount, sysItem.OutdoorItem.Type, thisProject.SubRegionCode); //20170513 增加 ODU UnitType 参数

            if (branchKit == null)
            {
                //如果FALSE查不到数据，试试NA
                branchKit = GetHeaderBranch(type, coolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), nodeYP.ChildCount, sysItem.OutdoorItem.Type, thisProject.SubRegionCode); //20170513 增加 ODU UnitType 参数
            }
            //判断梳形管需不需要放大一号
            if (nodeYP.NeedSizeUP)// || IsTeriaryBranchSizeUp(branchKit, sysItem))
            {
                branchKit = GetHeaderBranch(type, coolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), nodeYP.ChildCount, sysItem.OutdoorItem.Type, thisProject.SubRegionCode); //20170513 增加 ODU UnitType 参数
            }


            if (branchKit == null)
            {
                //还查不到就抛出错误 20160927 by Yunxiao Lin
                //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                errorType = PipingErrors.NO_MATCHED_BRANCH_KIT; //-18
                utilPiping.setItemWarning(nodeYP);
                return null;
            }
            else
            {
                branchKitNormal = branchKit;
                string model = branchKit.Model_York;
                if (isHitachi)
                    model = branchKit.Model_Hitachi;
                MyLink myLink = nodeYP.MyInLinks[0];
                string specG_h = "";
                string specG_l = "";
                string specL = "";
                specL = branchKit.LiquidPipe.Trim();
                //Heat Recovery 只有3Pipe的分歧管，如果当前配管只有2管，取到管径后需要手动设置High Pressure Gass pipe或Low Pressure Gass pipe为"-"
                if (IsHeatRecovery(sysItem) && branchKit.Type == "3Pipe" && nodeYP.IsCoolingonly)
                    specG_h = "-";
                else
                    specG_h = branchKit.HighPressureGasPipe.Trim();

                specG_l = branchKit.LowPressureGasPipe.Trim();

                nodeYP.Model = model;
                myLink.SpecL = specL; // 计算追加制冷剂时需要汇总管径规格
                myLink.SpecG_h = specG_h;
                myLink.SpecG_l = specG_l;
                myLink.SpecL_Normal = branchKitNormal.LiquidPipe.Trim(); // 计算追加制冷剂时需要汇总管径规格
                myLink.SpecG_h_Normal = branchKitNormal.HighPressureGasPipe.Trim();
                myLink.SpecG_l_Normal = branchKitNormal.LowPressureGasPipe.Trim();
            }

            if (branchKit != null)
            {
                //注意假如第一分歧管是HeaderBranch，管径使用firstbranch的数据。20180504 by Yunxiao Lin
                OldModel.PipingBranchKit firstBranch = getFirstPipeCalculation(nodeYP, sysItem, out errorType);
                if (firstBranch != null)
                {
                    //重新查询firstBranch之后，当前的nodeYP属性被替换成了firstbranch的，需要将model替换成HeaderBranch的。20180504 by Yunxiao Lin
                    string model = branchKit.Model_York;
                    if (isHitachi)
                        model = branchKit.Model_Hitachi;
                    nodeYP.Model = model;
                    //更新HeaderBranch的管径，输出报表用。 20180504 by Yunxiao Lin
                    branchKit.HighPressureGasPipe = firstBranch.HighPressureGasPipe;
                    branchKit.LowPressureGasPipe = firstBranch.LowPressureGasPipe;
                    branchKit.LiquidPipe = firstBranch.LiquidPipe;
                }
            }
            return branchKit;
        }

        /// 计算第一个连接管以及第一分歧管的信息，取决于当前室外机的数据
        /// <summary>
        /// 计算第一个连接管以及第一分歧管的信息，取决于当前室外机的数据
        /// </summary>
        /// <param name="myNodeOut"></param>
        /// <param name="firstLink"></param>
        public OldModel.PipingBranchKit getFirstPipeCalculation(MyNode myNode, SystemVRF sysItem, out PipingErrors errorType)
        {
            errorType = PipingErrors.OK;
            string factoryCode = PipingBLL.GetFactoryCode(sysItem);
            string type = PipingBLL.IsHeatRecovery(sysItem) ? "3Pipe" : "2Pipe";

            // TODO：判断是否需要放大一号，需要加入判断代码
            //bool isSizeUP = sysItem.PipeEquivalentLength > 100;
            OldModel.Outdoor outItem = sysItem.OutdoorItem;
            MyLink myLink = myNode.MyInLinks[0];
            string specL = "", specG_h = "", specG_l = "";

            // 不放大一号，直接根据室外机的容量来选择
            // 注意FSVN1Q, FSYN1Q, JCOC, JDOH(FSNMQ除外)的室外机选择第一分歧管不需要考虑容量限制，JDOH由于比较特殊(有两个系列的外机)，暂时还是参考容量限制 20160704 Yunxiao Lin
            OldModel.PipingBranchKit branchKit = null;
            OldModel.PipingBranchKit branchKitNormal = null;
            //bool isMal = outItem.RegionCode == "MAL";
            // 玻利维亚的York产品线和马来西亚的一样 20170629 by Yunxiao lin
            // Wuxi JROHQ 和 JVOHQ 和马来的其他产品不同，需要特殊判断 20180213 by Yunxiao Lin
            bool isMal = ((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV") && thisProject.BrandCode == "Y" && !outItem.Series.Contains("JROHQ") && !outItem.Series.Contains("JVOHQ")); //只有马来西亚的YORK品牌分歧管型号会以QE结尾。 20161023 by Yunxiao Lin
            if (outItem.Type.Contains("FSVN1Q") || outItem.Type.Contains("FSYN1Q") || outItem.Type.Contains("JCOC"))
            {
                branchKit = GetFirstBranchKit(factoryCode, type, outItem.Type, 0, OldModel.PipingSizeUPType.FALSE.ToString(), isMal, 0, outItem.RegionCode);
                if (branchKit == null)
                    branchKit = GetFirstBranchKit(factoryCode, type, outItem.Type, 0, OldModel.PipingSizeUPType.NA.ToString(), isMal, 0, outItem.RegionCode);
                branchKitNormal = branchKit;
                //判断是否需要放大一号
                if (myNode.NeedSizeUP || IsFirstPipeNeedSizeUp(branchKit, myLink, sysItem))
                {
                    branchKit = GetFirstBranchKit(factoryCode, type, outItem.Type, 0, OldModel.PipingSizeUPType.TRUE.ToString(), isMal, 0, outItem.RegionCode);
                }
            }
            else
            {
                //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径 by Shen Junjie on 20170409
                //当室内机总容量大于室外机容量时，取室内机总容量作为参数 20160921 by Yunxiao Lin
                double capacity = outItem.CoolingCapacity;
                //EU特殊处理piping使用马力值处理逻辑 add on 20180328 by Vince
                if (IsFirstPipeUseHorsePower())
                {
                    capacity = outItem.Horsepower;
                }
                ////double indoor_capacity = sysItem.GetTotalIndoorEstCapacity(ref thisProject);
                ////与排管相关的计算都要使用标准容量 20161116 by Yunxiao Lin
                //double indoor_capacity = sysItem.GetTotalIndoorRatedCapacity(ref thisProject);
                //if (capacity < indoor_capacity)
                //    capacity = indoor_capacity;
                //branchKit = pipBll.GetFirstBranchKit(factoryCode, type, outItem.Type, outItem.CoolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), isMal);
                //查找Firstbranch 增加IVX IDU数量的处理 20170704 by Yunxiao Lin
                int IDUNum = 0;
                int tempNum = sysItem.GetIndoorCount(thisProject);
                if (sysItem.OutdoorItem.Type == "HVN(P/C/C1)/HVRNM2 (Side flow)" && (sysItem.PipingLayoutType != OldModel.PipingLayoutTypes.Normal || tempNum == 2) && tempNum != 3)
                    IDUNum = tempNum;

                branchKit = GetFirstBranchKit(factoryCode, type, outItem.Type, capacity, OldModel.PipingSizeUPType.FALSE.ToString(), isMal, IDUNum, outItem.RegionCode);
                if (branchKit == null)
                    //branchKit = pipBll.GetFirstBranchKit(factoryCode, type, outItem.Type, outItem.CoolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), isMal);
                    branchKit = GetFirstBranchKit(factoryCode, type, outItem.Type, capacity, OldModel.PipingSizeUPType.NA.ToString(), isMal, IDUNum, outItem.RegionCode);

                branchKitNormal = branchKit;
                //判断是否需要放大一号
                if (myNode.NeedSizeUP || IsFirstPipeNeedSizeUp(branchKit, myLink, sysItem))
                {
                    //branchKit = pipBll.GetFirstBranchKit(factoryCode, type, outItem.Type, outItem.CoolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), isMal);
                    branchKit = GetFirstBranchKit(factoryCode, type, outItem.Type, capacity, OldModel.PipingSizeUPType.TRUE.ToString(), isMal, IDUNum, outItem.RegionCode);
                }
            }
            if (branchKit == null)
            {
                //还查不到就抛出错误 20160927 by Yunxiao Lin
                //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                errorType = PipingErrors.NO_MATCHED_BRANCH_KIT;//-18
                utilPiping.setItemWarning(myNode);
                return null;
            }

            if (myNode is MyNodeYP)
            {
                MyNodeYP nodeYP = myNode as MyNodeYP;
                if (!nodeYP.IsCP)
                {
                    if (thisProject.BrandCode == "H")
                        nodeYP.Model = branchKit.Model_Hitachi;
                    else
                        nodeYP.Model = branchKit.Model_York;
                }
            }

            specL = branchKit.LiquidPipe.Trim();
            specG_h = branchKit.HighPressureGasPipe.Trim();
            specG_l = branchKit.LowPressureGasPipe.Trim();
            myLink.SpecL = specL; // 计算追加制冷剂时需要汇总管径规格
            myLink.SpecG_h = specG_h;
            myLink.SpecG_l = specG_l;
            myLink.SpecL_Normal = branchKitNormal.LiquidPipe.Trim(); // 计算追加制冷剂时需要汇总管径规格
            myLink.SpecG_h_Normal = branchKitNormal.HighPressureGasPipe.Trim();
            myLink.SpecG_l_Normal = branchKitNormal.LowPressureGasPipe.Trim();

            return branchKit;
            //drawTextToYPNode(nodeYP);
        }
        /// <summary>
        /// 当“第一分歧管以后的管径或分歧管型号”大于“第一分歧管的管径或型号”时的处理规则
        /// 而另外一部分要求减小1st branch后面的其它multi-kit的管径
        /// </summary>
        public enum L2SizeDownRules
        {
            /// <summary>
            /// Always Size up Main pipe and make "Main pipe = After 1st branch pipe"
            /// </summary>
            AlwaysSizeUp1stBranch,

            /// <summary>
            /// 某些时候可以缩小后面的管径和型号
            /// </summary>
            SometimesSizeDown2ndBranch
        }
        public L2SizeDownRules GetL2SizeDownRule(SystemVRF sysItem)
        {
            MyProductTypeDAL ptDAL = new MyProductTypeDAL();
            OldModel.MyProductType productType = ptDAL.GetItem(thisProject.BrandCode, thisProject.SubRegionCode, sysItem.OutdoorItem.ProductType);
            switch (productType.L2SizeDownRule)
            {
                case "Y":
                    return L2SizeDownRules.SometimesSizeDown2ndBranch;
                default:
                    return L2SizeDownRules.AlwaysSizeUp1stBranch;
            }
        }

        /// <summary>
        /// 计算各个节点的总容量以及分歧管、连接管的气管、液管型号
        /// 如果后面的节点的型号、管径比前面的大，则改成前面的型号、管径
        /// Add by Shen Junjie on 20170419
        /// </summary>
        /// <param name="node"></param>
        /// <param name="factoryCode"></param>
        /// <param name="type"> 2pipe | 3pipe </param>
        /// <param name="ParentIsOutdoor"></param>
        public void getSumCalculationInversion(OldModel.PipingBranchKit previousBranchKit, Node parent, Node node, SystemVRF sysItem, bool isBehindCHBox, out PipingErrors errorType,L2SizeDownRules rule)
        {
            PipingErrors errorType1;
            errorType = PipingErrors.OK;
            if (node == null) return;

            double coolingCapacity;
            //double horsePower;

            //判断当前系统是Heat Recovery(R) 或 Heat Pump(H)
            bool isHR = PipingBLL.IsHeatRecovery(sysItem);
            string modelFull = sysItem.OutdoorItem.ModelFull;
            string factoryCode = modelFull.Substring(modelFull.Length - 1, 1);  //第十四位表示工厂代码
            string unitType = sysItem.OutdoorItem.Type;
            string type = "2Pipe";
            string series = sysItem.OutdoorItem.Series;
            bool isUseHorsePower = IsUseHorsePower(sysItem);

            if (isHR && !isBehindCHBox)
            {
                type = "3Pipe";
            }

            string specG_h = "";
            string specG_l = "";
            string specL = "";
            if (node is MyNodeYP)
            {
                #region YP
                MyNodeYP nodeYP = node as MyNodeYP;

                //if (HeatType == "R" && nodeYP.IsCoolingonly && type=="2Pipe")
                //    type = "3Pipe";
                MyLink myLink = nodeYP.MyInLinks[0];

                coolingCapacity = nodeYP.CoolingCapacity;
                //EU特殊处理piping使用马力值处理逻辑 add on 20180420 by Shen Junjie
                if (isUseHorsePower)
                {
                    coolingCapacity = nodeYP.HorsePower;
                }

                OldModel.PipingBranchKit branchKit = previousBranchKit;
                OldModel.PipingBranchKit branchKitNormal = branchKit;

                if (!nodeYP.IsFirstYP)
                {
                    if (nodeYP.IsCP)
                    {
                        #region CP 梳形分歧管
                        branchKit = GetHeaderBranch(type, coolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), nodeYP.ChildCount, unitType, thisProject.SubRegionCode); //20170513 增加 ODU UnitType 参数

                        if (branchKit == null)
                        {
                            //如果FALSE查不到数据，试试NA
                            branchKit = GetHeaderBranch(type, coolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), nodeYP.ChildCount, unitType, thisProject.SubRegionCode); //20170513 增加 ODU UnitType 参数
                        }
                        if (branchKit == null)
                        {
                            //还查不到就抛出错误
                            //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                            errorType = PipingErrors.NO_MATCHED_BRANCH_KIT;//-18
                            utilPiping.setItemWarning(nodeYP);
                            return;
                        }
                        branchKitNormal = branchKit;
                        //判断梳形管需不需要放大一号
                        if (nodeYP.NeedSizeUP)// || IsTeriaryBranchSizeUp(branchKit, sysItem))
                        {
                            branchKit = GetHeaderBranch(type, coolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), nodeYP.ChildCount, unitType, thisProject.SubRegionCode); //20170513 增加 ODU UnitType 参数
                        }
                        #endregion
                    }
                    else
                    {
                        #region Branch Kit Y形分歧管
                        //bool isMal = curSystemItem.OutdoorItem.RegionCode == "MAL";
                        // 玻利维亚的York产品线和马来西亚的一样  20170629 by yunxiao Lin
                        // Wuxi JROHQ 和 JVOHQ 和马来的其他产品不同，需要特殊判断 20180213 by Yunxiao Lin
                        bool isMal = ((sysItem.OutdoorItem.RegionCode == "MAL" || sysItem.OutdoorItem.RegionCode == "LA_BV") && thisProject.BrandCode == "Y" && !sysItem.OutdoorItem.Series.Contains("JROHQ") && !sysItem.OutdoorItem.Series.Contains("JVOHQ")); //只有马来西亚的YORK品牌分歧管型号会以QE结尾。 20161023 by Yunxiao Lin
                        // 注意FSVN1Q, FSYN1Q, JCOC, JDOH(FSNMQ除外)的室外机选择分歧管不需要考虑容量限制，JDOH由于比较特殊(有两个系列的外机)，暂时还是参考容量限制 20160704 Yunxiao Lin
                        if (unitType.Contains("FSVN1Q") || unitType.Contains("FSYN1Q") || unitType.Contains("JCOC"))
                        {
                            coolingCapacity = 0;
                        }
                        //IVX SMZ 查询branch kit 需要IDUNUmber参数 20170704 by Yunxiao Lin
                        int IDUNum = 0;
                        int tempNum = sysItem.GetIndoorCount(thisProject);
                        if (unitType == "HVN(P/C/C1)/HVRNM2 (Side flow)" && tempNum != 3 && sysItem.PipingLayoutType != OldModel.PipingLayoutTypes.Normal)
                            IDUNum = tempNum;

                        //EU特殊处理piping使用马力值处理逻辑 add on 20180328 by Vince
                        branchKit = GetTertiaryBranchKit(factoryCode, type, unitType, coolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), isMal, IDUNum, sysItem.OutdoorItem.RegionCode);
                        if (branchKit == null)
                        {
                            //如果FALSE查不到数据，试试NA
                            branchKit = GetTertiaryBranchKit(factoryCode, type, unitType, coolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), isMal, IDUNum, sysItem.OutdoorItem.RegionCode);
                        }

                        if (branchKit == null)
                        {
                            //还查不到就抛出错误
                            //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                            errorType = PipingErrors.NO_MATCHED_BRANCH_KIT;//-18
                            utilPiping.setItemWarning(nodeYP);
                            return;
                        }
                        branchKitNormal = branchKit;
                        //YVAHP的分歧管可能需要放大一号
                        if (nodeYP.NeedSizeUP)// || IsTeriaryBranchSizeUp(branchKit, sysItem))
                        {
                            //EU特殊处理piping使用马力值处理逻辑 add on 20180328 by Vince
                            branchKit = GetTertiaryBranchKit(factoryCode, type, unitType, coolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), isMal, IDUNum, sysItem.OutdoorItem.RegionCode);
                        }

                        if (branchKit == null)
                        {
                            errorType = PipingErrors.NO_MATCHED_BRANCH_KIT;//-18
                            utilPiping.setItemWarning(nodeYP);
                            return;
                        }


                        specL = branchKit.LiquidPipe.Trim();
                        //Heat Recovery 只有3Pipe的分歧管，如果当前配管只有2管，取到管径后需要手动设置High Pressure Gass pipe或Low Pressure Gass pipe为"-"
                        if (isHR && branchKit.Type == "3Pipe" && nodeYP.IsCoolingonly)
                            specG_h = "-";
                        else
                            specG_h = branchKit.HighPressureGasPipe.Trim();

                        specG_l = branchKit.LowPressureGasPipe.Trim();

                        //branch kit 应根据后面branch 增大而不是缩小  delete by Shen Junjie 2018/4/26
                        //if (inverse && previousBranchKit != null)
                        //{
                        //    //缩小后面branch Kit 管径的逻辑
                        //    //比较父YP节点的型号和管径，如果比父节点大，则需要缩小
                        //    double specL1, specL2, specG_h1, specG_h2, specG_l1, specG_l2;
                        //    string model1 = isHitachi ? previousBranchKit.Model_Hitachi : previousBranchKit.Model_York;
                        //    string model2 = isHitachi ? branchKit.Model_Hitachi : branchKit.Model_York;
                        //    int HP1 = 0, HP2 = 0; //从Model 中获取匹数
                        //    Match match1 = Regex.Match(model1, "\\d+");
                        //    if (match1 != null)
                        //    {
                        //        HP1 = int.Parse(match1.Value);
                        //    }
                        //    Match match2 = Regex.Match(model2, "\\d+");
                        //    if (match1 != null)
                        //    {
                        //        HP2 = int.Parse(match2.Value);
                        //    }
                        //    if (HP1 < HP2
                        //        || (double.TryParse(previousBranchKit.LiquidPipe, out specL1) && double.TryParse(specL, out specL2)
                        //            && specL1 < specL2)
                        //        || (double.TryParse(previousBranchKit.HighPressureGasPipe, out specG_h1) && double.TryParse(specG_h, out specG_h2)
                        //            && specG_h1 < specG_h2)
                        //        || (double.TryParse(previousBranchKit.LowPressureGasPipe, out specG_l1) && double.TryParse(specG_l, out specG_l2)
                        //            && specG_l1 < specG_l2))
                        //    {
                        //        branchKit = ShrinkTertiaryBranchKit(previousBranchKit, isHitachi, nodeYP.IsCoolingonly, branchKit.SizeUP, isMal, IDUNum);
                        //        branchKitNormal = branchKit;
                        //    }
                        //}
                        #endregion
                    }
                    if (branchKit == null)
                    {
                        errorType = PipingErrors.NO_MATCHED_BRANCH_KIT;//-18
                        utilPiping.setItemWarning(nodeYP);
                        return;
                    }

                    string model = branchKit.Model_York;
                    if (isHitachi)
                        model = branchKit.Model_Hitachi;

                    specL = branchKit.LiquidPipe.Trim();
                    //Heat Recovery 只有3Pipe的分歧管，如果当前配管只有2管，取到管径后需要手动设置High Pressure Gass pipe或Low Pressure Gass pipe为"-"
                    if (isHR && branchKit.Type == "3Pipe" && nodeYP.IsCoolingonly)
                        specG_h = "-";
                    else
                        specG_h = branchKit.HighPressureGasPipe.Trim();

                    specG_l = branchKit.LowPressureGasPipe.Trim();

                    nodeYP.Model = model;
                    myLink.SpecL = specL; // 计算追加制冷剂时需要汇总管径规格
                    myLink.SpecG_h = specG_h;
                    myLink.SpecG_l = specG_l;
                    myLink.SpecL_Normal = branchKitNormal.LiquidPipe.Trim(); // 计算追加制冷剂时需要汇总管径规格
                    myLink.SpecG_h_Normal = branchKitNormal.HighPressureGasPipe.Trim();
                    myLink.SpecG_l_Normal = branchKitNormal.LowPressureGasPipe.Trim();

                    L2SizeDownRules VarL2SizeDownRule = GetL2SizeDownRule(sysItem);
                    AdjustPipeSizeOfBranchKitsAfterFirstBranch(sysItem.MyPipingNodeOut, nodeYP, VarL2SizeDownRule);
                }
                string[] firstPipeSizeNormal = null;
                if (nodeYP.IsFirstYP)
                {
                    //先记录first branch的Spec_Normal  add by Shen Junjie on 2018/2/28
                    firstPipeSizeNormal = new string[]
                    {
                        myLink.SpecL_Normal,
                        myLink.SpecG_h_Normal,
                        myLink.SpecG_l_Normal
                    };
                    //first branch的Spec_Normal存放L1 Size Up处理过的管径，作为后面Size Down逻辑的基准管径  add by Shen Junjie on 2018/2/28
                    myLink.SpecL_Normal = myLink.SpecL;
                    myLink.SpecG_h_Normal = myLink.SpecG_h;
                    myLink.SpecG_l_Normal = myLink.SpecG_l;
                }
                //bug 3489
                var L2SizeDownRule = GetL2SizeDownRule(sysItem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
                //bug 3489
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    getSumCalculationInversion(branchKit, nodeYP, nodeYP.ChildNodes[i], sysItem, isBehindCHBox, out errorType1, L2SizeDownRule);
                    if (errorType == PipingErrors.OK)
                    {
                        errorType = errorType1;
                    }
                }
                #endregion
            }
            else if (node is MyNodeMultiCH)
            {
                #region Multi CH Box

                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                coolingCapacity = nodeMCH.CoolingCapacity;
                //EU特殊处理piping使用马力值处理逻辑 add on 20180420 by Shen Junjie
                if (isUseHorsePower)
                {
                    coolingCapacity = nodeMCH.HorsePower;
                }
                MyLink myLink = nodeMCH.MyInLinks[0];
                int branchCount = nodeMCH.ChildNodes.Count;
                double maxBranchCapacity = 0;
                int maxBranchIDU = 0;
                //bug 3489
                var L2SizeDownRule = GetL2SizeDownRule(sysItem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
                //bug 3489
                for (int i = 0; i < branchCount; i++)
                {
                    Node child = nodeMCH.ChildNodes[i];
                    maxBranchIDU = Math.Max(maxBranchIDU, getIndoorCount(child));
                    getSumCalculationInversion(null, nodeMCH, child, sysItem, true, out errorType1, L2SizeDownRule);
                    if (errorType == PipingErrors.OK)
                    {
                        errorType = errorType1;
                    }

                    if (child is MyNodeIn)
                    {
                        MyNodeIn nodeIn = child as MyNodeIn;
                        double childCapacity = nodeIn?.RoomIndooItem?.IndoorItem?.CoolingCapacity ?? 0;
                        double childHP = nodeIn?.RoomIndooItem?.IndoorItem?.Horsepower ?? 0;
                        if (isUseHorsePower)
                        {
                            childCapacity = childHP;
                        }
                        //当multi CH-Box分支上单独连接8HP或10HP室内机时，可以不受分支最大容量限制 20180822 by Yunxiao Lin
                        if (Convert.ToInt32(childHP) != 8 && Convert.ToInt32(childHP) != 10)
                            maxBranchCapacity = Math.Max(maxBranchCapacity, childCapacity);
                    }
                    else if (child is MyNodeYP)
                    {
                        MyNodeYP nodeYP = child as MyNodeYP;
                        maxBranchCapacity = Math.Max(maxBranchCapacity, isUseHorsePower ? nodeYP.HorsePower : nodeYP.CoolingCapacity);
                    }
                }

                OldModel.PipingMultiCHBox multiCHBox = GetMultiCHBox(thisProject.SubRegionCode, series, coolingCapacity, false,
                    branchCount, maxBranchCapacity, maxBranchIDU);
                if (multiCHBox == null)
                {
                    nodeMCH.Model = "";
                    myLink.SpecL = "";
                    myLink.SpecG_h = "";
                    myLink.SpecG_l = "";
                    myLink.SpecL_Normal = "";
                    myLink.SpecG_h_Normal = "";
                    myLink.SpecG_l_Normal = "";
                    //查不到就抛出错误
                    //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                    errorType = PipingErrors.NO_MATCHED_MULTI_CHBOX; //-20
                    utilPiping.setItemWarning(nodeMCH);
                    return;
                }
                else
                {
                    nodeMCH.Model = multiCHBox.Model_York;
                    if (isHitachi)
                        nodeMCH.Model = multiCHBox.Model_Hitachi;

                    myLink.SpecL = multiCHBox.LiquidPipe.Trim();
                    myLink.SpecG_h = multiCHBox.HighPressureGasPipe.Trim();
                    myLink.SpecG_l = multiCHBox.LowPressureGasPipe.Trim();
                    myLink.SpecL_Normal = multiCHBox.LiquidPipe.Trim();
                    myLink.SpecG_h_Normal = multiCHBox.HighPressureGasPipe.Trim();
                    myLink.SpecG_l_Normal = multiCHBox.LowPressureGasPipe.Trim();

                    nodeMCH.MaxTotalCHIndoorPipeLength = multiCHBox.MaxTotalCHIndoorPipeLength;
                    nodeMCH.MaxTotalCHIndoorPipeLength_MaxIU = multiCHBox.MaxTotalCHIndoorPipeLength_MaxIU;
                    nodeMCH.MaxIndoorCount = multiCHBox.MaxIU;

                    nodeMCH.PowerSupply = multiCHBox.PowerSupply;
                    nodeMCH.PowerConsumption = multiCHBox.PowerConsumption;
                    nodeMCH.PowerCurrent = multiCHBox.PowerCurrent;
                    nodeMCH.MaxBranches = multiCHBox.MaxBranches;
                    nodeMCH.MaxCapacityPerBranch = multiCHBox.MaxCapacityPerBranch;
                    nodeMCH.MaxIUPerBranch = multiCHBox.MaxIUPerBranch;
                    nodeMCH.PowerLineType = multiCHBox.PowerLineType;
                }

                //当Multi CHBox 直接连接到室外机，管径用First Branch表的管径
                if (parent is MyNodeOut)
                {
                    getFirstPipeCalculation(nodeMCH, sysItem, out errorType);
                }

                #endregion
            }
            else if (node is MyNodeCH)
            {
                #region CHbox 20160406 clh
                MyNodeCH nodeCH = node as MyNodeCH;
                coolingCapacity = nodeCH.CoolingCapacity;
                if (isUseHorsePower)
                {
                    coolingCapacity = nodeCH.HorsePower;
                }
                MyLink myLink = nodeCH.MyInLinks[0];

                int IUCount = getIndoorCount(nodeCH);
                //EU特殊处理piping使用马力值处理逻辑 add on 20180328 by Vince
                OldModel.PipingChangeOverKit chKit = new OldModel.PipingChangeOverKit();
                OldModel.PipingChangeOverKit chKitNormal = new OldModel.PipingChangeOverKit();
                chKit = GetChangeOverKit(factoryCode, coolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), IUCount, series, thisProject.SubRegionCode); // 只连接一个室内机，管径不放大
                if (chKit == null) //FALSE找不到，试试NA
                {
                    chKit = GetChangeOverKit(factoryCode, coolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), IUCount, series, thisProject.SubRegionCode);
                }
                if (chKit == null)
                {
                    //还查不到就抛出错误
                    //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                    errorType = PipingErrors.NO_MATCHED_CHBOX; //-19
                    utilPiping.setItemWarning(nodeCH);
                    return;
                }
                chKitNormal = chKit;
                //判断CH-Box是否需要SizeUp
                if (IsCHBoxSizeUp(chKit, nodeCH))
                {
                    //重新获取CH-Box管型
                    chKit = GetChangeOverKit(factoryCode, coolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), IUCount, series, thisProject.SubRegionCode);
                    if (chKit == null)
                    {
                        //没有size up的数据就用NA/FALSE的数据 add on 2018/7/19 by Shen Junjie   
                        chKit = chKitNormal;
                    }
                }

                nodeCH.Model = chKit.Model_York;
                if (isHitachi)
                    nodeCH.Model = chKit.Model_Hitachi;

                myLink.SpecL = "";
                myLink.SpecG_h = chKit.HighPressureGasPipe.Trim();
                myLink.SpecG_l = chKit.LowPressureGasPipe.Trim();
                myLink.SpecL_Normal = myLink.SpecL;
                myLink.SpecG_h_Normal = chKitNormal.HighPressureGasPipe.Trim();
                myLink.SpecG_l_Normal = chKitNormal.LowPressureGasPipe.Trim();
                //add on 20160515 by Yunxiao Lin
                nodeCH.MaxTotalCHIndoorPipeLength = chKit.MaxTotalCHIndoorPipeLength;
                nodeCH.MaxTotalCHIndoorPipeLength_MaxIU = chKit.MaxTotalCHIndoorPipeLength_MaxIU;
                nodeCH.MaxIndoorCount = chKit.MaxIU;
                //add on 20171221 by Shen Junjie
                nodeCH.PowerSupply = chKit.PowerSupply;
                nodeCH.PowerLineType = chKit.PowerLineType;
                nodeCH.PowerCurrent = chKit.PowerCurrent; //add on 20180615 by Shen Junjie
                nodeCH.PowerConsumption = chKit.PowerConsumption; //add on 20180615 by Shen Junjie

                //bug 3489
                var L2SizeDownRule = GetL2SizeDownRule(sysItem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
                //bug 3489

                if (nodeCH.ChildNode is MyNodeIn)
                {
                    MyNodeIn nodeIn = (nodeCH.ChildNode as MyNodeIn);
                    myLink.SpecL = nodeIn.RoomIndooItem.IndoorItem.LiquidPipe.Trim();
                    myLink.SpecL_Normal = myLink.SpecL;
                    getSumCalculationInversion(null, nodeCH, nodeIn, sysItem, true, out errorType1, L2SizeDownRule); //OldModel.Indoor如果需要放大一号，管径会变化
                    if (errorType == PipingErrors.OK)
                    {
                        errorType = errorType1;
                    }
                }
                else if (nodeCH.ChildNode is MyNodeYP)
                {
                    //add on 20160516 by Yunxiao Lin
                    MyNodeYP nodeYP = nodeCH.ChildNode as MyNodeYP;
                    getSumCalculationInversion(null, nodeCH, nodeYP, sysItem, true, out errorType1, L2SizeDownRule);
                    if (errorType == PipingErrors.OK)
                    {
                        errorType = errorType1;
                    }
                    myLink.SpecL = nodeYP.MyInLinks[0].SpecL; //CH-Box内部没有液管，液管只是从其旁边通过，不会改变管径。
                    myLink.SpecL_Normal = myLink.SpecL;
                }
                #endregion
            }
            else if (node is MyNodeIn)
            {
                #region OldModel.Indoor Unit
                //判断OldModel.Indoor的配管是否需要SizeUP
                MyNodeIn indoor = node as MyNodeIn;
                coolingCapacity = indoor.RoomIndooItem.IndoorItem.CoolingCapacity;
                if (IsIndoorSizeUp(indoor, factoryCode, thisProject.RegionCode))
                {
                    //重新计算OldModel.Indoor的配管尺寸
                    string[] cc = GetPipeSizeIDU_SizeUp(thisProject.RegionCode, factoryCode, coolingCapacity, indoor.RoomIndooItem.IndoorItem.Model_Hitachi, OldModel.PipingSizeUPType.TRUE.ToString());
                    if (cc == null)
                    {
                        //还查不到就抛出错误
                        //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
                        errorType = PipingErrors.NO_MATCHED_SIZE_UP_IU; //-21
                        utilPiping.setItemWarning(node);
                        return;
                    }
                    foreach (MyLink myLink in indoor.MyInLinks)
                    {
                        if (indoor.IsCoolingonly) //当cooling only时，气管为低压气管
                        {
                            myLink.SpecG_h = "-";
                            myLink.SpecG_l = cc[1];
                        }
                        else
                        {
                            myLink.SpecG_h = cc[1];
                            myLink.SpecG_l = "-";
                        }

                        myLink.SpecL = cc[0];
                        myLink.SpecL_Normal = myLink.SpecL;
                        myLink.SpecG_h_Normal = myLink.SpecG_h;
                        myLink.SpecG_l_Normal = myLink.SpecG_l;
                    }
                }
                else
                {
                    //如果不需要增大一号，则重置成室内机标准表的管径（可能已经被增大了）
                    specG_h = indoor.RoomIndooItem.IndoorItem.GasPipe.Trim();
                    specL = indoor.RoomIndooItem.IndoorItem.LiquidPipe.Trim();
                    foreach (MyLink myLink in indoor.MyInLinks)
                    {
                        if (indoor.IsCoolingonly) //当cooling only时，气管为低压气管
                        {
                            myLink.SpecG_h = "-";
                            myLink.SpecG_l = specG_h;
                        }
                        else
                        {
                            myLink.SpecG_h = specG_h;
                            myLink.SpecG_l = "-";
                        }
                        myLink.SpecL = specL;
                        myLink.SpecL_Normal = myLink.SpecL;
                        myLink.SpecG_h_Normal = myLink.SpecG_h;
                        myLink.SpecG_l_Normal = myLink.SpecG_l;
                    }
                }
                #endregion
            }
        }
        public void DoPipingInfo(PipingBLL pipBll, JCHVRF.Model.NextGen.MyNodeOut nodeOut, JCHVRF.Model.NextGen.SystemVRF currentSystem, out PipingErrors errorType)
        {
            errorType = PipingErrors.OK;
            if (nodeOut.ChildNode == null)
            {
                return;
            }
            pipBll.GetSumCapacity(nodeOut.ChildNode);
            pipBll.IsBranchKitNeedSizeUp(currentSystem);
            JCHVRF.Model.PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = nodeOut.ChildNode as JCHVRF.Model.NextGen.MyNodeYP;
                if (nodeYP.IsCP)
                {
                    //第一分歧管可能是梳形管 20170711 by Yunxiao Lin
                    firstBranchKit = pipBll.getFirstHeaderBranchPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                else
                {
                    // 第一分歧管放大一号计算
                    firstBranchKit = pipBll.getFirstPipeCalculation(nodeYP, currentSystem, out errorType);
                }

            }
            //bug 3489
            var L2SizeDownRule = GetL2SizeDownRule(currentSystem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            //bug 3489
            pipBll.getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, currentSystem, false, out errorType, L2SizeDownRule);
            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut);
        }

        private void AdjustPipeSizeOfBranchKitsAfterFirstBranch(MyNodeOut nodeOut, MyNodeYP currentYP, L2SizeDownRules rule)
        {
            if (nodeOut == null || !(nodeOut.ChildNode is MyNodeYP))
            {
                return;
            }

            MyNodeYP firstYP = nodeOut.ChildNode as MyNodeYP;
            if (!firstYP.IsFirstYP || firstYP.IsCP)  //Header branch后面没有multi-kit所以没有比较的可能
            {
                return;
            }

            if (currentYP == null || currentYP == firstYP)
            {
                return;
            }

            //if (thisProject.RegionCode.StartsWith("EU_") && parent is MyNodeYP)
            //适用所有区域 modified by Shen Junjie on 2018/8/2


            MyLink lkFirst = firstYP.MyInLinks[0];
            MyLink lkCurrent = currentYP.MyInLinks[0];
            string[][] properties = new string[][]
            {
                new string[] { nameof(lkFirst.SpecL_Normal), nameof(lkFirst.SpecL) },
                new string[] { nameof(lkFirst.SpecG_l_Normal), nameof(lkFirst.SpecG_l) },
                new string[] { nameof(lkFirst.SpecG_h_Normal), nameof(lkFirst.SpecG_h) }
            };

            foreach (string[] p in properties)
            {

                string sSizeFirst = lkFirst.GetProperyValue<string>(p[0]);
                string sBeforeCurrent = lkCurrent.GetProperyValue<string>(p[0]);
                string sAfterCurrent = lkCurrent.GetProperyValue<string>(p[1]);
                double sizeFirst;
                double sizeBeforeCurrent;
                double sizeAfterCurrent;

                if (!(double.TryParse(sSizeFirst, out sizeFirst))) continue;
                if (!(double.TryParse(sBeforeCurrent, out sizeBeforeCurrent))) continue;
                if (!(double.TryParse(sAfterCurrent, out sizeAfterCurrent))) continue;


                if (rule == L2SizeDownRules.AlwaysSizeUp1stBranch)
                {
                    if (sizeFirst < sizeAfterCurrent)
                    {
                        //增大1st branch管径
                        lkFirst.SetPropertyValue(p[1], sAfterCurrent);
                    }
                }
                else if (rule == L2SizeDownRules.SometimesSizeDown2ndBranch)
                {
                    if (sizeBeforeCurrent == sizeAfterCurrent)  //L2 =< 40m (No Size up condition)
                    {
                        if (sizeFirst < sizeBeforeCurrent)  //Before size-up: Main pipe < After 1st branch pipe
                        {
                            //Solution: Downsize after 1st branch pipe and make "Main pipe = After 1st branch pipe"
                            lkCurrent.SetPropertyValue(p[1], sSizeFirst);
                        }
                        else
                        {
                            //Keep the both pipe size
                        }
                    }
                    else //40m < L2 =< 90m (Size up condition)
                    {
                        if (sizeFirst > sizeBeforeCurrent) //Before size-up: Main pipe > After 1st branch pipe
                        {
                            if (sizeFirst < sizeAfterCurrent)  //After size-up: Main pipe < After 1st branch pipe
                            {
                                //TODO: ???? 
                            }
                            //Keep the both pipe size
                        }
                        else if (sizeFirst == sizeBeforeCurrent) //Before size-up: Main pipe = After 1st branch pipe
                        {
                            if (sizeFirst < sizeAfterCurrent)  //After size-up: Main pipe < After 1st branch pipe
                            {
                                //Solution: Size up Main pipe and make "Main pipe = After 1st branch pipe"
                                lkFirst.SetPropertyValue(p[1], sAfterCurrent);
                            }
                        }
                        else  //Before size-up: Main pipe < After 1st branch pipe
                        {
                            if (sizeFirst < sizeAfterCurrent)  //After size-up: Main pipe < After 1st branch pipe (2 size larger than main pipe)
                            {
                                //Solution: Size up Main pipe and stop size up after 1st branch pipe and make "Main pipe = After 1st branch pipe"
                                lkFirst.SetPropertyValue(p[1], sBeforeCurrent);
                                lkCurrent.SetPropertyValue(p[1], sBeforeCurrent);
                            }
                        }
                    }
                }
            }

            //当前YP为梳型管时，不替换model型号  //Header branch不能跟普通的branch比较型号
            if (!currentYP.IsCP && (
                //3pipe的和CH Box后面的2pipe不可以比较  //add by Shen Junjien on 2019/3/13
                firstYP.PipesType == currentYP.PipesType
                //3pipe的和cooling only的2pipe可以比较  //add by Shen Junjien on 2019/3/13
                || (firstYP.PipesType == JCHVRF.Model.PipeCombineType.HR_L_LG_HG && currentYP.PipesType == JCHVRF.Model.PipeCombineType.HR_L_LG)
                ))
            {
                string model1 = firstYP.Model;
                string model2 = currentYP.Model;
                int HP1 = 0, HP2 = 0; //从Model 中获取匹数
                Match match1 = Regex.Match(model1, "\\d+");
                if (match1 != null)
                {
                    HP1 = int.Parse(match1.Value);
                }
                Match match2 = Regex.Match(model2, "\\d+");
                if (match1 != null)
                {
                    HP2 = int.Parse(match2.Value);
                }

                if (HP1 < HP2)
                {
                    if (rule == L2SizeDownRules.AlwaysSizeUp1stBranch)
                    {
                        //增大1st branch型号
                        firstYP.Model = model2;
                    }
                    else if (rule == L2SizeDownRules.SometimesSizeDown2ndBranch)
                    {
                        //Conditions: 1. Whichever L2 is within 40m or more than 40m
                        //            2. 1st Branch multi kit < After 1st branch multi kit
                        //  Solution: Downsize after 1st branch multi kit size and make "1st Branch multi kit = After 1st Branch multi kit"
                        currentYP.Model = model1;
                    }
                }
            }
        }
        ///// <summary>
        ///// 获取计算各个管径的lable位置，并画出管径   add on 20180522 by Vince
        ///// </summary>
        ///// <param name="label"></param>
        ///// <param name="pipeSize"></param>
        ///// <param name="nodeUnit"></param>
        ///// <param name="GasNo">1:低压气管 2:高压气管 3：液管 </param>
        private void DrawPipeLableColor(Caption label, string pipeSize, Node nodeUnit, int GasNo)
        {
            initTextNode(label, pipeSize); //画出管径文字
            //addFlowPiping.Items.Add(label);
            this.addFlowPiping.AddCaption(label);
            if (nodeUnit != null)
                // label.Parent = nodeUnit;    // To be Fix latter
                //根据顺序定义管径颜色.
                switch (GasNo)
                {
                    case 1:
                        label.Foreground = System.Windows.Media.Brushes.Green;
                        break;
                    case 2:
                        label.Foreground = System.Windows.Media.Brushes.Chocolate;    // To be Fix latter
                        break;
                    case 3:
                        label.Foreground = System.Windows.Media.Brushes.Blue;
                        break;
                    case 4:
                        label.Foreground = System.Windows.Media.Brushes.Purple;
                        break;


                }

        }

        private void DrawPipeLableColor(JCHNode label, string pipeSize, Node nodeUnit, int GasNo, ref AddFlow addFlowPiping)
        {
            initTextNode(label, pipeSize);
            addFlowPiping.AddNode(label);
            if (nodeUnit != null)
                nodeUnit.AddFlow.Items.Add(label);
            //nodeUnit.Stroke = System.Windows.Media.Brushes.Red;
            switch (GasNo)
            {
                case 1:
                    label.Foreground = System.Windows.Media.Brushes.MediumPurple;
                    break;
                case 2:
                    label.Foreground = System.Windows.Media.Brushes.Blue;    // To be Fix latter
                    break;
                case 3:
                    label.Foreground = System.Windows.Media.Brushes.Green;
                    break;
                case 4:
                    label.Foreground = System.Windows.Media.Brushes.Red;
                    break;
            }

        }

        private void DrawPipeLableMyColor(Node label, string pipeSize, Node nodeUnit, int GasNo, ref AddFlow addFlowPiping)
        {
            initTextMyNode(label, pipeSize); //画出管径文字
                                             //addFlowPiping.Items.Add(label);
            addFlowPiping.Items.Add(label);
            if (nodeUnit != null)
                // label.Parent = nodeUnit;    // To be Fix latter
                //根据顺序定义管径颜色.
                switch (GasNo)
                {
                    case 1:
                        label.Foreground = System.Windows.Media.Brushes.Green;
                        break;
                    case 2:
                        label.Foreground = System.Windows.Media.Brushes.Chocolate;    // To be Fix latter
                        break;
                    case 3:
                        label.Foreground = System.Windows.Media.Brushes.Blue;
                        break;
                    case 4:
                        label.Foreground = System.Windows.Media.Brushes.Purple;
                        break;
                }

        }

        //统计当前系统的高度差 on 20180620 by xyj
        public void StatisticsSystem_HighDiff(SystemVRF sysItem, List<MyNode> list)
        {

            List<OldModel.RoomIndoor> indoorList = new List<OldModel.RoomIndoor>(); // 室内机集合 
            List<MyNode> chboxList = new List<MyNode>();  //CHBox 集合
            List<double> highList = new List<double>();   //高度差集合
            foreach (MyNode node in list)
            {
                if (node is MyNodeIn)
                {
                    MyNodeIn nodeIn = node as MyNodeIn;
                    indoorList.Add(nodeIn.RoomIndooItem);
                }
                else if (node is MyNodeCH)
                {
                    MyNodeCH chNode = node as MyNodeCH;
                    highList.Add(chNode.HeightDiff);
                    chboxList.Add(chNode);
                }
                else if (node is MyNodeMultiCH)
                {
                    MyNodeMultiCH chNode = node as MyNodeMultiCH;
                    highList.Add(chNode.HeightDiff);
                    chboxList.Add(chNode);
                }
            }

            //统计CHBox MultiCHBox 直接的高度差
            CHBoxMultiCHBox_HighDiff(sysItem, highList);

            //统计CHBox and OldModel.Indoor Units Height difference
            CHboxToIndoor_HighDiff(sysItem, chboxList);

            //取MulitCHbox 使用CH-Box的同一分支的室内机之间的高度差
            MultiCHBoxSameIndoor_HighDiff(sysItem, chboxList);

            //统计室内机直接的高度差
            RoomIndoor_HighDiff(sysItem, indoorList);
        }

        private void CHboxToIndoor_HighDiff(SystemVRF system, List<MyNode> list)
        {
            List<double> MaxHigh = new List<double>();
            if (list.Count > 0)
            {
                foreach (MyNode node in list)
                {
                    if (node is MyNodeCH)
                    {
                        MyNodeCH nodeCH = node as MyNodeCH;
                        if (nodeCH.ChildNode is MyNodeIn)
                        {
                            MyNodeIn inNode = nodeCH.ChildNode as MyNodeIn;
                            double val = inNode.RoomIndooItem.HeightDiff;
                            if (inNode.RoomIndooItem.PositionType == OldModel.PipingPositionType.Lower.ToString())
                            {
                                val = -inNode.RoomIndooItem.HeightDiff;
                            }
                            double values = Math.Abs(val - nodeCH.HeightDiff);
                            MaxHigh.Add(values);
                        }
                    }
                    else if (node is MyNodeMultiCH)
                    {
                        MyNodeMultiCH muNode = node as MyNodeMultiCH;
                        List<double> arrlist = new List<double>();
                        foreach (Node child in muNode.ChildNodes)
                        {
                            #region
                            if (child is MyNodeYP)
                            {
                                MyNodeYP ypNode = child as MyNodeYP;
                                foreach (Node chi in ypNode.ChildNodes)
                                {
                                    if (chi is MyNodeIn)
                                    {
                                        MyNodeIn inNode = chi as MyNodeIn;
                                        StatisticsIDU_HighDiff(inNode, ref arrlist);
                                    }
                                }
                            }
                            else if (child is MyNodeIn)
                            {
                                MyNodeIn inNode = child as MyNodeIn;
                                StatisticsIDU_HighDiff(inNode, ref arrlist);
                            }
                            #endregion
                        }
                        if (arrlist.Count > 0)
                        {
                            double maxH = arrlist.Max();
                            double values = Math.Abs(maxH - muNode.HeightDiff);
                            MaxHigh.Add(values);
                        }
                        else
                        {
                            MaxHigh.Add(0);
                        }
                    }
                }
                if (MaxHigh.Count > 0)
                    system.MaxCHBox_IndoorHighDiffLength = MaxHigh.Max() == 0 ? 0 : MaxHigh.Max();

            }
        }

        /// <summary>
        /// 取MulitCHbox 使用CH-Box的同一分支的室内机之间的高度差
        /// </summary>
        /// <param name="system">系统</param>
        /// <param name="multichList">MulitCHbox 集合</param>
        /// <param name="chList">CHbox 集合</param>
        private void MultiCHBoxSameIndoor_HighDiff(SystemVRF system, List<MyNode> list)
        {
            List<double> MaxHigh = new List<double>();

            foreach (MyNode node in list)
            {
                List<double> muHigh = new List<double>();
                bool MultiCHbox = false;
                int indoorCount = 0;
                if (node is MyNodeMultiCH)
                {
                    MultiCHbox = true;
                    MyNodeMultiCH munode = node as MyNodeMultiCH;
                    indoorCount = SameIndoorToCHBox_HighDiff(munode, ref muHigh);  //计算同一个chbox 下的室内机之间的高度差
                }
                if (node is MyNodeCH)
                {
                    MultiCHbox = false;
                    MyNodeCH chnode = node as MyNodeCH;
                    indoorCount = SameIndoorToCHBox_HighDiff(chnode, ref muHigh);  //计算同一个chbox 下的室内机之间的高度差
                }
                if (MultiCHbox || !MultiCHbox && indoorCount > 1)
                {
                    if (muHigh.Count > 0)
                    {
                        double maxCHhigh = muHigh.Max() == 0 ? 0 : muHigh.Max();
                        double minCHhigh = muHigh.Min() == 0 ? 0 : muHigh.Min();
                        double CHBoxHigh = 0.0;
                        CHBoxHigh = maxCHhigh - minCHhigh;
                        MaxHigh.Add(CHBoxHigh);
                    }
                }
            }
            if (MaxHigh.Count > 0)
            {
                system.MaxSameCHBoxHighDiffLength = MaxHigh.Max() == 0 ? 0 : MaxHigh.Max();
            }
        }



        //MulitCHbox CHbox 使用CH-Box的同一分支的室内机之间的高度差
        public int SameIndoorToCHBox_HighDiff(Node node, ref List<double> MaxHigh)
        {
            int sum = 0;
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                return SameIndoorToCHBox_HighDiff(nodeOut.ChildNode, ref MaxHigh);
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                return SameIndoorToCHBox_HighDiff(nodeCH.ChildNode, ref MaxHigh);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    sum += SameIndoorToCHBox_HighDiff(child, ref MaxHigh);
                }
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    sum += SameIndoorToCHBox_HighDiff(child, ref MaxHigh);
                }
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                StatisticsIDU_HighDiff(nodeIn, ref MaxHigh);
                sum++;
            }
            return sum;
        }

        /// <summary>
        /// 统计IDU 的高度差
        /// </summary>
        /// <param name="child"></param>
        /// <param name="MaxHigh"></param>
        private void StatisticsIDU_HighDiff(MyNodeIn child, ref List<double> MaxHigh)
        {
            if (child is MyNodeIn)
            {
                MyNodeIn inNode = child as MyNodeIn;
                double values = inNode.RoomIndooItem.HeightDiff;
                if (inNode.RoomIndooItem.PositionType == OldModel.PipingPositionType.Lower.ToString())
                {
                    values = -inNode.RoomIndooItem.HeightDiff;
                }
                MaxHigh.Add(values);
            }
        }

        /// <summary>
        /// 统计CHBox MultiCHBox 直接的高度差
        /// </summary>
        /// <param name="system">当前系统</param>
        /// <param name="chList">CHBox MultiCHBox 集合</param>
        private void CHBoxMultiCHBox_HighDiff(SystemVRF system, List<double> chList)
        {
            if (chList.Count > 0)
            {
                double maxCHhigh = chList.Max() == 0 ? 0 : chList.Max();
                double minCHhigh = chList.Min() == 0 ? 0 : chList.Min();
                //height  difference  between ch-boxes
                double CHBoxHigh = 0.0;
                CHBoxHigh = maxCHhigh - minCHhigh;
                system.MaxCHBoxHighDiffLength = CHBoxHigh;
            }
        }

        /// <summary>
        /// 统计室内机直接的高度差
        /// </summary>
        /// <param name="system">当前系统</param>
        /// <param name="list">室内机集合</param>
        private void RoomIndoor_HighDiff(SystemVRF system, List<OldModel.RoomIndoor> list)
        {
            if (list.Count > 0)
            {
                //获取 Higher 的最大值
                double HigherDiff = 0.0;
                List<OldModel.RoomIndoor> listH = list.FindAll(p => p.PositionType == OldModel.PipingPositionType.Upper.ToString());
                if (listH != null && listH.Count > 0)
                    HigherDiff = listH.Max(work => work.HeightDiff);

                //获取 Lower 的最大值
                double LowerDiff = 0.0;
                List<OldModel.RoomIndoor> listL = list.FindAll(p => p.PositionType == OldModel.PipingPositionType.Lower.ToString());
                if (listL != null && listL.Count > 0)
                    LowerDiff = listL.Max(work => work.HeightDiff);

                system.MaxIndoorHeightDifferenceLength = Calculate_HighDiff(list);
                system.MaxUpperHeightDifferenceLength = HigherDiff;
                system.MaxLowerHeightDifferenceLength = LowerDiff;
            }
        }

        /// <summary>
        /// 计算高度差
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private double Calculate_HighDiff(List<OldModel.RoomIndoor> list)
        {
            List<double> diffList = new List<double>(); //高度差集合
            double maxValue = 0; //最大高度差
            double minValue = 0; //最小高度差
            double indDiff = 0; //室内机与室内机直接的高度差
            if (list.Count > 0)
            {
                foreach (OldModel.RoomIndoor ri in list)
                {
                    double val = Convert.ToDouble(ri.HeightDiff);
                    if (ri.PositionType == OldModel.PipingPositionType.Lower.ToString())
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

        ///// 计算各个节点的总容量以及分歧管、连接管的气管、也管型号，TODO：制冷、制热模式分别处理？？？
        ///// modify on 20160516 by Yunxiao Lin 增加CH-Box连接YP的处理和计算各个管径放大一号，增加参数-当前节点是否直连外机
        ///// <summary>
        ///// 计算各个节点的总容量以及分歧管、连接管的气管、也管型号，TODO：制冷、制热模式分别处理？？？
        ///// </summary>
        ///// <param name="node"></param>
        ///// <param name="factoryCode"></param>
        ///// <param name="type"> 2pipe | 3pipe </param>
        ///// <param name="ParentIsOutdoor"></param>
        //private void getSumCalculation(ref Node node, string factoryCode, string type, string unitType)
        //{
        //    //判断当前系统是Heat Recovery(R) 或 Heat Pump(H)
        //    string HeatType = "H";
        //    if (curSystemItem.OutdoorItem.ProductType.Contains("Heat Recovery") || curSystemItem.OutdoorItem.ProductType.Contains(", HR"))
        //        HeatType = "R";
        //    //string HeatType = curSystemItem.OutdoorItem.ModelFull.Substring(3, 1);
        //    PipingBLL pipBll = GetPipingBLLInstance();
        //    MyLink myLink;
        //    string specG_h = "";
        //    string specG_l = "";
        //    string specL = "";
        //    if (node is MyNodeYP)
        //    {
        //        #region YP
        //        MyNodeYP nodeYP = node as MyNodeYP;
        //        //if (HeatType == "R" && nodeYP.IsCoolingonly && type=="2Pipe")
        //        //    type = "3Pipe";
        //        myLink = nodeYP.InLink;
        //        double sumCool = 0;
        //        double sumHeat = 0;
        //        foreach (Link l in nodeYP.OutLinks)
        //        {
        //            Node n = l.Dst; // 子节点
        //            getSumCalculation(ref n, factoryCode, type, unitType); // 迭代计算；
        //            if (n is MyNodeIn)
        //            {
        //                //sumCool += (n as MyNodeIn).RoomIndooItem.CoolingCapacity;
        //                //sumHeat += (n as MyNodeIn).RoomIndooItem.HeatingCapacity;
        //                //排管计算改用室内机的标准容量 20161116 by Yunxiao Lin
        //                sumCool += (n as MyNodeIn).RoomIndooItem.IndoorItem.CoolingCapacity;
        //                sumHeat += (n as MyNodeIn).RoomIndooItem.IndoorItem.HeatingCapacity;
        //            }
        //            else if (n is MyNodeYP)
        //            {
        //                sumCool += (n as MyNodeYP).CoolingCapacity;
        //                sumHeat += (n as MyNodeYP).HeatingCapacity;
        //            }
        //            else if (n is MyNodeCH)
        //            {
        //                sumCool += (n as MyNodeCH).CoolingCapacity;
        //                sumHeat += (n as MyNodeCH).HeatingCapacity;
        //            }
        //        }
        //        nodeYP.CoolingCapacity = sumCool;
        //        nodeYP.HeatingCapacity = sumHeat;

        //        OldModel.PipingBranchKit branchKit = null;
        //        if (!nodeYP.IsFirstYP)
        //        {
        //            if (nodeYP.IsCP)
        //            {
        //                branchKit = pipBll.GetHeaderBranch(type, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), nodeYP.ChildCount);

        //                if (branchKit == null)
        //                {
        //                    //如果FALSE查不到数据，试试NA
        //                    branchKit = pipBll.GetHeaderBranch(type, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), nodeYP.ChildCount);
        //                }
        //                if (branchKit == null)
        //                {
        //                    //还查不到就抛出错误
        //                    JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                    utilPiping.setItemWarning(nodeYP);
        //                    curSystemItem.IsPipingOK = false;
        //                    return;
        //                }
        //                //判断梳形管需不需要放大一号
        //                if (IsHeaderBranchSizeUp(branchKit, nodeYP))
        //                {
        //                    branchKit = pipBll.GetHeaderBranch(type, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), nodeYP.ChildCount);
        //                }
        //            }
        //            //else if (HeatType == "R")
        //            //{
        //            //    branchKit = pipBll.GetTertiaryBranchKit(factoryCode,type, unitType, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString());
        //            //    if (branchKit == null)
        //            //    {
        //            //        //如果FALSE查不到数据，试试NA
        //            //        branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.NA.ToString());
        //            //    }
        //            //    if (branchKit == null)
        //            //    {
        //            //        //还查不到就抛出错误
        //            //        JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //            //        utilPiping.setItemWarning(nodeYP);
        //            //        return;
        //            //    }
        //            //    //YVAHP的分歧管可能需要放大一号
        //            //    if (IsTeriaryBranchSizeUp(branchKit, nodeYP, curSystemItem))
        //            //    {
        //            //        branchKit = pipBll.GetTertiaryBranchKit(factoryCode, "3Pipe", unitType, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString());
        //            //    }
        //            //}
        //            else
        //            {
        //                //bool isMal = curSystemItem.OutdoorItem.RegionCode == "MAL";
        //                bool isMal = (curSystemItem.OutdoorItem.RegionCode == "MAL" && thisProject.BrandCode == "Y"); //只有马来西亚的YORK品牌的分歧管是以QE为结尾的 20161023 by Yunxiao Lin
        //                // 注意FSVN1Q, FSYN1Q, JCOC, JDOH(FSNMQ除外)的室外机选择分歧管不需要考虑容量限制，JDOH由于比较特殊(有两个系列的外机)，暂时还是参考容量限制 20160704 Yunxiao Lin
        //                if (curSystemItem.OutdoorItem.Type.Contains("FSVN1Q") || curSystemItem.OutdoorItem.Type.Contains("FSYN1Q") || curSystemItem.OutdoorItem.Type.Contains("JCOC"))
        //                {
        //                    branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, 0, OldModel.PipingSizeUPType.FALSE.ToString(), isMal);
        //                    if (branchKit == null)
        //                    {
        //                        //如果FALSE查不到数据，试试NA
        //                        branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, 0, OldModel.PipingSizeUPType.NA.ToString(), isMal);
        //                    }
        //                    if (branchKit == null)
        //                    {
        //                        //还查不到就抛出错误
        //                        JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                        utilPiping.setItemWarning(nodeYP);
        //                        curSystemItem.IsPipingOK = false;
        //                        return;
        //                    }
        //                    //YVAHP的分歧管可能需要放大一号
        //                    if (IsTeriaryBranchSizeUp(branchKit, nodeYP, curSystemItem))
        //                    {
        //                        branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, 0, OldModel.PipingSizeUPType.TRUE.ToString(), isMal);
        //                    }
        //                }
        //                else
        //                {
        //                    branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), isMal);
        //                    if (branchKit == null)
        //                    {
        //                        //如果FALSE查不到数据，试试NA
        //                        branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), isMal);
        //                    }
        //                    if (branchKit == null)
        //                    {
        //                        //还查不到就抛出错误
        //                        JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                        utilPiping.setItemWarning(nodeYP);
        //                        curSystemItem.IsPipingOK = false;
        //                        return;
        //                    }
        //                    //YVAHP的分歧管可能需要放大一号
        //                    if (IsTeriaryBranchSizeUp(branchKit, nodeYP, curSystemItem))
        //                    {
        //                        branchKit = pipBll.GetTertiaryBranchKit(factoryCode, type, unitType, nodeYP.CoolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), isMal);
        //                    }
        //                }
        //            }
        //        }

        //        if (branchKit == null)
        //            return;

        //        nodeYP.Model = branchKit.Model_York;
        //        if (isHitachi)
        //            nodeYP.Model = branchKit.Model_Hitachi;

        //        specL = branchKit.LiquidPipe.Trim();
        //        //Heat Recovery 只有3Pipe的分歧管，如果当前配管只有2管，取到管径后需要手动设置High Pressure Gass pipe或Low Pressure Gass pipe为"-"
        //        if (HeatType == "R" && branchKit.Type == "3Pipe" && nodeYP.IsCoolingonly)
        //            specG_h = "-";
        //        else
        //            specG_h = branchKit.HighPressureGasPipe.Trim();

        //        //if (HeatType == "R" && branchKit.Type == "3Pipe" && !nodeYP.IsCoolingonly)
        //        //    specG_l = "-";
        //        //else
        //        //    specG_l = branchKit.LowPressureGasPipe.Trim();
        //        //specG_h = branchKit.HighPressureGasPipe.Trim();
        //        specG_l = branchKit.LowPressureGasPipe.Trim();
        //        myLink.SpecL = specL; // 计算追加制冷剂时需要汇总管径规格
        //        myLink.SpecG_h = specG_h;
        //        myLink.SpecG_l = specG_l;
        //        myLink.SizeUP = branchKit.SizeUP;
        //        #endregion
        //    }
        //    else if (node is MyNodeCH)
        //    {
        //        type = "2Pipe";
        //        #region CHbox 20160406 clh
        //        MyNodeCH nodeCH = node as MyNodeCH;
        //        if (nodeCH.ChildNode is MyNodeIn)
        //        {
        //            Node n = nodeCH.ChildNode;
        //            getSumCalculation(ref n, factoryCode, type, unitType); //OldModel.Indoor如果需要放大一号，管径会变化
        //            myLink = nodeCH.InLink;
        //            MyNodeIn item = (nodeCH.ChildNode as MyNodeIn);
        //            //nodeCH.CoolingCapacity = item.RoomIndooItem.CoolingCapacity;
        //            //nodeCH.HeatingCapacity = item.RoomIndooItem.HeatingCapacity;
        //            //排管计算改用室内机的标准容量 20161116 by Yunxiao lin
        //            nodeCH.CoolingCapacity = item.RoomIndooItem.IndoorItem.CoolingCapacity;
        //            nodeCH.HeatingCapacity = item.RoomIndooItem.IndoorItem.HeatingCapacity;
        //            int IUCount = 1;
        //            OldModel.PipingChangeOverKit chKit = pipBll.GetChangeOverKit(factoryCode, nodeCH.CoolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), IUCount, curSystemItem.OutdoorItem.Series); // 只连接一个室内机，管径不放大
        //            if (chKit == null) //FALSE找不到，试试NA
        //                chKit = pipBll.GetChangeOverKit(factoryCode, nodeCH.CoolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), IUCount, curSystemItem.OutdoorItem.Series);
        //            if (chKit == null)
        //            {
        //                //还查不到就抛出错误
        //                JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                utilPiping.setItemWarning(nodeCH);
        //                curSystemItem.IsPipingOK = false;
        //                return;
        //            }

        //            nodeCH.Model = chKit.Model_York;
        //            if (isHitachi)
        //                nodeCH.Model = chKit.Model_Hitachi;

        //            myLink.SpecL = item.RoomIndooItem.IndoorItem.LiquidPipe.Trim();
        //            myLink.SpecG_h = chKit.HighPressureGasPipe.Trim();
        //            myLink.SpecG_l = chKit.LowPressureGasPipe.Trim();
        //            myLink.SizeUP = chKit.SizeUP;
        //            //add on 20160515 by Yunxiao Lin
        //            nodeCH.MaxTotalCHIndoorPipeLength = chKit.MaxTotalCHIndoorPipeLength;
        //            nodeCH.MaxTotalCHIndoorPipeLength_MaxIU = chKit.MaxTotalCHIndoorPipeLength_MaxIU;
        //            nodeCH.MaxIndoorCount = chKit.MaxIU;
        //        }
        //        else if (nodeCH.ChildNode is MyNodeYP)
        //        {
        //            //add on 20160516 by Yunxiao Lin
        //            myLink = nodeCH.InLink;
        //            Node n = nodeCH.ChildNode;
        //            getSumCalculation(ref n, factoryCode, type, unitType);
        //            nodeCH.CoolingCapacity = (n as MyNodeYP).CoolingCapacity;
        //            nodeCH.HeatingCapacity = (n as MyNodeYP).HeatingCapacity;
        //            int IUCount = pipBll.getIndoorCount(nodeCH, addFlowPiping);
        //            OldModel.PipingChangeOverKit chKit = pipBll.GetChangeOverKit(factoryCode, nodeCH.CoolingCapacity, OldModel.PipingSizeUPType.FALSE.ToString(), IUCount, curSystemItem.OutdoorItem.Series);

        //            if (chKit == null) //FALSE找不到，试试NA
        //                chKit = pipBll.GetChangeOverKit(factoryCode, nodeCH.CoolingCapacity, OldModel.PipingSizeUPType.NA.ToString(), IUCount, curSystemItem.OutdoorItem.Series);
        //            if (chKit == null)
        //            {
        //                //还查不到就抛出错误
        //                JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                utilPiping.setItemWarning(nodeCH);
        //                curSystemItem.IsPipingOK = false;
        //                return;
        //            }
        //            //判断CH-Box是否需要SizeUp
        //            if (IsCHBoxSizeUp(chKit, nodeCH))
        //            {
        //                //重新获取CH-Box管型
        //                chKit = pipBll.GetChangeOverKit(factoryCode, nodeCH.CoolingCapacity, OldModel.PipingSizeUPType.TRUE.ToString(), IUCount, curSystemItem.OutdoorItem.Series);
        //            }
        //            if (chKit == null)
        //            {
        //                //还查不到就抛出错误
        //                JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                utilPiping.setItemWarning(nodeCH);
        //                curSystemItem.IsPipingOK = false;
        //                return;
        //            }

        //            nodeCH.Model = chKit.Model_York;
        //            if (isHitachi)
        //                nodeCH.Model = chKit.Model_Hitachi;

        //            myLink.SpecL = (n as MyNodeYP).InLink.SpecL; //CH-Box内部没有液管，液管只是从其旁边通过，不会改变管径。
        //            myLink.SpecG_h = chKit.HighPressureGasPipe.Trim();
        //            myLink.SpecG_l = chKit.LowPressureGasPipe.Trim();
        //            myLink.SizeUP = chKit.SizeUP;

        //            nodeCH.MaxTotalCHIndoorPipeLength = chKit.MaxTotalCHIndoorPipeLength;
        //            nodeCH.MaxTotalCHIndoorPipeLength_MaxIU = chKit.MaxTotalCHIndoorPipeLength_MaxIU;
        //            nodeCH.MaxIndoorCount = chKit.MaxIU;
        //        }
        //        #endregion
        //    }
        //    else if (node is MyNodeIn)
        //    {
        //        //判断OldModel.Indoor的配管是否需要SizeUP
        //        MyNodeIn indoor = node as MyNodeIn;
        //        myLink = indoor.InLink;
        //        if (IsIndoorSizeUp(indoor, factoryCode))
        //        {
        //            //重新计算OldModel.Indoor的配管尺寸
        //            string[] cc = pipBll.GetPipeSizeIDU_SizeUp(factoryCode, indoor.RoomIndooItem.IndoorItem.CoolingCapacity, indoor.RoomIndooItem.IndoorItem.Model_Hitachi, OldModel.PipingSizeUPType.TRUE.ToString());
        //            if (cc == null)
        //            {
        //                //还查不到就抛出错误
        //                JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
        //                utilPiping.setItemWarning(node);
        //                curSystemItem.IsPipingOK = false;
        //                return;
        //            }
        //            myLink.SpecG_l = cc[1];
        //            myLink.SpecL = cc[0];
        //            myLink.SizeUP = OldModel.PipingSizeUPType.TRUE.ToString();
        //        }
        //        else
        //        {
        //            specG_h = indoor.RoomIndooItem.IndoorItem.GasPipe.Trim();
        //            specL = indoor.RoomIndooItem.IndoorItem.LiquidPipe.Trim();
        //            myLink.SpecG_h = specG_h;
        //            myLink.SpecG_l = "-";
        //            myLink.SpecL = specL;
        //        }
        //    }
        //}

        #endregion

        public PointF getCenterPointF(Node node)
        {
            float fx = (float)(node.Location.X + node.Size.Width / 2);
            float fy = (float)(node.Location.Y + node.Size.Height / 2);
            return new PointF(fx, fy);
        }
        public PointF getLeftCenterPointF(Node node)
        {
            float fx = (float)node.Location.X;
            float fy = (float)(node.Location.Y + node.Size.Height / 2);
            return new PointF(fx, fy);
        }
        public PointF getRightCenterPointF(Node node)
        {
            float fx = (float)(node.Location.X + node.Size.Width);
            float fy = (float)(node.Location.Y + node.Size.Height / 2);
            return new PointF(fx, fy);
        }
        public PointF getTopCenterPointF(Node node)
        {
            float fx = (float)(node.Location.X + node.Size.Width / 2);
            float fy = (float)node.Location.Y;
            return new PointF(fx, fy);
        }
        public PointF getBottomCenterPointF(Node node)
        {
            float fx = (float)(node.Location.X + node.Size.Width / 2);
            float fy = (float)(node.Location.Y + node.Size.Height);
            return new PointF(fx, fy);
        }
        public PointF getLeftBottomPointF(Node node)
        {
            float fx = (float)node.Location.X;
            float fy = (float)(node.Location.Y + node.Size.Height);
            return new PointF(fx, fy);
        }
        public PointF getRightBottomPointF(Node node)
        {
            float fx = (float)(node.Location.X + node.Size.Width);
            float fy = (float)(node.Location.Y + node.Size.Height);
            return new PointF(fx, fy);
        }
        public PointF getRightTopPointF(Node node)
        {
            float fx = (float)(node.Location.X + node.Size.Width);
            float fy = (float)node.Location.Y;
            return new PointF(fx, fy);
        }
        public PointF convertSystemPointToDrawingPoint(System.Windows.Point sysPoint)
        {
            PointF newPoint = new PointF((float)sysPoint.X, (float)sysPoint.Y);
            return newPoint;
        }
        public System.Windows.Point convertPointFToWinPoint(PointF drawPointF)
        {
            System.Windows.Point newPointWin = new System.Windows.Point((double)drawPointF.X, (double)drawPointF.Y);
            return newPointWin;
        }
        public System.Windows.Point convertPointFToWinPoint(double x, double y)
        {
            System.Windows.Point newPointWin = new System.Windows.Point(x, y);
            return newPointWin;
        }
        public System.Windows.Size convertSize(double x, double y)
        {
            System.Windows.Size newSize = new System.Windows.Size(x, y);
            return newSize;
        }

        public static double GetMaxPipeLengthOfNodeOut(MyNodeOut nodeOut)
        {
            if (nodeOut == null) return 0;

            double[] pipeLengthes = nodeOut.PipeLengthes;
            if (pipeLengthes == null)
                return 0;

            double b = 0, c = 0, d = 0, e = 0, f = 0, g = 0, max = 0;

            if (nodeOut.UnitCount >= 2)
            {
                if (nodeOut.UnitCount >= 4)
                {
                    if (pipeLengthes.Length >= 1) c = pipeLengthes[0];
                    if (pipeLengthes.Length >= 2) b = pipeLengthes[1];
                }
                else
                {
                    if (pipeLengthes.Length >= 1) b = pipeLengthes[0];
                    //if (_nodeOut.UnitCount >= 4)
                    //{
                    //    //4个机组有2个b
                    //    index++;
                    //}
                    if (pipeLengthes.Length >= 2) c = pipeLengthes[1];
                }
            }
            if (nodeOut.UnitCount >= 3)
            {
                if (pipeLengthes.Length >= 3) d = pipeLengthes[2];
                if (pipeLengthes.Length >= 4) e = pipeLengthes[3];
            }
            if (nodeOut.UnitCount >= 4)
            {
                if (pipeLengthes.Length >= 5) f = pipeLengthes[4];
                if (pipeLengthes.Length >= 6) g = pipeLengthes[5];
            }

            switch (nodeOut.UnitCount)
            {
                case 2:
                    max = (new double[] { b, c }).Max();
                    break;
                case 3:
                    max = (new double[] { c, b + e, b + d }).Max();
                    break;
                case 4:
                    max = (new double[] { b + d, b + e, c + f, c + g }).Max();
                    break;
            }
            return max;
        }
        public void DeleteCHBoxUpwards(JCHNode node)
        {
            try
            {
                if (node == null) return;
                if (!(node is MyNode)) return;
                MyNode parent = (node as MyNode).ParentNode;
                if (node is MyNodeCH)
                {
                    MyNodeCH nodeCH = node as MyNodeCH;
                    ReplaceChildNode(parent, nodeCH, nodeCH.ChildNode);
                }
                else if (node is MyNodeMultiCH)
                {
                    DeleteMultiCHBox(node as MyNodeMultiCH);
                }
                DeleteCHBoxUpward(parent);
            }
            catch (Exception ex)
            {
            }
        }
        public void DeleteCHBoxDownwards(JCHNode node)
        {
            if (node == null) return;
            if (!(node is MyNode)) return;
            MyNode parent = (node as MyNode).ParentNode;
            if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                JCHNode newChild = (JCHNode)nodeCH.ChildNode;
                ReplaceChildNode(parent, nodeCH, newChild);

                DeleteCHBoxDownward(newChild);
            }
            else if (node is MyNodeMultiCH)
            {
                JCHNode newChild = (JCHNode)DeleteMultiCHBox(node as MyNodeMultiCH);
                DeleteCHBoxDownward(newChild);
                return;
            }
            else if (node is MyNodeOut)
            {
                DeleteCHBoxDownward((JCHNode)(node as MyNodeOut).ChildNode);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    DeleteCHBoxDownward((JCHNode)nodeYP.ChildNodes[i]);
                }
            }
        }
        private void DragToNodeYPs(Node selPNode, Node selNode, MyNodeYP targetYP, bool isHR, Node cpChild = null)
        {
            if (targetYP.IsCP)
            {
                if (cpChild != null)
                {
                    int index = targetYP.GetIndex(cpChild);
                    targetYP.AddChildNode(selNode, index);
                }
                else
                {
                    targetYP.AddChildNode(selNode);
                }
            }
            else
            {
                InsertNodeYP(targetYP.ParentNode, targetYP, selNode);
            }

            if (isHR)
            {
                if (targetYP.IsCoolingonly || ExistCHBoxUpward(targetYP))
                {
                    DeleteCHBoxDownward((JCHNode)selNode);
                }
                else
                {
                    if (!ExistsCHBoxDownward(selNode))
                    {
                        InsertCHBox(selNode);
                    }
                }
            }
            CutOldRelation(selPNode, selNode);
        }
        private void DragToNodeMultiCHBoxs(Node selPNode, Node selNode, MyNodeMultiCH targetMCH)
        {
            targetMCH.ChildNodes.Add(selNode);
            if (selNode is MyNode)
            {
                (selNode as MyNode).ParentNode = targetMCH;
                DeleteCHBoxDownward((JCHNode)selNode);
            }
            else
            {
                return;
            }
            CutOldRelation(selPNode, selNode);
        }
        private void DragToNodeIns(Node selNode, MyNodeIn targetNodeIn, Node selPNode, Node aimPNode, bool isHR)
        {
            if (aimPNode is MyNodeYP)
            {
                MyNodeYP nodeYP = aimPNode as MyNodeYP;
                if (nodeYP.IsCP)
                {
                    if (nodeYP.ChildCount >= nodeYP.MaxCount)
                        return;

                    DragToNodeYP(selPNode, selNode, aimPNode as MyNodeYP, isHR, targetNodeIn);
                    return;
                }
                else
                {
                    if (nodeYP.ChildCount < nodeYP.MaxCount)
                    {
                        nodeYP.AddChildNode(selNode);
                    }
                    else
                    {
                        InsertNodeYP(aimPNode, targetNodeIn, selNode);
                    }
                }
            }
            else if (ExistCPUpward(aimPNode))
            {
                return;
            }
            else
            {
                InsertNodeYP(aimPNode, targetNodeIn, selNode);
            }

            if (isHR)
            {
                if (targetNodeIn.IsCoolingonly || ExistCHBoxUpward(targetNodeIn))
                {
                    DeleteCHBoxDownward((JCHNode)selNode);
                }
            }
            CutOldRelation(selPNode, selNode);
        }


        private void InsertPipingKitTable(DataTable tb, string KitType, string KitSys, string KitModel, int KitQty, string KitSysId)
        {
            if (tb != null)
            {
                DataRow row = tb.NewRow();
                row["Type"] = KitType;
                row["System"] = KitSys;
                row["Model"] = KitModel;
                row["Qty"] = KitQty;
                row["SystemId"] = KitSysId;
                tb.Rows.Add(row);
            }
        }

        private static void CopyLength(List<MyLink> srcLinks, List<MyLink> destLinks)
        {
            var allLinks = srcLinks.Zip(destLinks, (sl, dl) => new { sl, dl });

            foreach (var links in allLinks)
            {
                links.dl.Length = links.sl.Length;
                links.dl.ElbowQty = links.sl.ElbowQty;
                links.dl.OilTrapQty = links.sl.OilTrapQty;
            }
        }

        public static void CopyPipeLength(Lassalle.WPF.Flow.Node srcNode, Lassalle.WPF.Flow.Node destNode)
        {

            if (srcNode is JCHVRF.Model.NextGen.MyNodeOut)
            {
                var srcNodeOut = srcNode as JCHVRF.Model.NextGen.MyNodeOut;
                var destNodeOut = destNode as JCHVRF.Model.NextGen.MyNodeOut;
                CopyPipeLength(srcNodeOut.ChildNode, destNodeOut.ChildNode);
            }
           
            if (srcNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                var srcNodeYP = srcNode as JCHVRF.Model.NextGen.MyNodeYP;
                var destNodeYP = destNode as JCHVRF.Model.NextGen.MyNodeYP;
                CopyLength(srcNodeYP.MyInLinks, destNodeYP.MyInLinks);
                for (int i = 0; i < srcNodeYP.ChildCount; ++i)
                {
                    CopyPipeLength(srcNodeYP.ChildNodes[i], destNodeYP.ChildNodes[i]);
                }

            }
            else if (srcNode is JCHVRF.Model.NextGen.MyNodeMultiCH)
            {
                var srcNodeMultiCH = srcNode as JCHVRF.Model.NextGen.MyNodeMultiCH;
                var destNodeMultiCH = destNode as JCHVRF.Model.NextGen.MyNodeMultiCH;
                CopyLength(srcNodeMultiCH.MyInLinks, destNodeMultiCH.MyInLinks);
                for (int i = 0; i < srcNodeMultiCH.ChildNodes.Count; ++i)
                {
                    CopyPipeLength(srcNodeMultiCH.ChildNodes[i], destNodeMultiCH.ChildNodes[i]);
                }

            }
            else if (srcNode is JCHVRF.Model.NextGen.MyNodeCH)
            {
                var srcNodeCH = srcNode as JCHVRF.Model.NextGen.MyNodeCH;
                var destNodeCH = destNode as JCHVRF.Model.NextGen.MyNodeCH;
                CopyLength(srcNodeCH.MyInLinks, destNodeCH.MyInLinks);
                CopyPipeLength(srcNodeCH.ChildNode, destNodeCH.ChildNode);
            }
            else if (srcNode is JCHVRF.Model.NextGen.MyNodeIn)
            {
                var srcNodeIn = srcNode as JCHVRF.Model.NextGen.MyNodeIn;
                
                if (srcNodeIn.IsCoolingonly)
                {
                    
                    var destNodeCH = destNode as JCHVRF.Model.NextGen.MyNodeCH;
                    ReplaceChildNode(destNodeCH.ParentNode, destNodeCH, destNodeCH.ChildNode);
                    var destNodeIn = destNodeCH.ChildNode as JCHVRF.Model.NextGen.MyNodeIn;
                    CopyLength(srcNodeIn.MyInLinks, destNodeIn.MyInLinks);
                    destNodeIn.IsCoolingonly = srcNodeIn.IsCoolingonly;
                }
                else
                {
                    var destNodeIn = destNode as JCHVRF.Model.NextGen.MyNodeIn;
                    CopyLength(srcNodeIn.MyInLinks, destNodeIn.MyInLinks);
                }
              
              
                return;
            }

            
        }

        //public NodeElement_Piping GetPipingNodeOutElement(SystemVRF sysItem, bool isHitachi)
        //{
        //    string pipeSize = "2Pipe";
        //    string sizeUp = "NA";
        //    string outModel = sysItem.OutdoorItem.ModelFull;
        //    string unitType = sysItem.OutdoorItem.Type;
        //    if (unitType.Contains("JTWH (Water source)"))
        //    {
        //        if (!outModel.Contains("100")) //单室外机不需要SizeUp
        //        {
        //            //计算等效管长
        //            if (sysItem.PipeEquivalentLength > 80d)
        //            {
        //                sizeUp = "TRUE";
        //            }
        //            else
        //            {
        //                sizeUp = "FALSE";
        //            }
        //        }
        //    }
        //    else if (IsHeatRecovery(sysItem))
        //    {
        //        pipeSize = "3Pipe";
        //    }
        //    return _dal.GetPipingNodeOutElement(outModel, isHitachi, pipeSize, sizeUp, unitType);
        //}

        //added for internal Bug Find zero length Issue
         public void DrawCorrectionFactorText(SystemVRF sysItem)
        {
            // 计算管长修正系数 on 20190314 by xyj
            sysItem.PipingLengthFactor = GetPipeLengthFactor(sysItem, "Cooling");
            sysItem.PipingLengthFactor_H = GetPipeLengthFactor(sysItem, "Heating");

        }
    }

    public static class ManualPipingLinkValidator
    {
        public static bool IsValidateConnection(Link link)
        {

            //Nde Out Validation
            if (link.Org is MyNodeOut || link.Dst is MyNodeOut)
            {
                return NodeOutLinkValidator.IsValidLink(link);
            }

            if (link.Org is MyNodeIn || link.Dst is MyNodeIn)
            {
                return NodeInLinkValidator.IsValidLink(link);
            }

            if (link.Org is MyNodeYP || link.Dst is MyNodeYP)
            {
                return NodeYpLinkValidator.IsValidLink(link);
            }

            return true;
        }

        internal class NodeYpLinkValidator
        {
            public static bool IsValidLink(Link link)
            {
                if (link.Org == link.Dst) return false;

                if (link.Org is MyNodeYP)
                {
                    if (link.Dst is MyNodeYP)
                    {
                        if (link.PinOrgIndex == link.PinDstIndex) return false;
                        if (link.PinOrgIndex >= 1 && link.PinDstIndex > 0) return false;
                    }

                    if (link.Dst is MyNodeIn)
                    {
                        if (link.PinOrgIndex == 0) return false;
                    }

                    if (link.Dst is MyNodeOut)
                    {
                        if (link.PinDstIndex != 0) return false;
                    }
                }

                if (link.Dst is MyNodeYP)
                {
                    if (link.Org is MyNodeYP)
                    {
                        if (link.PinOrgIndex == link.PinDstIndex) return false;
                        if (link.PinOrgIndex >= 1 && link.PinDstIndex > 0) return false;
                    }
                }

                return true;
            }
        }

        internal class NodeInLinkValidator
        {
            public static bool IsValidLink(Link link)
            {
                if (link.Dst is MyNodeIn)
                {
                    if (link.Org is MyNodeYP)
                    {
                        if (link.PinOrgIndex <= 0) return false; //In door should not directly connect to Yp Inlet.
                    }
                }

                if (link.Org is MyNodeIn)
                {
                    if (link.Dst is MyNodeYP)
                    {
                        if (link.PinDstIndex <= 0) return false; //In door should not directly connect to Yp Inlet.
                    }
                }

                var validLinks = IsLinkValidBetweenNodes.IsValid(link);
                if (!validLinks)
                {
                    return false;
                }

                return true;
            }
        }

        internal class NodeOutLinkValidator
        {
            public static bool IsValidLink(Link link)
            {
                if (link.Dst is MyNodeOut) return false;
                if (link.Dst is MyNodeYP && link.PinDstIndex > 0) return false;
                if (link.Dst is MyNodeIn) return true;
                return true;
            }
        }


    }

    public class IsLinkValidBetweenNodes
    {
        public static bool IsValid(Link link)
        {
            var nodesInvolved = new List<Node>();

            nodesInvolved.Add(link.Dst);
            nodesInvolved.Add(link.Org);

            GetNodesInvolved(link, link.Org.Links, link.PinOrgIndex, nodesInvolved);
            GetNodesInvolved(link, link.Dst.Links, link.PinDstIndex, nodesInvolved);

            var isAlreadyConnectedInPath = link.Org.Links.Where(x => x != link).Any(x => x.Org == link.Org || x.Org == link.Dst) ||
                              link.Org.Links.Where(x => x != link).Any(x => x.Dst == link.Org || x.Dst == link.Dst);



            return nodesInvolved.Count <= 3;
        }

        private bool IsNodeAvailableInConnectionPathAlready(Link link, Node linkOrg, Node linkDst)
        {
            if (linkOrg.Links.Where(x => x != link).Any(x => x.Org == link.Org || x.Org == link.Dst) ||
                linkDst.Links.Where(x => x != link).Any(x => x.Dst == link.Org || x.Dst == link.Dst))
            {
                return true;
            }

            return false;
        }

        private static void GetNodesInvolved(Link link, List<Link> links, int pinIndex, List<Node> nodesInvolved)
        {
            var commonLinks = links.Where(x => x != link &&
                                              ((x.Dst == link.Dst || x.Org == link.Dst) ||
                                               (pinIndex == x.PinOrgIndex ||
                                                pinIndex == x.PinDstIndex
                                                )));
            foreach (var commonLink in commonLinks)
            {
                if (!nodesInvolved.Contains(commonLink.Dst))
                {
                    nodesInvolved.Add(commonLink.Dst);
                }
                if (!nodesInvolved.Contains(commonLink.Org))
                {
                    nodesInvolved.Add(commonLink.Org);
                }
            }

        }






    }
}



