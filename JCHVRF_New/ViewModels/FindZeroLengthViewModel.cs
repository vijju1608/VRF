using System;
using System.Collections.Generic;
using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Windows;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using JCHVRF_New.Utility;

namespace JCHVRF_New.ViewModels
{
    class FindZeroLengthViewModel: ViewModelBase
    {
        #region Fields
        IRegionManager regionManager;
        readonly IEventAggregator _eventAggregator;
        public DelegateCommand NextClickFindZeroLength { get; set; }
        public DelegateCommand LoadFindZeroLengthUI { get; set; }
        public DelegateCommand ApplyAllClick { get; set; }

        private JCHVRF.Model.NextGen.SystemVRF CurrentSystem;

        private bool _isApplyToAll;
        public bool IsApplyToAll
        {
            get { return _isApplyToAll; }
            set { this.SetValue(ref _isApplyToAll, value); }
        }
        private string _pipeLength;
        public string PipeLength
        {
            get { return _pipeLength; }
            set { this.SetValue(ref _pipeLength, value); }
        }

        private int _totalZeroLengthPipes;
        public int TotalZeroLengthPipes
        {
            get { return _totalZeroLengthPipes; }
            set { this.SetValue(ref _totalZeroLengthPipes, value); }
        }

        private string _resultTextZeroPipeLength;
        public string ResultTextZeroPipeLength
        {
            get { return _resultTextZeroPipeLength; }
            set { this.SetValue(ref _resultTextZeroPipeLength, value); }
        }

        private string _lengthUnit;
        public string LengthUnit
        {
            get { return _lengthUnit; }
            set { this.SetValue(ref _lengthUnit, value); }
        }
        #endregion


        public FindZeroLengthViewModel(IRegionManager regionManager,IEventAggregator eventAggregator)
        {
            try
            {
                this.regionManager = regionManager;              
                this._eventAggregator = eventAggregator;
                NextClickFindZeroLength = new DelegateCommand(OnNextClickFindZeroLength);
                ApplyAllClick = new DelegateCommand(OnApplyAllClick);
                LoadFindZeroLengthUI = new DelegateCommand(OnLoaded);
                LengthUnit = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            }
            catch (Exception ex)
            {
                Logger.LogProjectInfo(ex.Message);
            }
       }

        private void OnLoaded()
        {
            IsFocusLengthElement = true;
        }
        private void OnApplyAllClick()
        {
            IsApplyToAll = true;
            OnNextClickFindZeroLength();
            UndoRedoSetup.SetInstanceNullWithInitalizedProjectStack();

        }

        private bool isFocusLengthElement;

        public bool IsFocusLengthElement
        {
            get { return isFocusLengthElement; }
            set { this.SetValue(ref isFocusLengthElement, value); }
        }
        public void OnNextClickFindZeroLength()
        {
            if (!string.IsNullOrEmpty(PipeLength))
            {
                List<string> pipingData = new List<string>();
                pipingData.Add(Convert.ToString(PipeLength));
                if (IsApplyToAll == true)
                {
                    pipingData.Add("1");
                }
                else
                {
                    pipingData.Add("0");
                }
                _eventAggregator.GetEvent<FindZeroNextClick>().Publish(pipingData);
            }
            IsApplyToAll = true;
        }

    }
}