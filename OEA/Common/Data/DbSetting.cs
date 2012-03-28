/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace hxy.Common.Data
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DbSetting
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// 连接的提供器名称
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// 对应的数据库名称
        /// </summary>
        public string Database { get; private set; }

        /// <summary>
        /// 查找或者根据约定创建连接字符串
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public static DbSetting FindOrCreate(string dbSetting)
        {
            DbSetting setting = null;

            if (!_generatedSettings.TryGetValue(dbSetting, out setting))
            {
                lock (_generatedSettings)
                {
                    if (!_generatedSettings.TryGetValue(dbSetting, out setting))
                    {
                        var config = ConfigurationManager.ConnectionStrings[dbSetting];
                        if (config != null)
                        {
                            setting = new DbSetting
                            {
                                ConnectionString = config.ConnectionString,
                                ProviderName = config.ProviderName,
                            };
                        }
                        else
                        {
                            setting = Create(dbSetting);
                        }

                        setting.Name = dbSetting;

                        var factory = System.Data.Common.DbProviderFactories.GetFactory(setting.ProviderName);
                        var con = factory.CreateConnection();
                        con.ConnectionString = setting.ConnectionString;
                        setting.Database = con.Database;

                        _generatedSettings.Add(dbSetting, setting);
                    }
                }
            }

            return setting;
        }

        private static Dictionary<string, DbSetting> _generatedSettings = new Dictionary<string, DbSetting>();

        private static DbSetting Create(string dbSetting)
        {
            //查找连接字符串时，根据用户的 LocalSqlServer 来查找。
            var local = ConfigurationManager.ConnectionStrings["LocalSqlServer"];
            if (local != null && local.ProviderName == "System.Data.SqlClient")
            {
                var builder = new SqlConnectionStringBuilder(local.ConnectionString);

                var newCon = new SqlConnectionStringBuilder();
                newCon.DataSource = builder.DataSource;
                newCon.InitialCatalog = dbSetting;
                newCon.IntegratedSecurity = builder.IntegratedSecurity;
                if (!newCon.IntegratedSecurity)
                {
                    newCon.UserID = builder.UserID;
                    newCon.Password = builder.Password;
                }

                return new DbSetting
                {
                    ConnectionString = newCon.ToString(),
                    ProviderName = local.ProviderName
                };
            }

            return new DbSetting
            {
                ConnectionString = string.Format(@"Data Source=.\SQLExpress;Initial Catalog={0};Integrated Security=True", dbSetting),
                ProviderName = "System.Data.SqlClient"
            };
        }
    }
}