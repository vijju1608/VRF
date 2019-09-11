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
using JCHVRF.Const;
using JCHVRF.VRFMessage;

namespace JCHVRF
{
    public partial class frmLoadIndex : JCBase.UI.JCForm
    {
        #region Initialization 初始化
        string lang = "";
        string errMsg = "";
        RoomLoadIndex LoadIndexItem;
        RoomLoadIndexBLL bll;
        string regionCode = "";
        string utLoadIndex;

        public frmLoadIndex()
        {
            InitializeComponent();
            lang = this.JCCurrentLanguage;
            _selectedItem = new RoomLoadIndex(lang);
            LoadIndexItem = new RoomLoadIndex(lang);
        }

        public frmLoadIndex(string region,string location, string roomtype)
        {
            InitializeComponent();
            lang = this.JCCurrentLanguage;
            _selectedItem = new RoomLoadIndex(lang);
            LoadIndexItem = new RoomLoadIndex(lang);
            this._location = location;
            this._roomtype = roomtype;
            regionCode = region;
        }

        private void frmLoadIndex_Load(object sender, EventArgs e)
        {
            this.jccmbLocation.DisplayMember = "LoadCity"; //
            bll = new RoomLoadIndexBLL();
            BindUnit();
            JCSetLanguage();
            BindData();
        }

        private void BindData()
        {
            this.dgvLoadIndex.AutoGenerateColumns = false;
            Global.SetDGVHeaderText(ref dgvLoadIndex, NameArray_LoadIndex.LoadIndex_HeaderText);
            Global.SetDGVDataName(ref dgvLoadIndex, NameArray_LoadIndex.LoadIndex_DataName);

            BindCityList();
        }

        private void BindCityList()
        {
            DataTable dt = bll.GetCityList();
            if (!string.IsNullOrEmpty(regionCode) && (regionCode=="EU_W" || regionCode == "EU_S" || regionCode == "EU_E"))
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "LoadCity not in('ShangHai','上海','BeiJing','北京','GuangZhou','广州')";
                dt = dv.ToTable();
            }
            if (dt.Rows.Count > 0)
            {
                this.jccmbLocation.DataSource = dt;
                this.jccmbLocation.Text = this._location;
            }
        }

        private void BindLoadIndexList(string city)
        {
            if (!string.IsNullOrEmpty(city))
            {
                // 从 SettingConfig 中读取当前选择的 LoadIndex 的单位表达式
                string utLoadIndex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;

                this.dgvLoadIndex.DataSource = bll.GetRoomLoadIndexList(city, utLoadIndex);

                foreach (DataGridViewRow r in dgvLoadIndex.Rows)
                {
                    if (r.Cells[0].Value.ToString() == _roomtype)
                    {
                        r.Selected = true;
                        // Add on 20130820 clh 定位到选中行
                        this.dgvLoadIndex.CurrentCell = r.Cells[0];
                        //break;
                    }
                }
            }
        }

        private void BindUnit()
        {
            utLoadIndex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
            jclblUnitLoadIndex.Text = utLoadIndex;
        }

        #endregion

        #region Controls events 控件事件

