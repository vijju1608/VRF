using System;
using System.Collections.Generic;
using JCBase.UI;
using JCHVRF.Model;
using JCBase.Utility;

namespace JCHVRF.VRFMessage
{
    /// <summary>
    /// 弹出框消息统一管理，根据当前语言环境切换中英文内容
    /// </summary>
    public class Msg
    {

        #region 获取资源文件
        public static string GetResourceString(string messageID)
        {
            string strCurLanguage = "";
            try
            {
                if (LangType.CurrentLanguage == LangType.CHINESE || LangType.CurrentLanguage == LangType.CHINESE_SIMPLE)
                {
                    messageID = messageID + "_ZH";
                }
                else if (LangType.CurrentLanguage == LangType.CHINESE_TRADITIONAL)
                {
                    messageID = messageID + "_ZHT";
                }
                else if (LangType.CurrentLanguage == LangType.ENGLISH)
                {
                    messageID = messageID + "";
                }
                else if (LangType.CurrentLanguage == LangType.FRENCH)
                {
                    messageID = messageID + "_FR";
                }
                else if (LangType.CurrentLanguage == LangType.TURKISH)
                {
                    messageID = messageID + "_TK";
                }
                else if (LangType.CurrentLanguage == LangType.SPANISH)
                {
                    messageID = messageID + "_SP";
                }
                else if (LangType.CurrentLanguage == LangType.GERMANY)
                {
                    messageID = messageID + "_DE";
                }
                else if (LangType.CurrentLanguage == LangType.ITALIAN)
                {
                    messageID = messageID + "_IT";
                }
                else if (LangType.CurrentLanguage == LangType.BRAZILIAN_PORTUGUESS)
                {
                    messageID = messageID + "_PT_BR";
                }
                else
                {
                    messageID = messageID + "";
                }
                strCurLanguage = Message.ResourceManager.GetString(messageID);
            }
            catch
            {
                strCurLanguage = null;
            }
            if (strCurLanguage == null)
            {
                if (LangType.CurrentLanguage == LangType.CHINESE || LangType.CurrentLanguage == LangType.CHINESE_SIMPLE)
                {
                    strCurLanguage = "资源文件不存在:" + messageID + ", 请增加.";
                }
                else
                {
                    strCurLanguage = "No Resource:" + messageID + ", please add.";
                }
            }
            return strCurLanguage;
        }
        #endregion 

        #region 一般信息
        /// 无数据记录！
        /// <summary>
        /// 无数据记录！
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string DB_NODATA
        {
            get
            {
                return GetResourceString("DB_NODATA");
            }
        }

        /// 找不到对应的容量表！
        /// <summary>
        /// 找不到对应的容量表！
        /// </summary>
        public static string DB_NOTABLE_CAP
        {
            get
            {
                return GetResourceString("DB_NOTABLE_CAP");
            }

        }

        /// 确定删除 [{0}] ？
        /// <summary>
        /// 确定删除 [{0}] ？
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CONFIRM_DELETE(string name)
        {
            return string.Format(GetResourceString("CONFIRM_DELETE"),name);
        }

        /// 确认：切换 Region 选项时的提示
        /// <summary>
        /// 确认：切换 Region 选项时的提示
        /// </summary>
        public static string CONFIRM_CHANGEREGION
        {
            get
            {
                return GetResourceString("CONFIRM_CHANGEREGION");
            }

        }

        /// 确认：提示是否保存项目
        /// <summary>
        /// 确认：提示是否保存项目
        /// </summary>
        public static string CONFIRM_SAVEPROJ
        {
            get
            {
                return GetResourceString("CONFIRM_SAVEPROJ");
            }
        }

        /// 确认：您想保存这个项目吗？ 如果您选择“否”，未保存的更改将不会被导出。
        /// <summary>
        /// 确认：您想保存这个项目吗？ 如果您选择“否”，未保存的更改将不会被导出。
        /// </summary>
        public static string CONFIRM_SAVEPROJ_EXPORT
        {
            get
            {
                return GetResourceString("CONFIRM_SAVEPROJ_EXPORT");
            }
        }

        /// 提示：项目导入已被取消。
        /// <summary>
        /// 提示：项目导入已被取消。
        /// </summary>
        public static string IMPORT_PROJECT_FAILED
        {
            get
            {
                return GetResourceString("IMPORT_PROJECT_FAILED");
            }
        }

        /// <summary>
        /// 确认：是否打开报告文件
        /// </summary>
        public static string CONFIRM_REPORT_OPEN
        {
            get
            {
                return GetResourceString("CONFIRM_REPORT_OPEN");
            }
        }

        /// 当前程序已运行！
        /// <summary>
        /// 当前程序已运行！
        /// </summary>
        public static string WARNING_PROCESS_RUNNING
        {
            get
            {

                return GetResourceString("WARNING_PROCESS_RUNNING");
            }
        }

        /// 超出工况范围！
        /// <summary>
        /// 超出工况范围！
        /// </summary>
        public static string WARNING_DATA_EXCEED
        {
            get
            {
                return GetResourceString("WARNING_DATA_EXCEED");
            }
        }

        /// 找不到对应的容量表！
        /// <summary>
        /// 找不到对应的容量表！
        /// </summary>
        public static string WARNING_DATA_NOTABLE_CAP
        {
            get
            {
                return GetResourceString("WARNING_DATA_NOTABLE_CAP");
            }
        }

        /// {0}已存在重复项！
        /// <summary>
        /// {0}已存在重复项！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string WARNING_REPEATED(string name)
        {
            return string.Format(GetResourceString("WARNING_REPEATED"), name);
        }

        /// 请关注提示信息
        /// <summary>
        /// 请关注提示信息
        /// </summary>
        public static string WARNING_PAYATTENTION
        {
            get
            {
                return GetResourceString("WARNING_PAYATTENTION");
            }
        }


        /// 共享控制器
        /// <summary>
        /// 共享控制器
        /// </summary>
        public static string SHARE_REMOTECONTROL
        {
            get
            {
                return GetResourceString("SHARE_REMOTECONTROL");
            }
        }

        /// 不能分配不同productType的室内机
        /// <summary>
        /// 请关注提示信息
        /// </summary>
        public static string WARNING_ROOM_PRODUCTTYPE
        {
            get
            {
                return GetResourceString("WARNING_ROOM_PRODUCTTYPE");
            }
        }

        /// 不能分配不同productType的全热交换机
        /// <summary>
        /// 请关注提示信息
        /// </summary>
        public static string WARNING_EXC_PRODUCTTYPE
        {
            get
            {
                return GetResourceString("WARNING_EXC_PRODUCTTYPE");
            }
        }


        // 不能分配不同productType的室内机
        /// <summary>
        /// 请关注提示信息
        /// </summary>
        public static string WARNING_CONTROLLER_UNDONE(string type)
        {           
            return string.Format(GetResourceString("WARNING_CONTROLLER_UNDONE"), type);
        }


        /// 当前文件版本与应用程序版本不匹配！文件版本号为"{0}"，程序版本号为"{1}"，请检查！
        /// <summary>
        /// 当前文件版本与应用程序版本不匹配！文件版本号为"{0}"，程序版本号为"{1}"，请检查！
        /// </summary>
        /// <param name="fileVersion"></param>
        /// <returns></returns>
        public static string ERR_CONTROLLER_PRODUCTTYPE_COMPATIBILITY(string systemName)
        {
            string prjVersion = MyConfig.Version;
            return string.Format(GetResourceString("ERR_CONTROLLER_PRODUCTTYPE_COMPATIBILITY"), systemName);
        }

        /// 当前文件版本与应用程序版本不匹配！文件版本号为"{0}"，程序版本号为"{1}"，请检查！
        /// <summary>
        /// 当前文件版本与应用程序版本不匹配！文件版本号为"{0}"，程序版本号为"{1}"，请检查！
        /// </summary>
        /// <param name="fileVersion"></param>
        /// <returns></returns>
        public static string ERR_VERSION_NOTMATCH(string fileVersion)
        {
            string prjVersion = MyConfig.Version;
            return string.Format(GetResourceString("ERR_VERSION_NOTMATCH"), prjVersion);
        }

        /// 符合 | OK
        /// <summary>
        /// 符合 | OK
        /// </summary>
        public static string VERIFICATION_OK
        {
            get
            {
                return GetResourceString("VERIFICATION_OK");
            }
        }


        /// 按住Ctrl键可实现多选，批量设置{0}！
        /// <summary>
        /// 按住Ctrl键可实现多选，批量设置{0}！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string INFO_MULTI_SETTING(string name)
        {
            return string.Format(GetResourceString("INFO_MULTI_SETTING"), name);
        }


        /// 文件 [{0}] 已存在！是否覆盖？
        /// <summary>
        /// 文件 [{0}] 已存在！是否覆盖？
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        public static string FILE_CONFIRM_OVERWRITE(string fname)
        {
            return string.Format(GetResourceString("FILE_CONFIRM_OVERWRITE"), fname);
        }

        /// 文件已存在！
        /// <summary>
        /// 文件已存在！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FILE_WARN_EXIST()
        {
            return GetResourceString("FILE_WARN_EXIST");
        }

        /// 文件 [ {0} ] 已存在！
        /// <summary>
        /// 文件 [ {0} ] 已存在！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FILE_WARN_EXIST1(string name)
        {
            return string.Format(GetResourceString("FILE_WARN_EXIST1"), name);
        }

        /// 文件不存在！
        /// <summary>
        /// 文件不存在！
        /// </summary>
        /// <returns></returns>
        public static string FILE_WARN_NOTEXIST()
        {
            return GetResourceString("FILE_WARN_NOTEXIST");
        }

