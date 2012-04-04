/*******************************************************
 * 
 * 作者：CSLA
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace OEA.Library
{
    /// <summary>
    /// Provides an automated way to reuse open
    /// database connections within the context
    /// of a single data portal operation.
    /// </summary>
    /// <remarks>
    /// This type stores the open database connection
    /// in <see cref="OEA.ApplicationContext.LocalContext" />
    /// and uses reference counting through
    /// <see cref="IDisposable" /> to keep the connection
    /// open for reuse by child objects, and to automatically
    /// dispose the connection when the last consumer
    /// has called Dispose."
    /// </remarks>
    public class ConnectionManager : IDisposable
    {
        private static object _lock = new object();
        private IDbConnection _connection;
        private string _connectionString;
        private string _label;

        /// <summary>
        /// Gets the ConnectionManager object for the 
        /// specified database.
        /// </summary>
        /// <param name="database">
        /// Database name as shown in the config file.
        /// </param>
        public static ConnectionManager GetManager(string database)
        {
            return GetManager(database, true);
        }

        /// <summary>
        /// Gets the ConnectionManager object for the 
        /// specified database.
        /// </summary>
        /// <param name="database">
        /// Database name as shown in the config file.
        /// </param>
        /// <param name="label">Label for this connection.</param>
        public static ConnectionManager GetManager(string database, string label)
        {
            return GetManager(database, true, label);
        }

        /// <summary>
        /// Gets the ConnectionManager object for the 
        /// specified database.
        /// </summary>
        /// <param name="database">
        /// The database name or connection string.
        /// </param>
        /// <param name="isDatabaseName">
        /// True to indicate that the connection string
        /// should be retrieved from the config file. If
        /// False, the database parameter is directly 
        /// used as a connection string.
        /// </param>
        /// <returns>ConnectionManager object for the name.</returns>
        public static ConnectionManager GetManager(string database, bool isDatabaseName)
        {
            return GetManager(database, isDatabaseName, "default");
        }

        /// <summary>
        /// Gets the ConnectionManager object for the 
        /// specified database.
        /// </summary>
        /// <param name="database">
        /// The database name or connection string.
        /// </param>
        /// <param name="isDatabaseName">
        /// True to indicate that the connection string
        /// should be retrieved from the config file. If
        /// False, the database parameter is directly 
        /// used as a connection string.
        /// </param>
        /// <param name="label">Label for this connection.</param>
        /// <returns>ConnectionManager object for the name.</returns>
        public static ConnectionManager GetManager(string database, bool isDatabaseName, string label)
        {
            if (isDatabaseName)
            {
                database = hxy.Common.Data.DbSetting.FindOrCreate(database).ConnectionString;
            }

            lock (_lock)
            {
                var ctxName = GetContextName(database, label);
                ConnectionManager mgr = null;
                if (ApplicationContext.LocalContext.Contains(ctxName))
                {
                    mgr = (ConnectionManager)(ApplicationContext.LocalContext[ctxName]);

                }
                else
                {
                    mgr = new ConnectionManager(database, label);
                    ApplicationContext.LocalContext[ctxName] = mgr;
                }
                mgr.AddRef();
                return mgr;
            }
        }

        private ConnectionManager(string connectionString, string label)
        {
            _label = label;
            _connectionString = connectionString;

            string provider = ConfigurationManager.AppSettings["dbProvider"];
            if (string.IsNullOrEmpty(provider))
                provider = "System.Data.SqlClient";

            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

            // open connection
            _connection = factory.CreateConnection();
            _connection.ConnectionString = connectionString;
            _connection.Open();

        }

        private static string GetContextName(string connectionString, string label)
        {
            return "__db:" + label + "-" + connectionString;
        }

        /// <summary>
        /// Dispose object, dereferencing or
        /// disposing the connection it is
        /// managing.
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        #region  Reference counting

        private int mRefCount;

        private void AddRef()
        {
            mRefCount += 1;
        }

        private void DeRef()
        {

            lock (_lock)
            {
                mRefCount -= 1;
                if (mRefCount == 0)
                {
                    _connection.Dispose();
                    ApplicationContext.LocalContext.Remove(GetContextName(_connectionString, _label));
                }
            }

        }

        #endregion

        #region  IDisposable

        /// <summary>
        /// Dispose object, dereferencing or
        /// disposing the connection it is
        /// managing.
        /// </summary>
        public void Dispose()
        {
            DeRef();
        }

        #endregion
    }
}