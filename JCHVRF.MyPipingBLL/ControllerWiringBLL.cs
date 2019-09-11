using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lassalle.Flow;
using JCHVRF.Model;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Data;

namespace JCHVRF.MyPipingBLL
{
    public class ControllerWiringBLL
    {
        private Project _project;
        private UtilPiping utilPiping = new UtilPiping();
        private AddFlow addFlowControllerWiring;
        private bool isHitachi;
        Bitmap bmpMeasureString;
        Graphics gMeasureString;

        bool hasCHBox = false;
        string _imageDir = "";
        int _groudImageIndex = 0;

        const int X_CC_Title = 250;
        const int X_CC_Level1 = 50;
        const int X_CC_Level2 = 350;
        const int X_Outdoor = 650;
        const int X_Indoor = 950;
        const int X_CHBox = 950;
        const int X_HR_Indoor = 1270;
        const int X_RemoteController = 1380;
        const int X_HR_RemoteController = 1700;
        const int Y_ColumnHeader = 50;
        const int W_CentralControl = 100;
        const int W_Outdoor = 100;
        const int W_CHBox = 100;
        const int W_Indoor = 200;
        const int W_RemoteController = 100;
        const int W_ContactPoint = 4;
        const int W_PowerWire = 160; //电源线长度
        const int H_Rectangle = 30;  //所有方框的高度
        const int H_ContactPoint = 4;
        const int Y_ControlContact = 2;  //控制线连接点
        const int Y_OffsetStep = 50;

        Font textFontTitle = new Font("Arial", 13f, FontStyle.Regular, GraphicsUnit.Pixel);
        Font textFont1 = new Font("Arial", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        Font textFont2 = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Pixel);
        Color textBorderColor = Color.FromArgb(0, 0, 255);

        /// <summary>
        /// 室内机总电源类型列表(R S T/L N/L1 L2 L3 N)
        /// </summary>
        List<string> powerTypeList = new List<string>();
        /// <summary>
        /// 室内机总电源电压列表
        /// </summary>
        List<string> powerVoltageList = new List<string>();
        /// <summary>
        /// 室内机总电源电流列表
        /// </summary>
        List<double> powerCurrentList = new List<double>();

        public ControllerWiringBLL(Project thisProj, AddFlow addFlowControllerWiring)
        {
            this._project = thisProj;
            this.addFlowControllerWiring = addFlowControllerWiring;

            isHitachi = thisProj.BrandCode == "H";

            bmpMeasureString = new Bitmap(300, 100);
            gMeasureString = Graphics.FromImage(this.bmpMeasureString);

            _imageDir = JCBase.Utility.MyConfig.WiringNodeImageDirectory;
        }

        public ControllerWiringBLL(Project thisProj, AddFlow addFlowControllerWiring, string Imagedir)  // This Constructor is created for Wiring Testing Purpose
        {
            this._project = thisProj;
            this.addFlowControllerWiring = addFlowControllerWiring;

            isHitachi = thisProj.BrandCode == "H";

            bmpMeasureString = new Bitmap(300, 100);
            gMeasureString = Graphics.FromImage(this.bmpMeasureString);

            _imageDir = Imagedir;
        }

        /// <summary>
        /// 初始化配线界面，清空Node和Line
        /// </summary>
        /// <param name="addFlowControllerWiring"></param>
        private void InitWiringNodes(AddFlow addFlowControllerWiring)
        {
            addFlowControllerWiring.MouseAction = MouseAction.None;
            addFlowControllerWiring.MultiSel = false;       // 是否允许多选
            addFlowControllerWiring.AllowDrop = false;     // 外界拖入addflow
            addFlowControllerWiring.AutoScroll = true;
            addFlowControllerWiring.ScrollbarsDisplayMode = ScrollbarsDisplayMode.SizeOfDiagramOnly;

            addFlowControllerWiring.CanChangeOrg = false;   // 是否允许更改连线的源节点
            addFlowControllerWiring.CanChangeDst = false;   // 是否允许更改连线的目标节点
            addFlowControllerWiring.CanDrawNode = false;   // 是否允许用户绘制节点
            addFlowControllerWiring.CanDrawLink = false;
            addFlowControllerWiring.CanDrawNode = false;

            addFlowControllerWiring.CanLabelEdit = false;  // 是否允许用户编辑节点文字
            addFlowControllerWiring.CanSizeNode = false;   // 是否允许用户更改节点尺寸
            addFlowControllerWiring.CanMoveNode = false;   // 是否允许用户拖动节点
            //addFlowWiring.PageGrid = new Grid(true, true, GridStyle.DottedLines, Color.LightGray, new Size(3000, 100));

            addFlowControllerWiring.LinkCreationMode = LinkCreationMode.AllNodeArea; // 绘制 Link 时选中的控制区域，整个Node区域
            addFlowControllerWiring.LinkHandleSize = HandleSize.Small;
            addFlowControllerWiring.LinkSelectionAreaWidth = LinkSelectionAreaWidth.Small;
            addFlowControllerWiring.SelectionHandleSize = HandleSize.Small;

            addFlowControllerWiring.SendToBack();
            addFlowControllerWiring.Images.Clear();

            addFlowControllerWiring.Controls.Clear();
            addFlowControllerWiring.Nodes.Clear();
            addFlowControllerWiring.AutoScrollPosition = new Point(0, 0);
        }

        /// <summary>
        /// 为Central Controller布线图创建结构
        /// </summary>
        /// <param name="controlGroup"></param>
        /// <returns></returns>
        private WiringNodeGroup CreateCCWiringNodeStructure(ControlGroup controlGroup)
        {
            PipingBLL pipBll = new PipingBLL(_project);
            WiringNodeGroup wiringGroup = new WiringNodeGroup();

            foreach (Model.Controller controller in _project.ControllerList)
            {
                if (controller.ControlGroupID == controlGroup.Id)
                {
                    int level = 1;
                    //if (controller.Type != ControllerType.Software) //软件不要显示 add by Shen Junjie on 2017/12/20
                    if (controller.Model.Trim() == "CSNET MANAGER 2 T10" || controller.Model.Trim() == "CSNET MANAGER 2 T15")
                    {
                        level = 1;
                    }
                    else if (controller.Model.Trim() == "HC-A64NET" || controller.Model.Trim() == "PSC-A160WEB1")
                    {
                        level = 2;
                    }
                    else
                    {
                        continue;
                    }
                    for (int n = 0; n < controller.Quantity; n++)
                    {
                        WiringNodeCentralControl wiringController = new WiringNodeCentralControl();
                        wiringController.Controller = controller;
                        wiringController.Level = level;
                        if (level == 1)
                        {
                            wiringGroup.ControllerListLevel1.Add(wiringController);
                        }
                        else
                        {
                            wiringGroup.ControllerListLevel2.Add(wiringController);
                        }
                    }
                }
            }


            foreach (SystemVRF sysItem in _project.SystemList)
            {
                if (sysItem.OutdoorItem != null && sysItem.ControlGroupID.Contains(controlGroup.Id))
                {
                    if (sysItem.MyPipingNodeOut == null)
                    {
                        if (sysItem.MyPipingNodeOutTemp != null)
                        {
                            pipBll.LoadPipingNodeStructure(sysItem);
                        }
                    }
                    if (sysItem.MyPipingNodeOut == null) continue;

                    WiringNodeOut wiringOut = new WiringNodeOut();
                    wiringOut.SystemItem = sysItem;
                    wiringGroup.OutdoorList.Add(wiringOut);

                    //遍历Piping的节点树，依次生成Controller Wiring节点树
                    PipingBLL.EachNode(sysItem.MyPipingNodeOut, (node) =>
                    {
                        if (node is MyNodeCH)
                        {
                            hasCHBox = true;
                            WiringNodeCH wiringCH = new WiringNodeCH();
                            MyNodeCH ch = node as MyNodeCH;
                            wiringCH.Model = ch.Model;
                            wiringCH.PowerSupply = ch.PowerSupply;
                            wiringCH.PowerLineType = ch.PowerLineType;
                            wiringCH.PowerCurrent = ch.PowerCurrent;
                            wiringOut.ChildNodes.Add(wiringCH);

                            PipingBLL.EachNode(ch.ChildNode, (node1) =>
                            {
                                if (node1 is MyNodeIn)
                                {
                                    RoomIndoor riItem = (node1 as MyNodeIn).RoomIndooItem;
                                    WiringNodeIn wiringIn = new WiringNodeIn();
                                    wiringIn.RoomIndoorItem = riItem;
                                    wiringCH.ChildNodes.Add(wiringIn);
                                }
                            });
                            return true;
                        }
                        else if (node is MyNodeMultiCH)
                        {
                            hasCHBox = true;

                            WiringNodeMultiCH wiringMCH = new WiringNodeMultiCH();
                            MyNodeMultiCH mch = node as MyNodeMultiCH;
                            wiringMCH.Model = mch.Model;
                            wiringMCH.PowerSupply = mch.PowerSupply;
                            wiringMCH.PowerCurrent = mch.PowerCurrent;
                            wiringMCH.PowerLineType = mch.PowerLineType;
                            wiringOut.ChildNodes.Add(wiringMCH);

                            for (int i = 0; i < mch.ChildNodes.Count; i++)
                            {
                                bool isNewBranchChild = true;
                                PipingBLL.EachNode(mch.ChildNodes[i], (node1) =>
                                {
                                    if (node1 is MyNodeIn)
                                    {
                                        RoomIndoor riItem = (node1 as MyNodeIn).RoomIndooItem;
                                        WiringNodeIn wiringIn = new WiringNodeIn();
                                        wiringIn.RoomIndoorItem = riItem;
                                        wiringMCH.ChildNodes.Add(wiringIn);

                                        if (isNewBranchChild)
                                        {
                                            wiringIn.IsNewBranchOfParent = true;
                                            isNewBranchChild = false;
                                        }
                                    }
                                });
                            }
                            return true;
                        }
                        else if (node is MyNodeIn)
                        {
                            RoomIndoor riItem = (node as MyNodeIn).RoomIndooItem;
                            WiringNodeIn wiringIn = new WiringNodeIn();
                            wiringIn.RoomIndoorItem = riItem;
                            wiringOut.ChildNodes.Add(wiringIn);
                        }
                        return false;
                    }, false);
                }
            }

            foreach (RoomIndoor exchanger in _project.ExchangerList)
            {
                if (exchanger.ControlGroupID.Contains(controlGroup.Id))
                {
                    WiringNodeIn nodeIn = new WiringNodeIn();
                    nodeIn.RoomIndoorItem = exchanger;
                    wiringGroup.TotalHeatExchangerList.Add(nodeIn);
                }
            }

            return wiringGroup;
        }

