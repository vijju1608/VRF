using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF_New.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextGenModel = JCHVRF.Model.NextGen;

namespace JCHVRF_New.ViewModels
{
    public class ODUEquipmentPropertiesViewModel : INotifyPropertyChanged
    {
        JCHVRF.BLL.NextGen.OutdoorBLL bll;
        private string _oduName = string.Empty;
        NextGenModel.SystemVRF curSystemItem;
        public JCHVRF.Model.Project newProjectLegacy { get; set; }

        public string OduName
        {
            get { return _oduName; }
            set { _oduName = value; OnPropertyChanged("OduName"); }
        }

        public NextGenModel.SystemVRF CurrentSystemItem
        {
            get { return curSystemItem; }
            set { curSystemItem = value; }
        }
        private ObservableCollection<string> listType;
        public ObservableCollection<string> ListType
        {
            get { return listType; }
            set
            {

                value = listType;

            }
        }

        private ObservableCollection<PowerModel> listPower;

        public ObservableCollection<PowerModel> ListPower
        {
            get { return listPower; }
            set
            {

                value = listPower;
                OnPropertyChanged("ListPower");

            }
        }

        private ObservableCollection<string> listOutDoor;
        public ObservableCollection<string> ListOutdoor
        {
            get { return listOutDoor; }


            set
            {

                value = listOutDoor;
                OnPropertyChanged("ListOutdoor");

            }
        }
        private ObservableCollection<int> listMaxRatio;
        public ObservableCollection<int> ListMaxRatio
        {
            get { return listMaxRatio; }
            set { listMaxRatio = value; }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged("SelectedIndex");

            }
        }

        private int _selectedMaxRatio;
        public int SelectedMaxRatio
        {
            get
            {
                return _selectedMaxRatio;
            }
            set
            {
                _selectedMaxRatio = value;
                OnPropertyChanged("SelectedMaxRatio");

            }
        }

        private List<ComboBox> listModel;
        public List<ComboBox> ListModel
        {
            get
            {
                return listModel;
            }
            set
            {
                listModel = value;
                OnPropertyChanged("ListModel");
            }
        }

        private string _selectedOutdoor;
        public string SelectedOutdoor
        {
            get
            {
                return _selectedOutdoor;
            }
            set
            {
                _selectedOutdoor = value;
                OnPropertyChanged("SelectedOutdoor");

            }
        }

        private string _selectedSeries;
        public string SelectedSeries
        {
            get
            {
                return _selectedSeries;
            }
            set
            {
                _selectedSeries = value;
                OnPropertyChanged("SelectedSeries");

            }
        }

