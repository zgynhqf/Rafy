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
using OEA.MetaModel;
using OEA.ORM;
using OEA.ManagedProperty;
using OEA.Library.Validation;

namespace OEA.Library
{
    /// <summary>
    /// 实体类模块使用的抽象的懒加载提供器。
    /// 
    /// 实体类只依赖这个抽象类，而不依赖具体的提供方案。
    /// </summary>
    public interface IRepository : IDbFactory, IEntityInfoHost
    {
        /// <summary>
        /// 该实体的仓库。
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 是否声明本仓库为本地仓库（客户端只在客户端查询，服务端在服务端查询）
        /// </summary>
        DataPortalLocation DataPortalLocation { get; }

        /// <summary>
        /// 通过Id获取指定的实体对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Entity GetById(int id);

        /// <summary>
        /// 查询所有的实体类
        /// </summary>
        /// <param name="withCache">是否优先使用缓存中的数据</param>
        /// <returns></returns>
        EntityList GetAll(bool withCache = true);

        /// <summary>
        /// 创建一个全新的实体对象
        /// </summary>
        /// <returns></returns>
        Entity New();

        /// <summary>
        /// 把行转换为对象
        /// 
        /// （复制出一个新的实体对象）
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        Entity ConvertRow(Entity row);

        /// <summary>
        /// 创建一个全新的实体列表对象
        /// </summary>
        /// <returns></returns>
        EntityList NewList();

        /// <summary>
        /// 通过父对象获取子对象的集合。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        EntityList GetByParent(Entity parent);

        /// <summary>
        /// 递归查找所有树型子
        /// </summary>
        /// <param name="treeCode"></param>
        /// <returns></returns>
        EntityList GetByTreeParentCode(string treeCode);

        void AddBatch(IList<Entity> entityList);

        /// <summary>
        /// 把这个组件中的所有改动保存到仓库中。
        /// 
        /// 方法返回保存结束的结果对象（保存可能会涉及到跨网络，所以服务端传输回来的值并不是之前传入的对象。）
        /// </summary>
        /// <param name="component"></param>
        /// <param name="markOld">是否需要把参数对象保存为未修改状态</param>
        /// <returns></returns>
        IEntityOrList Save(IEntityOrList component, bool markOld = true);

        #region 一些其它的元数据获取方法

        bool SupportTree { get; }

        TreeCodeOption TreeCodeOption { get; }

        #endregion
    }

    /// <summary>
    /// ILazyProvider的抽象工厂
    /// </summary>
    internal interface IRepositoryFactory
    {
        /// <summary>
        /// 创建一个对应实体类型的LazyProvider
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IRepository Create(Type entityType);
    }

    /// <summary>
    /// 这个类主要用于依赖注入ILazyProviderFactory
    /// </summary>
    internal static class RepositoryFactoryHost
    {
        /// <summary>
        /// 依赖注入的ILazyProviderFactory。
        /// </summary>
        public static IRepositoryFactory Factory;
    }
}