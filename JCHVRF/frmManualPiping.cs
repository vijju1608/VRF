using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF.MyPipingBLL;
using JCBase.Utility;
using JCHVRF.VRFMessage;
using Lassalle.Flow;
using JCBase.UI;

namespace JCHVRF
{
    public partial class frmManualPiping : JCBase.UI.JCForm
    {
        private class RoomIndoorState
        {
            public RoomIndoor RoomIndoorItem { get; set; }
            public TreeNode TreeNodeItem { get; set; }
            public bool Used { get; set; }
        }

        private List<RoomIndoor> roomIndoorList = null;  //当前系统内的所有室内机
        private List<RoomIndoorState> roomIndoorStateList = null;
        private UtilPiping utilPiping = new UtilPiping();

        private Color colorSelect = Color.Blue;
        private Color colorDefault = Color.Black;
        private Color colorTransparent = Color.Transparent;
        private Color colorHover = Color.Orange;

        private Color colorSelectedIndoor = Color.DarkGray;

        public const float VDistance = 120; //纵向间距
        public const float HDistance = 120; //横向间距

        //private bool IsLayoutComplete { get; set; }

        public Project thisProject = null;
        public SystemVRF curSystemItem = null;
        PipingBLL pipBll = null;
        bool isHitachi;
        bool isInch;
        bool isHR;
        string imageDirectory;

        private string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        private string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        private string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;

        Node selNode = null;
        //Node hoverNode = null;
        Node nodeBuildingImage = null;
        Image originalBuildingImage = null;  //户型图的原始图片对象(原始尺寸)
        MyNodePlottingScale nodePlottingScale = null;

        public string DiagramScale
        {
            set
            {
                jccmbScale.Text = value;
            }
            get
            {
                return jccmbScale.Text;
            }
        }

        UndoRedo.UndoRedoHandler UndoRedoUtil = null;  //注册撤销实体对象 add by Shen Junjie 20190521

        public frmManualPiping(Project project, SystemVRF sysItem)
        {
            InitializeComponent();
            this.JCCallValidationManager = true;   //界面验证器

            //根主窗口数据隔离
            project = project.DeepClone();
            sysItem = project.SystemList.Find(p => p.Id == sysItem.Id);

            tvOutdoor.DoubleClick += new EventHandler(tvOutdoor_DoubleClick);

            InitMenus();
            InitAddFlow();

            this.thisProject = project;
            this.curSystemItem = sysItem;

            isHitachi = project.BrandCode == "H";
            isInch = CommonBLL.IsDimension_inch();
            isHR = PipingBLL.IsHeatRecovery(sysItem);
            imageDirectory = MyConfig.PipingNodeImageDirectory;

            pipBll = new PipingBLL(thisProject, utilPiping, addFlowPiping, isInch, ut_weight, ut_length, ut_power);
            
            Initialization(project, sysItem);

            /*注册撤销功能 add by Shen Junjie 20190521 begin */
            UndoRedoUtil = new UndoRedo.UndoRedoHandler(false);
            UndoRedoUtil.ShowIconsInPictureBoxes(pbUndo, pbRedo);

            UndoRedoUtil.GetCurrentProjectEventHandler += delegate (out Project prj) //获取最新项目数据
            {
                SavePiping();
                prj = this.thisProject.DeepClone();//返回当前项目数据的副本
            };
            UndoRedoUtil.ReloadProjectEventHandler += delegate (Project prj) //重新加载历史记录里面的项目数据
            {
                this.thisProject = prj;
                this.curSystemItem = prj.SystemList.Find(p => p.Id == this.curSystemItem.Id);
                Initialization(prj, this.curSystemItem);
            };
            UndoRedoUtil?.SaveProjectHistory();
            /*注册撤销功能 end*/
        }

        private void Initialization(Project project, SystemVRF sysItem)
        {
            if (sysItem.IsManualPiping)
            {
                utilPiping.colorDefault = sysItem.MyPipingNodeOutTemp.PipeColor;
                utilPiping.colorText = sysItem.MyPipingNodeOutTemp.TextColor;
                utilPiping.colorYP = sysItem.MyPipingNodeOutTemp.BranchKitColor;
                utilPiping.colorNodeBg = sysItem.MyPipingNodeOutTemp.NodeBgColor;
            }
            pbLineColor.BackColor = utilPiping.colorDefault;
            pbTextColor.BackColor = utilPiping.colorText;
            pbBranchKitColor.BackColor = utilPiping.colorYP;
            pbNodeBgColor.BackColor = utilPiping.colorNodeBg;

            rdoLineStylePolyLine.Enabled = false;
            rdoLineStyleHV.Enabled = false;
            rdoLineStyleVH.Enabled = false;
            rdoLineStyleHVH.Enabled = false;
            rdoLineStyleVHV.Enabled = false;

            //jcbtnOK.Enabled = false;

            roomIndoorList = (new ProjectBLL(project)).GetSelectedIndoorBySystem(sysItem.Id);
            BindTreeViewOutdoor();
            roomIndoorStateList = new List<RoomIndoorState>();
            foreach (TreeNode tn in tvOutdoor.Nodes[0].Nodes)
            {
                if (tn.Tag != null && (tn.Tag is RoomIndoor))
                {
                    roomIndoorStateList.Add(new RoomIndoorState()
                    {
                        RoomIndoorItem = tn.Tag as RoomIndoor,
                        TreeNodeItem = tn,
                        Used = false
                    });
                }
            }

            ClearPiping();
            DrawPiping(sysItem);

            InitPlottingScale(sysItem);

            SelectFirstAvailableIndoor();
        }

        private void ClearPiping()
        {
            addFlowPiping.Controls.Clear();
            addFlowPiping.Items.Clear();
            addFlowPiping.Images.Clear();
            selNode = null;
            nodeBuildingImage = null;
            originalBuildingImage = null;  //户型图的原始图片对象(原始尺寸)
            nodePlottingScale = null;
        }

        /// <summary>
        /// 在左边室内机列表选中第一个可用的室内机
        /// </summary>
        private void SelectFirstAvailableIndoor()
        {
            if (tvOutdoor.SelectedNode != null) return;

            foreach (RoomIndoorState nodeState in roomIndoorStateList)
            {
                if (!nodeState.Used)
                {
                    tvOutdoor.SelectedNode = nodeState.TreeNodeItem;
                    //tvOutdoor_AfterSelect(tvOutdoor, new TreeViewEventArgs(nodeState.TreeNodeItem));
                    break;
                }
            }
        }

        /// <summary>
        /// 在左边室内机列表选中下一个可用的室内机
        /// </summary>
        private void SelectNextAvailableIndoor()
        {
            if (tvOutdoor.SelectedNode == null)
            {
                SelectFirstAvailableIndoor();
                return;
            }

            bool foundCurrent = false;
            bool hasAvailable = false;
            do
            {
                foreach (RoomIndoorState nodeState in roomIndoorStateList)
                {
                    if (!nodeState.Used)
                    {
                        hasAvailable = true;
                        if (foundCurrent)
                        {
                            tvOutdoor.SelectedNode = nodeState.TreeNodeItem;
                            //tvOutdoor_AfterSelect(tvOutdoor, new TreeViewEventArgs(nodeState.TreeNodeItem));
                            return;
                        }
                    }
                    if (nodeState.TreeNodeItem == tvOutdoor.SelectedNode)
                    {
                        foundCurrent = true;
                        continue;
                    }
                }
            } while (hasAvailable);
            tvOutdoor.SelectedNode = null;
        }

        private void DrawPiping(SystemVRF sysItem)
        {
            if (sysItem != null && sysItem.MyPipingNodeOutTemp != null && sysItem.OutdoorItem != null)
            {
                //根据MyPipingNodeOutTemp生成新的节点结构,防止影响原来的Piping图
                pipBll.LoadPipingNodeStructure(sysItem);
                //根据MyPipingNodeOutTemp保存的坐标还原各节点和连接线，不重算坐标
                pipBll.DrawPipingNodesNoCaculation(imageDirectory, sysItem);
                
                //绘制背景图
                if (sysItem.MyPipingBuildingImageNodeTemp != null)
                {
                    nodeBuildingImage = pipBll.DrawBuildingImageNode(sysItem.MyPipingBuildingImageNodeTemp, out originalBuildingImage);
                }

                InitPipingNodes(sysItem.MyPipingNodeOut);
                CheckLayoutComplete();
            } 
            LockBuildingImageNode(true);
        }

        /// <summary>
        /// 绘制比例尺
        /// </summary>
        private void InitPlottingScale(SystemVRF sysItem)
        {
            jcbtnEnablePlottingScale.Visible = false;
            pbPlottingScaleColor.Visible = false;
            pbPlottingScaleRotation.Visible = false;
            jctxtPlottingScale.Visible = false;
            jclblPlottingScaleUnit.Visible = false;
            jclblPlottingScaleNotAvailable.Visible = false;

            if (sysItem.IsInputLengthManually)
            {
                tmpNodePlottingScale tmp = sysItem.MyPipingPlottingScaleNodeTemp;
                if (tmp != null)
                {
                    EnablePlottingScale(tmp, true);
                }
                else
                {
                    jcbtnEnablePlottingScale.Visible = true;
                }
            }
            else
            {
                jclblPlottingScaleNotAvailable.Visible = true;
            }
        }

        private void EnablePlottingScale(tmpNodePlottingScale tmp, bool enable)
        {
            jcbtnEnablePlottingScale.Visible = !enable;
            Application.DoEvents();
            pbPlottingScaleColor.Visible = enable;
            pbPlottingScaleRotation.Visible = enable;
            jctxtPlottingScale.Visible = enable;
            jclblPlottingScaleUnit.Visible = enable;

            if (!enable)
            {
                nodePlottingScale = null;
                return;
            }

            string text = "";
            nodePlottingScale = pipBll.DrawPlottingScaleNode(null);
            if (tmp != null)
            {
                nodePlottingScale.ActualLength = tmp.ActualLength;
                nodePlottingScale.IsVertical = tmp.IsVertical;

                nodePlottingScale.Size = tmp.Size;
                nodePlottingScale.Location = tmp.Location;
                nodePlottingScale.DrawColor = tmp.DrawColor;
                nodePlottingScale.FillColor = tmp.FillColor;
                nodePlottingScale.TextColor = tmp.TextColor;
            }
            double plottingScaleLength = nodePlottingScale.ActualLength;
            text = Unit.ConvertToControl(plottingScaleLength, UnitType.LENGTH_M, ut_length).ToString("0.##");
            //nodePlottingScale.Text = text1;
            nodePlottingScale.ActualLengthString = text + " " + ut_length;
            pipBll.DrawPlottingScaleNode(nodePlottingScale);
            if (nodePlottingScale != null)
            {
                pipBll.CalculateAllPipeLengthByPlottingScale(curSystemItem, nodePlottingScale.PlottingScale);
            }

            jctxtPlottingScale.Text = text;
            jclblPlottingScaleUnit.Text = ut_length;
            pbPlottingScaleColor.BackColor = nodePlottingScale.FillColor;
            SetPlottingScaleRotationIcon();
        }

