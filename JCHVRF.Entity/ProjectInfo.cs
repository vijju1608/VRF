using JCHVRF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.Entity
{
    /// <summary>
    /// Main Project Class 
    /// This Class Contains all fields of ProjectInfo Table
    /// This Class also Contains LegacyProject Object 
    /// </summary>
    public sealed class ProjectInfo
    {
        #region Properties
        public int ID { get; set; }
        public int SystemID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ContactName { get; set; }
        public bool ActiveFlag { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Version { get; set; }
        public string DBVersion { get; set; }
        public int Measure { get; set; }
        public string Location { get; set; }
        public string SoldTo { get; set; }
        public string ShipTo { get; set; }
        public string OrderNo { get; set; }
        public string ContractNo { get; set; }
        public string Region { get; set; }
        public string Office { get; set; }
        public string Engineer { get; set; }
        public string YINo { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string Remarks { get; set; }
        public string ProjectType { get; set; }
        public string Vendor { get; set; }
        public byte[] ProjectBlob { get; set; }
        public byte[] SystemBlob { get; set; }
        public byte[] SQBlob { get; set; }
        //Sync Fields
        public int UserID { get; set; }
        public int GlobalProjectID { get; set; }
        public Project ProjectLegacy { get; set; }
        #endregion Properties

        public static ProjectInfo create()
        {
            return new ProjectInfo();
        }

        private ProjectInfo()
        {

        }
    }
}