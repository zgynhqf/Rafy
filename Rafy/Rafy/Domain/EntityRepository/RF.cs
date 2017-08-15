/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101101
 * 说明：Repository Factory的API门户
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Data;
using System.Diagnostics;
using System.Runtime;
using Rafy.Data;
using Rafy.Domain.ORM;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库门面 API（快捷方式：<see cref="RF"/> 类型）
    /// 
    /// 封装了一些静态代理方法的实体分为代理类。
    /// 主要是方便上层的调用。
    /// </summary>
    public abstract class RepositoryFacade
    {
        /// <summary>
        /// 用于查找指定实体的仓库。
        /// </summary>
        /// <returns></returns>
        public static EntityRepository Find(Type entityType)
        {
            return RepositoryFactoryHost.Factory.FindByEntity(entityType) as EntityRepository;
        }

        /// <summary>
        /// 用于查找指定实体的仓库。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static EntityRepository Find<TEntity>()
            where TEntity : Entity
        {
            return HostByEntity<TEntity>.Instance;
        }

        /// <summary>
        /// 用于查找指定类型的仓库。
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        public static TRepository ResolveInstance<TRepository>()
            where TRepository : EntityRepository
        {
            return HostByRepo<TRepository>.Instance;
        }

        /// <summary>
        /// 用于查找指定类型的仓库。
        /// </summary>
        /// <param name="repositoryType">Type of the repository.</param>
        /// <returns></returns>
        public static EntityRepository ResolveInstance(Type repositoryType)
        {
            return RepositoryFactoryHost.Factory.Find(repositoryType) as EntityRepository;
        }

        /// <summary>
        /// 用于查找指定类型的仓库。
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        [Obsolete("Concrete 方法已过时，请用 ResolveInstance 方法替换")]
        public static TRepository Concrete<TRepository>()
            where TRepository : EntityRepository
        {
            return HostByRepo<TRepository>.Instance;
        }

        #region 类型高速键值对

        private class HostByRepo<TRepository>
            where TRepository : EntityRepository
        {
            public static readonly TRepository Instance = RepositoryFactoryHost.Factory.Find(typeof(TRepository)) as TRepository;
        }

        private class HostByEntity<TEntity>
            where TEntity : Entity
        {
            public static readonly EntityRepository Instance = RepositoryFactoryHost.Factory.FindByEntity(typeof(TEntity)) as EntityRepository;
        }

        #endregion

        #region Shortcuts

        /// <summary>
        /// 保存某个实体列表。
        /// </summary>
        /// <param name="entityList">The entity list.</param>
        public static void Save(EntityList entityList)
        {
            entityList.GetRepository().Save(entityList);
        }

        /// <summary>
        /// 保存某个实体。
        /// </summary>
        /// <param name="entity"></param>
        public static void Save(Entity entity)
        {
            Save(entity, EntitySaveType.Normal);
        }

        /// <summary>
        /// 保存某个实体。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveWay"></param>
        public static void Save(Entity entity, EntitySaveType saveWay)
        {
            var repo = entity.GetRepository();

            if (saveWay == EntitySaveType.DiffSave && repo.DataPortalLocation == DataPortalLocation.Dynamic)
            {
                var isNew = entity.IsNew;
                var res = DiffSaveService.Execute(entity);
                if (isNew) { entity.Id = res.NewId; }
            }
            else
            {
                repo.Save(entity);
            }
        }

        /// <summary>
        /// 通过数据库配置名构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public static SingleConnectionTransactionScope TransactionScope(string dbSetting)
        {
            return new SingleConnectionTransactionScope(DbSetting.FindOrCreate(dbSetting));
        }

        /// <summary>
        /// 通过数据库配置名的代理：实体仓库，构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbDelegate"></param>
        /// <returns></returns>
        public static SingleConnectionTransactionScope TransactionScope(EntityRepository dbDelegate)
        {
            return new SingleConnectionTransactionScope(RdbDataProvider.Get(dbDelegate).DbSetting);
        }

        /// <summary>
        /// 通过数据库配置名的代理：实体仓库，构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public static SingleConnectionTransactionScope TransactionScope(DbSetting dbSetting)
        {
            return new SingleConnectionTransactionScope(dbSetting);
        }

        /// <summary>
        /// 通过数据库配置名的代理：实体仓库，构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbSetting">整个数据库的配置名</param>
        /// <param name="level">事务的孤立级别</param>
        /// <returns></returns>
        public static SingleConnectionTransactionScope TransactionScope(DbSetting dbSetting, IsolationLevel level)
        {
            return new SingleConnectionTransactionScope(dbSetting);
        }

        /// <summary>
        /// 申明一个实体上下文操作代码块。
        /// </summary>
        /// <returns></returns>
        public static IDisposable EnterEntityContext()
        {
            return EntityContext.Enter();
        }

        /// <summary>
        /// 申明一个禁用了实体上下文操作代码块。
        /// </summary>
        /// <returns></returns>
        public static IDisposable DisableEntityContext()
        {
            return EntityContext.Disable();
        }

        ///// <summary>
        ///// 把这个组件中的所有改动保存到仓库中，并同时把参数替换为从仓库（服务端）返回的实体。
        ///// </summary>
        ///// <param name="component">
        ///// 传入参数：需要保存的实体/实体列表。
        ///// 传出结果：保存完成后的实体/实体列表。注意，它与传入的对象并不是同一个对象。
        ///// </param>
        //public static void SaveReplace<T>(ref T component)
        //    where T : class, IDomainComponent
        //{
        //    component = component.GetRepository().Save(component) as T;
        //}

        #endregion

        #region 私有化构造器

        internal RepositoryFacade() { }

        #endregion

        //#region 聚合SQL

        //public static TEntity ReadDataDirectly<TEntity>(DataRow rowData)
        //    where TEntity : Entity
        //{
        //    return Create<TEntity>().GetFromRow(rowData) as TEntity;
        //}

        //public static string GetReadableColumnsSql(Type entityType, string tableAlias)
        //{
        //    return Create(entityType).SQLColumnsGenerator.GetReadableColumnsSql(tableAlias);
        //}

        //public static string GetReadableColumnSql(Type entityType, string columnName)
        //{
        //    return Create(entityType).SQLColumnsGenerator.GetReadableColumnSql(columnName);
        //}

        //#endregion

        ///// <summary>
        ///// 把指定的oldList按照keySelector进行排序，并返回一个新的排序后的列表
        ///// </summary>
        ///// <typeparam name="TKey"></typeparam>
        ///// <param name="oldList"></param>
        ///// <param name="keySelector"></param>
        ///// <returns></returns>
        //public static EntityList NewChildOrderBy<TKey>(EntityList oldList, Func<Entity, TKey> keySelector)
        //{
        //    return oldList.GetRepository().CastTo<EntityRepository>().NewListOrderBy(oldList, keySelector);
        //}
    }

    /// <summary>
    /// 仓库门面 API
    /// <see cref="RepositoryFacade"/> 的缩写（快捷写法）
    /// </summary>
    public abstract class RF : RepositoryFacade
    {
        internal RF() { }
    }

    /// <summary>
    /// 实体保存时的类型
    /// </summary>
    public enum EntitySaveType
    {
        /// <summary>
        /// 一般保存
        /// </summary>
        Normal,
        /// <summary>
        /// 差异保存
        /// </summary>
        DiffSave
    }
}