        private string _selectedType;
        public string SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                _selectedType = value;
                OnPropertyChanged("SelectedType");
                GetPower(_selectedType);


            }
        }

        private string _selectedPower;
        public string SelectedPower
        {
            get
            {
                return _selectedPower;
            }
            set
            {
                _selectedPower = value;
                OnPropertyChanged("SelectedPower");
                GetOutDoor(_selectedType);

            }
        }

        public ODUEquipmentPropertiesViewModel(JCHVRF.Model.Project projectLegacy)
        {
            try
            {
                this.newProjectLegacy = projectLegacy;
                if (newProjectLegacy.SystemListNextGen[0] != null)
                {
                    this.IsAuto = newProjectLegacy.SystemListNextGen[0].IsAuto;
                    this.OduName = newProjectLegacy.SystemListNextGen[0].Name;
                    // curSystemItem = new SystemVRF(); 
                    curSystemItem = projectLegacy.SystemListNextGen[0]; // Right Now only 1 SystemVRF in projectLegacy obj
                                                                        // bll = new JCHVRF.BLL.NextGen.OutdoorBLL("GBR", "H", "EU_W");
                    bll = new JCHVRF.BLL.NextGen.OutdoorBLL(projectLegacy.SubRegionCode, projectLegacy.BrandCode, projectLegacy.RegionCode);

                    if (newProjectLegacy.SystemListNextGen[0].Series != null)
                    {
                        this.SelectedSeries = newProjectLegacy.SystemListNextGen[0].Series;
                        GetType(newProjectLegacy.SystemListNextGen[0].Series);
                    }

                    if (newProjectLegacy.SystemListNextGen[0].SelOutdoorType != null)
                        this.SelectedType = newProjectLegacy.SystemListNextGen[0].SelOutdoorType;
                    if (newProjectLegacy.SystemListNextGen[0].Power != null)
                        this.SelectedPower = newProjectLegacy.SystemListNextGen[0].Power;
                    if (newProjectLegacy.SystemListNextGen[0].OutdoorItem != null && newProjectLegacy.SystemListNextGen[0].OutdoorItem.ModelFull != null)
                        this.SelectedOutdoor = projectLegacy.SystemListNextGen[0].OutdoorItem.ModelFull;
                    // GetPower("Commercial VRF HP, FSXNSE", "FSXNSE (Top discharge)");
                    // GetOutDoor();
                    //if (projectLegacy.SystemList[0].Series != null)

                    BindMaxRatio();

                    if (projectLegacy.SystemListNextGen[0] != null)
                    {
                        this.SelectedMaxRatio = Convert.ToInt32(projectLegacy.SystemListNextGen[0].MaxRatio * 100);
                    }

                    if (projectLegacy.SystemListNextGen[0].SysType == SystemType.CompositeMode)
                        this.IsBothIndoreFreshAir = true;
                    else this.IsBothIndoreFreshAir = false;

                    //if (projectLegacy.SystemListNextGen[0].MaxRatio > 0)
                    //    this.SelectedMaxRatio = Convert.ToInt32(projectLegacy.SystemListNextGen[0].MaxRatio);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        public ODUEquipmentPropertiesViewModel(JCHVRF.Model.Project projectLegacy, NextGenModel.SystemVRF ODUItem)
        {
            try
            {
                this.newProjectLegacy = projectLegacy;
                if (ODUItem != null)
                {
                    this.IsAuto = ODUItem.IsAuto;
                    this.OduName = ODUItem.Name;
                    //  curSystemItem = new SystemVRF(); 
                    curSystemItem = ODUItem; // Right Now only 1 SystemVRF in projectLegacy obj
                                             // bll = new JCHVRF.BLL.NextGen.OutdoorBLL("GBR", "H", "EU_W");
                    bll = new JCHVRF.BLL.NextGen.OutdoorBLL(projectLegacy.SubRegionCode, projectLegacy.BrandCode, projectLegacy.RegionCode);

                    if (ODUItem.Series != null)
                    {
                        this.SelectedSeries = ODUItem.Series;
                        GetType(ODUItem.Series);
                    }

                    if (ODUItem.SelOutdoorType != null)
                        this.SelectedType = ODUItem.SelOutdoorType;
                    if (ODUItem.Power != null)
                        this.SelectedPower = ODUItem.Power;
                    if (ODUItem.OutdoorItem != null && ODUItem.OutdoorItem.ModelFull != null)
                        this.SelectedOutdoor = ODUItem.OutdoorItem.ModelFull;
                    // GetPower("Commercial VRF HP, FSXNSE", "FSXNSE (Top discharge)");
                    // GetOutDoor();
                    //if (projectLegacy.SystemList[0].Series != null)

                    BindMaxRatio();

                    if (ODUItem != null)
                    {
                        this.SelectedMaxRatio = Convert.ToInt32(ODUItem.MaxRatio * 100);
                    }

                    if (ODUItem.SysType == SystemType.CompositeMode)
                        this.IsBothIndoreFreshAir = true;
                    else this.IsBothIndoreFreshAir = false;

                    //if (projectLegacy.SystemListNextGen[0].MaxRatio > 0)
                    //    this.SelectedMaxRatio = Convert.ToInt32(projectLegacy.SystemListNextGen[0].MaxRatio);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }
        private bool isAuto;
        public bool IsAuto
        {
            get { return isAuto; }
            set
            {
                isAuto = value;
                OnPropertyChanged("IsAuto");
            }
        }

        private bool isManual;
        public bool IsManual
        {
            get { return isManual; }
            set { isManual = value; }
        }

        private bool isBothIndoreFreshAir;
        public bool IsBothIndoreFreshAir
        {
            get { return isBothIndoreFreshAir; }
            set
            {
                isBothIndoreFreshAir = value;
                OnPropertyChanged("IsBothIndoreFreshAir");
            }
        }

        private void GetType(string series)
        {
            string colName = "";
            listType = new ObservableCollection<string>();
            DataTable dtSeries = bll.GetOutdoorTypeListBySeries(out colName, series);
            foreach (DataRow typeRow in dtSeries.Rows)
            {
                listType.Add(typeRow.ItemArray[0].ToString());
            }

            this.ListType = listType;
        }

        private void GetPower(string selectedType)
        {
            listPower = new ObservableCollection<PowerModel>();
            DataTable dtPower = bll.GetOutdoorPowerListBySeriesAndType(this.SelectedSeries, selectedType);
            foreach (DataRow rowPower in dtPower.Rows)
            {
                listPower.Add(new PowerModel()
                {
                    DisplayName = rowPower.ItemArray[0].ToString(),
                    SelectedValues = rowPower.ItemArray[1].ToString()
                });
            }

            this.ListPower = listPower;
        }

        private void GetOutDoor(string SelectedType)
        {
            if (!String.IsNullOrEmpty(SelectedType))
            {
                listModel = new List<ComboBox>();
                listOutDoor = new ObservableCollection<string>();
                DataTable dtOutdoor = bll.GetOutdoorListStd();
                DataRow[] rows;
                string powerValue = "";
                if (SelectedPower != null)
                    powerValue = this.SelectedPower;
                if ("EU_E".Equals(newProjectLegacy.SubRegionCode) && "H".Equals(newProjectLegacy.BrandCode))
                    rows = dtOutdoor.Select("SubString(ModelFull,11,1)='" + powerValue + "' and UnitType='" + SelectedType.Trim() + "' and Series='" + SelectedSeries.Trim() + "'" + " and TypeImage <> '' and ModelFull <> 'JVOL060VVEM0AQ'", "Model asc");
                else
                    rows = dtOutdoor.Select("SubString(ModelFull,11,1)='" + powerValue + "' and UnitType='" + SelectedType.Trim() + "' and Series='" + SelectedSeries.Trim() + "'" + " and TypeImage <> ''", "Model asc");
                if (newProjectLegacy != null && newProjectLegacy.BrandCode == "H")
                {
                    DataTable dt = rows.CopyToDataTable();
                    List<string> outDoorList = dt.AsEnumerable()
                        .Select(r => r.Field<string>("Model_Hitachi"))
                        .ToList();
                    listOutDoor = new ObservableCollection<string>(outDoorList);

                    foreach(DataRow row1 in dt.Rows)
                    {
                        listModel.Add(new ComboBox
                        {
                            DisplayName = Convert.ToString(row1["Model_Hitachi"]),
                            Value = Convert.ToString(row1["ModelFull"])
                        });
                    }
                }
                else if(newProjectLegacy != null && newProjectLegacy.BrandCode == "S")
                {
                    DataTable dt = rows.CopyToDataTable();
                    List<string> outDoorList = dt.AsEnumerable()
                        .Select(r => r.Field<string>("Model_York"))
                        .ToList();
                    listOutDoor = new ObservableCollection<string>(outDoorList);
                    foreach (DataRow row1 in dt.Rows)
                    {
                        listModel.Add(new ComboBox
                        {
                            DisplayName = Convert.ToString(row1["Model_Hitachi"]),
                            Value = Convert.ToString(row1["ModelFull"])
                        });
                    }
                }
                this.ListOutdoor = listOutDoor;
                this.ListModel = listModel;
            }

        }


        private void BindMaxRatio()
        {
            listMaxRatio = new ObservableCollection<int>();
            double max = 1.3;
            double min = 0.5;
            double defaultMaxRatio = 1;
            if (SelectedSeries.Contains("FSNP") || SelectedSeries.Contains("FSXNP"))
            {
                max = 1.5;
            }
            else if (newProjectLegacy.SubRegionCode == "TW" && SelectedSeries.StartsWith("IVX,"))
            {
                min = 1;
            }
            if (curSystemItem.OutdoorItem != null)
            {
                if (SelectedSeries.Contains("FSXNPE") && curSystemItem.OutdoorItem.CoolingCapacity > 150)
                {
                    max = 1.3;
                }
                else if (SelectedSeries.Contains("FSXNPE") && curSystemItem.OutdoorItem.CoolingCapacity <= 150)
                {
                    max = 1.5;
                }
            }
            if (curSystemItem.SysType == SystemType.OnlyFreshAir)
                return;

            if (curSystemItem.MaxRatio > max)
            {
                curSystemItem.MaxRatio = max;
            }
            defaultMaxRatio = curSystemItem.MaxRatio;
            for (int i = (int)(max * 100); i >= min * 100; i -= 10)
            {
                listMaxRatio.Add(i);
            }
            this.ListMaxRatio = listMaxRatio;
            this.SelectedIndex = (int)(Math.Round(max - defaultMaxRatio, 2) * 10);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