        private void InitPipingNodes(Node node)
        {
            if (node == null) return;

            node.XMoveable = true;
            node.YMoveable = true;
            node.XSizeable = false;
            node.YSizeable = false;

            if (node.InLinks != null)
            {
                foreach (Link link in node.InLinks)
                {
                    link.Selectable = true;
                    link.Stretchable = true;
                }
            }

            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                InitPipingNodes(nodeOut.ChildNode);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                nodeYP.YMoveable = false;
                nodeYP.XMoveable = false;
                List<Node> children = new List<Node>(nodeYP.ChildNodes); //为了防止在子节点被删时，索引有变化
                for (int i = 0; i < children.Count; ++i)
                {
                    InitPipingNodes(children[i]);

                    if (curSystemItem.IsPipingVertical && i < children.Count - 1)
                    {
                        if (children[i] != null && children[i].InLinks != null)
                        {
                            foreach (Link link in children[i].InLinks)
                            {
                                link.Line.Style = LineStyle.HVH;
                            }
                        }
                    }
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                InitPipingNodes(nodeCH.ChildNode);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                List<Node> children = new List<Node>(nodeMCH.ChildNodes); //为了防止在子节点被删时，索引有变化
                for (int i = 0; i < children.Count; ++i)
                {
                    InitPipingNodes(children[i]);
                }
            }
            else if (node is MyNodeIn)
            {
                MyNodeIn nodeIn = node as MyNodeIn;

                // 节点上关联的室内机
                RoomIndoor riItem = nodeIn.RoomIndooItem;
                if (riItem != null)
                {
                    nodeIn.RoomIndooItem = null;
                    RoomIndoorState ris = roomIndoorStateList.Find(p => p.RoomIndoorItem.IndoorNO == riItem.IndoorNO);
                    if (ris != null)
                    {
                        nodeIn.RoomIndooItem = ris.RoomIndoorItem;
                        ris.TreeNodeItem.ForeColor = colorSelectedIndoor;
                        ris.Used = true;
                    }
                    else
                    {
                        //如果室内机已经从系统中删除，则从配管图上相应删除。
                        DeleteNode(nodeIn);
                    }
                }
                else
                {
                    //如果室内机已经从系统中删除，则从配管图上相应删除。
                    DeleteNode(nodeIn);
                }
            }
        }

        private void tvOutdoor_DoubleClick(object sender, EventArgs e)
        {
            //AddIndoorNode(selNode, -1);
        }

        private void InitAddFlow()
        {
            jccmbScale.Text = "100";

            addFlowPiping.MouseAction = MouseAction.None;   // 鼠标动作
            addFlowPiping.MultiSel = false;       // 是否允许多选
            addFlowPiping.AllowDrop = false;     // 外界拖入addflow
            addFlowPiping.AutoScroll = true;
            addFlowPiping.ScrollbarsDisplayMode = ScrollbarsDisplayMode.AddControlSize; //滚动条永远显示

            // 不允许用户直接拖动连线
            addFlowPiping.CanChangeOrg = false;   // 是否允许更改连线的源节点
            addFlowPiping.CanChangeDst = false;   // 是否允许更改连线的目标节点
            addFlowPiping.CanDrawNode = false;   // 是否允许用户绘制节点
            addFlowPiping.CanDrawLink = false;
            addFlowPiping.CanReflexLink = true;
            addFlowPiping.CanStretchLink = true;

            addFlowPiping.CanLabelEdit = false;  // 是否允许用户编辑节点文字
            addFlowPiping.CanSizeNode = true;   // 是否允许用户更改节点尺寸
            addFlowPiping.CanMoveNode = true;   // 是否允许用户拖动节点
            addFlowPiping.CanDragScroll = true;

            addFlowPiping.IsUnselectableItemHitable = true;  //不能选中的Node可以被获取到
            addFlowPiping.DefNodeProp.DrawWidth = 1;
            addFlowPiping.DefNodeProp.DashStyle = DashStyle.Solid;

            addFlowPiping.LinkCreationMode = LinkCreationMode.AllNodeArea; // 绘制 Link 时选中的控制区域，整个Node区域
            addFlowPiping.LinkHandleSize = HandleSize.Small;
            addFlowPiping.LinkSelectionAreaWidth = LinkSelectionAreaWidth.Small;
            addFlowPiping.SelectionHandleSize = HandleSize.Small;

            addFlowPiping.SendToBack();
            addFlowPiping.Images.Clear();

            addFlowPiping.MouseDown += new MouseEventHandler(this.addFlowPiping_MouseDown);
            //addFlowPiping.MouseWheel += new MouseEventHandler(this.addFlowPiping_MouseWheel); //无法阻止滚动条滚动，所以放弃
            addFlowPiping.AfterStretch += new Lassalle.Flow.AddFlow.AfterStretchEventHandler(this.AddFlowPiping_AfterStretch);
            addFlowPiping.SelectionChange += new Lassalle.Flow.AddFlow.SelectionChangeEventHandler(this.addFlowPiping_SelectionChange);
            addFlowPiping.AfterMove += new Lassalle.Flow.AddFlow.AfterMoveEventHandler(this.addFlowPiping_AfterMove);
            addFlowPiping.AfterResize += new Lassalle.Flow.AddFlow.AfterResizeEventHandler(this.addFlowPiping_AfterResize);
        }

