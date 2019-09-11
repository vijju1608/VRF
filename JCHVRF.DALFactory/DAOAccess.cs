//********************************************************************
// 文件名: DAOAccess.cs
// 描述: 定义 VRF 项目中的数据访问类
// 作者: clh
// 创建时间: 2016-1-29
// 修改历史: 
// 2016-2-17 增加GetConnRegion()方法与GetRegionData()方法，将Region单独处理
//********************************************************************

using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.Configuration;

namespace JCHVRF.DALFactory
{
    public class DAOAccess : IDataAccessObject
    {
        #region 数据库连接串 将access2013降级到access2003，提高速度 modify on 20160601 by Yunxiao Lin
        //string ConnectionString =
        //    //"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\\NVRF\\VRF.accdb;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
        //    "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\NVRF\\VRF.dat;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
        ////"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\NVRF\\VRF.mdb;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
        string VRFConn = ConfigurationManager.ConnectionStrings["VRFDB"].ConnectionString;
        string PerformanceCS =
            //"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\\NVRF\\VRFPS.accdb;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\NVRF\\VRFPS.dat;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
        //"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\NVRF\\VRFPS.mdb;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
        string PerformanceCS_Update = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\NVRF\\VRFPS_Update.dat;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;";
        #endregion


        public string GetConnString()
        {
            return VRFConn;
        }

        public string GetConnLoadIndex()
        {
            return PerformanceCS;
        }

        public string GetConnLoadIndexUpdate()
        {
            return PerformanceCS_Update;
        }

        /// <summary>
        /// 默认数据库
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string queryString)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection connection = new OleDbConnection(VRFConn))
            {
                try
                {
                    OleDbDataAdapter da = new OleDbDataAdapter(queryString, VRFConn);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return dt;
        }

        /// <summary>
        /// 默认数据库
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string queryString, params OleDbParameter[] cmdParms)
        {
            using (OleDbConnection conn = new OleDbConnection(VRFConn))
            {
                OleDbCommand cmd = new OleDbCommand();
                PrepareCommand(cmd, conn, null, queryString, cmdParms);
                using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        da.Fill(dt);
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return dt;
                }
            }
        }

        /// 获取Load指标，另一数据库
        /// <summary>
        /// 获取Load指标，另一数据库
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public DataTable GetLoadIndex(string queryString)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection connection = new OleDbConnection(PerformanceCS))
            {
                OleDbDataAdapter da = new OleDbDataAdapter(queryString, PerformanceCS);
                da.Fill(dt);
            }
            return dt;
        }

        /// Region数据库的数据
        /// <summary>
        /// 获取Region数据库的数据，Region表现已转移至主数据库
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public DataTable GetRegionData(string queryString)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(VRFConn))
            {
                OleDbDataAdapter da = new OleDbDataAdapter(queryString, VRFConn);
                da.Fill(dt);
            }
            return dt;
        }

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, string ConnectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.OleDb.OleDbException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public void ExecuteSqlTran(ArrayList SQLStringList, string ConnectionString)
        {
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;
                OleDbTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (System.Data.OleDb.OleDbException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, string content, string ConnectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                OleDbCommand cmd = new OleDbCommand(SQLString, connection);
                System.Data.OleDb.OleDbParameter myParameter = new System.Data.OleDb.OleDbParameter("@content", OleDbType.VarChar);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.OleDb.OleDbException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSqlInsertImg(string strSQL, byte[] fs, string ConnectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                OleDbCommand cmd = new OleDbCommand(strSQL, connection);
                System.Data.OleDb.OleDbParameter myParameter = new System.Data.OleDb.OleDbParameter("@fs", OleDbType.Binary);
                myParameter.Value = fs;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.OleDb.OleDbException E)
                {
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, string ConnectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.OleDb.OleDbException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回OleDbDataReader
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OleDbDataReader</returns>
        public OleDbDataReader ExecuteReader(string strSQL, string ConnectionString)
        {
            OleDbConnection connection = new OleDbConnection(ConnectionString);
            OleDbCommand cmd = new OleDbCommand(strSQL, connection);
            try
            {
                connection.Open();
                OleDbDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (System.Data.OleDb.OleDbException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, string ConnectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OleDbDataAdapter command = new OleDbDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.OleDb.OleDbException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds;
            }
        }

        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString, params OleDbParameter[] cmdParms)
        {
            using (OleDbConnection connection = new OleDbConnection(VRFConn))
            {
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.OleDb.OleDbException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的OleDbParameter[]）</param>
        public void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (OleDbConnection conn = new OleDbConnection(VRFConn))
            {
                conn.Open();
                using (OleDbTransaction trans = conn.BeginTransaction())
                {
                    OleDbCommand cmd = new OleDbCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            OleDbParameter[] cmdParms = (OleDbParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, params OleDbParameter[] cmdParms)
        {
            using (OleDbConnection connection = new OleDbConnection(VRFConn))
            {
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.OleDb.OleDbException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString, string Conn, params OleDbParameter[] cmdParms)
        {
            using (OleDbConnection connection = new OleDbConnection(Conn))
            {
                using (OleDbCommand cmd = new OleDbCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.OleDb.OleDbException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回OleDbDataReader
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OleDbDataReader</returns>
        public OleDbDataReader ExecuteReader(string SQLString, params OleDbParameter[] cmdParms)
        {
            OleDbConnection connection = new OleDbConnection(VRFConn);
            OleDbCommand cmd = new OleDbCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                OleDbDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.OleDb.OleDbException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string SQLString, params OleDbParameter[] cmdParms)
        {
            using (OleDbConnection connection = new OleDbConnection(VRFConn))
            {
                OleDbCommand cmd = new OleDbCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }

        private void PrepareCommand(OleDbCommand cmd, OleDbConnection conn, OleDbTransaction trans,
            string cmdText, OleDbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (OleDbParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public void ExecuteSqlTran(System.Collections.Generic.List<dynamic> SQLStringList, string ConnectionString)
        {
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;
                OleDbTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].sql;
                        if (strsql.Trim().Length > 1)
                        {
                            PrepareCommand(cmd, conn, tx, strsql, SQLStringList[n].cmdParms);
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }
                    tx.Commit();
                }
                catch (System.Data.OleDb.OleDbException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
            }
        }

        #endregion

    }
}
