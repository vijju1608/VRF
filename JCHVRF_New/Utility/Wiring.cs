using JCHVRF.MyPipingBLL;
using JCHVRF_New.Utility;
using JCHVRF_New.Views;
using Lassalle.WPF.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using drawing = System.Drawing;
using ng = JCHVRF.Model.NextGen;
using OldModel = JCHVRF.Model;
using JCHVRF_New.ViewModels;
using JCHVRF.Model;
using Prism.Events;
using JCHVRF_New.Common.Contracts;
namespace JCHVRF_New.Utility
{
    public class Wiring
    {
        private IEventAggregator _eventAggregator;
        private IModalWindowService _winService;
        private AddFlow addFlowWiring;
        List<drawing.PointF[]> ptArrayList = new List<drawing.PointF[]>();
        List<drawing.PointF[]> ptArrayList_power = new List<drawing.PointF[]>();
        List<drawing.PointF[]> ptArrayList_ground = new List<drawing.PointF[]>();
        List<drawing.PointF[]> ptArrayList_mainpower = new List<drawing.PointF[]>();
        List<string> strArrayList_powerType = new List<string>();
        List<string> strArrayList_powerVoltage = new List<string>();
        List<double> dArrayList_powerCurrent = new List<double>();
        public UtilityWiring utilWiring = new UtilityWiring();
        JCHVRF.Model.Project thisProject = new JCHVRF.Model.Project();
        ng.SystemVRF sysItem = new ng.SystemVRF();
        drawing.PointF convertedPointF = new drawing.PointF();
        public Wiring(JCHVRF.Model.Project project, ng.SystemVRF systemVRF, AddFlow AddFlowWiring)
        {
            thisProject = project;
            sysItem = systemVRF;
            addFlowWiring = AddFlowWiring;
            DoDrawingWiring(thisProject, sysItem);
            utilWiring = new UtilityWiring(thisProject);
        }
        public AddFlow GetAddFlowInstance()
        {
            return addFlowWiring;
        }

        private void DoDrawingWiring(JCHVRF.Model.Project thisProject, ng.SystemVRF sysItem)
        {
            //if (tnOut == null) return;
            //if (this.tabControl1.SelectedTab.Name != "tpgWiring" && this.tabControl1.SelectedTab.Name != "tpgReport")
            //    return;
            //SystemVRF sysItem = tnOut.Tag as SystemVRF;
            //if (sysItem == null || sysItem.OutdoorItem == null)
            //    return;
            //curSystemItem = sysItem;

            string imgDir = @"/Image/TypeImages/";

            InitWiringNodes(addFlowWiring);

            //if (addFlowPiping.Nodes.Count == 0)       DOUBT
            //    BindPipingDrawing(tnOut);

            //ControllerWiringBLL bll = new ControllerWiringBLL(thisProject, addFlowControllerWiring);
            WiringBLL bll = new WiringBLL(thisProject, addFlowWiring);
            bll.CreateWiringNodeStructure(sysItem);

            ptArrayList.Clear();
            ptArrayList_power.Clear();
            ptArrayList_ground.Clear();
            ptArrayList_mainpower.Clear();
            strArrayList_powerType.Clear();
            strArrayList_powerVoltage.Clear();
            dArrayList_powerCurrent.Clear();

            if (sysItem == null || sysItem.MyWiringNodeOut == null)
                return;

            ng.WiringNodeOut nodeOut = sysItem.MyWiringNodeOut;
            if (nodeOut.ChildNodes.Count == 0 || sysItem.OutdoorItem == null)
            {
                DrawDiagError();
                return;
            }

            if (bll.IsHeatRecovery(sysItem))
            {
                DoDrawingHRWiring(sysItem, nodeOut, imgDir);
            }
            else
            {
                DoDrawingWiring(sysItem, nodeOut, imgDir);
            }
        }
        private void InitWiringNodes(AddFlow addFlowWiring)
        {
            //addFlowWiring.Controls.Clear();
            addFlowWiring.Clear();

            //addFlowWiring.Nodes.Clear();

            //addFlowWiring.AutoScrollPosition = new Point(0, 0);
            addFlowWiring.ScrollIncrement = new Size(0, 0);
        }
        private void DrawWiringLegend(ng.WiringNodeOut nodeOut)
        {
            string text0 = "";
            string text1 = "Transmission Line";
            string text2 = "Electrical power line";
            string text3 = "Ground wire";

            Point ptf1 = new Point(0, nodeOut.Size.Height + 90f);
            convertedPointF = utilWiring.convertSystemPointToDrawingPoint(ptf1);
            utilWiring.createTextNode_wiring(text0, convertedPointF, nodeOut);

            ptf1 = new Point(105, ptf1.Y + 15f);
            convertedPointF = utilWiring.convertSystemPointToDrawingPoint(ptf1);
            utilWiring.createTextNode_wiring(text1, convertedPointF, nodeOut);

            ptf1 = new Point(105, ptf1.Y + 30f);
            convertedPointF = utilWiring.convertSystemPointToDrawingPoint(ptf1);
            utilWiring.createTextNode_wiring(text2, convertedPointF, nodeOut);

            ptf1 = new Point(105, ptf1.Y + 30f);
            convertedPointF = utilWiring.convertSystemPointToDrawingPoint(ptf1);
            utilWiring.createTextNode_wiring(text3, convertedPointF, nodeOut);

            Point ptf2 = new Point(nodeOut.Location.X, nodeOut.Location.Y + nodeOut.Size.Height + 110f);
            Point ptf3 = new Point(ptf2.X + 100, ptf2.Y);

            drawing.PointF convertedPtf2 = new drawing.PointF();
            drawing.PointF convertedPtf3 = new drawing.PointF();
            convertedPtf2 = utilWiring.convertSystemPointToDrawingPoint(ptf2);
            convertedPtf3 = utilWiring.convertSystemPointToDrawingPoint(ptf3);
            ptArrayList.Add(new drawing.PointF[] { convertedPtf2, convertedPtf3 });

            ptf2 = new Point(ptf2.X, ptf2.Y + 30f);
            ptf3 = new Point(ptf2.X + 100, ptf2.Y);
            convertedPtf2 = utilWiring.convertSystemPointToDrawingPoint(ptf2);
            convertedPtf3 = utilWiring.convertSystemPointToDrawingPoint(ptf3);
            ptArrayList_power.Add(new drawing.PointF[] { convertedPtf2, convertedPtf3 });

            ptf2 = new Point(ptf2.X, ptf2.Y + 30f);
            ptf3 = new Point(ptf2.X + 100, ptf2.Y);
            convertedPtf2 = utilWiring.convertSystemPointToDrawingPoint(ptf2);
            convertedPtf3 = utilWiring.convertSystemPointToDrawingPoint(ptf3);
            ptArrayList_ground.Add(new drawing.PointF[] { convertedPtf2, convertedPtf3 });
        }

