/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120424
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120424
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using hxy.Common.Data;

namespace OEA.Library
{
    /// <summary>
    /// 单连接事务块。
    /// 
    /// 可用于声明一个事务块，在这个事务块中，如果是访问同一个数据库，则整个代码块中只会用到同一个数据库连接。
    /// 这样也就不会造成为分布式事务。（分布式事务在一些数据库中并不支持，例如 SQLCE。）
    /// </summary>
    public class SingleConnectionTrasactionScope : IDisposable
    {
        private TransactionScope _core;

        private ConnectionManager _singleConnectionHolder;

        /// <summary>
        /// 构造一个事务块
        /// </summary>
        /// <param name="dbSetting">整个数据库的配置名</param>
        public SingleConnectionTrasactionScope(DbSetting dbSetting)
        {
            this._core = new TransactionScope();

            //只要找到一个当前数据库的连接管理对象，直到发生 Dispose 前，
            //这个连接都一直不会被关闭，那么代码块中的数据访问方法都是使用同一个打开的连接。
            //这样就不会升级为分布式事务。
            this._singleConnectionHolder = ConnectionManager.GetManager(dbSetting);
        }

        public void Complete()
        {
            this._core.Complete();
        }

        ~SingleConnectionTrasactionScope()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._core.Dispose();
                this._singleConnectionHolder.Dispose();
            }
        }
    }
}
