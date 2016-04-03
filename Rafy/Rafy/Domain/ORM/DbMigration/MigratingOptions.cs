/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 15:07
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 15:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration;

namespace Rafy.Domain.ORM.DbMigration
{
    /// <summary>
    /// 需要升级的数据库集合选项。
    /// </summary>
    [Serializable]
    public class MigratingOptions
    {
        /// <summary>
        /// 是否执行删除操作。
        /// </summary>
        public DataLossOperation RunDataLossOperation { get; set; }

        /// <summary>
        /// 是否需要保存数据库的升级记录到 <see cref="DbSettingNames.DbMigrationHistory"/> 的库中。
        /// 如果本属性为 true，需要在连接字符串配置中添加该库对应的连接配置。
        /// 默认为 false。
        /// </summary>
        public bool ReserveHistory { get; set; }

        /// <summary>
        /// 要升级的数据库列表。不能为 null。
        /// </summary>
        public string[] Databases { get; set; }

        /// <summary>
        /// 在自动升级过程中，需要忽略掉的表的列表。
        /// </summary>
        public string[] IgnoreTables { get; set; }
    }
}