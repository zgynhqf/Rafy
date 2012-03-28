using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;
using SimpleCsla.Data;
using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace OEA.Utils
{
    public static class DBHelper
    {
        //private static string _commonConnectionString;

        //private static string CommonConnectionString
        //{
        //    get
        //    {
        //        if (_commonConnectionString == null)
        //        {
        //            string conString = "BusinessDBName";
        //            var connectionString = Unity.Instance.Resolve<ICommonConnectionString>();
        //            if (connectionString != null)
        //            {
        //                conString = connectionString.Value;
        //            }
        //            else
        //            {
        //                //打断点用
        //            }

        //            _commonConnectionString = conString;
        //        }
        //        return _commonConnectionString;
        //    }
        //}

        //private static IDb CreateDb()
        //{
        //    return CreateDb(CommonConnectionString);
        //}

        public static IDb CreateDb(string connectionStringName)
        {
            return new DBProxy(connectionStringName);
        }

        /// <summary>
        /// 检测数据库是否可连接成功
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static bool CheckDBConnected(string connectionString)
        {
            bool connected = false;
            var connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                connected = true;
            }
            catch { }
            finally
            {
                connection.Close();
            }
            return connected;
        }
    }

    /// <summary>
    /// 重构数据访问代码。
    /// 整合LiteORM.IDb，CSLA.ConnectionManager。
    /// 这样可以：去除多余的Db构造代码，代码看上去较简洁，方便以后可能存在的扩展。
    /// </summary>
    internal class DBProxy : IDb
    {
        private string _connectionStringName;

        private ConnectionManager _connectionManager;

        private IDb _db;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionStringName">
        /// Config文件中的连接字符串名字
        /// </param>
        public DBProxy(string connectionStringName)
        {
            this._connectionStringName = connectionStringName;
        }

        private IDb Db
        {
            get
            {
                if (this._db == null)
                {
                    this._connectionManager = ConnectionManager.GetManager(this._connectionStringName);
                    this._db = DbFactory.Instance.GetDb(this._connectionManager.Connection);
                }
                return this._db;
            }
        }

        #region IDb Members

        public void Begin()
        {
            this.Db.Begin();
        }

        public object Call(string funcName, object[] parameters)
        {
            return this.Db.Call(funcName, parameters);
        }

        public void Commit()
        {
            this.Db.Commit();
        }

        public IDbConnection Connection
        {
            get { return this.Db.Connection; }
        }

        public int Delete<T>(IQuery query)
        {
            return this.Db.Delete<T>(query);
        }

        public int Delete(Type type, IQuery query)
        {
            return this.Db.Delete(type, query);
        }

        public int Delete(object item)
        {
            return this.Db.Delete(item);
        }

        public int Delete<T>(ICollection<T> items)
        {
            return this.Db.Delete<T>(items);
        }

        public int Delete(Type type, ICollection items)
        {
            return this.Db.Delete(type, items);
        }

        public IResultSet Exec(string procName, object[] parameters, int[] outputs)
        {
            return this.Db.Exec(procName, parameters, outputs);
        }

        public IResultSet Exec(string procName, object[] parameters)
        {
            return this.Db.Exec(procName, parameters);
        }

        public IList Exec(Type type, string procName, object[] parameters)
        {
            return this.Db.Exec(type, procName, parameters);
        }

        public T Find<T>(object key)
        {
            return this.Db.Find<T>(key);
        }

        public object Find(Type type, object key)
        {
            return this.Db.Find(type, key);
        }

        public ITable GetTable(Type type)
        {
            return this.Db.GetTable(type);
        }

        public int Insert(object item)
        {
            return this.Db.Insert(item);
        }

        public int Insert<T>(ICollection<T> items)
        {
            return this.Db.Insert<T>(items);
        }

        public int Insert(Type type, ICollection items)
        {
            return this.Db.Insert(type, items);
        }

        public bool IsAutoCommit
        {
            get
            {
                return this.Db.IsAutoCommit;
            }
        }

        public bool IsClosed
        {
            get
            {
                return this.Db.IsClosed;
            }
        }

        public IQuery Query(Type entityType)
        {
            return this.Db.Query(entityType);
        }

        public IQuery Query()
        {
            return this.Db.Query();
        }

        public void Rollback()
        {
            this.Db.Rollback();
        }

        public IList Select(IQuery typedQuery)
        {
            return this.Db.Select(typedQuery);
        }

        public IList<T> Select<T>(IQuery query)
        {
            return this.Db.Select<T>(query);
        }

        public IList Select(Type type, IQuery query)
        {
            return this.Db.Select(type, query);
        }

        public IDbTransaction Transaction
        {
            get
            {
                return this.Db.Transaction;
            }
        }

        public int Update(object item)
        {
            return this.Db.Update(item);
        }

        public int Update<T>(ICollection<T> items)
        {
            return this.Db.Update<T>(items);
        }

        public int Update(Type type, ICollection items)
        {
            return this.Db.Update(type, items);
        }

        public int Update(object item, IList<string> updateColumns)
        {
            return this.Db.Update(item, updateColumns);
        }

        public int Update<T>(ICollection<T> items, IList<string> updateColumns)
        {
            return this.Db.Update<T>(items, updateColumns);
        }

        public int Update(Type type, ICollection items, IList<string> updateColumns)
        {
            return this.Db.Update(type, items, updateColumns);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this._connectionManager != null)
            {
                this._connectionManager.Dispose();
            }
        }

        #endregion
    }
}
