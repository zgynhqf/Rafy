/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100330
 * 说明：对象实体，所有对象实体的基类
 * 运行环境：.NET 3.5 SP1
 * 版本号：2.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100330
 * 添加延迟外键 胡庆访 20100401
 * 自动获取属性，用于支持属性的复制 胡庆访 20100402
 * 改为非泛型的类 胡庆访 20100915
 * 添加FstField类 胡庆访 20101026
 * AutoCalcEntity 及 CalcEngine 中的代码移动到 GIX4.Library 中。Entity 则直接继承自 RafyEntity。胡庆访 20110314
 * 添加路由事件。胡庆访 20111108
 * ...
 * 支持多类型的标识属性（Int32、String、Guid、object）。胡庆访 20140510
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Transactions;
using Rafy;
using Rafy.Reflection;
using Rafy.Domain.Caching;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM;
using Rafy.Threading;

namespace Rafy.Domain
{
    /// <summary>
    /// 对象实体
    /// 继承此类，可以获得以下功能：
    /// * 自动管理托管属性，可以通过属性 FindRepository().GetAvailableIndicators() 获取到所有的托管属性。
    /// * 默认实现了所有数据库访问的方法：CDUQ。
    /// * 提供了延迟加载子对象集合的方法。
    /// * 提供了创建延迟外键对象的方法。
    /// * 提供实体验证规则框架。
    /// * 提供树型实体支持。
    /// * 支持聚合对象的路由事件。
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    [Serializable]
    public abstract partial class Entity : ManagedPropertyObject, IEntity, IRafyEntity
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Entity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        protected Entity()
        {
            //int 等类型的标识属性的默认值，不能是 null。
            //base.LoadProperty(IdProperty, KeyProvider.DefaultValue);

            //ValidationRules.Validate();

            //this.MarkNew();
        }

        /// <summary>
        /// Initializes the <see cref="Entity"/> class.
        /// </summary>
        static Entity()
        {
            //默认值最好是 0，这样开发人员比较理解。
            ////整型的主键，默认值是-1
            //if (typeof(TKey) == typeof(int) || typeof(TKey) == typeof(long))
            //{
            //    meta.DefaultValue = (TKey)(object)-1;
            //}

            //if (Entity.IdProperty != null)
            //{
            //    throw new InvalidProgramException(string.Format(
            //        "已经使用了 {0} 类型作为实体的主键。一个应用程序中，只能使用同一类型的主键。",
            //        Entity.IdProperty.PropertyType
            //        ));
            //}

            var meta = new PropertyMetadata<object>
            {
                CoerceGetValueCallBack = (o, v) => (o as Entity).CoerceGetId(v),
                PropertyChangingCallBack = (o, e) => (o as Entity).OnIdChanging(e),
                PropertyChangedCallBack = (o, e) => (o as Entity).OnIdChanged(e),
            };
            IdProperty = P<Entity>.Register(e => e.Id, meta);

            //静态属性注入
            EntityConvention.Property_Id = IdProperty;
            EntityConvention.Property_TreePId = TreePIdProperty;
            EntityConvention.Property_TreeIndex = TreeIndexProperty;
        }

        /// <summary>
        /// 通过实体类型反射构造一个新的实体。
        /// 
        /// 此方法功能与构造函数一致，主要用于不能显式调用的场景下。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static Entity New(Type entityType)
        {
            //经测试，Activator 创建对象也非常快，这是因为它的内部作了构造器缓存处理。
            return Activator.CreateInstance(entityType, true) as Entity;
        }

        #endregion

        #region 所属仓库

        [NonSerialized]
        private bool _repositoryLoaded;

        /// <summary>
        /// 实体对应的仓库。
        /// 
        /// 这个字段非常重要，这是因为实体很多逻辑的元数据信息，都存储在仓库中。
        /// </summary>
        [NonSerialized]
        private IRepository _repository;

        /// <summary>
        /// 尝试找到这个实体列表对应的仓库类。
        /// 
        /// 没有标记 RootEntity/ChildEntity 的类型是没有仓库类的，例如所有的条件类型。
        /// </summary>
        /// <returns></returns>
        public IRepository FindRepository()
        {
            if (!this._repositoryLoaded)
            {
                this._repository = RepositoryFactoryHost.Factory.FindByEntity(this.GetType());
                this._repositoryLoaded = true;
            }

            return this._repository;
        }

