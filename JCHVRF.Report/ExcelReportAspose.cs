using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCHVRF.Model;
using JCHVRF.BLL;
using Aspose;
using Aspose.Cells;
using System.Data;
using Lassalle.Flow;
using JCBase.UI;
using JCHVRF.MyPipingBLL;
using NextGenPipingBLL = JCHVRF.MyPipingBLL.NextGen;
using JCHVRF.VRFMessage;
using JCHVRF.Const;
using JCBase.Utility;
using System.Windows.Forms;
using System.Drawing;
using JCHVRF.VRFTrans;

namespace JCHVRF.Report
{

    #region Report Word 模板中, 书签文字常量
    class DocBookMark
    {
        public const string ProjectName = "ProjectName";
        public const string DealerName = "DealerName";
        public const string CurrentDate = "CurrentDate";
        public const string SystemName = "SystemName";
        public const string SystemDrawing = "SystemDrawing";
        public const string copyTable = "copyTable";
        public const string pipingTable = "pipingTable";
        // add on 20140926 clh
        public const string SystemWiring = "SystemWiring";
        public const string Controller = "Controller";

        public const string OptionPriceTable = "OptionPriceTable";
    }
    #endregion

    public class ExcelReportAspose
    {
        Dictionary<string, string> dicMaterialDes = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, int>> dicMaterial = new Dictionary<string, Dictionary<string, int>>();
        #region 初始化

        public ExcelReportAspose(Project proj)
        {
            JCBase.Utility.AsposeHelp.InitAsposeExcel();

            this.thisProject = proj;
            this.ReportLogList = new List<string>();
            bll = new ProjectBLL(proj);
        }

        /// <summary>
        /// add on 201508 clh 目前价格只对出口区域开放
        /// </summary>
        /// <returns></returns>
        private bool ShowPrice()
        {
            return CommonBLL.ShowPrice();
        }

        Project thisProject;
        //Project CurrentProject= Project.GetProjectInstance;

        ProjectBLL bll;
        List<string> ReportLogList;

        string utAirflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
        string utArea = SystemSetting.UserSetting.unitsSetting.settingAREA;
        string utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
        string utDimension = SystemSetting.UserSetting.unitsSetting.settingDimension;
        string utLoadIndex = SystemSetting.UserSetting.unitsSetting.settingLOADINDEX;
        string utPower = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        string utTemperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
        string utWeight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
        //string utPressure = Unit.ut_Pressure; //add by axj 20170710
        string utPressure = SystemSetting.UserSetting.unitsSetting.settingESP;   //modified on 20190522 by Vince
        string utPowerInput_Outdoor = "kW";//2015-06-19
        string utMaxOperationPI = "kW";

        Trans trans = new Trans();  //翻译初始化
        #endregion

