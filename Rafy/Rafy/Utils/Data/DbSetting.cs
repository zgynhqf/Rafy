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
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Rafy.Data
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DbSetting : DbConnectionSchema
    {
        private static Dictionary<string, DbSetting> _generatedSettings = new Dictionary<string, DbSetting>();

        private DbSetting() { }

        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 查找或者根据约定创建连接字符串
        /// </summary>
        /// <param name="dbSettingName"></param>
        /// <returns></returns>
        public static DbSetting FindOrCreate(string dbSettingName)
        {
            if (dbSettingName == null) throw new ArgumentNullException("dbSetting");//可以是空字符串。

            DbSetting setting = null;

            if (!_generatedSettings.TryGetValue(dbSettingName, out setting))
            {
                lock (_generatedSettings)
                {
                    if (!_generatedSettings.TryGetValue(dbSettingName, out setting))
                    {
                        var config = ConfigurationHelper.GetConnectionString(dbSettingName);
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
                            setting = Create(dbSettingName);
                        }

                        setting.Name = dbSettingName;

                        _generatedSettings.Add(dbSettingName, setting);
                    }
                }
            }

            return setting;
        }

        /// <summary>
        /// 添加一个数据库连接配置。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        public static DbSetting SetSetting(string name, string connectionString, string providerName)
        {
            if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("string.IsNullOrEmpty(dbSetting.Name) must be false.");
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException("providerName");

            var setting = new DbSetting
            {
                Name = name,
                ConnectionString = connectionString,
                ProviderName = providerName
            };

            lock (_generatedSettings)
            {
                _generatedSettings[name] = setting;
            }

            return setting;
        }

        /// <summary>
        /// 获取当前已经被生成的 DbSetting。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DbSetting> GetGeneratedSettings()
        {
            return _generatedSettings.Values;
        }

        private static DbSetting Create(string dbSettingName)
        {
            //查找连接字符串时，根据用户的 LocalSqlServer 来查找。
            var local = ConfigurationHelper.GetConnectionString(DbName_LocalServer);
            if (local != null && local.ProviderName == Provider_SqlClient)
            {
                var builder = new SqlConnectionStringBuilder(local.ConnectionString);

                var newCon = new SqlConnectionStringBuilder();
                newCon.DataSource = builder.DataSource;
                newCon.InitialCatalog = dbSettingName;
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
                ConnectionString = string.Format(@"Data Source={0}.sdf", dbSettingName),
                ProviderName = Provider_SqlCe
            };

            //return new DbSetting
            //{
            //    ConnectionString = string.Format(@"Data Source=.\SQLExpress;Initial Catalog={0};Integrated Security=True", dbSetting),
            //    ProviderName = "System.Data.SqlClient"
            //};
        }
    }
}