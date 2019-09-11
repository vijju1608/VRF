using JCHVRF.BLL;
using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NextGenModel = JCHVRF.Model.NextGen;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using JCHVRF_New.Utility;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    class OutDoorUnitInfoViewModel : ViewModelBase
    {
        #region Fields
        private JCHVRF.Model.Project _project;
        JCHVRF.BLL.OutdoorBLL bll;
        string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
        string navigateToFolder = "..\\..\\Image\\TypeImageProjectCreation";
        #endregion Fields

        #region View_Model_Property

        private SeriesModel _SelProductSeries;
        public SeriesModel SelProductSeries
        {
            get { return _SelProductSeries; }
            set { this.SetValue(ref _SelProductSeries, value); }
        }

        private string _ProductType;
        public string ProductType
        {
            get { return _ProductType; }
            set { this.SetValue(ref _ProductType, value); }
        }

        private ObservableCollection<string> listProductCategory;
        public ObservableCollection<string> ListProductCategory
        {
            get { return listProductCategory; }
            set { this.SetValue(ref listProductCategory, value); }
        }

        private string _selectedProductCategory;
        public string SelectedProductCategory
        {
            get { return _selectedProductCategory; }
            set
            {
                this.SetValue(ref _selectedProductCategory, value);
                if (SelectedProductCategory != null)
                {
                    GetSeries(SelectedProductCategory);
                    BindRatio();
                }
            }
        }

        private string _selectedPower;
        public string SelectedPower
        {
            get { return _selectedPower; }
            set { this.SetValue(ref _selectedPower, value); }
        }

        private ObservableCollection<int> listMaxRatio;
        public ObservableCollection<int> ListMaxRatio
        {
            get { return listMaxRatio; }
            set { this.SetValue(ref listMaxRatio, value); }
        }
        private ObservableCollection<string> listType;
        public ObservableCollection<string> ListType
        {
            get { return listType; }
            set { value = listType; }
        }

        private ObservableCollection<SeriesModel> listSeries;
        public ObservableCollection<SeriesModel> ListSeries
        {
            get { return listSeries; }
            set { this.SetValue(ref listSeries, value); }
        }

        private string _selectedSeries;
        public string SelectedSeries
        {
            get { return _selectedSeries; }
            set
            {
                this.SetValue(ref _selectedSeries, value);

                if (SelectedSeries != null)
                {
                    GetType(SelectedSeries);
                    BindOutDoorImageUI(SelectedSeries);
                    BindRatio();
                }
            }
        }

        private ObservableCollection<PowerModel> listPower;
        public ObservableCollection<PowerModel> ListPower
        {
            get { return listPower; }
            set { this.SetValue(ref listPower, value); }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { this.SetValue(ref _selectedIndex, value); }
        }

        private int _selectedMaxRatio;
        public int SelectedMaxRatio
        {
            get { return _selectedMaxRatio; }
            set { this.SetValue(ref _selectedMaxRatio, value); }
        }

        private ObservableCollection<Outdoor> _listOutdoor;
        public ObservableCollection<Outdoor> _ListOutdoor
        {
            get { return _listOutdoor; }
            set { this.SetValue(ref _listOutdoor, value); }
        }

        private string _OduImagePath = string.Empty;
        public string OduImagePath
        {
            get { return _OduImagePath; }
            set { this.SetValue(ref _OduImagePath, value); }
        }
        public DelegateCommand SaveClickCommand { get; set; }
        public DelegateCommand NextClickCommand { get; set; }
        public string ErrorMessage { get; private set; }

        private string _Errormsg;
        public string ODUErrorMessage
        {
            get { return _Errormsg; }
            set
            {
                this.SetValue(ref _Errormsg, value);
                RaisePropertyChanged("IsError");
            }
        }
        public bool IsError
        {
            get { return !string.IsNullOrEmpty(ODUErrorMessage); }
        }

        private IEventAggregator _eventAggregator;
        #endregion  View_Model_Property
        public OutDoorUnitInfoViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<TypeTabSubscriber>().Subscribe(ReLoadOduDetailsOnBrandCodeChange);
            JCHVRF.Model.Project.CurrentProject = JCHVRF.Model.Project.GetProjectInstance;
            bll = new JCHVRF.BLL.OutdoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);

            GetProductCategory();
            // BindRatio();
            SetOutDoorSystemSpecific();
            NextClickCommand = new DelegateCommand(OduTypeNextClick);
            SaveClickCommand = new DelegateCommand(OduSaveClick);
            _eventAggregator.GetEvent<ODUTypeTabNext>().Subscribe(OduTypeNextClick);
            _eventAggregator.GetEvent<ODUTypeTabSave>().Subscribe(OduSaveClick);
        }
        private void ReLoadOduDetailsOnBrandCodeChange(bool IsvalidateTabinfoReturn)
        {
            bll = new JCHVRF.BLL.OutdoorBLL(JCHVRF.Model.Project.CurrentProject.SubRegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);
            GetProductCategory();
            //BindRatio();
            SetOutDoorSystemSpecific();

        }

        private void SetOutDoorSystemSpecific()
        {
            if (!string.IsNullOrEmpty(JCHVRF.Model.Project.CurrentSystemId))
            {
                JCHVRF.Model.NextGen.SystemVRF _currentSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(x => ((SystemBase)x).Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
                if (_currentSystem != null)
                {
                    SelectedProductCategory = _currentSystem.ProductCategory;
                    SelectedSeries = _currentSystem.Series;
                    SelectedMaxRatio = Convert.ToInt32(_currentSystem.MaxRatio * 100);
                    BindOutDoorImageUI(SelectedSeries);
                }
            }
        }

        #region Methods

        private void GetType(string SelectedSeries)
        {
            if (!string.IsNullOrWhiteSpace(SelectedSeries))
            {
                listType = new ObservableCollection<string>();
                DataTable dtSeries = bll.GetOutdoorTypeBySeries(SelectedSeries);
                //foreach (DataRow typeRow in dtSeries.Rows)
                //{
                //    ListType.Add(typeRow.ItemArray[0].ToString());
                //   // GetPower(typeRow.ItemArray[0].ToString());
                //}
                if (dtSeries != null && dtSeries.Rows.Count > 0)
                {
                    ListType.Add(CommonBLL.StringConversion(dtSeries.Rows[0].ItemArray[0]));
                    ProductType = CommonBLL.StringConversion(dtSeries.Rows[0].ItemArray[1]);
                    GetPower(CommonBLL.StringConversion(dtSeries.Rows[0].ItemArray[0]));
                }
            }

        }

        private void GetPower(string selectedType)
        {
            listPower = new ObservableCollection<PowerModel>();
            DataTable dtPower = bll.GetOutdoorPowerListBySeriesAndType(this.SelectedSeries, selectedType);
            foreach (DataRow rowPower in dtPower.Rows)
            {
                listPower.Add(new PowerModel()
                {
                    DisplayName = CommonBLL.StringConversion(rowPower.ItemArray[0]),
                    SelectedValues = CommonBLL.StringConversion(rowPower.ItemArray[1])
                });
            }
            ListPower = listPower;
        }
        private void BindOutDoorImageUI(string series)
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            if (ListSeries.Where(MM => MM.SelectedValues == series).FirstOrDefault() != null)
                OduImagePath = ListSeries.Where(MM => MM.SelectedValues == series).FirstOrDefault().OduImagePath;
            this.OduImagePath = OduImagePath;
        }
        private void OduTypeNextClick()
        {
            if (Project.isSystemPropertyWiz)
            {
                OduNextToClick();
            }
            else
            {
                ODUErrorMessage = null;
                string errorMessage = string.Empty;
                if (OutDoorInfoProjectLegacyValidation(out errorMessage))
                {
                    JCHVRF.Model.NextGen.SystemVRF newSystem = (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.NewSystem;
                    newSystem.MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100);
                    newSystem.Power = ListPower.FirstOrDefault().SelectedValues;

                    if (newSystem.Power != null)
                    {
                        newSystem.SelOutdoorType = listType.FirstOrDefault();
                    }
                    //newSystem.ProductType = ListType.FirstOrDefault();
                    newSystem.ProductCategory = SelectedProductCategory;
                    newSystem.ProductSeries = SelProductSeries.DisplayName;
                    newSystem.Series = SelectedSeries;
                    newSystem.ProductType = ProductType;
                    newSystem.OutdoorItem = new Outdoor
                    {
                        ProductType = ProductType,
                        Series = SelectedSeries,
                        MaxRatio = SelectedMaxRatio,
                        TypeImage = OduImagePath
                    };
                    if (!String.IsNullOrEmpty(OduImagePath))
                        newSystem.DisplayImageName = OduImagePath;

                    _eventAggregator.GetEvent<OduTabSubscriber>().Publish(true);
                }
                else
                {
                    ODUErrorMessage = errorMessage;
                    // JCHMessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _eventAggregator.GetEvent<OduTabSubscriber>().Publish(false);
                }

            }
        }

        private void OduSaveClick()
        {
            if (!string.IsNullOrEmpty(JCHVRF.Model.Project.CurrentSystemId))
            {
                JCHVRF.Model.NextGen.SystemVRF _currentSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(x => ((SystemBase)x).Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
                //_currentSystem.MyPipingNodeOut = null;

                var proj = JCHVRF.Model.Project.CurrentProject;

                JCHVRF.Model.NextGen.SystemVRF selectedSys = _currentSystem.DeepCopy();
                if (proj.RoomIndoorList != null && proj.RoomIndoorList.Count > 0)
                {
                    selectedSys.OutdoorItem = new Outdoor
                    {
                        ProductType = ProductType,
                        Series = SelectedSeries,
                        MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100) ,
                        TypeImage = OduImagePath,
                        Type = ListType.FirstOrDefault(),
                    };

                    selectedSys.ProductCategory = SelectedProductCategory;
                    selectedSys.ProductSeries = SelProductSeries.DisplayName;
                    selectedSys.ProductType = ProductType;
                    selectedSys.MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100);
                    selectedSys.SelOutdoorType = ListType.FirstOrDefault();
                    selectedSys.Power = ListPower.FirstOrDefault().SelectedValues;
                    selectedSys.Series = SelectedSeries;


                    //selectedSys.Name = _currentSystem.Name;
                    //selectedSys.Id = JCHVRF.Model.Project.CurrentSystemId;
                    //ReSelectOutDoor:: Check If Valid System
                    selectedSys.IsInputLengthManually = false;
                    selectedSys.MyPipingNodeOut = null;
                    selectedSys.MyPipingNodeOutTemp = null;
                    selectedSys.IsOutDoorUpdated = false;
                    var IsValid = ReselectOutdoor(selectedSys);
                    if (IsValid == true)
                    {
                        _currentSystem.OutdoorItem = new Outdoor
                        {
                            ProductType = ProductType,
                            Series = SelectedSeries,
                            MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100),
                            TypeImage = OduImagePath,
                            Type = ListType.FirstOrDefault(),
                        };
                        _currentSystem.ProductCategory = SelectedProductCategory;
                        _currentSystem.ProductSeries = SelProductSeries.DisplayName;
                        _currentSystem.ProductType = ProductType;
                        _currentSystem.MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100);
                        _currentSystem.SelOutdoorType = ListType.FirstOrDefault();
                        _currentSystem.Power = ListPower.FirstOrDefault().SelectedValues;
                        _currentSystem.IsOutDoorUpdated = true;
                        _currentSystem.Series = SelectedSeries;
                        //Project.isSystemPropertyWiz = false;
                    }
                    else
                    {
                        _currentSystem.IsOutDoorUpdated =false;
                    }
                }

                else
                {
                    if (_currentSystem != null)
                    {
                        _currentSystem.OutdoorItem = new Outdoor
                        {
                            ProductType = ProductType,
                            Series = SelectedSeries,
                            MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100),
                            TypeImage = OduImagePath,
                            Type = ListType.FirstOrDefault(),
                        };
                        _currentSystem.ProductCategory = SelectedProductCategory;
                        _currentSystem.ProductSeries = SelProductSeries.DisplayName;
                        _currentSystem.ProductType = ProductType;
                        _currentSystem.MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100);
                        _currentSystem.SelOutdoorType = ListType.FirstOrDefault();
                        _currentSystem.Power = ListPower.FirstOrDefault().SelectedValues;
                        _currentSystem.IsOutDoorUpdated = true;
                        _currentSystem.Series = SelectedSeries;

                    }
                }
            }
        }

        private void OduNextToClick()
        {
            if (!string.IsNullOrEmpty(JCHVRF.Model.Project.CurrentSystemId))
            {
                JCHVRF.Model.NextGen.SystemVRF _currentSystem = JCHVRF.Model.Project.CurrentProject.SystemListNextGen.FirstOrDefault(x => ((SystemBase)x).Id.Equals(JCHVRF.Model.Project.CurrentSystemId));
                //_currentSystem.MyPipingNodeOut = null;

                var proj = JCHVRF.Model.Project.CurrentProject;

                JCHVRF.Model.NextGen.SystemVRF selectedSys = _currentSystem.DeepCopy();
                if (proj.RoomIndoorList != null && proj.RoomIndoorList.Count > 0)
                {
                    selectedSys.OutdoorItem = new Outdoor
                    {
                        ProductType = ProductType,
                        Series = SelectedSeries,
                        MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100),
                        TypeImage = OduImagePath,
                        Type = ListType.FirstOrDefault(),
                    };

                    selectedSys.ProductCategory = SelectedProductCategory;
                    selectedSys.ProductSeries = SelProductSeries.DisplayName;
                    selectedSys.ProductType = ProductType;
                    selectedSys.MaxRatio = Convert.ToDouble(Convert.ToDecimal(SelectedMaxRatio) / 100);
                    selectedSys.SelOutdoorType = ListType.FirstOrDefault();
                    selectedSys.Power = ListPower.FirstOrDefault().SelectedValues;
                    selectedSys.Series = SelectedSeries;


                    //selectedSys.Name = _currentSystem.Name;
                    //selectedSys.Id = JCHVRF.Model.Project.CurrentSystemId;
                    //ReSelectOutDoor:: Check If Valid System
                    selectedSys.IsInputLengthManually = false;
                    selectedSys.MyPipingNodeOut = null;
                    selectedSys.MyPipingNodeOutTemp = null;
                    selectedSys.IsOutDoorUpdated = false;
                    var IsValid = ReselectOutdoor(selectedSys);
                    if (IsValid == false)
                    {
                        _currentSystem.IsOutDoorUpdated = false;
                    }                   
                } 
            }
        }

        private bool ReselectOutdoor(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            string ErrMsg = string.Empty;
            var proj = JCHVRF.Model.Project.GetProjectInstance;
            bool IsValidDraw = Utility.Validation.IsValidatedSystemVRF(proj, CurrentSystem, out ErrMsg);
            try
            {
                NextGenModel.SystemVRF CurrVRF = new NextGenModel.SystemVRF();
                AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                if (IsValidDraw)
                {
                    IsValidDraw = Validation.IsIndoorIsValidForODUSeries(CurrentSystem);
                    if (IsValidDraw)
                    {
                        AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSystem, JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                        if (result.SelectionResult == SelectOutdoorResult.OK)
                        {
                            UpdatePipingNodeStructure(CurrentSystem);
                            UpdateWiringNodeStructure(CurrentSystem);
                            IsValidDraw = true;
                        }
                        else
                        {
                            IsValidDraw = false;
                            if (result.ERRList != null && result.ERRList.Count > 0)
                            {
                                _eventAggregator.GetEvent<ErrorLogVM>().Publish(result.ERRList);
                                // JCHMessageBox.Show("The capacity requirements are mismatching.\nPlease add/remove IDUs to make it compatible.\n", MessageType.Error);
                                //----------------- Code below for multi-langauge------------//
                                JCHMessageBox.Show(Language.Current.GetMessage("CAPCITYREQIREMENT"), MessageType.Error);
                            }
                            else if (result.MSGList != null && result.MSGList.Count > 0)
                            {
                                _eventAggregator.GetEvent<ErrorLogVM>().Publish(result.MSGList);
                                //JCHMessageBox.Show("The capacity requirements are mismatching.\nPlease add/remove IDUs to make it compatible.\n", MessageType.Error);
                                //----------------- Code below for multi-langauge------------//
                                JCHMessageBox.Show(Language.Current.GetMessage("CAPCITYREQIREMENT"), MessageType.Error);
                            }
                            else
                                //JCHMessageBox.Show("The capacity requirements are mismatching.\nPlease add/remove IDUs to make it compatible.\n", MessageType.Error);
                                //----------------- Code below for multi-langauge------------//
                                JCHMessageBox.Show(Language.Current.GetMessage("CAPCITYREQIREMENT"), MessageType.Error);
                        }
                    }else
                        JCHMessageBox.Show(Language.Current.GetMessage("CAPCITYREQIREMENT"), MessageType.Error);
                }
                else
                {
                    IsValidDraw = false;
                    JCHMessageBox.Show(string.Format(ErrMsg));
                }
            }
            catch (Exception ex)
            {
                IsValidDraw = false;
            }
            return IsValidDraw;

        }
        private void UpdatePipingNodeStructure(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            try
            {
                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                pipBll.CreatePipingNodeStructure(CurrentSystem);
            }
            catch (Exception ex)
            {
                //JCHMessageBox.Show("Error Occured : " + ex.Message);
                //----------------- Code below for multi-langauge------------//
                JCHMessageBox.Show(Language.Current.GetMessage("ERROR_OCCURED"), MessageType.Error);
            }
        }
        public void UpdateWiringNodeStructure(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            try
            {
                string imageDir = @"/Image/TypeImages/";
                JCHVRF_New.Utility.WiringBLL bll = new JCHVRF_New.Utility.WiringBLL(JCHVRF.Model.Project.GetProjectInstance, imageDir);
                bll.CreateWiringNodeStructure(CurrentSystem);
            }
            catch (Exception ex)
            {
                //JCHMessageBox.Show("Error Occured : " + ex.Message);
                //----------------- Code below for multi-langauge------------//
                JCHMessageBox.Show(Language.Current.GetMessage("ERROR_OCCURED"), MessageType.Error);
            }
        }
        private NextGenBLL.PipingBLL GetPipingBLLInstance()
        {
            //this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            bool isInch = CommonBLL.IsDimension_inch();
            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            return new NextGenBLL.PipingBLL(JCHVRF.Model.Project.GetProjectInstance, utilPiping, null, isInch, ut_weight, ut_length, ut_power);
        }

        bool OutDoorInfoProjectLegacyValidation(out string ODUErrorMessage)
        {
            bool isValid = true;
            ODUErrorMessage = null;

            if (string.IsNullOrEmpty(SelectedProductCategory))
            {
                //ODUErrorMessage = "Product Type cannot be blank";
                //----------------- Code below for multi-langauge------------//
                ODUErrorMessage = Language.Current.GetMessage("PRODUCT_TYPE_BLANK");
                isValid = false;
            }
            else if (string.IsNullOrEmpty(SelectedSeries))
            {
                //ODUErrorMessage = "Series Name cannot be blank";
                //----------------- Code below for multi-langauge------------//
                ODUErrorMessage = Language.Current.GetMessage("ALERT_SERIES_NAME");
                isValid = false;
            }

            else if (SelectedMaxRatio == 0)
            {
                //ODUErrorMessage = "Max Ratio cannot be blank";
                //----------------- Code below for multi-langauge------------//
                ODUErrorMessage = Language.Current.GetMessage("MAX_RATIO_BLANK");
                isValid = false;
            }
            return isValid;
        }

        #endregion Methods

        #region Binding Data
        private void GetProductCategory()
        {
            ListProductCategory = new ObservableCollection<string>();
            DataTable dtProductCategory = bll.GetOutdoorListStd();
            foreach (DataRow productcategoryeRow in dtProductCategory.Rows)
            {
                if (!string.IsNullOrEmpty(CommonBLL.StringConversion(productcategoryeRow["ProductCategory"])))
                {
                    if (!this.ListProductCategory.Contains(CommonBLL.StringConversion(productcategoryeRow["ProductCategory"])))
                    {
                        this.ListProductCategory.Add(CommonBLL.StringConversion(productcategoryeRow["ProductCategory"]));
                    }
                }
            }
        }
        private void GetSeries(string selectedProductCategory)
        {
            var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            ListSeries = new ObservableCollection<SeriesModel>();

            DataTable dtSeries = bll.GetOutdoorListStd(selectedProductCategory);
            foreach (DataRow dtSeriesTypeRow in dtSeries.Rows)
            {
                if (ListSeries.Count == 0)
                {
                    ListSeries.Add(new SeriesModel()
                    {
                        DisplayName = CommonBLL.StringConversion(dtSeriesTypeRow.ItemArray[0]),
                        SelectedValues = CommonBLL.StringConversion(dtSeriesTypeRow.ItemArray[1]),
                        OduImagePath = sourceDir + "\\" + CommonBLL.StringConversion(dtSeriesTypeRow["TypeImage"])
                    });
                }
                else
                {
                    if (ListSeries.Any(MM => MM.DisplayName == CommonBLL.StringConversion(dtSeriesTypeRow.ItemArray[0])))
                    {
                        continue;
                    }
                    else
                    {
                        ListSeries.Add(new SeriesModel()
                        {
                            DisplayName = CommonBLL.StringConversion(dtSeriesTypeRow.ItemArray[0]),
                            SelectedValues = CommonBLL.StringConversion(dtSeriesTypeRow.ItemArray[1]),
                            OduImagePath = sourceDir + "\\" + CommonBLL.StringConversion(dtSeriesTypeRow["TypeImage"])
                        });
                    }
                }
            }
        }

        private void BindRatio()
        {
            ListMaxRatio = new ObservableCollection<int>();
            double max = 1.3;
            double min = 0.5;
            double defaultMaxRatio = 1;
            defaultMaxRatio = 0;
            ListMaxRatio.Clear();
            SelectedMaxRatio = 0;
            if (SelectedSeries != null)
            {
                if (SelectedSeries.Contains("FSNP") || SelectedSeries.Contains("FSXNP"))
                {
                    max = 1.5;
                }
                else if (JCHVRF.Model.Project.CurrentProject.SubRegionCode == "TW" && SelectedSeries.StartsWith("IVX,"))
                {
                    min = 1;
                }

                for (int i = (int)(max * 100); i >= min * 100; i -= 10)
                {
                    ListMaxRatio.Add(i);
                }
                this.SelectedIndex = (int)(Math.Round(max - defaultMaxRatio, 2) * 10);
            }
        }
        #endregion Binding Data
    }
}


