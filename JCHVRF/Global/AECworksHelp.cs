

namespace JCHVRF
{
    public class AECworksHelp
    {
        public static string[] regionAEC = { "China", "MiddleEast", "Asia", "HongKong", "Singapore", "Thailand", "India", "Australia", "Europe", "LatinAmerica", "Other", "NorthAmerica" };
        public static string[] regionVRF = { "China", "Middle East", "Asia", "Asia", "Asia", "Asia", "Asia", "None","None", "None", "None", "North America" };

        /// <summary>
        /// 根据 AECworks 中传入的 Region 值返回对应的 RegionVRF 值
        /// </summary>
        /// <param name="regAEC"> AECworks 中传入的 Region 值 </param>
        public static string getRegionVRF(string regAEC)
        {
            for(int i=0;i<regionAEC.Length;++i)
            {
                if (regAEC == regionAEC[i])
                    return regionVRF[i];
            }
            return "";
        }

        /// <summary>
        /// 根据 VRF 程序中传入的 Region 值返回对应的 RegionAEC 值
        /// </summary>
        /// <param name="regionVRF">保存到AECWorkd中的Region值</param>
        /// <returns></returns>
        public static string getRegionAEC(string regVRF)
        {
            for (int i = 0; i < regionVRF.Length; ++i)
            {
                if (regVRF == regionVRF[i])
                    return regionAEC[i];
            }
            return "";
        }
    }
}
