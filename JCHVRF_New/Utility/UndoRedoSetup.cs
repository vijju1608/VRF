using JCHVRF_New.Utility;

namespace JCHVRF_New.Utility
{
    public sealed class UndoRedoSetup
    {
        private static UndoRedoSetup instance = null;
        private static readonly object padlock = new object();
        public RegUndoEnt RegEnt = null;
        public UndoEnableEnt UndoEnable = null;
        public bool IsInitialized = false;
        public delegate void DoEvent();
        public static DoEvent ResetUndoRedo;
        private UndoRedoSetup()
        {
            UtilTrace.Ini();
            UndoEnable = new UndoEnableEnt();
            //UndoEnable.redoRect = new System.Drawing.Rectangle(912, 95, 16, 18);
            //UndoEnable.undoRect = new System.Drawing.Rectangle(884, 95, 16, 18);
            //this.tabControl1.OnDrawControl += new PaintEventHandler(UndoEnable.ShowIcons);

            RegEnt = new RegUndoEnt();
            RegEnt.funHandler += delegate ()
            {
                //BindToControl();
            };
            RegEnt.enable = UndoEnable;
            //newUtilTrace.UtilTrace.ViewModel = this;
            //UtilTrace.tbMain = tabControl1;
            //regEnt.vm = this;
            //regEnt.tb = tabControl1;
            //UtilTrace.SaveHistoryTraces();
            //UtilTrace.SaveHistoryTraces("tab", this.tabControl1.SelectedTab, null);
            UtilTrace.RegUndo(RegEnt);
            UtilTrace.ChkMainUndo += delegate ()
            {
                //BindToControl();
                RegEnt.ChkUndoEnable();
                //this.Invalidate();
            };
            IsInitialized = false;
        }

        public static UndoRedoSetup Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new UndoRedoSetup();
                    }
                    return instance;
                }
            }
        }

        public static void SetInstanceNull()
        {
            lock (padlock)
            {
                instance = null;
                ResetUndoRedo?.Invoke();
            }
        }
        public static void SetInstanceNullWithInitalizedProjectStack()
        {
            lock (padlock)
            {
                instance = null;
                ResetUndoRedo?.Invoke();
                UtilTrace.SaveHistoryTraces();
            }
        }
    }
}
