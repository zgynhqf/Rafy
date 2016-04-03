/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Rafy.DbMigration.History
{
    /// <summary>
    /// 数据库版本迁移历史记录
    /// </summary>
    [DebuggerDisplay("{Description}")]
    public class HistoryItem
    {
        /// <summary>
        /// 唯一的时间标记
        /// </summary>
        public DateTime TimeId { get; set; }

        /// <summary>
        /// 是否自动生成
        /// </summary>
        public bool IsGenerated { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 对应的 MigrationClass 类全名
        /// 
        /// 注意：
        /// 如果该类在运行时找不到（可能被升级或改名），那么这条历史记录将会被忽略。
        /// </summary>
        public string MigrationClass { get; set; }

        /// <summary>
        /// MigrationClass 类的对象的 xml 序列化数据。
        /// </summary>
        public string MigrationContent { get; set; }

        /// <summary>
        /// 为实现层实现本类的存储，
        /// 数据层真正的存储对象可以临时存储在这个字段中。
        /// 
        /// 此字段不需要存储到数据库中。
        /// </summary>
        public object DataObject { get; set; }
    }
}