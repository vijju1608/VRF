using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

//using Microsoft.VisualBasic.PowerPacks;

namespace JCHVRF
{

    public class UtilControl
    {
        public const int vLocationController = 6;
        public const int vLocationGroup = 6;
        public const int hLocationController = 396;
        public const int hLocationGroup = 6;

        /// <summary>
        /// Control group中的Controller，256
        /// </summary>
        public const int hLocationController_group = 256;
        /// <summary>
        /// Control group中的Outdoor，3
        /// </summary>
        public const int hLocationOutdoor_group = 3;

        /// 校验拖拽的源对象为PictureBox类型，且其中的Image对象不为空
        /// <summary>
        /// 校验拖拽的源对象为PictureBox类型，且其中的Image对象不为空
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool CheckDragType_PictureBox(DragEventArgs e)
        {
            string type = typeof(PictureBox).ToString();
            if (e.Data.GetDataPresent(type, false))
            {
                PictureBox pb = (PictureBox)e.Data.GetData(type);
                if (pb != null && pb.Image != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// 校验拖拽的源对象是否为ListViewItem
        /// <summary>
        /// 校验拖拽的源对象是否为ListViewItem
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool CheckDragType_ListViewItem(DragEventArgs e)
        {
            string type = typeof(ListViewItem).ToString();
            if (e.Data.GetDataPresent(type, false))
            {
                return true;
            }
            return false;
        }

        // 目前不考虑Control System这个层次，20141029 clh
        ///// 检验指定容器中是否包含ucDropController类型的控件
        ///// <summary>
        ///// 检验指定容器中是否包含ucDropController类型的控件
        ///// </summary>
        ///// <param name="container"></param>
        ///// <returns></returns>
        //public static bool HasActiveController(Control container)
        //{
        //    foreach (Control c in container.Controls)
        //    {
        //        if (c is ucDropController)
        //        {
        //            if ((c as ucDropController).IsActive)
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        ///// 检查并设置当前界面中所有 ucDropControlGroup 控件的完整性
        ///// <summary>
        ///// 检查并设置当前界面中所有 ucDropControlGroup 控件的完整性:
        ///// system中添加Controller；
        ///// system中删除Controller；
        ///// system中添加group（拖拽第一个Outdoor记录至group时）；
        ///// </summary>
        //public static void CheckControlGroupComplete(ucDropControlSystem ucSystem)
        //{
        //    // 若当前Control system中不含Controller控件
        //    if (!UtilControl.HasActiveController(ucSystem))
        //    {
        //        foreach (Control ctrl2 in ucSystem.Controls)
        //        {
        //            if (ctrl2 is ucDropControlGroup)
        //            {
        //                ucDropControlGroup ucGroup = ctrl2 as ucDropControlGroup;
        //                // 当前Control group中含有Outdoor记录
        //                if (ucGroup.IsOutdoorActive)
        //                {
        //                    if (!UtilControl.HasActiveController(ucGroup))
        //                    {
        //                        ucGroup.SetIncomplete(Msg.CONTROLLER_WARNING_NOCONTROLLER(ucGroup.Title));
        //                    }
        //                    else
        //                    {
        //                        ucGroup.SetComplete();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (Control ctrl2 in ucSystem.Controls)
        //        {
        //            if (ctrl2 is ucDropControlGroup)
        //            {
        //                ucDropControlGroup ucGroup = ctrl2 as ucDropControlGroup;
        //                // 当前Control group中含有Outdoor记录
        //                if (ucGroup.IsOutdoorActive)
        //                {
        //                    ucGroup.SetComplete();
        //                }
        //            }
        //        }
        //    }
        //}

        ///// 检查并设置当前 ucDropControlGroup 控件的完整性
        ///// <summary>
        ///// 检查并设置当前 ucDropControlGroup 控件的完整性:
        ///// group中添加Controller；
        ///// group中删除Controller
        ///// </summary>
        ///// <param name="ucGroup"></param>
        //public static void CheckControlGroupComplete(ucDropControlGroup ucGroup)
        //{
        //    // 当前Control group中含有Outdoor记录
        //    if (ucGroup.IsOutdoorActive)
        //    {
        //        if (ucGroup.Parent is ucDropControlSystem)
        //        {
        //            if (!UtilControl.HasActiveController(ucGroup.Parent as ucDropControlSystem)
        //                && !UtilControl.HasActiveController(ucGroup))
        //            {
        //                ucGroup.SetIncomplete(Msg.CONTROLLER_WARNING_NOCONTROLLER(ucGroup.Title));
        //            }
        //            else
        //            {
        //                ucGroup.SetComplete();
        //            }
        //        }
        //    }
        //}


        ///// 绘制Controller中的连接线
        ///// <summary>
        ///// 绘制Controller中的连接线
        ///// </summary>
        ///// <param name="ucSystem"></param>
        //public static void DrawLine_Controller(ucDropControlSystem ucSystem)
        //{
        //    Graphics g = ucSystem.CreateGraphics();
        //    if (HasActiveController(ucSystem))
        //    {
        //        Pen pen = new Pen(Color.FromArgb(48, 48, 48));
        //        ptArray_Outdoor = new List<Point>();
        //        Point ptController_leftCenter = new Point(0, 0);

        //        foreach (Control c in ucSystem.Controls)
        //        {
        //            if (c is ucDropControlGroup)
        //            {
        //                ucDropControlGroup ucGroup = c as ucDropControlGroup;
        //                if (ucGroup.IsOutdoorActive)
        //                {
        //                    Point p1 = new Point(ucGroup.Location.X + ucGroup.Size.Width, ucGroup.Location.Y + ucGroup.Size.Height / 2);
        //                    ptArray_Outdoor.Add(p1);
        //                }
        //            }
        //            else if (c is ucDropController)
        //            {
        //                if (ptController_leftCenter.Y == 0)
        //                {
        //                    ucDropController ucController = c as ucDropController;
        //                    ptController_leftCenter = new Point(ucController.Location.X, ucController.Location.Y + ucController.Size.Height / 2);
        //                }
        //            }
        //        }

        //        if (ptArray_Outdoor.Count > 0)
        //        {
        //            lines.Clear();
        //            foreach (Point pt in ptArray_Outdoor)
        //            {
        //                Point pt2 = new Point(pt.X + 15, pt.Y);
        //                g.DrawLine(pen, pt, pt2);
        //                LinePoints line = new LinePoints(pt.X, pt2.X, pt.Y, pt2.Y);
        //                lines.Add(line);
        //            }

        //            Point ptEnd = ptArray_Outdoor[ptArray_Outdoor.Count - 1];

        //            Point ptController_end = new Point(ptEnd.X + 15, ptEnd.Y);
        //            Point ptController_2 = new Point(ptController_end.X, ptController_leftCenter.Y);

        //            ptArray_Controller = new Point[] { ptController_leftCenter, ptController_2, ptController_end };
        //            g.DrawLines(pen, ptArray_Controller);

        //            LinePoints line2 = new LinePoints(ptController_2.X, ptController_end.X, ptController_2.Y, ptController_end.Y);
        //            LinePoints line3 = new LinePoints(ptController_2.X, ptController_leftCenter.X, ptController_2.Y, ptController_leftCenter.Y);
        //            lines.Add(line2);
        //            lines.Add(line3);
        //        }
        //    }
        //}

        ///// 绘制Controller中的连接线
        ///// <summary>
        ///// 绘制Controller中的连接线
        ///// </summary>
        ///// <param name="ucSystem"></param>
        //public static void DrawLine_Controller(Panel pnl)
        //{
        //    foreach (Control ctrl in pnl.Controls)
        //    {
        //        ucDropControlSystem ucSystem = ctrl as ucDropControlSystem;
        //        if (ctrl is ucDropControlSystem)
        //        {
        //            Graphics g = pnl.CreateGraphics();
        //            if (HasActiveController(ucSystem))
        //            {
        //                Pen pen = new Pen(Color.FromArgb(48, 48, 48));
        //                ptArray_Outdoor = new List<Point>();
        //                Point ptController_leftCenter = new Point(0, 0);

        //                foreach (Control c in ucSystem.Controls)
        //                {
        //                    if (c is ucDropControlGroup)
        //                    {
        //                        ucDropControlGroup ucGroup = c as ucDropControlGroup;
        //                        if (ucGroup.IsOutdoorActive)
        //                        {
        //                            Point p1 = new Point(ucGroup.Location.X + ucGroup.Size.Width, ucGroup.Location.Y + ucGroup.Size.Height / 2);
        //                            ptArray_Outdoor.Add(p1);
        //                        }
        //                    }
        //                    else if (c is ucDropController)
        //                    {
        //                        if (ptController_leftCenter.Y == 0)
        //                        {
        //                            ucDropController ucController = c as ucDropController;
        //                            ptController_leftCenter = new Point(ucController.Location.X, ucController.Location.Y + ucController.Size.Height / 2);
        //                        }
        //                    }
        //                }

        //                if (ptArray_Outdoor.Count > 0)
        //                {
        //                    lines.Clear();
        //                    foreach (Point pt in ptArray_Outdoor)
        //                    {
        //                        Point pt2 = new Point(pt.X + 15, pt.Y);
        //                        g.DrawLine(pen, pt, pt2);
        //                        LinePoints line = new LinePoints(pt.X, pt2.X, pt.Y, pt2.Y);
        //                        lines.Add(line);
        //                    }

        //                    Point ptEnd = ptArray_Outdoor[ptArray_Outdoor.Count - 1];

        //                    Point ptController_end = new Point(ptEnd.X + 15, ptEnd.Y);
        //                    Point ptController_2 = new Point(ptController_end.X, ptController_leftCenter.Y);

        //                    ptArray_Controller = new Point[] { ptController_leftCenter, ptController_2, ptController_end };
        //                    g.DrawLines(pen, ptArray_Controller);

        //                    LinePoints line2 = new LinePoints(ptController_2.X, ptController_end.X, ptController_2.Y, ptController_end.Y);
        //                    LinePoints line3 = new LinePoints(ptController_2.X, ptController_leftCenter.X, ptController_2.Y, ptController_leftCenter.Y);
        //                    lines.Add(line2);
        //                    lines.Add(line3);
        //                }
        //            }
        //        }
        //    }
        //}

        public static List<Point> ptArray_Outdoor = new List<Point>();
        public static Point[] ptArray_Controller;
        public static List<LinePoints> lines = new List<LinePoints>();

    }

    public class LinePoints
    {
        public LinePoints(int x1, int x2, int y1, int y2)
        {
            _x1 = x1;
            _x2 = x2;
            _y1 = y1;
            _y2 = y2;
        }

        private int _x1;

        public int X1
        {
            get { return _x1; }
        }

        private int _x2;

        public int X2
        {
            get { return _x2; }
        }

        private int _y1;

        public int Y1
        {
            get { return _y1; }
        }

        private int _y2;

        public int Y2
        {
            get { return _y2; }
        }
    }

}