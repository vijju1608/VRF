using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JCHVRF
{
    /// 定义拖放后的事件委托
    /// <summary>
    /// 定义拖放后的事件委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param> 
    public delegate void DropEventHandler(object sender, DragEventArgs e);

    interface IDropController
    {
        /// 事件声明，拖拽动作发生前
        /// <summary>
        /// 事件声明，拖拽动作发生前
        /// </summary>
        event DropEventHandler BeforeDrop;

        /// 事件声明，拖拽动作完成后
        /// <summary>
        /// 事件声明，拖拽动作完成后
        /// </summary>
        event DropEventHandler AfterDrop;

        /// 事件声明，拖拽区域删除前
        /// <summary>
        /// 事件声明，拖拽区域删除前
        /// </summary>
        event EventHandler BeforeRemove;

        /// 定义引发事件的方法，拖拽动作发生前，如检测是否符合拖拽条件等
        /// <summary>
        /// 定义引发事件的方法，拖拽动作发生前，如检测是否符合拖拽条件等
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        void OnBeforeDrop(object sender, DragEventArgs e);

        /// 定义引发事件的方法，拖拽动作完成后
        /// <summary>
        /// 定义引发事件的方法，拖拽动作完成后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnAfterDrop(object sender, DragEventArgs e);

        /// 定义引发事件的方法，删除拖拽区域之前
        /// <summary>
        /// 定义引发事件的方法，删除拖拽区域之前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBeforeRemove(object sender, EventArgs e);


        /// 拖拽区域的标题（目前仅DropOutdoor使用）
        /// <summary>
        /// 拖拽区域的标题（目前仅DropOutdoor使用）
        /// </summary>
        string Title { get; set; }

        /// 当前拖拽区域是否为活动区域
        /// <summary>
        /// 当前拖拽区域是否为活动区域
        /// </summary>
        bool IsActive { get; }

        /// 拖拽区域非活动状态时的设置
        /// <summary>
        /// 拖拽区域非活动状态时的设置
        /// </summary>
        void SetInactive();

        /// 拖拽区域活动状态时的设置
        /// <summary>
        /// 拖拽区域活动状态时的设置
        /// </summary>
        void SetActive();

 
    }
}
