using JCHVRF.MyPipingBLL;
using Lassalle.WPF.Flow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ng = JCHVRF.Model.NextGen;
using old = JCHVRF.Model;

namespace JCHVRF_New.Utility
{
    public class UtilityWiring
    {
        private static JCHVRF.Model.Project ProjectLegacy;
        private Bitmap bmpMeasureString = null;
        private Graphics gMeasureString = null;
        public Font textFont_wiring = new Font("Arial", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        public Font textFont_wiring_linemark = new Font("Arial", 14f, FontStyle.Regular, GraphicsUnit.Pixel);
        public const float VDistanceVertical_wiring = 76;
        public const int HDistanceVertical_wiring = 236;
        public const int OutdoorOffset_Y_wiring = 38;
        System.Windows.Point convertedWinPoint = new System.Windows.Point();

        public UtilityWiring()
        {
            bmpMeasureString = new Bitmap(500, 100);
            gMeasureString = Graphics.FromImage(this.bmpMeasureString);
        }

        public UtilityWiring(JCHVRF.Model.Project thisProject)
        {
            ProjectLegacy = thisProject;
            bmpMeasureString = new Bitmap(500, 100);
            gMeasureString = Graphics.FromImage(this.bmpMeasureString);
        }

        public ng.WiringNodeOut createNodeOut_wiring()
        {
            ng.WiringNodeOut node = new ng.WiringNodeOut();
            node.Tooltip = old.NodeType.OUT.ToString();
            node.IsEditable = false;
            node.Stroke = System.Windows.Media.Brushes.Transparent;
            node.FontFamily = new System.Windows.Media.FontFamily("Arial");
            node.FontSize = 11;
            node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));

