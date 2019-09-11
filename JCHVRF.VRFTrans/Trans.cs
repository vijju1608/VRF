using System;
using System.Collections.Generic;
using System.Data;
using JCBase.UI;
using JCHVRF.Model;
using JCBase.Utility;

namespace JCHVRF.VRFTrans
{
    public class Trans
    {
        string langType;
        public Trans()
        {
            langType = LangType.CurrentLanguage;   //默认语言取窗体语言
        }
        public Trans(string langType)
        {
            this.langType = langType;
        }
        #region 获取资源文件翻译   add on 20180822 by Vince
        private string GetResourceTrans(string messageID)
        {
            messageID = "T_" + messageID;
            string strCurLanguage = "";
            try
            {
                if (langType == LangType.CHINESE || langType == LangType.CHINESE_SIMPLE)
                {
                    strCurLanguage = Translation_ZH.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.CHINESE_TRADITIONAL)
                {
                    strCurLanguage = Translation_ZHT.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.ENGLISH)
                {
                    strCurLanguage = Translation.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.FRENCH)
                {
                    strCurLanguage = Translation_FR.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.TURKISH)
                {
                    strCurLanguage = Translation_TK.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.SPANISH)
                {
                    strCurLanguage = Translation_SP.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.GERMANY)
                {
                    strCurLanguage = Translation_DE.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.ITALIAN)
                {
                    strCurLanguage = Translation_IT.ResourceManager.GetString(messageID);
                }
                else if (langType == LangType.BRAZILIAN_PORTUGUESS)
                {
                    strCurLanguage = Translation_PT_BR.ResourceManager.GetString(messageID);
                }
                else
                {
                    strCurLanguage = Translation.ResourceManager.GetString(messageID);
                }
            }
            catch
            {
                strCurLanguage = null;
            }
            return strCurLanguage;
        }
        #endregion

        /// 获取翻译重新构造DataTable
        /// <summary>
        /// 获取翻译重新构造DataTable   add on 20180823 by Vince
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="dataTable"></param>
        public DataTable getTypeTransDt(string transType, DataTable dataTable, string columnsName)
        {
            //当前DataTable 为null 直接返回null on 20190107 by xyj
            if (dataTable == null)
                return null;

            if (!dataTable.Columns.Contains("Trans_Name"))
                dataTable.Columns.Add("Trans_Name");   //添加新列用于存放翻译后的名称
            foreach (DataRow dr in dataTable.Rows)
            {
                string resourceStr = dr[columnsName].ToString();
                string typeTrans = "";
                if (resourceStr.Contains("Total Heat Exchanger"))   //total heat exchanger特殊处理，Global作为series，EU地区作为IndoorType
                    typeTrans = getUnitTypeTrans(TransType.Series.ToString(), resourceStr);
                else
                    typeTrans = getUnitTypeTrans(transType, resourceStr);
                if (string.IsNullOrEmpty(typeTrans))
                    dr["Trans_Name"] = resourceStr;   //取不到翻译默认显示英文
                else
                    dr["Trans_Name"] = typeTrans;

            }
            return dataTable;
        }

        /// 获取翻译重新构造string
        /// <summary>
        /// 获取翻译重新构造string   add on 20180824 by Vince
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="resourceStr"></param>
        public string getTypeTransStr(string transType, string resourceStr)
        {
            //string typeTrans = getUnitTypeTrans(transType, resourceStr);
            string typeTrans = "";
            if (resourceStr.Contains("Total Heat Exchanger"))   //total heat exchanger特殊处理为Series【Global作为series，EU地区作为IndoorType】
                typeTrans = getUnitTypeTrans(TransType.Series.ToString(), resourceStr);
            else
                typeTrans = getUnitTypeTrans(transType, resourceStr);
            if (string.IsNullOrEmpty(typeTrans))
                return resourceStr;
            return typeTrans;
        }


        /// 获取UnitType翻译
        /// <summary>
        /// 获取UnitType翻译   add on 20180823 by Vince
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="resourceStr"></param>
        private string getUnitTypeTrans(string transType, string resourceStr)
        {
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Trans_Ship");
            DataView dv = dt.DefaultView;
            dv.RowFilter = "Type = '" + transType + "'and Resource = '" + resourceStr + "' and DeleteFlag = 1";
            string id = "";
            foreach (DataRow dr in dv.ToTable().Rows)
            {
                id = dr["ID"].ToString();
            }
            if (string.IsNullOrEmpty(id))
                return null;
            return GetResourceTrans(id);
        }

    }
}
