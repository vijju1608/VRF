using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCBase.UI;
using JCHVRF.Model;
using JCBase.Utility;
using CDF;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Windows.Forms;
using JCHVRF.Const;
using System.Data;
using System.Runtime.CompilerServices;
using YAMIntegration;
using Microsoft.VisualBasic.CompilerServices;
//Start Backward Compatibility : Added references required to implement backward compatibilty code.
using AutoMapper;
using pt = System.Windows;
using NextGenModel = JCHVRF.Model.NextGen;
//End Backward Compatibility : Added references required to implement backward compatibilty code.
namespace JCHVRF.BLL
{
    public class CommonBLL
    {
        #region 判断当前长度单位是否为公英制

        //// 判断当前长度单位是否为英制
        ///// <summary>
        ///// 判断当前长度单位是否为英制,
        ///// </summary>
        ///// <returns></returns>
        //public static bool IsLength_ft()
        //{
        //    return SystemSetting.UserSetting.unitsSetting.settingLENGTH == Unit.ut_Size_ft;
        //}

        /// 判断当前管径单位是否为英制
        /// <summary>
        /// 判断当前管径单位是否为英制
        /// </summary>
        /// <returns></returns>
        public static bool IsDimension_inch()
        {
            return SystemSetting.UserSetting.unitsSetting.settingDimension == Unit.ut_Dimension_inch;
        }
        #endregion

        #region 报告相关

        /// <summary>
        /// 报告中是否显示价格信息，ver1.20版本暂放开出口区域的价格
        /// </summary>
        /// <returns></returns>
        public static bool ShowPrice()
        {
            CDL.SVRFreg sv = new CDL.SVRFreg();
            sv = CDL.Sec.GetSVRF();
            //if ((!IsDearler && IsPriceValid) || IsSuperUser)
            if (sv.PriceValid)
                return true;
            return false;
        }

        #endregion


        #region 判断当前界面语言
        // 判断当前界面的语言是否是中文
        /// <summary>
        /// 判断当前界面的语言是否是中文
        /// </summary>
        /// <returns></returns>
        public static bool IsChinese()
        {
            return LangType.CurrentLanguage == LangType.CHINESE;
        }

        // 判断当前界面的语言是否是英文
        /// <summary>
        /// 判断当前界面的语言是否是英文
        /// </summary>
        /// <returns></returns>
        public static bool IsEnglish()
        {
            return LangType.CurrentLanguage == LangType.ENGLISH;
        }


        // 判断当前界面的语言是否是法文
        /// <summary>
        /// 判断当前界面的语言是否是法文
        /// </summary>
        /// <returns></returns>
        public static bool IsFrance()
        {
            return LangType.CurrentLanguage == LangType.FRENCH;
        }


        // 判断当前界面的语言是否是西班牙
        /// <summary>
        /// 判断当前界面的语言是否是西班牙
        /// </summary>
        /// <returns></returns>
        public static bool IsSpain()
        {
            return LangType.CurrentLanguage == LangType.TURKISH;
        }


        // 判断当前界面的语言是否是土耳其
        /// <summary>
        /// 判断当前界面的语言是否是土耳其
        /// </summary>
        /// <returns></returns>
        public static bool IsTurkey()
        {
            return LangType.CurrentLanguage == LangType.TURKISH;
        }


        #endregion


        /// <summary>
        /// 检验房间内的室内机是否完全满足房间需求 --室外机选型后调用
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="thisProject"></param>
        /// <returns></returns>
        public static bool FullMeetRoomRequired(RoomIndoor ri, Project thisProject)
        {
            string wType;
            return MeetRoomRequired(ri, thisProject, 0, thisProject.RoomIndoorList, out wType);
        }

        /// <summary>
        /// 检验房间内的室内机是否满足有公差的房间需求 --室外机选型后调用
        /// </summary>
        /// <param name="ri"></param>
        /// <param name="thisProject"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool MeetRoomRequired(RoomIndoor ri, Project thisProject, double tolerance, List<RoomIndoor> RoomIndoorList, out string wType)
        {
            wType = "";
            if (ri.IndoorItem == null || thisProject == null)
                return false;
            if (!ri.IsFreshAirArea) //新风区域只有新风量需求，没有制冷，制热和显热需求，所以新风区域不需要比较
            {
                Room r = null;
                if (thisProject.FloorList != null)
                {
                    foreach (Floor floor in thisProject.FloorList)
                    {
                        if (floor.RoomList != null)
                        {
                            foreach (Room r_temp in floor.RoomList)
                            {
                                if (r_temp.Id == ri.RoomID)
                                {
                                    r = r_temp;
                                    break;
                                }
                            }
                        }
                        if (r != null)
                            break;
                    }
                }

                if (r == null)
                {
                    if (!ri.IsAuto) //手动非房间选型不需要检验需求
                        return true;
                }
                double coolcap = 0;
                double heatcap = 0;
                double sh = 0;
                double rqcoolcap = ri.RqCoolingCapacity;
                double rqheatcap = ri.RqHeatingCapacity;
                double rqsh = ri.RqSensibleHeat;
                foreach (RoomIndoor ri_temp in RoomIndoorList)
                {
                    if (ri_temp.IsFreshAirArea == ri.IsFreshAirArea && ri_temp.RoomID == ri.RoomID)
                    {
                        coolcap += ri_temp.ActualCoolingCapacity;
                        heatcap += ri_temp.ActualHeatingCapacity;
                        sh += ri_temp.ActualSensibleHeat;
                    }
                }
                //校验制冷容量
                if (thisProject.IsCoolingModeEffective)
                {
                    if (coolcap < rqcoolcap * (1 - tolerance))
                    {
                        wType = "reqCool";
                        return false;
                    }
                }
                //校验制热容量
                if (thisProject.IsHeatingModeEffective && !ri.IndoorItem.ProductType.Contains(", CO"))
                {
                    if (heatcap < rqheatcap * (1 - tolerance))
                    {
                        wType = "reqHeat";
                        return false;
                    }
                }
                //校验显热
                if (r != null && !r.IsSensibleHeatEnable)
                    return true;
                if (sh < rqsh * (1 - tolerance))
                {
                    wType = "sensible";
                    return false;
                }
            }
            return true;
        }

