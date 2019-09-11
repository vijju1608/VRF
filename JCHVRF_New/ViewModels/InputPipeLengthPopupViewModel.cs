using JCHVRF.Model;
using JCHVRF_New.Common.Helpers;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;

namespace JCHVRF_New.ViewModels
{
    public class InputPipeLengthPopupViewModel: ViewModelBase
    {
        #region Fields
        IRegionManager regionManager;
        readonly IEventAggregator _eventAggregator;

        public DelegateCommand NextClickInputPipeLength { get; set; }
        public DelegateCommand PreviousClickInputPipeLength { get; set; }
        public DelegateCommand ApplyAllClick { get; set; }
        private string _pipeLength;
        public string PipeLength
        {
            get { return _pipeLength; }
            set { this.SetValue(ref _pipeLength, value); }
        }

        private string _elbowQty;
        public string ElbowQty
        {
            get { return _elbowQty; }
            set { this.SetValue(ref _elbowQty, value); }
        }
        private string _oilTrapQty;
        public string OilTrapQty
        {
            get { return _oilTrapQty; }
            set { this.SetValue(ref _oilTrapQty, value); }
        }

        private bool _isApplyToAll;
        public bool IsApplyToAll
        {
            get { return _isApplyToAll; }
            set { this.SetValue(ref _isApplyToAll, value); }
        }
        private string _lengthUnit;
        public string LengthUnit
        {
            get { return _lengthUnit; }
            set { this.SetValue(ref _lengthUnit, value); }
        }
        #endregion
        public InputPipeLengthPopupViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            try
            {
                this.regionManager = regionManager;
                this._eventAggregator = eventAggregator;
                NextClickInputPipeLength = new DelegateCommand(OnNextClickInputPipeLength);
                PreviousClickInputPipeLength = new DelegateCommand(OnPreviousClickInputPipeLength);
                ApplyAllClick = new DelegateCommand(OnApplyAllClick);
                LengthUnit = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            }
            catch (Exception ex)
            {
                Logger.LogProjectInfo(ex.Message);
            }
        }

        private void OnApplyAllClick()
        {
            try
            {
                IsApplyToAll = true;
                OnPreviousClickInputPipeLength();
            }
            catch(Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void OnPreviousClickInputPipeLength()
        {
            try { 
            if (!string.IsNullOrEmpty(PipeLength))
            {
                List<string> pipingData = new List<string>();
                pipingData.Add(Convert.ToString(PipeLength));
                pipingData.Add(Convert.ToString(OilTrapQty));
                pipingData.Add(Convert.ToString(ElbowQty));
                if (IsApplyToAll == true)
                {
                    pipingData.Add("1");
                }
                else
                {
                    pipingData.Add("0");
                }
                _eventAggregator.GetEvent<InputPipeLengthPreviousClick>().Publish(pipingData);
            }
            IsApplyToAll = false;
            }
            catch(Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        public void OnNextClickInputPipeLength()
        {
            try { 
            if (!string.IsNullOrEmpty(PipeLength))
            {
                List<string> pipingData = new List<string>();
                pipingData.Add(Convert.ToString(PipeLength));
                pipingData.Add(Convert.ToString(OilTrapQty));
                pipingData.Add(Convert.ToString(ElbowQty));
                if (IsApplyToAll == true)
                {
                    pipingData.Add("1");
                }
                else
                {
                    pipingData.Add("0");
                }
                _eventAggregator.GetEvent<InputPipeLengthNextClick>().Publish(pipingData);
            }
            IsApplyToAll = false;
            }
            catch(Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
    }
}
