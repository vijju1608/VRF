using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.OleDb;
using System.Collections;

namespace JCHVRF.DALFactory
{
    public interface IDataAccessObject
    {
        string GetConnString();
        string GetConnLoadIndex();
        string GetConnLoadIndexUpdate();

        DataTable GetDataTable(string queryString);
        DataTable GetDataTable(string queryString, params OleDbParameter[] cmdParms);
        DataTable GetLoadIndex(string queryString);
        DataTable GetRegionData(string queryString);


        int ExecuteSql(string SQLString, string ConnectionString);
        void ExecuteSqlTran(ArrayList SQLStringList, string ConnectionString);
        int ExecuteSql(string SQLString, string content, string ConnectionString);
        int ExecuteSqlInsertImg(string strSQL, byte[] fs, string ConnectionString);
        object GetSingle(string SQLString, string ConnectionString);
        OleDbDataReader ExecuteReader(string strSQL, string ConnectionString);
        DataSet Query(string SQLString, string ConnectionString);


        int ExecuteSql(string SQLString, params OleDbParameter[] cmdParms);
        void ExecuteSqlTran(Hashtable SQLStringList);
        object GetSingle(string SQLString, params OleDbParameter[] cmdParms);
        object GetSingle(string SQLString, string Conn, params OleDbParameter[] cmdParms);
        OleDbDataReader ExecuteReader(string SQLString, params OleDbParameter[] cmdParms);
        DataSet Query(string SQLString, params OleDbParameter[] cmdParms);
        void ExecuteSqlTran(System.Collections.Generic.List<dynamic> SQLStringList, string ConnectionString);

    }
}