        private void AddFlowPiping_AfterStretch(object sender, EventArgs e)
        {
            if (addFlowPiping.SelectedItem is MyLink)
            {
                Link lk = addFlowPiping.SelectedItem as Link;

                //将起始点拽回到节点上
                ResetLinkPointLocation(lk, true);

                //将目标点拽回到节点上
                ResetLinkPointLocation(lk, false);

                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void ResetLinkPointLocation(Link lk, bool startOrEndPoint)
        {
            bool isStraightLine = false;  //是否是直角转弯直线类型（包括VH,HV,VHV,HVH...）
            bool isFirstPointFixed = false;
            bool isMidX = true;
            bool isMidY = true;
            bool connectTopBottom = false;  //连接到上下边框
            bool connectLeftRight = false;  //连接到左右边框

            if (lk.Line.Style >= LineStyle.VH)
            {
                isStraightLine = true;
                if (startOrEndPoint)
                {
                    if (lk.Line.Style.ToString().StartsWith("V"))
                    {
                        connectTopBottom = true;
                    }
                }
                else
                {
                    if (lk.Line.Style.ToString().EndsWith("V"))
                    {
                        connectTopBottom = true;
                    }
                }
                connectLeftRight = !connectTopBottom;
            }
            else
            {
                connectTopBottom = true;
                connectLeftRight = true;
            }

            int index1, index2;
            Node node;
            PointF pt1, pt2;
            if (startOrEndPoint)
            {
                index1 = 0;
                index2 = index1 + 1;
                node = lk.Org;

                pt1 = lk.Points[index1];
                if (node is MyNodeOut)
                {
                    //起始点是室外机，连线点是固定的左下角
                    pt1 = utilPiping.getLeftBottomPointF(node);
                    isFirstPointFixed = true;
                }
                else if (node is MyNodeYP)
                {
                    if ((node as MyNodeYP).IsCP)
                    {
                        //起点为header branch, 不强制连接中点
                        isMidX = false;
                        isMidY = false;
                        connectTopBottom = false;
                    }
                    else
                    {
                        //起始点是branch kit，连线点是中心（header branch除外）
                        pt1 = utilPiping.getCenterPointF(node);
                        isFirstPointFixed = true;
                    }
                }
                else if (node is MyNodeMultiCH)
                {
                    //起点为Multi CH Box， 不强制连接中点
                    isMidX = false;
                    isMidY = false;
                    connectTopBottom = false;
                }
            }
            else
            {
                index1 = lk.Points.Count - 1;
                index2 = index1 - 1;
                node = lk.Dst;

                pt1 = lk.Points[index1];
                if (node is MyNodeYP)
                {
                    //目标点是branch kit，则连线点是中心（包括header branch）
                    pt1 = utilPiping.getCenterPointF(node);
                    isFirstPointFixed = true;
                }
            }

            pt2 = lk.Points[index2];

            float x1, x2, y1, y2, rx1, rx2, ry1, ry2, xMid, yMid;
            x1 = pt1.X;
            y1 = pt1.Y;
            x2 = pt2.X;
            y2 = pt2.Y;
            rx1 = node.Location.X;
            ry1 = node.Location.Y;
            rx2 = rx1 + node.Size.Width;
            ry2 = ry1 + node.Size.Height;
            xMid = (rx1 + rx2) / 2f;
            yMid = (ry1 + ry2) / 2f;

            if (!isFirstPointFixed)
            {
                if (connectTopBottom && connectLeftRight && isMidX && isMidY)
                {
                    //如果接线点可以是4个边框上的中点，则自动吸附到最近的接线点
                    double d1 = GetDistance(x1, y1, xMid, ry1); //上中点
                    double d2 = GetDistance(x1, y1, rx2, yMid); //右中点
                    double d3 = GetDistance(x1, y1, xMid, ry2); //下中点
                    double d4 = GetDistance(x1, y1, rx1, yMid); //左中点
                    
                    if (d1 <= d2 && d1 <= d3 && d1 <= d4)
                    {
                        x1 = xMid;
                        y1 = ry1;
                    }
                    else if (d2 <= d1 && d2 <= d3 && d2 <= d4)
                    {
                        x1 = rx2;
                        y1 = yMid;
                    }
                    else if (d3 <= d1 && d3 <= d2 && d3 <= d4)
                    {
                        x1 = xMid;
                        y1 = ry2;
                    }
                    else
                    {
                        x1 = rx1;
                        y1 = yMid;
                    }
                }

                if (!connectLeftRight)
                {
                    //自动吸附顶边框或底边框
                    if (Math.Abs(y2 - ry1) < Math.Abs(y2 - ry2))
                    {
                        y1 = ry1;
                    }
                    else
                    {
                        y1 = ry2;
                    }
                }

                if (!connectTopBottom)
                {
                    //自动吸附左边框或右边框
                    if (Math.Abs(x2 - rx1) < Math.Abs(x2 - rx2))
                    {
                        x1 = rx1;
                    }
                    else
                    {
                        x1 = rx2;
                    }
                }

                if (connectTopBottom && !connectLeftRight)
                {
                    if (isMidX)
                    {
                        //x坐标强制放在边框中点 (大多数情况）
                        x1 = xMid;
                    }
                    else
                    {
                        //x坐标需要在边框范围内 (暂时没有Free x的情况)
                        if (x1 < rx1)
                        {
                            x1 = rx1;
                        }
                        if (x1 > rx2)
                        {
                            x1 = rx2;
                        }
                    }
                }

                if (connectLeftRight && !connectTopBottom)
                {
                    if (isMidY)
                    {
                        //y坐标强制放在边框中点 (大多数情况）
                        y1 = yMid;
                    }
                    else
                    {
                        //y坐标需要在边框范围内 (起始点是 header branch or multi CH-Box)
                        if (y1 < ry1)
                        {
                            y1 = ry1;
                        }
                        if (y1 > ry2)
                        {
                            y1 = ry2;
                        }
                    }
                }
            }

            if (isStraightLine)
            {
                if (connectTopBottom)
                {
                    x2 = x1;
                }
                else
                {
                    y2 = y1;
                }
            }

            lk.Points[index1] = new PointF(x1, y1);
            lk.Points[index2] = new PointF(x2, y2);
        }

        private double GetDistance(float x1, float y1, float x2, float y2)
        {
            float a = Math.Abs(x1 - x2);
            float b = Math.Abs(y1 - y2);
            return Math.Sqrt(a * a + b * b);
        }

        //private void addFlowPiping_MouseWheel(object sender, MouseEventArgs e)
        //{
        //    Item item = addFlowPiping.SelectedItem;
        //    if (nodeBuildingImage != null && item == nodeBuildingImage)
        //    {
        //        double scale = e.Delta > 0 ? 1.1 : 0.9;
        //        //scale = Math.Pow(scale, (double)Math.Abs(e.Delta));

        //        SizeF size = nodeBuildingImage.Size;
        //        int width = (int)Math.Round(size.Width * scale);
        //        int height = (int)Math.Round(size.Height * scale);

        //        nodeBuildingImage.Size = new SizeF(width, height);
        //        System.Drawing.Image img = new Bitmap(originalBuildingImage, width, height);
        //        addFlowPiping.Images[nodeBuildingImage.ImageIndex].Image = img;

        //        //阻止滚动事件
        //        HandledMouseEventArgs h = e as HandledMouseEventArgs;
        //        if (h != null)
        //        {
        //            h.Handled = true;
        //        }
        //        addFlowPiping.ScrollPosition = new Point(addFlowPiping.ScrollPosition.X, addFlowPiping.ScrollPosition.Y + e.Delta);
        //    }
        //}

        private void InitMenus()
        {
            menuAddInd.Click += new EventHandler(menuAddInd_Click);
            menuAddYP.Click += new EventHandler(menuAddYP_Click);
            menuAddCP.Click += new EventHandler(menuAddCP_Click);
            menuAddCHBox.Click += new EventHandler(menuAddCHBox_Click);
            menuAddMultiCHBox.Click += new EventHandler(menuAddMultiCHBox_Click);
            menuDeleteNode.Click += new EventHandler(menuDeleteNode_Click);
        }

        private void menuAddInd_Click(object sender, EventArgs e)
        {
            if (menuAddInd.DropDownItems.Count > 0) return;
            AddIndoorNode(selNode, PipingOrientation.Right);
        }

        private void menuAddCHBox_Click(object sender, EventArgs e)
        {
            if (menuAddCHBox.DropDownItems.Count > 0) return;
            AddCHBoxNode(selNode, PipingOrientation.Right);
        }

        private void menuAddMultiCHBox_Click(object sender, EventArgs e)
        {
            if (menuAddMultiCHBox.DropDownItems.Count > 0) return;
            AddMultiCHBoxNode(selNode, PipingOrientation.Right);
        }

        private void menuAddYP_Click(object sender, EventArgs e)
        {
            if (menuAddYP.DropDownItems.Count > 0) return;
            AddYPNode(selNode, PipingOrientation.Right);
        }

        private void menuAddCP_Click(object sender, EventArgs e)
        {
            if (menuAddCP.DropDownItems.Count > 0) return;
            AddCPNode(selNode, PipingOrientation.Right);
        }

        private void menuDeleteNode_Click(object sender, EventArgs e)
        {
            DeleteNode(selNode);
            //Relayout();
            CheckLayoutComplete();
            SelectFirstAvailableIndoor();

            UndoRedoUtil?.SaveProjectHistory();
        }

        private void addFlowPiping_MouseDown(object sender, MouseEventArgs e)
        {
            Item item = addFlowPiping.GetItemAt(addFlowPiping.PointToAddFlow(e.Location));
            Node selNode = null;
            if (item != null && item is Node)
            {
                selNode = item as Node;

                if (addFlowPiping.SelectedItems.Count > 0)
                {
                    addFlowPiping.SelectedItems.Clear();
                }
                selNode.Selected = true;
                this.selNode = selNode;
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    //PointF ptf = utilPiping.getCenterPointF(selNode);
                    PointF ptf = e.Location;
                    //ptf = addFlowPiping.PointFToDevice(ptf);
                    Point pt = new Point((int)ptf.X, (int)ptf.Y);
                    contextMenuStrip1.Show(addFlowPiping, pt);
                }
            }
        }

        /// <summary>
        /// 绑定Outdoor的树控件
        /// </summary>
        private void BindTreeViewOutdoor()
        {
            tvOutdoor.Nodes.Clear();
            TreeNode tnOut = tvOutdoor.Nodes.Add(curSystemItem.Name);
            Global.BindTreeNodeOut(tnOut, curSystemItem, roomIndoorList, thisProject);
            if (tvOutdoor.Nodes.Count > 0)
            {
                this.Cursor = Cursors.Default;
            }
            tvOutdoor.ExpandAll();

            //加上房间名 add by Shen Junjie on 2018/4/17
            foreach (TreeNode tn in tnOut.Nodes)
            {
                if (tn.Tag is RoomIndoor)
                {
                    RoomIndoor ri = tn.Tag as RoomIndoor;

                    string indoorName = ri.IndoorFullName;
                    if (!string.IsNullOrEmpty(ri.RoomID))
                    {
                        string floorName = "";
                        foreach (Floor f in thisProject.FloorList)
                        {
                            foreach (Room rm in f.RoomList)
                            {
                                if (rm.Id == ri.RoomID)
                                {
                                    floorName = f.Name;
                                }
                            }
                        }
                        indoorName = floorName + ":" + ri.RoomName + ":" + indoorName;
                    }
                    tn.Text = indoorName;
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (selNode == null)
                return;

            if (selNode == nodePlottingScale || selNode == nodeBuildingImage)
            {
                menuAddInd.Visible = false;
                menuAddYP.Visible = false;
                menuAddCP.Visible = false;
                menuAddCHBox.Visible = false;
                menuAddMultiCHBox.Visible = false;
                menuDeleteNode.Enabled = true;
                contextMenuStrip1.Refresh();
                return;
            }

            menuAddInd.Visible = true;
            menuAddYP.Visible = true;
            menuAddCP.Visible = false;
            menuAddCHBox.Visible = isHR;
            menuAddMultiCHBox.Visible = isHR;

            RoomIndoor selRoomIndoor = null;
            if (tvOutdoor.SelectedNode != null && tvOutdoor.SelectedNode.Tag is RoomIndoor)
            {
                selRoomIndoor = tvOutdoor.SelectedNode.Tag as RoomIndoor;
            }

            menuAddInd.Enabled = (selRoomIndoor != null);
            menuAddYP.Enabled = true;
            menuAddCP.Enabled = true;
            menuAddCHBox.Enabled = true;
            menuAddMultiCHBox.Enabled = true;
            menuDeleteNode.Enabled = true;

            string model = curSystemItem.OutdoorItem.ModelFull;
            string fcode = model.Substring(model.Length - 1, 1);
            if (fcode != "Q")
            {
                // Belinda,只有SMZ与HAPB有梳型分歧管
                menuAddCP.Visible = true;
            }

            ToolStripMenuItem[] menus = new ToolStripMenuItem[]{
                    menuAddInd,
                    menuAddYP,
                    menuAddCP,
                    menuAddCHBox,
                    menuAddMultiCHBox
                };

            menuAddInd.Text = Msg.MANUAL_PIPING_MENU_ADD_IND(selRoomIndoor == null ? "-" : selRoomIndoor.IndoorName);
            menuAddYP.Text = Msg.MANUAL_PIPING_MENU_ADD_YP; // "Add a &tertiary branch";
            menuAddCP.Text = Msg.MANUAL_PIPING_MENU_ADD_CP; // "Add a &header branch";
            menuAddCHBox.Text = Msg.MANUAL_PIPING_MENU_ADD_CHBox; // "Add a &CHBox";
            menuAddMultiCHBox.Text = Msg.MANUAL_PIPING_MENU_ADD_MULTI_CHBOX;
            foreach (ToolStripMenuItem menuItem in menus)
            {
                menuItem.DropDownItems.Clear();
            }
            if (selNode is MyNodeYP)
            {
                MyNodeYP nodeYP = selNode as MyNodeYP;
                List<PipingOrientation> directions = new List<PipingOrientation>(); 
                if (nodeYP.IsCP)
                {
                    directions.Add(pipBll.CheckLinkOrientation(nodeYP, nodeYP.MyInLinks[0]));
                    //为了画线方便并且不重叠，header branch kit 不能往上下两个方向加子节点 add by Shen Junjie on 2018/8/9
                    directions.Add(PipingOrientation.Up);
                    directions.Add(PipingOrientation.Down);
                }
                else
                {
                    directions = pipBll.CheckYPLinkOrientation(nodeYP);
                }
                InitSubMenu(menus, selRoomIndoor, directions);
            }
            else if (selNode is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = selNode as MyNodeMultiCH;
                List<PipingOrientation> directions = new List<PipingOrientation>();
                directions.Add(pipBll.CheckLinkOrientation(nodeMCH, nodeMCH.MyInLinks[0]));
                //为了画线方便并且不重叠，multi ch Box 不能往上下两个方向加子节点 add by Shen Junjie on 2018/8/9
                directions.Add(PipingOrientation.Up);
                directions.Add(PipingOrientation.Down);
                InitSubMenu(menus, selRoomIndoor, directions);
            }
            else if (selNode is MyNodeCH)
            {
                MyNodeCH nodeCH = selNode as MyNodeCH;
                List<PipingOrientation> directions = new List<PipingOrientation>();
                directions.Add(pipBll.CheckLinkOrientation(nodeCH, nodeCH.MyInLinks[0]));
                InitSubMenu(menus, selRoomIndoor, directions);
            }

            if (selNode is MyNodeOut)
            {
                MyNodeOut nodeOut = selNode as MyNodeOut;
                if (nodeOut.ChildNode != null)
                {
                    menuAddInd.Enabled = false;
                    menuAddYP.Enabled = false;
                    menuAddCP.Enabled = false;
                    menuAddCHBox.Enabled = false;
                    menuAddMultiCHBox.Enabled = false;
                }
                menuDeleteNode.Enabled = false;
            }
            else if (selNode is MyNodeIn)
            {
                menuAddInd.Enabled = false;
                menuAddYP.Enabled = false;
                menuAddCP.Enabled = false;
                menuAddCHBox.Enabled = false;
                menuAddMultiCHBox.Enabled = false;
            }
            else 
            { 
                //向上查找是否已有梳形管, 梳形管下面不能再添加梳形管或者分歧管
                if (pipBll.ExistCPUpward(selNode))
                {
                    menuAddYP.Enabled = false;
                    menuAddCP.Enabled = false;
                    menuAddMultiCHBox.Enabled = false;
                }

                if (isHR)
                {
                    //向上查找是否已有CH Box, CH Box下面不能再添加CH Box
                    if (pipBll.ExistCHBoxUpward(selNode))
                    {
                        menuAddCHBox.Enabled = false;
                        menuAddMultiCHBox.Enabled = false;
                    }
                }
                
                if (selNode is MyNodeYP)
                {
                    MyNodeYP nodeYP = selNode as MyNodeYP;

                    if (nodeYP.ChildCount >= nodeYP.MaxCount)
                    {
                        menuAddInd.Enabled = false;
                        menuAddYP.Enabled = false;
                        menuAddCP.Enabled = false;
                        menuAddCHBox.Enabled = false;
                        menuAddMultiCHBox.Enabled = false;
                    }
                }
                else if (selNode is MyNodeCH)
                {
                    MyNodeCH nodeCH = selNode as MyNodeCH;

                    if (nodeCH.ChildNode != null)
                    {
                        menuAddInd.Enabled = false;
                        menuAddYP.Enabled = false;
                        menuAddCP.Enabled = false;
                    }
                }
                else if (selNode is MyNodeMultiCH)
                {
                    MyNodeMultiCH nodeMCH = selNode as MyNodeMultiCH;
                    if (nodeMCH.ChildNodes.Count >= 16)
                    {
                        menuAddInd.Enabled = false;
                        menuAddYP.Enabled = false;
                        menuAddCP.Enabled = false;
                    }
                }
            }
            contextMenuStrip1.Refresh();
        }

        private void InitSubMenu(ToolStripMenuItem[] menus, RoomIndoor selRoomIndoor, List<PipingOrientation> forbiddenDirections)
        {
            menuAddInd.Text = Msg.MANUAL_PIPING_MENU_ADD_IND1(selRoomIndoor == null ? "-" : selRoomIndoor.IndoorName); // "Add the selected &indoor unit to ...";
            menuAddYP.Text = Msg.MANUAL_PIPING_MENU_ADD_YP1; // "Add a &tertiary branch to ...";
            menuAddCP.Text = Msg.MANUAL_PIPING_MENU_ADD_CP1; // "Add a &header branch to ...";
            menuAddCHBox.Text = Msg.MANUAL_PIPING_MENU_ADD_CHBox1; // "Add a &CHBox to ...";
            menuAddMultiCHBox.Text = Msg.MANUAL_PIPING_MENU_ADD_MULTI_CHBOX1; // "Add a &Multiple CHBox to ...";
            foreach (ToolStripMenuItem menuItem in menus)
            {
                ToolStripButton item;
                item = new ToolStripButton();

                item.Text = "(&1) " + Msg.MANUAL_PIPING_MENU_ADD_TO_ABOVE; // "(&1) Add to above ↑ ";
                item.Click += new EventHandler(delegate(object sender1, EventArgs e1)
                {
                    AddNodeToAddFlow(sender1 as ToolStripItem, PipingOrientation.Up);
                });
                //item.Enabled = nodeYP.ChildNodes[0] == null;
                item.Enabled = !forbiddenDirections.Contains(PipingOrientation.Up);
                menuItem.DropDownItems.Add(item);

                item = new ToolStripButton();

                item.Text = "(&2) " + Msg.MANUAL_PIPING_MENU_ADD_TO_RIGHT; // "(&2) Add to right → ";
                item.Click += new EventHandler(delegate(object sender1, EventArgs e1)
                {
                    AddNodeToAddFlow(sender1 as ToolStripItem, PipingOrientation.Right);
                });
                //item.Enabled = nodeYP.ChildNodes[1] == null;
                item.Enabled = !forbiddenDirections.Contains(PipingOrientation.Right);
                menuItem.DropDownItems.Add(item);

                item = new ToolStripButton();
                item.Text = "(&3) " + Msg.MANUAL_PIPING_MENU_ADD_TO_BELOW; // "(&2) Add to below ↓ ";
                item.Click += new EventHandler(delegate(object sender1, EventArgs e1)
                {
                    AddNodeToAddFlow(sender1 as ToolStripItem, PipingOrientation.Down);
                });
                item.Enabled = !forbiddenDirections.Contains(PipingOrientation.Down);
                menuItem.DropDownItems.Add(item);

                item = new ToolStripButton();
                item.Text = "(&4) " + Msg.MANUAL_PIPING_MENU_ADD_TO_LEFT; // "(&1) Add to left ← ";
                item.Click += new EventHandler(delegate(object sender1, EventArgs e1)
                {
                    AddNodeToAddFlow(sender1 as ToolStripItem, PipingOrientation.Left);
                });
                item.Enabled = !forbiddenDirections.Contains(PipingOrientation.Left);
                menuItem.DropDownItems.Add(item);
            }
        }

        private List<string> CheckYPLinkDirection(MyNodeYP nodeYP)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < nodeYP.Links.Count; i++)
            {
                string result1 = CheckLinkDirection(nodeYP, nodeYP.Links[i]);
                if (!string.IsNullOrEmpty(result1))
                {
                    result.Add(result1);
                }
            }
            return result;
        }

        private string CheckLinkDirection(Node node, Link link)
        {
            if (node == null || link == null) return null;

            bool left = false;
            bool right = false;
            bool up = false;
            bool down = false;
            //父节点四个顶点的坐标
            float x1 = node.Location.X;
            float x2 = node.Location.X + node.Size.Width;
            float y1 = node.Location.Y;
            float y2 = node.Location.Y + node.Size.Height;

            //验证折线的每一段，正反各检测一次
            PointF pt1, pt2;
            bool done = false;
            for (int i = 0; !done && i < link.Points.Count - 1; i++)
            {
                for (int j = 0; !done && j <= 1; j++)
                {
                    if (j == 0)
                    {
                        //正向
                        pt1 = link.Points[i];
                        pt2 = link.Points[i + 1];
                    }
                    else
                    {
                        //反向
                        pt1 = link.Points[i + 1];
                        pt2 = link.Points[i];
                    }

                    //判断线段起始坐标是否在节点范围内
                    if (pt1.X >= x1 && pt1.X <= x2 && pt1.Y >= y1 && pt1.Y <= y2)
                    {
                        //判断线段结束坐标是否在节点范围外
                        if (pt2.X < x1 || pt2.X > x2 || pt2.Y < y1 || pt2.Y > y2)
                        {
                            //判断线段1的方向
                            if (pt1.X == pt2.X)
                            {
                                //竖线
                                if (pt2.Y < y1) up = true; //向上
                                if (pt2.Y > y2) down = true; //向下
                            }
                            if (pt1.Y == pt2.Y)
                            {
                                //横线
                                if (pt2.X < x1) left = true; //向左
                                if (pt2.X > x2) right = true; //向右
                            }
                            done = true;
                        }
                    }
                }
            }

            if (up) return "up";
            if (down) return "down";
            if (left) return "left";
            if (right) return "right";

            return null;
        }

        private void AddNodeToAddFlow(ToolStripItem menu, PipingOrientation direction)
        {
            if (selNode == null) return;

            ToolStripItem parentMenu = menu.OwnerItem;
            if (parentMenu == menuAddInd)
            {
                AddIndoorNode(selNode, direction);
            }
            else if (parentMenu == menuAddYP)
            {
                AddYPNode(selNode, direction);
            }
            else if (parentMenu == menuAddCP)
            {
                AddCPNode(selNode, direction);
            }
            else if (parentMenu == menuAddCHBox)
            {
                AddCHBoxNode(selNode, direction);
            }
            else if (parentMenu == menuAddMultiCHBox)
            {
                AddMultiCHBoxNode(selNode, direction);
            }
        }

        /// <summary>
        /// 向下或者向上查找指定类型的节点数量
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeType">"yp": 分歧管(tertiary branch), "cp": 梳形管(header branch), "chbox": CH Box, "ind": Indoor unit</param>
        /// <param name="isContainSelf">收否要计算当前的节点</param>
        /// <param name="isReverse">是否是向上查找</param>
        /// <returns></returns>
        private int GetNodeCount(Node node, NodeType nodeType, bool isContainSelf, bool isReverse)
        {
            if (node == null) return 0;
            int count = 0;

            if (isContainSelf)
            {
                switch (nodeType)
                {
                    case NodeType.YP: //分歧管(tertiary branch)
                        if (node is MyNodeYP && !(node as MyNodeYP).IsCP) count++;
                        break;
                    case NodeType.CP: //梳形管(header branch)
                        if (node is MyNodeYP && (node as MyNodeYP).IsCP) count++;
                        break;
                    case NodeType.CHbox: //CH Box
                        if (node is MyNodeCH) count++;
                        break;
                    case NodeType.MultiCHbox: //CH Box
                        if (node is MyNodeMultiCH) count++;
                        break;
                    case NodeType.IN: //Indoor unit
                        if (node is MyNodeIn) count++;
                        break;
                }
            }

            if (isReverse)
            {
                if (node is MyNode)
                {
                    count += GetNodeCount((node as MyNode).ParentNode, nodeType, true, isReverse);
                }
                return count;
            }

            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                count += GetNodeCount(nodeOut.ChildNode, nodeType, true, isReverse);
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;
                for (int i = 0; i < nodeYP.MaxCount; ++i)
                {
                    count += GetNodeCount(nodeYP.ChildNodes[i], nodeType, true, isReverse);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                count += GetNodeCount(nodeCH.ChildNode, nodeType, true, isReverse);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                for (int i = 0; i < nodeMCH.ChildNodes.Count; ++i)
                {
                    count += GetNodeCount(nodeMCH.ChildNodes[i], nodeType, true, isReverse);
                }
            }
            return count;
        }

        /// <summary>
        /// 添加室内机
        /// </summary>
        /// <param name="parent"></param>
        private void AddIndoorNode(Node parent, PipingOrientation direction)
        {
            if (parent == null) return;
            if (parent.AddFlow == null) return;
            TreeNode indoorTreeNode = tvOutdoor.SelectedNode;
            if (indoorTreeNode == null || !(indoorTreeNode.Tag is RoomIndoor)) return;
            RoomIndoorState ris = roomIndoorStateList.Find(p => p.TreeNodeItem == indoorTreeNode);
            if (ris == null || ris.Used) return;
            RoomIndoor riItem = ris.RoomIndoorItem;
            if (riItem == null) return;

            MyNodeIn nodeIn = utilPiping.createNodeIn(riItem);
            nodeIn.XMoveable = true;
            nodeIn.YMoveable = true;
            nodeIn.XSizeable = false;
            nodeIn.YSizeable = false;

            DrawIndoorImage(nodeIn);

            if (!AppendToNodeTree(parent, nodeIn)) return;

            ris.Used = true;
            indoorTreeNode.BackColor = tvOutdoor.BackColor;
            indoorTreeNode.ForeColor = colorSelectedIndoor;
            SelectNextAvailableIndoor();
            Relayout(parent, nodeIn, direction);
            pipBll.drawTextToIDUNode(curSystemItem, nodeIn);
            
            UndoRedoUtil?.SaveProjectHistory();
        }

        /// <summary>
        /// 添加分歧管
        /// </summary>
        /// <param name="parent"></param>
        private void AddYPNode(Node parent, PipingOrientation direction)
        {
            MyNodeYP yp = utilPiping.createNodeYP(false);
            yp.XMoveable = true;
            yp.YMoveable = true;
            yp.XSizeable = false;
            yp.YSizeable = false;

            if (!AppendToNodeTree(parent, yp)) return;

            Relayout(parent, yp, direction);
            //drawLink(parent, yp, isVertical);

            UndoRedoUtil?.SaveProjectHistory();
        }

        /// <summary>
        /// 添加梳形管
        /// </summary>
        /// <param name="parent"></param>
        private void AddCPNode(Node parent, PipingOrientation direction)
        {
            MyNodeYP cp = utilPiping.createNodeYP(true);
            cp.XMoveable = true;
            cp.YMoveable = true;
            cp.XSizeable = false;
            cp.YSizeable = false;

            if (!AppendToNodeTree(parent, cp)) return;

            Relayout(parent, cp, direction);
            //drawLink(parent, cp, isVertical);

            UndoRedoUtil?.SaveProjectHistory();
        }

        /// <summary>
        /// 添加分歧管
        /// </summary>
        /// <param name="parent"></param>
        private void AddCHBoxNode(Node parent, PipingOrientation direction)
        {
            MyNodeCH nodeCH = utilPiping.createNodeCHbox(null);
            nodeCH.XMoveable = true;
            nodeCH.YMoveable = true;
            nodeCH.XSizeable = false;
            nodeCH.YSizeable = false;

            DrawCHBoxImage(nodeCH);

            if (!AppendToNodeTree(parent, nodeCH)) return;
            Relayout(parent, nodeCH, direction);
            //drawLink(parent, yp, isVertical);

            UndoRedoUtil?.SaveProjectHistory();
        }

        /// <summary>
        /// 添加分歧管
        /// </summary>
        /// <param name="parent"></param>
        private void AddMultiCHBoxNode(Node parent, PipingOrientation direction)
        {
            MyNodeMultiCH nodeMCH = utilPiping.createNodeMultiCHbox();
            nodeMCH.XMoveable = true;
            nodeMCH.YMoveable = true;
            nodeMCH.XSizeable = false;
            nodeMCH.YSizeable = false;

            DrawMultiCHBoxImage(nodeMCH);

            if (!AppendToNodeTree(parent, nodeMCH)) return;
            Relayout(parent, nodeMCH, direction);

            UndoRedoUtil?.SaveProjectHistory();
        }

        private void Relayout(Node parent, Node node, PipingOrientation direction)
        {
            addFlowPiping.Nodes.Add(node);

            Node[] childNodes = null;
            int leftCount = 0;
            int rightCount = 0;
            PipingOrientation parentDirection = PipingOrientation.Unknown;
            if (parent is MyNodeOut)
            {
                PointF pt = new PointF();
                pt.X = parent.Location.X + parent.Size.Width;
                pt.Y = parent.Location.Y + parent.Size.Height;
                node.Location = new PointF(pt.X - node.Size.Width / 2 + VDistance, pt.Y - node.Size.Height / 2 + VDistance);
            }
            else
            {
                PointF pt = utilPiping.getCenterPointF(parent);

                switch (direction)
                {
                    case PipingOrientation.Up:
                        pt = new PointF(pt.X - node.Size.Width / 2, pt.Y - node.Size.Height / 2 - VDistance);
                        break;
                    case PipingOrientation.Down:
                        pt = new PointF(pt.X - node.Size.Width / 2, pt.Y - node.Size.Height / 2 + VDistance);
                        break;
                    case PipingOrientation.Left:
                        pt = new PointF(pt.X - node.Size.Width / 2 - HDistance, pt.Y - node.Size.Height / 2);
                        break;
                    case PipingOrientation.Right:
                    default:
                        pt = new PointF(pt.X - node.Size.Width / 2 + HDistance, pt.Y - node.Size.Height / 2);
                        break;
                }

                if (parent is MyNodeMultiCH || parent is MyNodeYP)
                {
                    parentDirection = pipBll.CheckLinkOrientation(parent, parent.InLinks[0]);
                    if (parent is MyNodeMultiCH)
                    {
                        MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                        childNodes = parentMCH.ChildNodes.ToArray();
                    }
                    else if (parent is MyNodeYP)
                    {
                        MyNodeYP parentYP = parent as MyNodeYP;
                        if (parentYP.IsCP)
                        {
                            childNodes = parentYP.ChildNodes;
                        }
                    }
                }

                if (childNodes != null)
                {
                    int childCount = childNodes.Length;
                    for (int i = 0; i < childCount; i++)
                    {
                        MyNode child = childNodes[i] as MyNode;
                        if (child == null) continue;

                        if (child == node)
                        {
                            int sameSideCount = (direction == PipingOrientation.Left ? leftCount : rightCount);
                            if (parentDirection == PipingOrientation.Down)
                            {
                                pt.Y = parent.Location.Y + parent.Size.Height - (20 + VDistance * sameSideCount);
                            }
                            else
                            {
                                pt.Y = parent.Location.Y + 20 + VDistance * sameSideCount;
                            }
                            if (direction == PipingOrientation.Left)
                            {
                                leftCount++;
                            }
                            else
                            {
                                rightCount++;
                            }
                        }
                        else
                        {
                            if (child.Location.X < parent.Location.X)
                            {
                                //在父节点左侧
                                leftCount++;
                            }
                            else
                            {
                                //在父节点右侧
                                rightCount++;
                            }
                        }
                    }
                }

                //当节点的x位置超出左/上边界时，往右/下移动
                float dx, dy;
                dx = 50 - pt.X;
                dy = 50 - pt.Y;
                if (dx > 0 || dy > 0)
                {
                    pipBll.MoveAllItems(dx, dy);
                    if (dx > 0) pt.X += dx;
                    if (dy > 0) pt.Y += dy;
                }

                node.Location = pt;
            }
            //drawLink(parent, child, isVertical);

            if (node is MyNode)
            {
                MyNode myNode = node as MyNode;
                foreach (MyLink myLink in myNode.MyInLinks)
                {
                    myLink.Selectable = true;
                    myLink.Stretchable = false;
                    if (myLink.AddFlow == null)
                    {
                        addFlowPiping.AddLink(myLink, parent, node);    // 必须在重置 Link 顶点位置之前执行AddLink()方法
                    }
                    myLink.Line.Style = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
                    if (parent is MyNodeOut)
                    {
                        myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);
                        myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                        myLink.Line.Style = LineStyle.VH;
                    }
                    else
                    {
                        switch (direction)
                        {
                            case PipingOrientation.Up:
                                myLink.Points[0] = utilPiping.getTopCenterPointF(parent);
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getBottomCenterPointF(node);
                                myLink.Line.Style = LineStyle.VHV;
                                break;
                            case PipingOrientation.Down:
                                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
                                myLink.Line.Style = LineStyle.VHV;
                                break;
                            case PipingOrientation.Left:
                                myLink.Points[0] = utilPiping.getLeftCenterPointF(parent);
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getRightCenterPointF(node);
                                myLink.Line.Style = LineStyle.HVH;
                                break;
                            case PipingOrientation.Right:
                            default:
                                myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
                                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
                                myLink.Line.Style = LineStyle.HVH;
                                break;
                        }

                        if (parent is MyNodeYP && !(parent as MyNodeYP).IsCP)
                        {
                            myLink.Points[0] = utilPiping.getCenterPointF(parent);
                        }
                    }

                    if (node is MyNodeYP)
                    {
                        myLink.Points[myLink.Points.Count - 1] = utilPiping.getCenterPointF(node);
                    }
                }
            }

            if (childNodes != null)
            {
                int vectorHorizontal = 1; //x轴方向系数（左:-1, 右：1）
                int childCount = childNodes.Length;
                int leftIndex = 0;
                int rightIndex = 0;

                for (int i = 0; i < childCount; i++)
                {
                    MyNode child = childNodes[i] as MyNode;
                    if (child == null) continue;

                    PointF ptf = child.Location;
                    int sameSideCount = 0;
                    int sameSideIndex = 0;
                    if (ptf.X < parent.Location.X)
                    {
                        sameSideCount = leftCount;
                        sameSideIndex = leftIndex;
                        leftIndex++;
                        vectorHorizontal = -1;
                    }
                    else
                    {
                        sameSideCount = rightCount;
                        sameSideIndex = rightIndex;
                        rightIndex++;
                        vectorHorizontal = 1;
                    }
                    float x1 = 0, y1 = 0;
                    x1 = 3 * (sameSideCount - sameSideIndex) * vectorHorizontal;
                    if (parentDirection == PipingOrientation.Down)
                    {
                        y1 = parent.Location.Y + parent.Size.Height / (sameSideCount + 1) * (sameSideCount - sameSideIndex);
                    }
                    else
                    {
                        y1 = parent.Location.Y + parent.Size.Height / (sameSideCount + 1) * (sameSideIndex + 1);
                    }
                    foreach (MyLink lk in child.MyInLinks)
                    {
                        lk.Points[0] = new PointF(lk.Points[0].X, y1);
                        lk.Points[1] = new PointF(lk.Points[0].X + x1, y1);
                        lk.Points[2] = new PointF(lk.Points[1].X, utilPiping.getCenterPointF(child).Y);
                        lk.Points[3] = new PointF(lk.Points[3].X, lk.Points[2].Y);
                    }
                }
            }


            if (curSystemItem.IsInputLengthManually)
            {
                if (nodePlottingScale != null)
                {
                    pipBll.CalculatePipeLengthByPlottingScale(curSystemItem, nodePlottingScale.PlottingScale, node, parent);
                }
            }

            CheckLayoutComplete();
        }