        /// <summary>
        /// 为系统布线图创建结构
        /// </summary>
        /// <param name="tnOut"></param>
        public void CreateWiringNodeStructure(SystemVRF sysItem)
        {
            MyNodeOut pipingOut = sysItem.MyPipingNodeOut;
            if (pipingOut == null) return;
            bool isHeatRecovery = PipingBLL.IsHeatRecovery(sysItem);
            WiringNodeOut wiringOut = utilPiping.createNodeOut_wiring();
            CreateWiringNodeStructure(wiringOut, null, pipingOut.ChildNode, isHeatRecovery, false);
            if (isHeatRecovery)
            {
                CreateWiringNodeStructure(wiringOut, null, pipingOut.ChildNode, isHeatRecovery, true);
            }
            sysItem.MyWiringNodeOut = wiringOut;
        }

        /// <summary>
        /// 为系统布线图创建结构
        /// </summary>
        /// <param name="pipingNode"></param>
        /// <param name="isHeatRecovery"></param>
        /// <returns></returns>
        private void CreateWiringNodeStructure(WiringNodeOut wiringOut, WiringNodeCH wiringCH, Node pipingNode, bool isHeatRecovery, bool coolingOnly)
        {
            if (pipingNode == null) return;

            if (pipingNode is MyNodeYP)
            {
                MyNodeYP nodeYP = pipingNode as MyNodeYP;
                for (int i = 0; i < nodeYP.MaxCount; i++)
                {
                    CreateWiringNodeStructure(wiringOut, wiringCH, nodeYP.ChildNodes[i], isHeatRecovery, coolingOnly);
                }
            }
            else if (!coolingOnly && pipingNode is MyNodeCH)
            {
                MyNodeCH nodeCH = pipingNode as MyNodeCH;
                wiringCH = utilPiping.createNodeCH_wiring(nodeCH.Model);
                wiringCH.Model = nodeCH.Model;
                wiringCH.PowerSupply = nodeCH.PowerSupply;
                wiringCH.PowerLineType = nodeCH.PowerLineType;
                wiringCH.PowerCurrent = nodeCH.PowerCurrent;
                wiringOut.ChildNodes.Add(wiringCH);

                CreateWiringNodeStructure(wiringOut, wiringCH, nodeCH.ChildNode, isHeatRecovery, coolingOnly);
            }
            else if (!coolingOnly && pipingNode is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = pipingNode as MyNodeMultiCH;
                wiringCH = utilPiping.createNodeMCH_wiring(nodeMCH.Model);
                wiringCH.Model = nodeMCH.Model;
                wiringCH.PowerSupply = nodeMCH.PowerSupply;
                wiringCH.PowerCurrent = nodeMCH.PowerCurrent;
                wiringCH.PowerLineType = nodeMCH.PowerLineType;
                wiringOut.ChildNodes.Add(wiringCH);

                int newBranchChildIndex = 0;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    CreateWiringNodeStructure(wiringOut, wiringCH, nodeMCH.ChildNodes[i], isHeatRecovery, coolingOnly);
                    if (wiringCH.ChildNodes.Count > newBranchChildIndex)
                    {
                        WiringNodeIn wiringChild = wiringCH.ChildNodes[newBranchChildIndex] as WiringNodeIn;
                        wiringChild.IsNewBranchOfParent = true;
                        newBranchChildIndex = wiringCH.ChildNodes.Count;
                    }
                }
            }
            else if (pipingNode is MyNodeIn)
            {
                MyNodeIn nodeIn = pipingNode as MyNodeIn;
                RoomIndoor riItem = nodeIn.RoomIndooItem;
                WiringNodeIn wiringIn = utilPiping.createNodeIn_wiring(riItem);

                if (isHeatRecovery)
                {
                    if (coolingOnly == (wiringCH == null))
                    {
                        if (wiringCH != null)
                        {
                            wiringCH.ChildNodes.Add(wiringIn);
                        }
                        else
                        {
                            wiringOut.ChildNodes.Add(wiringIn);
                        }
                    }
                }
                else
                {
                    wiringOut.ChildNodes.Add(wiringIn);
                }
            }
        }


        public bool BeforeWiringValid(ControlGroup controlGroup, out string errorMsg)
        {
            bool hasContoller = false;
            foreach (Controller ctrl in _project.ControllerList)
            {
                if (ctrl.ControlGroupID == controlGroup.Id)
                {
                    hasContoller = true;
                }
            }
            if (!hasContoller)
            {
                errorMsg = "ERROR! Please drag and drop central controllers to this group.";
                return false;
            }

            bool hasSystem = false;
            foreach (SystemVRF sysItem in _project.SystemList)
            {
                if (sysItem.ControlGroupID.Contains(controlGroup.Id))
                {
                    hasSystem = true;
                    break;
                }
            }
            if (hasSystem)
            {
                foreach (SystemVRF sysItem in _project.SystemList)
                {
                    if (sysItem.ControlGroupID.Contains(controlGroup.Id))
                    {
                        //室外机没有选型成功，或者没有配管图
                        if (sysItem.OutdoorItem == null ||
                            (sysItem.MyPipingNodeOut == null && sysItem.MyPipingNodeOutTemp == null))
                        {
                            errorMsg = "ERROR! There is some invalid systems in this group.";
                            return false;
                        }
                    }
                }
            }
            else
            {
                bool hasExchanger = false;
                foreach (RoomIndoor exchanger in _project.ExchangerList)
                {
                    if (exchanger.ControlGroupID.Contains(controlGroup.Id))
                    {
                        hasExchanger = true;
                        break;
                    }
                }
                if (!hasExchanger)
                {
                    errorMsg = "ERROR! Please drag and drop systems to this group.";
                    return false;
                }
            }
            errorMsg = "";
            return true;
        }

