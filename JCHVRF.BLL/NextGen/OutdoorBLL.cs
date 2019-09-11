using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.DAL;
using NextGenModel = JCHVRF.Model.NextGen;
using NextGenDAL = JCHVRF.DAL.NextGen;

namespace JCHVRF.BLL.NextGen
{
    public class OutdoorBLL
    {
        string _region;
        //string _productType; //多ProductType系统中不只有一个ProductType，所以该变量没用了
        string _brandCode;
        NextGenDAL.OutdoorDAL _dal;

        //public OutdoorBLL(string region, string productType, string brandCode)
        public OutdoorBLL(string region, string brandCode)
        {
            _region = region;
            //_productType = productType;
            _brandCode = brandCode;
            //_dal = new OutdoorDAL(region, productType, _brandCode);
            _dal = new NextGenDAL.OutdoorDAL(region, _brandCode);
        }

        public OutdoorBLL(string region, string brandCode, string mainRegion)
        {
            _region = region;
            //_productType = productType;
            _brandCode = brandCode;
            //_dal = new OutdoorDAL(region, productType, _brandCode);
            _dal = new NextGenDAL.OutdoorDAL(region, _brandCode, mainRegion);
        }
        // 由于多ProductType功能，需要增加productType参数
        public Outdoor GetOutdoorItem(string modelFull, string productType)
        {
            return _dal.GetItem(modelFull, productType);
        }

        // 增加Series获取室内机
        public Outdoor GetOutdoorItemBySeries(string modelFull, string series)
        {
            return _dal.GetItemBySeries(modelFull, series);
        }

        /// 根据Factory + Region + Brand + ProductType + Series + Hitachi_Model找到相同的新型号 on 2017/06/27 by Shen Junjie
        /// <summary>
        /// 根据Factory + Region + Brand + ProductType + Series + Hitachi_Model找到相同的新型号
        /// </summary>
        /// <param name="modelFull"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        public Outdoor GetOutdoorItem(string productType, string series, string modelHitachi)
        {
            return _dal.GetItem(productType, series, modelHitachi);
        }

        /// <summary>
        /// 获取室外机标准表
        /// </summary>
        /// <returns></returns>
        public DataTable GetOutdoorListStd()
        {
            return _dal.GetOutdoorListStd();
        }
        // 20160822 因为多productType的需求，增加productType参数 by Yunxiao Lin
        /// <summary>
        /// 获取室外机类型list
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable GetOutdoorTypeList(out string colName, string productType)
        {
            //return _dal.GetOutdoorTypeList(out colName);
            return _dal.GetOutdoorTypeList(out colName, productType);
        }

        // 20161027 productType改为series做条件
        /// <summary>
        /// 获取室外机类型list
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable GetOutdoorTypeListBySeries(out string colName, string series)
        {
            //return _dal.GetOutdoorTypeList(out colName);
            return _dal.GetOutdoorTypeListBySeries(out colName, series);
        }

        /// <summary>
        /// 计算室外机估算容量值，查容量表计算
        /// 增加SystemVRF参数，可以处理水机流速 20170216 by Yunxiao Lin
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shortModel"></param>
        /// <param name="maxRatio"></param>
        /// <param name="OutTemperature"></param>
        /// <param name="InTemperature"></param>
        /// <param name="isHeating"></param>
        /// <returns></returns>
        public double CalOutdoorEstCapacity(Outdoor outItem, double maxRatio, double OutTemperature, double InTemperature, bool isHeating, NextGenModel.SystemVRF sysItem)
        {
            return _dal.CalOutdoorEstCapacityNextGen(outItem, maxRatio, OutTemperature, InTemperature, isHeating, sysItem);
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
            //return _dal.GetHitachiItem(Model_Hitachi);
            return _dal.GetHitachiItem(Model_Hitachi, productType);
        }

        /// 根据Series获取Model_Hitachi室外机对象 add on 20161027        
        /// <summary>
        /// 根据Series获取Model_Hitachi室外机对象
        /// </summary>
        /// <param name="Model_Hitachi"></param>
        /// <returns></returns>
        public Outdoor GetHitachiItemBySeries(string Model_Hitachi, string series)
        {
            return _dal.GetHitachiItemBySeries(Model_Hitachi, series);
        }

