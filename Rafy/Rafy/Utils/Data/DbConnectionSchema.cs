/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120429
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120429
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Data;
using Rafy.Data.Providers;

namespace Rafy.Data
{
    /// <summary>
    /// 数据库连接结构/方案
    /// </summary>
    public class DbConnectionSchema
    {
        public const string Provider_SqlClient = "System.Data.SqlClient";
        public const string Provider_SqlCe = "System.Data.SqlServerCe";
        public const string Provider_Odbc = "System.Data.Odbc";
        //public const string Provider_Oracle = "System.Data.OracleClient";
        //public const string Provider_Oracle = "Oracle.DataAccess.Client";
        //public const string Provider_Oracle = "Oracle.ManagedDataAccess.Client";

        //PatrickLiu增加的有关MySql的链接客户端
        public const string Provider_MySql = "MySql.Data.MySqlClient";

        public const string DbName_LocalServer = "LocalSqlServer";

        private string _database;

        public DbConnectionSchema(string connectionString, string providerName)
        {
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;
        }

        /// <summary>
        /// 子类使用
        /// </summary>
        internal DbConnectionSchema() { }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; internal set; }

        /// <summary>
        /// 连接的提供器名称
        /// </summary>
        public string ProviderName { get; internal set; }

        /// <summary>
        /// 对应的数据库名称
        /// </summary>
        public string Database
        {
            get
            {
                if (this._database == null)
                {
                    this.ParseDbName();
                }

                return this._database;
            }
        }

        private void ParseDbName()
        {
            var con = this.CreateConnection();
            var database = con.Database;

            //System.Data.OracleClient 解析不出这个值，需要特殊处理。
            if (string.IsNullOrWhiteSpace(database) && IsOracleProvider(this))
            {
                //Oracle 中，把用户名（Schema）认为数据库名。
                database = GetOracleUserId(this);
            }

            this._database = database;
        }

        /// <summary>
        /// 使用当前的结构来创建一个连接。
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection()
        {
            var factory = DbConnectorFactory.GetFactory(this.ProviderName);

            var connection = factory.CreateConnection();
            connection.ConnectionString = this.ConnectionString;

            return connection;
        }

        /// <summary>
        /// 判断指定的提供程序是否为 Oracle 提供程序。
        /// 目前已知的 Oracle 提供程序有：
        /// System.Data.OracleClient、Oracle.DataAccess.Client、Oracle.ManagedDataAccess.Client
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public static bool IsOracleProvider(DbConnectionSchema schema)
        {
            return IsOracleProvider(schema.ProviderName);
        }

        /// <summary>
        /// 判断指定的提供程序是否为 Oracle 提供程序。
        /// 目前已知的 Oracle 提供程序有：
        /// System.Data.OracleClient、Oracle.DataAccess.Client、Oracle.ManagedDataAccess.Client
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static bool IsOracleProvider(string providerName)
        {
            return providerName.Contains("Oracle");
        }

        /// <summary>
        /// 获取 Oracle 连接中的用户 Id。
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static string GetOracleUserId(DbConnectionSchema schema)
        {
            var match = Regex.Match(schema.ConnectionString, @"User Id=\s*(?<userId>\w+)\s*");
            if (!match.Success)
            {
                throw new NotSupportedException("无法解析出此数据库连接字符串中的数据库名：" + schema.ConnectionString);
            }
            var userId = match.Groups["userId"].Value;
            return userId;
        }
    }
}