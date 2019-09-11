using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Tables;
using JCBase.UI;
using JCHVRF.BLL;
using JCHVRF.Const;
using JCHVRF.Model;
using JCHVRF.MyPipingBLL;
using JCBase.Utility;
using JCHVRF.VRFMessage;
using Lassalle.Flow;
using Lassalle.WPF.Flow;
using System.Drawing;
using System.Configuration;
using NGModel = JCHVRF.Model.NextGen;
using NGPipingBLL = JCHVRF.MyPipingBLL.NextGen;

using JCHVRF.VRFTrans;

namespace JCHVRF.Report
{
    public class ReportForAspose
    {
        #region 定义 word 应用程序相关的变量
        public static Document Doc = null;//doc文档
        public static Section selection = null;//选择区域
        public static NodeCollection Tables = null;//doc表集合
        public static DocumentBuilder Builder = null;//doc构造器
        public static Bookmark mark = null;
        public int tableindex = 0;//表索引
        public int[] chapterNumbers;

        //待删除的内容
        List<Aspose.Words.Node> DelNodeList = null;
        Dictionary<string, Dictionary<string, int>> dicMaterial = null;
        Dictionary<string, string> dicMaterialDes = null;

        Trans trans = new Trans();   //翻译初始化
        #endregion

        #region 初始化
        public ReportForAspose(Project proj)
        {
            JCBase.Utility.AsposeHelp.InitAsposeWord();

            this.thisProject = proj;
            bll = new ProjectBLL(proj);
            this.ReportLogList = new List<string>();
            DelNodeList = new List<Aspose.Words.Node>();
            dicMaterial = new Dictionary<string, Dictionary<string, int>>();
            dicMaterial["Indoor"] = new Dictionary<string, int>();
            dicMaterial["FreshAir"] = new Dictionary<string, int>();
            dicMaterial["Outdoor"] = new Dictionary<string, int>();
            dicMaterial["Accessory"] = new Dictionary<string, int>();
            dicMaterial["Controller"] = new Dictionary<string, int>();
            dicMaterial["PipingConnectionKit"] = new Dictionary<string, int>();
            dicMaterial["BranchKit"] = new Dictionary<string, int>();
            dicMaterial["CHBox"] = new Dictionary<string, int>();
            dicMaterial["Exchanger"] = new Dictionary<string, int>();

            dicMaterialDes = new Dictionary<string, string>();
            DataTable dt = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Piping_HeaderBranch");
            foreach (DataRow dr in dt.Rows)
            {
                dicMaterialDes[dr["Model_York"].ToString()] = "Header branch";
                dicMaterialDes[dr["Model_Hitachi"].ToString()] = "Header branch";
            }
        }

        /// <summary>
        /// 生成章节编号字符串
        /// </summary>
        /// <param name="level">需要增1的层级, 层级从0开始</param>
        /// <returns></returns>
        private string ChapterNumber(int level = -1, int number = -1)
        {
            if (level < 0)
            {
                //初始化
                chapterNumbers = new int[] { 0, 0 };
            }
            else
            {
                if (number > 0)
                {
                    chapterNumbers[level] = number;
                }
                else
                {
                    //加1
                    chapterNumbers[level]++;
                }
                for (int i = level + 1; i < chapterNumbers.Length; i++)
                {
                    chapterNumbers[i] = 0;
                }
            }
            string text = "";
            for (int i = 0; i < chapterNumbers.Length; i++)
            {
                if (chapterNumbers[i] < 1)
                {
                    break;
                }
                if (text.Length > 0)
                {
                    text += ".";
                }
                text += chapterNumbers[i];
            }
            return text;
        }

        /// <summary>
        /// add on 201508 clh 目前价格只对出口区域开放
        /// </summary>
        /// <returns></returns>
        private bool ShowPrice()
        {
            //20150712 clh v1.20版本仅对出口区域放开价格显示
            return CommonBLL.ShowPrice();
        }

        Project thisProject;
        ProjectBLL bll;

        /// <summary>
        /// report 操作日志文件
        /// </summary>
        List<string> ReportLogList;
        int TableSum = 3;

        #region 获取当前设置的单位表达式
        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string utArea = SystemSetting.UserSetting.unitsSetting.settingAREA;
        string utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        string utDimension = SystemSetting.UserSetting.unitsSetting.settingDimensionUnit;
        string utLoadIndex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
        string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
        string utWeight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
        string utPressure = Unit.ut_Pressure; //add by axj 20170710
        string utPowerInput = "kW";
        string utPowerInput_Outdoor = "kW";
        string utMaxOperationPI = "kW";
        #endregion

        #endregion

        //----------------word通用方法-----------------------------//

        #region 获得表集合
        private void GetTables()
        {
            Tables = Doc.GetChildNodes(Aspose.Words.NodeType.Table, true);
        }
        #endregion

        #region 插入书签
        private bool DocSetBookMark(string bookmark, string value)
        {
            bool flag = true;
            try
            {
                Doc.Range.Bookmarks[bookmark].Text = value;
            }
            catch (Exception ex)
            {
                flag = false;
                LogHelp.WriteLog("SetBookMark失败", ex);
            }
            return flag;
        }
        #endregion

        #region 定位书签
        /// <summary>
        /// 定位书签
        /// </summary>
        /// <param name="markName">书签名</param>
        public void GoToBookMark(string markName)
        {
            Doc.Range.Bookmarks["markName"].Remove();
        }
        #endregion

        #region 表格赋值
        /// <summary>
        /// 表格赋值
        /// </summary>
        /// <param name="tableindex">表格索引</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="colindex">列索引</param>
        /// <param name="content">内容</param>
        public void SetCellText(int tableindex, int rowIndex, int colindex, string content)
        {
            Builder.MoveToCell(tableindex, rowIndex, colindex, 0);
            Builder.Write(content);
        }

        public void SetCellText(int tableindex, int rowIndex, int colindex, string content, DocumentBuilder B)
        {
            B.MoveToCell(tableindex, rowIndex, colindex, 0);
            B.Write(content);
        }

        #endregion

