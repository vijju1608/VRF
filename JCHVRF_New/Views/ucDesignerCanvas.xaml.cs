using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JCBase.Utility;
using JCHVRF.BLL;
using JCHVRF.Const;
using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF.MyPipingBLL;
using JCHVRF.VRFMessage;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Model;
using JCHVRF_New.Utility;
using JCHVRF_New.ViewModels;
using log4net.Util.TypeConverters;
using Prism.Events;
using Prism.Regions;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Cursors = System.Windows.Input.Cursors;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using drawing = System.Drawing;
using MenuItem = System.Windows.Controls.MenuItem;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using NextGenBLL = JCHVRF.MyPipingBLL.NextGen;
using NextGenModel = JCHVRF.Model.NextGen;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using WL = Lassalle.WPF.Flow;
using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;
using WiringNodeCentralControl = JCHVRF.Model.WiringNodeCentralControl;
using Xceed.Wpf.Toolkit.Zoombox;
using JCHVRF_New.Common.Constants;
using Lassalle.WPF.Flow;
using JCHVRF_New.Common.Contracts;

namespace JCHVRF_New.Views
{
    /// <summary>
    /// Interaction logic for ucDesignerCanvas.xaml
    /// </summary>
    public partial class ucDesignerCanvas : UserControl
    {
        List<drawing.PointF[]> ptArrayList = new List<drawing.PointF[]>();
        List<drawing.PointF[]> ptArrayList_power = new List<drawing.PointF[]>();
        List<drawing.PointF[]> ptArrayList_ground = new List<drawing.PointF[]>();
        List<drawing.PointF[]> ptArrayList_mainpower = new List<drawing.PointF[]>();
        JCHNode nodeWiring, nodePiping;
        NextGenModel.ImageData btnImagePiping, btnImageWiring;
        private ucDesignerCanvasViewModel ucViewModel;
        bool bToggle;
        JCHVRF.DAL.IndoorDAL _dal;
        Point location;
        System.Windows.Size size;
        NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
        NextGenBLL.PipingBLL pipBll = null;
        string ut_length;
        string ut_power;
        string ut_temperature;
        string ut_airflow;

        string ut_weight;

        //WL.AddFlow addflow;
        //WL.AddFlow addCurrentContextflow;
        public UtilityWiring utilWiring = new UtilityWiring();
        private Model.PlottingScaleHandler plottingScaleHandler;
        private IEventAggregator _eventAggregator;
        AutoPiping AutoPipingObj = new AutoPiping();
        bool isWiringViewOpen = false;
        bool iscooling = true;
        bool iscancelcooling = true;
        WL.AddFlow TempAddFlowWiring = new WL.AddFlow();
        MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
        public List<string> TempControllerList = new List<string>();
        public ControlGroup _group;
        public ControlSystem controlsystem;
        public List<SystemsOnCanvas> SelectedSystems;
        WL.Link DeleteLink = null;
        public ContextMenu AddflowLinkDeleteContextMenu = new ContextMenu();
        WL.Node _tmpNode;
        int InitPageLoad = 0;
        private bool flgEvent;
        internal static string __errorMessage;
        Cursor _grabCursor;
        Cursor _grabbingCursor;
        private Item ItemSelect;
        public IGlobalProperties globalproperties { get; set; }

        static Lassalle.WPF.Flow.Item currentNode = null;

        private double leftC = 0;
        private double topC = 0;
        
        
        internal void Refresh(SystemBase oldSytem, SystemBase newSystem)
        {            
            bToggle = false;
            ToggleImage.Source =
                new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Wiring.png"));
            if (oldSytem != null)
            {
                //oldSytem.SystemState = Serial.GetSavedAddFlow(addflow);
                oldSytem.UnSavedDrawing = new List<WL.Item>(addflow.Items);
            }
            addflow.Clear();            
            _currentSystem = newSystem;
            
            var context = (ucDesignerCanvasViewModel) DataContext;
            context.CurrentSystem = _currentSystem;
            if (_currentSystem != null)
            {
                //if (!string.IsNullOrEmpty(_currentSystem.SystemState))
                //{
                //    Serial.LoadSavedAddFlow(addflow, _currentSystem.SystemState);
                //}
                //To Do
                if (_currentSystem.UnSavedDrawing != null)
                {
                    //foreach (var item in newSystem.UnSavedDrawing)
                    //{
                    //    if (typeof(WL.Caption).Equals(item.GetType()))
                    //    {
                    //        addflow.AddCaption((WL.Caption) item);
                    //    }

                    //    if (typeof(WL.Node).Equals(item.GetType()))
                    //    {
                    //        addflow.AddNode((WL.Node) item);
                    //    }

                    //    if (typeof(WL.Link).Equals(item.GetType()))
                    //    {
                    //        addflow.AddLink((WL.Link) item);
                    //    }
                    //}

                }

                if (newSystem.HvacSystemType.Equals("1"))
                {
                    //ScrollViewer.Height = 410;                    
                    HEScrollViewer.Visibility = Visibility.Collapsed;                    
                    addflow.Visibility = Visibility.Visible;
                    heatExchangerCanvas.Visibility = Visibility.Collapsed;
                    ToggleImage.Visibility = Visibility.Visible;
                    btnPanning.Visibility = Visibility.Visible;
                    btnZoomIn.Visibility = Visibility.Visible;
                    btnZoomOut.Visibility = Visibility.Visible;
                    btnReAlign.Visibility = Visibility.Visible;
                    addflow.Zoom = 1;
                    btnPanning.IsEnabled = true;
                    btnZoomIn.IsEnabled = true;
                    btnZoomOut.IsEnabled = true;
                    btnReAlign.IsEnabled = true;

                    brdToggleimg.Visibility = Visibility.Collapsed;
                    //brdParent.Background=Brushes.White;
                    //brdTabular.Background = Brushes.White;

                    string CanvasBackgroundImage = ((NextGenModel.SystemVRF)newSystem).AddFlowBackgroundImage;
                    int BackgroundImageOpacity = ((NextGenModel.SystemVRF)newSystem).BackgroundImageOpacity;

                    BackgroundImageProperties imageProperties = new BackgroundImageProperties();
                    imageProperties.imagePath = CanvasBackgroundImage;
                    imageProperties.opacity = BackgroundImageOpacity;

                    _eventAggregator.GetEvent<CanvasBuildingImageExistsSubscriber>().Publish(imageProperties);

                    CurrentSystem = _currentSystem as NextGenModel.SystemVRF;
                    if (CurrentSystem != null && CurrentSystem.IsManualPiping)
                    {
                        {
                            InitPageLoad = 1;
                            ReDrawManualPiping(CurrentSystem, addflow);
                            InitPageLoad = 0;
                        }
                    }
                    else
                    {
                        GetDefaultAccessoryForIDU();
                        DrawPreSelectedNodesOnEdit(ref addflow, (NextGenModel.SystemVRF) newSystem);
                    }

                    if (!string.IsNullOrEmpty(CanvasBackgroundImage))
                        AddBuildingImage(CanvasBackgroundImage);
                    //Added to retain the value of plotting scale in canvas property
                    var plottingNode = addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().FirstOrDefault();

                    _eventAggregator.GetEvent<SetResetPlottingScale>().Publish(plottingNode);

                    this.ScrollViewer.Content = addflow;
                }

                else if (newSystem.HvacSystemType.Equals("6"))
                {
                    _eventAggregator.GetEvent<GetCurrentSystem>().Subscribe(GetCurrentSystem);
                    ScrollViewer.Height = 410;
                    HEScrollViewer.Visibility = Visibility.Collapsed;                    
                    addflow.Visibility = Visibility.Visible;                    
                    heatExchangerCanvas.Visibility = Visibility.Collapsed;
                    ToggleImage.Visibility = Visibility.Visible;
                    btnPanning.Visibility = Visibility.Visible;
                    btnZoomIn.Visibility = Visibility.Visible;
                    btnZoomOut.Visibility = Visibility.Visible;
                    btnReAlign.Visibility = Visibility.Visible;
                    btnPanning.IsEnabled = true;
                    btnZoomIn.IsEnabled = true;
                    btnZoomOut.IsEnabled = true;
                    btnReAlign.IsEnabled = true;
                    brdToggleimg.Visibility = Visibility.Collapsed;
                    addflow.Zoom = 1;
                    ucViewModel = (ucDesignerCanvasViewModel) this.DataContext;
                    ControlSystem cc = _currentSystem as ControlSystem;
                    DrawControllerNodesonRefresh(ref addflow, cc);
                    // DrawControllerNodesonRefresh(ref addflow, (ControlSystem) newSystem);
                    this.ScrollViewer.Content = addflow;
                    _eventAggregator.GetEvent<SendControllerDetails>().Publish(newSystem);
                    _eventAggregator.GetEvent<LoadAllSystem>().Publish();
                    
                }
                else
                {                    
                    HEScrollViewer.Visibility = Visibility.Visible;                                     
                    heatExchangerCanvas.Visibility = Visibility.Visible;
                    ToggleImage.Visibility = Visibility.Collapsed;
                    btnPanning.Visibility = Visibility.Collapsed;
                    btnZoomIn.Visibility = Visibility.Collapsed;
                    btnZoomOut.Visibility = Visibility.Collapsed;
                    btnReAlign.Visibility = Visibility.Collapsed;
                    heatExchangerCanvas.SetHvacSystem(newSystem);
                    brdToggleimg.Visibility = Visibility.Collapsed;
                    //brdParent.Background = Brushes.White;
                    //brdTabular.Background = Brushes.White;
                }
            }
        }

        private void GetDefaultAccessoryForIDU()
        {
            List<RoomIndoor> defaultAccessory = new List<RoomIndoor>();
            if (Project.CurrentProject.RoomIndoorList != null && CurrentSystem != null)
                defaultAccessory = (from idu in Project.CurrentProject.RoomIndoorList where CurrentSystem.Id == idu.SystemID select idu).ToList();


            if (defaultAccessory != null)
            {
                if (defaultAccessory.Count > 0)
                {
                    foreach (var indoor in defaultAccessory)
                    {
                        if (indoor.ListAccessory == null)
                            indoor.ListAccessory = new List<Accessory>();
                        if (globalproperties != null)
                        {
                            if (globalproperties.DefaultAccessoryDictionary != null)
                            {
                                if ((!globalproperties.DefaultAccessoryDictionary.ContainsKey(Project.CurrentProject.projectID) || !(globalproperties.DefaultAccessoryDictionary[Project.CurrentProject.projectID].Contains(indoor.IndoorNO)))
                                    && indoor.ListAccessory.Count == 0)
                                    indoor.ListAccessory = indoor.IndoorItem.ListAccessory;
                            }

                        }
                    }
                }
            }
        }


        private void ReDrawManualPiping(NextGenModel.SystemVRF newSystem, WL.AddFlow addflow)
        {
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            string dir = GetImagePathPiping();
            if (newSystem != null && newSystem.MyPipingNodeOutTemp != null)
            {
                pipBll.LoadPipingNodeStructure(newSystem);
                pipBll.LoadPipingOrphanNodes(newSystem);
                pipBll.DrawManualPipingNodes(newSystem, dir, ref addflow);
                pipBll.DrawOrphanIndoorNodes(newSystem, ref addflow);
                SetManualPipingModelOnAddFlow(ref addflow);
                addflow.LinkCreating -= Addflow_LinkCreating;
                addflow.LinkCreated -= Addflow_LinkCreated;
                pipBll.DrawManualLinkForNormal(newSystem.MyPipingNodeOut, newSystem.MyPipingNodeOut.ChildNode, true,
                    CurrentSystem, ref addflow);
               pipBll.DrawLegendText(CurrentSystem, ref addflow);
                if(CurrentSystem.IsPipingOK==true)
                Validate(CurrentSystem, ref addflow);
                _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(newSystem);
                addflow.LinkCreating += Addflow_LinkCreating;
                addflow.LinkCreated += Addflow_LinkCreated;
            }
        }

        private void addOrphanNode()
        {
            if (CurrentSystem == null || !CurrentSystem.IsManualPiping) return;
            NextGenModel.MyNodeOut tempNodOut = null;
            foreach (var item in addflow.Items)
            {
                if (item is NextGenModel.MyNodeOut)
                {
                    tempNodOut = item as NextGenModel.MyNodeOut;
                    CurrentSystem.MyPipingNodeOut = tempNodOut;
                }
                if (item is JCHNode)
                {
                    var orphanIndoor = item as JCHNode;
                    if (orphanIndoor.IsIndoorUnit() && !orphanIndoor.Links.Any())
                    {
                        AddOrphanNode(orphanIndoor);
                    }                  
                }
            }
        }
        private void SaveManualPiping()
        {
            if (CurrentSystem == null || !CurrentSystem.IsManualPiping) return;         
            foreach (var link in addflow.Items.OfType<WL.Link>())
            {
                //var linkDst = link.Dst;
                //var linkOrg = link.Org;
                //if (linkOrg is NextGenModel.MyNodeOut)
                //{
                //    (linkOrg as NextGenModel.MyNodeOut).ChildNode = link.Dst;
                //}
                //if (linkDst is NextGenModel.MyNodeOut)
                //{
                //    (linkDst as NextGenModel.MyNodeOut).ChildNode = link.Org;
                //}
                SetMyLinkValues(link);               
            }
            if (CurrentSystem != null)
            {
                
                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                pipBll.SavePipingStructure(CurrentSystem);               
                pipBll.SavePipingOrphanNodes(CurrentSystem);
            }
        }

        
        private void SetMyLinkValues(WL.Link link)
        {
           
            var linkOrg = link.Org;
            var linkDst = link.Dst;
            if (linkDst is NextGenModel.MyNodeYP)
            {
                //if (link.PinDstIndex == 0)
                {
                    AddMyInLinkForNode(linkDst as NextGenModel.MyNode, link);
                }
            }
            //if (linkOrg is NextGenModel.MyNodeYP)
            //{
            //    //if (link.PinOrgIndex == 0)
            //    {
            //        AddMyInLinkForNode(linkOrg as NextGenModel.MyNode, link);
            //    }
            //}
            if (linkDst is NextGenModel.MyNodeIn)
            {
                AddMyInLinkForNode(linkDst as NextGenModel.MyNode, link);
            }
            //if (linkOrg is NextGenModel.MyNodeIn)
            //{
            //    AddMyInLinkForNode(linkOrg as NextGenModel.MyNode, link);
            //}
        }
        private void AddMyInLinkForNode(NextGenModel.MyNode myNode, WL.Link link)
        {
            if (myNode != null)
            {
                myNode.MyInLinks.Clear();
                var mylink = new NextGenModel.MyLink(link.Org, link.Dst, "", addflow);
                //mylink.IsAdjustDst = true;
                //mylink.IsAdjustOrg = true;
                for (int i = 0; i < link.Points.Count; i++)
                {
                    mylink.Points.Insert(i, link.Points[i]);
                }
               mylink.LineStyle = link.LineStyle;
               mylink.PinDestinationIndex = link.PinDstIndex;
               mylink.PinOriginIndex = link.PinOrgIndex;
               mylink.PinDstIndex = link.PinDstIndex;
               mylink.PinOrgIndex = link.PinOrgIndex;
               myNode.MyInLinks.Add(mylink);
            }
        }

        public event EventHandler<Object> SelectEquipment;

        protected virtual void OnSelectEquipment(Object data)
        {
            if (SelectEquipment != null)
            {
                SelectEquipment(this, data);
            }
        }

        public event EventHandler<NextGenModel.ImageData> AddEquipment;

        protected virtual void OnAddEquipment(NextGenModel.ImageData data)
        {
            if (SelectEquipment != null)
            {
                AddEquipment(this, data);
            }
        }

        public JCHVRF.Model.Project projectLegacy { get; set; }
        public NextGenModel.SystemVRF CurrentSystem { get; private set; }

        //start for context menu canvas
        public ContextMenu AddflowContextMenu = new ContextMenu();
        JCHNode DeleteNode = null;

        JCHNode selNode = null;
        //end for context menu canvas

        private void InitializeWiringAddFlow()
        {
            //addCurrentContextflow = new WL.AddFlow();
            //addCurrentContextflow.AllowDrop = false;
            //addCurrentContextflow.CanDrawNode = false;
            //addCurrentContextflow.MouseLeftButtonDown += AddWiringflow_MouseLeftButtonDown;           
        }

        private void SaveAddFlow()
        {
            WorkFlowContext.CurrentSystem.SystemState = "New";
        }

        IRegionManager regionManager;

        public void IntializeContextMenu()
        {
            MenuItem DeleteMenuItem = new MenuItem();
            DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
            DeleteMenuItem.Click += DeleteMenuItem_Click;
            AddflowContextMenu.Items.Add(DeleteMenuItem);

            //KeyEventArgs DeleteKey = new KeyEventArgs();
            //if(DeleteKey.Equals(Key.Delete))
            //{
            //    DeleteKey. += DeleteMenuItem_Click;
            //}

            MenuItem DeleteLinkMenu = new MenuItem();
            DeleteLinkMenu.Header = "Delete Link";
            DeleteLinkMenu.Click += DeleteLinkMenu_Click; ;
            AddflowLinkDeleteContextMenu.Items.Add(DeleteLinkMenu);
        }

        private void DeleteLinkMenu_Click(object sender, RoutedEventArgs e)
        {
         
                if (DeleteLink != null)
                {
                    addflow.RemoveLink(DeleteLink);

                    if (DeleteLink.Dst != null)
                    {
                        var nodeIn = DeleteLink.Dst as NextGenModel.MyNodeIn;
                        if (nodeIn != null)
                        {
                                ((NextGenModel.MyNodeIn)nodeIn).MyInLinks.Clear();
                        }
                    }
                }

            
            bool errorState = IsDiagramInErrorState();
            if (errorState)
            {
                ShowErrorNotification();
            }
            else
            {
                ClearErrorNotification();
            }
        }
        private bool IsDiagramInErrorState()
        {
            var anyOrphanNode = addflow.Items.OfType<JCHNode>()
                .Any(x => (x.IsIndoorUnit() || x.IsOutdoorUnit()) && !x.Links.Any());
            var anyOrphanYp = addflow.Items.OfType<NextGenModel.MyNodeYP>()
                .Any(x => x.Links.Count < 3);
            var errorState = anyOrphanNode || anyOrphanYp;
            foreach (var link in addflow.Items.OfType<WL.Link>())
            {
                var org = link.Org as JCHNode;
                var dst = link.Dst as JCHNode;
                if (org == null || dst == null)
                {
                    continue;
                }
                if (org.IsIndoorUnit() && dst.IsIndoorUnit())
                {
                    link.Stroke = GetBrushFromCode("#D2103E");
                    errorState = true;
                }
            }
            return errorState;
        }
        private void DeleteLinkProperties(WL.Link link)
        {
            if (link.Org is NextGenModel.MyNodeYP)
            {
                var org = (NextGenModel.MyNodeYP)link.Org;
                string dstUniqueName = string.Empty ;
                if ((link.Dst as JCHNode)!=null && (link.Dst as JCHNode).ImageData != null && (link.Dst as JCHNode).ImageData.UniqName!=null)
                {
                    dstUniqueName = (link.Dst as JCHNode).ImageData.UniqName;
                }
                   
              
                org.RemoveChildNode(link.Dst);
                DeleteYpConnectionProperties(org, dstUniqueName);
            }
            if (link.Dst is NextGenModel.MyNodeYP)
            {
                var dst = (NextGenModel.MyNodeYP)link.Dst;
                dst.RemoveChildNode(link.Org);
                var dstUniqueName = (link.Org as JCHNode).ImageData.UniqName;
                DeleteYpConnectionProperties(dst, dstUniqueName);
            }           
            CheckForErrorState();
        }
        private static void DeleteYpConnectionProperties(NextGenModel.MyNodeYP org, string dstUniqueName)
        {
            if (!string.IsNullOrEmpty(NextGenModel.MyNodeYP.GetRightOutletId(org))
                                && NextGenModel.MyNodeYP.GetRightOutletId(org).Equals(dstUniqueName))
            {
                NextGenModel.MyNodeYP.SetRightOutletId(org, string.Empty);
            }
            if (!string.IsNullOrEmpty(NextGenModel.MyNodeYP.GetInLetId(org))
                && NextGenModel.MyNodeYP.GetInLetId(org).Equals(dstUniqueName))
            {
                NextGenModel.MyNodeYP.SetInLetId(org, string.Empty);
            }
            if (!string.IsNullOrEmpty(NextGenModel.MyNodeYP.GetBottomOutletId(org))
                && NextGenModel.MyNodeYP.GetBottomOutletId(org).Equals(dstUniqueName))
            {
                NextGenModel.MyNodeYP.SetBottomOutletId(org, string.Empty);
            }
        }
        public ucDesignerCanvas()
        {
            //addflow = new WL.AddFlow();
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(__errorMessage))
            {
                this.Snake.IsActive = true;
                tbError.Text = __errorMessage;

            }
            else
            {
                this.Snake.IsActive = false;
                tbError.Text = string.Empty;

            }
            _dal = new JCHVRF.DAL.IndoorDAL(JCHVRF.Model.Project.CurrentProject.SubRegionCode,
                JCHVRF.Model.Project.CurrentProject.RegionCode, JCHVRF.Model.Project.CurrentProject.BrandCode);

            InitializeWiringAddFlow();
            InitializePlottingScaleHandler();

            // start for context menu canvas
            IntializeContextMenu();
            //  end for context menu canvas
            if(!flgEvent)
            {
                _eventAggregator = ((ucDesignerCanvasViewModel)DataContext)._eventAggregator;
                _eventAggregator.GetEvent<ErrorMessageUC>().Subscribe(ShowErrorMessage);
                _eventAggregator.GetEvent<DisplayPipingLength>().Subscribe(DisplayLenPiping);
                flgEvent = true;
            }

            _eventAggregator = ((ucDesignerCanvasViewModel) DataContext)._eventAggregator;

            this.globalproperties = ((ucDesignerCanvasViewModel)DataContext).globalProperties;

            DragDrop.AddPreviewDragEnterHandler(addflow, AddFlowPreviewDragEnterHandler);
            DragDrop.AddPreviewDragOverHandler(addflow, AddFlowPreviewDragOverHandler);
            DragDrop.AddPreviewDropHandler(addflow, AddFlowPreviewDropHandler);
            DragDrop.AddPreviewDragLeaveHandler(addflow, AddFlowPreviewDragLeaveHandler);

            _eventAggregator.GetEvent<InSetController>().Subscribe(DrawControllerInSetNode);

        }

        private void InitializePlottingScaleHandler()
        {
            plottingScaleHandler = new Model.PlottingScaleHandler(addflow);
        }

        #region Canvas Properties Actions

        private JCHNode nodeBuildingImage = null;
        private JCHNode originalBuildingImage = null;
        NextGenModel.MyNodePlottingScale nodePlottingScale = null;
        private SystemBase _currentSystem;

