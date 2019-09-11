using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.DALFactory;
using JCHVRF.Model;
using System.Text.RegularExpressions;
using NextGenModel = JCHVRF.Model.NextGen;

namespace JCHVRF.DAL
{
    public class PipingDAL : IPipingDAL
    {
        IDataAccessObject _dao;
        public List<string> ListPipeSize;
        public List<string> ListPipeSize_inch;
        public List<double> ListElbowLength;
        public List<double> ListOilLength;
        Project thisProject;

        public string JOINTKIT_PRICE { get { return "PipingJointKitPrice"; } }

        public PipingDAL(Project thisProject)
        {
            _dao = (new GetDatabase()).GetDataAccessObject();

            if (ListPipeSize == null)
            {
                ListPipeSize = new List<string>();
                ListPipeSize_inch = new List<string>();
                ListElbowLength = new List<double>();
                ListOilLength = new List<double>();
                this.thisProject = thisProject;

                //string sql = "select * from " + TB_Correspondence;
                //DataTable dt = _dao.GetDataTable(sql);


                DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_Correspondence);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ListPipeSize.Add(dr["mm"].ToString().Trim());
                        ListPipeSize_inch.Add(dr["inch"].ToString().Trim());
                        ListElbowLength.Add(double.Parse(dr["ElbowLength"].ToString()));
                        ListOilLength.Add(double.Parse(dr["OilLength"].ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// 获取不同Region不同型号的分歧管对应的价格，返回值为 PriceG|PriceL 数组
        /// </summary>
        /// <param name="_region"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string GetJointKitPrice(string model, string _region)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(JOINTKIT_PRICE);
            DataView dv = dt.DefaultView;
            dv.RowFilter = " Region = '" + _region + "' and JointKitModel = '" + model + "'";
            dt = dv.ToTable(true, "Price");

            //string query = "SELECT DISTINCT Price From " + JOINTKIT_PRICE
            //    + " Where Region = '" + _region + "' and JointKitModel = '" + model + "'";
            //DataTable dt = _dao.GetDataTable(query);

            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                return dr[0].ToString();
            }
            return "-";
        }

        // clh 20160315 for HitachiVRF

        string TB_ConnectionKit = "JCHVRF_Piping_ConnectionKit";
        string TB_ChangeOverKit = "JCHVRF_Piping_ChangeOverKit";
        string TB_Multi_CHBox = "JCHVRF_Piping_Multi_CH_BOX";
        string TB_PipingImage = "JCHVRF_Piping_Image";
        string TB_FirstBranchKit = "JCHVRF_Piping_FirstBranchKit";
        string TB_TertiaryBranchKit = "JCHVRF_Piping_TertiaryBranchKit";
        string TB_HeaderBranch = "JCHVRF_Piping_HeaderBranch";
        string TB_PipeSizeIDU = "JCHVRF_Piping_PipeSize_IDU";
        //string TB_PipeSizeODU = "";
        string TB_Correspondence = "JCHVRF_Piping_Correspondence";
        string TB_PipingLengthTableName = "JCHVRF_PipingLengthTableName";


        public PipingBranchKit GetConnectionKit(Outdoor outItem)
        {
            string modelFull = outItem.ModelFull;
            string factoryCode = modelFull.Substring(modelFull.Length - 1, 1);
            //string type = modelFull.Substring(3, 1) == "R" ? "3Pipe" : "2Pipe";
            string type = outItem.ProductType.Contains("Heat Recovery") || outItem.ProductType.Contains(", HR") ? "3Pipe" : "2Pipe";
            double capacity = outItem.CoolingCapacity;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_ConnectionKit);
            DataView dv = dt.DefaultView;
            dv.RowFilter = " FactoryCode='" + factoryCode + "' and Type='" + type + "'"
                + " and MinCapacity <=" + Math.Round(capacity, 3) + " and MaxCapacity >= " + Math.Round(capacity, 3);

            //印度专享，remark为India.   add on 20180212 by Vince
            //if (thisProject.RegionCode == "INDIA" && outItem.Series == "Commercial VRF HP, HNCQ")
            //    dv.RowFilter += " and Remark = 'India'";
            //else
            //    dv.RowFilter += " and isnull(Remark,'') <> 'India' ";

            dt = dv.ToTable();

            //string sql = "select * from " + TB_ConnectionKit + " where FactoryCode='" + factoryCode + "' and Type='" + type + "'"
            //    + " and MinCapacity <=" + Math.Round(capacity, 3) + " and MaxCapacity >= " + Math.Round(capacity, 3);

            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string remark = dr["Remark"].ToString();

                    // 需要区分Model的情况
                    if (dt.Rows.Count > 1 && !string.IsNullOrEmpty(remark))
                    {
                        string[] arr = remark.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in arr)
                        {
                            if (outItem.Model_Hitachi.Contains(s))
                                break;
                            else
                                continue;
                        }
                    }

