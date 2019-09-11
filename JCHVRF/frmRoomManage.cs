using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Util;
using JCHVRF.VRFMessage;
using JCBase.Utility;
namespace JCHVRF
{
    public partial class frmRoomManage : JCBase.UI.JCForm
    {
        #region Initialization 初始化

        TreeNode curFloorNode = null;
        TreeNode curRoomNode = null;
        TreeNode curFreshAirAreaNode = null;
        Floor curFloor = null;
        Room curRoom = null;
        FreshAirArea curFreshAirArea = null;
        Project thisProject;
        ProjectBLL projBll;
        RoomLoadIndexBLL bll = new RoomLoadIndexBLL();
        UndoRedo.UndoRedoHandler UndoRedoUtil = null; //注册撤销实体对象 add by axj 20161228 
        string No_Requirements = "No_Requirements"; //无需求房间 Add room w/o load requirements on 20180507 by xyj

        #region 新需求 20130809 会议 —— 将房间类型改为下拉框
        RoomLoadIndex LoadIndexItem;
        string curLocation; // 当前城市
        string curRoomType;
        DataTable dtRoomTypeSource;

        #endregion

        string utArea;
        string utLength_m;
        string utLoadindex;
        string utPower;
        string utAirflow;
        string utPressure;

        //ucMultiSelectTreeView tvRoomTemp = new ucMultiSelectTreeView();   //EU需求，克隆treeView副本

        public frmRoomManage(Project thisProj)
        {
            InitializeComponent();
            thisProject = thisProj;
            projBll = new ProjectBLL(thisProj);
            LoadIndexItem = new RoomLoadIndex(this.JCCurrentLanguage);
        }

        private void frmRoomManage_Load(object sender, EventArgs e)
        {
            Initialization();
            /*注册撤销功能 add by axj 20161228 begin */
            UndoRedoUtil = new UndoRedo.UndoRedoHandler(true);
            UndoRedoUtil.ShowIconsOnTabPage(tapPageTrans1, new Rectangle(626, 6, 16, 18), new Rectangle(598, 6, 16, 18));

            UndoRedoUtil.GetCurrentProjectEventHandler += delegate (out Project prj) //获取最新项目数据
            {
                prj = thisProject.DeepClone();//返回当前项目数据的副本
            };
            UndoRedoUtil.ReloadProjectEventHandler += delegate (Project prj) //重新加载历史记录里面的项目数据
            {
                thisProject = prj;
                projBll = new ProjectBLL(thisProject);
                Initialization();
                //this.Invalidate();
            };
            /*注册撤销功能 end*/           
        }

        private void Initialization()
        {
            this.JCCallValidationManager = true;
            JCSetLanguage();
            BindUnit();
            BindCityList();
            BindTreeViewRoom(1);
            BindTreeViewFreshAirArea(1);
            //由于一个Project下可能存在多个ProductType，房间管理无法准确判断是否是新风，因此删除下面一行
            //CheckFreshAirAreaEnable();
            BindEvents();
            this.tvRoom.ExpandAll();
            if (this.tvRoom.Nodes.Count > 0 && this.JCFormMode == FormMode.NEW)
            {
                this.tvRoom.Nodes[0].EnsureVisible();
            }
            this.tvFreshAirArea.ExpandAll();
            if (this.tvFreshAirArea.Nodes.Count > 0 && this.JCFormMode == FormMode.NEW)
            {
                this.tvFreshAirArea.Nodes[0].EnsureVisible();
            }

            //EU特殊处理，去除确认按钮   --add on 20180518 by Vince
            if (thisProject.RegionCode=="EU_E" || thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S")
            {
                this.jcbtnUpdateFloor.Visible = false;
                this.jcbtnUpdateFreshAirArea.Visible = false;
                this.jcbtnUpdateRoom.Visible = false;
                this.jcbtnOK.Visible = true;
                this.jcbtnCancel.Visible = true;
                this.jcbtnClose.Visible = false;
            }
        }

        private void BindEvents()
        {
            this.tvRoom.ItemDrag += new ItemDragEventHandler(tvRoom_ItemDrag);

            this.tvFreshAirArea.ItemDrag += new ItemDragEventHandler(tvFreshAirArea_ItemDrag);
            this.tvFreshAirArea.DragOver += new DragEventHandler(tvFreshAirArea_DragOver);
            this.tvFreshAirArea.DragDrop += new DragEventHandler(tvFreshAirArea_DragDrop);
        }

        /// <summary>
        /// 绑定城市下拉框
        /// </summary>
        private void BindCityList()
        {
            this.jccmbLocation.DisplayMember = "LoadCity";
            DataTable dt = bll.GetCityList();
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "LoadCity not in('ShangHai','上海','BeiJing','北京','GuangZhou','广州')";
                dt = dv.ToTable();
            }
            if (dt.Rows.Count > 0)
            {
                this.jccmbLocation.DataSource = dt;
            }
        }

        /// <summary>
        /// 绑定 
        /// </summary>
        /// <param name="city"></param>
        private void BindRoomTypeList(string city)
        {
            if (dtRoomTypeSource == null)
                dtRoomTypeSource = bll.GetRoomTypeList();

            if (!string.IsNullOrEmpty(city))
            {
                dtRoomTypeSource.DefaultView.RowFilter = "LoadCity='" + city + "'";
                //this.jccmbRoomType.DisplayMember = "RoomType";
                this.jccmbRoomType.DisplayMember = "CityRoomType"; //房间类型前面加上城市名 20160418 by lin

                //判断datatable 是否存在Msg.ManagementRoom_Custom 存在则不在添加
                bool isAdd = true;
                foreach (DataRow dr in dtRoomTypeSource.Rows)
                {
                    if (dr["RoomType"].ToString() == Msg.ManagementRoom_Custom)
                    {
                        isAdd = false;
                        break;
                    }
                } 
                if (isAdd)
                {
                    dtRoomTypeSource.Rows.Add(city, Msg.ManagementRoom_Custom, Msg.ManagementRoom_Custom);
                }
                this.jccmbRoomType.DataSource = dtRoomTypeSource;
            }
        }

        private void BindRoomTypeList()
        {
            dtRoomTypeSource = bll.GetRoomTypeList();
        }

        private void jcbtnClose_Click(object sender, EventArgs e)
        {
                Close();
        }

        #endregion

        #region Controls events

        private void tvRoom_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ResetNodeColor(tvFreshAirArea.Nodes);
            ResetNodeColor(tvRoom.Nodes);
            e.Node.BackColor = Color.FromArgb(51, 153, 255);
            int nodeLevel = CheckSelectNode();
            if (nodeLevel > 0)
            {
                if (nodeLevel == 1)
                {
                    SetFloorInfoState(true);
                    SetRoomInfoState(false);
                    BindFloorSourceToControl(false);
                }
                else if (nodeLevel == 2)
                {
                    SetFloorInfoState(false);
                    SetRoomInfoState(true);
                    BindRoomSourceToControl(false);
                    ResetSensibleHeatValidation();
                    ResetAirFlowValidation();
                    ResetStaticPressureValidation();
                }
            }
            else
            {
                SetFloorInfoState(false);
                SetRoomInfoState(false);
            }
            SetFreshAirAreaInfoState(false);
        }

