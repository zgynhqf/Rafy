/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110314
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100314
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rafy;
using Rafy.Reflection;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;
using Rafy.Domain;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 实体默认视图及实体信息的仓库
    /// <remarks>
    /// 实体的元数据会在第一次被使用时而创建，并只会创建一次。
    /// </remarks>
    /// </summary>
    public class EntityMetaRepository : MetaRepositoryBase<EntityMeta>
    {
        #region 外部接口 - 查询

        /// <summary>
        /// 查询某个实体类型所对应的实体信息。查询不到，就报出异常。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public EntityMeta Get(Type entityType)
        {
            var result = this.Find(entityType);

            if (result == null) throw new InvalidProgramException("没有找到这个类型的实体：" + entityType + "，可能是没有标记 RootEntity/ChildEntity/Criteria 等标记。");

            return result;
        }

        private object _findLock = new object();

        /// <summary>
        /// 查询某个实体类型所对应的实体信息。
        /// 如果当前是第一次获取该实体的元数据，则会为它创建元数据。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public EntityMeta Find(Type entityType)
        {
            EntityMeta res = null;

            //实体类必须从 ManagedPropertyObject 上继承。
            if (entityType.IsSubclassOf(typeof(ManagedPropertyObject)))
            {
                res = this.FindCore(entityType);

                //如果当前还没有生成，则尝试为它创建元数据。
                if (res == null)
                {
                    lock (_findLock)
                    {
                        res = this.FindOrCreate(entityType);
                    }
                }
            }

            return res;
        }

        private EntityMeta FindOrCreate(Type entityType)
        {
            var res = this.FindCore(entityType);
            if (res == null)
            {
                ManagedPropertyRepository.RunPropertyResigtry(entityType);

                //如果是根类型，则直接生成该根类型的实体元数据
                if (RafyEnvironment.IsConcreteRootType(entityType))
                {
                    res = this.CreateEntityMetaRecur(entityType);
                }
                else
                {
                    //如果不是根类型，则应该先递归生成该根类型的整个组合元数据。
                    //这样，其中的子类型也就生成好了。这时再进行查询。
                    var rootType = RafyEnvironment.GetRootType(entityType);
                    if (rootType != null)
                    {
                        this.CreateEntityMetaRecur(rootType);
                        res = this.FindCore(entityType);
                    }
                }
            }

            return res;
        }

        private EntityMeta FindCore(Type entityType)
        {
            //性能不好，注释。
            //return this.FirstOrDefault(em => em.EntityType == entityType);

            var list = this.GetCurrentInnerList();
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.EntityType == entityType) return item;
            }
            return null;
        }

        #endregion

        #region CreateEntityMeta

        /// <summary>
        /// 元数据创建完成的事件。
        /// </summary>
        public event EventHandler<EntityMetaCreatedEventArgs> EntityMetaCreated;

        /// <summary>
        /// 创建一个实体类型的元数据信息。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parentMeta">
        /// 聚合父类型的元数据，如果 entityType 是根类型，则不需要传递此参数。
        /// </param>
        /// <returns></returns>
        private EntityMeta FindOrCreateEntityMetaRecur(Type entityType, EntityMeta parentMeta = null)
        {
            //由于一个类型可能挂接在多个类型下作为子类型，所以这里需要防止多次创建。
            var entityMetaFound = this.FindCore(entityType);
            if (entityMetaFound != null) return entityMetaFound;

            return this.CreateEntityMetaRecur(entityType, parentMeta);
        }

        private EntityMeta CreateEntityMetaRecur(Type entityType, EntityMeta parentMeta = null)
        {
            var boAttri = entityType.GetSingleAttribute<EntityAttribute>();
            if (boAttri == null) throw new ArgumentNullException(string.Format("{0} 没有标记 EntityAttribute", entityType.FullName));

            var entityMeta = new EntityMeta
            {
                EntityType = entityType
            };

            //标识属性的真实类型。
            var entity = Activator.CreateInstance(entityMeta.EntityType) as IEntityWithId;
            entityMeta.IdType = entity.IdProvider.KeyType;

            //聚合关系设置
            if (parentMeta != null)
            {
                entityMeta.AggtParent = parentMeta;
                parentMeta.AggtChildren.Add(entityMeta);
            }

            #region EntityCategory

            if (boAttri is QueryEntityAttribute)
            {
                entityMeta.EntityCategory = EntityCategory.QueryObject;
            }
            else if (boAttri is RootEntityAttribute)
            {
                entityMeta.EntityCategory = EntityCategory.Root;
            }
            else if (boAttri is ChildEntityAttribute)
            {
                entityMeta.EntityCategory = EntityCategory.Child;
            }
            else
            {
                throw new InvalidProgramException(string.Format("请为 {0} 类型标记以下标签：RootEntity/ChildEntity/QueryEntity。", entityType.FullName));
            }

            #endregion

            this.CreatePropertiesMeta(entityMeta);

            this.Config(entityMeta);

            var handler = this.EntityMetaCreated;
            if (handler != null) handler(this, new EntityMetaCreatedEventArgs(entityMeta));

            this.AddPrime(entityMeta);

            return entityMeta;
        }

        /// <summary>
        /// 调用配置类进行配置。
        /// </summary>
        /// <param name="em"></param>
        private void Config(EntityMeta em)
        {
            foreach (var config in RafyEnvironment.FindConfigurations(em.EntityType))
            {
                lock (config)
                {
                    config.Meta = em;
                    config.ConfigMeta();
                }
            }
        }

        private void CreatePropertiesMeta(EntityMeta entityMeta)
        {
            entityMeta.ManagedProperties = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(entityMeta.EntityType);

            //通过反射属性列表构建属性元数据列表
            foreach (var property in EntityMetaHelper.GetEntityProperties(entityMeta))
            {
                this.CreateEntityPropertyMeta(property, entityMeta);
            }

            foreach (var property in EntityMetaHelper.GetChildrenProperties(entityMeta))
            {
                this.CreateChildrenPropertyMeta(property, entityMeta);
            }
        }

        private EntityPropertyMeta CreateEntityPropertyMeta(IManagedProperty mp, EntityMeta entityMeta)
        {
            var propertyType = mp.PropertyType;
            if (mp == EntityConvention.Property_Id)
            {
                propertyType = entityMeta.IdType;
            }
            else if (mp == EntityConvention.Property_TreePId)
            {
                propertyType = entityMeta.IdType;
                if (!propertyType.IsClass)
                {
                    propertyType = typeof(Nullable<>).MakeGenericType(entityMeta.IdType);
                }
            }
            var item = new EntityPropertyMeta
            {
                Owner = entityMeta,
                Runtime = new ManagedPropertyRuntime(mp),
                PropertyType = propertyType,
                ManagedProperty = mp
            };

            #region 创建 ReferenceInfo 及聚合子类

            var refMP = mp as IRefEntityProperty;
            if (refMP != null)
            {
                var ri = new ReferenceInfo()
                {
                    RefEntityProperty = refMP
                };

                this.CreateReference(ri, entityMeta);

                item.ReferenceInfo = ri;
            }

            #endregion

            entityMeta.EntityProperties.Add(item);

            return item;
        }

        private void CreateReference(ReferenceInfo ri, EntityMeta entityMeta)
        {
            //如果它同时也是聚合子类，则也会被递归创建
            if (ri.Type == ReferenceType.Child)
            {
                ri.RefTypeMeta = this.FindOrCreateEntityMetaRecur(ri.RefType, entityMeta);
            }
            else
            {
                //以下代码转换为懒加载属性的模式。
                //this.FireAfterAllPrimesReady(() =>
                //{
                //    ri.RefTypeMeta = this.FindOrCreate(ri.RefType);
                //});
            }
        }

        private ChildrenPropertyMeta CreateChildrenPropertyMeta(IManagedProperty mp, EntityMeta entityMeta)
        {
            var item = new ChildrenPropertyMeta
            {
                Owner = entityMeta,
                ManagedProperty = mp,
                Runtime = new ManagedPropertyRuntime(mp),
                PropertyType = mp.PropertyType
            };

            var childType = EntityMatrix.FindByList(mp.PropertyType).EntityType;
            var entityType = entityMeta.EntityType;
            if (childType != entityType)
            {
                item.ChildType = this.FindOrCreateEntityMetaRecur(childType, entityMeta);
            }
            else
            {
                item.ChildType = entityMeta;
            }

            entityMeta.ChildrenProperties.Add(item);

            return item;
        }

        #endregion

        #region 全部加载

        private bool _allEntitiesLoaded;

        /// <summary>
        /// 调用此方法来加载整个系统中的所有插件中的实体元数据。
        /// （同时，也就包含所有实体的托管属性。）
        /// 
        /// 调用此方法前，遍历此集合只会返回当前已经加载的实体元数据。
        /// </summary>
        public void EnsureAllLoaded()
        {
            if (!this._allEntitiesLoaded)
            {
                var rootTypes = RafyEnvironment.SearchAllRootTypes();
                foreach (var rootType in rootTypes) { this.Find(rootType); }

                this._allEntitiesLoaded = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// 元数据创建完成的事件。
    /// </summary>
    public class EntityMetaCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMetaCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="meta">The meta.</param>
        internal EntityMetaCreatedEventArgs(EntityMeta meta)
        {
            this.EntityMeta = meta;
        }

        /// <summary>
        /// 创建完成的实体元数据。
        /// </summary>
        public EntityMeta EntityMeta { get; private set; }
    }
}