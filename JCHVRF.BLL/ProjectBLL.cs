using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Collections;
using System.Linq;

using JCHVRF.Model;
using JCHVRF.DAL;
using JCBase.Utility;

namespace JCHVRF.BLL
{
    public class ProjectBLL
    {

        Project _thisProject;
        ProjectDAL _dal;

        public ProjectBLL(Project thisProj)
        {
            _thisProject = thisProj;
            _dal = new ProjectDAL();
        }

        //public DataTable GetProductTypeList()
        //{
        //    return _dal.GetProductTypeList(_thisProject.RegionVRF);
        //}

        // 利用反射机制复制对象
        /// <summary>
        /// 利用反射机制复制对象，避免因对象属性增减造成的错误
        /// </summary>
        /// <param name="objFrom"></param>
        /// <param name="objTo"></param>
        public void CopyObject(object objFrom, object objTo)
        {
            FieldInfo[] fieldFroms = objFrom.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo[] fieldTos = objTo.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            int lenTo = fieldTos.Length;
            for (int i = 0, l = fieldFroms.Length; i < l; ++i)
            {
                for (int j = 0; j < lenTo; ++j)
                {
                    if (fieldTos[j].Name != fieldFroms[i].Name) continue;
                    fieldTos[j].SetValue(objTo, fieldFroms[i].GetValue(objFrom));
                }
            }
        }

        /// <summary>
        /// 是否支持Universal的选型方式
        /// </summary>
        /// <returns></returns>
        public static bool IsSupportedUniversalSelection(Project proj)
        {
            //欧洲和中国台湾地区支持Universal的选型方式
            return proj.RegionCode==("EU_E") || proj.RegionCode == ("EU_S") || proj.RegionCode == ("EU_W") || proj.SubRegionCode=="TW";
        }

        /// <summary>
        /// 是否支持新报告模板
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        public static bool IsSupportedNewReport(Project proj)
        {
            return proj.RegionCode=="EU_W" || proj.RegionCode == "EU_E" || proj.RegionCode == "EU_S" || proj.SubRegionCode == "ANZ" || proj.SubRegionCode == "TW" || proj.RegionCode.StartsWith("ME_A") || proj.RegionCode.StartsWith("LA") || proj.RegionCode.StartsWith("ASEAN") || proj.RegionCode.StartsWith("INDIA");
        }

        /// <summary>
        /// 是否使用EU的新的Project Info
        /// </summary>
        /// <param name="regionCode"></param>
        /// <returns></returns>
        public static bool IsUsingEuropeProjectInfo(string regionCode)
        {
            return regionCode == ("EU_E") || regionCode == ("EU_S") || regionCode == ("EU_W");
        }

        /// <summary>
        /// 是否室内机的PowerInput的数据是Kw为单位
        /// </summary>
        /// <param name="regionCode"></param>
        /// <returns></returns>
        public static bool IsIDUPowerInputKw(string regionCode)
        {
            //目前只有EU是Kw为单位，其它区域是W为单位  add by Shen Junjie on 2018/08/15
            return regionCode == ("EU_E") || regionCode == ("EU_S") || regionCode == ("EU_W");
        }

        /// 添加 Floor 节点
        /// <summary>
        /// 添加 Floor 节点
        /// </summary>
        public void AddFloor(Floor item)
        {
            item.Id = Guid.NewGuid().ToString("N");
            _thisProject.FloorList.Add(item);
        }

        /// 拷贝添加 Floor 节点
        /// <summary>
        /// 拷贝添加 Floor 节点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Floor CopyAddFloor(Floor item)
        {
            int no = GetNextFloorNO();
            Floor newItem = new Floor(no);
            CopyObject(item, newItem);
            newItem.Id = Guid.NewGuid().ToString("N");
            newItem.NO = no;

            foreach (Room r in item.RoomList)
            {
                CopyAddRoom(r, newItem);
            }
            if (_thisProject.FloorList != null)
            {
                _thisProject.FloorList.Add(newItem);
                return newItem;
            }
            return null;
        }

        /// 删除指定的 Floor 对象，连同其下的房间一起删除
        /// <summary>
        /// 删除指定的 Floor 对象，连同其下的房间一起删除
        /// </summary>
        /// <returns></returns>
        public bool DeleteFloor(string id)
        {
            if (_thisProject.FloorList != null)
            {
                foreach (Floor f in _thisProject.FloorList)
                {
                    if (f.Id == id)
                    {
                        _thisProject.FloorList.Remove(f);
                        return true;
                    }
                }
            }
            return false;
        }

        /// 计算下一个 Floor NO.
        /// <summary>
        /// 计算下一个 Floor NO.
        /// </summary>
        /// <returns></returns>
        public int GetNextFloorNO()
        {
            int max = 0;
            foreach (Floor f in _thisProject.FloorList)
            {
                max = max > f.NO ? max : f.NO;
            }
            return max + 1;
        }


        /// <summary>
        /// 获取Floor NO 最大NO
        /// </summary>
        /// <returns></returns>
        public int GetMaxFloorNO()
        {
            int max = 0;
            foreach (Floor f in _thisProject.FloorList)
            {
                max = max > f.NO ? max : f.NO;
            }
            return max;
        }



        /// <summary>
        /// 判断是否是空的房间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool isEmptyRoom(string id)
        {
            bool isEmpty = false;
            Room ri = GetRoom(id);
            if (ri != null)
            {
                if (ri.LoadIndexCool == 0 && ri.LoadIndexHeat == 0 && ri.SensibleHeat == 0 && ri.StaticPressure == 0 && ri.RqCapacityCool == 0 && ri.RqCapacityHeat == 0 && ri.IsSensibleHeatEnable == false && ri.IsStaticPressureEnable == false && ri.FreshAirIndex==0 && ri.FreshAir==0)
                {
                    isEmpty = true;
                }
            }
            return isEmpty;
        }

        /// <summary>
        /// 获取楼层名称：房间名称
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public string GetFloorAndRoom(string roomId)
        {
            string room = "";
            if (!string.IsNullOrEmpty(roomId))
            {
                foreach (Floor f in _thisProject.FloorList)
                {
                    foreach (Room ri in f.RoomList)
                    {
                        if (ri.Id == roomId)
                        {
                            return f.Name + ":" + ri.Name;
                        }
                    }
                }
                foreach (FreshAirArea f in _thisProject.FreshAirAreaList)
                {
                    if (f.Id == roomId)
                    {
                        return f.Name;
                    }
                }
            }
            return room;
        }


        /// 获取当前项目中指定的 Room 对象
        /// <summary>
        /// 获取当前项目中指定的 Room 对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Room GetRoom(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (Floor f in _thisProject.FloorList)
            {
                foreach (Room r in f.RoomList)
                {
                    if (r.Id == id)
                        return r;
                }
            }
            return null;
        }


        /// 添加 Room 对象
        /// <summary>
        /// 添加 Room 对象
        /// </summary>
        /// <param name="room"></param>
        public void AddRoom(Room item, Floor f)
        {
            item.Id = Guid.NewGuid().ToString("N");
            f.RoomList.Add(item);
        }

        /// 设置房间的所属新风区域
        /// <summary>
        /// 设置房间的所属新风区域
        /// </summary>
        /// <param name="room"></param>
        public void SetFreshAirAreaForRoom(Room item, string areaId)
        {
            item.FreshAirAreaId = areaId;
        }

