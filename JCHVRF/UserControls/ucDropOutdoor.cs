using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JCHVRF.Model;

namespace JCHVRF
{
    public partial class ucDropOutdoor : UserControl, IDropController
    {
        public ucDropOutdoor()
        {
            InitializeComponent();

            this.lvOutdoor.Columns[0].Width = 150;
            this.lvOutdoor.MultiSelect = false;
            this.lvOutdoor.AllowDrop = true;
            this.lvOutdoor.FullRowSelect = true;

            this.lvOutdoor.ItemDrag += new ItemDragEventHandler(lvOutdoor_ItemDrag);
            this.lvOutdoor.DragOver += new DragEventHandler(lvOutdoor_DragOver);
            this.lvOutdoor.DragDrop += new DragEventHandler(lvOutdoor_DragDrop);
            
            this.pbSetting.Click += new EventHandler(pbSetting_Click);
            this.pbSetting.MouseHover += new EventHandler(pbSetting_MouseHover);
            this.pbSetting.MouseLeave += new EventHandler(pbSetting_MouseLeave);

            this.pbRemove.Click += new EventHandler(pbRemove_Click);
            this.pbRemove.MouseHover += new EventHandler(pbRemove_MouseHover);
            this.pbRemove.MouseLeave += new EventHandler(pbRemove_MouseLeave);

            this.pbWarning.Click += new EventHandler(pbWarning_Click);
            this.pbWarning.MouseHover += new EventHandler(pbWarning_MouseHover);
            this.pbWarning.MouseLeave += new EventHandler(pbWarning_MouseLeave);
            SetInactive();
        }

        void lvOutdoor_ItemDrag(object sender, ItemDragEventArgs e)
        {
            (sender as ListView).DoDragDrop(e.Item, DragDropEffects.Move);
        }

        void lvOutdoor_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            if (UtilControl.CheckDragType_ListViewItem(e))
            {
                this.OnBeforeDrop(this.lvOutdoor, e);
            }

            string type = typeof(ListViewItem).ToString();
            if (e.Data.GetDataPresent(type, false))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        void lvOutdoor_DragDrop(object sender, DragEventArgs e)
        {
            this.OnAfterDrop(sender as ListView, e);
            UpdateQuantity();
        }


        void pbSetting_Click(object sender, EventArgs e)
        {
            SettingForm f = new SettingForm(this.Title);
            if (f.ShowDialog() == DialogResult.OK)
            {
                this.Title = f.SettingName;
                this.OnAfterSetting(e);
            }

        }

        void pbSetting_MouseHover(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Hand;
            pb.Image = JCHVRF.Properties.Resources.setting_White;
        }

        void pbSetting_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Default;
            pb.Image = JCHVRF.Properties.Resources.setting_Nor;
        }


        void pbRemove_Click(object sender, EventArgs e)
        {
            this.OnBeforeRemove(this.lvOutdoor, e);
            this.OnRemove(e);
        }

        void pbRemove_MouseHover(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Hand;
            pb.Image = JCHVRF.Properties.Resources.trash_can_white;
        }

