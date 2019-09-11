//********************************************************************
// 文件名: Global.cs
// 描述: 与多个界面操作相关的方法代码集合，主要目的简化界面的代码以及方法重用
// 作者: clh
// 创建时间: 2011-10-10
// 修改历史: --
//********************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.OleDb;
using System.Xml;

using JCBase.Util;
using JCBase.UI;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCBase.Utility;
using JCHVRF.VRFMessage;
using System.Windows.Forms;
using System.Drawing;

namespace JCHVRF
{
    /// <summary>
    /// 全局方法
    /// </summary>
    public class Global
    {
        #region 公共属性

        //public static bool IsSuperUser = CDL.Sec.IsSuperUser() || CDL.Sec.GetRegion() == CDL.Sec.ProductRegion.ALL;
        public static bool IsSuperUser = CDL.Sec.IsSuperUser("SVRF"); // 20140826
        public static bool IsDearler = CDL.Sec.IsDealer("SVRF");
        public static bool IsPriceValid = CDL.Sec.IsValidPrice("SVRF");

        #endregion

        #region 改变字体加粗设置
        /// <summary>
        /// 改变字体加粗设置（默认字体：Microsoft Sans Serif；默认尺寸：8.25f）
        /// 用于将已分配的房间编号节点加粗显示
        /// </summary>
        /// <param name="isBold">是否加粗</param>
        /// <returns>返回Font对象</returns>
        public static Font SetFont(bool isBold)
        {
            FontFamily ff = new FontFamily("Microsoft Sans Serif");
            float enSize = 8.25f;
            FontStyle style = FontStyle.Regular;
            //isBold ? FontStyle.Bold : FontStyle.Regular;
            GraphicsUnit unit = GraphicsUnit.Point;

            return new Font(ff, enSize, style, unit);
        }
        #endregion

        #region // 实现按房间编号排序的方法 已注销
        // 构造房间list的比较条件，使其按房间编号顺序排列
        /// <summary>
        /// 构造房间对象list的比较条件，使其按房间编号顺序排列
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        //public static int CompareRoomByName(Room x, Room y)
        //{
        //    if (x == null)
        //    {
        //        if (y == null)
        //            return 0;
        //        else
        //            return -1;
        //    }
        //    else
        //    {
        //        if (y == null)
        //            return 1;
        //        else
        //        {
        //            int retVal = x.Name.Length.CompareTo(y.Name.Length);
        //            if (retVal != 0)
        //                return retVal;
        //            else
        //            {
        //                return x.Name.CompareTo(y.Name);
        //            }
        //        }
        //    }
        //}
        #endregion

        #region 设置 DataGridView 的列属性，列名、列标题、点击列名自动排序等
        // 给项目中的 DataGridView 控件设置【HeaderText】属性值，已考虑中英文环境
        /// <summary>
        /// 给项目中的 DataGridView 控件设置【HeaderText】属性值，已考虑中英文环境
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="array"></param>
        public static void SetDGVHeaderText(ref DataGridView dgv, string[] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                dgv.Columns[i].HeaderText = array[i];
            }
        }