        public static Project OpenVRFProject(int projectId)
        {
            byte[] PorjectBlob = null;              //   Project global class

            //从Project.mdb数据库获取 project 序列化对象
            PorjectBlob = CDF.CDF.GetProjectBlob(projectId, 1, 1);
            if (PorjectBlob == null) return null;

            // 此次需要补充， 保存global VRF project 数据到全局静态类
            // 1. Readd global project class from Blob1 byte[] 
            MemoryStream stream1 = new MemoryStream(PorjectBlob);
            BinaryFormatter bf = new BinaryFormatter();
            Project importProj;
            //thisProject = (Project)bf.Deserialize(stream1);
            importProj = (Project)bf.Deserialize(stream1);
            stream1.Dispose();
            stream1.Close();

            
            //获取项目名称  on 20180910 by xyj
            string projectName = CDF.CDF.GetProjectName(projectId, 1);
            //如果当前的项目名称不等于数据中得到的名称 则变更当前项目名称
            if (!string.IsNullOrEmpty(projectName) && importProj.Name != projectName)
            {
                importProj.Name = projectName;
            }

            importProj.projectID = projectId;

            
            //Start Backward Compatibility : Conditions to check if project needs to be imported from backward compatibilty code.
            if ((importProj.SystemListNextGen == null || importProj.SystemListNextGen.Count < 1) && importProj.SystemList!= null && importProj.SystemList.Count > 0)
            {

                if (CommonBLL.CompareVersion(importProj.Version, "2.0.0") < 0)
                {
                    JCMsg.ShowWarningOK(VRFMessage.Msg.UNMAINTAINABLE_PROJECT_WARNING);
                    return importProj;
                }
                CheckOldSeriesChanged(importProj);

                if (!ProjectBLL.IsSupportedUniversalSelection(importProj))
                {
                    //A_SA部分旧HAPQ室内机UnitType变更，需要对旧项目做兼容处理。20190122 by Yunxiao Lin
                    compatSAOldIDUType(importProj);
                    //将ANZ的项目父region改为OCEANIA,因为之后可能还有其他操作需要读取该父region，所以需要先执行。 20170612 by Yunxiao Lin
                    compatOldProject2230(importProj);
                    //修改TW FSNS系列的Type名称 20181212 by Yunxiao Lin
                    compatTWFSNSType(importProj);
                    //修改巴西FSNS7B/5B系列的Type名称和Series名称 20190110 by Yunxiao Lin
                    compatBrazilFSNSTypeAndSeries(importProj);
                    //导入项目后，需要做兼容性处理 20160707 Yunxiao Lin
                    compatOldProject2120(importProj);
                    //旧项目兼容V1.3.0处理 20161020 by Yunxiao Lin
                    compatOldProject2130(importProj);
                    //LA_MMA旧项目中RPI-8FSNQH和RPI-10FSNQH改为RPI-8.0FSNQH和RPI-10.0FSNQH 20190318 by Yunxiao Lin
                    compatOldProject_RPI_8_10_FSNQH(importProj);

                    //兼容性处理 判断系统功率 add by axj 20160125
                    if (importProj != null && importProj.SystemList != null)
                    {
                        foreach (var sys in importProj.SystemList)
                        {
                            if (sys.Power == null || sys.Power == "")
                            {
                                if (sys.OutdoorItem != null)
                                {
                                    sys.Power = sys.OutdoorItem.ModelFull.Substring(10, 1);
                                }
                            }
                        }
                    }
                }
                if (ProjectBLL.IsSupportedUniversalSelection(importProj))
                {
                    compatOldProjectODU(importProj);
                }


                //if (importProj.BrandCode != Project.BrandCode)
                //{
                //    JCHMessageBox.Show(Msg.GetResourceString("PEOJECT_BRAND_DIFFERENT"));
                //    return;
                //}
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<JCHVRF.Model.SystemVRF, NextGenModel.SystemVRF>().ForMember(x => x.MyPipingPlottingScaleNodeTemp, opt => opt.Ignore())
                    .ForPath(x => x.MyPipingNodeOutTemp.pipeColor, opt => opt.Ignore())
                    .ForPath(x => x.MyPipingNodeOutTemp.textColor, opt => opt.Ignore())
                    .ForPath(x => x.MyPipingNodeOutTemp.branchKitColor, opt => opt.Ignore())
                    .ForPath(x => x.MyPipingNodeOutTemp.nodeBgcolor, opt => opt.Ignore())
                    ;
                });

                IMapper mapper = new Mapper(config);

                List<NextGenModel.SystemVRF> sysListNxtGen = new List<NextGenModel.SystemVRF>();
                foreach (var item in importProj.SystemList)
                {
                    sysListNxtGen.Add(mapper.Map<NextGenModel.SystemVRF>(item));
                }

                importProj.SystemListNextGen = sysListNxtGen;
                importProj.HeatExchangerSystems = new List<SystemHeatExchanger>();
                importProj.ControlSystemList = new List<ControlSystem>();

                for (int i = 0; i < sysListNxtGen.Count; i++)
                {
                    sysListNxtGen[i].HvacSystemType = "1";
                    new CommonBLL().CreateTempNodeOutRecursivelyForBWC(importProj.SystemListNextGen[i], importProj.SystemList[i].MyPipingNodeOutTemp);
                }

                //start mapping roomlist
                importProj.RoomList = new List<Room>();
                foreach (var floor in importProj.FloorList)
                {
                    foreach (var room in floor.RoomList)
                    {
                        importProj.RoomList.Add(room);   
                    }
                    
                }
                //end mapping roomlist

                //start room-indoor mapping
                foreach (var indoor in importProj.RoomIndoorList)
                {
                    indoor.SelectedRoom = importProj?.RoomList?.FirstOrDefault(room => room.Id == indoor.RoomID);
                }
                //end room-indoor mapping


                //start floor-indoor mapping
                int floorID = 0;
                bool isFloorIdUpdate = false;
                foreach (Floor floor in importProj.FloorList)
                {
                    isFloorIdUpdate = false;
                    foreach (var indoor in importProj.RoomIndoorList)
                    {
                        foreach (Room rm in floor.RoomList)
                        {
                            if (rm.Id == indoor.RoomID)
                            {
                                indoor.SelectedFloor = floor;
                                indoor.SelectedFloor.Id = floor.Id = Convert.ToString(floorID);
                                isFloorIdUpdate = true;
                                break;
                            }
                        }
                    }
                    if(!isFloorIdUpdate)
                    {
                        floor.Id = Convert.ToString(floorID);
                    }
                    floorID++;
                }
                //end floor-indoor mapping

                //start map default default design conditions to project
                if(importProj.DesignCondition == null)
                {
                    importProj.DesignCondition = GetDefaultDesignCondition();
                }
                //end map default default design conditions to project

                //start mapping system product category and product type
                OutdoorBLL bll = new OutdoorBLL(importProj.SubRegionCode, importProj.BrandCode);
                using (DataTable dtProductCategory = bll.GetOutdoorListStd())
                {
                    if (dtProductCategory != null && dtProductCategory.Rows.Count > 0)
                    {
                        foreach (var system in importProj.SystemListNextGen)
                        {
                            if (system.OutdoorItem != null && system.OutdoorItem.ProductType != null)
                            {
                                var row = dtProductCategory.AsEnumerable()?.FirstOrDefault(x => x.Field<string>("Expr1003").Equals(system.OutdoorItem.ProductType, StringComparison.OrdinalIgnoreCase));
                                if (row != null)
                                {
                                    system.ProductCategory = Convert.ToString(row["ProductCategory"]);
                                }
                                system.ProductType = system.OutdoorItem.ProductType;
                            }
                        }
                    }
                }
                bll = null;
                //end mapping system product category and product type
                
            }
            //End Backward Compatibility : Conditions to check if project needs to be imported from backward compatibilty code.

