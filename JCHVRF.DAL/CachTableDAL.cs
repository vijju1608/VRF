using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCHVRF.DALFactory;
using System.Data;
using System.Data.OleDb;
using JCHVRF.Model;
using System.Threading;
using System.Threading.Tasks;

namespace JCHVRF.DAL
{
    public class CachTableDAL
    {
        IDataAccessObject _dao;
        public CachTableDAL()
        {
            _dao = (new GetDatabase()).GetDataAccessObject();
        }

        public void CreateCachData()
        {
            DataSet ds = JCBase.Utility.Util.DsCach;
            using (OleDbConnection conn = new OleDbConnection(_dao.GetConnString()))
            {
                conn.Open();
                DataTable tb = conn.GetSchema("Tables");
                Parallel.ForEach(tb.AsEnumerable(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, row =>
                 {
                     string tableName = row["table_name"].ToString();
                     if (!ds.Tables.Contains(tableName))
                     {
                         if (tableName.IndexOf("MSys") == -1 && tableName.IndexOf("~TMP") == -1)
                         {
                             DataTable t = _dao.GetDataTable("select * from " + tableName);
                             t.TableName = tableName;
                             ds.Tables.Add(t);
                         }
                     }

                 });
                //foreach (DataRow row in tb.Rows)
                //{
                //    string tableName = row["table_name"].ToString();
                //    if (tableName.IndexOf("MSys") == -1 && tableName.IndexOf("~TMP") == -1)
                //    {
                //        DataTable t = _dao.GetDataTable("select * from " + tableName);
                //        t.TableName = tableName;
                //        ds.Tables.Add(t);
                //    }
                //}
            }

            if (!ds.Tables.Contains("T_PipingKitTable") || ds.Tables["T_PipingKitTable"] == null)
            {
                DataTable tb = new DataTable();
                tb.Columns.Add("Type");
                tb.Columns.Add("System");
                tb.Columns.Add("Model");
                tb.Columns.Add("Qty");
                tb.Columns.Add("Id");
                tb.TableName = "T_PipingKitTable";
                ds.Tables.Add(tb);

                //DataRow row = tb.NewRow();
                //row["Type"] = "PipingConnectionKit";
                //row["System"] = "H1";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);

                //row = tb.NewRow();
                //row["Type"] = "PipingConnectionKit";
                //row["System"] = "H1";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);

                //row = tb.NewRow();
                //row["Type"] = "BranchKit";
                //row["System"] = "H2";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);


                //row = tb.NewRow();
                //row["Type"] = "BranchKit";
                //row["System"] = "H2";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);



                //row = tb.NewRow();
                //row["Type"] = "BranchKit";
                //row["System"] = "H2";
                //row["Model"] = "H2";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);


                //row = tb.NewRow();
                //row["Type"] = "BranchKit";
                //row["System"] = "H2";
                //row["Model"] = "H2";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);


                //row = tb.NewRow();
                //row["Type"] = "CHBox";
                //row["System"] = "H2";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);


                //row = tb.NewRow();
                //row["Type"] = "CHBox";
                //row["System"] = "H2";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);



                //row = tb.NewRow();
                //row["Type"] = "CHBox";
                //row["System"] = "H2";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);


                //row = tb.NewRow();
                //row["Type"] = "CHBox";
                //row["System"] = "H2";
                //row["Model"] = "H1";
                //row["Qty"] = "0";
                //tb.Rows.Add(row);

            }
        }

    }
}
