//********************************************************************
// 文件名: UtilWord.cs
// 描述: 封装对 Word 的一些辅助方法
// 作者: clh
// 创建时间: 2012-04-13 
// 修改历史: 
//********************************************************************

using System;
using System.Collections.Generic;
using System.Data;

using Word = Microsoft.Office.Interop.Word;
using JCBase.UI;

namespace JCHVRF.Report
{
    public class UtilWord
    {
        // 给 Word 文档中的数据表插入空行（注：表中已有一行空行~）
        #region InsertEmptyRowsToTable
        /// <summary>
        /// 给 Word 文档中的数据表插入空行（注：表中已有一行空行~）
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="table"> word 文档中的表格 </param>
        /// <param name="docRowStart"> word 表格的起始行 </param>
        /// <param name="rowCount">要插入的空行数目</param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertEmptyRowsToTable(ref Word.Selection selection, ref Word.Table table, 
            int docRowStart,int docColStart, int rowCount, out string logInfo)
        {
            bool res = true;
            try
            {
                // 2012-01-10 Update 
                if (rowCount > 1)
                {
                    table.Cell(docRowStart, docColStart).Select();
                    selection.InsertRowsBelow(rowCount - 1);
                    logInfo = "插入空行成功！";
                }
                else
                {
                    res = false;
                    logInfo = "无对应的数据记录！";
                }
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "插入空行失败！";
            }
            return res;
        }
        #endregion