        /// 绘制配线图
        /// <summary>
        /// 绘制配线图，界面显示
        /// </summary>
        public bool DrawWiring(ControlGroup controlGroup)
        {
            InitWiringNodes(addFlowControllerWiring);
            LoadGroundImage();

            powerTypeList.Clear();
            powerVoltageList.Clear();
            powerCurrentList.Clear();

            //----------------
            // 检测是否可以绘图
            string errorMsg = "";
            if (!BeforeWiringValid(controlGroup, out errorMsg))
            {
                Label lbl2 = new Label();
                lbl2.Dock = DockStyle.Fill;
                lbl2.Text = errorMsg;
                lbl2.ForeColor = Color.Red;
                addFlowControllerWiring.Controls.Add(lbl2);
                return false;
            }

            WiringNodeGroup nodeGroup = CreateCCWiringNodeStructure(controlGroup);
            addFlowControllerWiring.Nodes.Add(nodeGroup);

            PointF pt = new PointF();

            float y = Y_ColumnHeader;
            float currentSystemY = 0;
            float legendY = 0;
            float maxY = 0;

            CreateTitleTextNode("Central Controller", new PointF(X_CC_Title, y));
            if (nodeGroup.OutdoorList.Count > 0)
            {
                CreateTitleTextNode("Outdoor Units", new PointF(X_Outdoor, y));
                CreateTitleTextNode("Indoor Units", new PointF(X_Indoor + 161, y));
            }
            CreateTitleTextNode("Remote Controller", new PointF(hasCHBox ? X_HR_RemoteController : X_RemoteController, y));

            //绘制Central Control Level 1
            y = Y_ColumnHeader + 100;
            foreach (WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel1)
            {
                DrawController(nodeController, y);
                y += Y_OffsetStep;
            }
            maxY = Math.Max(maxY, y);
            legendY = maxY;

            //绘制室外机、室内机、Remot Controller
            if (nodeGroup.OutdoorList.Count > 0)
            {
                y = Y_ColumnHeader + 30;
                for (int outdoorIndex = 0; outdoorIndex < nodeGroup.OutdoorList.Count; outdoorIndex++)
                {
                    WiringNodeOut nodeOut = nodeGroup.OutdoorList[outdoorIndex];
                    y += 20;

                    SystemVRF sysItem = nodeOut.SystemItem;

                    // 1. 绘制室外机节点
                    NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_ODU(sysItem.OutdoorItem, _project.BrandCode);
                    nodeOut.Location = new PointF(X_Outdoor, y); // 必须先设置好 Location
                    addFlowControllerWiring.Nodes.Add(nodeOut);

                    // 2. 室外机节点文字
                    pt = new PointF(0, 0);
                    CreateTextBoxNode(sysItem.Name, pt, W_Outdoor, false, nodeOut);

                    if (item_wiring.UnitCount > 1)
                    {
                        y += 20;
                        pt = new PointF(0, 20);
                        CreateTextBoxNode(sysItem.OutdoorItem.AuxModelName, pt, W_Outdoor, false, nodeOut);
                    }

                    y += 20;
                    currentSystemY = y;

                    // 3. 室外机上的电流线,控制线，以及室外机节点中的文字
                    for (int i = 0; i < item_wiring.UnitCount; ++i)
                    {
                        WiringNode nodeOutModel = DrawOutdoorModel(item_wiring, y, i);
                        nodeOut.Models.Add(nodeOutModel);

                        y += Y_OffsetStep;
                    }

                    maxY = Math.Max(maxY, y);
                    legendY = Math.Max(legendY, y);
                    y = currentSystemY;

                    WiringNode lastChild = null;
                    foreach (WiringNode node in nodeOut.ChildNodes)
                    {
                        if (node is WiringNodeCH)
                        {
                            if (lastChild is WiringNodeIn)
                            {
                                //如果单冷的室内机下面是CHBox， 
                                //则室内机的电源电压数据会被CHBox盖住，所以加大行间距
                                y += 10;
                            }

                            //绘制CH-Box
                            WiringNodeCH nodeCH = node as WiringNodeCH;
                            DrawCHBox(nodeCH, y);

                            //绘制CH-Box下的Indoor
                            foreach (WiringNode child1 in nodeCH.ChildNodes)
                            {
                                DrawIndoor(sysItem, child1 as WiringNodeIn, X_HR_Indoor, ref y);
                                y += Y_OffsetStep;
                            }

                            if (nodeCH.IsMultiCHBox && nodeCH.ChildNodes.Count > 1)
                            {
                                Node lastChildNode = nodeCH.ChildNodes.Last();
                                nodeCH.Size = new SizeF(nodeCH.Size.Width, lastChildNode.Location.Y + lastChildNode.Size.Height - nodeCH.Location.Y);
                            }

                            if (nodeCH.ChildNodes.Count == 0)
                            {
                                y += Y_OffsetStep;
                            }
                        }
                        else if (node is WiringNodeIn)
                        {
                            //绘制Outdoor下的Indoor
                            DrawIndoor(sysItem, node as WiringNodeIn, X_Indoor, ref y);
                            y += Y_OffsetStep;
                        }
                        lastChild = node;
                    }

                    maxY = Math.Max(maxY, y);
                    y = maxY;
                }
            }

            //绘制全热交换机
            if (nodeGroup.TotalHeatExchangerList.Count > 0)
            {
                if (nodeGroup.OutdoorList.Count > 0)
                {
                    y += 30;
                }
                else
                {
                    y = Y_ColumnHeader;
                }
                CreateTitleTextNode("Total Heat Exchanger Units", new PointF(X_Indoor + 80, y));
                y += 30;

                //全热交换机
                foreach (WiringNodeIn nodeExchanger in nodeGroup.TotalHeatExchangerList)
                {
                    DrawIndoor(null, nodeExchanger, X_Indoor, ref y);
                    y += Y_OffsetStep;
                }
            }

            maxY = Math.Max(maxY, y);

            //绘制Central Control Level 2
            y = Y_ColumnHeader + 30;
            for (int i = 0; i < nodeGroup.ControllerListLevel2.Count; i++)
            {
                if (i < nodeGroup.OutdoorList.Count)
                {
                    y = nodeGroup.OutdoorList[i].Models[0].Location.Y;
                }
                else if (i - nodeGroup.OutdoorList.Count < nodeGroup.TotalHeatExchangerList.Count)
                {
                    int i1 = i - nodeGroup.OutdoorList.Count;
                    y = nodeGroup.TotalHeatExchangerList[i1].Location.Y;
                }

                WiringNodeCentralControl nodeCC = nodeGroup.ControllerListLevel2[i];
                DrawController(nodeCC, y);
                y += Y_OffsetStep;
            }
            maxY = Math.Max(maxY, y);
            legendY = Math.Max(legendY, y);

            ////移动Central Control Level 2的y轴位置，使和室外机一一对齐
            //if (outdoorIndex < nodeGroup.ControllerListLevel2.Count && item_wiring.UnitCount > 0)
            //{
            //    //WiringNodeCentralControl nodeCC = nodeGroup.ControllerListLevel2[outdoorIndex];
            //    //float newCCY = nodeOut.Models[0].Location.Y;
            //    //float offsetCCY = newCCY - nodeCC.Location.Y;
            //    //nodeCC.Location = new PointF(nodeCC.Location.X, newCCY);
            //    //foreach (Node childOfCC in nodeCC.Children)
            //    //{
            //    //    childOfCC.Location = new PointF(childOfCC.Location.X, childOfCC.Location.Y + offsetCCY);
            //    //}
            //    //if (nodeCC.GroundContact != null)
            //    //{
            //    //    Link lkGround = nodeCC.GroundContact.Links[0];
            //    //    lkGround.Points[0] = new PointF(lkGround.Points[0].X, lkGround.Points[0].Y + offsetCCY);
            //    //    lkGround.Points[1] = new PointF(lkGround.Points[1].X, lkGround.Points[1].Y + offsetCCY);
            //    //}
            //}

            y = maxY;

            //绘制连线
            DrawLinks(nodeGroup);

            //绘制图例
            DrawLegend(nodeGroup, legendY);

            return true;
        }

