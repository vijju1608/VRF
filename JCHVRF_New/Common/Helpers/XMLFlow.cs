//---------------------------------------------------------------------------------------
// Lassalle.WPF.Flow.XML component
// Copyright (c) 2009-2016 Lassalle Technologies. All Rights Reserved.
// http://www.lassalle.com
// Author: Patrick Lassalle
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND EITHER EXPRESSED OR IMPLIED.
// ---------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Markup;
using System.IO;
using Lassalle.WPF.Flow;

namespace Lassalle.WPF.XMLFlow
{   
    /// <summary>
    /// This static class allows serializing an AddFlow diagram. 
    /// More precisely, it offers methods for saving/loading an 
    /// AddFlow diagram or copying/pasting a portion of it.
    /// </summary>
    public static class Serial
    {
        #region Properties

        #region ZOrder

        static readonly DependencyProperty ZorderSerialProperty =
            DependencyProperty.RegisterAttached("ZorderSerial", typeof(int), typeof(Serial),
                new FrameworkPropertyMetadata(0));

        internal static int GetZorderSerial(DependencyObject item)
        {
            if (item == null)
                return 0;
            return (int)item.GetValue(ZorderSerialProperty);
        }

        internal static void SetZorderSerial(DependencyObject item, int value)
        {
            if (item != null)
                item.SetValue(ZorderSerialProperty, value);
        }

        #endregion

        #region Owner

        static readonly DependencyProperty OwnerIndexProperty =
            DependencyProperty.RegisterAttached("OwnerIndex", typeof(int), typeof(Serial),
                new FrameworkPropertyMetadata(-1));

        internal static int GetOwnerIndex(DependencyObject item)
        {
            if (item == null)
                return -1;
            return (int)item.GetValue(OwnerIndexProperty);
        }

        internal static void SetOwnerIndex(DependencyObject item, int value)
        {
            if (item != null)
                item.SetValue(OwnerIndexProperty, value);
        }

        #endregion

        #endregion

        #region Methods
        public static void LoadSavedAddFlow(AddFlow addflow, string root)
        {
            XElement element = XElement.Parse(root);
            LoadXML(addflow, element);
        }

        public static string GetSavedAddFlow(AddFlow addflow)
        {
            XElement root = Serial.SaveXMLInt(addflow);
            return root.ToString();
        }

        /// <summary>Generates an AddFlow diagram from its XML representation.</summary>
        /// <param name="addflow">The AddFlow control where the diagram is loaded</param>
        /// <param name="root">The XML root element of the XML document representing 
        /// the AddFlow diagram</param>
        public static void LoadXML(AddFlow addflow, XElement root)
        {
            Serial.LoadXMLInt(addflow, root, false);
        }

        /// <summary>Converts the AddFlow diagram into its XML representation.</summary>
        /// <param name="addflow">The AddFlow control containing the diagram that 
        /// is saved</param>
        /// <returns>The XML root element of the XML document representing the AddFlow 
        /// diagram</returns>
        public static XElement SaveXML(AddFlow addflow)
        {
            return Serial.SaveXMLInt(addflow);
        }

        /// <summary>Removes the current selection from the addflow control 
        /// and copies it to the Clipboard</summary>
        /// <param name="addflow">The AddFlow control where the cut action occurs</param>
        public static void Cut(AddFlow addflow)
        {
            Serial.CopyInt(addflow);
            addflow.Delete();
        }

        /// <summary>Command that copies the current selection of the AddFlow
        /// diagram to the clipboard</summary>
        /// <param name="addflow">The AddFlow control where the copy action occurs</param>
        public static void Copy(AddFlow addflow)
        {
            Serial.CopyInt(addflow);
        }

        /// <summary>Pastes the contents of the Clipboard over the AddFlow diagram. </summary>
        /// <param name="acceptDefaultValues"></param>
        /// <param name="addflow">The AddFlow control where the paste action occurs</param>
        public static void Paste(AddFlow addflow, bool acceptDefaultValues)
        {
            Serial.PasteInt(addflow, acceptDefaultValues);
        }

        #endregion

        #region Helpers

        #region Clipboard

        internal static void CopyInt(AddFlow addflow)
        {
            foreach (Item item in addflow.SelectedItems)
            {
                Serial.SetZorderSerial(item, addflow.Items.IndexOf(item));
            }

            IEnumerable<Item> selitems = from item in addflow.SelectedItems orderby Serial.GetZorderSerial(item) ascending select item;

            IEnumerable<Node> nodes = selitems.OfType<Node>();

            // Remove the selected links whose either the origin node either 
            // the destination node is not selected
            List<Link> links = new List<Link>();
            foreach (Link link in selitems.OfType<Link>())
            {
                if (!link.IsSelected || (link.Org.IsSelected && link.Dst.IsSelected))
                {
                    links.Add(link);
                }
            }

            List<Caption> captions = selitems.OfType<Caption>().ToList();
            
            XElement nodeModelXML = Serial.GetNodeModelXML(addflow.NodeModel);
            XElement linkModelXML = Serial.GetLinkModelXML(addflow.LinkModel);
            XElement captionModelXML = Serial.GetCaptionModelXML(addflow.CaptionModel);
            XElement nodesXML = Serial.GetNodesXML(addflow, addflow.NodeModel, nodes);
            XElement linksXML = Serial.GetLinksXML(addflow, nodes.ToList(), addflow.LinkModel, links);
            XElement captionsXML = Serial.GetCaptionsXML(addflow, addflow.SelectedItems, addflow.CaptionModel, captions);
            XAttribute nodesAtt = new XAttribute("Nodes", nodes.Count());
            XAttribute linksAtt = new XAttribute("Links", links.Count());
            XAttribute captionsAtt = new XAttribute("Captions", captions.Count());
            XElement root = new XElement("AddFlow", nodesAtt, linksAtt, captionsAtt);
            root.Add(nodeModelXML);
            root.Add(linkModelXML);
            root.Add(captionModelXML);
            root.Add(nodesXML);
            root.Add(linksXML);

            Clipboard.Clear();
            Clipboard.SetData(DataFormats.Text, root);
        }

        internal static void PasteInt(AddFlow addflow, bool acceptDefaultValues)
        {
            XElement root = LoadSerializedDataFromClipBoard();
            if (root == null)
            {
                return;
            }

            // We group all the load actions in only one action so that we could undo
            // the paste action in one time.
            addflow.TaskManager.BeginAction(1001);

            int nodeCount = root.Attribute("Nodes") != null ?
                int.Parse(root.Attribute("Nodes").Value) : 0;
            int linkCount = root.Attribute("Links") != null ?
                int.Parse(root.Attribute("Links").Value) : 0;
            int captionCount = root.Attribute("Captions") != null ?
                int.Parse(root.Attribute("Captions").Value) : 0;
            int oldItemCount = addflow.Items.Count;

            // Deserialize the node and link models
            // Load the node model
            XElement nodeModelXML = root.Element("NodeModel");
            Node nodeModel = (nodeModelXML != null) ?
                Serial.DeserializeNodeModel(addflow, nodeModelXML) : null;

            // Load the link model
            XElement linkModelXML = root.Element("LinkModel");
            Link linkModel = (linkModelXML != null) ?
                Serial.DeserializeLinkModel(addflow, linkModelXML) : null;

            // Load the caption model
            XElement captionModelXML = root.Element("CaptionModel");
            Caption captionModel = (captionModelXML != null) ?
                Serial.DeserializeCaptionModel(addflow, captionModelXML) : null;

            // Deserialize the nodes
            List<Node> addedNodes = new List<Node>();
            IEnumerable<XElement> nodesXML = root.Elements("Nodes").Elements("Node");
            foreach (XElement nodeXML in nodesXML)
            {
                Node node = Serial.DeserializeNode(addflow, nodeXML, nodeModel);
                addedNodes.Add(node);
            }

            // Deserialize the links
            List<Link> addedLinks = new List<Link>();
            IEnumerable<XElement> linksXML = root.Elements("Links").Elements("Link");
            foreach (XElement linkXML in linksXML)
            {
                Link link = Serial.DeserializeLink(addflow, linkXML, linkModel, addedNodes);
                addedLinks.Add(link);
            }

            // Deserialize the captions
            List<Caption> addedCaptions = new List<Caption>();
            IEnumerable<XElement> captionsXML = root.Elements("Captions").Elements("Caption");
            foreach (XElement captionXML in captionsXML)
            {
                Caption caption = Serial.DeserializeCaption(addflow, captionXML, captionModel);
                addedCaptions.Add(caption);
            }

            // Translation
            // We move a little each pasted node and link so that they do not recover
            // the original items.
            // We first save the collection of points for each link
            List<PointCollection> pointCollections = new List<PointCollection>();
            foreach (Link link in addedLinks)
            {
                PointCollection points = new PointCollection(link.Points);
                pointCollections.Add(points);
            }

            double dx = addflow.GridSize.Width;
            double dy = addflow.GridSize.Height;
            foreach (Node node in addedNodes)
            {
                Point pt = node.Location;
                pt.X += dx;
                pt.Y += dy;
                node.Location = pt;
            }

            int idxPointCollection = 0;
            foreach (Link link in addedLinks)
            {
                for (int k = 0; k < link.Points.Count; k++)
                {
                    Point pt = pointCollections[idxPointCollection][k];
                    link.Points[k] = new Point(pt.X + dx, pt.Y + dy);
                }
                idxPointCollection++;
            }

            // Manage ZOrder
            List<Item> addedItems = new List<Item>();
            foreach (Node node in addedNodes)
            {
                addedItems.Add(node);
            }
            foreach (Link link in addedLinks)
            {
                addedItems.Add(link);
            }
            foreach (Caption caption in addedCaptions)
            {
                addedItems.Add(caption);
            }
            var items = from item in addedItems orderby Serial.GetZorderSerial(item) select item;
            foreach (Item item in items)
            {
                item.IsSelected = true;
            }
            addflow.BringToFront();

            // If we decide that the default values defined in the XML file will be
            // also the default values of the AddFlow control...
            if (acceptDefaultValues)
            {
                Serial.CopyNodeModelProperties(addflow.NodeModel, nodeModel);
                Serial.CopyLinkModelProperties(addflow.LinkModel, linkModel);
                Serial.CopyCaptionModelProperties(addflow.CaptionModel, captionModel);
            }

            // End action
            addflow.TaskManager.EndAction();
        }


        #endregion

        #region Load

