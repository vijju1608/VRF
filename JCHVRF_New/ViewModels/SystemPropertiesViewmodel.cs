using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextGenModel = JCHVRF.Model.NextGen;

namespace JCHVRF_New.ViewModels
{
    public class SystemPropertiesViewmodel : INotifyPropertyChanged
    {

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public string CoolingIDURatedCapacity { get; set; }

        public string CoolingODURatedCapacity { get; set; }

        public string CoolingSystemActualCapacity { get; set; }

        public string HeatingIDURatedCapacity { get; set; }

        public string HeatingODURatedCapacity { get; set; }

        public string HeatingingSystemActualCapacity { get; set; }

        public string MaxNumberOfIndoor { get; set; }

        public string ActualRatio { get; set; }

        public string MaxRatio { get; set; }

        public NextGenModel.SystemVRF CurrentSystem { get; set; }

        public JCHVRF.Model.Project ProjectLegacy { get; set; }

        public string PowerUnit { get; set; }
        #endregion

        #region variables
        double tot_indcap_c;
        double tot_indcap_h;
        double tot_act_indcap_c;
        double tot_act_indcap_h;
        double tot_FAcap;

        double tot_indcap_c_rat;
        double tot_indcap_h_rat;
        #endregion

        public SystemPropertiesViewmodel(JCHVRF.Model.Project projectLegacy)
        {
            try
            { 
            this.ProjectLegacy = projectLegacy;
            this.CurrentSystem = this.ProjectLegacy.SystemListNextGen.FirstOrDefault();

            GetSystemPropertiesValues();
            BindUnit();
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        private void BindUnit()
        {
            PowerUnit = SystemSetting.UserSetting.unitsSetting.settingPOWER;
        }

        void GetSystemPropertiesValues()
        {
            CalcualteIndoorAndOutdoorValue();
            this.ActualRatio = (this.CurrentSystem.Ratio * 100).ToString("n0") + "%";
            this.CoolingSystemActualCapacity = this.CurrentSystem.CoolingCapacity.ToString("n2");
            this.HeatingingSystemActualCapacity = this.CurrentSystem.HeatingCapacity.ToString("n2");



            if (this.CurrentSystem != null && this.CurrentSystem.OutdoorItem != null)
            {
                this.CoolingODURatedCapacity = this.CurrentSystem.OutdoorItem.CoolingCapacity.ToString("n2");
                this.HeatingODURatedCapacity = this.CurrentSystem.OutdoorItem.HeatingCapacity.ToString("n2");
                this.MaxNumberOfIndoor = CurrentSystem.OutdoorItem.MaxIU.ToString();
            }
            else
            {
                this.CoolingODURatedCapacity = "0.00";
                this.HeatingODURatedCapacity = "0.00";
            }

            
            this.CoolingIDURatedCapacity = tot_indcap_c_rat.ToString("n2");
            this.HeatingIDURatedCapacity = tot_indcap_h_rat.ToString("n2");

            if (!ProjectLegacy.IsCoolingModeEffective)
                this.CoolingIDURatedCapacity = "0.00";
                        if (!ProjectLegacy.IsHeatingModeEffective)
               // && !_productType.Contains(", CO")) Todo later
                this.HeatingIDURatedCapacity = "0.00";


            this.MaxRatio = this.CurrentSystem.MaxRatio * 100 + "%";


        }

        void CalcualteIndoorAndOutdoorValue()
        {
            tot_indcap_c = 0;
            tot_indcap_h = 0;
            tot_act_indcap_c = 0;
            tot_act_indcap_h = 0;
            tot_FAcap = 0;

            tot_indcap_c_rat = 0;
            tot_indcap_h_rat = 0;

            foreach (RoomIndoor ri in this.ProjectLegacy.RoomIndoorList)
            {
                tot_indcap_c += ri.CoolingCapacity;
                tot_indcap_h += ri.HeatingCapacity;
                tot_act_indcap_c += ri.ActualCoolingCapacity;
                tot_act_indcap_h += ri.ActualHeatingCapacity;
                tot_indcap_c_rat += ri.IndoorItem.CoolingCapacity;
                tot_indcap_h_rat += ri.IndoorItem.HeatingCapacity;
                if (ri.IndoorItem.Flag == IndoorType.FreshAir)
                {
                    tot_FAcap += ri.IndoorItem.CoolingCapacity;
                }
            }
        }
    }
}
