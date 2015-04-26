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
using System.Data;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 单连接事务块。
    /// 
    /// 可用于声明一个事务块，在这个事务块中，如果是访问同一个数据库，则整个代码块中只会用到同一个数据库连接。
    /// 这样也就不会造成为分布式事务。（分布式事务在一些数据库中并不支持，例如 SQLCE。）
    /// 
    /// 注意，多个数据库之间的事务，将会完全独立，互不干扰！
    /// 如果想主动使用分布式事务，请在最外层使用 ADO.NET 的 TransactionScope 类。
    /// </summary>
    public class SingleConnectionTrasactionScope : LocalTransactionBlock
    {
        private ConnectionManager _conMgr;

        /// <summary>
        /// 构造一个事务块
        /// </summary>
        /// <param name="dbSetting">整个数据库的配置名</param>
        public SingleConnectionTrasactionScope(DbSetting dbSetting)
            : base(dbSetting, IsolationLevel.Unspecified) { }

        /// <summary>
        /// 构造一个事务块
        /// </summary>
        /// <param name="dbSetting">整个数据库的配置名</param>
        /// <param name="level">事务的孤立级别</param>
        public SingleConnectionTrasactionScope(DbSetting dbSetting, IsolationLevel level)
            : base(dbSetting, level) { }

        protected override IDbTransaction BeginTransaction()
        {
            //只要找到一个当前数据库的连接管理对象，直到发生 Dispose 前，
            //这个连接都一直不会被关闭，那么代码块中的数据访问方法都是使用同一个打开的连接。
            //这样就不会升级为分布式事务。
            this._conMgr = ConnectionManager.GetManager(this.DbSetting);
            return this._conMgr.Connection.BeginTransaction(this.IsolationLevel);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this._conMgr != null)
                {
                    this._conMgr.Dispose();
                }
            }
        }
    }
}
