/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120521 15:10
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120521 15:10
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace hxy.Common.Data
{
    /// <summary>
    /// 表示一个使用单数据库连接的事务代码块
    /// Local 的意思是本地事务（即非分布式事务）
    /// </summary>
    public abstract class LocalTransactionBlock : IDisposable
    {
        /// <summary>
        /// 当前线程正在使用的数据库事务。
        /// 
        /// 设置后，DBAccesser 会从这里读取事务使用。
        /// </summary>
        [ThreadStatic]
        internal static IDbTransaction Current;

        /// <summary>
        /// 当前线程此事务被引用的次数
        /// 
        /// 正数表示当前事务正在使用中，还没有到提交的时候。
        /// 0 表示全部代码块提交完成。
        /// </summary>
        [ThreadStatic]
        internal static int CurrentTransactionRef;

        private DbSetting _dbSetting;

        private IsolationLevel _level;

        /// <summary>
        /// 是否已经提交完成。
        /// </summary>
        private bool _completed;

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

            if (Current == null)
            {
                Current = this.BeginTransaction();
            }

            //如果当前线程上次事务被回滚，则这个值会变为 -1，本次启动需要重新设置。
            if (CurrentTransactionRef < 0) { CurrentTransactionRef = 0; }

            CurrentTransactionRef++;
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
        /// 子类实现此方法进入指定库的事务。
        /// 注意，返回的事务，由基类负责它的 Commit、Rollback 和 Dispose。
        /// </summary>
        /// <returns></returns>
        protected abstract IDbTransaction BeginTransaction();

        /// <summary>
        /// 提交本事务。
        /// </summary>
        public void Complete()
        {
            this._completed = true;

            //只是把引用次数减一
            CurrentTransactionRef--;

            //当数据到达 0 时，说明所有提交完毕，这时才真正提交事务。
            if (CurrentTransactionRef == 0)
            {
                Current.Commit();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //如果在 Dispose 时还没有提交事务，则表示需要把事务回滚。
                var current = Current;
                if (current != null)
                {
                    if (!this._completed)
                    {
                        //把引用设置为负数，这样外层的代码块就不会再提交本事务。
                        CurrentTransactionRef = -1;
                        current.Rollback();
                    }

                    //不论是正常的提交，还是已经被回滚，最外层的事务块都需要把事务进行释放。
                    if (CurrentTransactionRef <= 0)
                    {
                        current.Dispose();
                        Current = null;
                    }
                }
            }
        }

        #region Dispose Pattern

        ~LocalTransactionBlock()
        {
            this.Dispose(false);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
