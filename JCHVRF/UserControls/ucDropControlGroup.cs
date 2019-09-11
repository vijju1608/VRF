using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JCHVRF.Model;
using JCHVRF.VRFMessage;
using JCBase.UI;

namespace JCHVRF
{
    public partial class ucDropControlGroup : UserControl, IDropController
    {
        int MaxY_Controller;
        int MaxY_Outdoor;
        string type_PictureBox = typeof(PictureBox).ToString();
        string type_ListViewItem = typeof(ListViewItem).ToString();
        public Action RemoveGroup = null;
        string _mainRegion = "";
        public ucDropControlGroup(string region)   
        {
            InitializeComponent();
            this.Title = "New control group";

            this.ucDropOutdoor1.BeforeDrop += new DropEventHandler(ucOutdoor_BeforeDrop);
            this.ucDropOutdoor1.AfterDrop += new DropEventHandler(ucOutdoor_AfterDrop);
            this.ucDropOutdoor1.BeforeRemove += new EventHandler(ucOutdoor_BeforeRemove);
            this.ucDropOutdoor1.AfterSetting += new EventHandler(ucOutdoor_AfterSetting);
            this.ucDropOutdoor1.Remove += new EventHandler(ucOutdoor_Remove);
            this.ucDropOutdoor1.Warning += new EventHandler(ucOutdoor_Warning);

            this.ucDropController1.BeforeDrop += new DropEventHandler(ucController_BeforeDrop);
            this.ucDropController1.AfterDrop += new DropEventHandler(ucController_AfterDrop);
            this.ucDropController1.BeforeRemove += new EventHandler(ucController_BeforeRemove);
            this.ucDropController1.BeforeAdd += new EventHandler(ucController_BeforeAdd);
            this._mainRegion = region;   //添加region参数，对EU进行特殊判断

            //SetInactive();
        }

        public ucDropControlGroup(string id, string region) 
        {
            InitializeComponent();
            this._controlGroupID = id;

            this.ucDropOutdoor1.BeforeDrop += new DropEventHandler(ucOutdoor_BeforeDrop);
            this.ucDropOutdoor1.AfterDrop += new DropEventHandler(ucOutdoor_AfterDrop);
            this.ucDropOutdoor1.BeforeRemove += new EventHandler(ucOutdoor_BeforeRemove);
            this.ucDropOutdoor1.AfterSetting += new EventHandler(ucOutdoor_AfterSetting);
            this.ucDropOutdoor1.Remove += new EventHandler(ucOutdoor_Remove);
            this.ucDropOutdoor1.Warning+=new EventHandler(ucOutdoor_Warning);

            this.ucDropController1.BeforeDrop += new DropEventHandler(ucController_BeforeDrop);
            this.ucDropController1.AfterDrop += new DropEventHandler(ucController_AfterDrop);
            this.ucDropController1.BeforeRemove += new EventHandler(ucController_BeforeRemove);
            this.ucDropController1.BeforeAdd += new EventHandler(ucController_BeforeAdd);
            this._mainRegion = region;   //添加region参数，对EU进行特殊判断

            //SetInactive();
        }

        private void ucDropControlGroup_Load(object sender, EventArgs e)
        {
        }

