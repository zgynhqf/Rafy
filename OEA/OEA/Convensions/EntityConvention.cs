/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100925
 * 说明：实体约束类，从Library中往下层移动到MetaModel中。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100925
 * 约定项添加RepositoryType 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SimpleCsla;
using System.Diagnostics;
using OEA.Threading;

namespace OEA
{
    /// <summary>
    /// 实体类的约定
    /// </summary>
    public static class EntityConvention
    {
        /// <summary>
        /// 通过实体类查找其对应的列表类。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Type ListType(Type entityType)
        {
            return FindByEntity(entityType).ListType;
        }

        /// <summary>
        /// 通过实体列表类查找其对应的实体类。
        /// </summary>
        /// <param name="listType"></param>
        /// <returns></returns>
        public static Type EntityType(Type listType)
        {
            return FindByList(listType).EntityType;
        }

        /// <summary>
        /// 通过实体类找到约定项
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static EntityMatrix FindByEntity(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException("entityType");

            var item = FastFindByEntityType(entityType);

            if (item == null)
            {
                var listType = Convetion_ListForEntity(entityType);
                var rpType = Convetion_RepositoryForEntity(entityType);
                item = new EntityMatrix(entityType, listType, rpType);
                Add(item);
            }

            return item;
        }

        /// <summary>
        /// 通过列表类找到约定项
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static EntityMatrix FindByList(Type listType)
        {
            if (listType == null) throw new ArgumentNullException("listType");

            var item = FastFindByListType(listType);

            if (item == null)
            {
                var entityType = Convetion_EntityForList(listType);
                var rpType = Convetion_RepositoryForEntity(entityType);
                item = new EntityMatrix(entityType, listType, rpType);
                Add(item);
            }

            return item;
        }

        /// <summary>
        /// 通过仓库类找到约定项
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static EntityMatrix FindByRepository(Type repositoryType)
        {
            if (repositoryType == null) return null;

            var item = FastFindByRepositoryType(repositoryType);

            if (item == null)
            {
                var entityType = Convetion_EntityForRepository(repositoryType);
                var listType = Convetion_ListForEntity(entityType);
                item = new EntityMatrix(entityType, listType, repositoryType);
                Add(item);
            }

            return item;
        }

        #region Convetion
        //以下四个方法通过命名约定找到对应的类型

        private static Type Convetion_ListForEntity(Type entityType)
        {
            string listTypeName = entityType.FullName + "s";
            Type listType = entityType.Assembly.GetType(listTypeName);

            //目前有些类型并不满足约定。
            if (listType == null)
            {
                listTypeName = entityType.FullName + "List";
                listType = entityType.Assembly.GetType(listTypeName);
            }

            //Criteria是没有对应的列表类型的
            //if (listType == null) throw new ArgumentException("没有找到与entityType对应的列表类型");

            return listType;
        }

        private static Type Convetion_EntityForList(Type listType)
        {
            Type entityType = null;

            if (listType.Name.EndsWith("s"))
            {
                var entityTypeName = listType.FullName.Substring(0, listType.FullName.Length - 1);
                entityType = listType.Assembly.GetType(entityTypeName);
            }

            if (entityType == null && listType.Name.EndsWith("List"))
            {
                var entityTypeName = listType.FullName.Substring(0, listType.FullName.Length - 4);
                entityType = listType.Assembly.GetType(entityTypeName);
            }

            if (entityType == null) throw new ArgumentException("没有找到与listType对应的实体类型");

            return entityType;
        }

        private static Type Convetion_RepositoryForEntity(Type entityType)
        {
            string rpName = entityType.FullName + "Repository";
            Type rpType = entityType.Assembly.GetType(rpName);
            return rpType;
        }

        private static Type Convetion_EntityForRepository(Type repositoryType)
        {
            Type entityType = null;

            if (repositoryType.Name.EndsWith("Repository"))
            {
                var entityTypeName = repositoryType.FullName.Substring(0, repositoryType.FullName.Length - "Repository".Length);
                entityType = repositoryType.Assembly.GetType(entityTypeName);
            }

            if (entityType == null) throw new ArgumentException(string.Format("没有找到与 {0} 对应的实体类型", repositoryType));

            return entityType;
        }

        #endregion

        #region 实现快速的、线程安全的读取方法
        //以下内容使用三个高速的字典类存储约定项，以方便查询。

        private static SimpleRWLock _rwLock = new SimpleRWLock();

        private static IDictionary<Type, EntityMatrix> _entityIndex;

        private static IDictionary<Type, EntityMatrix> _listIndex;

        private static IDictionary<Type, EntityMatrix> _repositoryIndex;

        static EntityConvention()
        {
            _entityIndex = new SortedDictionary<Type, EntityMatrix>(TypeNameComparer.Instance);
            _listIndex = new SortedDictionary<Type, EntityMatrix>(TypeNameComparer.Instance);
            _repositoryIndex = new SortedDictionary<Type, EntityMatrix>(TypeNameComparer.Instance);
        }

        private static EntityMatrix FastFindByEntityType(Type entityType)
        {
            using (_rwLock.BeginRead())
            {
                EntityMatrix item = null;
                _entityIndex.TryGetValue(entityType, out item);
                return item;
            }
        }

        private static EntityMatrix FastFindByListType(Type listType)
        {
            using (_rwLock.BeginRead())
            {
                EntityMatrix item = null;
                _listIndex.TryGetValue(listType, out item);
                return item;
            }
        }

        private static EntityMatrix FastFindByRepositoryType(Type repositoryType)
        {
            using (_rwLock.BeginRead())
            {
                EntityMatrix item = null;
                _repositoryIndex.TryGetValue(repositoryType, out item);
                return item;
            }
        }

        private static void Add(EntityMatrix item)
        {
            using (_rwLock.BeginWrite())
            {
                if (!_entityIndex.ContainsKey(item.EntityType))
                {
                    _entityIndex.Add(item.EntityType, item);
                    if (item.ListType != null)
                    {
                        _listIndex.Add(item.ListType, item);
                    }
                    if (item.RepositoryType != null)
                    {
                        _repositoryIndex.Add(item.RepositoryType, item);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 实体类型信息项
    /// </summary>
    public class EntityMatrix
    {
        public EntityMatrix(Type entity, Type list, Type repository)
        {
            this.EntityType = entity;
            this.ListType = list;
            this.RepositoryType = repository;
        }

        /// <summary>
        /// 实体类
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 实体类对应的列表类
        /// 
        /// Criteria是没有对应的列表类型的，所以这个属性可能为Null
        /// </summary>
        public Type ListType { get; private set; }

        /// <summary>
        /// 实体类对应的仓库类。
        /// 
        /// 如果找不到约定的仓库类，则这个属性为空。
        /// </summary>
        public Type RepositoryType { get; private set; }
    }
}
