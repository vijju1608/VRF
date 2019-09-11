using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Util;
using Lassalle.Flow;
using System.Windows.Forms;
using JCHVRF.VRFMessage;
namespace JCHVRF
{
    public partial class frmNewAccessory : JCBase.UI.JCForm
    {
        Project thisProject;                                                   //当前项目
        AccessoryBLL abll = new AccessoryBLL();
        string RemoteControl = "Remote Control Switch";                        //控制器
        string ShareRemoteControl = "Share Remote Control";                    //共享控制器
        string HalfRemoteControl = "Half-size Remote Control Switch";
        string ReceiverRemoteControl = "Receiver Kit for Wireless Control";
        string WirelessRemoteControl = "Wireless Remote Control Switch";
        string ReceiverKit = "Receiver Kit";                                   //无线控制器
        string Panel = "Panel";                                                //面板
        string Filter = "Filter";                                              //过滤器
        string FilterBox = "Filter Box";                                       //过滤盒
        string DrainUp = "Drain-up";                                           //排水
        string Others = "Others";
        string DividingLine = "\n----------------------------------------\n";  //换行显示分割线
        string br = Environment.NewLine;                                       //换行符
        string BuiltInIndoor = "Built in at Indoor unit";                      //内置于室内机
        DataTable dtAccessorys;
        DataTable dtShareAccessory;
        DataTable AccessoryType;
        DataTable dtUniversal;
        int selectedIndoorId = 0;                                              //选中的室内机Id
        bool existsExchanger = false;                                          //是否是Exchanger
        string CurrentColumnName;                                              //当前列的名称
        int row = 0;                                                           //行所以
        int col = 0;                                                           //列索引



        List<SelectedIDU_ExchangerList> selIDUandExcList = new List<SelectedIDU_ExchangerList>(); //选中室内机与热交换机的集合
        private class SelectedIDU_ExchangerList
        {
            public int selected_IndoorId { get; set; }   //Indoor 或Exchanger 的Id
            public bool selected_IsExchanger { get; set; } //判断是否是Indoor
        }


        public frmNewAccessory(Project thisProj)
        {
            InitializeComponent();
            thisProject = thisProj;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmNewAccessory_Load(object sender, EventArgs e)
        {
            this.JCCallValidationManager = true; //启动验证
            int i = -1;                          //单元格行初始索引 
            InitData();                          //初始化数据 
            JCSetLanguage();                     //语言翻译
            CreateDataGridViewColumns();         //创建列 

            #region 绑定数据 
            //清除MainIndoor 下GROUP
            DoRemoveIndoorItemGroup();
            //清除室内机类型High Wall，High Wall (w/o EXV) 下Receiver Kit for Wireless Control 兼容老数据
            DoRemoveAccessoryReceiverKit();

            i = InitIndoorAndExchanger(thisProject.RoomIndoorList, i); //绑定室内机 
            i = InitIndoorAndExchanger(thisProject.ExchangerList, i);  //绑定热交换器

            ValidateAccessory();   //验证存在错误的配件
            #endregion
        }

        /// <summary>
        /// 删除High Wall，High Wall (w/o EXV)   Receiver Kit for Wireless Control数据
        /// </summary>
        private void DoRemoveAccessoryReceiverKit()
        {
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (IncludeHighWall(ri))
                {
                    ri.ListAccessory.RemoveAll(p => p.Type == ReceiverRemoteControl);
                }
            }
        }

