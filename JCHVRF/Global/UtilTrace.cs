using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF.Model;
using System.Windows.Forms;
using JCBase.Utility;
using System.Drawing;

namespace JCHVRF.UndoRedo
{
    public delegate void ReloadProjectDataEvent(Project prj);
    public delegate void RestoreTabPageEvent(TabPage tab);
    public delegate void GetCurrentProjectEvent(out Project prj);

    public class UtilTrace
    {
        /// <summary>
        /// 历史工程列表
        /// </summary>
        private List<HistoryEnt> historyList;
        /// <summary>
        /// 当前工程ID
        /// </summary>
        private string _currentHistoryId = "";

        private List<UndoRedoHandler> Handlers = new List<UndoRedoHandler>();
        
        public UtilTrace()
        {
            historyList = new List<HistoryEnt>();
        }

        /// <summary>
        /// 保存历史痕迹
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="data">数据</param>
        /// <param name="ent">注册实体</param>
        public void SaveHistoryTraces(string type, object data)
        {
            var index = 0;
            for (var i = 0; i < historyList.Count; i++)
            {
                if (historyList[i].id == _currentHistoryId)
                {
                    index = i;
                    break;
                }
            }
            index = index + 1;
            if (index < historyList.Count)
            {
                historyList.RemoveRange(index, historyList.Count - index);
            }
            var _his_data = new HistoryEnt();
            _his_data.id = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(type) && data != null)
            {
                _his_data.type = type;
                _his_data.data = data;
            }
            historyList.Add(_his_data);
            _currentHistoryId = _his_data.id;

            Handlers.ForEach(p =>
            {
                RefreshStatus(p);
            });
        }

        /// <summary>
        /// 恢复选中原来的选项卡
        /// </summary>
        /// <param name="history"></param>
        private void RestoreTabPage(HistoryEnt history)
        {
            Handlers.ForEach(p =>
            {
                p.RestoreTabPageEventHandler?.Invoke(history.data as TabPage);
            });
        }