        private void jccmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindLoadIndexList(jccmbLocation.Text.Trim());
        }

        private void pbDefaultLocation_Click(object sender, EventArgs e)
        {
            if (CheckCitySelect())
                DoSetDefaultCity(jccmbLocation.Text);
        }

        private void pbAddLocation_Click(object sender, EventArgs e)
        {
            if (DoAddCity())
            {
                BindCityList();
            }
        }

        private void pbEditLocation_Click(object sender, EventArgs e)
        {
            if (CheckCitySelect() && DoEditCity(jccmbLocation.Text))
                BindCityList();
        }

        private void pbDeleteLocation_Click(object sender, EventArgs e)
        {
            if (CheckCitySelect())
            {
                DialogResult res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL);
                if (res == DialogResult.OK && DoDeleteCity(jccmbLocation.Text))
                    BindCityList();
            }
        }

        private void jcbtnDefaultIndex_Click(object sender, EventArgs e)
        {
            if (CheckIndexSelect())
            {
                string roomType = this.dgvLoadIndex.SelectedRows[0].Cells[0].Value.ToString();
                DoSetDefaultLoadIndex(this.jccmbLocation.Text, roomType);
            }
        }

        private void pbAddIndex_Click(object sender, EventArgs e)
        {
            DoAddIndex(this.jccmbLocation.Text);
        }

        private void pbEditIndex_Click(object sender, EventArgs e)
        {
            if (CheckIndexSelect())
            {
                RoomLoadIndex item = getSelectedRoomLoadIndexItem();
                DoEditIndex(item);
            }
        }

        private RoomLoadIndex getSelectedRoomLoadIndexItem()
        {
            string city = this.jccmbLocation.Text;
            string roomType = this.dgvLoadIndex.SelectedRows[0].Cells[0].Value.ToString();
            double value = Convert.ToDouble(this.dgvLoadIndex.SelectedRows[0].Cells[1].Value);
            double cIndex = value;
            value = Convert.ToDouble(this.dgvLoadIndex.SelectedRows[0].Cells[2].Value);
            double hIndex = value;

            RoomLoadIndex item = new RoomLoadIndex(city, roomType, lang);
            item.CoolingIndex = cIndex;
            item.HeatingIndex = hIndex;
            return item;
        }

        private void pbDeleteIndex_Click(object sender, EventArgs e)
        {
            if (CheckIndexSelect())
            {
                DialogResult res = JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL);
                if (res == DialogResult.OK)
                {
                    string roomType = this.dgvLoadIndex.SelectedRows[0].Cells[0].Value.ToString();
                    DoDeleteIndex(this.jccmbLocation.Text,roomType);
                }
            }
        }

        private void dgvLoadIndex_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (CheckIndexSelect())
                DoOK();
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (CheckIndexSelect())
                DoOK();
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion

        #region Response codes (Methods && Events) 事件响应代码
        /// <summary>
        /// 检测指定选项内容是否不为空
        /// </summary>
        /// <returns></returns>
        private bool CheckCitySelect()
        {
            if (string.IsNullOrEmpty(this.jccmbLocation.Text))
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检测当前 Load Index 选中项目是否不为空
        /// </summary>
        /// <returns></returns>
        private bool CheckIndexSelect()
        {
            if (this.dgvLoadIndex.SelectedRows.Count == 0)
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将当前城市设为默认城市
        /// </summary>
        private void DoSetDefaultCity(string city)
        {
            if (bll.SetDefaultCity(city, out errMsg))
                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
            else
                JCMsg.ShowErrorOK(errMsg);
        }

        /// <summary>
        /// 增加新城市
        /// </summary>
        private bool DoAddCity()
        {
            frmLoadIndexAddCity f = new frmLoadIndexAddCity();
            if (f.ShowDialog() == DialogResult.OK)
            {
                string city = f.CityName;
                int ret = bll.AddCity(city, out errMsg);
                if (ret == 1)
                {
                    this.jccmbLocation.Text = city;
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                    return true;
                }
                else if (ret == 0)
                    JCMsg.ShowWarningOK(errMsg);
                else
                    JCMsg.ShowErrorOK(errMsg);
            }
            return false;
        }

        /// <summary>
        /// 更改当前城市名称
        /// </summary>
        /// <param name="city"></param>
        private bool DoEditCity(string oldcity)
        {
            frmLoadIndexAddCity f = new frmLoadIndexAddCity(oldcity);
            if (f.ShowDialog() == DialogResult.OK)
            {
                string errMsg = "";
                string newcity = f.CityName;
                if (newcity == oldcity)
                    return false;
                int ret = bll.UpdateCity(newcity, oldcity, out errMsg);
                if (ret == 1)
                {
                    this.jccmbLocation.Text = newcity;
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                    return true;
                }
                else
                    JCMsg.ShowErrorOK(errMsg);
            }
            return false;
        }

        /// <summary>
        /// 删除当前选中城市，包括该城市下的所有数据
        /// </summary>
        /// <param name="city"></param>
        private bool DoDeleteCity(string city)
        {
            if (!bll.DeleteCity(city, out errMsg))
            {
                JCMsg.ShowErrorOK(errMsg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将当前 Load index 记录设为默认项
        /// </summary>
        private void DoSetDefaultLoadIndex(string city, string rType)
        {
            if (bll.SetDefaultRoomLoadIndex(city, rType, out errMsg))
            {
                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                BindLoadIndexList(city);
            }
            else
                JCMsg.ShowErrorOK(errMsg);
        }

        /// <summary>
        /// 增加新的 Load index 记录
        /// </summary>
        private void DoAddIndex(string city)
        {
            frmLoadIndexAddIndex f = new frmLoadIndexAddIndex(city);
            if (f.ShowDialog() == DialogResult.OK)
            {
                RoomLoadIndex newItem = f.LoadIndexItem;
                int index = bll.AddLoadIndex(newItem, out errMsg);
                if (index == 1)
                {
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                    BindLoadIndexList(jccmbLocation.Text.Trim());
                }
                else if(index == 0)
                    JCMsg.ShowErrorOK(Msg.ROOM_TYPE_IS_EXIST());
                else
                    JCMsg.ShowErrorOK(errMsg);
            }
        }

        /// <summary>
        /// 编辑当前选中的 Load index 记录
        /// </summary>
        /// <param name="roomType"></param>
        private void DoEditIndex(RoomLoadIndex item)
        {
            frmLoadIndexAddIndex f = new frmLoadIndexAddIndex(item);
            if (f.ShowDialog() == DialogResult.OK)
            {
                RoomLoadIndex newItem = f.LoadIndexItem;
                if (bll.UpdateLoadIndex(newItem, out errMsg) == 1)
                {
                    JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                    BindLoadIndexList(jccmbLocation.Text.Trim());
                }
                else
                    JCMsg.ShowErrorOK(errMsg);
            }
        }

        /// <summary>
        /// 删除当前选中的 Load index 记录
        /// </summary>
        /// <param name="roomType"></param>
        private void DoDeleteIndex(string city, string rType)
        {
            if (bll.DeleteLoadIndex(city, rType, out errMsg))
            {
                JCMsg.ShowInfoOK(JCMsg.INFO_SUCCESS);
                BindLoadIndexList(jccmbLocation.Text.Trim());
            }
            else
                JCMsg.ShowErrorOK(errMsg);
        }

        /// <summary>
        /// 返回当前选中的 Load index 记录
        /// </summary>
        private void DoOK()
        {
            // 返回选择的 Load index 记录
            _selectedItem = getSelectedRoomLoadIndexItem();
            DialogResult = DialogResult.OK;
        }

        private RoomLoadIndex _selectedItem;
        /// <summary>
        /// 当前选中的 RoomLoadIndex 项目
        /// </summary>
        public RoomLoadIndex SelectedItem
        {
            get { return _selectedItem; }
        }

        // 当前房间选择的地点与房间类型
        string _location;
        string _roomtype;

        #endregion

        private void dgvLoadIndex_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = Color.FromArgb(130, 130, 130);
            Pen pen_dgvBorder = new Pen(Color.FromArgb(127, 127, 127), 0.1f);

            DataGridView dgv = sender as DataGridView;

            if (e.RowIndex == -1)
            {
                SolidBrush brush = new SolidBrush(color_bg_dgvHeader);

                using (brush)
                {
                    e.Graphics.FillRectangle(brush, e.CellBounds);
                    Rectangle border = e.CellBounds;
                    border.X -= 1;
                    border.Y -= 1;
                    if (e.ColumnIndex == 0)
                    {
                        border.X += 1;
                    }
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

    }
}
