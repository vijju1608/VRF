using JCHVRF.BLL;
using JCHVRF.BLL.New;
using JCHVRF.Model;
using JCHVRF_New.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

using JCBase.UI;
using JCHVRF_New.Common.Helpers;
using JCHVRF_New.Views;
using Prism.Commands;


namespace JCHVRF_New.ViewModels
{
    class AddHEAccessoriesTemplateViewModel : ViewModelBase
    {        
        public RoomIndoor objIndoor;
        
        Project thisProject;
        public string BrandCode { get; set; }
        public string Region { get; set; }
        public ObservableCollection<AccessoryModel> ListAccessory
        {
            get { return _listAccessory; }
            set { this.SetValue(ref _listAccessory, value); }
        }     

        private int _maximunCount;
        public int MaximunCount
        {
            get { return _maximunCount; }
            set {
                this.SetValue(ref _maximunCount, value);
                
            }
        }

        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                this.SetValue(ref _count, value);
                IncrementCount();
            }
        }

        public void IncrementCount()
        {
            {
                
                return;
            } 
        }

        public ObservableCollection<AccessoryModel> ExistingAccessories
        {
            get
            {
                return _existingAccessories;
            }
            set
            {
                this.SetValue(ref _existingAccessories, value);
            }
        }

        private bool _addbuttonenable;
        public bool AddButtonEnable
        {
            get { return this._addbuttonenable; }
            set { this.SetValue(ref _addbuttonenable, value); }
        }

        private ObservableCollection<AccessoryModel> _existingAccessories;
        private ObservableCollection<AccessoryModel> _listAccessory;

        /// <summary>
        /// Gets or sets the MainAppLoadedCommand
        /// </summary>
        public DelegateCommand MainAppLoadedCommand { get; set; }

        public DelegateCommand<object> MaxTypeCount { get; set; }

        public DelegateCommand<object> Cancelcommand { get; set; }

        #region RelayCommand 
        // public RelayCommand Cancelcommand { get; set; }

        public RelayCommand AddAccessoriesCommand { get; set; }

        public RelayCommand RemoveFromAllCommand { get; set; }

        List<AccessoryModel> SelectedAccessories { get; set; }
        #endregion

        private IProjectInfoBAL _projectBAL;

        public AddHEAccessoriesTemplateViewModel(IProjectInfoBAL projectBAL)
        {
            try
            {
                this._projectBAL = projectBAL;
                //this.Cancelcommand = new RelayCommand(this.Cancel);
                AddButtonEnable = true;
                Cancelcommand = new DelegateCommand<object>(OnCancelClicked);
                MaxTypeCount = new DelegateCommand<object>(MaxTypeCountCheck);
                this.AddAccessoriesCommand = new RelayCommand(this.AddAccessoriesToHeatExchanger, CanAddExecute);
                this.RemoveFromAllCommand = new RelayCommand(this.RemoveFromAllAccessories, CanRemoveAllExecute);
                Project.CurrentProject = Project.GetProjectInstance;
                MainAppLoadedCommand = new DelegateCommand(
                   () =>
                   {
                       ListAccessory = GetAllAccessoriesCompatibleForHeatExchanger(objIndoor);
                       UpdateSelectedAccessories();
                       GetSelectedAccessories(ListAccessory);
                       MaxTypeCountCheck(null);
                   }
                   );

            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }



        bool validityStatus = true;

        #region priavate methods
       

        private void OnCancelClicked(object win)
        {
           CloseWindow(win);
        }

        private void MaxTypeCountCheck(object obj)
        {
            bool validstatis = true;
            int typecount = 0;
            if (ListAccessory != null)
            {
                var selectedrowobj = (AccessoryModel)obj;
                var selectedItems = (from s in ListAccessory where s.IsSelect == true select s).ToList();

                foreach (AccessoryModel accessory1 in selectedItems)
                {
                    typecount = 0;
                    if (accessory1.IsSelect == false) continue;

                    selectedrowobj = accessory1;
                if (selectedItems.Count > 0 && selectedrowobj != null)
                {
                    foreach (AccessoryModel accessory in selectedItems)
                    {
                        if (selectedrowobj.Description == accessory.Description)
                        {
                            typecount += accessory.Count;
                        }
                    }
                    if (typecount > selectedrowobj.MaxCount)
                    {
                            validstatis = false;
                    }
                    else
                    {
                       // AddButtonEnable = true;
                    }
                }
            }
                var selectedrowobjmsg = new AccessoryModel();
                if (obj != null)
                {
                    selectedrowobjmsg = (AccessoryModel)obj;
                }

                if (validstatis == true)
                {
                    AddButtonEnable = true;
                }
                else if (obj == null)
                {
                    AddButtonEnable = false;
                }
                else if(selectedrowobjmsg.IsSelect == true)
                {
                    AddButtonEnable = false;
                    JCHMessageBox.Show("selected count is greater than max count of type ");
                }
            }
        }
 
        private bool CanAddExecute(object parameter)
        {
            if (ListAccessory != null)
            {
                var selectedItems = (from s in ListAccessory where s.IsSelect == true select s).ToList();
                var applyToSimilar =
                    (from a in ListAccessory where a.IsApplyToSimilarUnit == true select a).ToList();
                if (selectedItems.Count > 0 || applyToSimilar.Count > 0)
                    return true;
            
            }
            return false;
        }

        private bool CanRemoveAllExecute(object parameter)
        {
            if (ListAccessory != null)
            {
                    var selectedItems = (from s in ListAccessory where s.IsSelect == true select s).ToList();
                    if (selectedItems.Count > 0)
                        return true;
                
            }

            return false;
        }

        private void AddAccessoriesToHeatExchanger(object win)
        {
            List<string> modelNames = new List<string>();

            try
            {
                if (objIndoor.ListAccessory == null)
                {
                    objIndoor.ListAccessory = new List<Accessory>();
                }

                var selectedItems = (from s in ListAccessory where s.IsSelect == true select s).ToList();
                var applyToSimilar = (from a in ListAccessory where a.IsApplyToSimilarUnit == true select a).ToList();
                var applyToSimilarOnly = (from a in ListAccessory where a.IsApplyToSimilarUnit == true && a.IsSelect != true select a);
                foreach (AccessoryModel accessory in selectedItems)
                {
                    Accessory acc = new Accessory()
                    {
                        Type = accessory.Type,
                        BrandCode = Project.CurrentProject.BrandCode,
                        FactoryCode = Project.CurrentProject.FactoryCode,
                        IsDefault = false, //? todo 
                        IsShared = false,//todo
                        MaxCapacity = 10,//todo
                        Model_Hitachi = accessory.Model,
                        MaxNumber = accessory.MaxCount,
                        Count = accessory.Count,
                        MinCapacity = 5,
                        Model_York = "",//todo
                        UnitType = "",//todo
                        IsSelect = true
                    };                   

                    if (accessory.Count > 0)
                    {
                        if (objIndoor != null)
                        {
                            if(objIndoor.ListAccessory.Count > 0)
                            {
                                int accCount = objIndoor.ListAccessory.Count;
                                for(int i = 0; i < accCount; i++)
                                {
                                    var result = objIndoor.ListAccessory.Select(x => x.Model_Hitachi).Distinct();
                                    foreach (var type in result)
                                    {
                                        modelNames.Add(type);
                                    }

                                    if (!modelNames.Contains(acc.Model_Hitachi))
                                    { 
                                        objIndoor.ListAccessory.Add(acc);
                                    }
                                    else
                                    {
                                        foreach(Accessory ac in objIndoor.ListAccessory)
                                        {
                                            if(ac.Model_Hitachi.Equals(acc.Model_Hitachi))
                                            {
                                                ac.Count = acc.Count;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                objIndoor.ListAccessory.Add(acc);
                            }                           
                        }
                    }

                    else
                    {
                        JCHMessageBox.Show("Count can not be zero");
                        return;
                    }
                }
                _projectBAL.UpdateProject(Project.CurrentProject);
                CloseWindow(win);
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }

        private void RemoveFromAllAccessories(object win)
        {
            if (JCHMessageBox.Show("Are you sure want to remove Accessories", MessageType.Information, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                try
                {

                    var selectedItems = (from s in ListAccessory where s.IsSelect == true select s).ToList();

                    foreach (AccessoryModel accessory in selectedItems)
                    {

                        if (objIndoor != null && objIndoor.IndoorItem != null) //ri.IndoorItem.ListAccessory.Count > 0
                        {
                            if (objIndoor.ListAccessory != null && objIndoor.ListAccessory.Count > 0)
                                objIndoor.ListAccessory.RemoveAll(c => c.Type == accessory.Type);
                            if (objIndoor.IndoorItem.ListAccessory != null && objIndoor.IndoorItem.ListAccessory.Count > 0)
                                objIndoor.IndoorItem.ListAccessory.RemoveAll(a => a.Type == accessory.Type);
                            accessory.IsSelect = false;
                        }

                    }
                }
                catch (Exception ex)
                {
                    int? id = Project.GetProjectInstance?.projectID;
                    Logger.LogProjectError(id, ex);
                }
               }           
            _projectBAL.UpdateProject(Project.CurrentProject);
            CloseWindow(win);

        }

        private static void CloseWindow(object parameter)
        {
            AddHEAccessoriesTemplate template = (AddHEAccessoriesTemplate) parameter;
            Window win = template.Parent as Window;
            if (win != null)
            {                
                win.Close();
            }
        }

        private RoomIndoor GetRoomIndoorDetial(RoomIndoor IndoorNo)
        {
            RoomIndoor ind = new RoomIndoor();
            return ind;
        }

        private ObservableCollection<AccessoryModel> GetAllAccessoriesCompatibleForHeatExchanger(RoomIndoor roomIndoor)
        {
            List<AccessoryModel> objListAccessory = new List<AccessoryModel>();
            AccessoryBLL accessoryBLL = new AccessoryBLL();
            var ri = roomIndoor;
            DataTable dtAccessory = null;
            if (ri != null && ri.IndoorItem != null)
            {
                string factoryCode = ri.IndoorItem.GetFactoryCode();

                dtAccessory = accessoryBLL.GetInDoorAccessoryItemList(Project.CurrentProject.BrandCode,
                 factoryCode, ri.IndoorItem.Type, ri.IndoorItem.CoolingCapacity,
                 Project.CurrentProject.RegionCode, ri, Project.CurrentProject.SubRegionCode);
            }

            //Display Name
            DataTable dtDisplayName = accessoryBLL.GetUniversal_ByAccessory(Project.CurrentProject.SubRegionCode, Project.CurrentProject.BrandCode);
            
            //Filling Accessory List
            if(dtAccessory != null)
            {
                if (dtAccessory.Rows.Count != 0)
                {
                    if (Project.CurrentProject.BrandCode == "H")
                    {
                        objListAccessory = (from DataRow row in dtAccessory.Rows
                                            select new AccessoryModel
                                            {
                                                Type = row["Type"].ToString(),
                                                Model = row["Model_Hitachi"].ToString(),
                                                Description = row["Type"].ToString(),
                                                Count = Convert.ToInt32(row["IsDefault"]),
                                                MaxCount = Convert.ToInt32(row["MaxNumber"]),
                                                IsSelect = false,
                                                IsApplyToSimilarUnit = false

                                            }).ToList();
                    }
                    else
                    {
                        objListAccessory = (from DataRow row in dtAccessory.Rows
                                            select new AccessoryModel
                                            {
                                                Type = row["Type"].ToString(),
                                                Model = row["Model_York"].ToString(),
                                                Description = row["Type"].ToString(),
                                                Count = Convert.ToInt32(row["IsDefault"]),
                                                MaxCount = Convert.ToInt32(row["MaxNumber"]),
                                                IsSelect = false,
                                                IsApplyToSimilarUnit = false

                                            }).ToList();
                    }
                }
            }
            
            //Final Accessory List
            List<AccessoryModel> finalListAccessory = new List<AccessoryModel>();
            List<string> distictType = new List<string>();
            
            var result = objListAccessory.Select(x => x.Model).Distinct();
            foreach (var type in result)
            {
                distictType.Add(type);
            }
            foreach (var item in distictType)
            {
                finalListAccessory.Add(new AccessoryModel
                {
                    Type = objListAccessory.Where(x => x.Model.Equals(item)).Select(y => y.Type).FirstOrDefault(),
                    ModelName = objListAccessory.Where(x => x.Model.Equals(item)).Select(y => y.Model).ToList(),
                    Model = objListAccessory.Where(x => x.Model.Equals(item)).Select(y => y.Model).FirstOrDefault(),
                    IsSelect = false,
                    IsApplyToSimilarUnit = false,
                    Count = objListAccessory.Where(x => x.Model.Equals(item)).Select(y => y.Count).FirstOrDefault(),
                    MaxCount = Convert.ToInt32(objListAccessory.Where(x => x.Model.Equals(item)).Select(y => y.MaxCount).FirstOrDefault()),
                    Description = objListAccessory.Where(x => x.Model.Equals(item)).Select(y => y.Type).FirstOrDefault()
                });
            }
            
            return new ObservableCollection<AccessoryModel>(finalListAccessory);
        }


        private void  UpdateSelectedAccessories()
        {

            //var accessories = new ObservableCollection<AccessoryModel>();
           
            //    if ( objIndoor.IndoorItem != null)
            //    {

            //        if (objIndoor.IndoorItem.ListAccessory != null && objIndoor.IndoorItem.ListAccessory.Count > 0)
            //        {
            //            foreach (Accessory accessory in objIndoor.IndoorItem.ListAccessory)
            //            {
            //                accessories.Add(new AccessoryModel() { Type = accessory.Type, Count = accessory.MaxNumber });

            //                foreach (var acc in ListAccessory)
            //                {
            //                    if (acc.Type.Equals(accessory.Type))
            //                    {
            //                        acc.IsSelect = true;
            //                    }
            //                }

            //            }
            //        }
            //    }

            //    _existingAccessories =  accessories;
        }
       

        private List<int> _similarIDUonCanvas;
        public List<int> similarIDUonCanvas
        {
            get
            {
                if (_similarIDUonCanvas == null)
                {
                    new List<int>();
                }
                return _similarIDUonCanvas;
            }
            set
            {
                this.SetValue(ref _similarIDUonCanvas, value);
            }

        }
        private ObservableCollection<Accessory> _accessories;

        public ObservableCollection<Accessory> Accessories
        {

            get
            {
                if (_accessories == null)
                {
                    _accessories = new ObservableCollection<Accessory>();
                }
                return _accessories;
            }
            set
            {
                this.SetValue(ref _accessories, value);
            }
        }       

        private void GetSelectedAccessories(ObservableCollection<AccessoryModel> listAcc)
        {
            if (objIndoor != null && objIndoor.ListAccessory != null)
            {
                foreach (Accessory row in objIndoor.ListAccessory)
                {
                    string Model = row.Model_Hitachi.ToString();
                    foreach (var currentrow in listAcc)
                    {
                        if (row.Type == currentrow.Description && Model == currentrow.Model)
                        {
                            currentrow.IsSelect = row.IsSelect;
                            currentrow.Count = row.Count;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
