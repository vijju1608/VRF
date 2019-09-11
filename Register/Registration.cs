using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using JCHVRF.Model;
using JCHVRF.BLL;

namespace Registr
{
    public class Registration
    {

        public static bool IsValidDate()
        {
            CDL.SVRFreg sv = CDL.Sec.GetSVRF();
            // 验证注册日期(在Region验证之后)
            DateTime dtExpire = new DateTime(2020, 9, 10); //V3.4.2a
            //DateTime dtExpire = new DateTime(2019, 3, 1); //V3.4.1
            return CDL.Sec.ValidateVRF(dtExpire, sv);
        }

        public static bool IsValid()
        {
            bool isValid = (CDL.Sec.GetValidflag() == 1);
            return isValid && IsValidDate();
        }

        public static bool IsSuperUser()
        {
            CDL.SVRFreg sv = CDL.Sec.GetSVRF();
            if (sv != null && sv.SuperUser)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 注销，测试用
        /// </summary>
        public static void LogOff()
        {
            CDL.SVRFreg sv = CDL.Sec.GetSVRF();
            sv.Validflag = 0;
            sv.SuperUser = false;
            CDL.Sec.UpdateValidDate(sv);
        }

        public static void DoValidation(string psw)
        {
            if (!string.IsNullOrEmpty(psw))
            {
                CDL.SVRFreg sv = new CDL.SVRFreg();
                DateTime dt = System.DateTime.Now;
                sv.Dealer = false;
                sv.LeftDay = 360;
                sv.PriceValid = false;
                sv.RegDate = dt;
                sv.SuperUser = false;
                sv.Validflag = 0;

                List<MyRegion> list = (new RegionBLL()).GetParentRegionList();
                if (list == null)
                    return;

                foreach (MyRegion item in list)
                {
                    sv.Region = item.Code;
                    if (psw == item.RegistPassword)
                    {
                        sv.Validflag = 1;
                        if (sv.Region == "Super")
                        {
                            sv.SuperUser = true;
                            sv.PriceValid = true;
                            sv.BrandCode = "ALL";
                        }

                        CDL.Sec.UpdateValidDate(sv);
                        return;
                    }
                    else if (psw == item.YorkPassword)
                    {
                        sv.Validflag = 1;
                        sv.BrandCode = "Y";
                        CDL.Sec.UpdateValidDate(sv);
                        return;
                    }
                    else if (psw == item.HitachiPassword)
                    {
                        sv.Validflag = 1;
                        sv.BrandCode = "H";
                        CDL.Sec.UpdateValidDate(sv);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 完成密码验证之后，可以获取用户所在区域
        /// </summary>
        /// <returns></returns>
        public static string GetRegionCode()
        {
            CDL.SVRFreg sv = CDL.Sec.GetSVRF();
            if (sv != null && !sv.SuperUser)
            {
                return sv.Region;
            }
            return "";
        }

        public static MyDictionary SelectedBrand { get; set; }

        /// <summary>
        /// “G”or其他，此处并非准确的工厂代码，要注意！
        /// </summary>
        public static string SelectedFactoryCode { get; set; }

        public static MyRegion SelectedSubRegion { get; set; }

        /// <summary>
        /// 完成密码验证之后，可以获取用户品牌权限
        /// </summary>
        /// <returns></returns>
        public static string GetBrandCode()
        {
            CDL.SVRFreg sv = CDL.Sec.GetSVRF();
            if (sv != null)
            {
                return sv.BrandCode;
            }
            return "";
        }

    }
}
