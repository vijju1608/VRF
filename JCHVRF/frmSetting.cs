using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using JCBase.UI;
using System.Threading;
using JCBase.Util;
using JCHVRF.VRFMessage;
using JCHVRF.BLL;
using JCHVRF.Model;
using JCBase.Utility;
namespace JCHVRF
{
    public partial class frmSetting : JCBase.UI.JCForm
    {
        private bool _switchingTemperatureUnit = false;  //切换温度单位时
        private bool _loading = false;
        private string DXFPath = "";
        private string reportPath = "";
        private string CurrLanguage = "";
        Model.Project thisProject;

        #region Initialization 初始化
        public frmSetting(Model.Project thisProj)
        {
            InitializeComponent();
            this.tcSetting.TabPages.Remove(tpgAdvanced);    // 临时移除Advanced页面
            thisProject = thisProj;
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            this.JCCallValidationManager = true;
            this.jcbtnOK.Focus();
            if (CultureInfo.CurrentCulture.Name == "zh-CN")
            { }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            }
            InitFormLanguage();
            LoadData();

            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            SetMinMaxTemperature(ut_temperature);

            //switchForOutdoorWorkingCondition();
        }

        /// 获取系统当前语言,初始化界面语言
        /// <summary>
        /// 获取系统当前语言,初始化界面语言
        /// </summary>
        private void InitFormLanguage()
        {
            
            string curLanguage = SystemSetting.UserSetting.fileSetting.settingLanguage==""?this.JCCurrentLanguage:SystemSetting.UserSetting.fileSetting.settingLanguage;
            switch (curLanguage)
            {
                case LangType.ENGLISH:
                    jcCoboxLanguage.Text = "English"; 
                    break;
                case LangType.CHINESE:
                    jcCoboxLanguage.Text = "中文(简体)"; 
                    break;
                case LangType.CHINESE_TRADITIONAL:
                    jcCoboxLanguage.Text = "中文(繁體)";
                    break;
                case LangType.FRENCH:
                    jcCoboxLanguage.Text = "Français"; 
                    break;
                case LangType.TURKISH:
                    jcCoboxLanguage.Text = "Türk"; 
                    break;
                case LangType.SPANISH:
                    jcCoboxLanguage.Text = "España"; 
                    break;
                case LangType.GERMANY:
                    jcCoboxLanguage.Text = "Deutsch";
                    break;
                case LangType.ITALIAN:
                    jcCoboxLanguage.Text = "Italiano";
                    break;
                case LangType.BRAZILIAN_PORTUGUESS:
                    jcCoboxLanguage.Text = "Brazilian Portuguese"; 
                    break;

            }
            
        }


        /// 切换界面控件语言
        /// <summary>
        /// 切换界面控件语言
        /// </summary>
        /// <param name="name"> 切换语言菜单的 Name </param>
        private void DoSwitchLanguage(string name)
        {
            string lang = LangType.ENGLISH;
            //if (name == tbtnLanguage_zh.Name)
            //{
            //    tbtnLanguage_en.Checked = false;
            //    tbtnLanguage_es.Checked = false;
            //    tbtnLanguage_tr.Checked = false;
            //    tbtnLanguage_fr.Checked = false;
            //    tbtnLanguage_zh.Checked = true;
            //    lang = LangType.CHINESE;
                
            //}
            //else if (name == tbtnLanguage_en.Name)
            //{
            //    tbtnLanguage_en.Checked = true;
            //    tbtnLanguage_es.Checked = false;
            //    tbtnLanguage_tr.Checked = false;
            //    tbtnLanguage_fr.Checked = false;
            //    tbtnLanguage_zh.Checked = false;
            //    lang = LangType.ENGLISH;
            //}
            //else if (name == tbtnLanguage_es.Name)
            //{
            //    tbtnLanguage_en.Checked = false;
            //    tbtnLanguage_es.Checked = true;
            //    tbtnLanguage_tr.Checked = false;
            //    tbtnLanguage_fr.Checked = false;
            //    tbtnLanguage_zh.Checked = false;
            //    lang = LangType.SPANISH;
            //}
            //else if (name == tbtnLanguage_tr.Name)
            //{
            //    tbtnLanguage_en.Checked = false;
            //    tbtnLanguage_es.Checked = false;
            //    tbtnLanguage_tr.Checked = true;
            //    tbtnLanguage_fr.Checked = false;
            //    tbtnLanguage_zh.Checked = false;
            //    lang = LangType.TURKISH;
            //}
            //else if (name == tbtnLanguage_fr.Name)
            //{
            //    tbtnLanguage_en.Checked = false;
            //    tbtnLanguage_es.Checked = false;
            //    tbtnLanguage_tr.Checked = false;
            //    tbtnLanguage_fr.Checked = true;
            //    tbtnLanguage_zh.Checked = false;
            //    lang = LangType.FRENCH;
            //}
            CurrLanguage = name;
            JCSetLanguage(lang);

             
          
        }


        /// <summary>
        /// 增加水冷机判断，水冷机参数不是空气温度，而是进水温度 20160720 by Shen Junjie
        /// </summary>
        private void switchForOutdoorWorkingCondition()
        {
            bool isWaterSource = thisProject.ProductType.Contains("Water Source");
            //Cooling DB
            jclblCoolDB.Visible = !isWaterSource;
            jctbOutdoorCoolingDB.Visible = !isWaterSource;
            jclblOutdoorCoolingDB.Visible = !isWaterSource;
            //Heating DB
            jclblHeatDB.Visible = !isWaterSource;
            jctbOutdoorHeatingDB.Visible = !isWaterSource;
            jclblOutdoorHeatingDB.Visible = !isWaterSource;
            //Cooling Inlet Water
            jclblCoolIW.Visible = isWaterSource;
            jctbOutdoorCoolingIW.Visible = isWaterSource;
            jclblOutdoorCoolingIW.Visible = isWaterSource;
            //Heating Inlet Water
            jclblHeatIW.Visible = isWaterSource;
            jctbOutdoorHeatingIW.Visible = isWaterSource;
            jclblOutdoorHeatingIW.Visible = isWaterSource;

            if (isWaterSource)
            {
                //Cooling Inlet Water
                jclblCoolIW.Location = new Point(3, 7);
                jctbOutdoorCoolingIW.Location = new Point(78, 5);
                jclblOutdoorCoolingIW.Location = new Point(120, 8);
                //Heating WB
                jclblHeatWB.Location = new Point(3, 7);
                jctbOutdoorHeatingWB.Location = new Point(35, 4);
                jclblOutdoorHeatingWB.Location = new Point(77, 8);
                //Heating RH
                jclblHeatRH.Location = new Point(107, 7);
                jctbOutdoorHeatingRH.Location = new Point(139, 4);
                jcLabel7.Location = new Point(182, 7);
                //Heating Inlet Water
                jclblHeatIW.Location = new Point(210, 7);
                jctbOutdoorHeatingIW.Location = new Point(285, 4);
                jclblOutdoorHeatingIW.Location = new Point(325, 8);
            }
            else
            {
                //Cooling Inlet Water
                jclblCoolIW.Location = new Point(313, 7);
                jctbOutdoorCoolingIW.Location = new Point(388, 5);
                jclblOutdoorCoolingIW.Location = new Point(430, 8);
                //Heating WB
                jclblHeatWB.Location = new Point(106, 7);
                jctbOutdoorHeatingWB.Location = new Point(138, 4);
                jclblOutdoorHeatingWB.Location = new Point(180, 8);
                //Heating RH
                jclblHeatRH.Location = new Point(210, 7);
                jctbOutdoorHeatingRH.Location = new Point(242, 4);
                jcLabel7.Location = new Point(285, 7);
                //Heating Inlet Water
                jclblHeatIW.Location = new Point(313, 7);
                jctbOutdoorHeatingIW.Location = new Point(388, 4);
                jclblOutdoorHeatingIW.Location = new Point(430, 8);
            }
        }