        /// <summary>
        /// 获取该实体列表对应的仓库类，如果没有找到，则抛出异常。
        /// </summary>
        /// <returns></returns>
        public IRepository GetRepository()
        {
            var repo = this.FindRepository();
            if (repo == null) throw new InvalidProgramException(string.Format("类型 {0} 没有对应的仓库类。", this.GetType().Name));

            return repo;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 实体的标识属性。
        /// 该属性由实体仓库内部自动编号。
        /// 
        /// 关于数据库映射：
        /// * Id 属性对应的列必须是自增长列。
        /// * 可以通过配置把 Id 属性对应的列更名。
        /// * Id 属性的默认数据库映射元数据是映射为主键的。这样在生成数据库时，引用本实体类型的其它的实体类型的引用属性都会添加相应的外键。
        ///     如果本属性被配置为不是主键时，则不会在数据库层面生成外键引用，但是实体间的引用关系依然存在。
        /// </summary>
        public static readonly Property<object> IdProperty;

        /// <summary>
        /// 强制获取 Id 逻辑。
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected virtual object CoerceGetId(object value) { return value; }

        /// <summary>
        /// Id 变更前事件。
        /// </summary>
        /// <param name="e">The decimal.</param>
        protected virtual void OnIdChanging(ManagedPropertyChangingEventArgs<object> e) { }

        /// <summary>
        /// Id 变更后事件。
        /// </summary>
        /// <param name="e">The <see cref="ManagedPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnIdChanged(ManagedPropertyChangedEventArgs e) { }

        /// <summary>
        /// 实体的标识属性。
        /// </summary>
        public object Id
        {
            get { return this.GetProperty(IdProperty); }
            set { this.SetProperty(IdProperty, value); }
        }

        /// <summary>
        /// 判断本实体是否已经拥有了可用的 Id 值。
        /// </summary>
        public bool HasId
        {
            get
            {
                return KeyProvider.HasId(this.Id);
            }
        }

        /// <summary>
        /// 获取实体所对应的属性容器。
        /// </summary>
        /// <returns></returns>
        protected override sealed ConsolidatedTypePropertiesContainer FindPropertiesContainer()
        {
            //使用 Repository 中缓存的容器覆盖基类接口，提升性能。
            if (_repository != null)
            {
                return _repository.EntityMeta.ManagedProperties;
            }

            return base.FindPropertiesContainer();
        }

        /// <summary>
        /// 实体标识属性的算法程序。
        /// </summary>
        public abstract IKeyProvider KeyProvider { get; }

        #endregion

        #region 值的复制

        /// <summary>
        /// 使用 <see cref="CloneOptions.ReadSingleEntity"/> 来复制目标对象。
        /// </summary>
        /// <param name="source"></param>
        public void Clone(Entity source)
        {
            this.CloneCore(source, CloneOptions.ReadSingleEntity());
        }

        /// <summary>
        /// 使用指定的复制条件来复制源对象的值。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="options"></param>
        public void Clone(Entity source, CloneOptions options)
        {
            this.CloneCore(source, options);
        }

        /// <summary>
        /// 复制目标对象的所有字段。
        /// 子类重写此方法额外做以下几件事：
        /// 1. 如果有自定义字段，请在此方法中进行值拷贝。
        /// 2. 如果有延迟加载的外键引用对象 ILazyEntityRef，请调用它的 Clone 方法进行拷贝。
        /// 3. 如果使用了快速字段 FastField 来进行属性的缓存，请在基类完成 Clone 后，调用本类的 ResetFastField 方法来清空缓存。
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">source
        /// or
        /// options</exception>
        protected virtual void CloneCore(Entity source, CloneOptions options)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (options == null) throw new ArgumentNullException("options");

            var grabChildren = options.HasAction(CloneActions.GrabChildren);
            var childrenRecur = options.HasAction(CloneActions.ChildrenRecur);
            var copyId = options.HasAction(CloneActions.IdProperty);

            var ingoreList = options.RetrieveIgnoreListOnce();

            //如果需要拷贝id，则应该先拷贝id，并立刻清空id的缓存。
            //注意：
            //由于 IdProperty 在 AllProperties 中的位置并不是第一个。所以会出现拷贝其它属性时，再次访问本ID导致缓存重建。
            //所以这里需要单独对 Id 进行一次拷贝。
            if (copyId) { this.CopyProperty(source, IdProperty, options); }

            //复制目标对象的所有托管属性。
            var allProperties = this.PropertiesContainer.GetAvailableProperties();
            for (int i = 0, c = allProperties.Count; i < c; i++)
            {
                var property = allProperties[i];
                if (property.IsReadOnly) continue;
                //var propertyMeta = property.GetMeta(this) as IPropertyMetadata;

                //过滤一些不需要拷贝的属性
                if (property == IdProperty && !copyId) continue;
                if (ingoreList != null && ingoreList.Contains(property)) continue;
                //已经更改了GetLazyChildren方法，不再考虑null值的拷贝。
                ////如果目标不存在这个值时，不需要也不能进行拷贝，否则会为懒加载属性的加载null值。
                //if (!target.FieldManager.FieldExists(propertyInfo)) continue;

                if (property is IListProperty)
                {
                    var listProperty = property as IListProperty;
                    if (childrenRecur)
                    {
                        var sourceList = source.GetLazyList(listProperty);
                        var targetList = this.GetLazyList(listProperty);
                        targetList.Clone(sourceList, options);

                        var isComposition = targetList.HasManyType == HasManyType.Composition;
                        if (isComposition) { targetList.SetParentEntity(this); }
                    }
                    else
                    {
                        if (grabChildren)
                        {
                            var children = source.GetProperty(property) as EntityList;
                            this.LoadProperty(property, children);
                            if (children == null) return;

                            var isComposition = children.HasManyType == HasManyType.Composition;
                            if (isComposition) { children.SetParentEntity(this); }
                        }
                    }
                }
                else
                {
                    this.CopyProperty(source, property, options);
                }
            }

            options.NotifyCloned(source, this);

            if (this.SupportTree) { this.OnTreeItemCloned(source, options); }
        }

