using JCHVRF.Model.New;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace JCHVRF.DAL.New
{
    public class DalProjectInfo
    {       
        string SqlString;
        string connectionString = ConfigurationManager.ConnectionStrings["ProjectDB"].ConnectionString;
        
        public int InsertProjectInfoDb(Project objProjectInfoBlob)
        {
            int val = 0;

          
            SqlString = "INSERT into ProjectInfo (SystemID,ProjectID,ProjectName,ActiveFlag,LastUpdateDate,Version,DBVersion,Measure,Location,SoldTo,ShipTo,OrderNo,ContractNo,Region,Office,Engineer,YINO,DeliveryDate,OrderDate,Remarks,ProjectType,Vendor,ProjectBlob,SystemBlob,SQBlob) Values(@SystemID, @ProjectID,@ProjectName,@ActiveFlag,@LastUpdateDate,@Version,@DBVersion,@Measure,@Location,@SoldTo,@ShipTo,@OrderNo,@ContractNo,@Region,@Office, @Engineer, @YINO,@DeliveryDate, @OrderDate,@Remarks,@ProjectType, @Vendor, @ProjectBlob,@SystemBlob,@SqBlob)";

            byte[] ProjectBlob = null;
            MemoryStream objstream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(objstream, objProjectInfoBlob);
            ProjectBlob = objstream.ToArray();

            JCHVRF.Model.New.Project objProjectInfo = new JCHVRF.Model.New.Project();

            #region SaveDataInDb

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))

                {
                    using (OleDbCommand cmd = new OleDbCommand(SqlString, conn))

                    {
                        conn.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@SystemID", objProjectInfoBlob.objProjectInfo.SystemID);
                        cmd.Parameters.AddWithValue("@ProjectID", 10);
                        cmd.Parameters.AddWithValue("@ProjectName", objProjectInfoBlob.objProjectInfo.ProjectName);
                        cmd.Parameters.AddWithValue("@ActiveFlag", 1);
                        cmd.Parameters.AddWithValue("@LastUpdateDate",DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@Version", objProjectInfoBlob.objProjectInfo.Version);
                        cmd.Parameters.AddWithValue("@DBVersion", objProjectInfoBlob.objProjectInfo.DBVersion);
                        cmd.Parameters.AddWithValue("@Measure", objProjectInfoBlob.objProjectInfo.Measure);
                        cmd.Parameters.AddWithValue("@Location", objProjectInfoBlob.objProjectInfo.Location);
                        cmd.Parameters.AddWithValue("@SoldTo", objProjectInfoBlob.objProjectInfo.SoldTo);
                        cmd.Parameters.AddWithValue("@ShipTo", objProjectInfoBlob.objProjectInfo.ShipTo);
                        cmd.Parameters.AddWithValue("@OrderNo", objProjectInfoBlob.objProjectInfo.OrderNo);
                        cmd.Parameters.AddWithValue("@ContractNo", objProjectInfoBlob.objProjectInfo.ContractNo);
                        cmd.Parameters.AddWithValue("@Region", objProjectInfoBlob.objProjectInfo.Region);
                        cmd.Parameters.AddWithValue("@Office", objProjectInfoBlob.objProjectInfo.Office);
                        cmd.Parameters.AddWithValue("@Engineer", objProjectInfoBlob.objProjectInfo.Engineer);
                        cmd.Parameters.AddWithValue("@YINO", objProjectInfoBlob.objProjectInfo.YINo);
                        cmd.Parameters.AddWithValue("@DeliveryDate",DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@Remarks", objProjectInfoBlob.objProjectInfo.Remarks);
                        cmd.Parameters.AddWithValue("@ProjectType", objProjectInfoBlob.objProjectInfo.ProjectType);
                        cmd.Parameters.AddWithValue("@Vendor", objProjectInfoBlob.objProjectInfo.Vendor);
                        cmd.Parameters.AddWithValue("@ProjectBlob", ProjectBlob);
                        cmd.Parameters.AddWithValue("@SystemBlob",DbType.Binary);
                        cmd.Parameters.AddWithValue("@SqBlob",DbType.Binary);
                        val = cmd.ExecuteNonQuery();
                        conn.Close();


                    }

                }
            }


            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return val;

        }

        #endregion SaveDataInDb

        public int InsertNewClient(Client objClientInfo)
        {
            int val = 0;
            connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\Project.mdb;Persist Security Info=False;Jet OLEDB:Database Password=YqJz2010Co04Ir15Kf;";
            SqlString = "INSERT into ClientInfo (COMPANYNAME, CONTACTNAME,STREETADDRESS,PHONE,CONTACTEMAIL,TOWNCITY,IDNUMBER,COUNTRY) Values(@COMPANYNAME,@CONTACTNAME,@STREETADDRESS,@PHONE,@CONTACTEMAIL,@TOWNCITY,@IDNUMBER,@COUNTRY)";

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    using (OleDbCommand cmd = new OleDbCommand(SqlString, conn))

                    {
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@COMPANYNAME", objClientInfo.CompanyName);
                        cmd.Parameters.AddWithValue("@CONTACTNAME", objClientInfo.ContactEmail);
                        cmd.Parameters.AddWithValue("@STREETADDRESS", objClientInfo.ContactName);
                        cmd.Parameters.AddWithValue("@PHONE", objClientInfo.Country);
                        cmd.Parameters.AddWithValue("@CONTACTEMAIL", objClientInfo.IdNumber);
                        cmd.Parameters.AddWithValue("@TOWNCITY", objClientInfo.Phone);
                        cmd.Parameters.AddWithValue("@IDNUMBER", objClientInfo.StreetAddress);
                        cmd.Parameters.AddWithValue("@COUNTRY", objClientInfo.Suburb);
                        conn.Open();

                        val = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return val;
        }



    }
}



