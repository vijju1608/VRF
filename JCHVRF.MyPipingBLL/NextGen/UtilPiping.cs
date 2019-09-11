using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using JCHVRF.Model.NextGen;

using System.Text.RegularExpressions;
using Lassalle.WPF.Flow;
using Lassalle.WPF.Flow.Layout;
using System.Windows.Media;
using Point = System.Windows.Point;
using OldModel = JCHVRF.Model;


namespace JCHVRF.MyPipingBLL.NextGen
{
    /// <summary>
    /// Piping 以及 Wiring 界面中绘图过程的辅助类
    /// </summary>
    public class UtilPiping
    {
        #region  基础变量

        public const float VDistanceVertical = 100;
        public const float HDistanceVertical = 150;
        public const float VDistanceHorizontal = 120;
        public const float VDistanceHorizontal_CHbox = 170;
        public const float HDistanceHorizontal = 150;
        public const float HeightForNodeText = 26f;

        public const float VDistanceVertical_wiring = 76;   // 纵向排列时，纵向单位距离
        public const float HDistanceVertical_wiring = 236;  // 纵向排列时，横向单位距离

        private Bitmap bmpMeasureString = null;
        private Graphics gMeasureString = null;

        //public SizeF sizeUnitOut = new SizeF(185, 136);
        public System.Windows.Size sizeYP = new System.Windows.Size(31, 16);
        public System.Windows.Size sizeCP = new System.Windows.Size(120, 30);

