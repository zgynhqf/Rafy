/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:48
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
    /// 一个列节点
    /// </summary>
    public interface IColumnNode : IQueryNode
    {
        /// <summary>
        /// 本列属于指定的数据源
        /// </summary>
        INamedSource Owner { get; set; }

        /// <summary>
        /// 本属性对应一个实体的托管属性
        /// </summary>
        IManagedProperty Property { get; set; }

        /// <summary>
        /// 本属性在查询结果中使用的别名。
        /// </summary>
        string Alias { get; set; }
    }
}
