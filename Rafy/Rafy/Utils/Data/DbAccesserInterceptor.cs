/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211028
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211028 19:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// DbAccesser 的拦截器。
    /// </summary>
    public static class DbAccesserInterceptor
    {
        [ThreadStatic]
        private static DbAccessingEventArgs _lastThreadDbAccessingEventArgs;
        [ThreadStatic]
        private static long _threadDbAccessingCount = 0;
        [ThreadStatic]
        private static int _lastRowEffected = 0;
        private static long _dbAccessingCount = 0;

        /// <summary>
        /// 是否启用 Sql 查询监听。 默认为 false。
        /// 打开后，<see cref="DbAccessing"/>、<see cref="ThreadDbAccessing"/> 两个事件才会发生。这样才可以监听每一个被执行 Sql。
        /// </summary>
        public static bool ObserveSql { get; set; }

        /// <summary>
        /// 返回系统运行到现在，一共记录了多少次 Sql 语句。
        /// </summary>
        public static long DbAccessingCount { get => _dbAccessingCount; }

        /// <summary>
        /// 返回当前线程运行到现在，一共记录了多少次 Sql 语句。
        /// </summary>
        public static long ThreadDbAccessingCount { get => _threadDbAccessingCount; }

        /// <summary>
        /// 当前线程最近一次执行的非查询的 SQL 影响的行数。
        /// </summary>
        public static int LastRowEffected { get => _lastRowEffected; }

        /// <summary>
        /// 当前线程最近一次数据库访问记录。
        /// </summary>
        public static DbAccessingEventArgs LastThreadDbAccessingEventArgs { get => _lastThreadDbAccessingEventArgs; }

        /// <summary>
        /// 发生了数据访问时的事件。
        /// </summary>
        public static event EventHandler<DbAccessingEventArgs> DbAccessing;

        [ThreadStatic]
        private static EventHandler<DbAccessingEventArgs> _threadDbAccessingHandler;
        /// <summary>
        /// 当前线程，发生了数据访问时的事件。
        /// </summary>
        public static event EventHandler<DbAccessingEventArgs> ThreadDbAccessing
        {
            add
            {
                _threadDbAccessingHandler = (EventHandler<DbAccessingEventArgs>)Delegate.Combine(_threadDbAccessingHandler, value);
            }
            remove
            {
                _threadDbAccessingHandler = (EventHandler<DbAccessingEventArgs>)Delegate.Remove(_threadDbAccessingHandler, value);
            }
        }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionSchema">The connection schema.</param>
        /// <param name="connection">The connection.</param>
        public static void LogDbAccessing(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            if (ObserveSql)
            {
                _dbAccessingCount++;
                _threadDbAccessingCount++;
            }

            Logger.LogDbAccessing(sql, parameters, connectionSchema, connection);

            if (ObserveSql)
            {
                var args = new DbAccessingEventArgs(sql, parameters, connectionSchema);
                _lastThreadDbAccessingEventArgs = args;

                var threadHandler = _threadDbAccessingHandler;
                var handler = DbAccessing;
                if (threadHandler != null || handler != null)
                {
                    if (threadHandler != null) { threadHandler(null, args); }
                    if (handler != null) { handler(null, args); }
                }
            }
        }

        /// <summary>
        /// 将上条 SQL 执行的结果记录到日志中。
        /// </summary>
        /// <param name="rowsEffect">The result.</param>
        public static void LogDbAccessedResult(int rowsEffect)
        {
            if (ObserveSql)
            {
                _lastRowEffected = rowsEffect;
            }

            Logger.LogDbAccessedResult(rowsEffect);
        }
    }
}
