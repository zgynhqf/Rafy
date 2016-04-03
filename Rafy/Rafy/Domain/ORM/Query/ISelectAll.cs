/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 表示查询数据源中的所有属性的节点、也可以表示查询某个指定数据源的所有属性。
    /// </summary>
    public interface ISelectAll : IQueryNode
    {
        /// <summary>
        /// 如果本属性为空，表示选择所有数据源的所有属性；否则表示选择指定数据源的所有属性。
        /// </summary>
        INamedSource Source { get; set; }
    }
}
