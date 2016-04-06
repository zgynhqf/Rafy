/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 15:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;

namespace Rafy.Accounts
{
    /// <summary>
    /// 帐户插件。
    /// 本插件中定义了用户实体及对应的领域控制器。
    /// </summary>
    public class AccountsPlugin : DomainPlugin
    {
        private static string _dbSettingName;
        /// <summary>
        /// 本插件中所有实体对应的连接字符串的配置名。
        /// 如果没有设置，则默认使用 <see cref="DbSettingNames.RafyPlugins"/>。
        /// </summary>
        public static string DbSettingName
        {
            get { return _dbSettingName ?? DbSettingNames.RafyPlugins; }
            set { _dbSettingName = value; }
        }

        /// <summary>
        /// 帐户插件中实体的主键的类型提供程序。
        /// 默认是 Long 型。
        /// </summary>
        public static IKeyProvider KeyProvider = KeyProviders.Get(typeof(long));

        /// <summary>
        /// Initializes the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public override void Initialize(IApp app)
        {
        }
    }
}