//********************************************************************
// 文件名: AccessoryDAL.cs
// 描述: 定义 VRF 项目中的附件DAL类
// 作者: clh
// 创建时间: 2016-2-15
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

using JCHVRF.DALFactory;
using JCHVRF.Model;
using System.Data.OleDb;
using JCHVRF.VRFTrans;

namespace JCHVRF.DAL
{
    public class AccessoryDAL : IAccessoryDAL
    {
        IDataAccessObject _dao;
        public AccessoryDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        public List<Accessory> GetDefault(Indoor inItem, string RegionCode, string SubRegionCode, string Series)
        {
            if (inItem == null)
                return null;
                       
            
            inItem.Series = Series;
            
            //string modelfull = inItem.ModelFull;
            string modelYork = inItem.Model_York;
            //string fCode = modelfull.Substring(modelfull.Length - 1, 1);
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string fCode = inItem.GetFactoryCodeForAccess();
            string bCode = (string.IsNullOrEmpty(modelYork) || modelYork == "-") ? "H" : "Y";
            string uType = inItem.Type;
            // add HAPE 20160618 by Yunxiao Lin
            //fCode = (fCode == "Q") ? "Q" : "S";

            //// 将High Wall（A Type）排除,OK
            //if (uType == "High Wall")
            //{
            //    string code = modelfull.Substring(modelfull.Length - 2, 1);// 第13位code
            //    if (code != "B")
            //        return null;
            //}


            //string sql = "FactoryCode = '" + fCode
            //    + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
            //    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity
            //    + " and IsDefault <> 0 ";
            // 增加UnitType='Ventilation' 判断，Ventilation相当于Indoor Unit，但是没有Capacity参数 20160618 by Yunxiao Lin
            string sql = " 1=1 ";

            //if (RegionCode.Substring(0, 2) == "EU")
            //{
            //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            //}
            //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20171229 by Yunxiao Lin
            //    || SubRegionCode == "LA_PERU"
            //    || SubRegionCode == "LA_SC")
            //    && inItem.ProductType.Contains("Comm. Tier 4,"))
            //{
            //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + Series + ";%' ";
            //}
            //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
            //{
            //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            //}
            ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
            //else if (isWuxiGen2)
            //{
            //    sql += " and Series like '%;" + Series + ";%' ";
            //}
            //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
            //{
            //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            //}
            //else if ((SubRegionCode == "ASIA" && bCode == "Y") || SubRegionCode == "BGD" || SubRegionCode == "SEA_60Hz") // ASIA York, BGD, SEA_60Hz，需要判断
            //{
            //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;"+Series+";%'";
            //}
            //else
            //{
            //    if (IsSpecial(SubRegionCode, fCode, bCode, inItem.Type, inItem))
            //    {
            //        sql += " and Series like '%;" + inItem.Series + ";%' ";
            //    }
            //    else
            //    {
            //        sql += " and RegionCode is null and Series is null";
            //    }
            //}
            // 将Region和Series特殊判断封装 20181121 by Yunxiao Lin
            sql = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, fCode, bCode, Series, inItem, sql);

