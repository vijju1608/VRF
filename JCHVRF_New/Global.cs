﻿using System;
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

namespace JCHVRF_New
{
   public class Global
    {
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
            curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            JCHVRF.MyPipingBLL.PipingBLL pipBll = new JCHVRF.MyPipingBLL.PipingBLL(thisProject);

            // 若所选室内机数量为0，则终止选型
            if (listRISelected == null || listRISelected.Count == 0)
            {
                return SelectOutdoorResult.Null;
            }

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            string productType = listRISelected[0].IndoorItem.ProductType;
            string series = listRISelected[0].IndoorItem.Series;
            if (string.IsNullOrEmpty(series))
            {
                series = curSystemItem.Series;
            }

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
            double inWB = 9999;//取最小值
            double inDB = 0;//取最大值

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double tot_indstdcap_c = 0;
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
                tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
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
            tot_indhpOnly = tot_indhp - tot_FAhp;

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
                        if (!string.IsNullOrEmpty(series) && series.Contains("FSNP") || series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1"))
                        {
                            isSMZ = true;
                        }
                        if (!isSMZ)
                        {
                            curSystemItem.SysType = SystemType.OnlyFreshAir;

                            // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                            // 20160821 新增productType参数 by Yunxiao Lin
                            inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType, inItem.Series);
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
                        tot_indstdcap_c = 0;
                        ratioFA = 0;
                        tot_indhpOnly = 0;
                        tot_indhp = 0;
                        foreach (RoomIndoor ri in listRISelected_temp)
                        {
                            tot_indcap_c += ri.CoolingCapacity; // 包含FA的冷量
                            tot_indcap_h += ri.HeatingCapacity;
                            tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
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

                    //检查IVX特殊连接 20170702 by Yunxiao Lin
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
                            }
                            else
                            {
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
                            }

                        }
                    }

                    // 检查容量配比率（此处仅校验上限值）
                    double outstdcap_c = Convert.ToDouble(r["CoolCapacity"].ToString());
                    outstdhp = Convert.ToDouble(r["Horsepower"].ToString());
                    curSystemItem.Ratio = Math.Round(tot_indhp / outstdhp, 3);
                    ratioFA = Math.Round(tot_FAhp / outstdhp, 3);
                    curSystemItem.RatioFA = ratioFA;