        private void CheckLayoutComplete()
        {
            //IsLayoutComplete = true;
            //jcbtnOK.Enabled = false;
            pipBll.CheckManualPipingComplete(curSystemItem.MyPipingNodeOut);

            //foreach (var ris in roomIndoorStateList)
            //{
            //    if (!ris.Used)
            //    {
            //        IsLayoutComplete = false;
            //        break;
            //    }
            //}
            //jcbtnOK.Enabled = IsLayoutComplete;
        }

        private bool AppendToNodeTree(Node parent, Node node)
        {
            if (parent is MyNodeOut)
            {
                MyNodeOut nodeOut = parent as MyNodeOut;
                if (nodeOut.ChildNode != null) return false;
                nodeOut.ChildNode = node;
            }
            else if (parent is MyNodeYP)
            {
                MyNodeYP parentYP = parent as MyNodeYP;
                return parentYP.AddChildNodeManually(node);
            }
            else if (parent is MyNodeCH)
            {
                MyNodeCH parentCH = parent as MyNodeCH;
                if (parentCH.ChildNode != null) return false;
                parentCH.ChildNode = node;
            }
            else if (parent is MyNodeMultiCH)
            {
                MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                parentMCH.AddChildNode(node);
            }
            return true;
        }

        ///// <summary>
        ///// 重定位节点
        ///// </summary>
        ///// <param name="node"></param>
        ///// <param name="parent"></param>
        ///// <param name="maxY"></param>
        //private void LayoutNormal(Node node, Node parent, ref float maxY)
        //{
        //    if (node == null) return;

