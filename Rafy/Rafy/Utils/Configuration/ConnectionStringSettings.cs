/*******************************************************
 * 
 * 作者：许保同
 * 创建日期：20170805
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 许保同 20170805 15:19
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 在 Net Standard 模式下模拟原来的查询字符串获取方式。
    /// </summary>
    internal class ConnectionStringSettings
    {
        //
        // 摘要:
        //     Initializes a new instance of a System.Configuration.ConnectionStringSettings
        //     class.
        public ConnectionStringSettings() { }
        //
        // 摘要:
        //     Initializes a new instance of a System.Configuration.ConnectionStringSettings
        //     class.
        //
        // 参数:
        //   name:
        //     The name of the connection string.
        //
        //   connectionString:
        //     The connection string.
        public ConnectionStringSettings(string name, string connectionString)
        {

            this.Name = name;
            this.ConnectionString = connectionString;
        }
        //
        // 摘要:
        //     Initializes a new instance of a System.Configuration.ConnectionStringSettings
        //     object.
        //
        // 参数:
        //   name:
        //     The name of the connection string.
        //
        //   connectionString:
        //     The connection string.
        //
        //   providerName:
        //     The name of the provider to use with the connection string.
        public ConnectionStringSettings(string name, string connectionString, string providerName) : this(name, connectionString)
        {
            this.ProviderName = providerName;
        }

        //
        // 摘要:
        //     Gets or sets the System.Configuration.ConnectionStringSettings name.
        //
        // 返回结果:
        //     The string value assigned to the System.Configuration.ConnectionStringSettings.Name
        //     property.
        //[ConfigurationProperty("name", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey, DefaultValue = "")]
        public string Name { get; set; }
        //
        // 摘要:
        //     Gets or sets the connection string.
        //
        // 返回结果:
        //     The string value assigned to the System.Configuration.ConnectionStringSettings.ConnectionString
        //     property.
        //[ConfigurationProperty("connectionString", Options = ConfigurationPropertyOptions.IsRequired, DefaultValue = "")]
        public string ConnectionString { get; set; }
        //
        // 摘要:
        //     Gets or sets the provider name property.
        //
        // 返回结果:
        //     Gets or sets the System.Configuration.ConnectionStringSettings.ProviderName property.
        //[ConfigurationProperty("providerName", DefaultValue = "System.Data.SqlClient")]
        public string ProviderName { get; set; }
        //protected internal override ConfigurationPropertyCollection Properties { get; }

        //
        // 摘要:
        //     Returns a string representation of the object.
        //
        // 返回结果:
        //     A string representation of the object.
        //public override string ToString()
        //{
        //    return 
        //}
    }
}
