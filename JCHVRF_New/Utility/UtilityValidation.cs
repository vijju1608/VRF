using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lassalle.WPF.Flow;
using old = JCHVRF.Model;
using ng = JCHVRF.Model.NextGen;
using System.Drawing;
using JCBase.Utility;
using System.Data;
using JCHVRF.DAL;
using System.Windows.Media;
using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF.MyPipingBLL.NextGen;

namespace JCHVRF_New.Utility
{

    public class UtilityValidation
    {
        private AddFlow addFlowPiping;
        PipingDAL _dal;
        private static old.Project thisProject;
        private bool isInch = false;
        private bool isHitachi = true;//TODO
        private string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER; // string.Empty; //TODO
        private string ut_pipeSize = SystemSetting.UserSetting.unitsSetting.settingDimension; //TODO will come from settings only 
        private string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        public Font textFont_piping = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Pixel);
        private Bitmap bmpMeasureString = null;
        private Graphics gMeasureString = null;
        UtilPiping utilPiping = new UtilPiping();


        public UtilityValidation(old.Project project, ref AddFlow addFlow)
        {
            thisProject = project;
            addFlowPiping = addFlow;
            bmpMeasureString = new Bitmap(500, 100);
            gMeasureString = Graphics.FromImage(this.bmpMeasureString);
            if (SystemSetting.UserSetting.unitsSetting.settingDimension == Unit.ut_Dimension_inch)
            {
                isInch = true;
            }
        }
        public void DrawTextToAllNodes(Node node, Node parent, ng.SystemVRF sysItem)
        {
            // RND
            if (sysItem.editrpt == false)
                sysItem.IsExportToReport = true;

            if (node is ng.MyNodeOut)
            {
                ng.MyNodeOut nodeOut = node as ng.MyNodeOut;
                DrawTextToAllNodes(nodeOut.ChildNode, nodeOut, sysItem);
                nodeOut.Stroke = System.Windows.Media.Brushes.RoyalBlue;
            }
            else if (node is ng.MyNodeYP)
            {
                ng.MyNodeYP nodeYP = node as ng.MyNodeYP;
                drawTextToOtherNode(node, parent, sysItem);

                foreach (Node item in nodeYP.ChildNodes)
                {
                    DrawTextToAllNodes(item, nodeYP, sysItem);
                }
            }
            else if (node is ng.MyNodeCH)
            {
                ng.MyNodeCH nodeCH = node as ng.MyNodeCH;
                drawTextToOtherNode(node, parent, sysItem);

                DrawTextToAllNodes(nodeCH.ChildNode, nodeCH, sysItem);
            }
            else if (node is ng.MyNodeMultiCH)
            {
                ng.MyNodeMultiCH nodeMCH = node as ng.MyNodeMultiCH;
                drawTextToOtherNode(node, parent, sysItem);

                foreach (Node item in nodeMCH.ChildNodes)
                {
                    DrawTextToAllNodes(item, nodeMCH, sysItem);
                }
            }
            else if (node is ng.MyNodeIn)
            {
                ng.MyNodeIn nodeIn = node as ng.MyNodeIn;
                drawTextToIDUNode(sysItem, nodeIn);
            }

            if (node is ng.MyNode)
            {
                ng.MyNode myNode = node as ng.MyNode;
                if (myNode != null && myNode.MyInLinks!=null && myNode.MyInLinks.Count>0)
                {
                    for (int i = 0; i < myNode.MyInLinks.Count; i++)
                    {
                        ng.MyLink myLink = myNode.MyInLinks[i] as ng.MyLink;                        
                        drawTextToLink(myLink, i, parent, node, isInch, sysItem);
                    }
                }
            }
        }

