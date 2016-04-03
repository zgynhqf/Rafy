/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110109
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110109
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.DbMigration.History;

namespace Rafy.DbMigration.Model
{
    /// <summary>
    /// 用于自动升级的目标数据库
    /// </summary>
    public class DestinationDatabase : Database
    {
        public DestinationDatabase(string name)
            : base(name)
        {
            this.IgnoreTables = new List<string>();

            this.IgnoreTables.Add(EmbadedDbVersionProvider.TableName);
        }

        /// <summary>
        /// 在自动升级过程中，需要忽略掉的表的列表。
        /// </summary>
        public IList<string> IgnoreTables { get; private set; }

        /// <summary>
        /// 判断某个表是否已经被忽略升级。
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsIgnored(string tableName)
        {
            return this.IgnoreTables.Any(t => t.EqualsIgnoreCase(tableName));
        }
    }
}