            return node;
        }
        public void setNode_wiring(Node node, string imgFile, AddFlow addFlowWiring)
        {
            BitmapImage img = new BitmapImage(new Uri("pack://application:,,," + imgFile, UriKind.RelativeOrAbsolute));
            ImageSource imgSrc = img;
            node.Image = imgSrc;
            var imgWidth = img.PixelWidth;
            var imgHeight = img.PixelHeight;
            node.Size = new System.Windows.Size(imgWidth, imgHeight);
            node.IsImageSizeFitContentArea = true;
            node.ImagePosition = ImagePosition.CenterBottom;
            node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            node.BackMode = BackMode.Transparent;
            node.IsSelectable = false;
            addFlowWiring.AddNode(node);
        }
        public void setNode_wiring(Node node, System.Windows.Size size, AddFlow addFlowWiring)
        {
            node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, size.Width, size.Height));
            node.Stroke = System.Windows.Media.Brushes.DarkGray;
            node.StrokeThickness = 1.6;
            node.Size = size;
            node.BackMode = BackMode.Transparent;
            node.IsSelectable = false;
            addFlowWiring.AddNode(node);
        }

        public Node createTextNode_wiring(string text, PointF pt, Node parent)
        {
            Node label = new Node();
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font ft = textFont_wiring;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 11;
            SizeF size = gMeasureString.MeasureString(text, ft);
            label.Fill = System.Windows.Media.Brushes.Transparent;
            label.Text = text;
            label.Size = new System.Windows.Size(size.Width + 15, size.Height);
            label.Stroke = System.Windows.Media.Brushes.Transparent;
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            label.IsSelectable = false;
            System.Windows.Point ptToPoint = new System.Windows.Point(pt.X, pt.Y);
            System.Windows.Point location = UtilityWiring.OffsetLocation(ptToPoint, parent.Location);
            if (parent is ng.WiringNodeCH)
            {
                location.X -= 60;
            }
            label.Location = location;
            parent.AddFlow.AddNode(label);
            return label;
        }
        public Node createTextNode_wiring(string text, PointF pt, Node parent, bool isLineMark = false)
        {
            Node label = new Node();
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font ft = textFont_wiring;
            if (isLineMark)
            {
                ft = textFont_wiring_linemark;
            }
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 14;
            SizeF size = gMeasureString.MeasureString(text, ft);
            label.Fill = System.Windows.Media.Brushes.Transparent;
            label.Text = text;
            label.Size = new System.Windows.Size(size.Width + 15, size.Height);
            if (isLineMark)
                label.Foreground = System.Windows.Media.Brushes.Red;

            label.Stroke = System.Windows.Media.Brushes.Transparent;
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            label.IsSelectable = false;
            PointF parentLoc = convertSystemPointToDrawingPoint(parent.Location);
            PointF location = UtilEMF.OffsetLocation(pt, parentLoc);
            if (parent is ng.WiringNodeCH)
            {
                location.X -= 60;
            }
            label.Location = convertPointFToWinPoint(location);
            parent.AddFlow.AddNode(label);
            return label;
        }


        public Node createTextNode_wiring(string text, PointF pt)
        {
            Node label = new Node();
            gMeasureString.PageUnit = GraphicsUnit.Pixel;
            Font ft = textFont_wiring;
            label.FontFamily = new System.Windows.Media.FontFamily("Arial");
            label.FontSize = 11;
            SizeF size = gMeasureString.MeasureString(text, ft);
            convertedWinPoint = convertPointFToWinPoint(pt);
            label.Location = convertedWinPoint;
            label.Fill = System.Windows.Media.Brushes.Transparent;
            label.Text = text;
            label.Size = new System.Windows.Size(size.Width + 15, size.Height);
            label.ImagePosition = ImagePosition.LeftTop;
            label.Stroke = System.Windows.Media.Brushes.Transparent;
            label.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            label.IsSelectable = false;
            return label;
        }

        public Node createLinePoint(PointF pt)
        {
            Node node = new Node();
            node.IsSelectable = false;
            node.Size = new System.Windows.Size(1, 1);
            node.Location = new System.Windows.Point(pt.X, pt.Y);
            node.Stroke = System.Windows.Media.Brushes.DarkGray;
            return node;
        }

        public Link createLine(Node src, Node dst, string linkText, AddFlow flowWiring)
        {
            Link lnk = new Link(src, dst, linkText, flowWiring);
            lnk.StrokeThickness = 1.8;
            lnk.Stroke = System.Windows.Media.Brushes.DarkGray;
            lnk.IsStretchable = false;
            lnk.IsSelectable = false;
            lnk.LineStyle = LineStyle.Polyline;
            return lnk;
        }

        public NodeElement_Wiring GetNodeElement_Wiring_ODU(old.Outdoor outItem, string brandCode)
        {
            string [] strs1 = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> unitFullName = new List<string>();
            foreach (string str1 in strs1)
            {
                if (str1.Contains("*"))
                {
                    string[] strs2 = str1.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs2.Length == 2)
                    {
                        int num = 0;
                        if (int.TryParse(strs2[1].Trim(), out num))
                        {
                            for (int i = 0; i < num; i++)
                            {
                                unitFullName.Add(strs2[0].Trim());
                            }
                        }
                    }
                }
                else
                    unitFullName.Add(str1.Trim());
            }
            int count = unitFullName.Count;

            old.Outdoor[] outItems = new old.Outdoor[count];
            if (count > 1)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        if (unitFullName[i].CompareTo(unitFullName[j]) == -1)
                        {
                            string largemodel = unitFullName[j];
                            unitFullName[j] = unitFullName[i];
                            unitFullName[i] = largemodel;
                        }
                    }
                }
                JCHVRF.DAL.OutdoorDAL dal = new JCHVRF.DAL.OutdoorDAL(outItem.RegionCode, brandCode, ProjectLegacy.RegionCode);
                for (int i = 0; i < count; i++)
                {
                    string modelname = (unitFullName[i]).Trim();
                    old.Outdoor coutItem = null;
                    if (((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV") && brandCode == "Y") || (outItem.Series == "Commercial VRF HP, YVAHP") || (outItem.Series == "Commercial VRF HR, YVAHR")) //马来西亚的YORK品牌以及北美机型需要这样处理 20170323 by Yunxiao Lin
                    {
                        coutItem = dal.GetYorkItemBySeries(modelname, outItem.Series);
                    }
                    else if (brandCode == "H")
                        coutItem = dal.GetHitachiItemBySeries(modelname, outItem.Series);
                    else
                        coutItem = dal.GetItemBySeries(modelname, outItem.Series);
                    if (coutItem != null)
                    {
                        outItems[i] = coutItem;
                    }
                }
            }
            else
                outItems[0] = outItem;

            JCHVRF.DAL.MyDictionaryDAL dicdal = new JCHVRF.DAL.MyDictionaryDAL();
            string strGroup2 = "";
            string strGroup3 = "";
            string strGroup4 = "";
            string modelGroup = "";
            foreach (old.Outdoor coutItem in outItems)
            {
                if (coutItem != null)
                {
                    if (outItem.Type.Contains("YVAHP") || outItem.Type.Contains("YVAHR"))
                    {
                        if (modelGroup != "")
                            modelGroup += "," + coutItem.Model_York;
                        else
                            modelGroup += coutItem.Model_York;
                    }
                    old.MyDictionary dic = dicdal.GetItem(old.MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1));

                    string strPowerLineType = "//";
                    string strPowerPhaseNumber = "R S";
                    if (dic.Name.Contains("3Ph"))
                    {
                        strPowerLineType = "////";
                        strPowerPhaseNumber = "L1L2L3N";
                    }
                    if (coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1) == "R")
                    {
                        strPowerPhaseNumber = "R S T";
                        strPowerLineType = "///";
                    }
                    string strPower = dic.Name.Replace("1Ph/", "").Replace("3Ph/", "");
                    double current = coutItem.MaxCurrent;
                    strPower += "/" + current.ToString() + "A";

                    if (strGroup2 != "")
                        strGroup2 += "|" + strPowerPhaseNumber;
                    else
                        strGroup2 += strPowerPhaseNumber;
                    if (strGroup3 != "")
                        strGroup3 += "|" + strPower;
                    else
                        strGroup3 += strPower;
                    if (strGroup4 != "")
                        strGroup4 += "," + strPowerLineType;
                    else
                        strGroup4 += strPowerLineType;
                }
            }

            string model = outItem.Model_York;
            if (!outItem.Type.Contains("YVAHP") && !outItem.Type.Contains("YVAHR"))
            {
                if (brandCode == "H")
                    model = outItem.Model_Hitachi;

                modelGroup = string.Join(",", unitFullName);
            }
            else
            {
                model = outItem.Model_York;
            }

            NodeElement_Wiring item = new NodeElement_Wiring("Out" + count.ToString(), model, count, modelGroup, strGroup2, strGroup3, strGroup4, brandCode);
            return item;
        }

        public NodeElement_Wiring GetNodeElement_Wiring_CH(string model, string powerSupply, string powerLineType, double powerCurrent)
        {
            string strPowerLineType = "//";
            string strPower = "";
            string strPowerPhaseNumber = "";
            if (!string.IsNullOrEmpty(powerSupply))
            {
                strPower = powerSupply;
                string[] strs1 = powerSupply.Split(new string[] { "=>" }, StringSplitOptions.None);
                if (strs1.Length > 1)
                {
                    strPowerPhaseNumber = strs1[0].Trim();
                    strPower = strs1[1].Trim();
                }
            }
            if (!string.IsNullOrEmpty(powerLineType))
            {
                if (powerLineType == "2")
                {
                    strPowerLineType = "//";
                }
                else if (powerLineType == "3")
                {
                    strPowerLineType = "///";
                }
                else if (powerLineType == "4")
                {
                    strPowerLineType = "////";
                }
            }
            if (powerCurrent > 0)
            {
                strPower += "/" + powerCurrent.ToString() + "A";
            }
            string brandCode = "H";
            NodeElement_Wiring item = new NodeElement_Wiring("CH", model, 1, model, strPowerPhaseNumber, strPower, strPowerLineType, brandCode);
            return item;
        }

        public NodeElement_Wiring GetNodeElement_Wiring_IDU(old.Indoor inItem, string brandCode, string outType, ref List<string> strArrayList_powerType, ref List<string> strArrayList_powerVoltage, ref List<double> dArrayList_powerCurrent, ref int powerIndex, ref bool isNewPower)
        {
            string model= inItem.Model_York;
            if (inItem.Model_York == "-")
                model = inItem.ModelFull;
            if (brandCode == "H")
                model = inItem.Model_Hitachi;
            if (outType.Contains("YVAHP") || outType.Contains("YVAHR"))
            {
                if (inItem.Model_York == "-")
                    model = inItem.ModelFull;
                else
                    model = inItem.Model_York;
            }

            string powerSupplyCode = inItem.ModelFull.Substring(inItem.ModelFull.Length - 4, 1);
            JCHVRF.DAL.MyDictionaryDAL dicdal = new JCHVRF.DAL.MyDictionaryDAL();
            old.MyDictionary dic = dicdal.GetItem(old.MyDictionary.DictionaryType.PowerSupply, powerSupplyCode);

            string strPowerLineType = "//";
            string strPowerPhaseNumber = "R S";
            if (dic.Name.Contains("3Ph"))
            {
                strPowerLineType = "////";
                strPowerPhaseNumber = "L1L2L3N";
            }
            if (powerSupplyCode == "R")
            {
                strPowerLineType = "///";
                strPowerPhaseNumber = "R S T";
            }
            string strPower = dic.Name;

            bool isFound = false;

            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                string compatPower = GetCompatiblePowerVoltage(strPower, strArrayList_powerVoltage[i]);
                if (!string.IsNullOrEmpty(compatPower))
                {
                    strPower = compatPower;
                    isFound = true;
                    powerIndex = i;
                    break;
                }
            }

            if (!isFound)
            {
                string strPowerPhaseNumber1 = "R S";
                if (strPower.Contains("3Ph"))
                {
                    strPowerPhaseNumber1 = "L1L2L3N";
                }
                if (powerSupplyCode == "R")
                {
                    strPowerPhaseNumber1 = "R S T";
                }

                isNewPower = true;
                powerIndex = strArrayList_powerType.Count;
                strArrayList_powerType.Add(strPowerPhaseNumber1);
                strArrayList_powerVoltage.Add(strPower);
                dArrayList_powerCurrent.Add(0);
            }
            else
            {
                strArrayList_powerVoltage[powerIndex] = strPower;
            }

            strPower = strPower.Replace("/1Ph/", "/").Replace("/3Ph/", "/");

            double current = inItem.RatedCurrent;
            dArrayList_powerCurrent[powerIndex] += current;
            strPower += "/" + current.ToString() + "A";
            NodeElement_Wiring item = new NodeElement_Wiring("Ind", model, 1, model, strPowerPhaseNumber, strPower, strPowerLineType, brandCode);
            return item;
        }

        private string GetCompatiblePowerVoltage(string power1, string power2)
        {
            if (power1 == power2) return power1;

            Regex regEx = new Regex("((\\d+?)(~(\\d+?))?)[Vv]/(\\dPh)/((\\d+?)(,(\\d+?))?)Hz");
            MatchCollection powerArr1 = regEx.Matches(power1);
            MatchCollection powerArr2 = regEx.Matches(power2);

            for (int i = 0; i < powerArr1.Count; i++)
            {
                for (int j = 0; j < powerArr2.Count; j++)
                {
                    Match match1 = powerArr1[i];
                    Match match2 = powerArr2[j];
                    if (match1.Value == match2.Value)
                    {
                        return match1.Value;
                    }
                    string strVoltage = "", strPh = "", strHz = "";

                    if (match1.Groups[1].Value == match2.Groups[1].Value)
                    {
                        strVoltage = match1.Groups[1].Value;
                    }
                    else
                    {
                        int minV1, maxV1, minV2, maxV2;
                        minV1 = int.Parse(match1.Groups[2].Value);
                        minV2 = int.Parse(match2.Groups[2].Value);
                        maxV1 = minV1;
                        maxV2 = minV2;
                        if (match1.Groups[4].Success)
                        {
                            maxV1 = int.Parse(match1.Groups[4].Value);
                        }
                        if (match2.Groups[4].Success)
                        {
                            maxV2 = int.Parse(match2.Groups[4].Value);
                        }

                        minV1 = Math.Max(minV1, minV2);
                        maxV1 = Math.Min(maxV1, maxV2);
                        if (minV1 == maxV1)
                        {
                            strVoltage = minV1.ToString();
                        }
                        else if (minV1 < maxV1)
                        {
                            strVoltage = string.Format("{0}~{1}", minV1, maxV1);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (match1.Groups[5].Value != match2.Groups[5].Value) continue;
                    strPh = match1.Groups[5].Value;

                    if (match1.Groups[6].Value == match2.Groups[6].Value)
                    {
                        strHz = match1.Groups[6].Value;
                    }
                    else if (match1.Groups[9].Success && match2.Groups[9].Success &&
                        match1.Groups[6].Value == match2.Groups[9].Value + "," + match2.Groups[7].Value)
                    {
                        strHz = match1.Groups[6].Value;
                    }
                    else if (match1.Groups[7].Value == match2.Groups[7].Value ||
                            match2.Groups[9].Success && match1.Groups[7].Value == match2.Groups[9].Value)
                    {
                        strHz = match1.Groups[7].Value;
                    }
                    else if (match1.Groups[9].Success &&
                        (match1.Groups[9].Value == match2.Groups[7].Value ||
                        match2.Groups[9].Success && match1.Groups[9].Value == match2.Groups[9].Value))
                    {
                        strHz = match1.Groups[9].Value;
                    }

                    if (strVoltage != "" && strPh != "" && strHz != "")
                    {
                        return string.Format("{0}V/{1}/{2}Hz", strVoltage, strPh, strHz);
                    }
                }
            }
            return "";
        }
        public System.Windows.Point getLocationChild_wiring(Node parent, Node node, int nodeIndex, bool isHeatRecovery)
        {
            System.Windows.Point ptf = new System.Windows.Point();

            if (parent is ng.WiringNodeOut)
                ptf.X = Convert.ToInt32(parent.Location.X) + HDistanceVertical_wiring + 60;
            else
                ptf.X = Convert.ToInt32(parent.Location.X) + HDistanceVertical_wiring + 10;

            if (isHeatRecovery && parent is ng.WiringNodeOut && node is ng.WiringNodeIn)
            {
                ptf.X += HDistanceVertical_wiring + 60;
            }
            if (node is ng.WiringNodeCH)
            {
                ptf.X += 60;
            }
            ptf.Y = Convert.ToInt32(parent.Location.Y) - OutdoorOffset_Y_wiring + VDistanceVertical_wiring * nodeIndex;
            return ptf;
        }

        public ng.WiringNodeCH createNodeCH_wiring(string model)
        {
            ng.WiringNodeCH node = new ng.WiringNodeCH();
            node.Tooltip = old.NodeType.CHbox.ToString();
            node.IsEditable = false;
            node.Stroke = System.Windows.Media.Brushes.Transparent;
            node.FontFamily = new System.Windows.Media.FontFamily("Arial");
            node.FontSize = 11;
            node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0)); ;
            node.Model = model;
            return node;
        }

        public ng.WiringNodeMultiCH createNodeMCH_wiring(string model)
        {
            ng.WiringNodeMultiCH node = new ng.WiringNodeMultiCH();
            node.Tooltip = old.NodeType.CHbox.ToString();
            node.IsEditable = false;
            node.Stroke = System.Windows.Media.Brushes.Transparent;
            node.FontFamily = new System.Windows.Media.FontFamily("Arial");
            node.FontSize = 11;
            node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0)); ;
            node.Model = model;
            return node;
        }

        public ng.WiringNodeIn createNodeIn_wiring(old.RoomIndoor riItem)
        {
            ng.WiringNodeIn node = new ng.WiringNodeIn();
            node.Tooltip = old.NodeType.IN.ToString();
            node.IsEditable = false;
            node.Stroke = System.Windows.Media.Brushes.Transparent;
            node.FontFamily = new System.Windows.Media.FontFamily("Arial");
            node.FontSize = 11;
            node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
            node.RoomIndoorItem = riItem;
            return node;
        }
        public static System.Windows.Point OffsetLocation(System.Windows.Point ptSrc, System.Windows.Point ptOffset)
        {
            return new System.Windows.Point(ptSrc.X + ptOffset.X, ptSrc.Y + ptOffset.Y);
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
    }
}
