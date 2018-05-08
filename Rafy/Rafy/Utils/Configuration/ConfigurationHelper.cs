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
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;
#if NS2
using Microsoft.Extensions.Configuration;
#endif

namespace Rafy
{
    /// <summary>
    /// 配置文件的帮助类。
    /// .NET Core 下的配置使用方法，见：https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration#in-memory-provider-and-binding-to-a-poco-class。
    /// </summary>
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
#if NET45
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
#elif NS2
            return Configuration.GetValue(key, defaultValue);
#endif
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
#if NET45
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
#elif NS2
            return Configuration.GetValue(key, defaultValue);
#endif
        }

#if NS2
        /// <summary>
        /// 获取配置文件中的ConnectionString的指定键的值
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        internal static ConnectionStringSettings GetConnectionString(string key, string providerName = "System.Data.SqlClient")
        {
            var section = Configuration.GetSection("ConnectionStrings:" + key);
            if (!section.Exists()) return null;

            var res = new ConnectionStringSettings();

            section.Bind(res);

            if (string.IsNullOrEmpty(res.ProviderName))
            {
                res.ProviderName = providerName;
            }

            return res;
        }

        private static IConfigurationRoot _configuration;

        /// <summary>
        /// 获取或设置主配置文件所对应的配置文件根对象。
        /// 
        /// 默认使用 AppDomain.CurrentDomain.BaseDirectory 中的 appsettings.json 文件作为配置文件。
        /// 也可以设置为别的配置文件。
        /// </summary>
        public static IConfigurationRoot Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    //默认使用运行目录中的 appsettings.json 文件作为配置文件。
                    _configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json")
                        .Build();
                }
                return _configuration;
            }
            set { _configuration = value; }
        }
#endif
    }
}