        private void DrawLinks(WiringNodeGroup nodeGroup)
        {
            List<Node> powerContactList = new List<Node>();
            List<Node[]> joinEthernetList = new List<Node[]>();
            List<Node[]> joinHLinkList = new List<Node[]>();
            List<Node[]> horizontalHLinkList = new List<Node[]>();
            List<Node[]> systemHLinkList = new List<Node[]>();
            List<WiringNodeIn> indoorNodeList = new List<WiringNodeIn>();
            List<PointF> powerWireEndPoints = new List<PointF>();

            Node nodeStart, nodeEnd;
            PointF startPoint, endPoint;
            Link lnk1;

            WiringNode nodePreviousCentralControl = null;
            foreach (WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel1)
            {
                //CentralController的电源线
                powerContactList.Add(nodeController.PowerContact);

                //CentralController  Level 1左边串连
                if (nodePreviousCentralControl != null)
                {
                    joinEthernetList.Add(new Node[] {
                            nodePreviousCentralControl.LeftControlContact,
                            nodeController.LeftControlContact
                        });
                }
                nodePreviousCentralControl = nodeController;
            }

            nodePreviousCentralControl = null;
            foreach (WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel2)
            {
                //CentralController的电源线
                powerContactList.Add(nodeController.PowerContact);

                //CentralController Level 2左边串连
                if (nodePreviousCentralControl != null)
                {
                    joinEthernetList.Add(new Node[] {
                            nodePreviousCentralControl.LeftControlContact,
                            nodeController.LeftControlContact
                        });
                }
                nodePreviousCentralControl = nodeController;
            }

            int outdoorIndex = 0;
            foreach (WiringNodeOut nodeOut in nodeGroup.OutdoorList)
            {
                WiringNode nodePreviousOutModel = null;
                foreach (WiringNode nodeOutModel in nodeOut.Models)
                {
                    //outdoor的电源线
                    powerContactList.Add(nodeOutModel.PowerContact);

                    //室外机左边串连
                    if (nodePreviousOutModel != null)
                    {
                        joinHLinkList.Add(new Node[] {
                                nodePreviousOutModel.LeftControlContact,
                                nodeOutModel.LeftControlContact
                            });
                    }
                    nodePreviousOutModel = nodeOutModel;
                }

                WiringNode nodePreviousOutdoorChild = null;
                foreach (WiringNode node in nodeOut.ChildNodes)
                {
                    if (node is WiringNodeCH)
                    {
                        WiringNodeCH nodeCH = node as WiringNodeCH;
                        //CH Box的电源线
                        powerContactList.Add(nodeCH.PowerContact);

                        WiringNode nodePreviousCHBoxChild = null;
                        int newBranchIndex = 0;
                        foreach (WiringNode child1 in nodeCH.ChildNodes)
                        {
                            WiringNodeIn wiringIn = child1 as WiringNodeIn;
                            if (wiringIn == null) continue;

                            //CH Box后面的indoor的电源线
                            powerContactList.Add(child1.PowerContact);

                            //Indoor 和 RemoteController的连接线 
                            indoorNodeList.Add(wiringIn);

                            if (nodePreviousCHBoxChild != null && !wiringIn.IsNewBranchOfParent)
                            {
                                //CH Box后面的室内机左边串连
                                joinHLinkList.Add(new Node[] {
                                        nodePreviousCHBoxChild.LeftControlContact,
                                        child1.LeftControlContact
                                    });
                            }
                            else
                            {
                                //CHBox/Multi CHBox 和 Indoor的水平连接线
                                horizontalHLinkList.Add(new Node[] {
                                    nodeCH.RightControlContacts[newBranchIndex],
                                    child1.LeftControlContact
                                });
                                newBranchIndex++;
                            }

                            nodePreviousCHBoxChild = child1;
                        }
                    }
                    else if (node is WiringNodeIn)
                    {
                        WiringNodeIn nodeIn = node as WiringNodeIn;
                        //indoor的电源线
                        powerContactList.Add(nodeIn.PowerContact);

                        //Indoor 和 RemoteController的连接线
                        indoorNodeList.Add(nodeIn);
                    }

                    //室内机、CH Box 左边串连
                    if (nodePreviousOutdoorChild != null)
                    {
                        joinHLinkList.Add(new Node[] {
                                nodePreviousOutdoorChild.LeftControlContact,
                                node.LeftControlContact
                            });
                    }
                    nodePreviousOutdoorChild = node;
                }

                //室外机连接第一台室内机
                horizontalHLinkList.Add(new Node[] {
                        nodeOut.Models[0].RightControlContact,
                        nodeOut.ChildNodes[0].LeftControlContact
                    });

                //室外机连接上一个系统的最后一台室内机
                if (outdoorIndex>0 && outdoorIndex >= nodeGroup.ControllerListLevel2.Count)
                {
                    systemHLinkList.Add(new Node[] {
                            nodeGroup.OutdoorList[outdoorIndex - 1].ChildNodes.Last().LeftControlContact,
                            nodeOut.Models[0].RightControlContact
                        });
                }

                outdoorIndex++;
            }

            WiringNodeIn nodePreviousExchanger = null;
            foreach (WiringNodeIn nodeExchanger in nodeGroup.TotalHeatExchangerList)
            {
                //CentralController的电源线
                powerContactList.Add(nodeExchanger.PowerContact);

                //Indoor 和 RemoteController的连接线 
                indoorNodeList.Add(nodeExchanger);

                //全热交换机左边串连
                if (nodePreviousExchanger != null)
                {
                    joinHLinkList.Add(new Node[] {
                        nodePreviousExchanger.LeftControlContact,
                        nodeExchanger.LeftControlContact
                    });
                }
                nodePreviousExchanger = nodeExchanger;
            }

            //绘制Central Control Level 1 和 Level 2 之间的连线
            if (nodeGroup.ControllerListLevel1.Count > 0 &&
                nodeGroup.ControllerListLevel2.Count > 0)
            {
                nodeStart = nodeGroup.ControllerListLevel1[0].LeftControlContact;
                nodeEnd = nodeGroup.ControllerListLevel2[0].LeftControlContact;
                lnk1 = CreateEthernetWire();
                addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

                startPoint = utilPiping.getCenterPointF(nodeStart);
                endPoint = utilPiping.getCenterPointF(nodeEnd);
                lnk1.Points.Add(new PointF(startPoint.X - 8, startPoint.Y - 8));
                lnk1.Points.Add(new PointF(startPoint.X - 8, endPoint.Y - 8));
                lnk1.Points.Add(new PointF(endPoint.X - 8, endPoint.Y - 8));
            }

            //绘制Central Control Level 1 和 Level 2左边的纵向串连线
            foreach (Node[] nodes in joinEthernetList)
            {
                lnk1 = CreateEthernetWire();
                addFlowControllerWiring.AddLink(lnk1, nodes[0], nodes[1]);

                startPoint = utilPiping.getCenterPointF(nodes[0]);
                endPoint = utilPiping.getCenterPointF(nodes[1]);
                lnk1.Points.Add(new PointF(startPoint.X - 8, startPoint.Y + 8));
                lnk1.Points.Add(new PointF(endPoint.X - 8, endPoint.Y - 8));
            }

            //Central Control Level 2，每一个和对应的一台室外机连线
            int outdoorCount = nodeGroup.OutdoorList.Count;
            for (int i = 0; i < nodeGroup.ControllerListLevel2.Count; i++)
            {
                WiringNodeCentralControl nodeCC = nodeGroup.ControllerListLevel2[i];
                if (outdoorCount > i)
                {
                    WiringNodeOut nodeOut = nodeGroup.OutdoorList[i];
                    nodeStart = nodeCC.RightControlContact;
                    nodeEnd = nodeOut.Models[0].RightControlContact;
                    lnk1 = CreateHLinkWire();
                    addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

                    startPoint = utilPiping.getCenterPointF(nodeStart);
                    endPoint = utilPiping.getCenterPointF(nodeEnd);
                    lnk1.Points.Add(new PointF(startPoint.X + 8, startPoint.Y));
                    lnk1.Points.Add(new PointF(startPoint.X + 8, nodeOut.Location.Y - 10));
                    lnk1.Points.Add(new PointF(endPoint.X + 8, nodeOut.Location.Y - 10));
                    lnk1.Points.Add(new PointF(endPoint.X + 8, endPoint.Y - 8));
                }
                else if (nodeGroup.TotalHeatExchangerList.Count > i - outdoorCount)
                {
                    //CentralControl连接第一台室外机全热交换机
                    nodeStart = nodeCC.RightControlContact;
                    nodeEnd = nodeGroup.TotalHeatExchangerList[i - outdoorCount].LeftControlContact;
                    lnk1 = CreateHLinkWire();
                    addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

                    startPoint = utilPiping.getCenterPointF(nodeStart);
                    endPoint = utilPiping.getCenterPointF(nodeEnd);
                    lnk1.Points.Add(new PointF(startPoint.X + 8, startPoint.Y));
                    lnk1.Points.Add(new PointF(startPoint.X + 8, endPoint.Y));
                }
            }

            ////绘制第一台全热交换机与上一台室内机/CHBox的连线
            //if (nodeGroup.TotalHeatExchangerList.Count > 0 && nodeGroup.OutdoorList.Count > 0)
            //{
            //    WiringNodeOut lastOutdoorNode = nodeGroup.OutdoorList[nodeGroup.OutdoorList.Count - 1];
            //    if (lastOutdoorNode.ChildNodes.Count > 0)
            //    {
            //        WiringNode lastIndoorNode = lastOutdoorNode.ChildNodes[lastOutdoorNode.ChildNodes.Count - 1];
            //        nodeStart = lastIndoorNode.LeftControlContact;
            //        nodeEnd = nodeGroup.TotalHeatExchangerList[0].LeftControlContact;
            //        lnk1 = CreateHLinkWire();
            //        addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

            //        startPoint = utilPiping.getCenterPointF(nodeStart);
            //        endPoint = utilPiping.getCenterPointF(nodeEnd);
            //        lnk1.Points.Add(new PointF(startPoint.X - 40, startPoint.Y + 40));
            //        lnk1.Points.Add(new PointF(startPoint.X - 40, endPoint.Y));
            //        lnk1.Points[3] = endPoint;
            //    }
            //}

            ////绘制第2个及以下的CentralControl往上连线
            //for (int i = 1; i < nodeGroup.ControllerListLevel1.Count; i++)
            //{
            //    nodeStart = nodeGroup.ControllerListLevel1[i].RightControlContact;
            //    nodeEnd = nodeGroup.ControllerListLevel1[i - 1].RightControlContact;

            //    startPoint = utilPiping.getCenterPointF(nodeStart);
            //    endPoint = utilPiping.getCenterPointF(nodeEnd);

            //    endPoint = new PointF(startPoint.X + 8, endPoint.Y);
            //    //nodeEnd = CreateContactNode(new PointF(startPoint.X - nodeStart.Location.X, endPoint.Y - nodeStart.Location.Y), nodeStart);
            //    nodeEnd = utilPiping.createLinePoint(endPoint);
            //    addFlowControllerWiring.Nodes.Add(nodeEnd);

            //    lnk1 = CreateHLinkWire();
            //    lnk1.Line.Style = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
            //    addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);
            //    lnk1.Points.Add(new PointF(startPoint.X + 8, startPoint.Y));
            //    lnk1.Points[0] = startPoint;
            //    lnk1.Points[2] = endPoint;
            //}

            //绘制电源线
            for (int i = 0; i < powerContactList.Count; i++)
            {
                nodeStart = powerContactList[i];
                if (nodeStart == null) continue;

                startPoint = utilPiping.getCenterPointF(nodeStart);
                //endPoint = new PointF(startPoint.X + W_PowerWire, startPoint.Y + 4);
                endPoint = new PointF(startPoint.X + W_PowerWire, startPoint.Y); //改成直线

                nodeEnd = utilPiping.createLinePoint(endPoint);
                nodeEnd.DrawColor = Color.Transparent;
                addFlowControllerWiring.Nodes.Add(nodeEnd);
                nodeEnd.Parent = nodeStart.Parent;

                lnk1 = CreateLine(Color.Red);
                addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);
                //lnk1.Points.Add(new PointF(startPoint.X + 8, endPoint.Y));
                lnk1.Points[0] = startPoint;
                //lnk1.Points[2] = endPoint;
                lnk1.Points[1] = endPoint;

                powerWireEndPoints.Add(endPoint);
            }

