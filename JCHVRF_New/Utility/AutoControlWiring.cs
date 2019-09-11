using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF.MyPipingBLL.NextGen;
using JCHVRF_New.Model;
using Lassalle.WPF.Flow;
using Brushes = System.Drawing.Brushes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using SystemVRF = JCHVRF.Model.SystemVRF;
using WiringNodeCentralControl = JCHVRF.Model.NextGen.WiringNodeCentralControl;
using JCHVRF_New.Views;

namespace JCHVRF_New.Utility
{
    class AutoControlWiring
    {
        private AddFlow _addflowWiring;
        JCHVRF.Model.Project thisProject;
        UtilPiping utilPiping = new UtilPiping();

        const int Y_CC_Level1 = 50;
        const int Y_CC_Level2 = 200;
        int Y_CC_Level3 = (Project.CurrentProject.RegionCode=="EU_W" || Project.CurrentProject.RegionCode == "EU_S" || Project.CurrentProject.RegionCode == "EU_E") ? 350 : Y_CC_Level1;
        int Y_SYSTEM_Level = (Project.CurrentProject.RegionCode == "EU_W" || Project.CurrentProject.RegionCode == "EU_S" || Project.CurrentProject.RegionCode == "EU_E") ? 500 : Y_CC_Level2;
        const int X_RowHeader = 50;
        private const int X_OffsetStep = 200;
        ControlSystem controlSys;
        bool atleast_1system;       

        public AutoControlWiring(bool isAutoWiringComplete, Project project, ControlSystem controlSystem, AddFlow addflowWiring)
        {
            //_addflowWiring = new AddFlow();
            thisProject = project;
            _addflowWiring = addflowWiring;
            DoAutoWiring(isAutoWiringComplete, thisProject, controlSystem);
        }

        private void DoAutoWiring(bool isWiringNeeded, Project project, ControlSystem controlSystem)
        {
            controlSys = controlSystem;
            InitWiringNodes(_addflowWiring);
            ControlGroup controlGroup = project.ControlGroupList.Find(c => c.ControlSystemID.Equals(controlSystem.Id));
            AutoWiringNodeGroup nodeGroup = null;
            if (controlGroup != null)
            {
                nodeGroup = CreateWiringNodeStructure(controlGroup);
                
                _addflowWiring.AddNode(nodeGroup);
                float x = X_RowHeader;

                foreach (WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel1)
                {
                    AutoDrawController(nodeController, x);
                    x += X_OffsetStep;
                }
                x = X_RowHeader;
                foreach (WiringNodeCentralControl nodeController in nodeGroup.ControllerListLevel2)
                {
                    AutoDrawController(nodeController, x);
                    x += X_OffsetStep;
                }
                x = X_RowHeader;
                foreach (WiringNodeCentralControl nodeController in nodeGroup.ControllerListOthers)
                {
                    AutoDrawController(nodeController, x);
                    x += X_OffsetStep;
                }
                x = X_RowHeader;
                foreach (WiringNodeCentralControl nodeSystem in nodeGroup.SystemList)
                {
                    AutoDrawController(nodeSystem, x);
                    x += X_OffsetStep;
                }
                if (isWiringNeeded)
                {
                    DrawLinks(nodeGroup);
                    controlSystem.IsAutoWiringPerformed = true;
                }
                else
                {
                    controlSystem.IsAutoWiringPerformed = false;
                }
            }
        }

