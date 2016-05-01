/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160502 01:50
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
using Rafy.FileStorage.Controllers;

namespace Rafy.FileStorage
{
    /// <summary>
    /// 文件存储插件。
    /// 用于将文件存储到各种不同的位置，并以 <see cref="FileInfo"/> 实体来表示存储的文件。
    /// 通过引用这个实体，其它插件将非常方便地找到对应的文件和文件中的数据。
    /// <para>
    /// 使用方法：
    /// 设置 <see cref="ContentProvider"/> 属性后，直接保存和读取 <see cref="FileInfo"/> 类型即可。
    /// </para>
    /// </summary>
    public class FileStoragePlugin : DomainPlugin
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
        /// 文件内容的提供程序。默认实现为 <see cref="DiskFileContentProvider"/>。
        /// </summary>
        public static IFileInfoContentProvider ContentProvider { get; set; } = new DiskFileContentProvider();

        public override void Initialize(IApp app)
        {
        }
    }
}
