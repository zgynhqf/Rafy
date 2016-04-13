/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160318
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160318 09:22
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

namespace Rafy.SerialNumber
{
    /// <summary>
    /// 自动流水号生成器插件。
    /// 可以按照指定规则生成流水号。例如如下格式：2016031800000001,2016031800000002
    /// </summary>
    public class SerialNumberPlugin : DomainPlugin
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

        public override void Initialize(IApp app)
        {
        }
    }
}