        private void DrawLinks(AutoWiringNodeGroup nodeGroup)
        {
            List<JCHNode> seriesList = new List<JCHNode>();
            JCHNode nodeStart, nodeEnd;
            if(thisProject.RegionCode=="EU_W" || thisProject.RegionCode == "EU_S" || thisProject.RegionCode == "EU_E")
            {
                if (nodeGroup.ControllerListLevel1.Count > 0)
                {
                    seriesList.Add(nodeGroup.ControllerListLevel1[0]);
                }

                if(nodeGroup.ControllerListLevel2.Count > 0)
                { 
                    seriesList.Add(nodeGroup.ControllerListLevel2[0]);
                }               
            }
            if (nodeGroup.ControllerListOthers.Count > 0)
            {
                seriesList.Add(nodeGroup.ControllerListOthers[0]);
            }
            if (nodeGroup.SystemList.Count > 0)
            {
                seriesList.Add(nodeGroup.SystemList[0]);
            }                

            for(int i = 0; i < seriesList.Count - 1; i++)
            {                            
                Link myLink = SeriesLink(seriesList[i], seriesList[i+1]);
                myLink.IsStretchable = true;
                myLink.IsSelectable = false;
                //myLink.LineStyle = LineStyle.Polyline;                
                _addflowWiring.AddLink(myLink);
            }            

            for (int i = 0; i < nodeGroup.ControllerListLevel1.Count - 1; i++)
            {
                nodeStart = nodeGroup.ControllerListLevel1[i];
                nodeEnd = nodeGroup.ControllerListLevel1[i + 1];

                Link customLink = ParallelLink(nodeStart, nodeEnd);
                _addflowWiring.AddLink(customLink);

                CustomizeLink(nodeStart, nodeEnd, customLink);
            }

            for (int i = 0; i < nodeGroup.ControllerListLevel2.Count - 1; i++)
            {
                nodeStart = nodeGroup.ControllerListLevel2[i];
                nodeEnd = nodeGroup.ControllerListLevel2[i + 1];

                Link customLink = ParallelLink(nodeStart, nodeEnd);
                _addflowWiring.AddLink(customLink);

                CustomizeLink(nodeStart, nodeEnd, customLink);
            }

            for (int i = 0; i < nodeGroup.ControllerListOthers.Count - 1; i++)
            {
                nodeStart = nodeGroup.ControllerListOthers[i];
                nodeEnd = nodeGroup.ControllerListOthers[i + 1];

                Link customLink = ParallelLink(nodeStart, nodeEnd);
                _addflowWiring.AddLink(customLink);

                CustomizeLink(nodeStart, nodeEnd, customLink);
            }

            for (int i = 0; i < nodeGroup.SystemList.Count - 1; i++)
            {
                nodeStart = nodeGroup.SystemList[i];
                nodeEnd = nodeGroup.SystemList[i + 1];

                Link customLink = ParallelLink(nodeStart, nodeEnd);
                _addflowWiring.AddLink(customLink);

                CustomizeLink(nodeStart, nodeEnd, customLink);
            }
        }

        private Link ParallelLink(JCHNode nodeStart, JCHNode nodeEnd)
        {
            return new Link(nodeStart, nodeEnd, "", _addflowWiring);
        }

        private Link SeriesLink(JCHNode nodeStart, JCHNode nodeEnd)
        {
            return new Link(nodeStart, nodeEnd, 1, 0, "", _addflowWiring);
        }

        private void CustomizeLink(JCHNode nodeStart, JCHNode nodeEnd, Link customLink)
        {
            customLink.IsStretchable = true;
            customLink.IsSelectable = false;
            customLink.IsAdjustDst = false;
            customLink.IsAdjustOrg = false;

            Point start = utilPiping.getTopCenterPointF(nodeStart);
            Point end = utilPiping.getTopCenterPointF(nodeEnd);
            Point midPt1 = new Point(start.X + 15, start.Y - 20);
            Point midPt2 = new Point(end.X, end.Y - 20);

            customLink.Points[0] = start;            
            customLink.Points.Add(midPt1);
            customLink.Points.Add(midPt2);
            customLink.Points[1] = midPt1;
            customLink.Points[2] = midPt2;
            customLink.Points[3] = end;
        }

