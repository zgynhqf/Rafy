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
using Rafy;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 它把自己存储在上下文中，
    /// 以提供一个简单的方式来在一个数据上下文环境中重用单个连接。
    /// 
    /// 注意：
    /// 暂时不再使用，原因：
    /// 多个线程使用同一个连接，可以解决大部分不需要分布式事务的情况，但是连接变为共享资源，混用会出错（如线程A在读取数据时，线程B想写数据则会报错）。
    /// 多个线程使用多个连接，则会会造成分布式事务的情况。
    /// </summary>
    internal class AppContextDependentConnectionManager : IConnectionManager, IDisposable
    {
        private static object _lock = new object();

        private IDbConnection _connection;

        private DbSetting _dbSetting;

        /// <summary>
        /// 根据数据库配置获取一个连接管理器
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public static AppContextDependentConnectionManager GetManager(DbSetting dbSetting)
        {
            var ctxName = GetContextName(dbSetting);

            lock (_lock)
            {
                var items = AppContext.Items;

                object value = null;
                items.TryGetValue(ctxName, out value);
                var mgr = value as AppContextDependentConnectionManager;
                if (mgr == null)
                {
                    mgr = new AppContextDependentConnectionManager(dbSetting);
                    items.Add(ctxName, mgr);
                }

                mgr.AddRef();
                return mgr;
            }
        }

        private AppContextDependentConnectionManager(DbSetting dbSetting)
        {
            _dbSetting = dbSetting;
            _connection = dbSetting.CreateConnection();
            _connection.Open();
        }

        private static string GetContextName(DbSetting dbSetting)
        {
            return "__db:" + dbSetting.Name;
        }

        /// <summary>
        /// Dispose object, dereferencing or
        /// disposing the connection it is
        /// managing.
        /// </summary>
        public IDbConnection Connection
        {
            get { return this._connection; }
        }

        /// <summary>
        /// 对应的数据库配置信息
        /// </summary>
        public DbSetting DbSetting
        {
            get { return this._dbSetting; }
        }

        private int _refCount;

        private void AddRef()
        {
            _refCount += 1;
        }

        private void DeRef()
        {
            lock (_lock)
            {
                _refCount -= 1;

                //当引用数为 0 时，应该释放连接。
                if (_refCount == 0)
                {
                    _connection.Dispose();
                    var name = GetContextName(this._dbSetting);
                    AppContext.Items.Remove(name);
                }
            }
        }

        #region  IDisposable

        /// <summary>
        /// Dispose 时减少引用数
        /// </summary>
        public void Dispose()
        {
            DeRef();
        }

        #endregion
    }
}