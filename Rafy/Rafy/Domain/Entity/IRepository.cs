/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101101
 * 说明：实体类模块使用的抽象的懒加载提供器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.Domain.ORM;
using Rafy.ManagedProperty;
using Rafy.Domain.Validation;
using Rafy.Domain.Caching;
using Rafy;
using Rafy.Domain.ORM.Linq;
using System.Linq.Expressions;
using Rafy.DataPortal;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体类模块使用的抽象的懒加载提供器。
    /// 实体类只依赖这个抽象类，而不依赖具体的提供方案。
    /// </summary>
    public interface IRepository : IEntityInfoHost
    {
        /// <summary>
        /// 该实体的仓库。
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 对应的实体是否为树型实体
        /// </summary>
        bool SupportTree { get; }

        /// <summary>
        /// 如果本仓库对应的实体是一个树型实体，那么这个属性表示这个实体使用的树型编号方案。
        /// </summary>
        TreeIndexOption TreeIndexOption { get; }

        /// <summary>
        /// 所有可用的仓库扩展。
        /// </summary>
        IList<IRepositoryExt> Extensions { get; }

        /// <summary>
        /// 当前使用的缓存对象。
        /// </summary>
        RepositoryCache Cache { get; }

        /// <summary>
        /// 是否声明本仓库为本地仓库（客户端只在客户端查询，服务端在服务端查询）
        /// </summary>
        DataPortalLocation DataPortalLocation { get; }

        /// <summary>
        /// 数据提供程序。
        /// </summary>
        IRepositoryDataProvider DataProvider { get; }

        /// <summary>
        /// 创建一个全新的实体对象
        /// </summary>
        /// <returns></returns>
        Entity New();

        /// <summary>
        /// 创建一个全新的实体列表对象
        /// </summary>
        /// <returns></returns>
        IEntityList NewList();

        /// <summary>
        /// 统计仓库中所有的实体数量
        /// </summary>
        /// <returns></returns>
        long CountAll();

        /// <summary>
        /// 统计某个父对象下的子对象条数
        /// </summary>
        /// <returns></returns>
        long CountByParentId(object parentId);

        /// <summary>
        /// 查询所有的根节点数量。
        /// </summary>
        /// <returns></returns>
        long CountTreeRoots();

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询实体的个数。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        long CountBy(CommonQueryCriteria criteria);

        /// <summary>
        /// 优先使用缓存中的数据来通过 Id 获取指定的实体对象
        /// 
        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetById 并返回结果。
        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Entity CacheById(object id);

        /// <summary>
        /// 优先使用缓存中的数据来查询所有的实体类
        /// 
        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetAll 并返回结果。
        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
        /// </summary>
        /// <returns></returns>
        IEntityList CacheAll();

        /// <summary>
        /// 通过Id获取指定的实体对象
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        Entity GetById(object id, LoadOptions loadOptions = null);

        /// <summary>
        /// 通过单一属性的精确匹配来查询单一实体。
        /// </summary>
        /// <param name="keyProperty"></param>
        /// <param name="key"></param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        Entity GetByKey(string keyProperty, object key, LoadOptions loadOptions = null);

        /// <summary>
        /// 以分页的方式查询所有实体。
        /// </summary>
        /// <param name="paging">分页信息。</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetAll(PagingInfo paging = null, LoadOptions loadOptions = null);

        /// <summary>
        /// 获取指定 id 集合的实体列表。
        /// </summary>
        /// <param name="idList">The identifier list.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetByIdList(object[] idList, LoadOptions loadOptions = null);

        /// <summary>
        /// 通过父对象的 Id 列表查询所有的实体。
        /// </summary>
        /// <param name="parentIdList">The parent identifier list.</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetByParentIdList(object[] parentIdList, PagingInfo paging = null, LoadOptions loadOptions = null);

        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetByParentId(object parentId, PagingInfo paging = null, LoadOptions loadOptions = null);

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        IEntityList GetBy(CommonQueryCriteria criteria);

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询唯一实体。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        Entity GetFirstBy(CommonQueryCriteria criteria);

        ///// <summary>
        ///// 递归统计所有树型子节点的个数。
        ///// </summary>
        ///// <param name="treeIndex"></param>
        ///// <returns></returns>
        //int CountByTreeParentIndex(string treeIndex);

        ///// <summary>
        ///// 统计指定节点的直接子节点的个数。
        ///// </summary>
        ///// <param name="treePId"></param>
        ///// <returns></returns>
        //int CountByTreePId(object treePId);

        /// <summary>
        /// 递归查找指定父索引的所有子节点。
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetByTreeParentIndex(string treeIndex, LoadOptions loadOptions = null);

        /// <summary>
        /// 获取指定节点的直接子节点。
        /// </summary>
        /// <param name="treePId"></param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetByTreePId(object treePId, LoadOptions loadOptions = null);

        /// <summary>
        /// 查询所有的根节点。
        /// 
        /// 与 GetAll 的区别在于：只查询所有的根节点，不查询子节点。
        /// </summary>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        IEntityList GetTreeRoots(LoadOptions loadOptions = null);

        /// <summary>
        /// 查询某个实体的某个属性的值。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        object GetEntityValue(object id, IManagedProperty property);

        /// <summary>
        /// 递归加载某个节点的所有父节点。
        /// </summary>
        /// <param name="node"></param>
        void LoadAllTreeParents(Entity node);

        /// <summary>
        /// 把这个组件中的所有改动保存到仓库中。
        /// 
        /// <remarks>
        /// * 当本地保存时，方法返回的就是传入的实体。
        /// * 当客户端保存时，方法返回的是服务端保存后并向客户端回传的实体。
        ///     此时，会对传入的实体或列表进行融合 Id 的操作。
        ///     也就是说，在服务端生成的所有 Id 都会设置到参数实体中。
        ///     而服务端设置其它的属性则会被忽略，如果想要使用其它的属性，则可以从返回值中获取。
        ///     
        /// 在客户端调用本方法保存实体的同时，服务端会把服务端保存完毕后的实体数据传输回客户端，这样才能保证客户端的实体能获取服务端生成的 Id 数据。
        /// 如果希望不进行如何大数据量的传输，则尽量不要在客户端直接调用 Save 来进行实体的保存。（例如可以通过 Service 来定义数据的传输。）
        /// </remarks>
        /// </summary>
        /// <param name="component">需要保存的组件，可以是一个实体，也可以是一个实体列表。</param>
        /// <returns>
        /// 返回在仓库中保存后的实体。
        /// </returns>
        IDomainComponent Save(IDomainComponent component);

        /// <summary>
        /// 获取指定类型的仓库扩展。
        /// </summary>
        /// <typeparam name="TRepositoryExt"></typeparam>
        /// <returns></returns>
        TRepositoryExt Extension<TRepositoryExt>() where TRepositoryExt : class, IRepositoryExt;
    }

    /// <summary>
    /// 仓库扩展接口
    /// </summary>
    public interface IRepositoryExt
    {
        /// <summary>
        /// 对应的仓库类型。
        /// </summary>
        Type RepositoryType { get; }

        /// <summary>
        /// 被扩展的仓库
        /// </summary>
        IRepository Repository { get; }
    }

    /// <summary>
    /// ILazyProvider的抽象工厂
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// 用于查找指定实体的仓库。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="throwIfNotfound"></param>
        /// <returns></returns>
        IRepository FindByEntity(Type entityType, bool throwIfNotfound = false);

        /// <summary>
        /// 通过仓库类型查找指定的仓库。
        /// </summary>
        /// <param name="repoType"></param>
        /// <param name="throwIfNotfound"></param>
        /// <returns></returns>
        IRepository Find(Type repoType, bool throwIfNotfound = false);
    }

    /// <summary>
    /// 这个类主要用于依赖注入 <see cref="IRepositoryFactory"/>
    /// </summary>
    public static class RepositoryFactoryHost
    {
        internal const string DataPortalTargetFactoryName = "RepoFty";

        private static IRepositoryFactory _factory;

        /// <summary>
        /// 依赖注入的 <see cref="IRepositoryFactory"/>。
        /// </summary>
        public static IRepositoryFactory Factory
        {
            get => _factory;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _factory = value;

                //同时，需要注册到注册表中。
                if (value is IDataPortalTargetFactory)
                {
                    var portalFactory = value as IDataPortalTargetFactory;
                    if (portalFactory.Name != DataPortalTargetFactoryName) throw new InvalidOperationException();
                    DataPortalTargetFactoryRegistry.Register(portalFactory);
                }
            }
        }
    }

    internal interface IRepositoryInternal : IRepository, IEntityInfoHost
    {
        ///// <summary>
        ///// 实体对应的数据表的信息。
        ///// </summary>
        //IRdbTableInfo TableInfo { get; }

        /// <summary>
        /// 通过父对象获取子对象的集合。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        IEntityList GetLazyListByParent(Entity parent);

        IEntityList GetByIdOrTreePId(object id);

        //DbTable DbTable { get; }

        //SQLColumnsGenerator SQLColumnsGenerator { get; }

        //EntityQueryProvider LinqProvider { get; }

        //IEntityList QueryListByLinq(IQueryable queryable);
    }
}