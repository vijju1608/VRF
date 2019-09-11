using JCHVRF.DALFactory.New;
using JCHVRF.Model.New;
using System;
using System.Data;
using System.Data.OleDb;

namespace JCHVRF.DAL.New
{
    public  class DalProjectLocation
    {

        string connectionString;
        string SqlString;

        public int InsertLocationDb(ProjectLocation objprojloc)
        {
            int val = 0;
            connectionString = Connection.GetConnectionString();
            SqlString = "INSERT into tblLocation (Region, SubRegion,GpsCoordinate) Values(@Region, @SubRegion,@GpsCoordinate)";

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))

                {
                    using (OleDbCommand cmd = new OleDbCommand(SqlString, conn))

                    {
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@Region", objprojloc.Region);

                        cmd.Parameters.AddWithValue("@SubRegion", objprojloc.SubRegion);
                        cmd.Parameters.AddWithValue("@GpsCoordinate", objprojloc.GpsCoordinate);

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

        public DataTable Read()
        {
            connectionString = Connection.GetConnection();
            string Sql = "select * from JCHVRF_Region where trim(ParentRegionCode)='" + "' or ParentRegionCode is null and Code <> 'Super' and DeleteFlag = 1";//

            using (OleDbConnection conn = new OleDbConnection(connectionString))

            {
                using (OleDbCommand cmd = new OleDbCommand(Sql, conn))
                {
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }

        }


        public DataTable ReadDataSubRegion(string pCode)
        {

            connectionString = Connection.GetConnection();
            string Sql = "select * from JCHVRF_Region where trim(ParentRegionCode)='" + pCode + "'";

            using (OleDbConnection conn = new OleDbConnection(connectionString))

            {
                using (OleDbCommand cmd = new OleDbCommand(Sql, conn))
                {
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }

        }
    }
}
