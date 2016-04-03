/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140502 23:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 一个关系型数据库的数据提供器。
    /// </summary>
    public interface IRepositoryDataProvider
    {
        /// <summary>
        /// 通过Id在数据层中查询指定的对象
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        Entity GetById(object id, EagerLoadOptions eagerLoad);
        /// <summary>
        /// 分页查询所有的实体类
        /// </summary>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        object GetAll(PagingInfo paging, EagerLoadOptions eagerLoad);
        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        object GetByParentId(object parentId, PagingInfo paging, EagerLoadOptions eagerLoad);
        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentIdList">The parent identifier list.</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        EntityList GetByParentIdList(object[] parentIdList, PagingInfo paging, EagerLoadOptions eagerLoad);
        /// <summary>
        /// 获取指定 id 集合的实体列表。
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        EntityList GetByIdList(object[] idList, EagerLoadOptions eagerLoad);

        /// <summary>
        /// 递归查找所有树型子
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        EntityList GetByTreeParentIndex(string treeIndex, EagerLoadOptions eagerLoad);
        /// <summary>
        /// 查询所有的根节点。
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        object GetTreeRoots(EagerLoadOptions eagerLoad);

        /// <summary>
        /// 通过 <see cref="CommonQueryCriteria"/> 来查询数据。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        object GetBy(CommonQueryCriteria criteria);
        /// <summary>
        /// 通过 <see cref="ODataQueryCriteria"/> 来查询数据。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        object GetBy(ODataQueryCriteria criteria);

        /// <summary>
        /// 查询某个实体的某个属性的值。
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        LiteDataTable GetEntityValue(object entityId, string property);
    }

    internal interface IRepositoryDataProviderInternal : IRepositoryDataProvider
    {
        ///// <summary>
        ///// 数据门户调用本接口来保存组合中的所有数据。
        ///// </summary>
        ///// <param name="component">
        ///// 一个组合实体、或组合实体的列表。
        ///// </param>
        //void SubmitComposition(IDomainComponent component);

        DataSaver DataSaver { get; }

        DataQueryer DataQueryer { get; }

        //void NotifyDbLoaded(Entity item);
    }
}