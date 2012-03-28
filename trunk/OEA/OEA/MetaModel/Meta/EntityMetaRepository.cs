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
using OEA.MetaModel.Attributes;
using OEA.Utils;
using OEA.ManagedProperty;
using OEA.Library;

namespace OEA.MetaModel
{
    /// <summary>
    /// 实体默认视图及实体信息的仓库
    /// </summary>
    public class EntityMetaRepository : MetaRepositoryBase<EntityMeta>
    {
        #region 内部接口 - 初始化

        /// <summary>
        /// 添加一个原始的实体类型
        /// </summary>
        /// <param name="entityType"></param>
        internal void AddRootPrime(Type entityType)
        {
            if (!this.IsOverrided(entityType))
            {
                this.CreateEntityMetaRecur(entityType);
            }
        }

        #endregion

        #region 外部接口 - 实体覆盖

        private Dictionary<Type, Type> _overriding = new Dictionary<Type, Type>(10);

        /// <summary>
        /// 使用子类完全覆盖父类。
        /// 一般用于客户化的扩展。
        /// </summary>
        /// <typeparam name="TSubclass"></typeparam>
        public void OverrideBase<TSubclass>()
        {
            var type = typeof(TSubclass);
            this._overriding[type.BaseType] = type;
        }

        /// <summary>
        /// 获取所有的客户化信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<Type, Type>> EnumerateAllOverriding()
        {
            return this._overriding;
        }

        /// <summary>
        /// 如果设置了覆盖，则返回覆盖的子类。
        /// </summary>
        /// <param name="entityType"></param>
        public void ReplaceIfOverrided(ref Type entityType)
        {
            Type subType;
            if (this._overriding.TryGetValue(entityType, out subType))
            {
                entityType = subType;
            }
        }

        internal bool IsOverrided(Type entityType)
        {
            return this._overriding.ContainsKey(entityType);
        }

        #endregion

        #region 外部接口 - 查询

        /// <summary>
        /// 查询某个实体类型所对应的实体信息
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public EntityMeta Get(Type entityType)
        {
            ReplaceIfOverrided(ref entityType);

            EntityMeta result = this.Find(entityType);

            if (result == null) throw new ArgumentNullException("没有找到这个类型的实体：" + entityType);

            return result;
        }

        public EntityMeta Find(Type entityType)
        {
            return this.FirstOrDefault(em => em.EntityType == entityType);
        }

        #endregion

        #region 内部接口 - 扩展属性

        public EntityPropertyMeta CreateExtensionPropertyMeta(IManagedProperty mp, EntityMeta em)
        {
            var epm = new EntityPropertyMeta()
            {
                Owner = em,
                Runtime = new ManagedPropertyRuntime(mp.GetMetaPropertyName(em.EntityType), mp)
            };
            em.EntityProperties.Add(epm);

            return epm;
        }

        #endregion

        #region CreateEntityMeta

        /// <summary>
        /// 创建一个实体类型的元数据信息。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parentMeta">
        /// 聚合父类型的元数据，如果 entityType 是根类型，则不需要传递此参数。
        /// </param>
        /// <returns></returns>
        private EntityMeta CreateEntityMetaRecur(Type entityType, EntityMeta parentMeta = null)
        {
            ReplaceIfOverrided(ref entityType);

            //由于一个类型可能挂接在多个类型下作为子类型，所以这里需要防止多次创建。
            var entityMeta = this.Find(entityType);
            if (entityMeta != null) return entityMeta;

            var boAttri = entityType.GetSingleAttribute<EntityAttribute>();
            if (boAttri == null) throw new ArgumentNullException(string.Format("{0} 没有标记 BusinessObjectAttribute", entityType.FullName));

            var instanceMeta = Activator.CreateInstance(entityType, true) as IEntityAttachedMeta;

            entityMeta = new EntityMeta
            {
                EntityType = entityType,
                IsTreeEntity = instanceMeta.SupportTree
            };

            //聚合关系设置
            if (parentMeta != null)
            {
                entityMeta.AggtParent = parentMeta;
                parentMeta.AggtChildren.Add(entityMeta);
            }

            #region EntityCategory

            if (boAttri is ConditionQueryEntityAttribute)
            {
                entityMeta.EntityCategory = EntityCategory.ConditionQueryObject;
            }
            else if (boAttri is NavigateQueryEntityAttribute)
            {
                entityMeta.EntityCategory = EntityCategory.NavigateQueryObject;
            }
            else if (boAttri is RootEntityAttribute)
            {
                entityMeta.EntityCategory = EntityCategory.Root;
            }
            else
            {
                entityMeta.EntityCategory = EntityCategory.Child;
            }

            #endregion

            #region TableAttribute

            var tableAttr = entityType.GetSingleAttribute<TableAttribute>();
            if (tableAttr != null)
            {
                string name = tableAttr.Name;
                if (string.IsNullOrEmpty(name)) name = entityType.Name;

                entityMeta.TableMeta = new TableMeta(name)
                {
                    SupportMigrating = tableAttr.SupprtMigrating
                };
            }

            #endregion

            this.CreatePropertiesMeta(entityMeta);

            if (entityMeta.IsTreeEntity)
            {
                var p = entityMeta.Property(DBConvention.FieldName_TreeCode);
                if (p != null) { entityMeta.DefaultOrderBy = p; }
            }

            this.AddPrime(entityMeta);

            this.Config(entityMeta);

            return entityMeta;
        }

