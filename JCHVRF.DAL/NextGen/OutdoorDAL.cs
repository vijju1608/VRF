using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

using JCHVRF.Model;
using JCHVRF.DALFactory;
using JCBase.Utility;
using System.Collections;
using System.Linq;
using NextGenModel = JCHVRF.Model.NextGen;

namespace JCHVRF.DAL.NextGen
{
    public class OutdoorDAL : IOutdoorDAL
    {
        IDataAccessObject _dao;
        string _region;
        //string _productType; //多ProductType系统中不只有一个productType，所以该变量没用了。
        string _brandCode;
        string _mainRegion;

        //public OutdoorDAL(string region, string productType, string brandCode)
        public OutdoorDAL(string region, string brandCode)
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
            _region = region;
            //_productType = productType;
            _brandCode = brandCode;
            _mainRegion = Project.CurrentProject.RegionCode;
        }

        public OutdoorDAL(string region, string brandCode, string mianRegion)
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
            _region = region;
            //_productType = productType;
            _brandCode = brandCode;
            _mainRegion = mianRegion;
        }

        /// 根据多ProductType需求，增加productType参数 20160822 by Yunxiao Lin
        /// <summary>
        /// 获取室外机对象
        /// </summary>
        /// <param name="modelFull"></param>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public Outdoor GetItem(string modelFull, string productType)
        {
            if (string.IsNullOrEmpty(modelFull))
                return null;

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;
            //dv.RowFilter="ProductType ='" + _productType+ "' and Model='" + modelFull + "' and Deleteflag = 1";
            dv.RowFilter = "ProductType ='" + productType + "' and Model='" + modelFull + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();


            //string sql = "select * from " + sTableName + " where ProductType ='" + _productType
            ////    + "' and Model='" + modelFull + "' and Deleteflag = 1";
            //DataTable dt = _dao.GetDataTable(sql);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                return DataRowToOutdoor(dr);
            }
            return null;
        }

        /// 根据多ProductType需求，增加productType参数 20160822 by Yunxiao Lin
        /// <summary>
        /// 获取室外机对象
        /// </summary>
        /// <param name="modelFull"></param>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public Outdoor GetItemBySeries(string modelFull, string series)
        {
            if (string.IsNullOrEmpty(modelFull))
                return null;

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;
            //dv.RowFilter="ProductType ='" + _productType+ "' and Model='" + modelFull + "' and Deleteflag = 1";
            dv.RowFilter = "Series ='" + series + "' and Model='" + modelFull + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                return DataRowToOutdoor(dr);
            }
            return null;
        }

        /// <summary>
        /// 根据Factory + Region + Brand + ProductType + Series + Hitachi_Model找到相同的新型号
        /// </summary>
        /// <param name="modelFull"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        public Outdoor GetItem(string productType, string series, string modelHitachi)
        {
            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;
            string filters = "ProductType='" + productType + "' and Series ='" + series + "' and Model_Hitachi='" + modelHitachi + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                filters += " and Model_York<>'-'";
            else
                filters += " and Model_York='-'";
            dv.RowFilter = filters;
            dt = dv.ToTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                return DataRowToOutdoor(dr);
            }
            return null;
        }

        /// <summary>
        /// 将DataRow转化成Outdoor实体对象 on 20170627 by Shen Junjie
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private Outdoor DataRowToOutdoor(DataRow dr)
        {
            Outdoor item = new Outdoor();
            item.RegionCode = _region;
            //item.ProductType = _productType;
            item.ProductType = dr["ProductType"] is DBNull ? "" : dr["ProductType"].ToString();
            string modelFull = dr["Model"] is DBNull ? "" : dr["Model"].ToString();
            item.Model = modelFull.Substring(0, 7);
            string _s = item.Model.Substring(6, 1); // 若第7位不是数字则只取前6位作为ShortModel
            if (!Util.IsNumber(_s))
                item.Model = modelFull.Substring(0, 6);


            item.ModelFull = modelFull; // Model全名
            item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
            item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

            item.Type = dr["UnitType"] is DBNull ? "" : dr["UnitType"].ToString();
            item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
            item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
            item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
            item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
            item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();
            item.GasPipe_Hi = (dr["GasPipe_Hi"] is DBNull) ? "-" : dr["GasPipe_Hi"].ToString();
            item.GasPipe_Lo = (dr["GasPipe_Lo"] is DBNull) ? "-" : dr["GasPipe_Lo"].ToString();    // TODO
            item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
            item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());
            //item.CSPF = (dr["CSPF"] is DBNull) ? 0 : double.Parse(dr["CSPF"].ToString());
            item.Power_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
            item.Power_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString()); // TODO
            item.MaxCurrent = (dr["MaxCurrent"] is DBNull) ? 0 : double.Parse(dr["MaxCurrent"].ToString()); // 20140827
            item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
            item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
            item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
            item.RefrigerantCharge = (dr["RefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["RefrigerantCharge"].ToString());
            item.MaxRefrigerantCharge = (dr["MaxRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MaxRefrigerantCharge"].ToString());
            // 根据EU 提供的冷媒追加算法，需要增加ODU最小追加冷媒量 20180502 by Yunxiao Lin
            if (dr.Table.Columns.Contains("MinRefrigerantCharge"))
                item.MinRefrigerantCharge = (dr["MinRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MinRefrigerantCharge"].ToString());
            else
                item.MinRefrigerantCharge = 0;

            item.MaxIU = (dr["MaxIU"] is DBNull) ? 0 : int.Parse(dr["MaxIU"].ToString());
            item.RecommendedIU = (dr["RecommendedIU"] is DBNull) ? 0 : int.Parse(dr["RecommendedIU"].ToString());
            item.Price = 99999;

            item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
            item.FullModuleName = (dr["FullModuleName"] is DBNull) ? "" : dr["FullModuleName"].ToString();
            item.AuxModelName = (dr["AuxModelName"] is DBNull) ? "" : dr["AuxModelName"].ToString();

            item.MaxPipeLength = (dr["MaxPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLength"].ToString());
            item.MaxEqPipeLength = (dr["MaxEqPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxEqPipeLength"].ToString());
            item.MaxOutdoorAboveHeight = (dr["MaxOutdoorAboveHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorAboveHeight"].ToString());
            item.MaxOutdoorBelowHeight = (dr["MaxOutdoorBelowHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorBelowHeight"].ToString());
            item.MaxDiffIndoorHeight = (dr["MaxDiffIndoorHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorHeight"].ToString());
            item.MaxIndoorLength = (dr["MaxIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength"].ToString());
            item.MaxIndoorLength_MaxIU = (dr["MaxIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength_MaxIU"].ToString());
            item.MaxPipeLengthwithFA = (dr["MaxPipeLengthwithFA"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLengthwithFA"].ToString());
            item.MaxDiffIndoorLength = (dr["MaxDiffIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength"].ToString());
            item.MaxDiffIndoorLength_MaxIU = (dr["MaxDiffIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength_MaxIU"].ToString());

            item.JointKitModelL = (dr["JointKitModelL"] is DBNull) ? "" : dr["JointKitModelL"].ToString();
            item.JointKitModelG = (dr["JointKitModelG"] is DBNull) ? "" : dr["JointKitModelG"].ToString();
            //报告中输出
            item.MaxOperationPI_Cooling = (dr["MaxOperationPI_Cooling"] is DBNull) ? "-" : dr["MaxOperationPI_Cooling"].ToString();
            item.MaxOperationPI_Heating = (dr["MaxOperationPI_Heating"] is DBNull) ? "-" : dr["MaxOperationPI_Heating"].ToString();

            item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

            //增加Total Liquid Piping Length 和L3 add on 20160515 by Yunxiao Lin
            item.MaxTotalPipeLength = (dr["MaxTotalPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength"].ToString());
            item.MaxTotalPipeLength_MaxIU = (dr["MaxTotalPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength_MaxIU"].ToString());
            item.MaxMKIndoorPipeLength = (dr["MaxMKIndoorPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength"].ToString());
            item.MaxMKIndoorPipeLength_MaxIU = (dr["MaxMKIndoorPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength_MaxIU"].ToString());

            //增加Series列 20161031 by Yunxiao Lin
            item.Series = (dr["Series"] is DBNull) ? "" : dr["Series"].ToString().Trim();
            //匹数 add on 20161109 by Yunxiao Lin
            item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());

            // 新增能效比参数赋值 add by axj 20180504
            if(dr.Table.Columns.Contains("EER"))
            {
                item.EER = (dr["EER"] is DBNull) ? 0 : double.Parse(dr["EER"].ToString());
            }
            if (dr.Table.Columns.Contains("COP"))
            {
                item.COP = (dr["COP"] is DBNull) ? 0 : double.Parse(dr["COP"].ToString());
            }
            if (dr.Table.Columns.Contains("SEER"))
            {
                item.SEER = (dr["SEER"] is DBNull) ? 0 : double.Parse(dr["SEER"].ToString());
            }
            if (dr.Table.Columns.Contains("SCOP"))
            {
                item.SCOP = (dr["SCOP"] is DBNull) ? 0 : double.Parse(dr["SCOP"].ToString());
            }
            if (dr.Table.Columns.Contains("SoundPower"))
            {
                item.SoundPower = (dr["SoundPower"] is DBNull) ? 0 : double.Parse(dr["SoundPower"].ToString());
            }
            if (dr.Table.Columns.Contains("CSPF"))
            {
                item.CSPF = (dr["CSPF"] is DBNull) ? 0 : double.Parse(dr["CSPF"].ToString());
            }

            return item;
        }

        /// <summary>
        /// 获取室外机标准表
        /// </summary>
        /// <returns></returns>
        public DataTable GetOutdoorListStd()
        {
            string stdTableName = GetStdTableName(IndoorType.Outdoor);
            if (!string.IsNullOrEmpty(stdTableName))
            {
                //2013-10-22 by Yang 新增读取长度/高度限制字段功能, 屏蔽原SQL查询代码
                //20160515 by Yunxiao Lin 新增液管总长限制，L3限制
                //20160822 by Yunxiao Lin 多ProductType功能需要取出全部productType的数据
                //20160826 by Yunxiao Lin 增加输出productType列
                //20160826 by Lingjia Qiu 增加输出Series列
                string query = "SELECT DISTINCT Mid(Model,1,7) as Model, Model as ModelFull, Model_York, Model_Hitachi, UnitType, CoolCapacity, HeatCapacity, MaxIU, RecommendedIU, "
                    + "TypeImage, MaxPipeLength, MaxEqPipeLength,MaxOutdoorAboveHeight,MaxOutdoorBelowHeight,MaxDiffIndoorHeight, MaxIndoorLength, MaxIndoorLength_MaxIU, "
                    + "MaxPipeLengthwithFA, MaxDiffIndoorLength, MaxDiffIndoorLength_MaxIU,MaxTotalPipeLength,MaxTotalPipeLength_MaxIU,MaxMKIndoorPipeLength,MaxMKIndoorPipeLength_MaxIU,ProductType,Series "//,CSPF
                                                                                                                                                                                                                  //增加输出Horsepower列 20161112 by Yunxiao Lin
                    + ",Horsepower "
                    //添加AuxModelName列  2016/12/23 by axj
                    + ",AuxModelName "
                    //+ "From " + stdTableName + " Where  ProductType= '" + _productType
                    //+ "' and DeleteFlag=1";//Region = '" + _region + "' and
                    + "From " + stdTableName + " where DeleteFlag=1";

                if (_brandCode == "Y")
                    query += " and Model_York<>'-'";
                else
                    query += " and Model_York='-'";
                query += " Order by CoolCapacity";

                DataTable dt = _dao.GetDataTable(query);
                foreach (DataRow dr in dt.Rows)
                {
                    double coolCap = dr["CoolCapacity"] is DBNull ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                    double heatCap = dr["HeatCapacity"] is DBNull ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                    dr["CoolCapacity"] = coolCap;
                    dr["HeatCapacity"] = heatCap;
                    string model = dr["Model"].ToString();
                    string _s = model.Substring(6, 1);
                    if (!Util.IsNumber(_s))
                        dr["Model"] = model.Substring(0, 6);
                }
                return dt;
            }
            return null;
        }
        // 20160822 增加productType参数 by Yunxiao Lin
        /// <summary>
        /// 获取室外机类型list
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable GetOutdoorTypeList(out string colName, string productType)
        {
            colName = "UnitType";
            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (!string.IsNullOrEmpty(sTableName))
            {
                DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
                DataView dv = dt.DefaultView;
                //dv.RowFilter =" ProductType= '" + _productType + "'and Deleteflag = 1";
                dv.RowFilter = " ProductType= '" + productType + "'and Deleteflag = 1";
                if (_brandCode == "Y")
                    dv.RowFilter += " and Model_York<>'-'";
                else
                    dv.RowFilter += " and Model_York='-'";
                dt = dv.ToTable(true, colName);

                //dt.AsEnumerable().Select(p => p.Field<int>(colName));
                //dt.AsEnumerable.Select(d => d.Field<int>("id")).Distinct();
                return dt;
                //string query = "SELECT DISTINCT " + colName + " From " + sTableName + " Where ProductType= '" + _productType + "'and Deleteflag = 1";
                //return _dao.GetDataTable(query);
            }
            return null;
        }

        /// <summary>
        /// 通过Series获取室外机类型list
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable GetOutdoorTypeListBySeries(out string colName, string series)
        {
            colName = "UnitType";
            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (!string.IsNullOrEmpty(sTableName))
            {
                DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
                DataView dv = dt.DefaultView;
                string filter = " Series= '" + series + "' and Deleteflag = 1";
                if (sTableName.Contains("_UNIVERSAL")) //Universal std表的RegionCode数据和其他Std表不一样，使用"/"作为分隔。 20180809 by Yunxiao Lin
                    filter += " and RegionCode like '%/" + _region + "/%'";
                else
                    filter += " and RegionCode='" + _region + "'";
                if (_brandCode == "Y")
                    filter += " and Model_York<>'-'";
                else
                    filter += " and Model_York='-'";
                dv.RowFilter = filter;
                dt = dv.ToTable(true, colName);
                return dt;
            }
            return null;
        }

        ///// <summary>
        ///// 计算室外机估算容量值，查容量表计算
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="shortModel"></param>
        ///// <param name="maxRatio"></param>
        ///// <param name="OutTemperature"></param>
        ///// <param name="InTemperature"></param>
        ///// <param name="isHeating"></param>
        ///// <returns></returns>
        //public double CalOutdoorEstCapacity(string type, string shortModel, double maxRatio, double OutTemperature, double InTemperature, bool isHeating)
        //{
        //    return 0;
        //}

        //public double CalOutdoorEstCapacity(Outdoor outItem, double maxRatio, double OutTemperature, double InTemperature, bool isHeating)
        //{
        //    string model = outItem.Model;
        //    string partLoadTableName = outItem.PartLoadTableName;
        //    if (string.IsNullOrEmpty(partLoadTableName))
        //    {
        //        return 0;
        //    }

        //    List<string> Target = new List<string> { };             //插值计算输出列表，如"Capacity","Power"等
        //    List<double> Val_Interpolate = new List<double> { };    //插值计算返回列表
        //    //int tmpRatio = 0;
        //    //int x0 = 0, x1 = 0;
        //    //double y0 = 0, y1 = 0;
        //    double outEstCap = 0;

        //    // 如果Capacity Factor为10的倍数是直接二维插值，如果不是，将两个二维插值结合，再线性插值
        //    string Condition = isHeating ? "Heating" : "Cooling";

        //    int maxR = Convert.ToInt32(maxRatio * 100);
        //    if (maxR >= 50 && maxR <= 130)
        //    {
        //        Target.Clear();
        //        Target.Add("TC");
        //        Val_Interpolate = CDL.CDL.Interpolate((new GetDatabase()).GetDataAccessObject().GetConnString(), partLoadTableName, "Condition ='" + Condition
        //            + "' and ShortModel='" + model + "'", "OutDB", OutTemperature, "InWB", InTemperature, Target);
        //        if (Val_Interpolate.Count > 0)
        //            outEstCap = Val_Interpolate[0];
        //        else
        //            return 0;

        //        // 获取50% ～ 130%时的Factor
        //        double factor = Math.Round((double)maxR / 100, 2);
        //        return outEstCap * factor;

        //    }
        //    return 0;
        //}

        ///// 查容量表，计算室外机估算容量
        ///// 第2个参数由actRatio改为Horsepower 20161110 by Yunxiao Lin
        ///// <summary>
        ///// 查容量表，计算室外机估算容量
        ///// </summary>
        ///// <param name="outItem"></param>
        ///// <param name="Horsepower"></param>
        ///// <param name="OutTemperature"></param>
        ///// <param name="InTemperature"></param>
        ///// <param name="isHeating"></param>
        ///// <returns></returns>
        //public double CalOutdoorEstCapacity(Outdoor outItem, double Horsepower, double OutTemperature, double InTemperature, bool isHeating)
        //{
        //    string model = outItem.Model;
        //    string partLoadTableName = outItem.PartLoadTableName;
        //    if (string.IsNullOrEmpty(partLoadTableName))
        //    {
        //        return 0;
        //    }
        //    //直接线性插值获取室外机的Total IDU Correction 参数
        //    string Condition = isHeating ? "Heating" : "Cooling";
        //    if (Horsepower >= outItem.Horsepower * 0.5 && Horsepower <= outItem.Horsepower * 1.3)
        //    {
        //        double Total_IDU_Factor = GetTotalIDUFactor(outItem.ModelFull, Horsepower, isHeating);
        //        if (Total_IDU_Factor > 0)
        //        {
        //            //线性插值获取室外机的Temperature est. Capacity
        //            List<string> Target = new List<string> { };             //插值计算输出列表，如"Capacity","Power"等
        //            List<double> Val_Interpolate = new List<double> { };    //插值计算返回列表
        //            double outEstCap = 0;
        //            Target.Clear();
        //            Target.Add("TC");
        //            Val_Interpolate = CDL.CDL.Interpolate((new GetDatabase()).GetDataAccessObject().GetConnString(), partLoadTableName, "Condition ='" + Condition
        //                + "' and ShortModel='" + model + "'", "OutDB", OutTemperature, "InWB", InTemperature, Target);
        //            if (Val_Interpolate.Count > 0)
        //                outEstCap = Val_Interpolate[0];
        //            else
        //                return 0;
        //            return outEstCap * Total_IDU_Factor;
        //        }
        //    }

        //    //int actR = Convert.ToInt32(actRatio * 100);
        //    //if (actR >= 50 && actR <= 130)
        //    //{
        //    //    if (actR % 10 > 0)
        //    //    {
        //    //        //如果Capacity Factor不是10的倍数，凑成10的倍数 20160921 by Yunxiao Lin
        //    //        actR = ((actR / 10) + 1) * 10;
        //    //    }
        //    //    Target.Clear();
        //    //    Target.Add("TC");
        //    //    Val_Interpolate = CDL.CDL.Interpolate((new GetDatabase()).GetDataAccessObject().GetConnString(), partLoadTableName, "Condition ='" + Condition
        //    //        + "' and ShortModel='" + model + "'", "OutDB", OutTemperature, "InWB", InTemperature, Target);
        //    //    if (Val_Interpolate.Count > 0)
        //    //        outEstCap = Val_Interpolate[0];
        //    //    else
        //    //        return 0;

        //    //    // 获取50% ～ 130%时的Factor
        //    //    double factor = Math.Round((double)actR / 100, 2);
        //    //    return outEstCap * factor;

        //    //}
        //    return 0;
        //}

        /// 查容量表，计算室外机估算容量
        /// 第2个参数由actRatio改为Horsepower 20161110 by Yunxiao Lin
        /// 优化计算速度 20161130 by Yunxiao Lin
        /// 增加水流速计算 20170216 by Yunxiao Lin
        /// <summary>
        /// 查容量表，计算室外机估算容量
        /// </summary>
        /// <param name="outItem"></param>
        /// <param name="Horsepower"></param>
        /// <param name="OutTemperature"></param>
        /// <param name="InTemperature"></param>
        /// <param name="isHeating"></param>
        /// <returns></returns>
        public double CalOutdoorEstCapacity(Outdoor outItem, double Horsepower, double OutTemperature, double InTemperature, bool isHeating, SystemVRF sysItem)
        {
            string model = outItem.Model;
            string partLoadTableName = outItem.PartLoadTableName;
            if (string.IsNullOrEmpty(partLoadTableName))
            {
                return 0;
            }
            //直接线性插值获取室外机的Total IDU Correction 参数
            string Condition = isHeating ? "Heating" : "Cooling";
            double min_ratio = 0.5d;
            double max_ratio = 1.3d;
            if (outItem.Series.Contains("FSNP") || outItem.Series.Contains("FSXNP") || outItem.Series.Contains("FSNS5B") || outItem.Series.Contains("FSNS7B")) //FSNP系列Connection Ratio上限可以到150%  FSXNP FSXNPE 上线也可以达到150% on 20180419 by xyj ,巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
            {
                max_ratio = 1.5d;
            }
            Horsepower = Math.Round(Horsepower, 3);
            //因巴西的150%数据没有给出，所以暂时先屏蔽 delete by Shen Junjie on 2018/5/7
            //else if (outItem.Series.Contains("FSNS5B") || outItem.Series.Contains("FSNS7B") 
            //      || outItem.Series.Contains("FSNP5B") || outItem.Series.Contains("FSNP7B")) //巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
            //{
            //    max_ratio = 1.5d;
            //}
            if (Horsepower >= Math.Round(outItem.Horsepower * min_ratio, 3) && Horsepower <= Math.Round(outItem.Horsepower * max_ratio, 3))
            {
                double Total_IDU_Factor = GetTotalIDUFactor(outItem.ModelFull, Horsepower, isHeating);
                if (Total_IDU_Factor > 0)
                {
                    double outEstCap = 0;
                    double flowrate = 0;
                    double maxdb = OutTemperature + 5;
                    double mindb = OutTemperature - 5;
                    double maxwb = InTemperature + 5;
                    double minwb = InTemperature - 5;
                    DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(partLoadTableName);
                    DataView dv = dt.DefaultView;
                    //dv.RowFilter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
                    //WaterSource 增加FlowRateLevel 参数 20170216 by Yunxiao Lin
                    string filter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
                    if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel))
                    {
                        filter += " and FlowRateLevel='" + (int)sysItem.FlowRateLevel + "'";
                    }
                    dv.RowFilter = filter;
                    dv.Sort = "OutDB asc, InWB asc";
                    dt = dv.ToTable();
                    int db1 = -100;
                    int db2 = 100;
                    int wb1 = -100;
                    int wb2 = 100;
                    double tc1 = 0;
                    double tc2 = 0;
                    double tc3 = 0;
                    double tc4 = 0;
                    if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel) && dt.Rows.Count > 0)
                        flowrate = (dt.Rows[0]["FlowRate"] is DBNull) ? 0 : double.Parse(dt.Rows[0]["FlowRate"].ToString());
                    if (isHeating)
                    {
                        sysItem.HeatingFlowRate = flowrate;
                    }
                    else
                    {
                        sysItem.CoolingFlowRate = flowrate;
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        int db = (dr["OutDB"] is DBNull) ? 0 : int.Parse(dr["OutDB"].ToString());
                        int wb = (dr["InWB"] is DBNull) ? 0 : int.Parse(dr["InWB"].ToString());
                        double tc = (dr["TC"] is DBNull) ? 0 : double.Parse(dr["TC"].ToString());
                        if (db == OutTemperature && wb == InTemperature)
                        {
                            return tc * Total_IDU_Factor;
                        }
                        if (db <= OutTemperature && db > db1)
                            db1 = db;
                        if (db >= OutTemperature && db < db2)
                            db2 = db;
                        if (wb <= InTemperature && wb > wb1)
                            wb1 = wb;
                        if (wb >= InTemperature && wb < wb2)
                            wb2 = wb;
                        if (db == db1 && wb == wb1)
                            tc1 = tc;
                        if (db == db1 && wb == wb2)
                            tc2 = tc;
                        if (db == db2 && wb == wb1)
                            tc3 = tc;
                        if (db == db2 && wb == wb2)
                            tc4 = tc;
                    }

                    if (tc1 > 0 && tc2 > 0 && tc3 > 0 && tc4 > 0)
                    {
                        if (db1 != db2 && wb1 != wb2)
                        {
                            double tc5 = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
                            double tc6 = tc3 + ((InTemperature - wb1) * (tc4 - tc3) / (wb2 - wb1));
                            outEstCap = tc5 + ((OutTemperature - db1) * (tc6 - tc5) / (db2 - db1));
                        }
                        else if (db1 == db2 && wb1 == wb2)
                        {
                            outEstCap = tc1;
                        }
                        else if (db1 == db2)
                        {
                            outEstCap = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
                        }
                        else if (wb1 == wb2)
                        {
                            outEstCap = tc1 + ((OutTemperature - db1) * (tc3 - tc1) / (db2 - db1));
                        }
                    }
                    return outEstCap * Total_IDU_Factor;
                }
            }
            return 0;
        }

        public double CalOutdoorEstCapacityNextGen(Outdoor outItem, double Horsepower, double OutTemperature, double InTemperature, bool isHeating, NextGenModel.SystemVRF sysItem)
        {
            string model = outItem.Model;
            string partLoadTableName = outItem.PartLoadTableName;
            if (string.IsNullOrEmpty(partLoadTableName))
            {
                return 0;
            }
            OutTemperature = Math.Round(OutTemperature, 1); //na
            InTemperature = Math.Round(InTemperature, 1);//na
            //直接线性插值获取室外机的Total IDU Correction 参数
            string Condition = isHeating ? "Heating" : "Cooling";
            double min_ratio = 0.5d;
            double max_ratio = 1.3d;
            //FSNP系列Connection Ratio上限可以到150%  
            //FSXNP FSXNPE 上线也可以达到150% on 20180419 by xyj ,
            //巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
            if (outItem.Series.Contains("FSNP")
                || outItem.Series.Contains("FSXNP")
                || outItem.Series.Contains("FSNS5B")
                || outItem.Series.Contains("FSNS7B")
                || outItem.Series.Contains("FSNC5B")
                || outItem.Series.Contains("FSNC7B"))
            {
                max_ratio = 1.5d;
            }
            double systemRatio = sysItem.Ratio; //系统配比率//na
            Horsepower = Math.Round(Horsepower, 3);
            if (Horsepower < Math.Round(outItem.Horsepower * min_ratio, 3) || Horsepower > Math.Round(outItem.Horsepower * max_ratio, 3))
            {
                return 0;  //超出功率范围
            }

            double Total_IDU_Factor = 1;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(partLoadTableName);
            bool hasRatioColumn = dt.Columns.Contains("Ratio");

            if (!hasRatioColumn)
            {
                Total_IDU_Factor = GetTotalIDUFactor(outItem.ModelFull, Horsepower, isHeating);
                if (Total_IDU_Factor <= 0)
                {
                    return 0; //没有有效的Total IDu Factor
                }
            }

            double flowrate = 0;
            double setrange = 5;
            // increasing the interpolation data range for HNRQ series 20Dec2018 
            // increasing the interpolation data range for JROHQ series 24Dec2018
            if (outItem.Series.Contains("Residential VRF HP, HNRQ") || outItem.Series.Contains("Residential VRF HP, JROHQ"))
            {
                setrange = 30;
            }
            double maxdb = OutTemperature + setrange;
            double mindb = OutTemperature - setrange;
            double maxwb = InTemperature + setrange;
            double minwb = InTemperature - setrange;
            DataView dv = dt.DefaultView;
            //dv.RowFilter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
            //WaterSource 增加FlowRateLevel 参数 20170216 by Yunxiao Lin
            string filter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
            if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel))
            {
                filter += " and FlowRateLevel='" + (int)sysItem.FlowRateLevel + "'";
            }
            dv.RowFilter = filter;
            dv.Sort = "OutDB asc, InWB asc";
            dt = dv.ToTable();
            if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel) && dt.Rows.Count > 0)
                flowrate = (dt.Rows[0]["FlowRate"] is DBNull) ? 0 : double.Parse(dt.Rows[0]["FlowRate"].ToString());
            if (isHeating)
            {
                sysItem.HeatingFlowRate = flowrate;
            }
            else
            {
                sysItem.CoolingFlowRate = flowrate;
            }

            List<ODUPartload> list = DataTableToODUPartloadList(dt);

            ODUPartload[] partloads = new ODUPartload[8];

            int? db1 = null;     // db1 <= OutTemperature
            int? db2 = null;      // db2 >= OutTemperature
            int? wb1 = null;     // wb1 <= InTemperature
            int? wb2 = null;      // wb2 >= InTemperature
            double? ratio1 = null;  // ratio1 <= systemRatio
            double? ratio2 = null;  // ratio2 >= systemRatio

            for (int i = 0; i < list.Count; i++)
            {
                ODUPartload item = list[i];

                if (item.OutDB == OutTemperature && item.InWB == InTemperature)
                {
                    if (hasRatioColumn)
                    {
                        if (item.Ratio == systemRatio)
                        {
                            return item.TC;//DB WB Ratio都相符，则直接返回TC
                        }
                    }
                    else
                    {
                        return item.TC * Total_IDU_Factor;
                    }
                }

                //找到8个相邻的值
                for (int j = 0; j < 8; j++)
                {
                    //0. DB <= OutTemperature, WB <= InTemperature, Ratio <= systemRatio
                    //1. DB >= OutTemperature, WB <= InTemperature, Ratio <= systemRatio
                    //2. DB <= OutTemperature, WB >= InTemperature, Ratio <= systemRatio
                    //3. DB >= OutTemperature, WB >= InTemperature, Ratio <= systemRatio
                    //4. DB <= OutTemperature, WB <= InTemperature, Ratio >= systemRatio
                    //5. DB >= OutTemperature, WB <= InTemperature, Ratio >= systemRatio
                    //6. DB <= OutTemperature, WB >= InTemperature, Ratio >= systemRatio
                    //7. DB >= OutTemperature, WB >= InTemperature, Ratio >= systemRatio
                    if (hasRatioColumn)
                    {
                        if (j == 0 || j == 1 || j == 2 || j == 3)  //Ratio <= systemRatio
                        {
                            if (item.Ratio > systemRatio || (ratio1.HasValue && item.Ratio < ratio1)) continue;
                            if (!ratio1.HasValue || item.Ratio > ratio1)
                            {
                                //找到更靠近systemRatio的Ratio
                                ratio1 = item.Ratio;
                                partloads[0] = null;
                                partloads[1] = null;
                                partloads[2] = null;
                                partloads[3] = null;
                            }
                        }
                        else  //Ratio >= systemRatio
                        {
                            if (item.Ratio < systemRatio || (ratio2.HasValue && item.Ratio > ratio2)) continue;
                            if (!ratio2.HasValue || item.Ratio < ratio2)
                            {
                                //找到更靠近systemRatio的Ratio
                                ratio2 = item.Ratio;
                                partloads[4] = null;
                                partloads[5] = null;
                                partloads[6] = null;
                                partloads[7] = null;
                            }
                        }
                    }

                    if (j == 0 || j == 2 || j == 4 || j == 6)  //DB <= OutTemperature
                    {
                        if (item.OutDB > OutTemperature || (db1.HasValue && item.OutDB < db1)) continue;
                        if (!db1.HasValue || item.OutDB > db1)
                        {
                            //找到更靠近OutTemperature的OutDB
                            db1 = item.OutDB;
                            partloads[0] = null;
                            partloads[2] = null;
                            partloads[4] = null;
                            partloads[6] = null;
                        }
                    }
                    else  //DB >= OutTemperature
                    {
                        if (item.OutDB < OutTemperature || (db2.HasValue && item.OutDB > db2)) continue;
                        if (!db2.HasValue || item.OutDB < db2)
                        {
                            //找到更靠近OutTemperature的OutDB
                            db2 = item.OutDB;
                            partloads[1] = null;
                            partloads[3] = null;
                            partloads[5] = null;
                            partloads[7] = null;
                        }
                    }

                    if (j == 0 || j == 1 || j == 4 || j == 5)  //WB <= InTemperature
                    {
                        if (item.InWB > InTemperature || (wb1.HasValue && item.InWB < wb1)) continue;
                        if (!wb1.HasValue || item.InWB > wb1)
                        {
                            //找到更靠近InTemperature的InWB
                            wb1 = item.InWB;
                            partloads[0] = null;
                            partloads[1] = null;
                            partloads[4] = null;
                            partloads[5] = null;
                        }
                    }
                    else  //WB >= InTemperature
                    {
                        if (item.InWB < InTemperature || (wb2.HasValue && item.InWB > wb2)) continue;
                        if (!wb2.HasValue || item.InWB < wb2)
                        {
                            //找到更靠近InTemperature的InWB
                            wb2 = item.InWB;
                            partloads[2] = null;
                            partloads[3] = null;
                            partloads[6] = null;
                            partloads[7] = null;
                        }
                    }

                    partloads[j] = item;
                }
            }

            if (partloads[0] == null || partloads[1] == null || partloads[2] == null || partloads[3] == null
                || partloads[4] == null || partloads[5] == null || partloads[6] == null || partloads[7] == null
                || partloads[0].TC <= 0 || partloads[1].TC <= 0 || partloads[2].TC <= 0 || partloads[3].TC <= 0
                || partloads[4].TC <= 0 || partloads[5].TC <= 0 || partloads[6].TC <= 0 || partloads[7].TC <= 0)
            {
                return 0;  //数据不完整
            }

            double tc1 = 0;     // DB == OutTemperature, WB <= InTemperature, Ratio <= systemRatio
            double tc2 = 0;     // DB == OutTemperature, WB >= InTemperature, Ratio <= systemRatio
            double tc3 = 0;     // DB == OutTemperature, WB <= InTemperature, Ratio >= systemRatio
            double tc4 = 0;     // DB == OutTemperature, WB >= InTemperature, Ratio >= systemRatio
            double tc5 = 0;     // DB == OutTemperature, WB == InTemperature, Ratio <= systemRatio
            double tc6 = 0;     // DB == OutTemperature, WB == InTemperature, Ratio >= systemRatio
            double tc7 = 0;     // DB == OutTemperature, WB == InTemperature, Ratio == systemRatio  final result

            //插值计算
            tc1 = partloads[0].TC;
            tc2 = partloads[2].TC;
            tc3 = partloads[4].TC;
            tc4 = partloads[6].TC;

            if (partloads[0].OutDB != partloads[1].OutDB)
            {
                tc1 = partloads[0].TC + ((OutTemperature - partloads[0].OutDB) * (partloads[1].TC - partloads[0].TC) / (partloads[1].OutDB - partloads[0].OutDB));
            }
            if (partloads[2].OutDB != partloads[3].OutDB)
            {
                tc2 = partloads[2].TC + ((OutTemperature - partloads[2].OutDB) * (partloads[3].TC - partloads[2].TC) / (partloads[3].OutDB - partloads[2].OutDB));
            }
            if (partloads[4].OutDB != partloads[5].OutDB)
            {
                tc3 = partloads[4].TC + ((OutTemperature - partloads[4].OutDB) * (partloads[5].TC - partloads[4].TC) / (partloads[5].OutDB - partloads[4].OutDB));
            }
            if (partloads[6].OutDB != partloads[7].OutDB)
            {
                tc4 = partloads[6].TC + ((OutTemperature - partloads[6].OutDB) * (partloads[7].TC - partloads[6].TC) / (partloads[7].OutDB - partloads[6].OutDB));
            }
            if (partloads[0].InWB != partloads[2].InWB)
            {
                tc5 = tc1 + ((InTemperature - partloads[0].InWB) * (tc2 - tc1) / (partloads[2].InWB - partloads[0].InWB));
            }
            else
            {
                tc5 = tc1;
            }
            if (partloads[4].InWB != partloads[6].InWB)
            {
                tc6 = tc3 + ((InTemperature - partloads[4].InWB) * (tc4 - tc3) / (partloads[6].InWB - partloads[4].InWB));
            }
            else
            {
                tc6 = tc3;
            }

            if (hasRatioColumn)
            {
                if (partloads[0].Ratio != partloads[4].Ratio)
                {
                    tc7 = tc5 + ((systemRatio - partloads[0].Ratio) * (tc6 - tc5) / (partloads[4].Ratio - partloads[0].Ratio));
                }
                else
                {
                    tc7 = tc5;
                }
            }
            else
            {
                tc7 = tc5 * Total_IDU_Factor;
            }

            return tc7;
        }
        //public double CalOutdoorEstCapacityNextGenBeforeversion472(Outdoor outItem, double Horsepower, double OutTemperature, double InTemperature, bool isHeating, NextGenModel.SystemVRF sysItem)
        //{
        //    string model = outItem.Model;
        //    string partLoadTableName = outItem.PartLoadTableName;
        //    if (string.IsNullOrEmpty(partLoadTableName))
        //    {
        //        return 0;
        //    }
        //    //直接线性插值获取室外机的Total IDU Correction 参数
        //    string Condition = isHeating ? "Heating" : "Cooling";
        //    double min_ratio = 0.5d;
        //    double max_ratio = 1.3d;
        //    //FSNP系列Connection Ratio上限可以到150%  
        //    //FSXNP FSXNPE 上线也可以达到150% on 20180419 by xyj ,
        //    //巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
        //    if (outItem.Series.Contains("FSNP")
        //        || outItem.Series.Contains("FSXNP")
        //        || outItem.Series.Contains("FSNS5B")
        //        || outItem.Series.Contains("FSNS7B")
        //        || outItem.Series.Contains("FSNC5B")
        //        || outItem.Series.Contains("FSNC7B"))
        //    {
        //        max_ratio = 1.5d;
        //    }
        //    Horsepower = Math.Round(Horsepower, 3);
        //    //因巴西的150%数据没有给出，所以暂时先屏蔽 delete by Shen Junjie on 2018/5/7
        //    //else if (outItem.Series.Contains("FSNS5B") || outItem.Series.Contains("FSNS7B") 
        //    //      || outItem.Series.Contains("FSNP5B") || outItem.Series.Contains("FSNP7B")) //巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
        //    //{
        //    //    max_ratio = 1.5d;
        //    //}
        //    if (Horsepower >= Math.Round(outItem.Horsepower * min_ratio, 3) && Horsepower <= Math.Round(outItem.Horsepower * max_ratio, 3))
        //    {
        //        double Total_IDU_Factor = GetTotalIDUFactor(outItem.ModelFull, Horsepower, isHeating);
        //        if (Total_IDU_Factor > 0)
        //        {
        //            double outEstCap = 0;
        //            double flowrate = 0;
        //            // increasing the interpolation data range for HNRQ series 20Dec2018 
        //            // increasing the interpolation data range for JROHQ series 24Dec2018
        //            //Start: Shweta- 'Outdoor unit capacity correction is out of range' for Residential VRF HP, JROHQ handled as per legacy
        //            if (outItem.Series.Contains("Residential VRF HP, HNRQ") || outItem.Series.Contains("Residential VRF HP, JROHQ"))
        //            {
        //                outEstCap = 30;
        //            }
        //            //End: Shweta- 'Outdoor unit capacity correction is out of range' for Residential VRF HP, JROHQ handled as per legacy
        //            double maxdb = OutTemperature + 5;
        //            double mindb = OutTemperature - 5;
        //            double maxwb = InTemperature + 5;
        //            double minwb = InTemperature - 5;
        //            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(partLoadTableName);
        //            DataView dv = dt.DefaultView;
        //            //dv.RowFilter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
        //            //WaterSource 增加FlowRateLevel 参数 20170216 by Yunxiao Lin
        //            string filter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
        //            if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel))
        //            {
        //                filter += " and FlowRateLevel='" + (int)sysItem.FlowRateLevel + "'";
        //            }
        //            dv.RowFilter = filter;
        //            dv.Sort = "OutDB asc, InWB asc";
        //            dt = dv.ToTable();
        //            int db1 = -100;
        //            int db2 = 100;
        //            int wb1 = -100;
        //            int wb2 = 100;
        //            double tc1 = 0;
        //            double tc2 = 0;
        //            double tc3 = 0;
        //            double tc4 = 0;
        //            if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel) && dt.Rows.Count > 0)
        //                flowrate = (dt.Rows[0]["FlowRate"] is DBNull) ? 0 : double.Parse(dt.Rows[0]["FlowRate"].ToString());
        //            if (isHeating)
        //            {
        //                sysItem.HeatingFlowRate = flowrate;
        //            }
        //            else
        //            {
        //                sysItem.CoolingFlowRate = flowrate;
        //            }

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                DataRow dr = dt.Rows[i];
        //                int db = (dr["OutDB"] is DBNull) ? 0 : int.Parse(dr["OutDB"].ToString());
        //                int wb = (dr["InWB"] is DBNull) ? 0 : int.Parse(dr["InWB"].ToString());
        //                double tc = (dr["TC"] is DBNull) ? 0 : double.Parse(dr["TC"].ToString());
        //                if (db == OutTemperature && wb == InTemperature)
        //                {
        //                    return tc * Total_IDU_Factor;
        //                }
        //                if (db <= OutTemperature && db > db1)
        //                    db1 = db;
        //                if (db >= OutTemperature && db < db2)
        //                    db2 = db;
        //                if (wb <= InTemperature && wb > wb1)
        //                    wb1 = wb;
        //                if (wb >= InTemperature && wb < wb2)
        //                    wb2 = wb;
        //                if (db == db1 && wb == wb1)
        //                    tc1 = tc;
        //                if (db == db1 && wb == wb2)
        //                    tc2 = tc;
        //                if (db == db2 && wb == wb1)
        //                    tc3 = tc;
        //                if (db == db2 && wb == wb2)
        //                    tc4 = tc;
        //            }

        //            if (tc1 > 0 && tc2 > 0 && tc3 > 0 && tc4 > 0)
        //            {
        //                if (db1 != db2 && wb1 != wb2)
        //                {
        //                    double tc5 = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
        //                    double tc6 = tc3 + ((InTemperature - wb1) * (tc4 - tc3) / (wb2 - wb1));
        //                    outEstCap = tc5 + ((OutTemperature - db1) * (tc6 - tc5) / (db2 - db1));
        //                }
        //                else if (db1 == db2 && wb1 == wb2)
        //                {
        //                    outEstCap = tc1;
        //                }
        //                else if (db1 == db2)
        //                {
        //                    outEstCap = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
        //                }
        //                else if (wb1 == wb2)
        //                {
        //                    outEstCap = tc1 + ((OutTemperature - db1) * (tc3 - tc1) / (db2 - db1));
        //                }
        //            }
        //            return outEstCap * Total_IDU_Factor;
        //        }
        //    }
        //    return 0;
        //}

        private List<ODUPartload> DataTableToODUPartloadList(DataTable dt)
        {
            List<ODUPartload> list = new List<ODUPartload>();
            bool hasPI = dt.Columns.Contains("PI");
            bool hasRatio = dt.Columns.Contains("Ratio");
            bool hasFlowRate = dt.Columns.Contains("FlowRate");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                ODUPartload model = new ODUPartload();
                model.Condition = (dr["Condition"] is DBNull) ? "" : dr["Condition"].ToString();
                model.ShortModel = (dr["ShortModel"] is DBNull) ? "" : dr["ShortModel"].ToString();
                model.OutDB = (dr["OutDB"] is DBNull) ? 0 : int.Parse(dr["OutDB"].ToString());
                model.InWB = (dr["InWB"] is DBNull) ? 0 : int.Parse(dr["InWB"].ToString());
                model.TC = (dr["TC"] is DBNull) ? 0 : double.Parse(dr["TC"].ToString());
                if (hasPI) model.PI = (dr["PI"] is DBNull) ? 0 : double.Parse(dr["PI"].ToString());
                if (hasRatio) model.Ratio = (dr["Ratio"] is DBNull) ? 0 : double.Parse(dr["Ratio"].ToString());
                if (hasFlowRate) model.FlowRate = (dt.Rows[0]["FlowRate"] is DBNull) ? 0 : double.Parse(dt.Rows[0]["FlowRate"].ToString());
                list.Add(model);
            }
            return list;
        }
        //public double CalOutdoorEstCapacityNextGen(Outdoor outItem, double Horsepower, double OutTemperature, double InTemperature, bool isHeating, NextGenModel.SystemVRF sysItem)
        //{
        //    string model = outItem.Model;
        //    string partLoadTableName = outItem.PartLoadTableName;
        //    if (string.IsNullOrEmpty(partLoadTableName))
        //    {
        //        return 0;
        //    }
        //    //直接线性插值获取室外机的Total IDU Correction 参数
        //    string Condition = isHeating ? "Heating" : "Cooling";
        //    double min_ratio = 0.5d;
        //    double max_ratio = 1.3d;
        //    if (outItem.Series.Contains("FSNP") || outItem.Series.Contains("FSXNP") || outItem.Series.Contains("FSNS5B") || outItem.Series.Contains("FSNS7B")) //FSNP系列Connection Ratio上限可以到150%  FSXNP FSXNPE 上线也可以达到150% on 20180419 by xyj ,巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
        //    {
        //        max_ratio = 1.5d;
        //    }
        //    Horsepower = Math.Round(Horsepower, 3);
        //    //因巴西的150%数据没有给出，所以暂时先屏蔽 delete by Shen Junjie on 2018/5/7
        //    //else if (outItem.Series.Contains("FSNS5B") || outItem.Series.Contains("FSNS7B") 
        //    //      || outItem.Series.Contains("FSNP5B") || outItem.Series.Contains("FSNP7B")) //巴西Connection Ratio上限可以到150% add by Shen Junjie on 2018/4/18
        //    //{
        //    //    max_ratio = 1.5d;
        //    //}
        //    if (Horsepower >= Math.Round(outItem.Horsepower * min_ratio, 3) && Horsepower <= Math.Round(outItem.Horsepower * max_ratio, 3))
        //    {
        //        double Total_IDU_Factor = GetTotalIDUFactor(outItem.ModelFull, Horsepower, isHeating);
        //        if (Total_IDU_Factor > 0)
        //        {
        //            double outEstCap = 0;
        //            double flowrate = 0;
        //            double maxdb = OutTemperature + 5;
        //            double mindb = OutTemperature - 5;
        //            double maxwb = InTemperature + 5;
        //            double minwb = InTemperature - 5;
        //            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(partLoadTableName);
        //            DataView dv = dt.DefaultView;
        //            //dv.RowFilter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
        //            //WaterSource 增加FlowRateLevel 参数 20170216 by Yunxiao Lin
        //            string filter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
        //            if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel))
        //            {
        //                filter += " and FlowRateLevel='" + (int)sysItem.FlowRateLevel + "'";
        //            }
        //            dv.RowFilter = filter;
        //            dv.Sort = "OutDB asc, InWB asc";
        //            dt = dv.ToTable();
        //            int db1 = -100;
        //            int db2 = 100;
        //            int wb1 = -100;
        //            int wb2 = 100;
        //            double tc1 = 0;
        //            double tc2 = 0;
        //            double tc3 = 0;
        //            double tc4 = 0;
        //            if (!FlowRateLevels.NA.Equals(sysItem.FlowRateLevel) && dt.Rows.Count > 0)
        //                flowrate = (dt.Rows[0]["FlowRate"] is DBNull) ? 0 : double.Parse(dt.Rows[0]["FlowRate"].ToString());
        //            if (isHeating)
        //            {
        //                sysItem.HeatingFlowRate = flowrate;
        //            }
        //            else
        //            {
        //                sysItem.CoolingFlowRate = flowrate;
        //            }

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                DataRow dr = dt.Rows[i];
        //                int db = (dr["OutDB"] is DBNull) ? 0 : int.Parse(dr["OutDB"].ToString());
        //                int wb = (dr["InWB"] is DBNull) ? 0 : int.Parse(dr["InWB"].ToString());
        //                double tc = (dr["TC"] is DBNull) ? 0 : double.Parse(dr["TC"].ToString());
        //                if (db == OutTemperature && wb == InTemperature)
        //                {
        //                    return tc * Total_IDU_Factor;
        //                }
        //                if (db <= OutTemperature && db > db1)
        //                    db1 = db;
        //                if (db >= OutTemperature && db < db2)
        //                    db2 = db;
        //                if (wb <= InTemperature && wb > wb1)
        //                    wb1 = wb;
        //                if (wb >= InTemperature && wb < wb2)
        //                    wb2 = wb;
        //                if (db == db1 && wb == wb1)
        //                    tc1 = tc;
        //                if (db == db1 && wb == wb2)
        //                    tc2 = tc;
        //                if (db == db2 && wb == wb1)
        //                    tc3 = tc;
        //                if (db == db2 && wb == wb2)
        //                    tc4 = tc;
        //            }

        //            if (tc1 > 0 && tc2 > 0 && tc3 > 0 && tc4 > 0)
        //            {
        //                if (db1 != db2 && wb1 != wb2)
        //                {
        //                    double tc5 = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
        //                    double tc6 = tc3 + ((InTemperature - wb1) * (tc4 - tc3) / (wb2 - wb1));
        //                    outEstCap = tc5 + ((OutTemperature - db1) * (tc6 - tc5) / (db2 - db1));
        //                }
        //                else if (db1 == db2 && wb1 == wb2)
        //                {
        //                    outEstCap = tc1;
        //                }
        //                else if (db1 == db2)
        //                {
        //                    outEstCap = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
        //                }
        //                else if (wb1 == wb2)
        //                {
        //                    outEstCap = tc1 + ((OutTemperature - db1) * (tc3 - tc1) / (db2 - db1));
        //                }
        //            }
        //            return outEstCap * Total_IDU_Factor;
        //        }
        //    }
        //    return 0;
        //}

        /// 获取VRF数据库中标准表关系表【dbo_Relationship_Std】中的某一【标准表表名】
        /// <summary>
        /// 获取VRF数据库中标准表关系表【dbo_Relationship_Std】中的某一【标准表表名】
        /// </summary>
        /// <param name="flag">IndoorType：Indoor|FreshAir|Outdoor</param>
        /// <returns></returns>
        private string GetStdTableName(IndoorType flag)
        {
            if (flag == IndoorType.Outdoor && !string.IsNullOrEmpty(_mainRegion) && (_mainRegion == "EU_W" || _mainRegion == "EU_E" || _mainRegion == "EU_S" || _region == "TW"))
                return "ODU_Std_UNIVERSAL";

            //OleDbParameter[] paras = {
            //                             new OleDbParameter("@sType",DbType.String), 
            //                             new OleDbParameter("@sRegion",DbType.String)
            //                             //,new OleDbParameter("@sProductType",DbType.String)
            //                         };
            //paras[0].Value = flag.ToString();
            //paras[1].Value = _region;
            //paras[2].Value = _productType;

            // 注意：
            // Indoor与FreshAir表对应列名均为“IndoorTableName”;Outdoor表对应的列名为“OutdoorTableName”

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Relationship_Std");
            DataView dv = dt.DefaultView;
            dv.RowFilter = " SelectionType = '" + flag + "' and RegionCode = '" + _region + "' and Deleteflag = 1";
            dt = dv.ToTable(true, "TableName");

            //string colName = "TableName";
            //string sql = "SELECT DISTINCT " + colName + " as TableName From JCHVRF_Relationship_Std "
            //+ "Where SelectionType = @sType and RegionCode = @sRegion and Deleteflag = 1"; //and ProductType = @sProductType 

            //DataTable dt = _dao.GetDataTable(sql, paras);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["TableName"].ToString();
            }
            return "";

        }
        //根据Model_York和系列名称得到室外机对象 20170406 by Yunxiao Lin
        public Outdoor GetYorkItemBySeries(string Model_York, string series)
        {
            if (string.IsNullOrEmpty(Model_York))
                return null;
            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;
            dv.RowFilter = "Series ='" + series.Trim() + "' and Model_York='" + Model_York.Trim() + "' and Deleteflag = 1";
            dt = dv.ToTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                Outdoor item = new Outdoor();
                DataRow dr = dt.Rows[0];
                item.ModelFull = dr["Model"].ToString(); //Model全名
                item.RegionCode = _region;
                item.ProductType = dr["ProductType"] is DBNull? "":dr["ProductType"].ToString();
                item.Model = item.ModelFull.Substring(0, 7);
                string _s = item.Model.Substring(6, 1); // 若第7位不是数字则只取前6位作为ShortModel
                if (!Util.IsNumber(_s))
                    item.Model = item.ModelFull.Substring(0, 6);

                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                item.Type = dr["UnitType"] is DBNull ? "" : dr["UnitType"].ToString();
                item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();
                item.GasPipe_Hi = (dr["GasPipe_Hi"] is DBNull) ? "-" : dr["GasPipe_Hi"].ToString();
                item.GasPipe_Lo = (dr["GasPipe_Lo"] is DBNull) ? "-" : dr["GasPipe_Lo"].ToString();    // TODO
                item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());

                item.Power_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                item.Power_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString()); // TODO
                item.MaxCurrent = (dr["MaxCurrent"] is DBNull) ? 0 : double.Parse(dr["MaxCurrent"].ToString()); // 20140827
                item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                item.RefrigerantCharge = (dr["RefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["RefrigerantCharge"].ToString());
                item.MaxRefrigerantCharge = (dr["MaxRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MaxRefrigerantCharge"].ToString());
                // 根据EU 提供的冷媒追加算法，需要增加ODU最小追加冷媒量 20180502 by Yunxiao Lin
                if (dr.Table.Columns.Contains("MinRefrigerantCharge"))
                    item.MinRefrigerantCharge = (dr["MinRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MinRefrigerantCharge"].ToString());
                else
                    item.MinRefrigerantCharge = 0;

                item.MaxIU = (dr["MaxIU"] is DBNull) ? 0 : int.Parse(dr["MaxIU"].ToString());
                item.RecommendedIU = (dr["RecommendedIU"] is DBNull) ? 0 : int.Parse(dr["RecommendedIU"].ToString());
                item.Price = 99999;

                item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                item.FullModuleName = (dr["FullModuleName"] is DBNull) ? "" : dr["FullModuleName"].ToString();
                item.AuxModelName = (dr["AuxModelName"] is DBNull) ? "" : dr["AuxModelName"].ToString();

                item.MaxPipeLength = (dr["MaxPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLength"].ToString());
                item.MaxEqPipeLength = (dr["MaxEqPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxEqPipeLength"].ToString());
                item.MaxOutdoorAboveHeight = (dr["MaxOutdoorAboveHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorAboveHeight"].ToString());
                item.MaxOutdoorBelowHeight = (dr["MaxOutdoorBelowHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorBelowHeight"].ToString());
                item.MaxDiffIndoorHeight = (dr["MaxDiffIndoorHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorHeight"].ToString());
                item.MaxIndoorLength = (dr["MaxIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength"].ToString());
                item.MaxIndoorLength_MaxIU = (dr["MaxIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength_MaxIU"].ToString());
                item.MaxPipeLengthwithFA = (dr["MaxPipeLengthwithFA"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLengthwithFA"].ToString());
                item.MaxDiffIndoorLength = (dr["MaxDiffIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength"].ToString());
                item.MaxDiffIndoorLength_MaxIU = (dr["MaxDiffIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength_MaxIU"].ToString());

                item.JointKitModelL = (dr["JointKitModelL"] is DBNull) ? "" : dr["JointKitModelL"].ToString();
                item.JointKitModelG = (dr["JointKitModelG"] is DBNull) ? "" : dr["JointKitModelG"].ToString();
                //报告中输出
                item.MaxOperationPI_Cooling = (dr["MaxOperationPI_Cooling"] is DBNull) ? "-" : dr["MaxOperationPI_Cooling"].ToString();
                item.MaxOperationPI_Heating = (dr["MaxOperationPI_Heating"] is DBNull) ? "-" : dr["MaxOperationPI_Heating"].ToString();

                item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

                //增加Total Liquid Piping Length 和L3 add on 20160515 by Yunxiao Lin
                item.MaxTotalPipeLength = (dr["MaxTotalPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength"].ToString());
                item.MaxTotalPipeLength_MaxIU = (dr["MaxTotalPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength_MaxIU"].ToString());
                item.MaxMKIndoorPipeLength = (dr["MaxMKIndoorPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength"].ToString());
                item.MaxMKIndoorPipeLength_MaxIU = (dr["MaxMKIndoorPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength_MaxIU"].ToString());

                //增加Series列 20161031 by Yunxiao Lin
                item.Series = series;
                //匹数 add on 20161109 by Yunxiao Lin
                item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                return item;
            }
            return null;
        }
        // 增加productType参数 20160822 by Yunxiao Lin
        public Outdoor GetYorkItem(string Model_York, string productType)
        {
            if (string.IsNullOrEmpty(Model_York))
                return null;

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;

            //dv.RowFilter = "ProductType ='" + _productType.Trim() + "' and Model_York='" + Model_York.Trim() + "' and Deleteflag = 1";
            dv.RowFilter = "ProductType ='" + productType.Trim() + "' and Model_York='" + Model_York.Trim() + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                Outdoor item = new Outdoor();
                DataRow dr = dt.Rows[0];
                item.ModelFull = dr["Model"].ToString(); //Model全名
                item.RegionCode = _region;
                //item.ProductType = _productType;
                item.ProductType = productType;
                item.Model = item.ModelFull.Substring(0, 7);
                string _s = item.Model.Substring(6, 1); // 若第7位不是数字则只取前6位作为ShortModel
                if (!Util.IsNumber(_s))
                    item.Model = item.ModelFull.Substring(0, 6);

                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                item.Type = dr["UnitType"] is DBNull ? "" : dr["UnitType"].ToString();
                item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();
                item.GasPipe_Hi = (dr["GasPipe_Hi"] is DBNull) ? "-" : dr["GasPipe_Hi"].ToString();
                item.GasPipe_Lo = (dr["GasPipe_Lo"] is DBNull) ? "-" : dr["GasPipe_Lo"].ToString();    // TODO
                item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());

                item.Power_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                item.Power_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString()); // TODO
                item.MaxCurrent = (dr["MaxCurrent"] is DBNull) ? 0 : double.Parse(dr["MaxCurrent"].ToString()); // 20140827
                item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                item.RefrigerantCharge = (dr["RefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["RefrigerantCharge"].ToString());
                item.MaxRefrigerantCharge = (dr["MaxRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MaxRefrigerantCharge"].ToString());
                // 根据EU 提供的冷媒追加算法，需要增加ODU最小追加冷媒量 20180502 by Yunxiao Lin
                if (dr.Table.Columns.Contains("MinRefrigerantCharge"))
                    item.MinRefrigerantCharge = (dr["MinRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MinRefrigerantCharge"].ToString());
                else
                    item.MinRefrigerantCharge = 0;

                item.MaxIU = (dr["MaxIU"] is DBNull) ? 0 : int.Parse(dr["MaxIU"].ToString());
                item.RecommendedIU = (dr["RecommendedIU"] is DBNull) ? 0 : int.Parse(dr["RecommendedIU"].ToString());
                item.Price = 99999;

                item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                item.FullModuleName = (dr["FullModuleName"] is DBNull) ? "" : dr["FullModuleName"].ToString();
                item.AuxModelName = (dr["AuxModelName"] is DBNull) ? "" : dr["AuxModelName"].ToString();

                item.MaxPipeLength = (dr["MaxPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLength"].ToString());
                item.MaxEqPipeLength = (dr["MaxEqPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxEqPipeLength"].ToString());
                item.MaxOutdoorAboveHeight = (dr["MaxOutdoorAboveHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorAboveHeight"].ToString());
                item.MaxOutdoorBelowHeight = (dr["MaxOutdoorBelowHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorBelowHeight"].ToString());
                item.MaxDiffIndoorHeight = (dr["MaxDiffIndoorHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorHeight"].ToString());
                item.MaxIndoorLength = (dr["MaxIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength"].ToString());
                item.MaxIndoorLength_MaxIU = (dr["MaxIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength_MaxIU"].ToString());
                item.MaxPipeLengthwithFA = (dr["MaxPipeLengthwithFA"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLengthwithFA"].ToString());
                item.MaxDiffIndoorLength = (dr["MaxDiffIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength"].ToString());
                item.MaxDiffIndoorLength_MaxIU = (dr["MaxDiffIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength_MaxIU"].ToString());

                item.JointKitModelL = (dr["JointKitModelL"] is DBNull) ? "" : dr["JointKitModelL"].ToString();
                item.JointKitModelG = (dr["JointKitModelG"] is DBNull) ? "" : dr["JointKitModelG"].ToString();
                //报告中输出
                item.MaxOperationPI_Cooling = (dr["MaxOperationPI_Cooling"] is DBNull) ? "-" : dr["MaxOperationPI_Cooling"].ToString();
                item.MaxOperationPI_Heating = (dr["MaxOperationPI_Heating"] is DBNull) ? "-" : dr["MaxOperationPI_Heating"].ToString();

                item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

                //增加Total Liquid Piping Length 和L3 add on 20160515 by Yunxiao Lin
                item.MaxTotalPipeLength = (dr["MaxTotalPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength"].ToString());
                item.MaxTotalPipeLength_MaxIU = (dr["MaxTotalPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength_MaxIU"].ToString());
                item.MaxMKIndoorPipeLength = (dr["MaxMKIndoorPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength"].ToString());
                item.MaxMKIndoorPipeLength_MaxIU = (dr["MaxMKIndoorPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength_MaxIU"].ToString());

                //增加Series列 20161031 by Yunxiao Lin
                item.Series = (dr["Series"] is DBNull) ? "" : dr["Series"].ToString().Trim();
                //匹数 add on 20161109 by Yunxiao Lin
                item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                return item;
            }
            return null;
        }

        /// 根据Model_Hitachi获取室外机对象 add on 20160521 by Yunxiao Lin
        /// 根据多ProductType的需求，增加productType参数 20160822 by Yunxiao Lin
        /// <summary>
        /// 根据Model_Hitachi获取室外机对象
        /// </summary>
        /// <param name="Model_Hitachi"></param>
        /// <returns></returns>
        public Outdoor GetHitachiItem(string Model_Hitachi, string productType)
        {
            if (string.IsNullOrEmpty(Model_Hitachi))
                return null;

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;
            
            //dv.RowFilter = "ProductType ='" + _productType.Trim() + "' and Model_Hitachi='" + Model_Hitachi.Trim() + "' and Deleteflag = 1";
            dv.RowFilter = "ProductType ='" + productType.Trim() + "' and Model_Hitachi='" + Model_Hitachi.Trim() + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                Outdoor item = new Outdoor();
                DataRow dr = dt.Rows[0];
                item.ModelFull = dr["Model"].ToString(); //Model全名
                item.RegionCode = _region;
                //item.ProductType = _productType;
                item.ProductType = productType;
                item.Model = item.ModelFull.Substring(0, 7);
                string _s = item.Model.Substring(6, 1); // 若第7位不是数字则只取前6位作为ShortModel
                if (!Util.IsNumber(_s))
                    item.Model = item.ModelFull.Substring(0, 6);

                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                item.Type = dr["UnitType"] is DBNull ? "" : dr["UnitType"].ToString();
                item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();
                item.GasPipe_Hi = (dr["GasPipe_Hi"] is DBNull) ? "-" : dr["GasPipe_Hi"].ToString();
                item.GasPipe_Lo = (dr["GasPipe_Lo"] is DBNull) ? "-" : dr["GasPipe_Lo"].ToString();    // TODO
                item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());

                item.Power_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                item.Power_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString()); // TODO
                item.MaxCurrent = (dr["MaxCurrent"] is DBNull) ? 0 : double.Parse(dr["MaxCurrent"].ToString()); // 20140827
                item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                item.RefrigerantCharge = (dr["RefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["RefrigerantCharge"].ToString());
                item.MaxRefrigerantCharge = (dr["MaxRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MaxRefrigerantCharge"].ToString());
                // 根据EU 提供的冷媒追加算法，需要增加ODU最小追加冷媒量 20180502 by Yunxiao Lin
                if (dr.Table.Columns.Contains("MinRefrigerantCharge"))
                    item.MinRefrigerantCharge = (dr["MinRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MinRefrigerantCharge"].ToString());
                else
                    item.MinRefrigerantCharge = 0;

                item.MaxIU = (dr["MaxIU"] is DBNull) ? 0 : int.Parse(dr["MaxIU"].ToString());
                item.RecommendedIU = (dr["RecommendedIU"] is DBNull) ? 0 : int.Parse(dr["RecommendedIU"].ToString());
                item.Price = 99999;

                item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                item.FullModuleName = (dr["FullModuleName"] is DBNull) ? "" : dr["FullModuleName"].ToString();
                item.AuxModelName = (dr["AuxModelName"] is DBNull) ? "" : dr["AuxModelName"].ToString();

                item.MaxPipeLength = (dr["MaxPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLength"].ToString());
                item.MaxEqPipeLength = (dr["MaxEqPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxEqPipeLength"].ToString());
                item.MaxOutdoorAboveHeight = (dr["MaxOutdoorAboveHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorAboveHeight"].ToString());
                item.MaxOutdoorBelowHeight = (dr["MaxOutdoorBelowHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorBelowHeight"].ToString());
                item.MaxDiffIndoorHeight = (dr["MaxDiffIndoorHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorHeight"].ToString());
                item.MaxIndoorLength = (dr["MaxIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength"].ToString());
                item.MaxIndoorLength_MaxIU = (dr["MaxIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength_MaxIU"].ToString());
                item.MaxPipeLengthwithFA = (dr["MaxPipeLengthwithFA"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLengthwithFA"].ToString());
                item.MaxDiffIndoorLength = (dr["MaxDiffIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength"].ToString());
                item.MaxDiffIndoorLength_MaxIU = (dr["MaxDiffIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength_MaxIU"].ToString());

                item.JointKitModelL = (dr["JointKitModelL"] is DBNull) ? "" : dr["JointKitModelL"].ToString();
                item.JointKitModelG = (dr["JointKitModelG"] is DBNull) ? "" : dr["JointKitModelG"].ToString();
                //报告中输出
                item.MaxOperationPI_Cooling = (dr["MaxOperationPI_Cooling"] is DBNull) ? "-" : dr["MaxOperationPI_Cooling"].ToString();
                item.MaxOperationPI_Heating = (dr["MaxOperationPI_Heating"] is DBNull) ? "-" : dr["MaxOperationPI_Heating"].ToString();

                item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

                //增加Total Liquid Piping Length 和L3 add on 20160515 by Yunxiao Lin
                item.MaxTotalPipeLength = (dr["MaxTotalPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength"].ToString());
                item.MaxTotalPipeLength_MaxIU = (dr["MaxTotalPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength_MaxIU"].ToString());
                item.MaxMKIndoorPipeLength = (dr["MaxMKIndoorPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength"].ToString());
                item.MaxMKIndoorPipeLength_MaxIU = (dr["MaxMKIndoorPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength_MaxIU"].ToString());
                //增加Series列 20161031 by Yunxiao Lin
                item.Series = (dr["Series"] is DBNull) ? "" : dr["Series"].ToString().Trim();
                //匹数 add on 20161109 by Yunxiao Lin
                item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                return item;
            }
            return null;
        }

        /// 根据Series获取Model_Hitachi室外机对象
        /// <summary>
        /// 根据Series获取Model_Hitachi室外机对象
        /// </summary>
        /// <param name="Model_Hitachi"></param>
        /// <returns></returns>
        public Outdoor GetHitachiItemBySeries(string Model_Hitachi, string series)
        {
            if (string.IsNullOrEmpty(Model_Hitachi))
                return null;

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;

            //dv.RowFilter = "ProductType ='" + _productType.Trim() + "' and Model_Hitachi='" + Model_Hitachi.Trim() + "' and Deleteflag = 1";
            dv.RowFilter = "Series ='" + series.Trim() + "' and Model_Hitachi='" + Model_Hitachi.Trim() + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                Outdoor item = new Outdoor();
                DataRow dr = dt.Rows[0];
                item.ModelFull = dr["Model"].ToString(); //Model全名
                item.RegionCode = _region;
                //item.ProductType = _productType;
                item.Series = series;
                item.Model = item.ModelFull.Substring(0, 7);
                string _s = item.Model.Substring(6, 1); // 若第7位不是数字则只取前6位作为ShortModel
                if (!Util.IsNumber(_s))
                    item.Model = item.ModelFull.Substring(0, 6);

                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                item.Type = dr["UnitType"] is DBNull ? "" : dr["UnitType"].ToString();
                item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();
                item.GasPipe_Hi = (dr["GasPipe_Hi"] is DBNull) ? "-" : dr["GasPipe_Hi"].ToString();
                item.GasPipe_Lo = (dr["GasPipe_Lo"] is DBNull) ? "-" : dr["GasPipe_Lo"].ToString();    // TODO
                item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());

                item.Power_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                item.Power_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString()); // TODO
                item.MaxCurrent = (dr["MaxCurrent"] is DBNull) ? 0 : double.Parse(dr["MaxCurrent"].ToString()); // 20140827
                item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                item.RefrigerantCharge = (dr["RefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["RefrigerantCharge"].ToString());
                item.MaxRefrigerantCharge = (dr["MaxRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MaxRefrigerantCharge"].ToString());
                // 根据EU 提供的冷媒追加算法，需要增加ODU最小追加冷媒量 20180502 by Yunxiao Lin
                if (dr.Table.Columns.Contains("MinRefrigerantCharge"))
                    item.MinRefrigerantCharge = (dr["MinRefrigerantCharge"] is DBNull) ? 0 : double.Parse(dr["MinRefrigerantCharge"].ToString());
                else
                    item.MinRefrigerantCharge = 0;

                item.MaxIU = (dr["MaxIU"] is DBNull) ? 0 : int.Parse(dr["MaxIU"].ToString());
                item.RecommendedIU = (dr["RecommendedIU"] is DBNull) ? 0 : int.Parse(dr["RecommendedIU"].ToString());
                item.Price = 99999;

                item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                item.FullModuleName = (dr["FullModuleName"] is DBNull) ? "" : dr["FullModuleName"].ToString();
                item.AuxModelName = (dr["AuxModelName"] is DBNull) ? "" : dr["AuxModelName"].ToString();

                item.MaxPipeLength = (dr["MaxPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLength"].ToString());
                item.MaxEqPipeLength = (dr["MaxEqPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxEqPipeLength"].ToString());
                item.MaxOutdoorAboveHeight = (dr["MaxOutdoorAboveHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorAboveHeight"].ToString());
                item.MaxOutdoorBelowHeight = (dr["MaxOutdoorBelowHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxOutdoorBelowHeight"].ToString());
                item.MaxDiffIndoorHeight = (dr["MaxDiffIndoorHeight"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorHeight"].ToString());
                item.MaxIndoorLength = (dr["MaxIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength"].ToString());
                item.MaxIndoorLength_MaxIU = (dr["MaxIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxIndoorLength_MaxIU"].ToString());
                item.MaxPipeLengthwithFA = (dr["MaxPipeLengthwithFA"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxPipeLengthwithFA"].ToString());
                item.MaxDiffIndoorLength = (dr["MaxDiffIndoorLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength"].ToString());
                item.MaxDiffIndoorLength_MaxIU = (dr["MaxDiffIndoorLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxDiffIndoorLength_MaxIU"].ToString());

                item.JointKitModelL = (dr["JointKitModelL"] is DBNull) ? "" : dr["JointKitModelL"].ToString();
                item.JointKitModelG = (dr["JointKitModelG"] is DBNull) ? "" : dr["JointKitModelG"].ToString();
                //报告中输出
                item.MaxOperationPI_Cooling = (dr["MaxOperationPI_Cooling"] is DBNull) ? "-" : dr["MaxOperationPI_Cooling"].ToString();
                item.MaxOperationPI_Heating = (dr["MaxOperationPI_Heating"] is DBNull) ? "-" : dr["MaxOperationPI_Heating"].ToString();

                item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

                //增加Total Liquid Piping Length 和L3 add on 20160515 by Yunxiao Lin
                item.MaxTotalPipeLength = (dr["MaxTotalPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength"].ToString());
                item.MaxTotalPipeLength_MaxIU = (dr["MaxTotalPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxTotalPipeLength_MaxIU"].ToString());
                item.MaxMKIndoorPipeLength = (dr["MaxMKIndoorPipeLength"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength"].ToString());
                item.MaxMKIndoorPipeLength_MaxIU = (dr["MaxMKIndoorPipeLength_MaxIU"] is DBNull) ? 0 : Convert.ToDouble(dr["MaxMKIndoorPipeLength_MaxIU"].ToString());

                //Series作为条件，ProductType需要进行封装支持特殊机型过滤条件使用
                item.ProductType = dr["ProductType"] is DBNull ? "" : dr["ProductType"].ToString();
                //匹数 add on 20161109 by Yunxiao Lin
                item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                return item;
            }
            return null;
        }

        /// 获得室外机的Total IDU Correction Factor add on 20161109 by Yunxiao Lin
        /// <summary>
        /// 获得室外机的Total IDU Correction Factor
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Horsepower"></param>
        /// <param name="isHeating"></param>
        /// <returns></returns>
        public double GetTotalIDUFactor(string Model, double Horsepower, bool isHeating)
        {
            double result = 0;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("ODU_Total_IDU_Factor");
            DataView dv = dt.DefaultView;
            dv.RowFilter = "Model ='" + Model + "' and Horsepower=" + Horsepower; //Horsepower数字参数不要加单引号，不然在非英美数字环境下会出错 20170817 by Yunxiao Lin
            dt = dv.ToTable();
            if (dt.Rows.Count == 0)
            {
                //如果没有正好等于条件匹数的数据，则取前后两条数据进行插值计算
                //首先获取小于条件匹数的数据
                dv.RowFilter = "Model ='" + Model + "' and Horsepower<" + Horsepower;
                dv.Sort = "Horsepower desc";
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    double hp1 = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                    double fc1 = 0;
                    if (isHeating)
                        fc1 = (dr["HeatingFactor"] is DBNull) ? 0 : double.Parse(dr["HeatingFactor"].ToString());
                    else
                        fc1 = (dr["CoolingFactor"] is DBNull) ? 0 : double.Parse(dr["CoolingFactor"].ToString());
                    if (hp1 > 0 && fc1 > 0)
                    {
                        //然后获取大于条件匹数的数据
                        dv.RowFilter = "Model ='" + Model + "' and Horsepower>" + Horsepower;
                        dv.Sort = "Horsepower asc";
                        dt = dv.ToTable();
                        if (dt.Rows.Count > 0)
                        {
                            dr = dt.Rows[0];
                            double hp2 = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                            double fc2 = 0;
                            if (isHeating)
                                fc2 = (dr["HeatingFactor"] is DBNull) ? 0 : double.Parse(dr["HeatingFactor"].ToString());
                            else
                                fc2 = (dr["CoolingFactor"] is DBNull) ? 0 : double.Parse(dr["CoolingFactor"].ToString());
                            if (hp2 > 0 && fc2 > 0)
                            {
                                //进行线性插值计算
                                result = fc1 + (((fc2 - fc1) / (hp2 - hp1)) * (Horsepower - hp1));
                            }
                        }
                    }
                }
            }
            else if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                if (isHeating)
                    result = (dr["HeatingFactor"] is DBNull) ? 0 : double.Parse(dr["HeatingFactor"].ToString());
                else
                    result = (dr["CoolingFactor"] is DBNull) ? 0 : double.Parse(dr["CoolingFactor"].ToString());
            }
            return result;
        }

        /// 从数据库中获取室外机海拔修正系数 20180626 by Yunxiao Lin
        /// <summary>
        /// 从数据库中获取室外机海拔修正系数
        /// </summary>
        /// <param name="altitude">海拔高度，单位暂时为m，以后需要支持ft</param>
        /// <param name="type">海拔修正数据分类，暂时有两个分类，NA和Other</param>
        /// <returns></returns>
        public double GetODUAltitudeFactor(int altitude, string type)
        {
            double result = 0;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Altitude");
            DataView dv = dt.DefaultView;
            //首先判断是否有相等的海拔
            dv.RowFilter = "Altitude_m=" + altitude + " and Type='" + type + "'";
            dt = dv.ToTable();
            if (dt.Rows.Count == 0)
            {
                //如果没有正好等于条件海拔的数据，则取前后两条数据进行插值计算
                //首先获取小于条件海拔的数据
                dv.RowFilter = "Altitude_m<" + altitude + " and Type='" + type + "'";
                dv.Sort = "Altitude_m desc";
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    int altitude1 = (dr["Altitude_m"] is DBNull) ? -10000 : int.Parse(dr["Altitude_m"].ToString());
                    double fc1 = (dr["Factor"] is DBNull) ? 0 : double.Parse(dr["Factor"].ToString());
                    if (altitude1 > -10000 && fc1 > 0)
                    {
                        //然后获取大于条件海拔的数据
                        dv.RowFilter = "Altitude_m>" + altitude + " and Type='" + type + "'";
                        dv.Sort = "Altitude_m asc";
                        dt = dv.ToTable();
                        if (dt.Rows.Count > 0)
                        {
                            dr = dt.Rows[0];
                            double altitude2 = (dr["Altitude_m"] is DBNull) ? -10000 : int.Parse(dr["Altitude_m"].ToString());
                            double fc2 = (dr["Factor"] is DBNull) ? 0 : double.Parse(dr["Factor"].ToString());
                            if (altitude2 > -10000 && fc2 > 0)
                            {
                                //进行线性插值计算
                                result = fc1 + (((fc2 - fc1) / (altitude2 - altitude1)) * (altitude - altitude1));
                            }
                        }
                        else
                        {
                            //如果海拔超出上限，取最大海拔修正
                            result = fc1;
                        }
                    }
                }
                else
                {
                    //如果海拔低于下限，不做修正
                    result = 1d;
                }
            }
            else if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                result = (dr["Factor"] is DBNull) ? 0 : double.Parse(dr["Factor"].ToString());
            }
            return result;
        }

        /// 从数据库中获取室外机除霜修正系数 20180626 by Yunxiao Lin
        /// <summary>
        /// 从数据库中获取室外机除霜修正系数
        /// </summary>
        /// <param name="outDB">室外干球温度，单位暂时为摄氏度，以后需要支持华氏度</param>
        /// <param name="type">除霜修正数据分类，暂时有两个分类，NA和Other</param>
        /// <returns></returns>
        public double GetODUDefrostFactor(double outDB, string type)
        {
            double result = 0;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Defrost");
            DataView dv = dt.DefaultView;
            //首先判断是否有相等的室外温度
            dv.RowFilter = "OutDB_C=" + outDB + " and Type='" + type + "'";
            dt = dv.ToTable();
            if (dt.Rows.Count == 0)
            {
                //如果没有正好等于条件温度的数据，则取前后两条数据进行插值计算
                //首先获取大于条件温度的数据
                dv.RowFilter = "OutDB_C>" + outDB + " and Type='" + type + "'";
                dv.Sort = "OutDB_C asc";
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    double outDB1 = (dr["OutDB_C"] is DBNull) ? -10000d : double.Parse(dr["OutDB_C"].ToString());
                    double fc1 = (dr["Factor"] is DBNull) ? 0 : double.Parse(dr["Factor"].ToString());
                    if (outDB1 > -9999 && fc1 > 0)
                    {
                        //然后获取小于条件温度的数据
                        dv.RowFilter = "OutDB_C<" + outDB + " and Type='" + type + "'";
                        dv.Sort = "OutDB_C desc";
                        dt = dv.ToTable();
                        if (dt.Rows.Count > 0)
                        {
                            dr = dt.Rows[0];
                            double outDB2 = (dr["OutDB_C"] is DBNull) ? -10000d : double.Parse(dr["OutDB_C"].ToString());
                            double fc2 = (dr["Factor"] is DBNull) ? 0 : double.Parse(dr["Factor"].ToString());
                            if (outDB2 > -9999 && fc2 > 0)
                            {
                                //进行线性插值计算
                                result = fc2 + (((fc1 - fc2) / (outDB1 - outDB2)) * (outDB - outDB2));
                            }
                        }
                        else
                        {
                            //如果温度低于下限，取最低温度修正
                            result = fc1;
                        }
                    }
                }
                else
                {
                    //如果温度超出上限, 不做修正
                    result = 1d;
                }
            }
            else if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                result = (dr["Factor"] is DBNull) ? 0 : double.Parse(dr["Factor"].ToString());
            }
            return result;
        }

        // 通过Serie和unittype获取室外机power list add by axj 20170122
        /// <summary>
        /// 通过Serie和unittype获取室外机power list
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable GetOutdoorPowerListBySeriesAndType(string series,string Type)
        {
            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (!string.IsNullOrEmpty(sTableName))
            {
                DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
                DataView dv = dt.DefaultView;
                dv.RowFilter = " Series= '" + series + "' and UnitType='" + Type + "' and Deleteflag = 1";
                if (_brandCode == "Y")
                    dv.RowFilter += " and Model_York<>'-'";
                else
                    dv.RowFilter += " and Model_York='-'";
                dt = dv.ToTable();
                DataTable dtDic = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Dictionary");
                DataRow[] drs = dtDic.Select("Type='PowerSupply'");
                Hashtable htPow = new Hashtable();
                foreach (var dr in drs)
                {
                    htPow[dr["Code"].ToString()] = dr["Name"].ToString();
                }
                DataTable dtNew = new DataTable();
                dtNew.Columns.Add("name");
                dtNew.Columns.Add("value");
                Hashtable ht=new Hashtable();

                var query = from t in dt.AsEnumerable()
                               group t by t.Field<string>("Model").Substring(10,1)
                                   into g
                                   select new
                                   {
                                       name = g.Key,
                                   };
                foreach (var ent in query)
                {
                    dtNew.Rows.Add(new object[] { htPow[ent.name].ToString(), ent.name });
                }
                return dtNew;
            }
            return null;
        }


        // 通过model和productType获取特殊机型的型号名称 add on 20170320 by Lingjia Qiu 
        /// <summary>
        /// 通过model和productType获取特殊机型的型号名称
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public string GetModelName(string model,string productType)
        {
            if (string.IsNullOrEmpty(model))
                return null;

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;

            //dv.RowFilter = "ProductType ='" + _productType.Trim() + "' and Model_York='" + Model_York.Trim() + "' and Deleteflag = 1";
            dv.RowFilter = "ProductType ='" + productType.Trim() + "' and Model='" + model.Trim() + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();

            if (dt != null && dt.Rows.Count > 0)
            {
                if (_brandCode == "Y")
                    return dt.Rows[0]["Model_York"].ToString();
                else
                    return dt.Rows[0]["Model_Hitachi"].ToString();
            }
            return null;

        }

        public dynamic GetODUCombined(string auxmodelname, string series)
        {
            if (string.IsNullOrEmpty(auxmodelname))
                return null;
            dynamic Combinedod = new System.Dynamic.ExpandoObject();

            string sTableName = GetStdTableName(IndoorType.Outdoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;


            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataView dv = dt.DefaultView;

            dv.RowFilter = "Series ='" + series + "' and Fullmodulename='" + auxmodelname + "' and Deleteflag = 1";
            if (_brandCode == "Y")
                dv.RowFilter += " and Model_York<>'-'";
            else
                dv.RowFilter += " and Model_York='-'";
            dt = dv.ToTable();
            string val = String.Empty;

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                Combinedod.Weight = (dr["Weight"] is DBNull) ? "0" : dr["Weight"].ToString();
                Combinedod.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                Combinedod.Maxcurrent = (dr["MaxCurrent"] is DBNull) ? "0" : dr["MaxCurrent"].ToString();
                return Combinedod;
            }
            return null;
        }

    }
}
