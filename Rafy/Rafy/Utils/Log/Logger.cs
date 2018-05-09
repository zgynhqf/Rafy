﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Data;
using Rafy.Data;

namespace Rafy
{
    /// <summary>
    /// 一个简单的日志记录类。
    /// 
    /// 目前只有处理异常的方法。
    /// </summary>
    public static class Logger
    {
        private static LoggerBase _impl = new FileLogger();

        /// <summary>
        /// 使用具体的日志记录器来接管本 API。
        /// </summary>
        /// <param name="loggerImpl"></param>
        public static void SetImplementation(LoggerBase loggerImpl)
        {
            _impl = loggerImpl;
        }

        /// <summary>
        /// 记录某个已经生成的异常到文件中。
        /// </summary>
        /// <param name="title">异常对应的标题，用于描述当前异常的信息。</param>
        /// <param name="e"></param>
        public static void LogError(string title, Exception e)
        {
            try
            {
                _impl.LogError(title, e);
            }
            catch { }
        }

        /// <summary>
        /// 记录某个消息到 Log 日志中。
        /// </summary>
        /// <param name="message">要记录的消息。</param>
        public static void LogInfo(string message)
        {
            try
            {
                _impl.LogInfo(message);
            }
            catch { }
        }

        [ThreadStatic]
        private static long _threadDbAccessedCount = 0;
        [ThreadStatic]
        private static int _lastRowEffected = 0;
        private static long _dbAccessedCount = 0;

        /// <summary>
        /// 返回系统运行到现在，一共记录了多少次 Sql 语句。
        /// </summary>
        public static long DbAccessedCount
        {
            get { return _dbAccessedCount; }
        }

        /// <summary>
        /// 返回当前线程运行到现在，一共记录了多少次 Sql 语句。
        /// </summary>
        public static long ThreadDbAccessedCount
        {
            get { return _threadDbAccessedCount; }
        }

        /// <summary>
        /// 当前线程最近一次执行的非查询的 SQL 影响的行数。
        /// </summary>
        public static int LastRowEffected
        {
            get { return _lastRowEffected; }
        }

        /// <summary>
        /// 是否启用 Sql 查询监听。 默认为 false。
        /// 打开后，DbAccessed、ThreadDbAccessed 两个事件才会发生。这样才可以监听每一个被执行 Sql。
        /// </summary>
        public static bool EnableSqlObervation { get; set; }

        /// <summary>
        /// 发生了数据访问时的事件。
        /// </summary>
        public static event EventHandler<DbAccessedEventArgs> DbAccessed;

        [ThreadStatic]
        private static EventHandler<DbAccessedEventArgs> _threadDbAccessedHandler;
        /// <summary>
        /// 当前线程，发生了数据访问时的事件。
        /// </summary>
        public static event EventHandler<DbAccessedEventArgs> ThreadDbAccessed
        {
            add
            {
                _threadDbAccessedHandler = (EventHandler<DbAccessedEventArgs>)Delegate.Combine(_threadDbAccessedHandler, value);
            }
            remove
            {
                _threadDbAccessedHandler = (EventHandler<DbAccessedEventArgs>)Delegate.Remove(_threadDbAccessedHandler, value);
            }
        }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionSchema">The connection schema.</param>
        /// <param name="connection">The connection.</param>
        public static void LogDbAccessed(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            if (EnableSqlObervation)
            {
                _dbAccessedCount++;
                _threadDbAccessedCount++;
            }

            try
            {
                _impl.LogDbAccessed(sql, parameters, connectionSchema, connection);
            }
            catch { }

            if (EnableSqlObervation)
            {
                var threadHandler = _threadDbAccessedHandler;
                var handler = DbAccessed;
                if (threadHandler != null || handler != null)
                {
                    var args = new DbAccessedEventArgs(sql, parameters, connectionSchema);

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
            if (EnableSqlObervation)
            {
                _lastRowEffected = rowsEffect;
            }

            _impl.LogDbAccessedResult(rowsEffect);
        }

        /// <summary>
        /// 数据访问事件参数。
        /// </summary>
        public struct DbAccessedEventArgs
        {
            public DbAccessedEventArgs(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema)
            {
                this.Sql = sql;
                this.Parameters = parameters;
                this.ConnectionSchema = connectionSchema;
            }

            /// <summary>
            /// 执行的 Sql
            /// </summary>
            public string Sql { get; private set; }

            /// <summary>
            /// 所有的参数值。
            /// </summary>
            public IDbDataParameter[] Parameters { get; private set; }

            /// <summary>
            /// 对应的数据库连接
            /// </summary>
            public DbConnectionSchema ConnectionSchema { get; private set; }
        }
    }
}