        private void DoDrawingHRWiring(ng.SystemVRF sysItem, ng.WiringNodeOut nodeOut, string imgDir)
        {
            Node textNode;
            drawing.PointF ptText = new drawing.PointF();

            NodeElement_Wiring item_wiring = utilWiring.GetNodeElement_Wiring_ODU(sysItem.OutdoorItem, thisProject.BrandCode);
            nodeOut.Location = new Point(10f, JCHVRF.MyPipingBLL.NextGen.UtilPiping.HeightForNodeText + (JCHVRF.MyPipingBLL.NextGen.UtilPiping.OutdoorOffset_Y_wiring) * 2 + 36f); // ?????? Location

            string imgFile = System.IO.Path.Combine(imgDir, item_wiring.KeyName + ".png");
            utilWiring.setNode_wiring(nodeOut, imgFile, addFlowWiring);

            drawing.PointF ptf1 = new drawing.PointF((float)(nodeOut.Location.X) + 140, (float)(nodeOut.Location.Y + 20));
            drawing.PointF ptf2 = new drawing.PointF(ptf1.X + 74, ptf1.Y + 4);
            drawing.PointF ptf3 = new drawing.PointF(ptf2.X - 10, ptf2.Y);
            ptArrayList_ground.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });

            string text = "";
            ptText = item_wiring.PtNodeNames[0];

            text = sysItem.Name;
            utilWiring.createTextNode_wiring(text, ptText, nodeOut);
            if (item_wiring.UnitCount > 1)
            {
                text = sysItem.OutdoorItem.AuxModelName;
                utilWiring.createTextNode_wiring(text, item_wiring.PtNodeNames[1], nodeOut);
            }