            //绘制室外机、室内机、CH Box、全热交换机左边的纵向串连线
            foreach (Node[] nodes in joinHLinkList)
            {
                lnk1 = CreateHLinkWire();
                addFlowControllerWiring.AddLink(lnk1, nodes[0], nodes[1]);

                startPoint = utilPiping.getCenterPointF(nodes[0]);
                endPoint = utilPiping.getCenterPointF(nodes[1]);
                lnk1.Points.Add(new PointF(startPoint.X - 8, startPoint.Y + 8));
                lnk1.Points.Add(new PointF(endPoint.X - 8, endPoint.Y - 8));
            }

            //绘制水平连接的直线(除RemoteControl)
            foreach (Node[] nodes in horizontalHLinkList)
            {
                lnk1 = CreateHLinkWire();
                addFlowControllerWiring.AddLink(lnk1, nodes[0], nodes[1]);
            }

            //绘制系统间的连接线（最后一台室内机或CHBox到下一个系统的室外机）
            foreach (Node[] nodes in systemHLinkList)
            {
                lnk1 = CreateHLinkWire();
                addFlowControllerWiring.AddLink(lnk1, nodes[0], nodes[1]);

                startPoint = utilPiping.getCenterPointF(nodes[0]);
                endPoint = utilPiping.getCenterPointF(nodes[1]);
                lnk1.Points.Add(new PointF(startPoint.X - 20, startPoint.Y + 40));
                lnk1.Points.Add(new PointF(endPoint.X + 4, endPoint.Y - 18));
            }

