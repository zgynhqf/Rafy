/*******************************************************
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
    /// 查询的数据源。
    /// </summary>
    public interface ISource : IQueryNode
    {
        /// <summary>
        /// 从当前数据源中查找指定仓库对应的表。
        /// </summary>
        /// <param name="repo">要查找这个仓库对应的表。
        /// 如果这个参数传入 null，则表示查找主表（最左边的表）。</param>
        /// <param name="alias">
        /// 要查找表的别名。
        /// 如果仓库在本数据源中匹配多个表，那么将使用别名来进行精确匹配。
        /// 如果仓库在本数据源中只匹配一个表，那么忽略本参数。
        /// </param>
        /// <returns></returns>
        ITableSource FindTable(IRepository repo = null, string alias = null);
    }
}
