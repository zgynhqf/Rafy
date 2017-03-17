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

namespace Rafy.DataTableMigration.Contexts
{
    /// <summary>
    /// 表示数据归档插件的上下文。
    /// </summary>
    public class DataTableMigrationContext
    {
        /// <summary>
        /// 初始化 <see cref="DataTableMigrationContext"/> 类的新实例。
        /// </summary>
        /// <param name="pageSize">批次大小。</param>
        /// <param name="dateOfArchiving">表示一个时间点，用于确定归档哪天之前的数据。</param>
        /// <param name="archivingAggregationRootTypeList">迁移的聚合根类型。</param>
        public DataTableMigrationContext(int pageSize, DateTime dateOfArchiving, List<Type> archivingAggregationRootTypeList)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            this.PageSize = pageSize;
            this.DateOfArchiving = dateOfArchiving;
            this.ArchivingAggregationRootTypeList = archivingAggregationRootTypeList;
        }

        /// <summary>
        /// 每批次迁移实体的个数
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 获取一个时间点，用于确定归档哪天之前的数据。
        /// </summary>
        public DateTime DateOfArchiving { get; }

        /// <summary>
        /// 迁移的聚合根类型集合
        /// </summary>
        public List<Type> ArchivingAggregationRootTypeList { get; }
    }
}