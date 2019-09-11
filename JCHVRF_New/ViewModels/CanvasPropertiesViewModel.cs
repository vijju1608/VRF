/****************************** File Header ******************************\
File Name:	CanvasPropertiesViewModel.cs
Date Created:	2/25/2019
Description:	
\*************************************************************************/

using System.Windows.Forms;
using System.Windows.Media;
using Prism.Commands;

namespace JCHVRF_New.ViewModels
{
    using JCHVRF_New.Common.Contracts;
    using JCHVRF_New.Common.Helpers;
    using Prism.Events;
    using System;
    using JCHVRF.Model.NextGen;
    using Language = JCHVRF_New.LanguageData.LanguageViewModel;

    class CanvasPropertiesViewModel : ViewModelBase
    {
        #region Fields
        readonly IEventAggregator _eventAggregator;

        public DelegateCommand ImportImageClickCommand { get; set; }
        public DelegateCommand ClearImageClickCommand { get; set; }
        public DelegateCommand LockImageCheckedCommand { get; set; }

        private bool _canImportImage;
        private bool _canClearImage;
        private bool _canLockImage;
        private bool _isLockChecked;
        private int _opacity;
        private bool _isPlottingScaleEnabled;
        private int _scalingMeterValue;
        private bool _isPlottingNodeVertical;
        private Color? _plottingScaleColor;
        private Color? _nodeTextColor;
        private Color? _pipeColor;
        private Color? _nodeBackgroundColor;
        private Color? _branchKitColor;
        #endregion

        #region Constructors

        public CanvasPropertiesViewModel(IEventAggregator eventAggregator)
        {
            try
            {
                _eventAggregator = eventAggregator;
                SetDefaultCanvasProperties();
                ImportImageClickCommand = new DelegateCommand(OnImageImportClick, CanExecuteImport);
                ClearImageClickCommand = new DelegateCommand(OnClearImageClick, CanExecuteClear);
                LockImageCheckedCommand = new DelegateCommand(OnLockImageCheck, CanExecuteLock);
                _eventAggregator.GetEvent<CanvasPropertiesSubscriber>().Subscribe(ApplyCanvasProperties);
                _eventAggregator.GetEvent<CanvasBuildingImageExistsSubscriber>().Subscribe(BuildingImageExists);
                _eventAggregator.GetEvent<SetResetPlottingScale>().Subscribe(PlottingScaleEnabled);
                _eventAggregator.GetEvent<CleanUpCanvaspropertyTab>().Subscribe(CleanUpCP);
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(null, ex);
            }
        }

