/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110422
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OEA.Serialization.Mobile;
using System.Runtime;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace OEA.Library
{
    /// <summary>
    /// 延迟加载的外键
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [MobileNonSerializedAttribute]
    [Serializable]
    internal class LazyEntityRef<TEntity> : CustomSerializationObject, ILazyEntityRef<TEntity>
        where TEntity : Entity
    {
        /// <summary>
        /// 默认数据加载器
        /// </summary>
        private static Func<int, TEntity> DefaultStaticLoader = GetEntityById;

        private int _id;

        private TEntity _entityField { get; set; }

        /// <summary>
        /// 外键的拥有者
        /// </summary>
        private IReferenceOwner _owner;

        private LazyEntityRefPropertyInfo _refPropertyInfo;

        private Func<int, Entity> _staticLoader;

        private Func<int, object, Entity> _instanceLoader;

        /// <summary>
        /// 这个值用于判断是否正在处于设置Entity的状态中。
        /// 
        /// 当在外界设置Entity属性时，如果获取Entity属性，不需要引起延迟加载。
        /// </summary>
        private bool _settingEntity;

        public LazyEntityRef(Func<int, Entity> staticLoader, IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
            : this(owner, refPropertyInfo)
        {
            if (staticLoader != null)
            {
                Debug.Assert(staticLoader != null, "没有注明加载器！");
                Debug.Assert(staticLoader.Target == null, "只能使用静态方法！否则不能序列化。");

                this._staticLoader = staticLoader;
            }
        }

        public LazyEntityRef(Func<int, object, Entity> instanceLoader, IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
            : this(owner, refPropertyInfo)
        {
            Debug.Assert(instanceLoader != null, "没有注明加载器！");
            Debug.Assert(instanceLoader.Target == null, "只能使用静态方法！否则不能序列化。");

            this._instanceLoader = instanceLoader;
        }

        private LazyEntityRef(IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
        {
            this._id = default(int);
            this._owner = owner;
            this._refPropertyInfo = refPropertyInfo;
        }

        #region 自定义序列化与反序列化

        /// <summary>
        /// 是否需要序列化引用的实体对象
        /// </summary>
        public bool? SerializeEntity { get; set; }

        /// <summary>
        /// 获取可用的 SerializeEntity 值。
        /// 
        /// 默认情况下，引用实体对象只从服务端序列化到客户端。
        /// </summary>
        /// <returns></returns>
        public bool GetSerializeEntityValue()
        {
            if (!this.SerializeEntity.HasValue)
            {
                //默认只从服务端序列化到客户端。
                return OEAEnvironment.IsOnServer();
            }
            return this.SerializeEntity.Value;
        }

        /// <summary>
        /// 序列化数据到 info 中。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected override void Serialize(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", this._id);
            info.AddValue("se", this.SerializeEntity);
            if (this.GetSerializeEntityValue())
            {
                info.AddValue("sev", true);
                info.AddValue("e", this._entityField, typeof(TEntity));
            }
            else
            {
                info.AddValue("sev", false);
            }
            info.AddValue("owner", this._owner, typeof(IReferenceOwner));
            info.AddValue("ri", this._refPropertyInfo, typeof(LazyEntityRefPropertyInfo));
            info.AddValue("sl", this._staticLoader, typeof(Func<int, Entity>));
            info.AddValue("il", this._instanceLoader, typeof(Func<int, object, Entity>));
        }

        /// <summary>
        /// 反序列化构造函数。
        /// 
        /// 需要更高安全性，加上以下这句：
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected LazyEntityRef(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this._id = info.GetInt32("id");
            this.SerializeEntity = info.GetValue<bool?>("se");
            if (info.GetBoolean("sev")) { this._entityField = info.GetValue<TEntity>("e"); }
            this._owner = info.GetValue<IReferenceOwner>("owner");
            this._refPropertyInfo = info.GetValue<LazyEntityRefPropertyInfo>("ri");
            this._staticLoader = info.GetValue<Func<int, Entity>>("sl");
            this._instanceLoader = info.GetValue<Func<int, object, Entity>>("il");
        }

        #endregion

        /// <summary>
        /// 被引用实体的ID
        /// </summary>
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id != value)
                {
                    var cancel = this.OnIdChanging(value);
                    if (cancel) return;

                    var oldId = this._id;
                    this._id = value;

                    var oldEntity = this._entityField;
                    if (oldEntity != null)
                    {
                        this._entityField = null;

                        //需要在 Entity 和 Id 都设置后之后，才发生外部事件，这样外部可以获得一致的 Entity 和 Id 值。
                        this.OnIdChanged(oldId, value);
                        this.OnEntityChanged(oldEntity, null);
                    }
                    else
                    {
                        this.OnIdChanged(oldId, value);
                    }
                }
            }
        }

        public int? NullableId
        {
            get
            {
                if (this.IsEmpty)
                {
                    return null;
                }
                return this.Id;
            }
            set
            {
                this.Id = value.HasValue ? value.Value : default(int);
            }
        }

        /// <summary>
        /// 如果是第一次获取此属性，并且还没有为它赋值时，会自动根据Id查询出对应的Entity。
        /// </summary>
        public TEntity Entity
        {
            get
            {
                if (!this._settingEntity && this._entityField == null)
                {
                    this.LoadEntity();
                }

                return this._entityField;
            }
            set
            {
                try
                {
                    this._settingEntity = true;

                    var oldEntity = this._entityField;

                    //如果 实体变更 或者 （设置实体为 null 并且 id 不为 null），都需要设置值改变。
                    if (oldEntity != value || (value == null && !this.IsEmpty))
                    {
                        var cancel = this.OnEntityChanging(value);
                        if (cancel) return;

                        //同步 Id
                        var oldId = this._id;
                        var newId = value == null ? default(int) : value.Id;
                        if (newId != oldId)
                        {
                            cancel = this.OnIdChanging(newId);
                            if (cancel) return;

                            this._entityField = value;
                            this._id = newId;

                            //需要在 Entity 和 Id 都设置后之后，才发生外部事件，这样外部可以获得一致的 Entity 和 Id 值。
                            //而且，应该先发生 Entity 变更事件，再发生 Id 变更事件。
                            this.OnEntityChanged(oldEntity, value);
                            this.OnIdChanged(oldId, newId);
                        }
                        else
                        {
                            this._entityField = value;

                            this.OnEntityChanged(oldEntity, value);
                        }
                    }
                }
                finally
                {
                    this._settingEntity = false;
                }
            }
        }

        public bool IsEmpty
        {
            get { return this._id.Equals(default(int)); }
        }

        public bool LoadedOrAssigned
        {
            get { return this._entityField != null; }
        }

        public void LoadId(int value)
        {
            if (this._id != value)
            {
                this._id = value;

                if (this._entityField != null) this._entityField = null;
            }
        }

        public void LoadId(int? value)
        {
            this.LoadId(value.GetValueOrDefault());
        }

        #region OnChanged methods

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private bool OnIdChanging(int newId)
        {
            return this._owner.NotifyIdChanging(this._refPropertyInfo, newId);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void OnIdChanged(int oldId, int newId)
        {
            this._owner.NotifyIdChanged(this._refPropertyInfo, oldId, newId);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private bool OnEntityChanging(Entity newEntity)
        {
            return this._owner.NotifyEntityChanging(this._refPropertyInfo, newEntity);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void OnEntityChanged(Entity oldEntity, Entity newEntity)
        {
            this._owner.NotifyEntityChanged(this._refPropertyInfo, oldEntity, newEntity);
        }

        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        //private void OnChangedCompleted(int oldId, int newId, Entity oldEntity, Entity newEntity)
        //{
        //    this._owner.NotifyLazyChanged(this._refPropertyInfo, oldId, newId, oldEntity, newEntity);
        //}

        #endregion

        private void LoadEntity()
        {
            if (this.IsEmpty) { return; }

            if (this._staticLoader != null)
            {
                this._entityField = this._staticLoader(this._id) as TEntity;
            }
            else if (this._instanceLoader != null)
            {
                this._entityField = this._instanceLoader(this._id, this._owner) as TEntity;
            }
            else
            {
                this._entityField = GetEntityById(this._id);
            }
        }

        private void Clone(LazyEntityRef<TEntity> target, bool cloneEntity)
        {
            if (this._id != target._id)
            {
                this._id = target._id;

                if (cloneEntity) this._entityField = target._entityField;
                else this._entityField = null;

                this._staticLoader = target._staticLoader;

                //this._owner = target._owner;
            }
        }

        #region ILazyEntityRef Members

        void ILazyEntityRef.Clone(ILazyEntityRef target, bool cloneEntity)
        {
            var targetRef = target as LazyEntityRef<TEntity>;
            if (targetRef != null)
            {
                this.Clone(targetRef, cloneEntity);
            }
        }

        void ILazyEntityRef.Clone(ILazyEntityRef target)
        {
            var targetRef = target as LazyEntityRef<TEntity>;
            if (targetRef != null)
            {
                this.Clone(targetRef, true);
            }
        }

        #endregion

        //#region Mobile Serialization

        //protected override void OnMobileSerializeRef(ISerializationContext context)
        //{
        //    base.OnMobileSerializeRef(context);

        //    context.AddRef("owner", this._owner);
        //    context.AddRef("refPI", this._refPropertyInfo);
        //}

        //protected override void OnMobileSerializeState(ISerializationContext context)
        //{
        //    base.OnMobileSerializeState(context);

        //    context.AddState("id", this._id);

        //    if (this._staticLoader != null) { context.AddDelegate("sl", this._staticLoader); }
        //    if (this._instanceLoader != null) { context.AddDelegate("il", this._instanceLoader); }
        //}

        //protected override void OnMobileDeserializeState(ISerializationContext context)
        //{
        //    base.OnMobileDeserializeState(context);

        //    this._id = context.GetState<int>("id");

        //    this._staticLoader = context.GetDelegate<Func<int, TEntity>>("sl");
        //    this._instanceLoader = context.GetDelegate<Func<int, object, TEntity>>("il");
        //}

        //protected override void OnMobileDeserializeRef(ISerializationContext context)
        //{
        //    this._owner = context.GetRef<IReferenceOwner>("owner");
        //    this._refPropertyInfo = context.GetRef<LazyEntityRefPropertyInfo>("refPI");

        //    base.OnMobileDeserializeRef(context);
        //}

        //#endregion

        //public void Load(IList list)
        //{
        //    if (this.IsEmpty == false)
        //    {
        //        //尝试从缓存中读取。
        //        var fkEntity = list.OfType<TEntity>().FirstOrDefault(e => e.Id == this._id);
        //        if (fkEntity != null)
        //        {
        //            this.Entity = fkEntity;
        //        }
        //        else
        //        {
        //            //缓存中没有，则从数据层加载
        //            fkEntity = this.Entity;

        //            //加载完毕，加入链表。
        //            if (fkEntity != null &&
        //                list.IsReadOnly == false && list.IsFixedSize == false
        //                )
        //            {
        //                list.Add(fkEntity);
        //            }
        //        }
        //    }
        //}

        public override string ToString()
        {
            if (this.IsEmpty)
            {
                return "Empty";
            }
            if (this.LoadedOrAssigned)
            {
                return this._entityField.ToString();
            }
            return "NotLoaded:" + this._id;
        }

        Entity ILazyEntityRef.Entity
        {
            get
            {
                return this.Entity;
            }
            set
            {
                var typedValue = value as TEntity;
                if (value != null && typedValue == null) throw new ArgumentException("请选择准确的类型赋值！");
                this.Entity = typedValue;
            }
        }

        #region 此函数作为本组件唯一与 实体代码 耦合的代码

        private static TEntity GetEntityById(int id)
        {
            var r = RepositoryFactoryHost.Factory.Create(typeof(TEntity));
            return r.GetById(id) as TEntity;
        }

        #endregion
    }

    //这些代码可能在将来会有用，不要删除。

    //这块代码是实体类中的。
    //protected ILazyEntityRef<TRefEntity> ConvertReference<TRefEntity>(ILazyEntityRef lazyRef)
    //    where TRefEntity : Entity
    //{
    //    return new TypedLazyEntityRef<TRefEntity>(lazyRef);
    //}

    ///// <summary>
    ///// 强类型的ILazyEntityRef(TEntity)
    ///// 
    ///// 能够把一个通用的ILazyEntityRef转换成强类型的ILazyEntityRef(TEntity)。
    ///// </summary>
    ///// <typeparam name="TEntity"></typeparam>
    //[Serializable]
    //internal class TypedLazyEntityRef<TEntity> : ILazyEntityRef<TEntity>
    //    where TEntity : Entity
    //{
    //    private ILazyEntityRef _innerRef;

    //    public TypedLazyEntityRef(ILazyEntityRef innerRef)
    //    {
    //        if (innerRef == null) throw new ArgumentNullException("innerRef");

    //        this._innerRef = innerRef;
    //    }

    //    #region 一般实现

    //    public int Id
    //    {
    //        get
    //        {
    //            return this._innerRef.Id;
    //        }
    //        set
    //        {
    //            this._innerRef.Id = value;
    //        }
    //    }

    //    public int? NullableId
    //    {
    //        get
    //        {
    //            return this._innerRef.NullableId;
    //        }
    //        set
    //        {
    //            this._innerRef.NullableId = value;
    //        }
    //    }

    //    public bool LoadedOrAssigned
    //    {
    //        get
    //        {
    //            return this._innerRef.LoadedOrAssigned;
    //        }
    //    }

    //    public bool IsEmpty
    //    {
    //        get
    //        {
    //            return this._innerRef.IsEmpty;
    //        }
    //    }

    //    //public void Load(IList list)
    //    //{
    //    //    this._innerRef.Load(list);
    //    //}

    //    Entity ILazyEntityRef.Entity
    //    {
    //        get
    //        {
    //            return this._innerRef.Entity;
    //        }
    //        set
    //        {
    //            this._innerRef.Entity = value;
    //        }
    //    }

    //    #endregion

    //    /// <summary>
    //    /// 传入的target需要也是本类的实例。
    //    /// </summary>
    //    /// <param name="target"></param>
    //    public void Clone(ILazyEntityRef target)
    //    {
    //        var e = target as TypedLazyEntityRef<TEntity>;
    //        if (e == null) throw new ArgumentNullException("传入的target需要也是本类的实例！");

    //        this._innerRef.Clone(e._innerRef);
    //    }

    //    /// <summary>
    //    /// 强类型的实体引用
    //    /// </summary>
    //    public TEntity Entity
    //    {
    //        get
    //        {
    //            return this._innerRef.Entity as TEntity;
    //        }
    //        set
    //        {
    //            this._innerRef.Entity = value;
    //        }
    //    }
    //}
}
