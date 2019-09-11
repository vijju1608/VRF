using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.Model.New
{
    [Serializable]
    public class ProjectInfo : ModelBase
    {
         //Win App  Properties
        public string SystemID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int ActiveFlag { get; set; }
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
        public DateTime OrderDate { get; set; }
        public string Remarks { get; set; }
        public string ProjectType { get; set; }
        public string Vendor { get; set; }
        public byte[] ProjectBlob { get; set; }
        public byte[] SystemBlob { get; set; }
        public byte[] SQBlob { get; set; }

        public DateTime DeliveryDate { get; set; }

        //Sync Fields
        public int UserID { get; set; }
        public int GlobalProjectID { get; set; }

        //Win App  Properties
    }

    [Serializable]
   public class ProjectBlobInfo : ModelBase
    {
        //  private string exstingClient;
        //public string ExistingClient
        //{
        //    get
        //    {
        //        return exstingClient;
        //    }
        //    set
        //    {
        //        exstingClient = value;
        //        OnPropertyRaised(ExistingClient);
        //    }
        //}

        public string ExistingClient { get; set; }
        public string Brand { get; set; }

        public string SelectedSource { get; set; }
    }

}
