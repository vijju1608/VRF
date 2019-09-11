using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Model;
using JCBase.Utility;
using JCHVRF.Model;
using System.Text.RegularExpressions;

namespace JCHVRF_New.Common.Helpers
{
    public static class ExtensionMethods
    {

        public static JCHVRF.Model.NextGen.SystemVRF Duplicate(this JCHVRF.Model.NextGen.SystemVRF systemToDuplicate)
        {

            if (systemToDuplicate == null)
                throw new ArgumentNullException("Input VRF system was empty");

            var currentProject = Project.CurrentProject;
            var newSystem = ((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).DeepClone<JCHVRF.Model.NextGen.SystemVRF>();

            var count = (currentProject.SystemListNextGen.Count + 1);
            var systemName = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName + " " + count;

            while(currentProject.SystemListNextGen.Any(i => i.Name.ToLower() == systemName.ToLower()))
            {
                count++;
                systemName = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName + " " + count;
            }

            newSystem.Name = systemName;
            newSystem.RegenerateId();
            newSystem.IsActiveSystem = true;
            newSystem.ControlGroupID = null;
            //newSystem.ControlGroupID= systemToDuplicate.ControlGroupID.DeepClone();
            newSystem.ControlGroupID = new List<string>();
            if (currentProject.RoomIndoorList.Count > 0)
            {
                var idNo = currentProject.RoomIndoorList.Max(i => i.IndoorNO);

                var newRoomIndoors = new List<RoomIndoor>();
                foreach (var ri in currentProject.RoomIndoorList.Where(ri => ri.SystemID == systemToDuplicate.Id))
                {
                    var newri = ri.DeepClone<RoomIndoor>();
                    newri.SystemID = newSystem.Id;
                    newri.IndoorNO = ++idNo;
                    newRoomIndoors.Add(newri);
                }

                currentProject.RoomIndoorList.AddRange(newRoomIndoors);
            }
            if (newSystem is JCHVRF.Model.NextGen.SystemVRF)
            {
                newSystem.MyPipingNodeOut = null;
                newSystem.MyPipingNodeOutTemp = null;
                newSystem.IsInputLengthManually = false;
            }
            currentProject.SystemListNextGen.Add((JCHVRF.Model.NextGen.SystemVRF)newSystem);
            //newSystem.MyPipingNodeOut = ((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).MyPipingNodeOut;
            //newSystem.MyPipingNodeOutTemp = ((JCHVRF.Model.NextGen.SystemVRF)systemToDuplicate).MyPipingNodeOutTemp;
            return newSystem;
        }


        public static SystemHeatExchanger Duplicate(this SystemHeatExchanger systemToDuplicate)
        {
            if (systemToDuplicate == null)
                throw new ArgumentNullException("Input Exchanger system was empty");
            var currentProject = Project.GetProjectInstance;
            var newSystem = ((SystemHeatExchanger)systemToDuplicate).DeepClone<SystemHeatExchanger>();

            // system id
            var count = (currentProject.HeatExchangerSystems.Count + 1);
            var systemName = SystemSetting.UserSetting.defaultSetting.ExchangerName + " " + count;

            while (currentProject.HeatExchangerSystems.Any(i => i.Name.ToLower() == systemName.ToLower()))
            {
                count++;
                systemName = SystemSetting.UserSetting.defaultSetting.ExchangerName + " " + count;
            }


            newSystem.Name = systemName;
            newSystem.RegenerateId();

            var roomIndoor = new RoomIndoor();
            roomIndoor = Project.CurrentProject.ExchangerList[Project.CurrentProject.ExchangerList.FindIndex(x => x.SystemID == systemToDuplicate.Id)].DeepClone();
            // roomIndoor = Project.CurrentProject.ExchangerList[Project.CurrentProject.ExchangerList.FindIndex(x => x.SystemID == systemToDuplicate.Id)];
            roomIndoor.SystemID = newSystem.Id;
            roomIndoor.ControlGroupID = new List<string>();
            Project.CurrentProject.ExchangerList.Add(roomIndoor);

            currentProject.HeatExchangerSystems.Add((SystemHeatExchanger)newSystem);

            return newSystem;
        }

        public static ControlSystem Duplicate(this ControlSystem systemToDuplicate)
        {
            var currentProject = Project.GetProjectInstance;
            var count = (currentProject.ControlSystemList.Count + 1).ToString();
           
            var newSystem = (systemToDuplicate).DeepClone<ControlSystem>();
            string  systemName = String.Format("Copy Of {0}", systemToDuplicate.Name);
            newSystem.Name = systemName;
            newSystem.RegenerateId();
            currentProject.ControlSystemList.Add(newSystem);
            CreateGroup(newSystem, systemToDuplicate);
            
            return newSystem;
        }

        private static void CreateGroup(SystemBase newSystem, ControlSystem systemToDuplicate)
        {
            ControlGroup _group = new ControlGroup();
            ControlGroup _groupP = Project.CurrentProject.ControlGroupList.Find(x => x.ControlSystemID.Equals(systemToDuplicate.Id));
         
           
            _group = _groupP.DeepClone();
            List<Controller> _controllers = Project.CurrentProject.ControllerList.FindAll(x => x.ControlGroupID.Equals(_groupP.Id));

           
            _group.AddToControlSystem(newSystem.Id);
            _group.SetName("Group " + (Project.CurrentProject.ControlGroupList.Count + 1));
            _group.Id = Guid.NewGuid().ToString("N");
            foreach (var item in _controllers)
            {
                Controller controllerCopy = item.DeepClone();
                controllerCopy.ControlGroupID = _group.Id;
                controllerCopy.ControlSystemID = newSystem.Id;
                Project.CurrentProject.ControllerList.Add(controllerCopy);

            }
            Project.CurrentProject.ControlGroupList.Add(_group);
            List<RoomIndoor> copySysHeatExch = Project.CurrentProject.ExchangerList.FindAll(x => x.ControlGroupID.Contains(_groupP.Id));
            foreach (var item in copySysHeatExch)
            {
                item.ControlGroupID.Add(_group.Id);
            }
            List<JCHVRF.Model.NextGen.SystemVRF> copySysVrf = Project.CurrentProject.SystemListNextGen.FindAll(x => x.ControlGroupID.Contains(_groupP.Id));
            foreach (var item in copySysVrf)
            {
                item.ControlGroupID.Add(_group.Id);
            }
            var copysystemoncanvaslist = ((ControlSystem)newSystem).SystemsOnCanvasList;
            foreach(var sys in copysystemoncanvaslist)
            {
                if(sys.System.GetType() == typeof(RoomIndoor))
                {
                    ((RoomIndoor)sys.System).ControlGroupID.Add(_group.Id);
                }
                else if (sys.System.GetType() == typeof(JCHVRF.Model.NextGen.SystemVRF))
                {
                    ((JCHVRF.Model.NextGen.SystemVRF)sys.System).ControlGroupID.Add(_group.Id);
                }                                                    
            }
        }
        public static double ValueAsPerUnit(this double Value, string ToUnit)
        {
            //if (string.Equals(FromUnit, ToUnit, StringComparison.OrdinalIgnoreCase))
            //    return Value;

            switch (ToUnit)
            {
                case Unit.ut_Capacity_ton:
                    return Value * 0.28434517;
                case Unit.ut_Capacity_btu:
                    return Value * 3412.142;

                case Unit.ut_Airflow_m3h:
                    return Value * 0.0167;
                case Unit.ut_Airflow_cfm:
                    return Value * 0.589;
                case Unit.ut_Airflow_ls:
                    return Value * 0.278;

                case Unit.ut_Area_ft2:
                    return Value * 10.764;

                case Unit.ut_Weight_lbs:
                    return Value * 2.20462262185;

                case Unit.ut_Dimension_inch:
                    return Value * 39.37007874;
                    
                case Unit.ut_Temperature_f:
                    return (Value * (9 / 5)) + 32;

                case Unit.ut_Size_ft:
                    return Value * 3.281;

                case Unit.ut_LoadIndex_MBH:
                    return Value * 0.317;
                    
                case Unit.ut_WaterFlowRate_lmin:
                    return Value * 16.667;
            }



            //switch (SelectedAirflowUnit)
            //{
            //    case AirflowUnit.ls:
            //        SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_ls;
            //        break;
            //    case AirflowUnit.m3h: //It means m3/min in legacy
            //        SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_m3h;
            //        break;
            //    case AirflowUnit.m3hr:// This is not in new application, it meant m3/h in legacy
            //        SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_m3hr;
            //        break;
            //    case AirflowUnit.cfm:
            //        SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_cfm;
            //        break;
            //}

            //switch (SelectedTemperatureUnit)
            //{
            //    case TemperatureUnit.F:
            //        SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = Unit.ut_Temperature_f;
            //        break;
            //    case TemperatureUnit.C:
            //        SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = Unit.ut_Temperature_c;
            //        break;
            //}

            //switch (SelectedLengthUnit)
            //{
            //    case LengthUnit.m:
            //        SystemSetting.UserSetting.unitsSetting.settingLENGTH = Unit.ut_Size_m;
            //        break;
            //    case LengthUnit.ft:
            //        SystemSetting.UserSetting.unitsSetting.settingLENGTH = Unit.ut_Size_ft;
            //        break;
            //}

            //switch (SelectedDimensionsUnit)
            //{
            //    case DimensionsUnit.mm:
            //        SystemSetting.UserSetting.unitsSetting.settingDimension = Unit.ut_Dimension_mm;
            //        break;
            //    case DimensionsUnit.inch:
            //        SystemSetting.UserSetting.unitsSetting.settingDimension = Unit.ut_Dimension_inch;
            //        break;
            //}

            //// This was in legacy app for Unit Dimensions and above one for Piping Dimensions
            ////if (rbDemensionsmmUnit.Checked == true)
            ////{
            ////    SystemSetting.UserSetting.unitsSetting.settingDimensionUnit = Unit.ut_Dimension_mm;
            ////}
            ////else
            ////{
            ////    SystemSetting.UserSetting.unitsSetting.settingDimensionUnit = Unit.ut_Dimension_inch;
            ////}

            //switch (SelectedWeightUnit)
            //{
            //    case WeightUnit.kg:
            //        SystemSetting.UserSetting.unitsSetting.settingWEIGHT = Unit.ut_Weight_kg;
            //        break;
            //    case WeightUnit.lbs:
            //        SystemSetting.UserSetting.unitsSetting.settingWEIGHT = Unit.ut_Weight_lbs;
            //        break;
            //}

            //switch (SelectedAreaUnit)
            //{
            //    case AreaUnit.m2:
            //        SystemSetting.UserSetting.unitsSetting.settingAREA = Unit.ut_Area_m2;
            //        break;
            //    case AreaUnit.ft2:
            //        SystemSetting.UserSetting.unitsSetting.settingAREA = Unit.ut_Area_ft2;
            //        break;
            //}

            //switch (SelectedLoadIndexUnit)
            //{
            //    case LoadIndexUnit.Wm2:
            //        SystemSetting.UserSetting.unitsSetting.settingLOADINDEX = Unit.ut_LoadIndex_w;
            //        break;
            //    case LoadIndexUnit.MBH:
            //        SystemSetting.UserSetting.unitsSetting.settingLOADINDEX = Unit.ut_LoadIndex_MBH;
            //        break;
            //}

            //switch (SelectedWaterFlowRateUnit)
            //{
            //    case WaterFlowRateUnit.m3h:
            //        SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate = Unit.ut_WaterFlowRate_m3h;
            //        break;
            //    case WaterFlowRateUnit.lmin:
            //        SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate = Unit.ut_WaterFlowRate_lmin;
            //        break;
            //}


            return Value;
        }

        public static int ExtractNumberFromEnd(this string Value)
        {
            int number = 0;

            if (!string.IsNullOrEmpty(Value))
            {
                string extractnumber = Regex.Match(Value, @"(\d+)(?!.*\d)").Value;
                if (!string.IsNullOrEmpty(extractnumber))
                {
                    return Convert.ToInt32(extractnumber);
                }
            }

            return number;
        }
    }
}