            return importProj;
        }


        #region Backward Compatibility Helper Methods
        private static JCHVRF.Model.New.DesignCondition GetDefaultDesignCondition()
        {
            JCHVRF.Model.DefaultSettingModel designConditionsLegacy = SystemSetting.UserSetting.defaultSetting;
            JCHVRF.Model.New.DesignCondition designconditions = new JCHVRF.Model.New.DesignCondition();
            designconditions.indoorCoolingDB = Convert.ToDecimal(designConditionsLegacy.IndoorCoolingDB);
            designconditions.indoorCoolingWB = Convert.ToDecimal(designConditionsLegacy.IndoorCoolingWB);
            designconditions.indoorCoolingRH = Convert.ToDecimal(designConditionsLegacy.IndoorCoolingRH);
            designconditions.indoorCoolingHDB = Convert.ToDecimal(designConditionsLegacy.IndoorHeatingDB);
            designconditions.outdoorCoolingDB = Convert.ToDecimal(designConditionsLegacy.OutdoorCoolingDB);
            designconditions.outdoorCoolingIW = Convert.ToDecimal(designConditionsLegacy.OutdoorCoolingIW);
            designconditions.outdoorHeatingDB = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingDB);
            designconditions.outdoorHeatingWB = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingWB);
            designconditions.outdoorHeatingRH = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingRH);
            designconditions.outdoorHeatingIW = Convert.ToDecimal(designConditionsLegacy.OutdoorHeatingIW);
            return designconditions;
        }

        private NextGenModel.tmpMyNode CreateTempNodeOutRecursivelyForBWC(NextGenModel.SystemVRF sysItem, JCHVRF.Model.tmpMyNode node)
        {
            if (node == null) return null;

            NextGenModel.tmpMyNode tempNode = null;
            if (node is JCHVRF.Model.tmpMyNodeOut)
            {
                JCHVRF.Model.tmpMyNodeOut nodeOut = node as JCHVRF.Model.tmpMyNodeOut;
                NextGenModel.tmpMyNodeOut tempOut = new NextGenModel.tmpMyNodeOut();

                tempOut.Location = new System.Windows.Point(nodeOut.Location.X, nodeOut.Location.Y);// nodeOut.Location;
                tempOut.PipeLengthes = nodeOut.PipeLengthes;
                tempOut.FillColor = nodeOut.FillColor;
                tempOut.TextColor = nodeOut.TextColor;
                sysItem.MyPipingNodeOutTemp = tempOut;

                tempOut.ChildNode = CreateTempNodeOutRecursivelyForBWC(sysItem, nodeOut.ChildNode);

                tempNode = tempOut;
            }
            else if (node is JCHVRF.Model.tmpMyNodeYP)
            {
                JCHVRF.Model.tmpMyNodeYP nodeYP = node as JCHVRF.Model.tmpMyNodeYP;
                NextGenModel.tmpMyNodeYP tempYP = new NextGenModel.tmpMyNodeYP(nodeYP.IsCP);
                tempYP.Location = new System.Windows.Point(nodeYP.Location.X, nodeYP.Location.Y);// nodeYP.Location;

                tempYP.FillColor = node.FillColor;
                tempYP.TextColor = node.TextColor;
                SaveNodeYPValue(tempYP, nodeYP);

                for (int i = 0; i < nodeYP.MaxCount; i++)
                {
                    tempYP.AddChildNode(CreateTempNodeOutRecursivelyForBWC(sysItem, nodeYP.ChildNodes[i]));
                }
                tempNode = tempYP;
            }
            else if (node is JCHVRF.Model.tmpMyNodeCH)
            {
                JCHVRF.Model.tmpMyNodeCH nodeCH = node as JCHVRF.Model.tmpMyNodeCH;
                NextGenModel.tmpMyNodeCH tempCH = new NextGenModel.tmpMyNodeCH(null);
                tempCH.Location = new System.Windows.Point(nodeCH.Location.X, nodeCH.Location.Y);// nodeCH.Location;
                tempCH.FillColor = node.FillColor;
                tempCH.TextColor = node.TextColor;

                SaveNodeCHValue(nodeCH, tempCH);

                tempCH.ChildNode = CreateTempNodeOutRecursivelyForBWC(sysItem, nodeCH.ChildNode);
                tempNode = tempCH;
            }
            else if (node is JCHVRF.Model.tmpMyNodeMultiCH)
            {
                JCHVRF.Model.tmpMyNodeMultiCH nodeMCH = node as JCHVRF.Model.tmpMyNodeMultiCH;
                NextGenModel.tmpMyNodeMultiCH tempMCH = new NextGenModel.tmpMyNodeMultiCH();
                tempMCH.Location = new System.Windows.Point(nodeMCH.Location.X, nodeMCH.Location.Y); //nodeMCH.Location;
                tempMCH.FillColor = node.FillColor;
                tempMCH.TextColor = node.TextColor;

                SaveNodeMultiCHValue(nodeMCH, tempMCH);

                for (int i = 0; i < nodeMCH.ChildNodes.Count; i++)
                {
                    tempMCH.ChildNodes.Add(CreateTempNodeOutRecursivelyForBWC(sysItem, nodeMCH.ChildNodes[i]));
                }
                tempNode = tempMCH;
            }
            else if (node is JCHVRF.Model.tmpMyNodeIn)
            {
                JCHVRF.Model.tmpMyNodeIn nodeIn = node as JCHVRF.Model.tmpMyNodeIn;
                JCHVRF.Model.RoomIndoor riItem = nodeIn.RoomIndoorItem;
                NextGenModel.tmpMyNodeIn tempIn = new NextGenModel.tmpMyNodeIn(riItem);
                tempIn.Location = new System.Windows.Point(nodeIn.Location.X, nodeIn.Location.Y); //new Point(nodeIn.Location.X, nodeIn.Location.Y);
                tempIn.FillColor = node.FillColor;
                tempIn.TextColor = node.TextColor;

                tempNode = tempIn;
            }

            if (tempNode == null) return null;

            //tempNode.TextColor = node.Foreground;
            //tempNode.FillColor = node.FillColor;    // // To be Fix latter
            tempNode.Location = new System.Windows.Point(node.Location.X, node.Location.Y); //node.Location;
            tempNode.FillColor = node.FillColor;
            tempNode.TextColor = node.TextColor;
            if (node is JCHVRF.Model.tmpMyNode)
            {
                JCHVRF.Model.tmpMyNode myNode = node as JCHVRF.Model.tmpMyNode;
                int inlinkCount = myNode.MyInLinks.Count();
                tempNode.MyInLinks = new NextGenModel.tmpMyLink[inlinkCount];
                for (int i = 0; i < inlinkCount; i++)
                {
                    NextGenModel.tmpMyLink tmpLink = new NextGenModel.tmpMyLink();
                    SaveLinkValue(tmpLink, myNode.MyInLinks[i]);
                    tempNode.MyInLinks[i] = tmpLink;
                }
            }

            return tempNode;
        }

        /// <summary>
        /// 保存 NodeYP 对象中绑定的数据
        /// </summary>
        /// <param name="tmpNodeYP"></param>
        /// <param name="yp"></param>
        private void SaveNodeYPValue(NextGenModel.tmpMyNodeYP tmpNodeYP, JCHVRF.Model.tmpMyNodeYP yp)
        {
            tmpNodeYP.CoolingCapacity = yp.CoolingCapacity;
            tmpNodeYP.HeatingCapacity = yp.HeatingCapacity;
            tmpNodeYP.Model = yp.Model;
            tmpNodeYP.PriceG = yp.PriceG;
            //add on 20160429 by Yunxiao Lin.
            tmpNodeYP.IsCoolingonly = yp.IsCoolingonly;
        }

        private void SaveNodeCHValue(JCHVRF.Model.tmpMyNodeCH ch, NextGenModel.tmpMyNodeCH tmpch)
        {
            tmpch.CoolingCapacity = ch.CoolingCapacity;
            tmpch.HeatingCapacity = ch.HeatingCapacity;
            tmpch.Model = ch.Model;
            tmpch.MaxTotalCHIndoorPipeLength = ch.MaxTotalCHIndoorPipeLength;
            tmpch.MaxTotalCHIndoorPipeLength_MaxIU = ch.MaxTotalCHIndoorPipeLength_MaxIU;
            tmpch.MaxIndoorCount = ch.MaxIndoorCount;
            tmpch.ActualTotalCHIndoorPipeLength = ch.ActualTotalCHIndoorPipeLength;
            tmpch.PowerSupply = ch.PowerSupply;//add by Shen Junjie on 2017/12/22
            tmpch.PowerLineType = ch.PowerLineType;//add by Shen Junjie on 2017/12/22 
            tmpch.HeightDiff = ch.HeightDiff; //add by xyj on 20180620 
            tmpch.PowerCurrent = ch.PowerCurrent;//add by Shen Junjie on 2018/6/15
            tmpch.PowerConsumption = ch.PowerConsumption;//add by Shen Junjie on 2018/6/15 
        }

        private void SaveNodeMultiCHValue(JCHVRF.Model.tmpMyNodeMultiCH mch, NextGenModel.tmpMyNodeMultiCH tmpmch)
        {
            tmpmch.CoolingCapacity = mch.CoolingCapacity;
            tmpmch.HeatingCapacity = mch.HeatingCapacity;
            tmpmch.Model = mch.Model;
            tmpmch.MaxTotalCHIndoorPipeLength = mch.MaxTotalCHIndoorPipeLength;
            tmpmch.MaxTotalCHIndoorPipeLength_MaxIU = mch.MaxTotalCHIndoorPipeLength_MaxIU;
            tmpmch.MaxIndoorCount = mch.MaxIndoorCount;
            tmpmch.ActualTotalCHIndoorPipeLength = mch.ActualTotalCHIndoorPipeLength;

            tmpmch.PowerSupply = mch.PowerSupply;
            tmpmch.PowerConsumption = mch.PowerConsumption;
            tmpmch.PowerCurrent = mch.PowerCurrent;
            tmpmch.MaxBranches = mch.MaxBranches;
            tmpmch.MaxCapacityPerBranch = mch.MaxCapacityPerBranch;
            tmpmch.MaxIUPerBranch = mch.MaxIUPerBranch;
            tmpmch.PowerLineType = mch.PowerLineType;
            tmpmch.HeightDiff = mch.HeightDiff; //add by xyj on 20180620
        }

        /// <summary>
        /// 保存 Link 对象绑定的数据
        /// </summary>
        /// <param name="tmpLink"></param>
        /// <param name="myLink"></param>
        private void SaveLinkValue(NextGenModel.tmpMyLink tmpLink, JCHVRF.Model.tmpMyLink myLink)
        {
            tmpLink.ElbowQty = myLink.ElbowQty;
            tmpLink.OilTrapQty = myLink.OilTrapQty;
            tmpLink.Length = myLink.Length;

            tmpLink.SpecG_h = myLink.SpecG_h;
            tmpLink.SpecG_l = myLink.SpecG_l;
            tmpLink.SpecL = myLink.SpecL;
            tmpLink.ValveLength = myLink.ValveLength;
            tmpLink.SpecG_h_Normal = myLink.SpecG_h_Normal;
            tmpLink.SpecG_l_Normal = myLink.SpecG_l_Normal;
            tmpLink.SpecL_Normal = myLink.SpecL_Normal;

            //tmpLink.Line = myLink.Line;
            tmpLink.Style = (Lassalle.WPF.Flow.LineStyle)((int)myLink.Style);
            if (myLink.Points != null)
            {
                List<pt.Point> points = new List<pt.Point>();
                foreach (System.Drawing.PointF pt in myLink.Points)
                {
                    points.Add(new pt.Point(pt.X, pt.Y));
                }
                tmpLink.Points = points;
            }
            // tmpLink.Stroke = myLink.Stroke;   // // To be Fix latter
        }

        private static void CheckOldSeriesChanged(Project thisProject)
        {

            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.Series == "Residential VRF HP, 3 phase, FSNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini S - FSNME (3 phase)";
                }
                else if (sysItem.Series == "Residential VRF HR, FSXNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini L - FSXNME (HR)";
                }
                else if (sysItem.Series == "Residential VRF HP, FSXNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini L - FSXNME (HP)";
                }
                else if (sysItem.Series == "Residential VRF HP, 1 phase, FSVNME")
                {
                    sysItem.OutdoorItem.Series = "S/F mini S - FSVNME (1 phase)";
                }
            }
        }

        /// <summary>
        /// 对4.2.1及以前版本中A_SA HAPQ 部分IDU的unit type进行兼容处理
        /// </summary>
        private static void compatSAOldIDUType(Project thisProject)
        {
            if (thisProject == null || thisProject.RoomIndoorList == null) return;
            if (thisProject.SubRegionCode == "A_SA" && CompareVersion(thisProject.Version, "4.2.1") < 0)
            {
                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    Indoor indoor = ri.IndoorItem;
                    if (indoor != null && indoor.Series != "Commercial VRF CO, CNCQ")
                    {
                        if (indoor.Type == "Compact Ducted" && indoor.Model_Hitachi.EndsWith("FSN1Q"))
                            indoor.Type = "Compact Ducted (FSN1Q)";
                        else if (indoor.Type == "High Static Ducted" && indoor.Model_Hitachi.EndsWith("FSNQH"))
                            indoor.Type = "High Static Ducted (FSNQH)";
                        else if (indoor.Type == "Medium Static Ducted" && indoor.Model_Hitachi.EndsWith("FSNQH"))
                            indoor.Type = "Medium Static Ducted (FSNQH)";
                        else if (indoor.Type == "Low Static Ducted" && indoor.Model_Hitachi.EndsWith("FSNQL"))
                            indoor.Type = "Low Static Ducted (FSNQL)";
                        else if (indoor.Type == "Four Way Cassette" && indoor.Model_Hitachi.EndsWith("FSN1Q"))
                            indoor.Type = "Four Way Cassette (FSN1Q)";
                    }
                }
            }
        }

        private static void compatOldProject2230(Project thisProject)
        {
            //ANZ的父region从2.3.0开始改为OCEANIA
            string region = thisProject.RegionCode;
            string sub_region = thisProject.SubRegionCode;
            if (region == "ASEAN" && sub_region == "ANZ")
            {
                thisProject.RegionCode = "OCEANIA";
            }
        }

        /// <summary>
        /// 对V4.1.0及之前版本中的TW FSNS系列的UnitType及系列名称做兼容处理 20181212 by Yunxiao Lin
        /// </summary>
        private static void compatTWFSNSType(Project thisProject)
        {
            if (thisProject == null || thisProject.SystemList == null || thisProject.RoomIndoorList == null) return;

            if (thisProject.SubRegionCode == "TW" && CompareVersion(thisProject.Version, "4.2.0") < 0)
            {
                foreach (SystemVRF sysItem in thisProject.SystemList)
                {
                    Outdoor outdoor = sysItem.OutdoorItem;
                    if (outdoor != null && outdoor.Type == "FSNS (Top discharge)")
                    {
                        sysItem.SelOutdoorType = outdoor.Type = "FSNS(TW) (Top discharge)";
                        sysItem.Series = outdoor.Series = "Commercial VRF HP, FSNS(TW)";
                    }
                }

                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    Indoor indoor = ri.IndoorItem;
                    if (indoor != null && indoor.Series == "Commercial VRF HP, FSNS")
                    {
                        indoor.Series = "Commercial VRF HP, FSNS(TW)";
                    }
                }
            }
        }

        /// <summary>
        /// 对V4.2.0及之前版本中的TW FSNS7B/5B系列的UnitType及系列名称做兼容处理 20181212 by Yunxiao Lin
        /// </summary>
        private static void compatBrazilFSNSTypeAndSeries(Project thisProject)
        {
            if (thisProject == null || thisProject.SystemList == null || thisProject.RoomIndoorList == null) return;

            if (thisProject.SubRegionCode == "LA_BR")
            {
                if (CompareVersion(thisProject.Version, "4.2.1") < 0)
                {
                    #region
                    if (thisProject.SystemList != null)
                    {
                        foreach (SystemVRF sysItem in thisProject.SystemList)
                        {
                            if (sysItem != null)
                            {
                                Outdoor outdoor = sysItem.OutdoorItem;
                                if (outdoor != null)
                                {
                                    if (outdoor.Type == "FSNS7B (Top discharge)")
                                        sysItem.SelOutdoorType = outdoor.Type = "Comm. 380V VRF HP, FSNS7B";
                                    if (outdoor.Type == "FSNS5B (Top discharge)")
                                        sysItem.SelOutdoorType = outdoor.Type = "Comm. 220V VRF HP, FSNS5B";
                                    if (outdoor.Series == "Commercial VRF HP, FSNS7B")
                                        sysItem.Series = outdoor.Series = "Comm. 380V VRF HP, FSNS7B";
                                    if (outdoor.Series == "Commercial VRF HP, FSNS5B")
                                        sysItem.Series = outdoor.Series = "Comm. 220V VRF HP, FSNS5B";
                                    if (outdoor.Series == "Commercial VRF CO, FSNS7B")
                                        sysItem.Series = outdoor.Series = "Comm. 380V VRF CO, FSNS7B";
                                    if (outdoor.Series == "Commercial VRF CO, FSNS5B")
                                        sysItem.Series = outdoor.Series = "Comm. 220V VRF CO, FSNS5B";
                                }
                            }
                        }
                    }
                    #endregion
                    #region
                    if (thisProject.RoomIndoorList != null)
                    {
                        foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                        {
                            if (ri.IndoorItem != null)
                            {
                                if (ri.IndoorItem.Series == "Commercial VRF HP, FSNS7B")
                                    ri.IndoorItem.Series = "Comm. 380V VRF HP, FSNS7B";
                                if (ri.IndoorItem.Series == "Commercial VRF HP, FSNS5B")
                                    ri.IndoorItem.Series = "Comm. 220V VRF HP, FSNS5B";
                                if (ri.IndoorItem.Series == "Commercial VRF CO, FSNS7B")
                                    ri.IndoorItem.Series = "Comm. 380V VRF CO, FSNS7B";
                                if (ri.IndoorItem.Series == "Commercial VRF CO, FSNS5B")
                                    ri.IndoorItem.Series = "Comm. 220V VRF CO, FSNS5B";
                            }
                        }
                    }
                    #endregion
                }
                #region
                // old data FSNS5B (54-72HP) Calibration data on 20190619 by xyj
                if (thisProject.SystemList != null)
                {
                    foreach (SystemVRF sysItem in thisProject.SystemList)
                    {
                        if (sysItem.OutdoorItem.Series == "Comm. 220V VRF HP, FSNS5B" && sysItem.OutdoorItem.AuxModelName.Contains("FSNS7B"))
                        {
                            sysItem.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItemBySeries(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.Series);
                        }
                    }
                }
                #endregion
            }
        }

        /// 导入旧项目时，更新旧项目到1.2.0 add on 20160707 by Yunxiao Lin
        /// <summary>
        /// 导入旧项目时，更新旧项目到1.2.0
        /// </summary>
        private static void compatOldProject2120(Project thisProject)
        {

            if (thisProject != null && thisProject.RoomIndoorList != null)
            {
                foreach (RoomIndoor roomItem in thisProject.RoomIndoorList)
                {
                    if (roomItem != null && roomItem.IndoorItem != null && roomItem.IndoorItem.Type.Trim() == "High Wall")
                    {
                        string model = roomItem.IndoorItem.ModelFull;
                        if (string.IsNullOrEmpty(model))
                        {
                            model = "";
                        }
                        model = model.Trim();
                        if (model.Length > 2)
                        {
                            string strCode = model.Substring(model.Length - 1, 1);
                            if (strCode == "B")
                                roomItem.IndoorItem.Type = "High Wall (w/o EXV)";
                        }
                    }
                    AccessoryBLL.CompatType(roomItem);
                }
            }
        }
        /// 打开旧项目时，更新旧项目到1.3.0 add on 20161020 by Yunxiao Lin
        /// <summary>
        /// 打开旧项目时，更新旧项目到1.3.0
        /// </summary>
        /// 
        //bool isImportProject;
        private static void compatOldProject2130(Project thisProject)
        {
            if (thisProject == null || thisProject.SystemList == null) return;

            //兼容没有控制器的老项目默认值为true
            thisProject.CentralControllerOK = true;

            if (CompareVersion(thisProject.Version, "1.3.0") < 0)
            {
                for (int i = thisProject.SystemList.Count - 1; i >= 0; i--)
                {
                    SystemVRF sys = thisProject.SystemList[i];
                    if (sys.OutdoorItem == null) continue;
                    string model = sys.OutdoorItem.ModelFull;
                    if (string.IsNullOrEmpty(model) || model.Length < 4) continue;
                    string factoryCode = model.Substring(model.Length - 1, 1);
                    string modelCode = model.Substring(0, 4);
                    if (factoryCode == "S" && (modelCode == "JUOH" || modelCode == "JVOH")) //FSXN(JVOH)、FSXNH(JUOH)型号的第4位从"H"变为"U"
                    {
                        model = model.Substring(0, 3) + "U" + model.Substring(4, model.Length - 4);
                        sys.OutdoorItem.ModelFull = model;
                        if (modelCode == "JVOH") //FSXN的Partload表名从ODU_PartLoad_S02_RAS_FSXN1改为ODU_PartLoad_S04_RAS_FSXN 20161021 by Yunxiao Lin
                            sys.OutdoorItem.PartLoadTableName = "ODU_PartLoad_S04_RAS_FSXN";
                    }
                }
                for (int j = thisProject.RoomIndoorList.Count - 1; j >= 0; j--)
                {
                    RoomIndoor ri = thisProject.RoomIndoorList[j];
                    //如果版本小于1.3.0，Indoor需要设置ActualValue,并增加SHF系数 20161121 by Yunxiao Lin
                    ri.ActualCoolingCapacity = 0;
                    ri.ActualHeatingCapacity = 0;
                    ri.ActualSensibleHeat = 0;
                    ri.FanSpeedLevel = -1;//ri.SHF_Mode = "High"; 
                }
            }

            //导入老项目会影响选型，做出提示
            //add by Shen Junjie on 2018/2/11
            if (CompareVersion(thisProject.Version, "2.0.0") < 0)
            {
                //JCMsg.ShowWarningOK(Msg.UNMAINTAINABLE_PROJECT_WARNING);
            }
            //if (JCMsg.ShowConfirmOKCancel(Msg.GetResourceString("PEOJECT_IMPORT_MSG")) == DialogResult.Cancel)
            //{
            //    isImportProject = false;
            //    return;
            //}
            //else
            //    isImportProject = true;

            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            IndoorBLL indbll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            //ProjectBLL projBll = new ProjectBLL(thisProject);
            Outdoor outdoor = null;
            Indoor indoor = null;
            //数据库中已经不存在的室外机和室内机的提示内容 add by Shen Junjie on 2018/2/13
            List<string> unmaintainableOutdoorsAndIndoors = new List<string>();
            for (int i = thisProject.SystemList.Count - 1; i >= 0; i--)
            {
                SystemVRF sys = thisProject.SystemList[i];
                if (sys.OutdoorItem == null) continue;
                outdoor = bll.GetOutdoorItem(sys.OutdoorItem.ModelFull, sys.OutdoorItem.ProductType);

                //如果没有此型号的室外机，或者此型号为无效状态，则变更为属性相同的有效型号
                if (outdoor == null)
                {
                    outdoor = bll.GetOutdoorItem(sys.OutdoorItem.ProductType, sys.OutdoorItem.Series, sys.OutdoorItem.Model_Hitachi);
                    if (outdoor != null)
                    {
                        sys.OutdoorItem = outdoor;
                        sys.Power = outdoor.ModelFull.Substring(10, 1);
                    }
                }

                if (outdoor == null)
                {
                    //室外机型号不存在，则提示 add by Shen Junjie on 2018/2/11
                    sys.Unmaintainable = true;
                    string sysModel = sys.OutdoorItem.Model_York;
                    if (string.IsNullOrEmpty(sysModel) || sysModel.Trim() == "-")
                    {
                        sysModel = sys.OutdoorItem.Model_Hitachi;
                    }
                    unmaintainableOutdoorsAndIndoors.Add(string.Format("{0}[{1}]", sys.Name, sysModel));
                    continue;
                }
                sys.Unmaintainable = false;
                //增加Series 20161028 by Yunxiao Lin
                sys.OutdoorItem.Series = outdoor.Series;
                //增加室外机马力值 20161121 by Yunxiao Lin
                sys.OutdoorItem.Horsepower = outdoor.Horsepower;

                for (int j = thisProject.RoomIndoorList.Count - 1; j >= 0; j--)
                {
                    RoomIndoor ri = thisProject.RoomIndoorList[j];
                    if (ri == null || ri.IndoorItem == null || ri.SystemID != sys.Id) continue;

                    indoor = indbll.GetItem(ri.IndoorItem.ModelFull, ri.IndoorItem.Type, ri.IndoorItem.ProductType, ri.IndoorItem.Series);
                    if (indoor == null)
                    {
                        //室内机型号不存在，则锁定系统 add by Shen Junjie on 2018/2/11
                        sys.Unmaintainable = true;
                        string indModel = ri.IndoorItem.Model_York;
                        if (string.IsNullOrEmpty(indModel) || indModel.Trim() == "-")
                        {
                            indModel = ri.IndoorItem.Model_Hitachi;
                        }
                        unmaintainableOutdoorsAndIndoors.Add(string.Format("{0}[{1}]", ri.IndoorName, indModel));
                        continue;
                    }

                    if (ri.IndoorItem.Type != "Fresh Air")
                    {
                        //indoor.ListAccessory = ri.IndoorItem.ListAccessory;  //防止室内机的accessory丢失 add by Shen Junjie on 2018/4/9
                        ri.SetIndoorItem(indoor); //维持原来的Accessory，所以只要替换IndoorItem add by Shen Junjie on 2018/4/28
                        ri.IndoorItem.Series = outdoor.Series;
                    }
                    //else
                    //{
                    //    //如果查不到相应型号的室内机或新风机，删除室内机 20161121 by Yunxiao Lin
                    //    projBll.RemoveRoomIndoor(ri.IndoorNO);
                    //    continue;
                    //}
                }
            }
            //if (unmaintainableOutdoorsAndIndoors.Count > 0)
            //{
            //    JCH.ShowWarningOK(Msg.UNMAINTAINABLE_ODU_IDU_WARNING(
            //        string.Join(",", unmaintainableOutdoorsAndIndoors.ToArray())));
            //}
        }

        /// <summary>
        /// 打开旧项目，将LA_MMA中的RPI-8FSNQH和RPI-10FSNQH改为RPI-8.0FSNQH和RPI-10.0FSNQH (V4.5.0)
        /// </summary>
        private static void compatOldProject_RPI_8_10_FSNQH(Project thisProject)
        {
            if (thisProject == null || thisProject.RoomIndoorList == null)
                return;
            if (CompareVersion(thisProject.Version, "4.5.0") < 0) //V4.5.0
            {
                if (thisProject.SubRegionCode == "LA_MMA")
                {
                    foreach (RoomIndoor roomItem in thisProject.RoomIndoorList)
                    {
                        if (roomItem != null && roomItem.IndoorItem != null)
                        {
                            switch (roomItem.IndoorItem.Model_Hitachi)
                            {
                                case "RPI-8FSNQH":
                                    roomItem.IndoorItem.Model_Hitachi = "RPI-8.0FSNQH"; break;
                                case "RPI-10FSNQH":
                                    roomItem.IndoorItem.Model_Hitachi = "RPI-10.0FSNQH"; break;
                                default: break;
                            }
                            switch (roomItem.IndoorItem.Model)
                            {
                                case "RPI-8FSNQH":
                                    roomItem.IndoorItem.Model = "RPI-8.0FSNQH"; break;
                                case "RPI-10FSNQH":
                                    roomItem.IndoorItem.Model = "RPI-10.0FSNQH"; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 打开旧项目需要修正ODU 数据（EU,TW） on 20190318 by xyj 
        /// </summary>
        private static void compatOldProjectODU(Project thisProject)
        {
            //欧洲区域 ODU 数据需要读取最新的数据信息 on 20190315 by xyj 
            foreach (SystemVRF sys in thisProject.SystemList)
            {
                if (IsReadODUdata(sys, thisProject))
                {
                    Outdoor outItem = sys.OutdoorItem.DeepClone();
                    sys.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItemBySeries(sys.OutdoorItem.ModelFull, sys.OutdoorItem.Series);
                    if (sys.OutdoorItem == null)
                    {
                        sys.OutdoorItem = outItem;
                    }
                }
            }
        }
        /// <summary>
        /// 判断是否需要重新读取ODU 数据 on 20190318 by xyj 
        /// </summary>
        /// <returns></returns>
        private static bool IsReadODUdata(SystemVRF sys, Project thisProject)
        {
            //需要修正ODU EER,COP,SEER,SCOP,SOUNDPOWER CSPF
            if ((thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E") && (sys.OutdoorItem.SEER == 0 || sys.OutdoorItem.COP == 0 || sys.OutdoorItem.SCOP == 0 || sys.OutdoorItem.SoundPower == 0))
            {
                return true;
            }
            else if (thisProject.SubRegionCode == "TW" && sys.OutdoorItem.CSPF == 0)
            {
                return true;
            }
            return false;
        }
        public static int CompareVersion(string version, string str1)
        {
            if (string.IsNullOrEmpty(version))
            {
                return -1;
            }
            //对版本进行处理，去掉尾部的测试版本符号
            string lastch = version.Substring(version.Length - 1, 1);
            while (string.CompareOrdinal(lastch, "0") < 0 || string.CompareOrdinal(lastch, "9") > 0)
            {
                version = version.Remove(version.Length - 1);
                lastch = version.Substring(version.Length - 1, 1);
            }

            //对版本进行处理，去掉尾部的测试版本符号
            lastch = str1.Substring(str1.Length - 1, 1);
            while (string.CompareOrdinal(lastch, "0") < 0 || string.CompareOrdinal(lastch, "9") > 0)
            {
                str1 = str1.Remove(str1.Length - 1);
                lastch = str1.Substring(str1.Length - 1, 1);
            }

            return string.CompareOrdinal(version, str1);
        }

        #endregion Backward Compatibility Helper Methods


        public static int ImportVRFProject(string productType, string path = null)
        {
            int num = 0;
            int num1;
            IEnumerator enumerator = null;
            IEnumerator enumerator1 = null;
            IEnumerator enumerator2 = null;
            if (String.Compare(productType, "VRF", true) != 0)
            {
                num = 0;
            }
            else
            {
                MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                byte[][] numArray = new byte[5][];
                openFileDialog.Filter = "VRF Project(*.vrf)|*.vrf";
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                openFileDialog.Multiselect = false;
                if (path != null || openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    frmWait _frmWait = new frmWait();
                    try
                    {
                        if (path == null)
                        {
                            path = openFileDialog.FileName;
                        }
                        byte[] numArray1 = File.ReadAllBytes(path);

                        saveOpenedFilePath(path);

                        List<DataTable> dataTables = (List<DataTable>)binaryFormatter.Deserialize(new MemoryStream(numArray1));
                        DataTable item = dataTables[0];
                        DataTable dataTable = dataTables[1];
                        DataTable item1 = dataTables[2];
                        DataTable dataTable1 = dataTables[3];
                        DataRow dataRow = item.Rows[0];
                        double num2 = Convert.ToDouble(dataRow["Version"]);
                        double num3 = Convert.ToDouble(dataRow["DBVersion"]);
                        if (!(num2 > Convert.ToDouble(CCL.Ver) | num3 > Convert.ToDouble(CCL.DBVer)))
                        {
                            string str = Convert.ToString(dataRow["ProjectName"]);
                            if (CDF.CDF.GetProjectID(str, 1) != 0)
                            {
                                if (MessageBox.Show("The project exists.Do you want to rename it?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.No)
                                {
                                    while (true)
                                    {
                                        str = Microsoft.VisualBasic.Interaction.InputBox("Input new project name:", "New Project", "", -1, -1);
                                        if (string.Compare(str, "", true) == 0)
                                        {
                                            return num;
                                        }
                                        if (CDF.CDF.GetProjectID(str, 1) != 0)
                                        {
                                            MessageBox.Show("The porject exists.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                        else if (CCL.ValidateName(str, "Project name"))
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    return num;
                                }
                            }
                            _frmWait.Show();
                            Projects project = new Projects()
                            {
                                SystemID = Convert.ToString(dataRow["SystemID"]),
                                ActiveFlag = Convert.ToInt32(dataRow["ActiveFlag"]),
                                LastDate = Convert.ToDateTime(dataRow["LastUpdateDate"]),
                                Version = Convert.ToString(dataRow["Version"]),
                                DBVer = Convert.ToString(dataRow["DBVersion"]),
                                Measure = Convert.ToInt32(dataRow["Measure"]),
                                Location = Convert.ToString(dataRow["Location"]),
                                SoldTo = Convert.ToString(dataRow["SoldTo"]),
                                ShipTo = Convert.ToString(dataRow["ShipTo"]),
                                OrderNo = Convert.ToString(dataRow["OrderNo"]),
                                ContractNo = Convert.ToString(dataRow["ContractNo"]),
                                Region = Convert.ToString(dataRow["Region"]),
                                Office = Convert.ToString(dataRow["Office"]),
                                Engineer = Convert.ToString(dataRow["Engineer"]),
                                YINo = Convert.ToString(dataRow["YINo"]),
                                DeliveryDate = Convert.ToDateTime(dataRow["DeliveryDate"]),
                                OrderDate = Convert.ToDateTime(dataRow["OrderDate"]),
                                Remarks = Convert.ToString(dataRow["Remarks"]),
                                ProjectType = Convert.ToString(dataRow["ProjectType"]),
                                Vendor = Convert.ToString(dataRow["Vendor"])
                            };
                            object objectValue = RuntimeHelpers.GetObjectValue(dataRow["ProjectBlob"]);
                            object obj = RuntimeHelpers.GetObjectValue(dataRow["systemBlob"]);
                            object objectValue1 = RuntimeHelpers.GetObjectValue(dataRow["sqblob"]);
                            if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue)))
                            {
                                objectValue = null;
                            }
                            if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(obj)))
                            {
                                obj = null;
                            }
                            if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue1)))
                            {
                                objectValue1 = null;
                            }
                            num1 = CDF.CDF.InsertProjectInfo(str, project, (byte[])objectValue, (byte[])obj, (byte[])objectValue1);
                            int num4 = 0;
                            try
                            {
                                enumerator = dataTable.Rows.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    DataRow current = (DataRow)enumerator.Current;
                                    current = dataTable.Rows[num4];
                                    Products product = new Products()
                                    {
                                        SystemID = Convert.ToString(current["SystemID"]),
                                        ProjectID = num1,
                                        ProductID = Convert.ToInt32(current["ProductID"]),
                                        Name = Convert.ToString(current["ProductName"]),
                                        Measure = Convert.ToInt32(current["Measure"]),
                                        Label = Convert.ToString(current["Label"]),
                                        ProductVer = Convert.ToString(current["ProductVersion"]),
                                        DBVer = Convert.ToString(current["DBVersion"]),
                                        ActiveFlag = Convert.ToInt32(current["ActiveFlag"])
                                    };
                                    object obj1 = RuntimeHelpers.GetObjectValue(current["ProductBolb"]);
                                    object objectValue2 = RuntimeHelpers.GetObjectValue(current["FanBlob"]);
                                    object obj2 = RuntimeHelpers.GetObjectValue(current["SystemBlob"]);
                                    object objectValue3 = RuntimeHelpers.GetObjectValue(current["FolderBlob"]);
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(obj1)))
                                    {
                                        obj1 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue2)))
                                    {
                                        objectValue2 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(obj2)))
                                    {
                                        obj2 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue3)))
                                    {
                                        objectValue3 = null;
                                    }
                                    CDF.CDF.InsertProductInfo(num1, product.ProductID, product.Measure, product, (byte[])obj1, (byte[])objectValue2, (byte[])obj2, (byte[])objectValue3);
                                    num4 = checked(num4 + 1);
                                }
                            }
                            finally
                            {
                                if (enumerator is IDisposable)
                                {
                                    (enumerator as IDisposable).Dispose();
                                }
                            }
                            int num5 = 0;
                            try
                            {
                                enumerator1 = item1.Rows.GetEnumerator();
                                while (enumerator1.MoveNext())
                                {
                                    DataRow current1 = (DataRow)enumerator1.Current;
                                    current1 = item1.Rows[num5];
                                    Units unit = new Units()
                                    {
                                        SystemID = Convert.ToString(current1["SystemID"]),
                                        ProjectID = num1,
                                        ProductID = Convert.ToInt32(current1["ProductID"]),
                                        Name = Convert.ToString(current1["UnitName"]),
                                        ActiveFlag = Convert.ToInt32(current1["ActiveFlag"]),
                                        LastDate = Convert.ToDateTime(current1["LastUpdateDate"]),
                                        Measure = Convert.ToInt32(current1["Measure"]),
                                        Label = Convert.ToString(current1["Label"]),
                                        ProductVer = Convert.ToString(current1["ProductVersion"]),
                                        DBVer = Convert.ToString(current1["DBVersion"]),
                                        MLPDate = Convert.ToDateTime(current1["MLPEffectiveDate"]),
                                        Tag = Convert.ToString(current1["Tag"]),
                                        Quantity = Convert.ToInt32(current1["Quantity"]),
                                        Price = Convert.ToDouble(current1["Price"])
                                    };
                                    object obj3 = RuntimeHelpers.GetObjectValue(current1["UnitBlob"]);
                                    object objectValue4 = RuntimeHelpers.GetObjectValue(current1["SQBlob"]);
                                    object obj4 = RuntimeHelpers.GetObjectValue(current1["PriceBlob"]);
                                    object objectValue5 = RuntimeHelpers.GetObjectValue(current1["SystemBlob"]);
                                    object obj5 = RuntimeHelpers.GetObjectValue(current1["ProjectBlob"]);
                                    object objectValue6 = RuntimeHelpers.GetObjectValue(current1["FolderBlob"]);
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(obj3)))
                                    {
                                        obj3 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue4)))
                                    {
                                        objectValue4 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(obj4)))
                                    {
                                        obj4 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue5)))
                                    {
                                        objectValue5 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(obj5)))
                                    {
                                        obj5 = null;
                                    }
                                    if (DBNull.Value.Equals(RuntimeHelpers.GetObjectValue(objectValue6)))
                                    {
                                        objectValue6 = null;
                                    }
                                    CDF.CDF.InsertUnitInfo(num1, unit.ProductID, unit.Name, unit, (byte[])obj3, (byte[])objectValue4, (byte[])obj4, (byte[])objectValue5, (byte[])obj5, (byte[])objectValue6);
                                    num5 = checked(num5 + 1);
                                }
                            }
                            finally
                            {
                                if (enumerator1 is IDisposable)
                                {
                                    (enumerator1 as IDisposable).Dispose();
                                }
                            }
                            int num6 = 0;
                            try
                            {
                                enumerator2 = dataTable1.Rows.GetEnumerator();
                                while (enumerator2.MoveNext())
                                {
                                    DataRow dataRow1 = (DataRow)enumerator2.Current;
                                    dataRow1 = dataTable1.Rows[num6];
                                    Documents document = new Documents()
                                    {
                                        ActiveFlag = Convert.ToInt32(dataRow1["ActiveFlag"]),
                                        Commands = Convert.ToString(dataRow1["Commands"]),
                                        DocName = Convert.ToString(dataRow1["DocumentName"]),
                                        Labels = Convert.ToString(dataRow1["Labels"]),
                                        LastDate = DateTime.Now.Date,
                                        ProductID = Convert.ToString(dataRow1["ProductID"]),
                                        ProjectID = Convert.ToString(num1),
                                        SystemID = Convert.ToString(dataRow1["SystemID"]),
                                        UnitName = Convert.ToString(dataRow1["UnitName"])
                                    };
                                    CDF.CDF.InsertDocumentInfo(num1, Convert.ToInt32(document.ProductID), document.UnitName, document);
                                    num6 = checked(num6 + 1);
                                }
                            }
                            finally
                            {
                                if (enumerator2 is IDisposable)
                                {
                                    (enumerator2 as IDisposable).Dispose();
                                }
                            }
                            _frmWait.Close();
                            if (CDF.CDF.IsProductExistInProject(num1, 205, 1))
                            {
                                if (!(new YAMIntegration.YAMIntegration()).SaveProjectToLocal(num1))
                                {
                                }
                            }
                        }
                        else
                        {
                            _frmWait.Close();
                            MessageBox.Show("You selected import file is not compatible with this version. This Project could not be imported!", "Import failed!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return num;
                        }
                    }
                    catch (Exception exception)
                    {
                        ProjectData.SetProjectError(exception);
                        MessageBox.Show("The project file is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        num = 0;
                        ProjectData.ClearProjectError();
                        return num;
                    }
                    num = num1;
                }
            }
            return num;
        }
        private static void saveOpenedFilePath(string filePath)
        {
            //string jchTempDir = Path.Combine(Path.GetTempPath(),"JCHTemp");
            if (!Directory.Exists(Paths.JchTempDir))
            {
                Directory.CreateDirectory(Paths.JchTempDir);
            }
            string recentOpenedFiles = Path.Combine(Paths.JchTempDir, Paths.RecentFileList);

            if (!File.Exists(recentOpenedFiles))
            {
                using (File.Create(recentOpenedFiles)) ;
            }

            List<string> fileLines = File.ReadAllLines(recentOpenedFiles)?.ToList();
            if (fileLines != null && fileLines.Contains(filePath))
            {
                fileLines.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            }

            fileLines.Insert(0, filePath);
            if (fileLines.Count > 5)
            {
                fileLines.RemoveRange(5, fileLines.Count - 5);
            }
            File.WriteAllLines(recentOpenedFiles, fileLines.ToArray());
            //GetRecentProjectFilesData();
        }

        public static string StringConversion(object Value)
        {
            string returnValue = string.Empty;
            if (Value != null)
            {
                if (Value is DBNull)
                {
                    returnValue = string.Empty;
                }
                else
                {
                    returnValue = Convert.ToString(Value);
                }
            }
            return returnValue;

        }
        public static double IntergerParser(object Value)
        {
            double ReturnValue = -1;
            if (Value != null)
            {
                if (Value is DBNull)
                {
                    ReturnValue = -1;
                }
                else
                {
                    Double.TryParse(Convert.ToString(Value), out ReturnValue);
                }
            }
            return ReturnValue;
        }
        public static double DoubleParser(object Value)
        {
            double ReturnValue = -1;
            if (Value != null)
            {
                if (Value is DBNull)
                {
                    ReturnValue = -1;
                }
                else
                {
                    Double.TryParse(Convert.ToString(Value), out ReturnValue);
                }
            }
            return ReturnValue;
        }

    }
}
