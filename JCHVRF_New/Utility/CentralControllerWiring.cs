using JCHVRF.Model;
using JCHVRF.MyPipingBLL;
using Lassalle.WPF.Flow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ng = JCHVRF.Model.NextGen;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using mod = JCHVRF.Model;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using System.IO;


namespace JCHVRF_New.Utility
{
    class CentralControllerWiring
    {
        private AddFlow addFlowWiring;

        public UtilityWiring utilWiring = new UtilityWiring();
        JCHVRF.Model.Project thisProject;
        private ng.SystemVRF sysItem1 = new ng.SystemVRF();

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
        const int W_PowerWire = 160;
        const int H_Rectangle = 30;
        const int H_ContactPoint = 4;
        const int Y_ControlContact = 2;
        const int Y_OffsetStep = 50;

        bool hasCHBox = false;
        private NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
        Bitmap bmpMeasureString;
        Graphics gMeasureString;
        List<string> powerTypeList = new List<string>();
        List<string> powerVoltageList = new List<string>();
        List<double> powerCurrentList = new List<double>();      

        public CentralControllerWiring(Project project, ControlSystem controlSystem, AddFlow AddFlowWiring)
        {
            thisProject = project;
            addFlowWiring = AddFlowWiring;
            utilWiring = new UtilityWiring(thisProject);
            bmpMeasureString = new Bitmap(300, 100);
            gMeasureString = Graphics.FromImage(bmpMeasureString);
            DoDrawingWiring(thisProject, controlSystem);            
        }
        private void DoDrawingWiring(Project project, ControlSystem system)
        {
            InitWiringNodes(addFlowWiring);

            powerTypeList.Clear();
            powerVoltageList.Clear();
            powerCurrentList.Clear();

            ControlGroup controlGroup = project.ControlGroupList.Find(x => x.ControlSystemID.Equals(system.Id));

            string errorMsg = "";
            //if (!BeforeWiringValid(controlGroup, out errorMsg))
            //{
            //    //Caption lbl2 = new Caption();
            //    //lbl2.Dock = DockStyle.Fill;
            //    //lbl2.Text = errorMsg;
            //    ////lbl2.Fill = new SolidColorBrush(Color.FromRgb());
            //    //addFlowWiring.AddCaption(lbl2);   
            //    return false;
            //}

            //if(controlGroup.IsValid)
            //{
            //    return;
            //}

            ng.WiringNodeGroup nodeGroup = CreateCCWiringNodeStructure(controlGroup);
            addFlowWiring.AddNode(nodeGroup);

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


            y = Y_ColumnHeader + 100;
            foreach (ng.WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel1)
            {
                DrawController(nodeController, y);
                y += Y_OffsetStep;
            }
            maxY = Math.Max(maxY, y);
            legendY = maxY;


            #region OldCCWiringLogic
            if (nodeGroup.OutdoorList.Count > 0)
            {
                y = Y_ColumnHeader + 30;
                for (int outdoorIndex = 0; outdoorIndex < nodeGroup.OutdoorList.Count; outdoorIndex++)
                {
                    ng.WiringNodeOut nodeOut = nodeGroup.OutdoorList[outdoorIndex];
                    y += 20;

                    ng.SystemVRF sysItem = nodeOut.SystemItem;


                    NodeElement_Wiring item_wiring = utilWiring.GetNodeElement_Wiring_ODU(sysItem.OutdoorItem, mod.Project.CurrentProject.BrandCode);
                    nodeOut.Location = new Point(X_Outdoor, y);
                    addFlowWiring.AddNode(nodeOut);


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


                    for (int i = 0; i < item_wiring.UnitCount; ++i)
                    {
                        ng.WiringNode nodeOutModel = DrawOutdoorModel(item_wiring, y, i);
                        nodeOut.Models.Add(nodeOutModel);

                        y += Y_OffsetStep;
                    }

                    maxY = Math.Max(maxY, y);
                    legendY = Math.Max(legendY, y);
                    y = currentSystemY;

                    ng.WiringNode lastChild = null;
                    foreach (ng.WiringNode node in nodeOut.ChildNodes)
                    {
                        if (node is ng.WiringNodeCH)
                        {
                            if (lastChild is ng.WiringNodeIn)
                            {
                                y += 10;
                            }

                            ng.WiringNodeCH nodeCH = node as ng.WiringNodeCH;
                            DrawCHBox(nodeCH, y);

                            foreach (ng.WiringNode child1 in nodeCH.ChildNodes)
                            {
                                DrawIndoor(sysItem, child1 as ng.WiringNodeIn, X_HR_Indoor, ref y);
                                y += Y_OffsetStep;
                            }

                            if (nodeCH.IsMultiCHBox && nodeCH.ChildNodes.Count > 1)
                            {
                                Node lastChildNode = nodeCH.ChildNodes.Last();
                                nodeCH.Size = new System.Windows.Size(nodeCH.Size.Width, lastChildNode.Location.Y + lastChildNode.Size.Height - nodeCH.Location.Y);
                            }

                            if (nodeCH.ChildNodes.Count == 0)
                            {
                                y += Y_OffsetStep;
                            }
                        }
                        else if (node is ng.WiringNodeIn)
                        {
                            DrawIndoor(sysItem, node as ng.WiringNodeIn, X_Indoor, ref y);
                            y += Y_OffsetStep;
                        }
                        lastChild = node;
                    }

                    maxY = Math.Max(maxY, y);
                    y = maxY;
                }
            }

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

                foreach (ng.WiringNodeIn nodeExchanger in nodeGroup.TotalHeatExchangerList)
                {
                    DrawIndoor(null, nodeExchanger, X_Indoor, ref y);
                    y += Y_OffsetStep;
                }
            }

            maxY = Math.Max(maxY, y);

            y = Y_ColumnHeader + 30;
            for (int i = 0; i < nodeGroup.ControllerListLevel2.Count; i++)
            {
                if (i < nodeGroup.OutdoorList.Count)
                {
                    y = (float)nodeGroup.OutdoorList[i].Models[0].Location.Y;
                }
                else if (i - nodeGroup.OutdoorList.Count < nodeGroup.TotalHeatExchangerList.Count)
                {
                    int i1 = i - nodeGroup.OutdoorList.Count;
                    y = (float)nodeGroup.TotalHeatExchangerList[i1].Location.Y;
                }

                ng.WiringNodeCentralControl nodeCC = nodeGroup.ControllerListLevel2[i];
                DrawController(nodeCC, y);
                y += Y_OffsetStep;
            }
            maxY = Math.Max(maxY, y);
            legendY = Math.Max(legendY, y);



            y = maxY;

            DrawLinks(nodeGroup);
            #endregion

            DrawLegend(nodeGroup, legendY);
           // return true;
        }


        private void DrawController(ng.WiringNodeCentralControl nodeController, float y)
        {
            Font textFont1 = new Font("Arial", 11f, GraphicsUnit.Pixel);
            float x = nodeController.Level == 1 ? X_CC_Level1 : X_CC_Level2;

            Controller ctrl = nodeController.Controller;
            nodeController.Location = utilWiring.convertPointFToWinPoint(new PointF(x, y));
            nodeController.Text = ctrl.Model;
            SetWiringNodeBorder(nodeController, W_Outdoor);
            addFlowWiring.AddNode(nodeController);

            nodeController.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeController);

            nodeController.RightControlContact = CreateContactNode(new PointF(W_CentralControl, Y_ControlContact), nodeController);

            PointF pt;
            string powerLineTypeMark;
            string powerPortName;
            string voltage;
            bool hasGroundWire;
            GetCentralControllerPowerInfo(nodeController.Controller.Model, out powerLineTypeMark, out powerPortName, out voltage, out hasGroundWire);