        /// 文件 [{0}] 不存在！
        /// <summary>
        /// 文件 [{0}] 不存在！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FILE_WARN_NOTEXIST1(string name)
        {
            return string.Format(GetResourceString("FILE_WARN_NOTEXIST1"), name);
        }

        /// 导入文件格式错误！
        /// <summary>
        /// 导入文件格式错误！
        /// </summary>
        public static string FILE_WARN_IMPORTFORMAT
        {
            get
            {
                return GetResourceString("FILE_WARN_IMPORTFORMAT");
            }
        }

        /// 请先选择{0}文件路径！
        /// <summary>
        /// 请先选择{0}文件路径！
        /// </summary>
        public static string FILE_WARN_SELECTPATH(string name)
        {
            return string.Format(GetResourceString("FILE_WARN_SELECTPATH"), name);
        }

        /// 报告模板文件不存在
        /// <summary>
        /// 报告模板文件不存在
        /// </summary>
        public static string FILE_WARN_NOTEXIST_RPTTEMPLATE
        {
            get
            {
                return GetResourceString("FILE_WARN_NOTEXIST_RPTTEMPLATE");
            }
        }

        /// <summary>
        /// 文件已打开
        /// </summary>
        public static string FILE_WARN_OPENED
        {
            get
            {
                return GetResourceString("FILE_WARN_OPENED");
            }
        }

        /// 配置信息导入成功
        /// <summary>
        /// 配置信息导入成功
        /// </summary>
        public static string SETTING_IMPORT_SUCCESS
        {
            get
            {
                return GetResourceString("SETTING_IMPORT_SUCCESS");
            }
        }

        /// 配置信息导出成功
        /// <summary>
        /// 配置信息导出成功
        /// </summary>
        public static string SETTING_EXPORT_SUCCESS
        {
            get
            {
                return GetResourceString("SETTING_EXPORT_SUCCESS");
            }
        }

        /// 当前室内机数超过室外机最大连接数
        /// <summary>
        /// 当前室内机数超过室外机最大连接数
        /// </summary>
        public static string MSGLIST_MAX_IDU_NUMBER
        {
            get
            {
                return GetResourceString("MSGLIST_MAX_IDU_NUMBER");
            }
        }

        /// 所选的IVX机型不可以进行组合
        /// <summary>
        /// 所选的IVX机型不可以进行组合
        /// </summary>
        public static string MSGLIST_IVX_COMBINATION
        {
            get
            {
                return GetResourceString("MSGLIST_IVX_COMBINATION");
            }
        }

        /// 所选的不是IVX机型
        /// <summary>
        /// 所选的不是IVX机型
        /// </summary>
        public static string MSGLIST_IVX_INVALID
        {
            get
            {
                return GetResourceString("MSGLIST_IVX_INVALID");
            }
        }

        /// RAS-3HRNM1Q 不能与4-way IDU搭配使用
        /// <summary>
        /// RAS-3HRNM1Q 不能与4-way IDU搭配使用
        /// </summary>
        public static string MSGLIST_3HRNM1Q_INFO1
        {
            get
            {
                return GetResourceString("MSGLIST_3HRNM1Q_INFO1");
            }
        }

        /// [型号] 当前配比率低于下限[值]
        /// <summary>
        /// [型号] 当前配比率低于下限[值]
        /// </summary>
        public static string MSGLIST_GENERAL_INFO1(string condition,string value)
        {
            return String.Format(GetResourceString("MSGLIST_GENERAL_INFO1"), condition,value);
        }

        /// [型号]当前配比率超过上限[值]
        /// <summary>
        /// [型号]当前配比率超过上限[值]
        /// </summary>
        public static string MSGLIST_GENERAL_INFO2(string condition,string value)
        {
            return String.Format(GetResourceString("MSGLIST_GENERAL_INFO2"), condition,value);
        }

        /// [型号]下室内机最大匹数超过上限[值] connType:1-for-1 or 1-for-n
        /// <summary>
        /// [型号]下室内机最大匹数超过上限[值] connType:1-for-1 or 1-for-n
        /// </summary>
        public static string MSGLIST_GENERAL_INFO3(string condition,string value,string connType)
        {
            return String.Format(GetResourceString("MSGLIST_GENERAL_INFO3"), condition, value, connType);
        }

        /// [型号]室内机数量超过限制[值]
        /// <summary>
        /// [型号]室内机数量超过限制[值]
        /// </summary>
        public static string MSGLIST_GENERAL_INFO4(string condition, int value)
        {
           return String.Format(GetResourceString("MSGLIST_GENERAL_INFO4"), condition,value);
        }

        /// 混连模式新风制冷容量超过上限30%
        /// <summary>
        /// 混连模式新风制冷容量超过上限30%
        /// </summary>
        public static string MSGLIST_COMPOSITE_MODE
        {
            get
            {
                return GetResourceString("MSGLIST_COMPOSITE_MODE");
            }
        }

        /// 配比率是不允许超出100%!
        /// <summary>
        /// 配比率是不允许超出100%!
        /// </summary>
        public static string MSGLIST_NOT_ALLOW_EXCEED_RATIO
        {
            get
            {
                return GetResourceString("MSGLIST_NOT_ALLOW_EXCEED_RATIO");
            }
        }

        /// 室内机和房间的需求不符合!
        /// <summary>
        /// 室内机和房间的需求不符合!
        /// </summary>
        public static string MSGLIST_CANT_REACH_DEMAND
        {
            get
            {
                return GetResourceString("MSGLIST_CANT_REACH_DEMAND");
            }
        }

        ///名称不能为空格串
        /// <summary>
        /// 名称不能为空格串
        /// </summary> 
        public static string NAME_BLANKSTRING
        {
            get
            {
                return GetResourceString("NAME_BLANKSTRING");
            }
        }


        #endregion

        #region 文本框校验

        /// [{0}] 输入了非法字符！
        /// <summary>
        /// [{0}] 输入了非法字符！
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string WARNING_TXT_CHARACTER(string lblName)
        {
            return string.Format(GetResourceString("WARNING_TXT_CHARACTER"), lblName);
        }

        /// [{0}] 输入了非法数字！
        /// <summary>
        /// [{0}] 输入了非法数字！
        /// </summary>
        /// <param name="lblName"></param>
        /// <returns></returns>
        public static string WARNING_TXT_INVALIDNUM(string lblName)
        {
            return string.Format(GetResourceString("WARNING_TXT_INVALIDNUM"), lblName);
        }

        /// [{0}] 值应为正数！
        /// <summary>
        /// [{0}] 值应为正数！
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string WARNING_TXT_POSITIVENUM(string name)
        {
            return string.Format(GetResourceString("WARNING_TXT_POSITIVENUM"), name);
        }

        /// [{0}] 必须大于 {1}！
        /// <summary>
        /// [{0}] 必须大于 {1}！
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="min">开区间最小值</param>
        /// <returns></returns>
        public static string WARNING_TXT_GREATERTHAN(string lblName, string min)
        {
            return string.Format(GetResourceString("WARNING_TXT_GREATERTHAN"), lblName, min);
        }

        /// [{0}] 必须不小于（大于等于） {1}！
        /// <summary>
        /// [{0}] 必须不小于（大于等于） {1}！
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="min">闭区间最小值</param>
        /// <returns></returns>
        public static string WARNING_TXT_NOLESSTHAN(string lblName, string min)
        {
            return string.Format(GetResourceString("WARNING_TXT_NOLESSTHAN"),lblName, min);
        }

        /// [{0}] 必须小于 {1}！
        /// <summary>
        /// [{0}] 必须小于 {1}！
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="max">开区间最大值</param>
        /// <returns></returns>
        public static string WARNING_TXT_LESSTHAN(string lblName, string max)
        {
            return string.Format(GetResourceString("WARNING_TXT_LESSTHAN"), lblName, max);
        }

        /// 输入的数值范围应为 {0} ~ {1} ！
        /// <summary>
        /// 输入的数值范围应为 {0} ~ {1} ！
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string WARNING_TXT_BETWEEN(string min, string max)
        {
            return string.Format(GetResourceString("WARNING_TXT_BETWEEN"), min, max);
        }

        /// [{2}] 中输入的数值范围应该为 {0} 到 {1} !
        /// <summary>
        /// [{2}] 中输入的数值范围应该为 {0} 到 {1} !
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="lblName">标签名称</param>
        /// <returns></returns>
        public static string WARNING_TXT_BETWEEN2(string min, string max, string lblName)
        {
            return string.Format(GetResourceString("WARNING_TXT_BETWEEN2"), min, max, lblName);
        }

        /// [{0}] 输入内容不能为空！
        /// <summary>
        /// [{0}] 输入内容不能为空！
        /// </summary>
        /// <param name="lblName"></param>
        /// <returns></returns>
        public static string WARNING_TXT_NOTEMPTY(string lblName)
        {
            return string.Format(GetResourceString("WARNING_TXT_NOTEMPTY"), lblName);
        }

        /// [{0}] 字符超过上限 [{1}] !
        /// <summary>
        /// [{0}] 字符超过上限 [{1}] !
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="bytesLimite"></param>
        /// <returns></returns>
        public static string WARNING_TXT_BYTES_LIMITATION(string lblName, int bytesLimite)
        {
            return string.Format(GetResourceString("WARNING_TXT_BYTES_LIMITATION"), lblName, bytesLimite);
        }

        #endregion

        #region 各界面的提示信息

        /// Start界面——VRF产品并未在当前的区域发布，请直接联系Asia UPG Marketing。
        /// <summary>
        /// Start界面——VRF产品并未在当前的区域发布，请直接联系Asia UPG Marketing。
        /// </summary>
        public static string WARNING_NOREGION_VRF
        {
            get
            {
                return GetResourceString("WARNING_NOREGION_VRF");
            }
        }