            //----------------  
            utilWiring.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeOut);

            drawing.PointF locationPointF = new drawing.PointF();
            for (int i = 0; i < item_wiring.UnitCount; ++i)
            {

                locationPointF = utilWiring.convertSystemPointToDrawingPoint(nodeOut.Location);

                ptf1 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[i], locationPointF);
                ptf2 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[i], locationPointF);
                ptArrayList_power.Add(new drawing.PointF[] { ptf1, ptf2 });

                Point pt = new Point(item_wiring.PtModelGroup[i].X, item_wiring.PtModelGroup[i].Y);
                utilWiring.createTextNode_wiring(item_wiring.ModelGroup[i], item_wiring.PtModelGroup[i], nodeOut);

                if (i < 4)
                {
                    Point pt1 = new Point(item_wiring.PtStrGroup1[i].X, item_wiring.PtStrGroup1[i].Y);
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup1[i], item_wiring.PtStrGroup1[i], nodeOut);
                }

                utilWiring.createTextNode_wiring(item_wiring.StrGroup2[i], item_wiring.PtStrGroup2[i], nodeOut);
                utilWiring.createTextNode_wiring(item_wiring.StrGroup3[i], item_wiring.PtStrGroup3[i], nodeOut);
                utilWiring.createTextNode_wiring(item_wiring.StrGroup4[i], item_wiring.PtStrGroup4[i], nodeOut, true);
            }


            drawing.PointF ptf4 = new drawing.PointF(0, 0);
            drawing.PointF ptf5 = new drawing.PointF(0, 0);
            ptText = item_wiring.PtNodeNames[0];
            ptText.Y += UtilPiping.HeightForNodeText / 2;

            List<ng.WiringNodeIn> wiringNodeInList = new List<ng.WiringNodeIn>();
            DrawWiringNodes(nodeOut, nodeOut.ChildNodes, wiringNodeInList, imgDir, true);

            DrawWiringRemoteControllers(wiringNodeInList, imgDir);


            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                ptArrayList_power.Add(ptArrayList_mainpower[i]);

                ptf4 = ptArrayList_mainpower[i][1];
                text = strArrayList_powerVoltage[i] + "/" + dArrayList_powerCurrent[i].ToString() + "A";
                textNode = utilWiring.createTextNode_wiring(text, new drawing.PointF(ptf4.X + 122, ptf4.Y + 2));
                addFlowWiring.AddNode(textNode); //.Nodes.Add(textNode);
                text = strArrayList_powerType[i];
                textNode = utilWiring.createTextNode_wiring(text, new drawing.PointF(ptf4.X + 166, ptf4.Y - 12));
                addFlowWiring.AddNode(textNode);  //.Nodes.Add(textNode);
            }

            DrawWiringLegend(nodeOut);

            foreach (drawing.PointF[] pt in ptArrayList)
            {
                if (pt[0].Y < 0)
                {
                    pt[0].Y = 0;
                }

                Node nd1 = utilWiring.createLinePoint(pt[0]);
                addFlowWiring.AddNode(nd1);  //.Nodes.Add(nd1);
                Node nd2 = utilWiring.createLinePoint(pt[pt.Length - 1]);
                addFlowWiring.AddNode(nd2); //.Nodes.Add(nd2);
                //Link lnk1 = utilWiring.createLine();

                Link lnk1 = utilWiring.createLine(nd1, nd2, "", addFlowWiring);
                addFlowWiring.AddLink(lnk1);
                //lnk1.Jump = Jump.Arc; 
                //nd1.OutLinks.Add(lnk1, nd2);
                if (pt.Length > 2)
                    lnk1.AddPoint(new Point(pt[1].X, pt[1].Y));
                // lnk1.Points.Add(new Point(pt[1].X, pt[1].Y));
                if (pt.Length > 3)
                    lnk1.AddPoint(new Point(pt[2].X, pt[2].Y));
                //lnk1.Points.Add(new Point(pt[2].X, pt[2].Y));

            }

            foreach (drawing.PointF[] pt in ptArrayList_power)
            {
                Node nd1 = utilWiring.createLinePoint(pt[0]);
                addFlowWiring.AddNode(nd1);  //.Nodes.Add(nd1);
                Node nd2 = utilWiring.createLinePoint(pt[pt.Length - 1]);
                //Link lnk1 = utilWiring.createLine();
                Link lnk1 = utilWiring.createLine(nd1, nd2, "", addFlowWiring);
                if (pt.Length > 2)
                    lnk1.LineStyle = LineStyle.VH;
                //lnk1.Line.Style = LineStyle.VH;
                // lnk1.Jump = Jump.Arc;
                //lnk1.DrawWidth = 1;
                // lnk1.DrawColor = Color.Red;
                lnk1.Stroke = System.Windows.Media.Brushes.Red;
                // nd1.OutLinks.Add(lnk1, nd2);
                addFlowWiring.AddLink(lnk1);
            }

            foreach (drawing.PointF[] pt in ptArrayList_ground)
            {
                if (pt.Length > 2)
                {
                    ng.MyNodeGround_Wiring nodeground = new ng.MyNodeGround_Wiring();
                    nodeground.Location = new Point(pt[2].X, pt[2].Y);
                    imgFile = System.IO.Path.Combine(imgDir, "Ground.png");
                    utilWiring.setNode_wiring(nodeground, imgFile, addFlowWiring);
                    // nodeground.DrawColor = Color.Transparent;
                }
                Node nd1 = utilWiring.createLinePoint(pt[0]);
                addFlowWiring.AddNode(nd1); // .Nodes.Add(nd1);
                Node nd2 = utilWiring.createLinePoint(pt[1]);
                //Link lnk1 = utilWiring.createLine();
                Link lnk1 = utilWiring.createLine(nd1, nd2, "", addFlowWiring);
                // lnk1.DashStyle = DashStyle.Dash;
                lnk1.DashStyle = DashStyles.Dash;
                // nd1.OutLinks.Add(lnk1, nd2);
                addFlowWiring.AddLink(lnk1);
            }
        }

        private void DrawWiringNodes(Node parent, List<ng.WiringNode> nodes, List<ng.WiringNodeIn> myNodeInList, string imgDir, bool isHR)
        {
            bool prevBrotherNodeIsIndoor = false;
            double sumCurrent = 0.0d;
            ng.WiringNodeIn lastIn = null;
            drawing.PointF ptf1, ptf2, ptf3, ptf4, ptf5, ptf6;
            NodeElement_Wiring item_wiring;
            string imgFile;
            int lastYIndex = 0;
            int index1 = 0;
            drawing.PointF nodeCHPointF = new drawing.PointF();
            drawing.PointF nodeInPointF = new drawing.PointF();
            foreach (ng.WiringNode node in nodes)
            {
                if (node is ng.WiringNodeCH)
                {
                    prevBrotherNodeIsIndoor = false;

                    ng.WiringNodeCH nodeCH = node as ng.WiringNodeCH;
                    item_wiring = utilWiring.GetNodeElement_Wiring_CH(nodeCH.Model, nodeCH.PowerSupply, nodeCH.PowerLineType, nodeCH.PowerCurrent);

                    nodeCH.Location = utilWiring.getLocationChild_wiring(parent, node, index1, isHR);
                    nodeCH.Text = item_wiring.ShortModel;

                    utilWiring.setNode_wiring(nodeCH, new Size(80, 52), addFlowWiring);

                    utilWiring.createTextNode_wiring("CH Unit", new drawing.PointF(86, -13), nodeCH);

                    DrawWiringNodes(nodeCH, nodeCH.ChildNodes, myNodeInList, imgDir, isHR);

                    double x = parent.Location.X + parent.Size.Width - 1;
                    double y = nodeCH.Location.Y + nodeCH.Size.Height - 2;
                    float enlargedHeight = 0;
                    if (nodeCH.IsMultiCHBox && nodeCH.ChildNodes.Count > 1)
                    {
                        enlargedHeight = UtilPiping.VDistanceVertical_wiring * (nodeCH.ChildNodes.Count - 1);
                        nodeCH.Size = new Size(nodeCH.Size.Width, nodeCH.Size.Height + enlargedHeight);
                    }

                    if (nodeCH.IsMultiCHBox)
                    {
                        int newBranchIndex = 0;
                        for (int i = 0; i < nodeCH.ChildNodes.Count; i++)
                        {
                            ng.WiringNodeIn wiringIn = nodeCH.ChildNodes[i] as ng.WiringNodeIn;
                            if (wiringIn != null && wiringIn.IsNewBranchOfParent)
                            {
                                utilWiring.createTextNode_wiring((newBranchIndex * 2 + 3) + " " + (newBranchIndex * 2 + 4)
                                    , new drawing.PointF(142, UtilPiping.VDistanceVertical_wiring * i), nodeCH);
                                newBranchIndex++;
                            }
                        }
                    }
                    else
                    {
                        utilWiring.createTextNode_wiring(item_wiring.StrGroup1[0], item_wiring.PtStr1, nodeCH);
                    }

                    utilWiring.createTextNode_wiring(item_wiring.Str1, new drawing.PointF(35, (float)(nodeCH.Size.Height - enlargedHeight - 14)), nodeCH);

                    if (index1 == 0)
                    {
                        ptf1 = new drawing.PointF((float)x, (float)y);
                        ptf2 = new drawing.PointF((float)(nodeCH.Location.X + 1), (float)y);
                        ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2 });
                    }
                    else
                    {
                        x = nodeCH.Location.X;
                        ptf1 = new drawing.PointF((float)x, (float)y);
                        ptf2 = new drawing.PointF((float)(x - 60), (float)y);
                        ptf3 = new drawing.PointF((float)(x - 60), (float)(y - 15));
                        ptf4 = new drawing.PointF((float)x, (float)(y - UtilityWiring.VDistanceVertical_wiring * (index1 - lastYIndex)));
                        ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2, ptf3, ptf4 });
                    }

                    ptf1 = new drawing.PointF((float)(nodeCH.Location.X + 140 - 60), (float)(nodeCH.Location.Y + 20));
                    ptf2 = new drawing.PointF(ptf1.X + 74, ptf1.Y + 4);
                    ptf3 = new drawing.PointF(ptf2.X - 10, ptf2.Y);
                    ptArrayList_ground.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });

                    utilWiring.createTextNode_wiring(item_wiring.StrGroup2[0], item_wiring.PtStrGroup2[0], nodeCH);

                    utilWiring.createTextNode_wiring(item_wiring.StrGroup3[0], item_wiring.PtStrGroup3[0], nodeCH);

                    utilWiring.createTextNode_wiring(item_wiring.StrGroup4[0], item_wiring.PtStrGroup4[0], nodeCH, true);

                    nodeCHPointF = utilWiring.convertSystemPointToDrawingPoint(nodeCH.Location);
                    ptf4 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[0], nodeCHPointF);
                    ptf5 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[0], nodeCHPointF);
                    ptf4.X -= 60;
                    ptf5.X -= 60;
                    ptArrayList_power.Add(new drawing.PointF[] { ptf4, ptf5 });

                    lastYIndex = index1;
                    if (nodeCH.ChildNodes.Count > 1)
                    {
                        index1 += nodeCH.ChildNodes.Count - 1;
                    }
                    index1++;
                }
                else if (node is ng.WiringNodeIn)
                {
                    ng.WiringNodeIn nodeIn = node as ng.WiringNodeIn;
                    lastIn = nodeIn;
                    int powerIndex = 0;
                    bool isNewPower = false;
                    item_wiring = utilWiring.GetNodeElement_Wiring_IDU(nodeIn.RoomIndoorItem.IndoorItem, thisProject.BrandCode, sysItem.OutdoorItem.Type, ref strArrayList_powerType, ref strArrayList_powerVoltage, ref dArrayList_powerCurrent, ref powerIndex, ref isNewPower);
                    double current = nodeIn.RoomIndoorItem.IndoorItem.RatedCurrent;
                    sumCurrent += current;
                    nodeIn.Location = utilWiring.getLocationChild_wiring(parent, node, index1, isHR);
                    nodeIn.Text = item_wiring.ShortModel;

                    imgFile = System.IO.Path.Combine(imgDir, item_wiring.KeyName + ".png");
                    utilWiring.setNode_wiring(nodeIn, imgFile, addFlowWiring);


                    string indoorName = nodeIn.RoomIndoorItem.IndoorName;

                    // DOUBT NEED TO CHECK LATER after FER as Room is not functional in FER

                    //if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.RoomID))
                    //{
                    //    RoomLoadIndexBLL roomBill = new RoomLoadIndexBLL();
                    //    string floorRoomName = roomBill.getFloorRoomName(nodeIn.RoomIndoorItem, thisProject);                        
                    //    indoorName = floorRoomName + ":" + indoorName;
                    //}
                    //else
                    //{
                    //    if (!string.IsNullOrEmpty(nodeIn.RoomIndoorItem.DisplayRoom))
                    //    {
                    //        indoorName = nodeIn.RoomIndoorItem.DisplayRoom + ":" + indoorName;
                    //    }
                    //}

                    utilWiring.createTextNode_wiring(indoorName, new drawing.PointF(66, -13), nodeIn);
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup3[0], item_wiring.PtStrGroup3[0], nodeIn);

                    utilWiring.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeIn);
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup1[0], item_wiring.PtStrGroup1[0], nodeIn);
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup2[0], item_wiring.PtStrGroup2[0], nodeIn);
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup4[0], item_wiring.PtStrGroup4[0], nodeIn, true);


                    double x1 = parent.Location.X + parent.Size.Width - 1;
                    double y1 = nodeIn.Location.Y + nodeIn.Size.Height - 2;
                    float x = (float)x1;
                    float y = (float)y1;
                    if (isHR && parent is ng.WiringNodeOut && !prevBrotherNodeIsIndoor)
                    {
                        x = (float)nodeIn.Location.X;
                        ptf1 = new drawing.PointF(x, y);
                        ptf2 = new drawing.PointF(x - UtilPiping.HDistanceVertical_wiring - 60, y);
                        ptf3 = new drawing.PointF(x - UtilPiping.HDistanceVertical_wiring - 60, y - 15);
                        ptf4 = new drawing.PointF(x - UtilPiping.HDistanceVertical_wiring, y - UtilPiping.VDistanceVertical_wiring * (index1 - lastYIndex));
                        ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2, ptf3, ptf4 });
                    }
                    else
                    {
                        if (index1 == 0 || nodeIn.IsNewBranchOfParent)
                        {
                            ptf1 = new drawing.PointF(x, y);
                            ptf2 = new drawing.PointF((float)(nodeIn.Location.X + 1), y);
                            ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2 });
                        }
                        else
                        {
                            x = (float)nodeIn.Location.X;
                            ptf1 = new drawing.PointF(x, y);
                            ptf2 = new drawing.PointF(x, y - 15);
                            ptf3 = new drawing.PointF(x + 60, y - UtilPiping.VDistanceVertical_wiring);
                            ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });
                        }
                    }

                    ptf1 = new drawing.PointF((float)(nodeIn.Location.X + 140), (float)(nodeIn.Location.Y + 20));
                    ptf2 = new drawing.PointF(ptf1.X + 74, ptf1.Y + 4);
                    ptf3 = new drawing.PointF(ptf2.X - 10, ptf2.Y);
                    ptArrayList_ground.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });

                    myNodeInList.Add(nodeIn);
                    nodeInPointF = utilWiring.convertSystemPointToDrawingPoint(nodeIn.Location);
                    ptf4 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[0], nodeInPointF);
                    ptf5 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[0], nodeInPointF);
                    ptf5.X += (10f * powerIndex) + 55f;
                    ptArrayList_power.Add(new drawing.PointF[] { ptf4, ptf5 });

                    if (isNewPower)
                    {
                        ptf4 = new drawing.PointF(ptf5.X, (float)(nodeIn.Location.Y + (nodeIn.Size.Height / 2)));
                        ptf6 = new drawing.PointF(ptf5.X + 240, ptf4.Y);
                        ptArrayList_mainpower.Add(new drawing.PointF[] { ptf5, ptf4, ptf6 });
                    }
                    else
                    {
                        ptArrayList_mainpower[powerIndex][0] = ptf5;
                    }

                    prevBrotherNodeIsIndoor = true;
                    lastYIndex = index1;
                    index1++;
                }
            }
        }

        private ng.MyNodeRemoteControler_Wiring DrawWiringRCNode(ng.WiringNodeIn nodeIn, JCHVRF.Model.Accessory RC, int RCIndex, string imgDir)
        {
            ng.MyNodeRemoteControler_Wiring nodeRC = null;
            double x = nodeIn.Location.X + nodeIn.Size.Width + ((RCIndex + 1) * 82f) + 60f;
            double y = nodeIn.Location.Y - 32f;

            string model = "";
            if (RC.BrandCode == "Y")
                model = RC.Model_York;
            else
                model = RC.Model_Hitachi;
            if (model != "")
            {
                nodeRC = new ng.MyNodeRemoteControler_Wiring();
                nodeRC.Location = new Point(x, y);
                string imgFile = System.IO.Path.Combine(imgDir, "RemoteControler.png");
                utilWiring.setNode_wiring(nodeRC, imgFile, addFlowWiring);

                //nodeRC.DrawColor = Color.Transparent;
                nodeRC.Stroke = Brushes.Transparent;

                drawing.PointF ptf = new drawing.PointF((float)nodeRC.Size.Width, 5);
                utilWiring.createTextNode_wiring(model, ptf, nodeRC);

                if (RC.IsShared)
                {
                    utilWiring.createTextNode_wiring("Shared", new drawing.PointF(ptf.X, ptf.Y + 15), nodeRC);
                }
            }
            return nodeRC;
        }

        private void DoDrawingWiring(ng.SystemVRF sysItem, ng.WiringNodeOut nodeOut, string imgDir)
        {
            Node textNode;
            drawing.Point ptText = new drawing.Point();
            drawing.PointF nodeOutPointF = new drawing.PointF();

            NodeElement_Wiring item_wiring = utilWiring.GetNodeElement_Wiring_ODU(sysItem.OutdoorItem, thisProject.BrandCode);
            nodeOut.Location = new Point(10f, UtilPiping.HeightForNodeText + UtilPiping.OutdoorOffset_Y_wiring + 36f);

            string imgFile = System.IO.Path.Combine(imgDir, item_wiring.KeyName + ".png");
            utilWiring.setNode_wiring(nodeOut, imgFile, addFlowWiring);

            drawing.PointF ptf1 = new drawing.PointF((float)(nodeOut.Location.X + 140), (float)(nodeOut.Location.Y + 20));
            drawing.PointF ptf2 = new drawing.PointF(ptf1.X + 74, ptf1.Y + 4);
            drawing.PointF ptf3 = new drawing.PointF(ptf2.X - 10, ptf2.Y);
            ptArrayList_ground.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });

            string text = "";
            ptText = item_wiring.PtNodeNames[0];

            text = sysItem.Name;
            utilWiring.createTextNode_wiring(text, ptText, nodeOut);
            if (item_wiring.UnitCount > 1)
            {
                text = sysItem.OutdoorItem.AuxModelName;
                utilWiring.createTextNode_wiring(text, item_wiring.PtNodeNames[1], nodeOut);
            }

            utilWiring.createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeOut);

            nodeOutPointF = utilWiring.convertSystemPointToDrawingPoint(nodeOut.Location);
            for (int i = 0; i < item_wiring.UnitCount; ++i)
            {
                ptf1 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[i], nodeOutPointF);
                ptf2 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[i], nodeOutPointF);
                ptArrayList_power.Add(new drawing.PointF[] { ptf1, ptf2 });

                if (i < item_wiring.ModelGroup.Length)
                    utilWiring.createTextNode_wiring(item_wiring.ModelGroup[i], item_wiring.PtModelGroup[i], nodeOut);
                if (item_wiring.ModelGroup.Length > 1 && i < item_wiring.StrGroup1.Length)
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup1[i], item_wiring.PtStrGroup1[i], nodeOut);
                if (i < item_wiring.StrGroup2.Length)
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup2[i], item_wiring.PtStrGroup2[i], nodeOut);
                if (i < item_wiring.StrGroup3.Length)
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup3[i], item_wiring.PtStrGroup3[i], nodeOut);
                if (i < item_wiring.StrGroup4.Length)
                    utilWiring.createTextNode_wiring(item_wiring.StrGroup4[i], item_wiring.PtStrGroup4[i], nodeOut, true);
            }

            drawing.PointF ptf4 = new drawing.PointF(0, 0);
            drawing.PointF ptf5 = new drawing.PointF(0, 0);
            ptText = item_wiring.PtNodeNames[0];
            ptText.Y += Convert.ToInt32(UtilPiping.HeightForNodeText / 2);


            List<ng.WiringNodeIn> sortNodeInList = getSortNodeInList(nodeOut);


            if (sortNodeInList == null)
                return;

            List<ng.WiringNodeIn> wiringNodeInList = new List<ng.WiringNodeIn>();
            DrawWiringNodes(nodeOut, sortNodeInList.ToList<ng.WiringNode>(), wiringNodeInList, imgDir, false);

            DrawWiringRemoteControllers(wiringNodeInList, imgDir);


            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                ptArrayList_power.Add(ptArrayList_mainpower[i]);

                ptf4 = ptArrayList_mainpower[i][1];
                text = strArrayList_powerVoltage[i] + "/" + dArrayList_powerCurrent[i].ToString() + "A";
                textNode = utilWiring.createTextNode_wiring(text, new drawing.PointF(ptf4.X + 122, ptf4.Y + 2));
                addFlowWiring.AddNode(textNode);
                text = strArrayList_powerType[i];
                textNode = utilWiring.createTextNode_wiring(text, new drawing.PointF(ptf4.X + 166, ptf4.Y - 12));
                addFlowWiring.AddNode(textNode);
            }

            DrawWiringLegend(nodeOut);

            foreach (drawing.PointF[] pt in ptArrayList)
            {
                Node nd1 = utilWiring.createLinePoint(pt[0]);
                addFlowWiring.AddNode(nd1);
                Node nd2 = utilWiring.createLinePoint(pt[pt.Length - 1]);
                addFlowWiring.AddNode(nd2);
                //Link lnk1 = utilWiring.createLine();
                Link lnk1 = utilWiring.createLine(nd1, nd2, "", addFlowWiring);
                //nd1.OutLinks.Add(lnk1, nd2);                
                addFlowWiring.AddLink(lnk1);

                if (pt.Length > 2)
                {
                    var pt1 = utilWiring.convertPointFToWinPoint(pt[1]);
                    lnk1.AddPoint(pt1);
                }
            }

            foreach (drawing.PointF[] pt in ptArrayList_power)
            {
                Node nd1 = utilWiring.createLinePoint(pt[0]);
                //addFlowWiring.Nodes.Add(nd1);
                addFlowWiring.AddNode(nd1);
                Node nd2 = utilWiring.createLinePoint(pt[pt.Length - 1]);
                Link lnk1 = utilWiring.createLine(nd1, nd2, "", addFlowWiring);

                if (pt.Length > 2)
                    lnk1.LineStyle = LineStyle.VH;
                //lnk1.Jump = Jump.Arc;
                //lnk1.DrawWidth = 1;
                lnk1.StrokeThickness = 1;

                //lnk1.DrawColor = Color.Red;
                lnk1.Stroke = System.Windows.Media.Brushes.Red;
                //nd1.OutLinks.Add(lnk1, nd2);
                addFlowWiring.AddLink(lnk1);
            }

            foreach (drawing.PointF[] pt in ptArrayList_ground)
            {
                if (pt.Length > 2)
                {
                    ng.MyNodeGround_Wiring nodeground = new ng.MyNodeGround_Wiring();
                    nodeground.Location = new Point(pt[2].X, pt[2].Y);
                    imgFile = System.IO.Path.Combine(imgDir, "Ground.png");
                    utilWiring.setNode_wiring(nodeground, imgFile, addFlowWiring);
                    //nodeground.DrawColor = Color.Transparent;

                }
                Node nd1 = utilWiring.createLinePoint(pt[0]);
                //addFlowWiring.Nodes.Add(nd1);
                addFlowWiring.AddNode(nd1);
                Node nd2 = utilWiring.createLinePoint(pt[1]);
                Link lnk1 = utilWiring.createLine(nd1, nd2, "", addFlowWiring);
                lnk1.DashStyle = DashStyles.Dash;
                //nd1.OutLinks.Add(lnk1, nd2);
                addFlowWiring.AddLink(lnk1);
            }
        }
        private void DrawWiringRemoteControllers(List<ng.WiringNodeIn> nodeInList, string imgDir)
        {
            List<bool[]> rcPositionTable = new List<bool[]>();
            int rowIndex = 0;
            foreach (ng.WiringNodeIn nodeIn in nodeInList)
            {
                if (nodeIn.RoomIndoorItem == null || nodeIn.RoomIndoorItem.IndoorItem == null) continue;

                ng.MyNodeRemoteControler_Wiring shardRCNode = null;
                List<JCHVRF.Model.Accessory> accList = new List<JCHVRF.Model.Accessory>();
                try
                {
                    if (nodeIn.RoomIndoorItem != null && nodeIn.RoomIndoorItem.ListAccessory != null)
                        foreach (var itemAcc in nodeIn.RoomIndoorItem.ListAccessory)
                        {
                            if (itemAcc != null && itemAcc.Count > 0)
                                for (int i = 0; i < itemAcc.Count; i++)
                                    accList.Add(itemAcc);
                        }
                }
                catch { accList = nodeIn.RoomIndoorItem.ListAccessory; }
                //accList = nodeIn.RoomIndoorItem.ListAccessory;
                List<JCHVRF.Model.RoomIndoor> indoorGroup = nodeIn.RoomIndoorItem.IndoorItemGroup;
                if (accList != null && accList.Count > 0)
                {
                    int RCIndex = 0;
                    if (nodeIn.RoomIndoorItem.IsMainIndoor)
                    {
                        if (!accList.Exists(acc => acc.IsShared))
                        {
                            accList[0].IsShared = true;
                        }
                    }

                    foreach (JCHVRF.Model.Accessory acc in accList)
                    {
                        if (acc.Type.ToLower() == "remote controler"
                            || acc.Type.ToLower() == "remote control switch"
                            || acc.Type.ToLower() == "half-size remote control switch"
                            || acc.Type.ToLower() == "receiver kit for wireless control"
                            || acc.Type.Contains("有線遙控器")
                            || acc.Type == "受光器"
                            )
                        {
                            int colIndex = RCIndex;
                            bool isCovered = true;

                            int firstShare = rowIndex;
                            int lastShare = rowIndex;
                            if (acc.IsShared && indoorGroup != null)
                            {
                                for (int index1 = 0; index1 < nodeInList.Count; index1++)
                                {
                                    ng.WiringNodeIn n = nodeInList[index1];
                                    if (indoorGroup.Contains(n.RoomIndoorItem))
                                    {
                                        firstShare = Math.Min(firstShare, index1);
                                        lastShare = Math.Max(lastShare, index1);
                                    }
                                }
                            }
                            while (isCovered)
                            {
                                isCovered = false;
                                if (rcPositionTable.Count - 1 < colIndex)
                                {
                                    rcPositionTable.Add(new bool[nodeInList.Count]);
                                }
                                for (int index1 = firstShare; index1 <= lastShare; index1++)
                                {
                                    if (rcPositionTable[colIndex][index1])
                                    {
                                        isCovered = true;
                                        break;
                                    }
                                }
                                if (isCovered)
                                {
                                    colIndex++;
                                }
                            }
                            for (int index1 = firstShare; index1 <= lastShare; index1++)
                            {
                                rcPositionTable[colIndex][index1] = true;
                            }

                            ng.MyNodeRemoteControler_Wiring nodeRC = DrawWiringRCNode(nodeIn, acc, colIndex, imgDir);
                            if (nodeIn.RoomIndoorItem.IsMainIndoor)
                            {
                                if (acc.IsShared)
                                {
                                    shardRCNode = nodeRC;
                                }
                            }

                            if (nodeRC != null)
                            {
                                drawing.PointF ptf1 = new drawing.PointF((float)(nodeRC.Location.X + (nodeRC.Size.Width / 2)), (float)(nodeRC.Location.Y + nodeRC.Size.Height));
                                drawing.PointF ptf2 = new drawing.PointF((float)(nodeRC.Location.X + (nodeRC.Size.Width / 2)), (float)(nodeIn.Location.Y + 12));
                                drawing.PointF ptf3 = new drawing.PointF((float)(nodeIn.Location.X + nodeIn.Size.Width - 1), (float)(nodeIn.Location.Y + 12));
                                ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });

                                RCIndex++;
                            }
                        }
                    }
                }
                if (shardRCNode != null && indoorGroup != null && indoorGroup.Count > 0)
                {
                    foreach (OldModel.RoomIndoor ri in indoorGroup)
                    {
                        //共享其它Remote Controller的室内机
                        if (ri == nodeIn.RoomIndoorItem) continue;
                        ng.WiringNodeIn nodeInShareTo = nodeInList.Find(n => n.RoomIndoorItem == ri);
                        if (nodeInShareTo == null) continue;

                        //共享连接线
                        drawing.PointF ptf1 = new drawing.PointF((float)(shardRCNode.Location.X + (shardRCNode.Size.Width / 2)), (float)(shardRCNode.Location.Y + shardRCNode.Size.Height));
                        if (nodeInShareTo.Location.Y + 12 < shardRCNode.Location.Y)
                        {
                            //室内机在共享控制器的上面
                            ptf1.Y = (float)(shardRCNode.Location.Y);
                        }
                        drawing.PointF ptf2 = new drawing.PointF((float)(shardRCNode.Location.X + (shardRCNode.Size.Width / 2)), (float)(nodeInShareTo.Location.Y + 12));
                        drawing.PointF ptf3 = new drawing.PointF((float)(nodeInShareTo.Location.X + nodeInShareTo.Size.Width - 1), (float)(nodeInShareTo.Location.Y + 12));
                        ptArrayList.Add(new drawing.PointF[] { ptf1, ptf2, ptf3 });
                    }
                }
                rowIndex++;
            }
        }
        private List<ng.WiringNodeIn> getSortNodeInList(Object objList)
        {
            List<ng.WiringNodeIn> myNodeList = new List<ng.WiringNodeIn>();
            List<ng.WiringNodeIn> sortNodeInList = new List<ng.WiringNodeIn>();
            List<ng.WiringNodeIn> mainNodeInList = new List<ng.WiringNodeIn>();
            if (objList is ng.WiringNodeOut)
            {
                ng.WiringNodeOut nodeOut = objList as ng.WiringNodeOut;
                foreach (Node node in nodeOut.ChildNodes)
                {
                    if (node is ng.WiringNodeIn)
                    {
                        ng.WiringNodeIn nodeIn = node as ng.WiringNodeIn;
                        myNodeList.Add(nodeIn);
                    }
                }
            }
            else if (objList is List<ng.WiringNodeIn>)
            {
                myNodeList = objList as List<ng.WiringNodeIn>;
            }
            else
            {
                return sortNodeInList;
            }

            foreach (ng.WiringNodeIn nodeIn in myNodeList)
            {
                if (nodeIn.RoomIndoorItem.IsMainIndoor)
                    mainNodeInList.Add(nodeIn);
                //nodeCout++;
            }
            if (mainNodeInList.Count == 0)
            {
                for (int i = 0; i < myNodeList.Count; i++)
                {
                    sortNodeInList.Add(myNodeList[i]);
                }
            }
            else
            {
                foreach (ng.WiringNodeIn mianNodeIn in mainNodeInList)
                {
                    sortNodeInList.Add(mianNodeIn);

                    if (mianNodeIn.RoomIndoorItem.IndoorItemGroup != null)
                    {
                        foreach (ng.WiringNodeIn nodeIn in myNodeList)
                        {
                            foreach (JCHVRF.Model.RoomIndoor rind in mianNodeIn.RoomIndoorItem.IndoorItemGroup)
                            {
                                if (nodeIn.RoomIndoorItem == rind && !nodeIn.RoomIndoorItem.IsMainIndoor)
                                {
                                    sortNodeInList.Add(nodeIn);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (sortNodeInList.Count < myNodeList.Count)
                {
                    foreach (ng.WiringNodeIn nodeIn in myNodeList)
                    {
                        if (!sortNodeInList.Contains(nodeIn))
                            sortNodeInList.Add(nodeIn);
                    }
                }

            }
            return sortNodeInList;
        }

        private void DrawDiagError()
        {
            string wiringDiagramError = "ERROR! Cannot draw the Wiring diagrams because there is no indoor unit";
            //  ErrorLog.LogError(ErrorLog.errorType.error, ErrorLog.errorMsg.PipeLength); // Appropriate error need to be updated using piping error class.
        }
    }
}
