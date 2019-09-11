using System.Collections.Generic;
using JCHVRF.MyPipingBLL.NextGen;
using System.IO;
using WL = Lassalle.WPF.Flow;
using NextGenModel = JCHVRF.Model.NextGen;
using System.Drawing;
using pt = System.Windows;


namespace JCHVRF_New.Utility
{
    class ControllerUtilEMF
    {
        public UtilityWiring utilWiring = new UtilityWiring();
        public static List<PointF> ptStart = new List<PointF>();
        public static List<PointF> ptEnd = new List<PointF>();
        float minx = 0;
        float miny = 0;
        float maxX = 0;
        float maxY = 0;

        public void DrawItemRecursion(Graphics g, WL.Item item, string WiringImageDir)
        {
            if (item is WL.Node)
            {
                if (item is NextGenModel.MyNodeGround_Wiring)
                {
                    NextGenModel.MyNodeGround_Wiring ndGnd = item as NextGenModel.MyNodeGround_Wiring;
                    string nodePointsFile = Path.Combine(WiringImageDir, "Ground.txt");
                    DrawNode_wiring(g, ndGnd, nodePointsFile, "", null);
                }
                else
                {
                    WL.Node nd = item as WL.Node;
                    DrawNode(g, nd);
                    if (nd.AddFlow != null)
                    {
                        if (nd.AddFlow.Children != null)
                        {
                            foreach (WL.Item child in nd.AddFlow.Children)
                            {
                                DrawItemRecursion(g, child, WiringImageDir);
                            }
                        }
                    }
                }
            }
            else if (item is WL.Link)
            {
                DrawLine(g, item as WL.Link);
            }

        }

        public void DrawNode(Graphics g, WL.Node nd)
        {
            if (nd == null) return;

            SizeF sf = new SizeF((float)nd.Bounds.Width, (float)nd.Bounds.Height);

            RectangleF rect = new RectangleF(utilWiring.convertSystemPointToDrawingPoint(nd.Location), sf);
            string text = nd.Text;

            pt.Media.SolidColorBrush newBrush = (pt.Media.SolidColorBrush)nd.Stroke;
            Color clr = System.Drawing.Color.FromArgb(newBrush.Color.A, newBrush.Color.R, newBrush.Color.G, newBrush.Color.B);

            if (newBrush.Color.R != 255 || newBrush.Color.G != 255 || newBrush.Color.B != 255)
            {
                Pen pen = new Pen(clr, (float)(0.3f));
                if (nd.Geometry is System.Windows.Media.RectangleGeometry)
                {
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }

            pt.Media.SolidColorBrush newFill = (pt.Media.SolidColorBrush)nd.Fill;
            if (newFill.Color.R != 255 || newFill.Color.G != 255 || newFill.Color.B != 255)
            {
                Color clr1 = System.Drawing.Color.FromArgb(newFill.Color.A, newFill.Color.R, newFill.Color.G, newFill.Color.B);
                Brush brush = new SolidBrush(clr1);
                g.FillRectangle(brush, rect);
            }
            if (!string.IsNullOrEmpty(text))
            {
                Font ft1;

                if (nd.FontSize > 8f)
                {
                    ft1 = new Font("Arial", 8f, GraphicsUnit.Pixel);
                }
                else
                {
                    ft1 = new Font("Arial", 6f, GraphicsUnit.Pixel);
                }
                SizeF textSize = g.MeasureString(text, ft1);
                textSize.Height = textSize.Height * 0.7f;

                float x = 0, y = 0;

                pt.Media.SolidColorBrush textClr = (pt.Media.SolidColorBrush)nd.Foreground;
                Color clr2 = System.Drawing.Color.FromArgb(textClr.Color.A, textClr.Color.R, textClr.Color.G, textClr.Color.B);

                x = rect.X + (rect.Width - textSize.Width) / 2;
                y = rect.Y + (rect.Height - textSize.Height) / 2;

                g.DrawString(text, ft1, new SolidBrush(clr2), x, y);
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

        public void DrawLine(Graphics g, WL.Link line)
        {
            List<PointF> ptl = new List<PointF>();
            foreach (pt.Point p in line.Points)
            {
                ptl.Add(utilWiring.convertSystemPointToDrawingPoint(p));
            }
            if (ptl.Count <= 1)
                return;
            PointF[] pts = ptl.ToArray();
            pt.Media.SolidColorBrush newBrush = (pt.Media.SolidColorBrush)line.Stroke;
            Color clr = System.Drawing.Color.FromArgb(newBrush.Color.A, newBrush.Color.R, newBrush.Color.G, newBrush.Color.B);
            Pen pen = new Pen(clr, 0.1f);
            WL.Node nd = line.Dst;
            if (nd != null && nd is NextGenModel.MyNodeGround_Wiring)
                pen = new Pen(Color.Yellow, 0.1f);
            g.DrawLines(pen, pts);
        }

        public void DrawNode_wiring(Graphics g, WL.Node nd, string nodeTextFile, string name, JCHVRF.MyPipingBLL.NodeElement_Wiring item)
        {
            Font textFont_wiring = new Font("Arial", 8f, System.Drawing.FontStyle.Regular);
            Brush textBrush_wiring = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black, 0.1f);
            if (nd is NextGenModel.MyNodeGround_Wiring)
                pen = new Pen(Color.Black, 0.1f);

            SizeF ndSize = new SizeF((float)nd.Size.Width, (float)nd.Size.Height);
            PointF ndLocation = new PointF((float)nd.Location.X, (float)nd.Location.Y);
            if (nd is NextGenModel.WiringNodeIn)
            {
                ndLocation = new PointF((float)nd.Location.X, (float)nd.Location.Y);
            }

            RectangleF rect = new RectangleF(ndLocation, ndSize);
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

            if (item != null && item.PtCircles.Count >= item.UnitCount * 4)
            {
                pt.Point pt = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStr1), utilWiring.convertPointFToWinPoint(ndLocation));
                g.DrawString(item.Str1, textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt));

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

                    pt.Point pt1 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtModelGroup[i]), utilWiring.convertPointFToWinPoint(ndLocation));

