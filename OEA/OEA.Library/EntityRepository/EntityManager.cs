/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：实体管理类，包括泛型版本和非泛型版本。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100920
 * 改为依赖Repository并构建在其之上，这个文件中的类算是API门户 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla.Core;
using System.Reflection;
using SimpleCsla;
using System.Diagnostics;
using System.Data;
using OEA.ORM;

using OEA.ORM.sqlserver;
using OEA.MetaModel;
using System.Linq.Expressions;
using OEA.Library.Caching;
using OEA.Utils;
using OEA.Library; using System.Runtime.Serialization;

namespace OEA
{
    /// <summary>
    /// Entity List Manager
    /// </summary>
    public static class ELM
    {
        #region Factory Methods

        /// <summary>
        /// 获取一个空的TCollection，准备加入数据库中存储的数据
        /// </summary>
        /// <returns></returns>
        public static TList GetList<TList>()
            where TList : EntityList
        {
            return Repository(typeof(TList)).CreateEmplyOldList() as TList;
        }

        /// <summary>
        /// 创建一个新的集合
        /// </summary>
        /// <returns></returns>
        public static TList NewList<TList>()
            where TList : EntityList
        {
            return Repository(typeof(TList)).NewList() as TList;
        }

        /// <summary>
        /// 此方法可以通过父对象的Id，从数据库中查询出所有的子对象。
        /// 返回出的孩子集合，父对象的外键需要进行手工设置。
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static TList GetByParent<TList>(Guid parentId)
            where TList : EntityList
        {
            return Repository(typeof(TList)).GetByParentId(parentId) as TList;
        }

        /// <summary>
        /// 此方法可以通过父对象，从数据库中查询出所有的子对象。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static TList GetByParent<TList>(Entity parent)
            where TList : EntityList
        {
            return Repository(typeof(TList)).GetByParent(parent) as TList;
        }

        /// <summary>
        /// 此方法返回所有的对象列表。
        /// </summary>
        /// <returns></returns>
        public static TList GetAll<TList>(bool withCache = true)
            where TList : EntityList
        {
            return Repository(typeof(TList)).GetAll(withCache) as TList;
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
            return Repository(oldList.GetType()).NewListOrderBy(oldList, keySelector);
        }

        internal static EntityRepository Repository(Type listType)
        {
            var entityType = EntityConvention.EntityType(listType);
            return RF.Create(entityType);
        }
    }

    /// <summary>
    /// Entity Manager
    /// </summary>
    public static class EM
    {
        #region Factory Method

        /// <summary>
        /// 通过Id获取对象。
        /// 这里是完全的懒加载，只会加载对象本身的属性，不会加载子对象和外键。
        /// 如果需要同时加载别的数据，请按以下规则重写方法：
        /// 如果loadAsChild为真，则只需要重写ChildFetch(T data)；否则重写DataPortal_Fetch(T data)方法。
        /// 
        /// 根据静态属性AsRoot来判断，是否需要加载为子对象。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Entity GetById(Type entityType, Guid id, bool withCache = true)
        {
            return Repository(entityType).GetById(id, withCache);
        }

        /// <summary>
        /// 把数据行转换为实体对象
        /// 返回的对象的属性只是简单的完全Copy参数data的数据。
        /// 注意，此方法只能在服务端调用。
        /// </summary>
        /// <param name="row">
        /// 目前的行数据也是没有类型T来存储。
        /// </param>
        /// <returns></returns>
        public static Entity Convert(Entity row)
        {
            return Repository(row.GetType()).Convert(row);
        }

        /// <summary>
        /// 创建一个新的实体
        /// </summary>
        /// <returns></returns>
        public static Entity New(Type entityType)
        {
            return Repository(entityType).New();
        }

        #endregion

        #region 聚合关系

        public static bool IsRootType(Type entityType)
        {
            return Repository(entityType).IsRootType();
        }

        #endregion

        #region 聚合SQL

        public static Entity ReadDataDirectly(Type entityType, DataRow rowData)
        {
            return Repository(entityType).SQLColumnsGenerator.ReadDataDirectly(rowData);
        }