        #region Excel 文档写入
        public void ExportReportExcel(string templateNamePath, string newFileName)
        {


            Workbook eWorkbook = null;
            Worksheet
                eProjectSheet = null,
                eBuildingSheet = null,
                eIndoorSheet = null,
                eExchangerSheet = null;
            // Worksheet eIndoorOptionSheet = null; 
            Worksheet eFreshAirSheet = null,
                eOutdoorSheet = null,
                eOptionSheet = null;
            Worksheet ePipingSheet = null, eController = null,
                eProductSheet = null;
            // ePricingSheet = null;
            Worksheet eSQSheet = null;
            Range lineRange = null;

            dicMaterial["Indoor"] = new Dictionary<string, int>();
            dicMaterial["FreshAir"] = new Dictionary<string, int>();
            dicMaterial["Outdoor"] = new Dictionary<string, int>();
            dicMaterial["Accessory"] = new Dictionary<string, int>();
            dicMaterial["Controller"] = new Dictionary<string, int>();
            dicMaterial["PipingConnectionKit"] = new Dictionary<string, int>();
            dicMaterial["BranchKit"] = new Dictionary<string, int>();
            dicMaterial["CHBox"] = new Dictionary<string, int>();
            dicMaterial["Exchanger"] = new Dictionary<string, int>();

            DataTable dtHeaderBranch = JCBase.Utility.Util.InsertDsCachForLoad("JCHVRF_Piping_HeaderBranch");
            foreach (DataRow dr in dtHeaderBranch.Rows)
            {
                dicMaterialDes[dr["Model_York"].ToString()] = "Header branch";
                dicMaterialDes[dr["Model_Hitachi"].ToString()] = "Header branch";
            }
            Worksheet eMaterialSheet = null;
            try
            {
                #region 单元格设置与工作表读取设置
                int startRow = 0;
                int startSum = 0;
                eWorkbook = new Workbook(templateNamePath);
                eProjectSheet = eWorkbook.Worksheets["ProjectInfo"];
                eBuildingSheet = eWorkbook.Worksheets["BuildingInfo"];
                eExchangerSheet = eWorkbook.Worksheets["ExchangerList"];
                eIndoorSheet = eWorkbook.Worksheets["IndoorList"];
                // eIndoorOptionSheet = eWorkbook.Worksheets["Indoor Option"]; 
                eFreshAirSheet = eWorkbook.Worksheets["FreashAirList"];
                eOutdoorSheet = eWorkbook.Worksheets["OutdoorList"];
                eOptionSheet = eWorkbook.Worksheets["AccessoryList"];
                ePipingSheet = eWorkbook.Worksheets["PipingList"];
                eController = eWorkbook.Worksheets["Controller"];
                eProductSheet = eWorkbook.Worksheets["ProductList"];
                //     ePricingSheet = eWorkbook.Worksheets["Pricing List"];
                eSQSheet = eWorkbook.Worksheets["SQList"];
                eMaterialSheet = eWorkbook.Worksheets["MaterialList"];
                #endregion

                #region 设置单元格样式
                Aspose.Cells.Style stemp = eWorkbook.Styles[eWorkbook.Styles.Add()];
                stemp.Borders[BorderType.DiagonalDown].LineStyle = CellBorderType.None;
                stemp.Borders[BorderType.DiagonalUp].LineStyle = CellBorderType.None;
                stemp.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                stemp.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                stemp.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                stemp.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                stemp.Borders[BorderType.Vertical].LineStyle = CellBorderType.Thin;
                stemp.Borders[BorderType.Horizontal].LineStyle = CellBorderType.Thin;
                #endregion

                #region Project Info setting
                //标题
                eProjectSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_ProjectInformation"));//ProjectInformation
                eProjectSheet.Cells[3, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_Item"));//Item
                eProjectSheet.Cells[3, 1].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_Content"));//Content

                eProjectSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_PurchaseOrderNO"));//PurchaseOrderNO
                eProjectSheet.Cells[4, 1].PutValue(thisProject.PurchaseOrderNO);

                eProjectSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_ProjectName"));//Name
                eProjectSheet.Cells[5, 1].PutValue(thisProject.Name);

                eProjectSheet.Cells[6, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_SalesRegion"));//SalesRegion
                eProjectSheet.Cells[6, 1].PutValue(thisProject.SubRegionCode);

                eProjectSheet.Cells[7, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_SalesOffice"));//SalesOffice
                eProjectSheet.Cells[7, 1].PutValue(thisProject.SalesOffice);

                eProjectSheet.Cells[8, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_SalesEngineer"));//SalesEngineer
                eProjectSheet.Cells[8, 1].PutValue(thisProject.SalesEngineer);

                eProjectSheet.Cells[9, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_SalesYINO"));//SalesYINO
                eProjectSheet.Cells[9, 1].PutValue(thisProject.SalesYINO);

                eProjectSheet.Cells[10, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_DeliveryRequiredDate"));//DeliveryRequiredDate
                eProjectSheet.Cells[10, 1].PutValue(thisProject.DeliveryRequiredDate.ToString("yyyy-MM-dd"));

                eProjectSheet.Cells[11, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_OrderDate"));//OrderDate
                eProjectSheet.Cells[11, 1].PutValue(thisProject.OrderDate.ToString("yyyy-MM-dd"));

                eProjectSheet.Cells[12, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_SoldTo"));//Customer
                eProjectSheet.Cells[12, 1].PutValue(thisProject.SoldTo);

                eProjectSheet.Cells[13, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_ShipTo"));//ShipTo
                eProjectSheet.Cells[13, 1].PutValue(thisProject.ShipTo);

                eProjectSheet.Cells[14, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_ContractNO"));//ContractNO
                eProjectSheet.Cells[14, 1].PutValue(thisProject.ContractNO);

                eProjectSheet.Cells[15, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_Location"));//Location
                eProjectSheet.Cells[15, 1].PutValue(thisProject.Location);

                eProjectSheet.Cells[16, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_Remarks"));//Remarks
                eProjectSheet.Cells[16, 1].PutValue(thisProject.Remarks);

                eProjectSheet.Cells[17, 0].PutValue(Msg.GetResourceString("Report_Excel_ProjectInfo_Version"));//Version
                eProjectSheet.Cells[17, 1].PutValue(thisProject.Version);

                string sheetName = Msg.GetResourceString("Report_Exce_ProjectInfo");
                eProjectSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                #endregion 

                #region Building Info setting
                //标题
                eBuildingSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_BuildingInformation"));//BuildingInformation
                eBuildingSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_RoomNo"));//Room No
                eBuildingSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_Floor"));//Floor
                eBuildingSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_RoomName"));//Room Name
                eBuildingSheet.Cells[5, 3].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_RoomType"));//Room Type
                eBuildingSheet.Cells[5, 4].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_RoomArea"));//Room Area
                eBuildingSheet.Cells[5, 5].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_CoolingLoad"));//Cooling Load
                eBuildingSheet.Cells[5, 6].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_SensibleHeatLoad"));//Sensible Heat Load
                eBuildingSheet.Cells[5, 7].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_HeatingLoad"));//Heating Load
                eBuildingSheet.Cells[5, 8].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_CirculatoryAirflowRequirement"));//Circulatory Airflow Requirement
                eBuildingSheet.Cells[5, 9].PutValue(Msg.GetResourceString("Report_Excel_BuildingInfo_FreshAirflowRequirement"));//Fresh Airflow Requirement

                startRow = 6;
                eBuildingSheet.Cells[startRow, 4].PutValue(utArea);
                eBuildingSheet.Cells[startRow, 5].PutValue(utPower);
                eBuildingSheet.Cells[startRow, 6].PutValue(utPower);
                eBuildingSheet.Cells[startRow, 7].PutValue(utPower);
                eBuildingSheet.Cells[startRow, 8].PutValue(utAirflow);
                eBuildingSheet.Cells[startRow, 9].PutValue(utAirflow);
                DataTable roomTable = GetExcelData_RoomList().ToTable();
                startRow = 7;
                for (int i = 0; i < roomTable.Rows.Count; i++)
                {
                    for (int j = 0; j < roomTable.Columns.Count; j++)
                    {
                        string zhi = roomTable.Rows[i][j].ToString();

                        if (j >= 4 && j <= 9)
                        {
                            double ss = 0;
                            if (double.TryParse(zhi, out ss))
                            {
                                eBuildingSheet.Cells[startRow, j].PutValue(ss);
                            }
                            else
                            {
                                eBuildingSheet.Cells[startRow, j].PutValue(zhi);
                            }
                        }
                        else
                        {
                            eBuildingSheet.Cells[startRow, j].PutValue(zhi);
                        }
                    }
                    startRow++;
                }
                if (startRow >7)
                {
                    eBuildingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_Excel_Sum"));
                    eBuildingSheet.Cells[startRow, 4].Formula = "=SUM(E8:E" + (startRow).ToString() + ")";
                    eBuildingSheet.Cells[startRow, 5].Formula = "=SUM(F8:F" + (startRow).ToString() + ")";
                    eBuildingSheet.Cells[startRow, 6].Formula = "=SUM(G8:G" + (startRow).ToString() + ")";
                    eBuildingSheet.Cells[startRow, 7].Formula = "=SUM(H8:H" + (startRow).ToString() + ")";
                    eBuildingSheet.Cells[startRow, 8].Formula = "=SUM(I8:I" + (startRow).ToString() + ")";
                    eBuildingSheet.Cells[startRow, 9].Formula = "=SUM(J8:J" + (startRow).ToString() + ")";

                    lineRange = eBuildingSheet.Cells.CreateRange(7, 0, startRow - 6, roomTable.Columns.Count);
                    lineRange.SetStyle(stemp);
                }
                sheetName = Msg.GetResourceString("Report_Exce_BuildingInfo");
                eBuildingSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eBuildingSheet.Name = Msg.GetResourceString("Report_Exce_BuildingInfo");
                if (!thisProject.IsRoomInfoChecked)
                {
                    eBuildingSheet.VisibilityType = VisibilityType.VeryHidden;
                }
                #endregion 

                #region  Indoor List setting

                //标题
                eIndoorSheet.Cells[0, 3].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_IndoorUnitInformation"));//IndoorUnitInformation
                eIndoorSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_IndoorUnitDetails"));//Indoor Unit Details

                eIndoorSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_RoomNo"));//Room No.
                eIndoorSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_RoomName"));//Room Name
                eIndoorSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Required"));//Required
                eIndoorSheet.Cells[5, 11].PutValue(Msg.GetResourceString("Report_Excel_IndoorName"));//IndoorName
                eIndoorSheet.Cells[5, 12].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_SelectedModel"));//Selected Model
                eIndoorSheet.Cells[5, 13].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Qty"));//Qty
                eIndoorSheet.Cells[5, 14].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Actual"));//Actual
                eIndoorSheet.Cells[5, 19].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Remark"));//Remark
                eIndoorSheet.Cells[6, 2].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingDB"));//CoolingDB
                eIndoorSheet.Cells[6, 3].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingWB"));//CoolingWB
                eIndoorSheet.Cells[6, 4].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_HeatingDB"));//HeatingDB
                eIndoorSheet.Cells[6, 5].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_RH"));//RH
                eIndoorSheet.Cells[6, 6].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingLoad"));//CoolingLoad
                eIndoorSheet.Cells[6, 7].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_HeatingLoad"));//HeatingLoad
                eIndoorSheet.Cells[6, 8].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingSensibleLoad"));//CoolingSensibleLoad
                eIndoorSheet.Cells[6, 9].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CirculatoryAirflow"));//Circulatory Airflow
                eIndoorSheet.Cells[6, 10].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_StaticPressure"));//StaticPressure
                eIndoorSheet.Cells[6, 14].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingCapacity"));//Cooling Capacity
                eIndoorSheet.Cells[6, 15].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_HeatingCapacity"));//Heating Capacity
                eIndoorSheet.Cells[6, 16].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingSensibleCapacity"));//Cooling Sensible Capacity
                eIndoorSheet.Cells[6, 17].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_R_CirculatoryAirflow"));//RCirculatoryAirflow
                eIndoorSheet.Cells[6, 18].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_StaticPressure"));//StaticPressure



                //单位赋值
                startRow = 7;
                eIndoorSheet.Cells[startRow, 6].PutValue(utPower);
                eIndoorSheet.Cells[startRow, 7].PutValue(utPower);
                eIndoorSheet.Cells[startRow, 8].PutValue(utPower);
                eIndoorSheet.Cells[startRow, 9].PutValue(utAirflow);
                eIndoorSheet.Cells[startRow, 10].PutValue(utPressure);
                eIndoorSheet.Cells[startRow, 13].PutValue("Pc");
                eIndoorSheet.Cells[startRow, 14].PutValue(utPower);
                eIndoorSheet.Cells[startRow, 15].PutValue(utPower);
                eIndoorSheet.Cells[startRow, 16].PutValue(utPower);
                eIndoorSheet.Cells[startRow, 17].PutValue(utAirflow);
                eIndoorSheet.Cells[startRow, 18].PutValue(utPressure);

                //数据赋值
                startRow = 8;
                bool isMsg = false;
                for (int i = 0; i < thisProject.SystemListNextGen.Count; i++)
                {
                    //if (!valSystemExport(thisProject.SystemListNextGen[i]))
                    //{
                    //    continue;
                    //}
                    //if (thisProject.SystemListNextGen[i].OutdoorItem == null)
                    //{
                    //    continue;
                    //}                    
                    JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[i];
                    if (sysItem.IsExportToReport)
                    {
                        DataTable indoorData = GetExcelData_InSelectionList(thisProject.SystemListNextGen[i], out isMsg).ToTable();
                        if (indoorData.Rows.Count > 0)
                        {
                            //标题  (Out1[JVOH-8HP]		
                            eIndoorSheet.Cells[startRow, 0].PutValue(thisProject.SystemListNextGen[i].Name + "[" + (thisProject.SystemListNextGen[i].OutdoorItem.AuxModelName == null ? " " : thisProject.SystemListNextGen[i].OutdoorItem.AuxModelName) + "]"); // .ModelFull
                            startRow++;
                            startSum = startRow;

                            //list数据
                            for (int j = 0; j < indoorData.Rows.Count; j++)
                            {
                                for (int k = 0; k < indoorData.Columns.Count; k++)
                                {
                                    string zhi = indoorData.Rows[j][k].ToString();
                                    if ((k >= 6 && k <= 9) || (k >= 14 && k <= 19))
                                    {
                                        double tempd = 0;
                                        if (double.TryParse(zhi, out tempd))
                                        {
                                            eIndoorSheet.Cells[startRow, k + 0].PutValue(tempd);
                                        }
                                        else
                                        {
                                            eIndoorSheet.Cells[startRow, k + 0].PutValue(zhi);
                                        }
                                    }
                                    else
                                    {
                                        eIndoorSheet.Cells[startRow, k + 0].PutValue(zhi);
                                    }
                                }
                                //var drIndoor = indoorData.Rows[j];
                                //FillMaterialData(dicMaterial["Indoor"], drIndoor[Name_Common.Model].ToString(), Convert.ToInt32(drIndoor[Name_Common.Qty].ToString()));
                                startRow++;
                            }

                            //合计数据
                            if (startRow > startSum)
                            {
                                eIndoorSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_Excel_Sum"));
                                eIndoorSheet.Cells[startRow, 6].Formula = "=SUM(G" + startSum.ToString() + ":G" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 7].Formula = "=SUM(H" + startSum.ToString() + ":H" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 8].Formula = "=SUM(I" + startSum.ToString() + ":I" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 9].Formula = "=SUM(J" + startSum.ToString() + ":J" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 14].Formula = "=SUM(O" + startSum.ToString() + ":O" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 15].Formula = "=SUM(P" + startSum.ToString() + ":P" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 16].Formula = "=SUM(Q" + startSum.ToString() + ":Q" + (startRow).ToString() + ")";
                                eIndoorSheet.Cells[startRow, 17].Formula = "=SUM(R" + startSum.ToString() + ":R" + (startRow).ToString() + ")";
                            }
                            startRow++;
                        }
                    }
                }
                if (isMsg)
                    eIndoorSheet.Cells[startRow + 1, 0].PutValue("* : " + Msg.GetResourceString("IND_CAPACITY_MSG"));
                if (startRow > 8)
                {
                    lineRange = eIndoorSheet.Cells.CreateRange(8, 0, startRow - 8, 20);
                    lineRange.SetStyle(stemp);
                }
                sheetName = Msg.GetResourceString("Report_Exce_IndoorList");
                eIndoorSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eIndoorSheet.Name = Msg.GetResourceString("Report_Exce_IndoorList");
                if (!thisProject.IsIndoorListChecked)
                {
                    eIndoorSheet.VisibilityType = VisibilityType.VeryHidden;
                }
                #endregion

                #region  Exchanger List setting

                //标题
                eExchangerSheet.Cells[0, 3].PutValue(Msg.GetResourceString("Report_Excel_ExchangerList_ExchangerUnitInformation"));//ExchangerUnitInformation
                eExchangerSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_ExchangerList_ExchangerUnitDetails"));//Exchanger Unit Details

                eExchangerSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_RoomNo"));//Room No.
                eExchangerSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_RoomName"));//Room Name
                eExchangerSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Required"));//Required
                eExchangerSheet.Cells[5, 10].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_SelectedModel"));//Selected Model
                eExchangerSheet.Cells[5, 11].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Qty"));//Qty
                eExchangerSheet.Cells[5, 12].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Actual"));//Actual
                eExchangerSheet.Cells[5, 16].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_Remark"));//Remark
                eExchangerSheet.Cells[6, 2].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingDB"));//CoolingDB
                eExchangerSheet.Cells[6, 3].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingWB"));//CoolingWB
                eExchangerSheet.Cells[6, 4].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_HeatingDB"));//HeatingDB
                eExchangerSheet.Cells[6, 5].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_RH"));//RH
                eExchangerSheet.Cells[6, 6].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingLoad"));//CoolingLoad
                eExchangerSheet.Cells[6, 7].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_HeatingLoad"));//HeatingLoad
                eExchangerSheet.Cells[6, 8].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingSensibleLoad"));//CoolingSensibleLoad
                eExchangerSheet.Cells[6, 9].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CirculatoryAirflow"));//Circulatory Airflow
                eExchangerSheet.Cells[6, 12].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingCapacity"));//Cooling Capacity
                eExchangerSheet.Cells[6, 13].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_HeatingCapacity"));//Heating Capacity
                eExchangerSheet.Cells[6, 14].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_CoolingSensibleCapacity"));//Cooling Sensible Capacity
                eExchangerSheet.Cells[6, 15].PutValue(Msg.GetResourceString("Report_Excel_IndoorList_R_CirculatoryAirflow"));//RCirculatoryAirflow



                //单位赋值
                startRow = 7;
                eExchangerSheet.Cells[startRow, 6].PutValue(utPower);
                eExchangerSheet.Cells[startRow, 7].PutValue(utPower);
                eExchangerSheet.Cells[startRow, 8].PutValue(utPower);
                eExchangerSheet.Cells[startRow, 9].PutValue(utAirflow);
                eExchangerSheet.Cells[startRow, 11].PutValue("Pc");
                eExchangerSheet.Cells[startRow, 12].PutValue(utPower);
                eExchangerSheet.Cells[startRow, 13].PutValue(utPower);
                eExchangerSheet.Cells[startRow, 14].PutValue(utPower);
                eExchangerSheet.Cells[startRow, 15].PutValue(utAirflow);

                //数据赋值
                startRow = 8;
                DataTable exchangerTable = GetExcelData_ExcSelectionList().ToTable();
                if (exchangerTable.Rows.Count > 0)
                {
                    ////list数据
                    for (int j = 0; j < exchangerTable.Rows.Count; j++)
                    {
                        startSum = startRow;
                        for (int k = 0; k < exchangerTable.Columns.Count; k++)
                        {
                            string zhi = exchangerTable.Rows[j][k].ToString();
                            if ((k >= 6 && k <= 9) || (k >= 12 && k <= 17))
                            {
                                double tempd = 0;
                                if (double.TryParse(zhi, out tempd))
                                {
                                    eExchangerSheet.Cells[startRow, k + 0].PutValue(tempd);
                                }
                                else
                                {
                                    eExchangerSheet.Cells[startRow, k + 0].PutValue(zhi);
                                }
                            }
                            else
                            {
                                eExchangerSheet.Cells[startRow, k + 0].PutValue(zhi);
                            }
                        }
                        var drIndoor = exchangerTable.Rows[j];
                        FillMaterialData(dicMaterial["Exchanger"], drIndoor[Name_Common.Model].ToString(), Convert.ToInt32(drIndoor[Name_Common.Qty].ToString()));

                        startRow++;
                    }

                    //合计数据
                    if (startRow > startSum)
                    {
                        eExchangerSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_Excel_Sum"));
                        eExchangerSheet.Cells[startRow, 6].Formula = "=SUM(G" + startSum.ToString() + ":G" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 7].Formula = "=SUM(H" + startSum.ToString() + ":H" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 8].Formula = "=SUM(I" + startSum.ToString() + ":I" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 9].Formula = "=SUM(J" + startSum.ToString() + ":J" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 12].Formula = "=SUM(M" + startSum.ToString() + ":M" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 13].Formula = "=SUM(N" + startSum.ToString() + ":N" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 14].Formula = "=SUM(O" + startSum.ToString() + ":O" + (startRow).ToString() + ")";
                        eExchangerSheet.Cells[startRow, 15].Formula = "=SUM(P" + startSum.ToString() + ":P" + (startRow).ToString() + ")";
                    }
                    startRow++;

                }

                if (startRow > 8)
                {
                    lineRange = eExchangerSheet.Cells.CreateRange(8, 0, startRow - 8, 17);
                    lineRange.SetStyle(stemp);
                }
                sheetName = Msg.GetResourceString("Report_Exce_ExchangerList");
                eExchangerSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eExchangerSheet.Name = Msg.GetResourceString("Report_Exce_ExchangerList");
                if (!thisProject.IsExchangerChecked)
                {
                    eExchangerSheet.VisibilityType = VisibilityType.VeryHidden;
                }
                else
                {
                    if (exchangerTable.Rows.Count < 1)
                    {
                        eExchangerSheet.VisibilityType = VisibilityType.VeryHidden;
                    }
                }
                #endregion

                #region Indoor Standard Option List setting
                //startRow = 6;
                //DataTable indoorOption = report.GetData_StdOptionBList().ToTable();
                //if (indoorOption.Rows.Count > 0)
                //{
                //    for (int j = 0; j < indoorOption.Rows.Count; j++)
                //    {
                //        for (int k = 0; k < indoorOption.Columns.Count; k++)
                //        {
                //            eIndoorOptionSheet.Cells[startRow, k + 0] .PutValue(indoorOption.Rows[j][k].ToString());
                //        }
                //        startRow++;
                //    }
                //}
                //if (startRow > 6)
                //{
                //    lineRange = eIndoorOptionSheet.Cells.CreateRange(6, 0, startRow-5, 7);
                //    lineRange.SetStyle(stemp);
                //}
                #endregion 

                #region Fresh air List setting

                //标题
                eFreshAirSheet.Cells[0, 2].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_FreshAirUnitInformation"));//FreshAirUnitInformation
                eFreshAirSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_FreshAirUnitDetails"));//Fresh Air Unit Details
                eFreshAirSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_RoomNo"));//Room No.
                eFreshAirSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_RoomName"));//RoomName
                eFreshAirSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_Required"));//Required
                eFreshAirSheet.Cells[5, 5].PutValue(Msg.GetResourceString("Report_Excel_IndoorName"));//IndoorName
                eFreshAirSheet.Cells[5, 6].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_SelectedModel"));//Selected Model
                eFreshAirSheet.Cells[5, 7].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_Qty"));//Qty
                eFreshAirSheet.Cells[5, 8].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_ActualFreshAirflow"));//ActualFreshAirflow
                eFreshAirSheet.Cells[5, 9].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_Remark"));//Remark
                eFreshAirSheet.Cells[6, 2].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_NoofPerson"));//No.of Person
                eFreshAirSheet.Cells[6, 3].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_FreshAir"));//Fresh Air
                eFreshAirSheet.Cells[7, 3].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_Index"));//Index 
                eFreshAirSheet.Cells[7, 4].PutValue(Msg.GetResourceString("Report_Excel_FreashAirList_Airflow"));//Airflow

                startRow = 8;
                eFreshAirSheet.VisibilityType = VisibilityType.Visible;
                eFreshAirSheet.Cells[startRow, 2].PutValue("P");
                eFreshAirSheet.Cells[startRow, 3].PutValue(utAirflow + "*P");
                eFreshAirSheet.Cells[startRow, 4].PutValue(utAirflow);
                eFreshAirSheet.Cells[startRow, 7].PutValue("Pc");
                eFreshAirSheet.Cells[startRow, 8].PutValue(utAirflow);
                startRow = 9;
                bool isFreshAirNone = true;
                for (int i = 0; i < thisProject.SystemList.Count; i++)
                {
                    if (thisProject.SystemList[i].IsExportToReport)
                    {
                        if (!valSystemExport(thisProject.SystemList[i]))
                            continue;

                        if (thisProject.SystemList[i].OutdoorItem == null)
                            continue;

                        DataTable freshData = GetExcelData_InSelectionListFA(thisProject.SystemListNextGen[i]).ToTable();
                        if (freshData.Rows.Count > 0)
                        {
                            isFreshAirNone = false;
                            eFreshAirSheet.Cells[startRow, 0].PutValue(thisProject.SystemListNextGen[i].Name + "[" + thisProject.SystemListNextGen[i].OutdoorItem.AuxModelName + "]");
                            startRow++;
                            startSum = startRow;

                            for (int j = 0; j < freshData.Rows.Count; j++)
                            {
                                for (int k = 0; k < freshData.Columns.Count; k++)
                                {
                                    double d = 0;
                                    if (double.TryParse(freshData.Rows[j][k].ToString(), out d))
                                    {
                                        eFreshAirSheet.Cells[startRow, k + 0].PutValue(d);
                                    }
                                    else
                                    {
                                        eFreshAirSheet.Cells[startRow, k + 0].PutValue(freshData.Rows[j][k].ToString());
                                    }
                                }
                                startRow++;
                                //var drFresh = freshData.Rows[j];
                                //FillMaterialData(dicMaterial["FreshAir"], drFresh[Name_Common.Model].ToString(), Convert.ToInt32(drFresh[Name_Common.Qty].ToString()));
                            }
                            if (startRow > startSum)
                            {
                                eFreshAirSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_Excel_Sum"));
                                eFreshAirSheet.Cells[startRow, 2].Formula = "=SUM(C" + startSum.ToString() + ":C" + (startRow).ToString() + ")";
                                eFreshAirSheet.Cells[startRow, 3].Formula = "=SUM(D" + startSum.ToString() + ":D" + (startRow).ToString() + ")";
                                eFreshAirSheet.Cells[startRow, 4].Formula = "=SUM(E" + startSum.ToString() + ":E" + (startRow).ToString() + ")";
                                eFreshAirSheet.Cells[startRow, 7].Formula = "=SUM(H" + startSum.ToString() + ":H" + (startRow).ToString() + ")";
                            }
                            startRow++;
                        }
                    }
                }

                if (isFreshAirNone)
                {
                    eFreshAirSheet.VisibilityType = VisibilityType.VeryHidden;
                }

                if (startRow > 9)
                {
                    lineRange = eFreshAirSheet.Cells.CreateRange(9, 0, startRow - 8, 10);
                    lineRange.SetStyle(stemp);
                }
                sheetName = Msg.GetResourceString("Report_Exce_FreashAirList");
                eFreshAirSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eFreshAirSheet.Name = Msg.GetResourceString("Report_Exce_FreashAirList");
                #endregion

                #region Outdoor List setting

                //标题
                eOutdoorSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_OutdoorUnitInformation"));//OutdoorUnitInformation
                eOutdoorSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_OutdoorUnitDetails"));//Outdoor Unit Details

                eOutdoorSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_OutdoorName"));//Outdoor Name
                eOutdoorSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_OutdoorUnitModel"));//Outdoor Unit Model
                eOutdoorSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_DOC_OutdoorProductType"));//ProductType
                eOutdoorSheet.Cells[5, 3].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_Required"));//Required

                eOutdoorSheet.Cells[5, 9].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_ActualCoolingCapacity"));//ActualCoolingCapacity
                eOutdoorSheet.Cells[5, 10].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_ActualHeatingCapacity"));//ActualHeatingCapacity
                eOutdoorSheet.Cells[5, 11].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_AdditionalRefrigerantCharge"));//AdditionalRefrigerantCharge
                eOutdoorSheet.Cells[5, 12].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_CapacityRatio"));//CapacityRatio
                eOutdoorSheet.Cells[5, 13].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_Remark"));//Remark


                eOutdoorSheet.Cells[6, 3].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_CoolingDB"));//Cooling DB
                eOutdoorSheet.Cells[6, 4].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_HeatingDB"));//Heating DB
                eOutdoorSheet.Cells[6, 5].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_HeatingWB"));//Heating WB
                eOutdoorSheet.Cells[6, 6].PutValue(Msg.GetResourceString("Report_Excel_OutdoorList_RH"));//RH
                eOutdoorSheet.Cells[6, 7].PutValue(Msg.GetResourceString("Report_DOC_OutSelectionBasic_CoolingInletWater"));//Outdoor Cooling Inlet Water DB
                eOutdoorSheet.Cells[6, 8].PutValue(Msg.GetResourceString("Report_DOC_OutSelectionBasic_HeatingInletWater"));//Outdoor Heating Inlet Water DB


                startRow = 7;
                eOutdoorSheet.Cells[startRow, 9].PutValue(utPower);
                eOutdoorSheet.Cells[startRow, 10].PutValue(utPower);
                eOutdoorSheet.Cells[startRow, 11].PutValue(utWeight);
                startRow = 8;
                for (int i = 0; i < thisProject.SystemListNextGen.Count; i++)
                {
                    //if (!valSystemExport(thisProject.SystemListNextGen[i]))
                    //    continue;

                    //if (thisProject.SystemList[i].OutdoorItem == null)
                    //    continue;
                    if (thisProject.SystemListNextGen[i].IsExportToReport)
                    {
                        DataTable outdoorData = GetExcelData_OutSelection(thisProject.SystemListNextGen[i]).ToTable();
                        for (int j = 0; j < outdoorData.Rows.Count; j++)
                        {
                            for (int k = 0; k < outdoorData.Columns.Count; k++)
                            {
                                if (k > 8 && k <= 10)
                                {
                                    double temp = 0;
                                    string zhi = outdoorData.Rows[j][k].ToString();
                                    if (double.TryParse(zhi, out temp))
                                    {
                                        eOutdoorSheet.Cells[startRow, k].PutValue(temp);
                                    }
                                }
                                else
                                {

                                    if (k == 12)
                                    {
                                        double temp = 0;
                                        string zhi = outdoorData.Rows[j][k].ToString().Replace("%", "");
                                        if (double.TryParse(zhi, out temp))
                                        {
                                            eOutdoorSheet.Cells[startRow, k].PutValue(temp.ToString("F1") + "%");
                                        }
                                        else
                                        {
                                            eOutdoorSheet.Cells[startRow, k].PutValue(outdoorData.Rows[j][k].ToString());
                                        }

                                    }
                                    else
                                    {
                                        eOutdoorSheet.Cells[startRow, k].PutValue(outdoorData.Rows[j][k].ToString());
                                        //判断是否是水机
                                        if (outdoorData.Rows[j][2].ToString().Contains("Water Source"))
                                        {
                                            eOutdoorSheet.Cells[startRow, 3].PutValue("-");
                                            eOutdoorSheet.Cells[startRow, 4].PutValue("-");
                                            eOutdoorSheet.Cells[startRow, 5].PutValue("-");
                                        }
                                        else
                                        {

                                            eOutdoorSheet.Cells[startRow, 7].PutValue("-");
                                            eOutdoorSheet.Cells[startRow, 8].PutValue("-");
                                        }


                                    }
                                }
                            }
                            var drOutdoor = outdoorData.Rows[j];
                            startRow++;
                        }
                        if (thisProject.SystemListNextGen[i].OutdoorItem.AuxModelName != null)
                        {
                            FillMaterialData(dicMaterial["Outdoor"], thisProject.SystemListNextGen[i].OutdoorItem.AuxModelName, 1);
                            dicMaterialDes[thisProject.SystemListNextGen[i].OutdoorItem.AuxModelName] = thisProject.SystemListNextGen[i].OutdoorItem.Series;
                        }
                    }
                }
                if (startRow > 8)
                {
                    eOutdoorSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_Excel_Sum"));
                    eOutdoorSheet.Cells[startRow, 9].Formula = "=SUM(J8:J" + (startRow).ToString() + ")";
                    eOutdoorSheet.Cells[startRow, 10].Formula = "=SUM(K8:K" + (startRow).ToString() + ")";
                    eOutdoorSheet.Cells[startRow, 11].Formula = "=SUM(L8:L" + (startRow).ToString() + ")";
                    //lineRange = eOutdoorSheet.Cells.CreateRange(8, 0, startRow - 7, 13);
                    lineRange = eOutdoorSheet.Cells.CreateRange(8, 0, startRow - 7, 14);//edit axj 2016/12/21 修正列数问题
                    lineRange.SetStyle(stemp);

                    //设置第2列(Outdoor Unit Model)自动换行
                    Style outdoorModelStyle = eOutdoorSheet.Cells[8, 1].GetStyle();
                    outdoorModelStyle.IsTextWrapped = true;
                    lineRange = eOutdoorSheet.Cells.CreateRange(8, 1, startRow - 8, 1);
                    lineRange.SetStyle(outdoorModelStyle);
                }
                sheetName = Msg.GetResourceString("Report_Exce_OutdoorList");
                eOutdoorSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eOutdoorSheet.Name = Msg.GetResourceString("Report_Exce_OutdoorList");
                if (!thisProject.IsOutdoorListChecked)
                {
                    eOutdoorSheet.VisibilityType = VisibilityType.VeryHidden;
                }
                #endregion

                #region AccessoryList
                //标题
                eOptionSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_AccessoryList_AccessoryInformation"));//AccessoryInformation
                eOptionSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_AccessoryList_AccessoryOptionSelection"));//Accessory Option Selection

                startRow = 5;
                eOptionSheet.Cells[startRow, 2].PutValue(Msg.GetResourceString("Report_Option_IndoorModel"));
                eOptionSheet.Cells[startRow, 1].PutValue(Msg.GetResourceString("Report_Option_AccessoryType"));
                eOptionSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_Option_AccessoryModel"));
                eOptionSheet.Cells[startRow, 3].PutValue(Msg.GetResourceString("Report_Option_Qty"));

                startRow = 6;
                DataTable tb = GetOptionInformationList();
                for (int i = 0; i < tb.Rows.Count; i++)
                {
                    eOptionSheet.Cells[startRow, 0].PutValue(tb.Rows[i][0].ToString());
                    eOptionSheet.Cells[startRow, 1].PutValue(tb.Rows[i][1].ToString());
                    eOptionSheet.Cells[startRow, 2].PutValue(tb.Rows[i][2].ToString());
                    eOptionSheet.Cells[startRow, 3].PutValue(tb.Rows[i][3].ToString());
                    startRow++;
                    var drAccessory = tb.Rows[i];
                    //FillMaterialData(dicMaterial["Accessory"], drAccessory["AccessoryModel"].ToString(), Convert.ToInt32(drAccessory["Qty"].ToString()));
                    dicMaterialDes[drAccessory["AccessoryModel"].ToString()] = drAccessory["AccessoryType"].ToString();
                }
                sheetName = Msg.GetResourceString("Report_Exce_AccessoryList");
                eOptionSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eOptionSheet.Name = Msg.GetResourceString("Report_Exce_AccessoryList");
                if (!thisProject.IsIndoorListChecked)
                {
                    eOptionSheet.VisibilityType = VisibilityType.VeryHidden;
                }
                if (startRow > 6)
                {
                    lineRange = eOptionSheet.Cells.CreateRange(6, 0, startRow - 6, 4);
                    lineRange.SetStyle(stemp);
                }
                #endregion

                #region Piping List setting
                //标题
                ePipingSheet.Cells[0, 2].PutValue(Msg.GetResourceString("Report_Excel_PipingList_PipingInformation"));//PipingInformation
                ePipingSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_PipingList_PipingDetails"));//Piping Details
                ePipingSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_PipingList_LiquidPipe"));//Liquid Pipe
                ePipingSheet.Cells[5, 4].PutValue(Msg.GetResourceString("Report_Excel_PipingList_GasPipe"));//Gas Pipe
                ePipingSheet.Cells[6, 0].PutValue(Msg.GetResourceString("Report_Excel_PipingList_PipeDiameter"));//Pipe Diameter
                ePipingSheet.Cells[6, 1].PutValue(Msg.GetResourceString("Report_Excel_PipingList_PipeLength"));//Pipe Length
                ePipingSheet.Cells[6, 2].PutValue(Msg.GetResourceString("Report_Excel_PipingList_JointSkitModel"));//Joint kit Model
                ePipingSheet.Cells[6, 3].PutValue(Msg.GetResourceString("Report_Excel_PipingList_JointkitNumber"));//Joint kit Number
                ePipingSheet.Cells[6, 4].PutValue(Msg.GetResourceString("Report_Excel_PipingList_G_PipeDiameter"));//Pipe Diameter
                ePipingSheet.Cells[6, 5].PutValue(Msg.GetResourceString("Report_Excel_PipingList_G_PipeLength"));//PipeLength
                ePipingSheet.Cells[6, 6].PutValue(Msg.GetResourceString("Report_Excel_PipingList_G_JointkitModel"));//JointkitModel
                ePipingSheet.Cells[6, 7].PutValue(Msg.GetResourceString("Report_Excel_PipingList_G_JointkitNumber"));//JointkitNumber

                //单位赋值
                startRow = 7;
                ePipingSheet.Cells[startRow, 0].PutValue(utDimension);
                ePipingSheet.Cells[startRow, 1].PutValue(utLength);
                ePipingSheet.Cells[startRow, 3].PutValue("Pc");
                ePipingSheet.Cells[startRow, 4].PutValue(utDimension);
                ePipingSheet.Cells[startRow, 5].PutValue(utLength);
                ePipingSheet.Cells[startRow, 7].PutValue("Pc");

                //填充数据
                startRow = 8;
                PipingBLL pipBll = new PipingBLL(thisProject);
                bool isHitachi = thisProject.BrandCode == "H" ? true : false;
                for (int i = 0; i < thisProject.SystemListNextGen.Count; i++)
                {
                    //if (!valSystemExport(thisProject.SystemListNextGen[i]))
                    //    continue;
                    if (thisProject.SystemListNextGen[i].IsExportToReport)
                    {
                        DataTable dataSpecL = null, dataSpecG = null, dataJointKitGNum = null, dataJointKitLNum = null;
                        DataView dvSpecL = null, dvJointKitGNum = null, dvJointKitLNum = null;
                        //SystemVRF sysItem = thisProject.SystemListNextGen[i];
                        JCHVRF.Model.NextGen.SystemVRF sysItem = thisProject.SystemListNextGen[i];
                        dataSpecG = GetData_LinkPipeSpecG(ref sysItem, out dvSpecL).ToTable();
                        dataSpecL = dvSpecL.ToTable();
                        //// Piping Materials 计算加入ODU 的管径 on 20180601 by xyj
                        //GetData_LinkPipe(ref dataSpecG, ref dataSpecL, sysItem, pipBll.GetPipingNodeOutElement(sysItem, isHitachi));

                        dvJointKitGNum = GetData_JointKitNumberList(ref sysItem, out dvJointKitLNum);
                        dataJointKitGNum = dvJointKitGNum.ToTable();
                        dataJointKitLNum = dvJointKitLNum.ToTable();
                        int RowsAddCount1 = Math.Max(dataSpecL.Rows.Count, dataSpecG.Rows.Count);
                        int RowsAddCount2 = Math.Max(dataJointKitLNum.Rows.Count, dataJointKitGNum.Rows.Count);
                        int RowsAddCount = Math.Max(RowsAddCount1, RowsAddCount2);
                        if (RowsAddCount > 0)
                        {
                            ePipingSheet.Cells[startRow, 0].PutValue(thisProject.SystemListNextGen[i].Name);
                            startRow++;

                            for (int l = 0; l < RowsAddCount; l++)
                            {
                                if (l < dataSpecL.Rows.Count)
                                {
                                    string spec = dataSpecL.Rows[l][0].ToString();
                                    if (CommonBLL.IsDimension_inch())
                                        spec = pipBll.GetPipeSize_Inch(spec);

                                    ePipingSheet.Cells[startRow + l, 0].PutValue(spec);

                                    ePipingSheet.Cells[startRow + l, 1].PutValue(dataSpecL.Rows[l][4]);
                                }

                                if (l < dataJointKitLNum.Rows.Count)
                                {
                                    ePipingSheet.Cells[startRow + l, 2].PutValue(dataJointKitLNum.Rows[l][0]);
                                    ePipingSheet.Cells[startRow + l, 3].PutValue(dataJointKitLNum.Rows[l][1]);
                                }

                                if (l < dataSpecG.Rows.Count)
                                {
                                    string spec = dataSpecG.Rows[l][0].ToString();
                                    if (CommonBLL.IsDimension_inch())
                                        spec = pipBll.GetPipeSize_Inch(spec);

                                    ePipingSheet.Cells[startRow + l, 4].PutValue(spec);
                                    ePipingSheet.Cells[startRow + l, 5].PutValue(dataSpecG.Rows[l][4]);
                                }

                                if (l < dataJointKitGNum.Rows.Count)
                                {
                                    ePipingSheet.Cells[startRow + l, 6].PutValue(dataJointKitGNum.Rows[l][0]);
                                    ePipingSheet.Cells[startRow + l, 7].PutValue(dataJointKitGNum.Rows[l][1]);
                                }
                            }
                        }
                        startRow = startRow + RowsAddCount;
                    }
                }
                if (startRow > 8)
                {
                    lineRange = ePipingSheet.Cells.CreateRange(8, 0, startRow - 7, 8);
                    lineRange.SetStyle(stemp);
                }

                //DataTable T_PipingKitTable = pipBll.GetPipingKitTable();
                DataTable T_PipingKitTable = new DataTable();
                List<dynamic> listPipingConnectionKit = new List<dynamic>();
                List<dynamic> listCHBox = new List<dynamic>();
                List<dynamic> listBranchKit = new List<dynamic>();
                if (JCBase.Utility.Util.DsCach.Tables["T_PipingKitTable"] != null && JCBase.Utility.Util.DsCach.Tables["T_PipingKitTable"].Rows.Count > 0)
                {for(int j=0;j< thisProject.SystemListNextGen.Count;j++)
                    { 
                    T_PipingKitTable = JCBase.Utility.Util.DsCach.Tables["T_PipingKitTable"];
                        for (int i = 0; i < T_PipingKitTable.Rows.Count; i++)
                        {
                            DataRow dr = T_PipingKitTable.Rows[i];
                            string type = dr["Type"].ToString();
                            string model = dr["Model"].ToString();
                            string sys = dr["System"].ToString();
                            string sysId = dr["Id"].ToString();
                            if (thisProject.SystemListNextGen[j].Id == sysId && thisProject.SystemListNextGen[j].IsExportToReport)
                            {
                                if (type == "PipingConnectionKit")
                                {
                                    var item = listPipingConnectionKit.Find(m => m.Name == model && m.System == sys);
                                    if (item == null)
                                    {
                                        item = new System.Dynamic.ExpandoObject();
                                        item.Name = model;
                                        item.System = sys;
                                        item.Type = "Outdoor units piping connection kit";
                                        item.Qty = 1;
                                        listPipingConnectionKit.Add(item);
                                    }
                                    else
                                    {
                                        item.Qty++;
                                    }
                                }
                                else if (type == "CHBox")
                                {

                                    var item = listCHBox.Find(m => m.Name == model && m.System == sys);
                                    if (item == null)
                                    {

                                        item = new System.Dynamic.ExpandoObject();
                                        item.Name = model;
                                        item.System = sys;
                                        item.Type = "Cooling/Heating Changeover Box";
                                        item.Qty = 1;
                                        item.Id = sysId;
                                        listCHBox.Add(item);
                                    }
                                    else
                                    {
                                        item.Id = sysId;
                                        item.Qty++;
                                    }
                                }
                                else if (type == "BranchKit")
                                {
                                    var item = listBranchKit.Find(m => m.Name == model && m.System == sys);
                                    if (item == null)
                                    {
                                        item = new System.Dynamic.ExpandoObject();
                                        item.Name = model;
                                        item.System = sys;
                                        item.Type = "Line branch kit";
                                        item.Qty = 1;
                                        item.Id = sysId;
                                        listBranchKit.Add(item);
                                    }
                                    else
                                    {
                                        item.Qty++;
                                    }
                                }
                            }
                        }
                    }
                }
                #region   PipingConnectionKit on 20161021 by Lingjia Qiu
                startRow = startRow + 2;
                ePipingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_DOC_PipingConnectionKit"));//PipingConnectionKit
                int ConnKitStart = ++startRow;
                ePipingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_DOC_Model"));//PipingConnectionKit
                ePipingSheet.Cells[startRow, 1].PutValue(Msg.GetResourceString("Report_DOC_Qty"));//PipingConnectionKit
                startRow++;

                Dictionary<string, string> DataDic = fillkits(T_PipingKitTable, "PipingConnectionKit");
                if (DataDic != null)
                {
                    foreach (var item in DataDic)
                    {
                        //if (item.Key.Equals("Model"))
                        //    ePipingSheet.Cells[startRow, 0].PutValue(item.Value);
                        //else if (item.Key.Equals("Qty"))
                        //    ePipingSheet.Cells[startRow, 1].PutValue(item.Value);
                        //else
                        //    startRow++;

                        ePipingSheet.Cells[startRow, 0].PutValue(item.Key);

                        ePipingSheet.Cells[startRow, 1].PutValue(item.Value);

                        startRow++;
                        //FillMaterialData(dicMaterial["PipingConnectionKit"], item.Key, Convert.ToInt32(item.Value));
                        dicMaterialDes[item.Key] = "Piping connection kit";
                    }

                }
                else
                {
                    ePipingSheet.Cells[startRow, 0].PutValue("-");
                    ePipingSheet.Cells[startRow, 1].PutValue("-");
                    startRow++;
                }
                if (startRow > ConnKitStart)
                {
                    lineRange = ePipingSheet.Cells.CreateRange(ConnKitStart, 0, startRow - ConnKitStart, 2);
                    lineRange.SetStyle(stemp);
                }
                #endregion

                #region   BranchKit on 20161021 by Lingjia Qiu
                startRow++;
                ePipingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_DOC_BranchKit"));//BranchKit
                int BranchKitStart = ++startRow;
                ePipingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_DOC_Model"));//BranchKit
                ePipingSheet.Cells[startRow, 1].PutValue(Msg.GetResourceString("Report_DOC_Qty"));//BranchKit
                startRow++;

                DataDic = fillkits(T_PipingKitTable, "BranchKit");
                if (DataDic != null)
                {
                    foreach (var item in DataDic)
                    {
                        //if (item.Key.Equals("Model"))
                        //    ePipingSheet.Cells[startRow, 0].PutValue(item.Value);
                        //else if (item.Key.Equals("Qty"))
                        //    ePipingSheet.Cells[startRow, 1].PutValue(item.Value);
                        //else
                        //startRow++;
                        ePipingSheet.Cells[startRow, 0].PutValue(item.Key);
                        ePipingSheet.Cells[startRow, 1].PutValue(item.Value);
                        startRow++;
                        //FillMaterialData(dicMaterial["BranchKit"], item.Key, Convert.ToInt32(item.Value));
                        if (!dicMaterialDes.Keys.Contains(item.Key))
                        {
                            dicMaterialDes[item.Key] = "Multi kit";
                        }
                    }

                }
                else
                {
                    ePipingSheet.Cells[startRow, 0].PutValue("-");
                    ePipingSheet.Cells[startRow, 1].PutValue("-");
                    startRow++;
                }
                if (startRow > BranchKitStart)
                {
                    lineRange = ePipingSheet.Cells.CreateRange(BranchKitStart, 0, startRow - BranchKitStart, 2);
                    lineRange.SetStyle(stemp);
                }
                #endregion


                #region   CHBox on 20161021 by Lingjia Qiu
                startRow++;
                ePipingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_DOC_CHBox"));//CHBox
                int CHBoxStart = ++startRow;
                ePipingSheet.Cells[startRow, 0].PutValue(Msg.GetResourceString("Report_DOC_Model"));//CHBox
                ePipingSheet.Cells[startRow, 1].PutValue(Msg.GetResourceString("Report_DOC_Qty"));//CHBox
                startRow++;

                DataDic = fillkits(T_PipingKitTable, "CHBox");
                if (DataDic != null)
                {
                    foreach (var item in DataDic)
                    {
                        //if (item.Key.Equals("Model"))
                        //    ePipingSheet.Cells[startRow, 0].PutValue(item.Value);
                        //else if (item.Key.Equals("Qty"))
                        //    ePipingSheet.Cells[startRow, 1].PutValue(item.Value);
                        //else
                        //    startRow++;
                        ePipingSheet.Cells[startRow, 0].PutValue(item.Key);

                        ePipingSheet.Cells[startRow, 1].PutValue(item.Value);

                        startRow++;
                        //FillMaterialData(dicMaterial["CHBox"], item.Key, Convert.ToInt32(item.Value));
                        dicMaterialDes[item.Key] = "CH-Box";
                    }

                }
                else
                {
                    ePipingSheet.Cells[startRow, 0].PutValue("-");
                    ePipingSheet.Cells[startRow, 1].PutValue("-");
                    startRow++;
                }
                if (startRow > CHBoxStart)
                {
                    lineRange = ePipingSheet.Cells.CreateRange(CHBoxStart, 0, startRow - CHBoxStart, 2);
                    lineRange.SetStyle(stemp);
                }
                #endregion

                sheetName = Msg.GetResourceString("Report_Exce_PipingList");
                ePipingSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //ePipingSheet.Name = Msg.GetResourceString("Report_Exce_PipingList");
                if (!thisProject.IsPipingDiagramChecked)
                {
                    ePipingSheet.VisibilityType = VisibilityType.VeryHidden;
                }
                #endregion 

                #region Controller Material List 

                //标题
                eController.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_Controller_Information"));//Information
                eController.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_Controller_Details"));//Details


                eController.Cells[5, 0].PutValue(Msg.GetResourceString("Report_DOC_ChapterControl_Type"));//Information
                eController.Cells[5, 1].PutValue(Msg.GetResourceString("Report_DOC_ChapterControl_Qty"));//Details
                eController.Cells[5, 2].PutValue(Msg.GetResourceString("Report_DOC_ChapterControl_Des"));//Information

                startRow = 6;
                DataTable dt = GetData_ControllerMaterial().ToTable();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; ++i)
                    {
                        eController.Cells[startRow, 0].PutValue(dt.Rows[i][0].ToString());
                        eController.Cells[startRow, 1].PutValue(dt.Rows[i][1].ToString());
                        eController.Cells[startRow, 2].PutValue(dt.Rows[i][2].ToString());
                        startRow++;
                        DataRow dr = dt.Rows[i];
                        FillMaterialData(dicMaterial["Controller"], dr[Name_Common.Model].ToString(), Convert.ToInt32(dr[Name_Common.Qty].ToString()));
                        dicMaterialDes[dr[Name_Common.Model].ToString()] = dr[Name_Common.Description].ToString();
                    }
                }
                sheetName = Msg.GetResourceString("Report_Exce_ControllerMaterialList");
                eController.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eController.Name = Msg.GetResourceString("Report_Exce_ControllerMaterialList").Trim();
                if (!thisProject.IsControllerChecked)
                {
                    eController.VisibilityType = VisibilityType.VeryHidden;
                }
                #endregion 

