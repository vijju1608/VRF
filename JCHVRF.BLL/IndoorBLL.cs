using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DAL;

namespace JCHVRF.BLL
{
    public class IndoorBLL
    {
        string _region;
        //string _productType; //V1.2.1之后，由于多ProductType需求，_productType不再使用
        string _brandCode;
        IndoorDAL _dal;

        //public IndoorBLL(string region, string productType, string brandCode)
        public IndoorBLL(string region, string brandCode)
        {
            _region = region;
            //_productType = productType;
            _brandCode = brandCode;
            //_dal = new IndoorDAL(region, productType, _brandCode);
            _dal = new IndoorDAL(region, _brandCode);
        }

        public IndoorBLL(string region, string mianRegion, string brandCode)
        {
            _region = region;
            //_productType = productType;
            _brandCode = brandCode;
            //_dal = new IndoorDAL(region, productType, _brandCode);
            _dal = new IndoorDAL(region, mianRegion, _brandCode);
        }
        // 20160821 多ProductType修改 by Yunxiao Lin
        public Indoor GetItem(string modelFull,string unitType, string productType, string Series)
        {
            return _dal.GetItem(modelFull, unitType, productType, Series);
        }

        public DataRow GetAirflowEsp(string modelname, string power)
        {
            return _dal.GetAirflowEsp(modelname,power);
        }
        /// 20160821 多ProductType修改 by Yunxiao Lin
        /// <summary>
        /// 仅针对有两条记录的新风机，即一对一模式与混连模式下Model相同，但Capacity不同的记录。
        /// </summary>
        /// <param name="modelFull"></param>
        /// <param name="unitType"></param>
        /// <param name="isCompositeMode"></param>
        /// <returns></returns>
        public Indoor GetFreshAirItem(string modelFull, string unitType, bool isCompositeMode, string productType, string Series)
        {
            return _dal.GetFreshAirItem(modelFull, unitType, isCompositeMode, productType, Series);
        }
        // 20160821 多ProductType修改 by Yunxiao Lin
        // 20161118 增加factoryCode参数 by Yunxiao Lin
        public DataTable GetIndoorListStd(string type, string productType, string factoryName)
        {
            return _dal.GetListStd(type, productType, factoryName);
        }

        public DataTable GetUniversalIndoorListStd(string displayName, string factoryCode, List<string> typeList)
        {
            return _dal.GetListStd(displayName, factoryCode, typeList);
        }

        // 20170726   by xyj 
        public DataTable GetExchnagerListStd(string type, string factoryName,string power)
        {
            return _dal.GetExchangerListStd(type, factoryName,power);
        }

        public string GetStdTableName()
        {
            return _dal.GetStdTableName(IndoorType.Indoor);
        }

        //20170728 获取全热交换机电源类型 by xyj
        public  DataTable GetExchangerPowerType(string type, string productType)
        {
            return _dal.GetExchangerPowerType(type, productType);
        }

        // 20160821 多ProductType修改 by Yunxiao Lin
        public DataTable GetIndoorTypeList(out string colName, string productType)
        {
            return _dal.GetTypeList(out colName, productType);
        }

        //
        public DataTable GetIndoorFacCodeList( string productType)
        {
            return _dal.GetIndoorFacCodeList(productType);
        }

        //获取通用IDU unitType别名   --add on 20171206 by Lingjia Qiu
        public DataTable GetIndoorDisplayName()
        {
            return _dal.GetIndoorDisplayName();
        }

        public DataTable GetIndoorDisplayNameForODUSeries(string ODUSeries)
        {
            return _dal.GetIndoorDisplayNameForODUSeries(ODUSeries);
        }
        public DataTable GetIduDisplayName()
        {
            return _dal.GetIduDisplayName();
        }

        //获取通用IDU 新风机别名   --add on 20171206 by Lingjia Qiu
        public string GetDisplayNameStr(string unitType,string factoryCode)
        {
            bool isFA = false;
            return _dal.GetDisplayNameStr(unitType, factoryCode, out isFA);
        }

