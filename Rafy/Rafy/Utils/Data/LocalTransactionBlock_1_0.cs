////*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120521 15:10
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120521 15:10
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Runtime;
//using System.Text;

//namespace Rafy.Data
//{
//    /// <summary>
//    /// 表示一个使用单数据库连接的事务代码块
//    /// Local 的意思是本地事务（即非分布式事务）
//    /// </summary>
//    public abstract class LocalTransactionBlock_1_0 : IDisposable
//    {
//        #region 获取、创建、释放 事务。

//        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
//        internal static TransactionRef GetTransactionRef(string database)
//        {
//            return GetTransactionRef(database, null);
//        }

//        /// <summary>
//        /// 获取某个数据库在当前线程中正在被使用的事务引用。
//        /// </summary>
//        /// <param name="database"></param>
//        /// <param name="currentBlock">如果指定此对象，则在还没有创建事务时，使用此类的抽象方法创建一个新的事务对象。</param>
//        /// <returns></returns>
//        private static TransactionRef GetTransactionRef(string database, LocalTransactionBlock_1_0 currentBlock)
//        {
//            var name = LocalContextName(database);
//            var items = ServerContext.Items;

//            object value = null;
//            items.TryGetValue(name, out value);

//            var tranRef = value as TransactionRef;
//            if (tranRef == null && currentBlock != null)
//            {
//                var tran = currentBlock.BeginTransaction();

//                tranRef = new TransactionRef(tran);
//                items.Add(name, tranRef);
//            }

//            return tranRef;
//        }

//        /// <summary>
//        /// 清除当前数据库在当前线程中正在使用的事务。
//        /// </summary>
//        private void ClearTransactionRef()
//        {
//            var name = LocalContextName(this._dbSetting.Database);
//            ServerContext.Items.Remove(name);
//            this._currentRef = null;
//        }

//        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
//        private static string LocalContextName(string databse)
//        {
//            return "LocalTransactionBlock:" + databse;
//        }

//        #endregion

//        #region 私有字段

//        private DbSetting _dbSetting;

//        private IsolationLevel _level;

//        private TransactionRef _currentRef;

//        /// <summary>
//        /// 是否已经提交完成。
//        /// </summary>
//        private bool _completed;

//        #endregion

//        /// <summary>
//        /// 构造一个本地事务代码块
//        /// </summary>
//        /// <param name="dbSetting">数据库配置</param>
//        /// <param name="level">
//        /// 此级别只在最外层的代码块中有效。
//        /// </param>
//        public LocalTransactionBlock_1_0(DbSetting dbSetting, IsolationLevel level)
//        {
//            this._dbSetting = dbSetting;
//            this._level = level;

//            var tranRef = GetTransactionRef(this._dbSetting.Database, this);

//            tranRef.RefCount++;

//            this._currentRef = tranRef;
//        }

//        /// <summary>
//        /// 对应的数据库配置
//        /// </summary>
//        public DbSetting DbSetting
//        {
//            get { return this._dbSetting; }
//        }

//        /// <summary>
//        /// 对应的事务孤立级别
//        /// </summary>
//        public IsolationLevel IsolationLevel
//        {
//            get { return this._level; }
//        }

//        /// <summary>
//        /// 子类实现此方法进入指定库的事务。
//        /// 
//        /// 注意，该方法只会在最外层的 using 块中被调用一次。
//        /// 返回的事务，由基类负责它的 Commit、Rollback 和 Dispose，子类不需要管理。
//        /// </summary>
//        /// <returns></returns>
//        protected abstract IDbTransaction BeginTransaction();

//        /// <summary>
//        /// 子类实现此方法释放指定的事务。
//        /// </summary>
//        /// <returns></returns>
//        protected virtual void DisposeTransaction(IDbTransaction tran)
//        {
//            tran.Dispose();
//        }

//        /// <summary>
//        /// 提交本事务。
//        /// </summary>
//        public void Complete()
//        {
//            this._completed = true;
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                //如果在 Dispose 时还没有提交事务，则表示需要把事务回滚。
//                var tranRef = this._currentRef;

//                //只是把引用次数减一
//                tranRef.RefCount--;

//                tranRef.NeedRoolback |= !this._completed;

//                //当数据到达 0 时，说明所有提交完毕，这时才真正提交事务。
//                if (tranRef.RefCount == 0)
//                {
//                    if (tranRef.NeedRoolback)
//                    {
//                        tranRef.Transaction.Rollback();
//                        tranRef.NeedRoolback = false;
//                    }
//                    else
//                    {
//                        tranRef.Transaction.Commit();
//                    }

//                    //不论是正常的提交，还是已经被回滚，最外层的事务块都需要把事务进行释放。
//                    this.DisposeTransaction(tranRef.Transaction);
//                    this.ClearTransactionRef();
//                }
//            }
//        }

//        #region Dispose Pattern

//        ~LocalTransactionBlock_1_0()
//        {
//            this.Dispose(false);
//        }

//        void IDisposable.Dispose()
//        {
//            this.Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        #endregion
//    }

//    /// <summary>
//    /// 某个事务的引用
//    /// </summary>
//    internal class TransactionRef
//    {
//        private IDbTransaction _transaction;

//        public TransactionRef(IDbTransaction transaction)
//        {
//            this._transaction = transaction;
//        }

//        /// <summary>
//        /// 当前线程正在使用的数据库事务。
//        /// 
//        /// 设置后，DBAccesser 会从这里读取事务使用。
//        /// </summary>
//        public IDbTransaction Transaction
//        {
//            get { return this._transaction; }
//        }

//        /// <summary>
//        /// 当前线程此事务被引用的次数
//        /// 
//        /// 正数表示当前事务正在使用中，还没有到提交的时候。
//        /// 0 表示全部代码块提交完成。
//        /// </summary>
//        public int RefCount;

//        /// <summary>
//        /// 是否需要把整个事务回滚。
//        /// </summary>
//        public bool NeedRoolback;
//    }
//}