                #region Product List setting

                //标题
                eProductSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_ProductList_ProductInformation"));//ProductInformation
                eProductSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_ProductList_Unit"));//Unit
                eProductSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_ProductList_Qty"));//Qty
                eProductSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_ProductList_RatedCoolingCapacity"));//Rated Cooling Capacity
                eProductSheet.Cells[5, 3].PutValue(Msg.GetResourceString("Report_Excel_ProductList_RatedHeatingCapacity"));//Rated Heating Capacity
                eProductSheet.Cells[5, 4].PutValue(Msg.GetResourceString("Report_Excel_ProductList_PowerInput"));//Power Input
                eProductSheet.Cells[5, 5].PutValue(Msg.GetResourceString("Report_Excel_ProductList_MaxOperationPI"));//Max Operation PI
                eProductSheet.Cells[5, 6].PutValue(Msg.GetResourceString("Report_Excel_ProductList_AirFlow"));//Air Flow
                eProductSheet.Cells[5, 7].PutValue(Msg.GetResourceString("Report_Excel_ProductList_Length"));//Length
                eProductSheet.Cells[5, 8].PutValue(Msg.GetResourceString("Report_Excel_ProductList_Width"));//Width
                eProductSheet.Cells[5, 9].PutValue(Msg.GetResourceString("Report_Excel_ProductList_High"));//High
                eProductSheet.Cells[5, 10].PutValue(Msg.GetResourceString("Report_Excel_ProductList_Weight"));//Weight
                eProductSheet.Cells[5, 11].PutValue(Msg.GetResourceString("Report_Excel_ProductList_Remark"));//Remark

