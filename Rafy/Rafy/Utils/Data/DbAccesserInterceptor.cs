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
        private static bool _observeSql;
        private static long _dbAccessedCount = 0;
        [ThreadStatic]
        private static DbAccessedEventArgs _lastThreadDbAccessedArgs;
        [ThreadStatic]
        private static long _threadDbAccessedCount;

        /// <summary>
        /// 是否启用 Sql 查询监听。 默认为 false。
        /// 打开后，<see cref="DbAccessing"/> 两个事件才会发生。这样才可以监听每一个被执行 Sql。
        /// </summary>
        public static bool ObserveSql { get => _observeSql; set => _observeSql = value; }

        /// <summary>
        /// 返回系统运行到现在，一共记录了多少次 Sql 语句。
        /// </summary>
        public static long DbAccessedCount { get => _dbAccessedCount; }

        /// <summary>
        /// 返回当前线程运行到现在，一共记录了多少次 Sql 语句。
        /// </summary>
        public static long ThreadDbAccessedCount { get => _threadDbAccessedCount; }

        /// <summary>
        /// 当前线程最近一次数据库访问记录。
        /// </summary>
        public static DbAccessedEventArgs ThreadLastDbAccessedArgs { get => _lastThreadDbAccessedArgs; }

        /// <summary>
        /// 发生了数据访问前的事件。
        /// </summary>
        public static event EventHandler<DbAccessEventArgs> DbAccessing;

        /// <summary>
        /// 发生了数据访问后的事件。
        /// 如果数据访问时发生异常，则不会发现这个事件。
        /// </summary>
        public static event EventHandler<DbAccessedEventArgs> DbAccessed;

        //[ThreadStatic]
        //private static EventHandler<DbAccessEventArgs> _threadDbAccessingHandler;
        ///// <summary>
        ///// 当前线程，发生了数据访问时的事件。
        ///// </summary>
        //public static event EventHandler<DbAccessEventArgs> ThreadDbAccessing
        //{
        //    add
        //    {
        //        _threadDbAccessingHandler = (EventHandler<DbAccessEventArgs>)Delegate.Combine(_threadDbAccessingHandler, value);
        //    }
        //    remove
        //    {
        //        _threadDbAccessingHandler = (EventHandler<DbAccessEventArgs>)Delegate.Remove(_threadDbAccessingHandler, value);
        //    }
        //}

        /// <summary>
        /// 记录 Sql 执行过程。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionSchema">The connection schema.</param>
        /// <param name="connection">The connection.</param>
        public static void LogDbAccessing(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            if (_observeSql)
            {
                var handler = DbAccessing;
                if (handler != null)
                {
                    var args = new DbAccessEventArgs
                    {
                        Sql = sql,
                        Parameters = parameters,
                        ConnectionSchema = connectionSchema,
                    };
                    handler(null, args);
                }
            }
        }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="result">
        /// Sql 执行后的结果。
        /// 如果是查询，可能是：DataSet、DataTable、DataRow、DataReader；
        /// 如果是非查询，返回的是受影响的行数。
        /// 如果发生了异常，返回的是异常对象。
        /// </param>
        /// <param name="connectionSchema">The connection schema.</param>
        /// <param name="connection">The connection.</param>
        public static void LogDbAccessed(string sql, IDbDataParameter[] parameters, object result, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            if (_observeSql)
            {
                _dbAccessedCount++;
                _threadDbAccessedCount++;
            }

            Logger.LogDbAccessed(sql, parameters, result, connectionSchema, connection);

            if (_observeSql)
            {
                var args = new DbAccessedEventArgs
                {
                    Sql = sql,
                    Parameters = parameters,
                    ConnectionSchema = connectionSchema,
                    Result = result
                };
                _lastThreadDbAccessedArgs = args;

                var handler = DbAccessed;
                if (handler != null) { handler(null, args); }
            }
        }
    }
}