        /// Project界面——海拔高度超出限制！
        /// <summary>
        /// Project界面——海拔高度超出限制！
        /// </summary>
        public static string WARNING_ALTITUDE
        {
            get
            {
                return string.Format(GetResourceString("WARNING_ALTITUDE"), 1000);
            }
        }

        /// Room窗口——请选择一个楼层节点
        /// <summary>
        /// Room窗口——请选择一个楼层节点
        /// </summary>
        public static string WARNING_SELECTFLOOR
        {
            get
            {     
                return GetResourceString("WARNING_SELECTFLOOR");
            }
        }


        /// LoadIndex窗口——{0} 被设为默认城市！
        /// <summary>
        /// LoadIndex窗口——{0} 被设为默认城市！
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public static string INFO_SETDEFAULTCITY(string cityName)
        {
            return string.Format(GetResourceString("INFO_SETDEFAULTCITY"), cityName);
        }


        /// Indoor 界面——当前室内机容量高于房间负荷的150%！
        /// <summary>
        /// Indoor 界面——当前室内机容量高于房间负荷的150%！
        /// </summary>
        public static string IND_WARN_CAPUpper
        {
            get
            {
                return GetResourceString("IND_WARN_CAPUpper");
            }
        }

        /// Indoor 界面——当前室内机容量低于房间负荷！
        /// <summary>
        /// Indoor 界面——当前室内机容量低于房间负荷！
        /// </summary>
        public static string IND_WARN_CAPLower
        {
            get
            {
                return GetResourceString("IND_WARN_CAPLower");
            }
        }

        /// 室内机容量低于房间需求!请确认是否接受该风险？
        /// <summary>
        /// 室内机容量低于房间需求!请确认是否接受该风险？
        /// </summary>
        public static string IND_WARN_CAPLower2
        {
            get
            {
                return GetResourceString("IND_WARN_CAPLower2");
            }
        }

        /// 不满足静压需求!
        /// <summary>
        /// 不满足静压需求!
        /// </summary>
        public static string Exc_NOTMEET_ESP
        {
            get
            {
                return GetResourceString("Exc_NOTMEET_ESP");
            }
        }


        /// 全热交换机静压低于房间需求!请确认是否接受该风险？
        /// <summary>
        /// 全热交换机静压低于房间需求!请确认是否接受该风险？
        /// </summary>
        public static string Exc_WARN_CAPESP
        {
            get
            {
                return GetResourceString("Exc_WARN_CAPESP");
            }
        }

        /// 不满足风量需求!
        /// <summary>
        /// 不满足风量需求!
        /// </summary>
        public static string Exc_NOTMEET_AirFlow
        {
            get
            {
                return GetResourceString("Exc_NOTMEET_AirFlow");
            }
        }


        /// 全热交换机风量低于房间需求!请确认是否接受该风险？
        /// <summary>
        /// 全热交换机风量低于房间需求!请确认是否接受该风险？
        /// </summary>
        public static string Exc_WARN_CAPFlowAir
        {
            get
            {
                return GetResourceString("Exc_WARN_CAPFlowAir");
            }
        }

        /// Indoor 界面——当前室内机风量低于房间需求！
        /// <summary>
        /// Indoor 界面——当前室内机风量低于房间需求！
        /// </summary>
        public static string IND_WARN_AFLower
        {
            get
            {
                return GetResourceString("IND_WARN_AFLower");
            }
        }

        /// Indoor 界面——当前室内机显热低于房间需求！
        /// <summary>
        /// Indoor 界面——当前室内机显热低于房间需求！
        /// </summary>
        public static string IND_WARN_SHLower
        {
            get
            {
                return GetResourceString("IND_WARN_SHLower");
            }
        }

        /// Indoor 界面——当前新风机风量低于房间或新风区域的风量需求！
        /// <summary>
        /// Indoor 界面——当前新风机风量低于房间或新风区域的风量需求！
        /// </summary>
        public static string IND_WARN_AFLowerFA
        {
            get
            {
                return GetResourceString("IND_WARN_AFLowerFA");
            }
        }

        /// Indoor 界面——自动选型时无匹配的室内机时
        /// <summary>
        /// Indoor 界面——自动选型时无匹配的室内机时
        /// </summary>
        public static string IND_NOTMATCH
        {
            get
            {
                return GetResourceString("IND_NOTMATCH");
            }
        }

        /// Exchanger 界面——自动选型时无匹配的全热交换机
        /// <summary>
        /// Exchanger 界面——自动选型时无匹配的全热交换机
        /// </summary>
        public static string EXC_NOTMATCH
        {
            get
            {
                return GetResourceString("EXC_NOTMATCH");
            }
        }

        /// 房间需求更改，已选室内机不满足房间需求.请重新选择！
        /// <summary>
        /// 房间需求更改，已选室内机不满足房间需求.请重新选择！
        /// </summary>
        public static string IND_NOTMATCH_ROOMCHANGE
        {
            get
            {
                return GetResourceString("IND_NOTMATCH_ROOMCHANGE");

            }
        }
        /// 房间需求更改，已选全热交换机不满足房间需求.请重新选择！
        /// <summary>
        /// 房间需求更改，已选全热交换机不满足房间需求.请重新选择！
        /// </summary>
        public static string Exc_NOTMATCH_ROOMCHANGE
        {
            get
            {
                return GetResourceString("Exc_NOTMATCH_ROOMCHANGE");

            }
        }


        /// 不满足制冷工况的需求！
        /// <summary>
        /// 不满足制冷工况的需求！
        /// </summary>
        public static string IND_NOTMEET_COOLING
        {
            get
            {
                return GetResourceString("IND_NOTMEET_COOLING");
            }
        }

        /// 不满足制热工况的需求！
        /// <summary>
        /// 不满足制热工况的需求！
        /// </summary>
        public static string IND_NOTMEET_HEATING
        {
            get
            {
                return GetResourceString("IND_NOTMEET_HEATING");
            }
        }

        /// 不满足新风的需求！
        /// <summary>
        /// 不满足新风的需求！
        /// </summary>
        public static string IND_DEL_FAIL
        {
            get
            {
                return GetResourceString("IND_DEL_FAIL");
            }
        }

        /// 新风机型号“JTAF1080”找不到适合的室外机
        /// <summary>
        /// 新风机型号“JTAF1080”找不到适合的室外机
        /// </summary>
        public static string IND_DEL_FRESHAIR_HINT
        {
            get
            {
                return GetResourceString("IND_DEL_FRESHAIR_HINT");
            }
        }
        

        /// 不满足新风的需求！
        /// <summary>
        /// 不满足新风的需求！
        /// </summary>
        public static string IND_NOTMEET_FA
        {
            get
            {
                return GetResourceString("IND_NOTMEET_FA");
            }
        }



        /// 请检查一对一室外机的型号是否匹配！
        /// <summary>
        /// 请检查一对一室外机的型号是否匹配！
        /// </summary>
        public static string OUTD_NOTMATCH_FA_Model
        {
            get {
                return GetResourceString("OUTD_NOTMATCH_FA_Model");
            }
        }