        //    if (node is MyNodeOut)
        //    {
        //        MyNodeOut nodeOut = node as MyNodeOut;

        //        maxY = node.Location.Y;
        //        LayoutNormal(nodeOut.ChildNode, nodeOut, ref maxY);
        //        return;
        //    }

        //    addFlowPiping.Nodes.Add(node);
        //    if (node is MyNodeYP)
        //    {
        //        MyNodeYP nodeYP = node as MyNodeYP;

        //        // 必须在节点加载后确定坐标位置，因为当子节点node（主要是InNode）的Size为0时，Piping中显示不正确
        //        if (nodeYP.IsFirstYP)
        //        {
        //            nodeYP.Location = utilPiping.getLocationFirstYP(parent);
        //        }
        //        else
        //        {
        //            nodeYP.Location = utilPiping.getLocationChild(parent, nodeYP, isVertical, maxY);
        //        }
        //        maxY = Math.Max(maxY, utilPiping.getCenterPointF(node).Y);

        //        if (isVertical)
        //        {
        //            for (int i = 0; i < nodeYP.MaxCount; ++i)
        //            {
        //                LayoutNormal(nodeYP.ChildNodes[i], nodeYP, ref maxY);
        //            }
        //        }
        //        else
        //        {
        //            for (int i = nodeYP.MaxCount - 1; i >= 0; --i)
        //            {
        //                LayoutNormal(nodeYP.ChildNodes[i], nodeYP, ref maxY);
        //            }
        //        }
        //    }
        //    else if (node is MyNodeCH)
        //    {
        //        MyNodeCH nodeCH = node as MyNodeCH;
        //        if (parent is MyNodeOut)
        //        {
        //            node.Location = utilPiping.getLocationFirstCH(parent, node);
        //        }
        //        else
        //        {
        //            node.Location = utilPiping.getLocationChild(parent, node, isVertical, maxY);
        //        }
        //        maxY = Math.Max(maxY, utilPiping.getCenterPointF(node).Y);

        //        LayoutNormal(nodeCH.ChildNode, nodeCH, ref maxY);
        //    }
        //    else if (node is MyNodeIn)
        //    {
        //        MyNodeIn nodeIn = node as MyNodeIn;
        //        if (parent is MyNodeOut)
        //        {
        //            PointF ptf = utilPiping.getLocationFirstYP(parent);
        //            nodeIn.Location = new PointF(ptf.X + 50, ptf.Y + 30);
        //        }
        //        else
        //        {
        //            nodeIn.Location = utilPiping.getLocationChild(parent, node, isVertical, maxY);
        //        }
        //        maxY = Math.Max(maxY, utilPiping.getCenterPointF(node).Y);