        private void AutoDrawController(WiringNodeCentralControl nodeController, float x)
        {
            float y = Y_SYSTEM_Level;
            if (nodeController.Level == 1)
            {
                y = Y_CC_Level1;
            }
            else if (nodeController.Level == 2)
            {
                y = Y_CC_Level2;
            }
            else if (nodeController.Level == 3)
            {
                y = Y_CC_Level3;
            }
            else
            {
                y = Y_SYSTEM_Level;
                if (nodeController.SystemItem != null)
                {
                    int IDUcount;
                    var system = nodeController.SystemItem;
                    IDUcount = Project.CurrentProject.RoomIndoorList.FindAll(s => s.SystemID.Equals(system.Id)).Count;
                    nodeController.Location = new Point(x, y);
                    nodeController.Text = system.Name + "\n" + "IDU count "+ IDUcount + "\n" + "ODU count 1";                   
                    SetWiringNodeStyleForSystems(nodeController);
                    _addflowWiring.AddNode(nodeController);
                    var nodes1 = _addflowWiring.Items.OfType<JCHNode>();
                    foreach (var jchNode in nodes1)
                    {
                        DrawMandatoryNodeVisualsForController(jchNode);
                    }
                }
                else
                {
                    var system = nodeController.RoomIndoorItem;
                    
                    string model = "";
                    if(Project.CurrentProject.BrandCode == "H")
                    {
                        model = system.IndoorItem.Model_Hitachi;
                    }
                    if (Project.CurrentProject.BrandCode == "Y")
                    {
                        model = system.IndoorItem.Model_York;
                    }
                    nodeController.Location = new Point(x, y);
                    nodeController.Text = model + "\n" + system.Power;
                    var imagePath1 = GetImagesSourceDirVRF() + "\\" + "u1483.png";
                    //if(!System.IO.File.Exists(imagePath1))
                    //{
                    //    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                    //    string navigateToFolder = "..\\..\\Image\\TypeImageProjectCreation";
                    //    string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
                    //    imagePath1 = sourceDir + "\\" + system.DisplayImagePath;
                    //}

                    System.Drawing.Image img1 = System.Drawing.Image.FromFile(imagePath1);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(img1, new System.Drawing.Size(60, 40));
                    var bitmapImage = BitmapToImageSource(bitmap);
                    nodeController.Image = bitmapImage;

                    SetWiringNodeStyleForSystems(nodeController);
                    _addflowWiring.AddNode(nodeController);
                    var nodes2 = _addflowWiring.Items.OfType<JCHNode>();
                    foreach (var jchNode in nodes2)
                    {
                        DrawMandatoryNodeVisualsForController(jchNode);
                    }
                }                
                return;
            }

            Controller ctrl = nodeController.Controller;
            nodeController.Location = new Point(x,y);
            string type = ctrl.Type.ToString();
            if(type.Equals("CentralController", StringComparison.OrdinalIgnoreCase) || type.Equals("ONOFF", StringComparison.OrdinalIgnoreCase) || type.Equals("Software", StringComparison.OrdinalIgnoreCase))
            {
                type = "No BMS";
            }
            if (type.Equals("BACNetInterface", StringComparison.OrdinalIgnoreCase))               
            {
                type = "BACnet";
            }
            if (type.Equals("ModBusInterface", StringComparison.OrdinalIgnoreCase))                
            {
                type = "ModBus";
            }
            if (type.Equals("KNXInterface", StringComparison.OrdinalIgnoreCase))                
            {
                type = "KNX";
            }
            if (type.Equals("LonWorksInterface", StringComparison.OrdinalIgnoreCase))                
            {
                type = "LonWorks";
            }

            nodeController.Text = ctrl.Model + "\n" + type;
            var imagePath = GetImagesSourceDir() + "\\" + ctrl.Image;

            System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);
            System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(img, new System.Drawing.Size(60, 50));
            var bitmapImage1 = BitmapToImageSource(bitmap1);

            nodeController.Image = bitmapImage1;
            SetWiringNodeStyle(nodeController);
            _addflowWiring.AddNode(nodeController);
            var nodes3 = _addflowWiring.Items.OfType<JCHNode>();
            foreach (var jchNode in nodes3)
            {
                DrawMandatoryNodeVisualsForController(jchNode);
            }

        }

        private void SetWiringNodeStyle(JCHNode node)
        {
            node.Geometry = new RectangleGeometry(new Rect(0, 0, 100, 70), 3, 3);
            // node.Stroke = System.Windows.Media.Brushes.RoyalBlue;
            node.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(185, 185, 185));// System.Windows.Media.Brushes.Gray;
            node.StrokeThickness = 2;
            node.Size = new Size(125, 100);
            node.DashStyle = DashStyles.Solid;
            node.Fill = System.Windows.Media.Brushes.White;
            node.ImageMargin = new Thickness(3);
            node.TextMargin = new Thickness(3);
            node.ImagePosition = ImagePosition.CenterTop;
            node.TextPosition = TextPosition.CenterBottom;
            node.IsXSizeable = false; //test resize
            node.IsYSizeable = false; //test resize                
            var pointOnNode = new PointCollection();
            pointOnNode.Add(new Point(50, 0));
            pointOnNode.Add(new Point(50, 100));
            node.PinsLayout = pointOnNode;

