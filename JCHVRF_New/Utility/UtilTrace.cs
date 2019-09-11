using JCBase.UI;
using JCHVRF.Model;
using JCHVRF_New.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;
using JCBase.Utility;
using System.Drawing;
using JCHVRF_New.Common.Helpers;
using System.Windows.Controls;
using System.Configuration;

namespace JCHVRF_New.Utility
{
    public class UtilTrace
    {
        /// <summary>
        /// 历史工程列表
        /// </summary>
        private static List<TraceEnt> _PrjList = new List<TraceEnt>();
        /// <summary>
        /// 当前工程ID
        /// </summary>
        private static string _currentHistoryId = "";
        /// <summary>
        /// 主界面
        /// </summary>
        public static ViewModelBase ViewModel = null;
        
        private static byte[] lastprojectblob = null;
        private static byte[] lastsystemblob = null;
        private static int undoRedoCount;
        public static Control View = null;
        /// <summary>
        /// 事件委托
        /// </summary>
        public delegate void DoEvent();
        /// <summary>
        /// 检查主界面撤销按钮状态事件
        /// </summary>
        public static DoEvent ChkMainUndo;
        public static DoEvent SaveSystemState;
        public static DoEvent EnableUndo;
        public static DoEvent EnableRedo;
        /// <summary>
        /// 刷新主界面
        /// </summary>
        public static DoEvent ReLoadMain;
        /// <summary>
        /// 主界面选项卡控件
        /// </summary>
        public static TapPageTrans tbMain = null;
        /// <summary>
        /// 选项卡切换是否是撤销操作导致
        /// </summary>
        public static bool isTabSwitching = false;
        /// <summary>
        /// 初始化方法
        /// </summary>
        public static void Ini()
        {
            _PrjList = new List<TraceEnt>();
            _currentHistoryId = "";
            ViewModel = null;
            View = null;
            undoRedoCount = string.IsNullOrEmpty(Convert.ToString(ConfigurationManager.AppSettings["UndoRedoCount"])) ? 10 : Convert.ToInt32(ConfigurationManager.AppSettings["UndoRedoCount"]);
            UndoRedoSetup.ResetUndoRedo += delegate ()
            {
                _PrjList.Clear();
                _currentHistoryId = string.Empty;
                lastprojectblob = lastsystemblob = null;
            };
        }

        public static void SaveHistoryTraces(JCHVRF.Model.NextGen.SystemVRF systemVrf)
        {
            byte[] currentSystemBlob = JCHVRF.DAL.New.Utility.Serialize(systemVrf);
            if ((lastsystemblob == null || !lastsystemblob.SequenceEqual(currentSystemBlob)))
            {
                Project.GetProjectInstance.SystemListNextGen[Project.GetProjectInstance.SystemListNextGen.FindIndex(ind => ind.Id.Equals(systemVrf.Id))] = systemVrf;
                SaveHistoryTraces();
            }
        }

        /// <summary>
        /// 保存历史痕迹
        /// </summary>
        public static void SaveHistoryTraces()
        {
            byte[] currentProjectBlob = JCHVRF.DAL.New.Utility.Serialize(Project.GetProjectInstance);
            if ((lastprojectblob == null || !lastprojectblob.SequenceEqual(currentProjectBlob)))
            {
                //SaveSystemState?.Invoke();
                lastprojectblob = currentProjectBlob;
                SaveHistoryTraces(null, null, null);
                
                if (_PrjList.Count > undoRedoCount)
                {
                    _PrjList.RemoveRange(0, _PrjList.Count - undoRedoCount);
                }
            }
        }

