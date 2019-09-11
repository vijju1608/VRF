using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Aspose.Words;
using Aspose.Words.Tables;
using JCBase.UI;
using JCHVRF.BLL;
using JCHVRF.Const;
using JCHVRF.Model;
using JCHVRF.MyPipingBLL;
using JCHVRF.Report;
using JCBase.Utility;
using JCHVRF.VRFMessage;
using System.Drawing;
using JCHVRF.VRFTrans;
using NGModel = JCHVRF.Model.NextGen;
using NGPipBLL = JCHVRF.MyPipingBLL.NextGen;
using WL = Lassalle.WPF.Flow;
using NextGenModel = JCHVRF.Model.NextGen;

namespace JCHVRF
{
    public class NewReport
    {
        #region 获取当前设置的单位表达式
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string utArea = SystemSetting.UserSetting.unitsSetting.settingAREA;
        string utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        string utDimension = SystemSetting.UserSetting.unitsSetting.settingDimensionUnit;
        string utLoadIndex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
        string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
        string utWeight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
        string utPressure = Unit.ut_Pressure; //add by axj 20170710
        string utPowerInput = "kW";
        string utMaxOperationPI = "kW";
        #endregion

        #region 内部成员
        Project thisProject;
        List<dynamic> outdoorUnits = new List<dynamic>();
        List<dynamic> indoorUnits = new List<dynamic>();
        dynamic outSectionBasic = null;
        List<dynamic> systemList = new List<dynamic>();
        List<dynamic> pipingList = new List<dynamic>();
        List<dynamic> wiringList = new List<dynamic>();
        List<dynamic> listOutdoors = new List<dynamic>();
        List<dynamic> listIndoors = new List<dynamic>();
        List<dynamic> listIndoorsBom = new List<dynamic>();
        List<dynamic> listAccessories = new List<dynamic>();
        List<dynamic> listAccessoriesBom = new List<dynamic>();
        List<dynamic> listPipingConnectionKit = new List<dynamic>();
        List<dynamic> listPipingConnectionKitBom = new List<dynamic>();
        List<dynamic> exc_listAccessories = new List<dynamic>();
        List<dynamic> listCHBox = new List<dynamic>();
        List<dynamic> listCHBoxBom = new List<dynamic>();
        List<dynamic> listBranchKit = new List<dynamic>();
        List<dynamic> listBranchKitBom = new List<dynamic>();
        List<dynamic> listPipingLen = new List<dynamic>();
        List<dynamic> listPipingLenBom = new List<dynamic>();
        string totalAddRefrigeration = "";
        List<dynamic> heatExchangerUnits = new List<dynamic>();
        List<dynamic> listControls = new List<dynamic>();
        Dictionary<string, dynamic> remoteControlSwith = new Dictionary<string, dynamic>();
        List<string> ReportLogList = null;
        int centralControllerNum = 0;
        Table tbBasic = null;
        bool IsZeropipelength = false;

        Document Doc = null;//doc文档
        Section selection = null;//选择区域
        NodeCollection Tables = null;//doc表集合
        DocumentBuilder Builder = null;//doc构造器
        Bookmark mark = null;
        //显示Actual和Nominal标识
        public bool isActual = true;
        private bool isIduCapacityW = false;

        Trans trans = null;
        #endregion

        #region 构造函数
        public NewReport(Project prj)
        {
            this.isIduCapacityW = prj.IsIduCapacityW;
            JCBase.Utility.AsposeHelp.InitAsposeWord();
            trans = InitTranslation();
            thisProject = prj;
            GetReportData();
            this.ReportLogList = new List<string>();
        }
        #endregion

        #region 建立报告数据对象
        /// <summary>
        /// 获取数据
        /// </summary>
        private void GetReportData()
        {
            //1、outdoor list
            BuildOutdoorUnitsData();
            //2、indoor list （按房间|无房间直接汇总）
            BuildIndoorUnitsData();
            //3、工程外部环境参数 （温度）
            // TODO BuildProjectBasicData();
            //4、系统设计： 系统集合 1、room 2、outdoor 3、 indoor（遍历系统）
            BuildSystemDesignData();
            //5、配管设计：1、配管图 2、规则限制 3、冷媒 （遍历系统）
            BuildPipingDesignData();
            //6、线图设计：1、线路图 2、功率 
            BuildWiringDesignData();
            //7、清单：1、室外机 2、室内机 配件 3、Heat Exchanger Units  4、控制器 5、Branch Kit 6、CH Box 7、Field Providing
            BuildProductListData();
            //其他
        }