            //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            //string navigateToFolder = "..\\..\\Image";
            //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            //string imageFullPath = sourceDir + "\\" + "CC_plus.png";

            //ImageBrush brush = new ImageBrush();
            //var bitmap = new Bitmap(imageFullPath);
            //var bitmapImage = BitmapToImageSource(bitmap);
            //brush.ImageSource = bitmapImage;

            //_addflowWiring.PinFill = brush;
            _addflowWiring.PinShape = PinShape.Circle;
            _addflowWiring.PinSize = 13;
            node.OutConnectionMode = ConnectionMode.Pin;
            node.InConnectionMode = ConnectionMode.Pin;
            _addflowWiring.CanDrawNode = false;
            node.FontSize = 9;
            //node.Tooltip = data.equipmentType + " : " + data.imageName;
            node.IsSelectable = true;
            node.IsEditable = false;
        }

        private void SetWiringNodeStyleForSystems(JCHNode node)
        {
            node.Geometry = new RectangleGeometry(new Rect(0, 0, 100, 70), 3, 3);
            // node.Stroke = System.Windows.Media.Brushes.RoyalBlue;
            node.Stroke =
                new SolidColorBrush(System.Windows.Media.Color.FromRgb(185, 185, 185)); // System.Windows.Media.Brushes.Gray;
            node.StrokeThickness = 2;
            node.Size = new Size(125, 75);
            node.DashStyle = DashStyles.Solid;
            node.Fill = System.Windows.Media.Brushes.White;            
            node.TextMargin = new Thickness(3);
            if (node.Image == null)
            {
                node.TextPosition = TextPosition.CenterMiddle;
            }
            else
            {
                node.ImageMargin = new Thickness(3);
                node.ImagePosition = ImagePosition.CenterTop;
                node.TextPosition = TextPosition.CenterBottom;
            }
            node.IsXSizeable = false; //test resize
            node.IsYSizeable = false; //test resize                
            var pointOnNode = new PointCollection();
            pointOnNode.Add(new Point(50, 0));
            pointOnNode.Add(new Point(50, 100));
            node.PinsLayout = pointOnNode;

            //string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            //string navigateToFolder = "..\\..\\Image";
            //string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            //string imageFullPath = sourceDir + "\\" + "Path 111.png";

            //ImageBrush brush = new ImageBrush();
            //var bitmap = new Bitmap(imageFullPath);
            //var bitmapImage = BitmapToImageSource(bitmap);
            //brush.ImageSource = bitmapImage;

            //_addflowWiring.PinFill = brush;
            _addflowWiring.PinShape = PinShape.Circle;
            _addflowWiring.PinSize = 13;
            node.OutConnectionMode = ConnectionMode.Pin;
            node.InConnectionMode = ConnectionMode.Pin;
            _addflowWiring.CanDrawNode = false;
            node.FontSize = 9;
            //node.Tooltip = data.equipmentType + " : " + data.imageName;
            node.IsSelectable = true;
            node.IsEditable = false;
        }

        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
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
        private string GetImagesSourceDir()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image\\ControllerImage";

            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        private string GetImagesSourceDirVRF()
        {
            string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
            string navigateToFolder = "..\\..\\Image";

            string sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolder);
            return sourceDir;
        }

        //To draw initial layout
        private AutoWiringNodeGroup CreateWiringNodeStructure(ControlGroup controlGroup)
        {
            AutoWiringNodeGroup wiringGroup = new AutoWiringNodeGroup();
            foreach (Controller controller in Project.CurrentProject.ControllerList)
            {
                if (controller.ControlGroupID == controlGroup.Id)
                {
                    int level = 1;
                    if (controller.Model.Trim() == "CSNET MANAGER 2 T10" ||
                        controller.Model.Trim() == "CSNET MANAGER 2 T15")
                    {
                        level = 1;
                    }
                    else if (controller.Model.Trim() == "HC-A64NET" || controller.Model.Trim() == "PSC-A160WEB1")
                    {
                        level = 2;
                    }
                    else
                    {
                        level = 3;
                    }

                    for (int n = 0; n < controller.Quantity; n++)
                    {
                        JCHVRF.Model.NextGen.WiringNodeCentralControl wiringController =
                            new JCHVRF.Model.NextGen.WiringNodeCentralControl();
                        wiringController.Controller = controller;
                        wiringController.Level = level;
                        if (level == 1)
                        {
                            wiringGroup.ControllerListLevel1.Add(wiringController);
                        }
                        else if (level == 2)
                        {
                            wiringGroup.ControllerListLevel2.Add(wiringController);
                        }
                        else
                        {
                            wiringGroup.ControllerListOthers.Add(wiringController);
                        }
                    }
                }
            }
            
            foreach (JCHVRF.Model.NextGen.SystemVRF systemVRF in Project.CurrentProject.SystemListNextGen)
            {                
                if (systemVRF.ControlGroupID != null)
                {
                    if (systemVRF.ControlGroupID.Contains(controlGroup.Id))
                    {
                        atleast_1system = true;
                        if (systemVRF.SystemStatus == SystemStatus.VALID)
                        {
                            JCHVRF.Model.NextGen.WiringNodeCentralControl wiringVrf = new JCHVRF.Model.NextGen.WiringNodeCentralControl();
                            wiringVrf.SystemItem = systemVRF;
                            wiringGroup.SystemList.Add(wiringVrf);
                            if (controlSys.SystemStatus != SystemStatus.VALID)
                            {
                                controlSys.SystemStatus = SystemStatus.WIP;
                            }
                        }
                        else
                        {
                            systemVRF.ControlGroupID.Remove(controlGroup.Id);
                            controlSys.SystemsOnCanvasList.RemoveAll(item => item.System.Equals(systemVRF));                            
                        }
                    }                               
                }                                                     
            }                                                      

            foreach (RoomIndoor exchanger in Project.CurrentProject.ExchangerList)
            {
                if (exchanger.ControlGroupID != null)
                {
                    if (exchanger.ControlGroupID.Contains(controlGroup.Id))
                    {
                        if (thisProject.HeatExchangerSystems.Find(he => he.Id == exchanger.SystemID).SystemStatus == SystemStatus.VALID)
                        {
                            JCHVRF.Model.NextGen.WiringNodeCentralControl wiringExchanger = new JCHVRF.Model.NextGen.WiringNodeCentralControl();
                            wiringExchanger.RoomIndoorItem = exchanger;
                            wiringGroup.SystemList.Add(wiringExchanger);
                            if (controlSys.SystemStatus != SystemStatus.VALID)
                            {
                                controlSys.SystemStatus = SystemStatus.WIP;
                            }
                        }
                        else
                        {
                            exchanger.ControlGroupID.Remove(controlGroup.Id);
                            controlSys.SystemsOnCanvasList.RemoveAll(item => item.System.Equals(exchanger));                            
                        }
                    }
                }
            }
            if (wiringGroup.SystemList.Count <= 0)
            {
                controlSys.IsValid = false;
                ErrorLog.LogError(Model.ErrorType.Error, Model.ErrorCategory.WiringError, "No System Selected");                
            }            
            return wiringGroup;
        }

        private void InitWiringNodes(AddFlow addflowWiring)
        {
           addflowWiring.Clear();
           addflowWiring.LinkModel.LineStyle = LineStyle.Polyline;
           addflowWiring.CanDrawLink = false;
           addflowWiring.ScrollIncrement = new Size(0,0);
        }

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
                var bitmap = new Bitmap(imageFullPath);
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
                var bitmap = new Bitmap(imageFullPath);
                var bitmapImage = BitmapToImageSource(bitmap);

                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmapImage;

                Rect rect = new Rect(new Point(connectionHandlePoints[1].X - 5, connectionHandlePoints[1].Y - 5), new Point(connectionHandlePoints[1].X + 5, connectionHandlePoints[1].Y + 5));
                dc.DrawRoundedRectangle(brush, null, rect, 5, 5);

            }
            #endregion
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

        private System.Windows.Media.Brush GetBrushFromCode(string code)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(code));
        }
    }
}