        void ucOutdoor_BeforeDrop(object sender, DragEventArgs e)
        {
            // 检验拖拽的对象是否符合拖放条件，参考室外机数量、型号的限制规则
            // add 20140923 clh 若拖入时该ControlGroup中还没有添加Controller控件，则默认16
            if (CheckBeforeDropOutdoor(sender, e))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        void ucOutdoor_AfterDrop(object sender, DragEventArgs e)
        {
            this.OnAfterDrop(sender as ListView, e);

            CheckControlGroupComplete();//必须在添加Outdoor记录完成后执行
        }

        void ucOutdoor_BeforeRemove(object sender, EventArgs e)
        {
            this.OnBeforeRemove(sender, e);
        }

        void ucOutdoor_AfterSetting(object sender, EventArgs e)
        {
            this.Title = (sender as ucDropOutdoor).Title;
            OnAfterSetting(sender, e);
        }

        void ucOutdoor_Remove(object sender, EventArgs e)
        {
            this.ucDropOutdoor1.UpdateQuantity(); //每次删除系统都需要更新数据
            if (GetOutdoorQty() == 0)  //系统删除完需要检查面板状态
                CheckControlGroupComplete();
            if (GetControllerQty() == 0 && GetOutdoorQty() == 0)
                Remove();
        }
        
        void ucOutdoor_Warning(object sender, EventArgs e)
        {
            JCMsg.ShowWarningOK(ucDropOutdoor1.ErrMsg);
        }


        void ucController_BeforeDrop(object sender, DragEventArgs e)
        {
            if (CheckBeforeDropController(sender, e))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        void ucController_AfterDrop(object sender, DragEventArgs e)
        {
            this.OnAfterDrop(sender as PictureBox, e);

            CheckControlGroupComplete();
        }

        void ucController_BeforeRemove(object sender, EventArgs e)
        {
            this.OnBeforeRemove(sender as ucDropController, e);

            if (GetControllerQty() == 0 && GetOutdoorQty() == 0)
                Remove();
        }

        void ucController_BeforeAdd(object sender, EventArgs e)
        {
            CentralController typeInfo = (sender as ucDropController).TypeInfo; 
            if (CheckBeforeAddController(typeInfo, 1) == false)
            {
                return;
            }

            this.OnBeforeAdd(sender as ucDropController, e);
        }

        private void ucDropControllerGroup_ControlAdded(object sender, ControlEventArgs e)
        {
            ReLocationControls(e);
        }

        private void ucDropControllerGroup_ControlRemoved(object sender, ControlEventArgs e)
        {
            ReLocationControls(e);

            if (e.Control is ucDropController)
            {
                //(sender as ucDropControlGroup).
                CheckControlGroupComplete();
                //必须在删除Controller控件完成后执行

                //Group删除自己，在outdoor 和 controller都被删除以后
                if (this.IsActive == false)
                {
                    this.Remove();
                }
            }
        }

        public event EventHandler BeforeAdd;
        public void OnBeforeAdd(object sender, EventArgs e)
        {
            if (BeforeAdd != null)
            {
                if (sender is ucDropController)
                    BeforeAdd(sender as ucDropController, e);
            }
        }

        public event EventHandler AfterSetting;
        public void OnAfterSetting(object sender, EventArgs e)
        {
            if (AfterSetting != null)
            {
                AfterSetting(this, e);
            }
        }

        #region IDropController

        public event DropEventHandler BeforeDrop;
        public event DropEventHandler AfterDrop;
        public event EventHandler BeforeRemove;

        public void OnBeforeDrop(object sender, DragEventArgs e)
        {
            if (BeforeDrop != null)
            {
                if (sender is ListView)
                    BeforeDrop(sender as ListView, e);
                else if (sender is PictureBox)
                    BeforeDrop(sender as PictureBox, e);
            }
        }

        public void OnAfterDrop(object sender, DragEventArgs e)
        {
            if (AfterDrop != null)
            {
                if (sender is ListView)
                    AfterDrop(sender as ListView, e);
                else if (sender is PictureBox)
                    AfterDrop(sender as PictureBox, e);
            }
        }

        public void OnBeforeRemove(object sender, EventArgs e)
        {
            if (BeforeRemove != null)
            {
                if (sender is ListView)
                    BeforeRemove(sender as ListView, e);
                else if (sender is ucDropController)
                    BeforeRemove(sender as ucDropController, e);
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                this.jclblTitle.Text = _title;
                this.ucDropOutdoor1.Title = _title;
            }
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
            this.BackColor = Color.FromArgb(236, 240, 241);
            this._isActive = false;

        }

        public void SetActive()
        {
            this.jclblTitle.ForeColor = Color.FromArgb(68, 92, 116);
            this.jclblTitle.BackColor = Color.FromArgb(167, 207, 56);
            this.BackColor = Color.FromArgb(236, 240, 241);
            this._isActive = true;
        }
        #endregion 

        
        /// 其中的Outdoor控件状态，检验group的Complete状态时以及检验是否需要绘线时使用
        /// <summary>
        /// 其中的Outdoor控件状态，检验group的Complete状态时以及检验是否需要绘线时使用
        /// </summary>
        public bool IsOutdoorActive
        {
            get
            {
                return this.ucDropOutdoor1.IsActive;
            }
        }


        /// 拖拽区域的控件需要重新定位时触发
        /// <summary>
        /// 拖拽区域的控件需要重新定位时触发
        /// </summary>
        public event EventHandler Relocation;
        public void OnRelocation(object sender, EventArgs e)
        {
            if (Relocation != null)
            {
                Relocation(this, e);
            }
        }

        // add on 20140910 clh 增加Controller数据
        private string _controlGroupID;
        /// 绑定当前项目中指定的ControlGroup对象
        /// <summary>
        /// 绑定当前项目中指定的ControlGroup对象
        /// </summary>
        public string ControlGroupID
        {
            get { return _controlGroupID; }
        }

        /// 将当前控件绑定到指定的ControlGroup对象
        /// <summary>
        /// 将当前控件绑定到指定的ControlGroup对象
        /// </summary>
        /// <param name="groupID"></param>
        public void BindToControlGroup(string groupID)
        {
            this._controlGroupID = groupID;
        }

        public void SetComplete()
        {
            this.ucDropOutdoor1.SetComplete();
        }

        public void SetIncomplete(string errMsg)
        {
            this.ucDropOutdoor1.SetErrMsg(errMsg);
            this.ucDropOutdoor1.SetIncomplete();
        }


        /// 执行当前控件的自我删除
        /// <summary>
        /// 执行当前控件的自我删除
        /// </summary>
        public void Remove()
        {
            if (this.Parent != null)
            {
                this.Parent.Controls.Remove(this);
                if (RemoveGroup != null)
                {
                    RemoveGroup();
                }
             
            }
        }

        /// 动态新增空的ucDropController控件
        /// <summary>
        /// 动态新增空的ucDropController控件
        /// </summary>
        public ucDropController AddController()
        {
            return AddController(null, null);
        }
        
        /// 后台加载Controller对象
        /// <summary>
        /// 后台加载Controller对象
        /// </summary>
        /// <param name="item"></param>
        public ucDropController AddController(Controller item, CentralController type)
        {
            ucDropController uc = new ucDropController();
            if (item != null && type != null)
            {
                uc.BindToControl_Controller(item, type);
            }
            this.Controls.Add(uc);

            uc.BeforeDrop += new DropEventHandler(ucController_BeforeDrop);
            uc.AfterDrop += new DropEventHandler(ucController_AfterDrop);
            uc.BeforeRemove += new EventHandler(ucController_BeforeRemove);
            uc.BeforeAdd += new EventHandler(ucController_BeforeAdd);

            return uc;
        }

        public void RemoveController()
        {
            this.ucDropController1.Remove();
        }

        public ucDropController GetEmptyController()
        {
            for (int i = this.Controls.Count - 1; i >= 0; i--)
            {
                if (this.Controls[i] is ucDropController)
                {
                    ucDropController c = this.Controls[i] as ucDropController;
                    if (c.Controller == null)
                    {
                        return c;
                    }
                }
            }
            return null;
        }        

        ///// 后台加载Controller对象
        ///// <summary>
        ///// 后台加载Controller对象
        ///// </summary>
        ///// <param name="item"></param>
        //public void BindtoControl_Controller(Controller item)
        //{
        //    this.ucDropController1.BindToControl_Controller(item);
        //}

        /// 当前group中有控件增减时需要重新布局其中的控件
        /// <summary>
        /// 当前group中有控件增减时需要重新布局其中的控件
        /// </summary>
        /// <param name="e"></param>
        private void ReLocationControls(EventArgs e)
        {
            MaxY_Outdoor = 27;
            MaxY_Controller = 27;
            this.AutoScrollPosition = new Point(0, 0);
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is ucDropController)
                {
                    ctrl.Location = new Point(UtilControl.hLocationController_group, MaxY_Controller);
                    MaxY_Controller += ctrl.Size.Height + 5;
                }
                else if (ctrl is ucDropOutdoor)
                {
                    ctrl.Location = new Point(UtilControl.hLocationOutdoor_group, MaxY_Outdoor);
                    MaxY_Outdoor += ctrl.Size.Height + 6;
                }
            }
            MaxY_Controller += 1;

            if (MaxY_Controller > MaxY_Outdoor)
            {
                this.Size = new Size(this.Width, MaxY_Controller);
            }
            else
            {
                if (MaxY_Controller > 220)
                    this.Size = new Size(this.Width, MaxY_Controller);
                else
                    this.Size = new Size(this.Width, MaxY_Outdoor);
            }

            this.OnRelocation(this, e);
        }


