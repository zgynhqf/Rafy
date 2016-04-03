/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 一个表数据源。
    /// </summary>
    public interface ITableSource : INamedSource
    {
        /// <summary>
        /// 本表数据源来对应这个实体仓库。
        /// </summary>
        IRepository EntityRepository { get; set; }

        /// <summary>
        /// 同一个实体仓库可以表示多个不同的数据源。这时，需要这些不同的数据源指定不同的别名。
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// 返回 Id 属性对应的列节点。
        /// </summary>
        IColumnNode IdColumn { get; }

        /// <summary>
        /// 查找出某个列对应的节点。
        /// 如果没有找到，则会抛出异常。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        IColumnNode Column(IManagedProperty property);

        /// <summary>
        /// 在查找出某个列的同时，设置它的查询的别名。
        /// 如果没有找到，则会抛出异常。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        IColumnNode Column(IManagedProperty property, string alias);

        /// <summary>
        /// 查找出某个属性对应的列节点。
        /// 没有找到，不会抛出异常。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        IColumnNode FindColumn(IManagedProperty property);
    }
}