        /// <summary>
        /// 生成OutdoorUnits数据对象
        /// </summary>
        /// 
        private void BuildOutdoorUnitsData()
        {
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                thisProject.SystemListNextGen.ForEach((p) =>
                {
                    if (p.IsExportToReport == true)
                    {
                        var result = outdoorUnits.Find(o => o.Model == p.OutdoorItem.AuxModelName && p.IsExportToReport == true);
                        if (result == null)
                        {
                            Outdoor item = p.OutdoorItem;
                            if (item != null)
                            {
                                //------------
                                string combinedwidth = string.Empty;
                                string combinedweight = string.Empty;
                                string combinedmaxcurr = string.Empty;

                                OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
                                if ((item.FullModuleName.Contains("+") || item.FullModuleName.Contains("*2") || item.FullModuleName.Contains("*3") || item.FullModuleName.Contains("*4"))
                                && (item.Type == "FSNYW1Q (Water source)" ||
                                item.Type == "HNCQ (Top flow, 3Ph)" || item.Type == "CNCQ (Top flow, 3Ph)" ||
                                item.Type == "HNBQ (Top flow, 3Ph)" || item.Type == "JNBBQ (Top flow, 3Ph)" ||
                                item.Type == "JTOH (All Inverter)" || item.Type == "JVOHQ (Top flow, 3Ph)" ||
                                item.Type == "HNCTQ (Top flow, 3Ph)"))
                                {
                                    dynamic dyn = new System.Dynamic.ExpandoObject();
                                    dyn = bll.GetOutdoorCombinedDetail(p.OutdoorItem.FullModuleName, p.OutdoorItem.Series);

                                    combinedwidth = dyn.Width;
                                    combinedweight = dyn.Weight;
                                }
                                else
                                {
                                    combinedwidth = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Width), UnitType.LENGTH_MM, utDimension).ToString("n0");
                                    combinedweight = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                                }
                                //------------

                                string fullname = item.FullModuleName == null ? " " : item.FullModuleName;
                                List<string> components = SplitOutdoorFullModuleName(item.FullModuleName);
                                if (components.Count == 1)
                                {
                                    components = new List<string>();
                                }
                                //dynamic SEER = "";//TODO
                                //dynamic SCOP = "";//TODO
                                var SoundPower = item.NoiseLevel;
                                MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, p.Power);
                                var Information = new
                                {
                                    PowerSupply = powerItem.Name,
                                    Cooling = Unit.ConvertToControl(p.OutdoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1"),
                                    Heating = Unit.ConvertToControl(p.OutdoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1"),
                                    Height = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Height), UnitType.LENGTH_MM, utDimension).ToString("n0"),
                                    Width = combinedwidth,
                                    Depth = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Length), UnitType.LENGTH_MM, utDimension).ToString("n0"),
                                    Weight = combinedweight,
                                    EER = p.OutdoorItem.EER == 0 ? "" : p.OutdoorItem.EER.ToString("n2"),
                                    COP = p.OutdoorItem.COP == 0 ? "" : p.OutdoorItem.COP.ToString("n2"),
                                    SEER = p.OutdoorItem.SEER == 0 ? "" : p.OutdoorItem.SEER.ToString("n2"),
                                    SCOP = p.OutdoorItem.SCOP == 0 ? "" : p.OutdoorItem.SCOP.ToString("n2"),
                                    SoundPower = p.OutdoorItem.SoundPower == 0 ? "" : p.OutdoorItem.SoundPower.ToString("n2"),
                                    CSPF = p.OutdoorItem.CSPF == 0 ? "" : p.OutdoorItem.CSPF.ToString("n2")
                                };
                                dynamic outdoor = new System.Dynamic.ExpandoObject();
                                outdoor.Pictures = p.OutdoorItem.TypeImage;
                                outdoor.Model = p.OutdoorItem.AuxModelName;
                                //outdoor.Description = p.OutdoorItem.Series;
                                outdoor.Description = trans.getTypeTransStr(TransType.Series.ToString(), p.OutdoorItem.Series);
                                outdoor.Quantity = 1;
                                outdoor.Components = components;
                                outdoor.Information = Information; ;
                                outdoorUnits.Add(outdoor);
                            }
                        }
                        //    else
                        //    {
                        //        result.Quantity++;
                        //    }
                    }
                });
            }
        }
        //private void BuildOutdoorUnitsData()
        //{
        //    if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
        //    {

        //        ////comment because prerequisit outdoor information we are not getting
        //        thisProject.SystemListNextGen.ForEach((p) =>
        //        {
        //            var result = outdoorUnits.Find(o => o.Model == p.OutdoorItem.AuxModelName);
        //            //if (result == null)
        //            //{
        //            if (p.IsExportToReport == true)
        //            {
        //                Outdoor item = p.OutdoorItem;
        //                if (item != null)
        //                {
        //                    //------------
        //                    string combinedwidth = string.Empty;
        //                    string combinedweight = string.Empty;
        //                    string combinedmaxcurr = string.Empty;

        //                    OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
        //                    if ((item.FullModuleName.Contains("+") || item.FullModuleName.Contains("*2") || item.FullModuleName.Contains("*3") || item.FullModuleName.Contains("*4"))
        //                    && (item.Type == "FSNYW1Q (Water source)" ||
        //                    item.Type == "HNCQ (Top flow, 3Ph)" || item.Type == "CNCQ (Top flow, 3Ph)" ||
        //                    item.Type == "HNBQ (Top flow, 3Ph)" || item.Type == "JNBBQ (Top flow, 3Ph)" ||
        //                    item.Type == "JTOH (All Inverter)" || item.Type == "JVOHQ (Top flow, 3Ph)"))
        //                    {
        //                        dynamic dyn = new System.Dynamic.ExpandoObject();
        //                        dyn = bll.GetOutdoorCombinedDetail(p.OutdoorItem.FullModuleName, p.OutdoorItem.Series);

        //                        combinedwidth = dyn.Width;
        //                        combinedweight = dyn.Weight;
        //                    }
        //                    else
        //                    {
        //                        combinedwidth = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Width), UnitType.LENGTH_MM, utDimension).ToString("n0");
        //                        combinedweight = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
        //                    }
        //                    //------------
        //                    string fullname = item.FullModuleName == null ? " " : item.FullModuleName;
        //                    List<string> components = SplitOutdoorFullModuleName(item.FullModuleName);
        //                    if (components.Count == 1)
        //                    {
        //                        components = new List<string>();
        //                    }
        //                    //dynamic components = new List<string>();
        //                    //if (fullname.Contains("+"))
        //                    //{
        //                    //    components = fullname.Split('+').ToList();
        //                    //}
        //                    //dynamic SEER = "";//TODO
        //                    //dynamic SCOP = "";//TODO
        //                    var SoundPower = item.NoiseLevel;
        //                    MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, p.Power);
        //                    var Information = new
        //                    {
        //                        PowerSupply = powerItem.Name,
        //                        Cooling = Unit.ConvertToControl(p.OutdoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1"),
        //                        Heating = Unit.ConvertToControl(p.OutdoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1"),
        //                        Height = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Height), UnitType.LENGTH_MM, utDimension).ToString("n0"),
        //                        // Previos Code // Width = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Width), UnitType.LENGTH_MM, utDimension).ToString("n0"),
        //                        Width = combinedwidth,  // New Code After Integrate 4.6
        //                        Depth = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Length), UnitType.LENGTH_MM, utDimension).ToString("n0"),
        //                        // Previos Code // Weight = Unit.ConvertToControl(Convert.ToDouble(p.OutdoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0"),
        //                        Weight = combinedweight,  // New Code After Integrate 4.6
        //                        EER = p.OutdoorItem.EER == 0 ? "" : p.OutdoorItem.EER.ToString("n2"),
        //                        COP = p.OutdoorItem.COP == 0 ? "" : p.OutdoorItem.COP.ToString("n2"),
        //                        SEER = p.OutdoorItem.SEER == 0 ? "" : p.OutdoorItem.SEER.ToString("n2"),
        //                        SCOP = p.OutdoorItem.SCOP == 0 ? "" : p.OutdoorItem.SCOP.ToString("n2"),
        //                        SoundPower = p.OutdoorItem.SoundPower == 0 ? "" : p.OutdoorItem.SoundPower.ToString("n2"),
        //                        CSPF = p.OutdoorItem.CSPF == 0 ? "" : p.OutdoorItem.CSPF.ToString("n2") //  New Code After Integrate 4.6
        //                    };
        //                    dynamic outdoor = new System.Dynamic.ExpandoObject();
        //                    outdoor.Pictures = p.OutdoorItem.TypeImage;
        //                    outdoor.Model = p.OutdoorItem.AuxModelName;
        //                    // Previos Code // outdoor.Description = p.OutdoorItem.Series;
        //                    outdoor.Description = trans.getTypeTransStr(TransType.Series.ToString(), p.OutdoorItem.Series);
        //                    outdoor.Quantity = 1;
        //                    outdoor.Components = components;
        //                    outdoor.Information = Information; ;
        //                    outdoorUnits.Add(outdoor);
        //                }
        //                //}
        //                //else
        //                //{
        //                //    //result.Quantity++;
        //                //}
        //            }
        //        });
        //    }
        //}

        /// <summary>
        /// 生成IndoorUnits数据对象
        /// </summary>
        private void BuildIndoorUnitsData()
        {
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                thisProject.SystemListNextGen.ForEach((s) =>
                {
                    if (s.IsExportToReport == true)
                    {
                        if (thisProject.RoomIndoorList != null && thisProject.RoomIndoorList.Count > 0)
                        {

                            int groupIndex = 1;
                            Dictionary<string, int> indoorGroupIndex = new Dictionary<string, int>();
                            thisProject.RoomIndoorList.ForEach((r) =>
                            {
                                if (r.SystemID == ((JCHVRF.Model.SystemBase)s).Id)
                                {
                                    if (r.IsMainIndoor && r.IndoorItemGroup != null && r.IndoorItemGroup.Count > 0)
                                    {
                                        indoorGroupIndex[r.IndoorNO + "&Indoor"] = groupIndex;
                                        r.IndoorItemGroup.ForEach((m) =>
                                        {
                                            if (m._isExchanger)
                                            {
                                                indoorGroupIndex[m.IndoorNO + "&Exchanger"] = groupIndex;
                                            }
                                            else
                                            {
                                                indoorGroupIndex[m.IndoorNO + "&Indoor"] = groupIndex;
                                            }
                                        });
                                        groupIndex++;
                                    }
                                }
                            });
                            if (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0)
                            {
                                thisProject.ExchangerList.ForEach((r) =>
                                {
                                    if (r.SystemID == ((JCHVRF.Model.SystemBase)s).Id)
                                    {
                                        if (r.IsMainIndoor && r.IndoorItemGroup != null && r.IndoorItemGroup.Count > 0)
                                        {
                                            indoorGroupIndex[r.IndoorNO + "&Exchanger"] = groupIndex;
                                            r.IndoorItemGroup.ForEach((m) =>
                                            {
                                                if (m._isExchanger)
                                                {
                                                    indoorGroupIndex[m.IndoorNO + "&Exchanger"] = groupIndex;
                                                }
                                                else
                                                {
                                                    indoorGroupIndex[m.IndoorNO + "&Indoor"] = groupIndex;
                                                }
                                            });
                                            groupIndex++;
                                        }
                                    }
                                });
                            }


                            var fun = new Action<List<RoomIndoor>, string, dynamic>((list, type, room) =>
                            {
                                list.ForEach((d) =>
                                {
                                    if ((d.SystemID == ((JCHVRF.Model.SystemBase)s).Id))
                                    {
                                        if (d.IndoorItem != null)
                                        {
                                            //var accessoryList = d.IndoorItem.ListAccessory;
                                            
                                                var accessoryList = d.ListAccessory?.Where(a => a.IsSelect).ToList(); //Bug 3957- added the where clause;
                                                string accessories = "";
                                                List<dynamic> controls = new List<dynamic>();
                                                if (accessoryList != null)
                                                {
                                                    accessoryList.ForEach((a) =>
                                                    {
                                                        string name = (a.BrandCode == "Y" ? a.Model_York : a.Model_Hitachi);
                                                    //if (!a.Type.Contains("Remote Control Switch"))
                                                    //{
                                                    //    if (accessories == "")
                                                    //    {
                                                    //        /*accessories = accessories + GetAccessoryDisplayName(a, d) + " " + a.Description*/
                                                    //        ;
                                                    //        accessories = accessories + GetAccessoryDisplayName(a, d) + "\r\n";
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        //accessories = accessories + "\r\n" + GetAccessoryDisplayName(a, d) + " " + a.Description;
                                                    //        accessories = accessories + "\r\n" + GetAccessoryDisplayName(a, d);
                                                    //    }
                                                    //}
                                                    //else
                                                    //{
                                                    string gp = "";
                                                    //accessories = accessories + GetAccessoryDisplayName(a, d) + "\r\n" + a.Description;
                                                    accessories = accessories + (a.Type==null?" ": a.Type) + " (" + name + ")" + "\r\n" + "\r\n";

                                                        if (d.IsMainIndoor && indoorGroupIndex.Keys.Contains(d.IndoorNO + type) && (a.IsShared == true || thisProject.SubRegionCode == "ANZ"))
                                                        {
                                                            gp = indoorGroupIndex[d.IndoorNO + type].ToString();
                                                        }
                                                        var control = new { Picture = name + ".PNG", Model = name, Gp = gp };
                                                        if (gp != "")
                                                        {
                                                            remoteControlSwith[gp] = new { Picture = control.Picture, Name = "Shared with " + d.IndoorName };
                                                        }
                                                        controls.Add(control);
                                                    //}
                                                });
                                                }
                                                if (!d.IsMainIndoor && indoorGroupIndex.Keys.Contains(d.IndoorNO + type))
                                                {
                                                    var control = new { Picture = "", Model = "", Gp = indoorGroupIndex[d.IndoorNO + type].ToString() };
                                                    controls.Add(control);
                                                }
                                                //var cool = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                                //var heat = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                                var cool = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");

                                                if (utPower == "kW" && isIduCapacityW == true)
                                                {
                                                    cool = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                                }

                                                var heat = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                                if (utPower == "kW" && isIduCapacityW == true)
                                                {
                                                    heat = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                                }
                                                var model = GetIndoorDisplayName(d) + "\r\n" + (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                                                var ind = new { Picture = d.IndoorItem.TypeImage, Ident = d.IndoorName, Model = model, Cool = type == "&Exchanger" ? "-" : cool, Heat = type == "&Exchanger" ? "-" : heat, Accessories = accessories, Control = controls, sysID = d.SystemID };
                                                room.indoors.Add(ind);
                                            }
                                        }
                                    
                                });
                            });

                            var funNew = new Action<RoomIndoor, string, dynamic>((item, type, room) =>
                            {
                                if (item != null)
                                {
                                    var accessoryList = item.ListAccessory.Where(a => a.IsSelect).ToList(); //Bug 3957- added the where clause;
                                    string accessories = "";
                                    List<dynamic> controls = new List<dynamic>();
                                    if (accessoryList != null)
                                    {
                                        accessoryList.ForEach((a) =>
                                        {
                                            string name = (a.BrandCode == "Y" ? a.Model_York : a.Model_Hitachi);
                                            //if (!a.Type.Contains("Remote Control Switch"))
                                            //{
                                            //    if (accessories == "")
                                            //    {
                                            //        accessories = accessories + GetAccessoryDisplayName(a, item) + " " + name;
                                            //    }
                                            //    else
                                            //    {
                                            //        accessories = accessories + "\r\n" + GetAccessoryDisplayName(a, item) + " " + name;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            string gp = "";
                                            accessories = accessories + GetAccessoryDisplayName(a, item) + "\r\n" + (a.Description==null?a.Type: a.Description);
                                            if (item.IsMainIndoor && indoorGroupIndex.Keys.Contains(item.IndoorNO + type) && (a.IsShared == true || thisProject.SubRegionCode == "ANZ"))
                                            {
                                                gp = indoorGroupIndex[item.IndoorNO + type].ToString();
                                            }
                                            var control = new { Picture = name + ".PNG", Model = name, Gp = gp };
                                            if (gp != "")
                                            {
                                                remoteControlSwith[gp] = new { Picture = control.Picture, Name = "Shared with " + item.IndoorName };
                                            }
                                            controls.Add(control);
                                            //}
                                        });
                                    }
                                    if (!item.IsMainIndoor && indoorGroupIndex.Keys.Contains(item.IndoorNO + type))
                                    {
                                        var control = new { Picture = "", Model = "", Gp = indoorGroupIndex[item.IndoorNO + type].ToString() };
                                        controls.Add(control);
                                    }
                                    var cool = Unit.ConvertToControl(item.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    var heat = Unit.ConvertToControl(item.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    var model = GetIndoorDisplayName(item) + "\r\n" + (thisProject.BrandCode == "Y" ? item.IndoorItem.Model_York : item.IndoorItem.Model_Hitachi);
                                    var ind = new { Picture = item.IndoorItem.TypeImage, Ident = item.IndoorName, Model = model, Cool = type == "&Exchanger" ? "-" : cool, Heat = type == "&Exchanger" ? "-" : heat, Accessories = accessories, Control = controls, sysID = item.SystemID };
                                    room.indoors.Add(ind);
                                }
                            });

                            //Bug#2481 - Commenting this part because in new application Roomlist and FloorList work independently
                            //if (thisProject.FloorList != null && thisProject.FloorList.Count > 0)
                            //{
                            //    thisProject.FloorList.ForEach((f) =>
                            //    {

                            //        if (f.RoomList != null)
                            //        {
                            //            f.RoomList.ForEach((r) =>
                            //            {
                            //                dynamic room = new { roomName = f.Name + "-" + r.Name, indoors = new List<dynamic>() };
                            //                var list = thisProject.RoomIndoorList.FindAll(m => m.RoomID == r.Id && m.SystemID== ((JCHVRF.Model.SystemBase)s).Id);
                            //                if (list != null && list.Count > 0)
                            //                {
                            //                    fun(list, "&Indoor", room);
                            //                }
                            //                if (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0)
                            //                {
                            //                    list = thisProject.ExchangerList.FindAll(m => m.RoomID == r.Id && m.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                            //                    if (list != null && list.Count > 0)
                            //                    {
                            //                        fun(list, "&Exchanger", room);
                            //                    }
                            //                }

                            //                indoorUnits.Add(room);
                            //            });
                            //        }
                            //    });
                            //}
                            //end Bug#2481

                            if (thisProject.RoomIndoorList != null && thisProject.RoomList.Count > 0)
                            {
                                var list = thisProject.RoomIndoorList.FindAll(m => !string.IsNullOrEmpty(m.RoomName) && m.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                                if (list != null && list.Count > 0)
                                {
                                    foreach (var item in list)
                                    {
                                        dynamic room = new { roomName = item.RoomName, indoors = new List<dynamic>() };
                                        funNew(item, "&Indoor", room);
                                        indoorUnits.Add(room);
                                    }
                                }
                            }
                            if (thisProject.FreshAirAreaList != null && thisProject.FreshAirAreaList.Count > 0)
                            {
                                thisProject.FreshAirAreaList.ForEach((f) =>
                                {
                                    dynamic room = new { roomName = f.Name, indoors = new List<dynamic>() };
                                    var list = thisProject.RoomIndoorList.FindAll(m => m.RoomID == f.Id && !string.IsNullOrEmpty(m.SystemID) && m.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                                    if (list != null && list.Count > 0)
                                    {
                                        fun(list, "&Indoor", room);
                                    }

                                    indoorUnits.Add(room);
                                });
                            }
                            if (thisProject.RoomIndoorList.Count > 0)
                            {
                                dynamic room = new { roomName = "No Room", indoors = new List<dynamic>() };
                                //var inds = thisProject.RoomIndoorList.FindAll(r => string.IsNullOrEmpty(r.RoomID));
                                var inds = thisProject.RoomIndoorList.FindAll(r => string.IsNullOrEmpty(r.RoomName) || (r.RoomID) == "0" || string.IsNullOrEmpty(r.RoomID) && r.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                                //var inds = thisProject.RoomIndoorList;FindAll(r => string.IsNullOrEmpty(r.RoomID) && !string.IsNullOrEmpty(r.SystemID));
                                if (inds != null && inds.Count > 0)
                                {
                                    var list = inds;
                                    if (list != null && list.Count > 0)
                                    {
                                        fun(list, "&Indoor", room);
                                    }
                                }
                                indoorUnits.Add(room);
                            }

                        }
                    }
                }
                );
            }
            var fun1 = new Action<List<RoomIndoor>, string, dynamic>((list, type, room) =>
            {
                Dictionary<string, int> indoorGroupIndex = new Dictionary<string, int>();

                list.ForEach((d) =>
                {
                    if (type == "&Exchanger")
                    {
                        if (d.IndoorItem != null)
                        {
                            var accessoryList = d.ListAccessory;
                            string accessories = "";
                            List<dynamic> controls = new List<dynamic>();
                            if (accessoryList != null)
                            {
                                accessoryList.ForEach((a) =>
                                {
                                    string name = (a.BrandCode == "Y" ? a.Model_York : a.Model_Hitachi);
                                    accessories = accessories + a.Type + "\r\n" + "\r\n";
                                    string gp = "";
                                    if (d.IsMainIndoor && indoorGroupIndex.Keys.Contains(d.IndoorNO + type) && (a.IsShared == true || thisProject.SubRegionCode == "ANZ"))
                                    {
                                        gp = indoorGroupIndex[d.IndoorNO + type].ToString();
                                    }
                                    var control = new { Picture = name + ".PNG", Model = name, Gp = gp };
                                    if (gp != "")
                                    {
                                        remoteControlSwith[gp] = new { Picture = control.Picture, Name = "Shared with " + d.IndoorName };
                                    }
                                    controls.Add(control);
                                });
                            }
                            if (!d.IsMainIndoor && indoorGroupIndex.Keys.Contains(d.IndoorNO + type))
                            {
                                var control = new { Picture = "", Model = "", Gp = indoorGroupIndex[d.IndoorNO + type].ToString() };
                                controls.Add(control);
                            }
                            var cool = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                            var heat = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                            var model = GetIndoorDisplayName(d) + "\r\n" + (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                            // Previos Code //   var ind = new { Picture = d.IndoorItem.TypeImage, Ident = d.IndoorName, Model = model, Cool = type == "&Exchanger" ? "-" : cool, Heat = type == "&Exchanger" ? "-" : heat, Accessories = accessories, Control = controls };
                            var ind = new { Picture = d.DisplayImagePath, Ident = d.IndoorName, Model = model, Cool = type == "&Exchanger" ? "-" : cool, Heat = type == "&Exchanger" ? "-" : heat, Accessories = accessories, Control = controls, sysID = d.SystemID };  //  New Code After Integrate 4.6
                            room.indoors.Add(ind);
                        }
                    }
                });
            });

            var funNew1 = new Action<RoomIndoor, string, dynamic>((item, type, room) =>
            {
                Dictionary<string, int> indoorGroupIndex = new Dictionary<string, int>();
                if (item != null)
                {
                    if (type == "&Exchanger")
                    {
                        if (item.IndoorItem != null)
                        {
                            var accessoryList = item.ListAccessory;
                            string accessories = "";
                            List<dynamic> controls = new List<dynamic>();
                            if (accessoryList != null)
                            {
                                accessoryList.ForEach((a) =>
                                {
                                    string name = (a.BrandCode == "Y" ? a.Model_York : a.Model_Hitachi);
                                    accessories = accessories + a.Type + "\r\n" + "\r\n";
                                    string gp = "";
                                    if (item.IsMainIndoor && indoorGroupIndex.Keys.Contains(item.IndoorNO + type) && (a.IsShared == true || thisProject.SubRegionCode == "ANZ"))
                                    {
                                        gp = indoorGroupIndex[item.IndoorNO + type].ToString();
                                    }
                                    var control = new { Picture = name + ".PNG", Model = name, Gp = gp };
                                    if (gp != "")
                                    {
                                        remoteControlSwith[gp] = new { Picture = control.Picture, Name = "Shared with " + item.IndoorName };
                                    }
                                    controls.Add(control);
                                });
                            }
                            if (!item.IsMainIndoor && indoorGroupIndex.Keys.Contains(item.IndoorNO + type))
                            {
                                var control = new { Picture = "", Model = "", Gp = indoorGroupIndex[item.IndoorNO + type].ToString() };
                                controls.Add(control);
                            }
                            var cool = Unit.ConvertToControl(item.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                            var heat = Unit.ConvertToControl(item.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                            var model = GetIndoorDisplayName(item) + "\r\n" + (thisProject.BrandCode == "Y" ? item.IndoorItem.Model_York : item.IndoorItem.Model_Hitachi);
                            var ind = new { Picture = item.DisplayImagePath, Ident = item.IndoorName, Model = model, Cool = type == "&Exchanger" ? "-" : cool, Heat = type == "&Exchanger" ? "-" : heat, Accessories = accessories, Control = controls, sysID = item.SystemID };
                            //var ind = new { Picture = item.IndoorItem.TypeImage, Ident = item.IndoorName, Model = model, Cool = type == "&Exchanger" ? "-" : cool, Heat = type == "&Exchanger" ? "-" : heat, Accessories = accessories, Control = controls };
                            room.indoors.Add(ind);
                        }
                    }
                }
            });


            if (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0)
            {
                dynamic room = new { roomName = "No Room", indoors = new List<dynamic>() };
                var inds = thisProject.ExchangerList.FindAll(r => string.IsNullOrEmpty(r.RoomID));

                if (inds != null && inds.Count > 0)
                {
                    var list = inds;
                    if (list != null && list.Count > 0)
                    {
                        fun1(list, "&Exchanger", room);
                    }
                }
                indoorUnits.Add(room);
            }
            if (thisProject.RoomIndoorList != null && thisProject.RoomList.Count > 0)
            {
                if (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0)
                {
                    var list = thisProject.ExchangerList.FindAll(m => m.RoomID != null);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            dynamic room = new { roomName = item.RoomName, indoors = new List<dynamic>() };
                            funNew1(item, "&Exchanger", room);
                            indoorUnits.Add(room);
                        }
                    }
                }
            }
            if (thisProject.FreshAirAreaList != null && thisProject.FreshAirAreaList.Count > 0)
            {
                thisProject.FreshAirAreaList.ForEach((f) =>
                {
                    dynamic room = new { roomName = f.Name, indoors = new List<dynamic>() };
                    if (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0)
                    {
                        var list = thisProject.ExchangerList.FindAll(m => m.RoomID == f.Id); //&& m.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                        if (list != null && list.Count > 0)
                        {
                            fun1(list, "&Exchanger", room);
                        }
                    }

                    indoorUnits.Add(room);
                });
            }
        }

        /// <summary>
        /// 生成项目环境数据对象
        /// </summary>
        private void BuildProjectBasicData()
        {
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                double dbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB;
                double wbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
                double dbHeat = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
                if (thisProject.RoomIndoorList.Count > 0)
                {
                    dbCool = thisProject.RoomIndoorList[0].DBCooling;
                    wbCool = thisProject.RoomIndoorList[0].WBCooling;
                    dbHeat = thisProject.RoomIndoorList[0].DBHeating;
                }
                double dbCoolOutdoor = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
                double dbHeatOutdoor = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB;
                double wbHeatOutdoor = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;

                double iwCoolingInletWaterOutdoor = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
                double iwOutdoorHeatingInletWater = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;
                bool isWaterSource = false;
                bool isUnWaterSource = false;
                //判断是否水机与非水机
                thisProject.SystemListNextGen.ForEach((s) =>
                {
                    if (s.IsExportToReport == true)
                    {
                        if (s.OutdoorItem.ProductType.Contains("Water Source"))
                        {
                            iwCoolingInletWaterOutdoor = s.IWCooling;
                            iwOutdoorHeatingInletWater = s.IWHeating;
                            isWaterSource = true;
                        }
                        else
                        {
                            dbCoolOutdoor = s.DBCooling;
                            dbHeatOutdoor = s.DBHeating;
                            wbHeatOutdoor = s.WBHeating;
                            isUnWaterSource = true;
                        }
                    }
                });

                //OutdoorCoolingDB
                string outdoorCoolingDB = Unit.ConvertToControl(dbCoolOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                //Outdoor Heating DB
                string outdoorHeatingDB = Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                //Outdoor Heating WB
                string outdoorHeatingWB = Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                //CoolingInletWater
                string coolingInletWater = Unit.ConvertToControl(iwCoolingInletWaterOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                //HeatingInletWater 
                string heatingInletWater = Unit.ConvertToControl(iwOutdoorHeatingInletWater, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                if (isWaterSource)
                {
                    outdoorCoolingDB = "-";
                    //Outdoor Heating DB
                    outdoorHeatingDB = "-";
                    //Outdoor Heating WB
                    outdoorHeatingWB = "-";
                }
                else if (isUnWaterSource)
                {
                    coolingInletWater = "-";
                    //HeatingInletWater 
                    heatingInletWater = "-";
                }
                FormulaCalculate fc = new FormulaCalculate();
                var dbcool = Unit.ConvertToControl(dbCool, UnitType.TEMPERATURE, utTemperature);
                var wbcool = Unit.ConvertToControl(wbCool, UnitType.TEMPERATURE, utTemperature);
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                var indoorCoolingRH = (rh * 100).ToString("n0");
                var dbheat = Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature);
                var wbheat = Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature);
                rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbheat), Convert.ToDecimal(wbheat), pressure));
                var outdoorHeatRH = (rh * 100).ToString("n0");
                outSectionBasic = new
                {
                    dbCool = dbcool.ToString("n1") + " " + utTemperature,
                    wbCool = wbcool.ToString("n1") + " " + utTemperature,
                    indoorCoolingRH = indoorCoolingRH + "%",
                    dbHeat = Unit.ConvertToControl(dbHeat, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature,
                    outdoorCoolingDB = outdoorCoolingDB,
                    outdoorHeatingDB = outdoorHeatingDB,
                    outdoorHeatingWB = outdoorHeatingWB,
                    outdoorHeatRH = outdoorHeatRH + "%",
                    coolingInletWater = coolingInletWater,
                    heatingInletWater = heatingInletWater
                };
            }
        }

        /// <summary>
        ///生成System Design 数据对象
        /// </summary>
        private void BuildSystemDesignData()
        {
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                thisProject.SystemListNextGen.ForEach((s) =>
                {
                    if (s.IsExportToReport == true)
                    {
                        //FindAll((p) => p.SystemID == s.Id);
                        //((JCHVRF.Model.SystemBase)_thisProject.SystemListNextGen[0]).Id; 
                        var indoorList = thisProject.RoomIndoorList.FindAll((p) => p.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                        double dbCool = indoorList[0].DBCooling;
                        double wbCool = 9999;
                        double dbHeat = 0;
                        indoorList.ForEach((d) =>
                        {
                            //取室内机制冷工况的最小值
                            if (wbCool > d.WBCooling)
                            {
                                wbCool = d.WBCooling;
                            }
                            //取室内机制热工况的最大值
                            if (dbHeat < d.DBHeating)
                            {
                                dbHeat = d.DBHeating;
                            }
                        });
                        string WaterSource = s.OutdoorItem.ProductType;
                        double iwCoolingInletWaterOutdoor = s.IWCooling;
                        double iwOutdoorHeatingInletWater = s.IWHeating;
                        double dbCoolOutdoor = s.DBCooling;
                        double dbHeatOutdoor = s.DBHeating;
                        double wbHeatOutdoor = s.WBHeating;
                        FormulaCalculate fc = new FormulaCalculate();
                        var dbcool = Unit.ConvertToControl(dbCool, UnitType.TEMPERATURE, utTemperature);
                        var wbcool = Unit.ConvertToControl(wbCool, UnitType.TEMPERATURE, utTemperature);
                        decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                        double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                        var indoorCoolingRH = (rh * 100).ToString("n0");
                        var dbheat = Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature);
                        var wbheat = Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature);
                        rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbheat), Convert.ToDecimal(wbheat), pressure));
                        var outdoorHeatRH = (rh * 100).ToString("n0");
                        //OutdoorCoolingDB
                        string outdoorCoolingDB = Unit.ConvertToControl(dbCoolOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                        //Outdoor Heating DB
                        string outdoorHeatingDB = Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                        //Outdoor Heating WB
                        string outdoorHeatingWB = Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                        //CoolingInletWater
                        string coolingInletWater = Unit.ConvertToControl(iwCoolingInletWaterOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                        //HeatingInletWater 
                        string heatingInletWater = Unit.ConvertToControl(iwOutdoorHeatingInletWater, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature;
                        if (s.OutdoorItem.ProductType.Contains("Water Source"))
                        {
                            outdoorCoolingDB = "-";
                            //Outdoor Heating DB
                            outdoorHeatingDB = "-";
                            //Outdoor Heating WB
                            outdoorHeatingWB = "-";
                        }
                        else
                        {
                            coolingInletWater = "-";
                            //HeatingInletWater 
                            heatingInletWater = "-";
                        }
                        dynamic sysBasic = new
                        {
                            dbCool = dbcool.ToString("n1") + " " + utTemperature,
                            wbCool = wbcool.ToString("n1") + " " + utTemperature,
                            indoorCoolingRH = indoorCoolingRH + "%",
                            dbHeat = Unit.ConvertToControl(dbHeat, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature,
                            outdoorCoolingDB = outdoorCoolingDB,
                            outdoorHeatingDB = outdoorHeatingDB,
                            outdoorHeatingWB = outdoorHeatingWB,
                            outdoorHeatRH = outdoorHeatRH + "%",
                            coolingInletWater = coolingInletWater,
                            heatingInletWater = heatingInletWater
                        };
                        dynamic total = new System.Dynamic.ExpandoObject();
                        total.TotalActActCap = 0d;
                        total.TotalNominalCap = 0d;
                        total.TotalActSensible = 0d;
                        total.TotalRqCap = 0d;
                        total.TotalActHeat = 0d;
                        total.TotalNominalHeat = 0d;
                        total.TotalRqHeat = 0d;
                        total.NominalSensible = 0d;  //  New Code After Integrate 4.6
                        dynamic totalOutdoor = new System.Dynamic.ExpandoObject();
                        totalOutdoor.NominalCap = 0d;
                        totalOutdoor.ActCap = 0d;
                        totalOutdoor.RqCap = 0d;
                        totalOutdoor.NominalHeat = 0d;
                        totalOutdoor.ActHeat = 0d;
                        totalOutdoor.RqHeat = 0d;
                        var sys = new
                        {
                            SysBasic = sysBasic,
                            SystemName = s.Name,
                            RoomInformation = new { RoomList = new List<dynamic>(), Total = total },
                            OutdoorUnits = new { OutdoorList = new List<dynamic>(), Total = totalOutdoor },
                            IndoorUnits = new { IndoorList = new List<dynamic>() },
                            FreshAirIndoors = new { IndoorList = new List<dynamic>() }
                        };
                        //room
                        if (thisProject.FloorList != null && thisProject.FloorList.Count > 0)
                        {
                            var indoors = thisProject.RoomIndoorList.FindAll(r => r.SystemID == ((JCHVRF.Model.SystemBase)s).Id && r.IndoorItem.Flag == IndoorType.Indoor);
                            thisProject.FloorList.ForEach((f) =>
                            {
                                f.RoomList.ForEach((r) =>
                                {
                                    var indList = indoors.FindAll(ind => ind.RoomID != null && ind.RoomID == r.Id);
                                    if (indList != null && indList.Count > 0)
                                    {
                                        var roomArea = Unit.ConvertToControl(r.Area, UnitType.AREA, utArea).ToString("n1");
                                        var dbCooling = Unit.ConvertToControl(indList[0].DBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                                        var wbCooling = Unit.ConvertToControl(indList[0].WBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                                        var dbHeating = Unit.ConvertToControl(indList[0].DBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                                        var actCap = Unit.ConvertToControl(indList.Sum(m => m.ActualCoolingCapacity), UnitType.POWER, utPower).ToString("n1");
                                        var nominalCap = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.CoolingCapacity), UnitType.POWER, utPower).ToString("n1");
                                        var actSensible = Unit.ConvertToControl(indList.Sum(m => m.ActualSensibleHeat), UnitType.POWER, utPower).ToString("n1");
                                        var rqCap = Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, utPower).ToString("n1");
                                        var actHeat = Unit.ConvertToControl(indList.Sum(m => m.ActualHeatingCapacity), UnitType.POWER, utPower).ToString("n1");
                                        var nominalHeat = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.HeatingCapacity), UnitType.POWER, utPower).ToString("n1");
                                        var rqHeat = Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, utPower).ToString("n1");
                                        var nominalsensible = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.SensibleHeat), UnitType.POWER, utPower).ToString("n1");   //  New Code After Integrate 4.6

                                        //New Code after Integrate 4.7

                                        if (utPower == "kW" && isIduCapacityW == true)
                                        {
                                            actCap = Unit.ConvertToControl(indList.Sum(m => m.ActualCoolingCapacity), UnitType.POWER, "W").ToString("n1");
                                            nominalCap = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.CoolingCapacity), UnitType.POWER, "W").ToString("n1");
                                            actSensible = Unit.ConvertToControl(indList.Sum(m => m.ActualSensibleHeat), UnitType.POWER, "W").ToString("n1");
                                            rqCap = Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, "W").ToString("n1");
                                            actHeat = Unit.ConvertToControl(indList.Sum(m => m.ActualHeatingCapacity), UnitType.POWER, "W").ToString("n1");
                                            nominalHeat = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.HeatingCapacity), UnitType.POWER, "W").ToString("n1");
                                            rqHeat = Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, "W").ToString("n1");
                                            nominalsensible = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.SensibleHeat), UnitType.POWER, "W").ToString("n1");
                                            sys.RoomInformation.Total.TotalActActCap += Unit.ConvertToControl(indList.Sum(m => m.ActualCoolingCapacity), UnitType.POWER, "W");
                                            sys.RoomInformation.Total.TotalNominalCap += Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.CoolingCapacity), UnitType.POWER, "W");
                                            sys.RoomInformation.Total.TotalActSensible += Unit.ConvertToControl(indList.Sum(m => m.ActualSensibleHeat), UnitType.POWER, "W");
                                            sys.RoomInformation.Total.TotalRqCap += Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, "W");
                                            sys.RoomInformation.Total.TotalActHeat += Unit.ConvertToControl(indList.Sum(m => m.ActualHeatingCapacity), UnitType.POWER, "W");
                                            sys.RoomInformation.Total.TotalNominalHeat += Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.HeatingCapacity), UnitType.POWER, "W");
                                            sys.RoomInformation.Total.TotalRqHeat += Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, "W");
                                        }
                                        else
                                        {
                                            sys.RoomInformation.Total.TotalActActCap += Unit.ConvertToControl(indList.Sum(m => m.ActualCoolingCapacity), UnitType.POWER, utPower);
                                            sys.RoomInformation.Total.TotalNominalCap += Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.CoolingCapacity), UnitType.POWER, utPower);
                                            sys.RoomInformation.Total.TotalActSensible += Unit.ConvertToControl(indList.Sum(m => m.ActualSensibleHeat), UnitType.POWER, utPower);
                                            sys.RoomInformation.Total.TotalRqCap += Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, utPower);
                                            sys.RoomInformation.Total.TotalActHeat += Unit.ConvertToControl(indList.Sum(m => m.ActualHeatingCapacity), UnitType.POWER, utPower);
                                            sys.RoomInformation.Total.TotalNominalHeat += Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.HeatingCapacity), UnitType.POWER, utPower);
                                            sys.RoomInformation.Total.TotalRqHeat += Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, utPower);
                                        }
                                        var roomInf = new
                                        {
                                            FloorName = f.Name,
                                            RoomName = r.Name,
                                            RoomArea = roomArea,
                                            DBCool = dbCooling,
                                            WBCool = wbCooling,
                                            DBHeat = dbHeating,
                                            ActCap = Convert.ToDouble(actCap),
                                            NominalCap = Convert.ToDouble(nominalCap),
                                            ActSensible = Convert.ToDouble(actSensible),
                                            NominalSensible = Convert.ToDouble(nominalsensible),  //  New Code After Integrate 4.6 
                                            RqCap = Convert.ToDouble(rqCap),
                                            ActHeat = Convert.ToDouble(actHeat),
                                            NominalHeat = Convert.ToDouble(nominalHeat),
                                            RqHeat = Convert.ToDouble(rqHeat)
                                        };
                                        sys.RoomInformation.RoomList.Add(roomInf);

                                        //indoor
                                        var roomOfInds = new { RoomInfo = roomInf, Indoors = new List<dynamic>() };
                                        indList.ForEach((d) =>
                                        {
                                            Indoor inselected = d.IndoorItem;  //  New Code After Integrate 4.6
                                            double fanspeedval = inselected.GetAirFlow(d.FanSpeedLevel);  //  New Code After Integrate 4.6
                                            string airflowtab = string.Empty;  //  New Code After Integrate 4.6
                                            airflowtab = GetAirFlowTab(inselected.AirFlow_Levels, inselected.Type, fanspeedval);  //  New Code After Integrate 4.6
                                            //New Code After Integrate 4.7

                                            string ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            string NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            string ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                            string NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                            string ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            string NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            if (utPower == "kW" && isIduCapacityW == true)
                                            {
                                                ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                                NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                                ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, "W").ToString("n1");
                                                NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, "W").ToString("n1");
                                                ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                                NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                            }

                                            //End Code After Integrate 4.6
                                            string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                                            var ind = new
                                            {
                                                Name = GetIndoorDisplayName(d) + " " + name,
                                                Ident = d.IndoorName,
                                                SoundPressure = GetSoundPressure(d),
                                                // Previos Code // FanSpeedLevel = d.FanSpeedLevel,
                                                FanSpeedLevel = d.FanSpeedLevel,  //  New Code After Integrate 4.6
                                                FanSpeedValue = Unit.ConvertToControl(fanspeedval, UnitType.AIRFLOW, utAirflow).ToString("n1"),  //  New Code After Integrate 4.6
                                                SelectedAirflow = airflowtab,  //  New Code After Integrate 4.6
                                                //  New Code After Integrate 4.7
                                                ActCap = ActCapL,
                                                NominalCap = NominalCapL,
                                                ActSensible = ActSensibleL,
                                                NominalSensible = NominalSensibleL,
                                                RqCap = "",
                                                ActHeat = ActHeatL,
                                                NominalHeat = NominalHeatL,
                                                //End Code After Integrate 4.7
                                                //ActCap = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n1"),
                                                //NominalCap = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1"),
                                                //ActSensible = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1"),
                                                //NominalSensible = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower).ToString("n1"),  //  New Code After Integrate 4.6
                                                //RqCap = "",
                                                //ActHeat = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n1"),
                                                //NominalHeat = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1"),
                                                RqHeat = ""
                                            };
                                            roomOfInds.Indoors.Add(ind);
                                        });
                                        sys.IndoorUnits.IndoorList.Add(roomOfInds);
                                    }
                                });
                            });
                        }
                        if (thisProject.FreshAirAreaList != null && thisProject.FreshAirAreaList.Count > 0)
                        {
                            var indoors = thisProject.RoomIndoorList.FindAll(r => r.SystemID == ((JCHVRF.Model.SystemBase)s).Id && r.IndoorItem.Flag == IndoorType.FreshAir);
                            thisProject.FreshAirAreaList.ForEach((r) =>
                            {
                                var indList = indoors.FindAll(ind => ind.RoomID != null && ind.RoomID == r.Id);
                                if (indList != null && indList.Count > 0)
                                {
                                    var roomArea = "";
                                    var dbCooling = Unit.ConvertToControl(indList[0].DBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                                    var wbCooling = Unit.ConvertToControl(indList[0].WBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                                    var dbHeating = Unit.ConvertToControl(indList[0].DBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                                    var actCap = Unit.ConvertToControl(indList.Sum(m => m.ActualCoolingCapacity), UnitType.POWER, utPower).ToString("n1");
                                    var nominalCap = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.CoolingCapacity), UnitType.POWER, utPower).ToString("n1");
                                    var actSensible = Unit.ConvertToControl(indList.Sum(m => m.ActualSensibleHeat), UnitType.POWER, utPower).ToString("n1");
                                    var rqCap = Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, utPower).ToString("n1");
                                    var actHeat = Unit.ConvertToControl(indList.Sum(m => m.ActualHeatingCapacity), UnitType.POWER, utPower).ToString("n1");
                                    var nominalHeat = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.HeatingCapacity), UnitType.POWER, utPower).ToString("n1");
                                    var rqHeat = Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, utPower).ToString("n1");
                                    var airFlow = Unit.ConvertToControl(indList.Sum(m => m.AirFlow), UnitType.AIRFLOW, utAirflow).ToString("n0");
                                    var nominalsensible = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.SensibleHeat), UnitType.POWER, utPower).ToString("n1"); //  //  New Code After Integrate 4.6
                                                                                                                                                                      //sys.RoomInformation.Total.TotalActActCap += Convert.ToDouble(actCap);
                                                                                                                                                                      //sys.RoomInformation.Total.TotalActSensible += Convert.ToDouble(actSensible);
                                                                                                                                                                      //sys.RoomInformation.Total.TotalRqCap += Convert.ToDouble(rqCap);
                                                                                                                                                                      //sys.RoomInformation.Total.TotalActHeat += Convert.ToDouble(actHeat);
                                                                                                                                                                      //sys.RoomInformation.Total.TotalRqHeat += Convert.ToDouble(rqHeat);
                                                                                                                                                                      //  New Code After Integrate 4.7
                                    if (utPower == "kW" && isIduCapacityW == true)
                                    {
                                        actCap = Unit.ConvertToControl(indList.Sum(m => m.ActualCoolingCapacity), UnitType.POWER, "W").ToString("n1");
                                        nominalCap = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.CoolingCapacity), UnitType.POWER, "W").ToString("n1");
                                        actSensible = Unit.ConvertToControl(indList.Sum(m => m.ActualSensibleHeat), UnitType.POWER, "W").ToString("n1");
                                        rqCap = Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, "W").ToString("n1");
                                        actHeat = Unit.ConvertToControl(indList.Sum(m => m.ActualHeatingCapacity), UnitType.POWER, "W").ToString("n1");
                                        nominalHeat = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.HeatingCapacity), UnitType.POWER, "W").ToString("n1");
                                        rqHeat = Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, "W").ToString("n1");
                                        nominalsensible = Unit.ConvertToControl(indList.Sum(m => m.IndoorItem.SensibleHeat), UnitType.POWER, "W").ToString("n1");
                                    }
                                    //  End Code After Integrate 4.7
                                    var roomInf = new
                                    {
                                        FloorName = "",
                                        RoomName = r.Name,
                                        RoomArea = roomArea,
                                        DBCool = dbCooling,
                                        WBCool = wbCooling,
                                        DBHeat = dbHeating,
                                        ActCap = Convert.ToDouble(actCap),
                                        NominalCap = Convert.ToDouble(nominalCap),
                                        ActSensible = Convert.ToDouble(actSensible),
                                        NominalSensible = Convert.ToDouble(nominalsensible), // //  New Code After Integrate 4.6 
                                        RqCap = Convert.ToDouble(rqCap),
                                        ActHeat = Convert.ToDouble(actHeat),
                                        NominalHeat = Convert.ToDouble(nominalHeat),
                                        RqHeat = Convert.ToDouble(rqHeat),
                                        AirFlow = Convert.ToDouble(airFlow)
                                    };
                                    //sys.RoomInformation.RoomList.Add(roomInf);

                                    //indoor
                                    var roomOfInds = new { RoomInfo = roomInf, Indoors = new List<dynamic>() };
                                    indList.ForEach((d) =>
                                    {
                                        Indoor inselected = d.IndoorItem; //  New Code After Integrate 4.6
                                        double fanspeedval = inselected.GetAirFlow(d.FanSpeedLevel); //  New Code After Integrate 4.6
                                        string airflowtab = string.Empty; //  New Code After Integrate 4.6
                                        airflowtab = GetAirFlowTab(inselected.AirFlow_Levels, inselected.Type, fanspeedval); //  New Code After Integrate 4.6
                                                                                                                             //  New Code After Integrate 4.7
                                        string ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                        string NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                        string ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");

                                        if (utPower == "kW" && isIduCapacityW == true)
                                        {
                                            ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                            NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                            ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                            NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        }
                                        //  End Code After Integrate 4.7

                                        string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                                        var ind = new
                                        {
                                            Name = GetIndoorDisplayName(d) + " " + name,
                                            Ident = d.IndoorName,
                                            SoundPressure = GetSoundPressure(d),
                                            FanSpeedLevel = d.FanSpeedLevel,
                                            FanSpeedValue = Unit.ConvertToControl(fanspeedval, UnitType.AIRFLOW, utAirflow).ToString("n1"), //  New Code After Integrate 4.6
                                            SelectedAirflow = airflowtab, //  New Code After Integrate 4.6
                                            //  New Code After Integrate 4.7
                                            ActCap = ActCapL,
                                            NominalCap = NominalCapL,
                                            ActSensible = ActSensibleL,
                                            NominalSensible = NominalSensibleL,
                                            RqCap = "",
                                            ActHeat = ActHeatL,
                                            NominalHeat = NominalHeatL,
                                            //  End Code After Integrate 4.7
                                            RqHeat = "",
                                            AirFlow = Unit.ConvertToControl(d.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0")
                                        };
                                        roomOfInds.Indoors.Add(ind);
                                    });
                                    sys.FreshAirIndoors.IndoorList.Add(roomOfInds);
                                }
                            });
                        }

                        //indoor
                        if (thisProject.RoomIndoorList != null && thisProject.RoomIndoorList.Count > 0)
                        {
                            var roomOfInds = new { RoomInfo = new System.Dynamic.ExpandoObject(), Indoors = new List<dynamic>() };
                            dynamic roominfo = roomOfInds.RoomInfo;
                            roominfo.ActCap = 0d;
                            roominfo.NominalCap = 0d;
                            roominfo.ActSensible = 0d;
                            roominfo.NominalSensible = 0d; // //  New Code After Integrate 4.6
                            roominfo.ActHeat = 0d;
                            roominfo.NominalHeat = 0d;
                            roominfo.RqCap = 0d;
                            roominfo.RqHeat = 0d;
                            roominfo.RoomName = "Total";
                            roominfo.FloorName = "";
                            dynamic roomOfIndsAirFlow = new { RoomInfo = new System.Dynamic.ExpandoObject(), Indoors = new List<dynamic>() };
                            dynamic roominfoAirFlow = roomOfIndsAirFlow.RoomInfo;
                            roominfoAirFlow.AirFlow = 0d;
                            roominfoAirFlow.ActCap = 0d;
                            roominfoAirFlow.NominalCap = 0d;
                            roominfoAirFlow.ActSensible = 0d;
                            roominfoAirFlow.NominalSensible = 0d;  //  New Code After Integrate 4.6
                            roominfoAirFlow.ActHeat = 0d;
                            roominfoAirFlow.NominalHeat = 0d;
                            roominfoAirFlow.RqCap = 0d;
                            roominfoAirFlow.RqHeat = 0d;
                            roominfoAirFlow.RoomName = "Total";
                            roominfoAirFlow.FloorName = "";
                            var indoors = thisProject.RoomIndoorList.FindAll(r => r.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                            indoors.ForEach((d) =>
                            {
                                Indoor inselected = d.IndoorItem; //  New Code After Integrate 4.6
                                double fanspeedval = inselected.GetAirFlow(d.FanSpeedLevel); //  New Code After Integrate 4.6
                                string airflowtab = string.Empty; //  New Code After Integrate 4.6
                                airflowtab = GetAirFlowTab(inselected.AirFlow_Levels, inselected.Type, fanspeedval); //  New Code After Integrate 4.6

                                if (d.IndoorItem.Flag == IndoorType.Indoor)  //in FER1 room is not in scope
                                {
                                    string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                                    //  New Code After Integrate 4.7
                                    string ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    string NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    string ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                    string NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                    string RqCapL = Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    string ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    string NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    string RqHeatL = Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                    if (utPower == "kW" && isIduCapacityW == true)
                                    {
                                        ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                        NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                        ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, "W").ToString("n1");
                                        NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, "W").ToString("n1");
                                        RqCapL = Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                        ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                        NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                        RqHeatL = Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                    }
                                    //  End Code After Integrate 4.7

                                    var ind = new
                                    {
                                        Name = GetIndoorDisplayName(d) + " " + name,
                                        Ident = d.IndoorName,
                                        SoundPressure = GetSoundPressure(d),
                                        FanSpeedLevel = d.FanSpeedLevel,
                                        FanSpeedValue = Unit.ConvertToControl(fanspeedval, UnitType.AIRFLOW, utAirflow).ToString("n1"), //  New Code After Integrate 4.6
                                        SelectedAirflow = airflowtab,
                                        //  New Code After Integrate 4.7
                                        ActCap = ActCapL,
                                        NominalCap = NominalCapL,
                                        ActSensible = ActSensibleL,
                                        NominalSensible = NominalSensibleL,
                                        RqCap = RqCapL,
                                        ActHeat = ActHeatL,
                                        NominalHeat = NominalHeatL,
                                        RqHeat = RqHeatL
                                        //  End Code After Integrate 4.7
                                    };
                                    //  New Code After Integrate 4.7
                                    if (utPower == "kW" && isIduCapacityW == true)
                                    {
                                        roominfo.ActCap += Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, "W");
                                        roominfo.NominalCap += Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, "W");
                                        roominfo.ActSensible += Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, "W");
                                        roominfo.NominalSensible += Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, "W");
                                        roominfo.ActHeat += Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, "W");
                                        roominfo.NominalHeat += Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, "W");
                                        roominfo.RqCap += Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, "W");
                                        roominfo.RqHeat += Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, "W");
                                    }
                                    //  End Code After Integrate 4.7
                                    else
                                    {
                                        roominfo.ActCap += Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower);
                                        roominfo.NominalCap += Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower);
                                        roominfo.ActSensible += Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower);
                                        roominfo.NominalSensible += Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower);//  New Code After Integrate 4.6 
                                        roominfo.ActHeat += Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower);
                                        roominfo.NominalHeat += Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower);
                                        roominfo.RqCap += Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, utPower);
                                        roominfo.RqHeat += Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, utPower);
                                    }
                                    roomOfInds.Indoors.Add(ind);
                                }
                                else if (d.IndoorItem.Flag == IndoorType.FreshAir)
                                {
                                    var item = thisProject.FreshAirAreaList.FindAll(f => f.Id == d.RoomID);
                                    if (item == null || item.Count == 0)
                                    {
                                        string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                                        //  New Code After Integrate 4.7
                                        string ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                        string NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                        string RqCapL = Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        string RqHeatL = Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                                        if (utPower == "kW" && isIduCapacityW == true)
                                        {
                                            ActCapL = Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                            NominalCapL = Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                            ActSensibleL = Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, "W").ToString("n1");
                                            NominalSensibleL = Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, "W").ToString("n1");
                                            RqCapL = Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, "W").ToString("n1");
                                            ActHeatL = Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                            NominalHeatL = Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                            RqHeatL = Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, "W").ToString("n1");
                                        }
                                        //  End Code After Integrate 4.7

                                        var ind = new
                                        {
                                            Name = GetIndoorDisplayName(d) + " " + name,
                                            Ident = d.IndoorName,
                                            SoundPressure = GetSoundPressure(d),
                                            FanSpeedLevel = d.FanSpeedLevel,
                                            FanSpeedValue = Unit.ConvertToControl(fanspeedval, UnitType.AIRFLOW, utAirflow).ToString("n1"),//  New Code After Integrate 4.6
                                            SelectedAirflow = airflowtab,//  New Code After Integrate 4.6
                                            //  New Code After Integrate 4.7
                                            ActCap = ActCapL,
                                            NominalCap = NominalCapL,
                                            ActSensible = ActSensibleL,
                                            NominalSensible = NominalSensibleL,
                                            RqCap = RqCapL,
                                            ActHeat = ActHeatL,
                                            NominalHeat = NominalHeatL,
                                            RqHeat = RqHeatL,
                                            //  End Code After Integrate 4.7
                                            AirFlow = Unit.ConvertToControl(d.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0")
                                        };
                                        //  New Code After Integrate 4.7
                                        roominfoAirFlow.AirFlow += Unit.ConvertToControl(d.AirFlow, UnitType.AIRFLOW, utAirflow);
                                        if (utPower == "kW" && isIduCapacityW == true)
                                        {
                                            roominfoAirFlow.ActCap += Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, "W");
                                            roominfoAirFlow.NominalCap += Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, "W");
                                            roominfoAirFlow.ActSensible += Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, "W");
                                            roominfoAirFlow.NominalSensible += Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, "W");
                                            roominfoAirFlow.ActHeat += Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, "W");
                                            roominfoAirFlow.NominalHeat += Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, "W");
                                            roominfoAirFlow.RqCap += Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, "W");
                                            roominfoAirFlow.RqHeat += Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, "W");
                                        }
                                        //  End Code After Integrate 4.7
                                        else
                                        {

                                            roominfoAirFlow.ActCap += Unit.ConvertToControl(d.ActualCoolingCapacity, UnitType.POWER, utPower);
                                            roominfoAirFlow.NominalCap += Unit.ConvertToControl(d.IndoorItem.CoolingCapacity, UnitType.POWER, utPower);
                                            roominfoAirFlow.ActSensible += Unit.ConvertToControl(d.ActualSensibleHeat, UnitType.POWER, utPower);
                                            roominfoAirFlow.NominalSensible += Unit.ConvertToControl(d.IndoorItem.SensibleHeat, UnitType.POWER, utPower); //  New Code After Integrate 4.6
                                            roominfoAirFlow.ActHeat += Unit.ConvertToControl(d.ActualHeatingCapacity, UnitType.POWER, utPower);
                                            roominfoAirFlow.NominalHeat += Unit.ConvertToControl(d.IndoorItem.HeatingCapacity, UnitType.POWER, utPower);
                                            // roominfoAirFlow.AirFlow += Unit.ConvertToControl(d.AirFlow, UnitType.AIRFLOW, utAirflow);//  New Code After Integrate 4.7
                                            roominfoAirFlow.RqCap += Unit.ConvertToControl(d.RqCoolingCapacity, UnitType.POWER, utPower);
                                            roominfoAirFlow.RqHeat += Unit.ConvertToControl(d.RqHeatingCapacity, UnitType.POWER, utPower);
                                        }
                                        roomOfIndsAirFlow.Indoors.Add(ind);
                                    }
                                }
                            });
                            if (roomOfInds.Indoors.Count > 0)
                            {
                                sys.IndoorUnits.IndoorList.Add(roomOfInds);
                            }
                            if (roomOfIndsAirFlow.Indoors.Count > 0)
                            {
                                sys.FreshAirIndoors.IndoorList.Add(roomOfIndsAirFlow);
                            }
                        }

                        //outdoor
                        var outdoor = new
                        {
                            // Previos Code //  Name = s.OutdoorItem.Series + " " + s.OutdoorItem.AuxModelName,
                            Name = trans.getTypeTransStr(TransType.Series.ToString(), s.OutdoorItem.Series) + " " + s.OutdoorItem.AuxModelName,  //  New Code After Integrate 4.6
                            ActRatio = (s.Ratio * 100).ToString("n0"),
                            MaxRatio = (s.MaxRatio * 100).ToString("n0"),
                            NominalCap = Unit.ConvertToControl(s.OutdoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1"),
                            ActCap = Unit.ConvertToControl(s.CoolingCapacity, UnitType.POWER, utPower).ToString("n1"),
                            RqCap = "0",
                            NominalHeat = Unit.ConvertToControl(s.OutdoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1"),
                            ActHeat = Unit.ConvertToControl(s.HeatingCapacity, UnitType.POWER, utPower).ToString("n1"),
                            RqHeat = "0"
                        };
                        var inds = sys.IndoorUnits.IndoorList;
                        var indsFA = sys.FreshAirIndoors.IndoorList;
                        double totalRqCap = 0;
                        double totalRqHeat = 0;
                        inds.ForEach((d) =>
                        {
                            totalRqCap += Convert.ToDouble(d.RoomInfo.RqCap);
                            totalRqHeat += Convert.ToDouble(d.RoomInfo.RqHeat);
                        });
                        indsFA.ForEach((d) =>
                        {
                            totalRqCap += Convert.ToDouble(d.RoomInfo.RqCap);
                            totalRqHeat += Convert.ToDouble(d.RoomInfo.RqHeat);
                        });
                        sys.OutdoorUnits.OutdoorList.Add(outdoor);
                        sys.OutdoorUnits.Total.NominalCap = Convert.ToDouble(outdoor.NominalCap);
                        sys.OutdoorUnits.Total.ActCap = Convert.ToDouble(outdoor.ActCap);
                        sys.OutdoorUnits.Total.RqCap = totalRqCap;
                        sys.OutdoorUnits.Total.NominalHeat = Convert.ToDouble(outdoor.NominalHeat);
                        sys.OutdoorUnits.Total.ActHeat = Convert.ToDouble(outdoor.ActHeat);
                        sys.OutdoorUnits.Total.RqHeat = totalRqHeat;


                        systemList.Add(sys);
                    }
                });
            }
        }

        /// <summary>
        /// 生成Piping Design 数据对象
        /// </summary>
        private void BuildPipingDesignData()
        {
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                thisProject.SystemListNextGen.ForEach((s) =>
                {
                    if (s.IsExportToReport == true)
                    {
                        var inds = thisProject.RoomIndoorList.FindAll(p => p.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                        int indsCount = 0;
                        if (inds != null && inds.Count > 0)
                        {
                            indsCount = inds.Count;
                        }

                        var lengthList = new List<dynamic>();
                        var heightList = new List<dynamic>();
                        var pipingRules = new { Length = lengthList, Height = heightList };
                        int totalLengthCount = 0;
                        int totalHeightCount = 0;
                        lengthList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_TotalPipeLength"),//"Total pipe length",
                            Actual = Unit.ConvertToControl(s.TotalPipeLength, UnitType.LENGTH_M, utLength).ToString("n0"),
                            Max = s.MaxTotalPipeLength == 0 ? "-" : Unit.ConvertToControl(s.MaxTotalPipeLength, UnitType.LENGTH_M, utLength).ToString("n0")
                        });
                        lengthList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_PipeActualLength"),//"Maximum piping length (Actual length)",
                            Actual = Unit.ConvertToControl(s.PipeActualLength, UnitType.LENGTH_M, utLength).ToString("n0"),
                            Max = s.MaxPipeLength == 0 ? "-" : Unit.ConvertToControl(s.MaxPipeLength, UnitType.LENGTH_M, utLength).ToString("n0")
                        });
                        lengthList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_PipeEquivalentLength"),//"Maximum piping length (Equivalent length)",
                            Actual = Unit.ConvertToControl(s.PipeEquivalentLength, UnitType.LENGTH_M, utLength).ToString("n0"),
                            Max = s.MaxEqPipeLength == 0 ? "-" : Unit.ConvertToControl(s.MaxEqPipeLength, UnitType.LENGTH_M, utLength).ToString("n0")
                        });
                        lengthList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_FirstPipeLength"),//"Maximum Piping Length between Multi-kit of 1st Branch and Each Indoor Unit",
                            Actual = Unit.ConvertToControl(s.FirstPipeLength, UnitType.LENGTH_M, utLength).ToString("n0"),
                            Max = s.MaxIndoorLength == 0 ? "-" : Unit.ConvertToControl(s.MaxIndoorLength, UnitType.LENGTH_M, utLength).ToString("n0")
                        });
                        lengthList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_ActualMaxMKIndoorPipeLength"),//"Maximum Piping Length between Each Multi-kit and Each Indoor Unit",
                            Actual = Unit.ConvertToControl(s.ActualMaxMKIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0"),
                            Max = s.MaxMKIndoorPipeLength == 0 ? "-" : Unit.ConvertToControl(s.MaxMKIndoorPipeLength, UnitType.LENGTH_M, utLength).ToString("n0")
                        });

                        //单独的室外机不需要显示该项限制 20171230 by Yunxiao Lin
                        if (!string.IsNullOrEmpty((s.OutdoorItem.JointKitModelG == null ? "" : s.OutdoorItem.JointKitModelG).Trim()) && (s.OutdoorItem.JointKitModelG == null ? "" : s.OutdoorItem.JointKitModelG).Trim() != "-")
                        {
                            string MaxPipeLengths = "0";
                            if (s.IsInputLengthManually)
                            {
                                MaxPipeLengths = Unit.ConvertToControl(NGPipBLL.PipingBLL.GetMaxPipeLengthOfNodeOut(s.MyPipingNodeOut), UnitType.LENGTH_M, utLength).ToString("n0");
                            }
                            lengthList.Add(new
                            {
                                Rules = Msg.GetResourceString("REPORT2_PipingRules_PipeLengthes"),//"Piping Length between Piping Connection Kit 1 and Each Outdoor Unit",
                                // Previos Code //  Actual = Unit.ConvertToControl(s.MyPipingNodeOut.PipeLengthes == null ? 0 : GetMaxPipingNodeOut(s.MyPipingNodeOut.PipeLengthes, s.MyPipingNodeOut), UnitType.LENGTH_M, utLength).ToString("n0"),
                                Actual = MaxPipeLengths, //  New Code After Integrate 4.6
                                Max = s.MaxFirstConnectionKitToEachODU == 0 ? "-" : Unit.ConvertToControl(s.MaxFirstConnectionKitToEachODU, UnitType.LENGTH_M, utLength).ToString("n0")
                            });
                        }

                        heightList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_HeightDiffH"),//"Height Difference between (O.U. is Higher)", 
                                                                                             // Maximum height difference between outdoor unit and  indoor units on 20180502 by xyj
                            Actual = Unit.ConvertToControl(s.MaxUpperHeightDifferenceLength, UnitType.LENGTH_M, utLength).ToString("0.#"),
                            Max = s.MaxOutdoorAboveHeight == 0 ? "-" : Unit.ConvertToControl(s.MaxOutdoorAboveHeight, UnitType.LENGTH_M, utLength).ToString("0.#")
                        });
                        heightList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_HeightDiffL"),//"Height Difference between (O.U. is Lower)",
                                                                                             // Maximum height difference between outdoor unit and  indoor units on 20180502 by xyj
                            Actual = Unit.ConvertToControl(s.MaxLowerHeightDifferenceLength, UnitType.LENGTH_M, utLength).ToString("0.#"),
                            Max = s.MaxOutdoorBelowHeight == 0 ? "-" : Unit.ConvertToControl(s.MaxOutdoorBelowHeight, UnitType.LENGTH_M, utLength).ToString("0.#")
                        });
                        heightList.Add(new
                        {
                            Rules = Msg.GetResourceString("REPORT2_PipingRules_DiffIndoorHeight"),//"Height Difference between Indoor Units", 
                            Actual = Unit.ConvertToControl(s.MaxIndoorHeightDifferenceLength, UnitType.LENGTH_M, utLength).ToString("0.#"),  //高度差0 改为每个室内单元之间的最大高度差 
                            Max = s.MaxDiffIndoorHeight == 0 ? "-" : Unit.ConvertToControl(s.MaxDiffIndoorHeight, UnitType.LENGTH_M, utLength).ToString("0.#")
                        });
                        if (s.OutdoorItem.ProductType.Contains("Heat Recovery") || s.OutdoorItem.ProductType.Contains(", HR"))
                        {
                            heightList.Add(new
                            {
                                Rules = Msg.GetResourceString("REPORT2_PipingRules_DiffCHBox_IndoorHeight"),//"Height Difference between CH-Box and Indoor Units", 
                                Actual = Unit.ConvertToControl(s.MaxCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#"),  //高度差0 改为CHBox 到室内单元之间的最大高度差 
                                Max = s.NormalCHBox_IndoorHighDiffLength == 0 ? "-" : Unit.ConvertToControl(s.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#")
                            });
                            heightList.Add(new
                            {
                                Rules = Msg.GetResourceString("REPORT2_PipingRules_DiffMulitBoxHeight"),//"Height Difference between Indoor Units using the Same Branch of CH-Box", 
                                Actual = Unit.ConvertToControl(s.MaxSameCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#"),  //高度差0 使用CH-Box的同一分支的室内机之间的高度差 
                                Max = s.NormalSameCHBoxHighDiffLength == 0 ? "-" : Unit.ConvertToControl(s.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#")
                            });
                            heightList.Add(new
                            {
                                Rules = Msg.GetResourceString("REPORT2_PipingRules_DiffCHBoxHeight"),//"Height Difference between CH-Boxes", 
                                Actual = Unit.ConvertToControl(s.MaxCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#"),  //高度差0 改为CHBox的最大高度差 
                                Max = s.NormalCHBoxHighDiffLength == 0 ? "-" : Unit.ConvertToControl(s.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, utLength).ToString("0.#")
                            });
                        }
                        totalLengthCount = lengthList.Count;
                        totalHeightCount = heightList.Count;
                        if (s.OutdoorItem.ProductType.Contains("Heat Recovery") || s.OutdoorItem.ProductType.Contains(", HR"))
                        {
                            bool isAllOK = true; //  New Code After Integrate 4.6
                            var chBoxs = new List<dynamic>();
                            NGPipBLL.PipingBLL.EachNode(s.MyPipingNodeOut, (node1) =>
                            {
                                double actual;
                                double max;
                                string model;
                                if (node1 is NextGenModel.MyNodeCH)
                                {
                                    var item = (NextGenModel.MyNodeCH)node1;
                                    actual = item.ActualTotalCHIndoorPipeLength;
                                    max = item.MaxTotalCHIndoorPipeLength;
                                    model = item.Model;
                                }
                                else if (node1 is NextGenModel.MyNodeMultiCH)
                                {
                                    var item = (NextGenModel.MyNodeMultiCH)node1;
                                    actual = item.ActualTotalCHIndoorPipeLength;
                                    max = item.MaxTotalCHIndoorPipeLength;
                                    model = item.Model;
                                }
                                else
                                {
                                    return;
                                }

                                bool isOK = !(max > 0 && actual > max);
                                isAllOK = isAllOK && isOK;

                                var chbox = chBoxs.Find(p => p.Rules == model);
                                if (chbox == null)
                                {
                                    chbox = new System.Dynamic.ExpandoObject();
                                    chbox.Rules = model;
                                    chbox.Actual = actual;
                                    chbox.Max = max;
                                    chbox.isOK = isOK;
                                    chBoxs.Add(chbox);
                                }
                                else
                                {
                                    if ((chbox.isOK && isOK && actual > chbox.Actual)  //高的覆盖低的
                                        || (chbox.isOK && !isOK))   //出错的覆盖正常的
                                    {
                                        chbox.Actual = actual;
                                        chbox.Max = max;
                                        chbox.isOK = isOK;
                                    }
                                }
                            });
                            if (chBoxs.Count > 0)
                            {
                                lengthList.Add(new
                                {
                                    Rules = Msg.GetResourceString("REPORT2_PipingRules_CHBoxs"),//"Total piping length between CH-Box and Each Indoor Unit",
                                    Actual = "-",
                                    Max = "-",
                                    CHBoxs = chBoxs
                                });
                                totalLengthCount += chBoxs.Count + 1;
                            }
                        }
                        string Ratio = "50%-130%";
                        if (s.OutdoorItem.Series.Contains("FSNP") || s.OutdoorItem.Series.Contains("FSXNP")
                             || s.OutdoorItem.Series.Contains("FSNS7B") || s.OutdoorItem.Series.Contains("FSNS5B")
                             || s.OutdoorItem.Series.Contains("FSNC7B") || s.OutdoorItem.Series.Contains("FSNC5B") //巴西的Connection Ratio可以达到150% 20190105 by Yunxiao Lin
                             )
                            Ratio = "50%-150%";

                        if (s.OutdoorItem.Series.Contains("FSXNPE"))
                        {
                            if (s.OutdoorItem.CoolingCapacity > 150)
                            {
                                Ratio = "50%-130%";
                            }
                        }

                        var piping = new
                        {
                            Name = s.Name,
                            // Previos Code // OutdoorName = s.OutdoorItem.Series + " " + s.OutdoorItem.AuxModelName,
                            OutdoorName = trans.getTypeTransStr(TransType.Series.ToString(), s.OutdoorItem.Series) + " " + s.OutdoorItem.AuxModelName, //  //  New Code After Integrate 4.6
                            RecommendedIU = s.OutdoorItem.RecommendedIU,
                            MaxIU = s.OutdoorItem.MaxIU,
                            IUCount = indsCount,
                            Rate = (s.Ratio * 100).ToString("n0") + "%",
                            RateRange = Ratio,//+ (s.MaxRatio * 100).ToString("n0") + "%",

                            PipingDiagram = FileLocal.GetNamePathPipingPicture(s.Id),

                            PipingRules = pipingRules,
                            RefrigerantLoad = new
                            {
                                Before = Unit.ConvertToControl(s.OutdoorItem.RefrigerantCharge, UnitType.WEIGHT, utWeight).ToString("n1"),
                                Add = Unit.ConvertToControl(s.AddRefrigeration, UnitType.WEIGHT, utWeight).ToString("n1"),
                                Total = Unit.ConvertToControl((s.OutdoorItem.RefrigerantCharge + s.AddRefrigeration), UnitType.WEIGHT, utWeight).ToString("n1")
                            },
                            Is2Pipes = (s.OutdoorItem.ProductType.Contains("Heat Recovery") || s.OutdoorItem.ProductType.Contains(", HR")) ? false : true,
                            TotalLengthCount = totalLengthCount,
                            TotalHeightCount = totalHeightCount,
                            Actuallength = s.PipeActualLength
                        };
                        pipingList.Add(piping);
                    }
                });
            }
        }
        private double GetMaxPipingNodeOut(double[] _pipeLengthes, NGModel.MyNodeOut _nodeOut)
        {
            int index = 0;
            double b = 0, c = 0, d = 0, e = 0, f = 0, g = 0, max = 0;
            if (_nodeOut.UnitCount >= 2)
            {
                if (_nodeOut.UnitCount >= 4)
                {
                    c = _pipeLengthes[index++];
                    b = _pipeLengthes[index++];
                }
                else
                {
                    b = _pipeLengthes[index++];
                    //if (_nodeOut.UnitCount >= 4)
                    //{
                    //    //4个机组有2个b
                    //    index++;
                    //}
                    c = _pipeLengthes[index++];
                }
                if (_nodeOut.UnitCount == 2)
                {
                    max = (new double[] { b, c }).Max();
                }
            }
            if (_nodeOut.UnitCount >= 3)
            {
                d = _pipeLengthes[index++];
                e = _pipeLengthes[index++];
                if (_nodeOut.UnitCount == 3)
                {
                    max = (new double[] { c, b + e, b + d }).Max();
                }
            }
            if (_nodeOut.UnitCount >= 4)
            {
                f = _pipeLengthes[index++];
                g = _pipeLengthes[index++];
                if (_nodeOut.UnitCount == 4)
                {
                    max = (new double[] { b + d, b + e, c + f, c + g }).Max();
                }
            }
            return max;
        }

        /// <summary>
        /// 生成Wiring Design 数据对象
        /// </summary>
        private void BuildWiringDesignData()
        {
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                thisProject.SystemListNextGen.ForEach((s) =>
                {
                    if (s.IsExportToReport == true)
                    {
                        Hashtable htChkExists = new Hashtable();
                        var wiring = new
                        {
                            Name = s.Name,
                            WiringDiagram = FileLocal.GetNamePathWiringPicture(s.Id),
                            PowerSupply = new List<dynamic>()
                        };
                        if (s.OutdoorItem != null)
                        {
                            string combinedmaxcurr = string.Empty;

                            OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
                            if ((s.OutdoorItem.FullModuleName.Contains("+") || s.OutdoorItem.FullModuleName.Contains("*2") || s.OutdoorItem.FullModuleName.Contains("*3") || s.OutdoorItem.FullModuleName.Contains("*4"))
                            && (s.OutdoorItem.Type == "FSNYW1Q (Water source)" ||
                            s.OutdoorItem.Type == "HNCQ (Top flow, 3Ph)" || s.OutdoorItem.Type == "CNCQ (Top flow, 3Ph)" ||
                            s.OutdoorItem.Type == "HNBQ (Top flow, 3Ph)" || s.OutdoorItem.Type == "JNBBQ (Top flow, 3Ph)" ||
                            s.OutdoorItem.Type == "JTOH (All Inverter)" || s.OutdoorItem.Type == "JVOHQ (Top flow, 3Ph)"))
                            {
                                dynamic dyn = new System.Dynamic.ExpandoObject();
                                dyn = bll.GetOutdoorCombinedDetail(s.OutdoorItem.FullModuleName, s.OutdoorItem.Series);
                                combinedmaxcurr = dyn.Maxcurrent;
                            }
                            else
                            {
                                combinedmaxcurr = s.OutdoorItem.MaxCurrent.ToString();
                            }

                            MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, s.Power);
                            // Previos Code //  var item = new { Picture = s.OutdoorItem.TypeImage, Model = s.OutdoorItem.AuxModelName, PowerSupply = powerItem != null ? powerItem.Name : "", InputPower = s.OutdoorItem.Power_Cooling, MaxCurrent = s.OutdoorItem.MaxCurrent };
                            var item = new { Picture = s.OutdoorItem.TypeImage, Model = s.OutdoorItem.AuxModelName, PowerSupply = powerItem != null ? powerItem.Name : "", InputPower = s.OutdoorItem.Power_Cooling, MaxCurrent = combinedmaxcurr };   //  New Code After Integrate 4.6
                            wiring.PowerSupply.Add(item);
                        }
                        var indoors = thisProject.RoomIndoorList.FindAll(r => r.SystemID == ((JCHVRF.Model.SystemBase)s).Id);
                        indoors.ForEach((d) =>
                        {
                            string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                            if (htChkExists[name] == null)
                            {
                                MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, d.IndoorItem.ModelFull.Substring(10, 1));
                                double powerInput = d.IndoorItem.PowerInput_Cooling;
                                //目前EU以外区域的IDU的power input是w作为单位，需要转换为Kw  add by Shen Junjie on 2018/8/15
                                if (!ProjectBLL.IsIDUPowerInputKw(thisProject.RegionCode))
                                {
                                    powerInput = Math.Round(powerInput / 1000, 2);
                                }
                                var item = new { Picture = d.IndoorItem.TypeImage, Model = name, PowerSupply = powerItem != null ? powerItem.Name : "", InputPower = powerInput, MaxCurrent = d.IndoorItem.RatedCurrent };
                                wiring.PowerSupply.Add(item);
                                htChkExists[name] = "ok";
                            }
                        });
                        wiringList.Add(wiring);
                    }
                });
            }
            if (thisProject.ControllerList != null && thisProject.ControllerList.Count > 0)
            {
                thisProject.ControlGroupList.ForEach((c) =>
                {
                    if (c.IsValidGrp)
                    {
                        if (c.Name.Contains("Central Station"))
                        {
                            centralControllerNum++;
                        }
                        var wiring = new
                        {
                            Name = c.Name,
                            WiringDiagram = FileLocal.GetNamePathWiringPicture(c.Id),
                            PowerSupply = new List<dynamic>()
                        };
                        wiringList.Add(wiring);
                    }
                });
            }
        }
        private List<string> SplitOutdoorFullModuleName(string fullModuleName)
        {
            List<string> models = new List<string>();
            string[] arr1;
            if (fullModuleName.Contains("+"))
            {
                arr1 = fullModuleName.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                arr1 = new string[] { fullModuleName };
            }
            foreach (string str1 in arr1)
            {
                string str2 = str1.Trim();
                if (!string.IsNullOrEmpty(str2))
                {
                    //JCHVRF 中不仅仅有 model1+model2+...，还会有 model1+model2*2+.... Yunxiao Lin
                    if (str2.Contains("*"))
                    {
                        string[] arr2 = str2.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr2.Length == 2)
                        {
                            int num = 0;
                            string str3 = arr2[0].Trim();
                            if (!string.IsNullOrEmpty(str3) && int.TryParse(arr2[1].Trim(), out num))
                            {
                                for (int i = 0; i < num; i++)
                                {
                                    models.Add(str3);
                                }
                            }
                        }
                    }
                    else
                    {
                        models.Add(str2);
                    }
                }
            }
            return models;
        }

        private void BuildProductListData()
        {
            NGPipBLL.PipingBLL pipBll = new NGPipBLL.PipingBLL(thisProject);
            bool isHitachi = thisProject.BrandCode == "H" ? true : false;
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                //室外机

                thisProject.SystemListNextGen.ForEach((s) =>
                {
                    if (s.IsExportToReport)
                    {
                        var outdoor = listOutdoors.Find(p => p.Name == s.OutdoorItem.AuxModelName && p.SystemId == s.Id && p.IsExportToReport == true);
                        if (outdoor == null)
                        {
                            outdoor = new System.Dynamic.ExpandoObject();
                            outdoor.Name = s.OutdoorItem.AuxModelName;
                            outdoor.System = s.Name;
                            outdoor.SystemId = s.Id;
                            outdoor.Series = s.OutdoorItem.Series;
                            outdoor.Qty = 1;
                            outdoor.Components = new List<dynamic>();
                            listOutdoors.Add(outdoor);
                        }
                        else
                        {
                            outdoor.Qty++;
                        }

                        string fullname = s.OutdoorItem.FullModuleName;
                        List<dynamic> components = outdoor.Components;
                        List<string> subModelNames = SplitOutdoorFullModuleName(fullname);
                        if (subModelNames.Count > 1)
                        {
                            subModelNames.ForEach(model =>
                            {
                                dynamic componentObj = components.Find(p => p.Name == model);
                                if (componentObj == null)
                                {
                                    componentObj = new System.Dynamic.ExpandoObject();
                                    componentObj.Name = model;
                                    componentObj.Qty = 1;
                                    components.Add(componentObj);
                                }
                                else
                                {
                                    componentObj.Qty++;
                                }
                            });
                        }
                    }
                });
                //室内机&&附件

                thisProject.RoomIndoorList.ForEach((d) =>
                {
                    //Keeping Old Code Block chandresh
                    string sysName = "-", systemId = "-";
                    if (!string.IsNullOrEmpty(d.SystemID))
                    {
                        var sys = thisProject.SystemListNextGen.Find(p => ((JCHVRF.Model.SystemBase)p).Id == d.SystemID);

                        if (sys != null)
                        {

                            sysName = sys.Name;
                            systemId = ((JCHVRF.Model.SystemBase)sys).Id;

                        }
                    }
                    //End Keeping Old Code Block chandresh
                    bool isexportedreport = false;
                    if (!string.IsNullOrEmpty(d.SystemID))
                    {
                        var sys = thisProject.SystemListNextGen.Find(p => p.Id == d.SystemID && p.IsExportToReport == true);
                        if (sys != null)
                        {
                            isexportedreport = true;
                        }
                    }

                    //if (thisProject.SystemList.Find(p => p.Id == d.SystemID).IsExportToReport == true)
                    //if (isexportedreport == true)
                    //{
                    string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                    var ind = listIndoors.Find(m => m.Name == name && m.SystemId == d.SystemID);
                    //var ind = listIndoors.Find(m => m.Name == name);
                    if (ind == null)
                    {
                        ind = new System.Dynamic.ExpandoObject();
                        ind.Name = name;
                        //ind.System = sysName;
                        ind.SystemId = d.SystemID;
                        ind.Type = GetIndoorDisplayName(d);
                        ind.Qty = 1;
                        ind.Horsepower = d.IndoorItem.Horsepower;
                        listIndoors.Add(ind);
                    }
                    else
                    {
                        ind.Qty++;
                    }

                    //listIndoors for Bom 

                    var indbom = listIndoorsBom.Find(m => m.Name == name && m.SystemId == d.SystemID);
                    if (indbom == null)
                    {
                        indbom = new System.Dynamic.ExpandoObject();
                        indbom.Name = name;
                        //ind.System = sysName;
                        indbom.SystemId = d.SystemID;
                        indbom.Type = GetIndoorDisplayName(d);
                        indbom.Qty = 1;
                        indbom.Horsepower = d.IndoorItem.Horsepower;
                        listIndoorsBom.Add(indbom);
                    }
                    else
                    {
                        indbom.Qty++;
                    }
                    //-------------------

                    //Accessory
                    var accessoryList = d.ListAccessory;
                    if (accessoryList != null)
                    {
                        accessoryList.ForEach((a) =>
                        {
                            string accessoryName = (a.BrandCode == "Y" ? a.Model_York : a.Model_Hitachi);
                            //var accessory = listAccessories.Find(m => m.Name == accessoryName && m.SystemId == systemId);
                            var accessory = listAccessories.Find(m => m.Name == accessoryName);

                            if (accessory == null)
                            {
                                accessory = new System.Dynamic.ExpandoObject();
                                accessory.Name = accessoryName;
                                //accessory.System = sysName;
                                accessory.SystemId = d.SystemID;
                                accessory.Type = GetAccessoryDisplayName(a, d);
                                accessory.Qty = 1;
                                listAccessories.Add(accessory);
                            }
                            else
                            {
                                accessory.Qty++;
                            }

                            //listAccessoriesBom 
                            var accessorybom = listAccessoriesBom.Find(m => m.Name == accessoryName && m.SystemId == d.SystemID);

                            if (accessorybom == null)
                            {
                                accessorybom = new System.Dynamic.ExpandoObject();
                                accessorybom.Name = accessoryName;
                                //accessory.System = sysName;
                                accessorybom.SystemId = d.SystemID;
                                accessorybom.Type = GetAccessoryDisplayName(a, d);
                                accessorybom.Qty = 1;
                                listAccessoriesBom.Add(accessorybom);
                            }
                            else
                            {
                                accessorybom.Qty++;
                            }
                            //listAccessoriesBom

                        });
                    }
                    
                });

                //Heat Exchanger Units
                if (thisProject.ExchangerList != null && thisProject.ExchangerList.Count > 0)
                {
                    thisProject.ExchangerList.ForEach((d) =>
                    {
                        string sysName = "-", systemId = "-";
                        if (!string.IsNullOrEmpty(d.SystemID))
                        {
                            var sys = thisProject.HeatExchangerSystems.Find(p => p.Id == d.SystemID);
                            if (sys != null)
                            {
                                sysName = sys.Name;
                                systemId = sys.Id;
                            }
                        }
                        //Consider here only those Heat Exchangner systems which has IndoorItems
                        if (d.IndoorItem != null)
                        {
                            string name = (thisProject.BrandCode == "Y" ? d.IndoorItem.Model_York : d.IndoorItem.Model_Hitachi);
                            var ind = heatExchangerUnits.Find(m => m.Name == name);
                            if (ind == null)
                            {
                                ind = new System.Dynamic.ExpandoObject();
                                ind.Name = name;
                                ind.Type = GetIndoorDisplayName(d);
                                ind.Qty = 1;
                                heatExchangerUnits.Add(ind);
                            }
                            else
                            {
                                ind.Qty++;
                            }

                            //Accessories For HE
                            var accessoryList = d.ListAccessory;


                            if (accessoryList != null)
                            {
                                accessoryList.ForEach((a) =>
                                {
                                    string accessoryName = (a.BrandCode == "Y" ? a.Model_York : a.Model_Hitachi);

                                    var accessory = listAccessories.Find(m => m.Name == accessoryName && m.SystemId == systemId);

                                    int accCount = 0;
                                    if (accessory == null)
                                    {
                                        accessory = new System.Dynamic.ExpandoObject();
                                        accessory.Name = accessoryName;
                                        accessory.System = sysName;
                                        accessory.SystemId = systemId;
                                        accessory.Type = a.Type;//GetAccessoryDisplayName(a, d);
                                        accessory.Qty = 1;
                                        listAccessories.Add(accessory);
                                    }
                                    //else
                                    //{
                                    if (d.SystemID == systemId)
                                    {
                                        accCount = a.Count;
                                    }
                                    accessory.Qty = accCount;
                                    //}                                   
                                });
                            }
                        }
                        //-------------------

                    });
                }
                //控制器

                if (thisProject.ControlGroupList != null && thisProject.ControlGroupList.Count > 0)
                {
                    thisProject.ControlGroupList.ForEach((g) =>
                    {
                        if (thisProject.ControllerList != null && thisProject.ControllerList.Count > 0)
                        {
                            thisProject.ControllerList.ForEach((c) =>
                                {
                                    if (g.Id == c.ControlGroupID && g.IsValidGrp)
                                    {
                                        var crl = listControls.Find(m => m.Name == c.Name);
                                        if (crl == null)
                                        {
                                            crl = new System.Dynamic.ExpandoObject();
                                            crl.Name = c.Name;
                                            crl.Type = c.Description;
                                            crl.Qty = c.Quantity;
                                            listControls.Add(crl);
                                        }
                                        else
                                        {
                                            crl.Qty += c.Quantity;
                                        }
                                    }
                                });
                        }

                    });
                }

                DataTable T_PipingKitTable = pipBll.GetPipingKitTable();
                if (T_PipingKitTable != null)
                {
                    for (int i = 0; i < T_PipingKitTable.Rows.Count; i++)
                    {
                        DataRow dr = T_PipingKitTable.Rows[i];
                        string type = dr["Type"].ToString();
                        string model = dr["Model"].ToString();
                        string sys = dr["SystemId"].ToString();
                        if (type == "PipingConnectionKit")
                        {
                            //var item = listPipingConnectionKit.Find(m => m.Name == model && m.System == sys);
                            var item = listPipingConnectionKit.Find(m => m.Name == model);
                            if (item == null)
                            {
                                item = new System.Dynamic.ExpandoObject();
                                item.Name = model;
                                //item.System = sys;
                                item.Type = "Outdoor units piping connection kit";
                                item.Qty = 1;
                                listPipingConnectionKit.Add(item);
                            }
                            else
                            {
                                item.Qty++;
                            }

                            //listPipingConnectionKitBom 
                            var itembom = listPipingConnectionKitBom.Find(m => m.Name == model && m.System == sys);
                            if (itembom == null)
                            {
                                itembom = new System.Dynamic.ExpandoObject();
                                itembom.Name = model;
                                itembom.System = sys;
                                itembom.Type = "Outdoor units piping connection kit";
                                itembom.Qty = 1;
                                listPipingConnectionKitBom.Add(itembom);
                            }
                            else
                            {
                                itembom.Qty++;
                            }
                            //listPipingConnectionKitBom 
                        }
                        else if (type == "CHBox")
                        {
                            var item = listCHBox.Find(m => m.Name == model);
                            if (item == null)
                            {
                                item = new System.Dynamic.ExpandoObject();
                                item.Name = model;
                                //item.System = sys;
                                item.Type = "Cooling/Heating Changeover Box";
                                item.Qty = 1;
                                listCHBox.Add(item);
                            }
                            else
                            {
                                item.Qty++;
                            }

                            //for listCHBoxBom
                            var itembom = listCHBoxBom.Find(m => m.Name == model && m.System == sys);
                            if (itembom == null)
                            {
                                itembom = new System.Dynamic.ExpandoObject();
                                itembom.Name = model;
                                itembom.System = sys;
                                itembom.Type = "Cooling/Heating Changeover Box";
                                itembom.Qty = 1;
                                listCHBoxBom.Add(itembom);
                            }
                            else
                            {
                                itembom.Qty++;
                            }
                            // end listCHBoxBom 
                        }
                        else if (type == "BranchKit")
                        {
                            //var item = listBranchKit.Find(m => m.Name == model && m.System == sys);
                            var item = listBranchKit.Find(m => m.Name == model);
                            if (item == null)
                            {
                                item = new System.Dynamic.ExpandoObject();
                                item.Name = model;
                                //item.System = sys;
                                item.Type = "Line branch kit";
                                item.Qty = 1;
                                listBranchKit.Add(item);
                            }
                            else
                            {
                                item.Qty++;
                            }

                            //for listBranchKitBom
                            var itembom = listBranchKitBom.Find(m => m.Name == model && m.System == sys);
                            if (itembom == null)
                            {
                                itembom = new System.Dynamic.ExpandoObject();
                                itembom.Name = model;
                                itembom.System = sys;
                                itembom.Type = "Line branch kit";
                                itembom.Qty = 1;
                                listBranchKitBom.Add(itembom);
                            }
                            else
                            {
                                itembom.Qty++;
                            }
                            //listBranchKitBom
                        }
                    }
                }
                //Piping Materials
                if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
                {
                    thisProject.SystemListNextGen.ForEach((s) =>
                    {
                        if (s.IsExportToReport)
                        {
                            if (s.PipeActualLength == 0)
                            {
                                IsZeropipelength = true;
                            }
                            DataView dvL = null;
                            DataView dvG = GetData_LinkPipeSpecG(ref s, out dvL);
                            DataTable dtG = dvG.ToTable();
                            DataTable dtL = dvL.ToTable();
                            //// Piping Materials 计算加入ODU 的管径 on 20180601 by xyj
                            //GetData_LinkPipe(ref dtG, ref dtL, s, pipBll.GetPipingNodeOutElement(s, isHitachi));
                            for (int i = 0; i < dtL.Rows.Count; i++)
                            {
                                DataRow dr = dtL.Rows[i];
                                string spec = dr[0].ToString();
                                if (CommonBLL.IsDimension_inch())
                                    spec = pipBll.GetPipeSize_Inch(spec);
                                string len = (dr[4].ToString() == "-") ? "0" : Unit.ConvertToControl(Convert.ToDouble(dr[4].ToString()), UnitType.LENGTH_M, utLength).ToString("n1");
                                var item = listPipingLen.Find(m => m.Name == spec);
                                if (item == null)
                                {
                                    item = new System.Dynamic.ExpandoObject();
                                    item.Name = spec;
                                    item.Len = Convert.ToDouble(len);
                                    item.SystemId = s.Id;
                                    listPipingLen.Add(item);
                                }
                                else
                                {
                                    item.Len += Convert.ToDouble(len);
                                }

                                // for listPipingLenBom 
                                var itembom = listPipingLenBom.Find(m => m.Name == spec && m.SystemId == s.Id);
                                if (itembom == null)
                                {
                                    itembom = new System.Dynamic.ExpandoObject();
                                    itembom.Name = spec;
                                    itembom.Len = Convert.ToDouble(len);
                                    itembom.SystemId = s.Id;
                                    listPipingLenBom.Add(itembom);
                                }
                                else
                                {
                                    itembom.Len += Convert.ToDouble(len);
                                }
                                // end listPipingLenBom
                            }
                            for (int i = 0; i < dtG.Rows.Count; i++)
                            {
                                DataRow dr = dtG.Rows[i];
                                string spec = dr[0].ToString();
                                if (CommonBLL.IsDimension_inch())
                                    spec = pipBll.GetPipeSize_Inch(spec);
                                string len = (dr[4].ToString() == "-") ? "0" : Unit.ConvertToControl(Convert.ToDouble(dr[4].ToString()), UnitType.LENGTH_M, utLength).ToString("n1");
                                var item = listPipingLen.Find(m => m.Name == spec);
                                if (item == null)
                                {
                                    item = new System.Dynamic.ExpandoObject();
                                    item.Name = spec;
                                    item.Len = Convert.ToDouble(len);
                                    item.SystemId = s.Id;
                                    listPipingLen.Add(item);
                                }
                                else
                                {
                                    item.Len += Convert.ToDouble(len);
                                }

                                // for listPipingLenBom 
                                var itembom = listPipingLenBom.Find(m => m.Name == spec && m.SystemId == s.Id);
                                if (itembom == null)
                                {
                                    itembom = new System.Dynamic.ExpandoObject();
                                    itembom.Name = spec;
                                    itembom.Len = Convert.ToDouble(len);
                                    itembom.SystemId = s.Id;
                                    listPipingLenBom.Add(itembom);
                                }
                                else
                                {
                                    itembom.Len += Convert.ToDouble(len);
                                }
                                // end listPipingLenBom

                            }
                        }
                    });
                    totalAddRefrigeration = Unit.ConvertToControl(thisProject.SystemListNextGen.Sum(s => s.IsExportToReport == true ? s.AddRefrigeration : 0), UnitType.WEIGHT, utWeight).ToString("n1");
                }
            }
        }
        /// <summary>
        /// 生成产品列表汇总数据对象
        /// </summary>

        ///// <summary>
        ///// Piping Materials 计算加入ODU 的管径 on 20180601 by xyj
        ///// </summary>
        ///// <param name="dtG">气管</param>
        ///// <param name="dtL">液管</param>
        ///// <param name="system">当前系统</param>
        ///// <param name="outNodeItem">室外机组合类</param>
        //private void GetData_LinkPipe(ref DataTable dtG, ref DataTable dtL, SystemVRF system, NodeElement_Piping outNodeItem)
        //{
        //    //如果当前系统 未设置管长 直接返回
        //    if (system.MyPipingNodeOut.PipeLengthes == null)
        //        return;

        //    PointF ptText = new PointF();
        //    for (int i = 0; i < outNodeItem.PtPipeDiameter.Count; ++i)
        //    {
        //        ptText = UtilEMF.OffsetLocation(outNodeItem.PtPipeDiameter[i], system.MyPipingNodeOut.Location);
        //        string s = outNodeItem.PipeSize[i];
        //        string[] aa = s.Split('x');
        //        double upper = 0;       //高压气管
        //        double lower = 0;       //低压气管
        //        double liqude = 0;      //液管
        //        if (aa.Length == 2)
        //        {
        //            upper = Convert.ToDouble(aa[0]);
        //            liqude = Convert.ToDouble(aa[1]);
        //        }
        //        else if (aa.Length == 3)
        //        {
        //            lower = Convert.ToDouble(aa[0]);
        //            upper = Convert.ToDouble(aa[1]);
        //            liqude = Convert.ToDouble(aa[2]);
        //        }
        //        double val = Convert.ToDouble(system.MyPipingNodeOut.PipeLengthes[i].ToString() == "" ? 0 : system.MyPipingNodeOut.PipeLengthes[i]);
        //        if (upper > 0)
        //        {
        //            //高压管径
        //            dtG.Rows.Add(upper, PipeType.Gas, 1, val, val, system.Name);
        //        }
        //        if (lower > 0)
        //        {
        //            //低压管径
        //            dtG.Rows.Add(lower, PipeType.Gas, 1, val, val, system.Name);
        //        }
        //        if (liqude > 0)
        //        {
        //            //液管
        //            dtL.Rows.Add(liqude, PipeType.Liquid, 1, val, val, system.Name);
        //        }
        //    }

        //}
        #endregion

        #region 输出报告
        public bool ExportReportPDF(string templateNamePath, string saveFilePath)
        {
            return ExportReport(templateNamePath, saveFilePath, SaveFormat.Pdf);
        }
        public bool ExportReportWiringJPEG(string saveFilePath)
        {
            return ExportReportWiring(saveFilePath, SaveFormat.Jpeg);
        }
        public bool ExportReportPipingJPEG(string saveFilePath)
        {
            return ExportReportPiping(saveFilePath, SaveFormat.Jpeg);
        }
        private bool ExportReportWiring(string saveFilePath, SaveFormat saveFormat)
        {
            if (!FileInUse(saveFilePath))
            {
                MessageBox.Show("File Already Open Close It. ");
                return false;
            }

            // 询问是否需要打开报告文件
            if (JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_REPORT_OPEN) == DialogResult.Yes)
            {

                string errMsg = "";
                Bitmap bmp = new Bitmap(saveFilePath);
                Image img = PictureProcess(bmp, Convert.ToInt32(bmp.Width * 1.25d), Convert.ToInt32(bmp.Height * 1.25));
                string image = Convert.ToString(img);
                //Util.Process_Start(image, out errMsg);
                img.Save(saveFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                string fileName = saveFilePath;//
                Util.Process_Start(fileName, out errMsg);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    JCMsg.ShowErrorOK(errMsg);
                }
            }
            return true;
        }
        public Image PictureProcess(Image sourceImage, int targetWidth, int targetHeight)
        {
            int width;
            int height;
            try
            {
                System.Drawing.Imaging.ImageFormat format = sourceImage.RawFormat;
                Bitmap targetPicture = new Bitmap(targetWidth, targetHeight);
                Graphics g = Graphics.FromImage(targetPicture);
                g.Clear(Color.White);
                if (sourceImage.Width > targetWidth && sourceImage.Height <= targetHeight)
                {
                    width = targetWidth;
                    height = (width * sourceImage.Height) / sourceImage.Width;
                }
                else if (sourceImage.Width <= targetWidth && sourceImage.Height > targetHeight)
                {
                    height = targetHeight;
                    width = (height * sourceImage.Width) / sourceImage.Height;
                }
                else if (sourceImage.Width <= targetWidth && sourceImage.Height <= targetHeight)
                {
                    width = sourceImage.Width;
                    height = sourceImage.Height;
                }
                else
                {
                    width = targetWidth;
                    height = (width * sourceImage.Height) / sourceImage.Width;
                    if (height > targetHeight)
                    {
                        height = targetHeight;
                        width = (height * sourceImage.Width) / sourceImage.Height;
                    }
                }
                g.DrawImage(sourceImage, (targetWidth - width) / 2, (targetHeight - height) / 2, width, height);
                sourceImage.Dispose();

                return targetPicture;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private bool ExportReportPiping(string saveFilePath, SaveFormat saveFormat)
        {
            if (!FileInUse(saveFilePath))
            {
                MessageBox.Show("File Already Open Close It. ");
                return false;
            }

            // 询问是否需要打开报告文件
            if (JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_REPORT_OPEN) == DialogResult.Yes)
            {
                string errMsg = "";
                Bitmap bmp = new Bitmap(saveFilePath);
                Image img = PictureProcess(bmp, Convert.ToInt32(bmp.Width * 1.25d), Convert.ToInt32(bmp.Height * 1.25));
                string image = Convert.ToString(img);
                img.Save(saveFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                string fileName = saveFilePath;//
                Util.Process_Start(fileName, out errMsg);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    JCMsg.ShowErrorOK(errMsg);
                }
            }
            return true;
        }

        public bool ExportReportWord(string templateNamePath, string saveFilePath)
        {
            return ExportReport(templateNamePath, saveFilePath, SaveFormat.Doc);
        }

        /// <summary>
        /// 输出 Word 报告
        /// </summary>
        /// <param name="templateNamePath"></param>
        private bool ExportReport(string templateNamePath, string saveFilePath, SaveFormat saveFormat)
        {
            try
            {
                string selMode = "";
                templateNamePath = GetDocPath(templateNamePath);
                bool cooling = thisProject.IsCoolingModeEffective;
                bool heating = thisProject.IsHeatingModeEffective;
                if (cooling || heating)
                {
                    if (cooling && heating)
                        selMode = Msg.GetResourceString("REPORT2_Cooling") + "+" + Msg.GetResourceString("REPORT2_Heating");
                    else if (cooling)
                        selMode = Msg.GetResourceString("REPORT2_Cooling");
                    else
                        selMode = Msg.GetResourceString("REPORT2_Heating");
                }

                //打开模版
                Doc = new Document(templateNamePath);//主模版
                Builder = new DocumentBuilder(Doc);
                var tbs = Doc.GetChildNodes(Aspose.Words.NodeType.Table, true);
                tbBasic = (Table)tbs[1];
                //Builder.MoveToBookmark("ProjectNameTitle");
                //Builder.Write(thisProject.Name);
                Builder.MoveToBookmark("UpdateDate");
                Builder.Write(string.IsNullOrEmpty(thisProject.ProjectUpdateDate) ? DateTime.Now.ToString("dd/MM/yyyy") : Convert.ToDateTime(thisProject.ProjectUpdateDate).ToString("dd/MM/yyyy"));
                Builder.MoveToBookmark("Version");
                Builder.Write(MyConfig.Version);
                Builder.MoveToBookmark("Version2");
                Builder.Write(MyConfig.Version);
                Builder.MoveToBookmark("Version3");
                Builder.Write(MyConfig.Version);
                Builder.MoveToBookmark("FirstPageProjectName");
                Builder.Write(thisProject.Name);
                Builder.MoveToBookmark("FooterProjectTitle");
                Builder.Write(thisProject.Name);
                Builder.MoveToBookmark("FooterProjectTitle2");
                Builder.Write(thisProject.Name);
                Builder.MoveToBookmark("FirstPageRegion");
                Builder.Write(thisProject.SubRegionCode);
                Builder.MoveToBookmark("FirstPageSelectionMode");
                Builder.Write(selMode);
                if (!ProjectBLL.IsUsingEuropeProjectInfo(thisProject.RegionCode)) //是否使用EU的project info
                {
                    //不使用EU的project info
                    Builder.MoveToBookmark("SoldTo");
                    Builder.Write(thisProject.SoldTo == null ? "" : thisProject.SoldTo);
                    Builder.MoveToBookmark("ShipTo");
                    Builder.Write(thisProject.ShipTo == null ? "" : thisProject.ShipTo);
                    Builder.MoveToBookmark("ContractNo");
                    Builder.Write(thisProject.ContractNO == null ? "" : thisProject.ContractNO);
                    Builder.MoveToBookmark("SalesOffice");
                    Builder.Write(thisProject.SalesOffice == null ? "" : thisProject.SalesOffice);
                }
                else
                {
                    Builder.MoveToBookmark("ClientName");
                    Builder.Write(thisProject.clientName == null ? "" : thisProject.clientName);
                    Builder.MoveToBookmark("PostCode");
                    Builder.Write(thisProject.postCode == null ? "" : thisProject.postCode);
                    Builder.MoveToBookmark("Tel");
                    Builder.Write(thisProject.clientTel == null ? "" : thisProject.clientTel);
                    Builder.MoveToBookmark("Mail");
                    Builder.Write(thisProject.clientMail == null ? "" : thisProject.clientMail);
                    Builder.MoveToBookmark("Company");
                    Builder.Write(thisProject.salesCompany == null ? "" : thisProject.salesCompany);
                    Builder.MoveToBookmark("Address");
                    Builder.Write(thisProject.salesAddress == null ? "" : thisProject.salesAddress);
                    Builder.MoveToBookmark("PhoneNo");
                    Builder.Write(thisProject.salesPhoneNo == null ? "" : thisProject.salesPhoneNo);
                }
                Builder.MoveToBookmark("SalesEngineer");
                Builder.Write(thisProject.SalesEngineer == null ? "" : thisProject.SalesEngineer);
                Builder.MoveToBookmark("OrderDate");
                Builder.Write(thisProject.OrderDate.ToString("dd/MM/yyyy"));
                Builder.MoveToBookmark("DeliveryDate");
                Builder.Write(thisProject.DeliveryRequiredDate.ToString("dd/MM/yyyy"));
                Builder.MoveToBookmark("ProjectRevision");
                Builder.Write(thisProject.ProjectRevision == null ? "" : thisProject.ProjectRevision);
                Builder.MoveToBookmark("CentralControlNum");
                Builder.Write(centralControllerNum.ToString());

                Builder.MoveToBookmark("CentralControllerImg");
                if (thisProject.ControlGroupList != null)
                {
                    if (thisProject.SubRegionCode == "TW")
                    {
                        if (thisProject.ControlGroupList.Count > 1)
                        {
                            //台湾导出controller屏幕截图 add by Shen Junjie on 2018/8/15
                            string fullPath = FileLocal.GetNamePathControllerPicture("1");
                            InsertImage(fullPath, Builder);
                        }
                    }
                    else
                    {
                        for (int i = thisProject.ControlGroupList.Count - 1; i >= 0; i--)
                        {
                            var group = thisProject.ControlGroupList[i];
                            if (!group.IsValidGrp) continue; //防止导出空白的Group  add by Shen Junjie on 2018/8/15

                            string fullPath = FileLocal.GetNamePathControllerWiringPicture(group.Id);
                            InsertImage(fullPath, Builder);
                        }
                    }
                }
                if (thisProject.RegionCode != "EU_W" || thisProject.RegionCode != "EU_E" || thisProject.RegionCode != "EU_S")
                {
                    string title = "JOHNSON CONTROLS-HITACHI AIR CONDITIONING";
                    Builder.MoveToBookmark("FooterTitle1");
                    if (Builder.CurrentNode is Run)
                    {
                        (Builder.CurrentNode as Run).Text = title;
                    }
                    Builder.MoveToBookmark("FooterTitle2");
                    if (Builder.CurrentNode is Run)
                    {
                        (Builder.CurrentNode as Run).Text = title;
                    }
                }

                    fillOutdoorUnits();

                    fillIndoorUnits();

                    if (!thisProject.IsIndoorListChecked && !thisProject.IsOutdoorListChecked)
                    {
                        Builder.MoveToBookmark("SystemSelectionTitle");
                        Builder.CurrentParagraph.Remove();
                    }
                    if (!thisProject.IsIndoorListChecked && !thisProject.IsOutdoorListChecked && !thisProject.IsRoomInfoChecked)
                    {
                        Builder.MoveToBookmark("SystemDesignTitle");
                        Builder.CurrentParagraph.Remove();
                        //Builder.MoveToBookmark("ProjectConditionsTitle");
                        //Builder.CurrentParagraph.Remove();
                        //tbBasic.Remove();
                        Builder.MoveToBookmark("SystemDesign");
                        Builder.CurrentParagraph.Remove();
                    }
                    else
                    {
                        //fillBasicData();

                        fillSystemDesign();
                    }

                    fillPipingDesign();

                    fillWiringDesign();

                    fillProductList();

                    fillBomBySystem();//  New Code After Integrate 4.7
               
                //更新目录
                Doc.UpdateFields();

                #region 保存到指定路径 & 询问是否需要打开报告文件

                //if (Util.IsFileInUse(saveFilePath))
                //{
                //    return false;
                //}

                if (!FileInUse(saveFilePath))
                {
                    MessageBox.Show("File Already Open Close It. ");
                    return false;
                }
                object filename = (object)saveFilePath;
                Doc.Save(filename.ToString(), saveFormat);

                // 询问是否需要打开报告文件
                if (JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_REPORT_OPEN) == DialogResult.Yes)
                {
                    Builder.MoveToBookmark(DocBookMark.ProjectName);
                    string errMsg = "";
                    Util.Process_Start(saveFilePath, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        JCMsg.ShowErrorOK(errMsg);
                    }
                }
                #endregion

            }
            catch (System.Exception ex)
            {
                #region 错误提示
                object miss = System.Reflection.Missing.Value;
                //As Per Bug 4711
                MessageBox.Show("Something went wrong, please contact admin!");
                //JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                //As Per Bug 4711
                AddToLog(ex.Message);
                return false;
                #endregion
            }
            finally
            {
                #region 写入日志文件
                //WriteToLogFile(ReportLogList, MyConfig.SystemLogNamePath);
                #endregion
            }
            return true;
        }
        //  New Code After Integrate 4.7
        private void fillBomBySystem()
        {
            //List<dynamic> SysOutdoors = new List<dynamic>();
            if (thisProject.SystemListNextGen != null && thisProject.SystemListNextGen.Count > 0)
            {
                try
                {
                    List<NextGenModel.SystemVRF> lstBsm = new List<NextGenModel.SystemVRF>();
                    lstBsm = thisProject.SystemListNextGen;
                    lstBsm.Reverse();
                    lstBsm.ForEach((p) =>
                    {
                        if (p.IsExportToReport == true)
                        {

                            //Document docProduct;
                            //docProduct = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\AppendixBOMbySystem.doc"));
                            ////string docProductnew = "..\\..\\Report\\Template\\NewReport\\AppendixBOMbySystem.doc";
                            //string sourceDir = System.IO.Path.Combine(defaultFolder, docProductnew);
                            //Document docOutdoor = new Document(sourceDir);
                            //DocumentBuilder buildProduct = new DocumentBuilder(docOutdoor);
                            //Document docProduct;
                            //docProduct = new Document(GetDocPath("..\\..\\Report\\Template\\NewReport\\AppendixBOMbySystem.doc"));
                            //DocumentBuilder buildProduct = new DocumentBuilder(docProduct);

                            // string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;

                            Document docProduct;
                            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                            string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\AppendixBOMbySystem.doc";
                            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                            docProduct = new Document(GetDocPath(sourceDir));
                            //docProduct = new Document(sourceDir);
                            //docProduct = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\EquipmentListAndInformation.doc"));
                            DocumentBuilder buildProduct = new DocumentBuilder(docProduct);
                            var tbs = docProduct.GetChildNodes(Aspose.Words.NodeType.Table, true);

                            SetBookMarkText(buildProduct, "systemname", p.Name);
                            SetBookMarkText(buildProduct, "WeightUnit", utWeight);
                            //SetBookMarkText(buildProduct, "LengthUnit", utLength);
                            SetBookMarkText(buildProduct, "PipeSizeUnit", utDimension);

                            Table tbEquipment = (Table)tbs[0];
                            Table tbPipingMaterials = (Table)tbs[1];
                            Table tbRefrigerant = (Table)tbs[2];
                            int index = 3;
                            int check = 0;

                            if (listOutdoors.Count > 0)
                            {
                                var rowHeader = tbEquipment.Rows[1];
                                var srowCY = tbEquipment.Rows[2];
                                tbEquipment.Rows[2].Remove();
                                for (int j = listOutdoors.Count - 1; j >= 0; j--)
                                {
                                    if (listOutdoors[j].SystemId == p.Id)
                                    {
                                        check++;
                                        var outdoor = listOutdoors[j];
                                        var rowCopy = rowHeader.Clone(true);
                                        tbEquipment.InsertBefore(rowCopy, tbEquipment.Rows[2]);
                                        List<dynamic> components = outdoor.Components;
                                        string outdoortran = Msg.GetResourceString("REPORTBom_outdoor");

                                        if (check == 1)
                                        {
                                            SetCellText(buildProduct, 0, 2, 0, outdoortran);
                                        }

                                        SetCellText(buildProduct, 0, 2, 1, outdoor.Name);
                                        SetCellText(buildProduct, 0, 2, 2, trans.getTypeTransStr(TransType.Series.ToString(), outdoor.Series));
                                        SetCellText(buildProduct, 0, 2, 3, outdoor.Qty);

                                        components.ForEach((c) =>
                                        {
                                            var rowCY = srowCY.Clone(true);
                                            tbEquipment.InsertAfter(rowCY, tbEquipment.Rows[2]);
                                        });

                                        components.ForEach((c) =>
                                        {
                                            //SetCellText(buildProduct, 0, index, 0, c.Name.Trim());
                                            SetCellText(buildProduct, 0, index, 1, c.Name.Trim());
                                            SetCellText(buildProduct, 0, index, 2, "component");
                                            SetCellText(buildProduct, 0, index, 3, c.Qty);
                                            index++;
                                        });
                                    }
                                }
                            }

                            //tbEquipment.Rows[2].Remove();

                            if (listIndoorsBom.Count > 0)
                            {

                                var list = listIndoorsBom.OrderBy(l => l.Type).ThenBy(l => l.Horsepower).ToList();
                                index++;
                                var rowCopy = tbEquipment.Rows[1].Clone(true);
                                tbEquipment.InsertAfter(rowCopy, tbEquipment.Rows[index - 1]);
                                index++;
                                check = 0;

                                list.ForEach((d) =>
                                {
                                    if (p.Id == d.SystemId)
                                    {
                                        check++;
                                        string indoortran = Msg.GetResourceString("REPORTBom_Indoor");
                                        var rowCopys = tbEquipment.Rows[1].Clone(true);
                                        tbEquipment.InsertAfter(rowCopys, tbEquipment.Rows[index - 1]);
                                        if (check == 1)
                                        {
                                            SetCellText(buildProduct, 0, index - 1, 0, indoortran);
                                        }
                                        SetCellText(buildProduct, 0, index - 1, 1, d.Name);
                                        SetCellText(buildProduct, 0, index - 1, 2, d.Type);
                                        SetCellText(buildProduct, 0, index - 1, 3, d.Qty);
                                        index++;
                                    }
                                });
                            }

                            if (listAccessoriesBom.Count > 0)
                            {
                                var rc = tbEquipment.Rows[1].Clone(true);
                                tbEquipment.InsertAfter(rc, tbEquipment.Rows[index - 1]);
                                index++;
                                check = 0;

                                var list = listAccessoriesBom.OrderBy(a => a.Type).ToList();
                                list.ForEach((d) =>
                                {
                                    if (d.SystemId == p.Id)
                                    {
                                        check++;
                                        string Accessorytran = Msg.GetResourceString("REPORTBom_Accessory");
                                        var rowCopy = tbEquipment.Rows[1].Clone(true);
                                        tbEquipment.InsertAfter(rowCopy, tbEquipment.Rows[index - 1]);
                                        if (check == 1)
                                        {
                                            SetCellText(buildProduct, 0, index - 1, 0, Accessorytran);
                                        }

                                        SetCellText(buildProduct, 0, index - 1, 1, d.Name);
                                        SetCellText(buildProduct, 0, index - 1, 2, d.Type);
                                        SetCellText(buildProduct, 0, index - 1, 3, d.Qty);
                                        index++;
                                    }
                                });

                            }

                            if (listPipingConnectionKitBom.Count > 0)
                            {
                                var rc = tbEquipment.Rows[1].Clone(true);
                                tbEquipment.InsertAfter(rc, tbEquipment.Rows[index - 1]);
                                index++;
                                check = 0;

                                listPipingConnectionKitBom.ForEach((d) =>
                                {
                                    if (d.System == p.Id)
                                    {
                                        check++;
                                        string PipingConnKitTran = Msg.GetResourceString("REPORTBom_PipingConKit");
                                        var rowCopy = tbEquipment.Rows[1].Clone(true);
                                        tbEquipment.InsertAfter(rowCopy, tbEquipment.Rows[index - 1]);
                                        if (check == 1)
                                        {
                                            SetCellText(buildProduct, 0, index, 0, PipingConnKitTran);
                                        }
                                        SetCellText(buildProduct, 0, index, 1, d.Name);
                                        SetCellText(buildProduct, 0, index, 2, d.Type);
                                        SetCellText(buildProduct, 0, index, 3, d.Qty);
                                        index++;
                                    }
                                });

                            }
                            if (listBranchKitBom.Count > 0)
                            {
                                var rc = tbEquipment.Rows[1].Clone(true);
                                tbEquipment.InsertAfter(rc, tbEquipment.Rows[index - 1]);
                                index++;
                                check = 0;

                                listBranchKitBom.ForEach((d) =>
                                {
                                    if (d.System == p.Id)
                                    {
                                        check++;
                                        string MultikitTran = Msg.GetResourceString("REPORTBom_MultiKit");
                                        var rowCopy = tbEquipment.Rows[1].Clone(true);
                                        tbEquipment.InsertAfter(rowCopy, tbEquipment.Rows[index - 1]);
                                        if (check == 1)
                                        {
                                            SetCellText(buildProduct, 0, index, 0, MultikitTran);
                                        }
                                        SetCellText(buildProduct, 0, index, 1, d.Name);
                                        SetCellText(buildProduct, 0, index, 2, d.Type);
                                        SetCellText(buildProduct, 0, index, 3, d.Qty);
                                        index++;
                                    }
                                });

                            }
                            if (listCHBoxBom.Count > 0)
                            {
                                var rc = tbEquipment.Rows[1].Clone(true);
                                tbEquipment.InsertAfter(rc, tbEquipment.Rows[index - 1]);
                                index++;
                                check = 0;

                                listCHBoxBom.ForEach((d) =>
                                {
                                    if (d.System == p.Id)
                                    {
                                        check++;
                                        string CHBoxTran = Msg.GetResourceString("REPORTBom_CHBox");
                                        var rowCopy = tbEquipment.Rows[1].Clone(true);
                                        tbEquipment.InsertAfter(rowCopy, tbEquipment.Rows[index - 1]);
                                        if (check == 1)
                                        {
                                            SetCellText(buildProduct, 0, index, 0, CHBoxTran);
                                        }
                                        SetCellText(buildProduct, 0, index, 1, d.Name);
                                        SetCellText(buildProduct, 0, index, 2, d.Type);
                                        SetCellText(buildProduct, 0, index, 3, d.Qty);
                                        index++;
                                    }
                                });

                            }

                            if (listPipingLenBom.Count > 0)
                            {
                                index = 2;

                                listPipingLenBom.ForEach((d) =>
                                {
                                    if (d.SystemId == p.Id)
                                    {
                                        var rowCopy = tbPipingMaterials.Rows[2].Clone(true);
                                        tbPipingMaterials.InsertAfter(rowCopy, tbPipingMaterials.Rows[index]);
                                        SetCellText(buildProduct, 1, index + 1, 0, d.Name);
                                        SetCellText(buildProduct, 1, index + 1, 1, d.Len);
                                        index++;
                                    }
                                });

                                tbPipingMaterials.Rows[2].Remove();

                                string totalAddRefrig = Unit.ConvertToControl(p.AddRefrigeration, UnitType.WEIGHT, utWeight).ToString("n1");

                                //totalAddRefrigeration = Unit.ConvertToControl(thisProject.SystemList.Sum(s => s.IsExportToReport == true ? s.AddRefrigeration : 0), UnitType.WEIGHT, utWeight).ToString("n1");
                                if (!string.IsNullOrEmpty(totalAddRefrig))
                                {
                                    buildProduct.MoveToBookmark("RefrigerantWeight");
                                    buildProduct.Write(totalAddRefrig);
                                }

                            }

                            Bookmark Tmark = Doc.Range.Bookmarks["AppedixBomList"];
                            InsertDocument(Tmark.BookmarkStart.ParentNode, docProduct);
                        }
                    });
                }
                catch (Exception ex)
                { }
            }
        }

        //  End Code After Integrate 4.7
        /// <summary>
        /// 填充室外机数据
        /// </summary>
        private void fillOutdoorUnits()
        {
            if (thisProject.IsOutdoorListChecked)
            {
                for (int i = outdoorUnits.Count - 1; i >= 0; i--)
                {
                    var outdoor = outdoorUnits[i];
                    Document docOutdoor;
                    //docOutdoor = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\OutdoorUnits.doc"));
                    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;

                    if (thisProject.SubRegionCode == "TW")
                    {
                        string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\OutdoorUnitsTW.doc";
                        string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                        docOutdoor = new Document(sourceDir);
                    }
                    else
                    {
                        string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\OutdoorUnits.doc";
                        string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                        //docOutdoor = new Document(sourceDir);
                        docOutdoor = new Document(GetDocPath(sourceDir));
                    }



                    DocumentBuilder buildOurdoor = new DocumentBuilder(docOutdoor);
                    var tbs = docOutdoor.GetChildNodes(Aspose.Words.NodeType.Table, true);
                    //添加室外机图片
                    // string fullPath = MyConfig.TypeImageDirectory + outdoor.Pictures;
                    //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                    string navigateToFolderImages = "..\\..\\Image\\TypeImages\\";
                    string sourceDirImages = System.IO.Path.Combine(defaultFolder, navigateToFolderImages);
                    string fullPath = sourceDirImages + outdoor.Pictures;

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.Drawing.Image img = new System.Drawing.Bitmap(fullPath);
                        buildOurdoor.MoveToBookmark("Pictures");
                        buildOurdoor.InsertImage(img, 50, 50);
                    }
                    else
                    {
                        fullPath = sourceDirImages + "HNBQ 36-40, HNCQ 44-48.png";
                        System.Drawing.Image img = new System.Drawing.Bitmap(fullPath);
                        buildOurdoor.MoveToBookmark("Pictures");
                        buildOurdoor.InsertImage(img, 50, 30);
                    }
                    SetBookMarkText(buildOurdoor, "CoolingUnit", utPower);
                    SetBookMarkText(buildOurdoor, "HeatingUnit", utPower);
                    SetBookMarkText(buildOurdoor, "DepthUnit", utDimension);
                    SetBookMarkText(buildOurdoor, "HeightUnit", utDimension);
                    SetBookMarkText(buildOurdoor, "WidthUnit", utDimension);
                    SetBookMarkText(buildOurdoor, "WeightUnit", utWeight);

                    var mark = docOutdoor.Range.Bookmarks["Model"];
                    mark.Text = outdoor.Model == null ? "" : outdoor.Model;
                    mark = docOutdoor.Range.Bookmarks["Description"];
                    mark.Text = outdoor.Description == null ? "" : outdoor.Description;
                    mark = docOutdoor.Range.Bookmarks["Quantity"];
                    mark.Text = outdoor.Quantity == null ? "" : outdoor.Quantity.ToString();
                    List<string> components = outdoor.Components;
                    for (var m = 0; m < components.Count; m++)
                    {
                        buildOurdoor.MoveToCell(0, m + 2, 4, 0);
                        foreach (Run run in buildOurdoor.CurrentParagraph.Runs)
                        {
                            run.Text = "";
                        }
                        buildOurdoor.Write(components[m].Trim());
                    }
                    buildOurdoor.MoveToBookmark("Model2");
                    buildOurdoor.Write(outdoor.Model == null ? "" : outdoor.Model);
                    buildOurdoor.MoveToBookmark("Power");
                    buildOurdoor.Write(outdoor.Information.PowerSupply == null ? "" : outdoor.Information.PowerSupply);
                    buildOurdoor.MoveToBookmark("Cooling");
                    buildOurdoor.Write(outdoor.Information.Cooling == null ? "" : outdoor.Information.Cooling.ToString());
                    buildOurdoor.MoveToBookmark("Heating");
                    buildOurdoor.Write(outdoor.Information.Heating == null ? "" : outdoor.Information.Heating.ToString());
                    if (thisProject.SubRegionCode == "TW")
                    {
                        buildOurdoor.MoveToBookmark("CSPF"); // used for CSPF
                        buildOurdoor.Write(outdoor.Information.CSPF.ToString());
                    }
                    else
                    {
                        buildOurdoor.MoveToBookmark("EER");
                        buildOurdoor.Write(outdoor.Information.EER.ToString());
                        buildOurdoor.MoveToBookmark("COP");
                        buildOurdoor.Write(outdoor.Information.COP.ToString());
                        buildOurdoor.MoveToBookmark("SEER");
                        buildOurdoor.Write(outdoor.Information.SEER.ToString());
                        buildOurdoor.MoveToBookmark("SCOP");
                        buildOurdoor.Write(outdoor.Information.SCOP.ToString());
                    }
                    buildOurdoor.MoveToBookmark("Height");
                    buildOurdoor.Write(outdoor.Information.Height == null ? "" : outdoor.Information.Height.ToString());
                    buildOurdoor.MoveToBookmark("Width");
                    buildOurdoor.Write(outdoor.Information.Width == null ? "" : outdoor.Information.Width.ToString());
                    buildOurdoor.MoveToBookmark("Depth");
                    buildOurdoor.Write(outdoor.Information.Depth == null ? "" : outdoor.Information.Depth.ToString());
                    buildOurdoor.MoveToBookmark("Sound");
                    buildOurdoor.Write(outdoor.Information.SoundPower == null ? "" : outdoor.Information.SoundPower.ToString());
                    buildOurdoor.MoveToBookmark("Weight");
                    buildOurdoor.Write(outdoor.Information.Weight == null ? "" : outdoor.Information.Weight.ToString());
                    Bookmark Tmark = Doc.Range.Bookmarks["OutdoorUnits"];//主文档书签
                    InsertDocument(Tmark.BookmarkStart.ParentNode, docOutdoor);//表格复制
                }
            }
            else
            {
                Builder.MoveToBookmark("OutdoorUnitsTitle");
                Builder.CurrentParagraph.Remove();
            }
            Builder.MoveToBookmark("OutdoorUnits");
            Builder.CurrentParagraph.Remove();
        }

        /// <summary>
        /// 填充室内机数据
        /// </summary>
        /// 

        private void fillIndoorUnits()
        {
            if (thisProject.IsIndoorListChecked)
            {
                Boolean indoorsystoprint;
                Boolean indoorsysitemtoprint;
                for (int i = indoorUnits.Count - 1; i >= 0; i--)
                {
                    var room = indoorUnits[i];
                    List<dynamic> indoors = room.indoors;
                    int index = 3;
                    if (indoors.Count > 0)
                    {

                        indoorsystoprint = false;
                        for (int k = 0; k < thisProject.SystemListNextGen.Count; k++)
                        {
                            for (int l = 0; l < indoors.Count; l++)
                            {
                                if (thisProject.SystemListNextGen[k].Id == indoors[l].sysID && thisProject.SystemListNextGen[k].IsExportToReport == true)
                                {
                                    indoorsystoprint = true;
                                    break;
                                }
                            }
                        };

                        if (indoorsystoprint == true)
                        {
                            Document docIndoor;
                            //docIndoor = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\IndoorUnits.doc"));
                            //DocumentBuilder buildIndoor = new DocumentBuilder(docIndoor);
                            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                            string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\IndoorUnits.doc";
                            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                            //docIndoor = new Document(sourceDir);
                            docIndoor = new Document(GetDocPath(sourceDir));
                            DocumentBuilder buildIndoor = new DocumentBuilder(docIndoor);
                            var tbs = docIndoor.GetChildNodes(Aspose.Words.NodeType.Table, true);
                            Table tb = (Table)tbs[0];
                            buildIndoor.MoveToBookmark("FloorRoom");
                            buildIndoor.Write(room.roomName);
                            buildIndoor.MoveToBookmark("CapUnit");
                            if (utPower == "kW" && isIduCapacityW == true)
                            {
                                buildIndoor.Write("W");
                            }
                            else
                            {
                                buildIndoor.Write(utPower);
                            }

                            for (int j = indoors.Count - 1; j >= 0; j--)
                            {
                                indoorsysitemtoprint = false;
                                for (int n = 0; n < thisProject.SystemListNextGen.Count; n++)
                                {
                                    if (thisProject.SystemListNextGen[n].Id == indoors[j].sysID && thisProject.SystemListNextGen[n].IsExportToReport == true)
                                    {
                                        indoorsysitemtoprint = true;
                                        break;
                                    }
                                };

                                if (indoorsysitemtoprint == true)
                                {
                                    var ind = indoors[j];
                                    List<dynamic> control = ind.Control;
                                    int addRows = 1;
                                    if (control.Count > 0)
                                    {
                                        addRows = control.Count;
                                    }
                                    if (j == indoors.Count - 1)
                                    {
                                        addRows--;
                                    }
                                    for (int m = 0; m < addRows; m++)
                                    {
                                        var row = tb.Rows[index - 1].Clone(true);
                                        tb.InsertAfter(row, tb.Rows[index - 1]);
                                    }
                                    buildIndoor.MoveToCell(0, index, 0, 0);
                                    //string fullPath = MyConfig.TypeImageDirectory + ind.Picture;
                                    string navigateToFolderImages = "";
                                    string ControllerSwitchImage = string.Empty;
                                    navigateToFolderImages = "..\\..\\Image\\TypeImages\\";
                                    ControllerSwitchImage = "..\\..\\Image\\ControllerSwitchImage\\";
                                    string sourceDirImages = System.IO.Path.Combine(defaultFolder, navigateToFolderImages);
                                    string sourceDirControllerSwitchImage = System.IO.Path.Combine(defaultFolder, ControllerSwitchImage);
                                    string fullPath = sourceDirImages + room.indoors[j].Picture;
                                    if (System.IO.File.Exists(fullPath))
                                    {
                                        using (System.Drawing.Image img = new System.Drawing.Bitmap(fullPath))
                                        {
                                            buildIndoor.InsertImage(img, 25, 25);
                                        }
                                    }
                                    //To display Heat Exchanger Image in report
                                    else if (!System.IO.File.Exists(fullPath))
                                    {
                                        navigateToFolderImages = "..\\..\\Image\\TypeImageProjectCreation\\";
                                        sourceDirImages = System.IO.Path.Combine(defaultFolder, navigateToFolderImages);
                                        string imagefullPath = sourceDirImages + room.indoors[j].Picture;
                                        if (System.IO.File.Exists(imagefullPath))
                                        {
                                            System.Drawing.Image img = new System.Drawing.Bitmap(imagefullPath);
                                            buildIndoor.InsertImage(img, 25, 25);
                                        }
                                    }
                                    //adding a default image
                                    else
                                    {
                                        fullPath = sourceDirImages + "RAS-12-18FSXNK.png";
                                    }
                                    SetCellText(buildIndoor, 0, index, 1, ind.Ident);
                                    SetCellText(buildIndoor, 0, index, 2, ind.Model);
                                    SetCellText(buildIndoor, 0, index, 3, ind.Cool);
                                    SetCellText(buildIndoor, 0, index, 4, ind.Heat);
                                    SetCellText(buildIndoor, 0, index, 5, ind.Accessories);
                                    for (int m = 0; m < control.Count; m++)
                                    {
                                        string pic = control[m].Picture;
                                        string name = control[m].Model;
                                        string gp = control[m].Gp;
                                        int indexCrl = index + m;
                                        if (!string.IsNullOrEmpty(gp) && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(pic))
                                        {
                                            //pic = remoteControlSwith[gp].Picture;
                                            if (remoteControlSwith.Keys.Contains(gp))
                                            {
                                                name = remoteControlSwith[gp].Name;
                                            }
                                        }
                                        //Old Code chandresh
                                        //fullPath = sourceDirImages + pic;
                                        //if (!File.Exists(fullPath))
                                        //{
                                        //    fullPath = sourceDirImages + "RemoteControler.png";
                                        //}
                                        //End old
                                        fullPath = sourceDirControllerSwitchImage + pic;
                                        if (!File.Exists(fullPath))
                                        {
                                            fullPath = sourceDirImages + "RemoteControler.png";
                                        }
                                        if (System.IO.File.Exists(fullPath))
                                        {

                                            buildIndoor.MoveToCell(0, indexCrl, 6, 0);
                                            using (System.Drawing.Image img = new System.Drawing.Bitmap(fullPath))
                                            {
                                                if (img.Width > img.Height)
                                                {
                                                    buildIndoor.InsertImage(img, 25, img.Height * 25 / img.Width);
                                                }
                                                else
                                                {
                                                    buildIndoor.InsertImage(img, img.Width * 25 / img.Height, 25);
                                                }
                                            }
                                        }
                                        //SetCellText(buildIndoor, 0, 3 + m, 6, control[m].Picture);
                                        SetCellText(buildIndoor, 0, indexCrl, 7, name);
                                        SetCellText(buildIndoor, 0, indexCrl, 8, gp);
                                        if (m == 0)
                                        {
                                            tb.Rows[indexCrl].Cells[0].CellFormat.VerticalMerge = CellMerge.First;
                                            tb.Rows[indexCrl].Cells[1].CellFormat.VerticalMerge = CellMerge.First;
                                            tb.Rows[indexCrl].Cells[2].CellFormat.VerticalMerge = CellMerge.First;
                                            tb.Rows[indexCrl].Cells[3].CellFormat.VerticalMerge = CellMerge.First;
                                            tb.Rows[indexCrl].Cells[4].CellFormat.VerticalMerge = CellMerge.First;
                                            tb.Rows[indexCrl].Cells[5].CellFormat.VerticalMerge = CellMerge.First;
                                        }
                                        else
                                        {
                                            tb.Rows[indexCrl].Cells[0].CellFormat.VerticalMerge = CellMerge.Previous;
                                            tb.Rows[indexCrl].Cells[1].CellFormat.VerticalMerge = CellMerge.Previous;
                                            tb.Rows[indexCrl].Cells[2].CellFormat.VerticalMerge = CellMerge.Previous;
                                            tb.Rows[indexCrl].Cells[3].CellFormat.VerticalMerge = CellMerge.Previous;
                                            tb.Rows[indexCrl].Cells[4].CellFormat.VerticalMerge = CellMerge.Previous;
                                            tb.Rows[indexCrl].Cells[5].CellFormat.VerticalMerge = CellMerge.Previous;
                                        }
                                    }
                                } // indoorsysitemtoprint closed 

                            }
                            tb.Rows[index - 1].Remove();
                            Bookmark Tmark = Doc.Range.Bookmarks["IndoorUnits"];//主文档书签
                            InsertDocument(Tmark.BookmarkStart.ParentNode, docIndoor);//表格复制
                        }//indoorsystoprint condition closed

                    }

                }
            }
            else
            {
                Builder.MoveToBookmark("IndoorUnitsTitle");
                Builder.CurrentParagraph.Remove();
            }
            Builder.MoveToBookmark("IndoorUnits");
            Builder.CurrentParagraph.Remove();
        }
        //private void fillIndoorUnits()
        //{
        //    try
        //    {
        //        if (thisProject.IsIndoorListChecked)
        //        {                   
        //            for (int i = indoorUnits.Count - 1; i >= 0; i--)
        //            {
        //                var room = indoorUnits[i];
        //                List<dynamic> indoors = room.indoors;
        //                int index = 3;
        //                if (indoors.Count > 0)
        //                {
        //                    Document docIndoor;
        //                    //docIndoor = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\IndoorUnits.doc"));
        //                    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        //                    string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\IndoorUnits.doc";
        //                    string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
        //                    docIndoor = new Document(sourceDir);
        //                    DocumentBuilder buildIndoor = new DocumentBuilder(docIndoor);
        //                    var tbs = docIndoor.GetChildNodes(Aspose.Words.NodeType.Table, true);
        //                    Table tb = (Table)tbs[0];
        //                    buildIndoor.MoveToBookmark("FloorRoom");
        //                    buildIndoor.Write(room.roomName);
        //                    buildIndoor.MoveToBookmark("CapUnit");
        //                    //  New Code After Integrate 4.7
        //                    if (utPower == "kW" && isIduCapacityW == true)
        //                    {
        //                        buildIndoor.Write("W");
        //                    }
        //                    else
        //                    {
        //                        buildIndoor.Write(utPower);
        //                    }
        //                    //  End Code After Integrate 4.7
        //                    for (int j = indoors.Count - 1; j >= 0; j--)
        //                    {
        //                        var ind = indoors[j];
        //                        List<dynamic> control = ind.Control;
        //                        int addRows = 1;
        //                        if (control.Count > 0)
        //                        {
        //                            addRows = control.Count;
        //                        }
        //                        if (j == indoors.Count - 1)
        //                        {
        //                            addRows--;
        //                        }
        //                        for (int m = 0; m < addRows; m++)
        //                        {
        //                            var row = tb.Rows[index - 1].Clone(true);
        //                            tb.InsertAfter(row, tb.Rows[index - 1]);
        //                        }
        //                        buildIndoor.MoveToCell(0, index, 0, 0);
        //                        // string fullPath = MyConfig.TypeImageDirectory + ind.Picture;
        //                        string navigateToFolderImages = "";
        //                        navigateToFolderImages = "..\\..\\Image\\TypeImages\\";
        //                        string sourceDirImages = System.IO.Path.Combine(defaultFolder, navigateToFolderImages);
        //                        string fullPath = sourceDirImages + room.indoors[j].Picture;
        //                        if (System.IO.File.Exists(fullPath))
        //                        {
        //                            System.Drawing.Image img = new System.Drawing.Bitmap(fullPath);
        //                            buildIndoor.InsertImage(img, 25, 25);
        //                        }
        //                        //To display Heat Exchanger Image in report
        //                        else if (!System.IO.File.Exists(fullPath))
        //                        {
        //                            //navigateToFolderImages = "..\\..\\Image\\TypeImageProjectCreation\\";
        //                            //sourceDirImages = System.IO.Path.Combine(defaultFolder, navigateToFolderImages);
        //                            string imagefullPath = room.indoors[j].Picture;
        //                            if (System.IO.File.Exists(imagefullPath))
        //                            {
        //                                System.Drawing.Image img = new System.Drawing.Bitmap(imagefullPath);
        //                                buildIndoor.InsertImage(img, 25, 25);
        //                            }
        //                        }
        //                        //adding a default image
        //                        else
        //                        {
        //                            fullPath = sourceDirImages + "RAS-12-18FSXNK.png";
        //                        }
        //                        SetCellText(buildIndoor, 0, index, 1, ind.Ident);
        //                        SetCellText(buildIndoor, 0, index, 2, ind.Model);
        //                        SetCellText(buildIndoor, 0, index, 3, ind.Cool);
        //                        SetCellText(buildIndoor, 0, index, 4, ind.Heat);
        //                        SetCellText(buildIndoor, 0, index, 5, ind.Accessories);
        //                        for (int m = 0; m < control.Count; m++)
        //                        {
        //                            string pic = control[m].Picture;
        //                            string name = control[m].Model;
        //                            string gp = control[m].Gp;
        //                            int indexCrl = index + m;
        //                            if (!string.IsNullOrEmpty(gp) && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(pic))
        //                            {
        //                                //pic = remoteControlSwith[gp].Picture;
        //                                if (remoteControlSwith.Keys.Contains(gp))
        //                                {
        //                                    name = remoteControlSwith[gp].Name;
        //                                }
        //                            }
        //                            fullPath = sourceDirImages + pic;
        //                            if (!File.Exists(fullPath))
        //                            {
        //                                fullPath = sourceDirImages + "RemoteControler.png";
        //                            }
        //                            if (System.IO.File.Exists(fullPath))
        //                            {
        //                                buildIndoor.MoveToCell(0, indexCrl, 6, 0);
        //                                System.Drawing.Image img = new System.Drawing.Bitmap(fullPath);
        //                                if (img.Width > img.Height)
        //                                {
        //                                    buildIndoor.InsertImage(img, 25, img.Height * 25 / img.Width);
        //                                }
        //                                else
        //                                {
        //                                    buildIndoor.InsertImage(img, img.Width * 25 / img.Height, 25);
        //                                }
        //                            }
        //                            //SetCellText(buildIndoor, 0, 3 + m, 6, control[m].Picture);
        //                            SetCellText(buildIndoor, 0, indexCrl, 7, name);
        //                            SetCellText(buildIndoor, 0, indexCrl, 8, gp);
        //                            if (m == 0)
        //                            {
        //                                tb.Rows[indexCrl].Cells[0].CellFormat.VerticalMerge = CellMerge.First;
        //                                tb.Rows[indexCrl].Cells[1].CellFormat.VerticalMerge = CellMerge.First;
        //                                tb.Rows[indexCrl].Cells[2].CellFormat.VerticalMerge = CellMerge.First;
        //                                tb.Rows[indexCrl].Cells[3].CellFormat.VerticalMerge = CellMerge.First;
        //                                tb.Rows[indexCrl].Cells[4].CellFormat.VerticalMerge = CellMerge.First;
        //                                tb.Rows[indexCrl].Cells[5].CellFormat.VerticalMerge = CellMerge.First;
        //                            }
        //                            else
        //                            {
        //                                tb.Rows[indexCrl].Cells[0].CellFormat.VerticalMerge = CellMerge.Previous;
        //                                tb.Rows[indexCrl].Cells[1].CellFormat.VerticalMerge = CellMerge.Previous;
        //                                tb.Rows[indexCrl].Cells[2].CellFormat.VerticalMerge = CellMerge.Previous;
        //                                tb.Rows[indexCrl].Cells[3].CellFormat.VerticalMerge = CellMerge.Previous;
        //                                tb.Rows[indexCrl].Cells[4].CellFormat.VerticalMerge = CellMerge.Previous;
        //                                tb.Rows[indexCrl].Cells[5].CellFormat.VerticalMerge = CellMerge.Previous;
        //                            }
        //                        }
        //                    }
        //                    tb.Rows[index - 1].Remove();
        //                    Bookmark Tmark = Doc.Range.Bookmarks["IndoorUnits"];//主文档书签
        //                    InsertDocument(Tmark.BookmarkStart.ParentNode, docIndoor);//表格复制
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Builder.MoveToBookmark("IndoorUnitsTitle");
        //            Builder.CurrentParagraph.Remove();
        //        }
        //        Builder.MoveToBookmark("IndoorUnits");
        //        Builder.CurrentParagraph.Remove();
        //    }
        //    catch { }
        //}

        /// <summary>
        /// 填充基础环境数据
        /// </summary>
        private void fillBasicData()
        {
            Builder.MoveToBookmark("OutdoorCoolingDB");
            Builder.Write(outSectionBasic.outdoorCoolingDB);
            Builder.MoveToBookmark("OutdoorHeatingDB");
            Builder.Write(outSectionBasic.outdoorHeatingDB);
            Builder.MoveToBookmark("OutdoorHeatingWB");
            Builder.Write(outSectionBasic.outdoorHeatingWB);
            Builder.MoveToBookmark("OutdoorHeatingRH");
            Builder.Write(outSectionBasic.outdoorHeatRH);

            Builder.MoveToBookmark("OutdoorCoolingInlet");
            Builder.Write(outSectionBasic.coolingInletWater);
            Builder.MoveToBookmark("OutdoorHeatingInlet");
            Builder.Write(outSectionBasic.heatingInletWater);

            Builder.MoveToBookmark("IndoorCoolingDB");
            Builder.Write(outSectionBasic.dbCool);
            Builder.MoveToBookmark("IndoorCoolingWB");
            Builder.Write(outSectionBasic.wbCool);
            Builder.MoveToBookmark("IndoorCoolingRH");
            Builder.Write(outSectionBasic.indoorCoolingRH);
            Builder.MoveToBookmark("IndoorHeatingDB");
            Builder.Write(outSectionBasic.dbHeat);

            for (int i = 0; i < tbBasic.Rows.Count; i++)
            {
                var cells = tbBasic.Rows[i].Cells;
                cells[6].Remove();
                if (outSectionBasic.coolingInletWater == "-" && outSectionBasic.heatingInletWater == "-")
                {
                    cells[3].Remove();
                }
            }
        }

        /// <summary>
        /// 填充系统数据
        /// </summary>
        private void fillSystemDesign()
        {
            var OutdoorCool = utPower;
            for (int i = systemList.Count - 1; i >= 0; i--)
            {
                var s = systemList[i];
                Document docSys;
               
                string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\SystemDesign.doc";
                string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                docSys = new Document(GetDocPath(sourceDir));
                //docSys = new Document(sourceDir);
                DocumentBuilder buildSys = new DocumentBuilder(docSys);
                var tbs = docSys.GetChildNodes(Aspose.Words.NodeType.Table, true);
                Table tbSysBasic = (Table)tbs[0];
                Table tbRoom = (Table)tbs[1];
                Table tbOutdoor = (Table)tbs[2];
                Table tbIndoor = (Table)tbs[3];
                Table tbFAIndoor = (Table)tbs[4];
                SetBookMarkText(buildSys, "SystemName", s.SystemName);
                SetBookMarkText(buildSys, "OutdoorCoolCapUnit", OutdoorCool);
                string utPowers = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                var OutdoorHeat = OutdoorCool;
                SetBookMarkText(buildSys, "OutdoorHeatCapUnit", OutdoorHeat);
                var InHeatCapUnit = utPower;
                //  New Code After Integrate 4.7
                SetBookMarkText(buildSys, "AreaUnit", utArea);
                SetBookMarkText(buildSys, "CoolUnit", utTemperature);
                SetBookMarkText(buildSys, "HeatUnit", utTemperature);
                SetBookMarkText(buildSys, "AirFlowUnit", utAirflow);
                if (utPower == "kW" && isIduCapacityW == true)
                {
                    SetBookMarkText(buildSys, "IndoorHeatCapUnit", "W");
                    SetBookMarkText(buildSys, "IndoorCoolCapUnit", "W");
                    SetBookMarkText(buildSys, "FreshAirIndoorHeatCapUnit", "W");
                    SetBookMarkText(buildSys, "FreshAirIndoorCoolCapUnit", "W");
                    SetBookMarkText(buildSys, "CoolCapUnit", "W");
                    SetBookMarkText(buildSys, "HeatCapUnit", "W");
                }
                else
                {
                    SetBookMarkText(buildSys, "IndoorHeatCapUnit", SystemSetting.UserSetting.unitsSetting.settingPOWER);
                    SetBookMarkText(buildSys, "IndoorCoolCapUnit", SystemSetting.UserSetting.unitsSetting.settingPOWER);
                    SetBookMarkText(buildSys, "CoolCapUnit", SystemSetting.UserSetting.unitsSetting.settingPOWER);
                    SetBookMarkText(buildSys, "HeatCapUnit", SystemSetting.UserSetting.unitsSetting.settingPOWER);
                    SetBookMarkText(buildSys, "FreshAirIndoorHeatCapUnit", SystemSetting.UserSetting.unitsSetting.settingPOWER);
                    SetBookMarkText(buildSys, "FreshAirIndoorCoolCapUnit", SystemSetting.UserSetting.unitsSetting.settingPOWER);
                }
                //  End Code After Integrate 4.7
                //SetBookMarkText(buildSys, "AreaUnit", utArea);
                //SetBookMarkText(buildSys, "CoolUnit", utTemperature);
                //SetBookMarkText(buildSys, "HeatUnit", utTemperature);
                //SetBookMarkText(buildSys, "AirFlowUnit", utAirflow);

                if (s.SysBasic != null)
                {
                    SetBookMarkText(buildSys, "OutdoorCoolingDB", s.SysBasic.outdoorCoolingDB);
                    SetBookMarkText(buildSys, "OutdoorHeatingDB", s.SysBasic.outdoorHeatingDB);
                    SetBookMarkText(buildSys, "OutdoorHeatingWB", s.SysBasic.outdoorHeatingWB);
                    SetBookMarkText(buildSys, "OutdoorHeatingRH", s.SysBasic.outdoorHeatRH);
                    SetBookMarkText(buildSys, "OutdoorCoolingInlet", s.SysBasic.coolingInletWater);
                    SetBookMarkText(buildSys, "OutdoorHeatingInlet", s.SysBasic.heatingInletWater);
                    SetBookMarkText(buildSys, "IndoorCoolingDB", s.SysBasic.dbCool);
                    SetBookMarkText(buildSys, "IndoorCoolingWB", s.SysBasic.wbCool);
                    SetBookMarkText(buildSys, "IndoorCoolingRH", s.SysBasic.indoorCoolingRH);
                    SetBookMarkText(buildSys, "IndoorHeatingDB", s.SysBasic.dbHeat);

                    for (int m = 0; m < tbSysBasic.Rows.Count; m++)
                    {
                        var cells = tbSysBasic.Rows[m].Cells;
                        cells[6].Remove();
                        if (s.SysBasic.coolingInletWater == "-" && s.SysBasic.heatingInletWater == "-")
                        {
                            cells[3].Remove();
                        }
                    }
                }

                string actNominal = Msg.GetResourceString(isActual ? "REPORT2_Actual" : "REPORT2_Nominal");
                Action<Node> fun = null;
                fun = new Action<Node>((n) =>
                {
                    if (n is Run)
                    {
                        (n as Run).Text = actNominal;
                    }
                    if (n is Paragraph)
                    {
                        (n as Paragraph).ChildNodes.ToArray().ToList().ForEach(fun);
                    }
                });
                tbRoom.Rows[1].Cells[6].ChildNodes.ToArray().ToList().ForEach(fun);
                tbRoom.Rows[1].Cells[9].ChildNodes.ToArray().ToList().ForEach(fun);
                tbIndoor.Rows[1].Cells[4].ChildNodes.ToArray().ToList().ForEach(fun);
                tbIndoor.Rows[1].Cells[7].ChildNodes.ToArray().ToList().ForEach(fun);
                if (tbFAIndoor != null)
                {
                    tbFAIndoor.Rows[1].Cells[5].ChildNodes.ToArray().ToList().ForEach(fun);
                    tbFAIndoor.Rows[1].Cells[8].ChildNodes.ToArray().ToList().ForEach(fun);
                }
                int tbIndex = 1;
                var roomInfo = s.RoomInformation;
                if (roomInfo != null && roomInfo.RoomList.Count > 0)
                {
                    Table tb = tbRoom;
                    List<dynamic> roomList = roomInfo.RoomList;
                    int addRows = roomList.Count - 2;
                    for (int m = 0; m < addRows; m++)
                    {
                        var row = tb.Rows[2].Clone(true);
                        tb.InsertAfter(row, tb.Rows[2]);
                    }
                    int index = 2;
                    for (int m = 0; m < roomList.Count; m++)
                    {
                        var room = roomList[m];
                        SetCellText(buildSys, tbIndex, index + m, 0, room.FloorName);
                        SetCellText(buildSys, tbIndex, index + m, 1, room.RoomName);
                        SetCellText(buildSys, tbIndex, index + m, 2, room.RoomArea);
                        SetCellText(buildSys, tbIndex, index + m, 3, room.DBCool);
                        SetCellText(buildSys, tbIndex, index + m, 4, room.WBCool);
                        SetCellText(buildSys, tbIndex, index + m, 5, room.DBHeat);
                        SetCellText(buildSys, tbIndex, index + m, 6, isActual ? room.ActCap : room.NominalCap);
                        SetCellText(buildSys, tbIndex, index + m, 7, room.ActSensible);
                        SetCellText(buildSys, tbIndex, index + m, 8, room.RqCap);
                        SetCellText(buildSys, tbIndex, index + m, 9, isActual ? room.ActHeat : room.NominalHeat);
                        SetCellText(buildSys, tbIndex, index + m, 10, room.RqHeat);
                    }
                    var total = roomInfo.Total;
                    SetCellText(buildSys, tbIndex, tb.Rows.Count - 1, 6, isActual ? total.TotalActActCap.ToString("n1") : total.TotalNominalCap.ToString("n1"));
                    SetCellText(buildSys, tbIndex, tb.Rows.Count - 1, 7, total.TotalActSensible.ToString("n1"));
                    SetCellText(buildSys, tbIndex, tb.Rows.Count - 1, 8, total.TotalRqCap.ToString("n1"));
                    SetCellText(buildSys, tbIndex, tb.Rows.Count - 1, 9, isActual ? total.TotalActHeat.ToString("n1") : total.TotalNominalHeat.ToString("n1"));
                    SetCellText(buildSys, tbIndex, tb.Rows.Count - 1, 10, total.TotalRqHeat.ToString("n1"));
                }

                var outdoors = s.OutdoorUnits;
                tbIndex++;
                if (outdoors != null && outdoors.OutdoorList.Count > 0)
                {
                    var outdoor = outdoors.OutdoorList[0];
                    SetCellText(buildSys, tbIndex, 2, 0, outdoor.Name);
                    SetCellText(buildSys, tbIndex, 2, 1, "");
                    SetCellText(buildSys, tbIndex, 2, 2, outdoor.ActRatio);
                    SetCellText(buildSys, tbIndex, 2, 3, outdoor.MaxRatio);
                    SetCellText(buildSys, tbIndex, 2, 4, isActual ? "-" : outdoor.NominalCap);
                    SetCellText(buildSys, tbIndex, 2, 5, isActual ? outdoor.ActCap : "-");
                    SetCellText(buildSys, tbIndex, 2, 6, outdoor.RqCap == "0" ? "-" : outdoor.RqCap);
                    SetCellText(buildSys, tbIndex, 2, 7, isActual ? "-" : outdoor.NominalHeat);
                    SetCellText(buildSys, tbIndex, 2, 8, isActual ? outdoor.ActHeat : "-");
                    SetCellText(buildSys, tbIndex, 2, 9, outdoor.RqHeat == "0" ? "-" : outdoor.RqHeat);
                }
                var totalOutdoor = outdoors.Total;
                SetCellText(buildSys, tbIndex, tbOutdoor.Rows.Count - 1, 4, isActual ? "-" : totalOutdoor.NominalCap);
                SetCellText(buildSys, tbIndex, tbOutdoor.Rows.Count - 1, 5, isActual ? totalOutdoor.ActCap : "-");
                SetCellText(buildSys, tbIndex, tbOutdoor.Rows.Count - 1, 6, totalOutdoor.RqCap.ToString() == "0" ? "-" : totalOutdoor.RqCap);
                SetCellText(buildSys, tbIndex, tbOutdoor.Rows.Count - 1, 7, isActual ? "-" : totalOutdoor.NominalHeat);
                SetCellText(buildSys, tbIndex, tbOutdoor.Rows.Count - 1, 8, isActual ? totalOutdoor.ActHeat : "-");
                SetCellText(buildSys, tbIndex, tbOutdoor.Rows.Count - 1, 9, totalOutdoor.RqHeat.ToString() == "0" ? "-" : totalOutdoor.RqHeat);

                var indoors = s.IndoorUnits;
                tbIndex++;
                if (indoors != null && indoors.IndoorList.Count > 0)
                {
                    Row rowTitle = tbIndoor.Rows[2];
                    Row rowContent = tbIndoor.Rows[3];
                    Row rowLast = tbIndoor.Rows[4];
                    List<dynamic> indList = indoors.IndoorList;
                    for (int m = indList.Count - 1; m >= 0; m--)
                    {
                        var roomOfInds = indList[m];
                        var room = roomOfInds.RoomInfo;
                        var rowClone = rowTitle.Clone(true);
                        tbIndoor.InsertBefore(rowClone, tbIndoor.Rows[2]);
                        List<dynamic> inds = roomOfInds.Indoors;
                        for (int n = 0; n < inds.Count; n++)
                        {
                            rowClone = rowContent.Clone(true);
                            tbIndoor.InsertAfter(rowClone, tbIndoor.Rows[2]);
                        }
                        SetCellText(buildSys, tbIndex, 1, 4, SystemSetting.UserSetting.unitsSetting.settingAIRFLOW);
                        SetCellText(buildSys, tbIndex, 2, 0, room.FloorName == "" ? room.RoomName : (room.FloorName + "-" + room.RoomName));
                        SetCellText(buildSys, tbIndex, 2, 3, isActual ? room.ActCap.ToString("n1") : room.NominalCap.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 4, room.ActSensible.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 5, room.RqCap.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 6, isActual ? room.ActHeat.ToString("n1") : room.NominalHeat.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 7, room.RqHeat.ToString("n1"));

                        for (int n = 0; n < inds.Count; n++)
                        {
                            var ind = inds[n];
                            SetCellText(buildSys, tbIndex, 3 + n, 0, ind.Name);
                            SetCellText(buildSys, tbIndex, 3 + n, 1, ind.Ident);
                            SetCellText(buildSys, tbIndex, 3 + n, 2, ind.SoundPressure);
                            SetCellText(buildSys, tbIndex, 3 + n, 3, GetSoundPressureLev(ind.FanSpeedLevel));
                            SetCellText(buildSys, tbIndex, 3 + n, 4, isActual ? ind.ActCap : ind.NominalCap);
                            SetCellText(buildSys, tbIndex, 3 + n, 5, ind.ActSensible);
                            SetCellText(buildSys, tbIndex, 3 + n, 6, ind.RqCap);
                            SetCellText(buildSys, tbIndex, 3 + n, 7, isActual ? ind.ActHeat : ind.NominalHeat);
                            SetCellText(buildSys, tbIndex, 3 + n, 8, ind.RqHeat);
                        }
                    }
                    var color = rowLast.Cells[0].CellFormat.Borders.Bottom.Color;
                    var lineWidth = rowLast.Cells[0].CellFormat.Borders.Bottom.LineWidth;
                    rowTitle.Remove();
                    rowContent.Remove();
                    rowLast.Remove();
                    var cells = tbIndoor.Rows[tbIndoor.Count - 1].Cells;
                    for (int m = 0; m < cells.Count; m++)
                    {
                        cells[m].CellFormat.Borders.Bottom.Color = color;
                        cells[m].CellFormat.Borders.Bottom.LineWidth = lineWidth;
                    }

                }
                var freshAirIndoors = s.FreshAirIndoors;
                tbIndex++;
                if (freshAirIndoors != null && freshAirIndoors.IndoorList.Count > 0 && tbFAIndoor != null)
                {
                    Row rowTitle = tbFAIndoor.Rows[2];
                    Row rowContent = tbFAIndoor.Rows[3];
                    Row rowLast = tbFAIndoor.Rows[4];
                    List<dynamic> indList = freshAirIndoors.IndoorList;
                    for (int m = indList.Count - 1; m >= 0; m--)
                    {
                        var roomOfInds = indList[m];
                        var room = roomOfInds.RoomInfo;
                        var rowClone = rowTitle.Clone(true);
                        tbFAIndoor.InsertBefore(rowClone, tbFAIndoor.Rows[2]);
                        List<dynamic> inds = roomOfInds.Indoors;
                        for (int n = 0; n < inds.Count; n++)
                        {
                            rowClone = rowContent.Clone(true);
                            tbFAIndoor.InsertAfter(rowClone, tbFAIndoor.Rows[2]);
                        }
                        SetCellText(buildSys, tbIndex, 2, 0, room.FloorName == "" ? room.RoomName : (room.FloorName + "-" + room.RoomName));
                        SetCellText(buildSys, tbIndex, 2, 4, isActual ? room.ActCap.ToString("n1") : room.NominalCap.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 5, room.ActSensible.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 6, room.RqCap.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 7, isActual ? room.ActHeat.ToString("n1") : room.NominalHeat.ToString("n1"));
                        SetCellText(buildSys, tbIndex, 2, 8, room.RqHeat.ToString("n1"));

                        for (int n = 0; n < inds.Count; n++)
                        {
                            var ind = inds[n];
                            SetCellText(buildSys, tbIndex, 3 + n, 0, ind.Name);
                            SetCellText(buildSys, tbIndex, 3 + n, 1, ind.Ident);
                            SetCellText(buildSys, tbIndex, 3 + n, 2, ind.SoundPressure);
                            SetCellText(buildSys, tbIndex, 3 + n, 3, GetSoundPressureLev(ind.FanSpeedLevel));
                            SetCellText(buildSys, tbIndex, 3 + n, 4, ind.AirFlow);
                            SetCellText(buildSys, tbIndex, 3 + n, 5, isActual ? ind.ActCap : ind.NominalCap);
                            SetCellText(buildSys, tbIndex, 3 + n, 6, ind.ActSensible);
                            SetCellText(buildSys, tbIndex, 3 + n, 7, ind.RqCap);
                            SetCellText(buildSys, tbIndex, 3 + n, 8, isActual ? ind.ActHeat : ind.NominalHeat);
                            SetCellText(buildSys, tbIndex, 3 + n, 9, ind.RqHeat);
                        }
                    }
                    var color = rowLast.Cells[0].CellFormat.Borders.Bottom.Color;
                    var lineWidth = rowLast.Cells[0].CellFormat.Borders.Bottom.LineWidth;
                    rowTitle.Remove();
                    rowContent.Remove();
                    rowLast.Remove();
                    var cells = tbFAIndoor.Rows[tbFAIndoor.Count - 1].Cells;
                    for (int m = 0; m < cells.Count; m++)
                    {
                        cells[m].CellFormat.Borders.Bottom.Color = color;
                        cells[m].CellFormat.Borders.Bottom.LineWidth = lineWidth;
                    }
                }
                if (!(roomInfo != null && roomInfo.RoomList.Count > 0) || !thisProject.IsRoomInfoChecked)
                {
                    buildSys.MoveToBookmark("RoomInformation");
                    buildSys.CurrentParagraph.Remove();
                    tbRoom.NextSibling.Remove();
                    tbRoom.Remove();
                }
                if (!thisProject.IsOutdoorListChecked)
                {
                    buildSys.MoveToBookmark("OutdoorUnitsOfTheSystem");
                    buildSys.CurrentParagraph.Remove();
                    tbOutdoor.NextSibling.Remove();
                    tbOutdoor.Remove();
                }
                if (!thisProject.IsIndoorListChecked)
                {
                    buildSys.MoveToBookmark("IndoorUnitsOfTheSystem");
                    buildSys.CurrentParagraph.Remove();
                    tbIndoor.NextSibling.Remove();
                    tbIndoor.Remove();
                }
                if (!(freshAirIndoors != null && freshAirIndoors.IndoorList.Count > 0) || !thisProject.IsIndoorListChecked)
                {
                    buildSys.MoveToBookmark("FreshAirIndoorUnits");
                    buildSys.CurrentParagraph.Remove();
                    if (tbFAIndoor != null)
                    {
                        tbFAIndoor.NextSibling.Remove();
                        tbFAIndoor.Remove();
                    }
                }

                Bookmark Tmark = Doc.Range.Bookmarks["SystemDesign"];//主文档书签
                InsertDocument(Tmark.BookmarkStart.ParentNode, docSys);//表格复制
            }
            Builder.MoveToBookmark("SystemDesign");
            Builder.CurrentParagraph.Remove();
        }

        /// <summary>
        /// 填充Piping
        /// </summary>
        private void fillPipingDesign()
        {
            if (thisProject.IsPipingDiagramChecked)
            {
                for (int i = pipingList.Count - 1; i >= 0; i--)
                {
                    var piping = pipingList[i];
                    Document docPiping;
                    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                    string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\PipingDesign.doc";
                    string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                    docPiping = new Document(GetDocPath(sourceDir));

                    //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                    //string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\PipingDesign.doc";
                    //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                    //docPiping = new Document(sourceDir);
                    // docPiping = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\PipingDesign.doc"));

                    DocumentBuilder buildPiping = new DocumentBuilder(docPiping);
                    var tbs = docPiping.GetChildNodes(Aspose.Words.NodeType.Table, true);
                    Table tbLimit = (Table)tbs[0];
                    Table tbPipingRules = (Table)tbs[0];
                    Table tbRefrigerant = (Table)tbs[1];

                    SetBookMarkText(buildPiping, "SystemName", piping.Name);
                    SetBookMarkText(buildPiping, "LengthUnit", utLength);
                    SetBookMarkText(buildPiping, "MaxUnit", utLength);
                    SetBookMarkText(buildPiping, "WeightUnit", utWeight);

                    SetBookMarkText(buildPiping, "IUConnectable", piping.IUCount);
                    SetBookMarkText(buildPiping, "Max", piping.MaxIU);
                    SetBookMarkText(buildPiping, "OutdoorName", piping.OutdoorName);
                    SetBookMarkText(buildPiping, "OutdoorName2", piping.OutdoorName);

                    //  SetBookMarkText(buildPiping, "PipingDiagram", piping.PipingDiagram);
                    SetBookMarkText(buildPiping, "Rate", piping.Rate);
                    SetBookMarkText(buildPiping, "RateRange", piping.RateRange);
                    SetBookMarkText(buildPiping, "Recommended", piping.RecommendedIU);
                    if (!isActual)
                    {
                        SetBookMarkText(buildPiping, "PipingMsg", Msg.GetResourceString("REPORT2_PipingMsg"));
                    }
                    else
                    {
                        buildPiping.MoveToBookmark("PipingMsg");
                        if (buildPiping.CurrentParagraph.ParentNode != null)
                        {
                            buildPiping.CurrentParagraph.Remove();
                        }
                    }

                    if (piping.Actuallength != 0)
                    {
                        buildPiping.MoveToBookmark("ZeroPipeLenMsg");
                        if (buildPiping.CurrentParagraph.ParentNode != null)
                        {
                            buildPiping.CurrentParagraph.Remove();
                        }
                    }

                    buildPiping.MoveToBookmark("PipingDiagram");
                    string fullPath = piping.PipingDiagram;
                    InsertImage(fullPath, buildPiping);
                    List<dynamic> lengthList = piping.PipingRules.Length;
                    List<dynamic> heightList = piping.PipingRules.Height;
                    int totalLengthCount = piping.TotalLengthCount - 3;
                    int totalHeightCount = piping.TotalHeightCount - 3;
                    var rowL = tbPipingRules.Rows[3];
                    var rowH = tbPipingRules.Rows[6];
                    for (int m = 0; m < totalLengthCount; m++)
                    {
                        var rowClone = rowL.Clone(true);
                        tbPipingRules.InsertAfter(rowClone, rowL);
                    }
                    for (int m = 0; m < totalHeightCount; m++)
                    {
                        var rowClone = rowH.Clone(true);
                        tbPipingRules.InsertAfter(rowClone, rowH);
                    }
                    int index = 2;
                    lengthList.ForEach((r) =>
                    {
                        SetCellText(buildPiping, 0, index, 1, r.Rules);
                        SetCellText(buildPiping, 0, index, 2, r.Actual);
                        SetCellText(buildPiping, 0, index, 3, r.Max);
                        index++;
                        if (r.Rules == Msg.GetResourceString("REPORT2_PipingRules_CHBoxs"))//"Total piping length between CH-Box and Each Indoor Unit"
                        {
                            List<dynamic> chboxs = r.CHBoxs;
                            chboxs.ForEach((c) =>
                            {
                                buildPiping.MoveToCell(0, index, 1, 0);
                                buildPiping.ParagraphFormat.Alignment = ParagraphAlignment.Right;
                                buildPiping.Write(c.Rules);
                                SetCellText(buildPiping, 0, index, 2, c.Actual);
                                SetCellText(buildPiping, 0, index, 3, c.Max);
                                index++;
                            });
                        }
                    });
                    heightList.ForEach((r) =>
                    {
                        SetCellText(buildPiping, 0, index, 1, r.Rules);
                        SetCellText(buildPiping, 0, index, 2, r.Actual);
                        SetCellText(buildPiping, 0, index, 3, r.Max);
                        index++;
                    });

                    SetCellText(buildPiping, 1, 2, 1, piping.RefrigerantLoad.Before);
                    SetCellText(buildPiping, 1, 3, 1, piping.RefrigerantLoad.Add);
                    SetCellText(buildPiping, 1, 4, 1, piping.RefrigerantLoad.Total);
                    //bool is2Pipes = piping.Is2Pipes;
                    //if (is2Pipes)
                    //{
                    //    tbLimit.Remove();
                    //}
                    Bookmark Tmark = Doc.Range.Bookmarks["PipingDesign"];//主文档书签
                    InsertDocument(Tmark.BookmarkStart.ParentNode, docPiping);//表格复制
                }
            }
            else
            {
                Builder.MoveToBookmark("PipingDesignTitle");
                Builder.CurrentParagraph.Remove();
            }
            Builder.MoveToBookmark("PipingDesign");
            Builder.CurrentParagraph.Remove();
        }

        /// <summary>
        /// 填充wiring
        /// </summary>
        private void fillWiringDesign()
        {
            if (thisProject.IsWiringDiagramChecked)
            {
                for (int i = wiringList.Count - 1; i >= 0; i--)
                {
                    var wiring = wiringList[i];
                    Document docWiring;

                    // docWiring = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\WiringDesign.doc"));
                    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                    //string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\WiringDesign.doc";
                    string navigateToFolder = string.Empty;
                    if (wiring.Name.Contains("Group"))
                    {
                        navigateToFolder = "..\\..\\Report\\Template\\NewReport\\WiringDesignCE.doc";
                        string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                        docWiring = new Document(sourceDir);
                    }
                    else
                    {
                        navigateToFolder = "..\\..\\Report\\Template\\NewReport\\WiringDesign.doc";
                        string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                        docWiring = new Document(GetDocPath(sourceDir));
                    }
                    
                    DocumentBuilder buildWiring = new DocumentBuilder(docWiring);
                    var tbs = docWiring.GetChildNodes(Aspose.Words.NodeType.Table, true);
                    SetBookMarkText(buildWiring, "SystemName", wiring.Name);
                    //Bug 4776- commenting the below two lines to fix this issue.
                    //SetBookMarkText(buildWiring, "PowerUnit", utPowerInput);
                    //SetBookMarkText(buildWiring, "CenterControlSwitch", "0");//TODO: 
                    //Bug 4776- commenting the above two lines to fix this issue.
                    buildWiring.MoveToBookmark("WiringDiagram");
                    string fullPath = wiring.WiringDiagram;
                    InsertImage(fullPath, buildWiring);

                    List<dynamic> powerSupply = wiring.PowerSupply;
                    if (wiring.PowerSupply.Count != 0)
                    {
                        Table tbPower = (Table)tbs[0];
                        Row row = tbPower.Rows[2];
                        int addRows = powerSupply.Count - 2;
                        for (int m = 0; m < addRows; m++)
                        {
                            var rowCopy = row.Clone(true);
                            tbPower.InsertAfter(rowCopy, row);
                        }
                        int index = 2;
                        powerSupply.ForEach((p) =>
                        {

                            buildWiring.MoveToCell(0, index, 0, 0);

                            // fullPath = MyConfig.TypeImageDirectory + p.Picture;

                            string defaultFolders = AppDomain.CurrentDomain.BaseDirectory;
                            //string navigateToFolder = "..\\..\\Report\\NodeImagePiping\\";
                            string navigateToFolders = "..\\..\\Image\\TypeImages\\";
                            string sourceDirs = System.IO.Path.Combine(defaultFolders, navigateToFolders);

                            string navigateToFolderImageWiring = "..\\..\\Report\\Template\\NewReport\\WiringDesign.doc";
                            string sourceDirImageWiring = System.IO.Path.Combine(defaultFolder, navigateToFolderImageWiring);
                            if (p.Picture != null)
                            {
                                fullPath = sourceDirs + p.Picture;
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.Drawing.Image img = new System.Drawing.Bitmap(fullPath);
                                    buildWiring.InsertImage(img, 35, 35);
                                }
                                SetCellText(buildWiring, 0, index, 1, p.Model == null ? "" : p.Model);
                                SetCellText(buildWiring, 0, index, 2, p.PowerSupply);
                                SetCellText(buildWiring, 0, index, 3, p.InputPower == 0 ? "-" : p.InputPower);
                                SetCellText(buildWiring, 0, index, 4, p.MaxCurrent == null ? "-" : p.MaxCurrent);
                            }
                            index++;
                        });
                    }
                    else
                    {
                        //buildWiring.CurrentParagraph.Document.Range.Bookmarks[3].BookmarkStart.Remove();
                        //buildWiring.CurrentParagraph.Remove();
                        //tbs[0].ParentNode.Remove();
                        //Doc.Sections.RemoveAt(1);

                        //tbs[0].Remove();

                    }
                    Bookmark Tmark = Doc.Range.Bookmarks["WiringDesign"];//主文档书签
                    InsertDocument(Tmark.BookmarkStart.ParentNode, docWiring);//表格复制                    
                }
            }
            else
            {
                Builder.MoveToBookmark("WiringDesignTitle");
                Builder.CurrentParagraph.Remove();
            }
            Builder.MoveToBookmark("WiringDesign");
            Builder.CurrentParagraph.Remove();
        }

        /// <summary>
        /// 填充所有产品汇总数据
        /// </summary>
        /// 

        private void fillProductList()
        {
            //Document docProduct;
            //docProduct = new Document(GetDocPath(MyConfig.ReportTemplateDirectory + @"NewReport\EquipmentListAndInformation.doc"));
            //DocumentBuilder buildProduct = new DocumentBuilder(docProduct);
            Document docProduct;
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\NewReport\\EquipmentListAndInformation.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            //docProduct = new Document(sourceDir);
            docProduct = new Document(GetDocPath(sourceDir));
            DocumentBuilder buildProduct = new DocumentBuilder(docProduct);
            var tbs = docProduct.GetChildNodes(Aspose.Words.NodeType.Table, true);
            Table tbOutdoors = (Table)tbs[0];
            Table tbIndoor = (Table)tbs[1];
            Table tbAccessories = (Table)tbs[2];
            Table tbHeatExchangerUnits = (Table)tbs[3];
            Table tbexcAccessories = (Table)tbs[4];
            Table tbControllers = (Table)tbs[5];
            Table tbPipeConnectionKit = (Table)tbs[6];
            Table tbMultikit = (Table)tbs[7];
            Table tbCHBox = (Table)tbs[8];
            Table tbPipingMaterials = (Table)tbs[9];
            Table tbRefrigerant = (Table)tbs[10];
            SetBookMarkText(buildProduct, "WeightUnit", utWeight);
            SetBookMarkText(buildProduct, "LengthUnit", utLength);
            SetBookMarkText(buildProduct, "PipeSizeUnit", utDimension);
            List<Action> listAction = new List<Action>();
            if (listOutdoors.Count > 0)
            {
                var rowHearder = tbOutdoors.Rows[2];
                var rowComponents = tbOutdoors.Rows[3];
                var rowLast = tbOutdoors.Rows[tbOutdoors.Rows.Count - 1];
                for (int i = listOutdoors.Count - 1; i >= 0; i--)
                {
                    var outdoor = listOutdoors[i];
                    var rowCopy = rowHearder.Clone(true);
                    tbOutdoors.InsertBefore(rowCopy, tbOutdoors.Rows[2]);
                    List<dynamic> components = outdoor.Components;
                    components.ForEach((c) =>
                    {
                        rowCopy = rowComponents.Clone(true);
                        tbOutdoors.InsertAfter(rowCopy, tbOutdoors.Rows[2]);
                    });
                    SetCellText(buildProduct, 0, 2, 0, outdoor.Name);
                    SetCellText(buildProduct, 0, 2, 1, outdoor.System);
                    //(buildProduct, 0, 2, 2, outdoor.Series);
                    SetCellText(buildProduct, 0, 2, 2, trans.getTypeTransStr(TransType.Series.ToString(), outdoor.Series));
                    SetCellText(buildProduct, 0, 2, 3, outdoor.Qty);
                    int index = 3;
                    components.ForEach((c) =>
                    {
                        SetCellText(buildProduct, 0, index, 0, c.Name.Trim());
                        SetCellText(buildProduct, 0, index, 1, "-");
                        SetCellText(buildProduct, 0, index, 2, "Components");
                        SetCellText(buildProduct, 0, index, 3, outdoor.Qty);
                        index++;
                    });
                }
                listAction.Add(delegate ()
                {
                    var color = tbOutdoors.Rows[tbOutdoors.Rows.Count - 1].Cells[0].CellFormat.Borders.Bottom.Color;
                    var lineWidth = tbOutdoors.Rows[tbOutdoors.Rows.Count - 1].Cells[0].CellFormat.Borders.Bottom.LineWidth;
                    var cells = tbOutdoors.Rows[tbOutdoors.Rows.Count - 1 - 3].Cells;
                    //var cells = tbOutdoors.Rows[tbOutdoors.Rows.Count - 1].Cells;
                    for (int m = 0; m < cells.Count; m++)
                    {
                        cells[m].CellFormat.Borders.Bottom.Color = color;
                        cells[m].CellFormat.Borders.Bottom.LineStyle = LineStyle.Single;
                        cells[m].CellFormat.Borders.Bottom.LineWidth = lineWidth;
                    }
                    tbOutdoors.Rows[tbOutdoors.Rows.Count - 1].Remove();
                    tbOutdoors.Rows[tbOutdoors.Rows.Count - 1].Remove();
                    tbOutdoors.Rows[tbOutdoors.Rows.Count - 1].Remove();

                });
            }
            if (listIndoors.Count > 0)
            {
                int addRows = listIndoors.Count > 2 ? (listIndoors.Count - 2) : 0;
                var row = tbIndoor.Rows[2];
                var rowLast = tbIndoor.Rows[tbIndoor.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbIndoor.InsertAfter(rowCopy, tbIndoor.Rows[2]);
                }
                int index = listIndoors.Count == 1 ? 3 : 2;
                //chandresh - 4.7.2 code

                //var list = listIndoors.OrderBy(p => p.Type).ThenBy(p => p.Horsepower).ToList();
                //list.ForEach((d) => {
                //    SetCellText(buildProduct, 1, index, 0, d.Name);
                //    //SetCellText(buildProduct, 1, index, 1, d.System);
                //    SetCellText(buildProduct, 1, index, 1, d.Type);
                //    SetCellText(buildProduct, 1, index, 2, d.Qty);
                //    index++;
                //});
                //chandresh - 4.7.2 code

                //chandresh keeping old code
                var list = listIndoors.OrderBy(p => p.Type).ThenBy(p => p.Horsepower).ToList();
                //var list = listIndoors.OrderBy(p => p.System).ThenBy(p => p.Type).ThenBy(p => p.Horsepower).ToList();
                list.ForEach((d) =>
                {
                    var proj = this.thisProject.SystemListNextGen.FindAll(a => a.Id == d.SystemId).ToList();
                    foreach (var obj in proj)
                    {
                        if (obj.IsExportToReport == true)
                        {
                            SetCellText(buildProduct, 1, index, 0, d.Name);
                            // SetCellText(buildProduct, 1, index, 1, d.System);
                            SetCellText(buildProduct, 1, index, 1, d.Type);
                            SetCellText(buildProduct, 1, index, 2, d.Qty);
                            index++;
                        }
                    }
                });
                //chandresh keeping old code
                listAction.Add(delegate ()
                {
                    if (listIndoors.Count == 1)
                    {
                        tbIndoor.Rows[2].Remove();
                    }
                });
            }
            if (listAccessories.Count > 0)
            {
                int addRows = listAccessories.Count > 2 ? (listAccessories.Count - 2) : 0;
                var row = tbAccessories.Rows[2];
                var rowLast = tbAccessories.Rows[tbAccessories.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbAccessories.InsertAfter(rowCopy, tbAccessories.Rows[2]);
                }
                int index = listAccessories.Count == 1 ? 3 : 2;
                //var list = listAccessories.OrderBy(p => p.System).ThenBy(p => p.Type).ToList();
                var list = listAccessories.OrderBy(p => p.Type).ToList();
                list.ForEach((d) =>
                {
                    SetCellText(buildProduct, 2, index, 0, d.Name);
                    //SetCellText(buildProduct, 2, index, 1, d.System);
                    SetCellText(buildProduct, 2, index, 1, d.Type);
                    SetCellText(buildProduct, 2, index, 2, d.Qty);
                    index++;
                });
                listAction.Add(delegate ()
                {
                    if (listAccessories.Count == 1)
                    {
                        tbAccessories.Rows[2].Remove();
                    }
                });
            }
            //if (listAccessories.Count > 0)
            //{
            //    int addRows = listAccessories.Count > 2 ? (listAccessories.Count - 2) : 0;
            //    var row = tbAccessories.Rows[2];
            //    var rowLast = tbAccessories.Rows[tbAccessories.Rows.Count - 1];
            //    for (int i = 0; i < addRows; i++)
            //    {
            //        var rowCopy = row.Clone(true);
            //        tbAccessories.InsertAfter(rowCopy, tbAccessories.Rows[2]);
            //    }
            //    int index = listAccessories.Count == 1 ? 3 : 2;
            //    //var list = listAccessories.OrderBy(p => p.System).ThenBy(p => p.Type).ToList();
            //    var list = listAccessories.OrderBy(p => p.Type).ToList();
            //    list.ForEach((d) =>
            //    {
            //        SetCellText(buildProduct, 2, index, 0, d.Name);
            //        //SetCellText(buildProduct, 2, index, 1, d.System);
            //        SetCellText(buildProduct, 2, index, 1, d.Type);
            //        SetCellText(buildProduct, 2, index, 2, d.Qty);
            //        index++;
            //    });
            //    listAction.Add(delegate ()
            //    {
            //        if (listAccessories.Count == 1)
            //        {
            //            tbAccessories.Rows[2].Remove();
            //        }
            //    });
            //}
            if (heatExchangerUnits.Count > 0)
            {
                int addRows = heatExchangerUnits.Count > 2 ? (heatExchangerUnits.Count - 2) : 0;
                var row = tbHeatExchangerUnits.Rows[2];
                var rowLast = tbHeatExchangerUnits.Rows[tbHeatExchangerUnits.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbHeatExchangerUnits.InsertAfter(rowCopy, tbHeatExchangerUnits.Rows[2]);
                }
                int index = heatExchangerUnits.Count == 1 ? 3 : 2;
                heatExchangerUnits.ForEach((d) =>
                {
                    SetCellText(buildProduct, 3, index, 0, d.Name);
                    //SetCellText(buildProduct, 3, index, 1, "-");
                    SetCellText(buildProduct, 3, index, 1, d.Type);
                    SetCellText(buildProduct, 3, index, 2, d.Qty);
                    index++;
                });
                listAction.Add(delegate ()
                {
                    if (heatExchangerUnits.Count == 1)
                    {
                        tbHeatExchangerUnits.Rows[2].Remove();
                    }
                });
            }
            if (exc_listAccessories.Count > 0)
            {
                int addRows = exc_listAccessories.Count > 2 ? (exc_listAccessories.Count - 2) : 0;
                var row = tbexcAccessories.Rows[2];
                var rowLast = tbexcAccessories.Rows[tbexcAccessories.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbexcAccessories.InsertAfter(rowCopy, tbexcAccessories.Rows[2]);
                }
                int index = exc_listAccessories.Count == 1 ? 3 : 2;
                var list = exc_listAccessories.OrderBy(p => p.Type).ToList();
                list.ForEach((d) =>
                {
                    SetCellText(buildProduct, 4, index, 0, d.Name);
                    SetCellText(buildProduct, 4, index, 1, d.Type);
                    SetCellText(buildProduct, 4, index, 2, d.Qty);
                    index++;
                });
                listAction.Add(delegate ()
                {
                    if (exc_listAccessories.Count == 1)
                    {
                        tbexcAccessories.Rows[2].Remove();
                    }
                });
            }
            if (listControls.Count > 0)
            {
                int addRows = listControls.Count > 2 ? (listControls.Count - 2) : 0;
                var row = tbControllers.Rows[2];
                var rowLast = tbControllers.Rows[tbControllers.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbControllers.InsertAfter(rowCopy, tbControllers.Rows[2]);
                }
                int index = listControls.Count == 1 ? 3 : 2;
                listControls.ForEach((d) =>
                {
                    SetCellText(buildProduct, 5, index, 0, d.Name);
                    //SetCellText(buildProduct, 4, index, 1, "-");
                    SetCellText(buildProduct, 5, index, 1, d.Type);
                    SetCellText(buildProduct, 5, index, 2, d.Qty);
                    index++;
                });
                listAction.Add(delegate ()
                {
                    if (listControls.Count == 1)
                    {
                        tbControllers.Rows[2].Remove();
                    }
                });
            }
            if (listPipingConnectionKit.Count > 0)
            {
                int addRows = listPipingConnectionKit.Count > 2 ? (listPipingConnectionKit.Count - 2) : 0;
                var row = tbPipeConnectionKit.Rows[2];
                var rowLast = tbPipeConnectionKit.Rows[tbPipeConnectionKit.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbPipeConnectionKit.InsertAfter(rowCopy, tbPipeConnectionKit.Rows[2]);
                }
                int index = listPipingConnectionKit.Count == 1 ? 3 : 2;
                listPipingConnectionKit.ForEach((d) =>
                {
                    SetCellText(buildProduct, 6, index, 0, d.Name);
                    //SetCellText(buildProduct, 5, index, 1, d.System);
                    SetCellText(buildProduct, 6, index, 1, d.Type);
                    SetCellText(buildProduct, 6, index, 2, d.Qty);
                    index++;
                });
                listAction.Add(delegate ()
                {
                    if (listPipingConnectionKit.Count == 1)
                    {
                        tbPipeConnectionKit.Rows[2].Remove();
                    }
                });
            }
            if (listBranchKit.Count > 0)
            {
                int addRows = listBranchKit.Count > 2 ? (listBranchKit.Count - 2) : 0;
                var row = tbMultikit.Rows[2];
                var rowLast = tbMultikit.Rows[tbMultikit.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbMultikit.InsertAfter(rowCopy, tbMultikit.Rows[2]);
                }
                int index = listBranchKit.Count == 1 ? 3 : 2;
                listBranchKit.ForEach((d) =>
                {
                    SetCellText(buildProduct, 7, index, 0, d.Name);
                    //SetCellText(buildProduct, 6, index, 1, d.System);
                    SetCellText(buildProduct, 7, index, 1, d.Type);
                    SetCellText(buildProduct, 7, index, 2, d.Qty);
                    index++;
                });
                //var projs = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.ToList();
                ////int index = projs.Count == 1 ? 3 : 2;
                //for (var i = 0; i < projs.Count(); i++)
                //{
                //    if (projs[i].IsExportToReport == true)
                //    {
                //        WL.AddFlow addFlowItemPiping = projs[i].MyPipingNodeOut.AddFlow;
                //        var nodes = addFlowItemPiping.Items.OfType<WL.Node>().ToArray();
                //        List<string> Listmodel = new List<string>();
                //        var branchkit = addFlowItemPiping.Items.OfType<JCHVRF.Model.NextGen.MyNodeYP>().GroupBy(a => a.Model).Select(b => new
                //        {
                //            Name = b.Key,
                //            System = projs[i].Name,
                //            Type = "Line branch kit",
                //            Count = b.Count()
                //        });
                //        foreach (var item in branchkit)
                //        {
                //            SetCellText(buildProduct, 7, index, 0, item.Name);
                //            // SetCellText(buildProduct, 6, index, 1, item.System);
                //            SetCellText(buildProduct, 7, index, 1, item.Type);
                //            SetCellText(buildProduct, 7, index, 2, item.Count);
                //            index++;
                //        }

                //    }
                //}
                listAction.Add(delegate ()
                {
                    if (listBranchKit.Count == 1)
                    {
                        tbMultikit.Rows[2].Remove();
                    }
                });
            }
            //bool IsCHBoxCountExists = false;
            if (listCHBox.Count > 0)
            {
                int addRows = listCHBox.Count > 2 ? (listCHBox.Count - 2) : 0;
                var row = tbCHBox.Rows[2];
                var rowLast = tbCHBox.Rows[tbCHBox.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbCHBox.InsertAfter(rowCopy, tbCHBox.Rows[2]);
                }
                int index = listCHBox.Count == 1 ? 3 : 2;

                listCHBox.ForEach((d) =>
                {
                    SetCellText(buildProduct, 8, index, 0, d.Name);
                    //SetCellText(buildProduct, 7, index, 1, d.System);
                    SetCellText(buildProduct, 8, index, 1, d.Type);
                    SetCellText(buildProduct, 8, index, 2, d.Qty);
                    index++;
                });
                //listCHBox.ForEach((d) =>
                //{

                //    var list = thisProject.SystemListNextGen.FindAll(a => a.Id == d.Id).ToList();
                //    if (list.Count > 0)
                //        IsCHBoxCountExists = true;
                //    for (int k = 0; k < list.Count; k++)
                //    {
                //        var ChBoxQty = list[k].MyWiringNodeOut.ChildNodes.Count;
                //        SetCellText(buildProduct, 8, index, 0, d.Name);
                //        // SetCellText(buildProduct, 7, index, 1, d.System);
                //        SetCellText(buildProduct, 8, index, 1, d.Type);
                //        SetCellText(buildProduct, 8, index, 2, ChBoxQty);
                //        index++;

                //    }

                //});
                listAction.Add(delegate ()
                {
                    if (listCHBox.Count == 1)
                    {
                        tbCHBox.Rows[2].Remove();
                    }
                });
            }

            if (listPipingLen.Count > 0)
            {
                int addRows = listPipingLen.Count > 2 ? (listPipingLen.Count - 2) : 0;
                var row = tbPipingMaterials.Rows[2];
                var rowLast = tbPipingMaterials.Rows[tbPipingMaterials.Rows.Count - 1];
                for (int i = 0; i < addRows; i++)
                {
                    var rowCopy = row.Clone(true);
                    tbPipingMaterials.InsertAfter(rowCopy, tbPipingMaterials.Rows[2]);
                }
                int index = listPipingLen.Count == 1 ? 3 : 2;
                listPipingLen.ForEach((d) =>
                {
                    SetCellText(buildProduct, 9, index, 0, d.Name);
                    SetCellText(buildProduct, 9, index, 1, d.Len);
                    index++;
                });
                listAction.Add(delegate ()
                {
                    if (listPipingLen.Count == 1)
                    {
                        tbPipingMaterials.Rows[2].Remove();
                    }
                });
            }
            if (listAction.Count > 0)
            {
                listAction.ForEach((fn) => { fn(); });
            }
            if (!string.IsNullOrEmpty(totalAddRefrigeration))
            {
                buildProduct.MoveToBookmark("RefrigerantWeight");
                buildProduct.Write(totalAddRefrigeration);
            }
            if (!thisProject.IsOutdoorListChecked)
            {
                buildProduct.MoveToBookmark("OutdoorUnits");
                buildProduct.CurrentParagraph.Remove();
                tbOutdoors.Remove();
            }
            if (!thisProject.IsIndoorListChecked)
            {
                buildProduct.MoveToBookmark("IndoorUnits");
                buildProduct.CurrentParagraph.Remove();
                tbIndoor.Remove();
            }
            if (listAccessories.Count == 0)
            {
                buildProduct.MoveToBookmark("Accessories");
                buildProduct.CurrentParagraph.Remove();
                tbAccessories.Remove();
            }
            if (heatExchangerUnits.Count == 0 || !thisProject.IsExchangerChecked)
            {
                buildProduct.MoveToBookmark("HeatExchangerUnits");
                buildProduct.CurrentParagraph.Remove();
                tbHeatExchangerUnits.Remove();
            }
            if (exc_listAccessories.Count == 0)
            {
                buildProduct.MoveToBookmark("Exc_Accessories");
                buildProduct.CurrentParagraph.Remove();
                tbexcAccessories.Remove();
            }
            if (listControls.Count == 0 || !thisProject.IsControllerChecked)
            {
                buildProduct.MoveToBookmark("Controllers");
                buildProduct.CurrentParagraph.Remove();
                tbControllers.Remove();
            }
            if (listPipingConnectionKit.Count == 0)
            {
                buildProduct.MoveToBookmark("PipeConnectionKit");
                buildProduct.CurrentParagraph.Remove();
                tbPipeConnectionKit.Remove();
            }
            if (listBranchKit.Count == 0)
            {
                buildProduct.MoveToBookmark("Multikit");
                //buildProduct.CurrentParagraph.Remove();
                //tbMultikit.Remove();
            }
            if (listPipingConnectionKit.Count == 0 && listBranchKit.Count == 0)
            {
                buildProduct.MoveToBookmark("BranchKit");
                buildProduct.CurrentParagraph.Remove();
            }
            if (listCHBox.Count == 0)
            {
                buildProduct.MoveToBookmark("CHBox");
                buildProduct.CurrentParagraph.Remove();
                tbCHBox.Remove();
            }
            if (listPipingLen.Count == 0)
            {
                buildProduct.MoveToBookmark("PipingMaterials");
                buildProduct.CurrentParagraph.Remove();
                tbPipingMaterials.Remove();
            }
            if (IsZeropipelength == false)
            {
                buildProduct.MoveToBookmark("ZeroPipeLen");
                if (buildProduct.CurrentParagraph.ParentNode != null)
                {
                    buildProduct.CurrentParagraph.Remove();
                }
                buildProduct.MoveToBookmark("ZeroPipelength");
                if (buildProduct.CurrentParagraph.ParentNode != null)
                {
                    buildProduct.CurrentParagraph.Remove();
                }
            }
            if (string.IsNullOrEmpty(totalAddRefrigeration))
            {
                buildProduct.MoveToBookmark("Refrigerant");
                if (buildProduct.CurrentParagraph.ParentNode != null)                   
                    buildProduct.CurrentParagraph.Remove();
                tbRefrigerant.Remove();
            }
            if (listPipingLen.Count == 0 && string.IsNullOrEmpty(totalAddRefrigeration))
            {
                buildProduct.MoveToBookmark("FieldProviding");
                //buildProduct.CurrentParagraph.Remove();
            }

            if (thisProject.ControlGroupList == null || (thisProject.ControlGroupList != null && thisProject.ControlGroupList.Count == 0 || !thisProject.IsControllerChecked))
            {
                Builder.MoveToBookmark("CentralControllerImg");
                Builder.CurrentParagraph.NextSibling.Remove();
                Builder.CurrentParagraph.Remove();
            }

            Bookmark Tmark = Doc.Range.Bookmarks["ProductList"];//主文档书签
            InsertDocument(Tmark.BookmarkStart.ParentNode, docProduct);//表格复制

            Builder.MoveToBookmark("ProductList");
            Builder.CurrentParagraph.Remove();

        }


        #endregion

        #region 内部方法

        /// <summary>
        /// 根据语言获取不同的模板
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetDocPath(string path)
        {
            string docPath = path;
            string tag = "";
            if (SystemSetting.UserSetting.defaultSetting.LanguageCode == LangType.FRENCH.Substring(3))
            {
                tag = "_FR";
                docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
                return docPath;
            }
            //else if (SystemSetting.UserSetting.defaultSetting.LanguageCode == LangType.TURKISH.Substring(3))
            //{
            //    tag = "_TK";

            //}
            else if (SystemSetting.UserSetting.defaultSetting.LanguageCode == "SP")
            {
                tag = "_SP";
                docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
                return docPath;
            }
            else if (SystemSetting.UserSetting.defaultSetting.LanguageCode == LangType.GERMANY.Substring(3))
            {
                tag = "_DE";
                docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
                return docPath;
            }
            else if (SystemSetting.UserSetting.defaultSetting.LanguageCode == LangType.ITALIAN.Substring(3))
            {
                tag = "_IT";
                docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
                return docPath;
            }
            else if (SystemSetting.UserSetting.defaultSetting.LanguageCode == "PT_BR")
            {
                tag = "_PT_BR";
                docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
                return docPath;
            }
            else if (SystemSetting.UserSetting.defaultSetting.LanguageCode == "ZHT")
            {
                tag = "_ZHT";
                docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
                return docPath;
            }
            if (thisProject.SubRegionCode == "ANZ" && Path.GetFileName(path).Contains("NewReport"))
            {
                tag += "_ANZ";
            }

            docPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + tag + Path.GetExtension(path);
            return docPath;
        }

        /// <summary>
        /// 插入图片
        /// </summary>
        /// <param name="fullPath">图片路径</param>
        /// <param name="docBulider">文档</param>
        /// <param name="size">宽高限制</param>
        /// <param name="scale">宽/高 比例</param>
        private void InsertImage(string fullPath, DocumentBuilder docBulider, int size = 480, int scale = 3)
        {

            if (System.IO.File.Exists(fullPath))
            {
                using (System.Drawing.Image img = new System.Drawing.Bitmap(fullPath))
                {
                    if (img.Width > img.Height)
                    {
                        if (img.Width < size)
                        {
                            docBulider.InsertImage(img);
                        }
                        else
                        {
                            if (img.Width > img.Height * scale)
                            {
                                img.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipXY);
                                docBulider.InsertImage(img, img.Width * (size + 80) / img.Height, (size + 80));
                            }
                            else
                            {
                                docBulider.InsertImage(img, size, img.Height * size / img.Width);
                            }
                        }
                    }
                    else
                    {
                        if (img.Height < size)
                        {
                            docBulider.InsertImage(img);
                        }
                        else
                        {
                            if (img.Height > img.Width * scale)
                            {
                                img.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipXY);
                                docBulider.InsertImage(img, size, img.Height * size / img.Width);
                            }
                            else
                            {
                                docBulider.InsertImage(img, img.Width * size / img.Height, size);
                            }
                        }
                    }
                }


            }
        }

        /// <summary>
        /// 填写单元格数据
        /// </summary>
        /// <param name="build"></param>
        /// <param name="tbIndex"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="text"></param>
        private void SetCellText(DocumentBuilder build, int tbIndex, int row, int col, object text)
        {
            if (text != null)
            {
                build.MoveToCell(tbIndex, row, col, 0);
                build.Write(Convert.ToString(text));
            }
        }
        /// <summary>
        /// 填写BookMark
        /// </summary>
        /// <param name="build"></param>
        /// <param name="bookMark"></param>
        /// <param name="text"></param>
        private void SetBookMarkText(DocumentBuilder build, string bookMark, object text)
        {
            if (text != null)
            {
                build.MoveToBookmark(bookMark);
                build.Write(Convert.ToString(text));
            }
        }
        /// <summary>
        /// 获取噪声数值
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private double GetSoundPressure(RoomIndoor d)
        {
            double noiseLevel = 0;
            switch (d.FanSpeedLevel)
            {
                case -1: noiseLevel = d.IndoorItem.NoiseLevel > 0 ? d.IndoorItem.NoiseLevel : d.IndoorItem.NoiseLevel_Hi; break;
                case 0: noiseLevel = d.IndoorItem.NoiseLevel; break;
                case 1: noiseLevel = d.IndoorItem.NoiseLevel_Hi; break;
                case 2: noiseLevel = d.IndoorItem.NoiseLevel_Med; break;
                case 3: noiseLevel = d.IndoorItem.NoiseLevel_Lo; break;
                default: break;
            }
            return noiseLevel;
        }

        /// <summary>
        /// 获取噪声级别
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private string GetSoundPressureLev(int lev)
        {
            string noiseLevel = "";
            switch (lev)
            {
                case -1: noiseLevel = "Max"; break;
                case 0: noiseLevel = "High 2"; break;
                case 1: noiseLevel = "High"; break;
                case 2: noiseLevel = "Medium"; break;
                case 3: noiseLevel = "Low"; break;
                default: break;
            }
            return noiseLevel;
        }

        public string GetAirFlowTab(double[] arr_rng, string type, double airflow)
        {
            string selectedTap = string.Empty;

            if (type.Contains("Hydro Free") || type == "DX-Interface")
            {
                selectedTap = "-";
            }
            else if (arr_rng == null)
            {
                selectedTap = "";
            }
            else
            {
                if (airflow == arr_rng[0])
                    selectedTap = "High2";
                else if (airflow == arr_rng[1])
                    selectedTap = "High";
                else if (airflow == arr_rng[2])
                    selectedTap = "Medium";
                else if (airflow == arr_rng[3])
                    selectedTap = "Low";
                else
                    selectedTap = "";
            }

            return selectedTap;
        }
        /// <summary>
        /// 获取室内机类型（显示名称）
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private string GetIndoorDisplayName(RoomIndoor d)
        {
            dynamic item = d.IndoorItem;
            try
            {
                if (item.DisplayName != null && item.DisplayName != "")
                {
                    // return item.DisplayName;
                    return trans.getTypeTransStr(TransType.Indoor.ToString(), item.DisplayName);
                }
                else
                {
                    //return item.Type;
                    return trans.getTypeTransStr(TransType.Indoor.ToString(), item.Type);
                }
            }
            catch
            {
                return item.Type;
            }
        }
        /// <summary>
        /// 获取Accessory DisplayName（配件的显示名称）
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private string GetAccessoryDisplayName(Accessory a, RoomIndoor r)
        {
            string type = AccessoryDisplayType.GetAccessoryDisplayTypeReport(thisProject.SubRegionCode, thisProject.BrandCode, a, r);
            //return type;
            return trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), type);

        }

        #region 文档节点复制
        /// <summary>
        /// Inserts content of the external document after the specified node.
        /// Section breaks and section formatting of the inserted document are ignored.
        /// </summary>
        /// <param name="insertAfterNode">Node in the destination document after which the content
        /// should be inserted. This node should be a block level node (paragraph or table).</param>
        /// <param name="srcDoc">The document to insert.</param>
        static void InsertDocument(Aspose.Words.Node insertAfterNode, Document srcDoc)
        {
            // Make sure that the node is either a paragraph or table.
            if ((!insertAfterNode.NodeType.Equals(Aspose.Words.NodeType.Paragraph)) &
              (!insertAfterNode.NodeType.Equals(Aspose.Words.NodeType.Table)))
            {
                throw new ArgumentException("The destination node should be either a paragraph or table.");
            }

            // We will be inserting into the parent of the destination paragraph.
            CompositeNode dstStory = insertAfterNode.ParentNode;

            // This object will be translating styles and lists during the import.
            NodeImporter importer = new NodeImporter(srcDoc, insertAfterNode.Document, ImportFormatMode.KeepSourceFormatting);

            // Loop through all sections in the source document.
            foreach (Section srcSection in srcDoc.Sections)
            {
                // Loop through all block level nodes (paragraphs and tables) in the body of the section.
                foreach (Aspose.Words.Node srcNode in srcSection.Body)
                {
                    // Let's skip the node if it is a last empty paragraph in a section.
                    if (srcNode.NodeType.Equals(Aspose.Words.NodeType.Paragraph))
                    {
                        Paragraph para = (Paragraph)srcNode;
                        if (para.IsEndOfSection && !para.HasChildNodes)
                            continue;
                    }

                    // This creates a clone of the node, suitable for insertion into the destination document.
                    Aspose.Words.Node newNode = importer.ImportNode(srcNode, true);

                    // Insert new node after the reference node.
                    dstStory.InsertAfter(newNode, insertAfterNode);
                    insertAfterNode = newNode;
                }
            }
        }
        #endregion
        // 得到指定系统的配管图中，连接管的管径规格及长度汇总记录
        #region GetData_LinkPipeSpecG & GetData_LinkPipeSpecL
        /// <summary>
        /// 得到指定系统的连接管 管径与长度汇总数据 Update on 20120828 clh 
        /// </summary>
        /// <param name="sysName"></param>
        /// <param name="dvL"></param>
        /// <returns></returns>
        public DataView GetData_LinkPipeSpecG(ref NGModel.SystemVRF sysItem, out DataView dvL)
        {
            DataTable dtG = Util.InitDataTable(NameArray_Report.PipeSpec_Name);
            //解决DataView默认按照字符串类型排序的问题20120111-clh-
            if (!CommonBLL.IsDimension_inch()) // 若为英制，此时的管径为分数格式（7/8等），不能转为double进行排序
                dtG.Columns[RptColName_PipeSpec.PipeSpec].DataType = typeof(double);
            DataTable dtL = dtG.Clone();

            if (sysItem.MyPipingNodeOut != null)
            {
                Lassalle.WPF.Flow.Node fNode = sysItem.MyPipingNodeOut;
                GatherLinkPipeSpec(ref dtG, ref dtL, fNode, sysItem.Name);
            }

            // add on 20120925 因为英制的管径规格为分数，故不能排序
            dtG.DefaultView.Sort = RptColName_PipeSpec.PipeSpec.ToString() + " asc";
            dtL.DefaultView.Sort = dtG.DefaultView.Sort;

            // add on 20140612 clh 若当前系统为自动配管计算，则等效长度默认为“-”
            if (!sysItem.IsInputLengthManually)
            {
                for (int i = 0; i < dtG.Rows.Count; i++)
                {
                    dtG.Rows[i][Name_Common.Length] = "-";
                    dtG.Rows[i][RptColName_PipeSpec.EqLength] = "-";
                }
                for (int i = 0; i < dtL.Rows.Count; i++)
                {
                    dtL.Rows[i][Name_Common.Length] = "-";
                    dtL.Rows[i][RptColName_PipeSpec.EqLength] = "-";
                }
            }

            dvL = dtL.DefaultView;
            return dtG.DefaultView;
        }

        private void GatherLinkPipeSpec(ref DataTable dtG, ref DataTable dtL, Lassalle.WPF.Flow.Node node, string sysName)
        {
            if (node == null) return;
            try
            {
                NGPipBLL.PipingBLL pipBll = new NGPipBLL.PipingBLL(thisProject);
                if (node is NGModel.MyNodeOut)
                {
                    NGModel.MyNodeOut nodeOut = node as NGModel.MyNodeOut;
                    if (nodeOut.PipeSize != null && nodeOut.PipeLengthes != null)
                    {
                        for (int i = 0; i < nodeOut.PipeSize.Length && i < nodeOut.PipeLengthes.Length; i++)
                        {
                            string pipeSizeGroup = nodeOut.PipeSize[i];
                            string[] aa = pipeSizeGroup.Split('x');
                            double pipeLength = nodeOut.PipeLengthes[i];
                            double eqLength = pipeLength;
                            if (aa.Length == 2)
                            {
                                AddLinkPipeSpec(ref dtG, aa[0].Trim(), PipeType.Gas.ToString(), pipeLength, eqLength, sysName);
                                AddLinkPipeSpec(ref dtL, aa[1].Trim(), PipeType.Liquid.ToString(), pipeLength, eqLength, sysName);
                            }
                            else if (aa.Length == 3)
                            {
                                AddLinkPipeSpec(ref dtG, aa[0].Trim(), PipeType.Gas.ToString(), pipeLength, eqLength, sysName);
                                AddLinkPipeSpec(ref dtG, aa[1].Trim(), PipeType.Gas.ToString(), pipeLength, eqLength, sysName);
                                AddLinkPipeSpec(ref dtL, aa[2].Trim(), PipeType.Liquid.ToString(), pipeLength, eqLength, sysName);
                            }
                        }
                    }
                }
                else if (node is NGModel.MyNode)
                {
                    foreach (NGModel.MyLink link in (node as NGModel.MyNode).MyInLinks)
                    {
                        double eqLength = pipBll.GetLinkLength_EQ(link);
                        if (!string.IsNullOrEmpty(link.SpecG_h) && link.SpecG_h != "-")
                            AddLinkPipeSpec(ref dtG, link.SpecG_h, PipeType.Gas.ToString(), link.Length, eqLength, sysName);
                        if (!string.IsNullOrEmpty(link.SpecG_l) && link.SpecG_l != "-")
                            AddLinkPipeSpec(ref dtG, link.SpecG_l, PipeType.Gas.ToString(), link.Length, eqLength, sysName);
                        if (!string.IsNullOrEmpty(link.SpecL) && link.SpecL != "-")
                            AddLinkPipeSpec(ref dtL, link.SpecL, PipeType.Liquid.ToString(), link.Length, eqLength, sysName);
                    }
                }

                if (node is NGModel.MyNodeOut)
                {
                    GatherLinkPipeSpec(ref dtG, ref dtL, (node as NGModel.MyNodeOut).ChildNode, sysName);
                }
                else if (node is NGModel.MyNodeYP)
                {
                    foreach (Lassalle.WPF.Flow.Node n in (node as NGModel.MyNodeYP).ChildNodes)
                    {
                        GatherLinkPipeSpec(ref dtG, ref dtL, n, sysName);
                    }
                }
                else if (node is NGModel.MyNodeCH)
                {
                    GatherLinkPipeSpec(ref dtG, ref dtL, (node as NGModel.MyNodeCH).ChildNode, sysName);
                }
                else if (node is NGModel.MyNodeMultiCH)
                {
                    foreach (Lassalle.WPF.Flow.Node n in (node as NGModel.MyNodeMultiCH).ChildNodes)
                    {
                        GatherLinkPipeSpec(ref dtG, ref dtL, n, sysName);
                    }
                }
            }
            catch (Exception exc)
            {
                //JCMsg.ShowErrorOK("未获取到配管图节点的Link对象！\n" + exc.Message);
            }
        }

        // 辅助方法
        private void AddLinkPipeSpec(ref DataTable dt, string pipeSpec, string pipeType, double realLength, double eqLength, string sysName)
        {
            // 若表中已存在重复记录，则相应的数量加1
            foreach (DataRow r in dt.Rows)
            {
                string existSpec = r[RptColName_PipeSpec.PipeSpec].ToString();
                if (existSpec == pipeSpec && r[RptColName_PipeSpec.PipeType].ToString() == pipeType
                    && r[RptColName_PipeSpec.SysName].ToString() == sysName)
                {
                    int qty = Int32.Parse(r[Name_Common.Qty].ToString());
                    r[Name_Common.Qty] = qty + 1;

                    r[Name_Common.Length] = (double.Parse(r[Name_Common.Length].ToString()) + realLength).ToString("n1");
                    r[RptColName_PipeSpec.EqLength] = (double.Parse(r[RptColName_PipeSpec.EqLength].ToString()) + eqLength).ToString("n1");
                    return;
                }
            }
            // 若不存在重复记录，则添加新记录
            DataRow newR = dt.NewRow();
            newR[RptColName_PipeSpec.PipeSpec] = pipeSpec;
            newR[RptColName_PipeSpec.PipeType] = pipeType;
            newR[Name_Common.Qty] = 1;
            newR[Name_Common.Length] = realLength;
            newR[RptColName_PipeSpec.EqLength] = eqLength;
            newR[RptColName_PipeSpec.SysName] = sysName;
            dt.Rows.Add(newR);
        }

        #endregion

        #region 将日志记录的内容写入指定的文件
        /// <summary>
        /// 将系统日志记录的内容写入指定的文件
        /// </summary>
        /// <param name="logList">系统日志记录</param>
        /// <param name="logPath"></param>
        private void WriteToLogFile(List<string> logList, string logPath)
        {
            if (logList == null || logList.Count == 0)
                return;
            StreamWriter streamWriter = new StreamWriter(logPath);
            for (int i = 0; i <= logList.Count - 1; i++)
            {
                streamWriter.WriteLine(logList[i]);
            }
            streamWriter.Flush();
            streamWriter.Close();
        }

        /// <summary>
        /// 将 Log 记录添加到 ReportLogList 中
        /// </summary>
        /// <param name="info"></param>
        private void AddToLog(string info)
        {
            ReportLogList.Add(info + "\t时间：" + DateTime.Now);
        }

        #endregion

        #region 根据语言标记初始化翻译
        /// <summary>
        /// 根据语言标记初始化翻译
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private Trans InitTranslation()
        {
            //简体中文暂不支持
            if (LangType.CurrentLanguage == LangType.CHINESE || LangType.CurrentLanguage == LangType.CHINESE_SIMPLE)
                trans = new Trans(LangType.ENGLISH);
            else
                trans = new Trans();

            return trans;
        }
        #endregion
        //File in use for Report

        static bool FileInUse(string path)
        {
            bool IsOpen = false;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    IsOpen = fs.CanWrite;
                }
                return IsOpen;
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        #endregion
    }
}