        /// 用于后台加载
        /// <summary>
        /// 用于后台加载
        /// </summary>
        /// <param name="item"></param>
        public void AddOutdoorItem(ListViewItem item)
        {
            // 后台加载时此处不需要 AddController();
            this.ucDropOutdoor1.AddOutdoorItem(item);
            this.ucDropOutdoor1.UpdateQuantity();
        }
        
        /// 获取当前控件中ucDropController控件的数量，用于验证是否超出最大数量限制
        /// <summary>
        /// 获取当前控件中ucDropController控件的数量，用于验证是否超出最大数量限制
        /// </summary>
        /// <returns></returns>
        public int GetControllerQty()
        {
            int qty = 0;
            foreach (Control item in this.Controls)
            {
                if (item is ucDropController)
                {
                    ucDropController ucController = item as ucDropController;
                    Controller controller = ucController.Controller;
                    CentralController typeInfo = ucController.TypeInfo;
                    if (controller != null)
                    { 
                        //软件不参与统计 modified by Shen Junjie on 2017/12/20
                        if (typeInfo.Type != ControllerType.Software)
                        {
                            qty += controller.Quantity;
                        }
                    }
                }
            }
            return qty;
        }

        /// 获取当前控件中指定控制器类型的ucDropController控件的数量，用于验证
        /// <summary>
        /// 获取当前控件中指定控制器类型的ucDropController控件的数量，用于验证,需求更改
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetControllerQty(ControllerType type)
        {
            int qty = 0;
            foreach (Control item in this.Controls)
            {
                if (item is ucDropController)
                {
                    ucDropController uc = (item as ucDropController);
                    Controller controller = uc.Controller;
                    CentralController typeInfo = uc.TypeInfo;
                    if (uc.IsActive)
                    {
                        if (uc.Controller.Type == type)
                        {
                            qty += uc.Controller.Quantity;
                        }
                    }
                }
            }
            return qty;
        }

        /// 获取当前控件中指定控制器类型的ucDropController控件的数量，用于验证
        /// <summary>
        /// 获取当前控件中指定控制器类型的ucDropController控件的数量，用于验证,需求更改
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetControllerQty(string modelName)
        {
            int qty = 0;
            foreach (Control item in this.Controls)
            {
                if (item is ucDropController)
                {
                   
                    ucDropController uc = (item as ucDropController);
                    if (uc.IsActive)
                    {
                        if (uc.Controller.Model == modelName)
                        {
                            qty += uc.Controller.Quantity;
                        }
                    }
                }
            }
            return qty;
        }

