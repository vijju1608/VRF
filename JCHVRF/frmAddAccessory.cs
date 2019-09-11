//********************************************************************
// 文件名: AccessoryDAL.cs
// 描述: 定义 VRF 项目中的附件DAL类
// 作者: clh
// 创建时间: 2016-2-15
// 修改历史: 
//********************************************************************
/* 需求逻辑描述：
 * 1，IsDefault为true，表示当前附件为机组出厂时标配，若同类型有其他Model的附件，则可以替换；
 * 2，MaxNumber限制了附件的最大可选数量；
 * 3，联动选项：
 * Factory/ IndoorUnitType/ Type/ Model_Hitachi
 * 【SMZ】/。。/【Wireless Remote Control Switch】/【PC-LH3A】--- Can be used for RPI-FSN2, RPF-FSN2E, RPFI-FSN2E only. Use together with Wireless receiver kit.
 * 【SMZ】/。。/【Wireless Remote Control Switch】/【PC-LH3B】--- 【Indoor Unit】，Use together with Wireless receiver kit.
 * 
 * 【SMZ】/【Four Way Cassette】/【T-Tube Connecting Kit】/【TKCI-160K】--- Use together with 【OACI-160K2】（【Fresh Air Intake Kit】）
 * 【SMZ】/【Four Way Cassette】/【Deodorant Air Filter】/【F-71L-D1 | F-160L-D1】--- Use together with 【B-160H2】（【Filter Box】）
 * 【SMZ】/【Medium Static Ducted】/【Filter Box】/【B-15MI3C】--- Use together with F-15LI3C（【Long-life Filter Kit】）
 * 【SMZ】/【High Static Ducted】/【Filter Box】/【B-23MI3 | B-34MI3 | B-46MI3】--- Use together with F-23LI3 | F-34LI3 | F-46LI3（【Long-life Filter Kit】）
 * 【SMZ】/【Two Way Cassette】/【Filter Box】/【B-90HD | B-160HD】--- Use together with F-90MD-K1 | F-160MD-K1（【Antibacterial Long Life Air Filter】）
 * 
 * 【SMZ】/【High Wall】/区分A type【Strainer Kit】/（MSF-NP63A|MSF-NP112A）与B type【Strainer Kit】/（MSF-NP36AH）|【Electronic Expansion Valve Kit】/（EV-1.5N1）
 * 
 * 【HAPQ】/【MAL】/【Drainage Pump】（...QE）|【Panel】（...QE）--- for Malaysia market
 * 
 * ============================================================================
 * 20160618 以上为之前的需求，现在需求发生了很大变动 Yunxiao Lin
 *  Factory/ BrandCode/ IndoorUnitType/ Type/ Model_Hitachi
 * 【SMZ】/【H】/。。/【Wireless Remote Control Switch】/--- Use together with Receiver Kit for Wireless Control【PC-ALH3】
 * 【SMZ】/【H】/【Four Way Cassette】/【Kit for Deodorant Filter (Deodorant Filter)】/【F-160L-D1】 --- Use together with filter box【B-160H2】
 * 【SMZ】/【H】/【Four Way Cassette】/【Antibacterial Long-life Filter】/【F-160L-K】 --- Use together with fiter box 【B-160H2】?
 * 【SMZ】/【H】/【Two Way Cassette】/【Antibacterial Long-life Filter】/【F-90MD-K1】 --- Use together with fiter box 【B-90HD】
 * 【SMZ】/【H】/【Two Way Cassette】/【Antibacterial Long-life Filter】/【F-160MD-K1】 --- Use together with fiter box 【B-160HD】
 * 【SMZ】/【Y】/【Four Way Cassette】/【Anti-bacterial Air Filter】/【F-71M-K2 | F-160M-K2】 --- Use together with fiter box 【B-160H3】
 * 【SMZ】/【Y】/【One Way Cassette】/【Anti-bacterial Air Filter】/【F-56MS-PK2】 --- Use together with fiter box (NO DATA)
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.UI;
using JCHVRF.Const;
using JCHVRF.VRFMessage;
using JCHVRF.VRFTrans;

namespace JCHVRF
{
    public partial class frmAddAccessory : JCBase.UI.JCForm
    {
        #region Init
        Indoor inItem;
        AccessoryBLL bll;
        string brandCode;
        List<RoomIndoor> rinItemList;
        List<RoomIndoor> inItemDisplay;
        Project thisProject;
        DataTable allAvailable;
        int mainIndoor;
        List<RoomIndoor> indoorNameList;
        string airPanelWith = "Air Panel with motion sensor";
        string airPanelWo = "Air Panel w/o motion sensor";

        Trans trans = new Trans();   //翻译初始化
      
        //public frmAddAccessory(Indoor item)
        //{
        //    InitializeComponent();
        //    inItem = item;
        //    bll = new AccessoryBLL();
        //    brandCode = (string.IsNullOrEmpty(item.Model_York) || item.Model_York == "-") ? "H" : "Y";
        //}

        //public frmAddAccessory(List<RoomIndoor> rinItemList, List<RoomIndoor> indItemList, Project thisProject, DataTable allAvailable)
        public frmAddAccessory(List<RoomIndoor> rinItemList, Project thisProject, DataTable allAvailable)
        {
            InitializeComponent();
            inItem = rinItemList[0].IndoorItem;
            this.rinItemList = rinItemList;
            //this.inItemDisplay = indItemList;
            this.inItemDisplay = (from p in rinItemList orderby p.IndoorFullName select p).ToList();  //重新按室内机名称排序 modify by Shen junjie 2018/4/9
            this.thisProject = thisProject;
            this.allAvailable = allAvailable;
            bll = new AccessoryBLL();
            indoorNameList = new List<RoomIndoor>(); 
            //直接取Project 的BrandCode  on 20180815 by xyj 隐藏 brandCode = (string.IsNullOrEmpty(inItem.Model_York) || inItem.Model_York == "-") ? "H" : "Y";
            brandCode = thisProject.BrandCode;
        }

        private void frmAddAccessory_Load(object sender, EventArgs e)
        { 
            this.JCSetLanguage();

            //this.lblHiddenModel_York.Text = inItem.ModelFull;
            //if (brandCode == "Y")
            //    this.jclblModelName.Text = inItem.Model_York;
            //else
            //    this.jclblModelName.Text = inItem.Model_Hitachi;

            BindDisplayInfo();
            BindCmbIndoorItem();
            BindDGVAvailable();
            BindDGVSelected();
            if (rinItemList[0].IsExchanger)
            {
                jclblModelNameLabel.Text = Msg.EXCHANGERNAME_ACCESSORY;
            }
            if (brandCode == "H")
            {
                this.dgvSelectedItems.Columns[Name_Common.SelModel_York].Visible = false;
                this.dgvSelectedItems.Columns[Name_Common.SelModel_Hitachi].Visible = true;
                this.dgvAvailableItems.Columns[Name_Common.ModelFull_York].Visible = false;
                this.dgvAvailableItems.Columns[Name_Common.ModelFull_Hitachi].Visible = true;
            }
            mainIndoor = (int)jccmbIndoorItem.SelectedValue;
        }

        private void BindDGVAvailable()
        {
            this.dgvAvailableItems.AutoGenerateColumns = false;
            NameArray_Accessory nameArr = new NameArray_Accessory();
            Global.SetDGVDataName(ref dgvAvailableItems, nameArr.Accessory_DataName);
            Global.SetDGVName(ref dgvAvailableItems, nameArr.Accessory_Name);
            Global.SetDGVHeaderText(ref dgvAvailableItems, nameArr.Accessory_HeaderText);

            this.dgvAvailableItems.DataSource = allAvailable;
        }

        private void BindDGVSelected()
        {
            this.dgvSelectedItems.AutoGenerateColumns = false;
            NameArray_Accessory nameArr = new NameArray_Accessory();
            Global.SetDGVDataName(ref dgvSelectedItems, nameArr.Accessory_DataName);
            Global.SetDGVName(ref dgvSelectedItems, nameArr.Accessory_Name_Sel);
            Global.SetDGVHeaderText(ref dgvSelectedItems, nameArr.Accessory_HeaderText_Sel);

            if (rinItemList.Count == 1)
            {
                RoomIndoor ri = rinItemList[0];
                if (ri != null && ri.ListAccessory != null)
                {
                    foreach (Accessory item in ri.ListAccessory)
                    {
                        if (item == null) continue;

                        //DoAddToSelectedItems(item);
                        this.dgvSelectedItems.Rows.Add(
                                item.Type,
                                trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type),
                                item.Model_York,
                                item.Model_Hitachi,
                                 "",
                                1,
                                item.MaxNumber,
                                item.IsDefault
                                );
                    }
                }
            }
            else
            {
                //如果多选暂时先全部清空所选配件
                foreach (RoomIndoor rinItem in rinItemList)
                {
                    if (rinItem != null)
                    {
                        rinItem.ListAccessory = null;
                    }
                }
            }
        }
        #endregion

        #region Event
        private void dgvAvailableItems_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = Color.FromArgb(130, 130, 130);
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
                    {
                        border.X += 1;
                    }
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private void dgvSelectedItems_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = Color.FromArgb(130, 130, 130);
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
                    {
                        border.X += 1;
                    }
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
            }
        }

        private void jcbtnSelect_Click(object sender, EventArgs e)
        {
            DoSelected();
        }

        private void dgvAvailableItems_DoubleClick(object sender, EventArgs e)
        {
            DoSelected();
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.dgvSelectedItems.RowCount > 0 && this.dgvSelectedItems.SelectedRows.Count > 0)
            {
                DataGridViewRow row = this.dgvSelectedItems.SelectedRows[0];
                string type = row.Cells[Name_Common.SelType].Value.ToString();

                if (rinItemList[0].IndoorItemGroup != null)
                {
                    if (rinItemList[0].IndoorItemGroup.Count != 0)
                    {
                        if (type.Equals("Remote Control Switch") || type.Equals("Half-size Remote Control Switch"))
                        {
                            if (deleteSharingRelationShip())
                            {
                                deleteAccessory(type, row);
                                uc_CheckBox_Sharing_RemoteController.Checked = false;
                                jccmbIndoorItem.Visible = false;
                            }
                        }
                        else
                        {
                            if (JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL) == DialogResult.OK)
                            {
                                deleteAccessory(type, row);
                            }
                        }
                            
                    }
                }
                else
                {
                    //需要判断是否存在默认绑定的Air panel w/o motion sensor
                    bool isDefault = GetDefaultAirPanel();
                    #region //删除ANZ Air panel with motion sensor 需要还原 Air panel w/o motion sensor on 20180810 by xyj
                    if (inItem.Type == "Four Way Cassette" && type == airPanelWith && isDefault)
                    {
                        if (JCMsg.ShowConfirmOKCancel(Msg.DEL_ACCESSORY_AIRPANEL(airPanelWo)) == DialogResult.OK)
                        {
                            deleteAccessory(type, row); //删除Air panel with motion sensor
                            //还原 Air panel w/o motion sensor
                            Accessory acitem = inItem.ListAccessory.Find(p => p.IsDefault == true && p.Type == airPanelWo);
                            if (acitem != null)
                            {
                                if (!HasItem(acitem))
                                {
                                    this.dgvSelectedItems.Rows.Add(
                                    acitem.Type,
                                    trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), acitem.Type),
                                    acitem.Model_York,
                                    acitem.Model_Hitachi,
                                    "",
                                    1,
                                    acitem.MaxNumber,
                                    acitem.IsDefault
                                    );
                                }
                            }
                        }
                        return;
                    }

                    #endregion

                    if (JCMsg.ShowConfirmOKCancel(JCMsg.CONFIRM_DEL) == DialogResult.OK)
                    {
                        if (type.Equals("Remote Control Switch") || type.Equals("Half-size Remote Control Switch"))
                        {
                            uc_CheckBox_Sharing_RemoteController.Checked = false;
                            jccmbIndoorItem.Visible = false;
                        }
                        deleteAccessory(type, row);
                    }
                }  
            }
            else
            {
                JCMsg.ShowWarningOK(JCMsg.WARN_SELECTONE);
            }
        }

        /// <summary>
        /// 获取室内机默认绑定的AirPanel
        /// </summary>
        /// <returns></returns>
        private bool GetDefaultAirPanel()
        {
            foreach (DataRow dr in allAvailable.Rows)
            {
                if (dr["Type"].ToString() == airPanelWo && dr["IsDefault"].ToString() == "True")
                {
                    return true;
                }
            }
            return false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (dgvSelectedItems.Rows.Count > 0)
            {
                DialogResult res = JCMsg.ShowConfirmOKCancel(Msg.IDU_ACCESSORYE_CLEAR);
                // 可以清空配件
                if (res == DialogResult.OK)
                {

                    if (rinItemList[0].IsMainIndoor || rinItemList[0].IndoorItemGroup != null)
                    {
                        if (!deleteSharingRelationShip())    //如果共享室内机成员清空配件，则解绑所有关联关系
                            return;
                    }
                    uc_CheckBox_Sharing_RemoteController.Checked = false;
                    jccmbIndoorItem.Visible = false;

                    this.dgvSelectedItems.Rows.Clear();
                    List<Accessory> list = (new AccessoryBLL()).GetDefault(inItem, thisProject.RegionCode, thisProject.SubRegionCode, inItem.Series);
                    if (list != null)
                    {
                        foreach (Accessory item in list)
                        {
                            //DoAddToSelectedItems(item);
                            this.dgvSelectedItems.Rows.Add(
                                    item.Type,
                                    trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type),
                                    item.Model_York,
                                    item.Model_Hitachi,
                                    "",
                                    1,
                                    item.MaxNumber,
                                    item.IsDefault
                                    );
                        }
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DoOK();
            Close();
            UndoRedo.UndoRedoHandler.SaveHistoryTraces();//保存历史痕迹 add by axj 20161228
        }
        #endregion

        /// OK按钮，将已选附件与当前室内机绑定
        /// <summary>
        /// 点击OK按钮，将已选附件与当前室内机绑定
        /// </summary>
        private void DoOK()
        {
            if (uc_CheckBox_Sharing_RemoteController.Checked)   //共享控制器
            {
                foreach (RoomIndoor rindItem in rinItemList)
                {
                    if (rindItem.IndoorNO.Equals(mainIndoor))   //指定共享控制器主室内机
                        rindItem.IsMainIndoor = true;
                    else
                        rindItem.IsMainIndoor = false;

                    if (dgvSelectedItems.RowCount > 0)
                    {
                        List<Accessory> list = new List<Accessory>();
                        foreach (DataGridViewRow r in dgvSelectedItems.Rows)
                        {
                            string AccessoryModel = (brandCode == "H" ? r.Cells[Name_Common.SelModel_Hitachi].Value.ToString() : r.Cells[Name_Common.SelModel_York].Value.ToString());
                            Accessory item = bll.GetItems(r.Cells[Name_Common.SelType].Value.ToString(), AccessoryModel, rindItem.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                            if (item != null)
                            {
                                //如果控制器模块分配给主室内机
                                if (item.Type.Equals("Remote Control Switch") || item.Type.Equals("Half-size Remote Control Switch"))
                                {
                                    if (rindItem.IsMainIndoor)
                                        list.Add(item);
                                }
                                else
                                    list.Add(item);
                            }
                        }
                        rindItem.ListAccessory = list;
                    }
                    else
                        rindItem.ListAccessory = null;

                    rindItem.IndoorItemGroup = indoorNameList;
                    
                }
            
            }
            else
            {
                foreach (RoomIndoor rindItem in rinItemList)
                {
                    if (dgvSelectedItems.RowCount > 0)
                    {
                        List<Accessory> list = new List<Accessory>();
                        foreach (DataGridViewRow r in dgvSelectedItems.Rows)
                        {
                            string AccessoryModel = (brandCode == "H" ? r.Cells[Name_Common.SelModel_Hitachi].Value.ToString() : r.Cells[Name_Common.SelModel_York].Value.ToString());
                            Accessory item = bll.GetItems(r.Cells[Name_Common.SelType].Value.ToString(), AccessoryModel, rindItem.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                            if (item != null)
                            {
                                if (addReceiver(list,item))
                                  list.Add(item);
                            }
                        }
                        rindItem.ListAccessory = list;
                    }
                    else
                        rindItem.ListAccessory = null;

                    if (rindItem.IndoorItemGroup != null && rindItem.IndoorItemGroup.Count != 0)   //如果不是共享remoteController就不存在Group概念
                        rindItem.IndoorItemGroup = null;
                }
            
            }            
            
        }
        
        //判断是否可以继续新增Receiver Kit for Wireless Control
        private bool addReceiver(List<Accessory> list,Accessory item)
        {
            foreach (Accessory acitem in list)
            {
                if (acitem.Type == "Receiver Kit for Wireless Control"&&acitem.Type==item.Type)
                {
                    return false;
                }
            }
            return true;
        }

        /// 双击可选数据行，或点击选择按钮
        /// <summary>
        /// 双击可选数据行，或点击选择按钮
        /// </summary>
        private void DoSelected()
        {
            if (this.dgvAvailableItems.RowCount > 0 && this.dgvAvailableItems.SelectedRows.Count > 0)
            {
                DataGridViewRow row = this.dgvAvailableItems.SelectedRows[0];
                string type = row.Cells[Name_Common.Type].Value.ToString();
                string model_Hitachi = (brandCode == "H" ? row.Cells[Name_Common.ModelFull_Hitachi].Value.ToString() : row.Cells[Name_Common.ModelFull_York].Value.ToString());
                Accessory item = bll.GetItems(type, model_Hitachi, inItem, thisProject.RegionCode, thisProject.SubRegionCode);
                if(item!=null)
                    DoAddToSelectedItems(item);
            }

        }

        /// 将附件对象插入到已选附件列表
        /// <summary>
        /// 将附件对象插入到已选附件列表
        /// </summary>
        /// <param name="item"></param>
        private void DoAddToSelectedItems(Accessory item)
        {
            bool isNewRow = true;
            if (!ChkSpecial(item.Type))
            {
                return;
            }
            if (this.dgvSelectedItems.RowCount > 0)
            {
                foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
                {
                    if (r.Cells[Name_Common.SelType].Value.ToString().ToLower() == item.Type.ToLower())
                    {
                        // 不需要新增项目
                        isNewRow = false;

                        // 同类型，看数量限制
                        if (item.MaxNumber == 1)
                        {
                            if (r.Cells[Name_Common.SelModel_Hitachi].Value.ToString() == item.Model_Hitachi)
                            {
                                JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                                break;
                            }
                            else
                            {
                                int count = GetCount(item);
                                if (count == 1)
                                {
                                    // 同类型不同Model，则替换Model_York & Model_Hitachi
                                    r.Cells[Name_Common.SelModel_York].Value = item.Model_York;
                                    r.Cells[Name_Common.SelModel_Hitachi].Value = item.Model_Hitachi;
                                }
                                else
                                {
                                    JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                                    break;
                                }
                            }
                        }
                        else if (item.MaxNumber >= 2)
                        {
                            int count = GetCount(item);
                            int maxNum = GetMaxNumCompactWithSelected(item); //由于同类型不同Model的最大数量可能不同，需要计算 20170417 by Yunxiao Lin
                            //if (count < item.MaxNumber)
                            if (count < maxNum)
                            {
                                isNewRow = true;
                            }
                            else
                            {
                                JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                                break;
                            }
                        }
                    }
                }
            } 

            if (isNewRow)
            {
                //ANZ 需要特殊处理Four Way Cassette(RCI)系列 Air panel with motion sensor, Air panel w/o motion sensor   on20180810 by xyj 
                //Air panel with motion sensor 与 Air panel w/o motion sensor 可以替换 
                if (inItem.Type == "Four Way Cassette")
                {
                    if (item.Type == airPanelWith)
                    {
                        UpdateAirPanel(item, inItem, airPanelWo, out isNewRow);
                        if(!isNewRow)
                            return;
                    }
                    else if (item.Type == airPanelWo)
                    {
                        UpdateAirPanel(item, inItem, airPanelWith, out isNewRow);
                        if (!isNewRow)
                            return;
                    }
                }

                //add axj 20170704 begin 不同类型共用同一最大数量限制处理
                int count = GetCount(item);
                int maxNum = GetMaxNumCompactWithSelected(item); //最大数量需要根据当前附件和已选附件判断，选最小的一个值。
                if (count < maxNum)
                {
                }
                else
                {
                    JCMsg.ShowWarningOK(Msg.Accessory_Warn_Number);
                    return;
                }
                //add axj 20170704 end
                
                this.dgvSelectedItems.Rows.Add(
                            item.Type,
                            trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type),
                            item.Model_York,
                            item.Model_Hitachi,
                            "",
                            1,
                            item.MaxNumber,
                            item.IsDefault
                            );
                string newType = GetNewTypeName(item.Type);
                if (!string.IsNullOrEmpty(newType))
                {
                    foreach (RoomIndoor ind in rinItemList)
                    {

                        Accessory newItem = bll.GetItem(newType, ind.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                        if (newItem != null)
                        {
                            //add xyj 20180207
                            string Description = SameAccessoryName(newItem, newType); //获取室内机名称通过相同配件  
                            if (!HasItem(newItem))
                            {
                                this.dgvSelectedItems.Rows.Add(
                                newItem.Type,
                                trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), newItem.Type),
                                newItem.Model_York,
                                newItem.Model_Hitachi,
                                Description,
                                1,
                                item.MaxNumber,
                                item.IsDefault
                                );
                                //JCMsg.ShowWarningOK("当前所选类型的附件必须与[" + newType + "]类型的附件搭配使用！");
                                //DoAddToSelectedItems(newItem);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否存在当前类型 
        /// </summary>
        /// <param name="airPanel"></param>
        /// <returns></returns>
        private bool ExistsType(string airPanel)
        {
            bool isTrue = false;
            //判断是否存在当前类型
            if (dgvSelectedItems.Rows.Count > 0)
            {
                for (int i = 0; i < dgvSelectedItems.Rows.Count; i++)
                {
                    if (dgvSelectedItems.Rows[i].Cells[Name_Common.SelType].Value.ToString() == airPanel)
                    {
                        return true;
                    }
                }
            }
            return isTrue;
        }

        private int UpdateAirPanel(Accessory item, Indoor ind, string airPanel,out bool isNewRow)
        {
            int id = 0;
            isNewRow = true; 
            if (ExistsType(airPanel))
            {
                //if (JCMsg.ShowConfirmOKCancel(Msg.UPDATE_ACCESSORY_AIRPANEL) == DialogResult.OK)
                //{
                    isNewRow = false;
                    //先删除当前配件
                    for (int i = dgvSelectedItems.Rows.Count - 1; i >= 0; i--)
                    {
                        id = i;
                        if (dgvSelectedItems.Rows[i].Cells[Name_Common.SelType].Value.ToString() == airPanel)
                            dgvSelectedItems.Rows.RemoveAt(i);
                    }
                    //替换
                    if (!HasItem(item))
                    {
                        this.dgvSelectedItems.Rows.Add(
                        item.Type,
                        trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type),
                        item.Model_York,
                        item.Model_Hitachi,
                        "",
                        1,
                        item.MaxNumber,
                        item.IsDefault
                        );
                    }
                //}
                //else {
                //    isNewRow = false;
                //}
            } 
            return id;
        }

        //获取室内机名称通过相同配件 
        public string SameAccessoryName(Accessory item, string newType)
        {
            string result = string.Empty;
            if (rinItemList.Count > 1)
            {
                foreach (RoomIndoor ind in rinItemList)
                {
                    Accessory newItems = bll.GetItem(newType, ind.IndoorItem, thisProject.RegionCode, thisProject.SubRegionCode);
                    if (newItems != null)
                    {
                        if (newItems.Type == item.Type && newItems.Model_Hitachi == item.Model_Hitachi && newItems.Model_York == item.Model_York)
                            result += ind.IndoorName + ",";
                    }
                }
            }
            if (result.Length > 0)
                return result.TrimEnd(',');
            else
                return result;
        }

        private bool HasItem(Accessory item)
        {
            foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
            {
                if (r.Cells[Name_Common.SelType].Value.ToString().ToLower() == item.Type.ToLower() && r.Cells[Name_Common.SelModel_Hitachi].Value.ToString() == item.Model_Hitachi)
                    return true;
            }
            return false;
        }
        /// 如果当前附件类型为混合类型(共用数量限制的类型)，返回混合类型下标，否则返回-1 20170417 by Yunxiao Lin
        /// <summary>
        /// 如果当前附件类型为混合类型(共用数量限制的类型)，返回混合类型下标，否则返回-1
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
        /// 对比已选附件，获取指定附件的最大可选数量 20170417 by Yunxiao Lin
        /// <summary>
        /// 对比已选附件，获取指定附件的最大可选数量
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetMaxNumCompactWithSelected(Accessory item)
        {
            if (item == null)
                return 0;
            int maxNum = item.MaxNumber;
            if (this.dgvSelectedItems.RowCount > 0)
            {
                int mixTypeIndex = GetMixTypeIndex(item.Type);
                List<string> mixTypeList = null;
                if (mixTypeIndex >= 0)
                    mixTypeList = TypeLimit[mixTypeIndex];
                foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
                {
                    if (r.Cells[Name_Common.SelModel_Hitachi].Value.ToString() == item.Model_Hitachi) //相同model的最小数量肯定是相同的，不需要判断
                        continue;
                    if (mixTypeIndex < 0)
                    {
                        //如果不是混合类型，只需要比对Type名称
                        if (r.Cells[Name_Common.SelType].Value.ToString().ToLower() == item.Type.ToLower())
                        {
                                //同类型不同Model，需要判断最大数量
                                int selMaxNum = maxNum;
                                int.TryParse(r.Cells[Name_Common.SelMaxNumber].Value.ToString(), out selMaxNum);
                                if (selMaxNum < maxNum)
                                    maxNum = selMaxNum;
                        }
                    }
                    else
                    {
                        //混合类型则需要判断已选附件的类型是否是同样的混合类型
                        string selType = r.Cells[Name_Common.SelType].Value.ToString();
                        if (mixTypeList.IndexOf(selType) >= 0)
                        {
                            //属于同组混合类型不同Model，需要判断最大数量
                            int selMaxNum = maxNum;
                            int.TryParse(r.Cells[Name_Common.SelMaxNumber].Value.ToString(), out selMaxNum);
                            if (selMaxNum < maxNum)
                                maxNum = selMaxNum;
                        }
                    }
                }
            }
            return maxNum;
        }


        /// 获取指定类型的附件的已选数量
        /// <summary>
        /// 获取指定类型的附件的已选数量
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetCount(Accessory item)
        {
            int count = 0;
            //int maxNum = 0;

            //#region  针对同一类型的机型取MaxNum最小值作为匹配数  add on 20170417 by Lingjia Qiu
            //if (this.dgvSelectedItems.RowCount > 0)            
            //{
            //    foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
            //    {
                   

            //        for (int i = 0; i < TypeLimit.Count; i++)
            //        {
            //            maxNum = 0;
            //            List<string> list = TypeLimit[i];
            //            for (int j = 0; j < list.Count; j++)
            //            {
            //                //区分特殊机型的maxNum
            //                if (item.Type == list[j] || GetNewTypeName(item.Type) == list[j])
            //                {
            //                    if (Convert.ToInt32(r.Cells[Name_Common.SelMaxNumber].Value) < item.MaxNumber)
            //                        maxNum = Convert.ToInt32(r.Cells[Name_Common.SelMaxNumber].Value);
            //                    else
            //                        maxNum = item.MaxNumber;
                                
            //                }
            //             }

            //            //区分相同机型的maxNum
            //            if (r.Cells[Name_Common.SelType].Value.ToString() == item.Type)
            //            {
            //                if (Convert.ToInt32(r.Cells[Name_Common.SelMaxNumber].Value) < item.MaxNumber)
            //                    maxNum = Convert.ToInt32(r.Cells[Name_Common.SelMaxNumber].Value);
            //                else
            //                    maxNum = item.MaxNumber;
            //            }
               
            //         }
            //    }
                
            //}

            ////去最小的MaxNum配对
            //if (maxNum != 0)
            //{
            //    if (this.dgvSelectedItems.RowCount == maxNum || this.dgvSelectedItems.RowCount > maxNum)
            //        return 3;
            //}
            //#endregion

            //add axj 20170704 begin 不同类型共用同一最大数量限制处理
            count = ChkTypeLimitCount(item.Type);
            if (count > 0)
            {
                return count;
            }
            //add axj 20170704 end
            if (this.dgvSelectedItems.RowCount > 0)
            {
                foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
                {
                    if (r.Cells[Name_Common.SelType].Value.ToString() == item.Type)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 获取不同类型的统计数量（使用同一最大限制数量）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int ChkTypeLimitCount(string type)
        {
            int count = 0;
            bool flag = false;
            int m = 0;
            for (int i = 0; i < TypeLimit.Count; i++)
            {
                List<string> list = TypeLimit[i];
                for (int j = 0; j < list.Count; j++)
                {
                    if (type == list[j] || GetNewTypeName(type) == list[j])
                    {
                        flag = true;
                        m = i;
                        break;
                    }
                }
                if (flag == true)
                {
                    break;
                }
            }
            //for (int i = 0; i < TypeLimitA.Length; i++)
            //{
            //    if (type == TypeLimitA[i] || type == TypeLimitB[i] || type == TypeLimitC[i])
            //    {
            //        flag = true;
            //        m = i;
            //        break;
            //    }
            //}
            if (flag == true)
            {
                foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
                {
                    var list = TypeLimit[m];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (r.Cells[Name_Common.SelType].Value.ToString() == list[j])
                        {
                            count++;
                        }
                    }
                    //if (r.Cells[Name_Common.SelType].Value.ToString() == TypeLimitA[m] || r.Cells[Name_Common.SelType].Value.ToString() == TypeLimitB[m] || r.Cells[Name_Common.SelType].Value.ToString() == TypeLimitC[m])
                        //count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 查找绑定项目
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetNewTypeName(string type)
        {
            string s = "";
            for (int i = 0; i < typeName1.Length; ++i)
            {
                if (typeName1[i] == type)
                    if (unitType[i] == "" || unitType[i] == inItem.Type)
                        return typeName2[i];
            }

            for (int i = 0; i < typeName2.Length; ++i)
            {
                if (typeName2[i] == type)
                {
                    if (unitType[i] == "" || unitType[i] == inItem.Type)
                        return typeName1[i];
                }
            }
            return s;
        }
        /// <summary>
        /// 特殊情况处理
        /// </summary>
        /// <returns></returns>
        public bool ChkSpecial(string type)
        {
            if (inItem.Type == "Floor Ceiling")
            {
                if (type == "Remote Control Switch" || type == "Half-size Remote Control Switch")
                {
                    foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
                    {
                        if (r.Cells[Name_Common.SelType].Value.ToString() == "Wireless Remote Control Switch")
                        {
                            return false;
                        }
                    }
                }
                if (type == "Wireless Remote Control Switch")
                {
                    foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
                    {
                        if (r.Cells[Name_Common.SelType].Value.ToString() == "Remote Control Switch" || r.Cells[Name_Common.SelType].Value.ToString() == "Half-size Remote Control Switch")
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        string[] typeName1 =
        {
            "Wireless Remote Control Switch",
            //"T-Tube Connecting Kit", 
            //"Deodorant Air Filter", 
            "Long-life Filter Kit",
            "Long-life Filter Kit", 
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
            "Medium Static Ducted",
            "High Static Ducted",
            //"Two Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Four Way Cassette",
            "Two Way Cassette",
            "Ceiling Suspended (NA)"
        };

        //add axj 20160704
        List<List<string>> TypeLimit =new List<List<string>>() {
            new List<string>() { 
                "Half-size Remote Control Switch", 
                "Remote Control Switch", 
                "Wireless Remote Control Switch"
                 }
         };


        //string[] TypeLimitA = { 
        //    "Half-size Remote Control Switch"
        //};
        //string[] TypeLimitB = { 
        //    "Remote Control Switch"
        //};
        //string[] TypeLimitC = { 
        //    "Wireless Remote Control Switch"
        //};

        /// <summary>
        /// 绑定主室内机的控件
        /// </summary>
        /// <returns></returns>
        private void BindCmbIndoorItem()
        {
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    this.jccmbProductType.ValueMember = "ProductType";
            //    this.jccmbProductType.DisplayMember = "Series";
            //    this.jccmbProductType.DataSource = dt;
            //    if (this.JCFormMode == FormMode.EDIT)
            //    {
            //        this.jccmbProductType.SelectedValue = curRI.IndoorItem.ProductType;
            //        this.jccmbProductType.Enabled = false;
            //    }
            //    else
            //        this.jccmbProductType.SelectedValue = thisProject.ProductType;
            //    _productType = this.jccmbProductType.SelectedValue.ToString();
            //    _series = this.jccmbProductType.Text;
            //}
          
            if (inItemDisplay != null && inItemDisplay.Count > 0)
            {
                jccmbIndoorItem.ValueMember = "IndoorNO";
                jccmbIndoorItem.DisplayMember = "IndoorFullName";
                jccmbIndoorItem.DataSource = inItemDisplay;
                jccmbIndoorItem.SelectedIndex = 0;
            }

        }

        private void jclblNameMore_MouseEnter(object sender, EventArgs e)
        {
            jclblNameMore.BackColor = UtilColor.bg_selected;
            jclblNameMore.ForeColor = UtilColor.font_selected; 

        }

        private void jclblNameMore_MouseLeave(object sender, EventArgs e)
        {
            jclblNameMore.BackColor = jclblModelName.BackColor;
            jclblNameMore.ForeColor = jclblModelName.ForeColor;            
        }

        /// <summary>
        /// 已选室内机名字的显示及共享控制器控件显示
        /// </summary>
        /// <returns></returns>
        private void BindDisplayInfo()
        {         
            int i = 0 ;
            jclblNameMore.Visible = false;
            string modelName = "";
            foreach (RoomIndoor rindItem in rinItemList)
            {
                if (i < 2)
                    //modelName += indItem.IndoorName.Split('[')[0] + ",";
                    modelName += rindItem.IndoorFullName + " ,";

                indoorNameList.Add(rindItem);   //维护已选择的indoorName
                if (rinItemList.Count == 1 || rinItemList.Count > 16 || (inItem.Series != null && rindItem.IndoorItem.Series != null && !inItem.Series.Equals(rindItem.IndoorItem.Series))) //增加Series非空判断 20180728 by Yunxiao Lin
                    uc_CheckBox_Sharing_RemoteController.Visible = false;
                //if (dgvSelectedItems.Rows.Count == 0)   //如果只有一个选项，不存在共享控制器
                    //uc_CheckBox_Sharing_RemoteController.Enabled = false;                

                i++;
            }         
            modelName = modelName.Substring(0, modelName.Length - 1);
            if (modelName.Length > 38 && i > 2)
                jclblModelName.Text = modelName.Substring(0, 38);
            else
                jclblModelName.Text = modelName;

            if(i > 2)
                jclblNameMore.Visible = true;
        }

        private void jclblNameMore_MouseClick(object sender, MouseEventArgs e)
        {
            if (rinItemList.Count > 0)
            {
                List<string> info = new List<string>();
                string title = "Indoor List";

                
                foreach (RoomIndoor rindItem in rinItemList)
                {
                    info.Add(rindItem.IndoorFullName); // 思考以下如何加上单位表达式？
                }

                frmHelpInfo f = new frmHelpInfo(info, title);
                f.StartPosition = FormStartPosition.CenterScreen;
                f.Show();
            }
        }

        private void dgvSelectedItems_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //if (dgvSelectedItems.Rows.Count > 0)   //控制分享控制器选项是否可用
            //{
            //    foreach (DataGridViewRow r in dgvSelectedItems.Rows)
            //    {
            //        string item = r.Cells[Name_Common.SelType].Value.ToString();
            //        //如果控制器模块分配给主室内机
            //        if (item.Equals("Remote Control Switch") || item.Equals("Half-size Remote Control Switch"))
            //        {
            //            uc_CheckBox_Sharing_RemoteController.Enabled = true;
            //            return;
            //        }
            //        else if (item.Equals("Wireless Remote Control Switch"))
            //        {
            //            uc_CheckBox_Sharing_RemoteController.Enabled = false;
            //            return;
            //        }
            //    }

            //}

        }

        private void dgvSelectedItems_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //uc_CheckBox_Sharing_RemoteController.Enabled = false; ;
            //if (dgvSelectedItems.Rows.Count > 0)   //控制分享控制器选项是否可用
            //{
            //    foreach (DataGridViewRow r in dgvSelectedItems.Rows)
            //    {
            //        string item = r.Cells[Name_Common.SelType].Value.ToString();
            //        //如果控制器模块分配给主室内机
            //        if (item.Equals("Remote Control Switch") || item.Equals("Half-size Remote Control Switch"))
            //        {
            //            uc_CheckBox_Sharing_RemoteController.Enabled = true;
            //            return;
            //        }
            //        else if (item.Equals("Wireless Remote Control Switch"))
            //        {
            //            uc_CheckBox_Sharing_RemoteController.Enabled = false;
            //            return;
            //        }
            //    }

            //}
            //else            
            //    uc_CheckBox_Sharing_RemoteController.Enabled = false;
        }


        /// <summary>
        /// 删除所选的配件
        /// </summary>
        /// <returns></returns>
        private void deleteAccessory(string type, DataGridViewRow row)
        {
            bool isContinue = true;
            // 删除联动选项
            string newType = GetNewTypeName(type);
            //判断type 是否存在多个 并且存在联动
            int rownumber = 0;
            foreach (DataGridViewRow r in this.dgvSelectedItems.Rows)
            {
                if (r.Cells[Name_Common.SelType].Value.ToString() == type)
                {
                    rownumber++;
                }
            }
            if (!string.IsNullOrEmpty(newType) && rownumber > 1)
            {
                if (JCMsg.ShowConfirmOKCancel(Msg.ACCESSORY_DELETEBYASSOCIATE_MSG) != DialogResult.OK)
                {
                    isContinue = false;
                }
            }
            if (isContinue)
            {
                List<Accessory> ListAccessory = rinItemList[0].ListAccessory;
                if (ListAccessory != null)
                {
                    foreach (Accessory item in ListAccessory)
                    {
                        if (item.IsDefault && item.Type == this.dgvSelectedItems.SelectedRows[0].Cells[Name_Common.SelType].Value.ToString())
                        {
                            return;
                        }
                    }
                }
                //默认绑定的accessory 不能删除
                if (this.dgvSelectedItems.SelectedRows[0].Cells[Name_Common.SelIsDefault].Value.ToString().ToLower() == "true")
                {
                    return;
                }

                this.dgvSelectedItems.Rows.RemoveAt(row.Index);   
                if (!string.IsNullOrEmpty(newType))
                { 
                    for (int i = dgvSelectedItems.Rows.Count - 1; i >= 0; i--)
                    {
                        if (dgvSelectedItems.Rows[i].Cells[Name_Common.SelType].Value.ToString() == newType)
                            dgvSelectedItems.Rows.RemoveAt(i);
                    } 
                }

                if (!string.IsNullOrEmpty(newType) && rownumber > 1)
                {
                    for (int i = dgvSelectedItems.Rows.Count - 1; i >= 0; i--)
                    {
                        if (dgvSelectedItems.Rows[i].Cells[Name_Common.SelType].Value.ToString() == type)
                            dgvSelectedItems.Rows.RemoveAt(i);
                    } 
                }
            }
        }

        /// <summary>
        /// 删除共享控制器关联关系
        /// </summary>
        /// <returns></returns>
        private bool deleteSharingRelationShip()
        {
            string reContrIndoorName = rinItemList[0].IndoorFullName;
            string groupIndoorName = "";
            foreach (RoomIndoor rind in rinItemList[0].IndoorItemGroup)
            {
                if (!rind.IndoorFullName.Equals(reContrIndoorName))
                    //groupIndoorName += rind.IndoorFullName.Split('[')[0] + ",";
                    groupIndoorName += rind.IndoorName + ",";
            }
            if (!string.IsNullOrEmpty(groupIndoorName))
            {
                groupIndoorName = groupIndoorName.Substring(0, groupIndoorName.Length - 1);
                DialogResult result = JCMsg.ShowConfirmOKCancel(Msg.IND_IS_Sharing_RemoteController(reContrIndoorName, groupIndoorName));
                if (result != DialogResult.OK)
                    return false;

                //清空删除对象及其共享控制器组成员所有关联关系
                foreach (RoomIndoor rind in rinItemList[0].IndoorItemGroup)
                {
                    //string shortInName = rind.IndoorName;
                    if (rind.IndoorItemGroup != null)
                    {
                        if (rind.IndoorItemGroup.Count != 0)
                            rind.IndoorItemGroup = null;   //清空关联关系
                        if (rind.IsMainIndoor)
                            rind.IsMainIndoor = false;   //重置主室内机
                    }
                }
            }
            return true;
        }

        private void uc_CheckBox_Sharing_RemoteController_CheckedChanged(object sender, EventArgs e)
        {
            //消息表示，0:无提示 1:没有分配"Remote Control Switch/Half-size Remote Control Switch"且没有"Wireless Remote Control Switch" 2:分配了"Wireless Remote Control Switch"
            int msgIndex = 0;
            if (dgvSelectedItems.Rows.Count == 0)
                msgIndex = 1;
            foreach (DataGridViewRow r in dgvSelectedItems.Rows)
            {
                string item = r.Cells[Name_Common.SelType].Value.ToString();
                if (item.Equals("Wireless Remote Control Switch"))
                {
                    msgIndex = 2;
                    break;
                }
                else
                {
                    //如果控制器模块分配给主室内机
                    if (item.Equals("Remote Control Switch") || item.Equals("Half-size Remote Control Switch"))
                    {
                        msgIndex = 0;
                        break;
                    }
                    else
                        msgIndex = 1;
                }
            }

            //根据msgIndex进行对应提示
            switch (msgIndex)
            {
                case 1:
                    JCMsg.ShowInfoOK(Msg.GetResourceString("ACCESSORY_SHARE_REMOTECONTROLLER_INFO_1"));
                    uc_CheckBox_Sharing_RemoteController.Checked = false;
                    break;
                case 2:
                    JCMsg.ShowInfoOK(Msg.GetResourceString("ACCESSORY_SHARE_REMOTECONTROLLER_INFO_2"));
                    uc_CheckBox_Sharing_RemoteController.Checked = false;
                    break;

            }

            if (uc_CheckBox_Sharing_RemoteController.Checked)   //控制主室内机Combobox
            {
                jccmbIndoorItem.Visible = true;               
            }
            else
                jccmbIndoorItem.Visible = false;
        }

        private void jccmbIndoorItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            mainIndoor = (int)jccmbIndoorItem.SelectedValue;   //    指定主室内机
        }

    }
}
    