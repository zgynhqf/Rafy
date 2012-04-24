using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using OEA.Library;
using OEA.ORM.sqlserver;
using hxy.Common.Data;

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

                    var dba = new DBAccesser(
                        this._connectionManager.Connection,
                        this._connectionManager.DbSetting.ProviderName
                        );

                    this._db = new SqlDb(dba);
                }
                return this._db;
            }
        }

        #region IDb Members

        hxy.Common.Data.IDBAccesser IDb.DBA
        {
            get { return this.Db.DBA; }
        }

        int IDb.Delete(Type type, IQuery query)
        {
            return this._db.Delete(type, query);
        }

        int IDb.Delete(IEntity item)
        {
            return this.Db.Delete(item);
        }

        ITable IDb.GetTable(Type type)
        {
            return this.Db.GetTable(type);
        }

        int IDb.Insert(IEntity item)
        {
            return this.Db.Insert(item);
        }

        IQuery IDb.Query(Type entityType)
        {
            return this.Db.Query(entityType);
        }

        IList IDb.Select(IQuery typedQuery)
        {
            return this.Db.Select(typedQuery);
        }

        IList IDb.Select(Type type, string sql)
        {
            return this.Db.Select(type, sql);
        }

        int IDb.Update(IEntity item)
        {
            return this.Db.Update(item);
        }

        void IDisposable.Dispose()
        {
            if (this._connectionManager != null)
            {
                this._connectionManager.Dispose();
            }
        }

        #endregion
    }
}