        // 给 Word 文档中的数据表插入数据（不带合计行）
        #region InsertDataToTable
        /// <summary>
        /// 给 Word 文档中的数据表插入数据（不带合计行）
        /// </summary>
        /// <param name="table"> word 文档中的表格 </param>
        /// <param name="docRowStart"> word 表格的起始行 </param>
        /// <param name="docColStart"> word 表格的起始列 </param>
        /// <param name="data"> DataTable 数据源 </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertDataToTable(ref Word.Table table, int docRowStart, int docColStart,
            DataTable data, out string logInfo)
        {
            bool res = true;
            try
            {
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow RowItem = data.Rows[rIndex];
                    for (int cIndex = 0; cIndex < data.Columns.Count; cIndex++)
                    {
                        string colValueStr = RowItem[cIndex].ToString();
                        // 逐个单元格填充表格数据
                        table.Cell(rIndex + docRowStart, cIndex + docColStart).Range.Text = colValueStr;
                    }
                }
                res = true;
                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "数据写入失败！";
            }
            return res;
        }
        #endregion

        // 给 Word 文档中的数据表插入数据(带合计行)
        #region InsertDataToTableWithSum
        /// <summary>
        /// 给 Word 文档中的数据表插入数据(带合计行)--注意：插入数据源无需拆分单元格！！
        /// </summary>
        /// <param name="table"> word 文档中的表格 </param>
        /// <param name="docRowStart"> word 表格的起始行 </param>
        /// <param name="docColStart"> word 表格的起始列 </param>
        /// <param name="data"> DataTable 数据源 </param>
        /// <param name="sumColIndexArray"> 求和的列的索引（数据源中的列索引号） </param>
        /// <param name="sumColIndexDiff"> word 文档中的表的合计列号跟数据源的列索引号的差距（正差） </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertDataToTableWithSum(ref Word.Table table, int docRowStart, int docColStart,
            DataTable data, int[] sumColIndexArray, int sumColIndexDiff, out string logInfo)
        {
            bool res = true;
            try
            {
                // 定义并初始化存放合计值的 list
                List<double> sumValueArray = new List<double>();
                for (int i = 0; i < sumColIndexArray.Length; i++)
                    sumValueArray.Add(0);

                // 逐行填充表格的数据部分（合计行除外）并累计每一个合计值
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow RowItem = data.Rows[rIndex];
                    // 逐列填充表格数据
                    for (int cIndex = 0; cIndex < data.Columns.Count; cIndex++)
                    {
                        string colValueStr = RowItem[cIndex].ToString();
                        table.Cell(rIndex + docRowStart, cIndex + docColStart).Range.Text = colValueStr;
                    }
                    // 累计计算当前行中参与合计的列
                    for (int i = 0; i < sumColIndexArray.Length; i++)
                    {
                        int cIndex = sumColIndexArray[i];
                        string colValueStr = RowItem[cIndex].ToString();
                        if (colValueStr == "-" || string.IsNullOrEmpty(colValueStr))
                            continue;
                        sumValueArray[i] += Convert.ToDouble(colValueStr);
                    }
                }

                // 逐个单元格填充合计行的值
                for (int i = 0; i < sumColIndexArray.Length; i++)
                {
                    string sumValueStr = sumValueArray[i].ToString();
                    if (sumValueStr == "0")
                        sumValueStr = "-";
                    table.Cell(table.Rows.Count, sumColIndexArray[i] + 1 + sumColIndexDiff).Range.Text = sumValueStr;
                }

                res = true;
                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "数据写入失败！";
            }
            return res;
        }
        #endregion

        // 在书签位置 插入指定文本（不带style）
        #region InsertTextToBookMark
        /// <summary>
        /// 在书签位置 插入指定文本（不带style）
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="markName"> BookMark Name </param>
        /// <param name="text"> 插入的字符串内容 </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertTextToBookMark(ref Word.Selection selection, 
            string markName, string text, out string logInfo)
        {
            bool res = true;
            try
            {
                GoToBookMark(ref selection, markName);
                selection.TypeText(text);
                logInfo = "写入 【" + markName + "】 成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "写入 【" + markName + "】 失败！";
            }
            return res;
        }
        #endregion

        // 在书签位置 插入指定文本（带style）
        #region InsertTextToBookMarkWithStyle
        /// <summary>
        /// 在书签位置 插入指定文本（带style）
        /// </summary>
        /// <param name="style"> 指定样式 </param>
        /// <param name="selection"></param>
        /// <param name="markName"> BookMark Name </param>
        /// <param name="text"> 插入的字符串内容 </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertTextToBookMarkWithStyle(object style, ref Word.Selection selection,
            string markName, string text, out string logInfo)
        {
            bool res = true;
            try
            {
                GoToBookMark(ref selection, markName);
                selection.set_Style(style);
                selection.TypeText(text);
                logInfo = "写入 【" + markName + "】 成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "写入 【" + markName + "】 失败！";
            }
            return res;
        }
        #endregion

        // 在书签位置 插入指定文本（带style和font）
        #region InsertTextToBookMarkWithStyle
        /// <summary>
        /// 在书签位置 插入指定文本（带style）
        /// </summary>
        /// <param name="style"> 指定样式 </param>
        /// <param name="font"> 指定字体 </param>
        /// <param name="selection"></param>
        /// <param name="markName"> BookMark Name </param>
        /// <param name="text"> 插入的字符串内容 </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertTextToBookMarkWithStyle(object style, Word.Font font, ref Word.Selection selection,
            string markName, string text, out string logInfo)
        {
            bool res = true;
            try
            {
                GoToBookMark(ref selection, markName);
                selection.set_Style(style);
                selection.Font = font;
                selection.TypeText(text);
                logInfo = "写入 【" + markName + "】 成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "写入 【" + markName + "】 失败！";
            }
            return res;
        }
        #endregion

        // 插入指定文本（带style和font）
        #region InsertTextWithStyle
        /// <summary>
        /// 在书签位置 插入指定文本（带style）
        /// </summary>
        /// <param name="style"> 指定样式 </param>
        /// <param name="font"> 指定字体 </param>
        /// <param name="selection"></param>
        /// <param name="text"> 插入的字符串内容 </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertTextWithStyle(object style, ref Word.Selection selection,
            string text, out string logInfo)
        {
            bool res = true;
            try
            {
                selection.set_Style(style);
                selection.TypeText(text);
                logInfo = "写入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "写入失败！";
            }
            return res;
        }
        #endregion

        // 在 word 文档中当前光标位置中插入指定路径下的图片
        #region InsertPicture
        /// <summary>
        /// 在 word 文档中当前光标位置中插入指定路径下的图片
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="fullPath"></param>
        /// <param name="logInfo"></param>
        /// <returns></returns>
        public static bool InsertPicture(ref Word.Selection selection, string fullPath,out string logInfo)
        {
            bool res = true;
            try
            {
                object linkToFile = false;      //默认
                object saveWithDocument = true; //默认
                //object drawrange = selection.Range;
                object Nothing = System.Reflection.Missing.Value;
                Word.InlineShape inlineShape = selection.InlineShapes.AddPicture(fullPath, ref linkToFile, ref saveWithDocument, ref Nothing);

                //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(drawPath);
                //double dpi = bmp.VerticalResolution; //96
                //设置图片大小 
                int maxHeight = JCBase.Utility.Util.CentimeterToPixel(22, 72);//TODO:待确认 word 2003默认为72
                if (inlineShape.Height > maxHeight)
                {
                    inlineShape.Height = maxHeight;
                }


                logInfo = "图片插入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
                logInfo = "图片插入失败！";
            }
            return res;
        }
        #endregion

        // 复制并向下粘贴指定书签范围的内容
        #region CopyBookMarkRange
        /// <summary>
        /// 复制并向下粘贴指定书签范围的内容
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="bookMark"></param>
        public static void CopyBookMarkRange(ref Word.Selection selection, string bookMark)
        {
            object unit = Word.WdUnits.wdLine;
            object count = 1;
            GoToBookMark(ref selection, bookMark);
            selection.Copy();
            selection.MoveDown(unit, count);
        }
        #endregion

        // 定位到指定的书签位置
        #region GoToBookMark
        /// <summary>
        /// 定位到指定的书签位置
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="markName"></param>
        public static void GoToBookMark(ref Word.Selection selection, string markName)
        {
            object gotoWhat = Word.WdGoToItem.wdGoToBookmark;
            object missing = Type.Missing;
            object bookMarkName = markName;
            selection.GoTo(ref gotoWhat, ref missing, ref missing, ref bookMarkName);
        }
        #endregion

        // 内存回收
        #region CollectGC
        /// <summary>
        /// 内存回收
        /// </summary>
        /// <param name="myWordApp"></param>
        public static void CollectGC(ref Word._Application myWordApp)
        {
            object missing = Type.Missing;
            if (myWordApp != null)
            {
                myWordApp.Quit(ref missing, ref missing, ref missing);
                myWordApp = null;
            }
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(myWordApp);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }
        #endregion

        // 判断电脑中是否存在Word2003或更高版本
        #region ValWordVersion
        /// <summary>
        /// 判断电脑中是否存在Word2003或更高版本
        /// </summary>
        /// <param name="curWordApp">当前 word 应用程序</param>
        /// <returns></returns>
        public static bool ValWordVersion(ref Word._Application curWordApp)
        {
            bool res = true;
            try
            {
                if (curWordApp == null)
                    curWordApp = new Word.Application();

                if (Convert.ToDouble(curWordApp.Version) < 11)
                {
                    JCMsg.ShowErrorOK("The version of Microsoft Word is too low. It must be Microsoft Word 2003 or higher.");
                    res = false;
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                JCMsg.ShowErrorOK("Missing support software:\nMicrosoft Word 2003 or higher.");
                res = false;
            }
            catch (System.Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                res = false;
            }
            return res;
        }
        #endregion
    }
}
