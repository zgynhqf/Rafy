﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 表示一个可进行查询的节点。
    /// </summary>
    public interface IQuery : IQueryNode
    {
        /// <summary>
        /// 是否只查询数据的条数。
        /// 
        /// 如果这个属性为真，那么不再需要使用 Selection。
        /// </summary>
        bool IsCounting { get; set; }

        /// <summary>
        /// 是否需要查询不同的结果。
        /// </summary>
        bool IsDistinct { get; set; }

        /// <summary>
        /// 要查询的内容。
        /// 如果本属性为空，表示要查询所有数据源的所有属性。
        /// 可以是单列、多列的数组、ISelectAll 等节点。
        /// </summary>
        IQueryNode Selection { get; set; }

        /// <summary>
        /// 要查询的数据源。
        /// </summary>
        ISource From { get; set; }

        /// <summary>
        /// 查询的过滤条件。
        /// </summary>
        IConstraint Where { get; set; }

        /// <summary>
        /// 查询的排序规则。
        /// 可以指定多个排序条件。
        /// </summary>
        IList<IOrderBy> OrderBy { get; }

        /// <summary>
        /// 获取这个查询中的主实体数据源。
        /// 
        /// 在使用 QueryFactory.Query 方法构造 IQuery 时，需要传入 from 参数，
        /// 如果传入的就是一个 IEntitySource，那么它就是本查询的主实体数据源；
        /// 如果传入的是一个连接的结果，那么整个连接中最左端的实体数据源就是主数据源。
        /// </summary>
        /// <returns></returns>
        ITableSource MainTable { get; }

        /// <summary>
        /// 从当前数据源中查找指定仓库对应的表。
        /// </summary>
        /// <param name="source">在这个结点中查找。</param>
        /// <param name="repo">要查找这个仓库对应的表。如果这个参数传入 null，则表示查找主表。</param>
        /// <param name="alias">
        /// 要查找表的别名。
        /// 如果仓库在本数据源中匹配多个表，那么将使用别名来进行精确匹配。
        /// 如果仓库在本数据源中只匹配一个表，那么忽略本参数。
        /// </param>
        /// <returns></returns>
        ITableSource FindTable(ISource source, IRepository repo = null, string alias = null);
    }
}
