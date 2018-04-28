/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101101
 * 说明：所有仓库类的基类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using Rafy;
using Rafy.Data;
using Rafy.Reflection;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Linq;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Utils;
using Rafy.Utils.Caching;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库类
    /// 用于某个实体类型及其实体列表类的管理
    /// 注意：
    /// 1. 其子类必须是线程安全的！
    /// 2. 子类的构建函数建议使用protected，不要向外界暴露。使用者只能全部通过仓库工厂获取。
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public abstract partial class EntityRepository : EntityRepositoryQueryBase,
        IRepository, IRepositoryInternal, IEntityInfoHost, ITypeValidationsHost
    {
        #region 创建实体/列表

        /// <summary>
        /// 创建一个新的实体。
        /// 
        /// 如果在已经获取 Repository 的场景下，使用本方法返回的实体会设置好内部的 Repository 属性，
        /// 这会使得 FindRepository、GetRepository 方法更加快速。
        /// </summary>
        /// <returns></returns>
        public Entity New()
        {
            var entity = Entity.New(this.EntityType);

            this.NotifyLoaded(entity);

            return entity;
        }

        /// <summary>
        /// 创建一个全新的列表
        /// </summary>
        /// <returns></returns>
        public EntityList NewList()
        {
            var list = NewListFast();

            this.NotifyLoaded(list);

            return list;
        }

        internal EntityList NewListFast()
        {
            return Activator.CreateInstance(this.ListType) as EntityList;
        }

        /// <summary>
        /// 把旧的实体列表中的实体按照一定的排序规则，排序后组装一个新的列表返回
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="oldList"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        internal EntityList NewListOrderBy<TKey>(IEnumerable<Entity> oldList, Func<Entity, TKey> keySelector)
        {
            if (oldList == null) throw new ArgumentNullException("oldList");

            var newList = this.NewList();

            newList.AddRange(oldList.OrderBy(keySelector));

            return newList;
        }

        #endregion

        #region EntityMatrix

        /// <summary>
        /// 被本仓库管理的列表类型
        /// </summary>
        /// <returns></returns>
        public Type ListType
        {
            get
            {
                var item = this.GetConventionItem();
                return item.ListType;
            }
        }

        /// <summary>
        /// 被本仓库管理的实体类型
        /// </summary>
        /// <returns></returns>
        public Type EntityType
        {
            get
            {
                var item = this.GetConventionItem();
                return item.EntityType;
            }
        }

        private EntityMatrix _cacheConvention;

        /// <summary>
        /// 获取当前的实体类型组合
        /// </summary>
        /// <returns></returns>
        protected EntityMatrix GetConventionItem()
        {
            if (this._cacheConvention == null)
            {
                this._cacheConvention = GetConventionItemCore();
            }
            return this._cacheConvention;
        }

        /// <summary>
        /// 查询实体类型组合
        /// </summary>
        /// <returns></returns>
        private EntityMatrix GetConventionItemCore()
        {
            //if (this.RealEntityType != null)
            //{
            //    return EntityMatrix.FindByEntity(this.RealEntityType);
            //}

            //默认使用约定查询出实体类型组合
            return EntityMatrix.FindByRepository(this.GetType());
        }

        ///// <summary>
        ///// 由于这个类是被多个实体类共用，所以需要带上这个字段以区分。
        ///// </summary>
        //internal Type RealEntityType;

        ///// <summary>
        ///// 判断当前的类型是否是默认的仓库
        ///// </summary>
        ///// <returns></returns>
        //public bool IsDefalutRepository()
        //{
        //    return this.RealEntityType != null;
        //}

        #endregion

        #region 仓库扩展

        private const BindingFlags _oneLevelFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// 本仓库对应所有插件的仓库扩展类实例。
        /// </summary>
        public IList<IRepositoryExt> Extensions { get; internal set; }

        /// <summary>
        /// 获取指定类型的仓库扩展。
        /// </summary>
        /// <typeparam name="TRepositoryExt"></typeparam>
        /// <returns></returns>
        public TRepositoryExt Extension<TRepositoryExt>() where TRepositoryExt : class, IRepositoryExt
        {
            var list = this.Extensions;
            for (int i = 0; i < list.Count; i++)
            {
                var ext = list[i];
                if (ext is TRepositoryExt) return ext as TRepositoryExt;
            }
            return null;
        }

        private EntityList GetByExtensions(object criteria)
        {
            var criteriaType = criteria.GetType();

            //当 Repository 被调用此参数对应的方法时，还应该检测仓库扩展中是否有已经定义的相应方法。
            foreach (var ext in this.Extensions)
            {
                var type = ext.GetType();
                var methods = type.GetMethods(_oneLevelFlags);
                foreach (var method in methods)
                {
                    if (method.Name == EntityConvention.GetByCriteriaMethod)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1)
                        {
                            var parameter = parameters[0];
                            if (parameter.ParameterType == criteriaType)
                            {
                                //方法找到，直接调用并返回。
                                var result = method.Invoke(ext, new object[] { criteria }) as EntityList;
                                return result;
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region 树型实体

        /// <summary>
        /// 对应的实体是否为树型实体
        /// </summary>
        public bool SupportTree
        {
            get { return this.EntityMeta.IsTreeEntity; }
        }

        /// <summary>
        /// 如果本仓库对应的实体是一个树型实体，那么这个属性表示这个实体使用的树型编号方案。
        /// </summary>
        public TreeIndexOption TreeIndexOption
        {
            get { return this.EntityMeta.TreeIndexOption ?? TreeIndexOption.Default; }
        }

        /// <summary>
        /// 递归加载某个节点的所有父节点。
        /// 使用此方法后，指定节点的父节点将被赋值到它的 TreeParent 属性上。
        /// </summary>
        /// <param name="node"></param>
        public void LoadAllTreeParents(Entity node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (node.TreePId != null)
            {
                var parents = this.GetAllTreeParents(node.TreeIndex);
                if (parents.Count > 0)
                {
                    //找到树中最下层的一个树节点。
                    var parent = parents[0];
                    while (true)
                    {
                        var tc = parent.TreeChildrenField;
                        if (tc != null && tc.Count == 1)
                        {
                            parent = tc[0];
                        }
                        else
                        {
                            break;
                        }
                    }

                    //由于索引并不一定真的准确，所以最后再做一次检测。
                    if (parent.Id.Equals(node.TreePId))
                    {
                        node.TreeParent = parent;
                    }
                }
            }
        }

        #endregion

        #region 实体元数据

        private bool _parentPropertyCacheLoaded;
        private EntityPropertyMeta _parentPropertyCache;
        /// <summary>
        /// 找到实体中对应聚合关系中的父实体引用属性元数据。
        /// 
        /// 注意，此函数返回的是引用实体属性，而非引用 Id 属性。
        /// </summary>
        /// <param name="throwOnNotFound"></param>
        /// <returns></returns>
        public EntityPropertyMeta FindParentPropertyInfo(bool throwOnNotFound = false)
        {
            if (!this._parentPropertyCacheLoaded)
            {
                var result = this.EntityMeta.FindParentReferenceProperty();
                if (result != null)
                {
                    this._parentPropertyCache = result;
                    this._parentPropertyCacheLoaded = true;
                }
            }

            if (this._parentPropertyCache == null && throwOnNotFound)
            {
                throw new NotSupportedException(this.EntityType + " 类型中没有注册引用类型为 ReferenceType.Parent 的父引用属性。");
            }

            return this._parentPropertyCache;
        }

        private IList<IProperty> _redundancies;
        /// <summary>
        /// 所有本实体中所有声明的冗余属性。
        /// </summary>
        /// <returns></returns>
        public IList<IProperty> GetPropertiesInRedundancyPath()
        {
            if (this._redundancies == null)
            {
                this._redundancies = this.EntityMeta.ManagedProperties.GetCompiledProperties()
                    .Cast<IProperty>()
                    .Where(p => p.IsInRedundantPath)
                    .ToArray();
            }

            return this._redundancies;
        }

        private EntityMeta _entityMeta;
        /// <summary>
        /// 对应的实体元数据
        /// </summary>
        public EntityMeta EntityMeta
        {
            get
            {
                if (_entityMeta == null)
                {
                    _entityMeta = CommonModel.Entities.Find(this.EntityType);
                    if (_entityMeta == null)
                    {
                        //标记了 RootEntity/ChildEntity 的实体类应该有对应的元数据。
                        throw new InvalidProgramException(string.Format("没有找到类型 {0} 对应的元数据。", this.EntityType.Name));
                    }
                }

                return _entityMeta;
            }
        }

        private IList<IProperty> _childProperties;
        /// <summary>
        /// 所有本实体中所有声明的子属性。
        /// 
        /// 每一个子属性值可能是一个列表，也可能是一个单一实体。
        /// </summary>
        /// <returns></returns>
        public IList<IProperty> GetChildProperties()
        {
            if (_childProperties == null)
            {
                _childProperties = this.EntityMeta.ManagedProperties.GetCompiledProperties()
                    .Where(p =>
                    {
                        if (p is IListProperty)
                        {
                            var lp = p as IListProperty;
                            if (lp.HasManyType == HasManyType.Composition) { return true; }
                        }
                        else if (p is IRefEntityProperty)
                        {
                            var rp = p as IRefEntityProperty;
                            if (rp.ReferenceType == ReferenceType.Child) { return true; }
                        }
                        return false;
                    })
                    .Cast<IProperty>()
                    .ToArray();
            }

            return _childProperties;
        }

        /// <summary>
        /// 把指定实体的所有组合子实体都加载到内存中。（非递归）
        /// </summary>
        /// <param name="entity"></param>
        public void LoadAllChildren(Entity entity)
        {
            var childProperties = this.GetChildProperties();
            for (int i = 0, c = childProperties.Count; i < c; i++)
            {
                var childProperty = childProperties[i] as IListProperty;
                if (childProperty != null)
                {
                    //不论这个列表属性是否已经加载，都必须获取其所有的数据行，并标记为删除。
                    entity.GetLazyList(childProperty);
                }
            }
        }

        /// <summary>
        /// 这个字段用于存储运行时解析出来的表的信息。
        /// </summary>
        private IPersistanceTableInfo _tableInfo;

        internal IPersistanceTableInfo TableInfo
        {
            get
            {
                if (_tableInfo == null && this.EntityMeta != null)
                {
                    _tableInfo = PersistanceTableInfoFactory.CreateTableInfo(this);
                }

                return _tableInfo;
            }
        }

        IPersistanceTableInfo IRepositoryInternal.TableInfo
        {
            get { return this.TableInfo; }
        }

        #endregion

        #region 验证

        ValidationRulesManager ITypeValidationsHost.Rules { get; set; }

        bool ITypeValidationsHost.TypeRulesAdded { get; set; }

        #endregion

        #region 其它方法

        /// <summary>
        /// 创建一个列表。
        /// 列表的数据来自于 srcList 中的所有项。
        /// </summary>
        /// <param name="srcList"></param>
        /// <returns></returns>
        public EntityList CreateList(IEnumerable srcList)
        {
            return this.CreateList(srcList, false);
        }

        /// <summary>
        /// 创建一个列表。
        /// 列表的数据来自于 srcList 中的所有项。
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="resetParent">此参数表示是否需要把 srcList 中的每一个实体的 <see cref="IEntity.ParentList"/> 属性设置为新列表，并把实体的 ParentEntity 也设置为新列表的父实体。</param>
        /// <returns></returns>
        public EntityList CreateList(IEnumerable srcList, bool resetParent)
        {
            var list = NewListFast();

            list.ResetItemParent = resetParent;

            list.LoadData(srcList);

            this.NotifyLoaded(list);

            return list;
        }

        /// <summary>
        /// 把整个聚合对象的 Id 设置完整。
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        private static void MergeIdRecur(Entity oldEntity, Entity newEntity)
        {
            oldEntity.LoadProperty(Entity.IdProperty, newEntity.GetProperty(Entity.IdProperty));

            foreach (var field in oldEntity.GetLoadedChildren())
            {
                var listProperty = field.Property as IListProperty;
                if (listProperty != null)
                {
                    var oldChildren = field.Value as EntityList;
                    var newChildren = newEntity.GetLazyList(listProperty) as EntityList;

                    //两个集合应该是一样的数据、顺序？如果后期出现 bug，则修改此处的逻辑为查找到对应项再修改。
                    for (int i = 0, c = oldChildren.Count; i < c; i++)
                    {
                        MergeIdRecur(oldChildren[i], newChildren[i]);
                    }

                    //同步组合父对象 Id
                    oldChildren.SyncParentEntityId(oldEntity);

                    //同步 TreePId
                    if (oldChildren.SupportTree)
                    {
                        for (int i = 0, c = oldChildren.Count; i < c; i++)
                        {
                            var oldChild = oldChildren[i];
                            var newChild = newChildren[i];
                            oldChild.TreePId = newChild.TreePId;
                        }
                    }
                }
            }
        }

        #endregion
    }
}