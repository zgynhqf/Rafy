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

namespace hxy.Common.Data
{
    /// <summary>
    /// 数据库连接结构/方案
    /// </summary>
    public class DbConnectionSchema
    {
        public const string Provider_SqlClient = "System.Data.SqlClient";
        public const string Provider_SqlCe = "System.Data.SqlServerCe";
        public const string Provider_Oracle = "System.Data.OracleClient";
        //public const string Provider_Oracle = "Oracle.DataAccess.Client";
        public const string Provider_Odbc = "System.Data.Odbc";

        private string _database;

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

        public DbConnectionSchema(string connectionString, string providerName)
        {
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;
        }

        /// <summary>
        /// 子类使用
        /// </summary>
        internal DbConnectionSchema() { }

        private void ParseDbName()
        {
            var factory = DbProviderFactories.GetFactory(this.ProviderName);
            var con = factory.CreateConnection();
            con.ConnectionString = this.ConnectionString;
            var database = con.Database;

            //System.Data.OracleClient 解析不出这个值，需要特殊处理。
            if (string.IsNullOrWhiteSpace(database))
            {
                var match = Regex.Match(this.ConnectionString, @"Data Source=\s*(?<dbName>\w+)\s*");
                if (!match.Success)
                {
                    throw new NotSupportedException("无法解析出此数据库连接字符串中的数据库名：" + this.ConnectionString);
                }
                database = match.Groups["dbName"].Value;
            }

            this._database = database;
        }
    }
}