        /// Outdoor 窗口，没有合适的室外机！
        /// <summary>
        /// Outdoor 窗口，没有合适的室外机！
        /// </summary>
        public static string OUTD_NOTMATCH
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH");
            }
        }

        public static string OUTD_NOTMATCH_WATER
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_WATER");
            }
        }

        /// Outdoor 窗口，所选的新风机不支持一对一模式！
        /// <summary>
        /// Outdoor 窗口，所选的新风机不支持一对一模式！
        /// </summary>
        public static string OUTD_NOTMATCH_FA
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_FA");
            }
        }

        /// Outdoor 窗口，多台新风机时不能包含一对一的新风机！
        /// <summary>
        /// Outdoor 窗口，多台新风机时不能包含一对一的新风机！
        /// </summary>
        public static string OUTD_NOTMATCH_FAMulti
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_FAMulti");
            }
        }

        /// Outdoor 窗口，混连模式必须同时选择室内机与新风机！
        /// <summary>
        /// Outdoor 窗口，混连模式必须同时选择室内机与新风机！
        /// </summary>
        public static string OUTD_NOTMATCH_Composite
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_Composite");
            }
        }

        /// 混连模式中只能包含0510~2100的新风机！
        /// <summary>
        /// 混连模式中只能包含0510~2100的新风机！
        /// </summary>
        public static string OUTD_NOTMATCH_CompositeFA
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_CompositeFA");
            }

        }

        /// Outdoor 窗口，非混连模式不能同时包含室内机与新风机！
        /// <summary>
        /// Outdoor 窗口，非混连模式不能同时包含室内机与新风机！
        /// </summary>
        public static string OUTD_NOTMATCH_NoComposite
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_NoComposite");
            }
        }

        /// Outdoor 窗口，任何模式不能替换新风机！
        /// <summary>
        /// Outdoor 窗口，任何模式不能替换新风机！
        /// </summary>
        public static string OUTD_NOTMATCH_NoFreshAir
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_NoFreshAir");
            }
        }

        /// Outdoor 窗口，混连模式不能替换室内机！
        /// <summary>
        /// Outdoor 窗口，混连模式不能替换室内机！
        /// </summary>
        public static string OUTD_NOTMATCH_COMPOSITE_NoIndoor
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_COMPOSITE_NoIndoor");
            }
        }

        /// Outdoor 窗口，ME T3 mini与super类型合并为T3之后的限制信息
        /// <summary>
        /// Outdoor 窗口，ME T3 mini与super类型合并为T3之后的限制信息
        /// </summary>
        public static string OUTD_NOTMATCH_ME_T3mini
        {
            get
            {
                return GetResourceString("OUTD_NOTMATCH_ME_T3mini");
            }
        }


        /// Outdoor 窗口，室内机数量超过室外机的最大连接数！
        /// <summary>室内机数量 ({0}) 超过室外机的最大连接数 ({1})！ ON 20180108 by xyj
        /// Outdoor 窗口，室内机数量超过室外{1})机的最大连接数！
        /// </summary>
        public static string OUTD_INDOORNUM_EXCEED(string MaxCount, string MaxLimited)
        {

            return String.Format(GetResourceString("OUTD_INDOORNUM_EXCEED"), MaxCount, MaxLimited); 
            
        }

        /// Outdoor 窗口，室内外机配比率不符合要求（50%~130%）！
        /// <summary>
        /// Outdoor 窗口，室内外机配比率不符合要求（50%~130%）！
        /// </summary>
        public static string OUTD_RATIO_AllIndoor(double minRatio, double maxRatio)
        {
            return String.Format(GetResourceString("OUTD_RATIO_AllIndoor"), (minRatio * 100).ToString(), (maxRatio * 100).ToString());
        }

        /// Outdoor 窗口，室内外机容量配比率高于110%（含），可能会降低系统性能！
        /// <summary>
        /// Outdoor 窗口，室内外机容量配比率高于110%（含），可能会降低系统性能！
        /// 模式四：全是室外机（非FA）时
        /// </summary>
        public static string OUTD_RATIO_AllIndoor2
        {
            get
            {
                return GetResourceString("OUTD_RATIO_AllIndoor2");
            }
        }

        /// Outdoor 窗口，当前室内机组合模式下配比率应为80%～105%！
        /// <summary>
        /// Outdoor 窗口，当前室内机组合模式下配比率应为80%～105%！
        /// 新增需求：20141218，内机和与外机冷量配比率范围80%～105%，当配比率>105%时，显示此提示
        /// </summary>
        public static string OUTD_RATIO_Composite
        {
            get
            {
                return GetResourceString("OUTD_RATIO_Composite");
            }
        }

        /// Outdoor 窗口，室内机容量超过100%，请确认是否接受该风险？
        /// <summary>
        /// Outdoor 窗口，室内机容量超过100%，请确认是否接受该风险？
        /// </summary>
        public static string OUTD_RATIO_Composite2
        {
            get
            {
                return GetResourceString("OUTD_RATIO_Composite2");
            }
        }

        /// Outdoor 窗口，混合模式下新风机的容量不能超过室外机容量的30%！
        /// <summary>
        /// Outdoor 窗口，混合模式下新风机的容量不能超过室外机容量的30%！
        /// </summary>
        public static string OUTD_RATIO_CompositeFA
        {
            get
            {
                return GetResourceString("OUTD_RATIO_CompositeFA");
            }
        }

        /// 若要切换当前系统的内机组合类型，则已选的室内机记录将清空，确认继续？
        /// <summary>
        /// 若要切换当前系统的内机组合类型，则已选的室内机记录将清空，确认继续？
        /// </summary>
        public static string OUTD_CONFIRM_SYSTYPE
        {
            get
            {
                return GetResourceString("OUTD_CONFIRM_SYSTYPE");
            }
        }

        /// <summary>
        /// 室外机额定容量小于室内机估算容量之和 
        /// </summary>
        public static string OUTD_RATED_CAP_CHK
        {
            get
            {
                return GetResourceString("OUTD_RATED_CAP_CHK");
            }
        }

        //Controller
        /// 切换Controller布局类型时的确认，Bacnet或Modbus
        /// <summary>
        /// 切换Controller布局类型时的确认，Bacnet或Modbus
        /// </summary>
        public static string CONTROLLER_CONFIRM_CHANGELAYOUT
        {
            get
            {
                return GetResourceString("CONTROLLER_CONFIRM_CHANGELAYOUT");
            }
        }

        /// Controlller界面,请至少配置一台控制器到“{0}”
        /// <summary>
        /// Controlller界面,请至少配置一台控制器到“{0}”
        /// </summary>
        /// <param name="ControlGroupName"></param>
        /// <returns></returns>
        public static string CONTROLLER_NONE(string ControlGroupName)
        {
            return string.Format(GetResourceString("CONTROLLER_NONE"), ControlGroupName);
        }

        /// 请至少配置一台室外机系统到“{0}”
        /// <summary>
        /// 请至少配置一台室外机系统到“{0}”
        /// </summary>
        /// <param name="ControlGroupName"></param>
        /// <returns></returns>
        public static string CONTROLLER_NOOUTDOOR(string ControlGroupName)
        {
            return string.Format(GetResourceString("CONTROLLER_NOOUTDOOR"), ControlGroupName);
        }

        /// 当前开关控制器的数量超出限制！
        /// <summary>
        /// 当前开关控制器的数量超出限制！
        /// </summary>
        public static string CONTROLLER_ONOFF_QTY
        {
            get
            {
                return GetResourceString("CONTROLLER_ONOFF_QTY");
            }
        }

        /// 当前控制器组的室外机数量超出限制！
        /// <summary>
        /// 当前控制器组的室外机数量超出限制！
        /// </summary>
        public static string CONTROLLER_OUTDOOR_QTY
        {
            get
            {
                return GetResourceString("CONTROLLER_OUTDOOR_QTY");
            }
        }

        /// 当前控制器数量超出限制！
        /// <summary>
        /// 当前控制器数量超出限制！
        /// </summary>
        public static string CONTROLLER_QTY
        {
            get
            {
                return GetResourceString("CONTROLLER_QTY");
            }
        }

        /// 当前触摸屏控制器的数量超出限制！
        /// <summary>
        /// 当前触摸屏控制器的数量超出限制！
        /// </summary>
        public static string CONTROLLER_TOUCH_QTY
        {
            get
            {
                return GetResourceString("CONTROLLER_TOUCH_QTY");
            }
        }

        #region 控制器错误提示 add by ShenJunjie
        
        /// <summary>
        /// 设备数量超出限制
        /// </summary>
        public static string CONTROLLER_TOTAL_DEVICE_QTY(int number)
        {
           return string.Format(GetResourceString("CONTROLLER_TOTAL_DEVICE_QTY"), number);
        }

        /// <summary>
        /// 室内机数量超出限制
        /// </summary>
        public static string CONTROLLER_MAX_INDOOR_QTY(string model, int number)
        {
            return string.Format(GetResourceString("CONTROLLER_MAX_INDOOR_QTY"), model, number);
        }

        /// <summary>
        /// 室外机数量超出限制
        /// </summary>
        public static string CONTROLLER_MAX_OUTDOOR_QTY(string model, int number)
        {
            return string.Format(GetResourceString("CONTROLLER_MAX_OUTDOOR_QTY"), model, number);
        }

        /// <summary>
        /// 室外机数量超出H-Link限制
        /// </summary>
        public static string CONTROLLER_HLINK_OUTDOOR_QTY(string version, int number)
        {
            return string.Format(GetResourceString("CONTROLLER_HLINK_OUTDOOR_QTY"), version, number);
        }

        /// <summary>
        /// 室内机数量超出H-Link限制
        /// </summary>
        public static string CONTROLLER_HLINK_INDOOR_QTY(string version, int number)
        {
            return string.Format(GetResourceString("CONTROLLER_HLINK_INDOOR_QTY"), version, number);
        }

        /// <summary>
        /// 设备总数量超出H-Link限制
        /// </summary>
        public static string CONTROLLER_HLINK_DEVICE_QTY(string version, int number)
        {
            return string.Format(GetResourceString("CONTROLLER_HLINK_DEVICE_QTY"), version, number);
        }

        /// <summary>
        /// 集中控制器数量超出H-Link限制
        /// </summary>
        public static string CONTROLLER_HLINK_CENTRAL_CONTROLLER_QTY(int number)
        {
            return string.Format(GetResourceString("CONTROLLER_HLINK_CENTRAL_CONTROLLER_QTY"), number);
        }

        /// <summary>
        /// 与其它集中控制器不兼容
        /// </summary>
        public static string CONTROLLER_NOT_COMPATIBLE
        {
            get
            {
                return GetResourceString("CONTROLLER_NOT_COMPATIBLE");
            }
        }

        /// <summary>
        /// 与{0}不兼容
        /// </summary>
        public static string CONTROLLER_NOT_COMPATIBLE_WITH(string name)
        {
            return string.Format(GetResourceString("CONTROLLER_NOT_COMPATIBLE_WITH"), name);
        }

        /// <summary>
        /// 在1个H-Link组中只能连接{0}个这种设备。
        /// </summary>
        public static string CONTROLLER_HLINK_CONTROLLER_QTY(int number)
        {
            return string.Format(GetResourceString("CONTROLLER_HLINK_CONTROLLER_QTY"), number);
        }
        
        /// <summary>
        /// 数量超出限制，最大连接数{0}
        /// </summary>
        public static string ACCESSORY_EXCEEDLIMITATION(int number)
        {
            return string.Format(GetResourceString("ACCESSORY_EXCEEDLIMITATION"), number);
        }
        
        /// <summary>
        /// BACNet接口只能连接最多{0}个集中控制器！
        /// </summary>
        public static string CONTROLLER_BACNET_CC_QTY(int number)
        {
            return string.Format(GetResourceString("CONTROLLER_BACNET_CC_QTY"), number);
        }

        /// <summary>
        /// Web Based Cotnrol只能连接最多{0}个单元(室内机+室外机)！
        /// </summary>
        public static string CONTROLLER_WEBBASEDCONTROL_UNIT_QTY(int number)
        {
            return string.Format(GetResourceString("CONTROLLER_WEBBASEDCONTROL_UNIT_QTY"), number);
        }

        /// <summary>
        /// {0}只能连接最多{1}个单元(室内机+室外机)！
        /// </summary>
        public static string CONTROLLER_UNIT_QTY(string model, int number)
        {
            return string.Format(GetResourceString("CONTROLLER_UNIT_QTY"), model, number);
        }
        #endregion

        /// 产品类型不匹配！
        /// <summary>
        /// 产品类型不匹配！
        /// </summary>
        public static string CONTROLLER_PRODUCTTYPE_NOT_MATCH
        {
            get
            {
                return GetResourceString("CONTROLLER_PRODUCTTYPE_NOT_MATCH");
            }
        }


        #endregion

        #region 配管图界面
        
        /// <summary>
        /// 最大实际管长超出{0}米！
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_WARN_ACTLENGTH(string val, string ActLength)
        {
            return string.Format(GetResourceString("PIPING_WARN_ACTLENGTH"), ActLength, val);
        }

      
        /// <summary>
        /// 检验最长等效管长 175m
        /// </summary>
        /// <returns></returns>
        public static string PIPING_EQLENGTH(string val, string length)
        {
            return string.Format(GetResourceString("PIPING_WARN_EQLENGTH"), length, val);
        }

        /// <summary>
        /// 检验第一分歧管到最远端室内机的距离 40m
        /// </summary>
        /// <returns></returns>
        public static string PIPING_FIRSTLENGTH(string val,string length)
        {
            return string.Format(GetResourceString("PIPING_WARN_FIRSTLENGTH"), length,val); // bug修正
        }

        /// <summary>
        /// 室外机在室内机下方时，内外机高差超过极限40m
        /// </summary>
        /// <returns></returns>
        public static string PIPING_DIFF_INBELOW(string inModelName)
        {
            return string.Format(GetResourceString("PIPING_WARN_DIFF_INBELOW"), inModelName);
        }

         /// <summary>
        /// 室外机在室内机上方时，内外机高差超过极限50m
        /// </summary>
        /// <param name="inModelName"></param>
        /// <returns></returns>
        public static string PIPING_DIFF_INABOVE(string inModelName) 
        {
            return string.Format(GetResourceString("PIPING_WARN_DIFF_INABOVE"), inModelName);
        }


        public static string PIPING_LENGTHFACTOR(string sysName, string eqLen, string highDiff)
        {
            return string.Format(GetResourceString("PIPING_WARN_LENGTHFACTOR"), sysName, eqLen, highDiff);
        }


        /// <summary>
        /// 所有连接管的长度都必须大于0！
        /// </summary>
        public static string PIPING_LINK_LENGTH
        {
            get
            {
                return GetResourceString("PIPING_WARN_LINK_LENGTH");
            }
        }

        /// <summary>
        /// 检验室内机之间的高度差
        /// </summary>
        /// <param name="inModelName1"></param>
        /// <param name="inModelName2"></param>
        /// <returns></returns>
        public static string PIPING_DIFF_IN(string inModelName1, string inModelName2)
        {
            string length = "15";
            return string.Format(GetResourceString("PIPING_WARN_DIFF_IN"), inModelName1, inModelName2, length);
        }

        /// <summary>
        /// 室内外机的最大高度差大于最长等效管长时
        /// </summary>
        /// <param name="inModelName"></param>
        /// <returns></returns>
        public static string PIPING_DIFF_INOUT(string inModelName)
        {
            return string.Format(GetResourceString("PIPING_WARN_DIFF_INOUT"), inModelName);
        }

        /// <summary>
        /// YVOH的室内外机最大高度差，极限值特殊指定；
        /// YVOH30KH-0A，YVOH35KH-0A，YVOH40KH-0A在高度差方向为-15≤Y≤30，
        /// YVOH45AH-0A，YVOH50AH-0A，YVOH60AH-0A在高度差方向为-20≤Y≤30
        /// </summary>
        /// <param name="inModelName"></param>
        /// <returns></returns>
        public static string PIPING_DIFF_INOUTMAX(string inModelName)
        {
            return string.Format(GetResourceString("PIPING_WARN_DIFF_INOUTMAX"), inModelName);
        }

        /// <summary>
        /// 管长修正系数错误警告 
        /// </summary>
        public static string PIPING_PIPE_LENGTH
        {
            get
            {
                return GetResourceString("PIPING_WARN_PIPE_LENGTH");
            }
        }

        /// <summary>
        /// 添加CP时，提示先选中一个室内机节点
        /// </summary>
        public static string PIPING_ADDCP
        {
            get 
            {
                return GetResourceString("PIPING_WARN_ADDCP");
            }
        }
        /// <summary>
        /// 存在相同类型的房间类型
        /// </summary>
        public static string RoomType_NO_SAME
        {
            get
            {
                return GetResourceString("RoomType_NO_SAME");
            }
        }


        #endregion

        // 请选择房间
        /// <summary>
        /// 请选择房间！
        /// </summary>
        public static string INDBYROOM_NO_ROOM
        {
            get
            {
                return GetResourceString("INDBYROOM_NO_ROOM");
            }
        }

        // 项目的标题名称 
        /// <summary>
        /// 项目的标题名称！
        /// </summary>
        public static string PROJECT_TITLE_NAME
        {
            get
            {
                return GetResourceString("PROJECT_TITLE_NAME");
            }
        }


        // 以下提示或警告信息的中英文尚未最终确认
        /// 由于该房间分配多台室内机，请到室内机编辑界面执行删除！
        /// <summary>
        /// 由于该房间分配多台室内机，请到室内机编辑界面执行删除！
        /// </summary>
        public static string IND_INFO_DEL_MAIN
        {
            get 
            {
                return GetResourceString("IND_INFO_DEL_MAIN");
            }
        }

        /// <summary>
        /// 由于该房间已配置多台全热交换机，请到全热交换机编辑界面执行删除
        /// </summary>
        public static string Exc_INFO_DEL_MAIN
        {
            get {
                return GetResourceString("Exc_INFO_DEL_MAIN");
            }
        }

        /// <summary>
        /// 请选择对应房间下的全热交换机，一起进行拷贝
        /// </summary>
        public static string Exc_EXIST_INDUNIT
        {
            get {
                return GetResourceString("Exc_EXIST_INDUNIT");
            }
        }


        /// 该室内机已添加到当前项目，点击OK按钮将正式删除！
        /// <summary>
        /// 该室内机已添加到当前项目，点击OK按钮将正式删除！
        /// </summary>
        public static string IND_INFO_DEL
        {
            get 
            {
                return GetResourceString("IND_INFO_DEL");
            }
        }


        /// 该全热交换机已添加到当前项目，点击OK按钮将正式删除！
        /// <summary>
        /// 该全热交换机已添加到当前项目，点击OK按钮将正式删除！
        /// </summary>
        public static string EXC_INFO_DEL
        {
            get
            {
                return GetResourceString("EXC_INFO_DEL");
            }
        }

        /// 确定要清空配件吗?
        /// <summary>
        /// 确定要清空配件吗?
        /// </summary>
        public static String IDU_ACCESSORYE_CLEAR
        {
            get
            {
                return GetResourceString("IDU_ACCESSORYE_CLEAR");
            }
        }

        /// 该全热交换机已添加到控制，点击界面下方确定按钮将正式删除！
        /// <summary>
        /// 该全热交换机已添加到控制，点击界面下方确定按钮将正式删除！
        /// </summary>
        public static string EXC_INFOCONTROL_DEL
        {
            get
            {
                return GetResourceString("EXC_INFOCONTROL_DEL");
            }
        }


        /// 当前系统未设置室内机与室外机之间的高度差，点击界面下方的 “确定” 按钮将设置室内机与室外之间的高度差.
        /// <summary>
        /// 当前系统未设置室内机与室外机之间的高度差，点击界面下方的 “确定” 按钮将设置室内机与室外之间的高度差.
        /// </summary>
        public static string PIPING_IDUHIGHDIFFERENCE
        {
            get
            {
                return GetResourceString("PIPING_IDUHIGHDIFFERENCE");
            }
        }


        /// 管道长度规格最小值为{0}米
        /// <summary>
        /// 管道长度规格最小值为{0}米
        /// </summary>
        public static string PIPING_MINLENGTH(string length)
        {
            return string.Format(GetResourceString("PIPING_MINLENGTH"), length);
        }

        /// <summary>
        /// 分歧管之间的最小管长为{0}。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_MIN_LEN_BETWEEN_MULTI_KITS(string length)
        {
            return string.Format(GetResourceString("PIPING_MIN_LEN_BETWEEN_MULTI_KITS"), length);
        }


        /// 室内机已存在该配件，点击界面下方的“确定”按钮将删除，重新配置
        /// <summary>
        /// 室内机已存在该配件，点击界面下方的“确定”按钮将删除，重新配置
        /// </summary>
        public static string ACCESSORY_ADD(string indoorName)
        {
            return string.Format(GetResourceString("ACCESSORY_ADD"), indoorName);
        }


        /// 确定删除？如果删除该配件，将会还原室内机[{0}]类型配件
        /// <summary>
        /// 确定删除？如果删除该配件，将会还原室内机[{0}]类型配件
        /// </summary>
        public static string DEL_ACCESSORY_AIRPANEL(string indoorType)
        {
            return string.Format(GetResourceString("DEL_ACCESSORY_AIRPANEL"), indoorType);
        }

        /// 已存在该类型配件，点击界面下方的“确定”按钮将删除，重新配置！
        /// <summary>
        /// 已存在该类型配件，点击界面下方的“确定”按钮将删除，重新配置！
        /// </summary>
        public static string UPDATE_ACCESSORY_AIRPANEL
        {
            get
            {
                return GetResourceString("UPDATE_ACCESSORY_AIRPANEL");
            }
        }


        /// 室内机已存在该类型的配件，点击界面下方的“确定”按钮将过滤已添加的室内机配件，继续添加
        /// <summary>
        /// 室内机已存在该类型的配件，点击界面下方的“确定”按钮将过滤已添加的室内机配件，继续添加
        /// </summary>
        public static string ACCESSORY_ADD_MSG(string indoorName)
        {
            return string.Format(GetResourceString("ACCESSORY_ADD_MSG"), indoorName);
        }


        /// 室内机已存在该类型的配件，点击界面下方的“确定”按钮将过滤已超出配件数量，重新配置
        /// <summary>
        /// 室内机已存在该类型的配件，点击界面下方的“确定”按钮将过滤已超出配件数量，重新配置
        /// </summary>
        public static string ACCESSORY_UPDATE_MSG(string indoorName)
        {
            return string.Format(GetResourceString("ACCESSORY_UPDATE_MSG"), indoorName);
        }

        /// <summary>
        /// 删除该室内机配件，将同时删除关联的配件 
        /// </summary> 
        /// <returns></returns>
        public static string ACCESSORY_DELETEBYASSOCIATE_MSG
        {
           get
            {
                return GetResourceString("ACCESSORY_DELETEBYASSOCIATE_MSG");
            }
        }


        /// 请选择相同型号名称的室内机进行批量设置！
        /// <summary>
        /// 请选择相同型号名称的室内机进行批量设置！
        /// </summary>
        public static string OPTION_WARN_SETALL
        {
            get
            {
                return GetResourceString("OPTION_WARN_SETALL");
            }
        }

        /// {0} 机组当前版本不提供 Option 处理！
        /// <summary>
        /// {0} 机组当前版本不提供 Option 处理！
        /// </summary>
        /// <param name="DoorType"></param>
        /// <returns></returns>
        public static string OPTION_WARN_YDCC(string DoorType)
        {
            return string.Format(GetResourceString("OPTION_WARN_YDCC"),DoorType);
        }

        /// <summary>
        /// 房间已经被新风区域选中  Add on 20160810 by LingJiaQiu
        /// </summary>
        public static string WARNING_ROOM_SELECTED(string roomName)
        {
            return string.Format(GetResourceString("WARNING_ROOM_SELECTED"), roomName);
        }

        /// <summary>
        /// 数量超出限制
        /// </summary>
        public static string Accessory_Warn_Number
        {
            get
            {
                return GetResourceString("WARNING_EXCEEDLIMITATION");
            }
        }


        /// <summary>
        /// 合计
        /// </summary>
        public static string Report_Sum
        {
            get
            {
                return GetResourceString("Report_Sum");
            }
        }

        #region 管长检验警告新增项 add on 20160518 by Yunxiao Lin
        /// <summary>
        /// 液管总长
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_TOTALLIQUIDLENGTH(string val, string length)
        {
            return string.Format(GetResourceString("PIPING_WARN_TOTALLIQUIDLENGTH"), length, val);
        }
        /// <summary>
        /// 每个Multi-kit到每个IDU的距离
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_MKTOINDOORLENGTH(string val, string length)
        {
            return string.Format(GetResourceString("PIPING_WARN_MKTOINDOORLENGTH"), length, val);
        }
        /// <summary>
        /// CH Unit到每个IDU的距离总和
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_CHTOINDOORTOTALLENGTH(string length)
        {
            return string.Format(GetResourceString("PIPING_WARN_CHTOINDOORTOTALLENGTH"), length);
        }
        /// <summary>
        /// 在室外机和第一连接套件之间的管道总长度{0}不能长于{1}
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH(string pipe, string length)
        {
            return string.Format(GetResourceString("PIPING_FIRST_CONNECTION_KIT_TO_ODU_MAX_LENGTH"), pipe, length);
        }
        /// <summary>
        /// 各连接套件之间的管道长度{0}不能小于{1}
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_BETWEEN_CONNECTION_KITS_MIN_LENGTH(string pipe, string length)
        {
            return string.Format(GetResourceString("PIPING_BETWEEN_CONNECTION_KITS_MIN_LENGTH"), pipe, length);
        }
        /// <summary>
        /// 在第一分歧管和第一连接套件之间的管道长度不能小于{1}
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string PIPING_FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH(string length)
        {
            return string.Format(GetResourceString("PIPING_FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH"), length);
        }
        /// <summary>
        /// Main Branch数量
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string PIPING_MAINBRANCHCOUNT(string count)
        {
            return string.Format(GetResourceString("PIPING_WARN_MAINBRANCHCOUNT"), count);
        }
        /// <summary>
        /// Main Branch的分支制冷容量不满比例
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static string PIPING_COOLINGCAPACITYRATE(string rate)
        {
            return string.Format(GetResourceString("PIPING_WARN_COOLINGCAPACITYRATE"), rate);
        }
        /// <summary>
        /// Main branch的分支制热容量不满比例
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static string PIPING_HEATINGCAPACITYRATE(string rate)
        {
            return string.Format(GetResourceString("PIPING_WARN_HEATINGCAPACITYRATE"), rate);
        }
        /// <summary>
        /// Heat Recovery系统中Cooling only内机的容量不能超过50%
        /// </summary>
        /// <returns></returns>
        public static string PIPING_COOLINGONLYCAPACITY()
        {
            return GetResourceString("PIPING_WARN_COOLINGONLYCAPACITY");
        }
        /// <summary>
        /// 修改海拔选项提示
        /// </summary>
        /// <returns></returns>
        public static string CONFIRM_CHANGEALTITUDE()
        {
            return GetResourceString("CONFIRM_CHANGEALTITUDE");
        }
        public static string PIPING_INDOORNUMBERTOCH(string count)
        {
            return string.Format(GetResourceString("PIPING_WARN_INDOORNUMBERTOCH"), count);
        }
        /// <summary>
        /// 修改海拔选项提示
        /// </summary>
        /// <returns></returns>
        public static string PIPING_INDOORHPERTOMUTICH(string currentCapacity,string MaxCapacity)
        {
            return string.Format(GetResourceString("PIPING_INDOORHPERTOMUTICH"), currentCapacity, MaxCapacity);
        }

        #endregion
        /// <summary>
        /// 在Outdoor新增界面变更productType时弹出警告
        /// </summary>
        public static string CONFIRM_CHANGEOUTPRODUCTTYPE
        {
            get
            {
               return GetResourceString("CONFIRM_CHANGEOUTPRODUCTTYPE");
            }
        }

        /// <summary>
        /// 在Outdoor新增界面变更series时弹出警告
        /// </summary>
        public static string CONFIRM_CHANGE_SERIES_REMOVE_IDU(string series, string[] indoorNames)
        {
            return string.Format(GetResourceString("CONFIRM_CHANGE_SERIES_REMOVE_IDU"), indoorNames.Length, series, string.Join(",", indoorNames));
        }

        /// 室内机重新选型成功提示 add by axj 20170119
        /// <summary>
        /// 室内机重新选型成功提示
        /// </summary>
        public static string MatchIndoor_OK
        {
            get
            {
                return GetResourceString("MatchIndoor_OK");
            }
        }

        /// 解除共享控制器绑定并重置配件提示 add by axj 20180503
        /// <summary>
        /// 解除共享控制器绑定并重置配件提示
        /// </summary>
        public static string Indoor_Unbinding
        {
            get
            {
                return GetResourceString("Indoor_Unbinding");
            }
        }

        /// 重置配件提示 add by axj 20180503
        /// <summary>
        /// 重置配件提示
        /// </summary>
        public static string Indoor_ResetAccessories
        {
            get
            {
                return GetResourceString("Indoor_ResetAccessories");
            }
        }
      
        /// 室内机多选没有相同的的配件
        /// <summary>
        /// 室内机多选没有相同的的配件
        /// </summary>
        public static string IND_NO_SAME_ACCESSORY
        {
            get
            {
                return GetResourceString("IND_NO_SAME_ACCESSORY");
            }
        }

        /// 全热交换机 没有合适的配件
        /// <summary>
        /// 全热交换机 没有合适的配件
        /// </summary>
        public static string EXC_NO_AVAILABLE_ACCESSORY
        {
            get
            {
                return GetResourceString("EXC_NO_AVAILABLE_ACCESSORY");
            }
        }


        /// 交换机名称
        /// <summary>
        /// 交换机名称
        /// </summary>
        public static string EXCHANGERNAME_ACCESSORY
        {
            get
            {
                return GetResourceString("ACCESSORY_EXCHANGERNAME");
            }
        }

        /// 室内机多选已有配件的清空
        /// <summary>
        /// 室内机多选已有配件的清空
        /// </summary>
        public static string IND_IS_ACCESSORY(string indoorName)
        {
           
                return string.Format(GetResourceString("IND_IS_ACCESSORY"), indoorName);            
        }

        /// 室内机多选共享控制器室内机清除共享关联
        /// <summary>
        /// 室内机多选共享控制器室内机清除共享关联
        /// </summary>
        public static string IND_IS_Sharing_RemoteController(string indoorName,string groupIndoorName)
        {
            if (!string.IsNullOrEmpty(groupIndoorName))
                groupIndoorName = "[" + groupIndoorName + "]";
            return string.Format(GetResourceString("IND_IS_Sharing_RemoteController"), indoorName, groupIndoorName);
        }

        /// 室内机清除共享关联
        /// <summary>
        /// 室内机清除共享关联
        /// </summary>
        public static string IND_Delete_Sharing_RelationShip(string indoorName, string groupIndoorName)
        {
            if (!string.IsNullOrEmpty(groupIndoorName))
                groupIndoorName = "[" + groupIndoorName + "]";
            return string.Format(GetResourceString("IND_Delete_Sharing_RelationShip"), indoorName, groupIndoorName);
        }

        /// 室内机在不同的系统中共享控制器，只能选择在同一系统中共享控制器的室内机
        /// <summary>
        /// 室内机在不同的系统中共享控制器，只能选择在同一系统中共享控制器的室内机
        /// </summary>
        public static string IND_MutiSelected_Sys_Different(string ind1,string ind2)
        {
            return string.Format(GetResourceString("IND_MutiSelected_Sys_Different"), ind1, ind2);
        }

        #region 系统间室内机相互拖动提示 add by axj 20170119
        /// <summary>
        /// 室外机系统的产品类型不一样，不能移动！
        /// </summary>
        public static string OUTD_CANNOT_MOVE_Series
        {
            get
            {
                return GetResourceString("OUTD_CANNOT_MOVE_Series");
            }
        }
        /// <summary>
        /// 室外机系统{0}的类型是混连模式，不能移动
        /// </summary>
        /// <param name="strArg"></param>
        /// <returns></returns>
        public static string OUTD_CANNOT_MOVE_CompositeMode(string strArg)
        {

            return string.Format(GetResourceString("OUTD_CANNOT_MOVE_CompositeMode"), strArg);

        }
        /// <summary>
        /// 室内机不能移动到新风机系统！
        /// </summary>
        public static string OUTD_CANNOT_MOVE_IndoorToFresh
        {
            get
            {
                return GetResourceString("OUTD_CANNOT_MOVE_IndoorToFresh");
            }
        }
        /// <summary>
        /// 新风机不能移动到室内机系统！
        /// </summary>
        public static string OUTD_CANNOT_MOVE_FreshToIndoor
        {
            get
            {
                return GetResourceString("OUTD_CANNOT_MOVE_FreshToIndoor");
            }
        }
        /// <summary>
        /// 选中的室内机所在房间存在多个室内机将会被一起移动，室外机系统{0}移动后将被删除，确认是否继续操作？
        /// </summary>
        /// <param name="strArg"></param>
        /// <returns></returns>
        public static string OUTD_TOGETHER_MOVE_DELETE_SYSTEM(string strArg)
        {

            return string.Format(GetResourceString("OUTD_TOGETHER_MOVE_DELETE_SYSTEM"), strArg);

        }
        /// <summary>
        /// 选中的室内机所在房间存在多个室内机将会被一起移动，确认是否继续操作？
        /// </summary>
        public static string OUTD_TOGETHER_MOVE_BYROOM
        {
            get
            {
                return GetResourceString("OUTD_TOGETHER_MOVE_BYROOM");
            }
        }

        /// <summary>
        /// 选中的室内机与其他房间室内机存在共享控制器关系，是否将所有存在关系的房间室内机一起移动？
        /// </summary>
        public static string OUTD_TOGETHER_MOVE_BYROOM_WITH_RELATIONSHIP
        {
            get
            {
                return GetResourceString("OUTD_TOGETHER_MOVE_BYROOM_WITH_RELATIONSHIP");
            }
        }
        /// <summary>
        /// 室内机{0}不能用于该系统，因为所在房间{1}存在不可用在当前系统的室内机。
        /// </summary>
        public static string OUTD_OTHERS_UNAVAILABLE_IN_SAME_ROOM(string[] indoorNames, string roomName)
        {
            return string.Format(GetResourceString("OUTD_OTHERS_UNAVAILABLE_IN_SAME_ROOM"), string.Join(",", indoorNames), roomName);
        }
        /// <summary>
        /// 室外机系统{0}移动后将被删除，确认是否移动？
        /// </summary>
        /// <param name="strArg"></param>
        /// <returns></returns>
        public static string OUTD_CONFIRM_DELETE_SYSTEM(string strArg)
        {

            return string.Format(GetResourceString("OUTD_CONFIRM_DELETE_SYSTEM"), strArg);

        }

        /// 室外机共享关系关联拖拽   add on 20170623 by Lingjia Qiu
        /// <summary>
        /// 室外机共享关系关联拖拽
        /// </summary>
        public static string OUTD_RELATIONSHIP_ITEM_DRAP(string indoorName, string groupIndoorName)
        {
            return string.Format(GetResourceString("OUTD_RELATIONSHIP_ITEM_DRAP"), indoorName, groupIndoorName);
        }

        #endregion

        #region Manual Piping add by Shen Junjie 20170615
        public static string MANUAL_PIPING_HORIZONTAL
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_HORIZONTAL");
            }
        }

        public static string MANUAL_PIPING_VERTICAL
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_VERTICAL");
            }
        }

        public static string MANUAL_PIPING_LAYOUT_TYPE_NORMAL
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_LAYOUT_TYPE_NORMAL");
            }
        }

        public static string MANUAL_PIPING_LAYOUT_TYPE_BINARY_TREE
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_LAYOUT_TYPE_BINARY_TREE");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_IND(string strArg)
        {
            return string.Format(GetResourceString("MANUAL_PIPING_MENU_ADD_IND"), strArg);
        }

        public static string MANUAL_PIPING_MENU_ADD_IND1(string strArg)
        {
            return string.Format(GetResourceString("MANUAL_PIPING_MENU_ADD_IND1"), strArg);
        }

        public static string MANUAL_PIPING_MENU_ADD_IND2
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_IND2");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_YP
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_YP");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_YP1
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_YP1");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_CP
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_CP");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_CP1
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_CP1");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_CHBox
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_CHBox");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_CHBox1
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_CHBox1");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_MULTI_CHBOX
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_MULTI_CHBOX");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_MULTI_CHBOX1
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_MULTI_CHBOX1");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_TO_LEFT
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_TO_LEFT");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_TO_RIGHT
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_TO_RIGHT");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_TO_ABOVE
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_TO_ABOVE");
            }
        }

        public static string MANUAL_PIPING_MENU_ADD_TO_BELOW
        {
            get
            {
                return GetResourceString("MANUAL_PIPING_MENU_ADD_TO_BELOW");
            }
        }
        #endregion

        #region "IVX"系列 手动选型错误信息 20170703
        /// <summary>
        /// [{0}]不能与Four way type 室内机搭配使用.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_NotMatch_FourWayTypeIDU(string model)
        {
            return string.Format(GetResourceString("OUTD_NotMatch_FourWayTypeIDU"), model);
        }
        /// <summary>
        /// 当环境温度小于等于{0}时，[{1}]与室内机的容量配比范围为{2}%~{3}%
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_TemperatureCapacityRatio_LessOrEqual(double temperature, string model, double MinRatio, double MaxRatio)
        {
            string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            double dTemperature = Unit.ConvertToControl(temperature, UnitType.TEMPERATURE, utTemperature);
            return string.Format(GetResourceString("OUTD_TemperatureCapacityRatio_LessOrEqual"), dTemperature + utTemperature, model, MinRatio, MaxRatio);
        }

        /// <summary>
        /// 当环境温度小于{0}时，[{1}]与室内机的容量配比范围为{2}%-{3}%
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_TemperatureCapacityRatio_Less(double temperature, string model, double MinRatio, double MaxRatio)
        {
            string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            double dTemperature = Unit.ConvertToControl(temperature, UnitType.TEMPERATURE, utTemperature);
            return string.Format(GetResourceString("OUTD_TemperatureCapacityRatio_Less"), dTemperature + utTemperature, model, MinRatio, MaxRatio);
        }

        /// <summary>
        /// 当环境温度大于{0}时，[{1}]与室内机的容量配比范围为{2}%~{3}%
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_TemperatureCapacityRatio_More(double temperature, string model, double MinRatio, double MaxRatio)
        {
            string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            double dTemperature = Unit.ConvertToControl(temperature, UnitType.TEMPERATURE, utTemperature);
            return string.Format(GetResourceString("OUTD_TemperatureCapacityRatio_More"), dTemperature + utTemperature, model, MinRatio, MaxRatio);
        }


        /// <summary>
        /// 当环境温度大于{0}时，1-for-1连接，[{1}]连接的室内机匹数不能超过{2}HP.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_TemperatureConnection1For1_More(double temperature, string model, double Hp)
        {
            string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            double dTemperature = Unit.ConvertToControl(temperature, UnitType.TEMPERATURE, utTemperature);
            return string.Format(GetResourceString("OUTD_TemperatureConnection1For1_More"), dTemperature + utTemperature, model, Hp);
        }

        /// <summary>
        /// 当环境温度大于{0}时，1-for-N连接，[{1}]连接的室内机匹数不能超过{2}HP.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_TemperatureConnection1ForN_More(double temperature, string model, double Hp)
        {
            string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            double dTemperature = Unit.ConvertToControl(temperature, UnitType.TEMPERATURE, utTemperature);
            return string.Format(GetResourceString("OUTD_TemperatureConnection1ForN_More"), dTemperature + utTemperature, model, Hp);
        }

        /// <summary>
        /// 当环境温度大于{0},连接的室内机数量不超过{1}时，[{2}]与室内机的容量配比范围为{3}%-{4}%.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_TemperatureCapacityRatioConnectionLimit_More(double temperature, int connNum, string model, double MinRatio, double MaxRatio)
        {
            string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            double dTemperature = Unit.ConvertToControl(temperature, UnitType.TEMPERATURE, utTemperature);
            return string.Format(GetResourceString("OUTD_TemperatureCapacityRatioConnectionLimit_More"), dTemperature + utTemperature, connNum, model, MinRatio, MaxRatio);
        }

        /// <summary>
        /// [{0}]只能与以下Indoor搭配使用:
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string OUTD_ODUMatchIDUList(string model)
        {
            return string.Format(GetResourceString("OUTD_ODUMatchIDUList"), model);
        }

        #endregion

        #region Hydro Free 选型错误信息 20171220 by Yunxiao Lin
        /// <summary>
        /// 当系统中安装了Hydro Free室内机时，其他室内机与室外机的容量配比范围不能超出{0}%-{1}%.
        /// </summary>
        /// <param name="MinRatio"></param>
        /// <param name="MaxRatio"></param>
        /// <returns></returns>
        public static string HydroFree_OtherIndoorRatio_LessOrMore(double MinRatio, double MaxRatio)
        {
            return string.Format(GetResourceString("HydroFree_OtherIndoorRatio_LessOrMore"), MinRatio, MaxRatio);
        }
        /// <summary>
        /// 当系统中安装了Hydro Free室内机时，Hydro Free与室外机的容量配比范围不能超出{0}%-{1}%.
        /// </summary>
        /// <param name="MinRatio"></param>
        /// <param name="MaxRatio"></param>
        /// <returns></returns>
        public static string HydroFree_HydroFreeRatio_LessOrMore(double MinRatio, double MaxRatio)
        {
            return string.Format(GetResourceString("HydroFree_HydroFreeRatio_LessOrMore"), MinRatio, MaxRatio);
        }
        #endregion

        /// <summary>
        /// 系统中KPI-X4E的总匹数不能大于系统中室内机总匹数的30%
        /// </summary>
        /// <param name="MinRatio"></param>
        /// <param name="MaxRatio"></param>
        /// <returns></returns>
        public static string KPI_X4E_HP_MAX_RATIO_LIMITATION(double maxRatio)
        {
            return string.Format(GetResourceString("KPI_X4E_HP_MAX_RATIO_LIMITATION"), maxRatio);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string UNMAINTAINABLE_ODU_IDU_WARNING(string list)
        {
            return string.Format(GetResourceString("UNMAINTAINABLE_ODU_IDU_WARNING"), list);
        }

        /// <summary>
        /// 
        /// </summary>
        public static string UNMAINTAINABLE_SYS_WARNING
        {
            get
            {
                return GetResourceString("UNMAINTAINABLE_SYS_WARNING");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string UNMAINTAINABLE_PROJECT_WARNING
        {
            get
            {
                return GetResourceString("UNMAINTAINABLE_PROJECT_WARNING");
            }
        }
        /// <summary>
        /// CSNET MANAGER 2 T10 or CSNET MANAGER 2 T15 must be selected
        /// add by axj 20180330
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ERR_CONTROLLER_HC_A64NET()
        {
            return GetResourceString("ERR_CONTROLLER_HC_A64NET");
            //return string.Format(GetResourceString("ERR_CONTROLLER_HC_A64NET"), num);
        }
        /// <summary>
        /// PSC-A160WEB1 or  HC-A64NET must be added or  PSC-A160WEB1 + HC-A64NET can be added.
        ///  add by axj 20180330
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ERR_CONTROLLER_CSNET_MANAGER_LT_XT()
        {
            return GetResourceString("ERR_CONTROLLER_CSNET_MANAGER_LT_XT");
            //return string.Format(GetResourceString("ERR_CONTROLLER_CSNET_MANAGER_LT_XT"), num);
        }

        /// <summary>
        /// The system count must be equal or greater than count of HC-A64NET and PSC-A160WEB1
        /// add by axj 20180330
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ERR_CONTROLLER_SYSTEM_NUM()
        {            
            return string.Format(GetResourceString("ERR_CONTROLLER_SYSTEM_NUM"));
        }
        
        /// 当前[{0}] 已经存在，请修改其他室内机名称！
        /// <summary>
        /// 当前[{0}] 已经存在，请修改其他室内机名称！   --add on 20180418 by Vince
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string WARNING_IND_NAME_EXIST(string indName)
        {
            return string.Format(GetResourceString("WARNING_IND_NAME_EXIST"), indName);
        }


        //add by 房间名称自定义 on 2018/05/01 by xyj
        /// <summary>
        /// 房间名称自定义。
        /// </summary>
        public static string ManagementRoom_Custom
        {
            get
            {
                return GetResourceString("ManagementRoom_Custom");
            }
        }



        public static string ODU_TEMPERATURE_REVISE
        { 
          get
            {
                return GetResourceString("ODU_TEMPERATURE_REVISE");
            }
        }

        /// 室外机和室内机之间的高差不能大于系统上限允许的最大高度差[{0}]！
        /// <summary>
        /// 室外机和室内机之间的高差不能大于系统上限允许的最大高度差[{0}]！   --add on 20180502 by xyj
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string Piping_HeightDiffH(string Length)
        {
            return string.Format(GetResourceString("Piping_HeightDiffH"), Length);
        }

        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！
        /// <summary>
        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！   --add on 20180502 by xyj
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string Piping_HeightDiffL(string Length)
        {
            return string.Format(GetResourceString("Piping_HeightDiffL"), Length);
        }

        /// 室内机之间的高度差不能大于系统允许的最大室内机高度差[{0}]！
        /// <summary>
        /// 室内机之间的高度差不能大于系统允许的最大室内机高度差[{0}]！   --add on 20180502 by xyj
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string Piping_Indoor_HeightDiff(string Length)
        {
            return string.Format(GetResourceString("Piping_Indoor_HeightDiff"), Length);
        }

        /// 切换选项后，当前项目已选的机组数据将全部被清空，确认继续？
        /// <summary>
        /// 切换选项后，当前项目已选的机组数据将全部被清空，确认继续？   --add on 20180524 by Vince
        /// </summary>
        /// <param name="currentType">带房间/不带房间</param>
        public static string CONFIRM_CHANGE_INDOOR_TYPE(string currentType)
        {
            return string.Format(GetResourceString("CONFIRM_CHANGE_INDOOR_TYPE"), currentType);
        }

        /// 不带房间的室内机选型/带房间的选型
        /// <summary>
        /// 不带房间的室内机选型/带房间的选型   --add on 20180525 by Vince
        /// </summary>
        /// <param name="isWithRoom">是否带房间</param>
        public static string INDOOR_SELECTED_TYPE(bool isWithRoom)
        {
            if (isWithRoom)
                return string.Format(GetResourceString("SELECT_IND_WITH_ROOM"));

            return string.Format(GetResourceString("SELECT_IND_WITHOUT_ROOM"));
        }

        //  房间类型已存在
        /// <summary>
        /// 房间类型已存在   --add on 20180614 by Vince
        /// </summary>
        /// <param name="currentType">带房间/不带房间</param>
        public static string ROOM_TYPE_IS_EXIST()
        {
            return string.Format(GetResourceString("ROOM_TYPE_IS_EXIST"));
        }

        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！
        /// <summary>
        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！   --add on 20180502 by xyj
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string DiffCHBox_IndoorHeightValue(string Length)
        {
            return string.Format(GetResourceString("DiffCHBox_IndoorHeightValue"), Length);
        }
        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！
        /// <summary>
        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！   --add on 20180502 by xyj
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string DiffMulitBoxHeightValue(string Length)
        {
            return string.Format(GetResourceString("DiffMulitBoxHeightValue"), Length);
        }
        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！
        /// <summary>
        /// 室外机和室内机之间的高差不能大于系统[{0}]下限允许的最大高度差！   --add on 20180502 by xyj
        /// </summary>
        /// <param name="lblName"></param>
        /// <param name="indName"></param>
        /// <returns></returns>
        public static string DiffCHBoxHeightValue(string Length)
        {
            return string.Format(GetResourceString("DiffCHBoxHeightValue"), Length);
        }

        /// <summary>
        /// 室内机到室外机的管长 小于室内机设置的高度差 on 20180621 by xyj
        /// </summary>
        public static string INDOORLENGTH_HIGHDIFF_MSG(string Length,string Height)
        {
            return string.Format(GetResourceString("INDOORLENGTH_HIGHDIFF"), Length, Height);
        }

        /// <summary>
        /// CHBOX到室外机的管长 小于CHBox设置的高度差 on 20180621 by xyj
        /// </summary>
        public static string CHBOXLENGTH_HIGHDIFF_MSG(string Length, string Height)
        {
            return string.Format(GetResourceString("CHBOXLENGTH_HIGHDIFF"), Length, Height);
        }

        /// <summary>
        /// 室外机到CHBOX的管长 小于室内机到CHBOX的高度差 on 20180621 by xyj
        /// </summary>
        public static string CHBOX_INDOORLENGTH_HIGHDIFF_MSG(string Length, string Height)
        {
            return string.Format(GetResourceString("CHBOX_INDOORLENGTH_HIGHDIFF"), Length, Height); 
        }

        public static string NO_IDU_CAN_BE_ASSIGNED()
        {
            return string.Format(GetResourceString("NO_IDU_CAN_BE_ASSIGNED"));
        }

        /// 组件不存在，请删除它！
        /// <summary>
        /// 组件不存在，请删除它！
        /// </summary>
        public static string ERR_CONTROLLER_NOCONTROLLER(string model)
        {
            return string.Format(GetResourceString("ERR_CONTROLLER_NOCONTROLLER"), model);
        }

        /// 配件不存在，请删除它！
        /// <summary>
        /// 配件不存在，请删除它！
        /// </summary>
        public static string ERR_ACCESSORY_NOACCESSORY(string model)
        {
            return string.Format(GetResourceString("ERR_ACCESSORY_NOACCESSORY"), model);
        }


    }
}
