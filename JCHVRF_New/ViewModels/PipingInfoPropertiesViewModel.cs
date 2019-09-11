/****************************** File Header ******************************\
File Name:	PipingInfoPropertiesViewModel.cs
Date Created:	2/20/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media;
    using JCBase.Utility;
    using JCHVRF.Model;
    using JCHVRF.Model.NextGen;
    using JCHVRF.MyPipingBLL.NextGen;
    using JCHVRF.VRFMessage;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using Prism.Commands;
    using Prism.Events;
    using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

    public class PipingInfoPropertiesViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<PipingInfoModel> _ListPipingInfoHeight;

        private ObservableCollection<PipingInfoModel> _ListPipingInfoLength;

        private ObservableCollection<PipingInfoModel> _ListPipingInfoAdditional;

        private string _LengthUnit;

        IEventAggregator _eventAggregator;

        private const string TOTAL_LENGTH = "Total length";
        private const string MAX_LENGTH = "Max. length";
        private const string PIPE_LENGTH = "Piping length";
        private const string HEIGHT_DIFF = "Ht diff.";
        private const string FONT_STYLE = "Arial";

        #endregion

        #region Constructors

        public PipingInfoPropertiesViewModel(IEventAggregator EventAggregator)
        {
            _eventAggregator = EventAggregator;
            LoadedCommand = new DelegateCommand(OnLoad);
            UnloadedCommand = new DelegateCommand(OnUnload);
            //Title = "Piping Info";
            LengthUnit = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            //subscribing event in conscturctor

        }

        private void BindPipingInfo(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
            if (currentSystem != null && currentSystem.MyPipingNodeOut != null)
            {
                BindPipingInfoLength(currentSystem);
                BindPipingInfoHeight(currentSystem);
                BindAdditionalPipingInfo(currentSystem);
            }
        }
        private void OnUnload()
        {
            _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Unsubscribe(OnPipingValidation);
            //_eventAggregator.GetEvent<SystemSelectedTabSubscriber>().Unsubscribe(OnSystemTabSelected);
            _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Unsubscribe(OnSystemLoad);
            _eventAggregator.GetEvent<SetDefaultPipingInfo>().Unsubscribe(SetDefaultPipingInfo);
        }

        private void OnPipingValidation(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
            BindPipingInfo(currentSystem);
        }

        private void OnLoad()
        {
            _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Subscribe(OnPipingValidation);
            _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Subscribe(OnSystemLoad);
            _eventAggregator.GetEvent<SetDefaultPipingInfo>().Subscribe(SetDefaultPipingInfo);
            //_eventAggregator.GetEvent<SetPipingInfoSubscriber>().Subscribe(OnSystemLoad);
        }

        private void OnSystemLoad(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
            BindPipingInfo(currentSystem);
        }

        #endregion

        #region Properties

        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand UnloadedCommand { get; set; }

        /// <summary>
        /// Gets or sets the ListPipingInfoHeight
        /// </summary>
        public ObservableCollection<PipingInfoModel> ListPipingInfoHeight
        {
            get
            {
                return _ListPipingInfoHeight;
            }
            set
            {
                this.SetValue(ref _ListPipingInfoHeight, value);
            }
        }

        ///// <summary>
        ///// Gets or sets the ListPipingInfoLength
        ///// </summary>
        public ObservableCollection<PipingInfoModel> ListPipingInfoLength
        {
            get
            {
                return _ListPipingInfoLength;
            }
            set
            {
                this.SetValue(ref _ListPipingInfoLength, value);
            }
        }

        ///// <summary>
        ///// Gets or sets the ListPipingInfoLength
        ///// </summary>
        public ObservableCollection<PipingInfoModel> ListPipingInfoAdditional
        {
            get
            {
                return _ListPipingInfoAdditional;
            }
            set
            {
                this.SetValue(ref _ListPipingInfoAdditional, value);
            }
        }

        /// <summary>
        /// Gets or sets the unit
        /// </summary>
        public string LengthUnit
        {
            get
            {
                return _LengthUnit;
            }
            set
            {
                this.SetValue(ref _LengthUnit, value);
            }
        }

        //Additional PipingInformation


        public string IUConnectable { get; set; }
        public string ConnectedCap { get; set; }
        public double IUConnectableProjectInfo { get; set; }
        public double IUConnectableMaxInfo { get; set; }
        public double ConnectedCapProjectInfo { get; set; }
        public double ConnectedCaplowerRangeMaxInfo { get; set; }
        public double ConnectedCapupperRangeMaxInfo { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// The BindPipingInfoLength
        /// </summary>
        public void BindPipingInfoLength(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
            ListPipingInfoLength = new ObservableCollection<PipingInfoModel>();

            PipingInfoModel PipingObjectLength = new PipingInfoModel();
            PipingObjectLength.Description = TOTAL_LENGTH;
            PipingObjectLength.LongDescription = Msg.GetResourceString("PipingRules_TotalPipeLength");
            PipingObjectLength.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.TotalPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectLength.Max = currentSystem.MaxTotalPipeLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxTotalPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectLength.IsValid = currentSystem.MaxTotalPipeLength == 0 ? true : (Unit.ConvertToControl(currentSystem.TotalPipeLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxTotalPipeLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //if (PipingObjectLength.Max.Equals("-"))
            //{
            //    PipingObjectLength.IsValid = true;
            //}
            //else
            //{
            //    PipingObjectLength.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectLength.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectLength.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectLength.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectLength.Max);
            //}

            ListPipingInfoLength.Add(PipingObjectLength);

            PipingInfoModel PipingObjectMaxLengthActual = new PipingInfoModel();

            FormattedText actualLength = new FormattedText("Actuals",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);

            PipingObjectMaxLengthActual.Description = MAX_LENGTH + " " + actualLength.Text;
            PipingObjectMaxLengthActual.LongDescription = Msg.GetResourceString("PipingRules_PipeActualLength");
            PipingObjectMaxLengthActual.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.PipeActualLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthActual.Max = currentSystem.MaxPipeLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthActual.IsValid = currentSystem.MaxPipeLength == 0 ? true : (Unit.ConvertToControl(currentSystem.PipeActualLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxPipeLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //if (PipingObjectMaxLengthActual.Max.Equals("-"))
            //{
            //    PipingObjectLength.IsValid = true;
            //}
            //else
            //{
            //    PipingObjectMaxLengthActual.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActual.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActual.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActual.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActual.Max);
            //}

            ListPipingInfoLength.Add(PipingObjectMaxLengthActual);

            PipingInfoModel PipingObjectMaxLengthEquiVal = new PipingInfoModel();

            FormattedText equivalLength = new FormattedText("Equival",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);

            PipingObjectMaxLengthEquiVal.Description = MAX_LENGTH + " " + equivalLength.Text;
            PipingObjectMaxLengthEquiVal.LongDescription = Msg.GetResourceString("PipingRules_PipeEquivalentLength");
            PipingObjectMaxLengthEquiVal.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.PipeEquivalentLength, UnitType.LENGTH_M, LengthUnit).ToString("n0"));
            PipingObjectMaxLengthEquiVal.Max = currentSystem.MaxEqPipeLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxEqPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthEquiVal.IsValid = currentSystem.MaxEqPipeLength == 0 ? true : (Unit.ConvertToControl(currentSystem.PipeEquivalentLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxEqPipeLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //if (PipingObjectMaxLengthEquiVal.Max.Equals("-"))
            //{
            //    PipingObjectLength.IsValid = true;
            //}
            //else
            //{
            //    PipingObjectMaxLengthEquiVal.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthEquiVal.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthEquiVal.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthEquiVal.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthEquiVal.Max);
            //}


            ListPipingInfoLength.Add(PipingObjectMaxLengthEquiVal);

            PipingInfoModel PipingObjectMaxLengthFirstPipeLength = new PipingInfoModel();

            FormattedText firstPipeLength = new FormattedText("Mkit-IDU",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);

            PipingObjectMaxLengthFirstPipeLength.Description = MAX_LENGTH + " " + firstPipeLength.Text;
            PipingObjectMaxLengthFirstPipeLength.LongDescription = Msg.GetResourceString("PipingRules_FirstPipeLength");
            PipingObjectMaxLengthFirstPipeLength.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.FirstPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthFirstPipeLength.Max = currentSystem.MaxIndoorLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxIndoorLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthFirstPipeLength.IsValid = currentSystem.MaxIndoorLength == 0 ? true : (Unit.ConvertToControl(currentSystem.FirstPipeLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxIndoorLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //if (PipingObjectMaxLengthFirstPipeLength.Max.Equals("-"))
            //{
            //    PipingObjectLength.IsValid = true;
            //}
            //else
            //{
            //    PipingObjectMaxLengthFirstPipeLength.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthFirstPipeLength.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthFirstPipeLength.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthFirstPipeLength.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthFirstPipeLength.Max);
            //}

            ListPipingInfoLength.Add(PipingObjectMaxLengthFirstPipeLength);

            PipingInfoModel PipingObjectMaxLengthActualMaxMKIndoorPipeLength = new PipingInfoModel();

            FormattedText eachIndoorLength = new FormattedText("Mkit-IDU",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);

            PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Description = MAX_LENGTH + " " + eachIndoorLength.Text;
            PipingObjectMaxLengthActualMaxMKIndoorPipeLength.LongDescription = Msg.GetResourceString("PipingRules_ActualMaxMKIndoorPipeLength");
            PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.ActualMaxMKIndoorPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Max = currentSystem.MaxMKIndoorPipeLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectMaxLengthActualMaxMKIndoorPipeLength.IsValid = currentSystem.MaxMKIndoorPipeLength == 0 ? true : (Unit.ConvertToControl(currentSystem.ActualMaxMKIndoorPipeLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //if (PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Max.Equals("-"))
            //{
            //    PipingObjectLength.IsValid = true;
            //}
            //else
            //{
            //    PipingObjectMaxLengthActualMaxMKIndoorPipeLength.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectMaxLengthActualMaxMKIndoorPipeLength.Max);
            //}

            ListPipingInfoLength.Add(PipingObjectMaxLengthActualMaxMKIndoorPipeLength);

            if ((currentSystem != null) && (currentSystem.OutdoorItem != null))
            {
                if (!string.IsNullOrEmpty(currentSystem.OutdoorItem.JointKitModelG) && !string.IsNullOrEmpty(currentSystem.OutdoorItem.JointKitModelG.Trim()) && currentSystem.OutdoorItem.JointKitModelG.Trim() != "-")
                {
                    PipingInfoModel PipingObjectPipingLengthConKitOutdoorPipeLength = new PipingInfoModel();

                    FormattedText conKitOutdoorPipeLength = new FormattedText("Ckit-ODU",
                            System.Globalization.CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);
                    double MaxPipeLengths = 0;
                    if (currentSystem.IsInputLengthManually)
                    {
                        MaxPipeLengths = PipingBLL.GetMaxPipeLengthOfNodeOut(currentSystem.MyPipingNodeOut);
                    }

                    PipingObjectPipingLengthConKitOutdoorPipeLength.Description = PIPE_LENGTH + " " + conKitOutdoorPipeLength.Text;
                    PipingObjectPipingLengthConKitOutdoorPipeLength.LongDescription = Msg.GetResourceString("PipingRules_PipeLengthes");

                    PipingObjectPipingLengthConKitOutdoorPipeLength.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(MaxPipeLengths, UnitType.LENGTH_M, LengthUnit));
                    PipingObjectPipingLengthConKitOutdoorPipeLength.Max = currentSystem.MaxFirstConnectionKitToEachODU == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxFirstConnectionKitToEachODU, UnitType.LENGTH_M, LengthUnit));
                    PipingObjectPipingLengthConKitOutdoorPipeLength.IsValid = currentSystem.MaxFirstConnectionKitToEachODU == 0 ? true : (Unit.ConvertToControl(MaxPipeLengths > 0 ? MaxPipeLengths : 0, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxFirstConnectionKitToEachODU, UnitType.LENGTH_M, LengthUnit) ? true : false);

                    ListPipingInfoLength.Add(PipingObjectPipingLengthConKitOutdoorPipeLength);
                }
            }

            var ishrModel = IsHeatRecovery(currentSystem);
            if (ishrModel)
            {
                AddCHBoxPipingLengthInfo(currentSystem);
            }
        }

        /// <summary>
        /// The BindPipingInfoHeight
        /// </summary>
        public void BindPipingInfoHeight(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
            ListPipingInfoHeight = new ObservableCollection<PipingInfoModel>();

            PipingInfoModel PipingObjectHeightDiffUpper = new PipingInfoModel();

            FormattedText upperOULength = new FormattedText("O.U is upper",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);

            PipingObjectHeightDiffUpper.Description = HEIGHT_DIFF + " " + upperOULength.Text;
            PipingObjectHeightDiffUpper.LongDescription = Msg.GetResourceString("PipingRules_HeightDiffH");
            PipingObjectHeightDiffUpper.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxUpperHeightDifferenceLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectHeightDiffUpper.Max = currentSystem.MaxOutdoorAboveHeight == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, LengthUnit));
            PipingObjectHeightDiffUpper.IsValid = currentSystem.MaxOutdoorAboveHeight == 0 ? true : (Unit.ConvertToControl(currentSystem.MaxUpperHeightDifferenceLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, LengthUnit) ? true : true);
            //PipingObjectHeightDiffUpper.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffUpper.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffUpper.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffUpper.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffUpper.Max);

            ListPipingInfoHeight.Add(PipingObjectHeightDiffUpper);

            PipingInfoModel PipingObjectHeightDiffLower = new PipingInfoModel();

            FormattedText lowerOULength = new FormattedText("O.U is lower",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, new Typeface(FONT_STYLE), 9, System.Windows.Media.Brushes.Black);

            PipingObjectHeightDiffLower.Description = HEIGHT_DIFF + " " + lowerOULength.Text;
            PipingObjectHeightDiffLower.LongDescription = Msg.GetResourceString("PipingRules_HeightDiffL");
            PipingObjectHeightDiffLower.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxLowerHeightDifferenceLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectHeightDiffLower.Max = currentSystem.MaxOutdoorBelowHeight == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, LengthUnit));
            PipingObjectHeightDiffLower.IsValid = currentSystem.MaxOutdoorBelowHeight == 0 ? true : (Unit.ConvertToControl(currentSystem.MaxLowerHeightDifferenceLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffLower.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffLower.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffLower.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffLower.Max);

            ListPipingInfoHeight.Add(PipingObjectHeightDiffLower);

            PipingInfoModel PipingObjectHeightDiffIDU = new PipingInfoModel();

            PipingObjectHeightDiffIDU.Description = HEIGHT_DIFF + " " + "between IDU";
            PipingObjectHeightDiffIDU.LongDescription = Msg.GetResourceString("PipingRules_DiffIndoorHeight");
            PipingObjectHeightDiffIDU.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxIndoorHeightDifferenceLength, UnitType.LENGTH_M, LengthUnit));
            PipingObjectHeightDiffIDU.Max = currentSystem.MaxDiffIndoorHeight == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currentSystem.MaxDiffIndoorHeight, UnitType.LENGTH_M, LengthUnit));
            PipingObjectHeightDiffIDU.IsValid = currentSystem.MaxDiffIndoorHeight == 0 ? true : (Unit.ConvertToControl(currentSystem.MaxIndoorHeightDifferenceLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currentSystem.MaxDiffIndoorHeight, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffIDU.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffIDU.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffIDU.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectHeightDiffIDU.Max);

            ListPipingInfoHeight.Add(PipingObjectHeightDiffIDU);

            var ishrModel = IsHeatRecovery(currentSystem);
            if (ishrModel)
            {
                AddCHBoxPipingHeightInfo(currentSystem);
            }

        }

        /// <summary>
        /// The BindAdditionalPipingInfo
        /// </summary>
        public void BindAdditionalPipingInfo(JCHVRF.Model.NextGen.SystemVRF currentSystem)
        {
            if (currentSystem.OutdoorItem == null) return;
            ListPipingInfoAdditional = new ObservableCollection<PipingInfoModel>();

            PipingInfoModel PipingObjectIUConnectable = new PipingInfoModel();

            int indoorsCount = 0;
            indoorsCount = Project.CurrentProject.RoomIndoorList.FindAll(p => p.SystemID == currentSystem.Id).Count;

            PipingObjectIUConnectable.Description = "IU Connectable (Min / recommended / Max)";
            PipingObjectIUConnectable.LongDescription = Msg.GetResourceString("PipingRules_IUConnectable");
            PipingObjectIUConnectable.ValueDescription = indoorsCount.ToString();
            PipingObjectIUConnectable.MaxDescription = ("1/" + currentSystem.OutdoorItem.RecommendedIU.ToString() + "/" + currentSystem.OutdoorItem.MaxIU).Trim();
            PipingObjectIUConnectable.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectIUConnectable.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectIUConnectable.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectIUConnectable.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectIUConnectable.Max);
            ListPipingInfoAdditional.Add(PipingObjectIUConnectable);

            PipingInfoModel PipingObjectConnectedCap = new PipingInfoModel();

            string Ratio = "50%-130%";

            if (currentSystem.OutdoorItem.Series.Contains("FSNP")
                || currentSystem.OutdoorItem.Series.Contains("FSXNP")
                || currentSystem.OutdoorItem.Series.Contains("FSNS7B") || currentSystem.OutdoorItem.Series.Contains("FSNS5B")
                || currentSystem.OutdoorItem.Series.Contains("FSNC7B") || currentSystem.OutdoorItem.Series.Contains("FSNC5B") //巴西的Connection Ratio可以达到150% 20190105 by Yunxiao Lin
                )
            {
                Ratio = "50%-150%";
            }

            if (currentSystem.OutdoorItem.Series.Contains("FSXNPE"))
            {
                if (currentSystem.OutdoorItem.CoolingCapacity > 150)
                {
                    Ratio = "50%-130%";
                }
            }


            PipingObjectConnectedCap.Description = "Connected Cap. (Min - Max)";
            PipingObjectConnectedCap.LongDescription = Msg.GetResourceString("PipingRules_ConnectedCap");
            PipingObjectConnectedCap.ValueDescription = (currentSystem.Ratio * 100).ToString("n0") + "%";
            PipingObjectConnectedCap.MaxDescription = Ratio;
            PipingObjectConnectedCap.IsValid = JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectConnectedCap.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectConnectedCap.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectConnectedCap.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(PipingObjectConnectedCap.Max);

            ListPipingInfoAdditional.Add(PipingObjectConnectedCap);
        }

        private void AddCHBoxPipingHeightInfo(JCHVRF.Model.NextGen.SystemVRF currSystem)
        {

            //Height Difference between CH-Box and Indoor Units
            var pipingHeightModel = new PipingInfoModel();

            pipingHeightModel.Description = Msg.GetResourceString("PipingRules_DiffCHBox_IndoorHeight");
            pipingHeightModel.LongDescription = Msg.GetResourceString("PipingRules_DiffCHBox_IndoorHeight");
            pipingHeightModel.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currSystem.MaxCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, LengthUnit));
            pipingHeightModel.Max = currSystem.NormalCHBox_IndoorHighDiffLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currSystem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, LengthUnit));
            pipingHeightModel.IsValid = currSystem.NormalCHBox_IndoorHighDiffLength == 0 ? true : (Unit.ConvertToControl(currSystem.MaxCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currSystem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            //JCHVRF.BLL.CommonBLL.DoubleParser(pipingHeightModel.Value) >= JCHVRF.BLL.CommonBLL.DoubleParser(pipingHeightModel.Min) && JCHVRF.BLL.CommonBLL.DoubleParser(pipingHeightModel.Value) <= JCHVRF.BLL.CommonBLL.DoubleParser(pipingHeightModel.Max);
            ListPipingInfoHeight.Add(pipingHeightModel);

            //Height Difference between Indoor Units using the Same Branch of CH-Box
            var pipingHeightBwIndoors = new PipingInfoModel();

            pipingHeightBwIndoors.Description = Msg.GetResourceString("PipingRules_DiffMulitBoxHeight");
            pipingHeightBwIndoors.LongDescription = Msg.GetResourceString("PipingRules_DiffMulitBoxHeight");
            pipingHeightBwIndoors.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currSystem.MaxSameCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit));
            pipingHeightBwIndoors.Max = currSystem.NormalSameCHBoxHighDiffLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currSystem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit));
            pipingHeightBwIndoors.IsValid = currSystem.NormalSameCHBoxHighDiffLength == 0 ? true : (Unit.ConvertToControl(currSystem.MaxSameCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currSystem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit) ? true : false);

            ListPipingInfoHeight.Add(pipingHeightBwIndoors);


            //Height Difference between CH-Boxes
            var pipingBwChBoxes = new PipingInfoModel();

            pipingBwChBoxes.Description = Msg.GetResourceString("PipingRules_DiffCHBoxHeight");
            pipingBwChBoxes.LongDescription = Msg.GetResourceString("PipingRules_DiffCHBoxHeight");
            pipingBwChBoxes.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currSystem.MaxCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit));
            pipingBwChBoxes.Max = currSystem.NormalCHBoxHighDiffLength == 0 ? "-" : JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(currSystem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit));
            pipingBwChBoxes.IsValid = currSystem.NormalCHBoxHighDiffLength == 0 ? true : (Unit.ConvertToControl(currSystem.MaxCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit) <= Unit.ConvertToControl(currSystem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, LengthUnit) ? true : false);
            ListPipingInfoHeight.Add(pipingBwChBoxes);
        }

        private void AddCHBoxPipingLengthInfo(JCHVRF.Model.NextGen.SystemVRF currSystem)
        {
            {
                bool isAllOK = true;
                var chBoxs = new List<dynamic>();
                Action<Lassalle.WPF.Flow.Node> getChBoxes = (node1) =>
                {
                    double actual;
                    double max;
                    string model;
                    if (node1 is JCHVRF.Model.NextGen.MyNodeCH)
                    {
                        var item = (JCHVRF.Model.NextGen.MyNodeCH)node1;
                        actual = item.ActualTotalCHIndoorPipeLength;
                        max = item.MaxTotalCHIndoorPipeLength;
                        model = item.Model;
                    }
                    else if (node1 is JCHVRF.Model.NextGen.MyNodeMultiCH)
                    {
                        var item = (JCHVRF.Model.NextGen.MyNodeMultiCH)node1;
                        actual = item.ActualTotalCHIndoorPipeLength;
                        max = item.MaxTotalCHIndoorPipeLength;
                        model = item.Model;
                    }
                    else
                    {
                        return;
                    }

                    bool isOK = !(max > 0 && actual > max);
                    isAllOK = isAllOK && isOK;

                    var chbox = chBoxs.Find(p => p.Rules == model);
                    if (chbox == null)
                    {
                        chbox = new System.Dynamic.ExpandoObject();
                        chbox.Rules = model;
                        chbox.Actual = actual;
                        chbox.Max = max;
                        chbox.isOK = isOK;
                        chBoxs.Add(chbox);
                    }
                    else
                    {
                        if ((chbox.isOK && isOK && actual > chbox.Actual)  //高的覆盖低的
                            || (chbox.isOK && !isOK))   //出错的覆盖正常的
                        {
                            chbox.Actual = actual;
                            chbox.Max = max;
                            chbox.isOK = isOK;
                        }
                    }
                };
                JCHVRF.MyPipingBLL.NextGen.PipingBLL.EachNode(currSystem.MyPipingNodeOut, getChBoxes);
                if (chBoxs.Count > 0)
                {
                    //Total piping length between CH-Box and Each Indoor Unit
                    PipingInfoModel pipingRulesChBoxLength = new PipingInfoModel();
                    pipingRulesChBoxLength.Description = Msg.GetResourceString("PipingRules_CHBoxs");
                    pipingRulesChBoxLength.LongDescription = Msg.GetResourceString("PipingRules_CHBoxs");
                    pipingRulesChBoxLength.Max = "-";
                    pipingRulesChBoxLength.Value = "-";
                    pipingRulesChBoxLength.IsValid = isAllOK;

                    ListPipingInfoLength.Add(pipingRulesChBoxLength);

                    chBoxs.ForEach((c) =>
                    {
                        PipingInfoModel pipingChBoxValues = new PipingInfoModel();
                        pipingChBoxValues.Description = c.Rules == null ? Msg.GetResourceString("PipingRules_CH_Box") : c.Rules;
                        pipingChBoxValues.LongDescription = c.Rules == null ? Msg.GetResourceString("PipingRules_CH_Box") : c.Rules;
                        pipingChBoxValues.Value = JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(c.Actual, UnitType.LENGTH_M, LengthUnit));
                        pipingChBoxValues.Max = c.Max > 0 ? JCHVRF.BLL.CommonBLL.StringConversion(Unit.ConvertToControl(c.Max, UnitType.LENGTH_M, LengthUnit)) : JCHVRF.BLL.CommonBLL.StringConversion("-");
                        pipingChBoxValues.IsValid = c.isOK ? true : false;
                        ListPipingInfoLength.Add(pipingChBoxValues);
                    });
                }
            }
        }

        private bool IsHeatRecovery(JCHVRF.Model.NextGen.SystemVRF system)
        {
            if (system == null || system.OutdoorItem==null) { return false; }
            return (system.OutdoorItem.ProductType.Contains("Heat Recovery") ||
                    system.OutdoorItem.ProductType.Contains(", HR"));
        }
        private void SetDefaultPipingInfo()
        {
            ListPipingInfoLength = new ObservableCollection<PipingInfoModel>();
            ListPipingInfoHeight = new ObservableCollection<PipingInfoModel>();
            ListPipingInfoAdditional = new ObservableCollection<PipingInfoModel>();
        }
        #endregion
    }
}
