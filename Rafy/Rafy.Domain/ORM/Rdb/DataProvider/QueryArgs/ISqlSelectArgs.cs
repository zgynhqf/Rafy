/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150316
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150316 16:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// IDb Select 方法的参数。
    /// </summary>
    internal interface ISqlSelectArgs : ISelectArgs
    {
        /// <summary>
        /// 查询的实体类型
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 查询 Sql
        /// </summary>
        string FormattedSql { get; set; }

        /// <summary>
        /// 查询 Sql 对应的参数列表。
        /// </summary>
        object[] Parameters { get; }
    }
}
