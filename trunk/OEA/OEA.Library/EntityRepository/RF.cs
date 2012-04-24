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

namespace OEA.Library
{
    /// <summary>
    /// Repository Factory的API门户
    /// 
    /// 封装了一些静态代理方法的实体分为代理类。
    /// 主要是方便上层的调用。
    /// </summary>
    public static class RF
    {
        /// <summary>
        /// 用于创建指定实体的仓库。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static EntityRepository Create<TEntity>()
            where TEntity : Entity
        {
            return RepositoryFactory.Instance.Find(typeof(TEntity));
        }

        /// <summary>
        /// 用于创建指定实体的仓库。
        /// </summary>
        /// <returns></returns>
        public static EntityRepository Create(Type entityType)
        {
            return RepositoryFactory.Instance.Find(entityType);
        }

        /// <summary>
        /// 用于创建指定类型的仓库。
        /// 
        /// 使用这个方法后，仓库工厂会缓存起这个仓库。
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        public static TRepository Concreate<TRepository>()
            where TRepository : EntityRepository
        {
            var entityType = EntityConvention.FindByRepository(typeof(TRepository)).EntityType;
            return RepositoryFactory.Instance.Find(entityType) as TRepository;
        }

        #region Shortcuts

        /// <summary>
        /// 获取一个空的TCollection，准备加入数据库中存储的数据
        /// </summary>
        /// <returns></returns>
        public static TList GetList<TList>()
            where TList : EntityList
        {
            var entityType = EntityConvention.EntityType(typeof(TList));
            return Create(entityType).NewList() as TList;
        }

        /// <summary>
        /// 创建一个新的集合
        /// </summary>
        /// <returns></returns>
        public static TList NewList<TList>()
            where TList : EntityList
        {
            var entityType = EntityConvention.EntityType(typeof(TList));
            return Create(entityType).NewList() as TList;
        }

        /// <summary>
        /// 保存某个实体的差异部分。
        /// </summary>
        /// <param name="entity"></param>
        public static void Save(EntityList entityList)
        {
            entityList.GetRepository().Save(entityList);
        }

        /// <summary>
        /// 保存某个实体的差异部分。
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
            if (saveWay == EntitySaveType.DiffSave)
            {
                var isNew = entity.IsNew;
                var res = DiffSaveService.Execute(entity);
                if (isNew) { entity.Id = res.NewId; }
            }
            else
            {
                entity.GetRepository().Save(entity);
            }
        }

        /// <summary>
        /// 通过数据库配置名构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <returns></returns>
        public static SingleConnectionTrasactionScope TransactionScope(string dbSetting)
        {
            return new SingleConnectionTrasactionScope(dbSetting);
        }

        /// <summary>
        /// 通过数据库配置名的代理：实体仓库，构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbDelegate"></param>
        /// <returns></returns>
        public static SingleConnectionTrasactionScope TransactionScope(EntityRepository dbDelegate)
        {
            return new SingleConnectionTrasactionScope(dbDelegate.ConnectionStringSettingName);
        }

        /// <summary>
        /// 通过数据库配置名的代理：实体，构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbDelegate"></param>
        /// <returns></returns>
        public static SingleConnectionTrasactionScope TransactionScope(Entity dbDelegate)
        {
            return new SingleConnectionTrasactionScope(dbDelegate.ConnectionStringSettingName);
        }

        /// <summary>
        /// 通过数据库配置名的代理：实体列表，构造一个 单连接事务块。
        /// </summary>
        /// <param name="dbDelegate"></param>
        /// <returns></returns>
        public static SingleConnectionTrasactionScope TransactionScope(EntityList dbDelegate)
        {
            return new SingleConnectionTrasactionScope(dbDelegate.GetRepository().ConnectionStringSettingName);
        }

        #endregion

        #region 聚合SQL

        public static TEntity ReadDataDirectly<TEntity>(DataRow rowData)
            where TEntity : Entity
        {
            return Create<TEntity>().GetFromRow(rowData) as TEntity;
        }

        public static string GetReadableColumnsSql(Type entityType, string tableAlias)
        {
            return Create(entityType).SQLColumnsGenerator.GetReadableColumnsSql(tableAlias);
        }

        public static string GetReadableColumnSql(Type entityType, string columnName)
        {
            return Create(entityType).SQLColumnsGenerator.GetReadableColumnSql(columnName);
        }

        #endregion

        /// <summary>
        /// 把指定的oldList按照keySelector进行排序，并返回一个新的排序后的列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="oldList"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static EntityList NewChildOrderBy<TKey>(EntityList oldList, Func<Entity, TKey> keySelector)
        {
            return oldList.GetRepository().CastTo<EntityRepository>().NewListOrderBy(oldList, keySelector);
        }
    }

    /// <summary>
    /// 实体保存时的类型
    /// </summary>
    public enum EntitySaveType
    {
        /// <summary>
        /// 差异保存
        /// </summary>
        DiffSave,

        /// <summary>
        /// 一般保存
        /// </summary>
        Normal
    }
}