            if (uType.Contains("Heat Exchanger"))
            {
                sql += " and FactoryCode = '" + fCode + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "') and IsDefault <> 0";
            }
            else
            {
                sql += " and FactoryCode = '" + fCode
                    + "' and BrandCode = '" + bCode + "' and (((UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity + ") or UnitType = 'Ventilation')"
                    + " and IsDefault <> 0 ";
            }




            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                //Indoor Unit排除High Wall
                if (uType == "High Wall" || uType == "High Wall (NA)" || uType == "High Wall (w/o EXV)")
                {
                    sql += " and Model_Hitachi<>'THM-R2A'";
                }

                // 处理MAL的特殊情况
                //if (inItem.RegionCode != "MAL")
                if ((inItem.RegionCode != "MAL" && inItem.RegionCode != "LA_BV") || bCode == "H" || isWuxiGen2(Series)) //只有马来西亚的YORK品牌才用QE作为Accressory型号的结尾 20161023 by Yunxiao Lin
                {
                    //玻利维亚的York产品线和马来西亚一样 20170629 by Yunxiao lin
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    sql += " and Model_York not like '%QE'  and (Remark<>'MAL' or Remark is null)";
                }
                else
                {
                    sql += " and (Remark='MAL' or (  Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    sql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    sql += " and  Model_York <>'JCRB10NEWS'))";
                }
                //由于产品线更新的原因，SMZ High Duct 和 Medium Ducted 会存在新老(FSN2/FSN3)型号并存的情况，在获取Accessory时须进行区分 20170928 by Yunxiao Lin
                if (fCode == "S" && bCode == "H" && uType == "High Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPI-FSN3'";
                }
                else if (fCode == "S" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPIM-FSN3'";
                }
                //新增特殊数据处理 on 20180801 by xyj  
                if (!(Project.CurrentProject.RegionCode=="EU_W" ||
                    Project.CurrentProject.RegionCode == "EU_E" ||
                    Project.CurrentProject.RegionCode == "EU_S" || 
                    Project.CurrentProject.SubRegionCode == "TW"))
                    sql += GetSpecialData_SQL(SubRegionCode, fCode, bCode, uType, inItem);
                ////处理LA_MMA Tier 1+ only add by axj 20160624
                //if (inItem.RegionCode == "LA_MMA" && inItem.ProductType == "Comm. Tier 1+, HP")
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'JCSA10NEWS' and Model_York <>'JRDA11NEWS' and Model_York<>'JCWB10NEWS'  and Model_York<>'JCRA10NEWQ' and  Model_York<>'JCRB10NEWS' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'RDH-01QE' and Model_York <>'JRDA10NEWQ' and Model_York<>'JCWA10NEWQ' and Model_York<>'CWH-01QE' and Model_York<>'CRH-01QE'";
                //    //S-Y
                //    sql += " and Model_York<>'JP-AP160NA1' and Model_York<>'JP-AP160NAE' and Model_York<>'JR4A10NEWS' ";
                //}
                //else
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'CIS01' and  Model_York <> 'CWDIRK01' and Model_York <>'PSC-5RA' and Model_York<>'CIW01' and Model_York<>'CIR01' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'KW-PP10Q' and Model_York <>'KW-PP5Q' and Model_York <>'KW-PP6Q' and Model_York <>'KW-PP7Q' and Model_York <>'KW-PP8Q' and Model_York <>'KW-PP9Q' and Model_York<>'CWDIRK01' ";
                //    //S-Y
                //    sql += " and Model_York<>'PI-160LS2' and Model_York<>'PIS-56LS' and Model_York<>'P-AP160NA2'  and Model_York<>'P-AP160NAE1' and Model_York<>'F-56MS-PK2' and Model_York<>'PD-75A' and Model_York<>'PD-100' and Model_York<>'B-160H3' and Model_York<>'OACI-160K3' and Model_York<>'DG-56SW1' and Model_York<>'SOR-NES'";
                //    sql += " and Model_York<>'C4IRK01' and Model_York<>'C1IRK01' and Model_York<>'TKCI-160K' ";
                //}

                dv.RowFilter = sql;
                dt = dv.ToTable();

                List<Accessory> list = new List<Accessory>();
                foreach (DataRow dr in dt.Rows)
                {
                    Accessory item = new Accessory();
                    item.FactoryCode = fCode;
                    item.BrandCode = bCode;
                    item.UnitType = uType;
                    item.MinCapacity = dr["MinCapacity"] is DBNull ? 0 : double.Parse(dr["MinCapacity"].ToString());
                    item.MaxCapacity = dr["MaxCapacity"] is DBNull ? 0 : double.Parse(dr["MaxCapacity"].ToString());
                    item.Type = dr["Type"] is DBNull ? "" : dr["Type"].ToString();
                    item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                    item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();
                    item.MaxNumber = dr["MaxNumber"] is DBNull ? 0 : int.Parse(dr["MaxNumber"].ToString());
                    item.Count = 1; //Shweta: added for VRF Next Gen app.
                    item.IsSelect = true;//Shweta: added for VRF Next Gen app.
                    item.IsDefault = bool.Parse(dr["IsDefault"].ToString());
                    list.Add(item);                   
                }
                return list;
            }

            return null;
        }

        ///// <summary>
        ///// 获取室内机配件 根据UnitType Type 筛选配件
        ///// </summary>
        ///// <param name="BrandCode"></param>
        ///// <param name="FactoryCode"></param>
        ///// <param name="UnitType"></param>
        ///// <param name="Type"></param>
        ///// <returns></returns>
        //public DataTable GetShareAccessoryItemList(string BrandCode, string FactoryCode, string UnitType, string Type, double Capacity, string RegionCode, string SubRegionCode)
        //{

        //    DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
        //    if (dt != null && dt.Rows.Count > 0)
        //    {
        //        DataView dv = dt.DefaultView;
        //        string sql = " 1=1 ";
        //        if (RegionCode.Substring(0, 2) == "EU" || SubRegionCode == "TW")
        //            sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
        //        else
        //            sql += " and RegionCode is  null ";
        //        string sqls = " and Type='" + Type + "' ";
        //        if (Type == "Remote Control Switch")
        //            sqls = " and Type in('" + Type + "','Half-size Remote Control Switch') ";

        //        if (UnitType == "Total Heat Exchanger (KPI-E4E)")
        //            sqls = sql + " and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and UnitType = '" + UnitType + "' ";
        //        else
        //            sqls = sql + " and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and UnitType = '" + UnitType + "'  and  MinCapacity<=" + Capacity + " and MaxCapacity >=" + Capacity + "";
        //        dv.RowFilter = sqls;
        //        dt = dv.ToTable();
        //    }
        //    return dt;
        //}

        /// <summary>
        /// 针对Commercial VRF CO, CNCQ;Commercial VRF High ambient, JNBBQ; 系列 特殊处理 on 20180928 by xyj
        /// </summary>
        /// <param name="SubRegionCode">Region</param>
        /// <param name="fCode">工厂</param>
        /// <param name="bCode">品牌</param>
        /// <param name="uType">类型</param>
        /// <param name="inItem">室内机</param>
        /// <returns></returns>
        public bool IsSpecial(string SubRegionCode, string fCode, string bCode, string uType, Indoor inItem)
        {
            bool isTrue = false; 
            string remark = GetSpecialData_SQL(SubRegionCode,fCode,bCode,uType,inItem);
            if (!string.IsNullOrEmpty(inItem.Series))
            {                
                string sql = "select * from JCHVRF_Accessory where 1=1 and FactoryCode='" + fCode + "' and BrandCode='" + bCode + "' and UnitType='" + uType + "' and (RegionCode is null or RegionCode like '%" + SubRegionCode + "%') and Series like '%;" + inItem.Series + ";%' " + remark;
                DataTable dt = _dao.GetDataTable(sql);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        isTrue = true;
                    }
                }
            }
            return isTrue;
        }

        /// <summary>
        /// 获取特殊的数据逻辑处理 已取得需要的sql
        /// </summary>
        /// <param name="SubRegionCode">RegionCode</param>
        /// <param name="fCode">工厂code</param>
        /// <param name="bCode">品牌code</param>
        /// <param name="uType">accessory type</param>
        /// <param name="inItem">indoor</param> 
        //private string GetSpecialData_SQL(string SubRegionCode, string fCode, string bCode, string uType, Indoor inItem)
        //{
        //    string sql = "";
        //    string series = "Commercial VRF CO, CNCQ;Commercial VRF High ambient, JNBBQ;";
        //    if (SubRegionCode == "TW")//台湾 S和T工厂
        //    {
        //        #region //是否需要使用台湾的特殊数据处理
        //        bool isRemark = false;
        //        if (fCode == "T" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-"))
        //        {
        //            isRemark = true;
        //        }
        //        else if (fCode == "S" || fCode == "T" && (bCode == "H" && uType == "Four Way Cassette" && inItem.Model_Hitachi.StartsWith("RCI-")))
        //        {
        //            isRemark = true;
        //        }
        //        else if (fCode == "S" && (bCode == "H" && uType == "Ceiling Suspended" && inItem.Model_Hitachi.StartsWith("RPC-") && inItem.Model_Hitachi.EndsWith("FSN3")))
        //        {
        //            isRemark = true;
        //        }
        //        else if (fCode == "S" && (bCode == "H" && uType == "Two Way Cassette" && inItem.Model_Hitachi.StartsWith("RCD-") && inItem.Model_Hitachi.EndsWith("FSN3")))
        //        {
        //            isRemark = true;
        //        }
        //        if (isRemark)
        //        {
        //            sql = " and (Remark like '%" + inItem.Model_Hitachi + "%' or Remark is null) ";
        //        }
        //        #endregion
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(inItem.Series))
        //        {
        //            if (series.Contains(inItem.Series) || (inItem.Series == "Residential VRF HP, HNRQ" && SubRegionCode == "LA_SC"))
        //            {
        //                if (fCode == "Q" && bCode == "H")
        //                {
        //                    #region //是否需要使用Q工厂的特殊数据处理(HAPQ) on 230180925 by xyj
        //                    if (uType == "Compact Ducted" && inItem.Model_Hitachi.StartsWith("RPIZ-") && inItem.Model_Hitachi.EndsWith("HNATNQ"))
        //                    {
        //                        sql = " and Remark='RPIZ-HNATNQ' ";
        //                    }
        //                    else if (uType == "Compact Ducted (DC)" && inItem.Model_Hitachi.StartsWith("RPIZ-") && inItem.Model_Hitachi.EndsWith("HNDTSQ"))
        //                    {
        //                        sql = " and Remark='RPIZ-HNDTSQ' ";
        //                    }
        //                    else if (uType == "Four way cassette" && inItem.Model_Hitachi.StartsWith("RCI-") && inItem.Model_Hitachi.EndsWith("FSKDNQ"))
        //                    {
        //                        sql = " and Remark='RCI-FSKDNQ' ";
        //                    }
        //                    else if (uType == "High Static Ducted")
        //                    {
        //                        if (inItem.Model_Hitachi.StartsWith("RPIH-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
        //                        {
        //                            sql = " and Remark='RPIH-HNAUNQ' ";
        //                        }
        //                        else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQH"))
        //                        {
        //                            sql = " and Remark='RPI-FSNQH' ";
        //                        }
        //                        else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQ"))
        //                        {
        //                            sql = " and Remark='RPI-FSNQ' ";
        //                        }
        //                    }
        //                    else if (uType == "Low Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIL-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
        //                    {
        //                        if (inItem.Model_Hitachi.StartsWith("RPIL-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
        //                        {
        //                            sql = " and Remark='RPIL-HNAUNQ' ";
        //                        }
        //                        else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQL"))
        //                        {
        //                            sql = " and Remark='RPI-FSNQL' ";
        //                        }
        //                    }
        //                    else if (uType == "Medium Static Ducted")
        //                    {
        //                        if (inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
        //                        {
        //                            sql = " and Remark='RPIM-HNAUNQ' ";
        //                        }
        //                        else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQH"))
        //                        {
        //                            sql = " and Remark='RPI-FSNQH' ";
        //                        }
        //                        else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3Q"))
        //                        {
        //                            sql = " and Remark='RPI-FSN3Q' ";
        //                        }
        //                    }
        //                    else if (uType == "Floor Ceiling" && inItem.Model_Hitachi.StartsWith("RPFC-") && inItem.Model_Hitachi.EndsWith("FSNQ"))
        //                    {
        //                        sql = " and Remark='RPFC-FSNQ' ";
        //                    }
        //                    else if (uType == "Floor Concealed" && inItem.Model_Hitachi.StartsWith("RPFI-") && inItem.Model_Hitachi.EndsWith("FSNQ"))
        //                    {
        //                        sql = " and Remark='RPFI-FSNQ' ";
        //                    }
        //                    else if (uType == "Fresh Air" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("KFNQ"))
        //                    {
        //                        sql = " and Remark='RPI-KFNQ' ";
        //                    }
        //                    else if (uType == "High Wall" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSNQS"))
        //                    {
        //                        sql = " and Remark='RPK-FSNQS' ";
        //                    }
        //                    else if (uType == "Slim Ducted" && inItem.Model_Hitachi.StartsWith("RPIZ-") && inItem.Model_Hitachi.EndsWith("FSNQS"))
        //                    {
        //                        sql = " and Remark='RPIZ-FSNQS' ";
        //                    }
        //                    #endregion
        //                }
        //                else if (fCode == "S" && bCode == "H")
        //                {
        //                    if (uType == "Ceiling Suspended" && inItem.Model_Hitachi.StartsWith("RPC-") && inItem.Model_Hitachi.EndsWith("FSN3"))
        //                    {
        //                        sql = " and Remark='RPC-FSN3' ";
        //                    }
        //                    else if (uType == "Mini Four Way Cassette" && inItem.Model_Hitachi.StartsWith("RCIM-") && inItem.Model_Hitachi.EndsWith("FSN4"))
        //                    {
        //                        sql = " and Remark='RCIM-FSN4' ";
        //                    }
        //                    else if (uType == "One way Cassette" && inItem.Model_Hitachi.StartsWith("RCS-") && inItem.Model_Hitachi.EndsWith("FSNS"))
        //                    {
        //                        sql = " and Remark='RCS-FSNS' ";
        //                    }
        //                    else if (uType == "Two Way Cassette" && inItem.Model_Hitachi.StartsWith("RCD-") && inItem.Model_Hitachi.EndsWith("FSN3"))
        //                    {
        //                        sql = " and Remark='RCD-FSN3' ";
        //                    }
        //                }
        //                else if (fCode == "M" && bCode == "H")
        //                {
        //                    #region //是否需要使用M工厂的特殊数据处理 on 230180925 by xyj
        //                    if (uType == "High Wall" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSN4M"))
        //                    {
        //                        sql = " and Remark='RPK-FSN4M' ";
        //                    }
        //                    #endregion
        //                }
        //            }
        //            else if (uType == "High Wall" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSN4M") && !isWuxiGen2(inItem.Series))
        //            {                       
        //                if (inItem.Series == "Residential VRF HP, HNRQ1")
        //                {
        //                    sql = " and (Remark<>'RPK-FSN4M' or Remark is null) ";
        //                }
        //                else
        //                {
        //                    sql = " and Remark='RPK-FSN4M' ";
        //                }
        //            }
        //            else if (uType == "High Wall (w/o EXV)" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSNH4M") && !isWuxiGen2(inItem.Series))
        //            {
        //                if (inItem.Series == "Residential VRF HP, HNRQ1")
        //                {
        //                    sql = " and (Remark<>'RPK-FSNH4M' or Remark is null ) ";
        //                }
        //                else
        //                {
        //                    sql = " and Remark='RPK-FSNH4M' ";
        //                }
        //            }

        //        }
        //    }
        //    return sql;
        //}
        private string GetSpecialData_SQL(string SubRegionCode, string fCode, string bCode, string uType, Indoor inItem)
        {
            string sql = "";
            string series = "Commercial VRF CO, CNCQ;Commercial VRF High ambient, JNBBQ;";
            if (SubRegionCode == "TW")//台湾 S和T工厂
            {
                #region //是否需要使用台湾的特殊数据处理
                bool isRemark = false;
                if (fCode == "T" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-"))
                {
                    isRemark = true;
                }
                else if (fCode == "S" || fCode == "T" && (bCode == "H" && uType == "Four Way Cassette" && inItem.Model_Hitachi.StartsWith("RCI-")))
                {
                    isRemark = true;
                }
                else if (fCode == "S" && (bCode == "H" && uType == "Ceiling Suspended" && inItem.Model_Hitachi.StartsWith("RPC-") && inItem.Model_Hitachi.EndsWith("FSN3")))
                {
                    isRemark = true;
                }
                else if (fCode == "S" && (bCode == "H" && uType == "Two Way Cassette" && inItem.Model_Hitachi.StartsWith("RCD-") && inItem.Model_Hitachi.EndsWith("FSN3")))
                {
                    isRemark = true;
                }
                if (isRemark)
                {
                    sql = " and (Remark like '%" + inItem.Model_Hitachi + "%' or Remark is null) ";
                }
                #endregion
            }
            else
            {
                if (!string.IsNullOrEmpty(inItem.Series))
                {
                    if (series.Contains(inItem.Series) || (inItem.Series == "Residential VRF HP, HNRQ" && SubRegionCode == "LA_SC"))
                    {
                        if (fCode == "Q" && bCode == "H")
                        {
                            #region //是否需要使用Q工厂的特殊数据处理(HAPQ) on 230180925 by xyj
                            if (uType == "Compact Ducted" && inItem.Model_Hitachi.StartsWith("RPIZ-") && inItem.Model_Hitachi.EndsWith("HNATNQ"))
                            {
                                sql = " and Remark='RPIZ-HNATNQ' ";
                            }
                            else if (uType == "Compact Ducted (DC)" && inItem.Model_Hitachi.StartsWith("RPIZ-") && inItem.Model_Hitachi.EndsWith("HNDTSQ"))
                            {
                                sql = " and Remark='RPIZ-HNDTSQ' ";
                            }
                            else if (uType == "Four way cassette" && inItem.Model_Hitachi.StartsWith("RCI-") && inItem.Model_Hitachi.EndsWith("FSKDNQ"))
                            {
                                sql = " and Remark='RCI-FSKDNQ' ";
                            }
                            else if (uType == "High Static Ducted")
                            {
                                if (inItem.Model_Hitachi.StartsWith("RPIH-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
                                {
                                    sql = " and Remark='RPIH-HNAUNQ' ";
                                }
                                else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQH"))
                                {
                                    sql = " and Remark='RPI-FSNQH' ";
                                }
                                else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQ"))
                                {
                                    sql = " and Remark='RPI-FSNQ' ";
                                }
                            }
                            else if (uType == "Low Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIL-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
                            {
                                if (inItem.Model_Hitachi.StartsWith("RPIL-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
                                {
                                    sql = " and Remark='RPIL-HNAUNQ' ";
                                }
                                else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQL"))
                                {
                                    sql = " and Remark='RPI-FSNQL' ";
                                }
                            }
                            else if (uType == "Medium Static Ducted")
                            {
                                if (inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("HNAUNQ"))
                                {
                                    sql = " and Remark='RPIM-HNAUNQ' ";
                                }
                                else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSNQH"))
                                {
                                    sql = " and Remark='RPI-FSNQH' ";
                                }
                                else if (inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3Q"))
                                {
                                    sql = " and Remark='RPI-FSN3Q' ";
                                }
                            }
                            else if (uType == "Floor Ceiling" && inItem.Model_Hitachi.StartsWith("RPFC-") && inItem.Model_Hitachi.EndsWith("FSNQ"))
                            {
                                sql = " and Remark='RPFC-FSNQ' ";
                            }
                            else if (uType == "Floor Concealed" && inItem.Model_Hitachi.StartsWith("RPFI-") && inItem.Model_Hitachi.EndsWith("FSNQ"))
                            {
                                sql = " and Remark='RPFI-FSNQ' ";
                            }
                            else if (uType == "Fresh Air" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("KFNQ"))
                            {
                                sql = " and Remark='RPI-KFNQ' ";
                            }
                            else if (uType == "High Wall" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSNQS"))
                            {
                                sql = " and Remark='RPK-FSNQS' ";
                            }
                            else if (uType == "Slim Ducted" && inItem.Model_Hitachi.StartsWith("RPIZ-") && inItem.Model_Hitachi.EndsWith("FSNQS"))
                            {
                                sql = " and Remark='RPIZ-FSNQS' ";
                            }
                            #endregion
                        }
                        else if (fCode == "S" && bCode == "H")
                        {
                            if (uType == "Ceiling suspended" && inItem.Model_Hitachi.StartsWith("RPC-") && inItem.Model_Hitachi.EndsWith("FSN3"))
                            {
                                sql = " and Remark='RPC-FSN3' ";
                            }
                            else if (uType == "Mini Four Way Cassette" && inItem.Model_Hitachi.StartsWith("RCIM-") && inItem.Model_Hitachi.EndsWith("FSN4"))
                            {
                                sql = " and Remark='RCIM-FSN4' ";
                            }
                            else if (uType == "One way Cassette" && inItem.Model_Hitachi.StartsWith("RCS-") && inItem.Model_Hitachi.EndsWith("FSNS"))
                            {
                                sql = " and Remark='RCS-FSNS' ";
                            }
                            else if (uType == "Two Way Cassette" && inItem.Model_Hitachi.StartsWith("RCD-") && inItem.Model_Hitachi.EndsWith("FSN3"))
                            {
                                sql = " and Remark='RCD-FSN3' ";
                            }
                        }
                        else if (fCode == "M" && bCode == "H")
                        {
                            #region //是否需要使用M工厂的特殊数据处理 on 230180925 by xyj
                            if (uType == "High Wall" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSN4M"))
                            {
                                sql = " and Remark='RPK-FSN4M' ";
                            }
                            #endregion
                        }
                    }
                    else if (uType == "High Wall" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSN4M") && !isWuxiGen2(inItem.Series))
                    {
                        if (inItem.Series == "Residential VRF HP, HNRQ1")
                        {
                            sql = " and (Remark<>'RPK-FSN4M' or Remark is null) ";
                        }
                        else
                        {
                            sql = " and Remark='RPK-FSN4M' ";
                        }
                    }
                    else if (uType == "High Wall (w/o EXV)" && inItem.Model_Hitachi.StartsWith("RPK-") && inItem.Model_Hitachi.EndsWith("FSNH4M") && !isWuxiGen2(inItem.Series))
                    {
                        if (inItem.Series == "Residential VRF HP, HNRQ1")
                        {
                            sql = " and (Remark<>'RPK-FSNH4M' or Remark is null ) ";
                        }
                        else
                        {
                            sql = " and Remark='RPK-FSNH4M' ";
                        }
                    }

                }
            }
            return sql;
        }

        public Accessory GetItems(string type, string model_Hitachi, Indoor inItem, string RegionCode, string SubRegionCode)
        {
            if (inItem == null)
                return null;

            //string modelfull = inItem.ModelFull;
            string modelYork = inItem.Model_York;
            //string fCode = modelfull.Substring(modelfull.Length - 1, 1);
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string fCode = inItem.GetFactoryCodeForAccess();
            string bCode = (string.IsNullOrEmpty(modelYork) || modelYork == "-") ? "H" : "Y";
            string uType = inItem.Type;

            // add HAPE 20160618 by Yunxiao Lin
            //fCode = (fCode == "Q") ? "Q" : "S";

            //string sql = "select * from JCHVRF_Accessory where FactoryCode = '" + fCode
            //    + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
            //    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity
            //    + " and Type='" + type + "' and Model_Hitachi='" + model_Hitachi + "'";

            //  string sql = string.Empty;

            string sql = " 1=1 ";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            if (dt != null && dt.Rows.Count > 0)
            {
                //sql= "FactoryCode = '" + fCode
                //+ "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                //+ " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity
                //+ " and Type='" + type + "' and Model_Hitachi='" + model_Hitachi + "'";
                // 增加UnitType='Ventilation' 判断，Ventilation相当于Indoor Unit，但是没有Capacity参数 20160618 by Yunxiao Lin

                if (uType.Contains("Heat Exchanger"))
                {
                    sql += " and FactoryCode = '" + fCode + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "') and Type='" + type + "'";
                    if (bCode == "Y")
                    {
                        sql += " and Model_York='" + model_Hitachi + "'";
                    }
                    else
                    {
                        sql += " and Model_Hitachi='" + model_Hitachi + "'";
                    }
                }
                else
                {
                    sql += " and FactoryCode = '" + fCode
                    + "' and BrandCode = '" + bCode + "' and (((UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity + ") or UnitType = 'Ventilation')"
                    + " and Type='" + type + "'";

                    if (bCode == "Y")
                    {
                        sql += " and Model_York='" + model_Hitachi + "'";
                    }
                    else
                    {
                        sql += " and Model_Hitachi='" + model_Hitachi + "'";
                    }

                }
                //Indoor Unit排除High Wall
                if (uType == "High Wall" || uType == "High Wall (NA)" || uType == "High Wall (w/o EXV)")
                {
                    sql += " and Model_Hitachi<>'THM-R2A'";
                }
                // 处理MAL的特殊情况
                //if (inItem.RegionCode != "MAL")
                if ((inItem.RegionCode != "MAL" && inItem.RegionCode != "LA_BV") || bCode == "H" || isWuxiGen2(inItem.Series)) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
                {
                    // 玻利维亚York产品线和马来西亚一样 20170629 by Yunxiao Lin
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    sql += " and Model_York not like '%QE'  and (Remark<>'MAL' or Remark is null)";
                }
                else
                {
                    sql += " and (Remark='MAL' or (  Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    sql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    sql += " and  Model_York <>'JCRB10NEWS'))";
                }
                //由于产品线更新的原因，SMZ High Duct 和 Medium Ducted 会存在新老(FSN2/FSN3)型号并存的情况，在获取Accessory时须进行区分 20170928 by Yunxiao Lin
                if (fCode == "S" && bCode == "H" && uType == "High Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPI-FSN3'";
                }
                else if (fCode == "S" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPIM-FSN3' ";
                }
                string Model_Hitachi = inItem.Model_Hitachi;
                if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S")
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                    if (inItem.Model_Hitachi.Contains('(') && inItem.Model_Hitachi.Contains(')'))
                    {
                        Model_Hitachi = inItem.Model_Hitachi.Substring(0, inItem.Model_Hitachi.IndexOf("(")).Trim();
                    }
                    if (bCode == "H" && uType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3"))
                    {
                        sql += " and (Remark='RPC-FSN3' or Remark is null) ";
                    }
                    if (bCode == "H" && uType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3E"))
                    {
                        sql += " and (Remark='RPC-FSN3E' or Remark is null) ";
                    }
                    if (bCode == "H" && uType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.EndsWith("FSN4E"))
                    {
                        sql += " and (Remark='RPIM-FSN4E' or Remark IS NULL) ";
                    }
                    if (bCode == "H" && uType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.Contains("FSN4E-DU"))
                    {
                        sql += " and (Remark='RPIM-FSN4E-DU' or Remark IS NULL) ";
                    }
                    if (bCode == "H" && uType == "Medium Static Ducted" && Model_Hitachi.StartsWith("RPI-") && Model_Hitachi.Contains("FSN5E"))
                    {
                        if (inItem.Model_Hitachi == "RPI-0.4FSN5E")
                        {
                            sql += " and (Remark='RPI-0.4FSN5E' or Remark IS NULL)";
                        }
                        else
                        {
                            sql += " and (Remark='RPI-FSN5E' or Remark IS NULL)";
                        }
                    }
                    if (bCode == "H" && uType.Contains("Total Heat Exchanger") && Model_Hitachi.StartsWith("KPI-"))
                    {
                        if ((Model_Hitachi.Substring(0, Model_Hitachi.Length - 3).Length > 0))
                        {
                            if (Model_Hitachi == "KPI-1502E4E" || Model_Hitachi == "KPI-2002E4E")
                                sql += " and (Remark LIKE '%" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "%' or Remark is null)";
                            else
                                sql += " and (Remark='" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "' or Remark is null) ";
                        }
                    }
                }
                else // 封装Region和Series判断 20181121 by Yunxiao Lin
                {
                    sql = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, fCode, bCode, inItem.Series, inItem, sql);
                }
                //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20171229 by Yunxiao Lin
                //|| SubRegionCode == "LA_PERU"
                //|| SubRegionCode == "LA_SC")
                //&& inItem.ProductType.Contains("Comm. Tier 4,"))
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
                //else if (isWuxiGen2)
                //{
                //    sql += " and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                //else
                //{
                //    if (IsSpecial(SubRegionCode, fCode, bCode, inItem.Type, inItem))
                //    {
                //        sql += " and Series like '%;" + inItem.Series + ";%' ";
                //    }
                //    else
                //    {
                //        sql += " and RegionCode is null and Series is null";
                //    }
                //}

                //新增特殊数据处理    on 20180801 by xyj   
                sql += GetSpecialData_SQL(SubRegionCode, fCode, bCode, uType, inItem);

                ////处理LA_MMA Tier 1+ only add by axj 20160624
                //if (inItem.RegionCode == "LA_MMA" && inItem.ProductType == "Comm. Tier 1+, HP")
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'JCSA10NEWS' and Model_York <>'JRDA11NEWS' and Model_York<>'JCWB10NEWS'  and Model_York<>'JCRA10NEWQ' and  Model_York<>'JCRB10NEWS' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'RDH-01QE' and Model_York <>'JRDA10NEWQ' and Model_York<>'JCWA10NEWQ' and Model_York<>'CWH-01QE' and Model_York<>'CRH-01QE'";
                //    //S-Y
                //    sql += " and Model_York<>'JP-AP160NA1' and Model_York<>'JP-AP160NAE' and Model_York<>'JR4A10NEWS' ";
                //}
                //else
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'CIS01' and  Model_York <> 'CWDIRK01' and Model_York <>'PSC-5RA' and Model_York<>'CIW01' and Model_York<>'CIR01' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'KW-PP10Q' and Model_York <>'KW-PP5Q' and Model_York <>'KW-PP6Q' and Model_York <>'KW-PP7Q' and Model_York <>'KW-PP8Q' and Model_York <>'KW-PP9Q' and Model_York<>'CWDIRK01' ";
                //    //S-Y
                //    sql += " and Model_York<>'PI-160LS2' and Model_York<>'PIS-56LS' and Model_York<>'P-AP160NA2'  and Model_York<>'P-AP160NAE1' and Model_York<>'F-56MS-PK2' and Model_York<>'PD-75A' and Model_York<>'PD-100' and Model_York<>'B-160H3' and Model_York<>'OACI-160K3' and Model_York<>'DG-56SW1' and Model_York<>'SOR-NES'";
                //    sql += " and Model_York<>'C4IRK01' and Model_York<>'C1IRK01' and Model_York<>'TKCI-160K' ";
                //}
                DataView dv = dt.DefaultView;
                dv.RowFilter = sql;
                dt = dv.ToTable();
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    Accessory item = new Accessory();
                    item.FactoryCode = fCode;
                    item.BrandCode = bCode;
                    item.UnitType = uType;
                    item.MinCapacity = dr["MinCapacity"] is DBNull ? 0 : double.Parse(dr["MinCapacity"].ToString());
                    item.MaxCapacity = dr["MaxCapacity"] is DBNull ? 0 : double.Parse(dr["MaxCapacity"].ToString());
                    item.Type = type;
                    item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                    item.Model_Hitachi = model_Hitachi;
                    item.MaxNumber = dr["MaxNumber"] is DBNull ? 0 : int.Parse(dr["MaxNumber"].ToString());
                    item.IsDefault = bool.Parse(dr["IsDefault"].ToString());
                    return item;
                }
            }

            return null;
        }

        public Accessory GetItem(string type, Indoor inItem, string RegionCode, string SubRegionCode)
        {
            if (inItem == null)
                return null;

            //string modelfull = inItem.ModelFull;
            string modelYork = inItem.Model_York;
            //string fCode = modelfull.Substring(modelfull.Length - 1, 1);
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string fCode = inItem.GetFactoryCodeForAccess();
            string bCode = (string.IsNullOrEmpty(modelYork) || modelYork == "-") ? "H" : "Y";
            string uType = inItem.Type;

            // add HAPE 20160618 by Yunxiao Lin
            //fCode = (fCode == "Q") ? "Q" : "S";

            //string sql = "select * from JCHVRF_Accessory where FactoryCode = '" + fCode
            //    + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
            //    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity
            //    + " and Type='" + type + "'";

            // string sql = string.Empty;
            string sql = " 1=1 ";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            //DataTable dt = _dao.GetDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                //sql=" FactoryCode = '" + fCode
                //+ "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                //+ " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity
                //+ " and Type='" + type + "'";
                // 增加UnitType='Ventilation' 判断，Ventilation相当于Indoor Unit，但是没有Capacity参数 20160618 by Yunxiao Lin

                if (uType.Contains("Heat Exchanger"))
                {
                    sql += " and FactoryCode = '" + fCode + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "') and Type='" + type + "'";
                }
                else
                {

                    sql += " and FactoryCode = '" + fCode
                    + "' and BrandCode = '" + bCode + "' and (((UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity + ") or UnitType = 'Ventilation')"
                    + " and Type='" + type + "'";
                }
                //Indoor Unit排除High Wall
                if (uType == "High Wall" || uType == "High Wall (NA)" || uType == "High Wall (w/o EXV)")
                {
                    sql += " and Model_Hitachi<>'THM-R2A'";
                }
                //默认匹配设置 add by axj 20160623
                if ((fCode == "Q") && (bCode == "H" || bCode == "Y") && type == "Wireless Remote Control Switch" && !uType.Contains("(NA)") && SubRegionCode != "ANZ")
                {
                    sql = sql + " and Model_Hitachi='PC-LH3A'";
                }
                //if ((fCode == "Q") && bCode == "Y" && type == "Wireless Remote Control Switch")
                //{
                //    sql = sql + " and Model_Hitachi='PC-LH3A'";
                //}
                // 处理MAL的特殊情况
                //if (inItem.RegionCode != "MAL")
                if ((inItem.RegionCode != "MAL" && inItem.RegionCode != "LA_BV") || bCode == "H" || isWuxiGen2(inItem.Series)) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
                {
                    // 玻利维亚的York产品线和马来西亚是一样的 20170629 by Yunxiao lin
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    sql += " and Model_York not like '%QE'  and (Remark<>'MAL' or Remark is null)";
                }
                else
                {
                    sql += " and (Remark='MAL' or ( Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    sql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    sql += " and  Model_York <>'JCRB10NEWS'))";
                }
                //由于产品线更新的原因，SMZ High Duct 和 Medium Ducted 会存在新老(FSN2/FSN3)型号并存的情况，在获取Accessory时须进行区分 20170928 by Yunxiao Lin
                if (fCode == "S" && bCode == "H" && uType == "High Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPI-FSN3'";
                }
                else if (fCode == "S" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPIM-FSN3'";
                }
                if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S")
                {
                    string Model_Hitachi = inItem.Model_Hitachi;
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                    if (inItem.Model_Hitachi.Contains('(') && inItem.Model_Hitachi.Contains(')'))
                    {
                        Model_Hitachi = inItem.Model_Hitachi.Substring(0, inItem.Model_Hitachi.IndexOf("(")).Trim();
                    }
                    if (bCode == "H" && uType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3"))
                    {
                        sql += " and (Remark='RPC-FSN3' or Remark is null)";
                    }
                    if (bCode == "H" && uType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3E"))
                    {
                        sql += " and (Remark='RPC-FSN3E' or Remark is null)";
                    }
                    if (bCode == "H" && uType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.EndsWith("FSN4E"))
                    {
                        sql += " and (Remark='RPIM-FSN4E' or Remark IS NULL)";
                    }
                    if (bCode == "H" && uType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.Contains("FSN4E-DU"))
                    {
                        sql += " and (Remark='RPIM-FSN4E-DU' or Remark IS NULL)";
                    }
                    if (bCode == "H" && uType == "Medium Static Ducted" && Model_Hitachi.StartsWith("RPI-") && Model_Hitachi.Contains("FSN5E"))
                    {
                        if (Model_Hitachi == "RPI-0.4FSN5E")
                        {
                            sql += " and (Remark='RPI-0.4FSN5E' or Remark IS NULL)";
                        }
                        else
                        {
                            sql += " and (Remark='RPI-FSN5E' or Remark IS NULL)";
                        }
                    }
                    if (bCode == "H" && uType.Contains("Total Heat Exchanger") && Model_Hitachi.StartsWith("KPI-"))
                    {
                        if ((Model_Hitachi.Substring(0, Model_Hitachi.Length - 3).Length > 0))
                        {
                            if (Model_Hitachi == "KPI-1502E4E" || Model_Hitachi == "KPI-2002E4E")
                                sql += " and (Remark LIKE '%" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "%' or Remark is null)";
                            else
                                sql += " and (Remark='" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "' or Remark is null)";
                        }
                    }
                }
                else //封装Region和Series判断 201801121 by Yunxiao Lin
                {
                    sql = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, fCode, bCode, inItem.Series, inItem, sql);
                }
                //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20171229 by Yunxiao Lin
                //|| SubRegionCode == "LA_PERU"
                //|| SubRegionCode == "LA_SC")
                //&& inItem.ProductType.Contains("Comm. Tier 4,"))
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
                //else if (isWuxiGen2)
                //{
                //    sql += " and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                //else
                //{
                //    if (IsSpecial(SubRegionCode, fCode, bCode, inItem.Type, inItem))
                //    {
                //        sql += " and Series like '%;" + inItem.Series + ";%' ";
                //    }
                //    else
                //    {
                //        sql += " and RegionCode is null and Series is null";
                //    }
                //}
                //新增特殊数据处理    on 20180801 by xyj  
                sql += GetSpecialData_SQL(SubRegionCode, fCode, bCode, uType, inItem);
                ////处理LA_MMA Tier 1+ only add by axj 20160624
                //if (inItem.RegionCode == "LA_MMA" && inItem.ProductType == "Comm. Tier 1+, HP")
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'JCSA10NEWS' and Model_York <>'JRDA11NEWS' and Model_York<>'JCWB10NEWS'  and Model_York<>'JCRA10NEWQ' and  Model_York<>'JCRB10NEWS' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'RDH-01QE' and Model_York <>'JRDA10NEWQ' and Model_York<>'JCWA10NEWQ' and Model_York<>'CWH-01QE' and Model_York<>'CRH-01QE'";
                //    //S-Y
                //    sql += " and Model_York<>'JP-AP160NA1' and Model_York<>'JP-AP160NAE' and Model_York<>'JR4A10NEWS' ";
                //}
                //else
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'CIS01' and  Model_York <> 'CWDIRK01' and Model_York <>'PSC-5RA' and Model_York<>'CIW01' and Model_York<>'CIR01' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'KW-PP10Q' and Model_York <>'KW-PP5Q' and Model_York <>'KW-PP6Q' and Model_York <>'KW-PP7Q' and Model_York <>'KW-PP8Q' and Model_York <>'KW-PP9Q' and Model_York<>'CWDIRK01' ";
                //    //S-Y
                //    sql += " and Model_York<>'PI-160LS2' and Model_York<>'PIS-56LS' and Model_York<>'P-AP160NA2'  and Model_York<>'P-AP160NAE1' and Model_York<>'F-56MS-PK2' and Model_York<>'PD-75A' and Model_York<>'PD-100' and Model_York<>'B-160H3' and Model_York<>'OACI-160K3' and Model_York<>'DG-56SW1' and Model_York<>'SOR-NES'";
                //    sql += " and Model_York<>'C4IRK01' and Model_York<>'C1IRK01' and Model_York<>'TKCI-160K' ";
                //}

                DataView dv = dt.DefaultView;
                dv.RowFilter = sql;
                dt = dv.ToTable();
                if (dt.Rows.Count == 0)
                {
                    return null;
                }

                if (dt.Rows.Count > 1)
                {
                    if (dt.Rows[0]["Type"].ToString() == "Receiver Kit for Wireless Control")
                    {
                        DataView dvs = dt.DefaultView;
                        // dvs.RowFilter = " Model_Hitachi='PC-ALHZ1' ";
                        dvs.Sort = "Model_Hitachi asc";
                        dt = dvs.ToTable();
                    }
                }
                DataRow dr = dt.Rows[0];

                Accessory item = new Accessory();
                item.FactoryCode = fCode;
                item.BrandCode = bCode;
                item.UnitType = uType;
                item.MinCapacity = dr["MinCapacity"] is DBNull ? 0 : double.Parse(dr["MinCapacity"].ToString());
                item.MaxCapacity = dr["MaxCapacity"] is DBNull ? 0 : double.Parse(dr["MaxCapacity"].ToString());
                item.Type = type;
                item.Model_York = dr["Model_York"] is DBNull ? "" : dr["Model_York"].ToString();
                item.Model_Hitachi = dr["Model_Hitachi"] is DBNull ? "" : dr["Model_Hitachi"].ToString();
                item.MaxNumber = dr["MaxNumber"] is DBNull ? 0 : int.Parse(dr["MaxNumber"].ToString());
                item.Count = 1; //Shweta
                item.IsSelect = true;//Shweta
                item.IsDefault = bool.Parse(dr["IsDefault"].ToString());
                return item;
            }

            return null;
        }

        public DataTable GetAllAvailable(Indoor inItem, string RegionCode, string SubRegionCode)
        {
            //string modelfull = inItem.ModelFull;
            string modelYork = inItem.Model_York;
            //string fCode = modelfull.Substring(modelfull.Length - 1, 1);
            // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
            string fCode = inItem.GetFactoryCodeForAccess();
            string bCode = (string.IsNullOrEmpty(modelYork) || modelYork == "-") ? "H" : "Y";
            string uType = inItem.Type;
            
            // add HAPE 20160618 by Yunxiao Lin
            //fCode = (fCode == "Q") ? "Q" : "S";

            //string sql = "select * from JCHVRF_Accessory where FactoryCode = '" + fCode
            //    + "' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
            //    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity;
            // string sql=string.Empty;

            string sql = " 1=1 ";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                //sql=  "FactoryCode = '" + fCode+"' and BrandCode = '" + bCode + "' and (UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                //+ " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity;
                // 新增Ventilation类型附件，等同于Indoor Unit类型，但是没有Capacity要求

                if (uType.Contains("Heat Exchanger"))
                {
                    sql += " and FactoryCode = '" + fCode + "' and BrandCode = '" + bCode + "' and (((UnitType = '" + uType + "')))";
                }
                else
                {
                    sql += " and FactoryCode = '" + fCode + "' and BrandCode = '" + bCode + "' and (((UnitType = '" + uType + "' or UnitType = 'Indoor Unit') and "
                    + " MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity + ") or UnitType = 'Ventilation')";
                }
                //Indoor Unit排除High Wall
                if (uType == "High Wall" || uType == "High Wall (NA)" || uType == "High Wall (w/o EXV)")
                {
                    sql += " and Model_Hitachi<>'THM-R2A'";
                }
                //if (uType == "High Wall")
                //{
                //    // 区分High Wall 类型中的A Type与B Type, OK
                //    string code = modelfull.Substring(modelfull.Length - 2, 1);//第13位code
                //    if (code == "A")
                //    {
                //        // 排除B类型
                //       sql += " and Model_Hitachi <> 'EV-1.5N1' and Model_Hitachi <> 'MSF-NP36AH' ";
                //    }
                //    else if (code == "B")
                //    {
                //        sql+= " and Model_Hitachi <> 'MSF-NP63A' and Model_Hitachi <> 'MSF-NP112A' ";
                //    }
                //}
                // 处理MAL的特殊情况
                //if (inItem.RegionCode != "MAL")
                if ((inItem.RegionCode != "MAL" && inItem.RegionCode != "LA_BV") || bCode == "H" || isWuxiGen2(inItem.Series)) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
                {
                    // 玻利维亚的York产品线和马来西亚的一样 20170629 by Yunxiao Lin
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    sql += " and Model_York not like '%QE' and (Remark<>'MAL' or Remark is null) ";
                }
                else
                {
                    sql += " and (Remark='MAL' or (Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    sql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    sql += " and  Model_York <>'JCRB10NEWS'))";
                }
                //由于产品线更新的原因，SMZ High Duct 和 Medium Ducted 会存在新老(FSN2/FSN3)型号并存的情况，在获取Accessory时须进行区分 20170928 by Yunxiao Lin
                if (fCode == "S" && bCode == "H" && uType == "High Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPI-FSN3'";
                }
                else if (fCode == "S" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPIM-FSN3'";
                }
                if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S")
                {
                    string Model_Hitachi = inItem.Model_Hitachi;
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                    if (inItem.Model_Hitachi.Contains('(') && inItem.Model_Hitachi.Contains(')'))
                    {
                        Model_Hitachi = inItem.Model_Hitachi.Substring(0, inItem.Model_Hitachi.IndexOf("(")).Trim();
                    }
                    if (bCode == "H" && uType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3"))
                    {
                        sql += " and (Remark='RPC-FSN3' or Remark is null) ";
                    }
                    if (bCode == "H" && uType == "Medium Static Ducted" && Model_Hitachi.StartsWith("RPI-") && Model_Hitachi.Contains("FSN5E"))
                    {
                        if (Model_Hitachi == "RPI-0.4FSN5E")
                        {
                            sql += " and (Remark='RPI-0.4FSN5E' or Remark IS NULL)";
                        }
                        else
                        {
                            sql += " and (Remark='RPI-FSN5E' or Remark IS NULL)";
                        }
                    }
                    if (bCode == "H" && uType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3E"))
                    {
                        sql += " and (Remark='RPC-FSN3E' or Remark is null) ";
                    }
                    if (bCode == "H" && uType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.EndsWith("FSN4E"))
                    {
                        sql += " and (Remark='RPIM-FSN4E' or Remark IS NULL) ";
                    }
                    if (bCode == "H" && uType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.Contains("FSN4E-DU"))
                    {
                        sql += " and (Remark='RPIM-FSN4E-DU' or Remark IS NULL) ";
                    }
                    if (bCode == "H" && uType.Contains("Total Heat Exchanger") && Model_Hitachi.StartsWith("KPI-"))
                    {
                        if ((Model_Hitachi.Substring(0, Model_Hitachi.Length - 3).Length > 0))
                        {
                            if (Model_Hitachi == "KPI-1502E4E" || Model_Hitachi == "KPI-2002E4E")
                                sql += " and (Remark LIKE '%" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "%' or Remark is null)";
                            else
                                sql += " and (Remark='" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "' or Remark is null) ";
                        }
                    }
                }
                else //封装Region和Series判断 201801121 by Yunxiao Lin
                {
                    sql = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, fCode, bCode, inItem.Series, inItem, sql);
                }
                //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20171229 by Yunxiao Lin
                //|| SubRegionCode == "LA_PERU"
                //|| SubRegionCode == "LA_SC")
                //&& inItem.ProductType.Contains("Comm. Tier 4,"))
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
                //else if (isWuxiGen2)
                //{
                //    sql += " and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                //else
                //{
                //    if (IsSpecial(SubRegionCode, fCode, bCode, inItem.Type, inItem))
                //    {
                //        sql += " and Series like '%;" + inItem.Series + ";%' ";
                //    }
                //    else
                //    {
                //        sql += " and RegionCode is null and Series is null";
                //    }
                //}

                //新增特殊数据处理    on 20180801 by xyj   
                sql += GetSpecialData_SQL(SubRegionCode, fCode, bCode, uType, inItem);
                ////处理LA_MMA Tier 1+ only add by axj 20160624
                //if (inItem.RegionCode == "LA_MMA" && inItem.ProductType == "Comm. Tier 1+, HP")
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'JCSA10NEWS' and Model_York <>'JRDA11NEWS' and Model_York<>'JCWB10NEWS'  and Model_York<>'JCRA10NEWQ' and  Model_York<>'JCRB10NEWS' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'RDH-01QE' and Model_York <>'JRDA10NEWQ' and Model_York<>'JCWA10NEWQ' and Model_York<>'CWH-01QE' and Model_York<>'CRH-01QE'";
                //    //S-Y
                //    sql += " and Model_York<>'JP-AP160NA1' and Model_York<>'JP-AP160NAE' and Model_York<>'JR4A10NEWS' ";
                //}
                //else
                //{
                //    //M-Y
                //    sql += " and Model_York <> 'CIS01' and  Model_York <> 'CWDIRK01' and Model_York <>'PSC-5RA' and Model_York<>'CIW01' and Model_York<>'CIR01' ";
                //    //Q-Y
                //    sql += " and Model_York <> 'KW-PP10Q' and Model_York <>'KW-PP5Q' and Model_York <>'KW-PP6Q' and Model_York <>'KW-PP7Q' and Model_York <>'KW-PP8Q' and Model_York <>'KW-PP9Q' and Model_York<>'CWDIRK01' ";
                //    //S-Y
                //    sql += " and Model_York<>'PI-160LS2' and Model_York<>'PIS-56LS' and Model_York<>'P-AP160NA2'  and Model_York<>'P-AP160NAE1' and Model_York<>'F-56MS-PK2' and Model_York<>'PD-75A' and Model_York<>'PD-100' and Model_York<>'B-160H3' and Model_York<>'OACI-160K3' and Model_York<>'DG-56SW1' and Model_York<>'SOR-NES'";
                //    sql += " and Model_York<>'C4IRK01' and Model_York<>'C1IRK01' and Model_York<>'TKCI-160K' ";
                //}


                // 【SMZ】/。。/【Wireless Remote Control Switch】/【PC-LH3A】--- Can be used for RPI-FSN2, RPF-FSN2E, RPFI-FSN2E only. Use together with Wireless receiver kit.
                // 【SMZ】/。。/【Wireless Remote Control Switch】/【PC-LH3B】--- 【Indoor Unit】，Use together with Wireless receiver kit.
                //if (fCode == "S")
                //{
                //    if (bCode == "Y")
                //    {
                //        if (inItem.RegionCode != "LA_BR") // LA_MMA
                //        {
                //            sql+= " and Model_York <> 'JCRB10NBWS' and Model_York <> 'JCVB10NBWS'";
                //        }
                //        else // LA_BR only
                //        {
                //            sql+= " and Model_York <> 'JCRB10NEWS' and Model_York <> 'JCWB10NEWS'";
                //        }
                //    }
                //    else if (bCode == "H")
                //    {
                //        Regex reg1 = new Regex("^RPI-.*FSN2$");
                //        Regex reg2 = new Regex("^RPF-.*FSN2E$");
                //        Regex reg3 = new Regex("^RPFI-.*FSN2E$");
                //        string model = inItem.Model_Hitachi;
                //        if (reg1.IsMatch(model) || reg2.IsMatch(model) || reg3.IsMatch(model))
                //        {
                //            sql+= " and Model_Hitachi <> 'PC-LH3B'"; // 区别“Indoor Unit”Type
                //        }
                //    }
                //}
                dv.RowFilter = sql;
                dv.Sort = "Type asc";
                return dv.ToTable();
            }

            return null;
        }


        public DataTable GetAllAvailableSameAccessory(List<Indoor> inItemList, string RegionCode, string SubRegionCode)
        {
            Trans trans = new Trans();
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("Model_Hitachi");
            dtResult.Columns.Add("Model_York");
            dtResult.Columns.Add("Type");
            dtResult.Columns.Add("MaxNumber");
            dtResult.Columns.Add("IsDefault");
            dtResult.Columns.Add("Number");
            dtResult.Columns.Add("TypeDisplay");

            string bCode = (string.IsNullOrEmpty(inItemList[0].Model_York) || inItemList[0].Model_York == "-") ? "H" : "Y";


            int rowCount = 0;
            string sql = "";
            foreach (Indoor inItem in inItemList)
            {
                rowCount++;
                //增加Wuxi Gen 2 特殊判断 20180639 by Yunxiao Lin
                bool isWuxiGen2 = false;
                if (inItem.Series == "Commercial VRF HP, HNCQ" ||
                    inItem.Series == "Commercial VRF HP, HNBQ" ||
                    inItem.Series == "Residential VRF HP, HNRQ" ||
                    inItem.Series == "Commercial VRF HP, JVOHQ" ||
                    inItem.Series == "Residential VRF HP, JROHQ")
                    isWuxiGen2 = true;
                //string fCode = inItem.ModelFull.Substring(inItem.ModelFull.Length - 1, 1);
                // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
                string fCode = inItem.GetFactoryCodeForAccess();
                sql = "select  *  from JCHVRF_Accessory where 1=1  and BrandCode = '" + bCode + "' and " +
                          " (((UnitType = '" + inItem.Type + "' or UnitType = 'Indoor Unit') and  MinCapacity<=" + inItem.CoolingCapacity + " and MaxCapacity >=" + inItem.CoolingCapacity + " and FactoryCode='" + fCode + "') or UnitType = 'Ventilation') ";

                //if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20171229 by Yunxiao Lin
                //|| SubRegionCode == "LA_PERU"
                //|| SubRegionCode == "LA_SC")
                //&& inItem.ProductType.Contains("Comm. Tier 4,"))
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
                //else if (isWuxiGen2)
                //{
                //    sql += " and Series like '%;" + inItem.Series + ";%' ";
                //}
                //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
                //{
                //    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                //else
                //{
                //    if (IsSpecial(SubRegionCode, fCode, bCode, inItem.Type, inItem))
                //    { 
                //        sql += " and Series like '%;" + inItem.Series + "%' "; 
                //    }
                //    else {
                //        sql += " and RegionCode is null and Series is null";
                //    }
                //}
                //封装Region和Series判断 201801121 by Yunxiao Lin
                sql = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, fCode, bCode, inItem.Series, inItem, sql);

                if (inItem.Type == "High Wall" || inItem.Type == "High Wall (NA)" || inItem.Type == "High Wall (w/o EXV)")
                {
                    sql += " and Model_Hitachi<>'THM-R2A' ";
                }

                if (fCode == "S" && bCode == "H" && inItem.Type == "High Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPI-FSN3'";
                }
                else if (fCode == "S" && bCode == "H" && inItem.Type == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    sql += " and Remark='RPIM-FSN3'";
                }
                if ((inItemList[0].RegionCode != "MAL" && inItemList[0].RegionCode != "LA_BV") || bCode == "H" || isWuxiGen2) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
                {
                    // 玻利维亚的York产品线和马来西亚的一样
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    //Wuxi Gen2 例外，没有'%QE'结尾                   
                    sql += " and Model_York not like '%QE' and (Remark<>'MAL' or Remark is null) ";                 
                }
                else
                {
                    sql += " and (Remark='MAL' or (Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    sql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    sql += " and  Model_York <>'JCRB10NEWS')) ";
                }

                //新增特殊数据处理    on 20180801 by xyj  
                sql += GetSpecialData_SQL(SubRegionCode, fCode, bCode, inItem.Type, inItem);
                DataTable dtsql = _dao.GetDataTable(sql);
                if (dtsql != null && dtsql.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtsql.Rows)
                    {
                        if (!IsExistsDataTable(dtResult, dr["Model_Hitachi"].ToString(), dr["Model_York"].ToString(), dr["Type"].ToString(), dr["MaxNumber"].ToString(), dr["IsDefault"].ToString(), bCode))
                            //dtResult.Rows.Add(dr["Model_Hitachi"].ToString(), dr["Model_York"].ToString(), dr["Type"].ToString(), dr["MaxNumber"].ToString(), dr["IsDefault"].ToString(), "1");
                            dtResult.Rows.Add(dr["Model_Hitachi"].ToString(), dr["Model_York"].ToString(), dr["Type"].ToString(),
                               dr["MaxNumber"].ToString(), dr["IsDefault"].ToString(), "1",
                               trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), dr["Type"].ToString()));
                    }
                }
            }

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                //过滤
                DataView dv = dtResult.DefaultView;
                dv.RowFilter = "Number='" + inItemList.Count + "'";
                dtResult = dv.ToTable();
                return dtResult;
            }
            return null;
        }

        //判断DataTable 是否存在当前数据
        private bool IsExistsDataTable(DataTable dt, string Hitachi, string York, string Type, string MaxNumber, string IsDefault, string Code)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (Code == "Y")
                    {
                        if (dr["Model_York"].ToString() == York && dr["Type"].ToString() == Type && dr["MaxNumber"].ToString() == MaxNumber && dr["IsDefault"].ToString() == IsDefault)
                        {
                            dr["Number"] = (Convert.ToInt32(dr["Number"]) + 1).ToString();
                            return true;
                        }
                    }
                    else
                    {
                        if (dr["Model_Hitachi"].ToString() == Hitachi && dr["Type"].ToString() == Type && dr["MaxNumber"].ToString() == MaxNumber && dr["IsDefault"].ToString() == IsDefault)
                        {
                            dr["Number"] = (Convert.ToInt32(dr["Number"]) + 1).ToString();
                            return true;
                        }
                    }

                }

            }
            return false;
        }


        /// <summary>
        /// 获取可用的Accessory  Heat Exchanger 专用
        /// </summary>
        /// <param name="inItemList"></param>
        /// <param name="RegionCode"></param>
        /// <param name="SubRegionCode"></param>
        /// <returns></returns>
        public DataTable GetAllAvailable(List<Indoor> inItemList, string RegionCode, string SubRegionCode)
        {
            //string fCode = "";
            //string uType = "";
            //List<string> uTypeList = new List<string>();
            //string bCode = (string.IsNullOrEmpty(inItemList[0].Model_York) || inItemList[0].Model_York == "-") ? "H" : "Y";
            //int minCapacity = Convert.ToInt32(inItemList[0].CoolingCapacity);
            //int maxCapacity = Convert.ToInt32(inItemList[0].CoolingCapacity);
            //foreach (Indoor inItem in inItemList)
            //{
            //    string modelfull = inItem.ModelFull;
            //    if (!fCode.Contains(modelfull.Substring(modelfull.Length - 1, 1)))
            //        fCode += "'" + modelfull.Substring(modelfull.Length - 1, 1) + "',";
            //    if (!uType.Contains(inItem.Type))
            //        uType += "'" + inItem.Type + "',";
            //    uTypeList.Add(inItem.Type);
            //    if (Convert.ToInt32(inItem.CoolingCapacity) < minCapacity)
            //        minCapacity = Convert.ToInt32(inItem.CoolingCapacity);
            //    if (Convert.ToInt32(inItem.CoolingCapacity) > maxCapacity)
            //        maxCapacity = Convert.ToInt32(inItem.CoolingCapacity);
            //}
            //fCode = fCode.Substring(0, fCode.Length - 1);
            //uType = uType.Substring(0, uType.Length - 1);


            List<string> fCodeList = new List<string>();
            List<string> uTypeList = new List<string>();
            bool existsFSNS = false;
            bool isWuxiDesign = false;
            string bCode = (string.IsNullOrEmpty(inItemList[0].Model_York) || inItemList[0].Model_York == "-") ? "H" : "Y";
            int minCapacity = Convert.ToInt32(inItemList[0].CoolingCapacity);
            int maxCapacity = Convert.ToInt32(inItemList[0].CoolingCapacity);
            foreach (Indoor inItem in inItemList)
            {
                //string modelfull = inItem.ModelFull;
                // 室内机工厂代码改为类函数获取，用于处理Wuxi design特殊逻辑 20180627 by Yunxiao Lin
                string fCode = inItem.GetFactoryCodeForAccess();
                if (!fCodeList.Contains(fCode))
                    fCodeList.Add(fCode);
                if (!uTypeList.Contains(inItem.Type))
                    // uTypeList.Add(inItem.Type + "," + inItem.CoolingCapacity); 
                    //添加工厂 
                    uTypeList.Add(inItem.Type + "," + inItem.CoolingCapacity + "," + inItem.GetFactoryCodeForAccess());
                if (Convert.ToInt32(inItem.CoolingCapacity) < minCapacity)
                    minCapacity = Convert.ToInt32(inItem.CoolingCapacity);
                if (Convert.ToInt32(inItem.CoolingCapacity) > maxCapacity)
                    maxCapacity = Convert.ToInt32(inItem.CoolingCapacity);
                if (inItem.ProductType.Contains("Comm. Tier 4,"))
                    existsFSNS = true;
                //if (inItem.Series == "Commercial VRF HP, HNCQ" ||
                //    inItem.Series == "Commercial VRF HP, HNBQ" ||
                //    inItem.Series == "Residential VRF HP, HNRQ" ||
                //    inItem.Series == "Commercial VRF HP, JVOHQ" ||
                //    inItem.Series == "Residential VRF HP, JROHQ")
                isWuxiDesign = isWuxiGen2(inItem.Series);
            }

            // string sql = string.Empty;
            string sql = " 1=1 ";

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;

                sql += " and BrandCode = '" + bCode + "'";
                //foreach (string fCode in fCodeList)
                //{
                //    sql += " and FactoryCode='" + fCode +"'";
                //}
                string fCodeStr = "";
                foreach (string fCode in fCodeList)
                {
                    fCodeStr += "'" + fCode + "',";
                }
                sql += " and FactoryCode in(" + fCodeStr.Substring(0, fCodeStr.Length - 1) + ")"; //这样把工厂代码合并判断很不合理，需改善
                if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S")
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                }
                else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20121229 by Yunxiao Lin
                || SubRegionCode == "LA_PERU"
                || SubRegionCode == "LA_SC")
                && existsFSNS)
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and (Series like '%;Commercial VRF HP, FSNS;%' or Series like '%;Commercial VRF HP, JTOH-BS1;%' or Series like '%;Commercial VRF HR, FSXNS;%' or Series like '%;Commercial VRF HR, JTOR-BS1;%') ";
                }
                else if (SubRegionCode == "LA_SC" && isWuxiDesign)
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%HP, HNCQ;%' ";
                }
                else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                }
                //增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
                else if (isWuxiDesign)
                {
                    sql += " and ((Series like '%HP, HNCQ;%' and RegionCode not like '%/LA_SC/%') or Series like '%HP, HNBQ;%' or Series like '%HP, HNRQ;%' or Series like '%HP, JVOHQ;%' or Series like '%HP, JROHQ;%')";
                }
                else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                }
                else if ((SubRegionCode == "ASIA" && bCode == "Y") || SubRegionCode == "BGD" || SubRegionCode == "SEA_60Hz") // ASIA York, BGD, SEA_60Hz，需要判断
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                }
                else
                {
                    sql += " and RegionCode is null and Series is null";
                }

                foreach (string uTypeStr in uTypeList)
                {
                    string[] uType = uTypeStr.Split(',');
                    if (uType[0].Contains("Heat Exchanger"))
                    {
                        sql += " and (((UnitType = '" + uType[0] + "')))";
                    }
                    else
                    {
                        sql += " and (((UnitType = '" + uType[0] + "' or UnitType = 'Indoor Unit') and "
                            + " MinCapacity<=" + uType[1] + " and MaxCapacity >=" + uType[1] + ") or UnitType = 'Ventilation' and FactoryCode = '" + uType[2] + "')";


                        //Indoor Unit排除High Wall
                        if (uType[0] == "High Wall" || uType[0] == "High Wall (NA)" || uType[0] == "High Wall (w/o EXV)")
                        {
                            sql += " and Model_Hitachi<>'THM-R2A'";
                        }
                    }
                }
                if ((inItemList[0].RegionCode != "MAL" && inItemList[0].RegionCode != "LA_BV") || bCode == "H") //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
                {
                    // 玻利维亚的York产品线和马来西亚的一样
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    sql += " and Model_York not like '%QE' and (Remark<>'MAL' or Remark is null) ";
                }
                else
                {
                    sql += " and (Remark='MAL' or (Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    sql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    sql += " and  Model_York <>'JCRB10NEWS'))";
                }
                //由于产品线更新的原因，SMZ High Duct 和 Medium Ducted 会存在新老(FSN2/FSN3)型号并存的情况，在获取Accessory时须进行区分 20170928 by Yunxiao Lin
                bool isFSN3 = false;
                foreach (Indoor inItem in inItemList)
                {
                    string fCode = inItem.GetFactoryCodeForAccess();
                    string uType = inItem.Type;
                    if (fCode == "S" && bCode == "H" && uType == "High Static Ducted" && inItem.Model_Hitachi.StartsWith("RPI-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                    {
                        sql += " and Remark='RPI-FSN3'";
                        isFSN3 = true;
                    }
                    else if (fCode == "S" && bCode == "H" && uType == "Medium Static Ducted" && inItem.Model_Hitachi.StartsWith("RPIM-") && inItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                    {
                        sql += " and Remark='RPIM-FSN3'";
                        isFSN3 = true;
                    }

                    if (isFSN3)
                        break;
                }
                dv.RowFilter = sql;
                dv.Sort = "Type asc";
                if (dv.Count > 0)
                {
                    if (dv.ToTable(true, new string[] { "FactoryCode" }).Rows.Count != fCodeList.Count)   //过滤厂家的记录比对(用于不同厂区分的UnitType记录筛选判断)
                        return null;
                    else
                        return dv.ToTable(true, new string[] { "Model_Hitachi", "Model_York", "Type", "MaxNumber", "IsDefault" });
                }
                else
                    return null;
            }
            return null;
        }


        /// <summary>
        /// 加载对应的Access类型
        /// </summary>
        /// <param name="BrandCode"></param>
        /// <param name="FactoryCode"></param>
        /// <param name="ItemType"></param>
        /// <returns></returns>
        public DataTable GetAvailableType(string BrandCode, string FactoryCode, string ItemType, string RegionCode, string SubRegionCode)
        {
            string sqlwhere = " 1=1 ";
            if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S" || SubRegionCode == "TW")
            {
                sqlwhere += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            }
            else if ((SubRegionCode == "ASIA" && BrandCode == "Y") || SubRegionCode == "BGD" || SubRegionCode == "SEA_60Hz") // ASIA York, BGD, SEA_60Hz，需要判断
            {
                sqlwhere += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            }
            else
                sqlwhere += " and RegionCode is  null";

            string sql = "SELECT  DISTINCT Type FROM JCHVRF_Accessory where " + sqlwhere + " and BrandCode = '" + BrandCode + "' and FactoryCode in(" + FactoryCode + ") and (((UnitType in(" + ItemType + ")or UnitType = 'Indoor Unit') or UnitType = 'Ventilation') and (Remark<>'MAL' or Remark is null) )";
            DataTable dt = _dao.GetDataTable(sql);
            return dt;
        }





        /// <summary>
        /// 加载对应的Access类型
        /// </summary>
        /// <param name="BrandCode"></param>
        /// <param name="FactoryCode"></param>
        /// <param name="ItemType"></param>
        /// <returns></returns>
        //public DataTable GetAvailableListType(string BrandCode, string FactoryCode, string ItemType, string RegionCode, RoomIndoor ri, string SubRegionCode)
        //{
        //    string str = " 1=1 ";

        //    //////增加Wuxi Gen 2 特殊判断 20180630 by Yunxiao Lin
        //    //bool isWuxiGen2 = false;
        //    //if (ri.IndoorItem.Series == "Commercial VRF HP, HNCQ" ||
        //    //    ri.IndoorItem.Series == "Commercial VRF HP, HNBQ" ||
        //    //    ri.IndoorItem.Series == "Residential VRF HP, HNRQ" ||
        //    //    ri.IndoorItem.Series == "Commercial VRF HP, JVOHQ" ||
        //    //    ri.IndoorItem.Series == "Residential VRF HP, JROHQ")
        //    //    isWuxiGen2 = true;

        //    if (ItemType == "High Wall" || ItemType == "High Wall (NA)" || ItemType == "High Wall (w/o EXV)")
        //    {
        //        str += " and Model_Hitachi<>'THM-R2A'";
        //    }
        //    if ((RegionCode != "MAL" && RegionCode != "LA_BV") || BrandCode == "H" || isWuxiGen2(ri.IndoorItem.Series)) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
        //    {
        //        // 玻利维亚的York产品线和马来西亚的一样
        //        // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
        //        str += " and Model_York not like '%QE' and (Remark<>'MAL' or Remark is null) ";
        //    }
        //    else
        //    {
        //        str += " and (Remark='MAL' or (Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
        //        //新增MAL过滤条件 add by axj 20160623
        //        str += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
        //        //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
        //        str += " and  Model_York <>'JCRB10NEWS'))";
        //    }

        //    if (FactoryCode == "S" && BrandCode == "H" && ItemType == "High Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPI-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
        //    {
        //        str += " and Remark='RPI-FSN3'";
        //    }
        //    else if (FactoryCode == "S" && BrandCode == "H" && ItemType == "Medium Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPIM-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
        //    {
        //        str += " and Remark='RPIM-FSN3'";
        //    }
        //    string Model_Hitachi = ri.IndoorItem.Model_Hitachi;
        //    if (RegionCode.Substring(0, 2) == "EU")
        //    {
        //        str += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
        //        if (ri.IndoorItem.Model_Hitachi.Contains('(') && ri.IndoorItem.Model_Hitachi.Contains(')'))
        //        {
        //            Model_Hitachi = ri.IndoorItem.Model_Hitachi.Substring(0, ri.IndoorItem.Model_Hitachi.IndexOf("(")).Trim();
        //        }
        //        if (BrandCode == "H" && ItemType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.EndsWith("FSN4E"))
        //        {
        //            str += " and (Remark='RPIM-FSN4E' or Remark IS NULL)";
        //        }
        //        if (BrandCode == "H" && ItemType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.Contains("FSN4E-DU"))
        //        {
        //            str += " and (Remark='RPIM-FSN4E-DU' or Remark IS NULL)";
        //        }
        //        if (BrandCode == "H" && ItemType == "Medium Static Ducted" && Model_Hitachi.StartsWith("RPI-") && Model_Hitachi.Contains("FSN5E"))
        //        {
        //            if (Model_Hitachi == "RPI-0.4FSN5E")
        //            {
        //                str += " and (Remark='RPI-0.4FSN5E' or Remark IS NULL)";
        //            }
        //            else
        //            {
        //                str += " and (Remark='RPI-FSN5E' or Remark IS NULL)";
        //            }
        //        }
        //        if (BrandCode == "H" && ItemType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3"))
        //        {
        //            str += " and (Remark='RPC-FSN3' or Remark is null)";
        //        }
        //        if (BrandCode == "H" && ItemType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3E"))
        //        {
        //            str += " and (Remark='RPC-FSN3E' or Remark is null)";
        //        }
        //        if (BrandCode == "H" && ItemType.Contains("Total Heat Exchanger") && Model_Hitachi.StartsWith("KPI-"))
        //        {
        //            if ((Model_Hitachi.Substring(0, Model_Hitachi.Length - 3).Length > 0))
        //            {
        //                if (Model_Hitachi == "KPI-1502E4E" || Model_Hitachi == "KPI-2002E4E")
        //                    str += " and (Remark LIKE '%" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "%' or Remark is null)";
        //                else
        //                    str += " and (Remark='" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "' or Remark is null)";
        //            }
        //        }
        //    }
        //    else
        //        str = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, FactoryCode, BrandCode, ri.IndoorItem.Series, ri.IndoorItem, str);
        //    //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20121229 by Yunxiao Lin
        //    //    || SubRegionCode == "LA_PERU"
        //    //    || SubRegionCode == "LA_SC")
        //    //    && ri.IndoorItem.ProductType.Contains("Comm. Tier 4,"))
        //    //{
        //    //    str += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + ri.IndoorItem.Series + ";%' ";
        //    //}
        //    //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
        //    //{
        //    //    str += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
        //    //}
        //    ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
        //    //else if (isWuxiGen2)
        //    //{
        //    //    str += " and Series like '%;" + ri.IndoorItem.Series + ";%' ";
        //    //}
        //    //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
        //    //{
        //    //    str += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
        //    //}
        //    //else
        //    //{
        //    //    if (IsSpecial(SubRegionCode, FactoryCode, BrandCode, ItemType, ri.IndoorItem))
        //    //    {
        //    //        str += " and Series like '%;" + ri.IndoorItem.Series + ";%' ";
        //    //    }
        //    //    else
        //    //    {
        //    //        str += " and RegionCode is null and Series is null";
        //    //    } 
        //    //}
        //    //新增特殊数据处理    on 20180801 by xyj  
        //    str += GetSpecialData_SQL(SubRegionCode, FactoryCode, BrandCode, ItemType, ri.IndoorItem);
        //    string sql = "";
        //    if (ri.IsExchanger)
        //        sql = "SELECT  DISTINCT Type FROM JCHVRF_Accessory where 1=1 and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and (((UnitType ='" + ItemType + "' or UnitType = 'Indoor Unit') or UnitType = 'Ventilation') and " + str + " )";
        //    else
        //        sql = "SELECT  DISTINCT Type FROM JCHVRF_Accessory where 1=1 and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and (((UnitType ='" + ItemType + "' or UnitType = 'Indoor Unit') or UnitType = 'Ventilation') and  MinCapacity<=" + ri.IndoorItem.CoolingCapacity + " and MaxCapacity >=" + ri.IndoorItem.CoolingCapacity + " and " + str + " )";

        //    DataTable dt = _dao.GetDataTable(sql);
        //    return dt;
        //}
        public DataTable GetAvailableListType(string BrandCode, string FactoryCode, string ItemType, string RegionCode, RoomIndoor ri, string SubRegionCode)
        {
            string str = " 1=1 ";

            //////增加Wuxi Gen 2 特殊判断 20180630 by Yunxiao Lin
            //bool isWuxiGen2 = false;
            //if (ri.IndoorItem.Series == "Commercial VRF HP, HNCQ" ||
            //    ri.IndoorItem.Series == "Commercial VRF HP, HNBQ" ||
            //    ri.IndoorItem.Series == "Residential VRF HP, HNRQ" ||
            //    ri.IndoorItem.Series == "Commercial VRF HP, JVOHQ" ||
            //    ri.IndoorItem.Series == "Residential VRF HP, JROHQ")
            //    isWuxiGen2 = true;

            if (ItemType == "High Wall" || ItemType == "High Wall (NA)" || ItemType == "High Wall (w/o EXV)")
            {
                str += " and Model_Hitachi<>'THM-R2A'";
            }
            if ((RegionCode != "MAL" && RegionCode != "LA_BV") || BrandCode == "H" || isWuxiGen2(ri.IndoorItem.Series)) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
            {
                // 玻利维亚的York产品线和马来西亚的一样
                // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                str += " and Model_York not like '%QE' and (Remark<>'MAL' or Remark is null) ";
            }
            else
            {
                str += " and (Remark='MAL' or (Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                //新增MAL过滤条件 add by axj 20160623
                str += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                str += " and  Model_York <>'JCRB10NEWS'))";
            }

            if (FactoryCode == "S" && BrandCode == "H" && ItemType == "High Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPI-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
            {
                str += " and Remark='RPI-FSN3'";
            }
            else if (FactoryCode == "S" && BrandCode == "H" && ItemType == "Medium Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPIM-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
            {
                str += " and Remark='RPIM-FSN3'";
            }
            string Model_Hitachi = ri.IndoorItem.Model_Hitachi;
            if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S")
            {
                str += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                if (ri.IndoorItem.Model_Hitachi.Contains('(') && ri.IndoorItem.Model_Hitachi.Contains(')'))
                {
                    Model_Hitachi = ri.IndoorItem.Model_Hitachi.Substring(0, ri.IndoorItem.Model_Hitachi.IndexOf("(")).Trim();
                }
                if (BrandCode == "H" && ItemType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.EndsWith("FSN4E"))
                {
                    str += " and (Remark='RPIM-FSN4E' or Remark IS NULL)";
                }
                if (BrandCode == "H" && ItemType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.Contains("FSN4E-DU"))
                {
                    str += " and (Remark='RPIM-FSN4E-DU' or Remark IS NULL)";
                }
                if (BrandCode == "H" && ItemType == "Medium Static Ducted" && Model_Hitachi.StartsWith("RPI-") && Model_Hitachi.Contains("FSN5E"))
                {
                    if (Model_Hitachi == "RPI-0.4FSN5E")
                    {
                        str += " and (Remark='RPI-0.4FSN5E' or Remark IS NULL)";
                    }
                    else
                    {
                        str += " and (Remark='RPI-FSN5E' or Remark IS NULL)";
                    }
                }
                if (BrandCode == "H" && ItemType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3"))
                {
                    str += " and (Remark='RPC-FSN3' or Remark is null)";
                }
                if (BrandCode == "H" && ItemType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3E"))
                {
                    str += " and (Remark='RPC-FSN3E' or Remark is null)";
                }
                if (BrandCode == "H" && ItemType.Contains("Total Heat Exchanger") && Model_Hitachi.StartsWith("KPI-"))
                {
                    if ((Model_Hitachi.Substring(0, Model_Hitachi.Length - 3).Length > 0))
                    {
                        if (Model_Hitachi == "KPI-1502E4E" || Model_Hitachi == "KPI-2002E4E")
                            str += " and (Remark LIKE '%" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "%' or Remark is null)";
                        else
                            str += " and (Remark='" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "' or Remark is null)";
                    }
                }
            }
            else
                str = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, FactoryCode, BrandCode, ri.IndoorItem.Series, ri.IndoorItem, str);
            //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20121229 by Yunxiao Lin
            //    || SubRegionCode == "LA_PERU"
            //    || SubRegionCode == "LA_SC")
            //    && ri.IndoorItem.ProductType.Contains("Comm. Tier 4,"))
            //{
            //    str += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + ri.IndoorItem.Series + ";%' ";
            //}
            //else if (SubRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
            //{
            //    str += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            //}
            ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
            //else if (isWuxiGen2)
            //{
            //    str += " and Series like '%;" + ri.IndoorItem.Series + ";%' ";
            //}
            //else if (SubRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
            //{
            //    str += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
            //}
            //else
            //{
            //    if (IsSpecial(SubRegionCode, FactoryCode, BrandCode, ItemType, ri.IndoorItem))
            //    {
            //        str += " and Series like '%;" + ri.IndoorItem.Series + ";%' ";
            //    }
            //    else
            //    {
            //        str += " and RegionCode is null and Series is null";
            //    } 
            //}
            //新增特殊数据处理    on 20180801 by xyj  
            if (!(Project.CurrentProject.RegionCode == "EU_W" ||
                    Project.CurrentProject.RegionCode == "EU_E" ||
                    Project.CurrentProject.RegionCode == "EU_S" || Project.CurrentProject.SubRegionCode == "TW"))
                str += GetSpecialData_SQL(SubRegionCode, FactoryCode, BrandCode, ItemType, ri.IndoorItem);
            string sql = "";
            if (ri.IsExchanger)
                sql = "SELECT  DISTINCT Type FROM JCHVRF_Accessory where 1=1 and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and (((UnitType ='" + ItemType + "' or UnitType = 'Indoor Unit') or UnitType = 'Ventilation') and " + str + " )";
            else
                sql = "SELECT  DISTINCT Type FROM JCHVRF_Accessory where 1=1 and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and (((UnitType ='" + ItemType + "' or UnitType = 'Indoor Unit') or UnitType = 'Ventilation') and  MinCapacity<=" + ri.IndoorItem.CoolingCapacity + " and MaxCapacity >=" + ri.IndoorItem.CoolingCapacity + " and " + str + " )";

            DataTable dt = _dao.GetDataTable(sql);
            return dt;
        }

        /// <summary>
        /// 获取室内机配件 根据UnitType Type 筛选配件
        /// </summary>
        /// <param name="BrandCode"></param>
        /// <param name="FactoryCode"></param>
        /// <param name="UnitType"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public DataTable GetInDoorAccessoryItemList(string BrandCode, string FactoryCode, string UnitType, double Capacity, string RegionCode, RoomIndoor ri, string SubRegionCode)
        {
            string strsql = " 1=1 ";

            ////增加Wuxi Gen 2 特殊判断 20180630 by Yunxiao Lin
            //bool isWuxiGen2 = false;
            //if (ri.IndoorItem.Series == "Commercial VRF HP, HNCQ" ||
            //        ri.IndoorItem.Series == "Commercial VRF HP, HNBQ" ||
            //        ri.IndoorItem.Series == "Residential VRF HP, HNRQ" ||
            //        ri.IndoorItem.Series == "Commercial VRF HP, JVOHQ" ||
            //        ri.IndoorItem.Series == "Residential VRF HP, JROHQ")
            //    isWuxiGen2 = true;

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            if (dt.Rows.Count > 0)
            {
                strsql += " and BrandCode='" + BrandCode + "'";
                strsql = AddRegionAndSeriesParameterIntoSql(RegionCode, SubRegionCode, FactoryCode, BrandCode, ri.IndoorItem.Series, ri.IndoorItem, strsql);
                //if (RegionCode.Substring(0, 2) == "EU" || SubRegionCode == "TW")
                //{
                //    strsql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                //}
                //else if ((SubRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20121229 by Yunxiao Lin
                //|| SubRegionCode == "LA_PERU"
                //|| SubRegionCode == "LA_SC")
                //&& ri.IndoorItem.ProductType.Contains("Comm. Tier 4,"))
                //{
                //    strsql += " and RegionCode  like '%/" + SubRegionCode + "/%' and Series like '%;" + ri.IndoorItem.Series + ";%' ";
                //}
                ////增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
                //else if (isWuxiGen2)
                //{
                //    strsql += " and Series like '%;" + ri.IndoorItem.Series + ";%' ";
                //}
                //else
                //{
                //    if (IsSpecial(SubRegionCode, FactoryCode, BrandCode, UnitType, ri.IndoorItem))
                //    {
                //        strsql += " and Series like '%;" + ri.IndoorItem.Series + ";%' ";
                //    }
                //    else {
                //        strsql += " and RegionCode is null and Series is null";
                //    }
                //}

                if (ri.IsExchanger)
                    strsql += " and FactoryCode ='" + FactoryCode + "' and (((UnitType = '" + UnitType + "' or UnitType = 'Indoor Unit')))";
                else
                    //strsql += " and FactoryCode ='" + FactoryCode + "' and (((UnitType = '" + UnitType + "' or UnitType = 'Indoor Unit') and  MinCapacity<=" + Capacity + " and MaxCapacity >=" + Capacity + "))";
                    strsql += "  and (((UnitType = '" + UnitType + "' or UnitType = 'Indoor Unit') and  MinCapacity<=" + Capacity + " and MaxCapacity >=" + Capacity + " and FactoryCode='" + FactoryCode + "') or UnitType = 'Ventilation') ";
                DataView dv = dt.DefaultView;

                if (UnitType == "High Wall" || UnitType == "High Wall (NA)" || UnitType == "High Wall (w/o EXV)" || ri.IsExchanger)
                {
                    strsql += " and Model_Hitachi<>'THM-R2A'";
                }

                if (FactoryCode == "S" && BrandCode == "H" && UnitType == "High Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPI-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    strsql += " and Remark='RPI-FSN3'";
                }


                if ((RegionCode != "MAL" && RegionCode != "LA_BV") || BrandCode == "H" ||  isWuxiGen2(ri.IndoorItem.Series)) //只有马来西亚的YORK品牌才用QE作为Accessory型号的结尾 20161023 by Yunxiao lin
                {
                    // 玻利维亚的York产品线和马来西亚的一样
                    // Access中通配符为'*',SQL中通配符为'%'，但是此处应用%
                    strsql += " and Model_York not like '%QE' and (Remark<>'MAL' or Remark is null) ";
                }
                else
                {
                    strsql += " and (Remark='MAL' or (Model_York <>'DUPI-162Q' and Model_York <>'DUPI-132CQ' and Model_York <>'JP-N23NAQ' ";// sql+= " and Model_York <>'JDUPI-162Q' and Model_York <>'JDUPI-132CQ' and Model_York <>'JP-N23NAQ' ";
                    //新增MAL过滤条件 add by axj 20160623
                    strsql += " and  Model_York <>'JCRA10NEWQ' and  Model_York <>'JCWA10NEWQ' and  Model_York <>'JCWB10NEWS' ";
                    //sql += " and  Model_York <>'JCRB10NEWS' and  Model_York <>'JR4A10NEWQ'  and  Model_York <>'JRDA10NEWQ'))";//and  Model_York <>'CIR01' and Model_York<>'CWDIRK01' and Model_York<>'CIW01' 
                    strsql += " and  Model_York <>'JCRB10NEWS'))";
                }

                if (FactoryCode == "S" && BrandCode == "H" && UnitType == "High Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPI-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    strsql += " and Remark='RPI-FSN3'";
                }
                else if (FactoryCode == "S" && BrandCode == "H" && UnitType == "Medium Static Ducted" && ri.IndoorItem.Model_Hitachi.StartsWith("RPIM-") && ri.IndoorItem.Model_Hitachi.EndsWith("FSN3") && SubRegionCode != "ANZ")
                {
                    strsql += " and Remark='RPIM-FSN3'";
                }
                string Model_Hitachi = ri.IndoorItem.Model_Hitachi;
                //欧洲区域 配件类型需要特殊处理 on 20171213 by xyj
                if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S")
                {
                    if (ri.IndoorItem.Model_Hitachi.Contains('(') && ri.IndoorItem.Model_Hitachi.Contains(')'))
                    {
                        Model_Hitachi = ri.IndoorItem.Model_Hitachi.Substring(0, ri.IndoorItem.Model_Hitachi.IndexOf("(")).Trim();
                    }
                    if (BrandCode == "H" && UnitType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.EndsWith("FSN4E"))
                    {
                        strsql += " and (Remark='RPIM-FSN4E' or Remark IS NULL)";
                    }
                    if (BrandCode == "H" && UnitType == "Mini Ducted" && Model_Hitachi.StartsWith("RPIM-") && Model_Hitachi.Contains("FSN4E-DU"))
                    {
                        strsql += " and (Remark='RPIM-FSN4E-DU' or Remark IS NULL)";
                    }
                    if (BrandCode == "H" && UnitType == "Medium Static Ducted" && Model_Hitachi.StartsWith("RPI-") && Model_Hitachi.Contains("FSN5E"))
                    {
                        if (Model_Hitachi == "RPI-0.4FSN5E")
                        {
                            strsql += " and (Remark='RPI-0.4FSN5E' or Remark IS NULL)";
                        }
                        else
                        {
                            strsql += " and (Remark='RPI-FSN5E' or Remark IS NULL)";
                        }
                    }
                    if (BrandCode == "H" && UnitType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3"))
                    {
                        strsql += " and (Remark='RPC-FSN3' or Remark is null)";
                    }
                    if (BrandCode == "H" && UnitType == "Ceiling Suspended" && Model_Hitachi.StartsWith("RPC-") && Model_Hitachi.EndsWith("FSN3E"))
                    {
                        strsql += " and (Remark='RPC-FSN3E' or Remark is null)";
                    }
                    if (BrandCode == "H" && UnitType.Contains("Total Heat Exchanger") && Model_Hitachi.StartsWith("KPI-"))
                    {
                        if ((Model_Hitachi.Substring(0, Model_Hitachi.Length - 3).Length > 0))
                        {
                            if (Model_Hitachi == "KPI-1502E4E" || Model_Hitachi == "KPI-2002E4E")
                                strsql += " and (Remark LIKE '%" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "%' or Remark is null)";
                            else
                                strsql += " and (Remark='" + Model_Hitachi.Substring(0, Model_Hitachi.Length - 3) + "' or Remark is null)";
                        }
                    }
                }
                //新增台湾的数据处理 //S和T工厂  on 20180801 by xyj  
                if (!(Project.CurrentProject.RegionCode == "EU_W" ||
                    Project.CurrentProject.RegionCode == "EU_E" ||
                    Project.CurrentProject.RegionCode == "EU_S" || Project.CurrentProject.SubRegionCode == "TW"))
                    strsql += GetSpecialData_SQL(SubRegionCode, FactoryCode, BrandCode, UnitType, ri.IndoorItem);
                dv.RowFilter = strsql;
                dv.Sort = "Type asc";
                return dv.ToTable();
            }

            return dt;
        }



        /// <summary>
        /// 获取室内机配件 根据UnitType Type 筛选配件
        /// </summary>
        /// <param name="BrandCode"></param>
        /// <param name="FactoryCode"></param>
        /// <param name="UnitType"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public DataTable GetShareAccessoryItemList(string BrandCode, string FactoryCode, string UnitType, string Type, double Capacity, string RegionCode, string SubRegionCode)
        {

            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Accessory");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                string sql = " 1=1 ";
                if (RegionCode == "EU_W" || RegionCode == "EU_E" || RegionCode == "EU_S" || SubRegionCode == "TW")
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                }
                else if ((SubRegionCode == "ASIA" && BrandCode == "Y") || SubRegionCode == "BGD" || SubRegionCode == "SEA_60Hz") // ASIA York, BGD, SEA_60Hz，需要判断
                {
                    sql += " and RegionCode  like '%/" + SubRegionCode + "/%' ";
                }
                else
                    sql += " and RegionCode is  null ";
                string sqls = " and Type='" + Type + "' ";
                if (Type == "Remote Control Switch")
                    sqls = " and Type in('" + Type + "','Half-size Remote Control Switch') ";

                if (UnitType == "Total Heat Exchanger (KPI-E4E)")
                    sqls = sql + " and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and UnitType = '" + UnitType + "' ";
                else
                    sqls = sql + " and BrandCode = '" + BrandCode + "' and FactoryCode ='" + FactoryCode + "' and UnitType = '" + UnitType + "'  and  MinCapacity<=" + Capacity + " and MaxCapacity >=" + Capacity + "";
                dv.RowFilter = sqls;
                dt = dv.ToTable();
            }
            return dt;
        }


        /// <summary>
        /// 获取Accessory 配件对应的DisplayType 类型
        /// </summary>
        /// <returns></returns>
        public DataTable GetUniversal_ByAccessory(string SubRegionCode, string BrandCode)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Universal_Display");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataView dv = dt.DefaultView;
                string sql = " 1=1 and DeleteFlag=1 and SelectionType='Accessory' and BrandCode = '" + BrandCode + "' and RegionCode like '%/" + SubRegionCode + "/%'";

                dv.RowFilter = sql;
                dt = dv.ToTable();
            }
            return dt;
        }

        public string AddRegionAndSeriesParameterIntoSql(string regionCode, string subRegionCode, string factoryCode, string brandCode, string series, Indoor inItem, string sql)
        {
            if (regionCode == "EU_NERP" && series != "")
            {
                sql += " and RegionCode  like '%/" + subRegionCode + "/%' and Series like '%" + series + "%' ";
            }
            else if (regionCode.Substring(0, 2) == "EU")
            {
                sql += " and RegionCode  like '%/" + subRegionCode + "/%' ";
            }
            else if (subRegionCode == "LA_MMA" //LA_MMA,LA_PERU,LA_SC Gen.2 系列Accessory需要特殊判断20171229 by Yunxiao Lin
                || subRegionCode == "LA_PERU"
                || subRegionCode == "LA_SC")
            {
                if (series == "")
                {
                    sql += " and RegionCode  like '%/" + subRegionCode + "/%' and (Series like '%;Commercial VRF HP, FSNS;%' or Series like '%;Commercial VRF HP, JTOH-BS1;%' or Series like '%;Commercial VRF HR, FSXNS;%' or Series like '%;Commercial VRF HR, JTOR-BS1;%') ";
                }
                else if (series == "Commercial VRF HP, JTOH-BS1"
                || series == "Commercial VRF HR, JTOR-BS1"
                || series == "Commercial VRF HP, FSNS"
                || series == "Commercial VRF HR, FSXNS")
                {
                    sql += " and RegionCode  like '%/" + subRegionCode + "/%' and Series like '%;" + series + ";%' ";
                }
            }
            else if (subRegionCode == "ANZ") //ANZ的Accessory需要特殊判断 20180414 by Yunxiao Lin
            {
                sql += " and RegionCode  like '%/" + subRegionCode + "/%' ";
                if (series != "")
                {
                    sql += " and (Series like '%" + series + "%' or series is null)";
                }
            }
            else if ((subRegionCode == "ASIA" && brandCode == "Y") || subRegionCode == "BGD" || subRegionCode == "SEA_60Hz") // ASIA York, BGD, SEA_60Hz，需要判断
            {
                sql += " and RegionCode  like '%/" + subRegionCode + "/%' ";
            }
            //增加Wuxi design配件选型特殊判断 20180627 by Yunxiao Lin
            else if (series != "" && isWuxiGen2(series))
            {
                sql += " and (RegionCode is null or RegionCode like '%/"+ subRegionCode + "/%') and Series like '%" + series + "%'";
                if (series == "Residential VRF HP, HNRQ")
                {
                    sql += " and (RegionCode not like '%/LA_SC/%' or Series not like '%" + series + "%' or RegionCode is null) ";
                }
            }
            else if (subRegionCode == "TW") //TW的Accessory需要特殊判断 20180727 by Yunxiao Lin
            {
                sql += " and RegionCode  like '%/" + subRegionCode + "/%' ";
            }
            else
            {
               if (series == "")
               {
                    if (IsSpecial(subRegionCode, factoryCode, brandCode, inItem.Type, inItem))
                    {
                        sql += " and (RegionCode is null or RegionCode like '%" + subRegionCode + "%') and Series like '%" + series + "%' ";
                    }
                    else
                    {
                        sql += " and RegionCode is null and Series is null";
                    }
               }
               else
               {
                   sql += " and RegionCode is null and Series is null";
               }
            }
            return sql;
        }

        public bool isWuxiGen2(string series)
        {
            if (series == "Commercial VRF HP, HNCQ" ||
                series == "Commercial VRF HP, HNBQ" ||
                series == "Residential VRF HP, HNRQ" ||
                series == "Commercial VRF HP, JVOHQ" ||
                series == "Residential VRF HP, JROHQ")
                return true;
            return false;
        }

    }

    /// <summary>
    /// 共有Accessory的厂家和UnitType记录集，用于公共Accessory筛选判断 20171115 by Yunxiao Lin
    /// </summary>
    public class CommonAccessory
    {
        string _model_Hitachi;
        List<CommonAccessoryUnitType> _unitTypeList;
        public CommonAccessory(string Model_H)
        {
            this._model_Hitachi = Model_H;
            _unitTypeList = new List<CommonAccessoryUnitType>();
        }

        public string Model_Hitachi
        {
            get { return _model_Hitachi; }
        }

        public List<CommonAccessoryUnitType> UnitTypeList
        {
            get { return _unitTypeList; }
        }
    }

    /// <summary>
    /// 共有Accessory的厂家和UnitType记录，用于公共Accessory判断 20171115 by Yunxiao Lin
    /// </summary>
    public class CommonAccessoryUnitType
    {
        string _factoryCode;
        string _unitType;
        bool _exists;

        public CommonAccessoryUnitType(string fCode, string uType)
        {
            _factoryCode = fCode;
            _unitType = uType;
            _exists = false;
        }

        public string FactoryCode
        {
            get { return _factoryCode; }
            set { _factoryCode = value; }
        }

        public string UnitType
        {
            get { return _unitType; }
            set { _unitType = value; }
        }

        public bool Exists
        {
            get { return _exists; }
            set { _exists = value; }
        }
    }
}