        // 20160821 多ProductType修改 by Yunxiao Lin
        public string GetFCodeByDisUnitType(string display_Name, out List<string> typeList)
        {            
            return _dal.GetFCodeByDisUnitType(display_Name,out typeList );
        }

        //public double CalIndoorEstCapacity(Indoor inItem, double OutTemperature, double InTemperature, out double estSH, bool isHeating)
        //{
        //    return _dal.CalEstCapacity(inItem, OutTemperature, InTemperature, out estSH, isHeating);
        //}
        //室内机估算容量不再返回显热 20161111
        public double CalIndoorEstCapacity(Indoor inItem, double OutTemperature, double InTemperature,  bool isHeating)
        {
            return _dal.CalEstCapacity(inItem, OutTemperature, InTemperature, isHeating);
        }


        /// 计算 Indoor 的估算容量总和（Cooling & Heating）
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
                capCool += ri.CoolingCapacity;
                capHeat += ri.HeatingCapacity;
            }
            return capCool;
        }


        /// 计算指定室内机的实际Capacity值（经过Piping校验计算之后的最终值，已改为实际值） 20161116 by Yunxiao Lin
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
         
        public static bool IsFreshAirUnit(string unitType)
        {
            return unitType.Contains("YDCF")
                || unitType.Contains("Fresh Air")
                || unitType.Contains("Ventilation");
        }


        public static bool IsExchanger(string unitType)
        {
            return unitType.Contains("Exchanger");
        }

        public Indoor getBiggerIndoor(Indoor litem)
        {
            return _dal.getBiggerIndoor(litem);
        }

        public string GetDefaultDuctedUnitType(string region, string brand, string series)
        {
            return _dal.GetDefaultDuctedUnitType(region, brand, series);
        }


        /// <summary>
        /// 获取全热交换机电源选项 
        /// </summary>
        /// <returns></returns>
        public DataTable GetPowerSupplyList()
        {
            return _dal.GetPowerSupplyList();
        }

          /// <summary>
        /// 获取全热交换机类型
        /// </summary>
        /// <returns></returns>
        public DataTable  GetExchangerTypeList()
        {
            return _dal.GetExchangerTypeList();
        }
        public DataTable GetIndoorDisplayNameRegionWise(string _productType,string _series)
        {
            DataTable dt = _dal.GetIndoorDisplayNameRegionWise(_productType);
            string colName = "UnitType";
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToInt32(dr["FactoryCount"].ToString()) > 1)
                {
                    switch (dr["FactoryCode"].ToString())
                    {
                        case "G":
                            dr[colName] += "-GZF";
                            break;
                        case "E":
                            dr[colName] += "-HAPE";
                            break;
                        case "Q":
                            dr[colName] += "-HAPQ";
                            break;
                        case "B":
                            dr[colName] += "-HAPB";
                            break;
                        case "I":
                            dr[colName] += "-HHLI";
                            break;
                        case "M":
                            dr[colName] += "-HAPM";
                            break;
                        case "S":
                            dr[colName] += "-SMZ";
                            break;
                    }
                }
            }

            DataView dv = new DataView(dt);
            if (_productType == "Comm. Tier 2, HP")
            {
                //Comm. Tier 2, HP 有3个series, FSN6Q, FSXN, FSXN1 能够选择的室内机系列是不同的 20161201 by Yunxiao Lin
                if (_series == "Commercial VRF HP, FSN6Q" || _series == "Commercial VRF HP, JVOH-Q")
                {
                    //FSN6Q, JVOH不能使用HAPE High Static Ducted / Medium Static Ducted / Low Static Ducted
                    //FSN6Q, JVOH不能使用SMZ High Static Ducted / Medium Static Ducted / Four Way Cassette
                    dv.RowFilter = "UnitType not in ('High Static Ducted-HAPE','Medium Static Ducted-HAPE','Low Static Ducted-HAPE','High Static Ducted-SMZ','Medium Static Ducted-SMZ','Four Way Cassette-SMZ')";
                }
                else
                {
                    //FSXN, FSXN1不能使用HAPQ Four Way Cassette
                    dv.RowFilter = "UnitType <>'Four Way Cassette-HAPQ'";
                }
            }
            return dv.Table;
        }
    }
}