        public static string GetReadableColumnsSql(Type entityType, string tableAlias)
        {
            return Repository(entityType).SQLColumnsGenerator.GetReadableColumnsSql(tableAlias);
        }

        public static string GetReadableColumnSql(Type entityType, string columnName)
        {
            return Repository(entityType).SQLColumnsGenerator.GetReadableColumnSql(columnName);
        }

        #endregion

        internal static EntityRepository Repository(Type entityType)
        {
            return RF.Create(entityType);
        }
    }

    /// <summary>
    /// 泛型版本的Entity Manager。
    /// 此类主要方便用户在知道具体类型的情况下，以强类型方式访问一些方法。
    /// 同时用于存储特定实体类型的一些信息，并能快速寻址。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EM<T>
        where T : Entity
    {
        #region Factory Method

        /// <summary>
        /// 通过Id获取对象。
        /// 这里是完全的懒加载，只会加载对象本身的属性，不会加载子对象和外键。
        /// 如果需要同时加载别的数据，请按以下规则重写方法：
        /// 如果loadAsChild为真，则只需要重写ChildFetch(T data)；否则重写DataPortal_Fetch(T data)方法。
        /// 
        /// 根据静态属性AsRoot来判断，是否需要加载为子对象。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T GetById(Guid id, bool withCache = true)
        {
            return EM.GetById(typeof(T), id, withCache) as T;
        }

        /// <summary>
        /// 把数据行转换为实体对象
        /// </summary>
        /// <param name="row">
        /// 目前的行数据也是没有类型T来存储。
        /// </param>
        /// <returns></returns>
        public static T Convert(T row)
        {
            return EM.Convert(row) as T;
        }

        /// <summary>
        /// 创建一个新的实体
        /// </summary>
        /// <returns></returns>
        public static T New()
        {
            return EM.New(typeof(T)) as T;
        }

        #endregion

        #region 聚合关系

        /// <summary>
        /// 找到本对象上层父聚合对象的外键
        /// </summary>
        /// <returns></returns>
        internal static EntityPropertyMeta GetParentPropertyInfo()
        {
            return EM.Repository(typeof(T)).GetParentPropertyInfo();
        }

        internal static bool AsRoot
        {
            get
            {
                return EM.Repository(typeof(T)).IsRootType();
            }
        }

        #endregion

        #region 聚合SQL

        /// <summary>
        /// 直接从数据集中取数据。
        /// 
        /// 注意：
        /// 数据集中的列字段约定为：“表名_列名”，如“PBS_Name”。
        /// 默认使用反射创建对象并读取数据！同“LiteORM”。
        /// 
        /// 意义：
        /// 由于各个类的列名不再相同，所以这个方法的意义在于可以使用一句复杂的组合SQL加载一个聚合对象！
        /// </summary>
        /// <param name="rowData">
        /// 这个数据集中的列字段约定为：“表名_列名”，如“PBS_Name”。
        /// </param>
        /// <returns>
        /// 如果id值为null，则返回null。
        /// </returns>
        public static T ReadDataDirectly(DataRow rowData)
        {
            return EM.ReadDataDirectly(typeof(T), rowData) as T;
        }

        /// <summary>
        /// 获取可用于ReadDirectly方法读取的列名表示法。如：
        /// PBS.Id as PBS_Id, PBS.Name as PBS_Name, ........
        /// </summary>
        /// <returns></returns>
        public static string GetReadableColumnsSql()
        {
            return EM.GetReadableColumnsSql(typeof(T), null);
        }

        /// <summary>
        /// 获取可用于ReadDirectly方法读取的列名表示法。如：
        /// p.Id as PBS_Id, p.Name as PBS_Name, ........
        /// </summary>
        /// <param name="tableAlias">表的别名</param>
        /// <returns></returns>
        public static string GetReadableColumnsSql(string tableAlias)
        {
            return EM.GetReadableColumnsSql(typeof(T), tableAlias);
        }

        /// <summary>
        /// 获取columnName在DataRow中使用时约定的列名。
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetReadableColumnSql(string columnName)
        {
            return EM.GetReadableColumnSql(typeof(T), columnName);
        }

        #endregion
    }
}
