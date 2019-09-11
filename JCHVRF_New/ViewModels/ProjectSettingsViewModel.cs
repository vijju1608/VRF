using JCHVRF.BLL.New;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.Common.Contracts;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Utility;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.ViewModels
{
    public class ProjectSettingsViewModel : ViewModelBase
    {
        #region Fields
        private IEventAggregator _eventAggregator;
        private IModalWindowService _winService;
        #endregion Fields
        #region Delegate Command
        public DelegateCommand SaveClickedCommand { get; set; }
        public DelegateCommand CancelClickCommand { get; private set; }

        #endregion Delegate Command
        #region constructor and Initisation
        public ProjectSettingsViewModel(IEventAggregator EventAggregator, IModalWindowService winService)
        {
            _eventAggregator = EventAggregator;
            _winService = winService;
            SaveClickedCommand = new DelegateCommand(OnSaveClickedCommand);
            CancelClickCommand = new DelegateCommand(CancelClick);
        }
        #endregion constructor and Initisation

        #region Private Methods
        private void OnSaveClickedCommand()
        {
            _eventAggregator.GetEvent<ProjectSettingsSave>().Publish();
            var proj = JCHVRF.Model.Project.GetProjectInstance;
            if (proj.IsBothMode == true)
            {
                proj.IsCoolingModeEffective = true;
                proj.IsHeatingModeEffective = true;
            }
            ProjectInfoBLL objPrjbll = new ProjectInfoBLL();
            if (objPrjbll.UpdateProject(proj))
            {
                JCHMessageBox.Show(Langauge.Current.GetMessage("SUCCESSFULLY_UPDATE"));//"Successfully Updated!");
                RefreshDashBoard();
            }
            //bug 3825
            var currentSystem = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
            _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(currentSystem);
            //bug 3825
            UtilTrace.SaveHistoryTraces();
            
        }
        private void CancelClick()
        {
            _winService.Close(ViewKeys.ProjectSettingsView);
            _eventAggregator.GetEvent<Cleanup>().Publish();

        }
        
        private void RefreshDashBoard()
        {
            _eventAggregator.GetEvent<RefreshDashboard>().Publish();
        }
        #endregion Private Methods
    }
}