        /// <summary>
        /// 复制指定的属性值。
        /// </summary>
        /// <param name="source">从这个对象拷贝</param>
        /// <param name="property">拷贝这个属性</param>
        /// <param name="options">The options.</param>
        private void CopyProperty(Entity source, IManagedProperty property, CloneOptions options)
        {
            var refIndicator = property as IRefEntityProperty;
            if (refIndicator != null)
            {
                bool copyEntity = refIndicator.ReferenceType == ReferenceType.Parent ?
                    options.HasAction(CloneActions.ParentRefEntity) :
                    options.HasAction(CloneActions.RefEntities);
                if (!copyEntity) { return; }
            }
            else
            {
                var value = source.GetProperty(property);
                if (options.Method == CloneValueMethod.LoadProperty)
                {
                    this.LoadProperty(property, value);
                }
                else
                {
                    this.SetProperty(property, value);
                }
            }
        }

        #endregion

        #region 预加载
        //暂时不支持。
        //这是因为直接在 Entity 上扩展这个字段，而又不是很常用，会比较浪费。
        //希望能够在 IManagedProperty 上扩展更好用的字段。

        ///// <summary>
        ///// 构建并获取某个子属性的预加载器
        ///// </summary>
        ///// <param name="childrenProperty"></param>
        ///// <returns></returns>
        //protected ForeAsyncLoader ChildrenLoader(IListProperty childrenProperty)
        //{
        //    ForeAsyncLoader result = null;

        //    if (this._allForeLoaders == null)
        //    {
        //        lock (this)
        //        {
        //            if (this._allForeLoaders == null)
        //            {
        //                this._allForeLoaders = new Dictionary<IManagedProperty, ForeAsyncLoader>();
        //            }
        //        }
        //    }

        //    if (!this._allForeLoaders.TryGetValue(childrenProperty, out result))
        //    {
        //        lock (this._allForeLoaders)
        //        {
        //            if (!this._allForeLoaders.TryGetValue(childrenProperty, out result))
        //            {
        //                result = new ForeAsyncLoader(() =>
        //                {
        //                    this.LoadLazyList(childrenProperty);
        //                });
        //                this._allForeLoaders.Add(childrenProperty, result);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //[NonSerialized]
        //private Dictionary<IManagedProperty, ForeAsyncLoader> _allForeLoaders;

        #endregion

        #region 聚合关系

        /// <summary>
        /// 根据孩子类型，直接获取孩子列表。
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <returns></returns>
        public EntityList GetChildProperty<TChild>()
        {
            var properties = this.PropertiesContainer.GetNonReadOnlyCompiledProperties();
            var childProperty = properties.OfType<IListProperty>().FirstOrDefault(listProperty =>
            {
                return listProperty.ListEntityType == typeof(TChild) &&
                    listProperty.HasManyType == HasManyType.Composition;
            });
            if (childProperty == null) throw new InvalidProgramException(string.Format("不存在 {0} 类型的子属性，获取失败。", typeof(TChild).Name));

            var value = this.GetLazyList(childProperty);
            return value;
        }

        #endregion
    }
}