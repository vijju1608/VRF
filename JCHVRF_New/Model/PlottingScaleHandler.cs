using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using JCHVRF.Model;
using JCHVRF_New.Common.Helpers;
using Lassalle.WPF.Flow;

namespace JCHVRF_New.Model
{
    /// <summary>
    /// Class for Adding and handling PlottingScale Node on a AddFlow Control
    /// </summary>
    public class PlottingScaleHandler
    {
        #region Private members

        private readonly AddFlow addFlow;
        private bool isPlottingScaleEnabled;
        private Node horizontalPlottingNode;
        private Node verticalPlottingNode;
        private Color plottingScaleColorToUse;
        private int meterValue;
       

        #endregion

        #region Private Property

        private string UserUnitSetting
        {
            get
            {
                if (string.IsNullOrEmpty(SystemSetting.UserSetting.unitsSetting.settingLENGTH))
                {
                    return "m";
                }

                return SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            }
        }

        private string PlotScaleText
        {
            get { return meterValue.ToString() + " " + UserUnitSetting; }
        }



        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for PlottingScaleHandler
        /// </summary>
        public PlottingScaleHandler(AddFlow addFlowControl)
        {
            addFlow = addFlowControl;
            meterValue = 0;
            plottingScaleColorToUse = Colors.Black;
        }

        #endregion

        #region AddFlowEvents