        #region 给 Word 文档中的数据表插入空行（注：表中已有一行空行~）
        /// <summary>
        /// 给 Word 文档中的数据表插入空行（注：表中已有一行空行~）
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="table"> word 文档中的表格 </param>
        /// <param name="docRowStart"> word 表格的起始行 </param>
        /// <param name="rowCount">要插入的空行数目</param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public static bool InsertEmptyRowsToTable(int rowCount, Table table, out string logInfo)
        {
            bool res = true;
            try
            {
                if (rowCount > 1)
                {
                    Row srcRow = (Row)table.LastRow;
                    for (int i = 0; i < rowCount - 1; i++)
                    {
                        Row clonedRow = (Row)srcRow.Clone(true);
                        foreach (Cell cell in clonedRow.Cells)
                        {
                            if ((i + 1) % 2 == 0)
                            {
                                cell.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(230, 230, 230);
                            }
                            else
                            {
                                cell.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                            }
                        }
                        //    foreach (Cell cell in clonedRow.Cells) { cell.RemoveAllChildren(); }
                        table.AppendChild(clonedRow);
                    }
                    foreach (Cell cell in srcRow.Cells)
                    {
                        cell.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(230, 230, 230);
                    }
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

        public static bool InsertEmptyRowsToTable(int rowCount, Table table, int index, out string logInfo)
        {
            bool res = true;
            try
            {
                if (rowCount > 0)
                {
                    int num = 0;
                    Row srcRow = (Row)table.Rows[index];
                    for (int i = 0; i < rowCount; i++)
                    {
                        Row clonedRow = (Row)srcRow.Clone(true);
                        table.InsertAfter(clonedRow, table.Rows[index + num]);
                        foreach (Cell cell in clonedRow.Cells)
                        {
                            if ((i + 1) % 2 == 0)
                            {
                                cell.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(230, 230, 230);
                            }
                            else
                            {
                                cell.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                            }
                        }
                        num++;
                        //Row clonedRow = (Row)table.LastRow.Clone(true);
                        //    foreach (Cell cell in clonedRow.Cells) { cell.RemoveAllChildren(); }
                        //table.AppendChild(clonedRow);
                    }

                    foreach (Cell cell in srcRow.Cells)
                    {
                        cell.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(230, 230, 230);
                    }
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

        #region InsertDataToTable 给 Word 文档中的数据表插入数据（不带合计行）
        /// <summary>
        /// 给 Word 文档中的数据表插入数据（不带合计行）
        /// </summary>
        /// <param name="docRowStart"> word 表格的起始行 </param>
        /// <param name="docColStart"> word 表格的起始列 </param>
        /// <param name="data"> DataTable 数据源 </param>
        /// <param name="logInfo"> 日志内容 </param>
        /// <returns></returns>
        public bool InsertDataToTable(int docRowStart, int docColStart, DataTable data, out string logInfo)
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
                        SetCellText(tableindex, rIndex + docRowStart, cIndex + docColStart, colValueStr);
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

        #region 插入图片
        public void InsertPic(string filepath, int width, int height, int tableindex, int rowindex, int colinex)
        {
            Aspose.Words.Drawing.Shape shape = new Aspose.Words.Drawing.Shape(Doc, ShapeType.Image);
            shape.ImageData.SetImage(filepath);
            //shape.Width = width;
            //shape.Height = height;
            shape.AllowOverlap = false;
            shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center; //靠右对齐  

            //把此图片移动到那个单元格中
            Builder.MoveToCell(0, rowindex, colinex, 0);
            Builder.InsertNode(shape);



        }
        #endregion

        #region 表格复制
        public void CopyTable(int tableindex)
        {
            Table table = (Table)Doc.GetChild(Aspose.Words.NodeType.Table, tableindex, true);
            Table tableClone = (Table)table.Clone(true);
            table.ParentNode.InsertAfter(tableClone, table);
            table.ParentNode.InsertAfter(new Paragraph(Doc), table);
        }
        #endregion

        #region 书签复制
        /// <summary>
        /// 书签复制
        /// </summary>
        /// <param name="TargetDoc">目标文档</param>
        /// <param name="SourceDoc">源文档</param>
        /// <param name="TargetBookMark">目标书签</param>
        /// <param name="SourceBookMark">源title书签</param>
        /// <param name="Title">标题内容</param>
        public void CopyBookMarks(Document TargetDoc, Document SourceDoc, string TargetBookMark, string SourceBookMark, string Title)
        {
            Bookmark Smark = SourceDoc.Range.Bookmarks["SourceTitle"];
            Bookmark Tmark = TargetDoc.Range.Bookmarks["TargetBookMark"];

            Smark.Text = Title;
            InsertDocument(Tmark.BookmarkStart.ParentNode, SourceDoc);
        }
        #endregion

        #region 文档节点复制
        /// <summary>
        /// Inserts content of the external document after the specified node.
        /// Section breaks and section formatting of the inserted document are ignored.
        /// </summary>
        /// <param name="insertAfterNode">Node in the destination document after which the content
        /// should be inserted. This node should be a block level node (paragraph or table).</param>
        /// <param name="srcDoc">The document to insert.</param>
        static void InsertDocument(Aspose.Words.Node insertAfterNode, Document srcDoc)
        {
            // Make sure that the node is either a paragraph or table.
            if ((!insertAfterNode.NodeType.Equals(Aspose.Words.NodeType.Paragraph)) &
              (!insertAfterNode.NodeType.Equals(Aspose.Words.NodeType.Table)))
            {
                throw new ArgumentException("The destination node should be either a paragraph or table.");
            }

            // We will be inserting into the parent of the destination paragraph.
            CompositeNode dstStory = insertAfterNode.ParentNode;

            // This object will be translating styles and lists during the import.
            NodeImporter importer = new NodeImporter(srcDoc, insertAfterNode.Document, ImportFormatMode.KeepSourceFormatting);

            // Loop through all sections in the source document.
            foreach (Section srcSection in srcDoc.Sections)
            {
                // Loop through all block level nodes (paragraphs and tables) in the body of the section.
                foreach (Aspose.Words.Node srcNode in srcSection.Body)
                {
                    // Let's skip the node if it is a last empty paragraph in a section.
                    if (srcNode.NodeType.Equals(Aspose.Words.NodeType.Paragraph))
                    {
                        Paragraph para = (Paragraph)srcNode;
                        if (para.IsEndOfSection && !para.HasChildNodes)
                            continue;
                    }

                    // This creates a clone of the node, suitable for insertion into the destination document.
                    Aspose.Words.Node newNode = importer.ImportNode(srcNode, true);

                    // Insert new node after the reference node.
                    dstStory.InsertAfter(newNode, insertAfterNode);
                    insertAfterNode = newNode;
                }
            }
        }
        #endregion

        //-----------------word通用方法---------------------------//

        #region 执行输出 Word 报告，ExportReportWord(string templateNamePath)

        public bool ExportReportPDF(string templateNamePath, string saveFilePath)
        {
            return ExportReport(templateNamePath, saveFilePath, SaveFormat.Pdf);
        }

        public bool ExportReportWord(string templateNamePath, string saveFilePath)
        {
            return ExportReport(templateNamePath, saveFilePath, SaveFormat.Doc);
        }

        /// <summary>
        /// 输出 Word 报告
        /// </summary>
        /// <param name="templateNamePath"></param>
        private bool ExportReport(string templateNamePath, string saveFilePath, SaveFormat saveFormat)
        {
            try
            {
                //打开模版
                Doc = new Document(templateNamePath);//主模版
                Builder = new DocumentBuilder(Doc);

                //获得表集合
                GetTables();

                //初始化章节编号
                ChapterNumber();

                #region 插入封面文字
                //Title
                DocSetBookMark("Title", Msg.GetResourceString("Report_DOC_Title"));
                //ProjectName
                this.DocSetBookMark(DocBookMark.ProjectName, thisProject.Name);
                //Current Date
                if (LangType.CurrentLanguage == LangType.SPANISH)
                {
                    this.DocSetBookMark(DocBookMark.CurrentDate, System.DateTime.Now.ToString("dd-MM-yyyy"));
                }
                else
                {
                    string strDateTime = GetDateTimeStr(DateTime.Now);
                    this.DocSetBookMark(DocBookMark.CurrentDate, strDateTime);
                }


                //页眉
                this.DocSetBookMark("HeadTitle", Msg.GetResourceString("Report_DOC_Head"));

                //页脚
                if (LangType.CurrentLanguage == LangType.SPANISH)
                {
                    DocSetBookMark("FootName", thisProject.Name + " " + System.DateTime.Now.ToString("dd-MM-yyyy"));
                }
                else
                {
                    DocSetBookMark("FootName", thisProject.Name + " " + System.DateTime.Now.ToString("yyyy-MM-dd"));
                }
                //DocSetBookMark("FootPage", Msg.GetResourceString("Report_DOC_Page"));
                //DocSetBookMark("FootTotalPage", Msg.GetResourceString("Report_DOC_TotalPage"));

                //目录
                DocSetBookMark("Contents", Msg.GetResourceString("Report_DOC_Contents"));

                #endregion

                Builder.MoveToBookmark("LogoImg");
                if (this.thisProject.BrandCode == "H")
                {
                    Builder.InsertImage(JCHVRF.Report.Properties.Resources.Logo_Hitachi_2, 120, 40);
                }
                else
                {
                    Builder.InsertImage(JCHVRF.Report.Properties.Resources.York_VRF_color_2, 120, 40);
                }

                #region 填充对应模块数据
                // 填充Room Information中的数据
                FillRoomInformation();

                // 填充Indoor/Outdoor Selection中的数据
                FillInOutSelection();

                // 向System Drawing中插入每个系统图片和填充对应的数据
                //FillSystemDrawing();

                // 向System Wiring中插入每个系统图的Wiring Layout
                // FillSystemWiring();

                // 向Controller中插入控制器系统的布局图
                // FillController();

                // 填充ProductList中的数据
                // FillProductList();

                //填充Pricing Form中的数据
                //FillPricingForm(ref activeDocument, ref selection);


                ////5产品功能、特点及优势
                //this.DocSetBookMark("Chapter5", Msg.GetResourceString("Report_DOC_Chapter5"));//Product Functions, Features & Benefits
                //this.DocSetBookMark("Des", Msg.GetResourceString("Report_DOC_Des"));//Des
                //this.DocSetBookMark("FeaturesofOutdoorUnits", Msg.GetResourceString("Report_DOC_Benefits_FeaturesofOutdoorUnits"));//FeaturesofOutdoorUnits
                //this.DocSetBookMark("TypesofIndoorUnits", Msg.GetResourceString("Report_DOC_TypesofIndoorUnits"));//TypesofIndoorUnits
                //this.DocSetBookMark("FeaturesofOutdoorUnitsContent", Msg.GetResourceString("Report_DOC_FeaturesofOutdoorUnitsContent"));//FeaturesofOutdoorUnitsContent
                //this.DocSetBookMark("TypesofIndoorUnitsContent", Msg.GetResourceString("Report_DOC_TypesofIndoorUnitsContent"));//TypesofIndoorUnitsContent

                ////修改章节编号
                //Builder.MoveToBookmark("Chapter5");
                //Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0);
                //Builder.MoveToBookmark("FeaturesofOutdoorUnits");
                //Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                //Builder.CurrentParagraph.Runs[1].Text = "";
                //Builder.MoveToBookmark("TypesofIndoorUnits");
                //Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                //Builder.CurrentParagraph.Runs[1].Text = "";

                ////6公司简介
                //this.DocSetBookMark("Chapter6", Msg.GetResourceString("Report_DOC_Chapter6"));//Company Briefing
                //this.DocSetBookMark("CompanyBriefing", Msg.GetResourceString("Report_DOC_CompanyBriefing"));//AccessoriesList

                ////修改章节编号
                //Builder.MoveToBookmark("Chapter6");
                //Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(0);

                //删除空白的模板内容
                this.ClearEmptyContent();

                #endregion

                //更新目录
                Doc.UpdateFields();

                #region 保存到指定路径 & 询问是否需要打开报告文件

                if (Util.IsFileInUse(saveFilePath))
                {
                    JCMsg.ShowWarningOK(Msg.FILE_WARN_OPENED);
                    return false;
                }

                object filename = (object)saveFilePath;
                Doc.Save(filename.ToString(), saveFormat);

                // 询问是否需要打开报告文件
                if (JCMsg.ShowConfirmYesNoCancel(Msg.CONFIRM_REPORT_OPEN) == DialogResult.Yes)
                {
                    Builder.MoveToBookmark(DocBookMark.ProjectName);
                    string errMsg = "";
                    Util.Process_Start(saveFilePath, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        JCMsg.ShowErrorOK(errMsg);
                    }
                }
                #endregion
            }
            catch (System.Exception ex)
            {
                #region 错误提示
                object miss = System.Reflection.Missing.Value;
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                AddToLog(ex.Message);
                return false;
                #endregion
            }
            finally
            {
                #region 写入日志文件
                // WriteToLogFile(ReportLogList, MyConfig.SystemLogNamePath);
                #endregion
            }
            return true;
        }

        /// <summary>
        /// 获取相应格式的日期--封面底部（October 26, 2011）
        /// </summary>
        /// <returns>返回日期</returns>
        private string GetDateTimeStr(DateTime time)
        {
            if (CommonBLL.IsChinese())
            {
                return time.Year.ToString() + "年" + time.Month.ToString() + "月" + time.Day.ToString() + "日";
            }
            else if (CommonBLL.IsEnglish())
            {
                string StrDateTime = time.ToString("D", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                string[] arr = StrDateTime.Split(',');
                StrDateTime = arr[1].ToString().Trim();
                arr = StrDateTime.Split(' ');
                StrDateTime = arr[1] + " " + arr[0] + ", " + arr[2];
                return StrDateTime;
            }
            else
            {
                return time.ToString("yyyy-MM-dd");
            }

        }

        #endregion

        #region Word 文档写入

        /// <summary>
        /// 清除未填充模板区域
        /// </summary>
        private void ClearEmptyContent()
        {
            //删除空数据的表、行、段落
            for (int i = 0; i < DelNodeList.Count; i++)
            {
                System.Diagnostics.Trace.WriteLine(i.ToString() + "   " + DelNodeList[i].GetText());
                try
                {
                    DelNodeList[i].Remove();
                }
                catch (Exception ex)
                {
                    string err = "Error in ClearEmptyContent:\n" + ex.Message + "!\n" + ex.StackTrace;
                    AddToLog(err);
                    AddToLog("[ClearEmptyContent] 删除失败！");
                }
            }
        }

        private void AddToDelNodeList(Aspose.Words.Node node)
        {
            DelNodeList.Add(node);
        }

        #region 填充Room Information中的数据
        /// <summary>
        /// 填充Room Information中的数据
        /// </summary>
        /// <param name="activeDocument">当前文档</param>
        /// <param name="selection">当前光标所在位置</param>
        private void FillRoomInformation()
        {
            const int TABLEINDEXBASIC = 0;  // 文档中 Room Basic 表格的索引号
            const int TABLEINDEX = 1;       // 文档中 Room 数据表格的索引号
            string logInfo = "";

            if (thisProject.IsRoomInfoChecked == false)
            {
                Builder.MoveToBookmark("Chapter1");
                //DelNodeListAdd(Builder.CurrentParagraph.PreviousSibling);
                AddToDelNodeList(Builder.CurrentParagraph);
                AddToDelNodeList(Tables[TABLEINDEXBASIC]);
                AddToDelNodeList(Tables[TABLEINDEXBASIC].NextSibling);
                AddToDelNodeList(Tables[TABLEINDEX]);
                AddToDelNodeList(Tables[TABLEINDEX].NextSibling);
                return;
            }
            //修改章节编号
            Builder.MoveToBookmark("Chapter1");
            Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(0) + "、";

            //Title
            this.DocSetBookMark("Chapter1", Msg.GetResourceString("Report_DOC_Chapter1"));//Project and Room Information

            try
            {

                // 1. 填充 Room Information Baic 表中的数据 -Y
                tableindex = TABLEINDEXBASIC;
                fillRoomInformationBasic();

                // 2. 填充Room表中的数据   -Y
                // 2-1 填充标题栏单位（根据当前项目单位制）
                tableindex = TABLEINDEX;
                fillUnitsRoomInformation();
                Table table = (Table)Tables[tableindex];

                // 2-2 Obtaining the data source of the Room table
                DataTable data = GetData_RoomList().ToTable();
                int rowCount = data.Rows.Count;

                // 2-2-1 插入相应的空行
                InsertEmptyRowsToTable(rowCount + 1, table, out logInfo);

                if (rowCount >= 1)
                {

                    int rowscount = ((Table)Tables[tableindex]).Rows.Count;
                    InsertRoomDataWithSum(data, rowscount - 1, out logInfo);
                }
                else
                {
                    AddToDelNodeList(table);
                    const int ROWSTART = 3;         // 文档中 Room 表格待填充数据起始行，起始索引号1
                    SetCellText(tableindex, ROWSTART, 0, Msg.Report_Sum);
                }
            }
            catch (Exception ex)
            {
                string err = "Error in FillRoomInformation:\n" + ex.Message + "!\n" + ex.StackTrace;
                AddToLog(err);
                AddToLog("[1、Room Information] 填充失败！");
            }

        }

        /// <summary>
        /// 将数据插入到 Room 表中，20130926 clh
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        /// <param name="logInfo"></param>
        private void InsertRoomDataWithSum(DataTable data, int rowscount, out string logInfo)
        {
            const int ROWSTART = 3;         // The starting line of the data to be filled in the Room table in the document, starting index number 0
            double[] sumValueArray = { 0, 0, 0, 0, 0 };
            try
            {
                // Populate the data portion of the table line by line (except for total rows) and accumulate each total value
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow dr = data.Rows[rIndex];
                    string sensibleheat = dr[RptColName_Room.RSensibleHeatLoad].ToString();
                    string airflow = dr[RptColName_Room.RAirFlow].ToString();

                    // Populate table data column by column
                    SetCellText(tableindex, rIndex + ROWSTART, 0, dr[RptColName_Room.RoomNO].ToString());
                    SetCellText(tableindex, rIndex + ROWSTART, 1, dr[RptColName_Room.FloorNO].ToString());
                    SetCellText(tableindex, rIndex + ROWSTART, 2, dr[RptColName_Room.RoomName].ToString());
                    SetCellText(tableindex, rIndex + ROWSTART, 3, dr[RptColName_Room.RoomType].ToString());
                    SetCellText(tableindex, rIndex + ROWSTART, 4, dr[RptColName_Room.RoomArea].ToString());
                    SetCellText(tableindex, rIndex + ROWSTART, 5, dr[RptColName_Room.RLoad].ToString() + "/" + dr[RptColName_Room.RLoadHeat].ToString());
                    SetCellText(tableindex, rIndex + ROWSTART, 6, sensibleheat);
                    SetCellText(tableindex, rIndex + ROWSTART, 7, airflow);

                    // Cumulatively calculate the columns participating in the total in the current row
                    sumValueArray[0] += Convert.ToDouble(dr[RptColName_Room.RoomArea]);
                    sumValueArray[1] += Convert.ToDouble(dr[RptColName_Room.RLoad]);
                    sumValueArray[2] += Convert.ToDouble(dr[RptColName_Room.RLoadHeat]);
                    double sh, af;
                    if (double.TryParse(sensibleheat, out sh))
                        sumValueArray[3] += sh;
                    if (double.TryParse(airflow, out af))
                        sumValueArray[4] += af;
                }

                // Fill the value of the total row cell by cell
                int rowindex = rowscount;
                SetCellText(tableindex, rowindex, 0, Msg.Report_Sum);   // RoomArea
                SetCellText(tableindex, rowindex, 1, thisProject.FloorList.Count.ToString());
                SetCellText(tableindex, rowindex, 4, sumValueArray[0].ToString());   // RoomArea
                SetCellText(tableindex, rowindex, 5, sumValueArray[1].ToString() + "/" + sumValueArray[2].ToString());   // Cooling/Heating Load
                SetCellText(tableindex, rowindex, 6, (sumValueArray[3] == 0) ? "-" : sumValueArray[3].ToString());  // SensibleHeat
                SetCellText(tableindex, rowindex, 7, (sumValueArray[4] == 0) ? "-" : sumValueArray[4].ToString());   // AirFlow

                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                logInfo = "数据写入失败！";
            }
        }

        /// <summary>
        /// 填充 Room Information 表中的单位（根据当前公英制）
        /// 2012-01-06
        /// </summary>
        /// <param name="activeDocument"></param>
        private void fillUnitsRoomInformation()
        {
            //title
            //code break here
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_RoomInformation_RoomNo"));//Room No
            SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_RoomInformation_Floor"));//Floor
            //SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_RoomInformation_RoomName"));//Room Name
            SetCellText(tableindex, 0, 3, Msg.GetResourceString("Report_DOC_RoomInformation_RoomType"));//Room Type
            SetCellText(tableindex, 0, 4, Msg.GetResourceString("Report_DOC_RoomInformation_RoomArea"));//Room Area
            SetCellText(tableindex, 0, 5, Msg.GetResourceString("Report_DOC_RoomInformation_RoomACRequirement"));//Room A/C Requirement

            SetCellText(tableindex, 1, 5, Msg.GetResourceString("Report_DOC_RoomInformation_CoolingHeatingLoad"));//Cooling/HeatingLoad
            SetCellText(tableindex, 1, 6, Msg.GetResourceString("Report_DOC_RoomInformation_SensibleHeatLoad"));//SensibleHeat Load
            SetCellText(tableindex, 1, 7, Msg.GetResourceString("Report_DOC_RoomInformation_AirFlow"));//Air Flow

            //unit
            SetCellText(tableindex, 2, 4, utArea);
            SetCellText(tableindex, 2, 5, utPower);
            SetCellText(tableindex, 2, 6, utPower);
            SetCellText(tableindex, 2, 7, utAirflow);

        }

        /// <summary>
        /// 填充 Room Information Baic 表中的数据
        /// </summary>
        /// <param name="activeDocument"></param>
        private void fillRoomInformationBasic()
        {
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_BasicInformation"));
            SetCellText(tableindex, 1, 0, ShowText.ProjectName + ": " + thisProject.Name);
            SetCellText(tableindex, 1, 1, ShowText.Location + ": " + thisProject.Location);
            SetCellText(tableindex, 2, 0, ShowText.SoldTo + ": " + thisProject.SoldTo);
            SetCellText(tableindex, 2, 1, ShowText.RegionVRF + ": " + thisProject.SubRegionCode);
            SetCellText(tableindex, 3, 0, ShowText.SalesEngineerName + ": " + thisProject.SalesEngineer);
            SetCellText(tableindex, 3, 1, ShowText.SelectionMode + ": " + getSelectionMode());
        }
        #endregion

        #region 填充Indoor/Outdoor Selection中的数据
        /// <summary>
        /// 填充Indoor/Outdoor Selection中的数据
        /// </summary>
        /// <param name="activeDocument">当前文档</param>
        /// <param name="selection">当前光标所在位置</param>
        private void FillInOutSelection()
        {
            //working data we are not getting 
            const int TABLEINDEXBASIC = 2;            // Basic 表格的索引
            const int TABLEINDEXIN = 3;                 // Indoor Index of the form
            const int TABLEINDEXFA = 4;                 // FreshAir 表格的索引
            const int TABLEINDEXOUT = 5;            // Outdoor 表格的索引

            TableSum = TABLEINDEXBASIC - 1;
            try
            {
                if (thisProject.IsOutdoorListChecked == false && thisProject.IsIndoorListChecked == false)
                {
                    Builder.MoveToBookmark("Chapter2");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Tables[TABLEINDEXBASIC]);
                    AddToDelNodeList(Tables[TABLEINDEXBASIC].NextSibling);
                    return;
                }

                //修改章节编号
                Builder.MoveToBookmark("Chapter2");
                Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0);

                //Title
                this.DocSetBookMark("Chapter2", Msg.GetResourceString("Report_DOC_Chapter2"));//Indoor/Outdoor Selection

                // 1、填充Baic Information表中的数据--OK
                tableindex = TABLEINDEXBASIC;
                //fillInOutSelectionBasic();

                // 2、填充 Indoor/Outdoor表
                Document DocDoor;
                // DocDoor = new Document(MyConfig.ReportTemplateDirectory + @"DoorSelection.doc");
                //string dir = GetBinDirectoryPath(MyConfig.ReportTemplateDirectory);
                // string dir= ConfigurationManager.AppSettings["ReportTemplateDirectory"];
                string reportDoorPath = GetReportPathDoor();
                DocDoor = new Document(reportDoorPath);
                string logInfo = string.Empty;
                //if (thisProject.SystemList.Count > 0)
                //{
                for (int i = thisProject.SystemListNextGen.Count - 1; i >= 0; i--)
                {
                    //if (!valSystemExport(thisProject.SystemListNextGen[i]))
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[i];
                    //IndoorTemplate setting
                    Bookmark Smark = DocDoor.Range.Bookmarks["title"];//Chapter title
                    string series;
                    //thisProject.SystemListNextGen).Items[0].Series
                    if (thisProject.SystemListNextGen[0].Series == null)

                    {
                        MyProductType pt = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, sysItem.OutdoorItem.ProductType);
                        series = pt.Series;
                    }
                    else
                    {
                        series = thisProject.SystemListNextGen[0].Series;
                    }
                    series = trans.getTypeTransStr(TransType.Series.ToString(), series);
                    //add by axj 添加静压的提示 20170710
                    List<RoomIndoor> list = bll.GetSelectedIndoorBySystem(sysItem.Id, IndoorType.Indoor);
                    bool isShowStaticPressureNotes = false;
                    foreach (var ri in list)
                    {
                        if (ri.IndoorItem.Type == "High Static Ducted (NA)")
                        {
                            isShowStaticPressureNotes = true;
                            break;
                        }
                    }
                    //Smark.Text = ChapterNumber(1, i + 1) + " " + thisProject.SystemListNextGen[i].Name + " [" + series + "]";
                    //Smark.Text = ChapterNumber(1, i + 1) + " " + thisProject.SystemList[i].Name + " [" + series + "]";
                    Smark.Text = ChapterNumber(1, i + 1) + " " + thisProject.SystemListNextGen[i].Name + " [" + trans.getTypeTransStr(TransType.Series.ToString(), series) + "]";
                    Smark = DocDoor.Range.Bookmarks["VRFIndoorUnitSelection"];//VRFIndoorUnitSelection
                    Smark.Text = Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_VRFIndoorUnitSelection");
                    Smark = DocDoor.Range.Bookmarks["VRFIndoorUnitSelection_Messgae"];
                    Smark.Text = "*" + Msg.GetResourceString("Report_DOC_VRFIndoorUnitSelection_SHF_Message");
                    Smark.Text += "\r\n*";
                    Smark.Text += Msg.GetResourceString("Report_DOC_VRFIndoorUnitSelection_AirFlow_Message");
                    //if (isShowStaticPressureNotes)
                    //{
                    //    Smark.Text += "\r\n*[High Static Ducted (NA)]";
                    //    Smark.Text += Msg.GetResourceString("Report_DOC_VRFIndoorUnitSelection_StaticPressure_Message");
                    //}
                    Smark = DocDoor.Range.Bookmarks["FreshAirIndoorUnitSelection"];//FreshAirIndoorUnitSelection
                    Smark.Text = Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_FreshAirIndoorUnitSelection");
                    Smark = DocDoor.Range.Bookmarks["OutdoorUnitSelection"];//OutdoorUnitSelection
                    Smark.Text = Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_OutdoorUnitSelection");
                    Smark = DocDoor.Range.Bookmarks["IndoorOutdoorSelection_Waring"];//备注
                                                                                     //隐藏* Please manually enter each pipe parameters to calculate the amount of refrigerant charging!
                                                                                     // modify on 20160606 by Yunxiao Lin
                                                                                     //Smark.Text = Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Waring");
                    Smark.Text = "";
                    Smark = DocDoor.Range.Bookmarks["IndoorOutdoorSelection_Msg"];
                    Smark.Text = "";
                    Smark = DocDoor.Range.Bookmarks["SystemBasic"];
                    Smark.Text = Msg.GetResourceString("Report_DOC_SystemBasic"); ;
                    foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                    {
                        //if (ri.SystemID == sysItem.Id)
                        //{
                        //    if (!CommonBLL.FullMeetRoomRequired(ri, thisProject))
                        //    {
                        //        Smark.Text = "* : " + Msg.GetResourceString("IND_CAPACITY_MSG");
                        //        break;
                        //    }
                        //}
                    }
                    //Smark = DocDoor.Range.Bookmarks["IndoorOutdoorSelection_Msg"];
                    //Smark.Text = "* : " + Msg.GetResourceString("IND_CAPACITY_MSG");
                    Bookmark Tmark = Doc.Range.Bookmarks["copyTable"];//Main document bookmark
                    InsertDocument(Tmark.BookmarkStart.ParentNode, DocDoor);//Form copy
                    GetTables();
                    TableSum++;
                    fillInOutSelectionBasic(TABLEINDEXBASIC, sysItem);

                    //Indoor
                    TableSum++;
                    // title
                    SetCellText(TABLEINDEXIN, 0, 0, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_RoomNo"));//Room No
                    SetCellText(TABLEINDEXIN, 0, 1, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_RoomName"));//Room Name
                    SetCellText(TABLEINDEXIN, 0, 2, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_RoomACRequirement"));//Room A/C Requirement
                    SetCellText(TABLEINDEXIN, 0, 3, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SelectedIndoorUnitPerformance"));//Selected Indoor Unit Performance
                    SetCellText(TABLEINDEXIN, 0, 4, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Remarks"));//Remarks
                    SetCellText(TABLEINDEXIN, 1, 2, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_CoolingDB_WB"));//add axj 20170621
                    SetCellText(TABLEINDEXIN, 1, 3, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_HeatingDB"));//add axj 20170621
                    SetCellText(TABLEINDEXIN, 1, 4, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_CoolingHeatingLoad"));//Cooling/HeatingLoad
                    SetCellText(TABLEINDEXIN, 1, 5, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SensibleHeatLoad"));//SensibleHeatLoad
                    SetCellText(TABLEINDEXIN, 1, 6, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_AirFlow"));//AirFlow
                    SetCellText(TABLEINDEXIN, 1, 7, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_StaticPressure"));//static pressure
                    SetCellText(TABLEINDEXIN, 1, 8, Msg.GetResourceString("Report_IndoorName"));//IndoorName
                    SetCellText(TABLEINDEXIN, 1, 9, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Model"));//Model
                    SetCellText(TABLEINDEXIN, 1, 10, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Qty"));//Qty
                    SetCellText(TABLEINDEXIN, 1, 11, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_ActualCoolingHeatingCapacity"));//Actual Cooling/HeatingCapacity
                                                                                                                                              //SetCellText(TABLEINDEXIN, 1, 8, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_ActualSensibleHeatCapacity"));//ActualSensibleHeatCapacity
                    SetCellText(TABLEINDEXIN, 1, 12, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_ActualSensibleCoolingCapacity"));//Changed to new demandActualSensibleCoolingCapacity
                                                                                                                                               //SetCellText(TABLEINDEXIN, 1, 13, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_AirFlow"));//AirFlow
                                                                                                                                               //SetCellText(TABLEINDEXIN, 1, 14, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_StaticPressure"));//static pressure

                    //unit
                    SetCellText(TABLEINDEXIN, 2, 2, utTemperature);
                    SetCellText(TABLEINDEXIN, 2, 3, utTemperature);
                    SetCellText(TABLEINDEXIN, 2, 4, utPower);
                    SetCellText(TABLEINDEXIN, 2, 5, utPower);
                    SetCellText(TABLEINDEXIN, 2, 6, utAirflow);
                    SetCellText(TABLEINDEXIN, 2, 7, utPressure);
                    SetCellText(TABLEINDEXIN, 2, 11, utPower);
                    SetCellText(TABLEINDEXIN, 2, 12, utPower);
                    //SetCellText(TABLEINDEXIN, 2, 13, utAirflow);
                    SetCellText(TABLEINDEXIN, 2, 14, utPressure);
                    //Data input
                    DataTable data = null;
                    int rowCount = 0;
                    Table table = (Table)Tables[TABLEINDEXIN];

                    if (thisProject.IsIndoorListChecked == true)
                    {
                        data = GetData_InSelectionList(sysItem).ToTable();
                        rowCount = data.Rows.Count;
                        if (rowCount > 1)
                        {
                            // 1-1 Insert the corresponding blank line
                            InsertEmptyRowsToTable(rowCount, table, 3, out logInfo);
                            // AddToLog("[2、Indoor Selection]" + logInfo);
                        }
                        if (rowCount > 0)
                        {
                            // 1-2 插入相应的数据
                            // update on 20130926 clh 单独处理，因为其中存在 Cooling/Heating 的数据列
                            InsertIndoorSelectionDataWithSum(TABLEINDEXIN, 3, ref data, ref logInfo);
                        }
                        else
                        {
                            AddToDelNodeList(table);
                        }
                    }
                    else
                    {
                        AddToDelNodeList(table);
                    }

                    //FreshAir
                    //标题
                    SetCellText(TABLEINDEXFA, 0, 0, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_RoomNo"));//Room No
                    SetCellText(TABLEINDEXFA, 0, 1, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_RoomName"));//Room Name
                    SetCellText(TABLEINDEXFA, 0, 2, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_RoomFARequirement"));//Room F/A Requirement
                    SetCellText(TABLEINDEXFA, 0, 3, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SelectedFAIndoorUnitPerformance"));//Selected F/A Indoor Unit Performance
                    SetCellText(TABLEINDEXFA, 0, 4, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Remarks"));//Remarks
                    SetCellText(TABLEINDEXFA, 1, 2, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_NoofPersons"));//No.ofPersons
                    SetCellText(TABLEINDEXFA, 1, 3, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_CoolingDB_WB"));//add by axj 20170621
                    SetCellText(TABLEINDEXFA, 1, 4, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_HeatingDB"));//add by axj 20170621
                    SetCellText(TABLEINDEXFA, 1, 5, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_FAIndex"));//F/A Index
                    SetCellText(TABLEINDEXFA, 1, 6, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_FreshAir"));//Fresh Air
                    SetCellText(TABLEINDEXFA, 1, 7, Msg.GetResourceString("Report_IndoorName"));//IndoorName
                    SetCellText(TABLEINDEXFA, 1, 8, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Model"));//Model
                    SetCellText(TABLEINDEXFA, 1, 9, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_Qty"));//Qty
                    SetCellText(TABLEINDEXFA, 1, 10, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_FreshAirActualCoolingHeatingCapacity"));//ActualCooling/HeatingCapacity
                    SetCellText(TABLEINDEXFA, 1, 11, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_AirFlow"));//Air Flow


                    //单位
                    TableSum++;
                    SetCellText(TABLEINDEXFA, 2, 3, utTemperature);
                    SetCellText(TABLEINDEXFA, 2, 4, utTemperature);
                    SetCellText(TABLEINDEXFA, 2, 5, utAirflow + " per person");
                    SetCellText(TABLEINDEXFA, 2, 6, utAirflow);
                    SetCellText(TABLEINDEXFA, 2, 10, utPower);
                    SetCellText(TABLEINDEXFA, 2, 11, utAirflow);

                    table = (Table)Tables[TABLEINDEXFA];
                    data = GetData_InSelectionListFA(sysItem).ToTable();
                    rowCount = data.Rows.Count;
                    if (rowCount > 1)
                    {
                        // 1-1 插入相应的空行
                        InsertEmptyRowsToTable(rowCount, table, 3, out logInfo);
                        AddToLog("[2、Indoor Selection]" + logInfo);
                    }
                    if (rowCount > 0)
                    {
                        // 1-2 插入相应的数据
                        InsertIndoorSelectionDataWithSumFA(TABLEINDEXFA, 3, ref data, ref logInfo);
                    }
                    else
                    {
                        //删除新风机表格
                        AddToDelNodeList(table);
                        Builder.MoveToBookmark("FreshAirIndoorUnitSelection");
                        Builder.CurrentParagraph.Runs.Clear();
                        Builder.MoveToBookmark("OutdoorUnitSelection");
                        Builder.CurrentParagraph.Runs[1].Text = "2";
                    }

                    //Outdoor
                    TableSum++;
                    //标题
                    SetCellText(TABLEINDEXOUT, 0, 0, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_OutdoorUnitModel"));//Outdoor Unit Model
                    if (sysItem.OutdoorItem.ProductType.Contains("Water Source"))
                    {
                        SetCellText(TABLEINDEXOUT, 0, 1, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_IWHeating"));//add by axj 20170621
                        SetCellText(TABLEINDEXOUT, 0, 2, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_IWCooling"));//add by axj 20170621
                    }
                    else
                    {
                        SetCellText(TABLEINDEXOUT, 0, 1, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_HeatingDB_WB"));//add by axj 20170621
                        SetCellText(TABLEINDEXOUT, 0, 2, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_CoolingDB"));//add by axj 20170621
                    }
                    SetCellText(TABLEINDEXOUT, 0, 3, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_ActualCoolingHeatingCapacity"));//Actual Cooling/HeatingCapacity
                    SetCellText(TABLEINDEXOUT, 0, 4, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_AdditionalRefrigerantCharge"));//Additional Refrigerant Charge
                    SetCellText(TABLEINDEXOUT, 0, 5, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_CapacityRatio"));//Capacity Ratio

                    //单位
                    SetCellText(TABLEINDEXOUT, 1, 1, utTemperature);
                    SetCellText(TABLEINDEXOUT, 1, 2, utTemperature);
                    SetCellText(TABLEINDEXOUT, 1, 3, utPower);
                    SetCellText(TABLEINDEXOUT, 1, 4, utWeight);

                    //数据
                    table = (Table)Tables[TABLEINDEXOUT];
                    if (thisProject.IsOutdoorListChecked == true)
                    {
                        //data = GetData_OutSelection(sysItem).ToTable();//comment because prerequisit outdoor information we are not getting


                        DataRow dataRow = data.Rows[0];
                        SetCellText(TABLEINDEXOUT, 2, 0, dataRow[0].ToString());
                        SetCellText(TABLEINDEXOUT, 2, 1, dataRow[1].ToString());
                        SetCellText(TABLEINDEXOUT, 2, 2, dataRow[2].ToString());
                        SetCellText(TABLEINDEXOUT, 2, 3, dataRow[3].ToString() + "/" + dataRow[4].ToString());
                        SetCellText(TABLEINDEXOUT, 2, 4, dataRow[5].ToString());
                        SetCellText(TABLEINDEXOUT, 2, 5, dataRow[6].ToString());

                    }
                    else
                    {
                        AddToDelNodeList(table);
                    }
                    // }
                    //}
                }


            } //end try 
            catch (Exception ex)
            {
                string err = "Error in FillInOutSelection:\n" + ex.Message + "!\n" + ex.StackTrace;
                AddToLog(err);
                AddToLog("[2、Indoor/Outdoor Selection] 填充失败！");
            }
        }


        #region 
        public string GetReportPathDoor()
        {
            string imageFullPath = "";
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\DoorSelection.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        #endregion

        #region 

        public string GetReportPathAutoPiping()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\SystemDrawing.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        #endregion

        #region 

        public string GetReportPathWiring()
        {

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Report\\Template\\SystemWiring.doc";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        #endregion

        /// <summary>
        /// 插入 Indoor Selection 数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        /// <param name="logInfo"></param>
        private void InsertIndoorSelectionDataWithSum(int index, ref DataTable data, ref string logInfo)
        {
            const int ROWSTART = 3;         // 文档中 Room 表格待填充数据起始行，起始索引号1
            double[] sumValueArray = { 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //CoolingLoad| HeatingLoad| SensibleHeatLoad| AirFlow| Qty| CoolingCapacity| HeatingCapacity| SensibleHeatCap| AirFlow
            try
            {
                // 逐行填充表格的数据部分（合计行除外）并累计每一个合计值
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow dr = data.Rows[rIndex];
                    string sensibleheat = dr[RptColName_Room.RSensibleHeatLoad].ToString();
                    string airflow = dr[RptColName_Room.RAirFlow].ToString();

                    // 逐列填充表格数据
                    SetCellText(index, rIndex + ROWSTART, 0, dr[RptColName_Room.RoomNO].ToString());
                    SetCellText(index, rIndex + ROWSTART, 1, dr[RptColName_Room.RoomName].ToString());
                    SetCellText(index, rIndex + ROWSTART, 2, dr[RptColName_Room.RLoad].ToString() + "/" + dr[RptColName_Room.RLoadHeat].ToString());
                    SetCellText(index, rIndex + ROWSTART, 3, sensibleheat);
                    SetCellText(index, rIndex + ROWSTART, 4, airflow);
                    SetCellText(index, rIndex + ROWSTART, 5, dr[Name_Common.Model].ToString());
                    SetCellText(index, rIndex + ROWSTART, 6, dr[Name_Common.Qty].ToString());
                    SetCellText(index, rIndex + ROWSTART, 7, dr[RptColName_Unit.ActualCapacity].ToString() + "/" + dr[RptColName_Unit.ActualCapacityHeat].ToString());
                    SetCellText(index, rIndex + ROWSTART, 8, dr[RptColName_Unit.ActualSensibleCapacity].ToString());
                    SetCellText(index, rIndex + ROWSTART, 9, dr[Name_Common.AirFlow].ToString());
                    SetCellText(index, rIndex + ROWSTART, 10, dr[Name_Common.Remark].ToString());


                    // 累计计算当前行中参与合计的列
                    sumValueArray[0] += Convert.ToDouble(dr[RptColName_Room.RLoad]);
                    sumValueArray[1] += Convert.ToDouble(dr[RptColName_Room.RLoadHeat]);
                    double sh, af;
                    if (double.TryParse(sensibleheat, out sh))
                        sumValueArray[2] += sh;
                    if (double.TryParse(airflow, out af))
                        sumValueArray[3] += af;

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Qty]
                    string qty = dr[Name_Common.Qty].ToString();
                    if (qty.Contains(';'))
                    {
                        string[] arr = qty.Split(';');
                        foreach (string s in arr)
                            sumValueArray[4] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[4] += Convert.ToDouble(qty);

                    // [Cooling Capacity]
                    sumValueArray[5] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacity]);
                    // [Heating Capacity]
                    sumValueArray[6] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacityHeat]);

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Indoor Sensible Heat Capacity]
                    string actSH = dr[RptColName_Unit.ActualSensibleCapacity].ToString();
                    if (actSH.Contains(';'))
                    {
                        string[] arr = actSH.Split(';');
                        foreach (string s in arr)
                            sumValueArray[7] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[7] += Convert.ToDouble(actSH);

                    // [Indoor Air Flow]
                    string actAF = dr[Name_Common.AirFlow].ToString();
                    if (actAF.Contains(';'))
                    {
                        string[] arr = actAF.Split(';');
                        foreach (string s in arr) sumValueArray[8] += Convert.ToDouble(s);
                    }
                    else sumValueArray[8] += Convert.ToDouble(actAF);

                }

                // 逐个单元格填充合计行的值
                Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[index];
                int rowindex = table.Rows.Count - 1;
                SetCellText(index, rowindex, 0, Msg.GetResourceString("Report_Sum"));// Cooling/Heating Load
                SetCellText(index, rowindex, 2, sumValueArray[0].ToString("n1") + "/" + sumValueArray[1].ToString("n1"));// Cooling/Heating Load
                SetCellText(index, rowindex, 3, (sumValueArray[2] == 0) ? "-" : sumValueArray[2].ToString("n1"));   // Room SensibleHeat
                SetCellText(index, rowindex, 4, (sumValueArray[3] == 0) ? "-" : sumValueArray[3].ToString("n0"));   // Room AirFlow
                SetCellText(index, rowindex, 6, sumValueArray[4].ToString("n0"));   // Qty
                SetCellText(index, rowindex, 7, sumValueArray[5].ToString("n1") + "/" + sumValueArray[6].ToString("n1"));  // Cooling/Heating Capacity
                SetCellText(index, rowindex, 8, sumValueArray[6].ToString("n1"));  // Indoor SensibleHeat
                SetCellText(index, rowindex, 9, sumValueArray[8].ToString("n0"));   // Indoor AirFlow


                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                logInfo = "数据写入失败！";
            }
        }

        /// <summary>
        /// 插入 Indoor Selection 数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        /// <param name="logInfo"></param>
        private void InsertIndoorSelectionDataWithSum(int index, int start, ref DataTable data, ref string logInfo)
        {
            int ROWSTART = 3;         // 文档中 Room 表格待填充数据起始行，起始索引号1
            ROWSTART = start;
            double[] sumValueArray = { 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //CoolingLoad| HeatingLoad| SensibleHeatLoad| AirFlow| Qty| CoolingCapacity| HeatingCapacity| SensibleHeatCap| AirFlow
            try
            {
                // 逐行填充表格的数据部分（合计行除外）并累计每一个合计值
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow dr = data.Rows[rIndex];
                    string sensibleheat = dr[RptColName_Room.RSensibleHeatLoad].ToString();
                    string airflow = dr[RptColName_Room.RAirFlow].ToString();

                    // 逐列填充表格数据
                    SetCellText(index, rIndex + ROWSTART, 0, dr[RptColName_Room.RoomNO].ToString());
                    SetCellText(index, rIndex + ROWSTART, 1, dr[RptColName_Room.RoomName].ToString());
                    SetCellText(index, rIndex + ROWSTART, 2, dr[RptColName_Room.CoolingDB_WB].ToString());//add by axj 20170622
                    SetCellText(index, rIndex + ROWSTART, 3, dr[RptColName_Room.HeatingDB].ToString());//add by axj 20170622
                    SetCellText(index, rIndex + ROWSTART, 4, dr[RptColName_Room.RLoad].ToString() + "/" + dr[RptColName_Room.RLoadHeat].ToString());
                    SetCellText(index, rIndex + ROWSTART, 5, sensibleheat);
                    SetCellText(index, rIndex + ROWSTART, 6, airflow);
                    SetCellText(index, rIndex + ROWSTART, 7, dr[RptColName_Room.RStaticPressure].ToString());
                    SetCellText(index, rIndex + ROWSTART, 8, dr[RptColName_Unit.IndoorName].ToString());
                    SetCellText(index, rIndex + ROWSTART, 9, dr[Name_Common.Model].ToString());
                    SetCellText(index, rIndex + ROWSTART, 10, dr[Name_Common.Qty].ToString());
                    SetCellText(index, rIndex + ROWSTART, 11, dr[RptColName_Unit.ActualCapacity].ToString() + "/" + dr[RptColName_Unit.ActualCapacityHeat].ToString());
                    SetCellText(index, rIndex + ROWSTART, 12, dr[RptColName_Unit.ActualSensibleCapacity].ToString());
                    SetCellText(index, rIndex + ROWSTART, 13, dr[Name_Common.AirFlow].ToString());
                    SetCellText(index, rIndex + ROWSTART, 14, dr[RptColName_Unit.StaticPressure].ToString());
                    SetCellText(index, rIndex + ROWSTART, 15, dr[Name_Common.Remark].ToString());


                    // 累计计算当前行中参与合计的列
                    sumValueArray[0] += Convert.ToDouble(dr[RptColName_Room.RLoad]);
                    sumValueArray[1] += Convert.ToDouble(dr[RptColName_Room.RLoadHeat]);
                    double sh, af;
                    if (double.TryParse(sensibleheat, out sh))
                        sumValueArray[2] += sh;
                    if (double.TryParse(airflow, out af))
                        sumValueArray[3] += af;

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Qty]
                    string qty = dr[Name_Common.Qty].ToString();
                    if (qty.Contains(';'))
                    {
                        string[] arr = qty.Split(';');
                        foreach (string s in arr)
                            sumValueArray[4] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[4] += Convert.ToDouble(qty);

                    // [Cooling Capacity]
                    sumValueArray[5] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacity]);
                    // [Heating Capacity]
                    sumValueArray[6] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacityHeat]);

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Indoor Sensible Heat Capacity]
                    string actSH = dr[RptColName_Unit.ActualSensibleCapacity].ToString();
                    if (actSH.Contains(';'))
                    {
                        string[] arr = actSH.Split(';');
                        foreach (string s in arr)
                            sumValueArray[7] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[7] += Convert.ToDouble(actSH);

                    // [Indoor Air Flow]
                    string actAF = dr[Name_Common.AirFlow].ToString();
                    if (actAF.Contains(';'))
                    {
                        string[] arr = actAF.Split(';');
                        foreach (string s in arr) sumValueArray[8] += Convert.ToDouble(s);
                    }
                    else sumValueArray[8] += Convert.ToDouble(actAF);

                }

                // 逐个单元格填充合计行的值
                Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[index];
                int rowindex = start + data.Rows.Count; //table.Rows.Count - 1;
                SetCellText(index, rowindex, 0, Msg.GetResourceString("Report_Sum"));// Cooling/Heating Load
                SetCellText(index, rowindex, 4, sumValueArray[0].ToString("n1") + "/" + sumValueArray[1].ToString("n1"));// Cooling/Heating Load
                SetCellText(index, rowindex, 5, (sumValueArray[2] == 0) ? "-" : sumValueArray[2].ToString("n1"));   // Room SensibleHeat
                SetCellText(index, rowindex, 6, (sumValueArray[3] == 0) ? "-" : sumValueArray[3].ToString("n0"));   // Room AirFlow
                SetCellText(index, rowindex, 10, sumValueArray[4].ToString("n0"));   // Qty
                SetCellText(index, rowindex, 11, sumValueArray[5].ToString("n1") + "/" + sumValueArray[6].ToString("n1"));  // Cooling/Heating Capacity
                SetCellText(index, rowindex, 12, sumValueArray[6].ToString("n1"));  // Indoor SensibleHeat
                SetCellText(index, rowindex, 13, sumValueArray[8].ToString("n0"));   // Indoor AirFlow
                for (int i = 0; i < table.Rows[rowindex].Cells.Count; i++)
                {
                    table.Rows[rowindex].Cells[i].CellFormat.Borders[BorderType.Left].Color = System.Drawing.Color.Transparent;
                    table.Rows[rowindex].Cells[i].CellFormat.Borders[BorderType.Bottom].Color = System.Drawing.Color.Transparent;
                    table.Rows[rowindex].Cells[i].CellFormat.Borders[BorderType.Right].Color = System.Drawing.Color.Transparent;
                    table.Rows[rowindex].Cells[i].CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White;
                }

                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                //JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                logInfo = "数据写入失败！";
            }
        }

        /// <summary>
        /// 插入 Fresh Air Selection 数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        /// <param name="logInfo"></param>
        private void InsertIndoorSelectionDataWithSumFA(int index, ref DataTable data, ref string logInfo)
        {
            const int ROWSTART = 3;         // 文档中 Room 表格待填充数据起始行，起始索引号1
            double[] sumValueArray = { 0, 0, 0, 0, 0 }; // RqFreshAir| Qty| CoolingCapacity| HeatingCapacity| AirFlow
            try
            {
                // 逐行填充表格的数据部分（合计行除外）并累计每一个合计值
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow dr = data.Rows[rIndex];
                    string freshair = dr[RptColName_Room.FreshAir].ToString();

                    // 逐列填充表格数据
                    SetCellText(index, rIndex + ROWSTART, 0, dr[RptColName_Room.RoomNO].ToString());
                    SetCellText(index, rIndex + ROWSTART, 1, dr[RptColName_Room.RoomName].ToString());
                    SetCellText(index, rIndex + ROWSTART, 2, dr[RptColName_Room.NoOfPerson].ToString());
                    SetCellText(index, rIndex + ROWSTART, 3, dr[RptColName_Room.FAIndex].ToString());
                    SetCellText(index, rIndex + ROWSTART, 4, freshair);
                    SetCellText(index, rIndex + ROWSTART, 5, dr[Name_Common.Model].ToString());
                    SetCellText(index, rIndex + ROWSTART, 6, dr[Name_Common.Qty].ToString());
                    SetCellText(index, rIndex + ROWSTART, 7, dr[RptColName_Unit.ActualCapacity].ToString() + "/" + dr[RptColName_Unit.ActualCapacityHeat].ToString());
                    SetCellText(index, rIndex + ROWSTART, 8, dr[Name_Common.AirFlow].ToString());
                    SetCellText(index, rIndex + ROWSTART, 9, dr[Name_Common.Remark].ToString());

                    // 累计计算当前行中参与合计的列
                    // [Fresh Air]
                    double af;
                    if (double.TryParse(freshair, out af))
                        sumValueArray[0] += af;

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Qty]
                    string qty = dr[Name_Common.Qty].ToString();
                    if (qty.Contains(';'))
                    {
                        string[] arr = qty.Split(';');
                        foreach (string s in arr)
                            sumValueArray[1] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[1] += Convert.ToDouble(qty);

                    // [Cooling Capacity]
                    sumValueArray[2] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacity]);
                    // [Heating Capacity]
                    sumValueArray[3] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacityHeat]);

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Air Flow]
                    string actAF = dr[Name_Common.AirFlow].ToString();
                    if (actAF.Contains(';'))
                    {
                        string[] arr = actAF.Split(';');
                        foreach (string s in arr)
                            sumValueArray[4] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[4] += Convert.ToDouble(actAF);
                }

                // 逐个单元格填充合计行的值
                //Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[index];
                //int rowindex = table.Rows.Count;
                //SetCellText(index, rowindex, 3, sumValueArray[0].ToString("n0"));   // [Fresh Air]
                //SetCellText(index, rowindex, 6, sumValueArray[1].ToString("n0"));   // [Qty]
                //SetCellText(index, rowindex, 7, sumValueArray[2].ToString("n1") + "/" + sumValueArray[3].ToString("n1"));   // [Cooling/Heating Capacity]
                //SetCellText(index, rowindex, 8, sumValueArray[4].ToString("n0"));   // [Air Flow]
                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                logInfo = "数据写入失败！";
            }
        }

        /// <summary>
        /// 插入 Fresh Air Selection 数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        /// <param name="logInfo"></param>
        private void InsertIndoorSelectionDataWithSumFA(int index, int start, ref DataTable data, ref string logInfo)
        {
            int ROWSTART = 3;         // 文档中 Room 表格待填充数据起始行，起始索引号1
            ROWSTART = start;
            double[] sumValueArray = { 0, 0, 0, 0, 0 }; // RqFreshAir| Qty| CoolingCapacity| HeatingCapacity| AirFlow
            try
            {
                // 逐行填充表格的数据部分（合计行除外）并累计每一个合计值
                for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
                {
                    DataRow dr = data.Rows[rIndex];
                    string freshair = dr[RptColName_Room.FreshAir].ToString();

                    // 逐列填充表格数据
                    SetCellText(index, rIndex + ROWSTART, 0, dr[RptColName_Room.RoomNO].ToString());
                    SetCellText(index, rIndex + ROWSTART, 1, dr[RptColName_Room.RoomName].ToString());
                    SetCellText(index, rIndex + ROWSTART, 2, dr[RptColName_Room.NoOfPerson].ToString());
                    SetCellText(index, rIndex + ROWSTART, 3, dr[RptColName_Room.CoolingDB_WB].ToString());
                    SetCellText(index, rIndex + ROWSTART, 4, dr[RptColName_Room.HeatingDB].ToString());
                    SetCellText(index, rIndex + ROWSTART, 5, dr[RptColName_Room.FAIndex].ToString());
                    SetCellText(index, rIndex + ROWSTART, 6, freshair);
                    SetCellText(index, rIndex + ROWSTART, 7, dr[RptColName_Unit.IndoorName].ToString());
                    SetCellText(index, rIndex + ROWSTART, 8, dr[Name_Common.Model].ToString());
                    SetCellText(index, rIndex + ROWSTART, 9, dr[Name_Common.Qty].ToString());
                    SetCellText(index, rIndex + ROWSTART, 10, dr[RptColName_Unit.ActualCapacity].ToString() + "/" + dr[RptColName_Unit.ActualCapacityHeat].ToString());
                    SetCellText(index, rIndex + ROWSTART, 11, dr[Name_Common.AirFlow].ToString());
                    SetCellText(index, rIndex + ROWSTART, 12, dr[Name_Common.Remark].ToString());

                    // 累计计算当前行中参与合计的列
                    // [Fresh Air]
                    double af;
                    if (double.TryParse(freshair, out af))
                        sumValueArray[0] += af;

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Qty]
                    string qty = dr[Name_Common.Qty].ToString();
                    if (qty.Contains(';'))
                    {
                        string[] arr = qty.Split(';');
                        foreach (string s in arr)
                            sumValueArray[1] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[1] += Convert.ToDouble(qty);

                    // [Cooling Capacity]
                    sumValueArray[2] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacity]);
                    // [Heating Capacity]
                    sumValueArray[3] += Convert.ToDouble(dr[RptColName_Unit.ActualCapacityHeat]);

                    // update 20130927 clh 此处需要考虑一个房间选择多台室内机的情况
                    // [Air Flow]
                    string actAF = dr[Name_Common.AirFlow].ToString();
                    if (actAF.Contains(';'))
                    {
                        string[] arr = actAF.Split(';');
                        foreach (string s in arr)
                            sumValueArray[4] += Convert.ToDouble(s);
                    }
                    else
                        sumValueArray[4] += Convert.ToDouble(actAF);
                }

                // 逐个单元格填充合计行的值
                Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[index];
                int rowindex = start + data.Rows.Count;//table.Rows.Count - 1;
                SetCellText(index, rowindex, 0, Msg.GetResourceString("Report_Sum"));
                SetCellText(index, rowindex, 6, sumValueArray[0].ToString("n0"));   // [Fresh Air]
                SetCellText(index, rowindex, 9, sumValueArray[1].ToString("n0"));   // [Qty]
                SetCellText(index, rowindex, 10, sumValueArray[2].ToString("n1") + "/" + sumValueArray[3].ToString("n1"));   // [Cooling/Heating Capacity]
                SetCellText(index, rowindex, 11, sumValueArray[4].ToString("n0"));   // [Air Flow]
                logInfo = "数据写入成功！";
            }
            catch (Exception ex)
            {
                //JCMsg.ShowErrorOK(ex.GetType().ToString() + "\n" + ex.Message);
                logInfo = "数据写入失败！";
            }
        }

        /// <summary>
        ///  填充Indoor/Outdoor Basic中的数据
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="table"></param>
        private void fillInOutSelectionBasic(int tbIndex, JCHVRF.Model.NextGen.SystemVRF system)
        {
            #region 废弃
            ////MyProductType pt = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, thisProject.ProductType);

            ////标题
            //SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_BasicInformation"));//BasicInformation
            ////SetCellText(tableindex, 1, 0, ShowText.ProductType + ": " + pt.Series);//ProductType
            ////SetCellText(tableindex, 1, 1, ShowText.SelectionMode + ": " + getSelectionMode());//SelectionMode

            //double dbCool = SystemSetting.UserSetting.defaultSetting.indoorCoolingDB;
            //double wbCool = SystemSetting.UserSetting.defaultSetting.indoorCoolingWB;
            //double dbHeat = SystemSetting.UserSetting.defaultSetting.indoorHeatingDB;


            //if (thisProject.RoomIndoorList.Count > 0)
            //{
            //    dbCool = thisProject.RoomIndoorList[0].DBCooling;
            //    wbCool = thisProject.RoomIndoorList[0].WBCooling;
            //    dbHeat = thisProject.RoomIndoorList[0].DBHeating;
            //}

            ////IndoorCoolingDB
            //SetCellText(tableindex, 1, 0, Msg.GetResourceString("Report_DOC_OutSelectionBasic_IndoorCoolingDB") + ": " + Unit.ConvertToControl(dbCool, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            ////IndoorCoolingWB
            //SetCellText(tableindex, 2, 0, Msg.GetResourceString("Report_DOC_OutSelectionBasic_IndoorCoolingWB") + ": " + Unit.ConvertToControl(wbCool, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            ////Indoor Heating DB
            //SetCellText(tableindex, 3, 0, Msg.GetResourceString("Report_DOC_OutSelectionBasic_IndoorHeatingDB") + ": " + Unit.ConvertToControl(dbHeat, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);

            //double dbCoolOutdoor = SystemSetting.UserSetting.defaultSetting.outdoorCoolingDB;
            //double dbHeatOutdoor = SystemSetting.UserSetting.defaultSetting.outdoorHeatingDB;
            //double wbHeatOutdoor = SystemSetting.UserSetting.defaultSetting.outdoorHeatingWB;

            //double iwCoolingInletWaterOutdoor = SystemSetting.UserSetting.defaultSetting.outdoorCoolingIW;
            //double iwOutdoorHeatingInletWater = SystemSetting.UserSetting.defaultSetting.outdoorHeatingIW;

            //string WaterSource = "";
            //if (thisProject.SystemList.Count > 0)
            //{
            //    //    dbCoolOutdoor = thisProject.SystemList[0].DBCooling;
            //    //    dbHeatOutdoor = thisProject.SystemList[0].DBHeating;
            //    //    wbHeatOutdoor = thisProject.SystemList[0].WBHeating;
            //    //    iwCoolingInletWaterOutdoor = thisProject.SystemList[0].IWCooling;
            //    //    iwOutdoorHeatingInletWater = thisProject.SystemList[0].IWHeating;
            //    WaterSource = thisProject.SystemList[0].OutdoorItem.ProductType;
            //}
            //bool isWaterSource = false;
            //bool isUnWaterSource = false;
            //bool UnWaterSource = false;
            ////判断是否水机与非水机
            //for (int i = 0; i < thisProject.SystemList.Count; i++)
            //{
            //    if (thisProject.SystemList[i].OutdoorItem.ProductType.Contains("Water Source"))
            //    {
            //        iwCoolingInletWaterOutdoor = thisProject.SystemList[i].IWCooling;
            //        iwOutdoorHeatingInletWater = thisProject.SystemList[i].IWHeating;
            //        isUnWaterSource = true;
            //    }
            //    else
            //    {

            //        dbCoolOutdoor = thisProject.SystemList[i].DBCooling;
            //        dbHeatOutdoor = thisProject.SystemList[i].DBHeating;
            //        wbHeatOutdoor = thisProject.SystemList[i].WBHeating;
            //        isWaterSource = true;
            //    }
            //    if (isWaterSource && isUnWaterSource)
            //    {
            //        UnWaterSource = true;
            //        break;
            //    }
            //}

            ////隐藏制冷静水温，制热静水温 on20170908 by xyj
            ////判断是否是水机
            //if (UnWaterSource)
            //{
            //    //OutdoorCoolingDB
            //    SetCellText(tableindex, 1, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorCoolingDB") + ": " + Unit.ConvertToControl(dbCoolOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //    //Outdoor Heating DB
            //    SetCellText(tableindex, 2, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingDB") + ": " + Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //    //Outdoor Heating WB
            //    SetCellText(tableindex, 3, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingWB") + ": " + Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //    //CoolingInletWater
            //    SetCellText(tableindex, 4, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_CoolingInletWater") + ": " + Unit.ConvertToControl(iwCoolingInletWaterOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //    //HeatingInletWater 
            //    SetCellText(tableindex, 5, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_HeatingInletWater") + ": " + Unit.ConvertToControl(iwOutdoorHeatingInletWater, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);

            //}
            //else
            //{

            //    if (WaterSource.Contains("Water Source"))
            //    {
            //        //OutdoorCoolingDB
            //        SetCellText(tableindex, 1, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorCoolingDB") + ": -");
            //        //Outdoor Heating DB
            //        SetCellText(tableindex, 2, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingDB") + ": -");
            //        //Outdoor Heating WB
            //        SetCellText(tableindex, 3, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingWB") + ": -");

            //        //CoolingInletWater
            //        SetCellText(tableindex, 4, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_CoolingInletWater") + ": " + Unit.ConvertToControl(iwCoolingInletWaterOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //        //HeatingInletWater 
            //        SetCellText(tableindex, 5, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_HeatingInletWater") + ": " + Unit.ConvertToControl(iwOutdoorHeatingInletWater, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //    }

            //    else
            //    {
            //        //OutdoorCoolingDB
            //        SetCellText(tableindex, 1, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorCoolingDB") + ": " + Unit.ConvertToControl(dbCoolOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //        //Outdoor Heating DB
            //        SetCellText(tableindex, 2, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingDB") + ": " + Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //        //Outdoor Heating WB
            //        SetCellText(tableindex, 3, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingWB") + ": " + Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);

            //        //CoolingInletWater
            //        SetCellText(tableindex, 4, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_CoolingInletWater") + ": -");
            //        //HeatingInletWater 
            //        SetCellText(tableindex, 5, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_HeatingInletWater") + ": -");
            //    }
            //}
            #endregion

            SetCellText(tbIndex, 0, 0, Msg.GetResourceString("Report_DOC_BasicInformation"));
            //var inds = thisProject.RoomIndoorList.FindAll((p) => p.SystemID == system.Id);
            var inds = thisProject.RoomIndoorList;
            double dbCool = inds[0].DBCooling;
            double wbCool = 9999;
            double dbHeat = 0;
            inds.ForEach((d) =>
            {
                //取室内机制冷工况的最小值
                if (wbCool > d.WBCooling)
                {
                    wbCool = d.WBCooling;
                }
                //取室内机制热工况的最大值
                if (dbHeat < d.DBHeating)
                {
                    dbHeat = d.DBHeating;
                }
            });
            //IndoorCoolingDB
            SetCellText(tbIndex, 1, 0, Msg.GetResourceString("Report_DOC_OutSelectionBasic_IndoorCoolingDB") + ": " + Unit.ConvertToControl(dbCool, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //IndoorCoolingWB
            SetCellText(tbIndex, 2, 0, Msg.GetResourceString("Report_DOC_OutSelectionBasic_IndoorCoolingWB") + ": " + Unit.ConvertToControl(wbCool, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
            //Indoor Heating DB
            SetCellText(tbIndex, 3, 0, Msg.GetResourceString("Report_DOC_OutSelectionBasic_IndoorHeatingDB") + ": " + Unit.ConvertToControl(dbHeat, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);

            if (system != null && system.OutdoorItem != null)
            {
                string WaterSource = system.OutdoorItem.ProductType;
                // string WaterSource = thisProject.SystemListNextGen[0].OutdoorItem.ProductType;

                double iwCoolingInletWaterOutdoor = system.IWCooling;
                double iwOutdoorHeatingInletWater = system.IWHeating;
                double dbCoolOutdoor = system.DBCooling;
                double dbHeatOutdoor = system.DBHeating;
                double wbHeatOutdoor = system.WBHeating;
                if (WaterSource.Contains("Water Source"))
                {
                    //CoolingInletWater
                    SetCellText(tableindex, 1, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_CoolingInletWater") + ": " + Unit.ConvertToControl(iwCoolingInletWaterOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
                    //HeatingInletWater 
                    SetCellText(tableindex, 2, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_HeatingInletWater") + ": " + Unit.ConvertToControl(iwOutdoorHeatingInletWater, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
                }
                else
                {
                    //OutdoorCoolingDB
                    SetCellText(tableindex, 1, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorCoolingDB") + ": " + Unit.ConvertToControl(dbCoolOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
                    //Outdoor Heating DB
                    SetCellText(tableindex, 2, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingDB") + ": " + Unit.ConvertToControl(dbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
                    //Outdoor Heating WB
                    SetCellText(tableindex, 3, 1, Msg.GetResourceString("Report_DOC_OutSelectionBasic_OutdoorHeatingWB") + ": " + Unit.ConvertToControl(wbHeatOutdoor, UnitType.TEMPERATURE, utTemperature).ToString("n1") + " " + utTemperature);
                }
            }
        }
        #endregion

        #region 向System Drawing Insert each system image and fill the corresponding data
        /// <summary>
        /// 向 System Drawing Insert each system image and fill the corresponding data
        /// </summary>
        /// <param name="activeDocument">Current document</param>
        /// <param name="selection">Current cursor position</param>
        private void FillSystemDrawing()
        {
            string logInfo = string.Empty;
            if (thisProject.IsPipingDiagramChecked == false)
            {
                Builder.MoveToBookmark("Chapter3");
                AddToDelNodeList(Builder.CurrentParagraph);
                AddToDelNodeList(Builder.CurrentParagraph.NextSibling);
                AddToDelNodeList(Builder.CurrentParagraph.NextSibling.NextSibling);
                return;
            }

            //Modify chapter number
            Builder.MoveToBookmark("Chapter3");
            Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0);
            bool isHitachi = thisProject.BrandCode == "H" ? true : false;
            PipingBLL pipBll = new PipingBLL(thisProject);
            //title
            this.DocSetBookMark("Chapter3", Msg.GetResourceString("Report_DOC_Chapter3"));//SystemDrawing
            // add on 20120524 clh System index number in which the pipe map file is stored
            List<int> listSysIndex = new List<int>();
            for (int i = thisProject.SystemListNextGen.Count - 1; i >= 0; i--)
            {
                //string dir = GetBinDirectoryPath(MyConfig.ReportTemplateDirectory);
                //string dir = ConfigurationManager.AppSettings["ReportTemplateDirectory"];
                string reportPathAutoPiping = GetReportPathAutoPiping();
                Document DocDraw = new Document(reportPathAutoPiping);
                //Document DocDraw = new Document(MyConfig.ReportTemplateDirectory + @"SystemDrawing.doc");
                listSysIndex.Add(i);
                if (valSystemExport(thisProject.SystemListNextGen[i]))
                {
                    continue;
                }
                else
                {
                    TableSum++;
                    tableindex = TableSum;
                    JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[i];

                    //Title and picture
                    Bookmark SmarkTitle = DocDraw.Range.Bookmarks["title"];//Chapter title
                    SmarkTitle.Text = ChapterNumber(1, i + 1) + " " + sysItem.Name;

                    //备注
                    SmarkTitle = DocDraw.Range.Bookmarks["mark"];

                    //如果特殊机型中高度高于50在报表中提示相应信息
                    string series = sysItem.OutdoorItem.Series;
                    // Add FSCN7B/5B by Yunxiao Lin 20190107           
                    if ((series.Contains("FSNS") || series.Contains("FSNP") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTPR-BS1") || series.Contains("FSNC7B") || series.Contains("FSNC5B")) && sysItem.HeightDiff > 50) //仅FSNS和FSNP高度高于50m需要提示 20170712 by Yunxiao Lin
                                                                                                                                                                                                                                                                                         //If the height of the special model is higher than 50, the corresponding information is prompted in the report.

                        //if((series.Contains("FSNS") || series.Contains("FSNP") || series.Contains("FSXNS") || series.Contains("FSXNP") || series.Contains("JTOH-BS1") || series.Contains("JTPR-BS1")) && sysItem.HeightDiff > 50) //仅FSNS和FSNP高度高于50m需要提示 20170712 by Yunxiao Lin
                        SmarkTitle.Text = "   " + Msg.GetResourceString("PIPING_POSITION_ODU_MSG") + "\n   " + Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SystemDrawing_Mark");
                    else
                        SmarkTitle.Text = "   " + Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SystemDrawing_Mark");

                    Bookmark SmarkPic = DocDraw.Range.Bookmarks["pic"];//Picture bookmark 9
                    string drawPath = FileLocal.GetNamePathPipingPicture(sysItem.NO.ToString());
                    DocumentBuilder ss = new DocumentBuilder(DocDraw);
                    //title
                    SetCellText(0, 0, 0, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SystemDrawing_LiquidPipe"), ss);//Liquid Pipe
                    SetCellText(0, 0, 1, Msg.GetResourceString("Report_DOC_IndoorOutdoorSelection_SystemDrawing_GasPipe"), ss);//Gas Pipe
                    //单位
                    SetCellText(0, 1, 0, ShowText.Diameter + " (" + utDimension + ")", ss);
                    SetCellText(0, 1, 1, ShowText.EQLength + " (" + utLength + ")", ss);
                    SetCellText(0, 1, 2, ShowText.Diameter + " (" + utDimension + ")", ss);
                    SetCellText(0, 1, 3, ShowText.EQLength + " (" + utLength + ")", ss);

                    //Temporary image data closure
                    if (!string.IsNullOrEmpty(drawPath))
                    {
                        Aspose.Words.Drawing.Shape shape = new Aspose.Words.Drawing.Shape(Doc, ShapeType.Image);
                        shape.AllowOverlap = false;
                        shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Left; //靠右对齐  
                        shape.ImageData.SetImage(drawPath);
                        ss.MoveToBookmark("pic");
                        System.Drawing.Image img = shape.ImageData.ToImage();
                        if (img.Width > 520)
                        {
                            double h = Convert.ToDouble(520 * img.Height / img.Width);
                            ss.InsertImage(drawPath, 520, h);
                        }
                        else
                        {
                            ss.InsertImage(drawPath);
                        }
                    }

                    //数据
                    DataTable dataSpecL = null, dataSpecG = null;
                    Table table = (Table)DocDraw.GetChildNodes(Aspose.Words.NodeType.Table, true)[0];
                    DataView dvSpecL = null;
                    dataSpecG = GetData_LinkPipeSpecG(ref sysItem, out dvSpecL).ToTable();
                    dataSpecL = dvSpecL.ToTable();
                    //// Piping Materials 计算加入ODU 的管径 on 20180601 by xyj
                    //GetData_LinkPipe(ref dataSpecG, ref dataSpecL, sysItem, pipBll.GetPipingNodeOutElement(sysItem, isHitachi));

                    int RowsAddCount = GetMaxNum(dataSpecL.Rows.Count, dataSpecG.Rows.Count);
                    if (RowsAddCount > 0)
                    {
                        // 向 Piping 表中插入空行
                        InsertEmptyRowsToTable(RowsAddCount, table, out logInfo);
                        // 向 Piping 表中填充对应的数据
                        SystemDrawingTableInsertRowsAndData(dataSpecL, dataSpecG, ref table, RowsAddCount, 0, ss);
                    }

                    Bookmark Tmark = Doc.Range.Bookmarks["pipingTable"];//图书签
                    InsertDocument(Tmark.BookmarkStart.ParentNode, DocDraw);//表格复制
                    GetTables();
                }
            }
            // add on 20120606 clh 当前项目没有配管图时
            if (listSysIndex.Count == 0)
            {
                ++TableSum;
                return;
            }
        }
        ///// <summary>
        ///// Piping Materials 计算加入ODU 的管径 on 20180601 by xyj
        ///// </summary>
        ///// <param name="dtG">气管</param>
        ///// <param name="dtL">液管</param>
        ///// <param name="system">当前系统</param>
        ///// <param name="outNodeItem">室外机组合类</param>
        //private void GetData_LinkPipe(ref DataTable dtG, ref DataTable dtL, SystemVRF system, NodeElement_Piping outNodeItem)
        //{
        //    //如果当前系统 未设置管长 直接返回
        //    if (system.MyPipingNodeOut.PipeLengthes == null)
        //        return;

        //    PointF ptText = new PointF();
        //    for (int i = 0; i < outNodeItem.PtPipeDiameter.Count; ++i)
        //    {
        //        ptText = UtilEMF.OffsetLocation(outNodeItem.PtPipeDiameter[i], system.MyPipingNodeOut.Location);
        //        string s = outNodeItem.PipeSize[i];
        //        string[] aa = s.Split('x');
        //        double upper = 0;       //高压气管
        //        double lower = 0;       //低压气管
        //        double liqude = 0;      //液管
        //        if (aa.Length == 2)
        //        {
        //            upper = Convert.ToDouble(aa[0]);
        //            liqude = Convert.ToDouble(aa[1]);
        //        }
        //        else if (aa.Length == 3)
        //        {
        //            lower = Convert.ToDouble(aa[0]);
        //            upper = Convert.ToDouble(aa[1]);
        //            liqude = Convert.ToDouble(aa[2]);
        //        }
        //        double value = Convert.ToDouble(system.MyPipingNodeOut.PipeLengthes[i].ToString() == "" ? 0 : system.MyPipingNodeOut.PipeLengthes[i]);
        //        if (upper > 0)
        //        {
        //            //高压管径   
        //            dtG = UpdatePipeDiameter(dtG, upper, PipeType.Gas.ToString(), value, system.Name);
        //        }
        //        if (lower > 0)
        //        {
        //            //低压管径
        //            dtG = UpdatePipeDiameter(dtG, lower, PipeType.Gas.ToString(), value, system.Name);
        //        }
        //        if (liqude > 0)
        //        {
        //            //液管
        //            dtL = UpdatePipeDiameter(dtL, liqude, PipeType.Liquid.ToString(), value, system.Name);
        //        }
        //    }

        //}

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dt">原始数据</param>
        /// <param name="pipeSpec">管径</param>
        /// <param name="pipeType">类型</param>
        /// <param name="value">值</param>
        /// <param name="sysName">室外机名称</param>
        /// <returns></returns>
        private DataTable UpdatePipeDiameter(DataTable dt, double pipeSpec, string pipeType, double value, string sysName)
        {
            if (dt.Rows.Count == 0)
                return dt;
            DataTable newDt = dt;
            //判断是否存在 
            bool isUpdate = false;
            foreach (DataRow r in newDt.Rows)
            {
                double existSpec = Convert.ToDouble(r[RptColName_PipeSpec.PipeSpec].ToString());
                if (existSpec.ToString("n2") == pipeSpec.ToString("n2") && r[RptColName_PipeSpec.PipeType].ToString() == pipeType
                    && r[RptColName_PipeSpec.SysName].ToString() == sysName)
                {
                    int qty = Int32.Parse(r[Name_Common.Qty].ToString());
                    r[Name_Common.Qty] = qty + 1;
                    r[Name_Common.Length] = (double.Parse(r[Name_Common.Length].ToString()) + value).ToString("n1");
                    r[RptColName_PipeSpec.EqLength] = (double.Parse(r[RptColName_PipeSpec.EqLength].ToString()) + value).ToString("n1");
                    isUpdate = true;
                    break;
                }
            }
            if (!isUpdate)
            {
                //若不存在重复记录，则添加新记录
                newDt.Rows.Add(pipeSpec, PipeType.Gas, value, value, value, sysName);
            }
            return newDt;
        }


        // 填充 Piping 表表头中的单位
        /// <summary>
        /// 填充 Piping 表的单位
        /// </summary>
        /// <param name="table"></param>
        private void fillUnitsSystemDrawing()
        {
            SetCellText(tableindex, 1, 0, ShowText.Diameter + " (" + utDimension + ")");
            SetCellText(tableindex, 1, 1, ShowText.EQLength + " (" + utLength + ")");
            SetCellText(tableindex, 1, 2, ShowText.Diameter + " (" + utDimension + ")");
            SetCellText(tableindex, 1, 3, ShowText.EQLength + " (" + utLength + ")");
        }


        /// <summary>
        /// 向SystemDrawing表中填充对应的数据
        /// </summary>
        /// <param name="tempTableSpecL">液管数据表</param>
        /// <param name="tempTableSpecG">气管数据表</param>
        /// <param name="table">Word文档中的表</param>
        /// <param name="RowsAddCount">Word文档中的表的空白行数</param>
        private void SystemDrawingTableInsertRowsAndData(DataTable tempTableSpecL, DataTable tempTableSpecG,
            ref Table table, int RowsAddCount, int tindex, DocumentBuilder ss)
        {
            try
            {
                PipingBLL pipBll = new PipingBLL(thisProject);
                for (int i = 0; i < RowsAddCount; i++)
                {
                    if (i < tempTableSpecL.Rows.Count)
                    {
                        DataRow dr = tempTableSpecL.Rows[i];

                        string spec = dr[0].ToString();
                        if (CommonBLL.IsDimension_inch())
                            spec = pipBll.GetPipeSize_Inch(spec);

                        // 如何判断当前是否需要输出英制形式的管径型号（分数）?此处不需要判断，直接输出
                        SetCellText(tindex, i + 2, 0, spec, ss);
                        // 第[4]列为eqLength
                        SetCellText(tindex, i + 2, 1, (dr[4].ToString() == "-") ? "-" : Unit.ConvertToControl(Convert.ToDouble(dr[4].ToString()), UnitType.LENGTH_M, utLength).ToString("n1"), ss);
                    }
                    if (i < tempTableSpecG.Rows.Count)
                    {
                        DataRow dr = tempTableSpecG.Rows[i];
                        string spec = dr[0].ToString();
                        if (CommonBLL.IsDimension_inch())
                            spec = pipBll.GetPipeSize_Inch(spec);

                        SetCellText(tindex, i + 2, 2, spec, ss);
                        // 第[4]列为eqLength
                        SetCellText(tindex, i + 2, 3, (dr[4].ToString() == "-") ? "-" : Unit.ConvertToControl(Convert.ToDouble(dr[4].ToString()), UnitType.LENGTH_M, utLength).ToString("n1"), ss);
                    }
                }
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
            }
        }

        #endregion

        #region 填充ProductList中的数据

        /// <summary>
        /// 填充ProductList中的数据
        /// </summary>
        /// <param name="activeDocument">当前文档</param>
        /// <param name="selection">当前光标所在位置</param>
        private void FillProductList()
        {
            PipingBLL pipBll = new PipingBLL(thisProject);
            Table table = null;
            try
            {
                //修改章节编号
                Builder.MoveToBookmark("Chapter4");
                Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0);

                //标题
                this.DocSetBookMark("Chapter4", Msg.GetResourceString("Report_DOC_Chapter4"));//ProductList

                tableindex = ++TableSum;
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                fillProductBasicInfo();

                // 填充Indoor Unit List表中的数据 4.1
                this.DocSetBookMark("IndoorUnitList", Msg.GetResourceString("Report_DOC_IndoorUnitList"));//IndoorUnitList
                tableindex = ++TableSum;
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                DataTable data = GetData_InProductList().ToTable();
                fillProductListInOut(ref data);

                Builder.MoveToBookmark("IndoorUnitList");
                Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                Builder.CurrentParagraph.Runs[1].Text = "";

                // 填充Outdoor Unit List表中的数据 4.2
                this.DocSetBookMark("OutdoorUnitList", Msg.GetResourceString("Report_DOC_OutdoorUnitList"));//OutdoorUnitList
                tableindex = ++TableSum;
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                data = GetData_OutProductList().ToTable();
                fillProductListInOut_Outdoor(ref data);

                //修改章节编号
                Builder.MoveToBookmark("OutdoorUnitList");
                Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                Builder.CurrentParagraph.Runs[1].Text = "";

                //填充Exchanger Unit List表中的数据

                this.DocSetBookMark("ExchangerUnitList", Msg.GetResourceString("Report_DOC_ExchangerUnitList"));//ExchangerUnitList
                tableindex = ++TableSum;
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];

                data = GetData_ExcProductList().ToTable();
                if (data == null || data.Rows.Count == 0)
                {
                    AddToDelNodeList(table);
                    Builder.MoveToBookmark("ExchangerUnitList");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                }
                else
                {
                    if (!thisProject.IsExchangerChecked)
                    {
                        AddToDelNodeList(table);
                        Builder.MoveToBookmark("ExchangerUnitList");
                        AddToDelNodeList(Builder.CurrentParagraph);
                        AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                    }
                    else
                    {
                        fillProductListExchanger(ref data);
                        Builder.MoveToBookmark("ExchangerUnitList");
                        Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                        Builder.CurrentParagraph.Runs[1].Text = "";
                    }
                }
                // 填充附件列表 4.3
                this.DocSetBookMark("AccessoriesList", Msg.GetResourceString("Report_DOC_AccessoriesList"));//AccessoriesList
                tableindex = ++TableSum;
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                data = GetOptionInformationList();
                foreach (DataRow row in data.Rows)
                {
                    FillMaterialData(dicMaterial["Accessory"], row["AccessoryModel"].ToString(), Convert.ToInt32(row["Qty"].ToString()));
                    dicMaterialDes[row["AccessoryModel"].ToString()] = row["AccessoryType"].ToString();
                }
                fillOption(ref data);
                if (data == null || data.Rows.Count == 0)
                {
                    AddToDelNodeList(table);
                    Builder.MoveToBookmark("AccessoriesList");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                }
                else
                {
                    //修改章节编号
                    Builder.MoveToBookmark("AccessoriesList");
                    Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                    Builder.CurrentParagraph.Runs[1].Text = "";
                    Builder.CurrentParagraph.Runs[2].Text = "";
                }

                //非标  4.4
                tableindex = ++TableSum;
                this.DocSetBookMark("NonstandardProductsList", Msg.GetResourceString("Report_DOC_NonstandardProductsList"));//NonstandardProductsList
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_NonstandardProductsName"));//Non-standard Products Name
                SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_Qty"));//Qty
                SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_NonstandardProductsDescription"));//Non-standard Products Description
                AddToDelNodeList(table);
                Builder.MoveToBookmark("NonstandardProductsList");
                AddToDelNodeList(Builder.CurrentParagraph);
                AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);

                //修改章节编号
                //Builder.MoveToBookmark("NonstandardProductsList");
                //Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                //Builder.CurrentParagraph.Runs[1].Text = "";

                DataTable T_PipingKitTable = pipBll.GetPipingKitTable();
                //4.5 PipingConnectionKit
                tableindex = ++TableSum;
                this.DocSetBookMark("PipingConnectionKit", Msg.GetResourceString("Report_DOC_PipingConnectionKit"));//PipingConnectionKit
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_Model"));//Report_DOC_Model
                SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_Qty"));//Qty
                fillkits(T_PipingKitTable, "PipingConnectionKit");
                if (T_PipingKitTable.Rows.Count == 0 || (T_PipingKitTable.Rows.Count > 0 && T_PipingKitTable.Select("Type='PipingConnectionKit'").Length == 0))
                {
                    AddToDelNodeList(table);
                    Builder.MoveToBookmark("PipingConnectionKit");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                }
                else
                {
                    //修改章节编号
                    Builder.MoveToBookmark("PipingConnectionKit");
                    Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                    Builder.CurrentParagraph.Runs[1].Text = "";
                }

                //4.6 BranchKit
                tableindex = ++TableSum;
                this.DocSetBookMark("BranchKit", Msg.GetResourceString("Report_DOC_BranchKit"));//BranchKit
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_Model"));//Report_DOC_Model
                SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_Qty"));//Qty
                fillkits(T_PipingKitTable, "BranchKit");
                if (T_PipingKitTable.Rows.Count == 0 || (T_PipingKitTable.Rows.Count > 0 && T_PipingKitTable.Select("Type='BranchKit'").Length == 0))
                {
                    AddToDelNodeList(table);
                    Builder.MoveToBookmark("BranchKit");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                }
                else
                {
                    //修改章节编号
                    Builder.MoveToBookmark("BranchKit");
                    Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                    Builder.CurrentParagraph.Runs[1].Text = "";
                }

                //4.7 CHBox
                tableindex = ++TableSum;
                this.DocSetBookMark("CHBox", Msg.GetResourceString("Report_DOC_CHBox"));//CHBox
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_Model"));//Report_DOC_Model
                SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_Qty"));//Qty
                fillkits(T_PipingKitTable, "CHBox");
                if (T_PipingKitTable.Rows.Count == 0 || (T_PipingKitTable.Rows.Count > 0 && T_PipingKitTable.Select("Type='CHBox'").Length == 0))
                {
                    AddToDelNodeList(table);
                    Builder.MoveToBookmark("CHBox");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                }
                else
                {
                    //修改章节编号
                    Builder.MoveToBookmark("CHBox");
                    Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                    Builder.CurrentParagraph.Runs[1].Text = "";
                }
                //MaterialList
                tableindex = ++TableSum;
                this.DocSetBookMark("MaterialList", Msg.GetResourceString("Report_DOC_MaterialList"));//CHBox
                table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_Model"));//Report_DOC_Model
                SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_Remark"));
                SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_Qty"));//Qty

                DataTable dtMaterial = GetMaterilList();
                string logInfo = "";
                InsertEmptyRowsToTable(dtMaterial.Rows.Count, table, out logInfo);
                if (dtMaterial.Rows.Count > 0)
                {
                    InsertDataToTable(1, 0, dtMaterial, out logInfo);
                }
                if (dtMaterial.Rows.Count == 0)
                {
                    AddToDelNodeList(table);
                    Builder.MoveToBookmark("MaterialList");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.PreviousSibling);
                }
                else
                {
                    //修改章节编号
                    Builder.MoveToBookmark("MaterialList");
                    Builder.CurrentParagraph.Runs[0].Text = ChapterNumber(1);
                    Builder.CurrentParagraph.Runs[1].Text = "";
                }
            }
            catch (Exception ex)
            {
                string err = "Error in FillProductList:\n" + ex.Message + "!\n" + ex.StackTrace;
                AddToLog(err);
                AddToLog("【6、ProductList】填充失败！");
            }

        }
        /// <summary>
        /// 获取材料列表
        /// </summary>
        /// <returns></returns>
        private DataTable GetMaterilList()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Model");
            dt.Columns.Add("Remark");
            dt.Columns.Add("Qty");

            foreach (var type in dicMaterial)
            {
                foreach (var item in type.Value)
                {
                    string remark = "";
                    if (dicMaterialDes.Keys.Contains(item.Key))
                    {
                        //remark = dicMaterialDes[item.Key];
                        remark = getMaterialRemarkTrans(type.Key, dicMaterialDes[item.Key]);
                    }
                    dt.Rows.Add(item.Key, remark, item.Value);
                }
            }
            return dt;
        }

        // 填充 Product List 的Basic表
        /// <summary>
        /// 填充 Product List 的Basic信息
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="selection"></param>
        /// <param name="table"></param>
        private void fillProductBasicInfo()
        {
            //MyProductType pt = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, thisProject.ProductType);

            //标题
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_BasicInformation"));

            //表格内容
            SetCellText(tableindex, 1, 0, ShowText.ProjectName + ": " + thisProject.Name);
            SetCellText(tableindex, 1, 1, ShowText.SalesEngineerName + ": " + thisProject.SalesEngineer);
            SetCellText(tableindex, 2, 0, ShowText.SoldTo + ": " + thisProject.SoldTo);
            SetCellText(tableindex, 2, 1, ShowText.RegionVRF + ": " + thisProject.SubRegionCode);
            SetCellText(tableindex, 3, 0, ShowText.Location + ": " + thisProject.Location);
            //SetCellText(tableindex, 3, 1, ShowText.ProductType + ": " + pt.Series);

        }

        // 填充 Indoor Product 表 
        /// <summary>
        /// 填充 Indoor Product 表
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="selection"></param>
        /// <param name="data"></param>
        private void fillProductListInOut(ref DataTable data)
        {
            const int ROWSTART = 2;     // 文档中当前表格数据起始行
            const int COLSTART = 0;     // 文档中当前表格数据起始列
            string logInfo = "";

            // 写入公英制的单位
            fillUnitsProductListInOut();

            //如果系统中有相关数据
            int rowCount = data.Rows.Count;
            Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
            InsertEmptyRowsToTable(rowCount, table, out logInfo);
            // AddToLog(logInfo);

            if (rowCount > 0)
            {
                // 方法改进：20120413 clh
                InsertDataToTable(ROWSTART, COLSTART, data, out logInfo);
            }
        }

        // 填充 Exchanger Product 表 
        /// <summary>
        /// 填充 Exchanger Product 表
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="selection"></param>
        /// <param name="data"></param>
        private void fillProductListExchanger(ref DataTable data)
        {
            const int ROWSTART = 2;     // 文档中当前表格数据起始行
            const int COLSTART = 0;     // 文档中当前表格数据起始列
            string logInfo = "";

            // 写入公英制的单位
            fillUnitsProductListExcOut();

            //如果系统中有相关数据
            int rowCount = data.Rows.Count;
            Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
            InsertEmptyRowsToTable(rowCount, table, out logInfo);
            // AddToLog(logInfo);

            if (rowCount > 0)
            {
                // 方法改进：20120413 clh
                InsertDataToTable(ROWSTART, COLSTART, data, out logInfo);
            }
        }        // 填充 Outdoor Product 表
        /// <summary>
        /// 填充 Outdoor Product 表
        /// add on 2015-06-19 clh，新增需求，室外机增加『PowerInput』『MaxOperationPI』两列，与室内机表结构需要区分
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="selection"></param>
        /// <param name="data"></param>
        private void fillProductListInOut_Outdoor(ref DataTable data)
        {
            const int ROWSTART = 2;     // 文档中当前表格数据起始行
            const int COLSTART = 0;     // 文档中当前表格数据起始列
            string logInfo = "";

            // 写入公英制的单位
            fillUnitsProductListInOut_Outdoor();

            //如果系统中有相关数据
            int rowCount = data.Rows.Count;
            Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
            InsertEmptyRowsToTable(rowCount, table, out logInfo);
            //  AddToLog(logInfo);

            if (rowCount > 0)
            {
                // 方法改进：20120413 clh
                InsertDataToTable(ROWSTART, COLSTART, data, out logInfo);
            }
        }

        // 设置文档中 Indoor Product 表格中的单位
        /// <summary>
        /// 设置文档中 Indoor Product 表格中的单位
        /// 2012-01-06 Update 设置公英制的单位
        /// 2015-06-19 clh 新增需求，室内机增加『PowerInput』列
        /// </summary>
        /// <param name="table"></param>
        private void fillUnitsProductListInOut()
        {
            //标题
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_ProductListInOut_IndoorUnitModel"));//Indoor UnitModel 
            SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_ProductListInOut_UnitType"));//UnitType
            SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_ProductListInOut_Qty"));//Qty
            SetCellText(tableindex, 0, 3, Msg.GetResourceString("Report_DOC_ProductListInOut_RatedCoolingCapacity"));//RatedCoolingCapacity
            SetCellText(tableindex, 0, 4, Msg.GetResourceString("Report_DOC_ProductListInOut_RatedHeatingCapacity"));//RatedHeatingCapacity
            SetCellText(tableindex, 0, 5, Msg.GetResourceString("Report_DOC_ProductListInOut_PI"));//PI
            SetCellText(tableindex, 0, 6, Msg.GetResourceString("Report_DOC_ProductListInOut_AirFlow"));//Air Flow
            SetCellText(tableindex, 0, 7, Msg.GetResourceString("Report_DOC_ProductListInOut_Length"));//Length
            SetCellText(tableindex, 0, 8, Msg.GetResourceString("Report_DOC_ProductListInOut_Width"));//Width
            SetCellText(tableindex, 0, 9, Msg.GetResourceString("Report_DOC_ProductListInOut_High"));//High
            SetCellText(tableindex, 0, 10, Msg.GetResourceString("Report_DOC_ProductListInOut_Weight"));//Weight

            //单位
            SetCellText(tableindex, 1, 3, utPower);
            SetCellText(tableindex, 1, 4, utPower);
            SetCellText(tableindex, 1, 5, utPowerInput);
            SetCellText(tableindex, 1, 6, utAirflow);
            SetCellText(tableindex, 1, 7, utDimension);
            SetCellText(tableindex, 1, 8, utDimension);
            SetCellText(tableindex, 1, 9, utDimension);
            SetCellText(tableindex, 1, 10, utWeight);

        }

        // 设置文档中 Exchanger Product 表格中的单位
        /// <summary>
        /// 设置文档中 Exchanger Product 表格中的单位 
        /// </summary>
        /// <param name="table"></param>
        private void fillUnitsProductListExcOut()
        {
            //标题
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_ProductListInOut_ExchangerUnitModel"));//Exchanger UnitModel 
            SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_ProductListInOut_UnitType"));//UnitType
            SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_ProductListInOut_Qty"));//Qty
            SetCellText(tableindex, 0, 3, Msg.GetResourceString("Report_DOC_ProductListInOut_RatedCoolingCapacity"));//RatedCoolingCapacity
            SetCellText(tableindex, 0, 4, Msg.GetResourceString("Report_DOC_ProductListInOut_RatedHeatingCapacity"));//RatedHeatingCapacity
            SetCellText(tableindex, 0, 5, Msg.GetResourceString("Report_DOC_ProductListInOut_PI"));//PI
            SetCellText(tableindex, 0, 6, Msg.GetResourceString("Report_DOC_ProductListInOut_AirFlow"));//Air Flow
            SetCellText(tableindex, 0, 7, Msg.GetResourceString("Report_DOC_ProductListInOut_Length"));//Length
            SetCellText(tableindex, 0, 8, Msg.GetResourceString("Report_DOC_ProductListInOut_Width"));//Width
            SetCellText(tableindex, 0, 9, Msg.GetResourceString("Report_DOC_ProductListInOut_High"));//High
            SetCellText(tableindex, 0, 10, Msg.GetResourceString("Report_DOC_ProductListInOut_Weight"));//Weight

            //单位
            SetCellText(tableindex, 1, 3, utPower);
            SetCellText(tableindex, 1, 4, utPower);
            SetCellText(tableindex, 1, 5, utPowerInput);
            SetCellText(tableindex, 1, 6, utAirflow);
            SetCellText(tableindex, 1, 7, utDimension);
            SetCellText(tableindex, 1, 8, utDimension);
            SetCellText(tableindex, 1, 9, utDimension);
            SetCellText(tableindex, 1, 10, utWeight);

        }
        // 设置文档中 Outdoor Product 表格中的单位 
        /// <summary>
        /// 设置文档中 Outdoor Product 表格中的单位
        /// 2012-01-06 Update 设置公英制的单位
        /// 2015-06-19 clh 新增需求，室外机增加『PowerInput』『MaxOperationPI』两列
        /// </summary>
        /// <param name="table"></param>
        private void fillUnitsProductListInOut_Outdoor()
        {
            //标题
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_ProductListInOut_OutdoorUnitModel"));//OutdoorUnitModel
            SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_OutdoorProductType"));//ProductType
            SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_ProductListInOut_Qty"));//Qty
            SetCellText(tableindex, 0, 3, Msg.GetResourceString("Report_DOC_ProductListInOut_RatedCoolingCapacity"));//RatedCoolingCapacity
            SetCellText(tableindex, 0, 4, Msg.GetResourceString("Report_DOC_ProductListInOut_RatedHeatingCapacity"));//RatedHeatingCapacity
            SetCellText(tableindex, 0, 5, Msg.GetResourceString("Report_DOC_ProductListInOut_PI"));//PI
            SetCellText(tableindex, 0, 6, Msg.GetResourceString("Report_DOC_ProductListInOut_MaxPI"));//MaxPI
            SetCellText(tableindex, 0, 7, Msg.GetResourceString("Report_DOC_ProductListInOut_AirFlow"));//Air Flow
            SetCellText(tableindex, 0, 8, Msg.GetResourceString("Report_DOC_ProductListInOut_Length"));//Length
            SetCellText(tableindex, 0, 9, Msg.GetResourceString("Report_DOC_ProductListInOut_Width"));//Width
            SetCellText(tableindex, 0, 10, Msg.GetResourceString("Report_DOC_ProductListInOut_High"));//High
            SetCellText(tableindex, 0, 11, Msg.GetResourceString("Report_DOC_ProductListInOut_Weight"));//Weight


            //单位
            SetCellText(tableindex, 1, 3, utPower);
            SetCellText(tableindex, 1, 4, utPower);
            SetCellText(tableindex, 1, 5, utPowerInput_Outdoor);
            SetCellText(tableindex, 1, 6, utMaxOperationPI);

            SetCellText(tableindex, 1, 7, utAirflow);
            SetCellText(tableindex, 1, 8, utDimension);
            SetCellText(tableindex, 1, 9, utDimension);
            SetCellText(tableindex, 1, 10, utDimension);
            SetCellText(tableindex, 1, 11, utWeight);
        }

        // 填充附件表 4.3
        /// <summary>
        /// 填充附件表 4.3
        /// <param name="activeDocument"></param>
        /// <param name="selection"></param>
        /// <param name="table"></param>
        private void fillOption(ref DataTable data)
        {
            const int ROWSTART = 1;     // 文档中当前表格数据起始行
            const int COLSTART = 0;     // 文档中当前表格数据起始列
            string logInfo = "";

            int rowCount = data.Rows.Count;
            Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];

            //标题
            SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_Option_IndoorModel"));//IndoorModel
            SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_Option_AccessoryType"));//Accessory Type
            SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_Option_AccessoryModel"));//Accessory Model
            SetCellText(tableindex, 0, 3, Msg.GetResourceString("Report_DOC_Option_Qty"));//Qty
            InsertEmptyRowsToTable(rowCount, table, out logInfo);
            //AddToLog(logInfo);
            InsertDataToTable(ROWSTART, COLSTART, data, out logInfo);
            // AddToLog("【4、ProductList】Standard Option A List: " + logInfo);
        }


        private void fillkits(DataTable data, string type)
        {
            if (data == null || data.Rows.Count == 0)
            {
                return;
            }

            const int ROWSTART = 1;     // 文档中当前表格数据起始行
            const int COLSTART = 0;     // 文档中当前表格数据起始列
            string logInfo = "";

            DataView dv = data.Copy().DefaultView;
            dv.RowFilter = "Type='" + type + "'";
            DataTable temp = dv.ToTable();

            DataTable tb = new DataTable();
            tb.Columns.Add("Model");//型号
            tb.Columns.Add("Qty");//数量

            var querySum = from t in temp.AsEnumerable()
                           group t by t.Field<string>("Model")
                               into g
                           select new
                           {
                               _Model = g.Key,
                               _Qty = g.Count()
                           };

            foreach (var vq in querySum)
            {
                DataRow vNewRow = tb.NewRow();
                vNewRow["Model"] = vq._Model;
                vNewRow["Qty"] = vq._Qty;
                tb.Rows.Add(vNewRow);
                FillMaterialData(dicMaterial[type], vq._Model, vq._Qty);
                if (type == "PipingConnectionKit")
                {
                    dicMaterialDes[vq._Model] = "Piping connection kit";
                }
                if (type == "CHBox")
                {
                    dicMaterialDes[vq._Model] = "CH-Box";
                }
                if (type == "BranchKit")
                {
                    if (!dicMaterialDes.Keys.Contains(vq._Model))
                    {
                        dicMaterialDes[vq._Model] = "Multi kit";
                    }
                }
            }

            int rowCount = tb.Rows.Count;
            Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
            InsertEmptyRowsToTable(rowCount, table, out logInfo);
            InsertDataToTable(ROWSTART, COLSTART, tb, out logInfo);
        }


        #endregion

        #region System Wiring
        private void FillSystemWiring()
        {
            try
            {
                if (thisProject.IsWiringDiagramChecked == false)
                {
                    Builder.MoveToBookmark("ChapterWriting");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.NextSibling);
                    return;
                }

                //修改章节编号
                Builder.MoveToBookmark("ChapterWriting");
                if (Builder.CurrentParagraph.Runs[1].Text.Contains("、"))
                {
                    Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0) + "、";
                }
                else
                {
                    Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0);
                }


                //标题
                this.DocSetBookMark("ChapterWriting", Msg.GetResourceString("Report_DOC_ChapterWriting"));//Writing
                List<int> listSysIndex = new List<int>();
                for (int i = thisProject.SystemListNextGen.Count - 1; i >= 0; i--)
                {
                    //  Document DocWrite = new Document(MyConfig.ReportTemplateDirectory + @"SystemWiring.doc");
                    // string dir =  ConfigurationManager.AppSettings["ReportTemplateDirectory"];
                    string reportPathWiring = GetReportPathWiring();
                    Document DocWrite = new Document(reportPathWiring);

                    // Document DocWrite = new Document("" + @"SystemWiring.doc");
                    listSysIndex.Add(i);
                    if (valSystemExport(thisProject.SystemListNextGen[i]))
                    {
                        continue;
                    }
                    else
                    {
                        JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[i];
                        //标题及图片
                        Bookmark SmarkTitle = DocWrite.Range.Bookmarks["title"];//章节标题
                        SmarkTitle.Text = ChapterNumber(1, i + 1) + " " + sysItem.Name;

                        Bookmark SmarkPic = DocWrite.Range.Bookmarks["pic"];//图片书签
                        string drawPath = FileLocal.GetNamePathWiringPicture(sysItem.NO.ToString());
                        DocumentBuilder ss = new DocumentBuilder(DocWrite);
                        //图片数据
                        if (!string.IsNullOrEmpty(drawPath))
                        {
                            Aspose.Words.Drawing.Shape shape = new Aspose.Words.Drawing.Shape(Doc, ShapeType.Image);
                            shape.AllowOverlap = false;
                            shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center; //靠右对齐  
                            shape.ImageData.SetImage(drawPath);
                            ss.MoveToBookmark("pic");
                            System.Drawing.Image img = shape.ImageData.ToImage();
                            int h = 590;
                            if (img.Height > h)
                            {
                                if (img.Width < 520)
                                {
                                    double w = Convert.ToDouble(h * img.Width / img.Height);
                                    ss.InsertImage(drawPath, w, h);
                                }
                                else
                                {
                                    double w = 520;
                                    h = 590;
                                    ss.InsertImage(drawPath, w, h);
                                }
                            }
                            else if (img.Width > 520)
                            {
                                double x = Convert.ToDouble(520 * img.Height / img.Width);
                                ss.InsertImage(drawPath, 520, x);
                            }
                            else
                            {
                                ss.InsertImage(drawPath);
                            }
                        }
                    }
                    Bookmark Tmark = Doc.Range.Bookmarks["WiringTable"];//图书签
                    InsertDocument(Tmark.BookmarkStart.ParentNode, DocWrite);//表格复制
                    GetTables();
                }
            }
            catch (Exception ex)
            {
                string err = "Error in FillSystemWiring:\n" + ex.Message + "!\n" + ex.StackTrace;
                LogHelp.WriteLog(err, ex);
            }
        }

        #endregion

        #region Controller
        private void FillController()
        {
            try
            {
                tableindex = ++TableSum;

                if (thisProject.IsControllerChecked == false)
                {
                    Builder.MoveToBookmark("ChapterControl");
                    AddToDelNodeList(Builder.CurrentParagraph);
                    AddToDelNodeList(Builder.CurrentParagraph.NextSibling);
                    AddToDelNodeList(Tables[tableindex]);
                    AddToDelNodeList(Tables[tableindex].NextSibling);
                    return;
                }

                //修改章节编号
                Builder.MoveToBookmark("ChapterControl");
                Builder.CurrentParagraph.Runs[1].Text = ChapterNumber(0);

                //title
                this.DocSetBookMark("ChapterControl", Msg.GetResourceString("Report_DOC_ChapterControl"));//Writing

                //标题
                SetCellText(tableindex, 0, 0, Msg.GetResourceString("Report_DOC_ChapterControl_Type"));//Report_DOC_ChapterControl_Type
                SetCellText(tableindex, 0, 1, Msg.GetResourceString("Report_DOC_ChapterControl_Qty"));//Report_DOC_ChapterControl_Qty
                SetCellText(tableindex, 0, 2, Msg.GetResourceString("Report_DOC_ChapterControl_Des"));//Report_DOC_ChapterControl_Des

                //非EU区域暂时隐藏ControllerWiring on 20171229 by Shen Junjie
                //ANZ区域可以使用ControllerWiring modify by Shen Junjie on 20180117
                if (thisProject.RegionCode=="EU_W" || thisProject.RegionCode == "EU_E" || thisProject.RegionCode == "EU_S" || thisProject.SubRegionCode == "ANZ")
                {
                    //插入Controller 配线图
                    foreach (ControlGroup group in thisProject.ControlGroupList)
                    {
                        string drawPath = FileLocal.GetNamePathControllerWiringPicture(group.Id);
                        DocumentBuilder ss = new DocumentBuilder(Doc);
                        //图片数据
                        if (!string.IsNullOrEmpty(drawPath) && File.Exists(drawPath))
                        {
                            Aspose.Words.Drawing.Shape shape = new Aspose.Words.Drawing.Shape(Doc, ShapeType.Image);
                            shape.AllowOverlap = false;
                            shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center; //靠右对齐  
                            shape.ImageData.SetImage(drawPath);
                            ss.MoveToBookmark("ControlTable");
                            System.Drawing.Image img = shape.ImageData.ToImage();
                            int h = 590;
                            if (img.Height > h)
                            {
                                if (img.Width < 520)
                                {
                                    double w = Convert.ToDouble(h * img.Width / img.Height);
                                    ss.InsertImage(drawPath, w, h);
                                }
                                else
                                {
                                    double w = 520;
                                    h = 590;
                                    ss.InsertImage(drawPath, w, h);
                                }
                            }
                            else if (img.Width > 520)
                            {
                                double x = Convert.ToDouble(520 * img.Height / img.Width);
                                ss.InsertImage(drawPath, 520, x);
                            }
                            else
                            {
                                ss.InsertImage(drawPath);
                            }
                        }
                    }
                }

                string[] drawPaths = FileLocal.GetNamePathControllerPictures();
                foreach (string drawPath in drawPaths)
                {
                    DocumentBuilder ss = new DocumentBuilder(Doc);
                    //图片数据
                    if (!string.IsNullOrEmpty(drawPath))
                    {
                        Aspose.Words.Drawing.Shape shape = new Aspose.Words.Drawing.Shape(Doc, ShapeType.Image);
                        shape.AllowOverlap = false;
                        shape.HorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Left; //靠右对齐  
                        shape.ImageData.SetImage(drawPath);
                        ss.MoveToBookmark("ControlTable");
                        System.Drawing.Image img = shape.ImageData.ToImage();

                        int h = 600;
                        if (img.Height > h)
                        {
                            double w = Convert.ToDouble(h * img.Width / img.Height);
                            ss.InsertImage(drawPath, w, h);
                        }
                        else
                        {
                            ss.InsertImage(drawPath);
                        }
                    }
                }

                //插入表格数据
                DataTable data = GetData_ControllerMaterial().ToTable();
                Table table = (Table)Doc.GetChildNodes(Aspose.Words.NodeType.Table, true)[tableindex];
                if (data.Rows.Count > 0)
                {
                    string logInfo = string.Empty;
                    InsertEmptyRowsToTable(data.Rows.Count, table, out logInfo);
                    InsertDataToTable(1, 0, data, out logInfo);
                }
                else
                {
                    AddToDelNodeList(table);
                }
            }
            catch (Exception ex)
            {
                string err = "Error in FillController:\n" + ex.Message + "!\n" + ex.StackTrace;
                LogHelp.WriteLog(err, ex);
            }
        }
        #endregion

        #endregion

        #region 获取与Report文档中各个表格对应的数据
        // 得到当前项目（Project类）中的所有的Room记录, update 20130926 clh
        #region GetData_RoomList
        /// <summary>
        /// 得到当前项目（Project类）中的所有的Room记录
        /// </summary>
        /// <returns></returns>
        public DataView GetData_RoomList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_Room);

            for (int i = 0; i < thisProject.FloorList.Count; i++)
            {
                Floor f = thisProject.FloorList[i];
                for (int j = 0; j < f.RoomList.Count; j++)
                {
                    DataRow dr = dt.NewRow();
                    Room r = f.RoomList[j];
                    //判断数据是否为空
                    if (r == null)
                        continue;
                    dr[RptColName_Room.RoomNO] = bll.GetRoomNOString(r.NO, f);
                    //r.NO;

                    //2013-04-11 Update Distinguish the floor of the imported project
                    dr[RptColName_Room.FloorNO] = "Building" + f.ParentId.ToString() + ":" + f.Name;
                    //2013-04-11 Update End

                    dr[RptColName_Room.RoomName] = r.Name;
                    dr[RptColName_Room.RoomType] = r.Type;

                    //Update on 20120105 clh (以此为例，对所有数据实现根据当前项目的公英制进行转换)
                    dr[RptColName_Room.RoomArea] = Unit.ConvertToControl(r.Area, UnitType.AREA, utArea).ToString("n1");

                    dr[RptColName_Room.RLoad] = Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Room.RLoadHeat] = Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, utPower).ToString("n1");


                    dr[RptColName_Room.RSensibleHeatLoad] =
                        r.IsSensibleHeatEnable ? Unit.ConvertToControl(r.SensibleHeat, UnitType.POWER, utPower).ToString("n1") : "-";
                    dr[RptColName_Room.RAirFlow] =
                        r.IsAirFlowEnable ? Unit.ConvertToControl(r.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0") : "-";
                    // 20120105 Update End 

                    dt.Rows.Add(dr);
                }
            }
            return dt.DefaultView;
        }
        #endregion

        // 得到指定系统中的 Indoor Selection 记录 
        #region GetData_InSelectionList
        /// <summary>
        /// 得到指定系统中的Indoor Selection记录
        /// </summary>
        /// <returns></returns>
        public DataView GetData_InSelectionList(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_InSel);

            // 此处与旧版VRF列排列格式有不同，新版VRF中将Cooling与Heating的Capacity值分两列显示，旧版仅显示一列
            List<RoomIndoor> list =
                bll.GetSelectedIndoorBySystem(sysItem.Id, IndoorType.Indoor); //获取分配给指定系统的室内机记录

            //foreach (RoomIndoor ri in list)
            //{
            //    DataRow dr = dt.NewRow();
            //    Room room = bll.GetRoom(ri.RoomID);
            //    double rLoad = 0;
            //    double rLoadH = 0;
            //    if (room != null)
            //    {
            //        string strRoomNO = bll.GetRoomNOString(room.Id);
            //        dr[RptColName_Room.RoomNO] = strRoomNO;
            //        dr[RptColName_Room.RoomName] = room.Name;

            //        // 查找是否已存在相同的 Room
            //        if (HasSameData(ref dt, strRoomNO, 1))
            //            continue;
            //        rLoad = Unit.ConvertToControl(room.RqCapacityCool, UnitType.POWER, utPower);
            //        rLoadH = Unit.ConvertToControl(room.RqCapacityHeat, UnitType.POWER, utPower);
            //        dr[RptColName_Room.RLoad] = rLoad.ToString("n1");
            //        dr[RptColName_Room.RLoadHeat] = rLoadH.ToString("n1");

            //        dr[RptColName_Room.RSensibleHeatLoad] =
            //            room.IsSensibleHeatEnable ? Unit.ConvertToControl(room.SensibleHeat, UnitType.POWER, utPower).ToString("n1") : "-";
            //        dr[RptColName_Room.RAirFlow] =
            //            room.IsAirFlowEnable ? Unit.ConvertToControl(room.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0") : "-";
            //        dr[RptColName_Room.RStaticPressure] = room.IsStaticPressureEnable ? room.StaticPressure.ToString() : "-";
            //    }
            //    else
            //    {
            //        if (!string.IsNullOrEmpty(ri.DisplayRoom))
            //        {
            //            dr[RptColName_Room.RoomName] = ri.DisplayRoom;
            //        }

            //        rLoad = Unit.ConvertToControl(ri.RqCoolingCapacity, UnitType.POWER, utPower);
            //        rLoadH = Unit.ConvertToControl(ri.RqHeatingCapacity, UnitType.POWER, utPower);
            //        dr[RptColName_Room.RLoad] = rLoad.ToString("n1");
            //        dr[RptColName_Room.RLoadHeat] = rLoadH.ToString("n1");

            //        dr[RptColName_Room.RSensibleHeatLoad] = Unit.ConvertToControl(ri.RqSensibleHeat, UnitType.POWER, utPower).ToString("n1");
            //        dr[RptColName_Room.RAirFlow] = Unit.ConvertToControl(ri.RqAirflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //        dr[RptColName_Room.RStaticPressure] = ri.RqStaticPressure;
            //    }
            //    //add by axj 20170621
            //    dr[RptColName_Room.CoolingDB_WB] = Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, utTemperature) + "/" + Unit.ConvertToControl(ri.WBCooling, UnitType.TEMPERATURE, utTemperature);
            //    dr[RptColName_Room.HeatingDB] = Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, utTemperature);
            //    dr[RptColName_Unit.StaticPressure] = ri.StaticPressure;
            //    string modelName = ri.IndoorItem.Model_Hitachi;
            //    if (thisProject.BrandCode == "Y")
            //    {
            //        if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
            //            modelName = ri.IndoorItem.Model_York; //NA室内机显示Model_York 20170401
            //        else
            //            //modelName = ri.IndoorItem.Model; //Short ModelName
            //            modelName = ri.IndoorItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            //    }
            //    dr[Name_Common.Model] =  modelName;
            //    dr[RptColName_Unit.IndoorName] = ri.IndoorName;
            //    dr[Name_Common.Qty] = "1";
            //    FillMaterialData(dicMaterial["Indoor"], modelName, 1);
            //    dicMaterialDes[modelName] = ri.IndoorItem.Type;
            //    double inCapHeat;
            //    double inCapCool = ProjectBLL.GetIndoorActCapacityCool(ri, out inCapHeat);
            //    dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
            //    dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

            //    dr[RptColName_Unit.ActualSensibleCapacity] = Unit.ConvertToControl(ri.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
            //    dr[Name_Common.AirFlow] = Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");

            //    #region AdditionalIndoor Info 一个房间对应多台室内机时，实现按房间汇总显示
            //    List<RoomIndoor> riList = bll.GetSelectedIndoorByRoom(ri.RoomID, IndoorType.Indoor);
            //    if (riList != null && riList.Count > 1)
            //    {
            //        foreach (RoomIndoor item in riList)
            //        {
            //            if (item.IndoorNO != ri.IndoorNO)
            //            {
            //                string model = item.IndoorItem.Model_Hitachi;
            //                if (thisProject.BrandCode == "Y")
            //                {
            //                    if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
            //                        model = item.IndoorItem.Model_York; //NA室内机显示Model_York 20170401
            //                    else
            //                        //model = item.IndoorItem.Model; //Short ModelName
            //                        model = item.IndoorItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            //                }
            //                dr[Name_Common.Model] += ";\n" + model;
            //                dr[Name_Common.Qty] += ";\n" + "1";
            //                dr[RptColName_Unit.IndoorName] += ";\n" + item.IndoorName;
            //                FillMaterialData(dicMaterial["Indoor"], model, 1);
            //                dicMaterialDes[model] = item.IndoorItem.Type;
            //                double inCap2H = 0;
            //                double inCap2 = ProjectBLL.GetIndoorActCapacityCool(item, out inCap2H);
            //                inCapCool += inCap2;
            //                inCapHeat += inCap2H;
            //                dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
            //                dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

            //                dr[RptColName_Unit.ActualSensibleCapacity]
            //                    += ";\n" + Unit.ConvertToControl(item.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
            //                dr[Name_Common.AirFlow]
            //                    += ";\n" + Unit.ConvertToControl(item.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //                dr[RptColName_Unit.StaticPressure] += ";\n" + item.StaticPressure;
            //            }
            //        }
            //    }
            //    #endregion

            //    //Updated on 20120118 -clh- 室内机容量之和与房间（区域）负荷之和相比
            //    //if (Convert.ToDouble(Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1")) >= Convert.ToDouble(rLoad.ToString("n1")))
            //    if (CommonBLL.FullMeetRoomRequired(ri, thisProject))
            //        dr[Name_Common.Remark] = "OK";
            //    else
            //        dr[Name_Common.Remark] = '*' + Msg.IND_WARN_CAPLower;
            //    dt.Rows.Add(dr);

            //}
            return dt.DefaultView;
        }

        // add on 20120511 clh 增加 FA Selection 记录
        /// <summary>
        /// 得到指定系统中的FA Selection记录
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        public DataView GetData_InSelectionListFA(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_FASel);

            List<RoomIndoor> list =
                bll.GetSelectedIndoorBySystem(sysItem.Id, IndoorType.FreshAir);
            //foreach (RoomIndoor ri in list)
            //{
            //    DataRow dr = dt.NewRow();
            //    Room room = bll.GetRoom(ri.RoomID);

            //    double freshair = 0;
            //    if (room != null)
            //    {
            //        string strRoomNO = bll.GetRoomNOString(room.Id);
            //        dr[RptColName_Room.RoomNO] = strRoomNO;
            //        dr[RptColName_Room.RoomName] = room.Name;

            //        // 查找是否已存在相同的 Room
            //        if (HasSameData(ref dt, strRoomNO, 1))
            //            continue;

            //        dr[RptColName_Room.NoOfPerson] = room.PeopleNumber;
            //        dr[RptColName_Room.FAIndex] =
            //            Unit.ConvertToControl(room.FreshAirIndex, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //        freshair = Unit.ConvertToControl(room.FreshAir, UnitType.AIRFLOW, utAirflow);
            //        dr[RptColName_Room.FreshAir] = freshair.ToString("n0");
            //    }
            //    else
            //    {
            //        //获取新风区域 add on 20160728 by Yunxiao Lin
            //        if (thisProject.FreshAirAreaList != null)
            //        {
            //            FreshAirArea area = null;
            //            foreach (FreshAirArea a in thisProject.FreshAirAreaList)
            //            {
            //                if (ri.RoomID == a.Id)
            //                {
            //                    area = a;
            //                    break;
            //                }
            //            }

            //            if (area != null)
            //            {
            //                string strRoomNO = area.NO.ToString();
            //                dr[RptColName_Room.RoomNO] = strRoomNO;
            //                dr[RptColName_Room.RoomName] = area.Name;
            //                dr[RptColName_Room.NoOfPerson] = bll.GetPeopleNoInFreshAirArea(area);
            //                double FAIndex = bll.GetFAIndexOfFreshAirArea(area);
            //                dr[RptColName_Room.FAIndex] =
            //                    Unit.ConvertToControl(FAIndex, UnitType.AIRFLOW, utAirflow).ToString("n2");
            //                freshair = Unit.ConvertToControl(area.FreshAir, UnitType.AIRFLOW, utAirflow);
            //                dr[RptColName_Room.FreshAir] = freshair.ToString("n0");
            //            }
            //            else
            //            {
            //                freshair = Unit.ConvertToControl(ri.RqFreshAir, UnitType.AIRFLOW, utAirflow);
            //                dr[RptColName_Room.FreshAir] = freshair.ToString("n0");
            //            }
            //        }
            //        else
            //        {
            //            freshair = Unit.ConvertToControl(ri.RqFreshAir, UnitType.AIRFLOW, utAirflow);
            //            dr[RptColName_Room.FreshAir] = freshair.ToString("n0");
            //        }
            //    }

            //    string modelName = "";
            //    if (thisProject.BrandCode == "Y")
            //        if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
            //            modelName = ri.IndoorItem.Model_York;
            //        else
            //            //modelName = ri.IndoorItem.Model; //Short ModelName
            //            modelName = ri.IndoorItem.Model_York; //根据PM要求，piping 中的IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            //    else if (thisProject.BrandCode == "H")
            //        modelName = ri.IndoorItem.Model_Hitachi;
            //    dr[Name_Common.Model] = modelName;
            //    dr[RptColName_Unit.IndoorName] = ri.IndoorName;
            //    //add by axj 20170621
            //    dr[RptColName_Room.CoolingDB_WB] = Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, utTemperature) + "/" + Unit.ConvertToControl(ri.WBCooling, UnitType.TEMPERATURE, utTemperature);
            //    dr[RptColName_Room.HeatingDB] = Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, utTemperature);

            //    dr[Name_Common.Qty] = "1";
            //    FillMaterialData(dicMaterial["FreshAir"], modelName, 1);
            //    dicMaterialDes[modelName] = ri.IndoorItem.Type;
            //    // 新风机容量
            //    double inCapHeat;
            //    double inCapCool = ProjectBLL.GetIndoorActCapacityCool(ri, out inCapHeat);
            //    dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
            //    dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

            //    // 新风机风量
            //    dr[Name_Common.AirFlow] =
            //        Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");

            //    #region AdditionalFreshAir Info 一个房间对应多台新风机时，实现按房间汇总显示
            //    List<RoomIndoor> riList = bll.GetSelectedIndoorByRoom(ri.RoomID, IndoorType.FreshAir);
            //    if (riList != null && riList.Count > 1)
            //    {
            //        foreach (RoomIndoor item in riList)
            //        {
            //            if (item.IndoorNO != ri.IndoorNO)
            //            {
            //                string model = "";
            //                if (thisProject.BrandCode == "Y")
            //                    if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
            //                        model = item.IndoorItem.Model_York;
            //                    else
            //                        //model = item.IndoorItem.Model; //Short ModelName
            //                        model = item.IndoorItem.Model_York; //根据PM要求，piping 中的IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
            //                else if (thisProject.BrandCode == "H")
            //                    model = item.IndoorItem.Model_Hitachi;
            //                dr[Name_Common.Model] += ";\n" + model;//item.IndoorItem.Model;
            //                dr[Name_Common.Qty] += ";\n" + "1";
            //                dr[RptColName_Unit.IndoorName] += ";\n" + item.IndoorName;
            //                FillMaterialData(dicMaterial["FreshAir"], model, 1);
            //                dicMaterialDes[model] = item.IndoorItem.Type;
            //                double inCap2H = 0;
            //                double inCap2 = ProjectBLL.GetIndoorActCapacityCool(item, out inCap2H);
            //                inCapCool += inCap2;
            //                inCapHeat += inCap2H;
            //                dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
            //                dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

            //                dr[Name_Common.AirFlow]
            //                    += ";\n" + Unit.ConvertToControl(item.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
            //            }
            //        }
            //    }
            //    #endregion

            //    //-clh- 新风机风量与新风区域风量需求之和相比
            //    if (ri.AirFlow >= freshair)
            //        dr[Name_Common.Remark] = "OK";
            //    else
            //        dr[Name_Common.Remark] = Msg.IND_WARN_AFLowerFA;
            //    dt.Rows.Add(dr);
            //}
            return dt.DefaultView;
        }
        #endregion

        // 得到指定系统中的 Outdoor Selection 记录
        #region GetData_OutSelection
        /// <summary>
        /// 得到指定系统中的Outdoor Selection记录
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        public DataView GetData_OutSelection(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            DataTable dt = new DataTable();
            foreach (string s in NameArray_Report.ColNameArray_OutSel)
                dt.Columns.Add(s);

            DataRow dr = dt.NewRow();
            string power = "";
            MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, sysItem.Power);
            if (powerItem != null)
            {
                power = System.Environment.NewLine + "(" + powerItem.Name + ")";
            }
            dr[Name_Common.Model] = sysItem.OutdoorItem.AuxModelName + power; //Model
            FillMaterialData(dicMaterial["Outdoor"], sysItem.OutdoorItem.AuxModelName, 1);
            dicMaterialDes[sysItem.OutdoorItem.AuxModelName] = sysItem.OutdoorItem.Series;
            dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
            dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
            dr[RptColName_Outdoor.AddRefrigerant] = Unit.ConvertToControl(sysItem.AddRefrigeration, UnitType.WEIGHT, utWeight).ToString("n1");
            dr[RptColName_Outdoor.CapacityRatio] = (sysItem.Ratio * 100).ToString("n0") + "%";
            if (!sysItem.OutdoorItem.ProductType.Contains("Water Source"))
            {
                //add by axj 20170621
                dr[RptColName_Outdoor.HeatingDB_WB] = Unit.ConvertToControl(sysItem.DBHeating, UnitType.TEMPERATURE, utTemperature) + "/" + Unit.ConvertToControl(sysItem.WBHeating, UnitType.TEMPERATURE, utTemperature);
                dr[RptColName_Outdoor.CoolingDB] = Unit.ConvertToControl(sysItem.DBCooling, UnitType.TEMPERATURE, utTemperature);
            }
            else
            {
                dr[RptColName_Outdoor.HeatingDB_WB] = Unit.ConvertToControl(sysItem.IWHeating, UnitType.TEMPERATURE, utTemperature);
                dr[RptColName_Outdoor.CoolingDB] = Unit.ConvertToControl(sysItem.IWCooling, UnitType.TEMPERATURE, utTemperature);
            }
            //自己构造 数据未添加到表中
            dt.Rows.Add(dr);
            return dt.DefaultView;
        }
        #endregion

        // 得到指定系统的配管图中，连接管的管径规格及长度汇总记录
        #region GetData_LinkPipeSpecG & GetData_LinkPipeSpecL
        /// <summary>
        /// 得到指定系统的连接管 管径与长度汇总数据 Update on 20120828 clh 
        /// </summary>
        /// <param name="sysName"></param>
        /// <param name="dvL"></param>
        /// <returns></returns>
        public DataView GetData_LinkPipeSpecG(ref JCHVRF.Model.NextGen.SystemVRF sysItem, out DataView dvL)
        {
            DataTable dtG = Util.InitDataTable(NameArray_Report.PipeSpec_Name);
            //Solve the problem of DataView sorting by string type by default 201120111 - clh -
            if (!CommonBLL.IsDimension_inch()) // 若为英制，此时的管径为分数格式（7/8等），不能转为double进行排序
                dtG.Columns[RptColName_PipeSpec.PipeSpec].DataType = typeof(double);
            DataTable dtL = dtG.Clone();

            if (sysItem.MyPipingNodeOut != null)
            {
                Lassalle.WPF.Flow.Node fNode = sysItem.MyPipingNodeOut;
                GatherLinkPipeSpec(ref dtG, ref dtL, fNode, sysItem.Name);
            }

            // add on 20120925 因为英制的管径规格为分数，故不能排序
            dtG.DefaultView.Sort = RptColName_PipeSpec.PipeSpec.ToString() + " asc";
            dtL.DefaultView.Sort = dtG.DefaultView.Sort;

            // add on 20140612 clh 若当前系统为自动配管计算，则等效长度默认为“-”
            if (!sysItem.IsInputLengthManually)
            {
                for (int i = 0; i < dtG.Rows.Count; i++)
                {
                    dtG.Rows[i][Name_Common.Length] = "-";
                    dtG.Rows[i][RptColName_PipeSpec.EqLength] = "-";
                }
                for (int i = 0; i < dtL.Rows.Count; i++)
                {
                    dtL.Rows[i][Name_Common.Length] = "-";
                    dtL.Rows[i][RptColName_PipeSpec.EqLength] = "-";
                }
            }

            dvL = dtL.DefaultView;
            return dtG.DefaultView;
        }

        private void GatherLinkPipeSpec(ref DataTable dtG, ref DataTable dtL, Lassalle.WPF.Flow.Node node, string sysName)
        {
            if (node == null) return;
            try
            {
                NGPipingBLL.PipingBLL pipBll = new NGPipingBLL.PipingBLL(thisProject);

                if (node is NGModel.MyNodeOut)
                {
                    NGModel.MyNodeOut nodeOut = node as NGModel.MyNodeOut;
                    if (nodeOut.PipeSize != null && nodeOut.PipeLengthes != null)
                    {
                        for (int i = 0; i < nodeOut.PipeSize.Length && i < nodeOut.PipeLengthes.Length; i++)
                        {
                            string pipeSizeGroup = nodeOut.PipeSize[i];
                            string[] aa = pipeSizeGroup.Split('x');
                            double pipeLength = nodeOut.PipeLengthes[i];
                            double eqLength = pipeLength;
                            if (aa.Length == 2)
                            {
                                AddLinkPipeSpec(ref dtG, aa[0].Trim(), PipeType.Gas.ToString(), pipeLength, eqLength, sysName);
                                AddLinkPipeSpec(ref dtL, aa[1].Trim(), PipeType.Liquid.ToString(), pipeLength, eqLength, sysName);
                            }
                            else if (aa.Length == 3)
                            {
                                AddLinkPipeSpec(ref dtG, aa[0].Trim(), PipeType.Gas.ToString(), pipeLength, eqLength, sysName);
                                AddLinkPipeSpec(ref dtG, aa[1].Trim(), PipeType.Gas.ToString(), pipeLength, eqLength, sysName);
                                AddLinkPipeSpec(ref dtL, aa[2].Trim(), PipeType.Liquid.ToString(), pipeLength, eqLength, sysName);
                            }
                        }
                    }
                }
                else if (node is NGModel.MyNode)
                {
                    foreach (NGModel.MyLink link in (node as NGModel.MyNode).MyInLinks)
                    {
                        double eqLength = pipBll.GetLinkLength_EQ(link);
                        if (!string.IsNullOrEmpty(link.SpecG_h) && link.SpecG_h != "-")
                            AddLinkPipeSpec(ref dtG, link.SpecG_h, PipeType.Gas.ToString(), link.Length, eqLength, sysName);
                        if (!string.IsNullOrEmpty(link.SpecG_l) && link.SpecG_l != "-")
                            AddLinkPipeSpec(ref dtG, link.SpecG_l, PipeType.Gas.ToString(), link.Length, eqLength, sysName);
                        if (!string.IsNullOrEmpty(link.SpecL) && link.SpecL != "-")
                            AddLinkPipeSpec(ref dtL, link.SpecL, PipeType.Liquid.ToString(), link.Length, eqLength, sysName);
                    }
                }

                if (node is NGModel.MyNodeOut)
                {
                    GatherLinkPipeSpec(ref dtG, ref dtL, (node as NGModel.MyNodeOut).ChildNode, sysName);
                }
                else if (node is NGModel.MyNodeYP)
                {
                    foreach (Lassalle.WPF.Flow.Node n in (node as NGModel.MyNodeYP).ChildNodes)
                    {
                        GatherLinkPipeSpec(ref dtG, ref dtL, n, sysName);
                    }
                }
                else if (node is NGModel.MyNodeCH)
                {
                    GatherLinkPipeSpec(ref dtG, ref dtL, (node as NGModel.MyNodeCH).ChildNode, sysName);
                }
                else if (node is NGModel.MyNodeMultiCH)
                {
                    foreach (Lassalle.WPF.Flow.Node n in (node as NGModel.MyNodeMultiCH).ChildNodes)
                    {
                        GatherLinkPipeSpec(ref dtG, ref dtL, n, sysName);
                    }
                }
            }
            catch (Exception exc)
            {
                JCMsg.ShowErrorOK("未获取到配管图节点的Link对象！\n" + exc.Message);
            }
        }

        // 辅助方法
        private void AddLinkPipeSpec(ref DataTable dt, string pipeSpec, string pipeType, double realLength, double eqLength, string sysName)
        {
            // 若表中已存在重复记录，则相应的数量加1
            foreach (DataRow r in dt.Rows)
            {
                string existSpec = r[RptColName_PipeSpec.PipeSpec].ToString();
                if (existSpec == pipeSpec && r[RptColName_PipeSpec.PipeType].ToString() == pipeType
                    && r[RptColName_PipeSpec.SysName].ToString() == sysName)
                {
                    int qty = Int32.Parse(r[Name_Common.Qty].ToString());
                    r[Name_Common.Qty] = qty + 1;

                    r[Name_Common.Length] = (double.Parse(r[Name_Common.Length].ToString()) + realLength).ToString("n1");
                    r[RptColName_PipeSpec.EqLength] = (double.Parse(r[RptColName_PipeSpec.EqLength].ToString()) + eqLength).ToString("n1");
                    return;
                }
            }
            // 若不存在重复记录，则添加新记录
            DataRow newR = dt.NewRow();
            newR[RptColName_PipeSpec.PipeSpec] = pipeSpec;
            newR[RptColName_PipeSpec.PipeType] = pipeType;
            newR[Name_Common.Qty] = 1;
            newR[Name_Common.Length] = realLength;
            newR[RptColName_PipeSpec.EqLength] = eqLength;
            newR[RptColName_PipeSpec.SysName] = sysName;
            dt.Rows.Add(newR);
        }

        #endregion

        /// 得到Controller组件的数据统计记录,20141010 clh
        /// <summary>
        /// 得到Controller组件的数据统计记录
        /// </summary>
        /// <returns></returns>
        public DataView GetData_ControllerMaterial()
        {
            NameArray_Controller arr = new NameArray_Controller();
            DataTable dt = Util.InitDataTable(arr.MaterialList_Name);
            //Name_Common.Model,
            //Name_Common.Qty,
            //Name_Common.Description
            foreach (Controller item in thisProject.ControllerList)
            {
                //查找此表中是否存在相同的数据
                if (HasTableData(ref dt, item.Name, 0, 1))
                    continue;
                DataRow dr = dt.NewRow();
                dr[Name_Common.Model] = item.Name;
                dr[Name_Common.Qty] = item.Quantity;
                //dr[Name_Common.Description] = item.Description;
                dr[Name_Common.Description] = trans.getTypeTransStr(TransType.Controller.ToString(), item.Description);
                dt.Rows.Add(dr);
                FillMaterialData(dicMaterial["Controller"], dr[Name_Common.Model].ToString(), item.Quantity);
                dicMaterialDes[dr[Name_Common.Model].ToString()] = item.Description;//item.Description.ToString();
            }

            foreach (ControllerAssembly item in thisProject.ControllerAssemblyList)
            {
                //查找此表中是否存在相同的数据
                if (HasTableData(ref dt, item.Name, 0, 1))
                    continue;
                DataRow dr = dt.NewRow();
                dr[Name_Common.Model] = item.Name;
                dr[Name_Common.Qty] = "1";
                dr[Name_Common.Description] = item.GetDescription(item.Name);
                dt.Rows.Add(dr);
                FillMaterialData(dicMaterial["Controller"], dr[Name_Common.Model].ToString(), 1);
                dicMaterialDes[dr[Name_Common.Model].ToString()] = item.GetDescription(item.Name);
            }

            return dt.DefaultView;
        }

        // 查找此表中是否存在重复的记录，若已存在则对应的Qty列加1
        #region HasTableData
        ///// <summary>
        ///// 查找此表中是否存在重复的记录，若已存在则对应的Qty列加1
        ///// </summary>
        ///// <param name="data">要查找的数据表</param>
        ///// <param name="keyName">要查找的型号</param>
        ///// <param name="cIndex">与数据比较的表列</param>
        ///// <returns>有相同的值，返回True，否则返回False</returns>
        //private bool HasTableData(ref DataTable data, string keyName, int cIndex)
        //{
        //    return HasTableData(ref data, keyName, cIndex, false);
        //}

        /// 20170314 重写调用参数 Yunxiao Lin
        /// <summary>
        /// 查找此表中是否存在重复的记录，若已存在则对应的Qty列加1
        /// </summary>
        /// <param name="data">需要查询的数据表</param>
        /// <param name="keyName">查询关键词</param>
        /// <param name="kIndex">查询关键词所在列索引，起始列索引为0</param>
        /// <param name="cIndex">数量列索引，起始列索引为0</param>
        /// <returns>有相同的值，返回True，否则返回False</returns>
        private bool HasTableData(ref DataTable data, string keyName, int kIndex, int cIndex)
        {
            return HasTableData(ref data, keyName, kIndex, cIndex, false, -1, -1);
        }

        ///// 查找此表中是否存在重复的机组记录，若已存在则对应的Qty列加1
        ///// <summary>
        ///// 查找此表中是否存在重复的机组记录，若已存在则对应的Qty列加1
        ///// </summary>
        ///// <param name="data"></param>
        ///// <param name="keyName">要查找的型号</param>
        ///// <param name="cIndex">起始索引为1</param>
        ///// <param name="isPrice"></param>
        ///// <returns></returns>
        //private bool HasTableData(ref DataTable data, string keyName, int cIndex, bool isPrice)
        //{
        //    bool returnBool = false;
        //    for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
        //    {
        //        DataRow dr = data.Rows[rIndex];
        //        string modelFull = dr[cIndex - 2].ToString();
        //        if (cIndex > 0 && modelFull == keyName) //
        //        {
        //            returnBool = true;
        //            string qty = dr[cIndex].ToString();
        //            if (!string.IsNullOrEmpty(qty) && qty != "-")
        //            {
        //                dr.BeginEdit();
        //                dr[cIndex] = Convert.ToInt32(qty) + 1;
        //                if (isPrice)
        //                {
        //                    string unitPrice = dr[cIndex + 1].ToString();
        //                    if (!string.IsNullOrEmpty(unitPrice) && unitPrice != "-")
        //                        dr[cIndex + 2] = Convert.ToDouble(dr[cIndex + 2]) + Convert.ToDouble(unitPrice);
        //                    //Convert.ToDouble(dr[cIndex]) * Convert.ToDouble(unitPrice);
        //                }
        //                dr.EndEdit();
        //            }
        //        }
        //    }
        //    return returnBool;
        //}

        /// 查找此表中是否存在重复的机组记录，若已存在则对应的Qty列加1
        /// 20170314 重写调用参数 Yunxiao Lin
        /// <summary>
        /// 查找此表中是否存在重复的机组记录，若已存在则对应的Qty列加1
        /// </summary>
        /// <param name="data">需要查询的数据表</param>
        /// <param name="keyName">查询关键词</param>
        /// <param name="kIndex">查询关键词所在列索引，起始列索引为0</param>
        /// <param name="cIndex">数量列索引，起始列索引为0</param>
        /// <param name="isPrice">是否需要计算价格</param>
        /// <param name="pIndex">单价列索引，起始索引为0</param>
        /// <param name="aIndex">总价列索引，起始索引为0</param>
        /// <returns>有相同的值，返回True，否则返回False</returns>
        private bool HasTableData(ref DataTable data, string keyName, int kIndex, int cIndex, bool isPrice, int pIndex, int aIndex)
        {
            bool returnBool = false;
            if (kIndex < 0)
                return returnBool;
            if (isPrice)
            {
                if (pIndex < 0 || cIndex < 0)
                    return returnBool;
            }
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                string modelFull = dr[kIndex].ToString();
                if (cIndex > 0 && modelFull == keyName)
                {
                    returnBool = true;
                    string qty = dr[cIndex].ToString();
                    if (!string.IsNullOrEmpty(qty) && qty != "-")
                    {
                        dr.BeginEdit();
                        dr[cIndex] = Convert.ToInt32(qty) + 1;
                        if (isPrice)
                        {
                            string unitPrice = dr[pIndex].ToString();
                            if (!string.IsNullOrEmpty(unitPrice) && unitPrice != "-")
                                dr[aIndex] = Convert.ToDouble(dr[aIndex]) + Convert.ToDouble(unitPrice);
                        }
                        dr.EndEdit();
                    }
                }
            }
            return returnBool;
        }

        // 通用查找，查找 data 表中指定的列是否已经包含指定的内容
        /// <summary>
        /// 通用查找，查找 data 表中指定的列是否已经包含指定的内容
        /// </summary>
        /// <param name="data"></param>
        /// <param name="text"></param>
        /// <param name="cIndex"> 起始索引值为 1 </param>
        /// <returns></returns>
        private bool HasSameData(ref DataTable data, string text, int cIndex)
        {
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                if (cIndex > 0 && dr[cIndex - 1].ToString() == text)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        // 得到该项目中汇总的 Indoor Product List
        #region GetData_InOutProductList\Option List
        /// <summary>
        /// 得到该项目中汇总的IndoorExchanger Product List
        /// </summary>
        /// <returns></returns>
        public DataView GetData_ExcProductList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitList);

            if (thisProject.ExchangerList != null)
            {
                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    //if (string.IsNullOrEmpty(ri.SystemID))  // TODO:待确认，没有分配室外机的室内机是否显示？？？20141209
                    //    continue;

                    //SystemVRF sysItem = (new ProjectBLL(thisProject)).GetSystem(ri.SystemID);
                    //if (!valSystemExport(sysItem))
                    //{
                    //    continue;
                    //}

                    //if (ri.IndoorItem == null)
                    //    continue;

                    // Updated on 20140530 clh
                    //string modelAfterOption = ri.IndoorItem.ModelFull;
                    //// 若当前选择了Option则此处的ModelName填写选择了Option之后的新型号名
                    //if (ri.OptionItem != null)
                    //{
                    //    modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
                    //    //if (HasTableData(ref dt, modelAfterOption, 1))
                    //    if (HasTableData(ref dt, modelAfterOption, 0, 2))
                    //        continue;
                    //}
                    //else
                    //{
                    //    // 判断有无重复记录
                    //    //if (HasTableData(ref dt, modelAfterOption, 1))
                    //    if (HasTableData(ref dt, modelAfterOption, 0, 2))
                    //        continue;
                    //}

                    //DataRow dr = dt.NewRow();
                    ////dr[Name_Common.ModelFull] = modelAfterOption;


                    //if (thisProject.BrandCode == "Y")
                    //    dr[Name_Common.ModelFull] = ri.IndoorItem.Model_York; //Short ModelName
                    //else if (thisProject.BrandCode == "H")
                    //    dr[Name_Common.ModelFull] = ri.IndoorItem.Model_Hitachi;

                    string model = ri.IndoorItem.Model_Hitachi;
                    if (thisProject.BrandCode == "Y")
                        model = ri.IndoorItem.Model_York;
                    //填充Exchanger 数据到材料清单 on 2017-07-31 by xyj
                    FillMaterialData(dicMaterial["Exchanger"], model, 1);
                    dicMaterialDes[model] = ri.IndoorItem.Series;
                    if (HasTableData(ref dt, model, 0, 2))
                        continue;

                    DataRow dr = dt.NewRow();
                    dr[Name_Common.ModelFull] = model;
                    //dr[Name_Common.UnitType] = ri.IndoorItem.Type;
                    dr[Name_Common.UnitType] = trans.getTypeTransStr(TransType.Indoor.ToString(), ri.IndoorItem.Type);
                    dr[Name_Common.Qty] = "1";

                    dr[RptColName_Unit.RatedCoolingCapacity] =
                        Unit.ConvertToControl(ri.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Unit.RatedHeatingCapacity] =
                        Unit.ConvertToControl(ri.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                    //目前EU以外区域的IDU的power input是w作为单位，需要转换为Kw  add by Shen Junjie on 2018/8/15
                    double powerInput = ri.IndoorItem.PowerInput_Cooling;
                    if (!ProjectBLL.IsIDUPowerInputKw(thisProject.RegionCode))
                    {
                        powerInput = powerInput / 1000;
                    }
                    dr[Name_Common.PowerInput] = powerInput;
                    dr[Name_Common.AirFlow] =
                        Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                    dr[Name_Common.Length] = Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Length), UnitType.LENGTH_MM, utDimension).ToString("n1");//ri.IndoorItem.Length;
                    dr[Name_Common.Width] = Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Width), UnitType.LENGTH_MM, utDimension).ToString("n1");//ri.IndoorItem.Width;
                    dr[Name_Common.Height] = Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Height), UnitType.LENGTH_MM, utDimension).ToString("n1");//ri.IndoorItem.Height;
                    dr[Name_Common.Weight] =
                        Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                    dt.Rows.Add(dr);
                }

            }
            return dt.DefaultView;
        }        /// 得到该项目中汇总的Indoor Product List
                 /// </summary>
                 /// <returns></returns>
        public DataView GetData_InProductList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitList);

            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (string.IsNullOrEmpty(ri.SystemID))  // TODO:待确认，没有分配室外机的室内机是否显示？？？20141209
                    continue;

                //JCHVRF.Model.NextGen.SystemVRF sysItem = (new ProjectBLL(thisProject)).GetSystem(ri.SystemID);
                //if (!valSystemExport(sysItem))
                //{
                //    continue;
                //}

                if (ri.IndoorItem == null)
                    continue;

                // Updated on 20140530 clh
                //string modelAfterOption = ri.IndoorItem.ModelFull;
                //// 若当前选择了Option则此处的ModelName填写选择了Option之后的新型号名
                //if (ri.OptionItem != null)
                //{
                //    modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
                //    //if (HasTableData(ref dt, modelAfterOption, 1))
                //    if (HasTableData(ref dt, modelAfterOption, 0, 2))
                //        continue;
                //}
                //else
                //{
                //    // 判断有无重复记录
                //    //if (HasTableData(ref dt, modelAfterOption, 1))
                //    if (HasTableData(ref dt, modelAfterOption, 0, 2))
                //        continue;
                //}

                //DataRow dr = dt.NewRow();
                ////dr[Name_Common.ModelFull] = modelAfterOption;


                //if (thisProject.BrandCode == "Y")
                //    dr[Name_Common.ModelFull] = ri.IndoorItem.Model_York; //Short ModelName
                //else if (thisProject.BrandCode == "H")
                //    dr[Name_Common.ModelFull] = ri.IndoorItem.Model_Hitachi;
                string model = ri.IndoorItem.Model_Hitachi;
                if (thisProject.BrandCode == "Y")
                    model = ri.IndoorItem.Model_York;
                if (HasTableData(ref dt, model, 0, 2))
                    continue;
                DataRow dr = dt.NewRow();
                dr[Name_Common.ModelFull] = model;
                //dr[Name_Common.UnitType] = ri.IndoorItem.Type;
                dr[Name_Common.UnitType] = trans.getTypeTransStr(TransType.Indoor.ToString(), ri.IndoorItem.Type);
                dr[Name_Common.Qty] = "1";

                dr[RptColName_Unit.RatedCoolingCapacity] =
                    Unit.ConvertToControl(ri.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                dr[RptColName_Unit.RatedHeatingCapacity] =
                    Unit.ConvertToControl(ri.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                //目前EU以外区域的IDU的power input是w作为单位，需要转换为Kw  add by Shen Junjie on 2018/8/15
                double powerInput = ri.IndoorItem.PowerInput_Cooling;
                if (!ProjectBLL.IsIDUPowerInputKw(thisProject.RegionCode))
                {
                    powerInput = Math.Round(powerInput / 1000, 2);
                }
                dr[Name_Common.PowerInput] = powerInput;


                dr[Name_Common.AirFlow] =
                    Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                dr[Name_Common.Length] = ri.IndoorItem.Length == "-" ? "0.0" : Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Length), UnitType.LENGTH_MM, utDimension).ToString("n1");//ri.IndoorItem.Length;
                dr[Name_Common.Width] = ri.IndoorItem.Width == "-" ? "0.0" : Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Width), UnitType.LENGTH_MM, utDimension).ToString("n1");//ri.IndoorItem.Width;
                dr[Name_Common.Height] = ri.IndoorItem.Height == "-" ? "0.0" : Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Height), UnitType.LENGTH_MM, utDimension).ToString("n1");//ri.IndoorItem.Height;
                dr[Name_Common.Weight] =
                    Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                dt.Rows.Add(dr);
            }

            return dt.DefaultView;
        }

        // 得到该项目中汇总的 Outdoor Product List
        /// <summary>
        /// 得到该项目中汇总的Outdoor Product List
        /// </summary>
        /// <returns></returns>
        public DataView GetData_OutProductList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitList_Outdoor);

            for (int systemCount = 0; systemCount < thisProject.SystemList.Count; systemCount++)
            {
                JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[systemCount];
                if (!valSystemExport(sysItem))
                    continue;

                //查找此表中是否存在相同的数据
                if (HasTableData(ref dt, sysItem.OutdoorItem.FullModuleName, 0, 2))
                    continue;
                DataRow dr = dt.NewRow();
                //dr[Name_Common.ModelFull] = sysItem.OutdoorItem.AuxModelName; // 是否改为.ModelFull

                string power = "";
                MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, sysItem.Power);
                if (powerItem != null)
                {
                    power = System.Environment.NewLine + "(" + powerItem.Name + ")";
                }
                dr[Name_Common.ModelFull] = sysItem.OutdoorItem.FullModuleName + power;
                if (sysItem.OutdoorItem.Series == null)
                {
                    MyProductType pt = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, sysItem.OutdoorItem.ProductType);
                    //dr[Name_Common.ProductType] = pt.Series;
                    dr[Name_Common.ProductType] = trans.getTypeTransStr(TransType.Series.ToString(), pt.Series);
                }
                else
                    //dr[Name_Common.ProductType] = sysItem.OutdoorItem.Series;
                    dr[Name_Common.ProductType] = trans.getTypeTransStr(TransType.Series.ToString(), sysItem.OutdoorItem.Series);
                dr[Name_Common.Qty] = "1";
                dr[RptColName_Unit.RatedCoolingCapacity] =
                    Unit.ConvertToControl(sysItem.OutdoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                dr[RptColName_Unit.RatedHeatingCapacity] =
                    Unit.ConvertToControl(sysItem.OutdoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                dr[Name_Common.PowerInput] = sysItem.OutdoorItem.Power_Cooling;
                dr[Name_Common.MaxOperationPI] = sysItem.OutdoorItem.MaxOperationPI_Cooling;
                if (string.IsNullOrEmpty(sysItem.OutdoorItem.MaxOperationPI_Cooling))
                {
                    //Outdoor item = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull);
                    //多ProductType功能修改 20160823 by Yunxiao Lin
                    Outdoor item = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.ProductType);
                    if (item != null)
                    {
                        sysItem.OutdoorItem = item;
                        dr[Name_Common.MaxOperationPI] = sysItem.OutdoorItem.MaxOperationPI_Cooling;
                    }
                }

                dr[Name_Common.AirFlow] =
                    Unit.ConvertToControl(sysItem.OutdoorItem.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                dr[Name_Common.Length] = sysItem.OutdoorItem.Length == "-" ? "0.0" : Unit.ConvertToControl(Convert.ToDouble(sysItem.OutdoorItem.Length), UnitType.LENGTH_MM, utDimension).ToString("n1");//sysItem.OutdoorItem.Length;
                dr[Name_Common.Width] = sysItem.OutdoorItem.Width == "-" ? "0.0" : Unit.ConvertToControl(Convert.ToDouble(sysItem.OutdoorItem.Width), UnitType.LENGTH_MM, utDimension).ToString("n1");//sysItem.OutdoorItem.Width;
                dr[Name_Common.Height] = sysItem.OutdoorItem.Height == "-" ? "0.0" : Unit.ConvertToControl(Convert.ToDouble(sysItem.OutdoorItem.Height), UnitType.LENGTH_MM, utDimension).ToString("n1");//sysItem.OutdoorItem.Height;
                dr[Name_Common.Weight] =
                    Unit.ConvertToControl(Convert.ToDouble(sysItem.OutdoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                dt.Rows.Add(dr);
            }
            return dt.DefaultView;
            //End
        }

        // 得到项目中汇总的标准 Option List —— Option B update on 20130922 clh
        /// <summary>
        /// 得到项目中汇总的标准Option List —— Option B
        /// </summary>
        /// <returns></returns>
        public DataView GetData_StdOptionBList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_StdOptionListB);

            // 遍历Project中所有的RoomIndoor，得到其“ModelFullAfterOption”属性，进行汇总
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                // 只统计已经加入到系统的Indoor记录，TODO:再次确定一下 20141209 clh
                if (ri.IndoorItem == null || string.IsNullOrEmpty(ri.SystemID))
                    continue;

                //add on 20120606 clh, 当机组为默认配置时，不要列出OptionList
                string modelAfterOption = "";
                if (ri.OptionItem == null)
                    continue;
                else
                {
                    modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
                    if (!ri.OptionItem.HasOption())
                        continue;
                }
                // 判断有无重复的 Option 记录
                if (HasRepeatedOption(ref dt, ri, 1))
                    continue;
                //HasTableData(ref dt, modelAfterOption, 1);

                DataRow dr = dt.NewRow();
                dr[Name_Common.ModelFull] = modelAfterOption;
                dr[Name_Common.Qty] = "1";

                string aeh = ri.OptionItem.OptionAEH == null ? "" : ri.OptionItem.OptionAEH.SelectedValue;
                string dped = ri.OptionItem.OptionDPED == null ? "" : ri.OptionItem.OptionDPED.SelectedValue;
                string cf = ri.OptionItem.OptionCF == null ? "" : ri.OptionItem.OptionCF.SelectedValue;
                string up = ri.OptionItem.OptionUP == null ? "" : ri.OptionItem.OptionUP.SelectedValue;
                string tio = ri.OptionItem.OptionTIO == null ? "" : ri.OptionItem.OptionTIO.SelectedValue;
                string power = ri.OptionItem.OptionPower == null ? "" : ri.OptionItem.OptionPower.GetDescription(ri.OptionItem.OptionPower.SelectedValue);
                string insulation = ri.OptionItem.OptionInsulation == null ? "" : ri.OptionItem.OptionInsulation.SelectedValue;

                dr[RptColName_Option.AuxiliaryElectricHeater] = aeh;
                dr[RptColName_Option.DrainagePumpandExpansionDevice] = dped;
                dr[RptColName_Option.Controller] = cf;
                dr[RptColName_Option.UnitPanel] = up;
                dr[RptColName_Option.DuctTypeTIO2] = tio; // ri.OptionItem.SelectedTO;
                dr[RptColName_Option.OptionPower] = power;
                dr[RptColName_Option.OptionInsulation] = insulation;

                dt.Rows.Add(dr);

            }
            // add on 20150625 clh 增加室外机的Option记录，与室内机的Option记录同一张表
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.OptionItem == null || sysItem.OptionItem.OptionPower == null)
                    continue;
                if (HasRepeatedOption_Outdoor(ref dt, sysItem, 1))
                    continue;
                DataRow dr = dt.NewRow();
                dr[Name_Common.ModelFull] = sysItem.OutdoorItem.ModelFull;
                dr[Name_Common.Qty] = "1";

                string power = sysItem.OptionItem.OptionPower == null ? "" : sysItem.OptionItem.OptionPower.GetDescription(sysItem.OptionItem.OptionPower.SelectedValue);
                dr[RptColName_Option.AuxiliaryElectricHeater] = "-";
                dr[RptColName_Option.DrainagePumpandExpansionDevice] = "-";
                dr[RptColName_Option.Controller] = "-";
                dr[RptColName_Option.UnitPanel] = "-";
                dr[RptColName_Option.DuctTypeTIO2] = "-"; // ri.OptionItem.SelectedTO;
                dr[RptColName_Option.OptionPower] = power;
                dr[RptColName_Option.OptionInsulation] = "-";
                dt.Rows.Add(dr);
            }

            return dt.DefaultView;
        }

        /// <summary>
        /// 判断有无重复的 Option 记录
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ri"></param>
        /// <param name="qtyIndex">数量，列号索引值</param>
        /// <returns></returns>
        public bool HasRepeatedOption(ref DataTable data, RoomIndoor ri, int qtyIndex)
        {
            bool ret = false;
            string modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                if (qtyIndex > 0 && dr[qtyIndex - 1].ToString() == modelAfterOption)
                {
                    string aeh = dr[RptColName_Option.AuxiliaryElectricHeater].ToString();
                    string dped = dr[RptColName_Option.DrainagePumpandExpansionDevice].ToString();
                    string cf = dr[RptColName_Option.Controller].ToString();
                    string up = dr[RptColName_Option.UnitPanel].ToString();
                    string tio = dr[RptColName_Option.DuctTypeTIO2].ToString();
                    string power = dr[RptColName_Option.OptionPower].ToString();
                    string insulation = dr[RptColName_Option.OptionInsulation].ToString();

                    if ((ri.OptionItem.OptionAEH == null || aeh == ri.OptionItem.OptionAEH.SelectedValue)
                    && (ri.OptionItem.OptionDPED == null || dped == ri.OptionItem.OptionDPED.SelectedValue)
                    && (ri.OptionItem.OptionCF == null || cf == ri.OptionItem.OptionCF.SelectedValue)
                    && (ri.OptionItem.OptionUP == null || up == ri.OptionItem.OptionUP.SelectedValue)
                    && (ri.OptionItem.OptionTIO == null || tio == ri.OptionItem.OptionTIO.SelectedValue)
                    && (ri.OptionItem.OptionPower == null || power == ri.OptionItem.OptionPower.SelectedValue)
                    && (ri.OptionItem.OptionInsulation == null || insulation == ri.OptionItem.OptionInsulation.SelectedValue))
                    {
                        ret = true;
                        string qty = dr[qtyIndex].ToString();
                        if (!string.IsNullOrEmpty(qty) && qty != "-")
                        {
                            dr.BeginEdit();
                            dr[qtyIndex] = Convert.ToInt32(qty) + 1;
                            dr.EndEdit();
                        }
                    }
                }
            }
            return ret;
        }

        /// 判断室外机的Option记录有没有重复项
        /// <summary>
        /// 判断室外机的Option记录有没有重复项
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sys"></param>
        /// <param name="qtyIndex"></param>
        /// <returns></returns>
        public bool HasRepeatedOption_Outdoor(ref DataTable data, SystemVRF sys, int qtyIndex)
        {
            bool ret = false;
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                if (qtyIndex > 0 && dr[qtyIndex - 1].ToString() == sys.OutdoorItem.ModelFull)
                {
                    string power = dr[RptColName_Option.OptionPower].ToString();

                    if (sys.OptionItem.OptionPower == null
                        || power == sys.OptionItem.OptionPower.GetDescription(sys.OptionItem.OptionPower.SelectedValue))
                    {
                        ret = true;
                        string qty = dr[qtyIndex].ToString();
                        if (!string.IsNullOrEmpty(qty) && qty != "-")
                        {
                            dr.BeginEdit();
                            dr[qtyIndex] = Convert.ToInt32(qty) + 1;
                            dr.EndEdit();
                        }
                    }
                }
            }
            return ret;
        }

        #endregion

        // 得到当前项目中汇总的室内外机的价格表
        #region GetData_UnitPriceList
        /// <summary>
        /// 得到当前项目中汇总的室内外机的价格表
        /// </summary>
        /// <returns></returns>
        public DataView GetData_UnitPriceList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitPrice);
            //添加Indoor
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (string.IsNullOrEmpty(ri.SystemID))
                    continue;