        /// <summary>
        /// 向前撤销
        /// </summary>
        /// <returns></returns>
        public void RevokeTrace()
        {
            for (var i = 0; i < historyList.Count; i++)
            {
                if (historyList[i].id == _currentHistoryId)
                {
                    var ent = historyList[i - 1];
                    _currentHistoryId = ent.id;
                    if (ent.type == "prj")
                    {
                        for (var m = i - 1; m >= 0; m--)
                        {
                            if (historyList[m].type == "tab")
                            {
                                RestoreTabPage(historyList[m]);
                                break;
                            }
                        }
                        ReloadHistoryData(ent.data.DeepClone());
                    }
                    else
                    {
                        for (var m = i - 1; m >= 0; m--)
                        {
                            if (historyList[m].type == "prj")
                            {
                                ReloadHistoryData(historyList[m].data.DeepClone());
                                break;
                            }
                        }
                        RestoreTabPage(ent);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 后后撤销
        /// </summary>
        /// <returns></returns>
        public void BackTrace()
        {
            for (var i = 0; i < historyList.Count; i++)
            {
                if (historyList[i].id == _currentHistoryId)
                {
                    var ent = historyList[i + 1];
                    _currentHistoryId = ent.id;
                    if (ent.type == "prj")
                    {
                        for (var m = i + 1; m >= 0; m--)
                        {
                            if (historyList[m].type == "tab")
                            {
                                RestoreTabPage(historyList[m]);
                                break;
                            }
                        }
                        ReloadHistoryData(ent.data.DeepClone());
                    }
                    else
                    {
                        for (var m = i + 1; m >= 0; m--)
                        {
                            if (historyList[m].type == "prj")
                            {
                                ReloadHistoryData(historyList[m].data.DeepClone());
                                break;
                            }
                        }
                        RestoreTabPage(ent);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 注册撤销实体方法
        /// </summary>
        /// <param name="regEnt"></param>
        public void Register(UndoRedoHandler handler)
        {
            //注册当前锁定标识位
            for (var i = 0; i < historyList.Count; i++)
            {
                var ent = historyList[i];
                if (ent.id == _currentHistoryId)
                {
                    ent.lockkey = handler.LockKey;
                    break;
                }
            }

            if (!Handlers.Contains(handler))
            {
                Handlers.Add(handler);
            }
        }

        /// <summary>
        /// 注销撤销实体方法
        /// </summary>
        /// <param name="regEnt"></param>
        public void Unregister(UndoRedoHandler handler)
        {
            if (!Handlers.Contains(handler))
            {
                Handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 状态改变
        /// </summary>
        public void RefreshStatus(UndoRedoHandler handler)
        {
            bool isReachedBegin = true;
            bool isReachedEnd = true;
            for (var i = 0; i < historyList.Count; i++)
            {
                if (historyList[i].id == _currentHistoryId)
                {
                    if ((i - 1) >= 0)
                    {
                        isReachedBegin = false;
                    }
                    if ((i + 1) < historyList.Count)
                    {
                        isReachedEnd = false;
                    }
                    if (historyList[i].lockkey != null && historyList[i].lockkey == handler.LockKey)
                    {
                        isReachedBegin = true;
                    }
                    break;
                }
            }

            handler.ReachedBegin = isReachedBegin;
            handler.ReachedEnd = isReachedEnd;

            handler.RefreshButtonStatusEventHandler?.Invoke();
        }
        
        /// <summary>
        /// 调用重新加载页面的回调函数
        /// </summary>
        private void ReloadHistoryData(object data)
        {
            Handlers.ForEach(p =>
            {
                p.ReloadProjectEventHandler?.Invoke(data as Project);
            });
        }
        
        /// <summary>
        /// 历史痕迹实体
        /// </summary>
        private class HistoryEnt
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
        }
    }

    public class UndoRedoHandler: IDisposable
    {
        public static UndoRedoHandler MainInstance;

        static UndoRedoHandler()
        {
            MainInstance = new UndoRedoHandler();
        }

        private UtilTrace _trace;

        /// <summary>
        /// 向前撤销状态 true 不可用 false 可用
        /// </summary>
        public bool ReachedBegin { get; set; }

        /// <summary>
        /// 向后撤销状态 true 不可用 false 可用
        /// </summary>
        public bool ReachedEnd { get; set; }

        /// <summary>
        /// 锁定标识
        /// </summary>
        public string LockKey { get; set; }

        /// <summary>
        /// Undo图标区域
        /// </summary>
        public Rectangle UndoIconRect { get; set; }

        /// <summary>
        /// Redo图标区域
        /// </summary>
        public Rectangle RedoIconRect { get; set; }
        
        /// <summary>
        /// 重置数据事件对象
        /// </summary>
        public ReloadProjectDataEvent ReloadProjectEventHandler;

        /// <summary>
        /// 重置TabPage事件对象
        /// </summary>
        public RestoreTabPageEvent RestoreTabPageEventHandler;

        /// <summary>
        /// 获得当前最新项目数据事件对象
        /// </summary>
        public GetCurrentProjectEvent GetCurrentProjectEventHandler;

        /// <summary>
        /// 重置Undo,Redo按钮状态
        /// </summary>
        public Action RefreshButtonStatusEventHandler;

        public UndoRedoHandler(bool useMainTrace = false)
        {
            if (useMainTrace)
            {
                _trace = MainInstance._trace;
            }
            else
            {
                _trace = new UtilTrace();
            }
            
            LockKey = Guid.NewGuid().ToString();

            _trace.Register(this);
        }
        
        /// <summary>
        /// 保存历史痕迹
        /// </summary>
        public static void SaveHistoryTraces()
        {
            MainInstance.SaveProjectHistory();
        }

        /// <summary>
        /// 保存项目数据历史
        /// </summary>
        public void SaveProjectHistory()
        {
            Project data1 = null;
            GetCurrentProjectEventHandler?.Invoke(out data1);
            _trace.SaveHistoryTraces("prj", data1);
        }

        /// <summary>
        /// 保存Tab控件跳转历史
        /// </summary>
        public void SaveTabHistory(TabPage tab)
        {
            _trace.SaveHistoryTraces("tab", tab);
        }

        /// <summary>
        /// 向前撤销方法
        /// </summary>
        public void Undo()
        {
            _trace.RevokeTrace();

            _trace.RefreshStatus(this);
        }

        /// <summary>
        /// 向后撤销方法
        /// </summary>
        public void Redo()
        {
            _trace.BackTrace();

            _trace.RefreshStatus(this);
        }

        public void ShowIconsInPictureBoxes(PictureBox undoButton, PictureBox redoButton)
        {
            RefreshButtonStatusEventHandler += delegate ()
            {
                undoButton.Image = ReachedBegin ? Properties.Resources.undo : Properties.Resources.undohov;
                redoButton.Image = ReachedEnd ? Properties.Resources.redo : Properties.Resources.redohov;

                undoButton.Cursor = ReachedBegin ? Cursors.Default : Cursors.Hand;
                redoButton.Cursor = ReachedEnd ? Cursors.Default : Cursors.Hand;
            };

            undoButton.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (ReachedBegin == false)
                {
                    Undo();
                }
            };

            redoButton.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (ReachedEnd == false)
                {
                    Redo();
                }
            };

            _trace.RefreshStatus(this);
        }

        public void ShowIconsOnTabPage(JCBase.UI.TapPageTrans tabPage, Rectangle redoIconRect, Rectangle undoIconRect)
        {
            RedoIconRect = redoIconRect;
            UndoIconRect = undoIconRect;

            tabPage.OnDrawControl += new PaintEventHandler(DrawIcons);

            RefreshButtonStatusEventHandler += delegate ()
            {
                tabPage.Invalidate();
            };
            _trace.RefreshStatus(this);

            Form frm = tabPage.FindForm();
            frm.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (ReachedBegin == false && UndoIconRect.Contains(e.Location))
                {
                    Undo();
                }
                if (ReachedEnd == false && RedoIconRect.Contains(e.Location))
                {
                    Redo();
                }
            };

            frm.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (ReachedBegin == false && UndoIconRect.Contains(e.Location))
                {
                    frm.Cursor = Cursors.Hand;
                }
                else if (ReachedEnd == false && RedoIconRect.Contains(e.Location))
                {
                    frm.Cursor = Cursors.Hand;
                }
                else
                {
                    frm.Cursor = Cursors.Default;
                }
            };
        }

        private void DrawIcons(object sender, PaintEventArgs e)
        {
            JCBase.UI.TapPageTrans hostControl = sender as JCBase.UI.TapPageTrans;
            ////绘制撤销 20161227 add by axj
            //if (ShowUndoRedo)
            //{
            Bitmap picUndo = Properties.Resources.undo;
            Bitmap picRedo = Properties.Resources.redo;
            if (!ReachedBegin)
            {
                picUndo = Properties.Resources.undohov;
            }
            if (!ReachedEnd)
            {
                picRedo = Properties.Resources.redohov;
            }
            int w = 2;
            int h = 2;
            int wid = 0;
            if (hostControl.TitleLogo != null)
            {
                wid = hostControl.TitleLogo.Width;
            }
            Rectangle clientArea = hostControl.ClientRectangle;
            Graphics g = e.Graphics;
            Point _picArea_1 = new Point(clientArea.Width - picUndo.Width - picRedo.Width - wid - 10 - 20, clientArea.Top + 6);
            g.DrawImage(picUndo, _picArea_1.X, _picArea_1.Y, picUndo.Width - w, picUndo.Height - h);
            Point _picArea_2 = new Point(clientArea.Width - picRedo.Width - wid - 10 - 10, clientArea.Top + 6);
            g.DrawImage(picRedo, _picArea_2.X, _picArea_2.Y, picRedo.Width - w, picRedo.Height - h);
            UndoIconRect = new Rectangle(new Point(hostControl.Location.X + _picArea_1.X, hostControl.Location.Y + _picArea_1.Y), new Size(picUndo.Width - w, picUndo.Height - h));
            RedoIconRect = new Rectangle(new Point(hostControl.Location.X + _picArea_2.X, hostControl.Location.Y + _picArea_2.Y), new Size(picRedo.Width - w, picRedo.Height - h));
            //}
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)。
                    _trace.Unregister(this);

                    RefreshButtonStatusEventHandler = null;
                    ReloadProjectEventHandler = null;
                    RestoreTabPageEventHandler = null;
                    GetCurrentProjectEventHandler = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~UndoRedoHandler() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
