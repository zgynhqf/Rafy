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
    /// IDb 的查询参数。
    /// </summary>
    internal interface ISelectArgs : IQueryArgs
    {
        /// <summary>
        /// 用于存储结果的实体列表
        /// </summary>
        IList<Entity> List { get; }

        /// <summary>
        /// 是否只支持一个实体。
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
        /// 贪婪加载某个属性
        /// </summary>
        /// <param name="property">需要贪婪加载的托管属性。可以是一个引用属性，也可以是一个组合子属性。</param>
        /// <param name="propertyOwner">这个属性的拥有者类型。</param>
        void EagerLoad(IProperty property, Type propertyOwner = null);
    }
}
