/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;

namespace Rafy.DataArchiver
{
    /// <summary>
    /// 表示数据归档插件的上下文。
    /// </summary>
    public class AggregationArchiveContext
    {
        /// <summary>
        /// 需要迁移的数据所在的库的配置名。
        /// </summary>
        public string OrignalDataDbSettingName { get; set; }

        /// <summary>
        /// 需要把数据迁移的目标数据库的配置名。
        /// </summary>
        public string BackUpDbSettingName { get; set; }

        /// <summary>
        /// 获取一个时间点，用于确定归档哪天之前的数据。
        /// </summary>
        public DateTime DateOfArchiving { get; set; }

        /// <summary>
        /// 归档的项的集合
        /// </summary>
        public List<ArchiveItem> ItemsToArchive { get; set; }

        /// <summary>
        /// 每批次迁移实体的个数。
        /// </summary>
        public int BatchSize { get; set; } = 1000;
    }

    /// <summary>
    /// 需要归档的项
    /// </summary>
    public class ArchiveItem
    {
        /// <summary>
        /// 需要归档的聚合根类型。
        /// </summary>
        public Type AggregationRoot { get; set; }

        /// <summary>
        /// 数据备份的类型。
        /// 默认为 <see cref="ArchiveType.Cut"/>。
        /// </summary>
        public ArchiveType ArchiveType { get; set; } = ArchiveType.Cut;
    }

    /// <summary>
    /// 数据备份的类型。
    /// </summary>
    public enum ArchiveType
    {
        /// <summary>
        /// 只是复制数据。不删除原来的数据。
        /// 某些公用的表，只要求复制数据。不能删除。例如用户。
        /// </summary>
        Copy,
        /// <summary>
        /// 将数据从原库复制到目标库，同时删除原库中的数据。
        /// 业务的主表，往往需要的是剪切数据，降低主库的压力。
        /// </summary>
        Cut
    }
}