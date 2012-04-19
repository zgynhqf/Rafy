/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100330
 * 说明：对象实体，所有对象实体的基类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.5
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100330
 * 添加延迟外键 胡庆访 20100401
 * 自动获取属性，用于支持属性的复制 胡庆访 20100402
 * 改为非泛型的类 胡庆访 20100915
 * 添加FstField类 胡庆访 20101026
 * AutoCalcEntity 及 CalcEngine 中的代码移动到 GIX4.Library 中。Entity 则直接继承自 OEAEntity。胡庆访 20110314
 * 添加路由事件。胡庆访 20111108
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Transactions;
using OEA.Library.Caching;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.ORM;
using OEA.Threading;


using OEA.Library.Validation;
using System.Diagnostics;

namespace OEA.Library
{
    /// <summary>
    /// 对象实体
    /// 
    /// 继承此类，可以获得以下功能：
    /// * 自动管理托管属性，可以通过属性 FindRepository().GetAvailableIndicators() 获取到所有的托管属性。
    /// * 默认实现了所有数据库访问的方法：CDUQ。
    /// * 提供了延迟加载子对象集合的方法。
    /// * 提供了创建延迟外键对象的方法。
    /// * 提供实体验证规则框架。
    /// * 提供树型实体支持。
    /// * 支持聚合对象的路由事件。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract partial class Entity : ManagedPropertyObject, IEntity
    {
        #region 构造函数及工厂方法

        protected Entity()
        {
            //ValidationRules.Validate();

            //不需要此行，所有新增的实体的 Id 都是 -1.
            //this.LoadProperty(IdProperty, OEAEnvironment.NewLocalId());

            //this.MarkNew();
        }

        /// <summary>
        /// 通过实体类型反射构造一个新的实体。
        /// 
        /// 此方法功能与构造函数一致，主要用于不能显式调用的场景下。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static Entity New(Type entityType)
        {
            //经测试，Activator 创建对象也非常快，这是因为它的内部作了缓存处理。
            return Activator.CreateInstance(entityType, true) as Entity;
        }

        #endregion

        #region 所属仓库

        [NonSerialized]
        private IRepository _repository;

        public IRepository FindRepository()
        {
            if (this._repository == null)
            {
                this._repository = RepositoryFactoryHost.Factory.Create(this.GetType());
            }

            return this._repository;
        }

        #endregion

        #region 属性

        [NonSerialized]
        private FastField<int> _idFast;
        /// <summary>
        /// 主键
        /// 重写时需要注意：如果使用 LiteORM 的话需要标记 PK，否则更新时会根据主键更新，找不到主键列会报错
        /// </summary>
        public static Property<int> IdProperty = P<Entity>.Register(e => e.Id, new PropertyMetadata<int>
        {
            CoerceGetValueCallBack = (o, v) => (o as Entity).CoerceGetId(v),
            PropertyChangingCallBack = (o, e) => (o as Entity).OnIdChanging(e),
            PropertyChangedCallBack = (o, e) => (o as Entity).OnIdChanged(e),
            DefaultValue = -1
        });
        [EntityProperty, Column, PK]
        public int Id
        {
            get { return this.GetProperty(IdProperty, ref this._idFast); }
            set { this.SetProperty(IdProperty, ref this._idFast, value); }
        }
        protected virtual int CoerceGetId(int value) { return value; }
        protected virtual void OnIdChanging(ManagedPropertyChangingEventArgs<int> e) { }
        protected virtual void OnIdChanged(ManagedPropertyChangedEventArgs<int> e) { }

        private void OnPropertyLoaded(IManagedProperty property)
        {
            if (property == IdProperty)
            {
                this.ResetFastField(this._idFast);
                this.OnTreeIdLoaded();
            }
        }

        #endregion

        #region 值的复制

        /// <summary>
        /// 把旧对象的所有属性都读取到本对象中。
        /// 读取完成后，旧对象不再可用。（孩子集合属性中的所有孩子的父指针已经被更改。）
        /// </summary>
        /// <param name="oldObject"></param>
        /// <returns></returns>
        public virtual Entity MergeOldObject(Entity oldEntity)
        {
            //从数据库对象中读取所有的属性，并设置好孩子结点的父指针。
            this.CloneCore(oldEntity.CastTo<Entity>(), CloneOptions.MergeOldEntity());

            this.Status = PersistenceStatus.Unchanged;

            return this;
        }

        /// <summary>
        /// 使用 CloneActions.ReadSingleEntity 来复制目标对象。
        /// </summary>
        /// <param name="target"></param>
        public void Clone(Entity target)
        {
            this.CloneCore(target, CloneOptions.ReadSingleEntity());
        }

        public void Clone(Entity target, CloneOptions options)
        {
            this.CloneCore(target, options);
        }

        /// <summary>
        /// 复制目标对象的所有字段。
        /// 
        /// 子类重写此方法额外做以下几件事：
        /// 1. 如果有自定义字段，请在此方法中进行值拷贝。
        /// 2. 如果有延迟加载的外键引用对象 ILazyEntityRef，请调用它的 Clone 方法进行拷贝。
        /// 3. 如果使用了快速字段 FastField 来进行属性的缓存，请在基类完成 Clone 后，调用本类的 ResetFastField 方法来清空缓存。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cloneChildren">是否复制孩子对象的引用</param>
        protected virtual void CloneCore(Entity target, CloneOptions options)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (options == null) throw new ArgumentNullException("options");

            var grabChildren = options.HasAction(CloneActions.GrabChildren);
            var childrenRecur = options.HasAction(CloneActions.ChildrenRecur);
            var copyId = options.HasAction(CloneActions.IdProperty);

            var ingoreList = options.RetrieveIgnoreListOnce();

            //如果需要拷贝id，则应该先拷贝id，并立刻清空id的缓存。
            //注意：
            //由于 IdProperty 在 AllProperties 中的位置并不是第一个。所以会出现拷贝其它属性时，再次访问本ID导致缓存重建。
            //所以这里需要单独对 Id 进行一次拷贝。
            if (copyId) { this.CopyProperty(target, IdProperty); }

            //复制目标对象的所有托管属性。
            var allProperties = this.FindRepository().GetAvailableIndicators();
            for (int i = 0, c = allProperties.Count; i < c; i++)
            {
                var property = allProperties[i];
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
                        var targetList = target.GetLazyList(listProperty);
                        var srcList = this.GetLazyList(listProperty);
                        srcList.Clone(targetList, options);

                        var isComposition = srcList.HasManyType == HasManyType.Composition;
                        if (isComposition) { srcList.SetParentEntity(this); }
                    }
                    else
                    {
                        if (grabChildren)
                        {
                            var children = target.GetProperty(property) as EntityList;
                            this.LoadProperty(property, children);
                            if (children == null) return;

                            var isComposition = children.HasManyType == HasManyType.Composition;
                            if (isComposition) { children.SetParentEntity(this); }
                        }
                    }
                }
                else
                {
                    this.CopyProperty(target, property, options);
                }
            }

            options.NotifyCloned(target, this);

            this.OnCloneCore(target, options);
        }

        #endregion

        #region 根对象

        /// <summary>
        /// 保存根对象
        /// </summary>
        internal void SaveRoot()
        {
            if (EntityListVersion.Repository != null)
            {
                using (EntityListVersion.Repository.BeginBillSave())
                {
                    using (var tran = new TransactionScope())
                    {
                        this.DoSaveRoot();

                        tran.Complete();
                    }

                    EntityListVersion.Repository.EndBillSave();
                }
            }
            else
            {
                using (var tran = new TransactionScope())
                {
                    this.DoSaveRoot();

                    tran.Complete();
                }
            }
        }

        private void DoSaveRoot()
        {
            if (this.IsDeleted)
            {
                if (!this.IsNew)
                {
                    this.OnDelete();
                    this.MarkNew();
                }
            }
            else
            {
                if (this.IsNew)
                {
                    this.OnInsert();
                }
                else
                {
                    this.OnUpdate();
                }
                this.MarkOld();
            }
        }

        #endregion

        #region 子对象

        /// <summary>
        /// 保存子对象
        /// </summary>
        /// <param name="parent"></param>
        internal void SaveChild(Entity parent)
        {
            var entity = this;

            // if the object isn't dirty, then just exit
            if (!entity.IsDirty) { return; }

            if (entity.IsDeleted)
            {
                if (!entity.IsNew)
                {
                    // tell the object to delete itself
                    entity.OnDelete();
                    entity.MarkNew();
                }
            }
            else
            {
                if (entity.IsNew)
                {
                    // tell the object to insert itself
                    entity.OnInsert();
                }
                else
                {
                    // tell the object to update itself
                    entity.OnUpdate();
                }
                entity.MarkOld();
            }
        }

        #endregion

        #region 缓存

        /// <summary>
        /// 在本实体更新时，通知服务器更新对象的版本号。
        /// </summary>
        protected void NotifyCacheVersion()
        {
            if (EntityListVersion.Repository != null)
            {
                var entityType = this.GetType();
                if (CacheDefinition.Instance.IsEnabled(entityType))
                {
                    EntityListVersion.Repository.UpdateVersion(entityType);
                }
            }
        }

        #endregion

        #region 预加载

        /// <summary>
        /// 构建并获取某个子属性的预加载器
        /// </summary>
        /// <param name="childrenProperty"></param>
        /// <returns></returns>
        protected ForeAsyncLoader ChildrenLoader(IListProperty childrenProperty)
        {
            ForeAsyncLoader result = null;

            if (this._allForeLoaders == null)
            {
                lock (this)
                {
                    if (this._allForeLoaders == null)
                    {
                        this._allForeLoaders = new Dictionary<IManagedProperty, ForeAsyncLoader>();
                    }
                }
            }

            if (!this._allForeLoaders.TryGetValue(childrenProperty, out result))
            {
                lock (this._allForeLoaders)
                {
                    if (!this._allForeLoaders.TryGetValue(childrenProperty, out result))
                    {
                        result = new ForeAsyncLoader(() =>
                        {
                            this.LoadLazyList(childrenProperty);
                        });
                        this._allForeLoaders.Add(childrenProperty, result);
                    }
                }
            }

            return result;
        }

        [NonSerialized]
        private Dictionary<IManagedProperty, ForeAsyncLoader> _allForeLoaders;

        #endregion

        #region 聚合关系

        /// <summary>
        /// 根据孩子类型，直接获取孩子列表。
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <returns></returns>
        public IList GetChildProperty<TChild>()
        {
            var entityInfo = this.FindRepository().EntityMeta;
            var childProperty = entityInfo.ChildrenProperties
                .FirstOrDefault(b => b.ChildType.EntityType == typeof(TChild));
            return this.GetPropertyValue<IList>(childProperty.Name);
        }

        #endregion

        #region 快速字段

        /// <summary>
        /// 使用快速字段完成数据的读取。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="fastfield"></param>
        /// <returns></returns>
        protected T GetProperty<T>(Property<T> property, ref FastField<T> fastfield)
        {
            if (fastfield == null) fastfield = new FastField<T>();

            if (fastfield.IsEmpty)
            {
                fastfield.Value = this.GetProperty(property);
                fastfield.IsEmpty = false;
            }

            return fastfield.Value;
        }

        /// <summary>
        /// 使用快速字段进行属性值的设置。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="fastfield"></param>
        /// <param name="value"></param>
        protected void SetProperty<T>(Property<T> property, ref FastField<T> fastfield, T value)
        {
            if (fastfield != null)
            {
                fastfield.Value = value;
            }

            this.SetProperty(property, value);
        }

        protected void ResetFastField<T>(FastField<T> fastfield)
        {
            if (fastfield != null) fastfield.IsEmpty = true;
        }

        /// <summary>
        /// 属性使用的快速字段。
        /// 
        /// 设计此类的原因是CSLA属性的ReadProperty方法往往比较耗时，
        /// 而且目前并不使用CSLA的属性权限等内容，
        /// 所以可以使用这个类对一些被频繁调用的类进行缓存。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Serializable]
        public class FastField<T>
        {
            internal FastField()
            {
                this.IsEmpty = true;
                this.Value = default(T);
            }

            /// <summary>
            /// 字段的值。 
            /// 框架内部使用。
            /// </summary>
            internal T Value;

            /// <summary>
            /// Bool值表示当前的值是否还没有和属性值进行同步。
            /// 框架内部使用。
            /// </summary>
            internal bool IsEmpty;
        }

        #endregion
    }
}