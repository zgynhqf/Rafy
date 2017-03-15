/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:47
 * 
*******************************************************/

using System;
using Rafy.ComponentModel;
using Rafy.Domain;

namespace Rafy.DataTableMigration
{
    public class DataTableMigrationPlugin : DomainPlugin
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
        /// 备份的连接字符串的配置名
        /// </summary>
        public static string BackUpDbSettingName
        {
            get;
            set;
        }

        /// <summary>
        /// 插件在 Initialize 时调用。
        /// </summary>
        public EventHandler OnInitialize { get; set; }

        public override void Initialize(IApp app)
        {
            OnInitialize?.Invoke(this, new EventArgs());
        }
    }
}