        void SetMinMaxTemperature(string ut_temperature)
        {
            this.jctbIndoorCoolingDB.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbIndoorCoolingDB.JCMaxValue = float.Parse(Unit.ConvertToControl(30, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbIndoorCoolingWB.JCMinValue = float.Parse(Unit.ConvertToControl(14, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//11->14
            this.jctbIndoorCoolingWB.JCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbIndoorHeatingDB.JCMinValue = float.Parse(Unit.ConvertToControl(16, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbIndoorHeatingDB.JCMaxValue = float.Parse(Unit.ConvertToControl(24, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//30->24

            this.jctbOutdoorCoolingDB.JCMinValue = float.Parse(Unit.ConvertToControl(10, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//-5->10
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                this.jctbOutdoorCoolingDB.JCMaxValue = float.Parse(Unit.ConvertToControl(52, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//52->43  43 改为52  欧洲需求 on 20180330 by xyj 
            }
            else {
                this.jctbOutdoorCoolingDB.JCMaxValue = float.Parse(Unit.ConvertToControl(43, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            }

            this.jctbIndoorCoolingRH.JCMinValue = float.Parse("0");//0-100
            this.jctbIndoorCoolingRH.JCMaxValue = float.Parse("100");//0-100
            // T3 室外机制冷温度最高为52摄氏度 20161028 by Yunxiao Lin
            if (thisProject.SubRegionCode == "ME_T3A" || thisProject.SubRegionCode == "ME_T3B")
                this.jctbOutdoorCoolingDB.JCMaxValue = float.Parse(Unit.ConvertToControl(52, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            if (thisProject.RegionCode == "EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                jctbIndoorCoolingRH.Enabled = false;
                jctbOutdoorHeatingRH.Enabled = false;
            }

            this.jctbOutdoorHeatingDB.JCMinValue = float.Parse(Unit.ConvertToControl(-18, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbOutdoorHeatingDB.JCMaxValue = float.Parse(Unit.ConvertToControl(33, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbOutdoorHeatingWB.JCMinValue = float.Parse(Unit.ConvertToControl(-20, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));
            this.jctbOutdoorHeatingWB.JCMaxValue = float.Parse(Unit.ConvertToControl(15, UnitType.TEMPERATURE, ut_temperature).ToString("n1"));//20->15

            this.jctbOutdoorHeatingRH.JCMinValue = float.Parse("0");//0-100
            this.jctbOutdoorHeatingRH.JCMaxValue = float.Parse("100");//0-100 

            this.jctbOutdoorCoolingIW.JCMinValue = float.Parse(Unit.ConvertToControl(10d, UnitType.TEMPERATURE, ut_temperature).ToString("n0"));
            this.jctbOutdoorCoolingIW.JCMaxValue = float.Parse(Unit.ConvertToControl(45d, UnitType.TEMPERATURE, ut_temperature).ToString("n0"));
            this.jctbOutdoorHeatingIW.JCMinValue = float.Parse(Unit.ConvertToControl(10d, UnitType.TEMPERATURE, ut_temperature).ToString("n0"));
            this.jctbOutdoorHeatingIW.JCMaxValue = float.Parse(Unit.ConvertToControl(45d, UnitType.TEMPERATURE, ut_temperature).ToString("n0"));
        }

        #endregion

        #region Controls events
        
        private void trackBarInC_ValueChanged(object sender, EventArgs e)
        {
            lblTrackBarValueInC.Text = trackBarInC.Value.ToString();
            lblTrackBarValueInH.Text = trackBarInH.Value.ToString();
            lblTrackBarValueOutC.Text = trackBarOutC.Value.ToString();
            lblTrackBarValueOutH.Text = trackBarOutH.Value.ToString();
        }

        private void rbTemperaturec_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (rbTemperaturec.Checked)
            {
                ChangeTempUnit(true);
            }
        }

        private void rbTemperaturef_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            if (rbTemperaturef.Checked)
            {
                ChangeTempUnit(false);
            }
        }

        private void btDXFSelect_Click(object sender, EventArgs e)
        {
            if (DXFPath == "")
            {
                DXFPath = SelectFolder("");
            }
            else
            {
                if (System.IO.Directory.Exists(DXFPath))
                {
                    DXFPath = SelectFolder(DXFPath);
                }
                else
                {
                    DXFPath = SelectFolder("");
                }
            }

            if (DXFPath == "")
            {
                //if (Util.IsChinese())
                //{
                //    lbDXFPath.Text = "请选择DXF存放目录";
                //}
                //else
                //{
                //    lbDXFPath.Text = "Please choose a directionary for saving DXF files";
                //}
                lbDXFPath.Text = Msg.GetResourceString("Msg_DXFPath");
            }
            else
            {
                lbDXFPath.Text = DXFPath;
            }
        }

        private void btReportSelect_Click(object sender, EventArgs e)
        {
            if (reportPath == "")
            {
                reportPath = SelectFolder("");
            }
            else
            {
                if (System.IO.Directory.Exists(reportPath))
                {
                    reportPath = SelectFolder(reportPath);
                }
                else
                {
                    reportPath = SelectFolder("");
                }
            }

            if (reportPath == "")
            {
                lbReportPath.Text = Msg.GetResourceString("Msg_ReportPath");
            }
            else
            {
                lbReportPath.Text = reportPath;
            }
        }
                
        private void btDXFClear_Click(object sender, EventArgs e)
        {
            //if (Util.IsChinese())
            //{
            //    lbDXFPath.Text = "请选择DXF存放目录";
            //}
            //else
            //{
            //    lbDXFPath.Text = "Please choose a directionary for saving DXF files";
            //}
            lbDXFPath.Text = Msg.GetResourceString("Msg_DXFPath");
            DXFPath = "";
        }

        private void btReportClear_Click(object sender, EventArgs e)
        {
            //if (Util.IsChinese())
            //{
            //    lbReportPath.Text = "请选择报表存放目录";
            //}
            //else
            //{
            //    lbReportPath.Text = "Please choose a directionary for saving report files";
            //}  
            lbReportPath.Text = Msg.GetResourceString("Msg_ReportPath");
            reportPath = "";
        }

        private void jcbtnOK_Click(object sender, EventArgs e)
        {
            if (this.JCValidateForm())
            {
                if (CheckData())
                {
                    SetData();
                    SystemSetting.Serialize();
                    DialogResult = DialogResult.OK;
                    Close();
                    RefreshLanguage();
                    HideshowIduCapacityW();
                }
            }
            
        }

        private void jcbtnCancel_Click(object sender, EventArgs e)
        {
           // RefreshLanguage();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Response codes (Methods && Events)

        /// <summary>
        /// 保存当前设置信息到配置文件
        /// </summary>
        private void DoOK()
        {
            if (this.JCValidateForm())
            {
                JCMsg.ShowInfoOK("Check error！");
            }
            else
            {
                this.tcSetting.Show();
            }
        }

        private void RefreshLanguage()
        {
            frmMain f = (frmMain)this.Owner;
            f.BindLanguage(CurrLanguage);
           
        }

        private void HideshowIduCapacityW()
        {
            frmMain f = (frmMain)this.Owner;
            //f.BindLanguage(CurrLanguage);

            JCBase.UI.uc_CheckBox chkbox = (JCBase.UI.uc_CheckBox)f.Controls.Find("uc_CheckBox_IduCapacityW", true)[0];
            if(rbCapacitykw.Checked==true)
            {
                chkbox.Visible = true;
            }
            else
            {
                chkbox.Visible = false;
            }
        }

        private void LoadData()
        {
            _switchingTemperatureUnit = false;
            _loading = true;

            if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_kw)
            {
                rbCapacitykw.Checked = true;
            }
            else if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_w)
            {
                rbCapacityw.Checked = true;
            }
            else if (SystemSetting.UserSetting.unitsSetting.settingPOWER == Unit.ut_Capacity_ton)
            {
                rbCapacityton.Checked = true;
            }
            else
            {
                rbCapacitybtu.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingAIRFLOW == Unit.ut_Airflow_ls)
            {
                rbAirflowls.Checked = true;
            }
            else if (SystemSetting.UserSetting.unitsSetting.settingAIRFLOW == Unit.ut_Airflow_m3h)
            {
                rbAirflowmh.Checked = true;
            }
            else if (SystemSetting.UserSetting.unitsSetting.settingAIRFLOW == Unit.ut_Airflow_m3hr)
            {
                rbAirflowmhr.Checked = true;
            }

            else
            {
                rbAirflowcfm.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE == Unit.ut_Temperature_c)
            {
                rbTemperaturec.Checked = true;
            }
            else
            {
                rbTemperaturef.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingLENGTH == Unit.ut_Size_m)
            {
                rbLengthm.Checked = true;
            }
            else
            {
                rbLengthft.Checked = true;
            }


            if (SystemSetting.UserSetting.unitsSetting.settingDimension == Unit.ut_Dimension_mm)
            {
                rbDemensionsmm.Checked = true;
            }
            else
            {
                rbDemensionsinch.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingDimensionUnit == Unit.ut_Dimension_mm)
            {
                rbDemensionsmmUnit.Checked = true;
            }
            else
            {
                rbDemensionsinchUnit.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingWEIGHT == Unit.ut_Weight_kg)
            {
                rbWeightkg.Checked = true;
            }
            else
            {
                rbWeightlbs.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingAREA == Unit.ut_Area_m2)
            {
                rbAream2.Checked = true;
            }
            else
            {
                rbAreaft2.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingLOADINDEX == Unit.ut_LoadIndex_w)
            {
                rbLoadIndexWm2.Checked = true;
            }
            else
            {
                rbLoadIndexMBH.Checked = true;
            }

            if (SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate == Unit.ut_WaterFlowRate_m3h)
            {
                rbWaterFlowRatem3h.Checked = true;
            }
            else
            {
                rbWaterFlowRatelmin.Checked = true;
            }

            //添加静压单位转换   add on 20190522 by Vince
            if (SystemSetting.UserSetting.unitsSetting.settingESP == Unit.ut_Pressure)
            {
                rbEspPa.Checked = true;
            }
            else
            {
                rbEspInWG.Checked = true;
            }

            jctbBuildingName.Text = SystemSetting.UserSetting.defaultSetting.BuildingName;
            jctbFloorName.Text = SystemSetting.UserSetting.defaultSetting.FloorName;
            jctbRoomName.Text = SystemSetting.UserSetting.defaultSetting.RoomName;
            jctbFreshAirAreaName.Text = SystemSetting.UserSetting.defaultSetting.FreshAirAreaName;
            jctbIndoorName.Text = SystemSetting.UserSetting.defaultSetting.IndoorName;
            jctbOutdoorName.Text = SystemSetting.UserSetting.defaultSetting.OutdoorName;
            jctbControlName.Text = SystemSetting.UserSetting.defaultSetting.ControllerName;
            jctbExchangerName.Text=SystemSetting.UserSetting.defaultSetting.ExchangerName;

            string temperatureUnit = Unit.ut_Temperature_c;
            if (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE != Unit.ut_Temperature_c)
            {
                temperatureUnit = Unit.ut_Temperature_f;
            }

            jclblIndoorCoolingDB.Text = temperatureUnit;
            jclblIndoorCoolingWB.Text = temperatureUnit;
            jclblIndoorHeatingDB.Text = temperatureUnit;
            jclblOutdoorCoolingDB.Text = temperatureUnit;
            jclblOutdoorHeatingDB.Text = temperatureUnit;
            jclblOutdoorHeatingWB.Text = temperatureUnit;

            jctbIndoorCoolingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            jctbIndoorCoolingWB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            jctbIndoorHeatingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            jctbOutdoorCoolingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            jctbOutdoorHeatingDB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            jctbOutdoorHeatingWB.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");

            //Add Water source Inlet water Temp. on 20160615 by Yunxiao Lin
            jclblOutdoorCoolingIW.Text = temperatureUnit;
            jclblOutdoorHeatingIW.Text = temperatureUnit;
            jctbOutdoorCoolingIW.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");
            jctbOutdoorHeatingIW.Text = Unit.ConvertToControl(SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW,
                                                             UnitType.TEMPERATURE, temperatureUnit).ToString("n1");

            //jctbIndoorCoolingRH.Text = SystemSetting.UserSetting.defaultSetting.indoorCoolingRH.ToString("n2");
            //jctbOutdoorHeatingRH.Text = SystemSetting.UserSetting.defaultSetting.outdoorHeatingRH.ToString("n2");

            calRH((decimal)SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB, (decimal)SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB, true);
            calRH((decimal)SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB, (decimal)SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB, false);

            jctbRoomHeight.Text = SystemSetting.UserSetting.defaultSetting.RoomHeight.ToString("n1");

            trackBarInC.Value = SystemSetting.UserSetting.advancedSetting.indoorCooling;
            trackBarInH.Value = SystemSetting.UserSetting.advancedSetting.indoorHeating;
            trackBarOutC.Value = SystemSetting.UserSetting.advancedSetting.outdoorCooling;
            trackBarOutH.Value = SystemSetting.UserSetting.advancedSetting.outdoorHeating;

            if (!System.IO.Directory.Exists(SystemSetting.UserSetting.fileSetting.DXFFiles))
            {
                SystemSetting.UserSetting.fileSetting.DXFFiles = "C:\\GlobalVRF";
                //if (Util.IsChinese())
                //{
                //    lbDXFPath.Text = "请选择DXF存放目录";
                //}
                //else
                //{
                //    lbDXFPath.Text = "Please choose a directionary for saving DXF files";
                //}
                lbDXFPath.Text = Msg.GetResourceString("Msg_DXFPath");
            }
            lbDXFPath.Text = SystemSetting.UserSetting.fileSetting.DXFFiles;
            DXFPath = SystemSetting.UserSetting.fileSetting.DXFFiles;

            if (!System.IO.Directory.Exists(SystemSetting.UserSetting.fileSetting.reportFiles))            
            {
                SystemSetting.UserSetting.fileSetting.reportFiles = "C:\\GlobalVRF";
                lbReportPath.Text = Msg.GetResourceString("Msg_ReportPath");
                //if (Util.IsChinese())
                //{
                //    lbReportPath.Text = "请选择报表存放目录";
                //}
                //else
                //{
                //    lbReportPath.Text = "Please choose a directionary for saving report files";
                //}  
            }
            lbReportPath.Text = SystemSetting.UserSetting.fileSetting.reportFiles;
            reportPath = SystemSetting.UserSetting.fileSetting.reportFiles;

            //this.jccbAltitude.Checked = SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor;
            //是否启用高度修正是项目属性 modify on 20160819 by Yunxiao Lin
            this.jccbAltitude.Checked = thisProject.EnableAltitudeCorrectionFactor;
             
            //cbNewKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstNewKey;
            //cbNewKeyLast.Text = SystemSetting.UserSetting.keySetting.lastNewKey;
            //cbOpenKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstOpenKey;
            //cbOpenKeyLast.Text = SystemSetting.UserSetting.keySetting.lastOpenKey;
            //cbSaveKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstSaveKey;
            //cbSaveKeyLast.Text = SystemSetting.UserSetting.keySetting.lastSaveKey;
            //cbSaveAsKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstSaveAsKey;
            //cbSaveAsKeyLast.Text = SystemSetting.UserSetting.keySetting.lastSaveAsKey;
            //cbImportKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstImportKey;
            //cbImportKeyLast.Text = SystemSetting.UserSetting.keySetting.lastImportKey;
            //cbSettingKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstSettingKey;
            //cbSettingKeyLast.Text = SystemSetting.UserSetting.keySetting.lastSettingKey;
            //cbExitKeyFirst.Text = SystemSetting.UserSetting.keySetting.firstExitKey;
            //cbExitKeyLast.Text = SystemSetting.UserSetting.keySetting.lastExitKey;
          //  DoCalculateRH();
            _loading = false;
            _switchingTemperatureUnit = false;
        }

        


        private bool CheckData()
        {
            bool isValid = true;
            int byteLimite = 15;   //name字节限制   --add on 20180417 by Vince 

            if (jctbBuildingName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_BuildName")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else 
            {
                if (getBytes(jctbBuildingName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_BuildName"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbFloorName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_FloorName")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbFloorName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_FloorName"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbRoomName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_RoomName")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbRoomName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_RoomName"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbFreshAirAreaName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_FreshAirAreaName")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbFreshAirAreaName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_FreshAirAreaName"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbIndoorName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorUnitsName")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbIndoorName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_IndoorUnitsName"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbOutdoorName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorUnitsName")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbOutdoorName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_OutdoorUnitsName"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbControlName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_Controllers")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbControlName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(Msg.GetResourceString("Msg_Controllers"), byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }


            if (jctbExchangerName.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(this.jcLabel16.Text));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (getBytes(jctbExchangerName.Text) > byteLimite)
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BYTES_LIMITATION(this.jcLabel16.Text, byteLimite));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            double outData = 0;
            if (jctbIndoorCoolingDB.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorCoolingDB")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbIndoorCoolingDB.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorCoolingDB")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbIndoorCoolingWB.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorCoolingWB")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbIndoorCoolingWB.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorCoolingWB")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbIndoorCoolingRH.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorCoolingRH")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbIndoorCoolingRH.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorCoolingRH")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbIndoorHeatingDB.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_IndoorHeatingDB")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbIndoorHeatingDB.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_IndoorHeatingDB")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbOutdoorCoolingDB.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorCoolingDB")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbOutdoorCoolingDB.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorCoolingDB")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbOutdoorHeatingDB.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorHeatingDB")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbOutdoorHeatingDB.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorHeatingDB")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbOutdoorHeatingRH.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorHeatingRH")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbOutdoorHeatingRH.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorHeatingRH")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }
            //add water source inlet water tempertaure set 20160615 by Yunxiao Lin
            if (jctbOutdoorCoolingIW.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorCoolingIW")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbOutdoorCoolingIW.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorCoolingIW")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbOutdoorHeatingIW.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_OutdoorHeatingIW")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbOutdoorHeatingIW.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_OutdoorHeatingIW")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }

            if (jctbRoomHeight.Text == "")
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_NOTEMPTY(Msg.GetResourceString("Msg_RoomHeight")));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            else
            {
                if (!double.TryParse(jctbRoomHeight.Text, out outData))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_INVALIDNUM(Msg.GetResourceString("Msg_RoomHeight")));
                    tcSetting.SelectedIndex = 1;
                    return false;
                }
            }  

            if (!JCValidateGroup(pnlIndoorWorkingCondition))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return false;
            }
            if (!JCValidateGroup(pnlOutdoorWorkingCondition))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_PAYATTENTION);
                return false;
            }
            if (Convert.ToDecimal(jctbIndoorCoolingDB.Text) < Convert.ToDecimal(jctbIndoorCoolingWB.Text))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                tcSetting.SelectedIndex = 1;
                return false;
            }

            if (Convert.ToDecimal(jctbOutdoorHeatingDB.Text) < Convert.ToDecimal(jctbOutdoorHeatingWB.Text))
            {
                JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                tcSetting.SelectedIndex = 1;
                return false;
            }
            
            if (DXFPath == "")
            {
                JCMsg.ShowWarningOK(Msg.FILE_WARN_SELECTPATH("DXF"));
                tcSetting.SelectedIndex = 3;
                return false;
            }
            else
            {
                if (!System.IO.Directory.Exists(DXFPath))
                {
                    JCMsg.ShowWarningOK(Msg.FILE_WARN_NOTEXIST1(Msg.GetResourceString("Msg_DXFFilesDirectionary")));
                    tcSetting.SelectedIndex = 3;
                    return false;
                }
            }

            if (reportPath == "")
            {
                JCMsg.ShowWarningOK(Msg.FILE_WARN_SELECTPATH(Msg.GetResourceString("Msg_Report")));
                tcSetting.SelectedIndex = 3;
                return false;
            }
            else
            {
                if (!System.IO.Directory.Exists(reportPath))
                {
                    JCMsg.ShowWarningOK(Msg.FILE_WARN_NOTEXIST1(Msg.GetResourceString("Msg_ReportFilesDirectionary")));
                    tcSetting.SelectedIndex = 3;
                    return false;
                }
            }



            #region 
            /*
            //if (cbNewKeyFirst.Text == "" || cbNewKeyLast.Text == "" ||
            //    cbOpenKeyFirst.Text == "" || cbOpenKeyLast.Text == "" ||
            //    cbSaveKeyFirst.Text == "" || cbSaveKeyLast.Text == "" ||
            //    cbSaveAsKeyFirst.Text == "" || cbSaveAsKeyLast.Text == "" ||
            //    cbImportKeyFirst.Text == "" || cbImportKeyLast.Text == "" ||
            //    cbSettingKeyFirst.Text == "" || cbSettingKeyLast.Text == "" ||
            //    cbExitKeyFirst.Text == "" || cbExitKeyLast.Text == "")
            //{
            //    if (Util.IsChinese())
            //    {
            //        JCMsg.ShowWarningOK(Msg.VAL_NOTEMPTY("快捷键"));
            //    }
            //    else
            //    {
            //        JCMsg.ShowWarningOK(Msg.VAL_NOTEMPTY("Keyboard Shortcuts"));
            //    }
            //    tcSetting.SelectedIndex = 4;
            //    return false;
            //}
            
        //    List<string> keyList = new List<string>();
        //    if (keyList.Contains(cbNewKeyFirst.Text.ToString() + cbNewKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbNewKeyFirst.Text.ToString() + cbNewKeyLast.Text.ToString());
        //    }

        //    if (keyList.Contains(cbOpenKeyFirst.Text.ToString() + cbOpenKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbOpenKeyFirst.Text.ToString() + cbOpenKeyLast.Text.ToString());
        //    }

        //    if (keyList.Contains(cbSaveKeyFirst.Text.ToString() + cbSaveKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbSaveKeyFirst.Text.ToString() + cbSaveKeyLast.Text.ToString());
        //    }

        //    if (keyList.Contains(cbSaveAsKeyFirst.Text.ToString() + cbSaveAsKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbSaveAsKeyFirst.Text.ToString() + cbSaveAsKeyLast.Text.ToString());
        //    }

        //    if (keyList.Contains(cbImportKeyFirst.Text.ToString() + cbImportKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbImportKeyFirst.Text.ToString() + cbImportKeyLast.Text.ToString());
        //    }

        //    if (keyList.Contains(cbSettingKeyFirst.Text.ToString() + cbSettingKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbSettingKeyFirst.Text.ToString() + cbSettingKeyLast.Text.ToString());
        //    }

        //    if (keyList.Contains(cbExitKeyFirst.Text.ToString() + cbExitKeyLast.Text.ToString()))
        //    {
        //        if (Util.IsChinese())
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("快捷键"));
        //        }
        //        else
        //        {
        //            JCMsg.ShowWarningOK(Msg.CheckDuplicate("Keyboard Shortcuts"));
        //        }
        //        tcSetting.SelectedIndex = 4;
        //        return false;
        //    }
        //    else
        //    {
        //        keyList.Add(cbExitKeyFirst.Text.ToString() + cbExitKeyLast.Text.ToString());
        //    }
             * */
            #endregion

            return isValid;
        }

        private void SetData()
        {
            if (rbCapacitykw.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_kw;
            }
            else if (rbCapacityw.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_w;
            }
            else if (rbCapacityton.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_ton;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingPOWER = Unit.ut_Capacity_btu;
            }

            if (rbAirflowls.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_ls;
            }
            else if (rbAirflowmh.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_m3h;
            }
            else if (rbAirflowmhr.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_m3hr;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingAIRFLOW = Unit.ut_Airflow_cfm;
            }

            if (rbTemperaturec.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = Unit.ut_Temperature_c;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE = Unit.ut_Temperature_f;
            }

            if (rbLengthm.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingLENGTH = Unit.ut_Size_m;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingLENGTH = Unit.ut_Size_ft;
            }


            if (rbDemensionsmm.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingDimension = Unit.ut_Dimension_mm;
            }
            else 
            {
                SystemSetting.UserSetting.unitsSetting.settingDimension = Unit.ut_Dimension_inch;
            }

            if (rbDemensionsmmUnit.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingDimensionUnit = Unit.ut_Dimension_mm;
            }
            else 
            {
                SystemSetting.UserSetting.unitsSetting.settingDimensionUnit = Unit.ut_Dimension_inch;
            }

            if (rbWeightkg.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingWEIGHT = Unit.ut_Weight_kg;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingWEIGHT = Unit.ut_Weight_lbs;
            }

            if (rbAream2.Checked == true)
            {
                SystemSetting.UserSetting.unitsSetting.settingAREA = Unit.ut_Area_m2;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingAREA = Unit.ut_Area_ft2;
            }

            //增加Load Index 20160526 by Yunxiao Lin
            if (rbLoadIndexWm2.Checked)
            {
                SystemSetting.UserSetting.unitsSetting.settingLOADINDEX = Unit.ut_LoadIndex_w;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingLOADINDEX = Unit.ut_LoadIndex_MBH;
            }

            if (rbWaterFlowRatem3h.Checked)
            {
                SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate = Unit.ut_WaterFlowRate_m3h;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingWaterFlowRate = Unit.ut_WaterFlowRate_lmin;
            }

            //添加静压单位转换   add on 20190522 by Vince
            if (rbEspPa.Checked)
            {
                SystemSetting.UserSetting.unitsSetting.settingESP = Unit.ut_Pressure;
            }
            else
            {
                SystemSetting.UserSetting.unitsSetting.settingESP = Unit.ut_Pressure_inWG;
            }

            //if (rbHeightm.Checked == true)
            //{                
            //    SystemSetting.UserSetting.unitsSetting.settingHeight = Unit.ut_Size_m;
            //}

            //选不选都是“m”, modify on 20160526 by Yunxiao Lin
            SystemSetting.UserSetting.unitsSetting.settingHeight = Unit.ut_Size_m;

            SystemSetting.UserSetting.defaultSetting.BuildingName = jctbBuildingName.Text;
            SystemSetting.UserSetting.defaultSetting.FloorName = jctbFloorName.Text;
            SystemSetting.UserSetting.defaultSetting.RoomName = jctbRoomName.Text;
            SystemSetting.UserSetting.defaultSetting.FreshAirAreaName = jctbFreshAirAreaName.Text;
            SystemSetting.UserSetting.defaultSetting.IndoorName = jctbIndoorName.Text;
            SystemSetting.UserSetting.defaultSetting.OutdoorName = jctbOutdoorName.Text;
            SystemSetting.UserSetting.defaultSetting.ControllerName = jctbControlName.Text;
            SystemSetting.UserSetting.defaultSetting.ExchangerName = jctbExchangerName.Text;
            //SystemSetting.UserSetting.fileSetting.EnableAltitudeCorrectionFactor = jccbAltitude.Checked;
            //是否启用海拔高度修正是项目属性 modify on 20160819 by Yunxiao Lin
            thisProject.EnableAltitudeCorrectionFactor = jccbAltitude.Checked;

            string temperatureUnit = Unit.ut_Temperature_c;
            if (SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE != Unit.ut_Temperature_c)
            {
                temperatureUnit = Unit.ut_Temperature_f;
            }

            SystemSetting.UserSetting.defaultSetting.IndoorCoolingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorCoolingDB.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorCoolingWB.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorHeatingDB.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorCoolingDB.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingDB.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingWB.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            //Add Water source inlet water Temp. on 20160615 by Yunxiao Lin
            SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorCoolingIW.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingIW.Text),
                                                                                            UnitType.TEMPERATURE,
                                                                                            temperatureUnit);
            
            SystemSetting.UserSetting.defaultSetting.IndoorCoolingRH = ConvertStringToDouble(jctbIndoorCoolingRH.Text);
            SystemSetting.UserSetting.defaultSetting.OutdoorHeatingRH = ConvertStringToDouble(jctbOutdoorHeatingRH.Text);
            SystemSetting.UserSetting.defaultSetting.RoomHeight = ConvertStringToDouble(jctbRoomHeight.Text);

            SystemSetting.UserSetting.advancedSetting.indoorCooling = trackBarInC.Value;
            SystemSetting.UserSetting.advancedSetting.indoorHeating = trackBarInH.Value;
            SystemSetting.UserSetting.advancedSetting.outdoorCooling = trackBarOutC.Value;
            SystemSetting.UserSetting.advancedSetting.outdoorHeating = trackBarOutH.Value;


            if (System.IO.Directory.Exists(lbDXFPath.Text))
            {
                SystemSetting.UserSetting.fileSetting.DXFFiles = DXFPath;
            }
            else
            {
                SystemSetting.UserSetting.fileSetting.DXFFiles = "";
            }

            if (System.IO.Directory.Exists(lbReportPath.Text))
            {
                SystemSetting.UserSetting.fileSetting.reportFiles = reportPath;
            }
            else
            {
                SystemSetting.UserSetting.fileSetting.reportFiles = "";
            }
            string strLanguage = LangType.ENGLISH;
            string strSwitchLanguage = "tbtnLanguage_en";
            if (jcCoboxLanguage.Text == "中文(简体)")
            {
                strLanguage = LangType.CHINESE;
                strSwitchLanguage = "tbtnLanguage_zh";
            }
            else if(jcCoboxLanguage.Text == "中文(繁體)")
            {
                strLanguage = LangType.CHINESE_TRADITIONAL;
                strSwitchLanguage = "tbtnLanguage_zht";
            }
            else if (jcCoboxLanguage.Text == "Français")
            {
                strLanguage = LangType.FRENCH;
                strSwitchLanguage = "tbtnLanguage_fr";
            }
            else if (jcCoboxLanguage.Text == "España")
            {
                strLanguage = LangType.SPANISH;
                strSwitchLanguage = "tbtnLanguage_es";
            }
            else if (jcCoboxLanguage.Text == "Türk")
            {
                strLanguage = LangType.TURKISH;
                strSwitchLanguage = "tbtnLanguage_tr";
            }
            else if (jcCoboxLanguage.Text == "Deutsch")
            {
                strLanguage = LangType.GERMANY;
                strSwitchLanguage = "tbtnLanguage_de";
            }
            else if (jcCoboxLanguage.Text == "Italiano")
            {
                strLanguage = LangType.ITALIAN;
                strSwitchLanguage = "tbtnLanguage_it";
            }
            else if (jcCoboxLanguage.Text == "Brazilian Portuguese")
            {
                strLanguage = LangType.BRAZILIAN_PORTUGUESS;
                strSwitchLanguage = "tbtnLanguage_pt_BR";
            }
            else
            {
                strLanguage = LangType.ENGLISH;
                strSwitchLanguage = "tbtnLanguage_en";
            }
            CurrLanguage = strSwitchLanguage;
            SystemSetting.UserSetting.fileSetting.settingLanguage = strLanguage;


            //SystemSetting.UserSetting.keySetting.firstNewKey = cbNewKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastNewKey = cbNewKeyLast.Text;
            //SystemSetting.UserSetting.keySetting.firstOpenKey = cbOpenKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastOpenKey = cbOpenKeyLast.Text;
            //SystemSetting.UserSetting.keySetting.firstSaveKey = cbSaveKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastSaveKey = cbSaveKeyLast.Text;
            //SystemSetting.UserSetting.keySetting.firstSaveAsKey = cbSaveAsKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastSaveAsKey = cbSaveAsKeyLast.Text;
            //SystemSetting.UserSetting.keySetting.firstImportKey = cbImportKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastImportKey = cbImportKeyLast.Text;
            //SystemSetting.UserSetting.keySetting.firstSettingKey = cbSettingKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastSettingKey = cbSettingKeyLast.Text;
            //SystemSetting.UserSetting.keySetting.firstExitKey = cbExitKeyFirst.Text;
            //SystemSetting.UserSetting.keySetting.lastExitKey = cbExitKeyLast.Text;
        }

        private void ChangeTempUnit(bool isTempC)
        {
            _switchingTemperatureUnit = true;

            string temperatureUnit = Unit.ut_Temperature_c;
            if (!isTempC)
            {
                temperatureUnit = Unit.ut_Temperature_f;
            } 

            SetMinMaxTemperature(temperatureUnit);
            jclblIndoorCoolingDB.Text = temperatureUnit;
            jclblIndoorCoolingWB.Text = temperatureUnit;
            jclblIndoorHeatingDB.Text = temperatureUnit;
            jclblOutdoorCoolingDB.Text = temperatureUnit;
            jclblOutdoorHeatingDB.Text = temperatureUnit;
            jclblOutdoorHeatingWB.Text = temperatureUnit;
            //Add Water source Inlet water Temp. on 20160615 by Yunxiao Lin
            jclblOutdoorCoolingIW.Text = temperatureUnit;
            jclblOutdoorHeatingIW.Text = temperatureUnit;

            if (isTempC)
            {
                jctbIndoorCoolingDB.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorCoolingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbIndoorCoolingWB.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorCoolingWB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbIndoorHeatingDB.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbIndoorHeatingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorCoolingDB.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorCoolingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorHeatingDB.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorHeatingWB.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingWB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorCoolingIW.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorCoolingIW.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorHeatingIW.Text = Unit.ConvertToSource(ConvertStringToDouble(jctbOutdoorHeatingIW.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
            }
            else
            {
                jctbIndoorCoolingDB.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbIndoorCoolingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbIndoorCoolingWB.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbIndoorCoolingWB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbIndoorHeatingDB.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbIndoorHeatingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorCoolingDB.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbOutdoorCoolingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorHeatingDB.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbOutdoorHeatingDB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorHeatingWB.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbOutdoorHeatingWB.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorCoolingIW.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbOutdoorCoolingIW.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
                jctbOutdoorHeatingIW.Text = Unit.ConvertToControl(ConvertStringToDouble(jctbOutdoorHeatingIW.Text),
                                                                 UnitType.TEMPERATURE, Unit.ut_Temperature_f).ToString("n1");
            }
        
            _switchingTemperatureUnit = false;
        }

        private double ConvertStringToDouble(string doubleString)
        {
            double returnDouble = 0;
            if (double.TryParse(doubleString, out returnDouble))
                return returnDouble;
            else
                return 0;
        }

        private string SelectFolder(string selectPath)
        {
            string folderPath = "";
            try
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = Msg.GetResourceString("FlolderDes");
                    fbd.ShowNewFolderButton = false;
                    if (selectPath != "")
                    {
                        fbd.SelectedPath = selectPath;
                    }
                    if (fbd.ShowDialog(this) == DialogResult.OK)
                    {
                        folderPath = fbd.SelectedPath.ToString();
                    }
                    else
                    {
                        folderPath = selectPath;
                    }
                }
                return folderPath;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion                

        private void jcButtonExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (CommonBLL.IsChinese())
            {
                saveFileDialog.Filter = "xml文件|*.xml";
            }
            else
            {
                saveFileDialog.Filter = "xml file|*.xml";
            }
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            saveFileDialog.FileName = "Settings.config.xml";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (System.IO.File.Exists(System.Windows.Forms.Application.StartupPath +
                                           @"\NVRF\Settings.config.xml"))
                {
                    System.IO.File.Copy(System.Windows.Forms.Application.StartupPath +
                                           @"\NVRF\Settings.config.xml", saveFileDialog.FileName, true);
                }
                else
                {
                    SystemSetting.Serialize(saveFileDialog.FileName);
                }
                JCMsg.ShowInfoOK(Msg.SETTING_EXPORT_SUCCESS);
            }
        }

        private void jcButtonImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (CommonBLL.IsChinese())
            {
                openFileDialog.Filter = "xml文件|*.xml";
            }
            else
            {
                openFileDialog.Filter = "xml file|*.xml";
            }
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (checkFile(openFileDialog.FileName))
                {
                    System.IO.File.Copy(openFileDialog.FileName, System.Windows.Forms.Application.StartupPath +
                                           @"\NVRF\Settings.config.xml", true);
                    SystemSetting.UserSetting = SystemSetting.Deserialize();
                    LoadData();
                    JCMsg.ShowInfoOK(Msg.SETTING_IMPORT_SUCCESS);                    
                }
                else
                {
                    JCMsg.ShowWarningOK(Msg.FILE_WARN_IMPORTFORMAT);
                }
            }            
        }

        private bool checkFile(string filePath)
        {
            bool returnCheck = false;

            System.Xml.XmlDocument docXML = new System.Xml.XmlDocument();
            try
            {
                docXML.Load(filePath);
            }
            catch
            {
                return returnCheck;
            }

            System.Xml.XmlNode root = docXML.SelectSingleNode("SystemSettingModel");
            if (root == null)
            {
                return returnCheck;
            }

            return true;
        }

        private void jctxtTemperature_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox tb = sender as TextBox;
                JCValidateGroup((sender as TextBox).Parent as Panel);


                //if (JCValidateGroup((sender as TextBox).Parent as Panel))
                //{
                //    string name = (sender as TextBox).Name;
                //    bool isInd = name.Contains("Indoor");
                //    string isKey = name.Substring(name.Length - 2, 2); ;
                //    string ut_temperature = this.jclblIndoorCoolingDB.Text;
                //    decimal dt = 0;
                //    decimal wt = 0;
                //    decimal rh = 0;
                //    if (isInd)
                //    {

                //        dt = (decimal)Unit.ConvertToSource(double.Parse(this.jctbIndoorCoolingDB.Text), UnitType.TEMPERATURE, ut_temperature);
                //        wt = (decimal)Unit.ConvertToSource(double.Parse(this.jctbIndoorCoolingWB.Text), UnitType.TEMPERATURE, ut_temperature);
                //        rh = (decimal)Unit.ConvertToSource(double.Parse(this.jctbIndoorCoolingRH.Text), UnitType.TEMPERATURE, ut_temperature);

                //        //    calRH(dt, wt, isInd);
                      
                       

                //    }
                //    else
                //    {
                //        dt = (decimal)Unit.ConvertToSource(double.Parse(this.jctbOutdoorHeatingDB.Text), UnitType.TEMPERATURE, ut_temperature);
                //        wt = (decimal)Unit.ConvertToSource(double.Parse(this.jctbOutdoorHeatingWB.Text), UnitType.TEMPERATURE, ut_temperature);
                //        rh = (decimal)Unit.ConvertToSource(double.Parse(this.jctbOutdoorHeatingRH.Text), UnitType.TEMPERATURE, ut_temperature);

                //      //  calRH(dt, wt, rh, isInd, isKey);
                //    }

                //    if (dt < wt)
                //    {
                //        JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN("WB", "DB"));
                //        return;
                //    }
                //}
            }
        }


        // 计算相对湿度
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateRH()
        {
            //this.jctbIndoorCoolingDB.TextChanged -= new EventHandler(jctbIndoorCoolingDB_TextChanged);
            //this.jctbIndoorCoolingWB.TextChanged -= new EventHandler(jctbIndoorCoolingWB_TextChanged);
            //this.jctbIndoorCoolingRH.TextChanged -= new EventHandler(jctbIndoorCoolingRH_TextChanged);
            //this.jctbOutdoorHeatingDB.TextChanged -= new EventHandler(jctbOutdoorHeatingDB_TextChanged);
            //this.jctbOutdoorHeatingWB.TextChanged -= new EventHandler(jctbOutdoorHeatingWB_TextChanged);
            //this.jctbOutdoorHeatingRH.TextChanged -= new EventHandler(jctbOutdoorHeatingRH_TextChanged);
            string ut_temperature = this.jclblIndoorCoolingDB.Text;
            FormulaCalculate fc = new FormulaCalculate();
            decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
            if (!string.IsNullOrEmpty(this.jctbIndoorCoolingDB.Text) && !string.IsNullOrEmpty(this.jctbIndoorCoolingWB.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbIndoorCoolingDB.Text), UnitType.TEMPERATURE, ut_temperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbIndoorCoolingWB.Text), UnitType.TEMPERATURE, ut_temperature);

                 
                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                this.jctbIndoorCoolingRH.Text = (rh * 100).ToString("n0"); 
            }
            if (!string.IsNullOrEmpty(this.jctbOutdoorHeatingDB.Text) && !string.IsNullOrEmpty(this.jctbOutdoorHeatingWB.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbOutdoorHeatingDB.Text), UnitType.TEMPERATURE, ut_temperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbOutdoorHeatingWB.Text), UnitType.TEMPERATURE, ut_temperature);


                double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                this.jctbOutdoorHeatingRH.Text = (rh * 100).ToString("n0");
            }
        }

        // 计算相对湿度(根据对应的DB，WB,RH 改变值) 室内机
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateByOptionInd(string Opt)
        {
            //this.jctbIndoorCoolingDB.TextChanged -= new EventHandler(jctbIndoorCoolingDB_TextChanged);
            //this.jctbIndoorCoolingWB.TextChanged -= new EventHandler(jctbIndoorCoolingWB_TextChanged);
            //this.jctbIndoorCoolingRH.TextChanged -= new EventHandler(jctbIndoorCoolingRH_TextChanged);

            string ut_temperature = this.jclblIndoorCoolingDB.Text;
            if (!string.IsNullOrEmpty(this.jctbIndoorCoolingDB.Text) && !string.IsNullOrEmpty(this.jctbIndoorCoolingWB.Text) && !string.IsNullOrEmpty(this.jctbIndoorCoolingRH.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbIndoorCoolingDB.Text), UnitType.TEMPERATURE, ut_temperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbIndoorCoolingWB.Text), UnitType.TEMPERATURE, ut_temperature);
                double rhcool = Convert.ToDouble(this.jctbIndoorCoolingRH.Text);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                if (Opt == UnitTemperature.WB.ToString())
                {
                     double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
                    
                    if (this.jctbIndoorCoolingRH.Text != (rh * 100).ToString("n0"))
                    {
                        this.jctbIndoorCoolingRH.Text = (rh * 100).ToString("n0");
                    }
                }
                else if (Opt == UnitTemperature.DB.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                   
                    if (this.jctbIndoorCoolingWB.Text != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.jctbIndoorCoolingWB.Text = wb.ToString("n1");
                        }
                    }
                }
                else if (Opt == UnitTemperature.RH.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                   
                    if (this.jctbIndoorCoolingWB.Text != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.jctbIndoorCoolingWB.Text = wb.ToString("n1");
                        }

                    }
                }

            }
            //this.jctbIndoorCoolingDB.TextChanged += new EventHandler(jctbIndoorCoolingDB_TextChanged);
            //this.jctbIndoorCoolingWB.TextChanged += new EventHandler(jctbIndoorCoolingWB_TextChanged);
            //this.jctbIndoorCoolingRH.TextChanged += new EventHandler(jctbIndoorCoolingRH_TextChanged);
        }


        // 计算相对湿度(根据对应的DB，WB,RH 改变值) 室外机
        /// <summary>
        /// 计算相对湿度
        /// </summary>
        private void DoCalculateByOptionOut(string Opt)
        {

            //this.jctbOutdoorHeatingDB.TextChanged -= new EventHandler(jctbOutdoorHeatingDB_TextChanged);
            //this.jctbOutdoorHeatingWB.TextChanged -= new EventHandler(jctbOutdoorHeatingWB_TextChanged);
            //this.jctbOutdoorHeatingRH.TextChanged -= new EventHandler(jctbOutdoorHeatingRH_TextChanged);
            string ut_temperature = this.jclblIndoorCoolingDB.Text;
            if (!string.IsNullOrEmpty(this.jctbOutdoorHeatingDB.Text) && !string.IsNullOrEmpty(this.jctbOutdoorHeatingWB.Text) && !string.IsNullOrEmpty(this.jctbOutdoorHeatingRH.Text))
            {
                double dbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbOutdoorHeatingDB.Text), UnitType.TEMPERATURE, ut_temperature);
                double wbcool = Unit.ConvertToSource(Convert.ToDouble(this.jctbOutdoorHeatingWB.Text), UnitType.TEMPERATURE, ut_temperature);


               double rhcool = 0;

               double.TryParse(this.jctbOutdoorHeatingRH.Text,out rhcool);

                FormulaCalculate fc = new FormulaCalculate();
                decimal pressure = fc.GetPressure(Convert.ToDecimal(thisProject.Altitude));
                if (Opt == UnitTemperature.WB.ToString())
                {
                    double rh = Convert.ToDouble(fc.GetRH(Convert.ToDecimal(dbcool), Convert.ToDecimal(wbcool), pressure));
	           
                    if (this.jctbOutdoorHeatingRH.Text != (rh * 100).ToString("n0"))
                    {
                        this.jctbOutdoorHeatingRH.Text = (rh * 100).ToString("n0");
                    }
                }
                else if (Opt == UnitTemperature.DB.ToString())
                {
                    double wb =  Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                    if (this.jctbOutdoorHeatingWB.Text != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.jctbOutdoorHeatingWB.Text = wb.ToString("n1");
                        }
                    }
                }
                else if (Opt == UnitTemperature.RH.ToString())
                {
                    double wb = Convert.ToDouble(fc.GetWTByDT(Convert.ToDecimal(dbcool), Convert.ToDecimal(rhcool / 100), pressure));
                   
                    if (this.jctbOutdoorHeatingWB.Text != wb.ToString("n1"))
                    {
                        if (rhcool != 0)
                        {
                            this.jctbOutdoorHeatingWB.Text = wb.ToString("n1");
                        }

                    }
                }

            }
            //this.jctbOutdoorHeatingDB.TextChanged += new EventHandler(jctbOutdoorHeatingDB_TextChanged);
            //this.jctbOutdoorHeatingWB.TextChanged += new EventHandler(jctbOutdoorHeatingWB_TextChanged);
            //this.jctbOutdoorHeatingRH.TextChanged += new EventHandler(jctbOutdoorHeatingRH_TextChanged);
        }

       

        void calRH(decimal dt, decimal wt, bool isIn)
        {
            FormulaCalculate fcal = new FormulaCalculate();
            decimal p = fcal.GetPressure(thisProject.Altitude);
           
            decimal rh = fcal.GetRH(dt, wt, p);

            if (isIn)
                this.jctbIndoorCoolingRH.Text = (rh * 100).ToString("n0");
            else
                this.jctbOutdoorHeatingRH.Text = (rh * 100).ToString("n0");
        }





        private void jctbOutdoorCoolingIW_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (JCValidateGroup(tb.Parent as Panel))
            {
                //bool isCool = tb.Name.Contains("Cool");
                string ut_temperature = this.jclblIndoorCoolingDB.Text;
                JCValidateGroup((sender as TextBox).Parent as Panel);
                //decimal iw = (decimal)Unit.ConvertToSource(double.Parse(tb.Text), UnitType.TEMPERATURE, ut_temperature);
                //if (iw < 10 || iw > 45)
                //{
                //    string strmax = Unit.ConvertToControl(45d, UnitType.TEMPERATURE, ut_temperature).ToString("n0") + ut_temperature;
                //    string strmin = Unit.ConvertToControl(10d, UnitType.TEMPERATURE, ut_temperature).ToString("n0") + ut_temperature;
                    
                //    JCMsg.ShowWarningOK(Msg.WARNING_TXT_BETWEEN(strmin,strmax));
                //    return;
                //}
            }
        }
         

        private void tbtnChangeLanguage_Click(object sender, EventArgs e)
        {
            //DoSwitchLanguage((sender as ToolStripMenuItem).Name);
            //if (this.WindowState == FormWindowState.Maximized)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //    this.WindowState = FormWindowState.Maximized;
            //}
        }

        private void frmSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
           // RefreshLanguage();
        }

        private void jctbIndoorCoolingDB_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _switchingTemperatureUnit) return;

            if (jctbIndoorCoolingDB.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctbIndoorCoolingDB.Text) && NumberUtil.IsNumber(jctbIndoorCoolingWB.Text))
                {
                    JCValidateGroup((sender as TextBox).Parent as Panel);
                    if (Convert.ToDecimal(jctbIndoorCoolingDB.Text) < Convert.ToDecimal(jctbIndoorCoolingWB.Text))
                    {
                        JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                        return;
                    }
                    if (JCValidateSingle(jctbIndoorCoolingDB) && JCValidateSingle(jctbIndoorCoolingRH))
                    {
                        DoCalculateByOptionInd(UnitTemperature.DB.ToString());
                    } 
                }
                ValidateSingle();
            }
        }

        private void jctbIndoorCoolingWB_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _switchingTemperatureUnit) return;

            if (jctbIndoorCoolingWB.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctbIndoorCoolingWB.Text) && NumberUtil.IsNumber(jctbIndoorCoolingDB.Text))
                {
                    JCValidateGroup((sender as TextBox).Parent as Panel);
                    if (Convert.ToDecimal(jctbIndoorCoolingDB.Text) < Convert.ToDecimal(jctbIndoorCoolingWB.Text))
                    {
                        JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                        return;
                    }
                    if (JCValidateSingle(jctbIndoorCoolingDB) && JCValidateSingle(jctbIndoorCoolingWB))
                    {
                        DoCalculateByOptionInd(UnitTemperature.WB.ToString());
                    }
                }
                ValidateSingle();
            }
        }

        private void jctbIndoorCoolingRH_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _switchingTemperatureUnit) return;

            if (jctbIndoorCoolingRH.Text.Length > 1)
            {
                if (NumberUtil.IsNumber(jctbIndoorCoolingRH.Text))
                {

                    JCValidateGroup((sender as TextBox).Parent as Panel);
                    if (Convert.ToDecimal(jctbIndoorCoolingRH.Text)>100)
                    {
                        return;
                    }
                    if (JCValidateSingle(jctbIndoorCoolingRH) && JCValidateSingle(jctbIndoorCoolingDB))
                    {
                        DoCalculateByOptionInd(UnitTemperature.RH.ToString());
                    }
                }
                ValidateSingle();
            }
        }
        
        private void jctbOutdoorHeatingDB_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _switchingTemperatureUnit) return;

            if (NumberUtil.IsNumber(jctbOutdoorHeatingDB.Text) && NumberUtil.IsNumber(jctbOutdoorHeatingWB.Text))
            {
                JCValidateGroup((sender as TextBox).Parent as Panel);
                if (jctbOutdoorHeatingWB.Text.Length < 1)
                    return;
                if (Convert.ToDecimal(jctbOutdoorHeatingDB.Text) < Convert.ToDecimal(jctbOutdoorHeatingWB.Text))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString()));
                    return;
                }
                if (JCValidateSingle(jctbOutdoorHeatingDB) && JCValidateSingle(jctbOutdoorHeatingRH))
                {
                    DoCalculateByOptionOut(UnitTemperature.DB.ToString());
                }
                ValidateSingle();
            }
        }

        private void ValidateSingle()
        {
            JCValidateSingle(jctbOutdoorHeatingDB);
            JCValidateSingle(jctbOutdoorHeatingRH);
            JCValidateSingle(jctbOutdoorHeatingWB);
            JCValidateSingle(jctbIndoorCoolingDB);
            JCValidateSingle(jctbIndoorCoolingWB);
            JCValidateSingle(jctbIndoorCoolingRH);
        }

        private void jctbOutdoorHeatingWB_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _switchingTemperatureUnit) return;

            if (NumberUtil.IsNumber(jctbOutdoorHeatingWB.Text) && NumberUtil.IsNumber(jctbOutdoorHeatingDB.Text))
            {

                JCValidateGroup((sender as TextBox).Parent as Panel); 
                if (Convert.ToDecimal(jctbOutdoorHeatingDB.Text) < Convert.ToDecimal(jctbOutdoorHeatingWB.Text))
                {
                    JCMsg.ShowWarningOK(Msg.WARNING_TXT_LESSTHAN(UnitTemperature.WB.ToString(), UnitTemperature.DB.ToString())); 
                    return;
                }
                if (JCValidateSingle(jctbOutdoorHeatingDB) && JCValidateSingle(jctbOutdoorHeatingWB))
                {
                    DoCalculateByOptionOut(UnitTemperature.WB.ToString());
                }
                ValidateSingle();
            }
        }

        private void jctbOutdoorHeatingRH_TextChanged(object sender, EventArgs e)
        {
            if (_loading || _switchingTemperatureUnit) return;

            if (NumberUtil.IsNumber(jctbOutdoorHeatingRH.Text))
            {

                JCValidateGroup((sender as TextBox).Parent as Panel);
                if (Convert.ToDecimal(jctbOutdoorHeatingRH.Text) > 100)
                {
                    return;
                } 
                if (JCValidateSingle(jctbOutdoorHeatingRH) && JCValidateSingle(jctbOutdoorHeatingDB))
                {
                    DoCalculateByOptionOut(UnitTemperature.RH.ToString());
                }
                ValidateSingle();
            } 
        }

        /// <summary>
        /// 获取字符串的字节数   --add on 20180417 by Vince    
        /// </summary>
        /// <param name="mainRegion"></param>       
        /// <param name="str"></param>       
        private int getBytes(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            byte[] byteArr = Encoding.Default.GetBytes(str); 

            return byteArr.Length;
        }
     
       
    }
}