        void pbRemove_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Default;
            pb.Image = JCHVRF.Properties.Resources.trash_can_nor;
            
        }


        void pbWarning_Click(object sender, EventArgs e)
        {
            this.OnWarning(e);
        }

        void pbWarning_MouseHover(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Hand;
            pb.Image = JCHVRF.Properties.Resources.remove_tab;
        }

        void pbWarning_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Default;
            pb.Image = JCHVRF.Properties.Resources.remove_nor;
        }


        #region IDropController

        public event DropEventHandler BeforeDrop;
        public event DropEventHandler AfterDrop;
        public event EventHandler BeforeRemove;

        public void OnBeforeDrop(object sender, DragEventArgs e)
        {
            if (BeforeDrop != null)
            {
                BeforeDrop(sender as ListView, e);
            }
        }

        public void OnAfterDrop(object sender, DragEventArgs e)
        {
            if (AfterDrop != null)
            {
                AfterDrop(sender as ListView, e);
            }
        }

        public void OnBeforeRemove(object sender, EventArgs e)
        {
            if (BeforeRemove != null)
            {
                BeforeRemove(sender as ListView, e);
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
        }


        public void SetInactive()
        {
            this.jclblTitle.ForeColor = Color.White;
            this.jclblTitle.BackColor = Color.FromArgb(189, 194, 198);
            this.BackColor = Color.FromArgb(220, 224, 225);

            this.pnlVisible.Visible = false;
            this._isActive = false;
        }

        public void SetActive()
        {
            this.jclblTitle.ForeColor = Color.White;
            this.jclblTitle.BackColor = Color.FromArgb(68, 92, 116);
            this.BackColor = Color.FromArgb(244, 233, 118);

            this.pnlVisible.Visible = true;
            this.pbWarning.Visible = false;

            this._isActive = true;

        }

        #endregion

        /// 设置标题变量之后
        /// <summary>
        /// 设置标题变量之后
        /// </summary>
        public event EventHandler AfterSetting;
        public void OnAfterSetting(EventArgs e)
        {
            if (AfterSetting != null)
            {
                AfterSetting(this, e);
            }
        }

        /// 点击删除图标之后触发的事件,删除所属的ControlGroup
        /// <summary>
        /// 点击删除图标之后触发的事件,删除所属的ControlGroup
        /// </summary>
        public event EventHandler Remove;
        public void OnRemove(EventArgs e)
        {
            if (Remove != null)
            {
                Remove(this, e);
            }
        }

        /// 点击警告图标之后触发的事件,点击删除所属的ControlGroup
        /// <summary>
        /// 点击警告图标之后触发的事件,点击删除所属的ControlGroup
        /// </summary>
        public event EventHandler Warning;
        public void OnWarning(EventArgs e)
        {
            if (Warning != null)
            {
                Warning(this, e);
            }
        }

        private string _errMsg;
        /// <summary>
        /// 点击Warning图标显示的信息
        /// </summary>
        public string ErrMsg
        {
            get { return _errMsg; }
        }

        /// 设置警告提示的内容
        /// <summary>
        /// 设置警告提示的内容
        /// </summary>
        /// <param name="msg"></param>
        public void SetErrMsg(string msg)
        {
            this._errMsg = msg;
        }

        private bool _isComplete;
        /// 只读属性，标记当前Outdoor所属的group是否有Controller相连接，判断Outdoor是否带面板时使用
        /// <summary>
        /// 只读属性，标记当前Outdoor所属的group是否有Controller相连接，判断Outdoor是否带面板时使用
        /// </summary>
        public bool IsComplete
        {
            get { return _isComplete; }
        }

        /// 当前控件为Active，所属的group有Controller相连时执行，且更新Outdoor List图标（带面板）
        /// <summary>
        /// 当前控件为Active，所属的group有Controller相连时执行，且更新Outdoor List图标（带面板）
        /// </summary>
        public void SetComplete()
        {
            SetActive();

            foreach (ListViewItem item in lvOutdoor.Items)
            {
                item.ImageIndex = 2;
            }
            this.pbWarning.Visible = false;
            this._isComplete = true;
        }

        /// 当前控件为Active，所属的group没有任何Controller相连时执行，且更新Oudoor List图标（不带面板）
        /// <summary>
        /// 当前控件为Active，所属的group没有任何Controller相连时执行，且更新Oudoor List图标（不带面板）
        /// </summary>
        public void SetIncomplete()
        {
            SetActive();

            this.BackColor = Color.FromArgb(255, 208, 208); // 浅红色背景

            foreach (ListViewItem item in lvOutdoor.Items)
            {
                item.ImageIndex = 0;
            }
            this.pbWarning.Visible = true;
            this._isComplete = false;
        }

        /// 拖入拖出室外机时，计算Outdoor,Indoor的数量
        /// <summary>
        /// 拖入拖出室外机时，计算Outdoor,Indoor的数量
        /// </summary>
        public void UpdateQuantity()
        {
            // Outdoor数量
           // this.jclblOutdoorCount.Text = this.lvOutdoor.Items.Count.ToString();
            // Outdoor数量去掉exchanger 的数量
            this.jclblOutdoorCount.Text = GetOutdoorRemoveExchangerQty().ToString();
            // Indoor数量
            int sum = GetIndoorQty();
            this.jclblIndoorCount.Text = sum.ToString();
            if (sum > 160)
            {
                this.jclblIndoor_DropOutdoor.ForeColor = Color.FromArgb(192, 0, 0);
                this.jclblIndoorCount.ForeColor = Color.FromArgb(192, 0, 0); // 红色字体
            }
            else
            {
                this.jclblIndoor_DropOutdoor.ForeColor = Color.FromArgb(68, 92, 116);
                this.jclblIndoorCount.ForeColor = Color.FromArgb(68, 92, 116);
            }
        }

        /// <summary>
        /// Outdoor数量去掉exchanger 的数量
        /// </summary>
        /// <returns></returns>
        public int GetOutdoorRemoveExchangerQty()
        {
            int sum = 0;
            foreach (ListViewItem item in this.lvOutdoor.Items)
            {
                if (!item.Name.Contains("Heat Exchanger"))
                {
                    sum++;
                } 
            }
            return sum;
        }

        /// 用于后台加载
        /// <summary>
        /// 用于后台加载
        /// </summary>
        /// <param name="item"></param>
        public void AddOutdoorItem(ListViewItem item)
        {
            this.lvOutdoor.Items.Add(item);
        }

        /// 获取当前ControlGroup中的室外机数量
        /// <summary>
        /// 获取当前ControlGroup中的室外机数量
        /// </summary>
        /// <returns></returns>
        public int GetOutdoorQty()
        {
            return this.lvOutdoor.Items.Count;
        }

        /// 获取当前ControlGroup中的室内机数量
        /// <summary>
        /// 获取当前ControlGroup中的室内机数量
        /// </summary>
        /// <returns></returns>
        public int GetIndoorQty()
        {
            int sum = 0;
            foreach (ListViewItem item in this.lvOutdoor.Items)
            {
                if (item.Tag is Int32)
                {
                    sum += Convert.ToInt32(item.Tag);
                } 
                else if (item.Tag is Object[])
                {
                    //取消exchanger 的数量
                    if (!item.Name.Contains("Heat Exchanger"))
                    {
                        Object[] values = item.Tag as Object[];
                        if (values != null && values.Length > 1 && values[1] is Int32)
                        {
                            sum += Convert.ToInt32(values[1]);
                        }
                    }
                } 

            }
            return sum;
        }

        /// <summary>
        /// 仅用于report
        /// </summary>
        /// <returns></returns>
        public ListView GetLVOutdoor()
        {
            return this.lvOutdoor;
        }

        /// <summary>
        /// 获得所有室外机的ProductType
        /// </summary>
        /// <returns></returns>
        public List<string> GetProductTypes()
        {
            return GetProductTypes(this.lvOutdoor.Items);
        }

        /// <summary>
        /// 获得所有室外机的ProductType
        /// </summary>
        /// <returns></returns>
        public List<string> GetProductTypesWith(ListViewItem newItem)
        {
            ArrayList items = new ArrayList();
            items.AddRange(this.lvOutdoor.Items);
            items.Add(newItem);
            return GetProductTypes(items);
        }

        /// <summary>
        /// 获得指定的一个或多个室外机的ProductType
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<string> GetProductTypes(ICollection items)
        {
            List<string> types = new List<string>();
            foreach (ListViewItem item in items)
            {
                if (item.Tag is Object[])
                {
                    Object[] values = item.Tag as Object[];
                    if (values != null && values.Length > 0 )
                    {

                        if (!item.Name.Contains("Heat Exchanger"))
                        {
                            SystemVRF system = values[0] as SystemVRF;
                            if (system.OutdoorItem != null)
                            {
                                types.Add(system.OutdoorItem.ProductType);
                            }
                        }
                        else {
                            RoomIndoor ri = values[0] as RoomIndoor;
                            if (ri.IndoorItem.Series != null)
                            {
                                types.Add(ri.IndoorItem.Series);
                            }
                        }
                    }
                }
            }
            return types;
        }

     
        /// <summary>
        /// 获取当前控制器里面包含的System 和exchanger 的数量
        /// </summary>
        /// <returns></returns>
        public DataTable GetOutdoorAndExchanger()
        {
            return GetOutdoorAndExchangers(this.lvOutdoor.Items);
        }

        /// <summary>
        /// 获取当前控制器里面包含的System 和exchanger 的数量
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DataTable GetOutdoorAndExchangers(ICollection items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SystemCount");
            dt.Columns.Add("ExchangerCount");
            int systemCount = 0;
            int exchangerCount = 0;
            foreach (ListViewItem item in items)
            {
                if (item.Tag is Object[])
                {
                    Object[] values = item.Tag as Object[];
                    if (values != null && values.Length > 0)
                    {
                        if (!item.Name.Contains("Heat Exchanger"))
                        {
                            systemCount++;
                        }
                        else
                        {
                            exchangerCount++;
                        }
                    }
                }
            }
            DataRow dr = dt.NewRow();
            dr["SystemCount"] = systemCount;
            dr["ExchangerCount"] = exchangerCount;
            dt.Rows.Add(dr);
            return dt;
        }



        /// <summary>
        /// 获取当前控制器里面包含的System 和exchanger 的数量
        /// </summary>
        /// <returns></returns>
        public DataTable GetExchangerTypesList()
        {
            return GetExchangerTypesLists(this.lvOutdoor.Items);
        }


        /// <summary>
        /// 获取当前控制器里面包含的System 和exchanger 的数量
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DataTable GetExchangerTypesLists(ICollection items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ExchangerType");
            dt.Columns.Add("Count");
            dt.Columns.Add("ProductType");
            DataTable dtItem = dt.Copy();
            foreach (ListViewItem item in items)
            {
                if (item.Tag is Object[])
                {
                    Object[] values = item.Tag as Object[];
                    if (values != null && values.Length > 0)
                    {
                        if (item.Name.Contains("Heat Exchanger"))
                        {
                            DataRow dr = dt.NewRow();
                            dr["ExchangerType"] = item.Name;
                            dr["Count"] = 1;
                            dr["ProductType"] = "Exchanger";
                            dt.Rows.Add(dr);
                        }
                        else {
                            DataRow dr = dt.NewRow();
                            dr["ExchangerType"] = item.Name;
                            dr["Count"] = 1;
                            dr["ProductType"] = "System";
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            
            if (dt.Rows.Count > 0)
            {
                DataRow[] rows =null;
                foreach (DataRow dr1 in dt.Rows)
                {
                    rows = dt.Select("ExchangerType='" + dr1["ExchangerType"].ToString() + "'");
                    if (rows.Length > 0)
                    {
                        DataRow drn = dtItem.NewRow();
                        drn["ExchangerType"] = rows[0][0];
                        drn["Count"] = rows.Length;
                        drn["ProductType"] = rows[0][2];
                        dtItem.Rows.Add(drn);
                    }
                }
            }
            DataView dv = new DataView(dtItem);
            DataTable dt2 = dv.ToTable(true, "ExchangerType", "Count", "ProductType");
            return dt2;
        }
    }
}
