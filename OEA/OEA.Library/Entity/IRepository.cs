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
        Type EntityType { get; }

        /// <summary>
        /// 通过Id获取指定的实体对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Entity GetById(int id);

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
        /// 保存某个实体或实体列表。
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        IEntityOrList Save(IEntityOrList component);

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