        private void AddFlow_SelectedItemsLayouting(object sender, SelectedItemsLayoutingEventArgs e)
        {
            if (sender == null) { return; }

            var addFlowControl = sender as AddFlow;

            if (addFlowControl == null) { return; }

            var nodes = addFlowControl.SelectedItems.OfType<Node>();

            if (!nodes.Any()) { return; }

            RedDrawPlottingScale(nodes);
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Adds Plotting Scale Node to Add Flow based on isVertical
        /// </summary>
        public void AddPlottingScaleToAddFlow(bool isVertical)
        {
            if (isVertical)
            {
                AddVerticalPlottingNodeToAddFlow();
            }
            else
            {
                AddHorizontalPlottingNodeToAddFlow();
            }
        }

        /// <summary>
        /// Adds/Removes the Plotting Scale
        /// </summary>
        /// <param name="enable"></param>
        public void EnableOrDisablePlottingScale(bool enable)
        {
            isPlottingScaleEnabled = enable;

            if (isPlottingScaleEnabled)
            {
                addFlow.SelectedItemsLayouting += AddFlow_SelectedItemsLayouting;
                AddPlottingScaleToAddFlow(false);
            }
            else
            {
                addFlow.SelectedItemsLayouting -= AddFlow_SelectedItemsLayouting;
                DeleteNodeIfExist(horizontalPlottingNode);
                DeleteNodeIfExist(verticalPlottingNode);
                horizontalPlottingNode = null;
                verticalPlottingNode = null;
            }
        }

        /// <summary>
        /// Updates the Meter value text on Plotting Node
        /// </summary>
        public void UpdatePlottingScaleMeterValue(int value)
        {
            meterValue = value;
            UpdateNodeScalingMeterText();
        }

        /// <summary>
        /// Apllies Color Changes to AddFlow Nodes
        /// </summary>
        public void ApplyColorChanges(CanvasColorPickerChanges canvasColorPicker)
        {
            Action<Color> colorUpdateAction = null;
            var colorToApply = canvasColorPicker.ColorToApply;

            var defaultColor = Colors.Black;
            Color color = colorToApply == null ? defaultColor : colorToApply.Value;
            switch (canvasColorPicker.PropertyEnum)
            {
                case CanvasColorPropertyEnum.PlottingScale:
                    colorUpdateAction = UpdatePlottingScaleColor;
                    break;
                case CanvasColorPropertyEnum.NodeText:
                    colorUpdateAction = UpdateAddFlowNodesTextColor;
                    break;
                case CanvasColorPropertyEnum.NodeBackground:
                    
                    color = colorToApply == null ? Colors.White : colorToApply.Value;
                    colorUpdateAction = UpdateAddFlowNodesBackgroundColor;
                    break;
                case CanvasColorPropertyEnum.PipeColor:
                    color = colorToApply == null ? Colors.White : colorToApply.Value;
                    colorUpdateAction = UpdateAddFlowPipesColor;
                    break;
                case CanvasColorPropertyEnum.BranchKit:
                    color = colorToApply == null ? Colors.Blue : colorToApply.Value;
                    colorUpdateAction = UpdateAddFlowBranchKitColor;
                    break;
            }

            colorUpdateAction?.Invoke(color);
        }

        #endregion

        #region Private  Methods

        private void RedDrawPlottingScale(IEnumerable<Node> addFlowNodes)
        {
            if (addFlowNodes == null || !addFlowNodes.Any()) { return; }


            Action<DrawingVisual, Node> drawHorizontalPlottingScaleAction = DrawHorizontalPlottingScale;
            Action<DrawingVisual, Node> drawVerticalPlottingScaleAction = DrawVerticalPlottingScale;

            foreach (var addFlowNode in addFlowNodes)
            {
                if (addFlowNode == horizontalPlottingNode)
                {
                    var dv = horizontalPlottingNode.Visual.Children[0] as DrawingVisual;
                    drawHorizontalPlottingScaleAction(dv, horizontalPlottingNode);
                }
                else if (addFlowNode == verticalPlottingNode)
                {
                    var dv = verticalPlottingNode.Visual.Children[0] as DrawingVisual;
                    drawVerticalPlottingScaleAction(dv, verticalPlottingNode);
                }
            }
        }

        private void UpdatePlottingScaleColor(Color selectedColor)
        {
            plottingScaleColorToUse = selectedColor;
            RedDrawPlottingScale(addFlow.Items.OfType<Node>().ToArray());
        }

        private void UpdateAddFlowNodesTextColor(Color color)
        {
            var addFlowNodes = addFlow.Items.OfType<Node>();
            foreach (var node in addFlowNodes)
            {
                if (node is JCHVRF.Model.NextGen.JCHNode && ((JCHVRF.Model.NextGen.JCHNode)node).IsLiquidGasPipeNode.Equals(true)) { continue; }

                if (node == horizontalPlottingNode || node == verticalPlottingNode || node is JCHVRF.Model.NextGen.MyNodeLegend) { continue; }
                node.Foreground = new SolidColorBrush(color);
            }
        }

        private void UpdateAddFlowNodesBackgroundColor(Color color)
        {
            var addFlowNodes = addFlow.Items.OfType<Node>();
            foreach (var node in addFlowNodes)
            {
                if (node == horizontalPlottingNode || node == verticalPlottingNode) { continue; }

                var jchNode = node as JCHVRF.Model.NextGen.JCHNode;
                if (jchNode.ImageData == null || jchNode.ImageData.equipmentType == "Pipe"
                                              || jchNode is JCHVRF.Model.NextGen.MyNodeYP
                                              || jchNode is JCHVRF.Model.NextGen.MyNodeLegend)
                {
                    continue;
                }

                node.Fill = new SolidColorBrush(color);
            }
        }

        private void UpdateAddFlowPipesColor(Color color)
        {
            var addFlowNodes = addFlow.Items.OfType<Link>();
            foreach (var addFlowNode in addFlowNodes)
            {
                addFlowNode.Stroke = new SolidColorBrush(color);
            }
        }

        private void UpdateAddFlowBranchKitColor(Color color)
        {
            var addFlowNodes = addFlow.Items.OfType<Node>();
            foreach (var addFlowNode in addFlowNodes)
            {
                if (addFlowNode is JCHVRF.Model.NextGen.MyNodeYP)
                {
                    addFlowNode.Fill = new SolidColorBrush(color);
                }
            }
        }

        private void DeleteNodeIfExist(Node plottingNode)
        {
            if (plottingNode == null)
            {
                return;
            }

            if (addFlow.Items.Contains(plottingNode))
            {
                addFlow.RemoveNode(plottingNode);
            }
        }

        private void DrawHorizontalPlottingScale(DrawingVisual dv, Node node)
        {
            if (dv != null)
            {

                var dc = dv.RenderOpen();
                Brush brush = new SolidColorBrush(plottingScaleColorToUse);
                horizontalPlottingNode.Foreground = new SolidColorBrush(plottingScaleColorToUse);
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

                FormattedText scaleTextFormat = new FormattedText(PlotScaleText,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface("TimeNewRoman"), 14, brush);

                dc.DrawText(scaleTextFormat, new Point(horizontalPlottingNode.Bounds.X + horizontalPlottingNode.Size.Width / 2 - PlotScaleText.Length / 2 - sideEmptySpace, horizontalPlottingNode.Bounds.Y + nodeHeight / 2 - 2 * barHeight));

                dc.Close();
            }
        }

        private void DrawVerticalPlottingScale(DrawingVisual dv, Node node)
        {
            if (dv != null)
            {
                var dc = dv.RenderOpen();

                System.Windows.Media.Brush brush = new SolidColorBrush(plottingScaleColorToUse);
                verticalPlottingNode.Foreground = new SolidColorBrush(plottingScaleColorToUse);
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
                FormattedText scaleTextFormat = new FormattedText(PlotScaleText,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface("TimeNewRoman"), 14, brush);

                dc.PushTransform(new RotateTransform(-270, verticalPlottingNode.Bounds.X + verticalBarWidth + sideEmptySpace + barHeight, verticalPlottingNode.Bounds.Y + nodeHeight / 2 - sideEmptySpace));
                dc.DrawText(scaleTextFormat, new Point(verticalPlottingNode.Bounds.X + verticalBarWidth + sideEmptySpace + barHeight, verticalPlottingNode.Bounds.Y + nodeHeight / 2 - sideEmptySpace));
                dc.Pop();

                dc.Close();
            }
        }

        private void UpdateNodeScalingMeterText()
        {
            RedDrawPlottingScale(addFlow.Items.OfType<Node>().ToArray());
        }

        private void AddHorizontalPlottingNodeToAddFlow()
        {
            DeleteNodeIfExist(verticalPlottingNode);
            DeleteNodeIfExist(horizontalPlottingNode);
            if(addFlow.ViewportHeight.Equals(double.NaN)){return;}
            horizontalPlottingNode = new Node(10, addFlow.ViewportHeight - 30, 150, 25, "", addFlow)
            {
                StrokeThickness = 0,
                TextPosition = TextPosition.CenterBottom,
                TextMargin = new Thickness(0, 1, 0, 1),
                Geometry = new RectangleGeometry(new Rect(0, 0, 64, 64)),
                IsYSizeable = false,
                IsRotatable = false,
                IsInLinkable = false,
                IsOutLinkable = false
            };

            if (verticalPlottingNode != null)
            {
                var newLocation = verticalPlottingNode.Bounds.TopLeft;
                // var newLocation = new Point(verticalPlottingNode.Location.X, verticalPlottingNode.Location.Y - verticalPlottingNode.Size.Width);

                var addFlowBounds = new Rect(0, 0, addFlow.ExtentWidth, addFlow.ExtentHeight);
                if (addFlowBounds.Contains(newLocation))
                {
                    horizontalPlottingNode.Location = new Point(newLocation.X, newLocation.Y);
                    horizontalPlottingNode.Size = new System.Windows.Size(verticalPlottingNode.Size.Height, verticalPlottingNode.Size.Width);
                }
            }

            var dv = new DrawingVisual();

            DrawHorizontalPlottingScale(dv, horizontalPlottingNode);

            horizontalPlottingNode.Visual.Children.Add(dv);

            horizontalPlottingNode.Fill = Brushes.Transparent;

            addFlow.AddNode(horizontalPlottingNode);
        }

        private void AddVerticalPlottingNodeToAddFlow()
        {
            DeleteNodeIfExist(horizontalPlottingNode);
            DeleteNodeIfExist(verticalPlottingNode);
            if(addFlow.ViewportHeight.Equals(double.NaN)){return;}
            verticalPlottingNode = new Node(10, addFlow.ViewportHeight - 150, 50, 150, "", addFlow)
            {
                StrokeThickness = 0,
                TextPosition = TextPosition.CenterMiddle,
                TextMargin = new Thickness(0, 10, 0, 10),
                Geometry = new RectangleGeometry(new Rect(0, 0, 64, 64)),
                IsXSizeable = false,
                IsRotatable = false,
                IsInLinkable = false,
                IsOutLinkable = false
            };

            if (horizontalPlottingNode != null)
            {
                var newLocation = horizontalPlottingNode.Bounds.TopLeft;

                var addFlowBounds = new Rect(0, 0, addFlow.ExtentWidth, addFlow.ExtentHeight);
                if (addFlowBounds.Contains(newLocation))
                {
                    verticalPlottingNode.Location = newLocation;
                    verticalPlottingNode.Size = new System.Windows.Size(horizontalPlottingNode.Size.Height, horizontalPlottingNode.Size.Width);
                }
            }

            var verticalPlottingVisual = new DrawingVisual();

            DrawVerticalPlottingScale(verticalPlottingVisual, verticalPlottingNode);

            verticalPlottingNode.Visual.Children.Add(verticalPlottingVisual);

            verticalPlottingNode.Fill = Brushes.Transparent;

            addFlow.AddNode(verticalPlottingNode);
        }

        #endregion
    }
}