                    g.DrawString(item.ModelGroup[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt1));      // YVOH200
                    pt.Point pt2 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStrGroup1[i]), utilWiring.convertPointFToWinPoint(ndLocation));
                    if (i < 2)
                    {
                        g.DrawString(item.StrGroup1[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt2));    // X Y | X Y
                    }
                    pt.Point pt3 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStrGroup2[i]), utilWiring.convertPointFToWinPoint(ndLocation));
                    g.DrawString(item.StrGroup2[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt3));        // L1L2L3N

                    pt.Point pt4 = UtilEMF.OffsetLocation(utilWiring.convertPointFToWinPoint(item.PtStrGroup3[i]), utilWiring.convertPointFToWinPoint(ndLocation));
                    if (item.UnitCount > 1)
                    {
                        g.DrawString(item.StrGroup3[i], textFont_wiring, textBrush_wiring, utilWiring.convertSystemPointToDrawingPoint(pt4));    // 19A 3Nph
                    }
                }
            }
        }

        private void RelocateLocation(ref RectangleF rectSrc, PointF ptOffset)
        {
            rectSrc.X += ptOffset.X;
            rectSrc.Y += ptOffset.Y;
        }

        private void InitPointF(string filePath)
        {
            ptStart.Clear();
            ptEnd.Clear();

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

        private void RelocatedNode(RectangleF rect)
        {
            SizeF actSize = new SizeF(maxX - minx + 1, maxY - miny - 1);
            if (actSize.Width == 0 || actSize.Height == 0)
                return;
            float scaleX = rect.Width / actSize.Width;
            float scaleY = rect.Height / actSize.Height;
            float scale = scaleX;

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
    }
}
