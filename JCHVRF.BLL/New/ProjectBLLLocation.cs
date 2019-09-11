using JCHVRF.DAL.New;
using JCHVRF.Model.New;
using System;
using System.Data;

namespace JCHVRF.BLL.New
{
    public class ProjectBLLLocation
    {
        public int InsertLocationDetails(ProjectLocation objUserDetails)
        {
            DalProjectLocation objUserDAL = new DalProjectLocation();
            try
            {
                return objUserDAL.InsertLocationDb(objUserDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objUserDAL = null;
            }
        }

        public DataTable GetData()
        {
            DalProjectLocation objUserDAL = new DalProjectLocation();
            return objUserDAL.Read();

        }

        public DataTable GetDataSubRegion(string pCode)
        {
            DalProjectLocation objUserDAL = new DalProjectLocation();
            return objUserDAL.ReadDataSubRegion(pCode);

        }
    }
    
}