                if (ri.IndoorItem == null)
                    continue;

                // 查找此表中是否存在相同的数据 
                // Updated on 20140530 clh
                // 此处区分是否相同的室内机，要判断所有的 Option 选项，若存在不影响Model的Option项目存在，则在Remark列中备注
                string modelAfterOption = ri.IndoorItem.ModelFull;
                string remark = "";

                // 若当前选择了Option则此处的ModelName填写选择了Option之后的新型号名
                if (ri.OptionItem != null)
                {
                    modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();

                    // TODO:待确认，AEH、DPED、CF等三个Option的描述是否需要显示？？？

                    if (ri.OptionItem.OptionUP != null && ri.OptionItem.OptionUP.HasOption())
                    {
                        remark = ri.OptionItem.OptionUP.GetDescription();
                    }

                    if (ri.OptionItem.OptionTIO != null && ri.OptionItem.OptionTIO.HasOption())
                    {
                        string str = ri.OptionItem.OptionTIO.GetDescription(ri.OptionItem.OptionTIO.SelectedValue);
                        remark = remark == "" ? remark = str : remark = remark + ";\n" + str;
                    }

                    if (ri.OptionItem.OptionPower != null && ri.OptionItem.OptionPower.HasOption())
                    {
                        string str = ri.OptionItem.OptionPower.GetDescription();
                        remark = remark == "" ? remark = str : remark = remark + ";\n" + str;
                    }

                    if (ri.OptionItem.OptionInsulation != null && ri.OptionItem.OptionInsulation.HasOption())
                    {
                        string str = ri.OptionItem.OptionInsulation.GetDescription();
                        remark = remark == "" ? remark = str : remark = remark + ";\n" + str;
                    }

                    if (hasSameOption(ref dt, modelAfterOption, remark))
                        continue;
                }
                else
                {
                    if (HasTableData(ref dt, modelAfterOption, 0, 2, true, 3, 4))
                        continue;
                }