        /// <summary>
        /// Clear the existing Building Background image
        /// </summary>
        private void ClearBuildingImage()
        {
            if (addflow.Items.Count > 0)
            {
                var jchnodes = addflow.Items.OfType<JCHNode>().ToArray();
                foreach (var node in jchnodes)
                {
                    if (node.ImageData != null)
                        if (node.ImageData.equipmentType == "BackgroundImage")
                        {
                            addflow.RemoveNode(node);
                            var sysItem = GetCurrentSystem();
                            sysItem.AddFlowBackgroundImage = string.Empty;                            
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Change Building Image Opacity
        /// </summary>
        /// <param name="opacity"></param>
        private void SetBuildingImageOpacity(int opacity)
        {
            if (nodeBuildingImage != null && opacity > 0)
            {
                nodeBuildingImage.Visual.Opacity = opacity * 0.01;
                var sysItem = GetCurrentSystem();
                sysItem.BackgroundImageOpacity = opacity;
            }
        }

        /// <summary>
        /// Add New Building Image Node
        /// </summary>
        /// <param name="imagePath"></param>
        private void AddBuildingImage(string imagePath)
        {

            #region Without pipingBLL Logic
            if (!String.IsNullOrEmpty(imagePath))
            {
                ClearBuildingImage();
                var bitmap = new drawing.Bitmap(imagePath);
                var bitmapImage = BitmapToImageSource(bitmap);
                var imageSizeUsed = new System.Windows.Size(addflow.ExtentWidth, addflow.ExtentHeight);
                imageSizeUsed.Width = (imageSizeUsed.Width == 0) || (double.IsNaN(imageSizeUsed.Width)) ? bitmapImage.Width : imageSizeUsed.Width;
                imageSizeUsed.Height = (imageSizeUsed.Height == 0) || (double.IsNaN(imageSizeUsed.Height)) ? bitmapImage.Height : imageSizeUsed.Height;

                nodeBuildingImage = new JCHNode(0, 0, imageSizeUsed.Width, imageSizeUsed.Height, null, (WL.AddFlow)null)
                {
                    ImagePosition = WL.ImagePosition.LeftTop,
                    Image = bitmapImage,
                    IsImageSizeFitContentArea = true,
                    BackMode = WL.BackMode.Transparent,
                    Geometry = new RectangleGeometry(new Rect(0, 0, imageSizeUsed.Width, imageSizeUsed.Height), 3, 3)
                };

                nodeBuildingImage.ImagePosition = WL.ImagePosition.LeftBottom;
                nodeBuildingImage.BackMode = WL.BackMode.Opaque;
                nodeBuildingImage.IsEditable = false;
                nodeBuildingImage.IsRotatable = false;
                nodeBuildingImage.IsSelectable = false;
                //addflow.AddNode(nodeBuildingImage);
                var imagedata = new ImageData();
                imagedata.equipmentType = "BackgroundImage";
                nodeBuildingImage.ImageData = imagedata;
                if (!addflow.SelectedItems.Contains(nodeBuildingImage))
                {
                    addflow.SelectedItems.Add(nodeBuildingImage);
                    addflow.AddNode(nodeBuildingImage);
                    addflow.SendToBack();
                    addflow.SelectedItems.Clear();
                    var sysItem = GetCurrentSystem();
                    sysItem.AddFlowBackgroundImage = imagePath;
                    if (sysItem.BackgroundImageOpacity > 0)
                        nodeBuildingImage.Visual.Opacity = sysItem.BackgroundImageOpacity * 0.01;
                    //sysItem.BackgroundImageOpacity = 
                    //sysItem.MyPipingBuildingImageNodeTemp = new NextGenModel.tmpNodeBgImage();

                    //sysItem.MyPipingBuildingImageNodeTemp.Image = bitmap;
                    //GetPipingBLLInstance().SavePipingBuildingImageNode(sysItem, nodeBuildingImage, bitmap);
                }
            }
            #endregion
        }

        /// <summary>
        /// Lock building image
        /// </summary>
        /// <param name="locked"></param>
        private void LockBuildingImageNode(bool locked)
        {
            if (nodeBuildingImage == null)
            {
                return;
            }

            nodeBuildingImage.IsSelectable = !locked;
            nodeBuildingImage.IsXMoveable = !locked;
            nodeBuildingImage.IsYMoveable = !locked;
            nodeBuildingImage.IsXSizeable = !locked;
            nodeBuildingImage.IsYSizeable = !locked;
            if (!locked)
            {
                addflow.SelectedItems.Clear();
            }

            nodeBuildingImage.IsSelected = !locked;
        }

        private void OnCanvasZoomInClick()
        {
            AddFlowZoomIn(addflow);
        }

        private void OnCanvasZoomOutClick()
        {
            AddFlowZoomOut(addflow);
        }

        private void AddFlowZoomIn(WL.AddFlow addFlow)
        {
            var zoomaddflow = (WL.AddFlow) this.ScrollViewer.Content;
            if (zoomaddflow.Zoom < 3)
            {
                zoomaddflow.Zoom = zoomaddflow.Zoom + 0.3;
            }
        }

        private void AddFlowZoomOut(WL.AddFlow addFlow)
        {
            var zoomaddflow = (WL.AddFlow) this.ScrollViewer.Content;
            if (zoomaddflow.Zoom > 0.3)
            {
                zoomaddflow.Zoom = zoomaddflow.Zoom - 0.3;
            }
        }

        private void OnCanvasCenterStageChange(bool enableZoom)
        {
            if (_currentSystem.HvacSystemType == "1")
            {
                //Point location = addflow.Origin;
                //Size size = addflow.Extent;
                //addflow.ZoomRectangle(new Rect(location.X, location.Y, size.Width, size.Height));

                var centreAddFlow = (WL.AddFlow)this.ScrollViewer.Content;
                Point location = centreAddFlow.Origin;
                Size size = centreAddFlow.Extent;
                centreAddFlow.ZoomRectangle(new Rect(location.X, location.Y, size.Width, size.Height));
            }
            else if (_currentSystem.HvacSystemType == "6")
            {
                if (bToggle == false)
                {
                    Point location = addflow.Origin;
                    Size size = addflow.Extent;
                    addflow.ZoomRectangle(new Rect(location.X, location.Y, size.Width, size.Height));
                }

                if (bToggle == true)
                {
                    Point location1 = TempAddFlowWiring.Origin;
                    Size size2 = TempAddFlowWiring.Extent;
                    TempAddFlowWiring.ZoomRectangle(new Rect(location1.X, location1.Y, size2.Width, size2.Height));
                }
            }
        }

        private void OnToolBarGridLinesEnableChange()
        {
            addflow.GridDraw = !addflow.GridDraw;
        }

        private void OnNavigatorZoomChange(double zoom)
        {
            if (Math.Abs(zoom - 50) < 0.8)
            {
                OnCanvasCenterStageChange(true);
                return;
            }

            var zoomaddflow = (WL.AddFlow) this.ScrollViewer.Content;
            if (zoomaddflow.Zoom < 3 && zoomaddflow.Zoom > 0.3)
            {
                zoomaddflow.Zoom = zoom / 50;
            }
        }

        private void OnNavigatorControlLoad(FrameworkElement element)
        {
            //ZoomBoxCtrl.ViewFinder = element;
            //ZoomBoxCtrl.ViewFinder.Visibility = Visibility.Visible;
            //ZoomBoxCtrl.FillToBounds();
            //ZoomBoxCtrl.MinScale = 0.1;
            //ZoomBoxCtrl.MaxScale = 50;
            //ZoomBoxCtrl.ZoomOn = Xceed.Wpf.Toolkit.Zoombox.ZoomboxZoomOn.Content;
        }

        /// <summary>
        /// Apply Color related changes from Canvas
        /// </summary>
        private void OnCanvasColorPropertiesChange(CanvasColorPickerChanges colorChange)
        {
            if (colorChange.PropertyEnum == CanvasColorPropertyEnum.PlottingScale)
            {
                var plottinNode = addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().FirstOrDefault();
                if (plottinNode != null)
                {
                    plottinNode.PlottingScaleColor = new SolidColorBrush(colorChange.ColorToApply.Value);
                    GetPipingBLLInstance().DrawPlottingScaleNode(GetCurrentSystem(), plottinNode, ref addflow);
                }
            }
            else
            {
                if(colorChange.PropertyEnum==CanvasColorPropertyEnum.NodeBackground)
                    nodeBackgroundColor = colorChange.ColorToApply == null ? Colors.White : colorChange.ColorToApply.Value;
                if(colorChange.PropertyEnum==CanvasColorPropertyEnum.PipeColor)
                    pipeColor= colorChange.ColorToApply == null ? Colors.White : colorChange.ColorToApply.Value;
                plottingScaleHandler.ApplyColorChanges(colorChange);
            }
        }

        private NextGenModel.SystemVRF GetCurrentSystem()
        {            
            if (WorkFlowContext.CurrentSystem is NextGenModel.SystemVRF)
                CurrentSystem = WorkFlowContext.CurrentSystem as NextGenModel.SystemVRF;
            
            return CurrentSystem;
        }

        /// <summary>
        /// Change Plotting Scale Meter
        /// </summary>
        /// <param name="meterValue"></param>
        private void OnPlottingScaleMeterValueChange(int meterValue)
        {
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();

            CurrentSystem.IsInputLengthManually = true;
            jctxtPlottingScale_TextChanged(meterValue);

            var plottingNode = addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().FirstOrDefault();
            if (plottingNode != null)
            {
                plottingNode.ActualLength = meterValue;
                plottingNode.ActualLengthString =
                    meterValue.ToString() + " " +
                    (string.IsNullOrEmpty(SystemSetting.UserSetting.unitsSetting.settingLENGTH)
                        ? "m"
                        : SystemSetting.UserSetting.unitsSetting.settingLENGTH);
                var node = GetPipingBLLInstance().DrawPlottingScaleNode(GetCurrentSystem(), plottingNode, ref addflow);
            }

           // pipBll.CalculatePipeLengthByPlottingScale(CurrentSystem, plottingNode.PlottingScale, plottingNode, plottingNode.ParentNode);

            // To change the status of CurrentSystem to wip
            CurrentSystem.IsPipingOK = false;
        }

        /// <summary>
        /// Change Plotting Scale Direction
        /// </summary>
        private void OnPlottingScaleDirectionChange(bool isVerticalPlotting)
        {
            var plottingNode = addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().FirstOrDefault();
            if (plottingNode != null)
            {
                plottingNode.IsVertical = isVerticalPlotting;
                GetPipingBLLInstance().DrawPlottingScaleNode(GetCurrentSystem(), plottingNode, ref addflow, true);
            }
        }

        NextGenModel.MyNodePlottingScale plottingScaleNode;

        /// <summary>
        /// Enable or Disable Plotting Selection
        /// </summary>
        private void OnPlottingScaleSelectionChange(bool enablePlottingScale)
        {
            if (enablePlottingScale)
            {
                if (!addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().Any())
                {
                    var system = GetCurrentSystem();
                    var plottingNode = GetPipingBLLInstance().DrawPlottingScaleNode(system, null, ref addflow);
                }
            }
            else
            {
                RemovePlottingScalNode();
            }
        }

        private void RemovePlottingScalNode()
        {
            var plottingNode = addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().FirstOrDefault();
            if (addflow.Items.Contains(plottingNode))
            {
                addflow.RemoveNode(plottingNode);
                var sysItem = GetCurrentSystem();
                if (sysItem != null)
                {
                    sysItem.MyPipingPlottingScaleNodeTemp = null;
                }
            }
        }

        private void jctxtPlottingScale_TextChanged(int plotscalevalue)
        {
            double len = 0;
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            var nodePlottingScale = addflow.Items.OfType<NextGenModel.MyNodePlottingScale>().FirstOrDefault();
            if (plotscalevalue == 0)
            {
                nodePlottingScale.Text = "";
                nodePlottingScale.ActualLength = 0;
            }
            else
            {
                double.TryParse(plotscalevalue.ToString(), out len);
                nodePlottingScale.ActualLengthString = len.ToString("0.##") + " " + SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                nodePlottingScale.ActualLength = Unit.ConvertToSource(len, UnitType.LENGTH_M, SystemSetting.UserSetting.unitsSetting.settingLENGTH);
                pipBll.DrawPlottingScaleNode(GetCurrentSystem(),nodePlottingScale, ref addflow );
            }
            if (nodePlottingScale != null)
            {
                pipBll.CalculateAllPipeLengthByPlottingScale(GetCurrentSystem(), nodePlottingScale.PlottingScale);
                //UndoRedoUtil?.SaveProjectHistory();
            }
        }

        /// <summary>
        /// Lock/UnLock Building Image Change
        /// </summary>
        private void OnBuildingImageLockChecked(bool isLockChecked)
        {
            LockBuildingImageNode(isLockChecked);
        }

        /// <summary>
        /// Change Building Node Image Opacity
        /// </summary>
        private void OnBuildingImageOpacityChange(int opacity)
        {
            SetBuildingImageOpacity(opacity);
        }

        /// <summary>
        /// Import and Add Building Node Image
        /// </summary>
        private void OnImageImportClicked(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                AddBuildingImage(imagePath);
            }
            else
            {
                ClearBuildingImage();
            }
        }

        /// <summary>
        /// Converts Bitmap to ImageSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static BitmapImage BitmapToImageSource(drawing.Bitmap bitmap)
        {
            var bitmapimage = new BitmapImage();
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
            }

            return bitmapimage;
        }

        #endregion

        public void AddFlowPreviewDragEnterHandler(object s, DragEventArgs a)
        {
            _tmpNode = new WL.Node();
            _tmpNode.Size = new Size(100, 50);
            _tmpNode.IsRotatable = false;
            _tmpNode.IsSelectable = false;
            var img = new System.Windows.Controls.Image()
            {
                Source = new BitmapImage(new Uri(a.Data.GetData(typeof(ImageData)).GetValue<string>("imagePath")))
            };
            _tmpNode.Image = img.Source;
            _tmpNode.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
            _tmpNode.DashStyle = DashStyles.DashDot;

            bool isIntersecting = false;
            _tmpNode.Fill = System.Windows.Media.Brushes.White;
            //foreach (var item in addflow.Items)
            //{
            //    var node = item as JCHNode;

            //    if (node != null)
            //    {
            //        var r1 = new Rect(node.Location.X, node.Location.Y, node.Size.Width, node.Size.Height);
            //        var r2 = new Rect(_tmpNode.Location.X, _tmpNode.Location.Y, _tmpNode.Size.Width, _tmpNode.Size.Height);

            //        if (r2.IntersectsWith(r1))
            //            isIntersecting = true;
            //    }
            //}
            
            //if (isIntersecting)
            //    _tmpNode.Fill = System.Windows.Media.Brushes.Red;
            //else
            //    _tmpNode.Fill = System.Windows.Media.Brushes.White;

            _tmpNode.Stroke = System.Windows.Media.Brushes.Gray;
            _tmpNode.StrokeThickness = 1;
            _tmpNode.ImageMargin = new Thickness(3);
            _tmpNode.TextMargin = new Thickness(3);
            _tmpNode.TextPosition = WL.TextPosition.CenterBottom;
            _tmpNode.ImagePosition = WL.ImagePosition.CenterTop;
            _tmpNode.Text = a.Data.GetData(typeof(ImageData)).GetValue<string>("imageName");
            System.Windows.Size nodeSize = new System.Windows.Size(150, 125);
            nodeSize.Width = 125;//(float)Math.Round(nodeSize.Width / 2f) * 2;  
            nodeSize.Height = 90; //(float)Math.Round(nodeSize.Height / 2f) * 2; 
            _tmpNode.Size = nodeSize;
            addflow.AddNode(_tmpNode);
        }
        
        public void AddFlowPreviewDragOverHandler(object s, DragEventArgs a)
        {
            _tmpNode.Location = new Point(a.GetPosition(addflow).X, a.GetPosition(addflow).Y);

        }
        public void AddFlowPreviewDropHandler(object s, DragEventArgs a)
        {
            addflow.RemoveNode(_tmpNode);
        }
        public void AddFlowPreviewDragLeaveHandler(object s, DragEventArgs a)
        {
            addflow.RemoveNode(_tmpNode);
        }

        public void DragImage(object sender, MouseButtonEventArgs e)
        {
            var _grabCursor = ((TextBlock)Resources["CursorGrab"]).Cursor;
            var _grabbingCursor = ((TextBlock)Resources["CursorGrabbing"]).Cursor;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.SetCursor(_grabbingCursor);
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                Mouse.SetCursor(_grabCursor);
            }

            object data;
            System.Windows.Controls.Image image = e.Source as System.Windows.Controls.Image;
            TextBlock tb = e.Source as TextBlock;
           
            if (image != null)
            {
                data = (NextGenModel.ImageData) image.DataContext;
                if (data != null && ((NextGenModel.ImageData) data).imageName.ToLower().Contains("linestyle"))
                {
                    e.Handled = true;
                }
                else
                {                    
                    DragDrop.DoDragDrop(image, data, DragDropEffects.Copy);
                }                   
            }
            else
            {
                data = (NextGenModel.ImageData) tb.DataContext;
                if (((data != null) && ((NextGenModel.ImageData) data).imageName.ToLower().Contains("linestyle"))||((NextGenModel.ImageData)data).imageName.Equals("BACNet") ||
                    ((NextGenModel.ImageData)data).imageName.Equals("LonWorks") || ((NextGenModel.ImageData)data).imageName.Equals("No BMS") || 
                    ((NextGenModel.ImageData)data).imageName.Equals("KNX") || ((NextGenModel.ImageData)data).imageName.Equals("ModBus")|| ((NextGenModel.ImageData)data).imageName.Equals("Heat Exchanger")
                    || ((NextGenModel.ImageData)data).imageName.Equals("VRF"))
                {
                    e.Handled = true;
                }
                else
                {
                    DragDrop.DoDragDrop(tb, data, DragDropEffects.Copy);
                }
            }
        }

        //end for context menu canvas
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
        }
        internal void ShowErrorMessage(string strMsg)
        {
            Application.Current.Dispatcher.Invoke(new System.Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    this.Snake.IsActive = true;
                    tbError.Text = strMsg;
                }
                else
                {
                    this.Snake.IsActive = false;
                    tbError.Text = strMsg;
                }

            }));

        }
        private void InitAutoPipingAddFlow()
        {
            //addflow = new WL.AddFlow();
            //addflow = new WL.AddFlow();
            //addflow = addflow;
            //addflow.AllowDrop = true;
            //addflow.CanDrawNode = false;
            //addflow.MouseDown += Addflow_MouseDown;
            //addflow.MouseUp += Addflow_MouseUp;
            //addflow.MouseRightButtonUp += addflow_rightClick;
            //addflow.Drop += addflow_Drop;
            //addflow.SelectionChanged += addflow_SelectionChanged;
            //InitializePlottingScaleHandler();
        }

        public void DrawPreSelectedNodesOnEdit(ref Lassalle.WPF.Flow.AddFlow adddflow,
            JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            InitAutoPipingAddFlow();

            //addCurrentContextflow.Zoom = 1;
            _eventAggregator.GetEvent<SystemDetailsSubscriber>().Publish(CurrentSystem);
            this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            double left = 220;
            double top = 220;
            NextGenBLL.PipingBLL pipBllNG = GetPipingBLLInstance();
            if (CurrentSystem.MyPipingNodeOut != null)
            {
                if (CurrentSystem.IsOutDoorUpdated)
                {
                    var selSys = CurrentSystem.DeepCopy();
                    selSys.IsInputLengthManually = false;
                    selSys.MyPipingNodeOut = null;
                    selSys.MyPipingNodeOutTemp = null;
                    var IsValid = ReselectOutdoor(selSys);
                    if (IsValid == true)
                    {
                        CurrentSystem = selSys;
                        _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                        DoDrawingPiping(true, CurrentSystem, ref addflow);
                        CurrentSystem.IsPipingOK = false;
                    }

                    //DoDrawingPiping(true, CurrentSystem, ref addflow);
                }
                else
                {
                    DoDrawingPiping(true, CurrentSystem, ref addflow);
                }


                if (CurrentSystem.IsInputLengthManually == true)
                {
                    //DoFindZeroLength();
                    // DoFindZeroLength
                    NextGenBLL.PipingErrors errorType = pipBllNG.ValidatePipeLength(CurrentSystem, ref addflow);
                    UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
                    ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
                }

                this.ScrollViewer.Content = addflow;
                if (CurrentSystem.IsPipingOK == true)
                {
                    Validate(CurrentSystem, ref addflow);
                }
                else
                {
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(CurrentSystem);
                }

                return;
            }

            ;

            if (CurrentSystem.MyPipingNodeOutTemp != null)
            {
                pipBllNG.LoadPipingNodeStructure(CurrentSystem);

                if (CurrentSystem.IsOutDoorUpdated)
                {
                    var selSys = CurrentSystem.DeepCopy();
                    selSys.IsInputLengthManually = false;
                    selSys.MyPipingNodeOut = null;
                    selSys.MyPipingNodeOutTemp = null;
                    var IsValid = ReselectOutdoor(selSys);
                    if (IsValid == true)
                    {
                        CurrentSystem = selSys;
                        _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                        DoDrawingPiping(true, CurrentSystem, ref addflow);
                        CurrentSystem.IsPipingOK = false;
                    }

                    //DoDrawingPiping(true, CurrentSystem, ref addflow);
                }
                else
                {
                    DoDrawingPiping(true, CurrentSystem, ref addflow);
                }


                if (CurrentSystem.IsInputLengthManually == true)
                {
                    // DoFindZeroLength
                    NextGenBLL.PipingErrors errorType = pipBllNG.ValidatePipeLength(CurrentSystem, ref addflow);
                    UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
                    ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
                }

                if (CurrentSystem.IsPipingOK == true)
                {
                    Validate(CurrentSystem, ref addflow);
                }
                else
                {
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(CurrentSystem);
                }

                return;
            }

            ;
            if (CurrentSystem != null)
            {
                AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSystem,
                    JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                if (result.SelectionResult == SelectOutdoorResult.OK)
                {

                    UpdatePipingNodeStructure(CurrentSystem);

                    if (CurrentSystem.IsOutDoorUpdated)
                    {
                        var selSys = CurrentSystem.DeepCopy();
                        selSys.IsInputLengthManually = false;
                        selSys.MyPipingNodeOut = null;
                        selSys.MyPipingNodeOutTemp = null;
                        var IsValid = ReselectOutdoor(selSys);
                        if (IsValid == true)
                        {
                            CurrentSystem = selSys;
                            _eventAggregator.GetEvent<ErrorLogVMClear>().Publish();
                            DoDrawingPiping(true, CurrentSystem, ref addflow);
                            CurrentSystem.IsPipingOK = false;
                        }

                        //DoDrawingPiping(true, CurrentSystem, ref addflow);
                    }
                    else
                    {
                        DoDrawingPiping(true, CurrentSystem, ref addflow);
                    }

                    if (CurrentSystem.IsPipingOK == true)
                    {
                        Validate(CurrentSystem, ref addflow);
                    }
                    else
                    {
                        _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(CurrentSystem);
                    }

                    return;
                }
            }

            if (this.projectLegacy.RoomIndoorList != null)
            {
                if (this.projectLegacy.RoomIndoorList.Count > 0)
                {
                    foreach (var node in this.projectLegacy.RoomIndoorList)
                    {
                        if (node.SystemID == CurrentSystem.Id)
                        {
                            if (node.IndoorNO > 0)
                            {
                                NextGenModel.ImageData data = new NextGenModel.ImageData();
                                if (node.IndoorItem.TypeImage == null)
                                {
                                    data.imagePath = node.DisplayImagePath;
                                }
                                else
                                {
                                    //data.imagePath = node.IndoorItem.TypeImage;
                                    data.imagePath = GetImagePathPiping() + "\\" + node.IndoorItem.TypeImage;
                                }

                                if (!File.Exists(data.imagePath))
                                {
                                    data.imagePath = GetImagePathPiping() + "\\" + "RWLT-10.0VNE.png";
                                }

                                data.imageName = node.DisplayImageName;
                                data.equipmentType = "Indoor";
                                data.NodeNo = node.IndoorNO;
                                data.UniqName = "Ind" + Convert.ToString(node.IndoorNO);
                                data.coolingCapacity = node.CoolingCapacity;
                                data.heatingCapacity = node.HeatingCapacity;
                                data.sensibleHeat = node.SensibleHeat;
                                try
                                {
                                    BitmapImage bitmap = new BitmapImage(new Uri(data.imagePath));
                                    ImageSource img = bitmap;
                                    JCHNode imageNode = new JCHNode(left, top, 125, 90, data.NodeNo.ToString(),
                                        addflow);
                                    imageNode.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
                                    imageNode.Image = img;
                                    setNodeStyle(imageNode, data);
                                    imageNode.ImageData = data;
                                    imageNode.ImagePosition = Lassalle.WPF.Flow.ImagePosition.CenterTop;
                                    imageNode.TextPosition = Lassalle.WPF.Flow.TextPosition.CenterBottom;
                                    addflow.AddNode(imageNode);
                                    left = left + 160;
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
            }

            if (CurrentSystem != null)
            {
                top = top + 200;
                left = 160;
                NextGenModel.ImageData data = new NextGenModel.ImageData();
                data.imagePath = CurrentSystem.DisplayImageName;
                if (!File.Exists(data.imagePath))
                {
                    data.imagePath = GetImagePathPiping() + "\\" + "JNBBQ 8-10.png";
                }

                data.imageName = CurrentSystem.Series;
                data.equipmentType = "Outdoor";
                data.NodeNo = CurrentSystem.NO;
                data.UniqName = "Out" + Convert.ToString(CurrentSystem.NO);
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(data.imagePath));
                    ImageSource img = bitmap;
                    //TO DO set margins dynamically
                    JCHNode imageNode = new JCHNode(10, 15, 125, 90, data.NodeNo.ToString(), addflow);
                    imageNode.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 90), 3, 3);
                    imageNode.Image = img;
                    setNodeStyle(imageNode, data);
                    imageNode.ImageData = data;
                    var nodeOut = utilPiping.createNodeOut();
                    nodeOut.ImageData = data;
                    GetPipingBLLInstance().drawNodeImage(nodeOut, data.imagePath, data, data.NodeNo, ref addflow);
                    // addflow.AddNode(imageNode);
                    //imageNode.ImagePosition = Lassalle.WPF.Flow.ImagePosition.CenterTop;
                    //imageNode.TextPosition = Lassalle.WPF.Flow.TextPosition.CenterBottom;
                    //addflow.AddNode(imageNode);
                    left = left + 100;
                }
                catch
                {
                }
            }


        }

        private bool ReselectOutdoor(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            string ErrMsg = string.Empty;
            this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            bool IsValidDraw = Utility.Validation.IsValidatedSystemVRF(this.projectLegacy, CurrentSystem, out ErrMsg);
            try
            {
                NextGenModel.SystemVRF CurrVRF = new NextGenModel.SystemVRF();
                AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                if (IsValidDraw)
                {
                    AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSystem,
                        JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
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
                            JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_IDU_CAPACITY_MISMATCH"),
                                MessageType.Error);
                        }
                        else if (result.MSGList != null && result.MSGList.Count > 0)
                        {
                            _eventAggregator.GetEvent<ErrorLogVM>().Publish(result.MSGList);
                            JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_IDU_CAPACITY_MISMATCH"),
                                MessageType.Error);
                        }
                        else
                        {
                            JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_IDU_CAPACITY_MISMATCH"),
                                MessageType.Error);
                        }
                    }
                }
                else
                {
                    IsValidDraw = false;
                    JCHMessageBox.Show(string.Format(ErrMsg));
                    ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, ErrMsg);
                }
            }
            catch (Exception)
            {

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
                JCHMessageBox.Show("Error Occured : " + ex.Message);
            }
        }

        public void UpdateWiringNodeStructure(JCHVRF.Model.NextGen.SystemVRF CurrentSystem)
        {
            try
            {
                string imageDir = @"/Image/TypeImages/";
                JCHVRF_New.Utility.WiringBLL bll =
                    new JCHVRF_New.Utility.WiringBLL(JCHVRF.Model.Project.GetProjectInstance, imageDir);
                bll.CreateWiringNodeStructure(CurrentSystem);
            }
            catch (Exception ex)
            {
                JCHMessageBox.Show("Error Occured : " + ex.Message);
            }
        }

        private void addflow_Drop(object sender, DragEventArgs e)
        {
            //if(_currentSystem==null)
            //    _currentSystem=WorkFlowContext.NewSystem;
            _currentSystem = WorkFlowContext.CurrentSystem;
            _eventAggregator.GetEvent<GetCurrentSystem>().Subscribe(GetCurrentSystem);

            if (_currentSystem != null)
            {
                switch (_currentSystem.HvacSystemType)
                {
                    case "1":
                        HandleDropForVrf(sender, e);
                        GetDefaultAccessoryForIDU();
                        break;

                    case "6":
                        HandleDropForController(e);
                        break;
                }
            }
        }

        public void GetCurrentSystem(SystemBase sys)
        {
            _currentSystem = sys;
        }
        /// <summary>
        /// Logic to Draw the Central Controller on Canvas
        /// </summary>
        /// <param name="e"></param>
        private void HandleDropForController(DragEventArgs e)
        {            
            lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Transparent);
            lblCanvasError.BorderThickness = new Thickness(0);
            lblCanvasError.Content = string.Empty;

            NextGenModel.ImageData data = new NextGenModel.ImageData();
            var SenderData = (NextGenModel.ImageData) e.Data.GetData(typeof(NextGenModel.ImageData));
            var AllNodeList = Enumerable.OfType<WL.Node>(addflow.Items).ToArray();

            data.imagePath = SenderData.imagePath;
            data.imageName = SenderData.imageName;
            data.equipmentType = SenderData.equipmentType;
            data.Source = SenderData.Source;

            ImageSource img = null;
            JCHNode imageNode = null;
            if (SenderData.imagePath != null)
            {
                System.Drawing.Image img1 = System.Drawing.Image.FromFile(SenderData.imagePath);
                drawing.Bitmap bitmap = new drawing.Bitmap(img1, new drawing.Size(60, 40));
                var bitmapImage = BitmapToImageSource(bitmap);
                img = bitmapImage;
            }

            double left = e.GetPosition(addflow).X;
            double top = e.GetPosition(addflow).Y;

            leftC = left;
            topC = top;

            imageNode = new JCHNode(left, top, 125, 100, data.NodeNo.ToString(), addflow);
            if (SenderData.imagePath != null)
            {
                imageNode.Image = img;
            }

            setNodeStyleForControllers(imageNode, data);
            imageNode.ImageData = data;
            
            var viewModel = (ucDesignerCanvasViewModel) DataContext;           
            viewModel.CurrentSystem = _currentSystem;
            /*Compatibility check, Existence in other group algorithm  */
            bool addOnCanvas = viewModel.AssignGroupToEquipment(SenderData, projectLegacy);
            if (addOnCanvas)
            {
                addflow.AddNode(imageNode);
                //To remove the system from system Equipment list
                _eventAggregator.GetEvent<DeleteEquipmentList>().Publish(SenderData);               
            }            

            var nodes = addflow.Items.OfType<JCHNode>();
            foreach (var jchNode in nodes)
            {
                //Apply Controller styles such as Pins with + and - symbol
                DrawMandatoryNodeVisualsForController(jchNode);
            }
            UtilTrace.SaveHistoryTraces();
        }

        
        /// <summary>
        /// Node style for Central Controller after the Drop on Canvas
        /// </summary>
        /// <param name="node"></param>
        /// <param name="data"></param>
        private void setNodeStyleForControllers(WL.Node node, NextGenModel.ImageData data)
        {
            node.Geometry = new RectangleGeometry(new Rect(0, 0, 100, 70), 3, 3);
            // node.Stroke = System.Windows.Media.Brushes.RoyalBlue;
            node.Stroke = new SolidColorBrush(Color.FromRgb(185, 185, 185)); // System.Windows.Media.Brushes.Gray;
            node.StrokeThickness = 2;
            if (node.Links == null || node.Links.Count == 0)
                node.DashStyle = DashStyles.Dash;
            node.Fill = System.Windows.Media.Brushes.White;
            node.Text = data.imageName;            
            node.TextMargin = new Thickness(3);            
            if(node.Image == null)
            {
                node.TextPosition = WL.TextPosition.CenterMiddle;
            }
            else
            {
                node.ImageMargin = new Thickness(3);
                node.ImagePosition = WL.ImagePosition.CenterTop;
                node.TextPosition = WL.TextPosition.CenterBottom;
            }
            
            node.IsXSizeable = false; //test resize
            node.IsYSizeable = false; //test resize                
            //var pointOnNode = new PointCollection();
            //pointOnNode.Add(new Point(50, 0));
            //pointOnNode.Add(new Point(50, 100));
            //node.PinsLayout = pointOnNode;

            //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            //string navigateToFolder = "..\\..\\Image";
            //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            //string imageFullPath = sourceDir + "\\" + "CC_plus.png";

            //ImageBrush brush = new ImageBrush();
            //System.Drawing.Image img1 = System.Drawing.Image.FromFile(imageFullPath);
            //drawing.Bitmap bitmap = new drawing.Bitmap(img1, new drawing.Size(10,10));            
            //var bitmapImage = BitmapToImageSource(bitmap);           
            //brush.ImageSource = bitmapImage;

            //addflow.PinFill = brush;
            //addflow.PinShape = WL.PinShape.Circle;
            //addflow.PinSize = 13;
            //node.OutConnectionMode = WL.ConnectionMode.Pin;
            //node.InConnectionMode = WL.ConnectionMode.Pin;
            
            addflow.CanDrawNode = false;
            addflow.CanDrawLink = true;
            node.FontSize = 9;            
            node.IsSelectable = true;
            node.IsEditable = false;
        }

        private void HandleDropForVrf(object sender, DragEventArgs e)
        {
            //Start Bug#1646
            lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Transparent);
            lblCanvasError.BorderThickness = new Thickness(0);
            lblCanvasError.Content = string.Empty;
            //End Bug#1646
            int InNodeCount = 0;
            int OutNodeCount = 1;
            int SeperatorNodeCount = 0;
            string UniName = "";
            NextGenModel.ImageData data = new NextGenModel.ImageData();
            var SenderData = (NextGenModel.ImageData) e.Data.GetData(typeof(NextGenModel.ImageData));
            var AllNodeList = Enumerable.OfType<WL.Node>(addflow.Items).ToArray();
            var YPNodeList = Enumerable.OfType<NextGenModel.MyNodeYP>(addflow.Items).ToArray();
            BitmapImage bitmap = new BitmapImage(new Uri(SenderData.imagePath));
            var filepath= SenderData.imagePath;
            drawing.Image path = drawing.Image.FromFile(filepath);
            System.Drawing.Bitmap bitmaps = new System.Drawing.Bitmap(path,60,55);
            
            ImageSource img = bitmap;
            double left = e.GetPosition(addflow).X;
            double top = e.GetPosition(addflow).Y;

            while (!addflow.Items.TrueForAll(a => !a.Bounds.Contains(new Point(left, top)) && !a.Bounds.Contains(new Point(left + 125, top)) && !a.Bounds.Contains(new Point(left, top + 100)) && !a.Bounds.Contains(new Point(left + 125, top + 100)) && !a.Bounds.Contains(new Point(left, top + 50))))
            {
                if (JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_EQUIPMENT_OVERLAP"), MessageType.Error, MessageBoxButton.OK) == MessageBoxResult.OK)
                {
                    return;
                }

            }
            if (Project.CurrentProject.RoomIndoorList != null)
            {
                InNodeCount = GetIndoorCount();
            }
            else
            {
                InNodeCount = 1;
            }
            if (SenderData.equipmentType == "Outdoor")
            {
                UniName = SystemSetting.UserSetting.defaultSetting.OutdoorName + Convert.ToString(OutNodeCount);
            }
            else if (SenderData.equipmentType == "Indoor")
            {
                UniName = SystemSetting.UserSetting.defaultSetting.IndoorName + Convert.ToString(InNodeCount);
            }
            else if (SenderData.equipmentType == "Pipe")
            {
                UniName = "Sep" + Convert.ToString(SeperatorNodeCount);
            }
            else if (SenderData.equipmentType == "HeaderBranch")
            {
                UniName = "Sep" + Convert.ToString(SeperatorNodeCount);
            }

            data.imagePath = SenderData.imagePath;
            data.imageName = SenderData.imageName;
            data.equipmentType = SenderData.equipmentType;
            data.NodeNo = InNodeCount;
            data.UniqName = UniName;
            if (SenderData.equipmentType.Equals("Indoor", StringComparison.OrdinalIgnoreCase))
            {
                AddIndoorToList(data);
            }
            JCHNode imageNode = new JCHNode(left, top, 125, 100, data.NodeNo.ToString(), addflow);
           
            
        //    var bitmapData = bitmaps.LockBits(
        //    new System.Drawing.Rectangle(0, 0, bitmaps.Width, bitmaps.Height),
        //System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmaps.PixelFormat);

        //    var bitmapSource = BitmapSource.Create(
        //        bitmapData.Width, bitmapData.Height,
        //       0, 0,
        //        PixelFormats.Gray16, null,
        //        bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

        //    bitmaps.UnlockBits(bitmapData);


            BitmapSource bitSrc = null;

            var hBitmap = bitmaps.GetHbitmap();


            bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());


            imageNode.Image = bitSrc;

            setNodeStyle(imageNode, data, true);
            imageNode.ImageData = data;
            if (CurrentSystem != null)
            {
                CurrentSystem.IsManualPiping = true;
                if (CurrentSystem.IsManualPiping)
                {
                    SetManualPipingModelOnAddFlow(ref addflow);
                }
            }

            //if (!CurrentSystem.IsManualPiping)
            //{
            //    ResetPipingNodeStructure();
            //}
            if (!string.IsNullOrEmpty(UniName))
            {
                string extractnumber = Regex.Match(UniName, @"\d+").Value;
                if (!string.IsNullOrEmpty(extractnumber))
                {
                    data.NodeNo = Convert.ToInt32(extractnumber);
                }
            }
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            if (data.equipmentType.Equals("Indoor"))
            {
                var allNodeList = addflow.Items.OfType<NextGenModel.MyNodeIn>();
                foreach (var nodeIndoor in allNodeList)
                {
                    nodeIndoor.IsSelected = false;
                }
                var nodeIn = utilPiping.createNodeIn(Project.GetProjectInstance.RoomIndoorList.Last());
                nodeIn.ImageData = data;
                pipBll.drawNodeImage(nodeIn, data.imagePath, data, data.NodeNo, ref addflow);
                nodeIn.Location = imageNode.Location;
                nodeIn.Stroke = Brushes.Gray;
                nodeIn.IsRotatable = false;
                nodeIn.IsSelected = true;
                //_eventAggregator.GetEvent<FloatProperties>().Publish();
            }
            else
            {
                if (Path.GetFileNameWithoutExtension(data.imagePath).Equals("HeaderBranch_8Seperator"))
                {
                    var header8Branch = CreateNewYpNode(NodeType.CP);
                    header8Branch.Location = imageNode.Location;
                    addflow.AddNode(header8Branch);
                }
                else if (Path.GetFileNameWithoutExtension(data.imagePath).Equals("HeaderBranch_4Seperator"))
                {
                    var header4Branch = CreateNewYpNode(NodeType.YP4);
                    header4Branch.Location = imageNode.Location;
                    addflow.AddNode(header4Branch);
                }
                else
                {
                    addflow.AddNode(imageNode);
                }
            }
            SetManualPipingModelOnAddFlow(ref addflow);
            AddEquipmentOnCanvas(data);
            addOrphanNode();
            CheckForErrorState();
            if (CurrentSystem!=null && CurrentSystem.IsPipingOK && CurrentSystem.IsManualPiping)
            {
                CurrentSystem.IsPipingOK = false;
                ClearValidationText(addflow);
            }
            
        }
        private void CheckForErrorState()
        {
            bool errorState = IsDiagramInErrorState();

            if (errorState)
            {
                if (CurrentSystem != null)
                    CurrentSystem.SystemStatus = SystemStatus.WIP;
                ShowErrorNotification();
            }
            else
            {
                ClearErrorNotification();
                CurrentSystem.SystemStatus = SystemStatus.VALID;
            }
            SaveManualPiping();

        }
        private void AddOrphanNode(JCHNode indoorNode)
        {
            if (CurrentSystem.MyPipingOrphanNodes == null)
            {
                CurrentSystem.MyPipingOrphanNodes = new List<NextGenModel.MyNodeIn>();
            }
            var node = indoorNode as NextGenModel.MyNodeIn;
            if (node == null) return;
            if (!CurrentSystem.MyPipingOrphanNodes.Contains(node))
            {
                CurrentSystem.MyPipingOrphanNodes.Add(node);
            }            
        }
        private void DeleteOrphanNodeIfExist(JCHNode indoorNode)
        {
            var node = indoorNode as NextGenModel.MyNodeIn;
            if (node == null) return;
            if (CurrentSystem.MyPipingOrphanNodes != null && CurrentSystem.MyPipingOrphanNodes.Contains(node))
            {
                CurrentSystem.MyPipingOrphanNodes.Remove(node);
            }
           if(CurrentSystem!=null &&  CurrentSystem.MyPipingOrphanNodes !=null && CurrentSystem.MyPipingOrphanNodes.Count==0)
            {
                CurrentSystem.SystemStatus = SystemStatus.INVALID;
            }
        }
        private void SetManualPipingModelOnAddFlow(ref WL.AddFlow addflow)
        {
            addflow.CanDrawNode = false;
            addflow.CanDrawLink = true;
            addflow.CanReflexLink = false;
            SetPinFillStyle();

            addflow.NodeModel.OutConnectionMode = WL.ConnectionMode.Pin;
            addflow.NodeModel.InConnectionMode = WL.ConnectionMode.Pin;
            addflow.NodeModel.IsSelectable = true;
            addflow.LinkModel.LineStyle = WL.LineStyle.Orthogonal;
            addflow.LinkModel.Stroke = Brushes.Gray;
            addflow.LinkModel.StrokeThickness = 2;
            addflow.GridSnap = false;
            SetPinLayOutForUnits(addflow);
            var ypNodes = addflow.Items.OfType<NextGenModel.MyNodeYP>();
            foreach (var myNodeYp in ypNodes)
            {
                myNodeYp.IsSelectable = true;
            }
            var hideHandleStyle = new Style(typeof(System.Windows.Controls.Primitives.Thumb));
            hideHandleStyle.Setters.Add(new Setter(VisibilityProperty, Visibility.Hidden));
            addflow.ResizeHandleStyle = hideHandleStyle;
            var nodes = addflow.Items.OfType<JCHNode>();
            foreach (var node in nodes)
            {
                node.IsRotatable = false;
            }
            // TODO : enable after manual piping undo redo
            //UtilTrace.SaveHistoryTraces();
        }

        private static void SetPinLayOutForUnits(WL.AddFlow addflow)
        {
            var indoorPinLayout = new PointCollection { new Point(50, 0) };
            var outdoorPinLayout = new PointCollection { new Point(50, 100) };
            foreach (var node in addflow.Items.OfType<JCHNode>().Where(x => x.IsIndoorUnit() || x.IsOutdoorUnit()))
            {
                node.PinsLayout = node.ImageData.equipmentType.Equals("Indoor") ? indoorPinLayout : outdoorPinLayout;
                node.InConnectionMode = WL.ConnectionMode.Pin;
                node.OutConnectionMode = WL.ConnectionMode.Pin;
            }
        }
        #region Visual Drawing
        private void DrawDefaultNodePin(JCHNode node)
        {
            if (node == null) return;
            var connectionHandlePoint = GetMandatoryConnectionHandleLocation(node);
            if (connectionHandlePoint == null)
            {
                return;
            }

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            node.Visual.Children.Clear();

            #region Default Values

            var handleSize = 2;
            var connectionhandleSize = 5;
            var rightCenterPoint = new Point(node.Location.X + node.Size.Width - handleSize - 2,
                node.Location.Y + node.Size.Height / 2);
            var rightCenterPairPoint = new Point(node.Location.X + node.Size.Width - 2 * handleSize - 6,
                node.Location.Y + node.Size.Height / 2);
            var handleBrush = GetBrushFromCode("#6d6e71");

            #endregion

            #region DrawDefaultNodePin


            if (connectionHandlePoint != null)
            {
                dc.DrawEllipse(GetBrushFromCode("#2DDDFA"), new Pen(GetBrushFromCode("#2DDDFA"), 2),
                    connectionHandlePoint.Value, connectionhandleSize, connectionhandleSize);

            }

            #endregion

            DrawHandles(node, dc);









            #region Draw Expand Button
            var closeIcon = Path.Combine(GetImagePathPiping(), "NodeCloseIcon.png");
            var expandIcon = Path.Combine(GetImagePathPiping(), "NodeExpandIcon.png");
            if (File.Exists(expandIcon))
            {
                var bitmap = new drawing.Bitmap(expandIcon);
                var imgSource = BitmapToImageSource(bitmap);
                dc.DrawImage(imgSource,
                    new Rect(new Point(node.Bounds.TopRight.X - 20, node.Bounds.TopRight.Y), new Size(20, 20)));

            }

            #endregion

            dc.Close();

            node.Visual.Children.Add(dv);
        }
        private void DrawMandatoryNodeVisualsIncludingHeaderButtons(JCHNode node)
        {
            var connectionHandlePoint = GetMandatoryConnectionHandleLocation(node);
            if (connectionHandlePoint == null)
            {
                return;
            }

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            node.Visual.Children.Clear();

            #region Default Values

            var handleSize = 2;
            var connectionhandleSize = 5;
            var rightCenterPoint = new Point(node.Location.X + node.Size.Width - handleSize - 2,
                node.Location.Y + node.Size.Height / 2);
            var rightCenterPairPoint = new Point(node.Location.X + node.Size.Width - 2 * handleSize - 6,
                node.Location.Y + node.Size.Height / 2);
            var handleBrush = GetBrushFromCode("#6d6e71");

            #endregion

            #region DrawDefaultNodePin


            if (connectionHandlePoint != null)
            {
                dc.DrawEllipse(GetBrushFromCode("#2DDDFA"), new Pen(GetBrushFromCode("#2DDDFA"), 2),
                    connectionHandlePoint.Value, connectionhandleSize, connectionhandleSize);

            }

            #endregion

            #region Draw Handles

            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    rightCenterPoint, handleSize, handleSize);
            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    rightCenterPairPoint, handleSize, handleSize);

            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPoint.X, rightCenterPoint.Y - 3 * handleSize), handleSize, handleSize);
            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPairPoint.X, rightCenterPairPoint.Y - 3 * handleSize), handleSize, handleSize);

            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPoint.X, rightCenterPoint.Y + 3 * handleSize), handleSize, handleSize);
            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPairPoint.X, rightCenterPairPoint.Y + 3 * handleSize), handleSize, handleSize);

            #endregion

            #region  DrawCloseButton

            var closeIcon = Path.Combine(GetImagePathPiping(), "NodeCloseIcon.png");
            var expandIcon = Path.Combine(GetImagePathPiping(), "NodeExpandIcon.png");
            if (File.Exists(closeIcon))
            {
                var bitmap = new drawing.Bitmap(closeIcon);
                var imgSource = BitmapToImageSource(bitmap);
                dc.DrawImage(imgSource, new Rect(node.Location, new Size(20, 20)));
                SetCloseLocation(node, new Rect(node.Location, new Size(20, 20)));

            }

            #endregion

            #region Draw Expand Button

            if (File.Exists(expandIcon))
            {
                var bitmap = new drawing.Bitmap(expandIcon);
                var imgSource = BitmapToImageSource(bitmap);
                dc.DrawImage(imgSource,
                    new Rect(new Point(node.Bounds.TopRight.X - 20, node.Bounds.TopRight.Y), new Size(20, 20)));

            }

            #endregion

            dc.Close();

            node.Visual.Children.Add(dv);
        }
        private void DrawMandatoryNodeVisuals(JCHNode node)
        {
            var connectionHandlePoint = GetMandatoryConnectionHandleLocation(node);
            if (connectionHandlePoint == null)
            {
                return;
            }

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            node.Visual.Children.Clear();

            #region Default Values

            var connectionhandleSize = 5;


            #endregion

            #region DrawDefaultNodePin


            if (connectionHandlePoint != null)
            {
                dc.DrawEllipse(GetBrushFromCode("#2DDDFA"), new Pen(GetBrushFromCode("#2DDDFA"), 2),
                    connectionHandlePoint.Value, connectionhandleSize, connectionhandleSize);

            }

            #endregion

            DrawHandles(node, dc);
            #region  DrawCloseButton
            var closeIcon = Path.Combine(GetImagePathPiping(), "NodeCloseIcon.png");
            if (File.Exists(closeIcon))
            {
                var bitmap = new drawing.Bitmap(closeIcon);
                var imgSource = BitmapToImageSource(bitmap);
                dc.DrawImage(imgSource, new Rect(new Point(node.Location.X - 1, node.Location.Y - 1), new Size(20, 20)));
                SetCloseLocation(node, new Rect(node.Location, new Size(20, 20)));
            }
            #endregion
            DrawExpandIcon(node, dc);
            dc.Close();
            node.Visual.Children.Add(dv);
        }
        private static void DrawHandles(JCHNode node, DrawingContext dc)
        {
            #region Draw Handles
            var handleBrush = Brushes.Gray;// GetBrushFromCode("#6d6e71");
            var handleSize = 2;
            var rightCenterPoint = new Point(node.Location.X + node.Size.Width - handleSize - 2,
                node.Location.Y + node.Size.Height / 2);
            var rightCenterPairPoint = new Point(node.Location.X + node.Size.Width - 2 * handleSize - 6,
                node.Location.Y + node.Size.Height / 2);

            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    rightCenterPoint, handleSize, handleSize);
            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    rightCenterPairPoint, handleSize, handleSize);

            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPoint.X, rightCenterPoint.Y - 3 * handleSize), handleSize, handleSize);
            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPairPoint.X, rightCenterPairPoint.Y - 3 * handleSize), handleSize, handleSize);

            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPoint.X, rightCenterPoint.Y + 3 * handleSize), handleSize, handleSize);
            //dc.DrawEllipse(handleBrush, new Pen(handleBrush, 2),
            //    new Point(rightCenterPairPoint.X, rightCenterPairPoint.Y + 3 * handleSize), handleSize, handleSize);

            #endregion
        }

        private void DrawExpandIcon(JCHNode node, DrawingContext dc)
        {
            #region Draw Expand Button
            var expandIcon = Path.Combine(GetImagePathPiping(), "NodeExpandIcon.png");

            if (File.Exists(expandIcon))
            {
                var bitmap = new drawing.Bitmap(expandIcon);
                var imgSource = BitmapToImageSource(bitmap);
                dc.DrawImage(imgSource, new Rect(new Point(node.Bounds.TopRight.X - 19, node.Bounds.TopRight.Y - 1), new Size(20, 20)));

        }
            #endregion
        }

        

        private Point? GetMandatoryConnectionHandleLocation(JCHNode node)
        {
            if (node.ImageData != null && node.ImageData.equipmentType != null)
            {
                if (node.ImageData.equipmentType.Equals("Outdoor"))
                {
                    return new Point(node.Location.X + node.Size.Width / 2, node.Location.Y + node.Size.Height);
                }

                if (node.ImageData.equipmentType.Equals("Indoor"))
                {
                    return new Point(node.Location.X + node.Size.Width / 2, node.Location.Y);
                }
            }
            return null;
        }

        private Brush GetBrushFromCode(string code)
        {
            return (SolidColorBrush) (new BrushConverter().ConvertFrom(code));
        }

        static readonly DependencyProperty CloseLocationProperty =
            DependencyProperty.RegisterAttached("CloseLocation", typeof(Rect), typeof(JCHNode),
                new PropertyMetadata(null));

        public static Rect? GetCloseLocation(DependencyObject node)
        {
            if (node == null)
            {
                return null;
            }

            return (Rect) node.GetValue(CloseLocationProperty);
        }

        public static void SetCloseLocation(DependencyObject node, Rect value)
        {
            if (node != null)
            {
                node.SetValue(CloseLocationProperty, value);
            }
        }

        #endregion

        void ResetPipingNodeStructure()
        {
            string SystemId = string.Empty;
            if (_currentSystem != null)
            {
                SystemId = _currentSystem.Id;
            }
            else
            {
                if (WorkFlowContext.CurrentSystem != null)
                {
                    if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                    {
                        SystemId = (WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF).Id;
                    }
                }
            }

            if (JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(a => a.Id == SystemId) != null)
            {
                JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(a => a.Id == SystemId)
                    .MyPipingNodeOut = null;
                JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(a => a.Id == SystemId)
                    .MyPipingNodeOutTemp = null;
                JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(a => a.Id == SystemId)
                    .IsPipingOK = false;
                _eventAggregator.GetEvent<RefreshSystems>().Publish();
            }
        }

        private void AddIndoorToList(ImageData data)
        {
            if (data != null && !string.IsNullOrEmpty(data.equipmentType) && data.equipmentType == "Pipe")
            {
                return;
            }
            //if (IsValidate())
            //{
            string SystemId = string.Empty;
            //var CurrentSys =  _currentSystem (JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.NewSystem;
            if (_currentSystem != null)
            {
                SystemId = _currentSystem.Id;
            }
            else
            {
                if (WorkFlowContext.CurrentSystem != null)
                {
                    if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                    {
                        SystemId = (WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF).Id;
                    }
                }
            }

            RoomIndoor RoomObj = new RoomIndoor();
            RoomObj.IndoorNO = data.NodeNo; //GetIndoorCount();
            RoomObj.IndoorName =
                data.UniqName; //SystemSetting.UserSetting.defaultSetting.IndoorName + GetIndoorCount();

            RoomObj.RqCoolingCapacity = 0;
            RoomObj.RqSensibleHeat = 0;
            RoomObj.RqHeatingCapacity = 0;
            RoomObj.RqAirflow = 0;
            RoomObj.RqFreshAir = 0;
            RoomObj.RqStaticPressure = 0;

            RoomObj.DBCooling = 0;
            RoomObj.WBCooling = 0;
            RoomObj.DBHeating = 0;
            RoomObj.IsAuto = true;
            RoomObj.HeightDiff = 0;
            RoomObj.FanSpeedLevel = 0;

            RoomObj.SelectedRoom = new Room();

            //Start Backward Compatibility : Added a null check to avoid Null reference exception.
            if (JCHVRF.Model.Project.GetProjectInstance.RoomList == null)
            {
                RoomObj.RoomID = "0";
            }
            else
            {
                RoomObj.RoomID = JCHVRF.Model.Project.GetProjectInstance.RoomList.Count.ToString();
            } 
            //End Backward Compatibility : Added a null check to avoid Null reference exception.

            RoomObj.RoomName = "Room" + RoomObj.RoomID;
            RoomObj.IndoorItem = new Indoor();
            //RoomObj.IndoorItem.TypeImage = data.imagePath;
            RoomObj.IndoorItem.TypeImage= data.imagePath.Substring((data.imagePath.LastIndexOf("\\"))+1); // TypeImage field contains only model name

            RoomObj.SetToSystemVRF(SystemId);
            RoomObj.DisplayImagePath = data.imagePath;
            RoomObj.DisplayImageName = data.imageName;

            //if (this.ManualSelection == false)
            //{
            //    RoomObj.RqCoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.CR_TotalCapacity), UnitType.POWER, utPower);
            //    RoomObj.RqSensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.CR_SensibleCapacity), UnitType.POWER, utPower);
            //    RoomObj.RqHeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.CR_HeatingCapacity), UnitType.POWER, utPower);
            //    RoomObj.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(this.CR_AirFlow), UnitType.AIRFLOW, utAirflow);
            //    RoomObj.RqFreshAir = Unit.ConvertToSource(Convert.ToDouble(this.CR_FreshAir), UnitType.AIRFLOW, utAirflow);
            //    RoomObj.RqStaticPressure = Convert.ToDouble(this.CR_ESP);
            //}
            //else
            //{
            //    RoomObj.RqCoolingCapacity = 0;
            //    RoomObj.RqSensibleHeat = 0;
            //    RoomObj.RqHeatingCapacity = 0;
            //    RoomObj.RqAirflow = 0;
            //    RoomObj.RqFreshAir = 0;
            //    RoomObj.RqStaticPressure = 0;
            //}
            //RoomObj.DBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            //RoomObj.WBCooling = Unit.ConvertToSource(Convert.ToDouble(this.CoolingWetBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            //RoomObj.DBHeating = Unit.ConvertToSource(Convert.ToDouble(this.HeatingDryBulb), UnitType.TEMPERATURE, this.DesignConditionTempMasureUnit);
            //RoomObj.IsAuto = !this.ManualSelection;
            //RoomObj.HeightDiff = Convert.ToDouble(this.HeightDifference);
            //RoomObj.FanSpeedLevel = (int)Enum.Parse(typeof(JCHVRF_New.Model.FanSpeed), this.SelectedFanSpeed);
            //RoomObj.SelectedRoom = this.SelectedRoom;
            //if (this.SelectedRoom != null)
            //{
            //    RoomObj.RoomID = this.SelectedRoom.Id;
            //    RoomObj.RoomName = this.SelectedRoom.Name;
            //}
            //RoomObj.SelectedFloor = JCHVRF.Model.Project.GetProjectInstance.FloorList.FirstOrDefault(i => i.Id == this.SelectedFloor);
            //RoomObj.PositionType = this.SelectedIduPosition;
            //RoomObj.RHCooling = Convert.ToDouble(this.RelativeHumidity);
            //Indoor inItem = IDUIndoorList.FirstOrDefault(m => m.ModelFull == this.SelectedModel);
            //if (inItem != null)
            //{
            //    inItem.Series = ODUSeries;
            //    inItem.CoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.POWER, utPower);
            //    inItem.SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.POWER, utPower);
            //    inItem.HeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.POWER, utPower);
            //    inItem.AirFlow = Unit.ConvertToSource(Convert.ToDouble(this.SR_AirFlow), UnitType.AIRFLOW, utAirflow);

            //    RoomObj.CoolingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_TotalCapacity), UnitType.POWER, utPower);
            //    RoomObj.HeatingCapacity = Unit.ConvertToSource(Convert.ToDouble(this.SR_SensibleCapacity), UnitType.POWER, utPower);
            //    RoomObj.SensibleHeat = Unit.ConvertToSource(Convert.ToDouble(this.SR_HeatingCapacity), UnitType.POWER, utPower);
            //    RoomObj.RqAirflow = Unit.ConvertToSource(Convert.ToDouble(this.SR_AirFlow), UnitType.AIRFLOW, utAirflow);
            //}
            //RoomObj.IndoorItem = inItem;
            //RoomObj.SelectedUnitType = this.SelectedUnitType;

            //if (WorkFlowContext.NewSystem != null)
            //    RoomObj.SetToSystemVRF(WorkFlowContext.NewSystem.Id);
            //else if (WorkFlowContext.CurrentSystem != null)
            //    RoomObj.SetToSystemVRF(WorkFlowContext.CurrentSystem.Id);
            //RoomObj.DisplayImagePath = this.IduImagePath;
            //RoomObj.DisplayImageName = this.SelectedUnitType;
            //ListRoomIndoor.Add(RoomObj);
            //Refresh();
            projectLegacy.RoomIndoorList.Add(RoomObj);
        }

        private int GetIndoorCount()
        {
            int InNodeCount = 0;
            if (JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList != null)
            {
                foreach (RoomIndoor ri in JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList)
                {
                    InNodeCount = InNodeCount > ri.IndoorNO ? InNodeCount : ri.IndoorNO;
                }
            }

            return InNodeCount + 1;
        }

        private void InitializeLinkValues()
        {
            addflow.CanDrawLink = true;
            addflow.CanStretchLink = true;
            addflow.LinkModel.LineStyle = WL.LineStyle.Orthogonal;
            addflow.LinkModel.Stroke = System.Windows.Media.Brushes.SkyBlue;
            addflow.LinkModel.StrokeThickness = 2;
        }

        public void UpdateEquipmentOnCanvas(NextGenModel.ImageData imgData)
        {
            try
            {
                if (imgData.equipmentType == "Outdoor")
                {                    
                    var allNodeList = Enumerable.OfType<JCHNode>(this.addflow.Items).ToArray();                    
                    var ProjcurrentSystem = JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen;
                    var Lists = ProjcurrentSystem.Where(idu => idu.Id == CurrentSystem.Id).FirstOrDefault();                    
                    foreach (var node in allNodeList)
                    {
                        var selectedNode = node.ImageData;
                        if (selectedNode == null)
                        {
                            continue;
                        }

                        if (selectedNode.UniqName == imgData.UniqName)
                        {
                            string smallImagePath = imgData.imagePath.Replace("TypeImageProjectCreation", "TypeImages");
                            smallImagePath = File.Exists(smallImagePath) ? smallImagePath : smallImagePath.Replace("TypeImages", "TypeImageProjectCreation");
                            if (CurrentSystem != null && CurrentSystem.IsManualPiping && File.Exists(imgData.imagePath))
                                smallImagePath = imgData.imagePath;
                            BitmapImage bitmap = new BitmapImage(new Uri(smallImagePath));
                            ImageSource img = bitmap;
                            node.Image = img;                            
                            string outDoorModel = string.Empty;
                            if (Lists != null)
                            {
                                if (JCHVRF.Model.Project.CurrentProject.BrandCode == "H")
                                    outDoorModel = Lists.OutdoorItem.Model_Hitachi;
                                else
                                    outDoorModel = Lists.OutdoorItem.ModelFull;
                                imgData.imageName = outDoorModel;
                            }
                            setNodeStyle(node, imgData);
                            node.ImageData = imgData;                            
                            break;
                        }

                    }
                }
                else if((imgData.equipmentType == "Indoor"))
                {
                    var allIduNodeList = addflow.Items.OfType<NextGenModel.MyNodeIn>().Where(a=>a.IsSelected);
                    foreach (var node in allIduNodeList)
                    {
                        var selectedNode = node.ImageData;
                        if (selectedNode != null)
                        {
                            if (selectedNode.UniqName == imgData.UniqName)
                            {
                                //BitmapImage bitmap = new BitmapImage(new Uri(imgData.imagePath));
                                BitmapImage bitmap = new BitmapImage(new Uri(imgData.imagePath));
                                var filepath = imgData.imagePath;
                                drawing.Image path = drawing.Image.FromFile(filepath);
                                System.Drawing.Bitmap bitmaps = new System.Drawing.Bitmap(path, 60, 55);
                                //                        var bitmapData = bitmaps.LockBits(
                                //    new System.Drawing.Rectangle(0, 0, bitmaps.Width, bitmaps.Height),
                                //System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmaps.PixelFormat);

                                //                        var bitmapSource = BitmapSource.Create(
                                //                            bitmapData.Width, bitmapData.Height,
                                //                            0, 0,
                                //                            PixelFormats.Gray16, null,
                                //                           bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                                //                        bitmaps.UnlockBits(bitmapData);



                                BitmapSource bitSrc = null;

                                var hBitmap = bitmaps.GetHbitmap();


                                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                    hBitmap,
                                    IntPtr.Zero,
                                    Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
                                ImageSource img = bitSrc;
                                node.Image = img;
                                setNodeStyle(node, imgData);
                                node.ImageData = imgData;
                                ((NextGenModel.MyNodeIn)node).RoomIndooItem = projectLegacy.RoomIndoorList.Where(x => x.IndoorName.ToUpper() == selectedNode.UniqName.ToUpper()).ToList().FirstOrDefault();
                            }
                            else
                            {
                                node.IsSelected = true;
                            }
                        }

                    }
                }
                else
                {
                    var allNodeList = Enumerable.OfType<JCHNode>(this.addflow.Items).ToArray();
                    foreach (var node in allNodeList)
                    {
                        var selectedNode = node.ImageData;
                        if (selectedNode != null)
                        {
                            if (selectedNode.UniqName == imgData.UniqName)
                            {
                                BitmapImage bitmap = new BitmapImage(new Uri(imgData.imagePath));
                                ImageSource img = bitmap;
                                node.Image = img;
                                setNodeStyle(node, imgData);
                                node.ImageData = imgData;

                                ((NextGenModel.MyNodeIn)node).RoomIndooItem = projectLegacy.RoomIndoorList.Where(x => x.IndoorName.ToUpper() == selectedNode.UniqName.ToUpper()).ToList().FirstOrDefault();

                                break;
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void BindProductType()
        {
            MyProductTypeBLL productTypeBll = new MyProductTypeBLL();
            DataTable dt = productTypeBll.GetProductTypeData(projectLegacy.BrandCode, projectLegacy.SubRegionCode);
            if (dt != null && dt.Rows.Count > 0)
            {
                projectLegacy.ProductType = Convert.ToString(dt.Rows[0]["ProductType"]);
            }
        }

        private NextGenBLL.PipingBLL GetPipingBLLInstance()
        {
            this.projectLegacy = JCHVRF.Model.Project.GetProjectInstance;
            string ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
            string ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
            string ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
            string ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
            string ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
            string ut_dimension = SystemSetting.UserSetting.unitsSetting.settingDimension;

            bool isInch = CommonBLL.IsDimension_inch();
            NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            return new NextGenBLL.PipingBLL(JCHVRF.Model.Project.GetProjectInstance, utilPiping, null, isInch,
                ut_weight, ut_length, ut_power);
        }

        private void setNodeStyle(WL.Node node, NextGenModel.ImageData data)
        {
            if (data.equipmentType != "Pipe")
            {
                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                string CapacityUnits = pipBll.CapacityUnits;
                node.Geometry = new RectangleGeometry(new Rect(0, 0, 125, 100), 3, 3);
                node.Stroke = new SolidColorBrush(Color.FromRgb(185, 185, 185)); // System.Windows.Media.Brushes.Gray;
                node.StrokeThickness = 2;
                if(node.Links==null || node.Links.Count==0)
                node.DashStyle = DashStyles.Dash;
                node.Fill = System.Windows.Media.Brushes.White;
                //node.Text = data.imageName + "\n" + data.coolingCapacity;
                node.Text = data.imageName + "\n" + Unit.ConvertToControl(Convert.ToDouble(data.coolingCapacity), UnitType.POWER, CapacityUnits).ToString("n1") + CapacityUnits + "/" +
                            Unit.ConvertToControl(Convert.ToDouble(data.sensibleHeat), UnitType.POWER, CapacityUnits).ToString("n1") + CapacityUnits + "/" +
                            Unit.ConvertToControl(Convert.ToDouble(data.heatingCapacity), UnitType.POWER, CapacityUnits).ToString("n1") +
                            CapacityUnits; //data.imageName + "\n" + data.coolingCapacity; 
                //node.Text = data.imageName + "\n" + data.coolingCapacity.ToString("n1") + CapacityUnits + "/" +
                //            data.sensibleHeat.ToString("n1") + CapacityUnits + "/" +
                //            data.heatingCapacity.ToString("n1") +
                //            CapacityUnits; //data.imageName + "\n" + data.coolingCapacity;

                node.ImageMargin = new Thickness(3);
                node.TextMargin = new Thickness(3);
                node.ImagePosition = WL.ImagePosition.CenterTop;
                node.TextPosition = WL.TextPosition.CenterBottom;
                node.IsXSizeable = false; //test resize
                node.IsYSizeable = false; //test resize
            }
            else
            {
                setNodeStyleForPipes(node, data);
            }

            addflow.CanDrawNode = false;
            addflow.CanDrawLink = CurrentSystem != null && CurrentSystem.IsManualPiping;// false;
            node.FontSize = 9;
            //node.Tooltip = data.equipmentType + " : " + data.imageName;
            node.IsEditable = false; //issue fix 1653
        }

        private void setNodeStyle(WL.Node node, NextGenModel.ImageData data, bool isManualPiping)
        {
            if (data.equipmentType != "Pipe")
            {
                node.Geometry = new RectangleGeometry(new Rect(0, 0, 100, 70), 3, 3);
                // node.Stroke = System.Windows.Media.Brushes.RoyalBlue;
                node.Stroke = new SolidColorBrush(Color.FromRgb(185, 185, 185)); // System.Windows.Media.Brushes.Gray;
                node.StrokeThickness = 2;
                if (node.Links == null || node.Links.Count == 0)
                    node.DashStyle = DashStyles.Dash;
                node.Fill = System.Windows.Media.Brushes.White;
                node.Text = data.imageName + "\n" + data.coolingCapacity;
                node.ImageMargin = new Thickness(3);
                node.TextMargin = new Thickness(3);
                node.ImagePosition = WL.ImagePosition.CenterTop;
                node.TextPosition = WL.TextPosition.CenterBottom;
                node.IsXSizeable = false; //test resize
                node.IsYSizeable = false; //test resize                

                //Pin style              
                if (data.equipmentType.Equals("Outdoor"))
                {
                    node.PinsLayout = new PointCollection {new Point(50, 100)};
                }
                else
                {
                    node.PinsLayout = new PointCollection {new Point(50, 0)};
                }

                SetPinFillStyle();
                node.OutConnectionMode = WL.ConnectionMode.Pin;
                node.InConnectionMode = WL.ConnectionMode.Pin;
                if (isManualPiping)
                {
                    //node.DashStyle = DashStyles.Dash;
                }
                else
                {
                    addflow.LinkModel.Stroke = System.Windows.Media.Brushes.Black;
                    addflow.LinkModel.StrokeThickness = 1;
                }
            }
            else
            {
                setNodeStyleForPipes(node, data);
            }

            addflow.CanDrawNode = false;
            addflow.CanDrawLink = true;
            node.FontSize = 9;
            node.IsSelectable = true;
            node.IsEditable = false; //issue fix 1653
        }

        private void SetPinFillStyle()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            string imageFullPath = sourceDir + "\\" + "PinFill.png";

            ImageBrush brush = new ImageBrush();
            var bitmap = new drawing.Bitmap(imageFullPath);
            var bitmapImage = BitmapToImageSource(bitmap);
            brush.ImageSource = bitmapImage;

            addflow.PinFill = brush;
            addflow.PinShape = WL.PinShape.Circle;
            addflow.PinSize = 12;
        }


        private void setNodeStyleForPipes(WL.Node node, NextGenModel.ImageData data)
        {
            var svgFilePath = data.imagePath.Replace(".png", ".svg");
            if (!File.Exists(svgFilePath))
            {
                return;
            }

            var svgParser = new SvgParser(svgFilePath);
            var svgData = svgParser.GetSvgData();
            if (svgData == null)
            {
                return;
            }

            int width = (int) svgData.SvgSize.Width;
            int height = (int) svgData.SvgSize.Height;
            var geometry = svgData.GeometryPath;
            var colorCode = svgData.ColorText;
            node.Geometry = Geometry.Parse(geometry);
            node.Size = new System.Windows.Size(width, height);
            node.Image = null;
            var brush = (SolidColorBrush) (new BrushConverter().ConvertFrom(colorCode));
            node.Fill = brush;
            node.PinsLayout = new PointCollection() { new Point(0, 20), new Point(40, 100), new Point(100, 20) };
            node.Text = string.Empty;
            addflow.PinSize = 10;
            node.InConnectionMode = WL.ConnectionMode.Pin;
            node.OutConnectionMode = WL.ConnectionMode.Pin;
        }

        #region add new methods take  after latest code

        public bool ExportVictorGraph(string filePath, bool isBackWhite = false)
        {
            try
            {
                WL.AddFlow addFlowItem = addflow;

                NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF) _currentSystem;
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(900, 900);

                System.Drawing.Graphics gs = System.Drawing.Graphics.FromImage(bmp);
                System.Drawing.Imaging.Metafile mf = new System.Drawing.Imaging.Metafile(filePath, gs.GetHdc());

                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(mf);

                string PipingImageDir = GetImagePathPiping();

                DrawEMF_PipingNextGen(g, addFlowItem, sysItem, PipingImageDir, this.projectLegacy);
                g.ResetTransform();
                g.Save();
                g.Flush();
                g.Dispose();
                gs.Dispose();
                mf.Dispose();
                // emf.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ExportVictorGraph_wiring(string filePath, bool isBackWhite = false)
        {
            WL.AddFlow addFlowItem = addflow;
            NextGenModel.SystemVRF sysItem = (NextGenModel.SystemVRF) _currentSystem;
            try
            {

                string imageDir = GetImagePathWiring();
                drawing.Bitmap bmp = new drawing.Bitmap(875, 348);
                drawing.Graphics gs = drawing.Graphics.FromImage(bmp);
                Metafile mf = new Metafile(filePath, gs.GetHdc());
                drawing.Graphics g = drawing.Graphics.FromImage(mf);
                if (isBackWhite)
                {
                    g.Clear(System.Drawing.Color.White);
                }

                DrawEMF_wiringNextGen(g, addFlowItem, sysItem, imageDir);
                g.Save();
                g.Dispose();
                gs.Dispose();
                mf.Dispose();
                //emf.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // TODO need to Club all the 3 methods(below) to make generic method
        private string GetImagePathWiring()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            // string navigateToFolder = defaultFolder+ "NVRF\\NodeImageWiring\\";
            string navigateToFolder = "..\\..\\Report\\NodeImageWiring\\";
            //string navigateToFolder= "..\\..\\Image\\NodeImageWiring\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }


        private string GetImagePathPiping()
        {
            //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            //string navigateToFolder = defaultFolder+ "NVRF\\NodeImagePiping\\";
            //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            //return sourceDir;

            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            //string navigateToFolder = "..\\..\\Report\\NodeImagePiping\\";
            string navigateToFolder = "..\\..\\Image\\TypeImages\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        private string GetImagePathOutdoor()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            // string navigateToFolder = defaultFolder+ "NVRF\\NodeImageWiring\\";
            string navigateToFolder = "..\\..\\Image\\TypeImageProjectCreation";
            //string navigateToFolder= "..\\..\\Image\\NodeImageWiring\\";
            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }
        //public void DrawEMF_Piping(Graphics g, AddFlow addFlowPiping, NextGenModel.SystemVRF curSystemItem, string PipingImageDir, JCHVRF.Model.Project thisProject)
        //{
        //    NextGenBLL.PipingBLL pipBll = new NextGenBLL.PipingBLL(thisProject);
        //    bool isHitachi = thisProject.BrandCode == "H";
        //    bool isInch = CommonBLL.IsDimension_inch();
        //    NextGenBLL.UtilEMF utilEMF = new NextGenBLL.UtilEMF();
        //    // bool isVertical = this.uc_CheckBox_PipingVertical.Checked;
        //    utilEMF.utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH;//设置长度单位 add by axj 20161229
        //    utilEMF.isManualLength = curSystemItem.IsInputLengthManually;//设置是否输入管长 add by axj 20161229
        //    string title = "Additional Refrigerant Charge";//添加追加冷媒title add by axj 20161229
        //    foreach (Item item in addFlowPiping.Items)
        //    {
        //        if (item is Node)
        //        {
        //            Node nd = item as Node;
        //            if (nd is NextGenModel.MyNodeYP)
        //            {
        //                if (!(nd as NextGenModel.MyNodeYP).IsCP)
        //                {
        //                    utilEMF.DrawYP(g, nd, false);
        //                }
        //                else
        //                {
        //                    utilEMF.DrawYP(g, nd, true);
        //                }
        //            }
        //            else if (nd is NextGenModel.MyNodeOut)
        //            {
        //                // Draw the YP model inside the outdoor unit combination and the pipe diameter data
        //                NextGenModel.MyNodeOut ndOut = nd as NextGenModel.MyNodeOut;
        //                //string outModel = curSystemItem.OutdoorItem.Model;
        //                string outModel = curSystemItem.OutdoorItem.Model_York; //According to the PM requirements, the outdoor unit and the extension will display Model_York or Model_Hitachi 20180214 by Yunxiao Lin.
        //                                                                        //JTWH 室外机需要做SizeUp计算 add on 20160615 by Yunxiao Lin
        //                NextGenModel.NodeElement_Piping itemOut = pipBll.GetPipingNodeOutElement(curSystemItem, isHitachi);
        //                string nodeImageFile = PipingImageDir + itemOut.Name + ".txt";

        //                if (itemOut.UnitCount == 1)
        //                {
        //                    if (isHitachi)
        //                        outModel = curSystemItem.OutdoorItem.Model_Hitachi;
        //                    //uncomment that code also
        //                    utilEMF.DrawNode(g, nd, nodeImageFile);
        //                }
        //                else
        //                {
        //                    outModel = curSystemItem.OutdoorItem.AuxModelName;
        //                    //uncomment that code also
        //                    utilEMF.DrawNode_OutdoorGroup(g, nd, nodeImageFile, outModel, curSystemItem.Name, itemOut, isInch);
        //                }
        //            }
        //            else if (nd is NextGenModel.MyNodeIn)
        //            {
        //                NextGenModel.MyNodeIn ndIn = nd as NextGenModel.MyNodeIn;

        //                NextGenModel.NodeElement_Piping itemInd = pipBll.GetPipingNodeIndElement(ndIn.RoomIndooItem.IndoorItem.Type, isHitachi);
        //                string nodeImageFile = PipingImageDir + itemInd.Name + ".txt";
        //                utilEMF.DrawNode(g, nd, nodeImageFile);
        //            }
        //            else if (nd is NextGenModel.MyNodeCH)
        //            {
        //                NextGenModel.MyNodeCH ndCH = nd as NextGenModel.MyNodeCH;
        //                string nodeImageFile = PipingImageDir + "CHbox.txt";
        //                utilEMF.DrawNode(g, nd, nodeImageFile);
        //            }
        //            else if (nd is NextGenModel.MyNodeMultiCH)
        //            {
        //                NextGenModel.MyNodeMultiCH ndMCH = nd as NextGenModel.MyNodeMultiCH;
        //                string nodeImageFile = PipingImageDir + "MultiCHbox.txt";
        //                utilEMF.DrawNode(g, nd, nodeImageFile);
        //            }
        //            else if (nd is NextGenModel.MyNodeLegend)
        //            {
        //                //绘制图例文字
        //                //uncomment also
        //                utilEMF.DrawText(g, nd);
        //            }
        //            else if (nd.Tooltip == title)//添加追加冷媒文字 add by axj 20161229
        //            {
        //                utilEMF.DrawText(g, nd);
        //            }
        //            else if (nd.Text != null && nd.Text.Contains("Cooling") && nd.Text.Contains("Heating"))//添加实际容量文字 add by axj 20161229
        //            {
        //                //uncomment code
        //                utilEMF.DrawActualCapacityText(g, nd);
        //            }
        //            else
        //            {
        //                //uncomment code
        //                utilEMF.DrawLabelText(g, nd);
        //            }
        //        }
        //        else if (item is Link)
        //        {
        //            //uncomment code
        //            utilEMF.DrawLine(g, (Link)item);
        //        }
        //    }
        //}


        #region add new methods take  after latest code 


        private void DrawEMF_wiringNextGen(drawing.Graphics g, WL.AddFlow addFlowWiring,
            NextGenModel.SystemVRF curSystemItem, string WiringImageDir)
        {
            try
            {
                Wiring WiringPage = new Wiring(this.projectLegacy, this.projectLegacy.SystemListNextGen[0], addflow);
                addFlowWiring = WiringPage.GetAddFlowInstance();


                JCHVRF.Model.Project thisProject = this.projectLegacy;
                drawing.PointF ptf1 = new drawing.PointF(0, 0);
                drawing.PointF ptf2 = new drawing.PointF(0, 0);
                drawing.PointF ptf3 = new drawing.PointF(0, 0);
                NextGenBLL.UtilEMF utilEMF = new NextGenBLL.UtilEMF();

                //try
                //{   
                foreach (WL.Item item in addFlowWiring.Items)
                {
                    if (item is WL.Node)
                    {
                        WL.Node nd = item as WL.Node;
                        if (nd is NextGenModel.WiringNodeOut)
                        {
                            NextGenModel.WiringNodeOut ndOut = nd as NextGenModel.WiringNodeOut;
                            NodeElement_Wiring item_wiring =
                                utilPiping.GetNodeElement_Wiring_ODUNextGen(curSystemItem.OutdoorItem,
                                    thisProject.BrandCode);
                            string nodePointsFile =
                                System.IO.Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                            //apply per new nextgen class
                            utilEMF.DrawNode_wiringNextGen(g, ndOut, nodePointsFile, curSystemItem.Name, item_wiring);
                        }
                        else if (nd is NextGenModel.WiringNodeIn)
                        {

                            NextGenModel.WiringNodeIn ndIn = nd as NextGenModel.WiringNodeIn;
                            List<string> strArrayList_powerType = new List<string>();
                            List<string> strArrayList_powerVoltage = new List<string>();
                            List<double> dArrayList_powerCurrent = new List<double>();

                            int powerIndex = 0;
                            bool isNewPower = false;
                            NodeElement_Wiring item_wiring = utilPiping.GetNodeElement_Wiring_IDUNextGen(
                                ndIn.RoomIndoorItem.IndoorItem, thisProject.BrandCode, curSystemItem.OutdoorItem.Type,
                                ref strArrayList_powerType, ref strArrayList_powerVoltage, ref dArrayList_powerCurrent,
                                ref powerIndex, ref isNewPower);
                            //string nodePointsFile = FileLocal.GetNodeImageTextWiring(item_wiring.KeyName);
                            string nodePointsFile =
                                System.IO.Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                            //apply as per new nextgen class
                            utilEMF.DrawNode_wiringNextGen(g, ndIn, nodePointsFile, ndIn.RoomIndoorItem.IndoorName,
                                item_wiring);
                        }
                        else if (nd is NextGenModel.WiringNodeCH)
                        {
                            //WiringNodeCH ndCH = nd as WiringNodeCH;
                            //NodeElement_Wiring item_wiring = utilEMF.GetNodeElement_Wiring_CH(ndCH);
                            //string nodePointsFile = Path.Combine(WiringImageDir, item_wiring.KeyName + ".txt");
                            //utilEMF.DrawNode_wiring(g, ndCH, nodePointsFile, "CH Unit", item_wiring);
                            //apply as per new next gen class
                            utilEMF.DrawNodeTextWiringCHbox(g, nd);
                        }
                        else if (nd is NextGenModel.MyNodeGround_Wiring)
                        {
                            NextGenModel.MyNodeGround_Wiring ndGnd = nd as NextGenModel.MyNodeGround_Wiring;
                            string nodePointsFile = System.IO.Path.Combine(WiringImageDir, "Ground.txt");
                            //aaply as per new next gen class
                            utilEMF.DrawNode_wiringNextGen(g, ndGnd, nodePointsFile, "", null);

                        }
                        else if (nd is NextGenModel.MyNodeRemoteControler_Wiring)
                        {

                            NextGenModel.MyNodeRemoteControler_Wiring ndRC =
                                nd as NextGenModel.MyNodeRemoteControler_Wiring;
                            string nodePointsFile = System.IO.Path.Combine(WiringImageDir, "RemoteControler.txt");
                            utilEMF.DrawNode_wiringNextGen(g, ndRC, nodePointsFile, "", null);
                        }
                        else
                        {

                            if (!string.IsNullOrEmpty(nd.Text))
                            {

                                drawing.PointF pf = new drawing.PointF(Convert.ToInt64(nd.Location.X),
                                    Convert.ToInt64(nd.Location.Y));

                                //g.DrawString(nd.Text, utilEMF.textFont_wiring, utilEMF.textBrush_wiring, pf);
                                if (nd.Text != "//" && nd.Text != "///" && nd.Text != "////")
                                {
                                    g.DrawString(nd.Text, utilEMF.textFont_wiring, utilEMF.textBrush_wiring, pf);
                                }
                                else
                                {
                                    System.Drawing.Brush brush = new System.Drawing.SolidBrush(drawing.Color.Red);
                                    pf.Y += 2.5f;
                                    g.DrawString(nd.Text, utilEMF.textFont_wiring, brush, pf);
                                }
                            }
                        }
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //    LogHelp.WriteLog(n.ToString()+"  "+ex.Message, ex);
                //}

                var links = addFlowWiring.Items.OfType<WL.Link>().ToArray();
                foreach (var item in links)
                {
                    utilEMF.DrawLine(g, item);
                }

                //foreach (PointF[] pt in ptArrayList)
                //{
                //    //drawing.Pen pen = new drawing.Pen(drawing.Color.Black, 0.1f);
                //    //g.DrawLines(pen, pt);
                //    //g.DrawLines(utilEMF.pen_wiring, pt);
                //}
                List<drawing.PointF[]> ptArrayList_power = new List<drawing.PointF[]>();

                List<drawing.PointF[]> ptArrayList_ground = new List<drawing.PointF[]>();
                /// <summary>
                /// 室内机总电源线坐标列表
                /// </summary>
                List<drawing.PointF[]> ptArrayList_mainpower = new List<drawing.PointF[]>();
                foreach (drawing.PointF[] pt in ptArrayList_power)
                {
                    drawing.Pen pen = new drawing.Pen(drawing.Color.Red, 0.3f);
                    g.DrawLines(pen, pt);
                    //g.DrawLines(utilEMF.pen_wiring_power, pt); 
                }

                foreach (drawing.PointF[] pt in ptArrayList_ground)
                {
                    drawing.Pen pen = new drawing.Pen(drawing.Color.Yellow, 0.1f);
                    drawing.PointF[] pt1 = pt;
                    if (pt.Length > 2)
                    {
                        pt1 = new drawing.PointF[] {pt[0], pt[1]};
                    }

                    g.DrawLines(pen, pt1);
                    //g.DrawLines(utilEMF.pen_wiring_dash, pt); 
                }
            }
            catch (Exception)
            {

            }
        }

        /// 绘制 EMF 图片(Piping)
        /// <summary>
        /// 绘制 EMF 图片(Piping)
        /// </summary>
        /// <param name="addFlow1"></param>
        /// <param name="g"></param>
        public void DrawEMF_PipingNextGen(drawing.Graphics g, WL.AddFlow addFlowPiping,
            NextGenModel.SystemVRF curSystemItem, string PipingImageDir, JCHVRF.Model.Project thisProject)
        {
            NextGenBLL.PipingBLL pipBll = new NextGenBLL.PipingBLL(thisProject);
            bool isHitachi = thisProject.BrandCode == "H";
            bool isInch = CommonBLL.IsDimension_inch();
            // bool isVertical = this.uc_CheckBox_PipingVertical.Checked;
            NextGenBLL.UtilEMF utilEMF = new NextGenBLL.UtilEMF();
            utilEMF.utLength = SystemSetting.UserSetting.unitsSetting.settingLENGTH; //设置长度单位 add by axj 20161229
            utilEMF.isManualLength = curSystemItem.IsInputLengthManually; //设置是否输入管长 add by axj 20161229
            string title = "Additional Refrigerant Charge"; //添加追加冷媒title add by axj 20161229

            var nodes = addFlowPiping.Items.OfType<WL.Node>().ToArray();
            var links = addFlowPiping.Items.OfType<WL.Link>().ToArray();

            foreach (var item in nodes)
            {
                if (item is NextGenModel.MyNodeYP)
                {
                    if (!(item as NextGenModel.MyNodeYP).IsCP)
                    {
                        utilEMF.DrawYPNextGen(g, item, false);
                    }
                    else
                    {
                        utilEMF.DrawYPNextGen(g, item, true);
                    }
                }
                else if (item is NextGenModel.MyNodeOut)
                {
                    //Draw the YP model inside the outdoor unit combination and the pipe diameter data
                    NextGenModel.MyNodeOut ndOut = item as NextGenModel.MyNodeOut;
                    //string outModel = curSystemItem.OutdoorItem.Model;
                    string outModel = curSystemItem.OutdoorItem.Model_York;
                    string nodeImageFile = ((System.Windows.Media.Imaging.BitmapImage) ndOut.Image).UriSource
                        .OriginalString;

                    if (isHitachi)
                    {
                        outModel = curSystemItem.OutdoorItem.Model_Hitachi;
                    }

                    // outModel = "RAS-14-18FSNSE";
                    utilEMF.DrawNodeNextGenPiping(g, item, nodeImageFile);

                }
                else if (item is NextGenModel.MyNodeIn)
                {
                    NextGenModel.MyNodeIn ndIn = item as NextGenModel.MyNodeIn;

                    string nodeImageFile = ((System.Windows.Media.Imaging.BitmapImage) ndIn.Image).UriSource
                        .OriginalString;
                    utilEMF.DrawNodeNextGenPiping(g, item, nodeImageFile);
                }
                else if (item is NextGenModel.MyNodeCH)
                {
                    NextGenModel.MyNodeCH ndCH = item as NextGenModel.MyNodeCH;
                    string nodeImageFile = ((System.Windows.Media.Imaging.BitmapImage) ndCH.Image).UriSource
                        .OriginalString;
                    utilEMF.DrawNodeNextGenPiping(g, item, nodeImageFile);
                }
                else if (item is NextGenModel.MyNodeMultiCH)
                {
                    NextGenModel.MyNodeMultiCH ndMCH = item as NextGenModel.MyNodeMultiCH;
                    string nodeImageFile = PipingImageDir + "MultiCHbox.png";
                    utilEMF.DrawNodeNextGenPiping(g, item, nodeImageFile);
                }
                else if (item is NextGenModel.MyNodeLegend)
                {
                    //绘制图例文字
                    utilEMF.DrawText(g, item);
                }
                else if (item.Tooltip == title)
                {
                    utilEMF.DrawText(g, item);
                }
                else if (item.Text != null && item.Text.Contains("Cooling") && item.Text.Contains("Heating"))
                {
                    utilEMF.DrawActualCapacityText(g, item);
                }
                else
                {
                    utilEMF.DrawLabelText(g, item);
                }
            }

            foreach (var item in links)
            {
                utilEMF.DrawLine(g, item);
            }
        }




        #endregion new methods 

        #endregion new methods add after latest

        /// <summary>
        /// Check Outdoor Equipment count in Canvas
        /// </summary>
        /// <returns></returns>
        private bool CheckOutdoorCount()
        {
            var AllNodeList = Enumerable.OfType<JCHNode>(addflow.Items).ToArray();
            int OutNodeCount = 0;
            foreach (JCHNode a in AllNodeList)
            {
                if (a.ImageData.equipmentType == "Outdoor")
                {
                    OutNodeCount = OutNodeCount + 1;
                }
            }

            if (OutNodeCount > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SelectedNode"></param>
        /// 
        public void DoDrawingPiping(bool reset, JCHVRF.Model.NextGen.SystemVRF CurrentSystem,
            ref WL.AddFlow addflowsCurrentContext)
        {
            isWiringViewOpen = false;
            if (CurrentSystem.IsManualPiping) return;
            //nodeBackgroundColor = Colors.White;
            addflow = AutoPipingObj.DoDrawingPiping(reset, CurrentSystem, addflow);
            AddBuildingImage(CurrentSystem.AddFlowBackgroundImage);
            addflow.CanDrawNode = false;
            foreach (var node in addflow.Items.OfType<JCHNode>())
            {
                if(node is NextGenModel.MyNodeIn || node is NextGenModel.MyNodeOut) { 
                node.Fill = new SolidColorBrush(nodeBackgroundColor);
                }              
                DrawMandatoryNodeVisuals(node);
            }

            foreach(var node in addflow.Items.OfType<NextGenModel.MyLink>())
            {
                node.Fill = new SolidColorBrush(pipeColor);
                node.Stroke = new SolidColorBrush(pipeColor);
            }

            this.ScrollViewer.Content = addflow;
            //Start: Bug#1646
            lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Transparent);
            lblCanvasError.BorderThickness = new Thickness(0);
            lblCanvasError.Content = string.Empty;

            if (CurrentSystem.IsValid)
            {
                CurrentSystem.MyPipingOrphanNodes?.Clear();
                CurrentSystem.MyPipingOrphanNodesTemp?.Clear();
            }
                
            //End: Bug#1646


            //addflow.Clear();
            //NextGenBLL.UtilPiping utilPiping = new NextGenBLL.UtilPiping();
            //NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            //bool isHitachi = projectLegacy.BrandCode == "H";
            //bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(CurrentSystem);
            ////string dir = GetBinDirectoryPath(ConfigurationManager.AppSettings["PipingNodeImageDirectory"].ToString());
            ////TO DO Pick VRF system in case of multi system
            //string dir = GetImagePathPiping();
            //NextGenModel.MyNodeOut pipingNodeOut = CurrentSystem.MyPipingNodeOut;
            //if (pipingNodeOut == null || CurrentSystem.OutdoorItem == null)
            //{
            //    return;
            //}
            //if (pipingNodeOut.ChildNode == null)
            //{
            //    return;
            //}
            //if (isHR)
            //{
            //    //SetAllNodesIsCoolingonlyFrom();
            //    pipBll.SetIsCoolingOnly(CurrentSystem.MyPipingNodeOut);
            //}
            //if (!reset)
            //{
            //    utilPiping.ResetColors();
            //    InitAndRemovePipingNodes(ref addflow);
            //    pipBll.DrawPipingNodes(CurrentSystem, dir, ref addflow);
            //    pipBll.DrawPipingLinks(CurrentSystem, ref addflow);
            //    pipBll.DrawLegendText(CurrentSystem, ref addflow);
            //}
            //if (reset)
            //{
            //    CurrentSystem.IsManualPiping = false;
            //    utilPiping.ResetColors();
            //    InitAndRemovePipingNodes(ref addflow);
            //    pipBll.DrawPipingNodes(CurrentSystem, dir, ref addflow);
            //    pipBll.DrawPipingLinks(CurrentSystem, ref addflow);
            //    pipBll.DrawLegendText(CurrentSystem, ref addflow);
            //    //pipBll.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem, ref addflows);
            //}
            //else
            //{
            //    if (CurrentSystem.IsManualPiping)
            //    {
            //        //utilPiping.colorDefault = CurrentSystem.MyPipingNodeOutTemp.pipeColor;
            //        //utilPiping.colorText = CurrentSystem.MyPipingNodeOutTemp.textColor;
            //        //utilPiping.colorYP= CurrentSystem.MyPipingNodeOutTemp.branchKitColor;
            //        //utilPiping.colorNodeBg = CurrentSystem.MyPipingNodeOutTemp.nodeBgcolor;
            //    }
            //    else
            //    {
            //        utilPiping.ResetColors();
            //    }
            //    pipBll.DrawPipingNodesNoCaculation(dir, CurrentSystem);
            //}
            //if (CurrentSystem.IsPipingOK)
            //{

            //    if (CurrentSystem.IsInputLengthManually && CurrentSystem.IsPipingOK)
            //    {
            //        pipBll.DrawAddRefrigerationText(CurrentSystem);
            //    }
            //    pipBll.drawPipelegend(isHR);
            //    pipBll.SetDefaultColor(ref addflow, isHR);               
            //}            
            //this.ScrollViewer.Content = addflow;
            ////Start: Bug#1646
            //lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Transparent);
            //lblCanvasError.BorderThickness = new Thickness(0);
            //lblCanvasError.Content = string.Empty;
            ////End: Bug#1646
        }

        public void DoFindZeroLength()
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;
            var CurrentSystem = (NextGenModel.SystemVRF) ucDesignerCanvasViewModel.CurrentSystem;
            if (CurrentSystem == null)
            {
                if (WorkFlowContext.CurrentSystem != null)
                {
                    if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                    {
                        CurrentSystem = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
                    }
                }
            }

            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            int totalZeroLengthPipes = addflow.Items.OfType<NextGenModel.MyLink>().Count(a => a.Length.Equals(0));
            bool AllHaveLengthGreaterThan0 = addflow.Items.OfType<NextGenModel.MyLink>().All(a => a.Length > 0);
            if (AllHaveLengthGreaterThan0)
            {
                return;
            }

            if (CurrentSystem.IsInputLengthManually == true)
            {
                SetSystemPipingOK(CurrentSystem, false);
            }

            if (CurrentSystem.IsManualPiping)
            {
                DoDrawingPiping(false, CurrentSystem, ref addflow);
            }
            else
            {
                DoDrawingPiping(true, CurrentSystem, ref addflow);
                UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
                ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
            }

            NextGenBLL.PipingErrors errorType = pipBll.ValidatePipeLength(CurrentSystem, ref addflow);
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                NextGenModel.MyLink errorLink = addflow.Items.OfType<NextGenModel.MyLink>()
                    .FirstOrDefault(a => a.Stroke == System.Windows.Media.Brushes.Red);
                if (errorLink != null)
                {
                    OpenPopupToSetPipeLength(errorLink, totalZeroLengthPipes);
                }
            }
        }

        private NextGenBLL.PipingErrors ClosePopupWindow()
        {
            foreach (var frmFindZeroLength in System.Windows.Application.Current.Windows.OfType<Views.FindZeroLength>())
            {
                frmFindZeroLength.Close();
            }

            foreach (var inputPipeLength in System.Windows.Application.Current.Windows
                .OfType<Views.InputPipeLengthPopup>())
            {
                inputPipeLength.Close();
            }
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            //Required for Equivalent Length Check
            NextGenBLL.PipingErrors errorType;
            DoPipingCalculation(pipBll, CurrentSystem.MyPipingNodeOut, CurrentSystem, out errorType);
            return pipBll.ValidatePipeLength(CurrentSystem, ref addflow);
        }

        private void OpenPopupToSetPipeLength(NextGenModel.MyLink errorLink, int totalZeroLengthPipes)
        {
            ClosePopupWindow();
            if (errorLink != null)
            {
                double pipeLength = errorLink.Length;
                FindZeroLength Find0LengthWizard = new Views.FindZeroLength(pipeLength, totalZeroLengthPipes);
                Find0LengthWizard.Owner = System.Windows.Application.Current.MainWindow;
                Find0LengthWizard.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Find0LengthWizard.ShowDialog();
            }
        }

        public void SetClosePopupFindZeroLength(List<string> pipingData)
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;
            var CurrentSystem = (NextGenModel.SystemVRF) ucDesignerCanvasViewModel.CurrentSystem;

            if (CurrentSystem == null)
            {
                if (WorkFlowContext.CurrentSystem != null)
                {
                    if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                    {
                        CurrentSystem = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
                    }
                }
            }

            CurrentSystem.IsInputLengthManually = true;
            UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
            Lassalle.WPF.Flow.Node node;
            NextGenModel.MyNode myNode = new NextGenModel.MyNode();
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            double pipeLength = Convert.ToDouble(pipingData[0]);
            int applyToAll = Convert.ToInt32(pipingData[1]);
            
                NextGenModel.MyLink mylink = addflow.Items.OfType<NextGenModel.MyLink>()
                    .FirstOrDefault(a => a.Stroke.Equals(System.Windows.Media.Brushes.Red));
            if (mylink != null)
            {
                node = mylink.Dst;
                myNode = node as NextGenModel.MyNode;
            }
            else
            {
                NextGenModel.MyLink selectedLink =
                    addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected);
                if (selectedLink != null)
                {
                    node = selectedLink.Dst;
                    myNode = node as NextGenModel.MyNode;
                }
                else
                {
                    return;
                }
            }

            if (applyToAll == 1)
            {
                pipBll.SetPipeLength(pipeLength, ref addflow);
            }
            else
            {
                foreach (NextGenModel.MyLink lk in myNode.MyInLinks)
                {
                    if (lk is NextGenModel.MyLink)
                    {
                        NextGenModel.MyLink myLnk = lk as NextGenModel.MyLink;
                        myLnk.Length = pipeLength;
                    }
                }
            }

            if (CurrentSystem.IsInputLengthManually == true)
            {
                SetSystemPipingOK(CurrentSystem, false);
            }

            if(CurrentSystem!=null){
                 if(CurrentSystem.MyPipingNodeOut != null) { 
            NextGenBLL.PipingErrors errorType;
            DoPipingCalculation(pipBll, CurrentSystem.MyPipingNodeOut, CurrentSystem, out errorType);
                    
                }
            }
            if (!CurrentSystem.IsManualPiping)
            {

                    DoDrawingPiping(true, CurrentSystem, ref addflow);
                    if (pipBll.ValidatePipeLength(CurrentSystem, ref addflow) == NextGenBLL.PipingErrors.OK)
                    {
                        AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                        JCHVRF.Model.NextGen.SystemVRF TempSystem = CurrentSystem.DeepCopy();
                        AutoSelectODUModel result = SelectODU.ReselectOutdoor(TempSystem, JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                        if(result.SelectionResult == SelectOutdoorResult.OK)
                        {
                            CurrentSystem = TempSystem;
                            DoDrawingPiping(true, CurrentSystem, ref addflow);
                        }
                    }

                ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
            }

            if (ClosePopupWindow() != NextGenBLL.PipingErrors.OK)
            {
                DoFindZeroLength();
            }
        }

        public void SetClosePopupInputPipeLengthOnNext(List<string> pipingData)
        {
            try
            {
                int linkOrder = 0;
                var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel)this.DataContext;
                var CurrentSystem = (NextGenModel.SystemVRF)ucDesignerCanvasViewModel.CurrentSystem;

                if (CurrentSystem == null)
                {
                    if (WorkFlowContext.CurrentSystem != null)
                    {
                        if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                        {
                            CurrentSystem = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
                        }
                    }
                }
                CurrentSystem.IsInputLengthManually = true;
                UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
                Lassalle.WPF.Flow.Node node;
                NextGenModel.MyNode myNode = new NextGenModel.MyNode();
                NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
                double pipeLength = Convert.ToDouble(pipingData[0]);
                double oilTrapQty = Convert.ToDouble(pipingData[1]);
                double elbowQty = Convert.ToDouble(pipingData[2]);
                int applyToAll = Convert.ToInt32(pipingData[3]);
                NextGenModel.MyLink selectedLink = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected);
                if (selectedLink != null)
                {
                    linkOrder = selectedLink.LinkOrder;
                    node = selectedLink.Dst;
                    myNode = node as NextGenModel.MyNode;
                }
                else
                {
                    return;
                }
                if (applyToAll == 1)
                {
                    pipBll.SetAllLinks(elbowQty, oilTrapQty, pipeLength, 0, ref addflow);
                }
                else
                {
                    foreach (NextGenModel.MyLink lk in myNode.MyInLinks)
                    {
                        if (lk is NextGenModel.MyLink)
                        {
                            NextGenModel.MyLink myLnk = lk as NextGenModel.MyLink;
                            myLnk.Length = pipeLength;
                            myLnk.ElbowQty = elbowQty;
                            myLnk.OilTrapQty = oilTrapQty;

                        }
                    }
                }
                if (CurrentSystem.IsInputLengthManually == true)
                {
                    SetSystemPipingOK(CurrentSystem, false);
                }
                //Required for Equivalent Length Check
                if (CurrentSystem != null )
                {
                    if (CurrentSystem.MyPipingNodeOut != null)
                    {
                        NextGenBLL.PipingErrors errorType;
                        DoPipingCalculation(pipBll, CurrentSystem.MyPipingNodeOut, CurrentSystem, out errorType);
                        if (pipBll.ValidatePipeLength(CurrentSystem, ref addflow) == NextGenBLL.PipingErrors.OK)
                        {
                            AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                            AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSystem, JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                        }
                    }
                }
                if (!CurrentSystem.IsManualPiping)
                {
                    DoDrawingPiping(true, CurrentSystem, ref addflow);
                    ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
                }
                ClosePopupWindow();

                if (addflow.Items.OfType<NextGenModel.MyLink>().Count() > linkOrder)
                {
                  
                    addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.LinkOrder.Equals(linkOrder + 1)).IsSelected = true;
                    pipeLength = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected).Length;
                    elbowQty = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected).ElbowQty;
                    oilTrapQty = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected).OilTrapQty;
                    InputPipeLengthPopup inputPipeLengthPopup = new Views.InputPipeLengthPopup(pipeLength, elbowQty, oilTrapQty, CurrentSystem);
                    inputPipeLengthPopup.Owner = System.Windows.Application.Current.MainWindow;
                    inputPipeLengthPopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    inputPipeLengthPopup.ShowDialog();
                }
                


            }


            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }

        }
        private void DisplayLenPiping()
        {
          //  DoDrawingPiping(true, CurrentSystem, ref addflow);
            UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
            ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
        }
        public void SetClosePopupInputPipeLengthOnPrevious(List<string> pipingData)
        {
            try { 
            int linkOrder = 0;
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel)this.DataContext;
            var CurrentSystem = (NextGenModel.SystemVRF)ucDesignerCanvasViewModel.CurrentSystem;

            if (CurrentSystem == null)
            {
                if (WorkFlowContext.CurrentSystem != null)
                {
                    if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                    {
                        CurrentSystem = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
                    }
                }
            }
           if (CurrentSystem == null)
                    return;
            CurrentSystem.IsInputLengthManually = true;
            UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
            Lassalle.WPF.Flow.Node node;
            NextGenModel.MyNode myNode = new NextGenModel.MyNode();
            NextGenBLL.PipingBLL pipBll = GetPipingBLLInstance();
            double pipeLength = Convert.ToDouble(pipingData[0]);
            double oilTrapQty = Convert.ToDouble(pipingData[1]);
            double elbowQty = Convert.ToDouble(pipingData[2]);
            int applyToAll = Convert.ToInt32(pipingData[3]);
            NextGenModel.MyLink selectedLink = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected);
                if (selectedLink != null)
                {
                    linkOrder = selectedLink.LinkOrder;
                    node = selectedLink.Dst;
                    myNode = node as NextGenModel.MyNode;
                }
                else
                {
                    return;
                }
            if (applyToAll == 1)
            {
                pipBll.SetAllLinks(elbowQty, oilTrapQty, pipeLength, 0, ref addflow);
            }
            else
            {
                foreach (NextGenModel.MyLink lk in myNode.MyInLinks)
                {
                    if (lk is NextGenModel.MyLink)
                    {
                        NextGenModel.MyLink myLnk = lk as NextGenModel.MyLink;
                        myLnk.Length = pipeLength;
                        myLnk.ElbowQty = elbowQty;
                        myLnk.OilTrapQty = oilTrapQty;

                    }
                }
            }
            if (CurrentSystem.IsInputLengthManually == true)
            {
                SetSystemPipingOK(CurrentSystem, false);
            }
                if (CurrentSystem != null)
                {
                    if(CurrentSystem.MyPipingNodeOut != null) { 
                    //Required for Equivalent Length Check
                    NextGenBLL.PipingErrors errorType;
                    DoPipingCalculation(pipBll, CurrentSystem.MyPipingNodeOut, CurrentSystem, out errorType);
                    if (pipBll.ValidatePipeLength(CurrentSystem, ref addflow) == NextGenBLL.PipingErrors.OK)
                    {
                        AutoSelectOutdoor SelectODU = new AutoSelectOutdoor();
                        AutoSelectODUModel result = SelectODU.ReselectOutdoor(CurrentSystem, JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList);
                    }
                    }
                }
                if (!CurrentSystem.IsManualPiping)
                {
                    DoDrawingPiping(true, CurrentSystem, ref addflow);
                    ObjPipValidation.DrawTextToAllNodes(CurrentSystem.MyPipingNodeOut, null, CurrentSystem);
                }
                ClosePopupWindow();
                if (addflow.Items.OfType<NextGenModel.MyLink>().Count() >= linkOrder)
            {
                if (applyToAll == 1)
                    return;
                if (linkOrder > 1)
                {
                    addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.LinkOrder.Equals(linkOrder - 1)).IsSelected = true;
                    pipeLength = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected).Length;
                    elbowQty = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected).ElbowQty;
                    oilTrapQty = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected).OilTrapQty;
                    InputPipeLengthPopup inputPipeLengthPopup = new Views.InputPipeLengthPopup(pipeLength, elbowQty, oilTrapQty, CurrentSystem);
                    inputPipeLengthPopup.Owner = System.Windows.Application.Current.MainWindow;
                    inputPipeLengthPopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    inputPipeLengthPopup.ShowDialog();
                }
            }
        }

            catch(Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);

            }
        }
        private void InitAndRemovePipingNodes(ref WL.AddFlow addflows)
        {
            var Nodes = Enumerable.OfType<WL.Node>(addflows.Items).ToList();
            var Captions = Enumerable.OfType<WL.Caption>(addflows.Items).ToList();
            var Links = Enumerable.OfType<WL.Link>(addflows.Items).ToList();
            foreach (var item in Nodes)
            {
                addflows.RemoveNode(item);
            }

            foreach (var item in Captions)
            {
                addflows.RemoveCaption(item);
            }

            foreach (var item in Links)
            {
                addflows.RemoveLink(item);
            }
        }


        /// <summary>
        /// Get bin directory path for debug folder 
        /// </summary>
        /// <param name="AppSettingPath"></param>
        /// <returns></returns>
        private string GetBinDirectoryPath(string AppSettingPath)
        {
            string binDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').ToString();
            binDirectory += AppSettingPath;
            return binDirectory;
        }


        private void DeleteOutdoorEquipment(NextGenModel.ImageData SelectedNode)
        {
            if (SelectedNode.equipmentType == "Outdoor")
            {
                var AllNodeList = Enumerable.OfType<JCHNode>(addflow.Items).ToArray();
                foreach (JCHNode a in AllNodeList)
                {
                    if (a.ImageData.UniqName == SelectedNode.UniqName)
                    {
                        MessageBoxResult messageBoxResult = JCHMessageBox.Show("Are you sure ?", MessageType.Warning,
                            MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            addflow.RemoveNode(a);
                            break;
                        }
                    }
                }
            }
        }

        private void CreatePipingWiringToggleImage()
        {
            nodeWiring = new JCHNode(800, 5, 50, 50, "", addflow);
            nodePiping = new JCHNode(800, 5, 50, 50, "", addflow);

            nodeWiring.Stroke = System.Windows.Media.Brushes.Transparent;
            nodeWiring.IsXMoveable = false;
            nodeWiring.IsYMoveable = false;

            nodePiping.Stroke = System.Windows.Media.Brushes.Transparent;
            nodePiping.IsXMoveable = false;
            nodePiping.IsYMoveable = false;

            addflow.CanDrawLink = false;

            btnImagePiping = new NextGenModel.ImageData();
            btnImageWiring = new NextGenModel.ImageData();


            btnImagePiping.imageName = "Piping";
            nodePiping.Image = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Piping.png"));

            btnImageWiring.imageName = "WireChange";
            nodeWiring.Image = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Wiring.png"));

            //Add default iamges to the node in the canvas.
            nodePiping.ImageData = btnImagePiping;
            addflow.AddNode(nodePiping);
        }

        private void UcDesignerCanvasZoomIn(object sender, MouseButtonEventArgs e)
        {
            AddFlowZoomIn(addflow);
        }

        private void UcDesignerCanvasZoomOut(object sender, MouseButtonEventArgs e)
        {
            AddFlowZoomOut(addflow);
        }

        private void UcDesignerCanvasPipingWiringToggle(object sender, MouseButtonEventArgs e)
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;

            if (ucDesignerCanvasViewModel.CurrentSystem.HvacSystemType.Equals("1"))
            {
                HandleWiringForVRF(ucDesignerCanvasViewModel);
            }
            else if (ucDesignerCanvasViewModel.CurrentSystem.HvacSystemType.Equals("6"))
            {
                HandleWiringForCentralController(ucDesignerCanvasViewModel);
                if (bToggle)
                {
                    ToggleImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ToggleImage.Visibility = Visibility.Visible;
                }
            }

        }

        private Border DrawTabularButton()
        {
            TextBlock tblock = new TextBlock();
            Border Outer1 = new Border()
            {
                CornerRadius = new CornerRadius(53),
                BorderThickness = new Thickness(2),
                Background = (Brush)new BrushConverter().ConvertFrom("#dfdfdf"),
                BorderBrush = Brushes.WhiteSmoke,
                Height = 40,
                Width = 40,
                Child = new Border()
                {
                    CornerRadius = new CornerRadius(53),
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)new BrushConverter().ConvertFrom("#e9e9e9"),
                    Background = Brushes.White,
                    Child = DrawIcon()
                }
            };
            Outer1.Cursor = Cursors.Hand;
            Outer1.MouseDown += new MouseButtonEventHandler(UcDesignerCanvasPipingWiringToggle);

            return Outer1;
        }

        private UIElement DrawIcon()
        {
            StackPanel stkPanel = new StackPanel();
            Image img = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Tabular_view_Icon_CC.png")),
                Margin = new Thickness(6, 4, 5, 0),
                Height = 16,
                Width = 16
            };
            TextBlock tblock = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Width = 23,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                LineHeight = 7,
                TextAlignment = TextAlignment.Center,
                FontSize = 7,
                Text = "Tabular View"
            };
            stkPanel.Children.Add(img);
            stkPanel.Children.Add(tblock);
            return stkPanel;
        }

        //ZoomIn btn
        //private UIElement DrawZoomInButton()
        //{
        //    Image img = new Image()
        //    {
        //        Name = "ZoomIn",
        //        Source = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/ZoomIn.png")),
        //        Width = 60,
        //        Height = 60,
        //        Margin = new Thickness(0, -5, 0, 0),
        //    };
        //    img.MouseDown += new MouseButtonEventHandler(frmAddflowControl_Click);
        //    return img;
        //}

        ////ZoomOut Btn
        //private UIElement DrawZoomOutButton()
        //{
        //    Image img = new Image()
        //    {
        //        Name = "ZoomOut",
        //        Source = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/ZoomOut.png")),
        //        Width = 60,
        //        Height = 60,
        //    };
        //    img.MouseDown += new MouseButtonEventHandler(frmAddflowControl_Click);
        //    return img;
        //}

        ////Panning Btn
        //private UIElement DrawPanningButton()
        //{
        //    Image img = new Image()
        //    {
        //        Name = "Panning",
        //        Source = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Pann.png")),
        //        Width = 60,
        //        Height = 60,
        //        Margin = new Thickness(0, -5, 0, 0),
        //    };
        //    img.MouseDown += new MouseButtonEventHandler(frmAddflowControl_Click);
        //    return img;
        //}

        ////ReAlign Btn
        //private UIElement DrawReAlignButton()
        //{
        //    Image img = new Image()
        //    {
        //        Name = "ReAlign",
        //        Source = new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/ReAlign.png")),
        //        Width = 60,
        //        Height = 60,
        //        Margin = new Thickness(0, -5, 0, 0),
        //    };
        //    img.MouseDown += new MouseButtonEventHandler(frmAddflowControl_Click);
        //    return img;
        //}

        //private void OldAddFlowPanning(AddFlow addFlow)
        //{
        //    //Mouse.OverrideCursor = Cursors.Hand;
        //}

        //private void OldAddFlowReAlign(AddFlow addFlow)
        //{
        //    float f = (float)1;
        //    Zoom ZoomReset = new Zoom(f, f);
        //    addFlow.Zoom = ZoomReset;
        //}

        //private void frmAddflowControl_Click(object sender, EventArgs e)
        //{
        //    var clickedOn = ((Image)sender).Name;
        //    switch (clickedOn)
        //    {
        //        case "ZoomIn":
        //            OldAddFlowZoomIn(oldAddflow);
        //            break;
        //        case "ZoomOut":
        //            OldAddFlowZoomOut(oldAddflow);
        //            break;
        //        case "Panning":
        //            OldAddFlowPanning(oldAddflow);
        //            break;
        //        case "ReAlign":
        //            OldAddFlowReAlign(oldAddflow);
        //            break;
        //    }
        //}
        //private void OldAddFlowZoomIn(AddFlow addFlow)
        //{
        //    var zoomaddflow = addFlow;
        //    float f = (float)0.3;
        //    Zoom ZoomIn = new Zoom(zoomaddflow.Zoom.X + f, zoomaddflow.Zoom.Y + f);
        //    if (ZoomIn.X < 3 || ZoomIn.Y < 3)
        //    {
        //        addFlow.Zoom = ZoomIn;
        //    }
        //}

        //private void OldAddFlowZoomOut(AddFlow addFlow)
        //{
        //    var zoomaddflow = addFlow;
        //    float f = (float)0.3;
        //    try
        //    {
        //        Zoom ZoomOut = new Zoom(zoomaddflow.Zoom.X - f, zoomaddflow.Zoom.Y - f);
        //        addFlow.Zoom = ZoomOut;
        //    }
        //    catch (Exception e)
        //    { }
        //}

        private void HandleWiringForCentralController(ucDesignerCanvasViewModel ucDesignerCanvasViewModel)
        {
            if (bToggle == false)
            {
                TempAddFlowWiring.AllowDrop = false;
                TempAddFlowWiring.CanDrawNode = false;

                ControlSystem controlSystem =
                    Project.CurrentProject.ControlSystemList.Find(x =>
                        x.Id.Equals(ucDesignerCanvasViewModel.CurrentSystem.Id));
                ControlGroup controlGroup = Project.CurrentProject.ControlGroupList.Find(x =>
                    x.ControlSystemID.Equals(ucDesignerCanvasViewModel.CurrentSystem.Id));

                if (controlSystem.SystemStatus == SystemStatus.VALID)
                {
                    if (controlSystem.Id == controlGroup.ControlSystemID)
                    {
                        if (controlGroup.IsValidGrp)
                        {                           
                            JCHVRF_New.Utility.CentralControllerWiring wiringCC =
                                new JCHVRF_New.Utility.CentralControllerWiring(Project.CurrentProject, controlSystem,
                                    TempAddFlowWiring);
                            TempAddFlowWiring.Width = 880;
                            this.ScrollViewer.Content = TempAddFlowWiring;
                            bToggle = true;
                            ToggleImage.Source =
                                new BitmapImage(
                                    new Uri("pack://application:,,,/Image/TypeImages/Ellipse.png"));
                            ToggleImage.Visibility = Visibility.Visible;
                          //  ToggleImageTabular.Visibility = Visibility.Visible;
                            brdToggleimg.Visibility = Visibility.Visible;
                            btnPanning.Visibility = Visibility.Visible;
                            btnZoomIn.Visibility = Visibility.Visible;
                            btnZoomOut.Visibility = Visibility.Visible;
                            btnReAlign.Visibility = Visibility.Visible;
                            btnPanning.IsEnabled = true;
                            btnZoomIn.IsEnabled = true;
                            btnZoomOut.IsEnabled = true;
                            btnReAlign.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_WIRING"));
                    return;
                }
            }
            else
            {
                ToggleImage.Source =
                    new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Wiring.png"));
                bToggle = false;
                addflow.Zoom = 1;
                this.ScrollViewer.Content = addflow;
                //ToggleImageTabular.Visibility = Visibility.Collapsed;
                brdToggleimg.Visibility = Visibility.Collapsed;
            }
        }


        private void HandleWiringForVRF(ucDesignerCanvasViewModel ucDesignerCanvasViewModel)
        {

            string SystemId = (ucDesignerCanvasViewModel.CurrentSystem as NextGenModel.SystemVRF).Id;
            var CurrentSystem =
                JCHVRF.Model.Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(sys => sys.Id == SystemId);
            if (CurrentSystem != null && CurrentSystem.MyPipingNodeOut == null)
            {
                if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                {

                    CurrentSystem = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
                }
            }

            if (CurrentSystem != null && CurrentSystem.MyPipingNodeOut != null)
            {
                if (isWiringViewOpen == true)
                {
                    CurrentSystem.IsPipingOK = true;
                }

                if (CurrentSystem.IsPipingOK == true)
                {
                    if (bToggle == false)
                    {
                        isWiringViewOpen = true;
                        ToggleImage.Source =
                            new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Piping.png"));
                        bToggle = true;
                        WL.AddFlow TempAddFlowWiring = new WL.AddFlow();
                        TempAddFlowWiring.AllowDrop = false;
                        TempAddFlowWiring.CanDrawNode = false;
                        //TempAddFlowWiring.Width = 880;
                        
                        // TODO : clean this dirty fix, i cannot believe i learning bad coding practices.
                        if (CurrentSystem != null && CurrentSystem.OutdoorItem != null && !string.IsNullOrEmpty(CurrentSystem.OutdoorItem.FullModuleName))
                        {
                            Wiring WiringPage = new Wiring(this.projectLegacy, CurrentSystem, TempAddFlowWiring);
                            this.ScrollViewer.Content = TempAddFlowWiring;
                        }
                        else
                        {
                            // TODO: language specific message
                            JCHMessageBox.Show("System is not valid please perform autopiping and validation");
                            return;
                        }

                    }
                    else
                    {
                        isWiringViewOpen = false;
                        ToggleImage.Source =
                            new BitmapImage(new Uri("pack://application:,,,/Image/TypeImages/Wiring.png"));
                        bToggle = false;
                        addflow.Zoom = 1;
                        //addflow.Clear();
                        //DoDrawingPiping(true, CurrentSystem, ref addflow);
                        this.ScrollViewer.Content = addflow;
                        //if (CurrentSystem.IsPipingOK == true)
                        //{
                        //    Validate(CurrentSystem, ref addflow);
                        //}
                    }
                }
                else
                {
                    // TODO : Language specific message
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_WIRING_DO_VALIDATION_FOR_ALL_SYSTEMS"));
                    return;
                }
            }
        }

        private void UcDesignerCanvasPanning(object sender, MouseButtonEventArgs e)
        {
            var AddflowToPanning = (WL.AddFlow)ScrollViewer.Content;
            if (AddflowToPanning.MouseSelection==MouseSelection.Pan)
            {
                AddflowToPanning.MouseSelection = WL.MouseSelection.Node;
            }
            else
            {
                AddflowToPanning.MouseSelection = WL.MouseSelection.Pan;
            }
        }

        private void UcDesignerCanvasReAlign(object sender, MouseButtonEventArgs e)
        {
            if (_currentSystem.HvacSystemType.Equals("1"))
            {
                var centreAddFlow = (WL.AddFlow)this.ScrollViewer.Content;
                Point location = centreAddFlow.Origin;
                Size size = centreAddFlow.Extent;
                centreAddFlow.ZoomRectangle(new Rect(location.X, location.Y, size.Width, size.Height));
            }

            if (_currentSystem.HvacSystemType.Equals("6"))
            {
                if (bToggle == false)
                {
                    Point location = addflow.Origin;
                    Size size = addflow.Extent;
                    addflow.ZoomRectangle(new Rect(location.X, location.Y, size.Width + 100, size.Height + 100));                                    
                }

                if (bToggle == true)
                {
                    Point location1 = TempAddFlowWiring.Origin;
                    Size size2 = TempAddFlowWiring.Extent;
                    TempAddFlowWiring.ZoomRectangle(new Rect(location1.X, location1.Y, size2.Width, size2.Height));
                }
            }
        }

        private void CreatePipingWiringToggleButton(string ImageType)
        {

            if (ImageType == "WireChange")
            {
                addflow.RemoveNode(nodePiping);
                btnImagePiping.imageName = "Piping";
                nodePiping.ImageData = btnImagePiping;
                addflow.AddNode(nodePiping);
            }
            else
            {
                addflow.RemoveNode(nodeWiring);
                btnImageWiring.imageName = "WireChange";
                nodeWiring.ImageData = btnImageWiring;
                addflow.AddNode(nodeWiring);
            }
        }
     
        private void Addflow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bool isIntersect = false;
            double left = e.GetPosition(addflow).X;
            double top = e.GetPosition(addflow).Y;
            var mouseLocation = new Point(left, top);
            var selectedItem = addflow.GetItemAt(mouseLocation);
            currentNode = null;
            currentNode = ItemSelect;
            selectedItem = ItemSelect;
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (selectedItem != null)
                {
                    if (selectedItem is JCHNode)
                    {
                        if (_currentSystem == null)
                            _currentSystem = CurrentSystem;

                        if (_currentSystem.HvacSystemType.Equals("1") && ((JCHVRF.Model.NextGen.SystemVRF)_currentSystem).IsManualPiping)
                        {
                            if (selectedItem == null)
                                return;
                            if (selectedItem != null && (selectedItem is NextGenModel.MyNodeYP || selectedItem is NextGenModel.MyNodeCH))
                                return;
                            JCHNode selectedNode = (JCHNode) selectedItem;
                            var SelectedNodeRect = new Rect(selectedItem.Bounds.X, selectedItem.Bounds.Y,
                                selectedNode.Bounds.Width, selectedNode.Bounds.Height);
                            foreach (var item in addflow.Items)
                            {
                                if (!item.Equals(selectedItem))
                                {
                                    if (item is JCHNode)
                                    {
                                        if (item is NextGenModel.MyNodeYP)
                                            continue;
                                        Rect nodeRect = new Rect(item.Bounds.X, item.Bounds.Y, item.Bounds.Width,
                                            item.Bounds.Height);
                                        isIntersect = nodeRect.IntersectsWith(SelectedNodeRect);
                                        if (isIntersect)
                                        {
                                            if (JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_EQUIPMENT_OVERLAP"),
                                                    MessageType.Error, MessageBoxButton.OK) == MessageBoxResult.OK)
                                            {
                                                addflow.Clear();
                                                InitPageLoad = 1;
                                                ReDrawManualPiping(CurrentSystem, addflow);
                                                InitPageLoad = 0;

                                            }

                                            break;
                                        }
                                    }

                                else if (item is NextGenModel.MyLink)
                                {
                                    if (isIntersect == false)
                                    {
                                        if (selectedNode != null && selectedNode.Links.Count > 0 
                                            && selectedNode.Links.Any(i => i.Equals(item)))
                                        {
                                            continue;
                                        }

                                            NextGenModel.MyLink myLink = (NextGenModel.MyLink) item;
                                            for (int i = 0; i < myLink.Points.Count; i++)
                                            {
                                                if (i + 1 == myLink.Points.Count) break;
                                                var pointA = new Point(myLink.Points[i].X, myLink.Points[i].Y);
                                                var pointB = new Point(myLink.Points[i + 1].X, myLink.Points[i + 1].Y);
                                                var linkRect = new Rect(pointA, pointB);
                                                isIntersect = linkRect.IntersectsWith(SelectedNodeRect);
                                                if (isIntersect)
                                                {
                                                    if (JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_EQUIPMENT_OVERLAP"),
                                                            MessageType.Error, MessageBoxButton.OK) ==
                                                        MessageBoxResult.OK)
                                                    {
                                                        addflow.Clear();
                                                        InitPageLoad = 1;
                                                        ReDrawManualPiping(CurrentSystem, addflow);
                                                        InitPageLoad = 0;

                                                    }

                                                    break;
                                                }

                                            }
                                        }
                                    }

                                    if (isIntersect)
                                        break;
                                }

                                if (isIntersect)
                                    break;

                            }
                        }
                    }
                }

            }

            if (selectedItem != null)
            {
                if (selectedItem is JCHNode)
                {
                    var boundaryRect = GetCloseLocation(selectedItem);
                    if (boundaryRect != null)
                    {
                        if (boundaryRect.Value.Contains(mouseLocation))
                        {
                            DeleteNode = selectedItem as JCHNode;
                            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;
                            ucDesignerCanvasViewModel.CurrentSystem = _currentSystem;
                            DeleteNodeOnCanvas(ucDesignerCanvasViewModel.CurrentSystem);
                        }
                    }
                }
                else
                {
                    var boundaryRect = GetCloseLocation(selectedItem);
                    if (boundaryRect != null)
                    {
                        if (boundaryRect.Value.Contains(mouseLocation))
                        {
                            DeleteNode = selectedItem as JCHNode;
                            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;
                            ucDesignerCanvasViewModel.CurrentSystem = _currentSystem;
                            DeleteNodeOnCanvas(ucDesignerCanvasViewModel.CurrentSystem);
                        }
                    }
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.SetCursor(_grabbingCursor);
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                Load_DeleteIcon(e);
            }
            if (isIntersect == false && selectedItem!=null && !(selectedItem is NextGenModel.MyLink))
            {
                SaveManualPiping();
            }
        }
        private void Addflow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                double left = e.GetPosition(addflow).X;
                double top = e.GetPosition(addflow).Y;
                var mouseLocation = new Point(left, top);
                 ItemSelect = addflow.GetItemAt(mouseLocation);
                if (e.ClickCount == 2)
                {
                    
                    CurrentSystem = GetCurrentSystem();
                    if (CurrentSystem != null && CurrentSystem.IsManualPiping)
                    {
                        CurrentSystem.IsManualPiping = false;
                    }
                    ClosePopupWindow();

                    int linkOrder = 0;
                    NextGenModel.MyLink selectedLink = addflow.Items.OfType<NextGenModel.MyLink>().FirstOrDefault(a => a.IsSelected);
                    NextGenModel.JCHNode selectedNode = addflow.Items.OfType<JCHNode>().FirstOrDefault(a => a.IsSelected);

                    if (selectedLink != null)
                    {
                        if ((CurrentSystem.MyPipingOrphanNodes != null && CurrentSystem.MyPipingOrphanNodes.Count > 0))
                        {
                            JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS")+"  .");
                            return;
                        }
                        if(CurrentSystem.IsManualPiping)
                        {
                            JCHMessageBox.Show("Input pipe length is not allowed in manual piping ");
                            return;
                        }
                        InputPipeLengthPopup inputPipeLengthPopup = new Views.InputPipeLengthPopup(selectedLink.Length, selectedLink.ElbowQty, selectedLink.OilTrapQty, CurrentSystem);
                        inputPipeLengthPopup.Owner = System.Windows.Application.Current.MainWindow;
                        inputPipeLengthPopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        linkOrder = selectedLink.LinkOrder;
                        Lassalle.WPF.Flow.Node node = selectedLink.Dst;
                        inputPipeLengthPopup.ShowDialog();

                    }
                    else if (selectedNode != null)
                    {
                        _eventAggregator.GetEvent<FloatProperties>().Publish();
                    }

                    if (addflow.SelectedItems != null && addflow.SelectedItems.Any(a => a is NextGenModel.MyNodeYP))
                    {
                        NextGenModel.MyNodeYP myNodeYP = (NextGenModel.MyNodeYP)addflow.SelectedItems.Where(a => a is NextGenModel.MyNodeYP).FirstOrDefault();
                        if (myNodeYP != null)
                        {
                            myNodeYP.RotateYpNode(myNodeYP.Size.Width < 40 ? 0 : -90, addflow);
                        }
                    }

                }
                else if(e.ClickCount==1)
                { 
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Mouse.SetCursor(_grabbingCursor);
                    }
                    else if (e.LeftButton == MouseButtonState.Released)
                    {
                        Mouse.SetCursor(_grabCursor);
                    }

                    SetIndoorOutdoorUnitFillStyle();
                    SetControllerFillStyle();
                }

                _eventAggregator.GetEvent<SystemTypeCanvasSubscriber>().Publish(_currentSystem);
            }
            catch (Exception)
            {
            }
        }

       

        private void SetIndoorOutdoorUnitFillStyle(JCHNode focusedNode = null)
        {
            var grayStroke = new SolidColorBrush(Color.FromRgb(185, 185, 185));
            var blueStroke = new SolidColorBrush(Color.FromRgb(45, 221, 250));
            var whiteFill = Brushes.Transparent; // new SolidColorBrush(Color.FromRgb(255, 255, 255)); //White Fill
            var grayFill = new SolidColorBrush(Color.FromRgb(247, 247, 247)); //Gray Fill
            foreach (var item in addflow.Items)
            {
                if (!(item is JCHNode))
                {
                    continue;
                }

                if (((JCHVRF.Model.NextGen.JCHNode) item).ImageData == null)
                {
                    continue;
                }

                if (((JCHVRF.Model.NextGen.JCHNode) item).ImageData.equipmentType == null)
                {
                    continue;
                }

                if ((((JCHVRF.Model.NextGen.JCHNode) item).ImageData.equipmentType ==
                     "Indoor") || (((JCHVRF.Model.NextGen.JCHNode) item).ImageData.equipmentType == "Outdoor"))
                {
                    item.Fill = item.IsSelected ? grayFill : new SolidColorBrush(nodeBackgroundColor);
                    //Gray Stroke
                    item.Stroke = new SolidColorBrush(Color.FromRgb(185, 185, 185));
                    DrawMandatoryNodeVisuals(item as JCHNode);

                    if (focusedNode != null)
                    {
                        if (item == focusedNode)
                        {
                            //Blue Strok
                            item.Stroke = new SolidColorBrush(Color.FromRgb(45, 221, 250));
                            DrawMandatoryNodeVisualsIncludingHeaderButtons(item as JCHNode);
                        }
                    }

                    if (item.IsSelected)
                    {
                        item.Fill = new SolidColorBrush(Color.FromRgb(247, 247, 247)); //Gray Fill
                        item.Stroke = new SolidColorBrush(Color.FromRgb(45, 221, 250));//Blue Stroke
                        DrawMandatoryNodeVisualsIncludingHeaderButtons(item as JCHNode);
                    }
                    else
                    {
                        item.Fill = new SolidColorBrush(nodeBackgroundColor);//new SolidColorBrush(Color.FromRgb(255, 255, 255)); //White Fill
                    }

                }
             
            }
        }


        //Please keep this code. Do not delete it please.
        /*
        public void createDynamicDiagramWithImages(AddFlow addFlow)
        {
            double incrementFactorLeft = 50;
            double incrementFactorTop = 30;
            double incrementFactorWidth = 55;
            double incrementFactorheight = 30;
            Image imageIndoor = new Image();
            Image imageIndoor1 = new Image();
            imageIndoor.Source = new BitmapImage(new Uri("pack://application:,,,/Image/4_Way_Cassette.png"));
            imageIndoor1.Source = new BitmapImage(new Uri("pack://application:,,,/Image/IDU_Image.png"));
            Image imageOutdoor = new Image();
            imageOutdoor.Source = new BitmapImage(new Uri("pack://application:,,,/Image/ODU_Image.png"));

            addflow.PinSize = 5;
            addflow.PinFill = Brushes.Aqua;
            addflow.PinStroke = Brushes.AliceBlue;

            addflow.NodeModel.PinsLayout =
                new PointCollection { new Point(0, 50), new Point(50, 0), new Point(100, 50), new Point(50, 100) };
            addflow.LinkModel.LineStyle = LineStyle.VH;

            for (int i = 0; i < 30; i++)
            {
                double left = incrementFactorLeft;
                double top = incrementFactorTop;
                double width = incrementFactorWidth;
                double height = incrementFactorheight;

                if (i == 0)
                {
                    Node outNode = new Node(left, top, 60, 60, "Outdoor Unit", addFlow);
                    outNode.Stroke = Brushes.Transparent;
                    outNode.Fill = Brushes.Transparent;
                    outNode.Image = imageOutdoor.Source;
                    addflow.AddNode(outNode);
                    incrementFactorLeft = incrementFactorLeft + 5;
                    incrementFactorTop = incrementFactorTop + 30;
                }
                else
                {
                    Node node = new Node(left+50, top+70, width+50, height+70, null, addFlow);
                    node.Stroke = Brushes.Transparent;
                    node.Fill = Brushes.Transparent;
                    if (i % 2 == 0)
                        node.Image = imageIndoor.Source;
                    else
                        node.Image = imageIndoor1.Source;

                    addflow.AddNode(node);
                    incrementFactorLeft = incrementFactorLeft + 80;

                    if (i % 5 == 0)
                    {
                        incrementFactorLeft = 50;
                        incrementFactorTop = incrementFactorTop + 70;
                    }
                }
            }
            var nodes = addflow.Items.OfType<Node>().ToArray();

            createLinksDynamically(nodes, addFlow);           

        }
        public void createLinksDynamically(Node[] nodeCollection, AddFlow addFlow)
        {
            for (var i = 0; i < nodeCollection.Length - 1; i++)
            {
                Link ConnectingLink = new Link(nodeCollection[0], nodeCollection[nodeCollection.Length - (i + 1)], 3, 3, "", addFlow);

                addFlow.AddLink(ConnectingLink);
            }
        }
        */

        public bool DrawAutoPipingValidation(out string ErrorMessage)
        {
            bool isValidData = true;
            ErrorMessage = string.Empty;
            var Nodes = Enumerable.OfType<JCHNode>(addflow.Items).ToArray();
            int OutDoorCount = 0;
            bool isOutdoorNotFound = false;
            NextGenModel.MyNodeOut myNodeOut = new NextGenModel.MyNodeOut();
            if (Nodes != null && Nodes.Length > 0) // idu and odu on canvas
            {
                if (Nodes.Length == 1)
                {

                    var selectedNode = Nodes[0];
                    var equipmentProperties = selectedNode.ImageData;
                    if (equipmentProperties.equipmentType.Equals("Indoor", StringComparison.OrdinalIgnoreCase)
                    ) //only one idu on the canvas
                    {
                        isValidData = false;
                        ErrorMessage = string.Format(ValidationMessage.AtleastOneODU);
                    }
                    else if (equipmentProperties.equipmentType.Equals("outdoor", StringComparison.OrdinalIgnoreCase)
                    ) // only one odu on the canvas
                    {
                        isValidData = false;
                        ErrorMessage = Langauge.Current.GetMessage("ALERT_INDOOR");
                    }
                }
                else // more than one nodes on canvas
                {
                    foreach (var item in Nodes.Distinct())
                    {
                        var selectedNode = item;
                        if (selectedNode.GetType().FullName == "JCHVRF.Model.NextGen.MyNodeOut" ||
                            selectedNode.GetType().FullName == "JCHVRF.Model.NextGen.MyNodeIn")
                        {
                            var equipmentProperties = selectedNode.ImageData;

                            if (equipmentProperties.equipmentType.Equals("Indoor", StringComparison.OrdinalIgnoreCase))
                            {
                                isValidData = true;
                                continue;
                            }
                            else if (equipmentProperties.equipmentType.Equals("outdoor",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                isOutdoorNotFound = true;
                                OutDoorCount += 1;
                                if (OutDoorCount > 1) // More than one odu placed.
                                {
                                    ErrorMessage = string.Format(ValidationMessage.MorethanOneODU);
                                    isValidData = false;
                                    break;
                                }
                            }
                        }
                        else
                        {

                            if (selectedNode.GetType().FullName.Equals("Lassalle.WPF.Flow.Node",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                var equipmentProperties = selectedNode.ImageData;

                                if (equipmentProperties != null && equipmentProperties.equipmentType != null)
                                {
                                    if (equipmentProperties.equipmentType.Equals("Indoor",
                                        StringComparison.OrdinalIgnoreCase))
                                    {
                                        isValidData = true;
                                        continue;
                                    }
                                    else if (equipmentProperties.equipmentType.Equals("outdoor",
                                        StringComparison.OrdinalIgnoreCase))
                                    {
                                        isOutdoorNotFound = true;
                                        OutDoorCount += 1;
                                        if (OutDoorCount > 1) // More than one odu placed.
                                        {
                                            ErrorMessage = string.Format(ValidationMessage.MorethanOneODU);
                                            isValidData = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!isOutdoorNotFound) //if only more than idu are placed.
                    {
                        isValidData = false;
                        ErrorMessage = string.Format(ValidationMessage.AtleastOneODU);
                    }
                }
            }
            else // empty canvas
            {
                isValidData = false;
                ErrorMessage = string.Format(ValidationMessage.AtleastOneODUAndOneIDU);
            }

            return isValidData;
        }

        private void DoPipingCalculation(NextGenBLL.PipingBLL pipBll, JCHVRF.Model.NextGen.MyNodeOut nodeOut,
            JCHVRF.Model.NextGen.SystemVRF currentSystem, out NextGenBLL.PipingErrors errorType)
        {
            errorType = NextGenBLL.PipingErrors.OK;
            if (nodeOut.ChildNode == null)
            {
                return;
            }
            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            //getSumCalculation(ref firstDstNode, factoryCode, type, unitType);

            pipBll.GetSumCapacity(nodeOut.ChildNode);

            pipBll.IsBranchKitNeedSizeUp(currentSystem);

            PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = nodeOut.ChildNode as JCHVRF.Model.NextGen.MyNodeYP;
                if (nodeYP.IsCP)
                {
                    //第一分歧管可能是梳形管 20170711 by Yunxiao Lin
                    firstBranchKit = pipBll.getFirstHeaderBranchPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                else
                {
                    // 第一分歧管放大一号计算
                    firstBranchKit = pipBll.getFirstPipeCalculation(nodeYP, currentSystem, out errorType);
                }

                if (errorType != NextGenBLL.PipingErrors.OK)
                {
                    SetSystemPipingOK(currentSystem, false);
                    return;
                }
            }
            //bug 3489
            var L2SizeDownRule = pipBll.GetL2SizeDownRule(currentSystem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            //bug 3489
            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            pipBll.getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, currentSystem, false,
                out errorType, L2SizeDownRule);
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);
                return;
            }

            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut);
        }

        public void DoPipingInfoCalculation(NextGenBLL.PipingBLL pipBll, JCHVRF.Model.NextGen.MyNodeOut nodeOut,
            JCHVRF.Model.NextGen.SystemVRF currentSystem, out NextGenBLL.PipingErrors errorType)
        {
            errorType = NextGenBLL.PipingErrors.OK;
            if (nodeOut.ChildNode == null)
            {
                return;
            }
            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            //getSumCalculation(ref firstDstNode, factoryCode, type, unitType);

            pipBll.GetSumCapacity(nodeOut.ChildNode);

            pipBll.IsBranchKitNeedSizeUp(currentSystem);

            PipingBranchKit firstBranchKit = null;
            if (nodeOut.ChildNode is JCHVRF.Model.NextGen.MyNodeYP)
            {
                JCHVRF.Model.NextGen.MyNodeYP nodeYP = nodeOut.ChildNode as JCHVRF.Model.NextGen.MyNodeYP;
                if (nodeYP.IsCP)
                {
                    //第一分歧管可能是梳形管 20170711 by Yunxiao Lin
                    firstBranchKit = pipBll.getFirstHeaderBranchPipeCalculation(nodeYP, currentSystem, out errorType);
                }
                else
                {
                    // 第一分歧管放大一号计算
                    firstBranchKit = pipBll.getFirstPipeCalculation(nodeYP, currentSystem, out errorType);
                }

                if (errorType != NextGenBLL.PipingErrors.OK)
                {
                    SetSystemPipingOK(currentSystem, false);
                    return;
                }
            }

            //bug 3489
            var L2SizeDownRule = pipBll.GetL2SizeDownRule(currentSystem);//增大1st branch的管径型号 或者 缩小2nd branch的管径型号 add by Shen Junjie on 2018/2/21
            //bug 3489

            //分歧管型号和管径改为如果后面的大于前面的，则后面的替换为前面的型号和管径  by Shen Junjie on 20170409
            pipBll.getSumCalculationInversion(firstBranchKit, nodeOut, nodeOut.ChildNode, currentSystem, false,
                out errorType, L2SizeDownRule);
            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);
                return;
            }

            pipBll.CheckIndoorNumberConnectedCHBox(nodeOut);
        }

        private void SetSystemPipingOK(JCHVRF.Model.NextGen.SystemVRF sysItem, bool isPipingOK)
        {
            if (sysItem.IsPipingOK != isPipingOK)
            {
                sysItem.IsPipingOK = isPipingOK;
                //SetTabControlImageKey();
            }
        }

        public NextGenBLL.PipingBLL GetPipingBLLInstanceValidation()
        {
            bool isInch = CommonBLL.IsDimension_inch();
            return new NextGenBLL.PipingBLL(this.projectLegacy, utilPiping, addflow, isInch, ut_weight, ut_length,
                ut_power);
        }

        private void DoPipingFinalVerification(JCHVRF.Model.NextGen.SystemVRF currentSystem, WL.AddFlow addflows)
        {
            NextGenBLL.PipingErrors errorType = NextGenBLL.PipingErrors.OK;
            NextGenBLL.PipingErrors Message = NextGenBLL.PipingErrors.OK;

            if (currentSystem.OutdoorItem == null)
            {
                return;
            }

            if (currentSystem.IsManualPiping && currentSystem.IsUpdated)
            {

                return;
            }

            //this.Cursor = Cursors.WaitCursor;
            UtilityValidation ObjPipValidation = new UtilityValidation(this.projectLegacy, ref addflow);
            JCHVRF.MyPipingBLL.NextGen.PipingBLL pipBll = GetPipingBLLInstanceValidation();
            bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(currentSystem);
            pipBll.SetPipingLimitation(currentSystem);

            errorType = pipBll.ValidateSystemHighDifference(currentSystem);

            if (errorType == NextGenBLL.PipingErrors.OK)
            {
                errorType = pipBll.ValidatePipeLength(currentSystem, ref addflow);
            }

            DoPipingInfoCalculation(pipBll, currentSystem.MyPipingNodeOut, currentSystem, out Message);

            #region

            if (errorType == NextGenBLL.PipingErrors.OK)
            {
                //DoDrawingPiping(true, currentSystem, ref addflow);
                if (!currentSystem.IsPipingOK)
                {
                    pipBll.SetDefaultColor(ref addflow, isHR);
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    if (currentSystem.PipeEquivalentLength < currentSystem.HeightDiff)
                    {
                        errorType = NextGenBLL.PipingErrors.PIPING_LENGTH_HEIGHT_DIFF; //-32;
                    }
                }

                if (currentSystem.IsInputLengthManually)
                {

                    errorType = pipBll.ValMainBranch(currentSystem, ref addflow);
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    if (NextGenBLL.PipingBLL.IsHeatRecovery(currentSystem) &&
                        !pipBll.ValCoolingOnlyIndoorCapacityRate(currentSystem, ref addflow))
                    {
                        errorType = NextGenBLL.PipingErrors.COOLINGONLYCAPACITY; //-12;
                    }
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    errorType = pipBll.ValidateIDUOfMultiCHBox(currentSystem);
                }

                if (errorType == NextGenBLL.PipingErrors.OK)
                {
                    SetSystemPipingOK(currentSystem, true);
                    DoPipingCalculation(pipBll, currentSystem.MyPipingNodeOut, currentSystem, out errorType);
                    if (currentSystem.IsPipingOK)
                    {
                        if (currentSystem.IsInputLengthManually &&
                            !pipBll.ValCHToIndoorMaxTotalLength(currentSystem, ref addflow))
                        {
                            errorType = NextGenBLL.PipingErrors.MKTOINDOORLENGTH1; //-8;
                        }
                        else if (!pipBll.ValMaxIndoorNumberConnectToCH(currentSystem, ref addflow))
                        {
                            errorType = NextGenBLL.PipingErrors.INDOORNUMBERTOCH; //-13;
                        }
                        else
                        {
                            SetSystemPipingOK(currentSystem, true);

                            if (currentSystem.IsInputLengthManually)
                            {
                                double d1 = pipBll.GetAddRefrigeration(currentSystem, ref addflow);
                                currentSystem.OutdoorItem.RefrigerantCharge = d1;

                                pipBll.DrawAddRefrigerationText(currentSystem);
                                _eventAggregator.GetEvent<SystemDetailsSubscriber>()
                                    .Publish((JCHVRF.Model.NextGen.SystemVRF) currentSystem);
                            }
                            else
                            {
                                currentSystem.AddRefrigeration = 0;
                            }
                        }
                    }

                    ObjPipValidation.DrawTextToAllNodes(currentSystem.MyPipingNodeOut, null, currentSystem);
                    //UtilTrace.SaveHistoryTraces();
                }
            }

            #endregion

            if (errorType != NextGenBLL.PipingErrors.OK)
            {
                SetSystemPipingOK(currentSystem, false);

                ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, ShowWarningMsg(errorType));
            }

         //   ShowWarningMsg(errorType);
            //UtilTrace.SaveHistoryTraces();                        
            //this.Cursor = Cursors.Default;
        }

        private string  ShowWarningMsg(NextGenBLL.PipingErrors errorType) //Shweta: updated to get the msg string as per errorType
        {
            double len = 0;
            int count = 0;
            string rate = "";
            string msg = "";
            string templen = "";
            string temphei = "";
            NextGenModel.SystemVRF currentSystem = (NextGenModel.SystemVRF) _currentSystem;
            switch (errorType)
            {
                case NextGenBLL.PipingErrors.LINK_LENGTH: //-1:
                    msg = Msg.PIPING_LINK_LENGTH;
                    break;
                case NextGenBLL.PipingErrors.WARN_ACTLENGTH: //-2:
                    len = Unit.ConvertToControl(currentSystem.MaxPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_WARN_ACTLENGTH(currentSystem.PipeActualLength.ToString("n0"), len.ToString("n0") + ut_length);

                    break;
                case NextGenBLL.PipingErrors.EQLENGTH: //-3:
                    len = Unit.ConvertToControl(currentSystem.MaxEqPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_EQLENGTH(currentSystem.PipeEquivalentLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.FIRSTLENGTH: //-4:
                    len = Unit.ConvertToControl(currentSystem.MaxIndoorLength, UnitType.LENGTH_M, ut_length);
                  //  currLen = Unit.ConvertToControl(currentSystem, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_FIRSTLENGTH(currentSystem.FirstPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.LENGTHFACTOR: //-5:
                    len = Unit.ConvertToControl(currentSystem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diff = Unit.ConvertToControl(currentSystem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(currentSystem.Name, len.ToString("n2") + ut_length,
                        Math.Abs(diff).ToString("n2") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.TOTALLIQUIDLENGTH: //-6:
                    len = Unit.ConvertToControl(currentSystem.MaxTotalPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_TOTALLIQUIDLENGTH(currentSystem.TotalPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MKTOINDOORLENGTH: //-7:
                    len = Unit.ConvertToControl(currentSystem.MaxMKIndoorPipeLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH(currentSystem.ActualMaxMKIndoorPipeLength.ToString("n0"), len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MKTOINDOORLENGTH1: //-8:
                    len = Unit.ConvertToControl(PipingBLL.MaxCHToIndoorTotalLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_MKTOINDOORLENGTH("0",len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAINBRANCHCOUNT: //-9:
                    count = PipingBLL.MaxMainBranchCount;
                    msg = Msg.PIPING_MAINBRANCHCOUNT(count.ToString());
                    break;
                case NextGenBLL.PipingErrors.COOLINGCAPACITYRATE: //-10:
                    rate = PipingBLL.MinMainBranchCoolingCapacityRate;
                    msg = Msg.PIPING_COOLINGCAPACITYRATE(rate);
                    break;
                //case -11:
                //    rate = PipingBLL.MinMainBranchHeatingCapacityRate;
                //    msg = Msg.PIPING_HEATINGCAPACITYRATE(rate);
                //    break;
                case NextGenBLL.PipingErrors.COOLINGONLYCAPACITY: //-12:
                    msg = Msg.PIPING_COOLINGONLYCAPACITY();
                    break;
                case NextGenBLL.PipingErrors.INDOORNUMBERTOCH: //-13:
                    count = PipingBLL.MaxIndoorNumberConnectToCH;
                    msg = Msg.PIPING_INDOORNUMBERTOCH(count.ToString());
                    break;
                // 多台室外机组成机组时，校验第一分歧管到第一Piping Connection kit之间的管长不能小于0.5m add on 20170720 by Shen Junjie
                case NextGenBLL.PipingErrors.FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH: //-14:
                    double betweenConnectionKits_Min = Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length);
                    string betweenConnectionKits_Msg = betweenConnectionKits_Min.ToString("n2") + ut_length;
                    msg = Msg.PIPING_FIRST_CONNECTION_KIT_TO_FIRST_BRANCH_MIN_LENGTH(betweenConnectionKits_Msg);
                    break;
                case NextGenBLL.PipingErrors._3RD_MAIN_BRANCH: //-15:
                    //不能有第三层主分支。
                    msg = Msg.GetResourceString("ERROR_PIPING_3RD_MAIN_BRANCH");
                    break;
                case NextGenBLL.PipingErrors._4TH_BRANCH_NOT_MAIN_BRANCH: //-16:
                    //第4(或更远的)分支不能是一个主分支。
                    msg = Msg.GetResourceString("ERROR_PIPING_4TH_BRANCH_NOT_MAIN_BRANCH");
                    break;
                case NextGenBLL.PipingErrors.DIFF_LEN_FURTHEST_CLOSESST_IU: //-17
                    msg = Msg.GetResourceString("ERROR_PIPING_DIFF_LEN_FURTHEST_CLOSESST_IU");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_BRANCH_KIT: //-18
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_BRANCH_KIT");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_CHBOX: //-19
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_CHBOX");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_MULTI_CHBOX: //-20
                    msg = Msg.GetResourceString("ERROR_PIPING_NO_MATCHED_MULTI_CHBOX");
                    break;
                case NextGenBLL.PipingErrors.NO_MATCHED_SIZE_UP_IU: //-21
                    msg = Msg.WARNING_DATA_EXCEED;
                    break;
                case NextGenBLL.PipingErrors.MAX_HIGHDIFF_UPPER: //-22:
                    len = Unit.ConvertToControl(currentSystem.MaxOutdoorAboveHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_HeightDiffH(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_HIGHDIFF_LOWER: //-23:
                    len = Unit.ConvertToControl(currentSystem.MaxOutdoorBelowHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_HeightDiffL(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_HIGHDIFF_INDOOR: //-24:
                    len = Unit.ConvertToControl(currentSystem.MaxDiffIndoorHeight, UnitType.LENGTH_M, ut_length);
                    msg = Msg.Piping_Indoor_HeightDiff(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_CHBOXHIGHDIFF: //-25:
                    len = Unit.ConvertToControl(currentSystem.NormalCHBoxHighDiffLength, UnitType.LENGTH_M, ut_length);
                    msg = Msg.DiffCHBoxHeightValue(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_MULTICHBOXHIGHDIFF: //-26:
                    len = Unit.ConvertToControl(currentSystem.NormalSameCHBoxHighDiffLength, UnitType.LENGTH_M,
                        ut_length);
                    msg = Msg.DiffMulitBoxHeightValue(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MAX_CHBOX_INDOORHIGHDIFF: //-27:
                    len = Unit.ConvertToControl(currentSystem.NormalCHBox_IndoorHighDiffLength, UnitType.LENGTH_M,
                        ut_length);
                    msg = Msg.DiffCHBox_IndoorHeightValue(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.INDOORLENGTH_HIGHDIFF: //-28
                    templen = Unit.ConvertToControl(NextGenBLL.PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length)
                                  .ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(NextGenBLL.PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length)
                                  .ToString("n2") + ut_length;
                    msg = Msg.INDOORLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case NextGenBLL.PipingErrors.CHBOXLENGTH_HIGHDIFF: //-29
                    templen = Unit.ConvertToControl(NextGenBLL.PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length)
                                  .ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(NextGenBLL.PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length)
                                  .ToString("n2") + ut_length;
                    msg = Msg.CHBOXLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case NextGenBLL.PipingErrors.CHBOX_INDOORLENGTH_HIGHDIFF: //-30
                    templen = Unit.ConvertToControl(NextGenBLL.PipingBLL.TempActualLength, UnitType.LENGTH_M, ut_length)
                                  .ToString("n2") + ut_length;
                    temphei = Unit.ConvertToControl(NextGenBLL.PipingBLL.TempMaxLength, UnitType.LENGTH_M, ut_length)
                                  .ToString("n2") + ut_length;
                    msg = Msg.CHBOX_INDOORLENGTH_HIGHDIFF_MSG(templen, temphei);
                    break;
                case NextGenBLL.PipingErrors.PIPING_CHTOINDOORTOTALLENGTH: //-31
                    len = Unit.ConvertToControl(40, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_CHTOINDOORTOTALLENGTH(len.ToString("n0") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.PIPING_LENGTH_HEIGHT_DIFF: //-32
                    len = Unit.ConvertToControl(currentSystem.PipeEquivalentLength, UnitType.LENGTH_M, ut_length);
                    double diffs = Unit.ConvertToControl(currentSystem.HeightDiff, UnitType.LENGTH_M, ut_length);
                    msg = Msg.PIPING_LENGTHFACTOR(currentSystem.Name, len.ToString("n2") + ut_length,
                        Math.Abs(diffs).ToString("n2") + ut_length);
                    break;
                case NextGenBLL.PipingErrors.MIN_DISTANCE_BETWEEN_MULTI_KITS: //-33
                    msg = Msg.PIPING_MIN_LEN_BETWEEN_MULTI_KITS(
                        Unit.ConvertToControl(0.5, UnitType.LENGTH_M, ut_length).ToString() + ut_length);
                    break;
                case NextGenBLL.PipingErrors.ODU_PIPE_LENGTH_LIMITS: //-34
                    msg = Msg.GetResourceString("PIPING_ODU_PIPE_LENGTH_LIMITS");
                    break;
                case NextGenBLL.PipingErrors.ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX: //-35                    
                    msg = Msg.GetResourceString("ANZ_MAX_BIG_IDU_OF_MUlTI_CHBOX");
                    break;
                case NextGenBLL.PipingErrors.OK: //0
                default:
                    msg = "";
                    _eventAggregator.GetEvent<ErrorLogVMClearAll>().Publish();
                    break;
            }

            //ShowWarningMsg(msg);
            currentSystem.Errors.Add(msg);
            _eventAggregator.GetEvent<ErrorLogVM>().Publish(currentSystem.Errors);
            return msg;
        }

        private void ShowWarningMsg(string msg)
        {
            ShowWarningMsg(msg, System.Drawing.Color.Red);
        }

        private void ShowWarningMsg(string msg, System.Drawing.Color msgColor)
        {
            //tlblStatus.Text = msg;
            //tlblStatus.ForeColor = msgColor;

            //JCHMessageBox.Show(msg);
        }

        public void Validate(JCHVRF.Model.NextGen.SystemVRF CurrentSystem, ref WL.AddFlow addflows)
        {
            try
            {

                if (CurrentSystem == null)
                {
                    return;
                }
                if(!Utility.Validation.ValidateAddFlow(addflow, CurrentSystem))
                {
                    if (InitPageLoad != 1)
                        JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS"));

                    CurrentSystem.IsPipingOK = false;
                    isWiringViewOpen = false;
                    return;
                }

                if (CurrentSystem.MyPipingNodeOut != null)
                {
                    //return;
                    utilPiping = new NextGenBLL.UtilPiping();
                    ut_length = SystemSetting.UserSetting.unitsSetting.settingLENGTH;
                    ut_power = SystemSetting.UserSetting.unitsSetting.settingPOWER;
                    ut_temperature = SystemSetting.UserSetting.unitsSetting.settingTEMPERATURE;
                    ut_airflow = SystemSetting.UserSetting.unitsSetting.settingAIRFLOW;
                    ut_weight = SystemSetting.UserSetting.unitsSetting.settingWEIGHT;
                    ClearValidationText(addflow);
                    DoPipingFinalVerification(CurrentSystem, addflow);
                    CurrentSystem.IsOutDoorUpdated = false;
                    _eventAggregator.GetEvent<CanvasPropertiesSubscriber>().Publish();
                    _eventAggregator.GetEvent<SetPipingInfoSubscriber>().Publish(CurrentSystem);


                }
            }
            catch (Exception ex)
            {
            }
        }

        //start for context menu canvas
        private void addflow_rightClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                addflow.ContextMenu = null;
                double left = e.GetPosition(addflow).X;
                double top = e.GetPosition(addflow).Y;
                var selectedItem = addflow.GetItemAt(new Point(left, top));
                var selectedNode = selectedItem as JCHNode;
                if (selectedNode != null)
                {
                    selNode = selectedNode;
                    DeleteNode = selectedNode;
                    var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;      
                    ucDesignerCanvasViewModel.CurrentSystem = _currentSystem;
                    
                    //Default Binding to 'Delete' menu on right click on any JCHNode type item
                    AddflowContextMenu.Items.Clear();
                    MenuItem DeleteMenuItem = new MenuItem();
                    DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
                    DeleteMenuItem.Click += DeleteMenuItem_Click;
                    AddflowContextMenu.Items.Add(DeleteMenuItem);

                    if (ucDesignerCanvasViewModel.CurrentSystem.HvacSystemType == "1")
                    {
                        var CurrentSystem = (NextGenModel.SystemVRF) ucDesignerCanvasViewModel.CurrentSystem;
                        bool isHR = NextGenBLL.PipingBLL.IsHeatRecovery(CurrentSystem);
                        if (isHR)
                        {
                            if (
                                ((JCHVRF.Model.NextGen.MyNode) selNode).ParentNode is NextGenModel.MyNodeMultiCH ||
                                ((JCHVRF.Model.NextGen.MyNode) selNode).ParentNode is NextGenModel.MyNodeCH)
                            {
                                AddflowContextMenu.Items.Clear();
                                MenuItem CoolingMenuItem = new MenuItem();
                                DeleteMenuItem = new MenuItem();
                                DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
                                CoolingMenuItem.Header = "Use of Cooling operation";
                                CoolingMenuItem.Click += CoolingMenuItem_Click;
                                DeleteMenuItem.Click += DeleteMenuItem_Click;
                                AddflowContextMenu.Items.Add(DeleteMenuItem);
                                AddflowContextMenu.Items.Add(CoolingMenuItem);
                            }
                            else if (((JCHVRF.Model.NextGen.MyNode) selNode).ParentNode is NextGenModel.MyNodeYP && ((JCHVRF.Model.NextGen.MyNode)selNode) is NextGenModel.MyNodeIn)
                            {
                                AddflowContextMenu.Items.Clear();
                                MenuItem CancelCoolingMenuItem = new MenuItem();
                                DeleteMenuItem = new MenuItem();
                                DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
                                CancelCoolingMenuItem.Header = "Cancel Use of Cooling operation";
                                CancelCoolingMenuItem.Click += CancelCoolingMenuItem_Click;
                                DeleteMenuItem.Click += DeleteMenuItem_Click;
                                AddflowContextMenu.Items.Add(DeleteMenuItem);
                                AddflowContextMenu.Items.Add(CancelCoolingMenuItem);
                            }
                            
                        }
                    }

                    if (ucDesignerCanvasViewModel.CurrentSystem.HvacSystemType == "6")
                    {
                        var CurrentSystem = (JCHVRF.Model.ControlSystem) ucDesignerCanvasViewModel.CurrentSystem;

                        if (selNode is NextGenModel.JCHNode)
                        {
                            AddflowContextMenu.Items.Clear();
                            DeleteMenuItem = new MenuItem();
                            DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
                            DeleteMenuItem.Click += DeleteMenuItem_Click;
                            AddflowContextMenu.Items.Add(DeleteMenuItem);
                        }

                        if (selNode is NextGenModel.WiringNodeCentralControl)
                        {
                            if (((JCHVRF.Model.NextGen.MyNode) selNode) is NextGenModel.WiringNodeCentralControl)
                            {
                                AddflowContextMenu.Items.Clear();
                                DeleteMenuItem = new MenuItem();
                                DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
                                DeleteMenuItem.Click += DeleteMenuItem_Click;
                                AddflowContextMenu.Items.Add(DeleteMenuItem);
                            }
                        }
                    }

                    addflow.ContextMenu = AddflowContextMenu;
                }
                else
                {
                    if (selectedItem is WL.Link)
                    {
                        DeleteLink = selectedItem as WL.Link;
                        addflow.ContextMenu = AddflowLinkDeleteContextMenu;
                    }
                }
            }

            catch (Exception ex)
            {
            }
        }

        private void CoolingOnCanvas(NextGenModel.SystemVRF sys)
        {

            JCHNode selPNode = selNode;
            NextGenBLL.PipingBLL pipBllNG = GetPipingBLLInstance();
            if (selNode is NextGenModel.MyNodeIn)
            {
                NextGenModel.MyNodeIn nodeIn = selNode as NextGenModel.MyNodeIn;
                pipBllNG.DeleteCHBoxUpward(nodeIn);
            }
            else if (selNode is NextGenModel.MyNodeCH || selNode is NextGenModel.MyNodeMultiCH)
            {
                pipBllNG.DeleteCHBoxUpwards(selPNode);
                pipBllNG.DeleteCHBoxDownwards(selNode);
            }
            else if (selNode is NextGenModel.MyNodeYP)
            {
                NextGenModel.MyNodeYP nodeYP = selNode as NextGenModel.MyNodeYP;
                pipBllNG.DeleteCHBoxUpwards(selPNode);
                pipBllNG.DeleteCHBoxDownwards(nodeYP);
            }

            SetSystemPipingOK(sys, false);
            DoDrawingPiping(true, sys, ref addflow);
            iscooling = false;
            iscancelcooling = true;

            AddflowContextMenu.Items.Clear();
            MenuItem DeleteMenuItem = new MenuItem();
            MenuItem CancelCoolingMenuItem = new MenuItem();
            DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
            CancelCoolingMenuItem.Header = "Cancel use of Cooling operation";
            CancelCoolingMenuItem.Click += CancelCoolingMenuItem_Click;
            DeleteMenuItem.Click += DeleteMenuItem_Click;
            AddflowContextMenu.Items.Add(DeleteMenuItem);

        }

        private void CancelCoolingOnCanvas(NextGenModel.SystemVRF sys)
        {
            if (selNode == null) return;
            if (selNode is NextGenModel.MyNodeIn || selNode is NextGenModel.MyNodeYP)
            {
                NextGenBLL.PipingBLL pipBllNG = GetPipingBLLInstance();
                pipBllNG.InsertCHBox(selNode);
                DoDrawingPiping(true, sys, ref addflow);
                AddflowContextMenu.Items.Clear();
                MenuItem DeleteMenuItem = new MenuItem();
                MenuItem CoolingMenuItem = new MenuItem();
                DeleteMenuItem.Header = Langauge.Current.GetMessage("DELETE_NODE");
                CoolingMenuItem.Header = "Use of Cooling operation";
                CoolingMenuItem.Click += CoolingMenuItem_Click;
                DeleteMenuItem.Click += DeleteMenuItem_Click;
                AddflowContextMenu.Items.Add(DeleteMenuItem);
                AddflowContextMenu.Items.Add(CoolingMenuItem);
            }
        }

        private void DeleteNodeOnCanvas(SystemBase sys)
        {
            try
            {
                string sysID = String.Empty;
                _currentSystem = WorkFlowContext.CurrentSystem;
                if (_currentSystem != null)
                {
                    sysID = _currentSystem.Id;
                }
                else
                {
                    if (WorkFlowContext.CurrentSystem != null)
                    {
                        if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                        {
                            sysID = (WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF).Id;
                        }
                    }
                }
                //else
                //    sysID = ((JCHVRF.Model.NextGen.SystemVRF)WorkFlowContext.CurrentSystem).Id;

                MessageBoxResult messageBoxResult = JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_DELETE_NODE"),
                    MessageType.Warning, System.Windows.MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var NodeType = DeleteNode.GetType().Name;
                    string pic = "";
                    // Node selectedNode = null;
                    // bool blnIsLastNode = false;
                    // MasterDesigner mdObj = new SelectNextNodeOnDeletionMasterDesigner(projectLegacy);
                    // var selectedNodelist = addflow.Items.OfType<Node>().ToArray();
                    //foreach (Node a in selectedNodelist)
                    //{
                    //    var NodeType = a.GetType().Name;
                    //    if (NodeType == "MyNodeIn" || NodeType == "MyNodeOut" || NodeType == "Node")
                    //    {
                    //        var equipmentProperties = NextGenBLL.PipingBLL.GetNodeProperty(a);
                    //        selectedNode = a;
                    //    }
                    //}
                    //if (selectedNode != null)
                    //{
                    //    if ((NextGenBLL.PipingBLL.GetNodeProperty(selectedNode).ToString()) != "False")
                    //    {
                    //        NextGenModel.ImageData equipmentProperties = (NextGenModel.ImageData)NextGenBLL.PipingBLL.GetNodeProperty(selectedNode);
                    //        if (equipmentProperties.equipmentType == "Indoor" || equipmentProperties.equipmentType == "Outdoor")
                    //        {
                    //            equipmentProperties.NodeNo = Convert.ToInt32(equipmentProperties.UniqName.Remove(0, 3));
                    //            //MasterDesigner.blnIsCallfromDelete = true;
                    //            OnSelectEquipment(equipmentProperties);
                    //        }
                    //    }
                    //}

                    //if (selectedNode != null)
                    //{

                    //    NextGenModel.ImageData NodeDataNext = (NextGenModel.ImageData)NextGenBLL.PipingBLL.GetNodeProperty(selectedNode);
                    //    mdObj.SelectNextNodeOnDeletion(NodeDataNext, blnIsLastNode);
                    //    selectedNode.IsSelected = true;
                    //}
                    //else
                    //{
                    //    blnIsLastNode = true;
                    //    mdObj.SelectNextNodeOnDeletion(null, blnIsLastNode);
                    //}
                    if (DeleteNode.ImageData != null)
                    {                        
                        NextGenModel.ImageData NodeData = DeleteNode.ImageData;
                        if (DeleteNode.ImageData.equipmentType == "Indoor")
                        {
                            addflow.Items.Remove(DeleteNode);
                            addflow.RemoveNode(DeleteNode);
                            DeleteOrphanNodeIfExist(DeleteNode);
                            foreach (RoomIndoor r in projectLegacy.RoomIndoorList)
                            {
                                //if (r.IndoorNO == NodeData.UniqName.ExtractNumberFromEnd() && r.SystemID == sysID)
                                //{
                                //    projectLegacy.RoomIndoorList.Remove(r);
                                //    break;
                                //}
                                if (r.IndoorName.ExtractNumberFromEnd() == NodeData.UniqName.ExtractNumberFromEnd() && r.SystemID == sysID)
                                {
                                    projectLegacy.RoomIndoorList.Remove(r);
                                    break;
                                }
                            }

                            //#region  bug fix 3423
                            //foreach (var ri in projectLegacy.RoomIndoorList.Where(ri => ri.SystemID == sysID).ToList())
                            //{
                            //    projectLegacy.RoomIndoorList.Remove(ri);
                            //    break;
                            //}
                            //#endregion bug fix 3423
                            //Start: Bug#1646
                            if (NodeType == "MyNodeIn" || NodeType == "MyNodeOut")
                            {
                                //lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Red);
                                //lblCanvasError.BorderThickness = new Thickness(2);
                                //lblCanvasError.Content =
                                    //"The diagram is not complete or there are some excess units, please amend it";
                                ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, Langauge.Current.GetMessage("ERROR_DIAGRAM"));
                                try
                                {
                                    if (this.projectLegacy.SystemListNextGen.FirstOrDefault(a => a.Id == sysID).IsManualPiping == false)
                                    {
                                    this.projectLegacy.SystemListNextGen.FirstOrDefault(a => a.Id == sysID)
                                        .MyPipingNodeOut = null;
                                    this.projectLegacy.SystemListNextGen.FirstOrDefault(a => a.Id == sysID)
                                        .MyPipingNodeOutTemp = null;
                                    this.projectLegacy.SystemListNextGen.FirstOrDefault(a => a.Id == sysID)
                                        .MyPipingNodeOut = null;
                                    this.projectLegacy.SystemListNextGen.FirstOrDefault(a => a.Id == sysID).IsPipingOK =
                                        false;
                                }
                                }
                                catch (Exception)
                                {
                                }
                            }
                            //Start: Bug#1646

                            UtilTrace.SaveHistoryTraces();
                        }
                        else if (DeleteNode.ImageData.equipmentType == "Outdoor")
                        {
                            JCHMessageBox.Show("Cannot Delete the Outdoor Item.Delete the system from Menu.");
                            //MessageBoxResult MsgboxSystemDelete = System.Windows.JCHMessageBox.Show("Deleting Outdoor will delete the system.Are you sure you want to proceed with deletion?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                            //if (projectLegacy.SystemListNextGen.Count > 1)
                            //{
                            //    if (MsgboxSystemDelete == MessageBoxResult.Yes)
                            //    {
                            //        addflow.RemoveNode(DeleteNode);
                            //        _eventAggregator.GetEvent<CanvasEqupmentDeleteSubscriber>().Publish(true);
                            //        if (projectLegacy.CanvasODUList != null && projectLegacy.CanvasODUList.Count > 0)
                            //        {
                            //            foreach (JCHVRF.Model.NextGen.SystemVRF r in projectLegacy.CanvasODUList)
                            //            {
                            //                if (r.NO == Convert.ToInt32(NodeData.UniqName.Remove(0, 3)))
                            //                {
                            //                    projectLegacy.CanvasODUList.Remove(r);
                            //                    break;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                            //else
                            //{ JCHMessageBox.Show("Cannot Delete the Last system"); }
                        }

                        else
                        {
                            //Delete JCHNode on Controller Addflow
                            if (_currentSystem.HvacSystemType == "6")
                            {
                                ucViewModel = (ucDesignerCanvasViewModel)this.DataContext;
                                ucViewModel.DeleteNodeForController(DeleteNode);
                                DeleteControllersInSet(DeleteNode);
                            }
                            addflow.Items.Remove(DeleteNode);
                            addflow.RemoveNode(DeleteNode);
                            DeleteOrphanNodeIfExist(DeleteNode);
                           // _currentSystem.SystemStatus = SystemStatus.WIP;
                            UtilTrace.SaveHistoryTraces();
                        }
                        
                        OnSelectEquipment(null);
                    }
                    else
                    {
                        if (NodeType.Equals("JCHNode"))
                        {
                            var node = (NextGenModel.JCHNode)DeleteNode;
                        }

                        if (NodeType.Equals("MyNodeCH"))
                        {
                            var node = (NextGenModel.JCHNode)DeleteNode;
                            if (node != null)
                            {
                                addflow.RemoveNode(node);
                            }
                        }                    
                    }

                    //For Node Deletion from Central Controller(Auto Wiring) addflow
                    if (NodeType.Equals("WiringNodeCentralControl"))
                    {
                        ucViewModel = (ucDesignerCanvasViewModel)this.DataContext;
                        ucViewModel.controlsystem = Project.CurrentProject.ControlSystemList.Find(x => x.Id.Equals(_currentSystem.Id));
                        ucViewModel.DeleteEquipmentOnCanvas(DeleteNode);
                        DeleteControllersInSet(DeleteNode);
                        addflow.Items.Remove(DeleteNode);
                        addflow.RemoveNode(DeleteNode);                        
                    }
                    _eventAggregator.GetEvent<AddEquipmentList>().Publish(DeleteNode);
                    UtilTrace.SaveHistoryTraces();
                }
                _eventAggregator.GetEvent<SendControllerDetails>().Publish(_currentSystem);
            }
            catch (Exception ex)
            {
            }

        }
        
        private void ShowErrorNotification()
        {
            //lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Red);
            //lblCanvasError.BorderThickness = new Thickness(2);
            //lblCanvasError.Content = "The diagram is not complete or there are some excess units, please amend it";
            ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.PipingErrors, Langauge.Current.GetMessage("ERROR_DIAGRAM"));
        }
        private void ClearErrorNotification()
        {
            lblCanvasError.BorderBrush = new SolidColorBrush(Colors.Transparent);
            lblCanvasError.BorderThickness = new Thickness(0);
            lblCanvasError.Content = string.Empty;
        }
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;

            if (ucDesignerCanvasViewModel.CurrentSystem.HvacSystemType == "1")
            {
                var CurrentSystem = (NextGenModel.SystemVRF)ucDesignerCanvasViewModel.CurrentSystem;
                DeleteNodeOnCanvas(CurrentSystem);
            }
            //TODO - Check once 
            if (ucDesignerCanvasViewModel.CurrentSystem.HvacSystemType == "6")
            {
                var CurrentSystem1 = (JCHVRF.Model.ControlSystem)ucDesignerCanvasViewModel.CurrentSystem;
                DeleteNodeOnCanvas(CurrentSystem1);
            }
        }

        private void CoolingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;
            var CurrentSystem = (NextGenModel.SystemVRF) ucDesignerCanvasViewModel.CurrentSystem;
            CoolingOnCanvas(CurrentSystem);
        }

        private void CancelCoolingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel) this.DataContext;
            var CurrentSystem = (NextGenModel.SystemVRF) ucDesignerCanvasViewModel.CurrentSystem;
            CancelCoolingOnCanvas(CurrentSystem);
        }

        void AddEquipmentOnCanvas(NextGenModel.ImageData imgData)
        {
            ProjectBLL pBLL = new ProjectBLL(JCHVRF.Model.Project.CurrentProject);
            string namePrefix;
            string SystemID = string.Empty;
            NextGenModel.SystemVRF currentSystem = null;
            if (_currentSystem.HvacSystemType.Equals("1"))
            {
                currentSystem = (NextGenModel.SystemVRF) _currentSystem;
            }

            if (currentSystem != null)
            {
                SystemID = currentSystem.Id;
            }

            if (imgData.equipmentType == "Indoor")
            {

                //var itemIndex = JCHVRF.Model.Project.CurrentProject.RoomIndoorList.FindIndex(x => x.SystemID == SystemID && x.IndoorNO == imgData.NodeNo);
                //if (itemIndex >= 0)
                //{
                //    JCHVRF.Model.Project.CurrentProject.RoomIndoorList.RemoveAt(itemIndex);
                //}

                //Indoor indoor = new Indoor
                //{
                //    Type = imgData.imageName
                //};
                //namePrefix = SystemSetting.UserSetting.defaultSetting.IndoorName;
                //var NodeName = namePrefix + Convert.ToString(imgData.NodeNo);//Using Name Prefixes
                //if (!(JCHVRF.Model.Project.CurrentProject.RoomIndoorList.Any(x => x.SystemID == SystemID && x.IndoorNO == imgData.NodeNo)))
                //{
                //    double wbCool = SystemSetting.UserSetting.defaultSetting.IndoorCoolingWB;
                //    double dbHeat = SystemSetting.UserSetting.defaultSetting.IndoorHeatingDB;
                //    var roomIndoor = pBLL.AddIndoor("1", indoor);  // This line is commented because it add a indoor node to obj
                //    roomIndoor.IndoorName = NodeName;
                //    roomIndoor.DisplayImagePath = imgData.imagePath;
                //    roomIndoor.IndoorNO = imgData.NodeNo;
                //    roomIndoor.DisplayImageName = imgData.imageName;
                //    roomIndoor.SystemID = SystemID;                    
                //}                
            }

            if (imgData.equipmentType == "Outdoor")
            {
                Outdoor outDooor = new Outdoor
                {
                    Series = imgData.imageName,
                };

                if (JCHVRF.Model.Project.CurrentProject.SystemListNextGen == null ||
                    JCHVRF.Model.Project.CurrentProject.CanvasODUList == null)
                {
                    JCHVRF.Model.Project.CurrentProject.SystemListNextGen = new List<NextGenModel.SystemVRF>();
                    JCHVRF.Model.Project.CurrentProject.CanvasODUList = new List<NextGenModel.SystemVRF>();
                }

                NextGenModel.SystemVRF ODUItem = new NextGenModel.SystemVRF();

                namePrefix = SystemSetting.UserSetting.defaultSetting.OutdoorName;
                ODUItem.Name =
                    namePrefix +
                    Convert.ToString(imgData.NodeNo); //NamePrefix is used instead of hardcoded string "Sys" in SER
                ODUItem.NO = imgData.NodeNo;
                ODUItem.IsAuto = true;
                ODUItem.SysType = SystemType.OnlyIndoor;
                ODUItem.OutdoorItem = outDooor;
                ODUItem.Series = imgData.imageName;
                ODUItem.DisplayImageName = imgData.imagePath;

                ODUItem.DBCooling = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingDB;
                ODUItem.DBHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingDB;
                ODUItem.WBHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingWB;
                ODUItem.RHHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingRH;
                ODUItem.PipeEquivalentLength = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                ODUItem.PipeEquivalentLengthbuff = SystemSetting.UserSetting.pipingSetting.pipingEqLength;
                ODUItem.FirstPipeLength = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
                ODUItem.FirstPipeLengthbuff = SystemSetting.UserSetting.pipingSetting.firstBranchLength;
                ODUItem.HeightDiff = SystemSetting.UserSetting.pipingSetting.pipingHighDifference;
                ODUItem.PipingLengthFactor = SystemSetting.UserSetting.pipingSetting.pipingCorrectionFactor;
                ODUItem.PipingPositionType = SystemSetting.UserSetting.pipingSetting.pipingPositionType;
                ODUItem.IWCooling = SystemSetting.UserSetting.defaultSetting.OutdoorCoolingIW;
                ODUItem.IWHeating = SystemSetting.UserSetting.defaultSetting.OutdoorHeatingIW;
                JCHVRF.Model.Project.CurrentProject.CanvasODUList.Add(ODUItem);
            }
        }

        //end for context menu canvas

        #region drawOutdoor

        public void getOutdoorDetails()
        {
            //TO DO to make it multi system specific
            if (projectLegacy.SystemListNextGen[0].OutdoorItem != null)
            {
                NextGenModel.ImageData imgData = new NextGenModel.ImageData();
                imgData.equipmentType = "Outdoor";
            }
        }

        #endregion

        #region Manual Piping 

        public void SelectLineStyle(object sender, MouseEventArgs e)
        {
            object data;
            WL.LineStyle lineStyle;
            System.Windows.Controls.Image image = e.Source as System.Windows.Controls.Image;
            TextBlock tb = e.Source as TextBlock;
            if (image != null)
            {
                data = (NextGenModel.ImageData) image.DataContext;
            }
            else
            {
                data = (NextGenModel.ImageData) tb.DataContext;
            }

            if (data != null && ((NextGenModel.ImageData) data).imageName.ToLower().Contains("linestyle"))
            {
                lineStyle = GetLineStyleByImageName(((NextGenModel.ImageData) data).imageName);
                SetLineStyle(lineStyle);
            }
        }

        private WL.LineStyle GetLineStyleByImageName(string imageName)
        {
            string imageFileType = Path.GetFileNameWithoutExtension(imageName);
            int underscoreIndex = imageFileType.LastIndexOf('_');
            imageFileType = imageFileType.Substring(underscoreIndex + 1);

            switch (imageFileType)
            {
                case "HV":
                    return WL.LineStyle.HV;

                case "HVH":
                    return WL.LineStyle.HVH;

                case "Polyline":
                    return WL.LineStyle.Polyline;

                case "VH":
                    return WL.LineStyle.HV;

                case "VHV":
                    return WL.LineStyle.HV;

                default:
                    break;
            }

            return WL.LineStyle.VH;
        }

        private void SetLineStyle(WL.LineStyle ls)
        {
            foreach (var linkItem in this.addflow.SelectedItems)
            {
                if (linkItem is WL.Link)
                {
                    WL.Link link = linkItem as WL.Link;
                    link.LineStyle = ls;

                    ResetLinkPointLocation(link, true);
                    ResetLinkPointLocation(link, false);
                }
            }
        }

        private void OnHeatExchangerClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (HeatExchangerCanvasEquipmentViewModel) heatExchangerCanvas.DataContext;
            viewModel.MouseClickCommandClick();
        }
        JCHNode currentHoverNode = null;
        private Color nodeBackgroundColor =Colors.White;
        private Color pipeColor =Colors.Black;

        private void addflow_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Load_DeleteIcon(e);
                }

                double left = e.GetPosition(addflow).X;
                double top = e.GetPosition(addflow).Y;
                NextGenModel.SystemVRF currentSystem = (NextGenModel.SystemVRF)_currentSystem;
                var selectedItem = addflow.GetItemAt(new Point(left, top));

                if (selectedItem != null)
                {
                    currentHoverNode = null;
                    if (!(selectedItem is JCHNode))
                    {

                        return;
                    }

                    selectedItem.Stroke = new SolidColorBrush(Color.FromRgb(45, 221, 250));
                    if (((JCHVRF.Model.NextGen.JCHNode) selectedItem).ImageData == null)
                    {
                        return;
                    }

                    if (((JCHVRF.Model.NextGen.JCHNode) selectedItem).ImageData.equipmentType == null)
                    {
                        return;
                    }

                    if ((((JCHVRF.Model.NextGen.JCHNode) selectedItem).ImageData.equipmentType == "Indoor")
                        || (((JCHVRF.Model.NextGen.JCHNode) selectedItem).ImageData.equipmentType == "Outdoor")
                        || (((JCHVRF.Model.NextGen.JCHNode) selectedItem).ImageData.equipmentType == "Controller"))
                    {
                        currentHoverNode = selectedItem as JCHNode;
                        //selectedItem.Stroke = new SolidColorBrush(Color.FromRgb(45, 221, 250));
                        selectedItem.StrokeThickness = 2;

                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            Mouse.SetCursor(_grabbingCursor);
                            //selectedItem.Tooltip = selectedItem.Bounds.Location.ToString();
                            //DetectNodeRegion(selectedItem, sender, e);

                            //JCHMessageBox.Show("X:"+ selectedItem.Bounds.Location);
                        }
                        else if (e.LeftButton == MouseButtonState.Released)
                        {
                            Mouse.SetCursor(_grabCursor);
                        }

                        SetIndoorOutdoorUnitFillStyle(selectedItem as JCHNode);
                    }

                    SetControllerFillStyle(selectedItem as JCHNode);
                }
                else
                {
                    // Mouse.SetCursor(Cursors.Arrow);
                    currentHoverNode = null;
                    SetIndoorOutdoorUnitFillStyle(selectedItem as JCHNode);
                    SetControllerFillStyle(selectedItem as JCHNode);
                    var items = addflow.Items;
                    foreach (var item in items)
                    {
                        if (item.ToString().Contains("JCHNode"))
                        {
                            item.Stroke = new SolidColorBrush(Color.FromRgb(185, 185, 185));
                        }

                        if (item.ToString().Contains("WiringNodeCentralControl"))
                        {
                            item.Stroke = new SolidColorBrush(Color.FromRgb(185, 185, 185));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Addflow_SelectedItemsLayouting(object sender, WL.SelectedItemsLayoutingEventArgs e)
        {
            var addflow = sender as WL.AddFlow;
            if (addflow == null)
            {
                return;
            }
            var selectedNodeFirst = addflow.SelectedItems.First() as JCHNode;
            if (selectedNodeFirst is NextGenModel.MyNodePlottingScale)
                {
                    GetPipingBLLInstance().DrawPlottingScaleNode(GetCurrentSystem(), selectedNodeFirst as NextGenModel.MyNodePlottingScale, ref addflow);
                }

                if (addflow.SelectedItems.OfType<JCHNode>().Any())
            {
                foreach (var selectedNode in addflow.SelectedItems.OfType<JCHNode>().Where(x => x.ImageData != null).Where(x => x.IsIndoorUnit()
                                                                                                   || x.IsOutdoorUnit()))
                {
                    DrawMandatoryNodeVisuals(selectedNode);

                    if (selectedNode is NextGenModel.MyNodePlottingScale)
                    {
                        GetPipingBLLInstance().DrawPlottingScaleNode(GetCurrentSystem(), selectedNode as NextGenModel.MyNodePlottingScale, ref addflow);
                    }
                }

                //if (!CurrentSystem.IsManualPiping)
                //{
                //    //CurrentSystem.IsManualPiping = true;
                //    //SetManualPipingModelOnAddFlow(ref addflow);
                //}

                //if (CurrentSystem.IsManualPiping)
                //{
                //    SaveManualPiping();
                //}
            }
        }
        private bool DrawManualPipingNodes(JCHNode org, JCHNode dst, Point? location = null, WL.Link linkInvolved = null)
        {
            var isInvalidLink = false;
            try
            {
                if (org == null || dst == null)
                {
                    return false;
                }
                if (CurrentSystem != null && CurrentSystem.IsManualPiping)
                {
                    var cancelCurrentLink = false;
                    //Unsubscribe
                    addflow.LinkCreating -= Addflow_LinkCreating;
                    var source = org as JCHNode;
                    var destination = dst as JCHNode;
                    if (IsYpNeeded(addflow, source, destination) && linkInvolved == null)
                    {
                        JCHNode ypNode = CreateNewYpNode();

                        var ypSourceNode = destination.IsIndoorUnit() ? destination : source;

                        var newYpParent = source is NextGenModel.MyNodeOut ? source : destination is NextGenModel.MyNodeOut ? destination :
                            destination is NextGenModel.MyNodeYP ? destination : source;

                        var newYpNodeChild = source.IsIndoorUnit() ? source : destination;

                        var ypLocation = new Point(utilPiping.getTopCenterPointF(ypSourceNode).X - 12.5, utilPiping.getTopCenterPointF(ypSourceNode).Y - 50);
                        if (location != null)
                        {
                            ypLocation = location.Value;
                        }
                        var ypParentNode = (ypNode as NextGenModel.MyNodeYP);
                        if (ypParentNode != null)
                        {
                            ypParentNode.ParentNode = newYpParent as NextGenModel.MyNode;
                            if (newYpParent is NextGenModel.MyNodeYP)
                                (newYpParent as NextGenModel.MyNodeYP).AddChildNode(ypNode);

                            ypParentNode.AddChildNode(newYpNodeChild);
                        }

                        ypNode.Location = ypLocation;
                        addflow.AddNode(ypNode);

                        GetPipingBLLInstance().DrawAutomaticYpLinks(ypNode as NextGenModel.MyNodeYP, addflow, CurrentSystem);
                        ypNode.BringIntoView();

                        cancelCurrentLink = true;
                    }
                    else
                    {
                        if (source is NextGenModel.MyNodeYP && linkInvolved == null)
                        {
                            if (destination.Links.Any())
                            {
                                (source as NextGenModel.MyNodeYP).AddChildNode(destination);
                            }
                            else
                            {
                                //if ((source as NextGenModel.MyNodeYP).ChildNodes.Count() > 2)
                                //{

                                //}
                                //else
                                {
                                    (source as NextGenModel.MyNodeYP).AddChildNode(destination);
                                    NextGenModel.MyNodeYP.SetRightOutletId(source as NextGenModel.MyNodeYP,
                                        destination.ImageData.UniqName);
                                }
                            }
                        }
                        else if (source.IsIndoorUnit() && destination.IsIndoorUnit())
                        {
                            // either destination or source should not be connected
                            if (destination.Links.Any() || source.Links.Any())//Allready connected.
                            {
                                var nodeWithLinks = destination.Links.Any() ? destination : source;

                                JCHNode ypNode = CreateNewYpNode();
                                var ypSourceNode = nodeWithLinks;
                                var newYpParent = ypSourceNode.Links.Count > 0 ? ypSourceNode.Links.First().Org : source;
                                if (linkInvolved != null && nodeWithLinks.Links.Any(x => x == linkInvolved))
                                {
                                    DeleteLink = nodeWithLinks.Links.First(x => x == linkInvolved);
                                }
                                else
                                {
                                    DeleteLink = nodeWithLinks.Links.First();
                                }

                                DeleteLinkMenu_Click(null, null);
                                cancelCurrentLink = true;
                                if (DeleteLink.Dst == destination && destination is NextGenModel.MyNodeIn)
                                {
                                    destination.Links.Clear();
                                }
                                if (DeleteLink.Org == destination && destination is NextGenModel.MyNodeIn)
                                {
                                    destination.Links.Clear();
                                }
                                //newYpParent = DeleteLink.Org.Bounds.Left < DeleteLink.Dst.Bounds.Left
                                //    ? DeleteLink.Org
                                //    : DeleteLink.Dst;
                                newYpParent = DeleteLink.Org;

                                if (!(DeleteLink.Org is NextGenModel.MyNodeYP) && (DeleteLink.Dst is NextGenModel.MyNodeYP))
                                {
                                    newYpParent = DeleteLink.Dst;
                                }

                                var ypLocation = new Point(utilPiping.getTopCenterPointF(ypSourceNode).X - 12.5, utilPiping.getTopCenterPointF(ypSourceNode).Y - 50);
                                if (location != null)
                                {
                                    ypLocation = location.Value;
                                }

                                var ypParentNode = ypNode as NextGenModel.MyNodeYP;

                                if (ypParentNode != null)
                                {
                                    ypParentNode.ParentNode = newYpParent as NextGenModel.MyNode;
                                    if (newYpParent is NextGenModel.MyNodeYP)
                                        (newYpParent as NextGenModel.MyNodeYP).AddChildNode(ypNode);

                                    var outlet = destination == ypSourceNode ? source : destination;

                                    // connect new yp with idus

                                    var idu1 = DeleteLink.Dst is NextGenModel.MyNodeIn ? DeleteLink.Dst : DeleteLink.Org;
                                    var idu2 = DeleteLink.Dst == destination ? outlet : destination;

                                    var qd1 = 0;
                                    var qd2 = 0;

                                    //quadrant map
                                    //1|2
                                    //4|3
                                    qd1 = GetQuadradant(ypLocation, idu1);
                                    qd2 = GetQuadradant(ypLocation, idu2);

                                    if(qd1 == qd2) // falling in the same quadrant
                                    {
                                        if(qd1== 3 || qd1 == 4)
                                        {
                                            if(idu1.Location.X < idu2.Location.X)
                                            {
                                                ypParentNode.AddChildNode(idu2);
                                                ypParentNode.AddChildNode(idu1);
                                            }
                                            else
                                            {
                                                ypParentNode.AddChildNode(idu1);
                                                ypParentNode.AddChildNode(idu2);
                                            }
                                        }
                                        else if(qd1 == 1 || qd1 == 2)
                                        {
                                            if (idu1.Location.X > idu2.Location.X)
                                            {
                                                ypParentNode.AddChildNode(idu2);
                                                ypParentNode.AddChildNode(idu1);
                                            }
                                            else
                                            {
                                                ypParentNode.AddChildNode(idu1);
                                                ypParentNode.AddChildNode(idu2);
                                            }
                                        }
                                      
                                    }
                                    if (qd1 != qd2) // in different quadrant
                                    {
                                        // upper adajacent
                                        if((qd1 == 1 && qd2 == 2) || (qd1 == 2 && qd2 == 1))
                                        {
                                            if(qd2 > qd1)
                                            {
                                                ypParentNode.AddChildNode(idu1);
                                                ypParentNode.AddChildNode(idu2);
                                            }
                                            else
                                            {
                                                ypParentNode.AddChildNode(idu2);
                                                ypParentNode.AddChildNode(idu1);
                                            }
                                        }
                                        // lower adajacent
                                        else if ((qd1 == 3 && qd2 == 4) || (qd1 == 4 && qd2 == 3))
                                        {
                                            if (qd2 > qd1)
                                            {
                                                ypParentNode.AddChildNode(idu2);
                                                ypParentNode.AddChildNode(idu1);
                                            }
                                            else
                                            {
                                                ypParentNode.AddChildNode(idu1);
                                                ypParentNode.AddChildNode(idu2);
                                            }
                                        }

                                        // upper and lower quadrants
                                        else if (qd1 > qd2)
                                        {
                                            ypParentNode.AddChildNode(idu1);
                                            ypParentNode.AddChildNode(idu2);
                                            
                                        }
                                       else
                                        {
                                            ypParentNode.AddChildNode(idu1);
                                            ypParentNode.AddChildNode(idu2);
                                        }
                                    }

                                    ypNode.Location = ypLocation;
                                    
                                    ypNode.Location = new Point((ypNode.Location.X - ypNode.Size.Width / 2) + 4,
                                        (ypNode.Location.Y - ypNode.Size.Height / 2) + 5);
                                    var gapBetweenYpAndChild =
                                        (ypParentNode.ChildNodes[0].Location - ypNode.Location).Y;
                                    if (gapBetweenYpAndChild < 50 && gapBetweenYpAndChild > 0)
                                    {
                                        ypNode.Location = new Point(ypNode.Location.X,
                                            ypNode.Location.Y - (50 - gapBetweenYpAndChild));
                                    }
                                }

                                addflow.AddNode(ypNode);

                                GetPipingBLLInstance().DrawAutomaticYpLinks(ypNode as NextGenModel.MyNodeYP, addflow, CurrentSystem, DeleteLink);
                                ypNode.BringIntoView();
                            }
                            else
                            {
                                isInvalidLink = true;
                                cancelCurrentLink = true;
                            }
                        }
                        // Main Pipe
                        else if (linkInvolved != null)
                        {
                            if (linkInvolved.Org is NextGenModel.MyNodeYP && linkInvolved.Dst is NextGenModel.MyNodeYP)
                            {
                                // YP on the left is considered as source
                                var sourceYp = linkInvolved.Org.Bounds.Left < linkInvolved.Dst.Bounds.Left
                                    ? linkInvolved.Org
                                    : linkInvolved.Dst;

                                var destinationYp = linkInvolved.Org.Bounds.Left > linkInvolved.Dst.Bounds.Left
                                    ? linkInvolved.Org
                                    : linkInvolved.Dst;

                                var bottomOutletNode = source.IsIndoorUnit() ? source :
                                    destination.IsIndoorUnit() ? destination : source;

                                DeleteLink = linkInvolved;
                                DeleteLinkMenu_Click(null, null);
                                cancelCurrentLink = true;
                                JCHNode ypNode = CreateNewYpNode();
                                ypNode.Location = location.Value;

                                var ypParentNode = ypNode as NextGenModel.MyNodeYP;

                                //New Yp parent
                                ypParentNode.ParentNode = sourceYp as NextGenModel.MyNode;
                                if (sourceYp is NextGenModel.MyNodeYP)
                                    (sourceYp as NextGenModel.MyNodeYP).AddChildNode(ypNode);

                                if (bottomOutletNode.Bounds.Left < destinationYp.Bounds.Left)
                                {
                                    
                                    //New Yp Bottom Outlet
                                    ypParentNode.AddChildNode(bottomOutletNode);

                                    //New Yp Right Outlet
                                    ypParentNode.AddChildNode(destinationYp);
                                }
                                else
                                {
                                    if (destinationYp.Bounds.Top < bottomOutletNode.Bounds.Top)
                                    {
                                        //New Yp Bottom Outlet
                                        ypParentNode.AddChildNode(bottomOutletNode);

                                        //New Yp Right Outlet
                                        ypParentNode.AddChildNode(destinationYp);
                                    }
                                    else
                                    {
                                        //New Yp Bottom Outlet
                                        ypParentNode.AddChildNode(destinationYp);

                                        //New Yp Right Outlet
                                        ypParentNode.AddChildNode(bottomOutletNode);
                                    }

                                }
                                ypNode.Location = new Point((ypNode.Location.X - ypNode.Size.Width / 2) + 4, (ypNode.Location.Y - ypNode.Size.Height / 2) + 5);
                                var gapBetweenYpAndChild =
                                    (ypParentNode.ChildNodes[0].Location - ypNode.Location).Y;
                                if (gapBetweenYpAndChild < 0)
                                    gapBetweenYpAndChild = 0;
                                if (gapBetweenYpAndChild < 50)
                                {
                                    ypNode.Location = new Point(ypNode.Location.X,
                                        ypNode.Location.Y - (50 - gapBetweenYpAndChild));
                                }
                                addflow.AddNode(ypNode);

                                GetPipingBLLInstance().DrawAutomaticYpLinks(ypNode as NextGenModel.MyNodeYP, addflow, CurrentSystem, DeleteLink);
                                ypNode.BringIntoView();
                            }

                        }
                        else if ((source.IsIndoorUnit() && destination.IsYpNode()) || (destination.IsIndoorUnit() && source.IsYpNode()))
                        {
                            if (source.Links.Any() || destination.Links.Count() > 2)
                            {
                                cancelCurrentLink = true;
                            }
                        }
                    }

                    //ReSubscribe
                    addflow.LinkCreating += Addflow_LinkCreating;
                    if (cancelCurrentLink && !isInvalidLink)
                    {
                        DeleteOrphanNodeIfExist(org);
                        DeleteOrphanNodeIfExist(dst);
                    }
                    CheckForErrorState();
                   
                    return cancelCurrentLink;
                }
                CheckForErrorState();
                return false;
            }
            catch (Lassalle.Flow.AddFlowException flowException)
            {
                Logger.LogProjectError(Project.GetProjectInstance?.projectID, flowException, true);
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogProjectError(Project.GetProjectInstance?.projectID, ex, false);
                return false;
            }
        }

        private static int GetQuadradant(Point location, WL.Node idu1)
        {
            var output = 1;

            if (idu1.Location.X <= location.X && idu1.Location.Y < location.Y)
            {
                output = 1;
            }

            if (idu1.Location.X > location.X && idu1.Location.Y < location.Y)
            {
                output = 2;
            }

            if (idu1.Location.X > location.X && idu1.Location.Y >= location.Y)
            {
                output = 3;
            }

            if (idu1.Location.X <= location.X && idu1.Location.Y > location.Y)
            {
                output = 4;
            }

            return output;

        }

        private JCHNode CreateNewYpNode(NodeType type = NodeType.YP)
        {
            var existingYpCount = addflow.Items.OfType<JCHNode>().Count(x => x is NextGenModel.MyNodeYP);

            if (type == NodeType.YP || type == NodeType.CP)
            {
                //Auto Create YP Node
                var ypNode = GetPipingBLLInstance().CreateYpNode(existingYpCount, type == NodeType.CP);
                return ypNode;
            }

            if (type == NodeType.YP4)
            {
                //Auto Create YP Node
                var ypNode = GetPipingBLLInstance().CreateYp4BranchNode(existingYpCount);
                return ypNode;
            }

            return null;
        }

        #region Manual Piping Connections



        private bool IsYpNeeded(WL.AddFlow addflowControl, JCHNode source, JCHNode dest)
        {

            // skip if destination is YP node
            if (dest is NextGenModel.MyNodeYP) return false;

            // check if source and dest IDU and no link exit
            if (source.IsIndoorUnit() && dest.IsIndoorUnit() && !source.Links.Any() && !dest.Links.Any()) { return false; }

            if (!addflowControl.Items.OfType<JCHNode>()
                .Any(x => x.IsIndoorUnit() && x != dest && x != source && !x.Links.Any()))
            {
                return false;
            }

           var connectedNodes = new List<JCHNode>();
            foreach (var myLink in addflowControl.Items.OfType<WL.Link>())
            {
                var org = myLink.Org;
                var dst = myLink.Dst;

                if (!connectedNodes.Contains(org) && org is JCHNode && (org as JCHNode).IsIndoorUnit() && org != source && dst != dest)
                {
                    connectedNodes.Add(org as JCHNode);
                }
                if (!connectedNodes.Contains(dst) && dst is JCHNode && (dst as JCHNode).IsIndoorUnit() && org != source && dst != dest)
                {
                    connectedNodes.Add(dst as JCHNode);
                }
            }

            var anyOrphans = addflowControl.Items.OfType<JCHNode>().Any(x => x.IsIndoorUnit() && x != dest && !connectedNodes.Contains(x));


            if (source is NextGenModel.MyNodeYP && (source as NextGenModel.MyNodeYP).ChildNodes.Count() == 2)
            {
                if (source.Links.Count < 2) { return false; }
            }

            //if (dest is NextGenModel.MyNodeYP && (dest as NextGenModel.MyNodeYP).ChildNodes.Count() == 2)
            //{
            //    if (dest.Links.Count < 2) { return false; }
            //}

            var orphanYpNodes = addflowControl.Items.OfType<JCHNode>()
                .Where(yp => yp is NextGenModel.MyNodeYP && (yp as NextGenModel.MyNodeYP).ChildNodes.Count() == 2 && yp.Links.Count < 3).Where(x => x != source && x != dest);
            if (orphanYpNodes.Any())
            {
                return false;
            }

            return anyOrphans;
        }

        private void Addflow_LinkDeleted(object sender, WL.LinkDeletedEventArgs e)
        {
            DeleteLinkProperties(e.Link);
            if (_currentSystem == null)
            {
                _currentSystem = WorkFlowContext.CurrentSystem;
            }
            if (_currentSystem != null)
            {
                if (_currentSystem.HvacSystemType.Equals("1"))
                {
                    CurrentSystem.IsManualPiping = true;
                    CurrentSystem.SystemStatus = SystemStatus.WIP;
                    SetManualPipingModelOnAddFlow(ref addflow);
                    CheckForErrorState();
                }
                else
                {
                    //TODO: SetAutoWiringModelOnAddflow(ref addflow)
                }
            }

        }

        private void Addflow_MouseEnter(object sender, MouseEventArgs e)
        {
            double left = e.GetPosition(addflow).X;
            double top = e.GetPosition(addflow).Y;
            var mousePoint = new Point(left, top);
            var selectedItem = addflow.GetItemAt(mousePoint);
            if (selectedItem != null && selectedItem is WL.Link)
            {
                var link = selectedItem as WL.Link;
                if (currentHoverNode != null && currentHoverNode != link.Dst && currentHoverNode != link.Org)
                {
                    var nodeToConnect = link.Dst;
                    if (link.Dst is NextGenModel.MyNodeYP)
                    {
                        nodeToConnect = link.Org;
                    }

                    if (currentHoverNode != null && currentHoverNode is NextGenModel.MyNodeIn && currentHoverNode.Links != null && currentHoverNode.Links.Count == 0)
                    {
                        DrawManualPipingNodes(nodeToConnect as JCHNode, currentHoverNode, mousePoint, link);
                    }
                }

            }
        }

        private void Addflow_LinkCreated(object sender, WL.LinkCreatedEventArgs e)
        {
            if (_currentSystem != null)
                if (_currentSystem.HvacSystemType.Equals("6")) return;
            if (InitPageLoad == 0)
            {
                if (!CurrentSystem.IsManualPiping) return;

                GetPipingBLLInstance().SetManualPipingLineStyle(e.Link);
                SetYpLinkProperties(e.Link);

                CheckForErrorState();
                SetIndoorOutdoorUnitFillStyle();
            }
        }

        private void SetYpLinkProperties(WL.Link link)
        {
            if (link != null)
            {
                if (link.Org is NextGenModel.MyNodeYP)
                {
                    if (link.PinOrgIndex == 0) NextGenModel.MyNodeYP.SetInLetId(link.Org, (link.Dst as JCHNode).GetUniqueName());
                    if (link.PinOrgIndex == 1) NextGenModel.MyNodeYP.SetBottomOutletId(link.Org, (link.Dst as JCHNode).GetUniqueName());
                    if (link.PinOrgIndex == 2) NextGenModel.MyNodeYP.SetRightOutletId(link.Org, (link.Dst as JCHNode).GetUniqueName());
                }
                if (link.Dst is NextGenModel.MyNodeYP)
            {
                    if (link.PinDstIndex == 0) NextGenModel.MyNodeYP.SetInLetId(link.Dst, (link.Org as JCHNode).GetUniqueName());
                    if (link.PinDstIndex == 1) NextGenModel.MyNodeYP.SetBottomOutletId(link.Dst, (link.Org as JCHNode).GetUniqueName());
                    if (link.PinDstIndex == 2) NextGenModel.MyNodeYP.SetRightOutletId(link.Dst, (link.Org as JCHNode).GetUniqueName());
                }
            }
            }

        #endregion

        private void Addflow_LinkCreating(object sender, WL.LinkCreatingEventArgs e)
        {
            if (_currentSystem != null)            
                if (_currentSystem.HvacSystemType.Equals("6")) return;

                if (InitPageLoad == 0)
                {
                CurrentSystem = GetCurrentSystem();
                if (CurrentSystem != null && !CurrentSystem.IsManualPiping) { return; }

                    if (addflow.Items.OfType<WL.Link>().Any(x => x.Org == e.Org && x.Dst == e.Dst) ||
                        addflow.Items.OfType<WL.Link>().Any(x => x.Dst == e.Org && x.Org == e.Dst))
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }
                    if (e.Org is JCHVRF.Model.NextGen.MyNodeYP && ((JCHVRF.Model.NextGen.MyNodeYP)e.Org) != null && ((JCHVRF.Model.NextGen.MyNodeYP)e.Org).ChildCount == 2)
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }
                    if (e.Dst is JCHVRF.Model.NextGen.MyNodeYP && ((JCHVRF.Model.NextGen.MyNodeYP)e.Dst) != null && ((JCHVRF.Model.NextGen.MyNodeYP)e.Dst).ChildCount == 2)
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }
                    if (e.Org is JCHVRF.Model.NextGen.MyNodeOut && ((JCHVRF.Model.NextGen.MyNodeOut)e.Org) != null && ((JCHVRF.Model.NextGen.MyNodeOut)e.Org).Links != null && ((JCHVRF.Model.NextGen.MyNodeOut)e.Org).Links.Count > 0)
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }
                    if (e.Dst is JCHVRF.Model.NextGen.MyNodeOut && ((JCHVRF.Model.NextGen.MyNodeOut)e.Dst) != null && ((JCHVRF.Model.NextGen.MyNodeOut)e.Dst).Links != null && ((JCHVRF.Model.NextGen.MyNodeOut)e.Dst).Links.Count > 0)
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }
                    if (e.Org is JCHVRF.Model.NextGen.MyNodeIn && ((JCHVRF.Model.NextGen.MyNodeIn)e.Org) != null && ((JCHVRF.Model.NextGen.MyNodeIn)e.Org).Links != null && ((JCHVRF.Model.NextGen.MyNodeIn)e.Org).Links.Count > 0)
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }
                    if (e.Dst is JCHVRF.Model.NextGen.MyNodeIn && ((JCHVRF.Model.NextGen.MyNodeIn)e.Dst) != null && ((JCHVRF.Model.NextGen.MyNodeIn)e.Dst).Links != null && ((JCHVRF.Model.NextGen.MyNodeIn)e.Dst).Links.Count > 0)
                    {
                        e.Cancel.Cancel = true;
                        return;
                    }

                    var result = DrawManualPipingNodes(e.Org as JCHNode, e.Dst as JCHNode);
                    e.Cancel.Cancel = result;
                    DeleteOrphanNodeIfExist(e.Dst as JCHNode);
                    
                if (CurrentSystem.IsPipingOK)
                    CurrentSystem.SystemStatus = SystemStatus.VALID;
            }
            
        }       
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_eventAggregator == null)
            {
                return;
            }
            _grabCursor = ((TextBlock)Resources["CursorGrab"]).Cursor;
            _grabbingCursor = ((TextBlock)Resources["CursorGrabbing"]).Cursor;

            _eventAggregator.GetEvent<CanvasBuildingImageSubscriber>().Subscribe(OnImageImportClicked);
            _eventAggregator.GetEvent<CanvasLockBuildingImageSubscriber>().Subscribe(OnBuildingImageLockChecked);
            _eventAggregator.GetEvent<CanvasBuildingImageOpacitySubscriber>().Subscribe(OnBuildingImageOpacityChange);
            _eventAggregator.GetEvent<CanvasPlottingScaleEnableDisableSubscriber>()
                .Subscribe(OnPlottingScaleSelectionChange);
            _eventAggregator.GetEvent<CanvasPlottingScaleDirectionChangeSubscriber>()
                .Subscribe(OnPlottingScaleDirectionChange);
            _eventAggregator.GetEvent<CanvasPlottingScalingMeterValueSubscriber>()
                .Subscribe(OnPlottingScaleMeterValueChange);
            _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Subscribe(OnCanvasColorPropertiesChange);
            _eventAggregator.GetEvent<ToolBarZoomInSubscriber>().Subscribe(OnCanvasZoomInClick);
            _eventAggregator.GetEvent<ToolBarZoomOutSubscriber>().Subscribe(OnCanvasZoomOutClick);
            _eventAggregator.GetEvent<CanvasCenterStageEnableChangeSubscriber>().Subscribe(OnCanvasCenterStageChange);
            _eventAggregator.GetEvent<ToolBarGridLineEnableChangeScubscriber>()
                .Subscribe(OnToolBarGridLinesEnableChange);
            _eventAggregator.GetEvent<RefreshCanvas>().Subscribe(RefreshCanvas);
            _eventAggregator.GetEvent<NavigatorZoomBoxSubscriber>().Subscribe(OnNavigatorControlLoad);
            _eventAggregator.GetEvent<NavigatorZoomValueChangeSubscriber>().Subscribe(OnNavigatorZoomChange);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<CanvasBuildingImageSubscriber>().Unsubscribe(OnImageImportClicked);
            _eventAggregator.GetEvent<CanvasLockBuildingImageSubscriber>().Unsubscribe(OnBuildingImageLockChecked);
            _eventAggregator.GetEvent<CanvasBuildingImageOpacitySubscriber>().Unsubscribe(OnBuildingImageOpacityChange);
            _eventAggregator.GetEvent<CanvasPlottingScaleEnableDisableSubscriber>()
                .Unsubscribe(OnPlottingScaleSelectionChange);
            _eventAggregator.GetEvent<CanvasPlottingScaleDirectionChangeSubscriber>()
                .Unsubscribe(OnPlottingScaleDirectionChange);
            _eventAggregator.GetEvent<CanvasPlottingScalingMeterValueSubscriber>()
                .Unsubscribe(OnPlottingScaleMeterValueChange);
            _eventAggregator.GetEvent<CanvasColorPropertyChangeSubscriber>().Unsubscribe(OnCanvasColorPropertiesChange);
            _eventAggregator.GetEvent<ToolBarZoomInSubscriber>().Unsubscribe(OnCanvasZoomInClick);
            _eventAggregator.GetEvent<ToolBarZoomOutSubscriber>().Unsubscribe(OnCanvasZoomOutClick);
            _eventAggregator.GetEvent<CanvasCenterStageEnableChangeSubscriber>().Unsubscribe(OnCanvasCenterStageChange);
            _eventAggregator.GetEvent<ToolBarGridLineEnableChangeScubscriber>()
                .Unsubscribe(OnToolBarGridLinesEnableChange);
            _eventAggregator.GetEvent<RefreshCanvas>().Unsubscribe(RefreshCanvas);
            _eventAggregator.GetEvent<NavigatorZoomBoxSubscriber>().Unsubscribe(OnNavigatorControlLoad);
            _eventAggregator.GetEvent<NavigatorZoomValueChangeSubscriber>().Unsubscribe(OnNavigatorZoomChange);
             _eventAggregator.GetEvent<InSetController>().Unsubscribe(DrawControllerInSetNode);
        }

        private void RefreshCanvas()
        {
            dynamic lastSystem =
                Project.GetProjectInstance.SystemListNextGen.FirstOrDefault(list =>
                    list.Id == Project.GetProjectInstance.SelectedSystemID);
            if (lastSystem == null)
            {
                lastSystem = Project.GetProjectInstance.ControlSystemList.FirstOrDefault(list =>
                    list.Id == Project.GetProjectInstance.SelectedSystemID);
            }

            if (lastSystem == null)
            {
                lastSystem = Project.GetProjectInstance.HeatExchangerSystems.FirstOrDefault(list =>
                    list.Id == Project.GetProjectInstance.SelectedSystemID);
            }

            if (CurrentSystem == null)
            {
                GetCurrentSystem();
            }

            if (lastSystem != null) // && CurrentSystem.Id == Project.GetProjectInstance.SelectedSystemID)
            {
                addflow.Clear();
                //foreach (var item in Project.GetProjectInstance.SystemDrawing)
                //{
                //    if (typeof(WL.Caption).Equals(item.GetType()))
                //    {
                //        addflow.AddCaption((WL.Caption)item);
                //    }

                //    if (typeof(WL.Node).Equals(item.GetType()))
                //    {
                //        addflow.AddNode((WL.Node)item);
                //    }

                //    if (typeof(WL.Link).Equals(item.GetType()))
                //    {
                //        addflow.AddLink((WL.Link)item);
                //    }
                //}

                if (lastSystem.HvacSystemType.Equals("1"))
                {
                    //ScrollViewer.Height = 410;
                    this.ScrollViewer.Content = addflow;
                    addflow.Visibility = Visibility.Visible;                    
                    heatExchangerCanvas.Visibility = Visibility.Collapsed;
                    ToggleImage.Visibility = Visibility.Visible;
                    btnPanning.Visibility = Visibility.Visible;
                    btnZoomIn.Visibility = Visibility.Visible;
                    btnZoomOut.Visibility = Visibility.Visible;
                    btnReAlign.Visibility = Visibility.Visible;

                    btnPanning.IsEnabled = true;
                    btnZoomIn.IsEnabled = true;
                    btnZoomOut.IsEnabled = true;
                    btnReAlign.IsEnabled = true;

                    brdToggleimg.Visibility = Visibility.Collapsed;
                    //brdParent.Background=Brushes.White;
                    //brdTabular.Background = Brushes.White;
                    //if (string.IsNullOrEmpty(_currentSystem.SystemState))
                    //{
                    DrawPreSelectedNodesOnEdit(ref addflow, lastSystem);
                    //}
                }

                else if (lastSystem.HvacSystemType.Equals("6"))
                {
                    //ScrollViewer.Height = 410;                    
                    addflow.Visibility = Visibility.Visible;                    
                    heatExchangerCanvas.Visibility = Visibility.Collapsed;
                    ToggleImage.Visibility = Visibility.Visible;
                    btnPanning.Visibility = Visibility.Collapsed;
                    btnZoomIn.Visibility = Visibility.Collapsed;
                    btnZoomOut.Visibility = Visibility.Collapsed;
                    btnReAlign.Visibility = Visibility.Collapsed;
                    btnPanning.IsEnabled = true;
                    btnZoomIn.IsEnabled = true;
                    btnZoomOut.IsEnabled = true;
                    btnReAlign.IsEnabled = true;
                    brdToggleimg.Visibility = Visibility.Collapsed;
                    ControlSystem cc = lastSystem as ControlSystem;
                    DrawControllerNodesonRefresh(ref addflow, cc);
                    //DrawControllerNodesonRefresh(ref addflow, lastSystem);
                    this.ScrollViewer.Content = addflow;
                    _eventAggregator.GetEvent<SendControllerDetails>().Publish(lastSystem);
                    //brdParent.Background = Brushes.Gray;
                    //brdTabular.Background = Brushes.Cyan;
                    //if system has control group etc
                    //show the control group
                    //else if there are existig vrf/heat exchanger systems
                    //show a new control group
                }
                else
                {                                 
                    HEScrollViewer.Visibility = Visibility.Visible;                                        
                    heatExchangerCanvas.Visibility = Visibility.Visible;
                    ToggleImage.Visibility = Visibility.Collapsed;
                    btnPanning.Visibility = Visibility.Collapsed;
                    btnZoomIn.Visibility = Visibility.Collapsed;
                    btnZoomOut.Visibility = Visibility.Collapsed;
                    btnReAlign.Visibility = Visibility.Collapsed;
                    heatExchangerCanvas.SetHvacSystem(lastSystem);
                    brdToggleimg.Visibility = Visibility.Collapsed;                                                                           
                    //brdParent.Background = Brushes.White;
                    //brdTabular.Background = Brushes.White;
                }

                //_eventAggregator.GetEvent<RefreshSystems>().Publish();
            }
        }

        private void DrawControllerNodesonRefresh(ref WL.AddFlow addFlow, ControlSystem currentsystem)
        {
            if(currentsystem.IsAutoWiringPerformed)
            {
                AutoControlWiring objAutoControlWiring = new AutoControlWiring(true, Project.CurrentProject, currentsystem, addFlow);
            }
            else
            {
                AutoControlWiring objAutoControlWiring = new AutoControlWiring(false, Project.CurrentProject, currentsystem, addFlow);
            }
        }

        //private void addflow_SelectionChanged(object sender, WL.SelectionChangedEventArgs e)
        //{
        //    //var selectedIted = addflow.Items.OfType<JCHNode>().FirstOrDefault(x => x.IsSelected);
        //    //(this.DataContext as ucDesignerCanvasViewModel).CanvasItemSelectedCommand.Execute(selectedIted);         
        //}

        private void ResetLinkPointLocation(WL.Link lk, bool startOrEndPoint)
        {
            bool isStraightLine = false; //是否是直角转弯直线类型（包括VH,HV,VHV,HVH...）
            bool isFirstPointFixed = false;
            bool isMidX = true;
            bool isMidY = true;
            bool connectTopBottom = false; //连接到上下边框
            bool connectLeftRight = false; //连接到左右边框

            if (lk.LineStyle >= WL.LineStyle.VH)
            {
                isStraightLine = true;
                if (startOrEndPoint)
                {
                    if (lk.LineStyle.ToString().StartsWith("V"))
                    {
                        connectTopBottom = true;
                    }
                }
                else
                {
                    if (lk.LineStyle.ToString().EndsWith("V"))
                    {
                        connectTopBottom = true;
                    }
                }

                connectLeftRight = !connectTopBottom;
            }
            else
            {
                connectTopBottom = true;
                connectLeftRight = true;
            }

            int index1, index2;
            WL.Node node;
            Point pt1, pt2;
            if (startOrEndPoint)
            {
                index1 = 0;
                index2 = index1 + 1;
                node = lk.Org;

                Point[] points = lk.Points.ToArray();
                pt1 = points[index1];
                if (node is NextGenModel.MyNodeOut)
                {
                    //起始点是室外机，连线点是固定的左下角
                    pt1 = utilPiping.getLeftBottomPointF(node);
                    isFirstPointFixed = true;
                }
                else if (node is NextGenModel.MyNodeYP)
                {
                    if ((node as NextGenModel.MyNodeYP).IsCP)
                    {
                        //起点为header branch, 不强制连接中点
                        isMidX = false;
                        isMidY = false;
                        connectTopBottom = false;
                    }
                    else
                    {
                        //起始点是branch kit，连线点是中心（header branch除外）
                        pt1 = utilPiping.getCenterPointF(node);
                        isFirstPointFixed = true;
                    }
                }
                else if (node is NextGenModel.MyNodeMultiCH)
                {
                    //起点为Multi CH Box， 不强制连接中点
                    isMidX = false;
                    isMidY = false;
                    connectTopBottom = false;
                }
            }
            else
            {
                index1 = lk.Points.Count - 1;
                index2 = index1 - 1;
                node = lk.Dst;

                pt1 = lk.Points[index1];
                if (node is NextGenModel.MyNodeYP)
                {
                    //目标点是branch kit，则连线点是中心（包括header branch）
                    pt1 = utilPiping.getCenterPointF(node);
                    isFirstPointFixed = true;
                }
            }

            pt2 = lk.Points[index2];

            double x1, x2, y1, y2, rx1, rx2, ry1, ry2, xMid, yMid;
            x1 = pt1.X;
            y1 = pt1.Y;
            x2 = pt2.X;
            y2 = pt2.Y;
            rx1 = node.Location.X;
            ry1 = node.Location.Y;
            rx2 = rx1 + node.Size.Width;
            ry2 = ry1 + node.Size.Height;
            xMid = (rx1 + rx2) / 2f;
            yMid = (ry1 + ry2) / 2f;

            if (!isFirstPointFixed)
            {
                if (connectTopBottom && connectLeftRight && isMidX && isMidY)
                {
                    //如果接线点可以是4个边框上的中点，则自动吸附到最近的接线点
                    double d1 = GetDistance(x1, y1, xMid, ry1); //上中点
                    double d2 = GetDistance(x1, y1, rx2, yMid); //右中点
                    double d3 = GetDistance(x1, y1, xMid, ry2); //下中点
                    double d4 = GetDistance(x1, y1, rx1, yMid); //左中点

                    if (d1 <= d2 && d1 <= d3 && d1 <= d4)
                    {
                        x1 = xMid;
                        y1 = ry1;
                    }
                    else if (d2 <= d1 && d2 <= d3 && d2 <= d4)
                    {
                        x1 = rx2;
                        y1 = yMid;
                    }
                    else if (d3 <= d1 && d3 <= d2 && d3 <= d4)
                    {
                        x1 = xMid;
                        y1 = ry2;
                    }
                    else
                    {
                        x1 = rx1;
                        y1 = yMid;
                    }
                }

                if (!connectLeftRight)
                {
                    //自动吸附顶边框或底边框
                    if (Math.Abs(y2 - ry1) < Math.Abs(y2 - ry2))
                    {
                        y1 = ry1;
                    }
                    else
                    {
                        y1 = ry2;
                    }
                }

                if (!connectTopBottom)
                {
                    //自动吸附左边框或右边框
                    if (Math.Abs(x2 - rx1) < Math.Abs(x2 - rx2))
                    {
                        x1 = rx1;
                    }
                    else
                    {
                        x1 = rx2;
                    }
                }

                if (connectTopBottom && !connectLeftRight)
                {
                    if (isMidX)
                    {
                        //x坐标强制放在边框中点 (大多数情况）
                        x1 = xMid;
                    }
                    else
                    {
                        //x坐标需要在边框范围内 (暂时没有Free x的情况)
                        if (x1 < rx1)
                        {
                            x1 = rx1;
                        }

                        if (x1 > rx2)
                        {
                            x1 = rx2;
                        }
                    }
                }

                if (connectLeftRight && !connectTopBottom)
                {
                    if (isMidY)
                    {
                        //y坐标强制放在边框中点 (大多数情况）
                        y1 = yMid;
                    }
                    else
                    {
                        //y坐标需要在边框范围内 (起始点是 header branch or multi CH-Box)
                        if (y1 < ry1)
                        {
                            y1 = ry1;
                        }

                        if (y1 > ry2)
                        {
                            y1 = ry2;
                        }
                    }
                }
            }

            if (isStraightLine)
            {
                if (connectTopBottom)
                {
                    x2 = x1;
                }
                else
                {
                    y2 = y1;
                }
            }

            lk.Points[index1] = new Point(x1, y1);
            lk.Points[index2] = new Point(x2, y2);
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            double a = Math.Abs(x1 - x2);
            double b = Math.Abs(y1 - y2);
            return Math.Sqrt(a * a + b * b);
        }

        private WL.Link CreateLine(WL.Node src, WL.Node dst, System.Windows.Media.Brush color)
        {
            WL.Link lnk = new WL.Link(src, dst, "", addflow);
            lnk.Stroke = color;
            lnk.StrokeThickness = 3;
            lnk.IsStretchable = false;
            lnk.IsSelectable = false;
            lnk.LineStyle = WL.LineStyle.Polyline;
            lnk.IsAdjustDst = false;
            lnk.IsAdjustOrg = false;
            return lnk;
        }
        /// <summary>
        /// Node Visuals as Pin Style for Controller Node
        /// </summary>
        /// <param name="node"></param>
        private void DrawMandatoryNodeVisualsForController(JCHNode node)
        {
            var connectionHandlePoints = GetMandatoryConnectionHandleLocationForController(node);
            if (connectionHandlePoints.Count < 2)
            {
                return;
            }

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            node.Visual.Children.Clear();          

            #region DrawDefaultNodePin
            if (connectionHandlePoints[0] != null)
            {
                string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                string navigateToFolder = "..\\..\\Image";
                string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                string imageFullPath = sourceDir + "\\" + "CC_plus.png";
                var bitmap = new drawing.Bitmap(imageFullPath);
                var bitmapImage = BitmapToImageSource(bitmap);

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmapImage;

                Rect rect = new Rect(new Point(connectionHandlePoints[0].X - 5, connectionHandlePoints[0].Y - 5), new Point(connectionHandlePoints[0].X + 5, connectionHandlePoints[0].Y + 5));
                dc.DrawRoundedRectangle(brush, null, rect, 5, 5);

            }
            if (connectionHandlePoints[1] != null)
            {
                string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                string navigateToFolder = "..\\..\\Image";
                string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                string imageFullPath = sourceDir + "\\" + "CC_minus.png";
                var bitmap = new drawing.Bitmap(imageFullPath);
                var bitmapImage = BitmapToImageSource(bitmap);

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmapImage;

                Rect rect = new Rect(new Point(connectionHandlePoints[1].X - 5, connectionHandlePoints[1].Y - 5), new Point(connectionHandlePoints[1].X + 5, connectionHandlePoints[1].Y + 5));                
                dc.DrawRoundedRectangle(brush, null, rect, 5, 5);                
               
            }
            #endregion DrawDefaultNodePin
            dc.Close();
            node.Visual.Children.Add(dv);
        }

        private List<Point> GetMandatoryConnectionHandleLocationForController(JCHNode node)
        {
            Point pt11 = new Point(node.Location.X + node.Size.Width / 2, node.Location.Y);
            Point pt12 = new Point(node.Location.X + node.Size.Width / 2, node.Location.Y + node.Size.Height);

            if (node.ImageData != null && node.ImageData.equipmentType != null)
            {
                List<Point> listPoints = new List<Point>();
                if (node.ImageData.equipmentType.Equals("Controller") || node.ImageData.equipmentType.Equals("System"))
                {
                    listPoints.Add(pt11);
                    listPoints.Add(pt12);
                }
                return listPoints;
            }
            else
            {
                List<Point> listPoints = new List<Point>();
                listPoints.Add(pt11);
                listPoints.Add(pt12);
                return listPoints;
            }
        }
        private void SetControllerFillStyle(JCHNode focusedNode = null)
        {
            var nodes = addflow.Items.OfType<JCHVRF.Model.NextGen.WiringNodeCentralControl>();
            foreach (var item in nodes)
            {
                if (item.SystemItem != null)
                {
                    DrawMandatoryNodeVisualsForController(item as JCHNode);
                }
                else if (item.RoomIndoorItem != null)
                {
                    DrawMandatoryNodeVisualsForController(item as JCHNode);
                }
                else if (item.Controller != null)
                {
                    DrawMandatoryNodeVisualsForController(item as JCHNode);
                }
            }            
        }
        private void Load_DeleteIcon(MouseEventArgs e)
        {
            double left = e.GetPosition(addflow).X;
            double top = e.GetPosition(addflow).Y;
            var mouseLocation = new Point(left, top);
            var selectedItem = addflow.GetItemAt(mouseLocation);
            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();
            if (_currentSystem !=null && _currentSystem.HvacSystemType.Equals("6"))
            {
                var AllNodeList = Enumerable.OfType<JCHNode>(addflow.Items).ToArray();
                foreach (JCHNode a in AllNodeList)
                {
                    a.Visual.Children.Clear();
                }

                var SelectNode = selectedItem as JCHNode;
                if (SelectNode != null)
                {
                    DrawMandatoryNodeVisualsForController(SelectNode);
                    var closeIcon = Path.Combine(GetImagePathPiping(), "NodeCloseIcon.png");
                    var expandIcon = Path.Combine(GetImagePathPiping(), "NodeExpandIcon.png");
                    if (File.Exists(closeIcon))
                    {

                        var bitmap = new drawing.Bitmap(closeIcon);
                        var imgSource = BitmapToImageSource(bitmap);
                        dc.DrawImage(imgSource, new Rect(SelectNode.Location, new Size(20, 20)));
                        SetCloseLocation(selectedItem, new Rect(SelectNode.Location, new Size(20, 20)));

                    }
                    if (File.Exists(expandIcon))
                    {
                        var bitmap = new drawing.Bitmap(expandIcon);
                        var imgSource = BitmapToImageSource(bitmap);
                        dc.DrawImage(imgSource,
                            new Rect(new Point(SelectNode.Bounds.TopRight.X - 20, SelectNode.Bounds.TopRight.Y), new Size(20, 20)));
                    }
                    dc.Close();

                    SelectNode.Visual.Children.Add(dv);
                }

            }
        }

        ~ucDesignerCanvas()
        {
            if (_eventAggregator != null)
            {
                _eventAggregator.GetEvent<DisplayPipingLength>().Unsubscribe(DisplayLenPiping);
                _eventAggregator.GetEvent<ErrorMessageUC>().Unsubscribe(ShowErrorMessage);
            }
        }

        private void ClearValidationText(WL.AddFlow addflow)
        {


            //var NodeList = addflow.Items.OfType<WL.Node>().ToList();
            //foreach (var item in NodeList)
            //{
            //    if (item.Captions.Count > 0)
            //        item.Captions.Clear();
            //}

            var CaptionList = addflow.Items.OfType<WL.Caption>().ToList();

            foreach (var item in CaptionList)
            {
                addflow.RemoveCaption(item);
            }
        }

        #endregion

        #region Controller-Auto-Wiring

        public void DrawControllerInSetNode(CentralController controller)
        {
            NextGenModel.ImageData data = new NextGenModel.ImageData();
            //var SenderData = (NextGenModel.ImageData) e.Data.GetData(typeof(NextGenModel.ImageData));
            //var AllNodeList = Enumerable.OfType<WL.Node>(addflow.Items).ToArray();

            data.imagePath = GetImagesSourceDir() + "\\" + controller.Image;
            data.imageName = controller.Model;
            data.equipmentType = "Controller";
            data.Source = controller;

            ImageSource img = null;
            JCHNode imageNode = null;
            if (data.imagePath != null)
            {
                System.Drawing.Image img1 = System.Drawing.Image.FromFile(data.imagePath);
                drawing.Bitmap bitmap = new drawing.Bitmap(img1, new drawing.Size(60, 40));
                var bitmapImage = BitmapToImageSource(bitmap);
                img = bitmapImage;
            }
            imageNode = new JCHNode(leftC + 100, topC + 100, 125, 100, data.NodeNo.ToString(), addflow);

            if (data.imagePath != null)
            {
                imageNode.Image = img;
            }

            setNodeStyleForControllers(imageNode, data);
            imageNode.ImageData = data;
            
            var viewModel = (ucDesignerCanvasViewModel) DataContext;           
            viewModel.CurrentSystem = _currentSystem;
            //bool addOnCanvas = viewModel.AssignGroupToEquipment(SenderData, projectLegacy);
            //if (addOnCanvas)
            //{
                addflow.AddNode(imageNode);
            //    //To remove the system from system list           
            //    //Todo: UtilTrace.SaveHistoryTraces();
            //
                       
            DrawMandatoryNodeVisualsForController(imageNode);            
        }

        private void ScrollViewer_KeyDown(object sender, KeyEventArgs e)
        {
            var ucDesignerCanvasViewModel = (ucDesignerCanvasViewModel)this.DataContext;
            ucDesignerCanvasViewModel.CurrentSystem = _currentSystem;
            if (_currentSystem != null)
            {                
                if (_currentSystem.HvacSystemType == "6")
                {
                    if (e.Key.Equals(Key.Delete))
                    {
                        if (currentNode != null)
                        {
                            if (currentNode is JCHNode)
                            {
                                DeleteNode = currentNode as JCHNode;

                                DeleteNodeOnCanvas(ucDesignerCanvasViewModel.CurrentSystem);
                            }
                            currentNode = null;
                        }
                    }
                }
            }
        }

        private string GetImagesSourceDir()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\ControllerImage";

            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        /// <summary>
        /// Deleting the In-set Controller
        /// </summary>
        /// <param name="node"></param>
        private void DeleteControllersInSet(object node)
        {
            var findcontrolsystem = Project.CurrentProject.ControlSystemList.Find(t => t.Id == _currentSystem.Id);
            var findgroup = Project.CurrentProject.ControlGroupList.Find(grp => grp.ControlSystemID == findcontrolsystem.Id);

            CentralController controller = null;
            NextGenModel.WiringNodeCentralControl node1 = null;
            JCHNode node2 = null;

            if (node is NextGenModel.WiringNodeCentralControl)
            {
                node1 = (NextGenModel.WiringNodeCentralControl)node;
                if (node1.SystemItem != null || node1.RoomIndoorItem != null)
                {
                    return;
                }
                controller = node1.Controller.CentralController;
            }
            else
            {
                node2 = (JCHNode)node;                
                if(node2.ImageData.Source is NextGenModel.SystemVRF || node2.ImageData.Source is RoomIndoor)
                {
                    return;
                }
                controller = (CentralController)node2.ImageData.Source;
            }
            var centralControllerControllersInSet = controller.ControllersInSet;

            if (centralControllerControllersInSet == null) return;
            List<NextGenModel.WiringNodeCentralControl> wiringNodeCentralControls = addflow.Items.OfType<NextGenModel.WiringNodeCentralControl>().ToList();
            List<JCHNode> jchNodes = addflow.Items.OfType<JCHNode>().ToList();

            foreach (var sysController in centralControllerControllersInSet)
            {
                var ct = Project.CurrentProject.ControllerList.Find(c => c.ControlGroupID == findgroup.Id && c.Model == sysController.Model);
                if (ct != null)
                {
                    if (ct.Quantity <= 1)
                    {
                        Project.CurrentProject.ControllerList.Remove(ct);
                    }
                    else
                    {
                        ct.Quantity -= 1;
                    }
                }

                if (node1 != null)
                {
                    var inset = wiringNodeCentralControls.Find(item => item.Controller.CentralController.Equals(sysController));
                    addflow.Items.Remove(inset);
                    addflow.RemoveNode(inset);
                }
                if (node2 != null)
                {

                    var inset = jchNodes.Find(n => n.GetType() != typeof(NextGenModel.WiringNodeCentralControl) && n.ImageData.imageName == sysController.Model);
                    addflow.Items.Remove(inset);
                    addflow.RemoveNode(inset);
                }

            }

            SelectedSystems = findcontrolsystem.SystemsOnCanvasList;
            if (SelectedSystems != null)
            {
                var context = (ucDesignerCanvasViewModel)DataContext;
                context.CheckForErrors(SelectedSystems, Project.CurrentProject.ControllerList.FindAll(c => c.ControlGroupID == findgroup.Id).Count);
            }
        }
      
        #endregion
    }
}

