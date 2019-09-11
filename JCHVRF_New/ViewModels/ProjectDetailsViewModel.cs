/****************************** File Header ******************************\
File Name:	ProjectDetailsViewModel.cs
Date Created:	2/20/2019
Description:	
\*************************************************************************/

using Lassalle.Flow;

namespace JCHVRF_New.ViewModels
{

    using JCHVRF.BLL.New;
    using JCHVRF.Model;
    using JCHVRF_New.Common.Contracts;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using Prism.Commands;
    using Prism.Events;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using Langauge = JCHVRF_New.LanguageData.LanguageViewModel;

    public class ProjectDetailsViewModel : ViewModelBase
    {
        #region Fields
        readonly IEventAggregator _eventAggregator;
        public DelegateCommand<string> LoadedUnloadedCommand { get; set; }
        public DelegateCommand<object> ReorderUpClickCommand { get; set; }
        public DelegateCommand<object> ReorderDownClickCommand { get; set; }
        #endregion

        #region Constructors

        bool firstInstance = true;
        bool fromLoad = true;
        internal bool? isEditable;
        internal bool? isSideBarVisible;
        private bool flgEventSubs;
        public ProjectDetailsViewModel(IEventAggregator eventAggregator, IProjectInfoBAL projectInfoBll)
        {
            Title = Langauge.Current.GetMessage("PROJECT_DETAILS");
            this._eventAggregator = eventAggregator;

            SelectedTreeViewItemCommand = new DelegateCommand<object>(SelectedTreeViewItemClick);
            ReorderUpClickCommand = new DelegateCommand<object>(OnReorderUpClick);
            ReorderDownClickCommand = new DelegateCommand<object>(OnReorderDownClick);
            //DisplayProjectDetails(projectInfoBll);
            firstInstance = true;
            fromLoad = true;
            LoadedUnloadedCommand = new DelegateCommand<string>(OnLoadedChanged);
            //RefreshSystemsList();
            LostFocusCommand = new DelegateCommand(OnSearchLostFocus);
            GotFocusCommand = new DelegateCommand(OnSearchGotFocus);
            TextChangedCommand = new DelegateCommand(OnSearchTextChanged);
            //SearchText = "Search";
            if (!flgEventSubs)
            {
                _eventAggregator.GetEvent<SystemSelectedTabSubscriber>().Subscribe(OnSelectedSystemTab);
                _eventAggregator.GetEvent<RefreshSystems>().Subscribe(RefreshSystemsList);
                flgEventSubs = true;
            }
        }

        private void OnReorderUpClick(object leftSideBarChild)
        {
            Reorder(leftSideBarChild, -1);
        }

        private void OnReorderDownClick(object leftSideBarChild)
        {
            Reorder(leftSideBarChild, 1);
        }        

        bool _isLoadedOnce= false;

