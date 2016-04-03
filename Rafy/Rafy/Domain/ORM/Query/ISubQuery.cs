/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:16
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
    /// 子查询。
    /// 对一个子查询分配别名后，可以作为一个新的源。
    /// </summary>
    public interface ISubQuery : INamedSource
    {
        /// <summary>
        /// 内部的查询对象。
        /// </summary>
        IQuery Query { get; set; }

        /// <summary>
        /// 必须对这个子查询指定别名。
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// 为这个子查询结果中的某个列来生成一个属于这个 ISubQueryRef 对象的结果列。
        /// </summary>
        /// <param name="rawColumn">子查询结果中的某个列。</param>
        /// <returns></returns>
        IColumnNode Column(IColumnNode rawColumn);

        /// <summary>
        /// 为这个子查询结果中的某个列来生成一个属于这个 ISubQueryRef 对象的结果列。
        /// 同时，设置它的查询的别名。
        /// </summary>
        /// <param name="rawColumn">子查询结果中的某个列。</param>
        /// <param name="alias">别名。</param>
        /// <returns></returns>
        IColumnNode Column(IColumnNode rawColumn, string alias);
    }
}