        private void ApplyCanvasProperties()
        {
            _eventAggregator.GetEvent<CanvasBuildingImageOpacitySubscriber>().Publish(Opacity);
            _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.NodeText, NodeTextColor));
            _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.NodeBackground, NodeBackgroundColor));
            _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.BranchKit, BranchKitColor));
            _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                new CanvasColorPickerChanges(CanvasColorPropertyEnum.PipeColor, PipeColor));

        }

        #endregion

        #region Properties

        public bool IsLockChecked
        {
            get { return this._isLockChecked; }
            set { this.SetValue(ref _isLockChecked, value); }
        }
        public bool IsPlottingScaleEnabled
        {
            get { return this._isPlottingScaleEnabled; }
            set
            {
                this.SetValue(ref _isPlottingScaleEnabled, value);
                if (ReloadFlag == false)
                {
                    _eventAggregator.GetEvent<CanvasPlottingScaleEnableDisableSubscriber>().Publish(IsPlottingScaleEnabled);
                    if (IsPlottingScaleEnabled)
                    {
                        _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                            new CanvasColorPickerChanges(CanvasColorPropertyEnum.PlottingScale, PlottingScaleColor));
                        _eventAggregator.GetEvent<CanvasPlottingScalingMeterValueSubscriber>().Publish(ScalingMeterValue);
                    }
                }
            }
        }
        public int Opacity
        {
            get { return this._opacity; }
            set
            {
                this.SetValue(ref _opacity, value);
                _eventAggregator.GetEvent<CanvasBuildingImageOpacitySubscriber>().Publish(Opacity);
            }
        }
        public Color? PlottingScaleColor
        {
            get { return this._plottingScaleColor; }
            set
            {
                this.SetValue(ref _plottingScaleColor, value);
                _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.PlottingScale,PlottingScaleColor));
            }
        }
        public Color? NodeTextColor
        {
            get { return this._nodeTextColor; }
            set
            {
                this.SetValue(ref _nodeTextColor, value);
                _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.NodeText,NodeTextColor));
            }
        }
        public Color? PipeColor
        {
            get { return this._pipeColor; }
            set
            {
                this.SetValue(ref _pipeColor, value);
                _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.PipeColor,PipeColor));
            }
        }
        public Color? NodeBackgroundColor
        {
            get { return this._nodeBackgroundColor; }
            set
            {
                this.SetValue(ref _nodeBackgroundColor, value);
                _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.NodeBackground,NodeBackgroundColor));
            }
        }
        public Color? BranchKitColor
        {
            get { return this._branchKitColor; }
            set
            {
                this.SetValue(ref _branchKitColor, value);
                _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Publish(
                    new CanvasColorPickerChanges(CanvasColorPropertyEnum.BranchKit,BranchKitColor));
            }
        }
        public bool IsPlotScaleVertical
        {
            get { return this._isPlottingNodeVertical; }
            set
            {
                this.SetValue(ref _isPlottingNodeVertical, value);
                _eventAggregator.GetEvent<CanvasPlottingScaleDirectionChangeSubscriber>().Publish(IsPlotScaleVertical);
            }
        }
        public int ScalingMeterValue
        {
            get { return this._scalingMeterValue; }
            set
            {
                this.SetValue(ref _scalingMeterValue, value);
                if (ReloadFlag == false)
                {
                    _eventAggregator.GetEvent<CanvasPlottingScalingMeterValueSubscriber>().Publish(ScalingMeterValue);
                }
            }
        }
        private bool CanImportImage
        {
            get { return _canImportImage; }
            set
            {
                _canImportImage = value;
                ImportImageClickCommand.RaiseCanExecuteChanged();
            }
        }
        private bool CanClearImage
        {
            get { return _canClearImage; }
            set
            {
                _canClearImage = value;
                ClearImageClickCommand.RaiseCanExecuteChanged();
            }
        }
        private bool CanLockImage
        {
            get { return _canLockImage; }
            set
            {
                _canLockImage = value;
                LockImageCheckedCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        #region Button Click Commands
        private bool CanExecuteImport()
        {
            return _canImportImage;
        }
        private bool CanExecuteClear()
        {
            return _canClearImage;
        }        
        private bool CanExecuteLock()
        {
            return _canLockImage;
        }

        private void CleanUpCP()
        {
            _eventAggregator.GetEvent<SetResetPlottingScale>().Unsubscribe(PlottingScaleEnabled);
            _eventAggregator.GetEvent<CleanUpCanvaspropertyTab>().Unsubscribe(CleanUpCP);
        }
        private bool ReloadFlag = false;
        private void PlottingScaleEnabled(object node)
        {
            ReloadFlag = true;
            MyNodePlottingScale plottingnode = (MyNodePlottingScale)node;
            if (plottingnode != null)
            {
                IsPlottingScaleEnabled = true;
                ScalingMeterValue = Convert.ToInt32(plottingnode.ActualLength);
            }
            else
            {
                ScalingMeterValue = 0;
                IsPlottingScaleEnabled = false;
            }
            ReloadFlag = false;
        }

        private void OnImageImportClick()
        {
            const string imageSelectWindowTitle = "Select a building image file";
            
           // const string imgSelect = Language.Current.GetMessage("SELCET_BUILD");
           
            const string imageFilter = "Image|*.jpg;*.jpeg;*.bmp;*.png;";
            var fileDialog = new OpenFileDialog
            {
                Title = imageSelectWindowTitle,
                Multiselect = false,
                Filter = imageFilter
            };
            if (fileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var imageFilePath = fileDialog.FileName;
            if (!string.IsNullOrEmpty(imageFilePath) && System.IO.File.Exists(imageFilePath))
            {
                _eventAggregator.GetEvent<CanvasBuildingImageSubscriber>().Publish(imageFilePath);
                CanImportImage = false;
                CanClearImage = true;
                CanLockImage = true;
            }
        }
        private void OnClearImageClick()
        {
            _eventAggregator.GetEvent<CanvasBuildingImageSubscriber>().Publish(string.Empty);
            CanClearImage = false;
            CanImportImage = true;
            CanLockImage = false;
        }
        private void OnLockImageCheck()
        {
            _eventAggregator.GetEvent<CanvasLockBuildingImageSubscriber>().Publish(IsLockChecked);
            CanClearImage = !_isLockChecked;
            CanImportImage = false;
        }
        private void BuildingImageExists(BackgroundImageProperties imageProperties)
        {
            if (!string.IsNullOrEmpty(imageProperties.imagePath))
            {
                CanImportImage = false;
                CanClearImage = true;
                CanLockImage = true;
                Opacity = imageProperties.opacity;
            }
            else
            {
                CanClearImage = false;
                CanImportImage = true;
                CanLockImage = false;
                Opacity = 100;
            }
        }
        #endregion
        #region Default Canvas Properties
        private void SetDefaultCanvasProperties()
        {
            _canImportImage = true;
            _canClearImage = false;
            _canLockImage = false;
            _opacity = 100;
            _scalingMeterValue = 0;
            _plottingScaleColor = Colors.Black;
            _pipeColor = Colors.Black;
            _nodeTextColor = Colors.Black;
            _nodeBackgroundColor = Colors.White;
            _branchKitColor = Colors.Blue;
            _isPlottingNodeVertical= false;
        }
        #endregion
    }
}