                startRow = 6;
                eProductSheet.Cells[startRow, 2].PutValue(utPower);
                eProductSheet.Cells[startRow, 3].PutValue(utPower);
                eProductSheet.Cells[startRow, 4].PutValue(utPowerInput_Outdoor);
                eProductSheet.Cells[startRow, 5].PutValue(utMaxOperationPI);
                eProductSheet.Cells[startRow, 6].PutValue(utAirflow);
                eProductSheet.Cells[startRow, 7].PutValue(utDimension);
                eProductSheet.Cells[startRow, 8].PutValue(utDimension);
                eProductSheet.Cells[startRow, 9].PutValue(utDimension);
                eProductSheet.Cells[startRow, 10].PutValue(utWeight);

                //数据
                startRow = 7;
                DataTable indoorProduct = GetExcelData_InProductList().ToTable();
                indoorProduct.Columns.Remove(Name_Common.ProductType);//add axj 2016/12/21 修正产品列表导出错位
                if (indoorProduct.Rows.Count > 0)
                {
                    eProductSheet.Cells[startRow, 0].PutValue("Indoor Unit");
                    startRow++;
                    for (int j = 0; j < indoorProduct.Rows.Count; j++)
                    {
                        for (int k = 0; k < indoorProduct.Columns.Count; k++)
                        {
                            eProductSheet.Cells[startRow, k + 0].PutValue(indoorProduct.Rows[j][k].ToString());
                        }
                        startRow++;
                    }
                }

                DataTable outdoorProduct = GetExcelData_OutProductList().ToTable();
                outdoorProduct.Columns.Remove(Name_Common.ProductType);//add axj 2016/12/21 修正产品列表导出错位
                if (outdoorProduct.Rows.Count > 0)
                {
                    eProductSheet.Cells[startRow, 0].PutValue("Outdoor Unit");
                    startRow++;
                    for (int j = 0; j < outdoorProduct.Rows.Count; j++)
                    {
                        for (int k = 0; k < outdoorProduct.Columns.Count; k++)
                        {
                            eProductSheet.Cells[startRow, k + 0].PutValue(outdoorProduct.Rows[j][k].ToString());
                        }
                        startRow++;
                    }
                }

                DataTable exchangerProduct = GetExcelData_ExcProductList().ToTable();
                exchangerProduct.Columns.Remove(Name_Common.ProductType);//add xyj 2017/07/19  
                if (exchangerProduct.Rows.Count > 0)
                {
                    eProductSheet.Cells[startRow, 0].PutValue("Total Heat Exchanger Unit");
                    startRow++;
                    for (int j = 0; j < exchangerProduct.Rows.Count; j++)
                    {
                        for (int k = 0; k < exchangerProduct.Columns.Count; k++)
                        {
                            eProductSheet.Cells[startRow, k + 0].PutValue(exchangerProduct.Rows[j][k].ToString());
                        }
                        startRow++;
                    }
                }


                if (startRow > 7)
                {
                    lineRange = eProductSheet.Cells.CreateRange(7, 0, startRow - 7, 12);
                    lineRange.SetStyle(stemp);
                }
                sheetName = Msg.GetResourceString("Report_Exce_ProductList");
                eProductSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eProductSheet.Name = Msg.GetResourceString("Report_Exce_ProductList");
                #endregion 

                #region SQList
                //标题
                eSQSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_SQList_SpecialQuoteInformation"));//Special Quote Information
                eSQSheet.Cells[4, 0].PutValue(Msg.GetResourceString("Report_Excel_SQList_NonStandardProductList"));//Non-Standard Product List
                eSQSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_SQList_SQName"));//SQ Name
                eSQSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_SQList_SQDescription"));//SQ Description
                eSQSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_SQList_Qty"));//Qty
                eSQSheet.Cells[5, 3].PutValue(Msg.GetResourceString("Report_Excel_SQList_TotalPrice"));//Total Price
                eSQSheet.Cells[5, 4].PutValue(Msg.GetResourceString("Report_Excel_SQList_Remark"));//Remark
                eSQSheet.Name = Msg.GetResourceString("Report_Exce_SQList");
                #endregion

                #region MaterialList
                //标题
                eMaterialSheet.Cells[0, 1].PutValue(Msg.GetResourceString("Report_Excel_MaterialList_MaterialInformation"));//Special Quote Information
                eMaterialSheet.Cells[5, 0].PutValue(Msg.GetResourceString("Report_Excel_MaterialList_Model"));//SQ Name
                //eMaterialSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_MaterialList_Sys"));//system name
                eMaterialSheet.Cells[5, 1].PutValue(Msg.GetResourceString("Report_Excel_MaterialList_Remark"));//Remark
                eMaterialSheet.Cells[5, 2].PutValue(Msg.GetResourceString("Report_Excel_MaterialList_Qty"));//Qty

