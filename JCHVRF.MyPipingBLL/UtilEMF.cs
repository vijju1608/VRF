using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using Lassalle.Flow;
using JCHVRF.Model;
using System.Text.RegularExpressions;
using System;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace JCHVRF.MyPipingBLL
{
    /// <summary>
    /// Piping 以及 Wiring 绘制 EMF 以及导出 DXF 过程的辅助类
    /// </summary>
    public class UtilEMF
    {
        public static List<PointF> ptStart = new List<PointF>();
        public static List<PointF> ptEnd = new List<PointF>();
        float minx = 0;
        float miny = 0;
        float maxX = 0;
        float maxY = 0;

        float textMargin = 5;

        UtilPiping pipingUtil = new UtilPiping();

        PointF ptText = new PointF();
        public Font textFont_piping = new Font("Arial", 6f, FontStyle.Regular); // 12f
        public Brush textBrush_piping = new SolidBrush(Color.Black);     // Brushes.Black; 文字的清晰度提高
        public Pen pen_piping = new Pen(Color.Black, 0.1f);

        public Font textFont_wiring = new Font("Arial", 8f, FontStyle.Regular);
        public Brush textBrush_wiring = new SolidBrush(Color.Black);
        public Pen pen_wiring = new Pen(Color.Black, 0.1f);
        public Pen pen_wiring_power = new Pen(Color.Red, 0.3f);
        public Pen pen_wiring_dash = new Pen(Color.Black, 0.1f);
        public bool isManualLength = false;//add by axj 20161229 添加是否手动输入管长标识
        public string utLength = "";//add by axj 20161229 添加当前长度单位
        public UtilEMF()
        {
            pen_wiring_dash.DashStyle = DashStyle.Dash;
            pen_wiring_dash.DashPattern = new float[] { 5, 5 };
        }
        /// 绘制文字节点 add on 20160524 by Yunxiao Lin
        /// <summary>
        /// 绘制文字节点
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        public void DrawText(Graphics g, Node nd)
        {
            ptText = new PointF(nd.Location.X, nd.Location.Y);
            g.DrawString(nd.Text, textFont_piping, new SolidBrush(nd.TextColor), ptText);
        }

        public void DrawItemRecursion(Graphics g, Item item, string WiringImageDir)
        {
            if (item is Node)
            {
                if (item is MyNodeGround_Wiring)
                {
                    MyNodeGround_Wiring ndGnd = item as MyNodeGround_Wiring;
                    string nodePointsFile = Path.Combine(WiringImageDir, "Ground.txt");
                    DrawNode_wiring(g, ndGnd, nodePointsFile, "", null);
                }
                else
                {
                    Node nd = item as Node;
                    DrawNode(g, nd);
                    if (nd.Children != null)
                    {
                        foreach (Item child in nd.Children)
                        {
                            DrawItemRecursion(g, child, WiringImageDir);
                        }
                    }
                }
            }
            else if (item is Link)
            {
                DrawLine(g, item as Link);
            }
        }

        /// <summary>
        /// 绘制节点文字、背景、边框
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        public void DrawNode(Graphics g, Node nd)
        {
            if (nd == null) return;

            RectangleF rect = new RectangleF(nd.Location, nd.Size);
            string text = nd.Text;
            if (nd.DrawColor.R != 255 || nd.DrawColor.G != 255 || nd.DrawColor.B != 255)
            {
                Pen pen = new Pen(nd.DrawColor, nd.DrawWidth * 0.1f); //nd.DrawColor
                //pen.DashStyle = nd.DashStyle;
                //if (pen.DashStyle == DashStyle.Dash)
                //{
                //    pen.DashPattern = new float[] { 5, 5 };
                //}
                if (nd.Shape.Style == ShapeStyle.Rectangle)
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
                else if (nd.Shape.Style == ShapeStyle.Connector)
                {
                    //g.DrawEllipse(pen, rect);
                    //g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
            //判断是不是 Transparent或者White，但是nd.FillColor == Color.White的判断方法不够准确
            if (nd.FillColor.R != 255 || nd.FillColor.G != 255 || nd.FillColor.B != 255)
            {
                Brush brush = new SolidBrush(nd.FillColor);
                if (nd.Shape.Style == ShapeStyle.Rectangle)
                {
                    g.FillRectangle(brush, rect);
                }
                else if (nd.Shape.Style == ShapeStyle.Connector)
                {
                    //g.FillEllipse(brush, rect);
                    FillEllipse(g, brush, rect);
                }
            }
            if (!string.IsNullOrEmpty(text))
            {
                Font ft = nd.Font;
                if (ft.Size > 8f)
                {
                    ft = new Font(ft.FontFamily, 8f, ft.Style, ft.Unit);
                }
                else
                {
                    ft = new Font(ft.FontFamily, 6f, ft.Style, ft.Unit);
                }
                SizeF textSize = g.MeasureString(text, ft);
                textSize.Height = textSize.Height * 0.7f;

                float x = 0, y = 0;
                switch (nd.Alignment)
                {
                    case Alignment.LeftJustifyTOP:
                    case Alignment.LeftJustifyMIDDLE:
                    case Alignment.LeftJustifyBOTTOM:
                        x = rect.X;
                        break;
                    case Alignment.CenterTOP:
                    case Alignment.CenterMIDDLE:
                    case Alignment.CenterBOTTOM:
                        x = rect.X + (rect.Width - textSize.Width) / 2;
                        break;
                    case Alignment.RightJustifyTOP:
                    case Alignment.RightJustifyMIDDLE:
                    case Alignment.RightJustifyBOTTOM:
                        x = rect.X + rect.Width - textSize.Width;
                        break;
                }
                switch (nd.Alignment)
                {
                    case Alignment.LeftJustifyTOP:
                    case Alignment.RightJustifyTOP:
                    case Alignment.CenterTOP:
                        y = rect.Y;
                        break;
                    case Alignment.LeftJustifyMIDDLE:
                    case Alignment.RightJustifyMIDDLE:
                    case Alignment.CenterMIDDLE:
                        y = rect.Y + (rect.Height - textSize.Height) / 2;
                        break;
                    case Alignment.LeftJustifyBOTTOM:
                    case Alignment.RightJustifyBOTTOM:
                    case Alignment.CenterBOTTOM:
                        y = rect.Y + rect.Height;
                        break;
                }
                g.DrawString(text, ft, new SolidBrush(nd.TextColor), x, y);
            }
        }

        public void DrawLink(Graphics g, Link lnk)
        { 
            if (lnk.DrawColor != Color.Transparent)
            {
                Pen pen = new Pen(lnk.DrawColor, lnk.DrawWidth * 0.1f);
                pen.DashStyle = lnk.DashStyle;
                PointF[] points = new PointF[lnk.Points.Count];
                for (int i = 0; i < lnk.Points.Count; i++)
                {
                    points[i] = lnk.Points[i];
                }
                g.DrawLines(pen, points);
            }
        }

        public void FillEllipse(Graphics g, Brush brush, RectangleF rect)
        {
            float x = rect.X, y = rect.Y, w = rect.Width, h = rect.Height;
            float w1 = w / h * 2, w2, x1, y1;
            for (int i = 0; i < h / 2; i++)
            {
                w2 = w1 * (i + 1);
                x1 = x + (w - w2) / 2; 
                y1 = y + i; 
                g.FillRectangle(brush, x1, y1, w2, h - i * 2);
            }
        }

        /// 绘制机组节点以及机组型号文字
        /// <summary>
        /// 绘制机组节点以及机组型号文字
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        /// <param name="nodeImageFile"></param>
        /// <param name="model">shortModel</param>
        /// <param name="name">out1</param>
        public void DrawNode(Graphics g, Node nd, string nodeImageFile)
        {
            DrawNode(g, nd);

            //RectangleF rect = new RectangleF(nd.Location, nd.Size);
            // 根据机组图片的坐标集合文件绘制机组的线条图
            InitPointF(nodeImageFile);
            RelocatedNode(nd.Rect);
            
            Pen pen1 = new Pen(Color.Black, 0.1f);
            if (ptStart.Count == ptEnd.Count)
            {
                for (int i = 0; i < ptStart.Count; ++i)
                {
                    g.DrawLine(pen1, ptStart[i], ptEnd[i]);
                }
            }
            g.ResetTransform();
        }

        /// 绘制机组节点以及机组型号文字
        /// <summary>
        /// 绘制机组节点以及机组型号文字
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        /// <param name="nodeImageFile"></param>
        /// <param name="model">shortModel</param>
        /// <param name="name">out1</param>
        public void DrawNode_OutdoorGroup(Graphics g, Node nd, string nodeImageFile, string model, string name, NodeElement_Piping item, bool isInch)
        {
            DrawNode(g, nd);

            // 根据机组图片的坐标集合文件绘制机组的线条图
            InitPointF(nodeImageFile);
            RelocatedNode(nd.Rect);

            if (ptStart.Count == ptEnd.Count)
            {
                for (int i = 0; i < ptStart.Count; ++i)
                {
                    g.DrawLine(pen_piping, ptStart[i], ptEnd[i]);
                }
            }
            g.ResetTransform();

            //delete by axj 20170122 文字绘制统一使用文字节点绘制
            //// 绘制机组型号文字
            //PointF ptfTopCenter = pipingUtil.getTopCenterPointF(nd);    // 节点顶端中心点坐标
            //SizeF sz = g.MeasureString(name, textFont_piping);                 // 文字尺寸

            //ptText = new PointF(ptfTopCenter.X - sz.Width / 2, ptfTopCenter.Y - UtilPiping.HeightForNodeText);    // 文字的起始坐标点，让文字居中
            //g.DrawString(name, textFont_piping, textBrush_piping, ptText);            // Out1

            //sz = g.MeasureString(model, textFont_piping);
            //ptText = new PointF(ptfTopCenter.X - sz.Width / 2, ptText.Y + sz.Height);
            //g.DrawString(model, textFont_piping, textBrush_piping, ptText);           // YVOH200

            // TODO:绘制组合室外机内部分歧管型号及管径尺寸
            // 分歧管型号
            for (int i = 0; i < item.PtConnectionKit.Count; ++i)
            {
                ptText = OffsetLocation(item.PtConnectionKit[i], nd.Location);
                g.DrawString(item.ConnectionKitModel[i], textFont_piping, textBrush_piping, ptText);
            }

            //delete by axj 20170122 文字绘制统一使用文字节点绘制
            //// 管径号
            //PipingBLL bll = new PipingBLL();
            //for (int i = 0; i < item.PtPipeDiameter.Count; ++i)
            //{
            //    ptText = OffsetLocation(item.PtPipeDiameter[i], nd.Location);
            //    //g.DrawString(item.PipeDiameter[i], textFont_piping, textBrush_piping, ptText);
            //    string s = item.PipeSize[i];
            //    string[] aa = s.Split('x');
            //    if (aa.Length == 2)
            //    {
            //        string p1 = "φ" + aa[0] + "mm";
            //        string p2 = "φ" + aa[1] + "mm";
            //        if (isInch)
            //        {
            //            p1 = bll.GetPipeSize_Inch(aa[0].Trim()) + "\"";
            //            p2 = bll.GetPipeSize_Inch(aa[1].Trim()) + "\"";
            //        }
            //        g.DrawString(p1, textFont_piping, textBrush_piping, ptText);
            //        ptText.Y += sz.Height;
            //        g.DrawString(p2, textFont_piping, textBrush_piping, ptText);
            //    }
            //    else if (aa.Length == 3)
            //    {
            //        string p1 = "φ" + aa[0] + "mm";
            //        string p2 = "φ" + aa[1] + "mm";
            //        string p3 = "φ" + aa[2] + "mm";
            //        if (isInch)
            //        {
            //            p1 = bll.GetPipeSize_Inch(aa[0].Trim()) + "\"";
            //            p2 = bll.GetPipeSize_Inch(aa[1].Trim()) + "\"";
            //            p3 = bll.GetPipeSize_Inch(aa[2].Trim()) + "\"";
            //            ptText.X += 20;
            //        }
            //        g.DrawString(p1, textFont_piping, textBrush_piping, ptText);
            //        ptText.Y += sz.Height;
            //        g.DrawString(p2, textFont_piping, textBrush_piping, ptText);
            //        ptText.Y += sz.Height;
            //        g.DrawString(p3, textFont_piping, textBrush_piping, ptText);
            //    }
            //}

            // 左下角竖线            
            Point p = new Point(item.PtVLine.X, item.PtVLine.Y + 50);
            g.DrawLine(pen_piping, OffsetLocation(item.PtVLine, nd.Location), OffsetLocation(p, nd.Location));
        }

        /// 绘制分歧管以及分歧管型号（气液管）
        /// <summary>
        /// 绘制分歧管以及分歧管型号（气液管）
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        /// <param name="isCP"></param>
        public void DrawYP(Graphics g, Node nd, bool isCP)
        {
            RectangleF recf = new RectangleF(nd.Location, nd.Size);
            Pen pen = new Pen(nd.DrawColor, 0.1f);

            if (!isCP)
            {
                PointF pt1 = pipingUtil.getLeftCenterPointF(nd);
                PointF[] ptf = { pt1, pipingUtil.getRightTopPointF(nd), pipingUtil.getRightBottomPointF(nd), pt1 };
                #region CADImportNet 不支持 DrawPolygon 方法绘制的图形
                //g.DrawPolygon(pen, ptf);
                //g.FillPolygon(Brushes.Blue, ptf, FillMode.Winding); //OK
                #endregion
                g.DrawLines(pen, ptf);
                g.FillPolygon(Brushes.Blue, ptf); //OK
            }
            else
            {
                g.DrawRectangle(pen, nd.Rect.X, nd.Rect.Y, nd.Rect.Width, nd.Rect.Height);
                g.FillRectangle(Brushes.Blue, nd.Rect);
            }
            //delete by axj 20170122 文字绘制统一使用文字节点绘制
            //// 绘制分歧管的型号文字
            //MyNodeYP ndYP = nd as MyNodeYP;
            //string text = ndYP.Model;
            //SizeF sz = g.MeasureString(text, textFont_piping);
            //ptText = new PointF(ndYP.Location.X + 10, ndYP.Location.Y - sz.Height);
            //g.DrawString(text, textFont_piping, textBrush_piping, ptText);

        }

        /// 绘制连接管以及管径型号（气液管）
        /// <summary>
        /// 绘制连接管以及管径型号（气液管）
        /// </summary>
        /// <param name="g"></param>
        /// <param name="line"></param>
        public void DrawLine(Graphics g, Link line)
        {
            List<PointF> ptl = new List<PointF>();
            foreach (PointF p in line.Points)
            {
                ptl.Add(p);
            }
            if (ptl.Count <= 1)
                return;

            PointF[] pts = ptl.ToArray();
            Pen pen = new Pen(line.DrawColor, 0.1f);
            Node nd = line.Dst;
            if (nd != null && nd is MyNodeGround_Wiring)
                pen = new Pen(Color.LightGreen, 0.1f);
            g.DrawLines(pen, pts);
        }

        #region 辅助方法

        /// <summary>
        /// 初始化机组图片坐标，平移至原点
        /// </summary>
        /// <param name="filePath"> 存放图片点坐标的txt文件 </param>
        private void InitPointF(string filePath)
        {
            ptStart.Clear();
            ptEnd.Clear();

            //minx和miny不再根据数据取得，固定从0开始。避免图形不是从0,0开始的而被放大。 modified by Shen Junjie on 2018/5/3
            minx = 100000;  
            miny = 100000;
            maxX = 0;
            maxY = 0;

            string s;
            StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default);
            while ((s = sr.ReadLine()) != null)
            {
                string[] ch = { " ", ",", "\t" };
                string[] str = s.Split(ch, System.StringSplitOptions.RemoveEmptyEntries);
                if (str.Length == 4)
                {
                    float x = float.Parse(str[0]);
                    float y = System.Math.Abs(float.Parse(str[1]));
                    minx = minx < x ? minx : x;
                    miny = miny < y ? miny : y;
                    maxX = maxX > x ? maxX : x;
                    maxY = maxY > y ? maxY : y;
                    PointF pts = new PointF(x, y);

                    x = float.Parse(str[2]);
                    y = System.Math.Abs(float.Parse(str[3]));
                    minx = minx < x ? minx : x;
                    miny = miny < y ? miny : y;
                    maxX = maxX > x ? maxX : x;
                    maxY = maxY > y ? maxY : y;
                    PointF pte = new PointF(x, y);

                    ptStart.Add(pts);
                    ptEnd.Add(pte);
                }
            }
        }

        /// <summary>
        /// 重新定位,将图形定位到指定的区域中
        /// </summary>
        /// <param name="rect"> 图形指定的区域 </param>
        private void RelocatedNode(RectangleF rect)
        {
            SizeF actSize = new SizeF(maxX - minx + 1, maxY - miny - 1);
            if (actSize.Width == 0 || actSize.Height == 0)
                return;
            float scaleX = rect.Width / actSize.Width;
            float scaleY = rect.Height / actSize.Height;
            float scale = scaleX;//scaleX > scaleY ? scaleX : scaleY

            if (ptStart.Count == ptEnd.Count)
            {
                for (int i = 0; i < ptStart.Count; ++i)
                {
                    PointF pts = new PointF((ptStart[i].X - minx) * scale, (ptStart[i].Y - miny) * scale);

                    pts.X += rect.X;
                    pts.Y += rect.Y;
                    ptStart[i] = pts;

                    PointF pte = new PointF((ptEnd[i].X - minx) * scale, (ptEnd[i].Y - miny) * scale);

                    pte.X += rect.X;
                    pte.Y += rect.Y;
                    ptEnd[i] = pte;
                }
            }
        }

        /// <summary>
        /// 将指定的点平移至指定的Node区域
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="ptOffset"></param>
        public void RelocateLocation(ref PointF pt, PointF ptOffset)
        {
            pt.X += ptOffset.X;
            pt.Y += ptOffset.Y;
        }

        /// <summary>
        /// 将指定的Rectangle平移至指定的Node区域
        /// </summary>
        /// <param name="rectSrc"></param>
        /// <param name="rect"></param>
        private void RelocateLocation(ref RectangleF rectSrc, PointF ptOffset)
        {
            rectSrc.X += ptOffset.X;
            rectSrc.Y += ptOffset.Y;
        }

        #endregion

        /// 绘制机组节点以及机组型号文字
        /// <summary>
        /// 绘制机组节点以及机组型号文字 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        /// <param name="nodeTextFile"></param>
        /// <param name="model">shortModel</param>
        /// <param name="name">out1</param>
        public void DrawNode_wiring(Graphics g, Node nd, string nodeTextFile, string name, NodeElement_Wiring item)
        {
            //Pen pen = pen_wiring;
            Pen pen = new Pen(Color.Black, 0.1f);
            if (nd is MyNodeGround_Wiring)
                pen = new Pen(Color.LightGreen, 0.1f);// this is for drawing the Ground icon of 3 horizontal lines

            SizeF ndSize = new SizeF(nd.Size.Width, nd.Size.Height);
            PointF ndLocation = new PointF(nd.Location.X, nd.Location.Y);
            if (nd is WiringNodeIn)
            {
                ndLocation = new PointF(nd.Location.X, nd.Location.Y);
            }

            RectangleF rect = new RectangleF(ndLocation, ndSize);
            //RectangleF rect = new RectangleF(ndLocation, item.NodeSize);
            // 根据机组图片的坐标集合文件绘制机组的线条图
            InitPointF(nodeTextFile);
            RelocatedNode(rect);

            if (ptStart.Count == ptEnd.Count)
            {
                for (int i = 0; i < ptStart.Count; ++i)
                {
                    g.DrawLine(pen, ptStart[i], ptEnd[i]);
                }
            }
            g.ResetTransform();

            if (item!=null && item.PtCircles.Count >= item.UnitCount * 4)
            {
                g.DrawString(item.Str1, textFont_wiring, textBrush_wiring, OffsetLocation(item.PtStr1, ndLocation));

                for (int i = 0; i < item.UnitCount; ++i)
                {
                    RectangleF r1 = new RectangleF(item.PtCircles[i * 4], item.CircleSize);
                    RelocateLocation(ref r1, ndLocation);
                    RectangleF r2 = new RectangleF(item.PtCircles[i * 4 + 1], item.CircleSize);
                    RelocateLocation(ref r2, ndLocation);
                    RectangleF r3 = new RectangleF(item.PtCircles[i * 4 + 2], item.CircleSize);
                    RelocateLocation(ref r3, ndLocation);
                    RectangleF r4 = new RectangleF(item.PtCircles[i * 4 + 3], item.CircleSize);
                    RelocateLocation(ref r4, ndLocation);
                    g.DrawEllipse(pen, r1);
                    g.DrawEllipse(pen, r2);
                    g.DrawEllipse(pen, r3);
                    g.DrawEllipse(pen, r4);

                    g.DrawString(item.ModelGroup[i], textFont_wiring, textBrush_wiring, OffsetLocation(item.PtModelGroup[i], ndLocation));      // YVOH200
                    if (i < 2)
                    {
                        g.DrawString(item.StrGroup1[i], textFont_wiring, textBrush_wiring, OffsetLocation(item.PtStrGroup1[i], ndLocation));    // X Y | X Y
                    }

                    g.DrawString(item.StrGroup2[i], textFont_wiring, textBrush_wiring, OffsetLocation(item.PtStrGroup2[i], ndLocation));        // L1L2L3N

                    if (item.UnitCount > 1)
                    {
                        g.DrawString(item.StrGroup3[i], textFont_wiring, textBrush_wiring, OffsetLocation(item.PtStrGroup3[i], ndLocation));    // 19A 3Nph
                    }
                }
            }

        }

        /// 将字符串转换为坐标列表
        /// <summary>
        /// 将字符串转换为坐标列表
        /// </summary>
        /// <param name="ptString">匹配格式(x,y)(x,y)...</param>
        /// <returns></returns>
        public static List<Point> transPoints(string ptString)
        {
            // 验证输入格式
            string pattern = @"\(.*?\)"; // 格式
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(ptString);

            // 转换过程
            List<Point> ptList = new List<Point>();
            foreach (Match match in matches)
            {
                string s = match.Value.Trim('(', ')');
                string[] ss = s.Split(',');
                Point pt = new Point(Convert.ToInt32(ss[0].Trim()), Convert.ToInt32(ss[1].Trim()));
                ptList.Add(pt);
            }
            return ptList;
        }

        public static PointF OffsetLocation(PointF ptSrc, PointF ptOffset)
        {
            return new PointF(ptSrc.X + ptOffset.X, ptSrc.Y + ptOffset.Y);
        }


        #region 绘制 EMF & DXF
        ///// 保存配线图中每条连接线上的点坐标
        ///// <summary>
        ///// 保存配线图中每条连接线上的点坐标
        ///// </summary>
        //List<PointF[]> ptArrayList = new List<PointF[]>();

        ///// 保存配线图中每虚线上的点坐标
        ///// <summary>
        ///// 保存配线图中每虚线上的点坐标
        ///// </summary>
        //List<PointF[]> ptArrayList_dash = new List<PointF[]>();
        
        //List<NodeElement_Wiring> OutdoorGroupList_Wiring = new List<NodeElement_Wiring>();
        ///// 绘制 EMF 图片 (Wiring 导出DXF)
        ///// <summary>
        ///// 绘制 EMF 图片 (Wiring)
        ///// </summary>
        ///// <param name="addFlow1"></param>
        ///// <param name="g"></param>
        //public void DrawEMF_wiring(Graphics g, AddFlow addFlowWiring, SystemVRF curSystemItem, string WiringImageDir, string brandCode)
        //{
        //    PointF ptf1 = new PointF(0, 0);
        //    PointF ptf2 = new PointF(0, 0);
        //    PointF ptf3 = new PointF(0, 0);
        //    foreach (Item item in addFlowWiring.Items)
        //    {
        //        if (item is Node)
        //        {
        //            Node nd = item as Node;
        //            if (nd is WiringNodeOut)
        //            {
        //                WiringNodeOut ndOut = nd as WiringNodeOut;
        //                NodeElement_Wiring item_wiring = GetNodeElement_Wiring_ODU(curSystemItem.OutdoorItem, brandCode);
        //                string nodePointsFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
        //                DrawNode_wiring(g, ndOut, nodePointsFile, curSystemItem.Name, item_wiring);
        //            }
        //            else if (nd is WiringNodeIn)
        //            {
        //                WiringNodeIn ndIn = nd as WiringNodeIn;
        //                NodeElement_Wiring item_wiring = GetNodeElement_Wiring_IDU(ndIn.RoomIndoorItem.IndoorItem, brandCode, curSystemItem.OutdoorItem.Type);
        //                string nodePointsFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
        //                DrawNode_wiring(g, ndIn, nodePointsFile, ndIn.RoomIndoorItem.IndoorName, item_wiring);
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(nd.Text))
        //                {
        //                    PointF pf = nd.Location;
        //                    g.DrawString(nd.Text, textFont_wiring, textBrush_wiring, pf);
        //                }
        //            }
        //        }
        //    }

        //    foreach (PointF[] pt in ptArrayList)
        //    {
        //        g.DrawLines(pen_wiring, pt);//
        //    }

        //    foreach (PointF[] pt in ptArrayList_dash)
        //    {
        //        g.DrawLines(pen_wiring, pt); //penDash_wiring
        //    }

        //}


        ///// 获取室外机Wiring图节点对象 Modify on 20160521 by Yunxiao Lin
        ///// <summary>
        ///// 获取室外机Wiring图节点对象
        ///// </summary>
        ///// <param name="outItem"></param>
        ///// <returns></returns>
        //public NodeElement_Wiring GetNodeElement_Wiring_ODU(Outdoor outItem, string brandCode)
        //{
        //    //string[] unitFullName = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

        //    string[] strs1 = outItem.FullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
        //    List<string> unitFullName = new List<string>();
        //    foreach (string str1 in strs1)
        //    {
        //        //JCHVRF 中不仅仅有 model1+model2+...，还会有 model1+model2*2+.... Yunxiao Lin
        //        if (str1.Contains("*"))
        //        {
        //            string[] strs2 = str1.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
        //            if (strs2.Length == 2)
        //            {
        //                int num = 0;
        //                if (int.TryParse(strs2[1], out num))
        //                {
        //                    for (int i = 0; i < num; i++)
        //                    {
        //                        unitFullName.Add(strs2[0]);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //            unitFullName.Add(str1);
        //    }
        //    int count = unitFullName.Count;

            
        //    //获取室外机所有的分机对象 20160521 Yunxiao Lin
        //    Outdoor[] outItems = new Outdoor[count];

        //    //JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, outItem.ProductType, brandCode);
        //    //OutdoorDAL 不再设置固定的productType, 该参数取消 20160823 by Yunxiao Lin
        //    JCHVRF.DAL.OutdoorDAL dal = new DAL.OutdoorDAL(outItem.RegionCode, brandCode);
        //    for (int i = 0; i < count; i++)
        //    {
        //        string modelname = unitFullName[i];
                
        //        //if (outItem.RegionCode == "MAL") //马来西亚的机组名称用的是Model_York,需要特殊处理 20160629 Yunxiao Lin
        //        //if (outItem.RegionCode == "MAL" && brandCode == "Y") //只有马来西亚的YORK品牌需要这样处理 20161023 by Yunxiao Lin
        //        if (((outItem.RegionCode == "MAL" || outItem.RegionCode == "LA_BV") && brandCode == "Y") || (outItem.Series == "Commercial VRF HP, YVAHP") || (outItem.Series == "Commercial VRF HR, YVAHR")) //马来西亚的YORK品牌以及北美机型需要这样处理 20170323 by Yunxiao Lin
        //        {
        //            // 玻利维亚的York产品线和马来西亚的一样 20170629 by Yunxiao Lin
        //            //Outdoor coutItem = dal.GetYorkItem(modelname);
        //            Outdoor coutItem = dal.GetYorkItem(modelname, outItem.ProductType);
        //            if (coutItem != null)
        //            {
        //                outItems[i] = coutItem;
        //            }
        //        }
        //        else if (brandCode == "H")//如果是Hitachi的外机，需用HitachiModel名查询 20160521 Yunxiao Lin
        //        {
        //            //Outdoor coutItem = dal.GetHitachiItem(modelname);
        //            Outdoor coutItem = dal.GetHitachiItem(modelname, outItem.ProductType);
        //            if (coutItem != null)
        //            {
        //                outItems[i] = coutItem;
        //            }
        //        }
        //        else
        //        {
        //            //York
        //            //Outdoor coutItem = dal.GetItem(modelname);
        //            Outdoor coutItem = dal.GetItem(modelname, outItem.ProductType);
        //            if (coutItem != null)
        //            {
        //                outItems[i] = coutItem;
        //            }
        //        }
        //    }
            
           
        //    //根据室外机分机型号名获取电源参数 20160521 by Yunxiao Lin
        //    DAL.MyDictionaryDAL dicdal = new DAL.MyDictionaryDAL();
        //    string strGroup2 = "";
        //    string strGroup3 = "";
        //    string strGroup4 = "";
        //    string modelGroup = "";
        //    foreach (Outdoor coutItem in outItems)
        //    {
        //        if (coutItem != null)
        //        {
        //            if (outItem.Type.Contains("YVAHP") || outItem.Type.Contains("YVAHR"))
        //            {
        //                //YVAHP 显示 Model_York
        //                if (modelGroup != "")
        //                    modelGroup += "," + coutItem.Model_York;
        //                else
        //                    modelGroup += coutItem.Model_York;
        //            }
        //            //MyDictionary dic = dicdal.GetItem(MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(10, 1));
        //            MyDictionary dic = dicdal.GetItem(MyDictionary.DictionaryType.PowerSupply, coutItem.ModelFull.Substring(coutItem.ModelFull.Length-4, 1));
        //            string strPowerLineType = dic.Name.Contains("3Ph") ? "///" : "//"; //电源线3芯2芯
        //            string strPowerPhaseNumber = "L1L2L3N"; // 三相>380V
        //            if (strPowerLineType == "//")
        //            {
        //                strPowerPhaseNumber = "R S"; //单相<380V
        //            }
        //            //if (coutItem.ModelFull.Substring(10, 1) == "R")
        //            if (coutItem.ModelFull.Substring(coutItem.ModelFull.Length - 4, 1) == "R")
        //                strPowerPhaseNumber = "R S T"; //三相<380V
        //            string strPower = dic.Name.Replace("1Ph/", "").Replace("3Ph/", ""); //电源标识
        //            double current = coutItem.MaxCurrent; // 电流值
        //            strPower += "/" + current.ToString() + "A";

        //            if (strGroup2 != "")
        //                strGroup2 += "|" + strPowerPhaseNumber;
        //            else
        //                strGroup2 += strPowerPhaseNumber;
        //            if (strGroup3 != "")
        //                strGroup3 += "|" + strPower;
        //            else
        //                strGroup3 += strPower;
        //            if (strGroup4 != "")
        //                strGroup4 += "," + strPowerLineType;
        //            else
        //                strGroup4 += strPowerLineType;
        //        }
        //    }

        //    string model = outItem.Model_York; //根据PM要求，此处显示分机的Model_York 20180214 by Yunxiao Lin
        //    if (!outItem.Type.Contains("YVAHP") && !outItem.Type.Contains("YVAHR"))
        //    {
        //        if (brandCode == "H")
        //            model = outItem.Model_Hitachi;
        //        //根据PM要求，此处显示分机的Model_York 20180214 by Yunxiao Lin
        //        //else // York 
        //        //{
        //        //    for (int i = 0; i < unitFullName.Count; ++i)
        //        //    {
        //        //        string s = unitFullName[i].Substring(0, 7);
        //        //        unitFullName[i] = s;
        //        //    }
        //        //}
        //        modelGroup = string.Join(",", unitFullName);
        //    }
        //    else
        //    {
        //        //YVAHP 显示 Model_York
        //        model = outItem.Model_York;
        //    }

        //    NodeElement_Wiring item = new NodeElement_Wiring("Out" + count.ToString(), model, count, modelGroup, strGroup2, strGroup3, strGroup4, brandCode);
        //    return item;
        //}

        //public NodeElement_Wiring GetNodeElement_Wiring_IDU(Indoor inItem, string brandCode, string outType)
        //{
        //    //string model = inItem.Model;
        //    string model = inItem.Model_York; //根据PM要求，piping 中的IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
        //    if (brandCode == "H")
        //        model = inItem.Model_Hitachi;
        //    if (outType.Contains("YVAHP") || outType.Contains("YVAHR"))
        //        model = inItem.Model_York;
        //    //根据室内机型号获取电源参数 20160521 by Yunxiao Lin
        //    DAL.MyDictionaryDAL dicdal = new DAL.MyDictionaryDAL();
        //    //MyDictionary dic = dicdal.GetItem(MyDictionary.DictionaryType.PowerSupply, inItem.ModelFull.Substring(10, 1));
        //    MyDictionary dic = dicdal.GetItem(MyDictionary.DictionaryType.PowerSupply, inItem.ModelFull.Substring(inItem.ModelFull.Length-4, 1));
        //    string strPowerLineType = dic.Name.Contains("3Ph") ? "///" : "//"; //电源线3芯2芯
        //    string strPowerPhaseNumber = "L1L2L3N"; // 三相>380V
        //    if (strPowerLineType == "//")
        //    {
        //        strPowerPhaseNumber = "R S"; //单相<380V
        //    }
        //    //if (inItem.ModelFull.Substring(10, 1) == "R")
        //    if (inItem.ModelFull.Substring(inItem.ModelFull.Length-4, 1) == "R")
        //        strPowerPhaseNumber = "R S T"; //三相<380V
        //    string strPower = dic.Name.Replace("1Ph/", "").Replace("3Ph/", ""); //电源标识
        //    double current = inItem.RatedCurrent; // 电流值
        //    strPower += "/" + current.ToString() + "A";
            
        //    NodeElement_Wiring item = new NodeElement_Wiring("Ind", model, 1, model, strPowerPhaseNumber, strPower, strPowerLineType, brandCode);
        //    return item;
        //}
        ///// 获取CH-Box Wiring图节点对象 add on 20160525 by Yunxiao Lin
        ///// <summary>
        ///// 获取CH-Box Wiring图节点对象
        ///// </summary>
        ///// <param name="ch"></param>
        ///// <returns></returns>
        //public NodeElement_Wiring GetNodeElement_Wiring_CH(WiringNodeCH ch)
        //{
        //    string model = ch.Model;
        //    NodeElement_Wiring item = new NodeElement_Wiring("CH", model, 1, model, "", "", "", "H");
        //    return item;
        //}

        /// 绘制文字节点 add on 20161229 add by axj
        /// <summary>
        /// 绘制文字节点
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        public void DrawActualCapacityText(Graphics g, Node nd)
        {
            string[] ArrayTxt = nd.Text.Split('\n');
            SizeF sz = new SizeF();
            PointF pt = new PointF(nd.Location.X, nd.Location.Y);
            for (int i = 0; i < ArrayTxt.Length; i++)
            {
                g.DrawString(ArrayTxt[i], textFont_piping, textBrush_piping, pt);
                sz = g.MeasureString(ArrayTxt[i], textFont_piping);
                pt.Y += sz.Height;
            }
        }


        ///// 绘制配线图
        ///// <summary>
        ///// 绘制配线图，界面显示
        ///// </summary>
        //public void DoDrawingWiring(TreeNode tnOut, AddFlow addFlowWiring, SystemVRF curSystemItem, string WiringImageDir, string brandCode)
        //{
        //    Graphics g = addFlowWiring.CreateGraphics();

        //    InitWiringNodes(addFlowWiring);
        //    if (tnOut != null)
        //        CreatePipingNodeStructure_wiring(tnOut, curSystemItem);

        //    ptArrayList.Clear();
        //    ptArrayList_dash.Clear();

        //    // 当删除所有的室内机后，curSystemItem.MyWiringNodeOut即为空 
        //    if (curSystemItem == null || curSystemItem.MyWiringNodeOut == null)
        //        return;

        //    //----------------
        //    // 检测是否可以绘图
        //    MyNodeOut_Wiring nodeOut = curSystemItem.MyWiringNodeOut;
        //    if (nodeOut.NodeInList == null || curSystemItem.OutdoorItem == null)
        //    {
        //        Label lbl2 = new Label();
        //        lbl2.Dock = DockStyle.Fill;
        //        lbl2.Text = "ERROR! Cannot draw the Wiring diagrams because there is no indoor unit";
        //        lbl2.ForeColor = Color.Red;
        //        addFlowWiring.Controls.Add(lbl2);
        //        return;
        //    }

        //    Node textNode = new Node();
        //    PointF ptText = new PointF();

        //    // 1. 绘制室外机节点
        //    NodeElement_Wiring item_wiring = GetNodeElement_Wiring_ODU(curSystemItem.OutdoorItem, brandCode);
        //    double current = curSystemItem.OutdoorItem.MaxCurrent; // 电流值
        //    nodeOut.Location = new PointF(10f, UtilPiping.HeightForNodeText + UtilPiping.OutdoorOffset_Y_wiring); // 必须先设置好 Location
        //    // 设置主节点加载的图片
        //    string imgFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".png");
        //    pipingUtil.setNode_wiring(nodeOut, imgFile, ref addFlowWiring, item_wiring);

        //    //----------------
        //    // 2. 室外机节点文字
        //    string text = "";
        //    ptText = item_wiring.PtNodeNames[0];
        //    if (item_wiring.UnitCount == 1)
        //    {
        //        //nodeOut.Text = item_wiring.ShortModel;
        //        ptText.Y += UtilPiping.HeightForNodeText / 2;

        //        // 4. 独立的室外机节点节点右下方电流参数
        //        text = current.ToString() + "A";
        //        createTextNode_wiring(text, item_wiring.PtStrGroup3[0], nodeOut, g);
        //    }

        //    text = curSystemItem.Name;
        //    createTextNode_wiring(text, ptText, nodeOut, g);
        //    if (item_wiring.UnitCount > 1)
        //    {
        //        text = curSystemItem.OutdoorItem.AuxModelName;
        //        // curSystemItem.OutdoorItem.Model;
        //        createTextNode_wiring(text, item_wiring.PtNodeNames[1], nodeOut, g);
        //    }

        //    //---------------- 
        //    createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeOut, g);
        //    // 3. 室外机上的电流线,虚线，以及室外机节点中的文字
        //    for (int i = 0; i < item_wiring.UnitCount; ++i)
        //    {
        //        PointF ptf1 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[i], nodeOut.Location);
        //        PointF ptf2 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[i], nodeOut.Location);
        //        ptArrayList_dash.Add(new PointF[] { ptf1, ptf2 });

        //        createTextNode_wiring(item_wiring.ModelGroup[i], item_wiring.PtModelGroup[i], nodeOut, g);
        //        if (i < 2)
        //            createTextNode_wiring(item_wiring.StrGroup1[i], item_wiring.PtStrGroup1[i], nodeOut, g);
        //        createTextNode_wiring(item_wiring.StrGroup2[i], item_wiring.PtStrGroup2[i], nodeOut, g);
        //        if (item_wiring.UnitCount > 1)
        //        {
        //            createTextNode_wiring(item_wiring.StrGroup3[i], item_wiring.PtStrGroup3[i], nodeOut, g);
        //        }
        //    }

        //    //----------------
        //    // 5. 绘制室内机节点
        //    PointF ptf4 = new PointF(0, 0);
        //    PointF ptf5 = new PointF(0, 0);
        //    PointF ptf6;
        //    double sumCurrent = 0;
        //    ptText = item_wiring.PtNodeNames[0];
        //    ptText.Y += UtilPiping.HeightForNodeText / 2;
        //    for (int i = 0; i < nodeOut.NodeInList.Length; i++)
        //    {
        //        MyNodeIn nodeIn = nodeOut.NodeInList[i];
        //        item_wiring = GetNodeElement_Wiring_IDU(nodeIn.RoomIndooItem.IndoorItem, brandCode);
        //        current = nodeIn.RoomIndooItem.IndoorItem.RatedCurrent;
        //        sumCurrent += current;

        //        nodeIn.Location = pipingUtil.getLocationChild_wiring(nodeOut, i);
        //        nodeIn.Text = item_wiring.ShortModel;
        //        // 设置主节点加载的图片
        //        imgFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".png");
        //        pipingUtil.setNode_wiring(nodeIn, imgFile, ref addFlowWiring, item_wiring);

        //        //----------------
        //        // 6. 室内机节点文字                
        //        createTextNode_wiring(nodeIn.RoomIndooItem.IndoorName, ptText, nodeIn, g);
        //        createTextNode_wiring(current.ToString() + "A", item_wiring.PtStrGroup3[0], nodeIn, g);

        //        createTextNode_wiring(item_wiring.Str1, item_wiring.PtStr1, nodeIn, g);
        //        createTextNode_wiring(item_wiring.StrGroup1[0], item_wiring.PtStrGroup1[0], nodeIn, g);
        //        createTextNode_wiring(item_wiring.StrGroup2[0], item_wiring.PtStrGroup2[0], nodeIn, g);


        //        //----------------
        //        // 8.生成连接线的坐标点集合
        //        float x = nodeOut.Location.X + nodeOut.Size.Width - 1;
        //        float y = nodeIn.Location.Y + nodeIn.Size.Height - 2; // 因为室内机节点的高度为52
        //        if (i == 0)
        //        {
        //            PointF ptf1 = new PointF(x, y);
        //            PointF ptf2 = new PointF(nodeIn.Location.X + 1, y);
        //            ptArrayList.Add(new PointF[] { ptf1, ptf2 });
        //        }
        //        else
        //        {
        //            x = nodeIn.Location.X;
        //            PointF ptf1 = new PointF(x, y);
        //            PointF ptf2 = new PointF(x, y - 15);
        //            PointF ptf3 = new PointF(x + 60, y - UtilPiping.VDistanceVertical_wiring);
        //            ptArrayList.Add(new PointF[] { ptf1, ptf2, ptf3 });
        //        }

        //        ptf4 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2L[0], nodeIn.Location);
        //        ptf5 = UtilEMF.OffsetLocation(item_wiring.PtStrGroupLine2R[0], nodeIn.Location);
        //        ptArrayList_dash.Add(new PointF[] { ptf4, ptf5 });
        //    }
        //    ptf4 = new PointF(ptf5.X, nodeOut.NodeInList[0].Location.Y);
        //    ptf6 = new PointF(ptf5.X + 90, ptf4.Y);
        //    ptArrayList_dash.Add(new PointF[] { ptf5, ptf4, ptf6 }); // 节点右侧的电流汇总，虚线

        //    //----------------
        //    // 9. 电流汇总显示
        //    text = sumCurrent.ToString() + "A";
        //    textNode = createTextNode_wiring(text, new PointF(ptf4.X + 12, ptf4.Y + 2), g);
        //    textNode.Location = new PointF(ptf4.X + 12, ptf4.Y + 2);
        //    addFlowWiring.Nodes.Add(textNode);
        //    //----------------

        //    // 10.绘制连线
        //    // 实线
        //    foreach (PointF[] pt in ptArrayList)
        //    {
        //        Node nd1 = pipingUtil.createLinePoint(pt[0]);
        //        addFlowWiring.Nodes.Add(nd1);
        //        Node nd2 = pipingUtil.createLinePoint(pt[pt.Length - 1]);
        //        addFlowWiring.Nodes.Add(nd2);
        //        Link lnk1 = pipingUtil.createLine();
        //        nd1.OutLinks.Add(lnk1, nd2);
        //        if (pt.Length > 2)
        //            lnk1.Points.Add(pt[1]);
        //    }
        //    // 虚线
        //    foreach (PointF[] pt in ptArrayList_dash)
        //    {
        //        Node nd1 = pipingUtil.createLinePoint(pt[0]);
        //        addFlowWiring.Nodes.Add(nd1);
        //        Node nd2 = pipingUtil.createLinePoint(pt[pt.Length - 1]);
        //        Link lnk1 = pipingUtil.createLine();
        //        if (pt.Length > 2)
        //            lnk1.Line.Style = LineStyle.VH;
        //        lnk1.DashStyle = DashStyle.Dash;
        //        nd1.OutLinks.Add(lnk1, nd2);
        //    }
        //}


        ///// 根据 TreeView 布局构造 Wiring 图中的机组节点对象
        ///// <summary>
        ///// 根据 TreeView 布局构造 Wiring 图中的机组节点对象
        ///// </summary>
        ///// <param name="tnOut"></param>
        //private void CreatePipingNodeStructure_wiring(TreeNode tnOut, SystemVRF curSystemItem)
        //{
        //    curSystemItem = tnOut.Tag as SystemVRF;
        //    curSystemItem.MyWiringNodeOut = utilPiping.createNodeOut_wiring(tnOut.Nodes.Count);
        //    MyNodeOut_Wiring nodeOut = curSystemItem.MyWiringNodeOut;

        //    foreach (TreeNode tn in tnOut.Nodes)
        //    {
        //        RoomIndoor riItem = tn.Tag as RoomIndoor;
        //        MyNodeIn nodeIn = utilPiping.createNodeIn_wiring(riItem);
        //        nodeOut.AddNodeIn(nodeIn, tn.Index);
        //    }
        //}

        ///// 创建文字节点--Wiring
        ///// <summary>
        ///// 创建文字节点--Wiring
        ///// </summary>
        ///// <param name="text"></param>
        ///// <param name="pt"></param>
        ///// <param name="parent"></param>
        ///// <returns></returns>
        //Node createTextNode_wiring(string text, PointF pt, Node parent, Graphics g)
        //{
        //    Node label = new Node();

        //    // 设置字体并量取文字标题的尺寸
        //    //Graphics g = this.CreateGraphics();
        //    g.PageUnit = GraphicsUnit.Pixel;            //单位为像素
        //    Font ft = utilPiping.textFont_wiring;
        //    label.Font = ft;
        //    SizeF size = g.MeasureString(text, ft);

        //    label.Transparent = true;
        //    label.Text = text;
        //    label.Size = new SizeF(size.Width + 15, size.Height);

        //    label.Alignment = Alignment.LeftJustifyTOP;
        //    label.DrawColor = Color.Transparent;
        //    label.Shape.Style = ShapeStyle.Rectangle;
        //    label.Logical = false;
        //    label.Selectable = false;
        //    label.AttachmentStyle = AttachmentStyle.Item;

        //    label.Location = UtilEMF.OffsetLocation(pt, parent.Location);

        //    parent.AddFlow.Nodes.Add(label);
        //    label.Parent = parent;
        //    return label;
        //}

        //Node createTextNode_wiring(string text, PointF pt, Graphics g)
        //{
        //    Node label = new Node();

        //    // 设置字体并量取文字标题的尺寸
        //    //Graphics g = this.CreateGraphics();
        //    g.PageUnit = GraphicsUnit.Pixel;            //单位为像素
        //    Font ft = utilPiping.textFont_wiring;
        //    label.Font = ft;
        //    SizeF size = g.MeasureString(text, ft);

        //    label.Transparent = true;
        //    label.Text = text;
        //    label.Size = new SizeF(size.Width + 15, size.Height);

        //    label.Alignment = Alignment.LeftJustifyTOP;
        //    label.DrawColor = Color.Transparent;
        //    label.Shape.Style = ShapeStyle.Rectangle;
        //    label.Logical = false;
        //    label.Selectable = false;
        //    label.AttachmentStyle = AttachmentStyle.Item;
        //    return label;
        //}

        //private void InitWiringNodes(AddFlow addFlowWiring)
        //{
        //    addFlowWiring.Controls.Clear();
        //    addFlowWiring.Nodes.Clear();
        //    addFlowWiring.AutoScrollPosition = new Point(0, 0);
        //}

        #endregion

        /// 绘制连接线文字节点 add on 20170122 add by axj
        /// <summary>
        /// 绘制连接线文字节点
        /// </summary>
        /// <param name="g"></param>
        /// <param name="nd"></param>
        public void DrawLabelText(Graphics g, Node nd)
        {
            if (nd == null || nd.Text == null)
            {
                return;
            }
            float x = 0;
            if (nd.Alignment == Alignment.CenterBOTTOM || nd.Alignment == Alignment.CenterMIDDLE || nd.Alignment == Alignment.CenterTOP)
            {
                PointF pf = pipingUtil.getTopCenterPointF(nd);
                x = pf.X;
            }
            float h = 0;
            string[] ArrayTxt = nd.Text.Split('\n');
            Brush textBrush = new SolidBrush(nd.TextColor);
            for (int i = 0; i < ArrayTxt.Length; i++)
            {
                var sz = g.MeasureString(ArrayTxt[i], textFont_piping);
                ptText = new PointF(x == 0 ? nd.Location.X : (x - sz.Width / 2), nd.Location.Y + h);
                g.DrawString(ArrayTxt[i], textFont_piping, textBrush, ptText);
                h = h + sz.Height;
            }
        }

    }

}