        /// 获取当前控件中指定控制器类型以外的ucDropController控件的数量，用于验证
        /// <summary>
        /// 获取当前控件中指定控制器类型以外的ucDropController控件的数量，用于验证
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetOtherControllerQty(string modelName)
        {
            int qty = 0;
            foreach (Control item in this.Controls)
            {
                if (item is ucDropController)
                {
                    ucDropController uc = (item as ucDropController);
                    if (uc.IsActive)
                    {
                        if (uc.Controller.Model != modelName)
                        {
                            qty += uc.Controller.Quantity;
                        }
                    }
                }
            }
            return qty;
        }

        /// 获取当前控件中的室外机数量
        /// <summary>
        /// 获取当前控件中的室外机数量
        /// </summary>
        /// <returns></returns>
        public int GetOutdoorQty()
        {
            return this.ucDropOutdoor1.GetOutdoorQty();
        }

        /// 获取当前控件中的室内机数量
        /// <summary>
        /// 获取当前控件中的室内机数量
        /// </summary>
        /// <returns></returns>
        public int GetIndoorQty()
        {
            return this.ucDropOutdoor1.GetIndoorQty();
        }


        

        ///// 计算当前Control Group中不同模式下最大的室外机数量
        ///// <summary>
        ///// 计算当前Control Group中不同模式下最大的室外机数量,
        ///// Model模式：一个ONOFF返回4，一个CC返回16；
        ///// Bacnet模式：返回16
        ///// </summary>
        ///// <returns></returns>
        //public int GetMaxOutdoorQty_BeforeDrop(ControllerLayoutType type)
        //{
        //    if (type == ControllerLayoutType.MODBUS)
        //    {
        //        foreach (Control item in this.Controls)
        //        {
        //            if (item is ucDropController)
        //            {
        //                ucDropController ucController = item as ucDropController;
        //                if (ucController.IsActive)
        //                {
        //                    ControllerType controllerType = ucController.TypeInfo.Type;
        //                    if (controllerType == ControllerType.ONOFF)
        //                        return 4;
        //                }
        //            }
        //        }
        //    }
        //    return 16;
        //}

        ///// 计算当前Control Group中不同模式下最大的室外机数量
        ///// <summary>
        ///// 计算当前Control Group中不同模式下最大的室外机数量，
        ///// 只要看Bacnet模式：有CC则返回16，没有CC则返回ONOFF个数×4（小于等于16时）的值
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public int GetMaxOutdoorQty_AfterDrop(ControllerLayoutType type)
        //{
        //    if (type == ControllerLayoutType.BACNET)
        //    {
        //        int controllerQty_CC = GetControllerQty(ControllerType.CentralController);
        //        if (controllerQty_CC == 0)
        //        {
        //            int controllerQty = GetControllerQty(ControllerType.ONOFF);
        //            return controllerQty * 4;
        //        }
        //    }
        //    return GetMaxOutdoorQty_BeforeDrop(type);
        //}

        ///// 获取当前Control Group中不同模式下最大的控制器数量
        ///// <summary>
        ///// 获取当前Control Group中不同模式下最大的控制器数量
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public int GetMaxControllerQty(ControllerLayoutType type)
        //{
        //    if (type == ControllerLayoutType.MODBUS)
        //        return 1;
        //    return 16;
        //}