        private void drawTextToOtherNode(Node node, Node parent, ng.SystemVRF sysItem)
        {
            if (node == null) return;

            //DOUBT
            //if (node.Children != null)
            //{
            //    node.Children.Clear();
            //}

            if (!sysItem.IsPipingOK) return;

            if (node is ng.MyNodeYP)
            {
                ng.MyNodeYP nodeYP = node as ng.MyNodeYP;
                if (!string.IsNullOrEmpty(nodeYP.Model) && sysItem.IsPipingOK)
                {
                    JCHNode label1 = new JCHNode();
                    initTextNode(label1, " ");

                   Caption Cap1= initCaptionText(nodeYP.Model, nodeYP);

                    label1.Location = convertPointFToWinPoint(nodeYP.Location.X + 20, nodeYP.Location.Y - label1.Size.Height);
                    if (sysItem.PipingLayoutType == old.PipingLayoutTypes.BinaryTree)
                    {
                        if (!sysItem.IsPipingVertical)
                        {
                            label1.Location = convertPointFToWinPoint(nodeYP.Location.X + 10, nodeYP.Location.Y);
                        }
                        else
                        {
                            label1.Location = convertPointFToWinPoint(nodeYP.Location.X + 15, nodeYP.Location.Y - label1.Size.Height);
                        }
                    }
                    else if (sysItem.PipingLayoutType == old.PipingLayoutTypes.SchemaA)
                    {
                        if (!sysItem.IsPipingVertical)
                        {
                            label1.Location = convertPointFToWinPoint(label1.Location.X + 3, label1.Location.Y);
                        }
                        else
                        {
                            label1.Location = convertPointFToWinPoint(label1.Location.X - 8, label1.Location.Y + 20);
                        }
                    }
                    else if (sysItem.PipingLayoutType == old.PipingLayoutTypes.Normal)
                    {
                        if (sysItem.IsPipingVertical)
                        {
                            label1.Location = convertPointFToWinPoint(nodeYP.Location.X + 20, nodeYP.Location.Y - label1.Size.Height - 8);
                        }
                        else
                        {
                            label1.Location = convertPointFToWinPoint(nodeYP.Location.X + 20, nodeYP.Location.Y - label1.Size.Height);
                        }
                    }

                    //addFlowPiping.AddNode(label1);

                    //nodeYP.AddFlow.Items.Add(label1);

                    Cap1.Location = label1.Location;
                    addFlowPiping.AddCaption(Cap1);

                    if (nodeYP.Model != "")
                    {
                        if (sysItem.IsExportToReport)
                            InsertPipingKitTable("BranchKit", sysItem.Name, nodeYP.Model, 1, sysItem.Id);
                    }
                    addFlowPiping.RemoveNode(label1);
                }
            }
            else if (node is ng.MyNodeCH)
            {
                ng.MyNodeCH nodeCH = node as ng.MyNodeCH;
                if (!string.IsNullOrEmpty(nodeCH.Model))
                {
                    if (sysItem.IsExportToReport)
                        InsertPipingKitTable("CHBox", sysItem.Name, nodeCH.Model, 1, sysItem.Id);
                    if (sysItem.IsPipingOK)
                    {
                        JCHNode label1 = new JCHNode();
                        initTextNode(label1, nodeCH.Model);
                        if (!sysItem.IsPipingVertical)
                            label1.Location = convertPointFToWinPoint(nodeCH.Location.X + nodeCH.Size.Width / 2 + 5, nodeCH.Location.Y - label1.Size.Height);
                        else
                            label1.Location = convertPointFToWinPoint((nodeCH.Location.X - 50) + nodeCH.Size.Width / 2 - label1.Size.Width / 2, (nodeCH.Location.Y + 12) - label1.Size.Height);

                        addFlowPiping.AddNode(label1);

                        nodeCH.AddFlow.Items.Add(label1);
                    }
                }
            }
            else if (node is ng.MyNodeMultiCH)
            {
                ng.MyNodeMultiCH nodeMCH = node as ng.MyNodeMultiCH;
                if (!string.IsNullOrEmpty(nodeMCH.Model))
                {
                    if (sysItem.IsExportToReport)
                        InsertPipingKitTable("CHBox", sysItem.Name, nodeMCH.Model, 1, sysItem.Id);
                    if (sysItem.IsPipingOK)
                    {
                        JCHNode label1 = new JCHNode();
                        initTextNode(label1, nodeMCH.Model);
                        if (sysItem.IsPipingVertical)
                        {
                            label1.Location = convertPointFToWinPoint(nodeMCH.Location.X + nodeMCH.Size.Width / 2 - label1.Size.Width / 2, nodeMCH.Location.Y - label1.Size.Height);
                        }
                        else
                        {
                            if (parent is ng.MyNodeYP && parent.Location.Y > node.Location.Y)
                            {
                                label1.Location = convertPointFToWinPoint(nodeMCH.Location.X + nodeMCH.Size.Width / 2 - label1.Size.Width / 2, nodeMCH.Location.Y - label1.Size.Height);
                            }
                            else
                            {
                                label1.Location = convertPointFToWinPoint(nodeMCH.Location.X + nodeMCH.Size.Width / 2 + 5, nodeMCH.Location.Y - label1.Size.Height);
                            }
                        }

                        addFlowPiping.AddNode(label1);

                        nodeMCH.AddFlow.Items.Add(label1);
                    }
                }
            }
        }