        /// <summary>
        /// 清除MainIndoor下 不存在不匹配的室内机与热交换机
        /// </summary>
        private void DoRemoveIndoorItemGroup()
        {

            List<RoomIndoor> list = new List<RoomIndoor>();
            //室内机与热交换器集合
            if (thisProject.RoomIndoorList.Count > 0 || thisProject.ExchangerList.Count > 0)
            {
                List<RoomIndoor> rlist = thisProject.RoomIndoorList;
                List<RoomIndoor> elist = thisProject.ExchangerList;
                if (rlist != null && rlist.Count > 0)
                {
                    foreach (RoomIndoor ritem in rlist)
                    {
                        list.Add(ritem);
                    }
                }
                if (elist != null && elist.Count > 0)
                {
                    foreach (RoomIndoor ritem in elist)
                    {
                        list.Add(ritem);
                    }
                }
                //主Indoor 集合
                List<RoomIndoor> Mainlist = GetMainIndoorItems();
                foreach (RoomIndoor ri in Mainlist)
                {
                    if (ri.IndoorItemGroup != null)
                    {
                        bool isExists = true;
                        RoomIndoor indoor = null;
                        foreach (RoomIndoor r in ri.IndoorItemGroup)
                        {
                            RoomIndoor rom = list.Find(p => p.IndoorNO == r.IndoorNO && p.IsExchanger == r.IsExchanger && p.IndoorItem.ModelFull == r.IndoorItem.ModelFull);
                            if (rom == null)
                            {
                                indoor = r;
                                isExists = false;
                                break;
                            }
                        }
                        if (!isExists && indoor != null)
                        {
                            ri.IndoorItemGroup.Remove(indoor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化绑定RoomIndoor 和 Exchanger 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private int InitIndoorAndExchanger(List<RoomIndoor> list, int i)
        {
            foreach (RoomIndoor ri in list)
            {
                i = i + 1;
                dgvAvailableItems.Rows[i].Height = 38;
                string Model = (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
                dgvAvailableItems.Rows[i].Cells[0].Value = ri.IndoorNO;
                dgvAvailableItems.Rows[i].Cells[1].Value = ri.IsExchanger;
                dgvAvailableItems.Rows[i].Cells[2].Value = (new ProjectBLL(thisProject)).GetFloorAndRoom(ri.RoomID);
                dgvAvailableItems.Rows[i].Cells[3].Value = ri.IndoorName;
                dgvAvailableItems.Rows[i].Cells[4].Value = Model;
                for (int j = 5; j < dgvAvailableItems.Columns.Count; j++)
                {
                    ucDataGridViewCell uc = new ucDataGridViewCell();
                    uc.BeforeRemove += new System.EventHandler(FrmNewAccessory_BeforeRemove);
                    uc.BeforeValueChanged += new System.EventHandler(FrmNewAccessory_BeforeValueChanged);
                    uc.BeforeAdd += new System.EventHandler(FrmNewAccessory_BeforeAdd);
                    uc.BeforeShareRemove += new System.EventHandler(FrmNewAccessory_BeforeShareRemove);
                    dgvAvailableItems.Rows[i].Cells[j] = uc;
                }
                #region
                if (IsSharedRemoteControl(ri).Length > 0)
                {
                    dgvAvailableItems.Rows[i].Cells[6].Value = IsSharedRemoteControl(ri);
                }
                //合计Accessory 数量 相同的类型
                if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
                {
                    //不包含Share remote control
                    DataTable dtSelectedAccessory = ConvertAccessoryCount(ri.ListAccessory, Model, false, ri);
                    if (dtSelectedAccessory.Rows.Count > 0)
                    {
                        //得到类型出现的次数 计算行高
                        dgvAvailableItems.Rows[i].Height = GetRowHeight(dtSelectedAccessory);
                        foreach (DataRow dr in dtSelectedAccessory.Rows)
                        {
                            // 获取列的索引
                            int number = GetColumnsNameIndex(dr["AccessoryType"].ToString(), thisProject.BrandCode);
                            string ColumnsValue = dr["DisplayType"] + br + dr["AccessoryModel"] + " * " + dr["Qty"];
                            object ob = dgvAvailableItems.Rows[i].Cells[number].Value;
                            //填充数据
                            if (ob == null)
                            {
                                dgvAvailableItems.Rows[i].Cells[number].Value = ColumnsValue;
                            }
                            else
                            {
                                dgvAvailableItems.Rows[i].Cells[number].Value = dgvAvailableItems.Rows[i].Cells[number].Value + DividingLine + ColumnsValue;
                            }
                        }
                    }
                    DataTable dtShareAccessory = ConvertAccessoryCount(ri.ListAccessory, Model, true, ri);
                    if (dtShareAccessory.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtShareAccessory.Rows)
                        {
                            dgvAvailableItems.Rows[i].Cells[6].Value = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, dr["AccessoryType"].ToString(), dr["AccessoryModel"].ToString(), ri, true) + br + dr["AccessoryModel"];
                        }
                    }
                }
                #endregion  
                NotAvailble_AccessoryType(ri, i);  //没有可用的室内机类型
                RefreshHighWall_ReceiverKit(ri);
            }
            return i;
        }



        /// <summary>
        /// 清空High Wall,High Wall (w/o EXV)  ReceiverKit 对应的配件
        /// </summary>
        /// <param name="ri"></param>
        private void ClearHighWall_ReceiverKit(RoomIndoor ri)
        {
            if (ri == null) return;
            //判断当前室内机类型是否是High Wall,High Wall (w/o EXV)
            bool includeHighWall = false;
            for (int i = 0; i < HighWallType.Length; ++i)
            {
                if (ri.IndoorItem.Type == HighWallType[i])
                {
                    includeHighWall = true;
                    break;
                }
            }
            if (includeHighWall)
            {
                int rowIndex = GetRowIndex(ri);
                dgvAvailableItems.Rows[rowIndex].Cells[7].Value = "";
            }
        }

        //当前室内机是否包含Receiver Kit for Wireless Control
        private bool IncludeHighWall(RoomIndoor ri)
        {

            //判断当前室内机类型是否是High Wall,High Wall (w/o EXV)
            bool includeHigh = false;
            for (int i = 0; i < HighWallType.Length; ++i)
            {
                if (ri.IndoorItem.Type == HighWallType[i])
                {
                    includeHigh = true;
                    break;
                }
            }
            if (includeHigh)
            {
                //判断室内机配件是否有Receiver Kit for Wireless Control
                if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
                {
                    if (ri.ListAccessory.FindAll(p => p.Type == ReceiverRemoteControl).Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 如果Indoor类型是High Wall,High Wall (w/o EXV) 绑定Wireless Remote Control Switch 或者当前室内机被共享（Wireless Remote Control Switch）
        /// </summary>
        /// <param name="ri"></param>
        private void RefreshHighWall_ReceiverKit(RoomIndoor ri)
        {
            if (ri == null) return;
            //判断当前室内机类型是否是High Wall,High Wall (w/o EXV)
            bool IncludeHighWall = false;
            for (int i = 0; i < HighWallType.Length; ++i)
            {
                if (ri.IndoorItem.Type == HighWallType[i])
                {
                    IncludeHighWall = true;
                    break;
                }
            }
            if (!IncludeHighWall) return;//如果当前室内机不是High Wall,High Wall (w/o EXV) 直接返回
            bool IncludeWireless = false;
            //判断当前室内机是否Wireless Remote Control Switch 配件
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                foreach (Accessory item in ri.ListAccessory)
                {
                    if (item.Type == WirelessRemoteControl)
                    {
                        IncludeWireless = true;
                        break;
                    }
                }
            }

            //判断当前室内机是否被共享（WirelessRemoteControl）
            bool isShared = false;
            List<RoomIndoor> list = GetMainIndoorItems();
            foreach (RoomIndoor ind in list)
            {
                if (ind.ListAccessory.FindAll(p => p.Type == WirelessRemoteControl && p.IsShared == true).Count > 0)
                {
                    if (ind.IndoorItemGroup != null && ind.IndoorItemGroup.Count > 0)
                    {
                        foreach (RoomIndoor curInd in ind.IndoorItemGroup)
                        {
                            if (curInd.IndoorNO == ri.IndoorNO && curInd.IsExchanger == ri.IsExchanger)
                            {
                                isShared = true;
                                break;
                            }
                        }
                    }
                }
                if (isShared)
                {
                    break;
                }
            }
            int rowIndex = GetRowIndex(ri);
            if (IncludeHighWall && IncludeWireless || IncludeHighWall && isShared)
            {
                dgvAvailableItems.Rows[rowIndex].Cells[7].Value = BuiltInIndoor;
            }

        }

        /// <summary>
        /// 室内机配件 没有对应类型的列 设置颜色区分
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="row"></param>
        private void NotAvailble_AccessoryType(RoomIndoor ri, int row)
        {
            DataTable dt = GetAvailableColumns_ByType(ri);
            for (int i = 0; i < AccUnitType.Length; ++i)
            {
                bool isReadOnly = false;
                DataTable dta = GetAccessoryModelByGroup(AccUnitType[i]);
                foreach (DataRow drow in dta.Rows)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (drow["AccessoryType"].ToString() == dr["Type"].ToString())
                        {
                            isReadOnly = true;
                            break;
                        }
                    }
                }
                if (!isReadOnly)
                {
                    dgvAvailableItems.Rows[row].Cells[i + 7].ReadOnly = true;
                    dgvAvailableItems.Rows[row].Cells[i + 7].Style.BackColor = Color.FromArgb(235, 235, 235);
                }
            }
            if (ri.IndoorItem.Type == "Hydro Free-High temp." || ri.IndoorItem.Type == "Hydro Free-Low temp.")
            {
                dgvAvailableItems.Rows[row].Cells[6].ReadOnly = true;
                dgvAvailableItems.Rows[row].Cells[6].Style.BackColor = Color.FromArgb(235, 235, 235);
            }
        }


        /// <summary>
        /// 创建Accessory 列
        /// </summary> 
        private void CreateDataGridViewColumns()
        {
            //加载数据
            dgvAvailableItems.AutoGenerateColumns = true;
            dgvAvailableItems.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvAvailableItems.RowsDefaultCellStyle.BackColor = Color.White;
            dgvAvailableItems.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            //创建表头 固定列
            dgvAvailableItems.Columns.Add("Ind", "Ind");
            dgvAvailableItems.Columns.Add("IsExchanger", "IsExchanger");
            dgvAvailableItems.Columns.Add("Room", ShowText.Room);
            dgvAvailableItems.Columns.Add("Name", ShowText.Name);
            dgvAvailableItems.Columns.Add("Model", ShowText.Model);
            dgvAvailableItems.Columns.Add(RemoteControl, ShowText.Accessory_RemoteControlSwitch);
            dgvAvailableItems.Columns.Add(ShareRemoteControl, ShowText.Accessory_ShareRemoteController);
            dgvAvailableItems.Columns.Add(ReceiverKit, ShowText.Accessory_ReceiverKit);
            dgvAvailableItems.Columns.Add(Panel, ShowText.Accessory_Panel);
            dgvAvailableItems.Columns.Add(Filter, ShowText.Accessory_Filter);
            dgvAvailableItems.Columns.Add(FilterBox, ShowText.Accessory_FilterBox);
            dgvAvailableItems.Columns.Add(DrainUp, ShowText.Accessory_Drainup);
            dgvAvailableItems.Columns.Add(Others, ShowText.Accessory_Others);
            //创建动态列
            // DataGridView_AddColumns();
            //指定列不可编辑及列宽
            InitDataGridView_ColumnsWidth();
            //添加行
            int rcount = thisProject.RoomIndoorList.Count + thisProject.ExchangerList.Count;
            dgvAvailableItems.Rows.Add(rcount);
        }

        /// <summary>
        /// 返回当前单元格数据（不包含Share Remote Control）
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        public void CurrentCellValue(int row, int col)
        {
            bool isExchanger = false;
            dgvAvailableItems.Rows[row].Cells[col].Value = null;       //清楚单元格内的数据
            string IndoorNo = dgvAvailableItems.Rows[row].Cells[0].Value.ToString();
            isExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[row].Cells[1].Value);
            RoomIndoor ri = GetRoomIndoorDetial(isExchanger, Convert.ToInt32(IndoorNo));
            string columnsName = dgvAvailableItems.Columns[col].Name;
            string Model = (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                DataTable dtSelectedAccessory = ConvertAccessoryCount(ri.ListAccessory, Model, false, ri);
                DataTable dta = GetAccessoryModelByGroup(columnsName);
                string availableType = "";
                foreach (DataRow drow in dta.Rows)
                {
                    availableType += "'" + drow["AccessoryType"] + "',";
                }
                List<AccessoryItem> list = new List<AccessoryItem>();
                //筛选出当前类型的配件
                DataView dv = dtSelectedAccessory.DefaultView;
                if (!string.IsNullOrEmpty(availableType))
                {
                    dv.RowFilter = "AccessoryType in(" + availableType.TrimEnd(',') + ")";
                    dtSelectedAccessory = dv.ToTable();
                }
                if (dtSelectedAccessory.Rows.Count > 0)
                {
                    //得到类型出现的次数 计算行高
                    if (dgvAvailableItems.Rows[row].Height <= GetRowHeight(dtSelectedAccessory))
                        dgvAvailableItems.Rows[row].Height = GetRowHeight(dtSelectedAccessory);
                    foreach (DataRow dr in dtSelectedAccessory.Rows)
                    {
                        // 获取列的索引
                        int number = GetColumnsNameIndex(dr["AccessoryType"].ToString(), thisProject.BrandCode);
                        string ColumnsValue = dr["DisplayType"] + br + dr["AccessoryModel"] + " * " + dr["Qty"];
                        object ob = dgvAvailableItems.Rows[row].Cells[number].Value;
                        //填充数据
                        if (ob == null)
                        {
                            dgvAvailableItems.Rows[row].Cells[number].Value = ColumnsValue;
                        }
                        else
                        {
                            dgvAvailableItems.Rows[row].Cells[number].Value = dgvAvailableItems.Rows[row].Cells[number].Value + DividingLine + ColumnsValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 返回当前Share Remote Control单元格数据 
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        public void ShareCurrentCellValue(int row, int col)
        {
            string IndoorNo = dgvAvailableItems.Rows[row].Cells[0].Value.ToString();
            bool isExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[row].Cells[1].Value);
            RoomIndoor ri = GetRoomIndoorDetial(isExchanger, Convert.ToInt32(IndoorNo));
            if (ri == null) return;
            string columnsName = dgvAvailableItems.Columns[col].Name;
            string Model = (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi;
            dgvAvailableItems.Rows[row].Cells[col].Value = null;
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                DataTable dtSelectedAccessory = ConvertAccessoryCount(ri.ListAccessory, Model, true, ri);
                if (dtSelectedAccessory.Rows.Count > 0)
                {
                    //得到类型出现的次数 计算行高
                    dgvAvailableItems.Rows[row].Height = GetRowHeight(dtSelectedAccessory);
                    foreach (DataRow dr in dtSelectedAccessory.Rows)
                    {
                        // 获取列的索引 
                        string ColumnsValue = dr["AccessoryModel"].ToString();
                        dgvAvailableItems.Rows[row].Cells[col].Value = ColumnsValue;
                    }
                }
            }
        }


        /// <summary>
        /// 刷新Share Remote Control单元格数据 
        /// </summary>
        /// <param name="ri">室内机</param>
        public void ShareCurrentCellValue(RoomIndoor ri)
        {
            if (ri == null) return;
            if (ri.IsMainIndoor)
            {
                Accessory item = ri.ListAccessory.Find(p => p.IsShared == true);
                if (item == null) return;
                dgvAvailableItems.Rows[row].Cells[6].Value = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, item.Type, item.Model_Hitachi, ri, true) + br + ((thisProject.BrandCode == "Y") ? item.Model_York : item.Model_Hitachi);
                if (item.Type == WirelessRemoteControl)
                {
                    CurrentCellValue(row, 7);
                }
                RefreshHighWall_ReceiverKit(ri);
                if (ri.IndoorItemGroup != null)
                {
                    foreach (RoomIndoor ind in ri.IndoorItemGroup)
                    {
                        int rowIndex = GetRowIndex(ind);
                        dgvAvailableItems.Rows[rowIndex].Cells[6].Value = IsSharedRemoteControl(ind);
                        if (item.Type == WirelessRemoteControl)
                        {
                            CurrentCellValue(rowIndex, 7);
                            RefreshHighWall_ReceiverKit(ind);
                        }
                    }
                }
            }
            else
            {
                int rowIndex = GetRowIndex(ri);
                dgvAvailableItems.Rows[rowIndex].Cells[6].Value = IsSharedRemoteControl(ri);
            }
        }


        /// <summary>
        /// 选中行的集合
        /// </summary>
        private void SelectedRowsList()
        {
            selIDUandExcList.Clear();
            foreach (DataGridViewRow r in this.dgvAvailableItems.SelectedRows)
            {
                SelectedIDU_ExchangerList sel = new SelectedIDU_ExchangerList();
                sel.selected_IndoorId = Convert.ToInt32(r.Cells[0].Value.ToString());
                sel.selected_IsExchanger = Convert.ToBoolean(r.Cells[1].Value.ToString());
                selIDUandExcList.Add(sel);
            }

            //判断是否存在IndoorId
            bool isIndoorNo = true;
            if (selectedIndoorId > 0)
            {
                foreach (SelectedIDU_ExchangerList sel in selIDUandExcList)
                {
                    if (sel.selected_IndoorId == selectedIndoorId && sel.selected_IsExchanger == existsExchanger)
                    {
                        isIndoorNo = false;
                        break;
                    }
                }
                SelectedIDU_ExchangerList sels = new SelectedIDU_ExchangerList();
                sels.selected_IndoorId = selectedIndoorId;
                sels.selected_IsExchanger = existsExchanger;
                //判断是否需要填充进选中的室内机列表中
                if (isIndoorNo)
                {
                    selIDUandExcList.Add(sels);
                }
            }
        }

        /// <summary>
        /// 删除共享配件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FrmNewAccessory_BeforeShareRemove(object sender, EventArgs e)
        {
            if (JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL) == DialogResult.OK)
            {
                selectedIndoorId = Convert.ToInt32(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[0].Value.ToString());
                existsExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[1].Value);
                SelectedRowsList();
                if (selIDUandExcList.Count > 0)
                {
                    foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                    {
                        RoomIndoor ri = GetIDU_ExchangerDetial(item);
                        if (ri.IsMainIndoor)
                        {
                            if (MainIndoorIsShareWirless(ri))
                            {
                                ri.ListAccessory.RemoveAll(c => c.Type == ReceiverRemoteControl);
                                CurrentCellValue(GetRowIndex(ri), 7);
                            }
                            ri.ListAccessory.RemoveAll(c => c.IsShared == true);
                            ri.IsMainIndoor = false;
                            if (ri.IndoorItemGroup != null && ri.IndoorItemGroup.Count > 0)
                            {
                                foreach (RoomIndoor indItem in ri.IndoorItemGroup)
                                {
                                    int rows = GetRowIndex(indItem);
                                    dgvAvailableItems.Rows[rows].Cells[6].Value = null;
                                }
                                ri.IndoorItemGroup = new List<RoomIndoor>();
                            }

                        }
                        RoomIndoor mainIndoor = new RoomIndoor();
                        if (IsExistsMainIndoor(ri, out mainIndoor) && !ri.IsMainIndoor)
                        {
                            if (mainIndoor.IndoorNO > 0)
                            {
                                if (mainIndoor.ListAccessory.Find(p => p.IsShared == true).Type == WirelessRemoteControl)
                                {
                                    ri.ListAccessory.RemoveAll(p => p.Type == ReceiverRemoteControl);
                                    CurrentCellValue(GetRowIndex(ri), 7);
                                }
                                mainIndoor.IndoorItemGroup.Remove(ri);
                            }
                        }
                    }
                }
                foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                {
                    RoomIndoor ri = GetIDU_ExchangerDetial(item);
                    int rows = GetRowIndex(ri);
                    dgvAvailableItems.Rows[rows].Cells[6].Value = null;
                }
                EditCurrentCell();
                if (dgvAvailableItems.CurrentCell is ucDataGridViewCell)
                {
                    ucDataGridViewCell uc = dgvAvailableItems.CurrentCell as ucDataGridViewCell;
                    uc.BindDgvAccessory(dgvAvailableItems.CurrentCell.Tag as DgvColumnProperties);
                }
            }
        }

        /// <summary>
        /// 判断共享的主Indoor 是否是WirelessRemoteControl
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        private bool MainIndoorIsShareWirless(RoomIndoor ri)
        {
            bool isExistes = false;
            if (ri.IsMainIndoor)
            {
                if (ri.ListAccessory.FindAll(c => c.Type == WirelessRemoteControl && c.IsShared == true).Count > 0)
                {
                    return true;
                }
            }
            return isExistes;
        }

        /// <summary>
        /// 删除配件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FrmNewAccessory_BeforeRemove(object sender, EventArgs e)
        {
            string iduId = "0";
            string indoorType = string.Empty;
            string accessoryName = string.Empty;
            bool isDelete = false;
            int accessoryCount = 0;
            selectedIndoorId = Convert.ToInt32(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[0].Value.ToString());
            existsExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[1].Value);
            SelectedRowsList();
            //获取控件的属性值
            GetControlAttributes(sender, ref iduId, ref indoorType, ref accessoryName, ref accessoryCount);
            if (iduId != "0" && !string.IsNullOrEmpty(indoorType) && !string.IsNullOrEmpty(accessoryName))
            {
                RoomIndoor ri = GetRoomIndoorDetial(existsExchanger, Convert.ToInt32(iduId));
                if (ri == null) return;
                selIDUandExcList = FilterSelectedIDU(selIDUandExcList, indoorType); //过滤
                if (JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL) == DialogResult.OK)
                {
                    isDelete = RemoveAccessory(ri, accessoryName, dgvAvailableItems.Columns[dgvAvailableItems.CurrentCell.ColumnIndex].Name);
                    if (selIDUandExcList.Count > 1)
                    {
                        foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                        {
                            RoomIndoor ris = GetIDU_ExchangerDetial(item);
                            if (ris == null) return;
                            isDelete = RemoveAccessory(ris, accessoryName, dgvAvailableItems.Columns[dgvAvailableItems.CurrentCell.ColumnIndex].Name);
                            if (indoorType == ShareRemoteControl)
                            {
                                ShareCurrentCellValue(ris);
                            }
                        }
                    }
                    if (indoorType == ShareRemoteControl)
                    {
                        ShareCurrentCellValue(ri);
                    }
                }
            }

            //刷新当前的单元格  
            if (isDelete)
            {
                ucDgvAccessory ucController = (sender as ucDgvAccessory);
                ucController.Remove();
                //刷新所选中的室内机与热交换器 
                if (indoorType == ShareRemoteControl)
                {
                    ShareCurrentCellValue(dgvAvailableItems.CurrentCell.RowIndex, dgvAvailableItems.CurrentCell.ColumnIndex);
                }
                else
                {
                    CurrentCellValue(dgvAvailableItems.CurrentCell.RowIndex, dgvAvailableItems.CurrentCell.ColumnIndex);
                    RefreshRowsCells(selIDUandExcList);
                }

                if (selIDUandExcList.Count > 0)
                {
                    EditCurrentCell();
                    if (dgvAvailableItems.CurrentCell is ucDataGridViewCell)
                    {
                        ucDataGridViewCell uc = dgvAvailableItems.CurrentCell as ucDataGridViewCell;
                        uc.BindDgvAccessory(dgvAvailableItems.CurrentCell.Tag as DgvColumnProperties);
                    }
                }
            }
            ValidateAccessory();
        }

        /// <summary>
        /// 过滤选中的室内机
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<SelectedIDU_ExchangerList> FilterSelectedIDU(List<SelectedIDU_ExchangerList> list, string type)
        { 
            if (type == ShareRemoteControl)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    SelectedIDU_ExchangerList items = new SelectedIDU_ExchangerList();
                    items.selected_IndoorId = list[i].selected_IndoorId;
                    items.selected_IsExchanger = list[i].selected_IsExchanger;
                    RoomIndoor rindoor = GetIDU_ExchangerDetial(items);
                    if (!rindoor.IsMainIndoor)
                    {
                        list.RemoveAt(i);
                    }
                }
                return list;
            }

            if (IsController(type))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    SelectedIDU_ExchangerList items = new SelectedIDU_ExchangerList();
                    items.selected_IndoorId = list[i].selected_IndoorId;
                    items.selected_IsExchanger = list[i].selected_IsExchanger;
                    RoomIndoor rindoor = GetIDU_ExchangerDetial(items);
                    if (rindoor.IsMainIndoor || SharedRemoteControl(rindoor))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 判断类型是否是Controller 类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsController(string type)
        { 
            for (int j = 0; j < TypeLimit.Count; j++)
            {
                List<string> listi = TypeLimit[0];
                for (int m = 0; m < listi.Count; m++)
                {
                    if (type == listi[m].ToString())
                    {
                       return true; 
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 移除配件
        /// </summary>
        /// <param name="ri">室内机</param>
        /// <param name="accessoryName">配件名称</param>
        /// <param name="type">配件类型</param>
        /// <returns></returns>
        private bool RemoveAccessory(RoomIndoor ri, string accessoryName, string type)
        {
            string AssociatedType = "";
            foreach (Accessory acitem in ri.ListAccessory)
            {
                if (acitem.Model_Hitachi == accessoryName || acitem.Model_York == accessoryName)
                {
                    AssociatedType = acitem.Type;
                    break;
                }
            }
            //判断ri 是否是WirelessRemoteControl共享
            if (ri.IsMainIndoor)
            {
                // 判断共享的主Indoor 是否是WirelessRemoteControl
                if (MainIndoorIsShareWirless(ri) && type == ShareRemoteControl)
                {
                    AssociatedType = WirelessRemoteControl;
                }
            }

            bool isTrue = true;
            if (type == RemoteControl)
            {
                ri.ListAccessory.RemoveAll(c => c.Model_Hitachi == accessoryName && c.IsShared == false);
            }
            else if (type == ShareRemoteControl)
            {
                if (ri.IndoorItemGroup != null)
                {
                    foreach (RoomIndoor ind in ri.IndoorItemGroup)
                    {
                        int rowIndex = GetRowIndex(ind);
                        dgvAvailableItems.Rows[rowIndex].Cells[6].Value = "";
                        dgvAvailableItems.Rows[rowIndex].Cells[6].ReadOnly = false;
                    }
                }
                if (AssociatedType == WirelessRemoteControl && ri.IndoorItemGroup != null)
                {
                    foreach (RoomIndoor ind in ri.IndoorItemGroup)
                    {
                        RoomIndoor indoor = GetRoomIndoorDetial(ind);
                        if (indoor != null)
                        {
                            indoor.ListAccessory.RemoveAll(c => c.Type == ReceiverRemoteControl);
                        } 
                        CurrentCellValue(GetRowIndex(indoor), 7);
                    }

                    if (AssociatedType == WirelessRemoteControl)
                    {
                        ri.ListAccessory.RemoveAll(c => c.Type == ReceiverRemoteControl || c.Type == WirelessRemoteControl);
                        CurrentCellValue(GetRowIndex(ri), 7);
                    }
                } 
                ri.ListAccessory.RemoveAll(c => c.IsShared == true);
                ri.IsMainIndoor = false;
                ri.IndoorItemGroup = null;
            }
            if (type == ReceiverKit && AssociatedType == ReceiverRemoteControl)
            {
                if (ri.IsMainIndoor && ri.ListAccessory.FindAll(c => c.Type == ReceiverRemoteControl).Count > 0)
                {
                    ri.IsMainIndoor = false;
                    ri.ListAccessory.RemoveAll(c => c.Model_Hitachi == accessoryName);
                    if (ri.IndoorItemGroup != null)
                    {
                        foreach (RoomIndoor ind in ri.IndoorItemGroup)
                        {
                            RoomIndoor indoor = GetRoomIndoorDetial(ind);
                            if (indoor != null)
                                indoor.ListAccessory.RemoveAll(c => c.Type == ReceiverRemoteControl);

                            CurrentCellValue(GetRowIndex(indoor), 7);
                            ShareCurrentCellValue(indoor);
                        }
                    }
                    ri.IndoorItemGroup = null;
                    ShareCurrentCellValue(ri);
                }
                else
                {
                    ri.ListAccessory.RemoveAll(c => c.Type == ReceiverRemoteControl);
                    List<RoomIndoor> list = thisProject.RoomIndoorList.FindAll(c => c.IsMainIndoor == true);
                    if (list != null && list.Count > 0)
                    {
                        foreach (RoomIndoor inds in list)
                        {
                            if (inds.ListAccessory.FindAll(c => c.Type == WirelessRemoteControl && c.IsShared == true).Count > 0)
                            {
                                if (IsExisteIndoor(inds, ri))
                                {
                                    inds.IndoorItemGroup.Remove(ri);
                                }
                            }
                        }
                    }
                    ShareCurrentCellValue(ri);
                }
            }
            else
            {
                ri.ListAccessory.RemoveAll(c => c.Model_Hitachi == accessoryName);
            }
            // 删除联动选项
            if (!string.IsNullOrEmpty(AssociatedType))
            {
                string newType = GetNewTypeName(AssociatedType, ri);
                if (!string.IsNullOrEmpty(newType))
                {
                    Accessory newItem = abll.GetItem(newType, ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                    if (newItem == null)
                    {
                        ClearHighWall_ReceiverKit(ri);
                        return true;
                    }
                    if (thisProject.BrandCode == "Y")
                    {
                        ri.ListAccessory.RemoveAll(c => c.Model_York == newItem.Model_York);
                    }
                    else
                    {
                        ri.ListAccessory.RemoveAll(c => c.Model_Hitachi == newItem.Model_Hitachi);
                    }

                    if (AssociatedType == WirelessRemoteControl && newType == ReceiverRemoteControl)
                    {
                        ri.ListAccessory.RemoveAll(c => c.Type == ReceiverRemoteControl);
                    }
                    CurrentCellValue(dgvAvailableItems.CurrentCell.RowIndex, GetColumnsNameIndex(newType, thisProject.BrandCode));
                }
            }

            return isTrue;
        }


        /// <summary>
        /// 判断是否存在当前室内机是否存在于IndoorItemGroup
        /// </summary>
        /// <param name="List"></param>
        /// <param name="ri"></param>
        /// <returns></returns>
        private bool IsExisteIndoor(RoomIndoor List, RoomIndoor ri)
        { 
            if (List.IndoorItemGroup != null)
            {
                if (List.IndoorItemGroup.Count > 0)
                {
                    foreach (RoomIndoor r in List.IndoorItemGroup)
                    {
                        if (r.IndoorNO == ri.IndoorNO && r.IsExchanger == ri.IsExchanger)
                        {
                            return true; 
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 通过当前RoomIndoor 判断是 Indoor 还是Exchanger
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        private RoomIndoor GetRoomIndoorDetial(RoomIndoor ri)
        {
            RoomIndoor ind = ri;
            if (ri.IsExchanger)
                ind = thisProject.ExchangerList.Find(c => c.IndoorNO == ri.IndoorNO);
            else
                ind = thisProject.RoomIndoorList.Find(c => c.IndoorNO == ri.IndoorNO);
            return ind;
        }


        /// <summary>
        /// 通过当前RoomIndoor 判断是 Indoor 还是Exchanger
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        private RoomIndoor GetIDU_ExchangerDetial(SelectedIDU_ExchangerList item)
        {
            RoomIndoor ind = new RoomIndoor();
            if (item.selected_IsExchanger)
                ind = thisProject.ExchangerList.Find(p => p.IndoorNO == item.selected_IndoorId);
            else
                ind = thisProject.RoomIndoorList.Find(p => p.IndoorNO == item.selected_IndoorId);
            return ind;
        }


        /// <summary>
        /// 通过当前RoomIndoor 判断是 Indoor 还是Exchanger
        /// </summary> 
        /// <returns></returns>
        private RoomIndoor GetRoomIndoorDetial(bool isExc, int IndoorNo)
        {
            RoomIndoor ind = new RoomIndoor();
            if (isExc)
                ind = thisProject.ExchangerList.Find(p => p.IndoorNO == IndoorNo);
            else
                ind = thisProject.RoomIndoorList.Find(p => p.IndoorNO == IndoorNo);
            return ind;
        }

        /// <summary>
        /// 修改配件数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FrmNewAccessory_BeforeValueChanged(object sender, EventArgs e)
        {
            string indoorId = "0";
            string indoorType = string.Empty;
            string accessoryName = string.Empty;
            int accessoryCount = 0;
            selectedIndoorId = Convert.ToInt32(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[0].Value.ToString());
            existsExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[1].Value);
            SelectedRowsList();
            GetControlAttributes(sender, ref indoorId, ref indoorType, ref accessoryName, ref accessoryCount);
            if (indoorId != "0" && !string.IsNullOrEmpty(indoorType) && !string.IsNullOrEmpty(accessoryName))
            {
                bool isAddOrDel = true; //add 累加 del 减少
                string indSttr = string.Empty;
                List<SelectedIDU_ExchangerList> curSelInd = new List<SelectedIDU_ExchangerList>();
                //新增  
                if (AccessoryIsAddOrDelete(Convert.ToInt32(indoorId), accessoryName, accessoryCount))
                {
                    foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                    {
                        RoomIndoor ris = GetIDU_ExchangerDetial(item);
                        if (ris != null)
                        {
                            if (ris.ListAccessory != null && ris.ListAccessory.Count > 0)
                            {
                                List<Accessory> listItem = ris.ListAccessory.FindAll(p => p.Model_Hitachi == accessoryName);
                                if (listItem != null && listItem.Count >= accessoryCount)
                                {
                                    indSttr += ris.IndoorName + ",";
                                    curSelInd.Add(item);
                                }
                            }
                        }
                    }
                    isAddOrDel = true;
                }
                else
                {
                    isAddOrDel = false;
                    foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                    {
                        RoomIndoor ris = GetIDU_ExchangerDetial(item);
                        if (ris != null)
                        {
                            if (ris.ListAccessory != null && ris.ListAccessory.Count > 0)
                            {
                                List<Accessory> listItem = ris.ListAccessory.FindAll(p => p.Model_Hitachi == accessoryName);
                                if (listItem != null && listItem.Count <= accessoryCount)
                                {
                                    indSttr += ris.IndoorName + ",";
                                    curSelInd.Add(item);
                                }
                            }
                        }
                    }
                    //删除
                }
                if (!string.IsNullOrEmpty(indSttr))
                {
                    if (curSelInd.Count > 0)
                    {
                        foreach (SelectedIDU_ExchangerList sitem in curSelInd)
                        {
                            selIDUandExcList.Remove(sitem);
                        }
                    }
                    if (JCMsg.ShowConfirmOKCancel(Msg.ACCESSORY_UPDATE_MSG(indSttr.TrimEnd(','))) == DialogResult.OK)
                    {
                        AccessoryAddOrDel(indoorType, accessoryName, isAddOrDel); //添加
                    }
                }
                else
                {
                    AccessoryAddOrDel(indoorType, accessoryName, isAddOrDel);
                }
                RefreshRowsCells(selIDUandExcList);
            }

            //重新赋值单元格
            CurrentCellValue(dgvAvailableItems.CurrentCell.RowIndex, dgvAvailableItems.CurrentCell.ColumnIndex);
            EditCurrentCell();
            if (dgvAvailableItems.CurrentCell is ucDataGridViewCell)
            {
                ucDataGridViewCell uc = dgvAvailableItems.CurrentCell as ucDataGridViewCell;
                uc.BindDgvAccessory(dgvAvailableItems.CurrentCell.Tag as DgvColumnProperties);
            }
        }

        /// <summary>
        /// 批量新增或者批量删除
        /// </summary>
        /// <param name="indoorType">类型</param>
        /// <param name="accessoryName">配件名称</param>
        /// <param name="isAddOrDel">新增还是删除</param>
        private void AccessoryAddOrDel(string indoorType, string accessoryName, bool isAddOrDel)
        {
            if (isAddOrDel) //累加数量
            { 
                foreach (SelectedIDU_ExchangerList item in selIDUandExcList)  //for (int i = 0; i < selectedLists.Count; i++)
                {
                    RoomIndoor rindoor = GetIDU_ExchangerDetial(item);
                    if (rindoor != null)
                    {
                        List<Accessory> list = rindoor.ListAccessory.FindAll(p => p.Model_Hitachi == accessoryName && p.IsShared == false);
                        if (list.Count > 0)
                        {
                            if (!AddAccessoryCount(rindoor, indoorType, 1))
                            {
                                Accessory aci = list[0];
                                rindoor.ListAccessory.Add(aci);
                            }
                        }
                    }
                }
            }
            else //减少数量
            {  
                foreach (SelectedIDU_ExchangerList item in selIDUandExcList)  //for (int i = 0; i < selectedLists.Count; i++)
                {
                    RoomIndoor rindoor = GetIDU_ExchangerDetial(item);
                    if (rindoor != null)
                    {
                        List<Accessory> list = rindoor.ListAccessory.FindAll(p => p.Model_Hitachi == accessoryName && p.IsShared == false);
                        if (list != null && list.Count > 0)
                        {
                            foreach (Accessory ac in list)
                            {
                                if ((thisProject.BrandCode == "Y" ? ac.Model_York : ac.Model_Hitachi) == accessoryName)
                                {
                                    rindoor.ListAccessory.Remove(ac);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            ValidateAccessory();
        }


        /// <summary>
        /// 判断是新增 还是删除
        /// </summary>
        /// <param name="indoorId"></param>
        /// <param name="accessoryName"></param>
        /// <param name="accessoryCount"></param>
        /// <returns></returns>
        private bool AccessoryIsAddOrDelete(int indoorId, string accessoryName, int accessoryCount)
        {
            // true 新增 false 删除
            RoomIndoor ri = GetRoomIndoorDetial(existsExchanger, indoorId);
            if (ri == null) return true;
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                List<Accessory> list = null;
                if (thisProject.BrandCode == "Y")
                {
                    list = ri.ListAccessory.FindAll(p => p.Model_York == accessoryName);
                }
                else
                {
                    list = ri.ListAccessory.FindAll(p => p.Model_Hitachi == accessoryName);
                }
                if (list.Count > 0)
                {
                    //判断是新增 还是删除
                    if (list.Count > accessoryCount)
                    {
                        foreach (Accessory ac in list)
                        {
                            if ((thisProject.BrandCode == "Y" ? ac.Model_York : ac.Model_Hitachi) == accessoryName)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// 获取控件属性值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="indoorId"></param>
        /// <param name="indoorType"></param>
        /// <param name="accessoryName"></param>
        /// <param name="accessoryCount"></param>
        private static void GetControlAttributes(object sender, ref string indoorId, ref string indoorType, ref string accessoryName, ref int accessoryCount)
        {
            if (sender is ucDgvAccessory)
            {
                foreach (Control ctrl in (sender as ucDgvAccessory).Controls)
                {
                    if (ctrl.Name == "labId")
                    {
                        indoorId = ctrl.Text;
                    }
                    else if (ctrl.Name == "labType")
                    {
                        indoorType = ctrl.Text;
                    }
                    else if (ctrl.Name == "labaccessory")
                    {
                        accessoryName = ctrl.Text;
                    }
                    else if (ctrl.Name == "NudAccessory")
                    {
                        if (ctrl is NumericUpDown)
                        {
                            NumericUpDown nu = ctrl as NumericUpDown;
                            accessoryCount = Convert.ToInt32(nu.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加室内机配件匹配数量
        /// </summary>
        /// <param name="ri">室内机</param>
        /// <param name="indoorType">配件类型</param>
        /// <param name="number">配件数量</param>
        /// <returns></returns>
        private bool AddAccessoryCount(RoomIndoor ri, string indoorType, int number)
        {
            bool isTrue = false;
            List<Accessory> selectedlist = ri.ListAccessory.FindAll(p => p.Type == indoorType);
            if (selectedlist != null && selectedlist.Count > 0)
            {
                if (selectedlist[0].MaxNumber < selectedlist.Count + number)
                {
                    CurrentCellValue(dgvAvailableItems.CurrentCell.RowIndex, dgvAvailableItems.CurrentCell.ColumnIndex);
                    EditCurrentCell();
                    if (dgvAvailableItems.CurrentCell is ucDataGridViewCell)
                    {
                        ucDataGridViewCell uc = dgvAvailableItems.CurrentCell as ucDataGridViewCell;
                        uc.BindDgvAccessory(dgvAvailableItems.CurrentCell.Tag as DgvColumnProperties);
                    }
                    isTrue = true;
                    JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                }
            }
            return isTrue;
        }


        /// <summary>
        /// 过滤\N
        /// </summary>
        /// <param name="sttrItem"></param>
        /// <returns></returns>
        private string ArrayString(string sttrItem)
        {
            string outName = sttrItem;
            if (sttrItem.IndexOf('\n') > 0)
            {
                string[] sArray = sttrItem.Split('\n');
                outName = sArray[1];
            }
            return outName;
        }

        /// <summary>
        /// 添加配件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FrmNewAccessory_BeforeAdd(object sender, EventArgs e)
        {
            // selectedLists.Clear(); 
            //取得当前选中的室内机
            CurrentColumnName = dgvAvailableItems.Columns[dgvAvailableItems.CurrentCell.ColumnIndex].Name;
            selectedIndoorId = Convert.ToInt32(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[0].Value.ToString());
            existsExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[1].Value);
            SelectedRowsList();
            RoomIndoor ri = GetRoomIndoorDetial(existsExchanger, selectedIndoorId);
            if (ri == null) return;
            if (ri.ListAccessory == null)
            {
                ri.ListAccessory = new List<Accessory>();
            }
            if (CurrentColumnName == ShareRemoteControl)
            {
                //获取已选中室内机的集合
                ShowSharePanel();
                this.jccmbControlType.DataSource = null;
                this.jccmbControlType.Items.Clear();
                dtShareAccessory = GetShareAvailableAccessory(ri);
                this.jclabTitle.Text = Msg.SHARE_REMOTECONTROL + "（" + ri.IndoorName + "）";
                if (dtShareAccessory.Rows.Count > 0)
                {
                    //判断当前室内机是否已分配室外机
                    if (!string.IsNullOrEmpty(ri.SystemID))
                    {
                        //如果存在已共享的室内机
                        string indoors = null;
                        List<RoomIndoor> llist = thisProject.RoomIndoorList.FindAll(p => p.SystemID == ri.SystemID);
                        if (llist.Count > 0)
                        {
                            foreach (RoomIndoor rind in llist)
                            {
                                if (rind.IsMainIndoor == true)
                                    indoors += rind.IndoorNO + ",";
                            }
                        }
                        if (!string.IsNullOrEmpty(indoors))
                        {
                            DataView dv = dtShareAccessory.DefaultView;
                            dv.RowFilter = " OptionType='Edit' and IndoorNo in(" + indoors.TrimEnd(',') + ") or OptionType='Add' and IndoorNo in(" + ri.IndoorNO + ")";
                            dtShareAccessory = dv.ToTable();
                        }
                        else
                        {
                            //去掉其他已分配系统的MainIndoor 
                            List<RoomIndoor> olist = thisProject.RoomIndoorList.FindAll(p => p.SystemID != null && p.SystemID != "" && p.SystemID != ri.SystemID && p.IsMainIndoor == true);
                            if (olist.Count > 0)
                            {
                                string indlist = null;
                                foreach (RoomIndoor oind in olist)
                                {
                                    indlist += oind.IndoorNO + ",";
                                }
                                if (!string.IsNullOrEmpty(indlist))
                                {
                                    DataView dv = dtShareAccessory.DefaultView;
                                    dv.RowFilter = " IndoorNo not in(" + indlist.TrimEnd(',') + ") ";
                                    dtShareAccessory = dv.ToTable();
                                }
                            }
                        }
                    }
                    this.jccmbControlType.DisplayMember = "AccessoryModel";
                    this.jccmbControlType.ValueMember = "AccessoryModel";
                    this.jccmbControlType.DataSource = dtShareAccessory;
                    this.jccmbControlType.SelectedIndex = 0;
                    this.jccmbControlType.Text = dtShareAccessory.Rows[0]["AccessoryModel"].ToString();
                    //获取可用的RomoteControl 集合
                    GetAvailableShareRomoteControlList(dtShareAccessory.Rows[0]["AccessoryModel"].ToString(), ri);
                }
            }
            else
            {
                ShowPanel();
                this.jccmbAccessoryModel.DataSource = null;
                this.jccmbAccessoryModel.Items.Clear();
                dtAccessorys = GetAvailableAccessorys(ri, CurrentColumnName);
                //移除已选的配件
                DataView dv = dtAccessorys.DefaultView;
                //判断是否存在Share remote control
                bool isNotShare = false;
                if (ri != null && ri.ListAccessory != null && ri.ListAccessory.Count > 0)
                {
                    if (ri.ListAccessory.FindAll(p => p.IsShared == true && p.Type == RemoteControl || p.IsShared == true && p.Type == HalfRemoteControl || p.IsShared == true && p.Type == WirelessRemoteControl).Count == 1)
                    {
                        isNotShare = true;
                    }
                }
                if (!isNotShare)
                {
                    dv.RowFilter = "Selected_Qty=0";
                }
                dv.Sort = "AccessoryModel";
                dtAccessorys = dv.ToTable();
                if (dtAccessorys.Rows.Count > 0)
                {
                    dtAccessorys = GetAccessoryFilterWireless(ri, dtAccessorys, WirelessRemoteControl);
                    if (dtAccessorys.Rows.Count > 0)
                    {
                        this.jccmbAccessoryModel.DataSource = dtAccessorys;
                        this.jccmbAccessoryModel.DisplayMember = "AccessoryModel";
                        this.jccmbAccessoryModel.ValueMember = "AccessoryModel";
                        this.jccmbAccessoryModel.SelectedIndex = 0;
                        this.jccmbAccessoryModel.Text = dtAccessorys.Rows[0]["AccessoryModel"].ToString();
                        this.txtMaxNumber.Text = dtAccessorys.Rows[0]["Max_Qty"].ToString();
                        this.txtNumber.Text = "1";
                        this.txtNumber.JCMinValue = float.Parse("1");
                        this.txtNumber.JCMaxValue = float.Parse(this.txtMaxNumber.Text);
                        jclabModelTypeName.Text = dtAccessorys.Rows[0]["DisplayType"].ToString();
                        jclabModelType.Text = dtAccessorys.Rows[0]["AccessoryType"].ToString();
                    }
                }
            }
        }



        /// <summary>
        /// Accessory 过滤Wireless(RemoteControl,HalfRemoteControl)
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="dt"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private DataTable GetAccessoryFilterWireless(RoomIndoor ri, DataTable dt, string name)
        {
            if (ri.ListAccessory.FindAll(p => p.Type == RemoteControl || p.Type == HalfRemoteControl).Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "AccessoryType not in('" + name + "')";
                dt = dv.ToTable();
            }
            return dt;
        }



        /// <summary>
        /// 获取可用的ShareRomoteContro 集合
        /// </summary>
        /// <param name="accessName"></param>
        /// <param name="ri"></param>
        public void GetAvailableShareRomoteControlList(string accessName, RoomIndoor ri)
        {
            bool isAdd = true;
            cklistIndoor.Items.Clear();
            string systemId = ri.SystemID;
            isAdd = ShareOption(accessName, isAdd);
            if (isAdd)
                this.jcChkControl.Checked = true;
            else
                this.jcChkControl.Checked = false;

            List<RoomIndoor> cklist = GetAvailableShareRoomIndoorList(ShareRemoteControl, accessName);
            if (cklist.Count > 0)
            {
                cklist.Remove(ri); //移除自己 
                //如果当前室内机未分配 判断cklist 是否已存在SystemId
                if (!string.IsNullOrEmpty(systemId))
                    cklist.RemoveAll(c => c.SystemID != null && c.SystemID != systemId);
                //   cklist.RemoveAll(c => c.SystemID != null && c.SystemID != systemId || c.SystemID != "" && c.SystemID != systemId); 
                else
                    cklist = cklist.FindAll(c => c.SystemID == systemId || c.SystemID == null || c.SystemID == "");
                foreach (RoomIndoor roindoor in cklist)
                {
                    bool isFalse = false;
                    if (selIDUandExcList.Count > 0)
                    {
                        foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                        {
                            if (roindoor.IndoorNO == item.selected_IndoorId && roindoor.IsExchanger == item.selected_IsExchanger)
                            {
                                isFalse = true;
                                break;
                            }
                        }
                        if (isFalse)
                            // cklistIndoor.Items.Add(roindoor.IndoorName + " [" + (thisProject.BrandCode == "Y" ? roindoor.IndoorItem.Model_York : roindoor.IndoorItem.Model_Hitachi) + "] ", true);
                            cklistIndoor.Items.Add(new CheckBoxItem(roindoor.IndoorName + " [" + (thisProject.BrandCode == "Y" ? roindoor.IndoorItem.Model_York : roindoor.IndoorItem.Model_Hitachi) + "] ", roindoor.IndoorNO.ToString()), true);
                        else
                            //  cklistIndoor.Items.Add(roindoor.IndoorName + " [" + (thisProject.BrandCode == "Y" ? roindoor.IndoorItem.Model_York : roindoor.IndoorItem.Model_Hitachi) + "] ", false);
                            cklistIndoor.Items.Add(new CheckBoxItem(roindoor.IndoorName + " [" + (thisProject.BrandCode == "Y" ? roindoor.IndoorItem.Model_York : roindoor.IndoorItem.Model_Hitachi) + "] ", roindoor.IndoorNO.ToString()), false);
                    }
                }
            }
        }




        /// <summary>
        /// 是否可以共享Remote Control Switch
        /// </summary>
        /// <param name="accessName"></param>
        /// <param name="isAdd"></param>
        /// <returns></returns>
        private bool ShareOption(string accessName, bool isAdd)
        {
            if (dtShareAccessory.Rows.Count > 0)
            {
                foreach (DataRow dr in dtShareAccessory.Rows)
                {
                    if (dr["AccessoryModel"].ToString().Contains(accessName))
                    {
                        if (dr["OptionType"].ToString() != "Add")
                        {
                            isAdd = false;
                        }
                        break;
                    }
                }
            }
            return isAdd;
        }


        /// <summary>
        /// 获取可用编辑RemoteControl 对应的室内机Id
        /// </summary>
        /// <param name="accessName">配件名称</param>
        /// <returns></returns>
        public int GetEditShareRemoteControlIndoorNo(string accessName, out bool isExc)
        {
            int id = 0;
            isExc = false;
            if (dtShareAccessory.Rows.Count > 0)
            {
                foreach (DataRow dr in dtShareAccessory.Rows)
                {
                    if (dr["AccessoryModel"].ToString().Contains(accessName))
                    {
                        id = Convert.ToInt32(dr["IndoorNo"]);
                        isExc = Convert.ToBoolean(dr["IndoorType"]);
                        break;
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// 初始化Accessory的Accessory Type 
        /// </summary>
        /// <returns></returns>
        public void InitData()
        {
            AccessoryType = new DataTable();
            AccessoryType.Columns.Add("AccessoryGroup");
            AccessoryType.Columns.Add("AccessoryType");
            AccessoryType.Columns.Add("AccessoryModel");
            AccessoryType.Rows.Add(RemoteControl, "Remote Control Switch", "Y");
            AccessoryType.Rows.Add(RemoteControl, "Half-size Remote Control Switch", "Y");
            AccessoryType.Rows.Add(RemoteControl, "Wireless Remote Control Switch", "Y");
            AccessoryType.Rows.Add(RemoteControl, "Remote Control Switch", "H");
            AccessoryType.Rows.Add(RemoteControl, "Half-size Remote Control Switch", "H");
            AccessoryType.Rows.Add(RemoteControl, "Wireless Remote Control Switch", "H");
            AccessoryType.Rows.Add(RemoteControl, "Simplified remote controller", "H");
            AccessoryType.Rows.Add(RemoteControl, "Remote controller with timer", "H");
            AccessoryType.Rows.Add(ReceiverKit, "Receiver Kit for Wireless Control", "Y");
            AccessoryType.Rows.Add(ReceiverKit, "Receivet Kit", "H");
            AccessoryType.Rows.Add(ReceiverKit, "Receiver Kit for Wireless Control", "H");
            AccessoryType.Rows.Add(Panel, "Air Panel", "Y");
            AccessoryType.Rows.Add(Panel, "Motion Sensor Kit", "Y");
            AccessoryType.Rows.Add(Panel, "Air Panel", "H");
            AccessoryType.Rows.Add(Panel, "Motion Sensor Kit", "H");
            AccessoryType.Rows.Add(Panel, "Air Panel with elevating grill", "H");
            AccessoryType.Rows.Add(Filter, "Antibacterial Air Filter", "Y");
            AccessoryType.Rows.Add(Filter, "Air Filter", "Y");
            AccessoryType.Rows.Add(Filter, "Long-Life Filter Kit", "H");
            AccessoryType.Rows.Add(Filter, "Kit for Deodorant Filter (Deodorant Filter)", "H");
            AccessoryType.Rows.Add(Filter, "W Long-life Filter Kit", "H");
            AccessoryType.Rows.Add(Filter, "High efficiency filter", "H");
            AccessoryType.Rows.Add(Filter, "Antibacterial Long-life Filter", "H");
            AccessoryType.Rows.Add(Filter, "Primary Filter Kit + Activated Carbon Filter Kit", "H");
            AccessoryType.Rows.Add(Filter, "Air filter(G4) in Aluminium Frame", "H");
            AccessoryType.Rows.Add(Filter, "Air filter(G4) in Cardboard Frame", "H");
            AccessoryType.Rows.Add(Filter, "Activated Carbon Filter Kit", "H");
            AccessoryType.Rows.Add(Filter, "Deodorant filter", "H");
            AccessoryType.Rows.Add(Filter, "Long life filter", "H");
            AccessoryType.Rows.Add(Filter, "Econofresh kit", "H");
            AccessoryType.Rows.Add(FilterBox, "Filter Box", "Y");
            AccessoryType.Rows.Add(FilterBox, "Filter Box", "H");
            AccessoryType.Rows.Add(FilterBox, "Kit for Deodorant Filter (Filter Box)", "H");
            AccessoryType.Rows.Add(DrainUp, "Drain Pump Kit", "Y");
            AccessoryType.Rows.Add(DrainUp, "Drain-up Mechanism Kit", "Y");
            AccessoryType.Rows.Add(DrainUp, "Drain Pump", "Y");
            AccessoryType.Rows.Add(DrainUp, "Drain-up Mechanism Kit", "H");
            AccessoryType.Rows.Add(DrainUp, "Drain Pump", "H");
            AccessoryType.Rows.Add(DrainUp, "Drain-up kit for RPI-3.0~6.0FSN2SQ", "H");
            AccessoryType.Rows.Add(Others, "Grille fro Front Discharge", "Y");
            AccessoryType.Rows.Add(Others, "Electronic Expansion Valve Kit", "Y");
            AccessoryType.Rows.Add(Others, "Strainer Kit", "Y");
            AccessoryType.Rows.Add(Others, "Fresh Air Inlet Kit", "Y");
            AccessoryType.Rows.Add(Others, "3P Connector Cable", "Y");
            AccessoryType.Rows.Add(Others, "Duct Adapter", "Y");
            AccessoryType.Rows.Add(Others, "Remote Sensor", "Y");
            AccessoryType.Rows.Add(Others, "Air Outlet Shutter Plate", "Y");
            AccessoryType.Rows.Add(Others, "Relay and 3 Pin Connector Kit", "Y");
            AccessoryType.Rows.Add(Others, "T-Tube Connecting Kit", "Y");
            AccessoryType.Rows.Add(Others, "Remote Control Cable", "H");
            AccessoryType.Rows.Add(Others, "3P Connector Cable", "H");
            AccessoryType.Rows.Add(Others, "Remote Sensor", "H");
            AccessoryType.Rows.Add(Others, "Electronic Expansion Valve Kit", "H");
            AccessoryType.Rows.Add(Others, "Fresh Air Intake Kit", "H");
            AccessoryType.Rows.Add(Others, "T-Pipe Connection Kit", "H");
            AccessoryType.Rows.Add(Others, "Duct Adapter", "H");
            AccessoryType.Rows.Add(Others, "3-Way Outlet Parts Set", "H");
            AccessoryType.Rows.Add(Others, "Wireless ON/OFF thermostat (Receiver + Room thermostat)", "H");
            AccessoryType.Rows.Add(Others, "Wired Thermostat  PC-ARFWE for HYDRO FREE", "H");
            AccessoryType.Rows.Add(Others, "Kit for 4-20 mA application", "H");
            AccessoryType.Rows.Add(Others, "Unit controller cover", "H");
            AccessoryType.Rows.Add(Others, "2nd.outdoor temperature sensor", "H");
            AccessoryType.Rows.Add(Others, "Indoor wired room temperature sensor", "H");
            AccessoryType.Rows.Add(Others, "Universal water temperature sensor", "H");
            AccessoryType.Rows.Add(Others, "Hydraulic separator", "H");
            AccessoryType.Rows.Add(Others, "Aquastat security", "H");
            AccessoryType.Rows.Add(Others, "Domestic hot water tank", "H");
            AccessoryType.Rows.Add(Others, "3-way valve (Internal thread and spring return)", "H");
            AccessoryType.Rows.Add(Others, "Water check valve", "H");
            AccessoryType.Rows.Add(Others, "Noise damper", "H");
            AccessoryType.Rows.Add(Others, "Water electric heater", "H");
            AccessoryType.Rows.Add(Others, "Differential pressure overflow valve", "H");
            AccessoryType.Rows.Add(Others, "Flexible water pipe for a high temperature HYDRO FREE", "H");
            AccessoryType.Rows.Add(Others, "Differential pressure overflow valve", "H");
            AccessoryType.Rows.Add(Others, "Cooling operation kit", "H");
            AccessoryType.Rows.Add(Others, "2nd zone mixing kit (Wall mounted model)", "H");
            AccessoryType.Rows.Add(Others, "Inlet change accessory", "H");
            AccessoryType.Rows.Add(Others, "Duct connecting flange for outdoor air outlet", "H");
            AccessoryType.Rows.Add(Others, "Electrical Dust Collecting Unit", "H");
            dtUniversal = abll.GetUniversal_ByAccessory(thisProject.SubRegionCode, thisProject.BrandCode);
        }

        /// <summary>
        /// 获取可共享的Remote Control Switch
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public DataTable GetShareAvailableAccessory(RoomIndoor ri)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IndoorNo");
            dt.Columns.Add("AccessoryType");
            dt.Columns.Add("AccessoryModel");
            dt.Columns.Add("OptionType");
            dt.Columns.Add("IndoorType");
            //判断当前室内机可进行共享
            if (ri.ListAccessory != null)
            {
                if (ri.ListAccessory.FindAll(p => p.Type == RemoteControl).Count >= 2)
                {
                    return dt;
                }
            }
            string RemoteControlItem = "";
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
            //可新增的Remote Control Switch
            DataTable AccessoryList = abll.GetInDoorAccessoryItemList(thisProject.BrandCode, fCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity, thisProject.RegionCode, ri, thisProject.SubRegionCode);
            if (AccessoryList.Rows.Count > 0)
            {
                DataView dv = AccessoryList.DefaultView;
                // 可共享的Controller (Remote Control Switch,Half-size Remote Control Switch,WirelessRemoteControl)
                dv.RowFilter = "Type in('" + RemoteControl + "','" + HalfRemoteControl + "','" + WirelessRemoteControl + "')";
                AccessoryList = dv.ToTable();
                if (AccessoryList.Rows.Count > 0)
                {
                    foreach (DataRow dr in AccessoryList.Rows)
                    {
                        DataRow row = dt.NewRow();
                        RemoteControlItem += (thisProject.BrandCode == "Y" ? dr["Model_York"].ToString() : dr["Model_Hitachi"].ToString()) + ",";
                        row["IndoorNo"] = ri.IndoorNO;
                        row["AccessoryType"] = dr["Type"].ToString();
                        row["AccessoryModel"] = (thisProject.BrandCode == "Y" ? dr["Model_York"].ToString() : dr["Model_Hitachi"].ToString());
                        row["OptionType"] = "Add";
                        row["IndoorType"] = ri.IsExchanger;
                        dt.Rows.Add(row);
                    }
                }
            }

            //获取已共享的室内机
            List<RoomIndoor> list = thisProject.RoomIndoorList.FindAll(p => p.IsMainIndoor == true);
            if (list != null)
            {
                foreach (RoomIndoor indoor in list)
                {
                    if (indoor.IndoorItemGroup != null && indoor.IndoorItemGroup.Count > 0)
                    {
                        //判断RemoteControl，HalfRemoteControl 是否等于15 
                        //判断WirelessRemoteControl 是否等于3
                        if (indoor.ListAccessory.Find(p => p.IsShared == true).Type == RemoteControl || indoor.ListAccessory.Find(p => p.IsShared == true).Type == HalfRemoteControl)
                        {
                            if (indoor.IndoorItemGroup.Count == 15)
                                continue;
                        }
                        if (indoor.ListAccessory.Find(p => p.IsShared == true).Type == WirelessRemoteControl)
                        {
                            if (indoor.IndoorItemGroup.Count == 3)
                                continue;
                        }
                    }

                    if (indoor.ListAccessory != null)
                    {
                        foreach (Accessory item in indoor.ListAccessory)
                        {
                            string modelename = (thisProject.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi);
                            if (RemoteControlItem.Length > 0)
                            {
                                if (RemoteControlItem.Contains(modelename) && item.IsShared == true)
                                {
                                    if (item.Type == RemoteControl || item.Type == HalfRemoteControl || item.Type == WirelessRemoteControl)
                                    {
                                        DataRow row = dt.NewRow();
                                        row["IndoorNo"] = indoor.IndoorNO;
                                        row["AccessoryType"] = RemoteControl;
                                        row["AccessoryModel"] = modelename + "(" + indoor.IndoorName + ")";
                                        row["OptionType"] = "Edit";
                                        row["IndoorType"] = indoor.IsExchanger;
                                        dt.Rows.Add(row);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            List<RoomIndoor> excList = thisProject.ExchangerList.FindAll(p => p.IsMainIndoor == true);
            if (excList != null)
            {
                foreach (RoomIndoor indoor in excList)
                {
                    if (indoor.ListAccessory != null)
                    {
                        if (indoor.IndoorItemGroup != null && indoor.IndoorItemGroup.Count > 0)
                        {
                            //判断RemoteControl，HalfRemoteControl 是否等于15 
                            //判断WirelessRemoteControl 是否等于3
                            if (indoor.ListAccessory.Find(p => p.IsShared == true).Type == RemoteControl || indoor.ListAccessory.Find(p => p.IsShared == true).Type == HalfRemoteControl)
                            {
                                if (indoor.IndoorItemGroup.Count == 15)
                                    break;
                            }
                            if (indoor.ListAccessory.Find(p => p.IsShared == true).Type == WirelessRemoteControl)
                            {
                                if (indoor.IndoorItemGroup.Count == 3)
                                    break;
                            }
                        }
                        foreach (Accessory item in indoor.ListAccessory)
                        {
                            string modelename = (thisProject.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi);
                            if (RemoteControlItem.Length > 0)
                            {
                                if (RemoteControlItem.Contains(modelename) && item.IsShared == true)
                                {
                                    if (item.Type == RemoteControl || item.Type == HalfRemoteControl || item.Type == WirelessRemoteControl)
                                    {
                                        DataRow row = dt.NewRow();
                                        row["IndoorNo"] = indoor.IndoorNO;
                                        row["AccessoryType"] = RemoteControl;
                                        row["AccessoryModel"] = modelename + "(" + indoor.IndoorName + ")";
                                        row["OptionType"] = "Edit";
                                        row["IndoorType"] = indoor.IsExchanger;
                                        dt.Rows.Add(row);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                dv.Sort = "OptionType asc,AccessoryModel asc";
                return dv.ToTable();
            }
            return dt;
        }

        /// <summary>
        /// 获取可用共享的室内机集合
        /// </summary>
        /// <param name="AccessoryType"></param>
        /// <param name="AccessoryModel"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetAvailableShareRoomIndoorList(string AccessoryType, string AccessoryModel)
        { 
            if (AccessoryModel.Contains("(") && AccessoryModel.Contains(")"))
            {
                int indexs = AccessoryModel.IndexOf("(");
                AccessoryModel = AccessoryModel.Substring(0, indexs);
            }
            List<RoomIndoor> list = new List<RoomIndoor>();
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
                string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
                DataTable AccessoryList = abll.GetShareAccessoryItemList(thisProject.BrandCode, fCode, ri.IndoorItem.Type, RemoteControl, ri.IndoorItem.CoolingCapacity, thisProject.RegionCode, thisProject.SubRegionCode);
                if (AccessoryList != null && AccessoryList.Rows.Count > 0)
                {
                    foreach (DataRow dr in AccessoryList.Rows)
                    {
                        if (dr["Model_Hitachi"].ToString() == AccessoryModel)
                        {
                            //判断是否可以共享
                            if (IsShareRemoteControl(ri) && ri.ListAccessory.FindAll(p => p.Type == RemoteControl || p.Type == HalfRemoteControl || p.Type == WirelessRemoteControl).Count == 0)
                            {
                                list.Add(ri);
                            }
                            break;
                        }
                    }
                }
            }
            foreach (RoomIndoor ri in thisProject.ExchangerList)
            {
                // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
                string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
                DataTable AccessoryList = abll.GetShareAccessoryItemList(thisProject.BrandCode, fCode, ri.IndoorItem.Type, RemoteControl, ri.IndoorItem.CoolingCapacity, thisProject.RegionCode, thisProject.SubRegionCode);
                if (AccessoryList != null && AccessoryList.Rows.Count > 0)
                {
                    foreach (DataRow dr in AccessoryList.Rows)
                    {
                        if (dr["Model_Hitachi"].ToString() == AccessoryModel)
                        {
                            //判断是否可以共享
                            if (IsShareRemoteControl(ri) && ri.ListAccessory.FindAll(p => p.Type == RemoteControl || p.Type == HalfRemoteControl || p.Type == WirelessRemoteControl).Count == 0)
                            {
                                list.Add(ri);
                            }
                            break;
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取RoomIndoor 与 Exchanger 共享的集合
        /// </summary>
        /// <returns></returns>
        private List<RoomIndoor> GetMainIndoorItems()
        {
            List<RoomIndoor> list = new List<RoomIndoor>();
            list = thisProject.RoomIndoorList.FindAll(p => p.IsMainIndoor == true);
            List<RoomIndoor> excList = thisProject.ExchangerList.FindAll(p => p.IsMainIndoor == true);
            if (excList != null && excList.Count > 0)
            {
                foreach (RoomIndoor ritem in excList)
                {
                    list.Add(ritem);
                }
            }
            return list;
        }

        /// <summary>
        /// 返回已共享的室内机
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public string IsSharedRemoteControl(RoomIndoor ri)
        {
            string IndoorName = "";
            if (ri.ListAccessory != null)
            {
                List<RoomIndoor> list = GetMainIndoorItems();
                foreach (RoomIndoor ind in list)
                {
                    Accessory item = ind.ListAccessory.Find(p => p.IsShared == true);
                    if (item == null)
                    {
                        return IndoorName;
                    }
                    string accessoryName = (thisProject.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi);
                    if (ind.IndoorItemGroup != null && !ri.IsMainIndoor)
                    {
                        RoomIndoor listInd = ind.IndoorItemGroup.Find(p => p.IndoorNO == ri.IndoorNO && p.IsExchanger == ri.IsExchanger);
                        if (listInd != null)
                        {
                            if (!string.IsNullOrEmpty(accessoryName))
                            {
                                IndoorName = ind.IndoorName + "(" + accessoryName + ")";
                            }
                            break;
                        }
                    }
                }
            }
            return IndoorName;
        }

        /// <summary>
        /// 当前室内机是否是主Indoor
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public bool IsMainIndoorByShared(RoomIndoor ri)
        { 
            List<RoomIndoor> list = GetMainIndoorItems();
            if (list.Count > 0)
            {
                if (list.FindAll(p => p.IndoorNO == ri.IndoorNO && p.IsExchanger == ri.IsExchanger).Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断当前室内机是否已共享
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public bool SharedRemoteControl(RoomIndoor ri)
        { 
            List<RoomIndoor> list = GetMainIndoorItems();
            foreach (RoomIndoor ind in list)
            {
                Accessory item = ind.ListAccessory.Find(p => p.IsShared == true);
                if (item == null) return false;
                if (ind.IndoorItemGroup != null && !ri.IsMainIndoor && ind.IndoorItemGroup.Count > 0)
                {
                    RoomIndoor listInd = ind.IndoorItemGroup.Find(p => p.IndoorNO == ri.IndoorNO && p.IsExchanger == ri.IsExchanger);
                    if (listInd != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否可以共享
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public bool IsShareRemoteControl(RoomIndoor ri)
        {
            if (ri.ListAccessory != null)
            {
                if (ri.ListAccessory.FindAll(p => p.Type == RemoteControl).Count >= 2)
                    return false;
                //判断是否已共享
                if (ri.IsMainIndoor) return false;
                //判断是否存在Wireless Remote Control Switch 与Receiver Kit for Wireless Control
                if (ri.ListAccessory.FindAll(p => p.Type == WirelessRemoteControl || p.Type == ReceiverRemoteControl).Count > 0)
                {
                    return false;
                }
                List<RoomIndoor> list = GetMainIndoorItems();
                foreach (RoomIndoor ind in list)
                {
                    if (ind.IndoorItemGroup != null && ind.IndoorItemGroup.Count != 0)
                    {
                        RoomIndoor listInd = ind.IndoorItemGroup.Find(p => p.IndoorNO == ri.IndoorNO && p.IsExchanger == ri.IsExchanger);
                        if (listInd != null) return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// 获取可用的配件
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="columnsName"></param>
        /// <returns></returns>
        public DataTable GetAvailableAccessorys(RoomIndoor ri, string columnsName)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("DisplayType");
            dt.Columns.Add("AccessoryType");
            dt.Columns.Add("AccessoryModel");
            dt.Columns.Add("Max_Qty");
            dt.Columns.Add("Available_Qty");
            dt.Columns.Add("Selected_Qty");
            DataTable dta = GetAccessoryModelByGroup(columnsName);
            string availableType = "";
            foreach (DataRow drow in dta.Rows)
            {
                availableType += "'" + drow["AccessoryType"] + "',";
            }
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
            DataTable AccessoryList = abll.GetInDoorAccessoryItemList(thisProject.BrandCode, fCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity, thisProject.RegionCode, ri, thisProject.SubRegionCode);
            if (AccessoryList.Rows.Count > 0 && !string.IsNullOrEmpty(availableType))
            {
                DataView dv = AccessoryList.DefaultView;
                dv.RowFilter = "Type in(" + availableType.TrimEnd(',') + ")";
                if (columnsName == ReceiverKit)
                {
                    dv.Sort = "Model_Hitachi asc";
                }
                AccessoryList = dv.ToTable();
                if (AccessoryList.Rows.Count > 0)
                {
                    foreach (DataRow dr in AccessoryList.Rows)
                    {
                        DataRow row = dt.NewRow();
                        row["AccessoryType"] = dr["Type"].ToString();
                        string model = (thisProject.BrandCode == "Y" ? dr["Model_York"].ToString() : dr["Model_Hitachi"].ToString());
                        row["DisplayType"] = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, dr["Type"].ToString(), model, ri, true);
                        row["AccessoryModel"] = model;
                        row["Max_Qty"] = dr["MaxNumber"].ToString();
                        int selected_qty = GetSelectedAccessoryCount((thisProject.BrandCode == "Y" ? dr["Model_York"].ToString() : dr["Model_Hitachi"].ToString()), ri);
                        row["Selected_Qty"] = selected_qty;
                        row["Available_Qty"] = Convert.ToInt32(dr["MaxNumber"]) - selected_qty;
                        dt.Rows.Add(row);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// 获取已选中的室内机配件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ri"></param>
        /// <returns></returns>
        public int GetSelectedAccessoryCount(string model, RoomIndoor ri)
        {
            int count = 0;
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                if (thisProject.BrandCode == "Y")
                {
                    count = ri.ListAccessory.FindAll(p => p.Model_York == model).Count;
                }
                else
                {
                    count = ri.ListAccessory.FindAll(p => p.Model_Hitachi == model).Count;
                }
            }
            return count;
        }


        /// <summary>
        /// 行高
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="i"></param>
        private int GetRowHeight(DataTable dt)
        {
            int defaultRowHeight = 38;
            int TypeCount = 1;
            int addHeight = 0;
            int maxCount = 0;
            bool isAddHeight = false;
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < maxAccessoryType.Length; i++)
                {
                    if (dr["DisplayType"].ToString() == maxAccessoryType[i])
                        maxCount++;
                }
            }
            if (maxCount > 0)
                isAddHeight = true;

            string MaxGroup = "";
            DataTable newdt = new System.Data.DataTable();
            newdt.Columns.Add("Count");
            newdt.Columns.Add("Group");
            DataTable dcopy = dt.Copy();
            foreach (DataRow dr in dcopy.Rows)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "AccessoryGroup='" + dr["AccessoryGroup"] + "'";
                DataTable dts = dv.ToTable();
                if (dts.Rows.Count > 0)
                {
                    DataRow drow = newdt.NewRow();
                    drow[0] = dts.Rows.Count;
                    drow[1] = dts.Rows[0]["AccessoryGroup"].ToString();
                    newdt.Rows.Add(drow);
                }
            }
            if (newdt.Rows.Count > 0)
            {
                DataView dtsm = newdt.DefaultView;
                dtsm.Sort = "Count desc";
                DataTable dtsml = dtsm.ToTable();
                // TypeCount = Convert.ToInt32(newdt.Compute("Max(Count)", ""));
                TypeCount = Convert.ToInt32(dtsml.Rows[0][0].ToString());
                MaxGroup = dtsml.Rows[0][1].ToString();
            }
            if (isAddHeight)
                addHeight = 15 * maxCount;

            if (TypeCount > 1)
                return (defaultRowHeight * TypeCount) + (TypeCount * 2) + addHeight;
            else
                return defaultRowHeight + addHeight + 2;
        }


        /// <summary>
        /// 获取配件类型通过分组
        /// </summary>
        /// <param name="group">组别</param>
        /// <returns></returns>
        public DataTable GetAccessoryModelByGroup(string group)
        {
            DataView dv = AccessoryType.DefaultView;
            dv.RowFilter = "AccessoryGroup='" + group + "' and AccessoryModel='" + thisProject.BrandCode + "'";
            return dv.ToTable();
        }

        /// <summary>
        /// 通过组别类型名称获取到组别名称
        /// </summary>
        /// <param name="ColumnsName"></param>
        /// <param name="Model"></param>
        /// <returns></returns>
        public string GetAccessoryGroupName(string ColumnsName, string Model)
        {
            string group = Others;
            foreach (DataRow dr in AccessoryType.Rows)
            {
                if (dr["AccessoryModel"].ToString() == Model && dr["AccessoryType"].ToString() == ColumnsName)
                {
                    group = dr["AccessoryGroup"].ToString();
                    break;
                }
            }
            return group;
        }


        /// <summary>
        /// 通过列名与类型获取到列的索引
        /// </summary>
        /// <param name="ColumnsName"></param>
        /// <param name="Model"></param>
        /// <returns></returns>
        private int GetColumnsNameIndex(string ColumnsName, string Model)
        {
            int numbers = 12;
            string group = Others;
            foreach (DataRow dr in AccessoryType.Rows)
            {
                if (dr["AccessoryModel"].ToString() == Model && dr["AccessoryType"].ToString() == ColumnsName)
                {
                    group = dr["AccessoryGroup"].ToString();
                    break;
                }
            }
            switch (group)
            {
                case "Remote Control Switch":
                    numbers = 5;
                    break;
                case "Receiver Kit":
                    numbers = 7;
                    break;
                case "Panel":
                    numbers = 8;
                    break;
                case "Filter":
                    numbers = 9;
                    break;
                case "Filter Box":
                    numbers = 10;
                    break;
                case "Drain-up":
                    numbers = 11;
                    break;
                default:
                    numbers = 12;
                    break;
            }

            return numbers;
        }

        /// <summary>
        /// 获取列的索引
        /// </summary>
        /// <param name="ColumnsName"></param>
        /// <returns></returns>
        private int GetColumnsNameIndex(string ColumnsName)
        {
            int number = -1;
            for (int i = 7; i < dgvAvailableItems.Columns.Count; i++)
            {
                if (this.dgvAvailableItems.Columns[i].Name == ColumnsName)
                {
                    number = i;
                    break;
                }
            }
            return number;
        }

        ///获取行的索引
        private int GetRowIndex(RoomIndoor ri)
        {
            if (ri.IndoorNO == 0)
                return 0;
            int row = 0;
            for (int i = 0; i < dgvAvailableItems.Rows.Count; i++)
            {
                if (this.dgvAvailableItems.Rows[i].Cells[0].Value.ToString() == ri.IndoorNO.ToString() && this.dgvAvailableItems.Rows[i].Cells[1].Value.ToString() == ri.IsExchanger.ToString())
                {
                    row = i;
                    break;
                }
            }
            return row;
        }

        /// <summary>
        /// 判断是否存在该类型
        /// </summary>
        /// <param name="ColumnsName"></param>
        /// <returns></returns>
        private bool IsColumnsName(string ColumnsName)
        { 
            for (int i = 7; i < dgvAvailableItems.Columns.Count; i++)
            {
                if (this.dgvAvailableItems.Columns[i].Name == ColumnsName)
                {
                    return true; 
                }
            }
            return false;
        }


        //转换Accessory 的数量
        private DataTable ConvertAccessoryCount(List<Accessory> list, string Model, bool isShare, RoomIndoor ri)
        {
            DataTable tb = new DataTable();
            tb.Columns.Add("IndoorModel");
            tb.Columns.Add("AccessoryGroup");
            tb.Columns.Add("AccessoryType");
            tb.Columns.Add("DisplayType");
            tb.Columns.Add("AccessoryModel");
            tb.Columns.Add("Qty");
            if (list != null)
            {
                list = list.FindAll(p => p.IsShared == isShare);
                foreach (Accessory item in list)
                {
                    string aModel = item.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi;
                    List<Accessory> Locunt = new List<Accessory>();
                    DataRow row = tb.NewRow();
                    row["AccessoryType"] = item.Type;
                    if (item.BrandCode == "Y")
                    {
                        row["IndoorModel"] = Model;
                        row["AccessoryModel"] = item.Model_York;
                        Locunt = list.FindAll(p => p.Model_York == aModel);
                    }
                    else
                    {
                        row["IndoorModel"] = Model;
                        row["AccessoryModel"] = item.Model_Hitachi;
                        Locunt = list.FindAll(p => p.Model_Hitachi == aModel);
                    }
                    row["DisplayType"] = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, item.Type, item.Model_Hitachi, ri, true);
                    row["AccessoryGroup"] = GetAccessoryGroupName(item.Type, item.BrandCode);
                    row["Qty"] = Locunt.Count;
                    if (!HasTableData(tb, aModel))
                        tb.Rows.Add(row);
                }
            }
            if (tb != null)
            {
                DataTable dtCopy = tb.Copy();
                DataView dv = tb.DefaultView;
                dv.Sort = "DisplayType";
                tb = dv.ToTable();
            }
            return tb;
        }


        /// <summary>
        /// 过滤字符串中的重复字符
        /// </summary>
        /// <param name="str">要过滤的字符串</param>
        /// <returns>返回过滤后的字符串</returns>
        public string FilterRepetitionChar(string sourceStr)
        {
            string returnStr = string.Empty;
            string[] strList = sourceStr.Split(',');
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            foreach (string strChar in strList)
            {
                if (!ht.ContainsKey(strChar))
                {
                    ht.Add(strChar, strChar);//这里让ht的key和value值相等，不影响下面的程序
                    returnStr += strChar + ",";//字符以逗号分隔
                }
            }
            returnStr = returnStr.Trim(',');//去掉最后一个逗号
            return returnStr;
        }


        // 获取当前室内机可用的类型 
        private DataTable GetAvailableColumns_ByType(RoomIndoor roomIndoor)
        {
            //string factoryCode =  roomIndoor.IndoorItem.ModelFull.Substring(roomIndoor.IndoorItem.ModelFull.Length - 1, 1);
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string factoryCode = roomIndoor.IndoorItem.GetFactoryCodeForAccess();
            string typeItem = roomIndoor.IndoorItem.Type;
            DataTable dt = abll.GetAvailableListType(thisProject.BrandCode, factoryCode.ToString(), typeItem.ToString(), thisProject.RegionCode, roomIndoor, thisProject.SubRegionCode);
            return dt;
        }


        /// <summary>
        ///   指定列不可编剧及列宽
        /// </summary>
        private void InitDataGridView_ColumnsWidth()
        {
            for (int i = 0; i < dgvAvailableItems.Columns.Count; i++)
            {
                this.dgvAvailableItems.Columns[0].Visible = false;
                this.dgvAvailableItems.Columns[1].Visible = false;
                if (i < 5)
                {
                    int columnswidth = 130;
                    if (i == 2)
                    {
                        columnswidth = 90;
                    }
                    if (i == 3)
                    {
                        columnswidth = 80;
                    }
                    this.dgvAvailableItems.Columns[i].ReadOnly = true;
                    dgvAvailableItems.Columns[i].Width = columnswidth;
                    dgvAvailableItems.Columns[i].Frozen = true;
                }
                if (i >= 5)
                {
                    dgvAvailableItems.Columns[i].Width = 170;
                    if (i < 7)
                        dgvAvailableItems.Columns[i].Frozen = true;
                }
            }
            this.WindowState = FormWindowState.Maximized;
            if (this.Width > 1675)
            {
                int Vwidth = Convert.ToInt32((this.Width - 1675) / 8); 
                for (int i = 5; i < dgvAvailableItems.Columns.Count; i++)
                {
                    dgvAvailableItems.Columns[i].Width = dgvAvailableItems.Columns[i].Width + Vwidth;
                }
            }
        }

        /// <summary>
        /// 重汇标题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAvailableItems_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = UtilColor.bg_dgvHeader_Indoor;
            Pen pen_dgvBorder = new Pen(Color.FromArgb(127, 127, 127), 0.1f);
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex == -1)
            {
                SolidBrush brush = new SolidBrush(color_bg_dgvHeader);
                using (brush)
                {
                    Rectangle border = e.CellBounds;
                    e.Graphics.FillRectangle(brush, border);
                    border.X -= 1;
                    border.Y -= 1;
                    if (e.ColumnIndex == 0)
                        border.X += 1;
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }
                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }


        /// <summary>
        /// 查找此表中是否存在重复的机组记录，若已存在则对应的Qty列加1
        /// </summary>
        /// <param name="data">需要查询的数据表</param>
        /// <param name="keyName">查询关键词</param> 
        private bool HasTableData(DataTable data, string keyName)
        { 
            foreach (DataRow dr in data.Rows)
            {
                if (dr["AccessoryModel"].ToString() == keyName)
                {
                    return true; 
                }
            }
            return false;
        }

        /// <summary>
        /// 验证错误的Accessory
        /// </summary>
        private void ValidateAccessory()
        {
            string outMsg = "";
            Dictionary<string, string> errorList = new Dictionary<string, string>();
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                List<Accessory> errorAccessoryList = new List<Accessory>();
                if (!ExistsAccessory(ri, out errorAccessoryList))
                {
                    foreach (Accessory item in errorAccessoryList)
                    {
                        string name = thisProject.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi;
                        if (!errorList.ContainsKey(name))
                        {
                            errorList.Add(name, item.Type);
                        }
                    } 
                }
            }
            foreach (RoomIndoor ri in thisProject.ExchangerList)
            {
                List<Accessory> errorAccessoryList = new List<Accessory>();
                if (!ExistsAccessory(ri, out errorAccessoryList))
                {
                    foreach (Accessory item in errorAccessoryList)
                    {
                        string name = thisProject.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi;
                        if (!errorList.ContainsKey(name))
                        {
                            errorList.Add(name, item.Type);
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, string> kvp in errorList)
            {
                outMsg += " "+kvp.Value+" (" + kvp.Key + ") ,";
            }
            outMsg = FilterRepetitionChar(outMsg);

            if (!string.IsNullOrEmpty(outMsg))
            {
                toolStripStatusLabel1.Text = Msg.ERR_ACCESSORY_NOACCESSORY(outMsg);
            }
            else
            {
                toolStripStatusLabel1.Text = "";
            }
        }


        /// <summary>
        /// 是否存在错误的配件
        /// </summary>
        /// <param name="ri">室内机</param>
        /// <param name="errorAccessoryList">错误的配件集合</param>
        /// <returns></returns>
        private bool ExistsAccessory(RoomIndoor ri, out List<Accessory> errorAccessoryList)
        {
            bool isTrue = true;
            errorAccessoryList = new List<Accessory>();
            string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
            DataTable AccessoryList = abll.GetInDoorAccessoryItemList(thisProject.BrandCode, fCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity, thisProject.RegionCode, ri, thisProject.SubRegionCode);
            if (AccessoryList.Rows.Count < 1) return isTrue;
            isTrue = ExistsAccessory(AccessoryList, ri.ListAccessory, out errorAccessoryList);
            return isTrue;
        }

        /// <summary>
        /// 是否存在错误的配件
        /// </summary>
        /// <param name="dt">配件</param>
        /// <param name="list">当前室内机配件</param>
        /// <param name="errorAccessoryList">错误的配件集合</param>
        /// <returns></returns>
        private bool ExistsAccessory(DataTable dt, List<Accessory> list, out List<Accessory> errorAccessoryList)
        {
            bool isTrue = true;
            DataRow[] rows;
            errorAccessoryList = new List<Accessory>();
            if (list.Count > 0)
            {
                DataTable newdt = new DataTable();
                foreach (Accessory acitem in list)
                {
                    string name = thisProject.BrandCode == "Y" ? acitem.Model_York : acitem.Model_Hitachi;
                    if (thisProject.BrandCode == "Y")
                    {
                        rows = dt.Select("Model_York='" + name + "'");
                    }
                    else
                    {
                        rows = dt.Select("Model_Hitachi='" + name + "'");
                    }
                    if (rows.Length == 0)
                    {
                        errorAccessoryList.Add(acitem);
                    }
                }
            }
            if (errorAccessoryList.Count > 0)
            {
                isTrue = false;
            }
            return isTrue;
        }



        /// <summary>
        /// 编辑单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAvailableItems_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            this.panelDetail.Visible = false;
            this.panelControl.Visible = false;
            EditCurrentCell();
        }

        /// <summary>
        /// 编辑单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditCurrentCell()
        {
            dgvAvailableItems.CurrentRow.Selected = true;
            if (dgvAvailableItems.CurrentCell != null)
            {
                bool isExchanger = false;
                //判断当前单元格是否可编辑
                if (dgvAvailableItems.CurrentCell.ReadOnly) return;
                row = dgvAvailableItems.CurrentCell.RowIndex;
                col = dgvAvailableItems.CurrentCell.ColumnIndex;
                //取得当前列名
                string columnsName = dgvAvailableItems.Columns[col].Name;
                //取得当前对应的室内机编号
                string indoorNo = dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Cells[0].Value.ToString();
                //获取当前对应的室内机对应的类型
                isExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[row].Cells[1].Value);
                RoomIndoor ri = GetRoomIndoorDetial(isExchanger, Convert.ToInt32(indoorNo));
                // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
                string fCode = ri.IndoorItem.GetFactoryCodeForAccess();
                DataTable AccessoryList = abll.GetInDoorAccessoryItemList(thisProject.BrandCode, fCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity, thisProject.RegionCode, ri, thisProject.SubRegionCode);

                if (AccessoryList.Rows.Count < 1) return;

                if (ri.ListAccessory == null)
                {
                    ri.ListAccessory = new List<Accessory>();
                }

                //错误的配件集合
                List<Accessory> errorList = new List<Accessory>();
                ExistsAccessory(AccessoryList, ri.ListAccessory, out errorList);

                DgvColumnProperties dgv = new DgvColumnProperties();
                dgv.Accessory_IndoorId = ri.IndoorNO;
                dgv.Accessory_Type = columnsName;
                dgv.Accessory_IsAdd = false;
                dgv.Accessory_IsShare = false;

                //根据列名 得到可以适配的配件
                DataTable dta = GetAccessoryModelByGroup(columnsName);
                string availableType = "";
                foreach (DataRow drow in dta.Rows)
                {
                    availableType += "'" + drow["AccessoryType"] + "',";
                }
                List<AccessoryItem> list = new List<AccessoryItem>();

                if (string.IsNullOrEmpty(availableType) && columnsName == ShareRemoteControl)
                {
                    availableType = "'Remote Control Switch','Half-size Remote Control Switch'";
                }
                //筛选出当前类型的配件
                DataView dv = AccessoryList.DefaultView;
                if (!string.IsNullOrEmpty(availableType))
                {
                    dv.RowFilter = "Type in(" + availableType.TrimEnd(',') + ")";
                    AccessoryList = dv.ToTable();
                }
                if (columnsName == RemoteControl)
                {
                    #region 判断是否存在Remote Control Swicth 与Half-size Remote Control Swicth 
                    //判断当前室内机已被共享   是否是MainIndoor
                    if (SharedRemoteControl(ri) || IsMainIndoorByShared(ri))
                    {
                        dgv.Accessory_IsAdd = false;
                        dgv.AccessoryItem = new List<AccessoryItem>();
                        dgvAvailableItems.CurrentCell.Tag = dgv;
                        return;
                    }
                    if (IsWireless_Control(ri.ListAccessory, WirelessRemoteControl))
                    {
                        List<Accessory> li = ri.ListAccessory.FindAll(p => p.Type == WirelessRemoteControl);
                        dgv.Accessory_IsAdd = false;
                        List<AccessoryItem> listItems = new List<AccessoryItem>();
                        AccessoryItem acItem = new AccessoryItem();
                        acItem.AccessoryItem_Name = thisProject.BrandCode == "Y" ? li[0].Model_York : li[0].Model_Hitachi;
                        acItem.AccessoryItem_MaxNumber = li[0].MaxNumber;
                        acItem.AccessoryItem_Number = li.Count;
                        acItem.AccessoryItem_Id = ri.IndoorNO;
                        acItem.AccessoryItem_Type = WirelessRemoteControl;
                        acItem.IsDelete = true;
                        acItem.AccessoryItem_ShowType = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, WirelessRemoteControl, acItem.AccessoryItem_Name, ri, true);
                        listItems.Add(acItem);
                        dgv.AccessoryItem = listItems;
                        dgvAvailableItems.CurrentCell.Tag = dgv;
                        return;
                    }
                    //判断是否存在Remote Control Swicth 与Half-size Remote Control Swicth
                    if (IsHalf_RemoteControl(ri.ListAccessory))
                    {
                        DataView dvs = AccessoryList.DefaultView;
                        dvs.RowFilter = "Type not in('" + WirelessRemoteControl + "')";
                        AccessoryList = dvs.ToTable();
                    }
                    #endregion
                }
                if (columnsName == ReceiverKit)
                {
                    #region
                    //判断是否ReceiverKit 配件类型
                    if (AccessoryList.Rows.Count < 1)
                    {
                        dgv.Accessory_IsAdd = false;
                        dgv.AccessoryItem = new List<AccessoryItem>();
                        dgvAvailableItems.CurrentCell.Tag = dgv;
                        return;
                    }

                    //存在 RemoteControl HalfRemoteControl 则不可编辑
                    List<Accessory> controlList = ri.ListAccessory.FindAll(p => p.Type == RemoteControl || p.Type == HalfRemoteControl);
                    {
                        if (controlList != null && controlList.Count > 0)
                        {
                            dgv.Accessory_IsAdd = false;
                            dgv.AccessoryItem = new List<AccessoryItem>();
                            dgvAvailableItems.CurrentCell.Tag = dgv;
                            return;
                        }
                    }
                    //判断是否已共享
                    if (SharedRemoteControl(ri))
                    {
                        //判断是否存在ReceiverRemoteControl 
                        if (ri.ListAccessory.FindAll(p => p.Type == ReceiverRemoteControl).Count == 0)
                        {
                            dgv.Accessory_IsAdd = false;
                            dgv.AccessoryItem = new List<AccessoryItem>();
                            dgvAvailableItems.CurrentCell.Tag = dgv;
                            return;
                        }
                    }
                    #endregion
                }
                if (columnsName == ShareRemoteControl)
                {
                    #region ShareRemoteControl
                    string shareSttr = IsSharedRemoteControl(ri);
                    //判断是否存在 HalfRemoteControl，RemoteControl，WirelessRemoteControl 
                    List<Accessory> controlList = ri.ListAccessory.FindAll(p => p.Type == RemoteControl || p.Type == HalfRemoteControl || p.Type == WirelessRemoteControl);
                    if (controlList != null && controlList.Count > 0)
                    {
                        //是否存在共享 不存在共享则不可编辑当前单元格
                        if (controlList.FindAll(p => p.IsShared == true).Count == 0)
                        {
                            dgv.Accessory_IsAdd = false;
                            dgv.AccessoryItem = new List<AccessoryItem>();
                            dgvAvailableItems.CurrentCell.Tag = dgv;
                            return;
                        }
                    }

                    //判断当前室内机是否已共享（被共享）
                    if (shareSttr.Length > 0)
                    {
                        dgv.Accessory_IsShare = true;
                        dgv.Accessory_IsAdd = false;
                        dgv.AccessoryItem = new List<AccessoryItem>();
                        AccessoryItemShare item = new AccessoryItemShare();
                        item.AccessoryItemShare_Id = ri.IndoorNO;
                        item.AccessoryItemShare_Name = shareSttr;
                        List<AccessoryItemShare> lists = new List<AccessoryItemShare>();
                        lists.Add(item);
                        dgv.AccessoryItemShare = lists;
                        dgvAvailableItems.CurrentCell.Tag = dgv;
                        return;
                    }

                    //判断是否存在ShareRemoteControl 
                    List<Accessory> li = ri.ListAccessory.FindAll(p => p.Type == RemoteControl && p.IsShared == true || p.Type == HalfRemoteControl && p.IsShared == true || p.Type == WirelessRemoteControl && p.IsShared == true);
                    if (li != null && li.Count > 0)
                    {
                        dgv.Accessory_IsAdd = false;
                        List<AccessoryItem> listItems = new List<AccessoryItem>();
                        AccessoryItem acItem = new AccessoryItem();
                        acItem.AccessoryItem_Name = thisProject.BrandCode == "Y" ? li[0].Model_York : li[0].Model_Hitachi;
                        acItem.AccessoryItem_MaxNumber = 1;
                        acItem.AccessoryItem_Number = 1;
                        acItem.AccessoryItem_Id = ri.IndoorNO;
                        acItem.AccessoryItem_Type = columnsName;
                        acItem.AccessoryItem_ShowType = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, columnsName, acItem.AccessoryItem_Name, ri, true);
                        acItem.IsDelete = true;
                        listItems.Add(acItem);
                        dgv.AccessoryItem = listItems;
                        dgvAvailableItems.CurrentCell.Tag = dgv;
                        return;
                    }
                    #endregion
                }

                #region//判断是否存在关联的类型
                string newType = GetNewTypeName(columnsName, ri);
                if (!string.IsNullOrEmpty(newType))
                {
                    if (ri.ListAccessory != null && columnsName == ReceiverRemoteControl || columnsName == WirelessRemoteControl)
                    {
                        List<Accessory> li = ri.ListAccessory.FindAll(p => p.Type == RemoteControl || p.Type == HalfRemoteControl);
                        if (li != null && li.Count > 0)
                        {
                            dgv.Accessory_IsAdd = false;
                            dgv.AccessoryItem = new List<AccessoryItem>();
                            dgvAvailableItems.CurrentCell.Tag = dgv;
                            return;
                        }
                    }
                }
                #endregion

                #region
                foreach (DataRow dr in AccessoryList.Rows)
                {
                    AccessoryItem access = new AccessoryItem();
                    string name = thisProject.BrandCode == "Y" ? dr["Model_York"].ToString() : dr["Model_Hitachi"].ToString();
                    access.AccessoryItem_Name = name;
                    access.AccessoryItem_MaxNumber = Convert.ToInt32(dr["MaxNumber"]);
                    access.AccessoryItem_Number = 0;
                    access.AccessoryItem_Id = ri.IndoorNO;
                    access.AccessoryItem_Type = dr["Type"].ToString();
                    access.AccessoryItem_ShowType = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, dr["Type"].ToString(), name, ri, true);
                    if (columnsName != RemoteControl && Convert.ToBoolean(dr["IsDefault"].ToString()))
                        access.IsDelete = false;
                    else
                        access.IsDelete = true;
                    foreach (Accessory item in ri.ListAccessory)
                    {
                        List<Accessory> Locunt = ri.ListAccessory;
                        if (name == (thisProject.BrandCode == "Y" ? item.Model_York : item.Model_Hitachi))
                        {
                            if (thisProject.BrandCode == "Y")
                            {
                                if (columnsName == ShareRemoteControl)
                                    Locunt = ri.ListAccessory.FindAll(p => p.Model_York == name && p.IsShared == true);
                                else
                                    Locunt = ri.ListAccessory.FindAll(p => p.Model_York == name && p.IsShared == false);
                            }
                            else
                            {
                                if (columnsName == ShareRemoteControl)
                                    Locunt = ri.ListAccessory.FindAll(p => p.Model_Hitachi == name && p.IsShared == true);
                                else
                                    Locunt = ri.ListAccessory.FindAll(p => p.Model_Hitachi == name && p.IsShared == false);
                            }
                            access.AccessoryItem_Number = Locunt.Count;
                            break;
                        }
                    }
                    list.Add(access);
                }
                #endregion
                //判断是否可以继续新增
                foreach (AccessoryItem itm in list)
                {
                    if (itm.AccessoryItem_Number == 0)
                    {
                        dgv.Accessory_IsAdd = true;
                        break;
                    }
                }

                //判断当前类型的Accessory 是否可以新增 
                List<Accessory> selectedlist = ri.ListAccessory.FindAll(p => p.Type == columnsName);
                if (selectedlist != null && selectedlist.Count > 0)
                {
                    if (selectedlist[0].MaxNumber == selectedlist.Count)
                    {
                        dgv.Accessory_IsAdd = false;
                    }
                }
                if (columnsName == ReceiverKit)
                {
                    List<Accessory> selectedlists = ri.ListAccessory.FindAll(p => p.Type == ReceiverRemoteControl);
                    if (selectedlists != null && selectedlists.Count > 0)
                    {
                        if (selectedlists[0].MaxNumber == selectedlists.Count)
                        {
                            dgv.Accessory_IsAdd = false;
                        }
                    }
                }

                //当前室内机配件
                DataTable dtSelectedAccessory = ConvertAccessoryCount(ri.ListAccessory, (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi, false, ri);
                //当前行的高度 
                int Height = GetRowHeight(dtSelectedAccessory);
                if (dgv.Accessory_IsAdd)
                {
                    if (list.FindAll(p => p.AccessoryItem_Number > 0).Count >= 1)
                    {
                        dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Height = Height + 38;
                    }
                }
                else
                {
                    dgvAvailableItems.Rows[dgvAvailableItems.CurrentCell.RowIndex].Height = Height;
                }


                if (errorList.Count > 0)
                {
                    list = AddErrorAccessory(list, errorList, columnsName, ri);
                } 

                //排序
                if (list != null && list.Count > 0)
                {
                    list.Sort(delegate (AccessoryItem x, AccessoryItem y)
                    {
                        return x.AccessoryItem_ShowType.CompareTo(y.AccessoryItem_ShowType);
                    });
                } 
                dgv.AccessoryItem = list;
                dgvAvailableItems.CurrentCell.Tag = dgv;
            }
        }

        /// <summary>
        /// 当前室内机存在已删除的配件 需要添加到列表可以提供删除 
        /// </summary>
        /// <param name="list">当前配件列表</param>
        /// <param name="errorList">错误配件列表</param>
        /// <param name="columnsName">当前列</param>
        /// <param name="ri">当前室内机</param>
        /// <returns></returns>
        private List<AccessoryItem> AddErrorAccessory(List<AccessoryItem> list, List<Accessory> errorList,string columnsName,RoomIndoor ri)
        {
            List<AccessoryItem> newlist = list;
            foreach (Accessory acitem in errorList)
            {
                if (GetAccessoryGroupName(acitem.Type, thisProject.BrandCode) == columnsName)
                {
                    AccessoryItem access = new AccessoryItem();
                    string name = thisProject.BrandCode == "Y" ? acitem.Model_York : acitem.Model_Hitachi;
                    access.AccessoryItem_Name = name;
                    access.AccessoryItem_MaxNumber = Convert.ToInt32(acitem.MaxNumber);
                    access.AccessoryItem_Number = 0;
                    access.AccessoryItem_Id = ri.IndoorNO;
                    access.AccessoryItem_Type = acitem.Type;
                    access.AccessoryItem_ShowType = AccessoryDisplayType.GetAccessoryDisplayTypeByModel(thisProject.SubRegionCode, dtUniversal, acitem.Type, name, ri, true);
                    if (columnsName != RemoteControl && Convert.ToBoolean(acitem.IsDefault))
                        access.IsDelete = false;
                    else
                        access.IsDelete = true;
                    if (thisProject.BrandCode == "Y")
                    {
                        access.AccessoryItem_Number = ri.ListAccessory.FindAll(p => p.Model_York == name).Count;
                    }
                    else
                    {
                        access.AccessoryItem_Number = ri.ListAccessory.FindAll(p => p.Model_Hitachi == name).Count;
                    }
                    newlist.Add(access);
                }
            }
            return newlist;
        }


        /// <summary>
        /// 判断是否共享（ReceiverKit）
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRemoteControlByReceiverKit(RoomIndoor ri, string name)
        {
            bool isTrue = false;
            DataTable dt = GetAccessoryModelByGroup(RemoteControl);
            //判断是否存在ReceiverKit 如果存在 name 改变 
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AccessoryType"].ToString() == ReceiverKit)
                    {
                        name = RemoteControl;
                    }
                }
            }
            if (ri.ListAccessory != null)
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = "AccessoryType not in('" + name + "')";
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        List<Accessory> li = ri.ListAccessory.FindAll(p => p.Type == dr["AccessoryType"].ToString());
                        if (li.Count > 0)
                        {
                            isTrue = true;
                            break;
                        }
                    }
                }
            }
            return isTrue;
        }


        //是否存在Half_Size Remote Control
        public bool IsHalf_RemoteControl(List<Accessory> list)
        {
            if (list != null && list.Count > 0)
            {
                if (list.FindAll(p => p.IsShared == false && p.Type == RemoteControl || p.Type == HalfRemoteControl).Count > 0)
                {
                    return true;
                }
            }
            return false;
        }


        // 是否存在Wireless Remote Control
        public bool IsWireless_Control(List<Accessory> list, string name)
        { 
            if (list != null && list.Count > 0)
            {
                foreach (Accessory acitem in list)
                {
                    if (acitem.Type == name)
                    {
                        return true; 
                    }
                }
            }
            return false;
        }


        /// <param name="ri"></param>
        /// <param name="isGroup">isGroup false 表示Remote Control Switch, Half-size Remote Control Switch</param>
        /// <param name="isGroup">isGroup true  表示Receiver Kit for Wireless Control, Wireless Remote Control Switch</param>
        /// <returns></returns>
        public int GetGroupLimitControl(RoomIndoor ri, bool isGroup)
        {
            int count = 0;
            List<Accessory> li = new List<Accessory>();
            if (ri.ListAccessory != null)
            {
                if (isGroup)
                {
                    li = ri.ListAccessory.FindAll(p => p.Type == unitRemote[2] || p.Type == unitRemote[3]);
                }
                else
                {
                    li = ri.ListAccessory.FindAll(p => p.Type == unitRemote[0] || p.Type == unitRemote[1]);
                }
                if (li.Count > 0)
                {
                    count = li.Count;
                }
            }
            return count;
        }


        /// <summary>
        /// 获取Control 不同的类型
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public int GetTypeLimitControl(RoomIndoor ri, string column)
        {
            int count = 0;
            List<Accessory> li = new List<Accessory>();
            if (ri.ListAccessory != null)
            {
                for (int i = 1; i < unitRemote.Length; i++)
                {
                    if (unitRemote[i].ToString() != column)
                    {
                        li = ri.ListAccessory.FindAll(p => p.Type == unitRemote[i]);
                        if (li.Count > 0)
                        {
                            count = li.Count;
                            break;
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 离开当前行 重新计算当前行的高度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAvailableItems_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            string columnsName = dgvAvailableItems.Columns[e.ColumnIndex].Name;
            //取得当前对应的室内机编号
            string IndoorNo = dgvAvailableItems.Rows[e.RowIndex].Cells[0].Value.ToString();
            bool isExc = Convert.ToBoolean(dgvAvailableItems.Rows[e.RowIndex].Cells[1].Value);
            RoomIndoor ri = GetRoomIndoorDetial(isExc, Convert.ToInt32(IndoorNo));
            if (ri == null) return;
            //当前室内机配件
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                DataTable dtSelectedAccessory = ConvertAccessoryCount(ri.ListAccessory, (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi, false, ri);
                if (dtSelectedAccessory.Rows.Count > 0)
                {
                    dgvAvailableItems.Rows[e.RowIndex].Height = GetRowHeight(dtSelectedAccessory);
                }
            }
            else
            {
                dgvAvailableItems.Rows[e.RowIndex].Height = 38;
            }
        }

        /// <summary>
        /// 重汇Panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelDetail_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelDetail.ClientRectangle, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid);
        }

        /// <summary>
        /// 显示添加配件
        /// </summary>
        private void ShowPanel()
        {
            if (!dgvAvailableItems.CurrentCell.ReadOnly)
            {
                int dgvX = dgvAvailableItems.Location.X;
                int dgvY = dgvAvailableItems.Location.Y;
                int cellX = dgvAvailableItems.GetCellDisplayRectangle(dgvAvailableItems.CurrentCell.ColumnIndex, dgvAvailableItems.CurrentCell.RowIndex, false).X;
                int cellY = dgvAvailableItems.GetCellDisplayRectangle(dgvAvailableItems.CurrentCell.ColumnIndex, dgvAvailableItems.CurrentCell.RowIndex, false).Y;
                int x = dgvX + cellX;
                int y = dgvY + cellY;
                if ((y + 375) > this.Height)
                {
                    panelDetail.Location = new Point(x - 240, this.Height - 375);
                }
                else
                {
                    panelDetail.Location = new Point(x - 240, y);
                }
                panelDetail.Visible = true;
            }
            else
            {
                panelDetail.Visible = false;
            }
        }

        /// <summary>
        /// 设置Share remote control  
        /// </summary>
        private void ShowSharePanel()
        {
            if (!dgvAvailableItems.CurrentCell.ReadOnly)
            {
                int dgvX = dgvAvailableItems.Location.X;
                int dgvY = dgvAvailableItems.Location.Y;
                int cellX = dgvAvailableItems.GetCellDisplayRectangle(dgvAvailableItems.CurrentCell.ColumnIndex, dgvAvailableItems.CurrentCell.RowIndex, false).X;
                int cellY = dgvAvailableItems.GetCellDisplayRectangle(dgvAvailableItems.CurrentCell.ColumnIndex, dgvAvailableItems.CurrentCell.RowIndex, false).Y;
                int x = dgvX + cellX;
                int y = dgvY + cellY;
                if ((y + 386) > this.Height)
                {
                    panelControl.Location = new Point(x - 235, this.Height - 386);
                }
                else
                {
                    panelControl.Location = new Point(x - 235, y); 
                }
                panelControl.Visible = true;
            }
            else
            {
                panelControl.Visible = false;
            }
        }

        /// <summary>
        /// 取消Panel 显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.panelDetail.Visible = false;
        }

        /// <summary>
        /// 新增配件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            bool isAdd = false;
            if (!JCValidateSingle(txtNumber))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return;
            }
            string model = jccmbAccessoryModel.Text;
            if (Convert.ToInt32(txtNumber.Text) > 0)
            {
                if (selectedIndoorId > 0 && !string.IsNullOrEmpty(jclabModelType.Text))
                {
                    //选中类型
                    string selectType = jclabModelType.Text.ToString();
                    //判断类型
                    #region
                    bool IsControls = false; //Control 类型单独处理
                    if (!string.IsNullOrEmpty(selectType))
                    {
                        for (int v = 0; v < unitRemote.Length; v++)
                        {
                            if (selectType.ToLower() == unitRemote[v].ToString().ToLower())
                            {
                                IsControls = true;
                                break;
                            }
                        }
                    }

                    string columnName = GetAccessoryGroupName(selectType, thisProject.BrandCode); //根据当前的配件类型 获取到对应列名
                    bool isSelectedByType = true;  //IsSelectedByType 选择标准 为True 是按类型为标准，false 为配件名称为标准
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        if (columnName.Contains(Filter) || columnName.Contains(FilterBox))
                        {
                            isSelectedByType = false;
                        }
                    }
                    #endregion
                    if (IsControls)
                    {
                        #region //控制器批量新增
                        bool isMsg = false;
                        string IndSttr = "";
                        List<string> curInd = new List<string>();
                        List<SelectedIDU_ExchangerList> curSelInd = new List<SelectedIDU_ExchangerList>();
                        #region
                        if (selectType == HalfRemoteControl || selectType == RemoteControl || selectType == WirelessRemoteControl || selectType == ReceiverRemoteControl)
                        {
                            //判断是否存在Receiver  Wireless 
                            foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                            {
                                RoomIndoor rindoor = GetIDU_ExchangerDetial(item);
                                if (rindoor != null && rindoor.ListAccessory != null && rindoor.ListAccessory.Count > 0)
                                {
                                    if (rindoor.ListAccessory.FindAll(p => p.Type == ReceiverRemoteControl && p.IsShared == false || p.Type == WirelessRemoteControl && p.IsShared == false).Count > 0)
                                    {
                                        isMsg = true;
                                        IndSttr += rindoor.IndoorName + ",";
                                        curInd.Add(rindoor.IndoorNO.ToString());
                                        curSelInd.Add(item);
                                    }
                                }
                            }
                            //如果室内机已共享，或被共享 踢出已选项
                            for (int i = selIDUandExcList.Count - 1; i >= 0; i--)
                            {
                                SelectedIDU_ExchangerList items = new SelectedIDU_ExchangerList();
                                items.selected_IndoorId = selIDUandExcList[i].selected_IndoorId;
                                items.selected_IsExchanger = selIDUandExcList[i].selected_IsExchanger;
                                RoomIndoor rindoor = GetIDU_ExchangerDetial(items);
                                if (!IsShareRemoteControl(rindoor))
                                {
                                    selIDUandExcList.RemoveAt(i);
                                }
                            }
                        }
                        else
                        {
                            foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                            {
                                RoomIndoor rindoor = GetIDU_ExchangerDetial(item);
                                if (rindoor != null && rindoor.ListAccessory != null && rindoor.ListAccessory.Count > 0)
                                {
                                    if (rindoor.ListAccessory.FindAll(p => p.Type == HalfRemoteControl && p.IsShared == false || p.Type == RemoteControl).Count > 0)
                                    {
                                        isMsg = true;
                                        IndSttr += rindoor.IndoorName + ",";
                                        curInd.Add(rindoor.IndoorNO.ToString());
                                        curSelInd.Add(item);
                                    }
                                }
                            }
                        }
                        #endregion
                        if (isMsg)
                        {
                            if (JCMsg.ShowConfirmOKCancel(Msg.ACCESSORY_ADD_MSG(IndSttr.TrimEnd(','))) == DialogResult.OK)
                            {
                                //过滤已有数据的室内机
                                if (curSelInd.Count > 0)
                                {
                                    foreach (SelectedIDU_ExchangerList sitem in curSelInd)
                                    {
                                        selIDUandExcList.Remove(sitem);
                                    }
                                }
                                isAdd = BatchAddAccessory(isAdd, model, selIDUandExcList);
                            }
                        }
                        else
                        {
                            isAdd = BatchAddAccessory(isAdd, model, selIDUandExcList);
                        }
                        #endregion
                    }
                    else
                    {
                        //批量添加 非控制器 其他配件
                        #region
                        bool isIdentical = false; //判断是否有相同项
                        string indNameItemList = string.Empty;
                        foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                        {
                            RoomIndoor rindoor = GetIDU_ExchangerDetial(item);
                            if (rindoor != null && rindoor.ListAccessory != null)
                            {
                                foreach (Accessory items in rindoor.ListAccessory)
                                {
                                    if ((isSelectedByType && items.Type == selectType) || (!isSelectedByType && items.Model_Hitachi == model))
                                    {
                                        isIdentical = true;
                                        indNameItemList += rindoor.IndoorName + ",";
                                        break;
                                    }
                                }
                            }
                        }
                        if (isIdentical)
                        {
                            //判断是否继续操作 提醒删除
                            if (JCMsg.ShowConfirmOKCancel(Msg.ACCESSORY_ADD(indNameItemList.TrimEnd(','))) == DialogResult.OK)
                            {
                                //执行删除任务 
                                foreach (SelectedIDU_ExchangerList item in selIDUandExcList)
                                {
                                    RoomIndoor rindoors = GetIDU_ExchangerDetial(item);
                                    if (rindoors != null && rindoors.ListAccessory != null && rindoors.ListAccessory.Count > 0)
                                    {
                                        if (isSelectedByType)
                                        {
                                            rindoors.ListAccessory.RemoveAll(c => c.Type == selectType && c.IsShared == false); //删除相同类型的配件   on 20180529 by xyj 
                                        }
                                        else
                                        {
                                            rindoors.ListAccessory.RemoveAll(c => c.Model_Hitachi == model && c.IsShared == false);//  删除相同配件
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        isAdd = BatchAddAccessory(isAdd, model, selIDUandExcList);
                        #endregion
                    }
                }
            }
        }


        /// <summary>
        /// 批量新增配件
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="model"></param>
        /// <param name="selectedList"></param>
        /// <returns></returns>
        private bool BatchAddAccessory(bool isAdd, string model, List<SelectedIDU_ExchangerList> selectedList)
        {

            //继续添加
            foreach (SelectedIDU_ExchangerList item in selectedList)
            {
                RoomIndoor ind = GetIDU_ExchangerDetial(item);
                if (ind != null)
                {
                    if (ind.ListAccessory == null)
                    {
                        ind.ListAccessory = new List<Accessory>();
                    }
                    if (ind.ListAccessory.FindAll(p => p.Model_Hitachi == model && p.IsShared == false).Count == 0)
                    {
                        isAdd = DoBatchAddAccessory(isAdd, model, item.selected_IndoorId, item.selected_IsExchanger);
                    }
                }
            }

            //判断是否存在IndoorId
            bool isIndoorNo = true;
            foreach (SelectedIDU_ExchangerList sel in selIDUandExcList)
            {
                if (sel.selected_IndoorId == selectedIndoorId && sel.selected_IsExchanger == existsExchanger)
                {
                    isIndoorNo = false;
                    break;
                }
            }
            SelectedIDU_ExchangerList sels = new SelectedIDU_ExchangerList();
            sels.selected_IndoorId = selectedIndoorId;
            sels.selected_IsExchanger = existsExchanger;
            if (isIndoorNo)
            {
                selIDUandExcList.Add(sels);
            }
            //刷新行数据
            RefreshRowsCells(selIDUandExcList);
            return isAdd;
        }


        /// <summary>
        /// 刷新配件
        /// </summary>
        /// <param name="list">需要刷新的室内机与热交换器列表</param>
        private void RefreshRowsCells(List<SelectedIDU_ExchangerList> list)
        {
            if (list.Count > 0)
            {
                foreach (SelectedIDU_ExchangerList item in list)
                {
                    RoomIndoor ri = GetIDU_ExchangerDetial(item);
                    if (ri == null) return;
                    int RowIndex = GetRowIndex(ri);
                    if (col == 5 || col == 7)
                    {
                        CurrentCellValue(RowIndex, 5);
                        CurrentCellValue(RowIndex, 7);
                        RefreshHighWall_ReceiverKit(ri);
                    }
                    else if (col == 9 || col == 10)
                    {
                        CurrentCellValue(RowIndex, 9);
                        CurrentCellValue(RowIndex, 10);
                    }
                    else
                    {
                        CurrentCellValue(RowIndex, col);
                    }

                    if (ri.ListAccessory != null)
                    {
                        DataTable dtSelectedAccessory = ConvertAccessoryCount(ri.ListAccessory, (thisProject.BrandCode == "Y") ? ri.IndoorItem.Model_York : ri.IndoorItem.Model_Hitachi, false, ri);
                        if (dtSelectedAccessory != null && dtSelectedAccessory.Rows.Count > 0)
                        {
                            dgvAvailableItems.Rows[RowIndex].Height = GetRowHeight(dtSelectedAccessory);
                        }
                    }
                    else
                    {
                        dgvAvailableItems.Rows[RowIndex].Height = 38;
                    }
                }
                this.panelDetail.Visible = false;
            }
        }

        //批量新增操作
        private bool DoBatchAddAccessory(bool isAdd, string model, int indoorId, bool ISIDU_EXC)
        {
            isAdd = AddAccessoryBatch(indoorId, model, ISIDU_EXC);
            RefreshHighWall_ReceiverKit(GetRoomIndoorDetial(ISIDU_EXC, Convert.ToInt32(indoorId)));
            return isAdd;
        }

        //批量新增操作
        private bool DoBatchAddAccessory(bool isAdd, string model, bool ISIDU_EXC)
        {
            isAdd = AddAccessoryBatch(selectedIndoorId, model, ISIDU_EXC);
            //刷新单元格数据 
            dgvAvailableItems.CurrentCell = dgvAvailableItems.Rows[row].Cells[col];
            CurrentCellValue(row, col);
            if (isAdd)
            {
                this.panelDetail.Visible = false;
            }
            RefreshHighWall_ReceiverKit(GetRoomIndoorDetial(ISIDU_EXC, Convert.ToInt32(selectedIndoorId)));
            return isAdd;
        }

        ///批量新增操作
        private bool AddAccessoryBatch(int indoorId, string model, bool ISIDU_EXC)
        {
            bool isAdd = false;
            RoomIndoor ri = GetRoomIndoorDetial(ISIDU_EXC, Convert.ToInt32(indoorId));
            if (ri == null) return false;
            Accessory item = abll.GetItems(jclabModelType.Text.ToString(), model, ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
            if (item != null)
            {
                int Count = GetCount(item, ri);
                if (Count + Convert.ToInt32(txtNumber.Text) > item.MaxNumber)
                {
                    JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                    return false;
                }
                for (int i = 1; i <= Convert.ToInt32(txtNumber.Text); i++)
                {
                    isAdd = true;
                    DoAddToSelectedItems(item, ri);
                }
            }
            return isAdd;
        }

        //配件关联 新增对应的室内机配件
        private void DoAddToSelectedItems(Accessory item, RoomIndoor ri)
        {
            if (ri.ListAccessory == null)
            {
                ri.ListAccessory = new List<Accessory>();
            }
            int Count = GetCount(item, ri);
            if (item.MaxNumber >= 2)
            {
                int maxNum = GetMaxNumCompactWithSelected(item, ri); //由于同类型不同Model的最大数量可能不同
                if (Count < maxNum)
                {
                }
                else
                {
                    JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                    return;
                }
            }
            ri.ListAccessory.Add(item);
            string newType = GetNewTypeName(item.Type, ri);
            if (!string.IsNullOrEmpty(newType))
            {
                Accessory newItem = abll.GetItem(newType, ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                if (newItem != null)
                {
                    if (!HasItem(newItem, ri))
                    {
                        ri.ListAccessory.Add(newItem);
                    }
                }
                CurrentCellValue(dgvAvailableItems.CurrentCell.RowIndex, GetColumnsNameIndex(newType, thisProject.BrandCode));
            }
        }

        private void DoAddToSelectedItem(Accessory item, RoomIndoor ri)
        {
            if (ri.ListAccessory == null)
            {
                ri.ListAccessory = new List<Accessory>();
            }
            string newType = GetNewTypeName(item.Type, ri);
            if (!string.IsNullOrEmpty(newType))
            {
                Accessory newItem = abll.GetItem(newType, ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                if (newItem != null)
                {
                    if (!HasItem(newItem, ri))
                    {
                        ri.ListAccessory.Add(newItem);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定类型的附件的已选数量
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetCount(Accessory item, RoomIndoor ri)
        {
            int count = 0;
            //不同类型共用同一最大数量限制处理
            count = ChkTypeLimitCount(item.Type, ri);
            if (count > 0) return count;
            if (ri.ListAccessory != null)
            {
                foreach (Accessory items in ri.ListAccessory)
                {
                    if (items.Type == item.Type)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 对比已选附件，获取指定附件的最大可选数量
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetMaxNumCompactWithSelected(Accessory item, RoomIndoor ri)
        {
            if (item == null) return 0;

            int maxNum = item.MaxNumber;
            if (ri.ListAccessory != null)
            {
                int mixTypeIndex = GetMixTypeIndex(item.Type);
                List<string> mixTypeList = null;
                if (mixTypeIndex >= 0)
                {
                    mixTypeList = TypeLimit[mixTypeIndex];
                }
                foreach (Accessory r in ri.ListAccessory)
                {
                    if (r.Model_Hitachi.ToString() == item.Model_Hitachi) //相同model的最小数量肯定是相同的，不需要判断
                        continue;
                    if (mixTypeIndex < 0)
                    {
                        //如果不是混合类型，只需要比对Type名称
                        if (r.Type.ToString() == item.Type)
                        {
                            //同类型不同Model，需要判断最大数量
                            int selMaxNum = maxNum;
                            int.TryParse(r.MaxNumber.ToString(), out selMaxNum);
                            if (selMaxNum < maxNum)
                            {
                                maxNum = selMaxNum;
                            }
                        }
                    }
                    else
                    {
                        //混合类型则需要判断已选附件的类型是否是同样的混合类型
                        string selType = r.Type.ToString();
                        if (mixTypeList.IndexOf(selType) >= 0)
                        {
                            //属于同组混合类型不同Model，需要判断最大数量
                            int selMaxNum = maxNum;
                            int.TryParse(r.MaxNumber.ToString(), out selMaxNum);
                            if (selMaxNum < maxNum)
                            {
                                maxNum = selMaxNum;
                            }
                        }
                    }
                }
            }
            return maxNum;
        }

        //获取类型对应的索引
        private int GetMixTypeIndex(string type)
        {
            int index = -1;
            bool flag = false;
            if (string.IsNullOrEmpty(type))
                return index;
            for (int i = 0; i < TypeLimit.Count; i++)
            {
                List<string> list = TypeLimit[i];
                for (int j = 0; j < list.Count; j++)
                {
                    if (type == list[j])
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        /// <summary>
        /// 获取不同类型的统计数量（使用同一最大限制数量）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int ChkTypeLimitCount(string type, RoomIndoor ri)
        {
            int count = 0;
            bool flag = false;
            int m = 0;
            for (int i = 0; i < TypeLimit.Count; i++)
            {
                List<string> list = TypeLimit[i];
                for (int j = 0; j < list.Count; j++)
                {
                    if (type == list[j] || GetNewTypeName(type, ri) == list[j])
                    {
                        flag = true;
                        m = i;
                        break;
                    }
                }
                if (flag == true)
                    break;
            }
            if (flag == true)
            {
                if (ri.ListAccessory != null)
                {
                    foreach (Accessory item in ri.ListAccessory)
                    {
                        var list = TypeLimit[m];
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (item.Type == list[j])
                                count++;
                        }
                    }
                }
            }
            return count;
        }


        /// <summary>
        /// 当前室内机 是否包含配件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ri"></param>
        /// <returns></returns>
        private bool HasItem(Accessory item, RoomIndoor ri)
        {
            if (ri.ListAccessory != null && ri.ListAccessory.Count > 0)
            {
                foreach (Accessory ac in ri.ListAccessory)
                {
                    if (ac.Type == item.Type && ac.Model_Hitachi == item.Model_Hitachi)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 查找绑定项目
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetNewTypeName(string type, RoomIndoor ri)
        {
            string s = "";
            for (int i = 0; i < typeName1.Length; ++i)
            {
                if (typeName1[i] == type)
                {
                    if (unitType[i] == "" || unitType[i] == ri.IndoorItem.Type)
                    {
                        return typeName2[i];
                    }
                }
            }

            for (int i = 0; i < typeName2.Length; ++i)
            {
                if (typeName2[i] == type)
                {
                    if (unitType[i] == "" || unitType[i] == ri.IndoorItem.Type)
                    {
                        return typeName1[i];
                    }
                }
            }
            return s;
        }


        //验证输入框
        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            JCValidateSingle(txtNumber);
        }

        //关闭Control
        private void btnControlCancel_Click(object sender, EventArgs e)
        {
            this.panelControl.Visible = false;
        }

        //重汇panel 样式
        private void panelControl_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelControl.ClientRectangle, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid, Color.FromArgb(0, 88, 131), 5,
                                   ButtonBorderStyle.Solid);
        }

        #region  配件关键词
        string[] typeName1 =
        {
            "Wireless Remote Control Switch",
            //"T-Tube Connecting Kit", 
            //"Deodorant Air Filter", 
            "Long-Life Filter Kit",
            "Long-Life Filter Kit",
            "Long-Life Filter Kit", 
            //"Antibacterial Long Life Air Filter",
            "Antibacterial Long-life Filter",
            "Anti-bacterial Air Filter",
            "Kit for Deodorant Filter (Deodorant Filter)",
            "Antibacterial Long-life Filter",
            "Antibacterial Air Filter"
        };
        string[] typeName2 =
        {
            "Receiver Kit for Wireless Control",//Wireless Receiver Kit
            //"Fresh Air Intake Kit", 
            //"Filter Box",
            "Filter Box",
            "Filter Box",
            "Filter Box",
            //"Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Kit for Deodorant Filter (Filter Box)",
            "Filter Box",
            "Filter Box"

        };
        string[] unitType =
        {
            "",
            //"Four Way Cassette",
            //"Four Way Cassette",
            "Ceiling Suspended",
            "Four Way Cassette",
            "Two Way Cassette",
            //"Two Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Two Way Cassette",
            "Ceiling Suspended (NA)"
        };

        //Remote Control Switch 关联
        List<List<string>> TypeLimit = new List<List<string>>() {
            new List<string>() {
                "Half-size Remote Control Switch",
                "Remote Control Switch",
                "Wireless Remote Control Switch"
                 }
         };


        //Remote Control Switch 关联
        string[] unitRemote =
        {
                "Half-size Remote Control Switch",
                "Remote Control Switch",
                "Receiver Kit for Wireless Control",
                "Wireless Remote Control Switch"
        };

        //High Wall High Wall (w/o EXV)
        string[] HighWallType =
        {
              "High Wall",
              "High Wall (w/o EXV)"
        };

        //配件分类 
        string[] AccUnitType =
        {
                "Receiver Kit",
                "Panel",
                "Filter",
                "Filter Box",
                "Drain-up",
                "Others"
        };

        //超出长度的室内机配件名称
        string[] maxAccessoryType = {
                "抗菌加工高清淨濾網(比色法65%)",
                "Outdoor air inlet kit (single inlet)",
                "Kit for Deodorant Filter (Filter Box)",
                "Indoor wired room temperature sensor",
                "Universal water temperature sensor",
                "Differential pressure overflow valve",
                "Remote temperature sensor (THM4)",
                "Kit for Deodorant Filter (Deodorant Filter)",
                "Receiver kit for wireless remote control",
                "Wired Thermostat  PC-ARFWE for HYDRO FREE",
                "2nd zone mixing kit (Wall mounted model)",
                "Primary Filter Kit + Activated Carbon Filter Kit",
                "Duct connecting flange for outdoor air outlet",
                "Outdoor air inlet T-shaped duct connection kit",
                "3-way valve (Internal thread and spring return)",
                "Receiver kit for wireless remote control(On the wall)",
                "Receiver kit for wireless remote control(On the panel)",
                "Wireless ON/OFF thermostat (Receiver + Room thermostat)",
                "Flexible water pipe for a high temperature HYDRO FREE"
         };
        #endregion


        /// <summary>
        /// 共享配件 保存操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnControlOK_Click(object sender, EventArgs e)
        {

            bool isTrue = false;
            string itemtype = RemoteControl;
            if (jccmbControlType.Text == "PC-ARH")
            {
                itemtype = HalfRemoteControl;
            }
            else if (jccmbControlType.Text == "PC-AWR"|| jccmbControlType.Text.Contains("PC-AWR"))
            {
                itemtype = WirelessRemoteControl;
            }

            if (selectedIndoorId == 0 || string.IsNullOrEmpty(CurrentColumnName)) return;

            //验证共享数量
            int mcount = 0;
            for (int m = 0; m < cklistIndoor.Items.Count; m++)
            {
                if (cklistIndoor.GetItemChecked(m))
                    mcount = mcount + 1;
            }
            if (itemtype == WirelessRemoteControl)
            {
                if (mcount > 3)
                {
                    JCMsg.ShowWarningOK(Msg.ACCESSORY_EXCEEDLIMITATION(4));
                    return;
                }
            }
            else
            {
                if (mcount > 15)
                {
                    JCMsg.ShowWarningOK(Msg.ACCESSORY_EXCEEDLIMITATION(16));
                    return;
                }
            }
            if (CurrentColumnName.Contains(ShareRemoteControl))
            {
                bool isAdd = true;
                if (ShareOption(jccmbControlType.Text, isAdd))
                {
                    #region  //新增共享
                    RoomIndoor ri = GetRoomIndoorDetial(existsExchanger, Convert.ToInt32(selectedIndoorId));
                    if (ri != null)
                    {
                        ri.IsMainIndoor = true;
                        Accessory item = abll.GetItems(itemtype, jccmbControlType.Text, ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                        if (item != null)
                        {
                            //判断是否已存在共享Remote Control Switch
                            if (ri.ListAccessory != null)
                            {
                                Accessory ac = ri.ListAccessory.Find(p => p.IsShared == true);
                                if (ac == null)
                                {
                                    item.IsShared = true;
                                    ri.ListAccessory.Add(item);
                                    if (itemtype == WirelessRemoteControl)
                                    {
                                        DoAddToSelectedItem(item, ri);
                                    }
                                    isTrue = true;
                                }
                            }
                        }
                        if (isTrue)
                        {
                            for (int i = 0; i < cklistIndoor.Items.Count; i++)
                            {
                                if (cklistIndoor.GetItemChecked(i))
                                {
                                    // string itemName = cklistIndoor.GetItemText(cklistIndoor.Items[i]);
                                    int IDU_ID = Convert.ToInt32(((CheckBoxItem)cklistIndoor.Items[i]).Value);
                                    string itemName = ((CheckBoxItem)cklistIndoor.Items[i]).Text;
                                    if (itemName.IndexOf("[") > 0)
                                    {
                                        string indName = itemName.Split('[')[0].ToString().Trim();
                                        RoomIndoor indoor = thisProject.RoomIndoorList.Find(p => p.IndoorName == indName && p.IndoorNO == IDU_ID);
                                        if (indoor != null)
                                        {
                                            indoor.IndoorItemGroup = null;
                                            if (ri.IndoorItemGroup == null)
                                            {
                                                ri.IndoorItemGroup = new List<RoomIndoor>();
                                            }
                                            ri.IndoorItemGroup.Add(indoor);
                                            if (itemtype == WirelessRemoteControl)
                                            {
                                                DoAddToSelectedItem(item, indoor);
                                            }
                                        }
                                        RoomIndoor exc = thisProject.ExchangerList.Find(p => p.IndoorName == indName && p.IndoorNO == IDU_ID);
                                        if (exc != null)
                                        {
                                            exc.IndoorItemGroup = null;
                                            if (ri.IndoorItemGroup == null)
                                            {
                                                ri.IndoorItemGroup = new List<RoomIndoor>();
                                            }
                                            ri.IndoorItemGroup.Add(exc);
                                            if (itemtype == WirelessRemoteControl)
                                            {
                                                DoAddToSelectedItem(item, indoor);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    #endregion  
                }
                else
                {
                    #region  //填充共享
                    bool isExc = false;
                    int indId = GetEditShareRemoteControlIndoorNo(jccmbControlType.Text, out isExc);
                    RoomIndoor ri = GetRoomIndoorDetial(existsExchanger, Convert.ToInt32(selectedIndoorId));
                    RoomIndoor riList = GetRoomIndoorDetial(isExc, Convert.ToInt32(indId));
                    if (riList.IndoorItemGroup == null)
                    {
                        riList.IndoorItemGroup = new List<RoomIndoor>();
                    }
                    string controlType = jccmbControlType.Text;
                    if (itemtype == WirelessRemoteControl)
                    {
                        controlType = controlType.Split('(')[0].ToString().Trim();
                    }
                    Accessory item = abll.GetItems(itemtype, controlType, ri.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                    int selectdcount = 0;
                    for (int i = 0; i < cklistIndoor.Items.Count; i++)
                    {
                        if (cklistIndoor.GetItemChecked(i))
                        {
                            selectdcount = selectdcount + 1;
                        }
                    }
                    if (itemtype == WirelessRemoteControl)
                    {
                        if (riList.IndoorItemGroup.Count + 1 + 1 + selectdcount > 4) // 共享控制器+当前主Indoor+当前需要共享的数量 on 20180306 by xyj
                        {
                            JCMsg.ShowWarningOK(Msg.ACCESSORY_EXCEEDLIMITATION(4));
                            return;
                        }
                    }
                    else
                    {
                        if (riList.IndoorItemGroup.Count + 1 + 1 + selectdcount > 16) // 共享控制器+当前主Indoor+当前需要共享的数量 on 20180306 by xyj
                        {
                            JCMsg.ShowWarningOK(Msg.ACCESSORY_EXCEEDLIMITATION(16));
                            return;
                        }
                    }
                    if (itemtype == WirelessRemoteControl)
                    {
                        DoAddToSelectedItem(item, ri);
                        CurrentCellValue(GetRowIndex(ri), 7);
                    }
                    riList.IndoorItemGroup.Add(ri);
                    if (itemtype == WirelessRemoteControl)
                    {
                        RefreshHighWall_ReceiverKit(ri);
                    }
                    isTrue = true;
                    if (isTrue)
                    {
                        for (int i = 0; i < cklistIndoor.Items.Count; i++)
                        {
                            if (cklistIndoor.GetItemChecked(i))
                            {
                                //  string itemName = cklistIndoor.GetItemText(cklistIndoor.Items[i]);
                                int IDU_ID = Convert.ToInt32(((CheckBoxItem)cklistIndoor.Items[i]).Value);
                                string itemName = ((CheckBoxItem)cklistIndoor.Items[i]).Text;
                                if (itemName.IndexOf("[") > 0)
                                {
                                    string indName = itemName.Split('[')[0].ToString().Trim();
                                    RoomIndoor indoor = thisProject.RoomIndoorList.Find(p => p.IndoorName == indName && p.IndoorNO == IDU_ID);
                                    if (indoor != null)
                                    {
                                        indoor.IndoorItemGroup = null;
                                        if (riList.IndoorItemGroup == null)
                                        {
                                            riList.IndoorItemGroup = new List<RoomIndoor>();
                                        }
                                        riList.IndoorItemGroup.Add(indoor);
                                        if (itemtype == WirelessRemoteControl)
                                        {
                                            DoAddToSelectedItem(item, indoor);
                                            CurrentCellValue(GetRowIndex(indoor), 7);
                                        }
                                        ShareCurrentCellValue(indoor);
                                    }
                                    RoomIndoor exc = thisProject.ExchangerList.Find(p => p.IndoorName == indName && p.IndoorNO == IDU_ID);
                                    if (exc != null)
                                    {
                                        exc.IndoorItemGroup = null;
                                        if (riList.IndoorItemGroup == null)
                                        {
                                            riList.IndoorItemGroup = new List<RoomIndoor>();
                                        }
                                        riList.IndoorItemGroup.Add(exc);
                                        if (itemtype == WirelessRemoteControl)
                                        {
                                            DoAddToSelectedItem(item, indoor);
                                            CurrentCellValue(GetRowIndex(indoor), 7);
                                        }
                                        ShareCurrentCellValue(exc);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            if (isTrue)
            {
                //刷新Share remote control 单元格数据  
                this.panelControl.Visible = false;
                if (existsExchanger)
                {
                    ShareCurrentCellValue(thisProject.ExchangerList.Find(p => p.IndoorNO == Convert.ToInt32(selectedIndoorId)));
                }
                else
                {
                    ShareCurrentCellValue(thisProject.RoomIndoorList.Find(p => p.IndoorNO == Convert.ToInt32(selectedIndoorId)));
                }
            }
        }

        //切换Control类型
        private void jccmbControlType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int IndoorId = Convert.ToInt32(dgvAvailableItems.Rows[row].Cells[0].Value.ToString());
            bool isExchanger = Convert.ToBoolean(dgvAvailableItems.Rows[row].Cells[1].Value);
            RoomIndoor ri = GetRoomIndoorDetial(isExchanger, Convert.ToInt32(IndoorId));
            if (ri != null)
            {
                GetAvailableShareRomoteControlList(jccmbControlType.Text, ri);
            }
        }
         

        //切换配件名称
        private void jccmbAccessoryModel_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (dtAccessorys.Rows.Count > 0)
            {
                foreach (DataRow dr in dtAccessorys.Rows)
                {
                    if (dr["AccessoryModel"].ToString() == jccmbAccessoryModel.Text)
                    {
                        this.txtMaxNumber.Text = dr["Max_Qty"].ToString();
                        this.txtNumber.Text = "1";
                        this.txtNumber.JCMinValue = float.Parse("1");
                        this.txtNumber.JCMaxValue = float.Parse(this.txtMaxNumber.Text);
                        jclabModelTypeName.Text = dr["DisplayType"].ToString();
                        jclabModelType.Text = dr["AccessoryType"].ToString();
                    }
                }
            }
        }

        //判断当前室内机是否存在于mainIndoor
        public bool IsExistsMainIndoor(RoomIndoor ri, out RoomIndoor outIndoor)
        {
            bool isExists = false;//默认不存在
            List<RoomIndoor> list = GetMainIndoorItems();
            outIndoor = new RoomIndoor();
            foreach (RoomIndoor rs in list)
            {
                if (rs.IndoorItemGroup != null)
                {
                    foreach (RoomIndoor rm in rs.IndoorItemGroup)
                    {
                        if (rm.IndoorNO == ri.IndoorNO && rm.IsExchanger == ri.IsExchanger)
                        {
                            outIndoor = rs;
                            return true;
                        }
                    }
                }
            }
            return isExists;
        }
        Point pt;
        private void panelDetail_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int px = Cursor.Position.X - pt.X;
                int py = Cursor.Position.Y - pt.Y;
                panelDetail.Location = new Point(panelDetail.Location.X + px, panelDetail.Location.Y + py);
                pt = Cursor.Position;
            }
        }

        private void panelDetail_MouseDown(object sender, MouseEventArgs e)
        {
            pt = Cursor.Position;
        }

        private void panelControl_MouseDown(object sender, MouseEventArgs e)
        {
            pt = Cursor.Position;
        }

        private void panelControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int px = Cursor.Position.X - pt.X;
                int py = Cursor.Position.Y - pt.Y;
                panelControl.Location = new Point(panelControl.Location.X + px, panelControl.Location.Y + py);
                pt = Cursor.Position;
            }
        }

    }
}