        //        pipBll.drawTextToUnitNode(curSystemItem, nodeIn);
        //    }
        //    drawLink(parent, node, isVertical);
        //}

        ///// <summary>
        ///// 计算所有节点（除室外机）的位置 (二叉树布局)
        ///// </summary>
        //private void LayoutBinaryTree(Node node, Node parent, ref float maxX, ref float maxY)
        //{
        //    if (node == null) return;

        //    if (node is MyNodeOut)
        //    {
        //        MyNodeOut nodeOut = node as MyNodeOut;
        //        maxX = 0;
        //        maxY = node.Location.Y;
        //        LayoutBinaryTree(nodeOut.ChildNode, nodeOut, ref maxX, ref maxY);
        //        DrawLinkForBinaryTree(nodeOut, nodeOut.ChildNode, 0, 1);
        //        return;
        //    }

        //    addFlowPiping.Nodes.Add(node);

        //    if (isVertical)
        //    {
        //        //纵向布局
        //        node.Location = utilPiping.GetBinaryTreeNodeLocationVertical(parent, node, maxY);
        //        PointF ptNode = utilPiping.getCenterPointF(node);
        //        maxY = Math.Max(maxY, ptNode.Y);
        //    }
        //    else
        //    {
        //        //横向布局
        //        node.Location = utilPiping.GetBinaryTreeNodeLocationHorizontal(parent, node, maxX);
        //        PointF ptNode = utilPiping.getCenterPointF(node);
        //        maxX = Math.Max(maxX, ptNode.X);
        //    }

        //    if (node is MyNodeYP)
        //    {
        //        MyNodeYP nodeYP = node as MyNodeYP;

        //        for (int i = 0; i < nodeYP.MaxCount; ++i)
        //        {
        //            LayoutBinaryTree(nodeYP.ChildNodes[i], nodeYP, ref maxX, ref maxY);
        //        }

        //        if (!nodeYP.IsCP)
        //        {
        //            if (nodeYP.ChildCount > 1)
        //            {
        //                //子节点排列好以后，再计算YP节点的坐标
        //                PointF ptFirstChild = utilPiping.getCenterPointF(nodeYP.ChildNodes[0]);
        //                PointF ptLastChild = utilPiping.getCenterPointF(nodeYP.ChildNodes[nodeYP.ChildCount - 1]);

        //                if (isVertical)
        //                {
        //                    nodeYP.Location = new PointF(nodeYP.Location.X, (ptFirstChild.Y + ptLastChild.Y - nodeYP.Size.Height) / 2);
        //                }
        //                else
        //                {
        //                    nodeYP.Location = new PointF((ptFirstChild.X + ptLastChild.X - nodeYP.Size.Width) / 2, nodeYP.Location.Y);
        //                }
        //            }
        //            else if (nodeYP.ChildCount == 1)
        //            {
        //                if (nodeYP.ChildNodes[0] != null)
        //                {
        //                    PointF ptFirstChild = utilPiping.getCenterPointF(nodeYP.ChildNodes[0]);
        //                    if (isVertical)
        //                    {
        //                        nodeYP.Location = new PointF(nodeYP.Location.X, ptFirstChild.Y + 50);
        //                    }
        //                    else
        //                    {
        //                        nodeYP.Location = new PointF(ptFirstChild.X + 50, nodeYP.Location.Y);
        //                    }
        //                }
        //                else if (nodeYP.ChildNodes[1] != null)
        //                {
        //                    PointF ptLastChild = utilPiping.getCenterPointF(nodeYP.ChildNodes[1]);
        //                    if (isVertical)
        //                    {
        //                        nodeYP.Location = new PointF(nodeYP.Location.X, ptLastChild.Y - UtilPiping.VDistanceVertical / 2);
        //                    }
        //                    else
        //                    {
        //                        nodeYP.Location = new PointF(ptLastChild.X - UtilPiping.HDistanceHorizontal / 2, nodeYP.Location.Y);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (isVertical)
        //                {
        //                    nodeYP.Location = new PointF(nodeYP.Location.X, maxY - node.Size.Height / 2 + UtilPiping.VDistanceVertical);
        //                }
        //                else
        //                {
        //                    nodeYP.Location = new PointF(maxX - node.Size.Width / 2 + UtilPiping.HDistanceHorizontal, nodeYP.Location.Y);
        //                }
        //            }

        //            if (isVertical)
        //            {
        //                maxY = Math.Max(maxY, nodeYP.Location.Y);
        //            }
        //            else
        //            {
        //                maxX = Math.Max(maxX, nodeYP.Location.X);
        //            }
        //        }

        //        for (int i = 0; i < nodeYP.MaxCount; ++i)
        //        {
        //            if (nodeYP.ChildNodes[i] != null)
        //            {
        //                DrawLinkForBinaryTree(nodeYP, nodeYP.ChildNodes[i], i, nodeYP.ChildCount);
        //            }
        //        }
        //    }
        //    else if (node is MyNodeCH)
        //    {
        //        MyNodeCH nodeCH = node as MyNodeCH;
        //        if (nodeCH.ChildNode != null)
        //        {
        //            LayoutBinaryTree(nodeCH.ChildNode, nodeCH, ref maxX, ref maxY);

        //            //子节点排列好以后，再计算CH Box节点的坐标 
        //            PointF ptChild = utilPiping.getCenterPointF(nodeCH.ChildNode);
        //            if (isVertical)
        //            {
        //                nodeCH.Location = new PointF(nodeCH.Location.X, ptChild.Y - nodeCH.Size.Height / 2);
        //            }
        //            else
        //            {
        //                nodeCH.Location = new PointF(ptChild.X - nodeCH.Size.Width / 2, nodeCH.Location.Y);
        //            }
        //            DrawLinkForBinaryTree(nodeCH, nodeCH.ChildNode, 0, 1);
        //        }
        //        else
        //        {
        //            if (isVertical)
        //            {
        //                nodeCH.Location = new PointF(nodeCH.Location.X, maxY - nodeCH.Size.Height / 2 + UtilPiping.VDistanceVertical);
        //                maxY = Math.Max(maxY, nodeCH.Location.Y);
        //            }
        //            else
        //            {
        //                nodeCH.Location = new PointF(maxX - nodeCH.Size.Width / 2 + UtilPiping.HDistanceHorizontal, nodeCH.Location.Y);
        //                maxX = Math.Max(maxX, nodeCH.Location.X);
        //            }
        //        }
        //    }
        //    else if (node is MyNodeIn)
        //    {
        //        MyNodeIn nodeIn = node as MyNodeIn;
        //        pipBll.drawTextToUnitNode(curSystemItem, nodeIn);
        //    }
        //}

        //// 绘制节点之间的连接线
        //public void drawLink(Node parent, Node node, bool isVertical)
        //{
        //    if (parent == null) return;

        //    bool isFirst = true;
        //    if (parent is MyNodeYP)
        //    {
        //        isFirst = ((parent as MyNodeYP).ChildNodes[0] == node);
        //    }

        //    LineStyle ls = LineStyle.VH;
        //    if (!isVertical && !isFirst) // 第二个节点
        //    {
        //        ls = LineStyle.HVH;
        //    }
        //    MyLink myLink = null;
        //    if (node is IInLink)
        //    {
        //        myLink = (node as IInLink).InLink;
        //    }
        //    if (myLink == null) return;
        //    myLink.Selectable = true;
        //    myLink.Stretchable = true;
        //    myLink.Line.Style = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
        //    if (myLink.AddFlow == null)
        //    {
        //        addFlowPiping.AddLink(myLink, parent, node);    // 必须在重置 Link 顶点位置之前执行AddLink()方法
        //    }

        //    if (node is MyNodeCH)
        //    {
        //        if (!(parent is MyNodeOut))
        //        {
        //            if (!isVertical)
        //            {
        //                myLink.Points[0] = utilPiping.getCenterPointF(parent);
        //                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
        //                myLink.Line.Style = LineStyle.HV;
        //            }
        //            else
        //            {
        //                myLink.Points[0] = utilPiping.getCenterPointF(parent);
        //                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
        //                myLink.Line.Style = LineStyle.VH;
        //            }
        //        }
        //        else
        //        {
        //            //第一子节点为CH-Box时，连线需要特殊处理
        //            myLink.Points[0] = utilPiping.setLinkStartPoint(myLink, isFirst, isVertical);
        //            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
        //            myLink.Line.Style = LineStyle.VHV;
        //        }
        //    }
        //    else
        //    {
        //        if (parent is MyNodeCH && node is MyNodeIn)
        //        {
        //            if (!isVertical)
        //            {
        //                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
        //            }
        //            else
        //            {
        //                myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
        //            }
        //            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
        //            myLink.Line.Style = LineStyle.VH;   // 该行代码必须在重置连接线顶点的坐标之后执行
        //        }
        //        else
        //        {
        //            myLink.Points[0] = utilPiping.setLinkStartPoint(myLink, isFirst, isVertical);
        //            myLink.Points[myLink.Points.Count - 1] = utilPiping.setLinkEndPoint(myLink, isVertical);
        //            myLink.Line.Style = ls;                         // 该行代码必须在重置连接线顶点的坐标之后执行

        //            if (ls == LineStyle.HVH)    // 横向排列时的第二个室内机节点的连线，折点位置调整
        //            {
        //                int index = 0;
        //                if (parent is MyNodeYP)
        //                {
        //                    MyNodeYP parentYP = parent as MyNodeYP;
        //                    index = parentYP.GetIndex(node);
        //                    float x = myLink.Points[0].X + UtilPiping.HDistanceHorizontal * index;
        //                    myLink.Points[1] = new PointF(x, myLink.Points[1].Y);
        //                    myLink.Points[2] = new PointF(x, myLink.Points[2].Y);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 绘制节点之间的连接线
        ///// </summary>
        //private void DrawLinkForBinaryTree(Node parent, Node node, int nodeIndex, int nodeCount)
        //{
        //    MyLink myLink = null;

        //    if (node is MyNodeIn || node is MyNodeYP)
        //    {
        //        if (node is MyNodeIn)
        //        {
        //            myLink = (node as MyNodeIn).InLink;
        //        }
        //        else
        //        {
        //            myLink = (node as MyNodeYP).InLink;
        //        }

        //        myLink.Line.Style = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
        //        if (myLink.AddFlow == null)
        //        {
        //            addFlowPiping.AddLink(myLink, parent, node);    // 必须在重置 Link 顶点位置之前执行AddLink()方法
        //        }
        //        if (node is MyNodeIn)
        //        {
        //            if (isVertical)
        //            {
        //                myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
        //            }
        //            else
        //            {
        //                myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
        //            }
        //        }
        //        else
        //        {
        //            myLink.Points[myLink.Points.Count - 1] = utilPiping.getCenterPointF(node);
        //        }

        //        if (parent is MyNodeOut)
        //        {
        //            myLink.Points[0] = utilPiping.getLeftBottomPointF(parent);

