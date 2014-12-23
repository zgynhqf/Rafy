/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120521 15:10
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120521 15:10
 * 2.0 胡庆访 20130417
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// 表示一个使用单数据库连接的事务代码块（版本 2.0）
    /// Local 的意思是本地事务（即非分布式事务）
    /// </summary>
    public abstract class LocalTransactionBlock : AppContextScope
    {
        #region 字段 - 所有范围对象

        private DbSetting _dbSetting;
        private IsolationLevel _level;
        /// <summary>
        /// 是否已经提交完成。
        /// </summary>
        private bool _rollback = true;

        #endregion

        #region 字段 - 最外层范围对象

        private IDbTransaction _transaction;
        /// <summary>
        /// 是否需要把整个事务回滚。
        /// </summary>
        private bool _wholeRoolback;

        #endregion

        /// <summary>
        /// 构造一个本地事务代码块
        /// </summary>
        /// <param name="dbSetting">数据库配置</param>
        /// <param name="level">
        /// 此级别只在最外层的代码块中有效。
        /// </param>
        public LocalTransactionBlock(DbSetting dbSetting, IsolationLevel level)
        {
            this._dbSetting = dbSetting;
            this._level = level;

            var name = LocalContextName(_dbSetting.Database);
            this.EnterScope(name);
        }

        /// <summary>
        /// 对应的数据库配置
        /// </summary>
        public DbSetting DbSetting
        {
            get { return this._dbSetting; }
        }

        /// <summary>
        /// 对应的事务孤立级别
        /// </summary>
        public IsolationLevel IsolationLevel
        {
            get { return this._level; }
        }

        /// <summary>
        /// 当前范围块正在使用的数据库事务。
        /// </summary>
        public IDbTransaction WholeTransaction
        {
            get
            {
                var ws = this.WholeScope as LocalTransactionBlock;
                return ws._transaction;
            }
        }

        /// <summary>
        /// 提交本事务。
        /// </summary>
        public void Complete()
        {
            this._rollback = false;
        }

        /// <summary>
        /// 子类实现此方法进入指定库的事务。
        /// 
        /// 注意，该方法只会在最外层的 using 块中被调用一次。
        /// 返回的事务，由基类负责它的 Commit、Rollback 和 Dispose，子类不需要管理。
        /// </summary>
        /// <returns></returns>
        protected abstract IDbTransaction BeginTransaction();

        protected override sealed void EnterWholeScope()
        {
            _transaction = this.BeginTransaction();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //如果本事务没有被提交，则整个事务也需要回滚。
                var ws = this.WholeScope as LocalTransactionBlock;
                ws._wholeRoolback |= this._rollback;
            }

            base.Dispose(disposing);
        }

        protected override sealed void ExitWholeScope()
        {
            if (this._wholeRoolback)
            {
                _transaction.Rollback();
                this._wholeRoolback = false;
            }
            else
            {
                _transaction.Commit();
            }

            //不论是正常的提交，还是已经被回滚，最外层的事务块都需要把事务进行释放。
            this.DisposeTransaction(_transaction);
        }

        /// <summary>
        /// 子类实现此方法释放指定的事务。
        /// </summary>
        /// <returns></returns>
        protected virtual void DisposeTransaction(IDbTransaction tran)
        {
            tran.Dispose();
        }

        #region 静态接口

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static IDbTransaction GetCurrentTransaction(string database)
        {
            var name = LocalContextName(database);
            var ws = GetWholeScope(name) as LocalTransactionBlock;
            if (ws != null) return ws._transaction;
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static string LocalContextName(string databse)
        {
            return "LocalTransactionBlock:" + databse;
        }

        #endregion
    }
}