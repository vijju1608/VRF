using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using JCHVRF.Model;
using JCHVRF_New.ViewModels;
using Prism.Events;
using Xceed.Wpf.Toolkit.Zoombox;
using Lassalle.WPF.Flow;

namespace JCHVRF_New.Common.Helpers
{
    class StringPayLoad1 : PubSubEvent<string>
    { }
    class RefreshDashboard : PubSubEvent
    { }
    class FloatProperties : PubSubEvent
    { }

    class BeforeCreate : PubSubEvent
    {

    }
    class BeforeSave : PubSubEvent
    {

    }

    class BeforeHESave : PubSubEvent
    {

    }

    class BeforeSaveVRF : PubSubEvent<JCHVRF.Model.NextGen.SystemVRF>
    {

    }

    class ErrorLogVMClear : PubSubEvent
    {

    }
    class ErrorLogVM : PubSubEvent<List<string>>
    {

    }
    class ErrorLogVMClearAll : PubSubEvent
    {

    }
    class ErrorMessageUC : PubSubEvent<string>
    {

    }
    class CleanupSystemWizard : PubSubEvent
    {

    }

    class Cleanup : PubSubEvent
    {

    }

    class CleanupHE : PubSubEvent
    {

    }
    class CleanupOnClose : PubSubEvent
    {

    }

    class SystemCreated : PubSubEvent<SystemBase>
    { }

    class GetCurrentSystem : PubSubEvent<SystemBase>
    { }

    class SystemSelectedTabSubscriber : PubSubEvent<SystemBase>
    { }

    class SystemTabClose : PubSubEvent<SystemBase>
    { }

    class SystemTypeCanvasSubscriber : PubSubEvent<SystemBase>
    { }

    class MasterDesignerUnload : PubSubEvent
    {

    }


    class OduIduVisibility : PubSubEvent<Visibility>
    { }

    class AddCreatorPayLoad : PubSubEvent
    { }
    class ProjectInfoSubscriber : PubSubEvent<bool>
    { }
    class DesignerTabSubscriber : PubSubEvent<bool>
    { }
    class TypeTabSubscriber : PubSubEvent<bool>
    { }
    class FloorListSaveSubscriber : PubSubEvent
    { }
    class CanvasEqupmentDeleteSubscriber : PubSubEvent<bool>
    { }
    class RoomListSaveSubscriber : PubSubEvent
    { }
    class FloorListAddSubscriber : PubSubEvent
    { }
    class FloorTabSubscriber : PubSubEvent<bool>
    { }

    class TheuInfoVisibility : PubSubEvent<Visibility>
    { }
    class cntrlTexthiding : PubSubEvent<Visibility>
    { }
    class OduTabSubscriber : PubSubEvent<bool>
    { }
    class IduTabSubscriber : PubSubEvent<bool>
    { }
    class ProjectTypeInfoTabNext : PubSubEvent
    { }
    class DesignConditionTabNext : PubSubEvent
    { }
    class TypeInfoTabNext : PubSubEvent
    { }
    class FloorTabNext : PubSubEvent
    { }
    class ODUTypeTabNext : PubSubEvent
    { }

    class ODUTypeTabSave : PubSubEvent
    { }
    class IDUTypeTabNext : PubSubEvent
    { }
    class CreateButtonVisibility : PubSubEvent<Visibility>
    { }
    class CreateButtonEnability : PubSubEvent<bool>
    { }
    class SaveButtonEnability : PubSubEvent<bool>
    { }

    class NextButtonVisibility : PubSubEvent<Visibility>
    { }
    class SaveButton_Visibility : PubSubEvent<Visibility>
    { }

    class SendOduName : PubSubEvent<string>
    { }

    class SendControllerDetails : PubSubEvent<SystemBase>
    { }

    class InSetController : PubSubEvent<CentralController>
    { }

    public class TabValidation
    {
        public bool IsProjectTypeTabValid { get; set; }

    }

    class ShowIduProperties : PubSubEvent<RoomIndoor>
    { }

    class SetHeatExchangerPropertiesOnCanvas : PubSubEvent<JCHVRF.Model.NextGen.ImageData>//sss
    { }
    class SendHEDetails : PubSubEvent<bool> //sss
    { }
    class DeleteEquipmentList : PubSubEvent<object> 
    { }

    class AddEquipmentList : PubSubEvent<object>
    { }
    
    class CleanEqupmentList : PubSubEvent
    { }
    class LoadAllSystem : PubSubEvent
    { }    
    class RefreshHECanvasProperty : PubSubEvent
    { }
    class SetPropertyOnCanvas : PubSubEvent<IDUPropertiesViewModel>
    { }

    class SetIduPropertiesOnCanvas : PubSubEvent<JCHVRF.Model.NextGen.ImageData>
    { }