                    if (thisProject.SubRegionCode == "TW" && series.StartsWith("IVX,"))
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
                            // S/F mini 6HP 室外机 Connection Ratio 上限只有 120% 20180505 by Yunxiao Lin
                            string model_Hitachi = r["Model_Hitachi"].ToString();
                            if (model_Hitachi == "RAS-6FSVN1Q" || model_Hitachi == "RAS-6FSYN1Q")
                            {
                                if (curSystemItem.Ratio > 1.2)
                                {
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
                    if (thisProject.IsHeatingModeEffective && !outItem.ProductType.Contains(", CO"))
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
                        if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
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
                    foreach (RoomIndoor ri in listRISelected_temp)
                    {
                        double indhp = ri.IndoorItem.Horsepower;

                        ri.ActualCoolingCapacity = ActualCoolingCapacity * indhp / tot_indhp;
                        // 计算实际显热
                        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.SHF;
                        //if (ri.SHF_Mode == "High")
                        //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                        //else if (ri.SHF_Mode == "Medium")
                        //{
                        //    //如果Med=0,则取Hi的值，下同
                        //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Med;
                        //    if (ri.ActualSensibleHeat == 0)
                        //        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                        //}
                        //else
                        //{
                        //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Lo;
                        //    if (ri.ActualSensibleHeat == 0)
                        //        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                        //}
                        if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
                        {
                            //非单冷模式或单冷系列需要分配Heating Capacity
                            ri.ActualHeatingCapacity = ActualHeatingCapacity * indhp / tot_indhp;
                        }
                    }
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
                                    if (!ri.IndoorItem.ProductType.Contains(", CO"))
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

            //从室内机属性中获取productType变量 add on 20160823 by Yunxiao Lin
            string productType = listRISelected[0].IndoorItem.ProductType;
            string series = listRISelected[0].IndoorItem.Series;

            if (curSystemItem != null && curSystemItem.IDUFirst && !series.Contains("IVX"))
                return DoSelectOutdoorIDUFirst(curSystemItem, listRISelected, thisProject, out ERRList);
            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            double tolerance = 0.05;

            if (curSystemItem.OutdoorItem != null)
            {
                //记住series以便自动重新选型时传入Series  add by Shen Junjie on 2018/3/29
                curSystemItem.Series = curSystemItem.OutdoorItem.Series;
            }
            curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            JCHVRF.MyPipingBLL.PipingBLL pipBll = new JCHVRF.MyPipingBLL.PipingBLL(thisProject);

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
            double inWB = 9999;//取最小值
            double inDB = 0;//取最大值

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double tot_indstdcap_c = 0;
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
                tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
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
            tot_indhpOnly = tot_indhp - tot_FAhp;

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
                        if (!string.IsNullOrEmpty(series) && series.Contains("FSNP") || series.Contains("FSNS") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTOR-BS1"))
                        {
                            isSMZ = true;
                        }
                        if (!isSMZ)
                        {
                            curSystemItem.SysType = SystemType.OnlyFreshAir;
                            // 此处重新获取室内机对象，因为对于旧项目，UniqueOutdoorName发生了更改！！
                            // 20160821 新增productType参数 by Yunxiao Lin
                            inItem = (new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetItem(inItem.ModelFull, inItem.Type, inItem.ProductType, inItem.Series);
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
                    }
                    else if (series.Contains("H(R/Y)NM1Q"))
                    {
                        //HAPQ IVX
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
                if (thisProject.IsHeatingModeEffective && !outItem.ProductType.Contains(", CO"))
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
                    if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
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
                    curSystemItem.HeatingCapacity = tot_indcap_h;   //赋值给系统 on 2018-01-05 by xyj
                }

                // 将修正后的室外机容量按匹数比例分配给室内机
                foreach (RoomIndoor ri in listRISelected)
                {
                    double indhp = ri.IndoorItem.Horsepower;

                    ri.ActualCoolingCapacity = ActualCoolingCapacity * indhp / tot_indhp;
                    // 计算实际显热
                    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.SHF;
                    //if(ri.SHF_Mode == "High")
                    //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                    //else if (ri.SHF_Mode == "Medium")
                    //{
                    //    //如果Med=0,则取Hi的值，下同
                    //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Med;
                    //    if (ri.ActualSensibleHeat == 0)
                    //        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                    //}
                    //else
                    //{
                    //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Lo;
                    //    if (ri.ActualSensibleHeat == 0)
                    //        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                    //}
                    if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
                    {
                        //非单冷模式或单冷系列需要分配Heating Capacity
                        ri.ActualHeatingCapacity = ActualHeatingCapacity * indhp / tot_indhp;
                    }

                }
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
            curSystemItem.OutdoorItem = null;
            string OutdoorModelFull = "";

            JCHVRF.MyPipingBLL.PipingBLL pipBll = new JCHVRF.MyPipingBLL.PipingBLL(thisProject);

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
            double inWB = 9999;//取最小值
            double inDB = 0;//取最大值

            // 计算此时的室内机容量和（新风机与室内机分开汇总）
            double tot_indcap_c = 0;
            double tot_indcap_h = 0;
            double tot_FAhp = 0;
            double tot_indstdcap_c = 0;
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
                tot_indstdcap_c += ri.IndoorItem.CoolingCapacity;
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
            tot_indhpOnly = tot_indhp - tot_FAhp;

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
                string productType = r["ProductType"].ToString();
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
                    }
                    else if (series.Contains("H(R/Y)NM1Q"))
                    {
                        //HAPQ IVX
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
                if (thisProject.IsHeatingModeEffective && !outItem.ProductType.Contains(", CO"))
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
                    if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
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
                    double indhp = ri.IndoorItem.Horsepower;

                    ri.ActualCoolingCapacity = ActualCoolingCapacity * indhp / tot_indhp;
                    // 计算实际显热
                    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.SHF;
                    //if(ri.SHF_Mode == "High")
                    //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                    //else if (ri.SHF_Mode == "Medium")
                    //{
                    //    //如果Med=0,则取Hi的值，下同
                    //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Med;
                    //    if (ri.ActualSensibleHeat == 0)
                    //        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                    //}
                    //else
                    //{
                    //    ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Lo;
                    //    if (ri.ActualSensibleHeat == 0)
                    //        ri.ActualSensibleHeat = ri.ActualCoolingCapacity * ri.IndoorItem.SHF_Hi;
                    //}
                    if (thisProject.IsHeatingModeEffective && !productType.Contains(", CO"))
                    {
                        //非单冷模式或单冷系列需要分配Heating Capacity
                        ri.ActualHeatingCapacity = ActualHeatingCapacity * indhp / tot_indhp;
                    }

                }
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

       
    }
}