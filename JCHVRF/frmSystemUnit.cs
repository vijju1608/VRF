using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JCHVRF.Model;
using JCHVRF.BLL;
using JCHVRF.MyPipingBLL;
using JCBase.Utility;
using JCBase.UI;
using JCBase.Util;
using JCHVRF.Const;
namespace JCHVRF
{
    public partial class frmSystemUnit :JCBase.UI.JCForm
    {
        public Project thisProject;
        public RoomIndoor indoor;
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;
        string ut_weight;
        string ut_dimension;
        string ut_esp;
         
        //public frmSystemUnit()
        //{
        //    InitializeComponent();
        //}

        public frmSystemUnit(Project thisProj,RoomIndoor ri)
          {
              InitializeComponent();
              thisProject = thisProj;
              indoor = ri;
              BindUnit();
          }


        private void BindUnit()
        {
            ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;
            ut_esp = SystemSetting.UserSetting.unitsSetting.settingESP;
        }

        private void frmSystemUnit_Load(object sender, EventArgs e)
        {
            JCSetLanguage();
            dgvSystemInfo.Rows.Clear();  //清空数据         
            BindSysTableInfo(indoor);
            NameArray_Indoor arr = new NameArray_Indoor(); 
            Global.SetDGVHeaderText(ref dgvSystemInfo, arr.SysInfo_HeaderText);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// 绑定系统信息表   --add on 20170610 by Lingjia Qiu
        /// <summary>
        /// 绑定系统信息表
        /// </summary>
        /// <param name="riItem"></param>
        private void BindSysTableInfo(RoomIndoor r)
        {
            if (thisProject.SystemList.Count == 0)
                return;

            dgvSystemInfo.Rows.Clear();  //清空数据                                    
            ProjectBLL bll = new ProjectBLL(thisProject);

            foreach (SystemVRF sysItem in thisProject.SystemList)
            {
                if (sysItem.OutdoorItem == null)
                {
                    this.dgvSystemInfo.Visible = false;
                    return;
                }
                if (sysItem.Id.Equals(r.SystemID))
                {
                    //this.jclblUnitInfo.Text = sysItem.Name + ShowText.SystemInfo;
                    double totestIndCap_h = 0;
                    double totestIndCap_c = getTotestIndCap_c(sysItem, bll, out totestIndCap_h);
                    string coolingReqd = "";
                    if (totestIndCap_c != 0d)
                        coolingReqd = Unit.ConvertToControl(totestIndCap_c, UnitType.POWER, ut_power).ToString("n1");
                    else
                        coolingReqd = "-";

                    double sysRH = bll.CalculateRH(sysItem.DBHeating, sysItem.WBHeating, thisProject.Altitude);
                    //this.dgvSystemInfo.Rows[0].Height = 30;
                    //先添加一条系统室外机记录
                    this.dgvSystemInfo.Rows.Add(
                                                  "",
                                                  coolingReqd,   // cooling Req'd
                                                  totestIndCap_h != 0d ? Unit.ConvertToControl(totestIndCap_h, UnitType.POWER, ut_power).ToString("n1") : "-",   //Heating Req'd
                                                  "-",   //Sensible Req'd
                                                  sysItem.Name,   //系统名字
                                                  sysItem.OutdoorItem.AuxModelName,   //室外机型号
                                                  Unit.ConvertToControl(sysItem.CoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),   //Cooling Actual
                                                  Unit.ConvertToControl(sysItem.HeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),    //Heating Actual
                                                  "-",
                                                  Unit.ConvertToControl(sysItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //CoolingDB
                                                  "-",   //CoolingWB
                                                  Unit.ConvertToControl(sysItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //HeatingDB
                                                  Unit.ConvertToControl(sysItem.WBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //HeatingWB
                                                 (sysRH * 100).ToString("n0")  //RH
                                                );


                    //添加系统室内机记录
                    List<RoomIndoor> riItemList = bll.GetSelectedIndoorBySystem(sysItem.Id);
                    int colorIndex = 0;
                    string RoomId = "";
                    string SensibleHeat = "";
                    foreach (RoomIndoor riItem in riItemList)
                    {


                        double riRH = (new ProjectBLL(thisProject)).CalculateRH(riItem.DBCooling, riItem.WBCooling, thisProject.Altitude);
                        //添加不基于房间的室内机riItem.IndoorName 改为 DisplayRoom
                        string DisplayRoom = string.IsNullOrEmpty(riItem.DisplayRoom) ? riItem.IndoorName : riItem.DisplayRoom + ":" + riItem.IndoorName;
                        string RqCoolingCapacity = riItem.RqCoolingCapacity != 0d ? Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") : "-";
                        string RqHeatingCapacity = riItem.RqHeatingCapacity != 0d ? Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") : "-";
                        string RqSensibleHeat = riItem.RqSensibleHeat != 0d ? Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") : "-";
                        string RoomName = "-";
                        if (!string.IsNullOrEmpty(riItem.RoomName) && !string.IsNullOrEmpty(riItem.RoomID))
                        {
                            if (RoomId != riItem.RoomID)
                            {
                                RoomName = riItem.RoomName;
                                Room ri = bll.GetRoom(riItem.RoomID);
                                RqCoolingCapacity = ri.RqCapacityCool != 0d ? Unit.ConvertToControl(ri.RqCapacityCool, UnitType.POWER, ut_power).ToString("n1") : " ";
                                RqHeatingCapacity = ri.RqCapacityHeat != 0d ? Unit.ConvertToControl(ri.RqCapacityHeat, UnitType.POWER, ut_power).ToString("n1") : " ";
                                RqSensibleHeat = ri.SensibleHeat != 0d ? Unit.ConvertToControl(ri.SensibleHeat, UnitType.POWER, ut_power).ToString("n1") : "-";
                                RoomId = riItem.RoomID;
                                SensibleHeat = RqSensibleHeat;
                            }
                            else
                            {
                                RoomName = "";
                                RqCoolingCapacity = "";
                                RqHeatingCapacity = "";
                                RqSensibleHeat = ""; 
                                //if (!string.IsNullOrEmpty(SensibleHeat) && SensibleHeat == "-")
                                //    RqSensibleHeat = "-"; 
                            }
                            
                        }

                        this.dgvSystemInfo.Rows.Add(
                                                RoomName,
                            //riItem.IsAuto ? Unit.ConvertToControl(riItem.RqCoolingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-",   // cooling Req'd
                                                RqCoolingCapacity,   // cooling Req'd
                                                RqHeatingCapacity,   //Heating Req'd
                                                RqSensibleHeat,
                            //riItem.IsAuto ? Unit.ConvertToControl(riItem.RqSensibleHeat, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-",   //Sensible Req'd
                                                riItem.IndoorName,   //室内机名字
                                                thisProject.BrandCode == "Y" ? riItem.IndoorItem.Model_York : riItem.IndoorItem.Model_Hitachi,   //室内机型号
                                                Unit.ConvertToControl(riItem.ActualCoolingCapacity, UnitType.POWER, ut_power).ToString("n1"),   //Cooling Actual  
                            //riItem.IsAuto ?Unit.ConvertToControl(riItem.RqHeatingCapacity, UnitType.POWER, ut_power).ToString("n1") + ut_power : "-",   //Heating Req'd 
                                                Unit.ConvertToControl(riItem.ActualHeatingCapacity, UnitType.POWER, ut_power).ToString("n1"),   //Heating Actual
                                                Unit.ConvertToControl(riItem.ActualSensibleHeat, UnitType.POWER, ut_power).ToString("n1"),   //Sensible Actual
                                                Unit.ConvertToControl(riItem.DBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //CoolingDB
                                                Unit.ConvertToControl(riItem.WBCooling, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //CoolingWB
                                                Unit.ConvertToControl(riItem.DBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1"),   //HeatingDB
                                                riItem.WBHeating != 0d ? Unit.ConvertToControl(riItem.WBHeating, UnitType.TEMPERATURE, ut_temperature).ToString("n1") : "-",   //HeatingWB
                                                (riRH * 100).ToString("n0")  //RH
                                                );
                        colorIndex++;
                        string wType;
                        if (!CommonBLL.MeetRoomRequired(riItem, thisProject, 0, thisProject.RoomIndoorList, out wType))
                        {
                            dgvSystemInfo.Rows[colorIndex].Cells[1].Style.ForeColor = Color.Chocolate;
                            switch (wType)
                            {
                                case "reqCool":
                                    dgvSystemInfo.Rows[colorIndex].Cells[4].Style.ForeColor = Color.Chocolate;
                                    dgvSystemInfo.Rows[colorIndex].Cells[5].Style.ForeColor = Color.Chocolate;
                                    break;
                                case "reqHeat":
                                    dgvSystemInfo.Rows[colorIndex].Cells[10].Style.ForeColor = Color.Chocolate;
                                    dgvSystemInfo.Rows[colorIndex].Cells[11].Style.ForeColor = Color.Chocolate;
                                    break;
                                case "sensible":
                                    dgvSystemInfo.Rows[colorIndex].Cells[6].Style.ForeColor = Color.Chocolate;
                                    dgvSystemInfo.Rows[colorIndex].Cells[7].Style.ForeColor = Color.Chocolate;
                                    break;

                            }

                        }


                    }

                    break;
                }

            }
            dgvSystemInfo.ClearSelection();   //取消默认选中
            this.dgvSystemInfo.Visible = true;
        }

        /// 获取制冷制热需求容量   --add on 20170610 by Lingjia Qiu
        /// <summary>
        /// 获取制冷制热需求容量
        /// </summary>
        /// <param name="riItem"></param>
        private double getTotestIndCap_c(SystemVRF sysItem, ProjectBLL bll, out double totestIndCap_h)
        {
            List<RoomIndoor> listRI = bll.GetSelectedIndoorBySystem(sysItem.Id);
            double totestIndCap_c = bll.CalIndoorEstCapacitySum(listRI, out totestIndCap_h);
            return totestIndCap_c;
        }

        private void dgvSystemInfo_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Color color_bg_dgvHeader = UtilColor.bg_dgvHeader_Indoor;
            Pen pen_dgvBorder = new Pen(UtilColor.border_dgvHeader, 0.1f);
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex == -1) //标题行
            {
                SolidBrush brush = new SolidBrush(color_bg_dgvHeader);
                using (brush)
                {
                    e.Graphics.FillRectangle(brush, e.CellBounds);
                    Rectangle border = e.CellBounds;
                    border.X -= 1;
                    //border.Y -= 1;
                    e.Graphics.DrawRectangle(pen_dgvBorder, border);
                }
                e.PaintContent(e.CellBounds);
                e.Handled = true;
                return;
            }
        }
    }
}