                    PipingBranchKit item = new PipingBranchKit();
                    item.FactoryCode = factoryCode;
                    item.Type = type;
                    item.UnitType = outItem.Type;
                    item.Model_York = dr["Model_York"].ToString();
                    item.Model_Hitachi = dr["Model_Hitachi"].ToString();
                    item.PipingSets = dr["PipingSets"] is DBNull ? 0 : int.Parse(dr["PipingSets"].ToString());
                    item.Capacity = capacity;
                    item.PartNumber = dr["PartNumber"].ToString();
                    return item;
                }
            }
            return null;
        }

        // 增加系统IDU数量限制，为0则没有限制 20170704 by Yunxiao Lin
        public PipingBranchKit GetFirstBranchKit(string factoryCode, string type, string unitType, double capacity, string sizeUP, bool isMal, int IDUNum, string region)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_FirstBranchKit);
            DataView dv = dt.DefaultView;
            string filter = " FactoryCode='" + factoryCode + "' and Type='" + type + "'"
                    + " and UnitType='" + unitType + "'";

            if (region != "TW")
            {
                //台湾区域筛选region为TW的数据,其它区域筛选region为OTHERS的数据 modified by Shen Junjie on 2018/8/8
                region = "OTHERS";
            }
            // This code is commented because database dont hhave Region column
            //<Sandeep Kushwah> Start Bug #2428 Fix - Uncommneted below line of code and also applied null check to get the required kit on piping. This was to fix bug number 2428.
            if (!string.IsNullOrEmpty(region))
            {
                filter += " and Region like '*/" + region + "/*'";
            }
            //<Sandeep Kushwah> End Bug #2428 Fix.

            if (isMal)
                //filter += " and Model_York like '%QE'";
                //RDH-01QE==>JRDA10NEWS, R4H-01QE==>JR4A11NEWQ 20161128 by Yunxiao Lin 
                filter += " and (Model_York like '%QE' or Model_York='JRDA10NEWS' or Model_York='JR4A11NEWQ')";
            else
                filter += " and Model_York not like '%QE'";
            // 增加系统IDU数量限制，为0则没有限制 20170704 by Yunxiao Lin
            if (IDUNum > 0)
                filter += " and IDUNumber=" + IDUNum.ToString();

            //印度专享，remark为India.   add on 20180212 by Vince
            //if (thisProject.RegionCode == "INDIA" && unitType == "HNCQ (Top flow, 3Ph)")
            //    filter += " and Remark = 'India'";
            //else
            //    filter += " and isnull(Remark,'') <> 'India' ";

            string capacityFilter = "";
            if (capacity != 0)
                capacityFilter = " and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity >= " + Math.Round(capacity, 1);

             
            dv.RowFilter = filter + capacityFilter;
            dt = dv.ToTable();
            //string sql = "select * from " + TB_FirstBranchKit + " where FactoryCode='" + factoryCode + "' and Type='" + type + "'"
            //    + " and UnitType='" + unitType + "' and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity >= " + Math.Round(capacity, 1);

            //DataTable dt = _dao.GetDataTable(sql);

            bool isGetMaxCapacity = false;
            if (capacity != 0 && dt != null && dt.Rows.Count == 0)
            {
                //如果是因为容量区间不连续造成的未找到数据，只判断最大值 20160921 by Yunxiao Lin
                capacityFilter = " and MaxCapacity >= " + Math.Round(capacity, 1);
                dv.RowFilter = filter + capacityFilter;
                dt = dv.ToTable();
                if (dt != null && dt.Rows.Count == 0)
                {
                    //如果实在找不到合适的第一分歧管，选最大的型号 20161017 by Yunxiao Lin
                    dv.RowFilter = filter;
                    dt = dv.ToTable();
                    isGetMaxCapacity = true;
                }
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                double max_capacity = 0;
                PipingBranchKit item = new PipingBranchKit();
                foreach (DataRow dr in dt.Rows)
                {
                    double mc = 0;
                    Double.TryParse(dr["MaxCapacity"].ToString(), out mc);
                    if (!isGetMaxCapacity || max_capacity < mc)
                    {
                        if (dr["SizeUP"].ToString() == sizeUP)
                            max_capacity = mc;
                        item.FactoryCode = factoryCode;
                        item.Type = type;
                        item.UnitType = unitType;
                        item.Model_York = dr["Model_York"].ToString();
                        item.Model_Hitachi = dr["Model_Hitachi"].ToString();
                        item.PipingSets = 1;
                        item.Capacity = capacity;
                        item.LiquidPipe = dr["LiquidPipe"].ToString();
                        item.HighPressureGasPipe = dr["HighPressureGasPipe"].ToString();
                        item.LowPressureGasPipe = dr["LowPressureGasPipe"].ToString();
                        item.SizeUP = dr["SizeUP"].ToString();
                        item.PartNumber = dr["PartNumber"].ToString();

                        if (!isGetMaxCapacity && item.SizeUP == sizeUP)
                            return item;
                    }
                }
                return item;
            }
            return null;
        }
        // 增加IDUNum限制，为0则没有数量限制 20170704 by Yunxiao Lin
        public PipingBranchKit GetTertiaryBranchKit(string factoryCode, string type, string unitType, double capacity, string sizeUP, bool isMal, int IDUNum, string region)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_TertiaryBranchKit);
            DataView dv = dt.DefaultView;
            //此处查询MaxCapacity用>=可能漏查数据，改为>，同时基础库MaxCapacity字段做+0.1处理 by Yunxiao Lin 20160429
            if (capacity != 0)
                dv.RowFilter = "FactoryCode='" + factoryCode + "' and Type='" + type + "'"
                    + " and UnitType='" + unitType + "'"
                    + " and ((MinCapacity<>MaxCapacity and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity > " + Math.Round(capacity, 1) + ") or (MinCapacity=MaxCapacity) and MinCapacity=" + Math.Round(capacity, 1) + ")"
                    //增加sizeUp判断，某些型号的分歧管可能需要放大一号 by Yunxiao Lin 20160529
                    + " and SizeUp='" + sizeUP + "'";
            else
                dv.RowFilter = "FactoryCode='" + factoryCode + "' and Type='" + type + "'"
                    + " and UnitType='" + unitType + "'"
                    + " and SizeUp='" + sizeUP + "'";

            //其它区域的数据和ANZ的重合部分都统一都用了Horse Power选型，所以可以合并。所以删除ANZ的特殊筛选。delete by Shen Junjie on 2018/9/18
            //if (region != "TW" && region != "ANZ")
            if (region != "TW")
            {
                //台湾区域筛选region为TW的数据,其它区域筛选region为OTHERS的数据 modified by Shen Junjie on 2018/8/8
                region = "OTHERS";
            }
            // THis is commented  no Region column in database 
           // dv.RowFilter += " and Region like '*/" + region + "/*'";

            if (isMal)
                //dv.RowFilter += " and Model_York like '%QE'";
                //RDH-01QE==>JRDA10NEWS, R4H-01QE==>JR4A11NEWQ 20161128 by Yunxiao Lin 
                dv.RowFilter += " and (Model_York like '%QE' or Model_York='JRDA10NEWS' or Model_York='JR4A11NEWQ')";
            else
                dv.RowFilter += " and Model_York not like '%QE'";
            // 增加IDUNum限制，默认为0则没有数量限制，用于IVX选型 20170704 by Yunxiao Lin
            if (IDUNum > 0)
                dv.RowFilter += "and IDUNumber=" + IDUNum.ToString();
            //dv.RowFilter = "FactoryCode='" + factoryCode + "' and Type='" + type + "'"
            //+ " and UnitType='" + unitType + "' and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity >= " + Math.Round(capacity, 1);

            //印度专享，remark为India.   add on 20180212 by Vince
            //if (thisProject.RegionCode == "INDIA" && unitType == "HNCQ (Top flow, 3Ph)")
            //    dv.RowFilter += " and Remark = 'India'";
            //else
            //    dv.RowFilter += " and isnull(Remark,'') <> 'India' ";

            dt = dv.ToTable();

            //string sql = "select * from " + TB_TertiaryBranchKit + " where FactoryCode='" + factoryCode + "' and Type='" + type + "'"
            //    + " and UnitType='" + unitType + "' and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity >= " + Math.Round(capacity, 1);

            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    PipingBranchKit item = new PipingBranchKit();
                    item.FactoryCode = factoryCode;
                    item.Type = type;
                    item.UnitType = unitType;
                    item.Model_York = dr["Model_York"].ToString();
                    item.Model_Hitachi = dr["Model_Hitachi"].ToString();
                    item.PipingSets = 1;
                    item.Capacity = capacity;
                    item.LiquidPipe = dr["LiquidPipe"].ToString();
                    item.HighPressureGasPipe = dr["HighPressureGasPipe"].ToString();
                    item.LowPressureGasPipe = dr["LowPressureGasPipe"].ToString();
                    item.SizeUP = dr["SizeUP"].ToString();
                    item.PartNumber = dr["PartNumber"].ToString();

                    if (dt.Rows.Count == 1 || item.SizeUP == sizeUP)
                        return item;
                }
            }
            return null;
        }

        // 增加IDUNum限制，为0则没有数量限制 20170704 by Yunxiao Lin
        public PipingBranchKit ShrinkTertiaryBranchKit(PipingBranchKit branchKit, bool isHitachi, bool IsCoolingonly, string sizeUP, bool isMal, int IDUNum, string region)
        {
            double capacity = branchKit.Capacity;
            string factoryCode = branchKit.FactoryCode;
            string type = branchKit.Type;
            string unitType = branchKit.UnitType;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_TertiaryBranchKit);
            DataView dv = dt.DefaultView;
            string filters = "FactoryCode='" + factoryCode + "' and Type='" + type + "'"
                    + " and UnitType='" + unitType + "'"
                    //增加sizeUp判断，某些型号的分歧管可能需要放大一号 by Yunxiao Lin 20160529
                    + " and SizeUp='" + sizeUP + "'";
            //此处查询MaxCapacity用>=可能漏查数据，改为>，同时基础库MaxCapacity字段做+0.1处理 by Yunxiao Lin 20160429
            //filters += " and ((MinCapacity<>MaxCapacity and MinCapacity <=" + Math.Round(capacity, 1)
            //    + " and MaxCapacity > " + Math.Round(capacity, 1)
            //    + ") or (MinCapacity=MaxCapacity) and MinCapacity=" + Math.Round(capacity, 1) + ")"; 
            if (isMal)
                //dv.RowFilter += " and Model_York like '%QE'";
                //RDH-01QE==>JRDA10NEWS, R4H-01QE==>JR4A11NEWQ 20161128 by Yunxiao Lin 
                filters += " and (Model_York like '%QE' or Model_York='JRDA10NEWS' or Model_York='JR4A11NEWQ')";
            else
                filters += " and Model_York not like '%QE'";
            //dv.RowFilter = "FactoryCode='" + factoryCode + "' and Type='" + type + "'"
            //+ " and UnitType='" + unitType + "' and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity >= " + Math.Round(capacity, 1);
            // 增加IDUNum限制，默认为0则没有数量限制，用于IVX选型 20170704 by Yunxiao Lin
            if (IDUNum > 0)
                dv.RowFilter += "and IDUNumber=" + IDUNum.ToString();

            //印度专享，remark为India.   add on 20180212 by Vince
            //if (thisProject.RegionCode == "INDIA" && unitType == "HNCQ (Top flow, 3Ph)")
            //    filters += " and Remark = 'India'";
            //else
            //    filters += " and isnull(Remark,'') <> 'India' ";
            if (region != "TW")
            {
                //台湾区域筛选region为TW的数据,其它区域筛选region为OTHERS的数据 modified by Shen Junjie on 2018/8/8
                region = "OTHERS";
            }
            dv.RowFilter += " and Region like '*/" + region + "/*'";

            dv.RowFilter = filters;
            dv.Sort = "MinCapacity DESC";
            dt = dv.ToTable();

            //string sql = "select * from " + TB_TertiaryBranchKit + " where FactoryCode='" + factoryCode + "' and Type='" + type + "'"
            //    + " and UnitType='" + unitType + "' and MinCapacity <=" + Math.Round(capacity, 1) + " and MaxCapacity >= " + Math.Round(capacity, 1);

            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    PipingBranchKit item = new PipingBranchKit();
                    item.FactoryCode = factoryCode;
                    item.Type = type;
                    item.UnitType = unitType;
                    item.Model_York = dr["Model_York"].ToString();
                    item.Model_Hitachi = dr["Model_Hitachi"].ToString();
                    item.PipingSets = 1;
                    item.Capacity = capacity;
                    item.LiquidPipe = dr["LiquidPipe"].ToString().Trim();
                    item.HighPressureGasPipe = dr["HighPressureGasPipe"].ToString().Trim();
                    item.LowPressureGasPipe = dr["LowPressureGasPipe"].ToString().Trim();
                    item.SizeUP = dr["SizeUP"].ToString();
                    item.PartNumber = dr["PartNumber"].ToString();

                    string model1 = isHitachi ? branchKit.Model_Hitachi : branchKit.Model_York;
                    string model2 = isHitachi ? item.Model_Hitachi : item.Model_York;
                    double specL1, specL2, specG_h1, specG_h2, specG_l1, specG_l2;

                    //如果比上一层型号或管径大，则缩小一号
                    int HP1 = 0, HP2 = 0; //从Model 中获取匹数
                    Match match1 = Regex.Match(model1, "\\d+");
                    if (match1 != null)
                    {
                        HP1 = int.Parse(match1.Value);
                    }
                    Match match2 = Regex.Match(model2, "\\d+");
                    if (match1 != null)
                    {
                        HP2 = int.Parse(match2.Value);
                    }
                    if (HP1 < HP2
                        || (double.TryParse(branchKit.LiquidPipe, out specL1) && double.TryParse(item.LiquidPipe, out specL2)
                            && specL1 < specL2)
                        || (double.TryParse(branchKit.HighPressureGasPipe, out specG_h1) && double.TryParse(item.HighPressureGasPipe, out specG_h2)
                            && specG_h1 < specG_h2)
                        || (double.TryParse(branchKit.LowPressureGasPipe, out specG_l1) && double.TryParse(item.LowPressureGasPipe, out specG_l2)
                            && specG_l1 < specG_l2))
                    {
                        continue;
                    }
                    return item;
                }
            }
            return null;
        }

        /// 20170513 梳形管查询增加ODUUnitType参数 by Yunxiao lin
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="unitType"></param>
        /// <param name="capacity"></param>
        /// <param name="sizeUP">TRUE|FALSE</param>
        /// <param name="currentBranchCount"></param>
        /// <returns></returns>
        public PipingHeaderBranch GetHeaderBranch(string type, double capacity, string sizeUP, int currentBranchCount, string ODUUnitType, string region)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_HeaderBranch);
            DataView dv = dt.DefaultView;
            string filter = "Type='" + type + "' and SizeUP = '" + sizeUP + "'"
                + " and MinCapacity <=" + Math.Round(capacity, 1)
                + " and MaxCapacity >= " + Math.Round(capacity, 1)
                + " and UnitType='" + ODUUnitType + "'";

            //其它区域的数据和ANZ的重合部分都统一都用了Horse Power选型，所以可以合并。所以删除ANZ的特殊筛选。delete by Shen Junjie on 2018/9/18
            //if (region != "ANZ")
            //{
            //    //ANZ区域筛选region为ANZ的数据,其它区域筛选region为OTHERS的数据 modified by Shen Junjie on 2018/8/8
            //    region = "OTHERS";
            //}
            region = "OTHERS";  //目前所有区域都是others add by Shen Junjie on 2018/9/18
            filter += " and Region like '*/" + region + "/*'";

            dv.RowFilter = filter;
            dv.Sort = "MinCapacity asc, MaxBranches asc";
            dt = dv.ToTable();

            //string sql = "select distinct * from " + TB_HeaderBranch + " where Type='" + type + "' and SizeUP = '" + sizeUP + "'"
            //    + " and MinCapacity <=" + Math.Round(capacity, 1)
            //    + " and MaxCapacity >= " + Math.Round(capacity, 1);
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    PipingHeaderBranch item = new PipingHeaderBranch();
                    item.FactoryCode = dr["FactoryCode"].ToString();
                    item.Type = type;
                    item.UnitType = dr["UnitType"].ToString();
                    item.Model_York = dr["Model_York"].ToString();
                    item.Model_Hitachi = dr["Model_Hitachi"].ToString();
                    item.Capacity = capacity;
                    item.LiquidPipe = dr["LiquidPipe"].ToString();
                    item.HighPressureGasPipe = dr["HighPressureGasPipe"].ToString();
                    item.LowPressureGasPipe = dr["LowPressureGasPipe"].ToString();
                    item.MaxBranches = dr["MaxBranches"] is DBNull ? 0 : int.Parse(dr["MaxBranches"].ToString());
                    item.SizeUP = dr["SizeUP"].ToString();
                    item.PartNumber = dr["PartNumber"].ToString();

                    if (item.MaxBranches >= currentBranchCount)
                        return item;
                }
            }
            return null;
        }
        /// 获取CH-Box，增加室内机数量限制 Modify on 20160529 by Yunxiao Lin
        /// 增加Series参数, 20170302 by Yunxiao Lin
        /// <summary>
        /// 获取CH-Box
        /// </summary>
        /// <param name="factoryCode"></param>
        /// <param name="capacity"></param>
        /// <param name="sizeUP"></param>
        /// <param name="IUCount"></param>
        /// <returns></returns>
        public PipingChangeOverKit GetChangeOverKit(string factoryCode, double capacity, string sizeUP, int IUCount, string Series, string region)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_ChangeOverKit);
            DataView dv = dt.DefaultView;
            string filter = " FactoryCode='" + factoryCode + "' and SizeUP='" + sizeUP
                + "' and MinCapacity <=" + Math.Round(capacity, 3)
                + " and MaxCapacity >= " + Math.Round(capacity, 3)
                + " and MinIU<=" + IUCount.ToString()
                + " and MaxIU>=" + IUCount.ToString()
                + " and Series like '%;" + Series + ";%'";

            if (region != "ANZ")
            {
                //台湾区域筛选region为TW的数据,其它区域筛选region为OTHERS的数据 modified by Shen Junjie on 2018/8/8
                region = "OTHERS";
            }
           // filter += " and Region like '*/" + region + "/*'";

            dv.RowFilter = filter;
            dv.Sort = "MinCapacity ASC";
            dt = dv.ToTable();


            //string sql = "select * from " + TB_ChangeOverKit + " where FactoryCode='" + factoryCode + "' and SizeUP='" + sizeUP 
            //    + "' and MinCapacity <=" + Math.Round(capacity, 3)
            //    + " and MaxCapacity >= " + Math.Round(capacity, 3);

            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                PipingChangeOverKit item = new PipingChangeOverKit();
                item.FactoryCode = factoryCode;
                item.Model_York = dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"].ToString();
                item.MaxIU = dr["MaxIU"] is DBNull ? 0 : int.Parse(dr["MaxIU"].ToString());
                item.Capacity = capacity;
                item.GasPipe = dr["LiquidPipe"].ToString(); // 下游气管管径
                item.HighPressureGasPipe = dr["HighPressureGasPipe"].ToString();
                item.LowPressureGasPipe = dr["LowPressureGasPipe"].ToString();
                item.SizeUP = dr["SizeUP"].ToString();
                item.PartNumber = dr["PartNumber"].ToString();
                //add on 20160515 by Yunxiao Lin
                item.MaxTotalCHIndoorPipeLength = Convert.ToDouble(dr["MaxTotalCHIndoorPipeLength"].ToString());
                item.MaxTotalCHIndoorPipeLength_MaxIU = Convert.ToDouble(dr["MaxTotalCHIndoorPipeLength_MaxIU"].ToString());
                //add on 20171221 by Shen Junjie
                item.PowerSupply = dr["PowerSupply"].ToString();
                item.PowerLineType = dr["PowerLineType"].ToString();
                item.PowerConsumption = dr["PowerConsumption"] is DBNull ? 0 : double.Parse(dr["PowerConsumption"].ToString()); //add by Shen Junjie on 2018/6/15
                item.PowerCurrent = dr["MaxCurrent"] is DBNull ? 0 : double.Parse(dr["MaxCurrent"].ToString());
                return item;
            }
            return null;
        }

        /// 获取Multi CH-Box，add on 20171130 by Junjie Shen
        /// <summary>
        /// 获取Multi CH-Box
        /// </summary>
        /// <param name="factoryCode"></param>
        /// <param name="capacity"></param>
        /// <param name="sizeUP"></param>
        /// <param name="IUCount"></param>
        /// <returns></returns>
        public PipingMultiCHBox GetMultiCHBox(string region, string series, double capacity, bool sizeUP,
            int branchCount, double maxBranchCapacity, int maxBranchIDU)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_Multi_CHBox);
            DataView dv = dt.DefaultView;
            string sizeUpString = "";
            if (sizeUP)
            {
                sizeUpString = " and SizeUP='TRUE'";
            }
            else
            {
                sizeUpString = " and (SizeUP='NA' or SizeUP='FALSE')";
            }
            dv.RowFilter = " Region like '%/" + region + "/%'"
                + " and Series like '%;" + series + ";%'"
                + sizeUpString
                + " and MinCapacity <=" + Math.Round(capacity, 3)
                + " and MaxCapacity >= " + Math.Round(capacity, 3)
                + " and MaxBranches>=" + branchCount.ToString()
                + " and MaxCapacityPerBranch>=" + Math.Round(maxBranchCapacity, 3)
                + " and MaxIUPerBranch>=" + maxBranchIDU;
            dv.Sort = "MinCapacity ASC, MaxBranches ASC";
            dt = dv.ToTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                PipingMultiCHBox item = DataRowToPipingMultiCHBox(dr);
                item.Capacity = capacity;
                return item;
            }
            return null;
        }

        /// 获取Multi CH-Box，add on 20171130 by Junjie Shen
        /// <summary>
        /// 获取Multi CH-Box
        /// </summary>
        /// <param name="factoryCode"></param>
        /// <param name="capacity"></param>
        /// <param name="sizeUP"></param>
        /// <param name="IUCount"></param>
        /// <returns></returns>
        public bool ExistsMultiCHBox(string region, string series)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_Multi_CHBox);
            DataView dv = dt.DefaultView;
            dv.RowFilter = " Region like '%/" + region + "/%'"
                + " and Series like '%;" + series + ";%'";
            if (dv.Count > 0)
            {
                return true;
            }
            return false;
        }

        private PipingMultiCHBox DataRowToPipingMultiCHBox(DataRow dr)
        {
            PipingMultiCHBox item = new PipingMultiCHBox();
            //item.FactoryCode = dr["FactoryCode"].ToString();
            item.Model_York = dr["Model_York"].ToString();
            item.Model_Hitachi = dr["Model_Hitachi"].ToString();
            item.PowerSupply = dr["PowerSupply"].ToString();
            item.PowerLineType = dr["PowerLineType"].ToString();
            item.PowerConsumption = dr["PowerConsumption"] is DBNull ? 0 : double.Parse(dr["PowerConsumption"].ToString());
            item.PowerCurrent = dr["MaxCurrent"] is DBNull ? 0 : double.Parse(dr["MaxCurrent"].ToString());
            //item.MaxIU = dr["MaxIU"] is DBNull ? 0 : int.Parse(dr["MaxIU"].ToString());
            item.MaxBranches = dr["MaxBranches"] is DBNull ? 0 : int.Parse(dr["MaxBranches"].ToString());
            item.MaxCapacityPerBranch = dr["MaxCapacityPerBranch"] is DBNull ? 0 : double.Parse(dr["MaxCapacityPerBranch"].ToString());
            item.MaxIUPerBranch = dr["MaxIUPerBranch"] is DBNull ? 0 : int.Parse(dr["MaxIUPerBranch"].ToString());
            item.LiquidPipe = dr["LiquidPipe"].ToString();
            item.HighPressureGasPipe = dr["HighPressureGasPipe"].ToString();
            item.LowPressureGasPipe = dr["LowPressureGasPipe"].ToString();
            item.SizeUP = dr["SizeUP"].ToString();
            item.PartNumber = dr["PartNumber"].ToString();
            item.MaxTotalCHIndoorPipeLength = Convert.ToDouble(dr["MaxTotalCHIndoorPipeLength"].ToString());
            item.MaxTotalCHIndoorPipeLength_MaxIU = Convert.ToDouble(dr["MaxTotalCHIndoorPipeLength_MaxIU"].ToString());
            item.Image = dr["Image"].ToString();
            return item;
        }

        /// <summary>
        /// //判断model_Hitachi 是否存在于IDUModel中
        /// </summary>
        /// <param name="sql">SQL </param>
        /// <param name="model">判断model_Hitachi</param>
        /// <returns></returns>
        public bool IsExistsModel(string sql, string model)
        {
            bool isfound = false;
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipeSizeIDU);
            DataView dv = dt.DefaultView;
            sql = sql + " and IDUModel is not null and IDUModel <>''";
            dv.RowFilter = sql;
            dt = dv.ToTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                string[] ss = new string[2];
                foreach (DataRow dr in dt.Rows)
                {
                    string remark = dr["IDUModel"].ToString();
                    // 需要区分Model的情况
                    if (dt.Rows.Count > 1 && !string.IsNullOrEmpty(remark))
                    {
                        string[] arr = remark.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in arr)
                        {
                            if (model.Contains(s))
                            {
                                isfound = true;
                                break;
                            }
                            else
                                continue;
                        }
                        if (isfound)
                        {
                            return isfound;
                        }
                        else
                            continue;
                    }
                }
            }
            return isfound;
        }

        // add sizeup on 20160516 by Yunxiao Lin
        public string[] GetPipeSizeIDU_SizeUP(string regionCode, string factoryCode, double capacity, string model_Hitachi, string sizeUP)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipeSizeIDU);
            DataView dv = dt.DefaultView;
            string sql = " FactoryCode='" + factoryCode
                    + "' and MinCapacity <=" + Math.Round(capacity, 3) + " and MaxCapacity >= " + Math.Round(capacity, 3)
                    + " and SizeUP='" + sizeUP + "'";
            //判断model_Hitachi 是否存在于IDUModel中 on 20180926 by xyj
            if (IsExistsModel(sql, model_Hitachi))
            {
                sql += "  and  IDUModel is not null and IDUModel <>'' ";
            }
            //if (regionCode.StartsWith("EU_") && factoryCode=="E")
            //{
            //    //欧洲区域Remark 不能为空
            //    sql += "  and  IDUModel is not null and IDUModel <>'' ";
            //} 
            dv.RowFilter = sql;
            dt = dv.ToTable();
            //string sql = "select * from " + TB_PipeSizeIDU + " where FactoryCode='" + factoryCode
            //    + "' and MinCapacity <=" + Math.Round(capacity, 3) + " and MaxCapacity >= " + Math.Round(capacity, 3)
            //    +" and SizeUP='TRUE'";

            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                string[] ss = new string[2];
                foreach (DataRow dr in dt.Rows)
                {
                    string remark = dr["IDUModel"].ToString();

                    // 需要区分Model的情况
                    if (dt.Rows.Count > 1 && !string.IsNullOrEmpty(remark))
                    {
                        string[] arr = remark.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        bool isfound = false;
                        foreach (string s in arr)
                        {
                            if (model_Hitachi.Contains(s))
                            {
                                isfound = true;
                                break;
                            }
                            else
                                continue;
                        }
                        if (isfound)
                        {
                            ss[0] = dr["LiquidPipe"].ToString();
                            ss[1] = dr["GasPipe"].ToString();
                            return ss;
                        }
                        else
                            continue;
                    }
                    else
                    {
                        ss[0] = dr["LiquidPipe"].ToString();
                        ss[1] = dr["GasPipe"].ToString();
                        return ss;
                    }
                }
            }
            return null;
        }

        public string[] GetPipeSizeODU(string factoryCode, string type, string unitType, double capacity)
        {
            return null;
        }

        /// 获取管长修正系数, 找不到则为0
        /// 增加PipeType参数 20161102 by Yunxiao Lin
        /// <summary>
        /// 获取管长修正系数, 找不到则为0
        /// </summary>
        /// <param name="factoryCode"></param>
        /// <param name="unitType"></param>
        /// <param name="model_Hitachi"></param>
        /// <param name="condition"></param>
        /// <param name="highDiff"></param>
        /// <param name="eqLength"></param>
        /// <param name="PipeType"></param>
        /// <param name="Cooling"></param>
        /// <param name="Heating"></param>
        /// <returns></returns>
        public double GetPipingLengthFactor(string factoryCode, string unitType, string model_Hitachi, string condition, double highDiff, double eqLength, string PipeType)
        {
            if (highDiff > eqLength)
                return 0;
            string pipingLengthTableName = GetPipingLengthTableName(factoryCode, unitType, model_Hitachi, condition, eqLength, PipeType);
            if (string.IsNullOrEmpty(pipingLengthTableName))
                return 0;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(pipingLengthTableName);
            DataView dv = dt.DefaultView;
            //dv.RowFilter = "HighDiff = " + highDiff + " and EqLength = " + eqLength;
            dv.Sort = "HighDiff asc, EqLength asc";
            dt = dv.ToTable();

            if (dt == null || dt.Rows.Count == 0)
            {
                return 0;
            }

            /*     E2  E   E1
             * H1  F1      F3
             * H   F5  F7  F6
             * H2  F2      F4  */
            decimal E = (decimal)eqLength;
            decimal H = (decimal)highDiff;
            decimal E1 = decimal.MaxValue, E2 = 0, H1 = decimal.MaxValue, H2 = decimal.MinValue; //E2 = decimal.MinValue added for internal Bug Find zero length Issue
            decimal F1 = 0, F2 = 0, F3 = 0, F4 = 0, F5 = 0, F6 = 0, F7 = 0;

            foreach (DataRow dr in dt.Rows)
            {
                decimal factor = dr["Factor"] is DBNull ? 0 : decimal.Parse(dr["Factor"].ToString());
                decimal el = dr["EqLength"] is DBNull ? 0 : decimal.Parse(dr["EqLength"].ToString());
                decimal h = dr["HighDiff"] is DBNull ? 0 : decimal.Parse(dr["HighDiff"].ToString());

                if (el == E && h == H)
                {
                    return (double)factor;
                }

                if (el <= E && el > E2)
                {
                    E2 = el;
                    F1 = 0;
                    F2 = 0;
                }
                if (el >= E && el < E1)
                {
                    E1 = el;
                    F3 = 0;
                    F4 = 0;
                }
                if (h <= H && h > H2)
                {
                    H2 = h;
                    F2 = 0;
                    F4 = 0;
                }
                if (h >= H && h < H1)
                {
                    H1 = h;
                    F1 = 0;
                    F3 = 0;
                }

                if (el == E2 && h == H1) F1 = factor;
                if (el == E2 && h == H2) F2 = factor;
                if (el == E1 && h == H1) F3 = factor;
                if (el == E1 && h == H2) F4 = factor;
            }
           // added for internal Bug Find zero length Issue
            //if (F1 == 0 || F2 == 0 || F3 == 0 || F4 == 0)
            //{
            //    return 0; //exceeded
            //}

            //Eq Length和High Diff都为0m时，管长修正系数为1  add by Shen Junjie on 2019/7/24
            if (E2 == 0)
            {
                if (H1 == 0) F1 = 1;
                if (H2 == 0) F2 = 1;
            }

            if (F3 == 0 || F4 == 0)
            {
                return 0; //out of range
            }

            if (F1 == 0 && F2 == 0)
            {
                return 0; //out of range
            }

            if (F1 == 0 || F2 == 0)
            {
                if (H1 == H2)
                {
                    return 0; //data error
                }

                decimal E3 = E2 + (E1 - E2) / (H1 - H2) * (H - H2);
                if (E < E3)
                {
                    return 0; //out of range
                }

                return (double)(F1 == 0 ? F2 : F1);
            }


            if (H1 == H2)
            {
                F5 = F2;
                F6 = F4;
            }
            else
            {
                F5 = (H - H2) / (H1 - H2) * (F1 - F2) + F2;
                F6 = (H - H2) / (H1 - H2) * (F3 - F4) + F4;
            }

            if (E1 == E2)
            {
                F7 = F5;
            }
            else
            {
                F7 = (E - E2) / (E1 - E2) * (F6 - F5) + F5;
            }

            return (double)Math.Round(F7, 3);
        }

        //增加PipeType参数
        //private string GetPipingLengthTableName(string factoryCode, string unitType, string model_Hitachi, string condition, int eqLength)
        private string GetPipingLengthTableName(string factoryCode, string unitType, string model_Hitachi, string condition, double eqLength, string PipeType)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingLengthTableName);
            DataView dv = dt.DefaultView;
            //此处需要增加model_Hitachi参数查询，如查不到，再放开model_Hitachi查UnitType Yunxiao Lin 20160618
            //dv.RowFilter = "FactoryCode = '" + factoryCode + "' and UnitType = '" + unitType + "' and Condition = '" + condition
            //    + "' and MinEqLength <= " + eqLength + " and MaxEqLength >= " + eqLength + " and Model_Hitachi='" + model_Hitachi + "' and DeleteFlag = 1";
            string strFilter = "FactoryCode = '" + factoryCode + "' and UnitType = '" + unitType + "' and Condition = '" + condition
                + "' and MinEqLength <= " + eqLength + " and MaxEqLength >= " + eqLength + " and Model_Hitachi='" + model_Hitachi + "' and DeleteFlag = 1"
                + " and Type='" + PipeType + "'";
            dv.RowFilter = strFilter;
            dt = dv.ToTable();
            if (dt.Rows.Count == 0)
            {
                //找不到数据，则去掉model_Hitachi参数，再查询一遍 Yunxiao Lin 20160618
                //dv.RowFilter = "FactoryCode = '" + factoryCode + "' and UnitType = '" + unitType + "' and Condition = '" + condition
                //+ "' and MinEqLength <= " + eqLength + " and MaxEqLength >= " + eqLength + " and DeleteFlag = 1";
                strFilter = "FactoryCode = '" + factoryCode + "' and UnitType = '" + unitType + "' and Condition = '" + condition
                    + "' and MinEqLength <= " + eqLength + " and MaxEqLength >= " + eqLength + " and DeleteFlag = 1"
                    + " and Type='" + PipeType + "'";
                dv.RowFilter = strFilter;
                dt = dv.ToTable();
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string name = dr["PipingLengthTableName"].ToString();
                return name;
            }
            return "";
        }

        public NodeElement_Piping GetPipingNodeOutElement(string model, bool isHitachi, string pipeType, string sizeUp, string UnitType)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingImage);
            DataView dv = dt.DefaultView;
            //dv.RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "'";
            //India的HNCQ (Top flow, 3Ph) ODU Model_Hitachi 是HNBCMQ1结尾，和其他Region不一样，取Piping Image数据需要进行特殊判断 20180213 by Yunxiao lin
            string RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "' and SizeUP='" + sizeUp + "' and pipeType='" + pipeType + "' and UnitType='" + UnitType + "'";
            if (UnitType == "HNCQ (Top flow, 3Ph)"
                && Project.CurrentProject != null
                && Project.CurrentProject.SubRegionCode == "IN_INDIA")
                RowFilter += " AND Model_Hitachi like '%HNBCMQ1'";
            else
                RowFilter += " AND Model_Hitachi not like '%HNBCMQ1'";

            dv.RowFilter = RowFilter;

            dt = dv.ToTable();
            // TODO: WaterSource需加上SizeUp计算 20160613 by Yunxiao Lin
            //string sql = "select * from "+TB_PipingImage+" where SelectionType = 'Outdoor' and Model='" + model + "'";
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string imageName = dr["Image_Hitachi"].ToString(); // 不带png后缀
                int count = dr["UnitCount"] is DBNull ? 1 : int.Parse(dr["UnitCount"].ToString());
                string connectKitModel = dr["ConnectionKitModel"].ToString();
                if (!isHitachi)// updated on 20160412 clh
                    connectKitModel = dr["ConnectionKitModel_York"].ToString();
                string ptConnectKit = dr["PtConnectionKit"].ToString();
                string pipeSize = dr["PipeSize"].ToString();
                string ptPipeSize = dr["PtPipeSize"].ToString();
                string ptVline = dr["PtVLine"].ToString();
                string ptModelLocation = dr["ModelLocation"].ToString();
                NodeElement_Piping item = new NodeElement_Piping(imageName, count, connectKitModel, ptConnectKit, pipeSize, ptPipeSize, ptVline, ptModelLocation);
                return item;
            }
            return GetPipingNodeOutElement(model, isHitachi, pipeType, sizeUp);
        }

        public NextGenModel.NodeElement_Piping GetPipingNodeOutElementNextGen(string model, bool isHitachi, string pipeType, string sizeUp, string UnitType)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingImage);
            DataView dv = dt.DefaultView;
            //dv.RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "'";
            //India的HNCQ (Top flow, 3Ph) ODU Model_Hitachi 是HNBCMQ1结尾，和其他Region不一样，取Piping Image数据需要进行特殊判断 20180213 by Yunxiao lin
            string RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "' and SizeUP='" + sizeUp + "' and pipeType='" + pipeType + "' and UnitType='" + UnitType + "'";
            if (UnitType == "HNCQ (Top flow, 3Ph)"
                && Project.CurrentProject != null
                && Project.CurrentProject.SubRegionCode == "IN_INDIA")
                RowFilter += " AND Model_Hitachi like '%HNBCMQ1'";
            else
                RowFilter += " AND Model_Hitachi not like '%HNBCMQ1'";

            dv.RowFilter = RowFilter;

            dt = dv.ToTable();
            // TODO: WaterSource需加上SizeUp计算 20160613 by Yunxiao Lin
            //string sql = "select * from "+TB_PipingImage+" where SelectionType = 'Outdoor' and Model='" + model + "'";
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string imageName = dr["Image_Hitachi"].ToString(); // 不带png后缀
                int count = dr["UnitCount"] is DBNull ? 1 : int.Parse(dr["UnitCount"].ToString());
                string connectKitModel = dr["ConnectionKitModel"].ToString();
                if (!isHitachi)// updated on 20160412 clh
                    connectKitModel = dr["ConnectionKitModel_York"].ToString();
                string ptConnectKit = dr["PtConnectionKit"].ToString();
                string pipeSize = dr["PipeSize"].ToString();
                string ptPipeSize = dr["PtPipeSize"].ToString();
                string ptVline = dr["PtVLine"].ToString();
                string ptModelLocation = dr["ModelLocation"].ToString();
                NextGenModel.NodeElement_Piping item = new NextGenModel.NodeElement_Piping(imageName, count, connectKitModel, ptConnectKit, pipeSize, ptPipeSize, ptVline, ptModelLocation);
                return item;
            }
            return GetPipingNodeOutElementNextGen(model, isHitachi, pipeType, sizeUp);
        }

        /// 获取室外机管径增加SizeUp参数 20160615 by Yunxiao Lin
        /// 增加pipeType参数，同样model的室外机可能有3管2管区别 20160627 by Yunxiao Lin
        /// <summary>
        /// 获取室外机节点对象，包含组合室外机内部的分支管和管径型号
        /// </summary>
        /// <param name="model">指虚拟Model全名</param>
        /// <param name="isHitachi"></param>
        /// <param name="pipeType">2管3管 2Pipe 3Pipe</param>
        /// <param name="sizeUp">是否需要SizeUp TRUE FALSE NA</param>
        /// <returns></returns>
        public NodeElement_Piping GetPipingNodeOutElement(string model, bool isHitachi, string pipeType, string sizeUp)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingImage);
            DataView dv = dt.DefaultView;
            //dv.RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "'";
            //India的HNCQ (Top flow, 3Ph) ODU Model_Hitachi 是HNBCMQ1结尾，和其他Region不一样，取Piping Image数据需要进行特殊判断 20180213 by Yunxiao lin
            string RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "' and SizeUP='" + sizeUp + "' and pipeType='" + pipeType + "'";
            if (model.StartsWith("JVOH")
                && (model.EndsWith("VAEM0BQ") || model.EndsWith("VPEM0BQ"))
                && Project.CurrentProject != null
                && Project.CurrentProject.SubRegionCode == "IN_INDIA")
                RowFilter += " AND Model_Hitachi like '%HNBCMQ1'";
            else
                RowFilter += " AND Model_Hitachi not like '%HNBCMQ1'";
            dv.RowFilter = RowFilter;
            dt = dv.ToTable();
            // TODO: WaterSource需加上SizeUp计算 20160613 by Yunxiao Lin
            //string sql = "select * from "+TB_PipingImage+" where SelectionType = 'Outdoor' and Model='" + model + "'";
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string imageName = dr["Image_Hitachi"].ToString(); // 不带png后缀
                int count = dr["UnitCount"] is DBNull ? 1 : int.Parse(dr["UnitCount"].ToString());
                string connectKitModel = dr["ConnectionKitModel"].ToString();
                if (!isHitachi)// updated on 20160412 clh
                    connectKitModel = dr["ConnectionKitModel_York"].ToString();
                string ptConnectKit = dr["PtConnectionKit"].ToString();
                string pipeSize = dr["PipeSize"].ToString();
                string ptPipeSize = dr["PtPipeSize"].ToString();
                string ptVline = dr["PtVLine"].ToString();
                string ptModelLocation = dr["ModelLocation"].ToString();
                NodeElement_Piping item = new NodeElement_Piping(imageName, count, connectKitModel, ptConnectKit, pipeSize, ptPipeSize, ptVline, ptModelLocation);
                return item;
            }
            return null;
        }

        public NextGenModel.NodeElement_Piping GetPipingNodeOutElementNextGen(string model, bool isHitachi, string pipeType, string sizeUp)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingImage);
            DataView dv = dt.DefaultView;
            //dv.RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "'";
            //India的HNCQ (Top flow, 3Ph) ODU Model_Hitachi 是HNBCMQ1结尾，和其他Region不一样，取Piping Image数据需要进行特殊判断 20180213 by Yunxiao lin
            string RowFilter = "SelectionType = 'Outdoor' and Model='" + model + "' and SizeUP='" + sizeUp + "' and pipeType='" + pipeType + "'";
            if (model!=null && model.StartsWith("JVOH")
                && (model.EndsWith("VAEM0BQ") || model.EndsWith("VPEM0BQ"))
                && Project.CurrentProject != null
                && Project.CurrentProject.SubRegionCode == "IN_INDIA")
                RowFilter += " AND Model_Hitachi like '%HNBCMQ1'";
            else
                RowFilter += " AND Model_Hitachi not like '%HNBCMQ1'";
            dv.RowFilter = RowFilter;
            dt = dv.ToTable();
            // TODO: WaterSource需加上SizeUp计算 20160613 by Yunxiao Lin
            //string sql = "select * from "+TB_PipingImage+" where SelectionType = 'Outdoor' and Model='" + model + "'";
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string imageName = dr["Image_Hitachi"].ToString(); // 不带png后缀
                int count = dr["UnitCount"] is DBNull ? 1 : int.Parse(dr["UnitCount"].ToString());
                string connectKitModel = dr["ConnectionKitModel"].ToString();
                if (!isHitachi)// updated on 20160412 clh
                    connectKitModel = dr["ConnectionKitModel_York"].ToString();
                string ptConnectKit = dr["PtConnectionKit"].ToString();
                string pipeSize = dr["PipeSize"].ToString();
                string ptPipeSize = dr["PtPipeSize"].ToString();
                string ptVline = dr["PtVLine"].ToString();
                string ptModelLocation = dr["ModelLocation"].ToString();
                NextGenModel.NodeElement_Piping item = new NextGenModel.NodeElement_Piping(imageName, count, connectKitModel, ptConnectKit, pipeSize, ptPipeSize, ptVline, ptModelLocation);
                return item;
            }
            return null;
        }


        public NodeElement_Piping GetPipingNodeIndElement(string unitType, bool isHitachi)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingImage);
            DataView dv = dt.DefaultView;
            dv.RowFilter = "SelectionType = 'Indoor' and UnitType='" + unitType + "'";
            dt = dv.ToTable();

            //string sql = "select * from " + TB_PipingImage + " where SelectionType = 'Indoor' and UnitType='" + unitType + "'";
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string imageName = dr["Image_Hitachi"].ToString(); // 不带png后缀
                int count = 1;
                NodeElement_Piping item = new NodeElement_Piping(imageName, count);
                return item;
            }
            return null;
        }

        public NextGenModel.NodeElement_Piping GetPipingNodeIndElementNextGen(string unitType, bool isHitachi)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad(TB_PipingImage);
            DataView dv = dt.DefaultView;
            dv.RowFilter = "SelectionType = 'Indoor' and UnitType='" + unitType + "'";
            dt = dv.ToTable();

            //string sql = "select * from " + TB_PipingImage + " where SelectionType = 'Indoor' and UnitType='" + unitType + "'";
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                string imageName = dr["Image_Hitachi"].ToString(); // 不带png后缀
                int count = 1;
                NextGenModel.NodeElement_Piping item = new NextGenModel.NodeElement_Piping(imageName, count);
                return item;
            }
            return null;
        }


        public string GetPipeSize_Inch(string orgPipeSize)
        {
            if (string.IsNullOrEmpty(orgPipeSize) || orgPipeSize.Equals("-"))
                return orgPipeSize;

            int index = ListPipeSize.IndexOf(orgPipeSize.Trim());
            return ListPipeSize_inch[index];
        }
    }
}