        internal static void LoadXMLInt(AddFlow addflow, XElement root, 
            bool acceptDefaultValues)
        {
            addflow.BeginUpdate();

            // Load the count of nodes and the count of links (currently not used)
            int nodeCount = root.Attribute("Nodes") != null ? 
                int.Parse(root.Attribute("Nodes").Value) : 0;
            int linkCount = root.Attribute("Links") != null ? 
                int.Parse(root.Attribute("Links").Value) : 0;
            int captionCount = root.Attribute("Captions") != null ? 
                int.Parse(root.Attribute("Captions").Value) : 0;

            // Load the node model
            XElement nodeModelXML = root.Element("NodeModel");
            Node nodeModel = (nodeModelXML != null) ?
                Serial.DeserializeNodeModel(addflow, nodeModelXML) : null;

            // Load the link model
            XElement linkModelXML = root.Element("LinkModel");
            Link linkModel = (linkModelXML != null) ?
                Serial.DeserializeLinkModel(addflow, linkModelXML) : null;

            // Load the caption model
            XElement captionModelXML = root.Element("CaptionModel");
            Caption captionModel = (captionModelXML != null) ?
                Serial.DeserializeCaptionModel(addflow, captionModelXML) : null;

            // Load all the nodes
            List<Node> addedNodes = new List<Node>();
            IEnumerable<XElement> nodesXML = root.Elements("Nodes").Elements("Node");
            foreach (XElement nodeXML in nodesXML)
            {
                Node node = Serial.DeserializeNode(addflow, nodeXML, nodeModel);
                addedNodes.Add(node);
            }

            // Load all the links
            IEnumerable<XElement> linksXML = root.Elements("Links").Elements("Link");
            foreach (XElement linkXML in linksXML)
            {
                Link link = Serial.DeserializeLink(addflow, linkXML, linkModel, addedNodes);
            }

            // Load all the captions
            List<Caption> addedCaptions = new List<Caption>();
            IEnumerable<XElement> captionsXML = root.Elements("Captions").Elements("Caption");
            foreach (XElement captionXML in captionsXML)
            {
                Caption caption = Serial.DeserializeCaption(addflow, captionXML, captionModel);
                addedCaptions.Add(caption);
            }

            List<Item> ordered = (from item in addflow.Items
                                  orderby Serial.GetZorderSerial(item) ascending
                                  select item).ToList();
            addflow.Clear();
            foreach (Item item in ordered)
            {
                addflow.Items.Add(item);
            }

            // Assign caption owners
            foreach (Caption caption in addedCaptions)
            {
                if (Serial.GetOwnerIndex(caption) >= 0)
                {
                    caption.Owner = addflow.Items[Serial.GetOwnerIndex(caption)];
                }
            }

            // If we decide that the default values defined in the XML file will be
            // also the default values of the AddFlow control...
            if (acceptDefaultValues)
            {
                Serial.CopyNodeModelProperties(addflow.NodeModel, nodeModel);
                Serial.CopyLinkModelProperties(addflow.LinkModel, linkModel);
                Serial.CopyCaptionModelProperties(addflow.CaptionModel, captionModel);
            }

            addflow.EndUpdate();
        }