    class SetOduPropertiesOnCanvas : PubSubEvent<JCHVRF.Model.NextGen.ImageData>
    { }
    #region Canvas Property Subscribers
    class CanvasBuildingImageSubscriber : PubSubEvent<string> { }
    class CanvasBuildingImageExistsSubscriber : PubSubEvent<BackgroundImageProperties> { }
    class CleanUpCanvaspropertyTab : PubSubEvent { }
    class SetResetPlottingScale : PubSubEvent<object> { }

    class CanvasLockBuildingImageSubscriber : PubSubEvent<bool> { }
    class CanvasBuildingImageOpacitySubscriber : PubSubEvent<int> { }
    class CanvasPlottingScaleEnableDisableSubscriber : PubSubEvent<bool> { }
    class CanvasPlottingScaleDirectionChangeSubscriber : PubSubEvent<bool> { }
    class CanvasPlottingScalingMeterValueSubscriber : PubSubEvent<int> { }
    class CanvasColorPropertyChangeSubscriber : PubSubEvent<CanvasColorPickerChanges> { }
    class ToolBarZoomInSubscriber : PubSubEvent { }
    class ToolBarZoomOutSubscriber : PubSubEvent { }
    class CanvasCenterStageEnableChangeSubscriber : PubSubEvent<bool> { }
    class ToolBarGridLineEnableChangeScubscriber : PubSubEvent { }
    class RefreshCanvas : PubSubEvent { }
    public class CanvasColorPickerChanges
    {
        public CanvasColorPickerChanges(CanvasColorPropertyEnum propertyEnum, Color? colorToUse)
        {
            PropertyEnum = propertyEnum;
            ColorToApply = colorToUse;
        }
        public CanvasColorPropertyEnum PropertyEnum { get; }
        public Color? ColorToApply { get; }
    }
    public enum CanvasColorPropertyEnum
    {
        PlottingScale,
        NodeText,
        NodeBackground,
        PipeColor,
        BranchKit
    }
    #endregion

    class SystemSelectedItemSubscriber : PubSubEvent<SystemBase> { }

    class CentralControllerCanvasMouseDownClickSubscriber : PubSubEvent<SystemBase> { }
    class HeatExchangerCanvasMouseDownClickSubscriber : PubSubEvent<SystemBase> { }


    class AuToPipingBtnSubscriber : PubSubEvent<JCHVRF.Model.NextGen.SystemVRF>
    { }
    class AutoControlWiringSubcriber : PubSubEvent<ControlSystem>{ }
    class FindLengthZeroBtnSubscriber : PubSubEvent<JCHVRF.Model.NextGen.SystemVRF>
    { }

    class PipingValidationBtnSubscriber : PubSubEvent<JCHVRF.Model.NextGen.SystemVRF>
    { }

    class SystemDetailsSubscriber : PubSubEvent<JCHVRF.Model.NextGen.SystemVRF>
    { }

    class SystemTabSubscriber : PubSubEvent<JCHVRF.Model.SystemBase>
    {
    }

    class FilterColor : PubSubEvent { }

    class HEFilterColor : PubSubEvent { }

    class VRFFilterColor : PubSubEvent { }

    class SystemExportSubscriber : PubSubEvent<JCHVRF.Model.SystemBase>
    {
    }

    class CanvasPropertiesSubscriber : PubSubEvent { }
    class ProjectSettingsSave : PubSubEvent { }

    class FindZeroNextClick : PubSubEvent<List<string>> { }
    class DisplayPipingLength : PubSubEvent { }
    class InputPipeLengthNextClick : PubSubEvent<List<string>> { }

    class InputPipeLengthPreviousClick : PubSubEvent<List<string>> { }
    class AddEventClickedDate : PubSubEvent<string> { }
    class EditEventEventId : PubSubEvent<int?> { }

    class GlobalPropertiesUpdated : PubSubEvent<string> { }
    class RefreshSystems : PubSubEvent { }
    
    class NavigatorZoomBoxSubscriber : PubSubEvent<FrameworkElement>{}
    class NavigatorViewFinderSubscriber : PubSubEvent<Zoombox>{}
    class NavigatorZoomValueChangeSubscriber : PubSubEvent<double> { }

    class ModalWindowClosed : PubSubEvent<string>
    { }
    class SetDefaultPipingInfo : PubSubEvent
    { }
    class SetPipingInfoSubscriber : PubSubEvent<JCHVRF.Model.NextGen.SystemVRF>
    { }

    class SystemDuplicateEvent : PubSubEvent<DuplicatedEventParams>
    {

    }
    class DuplicatedEventParams
    {   
        public AddFlow AddFlow { get; set; }
        public JCHVRF.Model.NextGen.SystemVRF NewSystem { get; set; }
        public JCHVRF.Model.NextGen.SystemVRF OldSystem { get; set; }
    }
    public class BackgroundImageProperties
    {
        public string imagePath;
        public int opacity;
        public bool ImageLock;
    }
    
}