        /// Outdoor拖拽前可行性检查，不满足拖拽条件则弹出提示
        /// <summary>
        /// Outdoor拖拽前可行性检查，不满足拖拽条件则弹出提示
        /// </summary>
        /// <param name="sender">目标控件</param>
        /// <param name="e">源控件</param>
        /// <returns></returns>
        private bool CheckBeforeDropOutdoor(object sender, DragEventArgs e)
        {
            if (UtilControl.CheckDragType_ListViewItem(e))
            {
                ListView lvTarget = (sender as ListView);   // 目标控件
                ListViewItem item = (ListViewItem)e.Data.GetData(type_ListViewItem, false); // 源控件
                ListView lvSrc = item.ListView;

                // 如果源跟目标ListView同一个，则不可拖拽且不提示
                if (lvTarget == lvSrc && lvSrc.Parent is ucDropOutdoor)
                    return false;

                //int currentOutdoorQty = GetOutdoorQty();
                //int maxOutdoorQty = GetMaxOutdoorQty_BeforeDrop(glProject.ControllerLayoutType);

                //// 拖拽前检查必须小于，不能等于允许的最大数量
                //if (currentOutdoorQty < maxOutdoorQty)
                //    return true;
                //else
                //    JCMsg.ShowWarningOK(Msg.CONTROLLER_OUTDOOR_QTY);

                //获取已经目标区域的各种设备数量
                int controllerQty = GetControllerQty();
                int outdoorQty = GetOutdoorQty();
                int indoorQty = GetIndoorQty();
                if (!item.Name.Contains("Heat Exchanger"))
                {
                    outdoorQty++;
                }

                //加上拖动的设备数量
               // outdoorQty++;
                if (item.Tag is Int32)
                {
                    indoorQty += Convert.ToInt32(item.Tag);
                }
                else if (item.Tag is Object[])
                {
                    Object[] values = item.Tag as Object[];
                    if (values != null && values.Length > 1 && values[1] is Int32)
                    {
                        indoorQty += Convert.ToInt32(values[1]);
                    }
                }
                //EU暂时忽略设备数及Hlink限制   add on 20180629 by axj   
                if (_mainRegion=="EU_E" || _mainRegion == "EU_S" || _mainRegion == "EU_W")
                    return true;

                //Ooutdoor Unit Quantity + Indoor Unit Quantity
                int unitQty = outdoorQty + indoorQty;

                //总设备数量
                int deviceQty = controllerQty + unitQty;  //TODO: 此处还需要加上remote controller的数量
                
                //检查设备数量是否符合H-Link的限制范围之内
                bool isHLinkII = !HasHLinkI();
                if (CheckHLinkLimiation(isHLinkII, indoorQty, outdoorQty, deviceQty) == false)
                {
                    return false;
                }

                List<string> types = ucDropOutdoor1.GetProductTypesWith(item);

                //if (_mainRegion.StartsWith("EU"))   //对EU暂时取消限制
                //    return true;

                //检查设备数量是否符合各种controller的限制
                foreach (Control control in this.Controls)
                {
                    if (control is ucDropController)
                    {
                        ucDropController ucController = control as ucDropController;
                        if (ucController.TypeInfo != null)
                        {
                            ////检查产品类型是否匹配
                            //if (CheckProductType(types, ucController.TypeInfo.ProductType) == false)
                            //{
                            //    return false;
                            //}

                            //检查室内机、室外机和设备总数的限制
                            if (CheckSystemQtyLimitation(ucController.TypeInfo, indoorQty, outdoorQty, deviceQty) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 检查室内机、室外机和设备总数的限制
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="indoorQty"></param>
        /// <param name="outdoorQty"></param>
        /// <param name="deviceQty"></param>
        /// <returns></returns>
        private bool CheckSystemQtyLimitation(CentralController typeInfo, int indoorQty, int outdoorQty, int deviceQty)
        {
            //1 - 200 devices (incl. ODU, IDU, remote controller, central controller) can be connected in 1 H-Link
            if (typeInfo.MaxDeviceNumber > 0 && deviceQty > typeInfo.MaxDeviceNumber)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_TOTAL_DEVICE_QTY(typeInfo.MaxDeviceNumber));
                return false;
            }

            //2 - Indoor quantity limitation
            if (typeInfo.MaxIndoorUnitNumber > 0 && indoorQty > typeInfo.MaxIndoorUnitNumber)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_MAX_INDOOR_QTY(typeInfo.Model, typeInfo.MaxIndoorUnitNumber));
                return false;
            }

            //3 - Outdoor quantity limitation
            if (typeInfo.MaxSystemNumber > 0 && outdoorQty > typeInfo.MaxSystemNumber)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_MAX_OUTDOOR_QTY(typeInfo.Model, typeInfo.MaxSystemNumber));
                return false;
            }
            return true;
        }

