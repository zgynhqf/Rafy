/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:40
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 排序节点
    /// </summary>
    public interface IOrderBy : IQueryNode
    {
        /// <summary>
        /// 使用这个属性进行排序。
        /// </summary>
        IColumnNode Column { get; set; }

        /// <summary>
        /// 使用这个方向进行排序。
        /// </summary>
        OrderDirection Direction { get; set; }
    }
}