        private void tvRoom_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.BackColor = this.BackColor;
        }
    

        #region 拖动相关事件

        /// 触发开始拖动的效果
        /// <summary>
        /// 触发开始拖动的效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvRoom_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
         
        /// 触发开始拖动的效果
        /// <summary>
        /// 触发开始拖动的效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvFreshAirArea_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        /// 拖拽到新风区域节点上时，做可行性检查
        /// <summary>
        /// 拖拽到新风区域节点上时，做可行性检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvFreshAirArea_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            ResetNodeColor(tvFreshAirArea.Nodes);

            // 获取目标节点
            TreeView tv = sender as TreeView;
            Point pt = tv.PointToClient(new Point(e.X, e.Y));
            TreeNode dstNode = tv.GetNodeAt(pt);

            if (dstNode == null) return;

            string typeSource = typeof(TreeNode).ToString();
            if (!e.Data.GetDataPresent(typeSource, false)) return;

            TreeNode srcNode = e.Data.GetData(typeSource) as TreeNode;
            ucMultiSelectTreeView srcTreeView = srcNode.TreeView as ucMultiSelectTreeView;
            foreach (TreeNode node in srcTreeView.SelectedNodes)
            {
                if (srcTreeView == tvRoom)
                {
                    if (node.Level == 2 && dstNode.Level == 1)
                    {
                        dstNode.ForeColor = Color.Red;
                        e.Effect = DragDropEffects.Move;
                        break;
                    }
                }
                else if (srcTreeView == tvFreshAirArea)
                {
                    if (node.Level == 2 && dstNode.Level == 1 && node.Parent != dstNode)
                    {
                        dstNode.ForeColor = Color.Red;
                        e.Effect = DragDropEffects.Move;
                        break;
                    }
                }
            }
        }

        /// 把房间拖拽到新风区域节点之后松开时，添加房间节点到新风区域
        /// <summary>
        /// 把房间拖拽到新风区域节点之后松开时，添加房间节点到新风区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tvFreshAirArea_DragDrop(object sender, DragEventArgs e)
        {
            ResetNodeColor(tvFreshAirArea.Nodes);


            // 获取目标节点
            TreeView tv = sender as TreeView;
            Point pt = tv.PointToClient(new Point(e.X, e.Y));
            TreeNode dstNode = tv.GetNodeAt(pt);
            if (dstNode == null) return;

            // 获取源节点
            TreeNode srcNode = null;
            string typeSource = typeof(TreeNode).ToString();
            if (e.Data.GetDataPresent(typeSource, false))
            {
                srcNode = e.Data.GetData(typeSource) as TreeNode;
            }
            if (srcNode == null) return;

            ucMultiSelectTreeView srcTreeView = srcNode.TreeView as ucMultiSelectTreeView;

            /*if (srcTreeView == tvRoom)
            {
                foreach (TreeNode node in srcTreeView.SelectedNodes)
                {
                    if (node.Level == 2 && node.TreeView != null)
                    {
                        Room room = node.Tag as Room;
                        FreshAirArea area = dstNode.Tag as FreshAirArea;

                        //如果此room已经添加过新风区域
                        if (!string.IsNullOrEmpty(room.FreshAirAreaId))
                        {
                            //如果此room已经属于此新风区域，则不做任何操作。
                            if (room.FreshAirAreaId == area.Id) break;

                            //否则，先把原来的room节点删掉
                            DeleteRoomFromFreshAirArea(room.FreshAirAreaId, room.Id);
                        }

                        //只在新风区域增加一个room节点，不会移除楼层的房间节点
                        TreeNode newNode = new TreeNode();
                        newNode.Tag = room;
                        newNode.Text = room.Name;
                        newNode.ForeColor = Color.Black;
                        dstNode.Nodes.Add(newNode);

                        newNode.EnsureVisible();

                        projBll.SetFreshAirAreaForRoom(room, area.Id);
                    }
                }
            }*/

            #region  TvRoom drag to FreshAirArea  modify on 20160810 by Lingjia Qiu
            if (srcTreeView == tvRoom)
            {
                StringBuilder roomNameSb= new StringBuilder();
                int index = 0;
                foreach (TreeNode node in srcTreeView.SelectedNodes)
                {
                    if (node.Level == 2 && node.TreeView != null)
                    {
                        Room room = node.Tag as Room;
                        FreshAirArea area = dstNode.Tag as FreshAirArea;

                        //如果此room已经添加过新风区域
                        if (!string.IsNullOrEmpty(room.FreshAirAreaId))
                        {
                            //如果此room已经属于此新风区域，则不做任何操作。 modify on 20160810 by Lingjia Qiu
                            //if (room.FreshAirAreaId == area.Id) break;
                            Boolean isSelectedTvRomm = false;
                            foreach (FreshAirArea freasAirArea in thisProject.FreshAirAreaList)
                            {
                                if (room.FreshAirAreaId == freasAirArea.Id)
                                {
                                    isSelectedTvRomm = true;
                                    break;
                                }
                            }
                            if (isSelectedTvRomm)
                            {
                                //roomNameSb.AppendLine(node.Parent.Text + ":" + room.Name);
                                roomNameSb.AppendFormat(" {0}:{1} ;", node.Parent.Text, room.Name);
                                index++;
                                //JCMsg.ShowWarningOK(Msg.WARNING_ROOM_SELECTED(roomNameStr.ToString(0,roomNameStr.Length-1)));
                                continue;
                            } 

                            //否则，先把原来的room节点删掉
                            DeleteRoomFromFreshAirArea(room.FreshAirAreaId, room.Id);
                        }

                        //只在新风区域增加一个room节点，不会移除楼层的房间节点
                        TreeNode newNode = new TreeNode();
                        newNode.Tag = room;
                        //newNode.Text = room.Name;
                        newNode.Text = node.Parent.Text + ":" + room.Name;  //modify on 20160810 by Lingjia Qiu
                        newNode.ForeColor = Color.Black;
                        dstNode.Nodes.Add(newNode);

                        newNode.EnsureVisible();

                        projBll.SetFreshAirAreaForRoom(room, area.Id);
                    }
                }
                if(index > 0)
                    JCMsg.ShowInfoOK(Msg.WARNING_ROOM_SELECTED(roomNameSb.ToString(0, roomNameSb.Length - 1)));
            }
            #endregion
            else if (srcTreeView == tvFreshAirArea)
            {
                for (int i = 0; i < srcTreeView.SelectedNodes.Count; i++)
                {
                    TreeNode node = srcTreeView.SelectedNodes[i] as TreeNode;
                    if (node.Level == 2 && node.TreeView != null)
                    {
                        node.Remove();
                        dstNode.Nodes.Add(node);
                        node.EnsureVisible();

                        Room room = node.Tag as Room;
                        FreshAirArea area = dstNode.Tag as FreshAirArea;
                        projBll.SetFreshAirAreaForRoom(room, area.Id);
                    }
                }
            }
        }

        #endregion

        #region 新风相关

        private void tvFreshAirArea_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ResetNodeColor(tvRoom.Nodes);
            ResetNodeColor(tvFreshAirArea.Nodes);
            e.Node.BackColor = Color.FromArgb(51, 153, 255);
            int nodeLevel = CheckSelectFreshAirAreaNode();

            SetRoomInfoState(false);
            SetFloorInfoState(false);
            SetFreshAirAreaInfoState(false);

            if (nodeLevel == 1)
            {
                SetFreshAirAreaInfoState(true);
                BindFreshAirAreaSourceToControl(false);
            }
            else if (nodeLevel == 2)
            {
                SetRoomInfoState(true);
                BindRoomSourceToControl(false);
                ResetSensibleHeatValidation();
                ResetAirFlowValidation();
                ResetStaticPressureValidation();
            }
        }

        private void tvFreshAirArea_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.BackColor = this.BackColor;
        }

        private void jcbtnAddFreshAirArea_Click(object sender, EventArgs e)
        {
            DoAddFreshAirArea();
            
            BindTreeViewFreshAirArea(1);
            UndoRedoUtil.SaveProjectHistory();
        }

        private void jcbtnUpdateFreshAirArea_Click(object sender, EventArgs e)
        {
            if (!this.JCValidateGroup(pnlContent_FreshAirArea_1))
                return;

            if (curFreshAirArea == null)
                return;

            DoUpdateFreshAirArea();
            BindTreeViewFreshAirArea(1);
            UndoRedoUtil.SaveProjectHistory();

            bindFreshAirInfo();
        }

        private void pbDeleteFreshAirArea_Click(object sender, EventArgs e)
        { 
            if (!CheckAllSelectNode(tvFreshAirArea))
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }

            DialogResult dr = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL, true);
            if (dr != DialogResult.OK)
            {
                return;
            }

            curFreshAirAreaNode = null;

            for (int i = 0; i < tvFreshAirArea.SelectedNodes.Count; i++)
            {

                //TreeNode node = tvRoom.SelectedNodes[i] as TreeNode;
                TreeNode node = tvFreshAirArea.SelectedNodes[i] as TreeNode;

                if (node.Level == 0)
                {
                    continue;
                }
                else if (node.Level == 1 && node.Tag is FreshAirArea)
                {
                    FreshAirArea f = node.Tag as FreshAirArea;
                    projBll.DeleteFreshAirArea(f.Id);
                }
                else if (node.Level == 2 && node.Tag is Room)
                {
                    projBll.SetFreshAirAreaForRoom(node.Tag as Room, null);
                }
                tvFreshAirArea.Nodes.Remove(node);//删除节点不需要刷新整个TreeView
                UndoRedoUtil.SaveProjectHistory();
            }
            tvFreshAirArea.SelectedNodes.Clear();
            tvRoom.Focus();
            tvFreshAirArea.Focus();
        }

        private void pbCopyFreshAirArea_Click(object sender, EventArgs e)
        {
            int nodeLevel = CheckSelectFreshAirAreaNode();
            if (nodeLevel == 1)
            {
                DoCopyAddFreshAirArea();
                UndoRedoUtil.SaveProjectHistory();
                BindTreeViewRoom(nodeLevel);
                BindTreeViewFreshAirArea(nodeLevel);
                ResetNodeColor(tvRoom.Nodes);
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        #endregion
        
        private void ResetNodeColor(TreeNodeCollection treeNodeCollection)
        {
            foreach (TreeNode node in treeNodeCollection)
            {
                node.ForeColor = this.ForeColor;
                node.BackColor = this.BackColor;

                ResetNodeColor(node.Nodes);
            }
        }

        private void pbCopy_Click(object sender, EventArgs e)
        {
            int nodeLevel = CheckSelectNode();
            if (nodeLevel > 0)
            {
                if (nodeLevel == 1)
                {
                    DoCopyAddFloor();
                    UndoRedoUtil.SaveProjectHistory();
                    BindTreeViewRoom(nodeLevel);
                    BindTreeViewFreshAirArea(nodeLevel);
                    ResetNodeColor(tvFreshAirArea.Nodes);
                }
                else if (nodeLevel == 2)
                {
                    DoCopyAddRoom();
                    UndoRedoUtil.SaveProjectHistory();
                    BindTreeViewRoom(nodeLevel);

                    if (curRoom != null && curRoom.FreshAirAreaId != null)
                    {
                        BindTreeViewFreshAirArea(nodeLevel);
                        ResetNodeColor(tvFreshAirArea.Nodes);
                    }
                }
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        private void pbDelete_Click(object sender, EventArgs e)
        {
            if (!CheckAllSelectNode(tvRoom))
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return;
            }

            DialogResult dr = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL, true);
            if (dr != DialogResult.OK)
            {
                return;
            }

            curFloor = null;
            curRoom = null;

            for (int i = 0; i < tvRoom.SelectedNodes.Count; i++)
            {
                TreeNode node = tvRoom.SelectedNodes[i] as TreeNode;

                if (node.TreeView == null) continue;

                if (node.Level == 0)
                {
                    continue;
                }
                else if (node.Level == 1 && node.Tag is Floor)
                {
                    Floor theFloor = node.Tag as Floor;
                    foreach (Room r in theFloor.RoomList)
                    {
                        ClearIndoorRoomID(r.Id);

                        //清除exchanger
                        ClearExchangerRoomID(r.Id);
                    }
                    projBll.DeleteFloor(theFloor.Id);
                }
                else if (node.Level == 2 && node.Tag is Room && node.Parent.Tag is Floor)
                {
                    Room theRoom = node.Tag as Room;
                    Floor theFloor = node.Parent.Tag as Floor;
                    ClearIndoorRoomID(theRoom.Id);
                    //清除exchanger
                    ClearExchangerRoomID(theRoom.Id);
                    projBll.DeleteRoom(theRoom.Id, theFloor);
                }
                //删除节点不需要刷新整个TreeView
                tvRoom.Nodes.Remove(node);
                UndoRedoUtil.SaveProjectHistory();

            }
            //同步刷新新风区域
            BindTreeViewFreshAirArea(2);
            tvFreshAirArea.Focus();
            tvRoom.Focus();
            //tvRoom.SelectedNodes.Clear();
        }

        private void jccbxSensibleHeat_CheckedChanged(object sender, EventArgs e)
        {
            //bool isChecked = this.uc_CheckBox_SensibleHeat.Checked;
            //this.jctxtSensibleHeat.Enabled = isChecked;
            //this.jctxtSensibleHeat.AllowNull = !isChecked;
            //this.jctxtSensibleHeat.RequireValidation = isChecked;
            //this.jctxtSensibleHeat.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            ResetSensibleHeatValidation();
            jctxtRoomInfo_Leave(sender, e);
        }

        private void jccbxAirFlow_CheckedChanged(object sender, EventArgs e)
        {
            ResetAirFlowValidation();
            jctxtRoomInfo_Leave(sender, e);
        }

        private void jccbxStaticPressure_CheckedChanged(object sender, EventArgs e)
        {
            ResetStaticPressureValidation();
            jctxtRoomInfo_Leave(sender, e);
        }

        private void pbLoadIndex_Click(object sender, EventArgs e)
        {
            frmLoadIndex f = new frmLoadIndex(thisProject.RegionCode,jccmbLocation.Text, jccmbRoomType.Text);
            if (f.ShowDialog() == DialogResult.OK)
            {
                BindCityList();
                BindRoomTypeList();

                this.jccmbLocation.Text = f.SelectedItem.City;
                BindRoomTypeList(f.SelectedItem.City);
                this.jccmbRoomType.Text = f.SelectedItem.RoomType;
            }
        }

        private void jcbtnAdd_Click(object sender, EventArgs e)
        {
            VRFButton jcbtn = sender as VRFButton;
            if (jcbtn.Name == jcbtnAddFloor.Name)
            {
                DoAddFloor(); 
                BindTreeViewRoom(1);
                UndoRedoUtil.SaveProjectHistory();
            }
            else if (jcbtn.Name == jcbtnAddRoom.Name)
            {
                DoAddRoom();
               
                BindTreeViewRoom(2);
                UndoRedoUtil.SaveProjectHistory();
            } 
        }

        private void jcbtnUpdate_Click(object sender, EventArgs e)
        {
            VRFButton jcbtn = sender as VRFButton;
            if (jcbtn.Name == jcbtnUpdateFloor.Name)
            {
                //if (!this.JCValidateGroup(pnlContent_Floor_1))
                //    return;

                //if (curFloor == null)
                //    return;
                //if (Convert.ToDecimal(jctxtHeight.Text) >= 0)
                //{
                //    DoUpdateFloor();

                //}
                //else { 
                //    JCMsg.ShowErrorOK(Msg.WARNING_TXT_POSITIVENUM(jclblHeight.Text));
                //    return;
                //}
                //BindTreeViewRoom(1);
                //UtilTrace.SaveHistoryTraces(null, null, regEnt);
                bindFloorInfo();
            }
            else if (jcbtn.Name == jcbtnUpdateRoom.Name)
            {
                //ResetSensibleHeatValidation();
                //ResetAirFlowValidation();
                //ResetStaticPressureValidation();
                ////if (!this.JCValidateGroup(pnlContent_2_Room))
                //if (!this.JCValidateGroup(pnlContent_Room))
                //    return;

                ////增加Sensib Heat小于Total Capacity提示
                //if ((jctxtSensibleHeat.Text != "") && (jctxtRqCapCool.Text != ""))
                //{
                //    if (double.Parse(jctxtSensibleHeat.Text) > double.Parse(jctxtRqCapCool.Text))
                //    {
                //        JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(uc_CheckBox_SensibleHeat.TextString, "[" + jclblRqCapCool.Text + "]"));
                //        return;
                //    }
                //}

                //if (curRoom == null)
                //    return;
                //if (DoUpdateRoom())
                //{
                //    // 检查该房间是否有关联的RoomIndoor记录
                //    UpdateIndoorRqInfo(curRoom.Id);
                //    // 检查该房间是否有关联的Exchanger记录
                //    UpdateExchangerRqInfo(curRoom.Id);
                //    BindTreeViewRoom(2);

                //    //更新新风区域节点下的房间节点
                //    if (curRoom.FreshAirAreaId != null)
                //    {
                //        UpdateRoomInfoOfFreshAirArea();
                //        BindTreeViewFreshAirArea(2);
                //    }
                //    UtilTrace.SaveHistoryTraces(null, null, regEnt);
                //}
                bindRoomInfo();
            }
            
        }

        private void jctxtLoadIndexCool_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox); 
            CalRqCapacityCool();
        }

        private void jctxtLoadIndexHeat_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox); 
            CalRqCapacityHeat();
        }

        private void jctxtArea_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox); 
            CalRqCapacityCool(); 
            CalRqCapacityHeat();
        }

        private void jctxtIndexFA_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox); 
            CalRqFA();
        }

        private void jctxtNoOfPeople_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox); 
            CalRqFA();
        }

        #region 新需求 20130809 会议
        private void jccmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (curLocation == this.jccmbLocation.Text)
                return;
            curLocation = this.jccmbLocation.Text.Trim();

            BindRoomTypeList(curLocation);

            RoomLoadIndex item = bll.GetRoomLoadIndexItem(curLocation, this.jccmbRoomType.Text);
            if (item != null)
            {
                this.jctxtLoadIndexCool.Text = Unit.ConvertToControl(item.CoolingIndex, UnitType.LOADINDEX, utLoadindex).ToString("n1");
                this.jctxtLoadIndexHeat.Text = Unit.ConvertToControl(item.HeatingIndex, UnitType.LOADINDEX, utLoadindex).ToString("n1");
            }
        }

        private void jccmbRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(curLocation))
            //    return;

            //if (this.jccmbRoomType.Text.ToString() == Msg.ManagementRoom_Custom.ToString())
            //{ 
            //    jctxtLoadIndexCool.Text = "0"; 
            //    jctxtLoadIndexHeat.Text = "0";
            //    jctxtRqCapCool.Text = "0";
            //    jctxtRqCapHeat.Text = "0";
            //    jctxtIndexFA.Text = "0";
            //    jctxtRqFA.Text = "0";
            //}

            ////if (curRoomType == this.jccmbRoomType.Text)
            ////    return;
            //curRoomType = this.jccmbRoomType.Text;

            //RoomLoadIndex item = bll.GetRoomLoadIndexItem(curLocation, curRoomType);
            //if (item != null)
            //{
            //    this.jctxtLoadIndexCool.Text = Unit.ConvertToControl(item.CoolingIndex, UnitType.LOADINDEX, utLoadindex).ToString("n1");
            //    this.jctxtLoadIndexHeat.Text = Unit.ConvertToControl(item.HeatingIndex, UnitType.LOADINDEX, utLoadindex).ToString("n1");
            //}
        }
        #endregion

        #endregion

        #region Response codes (Methods && Events)

        // 绑定单位表达式
        /// <summary>
        /// 绑定单位表达式
        /// </summary>
        private void BindUnit()
        {
            utArea = SystemSetting.UserSetting.unitsSetting.settingAREA;
            utLength_m = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            utLoadindex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
            utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            //utPressure = Unit.ut_Pressure;
            utPressure = SystemSetting.UserSetting.unitsSetting.settingESP;

            jclblUnitAirFlow1.Text = utAirflow;
            jclblUnitAirFlow2.Text = utAirflow;
            jclblUnitStaticPressure.Text = utPressure;

            jclblUnitArea.Text = utArea;
            jclblUnitLength_m.Text = utLength_m;
            jclblUnitLoadIndex1.Text = utLoadindex;
            jclblUnitLoadIndex2.Text = utLoadindex;
            jclblUnitLoadIndex3.Text = utAirflow + " per person"; //utAirflow.Contains("/") ? utAirflow + CDL.Unit.Boldsymbol  + "Person" : utAirflow + "/Person";   
            jclblUnitPower1.Text = utPower;
            jclblUnitPower2.Text = utPower;
            jclblUnitPower3.Text = utPower; 
        }

        // 绑定 TreeViewRoom
        /// <summary>
        /// 绑定 TreeViewRoom
        /// </summary>
        /// <param name="nodeLevel"> 需要展开的节点的层数 </param>
        private void BindTreeViewRoom(int nodeLevel)
        {
            this.tvRoom.Nodes.Clear();             
            foreach (Floor f in thisProject.FloorList)
            {
                if (!hasParentNode(f.ParentId))
                {
                    tvRoom.Nodes.Add(f.ParentId.ToString());
                   // tvRoom.Nodes[0].Expand();
                }
                tvRoom.ExpandAll();
                foreach (TreeNode tn in tvRoom.Nodes)
                {
                    
                    if (tn.Text == f.ParentId.ToString())
                    {
                        TreeNode nodeFloor = new TreeNode();
                        nodeFloor.Tag = f;
                        nodeFloor.Text = f.Name;
                        tn.Nodes.Add(nodeFloor);
                        nodeFloor.ForeColor = Color.Black;

                        if (nodeLevel == 1 && curFloor == f)
                        {
                            nodeFloor.EnsureVisible();
                            if (curRoom == null || (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_E" && thisProject.RegionCode != "EU_S"))       //EU区域统一提交信息，防止初始节点信息                     
                                tvRoom.SelectedNode = nodeFloor;
                            nodeFloor.ForeColor = Color.Blue;

                        }

                        foreach (Room r in f.RoomList)
                        {
                            TreeNode nodeRoom = new TreeNode();
                            nodeRoom.Tag = r;
                            //string roomNO = bll.GetRoomNOString(r.NO, f);
                            //nodeRoom.Text = roomNO +":"+ r.Name;  // 20130709 会议
                            nodeRoom.Text = r.Name;
                            nodeFloor.Nodes.Add(nodeRoom);
                            nodeRoom.ForeColor = Color.Black;

                            if (nodeLevel == 2 && curRoom == r)
                            {
                                nodeRoom.EnsureVisible();
                                tvRoom.SelectedNode = nodeRoom;
                                nodeRoom.ForeColor = Color.Blue;
                            }
                        }
                    }
                }
            }

            if (tvRoom.Nodes.Count == 0)
            {
                SetFloorInfoState(false);
                SetRoomInfoState(false);
            }
            else if (curRoom == null)
            {
                jctxtRoomNo.Text = "";
                jctxtRoomName.Text = "";
                if (curFloor == null)
                {
                    tvRoom.Nodes[0].Expand();
                    jctxtFloorNo.Text = "";
                    jctxtFloorName.Text = "";
                    jctxtHeight.Text = "";
                }
            }        

        }
        
        // 检测是否已经包含当前楼层所属的父节点
        /// <summary>
        /// 检测是否已经包含当前楼层所属的父节点
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        private bool hasParentNode(int pId)
        {
            foreach (TreeNode tn in tvRoom.Nodes)
            {
                if (Convert.ToInt32(tn.Text) == pId)
                    return true;
            }
            return false;
        }

        // 改变楼层信息区域的输入有效性
        /// <summary>
        /// 改变楼层信息区域的输入有效性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetFloorInfoState(bool isEnable)
        {
            Color cFloorInfo = Color.FromArgb(130, 130, 130);
            if (!isEnable)
            {
                cFloorInfo = Color.FromArgb(130, 130, 130);
            }

            pnlContent_Floor_1.Enabled = isEnable;
            //lblTitle_FloorInfo.Enabled = true;
            jcbtnAddFloor.Enabled = true;
            jcbtnUpdateFloor.Enabled = isEnable;

            lblTitle_FloorInfo.BackColor = cFloorInfo;
        }

        // 改变房间信息区域的输入有效性
        /// <summary>
        /// 改变房间信息区域的输入有效性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetRoomInfoState(bool isEnable)
        {
            Color cRoomInfo = Color.FromArgb(4, 69, 125);
            Color cCooling = Color.FromArgb(0, 107, 248);
            Color cHeating = Color.FromArgb(242, 121, 89);
            Color cFreshAir = Color.FromArgb(216, 221, 224);
            if (!isEnable)
            {
                cRoomInfo = Color.FromArgb(4, 69, 125);
                cCooling = Color.FromArgb(0, 107, 248);
                cHeating = Color.FromArgb(242, 121, 89);
                cFreshAir = Color.FromArgb(216, 221, 224);
            }
            lblTitle_RoomInfo.BackColor = cRoomInfo;
            lblTitle_Cooling.BackColor = cCooling;
            lblTitle_Heating.BackColor = cHeating;
            lblTitle_FreshAir.BackColor = cFreshAir;

            pnlContent_2_Room_1.Enabled = isEnable;
            pnlCooling_1.Enabled = isEnable;
            pnlHeating_1.Enabled = isEnable;
            pnlFreshAir_1.Enabled = isEnable;

            jcbtnAddRoom.Enabled = true;
            jcbtnUpdateRoom.Enabled = isEnable;
        }

        // 绑定楼层对象的属性到界面控件
        /// <summary>
        /// 绑定楼层对象的属性到界面控件
        /// </summary>
        /// <param name="isAdd"></param>
        private void BindFloorSourceToControl(bool isAdd)
        {
            if (isAdd)
            {
                curFloor = new Floor(projBll.GetNextFloorNO());

                string namePrefix = SystemSetting.UserSetting.defaultSetting.FloorName; // 从 SettingConfig 获取
                string _name = namePrefix + curFloor.NO.ToString();
                double _height = SystemSetting.UserSetting.defaultSetting.RoomHeight;

                curFloor.Name = _name;
                curFloor.Height = _height;

            }
            else
            {
                if (curFloorNode != null)
                {
                    curFloor = (Floor)curFloorNode.Tag;
                }
            }
            if (curFloor != null)
            {
                jctxtFloorNo.Text = curFloor.NO.ToString();
                jctxtFloorName.Text = curFloor.Name;
                jctxtHeight.Text = Unit.ConvertToControl(curFloor.Height, UnitType.LENGTH_M, utLength_m).ToString("n2");

                // //EU区域统一提交信息，防止初始楼层信息
                //if (!thisProject.RegionCode.StartsWith("EU") || string.IsNullOrEmpty(jctxtFloorName.Text)) 
                    //jctxtFloorName.Text = curFloor.Name;
                //if (!thisProject.RegionCode.StartsWith("EU") || string.IsNullOrEmpty(jctxtHeight.Text))
                    // jctxtHeight.Text = Unit.ConvertToControl(curFloor.Height, UnitType.LENGTH_M, utLength_m).ToString("n2");
            }
        }

        // 绑定房间对象的属性到界面控件
        /// <summary>
        /// 绑定房间对象的属性到界面控件
        /// </summary>
        /// <param name="isAdd"></param>
        private void BindRoomSourceToControl(bool isAdd)
        {
            if (curFloor == null)
                return;
            BindFloorSourceToControl(false);

            if (isAdd)
            {
                curRoom = new Room(projBll.GetNextRoomNO(curFloor));
                string namePrefix = SystemSetting.UserSetting.defaultSetting.RoomName;
                string _name = namePrefix + curRoom.NO.ToString();
                curRoom.Name = _name;

                curRoom.Location = this.jccmbLocation.Text;
                curRoom.Type = this.jccmbRoomType.Text.Trim();

                if (curRoom.Type == Msg.ManagementRoom_Custom)
                {
                    curRoom.Type = No_Requirements;
                }

                // LoadIndex数值由当前的RoomType决定
                curRoom.LoadIndexCool = string.IsNullOrEmpty(this.jctxtLoadIndexCool.Text) ? 0 :
Unit.ConvertToSource(double.Parse(this.jctxtLoadIndexCool.Text), UnitType.LOADINDEX, utLoadindex);
                curRoom.LoadIndexHeat = string.IsNullOrEmpty(this.jctxtLoadIndexHeat.Text) ? 0 :
Unit.ConvertToSource(double.Parse(this.jctxtLoadIndexHeat.Text), UnitType.LOADINDEX, utLoadindex);

                //2013-6-20 如果是新增房间, 则应该将当前软件界面上的控件值赋值给curRoom
                if (jctxtRoomNo.Text == "")
                {
                    //无RoomNo时(即添加第一个房间时)，获取默认curRoom，并进行下面的计算
                    curRoom.RqCapacityCool = curRoom.Area * curRoom.LoadIndexCool / 1000;
                    curRoom.RqCapacityHeat = curRoom.Area * curRoom.LoadIndexHeat / 1000;
                }
                else
                {
                    //有RoomNo时，将界面控件的属性值赋值给curRoom
                    DoAddCopyRoom();
                }
            }
            else
            {
                if (curRoomNode == null)
                    return;
                curRoom = (Room)curRoomNode.Tag;

                this.jccmbLocation.Text = curRoom.Location; 
                if (curRoom.Type == No_Requirements)
                {
                    this.jccmbRoomType.Text = Msg.ManagementRoom_Custom;
                }
                else
                {
                    this.jccmbRoomType.Text = curRoom.Type;
                }
                this.jctxtLoadIndexCool.Text = Unit.ConvertToControl(curRoom.LoadIndexCool, UnitType.LOADINDEX, utLoadindex).ToString("n1");
                this.jctxtLoadIndexHeat.Text = Unit.ConvertToControl(curRoom.LoadIndexHeat, UnitType.LOADINDEX, utLoadindex).ToString("n1");
            }

            #region 将新增或选中的当前房间的属性绑定到控件 
            //2013-6-20 如果不是新增房间，则将curRoom属性的值赋值给软件界面的控件
            jctxtRoomNo.Text = projBll.GetRoomNOString(curRoom.NO, curFloor);
            jctxtRoomName.Text = curRoom.Name;
            jctxtArea.Text = Unit.ConvertToControl(curRoom.Area, UnitType.AREA, utArea).ToString("n1");
            jctxtNoOfPeople.Text = curRoom.PeopleNumber.ToString("n0");

            // Load index 赋值语句需要放在RqCapacity控件赋值语句之前，否则会触发Load Index的chang事件
            jctxtIndexFA.Text = Unit.ConvertToControl(curRoom.FreshAirIndex, UnitType.AIRFLOW, utAirflow).ToString("n1");

            double d = Unit.ConvertToControl(curRoom.RqCapacityCool, UnitType.POWER, utPower);
            jctxtRqCapCool.Text = d.ToString("n1");
            double s = Unit.ConvertToControl(curRoom.RqCapacityHeat, UnitType.POWER, utPower);
            jctxtRqCapHeat.Text = s.ToString("n1");

            jctxtRqFA.Text = Unit.ConvertToControl(curRoom.FreshAir, UnitType.AIRFLOW, utAirflow).ToString("n1");

            this.uc_CheckBox_SensibleHeat.Checked = curRoom.IsSensibleHeatEnable;
            ResetSensibleHeatValidation();
            jctxtSensibleHeat.Text = curRoom.IsSensibleHeatEnable ? 
                Unit.ConvertToControl(curRoom.SensibleHeat, UnitType.POWER, utPower).ToString("n1") : "";
            
            this.uc_CheckBox_AirFlow.Checked = curRoom.IsAirFlowEnable;
            ResetAirFlowValidation();
            jctxtAirFlow.Text = curRoom.IsAirFlowEnable ?
                Unit.ConvertToControl(curRoom.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n1") : "";
            this.uc_CheckBox_StaticPressure.Checked = curRoom.IsStaticPressureEnable;
            ResetStaticPressureValidation();
            //jctxtStaticPressure.Text = curRoom.IsStaticPressureEnable ? curRoom.StaticPressure.ToString("n1") : "";
            jctxtStaticPressure.Text = curRoom.IsStaticPressureEnable ? Unit.ConvertToControl(curRoom.StaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2") : "";
            #endregion

            #region 第一个房间应该从控件中重新取一下capacity值 add on 20160818 by Yunxiao Lin
            DoAddCopyRoom();
            #endregion
        }

        // 绑定界面控件值到当前选中的楼层对象
        /// <summary>
        /// 绑定界面控件值到当前选中的楼层对象
        /// </summary>
        private void BindFloorControlToSource()
        {
            curFloor.Name = this.jctxtFloorName.Text;
            curFloor.Height = Unit.ConvertToSource(double.Parse(this.jctxtHeight.Text), UnitType.LENGTH_M, utLength_m);
        }

        // 绑定界面控件值到当前选中的房间对象
        /// <summary>
        /// 绑定界面控件值到当前选中的房间对象
        /// </summary>
        private void BindRoomControlToSource()
        {
            curRoom.Name = this.jctxtRoomName.Text;
            curRoom.Location = this.jccmbLocation.Text.Trim();
            curRoom.Type = this.jccmbRoomType.Text.Trim();
            curRoom.Area = Unit.ConvertToSource(double.Parse(this.jctxtArea.Text), UnitType.AREA, utArea);
            curRoom.PeopleNumber = int.Parse(this.jctxtNoOfPeople.Text);
            curRoom.LoadIndexCool = string.IsNullOrEmpty(this.jctxtLoadIndexCool.Text) ? 0 :
                Unit.ConvertToSource(double.Parse(this.jctxtLoadIndexCool.Text), UnitType.LOADINDEX, utLoadindex);
            curRoom.RqCapacityCool = Unit.ConvertToSource(double.Parse(this.jctxtRqCapCool.Text), UnitType.POWER, utPower);
            curRoom.IsSensibleHeatEnable = this.uc_CheckBox_SensibleHeat.Checked;
            curRoom.SensibleHeat = curRoom.IsSensibleHeatEnable ? Unit.ConvertToSource(double.Parse(this.jctxtSensibleHeat.Text), UnitType.POWER, utPower) : 0;
            curRoom.IsAirFlowEnable = this.uc_CheckBox_AirFlow.Checked;
            curRoom.AirFlow = curRoom.IsAirFlowEnable ? Unit.ConvertToSource(double.Parse(this.jctxtAirFlow.Text), UnitType.AIRFLOW, utAirflow) : 0;
            curRoom.IsStaticPressureEnable = this.uc_CheckBox_StaticPressure.Checked;
            //curRoom.StaticPressure = curRoom.IsStaticPressureEnable ? double.Parse(this.jctxtStaticPressure.Text) : 0;
            curRoom.StaticPressure = curRoom.IsStaticPressureEnable ? Unit.ConvertToSource(double.Parse(this.jctxtStaticPressure.Text), UnitType.STATICPRESSURE, utPressure) : 0;
            curRoom.LoadIndexHeat = string.IsNullOrEmpty(this.jctxtLoadIndexHeat.Text) ? 0 :
                Unit.ConvertToSource(double.Parse(this.jctxtLoadIndexHeat.Text), UnitType.LOADINDEX, utLoadindex);
            curRoom.RqCapacityHeat = Unit.ConvertToSource(double.Parse(this.jctxtRqCapHeat.Text), UnitType.POWER, utPower);
            curRoom.FreshAirIndex = string.IsNullOrEmpty(this.jctxtIndexFA.Text) ? 0 :
                Unit.ConvertToSource(double.Parse(this.jctxtIndexFA.Text), UnitType.AIRFLOW, utAirflow);

            if (curRoom.Type == Msg.ManagementRoom_Custom)
            {
                curRoom.Type = No_Requirements;
            }
            if (NumberUtil.IsNumber(this.jctxtRqFA.Text))
            {
                curRoom.FreshAir = Unit.ConvertToSource(double.Parse(this.jctxtRqFA.Text), UnitType.AIRFLOW, utAirflow);
            }
            else {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(this.jclblRqFA.Text));
                return;

              //  curRoom.FreshAir = Unit.ConvertToSource(double.Parse("0"), UnitType.AIRFLOW, utAirflow);
            }
        }

        // 获取当前选中的节点 level, 并绑定当前选中的节点
        /// <summary>
        /// 获取当前选中的节点 level, 并绑定当前选中的节点
        /// </summary>
        /// <returns></returns>
        private bool CheckAllSelectNode(ucMultiSelectTreeView tree)
        {
            if (tree.SelectedNodes == null) return false;

            foreach (TreeNode node in tree.SelectedNodes)
            {
                if (node.TreeView != null)
                {
                    return true;
                }
            }
            return false;
        }

        // 获取当前选中的节点 level, 并绑定当前选中的节点
        /// <summary>
        /// 获取当前选中的节点 level, 并绑定当前选中的节点
        /// </summary>
        /// <returns></returns>
        private int CheckSelectNode()
        {
            curRoomNode = null;
            curFloorNode = null;
            curFreshAirAreaNode = null;

            curFloor = null;
            curRoom = null;
            curFreshAirAreaNode = null;

            if (this.tvRoom.SelectedNode == null)
            {
                return -1;
            }
            int level = this.tvRoom.SelectedNode.Level;
            if (level == 1)
            {
                curFloorNode = tvRoom.SelectedNode;
                curFloor = (Floor)curFloorNode.Tag;
            }
            else if (level == 2)
            {
                curRoomNode = tvRoom.SelectedNode;
                curFloorNode = curRoomNode.Parent;

                curFloor = (Floor)curFloorNode.Tag;
                curRoom = (Room)curRoomNode.Tag;
            }
            else
                return 0;

            //if (curFloor == null)
            //    curFloor = (Floor)curFloorNode.Tag;
            return level;
        }

        // 添加新楼层节点
        /// <summary>
        /// 添加新楼层节点
        /// </summary>
        private void DoAddFloor()
        {
            // 创建新楼层对象或获取当前选中楼层对象，绑定到界面相应的控件
            BindFloorSourceToControl(true);
            projBll.AddFloor(curFloor);
            curRoom = null;
            
        }

        // 更新楼层属性
        /// <summary>
        /// 更新楼层属性
        /// </summary>
        private void DoUpdateFloor()
        {
            if (this.JCValidateGroup(pnlContent_Floor_1))
            {
                BindFloorControlToSource();
            }
        }

        // 拷贝选中的楼层，生成新楼层节点，默认选中新添加的节点
        /// <summary>
        /// 拷贝选中的楼层，生成新楼层节点，默认选中新添加的节点
        /// </summary>
        private void DoCopyAddFloor()
        {
            if (curFloor != null)
            {
                Floor newFloor = projBll.CopyAddFloor(curFloor);

                string namePrefix = SystemSetting.UserSetting.defaultSetting.FloorName; // 从 SettingConfig 获取
                string _name = namePrefix + newFloor.NO.ToString();
               // double _height = SystemSetting.UserSetting.defaultSetting.roomHeight; //取消拷贝楼层 默认去设置里面的楼层高度 on 2018/2/9 by xyj
                double _height = curFloor.Height; //取拷贝楼层的高度
                newFloor.Name = _name;
                newFloor.Height = _height;

                curFloor = newFloor;
                curRoom = null;
            }
            else
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
        }

        // 删除楼层节点
        /// <summary>
        /// 删除楼层节点
        /// </summary>
        private void DoDeleteFloor()
        {
            projBll.DeleteFloor(curFloor.Id);
            curFloorNode = null;
        }

        // 添加新房间节点
        /// <summary>
        /// 添加新房间节点
        /// </summary>
        private void DoAddRoom()
        {
            if (curFloorNode != null)
            {
                if (!checkSensible())   //添加room前需要校验当前sensible容量记录
                    return;
                BindRoomSourceToControl(true);
                projBll.AddRoom(curRoom, curFloor);
            }
            else
                JCMsg.ShowWarningOK(Msg.WARNING_SELECTFLOOR);
        }

        // 更新当前选中房间属性
        /// <summary>
        /// 更新当前选中房间属性
        /// </summary>
        /// <returns></returns>
        private bool DoUpdateRoom()
        {
            if (this.JCValidateGroup(pnlContent_Room))
            {
                BindRoomControlToSource();
                return true;
            }
            return false;
        }

        // 添加房间，使用界面输入的属性值，Yang
        private bool DoAddCopyRoom()
        {
            if (this.JCValidateGroup(pnlContent_Room))
            {
                curRoom.Area = Unit.ConvertToSource(double.Parse(this.jctxtArea.Text), UnitType.AREA, utArea);
                curRoom.PeopleNumber = int.Parse(this.jctxtNoOfPeople.Text);
                //curRoom.LoadIndexCool = string.IsNullOrEmpty(this.jctxtLoadIndexCool.Text) ? 0 :
                //    Unit.ConvertToSource(double.Parse(this.jctxtLoadIndexCool.Text), UnitType.LOADINDEX, utLoadindex);
                curRoom.RqCapacityCool = Unit.ConvertToSource(double.Parse(this.jctxtRqCapCool.Text), UnitType.POWER, utPower);
                curRoom.IsSensibleHeatEnable = this.uc_CheckBox_SensibleHeat.Checked;
                curRoom.SensibleHeat = curRoom.IsSensibleHeatEnable ? Unit.ConvertToSource(double.Parse(this.jctxtSensibleHeat.Text), UnitType.POWER, utPower) : 0;
                curRoom.IsAirFlowEnable = this.uc_CheckBox_AirFlow.Checked;
                curRoom.AirFlow = curRoom.IsAirFlowEnable ? Unit.ConvertToSource(double.Parse(this.jctxtAirFlow.Text), UnitType.AIRFLOW, utAirflow) : 0;
                curRoom.IsStaticPressureEnable = this.uc_CheckBox_StaticPressure.Checked;
                //curRoom.StaticPressure = curRoom.IsStaticPressureEnable ? double.Parse(this.jctxtStaticPressure.Text) : 0;
                curRoom.StaticPressure = curRoom.IsStaticPressureEnable ? Unit.ConvertToSource(double.Parse(this.jctxtStaticPressure.Text), UnitType.STATICPRESSURE, utPressure) : 0;
                //curRoom.LoadIndexHeat = string.IsNullOrEmpty(this.jctxtLoadIndexHeat.Text) ? 0 :
                //    Unit.ConvertToSource(double.Parse(this.jctxtLoadIndexHeat.Text), UnitType.LOADINDEX, utLoadindex);
                curRoom.RqCapacityHeat = Unit.ConvertToSource(double.Parse(this.jctxtRqCapHeat.Text), UnitType.POWER, utPower);
                curRoom.FreshAirIndex = string.IsNullOrEmpty(this.jctxtIndexFA.Text) ? 0 :
                    Unit.ConvertToSource(double.Parse(this.jctxtIndexFA.Text), UnitType.AIRFLOW, utAirflow);
                curRoom.FreshAir = Unit.ConvertToSource(double.Parse(this.jctxtRqFA.Text), UnitType.AIRFLOW, utAirflow);
                return true;
            }
            return false;
        }

        // 拷贝选中的房间节点，生成新的房间，默认选中新添加的节点
        /// <summary>
        /// 拷贝选中的房间节点，生成新的房间，默认选中新添加的节点
        /// </summary>
        private void DoCopyAddRoom()
        {
            if (curRoom != null)
            {
                Room newRoom = projBll.CopyAddRoom(curRoom, curFloor);
                string namePrefix = SystemSetting.UserSetting.defaultSetting.RoomName;
                string _name = namePrefix + newRoom.NO.ToString();
                newRoom.Name = _name;

                curRoom = newRoom;
            }
            else
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
        }

        // 删除房间节点，删除后需清空该房间信息
        /// <summary>
        /// 删除房间节点，删除后需清空该房间信息
        /// </summary>
        private void DoDeleteRoom()
        {
            projBll.DeleteRoom(curRoom.Id, curFloor);
        }

        // 删除新风区域的房间节点
        /// <summary>
        /// 删除新风区域的房间节点
        /// </summary>
        private void DoDeleteFreshAirRoom()
        {
            projBll.SetFreshAirAreaForRoom(curRoom, null);
        }

        // 当更改 Load index 或 房间面积 数值时，容量需求自动计算，制冷工况
        /// <summary>
        /// 当更改 Load index 或 房间面积 数值时，容量需求自动计算，制冷工况
        /// </summary>
        private void CalRqCapacityCool()
        {
            if (this.JCValidateSingle(jctxtArea) && this.JCValidateSingle(jctxtLoadIndexCool))
            {
                string areaStr = this.jctxtArea.Text.Trim();
                string indexStr = this.jctxtLoadIndexCool.Text.Trim();
                if (!string.IsNullOrEmpty(areaStr) && !string.IsNullOrEmpty(indexStr))
                {
                    //2013-12-26 by Yang 增加LoadIndex和Area对不同单位Power的转换
                    //如果Area单位是FT2，需要先转换为M2
                    if (SystemSetting.UserSetting.unitsSetting.settingAREA == Unit.ut_Area_ft2)
                    {
                        areaStr = Unit.ConvertToSource(double.Parse(areaStr), UnitType.AREA, Unit.ut_Area_ft2.ToString()).ToString("n1");
                    }
                    //如果Power单位不是Kw，需要转换
                    if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_kw)
                    {
                        this.jctxtRqCapCool.Text = (double.Parse(areaStr) * double.Parse(indexStr) / 1000).ToString("n1");
                    }
                    else if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_ton)
                    {
                        this.jctxtRqCapCool.Text = (Unit.ConvertToControl(double.Parse(areaStr) * double.Parse(indexStr) / 1000, UnitType.POWER, Unit.ut_Capacity_ton.ToString())).ToString("n1");
                        //(Unit.ConvertToControl(double.Parse(areaStr), UnitType.POWER, Unit.ut_Capacity_ton.ToString()) * double.Parse(indexStr) / 1000).ToString("n1");  
                    }
                    else if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_btu)
                    {
                        this.jctxtRqCapCool.Text = (Unit.ConvertToControl(double.Parse(areaStr) * double.Parse(indexStr) / 1000, UnitType.POWER, Unit.ut_Capacity_btu.ToString())).ToString("n1");  
                    }                  
                }
            }
        }

        // 当更改 Load index 或 房间面积 数值时，容量需求自动计算，制热工况
        /// <summary>
        /// 当更改 Load index 或 房间面积 数值时，容量需求自动计算，制热工况
        /// </summary>
        private void CalRqCapacityHeat()
        {
            if (this.JCValidateSingle(jctxtArea) && this.JCValidateSingle(jctxtLoadIndexHeat))
            {
                string areaStr = this.jctxtArea.Text.Trim();
                string indexStr = this.jctxtLoadIndexHeat.Text.Trim();
                if (!string.IsNullOrEmpty(areaStr) && !string.IsNullOrEmpty(indexStr))
                {
                    //2013-12-26 by Yang 增加LoadIndex和Area对不同单位Power的转换
                    //如果Area单位是FT2，需要先转换为M2
                    if (SystemSetting.UserSetting.unitsSetting.settingAREA == Unit.ut_Area_ft2)
                    {
                        areaStr = Unit.ConvertToSource(double.Parse(areaStr), UnitType.AREA, Unit.ut_Area_ft2.ToString()).ToString("n1");
                    }
                    //如果Power单位不是Kw，需要转换
                    if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_kw)
                    {
                        this.jctxtRqCapHeat.Text = (double.Parse(areaStr) * double.Parse(indexStr) / 1000).ToString("n1");
                    }
                    else if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_ton)
                    {
                        this.jctxtRqCapHeat.Text = (Unit.ConvertToControl(double.Parse(areaStr), UnitType.POWER, Unit.ut_Capacity_ton.ToString()) * double.Parse(indexStr) / 1000).ToString("n1");
                    }
                    else if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_btu)
                    {
                        this.jctxtRqCapHeat.Text = (Unit.ConvertToControl(double.Parse(areaStr), UnitType.POWER, Unit.ut_Capacity_btu.ToString()) * double.Parse(indexStr) / 1000).ToString("n1");
                    }                  
                }
            }
        }

        // 当更改 FA index 或 房间人数 数值时，新风量需求自动计算
        /// <summary>
        /// 当更改 FA index 或 房间人数 数值时，新风量需求自动计算
        /// </summary>
        private void CalRqFA()
        {
            if (this.JCValidateSingle(jctxtNoOfPeople) && this.JCValidateSingle(jctxtIndexFA))
            {
                string peopleNum = this.jctxtNoOfPeople.Text.Trim();
                string indexStr = this.jctxtIndexFA.Text.Trim();
                if (!string.IsNullOrEmpty(peopleNum) && !string.IsNullOrEmpty(indexStr))
                {
                    this.jctxtRqFA.Text = (double.Parse(peopleNum) * double.Parse(indexStr)).ToString("n1");
                }
            }
        }

        // 更新指定房间关联的RoomIndoor的需求信息，当执行房间信息的 Update 命令时调用
        /// <summary>
        /// 更新指定房间关联的RoomIndoor的需求信息，当执行房间信息的 Update 命令时调用
        /// </summary>
        /// <param name="p"></param>
        private void UpdateIndoorRqInfo(string roomID)
        {
            List<RoomIndoor> list = projBll.GetSelectedIndoorByRoom(roomID);
            foreach (RoomIndoor ri in list)
            {
                ri.RqCoolingCapacity = curRoom.RqCapacityCool;
                ri.RqHeatingCapacity = curRoom.RqCapacityHeat;
                ri.RqSensibleHeat = curRoom.SensibleHeat;
                ri.RqAirflow = curRoom.AirFlow;
                ri.RqFreshAir = curRoom.FreshAir;
                ri.RoomName = curRoom.Name;
            }
        }


        // 更新指定房间关联的Exchanger的需求信息，当执行房间信息的 Update 命令时调用
        /// <summary>
        /// 更新指定房间关联的Exchanger的需求信息，当执行房间信息的 Update 命令时调用
        /// </summary>
        /// <param name="p"></param>
        private void UpdateExchangerRqInfo(string roomID)
        {
            List<RoomIndoor> list = projBll.GetSelectedExchangerByRoom(roomID);
            foreach (RoomIndoor ri in list)
            {
                ri.RqCoolingCapacity = curRoom.RqCapacityCool;
                ri.RqHeatingCapacity = curRoom.RqCapacityHeat;
                ri.RqSensibleHeat = curRoom.SensibleHeat;
                ri.RqAirflow = curRoom.AirFlow;
                ri.RqFreshAir = curRoom.FreshAir;
                ri.RoomName = curRoom.Name;
            }
        }

        // 取消指定房间与相关RoomIndoor的关联关系
        /// <summary>
        /// 取消指定房间与相关RoomIndoor的关联关系
        /// </summary>
        /// <param name="roomID"></param>
        private void ClearIndoorRoomID(string roomID)
        {
            List<RoomIndoor> list = projBll.GetSelectedIndoorByRoom(roomID);
            foreach (RoomIndoor ri in list)
            {
                ri.RoomID = "";
                ri.RoomName = "";
            }
        }


        // 取消指定房间与相关Exchanger的关联关系
        /// <summary>
        /// 取消指定房间与相关Exchanger的关联关系
        /// </summary>
        /// <param name="roomID"></param>
        private void ClearExchangerRoomID(string roomID)
        {
            List<RoomIndoor> list = projBll.GetSelectedExchangerByRoom(roomID);
            foreach (RoomIndoor ri in list)
            {
                ri.RoomID = "";
                ri.RoomName = "";
            }
        }

        #region 新风区域相关

        private void UpdateRoomInfoOfFreshAirArea()
        {
            if (curRoom == null || string.IsNullOrEmpty(curRoom.FreshAirAreaId)) return;

            foreach (TreeNode tnFreshAirRoot in tvFreshAirArea.Nodes)
            {
                foreach (TreeNode tnFreshAirArea in tnFreshAirRoot.Nodes)
                {
                    if (tnFreshAirArea.Tag == null) continue;
                    FreshAirArea area = tnFreshAirArea.Tag as FreshAirArea;
                    if (area.Id != curRoom.FreshAirAreaId)
                    {
                        continue;
                    }
                    foreach (TreeNode tnFreshAirRoom in tnFreshAirArea.Nodes)
                    {
                        if (tnFreshAirRoom.Tag == null) continue;
                        Room freshAirRoom = tnFreshAirRoom.Tag as Room;
                        if (freshAirRoom.Id != curRoom.Id)
                        {
                            continue;
                        }

                        //更新房间名
                        tnFreshAirRoom.Text = curRoom.Name;
                    }
                }
            }
        }

        private void CheckFreshAirAreaEnable()
        {
            //目前(20160625)只有马来西亚ProductType为“Comm. Tier 2, CO”时可选 FreshAir
            // 玻利维亚的York产品线和马来西亚的一样 20170629 by Yunxiao Lin
            bool enable = (thisProject.SubRegionCode == "MAL" || thisProject.SubRegionCode == "LA_BV") && thisProject.ProductType == "Comm. Tier 2, CO";

            jcbtnAddFreshAirArea.Enabled = enable;
            jcbtnUpdateFreshAirArea.Enabled = enable;
        }

        private void DeleteRoomFromFreshAirArea(string freshAirAreaId, string roomId)
        {
            foreach (TreeNode nodeLevel1 in tvFreshAirArea.Nodes)
            {
                foreach (TreeNode nodeLevel2 in nodeLevel1.Nodes)
                {
                    FreshAirArea area = nodeLevel2.Tag as FreshAirArea;
                    if (area.Id == freshAirAreaId)
                    {
                        foreach (TreeNode nodeLevel3 in nodeLevel2.Nodes)
                        {
                            Room room = nodeLevel3.Tag as Room;
                            if (room.Id == roomId)
                            {
                                nodeLevel3.Remove();
                                break;
                            }
                        }
                    }
                }
            }
        }

        // 检测是否已经包含当前新风区域所属的父节点
        /// <summary>
        /// 检测是否已经包含当前新风区域所属的父节点
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        private bool hasParentFreshAirAreaNode(int pId)
        {
            foreach (TreeNode tn in tvFreshAirArea.Nodes)
            {
                if (Convert.ToInt32(tn.Text) == pId)
                    return true;
            }
            return false;
        }

        // 绑定 FreshAirArea TreeView
        /// <summary>
        /// 绑定 FreshAirArea TreeView
        /// </summary>
        /// <param name="nodeLevel"> 需要展开的节点的层数 </param>
        private void BindTreeViewFreshAirArea(int nodeLevel)
        {
            this.tvFreshAirArea.Nodes.Clear();
            foreach (FreshAirArea area in thisProject.FreshAirAreaList)
            {
                if (!hasParentFreshAirAreaNode(area.ParentId))
                    tvFreshAirArea.Nodes.Add(area.ParentId.ToString());

                tvFreshAirArea.ExpandAll();
                foreach (TreeNode tn in tvFreshAirArea.Nodes)
                {
                    if (tn.Text == area.ParentId.ToString())
                    {
                        TreeNode nodeFreshAirArea = new TreeNode();
                        nodeFreshAirArea.Tag = area;
                        nodeFreshAirArea.Text = area.Name;
                        tn.Nodes.Add(nodeFreshAirArea);
                        nodeFreshAirArea.ForeColor = Color.Black;

                        if (nodeLevel == 1 && curFreshAirArea == area)                        
                        {
                            nodeFreshAirArea.EnsureVisible();
                            //tvFreshAirArea.SelectedNode = nodeFreshAirArea;
                            if (curFloor == null ||curRoom == null || (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_E" && thisProject.RegionCode != "EU_S"))    //EU特殊处理，防止初始节点                     
                                tvFreshAirArea.SelectedNode = nodeFreshAirArea;
                            nodeFreshAirArea.ForeColor = Color.Blue;                            
                          
                        }

                        foreach (Floor floor in thisProject.FloorList)
                        {
                            foreach (Room room in floor.RoomList)
                            {
                                if (room.FreshAirAreaId == area.Id)
                                {
                                    TreeNode nodeRoom = new TreeNode();
                                    nodeRoom.Tag = room;
                                    //nodeRoom.Text = room.Name;
                                    nodeRoom.Text = floor.Name + ":" + room.Name; //modify on 20160810 by Lingjia Qiu
                                    nodeFreshAirArea.Nodes.Add(nodeRoom);
                                    nodeRoom.ForeColor = Color.Black;

                                    if (nodeLevel == 2 && curRoom == room)
                                    {
                                        nodeRoom.EnsureVisible();
                                        tvFreshAirArea.SelectedNode = nodeRoom;
                                        nodeRoom.ForeColor = Color.Blue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (tvFreshAirArea.Nodes.Count == 0)
            {
                SetFreshAirAreaInfoState(false);
            }
            else if (curFreshAirArea == null)
            {
                tvFreshAirArea.Nodes[0].Expand();
                jctxtFreshAirAreaNo.Text = "";
                jctxtFreshAirAreaName.Text = "";
            }
        }

        // 改变新风区域信息区域的输入有效性
        /// <summary>
        /// 改变新风区域信息区域的输入有效性
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetFreshAirAreaInfoState(bool isEnable)
        {
            Color color = Color.FromArgb(130, 130, 130);
            if (!isEnable)
            {
                color = Color.FromArgb(130, 130, 130);
            }

            pnlContent_FreshAirArea_1.Enabled = isEnable;
            lblTitle_FreshAirAreaInfo.BackColor = color;

            jcbtnUpdateFreshAirArea.Enabled = isEnable;
        }


        // 添加新风区域节点
        /// <summary>
        /// 添加新风区域节点
        /// </summary>
        private void DoAddFreshAirArea()
        {
            // 创建新楼层对象或获取当前选中楼层对象，绑定到界面相应的控件
            BindFreshAirAreaSourceToControl(true);
            projBll.AddFreshAirArea(curFreshAirArea);
            curFreshAirAreaNode = null;
             
        }

        // 更新新风区域属性
        /// <summary>
        /// 更新新风区域属性
        /// </summary>
        private void DoUpdateFreshAirArea()
        {
            if (this.JCValidateGroup(pnlContent_FreshAirArea_1))
            {
                BindFreshAirAreaControlToSource();
            }
            
        }

        // 删除新风区域节点
        /// <summary>
        /// 删除新风区域节点
        /// </summary>
        private void DoDeleteFreshAirArea()
        {
            projBll.DeleteFreshAirArea(curFreshAirArea.Id);
            curFreshAirAreaNode = null;
        }

        // 拷贝选中的楼层，生成新楼层节点，默认选中新添加的节点
        /// <summary>
        /// 拷贝选中的楼层，生成新楼层节点，默认选中新添加的节点
        /// </summary>
        private void DoCopyAddFreshAirArea()
        {
            if (curFreshAirArea != null)
            {
                List<Floor> copyedFloors;
                FreshAirArea newArea = projBll.CopyAddFreshAirArea(curFreshAirArea, out copyedFloors);

                string namePrefix = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName; // 从 SettingConfig 获取
                string _name = namePrefix + newArea.NO.ToString();

                newArea.Name = _name;

                curFreshAirArea = newArea;

                foreach (Floor f in copyedFloors)
                {
                    namePrefix = SystemSetting.UserSetting.defaultSetting.FloorName; // 从 SettingConfig 获取
                    _name = namePrefix + f.NO.ToString();
                    f.Name = _name;
                }
            }
            else
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
        }

        // 绑定新风区域对象的属性到界面控件
        /// <summary>
        /// 绑定新风区域对象的属性到界面控件
        /// </summary>
        /// <param name="isAdd"></param>
        private void BindFreshAirAreaSourceToControl(bool isAdd)
        {
            if (isAdd)
            {
                curFreshAirArea = new FreshAirArea(projBll.GetNextFreshAirAreaNO());

                string namePrefix = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName; // 从 SettingConfig 获取
                string _name = namePrefix + curFreshAirArea.NO.ToString();

                curFreshAirArea.Name = _name;
            }
            else
            {
                curFreshAirArea = (FreshAirArea)curFreshAirAreaNode.Tag;
            }
            jctxtFreshAirAreaNo.Text = curFreshAirArea.NO.ToString();
            jctxtFreshAirAreaName.Text = curFreshAirArea.Name;
        }

        // 绑定界面控件值到当前选中的楼层对象
        /// <summary>
        /// 绑定界面控件值到当前选中的楼层对象
        /// </summary>
        private void BindFreshAirAreaControlToSource()
        {
            curFreshAirArea.Name = this.jctxtFreshAirAreaName.Text;
        }

        // 获取当前选中的节点 level, 并绑定当前选中的节点
        /// <summary>
        /// 获取当前选中的节点 level, 并绑定当前选中的节点
        /// </summary>
        /// <returns></returns>
        private int CheckSelectFreshAirAreaNode()
        {
            if (this.tvFreshAirArea.SelectedNode == null)
            {
                return -1;
            }
            int level = this.tvFreshAirArea.SelectedNode.Level;

            curFloor = null;
            curFloorNode = null;
            curRoom = null;
            curRoomNode = null;

            if (level == 1)
            {
                curFreshAirAreaNode = tvFreshAirArea.SelectedNode;
                curFreshAirArea = (FreshAirArea)curFreshAirAreaNode.Tag;
            }
            else if (level == 2)
            {
                curRoomNode = tvFreshAirArea.SelectedNode;
                curFreshAirAreaNode = curRoomNode.Parent;

                curRoom = (Room)curRoomNode.Tag;
                curFreshAirArea = (FreshAirArea)curFreshAirAreaNode.Tag;

                foreach (Floor f in thisProject.FloorList)
                {
                    foreach (Room r in f.RoomList)
                    {
                        if (r.Id == curRoom.Id)
                        {
                            curFloor = f;
                            break;
                        }
                    }
                }
            }
            else
                return 0;


            return level;
        }

        #endregion

        #endregion

        //重新设置SensibleHeat文本框的校验
        private void ResetSensibleHeatValidation()
        {
            bool isChecked = this.uc_CheckBox_SensibleHeat.Checked;
            this.jctxtSensibleHeat.Enabled = isChecked;
            this.jctxtSensibleHeat.AllowNull = !isChecked;
            this.jctxtSensibleHeat.RequireValidation = isChecked;
            this.jctxtSensibleHeat.JCValidationType = JCBase.Validation.ValidationType.NUMBER; 
            if (!isChecked)
                this.jctxtSensibleHeat.Text = "";
        }

        //重新设置AirFlow文本框的校验
        private void ResetAirFlowValidation()
        {
            bool isChecked = this.uc_CheckBox_AirFlow.Checked;
            this.jctxtAirFlow.Enabled = isChecked;
            this.jctxtAirFlow.AllowNull = !isChecked; 
            this.jctxtAirFlow.RequireValidation = isChecked;
            this.jctxtAirFlow.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            if (!isChecked)
                this.jctxtAirFlow.Text = "";
        }

        /// <summary>
        /// 重新设置StaticPressure文本框的校验
        /// </summary>
        private void ResetStaticPressureValidation()
        {
            bool isChecked = this.uc_CheckBox_StaticPressure.Checked;
            this.jctxtStaticPressure.Enabled = isChecked;
            this.jctxtStaticPressure.AllowNull = !isChecked;
            this.jctxtStaticPressure.RequireValidation = isChecked;
            this.jctxtStaticPressure.JCValidationType = JCBase.Validation.ValidationType.NUMBER;
            if (!isChecked)
                this.jctxtStaticPressure.Text = "";
        }

        private void tvRoom_Enter(object sender, EventArgs e)
        {
            ResetNodeColor(tvFreshAirArea.Nodes);
        }

        private void tvFreshAirArea_Enter(object sender, EventArgs e)
        {
            ResetNodeColor(tvRoom.Nodes);
        }

        private void jctxtHeight_TextChanged(object sender, EventArgs e)
        {
            ControllerInput(sender as TextBox); 
        }

        public void ControllerInput(object sender)
        {
            TextBox tx = sender as TextBox;
            if (NumberUtil.IsNumber(tx.Text))
            {
                if (tx.Text.Contains("-"))
                {
                    tx.Text = "";
                }
            }
            else
            {
                if (!tx.Text.Contains("."))
                {
                    tx.Text = "";
                }
                else {
                    if (tx.Text.Length > 0)
                    {
                        if (tx.Text.Substring(tx.Text.Length-1,1) == ".")
                        {
                            return;
                        }
                    }
                    
                    tx.Text = "";
                }

            }
        }

        private void jccmbRoomType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(curLocation))
                return;

            if (this.jccmbRoomType.Text.ToString() == Msg.ManagementRoom_Custom.ToString())
            {
                jctxtLoadIndexCool.Text = "0";
                jctxtLoadIndexHeat.Text = "0";
                jctxtRqCapCool.Text = "0";
                jctxtRqCapHeat.Text = "0";
                jctxtIndexFA.Text = "0";
                jctxtRqFA.Text = "0";
            }

            //if (curRoomType == this.jccmbRoomType.Text)
            //    return;
            curRoomType = this.jccmbRoomType.Text;

            RoomLoadIndex item = bll.GetRoomLoadIndexItem(curLocation, curRoomType);
            if (item != null)
            {
                this.jctxtLoadIndexCool.Text = Unit.ConvertToControl(item.CoolingIndex, UnitType.LOADINDEX, utLoadindex).ToString("n1");
                this.jctxtLoadIndexHeat.Text = Unit.ConvertToControl(item.HeatingIndex, UnitType.LOADINDEX, utLoadindex).ToString("n1");
            }
        }

        /// <summary>
        /// 绑定楼层信息
        /// </summary>
        private void bindFloorInfo()
        {
            if (!this.JCValidateGroup(pnlContent_Floor_1))
                return;

            if (curFloor == null)
                return;
            if (Convert.ToDecimal(jctxtHeight.Text) >= 0)
            {
                DoUpdateFloor();

            }
            else
            {
                JCMsg.ShowErrorOK(Msg.WARNING_TXT_POSITIVENUM(jclblHeight.Text));
                return;
            }
            BindTreeViewRoom(1);
            UndoRedoUtil.SaveProjectHistory();
        }

        /// <summary>
        /// 绑定新风区域信息
        /// </summary>
        private void bindFreshAirInfo()
        {
            if (!this.JCValidateGroup(pnlContent_FreshAirArea_1))
                return;

            if (curFreshAirArea == null)
                return;

            DoUpdateFreshAirArea();
            BindTreeViewFreshAirArea(1);
            UndoRedoUtil.SaveProjectHistory();

        }

        /// <summary>
        /// 绑定房间信息
        /// </summary>
        private bool bindRoomInfo()
        {
            if (curRoom == null)
                return true;

            ResetSensibleHeatValidation();
            ResetAirFlowValidation();
            ResetStaticPressureValidation();
            //if (!this.JCValidateGroup(pnlContent_2_Room))
            if (!this.JCValidateGroup(pnlContent_Room))
                return false;

            //增加Sensib Heat小于Total Capacity提示
            if (!checkSensible())
                return false;

            if (DoUpdateRoom())
            {
                // 检查该房间是否有关联的RoomIndoor记录
                UpdateIndoorRqInfo(curRoom.Id);
                // 检查该房间是否有关联的Exchanger记录
                UpdateExchangerRqInfo(curRoom.Id);
                BindTreeViewRoom(2);

                //更新新风区域节点下的房间节点
                if (curRoom.FreshAirAreaId != null)
                {
                    UpdateRoomInfoOfFreshAirArea();
                    BindTreeViewFreshAirArea(2);
                }
                UndoRedoUtil.SaveProjectHistory();
            }
            return true;
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            #region   EU地区特殊需求，在OK前确认绑定信息无误后才可关闭窗口   add on 20180711 by Vince
            if (!bindRoomInfo())
                return;
            #endregion
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void jctxtFloorInfo_Leave(object sender, EventArgs e)
        {
            if (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_E" && thisProject.RegionCode != "EU_S")
                return;
            //EU特殊逻辑，房间信息统一提交   --add on 20180518 by Vince
            if (curFloor != null)
                bindFloorInfo();
        }

        private void jctxtFreshAirAreaInfo_Leave(object sender, EventArgs e)
        {
            if (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_E" && thisProject.RegionCode != "EU_S")
                return;
            //EU特殊逻辑，房间信息统一提交   --add on 20180518 by Vince
            if (curFreshAirArea != null)
                bindFreshAirInfo();
        }

        private void jctxtRoomInfo_Leave(object sender, EventArgs e)
        {
            if (thisProject.RegionCode!="EU_W" && thisProject.RegionCode != "EU_E" && thisProject.RegionCode != "EU_S")
                return;
            //EU特殊逻辑，房间信息统一提交   --add on 20180518 by Vince
            if (curRoom != null)
                bindRoomInfo();
        }

        /// <summary>
        /// 校验Room中Sensible的容量   --add on 20180711 by Vince
        /// </summary>
        private bool checkSensible()
        {
            //增加Sensib Heat小于Total Capacity提示
            if ((jctxtSensibleHeat.Text != "") && (jctxtRqCapCool.Text != ""))
            {
                if (double.Parse(jctxtSensibleHeat.Text) > double.Parse(jctxtRqCapCool.Text))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(uc_CheckBox_SensibleHeat.TextString, "[" + jclblRqCapCool.Text + "]"));
                    return false;
                }
            }
            return true;
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmRoomManage_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoRedoUtil.Dispose();
        }
    }
}