        /// 获得室外机的Total IDU Correction Factor add on 20161110 by Yunxiao Lin
        /// <summary>
        /// 获得室外机的Total IDU Correction Factor
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Horsepower"></param>
        /// <param name="isHeating"></param>
        /// <returns></returns>
        public double GetTotalIDUFactor(string Model, double Horsepower, bool isHeating)
        {
            return _dal.GetTotalIDUFactor(Model, Horsepower, isHeating);
        }
        /// 从数据库中获取室外机海拔修正系数 20180626 by Yunxiao Lin
        /// <summary>
        /// 从数据库中获取室外机海拔修正系数
        /// </summary>
        /// <param name="altitude">海拔高度，单位暂时为m，以后需要支持ft</param>
        /// <param name="series">室外机系列</param>
        /// <returns></returns>
        public double GetODUAltitudeFactor(int altitude, string series)
        {
            string type = "Other";
            if (series.EndsWith("YVAHP") || series.EndsWith("YVAHR") || series == "Residential VRF HP, HNSKQ")
                type = "NA";
            return _dal.GetODUAltitudeFactor(altitude, type);
        }
        /// 从数据库中获取室外机除霜修正系数 20180626 by Yunxiao Lin
        /// <summary>
        /// 从数据库中获取室外机除霜修正系数
        /// </summary>
        /// <param name="outDB">室外干球温度，单位暂时为摄氏度，以后需要支持华氏度</param>
        /// <param name="series">室外机系列</param>
        /// <returns></returns>
        public double GetODUDefrostFactor(double outDB, string series)
        {
            string type = "Other";
            if (series.EndsWith("YVAHP") || series.EndsWith("YVAHR"))
                type = "NA";
            return _dal.GetODUDefrostFactor(outDB, type);
        }

        // 通过Serie和unittype获取室外机power list add by axj 20170122
        /// <summary>
        /// 通过Serie和unittype获取室外机power list
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public DataTable GetOutdoorPowerListBySeriesAndType(string series, string Type)
        {
            return _dal.GetOutdoorPowerListBySeriesAndType(series, Type);
        }
        public dynamic GetOutdoorCombinedDetail(string modelFull, string series)
        {
            dynamic Combinedoutdoordt = new System.Dynamic.ExpandoObject();
            dynamic returnCombinedoutdoordt = new System.Dynamic.ExpandoObject();

            if (modelFull.Contains("+"))
            {
                string[] str = modelFull.Split('+');

                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j].Trim().Contains("*"))
                    {
                        //------------------
                        string[] str2 = str[j].Trim().Split('*');
                        Combinedoutdoordt = _dal.GetODUCombined(str2[0].Trim(), series);
                        if (Combinedoutdoordt != null)
                        {
                            int i = Convert.ToInt32(str2[1].Trim());
                            for (int k = 0; k < i; k++)
                            {
                                if (j == 0 && k == 0)
                                {
                                    returnCombinedoutdoordt.Weight = Combinedoutdoordt.Weight;
                                    returnCombinedoutdoordt.Width = Combinedoutdoordt.Width;
                                    returnCombinedoutdoordt.Maxcurrent = Combinedoutdoordt.Maxcurrent;
                                }
                                else
                                {
                                    returnCombinedoutdoordt.Weight = returnCombinedoutdoordt.Weight + "+" + Combinedoutdoordt.Weight;
                                    returnCombinedoutdoordt.Width = returnCombinedoutdoordt.Width + "+" + Combinedoutdoordt.Width;
                                    returnCombinedoutdoordt.Maxcurrent = returnCombinedoutdoordt.Maxcurrent + "+" + Combinedoutdoordt.Maxcurrent;
                                }
                            }
                        }
                        //-----------------
                    }
                    else
                    {
                        Combinedoutdoordt = _dal.GetODUCombined(str[j].Trim(), series);
                        if (Combinedoutdoordt != null)
                        {
                            if (j == 0)
                            {
                                returnCombinedoutdoordt.Weight = Combinedoutdoordt.Weight;
                                returnCombinedoutdoordt.Width = Combinedoutdoordt.Width;
                                returnCombinedoutdoordt.Maxcurrent = Combinedoutdoordt.Maxcurrent;
                            }
                            else
                            {
                                returnCombinedoutdoordt.Weight = returnCombinedoutdoordt.Weight + "+" + Combinedoutdoordt.Weight;
                                returnCombinedoutdoordt.Width = returnCombinedoutdoordt.Width + "+" + Combinedoutdoordt.Width;
                                returnCombinedoutdoordt.Maxcurrent = returnCombinedoutdoordt.Maxcurrent + "+" + Combinedoutdoordt.Maxcurrent;
                            }
                        }
                    }
                }
            }
            else if (modelFull.Contains("*"))
            {
                string[] str = modelFull.Split('*');
                Combinedoutdoordt = _dal.GetODUCombined(str[0].Trim(), series);
                if (Combinedoutdoordt != null)
                {
                    int i = Convert.ToInt32(str[1].Trim());
                    for (int j = 0; j < i; j++)
                    {
                        if (j == 0)
                        {
                            returnCombinedoutdoordt.Weight = Combinedoutdoordt.Weight;
                            returnCombinedoutdoordt.Width = Combinedoutdoordt.Width;
                            returnCombinedoutdoordt.Maxcurrent = Combinedoutdoordt.Maxcurrent;
                        }
                        else
                        {
                            returnCombinedoutdoordt.Weight = returnCombinedoutdoordt.Weight + "+" + Combinedoutdoordt.Weight;
                            returnCombinedoutdoordt.Width = returnCombinedoutdoordt.Width + "+" + Combinedoutdoordt.Width;
                            returnCombinedoutdoordt.Maxcurrent = returnCombinedoutdoordt.Maxcurrent + "+" + Combinedoutdoordt.Maxcurrent;
                        }
                    }
                }

            }
            return returnCombinedoutdoordt;
        }


    }
}
