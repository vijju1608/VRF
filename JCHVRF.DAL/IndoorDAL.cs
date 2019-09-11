using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

using JCHVRF.Model;
using JCHVRF.DALFactory;
using System.Linq;

namespace JCHVRF.DAL
{
    public class IndoorDAL : IIndoorDAL
    {
        IDataAccessObject _dao;
        string _region;
        //string _productType; //V1.2.1之后，由于多ProductType需求，_productType不再使用
        string _brandCode;
        string _mainRegion;
        string _subRegion;

        ////public IndoorDAL(string region, string productType, string brandCode)
        public IndoorDAL(string region, string brandCode)
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
            _region = region;
            //_productType = productType; //V1.2.1之后，由于多ProductType需求，_productType不再使用
            _brandCode = brandCode;
            _mainRegion = Project.CurrentProject.RegionCode;
            _subRegion = Project.CurrentProject.SubRegionCode;
        }

        //重构构造函数，增加mianRegion，暂时用于欧洲特殊判断
        public IndoorDAL(string subRegion, string mainRegion, string brandCode)
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
            _region = subRegion;
            _mainRegion = mainRegion;
            _brandCode = brandCode;
            _subRegion = Project.CurrentProject.SubRegionCode;
            //by pass here
            //_subRegion = Project.CurrentProject.SubRegionCode;
        }

        public DataRow GetAirflowEsp(string modelname, string power)
        {
            string sTableName = "JCHVRF_HeatExchanger";

            string sql = "select * from " + sTableName + " where Model_Hitachi = '" + modelname + "' and Power_Supply = '" + power + "' ";
            DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0 )
            {
                return dt.Rows[0];
            }
            return null;

        }

        public Indoor GetItem(string modelFull, string unitType, string productType, string Series)
        {
            if (string.IsNullOrEmpty(modelFull))
                return null;

            string sTableName = GetStdTableName(IndoorType.Indoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            //string sql = "select * from " + sTableName + " where ProductType = '" + _productType
            //    + "' and UnitType = '" + unitType + "' and Model = '" + modelFull + "' and Deleteflag = 1";
            // Indoor 选型时，ProductType不再是固定值，变成可选 20180621 Yunxiao Lin
            string sql = "select * from " + sTableName + " where ProductType = '" + productType
                +( "' and Model = '" + modelFull + "' or Model_Hitachi = '" + modelFull )+ "' and Deleteflag = 1";
            if (!string.IsNullOrEmpty(unitType))
                sql += " and UnitType = '" + unitType + "' ";

            if (_brandCode == "Y")
                sql += " and Model_York<>'-'";
            else
                sql += " and Model_York='-'";
            DataTable dt = _dao.GetDataTable(sql);
            DataTable dtAirFlow = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_IDU_AirFlow");

            if (dt != null && dt.Rows.Count > 0)
            {
                Indoor item = new Indoor();
                DataRow dr = dt.Rows[0];
                if (dr["SelectionType"].ToString() == IndoorType.Indoor.ToString())
                {
                    item.Model = modelFull.Substring(0, 7);
                    item.Flag = IndoorType.Indoor;
                }
                else if (dr["SelectionType"].ToString() == IndoorType.Exchanger.ToString())
                {
                    item.Model = modelFull.Substring(0, 7);
                    item.Flag = IndoorType.Exchanger;
                }
                else
                {
                    if (_region == "LA_BR") //巴西的新风机model取前7位，其他取前8位
                        item.Model = modelFull.Substring(0, 7);
                    else
                        item.Model = modelFull.Substring(0, 8);
                    item.Flag = IndoorType.FreshAir;
                }
                item.RegionCode = _region;
                item.ProductType = productType;
                item.ModelFull = modelFull;
                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                //item.Type = unitType;
                item.Type = dr["UnitType"] is DBNull ? "" : dr["UnitType"].ToString(); ;
                item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                item.SensibleHeat = (dr["SensibleHeat"] is DBNull) ? 0 : double.Parse(dr["SensibleHeat"].ToString());
                item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());
                //item.AirFlow_Hi = (dr["AirFlow_Hi"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Hi"].ToString());
                //item.AirFlow_Med = (dr["AirFlow_Med"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Med"].ToString());
                //item.AirFlow_Lo = (dr["AirFlow_Lo"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Lo"].ToString());

                item.GasPipe = (dr["GasPipe"] is DBNull) ? "-" : dr["GasPipe"].ToString();
                item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();

                item.PowerInput_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                item.PowerInput_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString());
                // wiring 图中需要标注电流文字
                item.RatedCurrent = (dr["RatedCurrent"] is DBNull) ? 0 : double.Parse(dr["RatedCurrent"].ToString());
                item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                item.NoiseLevel_Hi = (dr["NoiseLevel_Hi"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Hi"].ToString());
                item.NoiseLevel_Med = (dr["NoiseLevel_Med"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Med"].ToString());
                item.NoiseLevel_Lo = (dr["NoiseLevel_Lo"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Lo"].ToString());
                // 水盘，暂无应用
                item.DrainagePipe = (dr["DrainagePipe"] is DBNull) ? 0 : double.Parse(dr["DrainagePipe"].ToString());
                //// 静压
                // item.ESP = (dr["ESP"] is DBNull) ? "-" : dr["ESP"].ToString();

                item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                item.UniqueOutdoorName = (dr["UniqueOutdoorName"] is DBNull) ? "" : dr["UniqueOutdoorName"].ToString();
                item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

                //匹数 add on 20161109 by Yunxiao Lin
                item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                //SHF add on 20161109 by Yunxiao Lin
                //item.SHF_Hi = (dr["SHF_Hi"] is DBNull) ? 0 : double.Parse(dr["SHF_Hi"].ToString());
                //item.SHF_Med = (dr["SHF_Med"] is DBNull) ? 0 : double.Parse(dr["SHF_Med"].ToString());
                //item.SHF_Lo = (dr["SHF_Lo"] is DBNull) ? 0 : double.Parse(dr["SHF_Lo"].ToString());


                #region SHF/Air Flow/Static Pressure数据从JCHVRF_IDU_AirFlow取得 add on 20170703 by Shen Junjie

                DataRow[] drsAirFlow = dtAirFlow.Select("Model_Hitachi='" + item.Model_Hitachi + "'");

                if (drsAirFlow.Length > 0)
                {
                    DataRow drAirFlow = drsAirFlow[0];
                    item.AirFlow_Levels = new double[]{
                        (drAirFlow["AirFlow_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Hi2"].ToString()),
                        (drAirFlow["AirFlow_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Hi"].ToString()),
                        (drAirFlow["AirFlow_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Med"].ToString()),
                        (drAirFlow["AirFlow_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Lo"].ToString())
                    };
                    item.ESP_Levels = new double[]{
                        (drAirFlow["StaticPressure_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Hi2"].ToString()),
                        (drAirFlow["StaticPressure_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Hi"].ToString()),
                        (drAirFlow["StaticPressure_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Med"].ToString()),
                        (drAirFlow["StaticPressure_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Lo"].ToString())
                    };
                    item.SHF_Levels = new double[]{
                        (drAirFlow["SHF_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Hi2"].ToString()),
                        (drAirFlow["SHF_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Hi"].ToString()),
                        (drAirFlow["SHF_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Med"].ToString()),
                        (drAirFlow["SHF_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Lo"].ToString())
                    };
                }

                #endregion

                List<Accessory> list = (new AccessoryDAL()).GetDefault(item, _mainRegion, _subRegion, Series);
                item.ListAccessory = list; 
                bool isFA = false;
                if (!string.IsNullOrEmpty(_mainRegion))
                {
                    if (_mainRegion==("EU_W") || _mainRegion == ("EU_S") || _mainRegion == ("EU_E") || _subRegion == "TW")   //台湾地区数据与欧洲区域数据 都需要获取displayName on 20180730 by xyj
                    {

                        //新增工厂编号 区分displayName on 20180730 by xyj
                        string displayName = GetDisplayNameStr(item.Type, item.GetFactoryCode(), out isFA);  //排除获取displayName是新风机的显示名的可能
                        if (!isFA)
                            item.DisplayName = displayName;
                    }

                }

                return item;
            }
            return null;
        }

        public Indoor GetFreshAirItem(string modelFull, string unitType, bool isCompositeMode, string productType, string Series)
        {
            if (string.IsNullOrEmpty(modelFull))
                return null;

            string sTableName = GetStdTableName(IndoorType.Indoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;

            //string sql = "select * from " + sTableName + " where ProductType = '" + _productType
            //    + "' and UnitType = '" + unitType + "' and Model = '" + modelFull + "' and (UniqueOutdoorName = '' or UniqueOutdoorName is null) and Deleteflag = 1";
            // 由于多Product Typle 需求,ProductType不再作为固定值 20160821 by Yunxiao Lin
            string sql = "select * from " + sTableName + " where ProductType = '" + productType
                + "' and UnitType = '" + unitType + "' and Model = '" + modelFull + "' and (UniqueOutdoorName = '' or UniqueOutdoorName is null) and Deleteflag = 1";
            if (!isCompositeMode)  //非混连模式下查询条件 UniqueOutdoorName不为空 add on 20161008 by Lingjia Qiu
            {
                //sql = "select * from " + sTableName + " where ProductType = '" + _productType
                //+ "' and UnitType = '" + unitType + "' and Model = '" + modelFull + "' and Deleteflag = 1";
                sql = "select * from " + sTableName + " where ProductType = '" + productType
                + "' and UnitType = '" + unitType + "' and Model = '" + modelFull + "' and (UniqueOutdoorName <> '' or UniqueOutdoorName is not null) and Deleteflag = 1";
            }

            DataTable dt = _dao.GetDataTable(sql);
            DataTable dtAirFlow = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_IDU_AirFlow");

            if (dt != null && dt.Rows.Count > 0)
            {
                Indoor item = new Indoor();
                DataRow dr = dt.Rows[0];
                if (dr["SelectionType"].ToString() == IndoorType.Indoor.ToString())
                {
                    item.Model = modelFull.Substring(0, 7);
                    item.Flag = IndoorType.Indoor;
                }
                else
                {
                    if (_region == "LA_BR") //巴西的新风机model取前7位，其他取前8位。 20170330 by Yunxiao Lin
                        item.Model = modelFull.Substring(0, 7);
                    else
                        item.Model = modelFull.Substring(0, 8);
                    item.Flag = IndoorType.FreshAir;
                }
                item.RegionCode = _region;
                //item.ProductType = _productType;
                item.ProductType = productType;
                item.ModelFull = modelFull;
                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                item.Type = unitType;
                item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                item.SensibleHeat = (dr["SensibleHeat"] is DBNull) ? 0 : double.Parse(dr["SensibleHeat"].ToString());
                item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());
                //item.AirFlow_Hi = (dr["AirFlow_Hi"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Hi"].ToString());
                //item.AirFlow_Med = (dr["AirFlow_Med"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Med"].ToString());
                //item.AirFlow_Lo = (dr["AirFlow_Lo"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Lo"].ToString());

                item.GasPipe = (dr["GasPipe"] is DBNull) ? "-" : dr["GasPipe"].ToString();
                item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();

                item.PowerInput_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                item.PowerInput_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString());
                // wiring 图中需要标注电流文字
                item.RatedCurrent = (dr["RatedCurrent"] is DBNull) ? 0 : double.Parse(dr["RatedCurrent"].ToString());
                item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                item.NoiseLevel_Hi = (dr["NoiseLevel_Hi"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Hi"].ToString());
                item.NoiseLevel_Med = (dr["NoiseLevel_Med"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Med"].ToString());
                item.NoiseLevel_Lo = (dr["NoiseLevel_Lo"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Lo"].ToString());
                // 水盘，暂无应用
                item.DrainagePipe = (dr["DrainagePipe"] is DBNull) ? 0 : double.Parse(dr["DrainagePipe"].ToString());
                //// 静压
                //item.ESP = (dr["ESP"] is DBNull) ? "-" : dr["ESP"].ToString();

                item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                item.UniqueOutdoorName = (dr["UniqueOutdoorName"] is DBNull) ? "" : dr["UniqueOutdoorName"].ToString();
                item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();
                //匹数 add on 20161109 by Yunxiao Lin
                item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                //SHF add on 20161109 by Yunxiao Lin
                //item.SHF_Hi = (dr["SHF_Hi"] is DBNull) ? 0 : double.Parse(dr["SHF_Hi"].ToString());
                //item.SHF_Hi = (dr["SHF_Med"] is DBNull) ? 0 : double.Parse(dr["SHF_Med"].ToString());
                //item.SHF_Hi = (dr["SHF_Lo"] is DBNull) ? 0 : double.Parse(dr["SHF_Lo"].ToString());


                #region SHF/Air Flow/Static Pressure数据从JCHVRF_IDU_AirFlow取得 add on 20170703 by Shen Junjie

                DataRow[] drsAirFlow = dtAirFlow.Select("Model_Hitachi='" + item.Model_Hitachi + "'");

                if (drsAirFlow.Length > 0)
                {
                    DataRow drAirFlow = drsAirFlow[0];
                    item.AirFlow_Levels = new double[]{
                        (drAirFlow["AirFlow_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Hi2"].ToString()),
                        (drAirFlow["AirFlow_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Hi"].ToString()),
                        (drAirFlow["AirFlow_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Med"].ToString()),
                        (drAirFlow["AirFlow_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Lo"].ToString())
                    };
                    item.ESP_Levels = new double[]{
                        (drAirFlow["StaticPressure_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Hi2"].ToString()),
                        (drAirFlow["StaticPressure_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Hi"].ToString()),
                        (drAirFlow["StaticPressure_Med"] is DBNull) ? 0 : double.Parse(dr["StaticPressure_Med"].ToString()),
                        (drAirFlow["StaticPressure_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Lo"].ToString())
                    };
                    item.SHF_Levels = new double[]{
                        (drAirFlow["SHF_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Hi2"].ToString()),
                        (drAirFlow["SHF_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Hi"].ToString()),
                        (drAirFlow["SHF_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Med"].ToString()),
                        (drAirFlow["SHF_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Lo"].ToString())
                    };
                }

                #endregion

                List<Accessory> list = (new AccessoryDAL()).GetDefault(item, _region, _subRegion, Series);
                item.ListAccessory = list;

                return item;
            }
            return null;
        }

        /// 增加factoryName参数 20161118 by Yunxiao Lin
        /// 获取室内机标准表
        /// <summary>
        /// 获取室内机标准表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable GetListStd(string type, string productType, string factoryName)
        {
            // 此处GetStdTableName中的参数flag直接用“Indoor”，因为Indoor与FreshAir的数据放在同一张标准表中
            // update on 20120424 clh
            string sTableName = GetStdTableName(IndoorType.Indoor);

            if (!string.IsNullOrEmpty(sTableName))
            {
                string shortModel = "Mid(Model,1,7)";
                if (type.Contains("YDCF"))
                    shortModel = "Mid(Model,1,8)";

                //string query = "SELECT DISTINCT " + shortModel + " as Model, Model as ModelFull, Model_York, Model_Hitachi, "
                //    + "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName From "
                //    + sTableName + " Where ProductType= '" + _productType
                //    + "' and UnitType='" + type + "' and DeleteFlag = 1";
                // 由于多productType需求，productType不再作为固定值 20160821 by Yunxiao Lin
                // 增加参数Hosepower, SHF_Hi,SHF_Me,SHF_Lo 20161111 by Yunxiao Lin
                string query = "SELECT DISTINCT " + shortModel + " as Model, Model as ModelFull, Model_York, Model_Hitachi, "
                    //+ "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName From "
                    + "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName,Horsepower,SHF_Hi,SHF_Med,SHF_Lo From "
                    + sTableName + " Where ProductType= '" + productType
                    + "' and UnitType='" + type + "' and DeleteFlag = 1";
                if (_brandCode == "Y")
                    query += " and Model_York<>'-'";
                else
                    query += " and Model_York='-'";
                //增加factoryCode判断 20161118 by Yunxiao Lin
                if (!string.IsNullOrEmpty(factoryName))
                {
                    string factoryCode = "Q";
                    switch (factoryName)
                    {
                        case "HAPE":
                            factoryCode = "E";
                            break;
                        case "HAPB":
                            factoryCode = "B";
                            break;
                        case "HAPQ":
                            factoryCode = "Q";
                            break;
                        case "SMZ":
                            factoryCode = "S";
                            break;
                        case "HAPM":
                            factoryCode = "M";
                            break;
                        case "HHLI":
                            factoryCode = "I";
                            break;
                        case "GZF":
                            factoryCode = "G";
                            break;
                    }
                    query += " and right(trim(Model),1)='" + factoryCode + "'";
                }
                query += " Order by CoolCapacity";

                DataTable dt = _dao.GetDataTable(query);

                foreach (DataRow dr in dt.Rows)
                {
                    double cap = dr["CoolCapacity"] is DBNull ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                    double cap_h = dr["HeatCapacity"] is DBNull ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                    double heat = dr["SensibleHeat"] is DBNull ? 0 : double.Parse(dr["SensibleHeat"].ToString());
                    double flow = dr["AirFlow"] is DBNull ? 0 : double.Parse(dr["AirFlow"].ToString());
                    double hp = dr["Horsepower"] is DBNull ? 0 : double.Parse(dr["Horsepower"].ToString());
                    double shf_hi = dr["SHF_Hi"] is DBNull ? 0 : double.Parse(dr["SHF_Hi"].ToString());
                    double shf_med = dr["SHF_Med"] is DBNull ? 0 : double.Parse(dr["SHF_Med"].ToString());
                    double shf_lo = dr["SHF_Lo"] is DBNull ? 0 : double.Parse(dr["SHF_Lo"].ToString());
                    dr["CoolCapacity"] = cap;
                    dr["SensibleHeat"] = heat;
                    dr["AirFlow"] = flow;
                    dr["HeatCapacity"] = cap_h;
                    dr["Horsepower"] = hp;
                    dr["SHF_Hi"] = shf_hi;
                    dr["SHF_Med"] = shf_med;
                    dr["SHF_Lo"] = shf_lo;
                }

                // Fresh Air 类型中存在同Model的记录，需要特殊处理
                DataView dv = dt.DefaultView;
                if (type == "Fresh Air")
                {
                    dv.RowFilter = "ModelFull like '%1080%' or UniqueOutdoorName<>''";
                }

                return dv.ToTable();
            }
            return null;
        }


        /// <summary>
        /// 获取全热交换机电源类型 on 2017-07-28 by xyj
        /// </summary>
        /// <param name="type">全热交换机类型</param>
        /// <param name="productType"></param>
        /// <returns></returns>
        public DataTable GetExchangerPowerType(string type, string productType)
        {
            string sTableName = GetStdTableName(IndoorType.Indoor);
            //string sTableName = GetStdName(_mainRegion);

            if (!string.IsNullOrEmpty(sTableName))
            {
                //获取全热交换机电源类型Mid(Model,11,1)

                //string query = "SELECT DISTINCT left(right(Model,4),1) as ModelType From "
                //    + sTableName + " Where ProductType= '" + productType
                //    + "' and UnitType='" + type + "' and DeleteFlag = 1 ";
                string query = "SELECT DISTINCT Mid(Model,11,1) as ModelType From "
                  + sTableName + " Where  UnitType='" + type + "' and DeleteFlag = 1 ";

                if (_brandCode == "Y")
                    query += " and Model_York<>'-'";
                else
                    query += " and Model_York='-'";
                DataTable dt = _dao.GetDataTable(query);


                // Fresh Air 类型中存在同Model的记录，需要特殊处理
                DataView dv = dt.DefaultView;


                return dv.ToTable();
            }
            return null;
        }


        /// <summary>
        /// 获取全热交换机类型 on 2017-07-28 by xyj
        /// </summary>
        /// <returns></returns>
        public DataTable GetExchangerTypeList()
        {
            string sTableName = GetStdTableName(IndoorType.Indoor);

            if (!string.IsNullOrEmpty(sTableName))
            {
                string query = "SELECT DISTINCT UnitType From "
                  + sTableName + " Where SelectionType= 'Exchanger' and DeleteFlag = 1";
                if (sTableName.Contains("_UNIVERSAL")) //IDU_Std_UNIVERSAL表中regionCode格式和其他Std表不一样，code两侧有"/" 20180809 by Yunxiao Lin
                    query += " and RegionCode like '%/" + _region + "/%'";
                else
                    query += " and RegionCode='" + _region + "'";
                if (_brandCode == "Y")
                    query += " and Model_York<>'-'";
                else
                    query += " and Model_York='-'";    
                DataTable dt = _dao.GetDataTable(query);
                // Fresh Air 类型中存在同Model的记录，需要特殊处理
                DataView dv = dt.DefaultView;
                return dv.ToTable();
            }
            return null;
        }      

        /// 增加factoryName参数 20170726 by xyj 
        /// 获取全热交换机标准表
        /// <summary>
        /// 获取全热交换机标准表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable GetExchangerListStd(string type, string factoryName, string power)
        {
            // 此处GetStdTableName中的参数flag直接用“Indoor”，因为Indoor与FreshAir的数据放在同一张标准表中
            // update on 20120424 clh
            string sTableName = GetStdTableName(IndoorType.Indoor);

            if (!string.IsNullOrEmpty(sTableName))
            {
                string shortModel = "Mid(Model,1,7)";
                if (type.Contains("YDCF"))
                    shortModel = "Mid(Model,1,8)";

                //string query = "SELECT DISTINCT " + shortModel + " as Model, Model as ModelFull, Model_York, Model_Hitachi, "
                //    + "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName From "
                //    + sTableName + " Where ProductType= '" + _productType
                //    + "' and UnitType='" + type + "' and DeleteFlag = 1";
                // 由于多productType需求，productType不再作为固定值 20160821 by Yunxiao Lin
                // 增加参数Hosepower, SHF_Hi,SHF_Me,SHF_Lo 20161111 by Yunxiao Lin
                string query = "SELECT DISTINCT " + shortModel + " as Model, Model as ModelFull, Model_York, Model_Hitachi, "
                    //+ "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName From "
                    + "SelectionType,CoolCapacity, SensibleHeat, AirFlow, AirFlow_Hi, AirFlow_Med, AirFlow_Lo, HeatCapacity, TypeImage, UniqueOutdoorName,Horsepower,SHF_Hi,SHF_Med,SHF_Lo, ESP From "
                    + sTableName + " Where  UnitType='" + type + "' and DeleteFlag = 1 ";


                //if (power.Contains("N"))
                //{

                //    query += "and Mid(Model,11,1) in ('N','M')";
                //   // query += "and left(right(Model,4),1) in ('N','M')";
                //}
                //else {
                // query += "and left(right(Model,4),1)='" + power + "'";
                query += "and Mid(Model,11,1)='" + power + "'";
                // }

                if (_brandCode == "Y")
                    query += " and Model_York<>'-'";
                else
                    query += " and Model_York='-'";
                //增加factoryCode判断 20161118 by Yunxiao Lin
                if (!string.IsNullOrEmpty(factoryName))
                {
                    string factoryCode = "Q";
                    switch (factoryName)
                    {
                        case "HAPE":
                            factoryCode = "E";
                            break;
                        case "HAPB":
                            factoryCode = "B";
                            break;
                        case "HAPQ":
                            factoryCode = "Q";
                            break;
                        case "SMZ":
                            factoryCode = "S";
                            break;
                        case "HAPM":
                            factoryCode = "M";
                            break;
                        case "HHLI":
                            factoryCode = "I";
                            break;
                        case "GZF":
                            factoryCode = "G";
                            break;
                    }
                    query += " and right(trim(Model),1)='" + factoryCode + "'";
                }
                query += " Order by CoolCapacity";

                DataTable dt = _dao.GetDataTable(query);

                foreach (DataRow dr in dt.Rows)
                {
                    double cap = dr["CoolCapacity"] is DBNull ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                    double cap_h = dr["HeatCapacity"] is DBNull ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                    double heat = dr["SensibleHeat"] is DBNull ? 0 : double.Parse(dr["SensibleHeat"].ToString());
                    double flow = dr["AirFlow"] is DBNull ? 0 : double.Parse(dr["AirFlow"].ToString());
                    double hp = dr["Horsepower"] is DBNull ? 0 : double.Parse(dr["Horsepower"].ToString());
                    double shf_hi = dr["SHF_Hi"] is DBNull ? 0 : double.Parse(dr["SHF_Hi"].ToString());
                    double shf_med = dr["SHF_Med"] is DBNull ? 0 : double.Parse(dr["SHF_Med"].ToString());
                    double shf_lo = dr["SHF_Lo"] is DBNull ? 0 : double.Parse(dr["SHF_Lo"].ToString());
                    dr["CoolCapacity"] = cap;
                    dr["SensibleHeat"] = heat;
                    dr["AirFlow"] = flow;
                    dr["HeatCapacity"] = cap_h;
                    dr["Horsepower"] = hp;
                    dr["SHF_Hi"] = shf_hi;
                    dr["SHF_Med"] = shf_med;
                    dr["SHF_Lo"] = shf_lo;
                }

                // Fresh Air 类型中存在同Model的记录，需要特殊处理
                DataView dv = dt.DefaultView;
                if (type == "Fresh Air")
                {
                    dv.RowFilter = "ModelFull like '%1080%' or UniqueOutdoorName<>''";
                }

                return dv.ToTable();
            }
            return null;
        }

        /// 获取当前可选的室内机的类型记录列表
        /// <summary>
        /// 获取当前可选的室内机的类型记录列表，不区分 FreshAir
        /// </summary>
        /// <param name="colName"> 返回的字段列名，用于绑定界面下拉框的 DisplayMember 属性 </param>
        /// <returns></returns>
        public DataTable GetTypeList(out string colName, string productType)
        {
            colName = "UnitType";
            // 此处GetStdTableName中的参数flag直接用“Indoor”，因为Indoor与FreshAir的数据放在同一张标准表中
            // update on 20120424 clh
            string sTableName = GetStdTableName(IndoorType.Indoor);

            if (!string.IsNullOrEmpty(sTableName))
            {
                //string query = "SELECT DISTINCT " + colName + " From " + sTableName
                //    + " Where ProductType= '" + _productType + "' and Deleteflag=1"; //RegionCode = '" + _region + "' and 
                //多productType修改 20160821 by Yunxiao Lin
                string query = "SELECT DISTINCT " + colName + " From " + sTableName
                    + " Where ProductType= '" + productType + "' and Deleteflag=1"; //RegionCode = '" + _region + "' and
                if (_brandCode == "Y")
                    query += " and Model_York<>'-'";
                else
                    query += " and Model_York='-'";
                DataTable dt = _dao.GetDataTable(query);
                return dt;
            }
            return null;
        }


        /// 获取当前可选的室内机的工厂编号
        /// <summary>
        /// 获取当前可选的室内机的工厂编号
        /// </summary>
        /// <param name="colName"> 返回的字段列名，用于绑定界面下拉框的 DisplayMember 属性 </param>
        /// <returns></returns>
        public DataTable GetIndoorFacCodeList(string productType)
        {
            string sTableName = GetStdTableName(IndoorType.Indoor);

            if (!string.IsNullOrEmpty(sTableName))
            {
                //string query = "SELECT DISTINCT " + colName + " From " + sTableName
                //    + " Where ProductType= '" + _productType + "' and Deleteflag=1"; //RegionCode = '" + _region + "' and 
                //多productType修改 20160821 by Yunxiao Lin
                string strtemp = " and Model_York='-'";
                if (_brandCode == "Y")
                    strtemp = " and Model_York<>'-'";
                string query = "select t1.*,t2.FactoryCount from ( select distinct right(trim(model),1) as FactoryCode,UnitType,SelectionType from " + sTableName + " where ProductType='" + productType + "' and deleteFlag=1 " + strtemp + ") as t1 "
                       + "inner join (select UnitType, count(UnitType) as factoryCount from (select distinct  right(trim(model),1) as FactorCode,UnitType from " + sTableName + " where ProductType='" + productType + "' and deleteFlag=1 " + strtemp + ") "
                + "group by UnitType ) as t2 on t1.UnitType = t2.UnitType "
                + "order by t1.UnitType, t1.FactoryCode";
                DataTable dt = _dao.GetDataTable(query);
                return dt;
            }
            return null;
        }


        ///// 计算室内机的估算容量,不再返回显热值
        ///// <summary>
        ///// 计算室内机的估算容量，查容量表,TODO
        ///// </summary>
        ///// <param name="inItem"></param>
        ///// <param name="inWB"></param>
        ///// <param name="estSH"></param>
        ///// <param name="isHeating"></param>
        ///// <returns></returns>
        //public double CalEstCapacity(Indoor inItem, double OutTemperature, double InTemperature, bool isHeating)
        //{
        //    double estCap = 0;

        //    if (inItem != null)
        //    {
        //        if (inItem.Type == "Fresh Air" || inItem.Type == "Ventilation")
        //            return inItem.CoolingCapacity;


        //        string model = inItem.Model;
        //        string partLoadTableName = inItem.PartLoadTableName;
        //        if (string.IsNullOrEmpty(partLoadTableName))
        //        {
        //            return 0;
        //        }

        //        List<string> Target = new List<string> { };    //插值计算输出列表，如"Capacity","Power"等
        //        List<double> Val_Interpolate = new List<double> { }; //插值计算返回列表
        //        string Condition = isHeating ? "Heating" : "Cooling";

        //        Target.Clear();
        //        Target.Add("TC");
        //        if (!isHeating)
        //            Target.Add("SHC");
        //        Val_Interpolate = CDL.CDL.Interpolate((new GetDatabase()).GetDataAccessObject().GetConnString(), partLoadTableName,
        //            "Condition ='" + Condition + "' and ShortModel='" + model + "'", "OutDB", OutTemperature, "InWB", InTemperature, Target);
        //        if (Val_Interpolate.Count == 0)
        //        {
        //            return -1;
        //        }
        //        else
        //        {
        //            estCap = Val_Interpolate[0];
        //        }
        //    }
        //    return estCap;
        //}
        /// 计算室内机的估算容量,不再返回显热值
        /// 优化计算速度 20161130
        /// <summary>
        /// 计算室内机的估算容量，查容量表,TODO
        /// </summary>
        /// <param name="inItem"></param>
        /// <param name="inWB"></param>
        /// <param name="estSH"></param>
        /// <param name="isHeating"></param>
        /// <returns></returns>
        public double CalEstCapacity(Indoor inItem, double OutTemperature, double InTemperature, bool isHeating)
        {
            double estCap = 0;

            if (inItem != null)
            {
                //if (inItem.Type == "Fresh Air" || inItem.Type == "Ventilation")
                //    return inItem.CoolingCapacity;
                // FreeAir, Hydro Free和DX-Interface没有partload数据，取标准capacity 20171204 by Yunxiao Lin
                //取消Fresh Air 直接返回inItem.CoolingCapacity on 20180104 by xyj
                if (inItem.Type == "Ventilation" || inItem.Type.Contains("Hydro Free") || inItem.Type == "DX-Interface")
                    return inItem.CoolingCapacity;


                string model = inItem.Model;
                string partLoadTableName = inItem.PartLoadTableName;
                if (string.IsNullOrEmpty(partLoadTableName))
                {
                    if (inItem.Flag == IndoorType.FreshAir)
                    {
                        return inItem.CoolingCapacity;
                    }
                    return 0;
                }
                string Condition = isHeating ? "Heating" : "Cooling";
                double maxdb = OutTemperature + 5;
                double mindb = OutTemperature - 5;
                double maxwb = InTemperature + 5;
                double minwb = InTemperature - 5;
                DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(partLoadTableName);
                if (dt.Rows.Count < 1)
                {
                    return 0;
                }
                DataView dv = dt.DefaultView;
                dv.RowFilter = "Condition ='" + Condition + "' and ShortModel='" + model + "' and OutDB<=" + maxdb.ToString() + " and OutDB>=" + mindb.ToString() + " and InWB<=" + maxwb.ToString() + " and InWB>=" + minwb.ToString();
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
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    int db = (dr["OutDB"] is DBNull) ? 0 : int.Parse(dr["OutDB"].ToString());
                    int wb = (dr["InWB"] is DBNull) ? 0 : int.Parse(dr["InWB"].ToString());
                    double tc = (dr["TC"] is DBNull) ? 0 : double.Parse(dr["TC"].ToString());
                    if (db == OutTemperature && wb == InTemperature)
                    {
                        return tc;
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
                        estCap = tc5 + ((OutTemperature - db1) * (tc6 - tc5) / (db2 - db1));
                    }
                    else if (db1 == db2 && wb1 == wb2)
                    {
                        estCap = tc1;
                    }
                    else if (db1 == db2)
                    {
                        estCap = tc1 + ((InTemperature - wb1) * (tc2 - tc1) / (wb2 - wb1));
                    }
                    else if (wb1 == wb2)
                    {
                        estCap = tc1 + ((OutTemperature - db1) * (tc3 - tc1) / (db2 - db1));
                    }

                }
                if (estCap == 0)
                {
                    //OutDB 温度范围不在数据表中存在 提醒 超出工况范围 on 20190308 by xyj
                    if (tc1 > 0 && tc2 > 0 && tc3 == 0 && tc4 == 0)
                    {
                        return -1;
                    }
                    //找不到partLoad数据时,并且类型为FreshAir,取Std 表数据 on 20180104 by xyj
                    if (inItem.Flag == IndoorType.FreshAir)
                    {
                        return inItem.CoolingCapacity;
                    }
                }
            }
            return estCap;
        }
        //public double CalEstCapacity(Indoor inItem, double OutTemperature, double InTemperature, out double estSH, bool isHeating) 
        //{
        //    double estCap = 0;
        //    estSH = 0;

        //    if (inItem != null)
        //    {
        //        if (inItem.Type == "Fresh Air" || inItem.Type == "Ventilation")
        //            return inItem.CoolingCapacity;


        //        string model = inItem.Model;
        //        string partLoadTableName = inItem.PartLoadTableName;
        //        if (string.IsNullOrEmpty(partLoadTableName))
        //        {
        //            return 0;
        //        }

        //        List<string> Target = new List<string> { };    //插值计算输出列表，如"Capacity","Power"等
        //        List<double> Val_Interpolate = new List<double> { }; //插值计算返回列表
        //        string Condition = isHeating ? "Heating" : "Cooling";

        //        Target.Clear();
        //        Target.Add("TC");
        //        if (!isHeating)
        //            Target.Add("SHC");
        //        Val_Interpolate = CDL.CDL.Interpolate((new GetDatabase()).GetDataAccessObject().GetConnString(), partLoadTableName,
        //            "Condition ='" + Condition + "' and ShortModel='" + model + "'", "OutDB", OutTemperature, "InWB", InTemperature, Target);
        //        if (Val_Interpolate.Count == 0)
        //        {
        //            return -1;
        //        }
        //        else
        //        {
        //            estCap = Val_Interpolate[0];
        //            if (!isHeating)
        //                estSH = Val_Interpolate[1];
        //        }
        //    }
        //    return estCap;
        //}


        /// 获取VRF数据库中标准表关系表【dbo_Relationship_Std】中的某一【标准表表名】
        /// <summary>
        /// 获取VRF数据库中标准表关系表【dbo_Relationship_Std】中的某一【标准表表名】
        /// </summary>
        /// <param name="flag">IndoorType：Indoor|FreshAir|Outdoor</param>
        /// <returns></returns>
        public string GetStdTableName(IndoorType flag)
        {
            if (flag == IndoorType.Indoor && !string.IsNullOrEmpty(_mainRegion) && (_mainRegion=="EU_W" || _mainRegion=="EU_S" || _mainRegion=="EU_E" || _subRegion == "TW"))
                return "IDU_Std_UNIVERSAL";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Relationship_Std");
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

            string colName = "TableName";
            //string sql = "SELECT DISTINCT " + colName + " as TableName From JCHVRF_Relationship_Std "
            //+ "Where SelectionType = @sType and RegionCode = @sRegion and Deleteflag = 1"; //and ProductType = @sProductType 

            DataView dv = dt.DefaultView;
            dv.RowFilter = "  SelectionType ='" + flag.ToString() + "' and RegionCode ='" + _region + "' and Deleteflag = 1";
            dt = dv.ToTable(true, colName);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0][colName].ToString();
            }
            return "";
        }

        /// 获取比指定容量更大的室内机 add on 20161121 by Yunxiao Lin
        /// <summary>
        /// 获取比指定容量更大的室内机
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="unitType"></param>
        /// <param name="factoryCode"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public Indoor getBiggerIndoor(Indoor litem)
        {
            Indoor item = null;
            if (litem == null)
                return item;
            string sTableName = GetStdTableName(IndoorType.Indoor);
            if (string.IsNullOrEmpty(sTableName))
                return null;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(sTableName);
            DataTable dtAirFlow = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_IDU_AirFlow");
            DataView dv = dt.DefaultView;
            string strFilter = "productType='" + litem.ProductType + "' and DeleteFlag=1 and UnitType='" + litem.Type + "' and CoolCapacity>'" + litem.CoolingCapacity + "'";
            if (_brandCode == "Y")
                strFilter += " and Model_York<>'-'";
            else
                strFilter += " and Model_York='-'";
            dv.RowFilter = strFilter;
            dv.Sort = "CoolCapacity asc";
            dt = dv.ToTable();
            string fc1 = litem.ModelFull.Trim().Substring(litem.ModelFull.Trim().Length - 1, 1);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string modelFull = dr["Model"].ToString().Trim();
                string fc2 = modelFull.Substring(modelFull.Length - 1, 1);

                if (fc1 == fc2)
                {
                    item = new Indoor();
                    if (dr["SelectionType"].ToString() == IndoorType.Indoor.ToString())
                    {
                        item.Model = modelFull.Substring(0, 7);
                        item.Flag = IndoorType.Indoor;
                    }
                    else
                    {
                        if (_region == "LA_BR") //巴西的新风机取前7位，其他取前8位 20170330 by Yunxiao Lin
                            item.Model = modelFull.Substring(0, 7);
                        else
                            item.Model = modelFull.Substring(0, 8);
                        item.Flag = IndoorType.FreshAir;
                    }
                    item.Series = litem.Series;
                    item.RegionCode = _region;
                    item.ProductType = litem.ProductType;
                    item.ModelFull = modelFull;
                    item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                    item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();

                    item.Type = litem.Type;
                    item.CoolingCapacity = (dr["CoolCapacity"] is DBNull) ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                    item.HeatingCapacity = (dr["HeatCapacity"] is DBNull) ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                    item.SensibleHeat = (dr["SensibleHeat"] is DBNull) ? 0 : double.Parse(dr["SensibleHeat"].ToString());
                    item.AirFlow = (dr["AirFlow"] is DBNull) ? 0 : double.Parse(dr["AirFlow"].ToString());
                    //item.AirFlow_Hi = (dr["AirFlow_Hi"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Hi"].ToString());
                    //item.AirFlow_Med = (dr["AirFlow_Med"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Med"].ToString());
                    //item.AirFlow_Lo = (dr["AirFlow_Lo"] is DBNull) ? 0 : double.Parse(dr["AirFlow_Lo"].ToString());

                    item.GasPipe = (dr["GasPipe"] is DBNull) ? "-" : dr["GasPipe"].ToString();
                    item.LiquidPipe = (dr["LiquidPipe"] is DBNull) ? "-" : dr["LiquidPipe"].ToString();
                    item.Length = (dr["Length"] is DBNull) ? "-" : dr["Length"].ToString();
                    item.Width = (dr["Width"] is DBNull) ? "-" : dr["Width"].ToString();
                    item.Height = (dr["Height"] is DBNull) ? "-" : dr["Height"].ToString();

                    item.PowerInput_Cooling = (dr["Power_Cooling"] is DBNull) ? 0 : double.Parse(dr["Power_Cooling"].ToString());
                    item.PowerInput_Heating = (dr["Power_Heating"] is DBNull) ? 0 : double.Parse(dr["Power_Heating"].ToString());
                    // wiring 图中需要标注电流文字
                    item.RatedCurrent = (dr["RatedCurrent"] is DBNull) ? 0 : double.Parse(dr["RatedCurrent"].ToString());
                    item.MCCB = (dr["MCCB"] is DBNull) ? 0 : double.Parse(dr["MCCB"].ToString());
                    item.Weight = (dr["Weight"] is DBNull) ? 0 : double.Parse(dr["Weight"].ToString());
                    item.NoiseLevel = (dr["NoiseLevel"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel"].ToString());
                    item.NoiseLevel_Hi = (dr["NoiseLevel_Hi"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Hi"].ToString());
                    item.NoiseLevel_Med = (dr["NoiseLevel_Med"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Med"].ToString());
                    item.NoiseLevel_Lo = (dr["NoiseLevel_Lo"] is DBNull) ? 0 : double.Parse(dr["NoiseLevel_Lo"].ToString());
                    // 水盘，暂无应用
                    item.DrainagePipe = (dr["DrainagePipe"] is DBNull) ? 0 : double.Parse(dr["DrainagePipe"].ToString());
                    //// 静压
                    //item.ESP = (dr["ESP"] is DBNull) ? "-" : dr["ESP"].ToString();

                    item.TypeImage = (dr["TypeImage"] is DBNull) ? "" : dr["TypeImage"].ToString();
                    item.UniqueOutdoorName = (dr["UniqueOutdoorName"] is DBNull) ? "" : dr["UniqueOutdoorName"].ToString();
                    item.PartLoadTableName = (dr["PartLoadTableName"] is DBNull) ? "" : dr["PartLoadTableName"].ToString();

                    //匹数 add on 20161109 by Yunxiao Lin
                    item.Horsepower = (dr["Horsepower"] is DBNull) ? 0 : double.Parse(dr["Horsepower"].ToString());
                    //SHF add on 20161109 by Yunxiao Lin
                    //item.SHF_Hi = (dr["SHF_Hi"] is DBNull) ? 0 : double.Parse(dr["SHF_Hi"].ToString());
                    //item.SHF_Med = (dr["SHF_Med"] is DBNull) ? 0 : double.Parse(dr["SHF_Med"].ToString());
                    //item.SHF_Lo = (dr["SHF_Lo"] is DBNull) ? 0 : double.Parse(dr["SHF_Lo"].ToString());

                    #region SHF/Air Flow/Static Pressure数据从JCHVRF_IDU_AirFlow取得 add on 20170703 by Shen Junjie

                    DataRow[] drsAirFlow = dtAirFlow.Select("Model_Hitachi='" + item.Model_Hitachi + "'");

                    if (drsAirFlow.Length > 0)
                    {
                        DataRow drAirFlow = drsAirFlow[0];
                        item.AirFlow_Levels = new double[]{
                            (drAirFlow["AirFlow_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Hi2"].ToString()),
                            (drAirFlow["AirFlow_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Hi"].ToString()),
                            (drAirFlow["AirFlow_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Med"].ToString()),
                            (drAirFlow["AirFlow_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["AirFlow_Lo"].ToString())
                        };
                        item.ESP_Levels = new double[]{
                            (drAirFlow["StaticPressure_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Hi2"].ToString()),
                            (drAirFlow["StaticPressure_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Hi"].ToString()),
                            (drAirFlow["StaticPressure_Med"] is DBNull) ? 0 : double.Parse(dr["StaticPressure_Med"].ToString()),
                            (drAirFlow["StaticPressure_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["StaticPressure_Lo"].ToString())
                        };
                        item.SHF_Levels = new double[]{
                            (drAirFlow["SHF_Hi2"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Hi2"].ToString()),
                            (drAirFlow["SHF_Hi"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Hi"].ToString()),
                            (drAirFlow["SHF_Med"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Med"].ToString()),
                            (drAirFlow["SHF_Lo"] is DBNull) ? 0 : double.Parse(drAirFlow["SHF_Lo"].ToString())
                        };
                    }

                    #endregion
                    ////注意，自动重选的室内机要沿用之前的配件
                    //if (litem.ListAccessory != null)
                    //    item.ListAccessory = litem.ListAccessory;
                    break;
                }
            }
            return item;
        }

        public string GetDefaultDuctedUnitType(string region, string brand, string series)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_DefaultDuctedUnitType");
            DataView dv = dt.DefaultView;
            string strFilter = "Region='" + region + "' and Brand='" + brand + "' and Series='" + series + "'";
            dv.RowFilter = strFilter;
            dt = dv.ToTable();
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                return dr["DefaultUnitType"] is DBNull ? null : dr["DefaultUnitType"].ToString();
            }
            return null;
        }

        /// <summary>
        /// 获取电源供应列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetPowerSupplyList()
        {

            string query = "SELECT Type,Code,Name,Name_cn as ModelType From JCHVRF_Dictionary  where Type='PowerSupply'";
            DataTable dt = _dao.GetDataTable(query);
            DataView dv = dt.DefaultView;
            return dv.ToTable();

        }

        /// 获取当前可选的通用室内机显示类型名称
        /// <summary>
        /// 获取当前可选的通用室内机显示类型名称
        /// </summary>
        /// <returns></returns>
        public DataTable GetIndoorDisplayName()
        {
            //string sTableName = getStdName(_mainRegion);

            //if (!string.IsNullOrEmpty(sTableName))
            //{
            //    string query = "SELECT t1.Display_Name,t2.* from JCHVRF_Universal_Display t1 "
            //        + "left join " + sTableName + " t2 on t1.Universal_Name = t2.UnitType "
            //        + "where t1.RegionCode like '*" + _region + "*' and t1.BrandCode = '" + _brandCode + "' and t1.DeleteFlag = 1 and t2.DeleteFlag = 1";
            //    DataTable dt = _dao.GetDataTable(query);
            //    return dt;
            //}
            //return null;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Universal_Display");
            DataView dv = dt.DefaultView;
            string strFilter = "RegionCode like '*/" + _region + "/*' and BrandCode = '" + _brandCode + "' and SelectionType = '" + IndoorType.Indoor + "' and DeleteFlag = 1";
            dv.RowFilter = strFilter;
            dv.Sort = "Display_Name";
            return dv.ToTable(true, new string[] { "Display_Name" });
        }

        public DataTable GetIndoorDisplayNameForODUSeries(string ODUSeries)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("Display_Name", typeof(string));
            var dtIduUniversal = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Universal_Display");
            DataView dv = dtIduUniversal.DefaultView;
            string strFilter = "RegionCode like '*/" + _region + "/*' and BrandCode = '" + _brandCode + "' and SelectionType = '" + IndoorType.Indoor + "' and DeleteFlag = 1";
            dv.RowFilter = strFilter;
            dv.Sort = "Display_Name";
            dtIduUniversal = dv.ToTable(true, new string[] { "Display_Name", "Universal_Name" });

            var dtIduTypeBySeries = FilterIduTypeBySeries(ODUSeries);
            var result = dtIduUniversal.AsEnumerable().Join(dtIduTypeBySeries.AsEnumerable(),
                     dtUni => dtUni.Field<string>("Universal_Name")
                   , dtIdu => dtIdu.Field<string>("IDU_UnitType"),
                   (dtUni, dtIdu) => new { Display_Name = dtUni }).ToArray();
            if (result.Count() > 0)
            {                
                foreach (var rows in result)
                {
                    dtResult.Rows.Add(rows.Display_Name.ItemArray[0]);
                }
            }
            if (dtResult.Rows.Count > 0)
                return dtResult.DefaultView.ToTable(true, "Display_Name");
            else
                return dtIduUniversal;

        }
        private DataTable FilterIduTypeBySeries(string ODUSeries)
        {
            MyProductTypeDAL _dal = new MyProductTypeDAL();

            return _dal.GetIduTypeBySeries(JCHVRF.Model.Project.GetProjectInstance.BrandCode, JCHVRF.Model.Project.GetProjectInstance.SubRegionCode, ODUSeries);

        }

        public DataTable GetIduDisplayName()
        {
            DataTable dtIndoor = new DataTable();
            string query = string.Empty;
            if (!string.IsNullOrEmpty(_mainRegion) && (_mainRegion=="EU_W" || _mainRegion == "EU_E" || _mainRegion == "EU_S" || _subRegion == "TW"))
            {
                query = "Select t2.Display_Name as UnitType, t1.TypeImage, 'Indoor' as SelectionType from IDU_Std_UNIVERSAL t1 left join JCHVRF_Universal_Display t2 on t1.UnitType = t2.Universal_Name"
                 + " Where t1.RegionCode like '%/" + _region + "/%' and t2.RegionCode like '%/" + _region + "/%' and t1.DeleteFlag = 1 Order by t2.Display_Name";
            }
            else
            {
                string sTableName = GetStdTableName(IndoorType.Indoor);
                if (!string.IsNullOrEmpty(sTableName))
                {
                    query = "Select t1.UnitType,t1.TypeImage,'Indoor' as SelectionType from " + sTableName + " t1"
                     + " Where t1.RegionCode='" + _region + "' and t1.DeleteFlag = 1 Order by t1.UnitType";
                }

            }
            dtIndoor = _dao.GetDataTable(query);
            getPipingEquipments(ref dtIndoor);
            return dtIndoor;
        }
        public void getPipingEquipments(ref DataTable table)
        {            
            table.Rows.Add(new Object[] { "Y Branch Seperator", "YBranch_Seperator.png","Pipe" });
            table.Rows.Add(new Object[] { "Header Branch 4 Seperator", "HeaderBranch_4Seperator.png", "Pipe" });
            table.Rows.Add(new Object[] { "Header Branch 8 Seperator", "HeaderBranch_8Seperator.png", "Pipe" });
            table.Rows.Add(new Object[] {"CHBox", "CHBox.png","Pipe" });
            table.Rows.Add(new Object[] { "Multi CHBox", "MultiCHBox.png", "Pipe" });
        }
        /// 获取当前unitType的通用室内机的工厂名
        /// <summary>
        /// 获取当前unitType的通用室内机的工厂名
        /// </summary>
        /// <returns></returns>
        public string GetFCodeByDisUnitType(string display_Name, out List<string> typeList)
        {
            string fCode = "";
            typeList = new List<string>();
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Universal_Display");
            DataView dv = dt.DefaultView;
            string strFilter = "Display_Name = '" + display_Name + "' and RegionCode like '*/" + _region + "/*' and BrandCode = '" + _brandCode + "'"
                                + "and SelectionType = '" + IndoorType.Indoor + "' and DeleteFlag = 1";
            dv.RowFilter = strFilter;
            dt = dv.ToTable();
            if (dt.Rows.Count > 0)
            {
                //获取工厂号
                DataRow dr = dt.Rows[0];
                fCode = dr["FactoryCode"] is DBNull ? null : dr["FactoryCode"].ToString();
                foreach (DataRow rowItem in dt.Rows)
                {
                    string unitType = rowItem["Universal_Name"].ToString();
                    if (!typeList.Contains(unitType))
                        typeList.Add(unitType);

                }
            }
            if (string.IsNullOrEmpty(fCode))
                return null;

            return fCode;

        }

        /// 增加factoryName参数 20161118 by Yunxiao Lin
        /// 获取室内机标准表
        /// <summary>
        /// 获取室内机标准表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable GetListStd(string displayName, string factoryCode, List<string> typeList)
        {
            // 此处GetStdTableName中的参数flag直接用“Indoor”，因为Indoor与FreshAir的数据放在同一张标准表中
            // update on 20120424 clh
            string sTableName = GetStdTableName(IndoorType.Indoor);
            if (!string.IsNullOrEmpty(sTableName))
            {
                string shortModel = "Mid(Model,1,7)";
                string unitType = "";
                if (typeList.Count != 0)
                {
                    foreach (string type in typeList)   //比对通过displayName 获取的type对象特殊处理
                    {
                        if (type.Contains("YDCF"))
                        {
                            shortModel = "Mid(Model,1,8)";
                            unitType = type;
                            break;
                        }
                    }
                }

                //if (type.Contains("YDCF"))
                //    shortModel = "Mid(Model,1,8)";

                string query = "SELECT DISTINCT " + shortModel + " as Model, t1.Model as ModelFull, t1.Model_York, t1.Model_Hitachi, "
                    //+ "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName From "
                    + "CoolCapacity, SensibleHeat, AirFlow, HeatCapacity, TypeImage, UniqueOutdoorName,Horsepower,SHF_Hi,SHF_Med,SHF_Lo From "
                    + sTableName + " t1 left join JCHVRF_Universal_Display t2 on t1.UnitType = t2.Universal_Name"
                    + " Where t2.Display_Name='" + displayName + "' and t1.RegionCode like '%/" + _region + "/%' and t2.RegionCode like '%/" + _region + "/%' and t1.DeleteFlag = 1";
                if (_brandCode == "Y")
                    query += " and t1.Model_York<>'-'";
                else
                    query += " and t1.Model_York='-'";
                //增加factoryCode判断 20161118 by Yunxiao Lin               
                query += " and right(trim(Model),1)='" + factoryCode + "' Order by CoolCapacity";

                DataTable dt = _dao.GetDataTable(query);

                foreach (DataRow dr in dt.Rows)
                {
                    double cap = dr["CoolCapacity"] is DBNull ? 0 : double.Parse(dr["CoolCapacity"].ToString());
                    double cap_h = dr["HeatCapacity"] is DBNull ? 0 : double.Parse(dr["HeatCapacity"].ToString());
                    double heat = dr["SensibleHeat"] is DBNull ? 0 : double.Parse(dr["SensibleHeat"].ToString());
                    double flow = dr["AirFlow"] is DBNull ? 0 : double.Parse(dr["AirFlow"].ToString());
                    double hp = dr["Horsepower"] is DBNull ? 0 : double.Parse(dr["Horsepower"].ToString());
                    double shf_hi = dr["SHF_Hi"] is DBNull ? 0 : double.Parse(dr["SHF_Hi"].ToString());
                    double shf_med = dr["SHF_Med"] is DBNull ? 0 : double.Parse(dr["SHF_Med"].ToString());
                    double shf_lo = dr["SHF_Lo"] is DBNull ? 0 : double.Parse(dr["SHF_Lo"].ToString());
                    dr["CoolCapacity"] = cap;
                    dr["SensibleHeat"] = heat;
                    dr["AirFlow"] = flow;
                    dr["HeatCapacity"] = cap_h;
                    dr["Horsepower"] = hp;
                    dr["SHF_Hi"] = shf_hi;
                    dr["SHF_Med"] = shf_med;
                    dr["SHF_Lo"] = shf_lo;
                }

                // Fresh Air 类型中存在同Model的记录，需要特殊处理
                DataView dv = dt.DefaultView;

                if (unitType == "Fresh Air")
                    dv.RowFilter = "ModelFull like '%1080%' or UniqueOutdoorName<>''";


                return dv.ToTable();
            }
            return null;
        }


        /// 获取当前unitType的通用室内机的工厂名
        /// <summary>
        /// 获取当前unitType的通用室内机的工厂名
        /// </summary>
        /// <param name="unitType"> 当为NULL值返回新风机的displayName，当有值返回改unitType对应的displayName </param>
        /// <returns></returns>
        public string GetDisplayNameStr(string unitType, string factoryCode, out bool isFA)
        {
            isFA = false;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Universal_Display");
            DataView dv = dt.DefaultView;
            string faDisplayName = "";
            string strFilter = "RegionCode like '*/" + _region + "/*' and BrandCode = '" + _brandCode + "' and FactoryCode='" + factoryCode + "' and SelectionType = '" + IndoorType.Indoor + "' and DeleteFlag = 1";
            if (string.IsNullOrEmpty(unitType))
            {
                strFilter += " and Universal_Name in ('YDCF', 'Fresh Air', 'Ventilation')";
                isFA = true;
            }
            else
                strFilter += " and Universal_Name = '" + unitType + "'";
            dv.RowFilter = strFilter;
            dv.Sort = "Display_Name";
            dt = dv.ToTable(true, new string[] { "Display_Name" });
            if (isFA)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    faDisplayName += dr["Display_Name"].ToString() + ",";

                }
                if (string.IsNullOrEmpty(faDisplayName))
                    return null;
                return faDisplayName.Substring(0, faDisplayName.Length - 1);
            }
            else
                return dt.Rows[0]["Display_Name"].ToString();


        }

        public DataTable GetIndoorDisplayNameRegionWise(string productType)
        {
            string sTableName = GetStdTableName(IndoorType.Indoor);

            if (!string.IsNullOrEmpty(sTableName))
            {

                //string query = "SELECT DISTINCT " + colName + " From " + sTableName
                //    + " Where ProductType= '" + _productType + "' and Deleteflag=1"; //RegionCode = '" + _region + "' and 
                //多productType修改 20160821 by Yunxiao Lin
                string strtemp = " and Model_York='-'";
                if (_brandCode == "Y")
                    strtemp = " and Model_York<>'-'";
                string query = "select t1.*,t2.FactoryCount from ( select distinct right(trim(model),1) as FactoryCode,UnitType from " + sTableName + " where ProductType='" + productType + "' and deleteFlag=1 " + strtemp + ") as t1 "
                       + "inner join (select UnitType, count(UnitType) as factoryCount from (select distinct  right(trim(model),1) as FactorCode,UnitType from " + sTableName + " where ProductType='" + productType + "' and deleteFlag=1 " + strtemp + ") "
                + "group by UnitType ) as t2 on t1.UnitType = t2.UnitType "
                + "order by t1.UnitType, t1.FactoryCode";
                DataTable dt = _dao.GetDataTable(query);
                return dt;
            }
            return null;
        }

    }
}
