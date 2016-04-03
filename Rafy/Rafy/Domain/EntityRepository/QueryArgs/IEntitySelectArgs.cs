/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2012
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130523 16:36
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain
{
    /// <summary>
    /// IDb Select 方法的参数。
    /// </summary>
    internal interface IEntitySelectArgs : ISelectArgs
    {
        /// <summary>
        /// 对应的查询条件定义。
        /// </summary>
        IQuery Query { get; }
    }
}