        /// 移除 Room 对象
        /// <summary>
        /// 移除 Room 对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteRoom(string id, Floor f)
        {
            if (f.RoomList != null)
            {
                foreach (Room r in f.RoomList)
                {
                    if (r.Id == id)
                    {
                        f.RoomList.Remove(r);
                        return true;
                    }
                }
            }
            return false;
        }


        /// 拷贝添加 Room 节点
        /// <summary>
        /// 拷贝添加 Room 节点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Room CopyAddRoom(Room item, Floor f)
        {
            int no = GetNextRoomNO(f);
            Room newItem = new Room(no);

            CopyObject(item, newItem);
            newItem.Id = Guid.NewGuid().ToString("N");
            newItem.NO = no;

            if (f.RoomList != null)
            {
                f.RoomList.Add(newItem);
                return newItem;
            }
            return null;
        }

        /// 计算下一个 Room NO.
        /// <summary>
        /// 计算下一个 Room NO.
        /// </summary>
        /// <returns></returns>
        public int GetNextRoomNO(Floor f)
        {
            int max = 0;
            foreach (Room r in f.RoomList)
            {
                max = max > r.NO ? max : r.NO;
            }
            return max + 1;
        }

        /// 将指定的 RoomNo 转为类似 1001、1002 格式的字符串
        /// <summary>
        /// 将指定的 RoomNo 转为类似 1001、1002 格式的字符串
        /// </summary>
        /// <param name="roomNO"></param>
        /// <returns></returns>
        public string GetRoomNOString(int roomNO, Floor f)
        {
            string rNo = Util.ToFixedDigit(3, roomNO);
            return f.NO.ToString() + rNo;
        }

        /// 根据房间的id得到其房间编号,1001,1002...
        /// <summary>
        /// 根据房间的id得到其房间编号,1001,1002...
        /// </summary>
        /// <param name="rID"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public string GetRoomNOString(string rID)
        {
            if (string.IsNullOrEmpty(rID))
                return "";

            foreach (Floor f in _thisProject.FloorList)
            {
                foreach (Room r in f.RoomList)
                {
                    if (r.Id == rID)
                    {
                        return GetRoomNOString(r.NO, f);
                    }
                }
            }
            return "";
        }

        /// 拷贝添加 新风区域 节点
        /// <summary>
        /// 拷贝添加 新风区域 节点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public FreshAirArea CopyAddFreshAirArea(FreshAirArea item, out List<Floor> copyedFloors)
        {
            int no = GetNextFreshAirAreaNO();
            FreshAirArea newArea = new FreshAirArea(no);
            CopyObject(item, newArea);
            newArea.Id = Guid.NewGuid().ToString("N");
            newArea.NO = no;

            List<Floor> freshAirFloorList = _thisProject.FloorList.FindAll(f => f.RoomList.Exists(r => r.FreshAirAreaId == item.Id));
            copyedFloors = new List<Floor>();

            foreach (Floor f in freshAirFloorList)
            {
                Floor newFloor = null;
                foreach (Room r in f.RoomList)
                {
                    if (r.FreshAirAreaId != item.Id) continue;

                    if (newFloor == null)
                    {
                        int floorNo = GetNextFloorNO();
                        newFloor = new Floor(floorNo);
                        CopyObject(f, newFloor);
                        newFloor.Id = Guid.NewGuid().ToString("N");
                        newFloor.NO = floorNo;
                        if (_thisProject.FloorList != null)
                        {
                            _thisProject.FloorList.Add(newFloor);
                        }
                        copyedFloors.Add(newFloor);
                    }

                    Room newRoom = CopyAddRoom(r, newFloor);
                    newRoom.FreshAirAreaId = newArea.Id;
                }
            }
            if (_thisProject.FreshAirAreaList != null)
            {
                _thisProject.FreshAirAreaList.Add(newArea);
                return newArea;
            }
            return null;
        }

