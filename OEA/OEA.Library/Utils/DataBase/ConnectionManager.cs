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
using hxy.Common.Data;

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
        private DbSetting _dbSetting;

        private string _label;

        /// <summary>
        /// Gets the ConnectionManager object for the 
        /// specified database.
        /// </summary>
        /// <param name="dbSetting">
        /// Database name as shown in the config file.
        /// </param>
        public static ConnectionManager GetManager(string dbSetting)
        {
            return GetManager(dbSetting, "Default");
        }

        /// <summary>
        /// Gets the ConnectionManager object for the 
        /// specified database.
        /// </summary>
        /// <param name="dbSetting">
        /// The database name or connection string.
        /// </param>
        /// 
        /// <param name="label">Label for this connection.</param>
        /// <returns>ConnectionManager object for the name.</returns>
        public static ConnectionManager GetManager(string dbSetting, string label)
        {
            lock (_lock)
            {
                var ctxName = GetContextName(dbSetting, label);
                ConnectionManager mgr = null;
                if (ApplicationContext.LocalContext.Contains(ctxName))
                {
                    mgr = (ConnectionManager)(ApplicationContext.LocalContext[ctxName]);
                }
                else
                {
                    mgr = new ConnectionManager(DbSetting.FindOrCreate(dbSetting), label);
                    ApplicationContext.LocalContext[ctxName] = mgr;
                }
                mgr.AddRef();
                return mgr;
            }
        }

        private ConnectionManager(DbSetting dbSetting, string label)
        {
            this._label = label;
            this._dbSetting = dbSetting;
            DbProviderFactory factory = DbProviderFactories.GetFactory(dbSetting.ProviderName);

            // open connection
            _connection = factory.CreateConnection();
            _connection.ConnectionString = dbSetting.ConnectionString;
            _connection.Open();
        }

        private static string GetContextName(string dbSettingName, string label)
        {
            return "__db:" + label + "-" + dbSettingName;
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

        /// <summary>
        /// 对应的数据库配置信息
        /// </summary>
        public DbSetting DbSetting
        {
            get { return this._dbSetting; }
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
                    var name = GetContextName(this._dbSetting.Name, _label);
                    ApplicationContext.LocalContext.Remove(name);
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