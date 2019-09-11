/****************************** File Header ******************************\
File Name:	Enums.cs
Date Created:	2/1/2019
Description:	Will contain all enums in application.
\*************************************************************************/

namespace JCHVRF_New.Model
{
    #region Enums

    public enum CapacityUnit
    {
        kw,
        ton,
        btu
    }

    public enum AirflowUnit
    {
        ls,
        m3h,
        m3hr,
        cfm
    }

    public enum TemperatureUnit
    {
        F,
        C
    }

    public enum LengthUnit
    {
        m,
        ft
    }

    public enum DimensionsUnit
    {
        mm,
        inch
    }

    public enum WeightUnit
    {
        kg,
        lbs
    }

    public enum AreaUnit
    {
        m2,
        ft2
    }

    public enum WaterFlowRateUnit
    {
        m3h,
        lmin
    }

    public enum LoadIndexUnit
    {
        Wm2,
        MBH
    }

    public enum FanSpeed
    {
        Max = -1,
        High2 = 0,
        High =1,
        Medium=2,
        Low=3
    }
    public enum ESP
    {
        Pa,
        InWG
    }

    #endregion
}