        /// <summary>
        /// 保存历史痕迹
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="data">数据</param>
        /// <param name="ent">注册实体</param>
        public static void SaveHistoryTraces(string type, object data, RegUndoEnt ent)
        {
            var index = 0;
            for (var i = 0; i < _PrjList.Count; i++)
            {
                if (_PrjList[i].id == _currentHistoryId)
                {
                    index = i;
                    break;
                }
            }
            index = index + 1;
            if (index < _PrjList.Count)
            {
                _PrjList.RemoveRange(index, _PrjList.Count - index);
            }
            var _his_data = new TraceEnt();
            _his_data.id = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(type) && data != null)
            {
                _his_data.type = type;
                _his_data.data = data;
                if (type == "tab")
                {
                    //定义实现选项卡切换撤销方法
                    _his_data.eventHandler += delegate ()
                    {
                        isTabSwitching = true;
                        //tbMain.SelectedTab = (TabPage)_his_data.data;
                        isTabSwitching = false;
                    };
                }
            }
            else
            {

                _his_data.type = "prj";
                //ViewModel.DoSavePipingStructure();
                _his_data.data = Project.GetProjectInstance.DeepClone();
            }
            _PrjList.Add(_his_data);
            _currentHistoryId = _his_data.id;
            if (ChkMainUndo != null)
            {
                ChkMainUndo();
            }
            if (ent != null)
            {
                ent.ChkUndoEnable();
            }
            EnableUndo?.Invoke();
            EnableRedo?.Invoke();
        }
        /// <summary>
        /// 历史痕迹添加锁定标识方法
        /// </summary>
        /// <param name="lockkey"></param>
        public static void LockHistoryTrace(string lockkey)
        {
            for (var i = 0; i < _PrjList.Count; i++)
            {
                var ent = _PrjList[i];
                if (ent.id == _currentHistoryId)
                {
                    ent.lockkey = lockkey;
                    break;
                }
            }
        }
        /// <summary>
        /// 检查撤销按钮状态
        /// </summary>
        /// <param name="lockkey"></param>
        /// <returns></returns>
        public static UndoEnableEnt ChkUndoEnable(string lockkey)
        {
            var ret = new UndoEnableEnt();
            ret.revoke = true;
            ret.back = true;
            for (var i = 0; i < _PrjList.Count; i++)
            {
                if (_PrjList[i].id == _currentHistoryId)
                {
                    if ((i - 1) >= 0)
                    {
                        ret.revoke = false;
                    }
                    if ((i + 1) < _PrjList.Count)
                    {
                        ret.back = false;
                    }
                    if (_PrjList[i].lockkey != null && _PrjList[i].lockkey == lockkey)
                    {
                        ret.revoke = true;
                    }
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 向前撤销
        /// </summary>
        /// <param name="lockkey"></param>
        /// <returns></returns>
        public static UndoEnableEnt RevokeTrace(string lockkey)
        {
            var ret = new UndoEnableEnt();
            for (var i = 0; i < _PrjList.Count; i++)
            {
                if (_PrjList[i].id == _currentHistoryId)
                {
                    var ent = _PrjList[i - 1];
                    if (ent.lockkey != null && ent.lockkey == lockkey)
                    {
                        ret.revoke = true;
                    }
                    if ((i - 1) == 0)
                    {
                        ret.revoke = true;
                    }
                    _currentHistoryId = ent.id;
                    if (ent.type == "prj")
                    {
                        for (var m = i - 1; m >= 0; m--)
                        {
                            if (_PrjList[m].type != "prj")
                            {
                                _PrjList[m].doWork();
                                break;
                            }
                        }
                        Project.CurrentProject = (Project)ent.data;
                        //frmMan.DoSavePipingStructure();
                        ent.data = Project.GetProjectInstance.DeepClone();
                    }
                    else
                    {
                        for (var m = i - 1; m >= 0; m--)
                        {
                            if (_PrjList[m].type == "prj")
                            {
                                //frmMan.thisProject = (Project)_PrjList[m].data;
                                Project.CurrentProject = (Project)_PrjList[m].data;
                                //frmMan.DoSavePipingStructure();
                                //_PrjList[m].data = frmMan.thisProject.DeepClone();
                                _PrjList[m].data = Project.GetProjectInstance.DeepClone();
                                break;
                            }
                        }
                        ent.doWork();
                    }
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 后后撤销
        /// </summary>
        /// <returns></returns>
        public static UndoEnableEnt BackTrace()
        {
            var ret = new UndoEnableEnt();
            for (var i = 0; i < _PrjList.Count; i++)
            {
                if (_PrjList[i].id == _currentHistoryId)
                {
                    var ent = _PrjList[i + 1];
                    if ((i + 1) == (_PrjList.Count - 1))
                    {
                        ret.back = true;
                    }
                    _currentHistoryId = ent.id;
                    if (ent.type == "prj")
                    {
                        for (var m = i + 1; m >= 0; m--)
                        {
                            if (_PrjList[m].type != "prj")
                            {
                                _PrjList[m].doWork();
                                break;
                            }
                        }
                        //frmMan.thisProject = (Project)ent.data;
                        //frmMan.DoSavePipingStructure();
                        //ent.data = frmMan.thisProject.DeepClone();
                        Project.CurrentProject = (Project)ent.data;
                        //frmMan.DoSavePipingStructure();
                        ent.data = Project.GetProjectInstance.DeepClone();
                    }
                    else
                    {
                        for (var m = i + 1; m >= 0; m--)
                        {
                            if (_PrjList[m].type == "prj")
                            {
                                //frmMan.thisProject = (Project)_PrjList[m].data;
                                //frmMan.DoSavePipingStructure();
                                //_PrjList[m].data = frmMan.thisProject.DeepClone();

                                Project.CurrentProject = (Project)_PrjList[m].data;
                                _PrjList[m].data = Project.GetProjectInstance.DeepClone();
                                break;
                            }
                        }
                        ent.doWork();
                    }
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 注册撤销实体方法
        /// </summary>
        /// <param name="regEnt"></param>
        public static void RegUndo(RegUndoEnt regEnt)
        {
            regEnt.lockkey = Guid.NewGuid().ToString();
            regEnt.enable.revoke = true;
            regEnt.enable.back = true;
            //注册当前锁定标识位
            LockHistoryTrace(regEnt.lockkey);
            //实现检查撤销状态委托对象方法
            regEnt.ChkUndoEnableHandler += delegate ()
            {
                var ret = ChkUndoEnable(regEnt.lockkey);
                regEnt.enable.revoke = ret.revoke;
                regEnt.enable.back = ret.back;
                if (regEnt.tb != null)
                {
                    regEnt.tb.Invalidate();
                }

            };
            //实现向前撤销委托对象方法
            regEnt.undoHandler += delegate ()
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                var ret = RevokeTrace(regEnt.lockkey);
                regEnt.fun();
                regEnt.enable.revoke = ret.revoke;
                regEnt.enable.back = ret.back;
                ChkMainUndo();
                ReLoadMain();
                EnableRedo?.Invoke();
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            };
            //实现向后撤销委托对象方法
            regEnt.redoHandler += delegate ()
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                var ret = BackTrace();
                regEnt.fun();
                regEnt.enable.revoke = ret.revoke;
                regEnt.enable.back = ret.back;
                ChkMainUndo();
                ReLoadMain();
                EnableUndo?.Invoke();
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            };


        }
    }
    /// <summary>
    /// 历史痕迹实体
    /// </summary>
    public class TraceEnt
    {
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 痕迹类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 痕迹数据
        /// </summary>
        public object data { get; set; }
        /// <summary>
        /// 锁定标识
        /// </summary>
        public string lockkey { get; set; }
        /// <summary>
        /// 委托事件
        /// </summary>
        public delegate void DoEvent();
        /// <summary>
        /// 自定义委托事件对象
        /// </summary>
        public DoEvent eventHandler;
        /// <summary>
        /// 自定义方法
        /// </summary>
        public void doWork()
        {
            eventHandler();
        }
    }
    /// <summary>
    /// 撤销按钮状态实体
    /// </summary>
    public class UndoEnableEnt
    {
        /// <summary>
        /// 向前撤销状态 true 不可用 false 可用
        /// </summary>
        public bool revoke { get; set; }
        /// <summary>
        /// 向后撤销状态 true 不可用 false 可用
        /// </summary>
        public bool back { get; set; }

        public Rectangle undoRect { get; set; }

        public Rectangle redoRect { get; set; }

        public void ShowIcons(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            JCBase.UI.TapPageTrans hostControl = sender as JCBase.UI.TapPageTrans;
            ////绘制撤销 20161227 add by axj
            //if (ShowUndoRedo)
            //{
            //Bitmap picUndo = Properties.Resources.undo;
            //Bitmap picRedo = Properties.Resources.redo;
            //if (!this.revoke)
            //{
            //    picUndo = Properties.Resources.undohov;
            //}
            //if (!this.back)
            //{
            //    picRedo = Properties.Resources.redohov;
            //}
            int w = 2;
            int h = 2;
            int wid = 0;
            if (hostControl.TitleLogo != null)
            {
                wid = hostControl.TitleLogo.Width;
            }
            Rectangle clientArea = hostControl.ClientRectangle;
            Graphics g = e.Graphics;
            //Point _picArea_1 = new Point(clientArea.Width - picUndo.Width - picRedo.Width - wid - 10 - 20, clientArea.Top + 6);
            //g.DrawImage(picUndo, _picArea_1.X, _picArea_1.Y, picUndo.Width - w, picUndo.Height - h);
            //Point _picArea_2 = new Point(clientArea.Width - picRedo.Width - wid - 10 - 10, clientArea.Top + 6);
            //g.DrawImage(picRedo, _picArea_2.X, _picArea_2.Y, picRedo.Width - w, picRedo.Height - h);
            //undoRect = new Rectangle(new Point(hostControl.Location.X + _picArea_1.X, hostControl.Location.Y + _picArea_1.Y), new Size(picUndo.Width - w, picUndo.Height - h));
            //redoRect = new Rectangle(new Point(hostControl.Location.X + _picArea_2.X, hostControl.Location.Y + _picArea_2.Y), new Size(picRedo.Width - w, picRedo.Height - h));
            //}
        }
    }
    /// <summary>
    /// 注册撤销实体
    /// </summary>
    public class RegUndoEnt
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RegUndoEnt()
        {
            enable = new UndoEnableEnt();
        }
        /// <summary>
        /// 锁定标识
        /// </summary>
        public string lockkey { get; set; }
        /// <summary>
        /// 撤销状态
        /// </summary>
        public UndoEnableEnt enable { get; set; }
        /// <summary>
        /// 当前所属Form
        /// </summary>
        //public Form fm = null;

        ///// <summary>
        ///// 当前所属Form
        ///// </summary>
        public ViewModelBase vm = null;

        /// <summary>
        /// 当前所属Form
        /// </summary>
        public ContentControl View = null;
        /// <summary>
        /// 当前所属Tab
        /// </summary>
        public JCBase.UI.TapPageTrans tb = null;
        /// <summary>
        /// 委托
        /// </summary>
        public delegate void DoEvent();
        /// <summary>
        /// 检查撤销状态委托事件对象
        /// </summary>
        public DoEvent ChkUndoEnableHandler;
        /// <summary>
        /// 向前撤销委托事件对象
        /// </summary>
        public DoEvent undoHandler;
        /// <summary>
        /// 向后撤销委托事件对象
        /// </summary>
        public DoEvent redoHandler;
        /// <summary>
        /// 用户自定义委托事件对象
        /// </summary>
        public DoEvent funHandler;
        /// <summary>
        /// 检查撤销状态方法
        /// </summary>
        public void ChkUndoEnable()
        {
            ChkUndoEnableHandler();
        }
        /// <summary>
        /// 向前撤销方法
        /// </summary>
        public void undo()
        {
            undoHandler();
        }
        /// <summary>
        /// 向后撤销方法
        /// </summary>
        public void redo()
        {
            redoHandler();
        }
        /// <summary>
        /// 用户自定义方法
        /// </summary>
        public void fun()
        {
            funHandler();
        }

    }
}
