using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using System.Windows;
using JCHVRF.BLL;
using JCBase.Utility;
using JCHVRF.VRFMessage;
using NextGenModel = JCHVRF.Model.NextGen;
using System.Linq;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using WL = Lassalle.WPF.Flow;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Language = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{

    public class PipingLengthSettingsViewModel : ViewModelBase
    {
        #region Fields
        IndoorBLL bll;

        string ut_length;
        JCHVRF.Model.NextGen.SystemVRF sysItemSource = null;
        public JCHVRF.Model.NextGen.MyNode IndoorTags { get; set; }
        SystemVRF curSystemItem;
        private JCHVRF.Model.NextGen.SystemVRF CurrentSystems;
        Project thisProject;
        List<string> ERRList;
        List<string> MSGList;
        SelectOutdoorResult result;
        double originFirstPipeLength = 0;
        double originPipeEquivalentLength = 0;
        PipingPositionType originPipingPositionType = PipingPositionType.Upper;
        double originHeightDiff = 0;
        private JCHVRF.Model.Project _project;
        public DelegateCommand Settings { get; set; }
        public DelegateCommand SettingsOk { get; set; }
        public DelegateCommand SettingsCancel { get; set; }
        private string _lengthUnit;
        public string LengthUnit
        {
            get { return _lengthUnit; }
            set { this.SetValue(ref _lengthUnit, value); }
        }
        private string _Eq_lengthError;
        public string Eq_lengthError
        {
            get { return _Eq_lengthError; }
            set { this.SetValue(ref _Eq_lengthError, value); }
        }
        private string _HD_lengthError;
        public string HD_lengthError
        {
            get { return _HD_lengthError; }
            set { this.SetValue(ref _HD_lengthError, value); }
        }
        private decimal _jctxtEqPipeLength;
        private decimal _jctxtFirstPipeLength;
        private double _txtIndoorDifference;
        private string _cmbPositionType;
        private string _txtIndoorHighDifference;
        private bool _isPipingPopupOpened;
        private string _TxtMaxIndoorHeightDifferences;
        private List<ComboBox> _cmbPosition;
        private string _TxtMaxOutdoorHeightDifferences;
        private Visibility _popupSettings;
        private int _selectedIndex;
        private int selectedRow;
        private Visibility _validatEquivPipeLength = Visibility.Collapsed;
        private Visibility _validatFirstBranchIDU = Visibility.Collapsed;
        bool isVal = true;
        PipingInfoModel PipingObjectMaxLengthFirstPipeLength = new PipingInfoModel();
        #endregion
        #region properties
        public decimal jctxtEqPipeLength
        {
            get
            {
                if (_jctxtEqPipeLength > Convert.ToDecimal(Unit.ConvertToControl(sysItemSource.MaxEqPipeLength, UnitType.LENGTH_M, ut_length).ToString("n1")))
                {
                    validatEquivPipeLength = Visibility.Visible;
                    _jctxtEqPipeLength = 0;
                }
                else
                    validatEquivPipeLength = Visibility.Collapsed;
                sysItemSource.PipeEquivalentLength = Convert.ToDouble(_jctxtEqPipeLength);

                return _jctxtEqPipeLength;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(Convert.ToString(value)) == true)
                    _jctxtEqPipeLength = 0;
                else
                {
                    this.SetValue(ref _jctxtEqPipeLength, value);

                }
            }

        }
        public decimal jctxtFirstPipeLength
        {
            get
            {
                if (_jctxtFirstPipeLength > Convert.ToDecimal(Unit.ConvertToControl(sysItemSource.MaxDiffIndoorLength, UnitType.LENGTH_M, ut_length).ToString("n1")))
                {
                    validatFirstBranchIDU = Visibility.Visible;
                    _jctxtFirstPipeLength = 0;
                }
                else
                    validatFirstBranchIDU = Visibility.Collapsed;
                sysItemSource.FirstPipeLength = Convert.ToDouble(_jctxtFirstPipeLength);
                return _jctxtFirstPipeLength;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(Convert.ToString(value)) == true)
                    _jctxtFirstPipeLength = 0;
                else
                    this.SetValue(ref _jctxtFirstPipeLength, value);
            }

        }


        public string TxtIndoorHighDifference
        {
            get { return _txtIndoorHighDifference; }
            set
            {
                if (string.IsNullOrWhiteSpace(Convert.ToString(value)) == true)
                    _txtIndoorHighDifference = "0";
                this.SetValue(ref _txtIndoorHighDifference, value);
            }

        }
        public string cmbPositionType
        {
            get { return _cmbPositionType; }
            set { this.SetValue(ref _cmbPositionType, value); }

        }
        private List<IDUList> listselectedIDU;

        public JCHVRF.Model.Project Project
        {
            get { return _project; }
            set { this.SetValue(ref _project, value); }
        }

        public string TxtMaxIndoorHeightDifferences
        {
            get { return _TxtMaxIndoorHeightDifferences; }
            set { this.SetValue(ref _TxtMaxIndoorHeightDifferences, value); }
        }

        public string TxtMaxOutdoorHeightDifferences
        {
            get { return _TxtMaxOutdoorHeightDifferences; }
            set { this.SetValue(ref _TxtMaxOutdoorHeightDifferences, value); }
        }


        public bool IsPipingPopupOpened
        {
            get { return this._isPipingPopupOpened; }
            set { this.SetValue(ref _isPipingPopupOpened, value); }
        }


        public Visibility validatEquivPipeLength
        {
            get { return this._validatEquivPipeLength; }
            set
            {
                this.SetValue(ref _validatEquivPipeLength, value);
            }
        }



        public Visibility validatFirstBranchIDU
        {
            get { return this._validatFirstBranchIDU; }
            set
            {
                this.SetValue(ref _validatFirstBranchIDU, value);
            }
        }



        public List<ComboBox> cmbPosition
        {
            get
            {

                if (_cmbPosition == null)
                    _cmbPosition = new List<ComboBox>();
                _cmbPosition.Add(new ComboBox { DisplayName = Convert.ToString("Upper"), Value = Convert.ToString("Upper") });
                _cmbPosition.Add(new ComboBox { DisplayName = Convert.ToString("SameLevel"), Value = Convert.ToString("SameLevel") });
                _cmbPosition.Add(new ComboBox { DisplayName = Convert.ToString("Lower"), Value = Convert.ToString("Lower") });
                return _cmbPosition;
            }
            set
            {
                this.SetValue(ref _cmbPosition, value);
            }
        }


        public Visibility PopupSettings
        {
            get { return _popupSettings; }
            set
            {
                SetValue(ref _popupSettings, value);
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                this.SetValue(ref _selectedIndex, value);
                if (_selectedIndex == 1)
                {
                    this.IsEditable = true;
                    TxtIndoorHighDifference = "0.0";
                }

                else
                    this.IsEditable = false;
                TxtIndoorHighDifference = _txtIndoorHighDifference;
            }
        }



        public int SelectedRow
        {
            get
            {
                return selectedRow;
            }
            set
            {
                selectedRow = value;
                OnPropertyChanged("SelectedRow");
            }
        }
        public delegate void PropertyChangedEventHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e);
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value) return;
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }
        private string _lblPipingLegthError;
        public string lblPipingLegthError
        {
            get { return _lblPipingLegthError; }
            set
            {
                SetValue(ref _lblPipingLegthError, value);
            }

        }

        private IEventAggregator _eventAggregator;
        List<RoomIndoor> listRISelected = new List<RoomIndoor>();
        List<SelectedIDUList> SelIDUList = new List<SelectedIDUList>();

        public ObservableCollection<IDUList> ListIDU
        {
            get
            {
                if (_listIDU == null)
                {
                    List<IDUList> _listIDU = new List<IDUList>();

                }
                return _listIDU;
            }
            set
            { this.SetValue(ref _listIDU, value); }
        }
        private bool _isEditable;
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                SetValue(ref _isEditable, value);
            }
        }

        private bool _isEditableEqPipeLength;
        public bool IsEditableEqPipeLength
        {
            get { return _isEditableEqPipeLength; }
            set
            {
                SetValue(ref _isEditableEqPipeLength, value);
            }
        }
        private bool _isEditableFirstPipeLength;
        public bool IsEditableFirstPipeLength
        {
            get { return _isEditableFirstPipeLength; }
            set
            {
                SetValue(ref _isEditableFirstPipeLength, value);
            }
        }

        public DelegateCommand<IList<Object>> PipeLengthGridSelectionChanged { get; private set; }


        #endregion
        public class SelectedIDUList
        {
            public int IndoorNo { get; set; }
            public string Name { get; set; }

            public string IndoorName { get; set; }
            public string IndoorModel { get; set; }
            public JCHVRF.Model.NextGen.MyNode IndoorTag { get; set; }
        }
        public class IDUList
        {
            public int IndoorNo { get; set; }
            public string Name { get; set; }

            public string IndoorName { get; set; }
            public double HeightDiff { get; set; }
            public string PositionType { get; set; }
            public JCHVRF.Model.NextGen.MyNode IndoorTag { get; set; }
        }



        public PipingLengthSettingsViewModel(JCHVRF.Model.Project Project, IEventAggregator eventAggregator, IProjectInfoBAL projectInfoBll)
        {
            this.Project = JCHVRF.Model.Project.GetProjectInstance;
            thisProject = JCHVRF.Model.Project.GetProjectInstance;
            var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
            var Lists = ProjcurrentSystem.FirstOrDefault(); //.Where(idu => idu.Id == Syste.Id)
            PipeLengthGridSelectionChanged = new DelegateCommand<IList<Object>>(OnSelectionChanged);
            CurrentSystems = Lists;
            LengthUnit = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            //NextGenModel.SystemVRF curSystemItem = (NextGenModel.SystemVRF)CurrentSystems;
            if (WorkFlowContext.CurrentSystem != null)
            {
                if (WorkFlowContext.CurrentSystem is NextGenModel.SystemVRF)
                {
                    NextGenModel.SystemVRF curSystemItem = (NextGenModel.SystemVRF)WorkFlowContext.CurrentSystem;
                    this._eventAggregator = eventAggregator;
                    _eventAggregator = eventAggregator;
                    ListIDU = new ObservableCollection<IDUList>();
                    //SelectedRow = new ObservableCollection<IDUList>();
                    ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                    NextGenBLL.PipingBLL pipBll = new NextGenBLL.PipingBLL(this.Project);

                    if (curSystemItem != null)
                    {

                        sysItemSource = curSystemItem;
                        initPipLength();
                        //CreateDataGridViewColumns();
                        //BindPositionType();
                        BindHighDifference();
                    }
                    PopupSettings = Visibility.Hidden;
                    IsPipingPopupOpened = false;
                    Settings = new DelegateCommand(OnSettingsClicked);
                    SettingsOk = new DelegateCommand(OnSettingsOk);
                    SettingsCancel = new DelegateCommand(OnSettingsCancel);
                }
            }
            Eq_lengthError = "[0-" + Unit.ConvertToControl(sysItemSource.MaxEqPipeLength, UnitType.LENGTH_M, ut_length).ToString("n1") + "]";
            HD_lengthError = "[0-" + Unit.ConvertToControl(sysItemSource.MaxDiffIndoorLength, UnitType.LENGTH_M, ut_length).ToString("n1") + "]";
        }

        private void OnSelectionChanged(IList<Object> items)
        {

            listselectedIDU = items.Cast<IDUList>().ToList();
        }

        private void initPipLength()
        {

            this.originFirstPipeLength = sysItemSource.FirstPipeLength;
            this.originPipeEquivalentLength = sysItemSource.PipeEquivalentLength;
            this.originPipingPositionType = sysItemSource.PipingPositionType;
            this.originHeightDiff = sysItemSource.HeightDiff;

            jctxtEqPipeLength = Convert.ToDecimal(Unit.ConvertToControl(sysItemSource.PipeEquivalentLength, UnitType.LENGTH_M, ut_length).ToString("n1"));
            jctxtFirstPipeLength = Convert.ToDecimal(Unit.ConvertToControl(sysItemSource.FirstPipeLength, UnitType.LENGTH_M, ut_length).ToString("n1"));
            if (sysItemSource.IsInputLengthManually)
            {
                IsEditableEqPipeLength = true;
                IsEditableFirstPipeLength = true;
            }
        }
        private ObservableCollection<IDUList> _listIDU;

        private void BindHighDifference()
        {
            this.ListIDU = new ObservableCollection<IDUList>();
            this.ListIDU.Clear();
            List<JCHVRF.Model.NextGen.MyNode> nodes = new List<JCHVRF.Model.NextGen.MyNode>();
            WL.Node addFlowItemPiping = (WL.Node)sysItemSource.MyPipingNodeOut;
            GetHeightDifferenceNodes(addFlowItemPiping, null, sysItemSource, nodes);
            int i = -1;
            foreach (JCHVRF.Model.NextGen.MyNode node in nodes)
            {
                int Id = this.ListIDU.Count + 1;
                string modelName = "";
                if (node is JCHVRF.Model.NextGen.MyNodeCH)
                {
                    JCHVRF.Model.NextGen.MyNodeCH nodeCH = node as JCHVRF.Model.NextGen.MyNodeCH;

                    string PositionType = PipingPositionType.Upper.ToString();
                    if (!string.IsNullOrEmpty(nodeCH.Model))
                    {
                        modelName = "CHBox [" + nodeCH.Model + "]";
                    }
                    else
                    {
                        modelName = "CHBox";
                    }
                    double HeightDiff = nodeCH.HeightDiff;
                    if (HeightDiff < 0)
                    {
                        PositionType = PipingPositionType.Lower.ToString();
                        HeightDiff = -HeightDiff;
                    }
                    else if (HeightDiff == 0)
                    {
                        PositionType = PipingPositionType.SameLevel.ToString();
                    }
                    ListIDU.Add(new IDUList
                    {
                        IndoorNo = (int)Id,
                        IndoorName = (string)modelName,
                        Name = Id + "[" + modelName + "]",
                        PositionType = (string)PositionType,
                        HeightDiff = Convert.ToDouble(Unit.ConvertToControl(HeightDiff, UnitType.LENGTH_M, ut_length).ToString("n1")),
                        IndoorTag = node
                    });
                }
                if (node is JCHVRF.Model.NextGen.MyNodeMultiCH)
                {
                    JCHVRF.Model.NextGen.MyNodeMultiCH nodeCH = node as JCHVRF.Model.NextGen.MyNodeMultiCH;
                    string PositionType = PipingPositionType.Upper.ToString();
                    if (!string.IsNullOrEmpty(nodeCH.Model))
                    {
                        modelName = "MultiCHBox [" + nodeCH.Model + "]";
                    }
                    else
                    {
                        modelName = "MultiCHBox";
                    }
                    double HeightDiff = nodeCH.HeightDiff;
                    if (HeightDiff < 0)
                    {
                        PositionType = PipingPositionType.Lower.ToString();
                        HeightDiff = -HeightDiff;
                    }
                    else if (HeightDiff == 0)
                    {
                        PositionType = PipingPositionType.SameLevel.ToString();
                    }
                    ListIDU.Add(new IDUList
                    {
                        IndoorNo = (int)Id,
                        IndoorName = (string)modelName,
                        Name = Id + "[" + modelName + "]",
                        PositionType = (string)PositionType,
                        HeightDiff = Convert.ToDouble(Unit.ConvertToControl(HeightDiff, UnitType.LENGTH_M, ut_length).ToString("n1")),
                        IndoorTag = node
                    });
                }
                if (node is JCHVRF.Model.NextGen.MyNodeIn)
                {
                    JCHVRF.Model.NextGen.MyNodeIn nodeIn = node as JCHVRF.Model.NextGen.MyNodeIn;
                    string Room = "";
                    if (!string.IsNullOrEmpty((new ProjectBLL(thisProject)).GetFloorAndRoom(nodeIn.RoomIndooItem.RoomID)))
                    {
                        Room = (new ProjectBLL(thisProject)).GetFloorAndRoom(nodeIn.RoomIndooItem.RoomID) + " : ";
                    }
                    Room = Room + nodeIn.RoomIndooItem.IndoorName + " [" + (thisProject.BrandCode == "Y" ? nodeIn.RoomIndooItem.IndoorItem.Model_York : nodeIn.RoomIndooItem.IndoorItem.Model_Hitachi) + "]";
                    if (nodeIn.RoomIndooItem != null)
                        modelName = nodeIn.RoomIndooItem.IndoorFullName;
                    else
                        modelName = "";
                    ListIDU.Add(new IDUList
                    {
                        IndoorNo = (int)Id,
                        IndoorName = modelName,
                        Name = modelName,
                        PositionType = nodeIn.RoomIndooItem.PositionType,
                        HeightDiff = Convert.ToDouble(Unit.ConvertToControl(nodeIn.RoomIndooItem.HeightDiff, UnitType.LENGTH_M, ut_length)),
                        IndoorTag = node
                    });

                }
            }
            RecalculateMaxHeightDifference(nodes);
        }
        private void RecalculateMaxHeightDifference(List<NextGenModel.MyNode> nodes)
        {
            double MaxOutdoorLength = 0.0;
            NextGenBLL.PipingBLL pipBll = new NextGenBLL.PipingBLL(this.Project);

            pipBll.StatisticsSystem_HighDiff(sysItemSource, nodes);

            if (sysItemSource.MaxUpperHeightDifferenceLength > sysItemSource.MaxLowerHeightDifferenceLength)
            {
                MaxOutdoorLength = sysItemSource.MaxUpperHeightDifferenceLength;
            }
            else
            {
                MaxOutdoorLength = sysItemSource.MaxLowerHeightDifferenceLength;
            }
            //TxtMaxIndoorHeightDifferences = "Maximum height difference between each Indoor unit  : " + sysItemSource.MaxIndoorHeightDifferenceLength.ToString("n1") + ut_length;
            //TxtMaxOutdoorHeightDifferences = "Maximum height difference between outdoor unit and  indoor units  :" + MaxOutdoorLength.ToString("n1") + ut_length;
            //----------------- Code below for multi-langauge------------//
            TxtMaxIndoorHeightDifferences = Language.Current.GetMessage("MAXIMUM_HEIGHT_DIFFERENCE") + sysItemSource.MaxIndoorHeightDifferenceLength.ToString("n1") + ut_length;
            TxtMaxOutdoorHeightDifferences = Language.Current.GetMessage("MAXIMUM_HEIGHT_DIFFERENCE_BETWEEN_ODU_IDU") + MaxOutdoorLength.ToString("n1") + ut_length;


            VerificationHighDiff();

        }


        public void GetHeightDifferenceNodes(Lassalle.WPF.Flow.Node node, Lassalle.WPF.Flow.Node parent, JCHVRF.Model.NextGen.SystemVRF sysItem, List<JCHVRF.Model.NextGen.MyNode> list)
        {
            if (node is JCHVRF.Model.NextGen.MyNodeOut)
            {
                JCHVRF.Model.NextGen.MyNodeOut nodeOut = node as JCHVRF.Model.NextGen.MyNodeOut;
                GetHeightDifferenceNodes(nodeOut.ChildNode, nodeOut, sysItem, list);
            }
            else if (node is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = node as JCHVRF.Model.NextGen.MyNodeYP;
                foreach (Lassalle.WPF.Flow.Node item in nodeYP.ChildNodes)
                {
                    GetHeightDifferenceNodes(item, nodeYP, sysItem, list);
                }
            }
            else if (node is JCHVRF.Model.NextGen.MyNodeCH)
            {
                JCHVRF.Model.NextGen.MyNodeCH nodeCH = node as JCHVRF.Model.NextGen.MyNodeCH;
                list.Add(nodeCH);
                GetHeightDifferenceNodes(nodeCH.ChildNode, nodeCH, sysItem, list);
            }
            else if (node is JCHVRF.Model.NextGen.MyNodeMultiCH)
            {
                JCHVRF.Model.NextGen.MyNodeMultiCH nodeMCH = node as JCHVRF.Model.NextGen.MyNodeMultiCH;
                list.Add(nodeMCH);
                foreach (Lassalle.WPF.Flow.Node item in nodeMCH.ChildNodes)
                {
                    GetHeightDifferenceNodes(item, nodeMCH, sysItem, list);
                }
            }
            else if (node is JCHVRF.Model.NextGen.MyNodeIn)
            {
                //因为DoPipingCalculation之后可能影响indoor的管径，
                //所以绘制YP型号的时候顺便绘制indoor的管径 add on 20170512 by Shen Junjie
                JCHVRF.Model.NextGen.MyNodeIn nodeIn = node as JCHVRF.Model.NextGen.MyNodeIn;
                list.Add(nodeIn);
            }
        }

        private void OnSettingsClicked()
        {
            SelIDUList.Clear();

            if (listselectedIDU != null)
            {
                if (listselectedIDU.Count > 0)
                {
                    for (int i = 0; i < listselectedIDU.Count; i++)
                    {
                        if (this.SelectedIndex == 0)
                        {
                            cmbPositionType = listselectedIDU[i].PositionType;
                            TxtIndoorHighDifference = Convert.ToString(listselectedIDU[i].HeightDiff);
                        }
                        else if (this.SelectedIndex == 1)
                        {
                            cmbPositionType = listselectedIDU[i].PositionType;
                            TxtIndoorHighDifference = Convert.ToString(listselectedIDU[i].HeightDiff);
                        }
                        else
                        {
                            cmbPositionType = listselectedIDU[i].PositionType;
                            TxtIndoorHighDifference = Convert.ToString(listselectedIDU[i].HeightDiff);
                        }
                    }
                }
            }
            else
            {
                int i = this.selectedRow;
                if (i != -1)
                {
                    if (ListIDU != null && ListIDU.Count > 0)
                    {
                        if (this.SelectedIndex == 0)
                        {
                            cmbPositionType = ListIDU[i].PositionType;
                            TxtIndoorHighDifference = Convert.ToString(ListIDU[i].HeightDiff);
                        }
                        else if (this.SelectedIndex == 1)
                        {
                            cmbPositionType = ListIDU[i].PositionType;
                            TxtIndoorHighDifference = Convert.ToString(ListIDU[i].HeightDiff);
                        }
                        else
                        {
                            cmbPositionType = ListIDU[i].PositionType;
                            TxtIndoorHighDifference = Convert.ToString(ListIDU[i].HeightDiff);
                        }
                    }
                }
            }
            IsOpen = true;
            PopupSettings = Visibility.Visible;
            IsPipingPopupOpened = true;
            SelectedIDUList sel = new SelectedIDUList();
            int index = this.selectedRow + 1;

            if (listselectedIDU != null)
            {
                foreach (IDUList ind in listselectedIDU)
                {
                    SelectedIDUList sels = new SelectedIDUList();
                    sels.IndoorNo = ind.IndoorNo;
                    sels.IndoorName = ind.IndoorName;
                    sel.Name = ind.IndoorNo + "[" + ind.IndoorName + "]";
                    sels.IndoorTag = ind.IndoorTag;
                    SelIDUList.Add(sels);

                }
            }
            else
            {
                foreach (IDUList ind in ListIDU)
                {
                    if (ind.IndoorNo == index)
                    {
                        dynamic tag = ListIDU[SelectedRow];
                        SelectedIDUList sels = new SelectedIDUList();
                        sels.IndoorNo = ind.IndoorNo;
                        sels.IndoorName = ind.IndoorName;
                        sel.Name = ind.IndoorNo + "[" + ind.IndoorName + "]";
                        sels.IndoorTag = ind.IndoorTag;
                        SelIDUList.Add(sels);
                    }
                }

            }
        }
        private void OnSettingsOk()
        {
            RoomIndoor emptyIndoor = new RoomIndoor();
            if (this.SelectedIndex == 0)
            {
                emptyIndoor.PositionType = PipingPositionType.Upper.ToString();

            }
            else if (this.SelectedIndex == 1)
            {
                emptyIndoor.PositionType = PipingPositionType.SameLevel.ToString();

            }
            else
            {
                emptyIndoor.PositionType = PipingPositionType.Lower.ToString();
            }
            if (TxtIndoorHighDifference == "")
            {
                TxtIndoorHighDifference = "0";
            }
            emptyIndoor.HeightDiff = Unit.ConvertToSource(Convert.ToDouble(TxtIndoorHighDifference), UnitType.LENGTH_M, ut_length);

            if (emptyIndoor.PositionType != PipingPositionType.SameLevel.ToString())
            {
                if (emptyIndoor.HeightDiff <= 0)
                {

                    lblPipingLegthError = Msg.GetResourceString("INDOOR_HIGHERDIFFERENCE_LENGTH");
                    return;
                }
            }

            if (emptyIndoor.PositionType == PipingPositionType.Upper.ToString() && emptyIndoor.HeightDiff > sysItemSource.MaxOutdoorAboveHeight)
            {
                double len = Unit.ConvertToControl(sysItemSource.MaxOutdoorAboveHeight, UnitType.LENGTH_M, ut_length);

                IsPipingPopupOpened = false;
                JCHMessageBox.Show(Language.Current.GetMessage("HEIGHT_UPPER_MSG") + len.ToString("n0") + ut_length);
                return;
            }
            if (emptyIndoor.PositionType == PipingPositionType.Lower.ToString() && emptyIndoor.HeightDiff > sysItemSource.MaxOutdoorBelowHeight)
            {
                double len = Unit.ConvertToControl(sysItemSource.MaxOutdoorBelowHeight, UnitType.LENGTH_M, ut_length);
                IsPipingPopupOpened = false;
                JCHMessageBox.Show(Language.Current.GetMessage("HEIGHT_UPPER_MSG") + len.ToString("n0") + ut_length);

                return;
            }
            foreach (SelectedIDUList ind in SelIDUList)
            {

                int IDU_ID = ind.IndoorNo;
                double HeightDiff = emptyIndoor.HeightDiff;
                if (ind.IndoorTag is JCHVRF.Model.NextGen.MyNodeCH)
                {
                    //MyNodeCH nodech = ind.IndoorTag as MyNodeCH;
                    JCHVRF.Model.NextGen.MyNodeCH nodech = ind.IndoorTag as JCHVRF.Model.NextGen.MyNodeCH;
                    if (emptyIndoor.PositionType == PipingPositionType.Lower.ToString())
                    {
                        HeightDiff = -HeightDiff;
                    }
                    nodech.HeightDiff = HeightDiff;
                }
                else if (ind.IndoorTag is JCHVRF.Model.NextGen.MyNodeMultiCH)
                {
                    JCHVRF.Model.NextGen.MyNodeMultiCH nodech = ind.IndoorTag as JCHVRF.Model.NextGen.MyNodeMultiCH;
                    if (emptyIndoor.PositionType == PipingPositionType.Lower.ToString())
                    {
                        HeightDiff = -HeightDiff;
                    }
                    nodech.HeightDiff = HeightDiff;
                }
                else if (ind.IndoorTag is JCHVRF.Model.NextGen.MyNodeIn)
                {

                    JCHVRF.Model.NextGen.MyNodeIn node = ind.IndoorTag as JCHVRF.Model.NextGen.MyNodeIn;
                    node.RoomIndooItem.PositionType = emptyIndoor.PositionType.ToString();
                    node.RoomIndooItem.HeightDiff = HeightDiff;
                }

            }
            RefreshPanel();

            BindHighDifference();
            VerificationHighDiff();


            PopupSettings = Visibility.Hidden;
            IsPipingPopupOpened = false;
        }
        private void VerificationHighDiff()
        {
            ERRList = new List<string>();
            double maxValue = CalculateHighDiff();
            if (maxValue > sysItemSource.MaxDiffIndoorHeight)
            {
                double len = Unit.ConvertToControl(sysItemSource.MaxDiffIndoorHeight, UnitType.LENGTH_M, ut_length);

                lblPipingLegthError = Language.Current.GetMessage("HEIGHT_UPPER_MSG") + len.ToString("n0") + ut_length;
                ERRList.Add(Language.Current.GetMessage("HEIGHT_UPPER_MSG") + len.ToString("n0") + ut_length);
            }
            else if (sysItemSource.MaxCHBoxHighDiffLength > sysItemSource.NormalCHBoxHighDiffLength)
            {
                double len = Unit.ConvertToControl(sysItemSource.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);

                var labelRes = Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length);
                lblPipingLegthError = labelRes;
                ERRList.Add(Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length));
            }
            else if (sysItemSource.MaxCHBox_IndoorHighDiffLength > sysItemSource.NormalCHBox_IndoorHighDiffLength)
            {
                double len = Unit.ConvertToControl(sysItemSource.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, ut_length);

                lblPipingLegthError = Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length);
                ERRList.Add(Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length));
            }
            else if (sysItemSource.MaxSameCHBoxHighDiffLength > sysItemSource.NormalSameCHBoxHighDiffLength)
            {
                double len = Unit.ConvertToControl(sysItemSource.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);

                lblPipingLegthError = Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length);
                ERRList.Add(Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length));
            }
            else
            {
                lblPipingLegthError = "";

            }
        }
        private double CalculateHighDiff()
        {
            List<double> diffList = new List<double>(); //高度差集合
            double maxValue = 0; //最大高度差
            double minValue = 0; //最小高度差
            double indDiff = 0; //室内机与室内机直接的高度差
            if (ListIDU.Count > 0)
            {
                foreach (IDUList ri in ListIDU)
                {
                    double val = Convert.ToDouble(ri.HeightDiff);
                    if (ri.PositionType == PipingPositionType.Lower.ToString())
                    {
                        double m = 0 - val;
                        diffList.Add(m);
                    }
                    else
                    {
                        diffList.Add(val);
                    }
                }
                maxValue = Convert.ToDouble(diffList.Max().ToString("n1"));
                minValue = Convert.ToDouble(diffList.Min().ToString("n1"));
                if (maxValue > minValue)
                {
                    indDiff = maxValue - minValue;
                }
            }
            return indDiff;
        }

        private void OnSettingsCancel()
        {
            PopupSettings = Visibility.Hidden;
            IsPipingPopupOpened = false;
        }

        private void RefreshPanel()
        {
            //this.jccmbPosition.SelectedIndex = 1;
            //this.jctxtIndoorDifference.Text = "0.0";
            //this.jctxtIndoorDifference.Enabled = false;
            //this.cklistIndoor.Items.Clear();

        }

    }

}
