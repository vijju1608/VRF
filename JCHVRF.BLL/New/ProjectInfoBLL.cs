using JCHVRF.DAL.New;
using JCHVRF.Model.New;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Linq;

namespace JCHVRF.BLL.New
{

    public interface IProjectInfoBAL
    {
        List<JCHVRF.Entity.ProjectInfo> GetAllProjectsList();
        List<JCHVRF.Entity.ProjectInfo> GetAllProjectsRegionWise(string searchType, string region, string subRegion);
        JCHVRF.Entity.ProjectInfo GetProjectInfo(int projectId);
        bool CreateProject(JCHVRF.Model.Project project);
        bool UpdateProject(JCHVRF.Model.Project project);
        int GetMaxProjectId();
        string GetDefaultProjectName();
        List<Tuple<string, string>> GetClientInfoList(string typeText);
        List<Tuple<string, string>> GetCountry();
        List<Tuple<string, string>> GetCity(int? CountryID);

        List<Tuple<string, string>> GetCreatorInfo();
        List<Tuple<string, string>> GetClientInfo();

        int InsertCreatorInfo(Model.New.Creator ObjCreator);
        int UpdateCreatorInfo(Model.New.Creator ObjCreator);

        int InsertClientInfo(Client objClient);
        int UpdateClientInfo(Client objClient);
        List<Tuple<string, string, string>> GetAllSystemType();

    }
    public class ProjectInfoBLL : IProjectInfoBAL
    {
        private IProjectInfoDAL _projectInfoDAL;
        //public ProjectInfoBLL(IProjectInfoDAL projectInfoDAL)
        //{
        //    _projectInfoDAL = projectInfoDAL;
        //}
        public ProjectInfoBLL()
        {
            _projectInfoDAL = new ProjectInfoDAL();
        }
        public int InsertProjectInfoDetails(Project objProjectInfoBlob)
        {
            int val = 0;
            DalProjectInfo objProjectInfoDAL = new DalProjectInfo();
            try
            {

                objProjectInfoDAL.InsertProjectInfoDb(objProjectInfoBlob);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }

            return val;
        }
        /// <summary>
        /// To Get List of All Projects
        /// </summary>
        /// <returns></returns>
        public List<JCHVRF.Entity.ProjectInfo> GetAllProjectsList()
        {
            List<JCHVRF.Entity.ProjectInfo> projectsList = new List<Entity.ProjectInfo>();

            try
            {
                projectsList = _projectInfoDAL.GetAllProjectInfo();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objUserDAL = null;
            }

            return projectsList;
        }
        public List<JCHVRF.Entity.ProjectInfo> GetAllProjectsRegionWise(string searchType, string region, string subRegion)
        {
            List<JCHVRF.Entity.ProjectInfo> projectsList = new List<Entity.ProjectInfo>();

            try
            {
                projectsList = _projectInfoDAL.GetAllProjectsRegionWise(searchType, region, subRegion);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objUserDAL = null;
            }

            return projectsList;
        }
        public string GetDefaultProjectName()
        {
            List<JCHVRF.Entity.ProjectInfo> projectsList = new List<Entity.ProjectInfo>();
            try
            {
                projectsList = _projectInfoDAL.GetAllProjectInfo();
            }
            catch (Exception ex)
            {
                throw ex;
            }           
            string defaultName = "Untitled Project";
            Int64 count = 0;
            try
            {
                var list = projectsList.Where(a => a.ProjectName.Contains(DefaultDate()) && a.ProjectName.Contains(defaultName));
                if (list.Count() > 0)
                {
                    var projectcount = new List<Int64>();
                    foreach (var item in list)
                    {
                        string input = item.ProjectName.Replace(DefaultDate(), "");
                        bool containsNum = System.Text.RegularExpressions.Regex.IsMatch(input, @"\d");
                        if (containsNum)
                        {
                            System.Text.RegularExpressions.Regex digitsOnly = new System.Text.RegularExpressions.Regex(@"[^\d]");
                            var result = digitsOnly.Replace(input, "");

                            projectcount.Add(Convert.ToInt64(result));
                        }
                    }
                    count = projectcount.Max();
                }
                
            }
            catch(Exception ex)
            {

            }
            return defaultName + "-" + (count + 1) + "_" + DefaultDate();
        }







        public List<Tuple<string, string>> GetClientInfo()
        {
            return _projectInfoDAL.GetClientInfo();
        }
        public List<Tuple<string, string>> GetClientInfoList(string typeText)
        {
            return _projectInfoDAL.GetClientInfoList(typeText);
        }
        public List<Tuple<string, string>> GetCreatorInfo()
        {
            return _projectInfoDAL.GetCreatorInfo();
        }
        public int InsertCreatorInfo(Creator ObjCreator)
        {
            return _projectInfoDAL.InsertCreatorInfo(ObjCreator);
        }
        public int UpdateCreatorInfo(Creator ObjCreator)
        {
            return _projectInfoDAL.UpdateCreatorInfo(ObjCreator);
        }
        public int InsertClientInfo(JCHVRF.Model.New.Client ObjClient)
        {
            return _projectInfoDAL.InsertClientInfo(ObjClient);
        }
        public int UpdateClientInfo(JCHVRF.Model.New.Client ObjClient)
        {
            return _projectInfoDAL.UpdateClientInfo(ObjClient);
        }
        public JCHVRF.Entity.ProjectInfo GetProjectInfo(int projectId)
        {
            JCHVRF.Entity.ProjectInfo objProjectInfo = _projectInfoDAL.GetProjectDetailsById(projectId);
            return objProjectInfo;
        }

        public Client GetClientDetails(int ClientId)
        {
            Client objClient = _projectInfoDAL.GetClientDetails(ClientId);
            return objClient;
        }
        public Creator GetCreatorDetails(int CreatorId)
        {
            Creator objCreator = _projectInfoDAL.GetCreatorDetails(CreatorId);
            return objCreator;
        }
        private string DefaultDate()
        {
            DateTime today = DateTime.Now;
            string year = today.Year.ToString();
            string month = today.Month.ToString().Length == 1 ? "0" + today.Month.ToString() : today.Month.ToString();
            string day = today.Day.ToString().Length == 1 ? "0" + today.Day.ToString() : today.Day.ToString();
            return day + "/" + month + "/" + year;
        }
        public bool CreateProject(JCHVRF.Model.Project project)
        {

            bool isCreated = false;

            try
            {
                _projectInfoDAL.CreateProject(project);
                isCreated = true;
            }
            catch (Exception ex)
            {

                string exception = "Unhandled exception has occurred in your application. If you click Ok, the application will ignore this error" +
                    " and attempt to continue. If you click Cancel, the application will close immediately." + "\n" + ex.Message;
                var result = MessageBox.Show(exception, "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                {

                    return false;
                }
                else
                    isCreated = true;
            }
            return isCreated;
        }
        public bool UpdateProject(JCHVRF.Model.Project project)
        {
            bool isUpdated = false;

            try
            {
                isUpdated = _projectInfoDAL.updateProject(project);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isUpdated;
        }
        public int GetMaxProjectId()
        {

            return _projectInfoDAL.GetMaxProjectId();

        }
        public List<Tuple<string, string>> GetCountry()
        {
            return _projectInfoDAL.GetCountry();
        }
        public List<Tuple<string, string>> GetCity(int? CountryID)
        {
            return _projectInfoDAL.GetCity(CountryID);
        }
        public List<Tuple<string, string, string>> GetAllSystemType()
        {
            return _projectInfoDAL.GetAllSystemType();
        }

    }
}
