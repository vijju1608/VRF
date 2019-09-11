/****************************** File Header ******************************\
File Name:	SystemDetailsViewModel.cs
Date Created:	2/20/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New.ViewModels
{
    using System.Collections.Generic;
    using JCBase.Utility;
    using JCHVRF.Model;
    using JCHVRF_New.Common.Contracts;
    using JCHVRF_New.Common.Helpers;
    using JCHVRF_New.Model;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Prism.Events;
    using JCHVRF.Model.NextGen;
    using System;
    using Prism.Commands;
    using language = JCHVRF_New.LanguageData.LanguageViewModel;
    using JCHVRF_New.Common.Constants;

    public class VRFSystemDetailsViewModel : ViewModelBase
    {
        #region Fields

        private double _FloatingHeight;

        private double _FloatingLeft;

        private double _FloatingTop;

        private double _FloatingWidth;

        private string _title;

        private JCHVRF.Model.NextGen.SystemVRF CurrentSystem;

        public DelegateCommand EditSystemDetailsCommand { get; set; }

        public DelegateCommand VRFSystemDetails_Load { get; set; }

        public DelegateCommand VRFSystemDetails_UnLoad { get; set; }


        JCHVRF.Model.Project CurrentProject;

        private ObservableCollection<IDUODUCapacityDetails> _listIndoorCapacitydetails;
        private ObservableCollection<IDUODUCapacityDetails> _listOutdoorCapacitydetails;
        private List<IDUODUCapacityDetails> _listAdditionalCapacitydetails;
        // acc start IA
        private List<MaterialList> _ModelDetails;
        //Acc stop IA
        private double _actualRatio;
        private double _maxConnections;
        private double _minConnections;
        private double _additionalRefrigerantQty;
        private IModalWindowService _winService;
        private IEventAggregator eventAggregator;
        #endregion

        #region Constructors

        public VRFSystemDetailsViewModel(IEventAggregator _eventAggregator, IModalWindowService winService)
        {
            try
            {
                eventAggregator = _eventAggregator;
                _winService = winService;
                EditSystemDetailsCommand = new DelegateCommand(OnEditSystemDetailsCommandClick);
                VRFSystemDetails_Load = new DelegateCommand(VRFSystemDetails_Loaded);
                VRFSystemDetails_UnLoad = new DelegateCommand(VRFSystemDetails_UnLoaded);
                //if (WorkFlowContext.CurrentSystem != null)
                //{
                //    if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                //        reloadsystemDetails((WorkFlowContext.CurrentSystem) as JCHVRF.Model.NextGen.SystemVRF);
                //}
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }


        #endregion

        #region Properties

        // acc start -- IA
        public List<MaterialList> ModelDetail
        {
            get { return _ModelDetails; }
            set { this.SetValue(ref _ModelDetails, value); }
        }
        //acc stop -- IA
        /// <summary>
        /// Gets or sets the FloatingHeight
        /// </summary>
        public double FloatingHeight
        {
            get { return this._FloatingHeight; }
            set { this.SetValue(ref _FloatingHeight, value); }
        }

        /// <summary>
        /// Gets or sets the FloatingLeft
        /// </summary>
        public double FloatingLeft
        {
            get { return this._FloatingLeft; }
            set { this.SetValue(ref _FloatingLeft, value); }
        }

        /// <summary>
        /// Gets or sets the FloatingTop
        /// </summary>
        public double FloatingTop
        {
            get { return this._FloatingTop; }
            set { this.SetValue(ref _FloatingTop, value); }
        }

        /// <summary>
        /// Gets or sets the ListIndoorCapacitydetails
        /// </summary>
        public ObservableCollection<IDUODUCapacityDetails> ListIndoorCapacitydetails
        {
            get { return _listIndoorCapacitydetails; }
            set { this.SetValue(ref _listIndoorCapacitydetails, value); }
        }

        /// <summary>
        /// Gets or sets the ListAdditionalCapacitydetails
        /// </summary>
        public List<IDUODUCapacityDetails> ListAdditionalCapacitydetails
        {
            get { return _listAdditionalCapacitydetails; }
            set { this.SetValue(ref _listAdditionalCapacitydetails, value); }
        }

        /// <summary>
        /// Gets or sets the ListOutdoorCapacitydetails
        /// </summary>
        public ObservableCollection<IDUODUCapacityDetails> ListOutdoorCapacitydetails
        {
            get { return _listOutdoorCapacitydetails; }
            set { this.SetValue(ref _listOutdoorCapacitydetails, value); }
        }


        /// <summary>
        /// Gets or sets the FloatingWidth
        /// </summary>
        public double FloatingWidth
        {
            get { return this._FloatingWidth; }
            set { this.SetValue(ref _FloatingWidth, value); }
        }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set { this.SetValue(ref _title, value); }
        }


        /// <summary>
        /// Gets or sets the ActualRatio
        /// </summary>
        public double ActualRatio
        {
            get { return _actualRatio; }
            set { this.SetValue(ref _actualRatio, value); }
        }

        /// <summary>
        /// Gets or sets the minConnections
        /// </summary>
        public double minConnections
        {
            get { return _minConnections; }
            set { this.SetValue(ref _minConnections, value); }
        }

        /// <summary>
        /// Gets or sets the maxConnections
        /// </summary>
        public double maxConnections
        {
            get { return _maxConnections; }
            set { this.SetValue(ref _maxConnections, value); }
        }

        /// <summary>
        /// Gets or sets the AdditionalRefrigerantQty
        /// </summary>
        public double AdditionalRefrigerantQty
        {
            get { return _additionalRefrigerantQty; }
            set { this.SetValue(ref _additionalRefrigerantQty, value); }
        }
        public string SystemSettings { get; private set; }
        #endregion

        #region Methods

        private void reloadsystemDetails(JCHVRF.Model.NextGen.SystemVRF obj)
        {
            RefreshData(obj);
        }
        private void RefreshData(JCHVRF.Model.NextGen.SystemVRF currentSys)
        {
            CurrentSystem = currentSys;
            var IDUList=new List<RoomIndoor>();

            if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
            {
                if(CurrentSystem!=null)
                IDUList = JCHVRF.Model.Project.GetProjectInstance.RoomIndoorList.Where(idu => idu.SystemID == ((JCHVRF.Model.SystemBase)CurrentSystem).Id).ToList();
            }
            BindIndoorCapacityDetails(IDUList);
            BindOutdoorCapacityDetails();
            BindAdditionalCapacityDetails();
            BindModelDetails();

            if (CurrentSystem != null && CurrentSystem.OutdoorItem != null)
            {

                ActualRatio = CurrentSystem.Ratio * 100;
                minConnections = IDUList.Count();
                maxConnections = CurrentSystem.OutdoorItem.MaxIU;
                if (CurrentSystem.IsInputLengthManually == false)
                    AdditionalRefrigerantQty = 0;
                else
                    AdditionalRefrigerantQty = CurrentSystem.OutdoorItem.RefrigerantCharge;
            }
        }

        private void VRFSystemDetails_Loaded()
        {
            if (WorkFlowContext.CurrentSystem != null)
            {
                if (WorkFlowContext.CurrentSystem is JCHVRF.Model.NextGen.SystemVRF)
                    reloadsystemDetails((WorkFlowContext.CurrentSystem) as JCHVRF.Model.NextGen.SystemVRF);
            }
            eventAggregator.GetEvent<SystemDetailsSubscriber>().Subscribe(reloadsystemDetails);
        }

        private void VRFSystemDetails_UnLoaded()
        {
            eventAggregator.GetEvent<SystemDetailsSubscriber>().Unsubscribe(reloadsystemDetails);
        }
        private void OnEditSystemDetailsCommandClick()
        {
            _winService.ShowView(ViewKeys.EditSystemDetails);
        }
        public void BindOutdoorCapacityDetails()
        {
            try
            {
                if (CurrentSystem != null && CurrentSystem.OutdoorItem != null)
                {

                    ListOutdoorCapacitydetails = new ObservableCollection<IDUODUCapacityDetails>();
                    IDUODUCapacityDetails obj1 = new IDUODUCapacityDetails();
                    obj1.Capacity = language.Current.GetMessage("COOLING");
                    obj1.IsValidMode = true;
                    if (CurrentSystem != null && (Project.GetProjectInstance.IsCoolingModeEffective || (Project.GetProjectInstance.IsCoolingModeEffective && Project.GetProjectInstance.IsHeatingModeEffective) ||
                        Project.GetProjectInstance.IsBothMode))
                    {
                        obj1.RatedCapacity = CurrentSystem.OutdoorItem.CoolingCapacity;
                        obj1.CorrectedCapacity = CurrentSystem.CoolingCapacity;
                    }
                    else
                        obj1.IsValidMode = false;
                    ListOutdoorCapacitydetails.Add(obj1);

                    IDUODUCapacityDetails obj2 = new IDUODUCapacityDetails();
                    obj2.Capacity = language.Current.GetMessage("HEATING");
                    obj2.IsValidMode = true;
                    if (CurrentSystem != null && (Project.GetProjectInstance.IsHeatingModeEffective || (Project.GetProjectInstance.IsCoolingModeEffective && Project.GetProjectInstance.IsHeatingModeEffective) ||
                        Project.GetProjectInstance.IsBothMode))
                    {
                        obj2.RatedCapacity = CurrentSystem.OutdoorItem.HeatingCapacity;
                        obj2.CorrectedCapacity = CurrentSystem.HeatingCapacity;
                    }
                    else
                        obj2.IsValidMode = false;

                    ListOutdoorCapacitydetails.Add(obj2);
                }
            }
            catch (Exception ex)
            {
                int? id = Project.GetProjectInstance?.projectID;
                Logger.LogProjectError(id, ex);
            }
        }
        //Acc start IA
        public void BindModelDetails()
        {
            if (!(Project.CurrentProject.ControllerList.Count.Equals(0)))
            {
                ModelDetail = new List<MaterialList>();
                MaterialList obj1 = new MaterialList();
                obj1.Model = Project.CurrentProject.ControllerList[0].Model;
                obj1.Description = Project.CurrentProject.ControllerList[0].Description;
                obj1.Qty = Project.CurrentProject.ControllerList[0].Quantity;
                ModelDetail.Add(obj1);
            }



        }

        //Acc Stop IA
        public void BindIndoorCapacityDetails(List<RoomIndoor> IDUList)
        {
            try
            {
                if (CurrentSystem != null && CurrentSystem.OutdoorItem != null)
                {
                    ListIndoorCapacitydetails = new ObservableCollection<IDUODUCapacityDetails>();
                    IDUODUCapacityDetails obj1 = new IDUODUCapacityDetails();
                    obj1.Capacity = language.Current.GetMessage("COOLING");
                    IDUData(obj1, IDUList);
                    ListIndoorCapacitydetails.Add(obj1);

                    IDUODUCapacityDetails obj2 = new IDUODUCapacityDetails();
                    obj2.Capacity = language.Current.GetMessage("SENSIBLE");
                    IDUData(obj2, IDUList);

                    ListIndoorCapacitydetails.Add(obj2);

                    IDUODUCapacityDetails obj3 = new IDUODUCapacityDetails();
                    obj3.Capacity = language.Current.GetMessage("HEATING");
                    IDUData(obj3, IDUList);
                    ListIndoorCapacitydetails.Add(obj3);
                }
            }
            catch
            { }
        }

        public void BindAdditionalCapacityDetails()
        {
            ListAdditionalCapacitydetails = new List<IDUODUCapacityDetails>();
            IDUODUCapacityDetails obj1 = new IDUODUCapacityDetails();
            obj1.Capacity = language.Current.GetMessage("EER");
            //obj1.RatedCapacity = 0.0;
            if (obj1.RatedCapacity == 0.0)
            {
                obj1.RatedCapacityNew = "-";
            }
            else
            {
                obj1.RatedCapacityNew = Convert.ToString(obj1.RatedCapacity);
            }


            if (obj1.CorrectedCapacity == 0.0)
            {
                obj1.CorrectedCapacityNew = "-";
            }
            else
            {
                obj1.CorrectedCapacityNew = Convert.ToString(obj1.CorrectedCapacity);
            }

            ListAdditionalCapacitydetails.Add(obj1);


            IDUODUCapacityDetails obj2 = new IDUODUCapacityDetails();
            obj2.Capacity = language.Current.GetMessage("COP");
            //obj2.RatedCapacity = 0.0;
            if (obj2.RatedCapacity == 0.0)
            {
                obj2.RatedCapacityNew = "-";
            }
            else
            {
                obj2.RatedCapacityNew = Convert.ToString(obj2.RatedCapacity);
            }

            //obj2.CorrectedCapacity = 0.0;
            if (obj2.CorrectedCapacity == 0.0)
            {
                obj2.CorrectedCapacityNew = "-";
            }
            else
            {
                obj2.CorrectedCapacityNew = Convert.ToString(obj2.CorrectedCapacity);
            }
            ListAdditionalCapacitydetails.Add(obj2);


            IDUODUCapacityDetails obj3 = new IDUODUCapacityDetails();
            obj3.Capacity = language.Current.GetMessage("SEER");
            // obj3.RatedCapacity = 0.0;
            if (obj3.RatedCapacity == 0.0)
            {
                obj3.RatedCapacityNew = "-";
            }
            else
            {
                obj3.RatedCapacityNew = Convert.ToString(obj3.RatedCapacity);
            }

            //obj3.CorrectedCapacity = 0.0;
            if (obj3.CorrectedCapacity == 0.0)
            {
                obj3.CorrectedCapacityNew = "-";
            }
            else
            {
                obj3.CorrectedCapacityNew = Convert.ToString(obj3.CorrectedCapacity);
            }
            ListAdditionalCapacitydetails.Add(obj3);
        }

        private void IDUData(IDUODUCapacityDetails obj, List<RoomIndoor> IDUList)
        {

            if (obj.Capacity == language.Current.GetMessage("COOLING"))
            {
                if (Project.GetProjectInstance.IsCoolingModeEffective || (Project.GetProjectInstance.IsCoolingModeEffective && Project.GetProjectInstance.IsHeatingModeEffective) ||
                       Project.GetProjectInstance.IsBothMode)
                    obj.IsValidMode = true;
                else
                { 
                    obj.IsValidMode = false;
                    return;
                }

                foreach (var item in IDUList)
                {
                    if (item != null)
                    {
                        obj.RatedCapacity += item.IndoorItem.CoolingCapacity;
                        obj.CorrectedCapacity += item.ActualCoolingCapacity;
                    }
                    
                }
            }

            else if (obj.Capacity == language.Current.GetMessage("HEATING"))
            {
                if(Project.GetProjectInstance.IsHeatingModeEffective || (Project.GetProjectInstance.IsCoolingModeEffective && Project.GetProjectInstance.IsHeatingModeEffective) ||
                        Project.GetProjectInstance.IsBothMode)
                    obj.IsValidMode = true;
                else
                { 
                    obj.IsValidMode = false;
                    return;
                }

                foreach (var item in IDUList)
                {
                    if (item != null)
                    {
                        obj.RatedCapacity += item.IndoorItem.HeatingCapacity;
                        obj.CorrectedCapacity += item.ActualHeatingCapacity;
                    }
                    
                }
            }


            else if (obj.Capacity == language.Current.GetMessage("SENSIBLE"))
            {
                obj.IsValidMode = true;
                foreach (var item in IDUList)
                {
                    if (item != null)
                    {
                        obj.RatedCapacity += item.IndoorItem.SensibleHeat;
                        obj.CorrectedCapacity += item.ActualSensibleHeat;
                    }
                }
            }

        }
    }
    #endregion
}

