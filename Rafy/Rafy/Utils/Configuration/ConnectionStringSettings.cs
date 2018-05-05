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
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

#if NS2
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
        /// <summary>
        ///  摘要:
        ///      Gets or sets the connection string.
        /// 
        ///  返回结果:
        ///      The string value assigned to the System.Configuration.ConnectionStringSettings.ConnectionString
        ///      property.
        /// [ConfigurationProperty("connectionString", Options = ConfigurationPropertyOptions.IsRequired, DefaultValue = "")]
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///  摘要:
        ///      Gets or sets the provider name property.
        /// 
        ///  返回结果:
        ///      Gets or sets the System.Configuration.ConnectionStringSettings.ProviderName property.
        /// [ConfigurationProperty("providerName", DefaultValue = "System.Data.SqlClient")]
        /// </summary>
        public string ProviderName { get; set; }
    }
}
#endif