        private static Node DeserializeNodeModel(AddFlow addflow, XElement nodeXML)
        {
            Node node = new Node();
            double width = nodeXML.Attribute("Width") == null ? 0 :
                Double.Parse(nodeXML.Attribute("Width").Value, CultureInfo.InvariantCulture);
            double height = nodeXML.Attribute("Height") == null ? 0 :
                Double.Parse(nodeXML.Attribute("Height").Value, CultureInfo.InvariantCulture);
            double left = nodeXML.Attribute("Left") == null ? 0 :
                Double.Parse(nodeXML.Attribute("Left").Value, CultureInfo.InvariantCulture);
            double top = nodeXML.Attribute("Top") == null ? 0 :
                Double.Parse(nodeXML.Attribute("Top").Value, CultureInfo.InvariantCulture);

            if (nodeXML.Element("Text") != null)
            {
                node.Text = nodeXML.Element("Text").Value;
            }
            if (nodeXML.Element("Tooltip") != null)
            {
                node.Tooltip = nodeXML.Element("Tooltip").Value;
            }
            if (nodeXML.Element("IsHitTestVisible") != null)
            {
                node.IsSelectable =
                    (nodeXML.Element("IsHitTestVisible").Value == true.ToString());
            }
            if (nodeXML.Element("IsSelectable") != null)
            {
                node.IsSelectable =
                    (nodeXML.Element("IsSelectable").Value == true.ToString());
            }
            if (nodeXML.Element("IsXMoveable") != null)
            {
                node.IsXMoveable =
                    (nodeXML.Element("IsXMoveable").Value == true.ToString());
            }
            if (nodeXML.Element("IsYMoveable") != null)
            {
                node.IsYMoveable =
                    (nodeXML.Element("IsYMoveable").Value == true.ToString());
            }
            if (nodeXML.Element("IsXSizeable") != null)
            {
                node.IsXSizeable =
                    (nodeXML.Element("IsXSizeable").Value == true.ToString());
            }
            if (nodeXML.Element("IsYSizeable") != null)
            {
                node.IsYSizeable =
                    (nodeXML.Element("IsYSizeable").Value == true.ToString());
            }
            if (nodeXML.Element("IsRotatable") != null)
            {
                node.IsRotatable =
                    (nodeXML.Element("IsRotatable").Value == true.ToString());
            }
            if (nodeXML.Element("IsOutLinkable") != null)
            {
                node.IsOutLinkable =
                    (nodeXML.Element("IsOutLinkable").Value == true.ToString());
            }
            if (nodeXML.Element("IsInLinkable") != null)
            {
                node.IsInLinkable =
                    (nodeXML.Element("IsInLinkable").Value == true.ToString());
            }
            if (nodeXML.Element("IsEditable") != null)
            {
                node.IsEditable =
                    (nodeXML.Element("IsEditable").Value == true.ToString());
            }
            if (nodeXML.Element("Geometry") != null)
            {
                string s = nodeXML.Element("Geometry").Value;
                if (s == "System.Windows.Media.RectangleGeometry")
                {
                    node.Geometry = Geometry.Parse(s);
                    node.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
                }
                else if (s == "System.Windows.Media.EllipseGeometry")
                {
                    //node.Geometry = Geometry.Parse(s);
                    node.Geometry = new EllipseGeometry(new Rect(0, 0, 3, 3));
                }
                else
                {
                    node.Geometry = Geometry.Parse(s);
                }
            }
            if (nodeXML.Element("RotationAngle") != null)
            {
                node.RotationAngle = Double.Parse(
                    nodeXML.Element("RotationAngle").Value, CultureInfo.InvariantCulture);
            }
            if (nodeXML.Element("DashStyle") != null)
            {
                node.DashStyle = XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("DashStyle").Value))) as DashStyle;
            }
            if (nodeXML.Element("Fill") != null)
            {
                node.Fill = XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("Fill").Value))) as Brush;
            }
            if (nodeXML.Element("Stroke") != null)
            {
                node.Stroke = XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("Stroke").Value))) as Brush;
            }
            if (nodeXML.Element("StrokeThickness") != null)
            {
                node.StrokeThickness = Double.Parse(
                    nodeXML.Element("StrokeThickness").Value, CultureInfo.InvariantCulture);
            }
            if (nodeXML.Element("OutConnectionMode") != null)
            {
                node.OutConnectionMode = (ConnectionMode)Enum.Parse(typeof(ConnectionMode),
                    nodeXML.Element("OutConnectionMode").Value, false);
            }
            if (nodeXML.Element("InConnectionMode") != null)
            {
                node.InConnectionMode = (ConnectionMode)Enum.Parse(typeof(ConnectionMode),
                    nodeXML.Element("InConnectionMode").Value, false);
            }
            if (nodeXML.Element("TextPosition") != null)
            {
                node.TextPosition = (TextPosition)Enum.Parse(typeof(TextPosition),
                    nodeXML.Element("TextPosition").Value, false);
            }
            if (nodeXML.Element("TextTrimming") != null)
            {
                node.TextTrimming = (TextTrimming)Enum.Parse(typeof(TextTrimming),
                    nodeXML.Element("TextTrimming").Value, false);
            }
            if (nodeXML.Element("ImagePosition") != null)
            {
                node.ImagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition),
                    nodeXML.Element("ImagePosition").Value, false);
            }
            if (nodeXML.Element("TextMargin") != null)
            {
                node.TextMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("TextMargin").Value)));
            }
            if (nodeXML.Element("ImageMargin") != null)
            {
                node.ImageMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("ImageMargin").Value)));
            }

            // Font
            if (nodeXML.Element("FontFamily") != null)
            {
                node.FontFamily = XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("FontFamily").Value))) as FontFamily;
            }
            if (nodeXML.Element("FontSize") != null)
            {
                node.FontSize = Double.Parse(
                    nodeXML.Element("FontSize").Value, CultureInfo.InvariantCulture);
            }
            if (nodeXML.Element("Foreground") != null)
            {
                node.Foreground = XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("Foreground").Value))) as Brush;
            }
            if (nodeXML.Element("FontStyle") != null)
            {
                node.FontStyle = (FontStyle)XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("FontStyle").Value)));
            }
            if (nodeXML.Element("FontWeight") != null)
            {
                node.FontWeight = (FontWeight)XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("FontWeight").Value)));
            }
            if (nodeXML.Element("FontStretch") != null)
            {
                node.FontStretch = (FontStretch)XamlReader.Load(XmlReader.Create(
                    new StringReader(nodeXML.Element("FontStretch").Value)));
            }
            if (nodeXML.Element("FontUnderline") != null)
            {
                node.FontUnderline = (nodeXML.Element("FontUnderline").Value == true.ToString());
            }
            if (nodeXML.Element("FontStrikethrough") != null)
            {
                node.FontStrikethrough = (nodeXML.Element("FontStrikethrough").Value == true.ToString());
            }
            return node;
        }

        private static Link DeserializeLinkModel(AddFlow addflow, XElement linkXML)
        {
            Link link = new Link();
            if (linkXML.Element("Text") != null)
            {
                link.Text = linkXML.Element("Text").Value;
            }
            if (linkXML.Element("Tooltip") != null)
            {
                link.Tooltip = linkXML.Element("Tooltip").Value;
            }
            if (linkXML.Element("IsHitTestVisible") != null)
            {
                link.IsHitTestVisible =
                    (linkXML.Element("IsHitTestVisible").Value == true.ToString());
            }
            if (linkXML.Element("IsSelectable") != null)
            {
                link.IsSelectable =
                    (linkXML.Element("IsSelectable").Value == true.ToString());
            }
            if (linkXML.Element("IsStretchable") != null)
            {
                link.IsStretchable =
                    (linkXML.Element("IsStretchable").Value == true.ToString());
            }
            if (linkXML.Element("IsOrientedText") != null)
            {
                link.IsOrientedText =
                    (linkXML.Element("IsOrientedText").Value == true.ToString());
            }
            if (linkXML.Element("ArrowGeometryDst") != null)
            {
                link.ArrowGeometryDst =
                    Geometry.Parse(linkXML.Element("ArrowGeometryDst").Value);
            }
            if (linkXML.Element("ArrowGeometryOrg") != null)
            {
                link.ArrowGeometryOrg =
                    Geometry.Parse(linkXML.Element("ArrowGeometryOrg").Value);
            }
            if (linkXML.Element("ArrowGeometryMid") != null)
            {
                link.ArrowGeometryMid =
                    Geometry.Parse(linkXML.Element("ArrowGeometryMid").Value);
            }
            if (linkXML.Element("DashStyle") != null)
            {
                link.DashStyle = XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("DashStyle").Value))) as DashStyle;
            }
            if (linkXML.Element("Stroke") != null)
            {
                link.Stroke = XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("Stroke").Value))) as Brush;
            }
            if (linkXML.Element("StrokeThickness") != null)
            {
                link.StrokeThickness = Double.Parse(
                    linkXML.Element("StrokeThickness").Value, CultureInfo.InvariantCulture);
            }
            if (linkXML.Element("RoundedCornerSize") != null)
            {
                link.RoundedCornerSize =
                    Double.Parse(linkXML.Element("RoundedCornerSize").Value);
            }
            if (linkXML.Element("LineStyle") != null)
            {
                link.LineStyle = (LineStyle)Enum.Parse(typeof(LineStyle),
                    linkXML.Element("LineStyle").Value, false);
            }
            if (linkXML.Element("TextPlacementMode") != null)
            {
                link.TextPlacementMode = (TextPlacementMode)Enum.Parse(typeof(TextPlacementMode),
                    linkXML.Element("TextPlacementMode").Value, false);
            }
            if (linkXML.Element("BackMode") != null)
            {
                link.BackMode = (BackMode)Enum.Parse(typeof(BackMode),
                    linkXML.Element("BackMode").Value, false);
            }

            // Font
            if (linkXML.Element("FontFamily") != null)
            {
                link.FontFamily = XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("FontFamily").Value))) as FontFamily;
            }

            if (linkXML.Element("FontSize") != null)
            {
                link.FontSize = Double.Parse(
                    linkXML.Element("FontSize").Value, CultureInfo.InvariantCulture);
            }
            if (linkXML.Element("Foreground") != null)
            {
                link.Foreground = XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("Foreground").Value))) as Brush;
            }
            if (linkXML.Element("FontStyle") != null)
            {
                link.FontStyle = (FontStyle)XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("FontStyle").Value)));
            }
            if (linkXML.Element("FontWeight") != null)
            {
                link.FontWeight = (FontWeight)XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("FontWeight").Value)));
            }
            if (linkXML.Element("FontStretch") != null)
            {
                link.FontStretch = (FontStretch)XamlReader.Load(XmlReader.Create(
                    new StringReader(linkXML.Element("FontStretch").Value)));
            }
            if (linkXML.Element("FontUnderline") != null)
            {
                link.FontUnderline = (linkXML.Element("FontUnderline").Value == true.ToString());
            }
            if (linkXML.Element("FontStrikethrough") != null)
            {
                link.FontStrikethrough = (linkXML.Element("FontStrikethrough").Value == true.ToString());
            }
            return link;
        }

        private static Caption DeserializeCaptionModel(AddFlow addflow, XElement captionXML)
        {
            Caption caption = new Caption();
            double width = captionXML.Attribute("Width") == null ? 0 :
                Double.Parse(captionXML.Attribute("Width").Value, CultureInfo.InvariantCulture);
            double height = captionXML.Attribute("Height") == null ? 0 :
                Double.Parse(captionXML.Attribute("Height").Value, CultureInfo.InvariantCulture);
            double left = captionXML.Attribute("Left") == null ? 0 :
                Double.Parse(captionXML.Attribute("Left").Value, CultureInfo.InvariantCulture);
            double top = captionXML.Attribute("Top") == null ? 0 :
                Double.Parse(captionXML.Attribute("Top").Value, CultureInfo.InvariantCulture);

            if (captionXML.Element("Text") != null)
            {
                caption.Text = captionXML.Element("Text").Value;
            }
            if (captionXML.Element("Tooltip") != null)
            {
                caption.Tooltip = captionXML.Element("Tooltip").Value;
            }
            if (captionXML.Element("AnchorPositionOnLink") != null)
            {
                caption.AnchorPositionOnLink = Double.Parse(
                    captionXML.Element("AnchorPositionOnLink").Value, CultureInfo.InvariantCulture);
            }
            if (captionXML.Element("Dock") != null)
            {
                caption.Dock = (DockStyle)Enum.Parse(typeof(DockStyle),
                    captionXML.Element("Dock").Value, false);
            }
            if (captionXML.Element("IsHitTestVisible") != null)
            {
                caption.IsHitTestVisible =
                    (captionXML.Element("IsHitTestVisible").Value == true.ToString());
            }
            if (captionXML.Element("IsSelectable") != null)
            {
                caption.IsSelectable =
                    (captionXML.Element("IsSelectable").Value == true.ToString());
            }
            if (captionXML.Element("IsXMoveable") != null)
            {
                caption.IsXMoveable =
                    (captionXML.Element("IsXMoveable").Value == true.ToString());
            }
            if (captionXML.Element("IsYMoveable") != null)
            {
                caption.IsYMoveable =
                    (captionXML.Element("IsYMoveable").Value == true.ToString());
            }
            if (captionXML.Element("IsXSizeable") != null)
            {
                caption.IsXSizeable =
                    (captionXML.Element("IsXSizeable").Value == true.ToString());
            }
            if (captionXML.Element("IsYSizeable") != null)
            {
                caption.IsYSizeable =
                    (captionXML.Element("IsYSizeable").Value == true.ToString());
            }
            if (captionXML.Element("IsRotatable") != null)
            {
                caption.IsRotatable =
                    (captionXML.Element("IsRotatable").Value == true.ToString());
            }
            if (captionXML.Element("IsEditable") != null)
            {
                caption.IsEditable =
                    (captionXML.Element("IsEditable").Value == true.ToString());
            }
            if (captionXML.Element("Geometry") != null)
            {
                string s = captionXML.Element("Geometry").Value;
                if (s == "System.Windows.Media.RectangleGeometry")
                {
                    //caption.Geometry = Geometry.Parse(s);
                    caption.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
                }
                else if (s == "System.Windows.Media.EllipseGeometry")
                {
                    //caption.Geometry = Geometry.Parse(s);
                    caption.Geometry = new EllipseGeometry(new Rect(0, 0, 3, 3));
                }
                else
                {
                    caption.Geometry = Geometry.Parse(s);
                }
            }
            if (captionXML.Element("RotationAngle") != null)
            {
                caption.RotationAngle = Double.Parse(
                    captionXML.Element("RotationAngle").Value, CultureInfo.InvariantCulture);
            }
            if (captionXML.Element("DashStyle") != null)
            {
                caption.DashStyle = XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("DashStyle").Value))) as DashStyle;
            }
            if (captionXML.Element("Fill") != null)
            {
                caption.Fill = XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("Fill").Value))) as Brush;
            }
            if (captionXML.Element("Stroke") != null)
            {
                caption.Stroke = XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("Stroke").Value))) as Brush;
            }
            if (captionXML.Element("StrokeThickness") != null)
            {
                caption.StrokeThickness = Double.Parse(
                    captionXML.Element("StrokeThickness").Value, CultureInfo.InvariantCulture);
            }
            if (captionXML.Element("TextPosition") != null)
            {
                caption.TextPosition = (TextPosition)Enum.Parse(typeof(TextPosition),
                    captionXML.Element("TextPosition").Value, false);
            }
            if (captionXML.Element("TextTrimming") != null)
            {
                caption.TextTrimming = (TextTrimming)Enum.Parse(typeof(TextTrimming),
                    captionXML.Element("TextTrimming").Value, false);
            }
            if (captionXML.Element("ImagePosition") != null)
            {
                caption.ImagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition),
                    captionXML.Element("ImagePosition").Value, false);
            }
            if (captionXML.Element("TextMargin") != null)
            {
                caption.TextMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("TextMargin").Value)));
            }
            if (captionXML.Element("ImageMargin") != null)
            {
                caption.ImageMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("ImageMargin").Value)));
            }

            // Font
            if (captionXML.Element("FontFamily") != null)
            {
                caption.FontFamily = XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("FontFamily").Value))) as FontFamily;
            }
            if (captionXML.Element("FontSize") != null)
            {
                caption.FontSize = Double.Parse(
                    captionXML.Element("FontSize").Value, CultureInfo.InvariantCulture);
            }
            if (captionXML.Element("Foreground") != null)
            {
                caption.Foreground = XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("Foreground").Value))) as Brush;
            }
            if (captionXML.Element("FontStyle") != null)
            {
                caption.FontStyle = (FontStyle)XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("FontStyle").Value)));
            }
            if (captionXML.Element("FontWeight") != null)
            {
                caption.FontWeight = (FontWeight)XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("FontWeight").Value)));
            }
            if (captionXML.Element("FontStretch") != null)
            {
                caption.FontStretch = (FontStretch)XamlReader.Load(XmlReader.Create(
                    new StringReader(captionXML.Element("FontStretch").Value)));
            }
            if (captionXML.Element("FontUnderline") != null)
            {
                caption.FontUnderline = (captionXML.Element("FontUnderline").Value == true.ToString());
            }
            if (captionXML.Element("FontStrikethrough") != null)
            {
                caption.FontStrikethrough = (captionXML.Element("FontStrikethrough").Value == true.ToString());
            }
            return caption;
        }

        private static Node DeserializeNode(AddFlow addflow, XElement nodeXML, Node nodeModel)
        {
            double width = Double.Parse(nodeXML.Attribute("Width").Value,
                CultureInfo.InvariantCulture);
            double height = Double.Parse(nodeXML.Attribute("Height").Value,
                CultureInfo.InvariantCulture);
            double left = Double.Parse(nodeXML.Attribute("Left").Value,
                CultureInfo.InvariantCulture);
            double top = Double.Parse(nodeXML.Attribute("Top").Value,
                CultureInfo.InvariantCulture);
            int zorder = int.Parse(nodeXML.Attribute("ZOrder").Value);

            Node node = new Node(left, top, width, height, null, addflow);
            addflow.AddNode(node);
            if (node != null)
            {
                Serial.CopyNodeModelProperties(node, nodeModel);
                Serial.SetZorderSerial(node, zorder);
                if (nodeXML.Element("Text") != null)
                {
                    node.Text = nodeXML.Element("Text").Value;
                }
                if (nodeXML.Element("Tooltip") != null)
                {
                    node.Tooltip = nodeXML.Element("Tooltip").Value;
                }
                if (nodeXML.Element("IsHitTestVisible") != null)
                {
                    node.IsHitTestVisible =
                        (nodeXML.Element("IsHitTestVisible").Value == true.ToString());
                }
                if (nodeXML.Element("IsSelectable") != null)
                {
                    node.IsSelectable =
                        (nodeXML.Element("IsSelectable").Value == true.ToString());
                }
                if (nodeXML.Element("IsXMoveable") != null)
                {
                    node.IsXMoveable =
                        (nodeXML.Element("IsXMoveable").Value == true.ToString());
                }
                if (nodeXML.Element("IsYMoveable") != null)
                {
                    node.IsYMoveable =
                        (nodeXML.Element("IsYMoveable").Value == true.ToString());
                }
                if (nodeXML.Element("IsXSizeable") != null)
                {
                    node.IsXSizeable =
                        (nodeXML.Element("IsXSizeable").Value == true.ToString());
                }
                if (nodeXML.Element("IsYSizeable") != null)
                {
                    node.IsYSizeable =
                        (nodeXML.Element("IsYSizeable").Value == true.ToString());
                }
                if (nodeXML.Element("IsRotatable") != null)
                {
                    node.IsRotatable =
                        (nodeXML.Element("IsRotatable").Value == true.ToString());
                }
                if (nodeXML.Element("IsOutLinkable") != null)
                {
                    node.IsOutLinkable =
                        (nodeXML.Element("IsOutLinkable").Value == true.ToString());
                }
                if (nodeXML.Element("IsInLinkable") != null)
                {
                    node.IsInLinkable =
                        (nodeXML.Element("IsInLinkable").Value == true.ToString());
                }
                if (nodeXML.Element("IsEditable") != null)
                {
                    node.IsEditable =
                        (nodeXML.Element("IsEditable").Value == true.ToString());
                }
                if (nodeXML.Element("Geometry") != null)
                {
                    string s = nodeXML.Element("Geometry").Value;
                    if (s == "System.Windows.Media.RectangleGeometry")
                    {
                        //node.Geometry = Geometry.Parse(s);
                        node.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
                    }
                    else if (s == "System.Windows.Media.EllipseGeometry")
                    {
                        //node.Geometry = Geometry.Parse(s);
                        node.Geometry = new EllipseGeometry(new Rect(0, 0, 3, 3));
                    }
                    else
                    {
                        node.Geometry = Geometry.Parse(s);
                    }
                }
                if (nodeXML.Element("RotationAngle") != null)
                {
                    node.RotationAngle = Double.Parse(
                        nodeXML.Element("RotationAngle").Value, CultureInfo.InvariantCulture);
                }
                if (nodeXML.Element("DashStyle") != null)
                {
                    node.DashStyle = XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("DashStyle").Value))) as DashStyle;
                }
                if (nodeXML.Element("Fill") != null)
                {
                    node.Fill = XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("Fill").Value))) as Brush;
                }
                if (nodeXML.Element("Stroke") != null)
                {
                    node.Stroke = XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("Stroke").Value))) as Brush;
                }
                if (nodeXML.Element("StrokeThickness") != null)
                {
                    node.StrokeThickness = Double.Parse(
                        nodeXML.Element("StrokeThickness").Value, CultureInfo.InvariantCulture);
                }
                if (nodeXML.Element("TextPosition") != null)
                {
                    node.TextPosition = (TextPosition)Enum.Parse(typeof(TextPosition),
                        nodeXML.Element("TextPosition").Value, false);
                }
                if (nodeXML.Element("TextTrimming") != null)
                {
                    node.TextTrimming = (TextTrimming)Enum.Parse(typeof(TextTrimming),
                        nodeXML.Element("TextTrimming").Value, false);
                }
                if (nodeXML.Element("OutConnectionMode") != null)
                {
                    node.OutConnectionMode = (ConnectionMode)Enum.Parse(typeof(ConnectionMode),
                        nodeXML.Element("OutConnectionMode").Value, false);
                }
                if (nodeXML.Element("InConnectionMode") != null)
                {
                    node.InConnectionMode = (ConnectionMode)Enum.Parse(typeof(ConnectionMode),
                        nodeXML.Element("InConnectionMode").Value, false);
                }
                if (nodeXML.Element("ImagePosition") != null)
                {
                    node.ImagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition),
                        nodeXML.Element("ImagePosition").Value, false);
                }
                if (nodeXML.Element("TextMargin") != null)
                {
                    node.TextMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("TextMargin").Value)));
                }
                if (nodeXML.Element("ImageMargin") != null)
                {
                    node.ImageMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("ImageMargin").Value)));
                }

                // Font
                if (nodeXML.Element("FontFamily") != null)
                {
                    node.FontFamily = XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("FontFamily").Value))) as FontFamily;
                }
                if (nodeXML.Element("FontSize") != null)
                {
                    node.FontSize = Double.Parse(
                        nodeXML.Element("FontSize").Value, CultureInfo.InvariantCulture);
                }
                if (nodeXML.Element("Foreground") != null)
                {
                    node.Foreground = XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("Foreground").Value))) as Brush;
                }
                if (nodeXML.Element("FontStyle") != null)
                {
                    node.FontStyle = (FontStyle)XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("FontStyle").Value)));
                }
                if (nodeXML.Element("FontWeight") != null)
                {
                    node.FontWeight = (FontWeight)XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("FontWeight").Value)));
                }
                if (nodeXML.Element("FontStretch") != null)
                {
                    node.FontStretch = (FontStretch)XamlReader.Load(XmlReader.Create(
                        new StringReader(nodeXML.Element("FontStretch").Value)));
                }
                if (nodeXML.Element("FontUnderline") != null)
                {
                    node.FontUnderline = (nodeXML.Element("FontUnderline").Value == true.ToString());
                }
                if (nodeXML.Element("FontStrikethrough") != null)
                {
                    node.FontStrikethrough = (nodeXML.Element("FontStrikethrough").Value == true.ToString());
                }
                if (nodeXML.Element("Image") != null)
                {
                    node.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri((nodeXML.Element("Image").Value)));
                }
            }
            return node;
        }

        private static Link DeserializeLink(AddFlow addflow, XElement linkXML, Link linkModel, List<Node> addedNodes)
        {
            int idOrg = int.Parse(linkXML.Attribute("Org").Value);
            int idDst = int.Parse(linkXML.Attribute("Dst").Value);
            Node org = addedNodes[idOrg];
            Node dst = addedNodes[idDst];
            int zorder = int.Parse(linkXML.Attribute("ZOrder").Value);

            Link link = new Link(org, dst, null, addflow);
            addflow.AddLink(link);
            if (link != null)
            {
                Serial.CopyLinkModelProperties(link, linkModel);
                Serial.SetZorderSerial(link, zorder);
                if (linkXML.Element("Text") != null)
                {
                    link.Text = linkXML.Element("Text").Value;
                }
                if (linkXML.Element("Tooltip") != null)
                {
                    link.Tooltip = linkXML.Element("Tooltip").Value;
                }
                if (linkXML.Element("IsHitTestVisible") != null)
                {
                    link.IsHitTestVisible =
                        (linkXML.Element("IsHitTestVisible").Value == true.ToString());
                }
                if (linkXML.Element("IsSelectable") != null)
                {
                    link.IsSelectable =
                        (linkXML.Element("IsSelectable").Value == true.ToString());
                }
                if (linkXML.Element("IsStretchable") != null)
                {
                    link.IsStretchable =
                        (linkXML.Element("IsStretchable").Value == true.ToString());
                }
                if (linkXML.Element("IsOrientedText") != null)
                {
                    link.IsOrientedText =
                        (linkXML.Element("IsOrientedText").Value == true.ToString());
                }
                if (linkXML.Element("DashStyle") != null)
                {
                    link.DashStyle = XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("DashStyle").Value))) as DashStyle;
                }
                if (linkXML.Element("Stroke") != null)
                {
                    link.Stroke = XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("Stroke").Value))) as Brush;
                }
                if (linkXML.Element("StrokeThickness") != null)
                {
                    link.StrokeThickness = Double.Parse(
                        linkXML.Element("StrokeThickness").Value, CultureInfo.InvariantCulture);
                }
                if (linkXML.Element("RoundedCornerSize") != null)
                {
                    link.RoundedCornerSize =
                        Double.Parse(linkXML.Element("RoundedCornerSize").Value);
                }
                if (linkXML.Element("ArrowGeometryDst") != null)
                {
                    link.ArrowGeometryDst =
                        Geometry.Parse(linkXML.Element("ArrowGeometryDst").Value);
                }
                if (linkXML.Element("ArrowGeometryOrg") != null)
                {
                    link.ArrowGeometryOrg =
                        Geometry.Parse(linkXML.Element("ArrowGeometryOrg").Value);
                }
                if (linkXML.Element("ArrowGeometryMid") != null)
                {
                    link.ArrowGeometryMid =
                        Geometry.Parse(linkXML.Element("ArrowGeometryMid").Value);
                }
                if (linkXML.Element("LineStyle") != null)
                {
                    link.LineStyle = (LineStyle)Enum.Parse(typeof(LineStyle),
                        linkXML.Element("LineStyle").Value, false);
                }
                if (linkXML.Element("TextPlacementMode") != null)
                {
                    link.TextPlacementMode = (TextPlacementMode)Enum.Parse(typeof(TextPlacementMode),
                        linkXML.Element("TextPlacementMode").Value, false);
                }
                if (linkXML.Element("BackMode") != null)
                {
                    link.BackMode = (BackMode)Enum.Parse(typeof(BackMode),
                        linkXML.Element("BackMode").Value, false);
                }

                // Font
                if (linkXML.Element("FontFamily") != null)
                {
                    link.FontFamily = XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("FontFamily").Value))) as FontFamily;
                }
                if (linkXML.Element("FontSize") != null)
                {
                    link.FontSize = Double.Parse(
                        linkXML.Element("FontSize").Value, CultureInfo.InvariantCulture);
                }
                if (linkXML.Element("Foreground") != null)
                {
                    link.Foreground = XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("Foreground").Value))) as Brush;
                }
                if (linkXML.Element("FontStyle") != null)
                {
                    link.FontStyle = (FontStyle)XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("FontStyle").Value)));
                }
                if (linkXML.Element("FontWeight") != null)
                {
                    link.FontWeight = (FontWeight)XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("FontWeight").Value)));
                }
                if (linkXML.Element("FontStretch") != null)
                {
                    link.FontStretch = (FontStretch)XamlReader.Load(XmlReader.Create(
                        new StringReader(linkXML.Element("FontStretch").Value)));
                }
                if (linkXML.Element("FontUnderline") != null)
                {
                    link.FontUnderline = (linkXML.Element("FontUnderline").Value == true.ToString());
                }
                if (linkXML.Element("FontStrikethrough") != null)
                {
                    link.FontStrikethrough = (linkXML.Element("FontStrikethrough").Value == true.ToString());
                }

                // Points
                // If the link model is Bezier, then the link is also Bezier. 
                // Therefore it has been created with 4 points and we delete these points
                // in case the line style is polyline or spline.
                if (link.LineStyle == LineStyle.Polyline || link.LineStyle == LineStyle.Spline)
                {
                    link.ClearPoints();
                }
                if (linkXML.Element("Points") != null)
                {
                    link.Points = PointCollection.Parse(linkXML.Element("Points").Value);
                }
            }
            return link;
        }

        private static Caption DeserializeCaption(AddFlow addflow, XElement captionXML, Caption captionModel)
        {
            int ownerIndex = int.Parse(captionXML.Attribute("Owner").Value);
            double width = Double.Parse(captionXML.Attribute("Width").Value,
                CultureInfo.InvariantCulture);
            double height = Double.Parse(captionXML.Attribute("Height").Value,
                CultureInfo.InvariantCulture);
            double left = Double.Parse(captionXML.Attribute("Left").Value,
                CultureInfo.InvariantCulture);
            double top = Double.Parse(captionXML.Attribute("Top").Value,
                CultureInfo.InvariantCulture);
            int zorder = int.Parse(captionXML.Attribute("ZOrder").Value);

            Caption caption = new Caption(left, top, width, height, null, null, addflow);
            addflow.AddCaption(caption);
            if (caption != null)
            {
                Serial.CopyCaptionModelProperties(caption, captionModel);
                Serial.SetOwnerIndex(caption, ownerIndex);
                Serial.SetZorderSerial(caption, zorder);
                if (captionXML.Element("Text") != null)
                {
                    caption.Text = captionXML.Element("Text").Value;
                }
                if (captionXML.Element("Tooltip") != null)
                {
                    caption.Tooltip = captionXML.Element("Tooltip").Value;
                }
                if (captionXML.Element("AnchorPositionOnLink") != null)
                {
                    caption.AnchorPositionOnLink = Double.Parse(
                        captionXML.Element("AnchorPositionOnLink").Value, CultureInfo.InvariantCulture);
                }
                if (captionXML.Element("Dock") != null)
                {
                    caption.Dock = (DockStyle)Enum.Parse(typeof(DockStyle),
                        captionXML.Element("Dock").Value, false);
                }
                if (captionXML.Element("IsHitTestVisible") != null)
                {
                    caption.IsHitTestVisible =
                        (captionXML.Element("IsHitTestVisible").Value == true.ToString());
                }
                if (captionXML.Element("IsSelectable") != null)
                {
                    caption.IsSelectable =
                        (captionXML.Element("IsSelectable").Value == true.ToString());
                }
                if (captionXML.Element("IsXMoveable") != null)
                {
                    caption.IsXMoveable =
                        (captionXML.Element("IsXMoveable").Value == true.ToString());
                }
                if (captionXML.Element("IsYMoveable") != null)
                {
                    caption.IsYMoveable =
                        (captionXML.Element("IsYMoveable").Value == true.ToString());
                }
                if (captionXML.Element("IsXSizeable") != null)
                {
                    caption.IsXSizeable =
                        (captionXML.Element("IsXSizeable").Value == true.ToString());
                }
                if (captionXML.Element("IsYSizeable") != null)
                {
                    caption.IsYSizeable =
                        (captionXML.Element("IsYSizeable").Value == true.ToString());
                }
                if (captionXML.Element("IsRotatable") != null)
                {
                    caption.IsRotatable =
                        (captionXML.Element("IsRotatable").Value == true.ToString());
                }
                if (captionXML.Element("IsEditable") != null)
                {
                    caption.IsEditable =
                        (captionXML.Element("IsEditable").Value == true.ToString());
                }
                if (captionXML.Element("Geometry") != null)
                {
                    string s = captionXML.Element("Geometry").Value;
                    if (s == "System.Windows.Media.RectangleGeometry")
                    {
                        caption.Geometry = new RectangleGeometry(new Rect(0, 0, 3, 3));
                    }
                    else if (s == "System.Windows.Media.EllipseGeometry")
                    {
                        caption.Geometry = new EllipseGeometry(new Rect(0, 0, 3, 3));
                    }
                    else
                    {
                        caption.Geometry = Geometry.Parse(s);
                    }
                }
                if (captionXML.Element("RotationAngle") != null)
                {
                    caption.RotationAngle = Double.Parse(
                        captionXML.Element("RotationAngle").Value, CultureInfo.InvariantCulture);
                }
                if (captionXML.Element("DashStyle") != null)
                {
                    caption.DashStyle = XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("DashStyle").Value))) as DashStyle;
                }
                if (captionXML.Element("Fill") != null)
                {
                    caption.Fill = XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("Fill").Value))) as Brush;
                }
                if (captionXML.Element("Stroke") != null)
                {
                    caption.Stroke = XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("Stroke").Value))) as Brush;
                }
                if (captionXML.Element("StrokeThickness") != null)
                {
                    caption.StrokeThickness = Double.Parse(
                        captionXML.Element("StrokeThickness").Value, CultureInfo.InvariantCulture);
                }
                if (captionXML.Element("TextPosition") != null)
                {
                    caption.TextPosition = (TextPosition)Enum.Parse(typeof(TextPosition),
                        captionXML.Element("TextPosition").Value, false);
                }
                if (captionXML.Element("TextTrimming") != null)
                {
                    caption.TextTrimming = (TextTrimming)Enum.Parse(typeof(TextTrimming),
                        captionXML.Element("TextTrimming").Value, false);
                }
                if (captionXML.Element("ImagePosition") != null)
                {
                    caption.ImagePosition = (ImagePosition)Enum.Parse(typeof(ImagePosition),
                        captionXML.Element("ImagePosition").Value, false);
                }
                if (captionXML.Element("TextMargin") != null)
                {
                    caption.TextMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("TextMargin").Value)));
                }
                if (captionXML.Element("ImageMargin") != null)
                {
                    caption.ImageMargin = (Thickness)XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("ImageMargin").Value)));
                }

                // Font
                if (captionXML.Element("FontFamily") != null)
                {
                    caption.FontFamily = XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("FontFamily").Value))) as FontFamily;
                }
                if (captionXML.Element("FontSize") != null)
                {
                    caption.FontSize = Double.Parse(
                        captionXML.Element("FontSize").Value, CultureInfo.InvariantCulture);
                }
                if (captionXML.Element("Foreground") != null)
                {
                    caption.Foreground = XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("Foreground").Value))) as Brush;
                }
                if (captionXML.Element("FontStyle") != null)
                {
                    caption.FontStyle = (FontStyle)XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("FontStyle").Value)));
                }
                if (captionXML.Element("FontWeight") != null)
                {
                    caption.FontWeight = (FontWeight)XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("FontWeight").Value)));
                }
                if (captionXML.Element("FontStretch") != null)
                {
                    caption.FontStretch = (FontStretch)XamlReader.Load(XmlReader.Create(
                        new StringReader(captionXML.Element("FontStretch").Value)));
                }
                if (captionXML.Element("FontUnderline") != null)
                {
                    caption.FontUnderline = (captionXML.Element("FontUnderline").Value == true.ToString());
                }
                if (captionXML.Element("FontStrikethrough") != null)
                {
                    caption.FontStrikethrough = (captionXML.Element("FontStrikethrough").Value == true.ToString());
                }
            }
            return caption;
        }

        #endregion

        #region Save

        internal static XElement SaveXMLInt(AddFlow addflow)
        {
            IEnumerable<Node> nodes = addflow.Items.OfType<Node>();
            IEnumerable<Link> links = addflow.Items.OfType<Link>();
            IEnumerable<Caption> captions = addflow.Items.OfType<Caption>();
            XElement versionXML = Serial.GetAddFlowVersion(addflow);
            XElement nodeModelXML = Serial.GetNodeModelXML(addflow.NodeModel);
            XElement linkModelXML = Serial.GetLinkModelXML(addflow.LinkModel);
            XElement captionModelXML = Serial.GetCaptionModelXML(addflow.CaptionModel);
            XElement nodesXML = Serial.GetNodesXML(addflow, addflow.NodeModel, nodes);
            XElement linksXML = Serial.GetLinksXML(addflow, nodes.ToList(), addflow.LinkModel, links);
            XElement captionsXML = Serial.GetCaptionsXML(addflow, addflow.Items, addflow.CaptionModel, captions);
            XAttribute nodesAtt = new XAttribute("Nodes", nodes.Count());
            XAttribute linksAtt = new XAttribute("Links", links.Count());
            XAttribute captionsAtt = new XAttribute("Captions", captions.Count());
            XElement root = new XElement("AddFlow", nodesAtt, linksAtt, captionsAtt);
            root.Add(versionXML);
            root.Add(nodeModelXML);
            root.Add(linkModelXML);
            root.Add(captionModelXML);
            root.Add(nodesXML);
            root.Add(linksXML);
            root.Add(captionsXML);
            return root;
        }

        static XElement GetAddFlowVersion(AddFlow addflow)
        {
            return new XElement("Version", "AddFlow for WPF v2.0");
        }

        static XElement GetNodeModelXML(Node nodeModel)
        {
            return new XElement("NodeModel",
                string.IsNullOrEmpty(nodeModel.Text) ? null :
                    new XElement("Text", nodeModel.Text),
                string.IsNullOrEmpty(nodeModel.Tooltip) ? null :
                    new XElement("Tooltip", nodeModel.Tooltip),
                nodeModel.IsHitTestVisible ? null :
                    new XElement("IsHitTestVisible", nodeModel.IsHitTestVisible.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsSelectable ? null :
                    new XElement("IsSelectable", nodeModel.IsSelectable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsXMoveable ? null :
                    new XElement("IsXMoveable", nodeModel.IsXMoveable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsYMoveable ? null :
                    new XElement("IsYMoveable", nodeModel.IsYMoveable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsXSizeable ? null :
                    new XElement("IsXSizeable", nodeModel.IsXSizeable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsYSizeable ? null :
                    new XElement("IsYSizeable", nodeModel.IsYSizeable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsRotatable ? null :
                    new XElement("IsRotatable", nodeModel.IsRotatable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsOutLinkable ? null :
                    new XElement("IsOutLinkable", nodeModel.IsOutLinkable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsInLinkable ? null :
                    new XElement("IsInLinkable", nodeModel.IsInLinkable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.IsEditable ? null :
                    new XElement("IsEditable", nodeModel.IsEditable.ToString(CultureInfo.InvariantCulture)),
                nodeModel.Geometry == null ? null : new XElement("Geometry",
                    nodeModel.Geometry.ToString(CultureInfo.InvariantCulture)),
                nodeModel.RotationAngle == 0 ? null :
                    new XElement("RotationAngle",
                        nodeModel.RotationAngle.ToString(CultureInfo.InvariantCulture)),
                nodeModel.DashStyle == DashStyles.Solid ? null :
                    new XElement("DashStyle", XamlWriter.Save(nodeModel.DashStyle)),
                nodeModel.Fill.Equals(Brushes.White) ? null :
                    new XElement("Fill", XamlWriter.Save(nodeModel.Fill)),
                nodeModel.Stroke.Equals(Brushes.Black) ? null :
                    new XElement("Stroke", XamlWriter.Save(nodeModel.Stroke)),
                nodeModel.StrokeThickness == 1 ? null :
                    new XElement("StrokeThickness",
                        nodeModel.StrokeThickness.ToString(CultureInfo.InvariantCulture)),
                nodeModel.OutConnectionMode == ConnectionMode.Center ? null :
                    new XElement("OutConnectionMode", nodeModel.OutConnectionMode),
                nodeModel.InConnectionMode == ConnectionMode.Center ? null :
                    new XElement("InConnectionMode", nodeModel.InConnectionMode),
                nodeModel.TextPosition == TextPosition.CenterMiddle ? null :
                    new XElement("TextPosition", nodeModel.TextPosition),
                nodeModel.TextTrimming == TextTrimming.None ? null :
                    new XElement("TextTrimming", nodeModel.TextTrimming),
                nodeModel.ImagePosition == ImagePosition.CenterMiddle ? null :
                    new XElement("ImagePosition", nodeModel.ImagePosition),
                nodeModel.TextMargin == new Thickness(0) ? null :
                    new XElement("TextMargin", XamlWriter.Save(nodeModel.TextMargin)),
                nodeModel.ImageMargin == new Thickness(0) ? null :
                    new XElement("ImageMargin", XamlWriter.Save(nodeModel.ImageMargin)),
                nodeModel.FontFamily.Equals(SystemFonts.MessageFontFamily) ? null :
                    new XElement("FontFamily", XamlWriter.Save(nodeModel.FontFamily)),
                nodeModel.FontSize == SystemFonts.MessageFontSize ? null :
                    new XElement("FontSize", nodeModel.FontSize.ToString(CultureInfo.InvariantCulture)),
                nodeModel.Foreground.Equals(Brushes.Black) ? null :
                    new XElement("Foreground", XamlWriter.Save(nodeModel.Foreground)),
                nodeModel.FontStyle.Equals(SystemFonts.MessageFontStyle) ? null :
                    new XElement("FontStyle", XamlWriter.Save(nodeModel.FontStyle)),
                nodeModel.FontWeight.Equals(SystemFonts.MessageFontWeight) ? null :
                    new XElement("FontWeight", XamlWriter.Save(nodeModel.FontWeight)),
                nodeModel.FontStretch.Equals(FontStretches.Normal) ? null :
                    new XElement("FontStretch", XamlWriter.Save(nodeModel.FontStretch)),
                nodeModel.FontUnderline == false ? null :
                    new XElement("FontUnderline", nodeModel.FontUnderline.ToString(CultureInfo.InvariantCulture)),
                nodeModel.FontStrikethrough == false ? null :
                    new XElement("FontStrikethrough", nodeModel.FontStrikethrough.ToString(CultureInfo.InvariantCulture))
            );
        }

        static XElement GetLinkModelXML(Link linkModel)
        {
            return new XElement("LinkModel",
                string.IsNullOrEmpty(linkModel.Text) ? null :
                    new XElement("Text", linkModel.Text),
                string.IsNullOrEmpty(linkModel.Tooltip) ? null :
                    new XElement("Tooltip", linkModel.Tooltip),
                linkModel.IsHitTestVisible ? null :
                    new XElement("IsHitTestVisible", linkModel.IsHitTestVisible.ToString(CultureInfo.InvariantCulture)),
                linkModel.IsSelectable ? null :
                    new XElement("IsSelectable", linkModel.IsSelectable.ToString(CultureInfo.InvariantCulture)),
                linkModel.IsStretchable ? null :
                    new XElement("IsStretchable", linkModel.IsStretchable.ToString(CultureInfo.InvariantCulture)),
                linkModel.IsOrientedText ? null :
                    new XElement("IsOrientedText", linkModel.IsOrientedText.ToString(CultureInfo.InvariantCulture)),
                linkModel.ArrowGeometryDst == null ? null :
                    new XElement("ArrowGeometryDst",
                        linkModel.ArrowGeometryDst.ToString(CultureInfo.InvariantCulture)),
                linkModel.ArrowGeometryOrg == null ? null :
                    new XElement("ArrowGeometryOrg",
                        linkModel.ArrowGeometryOrg.ToString(CultureInfo.InvariantCulture)),
                linkModel.ArrowGeometryMid == null ? null :
                    new XElement("ArrowGeometryMid",
                        linkModel.ArrowGeometryMid.ToString(CultureInfo.InvariantCulture)),
                linkModel.DashStyle == DashStyles.Solid ? null :
                    new XElement("DashStyle", XamlWriter.Save(linkModel.DashStyle)),
                linkModel.Stroke.Equals(Brushes.Black) ? null :
                    new XElement("Stroke", XamlWriter.Save(linkModel.Stroke)),
                linkModel.RoundedCornerSize == 0 ? null :
                    new XElement("RoundedCornerSize",
                        linkModel.RoundedCornerSize.ToString(CultureInfo.InvariantCulture)),
                linkModel.StrokeThickness == 1 ? null :
                    new XElement("StrokeThickness",
                        linkModel.StrokeThickness.ToString(CultureInfo.InvariantCulture)),
                linkModel.LineStyle == LineStyle.Polyline ? null :
                    new XElement("LineStyle", linkModel.LineStyle),
                linkModel.TextPlacementMode == TextPlacementMode.MiddleLine ? null :
                    new XElement("TextPlacementMode", linkModel.TextPlacementMode),
                linkModel.BackMode == BackMode.Transparent ? null :
                    new XElement("BackMode", linkModel.BackMode),
                linkModel.FontFamily.Equals(SystemFonts.MessageFontFamily) ? null :
                    new XElement("FontFamily", XamlWriter.Save(linkModel.FontFamily)),
                linkModel.FontSize == SystemFonts.MessageFontSize ? null :
                    new XElement("FontSize", linkModel.FontSize.ToString(CultureInfo.InvariantCulture)),
                linkModel.Foreground.Equals(Brushes.Black) ? null :
                    new XElement("Foreground", XamlWriter.Save(linkModel.Foreground)),
                linkModel.FontStyle.Equals(SystemFonts.MessageFontStyle) ? null :
                    new XElement("FontStyle", XamlWriter.Save(linkModel.FontStyle)),
                linkModel.FontWeight.Equals(SystemFonts.MessageFontWeight) ? null :
                    new XElement("FontWeight", XamlWriter.Save(linkModel.FontWeight)),
                linkModel.FontStretch.Equals(FontStretches.Normal) ? null :
                    new XElement("FontStretch", XamlWriter.Save(linkModel.FontStretch)),
                linkModel.FontUnderline == false ? null :
                    new XElement("FontUnderline", linkModel.FontUnderline.ToString(CultureInfo.InvariantCulture)),
                linkModel.FontStrikethrough == false ? null :
                    new XElement("FontStrikethrough", linkModel.FontStrikethrough.ToString(CultureInfo.InvariantCulture))
            );
        }

        static XElement GetCaptionModelXML(Caption captionModel)
        {
            return new XElement("CaptionModel",
                string.IsNullOrEmpty(captionModel.Text) ? null :
                    new XElement("Text", captionModel.Text),
                string.IsNullOrEmpty(captionModel.Tooltip) ? null :
                    new XElement("Tooltip", captionModel.Tooltip),
                captionModel.AnchorPositionOnLink == 0.5f ? null :
                    new XElement("AnchorPositionOnLink",
                        captionModel.AnchorPositionOnLink.ToString(CultureInfo.InvariantCulture)),
                captionModel.Dock == DockStyle.None ? null :
                    new XElement("Dock", captionModel.Dock),
                captionModel.IsHitTestVisible ? null :
                    new XElement("IsHitTestVisible", captionModel.IsHitTestVisible.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsSelectable ? null :
                    new XElement("IsSelectable", captionModel.IsSelectable.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsXMoveable ? null :
                    new XElement("IsXMoveable", captionModel.IsXMoveable.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsYMoveable ? null :
                    new XElement("IsYMoveable", captionModel.IsYMoveable.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsXSizeable ? null :
                    new XElement("IsXSizeable", captionModel.IsXSizeable.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsYSizeable ? null :
                    new XElement("IsYSizeable", captionModel.IsYSizeable.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsRotatable ? null :
                    new XElement("IsRotatable", captionModel.IsRotatable.ToString(CultureInfo.InvariantCulture)),
                captionModel.IsEditable ? null :
                    new XElement("IsEditable", captionModel.IsEditable.ToString(CultureInfo.InvariantCulture)),
                captionModel.Geometry == null ? null : new XElement("Geometry",
                    captionModel.Geometry.ToString(CultureInfo.InvariantCulture)),
                captionModel.RotationAngle == 0 ? null :
                    new XElement("RotationAngle",
                        captionModel.RotationAngle.ToString(CultureInfo.InvariantCulture)),
                captionModel.DashStyle == DashStyles.Solid ? null :
                    new XElement("DashStyle", XamlWriter.Save(captionModel.DashStyle)),
                captionModel.Fill.Equals(Brushes.White) ? null :
                    new XElement("Fill", XamlWriter.Save(captionModel.Fill)),
                captionModel.Stroke.Equals(Brushes.Black) ? null :
                    new XElement("Stroke", XamlWriter.Save(captionModel.Stroke)),
                captionModel.StrokeThickness == 1 ? null :
                    new XElement("StrokeThickness",
                        captionModel.StrokeThickness.ToString(CultureInfo.InvariantCulture)),
                captionModel.TextPosition == TextPosition.CenterMiddle ? null :
                    new XElement("TextPosition", captionModel.TextPosition),
                captionModel.TextTrimming == TextTrimming.None ? null :
                    new XElement("TextTrimming", captionModel.TextTrimming),
                captionModel.ImagePosition == ImagePosition.CenterMiddle ? null :
                    new XElement("ImagePosition", captionModel.ImagePosition),
                captionModel.TextMargin == new Thickness(0) ? null :
                    new XElement("TextMargin", XamlWriter.Save(captionModel.TextMargin)),
                captionModel.ImageMargin == new Thickness(0) ? null :
                    new XElement("ImageMargin", XamlWriter.Save(captionModel.ImageMargin)),
                captionModel.FontFamily.Equals(SystemFonts.MessageFontFamily) ? null :
                    new XElement("FontFamily", XamlWriter.Save(captionModel.FontFamily)),
                captionModel.FontSize == SystemFonts.MessageFontSize ? null :
                    new XElement("FontSize", captionModel.FontSize.ToString(CultureInfo.InvariantCulture)),
                captionModel.Foreground.Equals(Brushes.Black) ? null :
                    new XElement("Foreground", XamlWriter.Save(captionModel.Foreground)),
                captionModel.FontStyle.Equals(SystemFonts.MessageFontStyle) ? null :
                    new XElement("FontStyle", XamlWriter.Save(captionModel.FontStyle)),
                captionModel.FontWeight.Equals(SystemFonts.MessageFontWeight) ? null :
                    new XElement("FontWeight", XamlWriter.Save(captionModel.FontWeight)),
                captionModel.FontStretch.Equals(FontStretches.Normal) ? null :
                    new XElement("FontStretch", XamlWriter.Save(captionModel.FontStretch)),
                captionModel.FontUnderline == false ? null :
                    new XElement("FontUnderline", captionModel.FontUnderline.ToString(CultureInfo.InvariantCulture)),
                captionModel.FontStrikethrough == false ? null :
                    new XElement("FontStrikethrough", captionModel.FontStrikethrough.ToString(CultureInfo.InvariantCulture))
            );
        }

        static XElement GetNodesXML(AddFlow addflow, Node nodeModel, IEnumerable<Node> nodes)
        {
            Geometry geometryModel = nodeModel.Geometry;
            return new XElement("Nodes",
                from node in nodes
                let strFill = XamlWriter.Save(node.Fill)
                let strStroke = XamlWriter.Save(node.Stroke)
                let strDashStyle = XamlWriter.Save(node.DashStyle)
                let strTextMargin = XamlWriter.Save(node.TextMargin)
                let strImageMargin = XamlWriter.Save(node.ImageMargin)
                let text = node.Text
                let tooltip = node.Tooltip
                let geometry = node.Geometry
                select new XElement("Node",
                    new XAttribute("Left", node.Location.X),
                    new XAttribute("Top", node.Location.Y),
                    new XAttribute("Width", node.Size.Width),
                    new XAttribute("Height", node.Size.Height),
                    new XAttribute("ZOrder", addflow.Items.IndexOf(node)),
                    string.IsNullOrEmpty(text) ? null :
                        new XElement("Text", text),
                    string.IsNullOrEmpty(tooltip) ? null :
                        new XElement("Tooltip", tooltip),
                    node.IsHitTestVisible == nodeModel.IsHitTestVisible ? null :
                        new XElement("IsHitTestVisible", node.IsHitTestVisible.ToString(CultureInfo.InvariantCulture)),
                    node.IsSelectable == nodeModel.IsSelectable ? null :
                        new XElement("IsSelectable", node.IsSelectable.ToString(CultureInfo.InvariantCulture)),
                    node.IsXMoveable == nodeModel.IsXMoveable ? null :
                        new XElement("IsXMoveable", node.IsXMoveable.ToString(CultureInfo.InvariantCulture)),
                    node.IsYMoveable == nodeModel.IsYMoveable ? null :
                        new XElement("IsYMoveable", node.IsYMoveable.ToString(CultureInfo.InvariantCulture)),
                    node.IsXSizeable == nodeModel.IsXSizeable ? null :
                        new XElement("IsXSizeable", node.IsXSizeable.ToString(CultureInfo.InvariantCulture)),
                    node.IsYSizeable == nodeModel.IsYSizeable ? null :
                        new XElement("IsYSizeable", node.IsYSizeable.ToString(CultureInfo.InvariantCulture)),
                    node.IsRotatable == nodeModel.IsRotatable ? null :
                        new XElement("IsRotatable", node.IsRotatable.ToString(CultureInfo.InvariantCulture)),
                    node.IsOutLinkable == nodeModel.IsOutLinkable ? null :
                        new XElement("IsOutLinkable", node.IsOutLinkable.ToString(CultureInfo.InvariantCulture)),
                    node.IsInLinkable == nodeModel.IsInLinkable ? null :
                        new XElement("IsInLinkable", node.IsInLinkable.ToString(CultureInfo.InvariantCulture)),
                    node.IsEditable == nodeModel.IsEditable ? null :
                        new XElement("IsEditable", node.IsEditable.ToString(CultureInfo.InvariantCulture)),
                    geometry == null || (geometryModel != null &&
                        geometry.ToString() == geometryModel.ToString()) ?
                        null : new XElement("Geometry", geometry.ToString(CultureInfo.InvariantCulture)),
                    node.RotationAngle == nodeModel.RotationAngle ? null :
                        new XElement("RotationAngle",
                            node.RotationAngle.ToString(CultureInfo.InvariantCulture)),
                    node.DashStyle == DashStyles.Solid ? null :
                        new XElement("DashStyle", strDashStyle),
                    node.Fill.Equals(nodeModel.Fill) ? null :
                        new XElement("Fill", strFill),
                    node.Stroke.Equals(nodeModel.Stroke) ? null :
                        new XElement("Stroke", strStroke),
                    node.StrokeThickness == nodeModel.StrokeThickness ? null :
                        new XElement("StrokeThickness",
                            node.StrokeThickness.ToString(CultureInfo.InvariantCulture)),
                    node.OutConnectionMode == nodeModel.OutConnectionMode ? null :
                        new XElement("OutConnectionMode", node.OutConnectionMode),
                    node.InConnectionMode == nodeModel.InConnectionMode ? null :
                        new XElement("InConnectionMode", node.InConnectionMode),
                    node.TextPosition == nodeModel.TextPosition ? null :
                        new XElement("TextPosition", node.TextPosition),
                    node.TextTrimming == nodeModel.TextTrimming ? null :
                        new XElement("TextTrimming", node.TextTrimming),
                    node.ImagePosition == nodeModel.ImagePosition ? null :
                        new XElement("ImagePosition", node.ImagePosition),
                    node.TextMargin == nodeModel.TextMargin ? null :
                        new XElement("TextMargin", strTextMargin),
                    node.ImageMargin == nodeModel.ImageMargin ? null :
                        new XElement("ImageMargin", strImageMargin),
                    node.FontFamily == nodeModel.FontFamily ? null :
                        new XElement("FontFamily", XamlWriter.Save(node.FontFamily)), 
                    node.FontSize == nodeModel.FontSize? null :
                        new XElement("FontSize", node.FontSize.ToString(CultureInfo.InvariantCulture)),
                    node.Foreground.Equals(nodeModel.Foreground) ? null :
                        new XElement("Foreground", XamlWriter.Save(node.Foreground)),
                    node.FontStyle.Equals(nodeModel.FontStyle) ? null :
                        new XElement("FontStyle", XamlWriter.Save(node.FontStyle)),
                    node.FontWeight.Equals(nodeModel.FontWeight) ? null :
                        new XElement("FontWeight", XamlWriter.Save(node.FontWeight)),
                    node.FontStretch.Equals(nodeModel.FontStretch) ? null :
                        new XElement("FontStretch", XamlWriter.Save(node.FontStretch)),
                    node.FontUnderline.Equals(nodeModel.FontUnderline) ? null :
                        new XElement("FontUnderline", node.FontUnderline.ToString(CultureInfo.InvariantCulture)),
                    node.FontStrikethrough.Equals(nodeModel.FontStrikethrough) ? null :
                        new XElement("FontStrikethrough", node.FontStrikethrough.ToString(CultureInfo.InvariantCulture)),
                    node.Image == null ? null :
                        new XElement("Image", node.Image.ToString(CultureInfo.InvariantCulture))
                    )
            );
        }

        static XElement GetLinksXML(AddFlow addflow, List<Node> listnodes, Link linkModel, IEnumerable<Link> links)
        {
            return new XElement("Links",
                from link in links
                let strPoints = link.Points.ToString(CultureInfo.InvariantCulture)
                let strStroke = XamlWriter.Save(link.Stroke)
                let strDashStyle = XamlWriter.Save(link.DashStyle)
                let arrowGeometryDst = link.ArrowGeometryDst
                let arrowGeometryOrg = link.ArrowGeometryOrg
                let arrowGeometryMid = link.ArrowGeometryMid
                select new XElement("Link",
                    new XAttribute("Org", listnodes.IndexOf(link.Org)),
                    new XAttribute("Dst", listnodes.IndexOf(link.Dst)),
                    new XAttribute("ZOrder", addflow.Items.IndexOf(link)),
                        string.IsNullOrEmpty(link.Text) ? null :
                    new XElement("Text", link.Text),
                        string.IsNullOrEmpty(link.Tooltip) ? null :
                    new XElement("Tooltip", link.Tooltip),
                    link.IsHitTestVisible == linkModel.IsHitTestVisible ? null :
                        new XElement("IsHitTestVisible", link.IsHitTestVisible.ToString(CultureInfo.InvariantCulture)),
                    link.IsSelectable == linkModel.IsSelectable ? null :
                        new XElement("IsSelectable", link.IsSelectable.ToString(CultureInfo.InvariantCulture)),
                    link.IsStretchable == linkModel.IsStretchable ? null :
                        new XElement("IsStretchable", link.IsStretchable.ToString(CultureInfo.InvariantCulture)),
                    link.IsOrientedText == linkModel.IsOrientedText ? null :
                        new XElement("IsOrientedText", link.IsOrientedText.ToString(CultureInfo.InvariantCulture)),
                    arrowGeometryDst == null || (linkModel.ArrowGeometryDst != null &&
                    arrowGeometryDst.ToString() == linkModel.ArrowGeometryDst.ToString()) ?
                    null : new XElement("ArrowGeometryDst", arrowGeometryDst == null ?
                        null : arrowGeometryDst.ToString(CultureInfo.InvariantCulture)),
                    arrowGeometryOrg == null || (linkModel.ArrowGeometryOrg != null &&
                    arrowGeometryOrg.ToString() == linkModel.ArrowGeometryOrg.ToString()) ?
                    null : new XElement("ArrowGeometryOrg", arrowGeometryOrg == null ?
                        null : arrowGeometryOrg.ToString(CultureInfo.InvariantCulture)),
                    arrowGeometryMid == null || (linkModel.ArrowGeometryMid != null &&
                    arrowGeometryMid.ToString() == linkModel.ArrowGeometryMid.ToString()) ?
                    null : new XElement("ArrowGeometryMid", arrowGeometryMid == null ?
                        null : arrowGeometryMid.ToString(CultureInfo.InvariantCulture)),
                    link.DashStyle == DashStyles.Solid ? null :
                        new XElement("DashStyle", strDashStyle),
                    link.Stroke.Equals(linkModel.Stroke) ? null :
                        new XElement("Stroke", strStroke),
                    link.StrokeThickness == linkModel.StrokeThickness ? null :
                        new XElement("StrokeThickness",
                            link.StrokeThickness.ToString(CultureInfo.InvariantCulture)),
                    link.RoundedCornerSize == linkModel.RoundedCornerSize ? null :
                        new XElement("RoundedCornerSize",
                            link.RoundedCornerSize.ToString(CultureInfo.InvariantCulture)),
                    link.LineStyle == linkModel.LineStyle ? null :
                        new XElement("LineStyle", link.LineStyle),
                    link.TextPlacementMode == linkModel.TextPlacementMode ? null :
                        new XElement("TextPlacementMode", link.TextPlacementMode),
                    link.BackMode == linkModel.BackMode ? null :
                        new XElement("BackMode", link.BackMode),
                    link.FontFamily == linkModel.FontFamily ? null :
                        new XElement("FontFamily", XamlWriter.Save(link.FontFamily)),
                    link.FontSize == linkModel.FontSize? null :
                        new XElement("FontSize", link.FontSize.ToString(CultureInfo.InvariantCulture)),
                    link.Foreground.Equals(linkModel.Foreground) ? null :
                        new XElement("Foreground", XamlWriter.Save(link.Foreground)),
                    link.FontStyle.Equals(linkModel.FontStyle) ? null :
                        new XElement("FontStyle", XamlWriter.Save(link.FontStyle)),
                    link.FontWeight.Equals(linkModel.FontWeight) ? null :
                        new XElement("FontWeight", XamlWriter.Save(link.FontWeight)),
                    link.FontStretch.Equals(linkModel.FontStretch) ? null :
                        new XElement("FontStretch", XamlWriter.Save(link.FontStretch)),
                    link.FontUnderline.Equals(linkModel.FontUnderline) ? null :
                        new XElement("FontUnderline", link.FontUnderline.ToString(CultureInfo.InvariantCulture)),
                    link.FontStrikethrough.Equals(linkModel.FontStrikethrough) ? null :
                        new XElement("FontStrikethrough", link.FontStrikethrough.ToString(CultureInfo.InvariantCulture)),
                    link.Points.Count == 2 &&
                        link.Org.OutConnectionMode == ConnectionMode.Center &&
                        link.Dst.InConnectionMode == ConnectionMode.Center ? null :
                        new XElement("Points", strPoints)
                    )
            );
        }

        static XElement GetCaptionsXML(AddFlow addflow, List<Item> items, Caption captionModel, IEnumerable<Caption> captions)
        {
            Geometry geometryModel = captionModel.Geometry;
            return new XElement("Captions",
                from caption in captions
                let strFill = XamlWriter.Save(caption.Fill)
                let strStroke = XamlWriter.Save(caption.Stroke)
                let strDashStyle = XamlWriter.Save(caption.DashStyle)
                let strTextMargin = XamlWriter.Save(caption.TextMargin)
                let strImageMargin = XamlWriter.Save(caption.ImageMargin)
                let text = caption.Text
                let tooltip = caption.Tooltip
                let geometry = caption.Geometry
                select new XElement("Caption",
                    new XAttribute("Owner", addflow.Items.IndexOf(caption.Owner)),
                    new XAttribute("Left", caption.Location.X),
                    new XAttribute("Top", caption.Location.Y),
                    new XAttribute("Width", caption.Size.Width),
                    new XAttribute("Height", caption.Size.Height),
                    new XAttribute("ZOrder", addflow.Items.IndexOf(caption)),
                    string.IsNullOrEmpty(text) ? null :
                        new XElement("Text", text),
                    string.IsNullOrEmpty(tooltip) ? null :
                        new XElement("Tooltip", tooltip),
                    caption.AnchorPositionOnLink == captionModel.AnchorPositionOnLink || !(caption.Owner is Link) ? null :
                        new XElement("AnchorPositionOnLink",
                            caption.AnchorPositionOnLink.ToString(CultureInfo.InvariantCulture)),
                    caption.Dock == captionModel.Dock ? null :
                        new XElement("Dock", caption.Dock),
                    caption.IsHitTestVisible == captionModel.IsHitTestVisible ? null :
                        new XElement("IsHitTestVisible", caption.IsHitTestVisible.ToString(CultureInfo.InvariantCulture)),
                    caption.IsSelectable == captionModel.IsSelectable ? null :
                        new XElement("IsSelectable", caption.IsSelectable.ToString(CultureInfo.InvariantCulture)),
                    caption.IsXMoveable == captionModel.IsXMoveable ? null :
                        new XElement("IsXMoveable", caption.IsXMoveable.ToString(CultureInfo.InvariantCulture)),
                    caption.IsYMoveable == captionModel.IsYMoveable ? null :
                        new XElement("IsYMoveable", caption.IsYMoveable.ToString(CultureInfo.InvariantCulture)),
                    caption.IsXSizeable == captionModel.IsXSizeable ? null :
                        new XElement("IsXSizeable", caption.IsXSizeable.ToString(CultureInfo.InvariantCulture)),
                    caption.IsYSizeable == captionModel.IsYSizeable ? null :
                        new XElement("IsYSizeable", caption.IsYSizeable.ToString(CultureInfo.InvariantCulture)),
                    caption.IsRotatable == captionModel.IsRotatable ? null :
                        new XElement("IsRotatable", caption.IsRotatable.ToString(CultureInfo.InvariantCulture)),
                    caption.IsEditable == captionModel.IsEditable ? null :
                        new XElement("IsEditable", caption.IsEditable.ToString(CultureInfo.InvariantCulture)),
                    geometry == null || (geometryModel != null &&
                        geometry.ToString() == geometryModel.ToString()) ?
                        null : new XElement("Geometry", geometry.ToString(CultureInfo.InvariantCulture)),
                    caption.RotationAngle == captionModel.RotationAngle ? null :
                        new XElement("RotationAngle",
                            caption.RotationAngle.ToString(CultureInfo.InvariantCulture)),
                    caption.DashStyle == DashStyles.Solid ? null :
                        new XElement("DashStyle", strDashStyle),
                    caption.Fill.Equals(captionModel.Fill) ? null :
                        new XElement("Fill", strFill),
                    caption.Stroke.Equals(captionModel.Stroke) ? null :
                        new XElement("Stroke", strStroke),
                    caption.StrokeThickness == captionModel.StrokeThickness ? null :
                        new XElement("StrokeThickness",
                            caption.StrokeThickness.ToString(CultureInfo.InvariantCulture)),
                    caption.TextPosition == captionModel.TextPosition ? null :
                        new XElement("TextPosition", caption.TextPosition),
                    caption.TextTrimming == captionModel.TextTrimming ? null :
                        new XElement("TextTrimming", caption.TextTrimming),
                    caption.ImagePosition == captionModel.ImagePosition ? null :
                        new XElement("ImagePosition", caption.ImagePosition),
                    caption.TextMargin == captionModel.TextMargin ? null :
                        new XElement("TextMargin", strTextMargin),
                    caption.ImageMargin == captionModel.ImageMargin ? null :
                        new XElement("ImageMargin", strImageMargin),
                    caption.FontFamily == captionModel.FontFamily ? null :
                        new XElement("FontFamily", XamlWriter.Save(caption.FontFamily)),
                    caption.FontSize == captionModel.FontSize ? null :
                        new XElement("FontSize", caption.FontSize.ToString(CultureInfo.InvariantCulture)),
                    caption.Foreground.Equals(captionModel.Foreground) ? null :
                        new XElement("Foreground", XamlWriter.Save(caption.Foreground)),
                    caption.FontStyle.Equals(captionModel.FontStyle) ? null :
                        new XElement("FontStyle", XamlWriter.Save(caption.FontStyle)),
                    caption.FontWeight.Equals(captionModel.FontWeight) ? null :
                        new XElement("FontWeight", XamlWriter.Save(caption.FontWeight)),
                    caption.FontStretch.Equals(captionModel.FontStretch) ? null :
                        new XElement("FontStretch", XamlWriter.Save(caption.FontStretch)),
                    caption.FontUnderline.Equals(captionModel.FontUnderline) ? null :
                        new XElement("FontUnderline", caption.FontUnderline.ToString(CultureInfo.InvariantCulture)),
                    caption.FontStrikethrough.Equals(captionModel.FontStrikethrough) ? null :
                        new XElement("FontStrikethrough", caption.FontStrikethrough.ToString(CultureInfo.InvariantCulture))
                    )
            );
        }

        #endregion

        #region Misc

        private static XElement LoadSerializedDataFromClipBoard()
        {
            if (Clipboard.ContainsData(DataFormats.Text))
            {
                String clipboardData = Clipboard.GetData(DataFormats.Text) as String;
                if (String.IsNullOrEmpty(clipboardData))
                    return null;
                try
                {
                    return XElement.Load(new StringReader(clipboardData));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        // Assign the property values of a node2 to node1 
        // The location and size are not copied.
        internal static void CopyNodeModelProperties(Node node1, Node node2)
        {
            if (node1 == null || node2 == null)
            {
                return;
            }
            node1.Geometry = node2.Geometry != null ? node2.Geometry.Clone() : null;
            node1.Fill = node2.Fill;
            node1.Stroke = node2.Stroke;
            node1.StrokeThickness = node2.StrokeThickness;
            node1.DashStyle = node2.DashStyle;
            node1.Foreground = node2.Foreground;
            node1.FontFamily = node2.FontFamily;
            node1.FontSize = node2.FontSize;
            node1.FontStyle = node2.FontStyle;
            node1.FontStretch = node2.FontStretch;
            node1.FontWeight = node2.FontWeight;
            node1.FontUnderline = node2.FontUnderline;
            node1.FontStrikethrough = node2.FontStrikethrough;
            node1.TextTrimming = node2.TextTrimming;
            node1.TextPosition = node2.TextPosition;
            node1.ImagePosition = node2.ImagePosition;
            node1.ImageMargin = node2.ImageMargin;
            node1.TextMargin = node2.TextMargin;
            node1.Text = node2.Text;
            node1.Tooltip = node2.Tooltip;
            node1.Image = node2.Image;
            node1.IsEditable = node2.IsEditable;
            node1.IsXMoveable = node2.IsXMoveable;
            node1.IsYMoveable = node2.IsYMoveable;
            node1.IsXSizeable = node2.IsXSizeable;
            node1.IsYSizeable = node2.IsYSizeable;
            node1.IsInLinkable = node2.IsInLinkable;
            node1.IsOutLinkable = node2.IsOutLinkable;
            node1.IsSelectable = node2.IsSelectable;
            node1.InConnectionMode = node2.InConnectionMode;
            node1.OutConnectionMode = node2.OutConnectionMode;
        }

        // Assign the property values of a caption2 to caption1
        // The location and size are not copied.
        internal static void CopyCaptionModelProperties(Caption caption1, Caption caption2)
        {
            if (caption1 == null || caption2 == null)
            {
                return;
            }
            caption1.Geometry = caption2.Geometry != null ? caption2.Geometry.Clone() : null;
            caption1.Fill = caption2.Fill;
            caption1.Stroke = caption2.Stroke;
            caption1.StrokeThickness = caption2.StrokeThickness;
            caption1.DashStyle = caption2.DashStyle;
            caption1.Foreground = caption2.Foreground;
            caption1.FontFamily = caption2.FontFamily;
            caption1.FontSize = caption2.FontSize;
            caption1.FontStyle = caption2.FontStyle;
            caption1.FontStretch = caption2.FontStretch;
            caption1.FontWeight = caption2.FontWeight;
            caption1.FontUnderline = caption2.FontUnderline;
            caption1.FontStrikethrough = caption2.FontStrikethrough;
            caption1.TextTrimming = caption2.TextTrimming;
            caption1.TextPosition = caption2.TextPosition;
            caption1.ImagePosition = caption2.ImagePosition;
            caption1.ImageMargin = caption2.ImageMargin;
            caption1.TextMargin = caption2.TextMargin;
            caption1.Text = caption2.Text;
            caption1.Tooltip = caption2.Tooltip;
            caption1.Image = caption2.Image;
            caption1.IsEditable = caption2.IsEditable;
            caption1.IsXMoveable = caption2.IsXMoveable;
            caption1.IsYMoveable = caption2.IsYMoveable;
            caption1.IsXSizeable = caption2.IsXSizeable;
            caption1.IsYSizeable = caption2.IsYSizeable;
            caption1.IsSelectable = caption2.IsSelectable;
        }

        // Assign the property values of link2 to link1
        // The collection of points is not copied.
        internal static void CopyLinkModelProperties(Link link1, Link link2)
        {
            if (link1 == null || link2 == null)
            {
                return;
            }
            link1.LineStyle = link2.LineStyle;
            link1.TextPlacementMode = link2.TextPlacementMode;
            link1.Stroke = link2.Stroke;
            link1.StrokeThickness = link2.StrokeThickness;
            link1.DashStyle = link2.DashStyle;
            link1.Foreground = link2.Foreground;
            link1.FontFamily = link2.FontFamily;
            link1.FontSize = link2.FontSize;
            link1.FontStyle = link2.FontStyle;
            link1.FontStretch = link2.FontStretch;
            link1.FontWeight = link2.FontWeight;
            //link1.JumpSize = link2.JumpSize;
            link1.RoundedCornerSize = link2.RoundedCornerSize;
            link1.Text = link2.Text;
            link1.Tooltip = link2.Tooltip;
            link1.IsStretchable = link2.IsStretchable;
            link1.IsSelectable = link2.IsSelectable;
            link1.ArrowGeometryDst = link2.ArrowGeometryDst != null ? link2.ArrowGeometryDst.Clone() : null;
            link1.ArrowGeometryOrg = link2.ArrowGeometryOrg != null ? link2.ArrowGeometryOrg.Clone() : null;
            link1.ArrowGeometryMid = link2.ArrowGeometryMid != null ? link2.ArrowGeometryMid.Clone() : null;
        }

        #endregion

        #endregion
    }
}
