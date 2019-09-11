using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using JCHVRF_New.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using Prism.Events;
using Lassalle.WPF.Flow;
using System.Windows.Media.Imaging;
using JCHVRF_New.Utility;

namespace JCHVRF_New.ViewModels
{
    public class CHBoxPropertiesViewModel : ViewModelBase
    {

        public bool IsSingle
        {
            get { return (SelectedNode is JCHVRF.Model.NextGen.MyNodeCH); }
            set {
                if (SelectedNode == null) return;
                if (value && (SelectedNode is JCHVRF.Model.NextGen.MyNodeMultiCH))
                {
                    SelectedNode = new JCHVRF.Model.NextGen.MyNodeCH(SelectedNode);
                }
                else if(!value && (SelectedNode is JCHVRF.Model.NextGen.MyNodeCH))
                {
                    SelectedNode = new JCHVRF.Model.NextGen.MyNodeMultiCH(SelectedNode);
                }
                RaisePropertyChanged();
                UtilTrace.SaveHistoryTraces();
            }
        }


        private int _selectedPositionIndex;

        public int SelectedPositionIndex
        {
            get { return _selectedPositionIndex; }
            set {
                this.SetValue(ref _selectedPositionIndex, value);
                UpdateHeightDiff();
                UtilTrace.SaveHistoryTraces();
            }
        }

        private void UpdateHeightDiff()
        {
            if (_selectedPositionIndex == 1)
                HeightDiff = 0;
            else
                HeightDiff = HeightDiff;
        }

        public double HeightDiff
        {
            get { return Math.Abs(SelectedNode==null ? 0 : SelectedNode.HeightDiff); }
            set {
                if(SelectedNode!=null)
                SelectedNode.HeightDiff = SelectedPositionIndex == 2 ? -value : value;
                RaisePropertyChanged("HeightDiff");
                UtilTrace.SaveHistoryTraces();
            }
        }


        private int _selectedNodeIndex;
        AddFlow _addFlow;

        public MyNodeCHBase SelectedNode
        {
            get {
                if (this._addFlow?.Items[_selectedNodeIndex] is MyNodeCHBase)
                    return this._addFlow?.Items[_selectedNodeIndex] as MyNodeCHBase;
                else
                    return null;
            }
            set {
                if (this._addFlow != null)
                {
                    List<Link> links = new List<Link>();

                        
                        (this._addFlow?.Items[_selectedNodeIndex] as Node).Links.ForEach(a=> {
                            links.Add(a);
                        });
                    this._addFlow.RemoveNode(this._addFlow?.Items[_selectedNodeIndex] as Node);
                    string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                    string navigateToFolderWithNewImage = "..\\..\\Image\\TypeImages\\";
                    var sourceDir = System.IO.Path.Combine(defaultFolder, navigateToFolderWithNewImage);
                    value.Image = new BitmapImage(new Uri(sourceDir + ((value is JCHVRF.Model.NextGen.MyNodeCH) ? "CHbox.png" :"MultiCHbox.png")));
                    this._addFlow.AddNode(value);
                    foreach (Link link in links)
                        this._addFlow.AddLink(link);
                    _selectedNodeIndex = this._addFlow.Items.IndexOf(value);
                    value.IsSelected = true;
                }
            }
        }

        public string LengthUnit { get { return SystemSetting.UserSetting.unitsSetting.settingLENGTH; } }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            _addFlow = navigationContext.Parameters["AddFlow"] as AddFlow;
            _selectedNodeIndex = (int)navigationContext.Parameters["Index"];
            RaisePropertyChanged("SelectedNode");
            RaisePropertyChanged("IsSingle");
            SelectedPositionIndex = (SelectedNode == null || SelectedNode.HeightDiff == 0) ? 1 : SelectedNode.HeightDiff > 0 ? 0 : 2;
        }
    }
}