        //            if (isVertical)
        //            {
        //                //垂直布局
        //                myLink.Line.Style = LineStyle.VH;
        //            }
        //            else
        //            {
        //                //水平布局
        //                myLink.Line.Style = LineStyle.VHV;
        //                myLink.Points[1] = new PointF(myLink.Points[0].X, myLink.Points[0].Y + 10);
        //                myLink.Points[2] = new PointF(myLink.Points[3].X, myLink.Points[0].Y + 10);
        //            }
        //        }
        //        else if (parent is MyNodeCH)
        //        {
        //            if (isVertical)
        //            {
        //                myLink.Points[0] = utilPiping.getRightCenterPointF(parent);
        //            }
        //            else
        //            {
        //                myLink.Points[0] = utilPiping.getBottomCenterPointF(parent);
        //            }
        //            //myLink.Line.Style = LineStyle.VH; //从CH出来的是直线
        //        }
        //        else if (parent is MyNodeYP)
        //        {
        //            myLink.Points[0] = utilPiping.getCenterPointF(parent);
        //            // 竖直布局处理
        //            if (isVertical)
        //            {
        //                // 分歧管的第一个连接线的起点，为分歧管中点
        //                if (nodeIndex == 0)
        //                {
        //                    if ((parent as MyNodeYP).IsCP)
        //                    {
        //                        PointF ptf = utilPiping.getTopCenterPointF(parent);
        //                        ptf.Y = ptf.Y + 10;
        //                        myLink.Points[0] = ptf;
        //                    }
        //                }
        //                myLink.Line.Style = LineStyle.VH; // 该行代码必须在重置连接线顶点的坐标之后执行
        //            }
        //            else
        //            {
        //                myLink.Line.Style = LineStyle.HV; // 该行代码必须在重置连接线顶点的坐标之后执行
        //            }
        //        }
        //    }
        //    else if (node is MyNodeCH)
        //    {
        //        MyNodeCH nodeCH = node as MyNodeCH;
        //        myLink = nodeCH.InLink;

        //        myLink.Line.Style = LineStyle.Polyline;         // 该行代码对重置link顶点位置很重要！
        //        if (myLink.AddFlow == null)
        //        {
        //            addFlowPiping.AddLink(myLink, parent, node);    // 必须在重置 Link 顶点位置之前执行AddLink()方法
        //        }

        //        if (isVertical)
        //        {
        //            myLink.Points[myLink.Points.Count - 1] = utilPiping.getLeftCenterPointF(node);
        //        }
        //        else
        //        {
        //            myLink.Points[myLink.Points.Count - 1] = utilPiping.getTopCenterPointF(node);
        //        }

        //        if (!(parent is MyNodeOut))
        //        {
        //            if (isVertical)
        //            {
        //                myLink.Points[0] = utilPiping.getCenterPointF(parent);
        //                myLink.Line.Style = LineStyle.VH;
        //            }
        //            else
        //            {
        //                myLink.Points[0] = utilPiping.getCenterPointF(parent);
        //                myLink.Line.Style = LineStyle.HV;
        //            }
        //        }
        //        else
        //        {
        //            //第一子节点为CH-Box时，连线需要特殊处理
        //            myLink.Points[0] = utilPiping.getCenterPointF(parent);
        //            myLink.Line.Style = LineStyle.VHV;
        //        }
        //    }
        //}

        //// 找到指定 YP 节点（非CP）的所有子节点中，第一个节点为YP节点的个数
        ///// <summary>
        ///// 找到指定 YP 节点（非CP）的所有子节点中，是第一个节点并且为YP节点的个数
        ///// </summary>
        ///// <param name="ypNode"></param>
        //private int FindFirstYPCount(Node node)
        //{
        //    if (node == null) return 0;

        //    int num = 0;
        //    if (node is MyNodeYP)
        //    {
        //        MyNodeYP nodeYP = node as MyNodeYP;

        //        if (!nodeYP.IsCP)
        //        {
        //            if (nodeYP.ChildNodes[0] is MyNodeYP)
        //            {
        //                num = 1;
        //            }
        //            foreach (Node child in nodeYP.ChildNodes)
        //            {
        //                num += FindFirstYPCount(child);
        //            }
        //        }
        //    }
        //    else if (node is MyNodeCH)
        //    {
        //        MyNodeCH nodeCH = node as MyNodeCH;
        //        if (nodeCH.ChildNode is MyNodeYP)
        //        {
        //            num = 1;
        //        }
        //        num += FindFirstYPCount(nodeCH.ChildNode);
        //    }
        //    return num;
        //}

        private void DrawIndoorImage(MyNodeIn nodeIn)
        {
            NodeElement_Piping indNodeItem = pipBll.GetPipingNodeIndElement(nodeIn.RoomIndooItem.IndoorItem.Type, isHitachi);
            string imgFile = imageDirectory + indNodeItem.Name + ".png";

            pipBll.drawNodeImage(nodeIn, imgFile, ref addFlowPiping);
        }

        private void DrawCHBoxImage(MyNodeCH nodeCH)
        {
            string imgFile = imageDirectory + "CHbox.png";
            pipBll.drawNodeImage(nodeCH, imgFile, ref addFlowPiping);
        }

        private void DrawMultiCHBoxImage(MyNodeMultiCH nodeMCH)
        {
            string imgFile = imageDirectory + "MultiCHbox.png";
            pipBll.drawNodeImage(nodeMCH, imgFile, ref addFlowPiping);
        }

        private void DeleteNode(Node node)
        {
            if (node == null) return;
            selNode = null;

            if (node == nodePlottingScale)
            {
                EnablePlottingScale(null, false);
            }
            else if (node == nodeBuildingImage)
            {
                DeleteBuildingImageNode();
                return;
            }
            
            if (node is MyNodeOut)
            {
                MyNodeOut nodeOut = node as MyNodeOut;
                DeleteNode(nodeOut.ChildNode);
                return;
            }

            if (node.Children != null)
            {
                node.Children.Clear();
            }

            if (node is MyNodeIn)
            {
                //将删除的室内机重新在左边显示为可用
                MyNodeIn nodeIn = node as MyNodeIn;
                foreach (var ris in roomIndoorStateList)
                {
                    if (ris.RoomIndoorItem == nodeIn.RoomIndooItem)
                    {
                        ris.TreeNodeItem.ForeColor = tvOutdoor.ForeColor;
                        ris.Used = false;
                    }
                }
            }
            else if (node is MyNodeYP)
            {
                MyNodeYP nodeYP = node as MyNodeYP;

                //if (nodeYP.IsCP)
                //{
                //    while (nodeYP.ChildCount > 0)
                //    {
                //        DeleteNode(nodeYP.ChildNodes[0]);
                //    }
                //}
                //else
                //{
                //    foreach (Node child in nodeYP.ChildNodes)
                //    {
                //        DeleteNode(child);
                //    }
                //}
                for (int i = 0; i < nodeYP.MaxCount; i++)
                {
                    DeleteNode(nodeYP.ChildNodes[i]);
                }
            }
            else if (node is MyNodeCH)
            {
                MyNodeCH nodeCH = node as MyNodeCH;
                DeleteNode(nodeCH.ChildNode);
            }
            else if (node is MyNodeMultiCH)
            {
                MyNodeMultiCH nodeMCH = node as MyNodeMultiCH;
                while (nodeMCH.ChildNodes.Count > 0)
                {
                    DeleteNode(nodeMCH.ChildNodes[0]);
                }
            }

            if (node.InLinks != null && node.InLinks.Count > 0)
            {
                Node parent = node.InLinks[0].Org;
                if (parent is MyNodeOut)
                {
                    MyNodeOut nodeOut = parent as MyNodeOut;
                    nodeOut.ChildNode = null;
                }
                else if (parent is MyNodeYP)
                {
                    MyNodeYP parentYP = parent as MyNodeYP;
                    //if (parentYP.IsCP)
                    //{
                    //    parentYP.RemoveChildNode(node);
                    //}
                    //else
                    //{
                    //    parentYP.RemoveChildNodeManually(node);
                    //}
                    parentYP.RemoveChildNodeManually(node);
                }
                else if (parent is MyNodeCH)
                {
                    MyNodeCH parentCH = parent as MyNodeCH;
                    parentCH.ChildNode = null;
                }
                else if (parent is MyNodeMultiCH)
                {
                    MyNodeMultiCH parentMCH = parent as MyNodeMultiCH;
                    parentMCH.ChildNodes.Remove(node);
                }
            }
            addFlowPiping.Nodes.Remove(node);
        }

        private void frmManualPiping_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil?.Dispose();
        }

        private void SavePiping()
        {
            curSystemItem.IsManualPiping = true;
            curSystemItem.IsPipingOK = false;
            pipBll.SavePipingStructure(curSystemItem);
            pipBll.SavePipingBuildingImageNode(curSystemItem, nodeBuildingImage, originalBuildingImage);
            pipBll.SavePipingPlottingScaleNode(curSystemItem, nodePlottingScale);
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            //if (this.JCValidateForm())
            //{
            SavePiping();

            addFlowPiping.Items.Clear();

            this.DialogResult = DialogResult.OK;

            this.Close();
            //}
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void jcbtnClear_Click(object sender, EventArgs e)
        {
            DeleteNode(curSystemItem.MyPipingNodeOut);
            //Relayout();
            CheckLayoutComplete();
            SelectFirstAvailableIndoor();

            UndoRedoUtil?.SaveProjectHistory();
        }

        private void tvOutdoor_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            RoomIndoorState ris = roomIndoorStateList.Find(p => p.TreeNodeItem == node);
            if (ris == null || ris.Used)
            {
                tvOutdoor.SelectedNode = null;
                return;
            }

            node.BackColor = UtilColor.bg_selected;
            node.ForeColor = UtilColor.font_selected;
        }

        private void tvOutdoor_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode selNode = tvOutdoor.SelectedNode;
            if (selNode == null) return;

