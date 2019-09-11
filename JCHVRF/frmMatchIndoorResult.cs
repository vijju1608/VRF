using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JCHVRF.Model;
using System.Threading;
using JCHVRF.VRFMessage;

namespace JCHVRF
{
    public partial class frmMatchIndoorResult : JCBase.UI.JCForm
    {
        //数据成员
        Project thisProject;//项目工程
        double outDBCool;//室外机制冷DB温度
        double outWBHeat;//室外机制热WB温度
        DataTable dtResult = null;//返回结果DT
        delegate void DoMatchIndoorEvent();//选型委托定义
        delegate void DoEvent();//事件定义
        List<RoomIndoor> roomIndoorList;//选中室内机列表
        public List<IndoorReselectionResult> ReselectionResultList = null;//返回结果集合
        public List<RoomIndoor> ReselectedIndoorList = null;//重新选型后室内机列表
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_roomIndoorList"></param>
        /// <param name="thisProj"></param>
        /// <param name="_outDBCool"></param>
        /// <param name="_outWBHeat"></param>
        public frmMatchIndoorResult(List<RoomIndoor> _roomIndoorList, Project thisProj, double _outDBCool, double _outWBHeat)
        {
            InitializeComponent();
            //初始化赋值
            thisProject = thisProj;
            outDBCool = _outDBCool;
            outWBHeat = _outWBHeat;
            dtResult = new DataTable();
            roomIndoorList = _roomIndoorList;
            ReselectionResultList = new List<IndoorReselectionResult>();
        }
        /// <summary>
        /// 初始化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMatchIndoorResult_Load(object sender, EventArgs e)
        {
            jcbtnClose.Enabled = false;
            jcbtnOK.Enabled = false;
            dtResult.Columns.Add("Model");
            dtResult.Columns.Add("Memo");
            dataGridView1.DataSource = dtResult;
            string strChecking = Msg.GetResourceString("IND_AUTO_ADJUST_CHECKING");
            foreach(var roomindoor in roomIndoorList)
            {
                IndoorReselectionResult ent = new IndoorReselectionResult();
                ent.IndoorNo = roomindoor.IndoorNO.ToString();
                ent.Name = roomindoor.IndoorFullName;
                ent.Message = strChecking;
                ent.Seccessful = false;
                ReselectionResultList.Add(ent);
                dtResult.Rows.Add(new object[] { ent.Name, ent.Message });
            }
            //定义选型委托对象
            DoMatchIndoorEvent matIn = DoMatchIndoor;
            var re = matIn.BeginInvoke(null, null); ;
        }
        /// <summary>
        /// 重新选型事件
        /// </summary>
        private void DoMatchIndoor()
        {
            List<string> stateList = new List<string>();
            MatchIndoor MatInd = new MatchIndoor();
            try
            {
                ReselectedIndoorList = MatInd.DoRoomIndoorReselection(roomIndoorList, thisProject, outDBCool, outWBHeat, out stateList);
            }
            catch (Exception ex)
            {
                DoEvent BtnEnableFun1 = delegate()
                {
                    jclblError.Text = ex.Message;
                    jclblError.Visible = true;
                    jcbtnClose.Enabled = true;
                };
                this.BeginInvoke(BtnEnableFun1);
                return;
            }
            //选型结果呈现，由主线程委托执行
            DoEvent BtnEnableFun = null;
            BtnEnableFun += delegate()
            {
                dtResult.Rows.Clear();
                foreach (string stateMsg in stateList)
                {
                    string[] arr = stateMsg.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    string indoorNo = arr[0];
                    var ent = ReselectionResultList.Find(p => p.IndoorNo == indoorNo);
                    switch (arr[1])
                    {
                        case "Indoor_NoChange":
                            ent.Seccessful = true;
                            ent.Message = Msg.MatchIndoor_OK;
                            break;
                        case "Indoor_ChangeModel":
                            ent.Seccessful = true;
                            RoomIndoor newRI = ReselectedIndoorList.Find(p => p.IndoorNO.ToString() == indoorNo);
                            string msg = "";
                            if (newRI != null)
                            {
                                msg = "=>" + newRI.IndoorFullName;
                            }
                            if (arr.Length > 2)
                            {
                                switch (arr[2])
                                {
                                    case "Indoor_Unbinding":
                                        msg += "; " + Msg.Indoor_Unbinding;
                                        break;
                                    case "Indoor_ResetAccessories":
                                        msg += "; " + Msg.Indoor_ResetAccessories;
                                        break;
                                }
                            }
                            ent.Message = msg;
                            break;
                        default:
                            if (ent.Seccessful)
                            {
                                ent.Message = "";
                                ent.Seccessful = false;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ent.Message))
                                {
                                    ent.Message += ";";
                                }
                            }
                            switch (arr[1])
                            {
                                case "IND_NOTMATCH":
                                    ent.Message += Msg.IND_NOTMATCH;
                                    break;
                                case "DATA_EXCEED":
                                    ent.Message += Msg.WARNING_DATA_EXCEED;
                                    break;
                                case "IND_NOTMEET_COOLING":
                                    ent.Message += Msg.IND_NOTMEET_COOLING;
                                    break;
                                case "IND_NOTMEET_HEATING":
                                    ent.Message += Msg.IND_NOTMEET_HEATING;
                                    break;
                                case "IND_NOTMEET_FA":
                                    ent.Message += Msg.IND_NOTMEET_FA;
                                    break;
                            }
                            break;
                    }
                }
                
                foreach (var ent in ReselectionResultList)
                {
                    dtResult.Rows.Add(new object[] { ent.Name, ent.Message });
                }
                jcbtnClose.Enabled = true;
                jcbtnOK.Enabled = true;
            };
            this.BeginInvoke(BtnEnableFun);
        }
        /// <summary>
        /// 确定按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        /// <summary>
        /// 关闭按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jcbtnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }


    }
}
