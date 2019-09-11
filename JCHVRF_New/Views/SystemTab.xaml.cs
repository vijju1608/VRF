using JCHVRF.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using JCHVRF_New.ViewModels;
using NextGenModel = JCHVRF.Model.NextGen;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Utility;
using JCHVRF_New.Model;
using JCHVRF.BLL;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using newUtilTrace = JCHVRF_New.Utility;
using Prism.Regions;
using JCHVRF_New.Common.Constants;
using JCHVRF_New.LanguageData;
using System.Linq;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for SystemTab.xaml
    /// </summary>
    public partial class SystemTab : UserControl
    {
        public JCHVRF.Model.Project projectLegacy
        {
            get; set;
        }

        public SystemBase HvacSystem
        {

            get { return (SystemBase)GetValue(SystemProperty); }
            set
            {
                SetValue(SystemProperty, value);
                //                TotalHeatExUnitInfoViewModel model = (TotalHeatExUnitInfoViewModel) DataContext;
                //                model.IsSelected = value;
            }
        }

        public static readonly DependencyProperty SystemProperty =
            DependencyProperty.Register("HvacSystem", typeof(SystemBase), typeof(SystemTab),
                new PropertyMetadata(null, new PropertyChangedCallback(IsSelectedChanged)));

        private IEventAggregator _eventAggregator;
        IRegionManager regionManagerBada;
        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            SystemTab systemTab = (SystemTab)d;
            SystemTabViewModel viewModel = (SystemTabViewModel)systemTab.DataContext;
            WorkFlowContext.CurrentSystem = systemTab.HvacSystem;
            viewModel.CurrentSystem = systemTab.HvacSystem;
            if (systemTab?.HvacSystem != null)
            {
                Project.GetProjectInstance.SelectedSystemID = systemTab.HvacSystem.Id;
            }
            else
            {
                Project.GetProjectInstance.SelectedSystemID = string.Empty;
            }
            systemTab.designerCanvas.Refresh((SystemBase)e.OldValue, (SystemBase)e.NewValue);
            //UndoRedoSetup.SetInstanceNull();
            if (!Project.GetProjectInstance.IsPerformingUndoRedo && !string.IsNullOrEmpty(Project.GetProjectInstance.SelectedSystemID))
            {
                UtilTrace.SaveHistoryTraces();
            }
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        public SystemTab()
        {
            InitializeComponent();
            this.regionManagerBada = ((SystemTabViewModel)DataContext).RegionManager;
            _eventAggregator = ((SystemTabViewModel)DataContext).EventAggregator;
            _eventAggregator.GetEvent<SetHeatExchangerPropertiesOnCanvas>().Subscribe(OnSetHeatExchangerPropertiesOnCanvas);//sss
            _eventAggregator.GetEvent<SetOduPropertiesOnCanvas>().Subscribe(OnSetOduPropertiesOnCanvas);
            _eventAggregator.GetEvent<FilterColor>().Subscribe(SetFilterBackground);
            _eventAggregator.GetEvent<HEFilterColor>().Subscribe(SetHEFilterBackground);
            _eventAggregator.GetEvent<VRFFilterColor>().Subscribe(SetVRFFilterBackground);
            _eventAggregator.GetEvent<MasterDesignerUnload>().Subscribe(Cleanup);            
        }

        private void Cleanup()
        {
            _eventAggregator.GetEvent<SetHeatExchangerPropertiesOnCanvas>().Unsubscribe(OnSetHeatExchangerPropertiesOnCanvas);//sss
            _eventAggregator.GetEvent<FilterColor>().Unsubscribe(SetFilterBackground);
            _eventAggregator.GetEvent<HEFilterColor>().Unsubscribe(SetHEFilterBackground);
            _eventAggregator.GetEvent<VRFFilterColor>().Unsubscribe(SetVRFFilterBackground);
            _eventAggregator.GetEvent<MasterDesignerUnload>().Unsubscribe(Cleanup);
        }

        private void SetVRFFilterBackground()
        {
            btnVRF_Click(null, null);
        }

        private void SetHEFilterBackground()
        {
            btnHeatExchanger_Click(null, null);
        }

        private void OnTabSelected(SystemBase obj)
        {
            //designerCanvas.Refresh(obj);
            SystemTabViewModel viewModel = (SystemTabViewModel)DataContext;
            //viewModel.Refresh(obj);
        }


        public void DragImage(object sender, MouseButtonEventArgs e)
        {

            designerCanvas.DragImage(sender, e);

        }

        public void SelectLineStyle(object sender, MouseEventArgs e)
        {
            //newUtilTrace.UtilTrace.SaveHistoryTraces();
            designerCanvas.SelectLineStyle(sender, e);
        }

        private void OnSetIduPropertiesOnCanvas(NextGenModel.ImageData objImageData)
        {            
            //newUtilTrace.UtilTrace.SaveHistoryTraces();
            designerCanvas.UpdateEquipmentOnCanvas(objImageData);
        }

        //sss
        private void OnSetHeatExchangerPropertiesOnCanvas(NextGenModel.ImageData objImageData)
        {
            designerCanvas.UpdateEquipmentOnCanvas(objImageData);
            newUtilTrace.UtilTrace.SaveHistoryTraces();
        }
        private void OnSetOduPropertiesOnCanvas(NextGenModel.ImageData objImageData)
        {
            //newUtilTrace.UtilTrace.SaveHistoryTraces();
            designerCanvas.UpdateEquipmentOnCanvas(objImageData);
        }
        public void DrawPiping(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            try
            {
                CurrentSystem.IsInputLengthManually = false;
                CurrentSystem.MyPipingNodeOut = null;
                CurrentSystem.MyPipingNodeOutTemp = null;
                CurrentSystem.IsManualPiping = false;
                var IsValid = ReselectOutdoor(CurrentSystem,1);
                if (IsValid == true)
                {
                    _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                    designerCanvas.DoDrawingPiping(true, CurrentSystem, ref designerCanvas.addflow);
                    CurrentSystem.IsODUDirty = false;//BUG # 4695 : Set the IdODUDitry flag to false once auto-piping succeeds.
                    CurrentSystem.IsPipingOK = false;
                    newUtilTrace.UtilTrace.SaveHistoryTraces(CurrentSystem);
                }
            }
            catch (Exception ex) { }
        }

        public void DoPipingValidation(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            if (!CurrentSystem.IsManualPiping)
            {                
                 if (CurrentSystem.MyPipingNodeOut != null)
                {
                    designerCanvas.Validate(CurrentSystem, ref designerCanvas.addflow);
                    newUtilTrace.UtilTrace.SaveHistoryTraces(CurrentSystem);
                }                 
                else
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS"));
            }
            else
            {
                if (CurrentSystem.MyPipingOrphanNodes != null && CurrentSystem.MyPipingOrphanNodes.Count > 0)
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS"));
                    return;
                }

                string ErrMsg = string.Empty;
                this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
                bool IsValidSystemVRF = Utility.Validation.IsValidatedSystemVRF(this.projectLegacy, CurrentSystem, out ErrMsg);
                if (IsValidSystemVRF)
                {
                    var IsValid = ReselectOutdoor(CurrentSystem,0);
                    if (IsValid == true)
                    {      
                        designerCanvas.Validate(CurrentSystem, ref designerCanvas.addflow);
                        //CurrentSystem.IsPipingOK = true;
                        newUtilTrace.UtilTrace.SaveHistoryTraces();
                    }
                }
                else
                {
                    CurrentSystem.IsPipingOK = false;
                    JCHMessageBox.Show(ErrMsg);
                }
            }
        }



        private bool ReselectOutdoor(JCHVRF.Model.NextGen.SystemVRF CurrentSystem, int IsNodeUpdateRequired=1)
        {
            string ErrMsg = string.Empty;
            this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            bool IsValidDraw = Utility.Validation.IsValidatedSystemVRF(this.projectLegacy, CurrentSystem, out ErrMsg);
            try
            {
                //bug fix 3422
                SetCompositeMode(CurrentSystem, JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                //bug fix 3422
                NextGenModel.SystemVRF CurrVRF = new NextGenModel.SystemVRF();
                AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                if (IsValidDraw)
                {
                    AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSystem, JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                    if (result.SelectionResult == SelectOutdoorResult.OK)
                    {
                        if (IsNodeUpdateRequired == 1)
                        {
                            UpdatePipingNodeStructure(CurrentSystem);
                            UpdateWiringNodeStructure(CurrentSystem);
                        }
                        IsValidDraw = true;
                    }
                    else
                    {
                        IsValidDraw = false;
                        if (result.ERRList != null && result.ERRList.Count > 0)
                        {
                            _eventAggregator.GetEvent<ErrorLogVM>().Publish(result.ERRList);
                            // JCHMessageBox.Show("No suitable outdoor unit available\n", MessageType.Error);
                            JCHMessageBox.Show(GetErrorMassage(result.ERRList), MessageType.Error);
                        }
                        else if (result.MSGList != null && result.MSGList.Count > 0)
                        {
                            _eventAggregator.GetEvent<ErrorLogVM>().Publish(result.MSGList);
                            JCHMessageBox.Show(Langauge.Current.GetMessage("CAPCITYREQIREMENT"), MessageType.Error);
                        }
                        else
                            JCHMessageBox.Show(Langauge.Current.GetMessage("CAPCITYREQIREMENT"), MessageType.Error);
                    }
                }
                else
                {
                    IsValidDraw = false;
                    JCHMessageBox.Show(string.Format(ErrMsg));
                    ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, ErrMsg);
                }
            }
            catch (Exception ex)
            {

            }
            return IsValidDraw;

        }
        private void SetCompositeMode(JCHVRF.Model.NextGen.SystemVRF CurrentSys, List<RoomIndoor> RoomIndoorList)
        {
            if (CurrentSys != null)
            {
                if (RoomIndoorList != null && RoomIndoorList.Count > 0)
                {  
                    //if (RoomIndoorList.Count(mm => mm.IndoorItem.Flag == IndoorType.FreshAir) >= 1)
                    //{
                    //    //CurrentSys.SysType = SystemType.CompositeMode;
                    //}
                    //else
                    //{
                        CurrentSys.SysType = SystemType.OnlyIndoor;
                    //}
                }

            }
        }
        private string GetErrorMassage(List<string> StringData)
        {
            string ErrorReturn = string.Empty;
            if (StringData != null && StringData.Count > 0)
            {
                StringData.Distinct().ToList().ForEach((item) =>
                {
                    ErrorReturn += item + "\n";
                });

            }
            return ErrorReturn;

        }
        public void UpdatePipingNodeStructure(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            try
            {
                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                pipBll.CreatePipingNodeStructure(CurrentSystem);
                //newUtilTrace.UtilTrace.SaveHistoryTraces();
            }
            catch (Exception ex)
            {
                JCHMessageBox.Show("Error Occured : " + ex.Message);
            }
        }
        private NextGenBLL.PipingBLL GetPipingBLLInstance()
        {
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
                JCHMessageBox.Show("Error Occured : " + ex.Message);
            }
        }

        private void SystemTab_Loaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Subscribe(DrawPiping);
            _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Subscribe(DoPipingValidation);
            _eventAggregator.GetEvent<SetIduPropertiesOnCanvas>().Subscribe(OnSetIduPropertiesOnCanvas);
            _eventAggregator.GetEvent<FindLengthZeroBtnSubscriber>().Subscribe(OnFindZeroLengthClicked);
            _eventAggregator.GetEvent<FindZeroNextClick>().Subscribe(OnSetClosePopupFindZeroLength);
            _eventAggregator.GetEvent<AutoControlWiringSubcriber>().Subscribe(DrawAutoWiring);
            _eventAggregator.GetEvent<InputPipeLengthNextClick>().Subscribe(OnSetClosePopupInputPipeLengthOnNext);
            _eventAggregator.GetEvent<InputPipeLengthPreviousClick>().Subscribe(OnSetClosePopupInputPipeLengthOnPrevious);
            _eventAggregator.GetEvent<SystemDuplicateEvent>().Subscribe(DuplicateSystemEventHandler);
        }

        private void DuplicateSystemEventHandler(DuplicatedEventParams copyLengthParams)
        {
            if (((JCHVRF.Model.NextGen.SystemVRF)copyLengthParams.OldSystem).IsAuto)
                DrawPiping(copyLengthParams.NewSystem);


            try
            {
                CopyPipingLengthRecursively(copyLengthParams.OldSystem.MyPipingNodeOut, copyLengthParams.NewSystem.MyPipingNodeOut);
            }
            catch (Exception ex)
            {
                var wrappedException = new Exception("VRF System duplication, error copying piping contents", ex);
                var id = -1;
                Int32.TryParse(Project.CurrentSystemId, out id);
                Logger.LogProjectError(id, wrappedException, true);
            }

            _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish((JCHVRF.Model.NextGen.SystemVRF)copyLengthParams.NewSystem);

            if (copyLengthParams.OldSystem.IsInputLengthManually)
            {
                copyLengthParams.NewSystem.IsInputLengthManually = true;

                if (!NextGenBLL.PipingBLL.IsHeatRecovery(copyLengthParams.OldSystem))
                    OnFindZeroLengthClicked((JCHVRF.Model.NextGen.SystemVRF)copyLengthParams.NewSystem);
            }
            

            if (((JCHVRF.Model.NextGen.SystemVRF)copyLengthParams.OldSystem).IsPipingOK)
            {
                ((JCHVRF.Model.NextGen.SystemVRF)copyLengthParams.NewSystem).IsPipingOK = true;

                if (!NextGenBLL.PipingBLL.IsHeatRecovery(copyLengthParams.OldSystem))
                    _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Publish((JCHVRF.Model.NextGen.SystemVRF)copyLengthParams.NewSystem);
            }
                

        }

        private void CopyPipingLengthRecursively(NextGenModel.MyNodeOut src, NextGenModel.MyNodeOut dest) 
        {
            NextGenBLL.PipingBLL.CopyPipeLength(src, dest);
        }

        private void DrawAutoWiring(ControlSystem system)
        {
            try
            {
                var findgroup = Project.CurrentProject.ControlGroupList.Find(g => g.ControlSystemID == system.Id);
                if (findgroup != null)
                {
                    if (findgroup.IsValidGrp)
                    {
                        AutoControlWiring objAutoControlWiring =
                            new AutoControlWiring(true, Project.GetProjectInstance, system, designerCanvas.addflow);
                        system.IsAutoWiringPerformed = true;
                    }
                    else
                    {
                        JCHMessageBox.Show("The Group has some errors. Please Check!", MessageType.Error);
                    }

                    UtilTrace.SaveHistoryTraces();
                }
                else
                {
                    JCHMessageBox.Show("Cannot perform Auto-Control Wiring on Empty Group", MessageType.Warning);
                }
            }
            catch (Exception ex)
            { //ToDo
            }
        }

        private void SystemTab_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<AuToPipingBtnSubscriber>().Unsubscribe(DrawPiping);
            _eventAggregator.GetEvent<PipingValidationBtnSubscriber>().Unsubscribe(DoPipingValidation);
            _eventAggregator.GetEvent<FindLengthZeroBtnSubscriber>().Unsubscribe(OnFindZeroLengthClicked);
            _eventAggregator.GetEvent<FindZeroNextClick>().Unsubscribe(OnSetClosePopupFindZeroLength);
            _eventAggregator.GetEvent<AutoControlWiringSubcriber>().Unsubscribe(DrawAutoWiring);
            _eventAggregator.GetEvent<InputPipeLengthNextClick>().Unsubscribe(OnSetClosePopupInputPipeLengthOnNext);
            _eventAggregator.GetEvent<InputPipeLengthPreviousClick>().Unsubscribe(OnSetClosePopupInputPipeLengthOnPrevious);
            _eventAggregator.GetEvent<SetIduPropertiesOnCanvas>().Unsubscribe(OnSetIduPropertiesOnCanvas);
            _eventAggregator.GetEvent<SystemDuplicateEvent>().Unsubscribe(DuplicateSystemEventHandler);
        }

        private void OnFindZeroLengthClicked(NextGenModel.SystemVRF currentSystem)
        {
            try
            {
                if (!currentSystem.IsManualPiping)
                {
                    if (currentSystem.MyPipingNodeOut != null)
                    {
                        currentSystem.IsInputLengthManually = true;
                        designerCanvas.DoFindZeroLength();
                        UndoRedoSetup.SetInstanceNullWithInitalizedProjectStack();
                    }
                }
                else
                {
                    if ((currentSystem.MyPipingOrphanNodes != null && currentSystem.MyPipingOrphanNodes.Count > 0) || currentSystem.SystemStatus == SystemStatus.INVALID)
                    {
                        JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS"));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // To DO
            }
        }
        private void OnSetClosePopupFindZeroLength(List<string> pipingData)
        {
            designerCanvas.SetClosePopupFindZeroLength(pipingData);
            UndoRedoSetup.SetInstanceNullWithInitalizedProjectStack();

        }

        private void OnSetClosePopupInputPipeLengthOnNext(List<string> pipingData)
        {
            designerCanvas.SetClosePopupInputPipeLengthOnNext(pipingData);
            UndoRedoSetup.SetInstanceNullWithInitalizedProjectStack();
        }
        private void OnSetClosePopupInputPipeLengthOnPrevious(List<string> pipingData)
        {
            designerCanvas.SetClosePopupInputPipeLengthOnPrevious(pipingData);
            UndoRedoSetup.SetInstanceNullWithInitalizedProjectStack();
        }

        private void StackPanel_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            var _grabCursor = ((TextBlock)Resources["CursorGrab"]).Cursor;
            var _grabbingCursor = ((TextBlock)Resources["CursorGrabbing"]).Cursor;

            //if (e.Effects == DragDropEffects.Copy)
            //{
            e.UseDefaultCursors = false;
            Mouse.SetCursor(_grabbingCursor);
            //}
            //else
            //    e.UseDefaultCursors = true;

            e.Handled = true;
        }

        private void btnAllSYS_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = new SolidColorBrush(Colors.Blue);


            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);

            btnAllSYS.Foreground = new SolidColorBrush(Colors.White);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnHeatExchanger_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = new SolidColorBrush(Colors.Blue);
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);

            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.White);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);

        }

        private void btnVRF_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = new SolidColorBrush(Colors.Blue);
            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);

            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnAllCC_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.Blue);
            btnAllCC.Foreground = new SolidColorBrush(Colors.White);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnBACNET_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Background = new SolidColorBrush(Colors.Blue);
            btnBACNET.Foreground = new SolidColorBrush(Colors.White);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnNoBMS_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Background = new SolidColorBrush(Colors.Blue);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.White);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnModbus_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Background = new SolidColorBrush(Colors.Blue);
            btnModbus.Foreground = new SolidColorBrush(Colors.White);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnKNX_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Background = new SolidColorBrush(Colors.Blue);
            btnKNX.Foreground = new SolidColorBrush(Colors.White);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void btnLonWorks_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.Black);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Background = new SolidColorBrush(Colors.Blue);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.White);
        }


        public void SetFilterBackground()
        {
            var bc = new BrushConverter();
            btnAllSYS.Background = (Brush)bc.ConvertFrom("#0000FF");
            btnHeatExchanger.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnVRF.Background = (Brush)bc.ConvertFrom("#FFADD8E6");
            btnAllSYS.Foreground = new SolidColorBrush(Colors.White);
            btnHeatExchanger.Foreground = new SolidColorBrush(Colors.Black);
            btnVRF.Foreground = new SolidColorBrush(Colors.Black);

            btnAllCC.Background = new SolidColorBrush(Colors.White);
            btnAllCC.Foreground = new SolidColorBrush(Colors.Black);
            btnBACNET.Background = new SolidColorBrush(Colors.White);
            btnBACNET.Foreground = new SolidColorBrush(Colors.Black);
            btnNoBMS.Background = new SolidColorBrush(Colors.White);
            btnNoBMS.Foreground = new SolidColorBrush(Colors.Black);
            btnModbus.Background = new SolidColorBrush(Colors.White);
            btnModbus.Foreground = new SolidColorBrush(Colors.Black);
            btnKNX.Background = new SolidColorBrush(Colors.White);
            btnKNX.Foreground = new SolidColorBrush(Colors.Black);
            btnLonWorks.Background = new SolidColorBrush(Colors.White);
            btnLonWorks.Foreground = new SolidColorBrush(Colors.Black);
        }

    }

}