        /// <summary>
        /// 调用配置类进行配置。
        /// </summary>
        /// <param name="em"></param>
        private void Config(EntityMeta em)
        {
            foreach (var config in OEAEnvironment.FindConfigurations(em.EntityType))
            {
                config.Meta = em;
                config.ConfigMeta();
            }
        }

        /// <summary>
        /// 聚合父类型的元数据，如果 entityType 是根类型，则不需要传递此参数。
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <param name="parentMeta">
        /// 聚合父类型的元数据，如果 entityType 是根类型，则不需要传递此参数。
        /// </param>
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

            //加入扩展属性元数据
            foreach (var mp in EntityMetaHelper.GetEntityPropertiesExtension(entityMeta))
            {
                this.CreateExtensionPropertyMeta(mp, entityMeta);
            }
        }

        private EntityPropertyMeta CreateEntityPropertyMeta(PropertySource propertySource, EntityMeta entityMeta)
        {
            var runtimeProperty = propertySource.CLR;

            var item = new EntityPropertyMeta
            {
                Owner = entityMeta,
                Runtime = new CLRPropertyRuntime(runtimeProperty),
                ManagedProperty = propertySource.MP
            };

            #region 创建 ColumnMeta

            var columnAttri = runtimeProperty.GetSingleAttribute<ColumnAttribute>();
            if (columnAttri != null)
            {
                var name = columnAttri.ColumnName;
                if (string.IsNullOrWhiteSpace(name)) name = runtimeProperty.Name;

                item.ColumnMeta = new ColumnMeta
                {
                    ColumnName = name,
                    IsPK = runtimeProperty.HasMarked<PKAttribute>()
                };
            }

            #endregion

            #region 创建 ReferenceInfo 及聚合子类

            ReferenceInfo ri = null;

            var lookupAttri = runtimeProperty.GetSingleAttribute<LookupAttribute>();
            var refMP = propertySource.MP as IOEARefProperty;
            if (refMP != null)
            {
                var refMeta = refMP.GetMeta(entityMeta.EntityType);
                ri = new ReferenceInfo()
                {
                    Type = refMeta.ReferenceType,
                    RefEntityProperty = refMeta.RefEntityProperty
                };
                if (lookupAttri != null) { ri.RefType = lookupAttri.LookupType; }
            }
            else if (lookupAttri != null)
            {
                ri = new ReferenceInfo()
                {
                    Type = lookupAttri.ReferenceType,
                    RefEntityProperty = lookupAttri.LookupPropertyName,
                    RefType = lookupAttri.LookupType
                };
            }

            if (ri != null)
            {
                this.CreateChildReference(ri, entityMeta);

                item.ReferenceInfo = ri;
            }

            #endregion

            entityMeta.EntityProperties.Add(item);

            return item;
        }

        private void CreateChildReference(ReferenceInfo ri, EntityMeta entityMeta)
        {
            if (ri.RefType == null && !string.IsNullOrEmpty(ri.RefEntityProperty))
            {
                var lookupInfo = entityMeta.EntityType.GetProperty(ri.RefEntityProperty);
                if (lookupInfo == null) throw new ArgumentNullException("类型" + entityMeta.EntityType + "不存在Lookup属性" + ri.RefEntityProperty);

                ri.RefType = lookupInfo.PropertyType;
            }

            //如果它同时也是聚合子类，则也会被递归创建
            if (ri.Type == ReferenceType.Child)
            {
                ri.RefTypeMeta = this.CreateEntityMetaRecur(ri.RefType, entityMeta);
            }
            else
            {
                this.FireAfterAllPrimesReady(() =>
                {
                    ri.RefTypeMeta = this.Get(ri.RefType);
                });
            }
        }

        private ChildrenPropertyMeta CreateChildrenPropertyMeta(PropertySource propertySource, EntityMeta entityMeta)
        {
            var runtimeProperty = propertySource.CLR;

            var item = new ChildrenPropertyMeta
            {
                Owner = entityMeta,
                ManagedProperty = propertySource.MP,
                Runtime = new CLRPropertyRuntime(runtimeProperty)
            };

            var childType = EntityConvention.EntityType(runtimeProperty.PropertyType);
            var entityType = entityMeta.EntityType;
            if (childType != entityType)
            {
                item.ChildType = this.CreateEntityMetaRecur(childType, entityMeta);
            }
            else
            {
                item.ChildType = entityMeta;
            }

            entityMeta.ChildrenProperties.Add(item);

            return item;
        }

        #endregion
    }

    internal class PropertySource
    {
        public PropertyInfo CLR;
        public IManagedProperty MP;
    }
}