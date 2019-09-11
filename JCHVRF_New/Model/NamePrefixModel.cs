/****************************** File Header ******************************\
File Name:	NamePrefixModel.cs
Date Created:	2/5/2019
Description:	
\*************************************************************************/

namespace JCHVRF_New.Model
{
    using JCHVRF_New.Common.Helpers;

    public class NamePrefixModel : ModelBase
    {
        #region Fields

        private string _buildingName;

        private string _controllers;

        private string _floorName;

        private string _indoorUnitsName;

        private string _outdoorUnitName;

        private string _roomName;

        private string _systemName;

        private string _totalHeatExchangers;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the BuildingName
        /// </summary>
        public string BuildingName
        {
            get { return _buildingName; }
            set { this.SetValue(ref _buildingName, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the Controllers
        /// </summary>
        public string Controllers
        {
            get { return _controllers; }
            set { this.SetValue(ref _controllers, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the FloorName
        /// </summary>
        public string FloorName
        {
            get { return _floorName; }
            set { this.SetValue(ref _floorName, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the IndoorUnitsName
        /// </summary>
        public string IndoorUnitsName
        {
            get { return _indoorUnitsName; }
            set { this.SetValue(ref _indoorUnitsName, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the OutdoorUnitName
        /// </summary>
        public string OutdoorUnitName
        {
            get { return _outdoorUnitName; }
            set { this.SetValue(ref _outdoorUnitName, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the RoomName
        /// </summary>
        public string RoomName
        {
            get { return _roomName; }
            set { this.SetValue(ref _roomName, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the SystemName
        /// </summary>
        public string SystemName
        {
            get { return _systemName; }
            set { this.SetValue(ref _systemName, value.TrimStart()); }
        }

        /// <summary>
        /// Gets or sets the TotalHeatExchangers
        /// </summary>
        public string TotalHeatExchangers
        {
            get { return _totalHeatExchangers; }
            set { this.SetValue(ref _totalHeatExchangers, value.TrimStart()); }
        }

        #endregion
    }
}
