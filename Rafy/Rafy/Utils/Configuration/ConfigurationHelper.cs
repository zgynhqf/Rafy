/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：????
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Rafy
{
    public static class ConfigurationHelper
    {
        /// <summary>
        /// 获取配置文件中的 AppSettings 配置节的的指定键的值，并转换为指定类型。
        /// 如果配置文件中没有该配置项，则方法返回给定的默认值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetAppSettingOrDefault<T>(string key, T defaultValue = default(T))
            where T : struct
        {
            var value = GetAppSettingOrDefault(key);
            if (value != string.Empty)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        return (T)converter.ConvertFromString(value);
                    }
                    catch { }
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 获取配置文件中的 AppSettings 配置节的的指定键的值。
        /// 如果配置文件中没有该配置项，则方法返回空字符串。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetAppSettingOrDefault(string key, string defaultValue = "")
        {
            return Configuration[key] ?? defaultValue;
        }

        /// <summary>
        /// 获取配置文件中的ConnectionString的指定键的值
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        internal static ConnectionStringSettings GetConnectionString(string key, string providerName = "System.Data.SqlClient")
        {
            var connectionStringSection = ConfigurationHelper.Configuration.GetSection("ConnectionStrings").GetSection(key);
            if (connectionStringSection == null) { return null; }

            string connectionString = connectionStringSection.GetSection("connectionString").Value;
            providerName = connectionStringSection.GetSection("providerName").Value ?? providerName;

            return new ConnectionStringSettings(key, connectionString, providerName);
        }

        private static IConfigurationRoot _configuration;

        /// <summary>
        /// 获取代表配置文件中配置根的对象。
        /// </summary>
        public static IConfigurationRoot Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        //.AddJsonFile("appsettings.json")
                        .Build();
                }
                return _configuration;
            }
        }
    }
}