                DataRow dr = dt.NewRow();
                dr[RptColName_Unit.UnitName] = "Indoor Unit";
                dr[Name_Common.ModelFull] = modelAfterOption;
                dr[Name_Common.Qty] = "1";
                // update on 20120612 clh
                double stdPrice = 0;
                //ri.IndoorItem.StandardPrice;
                // get option price sum 20130918 clh
                double optPrice = GetOptionPriceSum(ri);
                dr[Name_Common.UnitPrice] = "-";
                //ShowPrice() ? stdPrice.ToString() : "-"; //20150712 clh 放开出口区域的价格显示
                dr[Name_Common.TotalPrice] = dr[Name_Common.UnitPrice];
                // ShowPrice() ? (stdPrice + optPrice).ToString() : "-";
                //dr[Name_Common.UnitPrice] = "-"; //20140620 clh
                //dr[Name_Common.TotalPrice] = "-";

                dr[Name_Common.Remark] = remark;
                dt.Rows.Add(dr);
            }
            //添加Outdoor
            for (int i = 0; i < thisProject.SystemList.Count; i++)
            {
                JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[i];
                if (!valSystemExport(sysItem))
                    continue;
                //查找此表中是否存在相同的数据
                if (HasTableData(ref dt, sysItem.OutdoorItem.AuxModelName, 0, 2, true, 3, 4))
                    continue;
                DataRow dr = dt.NewRow();
                dr[RptColName_Unit.UnitName] = "Outdoor Unit";
                dr[Name_Common.ModelFull] = sysItem.OutdoorItem.AuxModelName;
                dr[Name_Common.Qty] = "1";

                double price = 0;

                dr[Name_Common.UnitPrice] = "-";
                //ShowPrice() ? price.ToString() : "-"; //20150712 clh 放开出口区域的价格显示
                //dr[Name_Common.UnitPrice] = "-"; //20140620 clh
                dr[Name_Common.TotalPrice] = dr[Name_Common.UnitPrice];

                string remark = "";

                dr[Name_Common.Remark] = remark;
                dt.Rows.Add(dr);
            }
            return dt.DefaultView;
        }

        /// <summary>
        /// 判断当前室内机与表格中已添加的室内机有无完全相同的记录，所有的Option选项都必须一致
        /// </summary>
        /// <param name="data">模板中的价格表</param>
        /// <param name="modelAfterOption">当前的产品型号</param>
        /// <param name="remark">UP以及TIO2的描述，默认则为空</param>
        /// <returns></returns>
        private bool hasSameOption(ref DataTable data, string modelAfterOption, string remark)
        {
            bool returnBool = false;
            int modelIndex = 1;
            int remarkIndex = 5;
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                if (dr[modelIndex].ToString() == modelAfterOption && dr[remarkIndex].ToString() == remark)
                {
                    returnBool = true;
                    string qty = dr[modelIndex + 1].ToString();
                    if (!string.IsNullOrEmpty(qty) && qty != "-")
                    {
                        dr.BeginEdit();
                        dr[modelIndex + 1] = Convert.ToInt32(qty) + 1;

                        string unitPrice = dr[modelIndex + 2].ToString();
                        if (!string.IsNullOrEmpty(unitPrice) && unitPrice != "-")
                            dr[modelIndex + 3] = Convert.ToDouble(qty) * Convert.ToDouble(unitPrice);
                        dr.EndEdit();
                    }
                }
            }
            return returnBool;
        }

        // 计算指定室内机的所有选配项的价格之和
        /// <summary>
        /// 计算指定室内机的所有选配项的价格之和
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public double GetOptionPriceSum(RoomIndoor ri)
        {
            double optPrice = 0;
            //if (ri.OptionItem != null)
            //{
            //    // 电加热
            //    if (ri.OptionItem.OptionAEH != null)
            //        optPrice += ri.OptionItem.OptionAEH.GetPrice();

            //    // 水泵 & 排水管
            //    if (ri.OptionItem.OptionDPED != null)
            //        optPrice += ri.OptionItem.OptionDPED.GetPrice();

            //    // 遥控+线控 & 过滤网
            //    if (ri.OptionItem.OptionCF != null)
            //        optPrice += ri.OptionItem.OptionCF.GetPrice();

            //    // 面板
            //    if (ri.OptionItem.OptionUP != null)
            //        optPrice += ri.OptionItem.OptionUP.GetPrice();

            //    // TIO2
            //    if (ri.OptionItem.OptionTIO != null)
            //        optPrice += ri.OptionItem.OptionTIO.GetPrice();

            //    // Power
            //    if (ri.OptionItem.OptionPower != null)
            //        optPrice += ri.OptionItem.OptionPower.GetPrice();

            //    // Insulation
            //    if (ri.OptionItem.OptionInsulation != null)
            //        optPrice += ri.OptionItem.OptionInsulation.GetPrice();
            //}
            return optPrice;
        }
        #endregion

        // 得到当前项目中汇总的分歧管的价格表,add on 20120827 clh
        #region GetData_JointKitPriceList
        /// <summary>
        /// 得到当前项目中汇总的分歧管的价格表，TODO：update 20130926
        /// </summary>
        /// <returns></returns>
        public DataView GetData_JointKitPriceList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_JointKitPrice);
            // 向 dt 中插入分歧管记录
            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.MyPipingNodeOut != null)
                {
                    Lassalle.Flow.Node fNode = sysItem.MyPipingNodeOut.ChildNode;
                    GatherJointKit(ref dt, fNode);
                }

                // 20150128 clh update 更新Outdoor类结构，增加分歧管相关字段
                if (sysItem.OutdoorItem != null)
                {
                    // 为兼容之前的项目
                    if (thisProject.CreateDate < DateTime.Parse("2015/1/28")
                        && Util.ParseToInt(sysItem.OutdoorItem.ModelFull) >= 200)
                    {
                        //OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode);
                        //sysItem.OutdoorItem = bll.GetOutdoorItem(sysItem.OutdoorItem.ModelFull);
                        //多ProductType功能修改 20160823 by Yunxiao Lin
                        OutdoorBLL bll = new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode);
                        sysItem.OutdoorItem = bll.GetOutdoorItem(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.ProductType);
                    }

                    if (!string.IsNullOrEmpty(sysItem.OutdoorItem.JointKitModelG) &&
                        !string.IsNullOrEmpty(sysItem.OutdoorItem.JointKitModelL)
                        )
                    {
                        string[] arrJointKitModelL = sysItem.OutdoorItem.JointKitModelL.Split('/');
                        string[] arrJointKitModelG = sysItem.OutdoorItem.JointKitModelG.Split('/');
                        //string[] arrJointKitPriceL = sysItem.OutdoorItem.JointKitPriceL.Split('/');改为直接从分歧管价格表中获取价格
                        //string[] arrJointKitPriceG = sysItem.OutdoorItem.JointKitPriceG.Split('/');                        
                        for (int i = 0; i < arrJointKitModelL.Length; ++i)
                        {
                            AddRowToTable_JointKit(ref dt, arrJointKitModelG[i], "-");
                            AddRowToTable_JointKit(ref dt, arrJointKitModelL[i], "-");
                        }
                    }
                }
            }

            DataView dv = dt.DefaultView;
            dv.Sort = Name_Common.ModelFull + " desc";

            return dv;
        }


        private void GatherJointKit(ref DataTable dt, Lassalle.Flow.Node node)
        {
            if (node is MyNodeYP)
            {
                MyNodeYP yp = node as MyNodeYP;
                AddRowToTable_JointKit(ref dt, yp.Model, yp.PriceG);
                //AddRowToTable_JointKit(ref dt, yp.ModelL, yp.PriceL);

                foreach (Lassalle.Flow.Node nd in yp.ChildNodes)
                    GatherJointKit(ref dt, nd);
            }
        }

        // 辅助方法
        private void AddRowToTable_JointKit(ref DataTable dt, string JointKitModel, string JointKitPrice)
        {
            if (HasTableData(ref dt, JointKitModel, 0, 1, true, 2, 3))
                return;
            DataRow dr = dt.NewRow();
            dr[Name_Common.ModelFull] = JointKitModel;
            dr[Name_Common.Qty] = "1";
            dr[Name_Common.UnitPrice] = "-";
            //ShowPrice() ? JointKitPrice : "-";//20150712 clh 1.20版本放开出口区域价格
            //dr[Name_Common.UnitPrice] = "-"; //20140620 clh
            dr[Name_Common.TotalPrice] = dr[Name_Common.UnitPrice];
            dr[Name_Common.Remark] = "";
            dt.Rows.Add(dr);
        }

        #endregion
        #endregion

        #region 将日志记录的内容写入指定的文件
        /// <summary>
        /// 将系统日志记录的内容写入指定的文件
        /// </summary>
        /// <param name="logList">系统日志记录</param>
        /// <param name="logPath"></param>
        private void WriteToLogFile(List<string> logList, string logPath)
        {
            if (logList == null || logList.Count == 0)
                return;
            StreamWriter streamWriter = new StreamWriter(logPath);
            for (int i = 0; i <= logList.Count - 1; i++)
            {
                streamWriter.WriteLine(logList[i]);
            }
            streamWriter.Flush();
            streamWriter.Close();
        }

        /// <summary>
        /// 将 Log 记录添加到 ReportLogList 中
        /// </summary>
        /// <param name="info"></param>
        private void AddToLog(string info)
        {
            ReportLogList.Add(info + "\t时间：" + DateTime.Now);
        }

        #endregion

        #region 其他方法
        /// <summary>
        /// 返回两数中的较大值
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        private int GetMaxNum(int n1, int n2)
        {
            return n1 > n2 ? n1 : n2;
        }

        /// <summary>
        /// 得到 selection mode 属性
        /// </summary>
        /// <returns></returns>
        private string getSelectionMode()
        {
            string selMode = "";
            bool cooling = thisProject.IsCoolingModeEffective;
            bool heating = thisProject.IsHeatingModeEffective;
            if (cooling || heating)
            {
                if (cooling && heating)
                    selMode = ShowText.Cooling + "+" + ShowText.Heating;
                else if (cooling)
                    selMode = ShowText.Cooling;
                else
                    selMode = ShowText.Heating;
            }
            return selMode;
        }

        // 验证当前系统是否符合输出条件
        /// <summary>
        /// 验证当前系统是否符合输出条件
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        private bool valSystemExport(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            if (sysItem.OutdoorItem == null)
                return false;
            else if (!sysItem.IsExportToReport)
                return false;

            return true;
        }

        /// <summary>
        /// 填充材料数据
        /// </summary>
        /// <param name="dicMaterial"></param>
        /// <param name="key"></param>
        /// <param name="qty"></param>
        private void FillMaterialData(Dictionary<string, int> dicMaterial, string key, int qty)
        {
            if (!dicMaterial.Keys.Contains(key))
            {
                dicMaterial[key] = qty;
            }
            else
            {
                dicMaterial[key] = dicMaterial[key] + qty;
            }
        }

        #endregion

        #region 附件列表
        private bool valIndoorExport(RoomIndoor roomItem)
        {
            bool isExport = true;
            for (int i = 0; i < thisProject.SystemListNextGen.Count; i++)
            {
                if (thisProject.SystemList[i].Id == roomItem.SystemID)
                {
                    isExport = valSystemExport(thisProject.SystemListNextGen[i]);
                    break;
                }
            }
            return isExport;
        }
        public DataTable GetOptionInformationList()
        {
            DataTable tb = new DataTable();
            tb.Columns.Add("IndoorModel");
            tb.Columns.Add("AccessoryType");
            tb.Columns.Add("AccessoryModel");
            tb.Columns.Add("Qty");

            int Qty = 0;

            List<AccessorySpc> ListAccessory = new List<AccessorySpc>();
            foreach (RoomIndoor ri in thisProject.RoomIndoorList)
            {
                if (!valIndoorExport(ri))
                    continue;

                if (ri.ListAccessory == null)
                    continue;

                foreach (Accessory item in ri.ListAccessory)
                {

                    string aModel = item.Model_Hitachi;
                    if (item.BrandCode == "Y")
                    {
                        aModel = item.Model_York;
                    }
                    if (item == null && HasTableData(ref tb, aModel, 2, 3))
                    {
                        continue;
                    }


                    DataRow row = tb.NewRow();
                    //row["AccessoryType"] = item.Type;
                    row["AccessoryType"] = trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type);
                    if (item.BrandCode == "Y")
                    {
                        row["IndoorModel"] = ri.IndoorItem.Model_York;
                        row["AccessoryModel"] = item.Model_York;
                        Qty = ri.ListAccessory.Where(p => p.Model_York == aModel).Count();
                    }
                    else
                    {
                        row["IndoorModel"] = ri.IndoorItem.Model_Hitachi;
                        row["AccessoryModel"] = item.Model_Hitachi;
                        Qty = ri.ListAccessory.Where(p => p.Model_Hitachi == aModel).Count();
                    }

                    AccessorySpc Acc = new AccessorySpc();
                    Acc.IndoorModel = row["IndoorModel"].ToString();
                    Acc.FactoryCode = item.FactoryCode;
                    Acc.BrandCode = item.BrandCode;
                    Acc.UnitType = item.UnitType;
                    Acc.MinCapacity = item.MinCapacity;
                    Acc.MaxCapacity = item.MaxCapacity;
                    Acc.Type = item.Type;
                    Acc.Model_York = item.Model_York;
                    Acc.Model_Hitachi = item.Model_Hitachi;
                    Acc.MaxNumber = item.MaxNumber;
                    Acc.IsDefault = item.IsDefault;
                    ListAccessory.Add(Acc);

                    row["Qty"] = Qty.ToString();
                    tb.Rows.Add(row);
                }
            }

            //添加Total Heat Exchanger 附件列表 on 20170830 by xyj
            if (thisProject.ExchangerList != null)
            {
                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    if (!valIndoorExport(ri))
                        continue;

                    if (ri.ListAccessory == null)
                        continue;

                    foreach (Accessory item in ri.ListAccessory)
                    {

                        string aModel = item.Model_Hitachi;
                        if (item.BrandCode == "Y")
                        {
                            aModel = item.Model_York;
                        }
                        if (item == null && HasTableData(ref tb, aModel, 2, 3))
                        {
                            continue;
                        }


                        DataRow row = tb.NewRow();
                        //row["AccessoryType"] = item.Type;
                        row["AccessoryType"] = trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type);
                        if (item.BrandCode == "Y")
                        {
                            row["IndoorModel"] = ri.IndoorItem.Model_York;
                            row["AccessoryModel"] = item.Model_York;
                            Qty = ri.ListAccessory.Where(p => p.Model_York == aModel).Count();
                        }
                        else
                        {
                            row["IndoorModel"] = ri.IndoorItem.Model_Hitachi;
                            row["AccessoryModel"] = item.Model_Hitachi;
                            Qty = ri.ListAccessory.Where(p => p.Model_Hitachi == aModel).Count();
                        }

                        AccessorySpc Acc = new AccessorySpc();
                        Acc.IndoorModel = row["IndoorModel"].ToString();
                        Acc.FactoryCode = item.FactoryCode;
                        Acc.BrandCode = item.BrandCode;
                        //Acc.UnitType = item.UnitType;
                        Acc.UnitType = trans.getTypeTransStr(TransType.Series.ToString(), item.UnitType);
                        Acc.MinCapacity = item.MinCapacity;
                        Acc.MaxCapacity = item.MaxCapacity;
                        Acc.Type = item.Type;
                        Acc.Model_York = item.Model_York;
                        Acc.Model_Hitachi = item.Model_Hitachi;
                        Acc.MaxNumber = item.MaxNumber;
                        Acc.IsDefault = item.IsDefault;
                        ListAccessory.Add(Acc);

                        row["Qty"] = Qty.ToString();
                        tb.Rows.Add(row);
                    }
                }
            }

            if (tb != null && tb.Rows.Count > 0)
            {
                DataTable tbtemp = tb.Clone();
                foreach (DataRow row in tb.Rows)
                {
                    DataView dv = tbtemp.DefaultView;
                    dv.RowFilter = "IndoorModel='" + row["IndoorModel"] + "' and AccessoryModel='" + row["AccessoryModel"] + "' and AccessoryType='" + row["AccessoryType"] + "'";
                    if (dv.Count == 0 || dv == null)
                    {
                        DataRow r = tbtemp.NewRow();
                        r["IndoorModel"] = row["IndoorModel"];
                        r["AccessoryType"] = row["AccessoryType"];
                        r["AccessoryModel"] = row["AccessoryModel"];
                        if (ListAccessory != null && ListAccessory.Count > 0)
                        {

                            if (ListAccessory[0].BrandCode == "Y")
                            {
                                r["Qty"] = ListAccessory.Where(p => p.Type == row["AccessoryType"].ToString() && p.Model_York == row["AccessoryModel"].ToString() && p.IndoorModel == row["IndoorModel"].ToString()).Count();
                            }
                            else
                            {
                                r["Qty"] = ListAccessory.Where(p => p.Type == row["AccessoryType"].ToString() && p.Model_Hitachi == row["AccessoryModel"].ToString() && p.IndoorModel == row["IndoorModel"].ToString()).Count();
                            }
                        }
                        else
                        {
                            r["Qty"] = "0";
                        }
                        tbtemp.Rows.Add(r);
                    }
                }
                tb = tbtemp;
            }
            //列值互换 axj 20170713
            DataTable tbNew = new DataTable();
            tbNew.Columns.Add("AccessoryModel");
            tbNew.Columns.Add("AccessoryType");
            tbNew.Columns.Add("IndoorModel");
            tbNew.Columns.Add("Qty");
            foreach (DataRow dr in tb.Rows)
            {
                tbNew.Rows.Add(dr[2], dr[1], dr[0], dr[3]);
            }
            return tbNew;
        }

        /// <summary>
        /// 获取附件清单中备注的翻译     add on 20181108 by Vince
        /// </summary>
        /// <param name="type"></param>
        /// <param name="remark"></param>
        public string getMaterialRemarkTrans(string type, string remark)
        {
            switch (type)
            {
                case "Indoor":
                    remark = trans.getTypeTransStr(TransType.Indoor.ToString(), remark);
                    break;
                case "FreshAir":
                    remark = trans.getTypeTransStr(TransType.Indoor.ToString(), remark);
                    break;
                case "Accessory":
                    remark = trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), remark);
                    break;
                case "Outdoor":
                    remark = trans.getTypeTransStr(TransType.Series.ToString(), remark);
                    break;
                case "Exchanger":
                    remark = trans.getTypeTransStr(TransType.Series.ToString(), remark);
                    break;
                case "Controller":
                    remark = trans.getTypeTransStr(TransType.Controller.ToString(), remark);
                    break;
            }
            return remark;
        }
        #endregion

        //read value from app.config file
        #region
        public string GetBinDirectoryPath(string AppSettingPath)
        {
            string binDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').ToString();
            binDirectory += AppSettingPath;
            return binDirectory;
        }
        #endregion
    }

    public class AccessorySpc : Accessory
    {
        /// <summary>
        /// 室内机类型
        /// </summary>

        public string _IndoorModel = "";
        public string IndoorModel
        {
            get { return _IndoorModel; }
            set { _IndoorModel = value; }
        }
    }

}
