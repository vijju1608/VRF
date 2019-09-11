using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JCHVRF.Model;
using JCBase.Utility;


namespace JCHVRF
{
    public partial class ucDropController : UserControl, IDropController
    {
        public ucDropController()
        {
            InitializeComponent();

            this.pbController.AllowDrop = true;
            this.Title = "Controller";

            this.pbController.MouseDown += new MouseEventHandler(pbController_MouseDown);
            this.pbController.DragOver += new DragEventHandler(pbController_DragOver);
            this.pbController.DragDrop += new DragEventHandler(pbController_DragDrop);

            this.pbRemove.Click += new EventHandler(pbRemove_Click);
            this.pbRemove.MouseHover += new EventHandler(pbRemove_MouseHover);
            this.pbRemove.MouseLeave += new EventHandler(pbRemove_MouseLeave);

            this.pbAdd.Click += new EventHandler(pbAdd_Click);
            this.pbAdd.MouseHover += new EventHandler(pbAdd_MouseHover);
            this.pbAdd.MouseLeave += new EventHandler(pbAdd_MouseLeave);
            
            SetInactive();
        }

        /// <summary>
        /// 将Controller对象的属性绑定到控件
        /// </summary>
        /// <param name="item"></param>
        public void BindToControl_Controller(Controller item, CentralController type)
        {
            if (item == null) return;

            this.Controller = item;
            this.TypeInfo = type;

            SetActive();

            this.SetControllerImage(type.Image);
            this.SetToolTip(type);

            this.UpdateQuantity();
        }

        public void UpdateQuantity()
        {
            this.lblQuantity.Text = this.Controller.Quantity.ToString();
        }
                
        void pbController_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as PictureBox).DoDragDrop(sender as PictureBox, DragDropEffects.Move);
        }

        /// 拖拽前校验
        /// <summary>
        /// 拖拽前校验
        /// </summary>
        /// <param name="sender">目标容器</param>
        /// <param name="e">源对象</param>
        void pbController_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            //不触发自己的事件
            if (sender == e.Data.GetData(typeof(PictureBox))) return;

            this.OnBeforeDrop(sender as PictureBox, e);
        }

        void pbController_DragDrop(object sender, DragEventArgs e)
        {
            this.OnAfterDrop(sender as PictureBox, e);
        }


        private void pbRemove_Click(object sender, EventArgs e)
        {
            this.OnBeforeRemove(this, e);
        }

        private void pbRemove_MouseHover(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Hand;
            pb.Image = JCHVRF.Properties.Resources.trash_can_white;
        }

        private void pbRemove_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Default;
            pb.Image = JCHVRF.Properties.Resources.trash_can_nor;
        }

        private void pbAdd_Click(object sender, EventArgs e)
        {
            this.OnBeforeAdd(this, e);
        }

        private void pbAdd_MouseHover(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Hand;
            pb.Image = JCHVRF.Properties.Resources.Simple_unit_add_hi_48x48;
        }

        private void pbAdd_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.Cursor = Cursors.Default;
            pb.Image = JCHVRF.Properties.Resources.Simple_unit_add_nr_48x48;
        }

        public event EventHandler BeforeAdd;
        public void OnBeforeAdd(object sender, EventArgs e)
        {
            if (BeforeAdd != null)
            {
                BeforeAdd(this, e);
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
                BeforeDrop(sender as PictureBox, e);
            }
        }

        public void OnAfterDrop(object sender, DragEventArgs e)
        {
            if (AfterDrop != null)
            {
                 AfterDrop(sender as PictureBox, e);
            }
        }

        public void OnBeforeRemove(object sender, EventArgs e)
        {
            if (BeforeRemove != null)
            {
                BeforeRemove(this, e);
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
            this.BackColor = Color.FromArgb(220, 224, 225);

            this.lblQuantity.Visible = false;
            this.pbRemove.Visible = false;
            this.pbAdd.Visible = false;
            this._isActive = false;
        }

        public void SetActive()
        {
            if (this.IsActive) return;

            this.jclblTitle.ForeColor = Color.FromArgb(68, 92, 116);
            this.jclblTitle.BackColor = Color.FromArgb(214, 223, 036);
            this.BackColor = Color.FromArgb(244, 233, 118);

            this.lblQuantity.Visible = true;
            this.pbRemove.Visible = true;
            this.pbAdd.Visible = true;
            this._isActive = true;
        }

        #endregion

        // controller增加指定的数量
        /// <summary>
        /// controller增加指定的数量
        /// </summary>
        /// <param name="number">要增加的数量</param>
        public void Add(int number)
        {
            this.Controller.Quantity += number;
            this.UpdateQuantity();
        }

        // 移除自己这个控件
        /// <summary>
        /// 移除自己这个控件
        /// </summary>
        public void Remove()
        {
            this.Parent.Controls.Remove(this);
        }


        //// 添加数据 20140912 clh
        //private string _controllerID;
        ///// 绑定的Controller对象的ID号
        ///// <summary>
        ///// 绑定的Controller对象的ID号
        ///// </summary>
        //public string ControllerID
        //{
        //    get { return _controllerID; }
        //    set { _controllerID = value; }
        //}

        ///// 将当前控件绑定到Controller对象
        ///// <summary>
        ///// 将当前控件绑定到Controller对象
        ///// </summary>
        ///// <param name="controllerID"></param>
        //public void BindToController(string controllerID)
        //{
        //    this._controllerID = controllerID;
        //}

        public Controller Controller
        {
            get;
            private set;
        }

        ///// 获取当前控制器的类型，Touch | ONOFF
        ///// <summary>
        ///// 获取当前控制器的类型，Touch | ONOFF
        ///// </summary>
        ///// <returns></returns>
        //public CentralController GetControllerType()
        //{
        //    return (CentralController)pbController.Tag;
        //}

        /// 获取当前控制器的类型信息
        /// <summary>
        /// 获取当前控制器的类型信息(即CentralController表中的Model值）
        /// </summary>
        /// <returns></returns>
        public CentralController TypeInfo
        {
            get ;
            private set ;
        }

        /// 获取当前图片对象
        /// <summary>
        /// 获取当前图片对象
        /// </summary>
        /// <returns></returns>
        public Image GetControllerImage()
        {
            return this.pbController.Image;
        }

        /// 绑定图片控件
        /// <summary>
        /// 绑定图片控件
        /// </summary>
        /// <param name="image"></param>
        public void SetControllerImage(string imageName)
        {
            JCBase.Utility.Util.SetControllerImage(this.pbController, imageName);
            //this.lblQuantity.Refresh();
        }

        /// <summary>
        /// 设置提示信息。包括型号和描述。
        /// </summary>
        /// <param name="data"></param>
        public void SetToolTip(CentralController type)
        {
            toolTip1.SetToolTip(this.pbController, type.Model + "\n" + type.Description);
        }
    }
}