        /// <summary>
        /// 给项目中的ListView控件列标题文字赋值，已考虑中英文环境
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="array"></param>
        public static void SetLVHeaderText(ref ListView lv, string[] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                lv.Columns[i].Text = array[i];
            }
        }

        // 给项目中的 DataGridView 控件设置【Name】属性值
        /// <summary>
        /// 给项目中的 DataGridView 控件设置【Name】属性值
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="array"></param>
        public static void SetDGVName(ref DataGridView dgv, string[] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                dgv.Columns[i].Name = array[i];
            }
        }

        // 给项目中的 DataGridView 控件设置【DataPropertyName】属性值
        /// <summary>
        /// 给项目中的 DataGridView 控件设置【DataPropertyName】属性值
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="array"></param>
        public static void SetDGVDataName(ref DataGridView dgv, string[] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                dgv.Columns[i].DataPropertyName = array[i];
            }
        }

        // 设置指定的 DataGridView 点击列名时不自动排序
        /// <summary>
        /// 设置指定的 DataGridView 点击列名时不自动排序
        /// </summary>
        /// <param name="dgv"></param>
        public static void SetDGVNotSortable(ref DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }


        /// <summary>
        /// exchanger 对应的电源供应
        /// </summary>
        /// <returns></returns>
        public static DataTable InitPowerList(Project thisProject, string UnitType)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PowerKey");
            dt.Columns.Add("PowerValue");
            IndoorBLL indoor = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            DataTable dtExchnager = indoor.GetExchangerPowerType(UnitType, "-");
            DataTable dtPower = indoor.GetPowerSupplyList();
            if (dtExchnager.Rows.Count > 0 && dtPower.Rows.Count > 0)
            {

                foreach (DataRow dr in dtPower.Rows)
                {

                    foreach (DataRow dx in dtExchnager.Rows)
                    {
                        if (dr["Code"].ToString() == dx["ModelType"].ToString())
                        {

                            DataRow dl = dt.NewRow();
                            dl["PowerValue"] = dr["Code"].ToString();
                            dl["PowerKey"] = dr["Name"].ToString();
                            dt.Rows.Add(dl);
                            break;
                        }
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// exchanger 对应的类型
        /// </summary>
        /// <returns></returns>
        public static DataTable InitExchangerTypeList(Project thisProject)
        {
            IndoorBLL indoor = new IndoorBLL(thisProject.SubRegionCode, thisProject.RegionCode, thisProject.BrandCode);
            DataTable dtPower = indoor.GetExchangerTypeList();
            return dtPower;
        }



        #endregion

        #region 窗体公共方法
        /// 获取选中节点的顶层父节点
        /// <summary>
        /// 获取选中节点的顶层父节点
        /// </summary>
        /// <param name="selNode"></param>
        /// <returns></returns>
        public static TreeNode GetTopParentNode(TreeNode selNode)
        {
            if (selNode == null) return null;

            if (selNode.Level != 0)
                return GetTopParentNode(selNode.Parent);
            else
                return selNode;
        }

        /// 设置系统树节点的节点图标
        /// <summary>
        /// 设置系统树节点的节点图标
        /// </summary>
        /// <param name="tn">节点</param>
        /// <param name="imgIndex">图标编号</param>
        /// <param name="imgIndex_sel">节点选中时的图标编号</param>
        public static void SetTreeNodeImage(TreeNode tn, int imgIndex, int imgIndex_sel)
        {
            tn.ImageIndex = imgIndex;
            tn.SelectedImageIndex = imgIndex_sel;
        }

        /// 将系统对象添加到系统树节点
        /// <summary>
        /// 将系统对象添加到系统树节点
        /// </summary>
        /// <param name="tv"> TreeView 控件</param>
        /// <param name="sysItem"> 系统对象 </param>
        public static void BindTreeNodeOut(TreeNode nodeOut, SystemVRF sysItem, List<RoomIndoor> listRISelected, Project thisProject)
        {
            nodeOut.Tag = sysItem;
            nodeOut.Name = sysItem.Id;
            nodeOut.ForeColor = UtilColor.ColorOriginal;
            if (listRISelected == null || listRISelected.Count == 0 || sysItem.OutdoorItem == null)
            {
                nodeOut.Text = sysItem.Name;
                nodeOut.ForeColor = UtilColor.ColorWarning;
                sysItem.Ratio = 0;
                SetTreeNodeImage(nodeOut, 0, 0);
            }
            else
            {
                string sRatio = (sysItem.Ratio * 100).ToString("n0") + "%";
                nodeOut.Text = sysItem.Name + "[" + sysItem.OutdoorItem.AuxModelName + "] - " + sRatio;

                if (sysItem.SysType == SystemType.OnlyIndoor)
                {
                    if (sysItem.Ratio < 0.5 || sysItem.Ratio > sysItem.MaxRatio)
                        Global.SetTreeNodeImage(nodeOut, 0, 0);
                    else
                        Global.SetTreeNodeImage(nodeOut, 1, 3);
                }
                else if (sysItem.SysType == SystemType.OnlyFreshAirMulti || sysItem.SysType == SystemType.CompositeMode)
                {
                    //if (sysItem.Ratio < 0.8 || sysItem.Ratio > 1.05)
                    if (sysItem.Ratio < 0.8 || sysItem.Ratio > 1) //纯新风和混连比例范围80%~100% 20160819 by Yunxiao Lin
                        Global.SetTreeNodeImage(nodeOut, 0, 0);
                    else
                        Global.SetTreeNodeImage(nodeOut, 1, 3);
                    //混连时，新风机容量不能超过所有室内机容量的30% 20160819 by Yunxiao Lin
                    if (sysItem.SysType == SystemType.CompositeMode)
                    {
                        if (sysItem.RatioFA > 0.3)
                            Global.SetTreeNodeImage(nodeOut, 0, 0);
                        else
                            Global.SetTreeNodeImage(nodeOut, 1, 3);
                    }
                }
                else
                {
                    //if (sysItem.Ratio == 1) //updated on 20151130 clh,不是必须的限制条件
                    Global.SetTreeNodeImage(nodeOut, 1, 3); // 一对一通过！
                }
                //如果室外机有更新，必须重新验证Piping add on 20160819 by Yunxiao Lin
                if (sysItem.IsUpdated)
                    sysItem.IsPipingOK = false;

                if (sysItem.IsPipingOK)
                    nodeOut.ForeColor = UtilColor.ColorOriginal;
                else
                    nodeOut.ForeColor = UtilColor.ColorWarning;
            }

            nodeOut.Nodes.Clear();
            //RoomLoadIndexBLL roomBill = new RoomLoadIndexBLL();
            foreach (RoomIndoor ri in listRISelected)
            {
                TreeNode nodeIn = new TreeNode();
                nodeIn.Tag = ri;
                nodeIn.Name = ri.IndoorNO.ToString();
                //string floorRoomName = roomBill.getFloorRoomName(ri,thisProject);               
                if (thisProject.BrandCode == "Y")
                    nodeIn.Text = ri.IndoorName + "[" + ri.IndoorItem.Model_York + "]";
                else
                    nodeIn.Text = ri.IndoorName + "[" + ri.IndoorItem.Model_Hitachi + "]";
                nodeOut.Nodes.Add(nodeIn);
                Global.SetTreeNodeImage(nodeIn, 2, 4);
            }
            nodeOut.Expand();
        }

        #endregion

        /// 比较两个图标资源是否相同，相同则返回 true，否则返回 false
        /// <summary>
        /// 比较两个图标资源是否相同，相同则返回 true，否则返回 false
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns></returns>
        public static bool ImageCompareString(Bitmap firstImage, Bitmap secondImage)
        {
            MemoryStream ms = new MemoryStream();
            firstImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            String firstBitmap = Convert.ToBase64String(ms.ToArray());
            ms.Position = 0;

            secondImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            String secondBitmap = Convert.ToBase64String(ms.ToArray());

            if (firstBitmap.Equals(secondBitmap))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region 注册相关
        // 软件注册密码
        public const string ActivationPwd_CN = "CHiN@R%g1&5";
        public const string ActivationPwd_ME = "MidE@St(G(15";
        public const string ActivationPwd_ASIA = "AsI@r?G1$5";

        // 价格权限密码
        public const string ActivationPwd_Price_CN = "cHIn@P?#15";
        public const string ActivationPwd_Price_ME = "miDeAs~tp%1@5";
        public const string ActivationPwd_Price_ASIA = "@SiAp*1#5";

        #endregion


        #region 选型相关

        public enum CapacitySelectionMode
        {
            Normal,
            LA_Except_Brazil
        }

        private static CapacitySelectionMode GetCapacitySelectionMode(Project proj)
        {
            //Reflect the room temperature to actual IDU capacity Add region ANZ  add on 20190215 by xyj 
            if ((proj.RegionCode == "LA" && proj.SubRegionCode != "LA_BR") || proj.SubRegionCode == "ANZ")
            {
                return CapacitySelectionMode.LA_Except_Brazil;
            }
            return CapacitySelectionMode.Normal;
        }

        /// <summary>
        /// 获取室外机选型时的室内机工况温度
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="listRISelected"></param>
        /// <param name="inWB"></param>
        /// <param name="inDB"></param>
        private static void GetIDUWorkTemperature(CapacitySelectionMode csMode, List<RoomIndoor> listRISelected, out double inWB, out double inDB)
        {
            //拉美地区（除巴西）需要用平均温度 //add by Shen Junjie on 20181113
            //ANZ 也需要用平均温度 //add on 20190215 by xyj
            if (csMode == CapacitySelectionMode.LA_Except_Brazil)
            {
                GetIDUWorkTemperatureLA(listRISelected, out inWB, out inDB);
                return;
            }

            inWB = 9999;//取最小值
            inDB = 0;//取最大值

            foreach (RoomIndoor ri in listRISelected)
            {
                //add by axj 20170120 室内机工况温度
                //取室内机制冷工况的最小值
                if (inWB > ri.WBCooling)
                {
                    inWB = ri.WBCooling;
                }
                //取室内机制热工况的最大值
                if (inDB < ri.DBHeating)
                {
                    inDB = ri.DBHeating;
                }
            }
        }

        /// <summary>
        /// 获取室外机选型时的室内机工况温度, 拉美地区（除巴西）计算平均温度
        /// </summary>
        private static void GetIDUWorkTemperatureLA(List<RoomIndoor> listRISelected, out double inWB, out double inDB)
        {
            //用平均温度取代原来的工况温度参数 add by Shen Junjie on 20181031
            double sumIndoorsCapcityDB = 0; //室内机制冷容量和DB的乘积和 add by Shen Junjie on 20181031
            double sumIndoorsCapcityWB = 0; //室内机制冷容量和WB的乘积和 add by Shen Junjie on 20181031

            double tot_indstdcap_c = 0;  //室内机标称制冷容量之和
            double tot_indstdcap_h = 0;  //室内机标称制热容量之和

            foreach (RoomIndoor ri in listRISelected)
            {
                tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
                tot_indstdcap_h += ri.IndoorItem.HeatingCapacity;

                //计算平均工况温度 add by Shen Junjie on 20181031
                sumIndoorsCapcityWB += ri.WBCooling * ri.IndoorItem.CoolingCapacity;
                sumIndoorsCapcityDB += ri.DBHeating * ri.IndoorItem.HeatingCapacity;
            }
            inDB = sumIndoorsCapcityDB / tot_indstdcap_h; //平均温度
            inWB = sumIndoorsCapcityWB / tot_indstdcap_c; //平均温度
        }

        private static void CalacIndoorActualCapacity(SystemVRF curSystemItem, List<RoomIndoor> listRISelected,
            CapacitySelectionMode csMode, bool hasHeating, double tot_indhp, double tot_indcap_c, double tot_indcap_h)
        {
            // 对比室外机est cap.和室内机est cap.，取小的值 20170104 by Yunxiao Lin
            double ActualCoolingCapacity = curSystemItem.CoolingCapacity;
            double ActualHeatingCapacity = curSystemItem.HeatingCapacity;
            if (ActualCoolingCapacity > tot_indcap_c)
            {
                ActualCoolingCapacity = tot_indcap_c;
                curSystemItem.CoolingCapacity = tot_indcap_c;  //赋值给系统 on 2018-01-05 by xyj
            }
            if (ActualHeatingCapacity > tot_indcap_h)
            {
                ActualHeatingCapacity = tot_indcap_h;
                curSystemItem.HeatingCapacity = tot_indcap_h;  //赋值给系统 on 2018-01-05 by xyj
            }

            // 将修正后的室外机容量按匹数比例分配给室内机
            foreach (RoomIndoor ri in listRISelected)
            {
                ri.ActualHeatingCapacity = 0;
                if (csMode == CapacitySelectionMode.LA_Except_Brazil)
                {
                    //add by Shen Junjie on 20181105
                    //改成室内机实际容量=室外机实际容量*室内机的估算容量的占比 
                    ri.ActualCoolingCapacity = ActualCoolingCapacity * ri.CoolingCapacity / tot_indcap_c;
                    if (hasHeating)
                    {
                        //改成室内机实际容量=室外机实际容量*室内机的估算容量的占比
                        ri.ActualHeatingCapacity = ActualHeatingCapacity * ri.HeatingCapacity / tot_indcap_h;
                    }
                }
                else
                {
                    double indhp = ri.IndoorItem.Horsepower;
                    ri.ActualCoolingCapacity = ActualCoolingCapacity * indhp / tot_indhp;
                    if (hasHeating)
                    {
                        //非单冷模式或单冷系列需要分配Heating Capacity
                        ri.ActualHeatingCapacity = ActualHeatingCapacity * indhp / tot_indhp;
                    }
                }
                // 计算实际显热
                ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.SHF;
            }
        }

        ///// 自动选择匹配的室外机（包括非严格一对一的情况）
        ///// 水机选型增加FlowRateLevel参数 20170216 by Yunxiao Lin
        ///// <summary>
        ///// 自动选择匹配的室外机（包括非严格一对一的情况）
        ///// </summary>
        //public static SelectOutdoorResult DoSelectOutdoor(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, out List<string> ERRList)
        //{
        //    //OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode);
        //    OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);

        //    ERRList = new List<string>();
        //    ERRList.Clear();    // 每次选型前清空错误记录表
        //    curSystemItem.OutdoorItem = null;
        //    string OutdoorModelFull = "";

        //    MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL();

        //    // 若所选室内机数量为0，则终止选型
        //    if (listRISelected == null || listRISelected.Count == 0)
        //    {
        //        return SelectOutdoorResult.Null;
        //    }

        //    //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
        //    string productType = listRISelected[0].IndoorItem.ProductType;
        //    string series = listRISelected[0].IndoorItem.Series;

        //    DataTable dtOutdoorStd = new DataTable();
        //    // 获取室外机标准表（初次加载或者更改室外机类型时）
        //    if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
        //    {
        //        dtOutdoorStd = bll.GetOutdoorListStd();
        //        if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
        //        {
        //            // ERROR:室外机标准表无数据记录！
        //            ERRList.Add(Msg.DB_NODATA);
        //            return SelectOutdoorResult.Null;
        //        }
        //    }

        //    // 计算此时的室内机容量和（新风机与室内机分开汇总）
        //    double tot_indcap_c = 0;
        //    double tot_indcap_h = 0;
        //    double tot_FAcap = 0;
        //    double tot_indstdcap_c = 0;
        //    double ratioFA = 0;
        //    double tot_indcap_cOnly = 0;
        //    double tot_indhp = 0;
        //    //判断室内机中是否含有4 way 机型 20170702 by Yunxiao Lin
        //    bool exists4way = false;
        //    //记录最大的室内机匹数 20170702 by Yunxiao Lin
        //    double max_indhp = 0;
        //    foreach (RoomIndoor ri in listRISelected)
        //    {
        //        //增加4 way 室内机判断，用于IVX选型 20170702 by Yunxiao lin
        //        if (ri.IndoorItem.Type.Contains("Four Way"))
        //            exists4way = true;

        //        //记录最大的室内机匹数，用于IVX选型 20170702 by Yunxiao lin
        //        if (ri.IndoorItem.Horsepower > max_indhp)
        //            max_indhp = ri.IndoorItem.Horsepower;

        //        tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
        //        tot_indcap_h += ri.HeatingCapacity;
        //        tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
        //        tot_indhp += ri.IndoorItem.Horsepower;
        //        if (ri.IndoorItem.Flag == IndoorType.FreshAir)
        //        {
        //            if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
        //            {
        //                // JCHVRF:混连模式下，1680与2100新风机取另一条记录
        //                //Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.ProductType,thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType);
        //                Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType);
        //                if (inItem != null)
        //                {
        //                    inItem.Series = series;
        //                    ri.IndoorItem = inItem;
        //                }

        //            }
        //            tot_FAcap += ri.IndoorItem.CoolingCapacity;
        //        }
        //    }
        //    tot_indcap_cOnly = tot_indcap_c - tot_FAcap;

        //    bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
        //    #region //确定当前内机组合模式
        //    // 1.混连模式
        //    if (isComposite)
        //    {
        //        // 1-1.必须同时包含室内机与新风机；
        //        if (tot_indcap_cOnly == 0 || tot_FAcap == 0)
        //        {
        //            ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
        //            return SelectOutdoorResult.Null;
        //        }
        //    }
        //    else
        //    {
        //        // 2-1.不允许同时包含室内机和新风机；
        //        if (tot_indcap_cOnly > 0 && tot_FAcap > 0)
        //        {
        //            ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
        //            return SelectOutdoorResult.Null;
        //        }

        //        // 2.全室内机模式
        //        if (tot_indcap_cOnly > 0)
        //        {
        //            curSystemItem.SysType = SystemType.OnlyIndoor;
        //        }
        //        // 3.全新风机模式
        //        else if (tot_FAcap > 0)
        //        {
        //            curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

        //            if (listRISelected.Count == 1)
        //            {
        //                Indoor inItem = listRISelected[0].IndoorItem;

        //                #region 一对一新风机
        //                curSystemItem.SysType = SystemType.OnlyFreshAir;

        //                // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
        //                // 20160821 新增productType参数 by Yunxiao Lin
        //                //inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType);
        //                inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType);
        //                string UniqueOutdoorName = inItem.UniqueOutdoorName;

        //                //curSystemItem.OutdoorItem = bll.GetOutdoorItem(UniqueOutdoorName);
        //                //curSystemItem.OutdoorItem = bll.GetOutdoorItem(UniqueOutdoorName, productType);
        //                //按照设定的model取一对一室外机需要判断品牌。 20161024 by Yunxiao Lin
        //                //同一个新风机可能被多个系列共用，因此可能存在多个一对一室外机，用“/”分隔 20170330 by Yunxiao Lin
        //                if (!UniqueOutdoorName.Contains("/"))
        //                {
        //                    if (thisProject.BrandCode == "Y")
        //                        //curSystemItem.OutdoorItem = bll.GetOutdoorItem(UniqueOutdoorName, productType);
        //                        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, series);
        //                    else
        //                        //curSystemItem.OutdoorItem = bll.GetHitachiItem(UniqueOutdoorName, productType);
        //                        curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, series);
        //                }
        //                else
        //                {
        //                    string[] UniqueOutdoorNameList = UniqueOutdoorName.Split(new char[]{'/'});
        //                    foreach (string model in UniqueOutdoorNameList)
        //                    {
        //                        if (thisProject.BrandCode == "Y")
        //                            curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, series);
        //                        else
        //                            curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(model, series);
        //                        if (curSystemItem.OutdoorItem != null)
        //                            break;
        //                    }
        //                }
        //                if (curSystemItem.OutdoorItem == null)
        //                {
        //                    // ERROR:数据库中的一对一室外机ModelName写错
        //                    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
        //                    return SelectOutdoorResult.Null;
        //                }
        //                else
        //                {
        //                    curSystemItem.MaxRatio = 1;
        //                    curSystemItem.Ratio = inItem.CoolingCapacity / curSystemItem.OutdoorItem.CoolingCapacity;
        //                    // FreshAir时不需要估算容量,直接绑定室外机的标准值
        //                    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
        //                    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
        //                    curSystemItem.MaxPipeLength = curSystemItem.OutdoorItem.MaxPipeLength;
        //                    curSystemItem.MaxEqPipeLength = curSystemItem.OutdoorItem.MaxEqPipeLength;
        //                    curSystemItem.MaxOutdoorAboveHeight = curSystemItem.OutdoorItem.MaxOutdoorAboveHeight;
        //                    curSystemItem.MaxOutdoorBelowHeight = curSystemItem.OutdoorItem.MaxOutdoorBelowHeight;
        //                    curSystemItem.MaxDiffIndoorHeight = curSystemItem.OutdoorItem.MaxDiffIndoorHeight;
        //                    curSystemItem.MaxIndoorLength = curSystemItem.OutdoorItem.MaxIndoorLength;
        //                    curSystemItem.MaxPipeLengthwithFA = curSystemItem.OutdoorItem.MaxPipeLengthwithFA;
        //                    curSystemItem.MaxDiffIndoorLength = curSystemItem.OutdoorItem.MaxDiffIndoorLength;
        //                    //增加系统液管总长上限变量，用于兼容IVX选型 20170704 by Yunxiao lin
        //                    curSystemItem.MaxTotalPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //                    curSystemItem.MaxTotalPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //                    curSystemItem.MaxMKIndoorPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //                    curSystemItem.MaxMKIndoorPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //                    // 一对一新风机选型成功！
        //                    return SelectOutdoorResult.OK;
        //                }
        //                #endregion

        //            }
        //        }// 全新风机 END
        //    }// 模式确定 END
        //    #endregion

        //    SelectOutdoorResult returnType = SelectOutdoorResult.OK;

        //    #region // 遍历室外机标准表逐个筛选

        //    // update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
        //    double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
        //    double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
        //    //室外机选型改为判断Series 20161031 by Yunxiao Lin
        //    //DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "' and ProductType='"+productType+"'"+" and TypeImage <> ''");
        //    DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''","Model asc");
        //    // 遍历选型过程 START 
        //    foreach (DataRow r in rows)
        //    {
        //        // 检查最大连接数 
        //        //int maxIU = Convert.ToInt32(r["MaxIU"].ToString());
        //        int maxIU = 0;
        //        int.TryParse(r["MaxIU"].ToString(), out maxIU);
        //        if (maxIU < listRISelected.Count)
        //            continue;

        //        //检查IVX特殊连接 20170702 by Yunxiao Lin
        //        if (series.Contains("IVX"))
        //        {
        //            string ODUmodel = r["Model_Hitachi"].ToString().Trim();
        //            if (series.Contains("HVRNM2"))
        //            {
        //                //SMZ IVX 校验组合是否有效
        //                string IDUmodel = "";
        //                int IDUmodelsCount = 0;
        //                bool combinationErr = false;
        //                foreach (RoomIndoor ri in listRISelected)
        //                {
        //                    //获取IDU 型号名称和数量
        //                    if (IDUmodel == "")
        //                    {
        //                        IDUmodel = ri.IndoorItem.Model_Hitachi;
        //                        IDUmodelsCount++;
        //                    }
        //                    else if (IDUmodel != ri.IndoorItem.Model_Hitachi)
        //                    {
        //                        combinationErr = true;
        //                        break;
        //                    }
        //                    else
        //                        IDUmodelsCount++;
        //                }
        //                if (!combinationErr && IDUmodelsCount > 0)
        //                {
        //                    //将IDU型号名称和数量组合成 "model x count" 字符串，与数据库中预设的组合进行比对 20170702 by Yunxiao Lin
        //                    string IDUmodels = IDUmodel + " x " + IDUmodelsCount.ToString();
        //                    IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
        //                    if (!IVXCombll.existsCombination(ODUmodel, IDUmodels))
        //                        continue;
        //                }
        //                else
        //                    continue;
        //            }
        //            else if (series.Contains("H(R/Y)NM1Q"))
        //            {
        //                //HAPQ IVX
        //                double IVXoutstdhp = Convert.ToDouble(r["Horsepower"].ToString());
        //                curSystemItem.Ratio = Math.Round(tot_indhp / IVXoutstdhp, 3);
        //                //根据室外温度和室外机型号进行检查 20170702 by Yunxiao Lin
        //                if (curSystemItem.DBCooling <= -10)
        //                {
        //                    switch (ODUmodel)
        //                    {
        //                        case "RAS-3HRNM1Q":
        //                            //RAS-3HRNM1Q 不能与4-way IDU搭配使用
        //                            if(exists4way)
        //                                continue;
        //                            //容量配比限制
        //                            if (curSystemItem.Ratio < 0.85)
        //                                continue;
        //                            else if (curSystemItem.Ratio > 1)
        //                                continue;
        //                            break;
        //                        case "RAS-4HRNM1Q":
        //                        case "RAS-5HRNM1Q":
        //                        case "RAS-5HYNM1Q":
        //                            //室内机数量限制
        //                            if (listRISelected.Count > 4)
        //                                continue;
        //                            else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
        //                                continue;
        //                            //容量配比限制
        //                            if (curSystemItem.Ratio < 0.9)
        //                                continue;
        //                            else if (curSystemItem.Ratio > 1.12)
        //                                continue;
        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    switch (ODUmodel)
        //                    {
        //                        case "RAS-3HRNM1Q":
        //                            //RAS-3HRNM1Q 不能与4-way IDU搭配使用
        //                            if (exists4way)
        //                                continue;
        //                            //容量配比限制
        //                            if (curSystemItem.Ratio < 0.85)
        //                                continue;
        //                            else if (curSystemItem.Ratio > 1.2)
        //                                continue;
        //                            //室内机最大匹数限制
        //                            //1-for-1 室内机最大匹数3
        //                            if (listRISelected.Count == 1)
        //                            {
        //                                if (max_indhp > 3)
        //                                    continue;
        //                            }
        //                            //1-for-N 室内机最大匹数2.3
        //                            else if (listRISelected.Count > 1)
        //                            {
        //                                if (max_indhp > 2.3)
        //                                    continue;
        //                            }
        //                            break;
        //                        case "RAS-4HRNM1Q":
        //                            //室内机数量限制
        //                            if (listRISelected.Count > 5)
        //                                continue;
        //                            else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
        //                                continue;
        //                            //容量配比限制
        //                            if (listRISelected.Count <= 4)
        //                            {
        //                                if (curSystemItem.Ratio < 0.9)
        //                                    continue;
        //                                else if (curSystemItem.Ratio > 1.35)
        //                                    continue;
        //                            }
        //                            else if (listRISelected.Count <= 5)
        //                            {
        //                                if (curSystemItem.Ratio < 0.9)
        //                                    continue;
        //                                else if (curSystemItem.Ratio > 1.2)
        //                                    continue;
        //                            }
        //                            //室内机最大匹数限制
        //                            //1-for-1 室内机最大匹数4
        //                            if (listRISelected.Count == 1)
        //                            {
        //                                if (max_indhp > 4)
        //                                    continue;
        //                            }
        //                            //1-for-N 室内机最大匹数2.5
        //                            else if (listRISelected.Count > 1)
        //                            {
        //                                if (max_indhp > 2.5)
        //                                    continue;
        //                            }
        //                            break;
        //                        case "RAS-5HRNM1Q":
        //                        case "RAS-5HYNM1Q":
        //                            //室内机数量限制
        //                            if (listRISelected.Count > 5)
        //                                continue;
        //                            else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
        //                                continue;
        //                            //容量配比限制
        //                            if (listRISelected.Count <= 4)
        //                            {
        //                                if (curSystemItem.Ratio < 0.9)
        //                                    continue;
        //                                else if (curSystemItem.Ratio > 1.30)
        //                                    continue;
        //                            }
        //                            else if (listRISelected.Count <= 5)
        //                            {
        //                                if (curSystemItem.Ratio < 0.9)
        //                                    continue;
        //                                else if (curSystemItem.Ratio > 1.2)
        //                                    continue;
        //                            }
        //                            //室内机最大匹数限制
        //                            //1-for-1 室内机最大匹数4
        //                            if (listRISelected.Count == 1)
        //                            {
        //                                if (max_indhp > 4)
        //                                    continue;
        //                            }
        //                            //1-for-N 室内机最大匹数3
        //                            else if (listRISelected.Count > 1)
        //                            {
        //                                if (max_indhp > 3)
        //                                    continue;
        //                            }
        //                            break;
        //                    }
        //                }

        //            }
        //        }

        //        // 检查容量配比率（此处仅校验上限值）
        //        double outstdcap_c = Convert.ToDouble(r["CoolCapacity"].ToString());
        //        double outstdhp = Convert.ToDouble(r["Horsepower"].ToString());
        //        curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
        //        ratioFA = Math.Round(tot_FAcap / outstdcap_c, 3);
        //        curSystemItem.RatioFA = ratioFA;

        //        if (curSystemItem.SysType == SystemType.OnlyIndoor)
        //        {
        //            // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
        //            if (curSystemItem.Ratio < 0.5)
        //            {
        //                //OutdoorModelFull = r["ModelFull"].ToString();
        //                //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
        //                //break;
        //                //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
        //                continue;
        //            }
        //            if (curSystemItem.Ratio > curSystemItem.MaxRatio)
        //                continue;
        //        }
        //        else
        //        {
        //            // 多新风机或者混连模式，则配比率校验规则有变
        //            if (curSystemItem.Ratio < 0.8)
        //            {
        //                OutdoorModelFull = r["ModelFull"].ToString();
        //                ERRList.Add(Msg.OUTD_RATIO_Composite);
        //                break;
        //            }
        //            if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
        //                continue;
        //            if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
        //                continue; // add on 20160307 clh 
        //        }

        //        // 3、比较估算容量与室内机容量和
        //        //Outdoor outItem = bll.GetOutdoorItem(r["ModelFull"].ToString());
        //        //utdoor outItem = bll.GetOutdoorItem(r["ModelFull"].ToString(), productType);
        //        Outdoor outItem = bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
        //        //curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.DBCooling, inWB, false);
        //        //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
        //        if (!outItem.ProductType.Contains("Water Source"))
        //            //curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.DBCooling, inWB, false);
        //            curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.DBCooling, inWB, false, curSystemItem);
        //        else
        //            //curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.IWCooling, inWB, false);
        //            curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.IWCooling, inWB, false, curSystemItem);

        //        if (thisProject.IsCoolingModeEffective)
        //        {
        //            curSystemItem.PipingLengthFactor = (double)pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
        //            if (curSystemItem.PipingLengthFactor == 0)
        //            {
        //                string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        //                double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
        //                double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
        //                JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

        //                return SelectOutdoorResult.Null;
        //            }

        //            curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
        //            if (curSystemItem.CoolingCapacity < tot_indcap_c * curSystemItem.DiversityFactor) // updated on 20140625 clh
        //                continue;
        //        }
        //        //  Hitachi的Fresh Air 不需要比较HeatCapacity值
        //        if (thisProject.IsHeatingModeEffective && !outItem.ProductType.Contains(", CO"))
        //        {
        //            //curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.WBHeating, inDB, true);
        //            //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
        //            if (!outItem.ProductType.Contains("Water Source"))
        //                //curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.WBHeating, inDB, true);
        //                curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.WBHeating, inDB, true, curSystemItem);
        //            else
        //                //curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.MaxRatio, curSystemItem.IWHeating, inDB, true);
        //                curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, curSystemItem.Ratio, curSystemItem.IWHeating, inDB, true, curSystemItem);
        //            //水冷机不需要除霜修正
        //            if (!outItem.ProductType.Contains("Water Source"))
        //            {
        //                double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
        //                curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
        //            }
        //            if (curSystemItem.HeatingCapacity < tot_indcap_h * curSystemItem.DiversityFactor) // changed on 20130710
        //                continue;
        //        }

        //        //海拔修正 add on 20160517 by Yunxiao Lin
        //        //注意某些机型可能无此限制? Wait check
        //        if (SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor)
        //        {
        //            //获取海拔修正系数
        //            double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
        //            curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;
        //            if (curSystemItem.CoolingCapacity < tot_indcap_c * curSystemItem.DiversityFactor)
        //                continue;
        //            //if (thisProject.IsHeatingModeEffective)
        //            // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
        //            // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
        //            if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
        //            {
        //                curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
        //                if (curSystemItem.HeatingCapacity < tot_indcap_h * curSystemItem.DiversityFactor)
        //                    continue;
        //            }
        //        }

        //        ////除霜修正,只有制热容量需要此修正 add on 20160525 by Yunxiao Lin 
        //        //if (!outItem.ProductType.Contains(", CO") && thisProject.IsHeatingModeEffective) //注意只有具备制热功能的室外机需要除霜修正
        //        //{
        //        //    double dcf = getDefrostCorrectionFactor(curSystemItem.DBCooling);
        //        //    curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * dcf;
        //        //    if (curSystemItem.HeatingCapacity < tot_indcap_h * curSystemItem.DiversityFactor)
        //        //        continue;
        //        //}

        //        // 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
        //        if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
        //        {
        //            if (curSystemItem.Ratio > 1)
        //            {
        //                if (!curSystemItem.AllowExceedRatio)
        //                    continue;
        //            }
        //        }

        //        OutdoorModelFull = r["ModelFull"].ToString();
        //        break; // 找到合适的室外机即跳出循环

        //    }
        //    // 遍历自动选型 END
        //    #endregion

        //    if (curSystemItem.SysType == SystemType.OnlyIndoor)
        //    {
        //        // 全室内机
        //        // 2-2.内机总冷量配置率为50%～130%；
        //        if (curSystemItem.Ratio > curSystemItem.MaxRatio)
        //        {
        //            //OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
        //            //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
        //            //returnType = SelectOutdoorResult.NotMatch;
        //            //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
        //        }
        //    }
        //    else
        //    {
        //        // 多新风机或者混连模式，则配比率校验规则有变

        //        // 1-2.新风机冷量与室外机的配置率不大于30%
        //        if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
        //        {
        //            if (string.IsNullOrEmpty(OutdoorModelFull))
        //                OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();

        //            ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
        //            returnType = SelectOutdoorResult.NotMatch;
        //        }

        //        // 1-3.内机总冷量配置率为80%～105%；
        //        if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
        //        {
        //            OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
        //            ERRList.Add(Msg.OUTD_RATIO_Composite);
        //            returnType = SelectOutdoorResult.NotMatch;
        //        }
        //    }


        //    if (!string.IsNullOrEmpty(OutdoorModelFull))
        //    {
        //        //curSystemItem.OutdoorItem = bll.GetOutdoorItem(OutdoorModelFull);
        //        //curSystemItem.OutdoorItem = bll.GetOutdoorItem(OutdoorModelFull, productType);
        //        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
        //        // updated by clh
        //        if (curSystemItem.OutdoorItem != null)
        //        {
        //            curSystemItem.MaxPipeLength = curSystemItem.OutdoorItem.MaxPipeLength;
        //            curSystemItem.MaxEqPipeLength = curSystemItem.OutdoorItem.MaxEqPipeLength;
        //            curSystemItem.MaxOutdoorAboveHeight = curSystemItem.OutdoorItem.MaxOutdoorAboveHeight;
        //            curSystemItem.MaxOutdoorBelowHeight = curSystemItem.OutdoorItem.MaxOutdoorBelowHeight;
        //            curSystemItem.MaxDiffIndoorHeight = curSystemItem.OutdoorItem.MaxDiffIndoorHeight;
        //            curSystemItem.MaxIndoorLength = curSystemItem.OutdoorItem.MaxIndoorLength;
        //            curSystemItem.MaxPipeLengthwithFA = curSystemItem.OutdoorItem.MaxPipeLengthwithFA;
        //            curSystemItem.MaxDiffIndoorLength = curSystemItem.OutdoorItem.MaxDiffIndoorLength;
        //            //增加系统液管总长上限变量，用于兼容IVX选型 20170704 by Yunxiao lin
        //            curSystemItem.MaxTotalPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //            curSystemItem.MaxTotalPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //            curSystemItem.MaxMKIndoorPipeLength = curSystemItem.OutdoorItem.MaxTotalPipeLength;
        //            curSystemItem.MaxMKIndoorPipeLength_MaxIU = curSystemItem.OutdoorItem.MaxTotalPipeLength_MaxIU;
        //            if (series.Contains("IVX"))
        //            {
        //                //IVX系统根据环境温度的不同，管长上限会变化 20170704 by Yunxiao lin
        //                if (series.Contains("H(R/Y)NM1Q")) //目前仅有HAPQ的H(R/Y)NM1Q需要改变管长上限 20170704 by Yunxiao Lin
        //                {
        //                    if (curSystemItem.DBCooling <= -10)
        //                    {
        //                        switch (curSystemItem.OutdoorItem.Model_Hitachi)
        //                        {
        //                            case "RAS-3HRNM1Q":
        //                                curSystemItem.MaxPipeLength = 30;
        //                                curSystemItem.MaxTotalPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 40;
        //                                curSystemItem.MaxIndoorLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength = 5;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 5;
        //                                break;
        //                            case "RAS-4HRNM1Q":
        //                            case "RAS-5HRNM1Q":
        //                            case "RAS-5HYNM1Q":
        //                                curSystemItem.MaxPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 40;
        //                                curSystemItem.MaxIndoorLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength = 5;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 5;
        //                                break;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        switch (curSystemItem.OutdoorItem.Model_Hitachi)
        //                        {
        //                            case "RAS-3HRNM1Q":
        //                                curSystemItem.MaxPipeLength = 30;
        //                                curSystemItem.MaxTotalPipeLength = 40;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 40;
        //                                curSystemItem.MaxIndoorLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength = 5;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 5;
        //                                break;
        //                            case "RAS-4HRNM1Q":
        //                            case "RAS-5HRNM1Q":
        //                            case "RAS-5HYNM1Q":
        //                                curSystemItem.MaxPipeLength = 50;
        //                                curSystemItem.MaxTotalPipeLength = 60;
        //                                curSystemItem.MaxTotalPipeLength_MaxIU = 60;
        //                                curSystemItem.MaxIndoorLength = 20;
        //                                curSystemItem.MaxMKIndoorPipeLength = 10;
        //                                curSystemItem.MaxMKIndoorPipeLength_MaxIU = 10;
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //            return returnType;
        //        }

        //    }
        //    if((thisProject!=null) && (thisProject.ProductType.Contains("Water Source")))
        //        ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
        //    else    
        //        ERRList.Add(Msg.OUTD_NOTMATCH);
        //    return SelectOutdoorResult.Null;
        //}

        //#region 计算除霜修正系数 add on 20160525 by Yunxiao Lin
        ///// <summary>
        ///// 计算除霜修正系数
        ///// </summary>
        ///// <param name="tmpDB">室外干球温度(摄氏度)</param>
        ///// <returns></returns>
        //public static double getDefrostCorrectionFactor(double tmpDB)
        //{
        //    double dcf = 1.0d;
        //    if (tmpDB >= 6.1d)
        //        dcf = 1.0d;
        //    else if (tmpDB >= 5d)
        //        dcf = 0.9d;
        //    else if (tmpDB >= 3.9d)
        //        dcf = 0.88d;
        //    else if (tmpDB >= 1.7d)
        //        dcf = 0.86d;
        //    else if (tmpDB >= -0.6d)
        //        dcf = 0.85d;
        //    else if (tmpDB >= -2.8d)
        //        dcf = 0.88d;
        //    else if (tmpDB >= -5d)
        //        dcf = 0.93d;
        //    else if (tmpDB >= -7.2d)
        //        dcf = 0.95d;
        //    return dcf;
        //}
        //#endregion

        //#region 计算海拔修正系数 add on 20160517 by Yunxiao Lin
        ///// <summary>
        ///// 计算室外机海拔修正系数
        ///// </summary>
        ///// <param name="Altitude">海拔</param>
        ///// <returns></returns>
        //public static double getAltitudeCorrectionFactor(double Altitude)
        //{
        //    double acf = 1.0d;  //0ft
        //    double a = Altitude;
        //    if (a >= 305d)
        //    {
        //        if (a >= 305d && a < 610d) //1000ft
        //            acf = 0.97d;
        //        else if (a < 914d)  //2000ft
        //            acf = 0.93d;
        //        else if (a < 1219d)  //3000ft
        //            acf = 0.9d;
        //        else if (a < 1524d)  //4000ft
        //            acf = 0.87d;
        //        else if (a < 1829d)  //5000ft
        //            acf = 0.83d;
        //        else if (a < 2133d)  //6000ft
        //            acf = 0.8d;
        //        else if (a < 2438d)  //7000ft
        //            acf = 0.77d;
        //        else if (a < 2743d)  //8000ft
        //            acf = 0.75d;
        //        else if (a < 3048d)  //9000ft
        //            acf = 0.72d;
        //        else
        //            acf = 0.69d;   //10000ft    
        //    }
        //    return acf;
        //}
        //#endregion
        ///// 根据Oudoor Inlet Air Temp.(DB)，获取外机的Defrosting factor
        ///// <summary>
        ///// 根据Oudoor Inlet Air Temp.(DB)，获取外机的Defrosting factor
        ///// 1，仅Heating工况；
        ///// 2，-7～7
        ///// </summary>
        ///// <param name="OutdoorDB"></param>
        ///// <returns></returns>
        //public static double GetDefrostingfactor(double OutdoorDB)
        //{
        //    double Defrostingfactor = 0;
        //    if (OutdoorDB <= -7)
        //    {
        //        Defrostingfactor = 0.95;
        //    }
        //    else if (OutdoorDB <= -5)
        //    {
        //        Defrostingfactor = 0.93;
        //    }
        //    else if (OutdoorDB <= -3)
        //    {
        //        Defrostingfactor = 0.88;
        //    }
        //    else if (OutdoorDB <= 0)
        //    {
        //        Defrostingfactor = 0.85;
        //    }
        //    else if (OutdoorDB <= 3)
        //    {
        //        Defrostingfactor = 0.87;
        //    }
        //    else if (OutdoorDB <= 5)
        //    {
        //        Defrostingfactor = 0.9;
        //    }
        //    else if (OutdoorDB <= 7)
        //    {
        //        Defrostingfactor = 1;
        //    }
        //    else if (OutdoorDB > 7)
        //    {
        //        Defrostingfactor = 1;
        //    }
        //    return Defrostingfactor;
        //}

        /// 自动选择匹配的室外机-室外机优先（包括非严格一对一的情况）20161110 add by Yunxiao Lin
        /// 增加公差参数，默认为0.05 20161125 by Yunxiao Lin
        /// SystemVRF增加FlowRateLevel参数 20170216 by Yunxiao Lin
        /// <summary>
        /// 自动选择匹配的室外机-室外机优先（包括非严格一对一的情况）
        /// </summary>
        public static SelectOutdoorResult DoSelectOutdoorODUFirst(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, out List<string> ERRList, out List<string> MSGList)
        {
            if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
                return DoSelectUniversalODUFirst(curSystemItem, listRISelected, thisProject, curSystemItem.Series, out ERRList, out MSGList);

            ERRList = new List<string>();
            MSGList = new List<string>();
            ERRList.Clear();    // 每次选型前清空错误记录表
            MSGList.Clear();
            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }

            CapacitySelectionMode csMode = GetCapacitySelectionMode(thisProject);

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            //string productType = listRISelected[0].IndoorItem.ProductType;
            //bool hasHeating = thisProject.IsHeatingModeEffective && !productType.Contains(", CO");
            string series = listRISelected[0].IndoorItem.Series;
            bool isCoolingOnly = series.Contains(" CO,");
            bool hasHeating = thisProject.IsHeatingModeEffective && !isCoolingOnly;

            if (curSystemItem != null && curSystemItem.IDUFirst && !series.Contains("IVX"))
                return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            double tolerance = 0.05;

            if (curSystemItem.OutdoorItem != null)
            {
                //记住series以便自动重新选型时传入Series  add by Shen Junjie on 2018/3/29
                curSystemItem.Series = curSystemItem.OutdoorItem.Series;
            }
            //curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL(thisProject);

            DataTable dtOutdoorStd = new DataTable();
            // 获取室外机标准表（初次加载或者更改室外机类型时）
            if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
                if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
                {
                    // ERROR:室外机标准表无数据记录！
                    ERRList.Add(Msg.DB_NODATA);
                    return SelectOutdoorResult.Null;
                }
            }
            //add by axj 20170120 室内机工况温度
            double inWB = 0; //制冷湿球温度
            double inDB = 0; //制热干球温度

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double ratioFA = 0;
            double tot_indhpOnly = 0;
            double tot_indhp = 0;

            //判断室内机中是否含有4 way 机型 20170702 by Yunxiao Lin
            bool exists4way = false;
            //记录最大的室内机匹数 20170702 by Yunxiao Lin
            double max_indhp = 0;
            //判断室内机中是否含有Hydro Free 20171220 by Yunxiao Lin
            bool existsHydroFree = false;
            //计算Hydro Free总容量和非Hydro Free室内机的总容量 20171220 by Yunxiao Lin
            double tot_HDhp = 0;
            double tot_noHDhp = 0;
            double ratioNoHDIU = 0;
            double ratioHD = 0;
            double tot_HP_KPI_X4E = 0; // KPI-X4E (全热交换机)的总匹数
            double outstdhp = 0;
            foreach (RoomIndoor ri in listRISelected)
            {
                //增加4 way 室内机判断，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Type.Contains("Four Way"))
                    exists4way = true;

                //记录最大的室内机匹数，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Horsepower > max_indhp)
                    max_indhp = ri.IndoorItem.Horsepower;

                //增加Hydro Free室内机判断,并分别统计系统内Hydro Free和非Hydro Free室内机的容量 20171220 by Yunxiao Lin
                if (ri.IndoorItem.Type.Contains("Hydro Free"))
                {
                    existsHydroFree = true;
                    tot_HDhp += ri.IndoorItem.Horsepower;
                }
                else
                {
                    tot_noHDhp += ri.IndoorItem.Horsepower;
                }

                if (ri.IndoorItem.Type == "Total Heat Exchanger (KPI-X4E)")
                {
                    tot_HP_KPI_X4E += ri.IndoorItem.Horsepower;
                }

                tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                tot_indcap_h += ri.HeatingCapacity;
                tot_indhp += ri.IndoorItem.Horsepower;

                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                    {
                        // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                        Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                        if (inItem != null)
                        {
                            inItem.Series = series;
                            ri.SetIndoorItemWithAccessory(inItem);
                        }

                    }
                    tot_FAhp += ri.IndoorItem.Horsepower;

                }
            }
            tot_indhpOnly = tot_indhp - tot_FAhp;

            //获取室内机工况温度 add by Shen Junjie on 20181113
            GetIDUWorkTemperature(csMode, listRISelected, out inWB, out inDB);

            bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
            #region //确定当前内机组合模式
            // 1.混连模式
            if (isComposite)
            {
                // 1-1.必须同时包含室内机与新风机；
                if (tot_indhpOnly == 0 || tot_FAhp == 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
                    return SelectOutdoorResult.Null;
                }
            }
            else
            {
                // 2-1.不允许同时包含室内机和新风机；
                if (tot_indhpOnly > 0 && tot_FAhp > 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
                    return SelectOutdoorResult.Null;
                }

                // 2.全室内机模式
                if (tot_indhpOnly > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyIndoor;
                }
                // 3.全新风机模式
                else if (tot_FAhp > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

                    if (listRISelected.Count == 1)
                    {
                        Indoor inItem = listRISelected[0].IndoorItem;

                        #region 一对一新风机
                        // 一对一新风机不适用于 SMZ Gen.2  on 20180918 
                        bool isSMZ = false;
                        //Add FSNC7B/5B by Yunxiao Lin 20190107
                        if (!string.IsNullOrEmpty(series) && (series.Contains("FSNP") || series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC7B") || series.Contains("FSNC5B")))
                        {
                            isSMZ = true;
                        }
                        if (!isSMZ)
                        {
                            curSystemItem.SysType = SystemType.OnlyFreshAir;
                            // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                            // 20160821 新增productType参数 by Yunxiao Lin
                            inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(inItem.ModelFull, inItem.Type, false, inItem.ProductType, inItem.Series);
                            if (inItem != null)
                            {
                                string UniqueOutdoorName = inItem.UniqueOutdoorName;

                                //按照设定的model取一对一室外机需要判断品牌。 20161024 by Yunxiao Lin
                                //同一个新风机可能被多个系列共用，因此可能存在多个一对一室外机，用“/”分隔 20170330 by Yunxiao Lin
                                if (!UniqueOutdoorName.Contains("/"))
                                {
                                    if (thisProject.BrandCode == "Y")
                                        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, series);
                                    else
                                        curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, series);
                                }
                                else
                                {
                                    string[] UniqueOutdoorNameList = UniqueOutdoorName.Split(new char[] { '/' });
                                    foreach (string model in UniqueOutdoorNameList)
                                    {
                                        if (thisProject.BrandCode == "Y")
                                            curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, series);
                                        else
                                            curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(model, series);
                                        if (curSystemItem.OutdoorItem != null)
                                            break;
                                    }
                                }
                                if (curSystemItem.OutdoorItem == null)
                                {
                                    // ERROR:数据库中的一对一室外机ModelName写错
                                    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
                                    return SelectOutdoorResult.Null;
                                }
                                else
                                {
                                    curSystemItem.MaxRatio = 1;
                                    //Connection Ratio改为由HorsePower对比计算
                                    curSystemItem.Ratio = inItem.Horsepower / curSystemItem.OutdoorItem.Horsepower;
                                    // FreshAir时不需要估算容量,直接绑定室外机的标准值
                                    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                    //一对一新风机将室外机的Capacity直接绑定到RoomIndoor 20161114 by Yunxiao Lin
                                    listRISelected[0].ActualCoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                    listRISelected[0].ActualHeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                    listRISelected[0].ActualSensibleHeat = 0d;

                                    pipBll.SetPipingLimitation(curSystemItem);
                                    // 一对一新风机选型成功！
                                    return SelectOutdoorResult.OK;
                                }
                            }
                        }
                        #endregion

                    }
                }// 全新风机 END
            }// 模式确定 END
            #endregion

            SelectOutdoorResult returnType = SelectOutdoorResult.OK;

            #region // 遍历室外机标准表逐个筛选
            //delete by axj 20170120
            //// update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
            //double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
            //室外机选型改为判断Series 20161031 by Yunxiao Lin
            DataRow[] rows = dtOutdoorStd.Select(" SubString(ModelFull,11,1)='" + curSystemItem.Power + "' and UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''", "Model asc");
            // 遍历选型过程 START 
            foreach (DataRow r in rows)
            {
                // 检查最大连接数 
                int maxIU = 0;
                double outstdcap_c = Convert.ToDouble(r["CoolCapacity"].ToString());
                int.TryParse(r["MaxIU"].ToString(), out maxIU);
                if (maxIU < listRISelected.Count)
                {
                    if (tot_indcap_c <= outstdcap_c)
                        MSGList.Add(Msg.MSGLIST_MAX_IDU_NUMBER);
                    continue;
                }

                //检查IVX特殊连接 20170702 by Yunxiao Lin
                if (series.Contains("IVX"))
                {
                    string ODUmodel = r["Model_Hitachi"].ToString().Trim();
                    if (series.Contains("HVRNM2"))
                    {
                        #region//SMZ IVX 校验组合是否有效
                        string IDUmodel = "";
                        int IDUmodelsCount = 0;
                        bool combinationErr = false;
                        foreach (RoomIndoor ri in listRISelected)
                        {
                            //获取IDU 型号名称和数量
                            if (IDUmodel == "")
                            {
                                IDUmodel = ri.IndoorItem.Model_Hitachi;
                                IDUmodelsCount++;
                            }
                            else if (IDUmodel != ri.IndoorItem.Model_Hitachi)
                            {
                                combinationErr = true;
                                break;
                            }
                            else
                                IDUmodelsCount++;
                        }
                        if (!combinationErr && IDUmodelsCount > 0)
                        {
                            //将IDU型号名称和数量组合成 "model x count" 字符串，与数据库中预设的组合进行比对 20170702 by Yunxiao Lin
                            string IDUmodels = IDUmodel + " x " + IDUmodelsCount.ToString();
                            IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                            if (!IVXCombll.existsCombination(ODUmodel, IDUmodels))
                            {
                                if (tot_indcap_c <= outstdcap_c)
                                    MSGList.Add(Msg.MSGLIST_IVX_COMBINATION);
                                continue;
                            }
                        }
                        else
                        {
                            if (tot_indcap_c <= outstdcap_c)
                                MSGList.Add(Msg.MSGLIST_IVX_INVALID);
                            continue;
                        }
                        #endregion
                    }
                    else if (series.Contains("H(R/Y)NM1Q"))
                    {
                        #region//HAPQ IVX
                        double IVXoutstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                        curSystemItem.Ratio = Math.Round(tot_indhp / IVXoutstdhp, 3);
                        //根据室外温度和室外机型号进行检查 20170702 by Yunxiao Lin
                        if (curSystemItem.DBCooling <= -10)
                        {
                            switch (ODUmodel)
                            {
                                case "RAS-3HRNM1Q":
                                    //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                    if (exists4way)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_3HRNM1Q_INFO1);
                                        continue;
                                    }
                                    //容量配比限制
                                    if (curSystemItem.Ratio < 0.85)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-3HRNM1Q", "85%"));
                                        continue;
                                    }
                                    else if (curSystemItem.Ratio > 1)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-3HRNM1Q", "100%"));
                                        continue;
                                    }
                                    break;
                                case "RAS-4HRNM1Q":
                                case "RAS-5HRNM1Q":
                                case "RAS-5HYNM1Q":
                                    //室内机数量限制
                                    if (listRISelected.Count > 4)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("RAS-5HYNM1Q", 4));
                                        continue;
                                    }
                                    else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("Four Way:RAS-5HYNM1Q", 3));
                                        continue;
                                    }
                                    //容量配比限制
                                    if (curSystemItem.Ratio < 0.9)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-5HYNM1Q", "90%"));
                                        continue;
                                    }
                                    else if (curSystemItem.Ratio > 1.12)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-5HYNM1Q", "112%"));
                                        continue;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (ODUmodel)
                            {
                                case "RAS-3HRNM1Q":
                                    //容量配比限制
                                    if (curSystemItem.Ratio < 0.85)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-3HRNM1Q", "85%"));
                                        continue;
                                    }
                                    else if (curSystemItem.Ratio > 1.2)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-3HRNM1Q", "120%"));
                                        continue;
                                    }
                                    //室内机最大匹数限制
                                    //1-for-1 室内机最大匹数3
                                    if (listRISelected.Count == 1)
                                    {
                                        if (max_indhp > 3)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-3HRNM1Q", "3P", "'1-for-1'"));
                                            continue;
                                        }
                                    }
                                    //1-for-N 室内机最大匹数2.3
                                    else if (listRISelected.Count > 1)
                                    {
                                        if (max_indhp > 2.3)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-3HRNM1Q", "3P", "'1-for-N'"));
                                            continue;
                                        }
                                    }
                                    break;
                                case "RAS-4HRNM1Q":
                                    //室内机数量限制
                                    if (listRISelected.Count > 5)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("RAS-4HRNM1Q", 5));
                                        continue;
                                    }
                                    else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("Four Way:RAS-4HRNM1Q", 3));
                                        continue;
                                    }
                                    //容量配比限制
                                    if (listRISelected.Count <= 4)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-4HRNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.35)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-4HRNM1Q", "135"));
                                            continue;
                                        }
                                    }
                                    else if (listRISelected.Count <= 5)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-4HRNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.2)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-4HRNM1Q", "120"));
                                            continue;
                                        }
                                    }
                                    //室内机最大匹数限制
                                    //1-for-1 室内机最大匹数4
                                    if (listRISelected.Count == 1)
                                    {
                                        if (max_indhp > 4)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-4HRNM1Q", "4P", "'1-for-1'"));
                                            continue;
                                        }
                                    }
                                    //1-for-N 室内机最大匹数2.5
                                    else if (listRISelected.Count > 1)
                                    {
                                        if (max_indhp > 2.5)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-4HRNM1Q", "2.5P", "'1-for-N'"));
                                            continue;
                                        }
                                    }
                                    break;
                                case "RAS-5HRNM1Q":
                                case "RAS-5HYNM1Q":
                                    //室内机数量限制
                                    if (listRISelected.Count > 5)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("RAS-5HYNM1Q", 5));
                                        continue;
                                    }
                                    else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("Four Way:RAS-5HYNM1Q", 3));
                                        continue;
                                    }
                                    //容量配比限制
                                    if (listRISelected.Count <= 4)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-5HYNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.30)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-5HYNM1Q", "130%"));
                                            continue;
                                        }
                                    }
                                    else if (listRISelected.Count <= 5)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-5HYNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.2)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-5HYNM1Q", "120%"));
                                            continue;
                                        }
                                    }
                                    //室内机最大匹数限制
                                    //1-for-1 室内机最大匹数4
                                    if (listRISelected.Count == 1)
                                    {
                                        if (max_indhp > 4)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-5HYNM1Q", "4P", "'1-for-1'"));
                                            continue;
                                        }
                                    }
                                    //1-for-N 室内机最大匹数3
                                    else if (listRISelected.Count > 1)
                                    {
                                        if (max_indhp > 3)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-5HYNM1Q", "3P", "'1-for-N'"));
                                            continue;
                                        }
                                    }
                                    break;
                            }
                        }
                        #endregion
                    }
                }
                // 检查容量配比率（此处仅校验上限值）                
                outstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
                ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
                curSystemItem.RatioFA = ratioFA;

                if (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
                {
                    //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                    if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        MSGList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                        continue;
                    }
                }

                if (curSystemItem.SysType == SystemType.OnlyIndoor)
                {
                    // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                    if (curSystemItem.Ratio < 0.5)
                    {
                        //OutdoorModelFull = r["ModelFull"].ToString();
                        //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                        //break;
                        //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1(curSystemItem.SelOutdoorType, "50%"));
                        continue;
                    }
                    if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, curSystemItem.MaxRatio * 100 + "%"));
                        continue;
                    }
                    else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                    {
                        // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                        string model_Hitachi = r["Model_Hitachi"].ToString();
                        if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                        {
                            if (curSystemItem.Ratio > 1.2)
                            {
                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, "120%"));
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    // 多新风机或者混连模式，则配比率校验规则有变
                    if (curSystemItem.Ratio < 0.8)
                    {
                        OutdoorModelFull = r["ModelFull"].ToString();
                        ERRList.Add(Msg.OUTD_RATIO_Composite);
                        break;
                    }
                    if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, "100%"));
                        continue;
                    }
                    if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, curSystemItem.MaxRatio * 100 + "%"));
                        continue;
                    }
                    if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_COMPOSITE_MODE);
                        continue; // add on 20160307 clh 
                    }
                }
                // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
                if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
                {
                    ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                    continue;
                }
                /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
                 *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
                 *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                 *3. Total ratio of Hydro Free 0%~100% 
                 *4. Only Hydro Free is not allowed 
                 **/
                if (existsHydroFree)
                {
                    ratioNoHDIU = Math.Round(tot_noHDhp / outstdhp, 3);
                    ratioHD = Math.Round(tot_HDhp / outstdhp, 3);
                    if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                    {
                        MSGList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                        if (ratioNoHDIU < 0.5)
                        {
                            OutdoorModelFull = r["ModelFull"].ToString();
                            break;
                        }
                        else
                            continue;
                    }
                    else if (ratioHD < 0 || ratioHD > 1)
                    {
                        MSGList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                        continue;
                    }
                }

                Outdoor outItem = bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
                ////获取室外机修正系数 add by axj 20170111
                //double Total_IDU_Factor = bll.GetTotalIDUFactor(outItem.ModelFull, tot_indhp, false);
                ////比较室外机额定容量和室内机估算容量之和 20161114 by Yunxiao Lin
                //if (outstdcap_c * Total_IDU_Factor < tot_indcap_c)
                //    continue;
                //if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                //    continue;

                ////比较室外机额定容量与Actual Ratio的乘积和室内机额定容量之和 20170122 by Yunxiao Lin
                //if (curSystemItem.Ratio >= 1)
                //{
                //    if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                //        continue;
                //}
                //else
                //{
                //    if (outstdcap_c < tot_indstdcap_c)
                //        continue;
                //}

                if (curSystemItem.SysType == SystemType.OnlyFreshAirMulti ||  curSystemItem.SysType == SystemType.OnlyFreshAir)
                {
                    inWB = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
                    inDB = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
                }
                
                //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
                //以下调用CalOutdoorEstCapacity的参数actRatio全部改为Horsepower 20161110 by Yunxiao Lin
                if (!outItem.ProductType.Contains("Water Source"))
                curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.DBCooling, inWB, false, curSystemItem);
                else
                    curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWCooling, inWB, false, curSystemItem);

                if (thisProject.IsCoolingModeEffective)
                {
                    //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                    if (curSystemItem.CoolingCapacity == 0)
                    {
                        ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                        return SelectOutdoorResult.NotMatch;
                    }
                    curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
                    if (curSystemItem.PipingLengthFactor == 0)
                    {
                        string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                        double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                        double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                        string msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                        JCMsg.ShowWarningOK(msg);
                        ERRList.Add(msg);
                        return SelectOutdoorResult.Null;
                    }

                    curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
                }
                //  Hitachi的Fresh Air 不需要比较HeatCapacity值
                if (hasHeating)
                {
                    //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
                    if (!outItem.ProductType.Contains("Water Source"))
                        curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.WBHeating, inDB, true, curSystemItem);
                    else
                        curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWHeating, inDB, true, curSystemItem);
                    //水冷机不需要除霜修正
                    if (!outItem.ProductType.Contains("Water Source"))
                    {
                        curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Heating");   //添加制热Factor
                        if (curSystemItem.PipingLengthFactor_H == 0)
                        {
                            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                            double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                            double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                            string msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                            JCMsg.ShowWarningOK(msg);
                            ERRList.Add(msg);
                            return SelectOutdoorResult.Null;
                        }
                        //double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
                        //从数据库获取除霜修正系数 20180626 by Yunxiao Lin
                        double defrostingFactor = bll.GetODUDefrostFactor(curSystemItem.DBHeating, outItem.Series);
                        //curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
                        curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor_H * defrostingFactor;
                    }
                    //判断如果HeatingCapacity 为 弹出提醒 on 20180510 by xyj
                    if (curSystemItem.HeatingCapacity == 0)
                    {
                        ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                        return SelectOutdoorResult.NotMatch;
                    }
                }

                //海拔修正 add on 20160517 by Yunxiao Lin
                //注意某些机型可能无此限制? Wait check
                if (thisProject.EnableAltitudeCorrectionFactor)
                {
                    //从数据库获取海拔修正系数 20180626 by Yunxiao Lin
                    //double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
                    double acf = bll.GetODUAltitudeFactor(thisProject.Altitude, outItem.Series);
                    curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;

                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
                    // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
                    if (hasHeating)
                    {
                        curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
                    }
                }

                // 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
                if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
                {
                    if (curSystemItem.Ratio > 1)
                    {
                        if (!curSystemItem.AllowExceedRatio)
                        {
                            if (tot_indcap_c <= outstdcap_c)
                                MSGList.Add(Msg.MSGLIST_NOT_ALLOW_EXCEED_RATIO);
                            continue;
                        }
                    }
                }

                //计算室内机实际容量 add by Shen Junjie on 20181113
                CalacIndoorActualCapacity(curSystemItem, listRISelected, csMode, hasHeating, tot_indhp, tot_indcap_c, tot_indcap_h);

                //比较室内机和房间的需求是否符合
                bool roomchecked = true;
                foreach (RoomIndoor ri in listRISelected)
                {
                    string wType;
                    if (!CommonBLL.MeetRoomRequired(ri, thisProject, tolerance, thisProject.RoomIndoorList, out wType))
                        roomchecked = false;
                    if (!roomchecked)
                        break;
                }
                if (!roomchecked)
                {
                    if (tot_indcap_c <= outstdcap_c)
                        MSGList.Add(Msg.MSGLIST_CANT_REACH_DEMAND);
                    continue;
                }

                OutdoorModelFull = r["ModelFull"].ToString();
                break; // 找到合适的室外机即跳出循环

            }
            // 遍历自动选型 END
            #endregion

            if (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                #region// 全室内机
                // 2-2.内机总冷量配置率为50%～130%；
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    //OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
                    //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    //returnType = SelectOutdoorResult.NotMatch;
                    //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
                }
                #endregion
            }
            else
            {
                #region// 多新风机或者混连模式，则配比率校验规则有变

                // 1-2.新风机冷量与室外机的配置率不大于30%
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
                {
                    if (string.IsNullOrEmpty(OutdoorModelFull))
                        OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();

                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    returnType = SelectOutdoorResult.NotMatch;
                }

                // 1-3.内机总冷量配置率为80%～105%；
                if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
                {
                    OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    returnType = SelectOutdoorResult.NotMatch;
                }
                #endregion
            }
            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
            *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
            *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
            *3. Total ratio of Hydro Free 0%~100% 
            *4. Only Hydro Free is not allowed 
            **/
            if (existsHydroFree)
            {
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3)  //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过系统的30%    -- add by Shen Junjie on 2017/12/22
            //  if (tot_HP_KPI_X4E / tot_indhp > 0.3)
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/29
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                returnType = SelectOutdoorResult.NotMatch;
            }

            if (!string.IsNullOrEmpty(OutdoorModelFull))
            {
                curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
                // updated by clh
                if (curSystemItem.OutdoorItem != null)
                {
                    pipBll.SetPipingLimitation(curSystemItem);
                    //如果按室外机选型得不到正确结果，使用按室内机选型再试一遍 20161130 by Yunxiao Lin
                    if (returnType != SelectOutdoorResult.OK)
                        return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
                    else
                        return returnType;
                }

            }
            if ((thisProject != null) && (thisProject.ProductType.Contains("Water Source")))
                ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
            else
                ERRList.Add(Msg.OUTD_NOTMATCH);
            //如果按室外机选型得不到正确结果，按室内机选型再试一遍 20161130 by Yunxiao Lin
            if (returnType != SelectOutdoorResult.OK)
                return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            return SelectOutdoorResult.Null;
        }

        #region 室内机优先增大，不锁定自动选型的室内机
        /// 自动选择匹配的室外机-室内机优先（包括非严格一对一的情况）20161110 add by Yunxiao Lin
        /// 水机选型增加水流速参数 20170216 by Yunxiao Lin
        /// <summary>
        /// 自动选择匹配的室外机-室内机优先（包括非严格一对一的情况）
        /// </summary>
        public static SelectOutdoorResult DoSelectOutdoorIDUFirst(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, out List<string> ERRList)
        {
            DateTime t1 = DateTime.Now;
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);

            ERRList = new List<string>();
            ERRList.Clear();    // 每次选型前清空错误记录表
            if (curSystemItem.OutdoorItem != null)
            {
                //记住series以便自动重新选型时传入Series  add by Shen Junjie on 2018/3/29
                curSystemItem.Series = curSystemItem.OutdoorItem.Series;
            }
        //    curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL(thisProject);

            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }

            CapacitySelectionMode csMode = GetCapacitySelectionMode(thisProject);

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            //string productType = listRISelected[0].IndoorItem.ProductType;
            //bool hasHeating = thisProject.IsHeatingModeEffective && !productType.Contains(", CO");
            string series = listRISelected[0].IndoorItem.Series;
            if (string.IsNullOrEmpty(series))
            {
                series = curSystemItem.Series;
            }
            bool isCoolingOnly = series.Contains(" CO,");
            bool hasHeating = thisProject.IsHeatingModeEffective && !isCoolingOnly;

            DataTable dtOutdoorStd = new DataTable();
            // 获取室外机标准表（初次加载或者更改室外机类型时）
            if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
                if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
                {
                    // ERROR:室外机标准表无数据记录！
                    ERRList.Add(Msg.DB_NODATA);
                    return SelectOutdoorResult.Null;
                }
            }
            //add by axj 20170120 室内机工况温度
            double inWB = 0; //制冷湿球温度
            double inDB = 0; //制热干球温度

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double ratioFA = 0;
            double tot_indhpOnly = 0;
            double tot_indhp = 0;
            //判断室内机中是否含有4 way 机型 20170702 by Yunxiao Lin
            bool exists4way = false;
            //记录最大的室内机匹数 20170702 by Yunxiao Lin
            double max_indhp = 0;
            //判断室内机中是否含有Hydro Free 20171220 by Yunxiao Lin
            bool existsHydroFree = false;
            //计算Hydro Free总容量和非Hydro Free室内机的总容量 20171220 by Yunxiao Lin
            double tot_HDhp = 0;
            double tot_noHDhp = 0;
            double ratioNoHDIU = 0;
            double ratioHD = 0;
            double tot_HP_KPI_X4E = 0; // KPI-X4E (全热交换机)的总匹数
            double outstdhp = 0; // ODU的总匹数
            foreach (RoomIndoor ri in listRISelected)
            {
                //增加4 way 室内机判断，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Type.Contains("Four Way"))
                    exists4way = true;

                //记录最大的室内机匹数，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Horsepower > max_indhp)
                    max_indhp = ri.IndoorItem.Horsepower;

                //增加Hydro Free室内机判断,并分别统计系统内Hydro Free和非Hydro Free室内机的容量 20171220 by Yunxiao Lin
                if (ri.IndoorItem.Type.Contains("Hydro Free"))
                {
                    existsHydroFree = true;
                    tot_HDhp += ri.IndoorItem.Horsepower;
                }
                else
                {
                    tot_noHDhp += ri.IndoorItem.Horsepower;
                }

                if (ri.IndoorItem.Type == "Total Heat Exchanger (KPI-X4E)")
                {
                    tot_HP_KPI_X4E += ri.IndoorItem.Horsepower;
                }

                tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                tot_indcap_h += ri.HeatingCapacity;
                tot_indhp += ri.IndoorItem.Horsepower;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                    {
                        // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                        Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                        if (inItem != null)
                        {
                            inItem.Series = series;
                            ri.SetIndoorItemWithAccessory(inItem);
                        }

                    }
                    tot_FAhp += ri.IndoorItem.Horsepower;

                }
            }
            tot_indhpOnly = tot_indhp - tot_FAhp;

            //获取室内机工况温度 add by Shen Junjie on 20181113
            GetIDUWorkTemperature(csMode, listRISelected, out inWB, out inDB);

            bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
            #region //确定当前内机组合模式
            // 1.混连模式
            if (isComposite)
            {
                // 1-1.必须同时包含室内机与新风机；
                if (tot_indhpOnly == 0 || tot_FAhp == 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
                    return SelectOutdoorResult.Null;
                }
            }
            else
            {
                // 2-1.不允许同时包含室内机和新风机；
                if (tot_indhpOnly > 0 && tot_FAhp > 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
                    return SelectOutdoorResult.Null;
                }

                // 2.全室内机模式
                if (tot_indhpOnly > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyIndoor;
                }
                // 3.全新风机模式
                else if (tot_FAhp > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

                    if (listRISelected.Count == 1)
                    {
                        Indoor inItem = listRISelected[0].IndoorItem;

                        #region 一对一新风机
                        // 一对一新风机不适用于 SMZ Gen.2  on 20180918 
                        bool isSMZ = false;
                        //Add FSNC7B/5B by Yunxiao Lin 20190107
                        if (!string.IsNullOrEmpty(series) && (series.Contains("FSNP") || series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC7B") || series.Contains("FSNC5B")))
                        {
                            isSMZ = true;
                        }
                        if (!isSMZ)
                        {
                            curSystemItem.SysType = SystemType.OnlyFreshAir;

                            // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                            // 20160821 新增productType参数 by Yunxiao Lin
                            inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(inItem.ModelFull, inItem.Type, false, inItem.ProductType, inItem.Series);
                            if (inItem != null)
                            {
                                string UniqueOutdoorName = inItem.UniqueOutdoorName;

                                //按照设定的model取一对一室外机需要判断品牌。 20161024 by Yunxiao Lin
                                //同一个新风机可能被多个系列共用，因此可能存在多个一对一室外机，用“/”分隔 20170330 by Yunxiao Lin
                                if (!UniqueOutdoorName.Contains("/"))
                                {
                                    if (thisProject.BrandCode == "Y")
                                        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, series);
                                    else
                                        curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, series);
                                }
                                else
                                {
                                    string[] UniqueOutdoorNameList = UniqueOutdoorName.Split(new char[] { '/' });
                                    foreach (string model in UniqueOutdoorNameList)
                                    {
                                        if (thisProject.BrandCode == "Y")
                                            curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, series);
                                        else
                                            curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(model, series);
                                        if (curSystemItem.OutdoorItem != null)
                                            break;
                                    }
                                }
                                if (curSystemItem.OutdoorItem == null)
                                {
                                    // ERROR:数据库中的一对一室外机ModelName写错
                                    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
                                    return SelectOutdoorResult.Null;
                                }
                                else
                                {
                                    curSystemItem.MaxRatio = 1;
                                    //Connection Ratio改为由HorsePower对比计算
                                    curSystemItem.Ratio = inItem.Horsepower / curSystemItem.OutdoorItem.Horsepower;
                                    // FreshAir时不需要估算容量,直接绑定室外机的标准值
                                    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                    //一对一新风机将室外机的Capacity直接绑定到RoomIndoor 20161114 by Yunxiao Lin
                                    listRISelected[0].ActualCoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                    listRISelected[0].ActualHeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                    listRISelected[0].ActualSensibleHeat = 0d;

                                    pipBll.SetPipingLimitation(curSystemItem);
                                    // 一对一新风机选型成功！
                                    return SelectOutdoorResult.OK;
                                }
                            }
                        }
                        #endregion

                    }
                }// 全新风机 END
            }// 模式确定 END
            #endregion

            SelectOutdoorResult returnType = SelectOutdoorResult.OK;

            #region // 遍历室外机标准表逐个筛选

            // update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
            //double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
            //室外机选型改为判断Series 20161031 by Yunxiao Lin
            DataRow[] rows = dtOutdoorStd.Select("  SubString(ModelFull,11,1)='" + curSystemItem.Power + "' and UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''", "Model asc");

            List<RoomIndoor> listRISelected_temp = null;
            bool isSelected = false;
            // 遍历选型过程 START 
            foreach (DataRow r in rows)
            {
                //每次选新的室外机都拷贝一份室内机列表
                listRISelected_temp = listRISelected.DeepClone();
                bool IDUincrease = true; //是否继续增大室内机，如果为否，则进入下一个室外机循环
                bool outbreak = false; //是否跳出室外机循环
                bool isFirst = true;
                while (IDUincrease) //如果只增大室内机不会超限，则继续增大。
                {
                    if (!isFirst)
                    {
                        //由于上一个室内机循环型号可能已经被改变，所以室内机的各项总指标需要重新计算。
                        tot_indcap_c = 0;
                        tot_indcap_h = 0;
                        tot_FAhp = 0;
                        ratioFA = 0;
                        tot_indhpOnly = 0;
                        tot_indhp = 0;
                        foreach (RoomIndoor ri in listRISelected_temp)
                        {
                            tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                            tot_indcap_h += ri.HeatingCapacity;
                            tot_indhp += ri.IndoorItem.Horsepower;
                            if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                            {
                                if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                                {
                                    // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                                    Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                                    if (inItem != null)
                                    {
                                        inItem.Series = series;
                                        ri.SetIndoorItemWithAccessory(inItem);
                                    }

                                }
                                tot_FAhp += ri.IndoorItem.Horsepower;

                            }
                        }
                        tot_indhpOnly = tot_indhp - tot_FAhp;
                    }
                    isFirst = false;

                    // 检查最大连接数 
                    int maxIU = 0;
                    int.TryParse(r["MaxIU"].ToString(), out maxIU);
                    if (maxIU < listRISelected.Count)
                    {
                        IDUincrease = false;
                        continue;
                    }

                    #region//检查IVX特殊连接 20170702 by Yunxiao Lin
                    if (series.Contains("IVX"))
                    {
                        string ODUmodel = r["Model_Hitachi"].ToString().Trim();
                        if (series.Contains("HVRNM2"))
                        {
                            //SMZ IVX 校验组合是否有效
                            string IDUmodel = "";
                            int IDUmodelsCount = 0;
                            bool combinationErr = false;
                            foreach (RoomIndoor ri in listRISelected)
                            {
                                //获取IDU 型号名称和数量
                                if (IDUmodel == "")
                                {
                                    IDUmodel = ri.IndoorItem.Model_Hitachi;
                                    IDUmodelsCount++;
                                }
                                else if (IDUmodel != ri.IndoorItem.Model_Hitachi)
                                {
                                    combinationErr = true;
                                    break;
                                }
                                else
                                    IDUmodelsCount++;
                            }
                            if (!combinationErr && IDUmodelsCount > 0)
                            {
                                //将IDU型号名称和数量组合成 "model x count" 字符串，与数据库中预设的组合进行比对 20170702 by Yunxiao Lin
                                string IDUmodels = IDUmodel + " x " + IDUmodelsCount.ToString();
                                IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                                if (!IVXCombll.existsCombination(ODUmodel, IDUmodels))
                                    continue;
                            }
                            else
                                continue;
                        }
                        else if (series.Contains("H(R/Y)NM1Q"))
                        {
                            //HAPQ IVX
                            double IVXoutstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                            curSystemItem.Ratio = Math.Round(tot_indhp / IVXoutstdhp, 3);
                            //根据室外温度和室外机型号进行检查 20170702 by Yunxiao Lin
                            if (curSystemItem.DBCooling <= -10)
                            {
                                #region
                                switch (ODUmodel)
                                {
                                    case "RAS-3HRNM1Q":
                                        //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                        if (exists4way)
                                            continue;
                                        //容量配比限制
                                        if (curSystemItem.Ratio < 0.85)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        break;
                                    case "RAS-4HRNM1Q":
                                    case "RAS-5HRNM1Q":
                                    case "RAS-5HYNM1Q":
                                        //室内机数量限制
                                        if (listRISelected.Count > 4)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        //容量配比限制
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.12)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                #region
                                switch (ODUmodel)
                                {
                                    case "RAS-3HRNM1Q":
                                        //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                        if (exists4way)
                                            continue;
                                        //容量配比限制
                                        if (curSystemItem.Ratio < 0.85)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.2)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        //室内机最大匹数限制
                                        //1-for-1 室内机最大匹数3
                                        if (listRISelected.Count == 1)
                                        {
                                            if (max_indhp > 3)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        //1-for-N 室内机最大匹数2.3
                                        else if (listRISelected.Count > 1)
                                        {
                                            if (max_indhp > 2.3)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        break;
                                    case "RAS-4HRNM1Q":
                                        //室内机数量限制
                                        if (listRISelected.Count > 5)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        //容量配比限制
                                        if (listRISelected.Count <= 4)
                                        {
                                            if (curSystemItem.Ratio < 0.9)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                            else if (curSystemItem.Ratio > 1.35)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        else if (listRISelected.Count <= 5)
                                        {
                                            if (curSystemItem.Ratio < 0.9)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                            else if (curSystemItem.Ratio > 1.2)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        //室内机最大匹数限制
                                        //1-for-1 室内机最大匹数4
                                        if (listRISelected.Count == 1)
                                        {
                                            if (max_indhp > 4)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        //1-for-N 室内机最大匹数2.5
                                        else if (listRISelected.Count > 1)
                                        {
                                            if (max_indhp > 2.5)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        break;
                                    case "RAS-5HRNM1Q":
                                    case "RAS-5HYNM1Q":
                                        //室内机数量限制
                                        if (listRISelected.Count > 5)
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                        {
                                            IDUincrease = false;
                                            continue;
                                        }
                                        //容量配比限制
                                        if (listRISelected.Count <= 4)
                                        {
                                            if (curSystemItem.Ratio < 0.9)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                            else if (curSystemItem.Ratio > 1.30)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        else if (listRISelected.Count <= 5)
                                        {
                                            if (curSystemItem.Ratio < 0.9)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                            else if (curSystemItem.Ratio > 1.2)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        //室内机最大匹数限制
                                        //1-for-1 室内机最大匹数4
                                        if (listRISelected.Count == 1)
                                        {
                                            if (max_indhp > 4)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        //1-for-N 室内机最大匹数3
                                        else if (listRISelected.Count > 1)
                                        {
                                            if (max_indhp > 3)
                                            {
                                                IDUincrease = false;
                                                continue;
                                            }
                                        }
                                        break;
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                    // 检查容量配比率（此处仅校验上限值）
                    double outstdcap_c = Convert.ToDouble(r["CoolCapacity"].ToString());
                    outstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                    curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
                    ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
                    curSystemItem.RatioFA = ratioFA;

                    if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
                    {
                        //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                        if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                        {
                            IDUincrease = false;
                            continue;
                        }
                    }

                    if (curSystemItem.SysType == SystemType.OnlyIndoor)
                    {
                        // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                        if (curSystemItem.Ratio < 0.5)
                        {
                            //OutdoorModelFull = r["ModelFull"].ToString();
                            //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                            //{
                            //    IDUincrease = false;
                            //    outbreak = true;
                            //    continue;
                            //}
                            //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
                            IDUincrease = false;
                            continue;
                        }
                        if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                        {
                            IDUincrease = false;
                            continue;
                        }
                        else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                        {
                            #region// S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                            string model_Hitachi = r["Model_Hitachi"].ToString();
                            if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                            {
                                if (curSystemItem.Ratio > 1.2)
                                {
                                    IDUincrease = false;
                                    continue;
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        #region// 多新风机或者混连模式，则配比率校验规则有变
                        if (curSystemItem.Ratio < 0.8)
                        {
                            OutdoorModelFull = r["ModelFull"].ToString();
                            ERRList.Add(Msg.OUTD_RATIO_Composite);
                            IDUincrease = false;
                            outbreak = true;
                            continue;
                        }
                        if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
                        {
                            IDUincrease = false;
                            continue;
                        }
                        if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
                        {
                            IDUincrease = false;
                            continue; // add on 20160307 clh
                        }
                        #endregion
                    }
                    // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
                    if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
                    {
                        ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                        IDUincrease = false;
                        continue;
                    }

                    /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
                     *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
                     *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                     *3. Total ratio of Hydro Free 0%~100% 
                     *4. Only Hydro Free is not allowed 
                     **/
                    if (existsHydroFree)
                    {
                        ratioNoHDIU = Math.Round(tot_noHDhp / outstdhp, 3);
                        ratioHD = Math.Round(tot_HDhp / outstdhp, 3);
                        if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                        {
                            if (ratioNoHDIU < 0.5)
                            {
                                OutdoorModelFull = r["ModelFull"].ToString();
                                ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                                outbreak = true;
                            }
                            IDUincrease = false;
                            continue;
                        }
                        else if (ratioHD < 0 || ratioHD > 1)
                        {
                            IDUincrease = false;
                            continue;
                        }
                    }

                    Outdoor outItem = bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
                    ////获取室外机修正系数 add by axj 20170111
                    //double Total_IDU_Factor = bll.GetTotalIDUFactor(outItem.ModelFull, tot_indhp, false);
                    ////比较室外机额定容量和室内机估算容量之和 20161114 by Yunxiao Lin
                    //if (outstdcap_c * Total_IDU_Factor < tot_indcap_c)
                    //{
                    //    IDUincrease = false;
                    //    continue;
                    //}

                    ////比较室外机额定容量与Actual Ratio的乘积和室内机额定容量之和 20170122 by Yunxiao Lin
                    //if (curSystemItem.Ratio >= 1)
                    //{
                    //    if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                    //    {
                    //        IDUincrease = false;
                    //        continue;
                    //    }
                    //}
                    //else
                    //{
                    //    if (outstdcap_c < tot_indstdcap_c)
                    //    {
                    //        IDUincrease = false;
                    //        continue;
                    //    }
                    //}

                    //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
                    //以下调用CalOutdoorEstCapacity的参数actRatio全部改为Horsepower 20161110 by Yunxiao Lin
                    if (!outItem.ProductType.Contains("Water Source"))
                        curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.DBCooling, inWB, false, curSystemItem);
                    else
                        curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWCooling, inWB, false, curSystemItem);

                    if (thisProject.IsCoolingModeEffective)
                    {

                        //判断如果HeatingCapacity 为 弹出提醒 on 20180510 by xyj
                        if (curSystemItem.CoolingCapacity == 0)
                        {
                            ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                            return SelectOutdoorResult.NotMatch;
                        }
                        curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
                        if (curSystemItem.PipingLengthFactor == 0)
                        {
                            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                            double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                            double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                            JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                            return SelectOutdoorResult.Null;
                        }

                        curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
                    }
                    //  Hitachi的Fresh Air 不需要比较HeatCapacity值
                    if (hasHeating)
                    {
                        //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
                        if (!outItem.ProductType.Contains("Water Source"))
                            curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.WBHeating, inDB, true, curSystemItem);
                        else
                            curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWHeating, inDB, true, curSystemItem);
                        //水冷机不需要除霜修正
                        if (!outItem.ProductType.Contains("Water Source"))
                        {
                            //double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
                            //从数据库获取除霜修正系数 20180626 by Yunxiao Lin
                            double defrostingFactor = bll.GetODUDefrostFactor(curSystemItem.DBHeating, outItem.Series);
                            curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Heating");   //添加制热Factor
                            if (curSystemItem.PipingLengthFactor_H == 0)
                            {
                                string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                                double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                                double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                                JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                                return SelectOutdoorResult.Null;
                            }
                            //curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
                            curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor_H * defrostingFactor;
                        }
                        //判断如果HeatingCapacity 为 弹出提醒 on 20180510 by xyj
                        if (curSystemItem.HeatingCapacity == 0)
                        {
                            ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                            return SelectOutdoorResult.NotMatch;
                        }
                    }

                    //海拔修正 add on 20160517 by Yunxiao Lin
                    //注意某些机型可能无此限制? Wait check
                    if (thisProject.EnableAltitudeCorrectionFactor)
                    {
                        //从数据库获取海拔修正系数 20180626 by Yunxiao Lin
                        //double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
                        double acf = bll.GetODUAltitudeFactor(thisProject.Altitude, outItem.Series);
                        curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;

                        // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
                        // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
                        if (hasHeating)
                        {
                            curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
                        }
                    }

                    // 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
                    if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
                    {
                        if (curSystemItem.Ratio > 1)
                        {
                            if (!curSystemItem.AllowExceedRatio)
                            {
                                IDUincrease = false;
                                continue;
                            }
                        }
                    }

                    //计算室内机实际容量 add by Shen Junjie on 20181113
                    CalacIndoorActualCapacity(curSystemItem, listRISelected_temp, csMode, hasHeating, tot_indhp, tot_indcap_c, tot_indcap_h);

                    //比较室内机和房间的需求是否符合
                    bool tot_roomchecked = true;
                    IDUincrease = false;
                    foreach (RoomIndoor ri in listRISelected_temp)
                    {
                        bool roomchecked = true;
                        string type;
                        if (!CommonBLL.MeetRoomRequired(ri, thisProject, 0, listRISelected_temp, out type))
                            roomchecked = false;
                        if (!roomchecked)
                        {
                            tot_roomchecked = false;
                            if (ri.IsAuto)
                            {
                                Indoor item = ri.IndoorItem.DeepClone();
                                IndoorBLL indbll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
                                item = indbll.getBiggerIndoor(item);
                                if (item != null)
                                {
                                    IDUincrease = true;
                                    ri.SetIndoorItem(item);
                                    //需要重算est. value
                                    ri.CoolingCapacity = indbll.CalIndoorEstCapacity(ri.IndoorItem, curSystemItem.DBCooling, ri.WBCooling, false);
                                    //if (!ri.IndoorItem.ProductType.Contains(", CO"))
                                    if (!isCoolingOnly)
                                    {
                                        //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
                                        ri.HeatingCapacity = indbll.CalIndoorEstCapacity(ri.IndoorItem, curSystemItem.WBHeating, ri.DBHeating, true);
                                        //显热改为由估算容量*SHF计算得到 20161111
                                        //double shf = 0;
                                        //if (ri.SHF_Mode.Equals("High"))
                                        //    shf = ri.IndoorItem.SHF_Hi;
                                        //else if (ri.SHF_Mode.Equals("Medium"))
                                        //    shf = ri.IndoorItem.SHF_Med;
                                        //else if (ri.SHF_Mode.Equals("Low"))
                                        //    shf = ri.IndoorItem.SHF_Lo;

                                        //if (shf == 0d)
                                        //    shf = ri.IndoorItem.SHF_Hi;

                                        ri.SensibleHeat = ri.CoolingCapacity * ri.SHF;
                                    }
                                }
                                else
                                {
                                    //如果已经无法继续增大室内机，则选型失败。
                                    IDUincrease = false;
                                    isSelected = false;
                                    outbreak = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!tot_roomchecked)
                    {
                        continue;
                    }
                    else
                    {
                        IDUincrease = false;
                        isSelected = true;
                        break;
                    }
                }
                if (!IDUincrease && !isSelected)
                {
                    if (outbreak)
                        break;
                    else
                        continue;
                }

                OutdoorModelFull = r["ModelFull"].ToString();
                break; // 找到合适的室外机即跳出循环

            }
            // 遍历自动选型 END
            #endregion

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机
                // 2-2.内机总冷量配置率为50%～130%；
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    //OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
                    //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    //returnType = SelectOutdoorResult.NotMatch;
                    //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变

                // 1-2.新风机冷量与室外机的配置率不大于30%
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
                {
                    if (string.IsNullOrEmpty(OutdoorModelFull))
                        OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();

                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    returnType = SelectOutdoorResult.NotMatch;
                }

                // 1-3.内机总冷量配置率为80%～105%；
                if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
                {
                    OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
            *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
            *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
            *3. Total ratio of Hydro Free 0%~100% 
            *4. Only Hydro Free is not allowed 
            **/
            if (existsHydroFree)
            {
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过系统的30%    -- add by Shen Junjie on 2017/12/22
            //if (tot_HP_KPI_X4E / tot_indhp > 0.3)
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/29
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                returnType = SelectOutdoorResult.NotMatch;
            }

            if (!string.IsNullOrEmpty(OutdoorModelFull))
            {
                curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
                if (curSystemItem.OutdoorItem != null)
                {
                    pipBll.SetPipingLimitation(curSystemItem);
                    //将已改变的RoomIndoorList复制回来,注意不能简单的Clone
                    foreach (RoomIndoor rt in listRISelected_temp)
                    {
                        foreach (RoomIndoor ri in listRISelected)
                        {
                            if (rt.IsAuto && ri.IsAuto && rt.RoomID == ri.RoomID && rt.IsFreshAirArea == ri.IsFreshAirArea)
                            {
                                ri.CoolingCapacity = rt.CoolingCapacity;
                                ri.HeatingCapacity = rt.HeatingCapacity;
                                ri.SensibleHeat = rt.SensibleHeat;
                                ri.ActualCoolingCapacity = rt.ActualCoolingCapacity;
                                ri.ActualHeatingCapacity = rt.ActualHeatingCapacity;
                                ri.ActualSensibleHeat = rt.ActualSensibleHeat;
                                ri.SetIndoorItemWithAccessory(rt.IndoorItem);
                            }
                        }
                    }
                    DateTime t2 = DateTime.Now;
                    TimeSpan ts = t2 - t1;
                    double MilliSec = ts.TotalMilliseconds;
                    //MessageBox.Show("Cost Time：" + MilliSec.ToString() + "MilliSec");

                    return returnType;
                }

            }
            if ((thisProject != null) && (thisProject.ProductType.Contains("Water Source")))
                ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
            else
                ERRList.Add(Msg.OUTD_NOTMATCH);
            return SelectOutdoorResult.Null;
        }
        #endregion



        /// 手动选择匹配的室外机（包括非严格一对一的情况）20161223 add by axj
        /// 水机选型增加FlowRateLevel参数 20170216 by Yunxiao Lin
        /// <summary>
        /// 手动选择匹配的室外机（包括非严格一对一的情况）
        /// </summary>
        public static SelectOutdoorResult DoSelectOutdoorManual(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, out List<string> ERRList)
        {
            if (ProjectBLL.IsSupportedUniversalSelection(thisProject))
                return DoSelectUniversalOduManual(curSystemItem, listRISelected, thisProject, curSystemItem.Series, out ERRList);

            ERRList = new List<string>();
            ERRList.Clear();    // 每次选型前清空错误记录表
            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }

            if (curSystemItem.OutdoorItem == null)
                return SelectOutdoorResult.Null;

            CapacitySelectionMode csMode = GetCapacitySelectionMode(thisProject);

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            //string productType = listRISelected[0].IndoorItem.ProductType;
            //bool hasHeating = thisProject.IsHeatingModeEffective && !productType.Contains(", CO");
            string series = listRISelected[0].IndoorItem.Series;
            bool isCoolingOnly = series.Contains(" CO,");
            bool hasHeating = thisProject.IsHeatingModeEffective && !isCoolingOnly;

            if (curSystemItem != null && curSystemItem.IDUFirst && !series.Contains("IVX"))
                return DoSelectOutdoorIDUFirstManual(curSystemItem, listRISelected, thisProject, out ERRList);
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            double tolerance = 0.05;

            //curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL(thisProject);

            DataTable dtOutdoorStd = new DataTable();
            // 获取室外机标准表（初次加载或者更改室外机类型时）
            if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
                if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
                {
                    // ERROR:室外机标准表无数据记录！
                    ERRList.Add(Msg.DB_NODATA);
                    return SelectOutdoorResult.Null;
                }
            }
            //add by axj 20170120 室内机工况温度
            double inWB = 0; //制冷湿球温度
            double inDB = 0; //制热干球温度

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double ratioFA = 0;
            double tot_indhpOnly = 0;
            double tot_indhp = 0;
            //判断室内机中是否含有4 way 机型 20170702 by Yunxiao Lin
            bool exists4way = false;
            //记录最大的室内机匹数 20170702 by Yunxiao Lin
            double max_indhp = 0;
            //判断室内机中是否含有Hydro Free 20171220 by Yunxiao Lin
            bool existsHydroFree = false;
            //计算Hydro Free总容量和非Hydro Free室内机的总容量 20171220 by Yunxiao Lin
            double tot_HDhp = 0;
            double tot_noHDhp = 0;
            double ratioNoHDIU = 0;
            double ratioHD = 0;
            double tot_HP_KPI_X4E = 0; // KPI-X4E (全热交换机)的总匹数
            double outstdhp = 0;
            foreach (RoomIndoor ri in listRISelected)
            {
                //增加4 way 室内机判断，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Type.Contains("Four Way"))
                    exists4way = true;

                //记录最大的室内机匹数，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Horsepower > max_indhp)
                    max_indhp = ri.IndoorItem.Horsepower;

                //增加Hydro Free室内机判断,并分别统计系统内Hydro Free和非Hydro Free室内机的容量 20171220 by Yunxiao Lin
                if (ri.IndoorItem.Type.Contains("Hydro Free"))
                {
                    existsHydroFree = true;
                    tot_HDhp += ri.IndoorItem.Horsepower;
                }
                else
                {
                    tot_noHDhp += ri.IndoorItem.Horsepower;
                }

                if (ri.IndoorItem.Type == "Total Heat Exchanger (KPI-X4E)")
                {
                    tot_HP_KPI_X4E += ri.IndoorItem.Horsepower;
                }

                tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                tot_indcap_h += ri.HeatingCapacity;
                tot_indhp += ri.IndoorItem.Horsepower;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                    {
                        // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                        Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                        if (inItem != null)
                        {
                            inItem.Series = series;
                            ri.SetIndoorItemWithAccessory(inItem);
                        }

                    }
                    tot_FAhp += ri.IndoorItem.Horsepower;

                }
            }
            tot_indhpOnly = tot_indhp - tot_FAhp;

            //获取室内机工况温度 add by Shen Junjie on 20181113
            GetIDUWorkTemperature(csMode, listRISelected, out inWB, out inDB);

            bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
            #region //确定当前内机组合模式
            // 1.混连模式
            if (isComposite)
            {
                // 1-1.必须同时包含室内机与新风机；
                if (tot_indhpOnly == 0 || tot_FAhp == 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
                    return SelectOutdoorResult.Null;
                }
            }
            else
            {
                // 2-1.不允许同时包含室内机和新风机；
                if (tot_indhpOnly > 0 && tot_FAhp > 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
                    return SelectOutdoorResult.Null;
                }

                // 2.全室内机模式
                if (tot_indhpOnly > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyIndoor;
                }
                // 3.全新风机模式
                else if (tot_FAhp > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

                    if (listRISelected.Count == 1)
                    {
                        Indoor inItem = listRISelected[0].IndoorItem;

                        #region 一对一新风机
                        // 一对一新风机不适用于 SMZ Gen.2  on 20180918 
                        bool isSMZ = false;
                        //Add FSNC7B/5B by Yunxiao Lin 20190107
                        if (!string.IsNullOrEmpty(series) && (series.Contains("FSNP") || series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC7B") || series.Contains("FSNC5B")))
                        {
                            isSMZ = true;
                        }
                        if (!isSMZ)
                        {
                            curSystemItem.SysType = SystemType.OnlyFreshAir;

                            // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                            // 20160821 新增productType参数 by Yunxiao Lin
                            inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(inItem.ModelFull, inItem.Type, false, inItem.ProductType, inItem.Series);
                            if (inItem != null)
                            {
                                curSystemItem.SysType = SystemType.OnlyFreshAir;

                                // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                                // 20160821 新增productType参数 by Yunxiao Lin
                                inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(inItem.ModelFull, inItem.Type, false, inItem.ProductType, inItem.Series);
                                if (inItem != null)
                                {
                                    string UniqueOutdoorName = inItem.UniqueOutdoorName;
                                    // if (curSystemItem.OutdoorItem.ModelFull == UniqueOutdoorName)  //按品牌选择Model Name on 2018/8/29
                                    if ((thisProject.BrandCode == "Y" ? curSystemItem.OutdoorItem.Model_York : curSystemItem.OutdoorItem.Model_Hitachi).TrimEnd() == UniqueOutdoorName)
                                    {
                                        curSystemItem.MaxRatio = 1;
                                        //Connection Ratio改为由HorsePower对比计算
                                        curSystemItem.Ratio = inItem.Horsepower / curSystemItem.OutdoorItem.Horsepower;
                                        // FreshAir时不需要估算容量,直接绑定室外机的标准值
                                        curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                        curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                        //一对一新风机将室外机的Capacity直接绑定到RoomIndoor 20161114 by Yunxiao Lin
                                        listRISelected[0].ActualCoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                        listRISelected[0].ActualHeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                        listRISelected[0].ActualSensibleHeat = 0d;
                                        pipBll.SetPipingLimitation(curSystemItem);
                                        // 一对一新风机选型成功！
                                        return SelectOutdoorResult.OK;
                                    }
                                    else
                                    {
                                        // ERROR:数据库中的一对一室外机ModelName写错
                                        ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
                                        return SelectOutdoorResult.Null;
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }// 全新风机 END
            }// 模式确定 END
            #endregion

            SelectOutdoorResult returnType = SelectOutdoorResult.OK;

            #region // 遍历室外机标准表逐个筛选

            // update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
            //double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
            //室外机选型改为判断Series 20161031 by Yunxiao Lin
            //DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''", "Model asc");
            // 遍历选型过程 START 
            //foreach (DataRow r in rows)
            //{
            // 检查最大连接数 
            //int maxIU = 0;
            //int.TryParse(r["MaxIU"].ToString(), out maxIU);
            if (curSystemItem.OutdoorItem.MaxIU < listRISelected.Count)
            {
                ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), curSystemItem.OutdoorItem.MaxIU.ToString()));//室内机数量超过室外机的最大连接数！
                return SelectOutdoorResult.NotMatch;
            }
            //continue;
            //检查IVX特殊连接 20170702 by Yunxiao Lin
            if (series.Contains("IVX"))
            {
                string ODUmodel = curSystemItem.OutdoorItem.Model_Hitachi;
                if (series.Contains("HVRNM2"))
                {
                    #region//SMZ IVX 校验组合是否有效
                    string IDUmodel = "";
                    int IDUmodelsCount = 0;
                    bool combinationErr = false;
                    foreach (RoomIndoor ri in listRISelected)
                    {
                        //获取IDU 型号名称和数量
                        if (IDUmodel == "")
                        {
                            IDUmodel = ri.IndoorItem.Model_Hitachi;
                            IDUmodelsCount++;
                        }
                        else if (IDUmodel != ri.IndoorItem.Model_Hitachi)
                        {
                            combinationErr = true;
                            break;
                        }
                        else
                            IDUmodelsCount++;
                    }
                    if (!combinationErr && IDUmodelsCount > 0)
                    {
                        //将IDU型号名称和数量组合成 "model x count" 字符串，与数据库中预设的组合进行比对 20170702 by Yunxiao Lin
                        string IDUmodels = IDUmodel + " x " + IDUmodelsCount.ToString();
                        IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                        if (!IVXCombll.existsCombination(ODUmodel, IDUmodels))
                        {
                            //ShowMessage: [ODUmodel]只能与以下Indoor搭配使用(IDU组合列表。。。建议用弹出窗口做)
                            string msg = Msg.OUTD_ODUMatchIDUList(ODUmodel) + "\r\n";
                            var list = IVXCombll.getCombination(ODUmodel).IDUModels;
                            foreach (string m in list)
                            {
                                msg = msg + m + "\r\n";
                            }
                            frmMsgInfo fm = new frmMsgInfo();
                            fm.ShowMsg(msg);
                            fm.StartPosition = FormStartPosition.CenterScreen;
                            fm.Show();
                            ERRList.Add(Msg.OUTD_NOTMATCH);
                            return SelectOutdoorResult.NotMatch;
                        }
                    }
                    else
                    {
                        //ShowMessage: [ODUmodel]只能与以下Indoor搭配使用(IDU组合列表。。。建议用弹出窗口做)
                        IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                        string msg = Msg.OUTD_ODUMatchIDUList(ODUmodel) + "\r\n";
                        var list = IVXCombll.getCombination(ODUmodel).IDUModels;
                        foreach (string m in list)
                        {
                            msg = msg + m + "\r\n";
                        }
                        frmMsgInfo fm = new frmMsgInfo();
                        fm.ShowMsg(msg);
                        fm.StartPosition = FormStartPosition.CenterScreen;
                        fm.Show();
                        ERRList.Add(Msg.OUTD_NOTMATCH);
                        return SelectOutdoorResult.NotMatch;
                    }
                    #endregion
                }
                else if (series.Contains("H(R/Y)NM1Q"))
                {
                    #region//HAPQ IVX
                    double IVXoutstdhp = curSystemItem.OutdoorItem.Horsepower;
                    curSystemItem.Ratio = Math.Round(tot_indhp / IVXoutstdhp, 3);
                    //根据室外温度和室外机型号进行检查 20170702 by Yunxiao Lin
                    if (curSystemItem.DBCooling <= -10)
                    {
                        switch (ODUmodel)
                        {
                            case "RAS-3HRNM1Q":
                                //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                if (exists4way)
                                {
                                    //ShowMessage: [ODUmodel]不能与Four way type 室内机搭配使用。
                                    ERRList.Add(Msg.OUTD_NotMatch_FourWayTypeIDU(ODUmodel)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (curSystemItem.Ratio < 0.85)
                                {
                                    //ShowMessage: 当环境温度小于等于-10℃时，[ODUmodel]与室内机的容量配比范围为85%~100%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_LessOrEqual(-10, ODUmodel, 85, 100)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (curSystemItem.Ratio > 1)
                                {
                                    //ShowMessage: 当环境温度小于-10℃时，[ODUmodel]与室内机的容量配比范围为85%-100%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_Less(-10, ODUmodel, 85, 100)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                break;
                            case "RAS-4HRNM1Q":
                            case "RAS-5HRNM1Q":
                            case "RAS-5HYNM1Q":
                                //室内机数量限制
                                if (listRISelected.Count > 4)
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "4"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "3"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (curSystemItem.Ratio < 0.9)
                                {
                                    //ShowMessage: 当环境温度小于等于-10℃时，[ODUmodel]与室内机的容量配比范围为90%-112%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_LessOrEqual(-10, ODUmodel, 90, 112));
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (curSystemItem.Ratio > 1.12)
                                {
                                    //ShowMessage: 当环境温度小于等于-10℃时，[ODUmodel]与室内机的容量配比范围为90%-112%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_LessOrEqual(-10, ODUmodel, 90, 112));
                                    return SelectOutdoorResult.NotMatch;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (ODUmodel)
                        {
                            case "RAS-3HRNM1Q":
                                //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                if (exists4way)
                                {
                                    //ShowMessage: [ODUmodel]不能与Four way type 室内机搭配使用。
                                    ERRList.Add(Msg.OUTD_NotMatch_FourWayTypeIDU(ODUmodel)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (curSystemItem.Ratio < 0.85)
                                {
                                    //ShowMessage: 当环境温度大于-10℃时，[ODUmodel]与室内机的容量配比范围为85%~100%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_More(-10, ODUmodel, 85, 100)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (curSystemItem.Ratio > 1.2)
                                    return SelectOutdoorResult.NotMatch;
                                //室内机最大匹数限制
                                //1-for-1 室内机最大匹数3
                                if (listRISelected.Count == 1)
                                {
                                    if (max_indhp > 3)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过3HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 3)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //1-for-N 室内机最大匹数2.3
                                else if (listRISelected.Count > 1)
                                {
                                    if (max_indhp > 2.3)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-N连接，[ODUmodel]连接的室内机匹数不能超过2.3HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1ForN_More(-10, ODUmodel, 2.3)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                break;
                            case "RAS-4HRNM1Q":
                                //室内机数量限制
                                if (listRISelected.Count > 5)
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "5"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "3"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (listRISelected.Count <= 4)
                                {
                                    if (curSystemItem.Ratio < 0.9)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-135%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 135)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.35)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-135%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 135)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                else if (listRISelected.Count <= 5)
                                {
                                    if (curSystemItem.Ratio < 0.95)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.2)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //室内机最大匹数限制
                                //1-for-1 室内机最大匹数4
                                if (listRISelected.Count == 1)
                                {
                                    if (max_indhp > 4)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过4HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 4)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //1-for-N 室内机最大匹数2.5
                                else if (listRISelected.Count > 1)
                                {
                                    if (max_indhp > 2.5)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-N连接，[ODUmodel]连接的室内机匹数不能超过2.5HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1ForN_More(-10, ODUmodel, 2.5)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                break;
                            case "RAS-5HRNM1Q":
                            case "RAS-5HYNM1Q":
                                //室内机数量限制
                                if (listRISelected.Count > 5)
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "5"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "3"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (listRISelected.Count <= 4)
                                {
                                    if (curSystemItem.Ratio < 0.9)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-130%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 130)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.30)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-130%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 130)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                else if (listRISelected.Count <= 5)
                                {
                                    if (curSystemItem.Ratio < 0.95)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.2)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //室内机最大匹数限制
                                //1-for-1 室内机最大匹数4
                                if (listRISelected.Count == 1)
                                {
                                    if (max_indhp > 4)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过4HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 4)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //1-for-N 室内机最大匹数3
                                else if (listRISelected.Count > 1)
                                {
                                    if (max_indhp > 3)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过3HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 3)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                break;
                        }
                    }
                    #endregion
                }
            }
            // 检查容量配比率（此处仅校验上限值）
            double outstdcap_c = curSystemItem.OutdoorItem.CoolingCapacity;
            outstdhp = curSystemItem.OutdoorItem.Horsepower;
            curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
            ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
            curSystemItem.RatioFA = ratioFA;

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    return SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                if (curSystemItem.Ratio < 0.5)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    return SelectOutdoorResult.NotMatch;
                    //break;
                }
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    //continue;
                    return SelectOutdoorResult.NotMatch;
                }
                else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                {
                    // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                    string model_Hitachi = curSystemItem.OutdoorItem.Model_Hitachi;
                    if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                    {
                        if (curSystemItem.Ratio > 1.2)
                        {
                            ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, 1.2));
                            return SelectOutdoorResult.NotMatch;
                        }
                    }
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变
                if (curSystemItem.Ratio < 0.8)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    //break;
                    return SelectOutdoorResult.NotMatch;
                }
                if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
                {  //continue;
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    return SelectOutdoorResult.NotMatch;
                }
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {   //continue;
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    return SelectOutdoorResult.NotMatch;
                }
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
                { // continue; // add on 20160307 clh 
                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    return SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                return SelectOutdoorResult.NotMatch;
            }
            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
             *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
             *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
             *3. Total ratio of Hydro Free 0%~100% 
             *4. Only Hydro Free is not allowed 
             **/
            if (existsHydroFree)
            {
                ratioNoHDIU = Math.Round(tot_noHDhp / outstdhp, 3);
                ratioHD = Math.Round(tot_HDhp / outstdhp, 3);
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    if (ratioNoHDIU < 0.5)
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    return SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    return SelectOutdoorResult.NotMatch;
                }
            }

            Outdoor outItem = curSystemItem.OutdoorItem;//bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
                                                        ////获取室外机修正系数 add by axj 20170111
                                                        //double Total_IDU_Factor = bll.GetTotalIDUFactor(outItem.ModelFull, tot_indhp, false);
                                                        //if (Total_IDU_Factor == 0)
                                                        //{
                                                        //    ERRList.Add(Msg.WARNING_DATA_EXCEED);
                                                        //    return SelectOutdoorResult.Null;
                                                        //}
                                                        ////比较室外机额定容量和室内机估算容量之和 20161114 by Yunxiao Lin
                                                        //if (outstdcap_c * Total_IDU_Factor < tot_indcap_c)
                                                        //{
                                                        //    //continue;
                                                        //    //RRList.Add(Msg.OUTD_NOTMATCH);//OUTD_COOLINGCAPACITY
                                                        //    ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
                                                        //    return SelectOutdoorResult.Null;
                                                        //}
                                                        ////比较室外机额定容量与Actual Ratio的乘积和室内机额定容量之和 20170122 by Yunxiao Lin
                                                        //if (curSystemItem.Ratio >= 1)
                                                        //{
                                                        //    if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                                                        //    {
                                                        //        ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
                                                        //        return SelectOutdoorResult.Null;
                                                        //    }
                                                        //}
                                                        //else
                                                        //{
                                                        //    if (outstdcap_c < tot_indstdcap_c)
                                                        //    {
                                                        //        ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
                                                        //        return SelectOutdoorResult.Null;
                                                        //    }
                                                        //}
            
            if (curSystemItem.SysType == SystemType.OnlyFreshAirMulti || curSystemItem.SysType == SystemType.OnlyFreshAir)
            {
                inWB = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
                inDB = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
            }

            //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
            //以下调用CalOutdoorEstCapacity的参数actRatio全部改为Horsepower 20161110 by Yunxiao Lin
            if (!outItem.ProductType.Contains("Water Source"))
                curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.DBCooling, inWB, false, curSystemItem);
            else
                curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWCooling, inWB, false, curSystemItem);

            if (thisProject.IsCoolingModeEffective)
            {
                //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                if (curSystemItem.CoolingCapacity == 0)
                {
                    ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                    return SelectOutdoorResult.NotMatch;
                }

                curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
                if (curSystemItem.PipingLengthFactor == 0)
                {
                    string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                    double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                    return SelectOutdoorResult.Null;
                }

                curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
            }
            //  Hitachi的Fresh Air 不需要比较HeatCapacity值
            if (hasHeating)
            {
                //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
                if (!outItem.ProductType.Contains("Water Source"))
                    curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.WBHeating, inDB, true, curSystemItem);
                else
                    curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWHeating, inDB, true, curSystemItem);

                //水冷机不需要除霜修正
                if (!outItem.ProductType.Contains("Water Source"))
                {
                    //double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
                    //从数据库获取除霜修正系数 20180626 by Yunxiao Lin
                    double defrostingFactor = bll.GetODUDefrostFactor(curSystemItem.DBHeating, outItem.Series);
                    curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Heating");   //添加制热Factor
                    if (curSystemItem.PipingLengthFactor_H == 0)
                    {
                        string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                        double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                        double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                        JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                        return SelectOutdoorResult.Null;
                    }
                    //curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
                    curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor_H * defrostingFactor;
                }

                //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                if (curSystemItem.HeatingCapacity == 0)
                {
                    ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                    return SelectOutdoorResult.NotMatch;
                }
            }

            //海拔修正 add on 20160517 by Yunxiao Lin
            //注意某些机型可能无此限制? Wait check
            if (thisProject.EnableAltitudeCorrectionFactor)
            {
                //从数据库获取海拔修正系数 20180626 by Yunxiao Lin
                //double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
                double acf = bll.GetODUAltitudeFactor(thisProject.Altitude, outItem.Series);
                curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;

                // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
                // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
                if (hasHeating)
                {
                    curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
                }
            }

            //// 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
            //if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
            //{
            //    if (curSystemItem.Ratio > 1)
            //    {
            //        if (!curSystemItem.AllowExceedRatio)
            //            continue;
            //    }
            //}

            //计算室内机实际容量 add by Shen Junjie on 20181113
            CalacIndoorActualCapacity(curSystemItem, listRISelected, csMode, hasHeating, tot_indhp, tot_indcap_c, tot_indcap_h);

            //比较室内机和房间的需求是否符合
            bool roomchecked = true;
            foreach (RoomIndoor ri in listRISelected)
            {
                string wType;
                if (!CommonBLL.MeetRoomRequired(ri, thisProject, tolerance, thisProject.RoomIndoorList, out wType))
                    roomchecked = false;
                if (!roomchecked)
                    break;
            }
            if (!roomchecked)
            {  //continue;
                ERRList.Add(Msg.OUTD_NOTMATCH);//"IND_WARN_CAPActCoolLower IND_WARN_CAPActHeatLower");
                returnType = SelectOutdoorResult.NotMatch;
            }

            OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
            //break; // 找到合适的室外机即跳出循环

            //}
            // 遍历自动选型 END
            #endregion

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机
                // 2-2.内机总冷量配置率为50%～130%；
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                {
                    // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                    string model_Hitachi = curSystemItem.OutdoorItem.Model_Hitachi;
                    if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                    {
                        if (curSystemItem.Ratio > 1.2)
                        {
                            OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                            ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, 1.2));
                            returnType = SelectOutdoorResult.NotMatch;
                        }
                    }
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变

                // 1-2.新风机冷量与室外机的配置率不大于30%
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
                {
                    if (string.IsNullOrEmpty(OutdoorModelFull))
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();

                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    returnType = SelectOutdoorResult.NotMatch;
                }

                // 1-3.内机总冷量配置率为80%～105%；
                if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }
            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
            *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
            *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
            *3. Total ratio of Hydro Free 0%~100% 
            *4. Only Hydro Free is not allowed 
            **/
            if (existsHydroFree)
            {
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过系统的30%    -- add by Shen Junjie on 2017/12/22
            //if (tot_HP_KPI_X4E / tot_indhp > 0.3)
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                returnType = SelectOutdoorResult.NotMatch;
            }

            if (!string.IsNullOrEmpty(OutdoorModelFull))
            {
                //curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
                // updated by clh
                if (curSystemItem.OutdoorItem != null)
                {
                    //配管图的各种限制值的取得 add by Shen Junjie on 20180620
                    pipBll.SetPipingLimitation(curSystemItem);

                    //如果按室外机选型得不到正确结果，使用按室内机选型再试一遍 20161130 by Yunxiao Lin
                    if (returnType != SelectOutdoorResult.OK)
                        return DoSelectOutdoorIDUFirstManual(curSystemItem, listRISelected, thisProject, out ERRList);
                    else
                        return returnType;
                }

            }
            if ((thisProject != null) && (thisProject.ProductType.Contains("Water Source")))
                ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
            else
                ERRList.Add(Msg.OUTD_NOTMATCH);
            ////如果按室外机选型得不到正确结果，按室内机选型再试一遍 20161130 by Yunxiao Lin
            //if (returnType != SelectOutdoorResult.OK)
            //    return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            return SelectOutdoorResult.Null;
        }

        /// 手动选择匹配的室外机-室内机优先（包括非严格一对一的情况）20161223 add by axj
        /// 水机选型增加FlowRateLevel 参数 20170216 by Yunxiao Lin
        /// <summary>
        /// 手动选择匹配的室外机-室内机优先（包括非严格一对一的情况）
        /// </summary>
        private static SelectOutdoorResult DoSelectOutdoorIDUFirstManual(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, out List<string> ERRList)
        {
            DateTime t1 = DateTime.Now;
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);

            ERRList = new List<string>();
            ERRList.Clear();    // 每次选型前清空错误记录表
            //curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL(thisProject);

            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }

            CapacitySelectionMode csMode = GetCapacitySelectionMode(thisProject);

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            //string productType = listRISelected[0].IndoorItem.ProductType;
            //bool hasHeating = thisProject.IsHeatingModeEffective && !productType.Contains(", CO");
            string series = listRISelected[0].IndoorItem.Series;
            if (string.IsNullOrEmpty(series))
            {
                series = curSystemItem.Series;
            }
            bool isCoolingOnly = series.Contains(" CO,");
            bool hasHeating = thisProject.IsHeatingModeEffective && !isCoolingOnly;

            DataTable dtOutdoorStd = new DataTable();
            // 获取室外机标准表（初次加载或者更改室外机类型时）
            if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
                if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
                {
                    // ERROR:室外机标准表无数据记录！
                    ERRList.Add(Msg.DB_NODATA);
                    return SelectOutdoorResult.Null;
                }
            }

            //add by axj 20170120 室内机工况温度
            double inWB = 0; //制冷湿球温度
            double inDB = 0; //制热干球温度

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double ratioFA = 0;
            double tot_indhpOnly = 0;
            double tot_indhp = 0;

            //判断室内机中是否含有Hydro Free 20171220 by Yunxiao Lin
            bool existsHydroFree = false;
            //计算Hydro Free总容量和非Hydro Free室内机的总容量 20171220 by Yunxiao Lin
            double tot_HDhp = 0;
            double tot_noHDhp = 0;
            double ratioNoHDIU = 0;
            double ratioHD = 0;
            double tot_HP_KPI_X4E = 0; // KPI-X4E (全热交换机)的总匹数
            double outstdhp = 0; // ODU的总匹数
            foreach (RoomIndoor ri in listRISelected)
            {
                //增加Hydro Free室内机判断,并分别统计系统内Hydro Free和非Hydro Free室内机的容量 20171220 by Yunxiao Lin
                if (ri.IndoorItem.Type.Contains("Hydro Free"))
                {
                    existsHydroFree = true;
                    tot_HDhp += ri.IndoorItem.Horsepower;
                }
                else
                {
                    tot_noHDhp += ri.IndoorItem.Horsepower;
                }

                if (ri.IndoorItem.Type == "Total Heat Exchanger (KPI-X4E)")
                {
                    tot_HP_KPI_X4E += ri.IndoorItem.Horsepower;
                }

                tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                tot_indcap_h += ri.HeatingCapacity;
                tot_indhp += ri.IndoorItem.Horsepower;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                    {
                        // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                        Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                        if (inItem != null)
                        {
                            inItem.Series = series;
                            ri.SetIndoorItemWithAccessory(inItem);
                        }

                    }
                    tot_FAhp += ri.IndoorItem.Horsepower;

                }
            }
            tot_indhpOnly = tot_indhp - tot_FAhp;

            //获取室内机工况温度 add by Shen Junjie on 20181113
            GetIDUWorkTemperature(csMode, listRISelected, out inWB, out inDB);

            bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
            #region //确定当前内机组合模式
            // 1.混连模式
            if (isComposite)
            {
                // 1-1.必须同时包含室内机与新风机；
                if (tot_indhpOnly == 0 || tot_FAhp == 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
                    return SelectOutdoorResult.Null;
                }
            }
            else
            {
                // 2-1.不允许同时包含室内机和新风机；
                if (tot_indhpOnly > 0 && tot_FAhp > 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
                    return SelectOutdoorResult.Null;
                }

                // 2.全室内机模式
                if (tot_indhpOnly > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyIndoor;
                }
                // 3.全新风机模式
                else if (tot_FAhp > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

                    if (listRISelected.Count == 1)
                    {
                        Indoor inItem = listRISelected[0].IndoorItem;

                        #region 一对一新风机
                        // 一对一新风机不适用于 SMZ Gen.2  on 20180918 
                        bool isSMZ = false;
                        //Add FSNC7B/5B by Yunxiao Lin 20190107
                        if (!string.IsNullOrEmpty(series) && (series.Contains("FSNP") || series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1") || series.Contains("FSNC7B") || series.Contains("FSNC5B")))
                        {
                            isSMZ = true;
                        }
                        if (!isSMZ)
                        {
                            curSystemItem.SysType = SystemType.OnlyFreshAir;

                            // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                            // 20160821 新增productType参数 by Yunxiao Lin
                            inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(inItem.ModelFull, inItem.Type, false, inItem.ProductType, inItem.Series);
                            if (inItem != null)
                            {
                                string UniqueOutdoorName = inItem.UniqueOutdoorName;

                                ////按照设定的model取一对一室外机需要判断品牌。 20161024 by Yunxiao Lin
                                //if (thisProject.BrandCode == "Y")
                                //    curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, series);
                                //else
                                //    curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, series);
                                //同一个新风机可能被多个系列共用，因此可能存在多个一对一室外机，用“/”分隔 20170330 by Yunxiao Lin
                                if (!UniqueOutdoorName.Contains("/"))
                                {
                                    if (thisProject.BrandCode == "Y")
                                        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, series);
                                    else
                                        curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, series);
                                }
                                else
                                {
                                    string[] UniqueOutdoorNameList = UniqueOutdoorName.Split(new char[] { '/' });
                                    foreach (string model in UniqueOutdoorNameList)
                                    {
                                        if (thisProject.BrandCode == "Y")
                                            curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, series);
                                        else
                                            curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(model, series);
                                        if (curSystemItem.OutdoorItem != null)
                                            break;
                                    }
                                }
                                // if (curSystemItem.OutdoorItem.ModelFull == UniqueOutdoorName)  //按品牌选择Model Name on 2018/8/29
                                if ((thisProject.BrandCode == "Y" ? curSystemItem.OutdoorItem.Model_York : curSystemItem.OutdoorItem.Model_Hitachi) == UniqueOutdoorName)
                                {
                                    // ERROR:数据库中的一对一室外机ModelName写错
                                    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
                                    return SelectOutdoorResult.Null;
                                }
                                else
                                {
                                    curSystemItem.MaxRatio = 1;
                                    //Connection Ratio改为由HorsePower对比计算
                                    curSystemItem.Ratio = inItem.Horsepower / curSystemItem.OutdoorItem.Horsepower;
                                    // FreshAir时不需要估算容量,直接绑定室外机的标准值
                                    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                    //一对一新风机将室外机的Capacity直接绑定到RoomIndoor 20161114 by Yunxiao Lin
                                    listRISelected[0].ActualCoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                                    listRISelected[0].ActualHeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                                    listRISelected[0].ActualSensibleHeat = 0d;

                                    pipBll.SetPipingLimitation(curSystemItem);
                                    // 一对一新风机选型成功！
                                    return SelectOutdoorResult.OK;
                                }
                            }
                        }
                        #endregion
                    }
                }// 全新风机 END
            }// 模式确定 END
            #endregion

            SelectOutdoorResult returnType = SelectOutdoorResult.OK;

            #region // 遍历室外机标准表逐个筛选

            // update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
            //double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
            //室外机选型改为判断Series 20161031 by Yunxiao Lin
            //DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''", "Model asc");

            List<RoomIndoor> listRISelected_temp = null;
            bool isSelected = false;
            // 遍历选型过程 START 
            //foreach (DataRow r in rows)
            //{
            //每次选新的室外机都拷贝一份室内机列表
            listRISelected_temp = listRISelected.DeepClone();
            bool IDUincrease = true; //是否继续增大室内机，如果为否，则进入下一个室外机循环
            bool outbreak = false; //是否跳出室外机循环
            bool isFirst = true;
            while (IDUincrease) //如果只增大室内机不会超限，则继续增大。
            {
                if (!isFirst)
                {
                    //由于上一个室内机循环型号可能已经被改变，所以室内机的各项总指标需要重新计算。
                    tot_indcap_c = 0;
                    tot_indcap_h = 0;
                    tot_FAhp = 0;
                    ratioFA = 0;
                    tot_indhpOnly = 0;
                    tot_indhp = 0;
                    foreach (RoomIndoor ri in listRISelected_temp)
                    {
                        tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                        tot_indcap_h += ri.HeatingCapacity;
                        tot_indhp += ri.IndoorItem.Horsepower;
                        if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                        {
                            if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                            {
                                // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                                Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                                if (inItem != null)
                                {
                                    inItem.Series = series;
                                    ri.SetIndoorItemWithAccessory(inItem);
                                }

                            }
                            tot_FAhp += ri.IndoorItem.Horsepower;

                        }
                    }
                    tot_indhpOnly = tot_indhp - tot_FAhp;
                }
                isFirst = false;

                // 检查最大连接数 
                //int maxIU = 0;
                //int.TryParse(r["MaxIU"].ToString(), out maxIU);
                if (curSystemItem.OutdoorItem.MaxIU < listRISelected.Count)
                {
                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), curSystemItem.OutdoorItem.MaxIU.ToString()));//室内机数量超过室外机的最大连接数！
                    IDUincrease = false;
                    continue;
                }

                // 检查容量配比率（此处仅校验上限值）
                double outstdcap_c = curSystemItem.OutdoorItem.CoolingCapacity;
                outstdhp = curSystemItem.OutdoorItem.Horsepower;
                curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
                ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
                curSystemItem.RatioFA = ratioFA;

                if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
                {
                    //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                    if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                        {
                            IDUincrease = false;
                            outbreak = true;
                            continue;
                        }
                    }
                }

                if (curSystemItem.SysType == SystemType.OnlyIndoor)
                {
                    // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                    if (curSystemItem.Ratio < 0.5)
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                        ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                        {
                            IDUincrease = false;
                            outbreak = true;
                            continue;
                        }
                    }
                    if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                        ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                        IDUincrease = false;
                        continue;
                    }
                    else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                    {
                        // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                        string model_Hitachi = curSystemItem.OutdoorItem.Model_Hitachi;
                        if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                        {
                            if (curSystemItem.Ratio > 1.2)
                            {
                                OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                                ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, 1.2));
                                IDUincrease = false;
                                continue;
                            }
                        }
                    }

                }
                else
                {
                    // 多新风机或者混连模式，则配比率校验规则有变
                    if (curSystemItem.Ratio < 0.8)
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                        ERRList.Add(Msg.OUTD_RATIO_Composite);
                        IDUincrease = false;
                        outbreak = true;
                        continue;
                    }
                    if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                        ERRList.Add(Msg.OUTD_RATIO_Composite);
                        IDUincrease = false;
                        continue;
                    }
                    if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                        ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                        IDUincrease = false;
                        continue; // add on 20160307 clh
                    }
                }
                // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
                if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
                {
                    ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                    IDUincrease = false;
                    continue;
                }
                /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
                 *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
                 *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                 *3. Total ratio of Hydro Free 0%~100% 
                 *4. Only Hydro Free is not allowed 
                 **/
                if (existsHydroFree)
                {
                    ratioNoHDIU = Math.Round(tot_noHDhp / outstdhp, 3);
                    ratioHD = Math.Round(tot_HDhp / outstdhp, 3);
                    if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                        ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                        IDUincrease = false;
                        if (ratioNoHDIU < 0.5)
                            outbreak = true;
                        continue;
                    }
                    else if (ratioHD < 0 || ratioHD > 1)
                    {
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                        ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                        IDUincrease = false;
                        outbreak = true;
                        continue;
                    }
                }

                Outdoor outItem = curSystemItem.OutdoorItem; //bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
                                                             ////获取室外机修正系数 add by axj 20170111
                                                             //double Total_IDU_Factor = bll.GetTotalIDUFactor(outItem.ModelFull, tot_indhp, false);
                                                             //if (Total_IDU_Factor == 0)
                                                             //{
                                                             //    ERRList.Add(Msg.WARNING_DATA_EXCEED);
                                                             //    IDUincrease = false;
                                                             //    continue;
                                                             //}
                                                             ////比较室外机额定容量和室内机估算容量之和 20161114 by Yunxiao Lin
                                                             //if (outstdcap_c * Total_IDU_Factor < tot_indcap_c)
                                                             //{
                                                             //    //ERRList.Add(Msg.OUTD_NOTMATCH);
                                                             //    ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
                                                             //    IDUincrease = false;
                                                             //    continue;
                                                             //}

                ////比较室外机额定容量与Actual Ratio的乘积和室内机额定容量之和 20170122 by Yunxiao Lin
                //if (curSystemItem.Ratio >= 1)
                //{
                //    if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                //    {
                //        ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
                //        IDUincrease = false;
                //        continue;
                //    }
                //}
                //else
                //{
                //    if (outstdcap_c < tot_indstdcap_c)
                //    {
                //        ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
                //        IDUincrease = false;
                //        continue;
                //    }
                //}

                //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
                //以下调用CalOutdoorEstCapacity的参数actRatio全部改为Horsepower 20161110 by Yunxiao Lin
                if (!outItem.ProductType.Contains("Water Source"))
                    curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.DBCooling, inWB, false, curSystemItem);
                else
                    curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWCooling, inWB, false, curSystemItem);

                if (thisProject.IsCoolingModeEffective)
                {
                    //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                    if (curSystemItem.CoolingCapacity == 0)
                    {
                        ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                        return SelectOutdoorResult.NotMatch;
                    }
                    curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
                    if (curSystemItem.PipingLengthFactor == 0)
                    {
                        string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                        double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                        double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                        JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                        return SelectOutdoorResult.Null;
                    }

                    curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
                }
                //  Hitachi的Fresh Air 不需要比较HeatCapacity值
                if (hasHeating)
                {
                    //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
                    if (!outItem.ProductType.Contains("Water Source"))
                        curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.WBHeating, inDB, true, curSystemItem);
                    else
                        curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWHeating, inDB, true, curSystemItem);
                    //水冷机不需要除霜修正
                    if (!outItem.ProductType.Contains("Water Source"))
                    {
                        //double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
                        //从数据库获取除霜修正系数 20180626 by Yunxiao Lin
                        double defrostingFactor = bll.GetODUDefrostFactor(curSystemItem.DBHeating, outItem.Series);
                        curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Heating");   //添加制热Factor
                        if (curSystemItem.PipingLengthFactor_H == 0)
                        {
                            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                            double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                            double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                            JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                            return SelectOutdoorResult.Null;
                        }
                        //curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
                        curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor_H * defrostingFactor;
                    }

                    //判断如果HeatingCapacity 为 弹出提醒 on 20180510 by xyj
                    if (curSystemItem.HeatingCapacity == 0)
                    {
                        ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                        return SelectOutdoorResult.NotMatch;
                    }
                }

                //海拔修正 add on 20160517 by Yunxiao Lin
                //注意某些机型可能无此限制? Wait check
                if (thisProject.EnableAltitudeCorrectionFactor)
                {
                    //从数据库获取海拔修正系数 20180626 by Yunxiao Lin
                    //double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
                    double acf = bll.GetODUAltitudeFactor(thisProject.Altitude, outItem.Series);
                    curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;

                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
                    // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
                    if (hasHeating)
                    {
                        curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
                    }
                }

                // 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
                if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
                {
                    if (curSystemItem.Ratio > 1)
                    {
                        if (!curSystemItem.AllowExceedRatio)
                        {
                            ERRList.Add(Msg.OUTD_NOTMATCH);
                            IDUincrease = false;
                            continue;
                        }
                    }
                }

                //计算室内机实际容量 add by Shen Junjie on 20181113
                CalacIndoorActualCapacity(curSystemItem, listRISelected_temp, csMode, hasHeating, tot_indhp, tot_indcap_c, tot_indcap_h);

                //比较室内机和房间的需求是否符合
                bool tot_roomchecked = true;
                IDUincrease = false;
                foreach (RoomIndoor ri in listRISelected_temp)
                {
                    bool roomchecked = true;
                    string wType;
                    if (!CommonBLL.MeetRoomRequired(ri, thisProject, 0, listRISelected_temp, out wType))
                        roomchecked = false;
                    if (!roomchecked)
                    {
                        tot_roomchecked = false;
                        if (ri.IsAuto)
                        {
                            Indoor item = ri.IndoorItem.DeepClone();
                            IndoorBLL indbll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
                            item = indbll.getBiggerIndoor(item);
                            if (item != null)
                            {
                                IDUincrease = true;
                                ri.SetIndoorItem(item);
                                //需要重算est. value
                                ri.CoolingCapacity = indbll.CalIndoorEstCapacity(ri.IndoorItem, curSystemItem.DBCooling, ri.WBCooling, false);
                                //if (!ri.IndoorItem.ProductType.Contains(", CO"))
                                if (!isCoolingOnly)
                                {
                                    //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
                                    ri.HeatingCapacity = indbll.CalIndoorEstCapacity(ri.IndoorItem, curSystemItem.WBHeating, ri.DBHeating, true);
                                    //显热改为由估算容量*SHF计算得到 20161111
                                    //double shf = 0;
                                    //if (ri.SHF_Mode.Equals("High"))
                                    //    shf = ri.IndoorItem.SHF_Hi;
                                    //else if (ri.SHF_Mode.Equals("Medium"))
                                    //    shf = ri.IndoorItem.SHF_Med;
                                    //else if (ri.SHF_Mode.Equals("Low"))
                                    //    shf = ri.IndoorItem.SHF_Lo;

                                    //if (shf == 0d)
                                    //    shf = ri.IndoorItem.SHF_Hi;

                                    ri.SensibleHeat = ri.CoolingCapacity * ri.SHF;
                                }
                            }
                            else
                            {
                                //如果已经无法继续增大室内机，则选型失败。
                                ERRList.Add(Msg.OUTD_NOTMATCH);
                                IDUincrease = false;
                                isSelected = false;
                                outbreak = true;
                                break;
                            }
                        }
                    }
                }
                if (!tot_roomchecked)
                {
                    continue;
                }
                else
                {
                    IDUincrease = false;
                    isSelected = true;
                    break;
                }
            }
            if (!IDUincrease && !isSelected)
            {
                if (ERRList.Count == 0)
                    //如果已经无法继续增大室内机，则选型失败。
                    ERRList.Add(Msg.OUTD_NOTMATCH);

                return SelectOutdoorResult.NotMatch;
                //if (outbreak)
                //    break;
                //else
                //    continue;
            }

            OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;//r["ModelFull"].ToString();
                                                                   //break; // 找到合适的室外机即跳出循环

            //}
            // 遍历自动选型 END
            #endregion

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机
                // 2-2.内机总冷量配置率为50%～130%；
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;//rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                {
                    // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                    string model_Hitachi = curSystemItem.OutdoorItem.Model_Hitachi;
                    if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                    {
                        if (curSystemItem.Ratio > 1.2)
                        {
                            OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                            ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, 1.2));
                            returnType = SelectOutdoorResult.NotMatch;
                        }
                    }
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变

                // 1-2.新风机冷量与室外机的配置率不大于30%
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
                {
                    if (string.IsNullOrEmpty(OutdoorModelFull))
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();

                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    returnType = SelectOutdoorResult.NotMatch;
                }

                // 1-3.内机总冷量配置率为80%～105%；
                if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
            *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
            *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
            *3. Total ratio of Hydro Free 0%~100% 
            *4. Only Hydro Free is not allowed 
            **/
            if (existsHydroFree)
            {
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过系统的30%    -- add by Shen Junjie on 2017/12/22
            //if (tot_HP_KPI_X4E / tot_indhp > 0.3)
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/29
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                returnType = SelectOutdoorResult.NotMatch;
            }

            if (!string.IsNullOrEmpty(OutdoorModelFull))
            {
                //curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
                if (curSystemItem.OutdoorItem != null)
                {
                    pipBll.SetPipingLimitation(curSystemItem);
                    //将已改变的RoomIndoorList复制回来,注意不能简单的Clone
                    foreach (RoomIndoor rt in listRISelected_temp)
                    {
                        foreach (RoomIndoor ri in listRISelected)
                        {
                            if (rt.IsAuto && ri.IsAuto && rt.RoomID == ri.RoomID && rt.IsFreshAirArea == ri.IsFreshAirArea)
                            {
                                ri.CoolingCapacity = rt.CoolingCapacity;
                                ri.HeatingCapacity = rt.HeatingCapacity;
                                ri.SensibleHeat = rt.SensibleHeat;
                                ri.ActualCoolingCapacity = rt.ActualCoolingCapacity;
                                ri.ActualHeatingCapacity = rt.ActualHeatingCapacity;
                                ri.ActualSensibleHeat = rt.ActualSensibleHeat;
                                ri.SetIndoorItemWithAccessory(rt.IndoorItem);
                            }
                        }
                    }
                    DateTime t2 = DateTime.Now;
                    TimeSpan ts = t2 - t1;
                    double MilliSec = ts.TotalMilliseconds;
                    //MessageBox.Show("Cost Time：" + MilliSec.ToString() + "MilliSec");

                    return returnType;
                }

            }
            if ((thisProject != null) && (thisProject.ProductType.Contains("Water Source")))
                ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
            else
                ERRList.Add(Msg.OUTD_NOTMATCH);
            return SelectOutdoorResult.Null;
        }

        /// 通用室内机自动选择匹配的室外机-室外机优先（包括非严格一对一的情况）   add on 20171212 by Lingjia Qiu
        /// 增加公差参数，默认为0.05 20161125 by Yunxiao Lin
        /// SystemVRF增加FlowRateLevel参数 20170216 by Yunxiao Lin
        /// <summary>
        /// 通用室内机自动选择匹配的室外机-室外机优先（包括非严格一对一的情况）
        /// </summary>
        public static SelectOutdoorResult DoSelectUniversalODUFirst(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, string series, out List<string> ERRList, out List<string> MSGList)
        {
            ERRList = new List<string>();
            MSGList = new List<string>();
            ERRList.Clear();    // 每次选型前清空错误记录表
            MSGList.Clear();
            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            //string productType = listRISelected[0].IndoorItem.ProductType;
            //string series = listRISelected[0].IndoorItem.Series;

            if (curSystemItem != null && curSystemItem.IDUFirst && !series.Contains("IVX"))
                return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode, thisProject.RegionCode);
            double tolerance = 0.05;

            if (curSystemItem.OutdoorItem != null)
            {
                //记住series以便自动重新选型时传入Series  add by Shen Junjie on 2018/3/29
                curSystemItem.Series = curSystemItem.OutdoorItem.Series;
            }

            CapacitySelectionMode csMode = GetCapacitySelectionMode(thisProject);

            //从室内机属性中获取productType变量
            //MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            //string productType = productTypeBll.GetProductTypeBySeries(thisProject.BrandCode, thisProject.SubRegionCode, curSystemItem.Series);
            //bool hasHeating = thisProject.IsHeatingModeEffective && !productType.Contains(", CO");
            bool isCoolingOnly = series.Contains(" CO,");
            bool hasHeating = thisProject.IsHeatingModeEffective && !isCoolingOnly;

            //curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL(thisProject);

            DataTable dtOutdoorStd = new DataTable();
            // 获取室外机标准表（初次加载或者更改室外机类型时）
            if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
                if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
                {
                    // ERROR:室外机标准表无数据记录！
                    ERRList.Add(Msg.DB_NODATA);
                    return SelectOutdoorResult.Null;
                }
            }
            //add by axj 20170120 室内机工况温度
            double inWB = 0; //制冷湿球温度
            double inDB = 0; //制热干球温度

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double ratioFA = 0;
            double tot_indhpOnly = 0;
            double tot_indhp = 0;
            //判断室内机中是否含有4 way 机型 20170702 by Yunxiao Lin
            bool exists4way = false;
            //记录最大的室内机匹数 20170702 by Yunxiao Lin
            double max_indhp = 0;
            //判断室内机中是否含有Hydro Free 20171220 by Yunxiao Lin
            bool existsHydroFree = false;
            //计算Hydro Free总容量和非Hydro Free室内机的总容量 20171220 by Yunxiao Lin
            double tot_HDhp = 0;
            double tot_noHDhp = 0;
            double ratioNoHDIU = 0;
            double ratioHD = 0;
            double tot_HP_KPI_X4E = 0; // KPI-X4E (全热交换机)的总匹数
            double outstdhp = 0;
            foreach (RoomIndoor ri in listRISelected)
            {
                #region
                //增加4 way 室内机判断，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Type.Contains("Four Way"))
                    exists4way = true;

                //记录最大的室内机匹数，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Horsepower > max_indhp)
                    max_indhp = ri.IndoorItem.Horsepower;

                //增加Hydro Free室内机判断,并分别统计系统内Hydro Free和非Hydro Free室内机的容量 20171220 by Yunxiao Lin
                if (ri.IndoorItem.Type.Contains("Hydro Free"))
                {
                    existsHydroFree = true;
                    tot_HDhp += ri.IndoorItem.Horsepower;
                }
                else
                {
                    tot_noHDhp += ri.IndoorItem.Horsepower;
                }

                if (ri.IndoorItem.Type == "Total Heat Exchanger (KPI-X4E)")
                {
                    tot_HP_KPI_X4E += ri.IndoorItem.Horsepower;
                }

                tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                tot_indcap_h += ri.HeatingCapacity;
                tot_indhp += ri.IndoorItem.Horsepower;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                    {
                        // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                        Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                        if (inItem != null)
                        {
                            //inItem.Series = series;
                            ri.SetIndoorItemWithAccessory(inItem);
                        }

                    }
                    tot_FAhp += ri.IndoorItem.Horsepower;

                }
                #endregion
            }
            tot_indhpOnly = tot_indhp - tot_FAhp;
            
            //获取室内机工况温度 add by Shen Junjie on 20181113
            GetIDUWorkTemperature(csMode, listRISelected, out inWB, out inDB);

            bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
            #region //确定当前内机组合模式
            // 1.混连模式
            if (isComposite)
            {
                // 1-1.必须同时包含室内机与新风机；
                if (tot_indhpOnly == 0 || tot_FAhp == 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
                    return SelectOutdoorResult.Null;
                }
            }
            else
            {
                // 2-1.不允许同时包含室内机和新风机；
                if (tot_indhpOnly > 0 && tot_FAhp > 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
                    return SelectOutdoorResult.Null;
                }

                // 2.全室内机模式
                if (tot_indhpOnly > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyIndoor;
                }
                // 3.全新风机模式
                else if (tot_FAhp > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

                    if (listRISelected.Count == 1)
                    {
                        Indoor inItem = listRISelected[0].IndoorItem;

                        #region 一对一新风机
                        //curSystemItem.SysType = SystemType.OnlyFreshAir;

                        //// 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                        //// 20160821 新增productType参数 by Yunxiao Lin
                        //inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType, inItem.Series);
                        //string UniqueOutdoorName = inItem.UniqueOutdoorName;

                        ////按照设定的model取一对一室外机需要判断品牌。 20161024 by Yunxiao Lin
                        ////同一个新风机可能被多个系列共用，因此可能存在多个一对一室外机，用“/”分隔 20170330 by Yunxiao Lin
                        //if (!UniqueOutdoorName.Contains("/"))
                        //{
                        //    if (thisProject.BrandCode == "Y")
                        //        curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(UniqueOutdoorName, series);
                        //    else
                        //        curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(UniqueOutdoorName, series);
                        //}
                        //else
                        //{
                        //    string[] UniqueOutdoorNameList = UniqueOutdoorName.Split(new char[] { '/' });
                        //    foreach (string model in UniqueOutdoorNameList)
                        //    {
                        //        if (thisProject.BrandCode == "Y")
                        //            curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(model, series);
                        //        else
                        //            curSystemItem.OutdoorItem = bll.GetHitachiItemBySeries(model, series);
                        //        if (curSystemItem.OutdoorItem != null)
                        //            break;
                        //    }
                        //}
                        //if (curSystemItem.OutdoorItem == null)
                        //{
                        //    // ERROR:数据库中的一对一室外机ModelName写错
                        //    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
                        //    return SelectOutdoorResult.Null;
                        //}
                        //else
                        //{
                        //    curSystemItem.MaxRatio = 1;
                        //    //Connection Ratio改为由HorsePower对比计算
                        //    curSystemItem.Ratio = inItem.Horsepower / curSystemItem.OutdoorItem.Horsepower;
                        //    // FreshAir时不需要估算容量,直接绑定室外机的标准值
                        //    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                        //    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                        //    //一对一新风机将室外机的Capacity直接绑定到RoomIndoor 20161114 by Yunxiao Lin
                        //    listRISelected[0].ActualCoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                        //    listRISelected[0].ActualHeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                        //    listRISelected[0].ActualSensibleHeat = 0d;

                        //    pipBll.SetPipingLimitation(curSystemItem);
                        //    // 一对一新风机选型成功！
                        //    return SelectOutdoorResult.OK;
                        //}
                        #endregion

                    }
                }// 全新风机 END
            }// 模式确定 END
            #endregion

            SelectOutdoorResult returnType = SelectOutdoorResult.OK;

            #region // 遍历室外机标准表逐个筛选
            //delete by axj 20170120
            //// update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
            //double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
            //室外机选型改为判断Series 20161031 by Yunxiao Lin
            DataRow[] rows = dtOutdoorStd.Select(" SubString(ModelFull,11,1)='" + curSystemItem.Power + "' and UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''", "Model asc");
            // 遍历选型过程 START 
            foreach (DataRow r in rows)
            {
                // 检查最大连接数 
                int maxIU = 0;
                double outstdcap_c = Convert.ToDouble(r["CoolCapacity"].ToString());
                //string productType = r["ProductType"].ToString();
                int.TryParse(r["MaxIU"].ToString(), out maxIU);
                if (maxIU < listRISelected.Count)
                {
                    if (tot_indcap_c <= outstdcap_c)
                        MSGList.Add(Msg.MSGLIST_MAX_IDU_NUMBER);
                    continue;
                }

                //检查IVX特殊连接 20170702 by Yunxiao Lin
                if (series.Contains("IVX"))
                {
                    string ODUmodel = r["Model_Hitachi"].ToString().Trim();
                    if (series.Contains("HVRNM2"))
                    {
                        #region
                        //SMZ IVX 校验组合是否有效
                        string IDUmodel = "";
                        int IDUmodelsCount = 0;
                        bool combinationErr = false;
                        foreach (RoomIndoor ri in listRISelected)
                        {
                            //获取IDU 型号名称和数量
                            if (IDUmodel == "")
                            {
                                IDUmodel = ri.IndoorItem.Model_Hitachi;
                                IDUmodelsCount++;
                            }
                            else if (IDUmodel != ri.IndoorItem.Model_Hitachi)
                            {
                                combinationErr = true;
                                break;
                            }
                            else
                                IDUmodelsCount++;
                        }
                        if (!combinationErr && IDUmodelsCount > 0)
                        {
                            //将IDU型号名称和数量组合成 "model x count" 字符串，与数据库中预设的组合进行比对 20170702 by Yunxiao Lin
                            string IDUmodels = IDUmodel + " x " + IDUmodelsCount.ToString();
                            IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                            if (!IVXCombll.existsCombination(ODUmodel, IDUmodels))
                            {
                                if (tot_indcap_c <= outstdcap_c)
                                    MSGList.Add(Msg.MSGLIST_IVX_COMBINATION);
                                continue;
                            }
                        }
                        else
                        {
                            if (tot_indcap_c <= outstdcap_c)
                                MSGList.Add(Msg.MSGLIST_IVX_INVALID);
                            continue;
                        }
                        #endregion
                    }
                    else if (series.Contains("H(R/Y)NM1Q"))
                    {
                        #region
                        //HAPQ IVX
                        double IVXoutstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                        curSystemItem.Ratio = Math.Round(tot_indhp / IVXoutstdhp, 3);
                        //根据室外温度和室外机型号进行检查 20170702 by Yunxiao Lin
                        if (curSystemItem.DBCooling <= -10)
                        {
                            switch (ODUmodel)
                            {
                                #region
                                case "RAS-3HRNM1Q":
                                    //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                    if (exists4way)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_3HRNM1Q_INFO1);
                                        continue;
                                    }
                                    //容量配比限制
                                    if (curSystemItem.Ratio < 0.85)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-3HRNM1Q", "85%"));
                                        continue;
                                    }
                                    else if (curSystemItem.Ratio > 1)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-3HRNM1Q", "100%"));
                                        continue;
                                    }
                                    break;
                                case "RAS-4HRNM1Q":
                                case "RAS-5HRNM1Q":
                                case "RAS-5HYNM1Q":
                                    //室内机数量限制
                                    if (listRISelected.Count > 4)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("RAS-5HYNM1Q", 4));
                                        continue;
                                    }
                                    else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("Four Way:RAS-5HYNM1Q", 3));
                                        continue;
                                    }
                                    //容量配比限制
                                    if (curSystemItem.Ratio < 0.9)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-5HYNM1Q", "90%"));
                                        continue;
                                    }
                                    else if (curSystemItem.Ratio > 1.12)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-5HYNM1Q", "112%"));
                                        continue;
                                    }
                                    break;
                                    #endregion
                            }
                        }
                        else
                        {
                            switch (ODUmodel)
                            {
                                #region
                                case "RAS-3HRNM1Q":
                                    //容量配比限制
                                    if (curSystemItem.Ratio < 0.85)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-3HRNM1Q", "85%"));
                                        continue;
                                    }
                                    else if (curSystemItem.Ratio > 1.2)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-3HRNM1Q", "120%"));
                                        continue;
                                    }
                                    //室内机最大匹数限制
                                    //1-for-1 室内机最大匹数3
                                    if (listRISelected.Count == 1)
                                    {
                                        if (max_indhp > 3)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-3HRNM1Q", "3P", "'1-for-1'"));
                                            continue;
                                        }
                                    }
                                    //1-for-N 室内机最大匹数2.3
                                    else if (listRISelected.Count > 1)
                                    {
                                        if (max_indhp > 2.3)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-3HRNM1Q", "3P", "'1-for-N'"));
                                            continue;
                                        }
                                    }
                                    break;
                                case "RAS-4HRNM1Q":
                                    //室内机数量限制
                                    if (listRISelected.Count > 5)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("RAS-4HRNM1Q", 5));
                                        continue;
                                    }
                                    else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("Four Way:RAS-4HRNM1Q", 3));
                                        continue;
                                    }
                                    //容量配比限制
                                    if (listRISelected.Count <= 4)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-4HRNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.35)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-4HRNM1Q", "135"));
                                            continue;
                                        }
                                    }
                                    else if (listRISelected.Count <= 5)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-4HRNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.2)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-4HRNM1Q", "120"));
                                            continue;
                                        }
                                    }
                                    //室内机最大匹数限制
                                    //1-for-1 室内机最大匹数4
                                    if (listRISelected.Count == 1)
                                    {
                                        if (max_indhp > 4)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-4HRNM1Q", "4P", "'1-for-1'"));
                                            continue;
                                        }
                                    }
                                    //1-for-N 室内机最大匹数2.5
                                    else if (listRISelected.Count > 1)
                                    {
                                        if (max_indhp > 2.5)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-4HRNM1Q", "2.5P", "'1-for-N'"));
                                            continue;
                                        }
                                    }
                                    break;
                                case "RAS-5HRNM1Q":
                                case "RAS-5HYNM1Q":
                                    //室内机数量限制
                                    if (listRISelected.Count > 5)
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("RAS-5HYNM1Q", 5));
                                        continue;
                                    }
                                    else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                    {
                                        if (tot_indcap_c <= outstdcap_c)
                                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO4("Four Way:RAS-5HYNM1Q", 3));
                                        continue;
                                    }
                                    //容量配比限制
                                    if (listRISelected.Count <= 4)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-5HYNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.30)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-5HYNM1Q", "130%"));
                                            continue;
                                        }
                                    }
                                    else if (listRISelected.Count <= 5)
                                    {
                                        if (curSystemItem.Ratio < 0.9)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO1("RAS-5HYNM1Q", "90%"));
                                            continue;
                                        }
                                        else if (curSystemItem.Ratio > 1.2)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2("RAS-5HYNM1Q", "120%"));
                                            continue;
                                        }
                                    }
                                    //室内机最大匹数限制
                                    //1-for-1 室内机最大匹数4
                                    if (listRISelected.Count == 1)
                                    {
                                        if (max_indhp > 4)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-5HYNM1Q", "4P", "'1-for-1'"));
                                            continue;
                                        }
                                    }
                                    //1-for-N 室内机最大匹数3
                                    else if (listRISelected.Count > 1)
                                    {
                                        if (max_indhp > 3)
                                        {
                                            if (tot_indcap_c <= outstdcap_c)
                                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO3("RAS-5HYNM1Q", "3P", "'1-for-N'"));
                                            continue;
                                        }
                                    }
                                    break;
                                    #endregion
                            }
                        }
                        #endregion
                    }
                }


                // 检查容量配比率（此处仅校验上限值）                
                outstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
                ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
                curSystemItem.RatioFA = ratioFA;

                if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
                {
                    //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                    if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        MSGList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                        continue;
                    }
                }

                if (curSystemItem.SysType == SystemType.OnlyIndoor)
                {
                    #region// 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                    if (curSystemItem.Ratio < 0.5)
                    {
                        //OutdoorModelFull = r["ModelFull"].ToString();
                        //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                        //break;
                        //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO1(curSystemItem.SelOutdoorType, "50%"));
                        continue;
                    }
                    if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, curSystemItem.MaxRatio * 100 + "%"));
                        continue;
                    }
                    else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                    {
                        // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                        string model_Hitachi = r["Model_Hitachi"].ToString();
                        if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                        {
                            if (curSystemItem.Ratio > 1.2)
                            {
                                MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, "120%"));
                                continue;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region// 多新风机或者混连模式，则配比率校验规则有变
                    if (curSystemItem.Ratio < 0.8)
                    {
                        OutdoorModelFull = r["ModelFull"].ToString();
                        ERRList.Add(Msg.OUTD_RATIO_Composite);
                        break;
                    }
                    if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, "100%"));
                        continue;
                    }
                    if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_GENERAL_INFO2(curSystemItem.SelOutdoorType, curSystemItem.MaxRatio * 100 + "%"));
                        continue;
                    }
                    if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
                    {
                        if (tot_indcap_c <= outstdcap_c)
                            MSGList.Add(Msg.MSGLIST_COMPOSITE_MODE);
                        continue; // add on 20160307 clh 
                    }
                    #endregion
                }
                // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
                if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
                {
                    ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                    continue;
                }
                /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
                 *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
                 *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                 *3. Total ratio of Hydro Free 0%~100% 
                 *4. Only Hydro Free is not allowed 
                 **/
                if (existsHydroFree)
                {
                    ratioNoHDIU = Math.Round(tot_noHDhp / outstdhp, 3);
                    ratioHD = Math.Round(tot_HDhp / outstdhp, 3);
                    if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                    {
                        MSGList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                        if (ratioNoHDIU < 0.5)
                        {
                            OutdoorModelFull = r["ModelFull"].ToString();
                            break;
                        }
                        else
                            continue;
                    }
                    else if (ratioHD < 0 || ratioHD > 1)
                    {
                        MSGList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                        continue;
                    }
                }

                Outdoor outItem = bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
                ////获取室外机修正系数 add by axj 20170111
                //double Total_IDU_Factor = bll.GetTotalIDUFactor(outItem.ModelFull, tot_indhp, false);
                ////比较室外机额定容量和室内机估算容量之和 20161114 by Yunxiao Lin
                //if (outstdcap_c * Total_IDU_Factor < tot_indcap_c)
                //    continue;
                //if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                //    continue;

                ////比较室外机额定容量与Actual Ratio的乘积和室内机额定容量之和 20170122 by Yunxiao Lin
                //if (curSystemItem.Ratio >= 1)
                //{
                //    if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
                //        continue;
                //}
                //else
                //{
                //    if (outstdcap_c < tot_indstdcap_c)
                //        continue;
                //}

                //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
                //以下调用CalOutdoorEstCapacity的参数actRatio全部改为Horsepower 20161110 by Yunxiao Lin
                if (!outItem.ProductType.Contains("Water Source"))
                    curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.DBCooling, inWB, false, curSystemItem);
                else
                    curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWCooling, inWB, false, curSystemItem);

                #region Cooling
                if (thisProject.IsCoolingModeEffective)
                {
                    //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                    if (curSystemItem.CoolingCapacity == 0)
                    {
                        ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                        return SelectOutdoorResult.NotMatch;
                    }

                    curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
                    if (curSystemItem.PipingLengthFactor == 0)
                    {
                        string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                        double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                        double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                        string msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                        JCMsg.ShowWarningOK(msg);
                        ERRList.Add(msg);
                        return SelectOutdoorResult.Null;
                    }

                    curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
                }
                #endregion
                //  Hitachi的Fresh Air 不需要比较HeatCapacity值
                #region Heating
                if (hasHeating)
                {

                    //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
                    if (!outItem.ProductType.Contains("Water Source"))
                        curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.WBHeating, inDB, true, curSystemItem);
                    else
                        curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWHeating, inDB, true, curSystemItem);

                    //Change the logic of part load data & TTL IDU CF calculation - EU on 20190122 by xyj
                    //Capacity at connection ratio < 100% at Outdoor ambient +7ºC
                    //if (thisProject.RegionCode.StartsWith("EU_") && curSystemItem.Ratio < 1 && curSystemItem.WBHeating < 7)
                    //{
                    //    SystemVRF copySystem = curSystemItem.DeepClone();
                    //    copySystem.Ratio = 1;
                    //    copySystem.WBHeating = 7;
                    //    double heatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, copySystem.WBHeating, inDB, true, copySystem);
                    //    curSystemItem.HeatingCapacity = heatingCapacity < curSystemItem.HeatingCapacity ? heatingCapacity : curSystemItem.HeatingCapacity;
                    //}

                    //水冷机不需要除霜修正
                    if (!outItem.ProductType.Contains("Water Source"))
                    {
                        //double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
                        //从数据库获取除霜修正系数 20180626 by Yunxiao Lin
                        double defrostingFactor = bll.GetODUDefrostFactor(curSystemItem.DBHeating, outItem.Series);
                        curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Heating");   //添加制热Factor
                        if (curSystemItem.PipingLengthFactor_H == 0)
                        {
                            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                            double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                            double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                            string msg = Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length);
                            JCMsg.ShowWarningOK(msg);
                            ERRList.Add(msg);
                            return SelectOutdoorResult.Null;
                        }
                        //curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
                        curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor_H * defrostingFactor;
                    }

                    //判断如果HeatingCapacity 为 弹出提醒 on 20180510 by xyj
                    if (curSystemItem.HeatingCapacity == 0)
                    {
                        ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                        return SelectOutdoorResult.NotMatch;
                    }
                }
                #endregion

                //海拔修正 add on 20160517 by Yunxiao Lin
                //注意某些机型可能无此限制? Wait check
                if (thisProject.EnableAltitudeCorrectionFactor)
                {
                    //从数据库获取海拔修正系数 20180626 by Yunxiao Lin
                    //double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
                    double acf = bll.GetODUAltitudeFactor(thisProject.Altitude, outItem.Series);
                    curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;

                    // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
                    // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
                    if (hasHeating)
                    {
                        curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
                    }
                }

                // 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
                if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
                {
                    if (curSystemItem.Ratio > 1)
                    {
                        if (!curSystemItem.AllowExceedRatio)
                        {
                            if (tot_indcap_c <= outstdcap_c)
                                MSGList.Add(Msg.MSGLIST_NOT_ALLOW_EXCEED_RATIO);
                            continue;
                        }
                    }
                }



                //计算室内机实际容量 add by Shen Junjie on 20181113
                CalacIndoorActualCapacity(curSystemItem, listRISelected, csMode, hasHeating, tot_indhp, tot_indcap_c, tot_indcap_h);

                //比较室内机和房间的需求是否符合
                bool roomchecked = true;
                foreach (RoomIndoor ri in listRISelected)
                {
                    string wType;
                    if (!CommonBLL.MeetRoomRequired(ri, thisProject, tolerance, thisProject.RoomIndoorList, out wType))
                        roomchecked = false;
                    if (!roomchecked)
                        break;
                }
                if (!roomchecked)
                {
                    if (tot_indcap_c <= outstdcap_c)
                        MSGList.Add(Msg.MSGLIST_CANT_REACH_DEMAND);
                    continue;
                }

                OutdoorModelFull = r["ModelFull"].ToString();
                break; // 找到合适的室外机即跳出循环

            }
            // 遍历自动选型 END
            #endregion

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机
                // 2-2.内机总冷量配置率为50%～130%；
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    //OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
                    //ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    //returnType = SelectOutdoorResult.NotMatch;
                    //暂时屏蔽50%~130%提示，改为显示没有合适的室外机 20170224 by Yunxiao Lin
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变

                // 1-2.新风机冷量与室外机的配置率不大于30%
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
                {
                    if (string.IsNullOrEmpty(OutdoorModelFull))
                        OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();

                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    returnType = SelectOutdoorResult.NotMatch;
                }

                // 1-3.内机总冷量配置率为80%～105%；
                if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
                {
                    OutdoorModelFull = rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
            *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
            *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
            *3. Total ratio of Hydro Free 0%~100% 
            *4. Only Hydro Free is not allowed 
            **/
            if (existsHydroFree)
            {
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过系统的30%    -- add by Shen Junjie on 2017/12/22
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by xyj on 2017/12/29 
            //  if (tot_HP_KPI_X4E / tot_indhp > 0.3)
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                returnType = SelectOutdoorResult.NotMatch;
            }

            if (!string.IsNullOrEmpty(OutdoorModelFull))
            {
                curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
                // updated by clh
                if (curSystemItem.OutdoorItem != null)
                {
                    pipBll.SetPipingLimitation(curSystemItem);
                    //如果按室外机选型得不到正确结果，使用按室内机选型再试一遍 20161130 by Yunxiao Lin
                    if (returnType != SelectOutdoorResult.OK)
                        return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
                    else
                        return returnType;
                }

            }
            if ((thisProject != null) && (thisProject.ProductType.Contains("Water Source")))
                ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
            else
                ERRList.Add(Msg.OUTD_NOTMATCH);
            //如果按室外机选型得不到正确结果，按室内机选型再试一遍 20161130 by Yunxiao Lin
            if (returnType != SelectOutdoorResult.OK)
                return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            return SelectOutdoorResult.Null;
        }

        /// 通用室内机手动选择匹配的室外机（包括非严格一对一的情况）  add on 20171212 by Lingjia Qiu
        /// 水机选型增加FlowRateLevel参数 20170216 by Yunxiao Lin
        /// <summary>
        /// 通用室内机手动选择匹配的室外机（包括非严格一对一的情况）
        /// </summary>
        public static SelectOutdoorResult DoSelectUniversalOduManual(SystemVRF curSystemItem, List<RoomIndoor> listRISelected, Project thisProject, string series, out List<string> ERRList)
        {
            ERRList = new List<string>();
            ERRList.Clear();    // 每次选型前清空错误记录表
            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }
            if (curSystemItem.OutdoorItem == null)
                return SelectOutdoorResult.Null;

            if (curSystemItem != null && curSystemItem.IDUFirst && !series.Contains("IVX"))
                return DoSelectOutdoorIDUFirstManual(curSystemItem, listRISelected, thisProject, out ERRList);
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode, thisProject.RegionCode);
            double tolerance = 0.05;

            //curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";
            //MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            //string productType = productTypeBll.GetProductTypeBySeries(thisProject.BrandCode, thisProject.SubRegionCode, series);
            CapacitySelectionMode csMode = GetCapacitySelectionMode(thisProject);
            //bool hasHeating = thisProject.IsHeatingModeEffective && !productType.Contains(", CO");
            bool isCoolingOnly = series.Contains(" CO,");
            bool hasHeating = thisProject.IsHeatingModeEffective && !isCoolingOnly;

            MyPipingBLL.PipingBLL pipBll = new MyPipingBLL.PipingBLL(thisProject);

            DataTable dtOutdoorStd = new DataTable();
            // 获取室外机标准表（初次加载或者更改室外机类型时）
            if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
            {
                dtOutdoorStd = bll.GetOutdoorListStd();
                if (dtOutdoorStd == null || dtOutdoorStd.Rows.Count == 0)
                {
                    // ERROR:室外机标准表无数据记录！
                    ERRList.Add(Msg.DB_NODATA);
                    return SelectOutdoorResult.Null;
                }
            }
            //add by axj 20170120 室内机工况温度
            double inWB = 0; //制冷湿球温度
            double inDB = 0; //制热干球温度

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double ratioFA = 0;
            double tot_indhpOnly = 0;
            double tot_indhp = 0;
            //判断室内机中是否含有4 way 机型 20170702 by Yunxiao Lin
            bool exists4way = false;
            //记录最大的室内机匹数 20170702 by Yunxiao Lin
            double max_indhp = 0;
            //判断室内机中是否含有Hydro Free 20171220 by Yunxiao Lin
            bool existsHydroFree = false;
            //计算Hydro Free总容量和非Hydro Free室内机的总容量 20171220 by Yunxiao Lin
            double tot_HDhp = 0;
            double tot_noHDhp = 0;
            double ratioNoHDIU = 0;
            double ratioHD = 0;
            double tot_HP_KPI_X4E = 0; // KPI-X4E (全热交换机)的总匹数
            double outstdhp = 0;
            foreach (RoomIndoor ri in listRISelected)
            {
                #region
                //增加4 way 室内机判断，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Type.Contains("Four Way"))
                    exists4way = true;

                //记录最大的室内机匹数，用于IVX选型 20170702 by Yunxiao lin
                if (ri.IndoorItem.Horsepower > max_indhp)
                    max_indhp = ri.IndoorItem.Horsepower;

                //增加Hydro Free室内机判断,并分别统计系统内Hydro Free和非Hydro Free室内机的容量 20171220 by Yunxiao Lin
                if (ri.IndoorItem.Type.Contains("Hydro Free"))
                {
                    existsHydroFree = true;
                    tot_HDhp += ri.IndoorItem.Horsepower;
                }
                else
                {
                    tot_noHDhp += ri.IndoorItem.Horsepower;
                }

                if (ri.IndoorItem.Type == "Total Heat Exchanger (KPI-X4E)")
                {
                    tot_HP_KPI_X4E += ri.IndoorItem.Horsepower;
                }

                tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                tot_indcap_h += ri.HeatingCapacity;
                tot_indhp += ri.IndoorItem.Horsepower;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    if (curSystemItem.SysType == SystemType.CompositeMode && (ri.IndoorItem.Model.Contains("1680") || ri.IndoorItem.Model.Contains("2100")))
                    {
                        // JCHVRF:混连模式下，1680与2100新风机取另一条记录
                        Indoor inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetFreshAirItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, true, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                        if (inItem != null)
                        {
                            //inItem.Series = series;
                            ri.SetIndoorItemWithAccessory(inItem);
                        }

                    }
                    tot_FAhp += ri.IndoorItem.Horsepower;

                }
                #endregion
            }
            tot_indhpOnly = tot_indhp - tot_FAhp;

            //获取室内机工况温度 add by Shen Junjie on 20181113
            GetIDUWorkTemperature(csMode, listRISelected, out inWB, out inDB);

            bool isComposite = (curSystemItem.SysType == SystemType.CompositeMode);
            #region //确定当前内机组合模式
            // 1.混连模式
            if (isComposite)
            {
                // 1-1.必须同时包含室内机与新风机；
                if (tot_indhpOnly == 0 || tot_FAhp == 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_Composite);
                    return SelectOutdoorResult.Null;
                }
            }
            else
            {
                // 2-1.不允许同时包含室内机和新风机；
                if (tot_indhpOnly > 0 && tot_FAhp > 0)
                {
                    ERRList.Add(Msg.OUTD_NOTMATCH_NoComposite);
                    return SelectOutdoorResult.Null;
                }

                // 2.全室内机模式
                if (tot_indhpOnly > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyIndoor;
                }
                // 3.全新风机模式
                else if (tot_FAhp > 0)
                {
                    curSystemItem.SysType = SystemType.OnlyFreshAirMulti;

                    if (listRISelected.Count == 1)
                    {
                        Indoor inItem = listRISelected[0].IndoorItem;

                        #region 一对一新风机
                        //curSystemItem.SysType = SystemType.OnlyFreshAir;

                        //// 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                        //// 20160821 新增productType参数 by Yunxiao Lin
                        //inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType, inItem.Series);
                        //string UniqueOutdoorName = inItem.UniqueOutdoorName;

                        //// if (curSystemItem.OutdoorItem.ModelFull == UniqueOutdoorName)  //按品牌选择Model Name on 2018/8/29
                        //if ((thisProject.BrandCode == "Y" ? curSystemItem.OutdoorItem.Model_York : curSystemItem.OutdoorItem.Model_Hitachi) == UniqueOutdoorName)
                        //{
                        //    curSystemItem.MaxRatio = 1;
                        //    //Connection Ratio改为由HorsePower对比计算
                        //    curSystemItem.Ratio = inItem.Horsepower / curSystemItem.OutdoorItem.Horsepower;
                        //    // FreshAir时不需要估算容量,直接绑定室外机的标准值
                        //    curSystemItem.CoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                        //    curSystemItem.HeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                        //    //一对一新风机将室外机的Capacity直接绑定到RoomIndoor 20161114 by Yunxiao Lin
                        //    listRISelected[0].ActualCoolingCapacity = curSystemItem.OutdoorItem.CoolingCapacity;
                        //    listRISelected[0].ActualHeatingCapacity = curSystemItem.OutdoorItem.HeatingCapacity;
                        //    listRISelected[0].ActualSensibleHeat = 0d;

                        //    pipBll.SetPipingLimitation(curSystemItem);
                        //    // 一对一新风机选型成功！
                        //    return SelectOutdoorResult.OK;
                        //}
                        //else
                        //{
                        //    // ERROR:数据库中的一对一室外机ModelName写错
                        //    ERRList.Add(Msg.OUTD_NOTMATCH_FA_Model + "  Region：" + thisProject.SubRegionCode + "  ModelName:" + UniqueOutdoorName);
                        //    return SelectOutdoorResult.Null;
                        //}
                        #endregion

                    }
                }// 全新风机 END
            }// 模式确定 END
            #endregion

            SelectOutdoorResult returnType = SelectOutdoorResult.OK;

            #region // 遍历室外机标准表逐个筛选

            // update on 20140821 clh: 放开室外机差值时，室内机19度20度的限制，仅允许Setting中统一修改温度值
            //double inWB = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double inDB = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;
            //室外机选型改为判断Series 20161031 by Yunxiao Lin
            //DataRow[] rows = dtOutdoorStd.Select("UnitType='" + curSystemItem.SelOutdoorType + "' and Series='" + series + "'" + " and TypeImage <> ''", "Model asc");
            // 遍历选型过程 START 
            //foreach (DataRow r in rows)
            //{
            // 检查最大连接数 
            //int maxIU = 0;
            //int.TryParse(r["MaxIU"].ToString(), out maxIU);
            if (curSystemItem.OutdoorItem.MaxIU < listRISelected.Count)
            {
                ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), curSystemItem.OutdoorItem.MaxIU.ToString()));//室内机数量超过室外机的最大连接数！
                return SelectOutdoorResult.NotMatch;
            }
            //continue;
            //检查IVX特殊连接 20170702 by Yunxiao Lin
            if (series.Contains("IVX"))
            {
                string ODUmodel = curSystemItem.OutdoorItem.Model_Hitachi;
                if (series.Contains("HVRNM2"))
                {
                    #region//SMZ IVX 校验组合是否有效
                    string IDUmodel = "";
                    int IDUmodelsCount = 0;
                    bool combinationErr = false;
                    foreach (RoomIndoor ri in listRISelected)
                    {
                        //获取IDU 型号名称和数量
                        if (IDUmodel == "")
                        {
                            IDUmodel = ri.IndoorItem.Model_Hitachi;
                            IDUmodelsCount++;
                        }
                        else if (IDUmodel != ri.IndoorItem.Model_Hitachi)
                        {
                            combinationErr = true;
                            break;
                        }
                        else
                            IDUmodelsCount++;
                    }
                    if (!combinationErr && IDUmodelsCount > 0)
                    {
                        //将IDU型号名称和数量组合成 "model x count" 字符串，与数据库中预设的组合进行比对 20170702 by Yunxiao Lin
                        string IDUmodels = IDUmodel + " x " + IDUmodelsCount.ToString();
                        IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                        if (!IVXCombll.existsCombination(ODUmodel, IDUmodels))
                        {
                            //ShowMessage: [ODUmodel]只能与以下Indoor搭配使用(IDU组合列表。。。建议用弹出窗口做)
                            string msg = Msg.OUTD_ODUMatchIDUList(ODUmodel) + "\r\n";
                            var list = IVXCombll.getCombination(ODUmodel).IDUModels;
                            foreach (string m in list)
                            {
                                msg = msg + m + "\r\n";
                            }
                            frmMsgInfo fm = new frmMsgInfo();
                            fm.ShowMsg(msg);
                            fm.StartPosition = FormStartPosition.CenterScreen;
                            fm.Show();
                            ERRList.Add(Msg.OUTD_NOTMATCH);
                            return SelectOutdoorResult.NotMatch;
                        }
                    }
                    else
                    {
                        //ShowMessage: [ODUmodel]只能与以下Indoor搭配使用(IDU组合列表。。。建议用弹出窗口做)
                        IVXCombinationBLL IVXCombll = new IVXCombinationBLL();
                        string msg = Msg.OUTD_ODUMatchIDUList(ODUmodel) + "\r\n";
                        var list = IVXCombll.getCombination(ODUmodel).IDUModels;
                        foreach (string m in list)
                        {
                            msg = msg + m + "\r\n";
                        }
                        frmMsgInfo fm = new frmMsgInfo();
                        fm.ShowMsg(msg);
                        fm.StartPosition = FormStartPosition.CenterScreen;
                        fm.Show();
                        ERRList.Add(Msg.OUTD_NOTMATCH);
                        return SelectOutdoorResult.NotMatch;
                    }
                    #endregion

                }
                else if (series.Contains("H(R/Y)NM1Q"))
                {
                    #region//HAPQ IVX
                    double IVXoutstdhp = curSystemItem.OutdoorItem.Horsepower;
                    curSystemItem.Ratio = Math.Round(tot_indhp / IVXoutstdhp, 3);
                    //根据室外温度和室外机型号进行检查 20170702 by Yunxiao Lin
                    if (curSystemItem.DBCooling <= -10)
                    {
                        switch (ODUmodel)
                        {
                            case "RAS-3HRNM1Q":
                                //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                if (exists4way)
                                {
                                    //ShowMessage: [ODUmodel]不能与Four way type 室内机搭配使用。
                                    ERRList.Add(Msg.OUTD_NotMatch_FourWayTypeIDU(ODUmodel)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (curSystemItem.Ratio < 0.85)
                                {
                                    //ShowMessage: 当环境温度小于等于-10℃时，[ODUmodel]与室内机的容量配比范围为85%~100%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_LessOrEqual(-10, ODUmodel, 85, 100)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (curSystemItem.Ratio > 1)
                                {
                                    //ShowMessage: 当环境温度小于-10℃时，[ODUmodel]与室内机的容量配比范围为85%-100%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_Less(-10, ODUmodel, 85, 100)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                break;
                            case "RAS-4HRNM1Q":
                            case "RAS-5HRNM1Q":
                            case "RAS-5HYNM1Q":
                                //室内机数量限制
                                if (listRISelected.Count > 4)
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "4"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "3"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (curSystemItem.Ratio < 0.9)
                                {
                                    //ShowMessage: 当环境温度小于等于-10℃时，[ODUmodel]与室内机的容量配比范围为90%-112%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_LessOrEqual(-10, ODUmodel, 90, 112));
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (curSystemItem.Ratio > 1.12)
                                {
                                    //ShowMessage: 当环境温度小于等于-10℃时，[ODUmodel]与室内机的容量配比范围为90%-112%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_LessOrEqual(-10, ODUmodel, 90, 112));
                                    return SelectOutdoorResult.NotMatch;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (ODUmodel)
                        {
                            case "RAS-3HRNM1Q":
                                //RAS-3HRNM1Q 不能与4-way IDU搭配使用
                                if (exists4way)
                                {
                                    //ShowMessage: [ODUmodel]不能与Four way type 室内机搭配使用。
                                    ERRList.Add(Msg.OUTD_NotMatch_FourWayTypeIDU(ODUmodel)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (curSystemItem.Ratio < 0.85)
                                {
                                    //ShowMessage: 当环境温度大于-10℃时，[ODUmodel]与室内机的容量配比范围为85%~100%
                                    ERRList.Add(Msg.OUTD_TemperatureCapacityRatio_More(-10, ODUmodel, 85, 100)); //add by axj 20170703
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (curSystemItem.Ratio > 1.2)
                                    return SelectOutdoorResult.NotMatch;
                                //室内机最大匹数限制
                                //1-for-1 室内机最大匹数3
                                if (listRISelected.Count == 1)
                                {
                                    if (max_indhp > 3)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过3HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 3)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //1-for-N 室内机最大匹数2.3
                                else if (listRISelected.Count > 1)
                                {
                                    if (max_indhp > 2.3)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-N连接，[ODUmodel]连接的室内机匹数不能超过2.3HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1ForN_More(-10, ODUmodel, 2.3)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                break;
                            case "RAS-4HRNM1Q":
                                //室内机数量限制
                                if (listRISelected.Count > 5)
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "5"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "3"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (listRISelected.Count <= 4)
                                {
                                    if (curSystemItem.Ratio < 0.9)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-135%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 135)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.35)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-135%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 135)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                else if (listRISelected.Count <= 5)
                                {
                                    if (curSystemItem.Ratio < 0.95)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.2)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //室内机最大匹数限制
                                //1-for-1 室内机最大匹数4
                                if (listRISelected.Count == 1)
                                {
                                    if (max_indhp > 4)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过4HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 4)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //1-for-N 室内机最大匹数2.5
                                else if (listRISelected.Count > 1)
                                {
                                    if (max_indhp > 2.5)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-N连接，[ODUmodel]连接的室内机匹数不能超过2.5HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1ForN_More(-10, ODUmodel, 2.5)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                break;
                            case "RAS-5HRNM1Q":
                            case "RAS-5HYNM1Q":
                                //室内机数量限制
                                if (listRISelected.Count > 5)
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "5"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                else if (exists4way && listRISelected.Count > 3) //当室内机中有Four Way机型时，室内机数量限制3
                                {
                                    ERRList.Add(Msg.OUTD_INDOORNUM_EXCEED(listRISelected.Count.ToString(), "3"));//室内机数量超过室外机的最大连接数！
                                    return SelectOutdoorResult.NotMatch;
                                }
                                //容量配比限制
                                if (listRISelected.Count <= 4)
                                {
                                    if (curSystemItem.Ratio < 0.9)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-130%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 130)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.30)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过4时，[ODUmodel]与室内机的容量配比范围为90%-130%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 4, ODUmodel, 90, 130)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                else if (listRISelected.Count <= 5)
                                {
                                    if (curSystemItem.Ratio < 0.95)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                    else if (curSystemItem.Ratio > 1.2)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃,连接的室内机数量不超过5时，[ODUmodel]与室内机的容量配比范围为95%-120%.
                                        ERRList.Add(Msg.OUTD_TemperatureCapacityRatioConnectionLimit_More(-10, 5, ODUmodel, 95, 120)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //室内机最大匹数限制
                                //1-for-1 室内机最大匹数4
                                if (listRISelected.Count == 1)
                                {
                                    if (max_indhp > 4)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过4HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 4)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                //1-for-N 室内机最大匹数3
                                else if (listRISelected.Count > 1)
                                {
                                    if (max_indhp > 3)
                                    {
                                        //ShowMessage: 当环境温度大于-10℃时，1-for-1连接，[ODUmodel]连接的室内机匹数不能超过3HP.
                                        ERRList.Add(Msg.OUTD_TemperatureConnection1For1_More(-10, ODUmodel, 3)); //add by axj 20170703
                                        return SelectOutdoorResult.NotMatch;
                                    }
                                }
                                break;
                        }
                    }
                    #endregion
                }
            }
            // 检查容量配比率（此处仅校验上限值）
            double outstdcap_c = curSystemItem.OutdoorItem.CoolingCapacity;
            outstdhp = curSystemItem.OutdoorItem.Horsepower;
            curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
            ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
            curSystemItem.RatioFA = ratioFA;

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    return SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机模式（2-2.室内机总冷量配置率为50%～130%；）
                if (curSystemItem.Ratio < 0.5)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    return SelectOutdoorResult.NotMatch;
                    //break;
                }
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    //continue;
                    return SelectOutdoorResult.NotMatch;
                }
                else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                {
                    // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                    string model_Hitachi = curSystemItem.OutdoorItem.Model_Hitachi;
                    if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                    {
                        if (curSystemItem.Ratio > 1.2)
                        {
                            ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, 1.2));
                            return SelectOutdoorResult.NotMatch;
                        }
                    }
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变
                if (curSystemItem.Ratio < 0.8)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    //break;
                    return SelectOutdoorResult.NotMatch;
                }
                if (curSystemItem.Ratio > 1)  // 1.05 改为1，201509 clh
                {  //continue;
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    return SelectOutdoorResult.NotMatch;
                }
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {   //continue;
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    return SelectOutdoorResult.NotMatch;
                }
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3) //混连模式新风制冷容量有30%限制 modify on 20160713 by Yunxiao Lin
                { // continue; // add on 20160307 clh 
                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    return SelectOutdoorResult.NotMatch;
                }
            }
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/22
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                return SelectOutdoorResult.NotMatch;
            }
            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
                 *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
                 *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                 *3. Total ratio of Hydro Free 0%~100% 
                 *4. Only Hydro Free is not allowed 
                 **/
            if (existsHydroFree)
            {
                ratioNoHDIU = Math.Round(tot_noHDhp / outstdhp, 3);
                ratioHD = Math.Round(tot_HDhp / outstdhp, 3);
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    if (ratioNoHDIU < 0.5)
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    return SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    return SelectOutdoorResult.NotMatch;
                }
            }

            Outdoor outItem = curSystemItem.OutdoorItem;//bll.GetOutdoorItemBySeries(r["ModelFull"].ToString(), series);
            ////获取室外机修正系数 add by axj 20170111
            //double Total_IDU_Factor = bll.GetTotalIDUFactor(outItem.ModelFull, tot_indhp, false);
            //if (Total_IDU_Factor == 0)
            //{
            //    ERRList.Add(Msg.WARNING_DATA_EXCEED);
            //    return SelectOutdoorResult.Null;
            //}
            ////比较室外机额定容量和室内机估算容量之和 20161114 by Yunxiao Lin
            //if (outstdcap_c * Total_IDU_Factor < tot_indcap_c)
            //{
            //    //continue;
            //    //RRList.Add(Msg.OUTD_NOTMATCH);//OUTD_COOLINGCAPACITY
            //    ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
            //    return SelectOutdoorResult.Null;
            //}
            ////比较室外机额定容量与Actual Ratio的乘积和室内机额定容量之和 20170122 by Yunxiao Lin
            //if (curSystemItem.Ratio >= 1)
            //{
            //    if (outstdcap_c * curSystemItem.Ratio < tot_indstdcap_c)
            //    {
            //        ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
            //        return SelectOutdoorResult.Null;
            //    }
            //}
            //else
            //{
            //    if (outstdcap_c < tot_indstdcap_c)
            //    {
            //        ERRList.Add(Msg.OUTD_RATED_CAP_CHK);
            //        return SelectOutdoorResult.Null;
            //    }
            //}


            //增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160718 by Yunxiao Lin
            //以下调用CalOutdoorEstCapacity的参数actRatio全部改为Horsepower 20161110 by Yunxiao Lin
            if (!outItem.ProductType.Contains("Water Source"))
                curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.DBCooling, inWB, false, curSystemItem);
            else
                curSystemItem.CoolingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWCooling, inWB, false, curSystemItem);

            if (thisProject.IsCoolingModeEffective)
            {
                //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                if (curSystemItem.CoolingCapacity == 0)
                {
                    ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                    return SelectOutdoorResult.NotMatch;
                }
                curSystemItem.PipingLengthFactor = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Cooling");
                if (curSystemItem.PipingLengthFactor == 0)
                {
                    string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                    double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                    return SelectOutdoorResult.Null;
                }
                curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * curSystemItem.PipingLengthFactor;
            }
            //  Hitachi的Fresh Air 不需要比较HeatCapacity值
            if (hasHeating)
            {
                //增加水冷机判断，水冷机参数不是室外温度，而是进水温度 20160615 by Yunxiao Lin
                if (!outItem.ProductType.Contains("Water Source"))
                    curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.WBHeating, inDB, true, curSystemItem);
                else
                    curSystemItem.HeatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, curSystemItem.IWHeating, inDB, true, curSystemItem);

                //Change the logic of part load data & TTL IDU CF calculation - EU on 20190122 by xyj
                // Capacity at connection ratio < 100% at Outdoor ambient +7ºC
                //if (thisProject.RegionCode.StartsWith("EU_") && curSystemItem.Ratio < 1 && curSystemItem.WBHeating < 7)
                //{
                //    SystemVRF copySystem = curSystemItem.DeepClone();
                //    copySystem.Ratio = 1;
                //    copySystem.WBHeating = 7;
                //    double heatingCapacity = bll.CalOutdoorEstCapacity(outItem, tot_indhp, copySystem.WBHeating, inDB, true, copySystem);
                //    curSystemItem.HeatingCapacity = heatingCapacity < curSystemItem.HeatingCapacity ? heatingCapacity : curSystemItem.HeatingCapacity;
                //}

                //水冷机不需要除霜修正
                if (!outItem.ProductType.Contains("Water Source"))
                {
                    //double defrostingFactor = Global.GetDefrostingfactor(curSystemItem.DBHeating);
                    //从数据库获取除霜修正系数 20180626 by Yunxiao Lin
                    double defrostingFactor = bll.GetODUDefrostFactor(curSystemItem.DBHeating, outItem.Series);
                    curSystemItem.PipingLengthFactor_H = pipBll.GetPipeLengthFactor(curSystemItem, outItem, "Heating");   //添加制热Factor
                    if (curSystemItem.PipingLengthFactor_H == 0)
                    {
                        string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                        double len = Unit.ConvertToControl(curSystemItem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                        double diff = Unit.ConvertToControl(curSystemItem.HeightDiff, UnitType.LENGTH_M, ut_length);
                        JCMsg.ShowWarningOK(Msg.PIPING_LENGTHFACTOR(curSystemItem.Name, len.ToString("n2") + ut_length, Math.Abs(diff).ToString("n2") + ut_length));

                        return SelectOutdoorResult.Null;
                    }
                    //curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor * defrostingFactor;
                    curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * curSystemItem.PipingLengthFactor_H * defrostingFactor;
                }
                //判断如果CoolingCapacity 为 弹出提醒 on 20180510 by xyj
                if (curSystemItem.HeatingCapacity == 0)
                {
                    ERRList.Add(Msg.ODU_TEMPERATURE_REVISE);
                    return SelectOutdoorResult.NotMatch;
                }
            }

            //海拔修正 add on 20160517 by Yunxiao Lin
            //注意某些机型可能无此限制? Wait check
            if (thisProject.EnableAltitudeCorrectionFactor)
            {
                //从数据库获取海拔修正系数 20180626 by Yunxiao Lin
                //double acf = getAltitudeCorrectionFactor(thisProject.Altitude);
                double acf = bll.GetODUAltitudeFactor(thisProject.Altitude, outItem.Series);
                curSystemItem.CoolingCapacity = curSystemItem.CoolingCapacity * acf;

                // 由于一个Project中可能存在多个不同的ProductType，因此单凭thisProject.IsHeatingModeEffective无法断定当前系统是否需要制热功能。
                // 还需要判断当前系统室外机的productType，如果productType包含", CO"，该系统肯定不需要制热功能。 20160826 by Yunxiao Lin
                if (hasHeating)
                {
                    curSystemItem.HeatingCapacity = curSystemItem.HeatingCapacity * acf;
                }
            }

            //// 注意混连的新风机与一对一的同Model新风机，Capacity数值不相等
            //if (curSystemItem.SysType == SystemType.CompositeMode || curSystemItem.SysType == SystemType.OnlyFreshAirMulti)
            //{
            //    if (curSystemItem.Ratio > 1)
            //    {
            //        if (!curSystemItem.AllowExceedRatio)
            //            continue;
            //    }
            //}

            //计算室内机实际容量 add by Shen Junjie on 20181113
            CalacIndoorActualCapacity(curSystemItem, listRISelected, csMode, hasHeating, tot_indhp, tot_indcap_c, tot_indcap_h);

            //比较室内机和房间的需求是否符合
            bool roomchecked = true;
            foreach (RoomIndoor ri in listRISelected)
            {
                string wType;
                if (!CommonBLL.MeetRoomRequired(ri, thisProject, tolerance, thisProject.RoomIndoorList, out wType))
                    roomchecked = false;
                if (!roomchecked)
                    break;
            }
            if (!roomchecked)
            {  //continue;
                ERRList.Add(Msg.OUTD_NOTMATCH);//"IND_WARN_CAPActCoolLower IND_WARN_CAPActHeatLower");
                returnType = SelectOutdoorResult.NotMatch;
            }

            OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
            //break; // 找到合适的室外机即跳出循环

            //}
            // 遍历自动选型 END
            #endregion

            if  (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
            {
                //台湾的IVX的ratio是100%~130% add by Shen Junjie on 2018/8/7
                if (curSystemItem.Ratio < 1 || curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(1, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            if (curSystemItem.SysType == SystemType.OnlyIndoor)
            {
                // 全室内机
                // 2-2.内机总冷量配置率为50%～130%；
                if (curSystemItem.Ratio > curSystemItem.MaxRatio)
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, curSystemItem.MaxRatio));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (series.Contains("FS(V/Y)N1Q") || series.Contains("JDOH-Q"))
                {
                    // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                    string model_Hitachi = curSystemItem.OutdoorItem.Model_Hitachi;
                    if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                    {
                        if (curSystemItem.Ratio > 1.2)
                        {
                            OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull;
                            ERRList.Add(Msg.OUTD_RATIO_AllIndoor(0.5, 1.2));
                            returnType = SelectOutdoorResult.NotMatch;
                        }
                    }
                }
            }
            else
            {
                // 多新风机或者混连模式，则配比率校验规则有变

                // 1-2.新风机冷量与室外机的配置率不大于30%
                if (curSystemItem.SysType == SystemType.CompositeMode && ratioFA > 0.3)
                {
                    if (string.IsNullOrEmpty(OutdoorModelFull))
                        OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();

                    ERRList.Add(Msg.OUTD_RATIO_CompositeFA);
                    returnType = SelectOutdoorResult.NotMatch;
                }

                // 1-3.内机总冷量配置率为80%～105%；
                if (curSystemItem.Ratio > 1) //1.05 改为1，201509 clh
                {
                    OutdoorModelFull = curSystemItem.OutdoorItem.ModelFull; //rows[rows.Length - 1]["ModelFull"].ToString();
                    ERRList.Add(Msg.OUTD_RATIO_Composite);
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            /*Hydro Free 系列的容量配比率有特殊限制 20171220 by Yunxiao Lin
            *1. Total ratio Hydro Free + IU(DX) Heat Recovery 50%~150%, Heat Pump 50%~130% (由于total IDU CF 数据不全，该限制暂时不判断)
            *2. Total ratio of IU(DX) when Hydro Free is installed 50%~130%
            *3. Total ratio of Hydro Free 0%~100% 
            *4. Only Hydro Free is not allowed 
            **/
            if (existsHydroFree)
            {
                if (ratioNoHDIU < 0.5 || ratioNoHDIU > 1.3) //Total ratio of IU(DX) when Hydro Free is installed 50%~130%
                {
                    ERRList.Add(Msg.HydroFree_OtherIndoorRatio_LessOrMore(50, 130));
                    returnType = SelectOutdoorResult.NotMatch;
                }
                else if (ratioHD < 0 || ratioHD > 1)
                {
                    ERRList.Add(Msg.HydroFree_HydroFreeRatio_LessOrMore(0, 100));
                    returnType = SelectOutdoorResult.NotMatch;
                }
            }

            // KPI-X4E (全热交换机)的总匹数不能超过系统的30%    -- add by Shen Junjie on 2017/12/22
            // KPI-X4E (全热交换机)的总匹数不能超过ODU的30%    -- add by XYJ on 2017/12/29
            if (outstdhp > 0 && tot_HP_KPI_X4E / outstdhp > 0.3)
            {
                ERRList.Add(Msg.KPI_X4E_HP_MAX_RATIO_LIMITATION(30));
                returnType = SelectOutdoorResult.NotMatch;
            }

            if (!string.IsNullOrEmpty(OutdoorModelFull))
            {
                //curSystemItem.OutdoorItem = bll.GetOutdoorItemBySeries(OutdoorModelFull, series);
                // updated by clh
                if (curSystemItem.OutdoorItem != null)
                {
                    pipBll.SetPipingLimitation(curSystemItem);
                    //如果按室外机选型得不到正确结果，使用按室内机选型再试一遍 20161130 by Yunxiao Lin
                    if (returnType != SelectOutdoorResult.OK)
                        return DoSelectOutdoorIDUFirstManual(curSystemItem, listRISelected, thisProject, out ERRList);
                    else
                        return returnType;
                }

            }
            if ((thisProject != null) && (thisProject.ProductType.Contains("Water Source")))
                ERRList.Add(Msg.OUTD_NOTMATCH_WATER);
            else
                ERRList.Add(Msg.OUTD_NOTMATCH);
            ////如果按室外机选型得不到正确结果，按室内机选型再试一遍 20161130 by Yunxiao Lin
            //if (returnType != SelectOutdoorResult.OK)
            //    return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            return SelectOutdoorResult.Null;
        }

        #endregion

    }
}