                sheetName = Msg.GetResourceString("Report_Exce_MaterialList");
                eMaterialSheet.Name = sheetName.Length < 32 ? sheetName : sheetName.Substring(0, 31);
                //eMaterialSheet.Name = Msg.GetResourceString("Report_Exce_MaterialList");
                startRow = 6;
                foreach (var type in dicMaterial)
                {
                    foreach (var item in type.Value)
                    {
                        string key = item.Key;
                        string model = "", sys = "";
                        if (key.Contains("$#$"))
                        {
                            model = key.Substring(0, key.IndexOf("$#$"));
                            sys = key.Replace(model + "$#$", "");
                        }
                        else
                        {
                            model = key;
                            sys = "-";
                        }
                        eMaterialSheet.Cells[startRow, 0].PutValue(model);
                        //eMaterialSheet.Cells[startRow, 1].PutValue(sys);
                        eMaterialSheet.Cells[startRow, 2].PutValue(item.Value);

                        string remark = "";
                        if (dicMaterialDes.Keys.Contains(model))
                        {
                            remark = dicMaterialDes[model];
                        }
                        eMaterialSheet.Cells[startRow, 1].PutValue(remark);
                        startRow++;
                    }
                }
                if (startRow > 6)
                {
                    //lineRange = eMaterialSheet.Cells.CreateRange(6, 0, startRow - 6, 4);
                    lineRange = eMaterialSheet.Cells.CreateRange(6, 0, startRow - 6, 3);
                    lineRange.SetStyle(stemp);
                }
                #endregion


                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(newFileName)) == false)
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(newFileName));
                }

                //保存文件
                eWorkbook.CalculateFormula();
                eWorkbook.Save(newFileName);
                eWorkbook.Worksheets[0].IsSelected = true;

                bool openFile = false;
                if (JCMsg.ShowConfirmYesNoCancel(Msg.GetResourceString("ViewReport")) == DialogResult.Yes)
                {
                    openFile = true;
                }

                if (openFile)
                {
                    try
                    {
                        if (System.IO.File.Exists(newFileName))
                        {
                            System.Diagnostics.Process.Start(newFileName);
                        }

                    }
                    catch (Exception ex)
                    {
                        JCMsg.ShowErrorOK(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                //As Per Bug 4711
                MessageBox.Show("File Already Open, Please close It.");
                //JCMsg.ShowErrorOK(ex.Message);
                return;
                //As Per Bug 4711
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

        /// <summary>
        /// 得到当前项目（Project类）中的所有的Room记录
        /// </summary>
        /// <returns></returns>
        private DataView GetExcelData_RoomList()
        {
            string[] ColNameArray_Room =
            {
                RptColName_Room.RoomNO,
                RptColName_Room.FloorNO,
                RptColName_Room.RoomName,
                RptColName_Room.RoomType,
                RptColName_Room.RoomArea,
                RptColName_Room.RLoad,
                RptColName_Room.RSensibleHeatLoad,
                RptColName_Room.RLoadHeat,
                RptColName_Room.RAirFlow,
                RptColName_Room.FreshAir
            };

            DataTable dt = Util.InitDataTable(ColNameArray_Room);

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
                    dr[RptColName_Room.RoomName] = r.Name;
                    dr[RptColName_Room.RoomType] = r.Type;

                    //2013-04-11 Update
                    dr[RptColName_Room.FloorNO] = "Building" + f.ParentId.ToString() + ":" + f.Name;
                    //2013-04-11 Update End

                    //Update on 20120105 clh (以此为例，对所有数据实现根据当前项目的公英制进行转换)
                    dr[RptColName_Room.RoomArea] = Unit.ConvertToControl(r.Area, UnitType.AREA, utArea).ToString("n1");
                    dr[RptColName_Room.RLoad] = Unit.ConvertToControl(r.RqCapacityCool, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Room.RLoadHeat] = Unit.ConvertToControl(r.RqCapacityHeat, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Room.RSensibleHeatLoad] =
                        r.IsSensibleHeatEnable ? Unit.ConvertToControl(r.SensibleHeat, UnitType.POWER, utPower).ToString("n1") : "-";
                    dr[RptColName_Room.RAirFlow] =
                        r.IsAirFlowEnable ? Unit.ConvertToControl(r.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0") : "-";
                    dr[RptColName_Room.FreshAir] = Unit.ConvertToControl(r.FreshAir, UnitType.AIRFLOW, utAirflow).ToString("n0");
                    // 20120105 Update End 

                    dt.Rows.Add(dr);
                }
            }
            return dt.DefaultView;
        }


        /// <summary>
        /// 得到指定系统中的Indoor Selection记录
        /// </summary>
        /// <returns></returns>
        public DataView GetExcelData_InSelectionList(JCHVRF.Model.NextGen.SystemVRF sysItem, out bool isMsg)
        {
            isMsg = false;
            string[] ColNameArray_InSel =
            {
                RptColName_Room.RoomNO,
                RptColName_Room.RoomName,
                Name_Common.CoolingDB,
                Name_Common.CoolingWB,
                Name_Common.HeatingWB,
                Name_Common.RH,
                RptColName_Room.RLoad,
                RptColName_Room.RLoadHeat,
                RptColName_Room.RSensibleHeatLoad,
                RptColName_Room.RAirFlow,
                RptColName_Room.RStaticPressure,
                RptColName_Unit.IndoorName,
                Name_Common.Model,
                Name_Common.Qty,
                RptColName_Unit.ActualCapacity,
                RptColName_Unit.ActualCapacityHeat,
                RptColName_Unit.ActualSensibleCapacity,
                Name_Common.AirFlow,
                RptColName_Unit.StaticPressure,
                Name_Common.Remark
            };

            DataTable dt = Util.InitDataTable(ColNameArray_InSel);

            // 此处与旧版VRF列排列格式有不同，新版VRF中将Cooling与Heating的Capacity值分两列显示，旧版仅显示一列
            List<RoomIndoor> list =
                bll.GetSelectedIndoorBySystem(sysItem.Id, IndoorType.Indoor); //获取分配给指定系统的室内机记录

            foreach (RoomIndoor ri in list)
            {
                DataRow dr = dt.NewRow();
                Room room = bll.GetRoom(ri.RoomID);
                double rLoad = 0;
                double rLoadH = 0;

                dr[Name_Common.CoolingDB] = Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                dr[Name_Common.CoolingWB] = Unit.ConvertToControl(ri.WBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                dr[Name_Common.HeatingWB] = Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");

                FormulaCalculate getRH = new FormulaCalculate();
                dr[Name_Common.RH] = (getRH.GetRH(Convert.ToDecimal(ri.DBCooling), Convert.ToDecimal(ri.WBCooling), getRH.GetPressure(thisProject.Altitude)) * 100).ToString("n0") + "%";
                if (room != null)
                {
                    string strRoomNO = bll.GetRoomNOString(room.Id);
                    dr[RptColName_Room.RoomNO] = strRoomNO;
                    //room.NO;
                    dr[RptColName_Room.RoomName] = room.Name;

                    // 查找是否已存在相同的 Room
                    if (HasSameData(ref dt, strRoomNO, 1)) // update on 20140605 clh
                        continue;

                    rLoad = Unit.ConvertToControl(room.RqCapacityCool, UnitType.POWER, utPower);
                    rLoadH = Unit.ConvertToControl(room.RqCapacityHeat, UnitType.POWER, utPower);
                    dr[RptColName_Room.RLoad] = rLoad.ToString("n1");
                    dr[RptColName_Room.RLoadHeat] = rLoadH.ToString("n1");

                    dr[RptColName_Room.RSensibleHeatLoad] =
                        room.IsSensibleHeatEnable ? Unit.ConvertToControl(room.SensibleHeat, UnitType.POWER, utPower).ToString("n1") : "-";
                    dr[RptColName_Room.RAirFlow] =
                        room.IsAirFlowEnable ? Unit.ConvertToControl(room.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0") : "-";
                    //dr[RptColName_Room.RStaticPressure] = room.IsStaticPressureEnable ? room.StaticPressure.ToString() : "-";
                    dr[RptColName_Room.RStaticPressure] = room.IsStaticPressureEnable ? Unit.ConvertToControl(room.StaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2") : "-";

                }
                else
                {
                    if (!string.IsNullOrEmpty(ri.DisplayRoom))
                    {
                        dr[RptColName_Room.RoomName] = ri.DisplayRoom;
                    }

                    rLoad = Unit.ConvertToControl(ri.RqCoolingCapacity, UnitType.POWER, utPower);
                    rLoadH = Unit.ConvertToControl(ri.RqHeatingCapacity, UnitType.POWER, utPower);
                    dr[RptColName_Room.RLoad] = rLoad.ToString("n1");
                    dr[RptColName_Room.RLoadHeat] = rLoadH.ToString("n1");

                    dr[RptColName_Room.RSensibleHeatLoad] = Unit.ConvertToControl(ri.RqSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Room.RAirFlow] = Unit.ConvertToControl(ri.RqAirflow, UnitType.AIRFLOW, utAirflow).ToString("n1");
                    //dr[RptColName_Room.RStaticPressure] = ri.RqStaticPressure;
                    dr[RptColName_Room.RStaticPressure] = Unit.ConvertToControl(ri.RqStaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
                }
                //dr[RptColName_Unit.StaticPressure] = ri.StaticPressure;
                dr[RptColName_Unit.StaticPressure] = Unit.ConvertToControl(ri.StaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
                // dr[Name_Common.Model] = ri.IndoorItem.Model; //Short ModelName
                string modelName = "";
                if (thisProject.BrandCode == "Y")
                {
                    if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
                        modelName = ri.IndoorItem.Model_York; //NA室内机显示Model_York 20170401
                    else
                        //modelName = ri.IndoorItem.Model; //Short ModelName
                        modelName = ri.IndoorItem.Model_York; //根据PM要求，piping 中的IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
                }
                else if (thisProject.BrandCode == "H")
                    modelName = ri.IndoorItem.Model_Hitachi;
                dr[Name_Common.Model] = modelName;
                dr[RptColName_Unit.IndoorName] = ri.IndoorName;
                //FillMaterialData(dicMaterial["Indoor"], modelName + "$#$" + sysItem.Name, 1);
                FillMaterialData(dicMaterial["Indoor"], modelName, 1);
                //dicMaterialDes[modelName] = ri.IndoorItem.Type;
                dicMaterialDes[modelName] = trans.getTypeTransStr(TransType.Indoor.ToString(), ri.IndoorItem.Type);
                dr[Name_Common.Qty] = "1";

                double inCapHeat;
                double inCapCool = ProjectBLL.GetIndoorActCapacityCool(ri, out inCapHeat);
                dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
                dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

                dr[RptColName_Unit.ActualSensibleCapacity] = Unit.ConvertToControl(ri.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                dr[Name_Common.AirFlow] = Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n1");

                #region AdditionalIndoor Info 一个房间对应多台室内机时，实现按房间汇总显示
                List<RoomIndoor> riList = bll.GetSelectedIndoorByRoom(ri.RoomID, IndoorType.Indoor);
                if (riList != null && riList.Count > 1)
                {
                    foreach (RoomIndoor item in riList)
                    {
                        if (item.IndoorNO != ri.IndoorNO)
                        {
                            //         dr[Name_Common.Model] += ";\n" + item.IndoorItem.Model; //Short ModelName
                            string model = "";
                            if (thisProject.BrandCode == "Y")
                            {
                                if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
                                    model = item.IndoorItem.Model_York; //NA室内机显示Model_York 20170401
                                else
                                    //model = item.IndoorItem.Model; //Short ModelName
                                    model = item.IndoorItem.Model_York; //根据PM要求，piping 中的IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
                            }
                            else if (thisProject.BrandCode == "H")
                                model = item.IndoorItem.Model_Hitachi;
                            dr[Name_Common.Model] += ";\n" + model;
                            dr[RptColName_Unit.IndoorName] += ";\n" + item.IndoorName;
                            //FillMaterialData(dicMaterial["Indoor"], model + "$#$" + sysItem.Name, 1);
                            FillMaterialData(dicMaterial["Indoor"], model, 1);
                            //dicMaterialDes[model] = item.IndoorItem.Type;
                            dicMaterialDes[model] = trans.getTypeTransStr(TransType.Indoor.ToString(), ri.IndoorItem.Type);
                            dr[Name_Common.Qty] += ";\n" + "1";

                            double inCap2H = 0;
                            double inCap2 = ProjectBLL.GetIndoorActCapacityCool(item, out inCap2H);
                            inCapCool += inCap2;
                            inCapHeat += inCap2H;
                            dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
                            dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

                            dr[RptColName_Unit.ActualSensibleCapacity]
                                += ";\n" + Unit.ConvertToControl(item.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                            dr[Name_Common.AirFlow]
                                += ";\n" + Unit.ConvertToControl(item.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                            //dr[RptColName_Unit.StaticPressure] += ";\n" + item.StaticPressure;
                            dr[RptColName_Unit.StaticPressure] += ";\n" + Unit.ConvertToControl(item.StaticPressure, UnitType.STATICPRESSURE, utPressure).ToString("n2");
                        }
                    }
                }
                #endregion

                //Updated on 20120118 -clh- 室内机容量之和与房间（区域）负荷之和相比
                //if (Convert.ToDouble(Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1")) >= Convert.ToDouble(rLoad.ToString("n1")))
                if (CommonBLL.FullMeetRoomRequired(ri, thisProject))
                    dr[Name_Common.Remark] = "OK";
                else
                {
                    if (!isMsg)
                        isMsg = true;
                    dr[Name_Common.Remark] = Msg.IND_WARN_CAPLower;
                }

                dt.Rows.Add(dr);

            }
            return dt.DefaultView;
        }


        /// <summary>
        /// 得到指定系统中的Exchanger Selection记录
        /// </summary>
        /// <returns></returns>
        public DataView GetExcelData_ExcSelectionList()
        {
            // isMsg = false;
            string[] ColNameArray_InSel =
            {
                RptColName_Room.RoomNO,
                RptColName_Room.RoomName,
                Name_Common.CoolingDB,
                Name_Common.CoolingWB,
                Name_Common.HeatingWB,
                Name_Common.RH,
                RptColName_Room.RLoad,
                RptColName_Room.RLoadHeat,
                RptColName_Room.RSensibleHeatLoad,
                RptColName_Room.RAirFlow,
                Name_Common.Model,
                Name_Common.Qty,
                RptColName_Unit.ActualCapacity,
                RptColName_Unit.ActualCapacityHeat,
                RptColName_Unit.ActualSensibleCapacity,
                Name_Common.AirFlow,
                Name_Common.Remark
            };

            DataTable dt = Util.InitDataTable(ColNameArray_InSel);

            if (thisProject.ExchangerList != null)
            {
                // 此处与旧版VRF列排列格式有不同，新版VRF中将Cooling与Heating的Capacity值分两列显示，旧版仅显示一列
                List<RoomIndoor> list = thisProject.ExchangerList; //获取分配给指定系统的室内机记录

                foreach (RoomIndoor ri in list)
                {
                    DataRow dr = dt.NewRow();
                    Room room = bll.GetRoom(ri.RoomID);
                    double rLoad = 0;
                    double rLoadH = 0;

                    dr[Name_Common.CoolingDB] = Unit.ConvertToControl(ri.DBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    dr[Name_Common.CoolingWB] = Unit.ConvertToControl(ri.WBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
                    dr[Name_Common.HeatingWB] = Unit.ConvertToControl(ri.DBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");

                    FormulaCalculate getRH = new FormulaCalculate();
                    dr[Name_Common.RH] = (getRH.GetRH(Convert.ToDecimal(ri.DBCooling), Convert.ToDecimal(ri.WBCooling), getRH.GetPressure(thisProject.Altitude)) * 100).ToString("n0") + "%";
                    if (room != null)
                    {
                        string strRoomNO = bll.GetRoomNOString(room.Id);
                        dr[RptColName_Room.RoomNO] = strRoomNO;
                        //room.NO;
                        dr[RptColName_Room.RoomName] = room.Name;
                        if (HasSameData(ref dt, strRoomNO, 1)) // update on 20140605 clh
                            continue;

                        rLoad = Unit.ConvertToControl(room.RqCapacityCool, UnitType.POWER, utPower);
                        rLoadH = Unit.ConvertToControl(room.RqCapacityHeat, UnitType.POWER, utPower);
                        dr[RptColName_Room.RLoad] = rLoad.ToString("n1");
                        dr[RptColName_Room.RLoadHeat] = rLoadH.ToString("n1");

                        dr[RptColName_Room.RSensibleHeatLoad] =
                            room.IsSensibleHeatEnable ? Unit.ConvertToControl(room.SensibleHeat, UnitType.POWER, utPower).ToString("n1") : "-";
                        dr[RptColName_Room.RAirFlow] =
                            room.IsAirFlowEnable ? Unit.ConvertToControl(room.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0") : "-";

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ri.DisplayRoom))
                        {
                            dr[RptColName_Room.RoomName] = ri.DisplayRoom;
                        }
                        rLoad = Unit.ConvertToControl(ri.RqCoolingCapacity, UnitType.POWER, utPower);
                        rLoadH = Unit.ConvertToControl(ri.RqHeatingCapacity, UnitType.POWER, utPower);
                        dr[RptColName_Room.RLoad] = rLoad.ToString("n1");
                        dr[RptColName_Room.RLoadHeat] = rLoadH.ToString("n1");

                        dr[RptColName_Room.RSensibleHeatLoad] = Unit.ConvertToControl(ri.RqSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                        dr[RptColName_Room.RAirFlow] = Unit.ConvertToControl(ri.RqAirflow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                    }

                    // dr[Name_Common.Model] = ri.IndoorItem.Model; //Short ModelName
                    if (thisProject.BrandCode == "Y")
                    {

                        //dr[Name_Common.Model] = ri.IndoorItem.Model; //Short ModelName
                        dr[Name_Common.Model] = ri.IndoorItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
                    }
                    else if (thisProject.BrandCode == "H")
                        dr[Name_Common.Model] = ri.IndoorItem.Model_Hitachi;

                    //dicMaterialDes[dr[Name_Common.Model].ToString()] = ri.IndoorItem.Type;
                    dicMaterialDes[dr[Name_Common.Model].ToString()] = trans.getTypeTransStr(TransType.Indoor.ToString(), ri.IndoorItem.Type);
                    dr[Name_Common.Qty] = "1";

                    double inCapHeat;
                    double inCapCool = ProjectBLL.GetIndoorActCapacityCool(ri, out inCapHeat);
                    dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

                    dr[RptColName_Unit.ActualSensibleCapacity] = Unit.ConvertToControl(ri.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                    dr[Name_Common.AirFlow] = Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");


                    #region AdditionalExchanger Info 一个房间对应多台全热交换机时，实现按房间汇总显示
                    List<RoomIndoor> riList = bll.GetSelectedExchangerByRoom(ri.RoomID, IndoorType.Exchanger);
                    if (riList != null && riList.Count > 1)
                    {
                        foreach (RoomIndoor item in riList)
                        {
                            if (item.IndoorNO != ri.IndoorNO)
                            {
                                //         dr[Name_Common.Model] += ";\n" + item.IndoorItem.Model; //Short ModelName

                                if (thisProject.BrandCode == "Y")
                                {
                                    //dr[Name_Common.Model] += ";\n" + item.IndoorItem.Model;
                                    dr[Name_Common.Model] += ";\n" + ri.IndoorItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
                                }
                                else if (thisProject.BrandCode == "H")
                                    dr[Name_Common.Model] += ";\n" + item.IndoorItem.Model_Hitachi;

                                //字符串格式无法转换 故累加  on 2017-07-18 by xyj
                                // dr[Name_Common.Qty] += ";\n" + "1";
                                dr[Name_Common.Qty] = Convert.ToInt32(dr[Name_Common.Qty]) + 1;
                                dicMaterialDes[dr[Name_Common.Model].ToString()] = ri.IndoorItem.Series;
                                double inCap2H = 0;
                                double inCap2 = ProjectBLL.GetIndoorActCapacityCool(item, out inCap2H);
                                inCapCool += inCap2;
                                inCapHeat += inCap2H;
                                dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
                                dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

                                dr[RptColName_Unit.ActualSensibleCapacity]
                                    += ";\n" + Unit.ConvertToControl(item.ActualSensibleHeat, UnitType.POWER, utPower).ToString("n1");
                                dr[Name_Common.AirFlow]
                                    += ";\n" + Unit.ConvertToControl(item.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                            }
                        }
                    }
                    #endregion

                    //Updated on 20120118 -clh- 室内机容量之和与房间（区域）负荷之和相比
                    //if (Convert.ToDouble(Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1")) >= Convert.ToDouble(rLoad.ToString("n1")))
                    // if (CommonBLL.FullMeetRoomRequired(ri, thisProject))
                    dr[Name_Common.Remark] = "OK";
                    //else
                    //{
                    // if (!isMsg)
                    //    isMsg = true;
                    //  dr[Name_Common.Remark] = Msg.IND_WARN_CAPLower;
                    //}

                    dt.Rows.Add(dr);

                }
            }
            return dt.DefaultView;
        }

        /// add on 20120511 clh 增加 FA Selection 记录
        /// <summary>
        /// 得到指定系统中的FA Selection记录
        /// add on 20120511 clh for Phase2
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        public DataView GetExcelData_InSelectionListFA(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            string[] ColNameArray_FASel =
            {
                RptColName_Room.RoomNO,
                RptColName_Room.RoomName,
                RptColName_Room.NoOfPerson,
                RptColName_Room.FAIndex,
                RptColName_Room.FreshAir,
                RptColName_Unit.IndoorName,
                Name_Common.Model,
                Name_Common.Qty,
                Name_Common.AirFlow,
                Name_Common.Remark
            };

            DataTable dt = Util.InitDataTable(ColNameArray_FASel);
            List<RoomIndoor> list =
                bll.GetSelectedIndoorBySystem(sysItem.Id, IndoorType.FreshAir);
            foreach (RoomIndoor ri in list)
            {
                DataRow dr = dt.NewRow();
                Room room = bll.GetRoom(ri.RoomID);
                double freshair = 0;
                if (room != null)
                {
                    dr[RptColName_Room.RoomNO] = room.NO;
                    dr[RptColName_Room.RoomName] = room.Name;

                    // 查找是否已存在相同的 Room
                    if (HasSameData(ref dt, room.Name, 2))
                        continue;

                    dr[RptColName_Room.NoOfPerson] = room.PeopleNumber;
                    dr[RptColName_Room.FAIndex] =
                        Unit.ConvertToControl(room.FreshAirIndex, UnitType.AIRFLOW, utAirflow).ToString("n0");
                    freshair = Unit.ConvertToControl(room.FreshAir, UnitType.AIRFLOW, utAirflow);
                    dr[RptColName_Room.FreshAir] = freshair.ToString("n0");
                }
                else
                {
                    freshair = Unit.ConvertToControl(ri.RqFreshAir, UnitType.AIRFLOW, utAirflow);
                    dr[RptColName_Room.FreshAir] = freshair.ToString("n0");
                }

                // dr[Name_Common.Model] = ri.IndoorItem.Model;
                string modelName = "";
                if (thisProject.BrandCode == "Y")
                {
                    if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
                        modelName = ri.IndoorItem.Model_York; //NA室内机显示Model_York 20170401
                    else
                        //modelName = ri.IndoorItem.Model; //Short ModelName
                        modelName = ri.IndoorItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
                }
                else if (thisProject.BrandCode == "H")
                    modelName = ri.IndoorItem.Model_Hitachi;
                dr[Name_Common.Model] = modelName;
                dr[RptColName_Unit.IndoorName] = ri.IndoorName;
                //FillMaterialData(dicMaterial["FreshAir"], modelName + "$#$" + sysItem.Name, 1);
                FillMaterialData(dicMaterial["FreshAir"], modelName, 1);
                //dicMaterialDes[modelName] = ri.IndoorItem.Type;
                dicMaterialDes[modelName] = trans.getTypeTransStr(TransType.Indoor.ToString(), ri.IndoorItem.Type);

                dr[Name_Common.Qty] = "1";

                // 新风机容量
                double inCapHeat;
                double inCapCool = ProjectBLL.GetIndoorActCapacityCool(ri, out inCapHeat);
                //dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
                //dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

                // 新风机风量
                dr[Name_Common.AirFlow] = Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");

                #region AdditionalFreshAir Info 一个房间对应多台新风机时，实现按房间汇总显示
                List<RoomIndoor> riList = bll.GetSelectedIndoorByRoom(ri.RoomID, IndoorType.FreshAir);
                if (riList != null && riList.Count > 1)
                {
                    foreach (RoomIndoor item in riList)
                    {
                        if (item.IndoorNO != ri.IndoorNO)
                        {
                            string model = "";
                            if (thisProject.BrandCode == "Y")
                                if (sysItem.OutdoorItem.Series.Contains("YVAHP") || sysItem.OutdoorItem.Series.Contains("YVAHR"))
                                    model = item.IndoorItem.Model_York;
                                else
                                    //model = item.IndoorItem.Model; //Short ModelName
                                    model = item.IndoorItem.Model_York; //根据PM要求，IDU ODU model name 显示model_York 或者 model_Hitachi 20180214 by Yunxiao lin
                            else if (thisProject.BrandCode == "H")
                                model = item.IndoorItem.Model_Hitachi;
                            dr[Name_Common.Model] += ";\n" + model; //item.IndoorItem.Model;
                            dr[RptColName_Unit.IndoorName] += ";\n" + item.IndoorName;
                            dr[Name_Common.Qty] += ";\n" + "1";
                            //FillMaterialData(dicMaterial["FreshAir"], model + "$#$" + sysItem.Name, 1);
                            FillMaterialData(dicMaterial["FreshAir"], model, 1);
                            //dicMaterialDes[model] = item.IndoorItem.Type;
                            dicMaterialDes[model] = trans.getTypeTransStr(TransType.Indoor.ToString(), item.IndoorItem.Type);
                            double inCap2H = 0;
                            double inCap2 = ProjectBLL.GetIndoorActCapacityCool(item, out inCap2H);
                            inCapCool += inCap2;
                            inCapHeat += inCap2H;
                            dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(inCapCool, UnitType.POWER, utPower).ToString("n1");
                            dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(inCapHeat, UnitType.POWER, utPower).ToString("n1");

                            dr[Name_Common.AirFlow]
                                += ";\n" + Unit.ConvertToControl(item.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                        }
                    }
                }
                #endregion

                //-clh- 新风机风量与新风区域风量需求之和相比
                if (ri.AirFlow >= freshair)
                    dr[Name_Common.Remark] = "OK";
                else
                    dr[Name_Common.Remark] = Msg.IND_WARN_AFLowerFA;
                dt.Rows.Add(dr);
            }
            return dt.DefaultView;
        }

        /// <summary>
        /// 得到指定系统中的Outdoor Selection记录
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        public DataView GetExcelData_OutSelection(JCHVRF.Model.NextGen.SystemVRF sysItem)
        {
            string[] ColNameArray_OutSel =
            {
                Name_Common.Name,
                Name_Common.ModelFull,
                Name_Common.ProductType,
                Name_Common.CoolingDB,
                Name_Common.HeatingDB,
                Name_Common.HeatingWB,
                Name_Common.RH,

                //Alex 20160728
                Name_Common.CoolingInletWater,
                Name_Common.HeatingInletWater,

                RptColName_Unit.ActualCapacity,
                RptColName_Unit.ActualCapacityHeat,
                RptColName_Outdoor.AddRefrigerant,
                RptColName_Outdoor.CapacityRatio,
                Name_Common.Remark
            };
            DataTable dt = Util.InitDataTable(ColNameArray_OutSel);

            DataRow dr = dt.NewRow();
            dr[Name_Common.Name] = sysItem.Name;
            string power = "";
            MyDictionary powerItem = (new MyDictionaryBLL()).GetItem(MyDictionary.DictionaryType.PowerSupply, sysItem.Power);
            if (powerItem != null)
            {
                power = System.Environment.NewLine + "(" + powerItem.Name + ")";
            }
            dr[Name_Common.ModelFull] = sysItem.OutdoorItem.AuxModelName + power;

            if (sysItem.OutdoorItem.Series == null)
            {
                MyProductType pt = (new MyProductTypeBLL()).GetItem(thisProject.BrandCode, thisProject.SubRegionCode, sysItem.OutdoorItem.ProductType);
                //dr[Name_Common.ProductType] = pt.Series;
                dr[Name_Common.ProductType] = trans.getTypeTransStr(TransType.Series.ToString(), pt.Series);
            }
            else
                //dr[Name_Common.ProductType] = sysItem.OutdoorItem.Series;
                dr[Name_Common.ProductType] = trans.getTypeTransStr(TransType.Series.ToString(), sysItem.OutdoorItem.Series);


            dr[Name_Common.CoolingDB] = Unit.ConvertToControl(sysItem.DBCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
            dr[Name_Common.HeatingDB] = Unit.ConvertToControl(sysItem.DBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");
            dr[Name_Common.HeatingWB] = Unit.ConvertToControl(sysItem.WBHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");


            //Alex 2017-0728
            dr[Name_Common.CoolingInletWater] = Unit.ConvertToControl(sysItem.IWCooling, UnitType.TEMPERATURE, utTemperature).ToString("n1");
            dr[Name_Common.HeatingInletWater] = Unit.ConvertToControl(sysItem.IWHeating, UnitType.TEMPERATURE, utTemperature).ToString("n1");

            FormulaCalculate getRH = new FormulaCalculate();
            dr[Name_Common.RH] = (getRH.GetRH(Convert.ToDecimal(sysItem.DBHeating), Convert.ToDecimal(sysItem.WBHeating), getRH.GetPressure(thisProject.Altitude)) * 100).ToString("n0") + "%";

            dr[RptColName_Unit.ActualCapacity] = Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
            dr[RptColName_Unit.ActualCapacityHeat] = Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
            dr[RptColName_Outdoor.AddRefrigerant] =
                Unit.ConvertToControl(sysItem.AddRefrigeration, UnitType.WEIGHT, utWeight).ToString("n1");
            dr[RptColName_Outdoor.CapacityRatio] =
                 (sysItem.Ratio * 100).ToString() + "%";
            dr[Name_Common.Remark] = "OK";
            dt.Rows.Add(dr);
            return dt.DefaultView;
        }

        /// <summary>
        /// 得到该项目中汇总的Indoor Product List
        /// </summary>
        /// <returns></returns>
        public DataView GetExcelData_InProductList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitList_Outdoor); //由于Excel表中室内机室外机的ProductList合并为一张表，以列数多的计算
            for (int i = 0; i < thisProject.SystemListNextGen.Count; i++)
            {
                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    if (!valIndoorExport(ri))
                        continue;

                    if (string.IsNullOrEmpty(ri.SystemID))  // TODO:待确认，没有分配室外机的室内机是否显示？？？20141209
                        continue;
                    if (thisProject.SystemListNextGen[i].Id == ri.SystemID && thisProject.SystemListNextGen[i].IsExportToReport)
                    {
                        // 判断有无重复记录
                        if (ri.IndoorItem == null || HasTableData(ref dt, ri.IndoorItem.Model, 1))
                            //if (ri.IndoorItem == null || HasTableData(ref dt, ri.IndoorItem.Model, 2))
                            continue;

                        // Updated on 20140530 clh
                        string modelAfterOption = ri.IndoorItem.ModelFull;
                        // 若当前选择了Option则此处的ModelName填写选择了Option之后的新型号名
                        if (ri.OptionItem != null)
                        {
                            modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
                            if (HasTableData(ref dt, modelAfterOption, 1))
                                //if (HasTableData(ref dt, modelAfterOption, 2))
                                continue;
                        }
                        else
                        {
                            // 判断有无重复记录
                            if (HasTableData(ref dt, modelAfterOption, 1))
                                //if (HasTableData(ref dt, modelAfterOption, 2))
                                continue;
                        }

                        DataRow dr = dt.NewRow();
                        //  dr[Name_Common.ModelFull] = modelAfterOption;

                        if (thisProject.BrandCode == "Y")
                            dr[Name_Common.ModelFull] = ri.IndoorItem.Model_York;
                        else if (thisProject.BrandCode == "H")
                            dr[Name_Common.ModelFull] = ri.IndoorItem.Model_Hitachi;
                        if (HasTableData(ref dt, dr[Name_Common.ModelFull].ToString(), 0, 2))
                            continue;
                        dr[Name_Common.Qty] = "1";

                        dr[RptColName_Unit.RatedCoolingCapacity] =
                            Unit.ConvertToControl(ri.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                        dr[RptColName_Unit.RatedHeatingCapacity] =
                            Unit.ConvertToControl(ri.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                        // dr[Name_Common.PowerInput] = (ri.IndoorItem.PowerInput_Cooling / 1000).ToString(); //add on 2015-06-19
                        //PowerInput 各区域单位不统一 部分需要转换为kw  on 20181019 by xyj
                        double powerInput = ri.IndoorItem.PowerInput_Cooling;
                        if (!ProjectBLL.IsIDUPowerInputKw(thisProject.RegionCode))
                        {
                            powerInput = Math.Round(powerInput / 1000, 4);
                        }
                        dr[Name_Common.PowerInput] = powerInput;
                        dr[Name_Common.MaxOperationPI] = "";
                        dr[Name_Common.AirFlow] =
                            Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                        dr[Name_Common.Length] = ri.IndoorItem.Length;
                        dr[Name_Common.Width] = ri.IndoorItem.Width;
                        dr[Name_Common.Height] = ri.IndoorItem.Height;
                        dr[Name_Common.Weight] =
                            Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt.DefaultView;
        }

        /// <summary>
        /// 得到该项目中汇总的Exchnager Product List
        /// </summary>
        /// <returns></returns>
        public DataView GetExcelData_ExcProductList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitList_Outdoor); //由于Excel表中室内机室外机的ProductList合并为一张表，以列数多的计算

            if (thisProject.ExchangerList != null)
            {
                foreach (RoomIndoor ri in thisProject.ExchangerList)
                {
                    if (!valIndoorExport(ri))
                        continue;



                    // 判断有无重复记录
                    if (ri.IndoorItem == null || HasTableData(ref dt, ri.IndoorItem.Model, 1))
                        //if (ri.IndoorItem == null || HasTableData(ref dt, ri.IndoorItem.Model, 2))
                        continue;

                    // Updated on 20140530 clh
                    string modelAfterOption = ri.IndoorItem.ModelFull;
                    // 若当前选择了Option则此处的ModelName填写选择了Option之后的新型号名
                    if (ri.OptionItem != null)
                    {
                        modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
                        if (HasTableData(ref dt, modelAfterOption, 1))
                            //if (HasTableData(ref dt, modelAfterOption, 2))
                            continue;
                    }
                    else
                    {
                        // 判断有无重复记录
                        if (HasTableData(ref dt, modelAfterOption, 1))
                            //if (HasTableData(ref dt, modelAfterOption, 2))
                            continue;
                    }

                    DataRow dr = dt.NewRow();
                    //  dr[Name_Common.ModelFull] = modelAfterOption;

                    if (thisProject.BrandCode == "Y")
                        dr[Name_Common.ModelFull] = ri.IndoorItem.Model_York;
                    else if (thisProject.BrandCode == "H")
                        dr[Name_Common.ModelFull] = ri.IndoorItem.Model_Hitachi;
                    if (HasTableData(ref dt, dr[Name_Common.ModelFull].ToString(), 0, 2))
                        continue;
                    dr[Name_Common.Qty] = "1";

                    dr[RptColName_Unit.RatedCoolingCapacity] =
                        Unit.ConvertToControl(ri.IndoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Unit.RatedHeatingCapacity] =
                        Unit.ConvertToControl(ri.IndoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                    //dr[Name_Common.PowerInput] = (ri.IndoorItem.PowerInput_Cooling / 1000).ToString(); //add on 2015-06-19
                    //PowerInput 各区域单位不统一 部分需要转换为kw  on 20181019 by xyj
                    double powerInput = ri.IndoorItem.PowerInput_Cooling;
                    if (!ProjectBLL.IsIDUPowerInputKw(thisProject.RegionCode))
                    {
                        powerInput = powerInput / 1000;
                    }
                    dr[Name_Common.PowerInput] = powerInput;
                    dr[Name_Common.MaxOperationPI] = "";
                    dr[Name_Common.AirFlow] =
                        Unit.ConvertToControl(ri.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                    dr[Name_Common.Length] = ri.IndoorItem.Length;
                    dr[Name_Common.Width] = ri.IndoorItem.Width;
                    dr[Name_Common.Height] = ri.IndoorItem.Height;
                    dr[Name_Common.Weight] =
                        Unit.ConvertToControl(Convert.ToDouble(ri.IndoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                    dt.Rows.Add(dr);
                }
            }
            return dt.DefaultView;
        }



        /// <summary>
        /// 得到该项目中汇总的Outdoor Product List
        /// </summary>
        /// <returns></returns>
        public DataView GetExcelData_OutProductList()
        {
            DataTable dt = Util.InitDataTable(NameArray_Report.ColNameArray_UnitList_Outdoor);

            for (int systemCount = 0; systemCount < thisProject.SystemList.Count; systemCount++)
            {
                if (thisProject.SystemList[systemCount].IsExportToReport)
                {
                    SystemVRF sysItem = thisProject.SystemList[systemCount];
                    if (!valSystemExport(sysItem))
                        continue;

                    //查找此表中是否存在相同的数据
                    if (sysItem.OutdoorItem == null || HasTableDataNew(ref dt, sysItem.OutdoorItem.AuxModelName, 1)) //Model
                                                                                                                     //if (sysItem.OutdoorItem == null || HasTableData(ref dt, sysItem.OutdoorItem.AuxModelName, 2)) //Model
                        continue;
                    DataRow dr = dt.NewRow();

                    //dr[Name_Common.ModelFull] = sysItem.OutdoorItem.AuxModelName; // .ModelFull
                    dr[Name_Common.ModelFull] = sysItem.OutdoorItem.FullModuleName;

                    dr[Name_Common.Qty] = "1";
                    dr[RptColName_Unit.RatedCoolingCapacity] =
                        Unit.ConvertToControl(sysItem.OutdoorItem.CoolingCapacity, UnitType.POWER, utPower).ToString("n1");
                    dr[RptColName_Unit.RatedHeatingCapacity] =
                        Unit.ConvertToControl(sysItem.OutdoorItem.HeatingCapacity, UnitType.POWER, utPower).ToString("n1");
                    dr[Name_Common.PowerInput] = sysItem.OutdoorItem.Power_Cooling;
                    dr[Name_Common.MaxOperationPI] = sysItem.OutdoorItem.MaxOperationPI_Cooling;
                    if (string.IsNullOrEmpty(sysItem.OutdoorItem.MaxOperationPI_Cooling))
                    {
                        //Outdoor item = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType,thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull);
                        // 多productType功能修改 20160823 by Yunxiao Lin
                        Outdoor item = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.ProductType);
                        sysItem.OutdoorItem = item;
                        dr[Name_Common.MaxOperationPI] = sysItem.OutdoorItem.MaxOperationPI_Cooling;
                    }
                    dr[Name_Common.AirFlow] =
                        Unit.ConvertToControl(sysItem.OutdoorItem.AirFlow, UnitType.AIRFLOW, utAirflow).ToString("n0");
                    dr[Name_Common.Length] = sysItem.OutdoorItem.Length;
                    dr[Name_Common.Width] = sysItem.OutdoorItem.Width;
                    dr[Name_Common.Height] = sysItem.OutdoorItem.Height;
                    dr[Name_Common.Weight] =
                        Unit.ConvertToControl(Convert.ToDouble(sysItem.OutdoorItem.Weight), UnitType.WEIGHT, utWeight).ToString("n0");
                    dt.Rows.Add(dr);
                }
            }
            return dt.DefaultView;
            //End
        }

        /// add by axj 20170710 从ExcelReportAspose移植
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

        /// 查找此表中是否存在重复的机组记录，若已存在则对应的Qty列加1
        /// add by axj 20170710 从ExcelReportAspose移植
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

        /// <summary>
        /// 查找此表中是否存在相同的数据
        /// </summary>
        /// <param name="data">要查找的数据表</param>
        /// <param name="modelFullName">要查找的数据</param>
        /// <param name="cIndex">与数据比较的表列</param>
        /// <returns>有相同的值，返回True，否则返回False</returns>
        private bool HasTableData(ref DataTable data, string modelFullName, int cIndex)
        {
            return HasTableData(ref data, modelFullName, cIndex, false);
        }

        private bool HasTableData(ref DataTable data, string modelFullName, int cIndex, bool isPrice)
        {
            bool returnBool = false;
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                if (cIndex > 0 && dr[cIndex - 1].ToString() == modelFullName)
                {
                    returnBool = true;
                    string qty = dr[cIndex].ToString();
                    if (!string.IsNullOrEmpty(qty) && qty != "-")
                    {
                        dr.BeginEdit();
                        dr[cIndex] = Convert.ToInt32(qty) + 1;
                        if (isPrice)
                        {
                            string unitPrice = dr[cIndex + 1].ToString();
                            if (!string.IsNullOrEmpty(unitPrice) && unitPrice != "-")
                                dr[cIndex + 2] = Convert.ToDouble(dr[cIndex + 2]) + Convert.ToDouble(unitPrice);
                        }
                        dr.EndEdit();
                    }
                }
            }
            return returnBool;
        }

        private bool HasTableDataNew(ref DataTable data, string modelFullName, int cIndex)
        {
            bool returnBool = false;
            for (int rIndex = 0; rIndex < data.Rows.Count; rIndex++)
            {
                DataRow dr = data.Rows[rIndex];
                if (cIndex > 0 && dr[cIndex - 1].ToString() == modelFullName)
                {
                    returnBool = true;
                    string qty = dr[cIndex+1].ToString();
                    if (!string.IsNullOrEmpty(qty) && qty != "-")
                    {
                        dr.BeginEdit();
                        dr[cIndex+1] = Convert.ToInt32(qty) + 1;
                        dr.EndEdit();
                    }
                }
            }
            return returnBool;
        }

        /// <summary>
        /// 得到当前项目中汇总的室内外机的价格表
        /// </summary>
        /// <returns></returns>
        public DataView GetExcelData_UnitPriceList(int priceNum)
        {
            //priceNum 1-indoor 2-outdoor
            /// <summary>
            /// 对应Report中【5. Pricing Form】机组价格统计
            /// </summary>
            string[] ColNameArray_UnitPrice =
            {
                Name_Common.ModelFull,
                Name_Common.Qty,
                Name_Common.UnitPrice,
                Name_Common.TotalPrice,
                Name_Common.Remark
            };

            DataTable dt = Util.InitDataTable(ColNameArray_UnitPrice);

            if (priceNum == 1)
            {
                //添加Indoor
                foreach (RoomIndoor ri in thisProject.RoomIndoorList)
                {
                    if (!valIndoorExport(ri))
                        continue;

                    if (string.IsNullOrEmpty(ri.SystemID))
                        continue;
                    // 查找此表中是否存在相同的数据 
                    // TODO：此处区分是否相同的室内机，要判断所有的 Option 选项，20130929 clh
                    if (ri.IndoorItem == null)
                        continue;

                    // 查找此表中是否存在相同的数据 
                    // Updated on 20140530 clh
                    // 此处区分是否相同的室内机，要判断所有的 Option 选项，若存在不影响Model的Option项目存在，则在Remark列中备注
                    string modelAfterOption = ri.IndoorItem.ModelFull;
                    //string remark = ri.IndoorItem.Series;
                    string remark = trans.getTypeTransStr(TransType.Series.ToString(), ri.IndoorItem.Series);
                    // 若当前选择了Option则此处的ModelName填写选择了Option之后的新型号名
                    if (ri.OptionItem != null)
                    {
                        modelAfterOption = ri.OptionItem.GetNewModelWithOptionB();
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
                        if (HasTableData(ref dt, modelAfterOption, 1, true))
                            continue;
                    }

                    DataRow dr = dt.NewRow();
                    dr[Name_Common.ModelFull] = modelAfterOption;

                    dr[Name_Common.Qty] = "1";
                    // update on 20120612 clh
                    //double stdPrice = ri.IndoorItem.StandardPrice;
                    // get option price sum 20130918 clh
                    double optPrice = GetOptionPriceSum(ri);

                    dr[Name_Common.UnitPrice] = "-";// ShowPrice() ? stdPrice.ToString() :"-"
                    dr[Name_Common.TotalPrice] = dr[Name_Common.UnitPrice];
                    //ShowPrice() ? (stdPrice + optPrice).ToString() : "-";
                    //dr[Name_Common.UnitPrice] = "-";
                    //dr[Name_Common.TotalPrice] = "-";
                    dr[Name_Common.Remark] = remark;
                    dt.Rows.Add(dr);
                }
            }
            else if (priceNum == 2)
            {
                //添加Outdoor
                for (int i = 0; i < thisProject.SystemList.Count; i++)
                {
                    SystemVRF sysItem = thisProject.SystemList[i];
                    if (!valSystemExport(sysItem))
                        continue;
                    //查找此表中是否存在相同的数据
                    if (HasTableData(ref dt, sysItem.OutdoorItem.AuxModelName, 2, true))//
                        continue;
                    DataRow dr = dt.NewRow();
                    dr[Name_Common.ModelFull] = sysItem.OutdoorItem.AuxModelName;
                    dr[Name_Common.Qty] = "1";
                    dr[Name_Common.UnitPrice] = "-";
                    //ShowPrice() ? sysItem.OutdoorItem.Price.ToString() : "-";
                    //dr[Name_Common.UnitPrice] = "-";
                    dr[Name_Common.TotalPrice] = dr[Name_Common.UnitPrice];

                    string remark = "";
                    if (sysItem.OptionItem != null && sysItem.OptionItem.OptionPower != null)
                    {
                        remark += sysItem.OptionItem.OptionPower.GetDescription(sysItem.OptionItem.OptionPower.SelectedValue);
                    }

                    dr[Name_Common.Remark] = remark;

                    dt.Rows.Add(dr);
                }
            }
            else
            {
                // 向 dt 中插入分歧管记录
                foreach (SystemVRF sysItem in thisProject.SystemList)
                {
                    if (!valSystemExport(sysItem))
                        continue;

                    if (sysItem.MyPipingNodeOut != null)
                    {
                        Node fNode = sysItem.MyPipingNodeOut.ChildNode;
                        GatherJointKit(ref dt, fNode);
                    }
                    //2013-11-1 by Yang 增加室外机模块机机组之间的分歧管型号
                    //GatherJointKit_Outdoor(ref dt, thisProject.RegionVRF.ToString(), thisProject.ProductType.ToString(), sysItem.SelOutdoorType.ToString(), sysItem.OutdoorItem.AuxModelName);

                    // 20150128 clh update 更新Outdoor类结构，增加室外机模块机机组之间的分歧管型号分歧管相关字段
                    if (sysItem.OutdoorItem != null)
                    {
                        // 为兼容之前的项目
                        if (thisProject.CreateDate < DateTime.Parse("2015/1/28")
                            && Util.ParseToInt(sysItem.OutdoorItem.ModelFull) >= 200)
                        {
                            //sysItem.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType, thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull);
                            //多ProductType功能修改 20160823 by Yunxiao Lin
                            sysItem.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.ProductType);
                        }

                        if (!string.IsNullOrEmpty(sysItem.OutdoorItem.JointKitModelG) &&
                            !string.IsNullOrEmpty(sysItem.OutdoorItem.JointKitModelL))
                        {
                            string[] arrJointKitModelL = sysItem.OutdoorItem.JointKitModelL.Split('/');
                            string[] arrJointKitModelG = sysItem.OutdoorItem.JointKitModelG.Split('/');

                            for (int i = 0; i < arrJointKitModelL.Length; ++i)
                            {
                                //    PipingRuleBLL pipingBll = new PipingRuleBLL(thisProject);
                                //  string pL = pipingBll.GetJointKitPrice(arrJointKitModelL[i]);
                                // string pG = pipingBll.GetJointKitPrice(arrJointKitModelL[i]);
                                AddRowToTable_JointKit(ref dt, arrJointKitModelL[i], "-");
                                AddRowToTable_JointKit(ref dt, arrJointKitModelG[i], "-");
                            }
                        }
                    }

                }
            }
            return dt.DefaultView;
        }

        // 验证当前系统是否符合输出条件
        /// <summary>
        /// 验证当前系统是否符合输出条件
        /// </summary>
        /// <param name="sysItem"></param>
        /// <returns></returns>
        private bool valSystemExport(SystemVRF sysItem)
        {
            if (sysItem.OutdoorItem == null)
                return false;
            else if (!sysItem.IsExportToReport)
                return false;

            return true;
        }

        private bool valIndoorExport(RoomIndoor roomItem)
        {
            bool isExport = true;
            for (int i = 0; i < thisProject.SystemList.Count; i++)
            {
                if (thisProject.SystemList[i].Id == roomItem.SystemID)
                {
                    isExport = valSystemExport(thisProject.SystemList[i]);
                    break;
                }
            }
            return isExport;
        }
        #endregion

        #region GetData_JointKitPriceList

        /// <summary>
        /// 对应Report中【5. Pricing Form】分歧管价格统计
        /// </summary>
        private string[] ColNameArray_JointKitPrice =
        {
            Name_Common.ModelFull,
            Name_Common.Qty
        };

        /// <summary>
        /// 得到当前项目中汇总的分歧管的数量统计表，update 20130926
        /// </summary>
        /// <returns></returns>
        private DataView GetData_JointKitNumberList(ref JCHVRF.Model.NextGen.SystemVRF sysItem, out DataView dvL)
        {
            DataTable dtG = Util.InitDataTable(ColNameArray_JointKitPrice);
            DataTable dtL = dtG.Clone();
            if (sysItem.MyPipingNodeOut != null)
            {
                JCHVRF.Model.NextGen.JCHNode fNode = (JCHVRF.Model.NextGen.JCHNode)sysItem.MyPipingNodeOut.ChildNode;

                // Node fNode = sysItem.MyPipingNodeOut.ChildNode;
                GatherJointKit(ref dtG, ref dtL, fNode);
            }
            //2013-11-1 by Yang 增加室外机模块机机组之间的分歧管型号
            //GatherJointKit_Outdoor(ref dtG, ref dtL, thisProject.RegionVRF, thisProject.ProductType, sysItem.SelOutdoorType, sysItem.OutdoorItem.ModelFull);

            // 20150128 clh update 更新Outdoor类结构，增加分歧管相关字段
            if (sysItem.OutdoorItem != null)
            {
                // 为兼容之前的项目
                if (thisProject.CreateDate < DateTime.Parse("2015/1/28")
                    && Util.ParseToInt(sysItem.OutdoorItem.ModelFull) >= 200)
                {
                    //sysItem.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.ProductType,thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull);
                    sysItem.OutdoorItem = (new OutdoorBLL(thisProject.SubRegionCode, thisProject.BrandCode)).GetOutdoorItem(sysItem.OutdoorItem.ModelFull, sysItem.OutdoorItem.ProductType);
                }

                if (!string.IsNullOrEmpty(sysItem.OutdoorItem.JointKitModelG) &&
                    !string.IsNullOrEmpty(sysItem.OutdoorItem.JointKitModelL)
                    )
                {
                    string[] arrJointKitModelL = sysItem.OutdoorItem.JointKitModelL.Split('/');
                    string[] arrJointKitModelG = sysItem.OutdoorItem.JointKitModelG.Split('/');
                    for (int i = 0; i < arrJointKitModelL.Length; ++i)
                    {
                        AddRowToTable_JointKit(ref dtL, arrJointKitModelL[i]);
                        AddRowToTable_JointKit(ref dtG, arrJointKitModelG[i]);
                    }
                }
            }

            dvL = dtL.DefaultView;
            dvL.Sort = Name_Common.ModelFull + " desc";
            DataView dvG = dtG.DefaultView;
            dvG.Sort = Name_Common.ModelFull + " desc";
            return dvG;
        }

        private void GatherJointKit(ref DataTable dtG, ref DataTable dtL, JCHVRF.Model.NextGen.JCHNode node)
        {
            if (node is  JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP  yp = node as JCHVRF.Model.NextGen.MyNodeYP;
                // MyNodeYP yp = node as MyNodeYP;
                AddRowToTable_JointKit(ref dtG, yp.Model);
                //AddRowToTable_JointKit(ref dtL, yp.ModelL);
                foreach (JCHVRF.Model.NextGen.JCHNode nd in yp.ChildNodes)
                    GatherJointKit(ref dtG, ref dtL, nd);
            }
        }

        // 辅助方法
        private void AddRowToTable_JointKit(ref DataTable dt, string JointKitModel)
        {
            if (HasTableData(ref dt, JointKitModel, 1, false))
                return;
            DataRow dr = dt.NewRow();
            dr[Name_Common.ModelFull] = JointKitModel;
            dr[Name_Common.Qty] = "1";
            dt.Rows.Add(dr);
        }


        private void GatherJointKit(ref DataTable dt, Node node)
        {
            if (node is MyNodeYP)
            {
                MyNodeYP yp = node as MyNodeYP;
                AddRowToTable_JointKit(ref dt, yp.Model, yp.PriceG);
                //AddRowToTable_JointKit(ref dt, yp.ModelL, yp.PriceL);

                foreach (Node nd in yp.ChildNodes)
                    GatherJointKit(ref dt, nd);
            }
        }

        // 辅助方法
        private void AddRowToTable_JointKit(ref DataTable dt, string JointKitModel, string JointKitPrice)
        {
            if (HasTableData(ref dt, JointKitModel, 1, true))
                return;
            DataRow dr = dt.NewRow();
            dr[Name_Common.ModelFull] = JointKitModel;
            dr[Name_Common.Qty] = "1";
            dr[Name_Common.UnitPrice] = "-";
            //ShowPrice() ? JointKitPrice : "-";
            //dr[Name_Common.UnitPrice] = "-";
            dr[Name_Common.TotalPrice] = dr[Name_Common.UnitPrice];
            dr[Name_Common.Remark] = "";
            dt.Rows.Add(dr);
        }

        #endregion

        #region 记录汇总
        /// <summary>
        /// 判断当前室内机与表格中已添加的室内机有无完全相同的记录，所有的Option选项都必须一致，UnitPanel & TIO2 在【Remark】列中以描述形式体现；
        /// 若完全相同，则需要合并【Qty】【Total Price】两列
        /// </summary>
        /// <param name="dt">模板中的价格表</param>
        /// <param name="modelAfterOption">当前的产品型号</param>
        /// <param name="remark"> UP & TIO2 的描述，选择默认值则为空 </param>
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
                    string qty = dr[modelIndex].ToString();
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

        #region 附件列表
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
                    //区分系统
                    string sysName = "";
                    if (!string.IsNullOrEmpty(ri.SystemID))
                    {
                        var sys = thisProject.SystemList.Find(p => p.Id == ri.SystemID && p.IsExportToReport == true);
                        if (sys != null)
                        {
                            sysName = sys.Name;
                        }
                    }
                    //FillMaterialData(dicMaterial["Accessory"], aModel + "$#$" + sysName, 1);
                    FillMaterialData(dicMaterial["Accessory"], aModel, 1);
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
                    //Acc.Type = item.Type;
                    Acc.Type = trans.getTypeTransStr(TransType.IDU_Accessory.ToString(), item.Type);
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
                        //区分系统
                        string sysName = ri.IndoorFullName;
                        //FillMaterialData(dicMaterial["Accessory"], aModel + "$#$" + sysName, 1);
                        FillMaterialData(dicMaterial["Accessory"], aModel, 1);
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
                        //Acc.Type = item.Type;
                        Acc.Type = trans.getTypeTransStr(TransType.Series.ToString(), item.Type);
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
        #endregion 

        // 计算指定室内机的所有选配项的价格之和
        /// <summary>
        /// 计算指定室内机的所有选配项的价格之和
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        public double GetOptionPriceSum(RoomIndoor ri)
        {
            double optPrice = 0;
            if (ri.OptionItem != null)
            {
                // 电加热
                if (ri.OptionItem.OptionAEH != null)
                    optPrice += ri.OptionItem.OptionAEH.GetPrice();

                // 水泵 & 排水管
                if (ri.OptionItem.OptionDPED != null)
                    optPrice += ri.OptionItem.OptionDPED.GetPrice();

                // 遥控+线控 & 过滤网
                if (ri.OptionItem.OptionCF != null)
                    optPrice += ri.OptionItem.OptionCF.GetPrice();

                // 面板
                if (ri.OptionItem.OptionUP != null)
                    optPrice += ri.OptionItem.OptionUP.GetPrice();

                // TIO2
                if (ri.OptionItem.OptionTIO != null)
                    optPrice += ri.OptionItem.OptionTIO.GetPrice();

                // Power
                if (ri.OptionItem.OptionPower != null)
                    optPrice += ri.OptionItem.OptionPower.GetPrice();

                // Insulation
                if (ri.OptionItem.OptionInsulation != null)
                    optPrice += ri.OptionItem.OptionInsulation.GetPrice();
            }
            return optPrice;
        }

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
                if (HasTableData(ref dt, item.Name, 1))
                    continue;
                DataRow dr = dt.NewRow();
                dr[Name_Common.Model] = item.Name;
                dr[Name_Common.Qty] = item.Quantity;
                //dr[Name_Common.Description] = item.Description;
                dr[Name_Common.Description] = trans.getTypeTransStr(TransType.Controller.ToString(), item.Description);
                dt.Rows.Add(dr);
            }

            foreach (ControllerAssembly item in thisProject.ControllerAssemblyList)
            {
                //查找此表中是否存在相同的数据
                if (HasTableData(ref dt, item.Name, 1))
                    continue;
                DataRow dr = dt.NewRow();
                dr[Name_Common.Model] = item.Name;
                dr[Name_Common.Qty] = "1";
                dr[Name_Common.Description] = item.GetDescription(item.Name);
                dt.Rows.Add(dr);
            }

            return dt.DefaultView;
        }

        // 得到指定系统的配管图中，连接管的管径规格及长度汇总记录
        /// <summary>
        /// 得到指定系统的连接管 管径与长度汇总数据 Update on 20120828 clh 
        /// </summary>
        /// <param name="sysName"></param>
        /// <param name="dvL"></param>
        /// <returns></returns>
        public DataView GetData_LinkPipeSpecG(ref JCHVRF.Model.NextGen.SystemVRF sysItem, out DataView dvL)
        {
            DataTable dtG = Util.InitDataTable(NameArray_Report.PipeSpec_Name);
            //解决DataView默认按照字符串类型排序的问题20120111-clh-
            if (!CommonBLL.IsDimension_inch()) // 若为英制，此时的管径为分数格式（7/8等），不能转为double进行排序
                dtG.Columns[RptColName_PipeSpec.PipeSpec].DataType = typeof(double);
            DataTable dtL = dtG.Clone();

            if (sysItem.MyPipingNodeOut != null)
            {
                JCHVRF.Model.NextGen.JCHNode fNode = sysItem.MyPipingNodeOut;
                //Node fNode = sysItem.MyPipingNodeOut;
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

        private void GatherLinkPipeSpec(ref DataTable dtG, ref DataTable dtL, JCHVRF.Model.NextGen.JCHNode node, string sysName)
        {
            if (node == null) return;
            try
            {
                NextGenPipingBLL.PipingBLL pipBll = new NextGenPipingBLL.PipingBLL(thisProject);
                if (node is JCHVRF.Model.NextGen.MyNodeOut)
                {
                    JCHVRF.Model.NextGen.MyNodeOut nodeOut = node as JCHVRF.Model.NextGen.MyNodeOut;
                    //MyNodeOut nodeOut = node as MyNodeOut;

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
                else if (node is JCHVRF.Model.NextGen.MyNode)
                {
                    foreach (JCHVRF.Model.NextGen.MyLink  link in (node as JCHVRF.Model.NextGen.MyNode).MyInLinks)
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

                if (node is JCHVRF.Model.NextGen.MyNodeOut)
                {
                    GatherLinkPipeSpec(ref dtG, ref dtL,  (JCHVRF.Model.NextGen.JCHNode)(node as JCHVRF.Model.NextGen.MyNodeOut).ChildNode, sysName);
                    //GatherLinkPipeSpec(ref dtG, ref dtL, (node as JCHVRF.Model.NextGen.MyNodeOut).ChildNode, sysName);

                }
                if (node is JCHVRF.Model.NextGen.MyNodeYP)
                {
                    foreach (Lassalle.WPF.Flow.Node n in (node as JCHVRF.Model.NextGen.MyNodeYP).ChildNodes)
                    {
                        GatherLinkPipeSpec(ref dtG, ref dtL, n as JCHVRF.Model.NextGen.JCHNode, sysName);
                    }
                }
                else if (node is JCHVRF.Model.NextGen.MyNodeCH)
                {
                    GatherLinkPipeSpec(ref dtG, ref dtL, (JCHVRF.Model.NextGen.JCHNode)(node as JCHVRF.Model.NextGen.MyNodeCH).ChildNode, sysName);
                }
                else if (node is JCHVRF.Model.NextGen.MyNodeMultiCH)
                {
                    foreach (Lassalle.WPF.Flow.Node n in (node as JCHVRF.Model.NextGen.MyNodeMultiCH).ChildNodes)
                    {
                        GatherLinkPipeSpec(ref dtG, ref dtL, (JCHVRF.Model.NextGen.JCHNode)n, sysName);
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

        private Dictionary<string, string> fillkits(DataTable data, string type)
        {
            if (data == null || data.Rows.Count == 0)
            {
                return null;
            }
            Dictionary<string, string> dataDic = new Dictionary<string, string>();
            DataView dv = data.Copy().DefaultView;
            dv.RowFilter = "Type='" + type + "'";
            DataTable temp = dv.ToTable();

            var querySum = from t in temp.AsEnumerable()
                           group t by t.Field<string>("Model")
                               into g
                           select new
                           {
                               _Model = g.Key,
                               _Qty = g.Count()
                           };
            foreach (DataRow dr in temp.Rows)
            {
                //FillMaterialData(dicMaterial[type], dr["Model"].ToString() + "$#$" + dr["System"].ToString(), Convert.ToInt32(dr["Qty"].ToString()));
                FillMaterialData(dicMaterial[type], dr["Model"].ToString(), Convert.ToInt32(dr["Qty"].ToString()));
            }
            //foreach (var vq in querySum)
            //{
            //    dataDic.Add("Model", vq._Model);
            //    dataDic.Add("Qty", vq._Qty.ToString());
            //    dataDic.Add("StartRow", "");
            //}
            foreach (var vq in querySum)
            {
                dataDic.Add(vq._Model, vq._Qty.ToString());
            }
            if (dataDic.Count == 0)
                return null;
            return dataDic;

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

    }
}