        /// 检查控制器和室外机系统的ProductType是否匹配
        /// <summary>
        /// 检查控制器和室外机系统的ProductType是否匹配
        /// </summary>
        /// <returns></returns>
        private bool CheckProductType(List<string> outdoorProductTypes, string controllerProductType)
        {
            if (outdoorProductTypes.Count > 0 && !outdoorProductTypes.Contains(controllerProductType))
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_PRODUCTTYPE_NOT_MATCH);
                return false;
            }
            return true;
        }


        /// 检查控制器和室外机系统的ProductType是否匹配
        /// <summary>
        /// 检查控制器和室外机系统的ProductType是否匹配
        /// </summary>
        /// <returns></returns>
        private bool CheckSystemAndExchanger(DataTable exchanger, string controllerProductType)
        {

           // DataTable dt = ucDropOutdoor1.GetOutdoorAndExchanger();
            if (exchanger != null && exchanger.Rows.Count > 0)
            {
                int systemCount = 0;
                int exchangerCount = 0;
                DataRow dr = exchanger.Rows[0];
                systemCount = Convert.ToInt32(dr[0].ToString());
                exchangerCount = Convert.ToInt32(dr[1].ToString());
                if (systemCount == 0 && exchangerCount > 0)
                {
                    if (!controllerProductType.Contains("Heat Exchanger"))
                    {
                        JCMsg.ShowWarningOK(Msg.CONTROLLER_PRODUCTTYPE_NOT_MATCH);
                        return false;
                    }
                }
                else if (systemCount > 0 && exchangerCount == 0)
                {
                    if (controllerProductType.Contains("Heat Exchanger"))
                    {
                        JCMsg.ShowWarningOK(Msg.CONTROLLER_PRODUCTTYPE_NOT_MATCH);
                        return false;
                    }
                }


            }

           
            return true;
        }

        /// 检查控制器和exchanger系统的ProductType是否匹配 on 20170904 by xyj
        /// <summary>
        /// 检查控制器和exchanger系统的ProductType是否匹配
        /// </summary>
        /// <returns></returns>
        private bool CheckExchangerIsMatch(DataTable exchanger, string controllerProductType)
        {
            if (exchanger != null && exchanger.Rows.Count > 0)
            {
                //判断当前节点里面是否包含System 
                DataRow[] rows = null;
                rows = exchanger.Select("ProductType='System'");
                if (rows.Length > 0)
                {
                    //如果包含直接返回不需验证
                    return true;
                }

                DataRow[] rowexchanger = exchanger.Select("ProductType='Exchanger'");
                if (rowexchanger.Length == 1)
                {
                    if (!rowexchanger[0]["ExchangerType"].ToString().Contains(controllerProductType))
                    {
                        JCMsg.ShowWarningOK(Msg.CONTROLLER_PRODUCTTYPE_NOT_MATCH);
                        return false;
                    }
                }
                else if (rowexchanger.Length > 1)
                {
                    //判断是否存在包含Exchanger 的类型
                    bool isTrue = false;
                    foreach (DataRow r in rowexchanger)
                    {
                        if (r["ExchangerType"].ToString().Contains(controllerProductType))
                        {
                            isTrue = true;
                            break;
                        }
                    }
                    if (!isTrue)
                    {
                        JCMsg.ShowWarningOK(Msg.CONTROLLER_PRODUCTTYPE_NOT_MATCH);
                        return false;
                    }
                }
                
              
            } 
            return true;
        }


        /// <summary>
        /// 检查H-Link/H-Link II的限制，H-Link与H-Link II并存时，使用H-Link的限制
        /// </summary>
        /// <param name="isHLinkII"></param>
        /// <param name="indoorQty"></param>
        /// <param name="outdoorQty"></param>
        /// <param name="deviceQty"></param>
        /// <returns></returns>
        private bool CheckHLinkLimiation(bool isHLinkII, int indoorQty, int outdoorQty, int deviceQty)
        {
            if (isHLinkII)                
            {
                if (indoorQty > ControllerConstValue.HLINKII_MAX_IDU_QTY)
                {
                    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_INDOOR_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_IDU_QTY));
                    return false;
                }
                if (outdoorQty > ControllerConstValue.HLINKII_MAX_ODU_QTY)
                {
                    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_OUTDOOR_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_ODU_QTY));
                    return false;
                }
                if (deviceQty > ControllerConstValue.HLINKII_MAX_DEVICE_QTY)
                {
                    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_DEVICE_QTY("H-Link II", ControllerConstValue.HLINKII_MAX_DEVICE_QTY));
                    return false;
                }
            }
            else
            {
                if (indoorQty > ControllerConstValue.HLINK_MAX_IDU_QTY)
                {
                    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_INDOOR_QTY("H-Link", ControllerConstValue.HLINKII_MAX_IDU_QTY));
                    return false;
                }
                if (outdoorQty > ControllerConstValue.HLINK_MAX_ODU_QTY)
                {
                    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_OUTDOOR_QTY("H-Link", ControllerConstValue.HLINKII_MAX_ODU_QTY));
                    return false;
                }
                if (deviceQty > ControllerConstValue.HLINK_MAX_DEVICE_QTY)
                {
                   // JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_DEVICE_QTY("H-Link", Model.ConstValue.HLINKII_MAX_DEVICE_QTY));//HLINKII_MAX_DEVICE_QTY 限制200
                    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_DEVICE_QTY("H-Link", ControllerConstValue.HLINK_MAX_DEVICE_QTY));// HLINK_MAX_DEVICE_QTY 限制145 on 20180306 by xyj
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查组合限制
        /// </summary>
        /// <returns></returns>
        private bool CheckCombinationLimitation(CentralController newType)
        { 
            foreach (Control item in this.Controls)
            {
                if (item is ucDropController)
                {
                    ucDropController uc = (item as ucDropController);
                    CentralController existControllerType = uc.TypeInfo;

                    if (existControllerType == null) continue;
                    if (newType == existControllerType) continue;

                    //验证新加的控制器类型是否和已有的控制器互相兼容
                    if (CheckControllerCompatible(newType, existControllerType, 1) == false) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取不再指定范围里的型号，如果都在范围里面则返回null
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <param name="number">比较的次数，1是A和B比较，2是B和A比较。</param>
        /// <returns></returns>
        private bool CheckControllerCompatible(CentralController type1, CentralController type2, int number)
        {
            if (string.IsNullOrEmpty(type1.CompatibleModel))
            {
                if (number == 1)
                {
                    //type1没有明确兼容的控制器，需要第二次比较，如果type2不兼容type1，则最后结果是不兼容
                    return CheckControllerCompatible(type2, type1, 2);
                }
                else
                {
                    return true;
                }
            }
            else if (type1.CompatibleModel == "none")
            {
                //2.1 - Not compatible with other central controler 
                JCMsg.ShowWarningOK(Msg.CONTROLLER_NOT_COMPATIBLE);
                return false;
            }
            else
            {
                //2.2 - is combinable
                List<string> compatibleModels = new List<string>();
                compatibleModels.AddRange(type1.CompatibleModel.Split(','));
                //兼容性逻辑应该是所有类型适用的，所以下面的判断可以去掉 20160901 by Yunxiao Lin
                //if (type2.Type == ControllerType.CentralController || type2.Type == ControllerType.ONOFF)
                //{
                if (!compatibleModels.Contains(type2.BrandCode == "H" ? type2.Model_Hitachi : type2.Model_York))
                { 
                    //type1不兼容type2, 有3种情况
                    //1. 如果是第二次比较还是不兼容的话，则肯定是不兼容
                    //2. 如果是第一次比较，type1不兼容typ2,而type2没有指定兼容的型号，则判断为不兼容。
                    if (number == 2 || string.IsNullOrEmpty(type2.CompatibleModel))
                    {
                        JCMsg.ShowWarningOK(Msg.CONTROLLER_NOT_COMPATIBLE_WITH(number == 1 ? type2.Model : type1.Model));
                        return false;
                    }
                    //3.如果第一次比较下来type1不兼容typ2, 并且type2有明确兼容的型号，则要看type2是不是兼容type1。
                    else
                    {
                        return CheckControllerCompatible(type2, type1, 2);
                    }
                 }
                //}
            }
            return true;
        }

        /// <summary>
        /// 在添加controller之前做检查
        /// </summary>
        /// <param name="typeInfo">controller类型</param>
        /// <param name="addNumber">增加的数量</param>
        /// <returns></returns>
        private bool CheckBeforeAddController(CentralController typeInfo, int addNumber)
        {
            ControllerLayoutType type = glProject.ControllerLayoutType;
            int controllerQty = GetControllerQty();
            int outdoorQty = GetOutdoorQty();
            int indoorQty = GetIndoorQty();
            int ccControllerQty = GetControllerQty(ControllerType.CentralController);
            int onoffQty = GetControllerQty(ControllerType.ONOFF);
            int unitQty = outdoorQty + indoorQty;

            //累加上需要新增的数量
            controllerQty += addNumber;
            switch (typeInfo.Type)
            {
                case ControllerType.ONOFF:
                    onoffQty += addNumber;
                    break;
                case ControllerType.Software: //软件不统计数量，但是因为会同时增加一个实体controller，所以需要加上
                case ControllerType.CentralController:
                    ccControllerQty += addNumber;
                    break;
            }

            int deviceQty = controllerQty + unitQty;  //TODO: 此处还需要加上remote controller的数量

            ////检查产品类型是否匹配
            //if (CheckProductType(ucDropOutdoor1.GetProductTypes(), typeInfo.ProductType) == false)
            //{
            //    return false;
            //}
            
            //区分exchanger 和system 选中的Controller on 20170830 by xyj
            if (CheckSystemAndExchanger(ucDropOutdoor1.GetOutdoorAndExchanger(), typeInfo.ProductType) == false)
            {
                return false;
            }

            //区分exchanger类型对应的Controller 类型是否相同on 20170904 by xyj
            if (CheckExchangerIsMatch(ucDropOutdoor1.GetExchangerTypesList(), typeInfo.ProductType) == false)
            {
                return false;
            }

            //EU暂时忽略设备数及Hlink限制   add on 20180621 by Vince   
            if (_mainRegion == "EU_E" || _mainRegion == "EU_S" || _mainRegion == "EU_W")
                return true;

            //检查设备数量是否符合拖动的controller的限制
            if (CheckSystemQtyLimitation(typeInfo, indoorQty, outdoorQty, deviceQty + addNumber) == false)
            {
                return false;
            }

            //检查设备数量是否在H-Link的限制范围之内
            bool isHLinkII = !(typeInfo.Protocol.Trim().ToUpper() == "H-LINK" || HasHLinkI());
            if (CheckHLinkLimiation(isHLinkII, indoorQty, outdoorQty, deviceQty + addNumber) == false)
            {
                return false;
            }

            ////1 - Can be connected upto 8 Central Controllers in 1 H-Link
            //if (typeInfo.Type == ControllerType.CentralController && ccControllerQty > Model.ConstValue.HLINKII_MAX_CC_QTY)
            //{
            //    JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_CENTRAL_CONTROLLER_QTY(Model.ConstValue.HLINKII_MAX_CC_QTY));
            //    return false;
            //}
            


            //1 - Can be connected upto 8 Central Controllers in 1 H-Link
            if (typeInfo.MaxControllerNumber > 0 && ccControllerQty > typeInfo.MaxControllerNumber)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_CENTRAL_CONTROLLER_QTY(ControllerConstValue.HLINKII_MAX_CC_QTY));
                return false;
            }

            //2 - combination limitation
            CheckCombinationLimitation(typeInfo);

            //3 - check quantity of this model
            if (typeInfo.MaxSameModel > 0 && GetControllerQty(typeInfo.Model) + addNumber > typeInfo.MaxSameModel)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_CONTROLLER_QTY(typeInfo.MaxSameModel));
                return false;
            }

            //4 - check quantity of this type
            if (typeInfo.MaxSameType > 0 && GetControllerQty(typeInfo.Type) + addNumber > typeInfo.MaxSameType)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_HLINK_CONTROLLER_QTY(typeInfo.MaxSameType));
                return false;
            }

            //5 - BACNet Interface Can be connected upto 4 Central Controllers
            if (typeInfo.Type == ControllerType.BACNetInterface && ccControllerQty > ControllerConstValue.BACNET_MAX_CC_QTY)
            {
                JCMsg.ShowWarningOK(Msg.CONTROLLER_BACNET_CC_QTY(ControllerConstValue.BACNET_MAX_CC_QTY));
            }

            //6 - For Web based Cotnrol, max 176 units (ODU + IDU) can be connected.
            if (typeInfo.MaxUnitNumber > 0 && unitQty > typeInfo.MaxUnitNumber)
            {
                //TODO: 此处需要翻译
                JCMsg.ShowWarningOK(Msg.CONTROLLER_UNIT_QTY(typeInfo.Model, typeInfo.MaxUnitNumber));
            }


            return true;
        }

        /// 检验ucDropControlGroup中的Controller的拖拽
        /// <summary>
        /// 检验ucDropControlGroup中的Controller的拖拽
        /// </summary>
        /// <param name="sender">目标控件</param>
        /// <param name="e">源控件</param>
        /// <returns></returns>
        private bool CheckBeforeDropController(object sender, DragEventArgs e)
        {
            if (!UtilControl.CheckDragType_PictureBox(e))
            {
                return false;
            }

            PictureBox pbTarget = sender as PictureBox; // 目标控件
            PictureBox pbSrc = (PictureBox)e.Data.GetData(type_PictureBox); // 源控件
             

            //不能自己拖向自己
            if (pbTarget == pbSrc) return false;

            ucDropController ucDropControllerSource = pbSrc.Parent is ucDropController ? (ucDropController)pbSrc.Parent : null;
            ucDropController ucDropControllerTarget = pbTarget.Parent is ucDropController ? (ucDropController)pbTarget.Parent : null;
            

            //不能目标不在controller group区域
            if (ucDropControllerTarget == null)
            {
                return false;
            }

            CentralController controllerTypeSource;
            if (ucDropControllerSource == null)
            {
                controllerTypeSource = (pbSrc.Tag as CentralController);
            }
            else
            {
                controllerTypeSource = ucDropControllerSource.TypeInfo;
            }

            if (controllerTypeSource == null) 
            {
                return false;
            }

            //目标ucDropController控件已经含有controller
            if (ucDropControllerTarget.IsActive)
            {
                //如果不是同一种控制器，则不允许拖上去
                if (controllerTypeSource.Model != ucDropControllerTarget.TypeInfo.Model)
                {
                    return false;
                }
            }
            //目标ucDropController控件是空白的
            else
            {
                // 若属于同一个ucDropControlGroup，则不可以拖拽
                if (pbSrc.Parent.Parent is ucDropControlGroup && pbSrc.Parent.Parent == this)
                    return false;

                //如果已经存在此型号的controller则不允许添加
                if (this.HasControllerType(controllerTypeSource.Model))
                {
                    return false;
                }
            }

            //拖动的controller的数量
            int addNumber = 1;
            if (ucDropControllerSource != null && ucDropControllerSource.Controller != null)
            {
                addNumber = ucDropControllerSource.Controller.Quantity;
            }
            ListViewItem item = (ListViewItem)e.Data.GetData(type_ListViewItem, false); 

            //继续检查
            return CheckBeforeAddController(controllerTypeSource, addNumber);
        }

        /// 检查并设置当前界面中所有 ucDropControlGroup 控件的完整性状态
        /// <summary>
        /// 检查并设置当前界面中所有 ucDropControlGroup 控件的完整性状态:
        /// Group中添加、删除Controller控件；
        /// Group中添加Outdoor记录至group时；
        /// </summary>
        public void CheckControlGroupComplete()
        {
            int OutdoorQty = GetOutdoorQty();
            int controllerQty = GetControllerQty();

            if (OutdoorQty == 0 && controllerQty == 0)
            {
                SetInactive();
                return;
            }

            SetActive();
            if (controllerQty > 0 && OutdoorQty > 0)
            {
                //int maxOutdoorQty = GetMaxOutdoorQty_AfterDrop(glProject.ControllerLayoutType);
                //// 当前室外机数量小于等于最大室外机数量
                //if (OutdoorQty <= maxOutdoorQty)
                //    SetComplete();
                //else
                //    SetIncomplete(Msg.CONTROLLER_OUTDOOR_QTY);
                
            }
            else
            {
                if (OutdoorQty == 0)
                    SetIncomplete(Msg.CONTROLLER_NOOUTDOOR(this.Title));
                else if (controllerQty == 0)
                    SetIncomplete(Msg.CONTROLLER_NONE(this.Title));
                return;
            }

            SetComplete();
        }

        public ucDropController GetControllerByModelName(string modelName)
        {
            foreach (Control control in this.Controls)
            {
                if (control is ucDropController)
                {
                    ucDropController ucController = control as ucDropController;
                    if (ucController.TypeInfo != null && ucController.TypeInfo.Model == modelName)
                    {
                        return ucController;
                    }
                }
            }
            return null;
        }

        public bool HasControllerType(string modelName)
        {
            return GetControllerByModelName(modelName) != null;
        }

        public bool HasHLinkI()
        {
            foreach (Control control in this.Controls)
            {
                if (control is ucDropController)
                {
                    ucDropController ucController = control as ucDropController;
                    if (ucController.TypeInfo != null && ucController.TypeInfo.Protocol.Trim().ToUpper() == "H-LINK")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
