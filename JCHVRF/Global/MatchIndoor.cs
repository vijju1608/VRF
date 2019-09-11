using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF.Model;
using System.Data;
using JCBase.UI;
using System.Collections;
using JCHVRF.BLL;
using JCHVRF.VRFMessage;

namespace JCHVRF
{
    public class MatchIndoor
    {
        #region 室内机批量重新选型 add by axj 20170117

        double est_cool = 0;
        double est_heat = 0;
        double est_sh = 0;
        double airflow = 0;
        Project thisProject = null;
        bool IsCoolingMode = false;
        bool IsHeatingMode = false;
        string BrandCode = "";
        double rq_cool = 0;
        double rq_airflow = 0;
        double rq_heat = 0;
        double rq_sensiable = 0;
        double rq_fa = 0;
        string RegCode = "";
        double rq_wb_c = 0;
        double rq_db_h = 0;
        double rq_wb_h = 0;
        double rq_db_c = 0;
        string _productType = "";
        string _series = "";
        //string _shf_mode = "";
        int _fanSpeedLevel = -1; //风扇速度等级 -1:Max, 0:High2, 1:High, 2:Med, 3:Low add on 20170703 by Shen Junjie
        string _unitType = "";
        IndoorBLL bll = null;

        /// <summary>
        /// 重新选型室内机
        /// </summary>
        /// <param name="roomIndoorList"></param>
        /// <param name="Project"></param>
        /// <param name="outDBCool"></param>
        /// <param name="outWBHeat"></param>
        /// <param name="stateList"></param>
        /// <returns></returns>
        public List<RoomIndoor> DoRoomIndoorReselection(List<RoomIndoor> roomIndoorList, Project Project, double outDBCool, double outWBHeat, out List<string> stateList)
        {
            List<RoomIndoor> newRoomindoorList = new List<RoomIndoor>();
            stateList = new List<string>();
            thisProject = Project;
            BrandCode = thisProject.BrandCode;
            IsCoolingMode = thisProject.IsCoolingModeEffective;
            IsHeatingMode = thisProject.IsHeatingModeEffective;
            var floorlist = thisProject.FloorList;
            bll = new IndoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
            //var dicSeries = CommonOperation.GetProductSeriesKeyValue();
            //List<RoomIndoor> list = new List<RoomIndoor>();
            //var unitlist = CommonOperation.GetUnitTypeList();
            for (int i = 0; i < roomIndoorList.Count; i++)
            {
                var roomIndoor = roomIndoorList[i];
                if (roomIndoor.IsFreshAirArea == true)
                {
                    newRoomindoorList.Add(roomIndoor);
                    //stateList.Add(roomIndoor.IndoorNO + ":Indoor_NoChange");
                    stateList.Add(roomIndoor.IndoorNO + "::Indoor_NoChange"); // 解决纯新风机 选型更改温度，选型报错 on 20180903 by xyj  
                    continue;
                }
                if (roomIndoor.IsAuto == true)
                {
                    //重新选型并计算估算容量
                    string region = roomIndoor.IndoorItem.RegionCode;
                    string unitType = roomIndoor.IndoorItem.Type;
                    string productType = roomIndoor.IndoorItem.ProductType;
                    double CoolCapacity = roomIndoor.RqCoolingCapacity;
                    double CoolSensible = roomIndoor.RqSensibleHeat;
                    double CoolAirFlow = roomIndoor.RqAirflow;
                    double HeatCapacity = roomIndoor.RqHeatingCapacity;
                    double FreshAir = roomIndoor.RqFreshAir;
                    double indoorCoolingWB = roomIndoor.WBCooling;
                    double indoorHeatingDB = roomIndoor.DBHeating;
                    string Series = roomIndoor.IndoorItem.Series;
                    
                    if (!string.IsNullOrEmpty(roomIndoor.RoomID))
                    {
                        CoolCapacity = 0;
                        CoolSensible = 0;
                        CoolAirFlow = 0;
                        HeatCapacity = 0;
                        FreshAir = 0;
                        for (int j = 0; j < floorlist.Count; j++)
                        {
                            var roomlist = floorlist[j].RoomList;
                            var room = roomlist.Find(p => p.Id == roomIndoor.RoomID);
                            if (room != null)
                            {
                                CoolCapacity = room.RqCapacityCool;
                                HeatCapacity = room.RqCapacityHeat;
                                CoolAirFlow = room.AirFlow;
                                CoolSensible = room.SensibleHeat;
                                FreshAir = room.FreshAir;
                                break;
                            }
                        }
                    }

                    rq_cool = CoolCapacity;
                    rq_airflow = CoolAirFlow;
                    rq_heat = HeatCapacity;
                    rq_sensiable = CoolSensible;
                    rq_fa = FreshAir;
                    RegCode = region;
                    rq_wb_c = indoorCoolingWB;
                    rq_db_h = indoorHeatingDB;
                    rq_wb_h = outWBHeat;
                    rq_db_c = outDBCool;
                    _productType = roomIndoor.IndoorItem.ProductType;
                    _series = roomIndoor.IndoorItem.Series;
                    //_shf_mode = roomIndoor.SHF_Mode;
                    _fanSpeedLevel = roomIndoor.FanSpeedLevel;
                    _unitType = roomIndoor.IndoorItem.Type;
                    //var redata = CommonOperation.GetIndoorOrOutdoorList(dic);
                    //需要将type进行处理，获取厂名 20161118 by Yunxiao Lin
                    string type = roomIndoor.IndoorItem.Type;
                    int m = type.IndexOf("-");
                    string factoryName = "";
                    if (m > 0)
                    {
                        if (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_S" && thisProject.RegionCode != "EU_E")//EU区域的 暂时不取厂名 20180302 by xyj
                        {
                            factoryName = type.Substring(m + 1, type.Length - m - 1);
                            type = type.Substring(0, m);
                        }
                    }
                    DataTable dtIndoorList = bll.GetIndoorListStd(type, roomIndoor.IndoorItem.ProductType, factoryName);
                    dtIndoorList.DefaultView.Sort = "CoolCapacity";
                    //List<IndoorTable> listIndoor = Common.Constant.JSON2Object<List<IndoorTable>>(redata["Data"]);
                    if (dtIndoorList.Rows.Count == 0)
                    {
                        stateList.Add(roomIndoor.IndoorNO + "::IND_NOTMATCH");
                        continue;
                    }
                    DataRow drMax = dtIndoorList.Rows[dtIndoorList.Rows.Count - 1];
                    if (IsCoolingMode && Convert.ToDecimal(drMax["CoolCapacity"]) < Convert.ToDecimal(CoolCapacity))
                    {
                        stateList.Add(roomIndoor.IndoorNO + "::IND_NOTMATCH");
                        continue;
                    }
                    if (IsHeatingMode && !roomIndoor.IndoorItem.ProductType.Contains(", CO") && Convert.ToDecimal(drMax["HeatCapacity"]) < Convert.ToDecimal(HeatCapacity))
                    {
                        stateList.Add(roomIndoor.IndoorNO + "::IND_NOTMATCH");
                        continue;
                    }
                    RoomIndoor newRoomIndoor = null;
                    for (int j = 0; j < dtIndoorList.Rows.Count; j++)
                    {
                        bool isPass = true;
                        DataRow dr = dtIndoorList.Rows[j];
                        if (IsCoolingMode && Convert.ToDecimal(dr["CoolCapacity"]) < Convert.ToDecimal(CoolCapacity) || Convert.ToDecimal(dr["SensibleHeat"]) < Convert.ToDecimal(CoolSensible) || Convert.ToDecimal(dr["AirFlow"]) < Convert.ToDecimal(CoolAirFlow))
                            isPass = false;

                        if (IsHeatingMode & !roomIndoor.IndoorItem.ProductType.Contains(", CO") && Convert.ToDecimal(dr["HeatCapacity"]) < Convert.ToDecimal(HeatCapacity))
                            isPass = false;
                        //ent.indoor = listIndoor[j];

                        if (!isPass || !autoCompare(_unitType, dr))
                        {
                            continue;
                        }

                        if (dr["ModelFull"].ToString() == roomIndoor.IndoorItem.ModelFull)
                        {
                            newRoomIndoor = roomIndoor;
                            stateList.Add(roomIndoor.IndoorNO + "::Indoor_NoChange");

                        }
                        else
                        {
                            //室内机型号变更，去除所有共享绑定关系 add by axj 20180503
                            bool isBinding = false;
                            thisProject.RoomIndoorList.ForEach((ind) =>
                            {
                                if (ind.IndoorNO != roomIndoor.IndoorNO)
                                {
                                    var grp = ind.IndoorItemGroup;
                                    if (grp != null)
                                    {
                                        var item = grp.Find((p) => p.IndoorNO == roomIndoor.IndoorNO);
                                        if (item != null)
                                        {
                                            isBinding = true;
                                            grp.Remove(item);
                                        }
                                    }
                                }
                            });
                            newRoomIndoor = GetNewRoomIndoor(dr, roomIndoor);
                            string sUnbinding = "";
                            if (roomIndoor.IsMainIndoor)
                            {
                                sUnbinding = "Indoor_Unbinding";
                            }
                            else
                            {
                                if (isBinding)
                                {
                                    sUnbinding = "Indoor_ResetAccessories";
                                }
                            }
                            stateList.Add(roomIndoor.IndoorNO + "::Indoor_ChangeModel::" + sUnbinding);
                        }

                        newRoomIndoor.IndoorNO = roomIndoor.IndoorNO;
                        newRoomIndoor.SystemID = roomIndoor.SystemID;
                        newRoomIndoor.IndoorName = roomIndoor.IndoorName;
                        newRoomIndoor.CoolingCapacity = est_cool;
                        newRoomIndoor.HeatingCapacity = est_heat;
                        newRoomIndoor.SensibleHeat = est_sh;
                        //newRoomIndoor.AirFlow = airflow;
                        newRoomIndoor.DBCooling = roomIndoor.DBCooling;
                        newRoomIndoor.DBHeating = roomIndoor.DBHeating;
                        newRoomIndoor.WBCooling = roomIndoor.WBCooling;
                        newRoomIndoor.RqCoolingCapacity = rq_cool;
                        newRoomIndoor.RqHeatingCapacity = rq_heat;
                        newRoomIndoor.RqSensibleHeat = rq_sensiable;
                        newRoomIndoor.RqAirflow = rq_airflow;
                        newRoomIndoor.RqFreshAir = rq_fa;
                        
                        if (!string.IsNullOrEmpty(roomIndoor.RoomID))
                        {
                            newRoomIndoor.RoomID = roomIndoor.RoomID;
                        }
                        newRoomindoorList.Add(newRoomIndoor);

                        break;
                    }
                    if (newRoomIndoor == null)
                    {
                        stateList.Add(roomIndoor.IndoorNO + "::IND_NOTMATCH");
                        newRoomindoorList.Add(roomIndoor);
                    }
                }
                else
                {
                    string type = roomIndoor.IndoorItem.Type;
                    int m = type.IndexOf("-");
                    string factoryName = ""; 
                    if (m > 0)
                    {
                        if (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_S" && thisProject.RegionCode != "EU_E") //EU区域的 暂时不取厂名 20180302 by xyj
                        {
                            factoryName = type.Substring(m + 1, type.Length - m - 1);
                            type = type.Substring(0, m);
                        }
                    }
                    DataTable dtIndoorList = bll.GetIndoorListStd(type, roomIndoor.IndoorItem.ProductType, factoryName);
                    dtIndoorList.DefaultView.Sort = "CoolCapacity";
                    DataRow stdRow = null;
                    foreach (DataRow dr in dtIndoorList.Rows)
                    {
                        if (roomIndoor.IndoorItem.ModelFull == dr["ModelFull"].ToString())
                        {
                            stdRow = dr;
                            break;
                        }
                    }
                    //重新计算估算容量
                    rq_wb_c = roomIndoor.WBCooling;
                    rq_db_h = roomIndoor.DBHeating;
                    rq_wb_h = outWBHeat;
                    rq_db_c = outDBCool;
                    RegCode = roomIndoor.IndoorItem.RegionCode;
                    _productType = roomIndoor.IndoorItem.ProductType;
                    _series = roomIndoor.IndoorItem.Series;
                    //_shf_mode = roomIndoor.SHF_Mode;
                    _fanSpeedLevel = roomIndoor.FanSpeedLevel;
                    _unitType = roomIndoor.IndoorItem.Type;
                    DoCalculateEstValue(stdRow);

                    if (est_cool <= 0 || est_heat <= 0)
                    {
                        bool isContinue = true;
                        //过滤 _productType 为,CO 类型的数据 不需要制热 on 20181016 by xyj
                        if (!string.IsNullOrEmpty(_productType)) 
                        {
                            if (_productType.Contains(", CO")&& est_heat==0)
                            {
                                isContinue = false;
                            }
                        }
                        if (isContinue)
                        {
                            stateList.Add(roomIndoor.IndoorNO + "::DATA_EXCEED");
                            newRoomindoorList.Add(roomIndoor);
                            continue;
                        }
                    }
                    roomIndoor.CoolingCapacity = est_cool;
                    roomIndoor.HeatingCapacity = est_heat;
                    roomIndoor.SensibleHeat = est_sh;
                    //roomIndoor.AirFlow = airflow;
                    newRoomindoorList.Add(roomIndoor);
                    stateList.Add(roomIndoor.IndoorNO + "::Indoor_NoChange");
                }
            }
            //重新计算房间需求是否满足
            var rmlist = newRoomindoorList.FindAll(p => !string.IsNullOrEmpty(p.RoomID));
            Hashtable htChk = new Hashtable();
            for (int i = 0; i < rmlist.Count; i++)
            {
                string roomId = rmlist[i].RoomID;
                if (htChk[roomId] != null)
                {
                    continue;
                }
                else
                {
                    htChk[roomId] = "";
                }
                bool isFreshAir = rmlist[i].IsFreshAirArea;
                var tplist = newRoomindoorList.FindAll(p => p.RoomID == roomId);
                Room curRoom = null;
                //获取房间容量需求
                for (int j = 0; j < floorlist.Count; j++)
                {
                    var roomlist = floorlist[j].RoomList;
                    var room = roomlist.Find(p => p.Id == roomId);
                    if (room != null)
                    {
                        curRoom = room;
                        break;
                    }
                }
                if (tplist.Count > 0)
                {

                    double total_cool_Capacity = 0;
                    double total_heat_Capacity = 0;
                    double total_sensible_Heat = 0;
                    double total_air_Flow = 0;
                    double total_fresh_Air = 0;
                    DoCalculateSelectedSumCapacity(tplist, isFreshAir, ref total_cool_Capacity, ref total_heat_Capacity, ref total_sensible_Heat, ref total_air_Flow, ref total_fresh_Air);
                    if (curRoom != null)
                    {
                        if (!isFreshAir)
                        {
                            if (IsCoolingMode)
                            {
                                if (total_cool_Capacity < curRoom.RqCapacityCool || total_sensible_Heat < curRoom.SensibleHeat || total_air_Flow < curRoom.AirFlow)
                                {
                                    if (total_cool_Capacity <= curRoom.RqCapacityCool)
                                    {
                                        foreach (var ent in tplist)
                                        {
                                            stateList.Add(ent.IndoorNO + "::IND_NOTMEET_COOLING");
                                        }
                                    }

                                }
                            }
                            if (IsHeatingMode && !_productType.Contains(", CO"))//tplist[0].IndoorItem.ProductType.Contains(", CO")
                            {
                                if (total_heat_Capacity < curRoom.RqCapacityHeat)
                                {
                                    if (total_heat_Capacity <= curRoom.RqCapacityHeat)
                                    {
                                        foreach (var ent in tplist)
                                        {
                                            stateList.Add(ent.IndoorNO + "::IND_NOTMEET_HEATING");
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (total_fresh_Air < curRoom.FreshAir)
                            {
                                foreach (var ent in tplist)
                                {
                                    stateList.Add(ent.IndoorNO + "::IND_NOTMEET_FA");
                                }
                            }
                        }
                    }

                }
            }
            return newRoomindoorList;
        }



        /// <summary>
        /// 比较指定行的室内机容量与需求容量，满足则返回true
        /// </summary>
        /// <param name="type"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private bool autoCompare(string type, DataRow r)
        {
            bool pass = true;
            bool isFreshAir = type.Contains("YDCF");
            // 计算估算容量
            DoCalculateEstValue(r);

            //将估算容量与当前需求进行比较
            if (isFreshAir)
            {
                if (airflow < rq_fa)
                    pass = false;
            }
            else
            {
                if (thisProject.IsCoolingModeEffective)
                {
                    if (est_cool < rq_cool || est_sh < rq_sensiable || airflow < rq_airflow)
                        pass = false;
                }
                //if (thisProject.IsHeatingModeEffective && !thisProject.ProductType.Contains(", CO"))
                if (thisProject.IsHeatingModeEffective && !_productType.Contains(", CO"))
                {
                    if (est_heat < rq_heat)
                        pass = false;
                }
            }

            return pass;
        }

        // 将选择的室内机预添加到已选室内机
        /// <summary>
        /// 将选择的室内机预添加到已选室内机，并执行容量估算
        /// </summary>
        /// <param name="stdRow">标准表记录行</param>
        /// <param name="isAppend">是否附加到已选记录行，true则附加且count值可修改；false则添加唯一记录且count值不可修改</param>
        private void DoCalculateEstValue(DataRow stdRow)
        {
            string type = _unitType; //this.jccmbType.Text.Trim();
            //由于untTypeList中的值含有厂名，所以在取出来的时候需要去掉 20161118 by Yunxiao Lin
            int i = type.IndexOf("-"); 
            if (i > 0)
            {
                if (thisProject.RegionCode != "EU_W" && thisProject.RegionCode != "EU_S" && thisProject.RegionCode != "EU_E") //EU区域的 暂时不取厂名 20180302 by xyj
                {
                    type = type.Substring(0, i);
                }
            } 
            string modelFull = stdRow["ModelFull"].ToString();
            Indoor inItem = bll.GetItem(modelFull, type, _productType, _series);
            if (inItem != null)
                inItem.Series = _series;   //将当前的series封装室内机列表   add on 20161027

            if (type.Contains("YDCF"))
            {
                est_cool = Convert.ToDouble(stdRow["CoolCapacity"].ToString());
                est_heat = Convert.ToDouble(stdRow["HeatCapacity"].ToString());
                est_sh = Convert.ToDouble(stdRow["SensibleHeat"].ToString());
            }
            else
            {
                // 执行容量估算
                double wb_c = rq_wb_c;
                double db_c = rq_db_c;
                double db_h = rq_db_h;
                double wb_h = rq_wb_h;
                //double est_sh_h = 0;

                //est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, out est_sh, false);
                //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
                est_cool = bll.CalIndoorEstCapacity(inItem, db_c, wb_c, false);
                if (!ValidateEstCapacity(est_cool, inItem.PartLoadTableName))
                    return;
                //显热由估算容量乘以SHF系数得到 20161112 by Yunxiao Lin
                //double shf = 0d;
                //if (_shf_mode == "High")
                //    shf = inItem.SHF_Hi;
                //else if (_shf_mode == "Medium")
                //    shf = inItem.SHF_Med;
                //else
                //    shf = inItem.SHF_Lo;
                //if (shf == 0d)
                //    shf = inItem.SHF_Hi;
                double shf = inItem.GetSHF(_fanSpeedLevel);
                est_sh = est_cool * shf;

                if (!inItem.ProductType.Contains(", CO"))
                {
                    //估算室内机容量不再返回显热 20161111 by Yunxiao Lin
                    //est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, out est_sh_h, true);
                    est_heat = bll.CalIndoorEstCapacity(inItem, wb_h, db_h, true);
                    if (!ValidateEstCapacity(est_heat, inItem.PartLoadTableName))
                        return;
                }
            }
            airflow = Convert.ToDouble(stdRow["AirFlow"].ToString());

        }
        /// <summary>
        /// 验证估算容量
        /// </summary>
        /// <param name="est"></param>
        /// <param name="partLoadTableName"></param>
        /// <returns></returns>
        private bool ValidateEstCapacity(double est, string partLoadTableName)
        {
            bool res = false;
            if (est == 0)
            {
                //JCMsg.ShowWarningOK(Msg.DB_NOTABLE_CAP + "[" + partLoadTableName + "]\nRegion:" + thisProject.SubRegionCode + ";ProductType:" + _productType + "");
            }
            else if (est == -1)
            {
                //JCMsg.ShowWarningOK(Msg.WARNING_DATA_EXCEED);
            }
            else
            {
                res = true;
            }
            return res;
        }
        /// <summary>
        /// 统计计算室内机Capacity
        /// </summary>
        /// <param name="selectIndoor"></param>
        /// <param name="isFreshAir"></param>
        /// <param name="total_cool_Capacity"></param>
        /// <param name="total_heat_Capacity"></param>
        /// <param name="total_sensible_Heat"></param>
        /// <param name="total_air_Flow"></param>
        /// <param name="total_fresh_Air"></param>
        private void DoCalculateSelectedSumCapacity(List<RoomIndoor> selectIndoor, bool isFreshAir, ref double total_cool_Capacity, ref double total_heat_Capacity, ref double total_sensible_Heat, ref double total_air_Flow, ref double total_fresh_Air)
        {
            double cool_Capacity = 0;
            double heat_Capacity = 0;
            double sensible_Heat = 0;
            double air_Flow = 0;
            for (int i = 0; i < selectIndoor.Count; i++)
            {
                //  int count = Convert.ToInt32(selectIndoor[i].Count);
                int count = 1;
                cool_Capacity = Convert.ToDouble(selectIndoor[i].CoolingCapacity);
                heat_Capacity = Convert.ToDouble(selectIndoor[i].HeatingCapacity);
                sensible_Heat = Convert.ToDouble(selectIndoor[i].SensibleHeat);
                air_Flow = Convert.ToDouble(selectIndoor[i].AirFlow);
                total_cool_Capacity += cool_Capacity * count;
                total_heat_Capacity += heat_Capacity * count;
                total_sensible_Heat += sensible_Heat * count;
                if (isFreshAir)  // 新风机计算新风风量合计
                    total_fresh_Air += air_Flow * count;
                else
                    total_air_Flow += air_Flow * count;

            }
        }

        private RoomIndoor GetNewRoomIndoor(DataRow stdRow, RoomIndoor ri)
        {
            RoomIndoor door = new RoomIndoor();
            Indoor inItem = bll.GetItem(stdRow["ModelFull"].ToString(), _unitType, _productType, _series); // 新室内机对象
            door.SetIndoorItemWithAccessory(inItem);
            door.RoomID = "";
            door.RoomName = "";
            door.IsFreshAirArea = ri.IsFreshAirArea;
            door.CoolingCapacity = Convert.ToDouble(inItem.CoolingCapacity);
            door.SensibleHeat = Convert.ToDouble(inItem.SensibleHeat);
            door.RqAirflow = door.AirFlow;//Convert.ToDouble(inItem.AirFlow);
            door.HeatingCapacity = Convert.ToDouble(inItem.HeatingCapacity);
            //door.AirFlow = Convert.ToDouble(inItem.AirFlow);
            door.IndoorItem.Series = ri.IndoorItem.Series;
            door.IsDelete = false;

            return door;
        }


        #endregion
    }
    /// <summary>
    /// 重新选型结果实体类
    /// </summary>
    public class IndoorReselectionResult
    {
        /// <summary>
        /// 室内机No
        /// </summary>
        public string IndoorNo { get; set; }
        /// <summary>
        /// 室内机名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 结果提示
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 状态 选型成功:true 选型失败:false 
        /// </summary>
        public bool Seccessful { get; set; }
    }
}
