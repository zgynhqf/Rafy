/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150510
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150510 22:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 数据库连接的管理只是依赖当前线程中的事务。
    /// 如果代码没有在事务中时，则每次都构建新的连接，不再进行连接对象的共享。
    /// 依赖于 <see cref="LocalTransactionBlock"/> 的连接管理器。
    /// </summary>
    internal class TransactionDependentConnectionManager : IConnectionManager
    {
        private LocalTransactionBlock _block;

        private IDbConnection _connection;

        private DbSetting _dbSetting;

        private TransactionDependentConnectionManager() { }

        public static TransactionDependentConnectionManager GetManager(DbSetting dbSetting)
        {
            var res = new TransactionDependentConnectionManager();

            res._block = LocalTransactionBlock.GetWholeScope(dbSetting.Database);
            if (res._block != null)
            {
                res._connection = res._block.WholeTransaction.Connection;
            }
            else
            {
                //没有定义事务范围时，无需共享连接。
                res._connection = dbSetting.CreateConnection();
                res._connection.Open();
            }

            res._dbSetting = dbSetting;

            return res;
        }

        public IDbConnection Connection
        {
            get { return _connection; }
        }

        public DbSetting DbSetting
        {
            get { return _dbSetting; }
        }

        public void Dispose()
        {
            //如果连接是来自事务，则不需要本对象来析构连接。
            if (_block == null)
            {
                _connection.Dispose();
            }
        }
    }
}