            if (voltage != "-")
            {
                pt = new PointF(W_CentralControl + 30, 5);
                CreateLittleTextNode(powerPortName, (float)nodeController.Size.Width, pt, textFont1, nodeController);

                pt = new PointF(W_CentralControl + 30, 16);
                CreateLittleTextNode(voltage, (float)nodeController.Size.Width, pt, textFont1, nodeController);

                pt = new PointF(W_CentralControl + 10, 8);
                CreatePowerLineMark(powerLineTypeMark, (float)nodeController.Size.Width, pt, textFont1, nodeController);

                pt = new PointF(W_CentralControl, 15);
                nodeController.PowerContact = CreateContactNode(pt, nodeController);

                if (hasGroundWire)
                {
                    DrawGroundNode(nodeController);
                }
            }
        }

        public bool BeforeWiringValid(mod.ControlGroup controlGroup, out string errorMsg)
        {
            bool hasContoller = false;
            foreach (mod.Controller ctrl in mod.Project.CurrentProject.ControllerList)
            {
                if (ctrl.ControlGroupID.Contains(controlGroup.Id))
                {
                    hasContoller = true;
                }
            }
            if (!hasContoller)
            {
                errorMsg = "WARNING! Please drag and drop central controllers to this group.";
                return false;
            }

            bool hasSystem = false;
            foreach (JCHVRF.Model.NextGen.SystemVRF sysItem in mod.Project.CurrentProject.SystemListNextGen)
            {
                if (sysItem.ControlGroupID.Contains(controlGroup.Id))
                {
                    hasSystem = true;
                    break;
                }
            }
            if (hasSystem)
            {
                foreach (JCHVRF.Model.NextGen.SystemVRF sysItem in mod.Project.CurrentProject.SystemListNextGen)
                {
                    if (sysItem.ControlGroupID.Contains(controlGroup.Id))
                    {
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
                foreach (mod.RoomIndoor exchanger in mod.Project.CurrentProject.ExchangerList)
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

        private void InitWiringNodes(AddFlow addFlowWiring)
        {
            addFlowWiring.Clear();
            addFlowWiring.ScrollIncrement = new Size(0, 0);
        }

        private ng.WiringNodeGroup CreateCCWiringNodeStructure(mod.ControlGroup controlGroup)
        {
            NextGenBLL.PipingBLL pipBll = new NextGenBLL.PipingBLL(mod.Project.CurrentProject);
            ng.WiringNodeGroup wiringGroup = new ng.WiringNodeGroup();
            foreach (mod.Controller controller in mod.Project.CurrentProject.ControllerList)
            {
                if (controller.ControlGroupID == controlGroup.Id)
                {
                    int level = 1;
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
                        ng.WiringNodeCentralControl wiringController = new ng.WiringNodeCentralControl();
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

            foreach (JCHVRF.Model.NextGen.SystemVRF system in mod.Project.CurrentProject.SystemListNextGen)
            {                
                ng.SystemVRF sysItem = system;                          

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

                    ng.WiringNodeOut wiringOut = new ng.WiringNodeOut();
                    wiringOut.SystemItem = sysItem;
                    wiringGroup.OutdoorList.Add(wiringOut);

                    NextGenBLL.PipingBLL.EachNode(sysItem.MyPipingNodeOut, (node) =>
                    {
                        if (node is ng.MyNodeCH)
                        {
                            hasCHBox = true;
                            ng.WiringNodeCH wiringCH = new ng.WiringNodeCH();
                            ng.MyNodeCH ch = node as ng.MyNodeCH;
                            wiringCH.Model = ch.Model;
                            wiringCH.PowerSupply = ch.PowerSupply;
                            wiringCH.PowerLineType = ch.PowerLineType;
                            wiringCH.PowerCurrent = ch.PowerCurrent;
                            wiringOut.ChildNodes.Add(wiringCH);

                            NextGenBLL.PipingBLL.EachNode(ch.ChildNode, (node1) =>
                            {
                                if (node1 is ng.MyNodeIn)
                                {
                                    mod.RoomIndoor riItem = (node1 as ng.MyNodeIn).RoomIndooItem;
                                    ng.WiringNodeIn wiringIn = new ng.WiringNodeIn();
                                    wiringIn.RoomIndoorItem = riItem;
                                    wiringCH.ChildNodes.Add(wiringIn);
                                }
                            });
                            return true;
                        }
                        else if (node is ng.MyNodeMultiCH)
                        {
                            hasCHBox = true;

                            ng.WiringNodeMultiCH wiringMCH = new ng.WiringNodeMultiCH();
                            ng.MyNodeMultiCH mch = node as ng.MyNodeMultiCH;
                            wiringMCH.Model = mch.Model;
                            wiringMCH.PowerSupply = mch.PowerSupply;
                            wiringMCH.PowerCurrent = mch.PowerCurrent;
                            wiringMCH.PowerLineType = mch.PowerLineType;
                            wiringOut.ChildNodes.Add(wiringMCH);

                            for (int i = 0; i < mch.ChildNodes.Count; i++)
                            {
                                bool isNewBranchChild = true;
                                NextGenBLL.PipingBLL.EachNode(mch.ChildNodes[i], (node1) =>
                                {
                                    if (node1 is ng.MyNodeIn)
                                    {
                                        mod.RoomIndoor riItem = (node1 as ng.MyNodeIn).RoomIndooItem;
                                        ng.WiringNodeIn wiringIn = new ng.WiringNodeIn();
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
                        else if (node is ng.MyNodeIn)
                        {
                            mod.RoomIndoor riItem = (node as ng.MyNodeIn).RoomIndooItem;
                            ng.WiringNodeIn wiringIn = new ng.WiringNodeIn();
                            wiringIn.RoomIndoorItem = riItem;
                            wiringOut.ChildNodes.Add(wiringIn);
                        }
                        return false;
                    }, false);
                }
            }

            foreach (mod.RoomIndoor exchanger in mod.Project.CurrentProject.ExchangerList)
            {
                if (exchanger.ControlGroupID.Contains(controlGroup.Id))
                {
                    ng.WiringNodeIn nodeIn = new ng.WiringNodeIn();
                    nodeIn.RoomIndoorItem = exchanger;
                    wiringGroup.TotalHeatExchangerList.Add(nodeIn);
                }
            }
            return wiringGroup;
        }

        public void CreateWiringNodeStructure(ng.SystemVRF sysItem)
        {
            ng.MyNodeOut pipingOut = sysItem.MyPipingNodeOut;
            if (pipingOut == null) return;
            bool isHeatRecovery = NextGenBLL.PipingBLL.IsHeatRecovery(sysItem);
            ng.WiringNodeOut wiringOut = utilWiring.createNodeOut_wiring();
            CreateWiringNodeStructure(wiringOut, null, pipingOut.ChildNode, isHeatRecovery, false);
            if (isHeatRecovery)
            {
                CreateWiringNodeStructure(wiringOut, null, pipingOut.ChildNode, isHeatRecovery, true);
            }
            if(wiringOut.AddFlow!=null)
            {
                sysItem.MyWiringNodeOut = wiringOut;
            }
          
        }

        private void CreateWiringNodeStructure(ng.WiringNodeOut wiringOut, ng.WiringNodeCH wiringCH, Node pipingNode, bool isHeatRecovery, bool coolingOnly)
        {
            if (pipingNode == null) return;

            if (pipingNode is ng.MyNodeYP)
            {
                ng.MyNodeYP nodeYP = pipingNode as ng.MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    CreateWiringNodeStructure(wiringOut, wiringCH, nodeYP.ChildNodes[i], isHeatRecovery, coolingOnly);
                }
            }
            else if (!coolingOnly && pipingNode is ng.MyNodeCH)
            {
                ng.MyNodeCH nodeCH = pipingNode as ng.MyNodeCH;
                wiringCH = utilWiring.createNodeCH_wiring(nodeCH.Model);
                wiringCH.Model = nodeCH.Model;
                wiringCH.PowerSupply = nodeCH.PowerSupply;
                wiringCH.PowerLineType = nodeCH.PowerLineType;
                wiringCH.PowerCurrent = nodeCH.PowerCurrent;
                wiringOut.ChildNodes.Add(wiringCH);

                CreateWiringNodeStructure(wiringOut, wiringCH, nodeCH.ChildNode, isHeatRecovery, coolingOnly);
            }
            else if (!coolingOnly && pipingNode is ng.MyNodeMultiCH)
            {
                ng.MyNodeMultiCH nodeMCH = pipingNode as ng.MyNodeMultiCH;
                wiringCH = utilWiring.createNodeMCH_wiring(nodeMCH.Model);
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
                        ng.WiringNodeIn wiringChild = wiringCH.ChildNodes[newBranchChildIndex] as ng.WiringNodeIn;
                        wiringChild.IsNewBranchOfParent = true;
                        newBranchChildIndex = wiringCH.ChildNodes.Count;
                    }
                }
            }
            else if (pipingNode is ng.MyNodeIn)
            {
                ng.MyNodeIn nodeIn = pipingNode as ng.MyNodeIn;
                mod.RoomIndoor riItem = nodeIn.RoomIndooItem;
                ng.WiringNodeIn wiringIn = utilWiring.createNodeIn_wiring(riItem);

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

        private Node CreateTextBoxNode(string text, PointF pt, float width, bool showBorder, Node parent)
        {
            Node node = CreateTextNode(text, pt, parent);
            node.TextPosition = TextPosition.CenterMiddle;
            node.DashStyle = System.Windows.Media.DashStyles.Dash;
            if (showBorder)
            {
                node.Size = new Size(width, H_Rectangle);
                node.Foreground = System.Windows.Media.Brushes.Blue;
            }
            else
            {
                node.Size = new Size(width, node.Size.Height);
            }
            return node;
        }

        private Node CreateTextNode(string text, PointF pt, Node parent)
        {
            Font textFont1 = new Font("Arial", 11f, GraphicsUnit.Pixel);
            return CreateTextNode(text, pt, textFont1, parent);
        }

        private Node CreateTextNode(string text, float width, PointF pt, Font ft, Node parent)
        {
            Node label = CreateTextNode(text, pt, ft, parent);
            label.TextPosition = TextPosition.CenterTop;
            label.Size = new Size(width, label.Size.Height);
            return label;
        }

        private Node CreateLittleTextNode(string text, float width, PointF pt, Font ft, Node parent)
        {
            Node label = CreateTextNode(text, pt, ft, parent);
            label.TextPosition = TextPosition.CenterTop;
            label.Size = new Size(width, label.Size.Height);
            label.FontSize = 9f;
            label.Foreground = System.Windows.Media.Brushes.Black;
            return label;
        }

        private Node CreateTextNode(string text, PointF pt, Font ft, Node parent)
        {
            Node label = new Node();

            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 11f;
            SizeF size = gMeasureString.MeasureString(text, ft);
            label.Text = text;
            label.Geometry = new System.Windows.Media.EllipseGeometry(new Rect(0, 0, 0, 0));
            label.Size = new Size(size.Width + 15, size.Height);
            label.IsSelectable = false;

            if (parent != null)
            {
                PointF convertedPtf = new PointF();
                PointF convertedPt = new PointF();
                convertedPtf = utilWiring.convertSystemPointToDrawingPoint(parent.Location);
                convertedPt = UtilEMF.OffsetLocation(pt, convertedPtf);
                label.Location = utilWiring.convertPointFToWinPoint(convertedPt);
                parent.AddFlow.AddNode(label);
            }
            else
            {
                label.Location = utilWiring.convertPointFToWinPoint(pt);
                addFlowWiring.AddNode(label);
            }
            return label;
        }


        private ng.WiringNode DrawOutdoorModel(NodeElement_Wiring item_wiring, float y, int index)
        {
            PointF pt;
            PointF convertedPt = new PointF();
            Font textFont1 = new Font("Arial", 11f, GraphicsUnit.Pixel);

            ng.WiringNode nodeOutdoorModel = new ng.WiringNode();
            convertedPt = new PointF(X_Outdoor, y);
            nodeOutdoorModel.Location = utilWiring.convertPointFToWinPoint(convertedPt);
            nodeOutdoorModel.Text = item_wiring.ModelGroup[index];
            SetWiringNodeBorder(nodeOutdoorModel, W_Outdoor);
            addFlowWiring.AddNode(nodeOutdoorModel);


            if (index < 4)
            {
                pt = new PointF(-65, 5);

                CreateLittleTextNode(item_wiring.StrGroup1[index], (float)nodeOutdoorModel.Size.Width, pt, textFont1, nodeOutdoorModel);

                if (item_wiring.UnitCount > 0)
                {
                    pt = new PointF(0, Y_ControlContact);
                    nodeOutdoorModel.LeftControlContact = CreateContactNode(pt, nodeOutdoorModel);
                }
            }
           
            const int W_Outdoor1 = 50;
            pt = new PointF(W_Outdoor + 45, 5);
            if (item_wiring.StrGroup2.Count() > index)
            {
                CreateLittleTextNode(item_wiring.StrGroup2[index], (float)nodeOutdoorModel.Size.Width, pt, textFont1, nodeOutdoorModel);
            }
            pt = new PointF(W_Outdoor + 45, 16);
            if (item_wiring.StrGroup3.Count() > index)
            {
                CreateLittleTextNode(item_wiring.StrGroup3[index], (float)nodeOutdoorModel.Size.Width, pt, textFont1, nodeOutdoorModel);
            }
            pt = new PointF(W_Outdoor + 10, 8);
            if (item_wiring.StrGroup4.Count() > index)
            {
                CreatePowerLineMark(item_wiring.StrGroup4[index], (float)nodeOutdoorModel.Size.Width, pt, textFont1, nodeOutdoorModel);
            }
            pt = new PointF(W_Outdoor, 15);
            nodeOutdoorModel.PowerContact = CreateContactNode(pt, nodeOutdoorModel);

            if (index == 0)
            {
                pt = new PointF(W_Outdoor1 + 17, -12);
                CreateLittleTextNode(item_wiring.Str1, (float)nodeOutdoorModel.Size.Width, pt, textFont1, nodeOutdoorModel);

                pt = new PointF(W_Outdoor, Y_ControlContact);
                nodeOutdoorModel.RightControlContact = CreateContactNode(pt, nodeOutdoorModel);
            }
            DrawGroundNode(nodeOutdoorModel);
            return nodeOutdoorModel;
        }

        private void SetWiringNodeBorder(Node node, float width)
        {
            node.Geometry = new System.Windows.Media.RectangleGeometry(new Rect(0, 0, 100, 100));
            node.DashStyle = System.Windows.Media.DashStyles.Dash;
            node.FontFamily = new System.Windows.Media.FontFamily("Arial");
            node.FontSize = 11f;
            node.Size = new Size(width, 30);
            node.Stroke = System.Windows.Media.Brushes.Blue;
            node.IsEditable = false;
            node.IsSelectable = false;
        }

        private Node CreateLittleTextNode(string text, PointF pt, Node parent = null)
        {
            Font textFont2 = new Font("Arial", 8f, GraphicsUnit.Pixel);
            return CreateTextNode(text, pt, textFont2, parent);
        }

        private Node CreateContactNode(PointF pt, Node parent)
        {
            Node node = new Node();
            PointF convertedPt = new PointF();
            convertedPt = utilWiring.convertSystemPointToDrawingPoint(parent.Location);
            if (parent != null)
            {
                pt = UtilEMF.OffsetLocation(pt, convertedPt);
            }
            pt.X -= W_ContactPoint / 2;
            pt.Y -= H_ContactPoint / 2;

            node.Location = utilWiring.convertPointFToWinPoint(pt);
            node.Size = new Size(W_ContactPoint, H_ContactPoint);

            node.Stroke = System.Windows.Media.Brushes.Gray;
            node.Fill = System.Windows.Media.Brushes.Gray;
            node.IsSelectable = false;
            if (parent != null)
            {
                parent.AddFlow.AddNode(node);
            }
            else
            {
                addFlowWiring.AddNode(node);
            }
            return node;
        }

        private Node CreatePowerLineMark(string text, PointF pt, Node parent = null)
        {
            Node label = CreateTextNode(text, pt, utilWiring.textFont_wiring_linemark, parent);
            label.Foreground = System.Windows.Media.Brushes.Red;
            return label;
        }

        private Node CreatePowerLineMark(string text, float width, PointF pt, Font ft, Node parent)
        {
            Node label = CreateTextNode(text, pt, ft, parent);
            label.TextPosition = TextPosition.CenterBottom;
            label.Size = new Size(width, label.Size.Height);
            label.Foreground = System.Windows.Media.Brushes.Red;
            return label;
        }

        private Node DrawGroundNode(ng.WiringNode parent)
        {
            float x = (float)(parent.Location.X + parent.Size.Width);
            float y = (float)(parent.Location.Y + parent.Size.Height);
            ng.MyNodeGround_Wiring node = new ng.MyNodeGround_Wiring();
            PointF convertedPt = new PointF(x + 40, y);
            node.Location = utilWiring.convertPointFToWinPoint(convertedPt);
            string imageDir = @"../../Image/TypeImages/";
            string imgFile = Path.Combine(imageDir, "Ground.png");
            System.Windows.Media.Imaging.BitmapImage img = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,," + imgFile, UriKind.RelativeOrAbsolute));
            ImageSource imgSrc = img;
            node.Image = imgSrc;
            node.ImagePosition = ImagePosition.LeftTop;
            node.Size = new Size(20, 20);
            node.IsSelectable = false;
            node.Stroke = System.Windows.Media.Brushes.Transparent;
            parent.AddFlow.AddNode(node);

            PointF convertedPt1 = new PointF((float)(parent.Size.Width), (float)(parent.Size.Height));
            PointF convertedPt2 = new PointF((float)(node.Location.X + 10), (float)(node.Location.Y));

            Point pt = utilWiring.convertPointFToWinPoint(convertedPt2);
            Node contactPt = new Node();
            contactPt.Location = pt;
            Node groundContact = CreateContactNode(convertedPt1, parent);
            Link lnk1 = CreateLine(groundContact, contactPt, utilPiping.colorWarning);
            lnk1.DashStyle = System.Windows.Media.DashStyles.Dash;
            parent.AddFlow.AddLink(lnk1);

            parent.GroundContact = groundContact;
            return node;
        }

        private Link CreateLine(Node src, Node dst, System.Windows.Media.Brush color)
        {
            Link lnk = new Link(src, dst, "", addFlowWiring);
            lnk.Stroke = color;
            lnk.IsStretchable = false;
            lnk.IsSelectable = false;
            lnk.LineStyle = LineStyle.Polyline;
            lnk.IsAdjustDst = false;
            lnk.IsAdjustOrg = false;
            return lnk;
        }

        private void DrawIndoor(ng.SystemVRF sysItem, ng.WiringNodeIn nodeIn, float x, ref float y)
        {
            PointF pt;
            Font textFont1 = new Font("Arial", 11f, GraphicsUnit.Pixel);
            Font textFont2 = new Font("Arial", 8f, GraphicsUnit.Pixel);

            int powerIndex = 0;
            bool isNewPower = false;
            string outdoorType = "";
            if (sysItem != null && sysItem.OutdoorItem != null)
            {
                outdoorType = sysItem.OutdoorItem.Type;
            }
            NodeElement_Wiring item_wiring = utilWiring.GetNodeElement_Wiring_IDU(nodeIn.RoomIndoorItem.IndoorItem, mod.Project.CurrentProject.BrandCode, outdoorType, ref powerTypeList, ref powerVoltageList, ref powerCurrentList, ref powerIndex, ref isNewPower);

            if (Project.CurrentProject.BrandCode == "Y" && nodeIn.RoomIndoorItem.IndoorItem != null)
            {
                item_wiring.ShortModel = nodeIn.RoomIndoorItem.IndoorItem.ModelFull;
            }

            PointF convertedPt = new PointF(x, y);
            nodeIn.Location = utilWiring.convertPointFToWinPoint(convertedPt);
            string indoorName = nodeIn.RoomIndoorItem.IndoorName;
            if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.RoomName))
            {
                indoorName = nodeIn.RoomIndoorItem.RoomName;
            }
            if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.RoomID))
            {
                string floorName = "";
                foreach (Floor f in mod.Project.CurrentProject.FloorList)
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
            SetWiringNodeBorder(nodeIn, W_Indoor);
            addFlowWiring.AddNode(nodeIn);

            const int W_Indoor1 = 150;

            CreateTextNode(indoorName, (float)nodeIn.Size.Width, new PointF(0, 3), textFont1, nodeIn);

            CreateTextNode(item_wiring.ShortModel, (float)nodeIn.Size.Width, new PointF(0, 18), textFont1, nodeIn);

            pt = new PointF(W_Indoor1 - 35, -12);
            CreateLittleTextNode(item_wiring.Str1, (float)nodeIn.Size.Width, pt, textFont1, nodeIn);

            pt = new PointF(W_Indoor1 - 270, 5);
            CreateLittleTextNode(item_wiring.StrGroup1[0], (float)nodeIn.Size.Width, pt, textFont1, nodeIn);

            pt = new PointF(W_Indoor1 + 45, 5);
            CreateLittleTextNode(item_wiring.StrGroup2[0], (float)nodeIn.Size.Width, pt, textFont1, nodeIn);

            pt = new PointF(W_Indoor1 + 45, 16);
            CreateLittleTextNode(item_wiring.StrGroup3[0], (float)nodeIn.Size.Width, pt, textFont1, nodeIn);

            pt = new PointF(W_Indoor1 + 10, 8);
            CreatePowerLineMark(item_wiring.StrGroup4[0], (float)nodeIn.Size.Width, pt, textFont1, nodeIn);

            nodeIn.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeIn);

            nodeIn.RightControlContact = CreateContactNode(new PointF(W_Indoor, Y_ControlContact), nodeIn);

            nodeIn.PowerContact = CreateContactNode(new PointF(W_Indoor, 15), nodeIn);

            DrawGroundNode(nodeIn);

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
                            //case "Remote Control Cable":
                            //case "3P Connector Cable":
                            if (RCIndex > 0)
                            {
                                y += Y_OffsetStep;
                            }
                            ng.WiringNode nodeRC = DrawRemoteControlWiring(nodeIn, acc, y);
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

        private ng.WiringNode DrawRemoteControlWiring(Node parent, Accessory RC, float y)
        {
            ng.WiringNode nodeRC = null;
            PointF convertedPt;
            float x = (hasCHBox ? X_HR_RemoteController : X_RemoteController);
            string model = "";
            if (RC.BrandCode == "Y")
                model = RC.Model_York;
            else
                model = RC.Model_Hitachi;
            if (model != "")
            {
                nodeRC = new ng.WiringNode();
                convertedPt = new PointF(x, y);
                nodeRC.Location = utilWiring.convertPointFToWinPoint(convertedPt);
                nodeRC.Text = model;
                SetWiringNodeBorder(nodeRC, W_RemoteController);
                addFlowWiring.AddNode(nodeRC);
                nodeRC.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeRC);

                if (RC.IsShared)
                {
                    CreateLittleTextNode("Shared", new PointF(W_RemoteController + 5, 10), nodeRC);
                }
            }
            return nodeRC;
        }

        private void DrawLinks(ng.WiringNodeGroup nodeGroup)
        {
            List<Node> powerContactList = new List<Node>();
            List<Node[]> joinEthernetList = new List<Node[]>();
            List<Node[]> joinHLinkList = new List<Node[]>();
            List<Node[]> horizontalHLinkList = new List<Node[]>();
            List<Node[]> systemHLinkList = new List<Node[]>();
            List<ng.WiringNodeIn> indoorNodeList = new List<ng.WiringNodeIn>();
            List<PointF> powerWireEndPoints = new List<PointF>();

            Node nodeStart, nodeEnd;
            PointF startPoint, endPoint;
            Link lnk1, lvl1_to_lvl2_Lnk;

            ng.WiringNode nodePreviousCentralControl = null;
            foreach (ng.WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel1)
            {
                powerContactList.Add(nodeController.PowerContact);
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
            foreach (ng.WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel2)
            {
                powerContactList.Add(nodeController.PowerContact);
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
            foreach (ng.WiringNodeOut nodeOut in nodeGroup.OutdoorList)
            {
                ng.WiringNode nodePreviousOutModel = null;
                foreach (ng.WiringNode nodeOutModel in nodeOut.Models)
                {
                    powerContactList.Add(nodeOutModel.PowerContact);
                    if (nodePreviousOutModel != null)
                    {
                        joinHLinkList.Add(new Node[] {
                                nodePreviousOutModel.LeftControlContact,
                                nodeOutModel.LeftControlContact
                            });
                    }
                    nodePreviousOutModel = nodeOutModel;
                }

                ng.WiringNode nodePreviousOutdoorChild = null;
                foreach (ng.WiringNode node in nodeOut.ChildNodes)
                {
                    if (node is ng.WiringNodeCH)
                    {
                        ng.WiringNodeCH nodeCH = node as ng.WiringNodeCH;
                        powerContactList.Add(nodeCH.PowerContact);

                        ng.WiringNode nodePreviousCHBoxChild = null;
                        int newBranchIndex = 0;
                        foreach (ng.WiringNode child1 in nodeCH.ChildNodes)
                        {
                            ng.WiringNodeIn wiringIn = child1 as ng.WiringNodeIn;
                            if (wiringIn == null) continue;
                            powerContactList.Add(child1.PowerContact);
                            indoorNodeList.Add(wiringIn);

                            if (nodePreviousCHBoxChild != null && !wiringIn.IsNewBranchOfParent)
                            {
                                joinHLinkList.Add(new Node[] {
                                        nodePreviousCHBoxChild.LeftControlContact,
                                        child1.LeftControlContact
                                    });
                            }
                            else
                            {
                                horizontalHLinkList.Add(new Node[] {
                                    nodeCH.RightControlContacts[newBranchIndex],
                                    child1.LeftControlContact
                                });
                                newBranchIndex++;
                            }

                            nodePreviousCHBoxChild = child1;
                        }
                    }
                    else if (node is ng.WiringNodeIn)
                    {
                        ng.WiringNodeIn nodeIn = node as ng.WiringNodeIn;
                        powerContactList.Add(nodeIn.PowerContact);
                        indoorNodeList.Add(nodeIn);
                    }
                    if (nodePreviousOutdoorChild != null)
                    {
                        joinHLinkList.Add(new Node[] {
                                nodePreviousOutdoorChild.LeftControlContact,
                                node.LeftControlContact
                            });
                    }
                    nodePreviousOutdoorChild = node;
                }
                horizontalHLinkList.Add(new Node[] {
                        nodeOut.Models[0].RightControlContact,
                        nodeOut.ChildNodes[0].LeftControlContact
                    });
                if (outdoorIndex > 0 && outdoorIndex >= nodeGroup.ControllerListLevel2.Count)
                {
                    systemHLinkList.Add(new Node[] {
                            nodeGroup.OutdoorList[outdoorIndex - 1].ChildNodes.Last().LeftControlContact,
                            nodeOut.Models[0].RightControlContact
                        });
                }

                outdoorIndex++;
            }

            ng.WiringNodeIn nodePreviousExchanger = null;
            foreach (ng.WiringNodeIn nodeExchanger in nodeGroup.TotalHeatExchangerList)
            {
                powerContactList.Add(nodeExchanger.PowerContact);
                indoorNodeList.Add(nodeExchanger);
                if (nodePreviousExchanger != null)
                {
                    joinHLinkList.Add(new Node[] {
                        nodePreviousExchanger.LeftControlContact,
                        nodeExchanger.LeftControlContact
                    });
                }
                nodePreviousExchanger = nodeExchanger;
            }
            if (nodeGroup.ControllerListLevel1.Count > 0 &&
                nodeGroup.ControllerListLevel2.Count > 0)
            {
                nodeStart = nodeGroup.ControllerListLevel1[0].LeftControlContact;
                nodeEnd = nodeGroup.ControllerListLevel2[0].LeftControlContact;

                //Point startDrawPt = utilPiping.getCenterPointF(nodeStart);
                //startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);

                //Point endDrawPt = utilPiping.getCenterPointF(nodeEnd);
                //endPoint = utilWiring.convertSystemPointToDrawingPoint(endDrawPt);

                Point pt11 = new Point();
                pt11 = utilPiping.getCenterPointF(nodeStart);
                Point pt12 = new Point();
                pt12 = utilPiping.getCenterPointF(nodeEnd);
                //pt12.X = endDrawPt.X;

                Node StartNode = new Node();
                StartNode.Location = pt11;
                Node EndNode = new Node();
                EndNode.Location = pt12;

                //PointF startWinPt = new PointF(startPoint.X -8 , startPoint.Y-78);
                //PointF midWinPt = new PointF(startPoint.X -8, endPoint.Y + 62);
                //PointF endWinPt = new PointF(endPoint.X-8, startPoint.Y - 78);
                
                //Point nodeWin2 = utilWiring.convertPointFToWinPoint(endWinPt);
                //Node node2 = new Node();
                //node2.Location = nodeWin2;

               // lnk1 = CreateEthernetWire(pt2, node2);
                //addFlowWiring.AddLink(lnk1);

                lvl1_to_lvl2_Lnk = CreateEthernetWire(StartNode, EndNode);
                addFlowWiring.AddLink(lvl1_to_lvl2_Lnk);

                //lnk1.Points.Add(utilWiring.convertPointFToWinPoint(startWinPt));
                //lnk1.Points.Add(utilWiring.convertPointFToWinPoint(midWinPt));
                //lnk1.Points.Add(pt11);
                
                lvl1_to_lvl2_Lnk.AddPoint(new Point(nodeStart.Location.X - 8, nodeStart.Location.Y - 12));//(point1)
                lvl1_to_lvl2_Lnk.AddPoint(new Point(nodeStart.Location.X - 8, nodeStart.Location.Y - 78));//(point2)
                lvl1_to_lvl2_Lnk.AddPoint(new Point(nodeStart.Location.X + 290, nodeStart.Location.Y - 78));//(point3)

            }

            foreach (Node[] nodes in joinEthernetList)
            {
                Point startDrawPt = utilPiping.getCenterPointF(nodes[0]);
                startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);

                Point endDrawPt = utilPiping.getCenterPointF(nodes[1]);
                endPoint = utilWiring.convertSystemPointToDrawingPoint(endDrawPt);

                lnk1 = CreateEthernetWire(nodes[0], nodes[1]);
                addFlowWiring.AddLink(lnk1);

                PointF startWinPt = new PointF(startPoint.X - 8, startPoint.Y + 8);
                PointF endWinPt = new PointF(startPoint.X - 8, endPoint.Y - 8);

                //lnk1.Points.Add(utilWiring.convertPointFToWinPoint(startWinPt));
                //lnk1.Points.Add(utilWiring.convertPointFToWinPoint(endWinPt));
                lnk1.AddPoint(utilWiring.convertPointFToWinPoint(startWinPt));
                lnk1.AddPoint(utilWiring.convertPointFToWinPoint(endWinPt));
            }

            int outdoorCount = nodeGroup.OutdoorList.Count;
            for (int i = 0; i < nodeGroup.ControllerListLevel2.Count; i++)
            {
                ng.WiringNodeCentralControl nodeCC = nodeGroup.ControllerListLevel2[i];
                if (outdoorCount > i)
                {
                    ng.WiringNodeOut nodeOut = nodeGroup.OutdoorList[i];
                    nodeStart = nodeCC.RightControlContact;
                    nodeEnd = nodeOut.Models[0].RightControlContact;

                    Point startDrawPt = utilPiping.getCenterPointF(nodeStart);
                    startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);

                    Point endDrawPt = utilPiping.getCenterPointF(nodeEnd);
                    endPoint = utilWiring.convertSystemPointToDrawingPoint(endDrawPt);

                    Point pt11 = new Point();
                    pt11 = utilPiping.getCenterPointF(nodeStart);
                    Point pt12 = new Point();
                    pt12.Y = startDrawPt.Y;
                    pt12.X = endDrawPt.X;

                    Node pt1 = new Node();
                    pt1.Location = pt11;
                    Node pt2 = new Node();
                    pt2.Location = pt12;

                    PointF startWinPt = new PointF(startPoint.X + 8, startPoint.Y);
                    PointF midWinPt1 = new PointF(startPoint.X + 8, (float)(nodeOut.Location.Y - 10));
                    PointF midWinPt2 = new PointF(endPoint.X + 8, (float)(nodeOut.Location.Y - 10));
                    PointF endWinPt = new PointF(endPoint.X + 8, endPoint.Y - 8);

                    Node node2 = new Node();
                    node2.Location = utilWiring.convertPointFToWinPoint(startWinPt);

                    lnk1 = CreateHLinkWire(pt1, node2);
                    addFlowWiring.AddLink(lnk1);

                    lnk1.Points.Add(utilWiring.convertPointFToWinPoint(midWinPt1));
                    lnk1.Points.Add(utilWiring.convertPointFToWinPoint(midWinPt2));
                    lnk1.Points.Add(utilWiring.convertPointFToWinPoint(endWinPt));
                    lnk1.Points.Add(pt12);
                }
                else if (nodeGroup.TotalHeatExchangerList.Count > i - outdoorCount)
                {
                    nodeStart = nodeCC.RightControlContact;
                    nodeEnd = nodeGroup.TotalHeatExchangerList[i - outdoorCount].LeftControlContact;
                    lnk1 = CreateHLinkWire(nodeStart, nodeEnd);
                    addFlowWiring.AddLink(lnk1);
                    Point startDrawPt = utilPiping.getCenterPointF(nodeStart);
                    startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);

                    Point endDrawPt = utilPiping.getCenterPointF(nodeEnd);
                    endPoint = utilWiring.convertSystemPointToDrawingPoint(endDrawPt);

                    PointF startWinPt = new PointF(startPoint.X + 8, startPoint.Y);
                    PointF endWinPt = new PointF(startPoint.X + 8, endPoint.Y);

                    lnk1.Points.Add(utilWiring.convertPointFToWinPoint(startWinPt));
                    lnk1.Points.Add(utilWiring.convertPointFToWinPoint(endWinPt));
                }
            }

            //for (int i = 1; i < nodeGroup.ControllerListLevel1.Count; i++)
            //{
            //    nodeStart = nodeGroup.ControllerListLevel1[i].RightControlContact;
            //    nodeEnd = nodeGroup.ControllerListLevel1[i - 1].RightControlContact;

            //    Point pt1 = utilPiping.getCenterPointF(nodeStart);
            //    startPoint = utilWiring.convertSystemPointToDrawingPoint(pt1);
            //    Point pt2 = utilPiping.getCenterPointF(nodeEnd);
            //    endPoint = utilWiring.convertSystemPointToDrawingPoint(pt2);
            //    //endPoint = new PointF(startPoint.X + 8, endPoint.Y);
            //    nodeEnd = utilPiping.createLinePoint(endPoint);
            //    addFlowWiring.AddNode(nodeEnd);

            //    lnk1 = CreateHLinkWire(nodeStart, nodeEnd);
            //    lnk1.LineStyle = LineStyle.Polyline;
            //    addFlowWiring.AddLink(lnk1);
            //    PointF ptF = new PointF(startPoint.X + 8, startPoint.Y);
            //    lnk1.Points.Add(utilWiring.convertPointFToWinPoint(ptF));
            //    lnk1.Points[0] = pt1;
            //    lnk1.Points[2] = pt2;
            //}
            for (int i = 0; i < powerContactList.Count; i++)
            {
                nodeStart = powerContactList[i];
                if (nodeStart == null) continue;

                Point startDrawPt = utilPiping.getCenterPointF(nodeStart);
                startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);
                endPoint = new PointF(startPoint.X + W_PowerWire, startPoint.Y);
                nodeEnd = utilWiring.createLinePoint(endPoint);
                nodeEnd.Foreground = System.Windows.Media.Brushes.Transparent;
                addFlowWiring.AddNode(nodeEnd);

                lnk1 = CreateLine(nodeStart, nodeEnd, System.Windows.Media.Brushes.Red);
                addFlowWiring.AddLink(lnk1);
                lnk1.Points[0] = utilWiring.convertPointFToWinPoint(startPoint);
                lnk1.Points[1] = utilWiring.convertPointFToWinPoint(endPoint);
                powerWireEndPoints.Add(endPoint);
            }
            foreach (Node[] nodes in joinHLinkList)
            {
                Point startDrawPt = utilPiping.getCenterPointF(nodes[0]);
                Point edDrawPt = utilPiping.getCenterPointF(nodes[1]);
                startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);
                endPoint = utilWiring.convertSystemPointToDrawingPoint(edDrawPt);

                Point pt11 = new Point();
                pt11 = utilPiping.getCenterPointF(nodes[0]);
                Point pt12 = new Point();
                pt12.Y = startDrawPt.Y + 15;
                pt12.X = edDrawPt.X - 10;

                Node pt1 = new Node();
                pt1.Location = pt11;
                Node pt2 = new Node();
                pt2.Location = pt12;

                lnk1 = CreateHLinkWire(pt1, pt2);
                addFlowWiring.AddLink(lnk1);

                PointF stDrawPt = new PointF((float)(edDrawPt.X - 10), startPoint.Y + 40);
                PointF enDrawPt = new PointF(endPoint.X, endPoint.Y - 2);
                Point startWinPt = utilWiring.convertPointFToWinPoint(stDrawPt);
                Point endWinPt = utilWiring.convertPointFToWinPoint(enDrawPt);
                lnk1.Points.Add(startWinPt);
                lnk1.Points.Add(endWinPt);
            }

            foreach (Node[] nodes in horizontalHLinkList)
            {
                Point startDrawPt = utilPiping.getCenterPointF(nodes[0]);
                Point edDrawPt = utilPiping.getCenterPointF(nodes[1]);
                startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);
                endPoint = utilWiring.convertSystemPointToDrawingPoint(edDrawPt);

                Point pt11 = new Point();
                pt11 = utilPiping.getCenterPointF(nodes[0]);
                Point pt12 = new Point();
                pt12.Y = startDrawPt.Y;
                pt12.X = edDrawPt.X - 50;

                Node pt1 = new Node();
                pt1.Location = pt11;
                Node pt2 = new Node();
                pt2.Location = pt12;

                lnk1 = CreateHLinkWire(pt1, pt2);
                addFlowWiring.AddLink(lnk1);

                PointF stDrawPt = new PointF(endPoint.X - 50, startPoint.Y);
                PointF enDrawPt = new PointF(endPoint.X, startPoint.Y);

                Point startWinPt = utilWiring.convertPointFToWinPoint(stDrawPt);
                Point endWinPt = utilWiring.convertPointFToWinPoint(enDrawPt);

                lnk1.Points.Add(startWinPt);
                lnk1.Points.Add(endWinPt);
            }
            foreach (Node[] nodes in systemHLinkList)
            {
                Point startDrawPt = utilPiping.getRightTopPointF(nodes[0]);
                Point edDrawPt = utilPiping.getCenterPointF(nodes[1]);
                startPoint = utilWiring.convertSystemPointToDrawingPoint(startDrawPt);
                endPoint = utilWiring.convertSystemPointToDrawingPoint(edDrawPt);

                Point pt11 = new Point();
                pt11 = startDrawPt;
                Point pt12 = new Point();
                pt12.Y = startDrawPt.Y + 50;
                pt12.X = edDrawPt.X + 150;

                Node pt1 = new Node();
                pt1.Location = pt11;
                Node pt2 = new Node();
                pt2.Location = pt12;

                lnk1 = CreateHLinkWire(pt1, pt2);
                addFlowWiring.AddLink(lnk1);

                PointF stDrawPt = new PointF(endPoint.X + 20, startPoint.Y + 70);
                PointF enDrawPt = new PointF(endPoint.X, endPoint.Y);

                Point startWinPt = utilWiring.convertPointFToWinPoint(stDrawPt);
                Point endWinPt = utilWiring.convertPointFToWinPoint(enDrawPt);

                lnk1.Points.Add(startWinPt);
                lnk1.Points.Add(endWinPt);
            }
            DrawRCLinks(indoorNodeList, powerWireEndPoints);
        }

        private Link CreateEthernetWire(Node nodeStart, Node nodeEnd)
        {
            return CreateLine(nodeStart, nodeEnd, utilPiping.colorWiring);
        }

        private Link CreateHLinkWire(Node nodeStart, Node nodeEnd)
        {
            return CreateLine(nodeStart, nodeEnd, System.Windows.Media.Brushes.Gray);
        }

        private void DrawRCLinks(List<ng.WiringNodeIn> indoorNodeList, List<PointF> roundPoints)
        {
            if (indoorNodeList == null || indoorNodeList.Count == 0) return;

            Node nodeStart, nodeEnd;
            PointF startPoint, endPoint;
            Link lnk1;

            List<Node[]> verticalWireNodes = new List<Node[]>();
            List<PointF[]> verticalWirePoints = new List<PointF[]>();

            foreach (ng.WiringNodeIn nodeInMain in indoorNodeList)
            {
                if (nodeInMain == null || nodeInMain.RoomIndoorItem == null || nodeInMain.RoomIndoorItem.IndoorItemGroup == null) continue;
                if (!nodeInMain.RoomIndoorItem.IsMainIndoor) continue;
                List<ng.WiringNodeIn> indoorNodeGroup = new List<ng.WiringNodeIn>();
                foreach (ng.WiringNodeIn nodeIn in indoorNodeList)
                {
                    if (nodeIn == null) continue;
                    if (nodeIn == nodeInMain || nodeInMain.RoomIndoorItem.IndoorItemGroup.Contains(nodeIn.RoomIndoorItem))
                    {
                        indoorNodeGroup.Add(nodeIn);
                    }
                }

                float[] yRange = new float[2];
                for (int i = 0; i < indoorNodeGroup.Count; i++)
                {
                    ng.WiringNodeIn nodeIn = indoorNodeGroup[i];

                    Point indConPt = utilPiping.getCenterPointF(nodeIn.RightControlContact);
                    PointF indoorContactPoint = utilWiring.convertSystemPointToDrawingPoint(indConPt);
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

                float endPointX = GetEndPointX(roundPoints, verticalWirePoints, yRange);
                Node verticalWireTop = null;
                Node verticalWireBottom = null;
                for (int i = 0; i < indoorNodeGroup.Count; i++)
                {
                    ng.WiringNodeIn nodeIn = indoorNodeGroup[i];
                    nodeStart = nodeIn.RightControlContact;
                    Point nodeStartF = utilPiping.getCenterPointF(nodeStart);
                    startPoint = utilWiring.convertSystemPointToDrawingPoint(nodeStartF);
                    endPoint = new PointF(endPointX, startPoint.Y);
                    if ((i > 0 && i < indoorNodeGroup.Count - 1) || nodeIn.ChildNodes.Count > 0)
                    {
                        nodeEnd = CreateContactNode(new PointF((float)(endPoint.X - nodeStart.Location.X), (float)(nodeStart.Size.Height) / 2), nodeStart);
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
                        nodeEnd = utilWiring.createLinePoint(endPoint);
                        addFlowWiring.AddNode(nodeEnd);
                    }
                    if (nodeIn.ChildNodes.Count == 0)
                    {
                        lnk1 = CreateHLinkWire(nodeStart, nodeEnd);
                        addFlowWiring.AddLink(lnk1);
                    }
                    if (i == 0)
                    {
                        verticalWireTop = nodeEnd;
                    }
                    verticalWireBottom = nodeEnd;
                }
                lnk1 = CreateHLinkWire(verticalWireTop, verticalWireBottom);
                addFlowWiring.AddLink(lnk1);
                startPoint = new PointF(endPointX, yRange[0]);
                endPoint = new PointF(endPointX, yRange[1]);
                verticalWireNodes.Add(new Node[] { verticalWireTop, verticalWireBottom });
                verticalWirePoints.Add(new PointF[] { startPoint, endPoint });
            }
            foreach (ng.WiringNodeIn nodeIn in indoorNodeList)
            {
                if (nodeIn == null) continue;
                if (nodeIn.ChildNodes.Count == 0) continue;

                nodeStart = nodeIn.RightControlContact;
                Point spt = utilPiping.getCenterPointF(nodeStart);
                startPoint = utilWiring.convertSystemPointToDrawingPoint(spt);

                for (int j = 0; j < nodeIn.ChildNodes.Count; j++)
                {
                    ng.WiringNode nodeRC = nodeIn.ChildNodes[j];
                    nodeEnd = nodeRC.LeftControlContact;
                    Point ept = utilPiping.getCenterPointF(nodeEnd);
                    endPoint = utilWiring.convertSystemPointToDrawingPoint(ept);

                    lnk1 = CreateHLinkWire(nodeStart, nodeEnd);
                    addFlowWiring.AddLink(lnk1);

                    if (j > 0)
                    {
                        float[] yRange = new float[2] { startPoint.Y, endPoint.Y };
                        float endPointX = GetEndPointX(roundPoints, verticalWirePoints, yRange);
                        PointF ept0 = new PointF(endPointX, yRange[0]);
                        PointF ept1 = new PointF(endPointX, yRange[1]);
                        lnk1.Points.Add(utilWiring.convertPointFToWinPoint(ept0));
                        lnk1.Points.Add(utilWiring.convertPointFToWinPoint(ept1));
                        verticalWirePoints.Add(new PointF[] {
                            new PointF(endPointX, yRange[0]),
                            new PointF(endPointX, yRange[1])
                        });
                    }
                }
            }

            foreach (ng.WiringNodeIn nodeIn in indoorNodeList)
            {
                if (nodeIn == null) continue;
                if (nodeIn.ChildNodes.Count == 0) continue;

                foreach (ng.WiringNode nodeRC in nodeIn.ChildNodes)
                {
                    Node leftContact = nodeRC.LeftControlContact;
                    Point rcWinPt = utilPiping.getCenterPointF(leftContact);
                    PointF ptRC = utilWiring.convertSystemPointToDrawingPoint(rcWinPt);
                    float xRC = ptRC.X;
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
                        float offsetX = xRC - ptRC.X;
                        ptRC.X = xRC;
                        PointF pt = new PointF((float)(nodeRC.Location.X + offsetX), (float)(nodeRC.Location.Y));
                        nodeRC.Location = utilWiring.convertPointFToWinPoint(pt);
                        foreach (Node childOfRC in nodeRC.ChildNodes)
                        {
                            PointF rcPt = new PointF((float)(childOfRC.Location.X + offsetX), (float)(childOfRC.Location.Y));
                            childOfRC.Location = utilWiring.convertPointFToWinPoint(rcPt);
                        }
                    }
                }
            }
        }

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
            endPointX += 10;

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
                        isWireOverwrite = true;
                        endPointX = startPoint.X + 20;
                    }
                }
            }
            return endPointX;
        }

        private Node CreateTitleTextNode(string text, PointF pt)
        {
            Font textFontTitle = new Font("Arial", 18f, GraphicsUnit.Pixel);
            Node titleTextNode = CreateTextNode(text, pt, textFontTitle, null);
            titleTextNode.FontSize = 14f;
            return titleTextNode;
        }

        private void DrawLegend(ng.WiringNodeGroup nodeGroup, float y)
        {
            string title = "Legend";
            string text0 = "ODU, IDU, CH-Box, Remote Controller, Central Controller Units";
            string text1 = "Transmission Line";
            string text2 = "Electrical power line";
            string text3 = "Contact";
            string text4 = "Line Type(2 Lines, 3 Lines, 4 Lines)";

            float x = 50;
            y += 100;

            CreateTitleTextNode(title, new PointF(x, y));
            CreateTextBoxNode("", new PointF(x, y + 30), 90, true, nodeGroup);
            CreateTextNode(text0, new PointF(x + 100, y + 30 + 8), nodeGroup);
            CreateTextNode(text1, new PointF(x + 100, y + 80 - 7), nodeGroup);
            CreateTextNode(text2, new PointF(x + 100, y + 100 - 7), nodeGroup);
            CreateTextNode(text3, new PointF(x + 100, y + 120 - 7), nodeGroup);
            CreateTextNode(text4, new PointF(x + 100, y + 140 - 7), nodeGroup);

            Node nodeStart, nodeEnd;
            Node boxNode = new Node();
            Link lnk1;

            PointF stRectPtF = new PointF(x, y + 30 + 8);
            Point pt = utilWiring.convertPointFToWinPoint(stRectPtF);
            boxNode.Location = pt;
            SetWiringNodeBorder(boxNode, W_CHBox);
            addFlowWiring.AddNode(boxNode);

            PointF stPtF = new PointF(x, y + 80);
            PointF endPtF = new PointF(x + 90, y + 80);
            nodeStart = utilWiring.createLinePoint(stPtF);
            nodeEnd = utilWiring.createLinePoint(endPtF);
            addFlowWiring.AddNode(nodeEnd);
            lnk1 = CreateHLinkWire(nodeStart, nodeEnd);
            addFlowWiring.AddLink(lnk1);

            PointF stF = new PointF(x, y + 100);
            PointF endF = new PointF(x + 90, y + 100);
            nodeStart = utilWiring.createLinePoint(stF);
            nodeEnd = utilWiring.createLinePoint(endF);
            addFlowWiring.AddNode(nodeEnd);
            lnk1 = CreateLine(nodeStart, nodeEnd, System.Windows.Media.Brushes.Red);
            addFlowWiring.AddLink(lnk1);

            CreateContactNode(new PointF(x + 30, y + 120), nodeGroup);
            CreateContactNode(new PointF(x + 60, y + 120), nodeGroup);
            CreatePowerLineMark("//", new PointF(x + 10 - 6, y + 140 - 9));
            CreatePowerLineMark("///", new PointF(x + 40 - 8, y + 140 - 9));
            CreatePowerLineMark("////", new PointF(x + 70 - 10, y + 140 - 9));
        }

        private void DrawCHBox(ng.WiringNodeCH nodeCH, float y)
        {
            PointF pt;
            Font textFont1 = new Font("Arial", 11f, GraphicsUnit.Pixel);

            NodeElement_Wiring item_wiring = utilWiring.GetNodeElement_Wiring_CH(nodeCH.Model, nodeCH.PowerSupply, nodeCH.PowerLineType, nodeCH.PowerCurrent);
            PointF pt1 = new PointF(X_CHBox, y);
            nodeCH.Location = utilWiring.convertPointFToWinPoint(pt1);
            nodeCH.Text = item_wiring.ShortModel;
            SetWiringNodeBorder(nodeCH, W_CHBox);
            addFlowWiring.AddNode(nodeCH);

            string rightNumber = item_wiring.StrGroup1[0];
            string leftNumber = item_wiring.Str1;

            if (nodeCH.IsMultiCHBox)
            {
                int newBranchIndex = 0;
                for (int i = 0; i < nodeCH.ChildNodes.Count; i++)
                {
                    ng.WiringNodeIn wiringIn = nodeCH.ChildNodes[i] as ng.WiringNodeIn;
                    if (wiringIn != null && wiringIn.IsNewBranchOfParent)
                    {
                        rightNumber = (newBranchIndex * 2 + 3) + " " + (newBranchIndex * 2 + 4);
                        pt = new PointF(W_CHBox - 35, -12 + i * Y_OffsetStep);
                        CreateLittleTextNode(rightNumber, (float)nodeCH.Size.Width, pt, textFont1, nodeCH);
                        nodeCH.RightControlContacts.Add(CreateContactNode(new PointF(W_CHBox, Y_ControlContact + i * Y_OffsetStep), nodeCH));
                        newBranchIndex++;
                    }
                }
            }
            else
            {
                pt = new PointF(W_CHBox - 35, -12);
                CreateLittleTextNode(rightNumber, (float)nodeCH.Size.Width, pt, textFont1, nodeCH);
                nodeCH.RightControlContacts.Add(CreateContactNode(new PointF(W_CHBox, Y_ControlContact), nodeCH));
            }

            const int W_CHBox1 = 50;

            pt = new PointF(W_CHBox1 - 120, 5);
            CreateLittleTextNode(leftNumber, (float)nodeCH.Size.Width, pt, textFont1, nodeCH);

            nodeCH.LeftControlContact = CreateContactNode(new PointF(0, Y_ControlContact), nodeCH);
            pt = new PointF(W_CHBox + 45, 5);
            CreateLittleTextNode(item_wiring.StrGroup2[0], (float)nodeCH.Size.Width, pt, textFont1, nodeCH);

            pt = new PointF(W_CHBox + 45, 16);
            string powerText = item_wiring.StrGroup3[0];
            powerText = powerText.Replace(" ", "");
            CreateLittleTextNode(powerText, (float)nodeCH.Size.Width, pt, textFont1, nodeCH);

            pt = new PointF(W_CHBox + 10, 8);
            CreatePowerLineMark(item_wiring.StrGroup4[0], (float)nodeCH.Size.Width, pt, textFont1, nodeCH);

            pt = new PointF(W_CHBox, 15);
            nodeCH.PowerContact = CreateContactNode(pt, nodeCH);

            DrawGroundNode(nodeCH);
        }

        public void GetCentralControllerPowerInfo(string model,
            out string powerLineTypeMark, out string powerPortName, out string voltage, out bool hasGroundWire)
        {
            powerLineTypeMark = "//";
            voltage = "";
            powerPortName = "";
            hasGroundWire = false;

            JCHVRF.DAL.CentralControllerDAL dal = new JCHVRF.DAL.CentralControllerDAL();
            System.Data.DataTable dt = dal.GetPowerSupply(mod.Project.CurrentProject.SubRegionCode, mod.Project.CurrentProject.BrandCode, model);
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
                    powerLineTypeMark = "//";
                }
                else if (powerLineType == "3")
                {
                    powerLineTypeMark = "///";
                }
                else if (powerLineType == "4")
                {
                    powerLineTypeMark = "////";
                }
            }
        }        
    }
}