            //绘制RemoteController的连接线
            DrawRCLinks(indoorNodeList, powerWireEndPoints);
        }

        private void DrawRCLinks(List<WiringNodeIn> indoorNodeList, List<PointF> roundPoints)
        {
            if (indoorNodeList == null || indoorNodeList.Count == 0) return;

            Node nodeStart, nodeEnd;
            PointF startPoint, endPoint;
            Link lnk1;

            List<Node[]> verticalWireNodes = new List<Node[]>();
            List<PointF[]> verticalWirePoints = new List<PointF[]>();

            //绘制共享RemoteControl的连线
            foreach (WiringNodeIn nodeInMain in indoorNodeList)
            {
                if (nodeInMain == null || nodeInMain.RoomIndoorItem == null || nodeInMain.RoomIndoorItem.IndoorItemGroup == null) continue;
                if (!nodeInMain.RoomIndoorItem.IsMainIndoor) continue;
                //查找跟这个主Indoor同一个Share组的所有indoor放到一个列表
                List<WiringNodeIn> indoorNodeGroup = new List<WiringNodeIn>();
                foreach (WiringNodeIn nodeIn in indoorNodeList)
                {
                    if (nodeIn == null) continue;
                    if (nodeIn == nodeInMain || nodeInMain.RoomIndoorItem.IndoorItemGroup.Contains(nodeIn.RoomIndoorItem))
                    {
                        indoorNodeGroup.Add(nodeIn);
                    }
                }

                //计算这个Share组的中indoor的最高位置和最低位置
                float[] yRange = new float[2];
                for (int i = 0; i < indoorNodeGroup.Count; i++)
                {
                    WiringNodeIn nodeIn = indoorNodeGroup[i];

                    PointF indoorContactPoint = utilPiping.getCenterPointF(nodeIn.RightControlContact);
                    if (i == 0)
                    {
                        yRange[0] = indoorContactPoint.Y;
                        yRange[1] = indoorContactPoint.Y;
                    }
                    else
                    {
                        yRange[0] = Math.Min(yRange[0], indoorContactPoint.Y);
                        yRange[1] = Math.Max(yRange[1], indoorContactPoint.Y);
                    }
                }

                //计算从室内机右边出来的横线的右端点的位置
                float endPointX = GetEndPointX(roundPoints, verticalWirePoints, yRange);
                //Share组串连竖线的起点和终点
                Node verticalWireTop = null;
                Node verticalWireBottom = null;
                //绘制这个Share组中每一个indoor（无remote controller的）右边的横线
                for (int i = 0; i < indoorNodeGroup.Count; i++)
                {
                    WiringNodeIn nodeIn = indoorNodeGroup[i];

                    nodeStart = nodeIn.RightControlContact;
                    startPoint = utilPiping.getCenterPointF(nodeStart);
                    endPoint = new PointF(endPointX, startPoint.Y);
                    if ((i > 0 && i < indoorNodeGroup.Count - 1) || nodeIn.ChildNodes.Count > 0)
                    {
                        nodeEnd = CreateContactNode(new PointF(endPoint.X - nodeStart.Location.X, nodeStart.Size.Height / 2), nodeStart);
                        //if (!nodeIn.RoomIndoorItem.IsMainIndoor && nodeIn.ChildNodes.Count > 0)
                        //{
                        //    //如果有普通的Remote Controller, 改变交叉点的位置
                        //    PointF startPoint1 = new PointF(endPoint.X - 10, endPoint.Y);
                        //    endPoint = new PointF(endPoint.X, endPoint.Y + 10);
                        //    nodeEnd.Location = new PointF(nodeEnd.Location.X, nodeEnd.Location.Y + 10);
                        //    Node nodeStart1 = utilPiping.createLinePoint(startPoint1);
                        //    lnk1 = CreateHLinkWire();
                        //    addFlowControllerWiring.AddLink(lnk1, nodeStart1, nodeEnd);
                        //    lnk1.Points[0] = startPoint1;
                        //    lnk1.Points[1] = endPoint;
                        //    lnk1.Points.Add(new PointF(startPoint1.X, endPoint.Y));
                        //}
                    }
                    else
                    {
                        nodeEnd = utilPiping.createLinePoint(endPoint);
                        addFlowControllerWiring.Nodes.Add(nodeEnd);
                    }

                    //没有自己的Remote Controller才需要在这里画横线
                    if (nodeIn.ChildNodes.Count == 0)
                    {
                        //从室内机右端出来的横向连线
                        lnk1 = CreateHLinkWire();
                        addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);
                        lnk1.Points[0] = startPoint;
                        lnk1.Points[1] = endPoint;
                    }

                    if (i == 0)
                    {
                        verticalWireTop = nodeEnd;
                    }
                    verticalWireBottom = nodeEnd;

                    ////主室内机到RemoteControl的连线
                    //if (nodeIn.RoomIndoorItem.IsMainIndoor)
                    //{
                    //    nodeStart = nodeEnd;
                    //    nodeEnd = nodeIn.ChildNodes[0].LeftControlContact;

                    //    lnk1 = CreateHLinkWire();
                    //    addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);
                    //}
                }

                //绘制共享RemoteControl的纵向连线

                //绘制共享RemoteControl的纵向连线
                lnk1 = CreateHLinkWire();
                //lnk1.Jump = Jump.Arc; //不使用跳线更清晰
                addFlowControllerWiring.AddLink(lnk1, verticalWireTop, verticalWireBottom);
                startPoint = new PointF(endPointX, yRange[0]);
                endPoint = new PointF(endPointX, yRange[1]);
                lnk1.Points[0] = startPoint;
                lnk1.Points[1] = endPoint;

                //保存纵向连线坐标点，后面一次批量绘制
                verticalWireNodes.Add(new Node[] { verticalWireTop, verticalWireBottom });
                verticalWirePoints.Add(new PointF[] { startPoint, endPoint });
            }

            //绘制各个Indoor 到 RemoteController的水平连线
            foreach (WiringNodeIn nodeIn in indoorNodeList)
            {
                if (nodeIn == null) continue;
                if (nodeIn.ChildNodes.Count == 0) continue;

                nodeStart = nodeIn.RightControlContact;
                startPoint = utilPiping.getCenterPointF(nodeStart);

                for (int j = 0; j < nodeIn.ChildNodes.Count; j++)
                {
                    WiringNode nodeRC = nodeIn.ChildNodes[j];
                    nodeEnd = nodeRC.LeftControlContact;
                    endPoint = utilPiping.getCenterPointF(nodeEnd);

                    lnk1 = CreateHLinkWire();
                    addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

                    if (j > 0)
                    {
                        float[] yRange = new float[2] { startPoint.Y, endPoint.Y };
                        float endPointX = GetEndPointX(roundPoints, verticalWirePoints, yRange);
                        lnk1.Points[0] = startPoint;
                        lnk1.Points[1] = endPoint;
                        lnk1.Points.Add(new PointF(endPointX, yRange[0]));
                        lnk1.Points.Add(new PointF(endPointX, yRange[1]));
                        verticalWirePoints.Add(new PointF[] {
                            new PointF(endPointX, yRange[0]),
                            new PointF(endPointX, yRange[1])
                        });
                    }
                }
            }

            //与纵向连线靠太近的共享遥控器往右调整位置
            foreach (WiringNodeIn nodeIn in indoorNodeList)
            {
                if (nodeIn == null) continue;
                if (nodeIn.ChildNodes.Count == 0) continue;

                foreach (WiringNode nodeRC in nodeIn.ChildNodes)
                {
                    Node leftContact = nodeRC.LeftControlContact;
                    PointF ptRC = utilPiping.getCenterPointF(leftContact);
                    float xRC = ptRC.X;
                    //计算不会被竖线覆盖的X坐标
                    for (int i = 0; i < verticalWireNodes.Count; i++)
                    {
                        startPoint = verticalWirePoints[i][0];
                        endPoint = verticalWirePoints[i][1];
                        if (ptRC.Y >= startPoint.Y && ptRC.Y <= endPoint.Y)
                        {
                            if (xRC < startPoint.X + 50)
                            {
                                xRC = startPoint.X + 50;
                            }
                        }
                    }
                    if (ptRC.X != xRC)
                    {
                        //移动Remote Controller节点和附属节点
                        float offsetX = xRC - ptRC.X;
                        ptRC.X = xRC;
                        nodeRC.Location = new PointF(nodeRC.Location.X + offsetX, nodeRC.Location.Y);
                        //leftContact.Location = new PointF(leftContact.Location.X + offsetX, leftContact.Location.Y);
                        if (leftContact.InLinks != null && leftContact.InLinks.Count > 0)
                        {
                            leftContact.InLinks[0].Points[leftContact.InLinks[0].Points.Count - 1] = ptRC;
                        }
                        foreach (Node childOfRC in nodeRC.Children)
                        {
                            childOfRC.Location = new PointF(childOfRC.Location.X + offsetX, childOfRC.Location.Y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 找出一个和现有垂直线条都不重合（保持最小10px的距离）的一个X坐标的位置
        /// </summary>
        /// <param name="roundPoints">所有电源线的右端点坐标</param>
        /// <param name="verticalWirePoints">已经画出的垂直线条的头尾坐标集合</param>
        /// <param name="yRange">将要画的垂直线条的Y坐标范围</param>
        /// <returns></returns>
        private float GetEndPointX(List<PointF> roundPoints, List<PointF[]> verticalWirePoints, float[] yRange)
        {
            float endPointX = 0;
            for (int i = 0; i < roundPoints.Count; i++)
            {
                PointF pt = roundPoints[i];
                if (pt.Y <= yRange[1] + 15 && pt.Y >= yRange[0])
                {
                    endPointX = Math.Max(endPointX, pt.X);
                }
            }
            //纵向连线的X坐标要超过室内机后面电源线的末端
            endPointX += 10;

            //纵向连线不能跟已有的纵向连线重合
            bool isWireOverwrite = true;
            while (isWireOverwrite)
            {
                isWireOverwrite = false;
                for (int i = 0; i < verticalWirePoints.Count; i++)
                {
                    PointF startPoint = verticalWirePoints[i][0];
                    PointF endPoint = verticalWirePoints[i][1];
                    if (startPoint.Y <= yRange[1] + 2 && endPoint.Y >= yRange[0] - 2 && Math.Abs(startPoint.X - endPointX) < 20)
                    {
                        //重合
                        isWireOverwrite = true;
                        endPointX = startPoint.X + 20;
                    }
                }
            }
            return endPointX;
        }

        public void GetCentralControllerPowerInfo(string model,
            out string powerLineTypeMark, out string powerPortName, out string voltage, out bool hasGroundWire)
        {
            powerLineTypeMark = "//"; //电源线2芯显示2根线
            voltage = "";
            powerPortName = "";
            hasGroundWire = false;

            JCHVRF.DAL.CentralControllerDAL dal = new JCHVRF.DAL.CentralControllerDAL();
            DataTable dt = dal.GetPowerSupply(_project.SubRegionCode, _project.BrandCode, model);
            if (dt.Rows.Count == 0)
            {
                return;
            }
            string powerSupply = Convert.ToString(dt.Rows[0]["PowerSupply"]);
            string powerLineType = Convert.ToString(dt.Rows[0]["PowerLineType"]);
            voltage = powerSupply;
            if (!string.IsNullOrEmpty(powerSupply))
            {
                string[] arr = powerSupply.Split(new string[] { "=>", "/" }, StringSplitOptions.None);
                if (arr.Length > 2)
                {
                    if (arr[2].Trim().ToLower() == "earth wiring")
                    {
                        hasGroundWire = true;
                    }
                }
                if (arr.Length > 1)
                {
                    powerPortName = arr[0].Trim();
                    voltage = arr[1].Trim();
                }
            }
            if (!string.IsNullOrEmpty(powerLineType))
            {
                if (powerLineType == "2")
                {
                    powerLineTypeMark = "//"; //电源线2芯 显示2根线
                }
                else if (powerLineType == "3")
                {
                    powerLineTypeMark = "///"; //电源线3芯 显示3根线
                }
                else if (powerLineType == "4")
                {
                    powerLineTypeMark = "////"; //电源线4芯 显示4根线
                }
            }
        }

        private void DrawController(WiringNodeCentralControl nodeController, float y)
        {
            float x = nodeController.Level == 1 ? X_CC_Level1 : X_CC_Level2;

            Controller ctrl = nodeController.Controller;
            nodeController.Location = new PointF(x, y); // 必须先设置好 Location
            nodeController.Text = ctrl.Model;
            addFlowControllerWiring.Nodes.Add(nodeController);
            SetWiringNodeBorder(nodeController, W_CentralControl);

            //左控制器线连接点
            nodeController.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeController);

            //右控制器线连接点
            nodeController.RightControlContact = CreateContactNode(new PointF(W_CentralControl, Y_ControlContact), nodeController);

            PointF pt;
            string powerLineTypeMark;
            string powerPortName;
            string voltage;
            bool hasGroundWire;
            GetCentralControllerPowerInfo(nodeController.Controller.Model, out powerLineTypeMark, out powerPortName, out voltage, out hasGroundWire);

            if (voltage != "-")
            {
                //Central Controller 右电源线型号
                pt = new PointF(W_CentralControl + 45, 5);
                CreateLittleTextNode(powerPortName, pt, nodeController);

                //Central Controller 右电源线电压
                pt = new PointF(W_CentralControl + 45, 16);
                CreateLittleTextNode(voltage, pt, nodeController);

                //Central Controller 电源线线型标识
                pt = new PointF(W_CentralControl + 10, 6);
                CreatePowerLineMark(powerLineTypeMark, pt, nodeController);

                //Central Controller 电源线连接点
                pt = new PointF(W_CentralControl, 15);
                nodeController.PowerContact = CreateContactNode(pt, nodeController);

                if (hasGroundWire)
                {
                    //接地线
                    DrawGroundNode(nodeController);
                }
            }
        }

        private WiringNode DrawOutdoorModel(NodeElement_Wiring item_wiring, float y, int index)
        {
            PointF pt;

            WiringNode nodeOutdoorModel = new WiringNode();
            nodeOutdoorModel.Location = new PointF(X_Outdoor, y);
            nodeOutdoorModel.Text = item_wiring.ModelGroup[index];
            addFlowControllerWiring.Nodes.Add(nodeOutdoorModel);
            SetWiringNodeBorder(nodeOutdoorModel, W_Outdoor);

            if (index < 4)
            {
                //左控制线号码
                pt = new PointF(-22, 5);
                CreateLittleTextNode(item_wiring.StrGroup1[index], pt, nodeOutdoorModel);

                if (item_wiring.UnitCount > 0)
                {
                    //左控制器线连接点
                    pt = new PointF(0, Y_ControlContact);
                    nodeOutdoorModel.LeftControlContact = CreateContactNode(pt, nodeOutdoorModel);
                }
            }

            //右电源线型号
            pt = new PointF(W_Outdoor + 45, 5);
            CreateLittleTextNode(item_wiring.StrGroup2[index], pt, nodeOutdoorModel);

            //右电源线电压
            pt = new PointF(W_Outdoor + 45, 16);
            CreateLittleTextNode(item_wiring.StrGroup3[index], pt, nodeOutdoorModel);

            // 室外机电源线线型标识
            pt = new PointF(W_Outdoor + 10, 6);
            CreatePowerLineMark(item_wiring.StrGroup4[index], pt, nodeOutdoorModel);

            //室外机电源线连接点
            pt = new PointF(W_Outdoor, 15);
            nodeOutdoorModel.PowerContact = CreateContactNode(pt, nodeOutdoorModel);

            if (index == 0)
            {
                //右控制线号码
                pt = new PointF(W_Outdoor + 10, -8);
                CreateLittleTextNode(item_wiring.Str1, pt, nodeOutdoorModel);

                //右控制器线连接点
                pt = new PointF(W_Outdoor, Y_ControlContact);
                nodeOutdoorModel.RightControlContact = CreateContactNode(pt, nodeOutdoorModel);
            }

            //接地线
            DrawGroundNode(nodeOutdoorModel);

            return nodeOutdoorModel;
        }

        private void DrawCHBox(WiringNodeCH nodeCH, float y)
        {
            PointF pt;

            NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_CH(nodeCH.Model, nodeCH.PowerSupply, nodeCH.PowerLineType, nodeCH.PowerCurrent);
            nodeCH.Location = new PointF(X_CHBox, y);
            nodeCH.Text = item_wiring.ShortModel;
            addFlowControllerWiring.Nodes.Add(nodeCH);
            SetWiringNodeBorder(nodeCH, W_CHBox);

            //EU的左右标注要互换
            //Change the wiring terminal name of Single CH-Box for all regions. modified by Shen Junjie on 2018/04/16
            //Single CH Box 1,2 is connected to Outdoor unit 1,2				
            //Single CH Box 3,4 is connected to Indoor unit 1,2
            string rightNumber = item_wiring.StrGroup1[0];
            string leftNumber = item_wiring.Str1;

            if (nodeCH.IsMultiCHBox)
            {
                //Multi CHBox和每一个Indoor的都有一个连接点  add by Shen Junjie 2018/5/5
                int newBranchIndex = 0;
                for (int i = 0; i < nodeCH.ChildNodes.Count; i++)
                {
                    WiringNodeIn wiringIn = nodeCH.ChildNodes[i] as WiringNodeIn;
                    if (wiringIn != null && wiringIn.IsNewBranchOfParent)
                    {
                        rightNumber = (newBranchIndex * 2 + 3) + " " + (newBranchIndex * 2 + 4);

                        //右控制线号码
                        pt = new PointF(W_CHBox + 10, -8 + i * Y_OffsetStep);
                        CreateLittleTextNode(rightNumber, pt, nodeCH);

                        //右控制器线连接点
                        nodeCH.RightControlContacts.Add(CreateContactNode(new PointF(W_CHBox, Y_ControlContact + i * Y_OffsetStep), nodeCH));

                        newBranchIndex++;
                    }
                }
            }
            else
            {
                //右控制线号码
                pt = new PointF(W_CHBox + 10, -8);
                CreateLittleTextNode(rightNumber, pt, nodeCH);

                //右控制器线连接点
                nodeCH.RightControlContacts.Add(CreateContactNode(new PointF(W_CHBox, Y_ControlContact), nodeCH));
            }

            //左控制线号码
            pt = new PointF(-22, 5);
            CreateLittleTextNode(leftNumber, pt, nodeCH);

            //左控制器线连接点
            nodeCH.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeCH);

            //右电源线型号
            pt = new PointF(W_CHBox + 45, 5);
            CreateLittleTextNode(item_wiring.StrGroup2[0], pt, nodeCH);

            //右电源线电压
            pt = new PointF(W_CHBox + 45, 16);
            CreateLittleTextNode(item_wiring.StrGroup3[0], pt, nodeCH);

            // 室外机电源线线型标识
            pt = new PointF(W_CHBox + 10, 6);
            CreatePowerLineMark(item_wiring.StrGroup4[0], pt, nodeCH);

            //CH Box电源线连接点
            pt = new PointF(W_CHBox, 15);
            nodeCH.PowerContact = CreateContactNode(pt, nodeCH);

            //接地线
            DrawGroundNode(nodeCH);
        }

        private void DrawIndoor(SystemVRF sysItem, WiringNodeIn nodeIn, float x, ref float y)
        {
            PointF pt;

            int powerIndex = 0;
            bool isNewPower = false;
            string outdoorType = "";
            if (sysItem != null && sysItem.OutdoorItem != null)
            {
                outdoorType = sysItem.OutdoorItem.Type;
            }
            NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_IDU(nodeIn.RoomIndoorItem.IndoorItem, _project.BrandCode, outdoorType, ref powerTypeList, ref powerVoltageList, ref powerCurrentList, ref powerIndex, ref isNewPower);
            nodeIn.Location = new PointF(x, y);
            string indoorName = nodeIn.RoomIndoorItem.IndoorName;
            if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.RoomName))
            {
                indoorName = nodeIn.RoomIndoorItem.RoomName + ":" + indoorName;
            }
            if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.RoomID))
            {
                string floorName = "";
                foreach (Floor f in _project.FloorList)
                {
                    foreach (Room rm in f.RoomList)
                    {
                        if (rm.Id == nodeIn.RoomIndoorItem.RoomID)
                        {
                            floorName = f.Name;
                        }
                    }
                }
                indoorName = floorName + ":" + indoorName;
            }
            //nodeIn.Text = indoorName + "\n" + item_wiring.ShortModel;
            addFlowControllerWiring.Nodes.Add(nodeIn);
            SetWiringNodeBorder(nodeIn, W_Indoor);

            //室内机名称(含楼层和房间）
            CreateTextNode(indoorName, nodeIn.Size.Width, new PointF(0, 3), textFont1, nodeIn);

            //室内机型号
            CreateTextNode(item_wiring.ShortModel, nodeIn.Size.Width, new PointF(0, 18), textFont2, nodeIn);

            //右控制线号码
            pt = new PointF(W_Indoor + 10, -8);
            CreateLittleTextNode(item_wiring.Str1, pt, nodeIn);

            //左控制线号码
            pt = new PointF(-22, 5);
            CreateLittleTextNode(item_wiring.StrGroup1[0], pt, nodeIn);

            //右电源线型号
            pt = new PointF(W_Indoor + 45, 5);
            CreateLittleTextNode(item_wiring.StrGroup2[0], pt, nodeIn);

            //右电源线电压
            pt = new PointF(W_Indoor + 45, 16);
            CreateLittleTextNode(item_wiring.StrGroup3[0], pt, nodeIn);

            // 室内机电源线线型标识
            pt = new PointF(W_Indoor + 10, 6);
            CreatePowerLineMark(item_wiring.StrGroup4[0], pt, nodeIn);

            //左控制器线连接点
            nodeIn.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeIn);

            //右控制器线连接点
            nodeIn.RightControlContact = CreateContactNode(new PointF(W_Indoor, Y_ControlContact), nodeIn);

            //室内机电源线连接点
            nodeIn.PowerContact = CreateContactNode(new PointF(W_Indoor, 15), nodeIn);

            //接地线
            DrawGroundNode(nodeIn);

            //绘制Remote Control
            int RCIndex = 0;
            List<Accessory> ListAccessory = nodeIn.RoomIndoorItem.ListAccessory;
            if (ListAccessory != null && ListAccessory.Count > 0)
            {
                foreach (Accessory acc in ListAccessory)
                {
                    if (acc == null) continue;
                    switch (acc.Type.ToLower())
                    {
                        case "remote controler":
                        case "remote control switch":
                        case "half-size remote control switch":
                        case "receiver kit for wireless control":
                            if (RCIndex > 0)
                            {
                                y += Y_OffsetStep;
                            }
                            WiringNode nodeRC = DrawRemoteControlWiring(nodeIn, acc, y);
                            if (nodeRC != null)
                            {
                                nodeIn.ChildNodes.Add(nodeRC);
                                RCIndex++;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 绘制单个Remote Control Switch
        /// </summary>
        /// <param name="parent">室内机节点</param>
        /// <param name="RC">室内机的Remote Control Switch附件</param>
        /// <param name="RCIndex"></param>
        /// <param name="imgDir"></param>
        /// <returns></returns>
        private WiringNode DrawRemoteControlWiring(Node parent, Accessory RC, float y)
        {
            WiringNode nodeRC = null;
            float x = (hasCHBox ? X_HR_RemoteController : X_RemoteController);
            string model = "";
            if (RC.BrandCode == "Y")
                model = RC.Model_York;
            else
                model = RC.Model_Hitachi;
            if (model != "")
            {
                //增加Remote Controler节点
                nodeRC = new WiringNode();
                nodeRC.Location = new PointF(x, y);
                nodeRC.Text = model;
                addFlowControllerWiring.Nodes.Add(nodeRC);
                SetWiringNodeBorder(nodeRC, W_RemoteController);
                nodeRC.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeRC);

                //Share标注
                if (RC.IsShared)
                {
                    CreateLittleTextNode("Shared", new PointF(W_RemoteController + 5, 10), nodeRC);
                }
            }
            return nodeRC;
        }

        /// <summary>
        /// 创建AddFlow.Link对象
        /// </summary>
        /// <returns></returns>
        private Link CreateLine(Color color)
        {
            Link lnk = new Link();
            lnk.DrawWidth = 1;
            lnk.DrawColor = color;
            lnk.ArrowDst = Arrow.None;
            lnk.Stretchable = false;
            lnk.Selectable = false;
            lnk.OwnerDraw = true;
            lnk.Line.Style = LineStyle.Polyline;

            lnk.ConnectionStyleOrg = ConnectionStyle.Inside;
            lnk.AdjustDst = true;
            lnk.AdjustOrg = true;

            return lnk;
        }

        private Link CreateEthernetWire()
        {
            //return CreateLine(Color.FromArgb(0, 153, 153));
            return CreateLine(utilPiping.colorWiring);
        }

        private Link CreateHLinkWire()
        {
            return CreateLine(utilPiping.colorWiring);
        }

        /// <summary>
        /// 创建文字节点--Wiring (小字)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pt"></param>
        /// <param name="parent"></param>
        /// <param name="isLineMark">是否电源线标记/// | //，如果是，和电源线一样设置为红色</param>
        /// <returns></returns>
        private Node CreateLittleTextNode(string text, PointF pt, Node parent = null)
        {
            return CreateTextNode(text, pt, textFont2, parent);
        }

        /// <summary>
        /// 创建文字节点--Wiring(标题大号字)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pt"></param>
        /// <param name="parent"></param>
        /// <param name="isLineMark">是否电源线标记/// | //，如果是，和电源线一样设置为红色</param>
        /// <returns></returns>
        private Node CreateTitleTextNode(string text, PointF pt)
        {
            return CreateTextNode(text, pt, textFontTitle, null);
        }

        /// <summary>
        /// 创建文字节点--Wiring
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pt"></param>
        /// <param name="parent"></param>
        /// <param name="isLineMark">是否电源线标记/// | //，如果是，和电源线一样设置为红色</param>
        /// <returns></returns>
        private Node CreateTextNode(string text, PointF pt, Node parent = null)
        {
            return CreateTextNode(text, pt, textFont1, parent);
        }

        private Node CreateTextNode(string text, float width, PointF pt, Font ft, Node parent = null)
        {
            Node label = CreateTextNode(text, pt, ft, parent);
            label.Alignment = Alignment.CenterMIDDLE;
            label.Size = new SizeF(width, label.Size.Height);
            return label;
        }

        /// <summary>
        /// 创建文字节点--Wiring
        /// </summary>
        /// <returns></returns>
        private Node CreateTextNode(string text, PointF pt, Font ft, Node parent = null)
        {
            Node label = new Node();

            // 设置字体并量取文字标题的尺寸
            gMeasureString.PageUnit = GraphicsUnit.Pixel;            //单位为像素 
            label.Font = ft;
            label.TextColor = Color.Black;
            SizeF size = gMeasureString.MeasureString(text, label.Font);

            label.Transparent = true;
            label.Text = text;

            label.Alignment = Alignment.LeftJustifyMIDDLE;
            label.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            label.Size = new SizeF(size.Width + 15, size.Height);
            label.DrawColor = Color.Transparent;
            label.Logical = false;
            label.Selectable = false;
            label.AttachmentStyle = AttachmentStyle.Item;

            if (parent != null)
            {
                label.Location = UtilEMF.OffsetLocation(pt, parent.Location);
                parent.AddFlow.Nodes.Add(label);
                label.Parent = parent;
            }
            else
            {
                label.Location = pt;
                addFlowControllerWiring.Nodes.Add(label);
            }
            return label;
        }

        private Node CreatePowerLineMark(string text, PointF pt, Node parent = null)
        {
            //如果是电源线标记，需要放大字体
            Node label = CreateTextNode(text, pt, utilPiping.textFont_wiring_linemark, parent);
            label.TextColor = Color.Red;
            return label;
        }

        /// <summary>
        /// 文字居中的节点，可以有虚线框
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pt"></param>
        /// <param name="width"></param>
        /// <param name="showBorder"></param>
        /// <returns></returns>
        private Node CreateTextBoxNode(string text, PointF pt, float width, bool showBorder, Node parent = null)
        {
            Node node = CreateTextNode(text, pt, parent);
            node.Alignment = Alignment.CenterMIDDLE;
            node.DashStyle = DashStyle.Dash;
            if (showBorder)
            {
                node.Size = new SizeF(width, H_Rectangle);
                node.DrawColor = textBorderColor;
            }
            else
            {
                node.Size = new SizeF(width, node.Size.Height);
            }
            return node;
        }

        private void LoadGroundImage()
        {
            string imgFile = Path.Combine(_imageDir, "Ground.png");

            Image img = new Bitmap(imgFile);
            addFlowControllerWiring.Images.Add(img);

            _groudImageIndex = addFlowControllerWiring.Images.Count - 1;
        }

        private Node DrawGroundNode(WiringNode parent)
        {
            //新增接地线节点
            float x = parent.Location.X + parent.Size.Width;
            float y = parent.Location.Y + parent.Size.Height;
            MyNodeGround_Wiring node = new MyNodeGround_Wiring();
            node.Location = new PointF(x + 50, y - 2);
            node.ImageIndex = _groudImageIndex;
            node.ImagePosition = ImagePosition.CenterBottom;
            node.Size = new SizeF(20, 20);
            node.BackMode = BackMode.Transparent;
            node.Selectable = false;
            //node.ZOrder = 10;

            node.DrawColor = Color.Transparent;
            parent.AddFlow.Nodes.Add(node);
            node.Parent = parent;

            //接地连接线
            Link lnk1 = CreateLine(utilPiping.colorWarning);
            lnk1.DashStyle = DashStyle.Dash;
            Node groundContact = CreateContactNode(new PointF(parent.Size.Width, parent.Size.Height - 5), parent);
            parent.AddFlow.AddLink(lnk1, groundContact, node);
            lnk1.Points[1] = new PointF(node.Location.X + 10, node.Location.Y);
            parent.GroundContact = groundContact;

            return node;
        }

        private void SetWiringNodeBorder(Node node, float width)
        {
            node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            node.DashStyle = DashStyle.Dash;
            node.Font = textFont1;
            node.Alignment = Alignment.CenterMIDDLE;
            node.Size = new SizeF(width, H_Rectangle);
            node.DrawColor = textBorderColor;
        }

        /// <summary>
        /// 接触点（圆点）
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private Node CreateContactNode(PointF pt, Node parent)
        {
            Node node = new Node();
            node.Shape = new Shape(ShapeStyle.Connector, ShapeOrientation.so_0);
            if (parent != null)
            {
                pt = UtilEMF.OffsetLocation(pt, parent.Location);
            }
            pt.X -= W_ContactPoint / 2;
            pt.Y -= H_ContactPoint / 2;
            node.Location = pt;
            node.Size = new SizeF(W_ContactPoint, H_ContactPoint);

            node.DrawColor = utilPiping.colorWiring;
            node.FillColor = utilPiping.colorWiring;
            node.Logical = false;
            node.Selectable = false;
            node.AttachmentStyle = AttachmentStyle.Item;
            if (parent != null)
            {
                parent.AddFlow.Nodes.Add(node);
                node.Parent = parent;
            }
            else
            {
                addFlowControllerWiring.Nodes.Add(node);
            }
            return node;
        }

        /// <summary>
        /// 绘制图例
        /// </summary>
        private void DrawLegend(WiringNodeGroup nodeGroup, float y)
        {
            //CAD显示中文有问题，这里一律改成英文
            string title = "Legend";
            string text0 = "ODU, IDU, CH-Box, Remote Controller, Central Controller Units";
            string text1 = "Transmission Line";
            string text2 = "Electrical power line";
            string text3 = "Contact";
            string text4 = "Line Type(2 Lines, 3 Lines, 4 Lines)";

            float x = 50;
            y += 100;

            CreateTitleTextNode(title, new PointF(x, y));
            CreateTextBoxNode("", new PointF(x, y + 30), 90, true);
            CreateTextNode(text0, new PointF(x + 100, y + 30 + 8));
            CreateTextNode(text1, new PointF(x + 100, y + 80 - 7));
            CreateTextNode(text2, new PointF(x + 100, y + 100 - 7));
            CreateTextNode(text3, new PointF(x + 100, y + 120 - 7));
            CreateTextNode(text4, new PointF(x + 100, y + 140 - 7));

            Node nodeStart, nodeEnd;
            Link lnk1;

            //控制器线
            nodeStart = utilPiping.createLinePoint(new PointF(x, y + 80));
            nodeEnd = utilPiping.createLinePoint(new PointF(x + 90, y + 80));
            nodeEnd.DrawColor = Color.Transparent;
            addFlowControllerWiring.Nodes.Add(nodeEnd);
            lnk1 = CreateHLinkWire();
            addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

            //电源线
            nodeStart = utilPiping.createLinePoint(new PointF(x, y + 100));
            nodeEnd = utilPiping.createLinePoint(new PointF(x + 90, y + 100));
            nodeEnd.DrawColor = Color.Transparent;
            addFlowControllerWiring.Nodes.Add(nodeEnd);
            lnk1 = CreateLine(Color.Red);
            addFlowControllerWiring.AddLink(lnk1, nodeStart, nodeEnd);

            CreateContactNode(new PointF(x + 30, y + 120), null);
            CreateContactNode(new PointF(x + 60, y + 120), null);
            CreatePowerLineMark("//", new PointF(x + 10 - 6, y + 140 - 9));
            CreatePowerLineMark("///", new PointF(x + 40 - 8, y + 140 - 9));
            CreatePowerLineMark("////", new PointF(x + 70 - 10, y + 140 - 9));
        }

        /// 获取AddFlow画布实际尺寸
        /// <summary>
        /// 获取AddFlow画布实际尺寸
        /// </summary>
        /// <param name="addFlowItem"></param>
        /// <returns></returns>
        public static SizeF GetAddFlowSize(AddFlow addFlowItem)
        {
            SizeF sz = new SizeF(0, 0);
            foreach (Node item in addFlowItem.Nodes)
            {
                float width = item.Location.X + item.Size.Width + 100;
                float height = item.Location.Y + item.Size.Height + 100;
                sz.Width = (sz.Width < width) ? width : sz.Width;
                sz.Height = (sz.Height < height) ? height : sz.Height;
            }
            return sz;
        }

        // 计算执行 Fitwindow 命令后的缩放比例
        /// <summary>
        /// 计算执行 Fitwindow 命令后的缩放比例
        /// </summary>
        /// <param name="addflowItem"></param>
        /// <returns></returns>
        public static float GetFitWindowZoom(AddFlow addflowItem)
        {
            SizeF sz = GetAddFlowSize(addflowItem); // 计算 mx & my
            // 若未撑满画布，则默认scale为100%，否则以实际撑满画布为准
            float f = addflowItem.Size.Width / sz.Width;
            float f1 = addflowItem.Size.Height / sz.Height;
            f = f < f1 ? f : f1;
            if (f > 1)
                f = 1; // 是否限制 FitWindow 时的 Zoom值 不超过100%
            return f;
        }
    }
}