        public void drawTextToIDUNode(ng.SystemVRF sysItem, ng.MyNodeIn nodeUnit)
        {
            //DOUBT
            //if (nodeUnit.Children != null)
            //{
            //    nodeUnit.Children.Clear();
            //}

            string text = "";
            old.RoomIndoor riItem = nodeUnit.RoomIndooItem;
            if (riItem == null) return;

            string model = riItem.IndoorItem.Model_York;
            if (isHitachi)
                model = riItem.IndoorItem.Model_Hitachi;
            if (sysItem.OutdoorItem.Type != null && (sysItem.OutdoorItem.Type.Contains("YVAHP") || sysItem.OutdoorItem.Type.Contains("YVAHR")))
                model = riItem.IndoorItem.Model_York;

            text = riItem.IndoorName + "\n" + model;
            if (!string.IsNullOrEmpty(riItem.RoomID))
            {
                string floorName = "";
                foreach (old.Floor f in thisProject.FloorList)
                {
                    foreach (old.Room rm in f.RoomList)
                    {
                        if (rm.Id == riItem.RoomID)
                        {
                            floorName = f.Name;
                            break;
                        }
                    }
                }

                text = floorName + "\n" + riItem.RoomName + ":" + text;
            }
            else
            {
                if (!string.IsNullOrEmpty(riItem.DisplayRoom))
                {
                    text = riItem.DisplayRoom + ":" + text;
                }
            }

            JCHNode label = new JCHNode();
            initTextNode(label, text);

            label.ImagePosition = ImagePosition.CenterBottom;

            if (label.Size.Width < nodeUnit.Size.Width)
                label.Size = convertSize(nodeUnit.Size.Width, label.Size.Height);

            //addFlowPiping.Items.Add(label);
            nodeUnit.Stroke = System.Windows.Media.Brushes.RoyalBlue;
            //nodeUnit.AddFlow.AddNode(label);
            //nodeUnit.AddFlow.Items.Add(label);

            //DOUBT
            //nodeUnit.HighlightChildren = true;

            JCHNode label2 = new JCHNode();
            string text2 = "";
            text2 = "Cooling: " + Unit.ConvertToControl(riItem.ActualCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            text2 += "\nHeating: " + Unit.ConvertToControl(riItem.ActualHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            text2 += "\nSensible Cooling: " + Unit.ConvertToControl(riItem.ActualSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power;
            initTextNode(label2, text2);

            label2.ImagePosition = ImagePosition.LeftMiddle;

            if (label2.Size.Width < nodeUnit.Size.Width)
            {
                label2.Size = convertSize(nodeUnit.Size.Width, label2.Size.Height);
            }
            //nodeUnit.AddFlow.AddNode(label2);
            //nodeUnit.AddFlow.Items.Add(label2);

            //addFlowPiping.AddNode(label2);         // This line show Cooling, Heating and Heating on Canvas
            //nodeUnit.AddFlow.Items.Add(label2);

            label.Location = convertPointFToWinPoint(nodeUnit.Location.X, nodeUnit.Location.Y - label.Size.Height);
            label2.Location = convertPointFToWinPoint(nodeUnit.Location.X, nodeUnit.Location.Y + nodeUnit.Size.Height);

            if (sysItem.PipingLayoutType == old.PipingLayoutTypes.Normal)
            {
                if (!sysItem.IsPipingVertical)
                {
                    label2.Location = convertPointFToWinPoint(label2.Location.X, label2.Location.Y + 8);
                }
            }
            else if (sysItem.PipingLayoutType == old.PipingLayoutTypes.BinaryTree)
            {
                if (!sysItem.IsPipingVertical)
                {
                    label.Location = convertPointFToWinPoint(nodeUnit.Location.X, nodeUnit.Location.Y + nodeUnit.Size.Height);
                    label2.Location = convertPointFToWinPoint(label2.Location.X, label2.Location.Y + label.Size.Height);
                }
            }
        }

        private void drawTextToLink(ng.MyLink myLink, int linkIndex, Node parent, Node node, bool isInch, ng.SystemVRF sysItem)
        {
            if (myLink == null || parent == null || node == null) return;            

            string p1 = "";   //Low Pressure Gas
            string p2 = "";   //High Pressure Gas
            string p3 = "";   //Liquid
            string p5 = "";

            if (sysItem.IsPipingOK)
            {
                string SpecL = myLink.SpecL;
                string SpecG_h = myLink.SpecG_h;
                string SpecG_l = myLink.SpecG_l;
                if (!string.IsNullOrEmpty(SpecL) && !string.IsNullOrEmpty(SpecG_h))
                {
                    if (SpecG_l != null && SpecG_l != "-")
                        p1 = "φ" + SpecG_l + ut_pipeSize;
                    if (SpecG_h != null && SpecG_h != "-")
                        p2 = "φ" + SpecG_h + ut_pipeSize;
                    p3 = "φ" + SpecL + ut_pipeSize;

                    if (isInch)
                    {
                        _dal = new PipingDAL(thisProject);
                        SpecL = GetPipeSize_Inch(myLink.SpecL);
                        SpecG_h = GetPipeSize_Inch(myLink.SpecG_h);
                        SpecG_l = GetPipeSize_Inch(myLink.SpecG_l);

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

            if (sysItem.IsInputLengthManually)
            {
                p5 = Unit.ConvertToControl(myLink.Length, UnitType.LENGTH_M, ut_length).ToString("0.##") + ut_length;
            }

            bool isCoolingOnly;
            if (node is ng.MyNodeIn)
                isCoolingOnly = (node as ng.MyNodeIn).IsCoolingonly;
            else if (node is ng.MyNodeYP)
                isCoolingOnly = (node as ng.MyNodeYP).IsCoolingonly;
            else
                isCoolingOnly = false;

            float width = 0;
            float height = 5;
            JCHNode label1 = new JCHNode();
            Caption Child1 = new Caption();
            if (!string.IsNullOrEmpty(p1))
            {
                if (string.IsNullOrEmpty(p2) && !isCoolingOnly)
                {
                    DrawPipeLableColor(label1, " ", node, 4);
                    Child1=DrawPipeCaptionColor(Child1, p1, node, 4);
                }
                else
                {
                    DrawPipeLableColor(label1, " ", node, 1);
                    Child1=DrawPipeCaptionColor(Child1, p1, node, 1);
                }
                height += 8;
            }

            JCHNode label2 = new JCHNode();
            Caption Child2 = new Caption();
            if (!string.IsNullOrEmpty(p2))
            {
                if (parent is ng.MyNodeMultiCH || parent is ng.MyNodeCH)
                {
                    DrawPipeLableColor(label2, " ", node, 4);
                    Child2=DrawPipeCaptionColor(Child2, p2, node, 4);
                }
                else if (string.IsNullOrEmpty(p1))
                {
                    DrawPipeLableColor(label2, " ", node, 4);
                    Child2=DrawPipeCaptionColor(Child2, p2, node, 4);
                }
                else
                {
                    if (parent is ng.MyNodeYP)
                    {
                        ng.MyNodeYP pNodeYp = parent as ng.MyNodeYP;
                        if (pNodeYp.ParentNode is ng.MyNodeMultiCH || pNodeYp.ParentNode is ng.MyNodeCH)
                        {
                            DrawPipeLableColor(label2, " ", node, 4);
                            Child2=DrawPipeCaptionColor(Child2, p2, node, 4);
                        }
                        else
                        {
                            DrawPipeLableColor(label2, " ", node, 2);
                            Child2=DrawPipeCaptionColor(Child2, p2, node, 2);
                        }
                    }
                    else
                    {
                        DrawPipeLableColor(label2, " ", node, 2);
                        Child2=DrawPipeCaptionColor(Child2, p2, node, 2);
                    }
                }
                height += 8;
            }

            JCHNode label3 = new JCHNode();
            Caption Child3 = new Caption();
            if (!string.IsNullOrEmpty(p3))
            {
                DrawPipeLableColor(label3, " ", node, 3);
                Child3= DrawPipeCaptionColor(Child3, p3, node, 3);
                height += 8;
            }
            JCHNode label5 = new JCHNode();
            Caption Child5 = new Caption();
            if (!string.IsNullOrEmpty(p5))
            {
                DrawPipeLableColor(label5, " ", node, 5);
                Child5= DrawPipeCaptionColor(Child5, p5, node, 5);
                height += 8;
            }
            width = Math.Max(Math.Max((float)label1.Size.Width, (float)label2.Size.Width), Math.Max((float)label3.Size.Width, (float)label5.Size.Width));
            label1.Size = convertSize(width, height);

            if (sysItem.PipingLayoutType == old.PipingLayoutTypes.BinaryTree)
            {
                SetLinkTextLocationForBinaryTree(myLink, linkIndex, parent, node, label1, sysItem.IsPipingVertical);//TODO
            }
            else if (sysItem.PipingLayoutType == old.PipingLayoutTypes.SchemaA)
            {
                SetLinkTextLocationForSechmaA(myLink, linkIndex, parent, node, label1, sysItem.IsPipingVertical);//TODO
            }
            else
            {
                SetLinkTextLocationForNormal(myLink, linkIndex, parent, node, label1, sysItem.IsPipingVertical);
            }

            System.Windows.Point pf = label1.Location;
                                  
           
            if (!string.IsNullOrEmpty(p1))
            {
                Child1.Location = pf;
                addFlowPiping.AddCaption(Child1);
                addFlowPiping.RemoveNode(label1);
                pf.Y += 8;
            }
            if (!string.IsNullOrEmpty(p2))
            {

                label2.Location = pf;
                Child2.Location = pf;
                addFlowPiping.AddCaption(Child2);
                addFlowPiping.RemoveNode(label2);
                pf.Y += 8;
            }
            if (!string.IsNullOrEmpty(p3))
            {
                label3.Location = pf;
                Child3.Location = pf;
                addFlowPiping.AddCaption(Child3);
                addFlowPiping.RemoveNode(label3);
                pf.Y += 8;
            }
            if (!string.IsNullOrEmpty(p5))
            {
                label5.Location = pf;
                Child5.Location = pf;
                addFlowPiping.AddCaption(Child5);
                addFlowPiping.RemoveNode(label5);
                pf.Y += 8;
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

        public string GetPipeSize_Inch(string orgPipeSize)
        {
            return _dal.GetPipeSize_Inch(orgPipeSize);
        }

        private void DrawPipeLableColor(JCHNode label, string pipeSize, Node nodeUnit, int GasNo)
        {
            initTextNode(label, pipeSize);
            label.IsLiquidGasPipeNode = true;
            addFlowPiping.AddNode(label);
            //if (nodeUnit != null)
            //    nodeUnit.AddFlow.Items.Add(label);

            switch (GasNo)
            {
                case 1:
                    label.Foreground = System.Windows.Media.Brushes.MediumPurple;
                    break;
                case 2:
                    label.Foreground = System.Windows.Media.Brushes.Blue;
                    break;
                case 3:
                    label.Foreground = System.Windows.Media.Brushes.Green;
                    break;
                case 4:
                    label.Foreground = System.Windows.Media.Brushes.Red;
                    break;
            }

        }

        private Caption DrawPipeCaptionColor(Caption label, string pipeSize, Node nodeUnit, int GasNo)
        {
           
             label = new Caption(0, 0, 90, 20, pipeSize, nodeUnit, addFlowPiping);            
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font ft = textFont_piping;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 8;
            label.Foreground = System.Windows.Media.Brushes.Black;
            label.Stroke = System.Windows.Media.Brushes.Black;
            label.Fill = System.Windows.Media.Brushes.Transparent;
            label.Text = pipeSize;
            SizeF size = gMeasureString.MeasureString(pipeSize, ft);
            label.Size = new System.Windows.Size(size.Width +5, size.Height + 3);
            label.ImagePosition = ImagePosition.LeftTop;
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            label.IsSelectable = false;
            
            switch (GasNo)
            {
                case 1:
                    label.Foreground = System.Windows.Media.Brushes.MediumPurple;
                    break;
                case 2:
                    label.Foreground = System.Windows.Media.Brushes.Blue;
                    break;
                case 3:
                    label.Foreground = System.Windows.Media.Brushes.Green;
                    break;
                case 4:
                    label.Foreground = System.Windows.Media.Brushes.Red;
                    break;
            }

            return label;
        }

        private void SetLinkTextLocationForBinaryTree(ng.MyLink myLink, int linkIndex, Node parent, Node node, Node label1, bool isVertical)
        {
            PointF ptText, ptParent, ptNode;
            ptText = getLeftCenterPointF(node);
            if (parent is ng.MyNodeOut)
            {
                ptParent = getLeftBottomPointF(parent);
            }
            else if (parent is ng.MyNodeCH)
            {
                if (isVertical)
                {
                    ptParent = getRightCenterPointF(parent);
                }
                else
                {
                    ptParent = getBottomCenterPointF(parent);
                }
            }
            else if (parent is ng.MyNodeMultiCH)
            {
                if (isVertical)
                {
                    ptParent = convertSystemPointToDrawingPoint(myLink.Points[2]);
                    //ptParent = myLink.Points[2];
                }
                else
                {
                    ptParent = getBottomCenterPointF(parent);
                }
            }
            else
            {
                ptParent = getCenterPointF(parent);
            }

            if (isVertical)
            {
                ptNode = getLeftCenterPointF(node);

                if (parent is ng.MyNodeOut)
                {                                      
                    ptNode = getRightTopPointF(node);
                    ptText.X = (float) (ptNode.X - label1.Size.Width - 55);
                    ptText.Y = ptNode.Y - 30;
                }
                else if(node is ng.MyNodeIn || node is ng.MyNodeCH)
                {
                    
                    ptNode = getTopCenterPointF(node);
                    ptText.X = ptNode.X + 5 ;
                    ptText.Y = ptNode.Y - 30;

                }
                else if(node is ng.MyNodeYP)
                {
                     ptText.X = (float)(((ptNode.X + ptParent.X - label1.Size.Width) / 2)-20);
                     ptText.Y = ptNode.Y;                  
                }
                else
                {
                    ptText.X = (float)(((ptNode.X + ptParent.X - label1.Size.Width) / 2) - 20);
                    ptText.Y = ptNode.Y ;
                }
            }
            else
            {
                ptNode = getTopCenterPointF(node);

                ptText.X = (float)(ptNode.X + label1.Size.Width);
                ptText.Y = (float)((ptNode.Y + ptParent.Y - label1.Size.Height) / 2);
            }

            if (linkIndex == 1)
            {
                if (parent is ng.MyNodeYP)
                {
                    if (node is ng.MyNodeIn)
                    {
                        ptParent = getCenterPointF(parent);
                        if (isVertical)
                        {
                            ptText.Y = (float)(ptParent.Y - label1.Size.Height - 2);
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
            label1.Location = convertPointFToWinPoint(ptText);
            // label1.Location = ptText;
        }

        private void SetLinkTextLocationForSechmaA(ng.MyLink myLink, int linkIndex, Node parent, Node node, Node label1, bool isVertical)
        {
            PointF ptText, ptParent, ptNode;
            ptText = getLeftCenterPointF(node);
            PipingOrientation inlinkOrientation = CheckLinkOrientation(node, myLink);

            if (inlinkOrientation == PipingOrientation.Down)
            {
                ptParent = convertSystemPointToDrawingPoint(parent.Location);
                //ptParent = parent.Location;
                ptNode = getBottomCenterPointF(node);

                ptText.X = (float)(ptNode.X - label1.Size.Width);
                //ptText.X = ptNode.X - label1.Size.Width;
                ptText.Y = (float)((ptNode.Y + ptParent.Y - label1.Size.Height) / 2);
                //ptText.Y = (ptNode.Y + ptParent.Y - label1.Size.Height) / 2;
            }
            else if (inlinkOrientation == PipingOrientation.Up)
            {
                ptParent = getLeftBottomPointF(parent);
                ptNode = getTopCenterPointF(node);

                ptText.X = (float)(ptNode.X - label1.Size.Width);
                ptText.Y = (float)((ptNode.Y + ptParent.Y - label1.Size.Height) / 2);
            }
            else if (inlinkOrientation == PipingOrientation.Left)
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
            else if (inlinkOrientation == PipingOrientation.Right)
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
            else if (inlinkOrientation == PipingOrientation.Unknown)
            {
                if (isVertical)
                {
                    if ((parent is ng.MyNodeYP) && (node is ng.MyNodeYP))
                    {
                        ptParent = getLeftBottomPointF(parent);
                        ptNode = getRightCenterPointF(node);
                        ptText.Y = (float)((ptNode.Y + ptParent.Y) / 2 - node.Size.Height);
                        ptText.X = (float)(ptNode.X - (label1.Size.Width + 5));
                    }
                    else if ((parent is ng.MyNodeYP) && (node is ng.MyNodeCH))
                    {
                        ptParent = getCenterPointF(parent);
                        ptNode = getCenterPointF(node);
                        ptText.X = (float)((ptNode.X + ptParent.X) / 2);
                        ptText.Y = (float)(ptNode.Y + Math.Abs((parent.Size.Height - 10)));
                    }
                    else
                    {
                        ptNode = getBottomCenterPointF(node);
                        ptText.X = (float)(ptNode.X - label1.Size.Width);
                        ptText.Y = (float)(ptNode.Y + (node.Size.Height));
                    }
                }
                else
                {
                    ptNode = getBottomCenterPointF(node);
                    ptText.X = (float)(ptNode.X + label1.Size.Width);
                    ptText.Y = (float)(ptNode.Y + (node.Size.Height));
                }
            }

            else
            {
                ptNode = getBottomCenterPointF(node);
                ptText.X = (float)(ptNode.X - label1.Size.Width);
                ptText.Y = (float)(ptNode.Y + (node.Size.Height));
            }
            if (isVertical)
            {
                if (node is ng.MyNodeIn)
                {
                    if (inlinkOrientation == PipingOrientation.Right)
                    {
                        ptNode = getRightCenterPointF(node);
                        ptText.X = ptNode.X + 20;
                        ptText.Y = ptNode.Y;
                    }
                    else
                    {
                        ptNode = getTopCenterPointF(node);
                       // ptNode = getLeftCenterPointF(node);
                        //ptText.X = (float)(ptNode.X + label1.Size.Width-80 + 20);
                        ptText.X = ptNode.X + 5;
                        ptText.Y = ptNode.Y - 35;
                    }
                }
                else if(node is ng.MyNodeCH)
                {
                    ptNode = getTopCenterPointF(node);
                    ptText.X = ptNode.X + 5;
                    ptText.Y = ptNode.Y - 33;
                }
                else
                {
                    ptText.X += 5;
                    if (myLink.Org is ng.MyNodeOut)
                    {
                        ptText.Y -= 100;
                    }
                }
            }
            else
            {
                if (node is ng.MyNodeIn)
                {
                    ptNode = getLeftCenterPointF(node);
                    ptParent = convertSystemPointToDrawingPoint(myLink.Points[0]);
                    ptText.X = ptText.X + 10;
                    //ptParent = myLink.Points[0];
                    //commented by SA
                    /* ptText.X = (float)(ptNode.X - (label1.Size.Width+40));
                     ptText.Y = ptNode.Y;
                     if (ptNode.Y < ptParent.Y)
                     {
                         ptText.Y = (float)(ptNode.Y - label1.Size.Height);
                     } */ //end of comment
                }
                else if (node is ng.MyNodeYP)
                {
                    if (parent is ng.MyNodeMultiCH)
                    {
                        ptParent = convertSystemPointToDrawingPoint(myLink.Points[1]);
                        //ptParent = myLink.Points[1];
                        ptNode = convertSystemPointToDrawingPoint(myLink.Points[2]);
                        //ptNode = myLink.Points[2];
                        ptText.X = (float)(ptParent.X + label1.Size.Width);
                        ptText.Y = ptParent.Y;
                        if (ptNode.Y < ptParent.Y)
                        {
                            ptText.Y = (float)(ptParent.Y - label1.Size.Height);
                        }
                    }
                }
                else if(node is ng.MyNodeCH)
                {
                    ptText.X += 10;
                    ptText.Y -= 10;
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

        public PipingOrientation CheckLinkOrientation(Node node, Link link)
        {
            if (node == null || link == null) return PipingOrientation.Unknown;

            bool left = false;
            bool right = false;
            bool up = false;
            bool down = false;
            float x1 = (float)node.Location.X;
            float x2 = (float)node.Location.X + (float)node.Size.Width;
            float y1 = (float)node.Location.Y;
            float y2 = (float)node.Location.Y + (float)node.Size.Height;
            PointF pt1, pt2;
            bool done = false;
            for (int i = 0; !done && i < link.Points.Count - 1; i++)
            {
                for (int j = 0; !done && j <= 1; j++)
                {
                    if (j == 0)
                    {
                        pt1 = convertSystemPointToDrawingPoint(link.Points[i]);
                        pt2 = convertSystemPointToDrawingPoint(link.Points[i + 1]);
                        //pt1 = link.Points[i];
                        //pt2 = link.Points[i + 1];
                    }
                    else
                    {
                        pt1 = convertSystemPointToDrawingPoint(link.Points[i + 1]);
                        pt2 = convertSystemPointToDrawingPoint(link.Points[i]);
                        //pt1 = link.Points[i + 1];
                        //pt2 = link.Points[i];
                    }
                    if (pt1.X >= x1 && pt1.X <= x2 && pt1.Y >= y1 && pt1.Y <= y2)
                    {
                        if (pt2.X < x1 || pt2.X > x2 || pt2.Y < y1 || pt2.Y > y2)
                        {
                            if (pt1.X == pt2.X)
                            {
                                if (pt2.Y < y1) up = true;
                                if (pt2.Y > y2) down = true;
                            }
                            if (pt1.Y == pt2.Y)
                            {
                                if (pt2.X < x1) left = true;
                                if (pt2.X > x2) right = true;
                            }
                            done = true;
                        }
                    }
                }
            }

            if (up) return PipingOrientation.Up;
            if (down) return PipingOrientation.Down;
            if (left) return PipingOrientation.Left;
            if (right) return PipingOrientation.Right;

            return PipingOrientation.Unknown;
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
                    ptParent = getRightCenterPointF(parent);
                    ptNode = getLeftCenterPointF(node);
                    if (ptParent.Y == ptText.Y)
                    {
                        ptText.X = (float)(((ptNode.X + ptParent.X - label1.Size.Width) / 2));
                    }
                    else
                    {
                        if(isVertical)
                            ptText.X = (float)(ptNode.X + label1.Size.Width);
                        else
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
                    ptText.X = (float)(ptNode.X - label1.Size.Width) - 40;
                    ptText.Y = (float)(ptNode.Y - label1.Size.Height);
                }
            }
            else if (node is ng.MyNodeIn)
            {
                if (isVertical)
                {
                    ptText.X = (float)(node.Location.X - (label1.Size.Height / 2) - 35);
                    ptText.Y = (float)(node.Location.Y + (node.Size.Width / 2));
                }
                else
                {
                    //ptText.X = (float)(ptText.X - label1.Size.Width);
                    ptText.X = (float)(node.Location.X + (node.Size.Width / 2));
                    ptText.Y = (float)(node.Location.Y - (node.Size.Height / 2) + 10);
                }

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

        private Caption initCaptionText(string inputtext, Node nodeUnit)
        {
            Caption label = new Caption(0, 0, 90, 20, inputtext, nodeUnit, addFlowPiping);
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font ft = textFont_piping;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 8;
            label.Foreground = System.Windows.Media.Brushes.Black;
            label.Stroke = System.Windows.Media.Brushes.Black;
            label.Fill = System.Windows.Media.Brushes.Transparent;
            label.Text = inputtext;
            SizeF size = gMeasureString.MeasureString(inputtext, ft);
            label.Size = new System.Windows.Size(size.Width + 5, size.Height + 3);
            label.ImagePosition = ImagePosition.LeftTop;
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            label.IsSelectable = false;
            return label;
        }
    }

}
