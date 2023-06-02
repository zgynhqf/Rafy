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
using Rafy.Domain;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain
{
    /// <summary>
    /// 查询结果为实体时的参数。
    /// </summary>
    internal interface IEntityQueryArgs : IQueryArgs
    {
        /// <summary>
        /// 用于存储结果的实体列表
        /// </summary>
        IList<Entity> List { get; }

        /// <summary>
        /// 是否只查询一个实体。
        /// </summary>
        bool FetchingFirst { get; }

        /// <summary>
        /// 分页信息
        /// </summary>
        PagingInfo PagingInfo { get; }

        /// <summary>
        /// 如果某次查询结果是一棵完整的子树，那么必须设置此属性为 true ，才可以把整个树标记为完整加载。
        /// 否则，所有节点的子节点集合 TreeChildren 处在未加载完全的状态（IsLoaded = false）。
        /// </summary>
        bool MarkTreeFullLoaded { get; }

        /// <summary>
        /// 数据加载选项
        /// </summary>
        LoadOptions LoadOptions { get; }

        /// <summary>
        /// 实体查询对应的方法和参数信息。
        /// </summary>
        IEntityQueryInvocation Invocation { get; }
    }
}
