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

namespace Rafy.Domain
{
    /// <summary>
    /// 所有查询的参数
    /// </summary>
    public interface IQueryArgs
    {
        /// <summary>
        /// 本次查询的类型。
        /// </summary>
        RepositoryQueryType QueryType { get; }
    }
}