        private void OnLoadedChanged(string state)
        {
            if (state=="Loaded")
            {
                fromLoad = false;
                if(!_isLoadedOnce)
                { 

                Project.GetProjectInstance.SelectedSystemID = string.Empty;
                RefreshSystemsList();

                LeftSideBarItem sideBarItem = AllSystems.FirstOrDefault(a => a.Children.Count > 0);
                LeftSideBarChild first = sideBarItem != null ? sideBarItem.Children[0] : null;

                if (first != null)
                {
                    if (firstInstance)
                    {
                        first.IsSelected = true;
                        first.IsEditable = false;
                        _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)first.Source);
                        firstInstance = false;
                    }
                    else
                    {
                        SystemBase selectedSystem = WorkFlowContext.CurrentSystem;
                        foreach (LeftSideBarItem item in AllSystems)
                        {
                            foreach (LeftSideBarChild child in item.Children)
                            {
                                if (child.Source.Equals(selectedSystem))
                                {
                                    child.IsSelected = true;
                                    _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish(selectedSystem);
                                }
                            }
                        }                    
                    }
                }
                }
                _isLoadedOnce = true;
             
                
            }
            else
            {
             
            }
            fromLoad = true;
        }


        private bool ContainsSearchText(SystemBase system)
        {
            return string.IsNullOrEmpty(SearchText) || system.Name.ToLower().Contains(SearchText.ToLower());
        }
        ~ProjectDetailsViewModel()
        {
            if(_eventAggregator!=null)
            {
                _eventAggregator.GetEvent<SystemSelectedTabSubscriber>().Unsubscribe(OnSelectedSystemTab);
                _eventAggregator.GetEvent<RefreshSystems>().Unsubscribe(RefreshSystemsList);
            }
         
        }
       public void RefreshSystemsList()
        {
            AllSystems = null;

            if (Project.GetProjectInstance?.SystemListNextGen != null)
            { 
                foreach (SystemBase system in Project.GetProjectInstance.SystemListNextGen)
                {

                    if (ContainsSearchText(system))
                        AllSystems[0].Children.Add(new LeftSideBarChild(system.Name, AllSystems[0].Header, system.StatusIcon, system));

                }
            }
            if (Project.GetProjectInstance?.HeatExchangerSystems != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.HeatExchangerSystems)
                {
                    if (ContainsSearchText(system))
                        AllSystems[1].Children.Add(new LeftSideBarChild(system.Name, AllSystems[0].Header, system.StatusIcon, system));

                }
            }

            if (Project.GetProjectInstance?.ControlSystemList != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.ControlSystemList)
                {
                    if (ContainsSearchText(system))
                        AllSystems[2].Children.Add(new LeftSideBarChild(system.Name, AllSystems[0].Header, system.StatusIcon, system));

                }
            }
            //RaisePropertyChanged("AllSystems");          
            if (fromLoad == true)
            {
                _lastSelectedId = Project.CurrentProject.SelectedSystemID;
                if (!string.IsNullOrEmpty(_lastSelectedId))
                {                    
                    LeftSideBarChild toselect = AllSystems?.FirstOrDefault(a => a.Children.Count > 0 && a.Children.FirstOrDefault(b => ((SystemBase)b.Source)?.Id == _lastSelectedId) != null)?.Children.FirstOrDefault(b => ((SystemBase)b.Source)?.Id == _lastSelectedId);
                    if (toselect != null)
                    {
                        toselect.IsSelected = true;
                        if (isEditable.HasValue)
                        {
                            toselect.IsEditable = isEditable.Value;
                            //toselect.IsEditable = false;
                        }
                        if (isSideBarVisible.HasValue)
                        {
                            toselect.IsSideBarVisible = isSideBarVisible.Value;
                        }
                        _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)toselect.Source);
                    }
                }
                else
                {
                    LeftSideBarItem sideBarItem = AllSystems.FirstOrDefault(a => a.Children.Count > 0);
                    LeftSideBarChild first = sideBarItem != null ? sideBarItem.Children[0] : null;
                    if (first != null)
                    {

                        first.IsSelected = true;
                        if (isEditable.HasValue)
                        {
                            first.IsEditable = isEditable.Value;
                        }
                        if (isSideBarVisible.HasValue)
                        {
                            first.IsSideBarVisible = isSideBarVisible.Value;
                        }
                        _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)first.Source);
                    }
                }
            }
        }

        void filterSystemList()
        {
            AllSystems = null;

            if (Project.GetProjectInstance?.SystemListNextGen != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.SystemListNextGen)
                {

                    if (ContainsSearchText(system))
                    {
                        AllSystems[0].Children.Add(new LeftSideBarChild(system.Name, AllSystems[0].Header, system.StatusIcon, system));
                    }


                }
            }
            if (Project.GetProjectInstance?.HeatExchangerSystems != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.HeatExchangerSystems)
                {
                    if (ContainsSearchText(system))
                    {
                        AllSystems[1].Children.Add(new LeftSideBarChild(system.Name, AllSystems[0].Header, system.StatusIcon, system));
                    }
                }
            }

            if (Project.GetProjectInstance?.ControlSystemList != null)
            {
                foreach (SystemBase system in Project.GetProjectInstance.ControlSystemList)
                {
                    if (ContainsSearchText(system))
                    {
                        AllSystems[2].Children.Add(new LeftSideBarChild(system.Name, AllSystems[0].Header, system.StatusIcon, system));
                    }
                }
            }
            if (AllSystems != null && AllSystems.Count > 0)
            {
                LeftSideBarItem sideBarItem = AllSystems.FirstOrDefault(a => a.Children.Count > 0);
                LeftSideBarChild first = sideBarItem != null ? sideBarItem.Children[0] : null;
                if (first != null)
                {
                    first.IsSelected = true;
                    _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)first.Source);
                }
            }
        }
        //Acc End IA
        private void OnSelectedSystemTab(SystemBase system)
        {
            LeftSideBarChild toSelect = AllSystems.FirstOrDefault(a => a.Children.FirstOrDefault(b=>b.Source.Id==system.Id)!=null)?.Children.FirstOrDefault(b => b.Source.Id == system.Id);
            if (toSelect != null)
            {
                toSelect.IsSelected = true;
            }
        }

        private void SelectedTreeViewItemClick(object leftSideBarChild)
        {
            if (leftSideBarChild.GetType().Equals(typeof(LeftSideBarChild)))
            {
               
                var object2 = ((LeftSideBarChild)leftSideBarChild).Source;

                var vrf = WorkFlowContext.CurrentSystem as JCHVRF.Model.NextGen.SystemVRF;
                if (vrf != null && vrf.HvacSystemType == "1")
                {
                   
                    if ((vrf.MyPipingOrphanNodes != null && vrf.MyPipingOrphanNodes.Count > 0 ) || (vrf.SystemStatus == SystemStatus.WIP && vrf.IsManualPiping))
                    {
                        JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_CANVAS"));
                        LeftSideBarChild toselect = AllSystems?.FirstOrDefault(a => a.Children.Count > 0 && a.Children.FirstOrDefault(b => ((SystemBase)b.Source)?.Id == _lastSelectedId) != null)?.Children.FirstOrDefault(b => ((SystemBase)b.Source)?.Id == _lastSelectedId);
                        if (toselect != null)
                        {
                            toselect.IsSelected = true;
                        }
                        else
                        {
                            toselect = AllSystems?.FirstOrDefault(a => a.Children.Count > 0 && a.Children.FirstOrDefault() != null)?.Children.FirstOrDefault();
                            toselect.IsSelected = true;
                        }
                        return;
                    }
                    //Start Bug#4695 : If ODU is dirty then get confirmation before user leaves the current system. Retain user on the same system if he wants to stay.
                    else if (vrf.IsODUDirty)
                    {
                        MessageBoxResult messageBoxResult = JCHMessageBox.Show(Langauge.Current.GetMessage("ALERT_CHANGES_ON_SYSTEM_FLIP"), MessageType.Warning, MessageBoxButton.YesNo);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            LeftSideBarChild toselect = AllSystems?.FirstOrDefault(a => a.Children.Count > 0 && a.Children.FirstOrDefault(b => ((SystemBase)b.Source)?.Id == _lastSelectedId) != null)?.Children.FirstOrDefault(b => ((SystemBase)b.Source)?.Id == _lastSelectedId);
                            if (toselect != null)
                            {
                                toselect.IsSelected = true;
                            }
                            else
                            {
                                toselect = AllSystems?.FirstOrDefault(a => a.Children.Count > 0 && a.Children.FirstOrDefault() != null)?.Children.FirstOrDefault();
                                toselect.IsSelected = true;
                            }
                            return;
                        }
                    }
                    //End Bug#4695
                }


                _eventAggregator.GetEvent<SystemSelectedItemSubscriber>().Publish((SystemBase)object2);
                _lastSelectedId = ((SystemBase)object2).Id;
                
            }
        }

        private void Reorder(object leftSideBarChild, int orderIdx)
        {
            if (leftSideBarChild.GetType().Equals(typeof(LeftSideBarChild)))
            {
                var selectedItem = ((LeftSideBarChild)leftSideBarChild).Source;
                int selectedItemType = 0;

                if (selectedItem.HvacSystemType == "1")
                {
                    selectedItemType = 0;
                }
                else if (selectedItem.HvacSystemType == "6")
                {
                    selectedItemType = 2;
                }
                else
                {
                    selectedItemType = 1;
                }
                if (AllSystems[selectedItemType].Children.Count > 1)
                {
                    int selectedItemIdx = AllSystems[selectedItemType].Children.IndexOf((LeftSideBarChild)leftSideBarChild);
                    int newIdx = selectedItemIdx + orderIdx;
                    int OrderLimit_Max = AllSystems[selectedItemType].Children.Count() - 1;

                    if (((orderIdx == -1) && (selectedItemIdx > 0)) || ((orderIdx == 1) && (OrderLimit_Max > selectedItemIdx)))
                    {
                        AllSystems[selectedItemType].Children.Move(selectedItemIdx, newIdx);
                    }
                }
            }
        }

        private void OnSearchGotFocus()
        {
            if (SearchText == Langauge.Current.GetMessage("DASHBOARD_SEARCH"))

                SearchText = string.Empty;
        }

        private void OnSearchLostFocus()
        {
            Regex regex = new Regex("^[A-Za-z0-9- ]+$");

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_SEARCH"));//Search text box should not contain blank spaces

                SearchText = Langauge.Current.GetMessage("DASHBOARD_SEARCH");
            }
            else
            {
                Match match = regex.Match(SearchText);
                if (!match.Success)
                {
                    JCHMessageBox.Show(Langauge.Current.GetMessage("ERROR_SEARCH_VALIDATION"));//Search text should be alphanumeric
                }
            }
        }

        private void OnSearchTextChanged()
        {
            filterSystemList();
        }

        #endregion

        #region Properties

        public string Title { get; private set; }
        public DelegateCommand<object> SelectedTreeViewItemCommand { get; set; }

        private ObservableCollection<LeftSideBarItem> _allSystems;

        /// <summary>
        /// Gets the LeftSideBarItems
        /// </summary>
        public ObservableCollection<LeftSideBarItem> AllSystems
        {
            get
            {
                if (_allSystems == null)
                {
                    _allSystems = new ObservableCollection<LeftSideBarItem>()
                    {
                        new LeftSideBarItem("VRF"){Children = new ObservableCollection<LeftSideBarChild>() },
                        new LeftSideBarItem("Heat Exchanger"){Children = new ObservableCollection<LeftSideBarChild>() },
                        new LeftSideBarItem("Central Controller"){Children = new ObservableCollection<LeftSideBarChild>() }
                    };
                }
                return _allSystems;
            }
            set
            {
                this.SetValue(ref _allSystems, value);
            }
        }

        public DelegateCommand LostFocusCommand { get; set; }
        public DelegateCommand GotFocusCommand { get; set; }
        public DelegateCommand TextChangedCommand { get; set; }

        private string _lastSelectedId;
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
            }
        }
        #endregion
    }
}