            RoomIndoorState ris = roomIndoorStateList.Find(p => p.TreeNodeItem == selNode);
            selNode.BackColor = tvOutdoor.BackColor;
            selNode.ForeColor = (ris != null && ris.Used) ? colorSelectedIndoor : tvOutdoor.ForeColor;
        }
        
        private void DeleteBuildingImageNode()
        {
            if (nodeBuildingImage != null)
            {
                nodeBuildingImage.Remove();
                addFlowPiping.Images.RemoveAt(nodeBuildingImage.ImageIndex);
                nodeBuildingImage = null;
                LockBuildingImageNode(false);
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void LockBuildingImageNode(bool locked)
        {
            if (nodeBuildingImage != null)
            {
                nodeBuildingImage.Selectable = !locked;
                nodeBuildingImage.XMoveable = !locked;
                nodeBuildingImage.YMoveable = !locked;
                nodeBuildingImage.XSizeable = !locked;
                nodeBuildingImage.YSizeable = !locked;

                chkBuildingImageLocked.Enabled = true;
                chkBuildingImageLocked.Checked = locked;
                //chkBuildingImageLocked.Visible = locked;
                pbDeleteBgImage.Enabled = !locked;
                pbDeleteBgImage.Visible = true;
                pbAddBuildingImage.Visible = false;

                if (!locked)
                {
                    addFlowPiping.SelectedItems.Clear();
                    addFlowPiping.SelectedItem = null;
                }
                nodeBuildingImage.Selected = !locked;
            }
            else
            {
                chkBuildingImageLocked.Checked = false;
                chkBuildingImageLocked.Enabled = false;
                //chkBuildingImageLocked.Visible = false;
                pbDeleteBgImage.Visible = false;
                pbAddBuildingImage.Visible = true;
            }
        }

        private void pbAddBuildingImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a building image file";
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Image|*.jpg;*.jpeg;*.bmp;*.png;";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(fileDialog.FileName);
                    Size bmpSize = img.Size;
                    if (bmpSize.Width > 3000)
                    {
                        bmpSize.Height = (int)Math.Round(3000d / bmpSize.Width * bmpSize.Height);
                        bmpSize.Width = 3000;
                    }
                    if (bmpSize.Height > 3000)
                    {
                        bmpSize.Width = (int)Math.Round(3000d / bmpSize.Height * bmpSize.Width);
                        bmpSize.Height = 3000;
                    }
                    System.Drawing.Image bmp = new System.Drawing.Bitmap(img, bmpSize);
                    img.Dispose(); //关闭文件，防止锁死文件写入

                    DeleteBuildingImageNode();
                    originalBuildingImage = bmp;
                    Size size2 = addFlowPiping.Size;
                    double scale = Math.Min(size2.Width * 1d / bmpSize.Width, size2.Height * 1d / bmpSize.Height);
                    if (scale < 1)
                    {
                        bmpSize = new Size((int)Math.Round(bmpSize.Width * scale), (int)Math.Round(bmpSize.Height * scale));
                    }
                    nodeBuildingImage = pipBll.DrawBuildingImageNode(bmp, bmpSize);
                    LockBuildingImageNode(false);
                    UndoRedoUtil?.SaveProjectHistory();
                }
                catch
                {
                }
            }
        }

        private void pbDeleteBgImage_Click(object sender, EventArgs e)
        {
            DeleteBuildingImageNode();
            originalBuildingImage = null;
        }

        private void rdoLineStylePolyLine_CheckedChanged(object sender, EventArgs e)
        {
            SetLineStyle(LineStyle.Polyline);
        }

        private void rdoLineStyleHV_CheckedChanged(object sender, EventArgs e)
        {
            SetLineStyle(LineStyle.HV);
        }

        private void rdoLineStyleVH_CheckedChanged(object sender, EventArgs e)
        {
            SetLineStyle(LineStyle.VH);
        }

        private void rdoLineStyleHVH_CheckedChanged(object sender, EventArgs e)
        {
            SetLineStyle(LineStyle.HVH);
        }

        private void rdoLineStyleVHV_CheckedChanged(object sender, EventArgs e)
        {
            SetLineStyle(LineStyle.VHV);
        }

        private void SetLineStyle(LineStyle ls)
        {
            if (addFlowPiping.SelectedItem is Link)
            {
                Link link = addFlowPiping.SelectedItem as Link;
                link.Line.Style = ls;
                ResetLinkPointLocation(link, true);
                ResetLinkPointLocation(link, false);

                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void pbLineColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pbLineColor.BackColor = colorDialog1.Color;
                utilPiping.colorDefault = colorDialog1.Color;
                foreach (Item item in addFlowPiping.Items)
                {
                    if (item is Link)
                    {
                        Link link = item as Link;
                        link.DrawColor = colorDialog1.Color;
                    }
                }
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void pbTextColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pbTextColor.BackColor = colorDialog1.Color;
                utilPiping.colorText = colorDialog1.Color;
                foreach (Item item in addFlowPiping.Items)
                {
                    if (item is Node)
                    {
                        Node node = item as Node;
                        node.TextColor = colorDialog1.Color;
                    }
                }
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void pbBranchKitColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pbBranchKitColor.BackColor = colorDialog1.Color;
                utilPiping.colorYP = colorDialog1.Color;
                foreach (Item item in addFlowPiping.Items)
                {
                    if (item is MyNodeYP)
                    {
                        Node node = item as Node;
                        node.FillColor = colorDialog1.Color;
                    }
                }
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void pbNodeBgColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pbNodeBgColor.BackColor = colorDialog1.Color;
                utilPiping.colorNodeBg = colorDialog1.Color;
                foreach (Item item in addFlowPiping.Items)
                {
                    if (item is MyNodeOut || item is MyNodeIn)
                    {
                        Node node = item as Node;
                        node.FillColor = colorDialog1.Color;
                    }
                }
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void lblTextColor_Click(object sender, EventArgs e)
        {
            pbTextColor_Click(sender, e);
        }

        private void lblLineColor_Click(object sender, EventArgs e)
        {
            pbLineColor_Click(sender, e);
        }

        private void lblBranchKitColor_Click(object sender, EventArgs e)
        {
            pbBranchKitColor_Click(sender, e);
        }

        private void lblNodeBgColor_Click(object sender, EventArgs e)
        {
            pbNodeBgColor_Click(sender, e);
        }

        private void addFlowPiping_SelectionChange(object sender, SelectionChangeArgs e)
        {
            ResetLineStyleRadioButtons();

            if (addFlowPiping.SelectedItem != null 
                && nodeBuildingImage != null
                && !chkBuildingImageLocked.Checked
                && addFlowPiping.SelectedItem != nodeBuildingImage)
            {
                //选中了其它节点，则锁定户型图
                LockBuildingImageNode(true);
            }
        }

        private void ResetLineStyleRadioButtons()
        {
            rdoLineStylePolyLine.Checked = false;
            rdoLineStyleHV.Checked = false;
            rdoLineStyleVH.Checked = false;
            rdoLineStyleHVH.Checked = false;
            rdoLineStyleVHV.Checked = false;
            if (addFlowPiping.SelectedItem is Link)
            {
                Link link = addFlowPiping.SelectedItem as Link;

                rdoLineStylePolyLine.Enabled = true;
                rdoLineStyleHV.Enabled = true;
                rdoLineStyleVH.Enabled = true;
                rdoLineStyleHVH.Enabled = true;
                rdoLineStyleVHV.Enabled = true;

                switch (link.Line.Style)
                {
                    case LineStyle.VH:
                        rdoLineStyleVH.Checked = true;
                        break;
                    case LineStyle.HV:
                        rdoLineStyleHV.Checked = true;
                        break;
                    case LineStyle.HVH:
                        rdoLineStyleHVH.Checked = true;
                        break;
                    case LineStyle.VHV:
                        rdoLineStyleVHV.Checked = true;
                        break;
                    default:
                        rdoLineStylePolyLine.Checked = true;
                        break;
                }
            }
            else
            {
                rdoLineStylePolyLine.Enabled = false;
                rdoLineStyleHV.Enabled = false;
                rdoLineStyleVH.Enabled = false;
                rdoLineStyleHVH.Enabled = false;
                rdoLineStyleVHV.Enabled = false;
            }
        }

        private void addFlowPiping_AfterResize(object sender, EventArgs e)
        {
            Item item = addFlowPiping.SelectedItem;
            if (nodeBuildingImage != null && item == nodeBuildingImage)
            {
                SizeF size1 = nodeBuildingImage.Size;
                SizeF size2 = addFlowPiping.Images[nodeBuildingImage.ImageIndex].Image.Size;
                SizeF size3 = originalBuildingImage.Size;
                //等比例缩放算法，先比较变化的幅度
                //图片最大尺寸限制在3000*3000,防止异常报出
                if (Math.Abs(size1.Width - size2.Width) >= Math.Abs(size1.Height - size2.Height))
                {
                    size1.Width = Math.Min(3000, size1.Width);
                    size1.Height = size1.Width / size3.Width * size3.Height;
                }
                else
                {
                    size1.Height = Math.Min(3000, size1.Height);
                    size1.Width = size1.Height / size3.Height * size3.Width;
                }
                nodeBuildingImage.Size = size1;

                int width = (int)Math.Round(size1.Width);
                int height = (int)Math.Round(size1.Height);
                System.Drawing.Image img = new Bitmap(originalBuildingImage, width, height);
                addFlowPiping.Images[nodeBuildingImage.ImageIndex].Image = img;
                UndoRedoUtil?.SaveProjectHistory();
            }
            else
            {
                if (nodePlottingScale != null)
                {
                    if (nodePlottingScale == item)
                    {
                        pipBll.DrawPlottingScaleNode(nodePlottingScale);
                        pipBll.CalculateAllPipeLengthByPlottingScale(curSystemItem, nodePlottingScale.PlottingScale);
                        UndoRedoUtil?.SaveProjectHistory();
                    }
                    else if (item is MyNodeIn || item is MyNodeYP || item is MyNodeOut || item is MyNodeCH)
                    {
                        Node node = item as Node;
                        foreach (Link lk in node.Links)
                        {
                            pipBll.CalculatePipeLengthByPlottingScale(curSystemItem, nodePlottingScale.PlottingScale, lk.Dst, lk.Org);
                        }
                        UndoRedoUtil?.SaveProjectHistory();
                    }
                }
            }
        }

        private void addFlowPiping_AfterMove(object sender, EventArgs e)
        {
            Item item = addFlowPiping.SelectedItem;
            if (item is MyNode)
            {
                Node node = item as Node;
                foreach (Link lk in node.Links)
                {
                    ResetLinkPointLocation(lk, true);
                    ResetLinkPointLocation(lk, false);
                    if (nodePlottingScale != null)
                    {
                        pipBll.CalculatePipeLengthByPlottingScale(curSystemItem, nodePlottingScale.PlottingScale, lk.Dst, lk.Org);
                    }
                }
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        // 改变配管图缩放比例
        private void jccmbScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            string scale = jccmbScale.Text.Trim();
            float f;
            if (!string.IsNullOrEmpty(scale) && float.TryParse(scale, out f))
            {
                f /= 100;
                addFlowPiping.Zoom = new Zoom(f, f);
            }
            else
            {
                scale = "100";
                this.jccmbScale.Text = scale;
            }
        }

        private void jctxtPlottingScale_TextChanged(object sender, EventArgs e)
        {
            double len = 0;
            if (string.IsNullOrEmpty(this.jctxtPlottingScale.Text))
            {
                nodePlottingScale.Text = "";
                nodePlottingScale.ActualLength = 0;
            }
            else
            {
                double.TryParse(this.jctxtPlottingScale.Text, out len);
                //nodePlottingScale.Text = len.ToString("0.##") + ut_length;
                nodePlottingScale.ActualLengthString = len.ToString("0.##") + " " + ut_length;
                nodePlottingScale.ActualLength = Unit.ConvertToSource(len, UnitType.LENGTH_M, ut_length);
                pipBll.DrawPlottingScaleNode(nodePlottingScale);
            }
            if (nodePlottingScale != null)
            {
                pipBll.CalculateAllPipeLengthByPlottingScale(curSystemItem, nodePlottingScale.PlottingScale);
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void pbPlottingScaleColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pbPlottingScaleColor.BackColor = colorDialog1.Color;
                nodePlottingScale.DrawColor = colorDialog1.Color;
                nodePlottingScale.FillColor = colorDialog1.Color;
                nodePlottingScale.TextColor = colorDialog1.Color;
                UndoRedoUtil?.SaveProjectHistory();
            }
        }

        private void jcbtnEnablePlottingScale_Click(object sender, EventArgs e)
        {
            EnablePlottingScale(null, true);
            UndoRedoUtil?.SaveProjectHistory();
        }

        private void chkBuildingImageLocked_CheckedChanged(object sender, EventArgs e)
        {
            LockBuildingImageNode(chkBuildingImageLocked.Checked); 
        }

        private void pbPlottingScaleRotation_Click(object sender, EventArgs e)
        {
            if (nodePlottingScale == null) return;
            nodePlottingScale.IsVertical = !nodePlottingScale.IsVertical;
            nodePlottingScale.Size = new SizeF(nodePlottingScale.Size.Height, nodePlottingScale.Size.Width);
            pipBll.DrawPlottingScaleNode(nodePlottingScale);
            SetPlottingScaleRotationIcon();
        }

        private void SetPlottingScaleRotationIcon()
        {
            if (nodePlottingScale != null && nodePlottingScale.IsVertical)
            {
                pbPlottingScaleRotation.Image = Properties.Resources.PlottingScale_V;
            }
            else
            {
                pbPlottingScaleRotation.Image = Properties.Resources.PlottingScale;
            }
        }
    }
}