        public System.Windows.Media.Brush colorHover = System.Windows.Media.Brushes.Orange;
        public System.Windows.Media.Brush colorDefault = System.Windows.Media.Brushes.Black;
        public System.Windows.Media.Brush colorSelect = System.Windows.Media.Brushes.Blue;
        public System.Windows.Media.Brush colorDash = System.Windows.Media.Brushes.Brown;
        public System.Windows.Media.Brush colorTransparent = System.Windows.Media.Brushes.Transparent;
        public System.Windows.Media.Brush colorWarning = System.Windows.Media.Brushes.Red;
        public System.Windows.Media.Brush colorWiring = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136));
        public System.Windows.Media.Brush colorYP = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(83, 43, 102));
        //public Brush colorWiring = (136, 136, 136);
        //public Brush colorYP = Brushes.FromArgb(51, 153, 255);
        public System.Windows.Media.Brush colorText = System.Windows.Media.Brushes.Black;
        public System.Windows.Media.Brush colorNodeBg = System.Windows.Media.Brushes.White;

        #region Heat Recovery piping color --add on 20160509 by Yunxiao Lin
        public System.Windows.Media.Brush color_piping_3pipe = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 75, 0)); // liquid pipe and high pressure gas pipe and low pressure gas pipe 
        public System.Windows.Media.Brush color_piping_2pipe_lowgas = System.Windows.Media.Brushes.SkyBlue; // liquid pipe and low pressure gas pipe
        public System.Windows.Media.Brush color_piping_2pipe = System.Windows.Media.Brushes.LightGreen; // liquid pipe and gas pipe
        #endregion

        public Font textFont_piping = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Pixel); // 12f, GraphicsUnit.Pixel
        //public Brush textBrush_piping = new SolidBrush(Color.Black);     // Brushes.Black; 文字的清晰度提高
        //public Pen pen_piping = new Pen(Color.Black, 0.1f);

        public Font textFont_wiring = new Font("Arial", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        public Font textFont_wiring_linemark = new Font("Arial", 14f, FontStyle.Regular, GraphicsUnit.Pixel);

        public const float OutdoorOffset_Y_wiring = 38f;
        public const float YPAdjustmentFactorToAchieveSymmetry = 4;

        /// <summary>
        /// 重置颜色到默认的颜色
        /// </summary>
        public void ResetColors()
        {
            colorDefault = System.Windows.Media.Brushes.Black;
            colorYP = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(83, 43, 102));
            //colorYP = Color.FromArgb(51, 153, 255);
            colorText = System.Windows.Media.Brushes.Black;
            colorNodeBg = System.Windows.Media.Brushes.Transparent;
        }

        #endregion

        public UtilPiping()
        {
            bmpMeasureString = new Bitmap(500, 100);
            gMeasureString = Graphics.FromImage(this.bmpMeasureString);
        }

        int Count = 0;
        #region 动态设置Piping图中节点的显示状态
        /// 设置Node外框颜色，连同其InLink一同设置
        /// <summary>
        /// 设置Node外框颜色，连同其InLink一同设置
        /// </summary>
        /// <param name="node"></param>
        /// <param name="color"></param>
        private void setNodeColor(Node node, System.Windows.Media.Brush color)
        {
            if (node is MyNodeOut)
                return;

            if (node is MyNode)
            {
                if ((node as MyNode).MyInLinks != null && (node as MyNode).MyInLinks.Count > 0)
                {
                    node.Stroke = color;
                }
            }
        }

        private void setLinkColor(Node node, System.Windows.Media.Brush color)
        {
            if (node is MyNode)
            { 
            if ((node as MyNode).MyInLinks != null && (node as MyNode).MyInLinks.Count > 0)
            {
                foreach (Link lk in (node as MyNode).MyInLinks)
                {
                    setLinkColor(lk, color);
                }
            }
            }
        }

        private void setLinkColor(Link lk, System.Windows.Media.Brush color)
        {
            if (lk != null)
            {
                lk.Stroke = color;
            }
        }

        /// 设置Item的Hover状态
        /// <summary>
        /// 设置Item的Hover状态
        /// </summary>
        /// <param name="item"></param>
        public void setItemHover(Item item)
        {
            if (item == null) return;
            if (item.Stroke == colorWarning) return;
            if (item is Node)
            {
                Node node = item as Node;
                node.Stroke = colorHover;
            }
            else
            {
                Link lk = item as Link;
                lk.Stroke = colorHover;
            }
        }

        /// 取消Item的Hover状态
        /// <summary>
        /// 取消Item的Hover状态
        /// </summary>
        public void cancelItemHover(Item item, bool isHR)
        {
            if (item == null) return;
            Node node;
            if (item is Node)
            {
                node = item as Node;
            }
            else
            {
                node = (item as Link).Dst;
            }
            if (node == null) return;
            if (node.Stroke == colorHover)
            {
                setNodeColor(node, colorTransparent);
            }
            if (node.Links != null && node.Links[0].Stroke == colorHover)
            {
                if (isHR)
                {
                    setHeatRecoveryLinkColor(node);
                }
                else
                {
                    setLinkColor(node, colorDefault);
                }
            }
        }

        /// 设置Item的选中状态
        /// <summary>
        /// 设置Item的选中状态
        /// </summary>
        /// <param name="item"></param>
        public void setItemSelected(Item item)
        {
            if (item == null) return;
            Node node;
            if (item is Node)
            {
                node = item as Node;
            }
            else
            {
                node = (item as Link).Dst;
            }
            if (node == null) return;

            setNodeColor(node, colorSelect);
            setLinkColor(node, colorSelect);

            // node.DrawWidth = 1;
            //node.DrawWidth = 1;
            node.DashStyle = DashStyles.Dash;
            // node.DashStyle =System.Windows.Media.DashStyle.DashesPropert;
            if (node.Links != null)
            {
                foreach (Link lk in node.Links)
                {
                    //lk.DrawWidth = 1;
                    lk.DashStyle = DashStyles.Dash;
                }
            }
        }

        /// <summary>
        /// 设置节点的默认颜色
        /// </summary>
        public void setItemDefault(Item item, bool isHR)
        {
            if (item == null) return;
            Node node = null;
            if (item is MyNode)
            {
                node = item as Node;
            }
            else if (item is MyLink)
            {
                node = (item as Link).Dst;
            }
            if (node == null) return;
            if (isHR)
            {
                setHeatRecoveryLinkColor(node);
            }
            else
            {
                setLinkColor(node, colorDefault);
            }
            setNodeColor(node, colorTransparent);

            // node.DrawWidth = 1;
            node.DashStyle = DashStyles.Solid;
            node.Stroke = System.Windows.Media.Brushes.RoyalBlue;

            if (node.Links != null)
            {
                foreach (Link lk in node.Links)
                {
                    // lk.DrawWidth = 1;
                    lk.DashStyle = DashStyles.Solid;
                }
            }
        }
        //Took latest code from legacy
        public void setItemDefault(Item item)
        {
            if (item == null) return;
            Node node = null;
            if (item is MyNode)
            {
                node = item as Node;
            }
            else if (item is MyLink)
            {
                node = (item as Link).Dst;
            }
            if (node == null) return;
            //if (isHR)
            //{
            //    setHeatRecoveryLinkColor(node);
            //}
            //else
            //{
            //    setLinkColor(node, colorDefault);
            //}
            setLinkDefaultColor(node);
            setNodeColor(node, colorTransparent);

            //node.DrawWidth = 1;
            //node.DashStyle = DashStyle.Solid;

            //if (node.InLinks != null)
            //{
            //    foreach (Link lk in node.InLinks)
            //    {
            //        lk.DrawWidth = 1;
            //        lk.DashStyle = DashStyle.Solid;
            //    }
            //}
        }
        //Took latest code from legacy
        private void setLinkDefaultColor(Node node)
        {
            if (!(node is MyNode)) return;

            System.Windows.Media.Brush linkColor;
            MyNode myNode = node as MyNode;
            switch (myNode.PipesType)
            {
                case JCHVRF.Model.PipeCombineType.HR_L_G:
                    linkColor = color_piping_2pipe;
                    break;
                case JCHVRF.Model.PipeCombineType.HR_L_LG:
                    linkColor = color_piping_2pipe_lowgas;
                    break;
                case JCHVRF.Model.PipeCombineType.HR_L_LG_HG:
                    linkColor = color_piping_3pipe;
                    break;
                case JCHVRF.Model.PipeCombineType.HP_L_G:
                default:
                    linkColor = colorDefault;
                    break;
            }
            setLinkColor(node, linkColor);
        }

        /// 设置Item的默认状态 Heat Recovery add on 20160509 by Yunxiao Lin
        /// <summary>
        /// 设置Item的默认状态 Heat Recovery
        /// </summary>
        private void setHeatRecoveryLinkColor(Node node)
        {
            if (node is MyNodeCH)
            {
                setLinkColor(node, color_piping_3pipe);
            }
            else if (node is MyNodeMultiCH)
            {
                setLinkColor(node, color_piping_3pipe);
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;
                if (nodeIn.IsCoolingonly)
                    setLinkColor(node, color_piping_2pipe_lowgas);
                else
                    setLinkColor(node, color_piping_2pipe);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                if (nodeYP.IsCoolingonly)
                {
                    setLinkColor(nodeYP, color_piping_2pipe_lowgas);
                }
                else if (ExistsCHDownward(nodeYP))
                {
                    setLinkColor(nodeYP, color_piping_3pipe);
                }
                else
                {
                    setLinkColor(nodeYP, color_piping_2pipe);
                }
            }
        }

        /// <summary>
        /// 设置Item的警告状态
        /// </summary>
        /// <param name="item"></param>
        public void setItemWarning(Item item)
        {
            if (item == null) return;
            Node node;
            if (item is Node)
            {
                node = item as Node;
            }
            else
            {
                node = (item as Link).Dst;
            }
            if (node == null) return;

            setNodeColor(node, colorWarning);
            setLinkColor(node, colorWarning);

            //  node.DrawWidth = 1;
            node.DashStyle = DashStyles.Dash;

            if (node.Links != null)
            {
                foreach (Link lk in node.Links)
                {
                    //lk.DrawWidth = 1;
                    lk.DashStyle = DashStyles.Solid;
                }
            }
        }
        #endregion

        #region 获取节点对象特殊位置的点坐标
        /// 获取节点对象的中心点坐标
        /// <summary>
        /// 获取节点对象的中心点坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getCenterPointF(Node node)
        {
            float fx = (AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width) / 2);
            float fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);
            return new Point(fx, fy);
        }

        /// 获取节点对象的左边中点的坐标
        /// <summary>
        /// 获取节点对象的左边中点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getLeftCenterPointF(Node node)
        {
            float fy;
            float fx = AddFlowExtension.FloatConverter(node.Location.X);
            if (node is MyNodeYP)
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2) - YPAdjustmentFactorToAchieveSymmetry;
            else
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);
                        
            return new Point(fx, fy);
        }        

        //getLeftCenterPointForHorizontalMirrorYP method can be used for both orientations - "Vertical" and "HorizontalMirror"
        public Point getLeftCenterPointForHorizontalMirrorYP(Node node)
        {
            float fy;
            float fx = AddFlowExtension.FloatConverter(node.Location.X);
            if (node is MyNodeYP)
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2) + YPAdjustmentFactorToAchieveSymmetry;
            else
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);

            return new Point(fx, fy);
        }
        public Point getRightCenterPointForHorizontalMirrorYP(Node node)
        {
            float fy;
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            if (node is MyNodeYP)
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2) + YPAdjustmentFactorToAchieveSymmetry;
            else
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);
            return new Point(fx, fy);
        }

        /// 获取节点对象的右边中点的坐标
        /// <summary>
        /// 获取节点对象的右边中点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getRightCenterPointF(Node node)
        {
            float fy;
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            if (node is MyNodeYP)            
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2) - YPAdjustmentFactorToAchieveSymmetry;                             
            else
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);
            return new Point(fx, fy);
        }

        public Point getRightCenterPointForVerticalMirrorYP(Node node)
        {
            float fy;
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            if (node is MyNodeYP)
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2) + YPAdjustmentFactorToAchieveSymmetry;                            
            else
                fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);
            return new Point(fx, fy);
        }

        /// 获取节点对象的上边中点的坐标
        /// <summary>
        /// 获取节点对象的上边中点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getTopCenterPointF(Node node)
        {
            float fx;
            Point ptTop;
            if (node is MyNodeYP)
                fx = (AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width) / 2) + YPAdjustmentFactorToAchieveSymmetry;
            else
                fx = (AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width) / 2);

            float fy = AddFlowExtension.FloatConverter(node.Location.Y);

            ptTop = new Point(fx, fy);

            ptTop = alignLinkPerYBranchOrientation(node, ptTop, "Top");
            return ptTop;
        }

        /// 获取节点对象的底边中点的坐标
        /// <summary>
        /// 获取节点对象的底边中点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getBottomCenterPointF(Node node)
        {
            float fx;
            Point ptBottom;
            if (node is MyNodeYP)
                fx = (AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width) / 2) + YPAdjustmentFactorToAchieveSymmetry;
            else
                fx = (AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width) / 2);

            float fy = AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height);

            ptBottom = new Point(fx, fy);

            ptBottom = alignLinkPerYBranchOrientation(node, ptBottom, "Bottom");
            return ptBottom;
        }

        /// 获取节点对象的左下角顶点的坐标
        /// <summary>
        /// 获取节点对象的左下角顶点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getLeftBottomPointF(Node node)
        {
            float fx = AddFlowExtension.FloatConverter(node.Location.X);
            float fy = AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height);
            return new Point(fx, fy);
        }

        /// 获取节点对象的右上角顶点的坐标
        /// <summary>
        /// 获取节点对象的右上角顶点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getRightTopPointF(Node node)
        {
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            float fy = AddFlowExtension.FloatConverter(node.Location.Y);
            return new Point(fx, fy);
        }

        /// 获取节点对象的右下角顶点的坐标
        /// <summary>
        /// 获取节点对象的右下角顶点的坐标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Point getRightBottomPointF(Node node)
        {
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            float fy = AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height);
            return new Point(fx, fy);
        }
        #endregion

        #region 获取绘制Piping图时指定的点坐标
        /// 获取某个点偏移后的坐标
        /// <summary>
        /// 获取某个点偏移后的坐标
        /// </summary>
        /// <param name="ptf">原始点坐标</param>
        /// <param name="size">偏移尺寸</param>
        /// <param name="isLeftUp">向左上方偏移</param>
        /// <returns></returns>
        public PointF getOffsetPointF(PointF ptf, SizeF size, bool isLeftUp)
        {
            if (isLeftUp)
                return new PointF(ptf.X - size.Width, ptf.Y - size.Height);
            else
                return new PointF(ptf.X + size.Width, ptf.Y + size.Height);
        }

        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <param name="isVertical">是否是竖直方向排版</param>
        /// <returns></returns>
        public Point getLocationChild(Node parent, Node node, bool isVertical, ref float maxY)
        {
            Point ptf = new Point();
            Point parentCenterPosi = new Point();
            try
            {
                if (parent != null) parentCenterPosi = getCenterPointF(parent);
            }
            catch { }

            // 第一分歧管 or 一对一室内机，位置固定
            if (parent is MyNodeOut)
            {
                try
                {
                    if (node is MyNodeIn)
                    {
                        ptf.X = parent.Location.X + parent.Size.Width + 70 - YPAdjustmentFactorToAchieveSymmetry;
                        ptf.Y = parent.Location.Y + parent.Size.Height + 70;
                    }
                    else if (node is MyNodeCH)
                    {
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                        ptf.Y = parent.Location.Y + parent.Size.Height + 65;
                    }
                    else if (node is MyNodeMultiCH)
                    {
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                        if (isVertical)
                        {
                            ptf.Y = parent.Location.Y + parent.Size.Height + 10;
                        }
                        else
                        {
                            ptf.Y = parent.Location.Y + parent.Size.Height + 65;
                        }
                    }
                    else if (node is MyNodeYP)
                    {
                        ptf.X = parent.Location.X + Math.Max(parent.Size.Width, 150);
                        ptf.Y = parent.Location.Y + Math.Max(parent.Size.Height, 50) + 18;
                    }
                }
                catch (Exception ex) { }
            }
            else if (parent is MyNodeYP)
            {
                try
                {
                    MyNodeYP parentYP = parent as MyNodeYP;
                    int nodeIndex = parentYP.GetIndex(node);    // 获取子节点的索引号 
                    if (isVertical)
                    {
                        // 纵向排列
                        if (nodeIndex == 0)
                        {
                            maxY = AddFlowExtension.FloatConverter(parentCenterPosi.Y);
                        }
                        else
                        {
                            maxY += VDistanceVertical;
                        }

                        if (node is MyNodeCH)
                        {
                            ptf.X = parent.Location.X + HDistanceVertical - 20;
                        }
                        else if (node is MyNodeMultiCH)
                        {
                            ptf.X = parent.Location.X + HDistanceVertical - 20;
                            maxY += 40;
                        }
                        else if (node is MyNodeYP)
                        {                            
                            if (parentYP.ParentNode is MyNodeOut)
                            {
                                ptf.X = parentCenterPosi.X - node.Size.Width / 2 + 8;
                            }
                            else
                            {
                                if (nodeIndex > 0)          // 第二个节点是 YP || CP
                                {
                                    ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                                }
                                else
                                {
                                    ptf.X = parent.Location.X + HDistanceVertical;
                                }
                            }                            
                        }
                        else if (node is MyNodeIn)
                        {
                            //ptf.X = parent.Location.X + HDistanceVertical * 1.5f;
                            ptf.X = parent.Location.X + node.Size.Width;
                        }
                        if(parentYP.ParentNode is MyNodeOut)
                            ptf.Y = maxY - (node.Size.Height / 2) + (50 * nodeIndex) - YPAdjustmentFactorToAchieveSymmetry;
                        else
                            ptf.Y = maxY - (node.Size.Height / 2) + (50 * nodeIndex) + YPAdjustmentFactorToAchieveSymmetry;
                    }
                    else
                    {
                        // 横向排列
                        if (node is MyNodeYP)   // YP || CP节点
                        {
                            if (nodeIndex == 0)
                            {
                                // YP 节点的第一个子节点是 NodeYP 节点
                                ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                                ptf.Y = maxY - node.Size.Height / 2 + VDistanceHorizontal * 0.75f;
                            }
                            else
                            {
                                //parent只能是YP,不会是CP，这是它的第二个子节点
                                ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                                ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                            }
                        }
                        else if (node is MyNodeCH)
                        {
                            // 横向排列时 CHbox 的坐标
                            ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal * nodeIndex + YPAdjustmentFactorToAchieveSymmetry;
                            ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + 80;
                        }
                        else if (node is MyNodeMultiCH)
                        {
                            // 横向排列时 Multi CHbox 的坐标
                            ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal * nodeIndex;
                            ptf.Y = parentCenterPosi.Y + 67;
                        }
                        else if (node is MyNodeIn)           // OldModel.Indoor节点
                        {
                            //ptf.X = parentCenterPosi.X + 50 + HDistanceHorizontal * nodeIndex;
                            ptf.X = parentCenterPosi.X + (150 * nodeIndex) - (node.Size.Width / 2) + YPAdjustmentFactorToAchieveSymmetry;
                            //ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceHorizontal - 22;
                            ptf.Y = parentCenterPosi.Y + node.Size.Height;

                            //根据maxY值计算, indoor与右边最低的indoor齐平
                            float y1 = maxY - AddFlowExtension.FloatConverter(node.Size.Height) / 2;
                            if (y1 > ptf.Y)
                            {
                                ptf.Y = y1;
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }
            else if (parent is MyNodeCH)
            {
                try
                {
                    if (node is MyNodeIn)
                    {
                        if (isVertical)
                        {
                            // CHBox 直接连接一个IDU
                            ptf.X = parent.Location.X + HDistanceVertical;
                            ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                        }
                        else
                        {
                            // 横向排列时 CHbox 下方节点的坐标
                            ptf.X = parentCenterPosi.X - (node.Size.Width / 2);
                            ptf.Y = parentCenterPosi.Y + node.Size.Height / 2 + 30;

                            //根据maxY值计算, indoor与右边最低的indoor齐平
                            float y1 = maxY - AddFlowExtension.FloatConverter(node.Size.Height) / 2;
                            if (y1 > ptf.Y)
                            {
                                ptf.Y = y1;
                                //OldModel.Indoor的位置重置CHBox的位置
                                parent.Location = new Point(parent.Location.X, y1 + node.Size.Height / 2 - 48 - parent.Size.Height / 2);
                            }
                        }
                    }
                    else if (node is MyNodeYP)
                    {
                        // add on 20160505 by Yunxiao Lin
                        if (isVertical)
                        {
                            // CHBox 直接连接一个YP/CP
                            maxY = AddFlowExtension.FloatConverter(parentCenterPosi.Y);
                            ptf.X = parent.Location.X + HDistanceVertical;
                            ptf.Y = maxY - node.Size.Height / 2;
                        }
                        else
                        {
                            // 横向排列时 CHbox 下方节点的坐标
                            ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                            ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + 85;

                            //根据maxY值计算, YP 应略低于右边最低的indoor
                            float y1 = maxY - AddFlowExtension.FloatConverter(node.Size.Height) / 2 + VDistanceHorizontal * 0.75f;
                            if (y1 > ptf.Y)
                            {
                                ptf.Y = y1;
                                //YP的位置重置CHBox的位置
                                parent.Location = new Point(parent.Location.X, y1 + node.Size.Height / 2 - 85 - parent.Size.Height / 2);
                            }
                        }
                    }
                }
                catch (Exception ex) { }

            }
            else if (parent is MyNodeMultiCH)
            {
                try
                {
                    MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                    int nodeIndex = parentMCH.GetIndex(node);    // 获取子节点的索引号

                    if (isVertical)
                    {
                        // 纵向排列
                        if (nodeIndex == 0)
                            maxY = AddFlowExtension.FloatConverter(parentCenterPosi.Y);
                        else
                            maxY += VDistanceVertical;

                        ptf.X = parentCenterPosi.X + (HDistanceVertical * 2) - node.Size.Width / 2;
                        ptf.Y = maxY - node.Size.Height / 2;
                    }
                    else
                    {
                        // 横向排列
                        if (node is MyNodeYP)   // YP || CP节点
                        {
                            ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;

                            if (nodeIndex == 0)
                            {
                                float y2 = AddFlowExtension.FloatConverter(parent.Size.Height) / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                                ptf.Y = parent.Location.Y + y2 - node.Size.Height / 2;

                                float y1 = maxY - AddFlowExtension.FloatConverter(node.Size.Height) / 2 + VDistanceHorizontal * 0.75f;
                                if (y1 > ptf.Y)
                                {
                                    ptf.Y = y1;
                                    //YP的位置重置CHBox的位置
                                    parent.Location = new Point(parent.Location.X, y1 + node.Size.Height / 2 - y2);
                                }
                            }
                            else
                            {
                                //根据maxY值计算, YP 应略低于右边最低的indoor
                                ptf.Y = maxY - node.Size.Height / 2 + VDistanceHorizontal * 0.75f;
                            }
                        }
                        else if (node is MyNodeIn)           // OldModel.Indoor节点
                        {
                            ptf.X = parentCenterPosi.X + HDistanceHorizontal + 18;
                            if (nodeIndex == 0)
                            {
                                float y2 = AddFlowExtension.FloatConverter(parent.Size.Height) / (parentMCH.ChildNodes.Count + 1) * (nodeIndex + 1);
                                ptf.Y = parent.Location.Y + y2 - node.Size.Height / 2;

                                //根据maxY值计算, OldModel.Indoor 应略低于右边最低的indoor
                                float y1 = maxY - AddFlowExtension.FloatConverter(node.Size.Height) / 2 + VDistanceHorizontal;
                                if (y1 > ptf.Y)
                                {
                                    ptf.Y = y1;
                                    //根据OldModel.Indoor的位置重置CHBox的位置
                                    parent.Location = new Point(parent.Location.X, y1 + node.Size.Height / 2 - y2);
                                }
                            }
                            else
                            {
                                //根据maxY值计算, OldModel.Indoor 应略低于右边最低的indoor
                                ptf.Y = maxY - node.Size.Height / 2 + VDistanceHorizontal;
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }
            return ptf;
        }

        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <param name="isVertical">是否是竖直方向排版</param>
        /// <returns></returns>
        public Point GetBinaryTreeNodeLocationHorizontal(Node parent, Node node, float maxX)
        {
            Point parentCenterPosi = getBottomCenterPointF(parent);
            Point ptf = new Point();

            // 室外机的第一个子节点，横向或纵向排列的位置相同
            if (parent is MyNodeOut)
            {
                ptf.X = parent.Location.X + parent.Size.Width;
                ptf.Y = parent.Location.Y + parent.Size.Height;

                if (node is MyNodeYP)
                {
                    ptf.X = parentCenterPosi.X - 5;
                    ptf.Y = ptf.Y - node.Size.Height / 2 + VDistanceHorizontal * 0.4f + 22;
                }
                else if (node is MyNodeIn)
                {
                    ptf.X += 30;
                    ptf.Y += 60;
                }
                else if (node is MyNodeCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height + 130;
                }
                else if (node is MyNodeMultiCH)
                {
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parent.Location.Y + parent.Size.Height + 65;
                }
                return ptf;
            }

            if(parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                int nodeIndex = parentYP.GetIndex(node);

                if(node is MyNodeYP)
                {                                    
                    if(parentYP.ChildNodes[0] is MyNodeYP && parentYP.ChildNodes[1] is MyNodeYP)
                    {                                               
                        if(node.Equals(parentYP.ChildNodes[0]))
                        {
                            GetChildIDUCount(parentYP.ChildNodes[1] as MyNodeYP);
                            //position of 0 index child will depend on number of childs in 1 index child.
                            ptf.X = parentYP.Location.X + HDistanceHorizontal * Count;
                            Count = 0;
                        }
                        else
                        {
                            ptf.X = parentYP.Location.X;
                        }
                        ptf.Y = parentYP.Location.Y + 100;
                    }
                    else
                    {
                       if(nodeIndex == 0)
                        {
                            ptf.X = parentYP.Location.X + node.Size.Width + 115;                        
                        }
                        else
                        {
                            ptf.X = parentYP.Location.X;
                        }
                        ptf.Y = parentYP.Location.Y + VDistanceHorizontal/3;     
                    }                    
                    
                }
                else if(node is MyNodeIn)
                {
                    ptf.Y = parentYP.Location.Y + VDistanceHorizontal;
                    if(nodeIndex == 0)
                    {
                        ptf.X = parentYP.Location.X + 84;                        
                    }
                    else
                    {
                        ptf.X = parentYP.Location.X - node.Size.Width / 2 + 4;
                        if(parentYP.ChildNodes[0] is MyNodeYP)
                        {
                            ptf.Y = ptf.Y + parentYP.Size.Height;
                        }
                    }                    
                }
                else if(node is MyNodeCH)
                {
                    ptf.Y = parentYP.Location.Y + 100;
                    if(nodeIndex == 0)
                    {
                        ptf.X = parentYP.Location.X + 150;                        
                    }
                    else
                    {
                        ptf.X = parentYP.Location.X - node.Size.Width / 2 + 4;                        
                    }  
                }
            }           
            else if (parent is MyNodeCH)               //  CHbox 
            {
                ptf.X = parent.Location.X - 47;   //CHBOX节点的横坐标在子节点排列好以后重置
                ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + 85;
            }
            else if (parent is MyNodeMultiCH)              // Multi CHbox 
            {
                ptf.X = parent.Location.X;   //Multi CHBOX节点的横坐标在子节点排列好以后重置
                ptf.Y = parent.Location.Y + parent.Size.Height + 60;
            }
            return ptf;
        }

        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <returns></returns>
        public Point GetBinaryTreeNodeLocationVertical(Node parent, Node node, float maxY)
        {
            Point ptf = new Point();
            Point parentRightCenterPosi = getRightCenterPointF(parent);
            Point parentBottomCenterPosi = getBottomCenterPointF(parent);

            // 室外机的第一个子节点，横向或纵向排列的位置相同
            if (parent is MyNodeOut)
            {                
                if (node is MyNodeYP)
                {
                    ptf.X = parentBottomCenterPosi.X - 5;
                    ptf.Y = parentBottomCenterPosi.Y + HDistanceVertical / 3;
                }
                else if (node is MyNodeIn)
                 {
                    ptf.X = parentBottomCenterPosi.X + 60;
                    ptf.Y = parentBottomCenterPosi.Y + 70;
                }
                else if (node is MyNodeCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算
                    ptf.X = parentBottomCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parentBottomCenterPosi.Y - node.Size.Height + 130;
                }
                else if (node is MyNodeMultiCH)
                {
                    ptf.X = parentBottomCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parent.Location.Y + parent.Size.Height + 10;
                }
                return ptf;
            }
            if(parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                int nodeIndex = parentYP.GetIndex(node);

                if(node is MyNodeYP)
                {
                    if (parentYP.ChildNodes[0] is MyNodeYP && parentYP.ChildNodes[1] is MyNodeYP)
                    {                        
                        if (node.Equals(parentYP.ChildNodes[0]))
                        {
                            GetChildIDUCount(parentYP.ChildNodes[1] as MyNodeYP);
                            ptf.Y = parentBottomCenterPosi.Y + 1.5 * HDistanceVertical * Count;
                            Count = 0;
                        }
                        else
                        {
                            ptf.Y = parentBottomCenterPosi.Y - node.Size.Width / 2;
                        }
                        ptf.X = parentRightCenterPosi.X + VDistanceHorizontal;
                    }
                    else
                    {
                        if (nodeIndex == 0)
                        {
                            ptf.Y = parentBottomCenterPosi.Y + node.Size.Height + VDistanceHorizontal + 100;
                        }
                        else
                        {
                            ptf.Y = parent.Location.Y;
                        }
                        ptf.X = parentRightCenterPosi.X + VDistanceHorizontal / 2;
                    }

                }
                else if(node is MyNodeIn)
                {
                    ptf.X = parentRightCenterPosi.X + VDistanceHorizontal;
                    if(nodeIndex == 0)
                    {
                        ptf.Y = parentRightCenterPosi.Y + 50 + node.Size.Height * 2;                        
                    }
                    else
                    {
                        ptf.Y = parentRightCenterPosi.Y + node.Size.Height / 2;
                        if(parentYP.ChildNodes[0] is MyNodeYP)
                        {
                            ptf.X = ptf.X + parentYP.Size.Width + node.Size.Width / 2;
                        }
                    }                    
                }
                else if(node is MyNodeCH)
                {                    
                    ptf.X = parentRightCenterPosi.X + VDistanceHorizontal + node.Size.Height; 
                    if(nodeIndex == 0)
                    {
                        ptf.Y = parentBottomCenterPosi.Y + VDistanceHorizontal + 115;
                        if (parentYP.ChildNodes[0] is MyNodeYP)
                        {
                            ptf.X = ptf.X + parentYP.Size.Width + node.Size.Width / 2;
                        }
                    }
                    else
                    {
                        if(parentYP.YBranchOrientation == MyNodeYP.BranchOrientations.VerticalMirror.ToString())
                        {
                            ptf.Y = parent.Location.Y - node.Size.Height / 2 + 65;
                        }
                        else
                        {
                            ptf.Y = parent.Location.Y - node.Size.Height / 2 + 49;
                        }                                    
                    }  
                }
            }
            else if (parent is MyNodeCH)               //  CHbox 
            {
                ptf.X = parent.Location.X - node.Size.Width / 2 + 15;   //CHBOX节点的横坐标在子节点排列好以后重置
                ptf.Y = parentBottomCenterPosi.Y + node.Size.Height / 4 + 20;
            }
            else if (parent is MyNodeMultiCH)              // Multi CHbox 
            {
                ptf.X = parent.Location.X + parent.Size.Width + 60;   //Multi CHBOX节点的横坐标在子节点排列好以后重置
                ptf.Y = parent.Location.Y;
            }            
            return ptf;
        }

        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <param name="isVertical">是否是竖直方向排版</param>
        /// <returns></returns>
        public Point GetSymmetriesLayoutNodeLocationHorizontal(Node parent, Node node, bool forward, float topY, out float offsetY)
        {
            Point parentCenterPosi = getCenterPointF(parent);
            Point ptf = new Point();
            offsetY = 0;

            // 室外机的第一个子节点，横向或纵向排列的位置相同
            if (parent is MyNodeOut)
            {
                ptf.X = parent.Location.X + parent.Size.Width;
                ptf.Y = parent.Location.Y + parent.Size.Height;

                if (node is MyNodeYP)
                {
                    ptf.X = 50;
                    //临时设置，根据子节点调整Y坐标
                    ptf.Y = ptf.Y - node.Size.Height / 2 + VDistanceHorizontal - 22;
                }
                else if (node is MyNodeIn)
                {
                    ptf.X += 50;
                    ptf.Y += 40;
                }
                else if (node is MyNodeCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height + 150;
                }
                return ptf;
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                int nodeIndex = parentYP.GetIndex(node);    // 获取子节点的索引号

                if (node is MyNodeYP)   // YP || CP节点
                {
                    // YP 节点的第一个子节点是 NodeYP 节点
                    if (nodeIndex == 0)
                    {
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                        //暂定Y坐标大于topY一个位置，需要根据子节点的Y坐标重新计算
                        ptf.Y = topY - node.Size.Height / 2 + (VDistanceHorizontal * 0.75f) * (forward ? 1 : -1);
                    }
                    else
                    {
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                        //暂定Y坐标跟前一个YP相等，需要根据子节点的Y坐标重新计算
                        ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                    }
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + 50 + HDistanceHorizontal * nodeIndex;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + (VDistanceHorizontal - 22) * (forward ? 1 : -1);
                }
                else if (node is MyNodeCH)
                {
                    // 横向排列时 CHbox 的坐标
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal * nodeIndex;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + 55 * (forward ? 1 : -1);
                }
            }
            else if (parent is MyNodeCH)
            {
                if (node is MyNodeYP)   // YP || CP节点
                {
                    // CH 节点的第一个子节点是 NodeYP 节点
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + 55;
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    // 横向排列时 CHbox 下方节点的坐标
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + 50;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + 90 * (forward ? 1 : -1);
                }
            }
            if (ptf.Y < topY)
            {
                offsetY = topY - AddFlowExtension.FloatConverter(ptf.Y);
                ptf.Y = topY;
            }
            return ptf;
        }

        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <returns></returns>
        public Point GetSymmetriesLayoutNodeLocationVertical(Node parent, Node node, bool forward, float leftX, out float offsetX)
        {
            Point parentCenterPosi = getCenterPointF(parent);
            Point ptf = new Point();
            offsetX = 0;

            // 室外机的第一个子节点，横向或纵向排列的位置相同
            if (parent is MyNodeOut)
            {
                ptf.X = parent.Location.X + parent.Size.Width;
                ptf.Y = parent.Location.Y + parent.Size.Height;

                if (node is MyNodeYP)
                {
                    //临时设置，根据子节点调整X坐标
                    ptf.X = 50;
                    ptf.Y = ptf.Y - node.Size.Height / 2 + VDistanceHorizontal - 22;
                }
                else if (node is MyNodeIn)
                {
                    ptf.X += 50;
                    ptf.Y += 40;
                }
                else if (node is MyNodeCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height + 150;
                }
                return ptf;
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                int nodeIndex = parentYP.GetIndex(node);    // 获取子节点的索引号

                if (node is MyNodeYP)   // YP || CP节点
                {
                    // YP 节点的第一个子节点是 NodeYP 节点
                    if (nodeIndex == 0)
                    {
                        //暂定X坐标大于topX一个位置，需要根据子节点的X坐标重新计算
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceVertical * (forward ? 1 : -1);
                        ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                    }
                    else
                    {
                        //暂定X坐标跟前一个YP相等，需要根据子节点的X坐标重新计算
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                        ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceVertical;
                    }
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    // 纵向排列时室内机的坐标
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceVertical * (forward ? 1 : -1);
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceVertical * nodeIndex;
                }
                else if (node is MyNodeCH)
                {
                    // 纵向排列时 CHbox 的坐标
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + (HDistanceVertical - 10) * (forward ? 1 : -1);
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceVertical * nodeIndex;
                }
            }
            else if (parent is MyNodeCH)
            {
                if (node is MyNodeYP)   // YP || CP节点
                {
                    // CHBox 直接连接一个YP/CP
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + (HDistanceVertical + 5) * (forward ? 1 : -1);
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    // CHBox 直接连接一个IDU
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + (HDistanceVertical + 15) * (forward ? 1 : -1);
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                }
            }
            if (ptf.X < leftX)
            {
                offsetX = leftX - AddFlowExtension.FloatConverter(ptf.X);
                ptf.X = leftX;
            }
            return ptf;
        }

        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <param name="isVertical">是否是竖直方向排版</param>
        /// <returns></returns>
        public Point GetSchemaALayoutNodeLocationHorizontal(Node parent, Node node, bool orientateUp, float minY, ref float maxX, out float offsetY)
        {
            Point parentCenterPosi = getCenterPointF(parent);
            Point ptf = new Point();
            offsetY = 0;
            
            // 室外机的第一个子节点，横向或纵向排列的位置相同
            if (parent is MyNodeOut)
            {
                ptf.X = AddFlowExtension.FloatConverter(parent.Location.X) + AddFlowExtension.FloatConverter(parent.Size.Width);
                ptf.Y = parent.Location.Y + parent.Size.Height;

                if (node is MyNodeYP)
                {
                    //ACC-RAG from outdoor to 1st Ybranch
                    ptf.X = 200;
                    //node.RotationAngle = 180;
                    //临时设置，根据子节点调整Y坐标
                    if ((node as MyNodeYP).IsCP)
                    {
                        ptf.Y = ptf.Y - node.Size.Height / 2 + VDistanceHorizontal - 22;
                    }
                    else
                    {
                        ptf.Y = ptf.Y - node.Size.Height / 2 + VDistanceHorizontal * 1;
                    }
                }
                else if (node is MyNodeIn)
                {
                    ptf.X += 50;
                    ptf.Y += 70;
                }
                else if (node is MyNodeCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parent.Location.Y + parent.Size.Height + 65;
                }
                else if (node is MyNodeMultiCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parent.Location.Y + parent.Size.Height + 65;
                }
                return ptf;
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                int nodeIndex = parentYP.GetIndex(node);    // 获取子节点的索引号

                if (node is MyNodeYP)   // YP || CP节点
                {
                    // YP 节点的第一个子节点是 NodeYP 节点
                    if (nodeIndex == 0)
                    {
                        //ACC-RAG
                        ptf.X = parentCenterPosi.X + 150;
                        //ptf.X = Math.Max(maxX, parentCenterPosi.X) - node.Size.Width / 2 + HDistanceHorizontal;//Set Two yp nodes
                        //暂定Y坐标大于topY一个位置，需要根据子节点的Y坐标重新计算
                        ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + (VDistanceHorizontal - 40) * 3;// (orientateUp ? -1 : 1);
                        //node.RotationAngle = -90;
                    }
                    else
                    {
                        ptf.X = parentCenterPosi.X + 150;
                        //ptf.X = Math.Max(maxX, parentCenterPosi.X) - node.Size.Width / 2 + HDistanceHorizontal;
                        //暂定Y坐标跟前一个YP相等，需要根据子节点的Y坐标重新计算
                        ptf.Y = parentCenterPosi.Y - node.Size.Height / 2;
                        //node.RotationAngle = -180;
                    }
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    maxX = Math.Max(maxX, AddFlowExtension.FloatConverter(parentCenterPosi.X)) + (nodeIndex > 0 ? HDistanceHorizontal + 45 : 35);
                    //ACC-RAG
                    if (nodeIndex == 0)
                    {
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2 + 4;
                    }
                    else
                    {
                        ptf.X = parentCenterPosi.X - node.Size.Width / 2 + 170;
                    }                    
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceHorizontal * (orientateUp ? -1 : 1);
                }
                else if (node is MyNodeCH)
                {
                    //maxX = Math.Max(maxX, AddFlowExtension.FloatConverter(parentCenterPosi.X)) + (nodeIndex > 0 ? HDistanceHorizontal + 60 : 0);
                    //ptf.X = Math.Max(maxX, parentCenterPosi.X) - node.Size.Width / 2 + HDistanceHorizontal * nodeIndex;
                    if(nodeIndex == 0)
                    {
                        ptf.X = (parent.Location.X - node.Size.Width / 2) + 20;
                        ptf.Y = (parent.Location.Y - node.Size.Height / 2 + (VDistanceHorizontal - 20) * (orientateUp ? -1 : 1)) - 30;
                    }
                    else
                    {
                        ptf.X = parent.Location.X + node.Size.Width * 5;
                        ptf.Y = (parent.Location.Y - node.Size.Height / 2 + (VDistanceHorizontal - 20) * (orientateUp ? -1 : 1)) - 30;
                    }
                   
                }
                else if (node is MyNodeMultiCH)
                {
                    // 横向排列时 Multi CHbox 的坐标
                    maxX = Math.Max(maxX, AddFlowExtension.FloatConverter(parentCenterPosi.X)) + (nodeIndex > 0 ? HDistanceHorizontal : 0);
                    ptf.X = maxX - node.Size.Width / 2;
                    if (orientateUp)
                    {
                        ptf.Y = parentCenterPosi.Y - 67 - node.Size.Height;
                    }
                    else
                    {
                        ptf.Y = parentCenterPosi.Y + 67;
                    }
                }
            }
            else if (parent is MyNodeCH)
            {
                if (node is MyNodeYP)   // YP || CP节点
                {
                    // CH 节点的第一个子节点是 NodeYP 节点
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + (VDistanceHorizontal - 20) * (orientateUp ? -1 : 1);
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    // 横向排列时 CHbox 下方节点的坐标
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 ;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + (VDistanceHorizontal - 20) * (orientateUp ? -1 : 1);
                }
            }
            else if (parent is MyNodeMultiCH)
            {
                if (node is MyNodeYP)   // YP || CP节点
                {
                    ptf.X = Math.Max(maxX, parentCenterPosi.X) - node.Size.Width / 2 + HDistanceHorizontal + 20;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceHorizontal * 0.75f * (orientateUp ? -1 : 1);
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    maxX = Math.Max(maxX, AddFlowExtension.FloatConverter(parentCenterPosi.X)) + HDistanceHorizontal;
                    ptf.X = maxX - node.Size.Width / 2 + 50;
                    ptf.Y = parentCenterPosi.Y - node.Size.Height / 2 + VDistanceHorizontal * (orientateUp ? -1 : 1);
                }
            }

            if (ptf.Y < minY)
            {
                offsetY = minY - AddFlowExtension.FloatConverter(ptf.Y);
                ptf.Y = minY;
            }
            return ptf;
        }

        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置,受横向或纵向的影响
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="node">子节点</param>
        /// <returns></returns>
        public Point GetSchemaALayoutNodeLocationVertical(Node parent, Node node, bool orientateRight, float minX, ref float maxY, out float offsetX)
        {
            Point parentCenterPosi = getCenterPointF(parent);
            Point ptf = new Point();
            offsetX = 0;

            if (parent is MyNodeOut)
            {
                // 室外机的第一个子节点，横向或纵向排列的位置相同
                //ptf.X = parent.Location.X + parent.Size.Width;
                ptf.X = 50;
                ptf.Y = parent.Location.Y + parent.Size.Height;

                if (node is MyNodeYP)
                {
                    if ((node as MyNodeYP).IsCP)
                    {
                        //临时设置，根据子节点调整X坐标
                        ptf.X = 50;
                        ptf.Y = ptf.Y - node.Size.Height / 2 + VDistanceHorizontal - 22;
                    }
                    else
                    {
                        //临时设置，根据子节点调整X坐标
                        ptf.X = parent.Size.Width / 2 - 3;
                        ptf.Y = parent.Location.Y + parent.Size.Height - node.Size.Height / 2 + VDistanceHorizontal - 22;
                    }
                }
                else if (node is MyNodeIn)
                {
                    //一对一连接
                    ptf.X += 180;
                    ptf.Y += 40;
                }
                else if (node is MyNodeCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parent.Location.Y + parent.Size.Height + 65;
                }
                else if (node is MyNodeMultiCH)
                {
                    //第一子节点为CH-Box时，节点位置需要另外计算 
                    ptf.X = parentCenterPosi.X - node.Size.Width / 2 + HDistanceHorizontal;
                    ptf.Y = parent.Location.Y + parent.Size.Height + 10;
                }
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                int nodeIndex = parentYP.GetIndex(node);    // 获取子节点的索引号                

                if (node is MyNodeYP)   // YP || CP节点
                {
                    // YP 节点的第一个子节点是 NodeYP 节点
                    //ACC-RAG
                    if (nodeIndex == 0)
                    {
                        //暂定X坐标大于topX一个位置，需要根据子节点的X坐标重新计算
                        ptf.X = parent.Location.X + 250;                       
                        ptf.Y = parent.Location.Y + 200;// - node.Size.Height / 2 ;                        
                    }
                    else
                    {
                        //暂定X坐标跟前一个YP相等，需要根据子节点的X坐标重新计算
                        ptf.X = parent.Location.X;
                        ptf.Y = parent.Location.Y + 200;                                               
                    }
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    // 纵向排列时室内机的坐标
                    ptf.X = (parent.Location.X - node.Size.Width / 2 + (HDistanceVertical + 20) * (orientateRight ? 1 : -1)) - 50;
                    if (nodeIndex == 0)
                    {
                        ptf.Y = parent.Location.Y + 40; //maxY - node.Size.Height / 2 - YPAdjustmentFactorToAchieveSymmetry;
                    }

                    if (nodeIndex == 1)
                    {
                        ptf.Y = parent.Location.Y + 173;
                    }
                }
                else if (node is MyNodeCH)
                {
                    // 纵向排列时 CHbox 的坐标
                    ptf.X = (parent.Location.X - node.Size.Width / 2 + (HDistanceVertical + 20) * (orientateRight ? 1 : -1)) - 50;
                    ptf.Y = parent.Location.Y + 30;

                    if (nodeIndex > 0)
                    {
                        ptf.X = (parent.Location.X - node.Size.Width / 2 + (HDistanceVertical + 20) * (orientateRight ? 1 : -1)) - 50;
                        ptf.Y = parent.Location.Y + 230;
                    }
                }
                else if (node is MyNodeMultiCH)
                {
                    maxY = Math.Max(maxY + 40, AddFlowExtension.FloatConverter(parent.Location.Y)) + (nodeIndex > 0 ? VDistanceVertical * 1.2f : 0);
                    ptf.X = parent.Location.X - node.Size.Width / 2 + (HDistanceVertical + 20) * (orientateRight ? 1 : -1);
                    ptf.Y = maxY - node.Size.Height / 2;
                }
            }
            else if (parent is MyNodeCH)
            {
                if (node is MyNodeYP)   // YP || CP节点
                {
                    // CHBox 直接连接一个YP/CP
                    ptf.X = parent.Location.X - node.Size.Width / 2 + (HDistanceVertical + 20) * (orientateRight ? 1 : -1);
                    ptf.Y = parent.Location.Y - node.Size.Height / 2;
                }
                else if (node is MyNodeIn)           // OldModel.Indoor节点
                {
                    // CHBox 直接连接一个IDU
                    ptf.X = (parent.Location.X - node.Size.Width / 2) + 15;// + (HDistanceVertical + 40) * (orientateRight ? 1 : -1);
                    ptf.Y = parent.Location.Y + 58;
                }
            }
            else if (parent is MyNodeMultiCH)
            {
                MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                int nodeIndex = parentMCH.GetIndex(node);    // 获取子节点的索引号

                maxY = Math.Max(maxY, AddFlowExtension.FloatConverter(parentCenterPosi.Y)) + VDistanceVertical * 1.2f * (nodeIndex > 0 ? 1 : 0);
                ptf.X = parentCenterPosi.X + (HDistanceVertical * 2) * (orientateRight ? 1 : -1) - node.Size.Width / 2;
                ptf.Y = maxY - node.Size.Height / 2;
            }

            if (ptf.X < minX)
            {
                offsetX = minX - AddFlowExtension.FloatConverter(ptf.X);
                ptf.X = minX;
            }
            return ptf;
        }

        /// 调整连接线的 起始点 坐标，根据其 Org 计算所得
        /// <summary>
        /// 调整连接线的 起始点 坐标，根据其 Org 计算所得
        /// </summary>
        /// <param name="link"> 连接线对象 </param>
        /// <param name="isVertical"> true：竖直布局；false：水平布局 </param>
        /// <returns></returns>
        public Point setLinkStartPoint(Link link, bool isFirst, bool isVertical)
        {
            Node parent = link.Org;
            Point ptf = link.Points[0];

            // 父节点为 室外机
            if (parent is MyNodeOut)
            {
                ptf = getLeftBottomPointF(parent);
            }
            // 父节点为 分歧管
            else if (parent is MyNodeYP)
            {
                MyNodeYP yp = parent as MyNodeYP;
                ptf = getCenterPointF(parent);//getRightCenterPointF(parent); 

                // 竖直布局处理
                if (isVertical)
                {
                    // 分歧管的第一个连接线的起点，为分歧管中点
                    if (isFirst)
                    {
                        //ls = LineStyle.HV;
                        if (yp.IsCP)
                            ptf.Y = getRightTopPointF(parent).Y + 10;
                    }
                    // 分歧管的其他连接线的起点，为分歧管的中心点
                    else
                    {
                        // 使用以下代码时，纵向布局连接到梳形管的连线会出现多条，因此注释掉。 20160705 by Yunxiao Lin
                        //if (yp.IsCP)
                        //{
                        //    ptf = getBottomCenterPointF(parent);
                        //    if (link != parent.OutLinks[1])
                        //    {
                        //        ptf.X -= 3 * (parent.OutLinks.Count - 2);
                        //    }
                        //}
                    }
                }
                // 水平布局处理
                else
                {
                    // TODO: 水平布局处理

                }
            }
            return ptf;
        }

        /// 调整连接线的 终点 坐标，根据其 Org 计算所得
        /// <summary>
        /// 调整连接线的 终点 坐标，根据其 Org 计算所得
        /// </summary>
        /// <param name="link"></param>
        /// <param name="isVertical"></param>
        /// <returns></returns>
        public Point setLinkEndPoint(Link link, bool isVertical)
        {
            Node node = link.Dst;
            Point ptf = link.Points[link.Points.Count - 1];

            if (node is MyNodeIn)
            {
                ptf = getLeftCenterPointF(node); // 目标节点为室内机，取其左边中点
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP yp = node as MyNodeYP;
                if (!yp.IsCP)
                {
                    ptf = getCenterPointF(node);
                }
                else
                {
                    Point ptfStart = link.Points[0];
                    if (!isVertical)
                    {
                        if (ptfStart.X == ptf.X)
                        {
                            ptf = getTopCenterPointF(node);
                        }
                        else
                        {
                            ptf = getLeftCenterPointF(node);
                        }
                    }
                }
            }
            return ptf;
        }

        #endregion

        #region 创建配管图节点对象

        /// 创建 OldModel.Outdoor 节点对象，添加到AddFlow控件中
        /// <summary>
        /// 创建 OldModel.Outdoor 节点对象，添加到AddFlow控件中
        /// </summary>
        /// <param name="sysItem"></param>
        /// <param name="addFlowPiping"></param>
        /// <returns></returns>
        public MyNodeOut createNodeOut()
        {
            MyNodeOut node = new MyNodeOut();
            //node.Tooltip = OldModel.NodeType.OUT.ToString();
            //node.LabelEdit = false;

            //node.DrawColor = colorTransparent;
            //node.FillColor = colorNodeBg;
            ////node.Transparent = true;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);    // To be Fix latter
            //node.ZOrder = 10;
            return node;
        }

        /// 创建 OldModel.Indoor 节点对象
        /// <summary>
        /// 创建 OldModel.Indoor 节点对象
        /// </summary>
        /// <param name="text"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public MyNodeIn createNodeIn(OldModel.RoomIndoor riItem)
        {
            MyNodeIn node = new MyNodeIn();
            //node.Tooltip = OldModel.NodeType.IN.ToString();
            //node.LabelEdit = false;

            //node.DrawColor = colorTransparent;
            //node.FillColor = colorNodeBg;
            ////node.Transparent = true;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);                 // To be Fix latter
            //node.ZOrder = 10;

            // 节点上关联的室内机
            node.RoomIndooItem = riItem;

            // 节点的 InLink 对象
            //node.InLink = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.Tooltip = OldModel.NodeType.MyLink.ToString();
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;
            node.MyInLinks.Add(createMyLink());

            return node;
        }

        /// 创建 CHbox 节点对象
        /// <summary>
        /// 创建 CHbox 节点对象
        /// </summary>
        /// <param name="text"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public MyNodeCH createNodeCHbox(Node child)
        {

            MyNodeCH node = new MyNodeCH();
            node.Tooltip = OldModel.NodeType.CHbox.ToString();
            //node.LabelEdit = false;

            node.Fill = colorTransparent;
            node.Stroke = colorNodeBg;
            //node.Transparent = true;
            //node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 61, 65), 3, 3);
            //node.Geometry = new RectangleGeometry(new System.Windows.Rect(0, 0, 61, 65), 3, 3);

            node.ChildNode = child;
            //node.ZOrder = 10;




            //node.MyInLinks = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.Tooltip = OldModel.NodeType.MyLink.ToString();
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;
            node.MyInLinks.Add(createMyLink());

            return node;
        }

        /// 创建 Multi CHbox 节点对象
        /// <summary>
        /// 创建 Multi CHbox 节点对象
        /// </summary>
        /// <returns></returns>
        public MyNodeMultiCH createNodeMultiCHbox()
        {
            MyNodeMultiCH node = new MyNodeMultiCH();
            //node.Tooltip = OldModel.NodeType.MultiCHbox.ToString();
            //node.LabelEdit = false;

            //node.DrawColor = colorTransparent;
            //node.FillColor = colorNodeBg;
            ////node.Transparent = true;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);      // To be Fix latter

            //node.ZOrder = 10;

            // 节点的 InLink 对象
            //node.InLink = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.Tooltip = OldModel.NodeType.MyLink.ToString();
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;
            node.MyInLinks.Add(createMyLink());

            return node;
        }

        /// 创建 YP 或 CP 节点对象
        /// <summary>
        /// 创建 YP 或 CP 节点对象
        /// </summary>
        /// <param name="isCP">是否是树形管</param>
        /// <param name="count">当前分歧管数量</param>
        /// <returns></returns>
        public MyNodeYP createNodeYP(bool isCP)
        {
            MyNodeYP node = new MyNodeYP(isCP);
            node.Tooltip = isCP ? OldModel.NodeType.CP.ToString() : OldModel.NodeType.YP.ToString();

            node.Size = isCP ? sizeCP : sizeYP;
            // TODO：获取并绑定对应的图片
            //Shape ypShape = new Shape(ShapeStyle.Triangle, ShapeOrientation.so_90);
            //Shape cpShape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);                         // To be Fix latter
            //node.Shape = isCP ? cpShape : ypShape;

            ////node.Text = isCP ? "CP" : "YP";
            //node.FillColor = colorYP;
            //node.DrawColor = colorTransparent;
            //node.ZOrder = 10;

            // 节点的 InLink 对象
            //node.InLink = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.Tooltip = OldModel.NodeType.MyLink.ToString();
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;

            node.PinsLayout = new PointCollection();
            if (!isCP)
            {
                SetNodeYpPinLayOut(node);
            }
            else
            {
                node.Geometry =
                    Geometry.Parse(
                        "M56.274 15V7.013H51.96V15h-3.725V7.013h-4.314V15H40.2V7.013h-4.314V15h-3.725V7.013h-4.314V15h-3.725V7.013H19.8V15h-3.726V7.013H11.76V15H8.035V7.013H3.721V15H0V3.507h60V15zM27.647 3.507V0h4.706v3.507z");
                var q = 100 / 8;
                node.PinsLayout.Add(new Point(50, 0));
                node.PinsLayout.Add(new Point(q - 10, 100));
                node.PinsLayout.Add(new Point((2 * q) - 9, 100));
                node.PinsLayout.Add(new Point((3 * q) - 8, 100));
                node.PinsLayout.Add(new Point((4 * q) - 6, 100));
                node.PinsLayout.Add(new Point((5 * q) - 4, 100));
                node.PinsLayout.Add(new Point((6 * q) - 3, 100));
                node.PinsLayout.Add(new Point(7 * q, 100));
                node.PinsLayout.Add(new Point(8 * q, 100));
            }

            node.Fill = colorYP;
            node.InConnectionMode = ConnectionMode.Pin;
            node.OutConnectionMode = ConnectionMode.Pin;

            node.Text = string.Empty;
            node.MyInLinks.Add(createMyLink());

            return node;
        }

        public void SetNodeYpPinLayOut(MyNodeYP ypNode)
        {
            if(ypNode==null) return;

            ypNode.PinsLayout = new PointCollection();

            //Horizontal|HorizontalMirror|VerticalMirror|Vertical
            switch (ypNode.YBranchOrientation)
            {
                case "HorizontalMirror":
                    ypNode.PinsLayout.Add(new Point(0, 75));
                    ypNode.PinsLayout.Add(new Point(60, 0));
                    ypNode.PinsLayout.Add(new Point(100, 75));
                    break;

                case "VerticalMirror":
                    ypNode.PinsLayout.Add(new Point(20, 0));
                    ypNode.PinsLayout.Add(new Point(20, 100));
                    ypNode.PinsLayout.Add(new Point(100, 60));
                    break;

                case "Vertical":
                    ypNode.PinsLayout.Add(new Point(0, 60));
                    ypNode.PinsLayout.Add(new Point(80, 100));
                    ypNode.PinsLayout.Add(new Point(80, 0));
                    break;
                default:
                    ypNode.PinsLayout.Add(new Point(0, 30));
                    ypNode.PinsLayout.Add(new Point(60, 100));
                    ypNode.PinsLayout.Add(new Point(100, 30));
                    break;
            }            
        }

        public MyNodeYP createNodeYP4BranchKit()
        {
            MyNodeYP node = new MyNodeYP(4);
            node.Tooltip = OldModel.NodeType.YP4.ToString();
            node.Geometry =
                Geometry.Parse(
                    "M-.138 24V5.677h17.791V0h7.325v5.677h19.884V24h-6.279V12.13h-6.28V24h-7.326V12.387h-6.279V24h-6.279V12.13h-6.28V24H-.138z");
            var q = 100 / 4;
            node.PinsLayout = new PointCollection();
            node.PinsLayout.Add(new Point(45, 0));
            node.PinsLayout.Add(new Point(q - 15, 100));
            node.PinsLayout.Add(new Point((2 * q) - 15, 100));
            node.PinsLayout.Add(new Point((3 * q) - 10, 100));
            node.PinsLayout.Add(new Point((4 * q) - 10, 100));
            node.Fill = colorYP;
            node.Text = string.Empty;
            node.InConnectionMode = ConnectionMode.Pin;
            node.OutConnectionMode = ConnectionMode.Pin;
            node.MyInLinks.Add(createMyLink());
            node.Size = new System.Windows.Size(45, 24);
            return node;
        }

        public MyLink createMyLink()
        {
            MyLink myLink = new MyLink();
            myLink.Tooltip = OldModel.NodeType.MyLink.ToString();
            //myLink. = ConnectionStyle.Inside;        // Need to do implementation
            // myLink.ArrowGeometryDst = Geometry.;
            myLink.IsAdjustDst = true;
            myLink.IsAdjustOrg = true;
            //// myLink.ZOrder = 5;
            //t
            myLink.StrokeThickness = 1;
            return myLink;
        }
        #endregion

        /// 检测两节点是否具有包含关系
        /// <summary>
        /// 检测两节点是否具有包含关系
        /// </summary>
        /// <param name="pn">父节点</param>
        /// <param name="an">子节点</param>
        /// <returns></returns>
        public bool isContain(Node pn, Node an)
        {
            if (pn == an)
                return true;

            if (pn is MyNodeOut)
            {
                MyNodeOut nodeOut = pn as MyNodeOut;
                return isContain(nodeOut.ChildNode, an);
            }
            else if (pn is MyNodeYP)
            {
                MyNodeYP nodeYP = pn as MyNodeYP;
                foreach (Node child in nodeYP.ChildNodes)
                {
                    if (isContain(child, an))
                        return true;
                }
            }
            else if (pn is MyNodeCH)
            {
                MyNodeCH nodeCH = pn as MyNodeCH;
                return isContain(nodeCH.ChildNode, an);
            }
            else if (pn is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = pn as MyNodeMultiCH;
                foreach (Node child in nodeMCH.ChildNodes)
                {
                    if (isContain(child, an))
                        return true;
                }
            }
            return false;
        }
        #region NextGen Methods
        public PointF getLeftCenterPointFNextGen(Node node)
        {
            float fx = AddFlowExtension.FloatConverter(node.Location.X);
            float fy = (AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height) / 2);
            return new PointF(fx, fy);
        }
        public PointF getRightTopPointFNextGen(Node node)
        {
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            float fy = AddFlowExtension.FloatConverter(node.Location.Y);
            return new PointF(fx, fy);
        }
        public PointF getRightBottomPointFNextGen(Node node)
        {
            float fx = AddFlowExtension.FloatConverter(node.Location.X) + AddFlowExtension.FloatConverter(node.Size.Width);
            float fy = AddFlowExtension.FloatConverter(node.Location.Y) + AddFlowExtension.FloatConverter(node.Size.Height);
            return new PointF(fx, fy);
        }
        public NodeElement_Wiring GetNodeElement_Wiring_ODUNextGen(OldModel.Outdoor outItem, string brandCode)
        {
            //string[] unitFullName = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            string[] strs1 = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
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

            //获取室外机所有的分机对象 20160521
            OldModel.Outdoor[] outItems = new OldModel.Outdoor[count];
            if (count > 1)
            {
                //对室外机分机按从大到小进行排序 20161027 by Yunxiao Lin
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
                //JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, outItem.ProductType, brandCode);
                //由于增加了多ProductType功能，productType参数变得不确定，所以取消该参数 20160823 by Yunxiao Lin
                JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, brandCode);
                for (int i = 0; i < count; i++)
                {
                    string modelname = (unitFullName[i]).Trim();
                    OldModel.Outdoor coutItem = null;
                    //if (outItem.RegionCode == "MAL") //马来西亚的机组名称用的是Model_York,需要特殊处理 20160629 Yunxiao Lin
                    //if (outItem.RegionCode == "MAL" && brandCode == "Y") //只有马来西亚的约克品牌需要这样处理 20161023 by Yunxiao Lin 
                    //if (((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV") && brandCode == "Y") || (outItem.Series == "Commercial VRF HP, YVAHP") || (outItem.Series == "Commercial VRF HR, YVAHR")) //马来西亚的YORK品牌以及北美机型需要这样处理 20170323 by Yunxiao Lin
                    if (((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV" || outItem.RegionCode == "ASIA" || outItem.RegionCode == "BGD" || outItem.RegionCode == "SEA_60Hz") && brandCode == "Y") || (outItem.Series == "Commercial VRF HP, YVAHP") || (outItem.Series == "Commercial VRF HR, YVAHR")) //马来西亚的YORK品牌以及北美机型需要这样处理 20170323 by Yunxiao Lin
                    {
                        // 玻利维亚的York产品线和马来西亚的一样 20170629 by Yunxiao Lin
                        //coutItem = dal.GetYorkItem(modelname);
                        // 需要指定productType参数 20160823 by Yunxiao Lin
                        //coutItem = dal.GetYorkItem(modelname, outItem.ProductType);
                        // ProductType参数改为 Series参数 20170406 by Yunxiao Lin
                        coutItem = dal.GetYorkItemBySeries(modelname, outItem.Series);
                    }
                    else if (brandCode == "H")
                        //coutItem = dal.GetHitachiItem(modelname);
                        //coutItem = dal.GetHitachiItem(modelname, outItem.ProductType);
                        // ProductType参数改为 Series参数 20170406 by Yunxiao Lin
                        coutItem = dal.GetHitachiItemBySeries(modelname, outItem.Series);
                    else
                        //coutItem = dal.GetItem(modelname);
                        //coutItem = dal.GetItem(modelname, outItem.ProductType);
                        // ProductType参数改为 Series参数 20170406 by Yunxiao Lin
                        coutItem = dal.GetItemBySeries(modelname, outItem.Series);
                    if (coutItem != null)
                    {
                        outItems[i] = coutItem;
                    }
                }
            }
            else
                outItems[0] = outItem;
            //根据室外机分机型号名获取电源参数 20160521 by Yunxiao Lin
            DAL.MyDictionaryDAL dicdal = new DAL.MyDictionaryDAL();
            string strGroup2 = "";
            string strGroup3 = "";
            string strGroup4 = "";
            string modelGroup = "";
            foreach (OldModel.Outdoor coutItem in outItems)
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
                    //OldModel.MyDictionary dic = dicdal.GetItem(OldModel.MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(10, 1));
                    OldModel.MyDictionary dic = dicdal.GetItem(OldModel.MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1));
                    //string strPowerLineType = dic.Name.Contains("3Ph") ? "///" : "//"; //电源线3芯2芯
                    //string strPowerPhaseNumber = "L1L2L3N"; // 三相>380V
                    //if (strPowerLineType == "//")
                    //{
                    //    strPowerPhaseNumber = "R S"; //单相<380V
                    //}
                    string strPowerLineType = "//"; //电源线2芯
                    string strPowerPhaseNumber = "R S"; //单相<380V
                    if (dic.Name.Contains("3Ph"))
                    {
                        strPowerLineType = "////"; //电源线3芯 显示4根线  modified by Shen Junjie on 2017/12/21
                        strPowerPhaseNumber = "L1L2L3N"; // 三相>380V 
                    }
                    //if (coutItem.ModelFull.Substring(10, 1) == "R")
                    if (coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1) == "R")
                    {
                        strPowerPhaseNumber = "R S T"; //三相<380V
                        strPowerLineType = "///"; //电源线3芯 add by Shen Junjie on 2017/12/21
                    }
                    string strPower = dic.Name.Replace("1Ph/", "").Replace("3Ph/", ""); //电源标识
                    double current = coutItem.MaxCurrent; // 电流值
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
                //else // York 
                //{
                //    for (int i = 0; i < unitFullName.Count; ++i)
                //    {
                //        string s = unitFullName[i].Substring(0, 7);
                //        unitFullName[i] = s;
                //    }
                //}
                modelGroup = string.Join(",", unitFullName);
            }
            else
            {
                //YVAHP显示Model_York 20160626 Yunxiao Lin
                model = outItem.Model_York;
            }

            NodeElement_Wiring item = new NodeElement_Wiring("Out" + count.ToString(), model, count, modelGroup, strGroup2, strGroup3, strGroup4, brandCode);
            return item;
        }

        public NodeElement_Wiring GetNodeElement_Wiring_IDUNextGen(OldModel.Indoor inItem, string brandCode, string outType, ref List<string> strArrayList_powerType, ref List<string> strArrayList_powerVoltage, ref List<double> dArrayList_powerCurrent, ref int powerIndex, ref bool isNewPower)
        {
            //string model = inItem.Model;
            string model = inItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            if (brandCode == "H")
                model = inItem.Model_Hitachi;
            if (outType.Contains("YVAHP") || outType.Contains("YVAHR"))
                model = inItem.Model_York;
            //根据室内机型号获取电源参数 20160521 by Yunxiao Lin
            string powerSupplyCode = inItem.ModelFull.Substring(inItem.ModelFull.Length - 4, 1);
            DAL.MyDictionaryDAL dicdal = new DAL.MyDictionaryDAL();
            OldModel.MyDictionary dic = dicdal.GetItem(OldModel.MyDictionary.DictionaryType.PowerSupply, powerSupplyCode);
            //string strPowerLineType = dic.Name.Contains("3Ph") ? "///" : "//"; //电源线3芯2芯
            string strPowerLineType = "//"; //电源线2芯
            string strPowerPhaseNumber = "R S"; //单相<380V
            if (dic.Name.Contains("3Ph"))
            {
                strPowerLineType = "////"; //电源线3芯 显示4根线  modified by Shen Junjie on 2017/12/21
                strPowerPhaseNumber = "L1L2L3N"; // 三相>380V 
            }
            if (powerSupplyCode == "R")
            {
                strPowerLineType = "///"; //add by Shen Junjie on 2017/12/21
                strPowerPhaseNumber = "R S T"; //三相<380V
            }
            string strPower = dic.Name;

            //strPower = strPower.Replace(",", " ");
            bool isFound = false;

            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                //if (strPowerPhaseNumber == strArrayList_powerType[i] && strPower == strArrayList_powerVoltage[i])
                //{
                //    isFound = true;
                //    powerIndex = i;
                //    break;
                //}

                //取得电源电压的共同点，如果有共同点则可以合并成一条总线 add by Shen Junjie 2018/5/2
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
                string strPowerPhaseNumber1 = "R S"; //单相<380V
                if (strPower.Contains("3Ph"))
                {
                    strPowerPhaseNumber1 = "L1L2L3N"; // 三相>380V 
                }
                if (powerSupplyCode == "R")
                {
                    strPowerPhaseNumber1 = "R S T"; //三相<380V
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

            strPower = strPower.Replace("/1Ph/", "/").Replace("/3Ph/", "/"); //电源标识

            double current = inItem.RatedCurrent; // 电流值
            dArrayList_powerCurrent[powerIndex] += current;
            strPower += "/" + current.ToString() + "A";
            NodeElement_Wiring item = new NodeElement_Wiring("Ind", model, 1, model, strPowerPhaseNumber, strPower, strPowerLineType, brandCode);
            return item;
        }
        #endregion NextGen Methods

        #region 创建配线图节点对象

        /// 创建 OldModel.Outdoor 节点对象
        /// <summary>
        /// 创建 OldModel.Outdoor 节点对象
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public WiringNodeOut createNodeOut_wiring()
        {
            WiringNodeOut node = new WiringNodeOut();
            //node.Tooltip = OldModel.NodeType.OUT.ToString();
            //node.LabelEdit = false;
            //node.DrawColor = colorTransparent;
            //node.Font = textFont_wiring;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            //node.ZOrder = 10;

            return node;

        }

        /// 创建 OldModel.Indoor 节点对象
        /// <summary>
        /// 创建 OldModel.Indoor 节点对象
        /// </summary>
        /// <param name="text"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public WiringNodeIn createNodeIn_wiring(OldModel.RoomIndoor riItem)
        {
            WiringNodeIn node = new WiringNodeIn();
            //node.Tooltip = OldModel.NodeType.IN.ToString();
            //node.LabelEdit = false;
            //node.DrawColor = colorTransparent;
            //node.Font = textFont_wiring;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            //node.ZOrder = 10;

            // 节点上关联的室内机
            node.RoomIndoorItem = riItem;
            //// 节点的 InLink 对象
            //node.InLink = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.Tooltip = OldModel.NodeType.MyLink.ToString();
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;

            return node;
        }
        /// 创建 CH Unit 布线节点对象 add on 20160519 by Yunxiao Lin
        /// <summary>
        /// 创建 CH Unit 布线节点对象
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public WiringNodeCH createNodeCH_wiring(string model)
        {
            WiringNodeCH node = new WiringNodeCH();
            //node.Tooltip = OldModel.NodeType.CHbox.ToString();
            //node.LabelEdit = false;
            //node.DrawColor = colorTransparent;
            //node.Font = textFont_wiring;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            //node.ZOrder = 10;
            //复制CH节点属性
            node.Model = model;
            ////节点的 InLink 对象
            //node.InLink = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;
            return node;
        }

        /// 创建 CH Unit 布线节点对象 add on 20160519 by Yunxiao Lin
        /// <summary>
        /// 创建 CH Unit 布线节点对象
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public WiringNodeMultiCH createNodeMCH_wiring(string model)
        {
            WiringNodeMultiCH node = new WiringNodeMultiCH();
            //node.Tooltip = OldModel.NodeType.CHbox.ToString();
            //node.LabelEdit = false;
            //node.DrawColor = colorTransparent;
            //node.Font = textFont_wiring;
            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            //node.ZOrder = 10;
            //复制CH节点属性
            node.Model = model;
            ////节点的 InLink 对象
            //node.InLink = new MyLink();
            //MyLink myLink = node.InLink;
            //myLink.ConnectionStyleOrg = ConnectionStyle.Inside;
            //myLink.ArrowDst = Arrow.None;
            //myLink.AdjustDst = true;
            //myLink.AdjustOrg = true;
            //myLink.ZOrder = 5;
            return node;
        }

        /// 创建点节点
        /// <summary>
        /// 创建点节点
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Node createLinePoint(PointF pt)
        {
            Node node = new Node();
            //node.Selectable = false;
            //node.Size = new SizeF(1, 1);
            //node.Location = pt;
            //node.DrawColor = colorWiring;
            //node.ZOrder = 10;
            return node;
        }

        /// <summary>
        /// 创建AddFlow.Link对象
        /// </summary>
        /// <returns></returns>
        public Link createLine()
        {
            Link lnk = new Link();
            //lnk.DrawWidth = 1;
            //lnk.DrawColor = colorWiring;
            //lnk.ArrowDst = Arrow.None;
            //lnk.Stretchable = false;
            //lnk.Selectable = false;
            //lnk.OwnerDraw = true;
            //lnk.Line.Style = LineStyle.Polyline;
            //lnk.ZOrder = 10;
            return lnk;
        }

        /// 设置 Wiring 节点的显示内容
        /// <summary>
        /// 设置 Wiring 节点的显示内容
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ndName">Out1;Ind1...</param>
        /// <param name="ndCurrent">电流值参数</param>
        /// <param name="addFlowWiring"></param>
        /// <param name="item_wiring"></param>
        public void setNode_wiring(Node node, string imgFile, AddFlow addFlowWiring)
        {
            //addFlowWiring.Nodes.Add(node);

            //Image img = new Bitmap(imgFile);
            //addFlowWiring.Images.Add(img);
            //node.ImageIndex = addFlowWiring.Images.Count - 1;
            //node.ImagePosition = ImagePosition.CenterBottom;
            //node.Size = new SizeF(img.Size.Width, img.Size.Height);
            //node.BackMode = BackMode.Transparent;
            //node.Selectable = false;
            //node.ZOrder = 10;
        }

        /// 设置 Wiring 节点的显示内容
        /// <summary>
        /// 设置 Wiring 节点的显示内容
        /// </summary>
        /// <param name="node"></param>
        /// <param name="addFlowWiring"></param>
        public void setNode_wiring(Node node, SizeF size, AddFlow addFlowWiring)
        {
            //addFlowWiring.Nodes.Add(node);

            //node.Shape = new Shape(ShapeStyle.Rectangle, ShapeOrientation.so_0);
            ////node.DashStyle = DashStyle.Solid;
            //node.DrawColor = colorWiring;
            //node.Size = size;
            //node.BackMode = BackMode.Transparent;
            //node.Selectable = false;
            //node.ZOrder = 10;
        }

        /// 根据 parent 节点，计算其对应子节点的位置
        /// <summary>
        /// 根据 parent 节点，计算其对应子节点的位置
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <param name="isVertical"></param>
        /// <returns></returns>
        public Point getLocationChild_wiring(Node parent, Node node, int nodeIndex, bool isHeatRecovery)
        {
            Point ptf = new Point();
            //50 室内机与室外机WIRING 之间的间距
            if (parent is WiringNodeOut)
                ptf.X = parent.Location.X + HDistanceVertical_wiring + 60;
            else
                ptf.X = parent.Location.X + HDistanceVertical_wiring + 10;

            if (isHeatRecovery && parent is WiringNodeOut && node is WiringNodeIn)
            {
                ptf.X += HDistanceVertical_wiring + 60;  //和CH Box后面的OldModel.Indoor对齐
            }
            if (node is WiringNodeCH)
            {
                ptf.X += 60;
            }
            ptf.Y = parent.Location.Y - UtilPiping.OutdoorOffset_Y_wiring + VDistanceVertical_wiring * nodeIndex;//HeightForNodeText
            return ptf;
        }

        #endregion

        #region 绘制Wiring图

        /// 保存配线图中每条连接线上的点坐标
        /// <summary>
        /// 保存配线图中每条连接线上的点坐标
        /// </summary>
        List<PointF[]> ptArrayList = new List<PointF[]>();

        /// 保存配线图中每虚线上的点坐标
        /// <summary>
        /// 保存配线图中每虚线上的点坐标
        /// </summary>
        List<PointF[]> ptArrayList_dash = new List<PointF[]>();


        /// 获取室外机Wiring图节点对象 Modify on 20160521 by Yunxiao Lin
        /// <summary>
        /// 获取室外机Wiring图节点对象
        /// </summary>
        /// <param name="outItem"></param>
        /// <returns></returns>
        public NodeElement_Wiring GetNodeElement_Wiring_ODU(OldModel.Outdoor outItem, string brandCode)
        {
            //string[] unitFullName = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            string[] strs1 = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
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

            //获取室外机所有的分机对象 20160521
            OldModel.Outdoor[] outItems = new OldModel.Outdoor[count];
            if (count > 1)
            {
                //对室外机分机按从大到小进行排序 20161027 by Yunxiao Lin
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
                //JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, outItem.ProductType, brandCode);
                //由于增加了多ProductType功能，productType参数变得不确定，所以取消该参数 20160823 by Yunxiao Lin
                JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, brandCode);
                for (int i = 0; i < count; i++)
                {
                    string modelname = (unitFullName[i]).Trim();
                    OldModel.Outdoor coutItem = null;
                    //if (outItem.RegionCode == "MAL") //马来西亚的机组名称用的是Model_York,需要特殊处理 20160629 Yunxiao Lin
                    //if (outItem.RegionCode == "MAL" && brandCode == "Y") //只有马来西亚的约克品牌需要这样处理 20161023 by Yunxiao Lin 
                    //if (((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV") && brandCode == "Y") || (outItem.Series == "Commercial VRF HP, YVAHP") || (outItem.Series == "Commercial VRF HR, YVAHR")) //马来西亚的YORK品牌以及北美机型需要这样处理 20170323 by Yunxiao Lin
                    if (((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV" || outItem.RegionCode == "ASIA" || outItem.RegionCode == "BGD" || outItem.RegionCode == "SEA_60Hz") && brandCode == "Y") || (outItem.Series == "Commercial VRF HP, YVAHP") || (outItem.Series == "Commercial VRF HR, YVAHR")) //马来西亚的YORK品牌以及北美机型需要这样处理 20170323 by Yunxiao Lin
                    {
                        // 玻利维亚的York产品线和马来西亚的一样 20170629 by Yunxiao Lin
                        //coutItem = dal.GetYorkItem(modelname);
                        // 需要指定productType参数 20160823 by Yunxiao Lin
                        //coutItem = dal.GetYorkItem(modelname, outItem.ProductType);
                        // ProductType参数改为 Series参数 20170406 by Yunxiao Lin
                        coutItem = dal.GetYorkItemBySeries(modelname, outItem.Series);
                    }
                    else if (brandCode == "H")
                        //coutItem = dal.GetHitachiItem(modelname);
                        //coutItem = dal.GetHitachiItem(modelname, outItem.ProductType);
                        // ProductType参数改为 Series参数 20170406 by Yunxiao Lin
                        coutItem = dal.GetHitachiItemBySeries(modelname, outItem.Series);
                    else
                        //coutItem = dal.GetItem(modelname);
                        //coutItem = dal.GetItem(modelname, outItem.ProductType);
                        // ProductType参数改为 Series参数 20170406 by Yunxiao Lin
                        coutItem = dal.GetItemBySeries(modelname, outItem.Series);
                    if (coutItem != null)
                    {
                        outItems[i] = coutItem;
                    }
                }
            }
            else
                outItems[0] = outItem;
            //根据室外机分机型号名获取电源参数 20160521 by Yunxiao Lin
            DAL.MyDictionaryDAL dicdal = new DAL.MyDictionaryDAL();
            string strGroup2 = "";
            string strGroup3 = "";
            string strGroup4 = "";
            string modelGroup = "";
            foreach (OldModel.Outdoor coutItem in outItems)
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
                    //OldModel.MyDictionary dic = dicdal.GetItem(OldModel.MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(10, 1));
                    OldModel.MyDictionary dic = dicdal.GetItem(OldModel.MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1));
                    //string strPowerLineType = dic.Name.Contains("3Ph") ? "///" : "//"; //电源线3芯2芯
                    //string strPowerPhaseNumber = "L1L2L3N"; // 三相>380V
                    //if (strPowerLineType == "//")
                    //{
                    //    strPowerPhaseNumber = "R S"; //单相<380V
                    //}
                    string strPowerLineType = "//"; //电源线2芯
                    string strPowerPhaseNumber = "R S"; //单相<380V
                    if (dic.Name.Contains("3Ph"))
                    {
                        strPowerLineType = "////"; //电源线3芯 显示4根线  modified by Shen Junjie on 2017/12/21
                        strPowerPhaseNumber = "L1L2L3N"; // 三相>380V 
                    }
                    //if (coutItem.ModelFull.Substring(10, 1) == "R")
                    if (coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1) == "R")
                    {
                        strPowerPhaseNumber = "R S T"; //三相<380V
                        strPowerLineType = "///"; //电源线3芯 add by Shen Junjie on 2017/12/21
                    }
                    string strPower = dic.Name.Replace("1Ph/", "").Replace("3Ph/", ""); //电源标识
                    double current = coutItem.MaxCurrent; // 电流值
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
                //else // York 
                //{
                //    for (int i = 0; i < unitFullName.Count; ++i)
                //    {
                //        string s = unitFullName[i].Substring(0, 7);
                //        unitFullName[i] = s;
                //    }
                //}
                modelGroup = string.Join(",", unitFullName);
            }
            else
            {
                //YVAHP显示Model_York 20160626 Yunxiao Lin
                model = outItem.Model_York;
            }

            NodeElement_Wiring item = new NodeElement_Wiring("Out" + count.ToString(), model, count, modelGroup, strGroup2, strGroup3, strGroup4, brandCode);
            return item;
        }
        /// 获取室内机布线对象 Modify on 20160521 by Yunxiao Lin
        /// <summary>
        /// 获取室内机布线对象
        /// </summary>
        /// <param name="inItem"></param>
        /// <param name="brandCode"></param>
        /// <returns></returns>
        public NodeElement_Wiring GetNodeElement_Wiring_IDU(OldModel.Indoor inItem, string brandCode, string outType, ref List<string> strArrayList_powerType, ref List<string> strArrayList_powerVoltage, ref List<double> dArrayList_powerCurrent, ref int powerIndex, ref bool isNewPower)
        {
            //string model = inItem.Model;
            string model = inItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            if (brandCode == "H")
                model = inItem.Model_Hitachi;
            if (outType.Contains("YVAHP") || outType.Contains("YVAHR"))
                model = inItem.Model_York;
            //根据室内机型号获取电源参数 20160521 by Yunxiao Lin
            string powerSupplyCode = inItem.ModelFull.Substring(inItem.ModelFull.Length - 4, 1);
            DAL.MyDictionaryDAL dicdal = new DAL.MyDictionaryDAL();
            OldModel.MyDictionary dic = dicdal.GetItem(OldModel.MyDictionary.DictionaryType.PowerSupply, powerSupplyCode);
            //string strPowerLineType = dic.Name.Contains("3Ph") ? "///" : "//"; //电源线3芯2芯
            string strPowerLineType = "//"; //电源线2芯
            string strPowerPhaseNumber = "R S"; //单相<380V
            if (dic.Name.Contains("3Ph"))
            {
                strPowerLineType = "////"; //电源线3芯 显示4根线  modified by Shen Junjie on 2017/12/21
                strPowerPhaseNumber = "L1L2L3N"; // 三相>380V 
            }
            if (powerSupplyCode == "R")
            {
                strPowerLineType = "///"; //add by Shen Junjie on 2017/12/21
                strPowerPhaseNumber = "R S T"; //三相<380V
            }
            string strPower = dic.Name;

            //strPower = strPower.Replace(",", " ");
            bool isFound = false;

            for (int i = 0; i < strArrayList_powerType.Count; i++)
            {
                //if (strPowerPhaseNumber == strArrayList_powerType[i] && strPower == strArrayList_powerVoltage[i])
                //{
                //    isFound = true;
                //    powerIndex = i;
                //    break;
                //}

                //取得电源电压的共同点，如果有共同点则可以合并成一条总线 add by Shen Junjie 2018/5/2
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
                string strPowerPhaseNumber1 = "R S"; //单相<380V
                if (strPower.Contains("3Ph"))
                {
                    strPowerPhaseNumber1 = "L1L2L3N"; // 三相>380V 
                }
                if (powerSupplyCode == "R")
                {
                    strPowerPhaseNumber1 = "R S T"; //三相<380V
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

            strPower = strPower.Replace("/1Ph/", "/").Replace("/3Ph/", "/"); //电源标识

            double current = inItem.RatedCurrent; // 电流值
            dArrayList_powerCurrent[powerIndex] += current;
            strPower += "/" + current.ToString() + "A";
            NodeElement_Wiring item = new NodeElement_Wiring("Ind", model, 1, model, strPowerPhaseNumber, strPower, strPowerLineType, brandCode);
            return item;
        }

        /// <summary>
        /// 根据两个电源类型得到他们的交集
        /// </summary>
        /// <param name="power1"></param>
        /// <param name="power2"></param>
        /// <returns></returns>
        private string GetCompatiblePowerVoltage(string power1, string power2)
        {
            if (power1 == power2) return power1;

            //string[] powerArr1 = power1.Split(';');
            //string[] powerArr2 = power2.Split(';');
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
                    //match1.Groups[0].Value  "220~240V/1Ph/50,60Hz"
                    //match1.Groups[1].Value  "220~240"
                    //match1.Groups[2].Value  "220"
                    //match1.Groups[3].Value  "~240"
                    //match1.Groups[4].Value  "240"
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
                        //取电压的交集
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
                    //match1.Groups[5].Value  "1Ph"
                    if (match1.Groups[5].Value != match2.Groups[5].Value) continue;
                    strPh = match1.Groups[5].Value;
                    //match1.Groups[6].Value  "50,60"
                    //match1.Groups[7].Value  "50"
                    //match1.Groups[8].Value  ",60"
                    //match1.Groups[9].Value  "60"
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

        /// <summary>
        /// 获取CH-Box Wiring图节点对象
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public NodeElement_Wiring GetNodeElement_Wiring_CH(string model, string powerSupply, string powerLineType, double powerCurrent)
        {
            string strPowerLineType = "//"; //电源线2芯显示2根线
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
                    strPowerLineType = "//"; //电源线2芯 显示2根线
                }
                else if (powerLineType == "3")
                {
                    strPowerLineType = "///"; //电源线3芯 显示3根线
                }
                else if (powerLineType == "4")
                {
                    strPowerLineType = "////"; //电源线4芯 显示4根线
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

        ///// <summary>
        ///// 获取CH-Box Wiring图节点对象
        ///// </summary>
        ///// <param name="ch"></param>
        ///// <returns></returns>
        //public NodeElement_Wiring GetNodeElement_Wiring_CH(string model)
        //{
        //    NodeElement_Wiring item = new NodeElement_Wiring("CH", model, 1, model, "", "", "", "H");
        //    return item;
        //}

        /// 创建文字节点--Wiring
        /// <summary>
        /// 创建文字节点--Wiring
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pt"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Node createTextNode_wiring(string text, PointF pt, Node parent)
        {
            Node label = new Node();

            // 设置字体并量取文字标题的尺寸
            //gMeasureString.PageUnit = GraphicsUnit.Pixel;            //单位为像素
            //Font ft = textFont_wiring;
            //label.Font = ft;
            //SizeF size = gMeasureString.MeasureString(text, ft);

            //label.Transparent = true;
            //label.Text = text;
            //label.Size = new SizeF(size.Width + 15, size.Height);

            //label.Alignment = Alignment.LeftJustifyTOP;
            //label.DrawColor = Color.Transparent;
            //label.Shape.Style = ShapeStyle.Rectangle;
            //label.Logical = false;
            //label.Selectable = false;
            //label.AttachmentStyle = AttachmentStyle.Item;

            //PointF location = UtilEMF.OffsetLocation(pt, parent.Location);
            //if (parent is WiringNodeCH)
            //{
            //    location.X -= 60;
            //}
            //label.Location = location;

            //parent.AddFlow.Nodes.Add(label);
            //label.Parent = parent;
            return label;
        }

        public Node createTextNode_wiring(string text, PointF pt)
        {
            Node label = new Node();

            // 设置字体并量取文字标题的尺寸
            //gMeasureString.PageUnit = GraphicsUnit.Pixel;            //单位为像素
            //Font ft = textFont_wiring;
            //label.Font = ft;
            //SizeF size = gMeasureString.MeasureString(text, ft);

            //label.Location = pt;
            //label.Transparent = true;
            //label.Text = text;
            //label.Size = new SizeF(size.Width + 15, size.Height);

            //label.Alignment = Alignment.LeftJustifyTOP;
            //label.DrawColor = Color.Transparent;
            //label.Shape.Style = ShapeStyle.Rectangle;
            //label.Logical = false;
            //label.Selectable = false;
            //label.AttachmentStyle = AttachmentStyle.Item;
            return label;
        }


        /// 创建文字节点--Wiring modyfy on 20160521 by Yunxiao Lin
        /// <summary>
        /// 创建文字节点--Wiring
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pt"></param>
        /// <param name="parent"></param>
        /// <param name="isLineMark">是否电源线标记/// | //，如果是，和电源线一样设置为红色</param>
        /// <returns></returns>
        public Node createTextNode_wiring(string text, PointF pt, Node parent, bool isLineMark = false)
        {
            Node label = new Node();

            // 设置字体并量取文字标题的尺寸
            //gMeasureString.PageUnit = GraphicsUnit.Pixel;            //单位为像素
            //Font ft = textFont_wiring;
            //if (isLineMark)
            //{
            //    ft = textFont_wiring_linemark; //如果是电源线标记，需要放大字体
            //}
            //label.Font = ft;
            //SizeF size = gMeasureString.MeasureString(text, ft);

            //label.Transparent = true;
            //label.Text = text;
            //label.Size = new SizeF(size.Width + 15, size.Height);

            //label.Alignment = Alignment.LeftJustifyTOP;
            //if (isLineMark)
            //    label.TextColor = Color.Red;
            //label.DrawColor = Color.Transparent;
            //label.Shape.Style = ShapeStyle.Rectangle;
            //label.Logical = false;
            //label.Selectable = false;
            //label.AttachmentStyle = AttachmentStyle.Item;

            //PointF location = UtilEMF.OffsetLocation(pt, parent.Location);
            //if (parent is WiringNodeCH)
            //{
            //    location.X -= 60;
            //}
            //label.Location = location;

            //parent.AddFlow.Nodes.Add(label);
            //label.Parent = parent;
            return label;
        }

        private void InitWiringNodes(AddFlow addFlowWiring)
        {
            //addFlowWiring.Controls.Clear();
            //addFlowWiring.Nodes.Clear();
            //addFlowWiring.AutoScrollPosition = new Point(0, 0);
        }

        #endregion


        /// 从某branch节点往下遍历搜索，是否存在CH-Box add on 20160509 by Yunxiao Lin
        /// <summary>
        /// 从某branch节点往下遍历搜索，是否存在CH-Box
        /// </summary>
        public bool ExistsCHDownward(Node node)
        {
            if (node is MyNodeCH || node is MyNodeMultiCH)
            {
                return true;
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.ChildCount; i++)
                {
                    if (ExistsCHDownward(nodeYP.ChildNodes[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// 获取室外机所有分机对象 add on 20170217 by Lingjia Qiu
        /// <summary>
        /// 获取室外机所有分机对象
        /// </summary>
        /// <param name="outItem"></param>
        /// <param name="brandCode"></param>
        /// <returns></returns>
        public string[] getOutdoorItemArry(OldModel.Outdoor outItem, string brandCode)
        {
            //string[] unitFullName = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            string[] strs1 = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
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

            //获取室外机所有的分机对象 20160521
            string[] outModelArry = new string[count];
            if (count > 1)
            {
                string modelname = "";

                //对室外机分机按从大到小进行排序 20161027 by Yunxiao Lin
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
                //JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, outItem.ProductType, brandCode);
                //由于增加了多ProductType功能，productType参数变得不确定，所以取消该参数 20160823 by Yunxiao Lin
                JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, brandCode);
                for (int i = 0; i < count; i++)
                {
                    modelname = (unitFullName[i]).Trim();
                    //OldModel.Outdoor coutItem = null; //由于piping需要显示分机的Model_York，所以需要取得分机的完整对象 20180215 by Yunxiao Lin
                    if (brandCode.Equals("Y"))
                    {
                        //if (outItem.Series == "Commercial VRF HP, YVAHP" && outItem.Series == "Commercial VRF HR, YVAHR")
                        //{
                        //    //modelname = modelname.Substring(0, 8);
                        //}
                        //else
                        //    modelname = modelname.Substring(0, 7);
                        //if (!(outItem.RegionCode == "MAL") && !(outItem.RegionCode == "LA_BV") && !(outItem.Series == "Commercial VRF HP, YVAHP") && !(outItem.Series == "Commercial VRF HR, YVAHR"))
                        //{
                        //    coutItem = dal.GetItemBySeries(modelname, outItem.Series);
                        //    if (coutItem != null)
                        //        modelname = coutItem.Model_York;
                        //}
                    }
                    //else
                    //{
                    //    coutItem = dal.GetHitachiItemBySeries(modelname, outItem.Series);
                    //    if (coutItem != null)
                    //        modelname = coutItem.Model_Hitachi;
                    //}
                    if (!string.IsNullOrEmpty(modelname))
                        outModelArry[i] = modelname;

                }
            }
            else if (count == 1)
                outModelArry[0] = ""; //单独一台室内机不需要显示piping分机型号 20180215 by Yunxiao Lin
            return outModelArry;
        }
        public string manageYBranchOrientation(Node parent, Node nodeYP, bool isVertical, string layoutType)
        {
            string YBranchImage = "YBranch";
            MyNodeYP yP = nodeYP as MyNodeYP;
            yP.YBranchOrientation = MyNodeYP.BranchOrientations.Horizontal.ToString();
            switch (layoutType)
            {
                case "Normal":
                    YBranchImage = "YBranch";
                    if (isVertical)
                    {
                        if (parent is MyNodeYP)
                        {
                            YBranchImage = "YBranchVerticalMirror";
                            yP.YBranchOrientation = MyNodeYP.BranchOrientations.VerticalMirror.ToString();
                        }
                    }
                    break;
                case "BinaryTree":
                    YBranchImage = "YBranchVerticalMirror";
                    yP.YBranchOrientation = MyNodeYP.BranchOrientations.VerticalMirror.ToString();
                    if (isVertical)
                    {
                        if(parent is MyNodeOut)
                        {
                            YBranchImage = "YBranchVerticalMirror";
                            yP.YBranchOrientation = MyNodeYP.BranchOrientations.VerticalMirror.ToString();
                        }
                        else
                        {
                            YBranchImage = "YBranch";
                            yP.YBranchOrientation = MyNodeYP.BranchOrientations.Horizontal.ToString();
                        }                        
                    }
                    break;
                case "SchemaA":                    
                    if (isVertical)
                    {
                        YBranchImage = "YBranchVerticalMirror";
                        yP.YBranchOrientation = MyNodeYP.BranchOrientations.VerticalMirror.ToString();
                    }
                    else
                    {
                        YBranchImage = "YBranch";
                        yP.YBranchOrientation = MyNodeYP.BranchOrientations.Horizontal.ToString();
                    }
                    break;
            }
            return YBranchImage;
        }
        public Point alignLinkPerYBranchOrientation(Node node, Point pt, string respectivePosition="")
        {
            Point ptReturn = pt;
            if (node is MyNodeYP)
            {                
                MyNodeYP nodeYP = node as MyNodeYP;
                switch(nodeYP.YBranchOrientation)
                {
                    case "VerticalMirror":
                        ptReturn.X = pt.X - 8;
                        ptReturn.Y = pt.Y;
                        break;
                    case "Vertical":
                        //if (respectivePosition == "Bottom")
                        //{
                        //    ptReturn.X = pt.X;
                        //    ptReturn.Y = pt.Y + 8;
                        //}
                        //else
                        //{
                        //    ptReturn.X = pt.X;
                        //    ptReturn.Y = pt.Y - 8;
                        //}
                        break;
                    default:
                        ptReturn.X = pt.X;
                        ptReturn.Y = pt.Y;
                        break;
                }                                
            }
            return ptReturn;
        }                

        /// <summary>
        /// Get Total Number of Counts in Cousin Nodes
        /// </summary>
        /// <param name="parentYP"></param>
        private void GetChildIDUCount(MyNodeYP parentYP)
        {
            if(parentYP.ChildNodes[0] is MyNodeIn || parentYP.ChildNodes[0] is MyNodeCH)
            {
                Count++;
            }
            if (parentYP.ChildNodes[1] is MyNodeIn || parentYP.ChildNodes[1] is MyNodeCH)
            {
                Count++;
            }
            if (parentYP.ChildNodes[0] is MyNodeYP)
            {
                GetChildIDUCount(parentYP.ChildNodes[0] as MyNodeYP);
                
            }
            if(parentYP.ChildNodes[1] is MyNodeYP) 
            {
                GetChildIDUCount(parentYP.ChildNodes[1] as MyNodeYP);
            }                                  
        }        
    }
}

