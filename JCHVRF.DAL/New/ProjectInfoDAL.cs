using Dapper;
using JCHVRF.Entity;
using JCHVRF.Model;
using JCHVRF_New.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace JCHVRF.DAL.New
{
    public interface IProjectInfoDAL
    {
        List<ProjectInfo> GetAllProjectInfo();
        List<ProjectInfo> GetAllProjectsRegionWise(string SearchType, string region, string subReion);
        ProjectInfo GetProjectDetailsById(int projectID);
        JCHVRF.Model.New.Client GetClientDetails(int ClientId);
        JCHVRF.Model.New.Creator GetCreatorDetails(int CreatorId);
        bool UpdateProjectOffline(ProjectInfo projectInfo);
        bool DeleteProject(ProjectInfo projectInfo);
        int CreateProject(Project project);
        bool updateProject(JCHVRF.Model.Project project);
        int GetMaxProjectId();
        int GetNumberOfProject();
        List<Tuple<string, string>> GetClientInfoList(string typeText);
        List<Tuple<string, string>> GetCountry();
        List<Tuple<string, string>> GetCity(int? CountryID);
        List<Tuple<string, string>> GetClientInfo();
        List<Tuple<string, string>> GetCreatorInfo();
        int InsertCreatorInfo(Model.New.Creator ObjCreator);
        int UpdateCreatorInfo(Model.New.Creator ObjCreator);
        int InsertClientInfo(JCHVRF.Model.New.Client objClient);
        int UpdateClientInfo(JCHVRF.Model.New.Client objClient);
        List<Tuple<string, string, string>> GetAllSystemType();
        }
    public class ProjectInfoDAL : IProjectInfoDAL
    {
        string projectDB = ConfigurationManager.ConnectionStrings["ProjectDB"].ConnectionString;
        public string Region { get; set; }
        public List<ProjectInfo> GetAllProjectsRegionWise(string SearchType, string Region, string SubRegion = "")
        {
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                List<ProjectInfo> projects = new List<ProjectInfo>();
                string SearchQuery = string.Empty;
                SearchType = string.IsNullOrEmpty(SearchType) ? "ALL" : SearchType;
                if (SearchType.Equals("All", StringComparison.OrdinalIgnoreCase) || SearchType.Equals(String.Empty))
                {
                    SearchQuery = "1=1 ";
                    projects = db.Query<ProjectInfo>
                ("Select * From ProjectInfo WHERE " + SearchQuery + " and Region = @Region and SubRegion = @SubRegion", new { Region, SubRegion }).ToList();
                }
                else if (SearchType.Equals("week", StringComparison.OrdinalIgnoreCase))
                {
                    List<ProjectInfo> WeekProject = new List<ProjectInfo>();
                    var weekDay = (int)DateTime.UtcNow.DayOfWeek;
                    var weekStartDate = DateTime.UtcNow.Date.AddDays(-weekDay + 1).Date;
                    var weekEndDate = DateTime.UtcNow.Date.AddDays(7 - weekDay).Date;
                    WeekProject = db.Query<ProjectInfo>
                ("Select * From ProjectInfo WHERE Region = @Region and SubRegion = @SubRegion", new { Region, SubRegion }).ToList();
                    projects = WeekProject.Where(x => x.DeliveryDate >= weekStartDate && x.DeliveryDate <= weekEndDate).ToList();
                }
                else if (SearchType.Equals("month", StringComparison.OrdinalIgnoreCase))
                {
                    List<ProjectInfo> MonthProject = new List<ProjectInfo>();
                    MonthProject = db.Query<ProjectInfo>
                ("Select * From ProjectInfo WHERE Region = @Region and SubRegion = @SubRegion", new { Region, SubRegion }).ToList();
                    projects = MonthProject.Where(x => x.DeliveryDate.Month == DateTime.UtcNow.Month && x.DeliveryDate.Year == DateTime.UtcNow.Year).ToList();
                }
                else
                {
                    SearchQuery = "ProjectName  like '%/" + SearchType + "/%' ";
                    projects = db.Query<ProjectInfo>
                ("Select * From ProjectInfo WHERE " + SearchQuery + " and Region = @Region and SubRegion = @SubRegion", new { Region, SubRegion }).ToList();
                }
                return projects;
            }
        }
        public List<ProjectInfo> GetAllProjectInfo()
        {
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                return db.Query<ProjectInfo>
                ("Select * From ProjectInfo").ToList();
            }
        }
        public int GetNumberOfProject()
        {
            int projectCount = 0;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                projectCount = db.Query<int>
                ("Select ID From ProjectInfo").Count();
                return projectCount;
            }
        }

       public List<Tuple<string, string>> GetClientInfo()
        {
            List<Tuple<string, string>> listClient = null;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                var Listpro = db.Query<Model.New.Creator>
                   ("Select ID,ContactName From Client").ToList();
                listClient = new List<Tuple<string, string>>();
                Listpro.ForEach((item) =>
                {
                    listClient.Add(Tuple.Create(Convert.ToString(item.Id), item.ContactName));
                });
            }
            return listClient;
        }
        public int InsertCreatorInfo(Model.New.Creator ObjCreator)
        {
            using (IDbConnection conn = new OleDbConnection(projectDB))
            {
                string processQuery = @"INSERT INTO Creator
                                          (CompanyName,ContactName,StreetAddress,Suburb,Town,Country,GPSPosition,Phone,ContactEmail,IdNumber) 
                                          VALUES 
                                          (@CompanyName,@ContactName,@StreetAddress,@Suburb,@Town,@Country,@GPSPosition,@Phone,@ContactEmail,@IdNumber)";
                int isInserted = conn.Execute(processQuery, new
                {
                    CompanyName = ObjCreator.CompanyName,
                    ContactName = ObjCreator.ContactName,
                    StreetAddress = ObjCreator.StreetAddress,
                    Suburb = ObjCreator.Suburb,
                    Town = ObjCreator.TownCity,
                    Country = ObjCreator.Country,
                    GPSPosition = ObjCreator.GpsPosition,
                    Phone = ObjCreator.Phone,
                    ContactEmail = ObjCreator.ContactEmail,
                    IdNumber = ObjCreator.IdNumber
                });
                return isInserted;
            }

        }
        public int UpdateCreatorInfo(Model.New.Creator ObjCreator)
        {
            using (IDbConnection conn = new OleDbConnection(projectDB))
            {
              
                string processQuery = @"Update Creator set  
                                                CompanyName=@CompanyName,
                                                ContactName=@ContactName,
                                                StreetAddress=@StreetAddress,
                                                Suburb=@Suburb,
                                                Town=@Town,
                                                Country=@Country,
                                                GPSPosition=@GPSPosition,
                                                Phone=@Phone,
                                                ContactEmail=@ContactEmail,
                                                IdNumber=@IdNumber
                                                Where ID=@ID ";

                int isRowUpdated = conn.Execute(processQuery, new
                {
                    CompanyName = ObjCreator.CompanyName,
                    ContactName = ObjCreator.ContactName,
                    StreetAddress = ObjCreator.StreetAddress,
                    Suburb = ObjCreator.Suburb,
                    Town = ObjCreator.TownCity,
                    Country = ObjCreator.Country,
                    GPSPosition = ObjCreator.GpsPosition,
                    Phone = ObjCreator.Phone,
                    ContactEmail = ObjCreator.ContactEmail,
                    IdNumber = ObjCreator.IdNumber,
                    Id = ObjCreator.Id
                });
                return isRowUpdated;
            }

        }
        public int InsertClientInfo(JCHVRF.Model.New.Client ObjClient)
        {
            using (IDbConnection conn = new OleDbConnection(projectDB))
            {
                string processQuery = @"INSERT INTO Client
                                          (CompanyName,ContactName,StreetAddress,Suburb,Town,Country,GPSPosition,Phone,ContactEmail,IdNumber) 
                                          VALUES 
                                          (@CompanyName,@ContactName,@StreetAddress,@Suburb,@Town,@Country,@GPSPosition,@Phone,@ContactEmail,@IdNumber)";
                int isInserted = conn.Execute(processQuery, new
                {
                    CompanyName = ObjClient.CompanyName,
                    ContactName = ObjClient.ContactName,
                    StreetAddress = ObjClient.StreetAddress,
                    Suburb = ObjClient.Suburb,
                    Town = ObjClient.TownCity,
                    Country = ObjClient.Country,
                    GPSPosition = ObjClient.GpsPosition,
                    Phone = ObjClient.Phone,
                    ContactEmail = ObjClient.ContactEmail,
                    IdNumber = ObjClient.IdNumber,
                    
                });
                return isInserted;
            }

        }
       
        public int UpdateClientInfo(JCHVRF.Model.New.Client ObjClient)
        {
            using (IDbConnection conn = new OleDbConnection(projectDB))
            {
                
                string processQuery = @"Update Client set  
                                                CompanyName=@CompanyName,
                                                ContactName=@ContactName,
                                                StreetAddress=@StreetAddress,
                                                Suburb=@Suburb,
                                                Town=@Town,
                                                Country=@Country,
                                                GPSPosition=@GPSPosition,
                                                Phone=@Phone,
                                                ContactEmail=@ContactEmail,
                                                IdNumber=@IdNumber
                                                Where ID=@ID ";


                int rowUpdated = conn.Execute(processQuery, new
                {
                    CompanyName = ObjClient.CompanyName,
                    ContactName = ObjClient.ContactName,
                    StreetAddress = ObjClient.StreetAddress,
                    Suburb = ObjClient.Suburb,
                    Town = ObjClient.TownCity,
                    Country = ObjClient.Country,
                    GPSPosition = ObjClient.GpsPosition,
                    Phone = ObjClient.Phone,
                    ContactEmail = ObjClient.ContactEmail,
                    IdNumber = ObjClient.IdNumber,
                    Id=ObjClient.Id
                });
                return rowUpdated;
            }

        }
        public List<Tuple<string, string>> GetCreatorInfo()
        {
            List<Tuple<string, string>> listCreator = null;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                var Listpro = db.Query<Model.New.Creator>
                  ("Select ID,ContactName From Creator").ToList();
                listCreator = new List<Tuple<string, string>>();
                Listpro.ForEach((item) =>
                {
                    listCreator.Add(Tuple.Create(Convert.ToString(item.Id), item.ContactName));
                });
            }
            return listCreator;
        }
        public List<Tuple<string, string>> GetClientInfoList(string typeText)
        {
            List<Tuple<string, string>> listClient = null;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                if (!string.IsNullOrEmpty(typeText))
                {
                    var sqlstr = "Select ID,ContactName From Client where ContactName like '" + typeText + "%'";
                    var Listpro = db.Query<JCHVRF.Model.New.Client>(sqlstr).ToList();
                    listClient = new List<Tuple<string, string>>();
                    Listpro.ForEach((item) =>
                    {
                        listClient.Add(Tuple.Create(Convert.ToString(item.Id), item.ContactName));
                    });
                }
            }
            return listClient;
        }
        /// <summary>
        /// To get Details of ProjectInfo
        /// </summary>
        /// <returns></returns>
        public ProjectInfo GetProjectDetailsById(int projectID)
        {
            ProjectInfo projectInfo;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                projectInfo = db.Query<ProjectInfo>("Select * From ProjectInfo WHERE projectID = @projectID", new { projectID }).FirstOrDefault();
            }
            if (projectInfo!=null && projectInfo.ProjectBlob != null)
            {
                projectInfo.ProjectLegacy = Utility.Deserialize<JCHVRF.Model.Project>(projectInfo.ProjectBlob);
            }
            return projectInfo;
        }

        public Model.New.Client GetClientDetails(int ClientId)
        {
            JCHVRF.Model.New.Client clientInfo;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                clientInfo = db.Query<JCHVRF.Model.New.Client>("Select * From ProjectInfo inner join Client on ProjectInfo.ClientId=Client.Id WHERE ProjectInfo.ClientId = @ClientId", new { ClientId }).FirstOrDefault();
            }
            return clientInfo;
        }
        public Model.New.Creator GetCreatorDetails(int CreatorId)
        {
            JCHVRF.Model.New.Creator creatorInfo;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                creatorInfo = db.Query<JCHVRF.Model.New.Creator>("Select * From ProjectInfo inner join Creator on ProjectInfo.CreatorId=Creator.Id WHERE ProjectInfo.CreatorId = @CreatorId", new { CreatorId }).FirstOrDefault();
            }
            return creatorInfo;
        }
        /// <summary>
        /// To Update a Project Data
        /// </summary>
        /// <returns></returns>
        public bool UpdateProjectOffline(ProjectInfo projectInfo)
        {
            bool isUpdated = false;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                string sqlQuery = "UPDATE ProjectInfo SET ProjectName = @ProjectName " + "WHERE ProjectID = @ProjectID";
                int rowsAffected = db.Execute(sqlQuery, projectInfo);
                if (rowsAffected > 0)
                {
                    isUpdated = true;
                }
            }
            return isUpdated;
        }
        /// <summary>
        /// To Delete a Project
        /// </summary>
        /// <param name="projectInfo"></param>
        /// <returns></returns>
        public bool DeleteProject(ProjectInfo projectInfo)
        {
            bool isDeleted = false;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                string sqlQuery = "UPDATE ProjectInfo SET ProjectName = @ProjectName " + "WHERE ProjectID = @ProjectID";
                int rowsAffected = db.Execute(sqlQuery, projectInfo);
                if (rowsAffected > 0)
                {
                    isDeleted = true;
                }
            }
            return isDeleted;
        }
        /// <summary>
        /// To Create a new Project Record in Old Existing Project Table
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public int CreateProject(Project project)
        {
          
            using (IDbConnection conn = new OleDbConnection(projectDB))
            {
                int newProjectID = 0;
                var availableProjects = conn.Query<int>("SELECT ProjectID from ProjectInfo");
                if (availableProjects.Count() < 1)
                {
                    newProjectID = 1;
                }
                else
                {
                    newProjectID = conn.Query<int>("SELECT MAX(ProjectID) from ProjectInfo").Single() + 1;
                }

                string processQuery = @"INSERT INTO ProjectInfo
                                          (SystemID,ProjectID,ProjectName,ActiveFlag,LastUpdateDate,Version,DBVersion,Measure,Location,SoldTo,ShipTo,OrderNo,ContractNo,Region,Office,Engineer,YINO,DeliveryDate,OrderDate,Remarks,ProjectType,Vendor,ProjectBlob,SystemBlob,SQBlob,ClientId,CreatorId,SubRegion) 
                                          VALUES 
                                          (@SystemID,@ProjectID,@ProjectName,@ActiveFlag,@LastUpdateDate,@Version,@DBVersion,@Measure,@Location,@SoldTo,@ShipTo,@OrderNo,@ContractNo,@Region,@Office,@Engineer,@YINO,@DeliveryDate,@OrderDate,@Remarks,@ProjectType,@Vendor,@ProjectBlob,@SystemBlob,@SQBlob,@ClientId,@CreatorId,@SubRegionCode)";

                //To Serialize ProjectBlob Data
                project.projectID = newProjectID;
                byte[] projectBlob = Utility.Serialize(project);
                //To Serialize SystemBlob Data
                int isInserted = conn.Execute(processQuery, new
                {
                    SystemID = Convert.ToString(999),
                    ProjectID = newProjectID,
                    ProjectName = project.Name,
                    ActiveFlag = Convert.ToInt32(1),
                    LastUpdateDate = Convert.ToString(project.UpdateDate),
                    Version = project.Version,
                    DBVersion = ConfigurationManager.AppSettings["DBVersion"],
                    Measure = 0,//ToDo
                    Location = string.IsNullOrEmpty(project.Location) ? string.Empty : project.Location,
                    SoldTo = string.IsNullOrEmpty(project.SoldTo) ? string.Empty : project.SoldTo,
                    ShipTo = string.IsNullOrEmpty(project.ShipTo) ? string.Empty : project.ShipTo,
                    OrderNo = string.IsNullOrEmpty(project.PurchaseOrderNO) ? string.Empty : project.PurchaseOrderNO,
                    ContractNo = string.IsNullOrEmpty(project.ContractNO) ? string.Empty : project.ContractNO,
                    Region = string.IsNullOrEmpty(project.RegionCode) ? string.Empty : project.RegionCode,
                    Office = string.IsNullOrEmpty(project.SalesOffice) ? string.Empty : project.SalesOffice,
                    Engineer = string.IsNullOrEmpty(project.SalesEngineer) ? string.Empty : project.SalesEngineer,
                    YINO = string.IsNullOrEmpty(project.SalesYINO) ? string.Empty : project.SalesYINO,
                    DeliveryDate = Convert.ToString(project.DeliveryRequiredDate),
                    OrderDate = Convert.ToString(project.OrderDate),
                    Remarks = string.IsNullOrEmpty(project.Remarks) ? string.Empty : project.Remarks,
                    ProjectType = "",// Blank as per the existing system
                    Vendor = string.IsNullOrEmpty(project.salesCompany) ? string.Empty : project.salesCompany,
                    ProjectBlob = projectBlob,
                    //ToDo
                    SystemBlob = projectBlob,
                    SQBlob = projectBlob,
                    ClientId = project.ClientId,
                    CreatorId=project.CreatorId,
                    SubRegionCode = project.SubRegionCode

                });
                //var projectID = conn.Query<int>("SELECT MAX(ProjectID) from ProjectInfo").Single();
                //return isInserted;
                return isInserted;
            }
        }
        public bool updateProject(JCHVRF.Model.Project project)
        {
            bool isUpdated = false;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                byte[] projectBlob = Utility.Serialize(project);
                string processQuery = @"Update ProjectInfo set  
                                                SystemID=@SystemID,
                                                ProjectName=@ProjectName,
                                                ActiveFlag=@ActiveFlag,
                                                LastUpdateDate=@LastUpdateDate,
                                                ShipTo=@ShipTo,
                                                DBVersion=@DBVersion,
                                                Location=@Location,
                                                SoldTo=@SoldTo,
                                                Version=@Version,
                                                OrderNo=@OrderNo,
                                                ContractNo=@ContractNo,
                                                Region=@Region,
                                                Office=@Office,
                                                Engineer=@Engineer,
                                                YINO=@YINO,
                                                DeliveryDate=@DeliveryDate,
                                                OrderDate=@OrderDate,
                                                Remarks=@Remarks,                                               
                                                Vendor=@Vendor,
                                                ProjectBlob=@ProjectBlob,
                                                SystemBlob=@ProjectBlob,
                                                SQBlob=@ProjectBlob
                                              Where ProjectID=@ProjectID ";

                int rowsAffected = db.Execute(processQuery, new
                {
                    SystemID = "999",
                    ProjectName = project.Name,
                    ActiveFlag = Convert.ToInt32(1),
                    LastUpdateDate = Convert.ToString(project.UpdateDate),
                    ShipTo = string.IsNullOrEmpty(project.ShipTo) ? string.Empty : project.ShipTo,
                    DBVersion = ConfigurationManager.AppSettings["DBVersion"],
                    Location = string.IsNullOrEmpty(project.Location) ? string.Empty : project.Location,
                    SoldTo = string.IsNullOrEmpty(project.SoldTo) ? string.Empty : project.SoldTo,
                    Version = string.IsNullOrEmpty(project.Version) ? string.Empty : project.Version,
                    OrderNo = string.IsNullOrEmpty(project.PurchaseOrderNO) ? string.Empty : project.PurchaseOrderNO,
                    ContractNo = string.IsNullOrEmpty(project.ContractNO) ? string.Empty : project.ContractNO,
                    Region = string.IsNullOrEmpty(project.RegionCode) ? string.Empty : project.RegionCode,
                    Office = string.IsNullOrEmpty(project.SalesOffice) ? string.Empty : project.SalesOffice,
                    Engineer = string.IsNullOrEmpty(project.SalesEngineer) ? string.Empty : project.SalesEngineer,
                    YINO = string.IsNullOrEmpty(project.SalesYINO) ? string.Empty : project.SalesYINO,
                    DeliveryDate = Convert.ToString(project.DeliveryRequiredDate),
                    OrderDate = Convert.ToString(project.OrderDate),
                    Remarks = string.IsNullOrEmpty(project.Remarks) ? string.Empty : project.Remarks,
                    Vendor = string.IsNullOrEmpty(project.salesCompany) ? string.Empty : project.salesCompany,
                    ProjectBlob = projectBlob,
                    //ToDo
                    SystemBlob = projectBlob,
                    SQBlob = projectBlob,
                    ProjectID = project.projectID
                });


                //string processQuery1 = "Update ProjectInfo set SystemID ='" + Convert.ToString(999) + "',ProjectName ='" + project.Name +
                //       "',ActiveFlag ='" + Convert.ToInt32(1) +
                //       "',LastUpdateDate ='" + project.UpdateDate +
                //       "',ShipTo = '" + project.ShipTo +
                //       "',DBVersion = '" + ConfigurationManager.AppSettings["DBVersion"] +
                //       "',Measure = '" + 0 + //ToDo
                //       "',Location = '" + project.Location +
                //       "',SoldTo = '" + project.SoldTo +
                //       "',Version = '" + project.Version +
                //       "',OrderNo = '" + project.PurchaseOrderNO +
                //       "',ContractNo = '" + project.ContractNO +
                //       "',Region = '" + project.RegionCode +
                //       "',Office = '" + project.SalesOffice +
                //       "',Engineer = '" + project.SalesEngineer +
                //       "',YINO = '" + project.SalesYINO +
                //       "',DeliveryDate = '" + project.DeliveryRequiredDate +
                //       "',OrderDate = '" + project.OrderDate +
                //       "',Remarks = '" + project.Remarks +
                //       "',ProjectType = '" + "" +
                //       "',Vendor = '" + project.salesCompany +
                //       "',SystemBlob = '" + projectBlob +
                //       "',SQBlob = '" + projectBlob + "' where ProjectID= " + project.projectID;

                // int rowsAffected = db.Execute(processQuery1);
                if (rowsAffected > 0)
                {
                    isUpdated = true;
                }
            }
            return isUpdated;

        }
        public int GetMaxProjectId()
        {
            int newProjectID = 0;
            using (IDbConnection conn = new OleDbConnection(projectDB))
            {
                var availableProjects = conn.Query<int>("SELECT ProjectID from ProjectInfo");
                if (availableProjects.Count() < 1)
                {
                    newProjectID = 1;
                }
                else
                {
                    newProjectID = conn.Query<int>("SELECT MAX(ProjectID) from ProjectInfo").Single();
                }
            }
            return newProjectID;
        }
        /// <summary>
        /// Get All country 
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string>> GetCountry()
        {
            List<Tuple<string, string>> listCountry = null;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                /// TODO Bind From Database.
                //List
                ////var Listpro = db.Query<ProjectInfo>
                ////  ("Select ID,ProjectName From ProjectInfo").ToList();
                //Listpro.ForEach((item) =>
                //{
                //    listClient.Add(Tuple.Create(item.ID, item.ProjectName));
                //});
                listCountry = new List<Tuple<string, string>>();
                listCountry.Add(Tuple.Create("0", "Select Country"));
                listCountry.Add(Tuple.Create("1", "India"));

            }
            return listCountry;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string>> GetCity(int? CountryID)
        {
            List<Tuple<string, string>> listCity = null;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                /// TODO Bind From Database.
                //List
                ////var Listpro = db.Query<ProjectInfo>
                ////  ("Select ID,ProjectName From ProjectInfo").ToList();
                //Listpro.ForEach((item) =>
                //{
                //    listClient.Add(Tuple.Create(item.ID, item.ProjectName));
                //});
                listCity = new List<Tuple<string, string>>();
                listCity.Add(Tuple.Create("0", "Select City"));
                listCity.Add(Tuple.Create("1", "Delhi"));

            }
            return listCity;
        }
        public List<Tuple<string, string, string>> GetAllSystemType()
        {
            List<Tuple<string, string, string>> listSystemType = null;
            using (IDbConnection db = new OleDbConnection(projectDB))
            {
                var Listpro = db.Query<SystemTypeList>
                  ("Select SystemID,Name,Path From SystemType where Enabled=1 order by Priority").ToList();
                listSystemType = new List<Tuple<string, string, string>>(); ;
                Listpro.ForEach((item) =>
                {
                    listSystemType.Add(Tuple.Create(item.SystemID, item.Name, item.Path));
                });
            }
            return listSystemType;
        }
    }
}