        /// 获取当前项目中指定的 FreshAirArea 对象
        /// <summary>
        /// 获取当前项目中指定的 FreshAirArea 对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FreshAirArea GetFreshAirArea(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (FreshAirArea f in _thisProject.FreshAirAreaList)
            {
                if (f.Id == id)
                    return f;
            }
            return null;
        }

        /// 添加 新风区域 节点
        /// <summary>
        /// 添加 新风区域 节点
        /// </summary>
        public void AddFreshAirArea(FreshAirArea item)
        {
            item.Id = Guid.NewGuid().ToString("N");
            _thisProject.FreshAirAreaList.Add(item);
        }

        /// 删除指定的 新风区域 对象，不会删除其下的房间
        /// <summary>
        /// 删除指定的 新风区域 对象，不会删除其下的房间
        /// </summary>
        /// <returns></returns>
        public bool DeleteFreshAirArea(string id)
        {
            if (_thisProject.FreshAirAreaList != null)
            {
                foreach (FreshAirArea area in _thisProject.FreshAirAreaList)
                {
                    if (area.Id == id)
                    {
                        foreach (Floor f in _thisProject.FloorList)
                        {
                            foreach (Room r in f.RoomList)
                            {
                                if (r.FreshAirAreaId == id)
                                {
                                    r.FreshAirAreaId = null;
                                }
                            }
                        }
                        _thisProject.FreshAirAreaList.Remove(area);
                        return true;
                    }
                }
            }
            return false;
        }

        /// 计算下一个 FreshAirArea NO.
        /// <summary>
        /// 计算下一个 FreshAirArea NO.
        /// </summary>
        /// <returns></returns>
        public int GetNextFreshAirAreaNO()
        {
            int max = 0;
            foreach (FreshAirArea f in _thisProject.FreshAirAreaList)
            {
                max = max > f.NO ? max : f.NO;
            }
            return max + 1;
        }
        /// 获取 FreshAirArea 内的人数 add on 20160728 by Yunxiao Lin
        /// <summary>
        /// 获取 FreshAirArea 内的人数
        /// </summary>
        /// <returns></returns>
        public int GetPeopleNoInFreshAirArea(FreshAirArea area)
        {
            int pn = 0;
            foreach (Floor f in _thisProject.FloorList)
            {
                foreach (Room r in f.RoomList)
                {
                    if (r.FreshAirAreaId == area.Id)
                    {
                        pn += r.PeopleNumber;
                    }
                }
            }
            return pn;
        }
        /// 获取 FreshAirArea 的新风指标 add on 20160728 by Yunxiao Lin
        /// <summary>
        /// 获取 FreshAirArea 的新风指标
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public double GetFAIndexOfFreshAirArea(FreshAirArea area)
        {
            double FAIndex = 0;
            int pn = 0;
            foreach (Floor f in _thisProject.FloorList)
            {
                foreach (Room r in f.RoomList)
                {
                    if (r.FreshAirAreaId == area.Id)
                    {
                        pn += r.PeopleNumber;
                    }
                }
            }
            if (pn > 0)
                FAIndex = area.FreshAir / pn;
            return FAIndex;
        }


        #region 获取List<RoomIndoor>

        /// 获取所有已选的 Indoor List （区分FA）
        /// <summary>
        /// 获取所有已选的 Indoor List （区分FA）
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedIndoor(IndoorType flag)
        {
            return _thisProject.RoomIndoorList.FindAll(p => p.IndoorItem.Flag == flag);
        }

        /// 获取指定房间的 Indoor List （含FA）
        /// <summary>
        /// 获取指定房间的 Indoor List （含FA）
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedIndoorByRoom(string roomID)
        {
            if (string.IsNullOrEmpty(roomID))
                return null;
            return _thisProject.RoomIndoorList.FindAll(p => p.RoomID == roomID);
        }

        /// 获取属于指定房间的 Indoor List （区分FA）
        /// <summary>
        /// 获取属于指定房间的 Indoor List （区分FA）
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedIndoorByRoom(string roomID, IndoorType flag)
        {
            if (string.IsNullOrEmpty(roomID))
                return null;
            return _thisProject.RoomIndoorList.FindAll(p => p.RoomID == roomID && p.IndoorItem.Flag == flag);
        }

 

        /// 获取所有已选的 Exchanger List （区分FA）
        /// <summary>
        /// 获取所有已选的 Exchanger List （区分FA）
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedExchanger(IndoorType flag)
        {
            return _thisProject.ExchangerList.FindAll(p => p.IndoorItem.Flag == flag);
        }

        /// 获取指定房间的 Exchanger List （含FA）
        /// <summary>
        /// 获取指定房间的 Exchanger List （含FA）
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedExchangerByRoom(string roomID)
        {
            if (string.IsNullOrEmpty(roomID))
                return null;
            return _thisProject.ExchangerList.FindAll(p => p.RoomID == roomID);
        }

        /// 获取属于指定房间的 Exchanger List （区分FA）
        /// <summary>
        /// 获取属于指定房间的 Exchanger List （区分FA）
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedExchangerByRoom(string roomID, IndoorType flag)
        {
            if (string.IsNullOrEmpty(roomID))
                return null;
            return _thisProject.ExchangerList.FindAll(p => p.RoomID == roomID && p.IndoorItem.Flag == flag);
        }



        /// 获取指定的室外机系统中包含的 Indoor List （含FA）
        /// <summary>
        /// 获取指定的室外机系统中包含的 Indoor List （含FA）
        /// </summary>
        /// <param name="sysItem">所属的 Outdoor 系统</param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedIndoorBySystem(string sysID)
        {
            if (string.IsNullOrEmpty(sysID))
                return new List<RoomIndoor>();
            return _thisProject.RoomIndoorList.FindAll(p => p.SystemID == sysID);
        }

        /// 获取指定系统选择的 Indoor List （区分FA）
        /// <summary>
        /// 获取指定系统选择的 Indoor List （区分FA），用于Report
        /// </summary>
        /// <param name="sysItem">所属的 Outdoor 系统</param>
        /// <param name="flag">区分Indoor|FreshAir</param>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedIndoorBySystem(string sysID, IndoorType flag)
        {
            if (string.IsNullOrEmpty(sysID))
                return null;
            string ID = sysID; //Shweta : ((JCHVRF.Model.SystemBase)_thisProject.SystemListNextGen[0]).Id;
            return _thisProject.RoomIndoorList.FindAll(p => p.SystemID == ID && p.IndoorItem.Flag == flag);
        }

        /// 获取已选择但尚未分配给 Outdoor 系统的 Indoor List 
        /// <summary>
        /// 获取已选择但尚未分配给 Outdoor 系统的 Indoor List 
        /// </summary>
        /// <returns></returns>
        public List<RoomIndoor> GetSelectedIndoorNotArranged()
        {
            return _thisProject.RoomIndoorList.FindAll(p => string.IsNullOrEmpty(p.SystemID));
        }
        #endregion


        /// 新增 RoomIndoor 记录
        /// <summary>
        /// 新增 RoomIndoor 记录
        /// </summary>
        /// <param name="roomID">所属的房间的ID，为空则表示未分配房间</param>
        /// <param name="indItem">Indoor 对象</param>
        /// <returns></returns>
        public RoomIndoor AddIndoor(string roomID, Indoor indItem)
        {
            RoomIndoor item = new RoomIndoor(GetNextRoomIndoorNo());
            item.RoomID = roomID;
            item.SetIndoorItemWithAccessory(indItem);
            item.IsExchanger = false;
            _thisProject.RoomIndoorList.Add(item);
            return item;
        }

        /// 新增Exchanger 记录
        /// <summary>
        /// 新增 Exchanger 记录
        /// </summary>
        /// <param name="roomID">所属的房间的ID，为空则表示未分配房间</param>
        /// <param name="indItem">Indoor 对象</param>
        /// <returns></returns>
        public RoomIndoor AddExchanger(string roomID, Indoor indItem)
        {
            RoomIndoor item = new RoomIndoor(GetNextExchangerNo());
            item.RoomID = roomID;
            item.SetIndoorItemWithAccessory(indItem);
            item.IsExchanger = true;
            _thisProject.ExchangerList.Add(item);
            return item;
        }

        /// 获取 RoomIndoor 对象
        /// <summary>
        /// 获取 RoomIndoor 对象
        /// </summary>
        /// <param name="indNo"></param>
        /// <returns></returns>
        public RoomIndoor GetIndoor(int indNo)
        {
            foreach (RoomIndoor item in _thisProject.RoomIndoorList)
            {
                if (item.IndoorNO == indNo)
                    return item;
            }
            return null;
        }

        /// 获取 Exchanger 对象
        /// <summary>
        /// 获取 Exchanger 对象
        /// </summary>
        /// <param name="indNo"></param>
        /// <returns></returns>
        public RoomIndoor GetExchanger(int indNo)
        {
            foreach (RoomIndoor item in _thisProject.ExchangerList)
            {
                if (item.IndoorNO == indNo)
                    return item;
            }
            return null;
        }

        /// 获取SystemVRF对象
        /// <summary>
        /// 获取SystemVRF对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RoomIndoor GetExchangerByName(string name)
        {
            return _thisProject.ExchangerList.Find(p => p.IndoorName == name);
        }


        /// 添加SystemVRF对象
        /// <summary>
        /// 添加SystemVRF对象
        /// </summary>
        /// <param name="sysItem"></param>
        public void AddSystemVRF(SystemVRF sysItem)
        {
            sysItem.Id = Guid.NewGuid().ToString("N");
            _thisProject.SystemList.Add(sysItem);
        }

        /// 获取SystemVRF对象
        /// <summary>
        /// 获取SystemVRF对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SystemVRF GetSystem(string id)
        {
            return _thisProject.SystemList.Find(p => p.Id == id);
        }

        /// 删除已选的 Indoor 对象
        /// <summary>
        /// 删除已选的 Indoor 对象
        /// </summary>
        /// <param name="indNo"></param>
        /// <returns></returns>
        public void RemoveRoomIndoor(int indNo)
        {
            _thisProject.RoomIndoorList.RemoveAll(c => c.IndoorNO == indNo);
        }


        /// 删除已选的 Exchanger 对象
        /// <summary>
        /// 删除已选的 Exchanger 对象
        /// </summary>
        /// <param name="indNo"></param>
        /// <returns></returns>
        public void RemoveExchanger(int indNo)
        {
            foreach (RoomIndoor ri in _thisProject.ExchangerList)
            {
                //删除全热交换机对应的Controller on20170901 by xyj
                if (ri.IndoorNO == indNo)
                {
                    if ((ri.ControlGroupID.Count>0))
                    {
                        _thisProject.ControllerList.RemoveAll(p =>  ri.ControlGroupID.Contains(p.Id));
                    }
                   // _thisProject.ControlGroupList.RemoveAll(p => p.Id == ri.ControlGroupID);
                }
            }
            _thisProject.ExchangerList.RemoveAll(c => c.IndoorNO == indNo);
           
        }

        /// 删除项目中指定的室外机系统对象，并清除室内机的关联关系
        /// <summary>
        /// 删除项目中指定的室外机系统对象，并清除室内机的关联关系
        /// </summary>
        /// <param name="sysId">SystemVRF ID</param>
        public void RemoveSystemVRF(string sysId)
        {
            SystemVRF item = _thisProject.SystemList.Find(p => p.Id == sysId);
            if (item != null)
            {
                // 删除室内机的关联
                RemoveIndoorListFromSystem(sysId);
                //// 如果已经加入Control Group，则删除关联的组件
                //if (!string.IsNullOrEmpty(item.ControlGroupID))
                //{
                //    _thisProject.ControllerAssemblyList.RemoveAll(p => p.SystemVRFID == sysId);
                //}
                _thisProject.SystemList.Remove(item);
            }
        }

        /// 删除室内机与某室外机的关联关系
        /// <summary>
        /// 删除室内机与某室外机的关联关系
        /// </summary>
        /// <param name="sysID"></param>
        public void RemoveIndoorListFromSystem(string sysID)
        {
            if (_thisProject.RoomIndoorList != null)
            {
                foreach (RoomIndoor item in _thisProject.RoomIndoorList)
                {
                    if (item.SystemID == sysID)
                    {
                        item.SetToSystemVRF("");
                        //删除室外机 清除室内机对应室外机的高度差 on 20180508 by xyj
                        item.HeightDiff = 0;
                        item.PositionType = PipingPositionType.SameLevel.ToString();
                    }
                   
                }
            }
        }


        /// 删除室内机与某室外机的关联关系
        /// <summary>
        /// 删除室内机与某室外机的关联关系
        /// </summary>
        /// <param name="sysID"></param>
        public void ClearIndoorListFromSystem(string sysID)
        {
            if (_thisProject.RoomIndoorList != null)
            {
                foreach (RoomIndoor item in _thisProject.RoomIndoorList)
                {
                    if (item.SystemID == sysID)
                    {
                        item.SetToSystemVRF(""); 
                    }

                }
            }
        }

        /// 清空 Controller 界面已有的布局，一般用于切换控制器模式时
        /// <summary>
        ///  清空 Controller 界面已有的布局，一般用于切换控制器模式时
        /// </summary>
        public void ClearControllerData()
        {
            // 清空Controller界面已有的布局？？删除当前项目中的Controller相关对象，以及SystemVRF与ControlGroup的关联
            _thisProject.ControlSystemList.Clear();
            _thisProject.ControlGroupList.Clear();
            _thisProject.ControllerAssemblyList.Clear();
            _thisProject.ControllerList.Clear();
            foreach (SystemVRF sysItem in _thisProject.SystemList)
            {
                sysItem.BindToControlGroup("");
            }
        }

        //清除不符合当前ControllerLayoutType的BMS Adapter
        /// <summary>
        /// 清除不符合当前ControllerLayoutType的BMS Adapter
        /// </summary>
        public void RemoveInvalidBMSAdapter()
        {
            //找出不符合当前ControllerLayoutType的BMS Adapter
            var controllersToRemove = _thisProject.ControllerList.Where(p =>
            {
                switch (p.Type)
                {
                    case ControllerType.LonWorksInterface:
                        if (_thisProject.ControllerLayoutType == ControllerLayoutType.LONWORKS)
                            return false; 
                        return true;
                    case ControllerType.ModBusInterface:
                        if (_thisProject.ControllerLayoutType == ControllerLayoutType.MODBUS)
                            return false; 
                        return true;
                    case ControllerType.BACNetInterface:
                        if (_thisProject.ControllerLayoutType == ControllerLayoutType.BACNET)
                            return false;
                        return true;
                    case ControllerType.KNXInterface:
                        if (_thisProject.ControllerLayoutType == ControllerLayoutType.KNX)
                            return false;
                        return true;
                    default:
                        return false;
                }
            });

            //删除controller后需要检查如下group是否为空
            var groupsToValidate = controllersToRemove.Select(p => p.ControlGroupID).Distinct(); 

            //删除controller
            controllersToRemove.ToList().ForEach(p => RemoveController(p.Id));

            //检查group是否为空
            if (_thisProject.ControlGroupList != null)
            {
                groupsToValidate.ToList().ForEach(groupId =>
                {
                    int controllerQty = GetControllerQuantityOfGroup(groupId);
                    int outdoorQty = GetOutdoorQuantityOfGroup(groupId);

                    if (controllerQty == 0 && outdoorQty == 0)
                    {
                        //如果为空则删除此group
                        _thisProject.ControlGroupList.RemoveAll(p => p.Id == groupId);
                    }
                });
            }
        }

        /// 清空无效的 Controller 界面已有的布局，一般用于保存数据和离开Controller 选项卡时
        /// <summary>
        ///  清空无效的 Controller 界面已有的布局，一般用于保存数据和离开Controller 选项卡时
        /// </summary>
        public void ClearInvalidControllerData()
        {
            //删除无效的ControlSystem
            _thisProject.ControlSystemList.FindAll(p => p.IsValid == false).ForEach(p => RemoveControlSystem(p.Id));

            //删除无效的ControlGroup
            _thisProject.ControlGroupList.FindAll(p => p.IsValidGrp == false).ForEach(p => RemoveControlGroup(p.Id));
        }

        /// 计算下一个 RoomIndoor NO.
        /// <summary>
        /// 计算下一下 RoomIndoor NO.
        /// </summary>
        /// <returns></returns>
        public int GetNextRoomIndoorNo()
        {
            int max = 0;
            foreach (RoomIndoor ri in _thisProject.RoomIndoorList)
            {
                max = max > ri.IndoorNO ? max : ri.IndoorNO;
            }
            max = max + 1;
            //新增判断去除重复
            if (_thisProject != null && _thisProject.RoomIndoorList != null)
            {
                while (true)
                {
                    string name = SystemSetting.UserSetting.defaultSetting.IndoorName + max;
                    var item = _thisProject.RoomIndoorList.Find(p => p.IndoorName == name);
                    if (item != null)
                    {
                        max++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return max;
        }


        /// 计算下一个 Exchanger NO.
        /// <summary>
        /// 计算下一下 Exchanger NO.
        /// </summary>
        /// <returns></returns>
        public int GetNextExchangerNo()
        {
            int max = 0;
            foreach (RoomIndoor ri in _thisProject.ExchangerList)
            {
                max = max > ri.IndoorNO ? max : ri.IndoorNO;
            }
            return max + 1;
        }

        /// 获取下一个室外机系统 NO.
        /// <summary>
        /// 获取下一个室外机系统 NO.
        /// </summary>
        /// <returns></returns>
        public int GetNextSystemNo()
        {
            int max = 0;
            foreach (SystemVRF item in _thisProject.SystemList)
            {
                max = max > item.NO ? max : item.NO;
            }
            return max + 1;
        }


        /// 计算相对湿度
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        public double CalculateRH(double db, double wb, int altitude)
        {
            FormulaCalculate formula = new FormulaCalculate();
            decimal p = formula.GetPressure(Convert.ToDecimal(altitude));
            decimal rh = formula.GetRH(Convert.ToDecimal(db), Convert.ToDecimal(wb), p);
            return Convert.ToDouble(rh);
        }



        // 计算 Indoor 的估算容量总和（Cooling）
        /// <summary>
        /// 计算 Indoor 的估算容量总和（Cooling）
        /// </summary>
        /// <param name="list"></param>
        /// <param name="senHeat"></param>
        /// <param name="airFlow"></param>
        /// <returns></returns>
        public double CalIndoorEstCapacitySumCool(List<RoomIndoor> list, out double senHeat, out double airFlow)
        {
            double capacity = 0;
            senHeat = 0;
            airFlow = 0;
            foreach (RoomIndoor ri in list)
            {
                capacity += ri.CoolingCapacity;
                senHeat += ri.SensibleHeat;
                airFlow += ri.AirFlow;
            }
            return capacity;
        }

        // 计算 Indoor 的估算容量总和（Cooling & Heating）
        /// <summary>
        /// 计算 Indoor 的估算容量总和（Cooling）
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public double CalIndoorEstCapacitySum(List<RoomIndoor> list, out double capHeat)
        {
            capHeat = 0;
            double capCool = 0;
            foreach (RoomIndoor ri in list)
            {
                //capCool += ri.CoolingCapacity;
                //capHeat += ri.HeatingCapacity;
                capCool += ri.RqCoolingCapacity;
                capHeat += ri.RqHeatingCapacity;
            }
            return capCool;
        }


        // 计算指定室内机的实际Capacity值（经过Piping校验计算之后的最终值，已改为实际值） 20161116 by Yunxiao Lin
        /// <summary>
        /// 计算指定室内机的实际Capacity值（经过Piping校验计算之后的最终值，已改为实际值）
        /// </summary>
        /// <param name="ri">RoomIndoor对象</param>
        /// <returns></returns>
        public static double GetIndoorActCapacityCool(RoomIndoor ri, out double actHeat)
        {
            //actHeat = 0;
            //double actCool = 0;

            //SystemVRF sysItem = GetSystem(ri.SystemName);
            //if (sysItem != null)
            //{
            //    double inRatCap = GetInRatCapacity(ri);

            //    double totInRating = GetTotalInRatCapacity(sysItem.Name);
            //    double outActCap = GetOutActCapacity(sysItem);

            //    double ret = inRatCap * outActCap / (totInRating * sysItem.DiversityFactor);
            //    return ret;
            //}
            //return actCool;
            //actHeat = ri.HeatingCapacity;
            //return ri.CoolingCapacity;
            actHeat = ri.ActualHeatingCapacity;
            return ri.ActualCoolingCapacity;
        }

        /// <summary>
        /// 检查系统内的室内机和附件是否符合要求
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns>
        /// </returns>
        public bool ValidateSystemForController(SystemVRF sysItem)
        {
            //string[] accLimitation = {"PC-RLH11", "PC-RLHN8"}; //TFS 3264: Remove this limitation 20190610 by Yunxiao Lin

            foreach (RoomIndoor roomIndoor in _thisProject.RoomIndoorList)
            {
                if (roomIndoor.SystemID != sysItem.Id) continue;
                if (roomIndoor.IndoorItem == null) continue;

                //for Malaysia case, when use “JTFE”(RPFC-FSNQ) as IDU, it has the limitation that central controller cannot be used.
                //“JTFE”(RPFC-FSNQ) models' unit type is "Floor Ceiling".
                //if (_thisProject.SubRegionCode == "MAL" && roomIndoor.IndoorItem.Type == "Floor Ceiling")
                if ((_thisProject.SubRegionCode == "MAL" || _thisProject.SubRegionCode == "LA_BV") && _thisProject.BrandCode == "Y" && roomIndoor.IndoorItem.Type == "Floor Ceiling") //马来西亚只有YORK品牌才有此限制 20161023 by Yunxiao Lin
                {
                    //玻利维亚的York产品线和马来西亚一样 20170629 by Yunxiao Lin
                    return false;
                }

                if (roomIndoor.ListAccessory == null) continue;

                //when HAPQ’s receiver kit (PC-RLH11, PC-RLHN8) is used, then, all central controller cannot be used.
                //if (roomIndoor.ListAccessory.Exists(acc => acc != null && acc.FactoryCode == "Q" && accLimitation.Contains(acc.Model_Hitachi)))
                //{
                //    return false;
                //}
            }
            return true;
        }

        #region 方法for Controller


        #region 方法 for Controller
        /// 从控制器组中移除某个室外机，同时删除其对应的组件记录
        /// <summary>
        /// 从控制器组中移除某个室外机，同时删除其对应的组件记录
        /// </summary>
        /// <param name="systemVRFID">SystemVRF对象的Id</param>
        public void RemoveOutdoorFromControlGroup(string systemVRFID)
        {
            SystemVRF item = this.GetSystem(systemVRFID);
            item.BindToControlGroup("");
            _thisProject.ControllerAssemblyList.RemoveAll(p => p.SystemVRFID == systemVRFID);
        }


        /// 从控制器组中移除某个室外机，同时删除其对应的组件记录
        /// <summary>
        /// 从控制器组中移除某个室外机，同时删除其对应的组件记录
        /// </summary>
        /// <param name="systemVRFID">SystemVRF对象的Id</param>
        public void RemoveOutdoorFromControlGroupByExchanger(string systemVRFID)
        {
            RoomIndoor ri = this.GetExchangerByName(systemVRFID);

            ri.ControlGroupID.Add("");
            //_thisProject.ControllerAssemblyList.RemoveAll(p => p.SystemVRFID == systemVRFID);
        }


        ///// 删除某个控制器组中的所有室外机记录，同时删除对应的所有的组件记录
        ///// <summary>
        ///// 删除某个控制器组中的所有室外机记录，同时删除对应的所有的组件记录
        ///// </summary>
        ///// <param name="controlGroupID"></param>
        //private void RemoveOutdoorListFromControlGroup(string controlGroupID)
        //{
        //    // 删除该Group下的Assembly记录
        //    if (this.ControllerAssemblyList != null)
        //    {
        //        this.ControllerAssemblyList.RemoveAll(p => p.ControlGroupID == controlGroupID);
        //    }

        //    // 移除该Group下的所有室外机记录
        //    foreach (SystemVRF item in this.SystemList)
        //    {
        //        if (item.ControlGroupID == controlGroupID)
        //            item.BindToControlGroup("");
        //    }
        //}

        /// 删除指定的控制器
        /// <summary>
        /// 删除指定的控制器
        /// </summary>
        /// <param name="id">Controller对象的Id</param>
        public void RemoveController(string id)
        {
            if (_thisProject.ControllerList != null)
            {
                _thisProject.ControllerList.RemoveAll(p => p.Id == id);
            }
        }

        /// 删除指定的控制器组，以及其中的子对象
        /// <summary>
        /// 删除指定的控制器组，以及其中的子对象
        /// </summary>
        /// <param name="id">ControlGroup对象的Id</param>
        public void RemoveControlGroup(string id)
        {
            // 删除该Group下的Controller记录
            if (_thisProject.ControllerList != null)
            {
                _thisProject.ControllerList.RemoveAll(p => p.ControlGroupID == id);
            }

            // 删除该Group下的Assembly记录
            if (_thisProject.ControllerAssemblyList != null)
            {
                _thisProject.ControllerAssemblyList.RemoveAll(p => p.ControlGroupID == id);
            }

            // 移除该Group下的所有室外机记录
            foreach (SystemVRF item in _thisProject.SystemList)
            {
                if (item.ControlGroupID.Contains(id))
                    item.BindToControlGroup("");
            }

            // 移除该Group下的所有热交换机记录  on 20180315 by xyj
            foreach (RoomIndoor item in _thisProject.ExchangerList)
            {
                if (item.ControlGroupID.Contains(id))
                    item.ControlGroupID.RemoveAll(x=>x==id) ;
            }

            // 删除Control Group对象
            if (_thisProject.ControlGroupList != null)
            {
                _thisProject.ControlGroupList.RemoveAll(p => p.Id == id);
            }

        }

        /// 删除指定的控制器系统，以及其中包含的所有子对象
        /// <summary>
        /// 删除指定的控制器系统，以及其中包含的所有子对象
        /// </summary>
        /// <param name="id">ControlSystem对象的Id</param>
        public void RemoveControlSystem(string id)
        {
            // 先删除该System下的Controller
            if (_thisProject.ControllerList != null)
            {
                _thisProject.ControllerList.RemoveAll(p => p.ControlSystemID == id);
            }

            if (_thisProject.ControlGroupList != null)
            {
                _thisProject.ControlGroupList.FindAll(p => p.ControlSystemID == id).ForEach(p => RemoveControlGroup(p.Id));
            }

            if (_thisProject.ControlSystemList != null)
            {
                _thisProject.ControlSystemList.Remove(_thisProject.ControlSystemList.Where(p => p.Id == id).FirstOrDefault());

            }

        }

        /// 新增指定类型的Controller对象，并添加到指定的ControlSystem中
        /// <summary>
        /// 新增指定类型的Controller对象，并添加到指定的ControlSystem中
        /// </summary>
        /// <param name="type">控制器系统的布局类型，Modbus or Bacnet</param>
        /// <param name="controlSystemID">ControlSystem对象的Id</param>
        /// <returns>返回添加的Controller对象的Id</returns>
        public string AddControllerToControlSystem(CentralController type, string controlSystemID)
        {
            Controller item = new Controller(type);
            item.AddToControlSystem(controlSystemID);
            _thisProject.ControllerList.Add(item);
            return item.Id;
        }

        /// 新增指定类型的Controller对象，并添加到指定ControlGroup中
        /// <summary>
        /// 新增指定类型的Controller对象，并添加到指定ControlGroup中
        /// </summary>
        /// <param name="type">用于确定控制器的型号</param>
        /// <param name="controlGroupID">ControlGroup对象的Id</param>
        /// <returns>返回添加的Controller对象的Id</returns>
        public Controller AddControllerToControlGroup(CentralController type, string controlGroupID)
        {
            Controller item = new Controller(type);
            item.AddToControlGroup(controlGroupID);
            _thisProject.ControllerList.Add(item);
            return item;
        }

        /// 将室外机系统对象添加到指定的ControlGroup对象中，并新增相应的ControllerAssembly对象
        /// <summary>
        /// 将室外机系统对象添加到指定的ControlGroup对象中，并新增相应的ControllerAssembly对象
        /// </summary>
        /// <param name="systemID">室外机记录所属的系统Id</param>
        /// <param name="controlGroupID">所属ControlGroup对象的Id</param>
        /// <param name="type">控制器系统的布局类型，Modbus or Bacnet，用于确定组件的型号</param>
        public void AddOutdoorToControlGroup(SystemVRF systemVRF, string controlGroupID, ControllerLayoutType type)
        {
            systemVRF.BindToControlGroup(controlGroupID);

            //ControllerAssembly assembly = new ControllerAssembly(type, systemVRF);
            //assembly.AddToControlGroup(controlGroupID);
            //_thisProject.ControllerAssemblyList.Add(assembly);
        }

        /// 新增ControlGroup对象，并添加到指定的ControlSystem对象中
        /// <summary>
        /// 新增ControlGroup对象，并添加到指定的ControlSystem对象中
        /// </summary>
        /// <param name="controlSystemID">所属ControlSystem对象的Id</param>
        /// <returns>返回添加的ControlGroup对象的Id</returns>
        public string AddGroupToControlSystem(string controlSystemID)
        {
            ControlGroup item = new ControlGroup();
            item.IsValidGrp = false;
            item.AddToControlSystem(controlSystemID);
            _thisProject.ControlGroupList.Add(item);
            return item.Id;
        }

        /// 新增ControlSystem对象
        /// <summary>
        /// 新增ControlSystem对象
        /// </summary>
        /// <returns>返回Id</returns>
        public string AddControlSystem()
        {
            ControlSystem item = new ControlSystem();
            _thisProject.ControlSystemList.Add(item);
            return item.Id;
        }

        public Controller GetController(string id)
        {
            return _thisProject.ControllerList.Find(p => p.Id == id);
        }

        public ControlGroup GetControlGroup(string id)
        {
            return _thisProject.ControlGroupList.Find(p => p.Id == id);
        }

        public ControlSystem GetControlSystem(string id)
        {
            return _thisProject.ControlSystemList.Find(p => p.Id == id);
        }

        public ControllerAssembly GetControllerAssembly(string systemVRFID)
        {
            return _thisProject.ControllerAssemblyList.Find(p => p.SystemVRFID == systemVRFID);
        }

        public int GetControllerQuantityOfGroup(string controllerGroupID)
        {
            return _thisProject.ControllerList.Count(p => p.ControlGroupID == controllerGroupID);
        }

        public int GetOutdoorQuantityOfGroup(string controllerGroupID)
        {
            return _thisProject.SystemList.Count(p => p.ControlGroupID.Contains(controllerGroupID));
        }

        #endregion

        #endregion

        
        #region Import 相关处理（外部项目导入）
        /// 为当前项目添加导入的 Floor List（含 Room）
        /// <summary>
        /// 为当前项目添加导入的 Floor List（含 Room）
        /// </summary>
        /// <param name="importList"></param>
        /// <param name="list"></param>
        private void AddImportFloorList(List<Floor> importList)
        {
            if (importList.Count > 0)
            {
                foreach (Floor f in importList)
                {
                    f.ParentId = Project.ProjectCount + 1;
                    f.IsImport = true;
                    _thisProject.FloorList.Add(f);
                }
                ++Project.ProjectCount;
            }
        }

        /// 为当前项目添加导入的 Indoor List
        /// <summary>
        /// 为当前项目添加导入的 Indoor List
        /// </summary>
        /// <param name="importList"></param>
        /// <param name="list"></param>
        private void AddImportRoomIndoor(List<RoomIndoor> importList)
        {
            if (importList.Count > 0)
            {
                foreach (RoomIndoor item in importList)
                {
                    _thisProject.RoomIndoorList.Add((RoomIndoor)item);
                }
            }
        }

        /// 导入室外机系统
        /// <summary>
        /// 
        /// </summary>
        /// <param name="importList"></param>
        private void AddImportSystem(List<SystemVRF> importList)
        {
            if (importList.Count > 0)
            {
                foreach (SystemVRF item in importList)
                {
                    _thisProject.SystemList.Add(item);
                }
            }
        }

        /// 导入控制器系统
        /// <summary>
        /// 导入控制器系统
        /// </summary>
        /// <param name="importList"></param>
        private void AddImportControllerSystem(List<ControlSystem> importList)
        {
            if (importList.Count > 0)
            {
                foreach (ControlSystem item in importList)
                {
                    _thisProject.ControlSystemList.Add(item);
                }
            }
        }

        /// 导入控制器组
        /// <summary>
        /// 导入控制器组
        /// </summary>
        /// <param name="importList"></param>
        private void AddImportControllerGroup(List<ControlGroup> importList)
        {
            if (importList.Count > 0)
            {
                foreach (ControlGroup item in importList)
                {
                    _thisProject.ControlGroupList.Add(item);
                }
            }
        }

        /// 导入控制器列表
        /// <summary>
        /// 导入控制器列表
        /// </summary>
        /// <param name="importList"></param>
        private void AddImportControllerList(List<Controller> importList)
        {
            if (importList.Count > 0)
            {
                foreach (Controller item in importList)
                {
                    _thisProject.ControllerList.Add(item);
                }
            }
        }

        ///// 导入控制器组件 （暂不需要/20160524/shenjunjie）
        ///// <summary>
        ///// 导入控制器组件
        ///// </summary>
        ///// <param name="importList"></param>
        //private void AddImportControllerAssembly(List<ControllerAssembly> importList)
        //{
        //    if (importList.Count > 0)
        //    {
        //        foreach (ControllerAssembly item in importList)
        //        {
        //            _thisProject.ControllerAssemblyList.Add(item);
        //        }
        //    }
        //}

        /// 导入指定的项目
        /// <summary>
        /// 导入指定的项目
        /// </summary>
        /// <param name="importProject"></param>
        public void AddImportProject(ref Project importProject)
        {
            AddImportFloorList(importProject.FloorList);
            AddImportRoomIndoor(importProject.RoomIndoorList);
            AddImportSystem(importProject.SystemList);

            // 20141029 clh 由于暂时不考虑多个Control System ，故此处必须省略以下内容的导入，因为导入即会导致有多个ControlSystem存在
            //AddImportControllerSystem(importProject.ControlSystemList);
            //AddImportControllerGroup(importProject.ControlGroupList);
            //AddImportControllerList(importProject.ControllerList);
            //AddImportControllerAssembly(importProject.ControllerAssemblyList);

            // 清空Controller界面已有的布局，删除当前项目中的Controller相关对象，以及SystemVRF与ControlGroup的关联
            importProject.ControlSystemList.Clear();
            importProject.ControlGroupList.Clear();
            importProject.ControllerAssemblyList.Clear();
            importProject.ControllerList.Clear();
            foreach (SystemVRF sysItem in importProject.SystemList)
            {
                sysItem.BindToControlGroup("");
            }
        }

        #endregion


        #region Copy Indoor

        /// <summary>
        /// 拷贝室内机
        /// </summary>
        public void CopyIndoor(
            List<RoomIndoor> roomInoorList,
            string indoorNamePrefix,
            string floorNamePrefix,
            string roomNamePrefix, 
            string freshAirAreaNamePrefix, 
            string systemID)
        {
            Hashtable floorCopiesCache = new Hashtable();
            Hashtable roomCopiesCache = new Hashtable();
            Hashtable freshAirAreaCopiesCache = new Hashtable();

            //复制新风区域
            var sortedFreshAirAreaList = from ind in roomInoorList
                                         from f in _thisProject.FreshAirAreaList
                                         where ind.IsFreshAirArea && f.Id == ind.RoomID
                                         orderby f.NO
                                         select f;

            foreach (FreshAirArea area in sortedFreshAirAreaList.Distinct())
            {
                CopyFreshAirAreaOfIndoor(area, freshAirAreaCopiesCache, freshAirAreaNamePrefix);
            }

            //复制房间
            var sortedRoomList = from ind in roomInoorList
                                 from f in _thisProject.FloorList
                                 from r in f.RoomList
                                 where !string.IsNullOrEmpty(ind.RoomID) && (ind.IsFreshAirArea ? r.FreshAirAreaId == ind.RoomID : r.Id == ind.RoomID)
                                 orderby f.NO, r.NO
                                 select new { floor = f, room = r };
            foreach (var ent in sortedRoomList.Distinct())
            {
                Floor floorCopy;
                Room roomCopy;
                CopyRoomOfIndoor(ent.floor, ent.room, out floorCopy, out roomCopy, floorCopiesCache, roomCopiesCache, floorNamePrefix, roomNamePrefix);

                if (!string.IsNullOrEmpty(roomCopy.FreshAirAreaId))
                {
                    if (freshAirAreaCopiesCache.ContainsKey(roomCopy.FreshAirAreaId))
                    {
                        FreshAirArea freshAirAreaCopy = freshAirAreaCopiesCache[roomCopy.FreshAirAreaId] as FreshAirArea;
                        //更新新风区域外键
                        roomCopy.FreshAirAreaId = freshAirAreaCopy.Id;
                    }
                    else
                    {
                        //关联的新风区域没有被复制，则脱钩
                        roomCopy.FreshAirAreaId = null;
                    }
                }
            }

            //复制室内机
            var sortedIndoorList = roomInoorList.OrderBy(ind => ind.IndoorNO);
            foreach (RoomIndoor ind in sortedIndoorList)
            {
                RoomIndoor indoorCopy = CopyAddRoomIndoor(ind, systemID, indoorNamePrefix);

                //基于房间室内机 需要更新RoomID
                if (!string.IsNullOrEmpty(ind.RoomID))
                {
                    if (ind.IsFreshAirArea)
                    {
                        FreshAirArea freshAirAreaCopy = freshAirAreaCopiesCache[ind.RoomID] as FreshAirArea;
                        indoorCopy.RoomID = freshAirAreaCopy.Id;
                        indoorCopy.RoomName = freshAirAreaCopy.Name;
                    }
                    else
                    {
                        Room roomCopy = roomCopiesCache[ind.RoomID] as Room;
                        indoorCopy.RoomID = roomCopy.Id;
                        indoorCopy.RoomName = roomCopy.Name;
                    }
                }
            }
        }

        /// 拷贝添加 RoomIndoor 节点
        /// <summary>
        /// 拷贝添加 RoomIndoor 节点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public RoomIndoor CopyAddRoomIndoor(RoomIndoor item, string systemID, string namePrefix)
        {
            int no = GetNextRoomIndoorNo();
            //RoomIndoor newItem = new RoomIndoor(no);
            //CopyObject(item, newItem);
            RoomIndoor newItem = item.DeepClone();

            newItem.IndoorNO = no;
            newItem.IndoorName = namePrefix + no;
            newItem.SystemID = systemID; 
            //if (!string.IsNullOrEmpty(item.IndoorFullName))
            //{
            //    newItem.IndoorFullName = newItem.IndoorName + "[" + item.IndoorFullName.Split('[')[1];
            //}
            //else
            //{
            //    //记录indoor对象的indoor名
            //    newItem.IndoorFullName = newItem.IndoorName + "[" + 
            //        (_thisProject.BrandCode == "Y" ? newItem.IndoorItem.Model_York : newItem.IndoorItem.Model_Hitachi) + "]"; 
            //}
            //共享控制器关联关系重置
            newItem.IndoorItemGroup = null;
            newItem.IsMainIndoor = false;
            if (newItem.ListAccessory != null && newItem.ListAccessory.Count > 0)
            {
                newItem.ListAccessory.RemoveAll(c => c.IsShared == true);
                if (IsReceiver(newItem.ListAccessory))
                {
                    newItem.ListAccessory.RemoveAll(c => c.Type == "Receiver Kit for Wireless Control");
                }
            }

            if (_thisProject.RoomIndoorList != null)
            {
                _thisProject.RoomIndoorList.Add(newItem);
            }
            return newItem;
        }

        //判断当前的室内机内是否只包含Receiver Kit for Wireless Control
        private bool IsReceiver(List<Accessory> list)
        {
            bool isExists = false;
            if (list.Count > 0)
            {
                if (list.FindAll(p => p.Type == "Receiver Kit for Wireless Control" && p.Type == "Wireless Remote Control Switch").Count>0)
                {
                    return false;
                }
                if (list.FindAll(p => p.Type == "Receiver Kit for Wireless Control").Count > 0 && list.FindAll(p => p.Type == "Wireless Remote Control Switch").Count < 1)
                {
                    return true;
                }
            }
            return isExists;
        }


        private void CopyRoomOfIndoor(Floor originFloor, Room originRoom,
            out Floor floorCopy, out Room roomCopy,
            Hashtable floorCopiesCache, Hashtable roomCopiesCache,
            string floorNamePrefix, string roomNamePrefix)
        {
            floorCopy = null;
            roomCopy = null;

            if (originRoom == null) return;

            if (floorCopiesCache.ContainsKey(originFloor.Id))
            {
                floorCopy = floorCopiesCache[originFloor.Id] as Floor;
            }

            if (roomCopiesCache.ContainsKey(originRoom.Id))
            {
                roomCopy = roomCopiesCache[originRoom.Id] as Room;
                return;
            }

            if (floorCopy == null)
            {
                floorCopy = CopyFloorOfIndoor(originFloor, floorNamePrefix);
                floorCopiesCache[originFloor.Id] = floorCopy;
            }
            roomCopy = CopyAddRoom(originRoom, floorCopy);
            roomCopy.Name = roomNamePrefix + roomCopy.NO;
            roomCopiesCache[originRoom.Id] = roomCopy;
        }

        private Floor CopyFloorOfIndoor(Floor originFloor, string namePrefix)
        {
            int no = GetNextFloorNO();
            //Floor newItem = new Floor(no);
            //CopyObject(originFloor, newItem);
            Floor newItem = originFloor.DeepClone();

            newItem.Id = Guid.NewGuid().ToString("N");
            newItem.NO = no;
            newItem.Name = namePrefix + no;
            newItem.RoomList = new List<Room>();

            if (_thisProject.FloorList != null)
            {
                _thisProject.FloorList.Add(newItem);
            }
            return newItem;
        }

        public FreshAirArea CopyFreshAirAreaOfIndoor(FreshAirArea originArea, Hashtable freshAirAreaCopiesCache, string freshAirAreaNamePrefix)
        {
            if (freshAirAreaCopiesCache.ContainsKey(originArea.Id))
            {
                return freshAirAreaCopiesCache[originArea.Id] as FreshAirArea;
            }
            
            int no = GetNextFreshAirAreaNO();
            //FreshAirArea areaCopy = new FreshAirArea(no);
            //CopyObject(originArea, areaCopy);
            FreshAirArea areaCopy = originArea.DeepClone();
            areaCopy.Id = Guid.NewGuid().ToString("N");
            areaCopy.NO = no;
            areaCopy.Name = freshAirAreaNamePrefix + no;

            if (_thisProject.FreshAirAreaList != null)
            {
                _thisProject.FreshAirAreaList.Add(areaCopy);
            }

            freshAirAreaCopiesCache[originArea.Id] = areaCopy;
            return areaCopy;
        }

        #endregion


        #region Copy Exchanger
        /// <summary>
        /// 拷贝全热交换机
        /// </summary>
        public void CopyExchanger(
            List<RoomIndoor> roomInoorList,
            string indoorNamePrefix,
            string floorNamePrefix,
            string roomNamePrefix,
            string freshAirAreaNamePrefix,
            string systemID)
        {
            Hashtable floorCopiesCache = new Hashtable();
            Hashtable roomCopiesCache = new Hashtable();
            Hashtable freshAirAreaCopiesCache = new Hashtable();

            

            //复制房间
            var sortedRoomList = from ind in roomInoorList
                                 from f in _thisProject.FloorList
                                 from r in f.RoomList
                                 where !string.IsNullOrEmpty(ind.RoomID) && (ind.IsFreshAirArea ? r.FreshAirAreaId == ind.RoomID : r.Id == ind.RoomID)
                                 orderby f.NO, r.NO
                                 select new { floor = f, room = r };
            foreach (var ent in sortedRoomList.Distinct())
            {
                Floor floorCopy;
                Room roomCopy;
                CopyRoomOfIndoor(ent.floor, ent.room, out floorCopy, out roomCopy, floorCopiesCache, roomCopiesCache, floorNamePrefix, roomNamePrefix);

                
            }

            //复制全热交换机
            var sortedIndoorList = roomInoorList.OrderBy(ind => ind.IndoorNO);
            foreach (RoomIndoor ind in sortedIndoorList)
            {
                RoomIndoor indoorCopy = CopyAddRoomExchanger(ind, systemID, indoorNamePrefix);

                //基于房间全热交换机 需要更新RoomID
                if (!string.IsNullOrEmpty(ind.RoomID))
                {

                    Room roomCopy = roomCopiesCache[ind.RoomID] as Room;
                    indoorCopy.RoomID = roomCopy.Id;
                    indoorCopy.RoomName = roomCopy.Name;
                    indoorCopy.ControlGroupID.Add("");
                }
            }
        }

        /// 拷贝添加 Exchanger 节点
        /// <summary>
        /// 拷贝添加 Exchanger 节点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public RoomIndoor CopyAddRoomExchanger(RoomIndoor item, string systemID, string namePrefix)
        {
            int no = GetNextExchangerNo();
            //RoomIndoor newItem = new RoomIndoor(no);
            //CopyObject(item, newItem);
            RoomIndoor newItem = item.DeepClone();

            newItem.IndoorNO = no;
            newItem.IndoorName = namePrefix + no;
            newItem.SystemID = systemID;
            //newItem.IndoorFullName = newItem.IndoorName + "[" + _thisProject.BrandCode == "Y" ? item.IndoorItem.Model_York : item.IndoorItem.Model_Hitachi + "]";
            //共享控制器关联关系重置
            //共享控制器关联关系重置
            newItem.IndoorItemGroup = null;
            newItem.IsMainIndoor = false;
            //清空交换机与控制器直接的关联关系
            newItem.ControlGroupID.Add(""); 
            if (_thisProject.ExchangerList != null)
            {
                _thisProject.ExchangerList.Add(newItem);
            }
            return newItem;
        }

        #endregion

        #region Copy Outdoor

        /// <summary>
        /// 拷贝室外机
        /// </summary>
        public SystemVRF CopyOutdoor(
            SystemVRF sysItem,
            string outdoorNamePrefix,
            string indoorNamePrefix,
            string floorNamePrefix,
            string roomNamePrefix,
            string freshAirAreaNamePrefix)
        {
            Hashtable floorCopiesCache = new Hashtable();
            SystemVRF sysCopy = CopyAddSystemVRF(sysItem, outdoorNamePrefix);

            List<RoomIndoor> indoorList = _thisProject.RoomIndoorList.FindAll(ind => ind.SystemID == sysItem.Id);

            //复制室内机
            CopyIndoor(indoorList,
                indoorNamePrefix,
                floorNamePrefix,
                roomNamePrefix,
                freshAirAreaNamePrefix,
                sysCopy.Id);

            return sysCopy;
        }

        /// 拷贝添加 SystemVRF
        /// <summary>
        /// 拷贝添加 SystemVRF
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public SystemVRF CopyAddSystemVRF(SystemVRF item, string namePrefix)
        {
            int no = GetNextSystemNo();
            //SystemVRF newItem = new SystemVRF(no);
            //CopyObject(item, newItem);
            SystemVRF newItem = item.DeepClone();

            newItem.Id = Guid.NewGuid().ToString("N");
            newItem.NO = no;
            newItem.Name = namePrefix + no;
            newItem.BindToControlGroup(null);

            //if (item.OutdoorItem != null)
            //{
            //    Outdoor outdoorCopy = new Outdoor();
            //    CopyObject(item.OutdoorItem, outdoorCopy);
            //    newItem.OutdoorItem = outdoorCopy;
            //}

            if (_thisProject.SystemList != null)
            {
                _thisProject.SystemList.Add(newItem);
            }
            return newItem;
        }


        #endregion

        public string CompatOldProjectRegion()
        {
            return _dal.CompatOldProjectRegion();
        }
        
    }
}
