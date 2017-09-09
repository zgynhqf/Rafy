/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 10:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 可查询的数据源，可用于 From 语句之后 。
    /// 目前有：<see cref="SqlTable"/>、<see cref="SqlJoin"/>、<see cref="SqlSubSelect"/>。
    /// </summary>
    abstract class SqlSource : SqlNode, ISqlSource
    {
    }
}
